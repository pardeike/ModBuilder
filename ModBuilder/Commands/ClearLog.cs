using System.Collections.Generic;
using System.IO;

namespace ModBuilder.Commands
{
	public class ClearLog
	{
		public static string Run(Dictionary<string, string> config)
		{
			var path = "".LocalSubDir() + Path.DirectorySeparatorChar + "Log.txt";
			File.WriteAllText(path, "");
			return null;
		}
	}
}