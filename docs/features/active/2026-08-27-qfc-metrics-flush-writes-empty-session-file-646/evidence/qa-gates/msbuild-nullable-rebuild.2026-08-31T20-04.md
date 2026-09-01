# QA Gate — MSBuild Nullable Rebuild, Final (P2-T4)

Timestamp: 2026-09-01T12-57

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0
Output Summary: `Build succeeded.` with 5 Warning(s) and 0 Error(s), unchanged from the P0-T9 baseline, and 0 `CS86xx` occurrences in the log at both baseline and final. Under `/p:TreatWarningsAsErrors=true` any C# compiler warning introduced by this change would have been promoted to a build error; none was. 36 `csc.exe` command-line occurrences in the log, matching baseline, confirming compiler diagnostics were genuinely produced.

## Verbatim Printed Summary Lines

```
Build succeeded.

    5 Warning(s)
    0 Error(s)
```

## Acceptance

| Condition | Required | Observed | Met |
|---|---|---|---|
| `EXIT_CODE` | `0` | `0` | Yes |
| Printed summary line reads `Build succeeded.` | yes | `Build succeeded.` | Yes |

ACCEPTANCE: MET.

## Comparison Against Baseline

| Measure | Baseline (P0-T9) | Final (P2-T4) | Delta |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | none |
| Summary line | `Build succeeded.` | `Build succeeded.` | none |
| Warnings | 5 | 5 | none |
| Errors | 0 | 0 | none |
| `CS86xx` occurrences in the log | 0 | 0 | none |

The change introduces zero compiler warnings. Under `/p:TreatWarningsAsErrors=true`, any C#
compiler warning this change had introduced would have been promoted to a build error and
failed this gate; none was. The 5 surviving warnings are the pre-existing MSBuild
`_RxCheckPackagesConfig` warnings described in the P2-T3 artifact, which
`TreatWarningsAsErrors` does not promote because it sets a C# compiler property and these are
raised by an MSBuild `Warning` task.

## Command Fidelity

This is character-for-character the command in `.github/workflows/ci.yml` (step "Build with
nullable warnings treated as errors"). Per `CLAUDE.md` C#1.3, two properties of it were
preserved deliberately:

- `/p:Nullable=enable` was **not** added. No project in this repository carries a `<Nullable>`
  element and there is no `Directory.Build.props`, so that property is a solution-wide opt-in
  that would conscript every file which has never adopted the pragma. CI omits it
  deliberately.
- `/t:Build` was **not** used. MSBuild's up-to-date check does not invalidate on a
  command-line `/p:` change, so a warm `/t:Build` returns exit 0 having skipped `CoreCompile`
  on every project and the gate could not fail.

## Non-Vacuity Check

The captured log contains 36 `csc.exe` command-line occurrences, matching the baseline,
confirming `CoreCompile` ran on every project and compiler diagnostics were genuinely
produced.

## Scope Qualification (recorded, not resolved)

Neither file changed by this item carries a `#nullable enable` directive:

Command: `grep -n "#nullable" QuickFiler/Controllers/QfcHomeController.Metrics.cs` — no match (exit 1)
Command: `grep -n "#nullable" QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` — no match (exit 1)

Nullable enforcement in this repository is per-file opt-in, so neither file participates in
nullable-flow analysis and this gate produced no `CS86xx` diagnostic about either. What this
gate does establish for this change is the stronger-than-nullable general condition that the
change introduces no C# compiler warning of any kind, since all such warnings are errors
here.

Adding a `#nullable enable` pragma to either file would be an opt-in that the plan does not
authorize and that would expand this item's diff beyond the four-line guard and the one added
test, so none was added. This is a statement of the gate's actual reach, not a gap being
waived: the guard `if (lines.Length == 0) { return; }` operates on `lines`, a non-nullable
`string[]` produced by `.ToArray()`, and introduces no null-state question for a nullable
analysis to answer.
