using System;
using System.Collections.Generic;
using System.IO;
using CodeSpell.SpellEngine;

namespace CodeSpell
{
	public static class Program
	{
		private static Dictionary<string, ISpellingAnalyzer> SpellingAnalyzerDictionary =
			new Dictionary<string, ISpellingAnalyzer>();

		private static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine(string.Concat("scan list was not entered ",
					Environment.NewLine,
					"CodeSpell.exe <ReportOutputPath> <FilesSpecsPath>",
					Environment.NewLine,
					"Example - ",
					Environment.NewLine,
					"CodeSpell.exe \"Report.xml\" \"FilesSearchRules.txt\""));
			}
			else
			{
				var fileset = new HashSet<string>();
				var filesetToExclude = new HashSet<string>();
				var reportName = @"Report.xml";
				var fileSpecsPath = @"FileSpecs.txt";

				var fileSpecs = GetFileSpecs(fileSpecsPath);

				using (var writer = new XmlErrorWriter(reportName))
				{
					foreach (var fileSpec in fileSpecs)
					{
						if (fileSpec.StartsWith("-:"))
						{
							filesetToExclude.UnionWith(FileMatcher.GetFiles("", fileSpec.Remove(0, 2)));
						}
						else
						{
							fileset.UnionWith(FileMatcher.GetFiles("", fileSpec));
						}
					}

					fileset.ExceptWith(filesetToExclude);
					var engine = new SpellCheckEngine();

					var misspellingCount = engine.AnalyzeFilesForMispellings(fileset, writer);

					Console.WriteLine("##teamcity[buildStatisticValue key='misspellingCount' value='" + misspellingCount + "']");
				}
			}
		}

		private static string[] GetFileSpecs(string searchRulesFileName)
		{
			if (!File.Exists(searchRulesFileName))
				Console.WriteLine("The given files search rules path was not found.");

			return File.ReadAllLines(searchRulesFileName);
		}
	}
}
