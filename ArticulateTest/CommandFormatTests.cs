using System;
using System.Linq;
using Articulate;
using Sprache;
using NUnit.Framework;
using System.Collections.Generic;

namespace ArticulateTest
{
    [TestFixture]
    public class CommandFormatSymbolTokenTests
    {
        [Test]
        public void SymbolNormal()
        {
            string parseMe = "hello";
            NewCommand.SymbolToken symbol = (NewCommand.SymbolToken) NewCommand.FormatGrammar.Symbol.End().Parse(parseMe);
            Assert.AreEqual(symbol.SymbolName, "hello");
            Assert.IsFalse(symbol.IsOptional);
        }

        [Test]
        public void SymbolWithNumberSymbol()
        {
            string parseMe = "hello32";
            NewCommand.SymbolToken symbol = (NewCommand.SymbolToken)NewCommand.FormatGrammar.Symbol.End().Parse(parseMe);
            Assert.AreEqual(symbol.SymbolName, "hello32");
            Assert.IsFalse(symbol.IsOptional);
        }

        [Test]
        public void SymbolWithNumberAndUnderscore()
        {
            string parseMe = "hello_32";
            NewCommand.SymbolToken symbol = (NewCommand.SymbolToken)NewCommand.FormatGrammar.Symbol.End().Parse(parseMe);
            Assert.AreEqual(symbol.SymbolName, "hello_32");
            Assert.IsFalse(symbol.IsOptional);
        }

