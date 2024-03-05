using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GameEngineEditor.Components
{
    //empty interface so we dont have to specify MScompoenet<T> type T in MSEntities observable collection
    interface IMSComponents
    {

    }

    [DataContract]
    abstract class Components : viewModelBase
    {
        public abstract IMSComponents GetMultiSelectionComponent(MSEntity msEntity);
        [DataMember]
        public gameEntitiy owner { get; private set; }

        public Components(gameEntitiy entity) 
        { 
            Debug.Assert(entity != null);
            owner = entity;
        }
    }

    abstract class MScomponent<T> : viewModelBase , IMSComponents where T : Components
    {
        private bool _enableUpdates = true;
        public List<T> SelectedComponents { get;}
    
        public MScomponent(MSEntity msEntity)
        {
            Debug.Assert(msEntity?.SelectedEntites?.Any() == true);
            SelectedComponents = msEntity.SelectedEntites.Select(entity=> entity.GetComponent<T>()).ToList();
            PropertyChanged += (s, e) => { if (_enableUpdates) UpdateComponents(e.PropertyName); };
        }

        protected abstract bool UpdateComponents(string propertyName);
        protected abstract bool UpdateMSComponent();
        
        public void Refresh()
        {
            _enableUpdates = false;
            UpdateMSComponent();
            _enableUpdates = true;
        }
    }
}
