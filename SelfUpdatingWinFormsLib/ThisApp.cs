namespace SelfUpdatingApp
{
    static class ThisApp
    {
        public static bool IsGUIApp => true;

        public const string Handler = "suag";

        public const string Extension = ".suag";

        public const string DestExe = "suag.exe";

        public const string SourceExe = "https://github.com/jasondavis303/SelfUpdatingApps/releases/latest/download/suag.exe";
    }
}
