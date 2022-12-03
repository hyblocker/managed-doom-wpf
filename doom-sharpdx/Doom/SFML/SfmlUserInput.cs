//
// Copyright (C) 1993-1996 Id Software, Inc.
// Copyright (C) 2019-2020 Nobuaki Tanaka
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//



using System;
using System.Runtime.ExceptionServices;
// using SFML.Graphics;
using SFML.System;
using SFML.Window;
using ManagedDoom.UserInput;
using System.Windows.Input;
using SFML.Graphics;
using System.Numerics;
using SFML;
using System.Windows.Media;
using WpfPoint = System.Windows.Point;

namespace ManagedDoom.SFML
{
    public sealed class SfmlUserInput : IUserInput, IDisposable
    {
        private Config config;

        private RenderWindow window;

        private bool useMouse;

        private bool[] weaponKeys;
        private int turnHeld;

        private bool mouseGrabbed;
        private int windowCenterX;
        private int windowCenterY;
        private int mouseX;
        private int mouseY;
        private bool cursorCentered;

        public SfmlUserInput(Config config, RenderWindow window, bool useMouse)
        {
            try
            {
                Console.Write("Initialize user input: ");

                this.config = config;

                config.mouse_sensitivity = Math.Max(config.mouse_sensitivity, 0);

                this.window = window;

                this.useMouse = useMouse;

                weaponKeys = new bool[7];
                turnHeld = 0;

                mouseGrabbed = false;
                WpfPoint elemPos =  window.Element.PointToScreen(new WpfPoint(0, 0));
                windowCenterX = (int)window.Element.ActualWidth / 2 + ( int ) elemPos.X;
                windowCenterY = ( int ) window.Element.ActualHeight / 2 + ( int ) elemPos.Y;
                mouseX = 0;
                mouseY = 0;
                cursorCentered = false;

                Console.WriteLine("OK");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed");
                Dispose();
                throw e;
            }
        }

