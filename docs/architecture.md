# Architecture

WerewolvesCompany is a BepInEx/Harmony mod targeting .NET Standard 2.1. Unity Netcode synchronizes roles, configuration, voting, quota state, and role actions.

## Runtime lifecycle

1. `Plugin.Awake` initializes logging/config/input support, loads `netcodemod`, attaches components to the `RolesManager` and `ConfigManager` prefabs, applies Harmony patches, and creates the persistent `ModManager`.
2. `GameNetworkManagerPatcher` registers both network prefabs.
3. `StartOfRoundPatcher` has the host spawn `ConfigManager` before `RolesManager`; roles read synchronized configuration during initialization.
4. `ModManager` creates the HUD and quota manager when the gameplay scene loads.
5. `RoundManagerPatcher` distributes roles and computes daily quota once scrap values synchronize. Round-start/end patches reset transient role and HUD state.

Preserve initialization order and null guards. Manager singletons assign both their static `Instance` and the matching `Plugin.Instance` field, then clear both in `OnDestroy`.

## Authority and synchronization

- The host/server owns role generation, vote resolution, quota calculation/counting, and synchronized config values.
- Client requests generally enter through `[ServerRpc(RequireOwnership = false)]`; server results and targeted notifications use `ClientRpcParams`.
- Use `Utils.BuildClientRpcParams` for a one-client response and player `OwnerClientId` as the stable role/vote key.
- Role-action results intentionally retain the original friends-only trust model: affected clients may acknowledge outcomes. Voting, death deduplication, quota changes, role setup, and administrative commands are still validated by the server to prevent accidental desynchronization.
- Quota scrap notifications are counted once by the host and the resulting value is broadcast to every client. Do not increment `QuotaManager.currentScrapValue` independently on peers.
- Network-prefab component or RPC changes may require rebuilding the `netcodemod` asset bundle and running the netcode patcher.

## Source map

| Area | Source |
| --- | --- |
| Bootstrap, asset bundle, scene managers | `WerewolvesCompany/Plugin.cs` |
| Role model, cooldowns, targeting, concrete roles | `WerewolvesCompany/Roles.cs` |
| Distribution, voting, RPCs, role actions | `WerewolvesCompany/Managers/RolesManager.cs` |
| Host config bindings and synchronized values | `WerewolvesCompany/Config/ConfigParameters.cs`, `ConfigManager.cs` |
| Quota state/calculation | `WerewolvesCompany/Managers/QuotaManager.cs` |
| HUD and voting UI | `WerewolvesCompany/UI/RoleHUD.cs` |
| Key declarations and callbacks | `WerewolvesCompany/Inputs/` |
| Hooks into Lethal Company | `WerewolvesCompany/Patches/` |
| Terminal commands and role setup | `WerewolvesCompany/Patches/TerminalPatcher.cs` |
| Coroner integration | `WerewolvesCompany/CustomDeaths.cs`, `Strings_en-us_doep-wc.xml` |

## Adding or changing a role

Update the concrete `Role` subclass, `References.RoleFactories`, and `References.RoleOrder` in `Roles.cs`. Every serialized `refInt` must remain unique and stable. Factories must return a fresh instance because cooldown and interaction state are player-specific. Then check role generation/distribution and action RPCs in `RolesManager`, terminal setup/help, config bindings plus synchronized fields, HUD/keybind behavior, and the user-facing `README.md`.
