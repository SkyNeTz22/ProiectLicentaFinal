using ProiectLicenta.MVVM.ViewModel;
using System;
using System.Data.SQLite;


namespace ProiectLicenta.Model
{
    internal static class CpuDataInserter
    {
        public static void InsertCpuUsageInDb(string table)
        {
            // Load the config
            ConfigLoader cfgLoader = new ConfigLoader();
            var config = cfgLoader.LoadConfig();
            // Connect to the database
            string connectionString = $"Data Source=monitoring.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                // Insert CPU data into the table
                string formattedDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                CPUDataViewModel cpuObj = new CPUDataViewModel();
                float cpuUsagePercent = cpuObj.GetCpuUsage(config.RemoteIp, config.CommunityString, int.Parse(config.Port));
                if (cpuUsagePercent == -1)
                {
                    connection.Close();
                    return;
                }
                string insertDataQuery = $"INSERT INTO {table} (Timestamp, UsagePercentage) VALUES ('{formattedDateTime}', {cpuUsagePercent});";
                using (SQLiteCommand command = new SQLiteCommand(insertDataQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
    }
}
