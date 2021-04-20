[![release](https://github.com/jasondavis303/SelfUpdatingApps/actions/workflows/release.yml/badge.svg)](https://github.com/jasondavis303/SelfUpdatingApps/actions/workflows/release.yml)


Nuget Windows Forms: https://www.nuget.org/packages/suag/

Nuget Console: https://www.nuget.org/packages/suac/


Windows Forms Exe: https://github.com/jasondavis303/SelfUpdatingApps/releases/latest/download/suag.exe 

Windows Console Exe: https://github.com/jasondavis303/SelfUpdatingApps/releases/latest/download/suac.exe

Linux Console Exe: https://github.com/jasondavis303/SelfUpdatingApps/releases/latest/download/suac


<pre>
Usage: suag.exe|suac.exe [verb] [options]

Verbs:

  install-me    (Default Verb)

  build

  install

  update

  uninstall

  help          Display more information on a specific command.

  version       Display version information.




suag.exe|suac.exe install-me

  --process-id    Wait for this process to close before installing

  --no-gui        Silent install (for scheduled updates)




suag.exe|suac.exe build [options]

  --app-id        Required.

  --source-exe    Required.

  --target-dir    Required.

  --depo          Required.

  --name          Friendly name. If not specified, it's derived from source-exe

  --app-version   Set app version. If not specified, it's derived from DateTime.UtcNow

  --force-suag    Force output suag instead of suac. Useful for command line builds with no gui

  


suag.exe|suac.exe install [options]

  --package    Required. Path to the package manifest to install


  

suag.exe|suac.exe update [options]

  --app-id        Required.

  --process-id    Wait for this process to close before updating

  --relaunch-args Base64 encoded string of args to pass to the app after updating




suag.exe|suac.exe uninstall [options]

  --app-id        Required.
</pre>