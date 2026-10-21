# P7-T9 — Write-set scope audit

Timestamp: 2026-09-08T10-19
Task: [P7-T9]
Command: `git add --intent-to-add -- <the three new test files>`, then `git diff --name-only bb1c7d4b60f7b782227956f36859314d5c47bb03 -- "*.cs" "*.csproj" "*.config" "*.props" "*.targets" "*.runsettings" "*.yml" ":(exclude).claude"`, then `git status --porcelain --untracked-files=all -- . ":(exclude).claude"`, then `git diff --numstat bb1c7d4b60f7b782227956f36859314d5c47bb03 -- UtilitiesCS/Threading/TimeOutTask.cs UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`
EXIT_CODE: 0

The `--intent-to-add` staging is what makes the three new files visible to an anchored
`--name-only` diff, which otherwise enumerates tracked changes only and would report them as
absent. The `:(exclude).claude` pathspec is applied to every span because the dot-claude
agent-memory tree is tracked in this repository and may be dirty from preparation agents.

## Anchored name-only diff — exactly the 20 Write Set paths

```
UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs
UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs
UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs
UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs
UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs
UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs
UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs
UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs
UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs
UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs
UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs
UtilitiesCS.Test/UtilitiesCS.Test.csproj
UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs
UtilitiesCS/Extensions/DfDeedle.cs
UtilitiesCS/Extensions/DictionaryExtensions.cs
UtilitiesCS/HelperClasses/PrettyPrint.cs
UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs
```

20 paths. Sorted, this list equals the plan's 20-path Write Set exactly:

- **No fewer.** Every one of the 20 declared paths is touched. Items 1 to 8 are the production
  files, 9 to 19 the test files, 20 the project file.
- **No more.** No path outside the set appears. In particular no `.github/workflows/` file, no
  `packages.config`, no `.runsettings`, no `.editorconfig` and no `BannedSymbols.txt` was touched,
  and the diff span explicitly includes the `*.config`, `*.props`, `*.targets`, `*.runsettings`
  and `*.yml` globs so that a change to any of them would have appeared.

## Scoped porcelain status

The porcelain output lists the same 20 source paths — three as `A` (the intent-to-added new files)
and seventeen as `M` — plus, under `<FEATURE>/`, the modified `plan.2026-09-07T22-03.md` and
`spec.md` and 27 untracked evidence artifacts. No path outside the 20 plus `<FEATURE>/` appears.

`docs/features/potential/` is empty at this point; P8-T9 has not yet run. The C1 helper script
`coverage/plan811-helper.ps1` does not appear because `coverage/*` is gitignored.

## Untouched-file control

```
git diff --numstat bb1c7d4b60f7b782227956f36859314d5c47bb03 -- UtilitiesCS/Threading/TimeOutTask.cs UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs
```

Output is empty. Both files are byte-identical to the base commit, which confirms two deliberate
non-goals held:

- The inert `(int, int)` `TimeoutAfter` overloads in `TimeOutTask.cs` were not deleted (D9; the
  file is 1011 lines and already over the cap, and the overloads retain two test callers).
- `DfDeedle.QfcColumns.cs` was not edited, so its line-96 doc comment still names the now-deleted
  `TableEtlInvoker` and is stale. That is recorded as a follow-up in P8-T9 rather than fixed here,
  because touching the file would widen the write set.

## Acceptance evaluation

- The name-only list, sorted, equals exactly the 20 Write Set paths: none missing, none extra.
  PASS
- The porcelain output lists no path outside the 20 paths plus `<FEATURE>/` plus
  `docs/features/potential/`. PASS
- `git diff --numstat` over `TimeOutTask.cs` and `DfDeedle.QfcColumns.cs` is empty. PASS

## Output Summary

The change is exactly the declared 20-path write set. No scope escape. Both files the plan
declared untouched are byte-identical to the base commit.
