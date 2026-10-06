# P5-T2 AC2 check-off

Timestamp: 2026-10-06T18-36
Command: Grep tool over spec.md `^- \[x\] AC2 ` and `^- \[[ x]\] AC[0-9]+ ` (count); Read of the named artifact
EXIT_CODE: 0
Output Summary: AC2 met and checked off. The negative-control run was taken after the sweep and the ADAL deletion and before any packages.config or csproj edit. The count assertion passed and the unverifiable-set assertion failed naming System.Linq.AsyncEnumerable.

Artifact read:
- evidence/regression-testing/binding-redirect-gate-fail-before-unverifiable.md: EXIT_CODE 1, ExpectedExitCode 1. PRECONDITION porcelain over `*packages.config` and `*.csproj` is empty. The main It message begins `Expected 'netstandard'` rather than `Expected 0`, so the count assertion passed; it ends `, but got @('netstandard', 'System.Linq.AsyncEnumerable').` and does not name ADAL.

Clauses verified: timing (after Parts A and B, before Part C); count assertion passed; unverifiable failure names System.Linq.AsyncEnumerable.
SPEC-LINE: `- [x] AC2 (install half has its own negative control).` (criterion text unchanged)
