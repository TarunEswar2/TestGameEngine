using GameEngineEditor.utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GameEngineEditor.Components
{
    [DataContract]
    class Transform : Components
    {
        private Vector3 _position;
        [DataMember]
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (value != _position) 
                {
                    _position = value;
                    onPropertyChanged(nameof(Position));
                }
            }
        }

        private Vector3 _rotation;
        [DataMember]
        public Vector3 Rotation
        {
            get => _rotation;
            set
            {
                if (value != _rotation)
                {
                    _rotation = value;
                    onPropertyChanged(nameof(Rotation));
                }
            }
        }

        private Vector3 _scale;
        [DataMember]
        public Vector3 Scale
        {
            get => _scale;
            set
            {
                if (value != _scale)
                {
                    _scale = value;
                    onPropertyChanged(nameof(Scale));
                }
            }
        }

        public Transform(gameEntitiy entity) : base(entity)
        {

        }

        public override IMSComponents GetMultiSelectionComponent(MSEntity msEntity) => new MSTransform(msEntity);

        public override void WrtieToBinary(BinaryWriter bw)
        {
            bw.Write(_position.X);
            bw.Write(_position.Y);
            bw.Write(_position.Z);

            bw.Write(_rotation.X);
            bw.Write(_rotation.Y);
            bw.Write(_rotation.Z);

            bw.Write(_scale.X);
            bw.Write(_scale.Y);
            bw.Write(_scale.Z);
        }
    }

    sealed class MSTransform : MScomponent<Transform>
    {
        private float? _posX;
        public float? PosX
        {
            get => _posX;
            set
            {
                if (!_posX.isTheSameAs(value))
                {
                    _posX = value;
                    onPropertyChanged(nameof(PosX));
                }
            }
        }

        private float? _posY;
        public float? PosY
        {
            get => _posY;
            set
            {
                if (!_posY.isTheSameAs(value))
                {
                    _posY = value;
                    onPropertyChanged(nameof(PosY));
                }
            }
        }

        private float? _posZ;
        public float? PosZ
        {
            get => _posZ;
            set
            {
                if (!_posZ.isTheSameAs(value))
                {
                    _posZ = value;
                    onPropertyChanged(nameof(PosZ));
                }
            }
        }

        private float? _rotX;
        public float? RotX
        {
            get => _rotX;
            set
            {
                if (!_rotX.isTheSameAs(value))
                {
                    _rotX = value;
                    onPropertyChanged(nameof(RotX));
                }
            }
        }

        private float? _rotY;
        public float? RotY
        {
            get => _rotY;
            set
            {
                if (!_rotY.isTheSameAs(value))
                {
                    _rotY = value;
                    onPropertyChanged(nameof(RotY));
                }
            }
        }

        private float? _rotZ;
        public float? RotZ
        {
            get => _rotZ;
            set
            {
                if (!_rotZ.isTheSameAs(value))
                {
                    _rotZ = value;
                    onPropertyChanged(nameof(RotZ));
                }
            }
        }

        private float? _scaleX;
        public float? ScaleX
        {
            get => _scaleX;
            set
            {
                if (!_scaleX.isTheSameAs(value))
                {
                    _scaleX = value;
                    onPropertyChanged(nameof(ScaleX));
                }
            }
        }

        private float? _scaleY;
        public float? ScaleY
        {
            get => _scaleY;
            set
            {
                if (!_scaleY.isTheSameAs(value))
                {
                    _scaleY = value;
                    onPropertyChanged(nameof(ScaleY));
                }
            }
        }

        private float? _scaleZ;
        public float? ScaleZ
        {
            get => _scaleZ;
            set
            {
                if (!_scaleZ.isTheSameAs(value))
                {
                    _scaleZ = value;
                    onPropertyChanged(nameof(ScaleZ));
                }
            }
        }

        public MSTransform(MSEntity msEntity) :base(msEntity)
        {
            Refresh();
        }

        protected override bool UpdateComponents(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(PosX):
                case nameof(PosY):
                case nameof(PosZ):
                    SelectedComponents.ForEach(c => c.Position = new Vector3(_posX ?? c.Position.X, _posY ?? c.Position.Y, _posZ ?? c.Position.Z));
                    return true;

                case nameof(RotX):
                case nameof(RotY):
                case nameof(RotZ):
                    SelectedComponents.ForEach(c => c.Rotation = new Vector3(_rotX ?? c.Rotation.X, _rotY ?? c.Rotation.Y, _rotZ ?? c.Rotation.Z));
                    return true;

                case nameof(ScaleX):
                case nameof(ScaleY):
                case nameof(ScaleZ):
                    SelectedComponents.ForEach(c => c.Scale = new Vector3(_scaleX ?? c.Scale.X, _scaleY ?? c.Scale.Y, _scaleZ ?? c.Scale.Z)) ;
                    return true;
            }
            return false;
        }

        protected override bool UpdateMSComponent()
        {
            PosX = MSEntity.GetMixedValue<Transform>(SelectedComponents, new Func<Transform, float>(x => x.Position.X));
            PosY = MSEntity.GetMixedValue<Transform>(SelectedComponents, new Func<Transform, float>(x => x.Position.Y));
            PosZ = MSEntity.GetMixedValue<Transform>(SelectedComponents, new Func<Transform, float>(x => x.Position.Z));

            RotX = MSEntity.GetMixedValue<Transform>(SelectedComponents, new Func<Transform, float>(x => x.Rotation.X));
            RotY = MSEntity.GetMixedValue<Transform>(SelectedComponents, new Func<Transform, float>(x => x.Rotation.Y));
            RotZ = MSEntity.GetMixedValue<Transform>(SelectedComponents, new Func<Transform, float>(x => x.Rotation.Z));

            ScaleX = MSEntity.GetMixedValue<Transform>(SelectedComponents, new Func<Transform, float>(x => x.Scale.X));
            ScaleY = MSEntity.GetMixedValue<Transform>(SelectedComponents, new Func<Transform, float>(x => x.Scale.Y));
            ScaleZ = MSEntity.GetMixedValue<Transform>(SelectedComponents, new Func<Transform, float>(x => x.Scale.Z));

            return true;
        }
    }
}
