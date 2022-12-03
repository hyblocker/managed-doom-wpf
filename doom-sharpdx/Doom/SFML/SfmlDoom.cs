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
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Windows;
using doom_sharpdx;
using SFML.Graphics;
using SFML.Window;

namespace ManagedDoom.SFML
{
    public sealed class SfmlDoom : IDisposable
    {
        private Config config;

        public RenderWindow window;

        private GameContent content;

        public SfmlVideo video;
        private SfmlSound sound;
        private SfmlMusic music;
        private SfmlUserInput userInput;

        private Doom doom;

        public SfmlDoom(CommandLineArgs args, int width = -1, int height = -1, FrameworkElement elem = null)
        {
            config = SfmlConfigUtilities.GetConfig();

            try
            {
                config.video_screenwidth    = (width == -1) ? MathExtensions.Clamp(config.video_screenwidth, 320, 3200)   : width;
                config.video_screenheight   = (height == -1) ? MathExtensions.Clamp(config.video_screenheight, 200, 2000) : height;
                var videoMode = new VideoMode((uint)config.video_screenwidth, (uint)config.video_screenheight);
                // var style = Styles.Close | Styles.Titlebar;
                if (config.video_fullscreen)
                {
                    // style = Styles.Fullscreen;
                }

                // window = new RenderWindow(videoMode, ApplicationInfo.Title, style);
                window = new RenderWindow(videoMode, ApplicationInfo.Title, elem);
                window.Clear(new Vector4(64/ 255f, 64 / 255f, 64 / 255f, 1));
                window.Display();

                content = new GameContent(args);

                video = new SfmlVideo(config, content, window);

                if (!args.nosound.Present && !args.nosfx.Present)
                {
                    sound = new SfmlSound(config, content.Wad);
                }

                if (!args.nosound.Present && !args.nomusic.Present)
                {
                    music = SfmlConfigUtilities.GetMusicInstance(config, content.Wad);
                }

                userInput = new SfmlUserInput(config, window, !args.nomouse.Present);

                // window.Closed += (sender, e) => window.Close();
                window.KeyPressed += KeyPressed;
                window.KeyReleased += KeyReleased;

                if (!args.timedemo.Present)
                {
                    // window.SetFramerateLimit(35);
                }

                doom = new Doom(args, config, content, video, sound, music, userInput);
            }
            catch (Exception e)
            {
                Dispose();
                throw e;
            }
        }

        public void Run()
        {
            while (window.IsOpen)
            {
                // window.DispatchEvents();

                if ( OnFrame() )
                    break;
            }

            config.Save(ConfigUtilities.GetConfigPath());
        }

        // Returns whether to kill the game loop or not
        public bool OnFrame() {

            if ( doom.Update() == UpdateResult.Completed ) {
                return true;
            }

            video.Render(doom);
            return false;
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            doom.PostEvent(new DoomEvent(EventType.KeyDown, SfmlUserInput.MapWpfKeyToDoomKey(e.Code)));
        }

        private void KeyReleased(object sender, KeyEventArgs e)
        {
            doom.PostEvent(new DoomEvent(EventType.KeyUp, SfmlUserInput.MapWpfKeyToDoomKey(e.Code)));
        }

        public void Dispose()
        {
            if (userInput != null)
            {
                userInput.Dispose();
                userInput = null;
            }

            if (music != null)
            {
                music.Dispose();
                music = null;
            }

            if (sound != null)
            {
                sound.Dispose();
                sound = null;
            }

            if (video != null)
            {
                video.Dispose();
                video = null;
            }

            if (content != null)
            {
                content.Dispose();
                content = null;
            }

            if (window != null)
            {
                window.Dispose();
                window = null;
            }
        }

        public string QuitMessage => doom.QuitMessage;
    }
}
