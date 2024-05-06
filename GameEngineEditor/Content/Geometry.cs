using GameEngineEditor.utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngineEditor.Content
{
    enum PrimitiveMeshType
    {
        Plane,
        Cube,
        UvSphere,
        IcoSphere,
        Cylinder,
        Capsule,
    }

    class Mesh : viewModelBase
    {
        private int _vertexSize;
        public int VertexSize
        {
            get => _vertexSize;
            
            set
            {
                if (_vertexSize != value)
                {
                    _vertexSize = value;
                    onPropertyChanged(nameof(VertexSize));
                }
            }
        }

        private int _vertexCount;
        public int VertexCount
        {
            get => _vertexCount;

            set
            {
                if (_vertexCount != value)
                {
                    _vertexCount = value;
                    onPropertyChanged(nameof(VertexCount));
                }
            }
        }

        private int _indexSize;
        public int IndexSize
        {
            get => _indexSize;

            set
            {
                if (_indexSize != value)
                {
                    _indexSize = value;
                    onPropertyChanged(nameof(IndexSize));
                }
            }
        }

        private int _indexCount;
        public int IndexCount
        {
            get => _indexCount;

            set
            {
                if (_indexCount != value)
                {
                    _indexCount = value;
                    onPropertyChanged(nameof(IndexCount));
                }
            }
        }

        public byte[] Vertices { get; set; }
        public byte[] Indices { get; set; }


    }
    class MeshLOD :viewModelBase
    {
        private string _name;
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

        private float _lodThreshold;
        public float LodThreshold
        {
            get => _lodThreshold;

            set
            {
                if (_lodThreshold != value)
                {
                    _lodThreshold = value;
                    onPropertyChanged(nameof(LodThreshold));
                }
            }
        }

        public ObservableCollection<Mesh> Meshes { get; } = new ObservableCollection<Mesh> { };
    }

    class LODGroup : viewModelBase
    {
        private string _name;
        public string Name 
        { 
            get=>_name;

            set
            {
                if(_name != value)
                {
                    _name = value;
                    onPropertyChanged(nameof(Name));
                }
            }
        }

        public ObservableCollection<MeshLOD> LODs { get;} = new ObservableCollection<MeshLOD> { };
    }

    class Geometry : Asset
    {
        private readonly List<LODGroup> _lodGroups = new List<LODGroup>();
        public LODGroup GetLODGroup(int lodGroup = 0)
        {
            Debug.Assert(lodGroup >= 0 && lodGroup < _lodGroups.Count);
            return _lodGroups.Any() ? _lodGroups[lodGroup] : null;
        }

        public Geometry() : base(AssetType.Mesh)
        {

        }

        public void FromRawData(byte[] data)
        {
            Debug.Assert(data?.Length > 0);
            _lodGroups.Clear();

            using var reader = new BinaryReader(new MemoryStream(data));
            //skip scene name
            var s = reader.ReadInt32();//automatically moves pointer
            reader.BaseStream.Position += s;//skipping string

            //get number of LODS
            var numLODGroups = reader.ReadInt32();
            Debug.Assert(numLODGroups > 0);

            for (int i = 0; i < numLODGroups; i++)
            {
                //get lod groups name
                s = reader.ReadInt32();
                string lodGroupName;
                if (s > 0)
                {
                    var nameBytes = reader.ReadBytes(s);
                    lodGroupName = Encoding.UTF8.GetString(nameBytes);
                }
                else
                {
                    lodGroupName = $"lod_{ContentHelper.GetRandomString()}";
                }

                //get number of meshes in this LOD group
                var numMeshes = reader.ReadInt32();
                Debug.Assert(numMeshes > 0);
                List<MeshLOD> lods = ReadMeshLODS(numMeshes, reader);

                var lodGroup = new LODGroup() { Name = lodGroupName };
                lods.ForEach(l => lodGroup.LODs.Add(l));

                _lodGroups.Add(lodGroup);
            }
        }

        private static List<MeshLOD> ReadMeshLODS(int numMeshes, BinaryReader reader)
        {
            //consolidating meshes with which lod they belong to
            var lodIds = new List<int>();
            var lodList = new List<MeshLOD>();
            for(int i=0;i<numMeshes; i++)
            {
                ReadMeshes(reader,lodIds,lodList);
            }

            return lodList;
        }

        private static void ReadMeshes(BinaryReader reader, List<int> lodIds, List<MeshLOD> lodList)
        {
            //get meshes name
             var s= reader.ReadInt32();
            string meshname;
            if(s>0)
            {
                var nameBytes =reader.ReadBytes(s);
                meshname = Encoding.UTF8.GetString(nameBytes);
            }
            else
            {
                meshname = $"mesh_{ContentHelper.GetRandomString()}";
            }

            var mesh = new Mesh();
            var lodId = reader.ReadInt32();
            mesh.VertexSize = reader.ReadInt32();
            mesh.VertexCount = reader.ReadInt32();
            mesh.IndexSize = reader.ReadInt32();
            mesh.IndexCount = reader.ReadInt32();
            var lodThreshold = reader.ReadSingle();
            
            var vertexBufferSize = mesh.VertexSize*mesh.VertexCount;
            var indexBufferSize = mesh.IndexSize*mesh.IndexCount;

            mesh.Vertices = reader.ReadBytes(vertexBufferSize);
            mesh.Indices = reader.ReadBytes(indexBufferSize);
            MeshLOD lod;
            if (ID.isValid(lodId) && lodIds.Contains(lodId))
            {
                lod = lodList[lodIds.IndexOf(lodId)];
                Debug.Assert(lod != null);
            }
            else
            {
                lodIds.Add(lodId);
                lod = new MeshLOD() { Name = meshname, LodThreshold = lodThreshold };
                lodList.Add(lod);
            }

            lod.Meshes.Add(mesh);
        }
    }
}
