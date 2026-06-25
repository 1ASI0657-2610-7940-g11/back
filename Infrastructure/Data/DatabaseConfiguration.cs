using MySql.Data.MySqlClient;

namespace FuelTrack.Api.Infrastructure.Data;

public static class DatabaseConfiguration
{
    public static string BuildConnectionString(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var host = configuration["MYSQLHOST"];
        var port = configuration["MYSQLPORT"];
        var user = configuration["MYSQLUSER"];
        var password = configuration["MYSQLPASSWORD"];
        var database = configuration["MYSQLDATABASE"];

        if (string.IsNullOrWhiteSpace(host)
            || string.IsNullOrWhiteSpace(user)
            || string.IsNullOrWhiteSpace(database))
        {
            if (!environment.IsDevelopment())
                throw new InvalidOperationException(
                    "MYSQLHOST, MYSQLUSER and MYSQLDATABASE are required in production.");

            host = "localhost";
            port = "3306";
            user = "root";
            password ??= "";
            database = "fueltrack";
        }

        var builder = new MySqlConnectionStringBuilder
        {
            Server = host,
            Port = uint.TryParse(port, out var parsedPort) ? parsedPort : 3306,
            UserID = user,
            Password = password,
            Database = database,
            CharacterSet = "utf8mb4",
            SslMode = MySqlSslMode.Preferred
        };
        return builder.ConnectionString;
    }
}
