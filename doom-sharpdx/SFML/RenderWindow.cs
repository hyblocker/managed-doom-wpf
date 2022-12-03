using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Input;
using WpfWindow = System.Windows.Window;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using SfmlKeyEventArgs = SFML.Window.KeyEventArgs;

namespace SFML.Graphics {
    public class RenderWindow {
        public bool IsOpen = true;
        public DrawEvent OnDraw;
        public delegate void DrawEvent(ref byte[] textureData);
        public FrameworkElement Element;
        public WpfWindow RootWindow;
        public Vector2 Size = new Vector2(0,0);

        public EventHandler<SfmlKeyEventArgs> KeyPressed;
        public EventHandler<SfmlKeyEventArgs> KeyReleased;

        private bool m_isCursorLocked = false;

        public Vector2 Position { get {

                // Left boundary
                var xL = (int)Application.Current.MainWindow.Left;
                // Top boundary
                var yT = (int)Application.Current.MainWindow.Top;

                return new Vector2(xL, yT);
                ;
            } }

        public RenderWindow(VideoMode mode, string title, FrameworkElement elem) {
            this.Element = elem;
            Size = new Vector2(mode.Width, mode.Height);


            if (Element != null) {
                RootWindow = WpfWindow.GetWindow(elem);

                RootWindow.PreviewKeyDown      += Wpf_PreviewKeyDown;
                RootWindow.PreviewKeyUp        += Wpf_PreviewKeyUp;
                RootWindow.PreviewMouseDown    += Wpf_PreviewMouseDown;
                RootWindow.PreviewMouseUp      += Wpf_PreviewMouseUp;
            }
        }

        public bool HasFocus() {
            // return Element == null ? true : Element.IsFocused;
            return true;
        }

        public void Clear(Vector4 clearColor) {

        }
        public void Display() {

        }
        public void Dispose() {

            if ( RootWindow != null ) {
                RootWindow.PreviewKeyDown      -= Wpf_PreviewKeyDown;
                RootWindow.PreviewKeyUp        -= Wpf_PreviewKeyUp;
                RootWindow.PreviewMouseDown    -= Wpf_PreviewMouseDown;
                RootWindow.PreviewMouseUp      -= Wpf_PreviewMouseUp;
            }
        }

        public void SetMouseCursorGrabbed(bool v) {
            m_isCursorLocked = v;
        }

        public void SetMouseCursorVisible(bool show) {
            ShowCursor(show);
        }

        internal void HandleCursor() {
            if ( m_isCursorLocked ) {
                SfmlMouse.SetPosition(new Vector2(Size.X / 2, Size.Y / 2), this);
            }
        }

        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);


        private void Wpf_PreviewKeyDown(object sender, WpfKeyEventArgs e) {

            SfmlKeyboard.KeyPressed(e.Key);

            if ( KeyPressed == null )
                return;

            KeyPressed(sender, new SfmlKeyEventArgs(e));
        }

        private void Wpf_PreviewKeyUp(object sender, WpfKeyEventArgs e) {

            SfmlKeyboard.KeyReleased(e.Key);

            if ( KeyReleased == null )
                return;

            KeyReleased(sender, new SfmlKeyEventArgs(e));
        }

        private void Wpf_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            SfmlMouse.ButtonReleased(e.ChangedButton);
        }
        private void Wpf_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            SfmlMouse.ButtonPressed(e.ChangedButton);
        }
    }
}
