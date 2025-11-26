## DESCRIPTION
Create great PC game cheat trainers in C# with this easy to use library! This library is available in a [NuGet package repo](https://github.com/erfg12/memory.dll/wiki/Adding-GitHub-NuGet-package-feed), includes XML IntelliSense docs and this code repo provides new build releases every commit. For support please check the [wiki tab](https://github.com/erfg12/memory.dll/wiki) in this repo and the [CSharp-Game-Trainers repo](https://github.com/erfg12/CSharp-Game-Trainers) for coding samples.

- For legacy Windows operating systems, check out [memory_legacy.dll](https://github.com/erfg12/memory_legacy.dll)

- For MacOS operating systems, check out [memory.dylib](https://github.com/erfg12/memory.dylib)

## FEATURES
* Check if process is running (ID or name) and open, all in 1 function.
* 32bit and 64bit games supported.
* AoB scanning with full & partial masking.
    * _Example: "?? ?? ?? ?5 ?? ?? 5? 00 ?? A9 C3 3B ?? 00 50 00"_
* Inject DLLs and create named pipes to communicate with them.
    * See [this wiki article](https://github.com/erfg12/memory.dll/wiki/Using-Named-Pipes) for more info.
* Write to addresses with many different value types.
    * _Example: byte, 2bytes, bytes, float, int, string, double or long_
* Optional external .ini file for code storage.
* Address structures are flexible. Can use modules, offsets and/or pointers. 
    * _Example: "game.exe+0x12345678,0x12,0x34,0x56"_
* Freeze values (infinte loop writing in threads)
* Bind memory addresses to UI elements

## DOCUMENTATION
[Wiki Pages](https://github.com/erfg12/memory.dll/wiki)

[Sample Trainer Code](https://github.com/erfg12/CSharp-Game-Trainers)

## VIDEOS
[![](https://img.youtube.com/vi/J-Zp6XtxnX0/0.jpg)](https://www.youtube.com/watch?v=J-Zp6XtxnX0)
[![](https://img.youtube.com/vi/OKJsbDDh5CE/0.jpg)](https://www.youtube.com/watch?v=OKJsbDDh5CE)
[![](https://img.youtube.com/vi/STPrGJ8eI8Y/0.jpg)](https://www.youtube.com/watch?v=STPrGJ8eI8Y)
[![](https://img.youtube.com/vi/w9m0gmcS82Y/0.jpg)](https://www.youtube.com/watch?v=w9m0gmcS82Y)
[![](https://img.youtube.com/vi/3u8bxtqCtcQ/0.jpg)](https://www.youtube.com/watch?v=3u8bxtqCtcQ)
