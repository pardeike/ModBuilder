using System.Collections.Generic;
using System.Reflection;

namespace ModBuilder.Commands
{
	public class AssemblyVersion
	{
		public static string Run(Dictionary<string, string> config)
		{
			var file = config.GetFile("file");

			var version = AssemblyName.GetAssemblyName(file).Version;
			return version.ToString();
		}
	}
}