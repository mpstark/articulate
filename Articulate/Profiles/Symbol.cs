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
    abstract class AbstractSymbol
    {
        #region Public Properties
        /// <summary>
        /// The symbol's name. Has to be unique in the profile namespace.
        /// </summary>
        [YAXAttributeFor("..")]
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

        #region Protected Methods
        protected abstract void GenerateRule();
        #endregion
    }

    /// <summary>
    /// A symbol that contains ties a name to a value and a list of pronounceables 
    /// </summary>
    class Symbol : AbstractSymbol
    {
        #region Public Properties
        /// <summary>
        /// The value that the symbol resolves to. Doesn't have to be unique.
        /// </summary>
        [YAXAttributeFor("..")]
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
            Value = "";
            Pronounceables = new List<string>();
        }
        #endregion

        #region Protected Methods
        protected override void GenerateRule()
        {
            // inits a new SrgsRule with a single item containing the pronounceable choice and the semantic tag and returns it
            Rule = new SrgsRule(Name, new SrgsItem(new SrgsOneOf(Pronounceables.ToArray()), new SrgsSemanticInterpretationTag("out += \"{" + Name + "}\";")));
        }
        #endregion
    }

    /// <summary>
    /// A symbol that contains other symbols, of type Symbol and of type SymbolGroup
    /// </summary>
    class SymbolGroup : AbstractSymbol
    {
        #region Public Properties
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
        #endregion

        #region Constructors
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

        #region Protected Methods
        protected override void GenerateRule()
        {
            Rule = new SrgsRule(Name);
            SrgsItem symbolItem = new SrgsItem();

            // add the prefix
            if(Prefix.Count > 0)
            {
                symbolItem.Add(new SrgsItem(PrefixRequired ? 1 : 0, 1, new SrgsOneOf(Prefix.ToArray())));
            }

            // add each of the members RuleRefs
            SrgsOneOf memberChoice = new SrgsOneOf();
            foreach (AbstractSymbol symbol in Members)
            {
                memberChoice.Add(new SrgsItem(new SrgsRuleRef(symbol.Rule)));
            }
            symbolItem.Add(memberChoice);
            symbolItem.Add(new SrgsSemanticInterpretationTag("out = rules.latest();")); // unsure if needed

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
