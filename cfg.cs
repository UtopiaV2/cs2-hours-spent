using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace cs2_hours_spent;

public class Cfg : BasePluginConfig {
    [JsonPropertyName("CommandNames")] public string[] Cmds {get; set;} = ["playtime"];
    [JsonPropertyName("CommandDesc")] public string CmdDesc {get; set;} = "Gets info about player spent time on server";
    [JsonPropertyName("DefaultLimit")] public int Limit {get; set;} = 5;
    [JsonPropertyName("SpentMsg")] public string SpentMsg {get; set;} = " has spent on the server!";
    [JsonPropertyName("Database")] public DB DbC {get; set;} = new DB{};
}

public class DB {
    public string Host {get; set;} = "localhost";
    public string User {get; set; } = "db_user";
    public string Password {get; set;} = "password";
    public string Database {get; set;} = "database";
    public string Table {get; set;} = "life_less_idiots";
    public string Prefix {get; set;} = "os_";
}