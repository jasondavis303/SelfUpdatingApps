using System;

namespace SelfUpdatingApp
{
    static class ThisApp
    {
        public const bool IsGUIApp = false;

        public const string Handler = "suac";

        public const string Extension = ".suac";

        public static string DestExe =>
            Environment.OSVersion.Platform == PlatformID.Win32NT ? "suac.exe" : "suac";

        public static string SourceExe =>
            Environment.OSVersion.Platform == PlatformID.Win32NT ?
            "https://github.com/jasondavis303/SelfUpdatingApps/releases/latest/download/suac.exe" :
            "https://github.com/jasondavis303/SelfUpdatingApps/releases/latest/download/suac";
    }
}
