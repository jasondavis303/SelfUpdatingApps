[![release](https://github.com/jasondavis303/SelfUpdatingApps/actions/workflows/release.yml/badge.svg)](https://github.com/jasondavis303/SelfUpdatingApps/actions/workflows/release.yml)


Nuget Windows Forms: https://www.nuget.org/packages/suag/

Nuget Console: https://www.nuget.org/packages/suac/


Windows Forms Exe: https://github.com/jasondavis303/SelfUpdatingApps/releases/latest/download/suag.exe 

Windows Console Exe: https://github.com/jasondavis303/SelfUpdatingApps/releases/latest/download/suac.exe

Linux Console Exe: https://github.com/jasondavis303/SelfUpdatingApps/releases/latest/download/suac


<pre>
Usage: suag.exe|suac.exe [verb] [options]

Verbs:

  build

  install-me    (Default Verb)

  install

  update

  uninstall

  help          Display more information on a specific command.

  version       Display version information.




suag.exe|suac.exe build [options]

  --app-id        Required.

  --source-exe    Required.

  --target-dir    Required.

  --depo          Required.

  --no-gui        When part of CI/CD, output build progress to the console

  --name          Friendly name. If not specified, it's derived from source-exe




suag.exe|suac.exe install [options]

  --package    Required. Path to the package manifest to install


  

suag.exe|suac.exe update [options]

  --app-id        Required.

  --process-id    Wait for this process to close before updating




suag.exe|suac.exe uninstall [options]

  --app-id        Required.
</pre>