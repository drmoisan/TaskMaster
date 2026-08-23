# Baseline Toolchain Step 5 — Tests with Coverage (Issue #449, [P0-T12], [P0-T13], [P7-T15])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command:
```
dotnet-coverage collect `
  --output <WORKTREE>\coverage\baseline-p0t12.cobertura.xml `
  --output-format cobertura `
  --settings coverage.config `
  -- "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" `
     <9 discovered assemblies> `
     /Settings:scripts\vscode\TaskMaster.cli.runsettings `
     /InIsolation `
     /TestCaseFilter:TestCategory!=LiveOutlook
```
EXIT_CODE: 0

The Cobertura report is written to the gitignored `coverage/` directory, NOT into the evidence tree,
per [P0-T12]. No helper script is retained under `evidence/`; the discovery-and-invocation script
lives in the session scratchpad outside the repository.

## Test-assembly discovery

Discovery recursed from WORKTREE for `*.Test.dll`, then filtered on the path **suffix after
WORKTREE** — never on the absolute path, because WORKTREE itself lies under `.claude\worktrees\` and
an absolute-path `\.claude\` exclusion would discard every assembly in this tree. Filters: keep
`*\bin\Debug\*`; exclude `*\obj\*`, `*\ref\*`, and `*\.claude\*`. This mirrors
`.github/workflows/_mstest-coverage.yml`, which filters on `\bin\Debug\` and excludes `\obj\` and
`\ref\`, with the `\.claude\` suffix exclusion added on top.

Raw matches: **18**. Retained after filtering: **9**.

```
QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
SVGControl.Test\bin\Debug\SVGControl.Test.dll
Tags.Test\bin\Debug\Tags.Test.dll
TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
TaskTree.Test\bin\Debug\TaskTree.Test.dll
TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

`/InIsolation` was supplied. Without it each assembly's `app.config` binding redirects are ignored
and roughly 1,695 phantom failures appear with empty messages and sub-millisecond durations,
surfacing as a Moq `TypeInitializationException` via `System.Threading.Tasks.Extensions`. No such
mass failure occurred, so the flag took effect and no test was modified.

## Test counts

```
Test Run Successful.
Total tests: 6437
     Passed: 6437
 Total time: 49.6296 Seconds
```

| Metric | Baseline value |
| --- | --- |
| Total | **6437** |
| Passed | **6437** |
| Failed | **0** |
| Skipped | **0** |

`vstest.console.exe` prints `Failed:` and `Skipped:` summary lines only when those counts are
non-zero; neither line is present, and the run is reported `Test Run Successful`, so both counts are
zero. A `grep -c -i failed` over the log returns 7, and all 7 are test METHOD NAMES containing the
word (for example `Passed FailedFactoryTask_ClosesWithoutLeavingAHostOrCallbackSubscription`), each
on a `Passed` line. There is no failing test at baseline.

## Coverage values (numeric)

Read from `coverage\baseline-p0t12.cobertura.xml`.

| Value | Baseline |
| --- | --- |
| Repo-wide root `line-rate` | `0.8532899236682991` = **85.3290 %** |
| Root `lines-covered` / `lines-valid` | 155,943 / 182,755 |
| `QuickFiler` package `line-rate` | `0.8091631603553062` = **80.9163 %** |
| `QfcExplorerController` figure | **ABSENT FROM THE REPORT** (see [P0-T13] below) |

Full per-package line rates at baseline, recorded so the `QuickFiler` figure is auditable in context:

| Package | line-rate | % |
| --- | --- | --- |
| QuickFiler.Test | 0.9636752136752137 | 96.3675 |
| SVGControl.Test | 0.8831710709318498 | 88.3171 |
| Tags.Test | 0.9706122448979592 | 97.0612 |
| TaskMaster.Test | 0.9501936912008855 | 95.0194 |
| TaskTree.Test | 1 | 100.0000 |
| TaskVisualization.Test | 0.9686780285582681 | 96.8678 |
| ToDoModel.Test | 0.8302401746724891 | 83.0240 |
| UtilitiesCS.Test | 0.9782362657530894 | 97.8236 |
| VBFunctions.Test | 1 | 100.0000 |
| **QuickFiler** | **0.8091631603553062** | **80.9163** |
| UtilitiesCS | 0.8957463976945245 | 89.5746 |
| TaskVisualization | 0.8984326018808777 | 89.8433 |
| log4net | 0.3019265926030094 | 30.1927 |
| Mono.Reflection | 0.39303482587064675 | 39.3035 |
| SVGControl | 0.47303128371089537 | 47.3031 |
| Microsoft.IO.RecyclableMemoryStream | 0 | 0.0000 |
| ToDoModel | 0.5731056563500534 | 57.3106 |
| Tags | 0.9268929503916449 | 92.6893 |
| TaskMaster | 0.7335945151811949 | 73.3595 |
| TaskTree | 0.9548387096774194 | 95.4839 |
| VBFunctions | 1 | 100.0000 |
| System.Linq.Async | 0.04755332496863237 | 4.7553 |
| System.Interactive | 0.02727272727272727 | 2.7273 |

