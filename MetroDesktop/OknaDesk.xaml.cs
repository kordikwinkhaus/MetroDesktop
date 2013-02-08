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
