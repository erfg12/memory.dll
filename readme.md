**My Edition Example**
```
AoBInput scanList = new AoBInput();
scanList.Add("LocalPlayer", "50 3F 9A FF");
scanList.Add("EntityList", "50 3F 9A AA");

AoBOutput result = (await _memLib.AoBScan(StartAddress, EndAddress, scanList, true, true, ""));
long LocalPlayerAddr = result["LocalPlayer"].FirstOrDefault();
List<long> EntityAddress = result["EntityList"];
```
----
This library file can be used to create PC game cheat trainers. It can read and write to any program process.

Please check out the [Wiki page](https://github.com/erfg12/memory.dll/wiki) on the github repo for how to use it.

NuGet has the latest compiled releases. https://www.nuget.org/packages/Memory.dll/ 

Use Visual Studio to get it! Project > Manage NuGet Packages... > Browse > Search "memory.dll".

memory.dll now requires 4.7.1 .NET framework! You can re-compile memory.dll at a lower framework with the compile flag "WINXP". However, AoB scanning will not work at anything lower than 4.7.1.

![https://discord.gg/9d7fB5a](https://i1.wp.com/community.sliver.tv/wp-content/uploads/2018/09/discooord.png)
