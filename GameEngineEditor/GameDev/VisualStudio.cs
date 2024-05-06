using GameEngineEditor.utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using System.IO;
using GameEngineEditor.gameProject;//IrunnningObjectReference

namespace GameEngineEditor.GameDev
{
    static class VisualStudio
    {
        private static EnvDTE80.DTE2 _vsInstance = null;//used for visual studio core automation
        //16.0 denotes visual studio 2019
        private static readonly string _progID = "VisualStudio.DTE.17.0"; //program ID to open a new instance of vs studio 2022

        public static bool BuildSucceeded { get; private set; } = true;
        public static bool BuildDone { get; private set; } = true;
        private static bool BuildIntiated { get; set; } = false;
        private static int BuildCount { get; set; } = 0;

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable pprot);
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        public static bool IsVSInstanceNull()
        {
            if (_vsInstance == null) return true;
            return false;
        }

        public static void OpenVisualStudio(string solutionPath)
        {
            IRunningObjectTable rot = null;//Reference to the table of all com objects
            IEnumMoniker monikerTable = null;//reference to the com object in the table
            IBindCtx bindCtx = null;
            try
            {
                if (_vsInstance == null)
                {
                    //we lost the reference to already existing vs stuidio reference
                    //find and open vs studio
                    var hResult = GetRunningObjectTable(0, out rot);
                    if (hResult < 0 || rot == null) throw new COMException($"GetRunningObjectTable returned HRESULT: {hResult:X8}");//X8 denotes hexadecimel and 8 digits
                    
                    rot.EnumRunning(out monikerTable);
                    monikerTable.Reset();

                    hResult = CreateBindCtx(0, out bindCtx);
                    if (hResult < 0 || bindCtx == null) throw new COMException($"CreateBindCtx returned HRESULT: {hResult:X8}");//X8 denotes hexadecimel and 8 digits

                    IMoniker[] currentMoniker = new IMoniker[1];
                    bool IsOpen = false;
                    while(!IsOpen && monikerTable.Next(1, currentMoniker, IntPtr.Zero) == 0)
                    {
                        string name = string.Empty;
                        currentMoniker[0]?.GetDisplayName(bindCtx, null,out name);
                        if(name.Contains(_progID))
                        {
                            hResult = rot.GetObject(currentMoniker[0], out object obj);
                            if (hResult < 0 || obj == null) throw new COMException($"Running object table's GetOBject() returned HRESULT: {hResult:X8}");
                            
                            EnvDTE80.DTE2 dte = obj as EnvDTE80.DTE2;
                            var solutionName = dte.Solution.FullName;
                            if (solutionName == solutionPath) 
                            {
                                _vsInstance = dte;
                                break;
                            }
                        }
                    }

                    //there is no existing reference
                    if (_vsInstance == null)
                    {
                        Type visualStudioType = Type.GetTypeFromProgID(_progID, true);
                        _vsInstance = Activator.CreateInstance(visualStudioType) as EnvDTE80.DTE2;
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, "Failed to open Visual Studio");
            }
            finally
            {
                if(monikerTable != null) Marshal.ReleaseComObject(monikerTable);
                if (rot != null) Marshal.ReleaseComObject(rot);
                if (bindCtx != null) Marshal.ReleaseComObject(bindCtx);
            }

        }

        public static void closeVisualStudio()
        {
            if(_vsInstance?.Solution.IsOpen == true)
            {
                _vsInstance.ExecuteCommand("File.SaveAll");
                _vsInstance.Solution.Close(true);
            }

            if(_vsInstance != null) _vsInstance.Quit();
        }

        internal static bool AddfilesToSolution(string solution, string projectName, string[] files)
        {
            Debug.Assert(files?.Length > 0);
            OpenVisualStudio(solution);
            try
            {
                if (_vsInstance != null)
                {
                    if (!_vsInstance.Solution.IsOpen) _vsInstance.Solution.Open(solution);
                    else _vsInstance.ExecuteCommand("File.SaveAll");

                    foreach(EnvDTE.Project project in _vsInstance.Solution.Projects)
                    {
                        if(project.UniqueName.Contains(projectName))
                        {
                            foreach(string file in files)
                            {
                                project.ProjectItems.AddFromFile(file);
                            }
                        }
                    }

                    var cpp = files.FirstOrDefault(x => Path.GetExtension(x) == ".cpp");
                    if(!string.IsNullOrEmpty(cpp))
                    {
                        string viewKindTextView = "{7651A703-06E5-11D1-8EBD-00A0C90F26EA}";
                        _vsInstance.ItemOperations.OpenFile(cpp, viewKindTextView).Visible = true;
                    }
                    _vsInstance.MainWindow.Activate();
                    _vsInstance.MainWindow.Visible = true;
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Failed to add files to Visual Studio project");
                return false;
            }
            return true;
        }

        public static void BuildSolution(Project project, string BuildConfig, bool showWindow = true)
        {
           if(IsDebugging())
           {
                Logger.Log(MessageType.Error, "Visual Studio is currently running a process.");
                return;
           }

           BuildIntiated = true;
           BuildDone = BuildSucceeded = false;

            for (int i=0; i<3; i++)
           {
                try
                {
                    if (BuildDone) break;
                    OpenVisualStudio(project.Solution);
                    
                    if (!_vsInstance.Solution.IsOpen) _vsInstance.Solution.Open(project.Solution);
                    _vsInstance.MainWindow.Visible = showWindow;

                    _vsInstance.Events.BuildEvents.OnBuildProjConfigBegin += onBuildSolutionBegin;
                    _vsInstance.Events.BuildEvents.OnBuildProjConfigDone += onBuildSolutionDone;

                    try
                    {
                        foreach (var pdbFile in Directory.GetFiles(Path.Combine($"{project.Path}", $@"x64\{BuildConfig}", "*.pdb")))
                        {
                            File.Delete(pdbFile);
                        }
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine($"Error: {ex.Message}");
                    }

                    _vsInstance.Solution.SolutionBuild.SolutionConfigurations.Item(BuildConfig).Activate();
                    _vsInstance.ExecuteCommand("Build.BuildSolution");
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine($"Attempt {i}: failed to build {project.Name}");
                    if (!BuildSucceeded) System.Threading.Thread.Sleep(1000);//
                    else break;//
                }
           }

        }

        private static void onBuildSolutionDone(string Project, string ProjectConfig, string Platform, string SolutionConfig, bool Success)
        {
            if (BuildDone) return;

            if (Success) Logger.Log(MessageType.Info, $"Build {++BuildCount} {ProjectConfig} configuration succeeded");
            else Logger.Log(MessageType.Info, $"Build {++BuildCount} {ProjectConfig} configuration failed");

            BuildDone = true;
            BuildSucceeded = Success;
        }

        private static void onBuildSolutionBegin(string Project, string ProjectConfig, string Platform, string SolutionConfig)
        {
            if (BuildIntiated)
            {
                Logger.Log(MessageType.Info, $"Building {Project}, {ProjectConfig}, {Platform}, {SolutionConfig}");
                BuildIntiated = false;
            }
        }

        public static bool IsDebugging()
        {
            if(_vsInstance == null) return false;
            
            bool result = false;
            bool tryagian = true;

            for (int i = 0; i < 3 && tryagian; ++i)
            {
                try
                {
                    result = (_vsInstance != null) &&
                        (_vsInstance.Debugger.CurrentProgram != null) ||
                        (_vsInstance.Debugger.CurrentMode == EnvDTE.dbgDebugMode.dbgRunMode);
                    tryagian = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    if (!result) System.Threading.Thread.Sleep(1000);
                }
            }

            return result;
        }

        public static void Run(Project project, string configName, bool debug)
        {
            if(_vsInstance != null && !IsDebugging() && BuildDone && BuildSucceeded)
            {
                _vsInstance.ExecuteCommand(debug ? "Debug.Start" : "Debug.StartWithoutDebugging");
            }
        }

        public static void Stop()
        {
            if(_vsInstance != null && IsDebugging())
            {
                _vsInstance.ExecuteCommand("Debug.StopDebugging");
            }
        }
    }
}
