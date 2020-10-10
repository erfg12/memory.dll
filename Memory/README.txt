Thank you for choosing Memory.dll! Created by NeWaGe and hollow87 at New Age Software.


BEFORE YOU START
================
1. First add an application manifest file.
    Project > Add New Item... > Application Manifest File (Windows Only)

2. Open the app.manifest file from your Solution Explorer window.

3. Edit line 19 where it says "level=" change "asInvoker" to "requireAdministrator". Save and close.

4. Open NuGet manager and Browse for "System.Security.Principal.Windows", install this.

5. Your trainer should match your game's platform. If compiling with Any CPU, uncheck the prefer 32bit checkbox in Build Project Properties if you need x64.
    Ex: If game is x86, trainer should be x86. If game is x64, trainer should be x64.


GETTING STARTED
===============
1. At the top of your game trainer, type in "using Memory;"

2. Inside your Class brackets, type in "Mem m = new Mem();"

You can now call Memory.dll functions using the prefix "m"! Ex: m.ReadMemory()


- DOCUMENTATION: https://github.com/erfg12/memory.dll/wiki

- SOURCE CODE REPO: https://github.com/erfg12/memory.dll

- WEBSITE: https://github.com/erfg12/memory.dll

