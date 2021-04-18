using System;
using System.Threading.Tasks;

namespace TestApp_Console
{
    class Program
    {
        const string APP_ID = "76B1FA17-7159-4434-9974-54B6D843EE87";

        static void Main()
        {
            bool launch = false;

            try
            {
                launch = Run().Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine();

            if (launch)
            {
                SelfUpdatingApp.Installer.Launch(APP_ID);
                return;
            }

            Console.WriteLine("Press any key to continue...");
            Console.Read();
        }

        static async Task<bool> Run()
        {
            Console.WriteLine("Current Version: {0}", SelfUpdatingApp.Installer.GetInstalledVersion(APP_ID));
            
            bool update = await SelfUpdatingApp.Installer.IsUpdateAvailableAsync(APP_ID).ConfigureAwait(false);
            Console.WriteLine("Update Available: {0}", update);


            return update;
        }
    }
}
