### [0.5.0]

Main additions
- New role: the Minion. He wins with the Werewolves. He can see the werewolves, but the werewolves cannot see him.
- Added the vote-kill system. Players can vote to kill a player at any time (120s cd by default). When a player reaches over 50% votes of alive players, he is vote-killed. Press [N] to open the voting window.

Minor additions
- Added colored roles when spectating
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