using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace ZipExtractor
{
    [Command(Name = "ZipUtil", Description = "<<<-----A CLI for nested zip file extractor----->>>",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue, AllowArgumentSeparator = false)]
    class Program
    {
        [Option("-d|--delete", Description = "Delete all zip files after extraction")]
        public bool DeleteAllZipAfterExtract { get; } = false;

        [Option("-o|--open", Description = "Open folder after extraction")]
        public bool OpenFolderAfterExtract { get; }

        [Option("-n", Description = "By default files are overwritten on extraction if there is any. Use this option to disable this")]
        public bool? DontOverwriteFiles { get; }

        [Argument(0, "Either relative or absolute path of zip file")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Either relative or absolute path of zip file is required")]
        [ValidZipFile]
        public string ZipFileToExtract { get; }


        public static int Main(string[] args)
        => CommandLineApplication.Execute<Program>(args);
        private void OnExecute()
        {
            try
            {
                string fullyQualifiedZipFilePath = Path.GetFullPath(ZipFileToExtract);
                ExtractAll(fullyQualifiedZipFilePath, out List<string> extractedZipFilePath, overwriteFiles: DontOverwriteFiles.HasValue ? false : true);

                if (DeleteAllZipAfterExtract)
                {
                    Console.WriteLine();
                    foreach (var zipFilePath in extractedZipFilePath)
                    {
                        try
                        {
                            File.Delete(zipFilePath);
                            Console.WriteLine($"Deleted - {zipFilePath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unable to delete {zipFilePath}.");
                            Console.WriteLine($"Exception - {ex}");
                        }
                    }
                }

                if (OpenFolderAfterExtract)
                    OpenFolder(fullyQualifiedZipFilePath);

                Console.WriteLine($"Completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong - {ex}");
            }
        }

        static void ExtractAll(string fullPath, out List<string> pathOfExtractedZipFiles, bool overwriteFiles = true)
        {
            pathOfExtractedZipFiles = new List<string>();
            if (fullPath.EndsWith(".zip"))
            {
                pathOfExtractedZipFiles.Add(fullPath);
                string extractPath = $@"{Path.GetDirectoryName(Path.GetFullPath(fullPath))}\{Path.GetFileNameWithoutExtension(fullPath)}";
                ZipFile.ExtractToDirectory(fullPath, extractPath, overwriteFiles);
                Console.WriteLine($"Extracted - {fullPath}");

                foreach (var allFiles in Directory.GetFiles(extractPath))
                {
                    ExtractAll(allFiles, out List<string> subPaths);
                    if (subPaths?.Count > 0)
                        pathOfExtractedZipFiles.AddRange(subPaths);
                }

                foreach (var dirs in Directory.GetDirectories(extractPath, "*", SearchOption.AllDirectories))
                {
                    foreach (var allFiles in Directory.GetFiles(dirs))
                    {
                        ExtractAll(allFiles, out List<string> subPaths);
                        if (subPaths?.Count > 0)
                            pathOfExtractedZipFiles.AddRange(subPaths);
                    }
                }
            }
        }

        private void OpenFolder(string fullFilePath)
        {
            var directoryPath = Path.GetDirectoryName(fullFilePath);
            var psi = new ProcessStartInfo();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                psi.FileName = "open";
                psi.ArgumentList.Add(directoryPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                psi.FileName = "xdg-open";
                psi.ArgumentList.Add(directoryPath);
            }
            else
            {
                //opening the folder in file explorer, by default the extracted folder will be selected
                string fullExtractedDirPath = string.Format($@"{directoryPath}\{Path.GetFileNameWithoutExtension(fullFilePath)}");
                psi.FileName = "explorer";
                psi.Arguments = string.Format("/select,\"{0}\"", fullExtractedDirPath);
            }

            Process.Start(psi);
        }
    }

    class ValidZipFileAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            string fullyQualifiedFilePath = Path.GetFullPath(Convert.ToString(value));

            if (!File.Exists(fullyQualifiedFilePath))
                return new ValidationResult($"File Not Found at - {fullyQualifiedFilePath}");

            if (!string.Equals(".zip", Path.GetExtension(fullyQualifiedFilePath), StringComparison.InvariantCultureIgnoreCase))
                return new ValidationResult($"Invalid zip file - {fullyQualifiedFilePath}");

            return ValidationResult.Success;
        }
    }
}
