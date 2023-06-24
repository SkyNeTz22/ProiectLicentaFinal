using ProiectLicenta.MVVM.ViewModel;
using System;
using System.Data.SQLite;

namespace ProiectLicenta.Model
{
    internal class NetworkDataInserter
    {

        public static async void InsertNetworkUsageInDb(string table)
        {
            // Load the config
            ConfigLoader cfgLoader = new ConfigLoader();
            var config = cfgLoader.LoadConfig();
            // Connect to the database
            string connectionString = $"Data Source=monitoring.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                // Insert Network data into the table
                string formattedDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var networkUsage = await NetworkDataViewModel.GetNetworkUsage(config.RemoteIp, config.CommunityString, int.Parse(config.Port));
                if (networkUsage != null)
                {
                    string insertDataQuery1 = $"INSERT INTO {table} (Timestamp, Interface, UsagePercentage) VALUES ('{formattedDateTime}', '{networkUsage["firstInterface"]}', {networkUsage["bandwidthUsageFirstInterface"]});";
                    string insertDataQuery2 = $"INSERT INTO {table} (Timestamp, Interface, UsagePercentage) VALUES ('{formattedDateTime}', '{networkUsage["secondInterface"]}', {networkUsage["bandwidthUsageSecondInterface"]});";

                    using (SQLiteCommand command = new SQLiteCommand(insertDataQuery1, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    using (SQLiteCommand command = new SQLiteCommand(insertDataQuery2, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    connection.Close();
                }
                connection.Close();
            }
        }
    }
}
