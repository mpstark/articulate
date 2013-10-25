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
	[StructLayout(LayoutKind.Sequential)]
	struct MOUSEINPUT
	{
		public int DX;
		public int DY;
		public uint Data;
		public uint Flags;
		public uint Time;
		public UIntPtr ExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct KEYBOARDINPUT
	{
		public ushort VirtualKey;
		public ushort ScanCode;
		public uint Flags;
		public uint Time;
		public UIntPtr ExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct HARDWAREINPUT
	{
		public uint Message;
		public ushort ParamL;
		public ushort ParamH;
	}

	[StructLayout(LayoutKind.Explicit, Size=28)]
	struct INPUT
	{
		[FieldOffset(0)]
		public UInt32 Type;

		//MOUSEINPUT
		[FieldOffset(4)]
		public MOUSEINPUT Mouse;

		[FieldOffset(4)]
		public KEYBOARDINPUT Keyboard;

		[FieldOffset(4)]
		public HARDWAREINPUT Hardware;
	}
	
	/// <summary>
	/// Stores the ushort keycodes for DirectInput. These vary per keyboard some.
	/// 
	/// More information about these is available at:
	/// http://www.gamespp.com/directx/directInputKeyboardScanCodes.html
	/// </summary>
	public enum DirectInputKeys : ushort
	{
		One = 0x02,
        Two = 0x03,
        Three = 0x04,
        Four = 0x05,
        Five = 0x06,
        Six = 0x07,
        Seven = 0x08,
		Eight = 0x09,
        Nine = 0x0A,
        Ten = 0x0B,

		Tilde = 0x29,

		F1 = 0x3B,
		F2 = 0x3C,
		F3 = 0x3D,
		F4 = 0x3E,
		F5 = 0x3F,
		F6 = 0x40,
		F7 = 0x41,
		F8 = 0x42,
		F9 = 0x43,
		F10 = 0x44,
		F11 = 0x57,
		F12 = 0x58
	}

    /// <summary>
    /// Exposes the SendInput method from the User32/Win32 library. Using this instead of other options because
    /// this allows input into DirectInput applications (i.e. Arma 3 and other applications that use DirectX).
    /// </summary>
    static class DirectInputEmulator
    {
        [DllImport("user32.dll")]
        static extern UInt32 SendInput(UInt32 nInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] pInputs, Int32 cbSize);

        enum SendInputFlags
        {
            KEYEVENTF_EXTENDEDKEY = 0x0001,
            KEYEVENTF_KEYUP = 0x0002,
            KEYEVENTF_UNICODE = 0x0004,
            KEYEVENTF_SCANCODE = 0x0008,
        }
		
		/// <summary>
		/// Sends a series of input operations using Direct Input
		/// </summary>
		/// <param name="inputs">The <see cref="INPUT"/> arrays representing operations to perform</param>
		/// <param name="delay">The delay to wait between each keypress.</param>
		public static void SendInput(int delay = 0, params INPUT[][] inputs)
		{
			foreach (var input in inputs)
			{
				SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(INPUT)));

                if (delay > 0)
                    Thread.Sleep(delay);
			}
		}

		/// <summary>
		/// Sends a series of input operations using Direct Input
		/// </summary>
		/// <param name="inputs">The <see cref="INPUT"/> arrays representing operations to perform</param>
		/// <param name="delay">The delay to wait between each keypress.</param>
		public static void SendInput(IEnumerable<INPUT[]> inputs, int delay = 0)
		{
			foreach (var input in inputs)
			{
				var result = SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(INPUT)));

				Trace.WriteLine("SendInput Result: " + result);

				Marshal.ThrowExceptionForHR((int)result);

				if (delay > 0)
					Thread.Sleep(delay);
			}
		}
		
		/// <summary>
		/// Creates a set of input operations to emulate pressing the given keys
		/// </summary>
		/// <param name="keys">The <see cref="DirectInputKeys"/> to emulate pressing</param>
		/// <returns>Returns an array of <see cref="INPUT"/> objects for use by <see cref="SendInput"/></returns>
		public static INPUT[] KeyDown(params DirectInputKeys[] keys)
		{
			return keys.Select(key =>
			{
				var input = new INPUT();
				input.Type = 1; //KEYBOARD_INPUT
				input.Keyboard.ScanCode = (ushort)key;
				input.Keyboard.Flags = (uint)SendInputFlags.KEYEVENTF_SCANCODE;
				input.Keyboard.Time = 0;
				input.Keyboard.ExtraInfo = UIntPtr.Zero;

				return input;
			}).ToArray();
		}

		/// <summary>
		/// Creates a set of input operations to emulate releasing the given keys
		/// </summary>
		/// <param name="keys">The <see cref="DirectInputKeys"/> to emulate releasing</param>
		/// <returns>Returns an array of <see cref="INPUT"/> objects for use by <see cref="SendInput"/></returns>
		public static INPUT[] KeyUp(params DirectInputKeys[] keys)
		{
			return keys.Select(key =>
			{
				var input = new INPUT();
				input.Type = 1; //KEYBOARD_INPUT
				input.Keyboard.ScanCode = (ushort)key;
				input.Keyboard.Flags = (uint)(SendInputFlags.KEYEVENTF_SCANCODE | SendInputFlags.KEYEVENTF_KEYUP);
				input.Keyboard.Time = 0;
				input.Keyboard.ExtraInfo = UIntPtr.Zero;

				return input;
			}).ToArray();
		}

		/// <summary>
		/// Creates a set of input operations to emulate pressing and releasing the given keys
		/// </summary>
		/// <param name="keys">The <see cref="DirectInputKeys"/> to emulate pressing and releasing</param>
		/// <returns>Returns an array of <see cref="INPUT"/> objects for use by <see cref="SendInput"/></returns>
		public static INPUT[] KeyPress(params DirectInputKeys[] keys)
		{
			return keys.Select(key =>
			{
				var input = new INPUT();
				input.Type = 1; //KEYBOARD_INPUT
				input.Keyboard.ScanCode = (ushort)key;
				input.Keyboard.Flags = (uint)SendInputFlags.KEYEVENTF_SCANCODE;
				input.Keyboard.Time = 0;
				input.Keyboard.ExtraInfo = UIntPtr.Zero;

				return input;
			}).Concat(
				keys.Reverse().Select(key =>
				{
					var input = new INPUT();
					input.Type = 1; //KEYBOARD_INPUT
					input.Keyboard.ScanCode = (ushort)key;
					input.Keyboard.Flags = (uint)(SendInputFlags.KEYEVENTF_SCANCODE | SendInputFlags.KEYEVENTF_KEYUP);
					input.Keyboard.Time = 0;
					input.Keyboard.ExtraInfo = UIntPtr.Zero;

					return input;
				})
			).ToArray();
		}

		/// <summary>
		/// Creates a set of input operations to emulate pressing the given mouse buttons
		/// </summary>
		/// <param name="buttons">The <see cref="System.Windows.Forms.MouseButtons"/> to emulate pressing</param>
		/// <returns>Returns an array of <see cref="INPUT"/> objects for use by <see cref="SendInput"/></returns>
		public static INPUT[] MouseDown(params System.Windows.Forms.MouseButtons[] buttons)
		{
			var input = new INPUT[buttons.Length];

			for (int i = 0; i < buttons.Length; i++)
			{
				input[i].Type = 0; // MOUSE_INPUT
				input[i].Mouse.Time = 0;
				input[i].Mouse.Data = 0;

				switch (buttons[i])
				{
					case System.Windows.Forms.MouseButtons.Left:
						input[i].Mouse.Flags = 0x2;
						break;
					case System.Windows.Forms.MouseButtons.Right:
						input[i].Mouse.Flags = 0x8;
						break;
					case System.Windows.Forms.MouseButtons.Middle:
						input[i].Mouse.Flags = 0x20;
						break;
					case System.Windows.Forms.MouseButtons.XButton1:
						input[i].Mouse.Flags = 0x80;
						input[i].Mouse.Data = 0x1;
						break;

					case System.Windows.Forms.MouseButtons.XButton2:
						input[i].Mouse.Flags = 0x80;
						input[i].Mouse.Data = 0x2;
						break;
				}

			}
			return input;
		}

		/// <summary>
		/// Creates a set of input operations to emulate releasing the given mouse buttons
		/// </summary>
		/// <param name="buttons">The <see cref="System.Windows.Forms.MouseButtons"/> to emulate releasing</param>
		/// <returns>Returns an array of <see cref="INPUT"/> objects for use by <see cref="SendInput"/></returns>
		public static INPUT[] MouseUp(params System.Windows.Forms.MouseButtons[] buttons)
		{
			var input = new INPUT[buttons.Length];

			for (int i = 0; i < buttons.Length; i++)
			{
				input[i].Type = 0; // MOUSE_INPUT
				input[i].Mouse.Time = 0;
				input[i].Mouse.Data = 0;

				switch (buttons[i])
				{
					case System.Windows.Forms.MouseButtons.Left:
						input[i].Mouse.Flags = 0x2 << 1;
						break;
					case System.Windows.Forms.MouseButtons.Right:
						input[i].Mouse.Flags = 0x8 << 1;
						break;
					case System.Windows.Forms.MouseButtons.Middle:
						input[i].Mouse.Flags = 0x20 << 1;
						break;
					case System.Windows.Forms.MouseButtons.XButton1:
						input[i].Mouse.Flags = 0x80 << 1;
						input[i].Mouse.Data = 0x1;
						break;

					case System.Windows.Forms.MouseButtons.XButton2:
						input[i].Mouse.Flags = 0x80 << 1;
						input[i].Mouse.Data = 0x2;
						break;
				}

			}
			return input;
		}

		/// <summary>
		/// Creates a set of input operations to emulate pressing and releasing the given mouse buttons
		/// </summary>
		/// <param name="buttons">The <see cref="System.Windows.Forms.MouseButtons"/> to emulate pressing and releasing</param>
		/// <returns>Returns an array of <see cref="INPUT"/> objects for use by <see cref="SendInput"/></returns>
		public static INPUT[] MousePress(params System.Windows.Forms.MouseButtons[] buttons)
		{
			var input = new INPUT[2 * buttons.Length];

			for (int i = 0, j = input.Length - 1; i < buttons.Length; i++, j--)
			{
				input[i].Type = 0; // MOUSE_INPUT
				input[i].Mouse.Time = 0;
				input[i].Mouse.Data = 0;

				input[j].Type = 0; // MOUSE_INPUT
				input[j].Mouse.Time = 0;
				input[j].Mouse.Data = 0;

				switch (buttons[i])
				{
					case System.Windows.Forms.MouseButtons.Left:
						input[i].Mouse.Flags = 0x2;
						input[j].Mouse.Flags = 0x2 << 1;
						break;
					case System.Windows.Forms.MouseButtons.Right:
						input[i].Mouse.Flags = 0x8;
						input[j].Mouse.Flags = 0x8 << 1;
						break;
					case System.Windows.Forms.MouseButtons.Middle:
						input[i].Mouse.Flags = 0x20;
						input[j].Mouse.Flags = 0x20 << 1;
						break;
					case System.Windows.Forms.MouseButtons.XButton1:
						input[i].Mouse.Flags = 0x80;
						input[i].Mouse.Data = 0x1;
						input[j].Mouse.Flags = 0x80 << 1;
						input[j].Mouse.Data = 0x1;
						break;

					case System.Windows.Forms.MouseButtons.XButton2:
						input[i].Mouse.Flags = 0x80;
						input[i].Mouse.Data = 0x2;
						input[j].Mouse.Flags = 0x80 << 1;
						input[j].Mouse.Data = 0x2;
						break;
				}

			}
			return input;
		}

    }
}
