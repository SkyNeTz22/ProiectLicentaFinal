using SnmpSharpNet;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProiectLicenta.MVVM.ViewModel
{
    internal class SystemDataViewModel
    {
        public List<Dictionary<string, string>> GetGeneralSystemData(string ip, string communityString, int port)
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
                Dictionary<Oid, AsnType> snmpSystemDict = snmpVerb.Walk(SnmpVersion.Ver1, "1.3.6.1.2.1.1");
                Dictionary<string, string> resultDict = new Dictionary<string, string>();
                // System data
                if (snmpSystemDict != null)
                {
                    if (snmpSystemDict.ContainsKey(new Oid("1.3.6.1.2.1.1.5.0")))
                    {
                        resultDict.Add("This is the System's name: ", snmpSystemDict[new Oid("1.3.6.1.2.1.1.5.0")].ToString());
                    }
                    else
                    {
                        resultDict.Add("Error System Monitoring #1: ", "Unable to get the System's name.");
                    }

                    if (snmpSystemDict.ContainsKey(new Oid("1.3.6.1.2.1.1.1.0")))
                    {
                        string systemInformation = snmpSystemDict[new Oid("1.3.6.1.2.1.1.1.0")].ToString();
                        string[] parts = systemInformation.Trim().Split(' ');
                        string os = parts[0];
                        string host = parts[1];
                        string kernel = parts[2] + " " + parts[3] + " " + parts[4];
                        string date = parts[5] + " " + parts[6] + " " + parts[7] + " " + parts[8] + " " + parts[9] + " " + parts[10];
                        string architecture = parts[11];
                        resultDict.Add("The System's OS is: ", os);
                        resultDict.Add("The System's host is: ", host);
                        resultDict.Add("The System's Kernel is: ", kernel);
                        resultDict.Add("The System's install date is: ", date);
                        resultDict.Add("The System's architecture is: ", architecture);
                    }
                    else
                    {
                        resultDict.Add("Error System Monitoring #2: ", "Unable to get the System's decription.");
                    }

                    if (snmpSystemDict.ContainsKey(new Oid("1.3.6.1.2.1.1.3.0")))
                    {
                        resultDict.Add("This is the System's uptime: ", snmpSystemDict[new Oid("1.3.6.1.2.1.1.3.0")].ToString());
                    }
                    else
                    {
                        resultDict.Add("Error System Monitoring #3: ", "Unable to get the System's uptime.");
                    }

                    if (snmpSystemDict.ContainsKey(new Oid("1.3.6.1.2.1.1.4.0")))
                    {
                        string contactInfo = snmpSystemDict[new Oid("1.3.6.1.2.1.1.4.0")].ToString();
                        contactInfo = contactInfo.Substring(1, contactInfo.Length - 2);
                        resultDict.Add("This is the System's contact information: ", contactInfo);
                    }
                    else
                    {
                        resultDict.Add("Error System Monitoring #4: ", "Unable to get the System's contact information.");
                    }
                    if (snmpSystemDict.ContainsKey(new Oid("1.3.6.1.2.1.1.6.0")))
                    {
                        resultDict.Add("This is the System's location: ", snmpSystemDict[new Oid("1.3.6.1.2.1.1.6.0")].ToString());
                    }
                    else
                    {
                        resultDict.Add("Error System Monitoring #5: ", "Unable to get the System's location.");
                    }
                    if (snmpSystemDict.ContainsKey(new Oid("1.3.6.1.2.1.1.2.0")))
                    {
                        resultDict.Add("This is the System's object ID: ", snmpSystemDict[new Oid("1.3.6.1.2.1.1.2.0")].ToString());
                    }
                    else
                    {
                        resultDict.Add("Error System Monitoring #6: ", "Unable to get the System's object ID.");
                    }
                }
                else
                {
                    resultDict.Add("Error System Monitoring #0: ", "Unable to get System general data.");
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
    }
}
