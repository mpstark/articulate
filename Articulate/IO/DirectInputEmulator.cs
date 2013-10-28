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
    /// Exposes the SendInput method from the User32/Win32 library. Using this instead of other options because
    /// this allows input into DirectInput applications (i.e. Arma 3 and other applications that use DirectX).
    /// </summary>
    static class DirectInputEmulator
    {		
		/// <summary>
		/// Sends a series of input operations using Direct Input
		/// </summary>
		/// <param name="inputs">The <see cref="INPUT"/> arrays representing operations to perform</param>
		/// <param name="delay">The delay to wait between each keypress.</param>
		public static void SendInput(params OutputBase[] outputOperations)
		{
			foreach (var op in outputOperations)
			{
				op.Execute();
			}
		}
		public static void SendInput(IEnumerable<OutputBase> outputOperations)
		{
			foreach (var op in outputOperations)
			{
				op.Execute();
			}
		}
		public static async Task SendInputAsync(params OutputBase[] outputOperations)
		{
			foreach (var op in outputOperations)			
				await op.ExecuteAsync();			
		}
		public static async Task SendInputAsync(IEnumerable<OutputBase> outputOperations)
		{
			foreach (var op in outputOperations)			
				await op.ExecuteAsync();			
		}
				
		/// <summary>
		/// Creates a set of input operations to emulate pressing the given keys
		/// </summary>
		/// <param name="keys">The <see cref="DirectInputKeys"/> to emulate pressing</param>
		/// <returns>Returns an array of <see cref="INPUT"/> objects for use by <see cref="SendInput"/></returns>
		public static OutputBase KeyDown(params DirectInputKeys[] keys)
		{
			if(keys.Length > 0)
				return new OutputGroup(keys.Select(x => new KeyDown(x)));

			return new KeyDown(keys[0]);
		}

		/// <summary>
		/// Creates a set of input operations to emulate releasing the given keys
		/// </summary>
		/// <param name="keys">The <see cref="DirectInputKeys"/> to emulate releasing</param>
		/// <returns>Returns an array of <see cref="INPUT"/> objects for use by <see cref="SendInput"/></returns>
		public static OutputBase KeyUp(params DirectInputKeys[] keys)
		{
			if (keys.Length > 0)
				return new OutputGroup(keys.Select(x => new KeyUp(x)));

			return new KeyUp(keys[0]);
		}

		/// <summary>
		/// Creates a set of input operations to emulate pressing and releasing the given keys
		/// </summary>
		/// <param name="keys">The <see cref="DirectInputKeys"/> to emulate pressing and releasing</param>
		/// <returns>Returns an array of <see cref="INPUT"/> objects for use by <see cref="SendInput"/></returns>
		public static OutputBase KeyPress(params DirectInputKeys[] keys)
		{
			if (keys.Length > 0)
				return new OutputGroup(keys.Select(x => new KeyPress(x)));

			return new KeyPress(keys[0]);
		}

		/// <summary>
		/// Creates a set of input operations to emulate pressing the given mouse buttons
		/// </summary>
		/// <param name="buttons">The <see cref="System.Windows.Forms.MouseButtons"/> to emulate pressing</param>
		/// <returns>Returns an array of <see cref="INPUT"/> objects for use by <see cref="SendInput"/></returns>
		public static OutputBase MouseDown(params System.Windows.Forms.MouseButtons[] buttons)
		{
			if (buttons.Length > 0)
				return new OutputGroup(buttons.Select(x => new MouseDown(x)));

			return new MouseDown(buttons[0]);
		}

		/// <summary>
		/// Creates a set of input operations to emulate releasing the given mouse buttons
		/// </summary>
		/// <param name="buttons">The <see cref="System.Windows.Forms.MouseButtons"/> to emulate releasing</param>
		/// <returns>Returns an array of <see cref="INPUT"/> objects for use by <see cref="SendInput"/></returns>
		public static OutputBase MouseUp(params System.Windows.Forms.MouseButtons[] buttons)
		{
			if (buttons.Length > 0)
				return new OutputGroup(buttons.Select(x => new MouseUp(x)));

			return new MouseUp(buttons[0]);
		}

		/// <summary>
		/// Creates a set of input operations to emulate pressing and releasing the given mouse buttons
		/// </summary>
		/// <param name="buttons">The <see cref="System.Windows.Forms.MouseButtons"/> to emulate pressing and releasing</param>
		/// <returns>Returns an array of <see cref="INPUT"/> objects for use by <see cref="SendInput"/></returns>
		public static OutputBase MouseClick(params System.Windows.Forms.MouseButtons[] buttons)
		{
			if (buttons.Length > 0)
				return new OutputGroup(buttons.Select(x => new MouseClick(x)));

			return new MouseClick(buttons[0]);
		}

    }
}
