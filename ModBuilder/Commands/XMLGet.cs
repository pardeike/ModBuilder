using System.Collections.Generic;
using System.Xml;

namespace ModBuilder.Commands
{
	public class XMLGet
	{
		public static string Run(Dictionary<string, string> config)
		{
			var file = config.GetFile("file");
			var xpath = config.GetConfig("xpath");

			var xml = new XmlDocument();
			xml.Load(file);
			var node = xml.SelectSingleNode(xpath);
			if (node == null)
				throw new KeyNotFoundException($"File {file} does not contain an element at {xpath}");
			return node.InnerText;
		}
	}
}