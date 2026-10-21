# Acceptance-Criteria Status Summary — Issue #285

Timestamp: 2026-09-01T08-31

- **Work Mode:** `full-bug`
- **AC source (sole):** `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/spec.md`, `## Acceptance Criteria` heading
- No `user-story.md` exists for this item and none was created.

## Per-Identifier Rows

| ID | Checked | Evidence artifact cited |
| --- | --- | --- |
| AC1 | `- [x]` | `evidence/regression-testing/p1-t6-red-new-test.md`; `evidence/qa-gates/p3-t9-test-hygiene.md` |
| AC2 | `- [x]` | `evidence/regression-testing/p2-t4-green-new-test.md` |
| AC3 | `- [x]` | `evidence/qa-gates/p3-t9-test-hygiene.md` |
| AC4 | `- [x]` | `evidence/regression-testing/p2-t5-at-risk-tests.md`; `evidence/regression-testing/p2-t6-additive-only-diff.md` |
| AC5 | `- [x]` | `evidence/qa-gates/p3-t8-source-census.md` |
| AC6 | `- [x]` | `evidence/qa-gates/p3-t8-source-census.md` |
| AC7 | `- [x]` | `evidence/qa-gates/p3-t8-source-census.md` |
| AC8 | `- [x]` | `evidence/qa-gates/p3-t2-format-check.md` |
| AC9 | `- [x]` | `evidence/qa-gates/p3-t3-analyzer-build.md` |
| AC10 | `- [x]` | `evidence/qa-gates/p3-t4-nullable-build.md` |
| AC11 | `- [x]` | `evidence/qa-gates/p3-t5-vstest-utilitiescs.md`; `evidence/qa-gates/p3-t6-vstest-quickfiler.md`; `evidence/qa-gates/p3-t7-coverage.md` |
| AC12 | `- [x]` | `evidence/qa-gates/p3-t11-footprint.md` |

## Per-Identifier Detail

### AC1 — new MSTest method exists, with captured failure output

`p1-t6-red-new-test.md` records `Total tests: 1`, `Failed: 1`, `Passed: 0`, `EXIT_CODE: 1` against
`ExpectedExitCode: 1`, with the failure text
`System.Threading.Tasks.TaskCanceledException: A task was canceled.` escaping the `await`.
`p3-t9-test-hygiene.md` records a test-name count of 1.

**Declared deviation from AC1's literal wording `against unmodified production code`.** The P1-T6 red
run was taken with the determinism seam present (P1-T1, P1-T2) and the defective
`catch (TimeoutException)` clause untouched. The P1-T1 acceptance measurement, taken immediately
after those edits and before any handler change, recorded an anchored `catch (TimeoutException)`
count of **4** — unchanged from the P0-T12 baseline of 4 — and a filtered-clause count of **0**. The
handler under test was therefore the original defective one. The deviation is justified because the
seam is behaviour-preserving by construction: `timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))`
with the parameter defaulted to `null` produces the identical `CancellationTokenSource` the
pre-change line produced, and all seven existing call sites bind unchanged. The seam is
unavoidable in this order because the regression test binds `timeoutSourceFactory` by name and
cannot compile until the parameter exists.

### AC2 — test passes after the fix

`p2-t4-green-new-test.md` records `Total tests: 1`, `Passed: 1`, `Failed: 0`, `EXIT_CODE: 0`. The
three assertions the test carries all held: `result.Should().Be("result-42")`;
`delegateCalls.Should().Be(1)`; `factoryCalls.Should().Be(2)`.

### AC3 — banned APIs absent, caller token and timeout value as specified

`p3-t9-test-hygiene.md` records zero counts for `Task.Delay`, `Thread.Sleep`, and `Thread.SpinWait`;
a count of 1 for `milliseconds: 30_000`; and a file-level `CancellationToken.None` count of **17**
against the P0-T12 baseline of **16**, which is the evidence for AC3's caller-token clause.

### AC4 — both at-risk tests pass with unchanged bodies

`p2-t5-at-risk-tests.md` records `Total tests: 2`, `Passed: 2`, `Failed: 0`.
`p2-t6-additive-only-diff.md` records an **empty diff** for `TimeOutTask_AdditionalTests.cs` against
the merge base, and a **deletion-free diff** for `TimeOutTask_OverloadCoverageTests.cs` (zero lines
beginning with a single `-` once the `--- a/` header is excluded; hunk header `@@ -383,5 +383,45 @@`).

### AC5 — catch-clause census

`p3-t8-source-census.md` records **9** `catch (TaskCanceledException)`, **3** `catch (TimeoutException)`,
**10** `catch (System.Exception e)`, and **1** filtered clause.

### AC6 — no widening to `OperationCanceledException`

`p3-t8-source-census.md` records an `OperationCanceledException` count of **0**.

### AC7 — seam declared on both members and forwarded

`p3-t8-source-census.md` records a parameter-literal count of **4**, a coalesce-literal count of
**2**, and a bare-token count of **10**.

