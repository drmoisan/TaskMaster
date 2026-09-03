# SVGControl.Test structural guard — green-after run (P3-T8)

Timestamp: 2026-09-02T23-25

Command: `& $vstest SVGControl.Test\bin\Debug\SVGControl.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~NoLiveFormInTestAssemblyTests"`

EXIT_CODE: 0

PassedCount: 1

FailedCount: 0

## Test node

`SVGControl.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` — `Passed`

Output Summary:

- `Test Run Successful.` `Total tests: 1` / `Passed: 1`.
- This is the green-after half of the Finding 2 red-before/green-after regression pair. The
  red-before half is recorded in `svgcontrol-guard-fail-before.2026-09-02T10-30.md`, which named
  both `SVGControl.Test.Form1` and `SVGControl.Test.Form2`.
- The guard now finds an empty `Form`-derived set, so `string.Join(", ", formDerivedTypeNames)`
  yields an empty string and `BeEmpty` passes without rendering the `because` message at all.
