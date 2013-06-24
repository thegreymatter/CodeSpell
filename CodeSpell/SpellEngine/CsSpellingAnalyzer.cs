using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeSpell.SpellEngine;

namespace CodeSpell
{
	public class CsSpellingAnalyzer : ISpellingAnalyzer
	{
		public CsSpellingAnalyzer(SpellChecker checker)
		{
			_checker = checker;
		}


		private readonly SpellChecker _checker;

		public IEnumerable<string> GetMisspellings(string text)
		{
			var reqstring = new Regex("((\\\".+?\\\")|('.+?'))");

			var literals = reqstring.Matches(text).OfType<Match>().ToList();
			foreach (var literal in literals)
			{
				//probably stringly type 
				if (literal.Value.Length <= 10) continue;
				//probably jquery
				if (literal.Value.Count(x => x == '.') >= 2 || literal.Value.Count(x => x == '#') >= 1 || literal.Value.StartsWith("\".") || literal.Value.Count(x => x == ' ') ==0) continue;
				var list = SpellingHelpers.GetWordsInText(literal.Value).ToList();
				if (list.Count <= 1) continue;
				foreach (var match in list)
				{
					var word = literal.Value.Substring(match.Start, match.Length);
					if (SpellingHelpers.IsProbablyARealWord(word) && !_checker.CheckWordSpelling(word))
					{
						yield return word;
					}
				}
			}
		}




	}
}