        [Test]
        public void OptionalSymbol()
        {
            string parseMe = "hello_32?";
            NewCommand.SymbolToken symbol = (NewCommand.SymbolToken)NewCommand.FormatGrammar.Symbol.End().Parse(parseMe);
            Assert.AreEqual(symbol.SymbolName, "hello_32");
            Assert.IsTrue(symbol.IsOptional);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void SymbolWithUnrecognizedSymbol()
        {
            string parseMe = "he%llo_32";
            NewCommand.SymbolToken symbol = (NewCommand.SymbolToken)NewCommand.FormatGrammar.Symbol.End().Parse(parseMe);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void SymbolWithBeginUnrecognizedSymbol()
        {
            string parseMe = "%hello_32";
            NewCommand.SymbolToken symbol = (NewCommand.SymbolToken)NewCommand.FormatGrammar.Symbol.End().Parse(parseMe);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void SymbolWithEndUnrecognizedSymbol()
        {
            string parseMe = "hello_32%";
            NewCommand.SymbolToken symbol = (NewCommand.SymbolToken)NewCommand.FormatGrammar.Symbol.End().Parse(parseMe);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void OptionalSymbolWithEndUnrecognizedSymbol()
        {
            string parseMe = "hello_32?%";
            NewCommand.SymbolToken symbol = (NewCommand.SymbolToken)NewCommand.FormatGrammar.Symbol.End().Parse(parseMe);
        }
    }

    [TestFixture]
    public class CommandFormatPronounceableTokenTests
    {
        [Test]
        public void PronounceableNormal()
        {
            string parseMe = "\"hello\"";
            NewCommand.PronounceableToken pronounceable = (NewCommand.PronounceableToken)NewCommand.FormatGrammar.Pronounceable.End().Parse(parseMe);
            Assert.AreEqual(pronounceable.Phrase, "hello");
            Assert.IsFalse(pronounceable.IsOptional);
        }

        [Test]
        public void PronounceableWithSpace()
        {
            string parseMe = "\"hello world\"";
            NewCommand.PronounceableToken pronounceable = (NewCommand.PronounceableToken)NewCommand.FormatGrammar.Pronounceable.End().Parse(parseMe);
            Assert.AreEqual(pronounceable.Phrase, "hello world");
            Assert.IsFalse(pronounceable.IsOptional);
        }

        [Test]
        public void PronounceableWithATab()
        {
            string parseMe = "\"hello\tworld\"";
            NewCommand.PronounceableToken pronounceable = (NewCommand.PronounceableToken)NewCommand.FormatGrammar.Pronounceable.End().Parse(parseMe);
            Assert.AreEqual(pronounceable.Phrase, "hello\tworld");
            Assert.IsFalse(pronounceable.IsOptional);
        }

        [Test]
        public void OptionalPronounceable()
        {
            string parseMe = "\"hello world\"?";
            NewCommand.PronounceableToken pronounceable = (NewCommand.PronounceableToken)NewCommand.FormatGrammar.Pronounceable.End().Parse(parseMe);
            Assert.AreEqual(pronounceable.Phrase, "hello world");
            Assert.IsTrue(pronounceable.IsOptional);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void PronounceableWithImproperSymbol()
        {
            string parseMe = "\"hello$ world\"";
            NewCommand.PronounceableToken pronounceable = (NewCommand.PronounceableToken)NewCommand.FormatGrammar.Pronounceable.End().Parse(parseMe);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void PronounceableWithImproperSymbolAfterFirstQuote()
        {
            string parseMe = "\"$hello world\"";
            NewCommand.PronounceableToken pronounceable = (NewCommand.PronounceableToken)NewCommand.FormatGrammar.Pronounceable.End().Parse(parseMe);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void PronounceableWithImproperSymbolBeforeFirstQuote()
        {
            string parseMe = "$\"hello world\"";
            NewCommand.PronounceableToken pronounceable = (NewCommand.PronounceableToken)NewCommand.FormatGrammar.Pronounceable.End().Parse(parseMe);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void PronounceableWithImproperSymbolBeforeLastQuote()
        {
            string parseMe = "\"hello world$\"";
            NewCommand.PronounceableToken pronounceable = (NewCommand.PronounceableToken)NewCommand.FormatGrammar.Pronounceable.End().Parse(parseMe);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void PronounceableWithImproperSymbolAfterLastQuote()
        {
            string parseMe = "\"hello world\"$";
            NewCommand.PronounceableToken pronounceable = (NewCommand.PronounceableToken)NewCommand.FormatGrammar.Pronounceable.End().Parse(parseMe);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void OptionalPronounceableWithImproperSymbolBeforeQuestionMark()
        {
            string parseMe = "\"hello world\"$?";
            NewCommand.PronounceableToken pronounceable = (NewCommand.PronounceableToken)NewCommand.FormatGrammar.Pronounceable.End().Parse(parseMe);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void OptionalPronounceableWithImproperSymbolAfterQuestionMark()
        {
            string parseMe = "\"hello world\"?$";
            NewCommand.PronounceableToken pronounceable = (NewCommand.PronounceableToken)NewCommand.FormatGrammar.Pronounceable.End().Parse(parseMe);
        }
    }

    [TestFixture]
    public class CommandFormatGroupTokenTests
    {
        [Test]
        public void ANDGroupOfOneSymbol()
        {
            string parseMe = "(hello)";
            NewCommand.GroupToken group = (NewCommand.GroupToken)NewCommand.FormatGrammar.Group.End().Parse(parseMe);
            Assert.IsFalse(group.IsOptional);
            Assert.AreEqual(group.Operation, NewCommand.GroupToken.GroupOperations.And);

            NewCommand.SymbolToken first = group.Members.First() as NewCommand.SymbolToken;
            Assert.AreEqual(first.SymbolName, "hello");
            Assert.IsFalse(first.IsOptional);
        }

        [Test]
        public void ANDGroupOfTwoSymbols()
        {
            string parseMe = "(hello world)";
            NewCommand.GroupToken group = (NewCommand.GroupToken)NewCommand.FormatGrammar.Group.End().Parse(parseMe);
            Assert.IsFalse(group.IsOptional);
            Assert.AreEqual(group.Operation, NewCommand.GroupToken.GroupOperations.And);

            NewCommand.SymbolToken first = group.Members.ToArray()[0] as NewCommand.SymbolToken;
            Assert.AreEqual(first.SymbolName, "hello");
            Assert.IsFalse(first.IsOptional);

            NewCommand.SymbolToken second = group.Members.ToArray()[1] as NewCommand.SymbolToken;
            Assert.AreEqual(second.SymbolName, "world");
            Assert.IsFalse(second.IsOptional);
        }

        [Test]
        public void ANDGroupOfTwoSymbolsWithOddWhitespace()
        {
            string parseMe = "(hello \t  world)";
            NewCommand.GroupToken group = (NewCommand.GroupToken)NewCommand.FormatGrammar.Group.End().Parse(parseMe);
            Assert.IsFalse(group.IsOptional);
            Assert.AreEqual(group.Operation, NewCommand.GroupToken.GroupOperations.And);

            NewCommand.SymbolToken first = group.Members.ToArray()[0] as NewCommand.SymbolToken;
            Assert.AreEqual(first.SymbolName, "hello");
            Assert.IsFalse(first.IsOptional);

            NewCommand.SymbolToken second = group.Members.ToArray()[1] as NewCommand.SymbolToken;
            Assert.AreEqual(second.SymbolName, "world");
            Assert.IsFalse(second.IsOptional);
        }

        [Test]
        public void ANDGroupOfOneSymbolOnePronounceable()
        {
            string parseMe = "(hello \"world\")";
            NewCommand.GroupToken group = (NewCommand.GroupToken)NewCommand.FormatGrammar.Group.End().Parse(parseMe);
            Assert.IsFalse(group.IsOptional);
            Assert.AreEqual(group.Operation, NewCommand.GroupToken.GroupOperations.And);

            NewCommand.SymbolToken first = group.Members.ToArray()[0] as NewCommand.SymbolToken;
            Assert.AreEqual(first.SymbolName, "hello");
            Assert.IsFalse(first.IsOptional);

            NewCommand.PronounceableToken second = group.Members.ToArray()[1] as NewCommand.PronounceableToken;
            Assert.AreEqual(second.Phrase, "world");
            Assert.IsFalse(second.IsOptional);
        }

        [Test]
        public void ORGroupOfTwoSymbols()
        {
            string parseMe = "(hello|world)";
            NewCommand.GroupToken group = (NewCommand.GroupToken)NewCommand.FormatGrammar.Group.End().Parse(parseMe);
            Assert.IsFalse(group.IsOptional);
            Assert.AreEqual(group.Operation, NewCommand.GroupToken.GroupOperations.Or);

            NewCommand.SymbolToken first = group.Members.ToArray()[0] as NewCommand.SymbolToken;
            Assert.AreEqual(first.SymbolName, "hello");
            Assert.IsFalse(first.IsOptional);

            NewCommand.SymbolToken second = group.Members.ToArray()[1] as NewCommand.SymbolToken;
            Assert.AreEqual(second.SymbolName, "world");
            Assert.IsFalse(second.IsOptional);
        }

        [Test]
        public void ORGroupOneOfSymbolOnePronounceable()
        {
            string parseMe = "(hello|\"world\")";
            NewCommand.GroupToken group = (NewCommand.GroupToken)NewCommand.FormatGrammar.Group.End().Parse(parseMe);
            Assert.IsFalse(group.IsOptional);
            Assert.AreEqual(group.Operation, NewCommand.GroupToken.GroupOperations.Or);

            NewCommand.SymbolToken first = group.Members.ToArray()[0] as NewCommand.SymbolToken;
            Assert.AreEqual(first.SymbolName, "hello");
            Assert.IsFalse(first.IsOptional);

            NewCommand.PronounceableToken second = group.Members.ToArray()[1] as NewCommand.PronounceableToken;
            Assert.AreEqual(second.Phrase, "world");
            Assert.IsFalse(second.IsOptional);
        }

        [Test]
        public void GroupOfOneGroupOfSymbolsOneSymbol()
        {
            string parseMe = "((hello world) bye)";
            NewCommand.GroupToken group = (NewCommand.GroupToken)NewCommand.FormatGrammar.Group.End().Parse(parseMe);
            Assert.IsFalse(group.IsOptional);
            Assert.AreEqual(group.Operation, NewCommand.GroupToken.GroupOperations.And);

            NewCommand.GroupToken first = group.Members.ToArray()[0] as NewCommand.GroupToken;
            NewCommand.SymbolToken firstfirst = first.Members.ToArray()[0] as NewCommand.SymbolToken;
            Assert.AreEqual(firstfirst.SymbolName, "hello");
            Assert.IsFalse(firstfirst.IsOptional);
            Assert.AreEqual(first.Operation, NewCommand.GroupToken.GroupOperations.And);
            Assert.IsFalse(first.IsOptional);

            NewCommand.SymbolToken firstsecond = first.Members.ToArray()[1] as NewCommand.SymbolToken;
            Assert.AreEqual(firstsecond.SymbolName, "world");
            Assert.IsFalse(firstsecond.IsOptional);

            NewCommand.SymbolToken second = group.Members.ToArray()[1] as NewCommand.SymbolToken;
            Assert.AreEqual(second.SymbolName, "bye");
            Assert.IsFalse(second.IsOptional);
        }

        [Test]
        public void OptionalGroupOfOneGroupOfSymbolsOneSymbol()
        {
            string parseMe = "((hello world) bye)?";
            NewCommand.GroupToken group = (NewCommand.GroupToken)NewCommand.FormatGrammar.Group.End().Parse(parseMe);
            Assert.IsTrue(group.IsOptional);
            Assert.AreEqual(group.Operation, NewCommand.GroupToken.GroupOperations.And);

            NewCommand.GroupToken first = group.Members.ToArray()[0] as NewCommand.GroupToken;
            NewCommand.SymbolToken firstfirst = first.Members.ToArray()[0] as NewCommand.SymbolToken;
            Assert.AreEqual(firstfirst.SymbolName, "hello");
            Assert.IsFalse(firstfirst.IsOptional);
            Assert.AreEqual(first.Operation, NewCommand.GroupToken.GroupOperations.And);
            Assert.IsFalse(first.IsOptional);

            NewCommand.SymbolToken firstsecond = first.Members.ToArray()[1] as NewCommand.SymbolToken;
            Assert.AreEqual(firstsecond.SymbolName, "world");
            Assert.IsFalse(firstsecond.IsOptional);

            NewCommand.SymbolToken second = group.Members.ToArray()[1] as NewCommand.SymbolToken;
            Assert.AreEqual(second.SymbolName, "bye");
            Assert.IsFalse(second.IsOptional);
        }

        [Test]
        public void GroupOfOneOptionalGroupOfSymbolsOneSymbol()
        {
            string parseMe = "((hello world)? bye)";
            NewCommand.GroupToken group = (NewCommand.GroupToken)NewCommand.FormatGrammar.Group.End().Parse(parseMe);
            Assert.IsFalse(group.IsOptional);
            Assert.AreEqual(group.Operation, NewCommand.GroupToken.GroupOperations.And);

            NewCommand.GroupToken first = group.Members.ToArray()[0] as NewCommand.GroupToken;
            NewCommand.SymbolToken firstfirst = first.Members.ToArray()[0] as NewCommand.SymbolToken;
            Assert.AreEqual(firstfirst.SymbolName, "hello");
            Assert.IsFalse(firstfirst.IsOptional);
            Assert.AreEqual(first.Operation, NewCommand.GroupToken.GroupOperations.And);
            Assert.IsTrue(first.IsOptional);

            NewCommand.SymbolToken firstsecond = first.Members.ToArray()[1] as NewCommand.SymbolToken;
            Assert.AreEqual(firstsecond.SymbolName, "world");
            Assert.IsFalse(firstsecond.IsOptional);

            NewCommand.SymbolToken second = group.Members.ToArray()[1] as NewCommand.SymbolToken;
            Assert.AreEqual(second.SymbolName, "bye");
            Assert.IsFalse(second.IsOptional);
        }

        [Test]
        public void GroupOfOneOptionalORGroupOfSymbolsOneSymbol()
        {
            string parseMe = "((hello|world)? bye)";
            NewCommand.GroupToken group = (NewCommand.GroupToken)NewCommand.FormatGrammar.Group.End().Parse(parseMe);
            Assert.IsFalse(group.IsOptional);
            Assert.AreEqual(group.Operation, NewCommand.GroupToken.GroupOperations.And);

            NewCommand.GroupToken first = group.Members.ToArray()[0] as NewCommand.GroupToken;
            NewCommand.SymbolToken firstfirst = first.Members.ToArray()[0] as NewCommand.SymbolToken;
            Assert.AreEqual(firstfirst.SymbolName, "hello");
            Assert.IsFalse(firstfirst.IsOptional);
            Assert.AreEqual(first.Operation, NewCommand.GroupToken.GroupOperations.Or);
            Assert.IsTrue(first.IsOptional);

            NewCommand.SymbolToken firstsecond = first.Members.ToArray()[1] as NewCommand.SymbolToken;
            Assert.AreEqual(firstsecond.SymbolName, "world");
            Assert.IsFalse(firstsecond.IsOptional);

            NewCommand.SymbolToken second = group.Members.ToArray()[1] as NewCommand.SymbolToken;
            Assert.AreEqual(second.SymbolName, "bye");
            Assert.IsFalse(second.IsOptional);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void GroupWithImproperSymbolInside()
        {
            string parseMe = "(hello%)";
            NewCommand.GroupToken group = (NewCommand.GroupToken)NewCommand.FormatGrammar.Group.End().Parse(parseMe);
            NewCommand.SymbolToken first = group.Members.First() as NewCommand.SymbolToken;
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void GroupWithImproperSymbolBefore()
        {
            string parseMe = "%(hello)";
            NewCommand.GroupToken group = (NewCommand.GroupToken)NewCommand.FormatGrammar.Group.End().Parse(parseMe);
            NewCommand.SymbolToken first = group.Members.First() as NewCommand.SymbolToken;
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void GroupWithImproperSymbolAfter()
        {
            string parseMe = "(hello)%";
            NewCommand.GroupToken group = (NewCommand.GroupToken)NewCommand.FormatGrammar.Group.End().Parse(parseMe);
            NewCommand.SymbolToken first = group.Members.First() as NewCommand.SymbolToken;
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void OptionalGroupWithImproperSymbolAfter()
        {
            string parseMe = "(hello)%?";
            NewCommand.GroupToken group = (NewCommand.GroupToken)NewCommand.FormatGrammar.Group.End().Parse(parseMe);
            NewCommand.SymbolToken first = group.Members.First() as NewCommand.SymbolToken;
        }
    }

    [TestFixture]
    public class CommandFormatStringTests
    {
        [Test]
        public void OneSymbol()
        {
            string parseMe = "hello";
            IEnumerable<NewCommand.FormatToken> result = NewCommand.FormatGrammar.Format.End().Parse(parseMe);

            NewCommand.SymbolToken first = result.First() as NewCommand.SymbolToken;
            Assert.AreEqual(first.SymbolName, "hello");
            Assert.IsFalse(first.IsOptional);
        }

        [Test]
        public void OneSymbolWithBeginningWhitespace()
        {
            string parseMe = "   \thello";
            IEnumerable<NewCommand.FormatToken> result = NewCommand.FormatGrammar.Format.End().Parse(parseMe);

            NewCommand.SymbolToken first = result.First() as NewCommand.SymbolToken;
            Assert.AreEqual(first.SymbolName, "hello");
            Assert.IsFalse(first.IsOptional);
        }

        [Test]
        public void TwoSymbolOneOptional()
        {
            string parseMe = "hello? world";
            IEnumerable<NewCommand.FormatToken> result = NewCommand.FormatGrammar.Format.End().Parse(parseMe);

            NewCommand.SymbolToken first = result.ToArray()[0] as NewCommand.SymbolToken;
            Assert.AreEqual(first.SymbolName, "hello");
            Assert.IsTrue(first.IsOptional);

            NewCommand.SymbolToken second = result.ToArray()[1] as NewCommand.SymbolToken;
            Assert.AreEqual(second.SymbolName, "world");
            Assert.IsFalse(second.IsOptional);
        }

        [Test]
        public void OneSymbolOnePronouncible()
        {
            string parseMe = "hello \"world\"";
            IEnumerable<NewCommand.FormatToken> result = NewCommand.FormatGrammar.Format.End().Parse(parseMe);

            NewCommand.SymbolToken first = result.ToArray()[0] as NewCommand.SymbolToken;
            Assert.AreEqual(first.SymbolName, "hello");
            Assert.IsFalse(first.IsOptional);

            NewCommand.PronounceableToken second = result.ToArray()[1] as NewCommand.PronounceableToken;
            Assert.AreEqual(second.Phrase, "world");
            Assert.IsFalse(second.IsOptional);
        }

        [Test]
        public void OneSymbolOnePronouncibleWithOddWhitespace()
        {
            string parseMe = "\thello \t \"world\"   ";
            IEnumerable<NewCommand.FormatToken> result = NewCommand.FormatGrammar.Format.End().Parse(parseMe);

            NewCommand.SymbolToken first = result.ToArray()[0] as NewCommand.SymbolToken;
            Assert.AreEqual(first.SymbolName, "hello");
            Assert.IsFalse(first.IsOptional);

            NewCommand.PronounceableToken second = result.ToArray()[1] as NewCommand.PronounceableToken;
            Assert.AreEqual(second.Phrase, "world");
            Assert.IsFalse(second.IsOptional);
        }

        [Test]
        public void EmailExample()
        {
            // Everytime I read this, I'm a little bit offended.
            // I wrote it.
            string parseMe = "(\"fucking\"|\"damn\"|\"cocksucking\"|\"god damn\")? subject? formation";
            IEnumerable<NewCommand.FormatToken> result = NewCommand.FormatGrammar.Format.End().Parse(parseMe);

            NewCommand.GroupToken first = result.ToArray()[0] as NewCommand.GroupToken;
            Assert.IsTrue(first.IsOptional);
            Assert.AreEqual(first.Operation, NewCommand.GroupToken.GroupOperations.Or);

            NewCommand.PronounceableToken firstfirst = first.Members.ToArray()[0] as NewCommand.PronounceableToken;
            Assert.AreEqual(firstfirst.Phrase, "fucking");
            Assert.IsFalse(firstfirst.IsOptional);

            NewCommand.PronounceableToken firstsecond = first.Members.ToArray()[1] as NewCommand.PronounceableToken;
            Assert.AreEqual(firstsecond.Phrase, "damn");
            Assert.IsFalse(firstsecond.IsOptional);

            NewCommand.PronounceableToken firstthird = first.Members.ToArray()[2] as NewCommand.PronounceableToken;
            Assert.AreEqual(firstthird.Phrase, "cocksucking");
            Assert.IsFalse(firstthird.IsOptional);

            NewCommand.PronounceableToken firstfourth = first.Members.ToArray()[3] as NewCommand.PronounceableToken;
            Assert.AreEqual(firstfourth.Phrase, "god damn");
            Assert.IsFalse(firstfourth.IsOptional);

            NewCommand.SymbolToken second = result.ToArray()[1] as NewCommand.SymbolToken;
            Assert.AreEqual(second.SymbolName, "subject");
            Assert.IsTrue(second.IsOptional);

            NewCommand.SymbolToken third = result.ToArray()[2] as NewCommand.SymbolToken;
            Assert.AreEqual(third.SymbolName, "formation");
            Assert.IsFalse(third.IsOptional);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void ImproperSymbolAfter()
        {
            string parseMe = " hello  $  ";
            IEnumerable<NewCommand.FormatToken> result = NewCommand.FormatGrammar.Format.End().Parse(parseMe);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void ImproperSymbolBefore()
        {
            string parseMe = "$  hello    ";
            IEnumerable<NewCommand.FormatToken> result = NewCommand.FormatGrammar.Format.End().Parse(parseMe);
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void EmptyGroupWithSymbol()
        {
            string parseMe = "() hello";
            IEnumerable<NewCommand.FormatToken> result = NewCommand.FormatGrammar.Format.End().Parse(parseMe);
        }
    }
}
