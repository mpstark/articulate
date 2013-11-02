using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SierraLib.Translation
{
    public class DesignerTranslation : TranslationBase
    {
		public override System.Globalization.CultureInfo Culture
        {
            get { return new System.Globalization.CultureInfo(""); }
        }

        public override string this[string key]
        {
            get { return this[key, "!!!" + key]; }
        }

		public override string this[string key, string defaultValue]
        {
            get { return defaultValue; }
        }


		public override string Author
        {
            get { return "Sierra Softworks"; }
        }
    }
}
