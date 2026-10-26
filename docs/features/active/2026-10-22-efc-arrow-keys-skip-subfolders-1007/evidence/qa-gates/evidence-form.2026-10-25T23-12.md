# Evidence Form (Issue #1007, P19-T7)

Timestamp: 2026-10-25T23-12
Command: git diff --name-status <merge-base of origin/main and HEAD> plus git status --porcelain --untracked-files=all (the P18-T1 path set, re-checked); file listing of `<FEATURE>/evidence/`; ripgrep over `<FEATURE>/evidence/` for `[A-Za-z]:[\\/]`, `Users[\\/]` and the account-name tokens
EXIT_CODE: 0
Output Summary: every check holds.
- Of the 91 changed or added paths, none ends in `.trx`, `.coverage` or `.xml`, and none contains `cobertura` or `jacoco`. The four project files end in `.csproj`.
- `<FEATURE>/evidence/` holds 48 files, and every one is Markdown (`.md`).
- The search of `<FEATURE>/evidence/` for a drive-rooted path or a `Users` profile segment returns 0 matches. Scratch locations are recorded as the token `SCRATCH`.
- Raw documents stay out of the commit. The trx, the Cobertura document and the JaCoCo projection are under the ignored `coverage/` folder or in SCRATCH. The feature-review copy `artifacts/csharp/coverage.xml` is git-ignored and untracked (coverage-artifact.2026-10-25T23-11.md).

| Check | Result |
|---|---|
| Added paths ending in .trx, .coverage or .xml | 0 |
| Added paths containing cobertura or jacoco | 0 |
| Non-Markdown files under `<FEATURE>/evidence/` | 0 of 48 |
| Drive-rooted path or `Users` profile segment in `<FEATURE>/evidence/` | 0 matches |

Superseded artifacts: `final-toolchain-format`, `final-toolchain-analyzers`, `final-toolchain-typecheck` and `final-toolchain-tests` each have earlier timestamped copies from passes that the P19-T5 coverage remediation restarted. A consumer selects the lexicographically greatest file of each name, which is the 23-07 or 23-09 clean pass.
