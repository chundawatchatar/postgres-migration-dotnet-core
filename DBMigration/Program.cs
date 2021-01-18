using System;
using System.Collections.Generic;
using System.IO;

namespace DBMigration
{
    class Program
    {
        static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            DBConfig dbConfig;

            try
            {
                dbConfig = MigrationTasks.GetConnectionString();
                DBConnect.ConnectionString = $"Host={dbConfig.HostName};Port={dbConfig.Port};Username={dbConfig.UserName};Password={dbConfig.Password};Database={dbConfig.DataBase}";
                await MigrationTasks.ConnectAsync(dbConfig);
                await MigrationTasks.CreateSchemaIfNotExist();
                List<string> scriptList = MigrationTasks.GetFilles();
                await MigrationTasks.ExecuteScriptsAsync(scriptList);

                Console.WriteLine("\nScripts are executed successfully.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return 1;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return 1;
            }
        }

    }
}
