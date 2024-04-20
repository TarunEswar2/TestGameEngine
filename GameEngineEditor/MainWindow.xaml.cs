using GameEngineEditor.gameProject;
using GameEngineEditor.TestFolder;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameEngineEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string TgePath { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += onMainWindowLoaded;//event handler to open window Browser
            Closing += onMainWindowClosing;
        }

        private void onMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= onMainWindowLoaded;
            GetEnginePath();
            openProjectBrowserDialog();
        }

        private void GetEnginePath()
        {
            var enginePath = Environment.GetEnvironmentVariable("TGE_ENGINE", EnvironmentVariableTarget.User);
            if(enginePath == null || !Directory.Exists(System.IO.Path.Combine(enginePath, @"GameEngine\EngineAPI")))
            {
                var dlg = new EnginePathDialog();
                if(dlg.ShowDialog() == true)
                {
                    TgePath = dlg.TgePath;
                    Environment.SetEnvironmentVariable("TGE_ENGINE", TgePath.ToUpper(), EnvironmentVariableTarget.User);
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
            else
            {
                TgePath = enginePath;
            }
        }

        private void onMainWindowClosing(object? sender, CancelEventArgs e)
        {
            Closing -= onMainWindowClosing;
            Project.Current?.unLoad();
        }

        private void openProjectBrowserDialog()
        {
            var projectBrowserWindow = new projectBrowseDialog();
            if (projectBrowserWindow.ShowDialog() == false || projectBrowserWindow.DataContext == null)
            {
                Application.Current.Shutdown();
            }
            else
            {
                Project.Current?.unLoad();
                DataContext = projectBrowserWindow.DataContext;
            }
        }
    }
}