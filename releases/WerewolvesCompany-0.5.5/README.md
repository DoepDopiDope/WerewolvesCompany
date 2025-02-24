# WerewolvesCompany

Adds a variant of the Werewolves Game to Lethal Company.

#### For requests, contact me on Discord ([doep](https://discord.com/users/238054619694104576)), on the dedicated [modding thread](https://discord.com/channels/1168655651455639582/1336122279863652454) or on [github](https://github.com/DoepDopiDope/WerewolvesCompany)



## General information

Werewolves is a party game where Villagers must find one (or multiple) Werewolves within the village. At night, the Werewolves may vote and kill a Villager. When Villagers wake up, they learn who was killed. They may vote and eliminate who they think may be a Werewolf.

WerewolvesCompany brings a variant of this game to Lethal Company. Each round, players are assigned a random role:
- If they're part of the village, they must eliminate the Werewolve(s) before ship departure,
- If they're a Werewolf, they must kill the Villagers before ship departure.

### IMPORTANT NOTE

By default, the mod disables the tooltip for dropping a player body in the ship. You can enable it if you wish by setting the config parameter "Disable Body in Ship tooltip" to false.





## Objectives

The game sets up random roles for all players. Each role is either part of the Village or the Werewolves. Your role is permanently displayed at the top of your screen. You can also bring the tooltip for your role by pressing **[M]**

### Villagers

Members of the Village (the Villagers) should bring back a certain amount of scrap to the ship during the round. The lever is blocked as long as this daily quota is not met.



### Werewolves

Werewolves (and their allies) should prevent the villagers from reaching the daily quota. Since the scraps are counted towards the quota when entering the ship, it is useless for werewolves to try and bring scraps out of the ship.



### Quota

Villagers are required to meet a daily quota of by bringing scraps to the ship. The daily quota is computed as follows:

`quota = totalLevelValue * (baseMultiplier + playerWeight * (Nplayers - NplayersOffset) )`, with default values being:
- `baseMultiplier = 0.25`
- `playerWeight = 0.05`
- `NplayersOffset = 3`.

- This yields the default formula :

`quota = totalLevelValue * (0.25 + 0.05 * (Nplayers-3) )`

As a safeguard, the daily quota value can never reach a certain multiplier (default `maxMultiplier`) of the `totalLevelValue`. This can be edited from the config file.

