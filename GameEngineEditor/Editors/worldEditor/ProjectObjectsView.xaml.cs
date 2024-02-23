using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using GameEngineEditor.Components;
using GameEngineEditor.Editors;
using GameEngineEditor.gameProject;
using GameEngineEditor.utilities;


namespace GameEngineEditor.Editors
{
    /// <summary>
    /// Interaction logic for ProjectObjects.xaml
    /// </summary>
    public partial class ProjectObjectsView : UserControl
    {
        public ProjectObjectsView()
        {
            InitializeComponent();
        }

        private void onAddEntity_Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var vm = btn.DataContext as Scene;
            vm.addGameEntityCommand.Execute(new gameEntitiy(vm) { Name = "Empty Game Entity" });
        }
        private void onGameEntities_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            var newSelection = listBox.SelectedItems.Cast<gameEntitiy>().ToList();//list of all the selections not just the recent one
            var previousSelection = newSelection.Except(e.AddedItems.Cast<gameEntitiy>()).Concat(e.RemovedItems.Cast<gameEntitiy>()).ToList();

            Project.UndoRedo.add(new undoRedoAction(
                () => //undo
                {
                    listBox.UnselectAll();
                    previousSelection.ForEach(x => (listBox.ItemContainerGenerator.ContainerFromItem(x) as ListBoxItem).IsSelected = true);
                    
                    MSGameEntity msEntity = null;

                    if (previousSelection.Any())
                    {
                        msEntity = new MSGameEntity(previousSelection);
                    }

                    gameEntityView.Instance.DataContext = msEntity;
                },
                () => //redo 
                {
                    listBox.UnselectAll();
                    newSelection.ForEach(x => (listBox.ItemContainerGenerator.ContainerFromItem(x) as ListBoxItem).IsSelected = true);

                    MSGameEntity msEntity = null;

                    if (newSelection.Any())
                    {
                        msEntity = new MSGameEntity(newSelection);
                    }

                    gameEntityView.Instance.DataContext = msEntity;
                },
                "Selection"
                ));

            MSGameEntity msEntity = null;

            if (newSelection.Any())
            {
                msEntity = new MSGameEntity(newSelection);
            }

            gameEntityView.Instance.DataContext = msEntity;
        }
    }
}
