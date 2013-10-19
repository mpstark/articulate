using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SierraLib.Translation
{
    public static class LanguageExtension
    {
        /// <summary>
        /// Translates a given string by interpreting it as a key
        /// for the currently loaded language
        /// </summary>
        /// <param name="key">The key used for the current translation</param>
        /// <param name="defaultValue">A default value for the translation if the key is not found</param>
        /// <returns>The translated string matching the <paramref name="key"/> or the <paramref name="defaultValue"/></returns>
        public static string Translate(this string key, string defaultValue = null)
        {
            return TranslationManager.Instance[key, defaultValue];
        }

    }
}
