using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace SierraLib.Translation
{
    public interface ITranslation
    {
        CultureInfo Culture { get; }

        string Author { get; }

        string this[string key] { get; }

        string this[string key, string defaultValue] { get; }
    }
}
