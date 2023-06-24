using ProiectLicenta.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ProiectLicenta.Model
{
    internal class DiskDataInserter
    {
        public static void InsertDiskUsageInDb(string table)
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
                DiskDataViewModel diskObj = new DiskDataViewModel();
                List<List<string>> resultDict = diskObj.GetDiskUsage(config.RemoteIp, config.Username, config.Password);
                if (resultDict.Count > 0 && resultDict != null)
                {
                    foreach (var item in resultDict)
                    {
                        string insertDataQuery = $"INSERT INTO {table} (Timestamp, Partition, TotalSpace, FreeSpace, UsedSpace) VALUES ('{formattedDateTime}', '{item[0]}', '{item[1]}', '{item[3]}', '{item[2]}');";
                        using (SQLiteCommand command = new SQLiteCommand(insertDataQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
                connection.Close();
            }
        }
    }
}
