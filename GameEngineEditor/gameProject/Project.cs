using GameEngineEditor.DllWrapper;
using GameEngineEditor.GameDev;
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
    enum BuildConfiguration 
    {
        Debug,
        DebugEditor,
        Release,
        ReleaseEditor
    }
    [DataContract(Name = "Game")]
    class Project : viewModelBase
    {
        public static string Extention { get; } = ".tge";
        [DataMember]
        public string Name { get; private set; } = "New Project";
        [DataMember]
        public string Path { get; private set; }

        public string Fullpath => $"{Path}{Name}{Extention}";
        public string Solution => $@"{Path}{Name}.sln";

        private static readonly string[] _buildConfigurationNames = new string[]
        {
            "Debug",
            "DebugEditor",
            "Release",
            "ReleaseEditor"
        };

        [DataMember(Name = "Scenes")]
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
        public ICommand buildCommand { get; private set; }
        public static undoRedo UndoRedo { get; } = new undoRedo();
        private void SetCommands()
        {
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
                () => _scenes.Insert(sceneIndex, x),
                () => removeSceneInternal(x),
                $"Remove {x.Name}"
                ));
            }, x => !x.IsActive);

            undoCommand = new RelayCommand<object>(x => UndoRedo.undo(), x => UndoRedo.UndoList.Any());
            redoCommand = new RelayCommand<object>(x => UndoRedo.redo(), x => UndoRedo.RedoList.Any());
            saveCommand = new RelayCommand<object>(x => Save(this));
            buildCommand = new RelayCommand<bool>(async x => await BuildGameCodeDll(x), x => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);

            onPropertyChanged(nameof(addSceneCommand));
            onPropertyChanged(nameof(removeSceneCommand));
            onPropertyChanged(nameof(undoCommand));
            onPropertyChanged(nameof(redoCommand));
            onPropertyChanged(nameof(saveCommand));
            onPropertyChanged(nameof(buildCommand));
        }

        private static string GetConfigurationName(BuildConfiguration config) => _buildConfigurationNames[(int)config];

        private int _buildConfig;
        [DataMember]
        public int BuildConfig
        {
            get => _buildConfig;
            set
            {
                if (_buildConfig != value)
                {
                    _buildConfig = value;
                    onPropertyChanged(nameof(BuildConfig));
                }
            }
        }

        public BuildConfiguration StandAloneBuildConfiguration => BuildConfig == 0 ? BuildConfiguration.Debug : BuildConfiguration.Release;
        public BuildConfiguration DllBuildConfiguration => BuildConfig == 0 ? BuildConfiguration.DebugEditor : BuildConfiguration.ReleaseEditor;

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
        private async void OnDeserialized(StreamingContext context) //initialisiation method
        {
            if(_scenes != null)
            {
                Scenes = new ReadOnlyObservableCollection<Scene>(_scenes);
                onPropertyChanged(nameof(Scenes));
            }

            ActiveScene = Scenes.FirstOrDefault(x => x.IsActive);
            
            await BuildGameCodeDll(false);//build the current game code

            SetCommands();
        }
        
        private async Task BuildGameCodeDll(bool showWindow = true)
        {
            try
            {
                UnloadGameCodeDll();

                await Task.Run(() => VisualStudio.BuildSolution(this, GetConfigurationName(DllBuildConfiguration), showWindow));

                if (VisualStudio.BuildSucceeded)
                {
                    LoadGameCodeDll();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        private void LoadGameCodeDll()
        {
            var configName = GetConfigurationName(DllBuildConfiguration);
            var dll = $@"{Path}x64\{configName}\{Name}.dll";

            if(File.Exists(dll) && GameEngineAPI.LoadGameCodeDll(dll) != 0)
            {
                Logger.Log(MessageType.Info, "Game Code DLL loaded succesfully");
            }
            else
            {
                Logger.Log(MessageType.Warning, $"Failed to load Game Code DLL at {dll}");
            }
        }

        private void UnloadGameCodeDll()
        {
            Debug.WriteLine("entered unloading");
            if(GameEngineAPI.UnloadGameCodeDll() != 0)
            {
                Logger.Log(MessageType.Info, "Game code Dll unloaded successfully");
            }
            else
            {
                Logger.Log(MessageType.Warning, "Game code Dll unloading failed");
            }
        }

        public void unLoad()
        {
            VisualStudio.closeVisualStudio();
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
