using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DBMigration
{
    public static class MigrationTasks
    {
        public static DBConfig GetConnectionString()
        {
            try
            {
                var connectioText = File.ReadAllText("DBConfig.json");
                DBConfig dbConfig = System.Text.Json.JsonSerializer.Deserialize<DBConfig>(connectioText);
                return dbConfig;
            }
            catch (Exception)
            {
                Console.WriteLine("DBConfig.json not found");
                throw;
            }
        }

        public static async System.Threading.Tasks.Task ConnectAsync(DBConfig dbConfig)
        {
            try
            {
                Console.WriteLine($"Host: {dbConfig.HostName}");
                Console.WriteLine($"Port: {dbConfig.Port}");
                Console.WriteLine($"Username: {dbConfig.UserName}");
                Console.WriteLine($"DataBase: {dbConfig.DataBase}\n");
                Console.WriteLine("Connecting to the database...");
                await DBConnect.Connect();
                Console.WriteLine("Connected successfully.\n");
            }
            catch (Exception)
            {
                Console.WriteLine("Connection Failed");
                throw;
            }
        }

        public static async System.Threading.Tasks.Task CreateSchemaIfNotExist()
        {
            try
            {
                //await 
                Console.WriteLine("Checking migration schema....");
                var isScemaExists = await DBConnect.IsSchemaExist();
                if (!isScemaExists)
                {
                    Console.WriteLine("Migration schema is not exist....");
                    Console.WriteLine("Creating migration schema....");
                    var scrpt = "CREATE TABLE db_migration(file_name character varying(500) primary key, created_on timestamp without time zone);";
                    await DBConnect.ExecuteScript(scrpt);
                    Console.WriteLine("Migration schema created successfully....");
                }
                else
                {
                    Console.WriteLine("Migration schema already exists. Skipping...");
                }
                Console.Write("\n");
            }
            catch (Exception)
            {
                Console.WriteLine("Connection Failed");
                throw;
            }
        }

        public static List<string> GetFilles()
        {
            try
            {
                List<string> scriptList = new List<string>();
                var files = Directory.GetFiles("scripts");
                foreach (var s in files)
                {
                    scriptList.Add(s);
                }
                scriptList.Sort();
                return scriptList;
            }
            catch (Exception)
            {
                Console.WriteLine("Error in reading script files.");
                throw;
            }
        }

        public static async System.Threading.Tasks.Task ExecuteScriptsAsync(List<string> scriptList)
        {
            foreach (var f in scriptList)
            {
                try
                {
                    Console.WriteLine($"Executing file  --> {Path.GetFileNameWithoutExtension(f)}");
                    var isExist = await DBConnect.IsExist(Path.GetFileNameWithoutExtension(f));
                    if (isExist)
                    {
                        Console.WriteLine("\t\t   script already executed. Skipping...");
                    }
                    else
                    {
                        var scrpt = File.ReadAllText(f);
                        if (scrpt.IndexOf(Path.GetFileNameWithoutExtension(f)) == -1)
                        {
                            var migrationScript = $"INSERT INTO db_migration(file_name, created_on) VALUES('{Path.GetFileNameWithoutExtension(f)}', current_timestamp);";
                            scrpt = migrationScript + scrpt;
                        }
                        await DBConnect.ExecuteScript(scrpt);
                        Console.WriteLine("\t\t   File executed successfully.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\tError in executing scripts");
                    Console.WriteLine("\t" + ex.Message);
                    Console.Write("\tTo continue press Y - ");
                    var choice = Console.ReadLine();
                    if (choice.ToLower() == "y")
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

    }
}
