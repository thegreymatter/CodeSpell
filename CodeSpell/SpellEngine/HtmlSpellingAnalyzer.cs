using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeSpell.SpellEngine;

namespace CodeSpell
{
	internal sealed class HtmlSpellingAnalyzer:ISpellingAnalyzer
	{
		private SpellChecker _checker;
		#region Private data members
		//=====================================================================

		// Word break characters.  Specifically excludes: _ . ' @
		private const string wordBreakChars = ",/<>?;:\"[]\\{}|-=+~!#$%^&*() \t”“";


		// Regular expressions used to find things that look like XML elements
		private static Regex reXml = new Regex(@"<[%A-Za-z/]+?.*?>");
		#endregion

		public HtmlSpellingAnalyzer(SpellChecker checker)
		{
			_checker = checker;
		}

		/// <returns>An enumerable list of misspelling tags</returns>
		public IEnumerable<string> GetMisspellings(string text)
		{
			List<Match> xmlTags = null;
			

			// Note the location of all XML elements if needed
			xmlTags = reXml.Matches(text).OfType<Match>().ToList();

			foreach(var word in SpellingHelpers.GetWordsInText(text))
			{
				string textToParse = text.Substring(word.Start, word.Length);

				// Spell check the word if it looks like one and is not ignored
				if(SpellingHelpers.IsProbablyARealWord(textToParse) && ( xmlTags.Count == 0 ||
				                                        !xmlTags.Any(match => word.Start >= match.Index &&
				                                                              word.Start <= match.Index + match.Length - 1)) &&
				   !_checker.CheckWordSpelling(textToParse))
				{
					// Sometimes it flags a word as misspelled if it ends with "'s".  Try checking the word
					// without the "'s".  If ignored or correct without it, don't flag it.  This appears to
					// be caused by the definitions in the dictionary rather than Hunspell.
					if(textToParse.EndsWith("'s", StringComparison.OrdinalIgnoreCase))
					{
						textToParse = textToParse.Substring(0, textToParse.Length - 2);

						if( _checker.CheckWordSpelling(textToParse))
							continue;

						textToParse += "'s";
					}

					TextSpan errorSpan = new TextSpan(word.Start,word.Start+ word.Length);

					yield return textToParse;
				}
			}
		}
        

	}
}