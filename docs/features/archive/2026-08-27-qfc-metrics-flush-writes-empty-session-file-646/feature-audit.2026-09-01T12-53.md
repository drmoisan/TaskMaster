# Feature Audit — Issue #646 (qfc-metrics-flush-writes-empty-session-file)

Timestamp: 2026-09-01T12-53

| Field | Value |
|---|---|
| Branch | `bug/qfc-metrics-flush-writes-empty-session-file-646` |
| HEAD | `0fe0668f146236c65aa93514fcb9756d366a6940` |
| Baseline | `origin/main` at `8996b28746d32f9f5996a037e0ca76be78b7684d` |
| Work mode | `minor-audit` |
| AC source | `issue.md`, section `## Acceptance Criteria` only |
| AC count | 8 (AC1-AC8) |
| Blocking findings | **0** |

## AC Source Resolution

`issue.md` line 12 carries the marker `- Work Mode: minor-audit`. Under the
`acceptance-criteria-tracking` protocol this resolves the sole AC source to `issue.md`, and
within it to the explicit `## Acceptance Criteria` heading at line 114. That section contains
eight checkbox items in the required `- [x] ACn:` form.

`spec.md` and `user-story.md` are absent. Under `minor-audit` they are not AC sources, so their
absence is correct by design and is not recorded as a gap. No other checkbox section of
`issue.md` — including `Logs / Screenshots`, `Impact / Severity`, `Proposed Fix / Validation
Ideas`, and `Next Step` — was treated as an acceptance criterion.

## Verification Method

Every AC was verified against primary evidence rather than accepted on the strength of its
checkbox. Where the evidence artifact stated a figure, this reviewer re-derived that figure
independently from the tree or the committed data. Independent re-derivations performed:

- `git diff --shortstat origin/main...HEAD` -> 31 files, 3223 insertions, 0 deletions.
- Mechanical inverse-prefix filter over `git diff --name-only origin/main...HEAD` -> zero paths
  outside the three AC7-allowed prefixes (grep exit 1, empty output).
- `git status --porcelain` -> empty.
- `wc -l` and `awk NR` on both changed source files -> 231 and 477 lines.
- Re-summed `LINE` counters in both committed JaCoCo projections -> 48426/142226 baseline and
  48436/142240 final, matching the recorded Cobertura root counters exactly.
- Re-derived the first-party subset from the final projection -> 14540 of 62121, matching the
  recorded 23.4059%.
- Read the delivered guard and the EFC reference guard directly from the working tree.
- `git diff --name-only 10aaaf65 HEAD -- "*.cs"` -> empty, confirming no source change after the
  toolchain gates ran.

## Acceptance Criteria Evaluation

| AC | Criterion (abridged) | Evidence | Independent check | Verdict |
|---|---|---|---|---|
| AC1 | `WriteMetricsAsync` returns without invoking `MetricsFileWriter` when the filtered array is empty | `evidence/regression-testing/fail-before-new-test...md`, `evidence/regression-testing/pass-after-new-test...md` | Read lines 174-188 of the delivered file; the `return;` precedes the `await MetricsFileWriter(...)` with no intervening statement | **PASS** |
| AC2 | Guard is an early return between the filter statement and the await, textually equivalent to the EFC guard | `evidence/other/production-diff-scope...md` | Compared both guards in the tree: QFC lines 175-178 against EFC lines 72-75; identical but for the array identifier, which AC2's own `if (<array>.Length == 0) { return; }` form contemplates | **PASS** |
| AC3 | New MSTest test stubs `GetMoveDiagnostics` to an all-null-or-whitespace array and asserts zero writer invocations | `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` lines 452-474 | Read the test: `BuildLooseMetricsController(new[] { "   ", null, "\t" })` — every element null or whitespace — and `invoked.Should().BeFalse(...)` | **PASS** |
| AC4 | Test fails against the unguarded implementation and passes after, with fail-before evidence under `evidence/regression-testing/` | `evidence/regression-testing/fail-before-new-test...md` (exit 1), `pass-after-new-test...md` (exit 0) | Both artifacts present at the required path; the RED run shows a genuine 346 ms assertion failure with the test's own `because` text, not an empty message or a sub-millisecond load failure | **PASS** |
| AC5 | The two named pre-existing tests still pass and are not modified | `evidence/regression-testing/existing-tests-pass...md` (`Passed: 2`), `evidence/other/test-file-diff-scope...md` | `git diff --numstat` on the test file at HEAD reports 23 insertions and **0 deletions**, a single pure-insertion hunk after old line 451; a zero-deletion diff cannot have altered either named test | **PASS** |
| AC6 | `MetricsFileWriter` signature and the writer failure-handling branch unchanged | `evidence/other/production-diff-scope...md` | The production diff is one hunk spanning old lines 172-177. The delegate declaration sits at lines 28-34 and the `if (!metricsWritten)` branch at 189-195; both are outside the hunk and both read unchanged in the tree | **PASS** |
| AC7 | No repository file outside the two owned source files and this feature folder is modified | `evidence/qa-gates/footprint-scope...md` | Re-run at HEAD `0fe0668f`: 31 diff paths, inverse-prefix filter returns zero results, working tree clean. The executor's artifact recorded 29 paths at the earlier HEAD `ba134b57`; the growth to 31 is the two JaCoCo projections and the substitution record, all inside the third allowed prefix | **PASS** |
| AC8 | C# toolchain passes in order in a single final pass | Five `evidence/qa-gates/` artifacts, each with command, exit code, and verbatim summary | Order verified as format (P2-T1) -> check (P2-T2) -> analyzer (P2-T3) -> nullable (P2-T4) -> vstest (P2-T5), all exit 0, uninterrupted after the mandated pass-1 restart. Confirmed still valid at HEAD: no `.cs` file changed after the gated commit | **PASS** |

