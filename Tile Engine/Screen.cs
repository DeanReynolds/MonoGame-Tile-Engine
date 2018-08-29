using Microsoft.Xna.Framework;
using System;
using System.Windows.Forms;

namespace Tile_Engine
{
    public static class Screen
    {
        public static Modes Mode
        {
            get
            {
#if WINDOWS
                return _mode;
#else
                return Modes.Fullscreen;
#endif
            }
#if WINDOWS
            set
            {
                if (_mode == value)
                    return;
                if (value == Modes.Windowed)
                {
                    Program.Game.Services.GetService<GraphicsDeviceManager>().IsFullScreen = false;
                    Program.Game.Services.GetService<GraphicsDeviceManager>().ApplyChanges();
                    Program.Game.Window.IsBorderless = false;
                    _form.Size = new System.Drawing.Size(_windowedWidth, _windowedHeight);
                    System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromPoint(_form.Location);
                    _form.Location = new System.Drawing.Point((screen.Bounds.X + (screen.Bounds.Width / 2) - (_form.Width / 2)), (screen.Bounds.Y + (screen.Bounds.Height / 2) - (_form.Height / 2)));
                }
                else if (value == Modes.Fullscreen)
                {
                    Program.Game.Services.GetService<GraphicsDeviceManager>().IsFullScreen = true;
                    Program.Game.Services.GetService<GraphicsDeviceManager>().ApplyChanges();
                }
                else if (value == Modes.WindowedBorderless)
                {
                    Program.Game.Services.GetService<GraphicsDeviceManager>().IsFullScreen = false;
                    Program.Game.Services.GetService<GraphicsDeviceManager>().ApplyChanges();
                    Program.Game.Window.IsBorderless = false;
                    System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromPoint(_form.Location);
                    _form.Location = screen.Bounds.Location;
                    _windowedWidth = _form.Width;
                    _windowedHeight = _form.Height;
                    Program.Game.Window.IsBorderless = true;
                    _form.Size = screen.Bounds.Size;
                }
                _mode = value;
            }
#endif
        }
        public static int ViewportX { get { return Program.Game.GraphicsDevice.Viewport.X; } }
        public static int ViewportY { get { return Program.Game.GraphicsDevice.Viewport.Y; } }
        public static int ViewportWidth { get { return Program.Game.GraphicsDevice.Viewport.Width; } }
        public static int ViewportHeight { get { return Program.Game.GraphicsDevice.Viewport.Height; } }
        public static int WindowWidth { get { return Program.Game.Window.ClientBounds.Width; } }
        public static int WindowHeight { get { return Program.Game.Window.ClientBounds.Height; } }
        public static int PreferredBackBufferWidth { get { return Program.Game.Services.GetService<GraphicsDeviceManager>().PreferredBackBufferWidth; } }
        public static int PreferredBackBufferHeight { get { return Program.Game.Services.GetService<GraphicsDeviceManager>().PreferredBackBufferHeight; } }
        public static int VirtualWidth;
        public static int VirtualHeight;

        public enum Modes { Windowed, Fullscreen, WindowedBorderless }

#if WINDOWS
        private static Modes _mode = Modes.Windowed;
        private static Form _form;
        private static int _windowedWidth;
        private static int _windowedHeight;
#endif

        static Screen()
        {
#if WINDOWS
            _form = (Form)Control.FromHandle(Program.Game.Window.Handle);
            VirtualWidth = _form.ClientSize.Width;
            VirtualHeight = _form.ClientSize.Height;
            //Console.WriteLine(string.Format("{0}, {1}", VirtualWidth, VirtualHeight));
#else
            VirtualWidth = 800;
            VirtualHeight = 480;
#endif
        }
    }
}