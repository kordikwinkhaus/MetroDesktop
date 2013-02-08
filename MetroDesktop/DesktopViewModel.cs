using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace MetroDesktop
{
    public class DesktopViewModel
    {
        private string _connstring;

        public DesktopViewModel(Dictionary<string, string> data)
        {
            this.Version = data["Version"];
            this.SmallInfoSize = 480;
            this.CommandPanelWidth = 360;
            _connstring = data["ConnectionString"]; 
            
            LoadDocsInfo();

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
        }

        private void LoadDocsInfo()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(_connstring))
                {
                    conn.Open();

                    this.Server = conn.DataSource;
                    this.Database = conn.Database;

                    string sql = @"DECLARE @d datetime
SET @d = CAST(FLOOR(CAST(GETDATE() as float)) as datetime)
SELECT COUNT(*) AS Liczba FROM oferty WHERE realizacja < @d AND do_arch=0 
UNION ALL
SELECT COUNT(*) AS Liczba FROM oferty WHERE realizacja=@d AND do_arch=0 
UNION ALL 
SELECT COUNT(*) AS Liczba FROM oferty WHERE realizacja=@d+1 AND do_arch=0";

                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    using (OleDbDataReader dr = cmd.ExecuteReader())
                    {
                        dr.Read();
                        this.OldDocs = "Počet dokumentů po termínu realizace: " + dr.GetInt32(0);
                        dr.Read();
                        this.TodayDocs = "Počet dokumentů s dnešním datem realizace: " + dr.GetInt32(0);
                        dr.Read();
                        this.TommorowDocs = "Počet dokumentů se zítřejším datem realizace: " + dr.GetInt32(0);
                    }

                    using (OleDbCommand cmd = new OleDbCommand("select db_id('magazyn')", conn))
                    {
                        this.ShowStoreButton = cmd.ExecuteScalar() != DBNull.Value;
                    }
                }
            }
            catch
            {
                this.Server = "n/a";
                this.Database = "n/a";
                this.OldDocs = "Počet dokumentů po termínu realizace: n/a";
                this.TodayDocs = "Počet dokumentů s dnešním datem realizace: n/a";
                this.TommorowDocs = "Počet dokumentů se zítřejším datem realizace: n/a";
            }
        }

        private void SetupTwoRowLayout()
        {
            this.SmallInfoSize = 230;
        }

        public bool ShowDealerCommunicationButton { get; private set; }

        public bool ShowProducerButtons { get; private set; }

        public bool ShowStoreButton { get; private set; }

        public string Version { get; private set; }

        public string Server { get; private set; }

        public string Database { get; private set; }

        public int CommandPanelWidth { get; private set; }

        public int SmallInfoSize { get; private set; }

        public string TommorowDocs { get; private set; }
        public string TodayDocs { get; private set; }
        public string OldDocs { get; private set; }
    }
}
