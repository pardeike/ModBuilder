using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace ModBuilder.Commands
{
	public class Template
	{
		// xml data must have a top level node named <Root>
		public class Root
		{
			public string A { get; set; }
			public int B { get; set; }

			[XmlIgnore]
			public Dictionary<string, string> Dict { get; set; }
		}

		public static string Run(Dictionary<string, string> config)
		{
			var templateFile = config.GetFile("template");
			var jsonFile = config.GetFile("json", true);
			var xmlFile = config.GetFile("xml", true);
			var file = config.GetConfig("destination");

			if (jsonFile != null && xmlFile != null)
			{
				"Cannot specify -json and -xml at the same time".Log(true);
				Environment.Exit(-1);
			}

			if (jsonFile == null && xmlFile == null)
			{
				"Need to specify either -json or -xml".Log(true);
				Environment.Exit(-1);
			}

			var templateData = File.ReadAllText(templateFile);
			var template = Handlebars.Compile(templateData);

			object data = null;
			if (jsonFile != null)
			{
				var txt = File.ReadAllText(jsonFile);
				data = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(txt);
			}
			if (xmlFile != null)
			{
				var xs = new XmlSerializer(typeof(Root));
				xs.UnknownElement += delegate (object sender, XmlElementEventArgs e)
				{
					var root = (Root)e.ObjectBeingDeserialized;
					if (root.Dict == null)
						root.Dict = new Dictionary<string, string>();
					root.Dict.Add(e.Element.Name, e.Element.InnerText);
				};
				using (var stream = new FileStream(xmlFile, FileMode.Open))
					data = ((Root)xs.Deserialize(stream)).Dict;
			}

			File.WriteAllText(file, template(data));
			return null;
		}
	}
}