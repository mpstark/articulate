using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAXLib;

namespace Articulate
{
    // soon to be renamed when remove old system
    [YAXSerializeAs("Command")]
    class NewCommand
    {
        #region Public Properties
        [YAXAttributeFor("..")]
        [YAXSerializeAs("name")]
        public string Name { get; set; }

        public string Format { get; set;}

        public string Output { get; set; }
        #endregion

        #region Constructors
        public NewCommand()
        {
            Name = "";
            Format = "";
            Output = "";
        }
        #endregion
    }
}
