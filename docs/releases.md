# Releases

## Version sources

Keep these in sync for a release:

- `Plugin.VERSION` in `WerewolvesCompany/Plugin.cs`.
- `version_number` in root `manifest.json`.
- The newest heading and notes in root `CHANGELOG.md`.

The root `README.md`, `LICENSE`, `icon.png`, `manifest.json`, `CHANGELOG.md`, compiled DLL, `netcodemod`, and Coroner strings form the distributable package.

## Packaging

Building with `-p:PackageMod=true` runs the `PostBuild` target in `WerewolvesCompany.csproj`. It updates `releases/latest/`, copies its BepInEx contents into the configured game installation, deletes/recreates `releases/latest/latest.zip`, and requires `7z` on `PATH`.

Because packaging mutates tracked binaries and a local game installation, do not enable it for ordinary verification. Inspect the exact `PostBuild` commands before changing the release layout.

Versioned directories under `releases/WerewolvesCompany-*` are historical snapshots. Do not retrofit source fixes into them. Prepare `releases/latest`, test that package in-game, then create a versioned snapshot only as an explicit release task.
