using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Test
{
    public class DbHelper
    {
        public static IDbConnection GetConn()
        {
            return new OracleConnection("User ID=test;Password=123456;Data Source=(DESCRIPTION =(ADDRESS_LIST =(ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521)))(CONNECT_DATA =(SERVICE_NAME = XE)))");
        }
    }
}
