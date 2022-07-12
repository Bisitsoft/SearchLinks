using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SearchLinks
{
	internal class Program
	{
		static void Main(string[] args)
		{
			if(args.Length == 0)
			{
				PrintSeeHelpAndExit(null);
			}
			else if (args.Length > 1)
			{
				PrintSeeHelpAndExit("Too much arguments.");
			}

			if (IgnoreCaseMatch(GetHelpSwitchNames(), args[0]))
			{
				PrintHelp();
				Environment.Exit(0);
			}

			DirectoryInfo directoryInfo = new DirectoryInfo(args[0]);
			if (!directoryInfo.Exists)
			{
				PrintSeeHelpAndExit("Your inputted directory doesn't exist.");
			}

			List<string> results = new List<string>();
			List<Tuple<string, Exception>> errors = new List<Tuple<string, Exception>>();

			if (IsSymbolicLinkDirectory(directoryInfo.FullName)){
				results.Add(directoryInfo.FullName);
				Console.WriteLine(directoryInfo.FullName);
			}
			else
			{
				Stack<Stack<string>> pathStack = new Stack<Stack<string>>();
				Stack<string> pathHistory = new Stack<string>();
				pathStack.Push(new Stack<string>());
				string[] subDirectoryNames = GetSubDirectories(directoryInfo);
				PushAll<string>(pathStack.Peek(), subDirectoryNames);
				pathHistory.Push(directoryInfo.FullName);

				while (pathStack.Count > 0)
				{
					while (pathStack.Peek().Count > 0)
					{
						string nowPath = System.IO.Path.Combine(pathHistory.Peek(), pathStack.Peek().Pop());
						DirectoryInfo nowDirectoryInfo = new DirectoryInfo(nowPath);
						
						Console.WriteLine($"Scaning \"{nowDirectoryInfo.FullName}\".");

						if (IsSymbolicLinkDirectory(nowDirectoryInfo.FullName))
						{
							results.Add(nowDirectoryInfo.FullName);
							Console.WriteLine(nowDirectoryInfo.FullName);
						}
						else
						{
							try
							{
								subDirectoryNames = GetSubDirectories(nowDirectoryInfo);
							}
							catch (Exception ex)
							{
								subDirectoryNames = new string[0];
								Console.WriteLine(ex.ToString());
								errors.Add(new Tuple<string, Exception>(nowDirectoryInfo.FullName, ex));
							}
							
							if (subDirectoryNames.Length > 0)
							{
								pathStack.Push(new Stack<string>());
								PushAll<string>(pathStack.Peek(), subDirectoryNames);
								pathHistory.Push(nowDirectoryInfo.FullName);
							}
						}
					}
					pathStack.Pop();
					pathHistory.Pop();
				}
			}

			Console.WriteLine("Errors:");
			foreach(Tuple<string, Exception> error in errors)
			{
				Console.WriteLine($"\"{error.Item1}\": {error.Item2}");
			}
			Console.WriteLine("Results:");
			foreach (string result in results)
			{
				Console.WriteLine(result);
			}

			string outputPath = OutputResults(results);
			Console.WriteLine($"All results has been written to \"{outputPath}\".");
			Environment.Exit(0);
		}

		private static string[] GetSubDirectories(DirectoryInfo directoryInfo)
		{
			return (from val in directoryInfo.GetDirectories()
					select val.Name).ToArray();
		}

		private static void PushAll<T>(Stack<T> stack, IEnumerable<T> values)
		{
			foreach(T value in values)
			{
				stack.Push(value);
			}
		}

		private static bool IgnoreCaseMatch(IEnumerable<string> values, string target)
		{
			int i;

			foreach(string value in values)
			{
				if (value.Length == target.Length)
				{
					for (i = 0; i < target.Length; i++)
					{
						if (Char.ToLower(value[i]) != Char.ToLower(target[i]))
						{
							break;
						}
					}
					if (i >= target.Length)
					{
						return true;
					}
				}
			}

			return false;
		}

		public static string OutputResults(IEnumerable<string> results)
		{
			FileStream fileStream = new FileStream($"results.{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough);
			StreamWriter streamWriter = new StreamWriter(fileStream);
			foreach(string result in results)
			{
				streamWriter.WriteLine(result + Environment.NewLine);
			}
			streamWriter.Close();
			fileStream.Close();

			return fileStream.Name;
		}

		public static string[] GetHelpSwitchNames()
		{
			List<string> helpSwitchNames = new List<string>(6) { "-h", "--help" };
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				helpSwitchNames.AddRange(new string[] { "/?", "/h", "/help" });
			}
			return helpSwitchNames.ToArray();
		}
		public static void PrintSeeHelpAndExit(string errorMessage, int exitCode = -1)
		{
			if (errorMessage != null)
			{
				Console.WriteLine(errorMessage);
			}
			PrintSeeHelp();
			Environment.Exit(exitCode);
		}
		public static void PrintSeeHelp()
		{
			Console.WriteLine("Use \"SearchLinks --help\" to see help.");
		}
		public static void PrintHelp()
		{
			Console.WriteLine("");
			Console.WriteLine("Use \"SearchLinks --help\" to see help.");
		}

		public static bool IsSymbolicLink(string path)
		{
			return IsSymbolicLinkDirectory(path) | IsSymbolicLinkFile(path);
		}
		public static bool IsSymbolicLinkFile(string path)
		{
			FileInfo fileInfo = new FileInfo(path);
			if (fileInfo.Exists)
			{
				return fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
			}
			else
			{
				return false;
			}

		}
		public static bool IsSymbolicLinkDirectory(string path)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			if (directoryInfo.Exists)
			{
				return directoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
			}
			else
			{
				return false;
			}
		}
	}
}
