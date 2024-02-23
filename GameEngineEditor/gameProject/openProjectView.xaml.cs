using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameEngineEditor.gameProject
{
    /// <summary>
    /// Interaction logic for openProjectView.xaml
    /// </summary>
    public partial class openProjectView : UserControl
    {
        public openProjectView()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var item = projectsListBox.ItemContainerGenerator.ContainerFromIndex(projectsListBox.SelectedIndex) as ListBoxItem;
                item?.Focus();
            };
        }

        private void onCLickOpen_Button(object sender, RoutedEventArgs e)
        {
            openSelectedProject();
        }
        private void onListBoxItem_Mouse_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            openSelectedProject();
        }

        private void openSelectedProject()
        {
            var project = openProject.open(projectsListBox.SelectedItem as ProjectData);
            bool dialogResult = false;
            var win = Window.GetWindow(this);
            if (project != null)
            {
                dialogResult = true;
                win.DataContext = project; //setting the data context
            }
            win.DialogResult = dialogResult;
            win.Close();
        }
    }
}
