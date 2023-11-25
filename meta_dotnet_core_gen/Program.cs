// ******************************************************************************************************************************
// Filename:    Program.cs
// Description: Scrapes Microsoft's website for new version of DotNet and AspNet and then generates bitbake recipes for them
//              using meta-dotnet-core.
// ******************************************************************************************************************************
// Copyright © Richard Dunkley 2023
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ******************************************************************************************************************************
using HtmlAgilityPack;
using meta_dotnet_core_gen.Auto;
using System.Net;

namespace meta_dotnet_core_gen
{
	internal class Program
	{
		public static int ClickDelayInMilliseconds = 1000;

		/// <summary>
		///   Enumerates the various versions currently supported by meta-dotnet-core.
		/// </summary>
		/// <remarks>This app will generate bitbake recipes and .inc files for new versions, but it assumes a generic major version .inc file (Ex: dotnet-core_5.x.x.inc) has already been created.</remarks>
		public static int[] SupportedVersions = new int[] { 6, 7, 8 };

		static void Main(string[] args)
		{
			Settings settings = new Settings();
			ConsoleArgs<Settings>.Populate(Environment.CommandLine, settings);

			if(settings.Help)
			{
				Console.Write(ConsoleArgs<Settings>.GenerateHelpText(Console.BufferWidth));
				return;
			}

			string message = settings.Validate();
			if(message != null)
			{
				Console.WriteLine($"ERROR: {message}");
				return;
			}

			string tmpFolder = Path.Combine(Environment.CurrentDirectory, "dotnet-tmp");
			if (!Directory.Exists(tmpFolder))
				Directory.CreateDirectory(tmpFolder);

			Dotnet dotnet = null;
			if(File.Exists(settings.Config))
				dotnet = new Dotnet(settings.Config);

			if (dotnet == null)
			{
				Console.WriteLine($"No previous configuration file was found in the working directory containing hashes of the online files.");
				Console.Write($"Do you want to regenerate this file (may take a very long time)? (Y/N):");
			}
			else
				Console.Write($"Do you want to update the configuration file with any updated versions online (may take a very long time)? (Y/N):");
			ConsoleKeyInfo key = Console.ReadKey();
			Console.WriteLine();

			// Exit if no configuration is found.
			if (key.KeyChar != 'y' && key.KeyChar != 'Y' && dotnet == null)
				return;

			if (key.KeyChar == 'y' || key.KeyChar == 'Y')
			{
				var badLinks = UpdateOnlineVersions(ref dotnet);
				if(badLinks.Length > 0)
				{
					Console.WriteLine("The following runtime download links were found which filename could not be parsed:");
					foreach(string link in badLinks)
						Console.WriteLine($"	{link}");
				}

				// Save the new configuration file.
				dotnet.ExportToXML(settings.Config);
			}
			Console.WriteLine();

			// List new versions found.
			var newVers = dotnet.GetNewVersionList();
			if(newVers.Length > 0)
			{
				Console.WriteLine($"The following new versions were found online:");
				foreach (string ver in newVers)
					Console.WriteLine(ver);
				Console.WriteLine();
			}

			// List changed links.
			var changedLinks = dotnet.GetChangedLinks();
			if (changedLinks.Length > 0)
			{
				Console.WriteLine($"The following links have changed:");
				foreach (string ver in changedLinks)
					Console.WriteLine(ver);
				Console.WriteLine();
			}

			// List changed checksums.
			var changedChecksums = dotnet.GetChangedChecksum();
			if (changedChecksums.Length > 0)
			{
				Console.WriteLine($"The following SHA-512 Hashes have changed:");
				foreach (string ver in changedChecksums)
					Console.WriteLine(ver);
				Console.WriteLine();
			}

			var filesNeedingHashes = dotnet.GetFilesNeedingHashes();
			if(filesNeedingHashes.Length > 0)
			{
				Console.WriteLine($"The following files need to have their hashes (re-)generated:");
				foreach(string ver in filesNeedingHashes)
					Console.WriteLine(ver);
				Console.WriteLine();

				// Prompt to update the new hashes.
				 Console.Write("Do you want to update hashes for these files (Y/N):");
				key = Console.ReadKey();
				Console.WriteLine();
				if (key.KeyChar == 'y' || key.KeyChar == 'Y')
				{
					dotnet.UpdateHashes(tmpFolder);

					// Save the new configuration file.
					dotnet.ExportToXML(settings.Config);
				}
			}

			// Parse the meta layer files.
			Dotnet meta = ParseRuntimeMetaLayer(settings.MetaFolder);

			// Find any meta that doesn't exist online.
			var metaNotOnline = Dotnet.FindMetaThatIsntOnline(dotnet, meta);
			if(metaNotOnline.Length > 0)
			{
				Console.WriteLine($"The following were found in the meta files, but not online:");
				foreach (string ver in metaNotOnline)
					Console.WriteLine(ver);
				Console.WriteLine();
			}

			// Find any Hash or link differences for matching.
			var metaDiffs = Dotnet.FindDifferencesInMeta(dotnet, meta);
			if (metaDiffs.Length > 0)
			{
				Console.WriteLine($"The following meta were found that has a different link or SHA-512 hash then the online version:");
				foreach (string ver in metaDiffs)
					Console.WriteLine(ver);
				Console.WriteLine();
			}

			// Find any in meta that aren't online.
			var onlineNotMeta = Dotnet.FindOnlineThatIsntMeta(dotnet, meta);
			if (onlineNotMeta.Length > 0)
			{
				Console.WriteLine($"The following were found online, but not in the meta files:");
				foreach (string ver in onlineNotMeta)
					Console.WriteLine(ver);
				Console.WriteLine();
			}

			string copyrightHolder = null;
			if (metaDiffs.Length > 0 || onlineNotMeta.Length > 0)
			{
				// Prompt to generate new meta.
				Console.Write("Do you want to fix/create meta files for the above changes (Y/N):");
				key = Console.ReadKey();
				Console.WriteLine();
				if (key.KeyChar == 'y' || key.KeyChar == 'Y')
				{
					Console.Write("Enter the copyright name to put in the new files: ");
					copyrightHolder = Console.ReadLine();
					Dotnet.UpdateMeta(dotnet, meta, settings.MetaFolder, copyrightHolder);
				}
			}
			else
			{
				Console.WriteLine("No additional meta files were generated/updated.");
			}

			var dbgVer = DebuggerUtils.GetCurrentOnlineDebuggerVersion(tmpFolder, out string scriptSha256, out string scriptMd5);
			if(dbgVer != null)
			{
				var lookup = DebuggerUtils.ParseDebuggerMetaLayer(settings.MetaFolder);
				if(!lookup.ContainsKey(dbgVer.Major))
				{
					Console.WriteLine($"Found online debugger version {dbgVer}. This is a new Major version without a current recipe. A version {dbgVer.Major} base recipe file (vsdbg_{dbgVer.Major}.x.inc) will need to be created first and the dependencies checked.");
				}
				else
				{
					if (!lookup[dbgVer.Major].Contains(dbgVer.ToString(4)))
					{
						Console.WriteLine($"Found an online debugger version ({dbgVer}) that does not have a recipe.");
						Console.Write($"Would you like to generate a new recipe for it (Y/N):");
						key = Console.ReadKey();
						Console.WriteLine();
						if(key.KeyChar == 'y' || key.KeyChar == 'Y')
						{
							if(copyrightHolder == null)
							{
								Console.Write("Enter the copyright name to put in the new files: ");
								copyrightHolder = Console.ReadLine();
							}

							// Create the meta files.
							DebuggerUtils.CreateRecipeFiles(settings.MetaFolder, tmpFolder, copyrightHolder, dbgVer, scriptSha256, scriptMd5, new Dotnet.Runtime.Build.Arch.TargetEnum[]
							{
								Dotnet.Runtime.Build.Arch.TargetEnum.Arm,
								Dotnet.Runtime.Build.Arch.TargetEnum.X64,
								Dotnet.Runtime.Build.Arch.TargetEnum.Arm64
							});
						}
					}
				}
			}
		}

