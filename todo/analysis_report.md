# mute.fm Reloaded - Codebase Analysis Report

**Date:** July 10, 2026 (Updated)  
**Author:** Analysis for Windows 11 rebuild

---

## Project Overview

**mute.fm reloaded** is a Windows desktop application that automatically mutes background music when foreground audio (like videos) is detected, and restores the music when the foreground audio stops.

### Core MVP Functionality
1. Monitor all audio sessions via Windows Core Audio API
2. Detect when non-music applications are playing audio
3. Automatically fade out/mute background music player
4. Automatically restore/fade in music when foreground audio stops
5. System tray integration for control

---


## Current Build Status

### Build Tools Available
- **MSBuild:** Available via `%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe`
- **.NET Framework 4.8:** Target framework (available on Windows 11)

### Build Tools NOT Available
- **Visual Studio:** Not installed (but MSBuild works fine)
- **.NET SDK:** Not installed (not needed for .NET Framework builds)

### Build Command
```cmd
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /p:Configuration=Release /p:Platform=x86 src\win\mutefm.sln
```

Or use the provided build script:
```cmd
build.bat
```

### Build Output
- **Executable:** `src\win\bin\Release\mute_fm_reloaded.exe` (209KB)
- **Dependencies:** CoreAudioApi.dll, WinCoreAudioApiSoundServer.dll, Newtonsoft.Json.Net35.dll

---

## Architecture Analysis

### Project Structure
```
mutefmreloaded/
├── src/
│   ├── win/                    # Main Windows Forms application
│   │   ├── Program.cs          # Entry point, threading, initialization
│   │   ├── mutefm.csproj       # Project file (.NET 4.8, x86)
│   │   ├── UiPackage/          # UI forms and controls
│   │   │   ├── PlayerForm.cs   # Main mixer window
│   │   │   ├── WinSoundServerSysTray.cs  # System tray icon
│   │   │   ├── UiCommands.cs   # UI command handling
│   │   │   ├── KeyboardHook.cs # Global hotkey support
│   │   │   ├── InitWizForm.cs  # Awesomium wizard (disabled via NOAWE)
│   │   │   └── WebBgMusicForm.cs # Awesomium web control (disabled via NOAWE)
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
├── build.bat                      # Build script
└── todo/
    └── analysis_report.md           # This file
```

### Key Components

1. **SoundServer.cs** - Main monitoring loop
   - Polls audio sessions every 0.5s (configurable)
   - Detects audio activity via peak values
   - Triggers callbacks for volume/mute changes

2. **BgMusicManager.cs** - State management
   - Tracks background music player state
   - Handles auto-mute logic
   - Manages fade in/out operations

3. **WinCoreAudioApiSoundServer.cs** - Audio API layer
   - Uses CoreAudioApi for Windows audio control
   - Gets/sets per-session volume and mute
   - Monitors master volume changes

4. **PlayerForm.cs** - UI
   - Shows active audio sessions
   - Play/Pause/Mute/Unmute controls
   - System tray integration

---

## Issues and Bugs Identified

### Critical Issues

1. **Outdated/Dead References**
   - **Awesomium** - Web browser control, discontinued (code is disabled via `NOAWE` flag but files still exist)
   - **CheckForUpdates.cs** - Points to dead domain (mutefm.com)

2. **Code Quality Issues**
   - Many commented-out code blocks
   - TODO comments throughout (some from 2013-2014)
   - Inconsistent naming (MuteFm vs MuteFmReloaded)
   - Thread.Abort() usage (deprecated, can cause issues)

3. **Potential Bugs**
   - `CheckForUpdates.cs` points to dead domain (mutefm.com)
   - Awesomium files (InitWizForm.cs, WebBgMusicForm.cs) still in project but disabled

### Minor Issues

1. **UI Issues**
   - MessageBoxEx form exists but not fully integrated
   - Some UI elements have commented-out code
   - Wizard functionality disabled (was for Awesomium)

2. **Configuration Issues**
   - Default config includes "Demo music" entry (uses Windows Media Player)
   - Old URLs in default configuration (but modern services added)

- [x] Icon fetching updated to use Google's favicon service instead of getfavicon.appspot.com
- [x] Executable renamed to `mute_fm_reloaded.exe`

---

## Current Status

### Build Status: ✅ SUCCESS
The application builds successfully on Windows 11 with MSBuild.


### Files Still Present (Awesomium - disabled via NOAWE)
- `src/win/UiPackage/InitWizForm.cs` - Still exists but disabled
- `src/win/UiPackage/WebBgMusicForm.cs` - Still exists but disabled

---

## Action Plan

### Phase 1: Testing (Next)
- [ ] Test audio detection with modern browsers (Chrome, Edge, Firefox)
- [ ] Test with music players (Spotify, YouTube, etc.)
- [ ] Verify system tray functionality
- [ ] Test auto-mute/restore behavior
- [ ] Test hotkey functionality

### Phase 2: Code Cleanup (Medium Priority)
- [ ] Remove Awesomium files (InitWizForm.cs, WebBgMusicForm.cs)
- [ ] Clean up commented code blocks
- [ ] Fix potential null reference issues

### Phase 3: Feature Updates (Low Priority)
- [ ] Update update checking to use GitHub releases
- [ ] Improve UI/UX

---

## Phase 2 Implementation Plan: Code Cleanup

### Task 1: Remove Awesomium Files

