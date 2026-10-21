# Coverage Reconciliation for `Sync-PackageReferences.ps1` — R2

- Timestamp: 2026-09-20T08-46-30
- Task: [P1-T11]
- Finding: R2
- EXIT_CODE: 0

This artifact exists so the next reader reads the arithmetic rather than re-deriving it. It is a
**consumer** of the two coverage documents; it read no coverage document itself and therefore
carries no gate rule 12 standing-in statement. The two artifacts it cites do.

## The Three Figures

| Figure | Source artifact | Value |
|---|---|---|
| Before | `evidence/remediation-baseline/p0-t8-pester.2026-09-20T01-37.md` | **95 of 127, 74.80 percent** |
| After | `evidence/qa-gates/p1-t10-pester-coverage.2026-09-20T01-37.md` | **104 of 127, 81.89 percent** |
| Delta | — | **+9 lines, +7.09 percentage points** |

All three are numbers. None is a placeholder. The instrumented denominator is 127 in both, because
Phase 1 edited no production file.

## Decision D5, Restated With Its Arithmetic

`scripts/vscode/Sync-PackageReferences.ps1` measured **95 covered of 127 instrumented, 74.80
percent**. Of its 32 uncovered lines:

| Class | Lines | Count |
|---|---|---|
| `Get-PackageSyncSeam` delegate table | 60 through 91 | 19 |
| Pure logic, every one a negative or error path | 151, 180, 248, 290, 293, 330, 336, 337, 345 | 9 |
| Top-level invocation | 387, 390, 410, 422 | 4 |

Covering all nine logic lines gives **104 of 127, which is 81.89 percent**. That is the measured
outcome and it is the planned one.

Reaching 85 percent requires one of exactly two things, and both are prohibited here.

**Executing the top-level invocation.** The four uncovered entry lines run only when the file is
invoked rather than dot-sourced. At that point `Invoke-PackageReferenceSync` runs the production
seam against a real tree, enumerating and rewriting real project files.
`.claude/rules/general-unit-test.md` prohibits external dependencies in unit tests and prohibits
temporary files in tests outright, and no injected seam reaches a top-level invocation: the
invocation guard at the end of the file constructs the production seam itself.

**Excluding the 19-line delegate table from measurement.** The Coverage Exclusion Policy in
`.claude/rules/general-unit-test.md` states that **no production file may be excluded from
coverage measurement**, and that the correct response to untestable lines is to refactor rather
than to exclude. `remediation-inputs.2026-09-20T01-37.md` states the same thing in terms for this
specific file: "Nineteen uncovered lines are the `Get-PackageSyncSeam` delegate table and four are
the top-level invocation; those legitimately remain in the denominator under the Coverage
Exclusion Policy and must not be excluded."

The delegate table is already the correct shape the policy asks for. It is the thinnest possible
wiring: every member is a one-line or two-line scriptblock that calls exactly one filesystem
cmdlet or one reflection API, and all repair logic sits outside it. Its uncovered lines are the
visible cost the policy intends to leave visible.

## The Post-Change Figure Against Both Floor Readings

| Floor reading | Source | Value | 81.89 clears it? |
|---|---|---|---|
| Authoritative for this cycle | `CLAUDE.md:304`, settled by the project maintainer on 2026-09-11 under issue #563 | 80 | **yes** |
| Superseded upstream boilerplate | `.claude/rules/general-unit-test.md:23` | 85 | no |

Per **gate rule 13** the `CLAUDE.md` figure is authoritative for this cycle. The 80-versus-85
conflict is **open issue #668** and this cycle does not resolve it. The shortfall against the 85
reading is recorded here rather than omitted, because omitting it would misrepresent the outcome
to a reader who takes the rules file as authoritative.

## The Scenario-Completeness Clause Is Discharged by the Tests, Not by the Percentage

The review recorded R2 as two readings of one defect: a **coverage figure** of 74.80 percent and a
**scenario-completeness failure** under `.claude/rules/general-unit-test.md`, which requires
negative flows, edge cases and error-handling behaviour independently of any percentage.

The scenario-completeness clause is discharged by the **eight named tests**, not by the
percentage:

| Test | Scenario class |
|---|---|
| [P1-T2] `R2- returns no identifier when the restore folder matches no manifest package` | negative flow, unresolvable input |
| [P1-T3] `R2- returns no asset folder when the library directory is absent` | error handling, absent resource |
| [P1-T4] `R2- warns and records no repair when no asset folder the target framework can consume ships the file` | error handling, the #902 rejection |
| [P1-T5] `R2- returns the project text unchanged when no Reference names the assembly` | negative flow, no match |
| [P1-T6] `R2- returns the project text unchanged when the Reference already names the resolved version` | edge case, idempotence boundary |
| [P1-T7] `R2- skips the manifest directory when no project file sits beside it` | edge case, empty collection |
| [P1-T8] `R2- skips the project with a warning when merge conflict markers are present` | error handling, hostile input |
| [P1-T9] `R2- returns an unskipped result with no fix when no hint path needs repair` | edge case, the second zero-fix state |

Eight tests, nine lines: [P1-T8] discharges 336 and 337 together because the warning and its skip
return are one behaviour.

## Output Summary

Before 74.80 percent, after **81.89 percent**, delta **+7.09 points** on an unchanged
127-line denominator. The post-change figure clears the authoritative floor of 80 and does not
clear the superseded 85, which is open issue #668. Reaching 85 would require either executing the
top-level invocation against a real tree or excluding the delegate table, and both are prohibited
by `.claude/rules/general-unit-test.md`. The scenario-completeness half of R2 is discharged by the
eight named tests.
