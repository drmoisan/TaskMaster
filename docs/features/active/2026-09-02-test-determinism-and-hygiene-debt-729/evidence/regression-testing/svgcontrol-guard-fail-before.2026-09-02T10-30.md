# SVGControl.Test structural guard — red-before run (P3-T4, expect-fail)

Timestamp: 2026-09-02T23-24

Command: `& $vstest SVGControl.Test\bin\Debug\SVGControl.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~NoLiveFormInTestAssemblyTests"`

EXIT_CODE: 1

ExpectedExitCode: 1

PassedCount: 0

FailedCount: 1

## Test node

`SVGControl.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType`

## Failure text (verbatim error message line)

```text
Expected formDerivedTypeNames to be empty because a unit-test assembly must not compile a live System.Windows.Forms.Form type, but found: SVGControl.Test.Form1, SVGControl.Test.Form2, but found at least one item {"SVGControl.Test.Form1"}.
```

Output Summary:

- `A total of 1 test files matched the specified pattern.` `Total tests: 1` / `Failed: 1` / `Test Run Failed.`
- This is the fail-before (red-before) evidence for Finding 2. It is the expected outcome for this
  task only; `ExpectedExitCode: 1` is declared above so the gate normalizes to `pass`.
- The failure text contains the token `SVGControl.Test.Form1` and the token
  `SVGControl.Test.Form2`, satisfying this task's acceptance and spec.md AC11's requirement that
  the red-before run name the two `Form`-derived types.
- Both tokens appear because revision round 16 changed Block E's `because` argument to append
  `string.Join(", ", formDerivedTypeNames)`. FluentAssertions 8.10.0 renders the collection
  itself as one representative item — visible above as `but found at least one item
  {"SVGControl.Test.Form1"}` — so without the enumeration in the `because` string the second type
  could never be named. The types compiled into the assembly are exactly the two named; the
  representative-item rendering was a message-shape limitation, not a difference in what the guard
  found.
- Stack trace omitted from this artifact because it carries an absolute host path.
