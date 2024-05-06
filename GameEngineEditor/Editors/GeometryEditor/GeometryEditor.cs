using EnvDTE;
using GameEngineEditor.Content;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace GameEngineEditor.Editors
{
    class MeshRendererVertexData : viewModelBase
    {
        private Brush _specular = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF111111"));

        public Brush Specular
        {
            get => _specular;
            set
            {
                if (_specular != value)
                {
                    _specular = value;
                    onPropertyChanged(nameof(Specular));
                }
            }
        }

        private Brush _diffuse = Brushes.White;

        public Brush Diffuse
        {
            get => _diffuse;
            set
            {
                if (_diffuse != value)
                {
                    _diffuse = value;
                    onPropertyChanged(nameof(Diffuse));
                }
            }
        }

        public Point3DCollection Positions { get; } = new Point3DCollection();
        public Vector3DCollection Normals { get; } = new Vector3DCollection();
        public PointCollection UVs { get; } = new PointCollection(); 
        public Int32Collection Indices { get; } = new Int32Collection();
    }

    class MeshRenderer : viewModelBase
    {
        public ObservableCollection<MeshRendererVertexData> Meshes { get; } = new ObservableCollection<MeshRendererVertexData>();

        private Vector3D _cameraDirection = new Vector3D(0, -10, 0);
        public Vector3D CameraDirection
        {
            get => _cameraDirection;
            set
            {
                if (_cameraDirection != value)
                {
                    _cameraDirection = value;
                    onPropertyChanged(nameof(CameraDirection));
                }
            }
        }

        private Point3D _cameraPosition = new Point3D(0, 10, 0);
        public Point3D CameraPosition
        {
            get => _cameraPosition;
            set
            {
                if (_cameraPosition != value)
                {
                    _cameraPosition = value;
                    CameraDirection = new Vector3D(-value.X, -value.Y, -value.Z);
                    onPropertyChanged(nameof(OffsetCameraPosition));
                    onPropertyChanged(nameof(CameraPosition));
                }
            }
        }

        private Point3D _cameraTarget = new Point3D(0,0,0);
        public Point3D CameraTarget
        {
            get => _cameraTarget;
            set
            {
                if (_cameraTarget != value)
                {
                    _cameraTarget = value;
                    onPropertyChanged(nameof(OffsetCameraPosition));
                    onPropertyChanged(nameof(CameraTarget));
                }
            }
        }

        public Point3D OffsetCameraPosition =>
            new Point3D(CameraPosition.X + CameraTarget.X, CameraPosition.Y + CameraTarget.Y, CameraPosition.Z + CameraTarget.Z);

        private Color _keyLight = (Color)ColorConverter.ConvertFromString($"#ffaeaeae");
        public Color KeyLight
        {
            get => _keyLight;
            set
            {
                if(value != _keyLight)
                {
                    _keyLight = value;
                    onPropertyChanged(nameof(KeyLight));
                }
            }
        }

        private Color _skyLight = (Color)ColorConverter.ConvertFromString($"#ff111b30");
        public Color SkyLight
        {
            get => _skyLight;
            set
            {
                if (value != _skyLight)
                {
                    _skyLight = value;
                    onPropertyChanged(nameof(SkyLight));
                }
            }
        }

        private Color _groundLight = (Color)ColorConverter.ConvertFromString($"#ff3f2f1e");
        public Color GroundLight
        {
            get => _groundLight;
            set
            {
                if (value != _groundLight)
                {
                    _groundLight = value;
                    onPropertyChanged(nameof(GroundLight));
                }
            }
        }
        private Color _ambientLight = (Color)ColorConverter.ConvertFromString($"#ff3f2f1e");
        public Color AmbientLight
        {
            get => _ambientLight;
            set
            {
                if (value != _ambientLight)
                {
                    _ambientLight = value;
                    onPropertyChanged(nameof(AmbientLight));
                }
            }
        }

        public MeshRenderer(MeshLOD lod,MeshRenderer old)
        {
            Debug.Assert(lod?.Meshes.Any() == true);
            //calculate vertec size minus the position and vectors
            var offset = lod.Meshes[0].VertexSize -3*sizeof(float) - sizeof(int) -2*sizeof(short); //skipping position resersed[3](3 bytes) + t_sign(1 byte) = 4 bytes=1 it and normal[2]

            double minX, minY, minZ; minX = minY = minZ = double.MaxValue;
            double maxX, maxY, maxZ; maxX = maxY = maxZ = double.MinValue;

            Vector3D avgNormal = new Vector3D();
            //This is to unpack the packed normals;
            var intervals = 2.0f / ((1 << 16) - 1);

            foreach(var mesh in lod.Meshes)
            {
                var vertexData = new MeshRendererVertexData();
                //unpack all vertices
                using (var reader = new BinaryReader(new MemoryStream(mesh.Vertices)))
                    for(int i=0;i<mesh.VertexCount;++i)
                    {
                            var posX = reader.ReadSingle();
                            var posY = reader.ReadSingle();
                            var posZ = reader.ReadSingle();
                            var signs = (reader.ReadUInt32() >> 24) & 0x000000ff;
                            vertexData.Positions.Add(new Point3D(posX, posY, posZ));

                            //adjust the bounding box
                            minX = Math.Min(minX, posX); maxX=Math.Max(maxX, posX);
                            minY = Math.Min(minY, posY); maxY=Math.Max(maxY, posY);
                            minZ = Math.Min(minZ, posZ); maxZ=Math.Max(maxZ, posZ);

                            //read normals(unpacking)
                            var nrmX = reader.ReadUInt16() * intervals - 1.0f;
                            var nrmY = reader.ReadUInt16() * intervals - 1.0f;
                            var nrmZ = Math.Sqrt(Math.Clamp(1f - (nrmX * nrmX + nrmY * nrmY), 0f, 1f))*((signs&0x2) - 1f);
                            var normal = new Vector3D(nrmX, nrmY, nrmZ);
                            normal.Normalize();
                            vertexData.Normals.Add(normal);
                            avgNormal += normal;

                            //read UVs(skip tangents and joint data)
                            reader.BaseStream.Position += (offset - sizeof(float) * 2);
                            var u = reader.ReadSingle();
                            var v = reader.ReadSingle();
                            vertexData.UVs.Add(new System.Windows.Point(u, v));
                    }

                using (var reader = new BinaryReader(new MemoryStream(mesh.Indices)))
                    if (mesh.IndexSize == sizeof(short))
                        for (int i = 0; i < mesh.IndexCount; i++) vertexData.Indices.Add(reader.ReadUInt16());
                    else
                        for (int i = 0; i < mesh.IndexCount; i++) vertexData.Indices.Add(reader.ReadInt32());

                vertexData.Positions.Freeze();
                vertexData.Normals.Freeze();
                vertexData.UVs.Freeze();
                vertexData.Indices.Freeze();
                Meshes.Add(vertexData);
            }

            if(old != null)
            {
                CameraTarget = old.CameraTarget;
                CameraPosition = old.CameraPosition;
            }
            else
            {
                var width = maxX - minX;
                var height = maxY - minY;
                var depth = maxZ - minZ;
                var radius = new Vector3D(height, width, depth).Length * 1.2;
                if(avgNormal.Length>0.8)
                {
                    avgNormal.Normalize();
                    avgNormal *= radius;
                    CameraPosition = new Point3D(avgNormal.X, avgNormal.Y, avgNormal.Z);
                }
                else
                {
                    CameraPosition = new Point3D(width, height*0.5, radius);
                }

                CameraTarget = new Point3D(minX+width*0.5, minY+height*0.5, minZ+depth*0.5);//center of the object we are rendering
            }
        }
    }

    class GeometryEditor : viewModelBase, IAssetEditor
    {
        public Content.Asset Asset => Geometry;

        private Content.Geometry _geometry;
        public Content.Geometry Geometry
        { 
            get => _geometry;
            set
            {
                if (_geometry != value) 
                { 
                    _geometry = value;
                    onPropertyChanged(nameof(Geometry));
                }
            }
        }

        private MeshRenderer _meshRenderer;
        public MeshRenderer MeshRenderer
        {
            get => _meshRenderer;
            set
            {
                if (_meshRenderer != value)
                {
                    _meshRenderer = value;
                    onPropertyChanged(nameof(MeshRenderer));
                }
            }
        }

        public void SetAsset(Content.Asset asset)
        {
            Debug.Assert(asset is Content.Geometry);
            if(asset is Content.Geometry geometry) 
            {
                Geometry = geometry;
                MeshRenderer = new MeshRenderer(Geometry.GetLODGroup().LODs[0],MeshRenderer);
            }
        }
    }
}
