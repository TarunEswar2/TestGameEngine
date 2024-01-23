using GameEngineEditor.gameProject;
using GameEngineEditor.TestFolder;
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
        public MainWindow()
        {
            InitializeComponent();
            Loaded += onMainWindowLoaded;//event handler to open window Browser
        }

        private void openTestWindow()
        {
            TestWindow testWindow = new TestWindow();
            testWindow.Show();
        }

        private void onMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= onMainWindowLoaded;
            openProjectBrowserDialog();
        }

        private void openProjectBrowserDialog()
        {
            var projectBrowserWindow = new projectBrowseDialog();
            if (projectBrowserWindow.ShowDialog() == false)
            {
                Application.Current.Shutdown();
            }
            else
            {

            }
        }
    }
}