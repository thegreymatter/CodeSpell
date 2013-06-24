using System.Collections.Generic;

namespace CodeSpell.SpellEngine
{
	public interface ISpellingAnalyzer
	{
		IEnumerable<string> GetMisspellings(string text);
	}
}