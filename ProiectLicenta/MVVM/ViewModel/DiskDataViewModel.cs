using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;

namespace ProiectLicenta.MVVM.ViewModel
{
    internal class DiskDataViewModel
    {
        public List<Dictionary<string, string>> GetDiskData(string ip, string username, string password)
        {
            bool invalidUser = false;
            bool invalidPassword = false;
            // disk data
            Dictionary<string, string> resultDictErr = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(username))
            {
                resultDictErr.Add("Fatal error disk data #1:", "invalid username");
                invalidUser = true;
            }
            if (string.IsNullOrEmpty(password))
            {
                resultDictErr.Add("Fatal error disk data #2:", "invalid password");
                invalidPassword = true;
            }
            if (invalidUser || invalidPassword)
            {
                var diskDataListErr = new List<Dictionary<string, string>>
                {
                    resultDictErr
                };
                return diskDataListErr;
            }
            while (true)
            {
                Task.Delay(2000);
                using (var client = new SshClient(ip, username, password))
                {
                    Dictionary<string, string> resultDict = new Dictionary<string, string>();
                    try
                    {
                        client.Connect();
                        var commandDf = client.CreateCommand("df -h");
                        var resultDf = commandDf.Execute();
                        var commandDm = client.CreateCommand("dmidecode -t baseboard");
                        var resultDm = commandDm.Execute();
                        client.Disconnect();
                        var lines = resultDf.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        var linesDm = resultDm.Split('\n');
                        var diskDataList = new List<Dictionary<string, string>>();
                        var smbiosVer = new Dictionary<string, string> { { "SMBIOS version: ", linesDm[2] } };
                        diskDataList.Add(smbiosVer);
                        foreach (var line in lines.Skip(1))
                        {
                            var values = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            var diskData = new Dictionary<string, string>
                            {
                                { "The file System name: ", values[0] },
                                { "Total size of the storage partition: ", values[1] },
                                { "Total space utilization of the partition: ", values[2] },
                                { "Total available space: ", values[3] },
                                { "Usage percent: ", values[4] },
                                { "Mounted on: ", values[5] }
                            };

                            diskDataList.Add(diskData);
                        }
                        return diskDataList;
                    }
                    catch (Exception ex)
                    {
                        var diskDataListExc = new List<Dictionary<string, string>>();
                        var diskData = new Dictionary<string, string>
                        {
                            { "Error Disk monitoring #0", ex.Message }
                        };
                        diskDataListExc.Add(diskData);

                        return diskDataListExc;
                    }
                }
            }

        }
      
        public List<List<string>> GetDiskUsage(string ip, string username, string password)
        {
            var newListErr = new List<string>();
            var newListsOfListErr = new List<List<string>>();
            if (string.IsNullOrEmpty(username))
            {
                newListErr.Add("Fatal error disk data #1: invalid username");
            }
            if (string.IsNullOrEmpty(password))
            {
                newListErr.Add("Fatal error disk data #2: invalid password");
            }
            if (newListErr.Count > 0)
            {
                newListsOfListErr.Add(newListErr);
                return newListsOfListErr;
            }
            using (var client = new SshClient(ip, username, password))
            {
                try
                {
                    client.Connect();
                    var commandDf = client.CreateCommand("df -h");
                    var resultDf = commandDf.Execute();
                    client.Disconnect();
                    var lines = resultDf.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var diskDataList = new List<List<string>>();
                    foreach (var line in lines.Skip(1))
                    {
                        var values = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (values[0] == "/dev/sda3")
                        {
                            var newList = new List<string>
                            {
                                values[0],
                                ConvertStorageValueToGB(values[1]).ToString(),
                                ConvertStorageValueToGB(values[2]).ToString(),
                                ConvertStorageValueToGB(values[3]).ToString()
                            };
                            diskDataList.Add(newList);
                        }
                        if (values[0] == "/dev/sda2")
                        {
                            var newList = new List<string>
                            {
                                values[0],
                                ConvertStorageValueToGB(values[1]).ToString(),
                                ConvertStorageValueToGB(values[2]).ToString(),
                                ConvertStorageValueToGB(values[3]).ToString()
                            };
                            diskDataList.Add(newList);
                        }
                    }
                    client.Disconnect();
                    return diskDataList;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    return null;
                }
            }
        }
        public Dictionary<string, string> GetPartitions(string ip, string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                return null;
            }
            if (string.IsNullOrEmpty(password))
            {
                return null;
            }
            Dictionary<string, string> resultDict = new Dictionary<string, string>();
            using (var client = new SshClient(ip, username, password))
            {
                try
                {
                    client.Connect();
                    var commandDf = client.CreateCommand("df -h");
                    var resultDf = commandDf.Execute();
                    client.Disconnect();
                    var lines = resultDf.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var partitionDict = new Dictionary<string, string>();
                    foreach (var line in lines.Skip(1))
                    {
                        var values = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (values[0] == "/dev/sda3")
                        {
                            partitionDict.Add("firstPartition", values[0]);
                        }
                        if (values[0] == "/dev/sda2")
                        {
                            partitionDict.Add("secondPartition", values[0]);
                        }
                    }
                    client.Disconnect();
                    return partitionDict;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    return null;
                }
            }
        }

        private double ConvertStorageValueToGB(string storageValue)
        {
            if (string.IsNullOrEmpty(storageValue))
                return 0;

            string suffix = storageValue.Substring(storageValue.Length - 1, 1).ToUpper();
            string numericValue = storageValue.Substring(0, storageValue.Length - 1);

            if (!double.TryParse(numericValue, out double value))
                return 0;

            switch (suffix)
            {
                case "K":
                    return value / (1024 * 1024);
                case "M":
                    return value / 1024;
                case "G":
                    return value;
                default:
                    return 0;
            }
        }
    }
}
