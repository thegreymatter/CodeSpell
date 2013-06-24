using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CodeSpell.Tests
{
	[TestFixture]
	public class SpellCheckEngineTests
	{
		[Test]
		public void AnalyzeCsFile_4ErrorsInStringLiterals_4ErrorsReports()
		{
			using (var engine = new SpellCheckEngine())
			{
				using (var reportWriter = new XmlErrorWriter("report.xml"))
				{
					var results = engine.AnalyzeFilesForMispellings(new[] {@"testdata\codeFile.cs"},reportWriter);
					Assert.AreEqual(4, results);
				}
				
			}
		}

		[Test]
		public void AnalyzeAspxFile_4ErrorsInHtmlBody_4ErrorsReports()
		{
			using (var engine = new SpellCheckEngine())
			{
				using (var reportWriter = new XmlErrorWriter("report.xml"))
				{
					var results = engine.AnalyzeFilesForMispellings(new[] {@"testdata\codeFile.aspx"},reportWriter);
					Assert.AreEqual(4, results);
				}
				
			}
		}

		[Test]
		public void AnalyzeJsFile_4ErrorsInStringLiterals_4ErrorsReports()
		{
			using (var engine = new SpellCheckEngine())
			{
				using (var reportWriter = new XmlErrorWriter("report.xml"))
				{
					var results = engine.AnalyzeFilesForMispellings(new[] {@"testdata\codeFile.js"},reportWriter);
					Assert.AreEqual(4, results);
				}
			}
		}


	}
}
