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
    /// Interaction logic for newProjectView.xaml
    /// </summary>
    public partial class newProjectView : UserControl
    {
        public newProjectView()
        {
            InitializeComponent();
        }

        private void onCreateButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as newProject;
            var projectPath = vm.createProject(templateListBox.SelectedItem as projectTemplate);
            bool dialogResult = false;
            var win = Window.GetWindow(this);
            if (!string.IsNullOrEmpty(projectPath))
            {
                dialogResult = true;
                var project = openProject.open(new ProjectData() { ProjectName = vm.ProjectName, ProjectPath = projectPath });
                win.DataContext = project; //setting the data context
            }
            win.DialogResult = dialogResult;
            win.Close();
        }
    }
}
