using System;

namespace SelfUpdatingApp
{
    class InstallResult
    {
        /// <summary>
        /// Whether the InstallOrUpdate process completed successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// If the InstallOrUpdate process failed, this will hold the exception
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// Path to the currently highest installed version executable
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The currently highest installed version
        /// </summary>
        public Version Version { get; set; }
    }
}