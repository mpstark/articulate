using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics.Contracts;

namespace SierraLib.Translation
{
    public class TranslationList : List<TranslationBase>
    {
        public TranslationList()
        {
            Contract.Ensures(Count == 0);
        }

        public TranslationBase this[CultureInfo culture]
        {
            get
            {
                foreach (TranslationBase translation in this)
                    if (translation.Culture == culture)
                        return translation;
                return null;
            }
        }
    }
}
