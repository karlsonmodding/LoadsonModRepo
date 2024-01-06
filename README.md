# LoadsonModRepo
Public mod repository that tracks actively-developend Loadson mods<br>
This mod automatically updates all of your installed mods that are part of this repository

# Tracker list and repository
[tracker list](https://raw.githubusercontent.com/karlsonmodding/LoadsonModRepo/master/Tracker/trackers) [repository](https://raw.githubusercontent.com/karlsonmodding/LoadsonModRepo/master/Tracker/repository)

# Add your mod
Since this repository is public, we want as many community mods as possible.<br>
Simply open an issue, and you will be instructed on how to add your mod here.

# Version Tracking
LMR searches tracked mods description for the following tags: `<LMR>` and `</LMR>`<br>
This allows easy tracking to check with the repository latest version.<br>
Simply put, if the version found in the installed mod's description doesn't match the one in the repository, the mod is updated.

# Behind the scenes
We keep track of URLs provided by mod-makers that give us the mod tracker<br>
The mod tracker is simply a string containing the mod's GUID, LoadsonModRepo version and download url<br>
At 00:00 UTC everyday, LMR fetches all the trackers and compiles them into a big list (the repository itself)