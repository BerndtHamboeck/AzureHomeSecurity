using System.Windows;
using Microsoft.Phone.Controls;
using SecureHome.WinStore;

namespace SecureHome.WinPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Refresh _refresh { get; set; }
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += Page_Loaded;


        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _refresh = new Refresh(WebView0x1, CurrentDateTextBlock);
            _refresh.Start();
        }

    }
}
