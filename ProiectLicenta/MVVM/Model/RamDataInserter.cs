using ProiectLicenta.MVVM.ViewModel;
using System;
using System.Data.SQLite;

namespace ProiectLicenta.Model
{
    internal class RamDataInserter
    {
        public static void InsertRamUsageInDb(string table)
        {
            // Load the config
            ConfigLoader cfgLoader = new ConfigLoader();
            var config = cfgLoader.LoadConfig();
            // Connect to the database
            string connectionString = $"Data Source=monitoring.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                // Insert RAM data into the table
                string formattedDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                RAMDataViewModel ramObj = new RAMDataViewModel();
                float ramUsage = ramObj.GetRamUsage(config.RemoteIp, config.CommunityString, int.Parse(config.Port));
                if (ramUsage == -1)
                {
                    connection.Close();
                    return;
                }
                string insertDataQuery = $"INSERT INTO {table} (Timestamp, Usage) VALUES ('{formattedDateTime}', {ramUsage});";
                using (SQLiteCommand command = new SQLiteCommand(insertDataQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
    }
}
