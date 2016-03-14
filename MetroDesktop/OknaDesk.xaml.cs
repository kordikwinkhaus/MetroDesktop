using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MetroDesktop;
using UserExtensions;

namespace WHDesktops
{
    public partial class OknaDesk : UserControl, IMyWPFControl, IMyWPFEventSink
    {
        private CommandNotification cn;
        private string _connstring;
        private DesktopViewModel _viewmodel;

        public OknaDesk()
        {
            InitializeComponent();
        }

        #region IMyWPFControl Members

        public void Init(Dictionary<string, string> data)
        {
            _viewmodel = new MetroDesktop.DesktopViewModel(data);
            _connstring = _viewmodel.ConnectionString;
            this.DataContext = _viewmodel;

            Nowa_oferta.Content = _viewmodel.GetResource("Offer").ToUpper();
            Nowe_zlecenie_produkcyjne.Content = _viewmodel.GetResource("Commission").ToUpper();
            Otworz_istniejacy_dokument.Content = _viewmodel.GetResource("ExistingDocuments", "Existing Documents").ToUpper();
            Nowe_zamowienie.Content = _viewmodel.GetResource("Order").ToUpper();
            Zamowienie_zbiorcze.Content = _viewmodel.GetResource("SummaryOrder", "Summary Order").ToUpper();
            Nowa_optymalizacja.Content = _viewmodel.GetResource("Optimalization").ToUpper();
            Wyslij_lub_odbierz_zlecenie.Content = _viewmodel.GetResource("DealersCommunication", "Dealers Communication").ToUpper();
            Magazyn.Content = _viewmodel.GetResource("StoreModule", "Store Module").ToUpper();
            Tools.Content = _viewmodel.GetResource("Tools").ToUpper();

            ipDocuments.Title = _viewmodel.GetResource("Documents").ToUpper();
            lblTomorrowDocs.Text = _viewmodel.GetResource("TomorrowDocuments", "Documents with tomorrow's production date:");
            lblTodayDocs.Text = _viewmodel.GetResource("TodayDocuments", "Documents with today's production date:");
            lblOldDocs.Text = _viewmodel.GetResource("OldDocuments", "Documents after production date:");

            ipServer.Title = _viewmodel.GetResource("Server").ToUpper();
            ipDatabase.Title = _viewmodel.GetResource("Database").ToUpper();

            lblError.Text = _viewmodel.GetResource("Error", string.Empty);
        }

        public void OnActivate()
        {
        }

        public void OnDeactivate()
        {
        }

        public void ProcessData(Dictionary<string, string> data)
        {
        }

        #endregion

        #region IMyWPFEventSink Members

        event CommandNotification IMyWPFEventSink.RoutedCommandHandler
        {
            add { cn = (CommandNotification)(Delegate.Combine(cn, value)); }
            remove { cn = (CommandNotification)(Delegate.Remove(cn, value)); }
        }

        void IMyWPFEventSink.SendNotification(string command, string type)
        {
            if (cn != null) cn.Invoke(command, type);
        }

        #endregion

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                // dostupné příkazy (argument command) jsou v souboru WPFFormView.cpp v OKNA
                ((IMyWPFEventSink)this).SendNotification(b.Name, "Event");
            }
        }

        private void Tools_Click(object sender, RoutedEventArgs e)
        {
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var ProcessInfo = new System.Diagnostics.ProcessStartInfo(path + "\\tools.exe");
            ProcessInfo.UseShellExecute = true;
            ProcessInfo.WorkingDirectory = path;
            ProcessInfo.Arguments = "\"" + _connstring + "\"";
            if (System.Environment.OSVersion.Version.Major >= 6) // Windows Vista or higher
            {
                //run as administrator
                ProcessInfo.Verb = "runas";
            }

            Process.Start(ProcessInfo);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_viewmodel == null) return;

            _viewmodel.Compact = (this.ActualHeight < _viewmodel.MinHeight || this.ActualWidth < _viewmodel.MinWidth);
        }
    }
}