		/// <summary>
		///   Adds new entries to <see cref="Dotnet"/> object based on what is new online.
		/// </summary>
		/// <param name="cfg"><see cref="Dotnet"/> object to add the new entries to.</param>
		/// <returns>List of links found on the website that appear to be a new download link, but can't be parsed to obtain the necessary information.</returns>
		private static string[] UpdateOnlineVersions(ref Dotnet cfg)
		{
			if (cfg == null)
				cfg = new Dotnet();

			var badList = new List<string>();
			HtmlWeb web = new HtmlWeb();
			foreach (int majVer in SupportedVersions)
			{
				Thread.Sleep(ClickDelayInMilliseconds);
				HtmlDocument doc = web.Load($"https://dotnet.microsoft.com/en-us/download/dotnet/{majVer}.0");
				foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
				{
					var linkTarget = Dotnet.Runtime.Build.Arch.GetLinkTarget(link.InnerText);
					if (linkTarget.HasValue)
					{
						var dpage = "https://dotnet.microsoft.com" + link.Attributes["href"].Value;
						if (dpage.Contains("linux") && dpage.Contains("runtime"))
						{
							Thread.Sleep(ClickDelayInMilliseconds);
							HtmlDocument downloadLink = web.Load(dpage);
							HtmlNode checkNode = downloadLink.GetElementbyId("checksum");
							HtmlNode linkNode = downloadLink.GetElementbyId("directLink");
							if (checkNode != null && linkNode != null)
							{
								string url = linkNode.Attributes["href"].Value;
								if (!TryUpdateLink(cfg, url, checkNode.Attributes["Value"].Value))
								{
									if (!url.Contains("preview") && !url.Contains("rc"))
										badList.Add(linkNode.Attributes["href"].Value);
								}
							}
						}
					}
				}
			}
			return badList.ToArray();
		}

