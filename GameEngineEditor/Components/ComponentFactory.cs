using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngineEditor.Components
{
    enum ComponentType
    {
        Transform,
        Script
    }

    static class ComponentFactory
    {
        //gameEntity and object are inputs and return type is components
        private static readonly Func<gameEntitiy, object, Components>[] _function =
            new Func<gameEntitiy, object, Components>[]
        {
            (entity,data) => new Transform(entity),
            (entity,data) => new Script(entity){ Name = (string)data },
        };

        public static Func<gameEntitiy, object, Components> GetCreationFuntion(ComponentType componentType)
        {
            Debug.Assert((int)componentType < _function.Length);
            return _function[(int)componentType];
        }

        public static ComponentType ToEnumType(this Components component)
        {
            switch (component.GetType().Name)
            {
                case nameof(Transform):
                    return ComponentType.Transform;
                case nameof(Script):
                    return ComponentType.Script;
                default:
                    throw new ArgumentException("Unknown component type.");
            }
        }
    }
}
