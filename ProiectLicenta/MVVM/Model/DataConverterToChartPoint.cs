using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace ProiectLicenta.Model
{
    internal class DataConverterToChartPoint
    {
        public static ChartValues<ObservablePoint> ExtractUsageData(List<Dictionary<string, string>> data, int minutesLookback, int daysLookback)
        {
            var usageData = new ChartValues<ObservablePoint>();
            var currentTimestamp = DateTime.Now;
            List<string> chartsArray = new List<string>
            {
                "LiveChart",
                "DailyChart",
                "WeeklyChart",
                "MonthlyChart"
            };
            foreach (var entry in chartsArray)
            {
                if (entry == "LiveChart")
                {
                    TimestampSetter(entry, "live");
                }
                else
                {
                    TimestampSetter(entry, "");
                }
            }
            if (data != null)
            {
                foreach (var entry in data)
                {
                    foreach (var item in entry)
                    {
                        if (float.TryParse(item.Value.ToString(), out float usagePercentage))
                        {
                            if (DateTime.TryParseExact(item.Key, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timestampFromDb))
                            {
                                TimeSpan timeDifference = currentTimestamp - timestampFromDb;
                                if (daysLookback == 0)
                                {
                                    if (timeDifference.TotalMinutes <= minutesLookback)
                                    {
                                        double timestampValue = timestampFromDb.Subtract(new DateTime(2000, 1, 1)).TotalSeconds;
                                        usageData.Add(new ObservablePoint(timestampValue, usagePercentage));
                                    }
                                }
                                else
                                {
                                    if (timeDifference.TotalDays <= daysLookback)
                                    {
                                        double timestampValue = timestampFromDb.Subtract(new DateTime(2000, 1, 1)).TotalSeconds;
                                        usageData.Add(new ObservablePoint(timestampValue, usagePercentage));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return null;
            }
            return usageData;
        }

        public static void ModifyChart(Action<LiveCharts.Wpf.CartesianChart> modifyAction, string chartName)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var chart = Application.Current.MainWindow.FindName(chartName) as LiveCharts.Wpf.CartesianChart;
                modifyAction.Invoke(chart);
            });
        }

        public static void TimestampSetter(string chartName, string chartType)
        {
            // Other chart-related code that can run concurrently

            ModifyChart(chart =>
            {
                chart.AxisX.Clear(); // Clear existing axes if needed
                chart.AxisX.Add(new Axis
                {
                    LabelFormatter = value =>
                    {
                        switch (chartType)
                        {
                            case "live":
                                var timestamp = new DateTime(2000, 1, 1).AddSeconds(value);
                                string formattedValue = timestamp.ToString("HH:mm");
                                return formattedValue;
                            default:
                                timestamp = new DateTime(2000, 1, 1).AddSeconds(value);
                                formattedValue = timestamp.ToString("yyyy-MM-dd HH:mm");
                                return formattedValue;
                        }
                    }
                });
            }, chartName);
        }
    }
}
