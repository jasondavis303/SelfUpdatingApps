using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SelfUpdatingApp
{
    class CLOptions
    {
        public enum Actions
        {
            InstallMe,
            Build,
            Install,
            Update,
            Uninstall,
            PrintUsage
        }

        public string Name { get; set; }
        public string AppId { get; set; }
        public string SourceExe { get; set; }
        public string TargetDir { get; set; }
        public string Depo { get; set; }
        public int ProcessId { get; set; }
        public string PackageFile { get; set; }
        public Actions Action { get; set; }
        public List<Exception> Errors { get; } = new List<Exception>();


        public static CLOptions Parse(string[] args)
        {
            CLOptions ret = new CLOptions();
            
            if (args == null)
                args = Array.Empty<string>();

            if (args.Length == 0)
            {
                ret.Action = Actions.InstallMe;
            }
            else if (args[0].Equals("--help", StringComparison.CurrentCultureIgnoreCase) || args[0].Equals("-h", StringComparison.CurrentCultureIgnoreCase) || args[0].Equals("/h", StringComparison.CurrentCultureIgnoreCase))
            {
                ret.Action = Actions.PrintUsage;
            }
            else
            {
                if (args[0].Equals("build", StringComparison.CurrentCultureIgnoreCase))
                {
                    ret.Action = Actions.Build;

                    if (GetArgument(args, "--name", "build", false, ret.Errors, out string val))
                        ret.Name = val;

                    if (GetArgument(args, "--app-id", "build", true, ret.Errors, out val))
                        ret.AppId = val;

                    if (GetArgument(args, "--source-exe", "build", true, ret.Errors, out val))
                        ret.SourceExe = val;

                    if (GetArgument(args, "--target-dir", "build", true, ret.Errors, out val))
                        ret.TargetDir = val;

                    if (GetArgument(args, "--depo", "build", true, ret.Errors, out val))
                        ret.Depo = val;

                    if (string.IsNullOrWhiteSpace(ret.Name))
                        ret.Name = Path.GetFileNameWithoutExtension(ret.SourceExe);

                }
                else if (args[0].Equals("install", StringComparison.CurrentCultureIgnoreCase))
                {
                    ret.Action = Actions.Install;

                    if (GetArgument(args, "--package", "install", true, ret.Errors, out string val))
                        ret.PackageFile = val;
                }
                else if (args[0].Equals("update", StringComparison.CurrentCultureIgnoreCase))
                {
                    ret.Action = Actions.Update;

                    if (GetArgument(args, "--app-id", "update", true, ret.Errors, out string val))
                        ret.AppId = val;

                    if (GetArgument(args, "--process-id", "update", false, ret.Errors, out val))
                    {
                        try { ret.ProcessId = int.Parse(val); }
                        catch { ret.Errors.Add(new Exception("Unable to read argument: --process-id")); }
                    }
                }
                else if (args[0].Equals("uninstall", StringComparison.CurrentCultureIgnoreCase))
                {
                    ret.Action = Actions.Uninstall;

                    if (GetArgument(args, "--app-id", "uninstall", true, ret.Errors, out string val))
                        ret.AppId = val;
                }
                else
                {
                    ret.Errors.Add(new Exception("Unknown action: " + args[0]));
                }
            }

            return ret;
        }

        private static bool GetArgument(string[] args, string arg, string verb, bool required, List<Exception> exceptions, out string val)
        {
            val = null;
            bool found = false;
            for(int i = 1; i < args.Length; i++)
            {
                if(args[i].Equals(arg, StringComparison.CurrentCultureIgnoreCase))
                {
                    found = true;
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    {
                        val = args[i + 1];
                        return true;
                    }
                    else
                    {
                        exceptions.Add(new Exception("Unable to read argument: " + arg));
                        break;
                    }
                }
            }

            if(!found && required)
                exceptions.Add(new Exception($"Missing required arguent for {verb}: {arg}"));
            
            return false;
        }


        public static string Usage()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Usage:");
            sb.AppendLine();
            sb.AppendLine("\t<no arguments>: Installs the package manager");

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("\tbuild [options]: Builds a package");
            sb.AppendLine("\tRequired Arguments:");
            sb.AppendLine();
            sb.AppendLine("\t\t--app-id\tstring");
            sb.AppendLine();
            sb.AppendLine("\t\t--source-exe\tstring");
            sb.AppendLine();
            sb.AppendLine("\t\t--target-dir\tstring");
            sb.AppendLine();
            sb.AppendLine("\t\t--depo\tstring");
            sb.AppendLine();
            sb.AppendLine("\tOptional arguments:");
            sb.AppendLine();
            sb.AppendLine("\t\t--name\tstring");

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("\tinstall [options]: Install a package");
            sb.AppendLine("\tRequired argument:");
            sb.AppendLine();
            sb.AppendLine("\t\t--package\tstring");

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("\tupdate [options]: Update a package");
            sb.AppendLine("\tRequired arguments:");
            sb.AppendLine();
            sb.AppendLine("\t\t--app-id\tstring");
            sb.AppendLine();
            sb.AppendLine("\tOptional arguments:");
            sb.AppendLine("\t\t--process-id\tnum");

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("\tuninstall [options]: Uninstall a package");
            sb.AppendLine("\tRequired arguments:");
            sb.AppendLine();
            sb.AppendLine("\t\t--app-id\tstring");

            return sb.ToString();
        }
    }
}
