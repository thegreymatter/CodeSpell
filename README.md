CodeSpell
=========


A command line utility that scans for spelling mistakes in UI code and returns a checkstyle formatted xml report.

How to use?

CodeSpell.exe [reportname] [file pattern to scan] [file pattern to scan] 

Example:

CodeSpell.exe Report.xml My.Web.UI\**\Views\**\*.aspx My.Web.UI\**\Renderers\**\*.cs  My.Web.UI\static\js\**\*.js
