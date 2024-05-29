using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace cs2_hours_spent;
public class Database(string dbConnectionString, ILogger logger) {
    public MySqlConnection GetConnection() {
        try {
            var connection = new MySqlConnection(dbConnectionString);
            connection.Open();
            return connection;
        }
        catch (Exception ex){
            logger.LogCritical(ex.ToString());
            throw;
        }
    }

    public async Task<MySqlConnection> GetConnectionAsync() {
        try {
            var connection = new MySqlConnection(dbConnectionString);
            await connection.OpenAsync();
            return connection;
        }
        catch (Exception ex) {
            logger.LogCritical(ex.ToString());
            throw;
        }
    }

    public bool CheckDatabaseConnection() {
        using var connection = GetConnection();
        try {
            return connection.Ping();
        }
        catch {
            return false;
        }
    }

    public void InitDb(string Table, string Prefix){
        var cmd = GetConnection().CreateCommand();
        cmd.CommandText = $"CREATE TABLE IF NOT EXISTS {Prefix}{Table} (steam_id bigint primary key not null, play_time bigint not null default 0, player_name varchar(255) not null)";
        cmd.ExecuteNonQuery();       
    }

    public void AddUserToDb(string table, ulong steamId, string name){
        var cmd = GetConnection().CreateCommand();
        cmd.CommandText = $"INSERT IGNORE INTO {table} VALUE ({steamId}, 0, \"{name}\")";
        cmd.ExecuteNonQuery();
    }

    public bool AddTimeToPlayerSId(string table, ulong steamId, int time, string name) {
        AddUserToDb(table, steamId, name);
        var cmd = GetConnection().CreateCommand();
        cmd.CommandText = $"SELECT t.play_time FROM {table} t WHERE t.steam_id = {steamId}";
        var reader = cmd.ExecuteReader();
        while (reader.Read()){
            var palyed = reader.GetInt32(0);
            time += palyed;
        }
        reader.Close();
        cmd.CommandText = $"UPDATE {table} t SET t.play_time = {time}, t.player_name = \"{name}\" WHERE t.steam_id = {steamId}";
        cmd.ExecuteNonQuery();
        return true;
    }
}