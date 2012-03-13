namespace RequestReduce.SqlServer.ORM
{
    public class RequestReduceDB : Database
    {
        public RequestReduceDB(string connectionStringName)
            : base(connectionStringName)
        {
        }

        public RequestReduceDB(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        public static string DefaultProviderName = "System.Data.SqlClient";

    }
}
