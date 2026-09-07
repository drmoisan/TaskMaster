# P11-T12 — Unfiltered QuickFiler.Test run and the NoLiveForm structural guard

Timestamp: 2026-08-28T02-33
Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p11-t12.trx" /ResultsDirectory:docs\features\active\itemviewer-surface-defects-489\evidence\qa-gates
EXIT_CODE: 0
ExpectedExitCode: 0

Loop iteration: **1**. TRX: `evidence/qa-gates/p11-t12.trx`.

`/InIsolation` and the explicit `/ResultsDirectory:` are both present, as the task requires. Without
`/ResultsDirectory:` the TRX would land in `TestResults\` outside the evidence tree and the
artifact-existence acceptance would be unsatisfiable.

TRX counters read verbatim: `total=1121 passed=1121 failed=0 notExecuted=0 error=0`. Console
reported `Test Run Successful.`

## Acceptance

### The structural guard test passes

```
ExecutingAssembly_ContainsNoFormDerivedType = Passed   (duration 00:00:00.0007795)
```

`QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs`. P0-T13's `BaselineNamedPins:` block recorded it
as `passed`, so the unconditional branch of the acceptance applies and it must simply pass — it does.
The conditional branch, which would have applied had the baseline recorded it as `failed`, does not
arise, and there is no sibling owner to name.

### Assembly-wide failed count is not greater than the baseline

```
BaselineFailed (P0-T13) = 0
Final failed            = 0        0 is not greater than 0.   SATISFIED
```

`FinalPassed` is 1121 against a baseline of 1099, and `notExecuted` is 0.

### No test added by this feature creates or reads a temporary file

The five test files this feature adds to or creates —
`QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs`,
`QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs`,
`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs`,
`QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs` and
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` — were searched for every
filesystem entry point a temporary file would require:

```
GetTempPath | GetTempFileName | Path.GetRandomFileName |
File.Create | File.WriteAllText | File.WriteAllLines | File.WriteAllBytes |
File.AppendAllText | File.Open | Directory.CreateDirectory |
new StreamWriter | new FileStream | TempDirectory | TestContext.DeploymentDirectory
```

The search returned **zero** matching lines across all five files. None of these tests creates,
writes, opens or reads a file of any kind; they exercise the controller and viewer seams through Moq
doubles only. This satisfies the General Unit Test Policy's prohibition on temporary files in tests,
for which the repository records no approved exception.

### Exit code

The observed exit code is `0`. `vstest.console.exe` exits non-zero whenever any executed test fails;
the assembly-wide failed count is `0`, so `ExpectedExitCode: 0` is declared per the task's branch
rule and the artifact normalizes to `pass`. The gate is the guard-test result and the no-regression
comparison, never the exit code. This run is unfiltered over the whole `QuickFiler.Test` assembly,
which is co-owned by siblings 468, 484, 446, 493 and 501; an absolute `EXIT_CODE: 0` requirement
would have been unsatisfiable the moment P0-T13 recorded a non-zero `BaselineFailed:`, which is why
the gate is relative. In this run the relative and absolute readings coincide, because both the
baseline and the final failed count are `0`.

## Artifact hygiene

This run does not pass `/EnableCodeCoverage`, and no attachment directory was produced: the results
directory contains **zero** subdirectories after the run. `p11-t12.trx` was sanitised in place — 2242
occurrences of the worktree root replaced with the repo-root placeholder, 1124 of the machine name
with the host placeholder and 3 of the account name with the user placeholder, all
case-insensitively because vstest writes the `storage=` attribute in all-lower-case. A
case-insensitive search of the committed TRX returns **0** for the account name and **0** for the
machine name.

**XML-escaping note.** Each placeholder is written in entity form — `&lt;repo-root&gt;`,
`&lt;host&gt;`, `&lt;user&gt;` — because XML forbids a raw less-than character in a text node or an
attribute value and the literal form would make the document unparseable. The sanitised file was
re-parsed with a strict XML reader: it parses, its `UnitTestResult` element count is **1121**, which
matches the `Counters total` of 1121 recorded above, and it carries no BOM, matching the committed
`evidence/baseline/p0-t13.trx`.

## Loop consequence

This is the last command stage of the loop. It passed and rewrote no tracked source file. No restart
is triggered; the loop terminates cleanly at iteration 1 and P11-T13 records its history.

Output Summary: The unfiltered `QuickFiler.Test` gate **passes** at loop iteration 1 with
`EXIT_CODE: 0` and `total=1121 passed=1121 failed=0`. The structural guard test
`ExecutingAssembly_ContainsNoFormDerivedType` reports **Passed**, matching its P0-T13 baseline state,
so no regression branch and no sibling attribution arises. The assembly-wide failed count of `0` is
not greater than the baseline `0`. A search of all five test files this feature adds to or creates
for fourteen temporary-file and filesystem entry points returned **zero** matches, so no test added
by this feature creates or reads a temporary file. No attachment directory was produced; the TRX is
sanitised, parses strictly at 1121 results, and carries zero residual account or machine identifiers.
