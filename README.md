# ZipExtractor
A CLI for nested zip file extractor

[![NuGet][main-nuget-badge]][main-nuget]

[main-nuget]: https://www.nuget.org/packages/ZipExtractor/
[main-nuget-badge]: https://img.shields.io/nuget/v/ZipExtractor.svg?style=flat-square&label=nuget

A CLI tool for extracting zip files. It can extract all the nested zip files as well.

## Get started

Download .NET Core [3.1](https://get.dot.net) or newer.
Once installed, run this command:

```
dotnet tool install --global ZipExtractor
```

Once installed, it can be invoked using ```ziputil``` command from command prompt.


## Usage

```
Usage: ziputil <zip file path> [options]

Options:
  -d|--delete           Delete all zip files after extraction
  -o|--open             Open folder after extraction
  -n                    By default files are overwritten on extraction if there is any. Use this option to disable this

Note : Zip file path can be relative or absolute.
```

## Example
Extracting file using relative path

```ziputil MyFile.zip```

Extracting file using absolute path

```ziputil "C:\Users\My User Name\Downloads\MyFile.zip"```

Extracting file, deleting the zip files, and opening the folder after extraction

```ziputil MyFile.zip -d -o```
