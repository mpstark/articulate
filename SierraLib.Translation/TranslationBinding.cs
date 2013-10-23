using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace SierraLib.Translation
{
    public class TranslationBinding : IWeakEventListener,
                  INotifyPropertyChanged, IDisposable
    {
        private string _key;
        private string _default = null;

        public TranslationBinding(string key, string defaultvalue)
        {
            _key = key;
            _default = defaultvalue;
            LanguageChangedEventManager.AddListener(
                      TranslationManager.Instance, this);
        }

        ~TranslationBinding()
        {
            Dispose(false);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                LanguageChangedEventManager.RemoveListener(
                          TranslationManager.Instance, this);
            }
        }


        public object Value
        {
            get
            {
                return TranslationManager.Instance[_key, _default ?? "!!!" + _key];
            }
        }

        public bool ReceiveWeakEvent(Type managerType,
                                object sender, EventArgs e)
        {
            if (managerType == typeof(LanguageChangedEventManager))
            {
                OnLanguageChanged(sender, e);
                return true;
            }
            return false;
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Value"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
