using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Diagnostics;

namespace Articulate
{
    abstract class CommandChunk
    {
        /// <summary>
        /// Semantic to KeyList lookup table
        /// </summary>
        public Dictionary<string, List<INPUT[]>> KeyLookup { get; protected set; }
        
        /// <summary>
        /// Changes a CommandChunk's KeyList for a particular semantic
        /// </summary>
        /// <param name="semantic">Semantic to change the keylist for</param>
        /// <param name="keys">KeyList to change to</param>
        /// <returns>true if keylist changed</returns>
        public bool ChangeKey(string semantic, List<INPUT[]> keys)
        {
            if (KeyLookup.ContainsKey(semantic))
            {
                KeyLookup[semantic] = keys;
                return true;
            }

            return false;
        }

        /// <summary>
        /// List of SrgsRules that match the spoken input
        /// </summary>
        public List<SrgsRule> RuleList { get; protected set; }

        /// <summary>
        /// The root rule for the CommandChunk
        /// </summary>
        public SrgsRule RootRule { get; protected set; }

        /// <summary>
        /// Gets a new SrgsItem with the spoken word alternates and a semantic assoicated
        /// </summary>
        /// <param name="alternates">Array of strings that are all alternate spoken word triggers</param>
        /// <param name="semantic">Semanitic assoicated</param>
        /// <returns>A new SrgsItem</returns>
        protected static SrgsItem GetNewNode(string[] alternates, string semantic)
        {
            return new SrgsItem(new SrgsOneOf(alternates), new SrgsSemanticInterpretationTag("out += \"{" + semantic + "}\";"));
        }
    }
}
