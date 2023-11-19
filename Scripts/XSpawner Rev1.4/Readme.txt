=============================
XSpawner Rev 1.4 (ServUO)
Originally Modded By Demeted
Rev 1.4 Updated by Delphi
=============================

=============================
Changelog: 1.3 to 1.4
=============================
Fixed dead command links
Changed load paths to "Xspawner" from "Spawner" to prevent xmlspawner from mixing the spawnfiles up
Removed "Get Spawner" Command
Removed "Set Spawner" Command
Added "Smartspawn On"
Added "XML Attachement List"
Fixed GMBody
Fixed Show and Hide Spawners
Added Spawn Files for Trammel, Felucca, Illshenar, Malas, Tokuno and Termur

==============
Known Bugs
==============
The "Clearall" command will sometimes crash the server. I have been able to prevent this by using [Deleteworld first then use the "Clear All Facets". Something in "Server\Timer.cs" in ServUO is causing this. 

The TerMur spawns are incomplete fome of the files are empty. I had to track down the XML spawnfiles online so I have collected what I could. The rest of the Facet spawn files should be complete.  
===============
Credits
===============
All credit for this cool system goes to Demented. He was the first one to make this mod. Thank you Demented for your work, and of course Nerun who made the original "Premium Spawner" system off of which this is based. 

Also the respective authors of all the XML tutorials included. I do not know them all but thank you for your work!

Thanks to Dmurphy for posting the spawnfiles located here https://sourceforge.net/p/uov/uov/ci/67ddcdd8a18179afa718011ceb04e93c55489d09/?page=0

===============
Installation
===============
1) Just drop the whole folder into your Custom Scripts folder.

2) Move the XSpawner folder to the root directory of your server where your .EXE is located

3) Enjoy!


