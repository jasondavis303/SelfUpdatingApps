using CommandLine;

namespace SelfUpdatingApp
{
    class CLOptions
    {
        [Verb("install-me", isDefault: true)]
        public class InstallMeOptions 
        {
            [Option("process-id", HelpText = "Wait for this process to close before updating")]
            public int ProcessId { get; set; }

            [Option("no-gui", HelpText = "Silent install (for scheduled updates)")]
            public bool NoGui { get; set; }
        }

        [Verb("build")]
        public class BuildOptions
        {
            [Option("app-id", Required = true)]
            public string AppId { get; set; }
            
            [Option("source-exe", Required = true)]
            public string SourceExe { get; set; }
            
            [Option("target-dir", Required = true)]
            public string TargetDir { get; set; }
            
            [Option("depo", Required = true)]
            public string Depo { get; set; }

            [Option("name", HelpText = "Friendly name. If not specified, it's derived from source-exe")]
            public string Name { get; set; }

            [Option("app-version", HelpText = "Set app version. If not specified, it's derived from DateTime.UtcNow")]
            public string Version { get; set; }

            [Option("force-suag", HelpText = "Force output suag instead of suac. Useful for command line builds with no gui")]
            public bool ForceSUAG { get; set; }
        }

        [Verb("install")]
        public class InstallOptions
        {
            [Option("package", Required = true, HelpText = "Path to the package manifest to install")]
            public string Package { get; set; }
        }

        
        [Verb("update")]
        public class UpdateOptions
        {
            [Option("app-id", Required = true)]
            public string AppId { get; set; }

            [Option("process-id", HelpText = "Wait for this process to close before updating")]
            public int ProcessId { get; set; }

            [Option("relaunch-args", HelpText ="Base64 encoded string of args to pass to the app after updating")]
            public string RelaunchArgs { get; set; }
        }

        [Verb("uninstall")]
        public class UninstallOptions
        {
            [Option("app-id", Required = true)]
            public string AppId { get; set; }
        }
    }
}