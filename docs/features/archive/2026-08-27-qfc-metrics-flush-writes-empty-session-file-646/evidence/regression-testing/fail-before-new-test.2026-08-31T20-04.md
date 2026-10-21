# Fail-Before — New Regression Test Against the Unguarded Implementation (P1-T4)

Timestamp: 2026-09-01T12-35

Task: `[P1-T4]` `[expect-fail]`
Test: `WriteMetricsAsync_WithAllNullOrWhitespaceDiagnostics_DoesNotInvokeWriter`
Test file: `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` (added by P1-T2)
Production file state: **unguarded** — the P1-T5 fix has not yet been applied to
`QuickFiler/Controllers/QfcHomeController.Metrics.cs`.

A failing result is the expected and required outcome of this task.

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

`/p:Platform=AnyCPU` (no space) is used here, not the solution-level `"/p:Platform=Any CPU"`
alias. `QuickFiler.Test.csproj` conditions its `PropertyGroup` on the literal
`Debug|AnyCPU` string, so a `Platform` value containing a space matches no `PropertyGroup`,
leaves `OutputPath` unset, and fails the build outright. The build succeeding confirms the
correct spelling was used.

## Step 2 — Run the New Test, Scoped

Command:
`& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~WriteMetricsAsync_WithAllNullOrWhitespaceDiagnostics_DoesNotInvokeWriter"`
EXIT_CODE: 1

Verbatim summary lines:

```
Total tests: 1
     Failed: 1
Test Run Failed.
 Total time: 1.5078 Seconds
```

## Verbatim Failure Detail

```
  Failed WriteMetricsAsync_WithAllNullOrWhitespaceDiagnostics_DoesNotInvokeWriter [346 ms]
  Error Message:
   Expected invoked to be False because an empty filtered array must not reach the writer at all, but found True.
```

Stack frames identify the failing assertion as
`FluentAssertions.Primitives.BooleanAssertions.BeFalse`.

## Acceptance

| Condition | Required | Observed | Met |
|---|---|---|---|
| Non-zero `EXIT_CODE` | non-zero | `1` | Yes |
| Printed summary shows `Failed:     1` | yes | `     Failed: 1` | Yes |
| Test filter selected the new test only | yes | `Total tests: 1` | Yes |

ACCEPTANCE: MET.

## The Failure Is Genuine, Not a Harness Artifact

The distinction matters because an assembly-load or test-host failure would also produce a
non-zero exit code while proving nothing about the defect. This run is a real assertion
failure:

- The test was discovered, selected, and **executed**: it ran for 346 ms, not sub-millisecond.
- The failure message is the assertion's own text, including the `because` reason string
  written into the test in P1-T2. It is not an empty message.
- The reported value is the defect itself: `invoked` was `True`, meaning
  `WriteMetricsAsync` reached `MetricsFileWriter` even though the null-and-whitespace filter
  had reduced `GetMoveDiagnostics`' `{ "   ", null, "\t" }` output to an empty array.
- `MyDocuments` was present in this fixture (`withMyDocuments` defaults to `true`), so the
  pre-existing MyDocuments guard at lines 131-134 was not the cause of, and did not mask,
  this result.

This is precisely the behavior issue #646 reports, reproduced deterministically in a unit
test with no filesystem, no live Outlook, and no wall-clock wait.

The `Warning:` blocks in the raw runner output concern the Xceed Fluent Assertions
community licence and are printed on every run of this suite; they are unrelated to the
result.
