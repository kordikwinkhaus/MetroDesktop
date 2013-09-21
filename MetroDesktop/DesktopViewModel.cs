using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.OleDb;
using System.Windows.Threading;

namespace MetroDesktop
{
    public class DesktopViewModel : INotifyPropertyChanged
    {
        private readonly string _connstring;
        private readonly Dictionary<string, string> _resources;
        private readonly DispatcherTimer _timer;

        public DesktopViewModel(Dictionary<string, string> data)
        {
            _connstring = data["ConnectionString"];
            _resources = ResourceReader.ReadResources(data["AppPath"], data["Language"]);

            this.Server = "n/a";
            this.Database = "n/a";

            this.Version = data["Version"];
            this.SmallInfoSize = 480;
            this.CommandPanelWidth = 360;
            
            LoadDocsInfo(firstRun: true);

            switch (data["ProgramMode"])
            {
                case "0": // výrobce
                    this.ShowProducerButtons = true;
                    if (!this.ShowStoreButton)
                    {
                        SetupTwoRowLayout();
                    }
                    break;

                case "1": // dealer
                    this.ShowDealerCommunicationButton = true;
                    this.ShowStoreButton = false;
                    SetupTwoRowLayout();
                    this.CommandPanelWidth = 240;
                    break;

                case "2": // výrobce s dealery
                    this.ShowProducerButtons = true;
                    this.ShowDealerCommunicationButton = true;
                    break;
            }

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
                        this.Server = conn.DataSource;
                        if (this.Server == ".")
                        {
                            this.Server = GetResource("Local");
                        }
                        this.Database = conn.Database;

                        using (OleDbCommand cmd = new OleDbCommand("SELECT db_id('Magazyn')", conn))
                        {
                            this.ShowStoreButton = cmd.ExecuteScalar() != DBNull.Value;
                        }
                    }
                }
            }
            catch { }
        }

        private void SetupTwoRowLayout()
        {
            this.SmallInfoSize = 235;
        }

        public bool ShowDealerCommunicationButton { get; private set; }

        public bool ShowProducerButtons { get; private set; }

        public bool ShowStoreButton { get; private set; }

        public string Version { get; private set; }

        public string Server { get; private set; }

        public string Database { get; private set; }

        public int CommandPanelWidth { get; private set; }

        public int SmallInfoSize { get; private set; }

        private int _TomorrowDocs;
        private int _TodayDocs;
        private int _OldDocs;
        public int TomorrowDocs 
        {
            get { return _TomorrowDocs; }
            private set
            {
                _TomorrowDocs = value;
                OnPropertyChanged("TomorrowDocs");
            }
        }
        public int TodayDocs 
        {
            get { return _TodayDocs; }
            private set
            {
                _TodayDocs = value;
                OnPropertyChanged("TodayDocs");
            }
        }
        public int OldDocs 
        {
            get { return _OldDocs; }
            private set
            {
                _OldDocs = value;
                OnPropertyChanged("OldDocs");
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
