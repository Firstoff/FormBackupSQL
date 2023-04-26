using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/* Microsoft.SqlServer.Management.Smo.Server server = new ServerConnection("enter server name");
 Microsoft.SqlServer.Management.Smo.Database db = server.Databases("enter db name");
 Console.WriteLine(db.FileGroups[0].Files[0].FileName); 'the mdf file
 Console.WriteLine(db.LogFiles[0].FileName); 'the log file
*/

namespace FormBackupSQL
{
    internal class CheckExistBase // данный код выдает единицу в Int32 если находит базу на скл сервере.
    {
        private bool CheckDatabase(string databaseName)
        {
            // You know it's a string, use var
            var connString = "Server=zxhome\\SQLEXPRESS;Integrated Security=SSPI;database=northwind";
            // Note: It's better to take the connection string from the config file.

            var cmdText = "select count(*) from master.dbo.sysdatabases where name=@database";

            using (var sqlConnection = new SqlConnection(connString))
            {
                using (var sqlCmd = new SqlCommand(cmdText, sqlConnection))
                {
                    // Use parameters to protect against Sql Injection
                    sqlCmd.Parameters.Add("@database", System.Data.SqlDbType.NVarChar).Value = databaseName;

                    // Open the connection as late as possible
                    sqlConnection.Open();
                    // count(*) will always return an int, so it's safe to use Convert.ToInt32
                    return Convert.ToInt32(sqlCmd.ExecuteScalar()) == 1;
                }
            }

        }

    }
}
