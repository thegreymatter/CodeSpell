using System;
using System.Collections;
using System.IO;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeSpell
{
    internal static class FileMatcher
    {
		internal delegate bool DirectoryExists(string path);

        private readonly static string directorySeparator;

        private readonly static string altDirectorySeparator;

        private readonly static char[] wildcardCharacters;

        private readonly static char[] wildcardAndSemicolonCharacters;

        internal readonly static char[] directorySeparatorCharacters;

        private readonly static FileMatcher.GetFileSystemEntries defaultGetFileSystemEntries;

        private readonly static DirectoryExists defaultDirectoryExists;

        private readonly static char[] invalidPathChars;

        private const string recursiveDirectoryMatch = "**";

        private const string dotdot = "..";

        static FileMatcher()
        {
            FileMatcher.directorySeparator = new string(Path.DirectorySeparatorChar, 1);
            FileMatcher.altDirectorySeparator = new string(Path.AltDirectorySeparatorChar, 1);
            char[] chrArray = new char[] { '*', '?' };
            FileMatcher.wildcardCharacters = chrArray;
            FileMatcher.wildcardAndSemicolonCharacters = new char[] { '*', '?', ';' };
            char[] directorySeparatorChar = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            FileMatcher.directorySeparatorCharacters = directorySeparatorChar;
            FileMatcher.defaultGetFileSystemEntries = new FileMatcher.GetFileSystemEntries(FileMatcher.GetAccessibleFileSystemEntries);
            FileMatcher.defaultDirectoryExists = new DirectoryExists(Directory.Exists);
            FileMatcher.invalidPathChars = Path.GetInvalidPathChars();
        }

        internal static FileMatcher.Result FileMatch(string filespec, string fileToMatch)
        {
            Regex regex;
            FileMatcher.Result result = new FileMatcher.Result();
            fileToMatch = FileMatcher.GetLongPathName(fileToMatch, FileMatcher.defaultGetFileSystemEntries);
            FileMatcher.GetFileSpecInfo(filespec, out regex, out result.isFileSpecRecursive, out result.isLegalFileSpec, FileMatcher.defaultGetFileSystemEntries);
            if (result.isLegalFileSpec)
            {
                Match match = regex.Match(fileToMatch);
                result.isMatch = match.Success;
                if (result.isMatch)
                {
                    result.fixedDirectoryPart = match.Groups["FIXEDDIR"].Value;
                    result.wildcardDirectoryPart = match.Groups["WILDCARDDIR"].Value;
                    result.filenamePart = match.Groups["FILENAME"].Value;
                }
            }
            return result;
        }

        private static string[] GetAccessibleDirectories(string path, string pattern)
        {
            string[] strArrays;
            string str;
            string str1;
            try
            {
                string[] directories = null;
                if (pattern != null)
                {
                    str = (path.Length == 0 ? ".\\" : path);
                    directories = Directory.GetDirectories(str, pattern);
                }
                else
                {
                    str1 = (path.Length == 0 ? ".\\" : path);
                    directories = Directory.GetDirectories(str1);
                }
                if (!path.StartsWith(".\\", StringComparison.Ordinal))
                {
                    FileMatcher.RemoveInitialDotSlash(directories);
                }
                strArrays = directories;
            }
            catch (SecurityException securityException)
            {
                strArrays = new string[0];
            }
            catch (UnauthorizedAccessException unauthorizedAccessException)
            {
                strArrays = new string[0];
            }
            return strArrays;
        }

        private static string[] GetAccessibleFiles(string path, string filespec, string projectDirectory, bool stripProjectDirectory)
        {
            string[] strArrays;
            string str;
            string[] strArrays1;
            try
            {
                str = (path.Length == 0 ? ".\\" : path);
                string str1 = str;
                strArrays1 = (filespec == null ? Directory.GetFiles(str1) : Directory.GetFiles(str1, filespec));
                string[] strArrays2 = strArrays1;
                if (stripProjectDirectory)
                {
                    FileMatcher.RemoveProjectDirectory(strArrays2, projectDirectory);
                }
                else if (!path.StartsWith(".\\", StringComparison.Ordinal))
                {
                    FileMatcher.RemoveInitialDotSlash(strArrays2);
                }
                strArrays = strArrays2;
            }
            catch (SecurityException securityException)
            {
                strArrays = new string[0];
            }
            catch (UnauthorizedAccessException unauthorizedAccessException)
            {
                strArrays = new string[0];
            }
            return strArrays;
        }

        private static string[] GetAccessibleFilesAndDirectories(string path, string pattern)
        {
            string[] fileSystemEntries = null;
            if (Directory.Exists(path))
            {
                try
                {
                    fileSystemEntries = Directory.GetFileSystemEntries(path, pattern);
                }
                catch (UnauthorizedAccessException unauthorizedAccessException)
                {
                }
                catch (SecurityException securityException)
                {
                }
            }
            if (fileSystemEntries == null)
            {
                fileSystemEntries = new string[0];
            }
            return fileSystemEntries;
        }

        private static string[] GetAccessibleFileSystemEntries(FileMatcher.FileSystemEntity entityType, string path, string pattern, string projectDirectory, bool stripProjectDirectory)
        {
            string[] accessibleFiles = null;
            switch (entityType)
            {
                case FileMatcher.FileSystemEntity.Files:
                {
                    accessibleFiles = FileMatcher.GetAccessibleFiles(path, pattern, projectDirectory, stripProjectDirectory);
                    break;
                }
                case FileMatcher.FileSystemEntity.Directories:
                {
                    accessibleFiles = FileMatcher.GetAccessibleDirectories(path, pattern);
                    break;
                }
                case FileMatcher.FileSystemEntity.FilesAndDirectories:
                {
                    accessibleFiles = FileMatcher.GetAccessibleFilesAndDirectories(path, pattern);
                    break;
                }
                default:
                {

                    break;
                }
            }
            return accessibleFiles;
        }

        internal static string[] GetFiles(string projectDirectoryUnescaped, string filespecUnescaped)
        {
            return FileMatcher.GetFiles(projectDirectoryUnescaped, filespecUnescaped, FileMatcher.defaultGetFileSystemEntries, FileMatcher.defaultDirectoryExists);
        }

        internal static string[] GetFiles(string projectDirectoryUnescaped, string filespecUnescaped, FileMatcher.GetFileSystemEntries getFileSystemEntries, DirectoryExists directoryExists)
        {
            string str;
            string str1;
            string str2;
            string str3;
            bool flag;
            bool flag1;
            string[] strArrays;
            bool flag2;
            string extension;
            bool flag3;
            string str4;
            int num;
            Regex regex;
            if (!FileMatcher.HasWildcards(filespecUnescaped))
            {
                string[] strArrays1 = new string[] { filespecUnescaped };
                return strArrays1;
            }
            ArrayList arrayLists = new ArrayList();
            IList lists = arrayLists;
            FileMatcher.GetFileSpecInfo(filespecUnescaped, out str, out str1, out str2, out str3, out flag, out flag1, getFileSystemEntries);
            if (!flag1)
            {
                string[] strArrays2 = new string[] { filespecUnescaped };
                return strArrays2;
            }
            bool flag4 = false;
            if (projectDirectoryUnescaped != null)
            {
                if (str == null)
                {
                    str = projectDirectoryUnescaped;
                    flag4 = true;
                }
                else
                {
                    string str5 = str;
                    try
                    {
                        str = Path.Combine(projectDirectoryUnescaped, str);
                    }
                    catch (ArgumentException argumentException)
                    {
                        strArrays = new string[0];
                        return strArrays;
                    }
                    flag4 = !string.Equals(str, str5, StringComparison.OrdinalIgnoreCase);
                }
            }
            if (str.Length > 0 && !directoryExists(str))
            {
                return new string[0];
            }
            flag2 = (str1.Length <= 0 ? false : str1 != string.Concat("**", FileMatcher.directorySeparator));
            bool flag5 = flag2;
            if (flag5)
            {
                extension = null;
            }
            else
            {
                extension = Path.GetExtension(str2);
            }
            string str6 = extension;
            if (str6 == null || str6.IndexOf('*') != -1)
            {
                flag3 = false;
            }
            else if (str6.EndsWith("?", StringComparison.Ordinal))
            {
                flag3 = true;
            }
            else
            {
                flag3 = (str6.Length != 4 ? false : str2.IndexOf('*') != -1);
            }
            bool flag6 = flag3;
            try
            {
                IList lists1 = lists;
                string str7 = str;
                string str8 = str1;
                if (flag5)
                {
                    str4 = null;
                }
                else
                {
                    str4 = str2;
                }
                num = (flag6 ? str6.Length : 0);
                if (flag5)
                {
                    regex = new Regex(str3, RegexOptions.IgnoreCase);
                }
                else
                {
                    regex = null;
                }
                FileMatcher.GetFilesRecursive(lists1, str7, str8, str4, num, regex, flag, projectDirectoryUnescaped, flag4, getFileSystemEntries);
                return (string[])arrayLists.ToArray(typeof(string));
            }
            catch (Exception exception)
            {

                string[] strArrays3 = new string[] { filespecUnescaped };
                strArrays = strArrays3;
            }
            return strArrays;
        }

        internal static void GetFileSpecInfo(string filespec, out Regex regexFileMatch, out bool needsRecursion, out bool isLegalFileSpec, FileMatcher.GetFileSystemEntries getFileSystemEntries)
        {
            string str;
            string str1;
            string str2;
            string str3;
            FileMatcher.GetFileSpecInfo(filespec, out str, out str1, out str2, out str3, out needsRecursion, out isLegalFileSpec, getFileSystemEntries);
            if (!isLegalFileSpec)
            {
                regexFileMatch = null;
                return;
            }
            regexFileMatch = new Regex(str3, RegexOptions.IgnoreCase);
        }

        private static void GetFileSpecInfo(string filespec, out string fixedDirectoryPart, out string wildcardDirectoryPart, out string filenamePart, out string matchFileExpression, out bool needsRecursion, out bool isLegalFileSpec, FileMatcher.GetFileSystemEntries getFileSystemEntries)
        {
            isLegalFileSpec = true;
            needsRecursion = false;
            fixedDirectoryPart = string.Empty;
            wildcardDirectoryPart = string.Empty;
            filenamePart = string.Empty;
            matchFileExpression = null;
            if (-1 != filespec.IndexOfAny(FileMatcher.invalidPathChars))
            {
                isLegalFileSpec = false;
                return;
            }
            if (-1 != filespec.IndexOf("...", StringComparison.Ordinal))
            {
                isLegalFileSpec = false;
                return;
            }
            int num = filespec.LastIndexOf(":", StringComparison.Ordinal);
            if (-1 != num && 1 != num)
            {
                isLegalFileSpec = false;
                return;
            }
            FileMatcher.SplitFileSpec(filespec, out fixedDirectoryPart, out wildcardDirectoryPart, out filenamePart, getFileSystemEntries);
            matchFileExpression = FileMatcher.RegularExpressionFromFileSpec(fixedDirectoryPart, wildcardDirectoryPart, filenamePart, out isLegalFileSpec);
            if (!isLegalFileSpec)
            {
                return;
            }
            needsRecursion = wildcardDirectoryPart.Length != 0;
        }

        private static void GetFilesRecursive(IList listOfFiles, string baseDirectory, string remainingWildcardDirectory, string filespec, int extensionLengthToEnforce, Regex regexFileMatch, bool needsRecursion, string projectDirectory, bool stripProjectDirectory, FileMatcher.GetFileSystemEntries getFileSystemEntries)
        {
            bool flag;
            bool flag1;
            flag = (filespec == null ? true : regexFileMatch == null);

            flag1 = (filespec != null ? true : regexFileMatch != null);

            bool flag2 = false;
            if (remainingWildcardDirectory.Length == 0)
            {
                flag2 = true;
            }
            else if (remainingWildcardDirectory.IndexOf("**", StringComparison.Ordinal) == 0)
            {
                flag2 = true;
            }
            if (flag2)
            {
                string[] strArrays = getFileSystemEntries(0, baseDirectory, filespec, projectDirectory, stripProjectDirectory);
                for (int i = 0; i < (int)strArrays.Length; i++)
                {
                    string str = strArrays[i];
                    if ((filespec != null || regexFileMatch.IsMatch(str)) && (filespec == null || extensionLengthToEnforce == 0 || Path.GetExtension(str).Length == extensionLengthToEnforce))
                    {
                        listOfFiles.Add(str);
                    }
                }
            }
            if (needsRecursion && remainingWildcardDirectory.Length > 0)
            {
                string str1 = null;
                if (remainingWildcardDirectory != "**")
                {
                    int num = remainingWildcardDirectory.IndexOfAny(FileMatcher.directorySeparatorCharacters);

                    str1 = remainingWildcardDirectory.Substring(0, num);
                    remainingWildcardDirectory = remainingWildcardDirectory.Substring(num + 1);
                    if (str1 == "**")
                    {
                        str1 = null;
                        remainingWildcardDirectory = "**";
                    }
                }
				string[] strArrays1 = getFileSystemEntries(FileSystemEntity.Directories, baseDirectory, str1, null, false);
                for (int j = 0; j < (int)strArrays1.Length; j++)
                {
                    string str2 = strArrays1[j];
                    FileMatcher.GetFilesRecursive(listOfFiles, str2, remainingWildcardDirectory, filespec, extensionLengthToEnforce, regexFileMatch, true, projectDirectory, stripProjectDirectory, getFileSystemEntries);
                }
            }
        }

        internal static string GetLongPathName(string path)
        {
            return FileMatcher.GetLongPathName(path, FileMatcher.defaultGetFileSystemEntries);
        }

        internal static string GetLongPathName(string path, FileMatcher.GetFileSystemEntries getFileSystemEntries)
        {
            string empty;
            if (path.IndexOf("~", StringComparison.Ordinal) == -1)
            {
                return path;
            }
         
            string[] strArrays = path.Split(FileMatcher.directorySeparatorCharacters);
            int num = 0;
            if (path.StartsWith(string.Concat(FileMatcher.directorySeparator, FileMatcher.directorySeparator), StringComparison.Ordinal))
            {
                empty = string.Concat(FileMatcher.directorySeparator, FileMatcher.directorySeparator);
                empty = string.Concat(empty, strArrays[2]);
                empty = string.Concat(empty, FileMatcher.directorySeparator);
                empty = string.Concat(empty, strArrays[3]);
                empty = string.Concat(empty, FileMatcher.directorySeparator);
                num = 4;
            }
            else if (path.Length <= 2 || path[1] != ':')
            {
                empty = string.Empty;
                num = 0;
            }
            else
            {
                empty = string.Concat(strArrays[0], FileMatcher.directorySeparator);
                num = 1;
            }
            string[] fileName = new string[(int)strArrays.Length - num];
            string str = empty;
            for (int i = num; i < (int)strArrays.Length; i++)
            {
                if (strArrays[i].Length == 0)
                {
                    fileName[i - num] = string.Empty;
                }
                else if (strArrays[i].IndexOf("~", StringComparison.Ordinal) != -1)
                {
                    string[] strArrays1 = getFileSystemEntries(FileSystemEntity.FilesAndDirectories, str, strArrays[i], null, false);
                    if ((int)strArrays1.Length != 0)
                    {
                      
                        str = strArrays1[0];
                        fileName[i - num] = Path.GetFileName(str);
                    }
                    else
                    {
                        for (int j = i; j < (int)strArrays.Length; j++)
                        {
                            fileName[j - num] = strArrays[j];
                        }
                        break;
                    }
                }
                else
                {
                    fileName[i - num] = strArrays[i];
                    str = Path.Combine(str, strArrays[i]);
                }
            }
            return string.Concat(empty, string.Join(FileMatcher.directorySeparator, fileName));
        }

        internal static bool HasWildcards(string filespec)
        {
            return -1 != filespec.IndexOfAny(FileMatcher.wildcardCharacters);
        }

        internal static bool HasWildcardsSemicolonItemOrPropertyReferences(string filespec)
        {
            if (-1 != filespec.IndexOfAny(FileMatcher.wildcardAndSemicolonCharacters) || filespec.Contains("$("))
            {
                return true;
            }
            return filespec.Contains("@(");
        }

        internal static bool IsDirectorySeparator(char c)
        {
            if (c == Path.DirectorySeparatorChar)
            {
                return true;
            }
            return c == Path.AltDirectorySeparatorChar;
        }

        private static void PreprocessFileSpecForSplitting(string filespec, out string fixedDirectoryPart, out string wildcardDirectoryPart, out string filenamePart)
        {
            int num = filespec.LastIndexOfAny(FileMatcher.directorySeparatorCharacters);
            if (-1 == num)
            {
                fixedDirectoryPart = string.Empty;
                wildcardDirectoryPart = string.Empty;
                filenamePart = filespec;
                return;
            }
            int num1 = filespec.IndexOfAny(FileMatcher.wildcardCharacters);
            if (-1 == num1 || num1 > num)
            {
                fixedDirectoryPart = filespec.Substring(0, num + 1);
                wildcardDirectoryPart = string.Empty;
                filenamePart = filespec.Substring(num + 1);
                return;
            }
            string str = filespec.Substring(0, num1);
            int num2 = str.LastIndexOfAny(FileMatcher.directorySeparatorCharacters);
            if (-1 == num2)
            {
                fixedDirectoryPart = string.Empty;
                wildcardDirectoryPart = filespec.Substring(0, num + 1);
                filenamePart = filespec.Substring(num + 1);
                return;
            }
            fixedDirectoryPart = filespec.Substring(0, num2 + 1);
            wildcardDirectoryPart = filespec.Substring(num2 + 1, num - num2);
            filenamePart = filespec.Substring(num + 1);
        }

        private static string RegularExpressionFromFileSpec(string fixedDirectoryPart, string wildcardDirectoryPart, string filenamePart, out bool isLegalFileSpec)
        {
            int length;
            isLegalFileSpec = true;
            if (fixedDirectoryPart.IndexOf("<:", StringComparison.Ordinal) != -1 || fixedDirectoryPart.IndexOf(":>", StringComparison.Ordinal) != -1 || wildcardDirectoryPart.IndexOf("<:", StringComparison.Ordinal) != -1 || wildcardDirectoryPart.IndexOf(":>", StringComparison.Ordinal) != -1 || filenamePart.IndexOf("<:", StringComparison.Ordinal) != -1 || filenamePart.IndexOf(":>", StringComparison.Ordinal) != -1)
            {
                isLegalFileSpec = false;
                return string.Empty;
            }
            if (wildcardDirectoryPart.Contains(".."))
            {
                isLegalFileSpec = false;
                return string.Empty;
            }
            if (filenamePart.EndsWith(".", StringComparison.Ordinal))
            {
                filenamePart = filenamePart.Replace("*", "<:anythingbutdot:>");
                filenamePart = filenamePart.Replace("?", "<:anysinglecharacterbutdot:>");
                filenamePart = filenamePart.Substring(0, filenamePart.Length - 1);
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<:bol:>");
            stringBuilder.Append("<:fixeddir:>").Append(fixedDirectoryPart).Append("<:endfixeddir:>");
            stringBuilder.Append("<:wildcarddir:>").Append(wildcardDirectoryPart).Append("<:endwildcarddir:>");
            stringBuilder.Append("<:filename:>").Append(filenamePart).Append("<:endfilename:>");
            stringBuilder.Append("<:eol:>");
            stringBuilder.Replace(FileMatcher.directorySeparator, "<:dirseparator:>");
            stringBuilder.Replace(FileMatcher.altDirectorySeparator, "<:dirseparator:>");
            stringBuilder.Replace("<:fixeddir:><:dirseparator:><:dirseparator:>", "<:fixeddir:><:uncslashslash:>");
            do
            {
                length = stringBuilder.Length;
                stringBuilder.Replace("<:dirseparator:>.<:dirseparator:>", "<:dirseparator:>");
                stringBuilder.Replace("<:dirseparator:><:dirseparator:>", "<:dirseparator:>");
                stringBuilder.Replace("<:fixeddir:>.<:dirseparator:>.<:dirseparator:>", "<:fixeddir:>.<:dirseparator:>");
                stringBuilder.Replace("<:dirseparator:>.<:endfilename:>", "<:endfilename:>");
                stringBuilder.Replace("<:filename:>.<:endfilename:>", "<:filename:><:endfilename:>");
                
            }
            while (stringBuilder.Length < length);
            do
            {
                length = stringBuilder.Length;
                stringBuilder.Replace("**<:dirseparator:>**", "**");
              
            }
            while (stringBuilder.Length < length);
            do
            {
                length = stringBuilder.Length;
                stringBuilder.Replace("<:dirseparator:>**<:dirseparator:>", "<:middledirs:>");
                stringBuilder.Replace("<:wildcarddir:>**<:dirseparator:>", "<:wildcarddir:><:leftdirs:>");

            }
            while (stringBuilder.Length < length);
            if (stringBuilder.Length > stringBuilder.Replace("**", null).Length)
            {
                isLegalFileSpec = false;
                return string.Empty;
            }
            stringBuilder.Replace("*.*", "<:anynonseparator:>");
            stringBuilder.Replace("*", "<:anynonseparator:>");
            stringBuilder.Replace("?", "<:singlecharacter:>");
            stringBuilder.Replace("\\", "\\\\");
            stringBuilder.Replace("$", "\\$");
            stringBuilder.Replace("(", "\\(");
            stringBuilder.Replace(")", "\\)");
            stringBuilder.Replace("*", "\\*");
            stringBuilder.Replace("+", "\\+");
            stringBuilder.Replace(".", "\\.");
            stringBuilder.Replace("[", "\\[");
            stringBuilder.Replace("?", "\\?");
            stringBuilder.Replace("^", "\\^");
            stringBuilder.Replace("{", "\\{");
            stringBuilder.Replace("|", "\\|");
            stringBuilder.Replace("<:middledirs:>", "((/)|(\\\\)|(/.*/)|(/.*\\\\)|(\\\\.*\\\\)|(\\\\.*/))");
            stringBuilder.Replace("<:leftdirs:>", "((.*/)|(.*\\\\)|())");
            stringBuilder.Replace("<:rightdirs:>", ".*");
            stringBuilder.Replace("<:anything:>", ".*");
            stringBuilder.Replace("<:anythingbutdot:>", "[^\\.]*");
            stringBuilder.Replace("<:anysinglecharacterbutdot:>", "[^\\.].");
            stringBuilder.Replace("<:anynonseparator:>", "[^/\\\\]*");
            stringBuilder.Replace("<:singlecharacter:>", ".");
            stringBuilder.Replace("<:dirseparator:>", "[/\\\\]+");
            stringBuilder.Replace("<:uncslashslash:>", "\\\\\\\\");
            stringBuilder.Replace("<:bol:>", "^");
            stringBuilder.Replace("<:eol:>", "$");
            stringBuilder.Replace("<:fixeddir:>", "(?<FIXEDDIR>");
            stringBuilder.Replace("<:endfixeddir:>", ")");
            stringBuilder.Replace("<:wildcarddir:>", "(?<WILDCARDDIR>");
            stringBuilder.Replace("<:endwildcarddir:>", ")");
            stringBuilder.Replace("<:filename:>", "(?<FILENAME>");
            stringBuilder.Replace("<:endfilename:>", ")");
            return stringBuilder.ToString();
        }

        private static void RemoveInitialDotSlash(string[] paths)
        {
            for (int i = 0; i < (int)paths.Length; i++)
            {
                if (paths[i].StartsWith(".\\", StringComparison.Ordinal))
                {
                    paths[i] = paths[i].Substring(2);
                }
            }
        }

        internal static void RemoveProjectDirectory(string[] paths, string projectDirectory)
        {
            bool flag = FileMatcher.IsDirectorySeparator(projectDirectory[projectDirectory.Length - 1]);
            for (int i = 0; i < (int)paths.Length; i++)
            {
                if (paths[i].StartsWith(projectDirectory, StringComparison.Ordinal))
                {
                    if (flag)
                    {
                        paths[i] = paths[i].Substring(projectDirectory.Length);
                    }
                    else if (paths[i].Length > projectDirectory.Length && FileMatcher.IsDirectorySeparator(paths[i][projectDirectory.Length]))
                    {
                        paths[i] = paths[i].Substring(projectDirectory.Length + 1);
                    }
                }
            }
        }

        internal static void SplitFileSpec(string filespec, out string fixedDirectoryPart, out string wildcardDirectoryPart, out string filenamePart, FileMatcher.GetFileSystemEntries getFileSystemEntries)
        {
            FileMatcher.PreprocessFileSpecForSplitting(filespec, out fixedDirectoryPart, out wildcardDirectoryPart, out filenamePart);
            if ("**" == filenamePart)
            {
                wildcardDirectoryPart = string.Concat(wildcardDirectoryPart, "**");
                wildcardDirectoryPart = string.Concat(wildcardDirectoryPart, FileMatcher.directorySeparator);
                filenamePart = "*.*";
            }
            fixedDirectoryPart = FileMatcher.GetLongPathName(fixedDirectoryPart, getFileSystemEntries);
        }

        internal enum FileSystemEntity
        {
            Files,
            Directories,
            FilesAndDirectories
        }

        internal delegate string[] GetFileSystemEntries(FileMatcher.FileSystemEntity entityType, string path, string pattern, string projectDirectory, bool stripProjectDirectory);

        internal sealed class Result
        {
            internal bool isLegalFileSpec;

            internal bool isMatch;

            internal bool isFileSpecRecursive;

            internal string fixedDirectoryPart;

            internal string wildcardDirectoryPart;

            internal string filenamePart;

            internal Result()
            {
            }
        }
    }
}
