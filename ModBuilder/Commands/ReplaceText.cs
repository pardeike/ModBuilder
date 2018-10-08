using System.Collections.Generic;
using System.IO;

namespace ModBuilder.Commands
{
	public class ReplaceText
	{
		public static string Run(Dictionary<string, string> config)
		{
			var file = config.GetFile("file");
			var search = config.GetConfig("search");
			var replace = config.GetConfig("replace");

			var oldText = File.ReadAllText(file);
			var newText = oldText.Replace(search, replace);
			if (oldText != newText)
				File.WriteAllText(file, newText);
			return null;
		}
	}
}