### AC8 — formatting

`p3-t2-format-check.md` records `EXIT_CODE: 0` and an unformatted-file count of **0** across 1565
files.

### AC9 — analyzer build

`p3-t3-analyzer-build.md` records `0 Error(s)` and a warning count of **5**, no greater than the
P0-T7 baseline of **5** (delta 0), with zero diagnostics naming either changed file.

### AC10 — nullable build

`p3-t4-nullable-build.md` records `0 Error(s)` and a quoted command line containing
`TreatWarningsAsErrors=true` and free of `Nullable=enable`.

### AC11 — zero failures and changed lines covered

`p3-t5-vstest-utilitiescs.md` records `Passed: 4771`, `Failed: 0`.
`p3-t6-vstest-quickfiler.md` records `Passed: 1272`, `Failed: 0`.

**Both BASELINE_FAILURE_SETs recorded in Phase 0 were EMPTY** (P0-T10 cardinality 0; P0-T11
cardinality 0). AC11's literal wording `0 failures` is therefore **literally met** with no
pre-existing failures to name. **No `REMEDIATION-REQUIRED` entry arises from AC11.**

`p3-t7-coverage.md` records the changed-line hit counts. L_FILTER = 217, L_GUARD = 219, L_CTOR = 199.

- Modified catch clause: recorded hit count at L_GUARD = **1** (greater than 0), proving the widened
  clause body executed.
- Modified timeout-source construction: recorded hit counts at lines 199, 200, 201 = **1, 1, 1**.
- **A `<line>` element exists at L_FILTER (line 217) and its recorded hit count is 1.** L_GUARD is
  used as the coverage proxy for the filter clause because a `when` filter expression may emit no
  `<line>` element of its own; the clause body executing proves the filter matched. Here both the
  proxy and the direct reading agree.

### AC12 — footprint

`p3-t11-footprint.md` is cited.

**Full two-source exclusion set restated: `.claude/agent-memory/` plus the P0-T6 unformatted-file
list, and nothing else.**

**Cardinality of the P0-T6 unformatted-file list: 0.** P0-T6 recorded the tree as already fully
formatted with an empty unformatted-file list, so that source contributes no exclusion.

Every excluded entry, enumerated by full path with its source:

| # | Full path | Source |
| --- | --- | --- |
| 1 | `.claude/agent-memory/orchestrator/MEMORY.md` | `.claude/agent-memory/` |
| 2 | `.claude/agent-memory/orchestrator/check-ignore-false-negative-on-directory-glob.md` | `.claude/agent-memory/` |
| 3 | `.claude/agent-memory/orchestrator/feature-folder-order-hook-is-workmode-blind.md` | `.claude/agent-memory/` |

Zero entries were excluded from the P0-T6 unformatted-file list source, because that list is empty.
The three excluded entries are tracked agent-memory files changed by commits already on this branch
between the merge base and HEAD; they do not appear in `git status --porcelain`, confirming they are
committed rather than pending, and they are not part of this item's code change.

**The plan's not-met condition for AC12 does not trigger.** That condition requires the P0-T6
unformatted-file list to have been non-empty with one of its paths appearing in the P3-T11 output.
The list is empty, so no such path exists. **No `REMEDIATION-REQUIRED` entry arises from AC12.**

After applying the exclusion set, the union of the two P3-T11 outputs contains only
`UtilitiesCS/Threading/TimeOutTask.cs`,
`UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`, and paths under
`docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/`.

## REMEDIATION-REQUIRED Entries

**None.** No acceptance criterion was left unchecked, and no evidence failed to support the bullet it
was cited for.

## Recorded, Not Remediated (out of scope, no AC impact)

`p3-t10-file-size-audit.md` records that `UtilitiesCS/Threading/TimeOutTask.cs` is 1011 lines and
exceeds the 500-line ceiling in `.claude/rules/general-code-change.md`. The file already breached the
ceiling at the merge base with 993 lines; this change accounts for 18 of them. The breach cannot be
corrected inside this item's scope boundary, because splitting the file would create a path outside
the three permitted paths. It is a candidate follow-up issue alongside the spec's five existing
Non-Goals. No acceptance criterion covers file size, so this does not affect any check-off above.

## Closing Counts

- **Total AC items: 12**
- **Checked off (delivered): 12**
- **Remaining (unchecked): 0**
- **Items remaining: none**

12 checked + 0 unchecked = **12**, which sums to the total.

Verified directly against the `## Acceptance Criteria` section of `spec.md` by counting checkbox
lines within that section: `CHECKED=12`, `UNCHECKED=0`, `TOTAL=12`. The closing counts agree with the
actual `- [x]` count in the source file.

Acceptance: met. This artifact contains exactly 12 rows with the identifiers AC1 through AC12
appearing once each in the per-identifier table, and the closing counts sum to 12 and agree with the
actual `- [x]` count under the `## Acceptance Criteria` heading of `spec.md`.
