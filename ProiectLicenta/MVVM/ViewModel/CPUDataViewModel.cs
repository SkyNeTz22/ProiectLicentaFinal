using SnmpSharpNet;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProiectLicenta.MVVM.ViewModel
{
    internal class CPUDataViewModel
    {
        public List<Dictionary<string, string>> GetCpuData(string ip, string communityString, int port)
        {
            SimpleSnmp snmpVerb = new SimpleSnmp(ip, port, communityString);
            if (!snmpVerb.Valid)
            {
                Dictionary<string, string> resultDict = new Dictionary<string, string>();
                resultDict.Add("FatalError: ", "Could not connect to the IP.");
                var systemDataList = new List<Dictionary<string, string>>();
                foreach (KeyValuePair<string, string> entry in resultDict)
                {
                    var systemData = new Dictionary<string, string>
                {
                    { entry.Key, entry.Value }
                };

                    systemDataList.Add(systemData);
                }
                return systemDataList;
            }
            while (true)
            {
                Task.Delay(2000);
                Dictionary<Oid, AsnType> snmpDataCPUDict = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.4.1.2021.11");
                int firstOid = 196608;
                int cpuCoreCount = 0;
                Dictionary<Oid, AsnType> snmpCPUGeneralInformation = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.25.3.2.1.3");
                Dictionary<string, string> resultDict = new Dictionary<string, string>();
                // cpu model
                if (snmpCPUGeneralInformation != null)
                {
                    if (snmpCPUGeneralInformation.ContainsKey(new Oid("1.3.6.1.2.1.25.3.2.1.3.196608")))
                    {
                        resultDict.Add("CPU model: ", snmpCPUGeneralInformation[new Oid("1.3.6.1.2.1.25.3.2.1.3.196608")].ToString());
                    }
                    else
                    {
                        resultDict.Add("Error CPU monitoring #6: ", "Unable to get CPU name.");
                    }
                }
                // cpu usage data
                Dictionary<Oid, AsnType> snmpCpuCoreData = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.25.3.3.1.2");
                float cpuUsagePercent = -1;
                if (snmpCpuCoreData != null)
                {
                    foreach (KeyValuePair<Oid, AsnType> entry in snmpCpuCoreData)
                    {
                        // Check if the SNMP OID represents a CPU core
                        if (entry.Key.ToString().StartsWith("1.3.6.1.2.1.25.3.3.1.2"))
                        {
                            cpuCoreCount++;
                            cpuUsagePercent += int.Parse(snmpCpuCoreData[new Oid($"1.3.6.1.2.1.25.3.3.1.2.{firstOid + cpuCoreCount - 1}")].ToString());
                        }
                    }
                    cpuUsagePercent /= cpuCoreCount;
                    resultDict.Add("CPU usage:", $"{cpuUsagePercent} %");
                }
                else
                {
                    resultDict.Add("Error CPU monitoring #1: ", "Unable to get CPU usage.");
                }

                if (snmpDataCPUDict != null)
                {
                    if (snmpDataCPUDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.11.52.0")))
                    {
                        float frequency = float.Parse(snmpDataCPUDict[new Oid("1.3.6.1.4.1.2021.11.52.0")].ToString()) / 10000000;
                        resultDict.Add("CPU frequency:", $"{frequency.ToString()} GHz");
                    }
                    else
                    {
                        resultDict.Add("Error CPU monitoring #10: ", "Unable to get CPU frequency.");
                    }
                    float cpuIdleTime;
                    if (snmpDataCPUDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.11.52.0")) && snmpDataCPUDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.11.50.0")))
                    {
                        cpuIdleTime = float.Parse(snmpDataCPUDict[new Oid("1.3.6.1.4.1.2021.11.52.0")].ToString()) / float.Parse(snmpDataCPUDict[new Oid("1.3.6.1.4.1.2021.11.50.0")].ToString());
                    }
                    else
                    {
                        cpuIdleTime = -1;
                    }
                    if (cpuIdleTime == -1)
                    {
                        resultDict.Add("Error CPU monitoring #2: ", "Unable to get CPU idle time.");
                    }
                    else
                    {
                        resultDict.Add("CPU idle time:", cpuIdleTime.ToString());
                    }

                    if (snmpDataCPUDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.11.60.0")))
                    {
                        resultDict.Add("Context switches since last boot: ", snmpDataCPUDict[new Oid("1.3.6.1.4.1.2021.11.60.0")].ToString());
                    }
                    else
                    {
                        resultDict.Add("Error CPU monitoring #3: ", "Unable to get context switchs since last boot.");
                    }

                    if (snmpDataCPUDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.11.59.0")))
                    {
                        resultDict.Add("Processed interrupts since last boot: ", snmpDataCPUDict[new Oid("1.3.6.1.4.1.2021.11.59.0")].ToString());
                    }
                    else
                    {
                        resultDict.Add("Error CPU monitoring #4: ", "Unable to get proccessed interrupts since last boot.");
                    }

                    if (snmpDataCPUDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.11.67.0")))
                    {
                        resultDict.Add("Memory buffer cache hits: ", snmpDataCPUDict[new Oid("1.3.6.1.4.1.2021.11.67.0")].ToString());
                    }
                    else
                    {
                        resultDict.Add("Error CPU monitoring #5: ", "Unable to get memory buffer cache hits.");
                    }
                    // cpu loads
                    Dictionary<Oid, AsnType> snmpCpuLoadDict = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.4.1.2021.10.1.3");
                    if (snmpCpuLoadDict != null)
                    {
                        if (snmpCpuLoadDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.10.1.3.1")))
                        {
                            resultDict.Add("CPU load average over the last minute: ", snmpCpuLoadDict[new Oid("1.3.6.1.4.1.2021.10.1.3.1")].ToString() + "%");

                        }
                        else
                        {
                            resultDict.Add("Error CPU load monitoring #1: ", "Unable to get cpu load average over the last minute.");
                        }
                        if (snmpCpuLoadDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.10.1.3.2")))
                        {
                            resultDict.Add("CPU load average over the last 5 minutes: ", snmpCpuLoadDict[new Oid("1.3.6.1.4.1.2021.10.1.3.2")].ToString() + "%");

                        }
                        else
                        {
                            resultDict.Add("Error CPU load monitoring #2: ", "Unable to get cpu load average over the last 5 minutes.");
                        }
                        if (snmpCpuLoadDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.10.1.3.3")))
                        {
                            resultDict.Add("CPU load average over the last 15 minutes: ", snmpCpuLoadDict[new Oid("1.3.6.1.4.1.2021.10.1.3.3")].ToString() + "%");

                        }
                        else
                        {
                            resultDict.Add("Error CPU load monitoring #3: ", "Unable to get cpu load average over the last 15 minutes.");
                        }
                    }
                    else
                    {
                        resultDict.Add("Error CPU load monitoring #0: ", "Could not get CPU load");
                    }
                }
                else
                {
                    resultDict.Add("Error CPU monitoring #0: ", "Unable to get CPU data.");
                    var systemDataListElse = new List<Dictionary<string, string>>();

                    foreach (KeyValuePair<string, string> entry in resultDict)
                    {
                        var systemData = new Dictionary<string, string>
                        {
                            { entry.Key, entry.Value }
                        };

                        systemDataListElse.Add(systemData);
                    }
                    return systemDataListElse;
                }

                var systemDataList = new List<Dictionary<string, string>>();
                foreach (KeyValuePair<string, string> entry in resultDict)
                {
                    var systemData = new Dictionary<string, string>
                    {
                        { entry.Key, entry.Value }
                    };

                    systemDataList.Add(systemData);
                }
                return systemDataList;
            }

        }
        public float GetCpuUsage(string ip, string communityString, int port)
        {
            SimpleSnmp snmpVerb = new SimpleSnmp(ip, port, communityString);
            float cpuUsagePercent = -1;
            int cpuCoreCount = 0;
            if (!snmpVerb.Valid)
            {
                return cpuUsagePercent;
            }
            int firstOid = 196608;
            Dictionary<Oid, AsnType> snmpCpuCoreData = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.25.3.3.1.2");
            if (snmpCpuCoreData != null)
            {
                // Iterate through the SNMP data to count the CPU cores
                foreach (KeyValuePair<Oid, AsnType> entry in snmpCpuCoreData)
                {
                    // Check if the SNMP OID represents a CPU core
                    if (entry.Key.ToString().StartsWith("1.3.6.1.2.1.25.3.3.1.2"))
                    {
                        cpuCoreCount++;
                        cpuUsagePercent += int.Parse(snmpCpuCoreData[new Oid($"1.3.6.1.2.1.25.3.3.1.2.{firstOid + cpuCoreCount - 1}")].ToString());
                    }
                }
                cpuUsagePercent /= cpuCoreCount;
            }
            return cpuUsagePercent;
        }
    }
}
