using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using WpfMouse = System.Windows.Input.Mouse;

namespace SFML {

    public static class SfmlKeyboard {
        private static List<Key> s_keys = new List<Key>();

        public static bool IsKeyPressed(Key key) {
            return s_keys.Contains(key);
        }


        internal static void KeyPressed(Key key) {
            if ( !s_keys.Contains(key) ) {
                s_keys.Add(key);
            }
        }

        internal static void KeyReleased(Key key) {
            if ( s_keys.Contains(key) ) {
                s_keys.Remove(key);
            }
        }
    }

    public static class SfmlMouse {

        private static List<MouseButton> s_mouseButtons = new List<MouseButton>();

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public static Vector2 GetPosition(RenderWindow window) {
            var win32Pt = WpfMouse.GetPosition(window.Element);
            return new Vector2(( int ) win32Pt.X, ( int ) win32Pt.Y);
        }

        public static bool IsButtonPressed(MouseButton mouseButton) {
            return s_mouseButtons.Contains(mouseButton);
        }

        public static void SetPosition(Vector2 pos, RenderWindow window) {
            SetCursorPos(( int ) pos.X + ( int ) window.Position.X, ( int ) pos.Y + ( int ) window.Position.Y);
        }

        internal static void ButtonPressed(MouseButton mouseButton) {
            if (!s_mouseButtons.Contains(mouseButton)) {
                s_mouseButtons.Add(mouseButton);
            }
        }

        internal static void ButtonReleased(MouseButton mouseButton) {
            if (s_mouseButtons.Contains(mouseButton)) {
                s_mouseButtons.Remove(mouseButton);
            }
        }
    }
}