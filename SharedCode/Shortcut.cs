using System;
using System.IO;

namespace SelfUpdatingApp
{
    static class Shortcut
    {
        private static readonly Type ShType = Type.GetTypeFromProgID("WScript.Shell");
        private static readonly object Shell = Activator.CreateInstance(ShType);

        public static void Create(string fileName, string targetPath, string arguments = null, string workingDirectory = null, string description = null, string hotkey = null, string iconPath = null)
        {
            IWshShortcut shortcut = (IWshShortcut)ShType.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, Shell, new object[] { fileName });
            shortcut.TargetPath = targetPath;

            if (!string.IsNullOrWhiteSpace(description))
                shortcut.Description = description;

            if (!string.IsNullOrWhiteSpace(hotkey))
                shortcut.Hotkey = hotkey;

            if (string.IsNullOrWhiteSpace(workingDirectory))
                workingDirectory = Path.GetDirectoryName(targetPath);
            shortcut.WorkingDirectory = workingDirectory;

            if (!string.IsNullOrWhiteSpace(arguments))
                shortcut.Arguments = arguments;

            if (!string.IsNullOrEmpty(iconPath))
                shortcut.IconLocation = iconPath;

            shortcut.Save();
        }
    }
}
