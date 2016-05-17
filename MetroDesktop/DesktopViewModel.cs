using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using Okna.Plugins;

namespace MetroDesktop
{
    public class DesktopViewModel : INotifyPropertyChanged
    {
        private const int PRODUCER = 0;
        private const int DEALER = 1;
        private const int PRODUCER_WITH_DEALERS = 2;
        private readonly string _connstring;
        private readonly Dictionary<string, string> _resources;
        private readonly DispatcherTimer _timer;
        private readonly string _toolsFileName;

        public DesktopViewModel(Dictionary<string, string> data)
        {
            _connstring = data["ConnectionString"];
            _resources = ResourceReader.ReadResources(data["AppPath"], data["Language"]);

            this.Server = "n/a";
            this.Database = "n/a";

            this.Version = data["Version"];
            this.SmallInfoSize = 480;
            this.CommandPanelWidth = 360;
            _toolsFileName = Path.Combine(data["AppPath"], "tools.exe");
            this.ShowToolsButton = File.Exists(_toolsFileName);
            this.ToolsCommand = new RelayCommand(Tools);
            
            LoadDocsInfo(firstRun: true);

            this.Mode = Convert.ToInt32(data["ProgramMode"]);
            int countOfButtons = 1; // open existing doc
            if (this.ShowOfferButton) countOfButtons++;
            if (this.ShowCommissionButton) countOfButtons++;
            if (this.ShowOrderButton) countOfButtons++;
            if (this.ShowAggrOrderButton) countOfButtons++;
            if (this.ShowOptimalizationButton) countOfButtons++;
            if (this.ShowDealerCommunicationButton) countOfButtons++;
            if (this.ShowStoreButton) countOfButtons++;
            if (this.ShowToolsButton) countOfButtons++;

            if (countOfButtons <= 6)
            {
                this.SmallInfoSize = 235;
            }
            if (countOfButtons <= 4)
            {
                this.CommandPanelWidth = 240;
            }
            this.MinWidth = (this.CommandPanelWidth + this.MiddleColumnWidth + 480);
            this.MinHeight = ((this.SmallInfoSize < 300) ? 240 : 360) + 40;

            _timer = new DispatcherTimer();
#if (DEBUG)
            _timer.Interval = new TimeSpan(0, 0, 5);
#else
            _timer.Interval = new TimeSpan(0, 2, 0);
#endif
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            LoadDocsInfo(firstRun: false);
        }

        internal string GetResource(string key, string defaultResource = null)
        {
            if (_resources.ContainsKey(key))
            {
                return _resources[key];
            }
            else
            {
                return defaultResource ?? key;
            }
        }

