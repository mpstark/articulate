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
    /// A symbol that is used to generate a SrgsRule.
    /// </summary>
    public abstract class AbstractSymbol
    {
        #region Public Properties
        /// <summary>
        /// The symbol's name. Has to be unique in the profile namespace.
        /// </summary>
        [YAXAttributeFor(".")]
        [YAXSerializeAs("name")]
        public string Name { get; set; }

        /// <summary>
        /// Rule generated from symbol.
        /// </summary>
        [YAXDontSerialize]
        public SrgsRule Rule
        {
            get
            {
                if (_rule == null)
                    GenerateRule();

                return _rule;
            }

            protected set
            {
                _rule = value;
            }
        }

        /// <summary>
        /// Internal storage for Rule
        /// </summary>
        private SrgsRule _rule;
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets all of the rules that are underneath this symbol.
        /// </summary>
        /// <returns>A List containing all of the SrgsRules</returns>
        public abstract Dictionary<string, SrgsRule> GetRules();
        #endregion

        #region Protected Methods
        /// <summary>
        /// Generate the SrgsRule that this symbol represents.
        /// </summary>
        protected abstract void GenerateRule();
        #endregion
    }

    /// <summary>
    /// A symbol that contains ties a name to a value and a list of pronounceables 
    /// </summary>
    public class Symbol : AbstractSymbol
    {
        #region Public Properties
        /// <summary>
        /// The value that the symbol resolves to. Doesn't have to be unique.
        /// </summary>
        [YAXAttributeFor(".")]
        [YAXSerializeAs("value")]
        public string Value { get; set; }

        /// <summary>
        /// The list of spoken phrases/words that trigger this symbol.
        /// </summary>
        [YAXCollection(YAXCollectionSerializationTypes.Serially, SeparateBy = "; ")]
        public List<string> Pronounceables { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Symbol()
        {
            Name = "";
            Value = "";
            Pronounceables = new List<string>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets all of the rules that are underneath this symbol.
        /// </summary>
        /// <returns>A List containing all of the SrgsRules</returns>
        public override Dictionary<string, SrgsRule> GetRules()
        {
            Dictionary<string, SrgsRule> ruleList = new Dictionary<string, SrgsRule>();
            ruleList.Add(Name, Rule);
            return ruleList;
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Generate the SrgsRule that this symbol represents.
        /// </summary>
        protected override void GenerateRule()
        {
            // inits a new SrgsRule with a single item containing the pronounceable choice and the semantic tag and returns it
            // TODO: make sure that this is the way that we should be using SrgsSemanticInterpretationTag
            Rule = new SrgsRule(Name, new SrgsItem(new SrgsOneOf(Pronounceables.ToArray()), new SrgsSemanticInterpretationTag("out += \"" + Name + "; \"")));
        }
        #endregion
    }

    /// <summary>
    /// A symbol that contains other symbols, of type Symbol and of type SymbolGroup
    /// </summary>
    public class SymbolGroup : AbstractSymbol
    {
        #region Public Properties
        /// <summary>
        /// The symbols that this SymbolGroup contains.
        /// </summary>
        public List<AbstractSymbol> Members { get; set; }

        /// <summary>
        /// Spoken words/phrases that do not have any semantic meaning and come before this symbol
        /// </summary>
        [YAXCollection(YAXCollectionSerializationTypes.Serially, SeparateBy = "; ")]
        public List<string> Prefix { get; set; }

        /// <summary>
        /// Whether or not the prefix collection is required or optional. Only valid if Prefix exists and has words/phrases.
        /// </summary>
        [YAXAttributeFor("Prefix")]
        [YAXSerializeAs("required")]
        public bool PrefixRequired { get; set; }

        /// <summary>
        /// Spoken words/phrases that do not have any semantic meaning and come after this symbol
        /// </summary>
        [YAXCollection(YAXCollectionSerializationTypes.Serially, SeparateBy = "; ")]
        public List<string> Postfix { get; set; }

        /// <summary>
        /// Whether or not the postfix collection is required or optional. Only valid if Prefix exists and has words/phrases.
        /// </summary>
        [YAXAttributeFor("Postfix")]
        [YAXSerializeAs("required")]
        public bool PostfixRequired { get; set; }

        /// <summary>
        /// Number of times that symbols in this SymbolGroup can repeat. Must 1 or greater. 1 is default.
        /// </summary>
        [YAXAttributeFor(".")]
        [YAXSerializeAs("repeat")]
        public int Repeat { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constuctor
        /// </summary>
        public SymbolGroup()
        {
            Members = new List<AbstractSymbol>();
            Prefix = new List<string>();
            PrefixRequired = false;
            Postfix = new List<string>();
            PostfixRequired = false;
            Repeat = 1;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets all of the rules that are underneath this symbol.
        /// </summary>
        /// <returns>A List containing all of the SrgsRules</returns>
        public override Dictionary<string, SrgsRule> GetRules()
        {
            Dictionary<string, SrgsRule> ruleList = new Dictionary<string, SrgsRule>();
            
            // add all of the rules underneath
            foreach(AbstractSymbol symbol in Members)
            {
                foreach (var pair in symbol.GetRules())
                    ruleList.Add(pair.Key, pair.Value);
            }
            
            // add this rule
            ruleList.Add(Name, Rule);

            return ruleList;
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Generate the SrgsRule that this symbol represents.
        /// </summary>
        protected override void GenerateRule()
        {
            Rule = new SrgsRule(Name);
            SrgsItem symbolItem = new SrgsItem();

            // add the prefix
            if(Prefix.Count > 0)
            {
                symbolItem.Add(new SrgsItem(PrefixRequired ? 1 : 0, 1, new SrgsOneOf(Prefix.ToArray())));
            }

            // add each of the members RuleRefs into a choice
            SrgsOneOf memberChoice = new SrgsOneOf();
            foreach (AbstractSymbol symbol in Members)
            {
                memberChoice.Add(new SrgsItem(new SrgsRuleRef(symbol.Rule)));
            }

            // can recognize 1 to Repeat of these symbols
            symbolItem.Add(new SrgsItem(1, Repeat, memberChoice));

            // add the postfix
            if (Postfix.Count > 0)
            {
                symbolItem.Add(new SrgsItem(PostfixRequired ? 1 : 0, 1, new SrgsOneOf(Postfix.ToArray())));
            }

            Rule.Add(symbolItem);
        }
        #endregion
    }
}
