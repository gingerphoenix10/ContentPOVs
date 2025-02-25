# Changelog

All notable changes to this project will be documented in this file.
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 1.3.6
- Added the option to change how much view division is affected by more players

## 1.3.5
- Reverted the duplicate mod check, as it crashes the mod if BepInEx isn't installed

## 1.3.4
- Fixed a bug where cameras would be duplicated when going to or coming back from old world
- Added duplicate mod detection (BepInEx and Workshop both installed)

## 1.3.3
- Fixed Virality late joining
- Fixed the mod being marked as vanilla compatible on Thunderstore
- Added console commands for spawning cameras for players

## 1.3.2
- Added options for picking up / using cameras owned by dead players
- Optimised the mod a lil bit I think

## 1.3.1
- Change mod settings prefix to "[ContentPOVs] "
- Write an actual Steam Workshop description based on the Thunderstore, GitHub and Personal Site README’s

## 1.3.0
- Internal code restructure. No actual changes to the mod

## 1.2.0
- Reworked to support the latest version of Content Warning (just make sure you don't have both Thunderstore and Workshop versions installed)
- Updated README

## 1.1.1
- Automatically reopens the extractor if a video fails to extractor
- Add steam workshop shields to README

## 1.1.0
- Added score division to balance out gameplay with more cameras
- Updated README

## 1.0.6
- Switched from ConfigurableWarning to ContentSettings for better mod compatibility

## 1.0.5
- Added v1.0.4, because I might've forgotten to actually add the updated code ._.
- Fixed bug where any player could change the server's mod settings
- Fixed broken purchased cameras showing up as "?'s Broken Camera"
- Fixed GitHub actions build. Now passing
- Updated README

## 1.0.4
- Synced host's configuration with all players
- Made configuration changes have immediate effects
- Added Changelog

## 1.0.3
- Fixed cameras spawned by mods being deleted immediately
- Potential fix for cameras not spawning on clients' screens after sleeping

## 1.0.2
- Updated README for Thunderstore

## 1.0.1
- Updated README for Thunderstore

## 1.0.0
- Initial Release