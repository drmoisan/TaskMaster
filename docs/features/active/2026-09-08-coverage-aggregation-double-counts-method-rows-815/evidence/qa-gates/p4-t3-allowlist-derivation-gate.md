# P4-T3 — AC4 Hard-Coded-Allowlist Gate

Timestamp: 2026-09-09T11-15
Task: [P4-T3]
EXIT_CODE: 0

## Command 1 — no hard-coded production assembly name under `scripts/vscode/`

Command: `git grep -c -F -e 'VBFunctions' -- scripts/vscode docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/plan.2026-09-07T20-14.md`
EXIT_CODE: 0

```
docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/plan.2026-09-07T20-14.md:3
```

Exactly one line. Its path is the 2026-09-07 plan document, which is the positive control: that
document carries the pinned nine-name literal, so a hit there proves the search reports an
occurrence when one is present. Its count is **3**, which equals the baseline integer recorded in
`evidence/baseline/p0-t10-descendant-axis-baseline.md`, so the historical document is unmodified by
this feature. **No line of the output names any path under `scripts/vscode/`**, so the delivered
production code contains no literal list of production assembly names.

## Command 2 — the delivered function names the derived allowlist helper

Command: `git grep -c -F -e 'Get-KoverageProjectAllowlist' -- scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1`
EXIT_CODE: 0

```
scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1:4
```

One line with count 4, which is 1 or greater. The four occurrences are the parameter default on
`Get-CoberturaFirstPartyCoverageSummary`, the parameter default on
`Get-CoberturaFirstPartyCoverageReport`, and one mention in each of their comment-based help blocks.
Test T-C, `defaults the ProjectNames parameter to Get-KoverageProjectAllowlist`, reads the parameter
default from the function AST and asserts its extent text is exactly `(Get-KoverageProjectAllowlist)`;
it passes. See `evidence/regression-testing/p3-t1-pass-after.md`.

## Search scope, and the inherited baseline finding it excludes

The search path deliberately excludes `tests/scripts/vscode`. AC4's stated scope names
`scripts/vscode/` only, and the tests tree carries a pre-existing occurrence of the literal that is
outside that scope:

```
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:190:                'C:\repo\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll'
```

That is a test-data assembly path, not an allowlist literal. It predates this issue, is recorded here
as an inherited baseline finding, and is **not modified by this feature**. The new test file
`tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1` contains no production
assembly name at all, because plan decision D9 replaced the pinned snippet's nine-name literal with
a parameter in the differential helper.

Output Summary: The literal search over `scripts/vscode` and the pinned historical plan returns
exactly one line, naming the 2026-09-07 plan document with count 3, matching the recorded P0-T10
baseline, and naming no path under `scripts/vscode/`. The search for the derived allowlist helper in
the new production file returns one line with count 4. AC4 is discharged: the allowlist is derived
from `Get-KoverageProjectAllowlist`, not hard-coded, and a passing test asserts the parameter default
and a second passing test asserts that a package outside an explicit override enters neither the
numerator nor the denominator.