Note the denominator caveat that [P7-T9] must carry forward: this direct
`dotnet-coverage collect --settings coverage.config` invocation does not apply the effective-config
test-assembly `ModulePath` exclusion that `scripts/vscode/Invoke-MSTestWithCoverage.ps1` derives, so
the nine `*.Test` packages are IN this denominator. The absolute repo-wide figure is therefore not
directly comparable to the 80% helper gate at
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-489`. The baseline and post-change runs use
the identical method, so the DELTA is sound even though the absolute value is not the gated figure.

### Coverage-seam method for the `QfcExplorerController` figure

The figure is computed by aggregating **every** Cobertura `<class>` element whose `filename` attribute
ends with the path segment `QuickFiler\Controllers\QfcExplorerController.cs`, summing hit and total
line counts across them (filename separators normalised to `\` before the suffix test). This is
required because `OpenQFItem` is `async`, so the compiler emits its state machine as a separate
`<class>` element with a mangled name, and the lambdas passed to `Task.Run` emit further separate
elements. Reading a single `<class>` element would report a figure for a fragment of the file. The
direct `dotnet-coverage collect` invocation performs no closure post-processing, so those elements
are present in the raw report.

---

## [P0-T13] — The baseline `QfcExplorerController` value is ABSENT FROM THE REPORT, not zero

Search performed over the Cobertura report:

Search target (suffix match, separators normalised to `\`):
`QuickFiler\Controllers\QfcExplorerController.cs`
Search scope: every `<class>` element under every `<package>` in
`coverage\baseline-p0t12.cobertura.xml`.

Search result:
```
QFCEXPL_MATCHED_CLASS_COUNT=0
QFCEXPL_LINES_HIT=0
QFCEXPL_LINES_TOTAL=0
QFCEXPL_LINE_RATE_PCT=ABSENT-FROM-REPORT (zero matching <class> elements)
```

**Recorded baseline value for `QfcExplorerController`: absent.**

The search matched **zero** `<class>` elements. This is not a coverage of 0%: it is the total absence
of the class from the report. The distinction is material and 0% would be a fabricated figure. A rate
requires a denominator, and the denominator here does not exist — the class contributes no
`<class>` element and no lines at all, so no ratio is defined.

The cause is the class-level `[ExcludeFromCodeCoverage]` attribute at
`QuickFiler/Controllers/QfcExplorerController.cs:20`, verified present at that exact line in the
merge-base tree:

```
    20	    [ExcludeFromCodeCoverage]
    21	    internal class QfcExplorerController : IQfcExplorerController
```

The attribute suppresses every member of the class, including the compiler-generated `async` state
machine and lambda display classes, so the instrumentation emits nothing for the file. [P5-T1]
removes that attribute, which is what brings the class into the report for the first time — and, per
[P7-T10], into the coverage DENOMINATOR for the first time, which is the declared reason a
`QuickFiler` package shortfall would be an expected rather than an anomalous outcome.

---

## [P7-T15] — No Python toolchain step was run, because none exists

Command: `ls -d scripts/dev_tools` -> EXIT_CODE 2,
`ls: cannot access 'scripts/dev_tools': No such file or directory`
Command: `ls -1 pyproject.toml poetry.lock` -> EXIT_CODE 2, both absent
Command: `git ls-files "*.py"` -> EXIT_CODE 0, 2 files, both inside
`docs/features/archive/2026-07-18-stale-app-config-binding-redirects-354/`

There is no `scripts/dev_tools/` directory (the only similar path is `scripts/dev-tools/`, hyphenated,
holding one PowerShell script) and no Poetry manifest. The importable package `scripts.dev_tools`
does not exist and there is no Poetry environment to run it in, so any skill step naming
`poetry run python -m scripts.dev_tools.*` is **unrunnable by absence**. It is recorded here as such.
No result is fabricated for it and it is not silently omitted. C# coverage in this plan is collected
by `dotnet-coverage` and read from the Cobertura report; no Python coverage runner exists here to
consume a coverage-target argument, which is why no task in this plan states one. See
`environment-preconditions.2026-08-22T09-16.md` finding (a) for the full verification set.

---

## Output Summary

Baseline suite: **6437 total, 6437 passed, 0 failed, 0 skipped**, EXIT_CODE 0, 49.63 s, across 9
discovered test assemblies with `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook`.
Coverage: repo-wide root line rate **85.3290 %** (155,943 / 182,755); `QuickFiler` package line rate
**80.9163 %**; `QfcExplorerController` **absent from the report** — zero matching `<class>` elements,
because the class-level `[ExcludeFromCodeCoverage]` at line 20 suppresses every member, so "absent" is
the correct baseline value and "0%" would be fabricated. No Python toolchain step was run because
none exists in this repository; that absence is recorded rather than fabricated or skipped.
