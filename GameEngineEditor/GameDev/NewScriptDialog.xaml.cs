using EnvDTE;
using GameEngineEditor.gameProject;
using GameEngineEditor.utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Project = GameEngineEditor.gameProject.Project;

namespace GameEngineEditor.GameDev
{
    /// <summary>
    /// Interaction logic for NewScriptDialog.xaml
    /// </summary>
    public partial class NewScriptDialog : System.Windows.Window
    {
        private static readonly string _cppCode = @"#include""{0}.h""

namespace {1}
{{
	REGISTER_SCRIPT({0});
void {0}::begin_play()
	{{

	}}

void {0}::update(float dt)
	{{

	}}
}} //namespace {1}";

        private static readonly string _hCode = @"#pragma once

namespace {1}{{
	class {0} : public tge::script::entity_script
	{{
	public:
		constexpr explicit {0}(tge::game_entity::entity entity)
			: tge::script::entity_script(entity)
		{{

		}}

        void begin_play() override;
		void update(float dt) override;
	private:
    }};
}} //namespace {1}";

        private static readonly string _namespace = GetNamespaceFromProjectName();

        public NewScriptDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            scriptPath.Text = @"GameCode\";
        }

        private bool Validate()
        {
            bool IsValid = false;
            var name = ScriptName.Text.Trim();
            var path = scriptPath.Text.Trim();
            string errorMsg = string.Empty;
            if(string.IsNullOrEmpty(name))
            {
                errorMsg = "Type in a script Name";
            }
            else if(name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1 || name.Any(x => char.IsWhiteSpace(x)))
            {
                errorMsg = "Invalid Character(s) in script name";
            }
            else if (string.IsNullOrEmpty(path))
            {
                errorMsg = "Type in a script Path";
            }
            else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                errorMsg = "Invalid Character(s) in script path";
            }
            else if(!Path.GetFullPath(Path.Combine(Project.Current.Path,path)).Contains(Path.Combine(Project.Current.Path,@"GameCode\")))
            {
                errorMsg = "SScript Must be added to a sub folder of GameCode";
            }
            else if(File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path),$"{name}.cpp"))) ||
                    File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.h"))))
            {
                errorMsg = $"script {name} already exists in this folder";
            }
            else
            {
                IsValid = true;
            }

            if(!IsValid)
            {
                messageTextBlock.Foreground = FindResource("Editor.RedBrush") as Brush;
            }
            else
            {
                messageTextBlock.Foreground = FindResource("Editor.FontBrush") as Brush;
            }
            messageTextBlock.Text = errorMsg;

            return IsValid;
        }

        private void onScriptName_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Validate()) return;
            var name = ScriptName.Text.Trim();
            var project = Project.Current;
            messageTextBlock.Text = $"{name}.h and {name}.cpp will be added to {Project.Current.Name}";
        }

        private void onScriptPath_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Validate();
        }

        private async void onCreate_Button_Click(object sender, RoutedEventArgs e)
        {
            if(!Validate()) return;
            IsEnabled = false;

            try
            {
                var name = ScriptName.Text.Trim();
                var projectName = Project.Current.Name;
                var solution = Project.Current.Solution;
                var path = Path.GetFullPath(Path.Combine(Project.Current.Path, scriptPath.Text.Trim()));
                await Task.Run(() => CreateScript(name, path, solution, projectName));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to create script {ScriptName.Text}");
            }
            finally
            {
                Close();
            }
        }

        private void CreateScript(string name, string path, string solution, string projectName)
        {
            if(!Directory.Exists(path)) Directory.CreateDirectory(path);

            var cpp = Path.GetFullPath(Path.Combine(path, $"{name}.cpp"));
            var h = Path.GetFullPath(Path.Combine(path, $"{name}.h"));

            using(var sw = File.CreateText(cpp))
            {
                sw.Write(string.Format(_cppCode, name, _namespace));
            }
            using (var sw = File.CreateText(h))
            {
                sw.Write(string.Format(_hCode, name, _namespace));

            }

            string[] files = new string[] { cpp, h };

            for (int i = 0; i < 3; ++i)
            {
                if (!VisualStudio.AddfilesToSolution(solution, projectName, files)) System.Threading.Thread.Sleep(1000);
                else break;
            }
        }

        private static string GetNamespaceFromProjectName()
        {
            var projectName = Project.Current.Name;
            projectName = projectName.Replace(" ", "_");
            return projectName;
        }
    }
}
