using GameEngineEditor.utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GameEngineEditor.gameProject
{
    [DataContract]
    public class ProjectData
    {
        [DataMember]
        public string ProjectName { get; set; }
        [DataMember]
        public string ProjectPath { get; set; }
        [DataMember]
        public DateTime Date { get; set; }

        public string FullPath { get => $"{ProjectPath}{ProjectName}{Project.Extention}"; } //Project file(test.tge) path
        public byte[] Icon { get; set; }
        public byte[] Screenshot { get; set; }
    }

    [DataContract]
    public class ProjectDataList
    {
        [DataMember]
        public List<ProjectData> Projects { get; set; }
    }
    class openProject
    {
        private static readonly string _applicationDataPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\GameEngineEditor\";
        private static readonly string _projectDataPath;
        private static readonly ObservableCollection<ProjectData> _projects = new ObservableCollection<ProjectData>();        
        public static ReadOnlyObservableCollection<ProjectData> Projects { get; }
        
        static openProject()
        {
            try
            {
                if(!Directory.Exists(_applicationDataPath)) Directory.CreateDirectory(_applicationDataPath);
                _projectDataPath = $@"{_applicationDataPath}/ProjectData.xml";
                Projects = new ReadOnlyObservableCollection<ProjectData>(_projects);
                readProjectData();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to open Project from {_applicationDataPath}");
                throw;
            }
        }

        private static void readProjectData()
        {
            if(File.Exists(_projectDataPath))
            {
                var projects = Serializer.fromFile<ProjectDataList>(_projectDataPath).Projects.OrderByDescending(x =>x.Date);
                _projects.Clear();
                foreach (var project in projects) 
                { 
                    if(File.Exists(project.FullPath))
                    {
                        project.Icon = File.ReadAllBytes($@"{project.ProjectPath}\.tge\icon.png");
                        project.Screenshot = File.ReadAllBytes($@"{project.ProjectPath}\.tge\screenshot.png");
                        _projects.Add(project);
                    }
                }
            }
        }

        private static void WriteProjectData()
        {
            while (true)
            {

                try
                {
                    var projects = _projects.OrderBy(x => x.Date).ToList();
                    Serializer.toFile(new ProjectDataList() { Projects = projects }, _projectDataPath);
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public static Project open(ProjectData data)
        {
            readProjectData();
            var project = _projects.FirstOrDefault(x => x.FullPath == data.FullPath);

            if (project != null) 
            {
                project.Date = DateTime.Now;
            }
            else
            {
                project = data;
                project.Date = DateTime.Now;
                _projects.Add(project);
            }
            WriteProjectData();

            return Project.Load(project.FullPath);
        }
    }
}
