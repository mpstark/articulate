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
		Escape = 0x01,
		One = 0x02,
        Two = 0x03,
        Three = 0x04,
        Four = 0x05,
        Five = 0x06,
        Six = 0x07,
        Seven = 0x08,
		Eight = 0x09,
        Nine = 0x0A,
        Zero = 0x0B,
		Minus = 0x0C,
		Equals = 0x0D,
		Backspace = 0x0E,
		Tab = 0x0F,
		Q = 0x10,
		W = 0x11,
		E = 0x12,
		R = 0x13,
		T = 0x14,
		Y = 0x15,
		U = 0x16,
		I = 0x17,
		O = 0x18,
		P = 0x19,
		LBracket = 0x1A,
		RBracket = 0x1B,
		Enter = 0x1C,
		LControl = 0x1D,
		A = 0x1E,
		S = 0x1F,
		D = 0x20,
		F = 0x21,
		G = 0x22,
		H = 0x23,
		J = 0x24,
		K = 0x25,
		L = 0x26,
		Semicolon = 0x27,
		Apostrophe = 0x28,
		Tilde = 0x29,
		LShift = 0x2A, Shift = LShift,
		Backslash = 0x2B,
		Z = 0x2C,
		X = 0x2D,
		C = 0x2E,
		V = 0x2F,
		B = 0x30,
		N = 0x31,
		M = 0x32,
		Comma = 0x33,
		Period = 0x34,
		Slash = 0x35,
		RShift = 0x36,
		NumMultiply = 0x37,
		LMenu = 0x38, LAlt = LMenu, Alt = LAlt,
		Space = 0x39,
		Capital = 0x3A, CapsLock = Capital,
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
		Numlock = 0x45,
		ScrollLock = 0x46,
		Num7 = 0x47,
		Num8 = 0x48,
		Num9 = 0x49,
		NumSubtract = 0x4A,
		Num4 = 0x4B,
		Num5 = 0x4C,
		Num6 = 0x4D,
		NumAdd = 0x4E,
		Num1 = 0x4F,
		Num2 = 0x50,
		Num3 = 0x51,
		Num0 = 0x52,
		NumDecimal = 0x53,
		F11 = 0x57,
		F12 = 0x58,

		NumEnter = 0x9C,
		RControl = 0x9D,
		NumDivide = 0xB5,
		RMenu = 0xB8, RAlt = RMenu,

		Up = 0xC8,
		Left = 0xCB,
		Right = 0xCD,
		Down = 0xD0,

		Home = 0xC7,
		End = 0xCF,
		Next = 0xD1, PageDown = Next,
		Prior = 0xC9, PageUp = Prior,
		Insert = 0xD2,
		Delete = 0xD3,

		LWin = 0xDB,
		RWin = 0xDC,
		AppMenu = 0xDD
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
