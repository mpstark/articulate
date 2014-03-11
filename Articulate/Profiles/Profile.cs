using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAXLib;

namespace Articulate
{
    class Profile
    {
        [YAXAttributeFor("..")]
        [YAXSerializeAs("name")]
        public string Name { get; set; }

        [YAXAttributeFor("..")]
        [YAXSerializeAs("language")]
        public string Language { get; set; }

        [YAXCollection(YAXCollectionSerializationTypes.Serially, SeparateBy = ", ")]
        public List<string> Applications { get; set; }

        [YAXAttributeFor("Applications")]
        [YAXSerializeAs("require")]
        public bool RequireApplication { get; set; }

        [YAXAttributeFor("Applications")]
        [YAXSerializeAs("foreground")]
        public bool RequireForeground { get; set; }

        [YAXSerializeAs("Initialization")]
        public string InitCode { get; set; }

        public List<AbstractSymbol> Symbols { get; set; }

        public List<NewCommand> Commands { get; set; }
    }
}
