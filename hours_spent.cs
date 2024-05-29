using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
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
    }
   
}