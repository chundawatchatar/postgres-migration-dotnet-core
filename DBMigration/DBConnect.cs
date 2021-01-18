using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DBMigration
{
    public static class DBConnect
    {
        public static string ConnectionString;
        public static NpgsqlConnection conn;
        public static async Task<bool> Connect()
        {
            conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();
            return true;
        }

        public static async Task<bool> IsSchemaExist()
        {
            var retVal = true;
            await using (var cmd = new NpgsqlCommand("SELECT EXISTS( SELECT * FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'db_migration');", conn))
            {
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        retVal = reader.GetBoolean(0);
                    }
                }

            }

            return retVal;
        }

        public static async Task<bool> IsExist(string file_name)
        {
            var retVal = true;
            await using (var cmd = new NpgsqlCommand("SELECT COUNT(*)::text FROM db_migration where file_name = @f", conn))
            {
                cmd.Parameters.AddWithValue("f", file_name);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        retVal = Int32.Parse(reader.GetString(0)) == 0 ? false : true;
                }

            }

            return retVal;
        }

        public static async Task<bool> ExecuteScript(string script)
        {           
            await using (var cmd = new NpgsqlCommand(script, conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }
            return true;
        }
    }
}
