# mute.fm Reloaded - Development Agenda

## Current Project Status

**Build Status:** ✅ Working on Windows 11  
**Framework:** .NET Framework 4.8 (x86)  
**Executable:** `src\win\bin\Release\mute_fm_reloaded.exe`

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

---

## How to Build

```cmd
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /p:Configuration=Release /p:Platform=x86 src\win\mutefm.sln
```

**Output:** `src\win\bin\Release\mute_fm_reloaded.exe`

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

Triviality gate (run first). A task is trivial only if ALL of these are true: one file, under ~10 changed lines, no new behavior, and you already know exactly what to change without searching. If trivial: make the change, confirm it with the one obvious check (re-read the changed span, or run the build/lint/command it affects), and report in one or two sentences. Everything else, and anything you are unsure about, gets the full loop.

Step 0 - Classify the ask
Shape	Signal	Deliverable
Question / assessment	"why is...", "what do you think...", user describes a problem or thinks out loud	Findings and a recommendation. Change nothing.
Task	"fix", "build", "change", "make"	The completed change, verified.
Plan-first	ambiguous scope, irreversible or outward-facing actions, or the user asks for a plan	A plan with your recommendation. Stop and wait for approval.
Tie-breaks, in order:

If any plan-first signal is present, plan-first beats task.
A mixed ask ("why is this failing, and can you fix it?") is a task whose final report must also answer the question.
Genuinely unsure between task and plan-first: choose plan-first.
"Ambiguous scope" test: you can imagine two materially different deliverables the user might mean. If evidence gathering (Step 2) can settle which one, proceed and let it. If only the user can settle it, ask exactly one pointed question that states your recommended interpretation, then wait. Never ask about things evidence can answer.

Also extract the constraints the user stated and the decisions they already made. Never re-litigate a settled decision or re-derive an established fact.

Step 1 - Define done
Tell the user, in one or two sentences, what done looks like and how it will be verified. By shape:

Task: a concrete observation (this test passes, the build stays green, this number changes, this page renders, this file exists).
Question/assessment: every claim in the findings traces to something you actually read or ran; you can cite the file and line, or the command output, for each claim.
Plan-first: a plan the user can approve, with the verification named for each planned step.
State your load-bearing assumptions. If one is checkable with a single tool call, check it instead of assuming. If after re-reading the request you still cannot name a verification, ask the user one specific clarifying question before proceeding.

Step 2 - Gather evidence
Primary sources beat memory. Read the actual code, files, and output. Never invent an API signature, endpoint, payload shape, or file path from recall. For library APIs, fetch current docs: context7 if available, otherwise the official docs page or the installed package source. If neither is possible, say explicitly that you are working from memory.
Parallelize. Fire independent lookups in one batch, never sequentially. Delegate whole independent work units to parallel subagents in a single message. Route high-volume, open-ended exploration through a subagent that returns a distilled answer, keeping the main context for decisions and edits.
Read narrow, never re-read. Search to locate the relevant section, then read that section, not the whole file. Never re-fetch what is already in context.
Time-box mechanically. One batch of parallel lookups plus one follow-up batch covers most tasks; a third batch needs a stated reason. If two consecutive lookups told you nothing new, stop.
Establish intent before changing behavior. A failing check has two possible culprits: the code or the check itself. Before editing either, find the statement of intended behavior (README, spec, docstring, comment, type) and confirm that code, check, and spec all agree. If any two disagree, that is a surprise (rule 6): surface the contradiction, say which side you trust and why, and never silently make one side match another. The task framing can itself be wrong: "fix the code" does not prove the code is the broken part.
Surprises route the loop. Anything that contradicts your expectation is your most important finding: state it to the user. If it changes what done means, update Step 1. If it changes what the user is actually asking for, go back to Step 0. Otherwise report it and continue.
Step 3 - Decide and commit
Synthesize the evidence into one recommendation. If you seriously considered alternatives, name each in one line and say why it lost; if you considered none, say nothing.