**Files to Delete:**
- `src/win/UiPackage/InitWizForm.cs` (108 lines)
- `src/win/UiPackage/InitWizForm.Designer.cs`
- `src/win/UiPackage/InitWizForm.resx`
- `src/win/UiPackage/WebBgMusicForm.cs` (352 lines)
- `src/win/UiPackage/WebBgMusicForm.Designer.cs`
- `src/win/UiPackage/WebBgMusicForm.resx`
- `src/win/UiPackage/WebBgMusicForm.af-ZA.resx`

**Files to Update:**
- `src/win/mutefm.csproj` - Remove Compile and EmbeddedResource entries for InitWizForm and WebBgMusicForm
- `src/win/UiPackage/UiCommands.cs` - Remove all `#if !NOAWE` blocks and related code (lines 12-24, 104-120, 266-320, 775-800)

**Impact:**
- These files are already disabled via `#if !NOAWE` conditional compilation
- Removing them will clean up ~500+ lines of dead code
- No runtime impact since NOAWE is defined in the build configuration

### Task 2: Update CheckForUpdates.cs

**Current State:**
- Points to `http://www.mutefm.com` (dead domain)
- Uses `Constants.MuteFmDomain` which returns "www.mutefm.com"

**Changes Required:**
- Update `Constants.MuteFmDomain` to use GitHub API
- Or create a new update checking mechanism using GitHub releases API
- URL: `https://api.github.com/repos/Krasen007/mutefmreloaded/releases/latest`

**Implementation Options:**
1. **Simple approach:** Update to use GitHub releases URL directly
2. **Better approach:** Use GitHub API to check for latest release and compare versions

### Task 3: Clean Up Commented Code Blocks

**Files with significant commented code:**
- `src/win/Program.cs` - Large blocks of commented code (lines 62-67, 177-194, 243-267, etc.)
- `src/win/UiPackage/UiCommands.cs` - Multiple commented sections
- `src/shared/SmartVolManagerPackage/BgMusicManager.cs` - Likely has commented code
- `src/win/MuteTunesConfig.cs` - Commented-out music service configurations

**Approach:**
- Remove large blocks of commented code that are no longer relevant
- Keep essential TODOs that represent valid future work
- Focus on code that's been commented for years (2013-2014 era)

### Task 4: Fix Thread.Abort() Usage

**Location:** `src/win/UiPackage/UiCommands.cs` line 645

**Current Code:**
```csharp
if (Program.SoundServerThread != null)
    Program.SoundServerThread.Abort();
```

**Recommended Fix:**
- Use a cancellation token pattern instead of Thread.Abort()
- Add a volatile boolean flag to signal thread termination
- The SoundServer thread already has a loop that could be modified to check for cancellation

### Task 5: Clean Up TODO Comments

**Files with old TODOs:**
- `src/win/Program.cs` - Multiple TODOs from 2013-2014
- `src/shared/SoundServer/SoundServer.cs` - Likely has TODOs
- Various UI files

**Approach:**
- Review each TODO and either:
  - Remove if no longer relevant
  - Keep if it represents valid future work
  - Update with current context if needed

---

### Phase 4: Distribution (Low Priority)
- [ ] Create simple distribution script (copy exe + DLLs to output folder)
- [ ] No installer needed - app runs directly!
- [ ] Optional: Create ZIP package for easy distribution

---

## Files to Focus On

### Critical for MVP
1. `src/shared/SoundServer/WinCoreAudioApiSoundServer.cs` - Audio detection
2. `src/shared/SmartVolManagerPackage/SoundServer.cs` - Monitoring loop
3. `src/shared/SmartVolManagerPackage/BgMusicManager.cs` - Auto-mute logic
4. `src/win/Program.cs` - Entry point
5. `src/win/UiPackage/PlayerForm.cs` - Main UI
6. `src/win/UiPackage/WinSoundServerSysTray.cs` - System tray

### To Remove/Clean
1. ~~`src/win/GrowlInstallHelper.cs` - Remove entirely~~ - **DONE**
2. ~~`lib/Growl.Connector.dll` - Remove~~ - **DONE**
3. ~~`lib/Growl.CoreLibrary.dll` - Remove~~ - **DONE**
4. `src/win/UiPackage/InitWizForm.cs` - Remove (Awesomium, disabled)
5. `src/win/UiPackage/WebBgMusicForm.cs` - Remove (Awesomium, disabled)

---

## How to Run

The application can be run directly without an installer:
```
src\win\bin\Release\mute_fm_reloaded.exe
```

---

## Summary

**BUILD STATUS: SUCCESS** - The application now builds successfully on Windows 11!

### What Was Fixed
1. **Build issues resolved:**
   - Fixed `System.Threading.dll` reference (now uses built-in .NET 4.8)
   - Removed all Growl references and code
   - Added x86 configuration to WinCoreAudioApiSoundServer project
   - Fixed solution file configuration mappings

2. **Files cleaned:**
   - Deleted `lib/Growl.Connector.dll`
   - Deleted `lib/Growl.CoreLibrary.dll`
   - Removed `GrowlInstallHelper.cs` references

3. **Modern updates:**
   - Added Spotify Web and YouTube Music to default config
   - Updated icon fetching to use Google's favicon service
   - Renamed executable to `mute_fm_reloaded.exe`

### Next Steps
1. Test the application on Windows 11 with modern browsers (Chrome, Edge, Firefox)
2. Test with music players (Spotify, YouTube, etc.)
3. Clean up remaining Awesomium code (InitWizForm.cs, WebBgMusicForm.cs)
4. Update update checking to use GitHub releases

**Recommendation:** The .NET Framework 4.8 approach is working well. No migration needed unless you want modern features. The project is in a reasonably good state for a Windows 11 rebuild.