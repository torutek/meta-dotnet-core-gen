// ******************************************************************************************************************************
// Filename:    DebuggerUtils.cs
// Description: Various utility methods to generate vsdebugger recipes.
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
using meta_dotnet_core_gen.Auto;
using System.Reflection;

namespace meta_dotnet_core_gen
{
	/// <summary>
	///   Various methods to aide in vsdebugger recipe creation.
	/// </summary>
	internal static class DebuggerUtils
	{
		/// <summary>
		///   Base URL to download the debuggers.
		/// </summary>
		public static string BaseUrl = "https://vsdebugger-cyg0dxb6czfafzaz.b01.azurefd.net/";

		/// <summary>
		///   Downloads the GetVsDbg.sh script and parses it to see the current online debugger version.
		/// </summary>
		/// <param name="tmpFolder">Temporary folder to download the script to.</param>
		/// <param name="scriptSha256">Contains the SHA256 hash of the downloaded script file.</param>
		/// <param name="scriptMd5">Contains the MD5 hash of the downloaded script file.</param>
		/// <returns>Version found or null if no version was found.</returns>
		public static Version GetCurrentOnlineDebuggerVersion(string tmpFolder, out string scriptSha256, out string scriptMd5)
		{
			var downloadedFile = Path.Combine(tmpFolder, "GetVsDbg.sh");
			Console.Write($"Downloading GetVsDbg.sh...");
			RecipeUtils.DownloadFile("https://aka.ms/getvsdbgsh", downloadedFile);

			// Compute the hashes.
			Console.Write("Computing Hashes...");
			RecipeUtils.ComputeHashes(downloadedFile, true, true, out scriptSha256, out scriptMd5);

			var scriptContents = File.ReadAllLines(downloadedFile);
			Version ver = null;
			string baseUrl = null;
			foreach (string line in scriptContents)
			{
				var tempLine = line.Trim();
				if (tempLine.StartsWith("__VsDbgVersion=", StringComparison.OrdinalIgnoreCase))
				{
					if (ver == null && Version.TryParse(tempLine.Substring(15), out Version tempVer))
						ver = tempVer;
				}
				else if (tempLine.StartsWith("url=\"https://"))
				{
					var splits = tempLine.Substring(13).Split('/', StringSplitOptions.RemoveEmptyEntries);
					if (splits.Length > 0)
						baseUrl = $"https://{splits[0]}/";
				}
			}
			Console.WriteLine("done!");

			if(string.Compare(baseUrl, BaseUrl, StringComparison.OrdinalIgnoreCase) != 0)
			{
				Console.WriteLine($"WARNING: the download URL for the debugger has changed! This may mean all debugger downloads have changed.");
				BaseUrl = baseUrl;
			}

			return ver;
		}

		/// <summary>
		///   Parses the debugger meta-layer and builds up a lookup table of Major version and a list of versions.
		/// </summary>
		/// <param name="metaFolder">Meta-layer root folder.</param>
		/// <returns>Lookup table containing each version that has a recipe in a lookup table based on it's major version.</returns>
		public static Dictionary<int, List<string>> ParseDebuggerMetaLayer(string metaFolder)
		{
			var list = new Dictionary<int, List<string>>();

			// Parse the debugger recipes.
			string dbgFolder = Path.Combine(metaFolder, "recipes-devtools", "vsdbg");
			string[] files = Directory.GetFiles(dbgFolder);
			foreach (string file in files)
			{
				if (Path.GetExtension(file) == ".bb")
				{
					string fileBeginning = $"vsdbg_";
					var verString = Path.GetFileNameWithoutExtension(file).Replace(fileBeginning, string.Empty);
					if (Version.TryParse(verString, out Version version))
					{
						if (!list.ContainsKey(version.Major))
							list.Add(version.Major, new List<string>());
						list[version.Major].Add(version.ToString(4));
					}
				}
				else if (Path.GetExtension(file) == ".inc")
				{
					// Just to make sure we have the major version inc files too.
					string fileBeginning = $"vsdbg_";
					var verString = Path.GetFileNameWithoutExtension(file).Replace(fileBeginning, string.Empty);
					var splits = verString.Split('.', StringSplitOptions.RemoveEmptyEntries);
					if (splits.Length == 2 && int.TryParse(splits[0], out int majorVer))
					{
						if (!list.ContainsKey(majorVer))
							list.Add(majorVer, new List<string>());
					}
				}
			}
			return list;
		}

