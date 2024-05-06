using System.Diagnostics;

namespace GameEngineEditor.Content
{
    enum AssetType
    {
        Unknown,
        Animation,
        Audio,
        Material,
        Mesh,
        Skeleton,
        Texture
    }

    abstract class Asset : viewModelBase
    {
        public AssetType Type { get; private set; }
        public Asset(AssetType type)
        {
            Debug.Assert(type != AssetType.Unknown);
            Type = type;
        }
    }
}