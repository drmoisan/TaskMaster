# P9-T7 - Toolchain single-clean-pass declaration (#614; AC24)

Timestamp: 2026-08-26T19-58

## Declaration

Steps 1 through 4 of the Phase 9 loop all passed in ONE uninterrupted sequence with **no file
rewritten between them**. That sequence is loop attempt 4, run end-to-end as a single scripted
pass.

Non-rewrite proof for the clean pass: before step 1, the SHA-256 of every one of the **4732**
tracked `*.cs`, `*.csproj`, `*.xml` and `packages.config` files outside `obj/`, `bin/`, `packages/`,
`.dotnet-sdk/`, `coverage/` and `.git/` was recorded. After `dotnet tool run csharpier format .`
the hashes were recomputed: **FILES_REWRITTEN_BY_FORMAT = 0**. `dotnet tool run csharpier check .`
then exited 0. Steps 2, 3 and 4 ran against that byte-identical tree.

## The four commands, verbatim, with their exit codes

1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
   - EXIT_CODE **0** (format) and **0** (check). `Checked 1530 files`. 0 files rewritten.
2. `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
   - EXIT_CODE **0**. 0 errors, 5 pre-existing System.Reactive advisories. Non-vacuity: 18 project
     DLL outputs, 36 `csc.exe` invocations, 59 `CoreCompile:` occurrences.
3. `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
   - EXIT_CODE **0**. 0 errors, 0 `CS86xx` diagnostics, 5 pre-existing advisories. Non-vacuity: 18
     project DLL outputs, 36 `csc.exe` invocations, 52 `CoreCompile:` occurrences.
4. `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .`
   - EXIT_CODE **0**, matching its declared expectation of 0 (the run is fully green, so
     `ExpectedExitCode` is omitted per the conventions skill). 6569 total, 6569 passed, 0 failed,
     0 skipped. Filtered first-party line coverage 84.8696%.

## Required statements

- **`/t:Rebuild` was used for BOTH MSBuild gates.** `/t:Build` was not substituted for either. This
  is what makes the analyzer and nullable gates non-vacuous: MSBuild's incremental up-to-date check
  does not invalidate on a command-line `/p:` change, so a warm `/t:Build` would exit 0 having
  skipped `CoreCompile` on every project.
- **`/p:Nullable=enable` was NOT added.** The step-3 command is character-for-character the command
  in `.github/workflows/ci.yml`. Adding the flag would diverge from CI and is red on `main` by
  construction.
- No step reported `EXIT_CODE: SKIPPED`; all four were executed and recorded.

## Restart count: 3

| Attempt | Step 1 | Step 2 | Step 3 | Step 4 | Outcome |
| --- | --- | --- | --- | --- | --- |
| 1 | pass | pass | pass | pass (6568/6568) | **Superseded.** The P9-T5 coverage analysis found `ArchiveStemContract.TryMakeArchiveRelative` at 92.0000%, with the reachable `root.Length == 0` guard uncovered. AC23 requires every pure branch introduced to be covered, so `TryMakeArchiveRelative_SeparatorOnlyRoot_ReturnsFalse` was added and the loop restarted from step 1. |
| 2 | pass | pass | pass | **FAIL** | `SegmentActivate_CrossStoreAncestor_LeavesSelectionUnchangedAndDiagnoses` failed: "Expected RenderedMessages() to contain a single item matching Contains("rejected"), but 2 such items were found." Root cause: log4net binds ONE logger per type, so the `MemoryAppender` is shared with router tests running concurrently in other classes; an exact-count assertion on a shared appender is order-dependent. Fixed by replacing the three `ContainSingle` count assertions with existence assertions plus an "only value-free rejection messages" assertion, documented in the test file. Restarted from step 1. |
| 3 | pass (2 files rewritten) | pass | pass | pass (6569/6569) | **Superseded.** Step 1 rewrote the two files edited during the attempt-2 remediation, so per the loop rule the pass restarted from step 1. |
| 4 | pass (**0** files rewritten) | pass | pass | pass (6569/6569) | **CLEAN PASS.** |

Both restarts that followed a genuine defect (attempts 1 and 2) fixed test code only; no production
behaviour was changed after attempt 1.
