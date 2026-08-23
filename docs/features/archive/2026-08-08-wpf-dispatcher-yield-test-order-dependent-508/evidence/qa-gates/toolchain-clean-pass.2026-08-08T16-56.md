# Toolchain Clean-Pass Attestation

Timestamp: 2026-08-08T16-56

Task: [P2-T6]

AC served: AC8.

## Attestation

I attest that P2-T1 through P2-T5 executed **in that order, within a single pass (pass 4), with no
step failing and no file rewritten between steps.**

Pass ordinal: **4**.

| Step | Task | Command | EXIT_CODE | Result | Artifact |
|---|---|---|---|---|---|
| 1 | P2-T1 | `csharpier format <workspace>` | 0 | 1488 files processed, 0 rewritten | `csharpier-format.2026-08-08T16-48.md` |
| 1v | P2-T2 | `csharpier check <2 in-scope files>` | 0 | 2 files checked, 0 unformatted | `csharpier-check.2026-08-08T16-48.md` |
| 2 | P2-T3 | `msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | 6 warnings, 0 errors, CoreCompile ran | `msbuild-analyzers.2026-08-08T16-49.md` |
| 3 | P2-T4 | `msbuild TaskMaster.sln /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 | 5 warnings, 0 errors | `msbuild-nullable.2026-08-08T16-50.md` |
| 4 | P2-T5 | `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput ...` | 0 | Total 6295, Passed 6295, Failed 0 | `tests-coverage.2026-08-08T16-55.md` |

Every step ran; none was skipped. No `EXIT_CODE: SKIPPED` appears anywhere in this pass.

## No file was rewritten during the pass

- P2-T1 rewrote nothing: `git diff --stat -- '*.cs'` after the format run was identical to before
  (2 files, 201 insertions / 6 deletions).
- P2-T2 is read-only.
- P2-T3 and P2-T4 write only `bin/` and `obj/` build outputs, not source.
- P2-T5 writes only the Cobertura report under `<FEATURE>/evidence/qa-gates/`.

Therefore no restart condition arose within pass 4.

## Full pass history (disclosed, not concealed)

The loop restarted per `.claude/rules/general-code-change.md`. Four passes were required:

| Pass | Steps 1-3 | Step 4 (tests) | Disposition |
|---|---|---|---|
| 1 | all passed | EXIT 1 — `Total 6295 / Passed 6293 / Failed 2` | Restarted. Artifact: `tests-coverage-pass1-failed.2026-08-08T16-42.md` |
| 2 | all passed | EXIT 1 — same 2 failures | Restarted. |
| 3 | — | not reached | Abandoned mid-pass on detecting a stale-build condition (below). |
| **4** | **all passed** | **EXIT 0 — `Total 6295 / Passed 6295 / Failed 0`** | **CLEAN — attested above.** |

### Why passes 1 and 2 failed

Both failed on the same two tests, in `QuickFiler.Test`,
`QfcItemController_InitializationTests`: `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState`
and `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates`, each throwing
`System.InvalidOperationException: Invoke or BeginInvoke cannot be called on a control until the
window handle has been created` from `QfcItemController.FocusAndTheme.cs:256`.

A controlled four-run attribution experiment
(`<FEATURE>/evidence/regression-testing/preexisting-failure-attribution.2026-08-08T16-52.md`) proved
these are pre-existing and not caused by this change: with both in-scope files reverted to
merge-base `003c5715` and the solution rebuilt, the combined suite produced
`Total 6293 / Passed 6291 / Failed 2` — the same two tests, and a byte-for-byte match with the
"Run 1" baseline already recorded at `<FEATURE>/issue.md:53` before this work began. The two tests
pass in class isolation (9/9) and in their own assembly (867/867); they fail only in the combined
instrumented run, the signature of a timing-sensitive WinForms handle race.

### Why pass 3 was abandoned

The attribution experiment restored the changed files with `Copy-Item`, which preserves the source
file's `LastWriteTime`. The restored files were therefore *older* than the build outputs produced
during the experiment, so MSBuild's up-to-date check skipped compilation (1.06s, no `CoreCompile`,
5 warnings instead of 6) and the binaries still contained baseline code. Reporting that as a passing
gate would have been a false pass. The condition was detected from the missing `CS2002`/`CoreCompile`
signal, the two files' timestamps were set forward, and the loop restarted at P2-T1 as pass 4, where
`CoreCompile` demonstrably ran (13.61s, CS2002 present).

The timestamp adjustment changed filesystem metadata only, not content: SHA-256 of both files is
identical before and after the experiment (`WpfDispatcherYield.cs`
`02986C1C…C352A364`, `WpfDispatcherYieldTests.cs` `4374A608…2FE28701`). It occurred **before** pass
4 began, so it does not violate the "no file rewritten during the pass" condition.

## Integrity statement

The green result in pass 4 was obtained by rerunning the identical, unmodified command. No
`[Ignore]`, `[DoNotParallelize]`, `/TestCaseFilter` exclusion, retry wrapper, sleep, or timing hack
was introduced anywhere to route around the pre-existing failures, and no assertion was weakened.
The out-of-scope `QuickFiler` handle race is reported for separate triage rather than absorbed or
suppressed.

Output Summary: ATTESTED. Pass 4 is a single clean pass of the full C# toolchain in order —
csharpier format (EXIT 0, 0 rewrites), csharpier check (EXIT 0), analyzer msbuild (EXIT 0, 6/0,
CoreCompile ran), nullable msbuild (EXIT 0, 5/0), and vstest with coverage (EXIT 0, 6295/6295/0) —
with no step failing and no file rewritten between steps. Passes 1 and 2 restarted on two
pre-existing out-of-scope `QuickFiler.Test` failures (proven pre-existing by a controlled
merge-base attribution run) and pass 3 was abandoned after a stale-build false-pass condition was
detected and corrected. No gate was weakened to reach the clean pass.
