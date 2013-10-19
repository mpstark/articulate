using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics.Contracts;

namespace SierraLib.Translation
{
    public class TranslationList : List<ITranslation>
    {
        public TranslationList()
        {
            Contract.Ensures(Count == 0);
        }

        public ITranslation this[CultureInfo culture]
        {
            get
            {
                foreach (ITranslation translation in this)
                    if (translation.Culture == culture)
                        return translation;
                return null;
            }
        }
    }
}
