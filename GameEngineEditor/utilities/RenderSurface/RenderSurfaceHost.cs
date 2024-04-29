using GameEngineEditor.DllWrapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;

namespace GameEngineEditor.utilities
{
    class RenderSurfaceHost : HwndHost
    {
        private readonly int _width = 800;
        private readonly int _height = 600;
        private IntPtr _renderWindowHandle = IntPtr.Zero;
        public int SurfaceID { get; private set; } = ID.INVALID_ID;
        private DelayEventTimer _resizeTimer;
        
        public RenderSurfaceHost(double width, double height)
        {
            _width = (int)width;
            _height = (int)height;
            _resizeTimer = new DelayEventTimer(TimeSpan.FromMilliseconds(250.0));
            _resizeTimer.Triggered += Resize;
        }

        private void Resize(object sender, DelayEventTimerArgs e)
        {
            e.RepeatEvent = (Mouse.LeftButton == MouseButtonState.Pressed);
            if(!e.RepeatEvent)
            {
                Logger.Log(MessageType.Info, "Resized");
                GameEngineAPI.ResizeRenderSurface(SurfaceID);
            }
        }

        public void Resize()
        {
            _resizeTimer.Trigger();
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            SurfaceID = GameEngineAPI.CreateRenderSurface(hwndParent.Handle,_width,_height);
            Debug.Assert(ID.isValid(SurfaceID));

            _renderWindowHandle = GameEngineAPI.GetWindowHandle(SurfaceID);
            Debug.Assert(_renderWindowHandle != IntPtr.Zero);

            return new HandleRef(this, _renderWindowHandle);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            GameEngineAPI.RemoveRenderSurface(SurfaceID);
            SurfaceID = ID.INVALID_ID;
            _renderWindowHandle = IntPtr.Zero;
        }
    }
}
