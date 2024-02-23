using GameEngineEditor.utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GameEngineEditor.gameProject
{
    [DataContract(Name = "Game")]
    class Project : viewModelBase
    {
        public static string Extention { get; } = ".tge";
        [DataMember]
        public string Name { get; private set; } = "New Project";
        [DataMember]
        public string Path { get; private set; }

        public string Fullpath => $"{Path}{Name}/{Name}{Extention}";

        [DataMember(Name ="Scenes")]
        private ObservableCollection<Scene> _scenes = new ObservableCollection<Scene>();
        public ReadOnlyObservableCollection<Scene> Scenes { get; private set; }

        public static Project Current => Application.Current.MainWindow.DataContext as Project;

        private Scene _activeScene;
        public Scene ActiveScene
        {
            get => _activeScene;
            set
            {
                if (_activeScene != value)
                {
                    _activeScene = value;
                    onPropertyChanged(nameof(ActiveScene));
                }
            }
        }

        public ICommand addSceneCommand { get; private set; }
        public ICommand removeSceneCommand { get; private set; }
        public ICommand undoCommand { get; private set; }
        public ICommand redoCommand { get; private set; }

        public ICommand saveCommand { get; private set; }
        public static undoRedo UndoRedo { get; } = new undoRedo();

        public Project(string name, string path) 
        { 
            Name = name;
            Path = path;


            OnDeserialized(new StreamingContext());
        }
        
        public static Project Load(string file)
        {
            Debug.Assert(File.Exists(file));
            return Serializer.fromFile<Project>(file);
        }

        public static void Save(Project project) 
        {
            Serializer.toFile(project, project.Fullpath);
            Logger.Log(MessageType.Info, $"Saved Project to {project.Fullpath}");
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) //initialisiation method
        {
            if(_scenes != null)
            {
                Scenes = new ReadOnlyObservableCollection<Scene>(_scenes);
                onPropertyChanged(nameof(Scenes));
            }

            ActiveScene = Scenes.FirstOrDefault(x => x.IsActive);

            addSceneCommand = new RelayCommand<object>(x =>
            {
                addSceneInternal($"New Scene {_scenes.Count}");
                var newScene = _scenes.Last();
                var sceneIndex = _scenes.Count - 1;

                UndoRedo.add(new undoRedoAction(
                    () => removeSceneInternal(newScene),
                    () => _scenes.Insert(sceneIndex, newScene),
                    $"Add {newScene.Name}"
                    ));
            });

            removeSceneCommand = new RelayCommand<Scene>(x =>
            {
                var sceneIndex = _scenes.Count - 1;
                removeSceneInternal(x);

                UndoRedo.add(new undoRedoAction(
                () => _scenes.Insert(sceneIndex,x),
                () => removeSceneInternal(x),
                $"Remove {x.Name}"
                ));


            }, x => !x.IsActive);

            undoCommand = new RelayCommand<object>(x => UndoRedo.undo());
            redoCommand = new RelayCommand<object>(x => UndoRedo.redo());
            saveCommand = new RelayCommand<object>(x => Save(this));
        }

        public void unLoad()
        {
            UndoRedo.Reset();
        }

        private void addSceneInternal(string sceneName)
        {
            Debug.Assert(!string.IsNullOrEmpty(sceneName.Trim()));
            _scenes.Add(new Scene(this, sceneName));
        }
        private void removeSceneInternal(Scene scene)
        {
            Debug.Assert(_scenes.Contains(scene));
            _scenes.Remove(scene);
        }

        
    }
}
