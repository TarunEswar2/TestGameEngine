using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GameEngineEditor.gameProject
{
    [DataContract]
    public class Scene:viewModelBase
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

        public Scene(Project project, string name)
        {
            Debug.Assert(project != null);
            Project = project;
            Name = name;
        }
        //List of Game Entitites
    }
}
