using GameEngineEditor.Components;
using GameEngineEditor.gameProject;
using GameEngineEditor.utilities;
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
using static System.Net.Mime.MediaTypeNames;

namespace GameEngineEditor.Editors
{
    /// <summary>
    /// Interaction logic for gameEntityView.xaml
    /// </summary>
    public partial class gameEntityView : UserControl
    {

        private Action _undoAction;
        private Action _redoAction;
        private string _propertyName;
        public static gameEntityView Instance { get; private set; }
        public gameEntityView()
        {
            DataContext = null;
            Instance = this;
            DataContextChanged += (_, __) =>
            {
                if (DataContext != null)
                {
                    (DataContext as MSEntity).PropertyChanged += (s, e) => _propertyName = e.PropertyName;
                }
            };
            InitializeComponent();
        }
        private Action getRenameAction()
        {
        var vm = DataContext as MSEntity;
        var selection = vm.SelectedEntites.Select(entity => (entity, entity.Name)).ToList();
        return new Action(() =>
        {
        selection.ForEach(item => item.entity.Name = item.Item2);
        (DataContext as MSEntity).Refresh();
        });
        }

        private void onName_TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _propertyName =string.Empty;
            _undoAction = getRenameAction();
        }

        private void onName_TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(_propertyName == nameof(MSEntity.Name) && _undoAction != null)
            {
            _redoAction = getRenameAction();
            Project.UndoRedo.add(new undoRedoAction(_undoAction, _redoAction, "Rename game entity/entities"));
            _propertyName = null;
            }
            _undoAction = null;
        }

        private Action getIsEnableAction()
        {
            var vm = DataContext as MSEntity;
            var selection = vm.SelectedEntites.Select(entity => (entity, entity.IsEnable)).ToList();
            
            return new Action(() =>
            {
                selection.ForEach(item => item.entity.IsEnable = item.Item2);
                (DataContext as MSEntity).Refresh();
            });
        }

        private void onIsEnable_click(object sender, RoutedEventArgs e)
        {
            var undoAction = getIsEnableAction();//old state
            var vm = DataContext as MSEntity;
            vm.IsEnable = (sender as CheckBox).IsChecked == true;//setting new state
            var redoAction = getIsEnableAction();//remembers new state
            Project.UndoRedo.add(new undoRedoAction(undoAction, redoAction, 
                vm.IsEnable == true? "Enabled game entity/entities" : "Disabled game entity/entities"));
        }
    }
}



