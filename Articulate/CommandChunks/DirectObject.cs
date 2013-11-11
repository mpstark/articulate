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
    class DirectObject : CommandChunk
    {
        public SrgsRuleRef RuleRef;
        private List<directObjectEntry> entries;
        public string RuleName;

        private struct directObjectEntry
        {
            public string semantic;
            public string[] alternates;
			public IEnumerable<INPUT[]> keyList;
        }

        public DirectObject(string name)
        {
            entries = new List<directObjectEntry>();
            RuleName = name;
        }


        private void GenerateRuleList()
        {
            RuleList = new List<SrgsRule>();

            SrgsOneOf directObject = new SrgsOneOf();

            foreach(directObjectEntry entry in entries)
            {
                SrgsItem item = GetNewNode(entry.alternates, entry.semantic);
                directObject.Add(item);
            }
            
            RuleList = new List<SrgsRule>();
            SrgsRule rule = new SrgsRule(RuleName);
            rule.Add(directObject);

            RuleRef = new SrgsRuleRef(rule);

            RuleList.Add(rule);
            RootRule = rule;
        }

        private void GenerateKeyList()
        {
            KeyLookup = new Dictionary<string, List<INPUT[]>>();

            foreach (directObjectEntry entry in entries)
            {
				var list = new List<INPUT[]>();
                foreach(var key in entry.keyList)
                {
                    list.Add(key);
                }
                
                // add entry to keylookup
                KeyLookup.Add(entry.semantic, list);
            }
        }

		public void Add(string[] alternates, string semantic, IEnumerable<INPUT[]> keyList)
        {
            // create a new entry
            directObjectEntry newEntry;
            newEntry.semantic = semantic;
            newEntry.alternates = alternates;
            newEntry.keyList = keyList;

            // add the entry
            entries.Add(newEntry);

            // generate new rulelist and keylist
            GenerateRuleList();
            GenerateKeyList();
        }
    }
}
