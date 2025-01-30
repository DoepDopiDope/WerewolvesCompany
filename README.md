# WerewolvesCompany

Adds the Werewolves Game to Lethal Company.

#### For requests, contact me on Discord (doep) or on [github](https://github.com/DoepDopiDope/WerewolvesCompany)

## General information

Werewolves is a party game where Villagers must find one (or multiple) Werewolves within the village. At night, the Werewolves may vote and kill a Villager. When Villagers wake up, they take a note note of who was killed. They may vote and eliminate who they think may be a Werewolf.

WerewolvesCompany brings a variant of this game to Lethal Company. Each round, players are assigned a random role:
- If they're part of the village, they must eliminate the Werewolve(s) before ship departure,
- If they're a Werewolf, they must kill the Villagers before ship departure.

The roles are currently filled as follows (Werewolf, Witch, Seer, Wild Boy, Villager, ...). Remaining slots are filled with Villagers. The roles are shuffled every round.

It is not currently possible to setup which roles should be present in the game. This feature will be added in the next(s) update.

Your role is permanently displayed at the top of your screen. You can also bring the tooltip for your role by pressing **[M]**

## Roles

### Werewolf

The Werewolf shall kill other players before ship departure.

The Werewolf has the ability to kill another player (with cooldown).

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

The Witch shall find and kill the Werewolves before ship departure.

The Seer can seer another player's role (once per round).

### Wild Boy

The Wild Boy wins either with the Villagers or the Werewolves, depending on its status.

The Wild Boy can target a player who becomes his idol. If the idol dies, the Wild Boy becomes a Werewolf. As long as his idol is alive, he wins with the Villagers.

## Setup and parameters

You can edit the roles configuration from the Ship's terminal. You can access the configuration menu by typing 'werewolves'.

Once in the Werewolves menu, you can add or remove roles from the current setup by typing :
- werewolves add Role_Name
- werewolves del Role_Name

You can also shorten 'werewolves' with its alias 'wc'.

## KeyBinds

- **[W]** Perform main action (Kill, Poison, Seer, Idolize)
- **[V]** Perform secondary action (Make Immune)
- **[M]** Display my role ToolTip
- **[P]** (hold for 5s) Distribute roles. Only exists in case the roles fail to be distributed at the beginning of the round

## Debug

While they are mainly for my personal use during playtests, you can use debug commands from the terminal (mainly for my personal use when testing changes)
- wc debug         -> show available debug commands
- wc debug cd      -> set all cooldowns to 0
- wc debug distrib -> distribute roles. Alternative to the hold-P (5s) keybind
- wc debug reset   -> reset state of every player current role to its initial state

## Planned updates

Theses features are not implemented yet, but are planned:
- Permit adding/deleting a selected amount of roles: 'wc add werewolf 5' or 'wc del werewolf 5'
- Disable pop-up when a body is placed in the ship. During playtests, this led to many times where this would reveal the werewolf.
- When spectating, displaying the role of the spectated player
- Change the Witch poison to actually poison the target. The poisoned player would start losing HP (after some time, to avoid revealing the Witch identity)
- Icon for the roles to be displayed at the top of the screen
- End of game screen displaying which team won
- More roles, a few ideas below, they may not all be released:
  - **Cupid** - Selects two players who become lovers. If one of them die, the other one also dies. They must win together, whether they initially were in the same team or not.
  - **The Apprentice Seer** - As long as the Seer is alive, she does not have any power. Once the Seer dies, she becomes the new Seer.
  - **The Fake Seer** - She only has a 50% chance of guessing the target player's role. The role is given to her as Seer, so she does not know whether she is a Seer or a Fake Seer. To be used simultaneously with the actual Seer.
  - **The Hunter** - He can kill another player within a few seconds of his death
  - **The Flute Player** - He can charm people. His goal is to charm all other players.
  - **The Drunken Man** - He is immune to the Witch poison.
  - **The Little Girl** - I'm not fully sure how to adapt her from the original game, as even a glimpse of a werewolf would provide too much of an information. A few ideas, where she could get hints on who is a werewolf:
	- Once every cooldown-time, she gets a sets of letter, of which only a few are part of an actual Werewolf name
	- Once every cooldown-time, she can play Mastermind to try and guess a werewolf name (that sounds a bit too strong to me, as she could simply write an actual player name, making her a stronger Seer).
  - **The Sisters** - They are both part of the Village. They know each other's role, and therefore know they can trust each other.
  - **The Rusted Sword Knight** - If he is killed by a Werewolf, that werewolf is doomed and will die after some time (poison? or instant death after some time?).

## Known issues

Major:
- No major bugs have been reported.

Minor:
- No minor bugs have been reported.

## Contact

Discord: @doep

Github: DoepDopiDope