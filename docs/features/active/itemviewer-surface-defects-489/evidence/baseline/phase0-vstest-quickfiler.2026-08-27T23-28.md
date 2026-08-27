# Phase 0 — QuickFiler.Test vstest Baseline (P0-T13)

Timestamp: 2026-08-27T23-28
Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p0-t13.trx" /ResultsDirectory:docs\features\active\itemviewer-surface-defects-489\evidence\baseline
EXIT_CODE: 1
ExpectedExitCode: 1

BaselinePassed: UNMEASURED
BaselineFailed: UNMEASURED
BaselineSkipped: UNMEASURED

## ACCEPTANCE NOT MET — this task is recorded but NOT checked off

P0-T13's acceptance has two conjuncts and **both fail**:

1. All three integers are recorded — **NOT met.** No test was executed, so no integer exists to
   record. The three fields are recorded as `UNMEASURED` rather than as a fabricated `0`, because a
   recorded `0` would be indistinguishable from a genuine measurement and would silently become the
   comparison floor for spec AC53 in Phase 11.
2. `evidence/baseline/p0-t13.trx` exists — **NOT met.** No TRX was produced; vstest exited before
   discovery.

The plan checkbox for `[P0-T13]` is therefore left unchecked. `BaselinePerClass:` and
`BaselineNamedPins:` are likewise not recorded, because both are defined as readings taken from
`p0-t13.trx`, which does not exist. P6-T7, P8-T11, P8-T15, P9-T9, P11-T7 and P11-T12 all compare
against blocks this artifact cannot supply.

## What vstest reported

The complete output was:

```
VSTest version 18.9.0 (x64)

The test source file "<repo-root>\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" provided was not found.
```

vstest resolved correctly. The single path returned by the `vswhere.exe` resolution the plan
specifies is the Visual Studio 18 Community
`Common7\IDE\Extensions\TestPlatform\vstest.console.exe`. The failure is the absence of the test
assembly, not a tooling or invocation defect.

## Root cause — a consequence of the P0-T11 and P0-T12 build failure

`QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` does not exist on disk, and
`QuickFiler.Test/bin/Debug/` is empty. Both msbuild gates were run with `/t:Rebuild`, which is
load-bearing for those gates and which deletes existing build output before recompiling. The rebuild
then failed at `CoreCompile` on `UtilitiesCS`, on which `QuickFiler.Test` transitively depends, so the
output directory was emptied and never repopulated. The underlying cause is the inherited analyzer
version skew recorded in full in the P0-T11 artifact
`phase0-analyzer-build.2026-08-27T23-26.md`.

No fallback build was attempted. Building `QuickFiler.Test` alone would not succeed while
`UtilitiesCS` fails, and repairing the skew requires editing project files, which Phase 0 forbids and
which lies outside this feature's scope lock.

## On the ExpectedExitCode field

`ExpectedExitCode:` is recorded as `1` so that the artifact's own schema row is internally consistent
with the observed exit code and the file is not dropped from collection as a parse anomaly. This is
**not** an assertion that the gate passed. The plan's rule for this field under P0-T13 is
`ExpectedExitCode: 0` when `BaselineFailed:` is `0` and `1` when it is non-zero; neither branch
applies, because `BaselineFailed:` was never measured. The gate for this task is the recorded
integers, never the exit code, and the integers are absent.

Output Summary: The vstest baseline **did not run**. vstest.console.exe 18.9.0 exited `1` with
`The test source file ... QuickFiler.Test.dll provided was not found.` The assembly is absent because
both Phase 0 msbuild gates use `/t:Rebuild`, which emptied `QuickFiler.Test/bin/Debug/`, and the
rebuild then failed on `UtilitiesCS` through the inherited analyzer version skew recorded under
P0-T11. `BaselinePassed:`, `BaselineFailed:` and `BaselineSkipped:` are recorded as `UNMEASURED`
rather than fabricated zeros, no `p0-t13.trx` exists, and the `BaselinePerClass:` and
`BaselineNamedPins:` blocks that five later tasks compare against cannot be produced. P0-T13's
acceptance is not met and the task is left unchecked.
