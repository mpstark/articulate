using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace SierraLib.Translation
{
	/// <summary>
	/// Provides translation support for applications which make use of WPF user interfaces or
	/// require the ability to load translations from different sources.
	/// </summary>
    public class TranslationManager
    {
        protected TranslationManager()
        {
            Translations = new TranslationList();
        }

		private static TranslationManager _translationManager;

		#region Singleton

		/// <summary>
		/// Gets the active singleton instance of this <see cref="TranslationManager"/> for use throughout
		/// the application.
		/// </summary>
		public static TranslationManager Instance
		{
			get
			{
				if (_translationManager == null)
					_translationManager = new TranslationManager();
				return _translationManager;
			}
		}

		#endregion

		#region Public Events

		/// <summary>
		/// Triggered whenever the <see cref="CurrentTranslation"/> changes, allowing data
		/// that is not bound to be updated.
		/// </summary>
		public event EventHandler<TranslationChangedEventArgs> LanguageChanged;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the <see cref="CultureInfo"/> describing the currently selected
		/// language for translation.
		/// </summary>
		public CultureInfo CurrentLanguage
		{
			get { return Thread.CurrentThread.CurrentUICulture; }
			set
			{
				if (value != Thread.CurrentThread.CurrentUICulture)
				{
					var currentTranslation = CurrentTranslation;

					Thread.CurrentThread.CurrentUICulture = value;
					RefreshCurrentTranslation();

					if (CurrentTranslation != currentTranslation)
						OnLanguageChanged(currentTranslation);
				}
			}
		}

		private CultureInfo _DefaultLanguage = new CultureInfo("en");
		/// <summary>
		/// Gets or sets the default <see cref="CultureInfo"/> to use if no translation for the
		/// <see cref="CurrentLanguage"/> could be found.
		/// </summary>
		public CultureInfo DefaultLanguage
		{
			get { return _DefaultLanguage; }
			set
			{
				var currentTranslation = CurrentTranslation;

				_DefaultLanguage = value;
				RefreshCurrentTranslation();

				if (CurrentTranslation != currentTranslation)
					OnLanguageChanged(currentTranslation);
			}
		}

		/// <summary>
		/// Gets the currently active <see cref="TranslationBase"/> used to provide translations.
		/// </summary>
		public TranslationBase CurrentTranslation
		{
			get;
			private set;
		}

		#endregion

		#region Public Collections

		/// <summary>
		/// Gets a collection of <see cref="CultureInfo"/> objects representing the languages
		/// available for translation.
		/// </summary>
        public IEnumerable<CultureInfo> Languages
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<CultureInfo>>() != null);

                if (Translations != null)
                    foreach (TranslationBase translation in Translations)
                        yield return translation.Culture;
            }
        }
		
		/// <summary>
		/// Gets the list of <see cref="TranslationBase"/> instances providing translations
		/// for the current application.
		/// </summary>
        public TranslationList Translations { get; private set; }
		
		#endregion

		#region Translation Accessors

		/// <summary>
		/// Method which retrieves the translation for the requested <paramref name="key"/> if one is available.
		/// </summary>
		/// <param name="key">The unique key used to identify the requested translated text</param>
		/// <returns>Returns the translated text if it is available, or a predetermined default</returns>
        public virtual string this[string key]
        {
            get
            {
                return this[key, "!!!" + key];
            }
        }

		/// <summary>
		/// Method which retrieves the translation for the requested <paramref name="key"/> if one is available, 
		/// using the provided <paramref name="defaultValue"/> if not.
		/// </summary>
		/// <param name="key">The unique key used to identify the requested translated text</param>
		/// <param name="defaultValue">The text to return if a translated default couldn't be found</param>
		/// <returns>Returns the translated text if it is available, or the specified <paramref name="defaultValue"/></returns>
		public virtual string this[string key, string defaultValue]
        {
            get
            {
                if (CurrentTranslation == null)
                    RefreshCurrentTranslation();

                if (CurrentTranslation != null)
                {
                    string translatedValue = CurrentTranslation[key, defaultValue];
                    if (translatedValue != null)
                        return translatedValue;
                }
                return defaultValue;
            }
        }
		
		#endregion

		#region Public Methods

		/// <summary>
		/// Loads the best available translation for the current thread's UI culture
		/// </summary>
		public void LoadBest()
		{
			LoadBest(Thread.CurrentThread.CurrentUICulture);
		}
		
		/// <summary>
		/// Loads the best available translation for the specified culture
		/// </summary>
		/// <param name="targetCulture">The <see cref="CultureInfo"/> describing the ideal translation to use</param>
		public void LoadBest(CultureInfo targetCulture)
		{
			if (targetCulture == CurrentLanguage)
			{
				var currentTranslation = CurrentTranslation;

				RefreshCurrentTranslation();

				if(currentTranslation != CurrentTranslation)
					OnLanguageChanged(currentTranslation);
			}
			else
				CurrentLanguage = targetCulture;
		}

		#endregion

		private void OnLanguageChanged(TranslationBase oldTranslation)
		{
			if (LanguageChanged != null)
			{
				LanguageChanged(this, new TranslationChangedEventArgs(oldTranslation, CurrentTranslation));
			}
		}
		

        void RefreshCurrentTranslation()
        {
            string current = Thread.CurrentThread.CurrentUICulture.Name;
			
            //First, we attempt to find a translator for this exact dialect
            while (true)
            {
                foreach (TranslationBase translation in Translations)
                    if (translation.Culture.Name == current)
                    {
                        CurrentTranslation = translation;
                        return;
                    }

                //If we get here, it means that we couldn't find anything, so
                //we will just resort to using the defaults specified, or showing keys
                if (current.Length == 0)
                {
					CurrentTranslation = Translations.FirstOrDefault(x => x.Culture.Equals(DefaultLanguage)) ?? new DesignerTranslation();
					return;
                }

                //If we get here it means we didn't find one, so lets try
                //broadening the search
                int index = current.IndexOf("-");
                if (index < 0)
                    current = "";
                else
                    current = current.Remove(index);
            }

        }

        public bool LoadTranslations(string sourceDirectory)
        {
            //Load each of the translation's DLL files
            if (!Directory.Exists(sourceDirectory))
                return false;

            Translations.Clear();

            foreach (FileInfo file in new DirectoryInfo(sourceDirectory).GetFiles("*.dll"))
            {
                Assembly assembly = Assembly.LoadFile(file.FullName);

                if (assembly != null)
                {
                    Type[] pluginTypes = assembly.GetTypes();

                    foreach (Type t in pluginTypes)
                    {
                        if (typeof(TranslationBase).IsAssignableFrom(t))
                        {
                            if (!t.IsAbstract && t.IsPublic)
                            {
                                //Load this plugin
                                try
                                {
                                    TranslationBase newTranslation = (TranslationBase)assembly.CreateInstance(t.FullName);
                                    Translations.Add(newTranslation);
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
            }

            RefreshCurrentTranslation();

            return true;
        }
    }
	
	/// <summary>
	/// Describes the changes to the active translation
	/// </summary>
	public sealed class TranslationChangedEventArgs : EventArgs
	{
		internal TranslationChangedEventArgs(TranslationBase oldTranslation, TranslationBase newTranslation)
		{
			Previous = oldTranslation;
			Current = newTranslation;
		}

		/// <summary>
		/// The previously active <see cref="TranslationBase"/> instance
		/// </summary>
		public TranslationBase Previous
		{ get; private set; }

		/// <summary>
		/// The currently active <see cref="TranslationBase"/> instance providing translations
		/// </summary>
		public TranslationBase Current
		{ get; private set; }
	}
}

