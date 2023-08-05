using Npgsql;

namespace DeveloperBlog.Services
{
    public class ConnectionService
    {
        #region Get Connection String
        public static string GetConnectionString(IConfiguration configuration)
        {
            //The default connection string will come from appSettings/secrets like usual
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            //Updated for Railway
            //It will be automatically overwritten if we are running on railway
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
        }
        #endregion

        #region Build Connection String
        private static string BuildConnectionString(string databaseUrl)
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = Environment.GetEnvironmentVariable("PGHOST"),
                Port = Int32.Parse(Environment.GetEnvironmentVariable("PGPORT")),
                Username = Environment.GetEnvironmentVariable("PGUSER"),
                Password = Environment.GetEnvironmentVariable("PGPASSWORD"),
                Database = Environment.GetEnvironmentVariable("PGDATABASE"),

                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };

            return builder.ToString();
        }
        #endregion

    }
}
