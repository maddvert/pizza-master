# Pizza Master — original 3D restaurant game prototype

Unity C# project for Android and iOS. The inspiration is the genre of light, smooth idle restaurant management: make pizzas, serve a queue, earn money, hire a helper and upgrade equipment. This project uses original code, title and generated placeholder geometry. It does not include assets, levels, UI, text, music or source code from Pizza Ready!.

## Current playable mechanics

- Drag to move the owner around a bright 3D restaurant (WASD in the Editor).
- Stand at the red oven pad to bake pizzas and fill the carrying tray.
- Take pizza to the blue service pad to serve customers and earn cash.
- Visit gold, green and red upgrade pads to expand tray capacity, hire an automatic serving helper and speed up cooking.
- Queue length, customer patience, shop levels and offline staff earnings.
- Save cash, upgrades and progress locally between sessions.

## Open and build

Install Unity 2022.3 LTS with Android Build Support. Open the repository as a Unity project. Choose **Pizza Master > Create Playable Scene**, then press Play. The `BuildGame.Android` command creates the scene and builds a debug APK when Android support is installed. The manual GitHub Action needs a Unity license activated in repository secrets. `BuildGame.Ios` exports an Xcode project; installing on an iPhone needs a Mac, Xcode and Apple signing.

## Status

This is a mechanics prototype with generated geometry, not a tested APK/IPA or finished game. It needs authored original 3D characters, food models, animations, interface, sound, device optimization, ads integration and on-device testing. No ad SDK, payments or store release is included yet.
