using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Articulate
{

	#region Native Structures

	[StructLayout(LayoutKind.Sequential)]
	public struct MOUSEINPUT
	{
		public int DX;
		public int DY;
		public uint Data;
		public uint Flags;
		public uint Time;
		public UIntPtr ExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct KEYBOARDINPUT
	{
		public const int ExtendedKey = 0x1;
		public const int KeyUp = 0x2;
		public const int Unicode = 0x4;
		public const int KeyboardScanCode = 0x8;

		public ushort VirtualKey;
		public ushort ScanCode;
		public uint Flags;
		public uint Time;
		public UIntPtr ExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct HARDWAREINPUT
	{
		public uint Message;
		public ushort ParamL;
		public ushort ParamH;
	}

	[StructLayout(LayoutKind.Explicit, Size = 28)]
	public struct INPUT
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

	#endregion

	#region Native Enumerations

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

	#endregion

	public abstract class DirectInputBase : OutputBase
	{
		protected DirectInputBase(string displayName, string serializationName)
			: base(displayName, serializationName)
		{

		}

		protected DirectInputBase(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}

		[DllImport("user32.dll")]
		protected static extern UInt32 SendInput(UInt32 nInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] pInputs, Int32 cbSize);

		protected static readonly int INPUTSize = Marshal.SizeOf(typeof(INPUT));

		protected abstract INPUT[] ToDirectInput();
		
		public override void Execute()
		{
			var inputs = ToDirectInput();
			var result = SendInput((uint)inputs.Length, inputs, INPUTSize);

			Trace.WriteLine("SendInput result: " + result);
		}

		public override async Task ExecuteAsync()
		{
			var inputs = ToDirectInput();
			var result = await Task.Factory.StartNew(() => SendInput((uint)inputs.Length, inputs, INPUTSize));

			Trace.WriteLine("SendInput result: " + result);
		}
	}

	#region Keyboard Outputs

	public abstract class KeyboardOutputBase : DirectInputBase
	{		
		protected KeyboardOutputBase(string displayName, string serializationName)
			: base(displayName, serializationName)
		{

		}

		protected KeyboardOutputBase(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			Key = (DirectInputKeys)info.GetInt16("Key");
		}

		public virtual DirectInputKeys Key { get; protected set; }
		
		protected override INPUT[] ToDirectInput()
		{
			return new[] {
				new INPUT() { 
					Type = 1, //KEYBOARD_INPUT
					Keyboard = new KEYBOARDINPUT() {
						Time = 0,
						Flags = KEYBOARDINPUT.KeyboardScanCode,
						ExtraInfo = UIntPtr.Zero,
						VirtualKey = 0,
						ScanCode = (ushort)Key
					}
				}
			};
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("Key", (Int16)Key);
		}
	}

	public sealed class KeyDown : KeyboardOutputBase
	{
		public KeyDown(DirectInputKeys key)
			: base("output_keydown", "KeyDown")
		{
			Key = key;
		}

		private KeyDown(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}
	}

	public sealed class KeyUp : KeyboardOutputBase
	{
		public KeyUp(DirectInputKeys key)
			: base("output_keyup", "KeyUp")
		{
			Key = key;
		}

		private KeyUp(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}

		protected override INPUT[] ToDirectInput()
		{
			var input = base.ToDirectInput();
			input[0].Keyboard.Flags |= KEYBOARDINPUT.KeyUp;
			return input;
		}
	}

	public sealed class KeyPress : OutputGroup
	{
		public KeyPress(DirectInputKeys key)
			: base("output_keypress", "KeyPress")
		{
			Operations = new OutputBase[] {
				new KeyDown(key),
				new Sleep(Core.Instance.Configuration.KeyReleaseDelay),
				new KeyUp(key)
			};
		}

		private KeyPress(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}
	}
	
	#endregion

	#region Mouse Outputs

	public abstract class MouseOutputBase : DirectInputBase
	{
		protected MouseOutputBase(string displayName, string serializationName)
			: base(displayName, serializationName)
		{

		}

		protected MouseOutputBase(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			Button = (System.Windows.Forms.MouseButtons)info.GetInt32("Button");
		}

		public virtual System.Windows.Forms.MouseButtons Button
		{ get; set; }

		protected override INPUT[] ToDirectInput()
		{
			var input = new INPUT()
			{
				Type = 0,
				Mouse = new MOUSEINPUT()
				{
					Time = 0,
					Data = 0,
					DX = 0,
					DY = 0,
					ExtraInfo = UIntPtr.Zero,
					Flags = 0
				}
			};

			switch (Button)
			{
				case System.Windows.Forms.MouseButtons.Left:
					input.Mouse.Flags = 0x2;
					break;
				case System.Windows.Forms.MouseButtons.Right:
					input.Mouse.Flags = 0x8;
					break;
				case System.Windows.Forms.MouseButtons.Middle:
					input.Mouse.Flags = 0x20;
					break;
				case System.Windows.Forms.MouseButtons.XButton1:
					input.Mouse.Flags = 0x80;
					input.Mouse.Data = 0x1;
					break;

				case System.Windows.Forms.MouseButtons.XButton2:
					input.Mouse.Flags = 0x80;
					input.Mouse.Data = 0x2;
					break;
			}

			return new[] { input };
		}
		
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("Button", (Int32)Button);
		}
	}


	public sealed class MouseDown : MouseOutputBase
	{
		public MouseDown(System.Windows.Forms.MouseButtons button)
			: base("output_mousedown", "MouseDown")
		{
			Button = button;
		}

		private MouseDown(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}
	}

	public sealed class MouseUp : MouseOutputBase
	{
		public MouseUp(System.Windows.Forms.MouseButtons button)
			: base("output_mouseup", "MouseUp")
		{
			Button = button;
		}

		private MouseUp(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}

		protected override INPUT[] ToDirectInput()
		{
			var input = base.ToDirectInput();
			input[0].Mouse.Flags <<= 1;
			return input;
		}
	}

	public sealed class MouseClick : OutputGroup
	{
		public MouseClick(System.Windows.Forms.MouseButtons button)
			: base("output_mouseclick", "MouseClick")
		{
			Operations = new OutputBase[] { 
				new MouseDown(button),
				new Sleep(Core.Instance.Configuration.MouseReleaseDelay),
				new MouseUp(button)
			};
		}

		private MouseClick(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}
	}
	

	#endregion

	#region Utility "Outputs"

	/// <summary>
	/// Allows outputs to be grouped while still behaving like an <see cref="OutputBase"/>.
	/// </summary>
	public class OutputGroup : OutputBase
	{
		protected OutputGroup(string displayName, string serializationName)
			: base(displayName, serializationName)
		{

		}

		public OutputGroup(IEnumerable<OutputBase> operations) 
			: base("output_group", "Group")
		{
			Operations = operations;
		}

		protected OutputGroup(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info == null) throw new ArgumentNullException("info");

			Operations = (IEnumerable<OutputBase>)info.GetValue("Operations", typeof(IEnumerable<OutputBase>));
		}

		public IEnumerable<OutputBase> Operations
		{ get; protected set; }

		public override void Execute()
		{
			foreach (var op in Operations)
				op.Execute();
		}

		public override async Task ExecuteAsync()
		{
			foreach (var op in Operations)
				await op.ExecuteAsync();
		}

		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Operations", Operations, typeof(IEnumerable<OutputBase>));
		}
	}

	/// <summary>
	/// Causes the program to delay the next <see cref="OutputBase"/> by the specified number of milliseconds
	/// </summary>
	public sealed class Sleep : OutputBase
	{
		public Sleep(int duration)
			: base("output_sleep", "Sleep")
		{
			Duration = duration;
		}

		private Sleep(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info == null) throw new ArgumentNullException("info");

			Duration = info.GetInt32("Duration");
		}

		public int Duration
		{ get; set; }

		public override void Execute()
		{
			if(Duration > 0)
				Thread.Sleep(Duration);
		}

		public override async Task ExecuteAsync()
		{
			if(Duration > 0)
				await Task.Delay(Duration);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("Duration", Duration);
		}
	}

	#endregion

}
