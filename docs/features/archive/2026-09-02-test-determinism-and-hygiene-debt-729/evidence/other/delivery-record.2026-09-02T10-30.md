# Delivery record (P7-T6)

Timestamp: 2026-09-03T00-13

EXIT_CODE: 0

## (a) `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` is green-from-birth

This guard is `green-from-birth` and is regression prevention, not a fail-before/pass-after
regression test. `UtilitiesCS.Test` compiles zero `Form`-derived types today, so the guard passes
on its first run and there is no red state to reproduce. **No reviewer should expect a red run for
it.** Its P4-T6 evidence records `PassedCount: 1` and `FailedCount: 0` for the test node
`UtilitiesCS.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType`, and
that artifact carries the same `green-from-birth` token.

The three `Form` sources this phase deleted from `UtilitiesCS.Test` (`Form1`, `Form2`, `Form3`,
with their Designer and resx siblings, plus `ResourceTests.cs`) were orphaned: the project file
did not compile them, which is why the assembly contained no `Form`-derived type before the
deletion and why the guard could not go red.

## (b) `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` is the genuine regression test

This is the red-before / green-after regression test for this change. `SVGControl.Test` did
compile live `Form`-derived types, so the guard fails against the pre-deletion assembly and passes
after the deletion.

- Red-before evidence:
  `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-guard-fail-before.2026-09-02T10-30.md`
  (P3-T4, `ExpectedExitCode: 1`, `PassedCount: 0`, `FailedCount: 1`, failure text naming both
  `SVGControl.Test.Form1` and `SVGControl.Test.Form2`)
- Green-after evidence:
  `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-guard-pass-after.2026-09-02T10-30.md`
  (P3-T8, `EXIT_CODE: 0`, `PassedCount: 1`, `FailedCount: 0`)

## (c) The five final Phase 6 toolchain commands and their exit codes

| # | Task | Command | EXIT_CODE |
|---|---|---|---|
| 1 | P6-T1 | `dotnet tool run csharpier format` over the seven plan-owned formattable paths | 0 |
| 2 | P6-T2 | `dotnet tool run csharpier check .` | 0 |
| 3 | P6-T3 | `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 |
| 4 | P6-T4 | `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 |
| 5 | P6-T5 | `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput '<feature evidence>\coverage-final.cobertura.xml'` | 0 |

The final pass recorded `RewrittenFileCount: 0`, `PassedCount: 6955`, and `FailedCount: 0`.

## (d) Finding 4 is out of scope

Finding 4 — the pump-hosted QuickFiler.Test UI-marshalling defect — is **out of scope for this
change** and is carried by issue **#743**. No QuickFiler path is modified: P7-T3 records that both
`git diff --name-only $base HEAD -- QuickFiler` and `git status --porcelain -- QuickFiler` return
empty output. The supporting analysis is recorded in the promotion record
`docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md`, and
the four reasons no test-only fix exists are enumerated in `spec.md` under the Finding 4
out-of-scope bullet.

## Known out-of-scope flakes:

None observed.

The P6-T5 full-suite coverage run exited 0 with `FailedCount: 0`, so the #743 mechanical re-run
branch was never entered and no QuickFiler.Test test node failed. There is no failing node
identifier to list.

Output Summary: `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` is `green-from-birth`
regression prevention with no expected red run; `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs`
is the genuine red-before/green-after regression test with both Phase 3 evidence artifacts named
above; all five Phase 6 toolchain commands exited 0; Finding 4 remains out of scope under issue
#743; and no out-of-scope flake was observed.
