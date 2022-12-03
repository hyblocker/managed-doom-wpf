using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFML.Window {
    public struct VideoMode {
        public uint Width;

        public uint Height;

        public uint BitsPerPixel;

        public static VideoMode DesktopMode => sfVideoMode_getDesktopMode();

        public VideoMode(uint width, uint height)
            : this(width, height, 32u) {
        }

        public VideoMode(uint width, uint height, uint bpp) {
            Width = width;
            Height = height;
            BitsPerPixel = bpp;
        }

        private static VideoMode sfVideoMode_getDesktopMode() {
            VideoMode vm = new VideoMode();

            // @TODO: Stuff

            return vm;
        }
    }
}
