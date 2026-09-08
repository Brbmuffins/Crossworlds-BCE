# Crossworlds Direct Launcher

This launcher is exclusively for website/direct distribution. It installs and
updates the Windows client from:

`https://playcrossworlds.com/downloads/updates/win-x64`

The packaged Unity player lives under `Game/` and is launched with
`--distribution=direct`. A future Steam depot should package the Unity player
directly and launch it with `--distribution=steam`; Steam then owns all file
installation and patching, and Velopack is not included in that depot.

## Build a direct release

1. Build the Windows Unity client to `build/WindowsClient`.
2. Run `tools/build-windows-installer.ps1 -Version 1.1.0`.
3. Test the generated installer and upgrade from the previous version.
4. Publish the contents of `build/DirectRelease/releases` together. Never upload
   only one package because the release feed and delta chain must stay consistent.

Production releases should be Authenticode-signed before public distribution.