        public void BuildTicCmd(TicCmd cmd)
        {
            var keyForward = IsPressed(config.key_forward);
            var keyBackward = IsPressed(config.key_backward);
            var keyStrafeLeft = IsPressed(config.key_strafeleft);
            var keyStrafeRight = IsPressed(config.key_straferight);
            var keyTurnLeft = IsPressed(config.key_turnleft);
            var keyTurnRight = IsPressed(config.key_turnright);
            var keyFire = IsPressed(config.key_fire);
            var keyUse = IsPressed(config.key_use);
            var keyRun = IsPressed(config.key_run);
            var keyStrafe = IsPressed(config.key_strafe);

            weaponKeys[0] = SfmlKeyboard.IsKeyPressed(Key.NumPad1);
            weaponKeys[1] = SfmlKeyboard.IsKeyPressed(Key.NumPad2);
            weaponKeys[2] = SfmlKeyboard.IsKeyPressed(Key.NumPad3);
            weaponKeys[3] = SfmlKeyboard.IsKeyPressed(Key.NumPad4);
            weaponKeys[4] = SfmlKeyboard.IsKeyPressed(Key.NumPad5);
            weaponKeys[5] = SfmlKeyboard.IsKeyPressed(Key.NumPad6);
            weaponKeys[6] = SfmlKeyboard.IsKeyPressed(Key.NumPad7);

            cmd.Clear();

            var strafe = keyStrafe;
            var speed = keyRun ? 1 : 0;
            var forward = 0;
            var side = 0;

            if (config.game_alwaysrun)
            {
                speed = 1 - speed;
            }

            if (keyTurnLeft || keyTurnRight)
            {
                turnHeld++;
            }
            else
            {
                turnHeld = 0;
            }

            int turnSpeed;
            if (turnHeld < PlayerBehavior.SlowTurnTics)
            {
                turnSpeed = 2;
            }
            else
            {
                turnSpeed = speed;
            }

            if (strafe)
            {
                if (keyTurnRight)
                {
                    side += PlayerBehavior.SideMove[speed];
                }
                if (keyTurnLeft)
                {
                    side -= PlayerBehavior.SideMove[speed];
                }
            }
            else
            {
                if (keyTurnRight)
                {
                    cmd.AngleTurn -= (short)PlayerBehavior.AngleTurn[turnSpeed];
                }
                if (keyTurnLeft)
                {
                    cmd.AngleTurn += (short)PlayerBehavior.AngleTurn[turnSpeed];
                }
            }

            if (keyForward)
            {
                forward += PlayerBehavior.ForwardMove[speed];
            }
            if (keyBackward)
            {
                forward -= PlayerBehavior.ForwardMove[speed];
            }

            if (keyStrafeLeft)
            {
                side -= PlayerBehavior.SideMove[speed];
            }
            if (keyStrafeRight)
            {
                side += PlayerBehavior.SideMove[speed];
            }

            if (keyFire)
            {
                cmd.Buttons |= TicCmdButtons.Attack;
            }

            if (keyUse)
            {
                cmd.Buttons |= TicCmdButtons.Use;
            }

            // Check weapon keys.
            for (var i = 0; i < weaponKeys.Length; i++)
            {
                if (weaponKeys[i])
                {
                    cmd.Buttons |= TicCmdButtons.Change;
                    cmd.Buttons |= (byte)(i << TicCmdButtons.WeaponShift);
                    break;
                }
            }

            UpdateMouse();
            var ms = 0.5F * config.mouse_sensitivity;
            var mx = (int)Math.Round(ms * mouseX);
            var my = (int)Math.Round(ms * mouseY);
            forward += my;
            if (strafe)
            {
                side += mx * 2;
            }
            else
            {
                cmd.AngleTurn -= (short)(mx * 0x8);
            }

            if (forward > PlayerBehavior.MaxMove)
            {
                forward = PlayerBehavior.MaxMove;
            }
            else if (forward < -PlayerBehavior.MaxMove)
            {
                forward = -PlayerBehavior.MaxMove;
            }
            if (side > PlayerBehavior.MaxMove)
            {
                side = PlayerBehavior.MaxMove;
            }
            else if (side < -PlayerBehavior.MaxMove)
            {
                side = -PlayerBehavior.MaxMove;
            }

            cmd.ForwardMove += (sbyte)forward;
            cmd.SideMove += (sbyte)side;
        }

