using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace SelfUpdatingApp
{
    static class Packager
    {
        public static async Task BuildPackageAsync(CLOptions.BuildOptions opts, IProgress<ProgressData> progress = null)
        {
            opts.SourceExe = Path.GetFullPath(opts.SourceExe);

            //if (!opts.SourceExe.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
            //    throw new Exception("The source-exe argument must be an executable file");

            if (!File.Exists(opts.SourceExe))
                throw new FileNotFoundException("The executable file was not found", opts.SourceExe);

            if (string.IsNullOrWhiteSpace(opts.Name))
                opts.Name = Path.GetFileNameWithoutExtension(opts.SourceExe);

            //Get info
            progress?.Report(new ProgressData("Gathering info"));
            DateTime dt = DateTime.UtcNow;
            Version version = new Version
                (
                    dt.Year - 2000,
                    dt.Month,
                    dt.Day,
                    (dt.Hour * 60) + dt.Minute
                );
            if (!string.IsNullOrWhiteSpace(opts.Version))
                version = new Version(opts.Version);

            
            string inDir = Path.GetDirectoryName(opts.SourceExe);
            var sourceFiles = new DirectoryInfo(inDir).GetFiles("*", SearchOption.AllDirectories);
            double totalLength = sourceFiles.Sum(item => item.Length);

            //Fix the input folder - if it doesn't end with the dir sep char, then the entries in the zip will be wrong
            if (!inDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                inDir += Path.DirectorySeparatorChar;

            string exeName = Path.GetFileName(opts.SourceExe);
            string xmlFile = Path.Combine(opts.TargetDir, "packages", opts.AppId + ThisApp.Extension);
            string zipFile = Path.Combine(opts.TargetDir, "packages", opts.AppId + ".zip");
            string friendlyFile = Path.Combine(opts.TargetDir, opts.Name + ThisApp.Extension);
            string msg = $"Building v{version}";

            if(opts.ForceSUAG && ThisApp.Extension == ".suac")
            {
                xmlFile = Path.ChangeExtension(xmlFile, ".suag");
                friendlyFile = Path.ChangeExtension(friendlyFile, ".suag");
            }



            //Build the xml
            progress?.Report(new ProgressData(msg));
            var package = new XmlData
            {
                Depo = opts.Depo,
                ExeName = exeName,
                Id = opts.AppId,
                Name = opts.Name,
                Version = version
            };
            package.Save(xmlFile);
            package.Save(friendlyFile);

            //Create the package
            double totalRead = 0;
            int lastPerc = 0;
            byte[] buffer = new byte[Constants.BUFFER_SIZE];
            using (var zipStream = StreamHelper.CreateAsyncWrite(zipFile))
            {
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
                foreach (var file in sourceFiles)
                {
                    using var fileStream = file.OpenAsyncRead();
                    string entryName = file.FullName.Substring(inDir.Length);
                    using var entryStream = archive.CreateEntry(entryName, CompressionLevel.Optimal).Open();
                    long readFromFile = 0;
                    while (readFromFile < file.Length)
                    {
                        int thisRead = await fileStream.ReadAsync(buffer, 0, Constants.BUFFER_SIZE).ConfigureAwait(false);
                        await entryStream.WriteAsync(buffer, 0, thisRead).ConfigureAwait(false);
                        readFromFile += thisRead;
                        totalRead += thisRead;

                        int perc = Math.Min((int)Math.Floor((totalRead / totalLength) * 100), 99);
                        if (perc != lastPerc)
                        {
                            lastPerc = perc;
                            progress?.Report(new ProgressData(msg, perc));
                        }
                    }
                }
            }

            progress?.Report(new ProgressData(msg, 100));
        }
    }
}