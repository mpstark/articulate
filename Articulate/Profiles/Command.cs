using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using YAXLib;
using Sprache;

namespace Articulate
{
    /// <summary>
    /// A recognizable built from Symbols command that has some action.
    /// </summary>
    [YAXSerializeAs("Command")]
    public class NewCommand
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

        #region Parser
        /// <summary>
        /// A token from a command format string.
        /// </summary>
        public class FormatToken
        {
            #region Public Members
            /// <summary>
            /// Whether or not this token is optional.
            /// </summary>
            public bool IsOptional;
            #endregion

            #region Constructors
            /// <summary>
            /// Default constructor
            /// </summary>
            public FormatToken()
            {
                IsOptional = false;
            } 
            #endregion
        }

        /// <summary>
        /// A token representing a symbol from a command format string
        /// </summary>
        public class SymbolToken : FormatToken
        {
            #region Public Members
            /// <summary>   
            /// The name of the symbol that this token represents.
            /// </summary>
            public string SymbolName; 
            #endregion

            #region Constructors
            /// <summary>
            /// Contructor
            /// </summary>
            /// <param name="symbolName">The name of the symbol that this token represents.</param>
            /// <param name="optional">Whether or not this token is optional.</param>
            public SymbolToken(string symbolName, bool optional)
            {
                SymbolName = symbolName;
                IsOptional = optional;
            } 
            #endregion
        }

        /// <summary>
        /// A token representing a pronounceable phrase or word in a command format string.
        /// </summary>
        public class PronounceableToken : FormatToken
        {
            #region Public Members
            /// <summary>
            /// The pronounceable phrase that this token represents.
            /// </summary>
            public string Phrase; 
            #endregion  

            #region Constructors
            // <summary>
            /// Contructor
            /// </summary>
            /// <param name="phrase">The pronounceable phrase that this token represents.</param>
            /// <param name="optional">Whether or not this token is optional.</param>
            public PronounceableToken(string phrase, bool optional)
            {
                Phrase = phrase;
                IsOptional = optional;
            } 
            #endregion
        }

        /// <summary>
        /// A token representing a group of other tokens in a command format string.
        /// </summary>
        public class GroupToken : FormatToken
        {
            #region Enums
            /// <summary>
            /// An enumeration of all of the possible operations that can take place in a group.
            /// </summary>
            public enum GroupOperations
            {
                /// <summary>
                /// A choice can be made between the tokens in the format string.
                /// </summary>
                Or,

                /// <summary>
                /// The tokens all must be present in the order specified.
                /// </summary>
                And
            } 
            #endregion

            #region Public Members
            /// <summary>
            /// The tokens contained within the group.
            /// </summary>
            public IEnumerable<FormatToken> Members;

            /// <summary>
            /// The operation taking place inside this group.
            /// </summary>
            public GroupOperations Operation; 
            #endregion

            #region Constructors
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="members">The members present in this group</param>
            /// <param name="optional">Whether or not this token is optional.</param>
            public GroupToken(IEnumerable<FormatToken> members, bool optional)
            {
                Members = members;
                IsOptional = optional;
                Operation = GroupOperations.And;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="members">The members present in this group</param>
            /// <param name="optional">Whether or not this token is optional.</param>
            /// <param name="operation">The operation taking place inside this group.</param>
            public GroupToken(IEnumerable<FormatToken> members, bool optional, GroupOperations operation)
            {
                Members = members;
                IsOptional = optional;
                Operation = operation;
            } 
            #endregion
        }

        /// <summary>
        /// A static class that contains all of the Sprache Parsers for parsing a command format string
        /// </summary>
        public static class FormatGrammar
        {
            #region Public Static Methods
            /// <summary>
            /// Parses a PronounceableToken
            /// </summary>
            public static readonly Parser<FormatToken> Pronounceable =
                from start in Parse.Char('"')
                from content in Parse.Letter.Or(Parse.WhiteSpace).Many().Text()
                from end in Parse.Char('"')
                from optional in Parse.Optional(Parse.Char('?'))
                select new PronounceableToken(content, optional.IsDefined && !optional.IsEmpty);

            /// <summary>
            /// Parses a SymbolToken
            /// </summary>
            public static readonly Parser<FormatToken> Symbol =
                from content in Parse.LetterOrDigit.Or(Parse.Char('_')).AtLeastOnce().Text()
                from optional in Parse.Optional(Parse.Char('?'))
                select new SymbolToken(content, optional.IsDefined && !optional.IsEmpty);

            /// <summary>
            /// Parses a GroupToken using OR operation
            /// </summary>
            public static readonly Parser<FormatToken> OrGroup =
                from start in Parse.Char('(')
                from members in Group.Or(Pronounceable).Or(Symbol).DelimitedBy(Parse.Char('|'))
                from end in Parse.Char(')')
                from optional in Parse.Optional(Parse.Char('?'))
                select new GroupToken(members, optional.IsDefined && !optional.IsEmpty, GroupToken.GroupOperations.Or);

            /// <summary>
            /// Parses a GroupToken using AND operation
            /// </summary>
            public static readonly Parser<FormatToken> AndGroup =
                from start in Parse.Char('(')
                from members in Group.Or(Pronounceable).Or(Symbol).DelimitedBy(Parse.WhiteSpace)
                from end in Parse.Char(')')
                from optional in Parse.Optional(Parse.Char('?'))
                select new GroupToken(members, optional.IsDefined && !optional.IsEmpty, GroupToken.GroupOperations.And);


            /// <summary>
            /// Parses a GroupToken
            /// </summary>
            public static readonly Parser<FormatToken> Group =
                from newGroup in AndGroup.Or(OrGroup)
                select newGroup;

            /// <summary>
            /// Parses a FormatToken
            /// </summary>
            public static readonly Parser<FormatToken> FormatToken =
                (from content in Group.Or(Pronounceable).Or(Symbol)
                 select content).Token();

            /// <summary>
            /// Parses a Format String
            /// </summary>
            public static readonly Parser<IEnumerable<FormatToken>> Format =
                from content in FormatToken.DelimitedBy(Parse.WhiteSpace)
                select content;
            #endregion
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
