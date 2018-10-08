using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModBuilder
{
	public class Program
	{
		public static bool verbose = false;
		const string usage = "Usage: ModBuilder (-v) <cmd> (-p1 val1) (-p2 val2) ...";

		static void Main(string[] args)
		{
			if (args.Length > 0 && args[0] == "-v")
			{
				args = args.Skip(1).ToArray();
				Program.verbose = true;
			}

			if (args.Length == 0 || (args.Length - 1) % 2 != 0)
			{
				Console.WriteLine(usage);
				Environment.Exit(-1);
			}

			var cmd = args[0];
			args = args.Skip(1).ToArray();
			var cmdType = Type.GetType($"ModBuilder.Commands.{cmd}");
			if (cmdType == null)
			{
				var availableCommands = string.Join(" ", typeof(Program).Assembly.GetTypes()
					.Where(t => t.GetMethod("Run") != null)
					.Select(t => t.Name).ToArray());
				$"Unknown command '{cmd}'. Available commands: {availableCommands}".Log(true);
				Environment.Exit(-1);
			}
			$"Executing {cmd}".Log();

			var argPairs = new Dictionary<string, string>();
			for (var i = 0; i < args.Length; i += 2)
			{
				var key = args[i].Substring(1);
				var val = args[i + 1];
				var stateDir = "State".LocalSubDir();
				while (true)
				{
					var i1 = val.IndexOf("{{");
					var i2 = val.IndexOf("}}");
					if (i1 >= 0 && i2 >= i1 + 2)
					{
						var name = val.Substring(i1 + 2, i2 - i1 - 2);
						var val2 = File.ReadAllText(stateDir + Path.DirectorySeparatorChar + name + ".val");
						val = val.Substring(0, i1) + val2 + val.Substring(i2 + 2);
					}
					else
						break;
				}
				argPairs[key] = val;
				$"Parmeter {key}: {val}".Log();
			}

			var result = cmdType.GetMethod("Run").Invoke(null, new[] { argPairs }) as string;
			if (result != null)
			{
				$"{cmdType.Name}: {result}".Log();
				if (argPairs.TryGetValue("save", out var save))
				{
					var stateDir = "State".LocalSubDir();
					File.WriteAllText(stateDir + Path.DirectorySeparatorChar + save + ".val", result);
				}
			}

			Environment.Exit(0);
		}
	}

	public static class Extensions
	{
		public static void Log(this string str, bool error = false)
		{
			if (Program.verbose || error)
				Console.WriteLine(str);
			var date = DateTime.Now.ToString("yyyyMMddHHmmss");
			File.AppendAllText("".LocalSubDir() + Path.DirectorySeparatorChar + "Log.txt", date + " " + str + "\r\n");
		}

		public static string GetConfig(this Dictionary<string, string> config, string key, bool optional = false)
		{
			if (config.TryGetValue(key, out var val))
				return val;
			if (!optional)
			{
				$"Required argument -{key} not found".Log(true);
				Environment.Exit(-1);
			}
			return null;
		}

		public static string GetFile(this Dictionary<string, string> config, string key, bool optional = false)
		{
			var path = config.GetConfig(key, optional);
			if (path == null)
				return null;

			if (path.StartsWith("{{") && key.EndsWith("}}"))
			{
				var name = path.Substring(2, path.Length - 4);
				var stateDir = "State".LocalSubDir();
				path = stateDir + Path.DirectorySeparatorChar + name + ".val";
			}

			if (!File.Exists(path))
			{
				$"File {path} does not exist".Log(true);
				Environment.Exit(-1);
			}
			return path;
		}

		public static string LocalSubDir(this string name)
		{
			var root = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
			var dir = new FileInfo(root).DirectoryName;
			if (name == "")
				return dir;
			var path = dir + Path.DirectorySeparatorChar + name;
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			return path;
		}
	}
}