		/// <summary>
		///   Parses a folder on the local system that is a meta-dotnet-core folder containing bitbake recipes.
		/// </summary>
		/// <param name="folder">Location of the meta layer.</param>
		/// <returns><see cref="Dotnet"/> object containing the parsed information.</returns>
		private static Dotnet ParseRuntimeMetaLayer(string folder)
		{
			var cfg = new Dotnet();

			// Parse the runtimes.
			string runtimeFolder = Path.Combine(folder, "recipes-runtime");
			foreach(Dotnet.Runtime.NameEnum typeName in Enum.GetValues(typeof(Dotnet.Runtime.NameEnum)))
			{
				string coreFolder = Path.Combine(runtimeFolder, $"{Enum.GetName(typeof(Dotnet.Runtime.NameEnum), typeName).ToLower()}-core");
				string[] files = Directory.GetFiles(coreFolder);
				foreach (string file in files)
				{
					Dotnet.Runtime run = null;
					if (Path.GetExtension(file) == ".bb")
					{
						string fileBeginning = $"{Enum.GetName(typeName).ToLower()}-core_";
						var verString = Path.GetFileNameWithoutExtension(file).Replace(fileBeginning, string.Empty);
						if(Version.TryParse(verString, out Version version))
						{
							run = cfg.FindRuntime(typeName, version.Major);
							if (run == null)
							{
								run = new Dotnet.Runtime(typeName, new Version(version.Major, version.Minor));
								cfg.AddRuntime(run);
							}

							// Check for inc file.
							if (!File.Exists(Path.Combine(coreFolder, $"{fileBeginning}{version.ToString(3)}.inc")))
								Console.WriteLine($"WARNING: The '.inc' file for {typeName} version {version} was not found.");

							// Check for none file.
							if (!File.Exists(Path.Combine(coreFolder, $"{fileBeginning}{version.ToString(3)}_none.inc")))
								Console.WriteLine($"WARNING: The '_none.inc' file for {typeName} version {version} was not found.");

							Dotnet.Runtime.Build build = run.FindBuild(version.Build);
							if (build == null)
							{
								build = new Dotnet.Runtime.Build(version.Build);
								run.AddBuild(build);
							}

							foreach (Dotnet.Runtime.Build.Arch.TargetEnum target in Enum.GetValues(typeof(Dotnet.Runtime.Build.Arch.TargetEnum)))
							{
								string filePath = Path.Combine(coreFolder, $"{fileBeginning}{version}_{Enum.GetName(target).ToLower()}.inc");
								if (File.Exists(filePath))
									build.AddArch(Dotnet.Runtime.Build.Arch.LoadRecipe(target, filePath));
							}
						}
					}
				}
			}
			return cfg;
		}

