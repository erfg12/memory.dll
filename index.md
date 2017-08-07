---
layout: default
---

# [](#header-1)Getting Started

<p><li style="font-weight:bold;">Open Visual Studio and make a new project.</li></p>
<li style="font-weight:bold;">Add reference to Memory.dll.</li>
<span class="highlight_this">Project</span> » <span class="highlight_this">Manage NuGet Packages...</span> » <span class="highlight_this">select Browse</span> » <span class="highlight_this">Search For Memory.dll</span>
<div style="padding:5px;"><i>Or, for manual installation...</i></div>
<span class="highlight_this">Project</span> » <span class="highlight_this">Add Reference</span> » <span class="highlight_this">Browse...</span> » <span class="highlight_this">Select Memory.dll</span>
<li style="font-weight:bold;">Place using statement above namespace.</li>
```csharp
using Memory;
```
<li style="font-weight:bold;">Create a namespace for MemLib in your class.</li>
```csharp
public Mem MemLib = new Mem();
```

<span style="font-weight:bold;">IMPORTANT:</span> Your program must run with admin privileges! <a href="https://github.com/erfg12/memory.dll/wiki/Administrative-Privileges">Click here to learn how.</a>

Now you can use MemLib.FUNCTIONS(); in our project! See sample projects below.

<a href="https://github.com/erfg12/memory.dll/wiki">All functions are listed in the wiki docs. Click here to visit the page.</a>
<hr>
# [](#header-1)Sample Projects

<a href="https://github.com/erfg12/Game-Trainers" id="view-on-github" class="button"><span>Game Trainers</span></a>