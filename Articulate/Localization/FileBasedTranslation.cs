using SierraLib.Translation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articulate
{
	public sealed class FileBasedTranslation : ITranslation
	{
		public FileBasedTranslation(CultureInfo culture, Stream file)
		{
			Culture = culture;
			ProcessFile(file);
		}

		public System.Globalization.CultureInfo Culture
		{
			get;
			private set;
		}

		public string Author
		{
			get;
			private set;
		}

		private Dictionary<string, string> FileData = new Dictionary<string, string>();

		public string this[string key]
		{
			get { return FileData.ContainsKey(key) ? FileData[key] : "!!!" + key; }
		}

		public string this[string key, string defaultValue]
		{
			get { return FileData.ContainsKey(key) ? FileData[key] : defaultValue; }
		}

		#region File Processing

		enum LexerToken
		{
			Ignore,
			BeginKey,
			Key,
			BeginValue,
			Value,
			Invalid
		}

		class Token
		{
			public Token()
			{
				Type = LexerToken.BeginKey;
				Value = "";
			}

			public LexerToken Type;
			public string Value;
			public Token Next;
		}
				
		LexerToken NextToken(string content, ref int index, LexerToken currentToken)
		{
			switch (currentToken)
			{
				case LexerToken.BeginKey:
					if (content[index] == '\r' || content[index] == '\n') return LexerToken.Ignore;
					if (char.IsLetter(content, index) || content[index] == '_') return LexerToken.Key;
					return LexerToken.Invalid;
				case LexerToken.BeginValue:
					if (char.IsWhiteSpace(content, index) || content[index] == '=') return LexerToken.Ignore;
					return LexerToken.Value;
				case LexerToken.Key:
					if (char.IsLetterOrDigit(content, index) || content[index] == '_') return LexerToken.Key;
					if (char.IsWhiteSpace(content, index) || content[index] == '=') return LexerToken.BeginValue;
					return LexerToken.Invalid;
				case LexerToken.Value:
					if(content[index] == '\r') return LexerToken.Ignore;
					if (content[index] == '\n')
					{
						if (content[index + 1] == '\t') return LexerToken.Ignore;
						return LexerToken.BeginKey;
					}
					if (content[index] == '\t' && content[index - 1] == '\n') return LexerToken.Ignore;
					return LexerToken.Value;
				default: return LexerToken.Invalid;
			}
		}

		void ProcessFile(Stream file)
		{
			var sr = new StreamReader(file);
			var data = sr.ReadToEnd();
			
			var RootToken = new Token();
			var ActiveToken = RootToken;

			var ActiveStringBuilder = new StringBuilder();

			for (var i = 0; i < data.Length; i++)
			{
				var nextTokenType = NextToken(data, ref i, ActiveToken.Type);
				
				switch (nextTokenType)
				{
					case LexerToken.Ignore:
						continue;
					case LexerToken.BeginKey:
					case LexerToken.BeginValue:
						ActiveToken.Value = ActiveStringBuilder.ToString();
						ActiveStringBuilder.Clear();
						ActiveToken = ActiveToken.Next = new Token()
						{
							Type = nextTokenType
						};
						break;
					case LexerToken.Key:
						if (ActiveToken.Type == LexerToken.BeginKey)
						{
							ActiveToken.Type = LexerToken.Key;
							ActiveStringBuilder.Append(data[i]);
						}
						else if (ActiveToken.Type == LexerToken.Key)
							ActiveStringBuilder.Append(data[i]);
						else throw new InvalidOperationException("Invalid state transitions in translation file lexer, a key cannot appear after a " + ActiveToken.Type.ToString());
						break;
					case LexerToken.Value:
						if (ActiveToken.Type == LexerToken.BeginValue)
						{
							ActiveToken.Type = LexerToken.Value;
							ActiveStringBuilder.Append(data[i]);
						}
						else if (ActiveToken.Type == LexerToken.Value)
							ActiveStringBuilder.Append(data[i]);
						else throw new InvalidOperationException("Invalid state transitions in translation file lexer, a value cannot appear after a " + ActiveToken.Type.ToString());
						break;
					case LexerToken.Invalid:
						throw new FileFormatException("Bad file format near '" + data.Substring(Math.Max(0, i - 10), Math.Min(20, data.Length - 20)) + "'");
				}
			}

			ActiveToken.Value = ActiveStringBuilder.ToString();

			ActiveToken = RootToken;
			while (ActiveToken != null)
			{
				if (ActiveToken.Type != LexerToken.Key)
					throw new FileFormatException("Expected a Key field but found a " + ActiveToken.Type.ToString() + " token instead. File may be missing a value for a key");

				var key = ActiveToken.Value;
				ActiveToken = ActiveToken.Next;

				if (ActiveToken == null)
					throw new FileFormatException("Expected a Value field but found nothing, file may be incomplete or corrupted.");
				if (ActiveToken.Type != LexerToken.Value)
					throw new FileFormatException("Expected a Value field but found a " + ActiveToken.Type.ToString() + " token instead. File may be missing a value for a key");

				var value = ActiveToken.Value.Replace("\\r", "\r").Replace("\\n", "\n");
				ActiveToken = ActiveToken.Next;

				switch(key)
				{
					case "translation_author":
						Author = value;
						break;
				}

				FileData.Add(key, value);

				Trace.WriteLine("!!!" + key + " -> '" + value + "'");
			}

		}

		#endregion
	}
}
