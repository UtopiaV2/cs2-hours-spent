using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;

namespace cs2_hours_spent;
public class HoursSpent : BasePlugin, IPluginConfig<Cfg>{
    public override string ModuleName => "Time spent tracker";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "OwnSample";
    public override string ModuleDescription => "Tracks each players spent time on the server";
    Database? db;
    Dictionary<ulong, int> CurrentTime = [];
    public Cfg Config {get; set;} = new Cfg{};
    public void OnConfigParsed(Cfg config){
        Config = config;
    }
    public override void Load(bool hotReload) {
        Logger.LogInformation("Loading!");
        db = new Database($"Server={Config.DbC.Host};User ID={Config.DbC.User};Password={Config.DbC.Password};Database={Config.DbC.Database}", Logger);
        db.InitDb(Config.DbC.Table, Config.DbC.Prefix);
        RegisterListener<Listeners.OnClientConnected>(OnPlayerConnect);
        RegisterListener<Listeners.OnClientDisconnect>(OnPlayerDisconnect);
        foreach (var cmd in Config.Cmds)
            AddCommand($"css_{cmd}", Config.CmdDesc, Status);
    }
    private void OnPlayerConnect(int playerSlot) {
        var player = Utilities.GetPlayerFromSlot(playerSlot) ?? throw new Exception($"Player is null, {playerSlot}");
        if (player.SteamID == 0) return;
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        var logged_in = (int)t.TotalSeconds;
        CurrentTime.Add(player.SteamID, logged_in);        
    }

    private void OnPlayerDisconnect(int playerSlot) {
        var player = Utilities.GetPlayerFromSlot(playerSlot) ?? throw new Exception($"Player is null, {playerSlot}");
        db = db ?? throw new Exception("Db is null");
        if (player.SteamID == 0) return;
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        var logged_out = (int)t.TotalSeconds;
        db.AddTimeToPlayerSId(Config.DbC.Prefix+Config.DbC.Table, player.SteamID, logged_out - CurrentTime[player.SteamID], player.PlayerName);
        CurrentTime.Remove(player.SteamID);
    }

    public void Status(CCSPlayerController? player, CommandInfo info) {
        db = db ?? throw new Exception("Db is null");
        var cmd = db.GetConnection().CreateCommand();
        cmd.CommandText = $"SELECT t.player_name, t.play_time FROM {Config.DbC.Prefix}{Config.DbC.Table} t ";
        if (info.ArgCount == 1) {
            cmd.CommandText += "ORDER BY t.play_time DESC LIMIT 5";
        }
        else {
            var searchee = info.ArgByIndex(1);
            cmd.CommandText += $"WHERE t.{(searchee.StartsWith('#') ? "steam_id" : "player_name")} = {(searchee.StartsWith('#') ? searchee.Replace("#", "") : searchee)}";
        }
        var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            var player_n = reader.GetString(0);
            var time = reader.GetInt64(1);
             var timeSpan = TimeSpan.FromSeconds(time);
                var msg = string.Format("{0}s", timeSpan.TotalSeconds);
            if (player == null){
                Server.PrintToConsole($"{player_n}: " + msg + Config.SpentMsg);
            } else {
                player.PrintToChat($"{player_n}: " + msg + Config.SpentMsg);
            }
        }
    }
}