## Notes on Individual Criteria

**AC2 — textual equivalence.** The two guards differ only in the array identifier (`lines`
against `dataLines`). AC2 states the required form as `if (<array>.Length == 0) { return; }`
with the array name as a placeholder, so identifier divergence is what the criterion expects
rather than a shortfall. The structural relationship also matches: in both files the guard
directly follows its computing statement with no blank line between them, and is followed by a
blank line before the next construct.

**AC4 — the RED result is genuine.** This is the criterion most easily satisfied vacuously, so
it received the closest scrutiny. Four independent signals confirm the failure was a real
assertion rather than a harness or assembly-load failure: the test ran 346 ms rather than
sub-millisecond; the failure message is the test's own `because` string rather than empty; the
reported value (`invoked` was `True`) is the defect itself; and the fixture supplied
`MyDocuments`, so the pre-existing folder guard neither caused nor masked the result. The GREEN
run used a byte-identical `/TestCaseFilter` against the same assembly path, isolating the guard
as the only changed variable.

**AC7 — evidence recorded at an earlier HEAD.** The footprint artifact was written at HEAD
`ba134b57` and records 29 paths; HEAD is now `0fe0668f` with 31. The artifact anticipates this
and states that the later coverage-substitution pass adds two projections and one record inside
the allowed feature-folder prefix. This reviewer re-ran the mechanical check at the current HEAD
rather than relying on the recorded count, and it passes: zero paths outside the allowed set.

An intermediate commit on this branch (`9f578b3c`) added two files under
`.claude/agent-memory/`, which is outside the allowed set, and a later commit (`8a2054cd`)
removed them. `git diff --stat origin/main...HEAD -- .claude` returns empty, so the merged tree
gains nothing there. AC7 is evaluated against the branch diff, which is the reading the
criterion's own wording and the plan's gate both use, and it holds. Recorded for transparency
in `code-review.2026-09-01T12-53.md` as finding CR-6.

**AC8 — coverage evidence substitution does not weaken the gate.** The `vstest` gate's coverage
artifacts were converted from raw Cobertura to package-level JaCoCo projections after the gate
completed. The three tasks that read the raw reports (P0-T11, P2-T6, P2-T7) had already
evaluated their acceptance conditions against them. This reviewer re-summed the projections and
reproduced the recorded root counters exactly on both sides, so every figure those gates quoted
remains verifiable from the committed evidence. The gate sequence is auditable.

## Baseline Behaviour Comparison

| Scenario | `origin/main` behaviour | HEAD behaviour | Matches issue's Expected Behavior |
|---|---|---|---|
| Filtered diagnostic array is empty | `MetricsFileWriter` invoked; the default append writer creates the session-metrics file if absent or updates its last-write time if present, recording nothing | `WriteMetricsAsync` returns before the writer; no file created, no file touched | Yes |
| Filtered array is non-empty | Writer invoked once with the filtered lines | Unchanged — writer invoked once with the filtered lines | Yes (no regression) |
| Input contains a mix of valid and null/whitespace entries | Writer receives only the valid entries | Unchanged — writer receives only the valid entries | Yes (no regression) |
| `MyDocuments` absent | Returns before the writer via the pre-existing folder guard | Unchanged | Yes (no regression) |

The defect described in the issue is resolved and the three adjacent behaviours are preserved,
each held by a passing test. The full suite moved from 1284 passing to 1285 passing with zero
failures, and the `+1` is exactly the test added here.

One behaviour is deliberately not changed: the Outlook calendar appointment written by
`WriteMoveToCalendar` at line 154 still occurs in the empty-diagnostics case, because that call
precedes the guard. The issue's Expected Behavior is scoped to the metrics file and AC1 is
scoped to `MetricsFileWriter`, so this is correct against the criteria as written. It is
recorded as finding CR-2 for a follow-up decision.

## Check-Off Reconciliation

All eight criteria were already checked `- [x]` in `issue.md` before this review. Each was
re-verified against its evidence, and all eight are supported. No criterion was found checked
without support, so no correction to `issue.md` is required and none was made. No criterion was
added, reworded, or unchecked by this review.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/issue.md
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none
```

## Verdict

**PASS — 8 of 8 acceptance criteria verified against evidence. 0 blocking findings.**

The delivered change resolves the reported defect with a four-line guard and one regression
test, backed by a genuine fail-before / pass-after pair and a clean five-gate toolchain
sequence. The non-blocking findings raised in `code-review.2026-09-01T12-53.md` and the coverage
provisioning gaps recorded in `policy-audit.2026-09-01T12-53.md` do not affect any acceptance
criterion and none is remediable within AC7's footprint restriction.
