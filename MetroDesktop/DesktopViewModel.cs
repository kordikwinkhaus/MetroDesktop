using System.Collections.Generic;

namespace MetroDesktop
{
    public class DesktopViewModel
    {
        public DesktopViewModel(Dictionary<string, string> data)
        {
            this.Version = data["Version"];

            switch (data["ProgramMode"])
            {
                case "0":
                    this.ShowProducerButtons = true;
                    break;

                case "1":
                    this.ShowDealerCommunicationButton = true;
                    break;

                case "2":
                    this.ShowProducerButtons = true;
                    this.ShowDealerCommunicationButton = true;
                    break;
            }
        }

        public bool ShowDealerCommunicationButton { get; private set; }

        public bool ShowProducerButtons { get; private set; }

        public string Version { get; private set; }
    }
}