		/// <summary>
		///   Attempts to update an entry in <paramref name="cfg"/> that pertains to <paramref name="link"/> and <paramref name="checksum"/>.
		/// </summary>
		/// <param name="cfg"><see cref="Dotnet"/> object to be updated.</param>
		/// <param name="link">Link of a download file to add to the configuration.</param>
		/// <param name="checksum">SHA-512 hash of the download file pointed to by <paramref name="link"/>.</param>
		/// <returns>True on success, false if the link couldn't be parsed correctly.</returns>
		private static bool TryUpdateLink(Dotnet cfg, string link, string checksum)
		{
			if (!TryParseFileLink(link, out Dotnet.Runtime.NameEnum runtime, out Version version, out Dotnet.Runtime.Build.Arch.TargetEnum arch))
				return false;

			var run = cfg.FindRuntime(runtime, version.Major);
			if (run == null)
			{
				run = new Dotnet.Runtime(runtime, new Version(version.Major, version.Minor));
				cfg.AddRuntime(run);
			}

			var build = run.FindBuild(version.Build);
			if(build == null)
			{
				build = new Dotnet.Runtime.Build(version.Build);
				run.AddBuild(build);
			}

			var targetArch = build.FindArch(arch);
			if(targetArch == null)
			{
				targetArch = new Dotnet.Runtime.Build.Arch(link, null, null, checksum, arch);
				targetArch.New = true;
				build.AddArch(targetArch);
			}
			else
			{
				if (string.Compare(targetArch.Link, link, true) != 0)
					targetArch.NewLink = link;
				if (string.Compare(targetArch.Sha512, checksum, true) != 0)
					targetArch.NewSha512 = checksum;
			}
			Console.WriteLine($"Found {runtime} version {version.ToString(3)} ({arch})");
			return true;
		}

		public static bool TryParseFileLink(string link, out Dotnet.Runtime.NameEnum runtimeName, out Version version, out Dotnet.Runtime.Build.Arch.TargetEnum targetArch)
		{
			var uri = new Uri(link);
			var fileName = uri.Segments.Last().TrimEnd('/');

			runtimeName = Dotnet.Runtime.NameEnum.DotNet;
			version = null;
			targetArch = Dotnet.Runtime.Build.Arch.TargetEnum.Arm;

			if (!fileName.EndsWith(".tar.gz"))
				return false;

			fileName = fileName.Substring(0, fileName.Length - 7);

			var splits = fileName.Split('-', StringSplitOptions.RemoveEmptyEntries);
			if (splits.Length != 5)
				return false;

			if (splits[0].EndsWith("core"))
				splits[0] = splits[0].Substring(0, splits[0].Length - 4);

			if (!Enum.TryParse(splits[0], true, out runtimeName))
				return false;
			if (splits[1] != "runtime")
				return false;
			if (!Version.TryParse(splits[2], out version))
				return false;
			if (splits[3] != "linux")
				return false;
			return Enum.TryParse(splits[4], true, out targetArch);
		}
	}
}