This formula was copied from what had been done for [Infected Company](https://thunderstore.io/c/lethal-company/p/InfectedCompany/InfectedCompany/). However, it included some game modifiers that affected the pace of the rounds. It is likely that this formula should be tweaked. All parameters can be edited from the Config File.





### Voting

Players can open the vote menu (**[N]** key by default) and select a player that they want to kill. When a player reaches enough votes towards him (>50% of alive players), he is killed.



### Roles setup

There are lots of roles to choose from, that will be distributed during the game. It is recommended that players try different configurations. The general recommendation is to have 1 Werewolves for every 3 to 4 Villagers, depending on how strong the Village is from his players powers (e.g., Witch, Seer, ...).

Players can edit the roles setup from the ship's terminal. See section: Setup and parameters. Empty slots will be filled with regular Villagers.





## Setup and parameters

You can edit the roles configuration from the Ship's terminal. You can access the configuration menu by typing 'werewolves'.

Once in the Werewolves menu, you can add or remove roles from the current setup by typing :
- werewolves add Role_Name N - Add N (if provided, else 1) of the selected role
- werewolves del Role_Name   - delete the role from the current list
- werewolves del *           - deletes all roles from the current list

You can get informations on a role by typing:
- werewolves Role_Name

You can also shorten 'werewolves' with its alias 'wc'.





## KeyBinds

- **[Z]** Perform main action (Kill, Poison, Seer, Idolize)
- **[V]** Perform secondary action (Make Immune)
- **[M]** Display my role ToolTip
- **[P]** (hold for 5s) Distribute roles. Only exists in case the roles fail to be distributed at the beginning of the round





## Roles

### Werewolf

The Werewolf shall kill other players before ship departure.

The Werewolf has the ability to kill another player (with cooldown).

The Werewolves can see each other if the option is enabled (on by default). See the "Werewolves Know Each Other" config parameter.



### Villager

The Villager shall find and kill the Werewolves before ship departure.

The Villager can *patpat* others players.



### Witch

The Witch shall find and kill the Werewolves before ship departure.

The Witch has two potions, and can do two things:
- Poison another player and kill him (once per round)
- Protect another player and make him immune **once** to a Werewolf attack (once per round). The immune player won't know he has been immunized, nor will he know he loses his immune status. The Witch cannot protect herself.

Note that unlike the original Werewolves game, the Witch here does not revive a dead player. This change was made so that a killed-player cannot instantly reveal the Werewolf(ves) identity.



### Seer

The Seer shall find and kill the Werewolves before ship departure.

The Seer can seer another player's role.



### Wild Boy

The Wild Boy wins either with the Villagers or the Werewolves, depending on his status.

The Wild Boy can target a player who becomes his idol. If the idol dies, the Wild Boy becomes a Werewolf. As long as his idol is alive, he wins with the Villagers.



### Cupid

Cupid wins with the village.

Cupid can make two players fall in love. Their fate is linked: if one of them dies, their lover also dies. If they were both originally in the same team, they must win with that team. If they were originally in different teams, they now must be the only two survivors.



### Minion

The Minion wins with the werewolves.

The Minion can see other Werewolves, but the Werewolves cannot see him.



### The Drunken Man

The Drunken Man wins with the village

The Drunken Man is so drunk that he is immune to the Witch poison. He will be notified that an old lady has tried giving him a strong beverage. The Witch will also be notified that her potion has no effect.

 



## Debug

While they are mainly for my personal use during playtests, you can use debug commands from the terminal (mainly for my personal use when testing changes)
- wc debug         -> show available debug commands
- wc debug cd      -> set all cooldowns to 0
- wc debug distrib -> distribute roles. Alternative to the hold-P (5s) keybind
- wc debug reset   -> reset state of every player current role to its initial state





## Planned updates

Theses features may be added in the future:

### Major possible updates

- Change the Witch poison to actually poison the target. The poisoned player would start losing HP (after some time, to avoid revealing the Witch identity)
- Maybe add a vote system for the ship departure. Will have to think about it.
- Change the vote kill trigger (idea from glitched npc). Add something in the ship with a screen and a button/bell


### Minor possible updates

- Add a way for players to save custom roles setups
- Add a way for players to change the role setup from LethalConfig
- Icon for the roles to be displayed at the top of the screen
- Randomize the roles list to avoid meta-game



### New roles ideas

- **The Bounty Hunter** - Each round, his goal is to kill a random player
- **The Apprentice Seer** - As long as the Seer is alive, she does not have any power. Once the Seer dies, she becomes the new Seer.
- **The Fake Seer** - She only has a 50% chance of guessing the target player's role. The role is given to her as Seer, so she does not know whether she is a Seer or a Fake Seer. To be used simultaneously with the actual Seer.
- **The Hunter** - He can kill another player within a few seconds of his death
- **The Flute Player** - He can charm people. His goal is to charm all other players.
- **The Little Girl** - I'm not fully sure how to adapt her from the original game, as even a glimpse of a werewolf would provide too much of an information. A few ideas, where she could get hints on who is a werewolf:
  - Once every cooldown-time, she gets a sets of letter, of which only a few are part of an actual Werewolf name
  - Once every cooldown-time, she can play Mastermind to try and guess a werewolf name (that sounds a bit too strong to me, as she could simply write an actual player name, making her a stronger Seer).
- **The Sisters** - They are both part of the Village. They know each other's role, and therefore know they can trust each other.
- **The Rusted Sword Knight** - If he is killed by a Werewolf, that werewolf is doomed and will die after some time (poison? or instant death after some time?).
- **Alpha Werewolf** - Can convert a villager to a werewolf




## Known issues

Major:
- Sometimes, the roles of everyone may be reset at random. It happened many times during Online games, but I could not reproduce it in LAN local testing games. I changed what I think causes the bug, but since I cannot reproduce it, I don't know if it is really fixed. Will have to see in future games.
  - At some point I thought it was the Wild Boy transformation that triggered it, but it also happened when the Wild Boy did not transform. I really have no idea...
  - This seems not to be a bug anymore?

Minor:
- It is possible to die before the roles distribution has happened, as it waits for all scraps to be placed. Please don't jump off the ship while it's moving, you may die.
- If someone leaves the game and does not rejoin, the roles distribution may fail (only happened once with friends, I could not reproduce the issue?). Maybe it has to do with LateCompany or MoreCompany
- I got my pop up to stop working at some point. It was only a visual bug as the roles actions were still working properly. I got it working again after rejoining the game. No idea why this happens.
- With Glitch's pack, while testing, as the only werewolf, I could not depart.
- With Glitch's pack, the roles are not distributed
- With Glitch's pack, the quota seems really really bugged


From glitch's stream
- Some incompatibility with NightOfLTheLivingMimics only in the lobby, so that's not too much of a problem. I could not reproduce the issue.
- Some mod in Glitch's pack seems to be interfering with the roles range. I cannot run the pack in local instances, only through R2ModMan. After removing some mods from the pack, I was able to run a local instance, but everything seemed fine to me, I'll have to ask someone for help testing the full pack.



## Contact

Discord: @doep

Github: DoepDopiDope
