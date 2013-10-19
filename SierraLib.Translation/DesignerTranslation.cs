using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SierraLib.Translation
{
    public class DesignerTranslation : ITranslation
    {
        public System.Globalization.CultureInfo Culture
        {
            get { return new System.Globalization.CultureInfo(""); }
        }

        public string this[string key]
        {
            get { return this[key, "!!!" + key]; }
        }

        public string this[string key, string defaultValue]
        {
            get { return defaultValue; }
        }


        public string Author
        {
            get { return "Sierra Softworks"; }
        }
    }
}
