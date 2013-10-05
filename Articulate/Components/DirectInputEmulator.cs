using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Articulate
{
    /// <summary>
    /// Stores the ushort keycodes for DirectInput. These vary per keyboard some.
    /// 
    /// More information about these is available at:
    /// http://www.gamespp.com/directx/directInputKeyboardScanCodes.html
    /// </summary>
    static class Keys
    {
        public const ushort F1 = 0x3B;
        public const ushort F2 = 0x3C;
        public const ushort F3 = 0x3D;
        public const ushort F4 = 0x3E;
        public const ushort F5 = 0x3F;
        public const ushort F6 = 0x40;
        public const ushort F7 = 0x41;
        public const ushort F8 = 0x42;
        public const ushort F9 = 0x43;
        public const ushort F10 = 0x44;
        public const ushort F11 = 0x57;
        public const ushort F12 = 0x58;

        public const ushort One = 0x02;
        public const ushort Two = 0x03;
        public const ushort Three = 0x04;
        public const ushort Four = 0x05;
        public const ushort Five = 0x06;
        public const ushort Six = 0x07;
        public const ushort Seven = 0x08;
        public const ushort Eight = 0x09;
        public const ushort Nine = 0x0A;
        public const ushort Ten = 0x0B;

        public const ushort Tilde = 0x29;
    }

    /// <summary>
    /// Exposes the SendInput method from the User32/Win32 library. Using this instead of other options because
    /// this allows input into DirectInput applications (i.e. Arma 3 and other applications that use DirectX).
    /// </summary>
    static class DirectInputEmulator
    {
        [DllImport("user32.dll")]
        static extern UInt32 SendInput(UInt32 nInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] pInputs, Int32 cbSize);

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public UInt32 Type;
            //KEYBDINPUT:
            public ushort Vk;
            public ushort Scan;
            public UInt32 Flags;
            public UInt32 Time;
            public UIntPtr ExtraInfo;

            //HARDWAREINPUT:
            public UInt32 uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        enum SendInputFlags
        {
            KEYEVENTF_EXTENDEDKEY = 0x0001,
            KEYEVENTF_KEYUP = 0x0002,
            KEYEVENTF_UNICODE = 0x0004,
            KEYEVENTF_SCANCODE = 0x0008,
        }

        /// <summary>
        /// Sends a series of keypresses in order from a List with an optional delay. This is a blocking operation.
        /// </summary>
        /// <param name="keys">The keypresses to send.</param>
        /// <param name="delay">The delay to wait between each keypress.</param>
        public static void SendKeyPresses(List<ushort> keys, int delay = 0)
        {
            foreach (ushort key in keys)
            {
                SendKeyPress(key);

                if (delay > 0)
                    Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// Sends a single keypress (Down and then up)
        /// </summary>
        /// <param name="key">The key to press</param>
        public static void SendKeyPress(ushort key)
        {
            INPUT[] InputData = new INPUT[2];

            InputData[0].Type = 1; //INPUT_KEYBOARD
            InputData[0].Scan = (ushort)key;
            InputData[0].Flags = (uint)SendInputFlags.KEYEVENTF_SCANCODE;
            InputData[0].Time = 0;
            InputData[0].ExtraInfo = UIntPtr.Zero;

            InputData[1].Type = 1; //INPUT_KEYBOARD
            InputData[1].Scan = (ushort)key;
            InputData[1].Flags = (uint)(SendInputFlags.KEYEVENTF_SCANCODE | SendInputFlags.KEYEVENTF_KEYUP);
            InputData[1].Time = 0;
            InputData[1].ExtraInfo = UIntPtr.Zero;

            // send keydown
            SendInput(2, InputData, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>
        /// Sends a key down for a single key
        /// </summary>
        /// <param name="key">Key to send key down on</param>
        public static void SendKeyDown(ushort key)
        {
            INPUT[] InputData = new INPUT[1];

            InputData[0].Type = 1; //INPUT_KEYBOARD
            InputData[0].Scan = (ushort)key;
            InputData[0].Flags = (uint)SendInputFlags.KEYEVENTF_SCANCODE;
            InputData[0].Time = 0;
            InputData[0].ExtraInfo = UIntPtr.Zero;

            // send keydown
            SendInput(1, InputData, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>
        /// Sends a key up for a single key
        /// </summary>
        /// <param name="key">Key to send key up on</param>
        public static void SendKeyUp(ushort key)
        {
            INPUT[] InputData = new INPUT[1];

            InputData[0].Type = 1; //INPUT_KEYBOARD
            InputData[0].Scan = (ushort)key;
            InputData[0].Flags = (uint)(SendInputFlags.KEYEVENTF_SCANCODE | SendInputFlags.KEYEVENTF_KEYUP);
            InputData[0].Time = 0;
            InputData[0].ExtraInfo = UIntPtr.Zero;

            // send keydown
            SendInput(1, InputData, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