        private bool IsPressed(KeyBinding keyBinding)
        {
            foreach (var key in keyBinding.Keys)
            {
                if (SfmlKeyboard.IsKeyPressed(MapDoomKeyToWpfKey(key)))
                {
                    return true;
                }
            }

            if (mouseGrabbed)
            {
                foreach (var mouseButton in keyBinding.MouseButtons)
                {
                    if ( SfmlMouse.IsButtonPressed(MapDoomMouseToWpfMouse(mouseButton)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Reset()
        {
            mouseX = 0;
            mouseY = 0;
            cursorCentered = false;
        }

        public void GrabMouse()
        {
            if (useMouse && !mouseGrabbed)
            {
                window.SetMouseCursorGrabbed(true);
                window.SetMouseCursorVisible(false);
                mouseGrabbed = true;
                mouseX = 0;
                mouseY = 0;
                cursorCentered = false;
            }
        }

        public void ReleaseMouse()
        {
            if (useMouse && mouseGrabbed)
            {
                var posX = (int)(0.9 * window.Size.X);
                var posY = (int)(0.9 * window.Size.Y);
                SfmlMouse.SetPosition(new Vector2(posX, posY), window);
                window.SetMouseCursorGrabbed(false);
                window.SetMouseCursorVisible(true);
                mouseGrabbed = false;
            }
        }

        private void UpdateMouse()
        {
            if (mouseGrabbed)
            {
                if (cursorCentered)
                {
                    var current = SfmlMouse.GetPosition(window);

                    mouseX = (int)current.X - windowCenterX;

                    if (config.mouse_disableyaxis)
                    {
                        mouseY = 0;
                    }
                    else
                    {
                        mouseY = ( int ) -(current.Y - windowCenterY);
                    }
                }
                else
                {
                    mouseX = 0;
                    mouseY = 0;
                }

                SfmlMouse.SetPosition(new Vector2(windowCenterX, windowCenterY), window);
                var pos = SfmlMouse.GetPosition(window);
                cursorCentered = (pos.X == windowCenterX && pos.Y == windowCenterY);
            }
            else
            {
                mouseX = 0;
                mouseY = 0;
            }
        }

        public void Dispose()
        {
            Console.WriteLine("Shutdown user input.");

            ReleaseMouse();
        }

        public int MaxMouseSensitivity
        {
            get
            {
                return 15;
            }
        }

        public int MouseSensitivity
        {
            get
            {
                return config.mouse_sensitivity;
            }

            set
            {
                config.mouse_sensitivity = value;
            }
        }

        public static Key MapDoomKeyToWpfKey(DoomKey key) {
            switch (key) {
                case DoomKey.A:
                    return Key.A;
                case DoomKey.B:
                    return Key.B;
                case DoomKey.C:
                    return Key.C;
                case DoomKey.D:
                    return Key.D;
                case DoomKey.E:
                    return Key.E;
                case DoomKey.F:
                    return Key.F;
                case DoomKey.G:
                    return Key.G;
                case DoomKey.H:
                    return Key.H;
                case DoomKey.I:
                    return Key.I;
                case DoomKey.J:
                    return Key.J;
                case DoomKey.K:
                    return Key.K;
                case DoomKey.L:
                    return Key.L;
                case DoomKey.M:
                    return Key.M;
                case DoomKey.N:
                    return Key.N;
                case DoomKey.O:
                    return Key.O;
                case DoomKey.P:
                    return Key.P;
                case DoomKey.Q:
                    return Key.Q;
                case DoomKey.R:
                    return Key.R;
                case DoomKey.S:
                    return Key.S;
                case DoomKey.T:
                    return Key.T;
                case DoomKey.U:
                    return Key.U;
                case DoomKey.V:
                    return Key.V;
                case DoomKey.W:
                    return Key.W;
                case DoomKey.X:
                    return Key.X;
                case DoomKey.Y:
                    return Key.Y;
                case DoomKey.Z:
                    return Key.Z;

                case DoomKey.Num0:
                    return Key.D0;
                case DoomKey.Num1:
                    return Key.D1;
                case DoomKey.Num2:
                    return Key.D2;
                case DoomKey.Num3:
                    return Key.D3;
                case DoomKey.Num4:
                    return Key.D4;
                case DoomKey.Num5:
                    return Key.D5;
                case DoomKey.Num6:
                    return Key.D6;
                case DoomKey.Num7:
                    return Key.D7;
                case DoomKey.Num8:
                    return Key.D8;
                case DoomKey.Num9:
                    return Key.D9;

                case DoomKey.F1:
                    return Key.F1;
                case DoomKey.F2:
                    return Key.F2;
                case DoomKey.F3:
                    return Key.F3;
                case DoomKey.F4:
                    return Key.F4;
                case DoomKey.F5:
                    return Key.F5;
                case DoomKey.F6:
                    return Key.F6;
                case DoomKey.F7:
                    return Key.F7;
                case DoomKey.F8:
                    return Key.F8;
                case DoomKey.F9:
                    return Key.F9;
                case DoomKey.F10:
                    return Key.F10;
                case DoomKey.F11:
                    return Key.F11;
                case DoomKey.F12:
                    return Key.F12;
                case DoomKey.F13:
                    return Key.F13;
                case DoomKey.F14:
                    return Key.F14;
                case DoomKey.F15:
                    return Key.F15;

                case DoomKey.Space:
                    return Key.Space;
                case DoomKey.Enter:
                    return Key.Return;

                case DoomKey.Left:
                    return Key.Left;
                case DoomKey.Right:
                    return Key.Right;
                case DoomKey.Up:
                    return Key.Up;
                case DoomKey.Down:
                    return Key.Down;

                case DoomKey.Backspace:
                    return Key.Back;

                case DoomKey.Backslash:
                    return Key.OemBackslash;

                case DoomKey.LControl:
                    return Key.LeftCtrl;
                case DoomKey.RControl:
                    return Key.RightCtrl;
                case DoomKey.LShift:
                    return Key.LeftShift;
                case DoomKey.RShift:
                    return Key.RightShift;
                case DoomKey.LAlt:
                    return Key.LeftAlt;
                case DoomKey.RAlt:
                    return Key.RightAlt;

                case DoomKey.Hyphen:
                    return Key.OemMinus;
                case DoomKey.Equal:
                    return Key.OemPlus;
                case DoomKey.Slash:
                    return Key.OemQuestion;
                case DoomKey.LBracket:
                    return Key.OemOpenBrackets;
                case DoomKey.RBracket:
                    return Key.OemCloseBrackets;
                case DoomKey.Period:
                    return Key.OemPeriod;
                case DoomKey.Comma:
                    return Key.OemComma;
                case DoomKey.Semicolon:
                    return Key.OemSemicolon;
                case DoomKey.Quote:
                    return Key.OemQuotes;
                case DoomKey.Tilde:
                    return Key.OemTilde;
                case DoomKey.Tab:
                    return Key.Tab;
                case DoomKey.Delete:
                    return Key.Delete;

                case DoomKey.PageDown:
                    return Key.PageDown;
                case DoomKey.PageUp:
                    return Key.PageUp;
                case DoomKey.Home:
                    return Key.Home;
                case DoomKey.End:
                    return Key.End;

                case DoomKey.Escape:
                    return Key.Escape;

                default:
                    return (Key)key;
            }
        }

        public static MouseButton MapDoomMouseToWpfMouse(DoomMouseButton mouseButton) {
            switch (mouseButton) {
                case DoomMouseButton.Mouse1:
                    return MouseButton.Left;
                case DoomMouseButton.Mouse2:
                    return MouseButton.Right;
                case DoomMouseButton.Mouse3:
                    return MouseButton.Middle;
                case DoomMouseButton.Mouse4:
                    return MouseButton.XButton1;
                case DoomMouseButton.Mouse5:
                    return MouseButton.XButton2;
                default:
                    return (MouseButton)mouseButton;
            }
        }

        public static DoomMouseButton MapWpfMouseToDoomMouse(MouseButton mouseButton) {
            switch (mouseButton) {
                case MouseButton.Left:
                    return DoomMouseButton.Mouse1;
                case MouseButton.Right:
                    return DoomMouseButton.Mouse2;
                case MouseButton.Middle:
                    return DoomMouseButton.Mouse3;
                case MouseButton.XButton1:
                    return DoomMouseButton.Mouse4;
                case MouseButton.XButton2:
                    return DoomMouseButton.Mouse5;
                default:
                    return (DoomMouseButton)mouseButton;
            }
        }


        public static DoomKey MapWpfKeyToDoomKey(Key key) {
            switch ( key ) {
                case Key.A:
                    return DoomKey.A;
                case Key.B:
                    return DoomKey.B;
                case Key.C:
                    return DoomKey.C;
                case Key.D:
                    return DoomKey.D;
                case Key.E:
                    return DoomKey.E;
                case Key.F:
                    return DoomKey.F;
                case Key.G:
                    return DoomKey.G;
                case Key.H:
                    return DoomKey.H;
                case Key.I:
                    return DoomKey.I;
                case Key.J:
                    return DoomKey.J;
                case Key.K:
                    return DoomKey.K;
                case Key.L:
                    return DoomKey.L;
                case Key.M:
                    return DoomKey.M;
                case Key.N:
                    return DoomKey.N;
                case Key.O:
                    return DoomKey.O;
                case Key.P:
                    return DoomKey.P;
                case Key.Q:
                    return DoomKey.Q;
                case Key.R:
                    return DoomKey.R;
                case Key.S:
                    return DoomKey.S;
                case Key.T:
                    return DoomKey.T;
                case Key.U:
                    return DoomKey.U;
                case Key.V:
                    return DoomKey.V;
                case Key.W:
                    return DoomKey.W;
                case Key.X:
                    return DoomKey.X;
                case Key.Y:
                    return DoomKey.Y;
                case Key.Z:
                    return DoomKey.Z;

                case Key.D0:
                    return DoomKey.Num0;
                case Key.D1:
                    return DoomKey.Num1;
                case Key.D2:
                    return DoomKey.Num2;
                case Key.D3:
                    return DoomKey.Num3;
                case Key.D4:
                    return DoomKey.Num4;
                case Key.D5:
                    return DoomKey.Num5;
                case Key.D6:
                    return DoomKey.Num6;
                case Key.D7:
                    return DoomKey.Num7;
                case Key.D8:
                    return DoomKey.Num8;
                case Key.D9:
                    return DoomKey.Num9;

                case Key.F1:
                    return DoomKey.F1;
                case Key.F2:
                    return DoomKey.F2;
                case Key.F3:
                    return DoomKey.F3;
                case Key.F4:
                    return DoomKey.F4;
                case Key.F5:
                    return DoomKey.F5;
                case Key.F6:
                    return DoomKey.F6;
                case Key.F7:
                    return DoomKey.F7;
                case Key.F8:
                    return DoomKey.F8;
                case Key.F9:
                    return DoomKey.F9;
                case Key.F10:
                    return DoomKey.F10;
                case Key.F11:
                    return DoomKey.F11;
                case Key.F12:
                    return DoomKey.F12;
                case Key.F13:
                    return DoomKey.F13;
                case Key.F14:
                    return DoomKey.F14;
                case Key.F15:
                    return DoomKey.F15;

                case Key.Space:
                    return DoomKey.Space;
                case Key.Return:
                    return DoomKey.Enter;

                case Key.Left:
                    return DoomKey.Left;
                case Key.Right:
                    return DoomKey.Right;
                case Key.Up:
                    return DoomKey.Up;
                case Key.Down:
                    return DoomKey.Down;

                case Key.Back:
                    return DoomKey.Backspace;

                case Key.OemBackslash:
                    return DoomKey.Backslash;

                case Key.LeftCtrl:
                    return DoomKey.LControl;
                case Key.RightCtrl:
                    return DoomKey.RControl;
                case Key.LeftShift:
                    return DoomKey.LShift;
                case Key.RightShift:
                    return DoomKey.RShift;
                case Key.LeftAlt:
                    return DoomKey.LAlt;
                case Key.RightAlt:
                    return DoomKey.RAlt;

                case Key.OemMinus:
                    return DoomKey.Hyphen;
                case Key.OemPlus:
                    return DoomKey.Equal;
                case Key.OemQuestion:
                    return DoomKey.Slash;
                case Key.OemOpenBrackets:
                    return DoomKey.LBracket;
                case Key.OemCloseBrackets:
                    return DoomKey.RBracket;
                case Key.OemPeriod:
                    return DoomKey.Period;
                case Key.OemComma:
                    return DoomKey.Comma;
                case Key.OemSemicolon:
                    return DoomKey.Semicolon;
                case Key.OemQuotes:
                    return DoomKey.Quote;
                case Key.OemTilde:
                    return DoomKey.Tilde;
                case Key.Tab:
                    return DoomKey.Tab;
                case Key.Delete:
                    return DoomKey.Delete;

                case Key.PageDown:
                    return DoomKey.PageDown;
                case Key.PageUp:
                    return DoomKey.PageUp;
                case Key.Home:
                    return DoomKey.Home;
                case Key.End:
                    return DoomKey.End;

                case Key.Escape:
                    return DoomKey.Escape;

                default:
                    return ( DoomKey ) key;
            }
        }
    }
}
