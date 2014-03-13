using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using YAXLib;

namespace Articulate
{
    /// <summary>
    /// An Articulate XML+Lua-based profile.
    /// </summary>
    public class Profile
    {
        #region Enums
        /// <summary>
        /// The status of the application that must be true for recognition to be valid
        /// </summary>
        public enum ApplicationRequirements
        {
            /// <summary>
            /// Application must be running
            /// </summary>
            Running,

            /// <summary>
            /// Application must be running in the foreground
            /// </summary>
            Foreground
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Name of the profile. Must be unique between loaded profiles.
        /// </summary>
        [YAXAttributeFor("..")]
        [YAXSerializeAs("name")]
        public string Name { get; set; }

        /// <summary>
        /// Locale of the profile that is loaded. This will be the recognizor that is used for this profile.
        /// </summary>
        [YAXAttributeFor("..")]
        [YAXSerializeAs("locale")]
        public string Locale { get; set; }

        /// <summary>
        /// The applications that this profile affects.
        /// </summary>
        [YAXCollection(YAXCollectionSerializationTypes.Serially, SeparateBy = ", ")]
        public List<string> Applications { get; set; }

        /// <summary>
        /// The state of the application required.
        /// </summary>
        [YAXAttributeFor("Applications")]
        [YAXSerializeAs("require")]
        public ApplicationRequirements AppRequirements { get; set; }

        /// <summary>
        /// A block of code that is run on profile namespace initilization.
        /// </summary>
        [YAXSerializeAs("Initialization")]
        public string InitCode { get; set; }

        /// <summary>
        /// A file containing Lua that is run on profile namespace initilization.
        /// </summary>
        [YAXSerializeAs("Include")]
        public string IncludeFile { get; set; }

        /// <summary>
        /// The symbols that the profile is defined with.
        /// </summary>
        public List<AbstractSymbol> Symbols { get; set; }

        /// <summary>
        /// The Commands that are recognized and acted upon.
        /// </summary>
        public List<NewCommand> Commands { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Profile()
        {
            Name = "";
            Locale = "en";
            Applications = new List<string>();
            AppRequirements = ApplicationRequirements.Running;
            InitCode = "";
            Symbols = new List<AbstractSymbol>();
            Commands = new List<NewCommand>();
        }
        #endregion

        #region Public Methods
        public SrgsDocument CompileToSrgsDocument()
        {
            SrgsDocument document = new SrgsDocument();
            document.Culture = new System.Globalization.CultureInfo(Locale);
            Dictionary<string, SrgsRule> rules = new Dictionary<string, SrgsRule>();

            // go through all of the symbols and add their rules
            foreach(AbstractSymbol symbol in Symbols)
            {
                Dictionary<string, SrgsRule> ruleList = symbol.GetRules();
                foreach (var rulePair in ruleList)
                {
                    rules.Add(rulePair.Key, rulePair.Value);
                    document.Rules.Add(rulePair.Value);
                }
            }

            // go through all of the commands, add their SrgsItem's to a SrgsRule that will become the root of the document
            // the root of the document is where all recognition starts
            SrgsRule newRootRule = new SrgsRule(Name + " Commands");
            SrgsOneOf commandChoice = new SrgsOneOf();
            foreach(NewCommand command in Commands)
            {
                commandChoice.Add(command.GenerateSrgsItem(rules));
            }

            document.Rules.Add(newRootRule);
            document.Root = newRootRule;
            return document;
        }
        #endregion
    }
}
