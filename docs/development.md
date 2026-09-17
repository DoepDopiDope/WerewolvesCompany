# Development

## Prerequisites

- Visual Studio 2022 or a compatible .NET SDK.
- A Lethal Company installation with BepInEx and the dependencies listed in `manifest.json` installed.
- The `netcode-patch` CLI 4.5.0 or newer (the project targets Unity `2022.3.62`, Netcode `1.12.2`, and Unity Transport `1.0.0`) and 7-Zip for packaging.

The version passed to `netcode-patch` must match the Netcode for GameObjects version embedded in the supported Lethal Company build. A mismatch can generate RPC send stubs and receive registrations for different internal handler tables, causing `KeyNotFoundException` failures when an RPC is invoked.

`WerewolvesCompany/WerewolvesCompany.csproj` defaults `LethalCompanyDir` to the author's local Steam path. Override that MSBuild property when the game is elsewhere. References are loaded directly from the game's `Lethal Company_Data/Managed` and `BepInEx` directories; this is not a self-contained NuGet build.

## Build and validate

Use:

```powershell
dotnet build WerewolvesCompany.sln --no-restore -p:RunPostBuildEvent=Never
```

Add `-p:LethalCompanyDir="X:\path\to\Lethal Company"` when needed. The build invokes `netcode-patch`; ordinary builds do not package when `PackageMod` is unset/false.

There is no automated test project. For behavior changes, manually test at least host and one client, round transitions, reconnect/disconnect, and the affected role/RPC/HUD path. For documentation-only changes, verify links and commands instead of building game code.

## Change checklist

- Keep server-authoritative decisions and targeted RPC responses intact.
- Pair config changes between `ConfigParameters` bindings and `ConfigManager` network variables/copy logic.
- Pair input changes between `InputsKeybinds`, `KeybindsLogic`, and callback registration in `RolesManager`.
- Pair UI lifecycle changes with scene, round-start/end, death/spectate, and disconnect handling.
- Update `README.md` for player-visible behavior and `CHANGELOG.md` for release-facing changes.
- Do not commit `bin/`, `obj/`, local game files, or rebuilt release artifacts unless explicitly required.

`performance_tests/` and `quota_theory/` are offline analysis utilities/data, not runtime code or an automated test suite.
