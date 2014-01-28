using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace SierraLib.Translation
{
	/// <summary>
	/// Base class from which all translation implementations derive.
	/// </summary>
	/// <remarks>
	/// This design allows translations to be sourced from compiled classes,
	/// string tables, embedded and external files, as well as supporting a number
	/// of file types and structures, or even custom logic.
	/// </remarks>
    public abstract class TranslationBase
    {
		/// <summary>
		/// The <see cref="CultureInfo"/> to which this translation applies
		/// </summary>
		public virtual CultureInfo Culture { get; protected set; }

		/// <summary>
		/// The name of the author of this translation
		/// </summary>
		public virtual string Author { get; protected set; }

		/// <summary>
		/// Method which retrieves the translation for the requested <paramref name="key"/> if one is available.
		/// </summary>
		/// <param name="key">The unique key used to identify the requested translated text</param>
		/// <returns>Returns the translated text if it is available, or a predetermined default</returns>
		public abstract string this[string key] { get; }

		/// <summary>
		/// Method which retrieves the translation for the requested <paramref name="key"/> if one is available, 
		/// using the provided <paramref name="defaultValue"/> if not.
		/// </summary>
		/// <param name="key">The unique key used to identify the requested translated text</param>
		/// <param name="defaultValue">The text to return if a translated default couldn't be found</param>
		/// <returns>Returns the translated text if it is available, or the specified <paramref name="defaultValue"/></returns>
		public abstract string this[string key, string defaultValue] { get; }

		/// <summary>
		/// Determines whether the specified object is the same as the current <see cref="TranslationBase"/>
		/// </summary>
		/// <param name="obj">The object to compare to the current <see cref="TranslationBase"/></param>
		/// <returns>Returns <c>true</c> if the objects match</returns>
		public override bool Equals(object obj)
		{
			if (obj == null) return false;

			var other = obj as TranslationBase;

			if (other == null) return false;

			return String.Equals(Author, other.Author) && Culture.Equals(other.Culture);
		}

		/// <summary>
		/// Serves as a hash function for the current <see cref="TranslationBase"/>,
		/// suitable for hashing algorithms and data structures, such as a hash table.
		/// </summary>
		/// <returns>A hash code for the current <see cref="TranslationBase"/></returns>
		public override int GetHashCode()
		{
			return Author.GetHashCode() ^ Culture.GetHashCode();
		}
    }
}
