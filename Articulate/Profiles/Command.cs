using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAXLib;

namespace Articulate
{
    /// <summary>
    /// A recognizable built from Symbols command that has some action.
    /// </summary>
    [YAXSerializeAs("Command")]
    class NewCommand
    {
        #region Public Properties
        /// <summary>
        /// The name of the command. Must be unique within containing profile.
        /// </summary>
        [YAXAttributeFor("..")]
        [YAXSerializeAs("name")]
        public string Name { get; set; }

        /// <summary>
        /// The format of the command.
        /// 
        /// Format specification:
        /// 
        /// TODO: fill in
        /// </summary>
        public string Format { get; set;}

        /// <summary>
        /// The Lua output of the command.
        /// </summary>
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

        #region Public Methods
        
        #endregion
    }
}
