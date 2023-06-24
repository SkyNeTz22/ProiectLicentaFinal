using LiveCharts;
using LiveCharts.Defaults;
using MailKit.Net.Smtp;
using MimeKit;
using ProiectLicenta.Core;
using ProiectLicenta.Model;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ProiectLicenta.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        // relay command definitions
        public RelayCommand CpuDataViewCommand { get; set; }
        public RelayCommand DiskDataViewCommand { get; set; }
        public RelayCommand LoadConfigCommand { get; set; }
        public RelayCommand NetworkDataViewCommand { get; set; }
        public RelayCommand RamDataViewCommand { get; set; }
        public RelayCommand SystemDataViewCommand { get; set; }
        public ConfigLoader CfgLoader { get; set; }
        public CPUDataViewModel CpuDataVM { get; set; }
        public DiskDataViewModel DiskDataVM { get; set; }
        public SystemDataViewModel GeneralSysDataVM { get; set; }
        public NetworkDataViewModel NetworkDataVM { get; set; }
        public RAMDataViewModel RAMDataVM { get; set; }
        private DataInsertionThreads dataInsertionThread;

        // object that populates the text information of each window
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        // cartesian charts initializations
        private ChartValues<ObservablePoint> _liveUsageChart;
        public ChartValues<ObservablePoint> LiveUsageChart
        {
            get { return _liveUsageChart; }
            set
            {
                _liveUsageChart = value;
                OnPropertyChanged();
            }
        }
        public ChartValues<ObservablePoint> DailyUsageChart
        {
            get { return _dailyUsageChart; }
            set
            {
                _dailyUsageChart = value;
                OnPropertyChanged();
            }
        }
        private ChartValues<ObservablePoint> _weeklyUsageChart;
        public ChartValues<ObservablePoint> WeeklyUsageChart
        {
            get { return _weeklyUsageChart; }
            set
            {
                _weeklyUsageChart = value;
                OnPropertyChanged();
            }
        }
        private ChartValues<ObservablePoint> _monthlyUsageChart;
        public ChartValues<ObservablePoint> MonthlyUsageChart
        {
            get { return _monthlyUsageChart; }
            set
            {
                _monthlyUsageChart = value;
                OnPropertyChanged();
            }
        }

        // piecharts initializations
        private ChartValues<ObservableValue> _diskDataFreeSpacePiechart;
        public ChartValues<ObservableValue> DiskDataFreeSpacePiechart
        {
            get { return _diskDataFreeSpacePiechart; }
            set
            {
                _diskDataFreeSpacePiechart = value;
                OnPropertyChanged();
            }
        }
        private ChartValues<ObservableValue> _diskDataUsedSpacePiechart;
        public ChartValues<ObservableValue> DiskDataUsedSpacePiechart
        {
            get { return _diskDataUsedSpacePiechart; }
            set
            {
                _diskDataUsedSpacePiechart = value;
                OnPropertyChanged();
            }
        }
        private ChartValues<ObservablePoint> _dailyUsageChart;

        private ObservableCollection<string> _dropdownItems;
        public ObservableCollection<string> DropdownItems
        {
            get { return _dropdownItems; }
            set
            {
                _dropdownItems = value;
                OnPropertyChanged();
            }
        }

        private string _errorLabelText;
        public string ErrorLabelText
        {
            get { return _errorLabelText; }
            set
            {
                _errorLabelText = value;
                OnPropertyChanged(nameof(ErrorLabelText));
            }
        }


        // elements visibility
        private bool areGraphsVisible;
        private bool arePiechartsVisible;
        private bool isLabelVisible;
        public bool AreGraphsVisible
        {
            get { return areGraphsVisible; }
            set
            {
                areGraphsVisible = value;
                OnPropertyChanged(nameof(AreGraphsVisible));
            }
        }
        public bool IsLabelVisible
        {
            get { return isLabelVisible; }
            set
            {
                isLabelVisible = value;
                OnPropertyChanged(nameof(IsLabelVisible));
            }
        }
        public bool ArePiechartsVisible
        {
            get { return arePiechartsVisible; }
            set
            {
                arePiechartsVisible = value;
                OnPropertyChanged(nameof(ArePiechartsVisible));
            }
        }

        // combo box and it's logic
        private bool isComboBoxVisible;
        public bool IsComboBoxVisible
        {
            get { return isComboBoxVisible; }
            set
            {
                isComboBoxVisible = value;
                OnPropertyChanged(nameof(IsComboBoxVisible));
            }
        }

        // selected network interface
        private string _selectedInterface;
        public string SelectedInterface
        {
            get { return _selectedInterface; }
            set
            {
                _selectedInterface = value;
                OnPropertyChanged();
            }
        }

        // delay variables and their values
        int liveUpdateDelay = 12600;     // Delay for live chart updates (5 seconds)
        int hourlyUpdateDelay = 3600000; // Delay for daily chart updates (1 hour)
        int delayVar = 7533;             // random picked value to try and avoid overlapping..

        private static CancellationTokenSource _cancellationTokenSource;
        public MainViewModel()
        {
            // initialization of objects based on each class
            GeneralSysDataVM = new SystemDataViewModel();
            CpuDataVM = new CPUDataViewModel();
            RAMDataVM = new RAMDataViewModel();
            DiskDataVM = new DiskDataViewModel();
            NetworkDataVM = new NetworkDataViewModel();
            CfgLoader = new ConfigLoader();

            // default values for each config variable
            string remoteIp = "";
            string comString = "";
            string recipient = "";
            int snmpPort = 0;
            string username = "";
            string password = "";
            TimeSpan timeSpan5Minutes = TimeSpan.FromMinutes(5);
            TimeSpan timeSpan1Day = TimeSpan.FromDays(1);
            TimeSpan timeSpan7Days = TimeSpan.FromDays(7);
            TimeSpan timeSpan30Days = TimeSpan.FromDays(30);
            LoadConfigCommand = new RelayCommand(o =>
            {
                var config = CfgLoader.LoadConfig();
                remoteIp = config.RemoteIp;
                comString = config.CommunityString;
                snmpPort = int.Parse(config.Port);
                recipient = config.Mail;
                username = config.Username;
                password = config.Password;
                CurrentView = GeneralSysDataVM.GetGeneralSystemData(remoteIp, comString, snmpPort);
                IsComboBoxVisible = false;
                AreGraphsVisible = false;
                ArePiechartsVisible = false;

                // Start the data insertion thread after the config is loaded
                Task.Run(() =>
                {
                    Debug.WriteLine("Threads started!");
                    dataInsertionThread = new DataInsertionThreads();
                    dataInsertionThread.Start();
                });
            },
            o => true);
            CpuDataViewCommand = new RelayCommand(async o =>
            {
                CurrentView = CpuDataVM.GetCpuData(remoteIp, comString, snmpPort);
                IsComboBoxVisible = false;
                AreGraphsVisible = true;
                ArePiechartsVisible = false;
                IsLabelVisible = false;
                // Create a new CancellationTokenSource only if it hasn't been created yet or it has been canceled
                _cancellationTokenSource?.Cancel();
                if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                }

                CancellationToken cancellationToken = _cancellationTokenSource.Token;

                DateTime lastLiveUpdate = DateTime.MinValue;
                DateTime lastDailyUpdate = DateTime.MinValue;
                DateTime lastWeeklyUpdate = DateTime.MinValue;
                DateTime lastMonthlyUpdate = DateTime.MinValue;

                bool initialUpdateDaily = true;
                bool initialUpdateWeekly = true;
                bool initialUpdateMonthly = true;
                // Start the tasks for updating the graphs
                Task liveUpdateTask = Task.Run(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        LiveUsageChart = new ChartValues<ObservablePoint>();
                        var liveCpuUsageData = DataSelectorFromDB.GetTotalUsageData("CPULiveUsageData", "", "", false);
                        var liveUsageData = DataConverterToChartPoint.ExtractUsageData(liveCpuUsageData, 5, 0);
                        if ((DateTime.Now - lastLiveUpdate).TotalMilliseconds >= liveUpdateDelay)
                        {
                            if (liveUsageData != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    // Add new data points to the LiveUsageChart
                                    foreach (var dataPoint in liveUsageData)
                                    {
                                        LiveUsageChart.Add(dataPoint);
                                    }
                                });
                                lastLiveUpdate = DateTime.Now;
                            }
                        }

                        await Task.Delay(liveUpdateDelay);
                    }
                });
                Task dailyUpdateTask = Task.Run(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var latestCpuUsageValue = DataSelectorFromDB.GetLastUsageValue("CPULiveUsageData", "", false);
                        if (float.Parse(latestCpuUsageValue) > 80)
                        {
                            var body = new TextPart("plain")
                            {
                                Text = $"The current CPU usage ({latestCpuUsageValue}) exceeds 80%."
                            };
                            IsLabelVisible = true;
                            ErrorLabelText = $"The current CPU usage ({latestCpuUsageValue}%) exceeds 80 %.";
                            SendMail(recipient, "CPU Usage Alert", body);
                        }
                        DailyUsageChart = new ChartValues<ObservablePoint>();
                        var dailyCpuUsageData = DataSelectorFromDB.GetTotalUsageData("CPUHourlyUsageData", "", "", false);

                        if (initialUpdateDaily || (DateTime.Now - lastDailyUpdate).TotalMinutes >= 30)
                        {
                            var dailyUsageData = DataConverterToChartPoint.ExtractUsageData(dailyCpuUsageData, 0, 1);

                            if (dailyUsageData != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    foreach (var dataPoint in dailyUsageData)
                                    {
                                        DailyUsageChart.Add(dataPoint);
                                    }
                                });

                                lastDailyUpdate = DateTime.Now;
                                initialUpdateDaily = false;
                            }
                        }

                        await Task.Delay(TimeSpan.FromMinutes(30));
                    }
                });
                Task weeklyUpdateTask = Task.Run(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        WeeklyUsageChart = new ChartValues<ObservablePoint>();
                        var weeklyCpuUsageData = DataSelectorFromDB.GetTotalUsageData("CPUHourlyUsageData", "", "", false);

                        if (initialUpdateWeekly || (DateTime.Now - lastWeeklyUpdate).TotalMinutes >= 30)
                        {
                            var weeklyUsageData = DataConverterToChartPoint.ExtractUsageData(weeklyCpuUsageData, 0, 7);

                            if (weeklyUsageData != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    foreach (var dataPoint in weeklyUsageData)
                                    {
                                        WeeklyUsageChart.Add(dataPoint);
                                    }
                                });

                                lastWeeklyUpdate = DateTime.Now;
                                initialUpdateWeekly = false;
                            }
                        }

                        await Task.Delay(TimeSpan.FromMinutes(30));
                    }
                });
                MonthlyUsageChart = new ChartValues<ObservablePoint>();

                Task monthlyUpdateTask = Task.Run(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var monthlyCpuUsageData = DataSelectorFromDB.GetTotalUsageData("CPUHourlyUsageData", "", "", false);
                        if (initialUpdateMonthly || (DateTime.Now - lastMonthlyUpdate).TotalMinutes >= 30)
                        {
                            var monthlyUsageData = DataConverterToChartPoint.ExtractUsageData(monthlyCpuUsageData, 0, 30);

                            if (monthlyUsageData != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    foreach (var dataPoint in monthlyUsageData)
                                    {
                                        MonthlyUsageChart.Add(dataPoint);
                                    }
                                });

                                lastMonthlyUpdate = DateTime.Now;
                                initialUpdateMonthly = false;
                            }
                        }

                        await Task.Delay(TimeSpan.FromMinutes(30));
                    }
                });

                await Task.WhenAll(liveUpdateTask, dailyUpdateTask, weeklyUpdateTask, monthlyUpdateTask);
            }, o => true);
            DiskDataViewCommand = new RelayCommand(async o =>
            {
                var sentMail = false;
                var counter = 0;
                bool partitionsNotNull = false;
                _cancellationTokenSource?.Cancel();
                CurrentView = DiskDataVM.GetDiskData(remoteIp, username, password);
                var partitions = DiskDataVM.GetPartitions(remoteIp, username, password);
                if (partitions != null)
                {
                    DropdownItems = new ObservableCollection<string>
                    {
                        partitions["firstPartition"],
                        partitions["secondPartition"]
                    };
                    partitionsNotNull = true;

                }
                IsComboBoxVisible = true;
                AreGraphsVisible = false;
                ArePiechartsVisible = false;
                IsLabelVisible = false;
                // Create a new CancellationTokenSource only if it hasn't been created yet or it has been canceled
                if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                }
                CancellationToken cancellationToken = _cancellationTokenSource.Token;
                if (partitionsNotNull)
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        if (sentMail == true)
                        {
                            counter++;
                        }
                        if (SelectedInterface == partitions["firstPartition"])
                        {
                            var diskData = DataSelectorFromDB.GetDiskData(partitions["firstPartition"]);
                            var freeSpace = double.Parse(diskData["freeSpace"]);
                            var usedSpace = double.Parse(diskData["usedSpace"]);
                            if (!sentMail || counter == 1800)
                            {
                                sentMail = false;

                                if (usedSpace > (freeSpace * 0.9) && !sentMail)
                                {
                                    var body = new TextPart("plain")
                                    {
                                        Text = $"The current disk space usage exceeds 90% of {freeSpace + usedSpace} bytes."
                                    };
                                    SendMail(recipient, "Disk Space Usage Alert", body);
                                    IsLabelVisible = true;
                                    ErrorLabelText = $"The current disk space usage exceeds 90% of {freeSpace + usedSpace} bytes.";
                                    sentMail = true;

                                }
                            }
                            // Create the data points for the pie chart
                            var pieChartData = new ChartValues<ObservableValue>
                        {
                            new ObservableValue(freeSpace),
                            new ObservableValue(usedSpace)
                        };
                            // Create the data points for the pie chart
                            var pieChartFreeSpace = new ChartValues<ObservableValue>
                        {
                            new ObservableValue(freeSpace),
                        };
                            var pieChartUsedSpace = new ChartValues<ObservableValue>
                        {
                            new ObservableValue(usedSpace),
                        };
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                DiskDataFreeSpacePiechart = pieChartFreeSpace;
                                DiskDataUsedSpacePiechart = pieChartUsedSpace;
                            });
                            ArePiechartsVisible = true;
                        }
                        if (SelectedInterface == partitions["secondPartition"])
                        {
                            var diskData = DataSelectorFromDB.GetDiskData(partitions["secondPartition"]);
                            var freeSpace = double.Parse(diskData["freeSpace"]);
                            var usedSpace = double.Parse(diskData["usedSpace"]);
                            if (!sentMail || counter == 1800)
                            {
                                sentMail = false;

                                if (usedSpace > (freeSpace * 0.9) && !sentMail)
                                {
                                    var body = new TextPart("plain")
                                    {
                                        Text = $"The current disk space usage exceeds 90% of {freeSpace + usedSpace} bytes."
                                    };
                                    SendMail(recipient, "Disk Space Usage Alert", body);
                                    IsLabelVisible = true;
                                    ErrorLabelText = $"The current disk space usage exceeds 90% of {freeSpace + usedSpace} bytes.";
                                    sentMail = true;

                                }
                            }
                            // Create the data points for the pie chart
                            var pieChartData = new ChartValues<ObservableValue>
                        {
                            new ObservableValue(freeSpace),
                            new ObservableValue(usedSpace)
                        };
                            // Create the data points for the pie chart
                            var pieChartFreeSpace = new ChartValues<ObservableValue>
                        {
                            new ObservableValue(freeSpace),
                        };
                            var pieChartUsedSpace = new ChartValues<ObservableValue>
                        {
                            new ObservableValue(usedSpace),
                        };
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                DiskDataFreeSpacePiechart = pieChartFreeSpace;
                                DiskDataUsedSpacePiechart = pieChartUsedSpace;
                            });
                            ArePiechartsVisible = true;
                        }
                        // Delay between updates
                        await Task.Delay(delayVar);
                    }
                }
            },
            o => true);
            NetworkDataViewCommand = new RelayCommand(async o =>
            {
                _cancellationTokenSource?.Cancel();
                CurrentView = await NetworkDataVM.GetNetworkDataAsync(remoteIp, comString, snmpPort);
                var interfaces = NetworkDataVM.GetInterfaces(remoteIp, comString, snmpPort);
                ArePiechartsVisible = false;
                if (interfaces != null)
                {
                    DropdownItems = new ObservableCollection<string>
                    {
                        interfaces["firstInterface"],
                        interfaces["secondInterface"]
                    };
                    IsComboBoxVisible = true;
                    AreGraphsVisible = true;
                    IsLabelVisible = false;
                    // Create a new CancellationTokenSource only if it hasn't been created yet or it has been canceled
                    if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                    {
                        _cancellationTokenSource?.Dispose();
                        _cancellationTokenSource = new CancellationTokenSource();
                    }
                    CancellationToken cancellationToken = _cancellationTokenSource.Token;

                    LiveUsageChart = new ChartValues<ObservablePoint>();
                    DailyUsageChart = new ChartValues<ObservablePoint>();
                    WeeklyUsageChart = new ChartValues<ObservablePoint>();
                    MonthlyUsageChart = new ChartValues<ObservablePoint>();

                    DateTime lastLiveUpdate = DateTime.MinValue;
                    DateTime lastDailyUpdate = DateTime.MinValue;
                    DateTime lastWeeklyUpdate = DateTime.MinValue;
                    DateTime lastMonthlyUpdate = DateTime.MinValue;

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        if (SelectedInterface == interfaces["firstInterface"])
                        {
                            var liveNetworkUsageData1 = DataSelectorFromDB.GetTotalUsageData("NetworkLiveUsageData", interfaces["firstInterface"], "", false);
                            var dailyNetworkUsageData1 = DataSelectorFromDB.GetTotalUsageData("NetworkHourlyUsageData", interfaces["firstInterface"], "", false);
                            var weeklyNetworkUsageData1 = DataSelectorFromDB.GetTotalUsageData("NetworkHourlyUsageData", interfaces["firstInterface"], "", false);
                            var monthlyNetworkUsageData1 = DataSelectorFromDB.GetTotalUsageData("NetworkHourlyUsageData", interfaces["firstInterface"], "", false);
                            var liveUsageData = DataConverterToChartPoint.ExtractUsageData(liveNetworkUsageData1, 5, 0);
                            if ((DateTime.Now - lastLiveUpdate).TotalMilliseconds >= liveUpdateDelay)
                            {
                                if (liveUsageData != null)
                                {

                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        LiveUsageChart = liveUsageData;
                                    });
                                    lastLiveUpdate = DateTime.Now;
                                }
                            }
                            // Update the DailyUsageChart if 1 hour has passed since the last update
                            if ((DateTime.Now - lastDailyUpdate).TotalMilliseconds >= hourlyUpdateDelay)
                            {
                                var latestUsageFirstInterface = DataSelectorFromDB.GetLastUsageValue("NetworkLiveUsageData", interfaces["firstInterface"], false);
                                var latestUsageSecondInterface = DataSelectorFromDB.GetLastUsageValue("NetworkLiveUsageData", interfaces["secondInterface"], false);
                                if (float.Parse(latestUsageFirstInterface) > 80)
                                {
                                    var body = new TextPart("plain")
                                    {
                                        Text = $"The current Network bandwidth ussage ({latestUsageFirstInterface}%) exceeds 80%."
                                    };
                                    SendMail(recipient, "Network Bandwidth Usage Alert", body);
                                    IsLabelVisible = true;
                                    ErrorLabelText = $"The current Network bandwidth ussage ({latestUsageFirstInterface}%) exceeds 80%.";

                                }
                                if (float.Parse(latestUsageSecondInterface) > 80)
                                {
                                    var body = new TextPart("plain")
                                    {
                                        Text = $"The current Network bandwidth ussage ({latestUsageSecondInterface} MB) exceeds 80%."
                                    };
                                    SendMail(recipient, "Network Bandwidth Usage Alert", body);
                                    IsLabelVisible = true;
                                    ErrorLabelText = $"The current Network bandwidth ussage ({latestUsageSecondInterface} MB) exceeds 80%.";
                                }
                                var dailyUsageData = DataConverterToChartPoint.ExtractUsageData(dailyNetworkUsageData1, 0, 1);
                                if (dailyUsageData != null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        DailyUsageChart = dailyUsageData;
                                    });
                                    lastDailyUpdate = DateTime.Now;
                                }
                            }
                            if ((DateTime.Now - lastWeeklyUpdate).TotalMilliseconds >= hourlyUpdateDelay)
                            {
                                var weeklyUsageData = DataConverterToChartPoint.ExtractUsageData(weeklyNetworkUsageData1, 0, 7);
                                if (weeklyUsageData != null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        WeeklyUsageChart = weeklyUsageData;
                                    });
                                    lastWeeklyUpdate = DateTime.Now;
                                }
                            }

                            if ((DateTime.Now - lastMonthlyUpdate).TotalMilliseconds >= hourlyUpdateDelay)
                            {
                                var monthlyUsageData = DataConverterToChartPoint.ExtractUsageData(monthlyNetworkUsageData1, 0, 30);
                                if (monthlyUsageData != null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        MonthlyUsageChart = monthlyUsageData;
                                    });
                                    lastMonthlyUpdate = DateTime.Now;
                                }
                            }
                        }
                        if (SelectedInterface == interfaces["secondInterface"])
                        {
                            var liveNetworkUsageData2 = DataSelectorFromDB.GetTotalUsageData("NetworkLiveUsageData", interfaces["secondInterface"], "", false);
                            var dailyNetworkUsageData2 = DataSelectorFromDB.GetTotalUsageData("NetworkHourlyUsageData", interfaces["secondInterface"], "", false);
                            var weeklyNetworkUsageData2 = DataSelectorFromDB.GetTotalUsageData("NetworkHourlyUsageData", interfaces["secondInterface"], "", false);
                            var monthlyNetworkUsageData2 = DataSelectorFromDB.GetTotalUsageData("NetworkHourlyUsageData", interfaces["secondInterface"], "", false);
                            var liveUsageData = DataConverterToChartPoint.ExtractUsageData(liveNetworkUsageData2, 5, 0);
                            if ((DateTime.Now - lastLiveUpdate).TotalMilliseconds >= liveUpdateDelay)
                            {
                                if (liveUsageData != null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        LiveUsageChart = liveUsageData;
                                    });
                                    lastLiveUpdate = DateTime.Now;
                                }
                            }
                            // Update the DailyUsageChart if 1 hour has passed since the last update
                            if ((DateTime.Now - lastDailyUpdate).TotalMilliseconds >= hourlyUpdateDelay)
                            {
                                var latestUsageFirstInterface = DataSelectorFromDB.GetLastUsageValue("NetworkLiveUsageData", interfaces["firstInterface"], false);
                                var latestUsageSecondInterface = DataSelectorFromDB.GetLastUsageValue("NetworkLiveUsageData", interfaces["secondInterface"], false);
                                if (float.Parse(latestUsageFirstInterface) > 80)
                                {
                                    var body = new TextPart("plain")
                                    {
                                        Text = $"The current Network bandwidth ussage ({latestUsageFirstInterface}%) exceeds 80%."
                                    };
                                    SendMail(recipient, "Network Bandwidth Usage Alert", body);
                                }
                                if (float.Parse(latestUsageSecondInterface) > 10)
                                {
                                    var body = new TextPart("plain")
                                    {
                                        Text = $"The current Network bandwidth ussage ({latestUsageSecondInterface}%) exceeds 80%."
                                    };
                                    SendMail(recipient, "Network Bandwidth Usage Alert", body);
                                }
                                var dailyUsageData = DataConverterToChartPoint.ExtractUsageData(dailyNetworkUsageData2, 0, 1);
                                if (dailyUsageData != null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        DailyUsageChart = dailyUsageData;
                                    });
                                    lastDailyUpdate = DateTime.Now;
                                }
                            }
                            if ((DateTime.Now - lastWeeklyUpdate).TotalMilliseconds >= hourlyUpdateDelay)
                            {
                                var weeklyUsageData = DataConverterToChartPoint.ExtractUsageData(weeklyNetworkUsageData2, 0, 7);
                                if (weeklyUsageData != null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        WeeklyUsageChart = weeklyUsageData;
                                    });
                                    lastWeeklyUpdate = DateTime.Now;
                                }
                            }

                            if ((DateTime.Now - lastMonthlyUpdate).TotalMilliseconds >= hourlyUpdateDelay)
                            {
                                var monthlyUsageData = DataConverterToChartPoint.ExtractUsageData(monthlyNetworkUsageData2, 0, 30);
                                if (monthlyUsageData != null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        MonthlyUsageChart = monthlyUsageData;
                                    });
                                    lastMonthlyUpdate = DateTime.Now;
                                }
                            }
                        }

                        await Task.Delay(delayVar);
                    }
                }
            },
            o => true);
            RamDataViewCommand = new RelayCommand(async o =>
            {
                _cancellationTokenSource?.Cancel();
                CurrentView = RAMDataVM.GetRamData(remoteIp, comString, snmpPort);
                IsComboBoxVisible = false;
                AreGraphsVisible = true;
                ArePiechartsVisible = false;
                IsLabelVisible = false;
                // Create a new CancellationTokenSource only if it hasn't been created yet or it has been canceled
                if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                }
                CancellationToken cancellationToken = _cancellationTokenSource.Token;

                DateTime lastLiveUpdate = DateTime.MinValue;
                DateTime lastDailyUpdate = DateTime.MinValue;
                DateTime lastWeeklyUpdate = DateTime.MinValue;
                DateTime lastMonthlyUpdate = DateTime.MinValue;

                while (!cancellationToken.IsCancellationRequested)
                {
                    var liveRamUsageData = DataSelectorFromDB.GetTotalUsageData("RamLiveUsageData", "", "", true);
                    var dailyRamUsageData = DataSelectorFromDB.GetTotalUsageData("RamHourlyUsageData", "", "", true);
                    var weeklyRamUsageData = DataSelectorFromDB.GetTotalUsageData("RamHourlyUsageData", "", "", true);
                    var monthlyRamUsageData = DataSelectorFromDB.GetTotalUsageData("RamHourlyUsageData", "", "", true);

                    // Update the LiveUsageChart if the data is available
                    var liveUsageData = DataConverterToChartPoint.ExtractUsageData(liveRamUsageData, 5, 0);

                    if ((DateTime.Now - lastLiveUpdate).TotalMilliseconds >= liveUpdateDelay)
                    {
                        if (liveUsageData != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                LiveUsageChart = liveUsageData;
                            });
                            lastLiveUpdate = DateTime.Now;
                        }
                    }
                    if ((DateTime.Now - lastDailyUpdate).TotalMilliseconds >= hourlyUpdateDelay)
                    {
                        var totalRamMemory = RAMDataVM.GetTotalRamMemory(remoteIp, comString, snmpPort);

                        var latestRamUsageValue = DataSelectorFromDB.GetLastUsageValue("RamLiveUsageData", "", true);
                        if (float.Parse(latestRamUsageValue) > (totalRamMemory * 0.8))
                        {
                            var body = new TextPart("plain")
                            {
                                Text = $"The current RAM usage ({latestRamUsageValue} MB) exceeds 80%."
                            };
                            SendMail(recipient, "RAM Usage Alert", body);
                            IsLabelVisible = true;
                            ErrorLabelText = $"The current RAM usage ({latestRamUsageValue} MB) exceeds 80%.";
                        }
                        var dailyUsageData = DataConverterToChartPoint.ExtractUsageData(dailyRamUsageData, 0, 1);
                        if (dailyUsageData != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                DailyUsageChart = dailyUsageData;
                            });
                            lastDailyUpdate = DateTime.Now;
                        }
                    }
                    if ((DateTime.Now - lastWeeklyUpdate).TotalMilliseconds >= hourlyUpdateDelay)
                    {
                        var weeklyUsageData = DataConverterToChartPoint.ExtractUsageData(weeklyRamUsageData, 0, 7);
                        if (weeklyUsageData != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                WeeklyUsageChart = weeklyUsageData;
                            });
                            lastWeeklyUpdate = DateTime.Now;
                        }
                    }

                    if ((DateTime.Now - lastMonthlyUpdate).TotalMilliseconds >= hourlyUpdateDelay)
                    {
                        var monthlyUsageData = DataConverterToChartPoint.ExtractUsageData(monthlyRamUsageData, 0, 30);
                        if (monthlyUsageData != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MonthlyUsageChart = monthlyUsageData;
                            });
                            lastMonthlyUpdate = DateTime.Now;
                        }
                    }

                    // Delay between updates
                    await Task.Delay(delayVar);
                }
            },
            o => true);
            SystemDataViewCommand = new RelayCommand(o =>
            {
                CurrentView = GeneralSysDataVM.GetGeneralSystemData(remoteIp, comString, snmpPort);
                IsComboBoxVisible = false;
                AreGraphsVisible = false;
                ArePiechartsVisible = false;
            },
            o => true);
        }

        private void SendMail(string recipient, string subject, TextPart body)
        {
            if (string.IsNullOrEmpty(recipient))
            {
                return;
            }
            try
            {
                // Sender and recipient email addresses
                string senderEmail = "monitoringSender01@gmail.com";

                // Create the email message
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sender", senderEmail));
                message.To.Add(new MailboxAddress("Recipient", recipient));
                message.Subject = subject;
                message.Body = body;

                // Create the SMTP client
                using (var client = new SmtpClient())
                {
                    // Connect to the local SMTP server
                    client.Connect("smtp.gmail.com", 465, true);
                    client.Authenticate("monitoringSender01@gmail.com", "abbgiuirpanqqrxh");
                    // Send the email
                    client.Send(message);
                    // Disconnect from the server
                    client.Disconnect(true);
                }
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}