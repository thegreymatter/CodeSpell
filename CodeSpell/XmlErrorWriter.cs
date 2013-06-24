using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeSpell
{
	public class XmlErrorWriter:IDisposable
	{
		private readonly XmlWriter _writer;
		public XmlErrorWriter(string filename)
		{
			_writer = new XmlTextWriter(filename,Encoding.UTF8);
			_writer.WriteStartElement("checkstyle");
			_writer.WriteAttributeString("version","5.0");
		}

		public void AddError(string filename,string error)
		{

			_writer.WriteStartElement("file");
			_writer.WriteAttributeString("name", filename);
			_writer.WriteStartElement("error");
			_writer.WriteAttributeString("message","Potential spelling error in page - '"+error+"'");
			_writer.WriteAttributeString("severity","warning");
			_writer.WriteAttributeString("line", "0");
			_writer.WriteAttributeString("column", "0");
			_writer.WriteEndElement();
			_writer.WriteEndElement();
		}


		public void Dispose()
		{
			_writer.WriteEndElement();
			_writer.Flush();
			_writer.Close();
		}
	}
}

