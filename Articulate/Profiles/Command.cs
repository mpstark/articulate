using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using YAXLib;
using System.Text.RegularExpressions;

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
        /// Enclosed in:
        /// []: required
        /// {}: optional
        /// 
        /// symbols are not enclosed in ""
        /// semantic-less words/phrases must be inclosed in ""
        /// 
        /// Inside [] or {}, can have | to indicate OR
        /// Cannot mix symbols and semantic-less words/phrases
        /// 
        /// Examples:
        /// 
        /// {"fucking"|"damn"|"cocksucking"|"god damn"} : optional, semantic-less, word choice.
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
        /// <summary>
        /// Creates a SrgsItem that conforms to the Format string and a list of symbol names with their rules
        /// </summary>
        /// <param name="availableRules">A dictionary holding the available symbols and their rules</param>
        /// <returns>A SrgsItem that matches the Format string</returns>
        public SrgsItem GenerateSrgsItem(Dictionary<string, SrgsRule> availableRules)
        {
            SrgsItem commandItem = new SrgsItem();

            return commandItem;
        }
        #endregion
    }
}
