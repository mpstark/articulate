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
    class Command : CommandChunk
    {
        public SrgsItem Item { get; private set; }

		public Command(string semantic, string[] alternates, IEnumerable<INPUT[]> keyList)
        {
            GenerateRuleList(semantic, alternates);
            GenerateKeyLookup(semantic, keyList);
        }

		public Command(string semantic, string[] alternates, IEnumerable<INPUT[]> keyList, SrgsRuleRef subjectRef)
        {
            GenerateRuleList(semantic, alternates, subjectRef);
            GenerateKeyLookup(semantic, keyList);
        }

		public Command(string semantic, string[] alternates, IEnumerable<INPUT[]> keyList, SrgsRuleRef subjectRef, DirectObject directObject)
        {
            GenerateRuleList(semantic, alternates, subjectRef, directObject);
            GenerateKeyLookup(semantic, keyList, directObject);
        }

        private void GenerateRuleList(string semantic, string[] alternates, SrgsRuleRef subjectRef = null, DirectObject directObject = null)
        {
            SrgsOneOf commandAlternates = new SrgsOneOf(alternates);
            SrgsItem command = new SrgsItem();

            if (subjectRef != null)
            {
                command.Add(new SrgsItem(subjectRef));
                command.Add(new SrgsSemanticInterpretationTag("out.subject=rules.subject;"));
            }

            command.Add(commandAlternates);
            command.Add(new SrgsSemanticInterpretationTag("out.command=\"" + semantic + "\";"));

            if (directObject != null)
            {
                command.Add(directObject.RuleRef);
                command.Add(new SrgsSemanticInterpretationTag("out.directObject=rules." + directObject.RuleName + ";"));
            }

            Item = command;

            RuleList = new List<SrgsRule>();
            SrgsRule rule = new SrgsRule(semantic);
            rule.Add(command);

            RuleList.Add(rule);
            RootRule = rule;
        }

        private void GenerateKeyLookup(string semantic, IEnumerable<INPUT[]> keyList, DirectObject directObject = null)
        {
            var keys = new List<INPUT[]>();
            foreach (var key in keyList)
            {
                keys.Add(key);
            }

            KeyLookup = new Dictionary<string, List<INPUT[]>>();
            KeyLookup.Add(semantic, keys);

            if (directObject != null)
            {
				foreach (var entry in directObject.KeyLookup)
                {
                    KeyLookup.Add(entry.Key, entry.Value);
                }
            }
        }
    }
}
