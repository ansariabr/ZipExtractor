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
        [Option("-k|--keep", CommandOptionType.SingleValue, Description = "Whether to keep all zip files after extraction. Set either true or false. By default all zip files are deleted after extraction")]
        public bool KeepZipAfterExtract { get; } = false;

        [Option("-o|--open", Description = "Open folder after extraction")]
        public bool OpenFolderAfterExtract { get; set; }

        [Option("-d", Description = "By default files are overwritten on extraction if there is any. Use this option to disable this.")]
        public bool DontOverwriteFiles { get; } = true;

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
                Console.WriteLine($"Extracting - {ZipFileToExtract}");
                ExtractAll(ZipFileToExtract, out List<string> extractedZipFilePath, overwriteFiles: DontOverwriteFiles);

                if (KeepZipAfterExtract == false)
                {
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
                    OpenFolder(ZipFileToExtract);
            }
            catch(Exception ex)
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

        private void OpenFolder(string extractedFolderPath)
        {
            string fullZipExtractedPath = Path.GetFullPath(extractedFolderPath);
            var psi = new ProcessStartInfo();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                psi.FileName = "open";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                psi.FileName = "xdg-open";
            }
            else
            {
                psi.FileName = "cmd";
                psi.ArgumentList.Add("/C");
                psi.ArgumentList.Add("start");
            }

            psi.ArgumentList.Add(fullZipExtractedPath);
            Process.Start(psi);
        }
    }

    class ValidZipFileAttribute : ValidationAttribute
    {
        public ValidZipFileAttribute()
            : base("The value must be a valid path(either relative or absolute) to a zip file")
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(Convert.ToString(value)))
                return new ValidationResult(FormatErrorMessage(context.DisplayName));

            if (!File.Exists(Path.GetFullPath(Convert.ToString(value))))
                return new ValidationResult($"File Not Found at - {Path.GetFullPath(Path.GetFullPath(Convert.ToString(value)))}");

            if (!string.Equals(".zip", Path.GetExtension(Convert.ToString(value)), StringComparison.InvariantCultureIgnoreCase))
                return new ValidationResult($"This is not a zip file - {Path.GetFullPath(Path.GetFullPath(Convert.ToString(value)))}");

            return ValidationResult.Success;
        }
    }
}
