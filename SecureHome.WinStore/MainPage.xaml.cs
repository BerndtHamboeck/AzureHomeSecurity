using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SecureHome.WinStore
{
    public sealed partial class MainPage : Page
    {

        private Refresh _refresh;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += Page_Loaded;


        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _refresh = new Refresh(WebView0x1, CurrentDateTextBlock);
            _refresh.Start();
        }
    }
}
