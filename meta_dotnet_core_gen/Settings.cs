// ******************************************************************************************************************************
// Filename:    Dotnet.AutoGen.cs
// Description: Enumerates the various command line arguments that can be provided to the application.
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
using System.Security;

namespace meta_dotnet_core_gen
{
	[Usage("Scrapes the .Net Download websites and generates bitbake recipes to pull in the Arm, Arm64, and x64 binaries.", "[-h] [-c=<config file>] -m=<meta-dotnet-core folder>")]
	public class Settings
	{
		#region Arguments

		[Argument('c', "Specifies the configuration file to pull in the previously scraped and computed values. Defaults to core_download_cfg.xml in the working directory if no value is provided.", Required = false, Word = "config")]
		public string Config { get; set; }

		[Argument('h', "Displays this help and then exits.", Required = false, Word = "help")]
		public bool Help { get; set; }

		[Argument('m', "Path to the meta-dotnet-core folder. This app uses this folder to determine which versions have already been created. It will also populate this folder with the newly generated output files.", Required = false, Word = "metalayer")]
		public string MetaFolder { get; set; }

		[Argument('v', "Verifies the existing recipes with the online content.", Required = false, Word = "verify")]
		public bool Verify { get; set; }

		#endregion Arguments

		#region Methods

		public string Validate()
		{
			if (string.IsNullOrEmpty(MetaFolder))
				return $"No meta-dotnet-core folder was specified. Please use '-m' argument to specify the folder for this layer.";

			if(string.IsNullOrEmpty(Config))
				Config = Path.Combine(Environment.CurrentDirectory, "core_download_cfg.xml");

			try
			{
				Config = Path.GetFullPath(Config);
			}
			catch(Exception e) when (e is SecurityException || e is ArgumentException || e is NotSupportedException || e is PathTooLongException)
			{
				return $"The config file path specified ({Config}) is not valid: {e.Message}";
			}

			try
			{
				MetaFolder = Path.GetFullPath(MetaFolder);
			}
			catch(ArgumentException e)
			{
				return $"The meta folder specified ({MetaFolder}) is not a valid directory: {e.Message}";
			}
			catch(SecurityException e)
			{
				return $"The caller does not have access to the meta folder specified ({MetaFolder}): {e.Message}";
			}
			catch(NotSupportedException e)
			{
				return $"The meta folder specified ({MetaFolder}) is not a valid directory: {e.Message}";
			}
			catch(PathTooLongException e)
			{
				return $"The meta folder specified ({MetaFolder}) is not a valid directory: {e.Message}";
			}

			if(!Directory.Exists(MetaFolder))
				return $"The meta folder specified ({MetaFolder}) does not exist.";

			return null;
		}

		#endregion
	}
}
