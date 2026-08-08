# Probe Teardown Confirmation

Timestamp: 2026-08-08T16-28

Task: [P0-T14]

Confirms the temporary `[expect-fail]` probe edit made for P0-T12 has been fully reverted, so
Phase 1 starts from an unmodified merge-base source tree.

## Revert action

Command: `git checkout -- UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`
EXIT_CODE: 0 (no output)

## Gate 1 — probe file diff is empty

Command: `git diff -- UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`
EXIT_CODE: 0

```
(empty)
```

PASS.

## Gate 2 — scoped source status is empty

Command: `git status --porcelain -- '*.cs' '*.csproj' '*.sln'`
EXIT_CODE: 0

```
(empty)
```

PASS. Per P0-T3 and the plan's git-gate scoping clause, globally-clean porcelain is NOT gated:
`.claude/agent-memory/**` is tracked and dirty at branch head and the `<FEATURE>` folder is
untracked by construction.

## Gate 3 — `UtilitiesCS.Test/UtilitiesCS.Test.csproj` unmodified

Command: `git diff --name-only -- UtilitiesCS.Test/UtilitiesCS.Test.csproj`
EXIT_CODE: 0

```
(empty)
```

PASS. The probe deliberately edited the existing test method in place rather than adding a `.cs`
file, precisely so that no `<Compile Include>` item had to be added to this legacy non-SDK project.
The csproj is untouched.

## Gate 4 — no source drift versus merge-base

Command: `git diff --stat 003c5715055d7d1933db68a742531332756e30b2 -- '*.cs' '*.csproj' '*.sln'`
EXIT_CODE: 0

```
(empty)
```

PASS. The whole source tree is byte-identical to the merge-base, matching the P0-T3 baseline.

## Gate 5 — no probe token survives in the file

Command: grep of `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` for
`ProbeStaDispatcherHost`, `Timeout\(30000\)`, `System\.Windows\.Threading`

```
Found 0 total occurrences across 0 files.
```

PASS. All three probe-only constructs are gone: the temporary nested STA host class, the temporary
`[Timeout(30000)]` attribute, and the temporary `using System.Windows.Threading;` directive. This is
a content check independent of git, so it would catch a revert that git considered clean for any
reason.

## Note on build outputs

`UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` currently still contains the compiled probe, since
only the source was reverted. This is not a defect and is not gated here: Phase 1 modifies the same
file and Phase 2 rebuilds the solution (P2-T3) before any test command runs, so no probe code can
reach a Phase 2 measurement.

Output Summary: PASS, all five gates. The probe edit is fully reverted — empty diff on the probe
file, empty scoped `git status`, `UtilitiesCS.Test.csproj` unmodified, zero source drift versus
merge-base `003c5715`, and zero surviving probe tokens in the file. The tree is back to its P0-T3
baseline state and Phase 1 begins from unmodified sources.