        private void LoadDocsInfo(bool firstRun = false)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(_connstring))
                {
                    conn.Open();

                    string sql = @"DECLARE @d datetime
SET @d = CAST(FLOOR(CAST(GETDATE() as float)) as datetime)
SELECT COUNT(*) AS DocsCount FROM oferty WHERE realizacja<@d AND do_arch=0 
UNION ALL
SELECT COUNT(*) AS DocsCount FROM oferty WHERE realizacja=@d AND do_arch=0 
UNION ALL 
SELECT COUNT(*) AS DocsCount FROM oferty WHERE realizacja=@d+1 AND do_arch=0";

                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    using (OleDbDataReader dr = cmd.ExecuteReader())
                    {
                        dr.Read();
                        this.OldDocs = dr.GetInt32(0);
                        dr.Read();
                        this.TodayDocs = dr.GetInt32(0);
                        dr.Read();
                        this.TomorrowDocs = dr.GetInt32(0);
                    }

                    if (firstRun)
                    {
                        LoadFirstTimeOnlyData(conn);
                    }
                }
            }
            catch { }
        }

        private void LoadFirstTimeOnlyData(OleDbConnection conn)
        {
            this.Server = conn.DataSource;
            if (this.Server == ".")
            {
                this.Server = GetResource("Local");
            }
            this.Database = conn.Database;

            string sql = @"SELECT 
ISNULL(IS_ROLEMEMBER('AUT_EDYCJA_OFERTY'), 0) AS Offer,
ISNULL(IS_ROLEMEMBER('AUT_EDYCJA_ZLECENIA'), 0) AS Commission,
ISNULL(IS_ROLEMEMBER('AUT_EDYCJA_ZAMOWIENIA'), 0) AS [Order],
ISNULL(IS_ROLEMEMBER('AUT_EDYCJA_ZAMOWIENIA_ZBIOR'), 0) AS AggrOrder,
ISNULL(IS_ROLEMEMBER('AUT_EDYCJA_OPTYMALIZACJA_STER'), 0) AS Optim,
ISNULL(IS_ROLEMEMBER('AUT_ZAMIANA_DEALERSKICH'), 0) AS Dealer,
ISNULL(IS_ROLEMEMBER('AUT_NADZORCA_BAZ'), 0) AS Supervisor,
ISNULL(IS_SRVROLEMEMBER('sysadmin'), 0) AS SysAdmin,
db_id('Magazyn') AS Store";

            using (OleDbCommand cmd = new OleDbCommand(sql, conn))
            using (OleDbDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
            {
                dr.Read();

                bool admin = Convert.ToBoolean(dr["Supervisor"]) || Convert.ToBoolean(dr["SysAdmin"]);

                this.ShowOfferButton = admin || dr.GetInt32(0) == 1;
                this.ShowCommissionButton = admin || dr.GetInt32(1) == 1;
                this.ShowOrderButton = admin || dr.GetInt32(2) == 1;
                this.ShowAggrOrderButton = admin || dr.GetInt32(3) == 1;
                this.ShowOptimalizationButton = admin || dr.GetInt32(4) == 1;
                this.ShowDealerCommunicationButton = admin || dr.GetInt32(5) == 1;
                this.ShowStoreButton = dr["Store"] != DBNull.Value;
                this.ShowToolsButton &= admin;
            }
        }

        private string _errorText = string.Empty;
        public string ErrorText
        {
            get { return _errorText; }
            set
            {
                if (_errorText != value)
                {
                    _errorText = value;
                    OnPropertyChanged(nameof(ErrorText));
                }
            }
        }

        public int Mode { get; private set; }

        public bool ShowProducerButtons { get; private set; }

        public bool ShowOfferButton { get; private set; }

        public bool ShowCommissionButton { get; private set; }

        private bool _showOrderButton;
        public bool ShowOrderButton 
        {
            get { return this.Mode != DEALER && _showOrderButton; }
            private set { _showOrderButton = value; }
        }

        private bool _showAggrOrderButton;
        public bool ShowAggrOrderButton 
        {
            get { return this.Mode != DEALER && _showAggrOrderButton; }
            private set { _showAggrOrderButton = value; }
        }

        private bool _showOptimalizationButton;
        public bool ShowOptimalizationButton 
        {
            get { return this.Mode != DEALER && _showOptimalizationButton; }
            private set { _showOptimalizationButton = value; }
        }

        private bool _showDealerCommunicationButton;
        public bool ShowDealerCommunicationButton 
        {
            get { return this.Mode != PRODUCER && _showDealerCommunicationButton; }
            private set { _showDealerCommunicationButton = value; }
        }

        private bool _showStoreButton;
        public bool ShowStoreButton 
        {
            get { return this.Mode != DEALER && _showStoreButton; }
            private set { _showStoreButton = value; }
        }

        public bool ShowToolsButton { get; private set; }

        public ICommand ToolsCommand { get; private set; }

        public string Version { get; private set; }

        public string Server { get; private set; }

        public string Database { get; private set; }

        private int _commandPanelWidth;
        public int CommandPanelWidth 
        {
            get { return this.Compact ? 240 : _commandPanelWidth; }
            private set
            {
                _commandPanelWidth = value;
                OnPropertyChanged("CommandPanelWidth");
            }
        }

        private int _smallInfoSize;
        public int SmallInfoSize 
        {
            get { return this.Compact ? 180 : _smallInfoSize; }
            private set 
            { 
                _smallInfoSize = value;
                OnPropertyChanged("SmallInfoSize");
            }
        }

        public int MiddleColumnWidth
        {
            get { return this.Compact ? 60 : 120; }
        }

        private int _tomorrowDocs;
        private int _todayDocs;
        private int _oldDocs;
        public int TomorrowDocs 
        {
            get { return _tomorrowDocs; }
            private set
            {
                _tomorrowDocs = value;
                OnPropertyChanged("TomorrowDocs");
            }
        }
        public int TodayDocs 
        {
            get { return _todayDocs; }
            private set
            {
                _todayDocs = value;
                OnPropertyChanged("TodayDocs");
            }
        }
        public int OldDocs 
        {
            get { return _oldDocs; }
            private set
            {
                _oldDocs = value;
                OnPropertyChanged("OldDocs");
            }
        }

        private bool _compact;
        public bool Compact 
        {
            get { return _compact; }
            set
            {
                if (_compact != value)
                {
                    _compact = value;
                    OnPropertyChanged("Compact");
                    OnPropertyChanged("SmallInfoSize");
                    OnPropertyChanged("CommandPanelWidth");
                    OnPropertyChanged("MiddleColumnWidth");
                }
            }
        }

        public int MinWidth { get; private set; }
        public int MinHeight { get; private set; }

        private void Tools(object param)
        {
            this.ErrorText = string.Empty;

            try
            {
                var processInfo = new ProcessStartInfo(_toolsFileName);
                processInfo.UseShellExecute = true;
                processInfo.WorkingDirectory = new FileInfo(_toolsFileName).DirectoryName;
                processInfo.Arguments = "\"" + _connstring + "\"";
                if (Environment.OSVersion.Version.Major >= 6) // Windows Vista or higher
                {
                    // run as administrator
                    processInfo.Verb = "runas";
                }

                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                this.ErrorText = ex.Message;
            }
        }

        #region INotifyPropertyChanged Members

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
