using GameEngineEditor.DllWrapper;
using GameEngineEditor.gameProject;
using GameEngineEditor.utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace GameEngineEditor.Components
{
    [DataContract]
    [KnownType(typeof(Transform))]
    class gameEntitiy : viewModelBase
    {
        private int _entityID = ID.INVALID_ID;
        public int EntityID {
            get => _entityID;  
            set {
                if(value != _entityID)
                {
                    _entityID = value;
                    onPropertyChanged(nameof(EntityID));
                }
            }
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (value != _isActive)
                {
                    _isActive = value;
                    if (_isActive)
                    {
                        EntityID = GameEngineAPI.CreateGameEntity(this);
                        Debug.Assert(ID.isValid(_entityID));
                    }
                    else
                    {
                        GameEngineAPI.RemoveGameEntity(this);
                    }
                    onPropertyChanged(nameof(IsActive));
                }
            }
        }
        private bool _isEnable;
        [DataMember]
        public bool IsEnable
        {
            get => _isEnable;
            set
            {
                if (value != _isEnable)
                {
                    _isEnable = value;
                    onPropertyChanged(nameof(IsEnable));
                }
            }
        }

        private string _name;
        [DataMember]
        public string Name
        {
            get => _name;
            set
            {
                if (value != _name)
                {
                    _name = value;
                    onPropertyChanged(nameof(Name));
                }
            }
        }

        [DataMember(Name = nameof(Components))]
        private ObservableCollection<Components> _components = new ObservableCollection<Components>();
        public ReadOnlyObservableCollection<Components> Components { get; private set; }

        [DataMember]
        public Scene parentScene { get; private set; }

        public gameEntitiy(Scene scene)
        {
            Debug.Assert(scene != null);
            parentScene = scene;
            _components.Add(new Transform(this));
            OnDeserialized(new StreamingContext());
        }

        public Components GetComponent(Type type) => Components.FirstOrDefault(c => c.GetType() == type);
        public T GetComponent<T>() where T : Components => GetComponent(typeof(T))  as T; //T : component T must derive from component or be component
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_components != null)
            {
                Components = new ReadOnlyObservableCollection<Components>(_components);
                onPropertyChanged(nameof(Components));
            }
        }
    }

    abstract class MSEntity : viewModelBase
    {
        private bool _enableUpdates = true;
        //nullable becuase the ms entities can have different value in which case we will assign null
        private bool? _isEnable;
        public bool? IsEnable
        {
            get => _isEnable;
            set
            {
                if (value != _isEnable)
                {
                    _isEnable = value;
                    onPropertyChanged(nameof(IsEnable));
                }
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (value != _name)
                {
                    _name = value;
                    onPropertyChanged(nameof(Name));
                }
            }
        }

        private readonly ObservableCollection<IMSComponents> _components = new ObservableCollection<IMSComponents>();
        public ReadOnlyObservableCollection<IMSComponents> Components { get; }

        public List<gameEntitiy> SelectedEntites { get; }

        public MSEntity(List<gameEntitiy> entities)
        {
            Debug.Assert(entities?.Any() != null);
            Components = new ReadOnlyObservableCollection<IMSComponents>(_components);
            SelectedEntites = entities;
            PropertyChanged += (s, e) => { if (_enableUpdates) UpdateGameEntites(e.PropertyName); };
        }

        public void Refresh()
        {
            _enableUpdates = false;
            UpdateMSGameEntites();
            _enableUpdates = true;
        }

        protected virtual bool UpdateMSGameEntites()
        {
            IsEnable = GetMixedValue(SelectedEntites, new Func<gameEntitiy, bool>(x => x.IsEnable));
            Name = GetMixedValue(SelectedEntites, new Func<gameEntitiy, string>(x => x.Name));

            return true;
        }

        protected virtual bool UpdateGameEntites(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(IsEnable): SelectedEntites.ForEach(x => x.IsEnable = IsEnable.Value); return true;
                case nameof(Name): SelectedEntites.ForEach(x => x.Name = Name); return true;

            }
            return false;
        }

        public static float? GetMixedValue(List<gameEntitiy> entities, Func<gameEntitiy, float> getProperty)
        {
            var value = getProperty(entities.First());
            foreach (var entity in entities.Skip(1))
            {
                if (!value.isTheSameAs(getProperty(entity)))
                {
                    return null;
                }
            }

            return value;
        }

        public static bool? GetMixedValue(List<gameEntitiy> entities, Func<gameEntitiy, bool> getProperty)
        {
            var value = getProperty(entities.First());
            foreach (var entity in entities.Skip(1))
            {
                if (value != getProperty(entity))
                {
                    return null;
                }
            }

            return value;
        }

        public static string? GetMixedValue(List<gameEntitiy> entities, Func<gameEntitiy, string> getProperty)
        {
            var value = getProperty(entities.First());
            foreach (var entity in entities.Skip(1))
            {
                if (value != getProperty(entity))
                {
                    return null;
                }
            }

            return value;
        }


    }

    class MSGameEntity : MSEntity
    {
        public MSGameEntity(List<gameEntitiy> entities) : base(entities)
        {
            Refresh();
        }
    }
}
