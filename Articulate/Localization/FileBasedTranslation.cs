using SierraLib.Translation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Articulate
{
	public sealed class FileBasedTranslation : SierraLib.Translation.FileBasedTranslation
	{
		public FileBasedTranslation(CultureInfo culture, Stream file)
			: base(culture, file)
		{

		}
		
		public override string this[string key]
		{
			get 
			{
				return this[key, "!!!" + key];
			}
		}

		public override string this[string key, string defaultValue]
		{
			get 
			{
				if (FileData.ContainsKey(key)) return FileData[key];
				else if (key.StartsWith("keyboard_"))
				{
					var keyCode = Convert.ToInt32(key.Substring("keyboard_".Length));
					return ((System.Windows.Forms.Keys)keyCode).ToString();
				}
				else if (key.StartsWith("mouse_"))
				{
					var keyCode = Convert.ToInt32(key.Substring("mouse_".Length));
					return ((System.Windows.Forms.MouseButtons)keyCode).ToString();
				}
				else return defaultValue;
			}
		}

	}
}