		/// <summary>
		///   Creates the debugger's Architecture specific inc file.
		/// </summary>
		/// <param name="metaFolder">Root folder of the meta-layer to write the files to.</param>
		/// <param name="tmpFolder">Temporary folder for downloading the file (for hashing).</param>
		/// <param name="copyrightHolder">Copyright holder of the generated code.</param>
		/// <param name="ver">Version to be generated.</param>
		/// <param name="target">Target to generate.</param>
		private static void CreateArchFile(string metaFolder, string tmpFolder, string copyrightHolder, Version ver, Dotnet.Runtime.Build.Arch.TargetEnum target)
		{
			string targetName = target.ToString().ToLower();
			string name = $"vsdbg_{ver.ToString(4)}_{targetName}";

			// Download the file.
			var downloadedFile = Path.Combine(tmpFolder, $"{name}.zip");
			var fileName = $"vsdbg-{ver.Major}-{ver.Minor}-{ver.Build}-{ver.Revision}/vsdbg-linux-{targetName}.zip";
			var url = $"{BaseUrl}{fileName}";
			Console.Write($"Downloading {fileName}...");
			RecipeUtils.DownloadFile(url, downloadedFile);

			// Compute the hashes.
			Console.Write("Computing Hashes...");
			RecipeUtils.ComputeHashes(downloadedFile, true, true, out string sha256Hash, out string md5Hash);

			// Create the recipe file.
			using (StreamWriter sw = new StreamWriter(Path.Combine(metaFolder, "recipes-devtools", "vsdbg", $"{name}.inc"), false))
			{
				sw.WriteLine($"###################################################################################################");
				sw.WriteLine($"# Contains the URL and checksums to download version {ver.ToString(4)} of the {targetName.ToUpper()} Visual Studio");
				sw.WriteLine($"# debugger from Microsoft.");
				sw.WriteLine($"# Copyright {copyrightHolder} {DateTime.Now.Year}");
				sw.WriteLine($"# Auto-generated using {Assembly.GetExecutingAssembly().GetName().Name}");
				sw.WriteLine($"###################################################################################################");
				sw.WriteLine($"SRC_URI += \"{url};subdir=vsdbg-${{PV}};name=source\"");
				sw.WriteLine();
				sw.WriteLine($"SRC_URI[source.md5sum] = \"{md5Hash.ToLower()}\"");
				sw.WriteLine($"SRC_URI[source.sha256sum] = \"{sha256Hash.ToLower()}\"");
			}
			Console.WriteLine("done!");
		}