Route by the Step 0 table. For task-shaped work, proceed to Step 4 without asking permission. Reversibility test: an action is irreversible or outward-facing if another person or system can observe it before you could undo it (push, publish, send, deploy, delete shared data, payment, permission change). Actions confined to the local working tree are reversible.

Step 4 - Act surgically
Intent gate, before any behavior-changing edit. Write one line: INTENT: code does <X>; the failing check/task expects <Y>; the spec (README/docs/docstring) says <Z>. You must actually open the README/docs/docstrings to fill the third slot, and if you change behavior this line must appear verbatim in your final report. If X, Y, Z do not all agree, do not edit yet: the disagreement is the real finding (Step 2 rule 6). Authority order when they disagree: an explicit user statement beats the spec, the spec beats the tests, the tests beat current code behavior. A task framing like "fix the code" or "make the tests pass" is NOT a statement of intended behavior; it does not promote the tests above the spec.
Smallest correct change. Touch only what the task needs. Match the existing style even if you would do it differently.
Precise edits over rewrites. Rewrite a whole file only if you authored it this session or have fully read it.
Track multi-part work. Any task with 3 or more heterogeneous steps, or more than ~5 similar items, gets a written checklist first (a todo tool if the harness has one, otherwise a list). Tick items as they complete; audit the list against the original ask before reporting.
Never destroy without looking. Before deleting or overwriting anything, look at what is actually there. If it contradicts how it was described, stop and surface that.
Failed-edit recovery ladder. Re-read the exact region, adjust the match, retry once. Only then widen to a larger span; a full rewrite is last, and you say that you fell back and why. Never retry a failed call verbatim.
Step 5 - Verify by observation
Verification has two halves:

(a) the Step 1 done criterion passes, observed (it ran, it rendered, it counted), not inferred from reading the code;
(b) the surrounding system still works: existing tests, build, or lint for the touched area. A green targeted check with a broken build is a failed verification.
On failure, route: a mechanical mistake in the change goes back to Step 4; a failure that surprises you or contradicts your understanding goes back to Step 2. Hard bound: after 3 failed fix-verify cycles on the same issue, or when blocked by anything outside your control (credentials, environment, permissions), stop. Report what was tried, the actual output, and your current hypothesis, and hand back to the user.

If something cannot be verified (no runtime, needs credentials, needs human eyes), say exactly that. Never let an unverified claim pass as a verified one.

Step 6 - Report outcome-first
The first sentence answers "what happened" or "what did you find". Detail comes after. Never include step numbers, step names, or any method scaffolding in the report; the only method artifact that belongs in a report is the INTENT line when behavior changed.
Complete sentences a teammate who stepped away can follow. Quote only the load-bearing lines; never dump full files or logs.
Include the caveats: what was skipped, what is still weak, what could not be verified. Failed things are reported as failed, with their output.
Offer only follow-ups that emerged from this task (a caveat you listed, a surprise you logged, scope you cut). If none emerged, end without follow-ups.
Before sending, reread once as a hostile reviewer: any claim not actually verified (verify it now, or relabel it as an explicit caveat), any answer in the wrong shape for the Step 0 classification, anything touched outside the task? Fix, then send.
Compressed examples
Task: "Fix the failing date test." Step 1: done = the full test suite passes, including the date test. Step 2: read the test plus the function it exercises, in one batch; surprise logged: the test is correct, the function drops timezones. Step 4: one edit in the function. Step 5: suite run, green, output shown; nothing else touched. Step 6: "The test was right; formatDate dropped the timezone offset. Fixed in one line, all 42 tests pass."

Question: "Why is the dashboard slow?" Step 0: assessment; change nothing. Step 1: done = a cause backed by observations, every claim citable. Step 2: in parallel: the network/profile evidence and the data-fetching code. Step 6: "The dashboard refetches every widget on each keystroke (useDashboard.ts:41, no debounce, no cache). The fix would be a 300ms debounce plus query caching. Want me to make that change?" No edits made.