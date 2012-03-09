using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RequestReduce.Facts.Store
{
    public class SqliteHelper
    {
        static string sql = GetSqlLightSafeSql(File.ReadAllText((AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory) + "..\\..\\..\\..\\RequestReduce.SqlServer\\Nuget\\Tools\\RequestReduceFiles.Sql"));

        public  static string GetSqlLightSafeSql()
        {
            return GetSqlLightSafeSql(sql);
        }

        private static string GetSqlLightSafeSql(string sql)
        {
            var result = sql.Replace("[dbo].", string.Empty);
            result = result.Replace("(max)", "(1000)");
            result = result.Replace("CLUSTERED", string.Empty);
            result = result.Replace("GO", string.Empty);
            return result;
        }
    }
}
