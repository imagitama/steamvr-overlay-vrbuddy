# SteamVR Overlay VRBuddy

A SteamVR app for displaying the position of your buddy's head and hands in your playspace. Useful for co-op VR games like flight simulators, tanks, etc. that don't render a mesh for your friends.

<img src="screenshot.png" width="500px">

# Usage

Run `VRBuddy.exe` and follow the prompts.

You must port forward and copy your public IP to give it to your friend. Use 127.0.0.1 to see yourself locally.

Settings are saved to `settings.json`.

Change `texture_head.png` etc. to anything you like :)

# Development

```cli
dotnet restore
dotnet run
```

OpenVR SDK is downloaded from their GitHub: https://github.com/ValveSoftware/openvr

# Building

```cli
.\build.ps1
```

# Issues

- overlays sometime take 10 seconds+ to even render (might need to wait for something to initialize?)
- facing your head doesn't work sometimes

# Ideas

- use a SteamVR dashboard overlay to configure offsets/settings
- use [Steamworks](https://github.com/rlabrecque/Steamworks.NET) to handle authentication/P2P
- adjust scale and transparency of head and hands
- only render overlays when looking at them
