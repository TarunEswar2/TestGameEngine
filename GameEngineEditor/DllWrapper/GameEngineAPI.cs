using EnvDTE;
using GameEngineEditor.Components;
using GameEngineEditor.EngineAPIStructs;
using GameEngineEditor.gameProject;
using GameEngineEditor.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameEngineEditor.EngineAPIStructs
{
    [StructLayout(LayoutKind.Sequential)]
    class TransfromComponent
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
    }

    [StructLayout(LayoutKind.Sequential)]
    class ScriptComponent
    {
        public IntPtr ScriptCreator;
    }

    [StructLayout(LayoutKind.Sequential)]
    class GameEntityDescriptor
    {
        public TransfromComponent Transform = new TransfromComponent();
        public ScriptComponent Script = new ScriptComponent();
    }
}

namespace GameEngineEditor.DllWrapper
{
    static class GameEngineAPI
    {
        private const string _engineDll = "GameEngineDll.dll";
        [DllImport(_engineDll, CharSet = CharSet.Ansi)]
        public static extern int LoadGameCodeDll(string dllPath);

        [DllImport(_engineDll)]
        public static extern int UnloadGameCodeDll();

        [DllImport(_engineDll)]
        public static extern IntPtr GetScriptCreator(string name);

        [DllImport(_engineDll)]
        [return: MarshalAs(UnmanagedType.SafeArray)]
        public static extern string[] GetScriptNames();
        [DllImport(_engineDll)]
        public static extern int CreateRenderSurface(IntPtr host, int width, int height);
        [DllImport(_engineDll)]
        public static extern void RemoveRenderSurface(int sufaceId);
        [DllImport(_engineDll)]
        public static extern IntPtr GetWindowHandle(int sufaceId);
        [DllImport(_engineDll)]
        public static extern void ResizeRenderSurface(int surfaceId);

        internal static class EntityAPI
        {
            [DllImport(_engineDll)]
            private static extern int CreateGameEntity(GameEntityDescriptor desc);

            public static int CreateGameEntity(gameEntitiy entity)
            {
                GameEntityDescriptor desc = new GameEntityDescriptor();

                //transform component
                {
                    var c = entity.GetComponent<Transform>();
                    desc.Transform.Position = c.Position;
                    desc.Transform.Rotation = c.Rotation;
                    desc.Transform.Scale = c.Scale;
                }

                //script component
                {
                    var c = entity.GetComponent<Script>();
                    if(c != null && gameProject.Project.Current != null)
                    {
                        if (gameProject.Project.Current.AvailableScripts.Contains(c.Name))
                        {
                            desc.Script.ScriptCreator = GetScriptCreator(c.Name);
                        }
                        else
                        {
                            Logger.Log(MessageType.Error, $"Unable to Find script with name {c.Name}. Game entity will be created without Script component.");
                        }
                    }
                }

                return CreateGameEntity(desc);
            }

            [DllImport(_engineDll)]
            private static extern int RemoveGameEntity(int id);

            public static void RemoveGameEntity(gameEntitiy entity)
            {
                RemoveGameEntity(entity.EntityID);
            }
        }
    }
}
