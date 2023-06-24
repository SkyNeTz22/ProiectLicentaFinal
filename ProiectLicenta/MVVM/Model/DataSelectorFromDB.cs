using System.Collections.Generic;
using System.Data.SQLite;


namespace ProiectLicenta.Model
{
    internal static class DataSelectorFromDB
    {
        public static List<Dictionary<string, string>> GetTotalUsageData(string table, string networkInterface, string partition, bool ram)
        {
            List<Dictionary<string, string>> jsonArray = new List<Dictionary<string, string>>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=monitoring.db;Version=3;"))
            {
                connection.Open();
                if (networkInterface == "" && partition == "")
                {
                    if (ram == true)
                    {
                        using (SQLiteCommand command = new SQLiteCommand($"SELECT Timestamp, Usage FROM {table} ORDER BY datetime(Timestamp) ASC", connection))
                        {
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Dictionary<string, string> jsonObject = new Dictionary<string, string>
                                {
                                    { reader.GetString(0), reader.GetFloat(1).ToString() }
                                };
                                    jsonArray.Add(jsonObject);
                                }
                            }
                        }
                    }
                    else
                    {
                        using (SQLiteCommand command = new SQLiteCommand($"SELECT Timestamp, UsagePercentage FROM {table} ORDER BY datetime(Timestamp) ASC", connection))
                        {
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Dictionary<string, string> jsonObject = new Dictionary<string, string>
                                {
                                    { reader.GetString(0), reader.GetFloat(1).ToString() }
                                };
                                    jsonArray.Add(jsonObject);
                                }
                            }
                        }
                    }
                }
                else if (networkInterface != "" && partition == "")
                {
                    using (SQLiteCommand command = new SQLiteCommand($"SELECT Timestamp, UsagePercentage, Interface FROM {table} WHERE Interface = '{networkInterface}' ORDER BY datetime(Timestamp) ASC", connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, string> jsonObject = new Dictionary<string, string>
                                {
                                    { reader.GetString(0), reader.GetFloat(1).ToString() }
                                };
                                jsonArray.Add(jsonObject);
                            }
                        }
                    }
                }
                else if (networkInterface == "" && partition != "")
                {
                    using (SQLiteCommand command = new SQLiteCommand($"SELECT Timestamp, Partition, TotalSpace, FreeSpace, UsedSpace FROM {table} WHERE Partition = '{partition}' ORDER BY datetime(Timestamp) ASC", connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, string> jsonObject = new Dictionary<string, string>
                                {
                                    { "partition", reader.GetString(0) },
                                    { "totalSpace", reader.GetString(1) },
                                    { "freeSpace", reader.GetString(2) },
                                    { "usedSpace", reader.GetString(3) },
                                };
                                jsonArray.Add(jsonObject);
                            }
                        }
                    }
                }

                connection.Close();
            }
            if (jsonArray.Count > 0)
            {
                return jsonArray;
            }
            else
            {
                return null;
            }
        }

        public static Dictionary<string, string> GetDiskData(string partition)
        {
            Dictionary<string, string> jsonObject = new Dictionary<string, string>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=monitoring.db;Version=3;"))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand($"SELECT Partition, TotalSpace, FreeSpace, UsedSpace FROM DiskLiveUsageData WHERE Partition = '{partition}' ORDER BY datetime(Timestamp) DESC LIMIT 1", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            jsonObject.Add("partition", reader.GetString(0));
                            jsonObject.Add("totalSpace", reader.GetString(1));
                            jsonObject.Add("freeSpace", reader.GetString(2));
                            jsonObject.Add("usedSpace", reader.GetString(3));
                        }
                    }
                }
                connection.Close();
            }
            return jsonObject;
        }

        public static string GetLastUsageValue(string table, string networkInterface, bool ram)
        {
            if (networkInterface == "")
            {
                if (ram)
                {
                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=monitoring.db;Version=3;"))
                    {
                        connection.Open();
                        using (SQLiteCommand command = new SQLiteCommand($"SELECT Usage FROM {table} ORDER BY datetime(Timestamp) DESC LIMIT 1", connection))
                        {
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    return reader.GetFloat(0).ToString();
                                }
                            }
                        }
                        connection.Close();
                    }
                }
                else
                {
                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=monitoring.db;Version=3;"))
                    {
                        connection.Open();
                        using (SQLiteCommand command = new SQLiteCommand($"SELECT UsagePercentage FROM {table} ORDER BY Timestamp DESC LIMIT 1", connection))
                        {
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    return reader.GetFloat(0).ToString();
                                }
                            }
                        }
                        connection.Close();
                    }
                }
            }
            else
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=monitoring.db;Version=3;"))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand($"SELECT UsagePercentage FROM {table} WHERE Interface = '{networkInterface}' ORDER BY Timestamp DESC LIMIT 1", connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                return reader.GetFloat(0).ToString();
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return "";
        }
    }
}
