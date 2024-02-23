using GameEngineEditor.Components;
using GameEngineEditor.utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GameEngineEditor.gameProject
{
    [DataContract]
    class Scene:viewModelBase
    {
        
        private string _name;
        [DataMember]
        public string Name
        {
            get =>_name;
            set
            {
                if (_name != value) 
                { 
                    _name = value;
                    onPropertyChanged("Name");
                }
            }
        }

        [DataMember]
        public Project Project { get; private set; }

        private bool _isActive;

        [DataMember]
        public bool IsActive
        {
            get => _isActive;

            set
            {
                if(_isActive != value)
                {
                    _isActive = value;
                    onPropertyChanged(nameof(IsActive));
                }
            }
        }

        [DataMember(Name = nameof(GameEntities))]
        private ObservableCollection<gameEntitiy> _gameEntities = new ObservableCollection<gameEntitiy>();
        public ReadOnlyObservableCollection<gameEntitiy> GameEntities { get; private set; }

        public ICommand addGameEntityCommand { get; private set; }
        public ICommand removeGameEntityCommand { get; private set; }

        public Scene(Project project, string name)
        {
            Debug.Assert(project != null);
            Project = project;
            Name = name;

            OnDeserialized(new StreamingContext());
        }

        private void addGameEntity(gameEntitiy entity, int index = -1)
        {
            entity.IsActive = IsActive;
            Debug.Assert(!_gameEntities.Contains(entity));
            if( index == -1)
            {
                _gameEntities.Add(entity);
            }
            else
            {
                _gameEntities.Insert(index, entity);
            }
            
        }

        private void removeGameEntity(gameEntitiy entity)
        {
            Debug.Assert(_gameEntities.Contains(entity));
            entity.IsActive = false;
            _gameEntities.Remove(entity);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) //initialisiation method
        {
            if (_gameEntities != null)
            {
                GameEntities = new ReadOnlyObservableCollection<gameEntitiy>(_gameEntities);
                onPropertyChanged(nameof(GameEntities));
            }
            foreach(gameEntitiy entity in GameEntities) { entity.IsActive = true; } //load into game engine onproperty change(isActive) call create Gasmeentity

            addGameEntityCommand = new RelayCommand<gameEntitiy>(x =>
            {
                addGameEntity(x);
                var entityIndex = _gameEntities.Count - 1;

                Project.UndoRedo.add(new undoRedoAction(
                    () => removeGameEntity(x),
                    () => addGameEntity(x, entityIndex),
                    $"Add {x.Name} to {Name}"
                    ));
            });

            removeGameEntityCommand = new RelayCommand<gameEntitiy>(x =>
            {
                var entityIndex = _gameEntities.IndexOf(x);
                removeGameEntity(x);

                Project.UndoRedo.add(new undoRedoAction(
                () => addGameEntity(x, entityIndex),
                () => removeGameEntity(x),
                $"Remove {x.Name}"
                ));
            });
        }
    }
}
