using System.Configuration;
using MySqlConnector;

public static class Db
{
    private static readonly string connStr =
        ConfigurationManager.ConnectionStrings["MovieReserveDb"].ConnectionString;

    public static   MySqlConnection GetConnection()
    {
        return new MySqlConnection(connStr);
    }
}