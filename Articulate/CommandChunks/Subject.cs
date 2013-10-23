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
    class Subject : CommandChunk
    {
        public Subject()
        {
            GenerateRuleList();
            GenerateKeyLookup();
        }

        private void GenerateRuleList()
        {
            RuleList = new List<SrgsRule>();

            // SQUAD SELECTION
            SrgsItem one = GetNewNode(new string[] {"one"}, "ONE");
            SrgsItem two = GetNewNode(new string[] { "two" }, "TWO");
            SrgsItem three = GetNewNode(new string[] { "three" }, "THREE");
            SrgsItem four = GetNewNode(new string[] { "four" }, "FOUR");
            SrgsItem five = GetNewNode(new string[] { "five" }, "FIVE");
            SrgsItem six = GetNewNode(new string[] { "six" }, "SIX");
            SrgsItem seven = GetNewNode(new string[] { "seven" }, "SEVEN");
            SrgsItem eight = GetNewNode(new string[] { "eight" }, "EIGHT");
            SrgsItem nine = GetNewNode(new string[] { "nine" }, "NINE");
            SrgsItem ten = GetNewNode(new string[] { "ten" }, "TEN");

            SrgsOneOf squadNumbersChoice = new SrgsOneOf(one, two, three, four, five, six, seven, eight, nine, ten);
            SrgsItem squadNumbersConcatChoice = new SrgsItem(squadNumbersChoice, new SrgsItem(0, 1, "and"));
            squadNumbersConcatChoice.SetRepeat(1, 10);
            SrgsRule squadNumbers = new SrgsRule("squadNumbers");
            squadNumbers.Add(squadNumbersConcatChoice);
            RuleList.Add(squadNumbers);
            SrgsRule squadMembers = new SrgsRule("squadSelections");
            squadMembers.Add(new SrgsRuleRef(squadNumbers));
            squadMembers.Add(new SrgsSemanticInterpretationTag("out=rules.squadNumbers;"));
            RuleList.Add(squadMembers);

            // TEAM
            SrgsItem red = GetNewNode(new string[] { "red" }, "RED");
            SrgsItem yellow = GetNewNode(new string[] { "yellow" }, "YELLOW");
            SrgsItem white = GetNewNode(new string[] { "white" }, "WHITE");
            SrgsItem blue = GetNewNode(new string[] { "blue" }, "BLUE");
            SrgsItem green = GetNewNode(new string[] { "green" }, "GREEN");

            SrgsOneOf teamColorsChoice = new SrgsOneOf(blue, green, white, yellow, red);
            SrgsRule teamColors = new SrgsRule("teamColors");
            teamColors.Add(teamColorsChoice);
            RuleList.Add(teamColors);

            SrgsRule teams = new SrgsRule("teams");
            teams.Add(new SrgsItem("team"));
            teams.Add(new SrgsRuleRef(teamColors));
            teams.Add(new SrgsSemanticInterpretationTag("out=rules.teamColors;"));
            RuleList.Add(teams);

            // ALL
            SrgsItem allItems = GetNewNode(new string[] { "all", "everyone", "team", "squad" }, "ALL");
            SrgsRule all = new SrgsRule("all");
            all.Add(allItems);
            RuleList.Add(all);

            // ALL TOGETHER NOW
            SrgsOneOf subjectChoice = new SrgsOneOf();
            subjectChoice.Add(new SrgsItem(new SrgsRuleRef(teams)));
            subjectChoice.Add(new SrgsItem(new SrgsRuleRef(squadMembers)));
            subjectChoice.Add(new SrgsItem(new SrgsRuleRef(all)));

            SrgsRule subject = new SrgsRule("subject");
            subject.Add(subjectChoice);

            RootRule = subject;
            RuleList.Add(subject);
        }

        private void GenerateKeyLookup()
        {
            KeyLookup = new Dictionary<string, List<uint>>();

            var one = new List<uint>();
            one.Add(Keys.F1);
			var two = new List<uint>();
            two.Add(Keys.F2);
			var three = new List<uint>();
            three.Add(Keys.F3);
			var four = new List<uint>();
            four.Add(Keys.F4);
			var five = new List<uint>();
            five.Add(Keys.F5);
			var six = new List<uint>();
            six.Add(Keys.F6);
			var seven = new List<uint>();
            seven.Add(Keys.F7);
			var eight = new List<uint>();
            eight.Add(Keys.F8);
			var nine = new List<uint>();
            nine.Add(Keys.F9);
			var ten = new List<uint>();
            ten.Add(Keys.F10);

			var red = new List<uint>();
            red.Add(Keys.Nine);
            red.Add(Keys.Nine);
            red.Add(Keys.One);

			var green = new List<uint>();
            green.Add(Keys.Nine);
            green.Add(Keys.Nine);
            green.Add(Keys.Two);

			var blue = new List<uint>();
            blue.Add(Keys.Nine);
            blue.Add(Keys.Nine);
            blue.Add(Keys.Three);

			var yellow = new List<uint>();
            yellow.Add(Keys.Nine);
            yellow.Add(Keys.Nine);
            yellow.Add(Keys.Four);

			var white = new List<uint>();
            white.Add(Keys.Nine);
            white.Add(Keys.Nine);
            white.Add(Keys.Five);

			var everyone = new List<uint>();
            everyone.Add(Keys.Tilde);

            // SQUAD SELECTION
            KeyLookup.Add("ONE", one);
            KeyLookup.Add("TWO", two);
            KeyLookup.Add("THREE", three);
            KeyLookup.Add("FOUR", four);
            KeyLookup.Add("FIVE", five);
            KeyLookup.Add("SIX", six);
            KeyLookup.Add("SEVEN", seven);
            KeyLookup.Add("EIGHT", eight);
            KeyLookup.Add("NINE", nine);
            KeyLookup.Add("TEN", ten);

            // TEAM SELECTION
            KeyLookup.Add("RED", red);
            KeyLookup.Add("BLUE", blue);
            KeyLookup.Add("YELLOW", yellow);
            KeyLookup.Add("WHITE", white);
            KeyLookup.Add("GREEN", green);

            // ALL
            KeyLookup.Add("ALL", everyone);
        }
    }
}