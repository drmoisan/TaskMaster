# QA Gate — Fail-Before / Pass-After Comparison (Issue #656)

Timestamp: 2026-09-01T14-48
Task: [P3-T3]

Test under comparison: `CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain`

Source artifacts named by this comparison:

- Red run: `docs/features/active/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate-656/evidence/regression-testing/red-run.2026-08-31T20-40.md`
- Green run: `docs/features/active/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate-656/evidence/qa-gates/green-run.2026-08-31T20-40.md`

## Result pair

| Run | Phase | `total` | `passed` | `failed` | Exit |
|---|---|---|---|---|---|
| Red (before the production edit) | P1-T3 | 1 | **0** | **1** | 1 |
| Green (after the production edit) | P3-T2 | 1 | **1** | **0** | 0 |

The red run recorded `failed=1, passed=0`; the green run recorded `failed=0, passed=1`. Both are for
the same test name and the same test assembly, under commands identical apart from the results
directory.

---

## Embedded verbatim blocks — red artifact

Source: `evidence/regression-testing/red-run.2026-08-31T20-40.md`

Timestamp: 2026-09-01T14-42

Command:
```
$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
New-Item -ItemType Directory -Force -Path 'TestResults\p1-t3' | Out-Null
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain' '/Logger:trx' '/ResultsDirectory:TestResults\p1-t3'
```

EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary: The new regression test ran alone and failed as expected. Exit code 1, which equals
the declared expectation, so this gate is normalized to pass. The TRX reports 1 total, 0 passed, 1
failed. The failure is a runtime assertion failure, not a compile failure: the assembly built
cleanly in P1-T2 against unmodified production code.

Failure message from the red TRX:
```
Expected harness.Host.CloseReasons to be equal to {BreadcrumbDropDownCloseReason.Uncommitted {value: 1}, BreadcrumbDropDownCloseReason.Uncommitted {value: 1}} because the close after a bypassing reopen must reach _host.Close a second time, but {BreadcrumbDropDownCloseReason.Uncommitted {value: 1}} contains 1 item(s) less.
```

---

## Embedded verbatim blocks — green artifact

Source: `evidence/qa-gates/green-run.2026-08-31T20-40.md`

Timestamp: 2026-09-01T14-47

Command:
```
$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
New-Item -ItemType Directory -Force -Path 'TestResults\p3-t2' | Out-Null
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain' '/Logger:trx' '/ResultsDirectory:TestResults\p3-t2'
```

EXIT_CODE: 0

Output Summary: The new regression test passes after the production edit. Exit code 0, 1 total, 1
passed, 0 failed. Together with the P1-T3 red run this establishes the fail-before / pass-after pair.

---

## AC-4 Reconciliation:

AC-4 states the pair should be recorded "in the feature evidence folder under `evidence/qa-gates/`"
and checked "by comparing the two recorded `Invoke-MSTestWithCoverage.ps1` outputs for that test
name". Two departures from that stated check method were necessary. Each is recorded here with its
reason.

**(a) Storage location of the red run.** The red run is stored under
`evidence/regression-testing/`, not under `evidence/qa-gates/`. That is the canonical fail-before
location required by `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, whose evidence
path scheme is non-overridable and which directs a fail-before artifact search at
`<FEATURE>/evidence/regression-testing/`. Writing the red run to `evidence/qa-gates/` instead would
place it outside the location a later audit searches for fail-before evidence. This artifact
resolves the tension by embedding the red run's `Timestamp:`, `Command:`, `EXIT_CODE:` and
`Output Summary:` blocks verbatim above, so both run outputs are present under `evidence/qa-gates/`
exactly as AC-4 requires, while the authoritative red artifact remains in its canonical folder.

**(b) Runner used for both single-test runs.** Both runs use `vstest.console.exe` directly rather
than `scripts/vscode/Invoke-MSTestWithCoverage.ps1`. Neither wrapper accepts a `TestCaseFilter`
override — `scripts/vscode/Invoke-MSTest.ps1:54` and
`scripts/vscode/Invoke-MSTestWithCoverage.ps1:76` each pin the filter — and editing either script is
outside this item's authorized two-file footprint. A wrapper run would therefore have executed the
entire suite, which cannot exit 0 while a test is deliberately failing and so could not have
produced a scoped red record at all. Both wrapper protections are reproduced explicitly in the
direct invocation: `/InIsolation` is passed, and `TestCategory!=LiveOutlook` is the first conjunct
of the filter, so no real Outlook process can be launched. The full-suite wrapper run that AC-18
requires is executed separately in P4-T7 and covers this test along with every other.

Output Summary: The fail-before / pass-after pair is established for
`CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain`: failed=1/passed=0 before the
production edit, failed=0/passed=1 after it. Both source artifacts are named and both run outputs
are embedded verbatim. Two departures from AC-4's stated check method are recorded above with their
reasons.
