using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CodeSpell.SpellEngine;

namespace CodeSpell
{
	public class SpellCheckEngine : IDisposable
	{
		private readonly Dictionary<string, ISpellingAnalyzer> _spellingAnalyzerDictionary = new Dictionary<string, ISpellingAnalyzer>();
		private readonly SpellChecker _checker;

		public string Report { get; set; }


		public SpellCheckEngine()
		{
			_checker = new SpellChecker();
			var htmlSpellingProvider = new HtmlSpellingAnalyzer(_checker);
			var csSpellingProvider = new CsSpellingAnalyzer(_checker);

			_spellingAnalyzerDictionary.Add(".aspx", htmlSpellingProvider);
			_spellingAnalyzerDictionary.Add(".ascx", htmlSpellingProvider);
			_spellingAnalyzerDictionary.Add(".html", htmlSpellingProvider);
			_spellingAnalyzerDictionary.Add(".cs", csSpellingProvider);
			_spellingAnalyzerDictionary.Add(".js", csSpellingProvider);
		}

		public int AnalyzeFilesForMispellings(IEnumerable<string> fileset,XmlErrorWriter writer)
		{
			var builder = new StringBuilder();

			int misspellingCount = 0;

			foreach (var file in fileset)
			{
				var analyzer = _spellingAnalyzerDictionary.ContainsKey(Path.GetExtension(file))
								   ? _spellingAnalyzerDictionary[Path.GetExtension(file)]
								   : _spellingAnalyzerDictionary[".cs"];

				var spellingErrors = analyzer.GetMisspellings(File.ReadAllText(file));

				foreach (var spelling in spellingErrors)
				{
					misspellingCount++;
					writer.AddError(file,spelling);
				}
			}
			Report = builder.ToString();
			return misspellingCount;
		}

		public void Dispose()
		{
			_checker.Dispose();
		}
	}
}