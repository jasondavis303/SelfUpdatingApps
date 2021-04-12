﻿using System;
using System.IO;

namespace SelfUpdatingApp
{
    static class Path2
    {
        /// <summary>
        /// Root path of the self updating apps on enduser computer
        /// </summary>
        public static string RootPath => Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "sua")).FullName;


        /// <summary>
        /// Path where all package files are stored
        /// </summary>
        private static string DataPath => Directory.CreateDirectory(Path.Combine(RootPath, "data")).FullName;

        /// <summary>
        /// Path where all apps are stored
        /// </summary>
        private static string AppsPath => Directory.CreateDirectory(Path.Combine(RootPath, "apps")).FullName;

        

        /// <summary>
        /// Location of the specified package file
        /// </summary>
        public static string LocalPackage(string id) => Path.Combine(DataPath, id + ThisApp.Extension);

        /// <summary>
        /// Location of the server package file
        /// </summary>
        public static string ServerPackage(XmlData data)
        {
            string ret = data.Depo;
            if (!ret.EndsWith("/"))
                ret += '/';
            return ret + data.Id + ThisApp.Extension;
        }

        /// <summary>
        /// Location of the folder where an app is installed
        /// </summary>
        public static string InstalledDirectory(string id) => Directory.CreateDirectory(Path.Combine(AppsPath, id)).FullName;

        /// <summary>
        /// Location of the installed executable app
        /// </summary>
        public static string InstalledExe(XmlData data) => Path.Combine(InstalledDirectory(data.Id), data.ExeName);


        /// <summary>
        /// Directory of installed suac.exe & suag.exe
        /// </summary>
        public static string SelfUpdatingBin =>  Directory.CreateDirectory(Path.Combine(RootPath, "bin")).FullName;
            

        /// <summary>
        /// Location of installed suag.exe & suac.exe
        /// </summary>
        public static string SelfUpdatingExe
        {
            get
            {
                string filename = ThisApp.Handler;
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    filename += ".exe";
                return Path.Combine(SelfUpdatingBin, filename);
            }
        }
        

        public static string DesktopLinkPath(string name) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), name + ".lnk");

        public static string TaskBarShortcutPath(string name) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Internet Explorer", "Quick Launch", "User Pinned", "TaskBar", name + ".lnk");
    }
}
