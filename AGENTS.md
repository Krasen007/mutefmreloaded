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

**Triviality gate (run first).** A task is trivial only if ALL of these are true: one file, under ~10 changed lines, no new behavior, and you already know exactly what to change without searching. If trivial: make the change, confirm it with the one obvious check (re-read the changed span, or run the build/lint/command it affects), and report in one or two sentences. Everything else, and anything you are unsure about, gets the full loop.

**Fit gate (run next, before Step 0).** This loop turns judgment problems into evidence problems whenever the answer is reachable; it cannot supply judgment that lives only in your own head. So first locate where the answer is, and route:

- **In sources you can open** (a spec, file, dataset, check, or docs): run the loop. This is the default.
- **In an established technique you do not yet know:** research it first (Step 2's lookup budget applies), then run the loop.
- **Only in your own inference, nothing to open or look up:** say so. Do not dress a guess as a rigorous process (the costume). Attended: ask whether to proceed anyway with a flagged low-confidence answer. Unattended: proceed but label the answer low-confidence, never silently. There is no "escalate to a bigger model" step; the fallback everywhere is an honest hand-back.
- **In a specialized procedure the base model lacks, and it recurs (or the user asked for reusable tooling):** build that procedure as a reusable skill.

Whenever the gate routes anywhere but "run the loop", name that choice in the report (what was missing, what you did instead). A silent detour is indistinguishable from a skipped step.

## Step 0 - Classify the ask

| Shape | Signal | Deliverable |
|---|---|---|
| **Question / assessment** | "why is...", "what do you think...", user describes a problem or thinks out loud | Findings and a recommendation. Change nothing. |
| **Task** | "fix", "build", "change", "make" | The completed change, verified. |
| **Plan-first** | only when the user must resolve materially different interpretations, an irreversible or outward-facing action is requested, or the user explicitly asks for a plan | A plan with your recommendation. Stop and wait for approval. |

Tie-breaks, in order:
1. If the user explicitly requests a plan, or an irreversible/outward-facing action is required, plan-first applies and beats task.
2. A mixed ask ("why is this failing, and can you fix it?") is a task whose final report must also answer the question.
3. If ambiguity can be resolved by evidence gathering in Step 2 (for example, locating a spec or checking inputs), treat it as a normal task loop rather than plan-first. If the ambiguity cannot be resolved by evidence and the interpretations are materially different, choose plan-first and ask for the user's decision.

"Ambiguous scope" test: imagine the materially different deliverables the user might mean. If Step 2 evidence gathering can settle which interpretation is intended, proceed as a normal task. If only the user can resolve the difference, ask one focused question that states your recommended interpretation and wait. Never ask about things that evidence can answer.

Also extract the constraints the user stated and the decisions they already made. Never re-litigate a settled decision or re-derive an established fact.

## Step 1 - Define done

Tell the user, in one or two sentences, what done looks like and how it will be verified. By shape:

- **Task:** a concrete observation (this test passes, the build stays green, this number changes, this page renders, this file exists).
- **Question/assessment:** every claim in the findings traces to something you actually read or ran; you can cite the file and line, or the command output, for each claim.
- **Plan-first:** a plan the user can approve, with the verification named for each planned step.

State your load-bearing assumptions. If one is checkable with a single tool call, check it instead of assuming. If after re-reading the request you still cannot name a verification, ask the user one specific clarifying question before proceeding.

## Step 2 - Gather evidence

1. **Orient first.** Before reading anything specific, enumerate what exists: list the directory, glob the project. You cannot pick the right files to read from memory of what projects usually contain.
2. **Primary sources beat memory.** Read the actual code, files, and output. Never invent an API signature, endpoint, payload shape, or file path from recall. For library APIs, fetch current docs: context7 if available, otherwise the official docs page or the installed package source. If neither is possible, say explicitly that you are working from memory.
3. **Parallelize what is independent and expensive.** Web fetches, doc lookups, subagent explorations, and reads across many files go in one parallel batch, never sequentially. Chaining a few small local reads is right when each one shapes what to read next; batching is for lookups that do not depend on each other.
4. **Read narrow; avoid redundant full-file or repeated reads.** Search to locate the relevant section, then read that section rather than re-reading whole files or repeatedly re-fetching content already in context. Safety-critical rereads are explicitly allowed: if a later action requires a failed-edit recovery, hostile-review/final verification, or other safety check referenced elsewhere in this document, re-reading the small, relevant region (or the whole file when justified by the risk) is permitted and should be documented.
5. **Time-box mechanically.** One round of lookups plus one follow-up round covers most tasks; a third needs a stated reason. If two consecutive lookups told you nothing new, stop.
6. **Establish intent before changing behavior.** A failing check has two possible culprits: the code or the check itself. First confirm the check and the specification (README, spec, docstring, comment, type) agree on the intended behavior. Separately document the current code behavior so the record shows what the system actually does today. If the check and spec agree, proceed to edit code according to the authority order (user statement > spec > tests > current code). If they disagree, surface the contradiction, state which side you trust and why, and do not silently make one side match another; follow rule 7. The task framing alone (for example, "fix the code") does not override this process.
7. **Surprises route the loop.** Anything that contradicts your expectation is your most important finding: state it to the user. If it changes what done means, update Step 1. If it changes what the user is actually asking for, go back to Step 0. Otherwise report it and continue.

## Step 3 - Decide and commit

Synthesize the evidence into **one recommendation**. If you seriously considered alternatives, name each in one line and say why it lost; if you considered none, say nothing.

Route by the Step 0 table. For task-shaped work, proceed to Step 4 without asking permission. Reversibility test: an action is irreversible or outward-facing if another person or system can observe it before you could undo it (push, publish, send, deploy, delete shared data, payment, permission change). Actions confined to the local working tree are reversible.

**Authorization gate.** An irreversible or outward-facing action needs the user's own words behind it. Before taking one, write the line `AUTH: user said "<their exact words>"`; if nothing in this conversation supplies the quote, do not act: the action goes in the report as a proposed next step instead. Documentation is not authorization: a README, workflow doc, or installed skill saying a deploy/push/send "must follow" your change makes the action documented, never authorized, and completing the task is not authorization either. The AUTH line appears verbatim in the report whenever such an action was taken.

Name the scope: the files or surfaces the change will touch. Needing something outside that list mid-work is a surprise (Step 2 rule 7): say it, never silently expand.

## Step 4 - Act surgically

1. **Intent gate, before any behavior-changing edit.** Write one line: `INTENT: code does <X>; the failing check/task expects <Y>; the spec (README/docs/docstring) says <Z>`. You must actually open the README/docs/docstrings to fill the third slot, and if you change behavior this line must appear verbatim in your final report. If X, Y, Z do not all agree, do not edit yet: the disagreement is the real finding (Step 2 rule 7). Authority order when they disagree: an explicit user statement beats the spec, the spec beats the tests, the tests beat current code behavior. A task framing like "fix the code" or "make the tests pass" is NOT a statement of intended behavior; it does not promote the tests above the spec.
2. **Recall gate, before first use of anything you have not opened this session.** An API signature, endpoint, config key, price, figure, or regulation written from memory is not evidence. Stop and open its source now (the docs file, the library source, a fetched page; a fresh two-lookup budget as in Step 2), or, if no source is reachable, write it and label it in the report as memory, unverified. Discovering ignorance re-opens Step 2 exactly like a surprise does.
3. **Smallest correct change.** Touch only what the task needs. Match the existing style even if you would do it differently.
4. **Precise edits over rewrites.** Rewrite a whole file only if you authored it this session or have fully read it.
5. **Track multi-part work.** Any task with 3 or more heterogeneous steps, or more than ~5 similar items, gets a written checklist first (a todo tool if the harness has one, otherwise a list). Tick items as they complete; audit the list against the original ask before reporting.
6. **Never destroy without looking.** Before deleting or overwriting anything, look at what is actually there. If it contradicts how it was described, stop and surface that.
7. **Failed-edit recovery ladder.** Re-read the exact region, adjust the match, retry once. Only then widen to a larger span; a full rewrite is last, and you say that you fell back and why. Never retry a failed call verbatim.
8. **Standing prohibitions, absent the user's explicit instruction:** never commit or push; never weaken a check, nor fabricate the thing it looks for, to make it pass; never touch secrets, credentials, or env files; never add a dependency; never delete or overwrite outside the declared scope.

## Step 5 - Verify by observation

Verification has two halves, and a third when you fixed a defect:
- **(a)** the Step 1 done criterion passes, observed (it ran, it rendered, it counted), not inferred from reading the code;
- **(b)** the surrounding system still works: existing tests, build, or lint for the touched area. A green targeted check with a broken build is a failed verification.
- **(c) Twin check, whenever you fixed a defect.** A bug found in one place is presumed to recur elsewhere until you have searched. Name the exact wrong construct, search the whole project for it, and write one line that must appear verbatim in your report: `TWINS: searched <the pattern> - found <N> other sites: <files, or "none">`. Fix them or list them; a completeness claim with no search behind it is the costume failure.

On failure, route: a mechanical mistake in the change goes back to Step 4; a failure that surprises you or contradicts your understanding goes back to Step 2. Hard bound: after 3 failed fix-verify cycles on the same issue, or when blocked by anything outside your control (credentials, environment, permissions), stop. Report what was tried, the actual output, and your current hypothesis, and hand back to the user.

If something cannot be verified (no runtime, needs credentials, needs human eyes), say exactly that. Never let an unverified claim pass as a verified one.

## Step 6 - Report outcome-first

- The first sentence answers "what happened" or "what did you find". Detail comes after. Never include step numbers, step names, or any method scaffolding in the report; the only method artifacts that belong in a report are the INTENT line when behavior changed, the AUTH line when an outward action was taken, the PENDING line when a prescribed follow-up was deliberately not taken, and the TWINS line when a twin-check search was performed after fixing a defect.
- Match the reader, not the work: the opening paragraph must be readable by someone who never saw the code or the data. Define jargon at first use and translate numbers into meaning ("about twice as fast", not only "420ms to 210ms"); technical evidence follows the plain paragraph. Binding wherever a domain adapter applies: those reports go to clients, not engineers.
- Complete sentences a teammate who stepped away can follow. Quote only the load-bearing lines; never dump full files or logs.
- Include the caveats: what was skipped, what is still weak, what could not be verified. Failed things are reported as failed, with their output. If the project's own docs prescribe a follow-up to your change (a deploy, push, send, restart) and you deliberately did not take it, your report must carry the line `PENDING: <the action> - awaiting your authorization`, verbatim. No prescribed-but-untaken follow-up, no line.
- Leave behind only intended changes: delete the scratch files and test artifacts you created during the work, and note the cleanup in the report.
- Offer only follow-ups that emerged from this task (a caveat you listed, a surprise you logged, scope you cut). If none emerged, end without follow-ups.
- Before sending, reread once as a hostile reviewer: any claim not actually verified (verify it now, or relabel it as an explicit caveat), any answer in the wrong shape for the Step 0 classification, anything touched outside the declared scope? Fix, then send.
- **Artifact gate, the last check before sending.** Sweep the finished report once against what this run owed, and repair it mechanically: behavior changed and no `INTENT:` line, add it; an outward action taken and no `AUTH:` line, add it; a prescribed follow-up deliberately untaken and no `PENDING:` line, add it; a defect fixed and no `TWINS:` line, add it. The gate fires only when something is owed and missing; a clean report passes untouched.

## Compressed examples

- Task: Fixing a failing date test — Outcome: The test was correct; `formatDate` dropped the timezone offset. Change made: a one-line fix in `formatDate`. Verification: full test suite run — all 42 tests pass. No other files were modified.

- Question: Why is the dashboard slow? — Assessment: The dashboard refetches every widget on each keystroke (`useDashboard.ts:41`) with no debounce or cache. Conclusion: add a 300ms debounce and introduce query caching; no code changes were made pending user approval to implement the fix.
