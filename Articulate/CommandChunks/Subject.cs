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
            KeyLookup = new Dictionary<string, List<INPUT[]>>();

            var one = DirectInputEmulator.KeyPress(DirectInputKeys.F1);
			var two = DirectInputEmulator.KeyPress(DirectInputKeys.F2);
			var three = DirectInputEmulator.KeyPress(DirectInputKeys.F3);
			var four = DirectInputEmulator.KeyPress(DirectInputKeys.F4);
			var five = DirectInputEmulator.KeyPress(DirectInputKeys.F5);
			var six = DirectInputEmulator.KeyPress(DirectInputKeys.F6);
			var seven = DirectInputEmulator.KeyPress(DirectInputKeys.F7);
			var eight = DirectInputEmulator.KeyPress(DirectInputKeys.F8);
			var nine = DirectInputEmulator.KeyPress(DirectInputKeys.F9);
			var ten = DirectInputEmulator.KeyPress(DirectInputKeys.F10);

			var red = DirectInputEmulator.KeyPress(DirectInputKeys.Shift, DirectInputKeys.F1);
			var green = DirectInputEmulator.KeyPress(DirectInputKeys.Shift, DirectInputKeys.F2);
			var blue = DirectInputEmulator.KeyPress(DirectInputKeys.Shift, DirectInputKeys.F3);
			var yellow = DirectInputEmulator.KeyPress(DirectInputKeys.Shift, DirectInputKeys.F4);
			var white = DirectInputEmulator.KeyPress(DirectInputKeys.Shift, DirectInputKeys.F5);

			var everyone = new List<INPUT[]>();
			everyone.Add(DirectInputEmulator.KeyPress(DirectInputKeys.Tilde));

            // SQUAD SELECTION
            KeyLookup.Add("ONE", new List<INPUT[]>() { one });
			KeyLookup.Add("TWO", new List<INPUT[]>() { two });
			KeyLookup.Add("THREE", new List<INPUT[]>() { three });
			KeyLookup.Add("FOUR", new List<INPUT[]>() { four });
			KeyLookup.Add("FIVE", new List<INPUT[]>() { five });
			KeyLookup.Add("SIX", new List<INPUT[]>() { six });
			KeyLookup.Add("SEVEN", new List<INPUT[]>() { seven });
			KeyLookup.Add("EIGHT", new List<INPUT[]>() { eight });
			KeyLookup.Add("NINE", new List<INPUT[]>() { nine });
			KeyLookup.Add("TEN", new List<INPUT[]>() { ten });

            // TEAM SELECTION
            KeyLookup.Add("RED", new List<INPUT[]>() { red });
            KeyLookup.Add("BLUE", new List<INPUT[]>() { blue });
            KeyLookup.Add("YELLOW", new List<INPUT[]>() { yellow });
            KeyLookup.Add("WHITE", new List<INPUT[]>() { white });
			KeyLookup.Add("GREEN", new List<INPUT[]>() { green });

            // ALL
            KeyLookup.Add("ALL", everyone);
        }
    }
}