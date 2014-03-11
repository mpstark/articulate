using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAXLib;

namespace Articulate
{
    abstract class AbstractSymbol
    {
        [YAXAttributeFor("..")]
        [YAXSerializeAs("name")]
        public string Name { get; set; }
    }

    class Symbol : AbstractSymbol
    {
        [YAXAttributeFor("..")]
        [YAXSerializeAs("value")]
        public string Value { get; set; }

        [YAXCollection(YAXCollectionSerializationTypes.Serially, SeparateBy = "; ")]
        public List<string> Pronounceables { get; set; }
    }

    class SymbolGroup : AbstractSymbol
    {
        public List<AbstractSymbol> Members { get; set; }

        [YAXCollection(YAXCollectionSerializationTypes.Serially, SeparateBy = "; ")]
        public List<string> Prefix { get; set; }

        [YAXAttributeFor("Prefix")]
        [YAXSerializeAs("required")]
        public bool PrefixRequired { get; set; }

        [YAXCollection(YAXCollectionSerializationTypes.Serially, SeparateBy = "; ")]
        public List<string> Postfix { get; set; }

        [YAXAttributeFor("Postfix")]
        [YAXSerializeAs("required")]
        public bool PostfixRequired { get; set; }

        [YAXAttributeFor("..")]
        [YAXSerializeAs("repeat")]
        public int Repeat { get; set; }
    }
}
