using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CodeSpell.SpellEngine;
using NHunspell;

namespace CodeSpell
{
	public sealed class SpellChecker : IDisposable
	{
		private const string CustomDictionaryPath = "dic/custom.dic";
		private const int MaxLengthForSplit = 30;
		private readonly List<string> myCustomDictionary = new List<string>();
		private readonly Hunspell myHunspell;
		private readonly string myPluginPath;
		private readonly string myUserDicFilename;
		private readonly HashSet<string> myUserDictionary = new HashSet<string>();
		private volatile object myHunspellSync = new object();

		public SpellChecker()
		{
			

			myPluginPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof (SpellChecker)).Location) +
			               Path.DirectorySeparatorChar;
			Hunspell.NativeDllPath = myPluginPath;
			myHunspell = new Hunspell(myPluginPath + "dic/en_us.aff", myPluginPath + "dic/en_us.dic");

			try
			{
				myCustomDictionary.AddRange(File.ReadAllLines(myPluginPath + CustomDictionaryPath));
				myCustomDictionary.ForEach(s => myHunspell.Add(s));
			}
			catch
			{
			}
			string userDicDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			                                       "ResharperSpellChecker");
			if (!Directory.Exists(userDicDirectory))
			{
				Directory.CreateDirectory(userDicDirectory);
			}
			myUserDicFilename = Path.Combine(userDicDirectory, "user.dic");
			try
			{
				string[] readAllLines = File.ReadAllLines(myUserDicFilename);
				foreach (var line in readAllLines)
				{
					myUserDictionary.Add(line);
					myHunspell.Add(line);
				}
			}
			catch
			{
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			myHunspell.Dispose();
		}

		#endregion

		public bool CheckWordSpelling(string word)
		{
			lock (myHunspellSync)
			{
				// try to check in many styles
				bool wholeWordCheck = myHunspell.Spell(word.ToLower()) 
				                      || myHunspell.Spell(word.ToUpper())
				                      || myHunspell.Spell(NamingStyleConverter.Convert(word, "Aaa"));
				if (wholeWordCheck)
					return true;
				//try to split into pair of words
				if (word.Length < 4 || word.Length > MaxLengthForSplit) //min size of each word == 2, e.g. inplace -> in+place
					//also very long words will be ignored
					return false;
				for (int i = 2; i < word.Length - 1; ++i)
				{
					string first = word.Substring(0, i);
					string second = word.Substring(i);
					if (myHunspell.Spell(first) && myHunspell.Spell(second))
						return true;
				}
				return false;
			}
		}

		public List<string> Suggestions(string word)
		{
			lock (myHunspellSync)
			{
				List<string> result = myHunspell.Suggest(word)
				                                .Where(s => s.ToLower() != word.ToLower()) // filter suggests not equals original word
				                                .Select(s => NamingStyleConverter.Convert(s, word)).ToList(); // convert style
				return result.Any() ? result.ToList() : new List<string> {word};
			}
		}

		public void AddWordToUserDict(string word)
		{
			lock (myHunspellSync)
			{
				string downcased = word.ToLower();
				myUserDictionary.Add(downcased);
				myHunspell.Add(downcased);
				SaveUserDictionary();
			}
		}

		internal void AddWordToCustomDict(string word)
		{
			lock (myHunspellSync)
			{
				string downcased = word.ToLower();
				string[] content = File.ReadAllLines(CustomDictionaryPath);
				HashSet<string> hashSet = new HashSet<string>(content);
				hashSet.Add(downcased);
				File.WriteAllLines(CustomDictionaryPath, hashSet.ToArray());
				myHunspell.Add(downcased);
			}
		}

		private void SaveUserDictionary()
		{
			File.WriteAllLines(myUserDicFilename, myUserDictionary.ToArray());
		}
	}
}