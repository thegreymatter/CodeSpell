using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodeSpell.SpellEngine;

namespace CodeSpell
{
	public static class Program
	{
		private static Dictionary<string, ISpellingAnalyzer> SpellingAnalyzerDictionary =
			new Dictionary<string, ISpellingAnalyzer>();

		private static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("scan list was not entered " + Environment.NewLine +
								  "CodeSpell.exe <ReportOutputPath> [File Search pattern] [File Search pattern] ..." +
								  Environment.NewLine +
								  "Example - " + Environment.NewLine +
								  "CodeSpell.exe \"\" \"C:\\dev\\*\\Views\\*.aspx\" \"C:\\dev\\**\\*.ascx\"");
				return;
			}

			var fileset = new HashSet<string>();
			var reportName = args[0];
			using (XmlErrorWriter writer = new XmlErrorWriter(reportName))
			{
				foreach (var arg in args.Skip(1))
				{
					fileset.UnionWith(FileMatcher.GetFiles("", arg));

				}
				SpellCheckEngine engine = new SpellCheckEngine();

				var misspellingCount = engine.AnalyzeFilesForMispellings(fileset, writer);

				Console.WriteLine("##teamcity[buildStatisticValue key='misspellingCount' value='" + misspellingCount + "']");
			}
		}


	}
}


