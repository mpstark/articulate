using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.Windows.Data;

namespace SierraLib.Translation
{
    public class TranslateExtension : System.Windows.Markup.MarkupExtension
    {
        private string _key;
        private string _default;

        public TranslateExtension()
            : this("unknown")
        { }

        public TranslateExtension(string key)
            : this(key, null)
        {   }

        public TranslateExtension(string key, string defaultValue)
        {
            _key = key;
            _default = defaultValue;
        }

        [ConstructorArgument("key")]
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        [ConstructorArgument("defaultValue")]
        public string Default
        {
            get { return _default; }
            set { _default = value; }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding("Value")
                  {
                      Source = new TranslationBinding(_key, _default)
                  };
            return binding.ProvideValue(serviceProvider);
        }
    }
}
