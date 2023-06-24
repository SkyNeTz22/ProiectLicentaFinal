using SnmpSharpNet;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProiectLicenta.MVVM.ViewModel
{
    internal class RAMDataViewModel
    {
        public List<Dictionary<string, string>> GetRamData(string ip, string communityString, int port)
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
                Dictionary<Oid, AsnType> snmpRamDict = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.4.1.2021.4");
                Dictionary<string, string> resultDict = new Dictionary<string, string>();
                // Ram data
                if (snmpRamDict != null)
                {
                    if (snmpRamDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.4.3.0")))
                    {
                        resultDict.Add("Total SWAP memory: ", (int.Parse(snmpRamDict[new Oid("1.3.6.1.4.1.2021.4.3.0")].ToString()) / 1024).ToString() + " MB");
                    }
                    else
                    {
                        resultDict.Add("Error RAM Monitoring #1: ", "Unable to get total SWAP memory.");
                    }

                    if (snmpRamDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.4.4.0")))
                    {
                        resultDict.Add("Total available SWAP space: ", (int.Parse(snmpRamDict[new Oid("1.3.6.1.4.1.2021.4.4.0")].ToString()) / 1024).ToString() + " MB");
                    }
                    else
                    {
                        resultDict.Add("Error RAM Monitoring #2: ", "Unable to get total SWAP space.");
                    }

                    if (snmpRamDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.4.5.0")))
                    {
                        resultDict.Add("Total RAM memory: ", (int.Parse(snmpRamDict[new Oid("1.3.6.1.4.1.2021.4.5.0")].ToString()) / 1024).ToString() + " MB");
                    }
                    else
                    {
                        resultDict.Add("Error RAM Monitoring #3: ", "Unable to get total RAM memory.");
                    }

                    if (snmpRamDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.4.11.0")))
                    {
                        resultDict.Add("Total free RAM memory: ", (int.Parse(snmpRamDict[new Oid("1.3.6.1.4.1.2021.4.11.0")].ToString()) / 1024).ToString() + " MB");
                    }
                    else
                    {
                        resultDict.Add("Error RAM Monitoring #4: ", "Unable to get total free RAM memory.");
                    }

                    if (snmpRamDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.4.5.0")) && snmpRamDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.4.11.0")))
                    {
                        resultDict.Add("Total used RAM memory: ", ((int.Parse(snmpRamDict[new Oid("1.3.6.1.4.1.2021.4.5.0")].ToString()) / 1024) - (int.Parse(snmpRamDict[new Oid("1.3.6.1.4.1.2021.4.11.0")].ToString()) / 1024)).ToString() + " MB");
                    }
                    else
                    {
                        resultDict.Add("Error RAM Monitoring #5: ", "Unable to get total used RAM memory.");
                    }

                    if (snmpRamDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.4.13.0")))
                    {
                        resultDict.Add("Total shared RAM memory: ", (int.Parse(snmpRamDict[new Oid("1.3.6.1.4.1.2021.4.13.0")].ToString()) / 1024).ToString() + " MB");
                    }
                    else
                    {
                        resultDict.Add("Error RAM Monitoring #6: ", "Unable to get total shared RAM memory.");
                    }

                    if (snmpRamDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.4.14.0")))
                    {
                        resultDict.Add("Total RAM memory buffer: ", (int.Parse(snmpRamDict[new Oid("1.3.6.1.4.1.2021.4.14.0")].ToString()) / 1024).ToString() + " MB");
                    }
                    else
                    {
                        resultDict.Add("Error RAM Monitoring #7: ", "Unable to get total RAM memory buffer.");
                    }

                    if (snmpRamDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.4.15.0")))
                    {
                        resultDict.Add("Total cached RAM memory: ", (int.Parse(snmpRamDict[new Oid("1.3.6.1.4.1.2021.4.15.0")].ToString()) / 1024).ToString() + " MB");
                    }
                    else
                    {
                        resultDict.Add("Error RAM Monitoring #8: ", "Unable to get total cached RAM memory.");
                    }
                }
                else
                {
                    resultDict.Add("Error RAM Monitoring #0: ", "Unable to get System general data.");
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

        public float GetRamUsage(string ip, string communityString, int port)
        {
            float ramUsage = -1;
            SimpleSnmp snmpVerb = new SimpleSnmp(ip, port, communityString);
            if (!snmpVerb.Valid)
            {
                return ramUsage;
            }
            Dictionary<Oid, AsnType> snmpRamDict = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.4.1.2021.4");
            if (snmpRamDict.Count > 0)
            {
                if (snmpRamDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.4.5.0")) && snmpRamDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.4.11.0")))
                {
                    ramUsage = (float.Parse(snmpRamDict[new Oid("1.3.6.1.4.1.2021.4.5.0")].ToString()) / 1024) - (float.Parse(snmpRamDict[new Oid("1.3.6.1.4.1.2021.4.11.0")].ToString()) / 1024);
                }
            }
            return ramUsage;
        }
        public float GetTotalRamMemory(string ip, string communityString, int port)
        {
            float ramUsage = -1;
            SimpleSnmp snmpVerb = new SimpleSnmp(ip, port, communityString);
            if (!snmpVerb.Valid)
            {
                return ramUsage;
            }
            Dictionary<Oid, AsnType> snmpRamDict = snmpVerb.Walk(SnmpVersion.Ver2, "1.3.6.1.4.1.2021.4");
            if (snmpRamDict != null)
            {
                if (snmpRamDict.ContainsKey(new Oid("1.3.6.1.4.1.2021.4.5.0")))
                {
                    ramUsage = (float.Parse(snmpRamDict[new Oid("1.3.6.1.4.1.2021.4.5.0")].ToString()) / 1024);
                }
            }
            return ramUsage;
        }
    }
}
