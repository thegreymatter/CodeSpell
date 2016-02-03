using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSpell
{
	public static class SpellingHelpers
	{
		/// <summary>
		/// Get all words in the specified text string
		/// </summary>
		/// <param name="text">The text to break into words</param>
		/// <returns>An enumerable list of word spans</returns>
		internal static IEnumerable<TextSpan> GetWordsInText(string text)
		{
			if (String.IsNullOrWhiteSpace(text))
				yield break;

			for (int i = 0; i < text.Length; i++)
			{
				if (IsWordBreakCharacter(text[i]))
				{
					// Skip escape sequences.  If not, they can end up as part of the word or cause words to be
					// missed.  For example, "This\r\nis\ta\ttest \x22missing\x22" would incorrectly yield "r",
					// "nis", "ta", and "ttest" and incorrectly exclude "missing").  This can cause the
					// occasional false positive in file paths (i.e. \Folder\transform\File.txt flags "ransform"
					// as a misspelled word because of the lowercase "t" following the backslash) but I can live
					// with that.
					if (text[i] == '\\' && i + 1 < text.Length)
						switch (text[i + 1])
						{
							case '\'':
							case '\"':
							case '\\':
							case '0':
							case 'a':
							case 'b':
							case 'f':
							case 'n':
							case 'r':
							case 't':
							case 'v':
								i++;
								break;

							case 'u':
							case 'U':
							case 'x':
								i++;

								// Special handling for \x, \u, and \U.  Skip the hex digits too.
								if (i + 1 < text.Length)
								{
									do
									{
										i++;

									} while (i < text.Length && (Char.IsDigit(text[i]) ||
																 (Char.ToLower(text[i]) >= 'a' && Char.ToLower(text[i]) <= 'f')));

									i--;
								}
								break;

							default:
								break;
						}

					continue;
				}

				int end = i;

				for (; end < text.Length; end++)
					if (IsWordBreakCharacter(text[end]))
						break;

				// If it looks like an XML entity, ignore it
				if (i == 0 || end >= text.Length || text[i - 1] != '&' || text[end] != ';')
				{
					// Ignore leading apostrophes
					while (i < end && text[i] == '\'')
						i++;

					// Ignore trailing apostrophes, periods, and at-signs
					while (end > i && (text[end - 1] == '\'' || text[end - 1] == '.' || text[end - 1] == '@'))
						end--;

					// Ignore anything less than two characters
					if (end <= i)
						end++;
					else
						if (end - i > 1)
							yield return new TextSpan(i, end);
				}

				i = end - 1;
			}
		}
		// Word break characters.  Specifically excludes: _ . ' @
		private const string wordBreakChars = ",<>?;:\"[]{}|-=+~!$%^&*() \t”“’�";


		/// <summary>
		/// See if the specified character is a word break character
		/// </summary>
		/// <param name="c">The character to check</param>
		/// <returns>True if the character is a word break, false if not</returns>
		private static bool IsWordBreakCharacter(char c)
		{
			return wordBreakChars.Contains(c) || Char.IsWhiteSpace(c) ||
				   (c == '_') ||
				   ((c == '.' || c == '@'));
		}

		/// <summary>
		/// Determine if a word is probably a real word
		/// </summary>
		/// <param name="word">The word to check</param>
		/// <returns>True if it appears to be a real word or false if any of the following conditions are met:
		/// 
		/// <list type="bullet">
		///     <description>The word contains a period or an at-sign (it looks like a filename or an e-mail
		/// address) and those words are being ignored.  We may miss a few real misspellings in this case due
		/// to a missed space after a period, but that's acceptable.</description>
		///     <description>The word contains an underscore and underscores are not being treated as
		/// separators.</description>
		///     <description>The word contains a digit and words with digits are being ignored.</description>
		///     <description>The word is composed entirely of digits when words with digits are not being
		/// ignored.</description>
		///     <description>The word is in all uppercase and words in all uppercase are being ignored.</description>
		///     <description>The word is camel cased.</description>
		/// </list>
		/// </returns>
		public static bool IsProbablyARealWord(string word)
		{
			if (String.IsNullOrWhiteSpace(word))
				return false;

			word = word.Trim();

			if (word.Length >= 15)
				return false;

			if (word.Length < 3)
				return false;

			// Check for a period or an at-sign in the word (things that look like filenames, e-mail addresses or rgb colors)
			if (word.IndexOfAny(new[] { '.', '@','\\','/', '#' }) >= 0)
				return false;

			// Check for underscores and digits
			if (word.Any(c => c == '_' || (Char.IsDigit(c))))
				return false;

			// Ignore if all digits (this only happens if the Ignore Words With Digits option is false)
			if (!word.Any(c => Char.IsLetter(c)))
				return false;

			// Ignore if all uppercase, accounting for apostrophes and digits
			if (word.All(c => Char.IsUpper(c) || !Char.IsLetter(c)))
				return false;

			// Ignore if camel cased
			if (Char.IsLetter(word[0]) && word.Skip(1).Any(c => Char.IsUpper(c)))
				return false;

			return true;
		}
	}
}
