using Newtonsoft.Json;
using ProiectLicenta.Model;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ProiectLicenta
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            SetupDatabase();
            InitializeComponent();
        }

        private void SetupDatabase()
        {
            // Specify the path and filename of the SQLite database file
            string databaseFile = "monitoring.db";

            // Create the SQLite database file if it doesn't exist
            if (!System.IO.File.Exists(databaseFile))
            {
                SQLiteConnection.CreateFile(databaseFile);
            }
            string connectionString = $"Data Source={databaseFile};Version=3;";
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Create a table if it doesn't exist
                    string createLiveCpuDataTableQuery = "CREATE TABLE IF NOT EXISTS CPULiveUsageData (Id INTEGER PRIMARY KEY AUTOINCREMENT, Timestamp TEXT, UsagePercentage REAL);";
                    using (SQLiteCommand command = new SQLiteCommand(createLiveCpuDataTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    string createHourlyCpuDataTableQuery = "CREATE TABLE IF NOT EXISTS CPUHourlyUsageData (Id INTEGER PRIMARY KEY AUTOINCREMENT, Timestamp TEXT, UsagePercentage REAL);";
                    using (SQLiteCommand command = new SQLiteCommand(createHourlyCpuDataTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    string createLiveRamDataTableQuery = "CREATE TABLE IF NOT EXISTS RamLiveUsageData (Id INTEGER PRIMARY KEY AUTOINCREMENT, Timestamp TEXT, Usage REAL);";
                    using (SQLiteCommand command = new SQLiteCommand(createLiveRamDataTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    string createHourlyRamDataTableQuery = "CREATE TABLE IF NOT EXISTS RamHourlyUsageData (Id INTEGER PRIMARY KEY AUTOINCREMENT, Timestamp TEXT, Usage REAL);";
                    using (SQLiteCommand command = new SQLiteCommand(createHourlyRamDataTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    string createLiveNetworkDataTableQuery = "CREATE TABLE IF NOT EXISTS NetworkLiveUsageData (Id INTEGER PRIMARY KEY AUTOINCREMENT, Timestamp TEXT, Interface TEXT, UsagePercentage REAL);";
                    using (SQLiteCommand command = new SQLiteCommand(createLiveNetworkDataTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    string createHourlyNetworkDataTableQuery = "CREATE TABLE IF NOT EXISTS NetworkHourlyUsageData (Id INTEGER PRIMARY KEY AUTOINCREMENT, Timestamp TEXT, Interface TEXT, UsagePercentage REAL);";
                    using (SQLiteCommand command = new SQLiteCommand(createHourlyNetworkDataTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    string createLiveDiskDataTableQuery = "CREATE TABLE IF NOT EXISTS DiskLiveUsageData (Id INTEGER PRIMARY KEY AUTOINCREMENT, Timestamp TEXT, Partition TEXT, TotalSpace TEXT, FreeSpace TEXT, UsedSpace TEXT);";
                    using (SQLiteCommand command = new SQLiteCommand(createLiveDiskDataTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();

                }
            }
            catch (SQLiteException)
            {
                Application.Current.Shutdown();
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PortInput.Text))
            {
                PortInput.Text = "161";
            }
            if (string.IsNullOrEmpty(MailInput.Text))
            {
                MailInput.Text = "";
            }
            if (string.IsNullOrEmpty(UserInput.Text))
            {
                UserInput.Text = "";
            }
            if (string.IsNullOrEmpty(PassInput.Password))
            {
                PassInput.Password = "";
            }
            if (string.IsNullOrEmpty(RemoteIPInput.Text))
            {
                RemoteIPInput.Text = "192.168.0.1";
            }
            if (string.IsNullOrEmpty(CommunityStringInput.Text))
            {
                CommunityStringInput.Text = "public";
            }
            ConfigDataStructure config = new ConfigDataStructure
            {
                RemoteIp = RemoteIPInput.Text,
                CommunityString = CommunityStringInput.Text,
                Port = PortInput.Text,
                Mail = MailInput.Text,
                Username = UserInput.Text,
                Password = PassInput.Password
            };
            string configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText("config.json", configJson);
        }
    }
}
