using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProiectLicenta.MVVM.ViewModel
{
    internal class NetworkDataViewModel
    {
        public async Task<List<Dictionary<string, string>>> GetNetworkDataAsync(string ip, string communityString, int port)
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
                Dictionary<Oid, AsnType> snmpNetworkDict = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.31.1");
                
                Dictionary<string, string> resultDict = new Dictionary<string, string>();
                // Network data
                if (snmpNetworkDict != null)
                {
                    var interfaceNumber = snmpVerb.Get(SnmpVersion.Ver2, new string[] { "1.3.6.1.2.1.2.1.0" });
                    if (interfaceNumber != null)
                    {
                        resultDict.Add("Number of interfaces on the system: ", interfaceNumber[new Oid("1.3.6.1.2.1.2.1.0")].ToString());
                    }
                    else
                    {
                        resultDict.Add("Error Network monitoring #8: ", "Unable to get number of interfaces.");
                    }
                    if (snmpNetworkDict.ContainsKey(new Oid("1.3.6.1.2.1.31.1.1.1.1.1")))
                    {
                        resultDict.Add("First interface: ", snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.1")].ToString());
                    }
                    else
                    {
                        resultDict.Add("Error Network monitoring #1: ", "Unable to get interface.");
                    }
                    if (snmpNetworkDict.ContainsKey(new Oid("1.3.6.1.2.1.31.1.1.1.1.2")))
                    {
                        resultDict.Add("Second interface: ", snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.2")].ToString());
                    }
                    else
                    {
                        resultDict.Add("Error Network monitoring #2: ", "Unable to get interface.");
                    }
                    if (snmpNetworkDict.ContainsKey(new Oid("1.3.6.1.2.1.31.1.1.1.1.1")))
                    {
                        // Calculate usage % of the bandwidth of the first interface
                        Dictionary<Oid, AsnType> snmpNetworkDictFirstInitial = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.2.2");
                        float initialInOctetsFirstInterface = float.Parse(snmpNetworkDictFirstInitial[new Oid("1.3.6.1.2.1.2.2.1.10.1")].ToString());
                        float initialOutOctetsFirstInterface = float.Parse(snmpNetworkDictFirstInitial[new Oid("1.3.6.1.2.1.2.2.1.16.1")].ToString());
                        Dictionary<Oid, AsnType> timeTicksDictFirst = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.1.3");
                        float initialTimeticksFirst = CalculateTimeticks(timeTicksDictFirst[new Oid("1.3.6.1.2.1.1.3.0")].ToString());
                        await Task.Delay(5000);
                        Dictionary<Oid, AsnType> snmpNetworkDictFirstUpdated = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.2.2");
                        float updatedInOctetsFirstInterface = float.Parse(snmpNetworkDictFirstUpdated[new Oid("1.3.6.1.2.1.2.2.1.10.1")].ToString());
                        float updatedOutOctetsFirstInterface = float.Parse(snmpNetworkDictFirstUpdated[new Oid("1.3.6.1.2.1.2.2.1.16.1")].ToString());
                        timeTicksDictFirst = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.1.3");
                        float updatedTimeticksFirst = CalculateTimeticks(timeTicksDictFirst[new Oid("1.3.6.1.2.1.1.3.0")].ToString());
                        float ifSpeedFirstInterface = float.Parse(snmpVerb.Get(SnmpVersion.Ver2, new string[] { "1.3.6.1.2.1.2.2.1.5.1" })[new Oid("1.3.6.1.2.1.2.2.1.5.1")].ToString());
                        if (ifSpeedFirstInterface == 0)
                        {
                            ifSpeedFirstInterface = 1000000000;
                        }
                        float firstInterfaceInUtilizationPercent = (updatedInOctetsFirstInterface - initialInOctetsFirstInterface) * 8 * 100 / (((updatedTimeticksFirst - initialTimeticksFirst) / 100) * ifSpeedFirstInterface);
                        float firstInterfaceOutUtilizationPercent = (updatedOutOctetsFirstInterface - initialOutOctetsFirstInterface) * 8 * 100 / (((updatedTimeticksFirst - initialTimeticksFirst) / 100) * ifSpeedFirstInterface);
                        firstInterfaceInUtilizationPercent = Math.Abs(firstInterfaceInUtilizationPercent);
                        firstInterfaceOutUtilizationPercent = Math.Abs(firstInterfaceOutUtilizationPercent);
                        resultDict.Add($"Bandwidth utilization IN of the {snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.1")]} interface over 5 seconds: ", $"{(float)Math.Round(firstInterfaceInUtilizationPercent, 2)}%");
                        resultDict.Add($"Bandwidth utilization OUT of the {snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.1")]} interface over 5 seconds: ", $"{(float)Math.Round(firstInterfaceOutUtilizationPercent, 2)}%");
                        if (firstInterfaceInUtilizationPercent == -1 && firstInterfaceOutUtilizationPercent == -1)
                        {
                            resultDict.Add("Error Network monitoring #4:", "Unable to get the bandwidth utilization of the first interface.");
                        }
                        else
                        {
                            float totalBandwidthFirst = firstInterfaceInUtilizationPercent + firstInterfaceOutUtilizationPercent;
                            resultDict.Add($"Total bandwidth utilization of the {snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.1")]} interface over 5 seconds: ", $"{(float)Math.Round(totalBandwidthFirst, 2)}%");

                        }
                        // Calculate usage % of the bandwidth of the second interface
                        Dictionary<Oid, AsnType> snmpNetworkDictSecondInitial = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.2.2");
                        float initialInOctetsSecondInterface = float.Parse(snmpNetworkDictSecondInitial[new Oid("1.3.6.1.2.1.2.2.1.10.2")].ToString());
                        float initialOutOctetsSecondInterface = float.Parse(snmpNetworkDictSecondInitial[new Oid("1.3.6.1.2.1.2.2.1.16.2")].ToString());
                        Dictionary<Oid, AsnType> timeTicksDictSecond = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.1.3");
                        float initialTimeticksSecond = CalculateTimeticks(timeTicksDictSecond[new Oid("1.3.6.1.2.1.1.3.0")].ToString());
                        await Task.Delay(5000);
                        Dictionary<Oid, AsnType> snmpNetworkDictSecondUpdated = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.2.2");
                        float updatedInOctetsSecondInterface = float.Parse(snmpNetworkDictSecondUpdated[new Oid("1.3.6.1.2.1.2.2.1.10.2")].ToString());
                        float updatedOutOctetsSecondInterface = float.Parse(snmpNetworkDictSecondUpdated[new Oid("1.3.6.1.2.1.2.2.1.16.2")].ToString());
                        timeTicksDictSecond = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.1.3");
                        float updatedTimeticksSecond = CalculateTimeticks(timeTicksDictSecond[new Oid("1.3.6.1.2.1.1.3.0")].ToString());
                        float ifSpeedSecondInterface = float.Parse(snmpVerb.Get(SnmpVersion.Ver2, new string[] { "1.3.6.1.2.1.2.2.1.5.2" })[new Oid("1.3.6.1.2.1.2.2.1.5.2")].ToString());
                        if (ifSpeedSecondInterface == 0)
                        {
                            ifSpeedSecondInterface = 1000000000;
                        }
                        float secondInterfaceInUtilizationPercent = (updatedInOctetsSecondInterface - initialInOctetsSecondInterface) * 8 * 100 / (((updatedTimeticksSecond - initialTimeticksSecond) / 100) * ifSpeedSecondInterface);
                        float secondInterfaceOutUtilizationPercent = (updatedOutOctetsSecondInterface - initialOutOctetsSecondInterface) * 8 * 100 / (((updatedTimeticksSecond - initialTimeticksSecond) / 100) * ifSpeedSecondInterface);
                        secondInterfaceInUtilizationPercent = Math.Abs(secondInterfaceInUtilizationPercent);
                        secondInterfaceOutUtilizationPercent = Math.Abs(secondInterfaceOutUtilizationPercent);
                        resultDict.Add($"Bandwidth utilization IN of the {snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.2")]} interface over 5 seconds: ", $"{(float)Math.Round(secondInterfaceInUtilizationPercent, 2)}%");
                        resultDict.Add($"Bandwidth utilization OUT of the {snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.2")]} interface over 5 seconds: ", $"{(float)Math.Round(secondInterfaceOutUtilizationPercent, 2)}%");
                        if (secondInterfaceInUtilizationPercent == -1 && secondInterfaceOutUtilizationPercent == -1)
                        {
                            resultDict.Add("Error Network monitoring #4:", "Unable to get the bandwidth utilization of the first interface.");
                        }
                        else
                        {
                            float totalBandwidthSecond = secondInterfaceInUtilizationPercent + secondInterfaceOutUtilizationPercent;
                            resultDict.Add($"Total bandwidth utilization of the {snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.2")]} interface over 5 seconds: ", $"{(float)Math.Round(totalBandwidthSecond, 2)}%");
                        }
                    }
                    else
                    {
                        resultDict.Add("Error Network monitoring #3:", "Unable to get the bandwidth of the first interface.");
                    }
                    var operationalStatusFirstInterface = snmpVerb.Get(SnmpVersion.Ver1, new string[] { "1.3.6.1.2.1.2.2.1.8.1" });
                    var operationalStatusSecondInterface = snmpVerb.Get(SnmpVersion.Ver1, new string[] { "1.3.6.1.2.1.2.2.1.8.2" });
                    if (operationalStatusFirstInterface != null)
                    {
                        if (operationalStatusFirstInterface[new Oid("1.3.6.1.2.1.2.2.1.8.1")] != null)
                        {

                            if (operationalStatusFirstInterface[new Oid("1.3.6.1.2.1.2.2.1.8.1")].ToString() == "1")
                            {
                                resultDict.Add($"Operational status of {snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.1")]} interface: ", "UP");
                            }
                            else
                            {
                                    resultDict.Add($"Operational status of {snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.1")]} interface: ", "DOWN");
                            }
                        }
                        else
                        {
                            resultDict.Add("Error Network monitoring #6:", "Unable to get operational status of the first interface.");
                        }
                    }
                    if (operationalStatusSecondInterface != null) {
                        if (operationalStatusSecondInterface[new Oid("1.3.6.1.2.1.2.2.1.8.2")] != null)
                        {
                            if (operationalStatusSecondInterface[new Oid("1.3.6.1.2.1.2.2.1.8.2")].ToString() == "1")
                            {
                                resultDict.Add($"Operational status of {snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.2")]} interface: ", "UP");
                            }
                            else
                            {
                                resultDict.Add($"Operational status of {snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.2")]} interface: ", "DOWN");
                            }
                        }
                        else
                        {
                            resultDict.Add("Error Network monitoring #7:", "Unable to get operational status of the second interface.");
                        }
                    }
                    var strSpeedDict1 = snmpVerb.Get(SnmpVersion.Ver1, new string[] { "1.3.6.1.2.1.2.2.1.5.1" });
                    if (strSpeedDict1.Count > 0) {
                        resultDict.Add($"{snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.1")]} interface speed: ", $"{int.Parse(strSpeedDict1[new Oid("1.3.6.1.2.1.2.2.1.5.1")].ToString()) / 1000000} MB/s");
                    }
                    else
                    {
                        resultDict.Add("Error Network monitoring #8: ", "Unable to get first interface speed.");
                    }
                    var strSpeedDict2 = snmpVerb.Get(SnmpVersion.Ver1, new string[] { "1.3.6.1.2.1.2.2.1.5.2" });
                    if (strSpeedDict2.Count > 0) {
                        resultDict.Add($"{snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.2")]} interface speed: ", $"{int.Parse(strSpeedDict2[new Oid("1.3.6.1.2.1.2.2.1.5.2")].ToString()) / 1000000} MB/s");
                    }
                    else
                    {
                        resultDict.Add("Error Network monitoring #8: ", "Unable to get second interface speed.");
                    }
                }
                else
                {
                    resultDict.Add("Error Network monitoring #0: ", "Unable to get Network data.");
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
        public static async Task<Dictionary<string, string>> GetNetworkUsage(string ip, string communityString, int port)
        {
            SimpleSnmp snmpVerb = new SimpleSnmp(ip, port, communityString);

            if (!snmpVerb.Valid)
            {
                return null;
            }
            Dictionary<Oid, AsnType> snmpNetworkDict = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.31.1");
            Dictionary<string, string> resultDict = new Dictionary<string, string>();
            if (snmpNetworkDict.Count > 0)
            {
                // Calculate usage % of the bandwidth of the first interface
                Dictionary<Oid, AsnType> snmpNetworkDictFirstInitial = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.2.2");
                float initialInOctetsFirstInterface = float.Parse(snmpNetworkDictFirstInitial[new Oid("1.3.6.1.2.1.2.2.1.10.1")].ToString());
                float initialOutOctetsFirstInterface = float.Parse(snmpNetworkDictFirstInitial[new Oid("1.3.6.1.2.1.2.2.1.16.1")].ToString());
                Dictionary<Oid, AsnType> timeTicksDictFirst = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.1.3");
                float initialTimeticksFirst = CalculateTimeticks(timeTicksDictFirst[new Oid("1.3.6.1.2.1.1.3.0")].ToString());
                await Task.Delay(5000);
                Dictionary<Oid, AsnType> snmpNetworkDictFirstUpdated = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.2.2");
                float updatedInOctetsFirstInterface = float.Parse(snmpNetworkDictFirstUpdated[new Oid("1.3.6.1.2.1.2.2.1.10.1")].ToString());
                float updatedOutOctetsFirstInterface = float.Parse(snmpNetworkDictFirstUpdated[new Oid("1.3.6.1.2.1.2.2.1.16.1")].ToString());
                timeTicksDictFirst = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.1.3");
                float updatedTimeticksFirst = CalculateTimeticks(timeTicksDictFirst[new Oid("1.3.6.1.2.1.1.3.0")].ToString());
                float ifSpeedFirstInterface = float.Parse(snmpVerb.Get(SnmpVersion.Ver2, new string[] { "1.3.6.1.2.1.2.2.1.5.1" })[new Oid("1.3.6.1.2.1.2.2.1.5.1")].ToString());
                if (ifSpeedFirstInterface == 0)
                {
                    ifSpeedFirstInterface = 1000000000;
                }
                float firstInterfaceInUtilizationPercent = (updatedInOctetsFirstInterface - initialInOctetsFirstInterface) * 8 * 100 / (((updatedTimeticksFirst - initialTimeticksFirst) / 100) * ifSpeedFirstInterface);
                float firstInterfaceOutUtilizationPercent = (updatedOutOctetsFirstInterface - initialOutOctetsFirstInterface) * 8 * 100 / (((updatedTimeticksFirst - initialTimeticksFirst) / 100) * ifSpeedFirstInterface);
                firstInterfaceInUtilizationPercent = Math.Abs(firstInterfaceInUtilizationPercent);
                firstInterfaceOutUtilizationPercent = Math.Abs(firstInterfaceOutUtilizationPercent);
                float totalBandwidthFirst = firstInterfaceInUtilizationPercent + firstInterfaceOutUtilizationPercent;
                resultDict.Add("bandwidthUsageFirstInterface", ((float)Math.Round(totalBandwidthFirst, 2)).ToString());
                // Calculate usage % of the bandwidth of the second interface
                Dictionary<Oid, AsnType> snmpNetworkDictSecondInitial = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.2.2");
                float initialInOctetsSecondInterface = float.Parse(snmpNetworkDictSecondInitial[new Oid("1.3.6.1.2.1.2.2.1.10.2")].ToString());
                float initialOutOctetsSecondInterface = float.Parse(snmpNetworkDictSecondInitial[new Oid("1.3.6.1.2.1.2.2.1.16.2")].ToString());
                Dictionary<Oid, AsnType> timeTicksDictSecond = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.1.3");
                float initialTimeticksSecond = CalculateTimeticks(timeTicksDictSecond[new Oid("1.3.6.1.2.1.1.3.0")].ToString());
                await Task.Delay(5000);
                Dictionary<Oid, AsnType> snmpNetworkDictSecondUpdated = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.2.2");
                float updatedInOctetsSecondInterface = float.Parse(snmpNetworkDictSecondUpdated[new Oid("1.3.6.1.2.1.2.2.1.10.2")].ToString());
                float updatedOutOctetsSecondInterface = float.Parse(snmpNetworkDictSecondUpdated[new Oid("1.3.6.1.2.1.2.2.1.16.2")].ToString());
                timeTicksDictSecond = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.1.3");
                float updatedTimeticksSecond = CalculateTimeticks(timeTicksDictSecond[new Oid("1.3.6.1.2.1.1.3.0")].ToString());
                float ifSpeedSecondInterface = float.Parse(snmpVerb.Get(SnmpVersion.Ver2, new string[] { "1.3.6.1.2.1.2.2.1.5.2" })[new Oid("1.3.6.1.2.1.2.2.1.5.2")].ToString());
                if (ifSpeedSecondInterface == 0)
                {
                    ifSpeedSecondInterface = 1000000000;
                }
                float secondInterfaceInUtilizationPercent = (updatedInOctetsSecondInterface - initialInOctetsSecondInterface) * 8 * 100 / (((updatedTimeticksSecond - initialTimeticksSecond) / 100) * ifSpeedSecondInterface);
                float secondInterfaceOutUtilizationPercent = (updatedOutOctetsSecondInterface - initialOutOctetsSecondInterface) * 8 * 100 / (((updatedTimeticksSecond - initialTimeticksSecond) / 100) * ifSpeedSecondInterface);
                secondInterfaceInUtilizationPercent = Math.Abs(secondInterfaceInUtilizationPercent);
                secondInterfaceOutUtilizationPercent = Math.Abs(secondInterfaceOutUtilizationPercent);
                float totalBandwidthSecond = secondInterfaceInUtilizationPercent + secondInterfaceOutUtilizationPercent;
                resultDict.Add("bandwidthUsageSecondInterface", ((float)Math.Round(totalBandwidthSecond, 2)).ToString());
                resultDict.Add("firstInterface", snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.1")].ToString());
                resultDict.Add("secondInterface", snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.2")].ToString());
                return resultDict;
            }
            else
            {
                return null;
            }
        }

        public Dictionary<string, string> GetInterfaces(string ip, string communityString, int port)
        {
            SimpleSnmp snmpVerb = new SimpleSnmp(ip, port, communityString);
            if (!snmpVerb.Valid)
            {
                return null;
            }
            Dictionary<Oid, AsnType> snmpNetworkDict = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.2.1.31.1");
            Dictionary<string, string> resultDict = new Dictionary<string, string>();
            if (snmpNetworkDict != null)
            {
                if (snmpNetworkDict.ContainsKey(new Oid("1.3.6.1.2.1.31.1.1.1.1.1")))
                {
                    resultDict.Add("firstInterface", snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.1")].ToString());
                    resultDict.Add("secondInterface", snmpNetworkDict[new Oid("1.3.6.1.2.1.31.1.1.1.1.2")].ToString());
                }
            }
            return resultDict;
        }

        public static float CalculateTimeticks(string timestamp)
        {
            int days = 0, hours = 0, minutes = 0, seconds = 0, milliseconds = 0;

            string[] components = timestamp.Split(' ');

            foreach (string component in components)
            {
                if (component.EndsWith("d"))
                {
                    days = int.Parse(component.TrimEnd('d'));
                }
                else if (component.EndsWith("h"))
                {
                    hours = int.Parse(component.TrimEnd('h'));
                }
                else if (component.EndsWith("m"))
                {
                    minutes = int.Parse(component.TrimEnd('m'));
                }
                else if (component.EndsWith("s"))
                {
                    if (component.EndsWith("ms"))
                    {
                        milliseconds = int.Parse(component.TrimEnd('m', 's'));
                    }
                    else
                    {
                        seconds = int.Parse(component.TrimEnd('s'));
                    }
                }
            }

            int timeticks = ((days * 24 + hours) * 3600 + minutes * 60 + seconds) * 100 + milliseconds;
            return timeticks;
        }
    }
}
