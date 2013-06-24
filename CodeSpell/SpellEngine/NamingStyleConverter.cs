using System;
using System.Linq;

namespace CodeSpell.SpellEngine
{
	public static class NamingStyleConverter
	{
		public static string Convert(string word, string pattern)
		{
			if (word.Length < 1 || pattern.Length < 1)
			{
				throw new ArgumentException("Both arguments must be non-empty strings");
			}
			if (pattern.All(Char.IsUpper))
			{
				return word.ToUpper();
			}
			if (pattern.All(Char.IsLower))
			{
				return word.ToLower();
			}
			if (Char.IsUpper(pattern[0]) && pattern.Skip(1).All(Char.IsLower))
			{
				return Char.ToUpper(word[0]) + word.Substring(1).ToLower();
			}
			return word;
		}
	}
}