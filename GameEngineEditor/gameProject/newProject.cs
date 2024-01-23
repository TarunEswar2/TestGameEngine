﻿using GameEngineEditor.gameProject.utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameEngineEditor.gameProject
{
    [DataContract]
    public class projectTemplate
    {
        [DataMember]
        public string projectType { get; set; }
        [DataMember]
        public string projectFile { get; set; }
        [DataMember]
        public List<string> folderNames { get; set; }

        public byte[] Icon { get; set; }
        public byte[] Screenshot { get; set; }
        public string IconFilePath {  get; set; }
        public string ScreenshotFilePath { get; set;}
        public string ProjectFilePath { get; set;}
    }

    class newProject : viewModelBase
    {
        private readonly string _templatePath = @"../../GameEngineEditor/projectTemplate/";

        private ObservableCollection<projectTemplate> _projectTemplates = new ObservableCollection<projectTemplate>();
        public ReadOnlyObservableCollection<projectTemplate> ProjectTemplates { get; }

        //constructor
        public newProject()
        {
            ProjectTemplates = new ReadOnlyObservableCollection<projectTemplate>(_projectTemplates);
            try
            {
                var templateFiles = Directory.GetFiles(_templatePath, "template.xml", SearchOption.AllDirectories);
                Debug.Assert(templateFiles.Any());
                foreach (var file in templateFiles)
                {
                    var template = Serializer.fromFile<projectTemplate>(file);
                    template.IconFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), "icon.png"));
                    template.Icon = File.ReadAllBytes(template.IconFilePath);
                    template.ScreenshotFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), "screenshot.png"));
                    template.Screenshot = File.ReadAllBytes(template.ScreenshotFilePath);
                    template.ProjectFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), template.projectFile));
                    _projectTemplates.Add(template);
                }
                validateDirectoryPath();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public string _projectName = "newProject";

        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName != value)
                {
                    _projectName = value;
                    validateDirectoryPath();
                    onPropertyChanged(nameof(ProjectName));
                }
            }
        }

        public string _projectPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\TestGameEngineProjects\";

        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                if (_projectPath != value)
                {
                    _projectPath = value;
                    validateDirectoryPath();
                    onPropertyChanged(nameof(ProjectPath));
                }
            }
        }

        private bool _isValid;
        public bool IsValid
        {
            get => _isValid;

            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    onPropertyChanged(nameof(IsValid));
                }
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;

            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    onPropertyChanged(nameof(ErrorMessage));
                }
            }
        }
        private bool validateDirectoryPath()
        {
            var path = ProjectPath;
            if (!Path.EndsInDirectorySeparator(path)) path += @"\";
            path += @$"{ProjectName}\";

            IsValid = false;
            if (string.IsNullOrWhiteSpace(ProjectName.Trim()))
            {
                ErrorMessage = "Type in Project Name";
            }
            else if (ProjectName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                ErrorMessage = "Project Name contains invalid character(s)";
            }
            else if (string.IsNullOrWhiteSpace(ProjectPath.Trim()))
            {
                ErrorMessage = "Select a valid Project Path";
            }
            else if (ProjectPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                ErrorMessage = "Project Path contains invalid character(s)";
            }
            else if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any())
            {
                ErrorMessage = "Project Directory already exists";
            }
            else
            {
                ErrorMessage = string.Empty;
                IsValid = true;

            }
         
            return IsValid;
        }

        public string createProject(projectTemplate template)
        {
            validateDirectoryPath();
            if(!IsValid)
            {
                return string.Empty;
            }

            var path = ProjectPath;
            if (!Path.EndsInDirectorySeparator(path)) ProjectPath += @"\";
            path = @$"{ProjectPath}{ProjectName}\";

            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                foreach(var folder in template.folderNames)
                {
                    Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), folder)));
                }

                //hidden directory
                var dirinfo = new DirectoryInfo(path + @".tge\");
                dirinfo.Attributes |= FileAttributes.Hidden;

                File.Copy(template.IconFilePath, Path.GetFullPath(Path.Combine(dirinfo.FullName, "icon.png")));
                File.Copy(template.ScreenshotFilePath, Path.GetFullPath(Path.Combine(dirinfo.FullName, "screenshot.png")));

                var projectXml = File.ReadAllText(template.ProjectFilePath);
                projectXml = string.Format(projectXml, ProjectName, ProjectPath);
                var projectPath = Path.GetFullPath(Path.Combine(path, $"{ProjectName}{Project.Extention}"));
                File.WriteAllText(projectPath, projectXml);
                return path;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }
    }
}
