### [0.7.0]

- Updated and rebuilt against Lethal Company Version 80 (build 22825947).
- Updated the BepInEx dependency to 5.4.2305 for the game's current Unity runtime.
- Updated InputUtils to 0.7.13 and Coroner to 2.4.1.
- Updated NetcodePatcher to 4.5.0 and generated RPC handlers for the game's Netcode 1.12.2 runtime.
- Fixed network manager lifecycle across disconnects and subsequent lobbies.
- Fixed a mismatched Harmony patch signature on the end-of-round RPC.
- Updated the runtime UI font for current Unity 2022.3 releases.
- Added null guards for lobby, disconnect, death, and spectating transitions.
- Made packaging/deployment opt-in and updated deployment to the configured game directory.
- Fixed role initialization before the synchronized configuration manager was available.
- Updated custom terminal nodes to use Unity's required ScriptableObject factory.
- Synchronized quota progress from a single host-side counter and ignored duplicate scrap notifications.
- Prevented duplicated configured roles from sharing cooldown and interaction state.
- Validated votes against active players and resolved votes only when voting state changes.
- Made death processing idempotent and fixed Wild Boy deaths before an idol is selected.
- Made role distribution use the actual spawned, controlled player set.
- Reduced targeting and HUD per-frame allocations and completed persistent-manager cleanup.
- Prevented Seer targeting from throwing when a raycast hit references an invalid Unity collider or transform.
- Hardened terminal parsing, role deserialization, player lookup, and configuration ranges.



### [0.6.0]

New roles:
- Alpha Werewolf: Can turn other players into werewolves
- Fake Seer: she only has a 50% chance of guessing the actual player's role

Minor additions and changes
- Added the option to disable the quota from the LethalConfig menu

Bug fixes
- Fixed a bug that would prevent closing the vote window at the lobby



### [0.5.5]

Minor additions:
- Added custom death messages (Coroner)
- Now displays the winning team on the performance report

Bug fixes:
- Fixed the targetting issue from Glitch's stream. It was due to a conflict with OpenBodyCams.
- Fixed a bug on detecting when only werewolves are left, that was preventing werewolves to ignore the daily quota.



### [0.5.4]

Minor changes
- Added support for LethalConfig. All parameters can now be edited in game through the LethalConfig menu, without a restart (host only).
- When the wild boy transforms, he now only has 30s on his werewolf cooldown, instead of the base 120s
- Wild Boy kill cooldown on transform can be edited from config file

Bug fixes
- Fixed a bug where the wild boy transformation would reset its lover status
- Added a check on automated roles distribution, to avoid distributing roles multiple times per round. This is to prevent some external mods from interferring with Werewolves Company
- Fixed a bug where the daily quota could be reset to 0 due to interferences with other mods
- Fixed a bug where a daily quota would be required at the company building
- Properly disables role HUD on round end



### [0.5.3]

Bug fixes:
- Fixed a bug where changing roles mid-round (e.g., Wild Boy) would reset the lovers status.
- Fixed a bug where actions would failed to be performed due to some desync between clients and server. At least that's what I think is causing the issue, I could not test it locally.
- Fixed a bug where the role HUD would not disappear at lobby.
- Temporary fix for the quota bug




### [0.5.2]

Minor additions:
- Allows werewolves to depart the ship when there are no villagers alive, even if the quota is not met.

Bug fixes:
- Fixed tooltip not showing up when aiming at a player (bug introduced in 0.5.0)
- Removed unused "Default Role" parameters





### [0.5.1]

I forgot to add the assetbundle in 0.5.0. I added it there.

Added configurable quota parameters.





### [0.5.0]

Main additions
- Added the vote-kill system. Players can vote to kill a player at any time (120s cd by default). When a player reaches over 50% votes of alive players, he is vote-killed. Press [N] to open the voting window.
- Added a daily Quota. It scales from the number of players and the total scrap value of the map. It was copied from what was done for Infected Company. It will probably require tweaking.
- New roles:
  - The Minion: He wins with the Werewolves. He can see the werewolves, but the werewolves cannot see him.
  - The Drunken Man: He is so drunk that he is immune to the Witch poison.

Minor additions
- Added colored roles names when spectating
- Added colored roles names in the terminal
- Disables the mod HUD when dead
- Disables the HUD when disconnecting from a game

Bug fixes:
- Fixed a bug where Cupid would see he made a couple of twice the same person. It was only a visual bug though, and only for Cupid. The couple would still know they were targetted.




### [0.4.0]

Main additions
- New role: Cupid. He can make two players fall in love. If one of them dies, their lover also dies.
- Werewolves can now see each others (Can be disabled in configs)
- Disabled the tooltip for when a player body is dropped in the ship (can be disabled in configs)

Other additions
- Werewolf role is now displayed in red at the top of the screen
- The seer now see werewolves roles in red
- Added terminal command to get informations on a specific role : 'wc RoleName'
- For roles that can use their action only once per round, disable the tooltip on aiming at a player after using the action
 
Bug fixes:
- Fixed a bug introduced in 0.3.0 that prevented adding multiple roles at once ('wc add role1 role2')
- Re-organized the config file sections
 




### [0.3.1]

Turned off the debugging entries





### [0.3.0]

Added 'wc del *' command, to remove all current roles from the list

Added 'wc add RoleName N' command, to add N times the role RoleName

Multiple bug fixes:
- Fixed a bug where players were able to use their action through walls and objects
- Fixed a bug where spectating a player would display the role of the previously spectated player rather than the currently spectated one
- Fixed a bug where the first time a non-host would check the current roles list, it would appear empty.
- Fixed a bug where clients were not able to use the 'wc debug distrib/distribute' terminal command






### [0.2.3]

Now fully compatible with LateCompany (I think). Fixed multiples bugs with disconnecting and reconnecting:
- Fixed a bug where a player would not be given a role if he disconnected and reconnected during the lobby
- Fixed a bug where a player could not interact with other players if he disconnected and reconnected during the lobby

Probably also fully compatible with MoreCompany. Will require further testing with 5+ instances/players





### [0.2.2]

Custom death message when you die to a player's role.

When spectating a player, now also displays its role next to his name.

Increased Werewolf, Witch and Villager default range from 1.0 to 1.5.





### [0.2.1]

Centered RoleHUD

Fixed an issue where the mod would prevent interactions texts from displaying when facing interactable objects.





### [0.2.0]

Huge performance improvements.

Changed default config values.

Added debug commands in the terminal.

Changed default keybinds.

Added cooldowns to the top-screen HUD.





### [0.1.6]

Fixed a bug where all actions would target the host instead of the desired target. It worked in local tests, but I realized the bug when trying the mod online with friends... Sorry!

Fixed a bug where the mod would prevent Hover-Tips from displaying





### [0.1.5]

Added 'wc' alias for 'werewolves' in the terminal.

Fixed bugs with the terminal interface.





### [0.1.4]

Added a default roles setup list, including all roles.

Also disabled the debug logs, which I forgot to do in 0.1.4.





### [0.1.4]

Added the roles setup in the ship terminal

Re-Enabled the hold-P (5s) keybind to distribute roles. This is to be used in case the default roles distribution fails.





### [0.1.3]

Added LICENSE

Notified in README that hold-P keybind is disabled





### [0.1.2]

Disable the hold-P keybind, it's giving errors





### [0.1.1]

Add missing files





### [0.1.0]

First release. Missing the possibility to change the enabled roles within the ship.
