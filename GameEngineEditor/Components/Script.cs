using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GameEngineEditor.Components
{
    [DataContract]
    class Script : Components
    {
        private string _name;
        [DataMember]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    onPropertyChanged(nameof(Name));
                }
            }
        }

        public Script(gameEntitiy entity) : base(entity)
        {
        }


        public override IMSComponents GetMultiSelectionComponent(MSEntity msEntity) => new MSScript(msEntity);

        public override void WrtieToBinary(BinaryWriter bw)
        {
            var nameBytes = Encoding.UTF8.GetBytes(Name);
            bw.Write(nameBytes.Length);
            bw.Write(nameBytes);
        }
    }

    sealed class MSScript : MScomponent<Script>
    {
        private string _name;
        [DataMember]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    onPropertyChanged(nameof(Name));
                }
            }
        }

        public MSScript(MSEntity msEntity) : base(msEntity)
        {
            Refresh();
        }

        protected override bool UpdateComponents(string propertyName)
        {
            if(propertyName == nameof(Name))
            {
                SelectedComponents.ForEach(c=> c.Name = _name);
                return true;
            }
            return false;
        }

        protected override bool UpdateMSComponent()
        {
            Name = MSEntity.GetMixedValue(SelectedComponents, new Func<Script, string>(x => x.Name));
            return true;
        }
    }
}
