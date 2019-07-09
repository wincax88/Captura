using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Captura.ViewModels;
using Sentry;

namespace Captura
{
    public partial class MainWindow
    {
        public static MainWindow Instance { get; private set; }

        readonly MainWindowHelper _helper;

        public MainWindow()
        {
            using (SentrySdk.Init("https://88124b0c389141c7907e11ee0f76a8b8@sentry.io/1500124"))
            {
                try
                {
                    Instance = this;

                    InitializeComponent();

                    _helper = ServiceProvider.Get<MainWindowHelper>();

                    _helper.MainViewModel.Init(!App.CmdOptions.NoPersist, !App.CmdOptions.Reset);

                    _helper.HotkeySetup.Setup();

                    _helper.TimerModel.Init();

                    Loaded += (Sender, Args) =>
                    {
                        RepositionWindowIfOutside();

                        ServiceProvider.Get<WebcamPage>().SetupPreview();

                        _helper.HotkeySetup.ShowUnregistered();
                    };

                    if (App.CmdOptions.Tray || _helper.Settings.Tray.MinToTrayOnStartup)
                        Hide();

                    Closing += (Sender, Args) =>
                    {
                        if (!TryExit())
                            Args.Cancel = true;
                    };
                }
                catch (System.Exception e)
                {
                    SentrySdk.CaptureException(e);
                }
            }
        }

        void RepositionWindowIfOutside()
        {
            // Window dimensions taking care of DPI
            var rect = new RectangleF((float) Left,
                (float) Top,
                (float) ActualWidth,
                (float) ActualHeight).ApplyDpi();
            
            if (!Screen.AllScreens.Any(M => M.Bounds.Contains(rect)))
            {
                Left = 50;
                Top = 50;
            }
        }

        void Grid_PreviewMouseLeftButtonDown(object Sender, MouseButtonEventArgs Args)
        {
            DragMove();

            Args.Handled = true;
        }

        void MinButton_Click(object Sender, RoutedEventArgs Args) => SystemCommands.MinimizeWindow(this);

        void CloseButton_Click(object Sender, RoutedEventArgs Args)
        {
            if (_helper.Settings.Tray.MinToTrayOnClose)
            {
                Hide();
            }
            else Close();
        }

        void SystemTray_TrayMouseDoubleClick(object Sender, RoutedEventArgs Args)
        {
            if (Visibility == Visibility.Visible)
            {
                Hide();
            }
            else this.ShowAndFocus();
        }

        bool TryExit()
        {
            if (!_helper.RecordingViewModel.CanExit())
                return false;

            ServiceProvider.Dispose();

            return true;
        }

        void MenuExit_Click(object Sender, RoutedEventArgs Args) => Close();

        void HideButton_Click(object Sender, RoutedEventArgs Args) => Hide();

        void ShowMainWindow(object Sender, RoutedEventArgs E) => this.ShowAndFocus();
    }
}
