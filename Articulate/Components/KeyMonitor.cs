using SierraLib.GlobalHooks;
using SierraLib.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Articulate
{
	public class KeyMonitor : IDisposable
	{
		public KeyMonitor(Settings settings)
		{
			Configuration = settings;

			HookManager.KeyDown += OnKeyDown;
			HookManager.KeyUp += OnKeyUp;
			HookManager.MouseDown += OnMouseDown;
			HookManager.MouseUp += OnMouseUp;
		}
				
		#region Public Properties

		public Settings Configuration
		{ get; private set; }

		/// <summary>
		/// Gets the <see cref="CompoundKeyBind"/> that is currently active
		/// or <c>null</c> if no keybind is pressed.
		/// </summary>
		public CompoundKeyBind ActiveKeyBind
		{ get; private set; }

		#endregion

		#region Public Methods

		public void BeginMapping()
		{
			IsMapping = true;
		}

		public void ClearMapping()
		{
			Configuration.KeyBinds.Clear();
			TestKeys();
		}

		#endregion

		#region Events

		public event EventHandler<CompoundKeyBind> KeysPressed = null;

		public event EventHandler<CompoundKeyBind> KeysReleased = null;

		public event EventHandler<IEnumerable<CompoundKeyBind>> MappingCompleted = null;

		#endregion

		#region Local State

		bool IsMapping = false;

		List<System.Windows.Forms.Keys> ActiveKeyboardKeys = new List<System.Windows.Forms.Keys>();
		List<System.Windows.Forms.MouseButtons> ActiveMouseButtons = new List<System.Windows.Forms.MouseButtons>();

		#endregion

		#region Global Event Handlers

		void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (IsMapping) { EndMapping(); return; }

			ActiveMouseButtons.Remove(e.Button);
			TestKeys();
		}

		void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (!ActiveMouseButtons.Any(x => x == e.Button))
				ActiveMouseButtons.Add(e.Button);
			if(!IsMapping) TestKeys();
		}

		void OnKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (IsMapping) { EndMapping(); return; }
				
			ActiveKeyboardKeys.Remove(e.KeyCode);
			TestKeys();
		}

		void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if(!ActiveKeyboardKeys.Any(x => x == e.KeyCode))
				ActiveKeyboardKeys.Add(e.KeyCode);			
			if (!IsMapping) TestKeys();
		}

		#endregion

		#region Local Logic

		void TestKeys()
		{
			var newActiveKeyBind = Configuration.KeyBinds.FirstOrDefault(x => x.IsActive(ActiveKeyboardKeys, ActiveMouseButtons));

			if (newActiveKeyBind != ActiveKeyBind)
			{
				if (newActiveKeyBind != null && KeysPressed != null)
					KeysPressed(this, newActiveKeyBind);
				else if (newActiveKeyBind == null && KeysReleased != null)
					KeysReleased(this, ActiveKeyBind);

				ActiveKeyBind = newActiveKeyBind;
			}
		}
		
		void EndMapping()
		{
			// Create the keybind for the currently pressed keys

			var binding = new CompoundKeyBind(ActiveKeyboardKeys, ActiveMouseButtons);

			Configuration.KeyBinds.Add(binding);

			if (MappingCompleted != null)
				MappingCompleted(this, Configuration.KeyBinds);

			ActiveMouseButtons.Clear();
			ActiveKeyboardKeys.Clear();
			IsMapping = false;
		}

		#endregion

		public void Dispose()
		{
			HookManager.KeyDown -= OnKeyDown;
			HookManager.KeyUp -= OnKeyUp;
			HookManager.MouseDown -= OnMouseDown;
			HookManager.MouseUp -= OnMouseUp;
		}
	}

	public enum KeyType
	{
		Mouse = 1,
		Keyboard = 2
	}

	[Serializable]
	public class CompoundKeyBind : ISerializable
	{
		public CompoundKeyBind(IEnumerable<System.Windows.Forms.Keys> keys, IEnumerable<System.Windows.Forms.MouseButtons> buttons)
		{
			var binds = new List<KeyBind>();
			if(keys != null && keys.Any())
				binds.AddRange(keys.Select(x => new KeyBind(x)));
			if(buttons != null && buttons.Any())
				binds.AddRange(buttons.Select(x => new KeyBind(x)));

			if (!binds.Any()) throw new InvalidOperationException("Cannot create a compound bind with no key or button presses in it");

			Keys = binds.ToArray();
		}

		public KeyBind[] Keys
		{ get; private set; }

		public bool IsActive(IEnumerable<System.Windows.Forms.Keys> keys, IEnumerable<System.Windows.Forms.MouseButtons> buttons)
		{
			int matched = 0;
			foreach (var k in Keys)			
				if (keys.Any(x => k.IsActive(x)) || buttons.Any(x => k.IsActive(x))) matched++;

			return matched == Keys.Length;
		}
		
		public override string ToString()
		{
			return Keys.Select(x => x.ToString()).Aggregate((x, y) => x + " + " + y);
		}

		#region ISerializable

		protected CompoundKeyBind(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new System.ArgumentNullException("info");

			Keys = (KeyBind[])info.GetValue("keys", typeof(KeyBind[]));
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new System.ArgumentNullException("info");

			info.AddValue("keys", Keys, typeof(KeyBind[]));
		}
		
		#endregion
	}

	[Serializable]
	public class KeyBind : ISerializable
	{
		public KeyBind(System.Windows.Forms.Keys key)
		{
			Type = KeyType.Keyboard;
			Code = (uint)key;
		}

		public KeyBind(System.Windows.Forms.MouseButtons button)
		{
			Type = KeyType.Mouse;
			Code = (uint)button;
		}

		public KeyType Type
		{ get; private set; }

		public uint Code
		{ get; private set; }

		public bool IsActive(System.Windows.Forms.Keys key)
		{
			return Type == KeyType.Keyboard && Code == (uint)key;
		}

		public bool IsActive(System.Windows.Forms.MouseButtons button)
		{
			return Type == KeyType.Mouse && Code == (uint)button;
		}

		public override string ToString()
		{
			switch (Type)
			{
				case KeyType.Keyboard:
					return TranslationManager.Instance["keyboard_" + Code, ((System.Windows.Forms.Keys)Code).ToString()];
				case KeyType.Mouse:
					return TranslationManager.Instance["mouse_" + Code, ((System.Windows.Forms.MouseButtons)Code).ToString()];
				default: return "";
			}
		}

		#region ISerializable

		protected KeyBind(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new System.ArgumentNullException("info");
			Type = (KeyType)info.GetByte("type");
			Code = info.GetUInt32("code");
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
            if (info == null)
                throw new System.ArgumentNullException("info");
			info.AddValue("type", (byte)Type);
			info.AddValue("code", Code);
		}

		#endregion
	}
}
