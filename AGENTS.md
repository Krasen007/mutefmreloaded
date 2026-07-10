# mute.fm Reloaded - Development Agenda

## Current Project Status

**Build Status:** ✅ Working on Windows 11  
**Framework:** .NET Framework 4.8 (x86)  
**Executable:** `src\win\bin\Release\mute_fm.exe`

---

## What We Learned

### Project Structure
```
mutefmreloaded/
├── src/
│   ├── win/                    # Main Windows Forms application
│   │   ├── Program.cs          # Entry point, threading, initialization
│   │   ├── mutefm.csproj       # Project file
│   │   ├── UiPackage/          # UI forms and controls
│   │   │   ├── PlayerForm.cs   # Main mixer window
│   │   │   ├── WinSoundServerSysTray.cs  # System tray icon
│   │   │   ├── UiCommands.cs   # UI command handling
│   │   │   ├── KeyboardHook.cs # Global hotkey support
│   │   │   └── ...             # Other UI forms
│   │   └── ...                 # Other Windows-specific code
│   └── shared/                 # Cross-platform code
│       ├── SmartVolManagerPackage/
│       │   ├── SoundServer.cs       # Sound monitoring thread
│       │   ├── BgMusicManager.cs    # Background music state management
│       │   └── SoundSourceInfo.cs   # Audio session info tracking
│       ├── SoundServer/
│       │   └── WinCoreAudioApiSoundServer.cs  # Core Audio API wrapper
│       ├── OsIntegrationPackage/
│       │   └── Operation.cs         # Process control operations
│       └── WebServer.cs             # Internal web server for UI assets
├── lib/
│   ├── Newtonsoft.Json.Net35.dll    # JSON serialization
│   └── src/CoreAudio2_Src/          # Core Audio API wrapper source
├── build/                          # Build scripts (not needed)
└── todo/
    └── analysis_report.md           # Detailed analysis
```

### Core Functionality
1. **Audio Detection** - Uses Windows Core Audio API to monitor all audio sessions
2. **Auto-Mute Logic** - Automatically mutes background music when foreground audio is detected
3. **System Tray** - Runs in system tray with context menu for control
4. **Hotkeys** - Global hotkey support for Play/Pause/Mute/Unmute
5. **Configuration** - JSON-based config stored in `%APPDATA%\mute.fm reloaded\config.json`

### What Was Fixed (2026)
- Removed Growl notification library (outdated, website dead)
- Fixed `System.Threading.dll` reference
- Added x86 configuration to WinCoreAudioApiSoundServer project
- Removed Awesomium code (disabled via NOAWE flag)

---

## How to Build

```cmd
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /p:Configuration=Release /p:Platform=x86 src\win\mutefm.sln
```

**Output:** `src\win\bin\Release\mute_fm.exe`

---

## Future Development Plan

### Phase 1: Testing (Next)
- [ ] Test audio detection with modern browsers (Chrome, Edge, Firefox)
- [ ] Test with music players (Spotify, YouTube, etc.)
- [ ] Verify system tray functionality
- [ ] Test auto-mute/restore behavior
- [ ] Test hotkey functionality

### Phase 2: Code Cleanup
- [ ] Remove Awesomium files (InitWizForm.cs, WebBgMusicForm.cs)
- [ ] Clean up commented code blocks
- [ ] Remove dead music service defaults (Rdio, Grooveshark, MOG)
- [ ] Update default music services (add YouTube Music, Spotify Web)
- [ ] Fix warnings (unused variables, unreachable code)

### Phase 3: Feature Updates
- [ ] Update update checking to use GitHub releases
- [ ] Replace getfavicon.appspot.com with local icon extraction
- [ ] Add modern browser support
- [ ] Improve UI/UX

### Phase 4: Distribution
- [ ] Create simple distribution script (copy exe + DLLs to output folder)
- [ ] No installer needed - app runs directly!
- [ ] Optional: Create ZIP package for easy distribution

---

## Key Files to Understand

| File | Purpose |
|------|---------|
| `src/shared/SoundServer/SoundServer.cs` | Main audio monitoring loop (polls every 0.5s) |
| `src/shared/SmartVolManagerPackage/BgMusicManager.cs` | Auto-mute logic and state management |
| `src/shared/SoundServer/WinCoreAudioApiSoundServer.cs` | Core Audio API wrapper |
| `src/win/UiPackage/PlayerForm.cs` | Main UI window |
| `src/win/UiPackage/WinSoundServerSysTray.cs` | System tray icon and menu |
| `src/win/UiPackage/KeyboardHook.cs` | Global hotkey handling |
| `src/win/Program.cs` | Application entry point |

---

## Configuration

The app stores configuration in:
```
%APPDATA%\mute.fm reloaded\config.json
```

Default music services are defined in `MuteTunesConfig.cs` or loaded from config.

---

## Notes

- **No Visual Studio required** - MSBuild works fine
- **No installer required** - App runs directly
- **.NET Framework 4.8** is sufficient for Windows 11
- **x86 architecture** is required for Core Audio API compatibility