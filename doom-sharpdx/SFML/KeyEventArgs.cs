
using System;
using System.Windows.Input;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace SFML.Window {
    //
    // Summary:
    //     Keyboard event parameters
    public class KeyEventArgs : EventArgs {
        //
        // Summary:
        //     Code of the key (see KeyCode enum)
        public Key Code;

        //
        // Summary:
        //     Is the Alt modifier pressed?
        public bool Alt;

        //
        // Summary:
        //     Is the Control modifier pressed?
        public bool Control;

        //
        // Summary:
        //     Is the Shift modifier pressed?
        public bool Shift;

        //
        // Summary:
        //     Is the System modifier pressed?
        public bool System;

        //
        // Summary:
        //     Construct the key arguments from a key event
        //
        // Parameters:
        //   e:
        //     Key event
        public KeyEventArgs(WpfKeyEventArgs e) {
            Code    = e.Key;
            Alt     = ( Keyboard.Modifiers & ModifierKeys.Alt ) == ModifierKeys.Alt;
            Control = ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control;
            Shift   = ( Keyboard.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift;
            System  = ( Keyboard.Modifiers & ModifierKeys.Windows ) == ModifierKeys.Windows;
        }

        //
        // Summary:
        //     Provide a string describing the object
        //
        // Returns:
        //     String description of the object
        public override string ToString() {
            return string.Concat("[KeyEventArgs] Code(", Code, ") Alt(", Alt.ToString(), ") Control(", Control.ToString(), ") Shift(", Shift.ToString(), ") System(", System.ToString(), ")");
        }
    }
}