		/// <summary>
		///   Creates all the recipe files for the specific vsdebugger version.
		/// </summary>
		/// <param name="metaFolder">Root folder of the meta-layer to write the files to.</param>
		/// <param name="tmpFolder">Temporary folder for downloading the file (for hashing).</param>
		/// <param name="copyrightHolder">Copyright holder of the generated code.</param>
		/// <param name="ver">Version to be generated.</param>
		/// <param name="scriptSha256">SHA256 of the GetVsDbg.sh file the version was pulled from.</param>
		/// <param name="scriptMd5">MD5 of the GetVsDbg.sh file the version was pulled from.</param>
		/// <param name="targets">Targets to generate recipes for.</param>
		public static void CreateRecipeFiles(string metaFolder, string tmpFolder, string copyrightHolder, Version ver, string scriptSha256, string scriptMd5, Dotnet.Runtime.Build.Arch.TargetEnum[] targets)
		{
			var dbgFolder = Path.Combine(metaFolder, "recipes-devtools", "vsdbg");
			using (var sw = new StreamWriter(Path.Combine(dbgFolder, $"vsdbg_{ver.ToString(4)}.bb"), false))
			{
				sw.WriteLine($"###################################################################################################");
				sw.WriteLine($"# Contains the recipe to download the Visual Studio debugger from Microsoft to provide remote");
				sw.WriteLine($"# debugging from Visual Studio and Visual Studio Code.");
				sw.WriteLine($"# Copyright {copyrightHolder} {DateTime.Now.Year}");
				sw.WriteLine($"# Auto-generated using {Assembly.GetExecutingAssembly().GetName().Name}");
				sw.WriteLine($"###################################################################################################");
				sw.WriteLine();
				sw.WriteLine($"require recipes-devtools/vsdbg/vsdbg_{ver.ToString(4)}.inc");
				sw.WriteLine($"require recipes-devtools/vsdbg/vsdbg_{ver.Major}.x.inc");
			}

			using(var sw = new StreamWriter(Path.Combine(dbgFolder, $"vsdbg_{ver.ToString(4)}.inc"), false))
			{
				sw.WriteLine($"###################################################################################################");
				sw.WriteLine($"# Contains additional parameters for the recipe to download the release binaries from Microsoft.");
				sw.WriteLine($"# Copyright {copyrightHolder} {DateTime.Now.Year}");
				sw.WriteLine($"# Auto-generated using {Assembly.GetExecutingAssembly().GetName().Name}");
				sw.WriteLine($"###################################################################################################");
				sw.WriteLine($"SUMMARY = \"Contains the binaries for Microsoft's Visual Studio Remote Debugger for Linux\"");
				sw.WriteLine($"HOMEPAGE = \"https://visualstudio.microsoft.com/\"");
				sw.WriteLine();
				sw.WriteLine($"SRC_URI += \"https://vsdebugger.blob.core.windows.net/vsdbg-{ver.Major}-{ver.Minor}-{ver.Build}-{ver.Revision}/GetVsDbg.sh;name=script\"");
				sw.WriteLine($"SRC_URI[script.md5sum] = \"{scriptMd5.ToLower()}\"");
				sw.WriteLine($"SRC_URI[script.sha256sum] = \"{scriptSha256.ToLower()}\"");
				sw.WriteLine();
				sw.WriteLine($"RDEPENDS:${{PN}} += \"procps openssh-sftp-server\"");
				sw.WriteLine();
				sw.WriteLine($"DOTNET_RUNTIME_ARCH = \"none\"");
				foreach(var target in targets)
					sw.WriteLine($"DOTNET_RUNTIME_ARCH:{GetRuntimeArch(target)} = \"{target.ToString().ToLower()}\"");
				sw.WriteLine();
				sw.WriteLine($"# This is here because it doesn't seem like bitbake likes ${{PV}} used in require statements.");
				sw.WriteLine($"require recipes-devtools/vsdbg/vsdbg_{ver.ToString(4)}_${{DOTNET_RUNTIME_ARCH}}.inc");
				sw.WriteLine();
				sw.WriteLine($"do_install:append() {{");
				sw.WriteLine($"	echo \"{ver.ToString(4)}\" > ${{D}}${{ROOT_HOME}}/.vs-debugger/vs2022/success.txt");
				sw.WriteLine($"}}");
			}

			// Create the none file.
			File.WriteAllText(Path.Combine(dbgFolder, $"vsdbg_{ver.ToString(4)}_none.inc"), string.Empty);

			// Create the other target files.
			foreach(var target in targets)
				CreateArchFile(metaFolder, tmpFolder, copyrightHolder, ver, target);
		}

		/// <summary>
		///   Determines the runtime architecture type used in the recipes.
		/// </summary>
		/// <param name="target">Target architecture.</param>
		/// <returns>String of the architectures runtime type.</returns>
		/// <exception cref="NotImplementedException"><paramref name="target"/> is unsupported.</exception>
		private static string GetRuntimeArch(Dotnet.Runtime.Build.Arch.TargetEnum target)
		{
			switch(target)
			{
				case Dotnet.Runtime.Build.Arch.TargetEnum.Arm:
					return "arm";
				case Dotnet.Runtime.Build.Arch.TargetEnum.Arm64:
					return "aarch64";
				case Dotnet.Runtime.Build.Arch.TargetEnum.X64:
					return "x86_64";
				default:
					throw new NotImplementedException();
			}
		}
	}
}
