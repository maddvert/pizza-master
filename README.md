# Food Street — playable Unity prototype

Original 3D restaurant game prototype for Android and iOS, built in Unity C#.

## Art direction

Aim for a sophisticated night-market restaurant: charcoal stone, walnut cabinetry, brass hardware, warm practical lights, adult staff and customers, and a restrained dark UI. The current scene generates a first pass of these shapes in code. The final visual target requires original textured 3D models, character rigs and animation, baked lighting, authored UI, and device profiling. The concept render is art direction only and is not a screenshot or in-game asset.

## Play in the Editor

1. Install Unity Hub and Unity **2022.3 LTS** with Android Build Support. For iPhone builds, also use a Mac with Xcode and Unity iOS Build Support.
2. Open this folder as a Unity project. On first opening, let packages import.
3. In the top menu choose **Food Street > Create Playable Scene**; then press Play.
4. Use WASD or arrow keys in the Editor. On a phone, drag a finger on the left half of the screen.
5. Walk to the pink counter to cook pizza, the gold counter to serve waiting customers, and the green pad to buy a tray upgrade.

## Build

In File > Build Settings select Android, Switch Platform, then Build (APK for direct testing; AAB for Play Store later). For iPhone select iOS, Switch Platform, Build, then open the generated Xcode project on a Mac and sign it with your Apple developer team. Add the generated FoodStreet scene to Scenes In Build if Unity does not list it automatically.

For automation, `BuildGame.Android` creates the scene and builds a debug-signed APK. The supplied GitHub Actions workflow requires an activated Unity license in repository secrets before it can build. `BuildGame.Ios` creates an Xcode project; an installable iPhone app requires Apple signing and a macOS/Xcode build.

## Prototype status

This is a first playable mechanics build with procedural placeholder art, **not** the finished high-graphics game. It does not contain ads, purchases, analytics, a store listing, or completed Android/iOS device testing. Ads should be integrated with test ad unit IDs first, after the core gameplay has been tested. Replace placeholder art with licensed original models, animations, textures, lighting and UI for the final visual target.
