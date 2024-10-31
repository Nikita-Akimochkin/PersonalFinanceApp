using Npgsql;

namespace PersonalFinanceApp
{
    internal class DataBaseHelper
    {

        private string GetConnectionString()
        {
            return "Host=localhost;Username=postgres;Password=9097988588;Database=PersonalFinanceDB";
        }

        public NpgsqlConnection GetConnection()
        {
            var connection = new NpgsqlConnection(GetConnectionString());
            connection.Open();
            return connection;
        }

        public void CloseConnection(NpgsqlConnection connection)
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }
    }
}
