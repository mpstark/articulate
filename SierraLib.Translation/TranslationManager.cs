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

    public class TranslationManager
    {
        public TranslationManager()
        {
            Translations = new TranslationList();
        }

        private static TranslationManager _translationManager;

        public event EventHandler LanguageChanged;

        public CultureInfo CurrentLanguage
        {
            get { return Thread.CurrentThread.CurrentUICulture; }
            set
            {
                if (value != Thread.CurrentThread.CurrentUICulture)
                {
                    Thread.CurrentThread.CurrentUICulture = value;
                    RefreshCurrentTranslation();
                    OnLanguageChanged();
                }
            }
        }

		private CultureInfo _DefaultLanguage = new CultureInfo("en");
		public CultureInfo DefaultLanguage
		{
			get { return _DefaultLanguage; }
			set 
			{
				_DefaultLanguage = value;
				RefreshCurrentTranslation();
				OnLanguageChanged();
			}
		}

        public IEnumerable<CultureInfo> Languages
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<CultureInfo>>() != null);

                if (Translations != null)
                    foreach (ITranslation translation in Translations)
                        yield return translation.Culture;
            }
        }

        public static TranslationManager Instance
        {
            get
            {
                if (_translationManager == null)
                    _translationManager = new TranslationManager();
                return _translationManager;
            }
        }

        public TranslationList Translations { get; private set; }

        public ITranslation CurrentTranslation
        {
            get;
            private set;
        }
        
        private void OnLanguageChanged()
        {
            if (LanguageChanged != null)
            {
                LanguageChanged(this, EventArgs.Empty);
            }
        }
        
        public string this[string key]
        {
            get
            {
                return this[key, "!!!" + key];
            }
        }

        public string this[string key, string defaultValue]
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
				RefreshCurrentTranslation();
				OnLanguageChanged();
			}
			else
				CurrentLanguage = targetCulture;
		}

        void RefreshCurrentTranslation()
        {
            string current = Thread.CurrentThread.CurrentUICulture.Name;

            //First, we attempt to find a translator for this exact dialect
            while (true)
            {
                foreach (ITranslation translation in Translations)
                    if (translation.Culture.Name == current)
                    {
                        CurrentTranslation = translation;
                        return;
                    }

                //If we get here, it means that we couldn't find anything, so
                //we will just resort to using the defaults specified, or showing keys
                if (current.Length == 0)
                {
					CurrentTranslation = Translations.FirstOrDefault(x => x.Culture == DefaultLanguage) ?? new DesignerTranslation();
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
                        if (typeof(ITranslation).IsAssignableFrom(t))
                        {
                            if (!t.IsAbstract && t.IsPublic)
                            {
                                //Load this plugin
                                try
                                {
                                    ITranslation newTranslation = (ITranslation)assembly.CreateInstance(t.FullName);
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
}

