using GameEngineEditor.Components;
using GameEngineEditor.DllWrapper;
using GameEngineEditor.GameDev;
using GameEngineEditor.utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
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
        public ICommand debugStartCommand { get; private set; }
        public ICommand debugStartWithoutDebuggingCommand { get; private set; }
        public ICommand debugStopCommand { get; private set; }

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
            debugStartCommand = new RelayCommand<object>(async x=> await RunGame(true), x => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);
            debugStartWithoutDebuggingCommand = new RelayCommand<object>(async x=> await RunGame(false), x => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);
            debugStopCommand = new RelayCommand<object>(async x=> await StopGame(), x => VisualStudio.IsDebugging());

            onPropertyChanged(nameof(addSceneCommand));
            onPropertyChanged(nameof(removeSceneCommand));
            onPropertyChanged(nameof(undoCommand));
            onPropertyChanged(nameof(redoCommand));
            onPropertyChanged(nameof(saveCommand));
            onPropertyChanged(nameof(buildCommand));
            onPropertyChanged(nameof(debugStartCommand));
            onPropertyChanged(nameof(debugStartWithoutDebuggingCommand));
            onPropertyChanged(nameof(debugStopCommand));
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

        private string[] _availableScripts;
        public string[] AvailableScripts
        {
            get => _availableScripts;
            set
            {
                if(_availableScripts != value)
                {
                    _availableScripts = value;
                    onPropertyChanged(nameof(AvailableScripts));
                }
            }
        }

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
            Debug.Assert(ActiveScene != null);
            
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

            AvailableScripts = null;
            if(File.Exists(dll) && GameEngineAPI.LoadGameCodeDll(dll) != 0)
            {
                AvailableScripts = GameEngineAPI.GetScriptNames();
                ActiveScene.GameEntities.Where(x => x.GetComponent<Script>() != null).ToList().ForEach(x => x.IsActive = true);
                Logger.Log(MessageType.Info, "Game Code DLL loaded succesfully");
            }
            else
            {
                Logger.Log(MessageType.Warning, $"Failed to load Game Code DLL at {dll}");
            }
        }

        private void UnloadGameCodeDll()
        {
            var UpdateScenes = ActiveScene.GameEntities.Where(x => x.GetComponent<Script>() != null).ToList();
            UpdateScenes.ForEach(x => x.IsActive = false);
            if(GameEngineAPI.UnloadGameCodeDll() != 0)
            {
                Logger.Log(MessageType.Info, "Game code Dll unloaded successfully");
            }
            else
            {
                Logger.Log(MessageType.Warning, "Game code Dll unloading failed");
            }
        }

        private void SaveToBinary()
        {
            var configName = GetConfigurationName(StandAloneBuildConfiguration);
            var bin = $@"{Path}x64\{configName}\game.bin";

            using (var bw = new BinaryWriter(File.Open(bin,FileMode.Create,FileAccess.Write))) 
            {
                bw.Write(ActiveScene.GameEntities.Count);
                foreach(var entity in ActiveScene.GameEntities)
                {
                    bw.Write(0);
                    bw.Write(entity.Components.Count);
                    foreach(var component in entity.Components)
                    {
                        bw.Write((int)component.ToEnumType());
                        component.WrtieToBinary(bw);
                    }
                }
            }
        }

        private async Task RunGame(bool debug)
        {
            var configName = GetConfigurationName(StandAloneBuildConfiguration);
            await Task.Run(() => VisualStudio.BuildSolution(this, configName, debug));
            if(VisualStudio.BuildSucceeded)
            {
                SaveToBinary();
                await Task.Run(() => VisualStudio.Run(this, configName, debug));
            }
        }

        private async Task StopGame() => await Task.Run(() => VisualStudio.Stop());

        public void unLoad()
        {
            UnloadGameCodeDll();
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
