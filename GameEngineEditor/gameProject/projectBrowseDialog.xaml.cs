using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GameEngineEditor.gameProject
{
    /// <summary>
    /// Interaction logic for projectBrowseDialg.xaml
    /// </summary>
    public partial class projectBrowseDialog : Window
    {
        private readonly CubicEase _easing = new CubicEase() { EasingMode = EasingMode.EaseInOut};
        public projectBrowseDialog()
        { 
            InitializeComponent();
            openProjectButton.IsChecked = true;
            Loaded += onProjectBrowserLoaded;
        }

        private void onProjectBrowserLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= onProjectBrowserLoaded;
            if(!openProject.Projects.Any())
            {
                openProjectButton.IsEnabled = false;
                openProjectView.Visibility = Visibility.Hidden;
                onToggleButton_click(newProjectButton, new RoutedEventArgs());
            }
        }

        private void onToggleButton_click(object sender, RoutedEventArgs e)
        {
            if(sender == openProjectButton )
            {
                if(newProjectButton.IsChecked == true)
                {
                    newProjectButton.IsChecked = false;
                    AnimateToOpenProject();
                    openProjectView.IsEnabled = true;
                    newProjectView.IsEnabled = false;
                }
                openProjectButton.IsChecked = true;
            }
            else if( sender == newProjectButton ) 
            {
                if (openProjectButton.IsChecked == true)
                {
                    openProjectButton.IsChecked = false;
                    AnimateToNewProject();
                    openProjectView.IsEnabled = false;
                    newProjectView.IsEnabled = true;
                }
                newProjectButton.IsChecked = true;
            }
        }

        private void AnimateToNewProject()
        {
            var animation = new ThicknessAnimation(new Thickness(0 ,0 ,0, 0), new Thickness(-1000, 0, 0, 0), new Duration(TimeSpan.FromSeconds(0.5)));
            animation.EasingFunction = _easing;
            browserContent.BeginAnimation(MarginProperty, animation);
        }

        private void AnimateToOpenProject()
        {
            var animation = new ThicknessAnimation(new Thickness(-1000, 0, 0, 0), new Thickness(0, 0, 0, 0), new Duration(TimeSpan.FromSeconds(0.5)));
            animation.EasingFunction = _easing;
            browserContent.BeginAnimation(MarginProperty, animation);
        }
    }
}
