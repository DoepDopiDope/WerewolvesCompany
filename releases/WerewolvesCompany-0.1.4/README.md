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

The Werefolf has the ability to kill another player (with cooldown).

### Villager

The Villager shall find and kill the Werewolves before ship departure.

The Villager can *patpat* others players.

### Witch

The Witch shall find and kill the Werewolves before ship departure.

The Witch has two potions, and can do two things:
- Poison another player and kill him (once per round)
- Protect another player and make him immune **once** to a Werewolf attack (once per round). The immune player won't know he has been immunized, nor will he know he loses his immune status

Note that unlike the original Werewolves game, the Witch here does not revive a dead player. This change was made so that a killed-player cannot instantly reveal the Werewolf(ves) identity.

### Seer

The Witch shall find and kill the Werewolves before ship departure.

The Seer can seer another player's role (once per round).

### Wild Boy

The Wild Boy wins either with the Villagers or the Werewolves, depending on its status.

The Wild Boy can target a player who becomes his idol. If the idol dies, the Wild Boy becomes a Werewolf. As long as his idol is alive, he wins with the Villagers.

## Setup

You can edit the roles configuration from the Ship's terminal. You can access the configuration menu by typing 'werewolves'.

Once in the Werewolves menu, you can add or remove roles from the current setup by typing :
- werewolves add Role_Name
- werewolves del Role_Name

## KeyBinds

- **[K]** Perform main action (Kill, Poison, Seer, Idolize)
- **[L]** Perform secondary action (Make Immune)
- **[M]** Display my role ToolTip
- **[P]** (hold for 5s) Distribute roles. Only exists in case the roles fail to be distributed at the beginning of the round

## TODO List

Theses features are not implemented yet, but are planned:
- Icon for the roles to be displayed at the top of the screen
- End of game screen displaying which team won
- More roles (planned: Cupid, Apprentice Seer)

## Contact

Discord: @doep

Github: DoepDopiDope