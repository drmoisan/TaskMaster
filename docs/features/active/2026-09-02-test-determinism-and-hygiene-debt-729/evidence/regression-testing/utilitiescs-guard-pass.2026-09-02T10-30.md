# UtilitiesCS.Test structural guard — green-from-birth run (P4-T6)

Timestamp: 2026-09-02T23-28

Command: `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~NoLiveFormInTestAssemblyTests"`

EXIT_CODE: 0

PassedCount: 1

FailedCount: 0

## Test node

`UtilitiesCS.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` — `Passed`

Output Summary:

- `Test Run Successful.` `Total tests: 1` / `Passed: 1`.
- This guard is green-from-birth. It is regression *prevention*, not a fail-before/pass-after
  regression test, and no reviewer should expect a red run for it.
- The reason is structural: `UtilitiesCS.Test/UtilitiesCS.Test.csproj` uses an explicit
  `<Compile Include>` list with no wildcard globbing, and the `Form1.cs`, `Form2.cs`, `Form3.cs`
  and `ResourceTests.cs` sources were never listed in it. `UtilitiesCS.Test` therefore compiles
  zero `Form`-derived types today and compiled zero before this change, so no red state exists or
  can be produced in this assembly.
- The orphan sources were deleted (P4-T2) rather than left on disk, because leaving them would
  keep a live `Form` one csproj line away from re-entering the assembly.
