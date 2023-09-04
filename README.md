# meta-dotnet-core-gen
Scapes Microsoft's download website and generates bitbake recipe files for meta-dotnet-core project.

## Background
This program will scrape Microsoft's online download webpages and pull all the current versions that can be downloaded. It will then
compute MD5 and SHA256 hashes of the files and store that information in a config file (so you don't have to do this each time). It will
then compare the online information to a meta-dotnet-core file structure and generate/update and bitbake recipe files that have changed
or are not found.

## Usage
Compile and run as a command line. Running with a '-h' ('dotnet ./meta_dotnet_core_gen.dll -h') will output the following help:

````
NAME
    meta_dotnet_core_gen

SYNOPSIS
    [-h] [-c=<config file>] -m=<meta-dotnet-core folder>

DESCRIPTION
    Scrapes the .Net Download websites and generates bitbake recipes to pull in the Arm, Arm64, and x64 binaries.

    c,config=Config
        [Optional] - Specifies the configuration file to pull in the previously scraped and computed values. Defaults
        to core_download_cfg.xml in the working directory if no value is provided.
    h,help
        [Optional] - Displays this help and then exits.
    m,metalayer=MetaFolder
        [Optional] - Path to the meta-dotnet-core folder. This app uses this folder to determine which versions have
        already been created. It will also populate this folder with the newly generated output files.
    v,verify
        [Optional] - Verifies the existing recipes with the online content.
````

## Examples
A current configuration file is provided so if you were running this out of the build folder you could run:
````
dotnet ./meta_dotnet_core_gen.dll -c=../../../../core_download_cfg.xml -m=/opt/poky/3.4.4/sources/meta-dotnet-core
````
The application will then prompt you to update online information (configuration file) and then provide information on
any differences between online and meta layer. It will then prompt you to update the meta layer. Selecting yes will generate
the latest build recipes.
