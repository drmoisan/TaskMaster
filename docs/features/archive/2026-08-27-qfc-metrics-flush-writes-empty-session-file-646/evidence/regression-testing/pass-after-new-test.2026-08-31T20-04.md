# Pass-After — New Regression Test Against the Guarded Implementation (P1-T9)

Timestamp: 2026-09-01T12-43

Task: `[P1-T9]`
Test: `WriteMetricsAsync_WithAllNullOrWhitespaceDiagnostics_DoesNotInvokeWriter`
Test file: `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`
Production file state: **guarded** — the P1-T5 fix is applied to
`QuickFiler/Controllers/QfcHomeController.Metrics.cs` at lines 175-178.

## Step 1 — Rebuild the Test Project

Command:
`msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`
EXIT_CODE: 0

Verbatim summary lines:

```
Build succeeded.

    3 Warning(s)
    0 Error(s)
```

`/p:Platform=AnyCPU` (no space) is used, per the same project-level requirement documented
in P1-T4. The changed production file reaches this build through the `ProjectReference` from
`QuickFiler.Test.csproj` to `QuickFiler\QuickFiler.csproj`, and `QuickFiler.csproj` also
conditions its `PropertyGroup` on the literal `Debug|AnyCPU` string, so the no-space spelling
propagates correctly to it as a global property.

## Step 2 — Run the Same Scoped Command as P1-T4

Command:
`& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~WriteMetricsAsync_WithAllNullOrWhitespaceDiagnostics_DoesNotInvokeWriter"`
EXIT_CODE: 0

The `/TestCaseFilter` is byte-identical to the one used in P1-T4. The only variable that
changed between the two runs is the presence of the four-line guard in the production file.

## Verbatim Output

```
  Passed WriteMetricsAsync_WithAllNullOrWhitespaceDiagnostics_DoesNotInvokeWriter [242 ms]

Test Run Successful.
Total tests: 1
     Passed: 1
 Total time: 1.4446 Seconds
```

## Acceptance

| Condition | Required | Observed | Met |
|---|---|---|---|
| `EXIT_CODE` | `0` | `0` | Yes |
| Printed summary shows `Passed:     1` | yes | `     Passed: 1` | Yes |
| Filter selected the new test only | yes | `Total tests: 1` | Yes |

ACCEPTANCE: MET.

## Fail-Before / Pass-After Pair

| Run | Task | Production state | EXIT_CODE | Result |
|---|---|---|---|---|
| Fail-before | P1-T4 | unguarded | 1 | `Failed: 1` — `Expected invoked to be False ... but found True.` |
| Pass-after | P1-T9 | guarded | 0 | `Passed: 1` |

Same test, same filter, same runner flags, same assembly path; only the four-line guard
differs. The test is therefore demonstrably sensitive to the defect rather than passing
vacuously, which is the evidence AC4 requires and the evidence backing AC1.

The test also ran for 242 ms rather than sub-millisecond, confirming it was genuinely
discovered and executed rather than skipped or silently dropped by the filter.
