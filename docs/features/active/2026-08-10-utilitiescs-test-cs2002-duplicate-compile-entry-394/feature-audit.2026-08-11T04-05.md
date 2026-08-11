# Feature Audit — utilitiescs-test-cs2002-duplicate-compile-entry-394 (Reaudit — Remediation Cycle 1 Exit Gate)

- **Issue:** #394
- **Branch:** `bug/utilitiescs-test-cs2002-duplicate-compile-entry-394` (HEAD `f39c6fc9`)
- **Timestamp:** 2026-08-11T04-05
- **Entry feature-audit (unmodified, retained):** `feature-audit.2026-08-10T23-45.md`

## Scope and Baseline

- **Base branch (resolved):** `origin/epic/build-ci-coverage-gate-fidelity-integration`
- **Diff command:** `git diff origin/epic/build-ci-coverage-gate-fidelity-integration...HEAD`
- **Work mode:** `full-bug` — authoritative AC source is `spec.md` `## Acceptance Criteria` only.

## AC Count Correction

The reaudit directive notes a discrepancy: "the prior review counted 8 items and the executor counted 7." Direct inspection of `spec.md`'s `## Acceptance Criteria` section (lines 316-348) shows **8** checkbox items, all `[x]`:

1. Exactly one `<Compile Include>` item remains.
2. Fail-before evidence via `/t:Rebuild`.
3. Post-change build emits no CS2002.
4. `PercentageFormatterTests` test count unchanged at 7 (numerically recorded).
5. Duplicate sweep recorded across every item type.
6. Diff touches only the csproj plus feature-folder docs/evidence.
7. Full toolchain pass for applicable stages.
8. Docs/config references updated and mutually consistent.

`issue.md`'s mirrored list (lines 107-117) contains only 6 items — it is a condensed convenience mirror and explicitly is **not** the authoritative source for `full-bug` mode (`spec.md` is, per `acceptance-criteria-tracking` and `issue.md`'s own work-mode note). No document in the feature folder was found asserting a count of 7 for the authoritative `spec.md` list. **The correct, authoritative count is 8**, matching the entry feature-audit (`feature-audit.2026-08-10T23-45.md`), not a lower count.

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/spec.md`, `## Acceptance Criteria` (8 items, all currently checked `[x]`) — unchanged in content and text since the entry audit; only the Root Cause Analysis table (not the AC list itself) was edited by the remediation cycle.

## Acceptance Criteria Evaluation

| # | Status | Evidence | Notes |
|---|---|---|---|
| 1 | PASS | `git diff` hunk shows exactly one deleted `<Compile Include>` line (former line 356), unchanged since the entry audit; the remediation cycle touched zero lines of the csproj | Re-confirmed this cycle: `git diff origin/epic/build-ci-coverage-gate-fidelity-integration...HEAD -- UtilitiesCS.Test/UtilitiesCS.Test.csproj` shows `1 -`, `0 +`. |
| 2 | PASS | `evidence/baseline/fail-before-cs2002.2026-08-10T22-31.md` — unchanged, genuine `/t:Rebuild`, CS2002 present, EXIT_CODE 0 | Not affected by the remediation cycle. |
| 3 | PASS | `evidence/qa-gates/post-fix-cs2002.2026-08-10T22-31.md` — unchanged, identical `/t:Rebuild` command, CS2002 absent | Not affected by the remediation cycle. |
| 4 | PASS | `evidence/baseline/baseline-test-count.2026-08-10T22-31.md` (7/7 passed) and `evidence/regression-testing/post-fix-test-count.2026-08-10T22-31.md` (7/7 passed) | Not affected by the remediation cycle. |
| 5 | PASS | `evidence/baseline/duplicate-sweep.2026-08-10T22-31.md` — the sweep's findings (exactly one duplicate, `Compile`/`PercentageFormatterTests.cs`; zero duplicates elsewhere) are unchanged. The retained-script policy gap that the entry audit separately raised (not an AC5 defect, per entry audit's own reasoning) is now closed: `duplicate-sweep.ps1` is deleted, and `spec.md`'s reported figures (`Analyzer`=11, `Reference`=126, `packages.config`=105) now match the sweep evidence exactly, resolving the entry code-review's Minor documentation-accuracy finding as well | Improved since entry audit: the entry audit's Minor finding (stale figures) is now resolved, and the separate policy-level PowerShell finding no longer applies to any language on this branch. |
| 6 | PASS | Re-derived independently this cycle: `git diff --name-only origin/epic/build-ci-coverage-gate-fidelity-integration...HEAD` (33 files) contains only `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (1 deletion, 0 insertions), one `.claude/agent-memory/**` subagent memory-index append (an expected delegation byproduct, not a scope violation — verified as a genuine 2-line append, see policy-audit reaudit Section 3), and this feature folder's own docs/evidence files. No `CLAUDE.md`, `.claude/rules/**`, or `scripts/**` path appears | The unplanned `.ps1` file that WAS present at the entry audit (and textually permitted by AC6's literal wording, per the entry feature-audit's reasoning) is now removed entirely, so AC6 is satisfied both by the literal AC text and by the stricter policy-level coverage-verification rule that the entry policy-audit separately enforced. |
| 7 | PASS | `evidence/qa-gates/csharpier-not-applicable.2026-08-10T22-31.md`, `evidence/qa-gates/nullable-gate-not-run.2026-08-10T22-31.md`, `evidence/qa-gates/solution-rebuild.2026-08-10T22-31.md` (unchanged), plus new `evidence/qa-gates/analyzer-not-applicable.2026-08-10T23-45.md` added this cycle | Entry feature-audit's non-blocking observation (missing dedicated analyzer-not-applicable artifact) is now resolved — evidentiary parity achieved across all four "not applicable"/"not run" determinations (CSharpier, analyzer, nullable, and the CI-equivalent rebuild substitute). |
| 8 | PASS | Direct read of current `spec.md` and `issue.md`: both carry the AC lists, both fully checked `[x]`, and both remain internally consistent. `spec.md`'s Root Cause Analysis table figures were corrected this cycle (Analyzer 9->11, Reference ~114->126, packages.config ~99->105), independently verified accurate against `evidence/baseline/duplicate-sweep.2026-08-10T22-31.md` in this reaudit's policy-audit Section 5 | Improved since entry audit: the stale-figure discrepancy the entry code-review flagged as a Minor documentation-accuracy finding is now corrected. |

## Acceptance Criteria Check-off Verification

All 8 items in `spec.md` remain checked `[x]`; no reversion is warranted. This reaudit confirms each check-off remains supported by evidence, with two items (5 and 8) now supported by *stronger* evidence than at the entry audit (stale figures corrected; unplanned PowerShell artifact removed).

### Acceptance Criteria Status
- Source: `docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/spec.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none

## Relationship to Policy Audit (This Cycle)

The entry policy-audit's sole BLOCKING finding (PowerShell coverage-artifact absence for the committed `evidence/baseline/duplicate-sweep.ps1`) is independently confirmed closed in `policy-audit.2026-08-11T04-05.md` Sections 1 and 6. That closure has a direct, positive effect on AC5, AC6, and AC8 above (removing the artifact that was the subject of a non-AC-invalidating but real policy gap, and correcting the stale figures the entry code-review flagged). No new AC regression is introduced by the remediation cycle.
