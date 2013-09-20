using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using UserExtensions;

namespace WHDesktops
{
    public partial class OknaDesk : UserControl, IMyWPFControl, IMyWPFEventSink
    {
        private CommandNotification cn;

        public OknaDesk()
        {
            InitializeComponent();
        }

        #region IMyWPFControl Members

        public void Init(Dictionary<string, string> data)
        {
            MetroDesktop.DesktopViewModel viewmodel = new MetroDesktop.DesktopViewModel(data);
            this.DataContext = viewmodel;

            Nowa_oferta.Tag = viewmodel.GetResource("Offer").ToUpper();
            Nowe_zlecenie_produkcyjne.Tag = viewmodel.GetResource("Commission").ToUpper();
            Otworz_istniejacy_dokument.Tag = viewmodel.GetResource("ExistingDocuments", "Existing Documents").ToUpper();
            Nowe_zamowienie.Tag = viewmodel.GetResource("Order").ToUpper();
            Zamowienie_zbiorcze.Tag = viewmodel.GetResource("SummaryOrder", "Summary Order").ToUpper();
            Nowa_optymalizacja.Tag = viewmodel.GetResource("Optimalization").ToUpper();
            Wyslij_lub_odbierz_zlecenie.Tag = viewmodel.GetResource("DealersCommunication", "Dealers Communication").ToUpper();
            Magazyn.Tag = viewmodel.GetResource("StoreModule", "Store Module").ToUpper();

            lblDocuments.Text = viewmodel.GetResource("Documents").ToUpper();
            lblTomorrowDocs.Text = viewmodel.GetResource("TomorrowDocuments", "Documents with tomorrow's production date:");
            lblTodayDocs.Text = viewmodel.GetResource("TodayDocuments", "Documents with today's production date:");
            lblOldDocs.Text = viewmodel.GetResource("OldDocuments", "Documents after production date:");

            lblServer.Text = viewmodel.GetResource("Server").ToUpper();
            lblDatabase.Text = viewmodel.GetResource("Database").ToUpper();
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
                ((IMyWPFEventSink)this).SendNotification(b.Name, "Event");
            }
        }
    }
}
