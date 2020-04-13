using System;
using System.Data;

namespace DapperExtensions.Oracle
{
    internal class BuilderFactory
    {
        private static readonly ISqlBuilder Oracle = new OracleBuilder();

        public static ISqlBuilder GetBuilder(IDbConnection conn)
        {
            string dbType = conn.GetType().Name;
            if (dbType.Equals("OracleConnection"))
            {
                return Oracle;
            }
            else
            {
                throw new Exception("Unknown DbType:" + dbType);
            }
        }
    }
}
