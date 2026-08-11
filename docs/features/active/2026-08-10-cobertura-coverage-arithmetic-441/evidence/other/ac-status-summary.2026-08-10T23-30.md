# Acceptance Criteria Status Summary (P7-T22)

Timestamp: 2026-08-10T23-30

Narrative artifact. It records no command and therefore carries no `Command:` or `EXIT_CODE:` field,
which is permitted for narrative artifacts by the 2026-08-10T21-40 amendment recorded in `spec.md`
§ Acceptance Criteria. Its existence and intended path were declared in advance in the `## Final
sweep` section of `<FEATURE>/evidence/other/evidence-location-audit.2026-08-10T23-20.md`.

### Acceptance Criteria Status

- **Source:** `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md`
  (work mode `full-bug`, so `spec.md` is the sole authoritative AC source)
- **Total AC items:** 20
- **Checked off (delivered):** 20
- **Remaining (unchecked):** 0
- **Items remaining:** none

> **Superseding update, 2026-08-10T23-50 (orchestrator).** This summary originally recorded 19/20
> with AC-20 unchecked. AC-20 has since been satisfied: the orchestrator session DOES expose the
> promotion-lifecycle MCP tools that the executing `atomic-executor` session lacked, so the
> `POSTING BLOCKED` branch was resolved rather than deferred. All four candidates were filed through
> `new_potential_bug_entry` -> `potential_to_issue` as issues **#529, #530, #531, #532**. See the
> RESOLUTION section of `evidence/issue-updates/followups-441.2026-08-10T23-25.md`. The narrative
> below is retained unedited as the record of the executing session's state; the counts above and
> the AC-20 row are the current values.

## Per-item detail

| AC | Subject | Status | Evidence pointer |
| --- | --- | --- | --- |
| AC-1 | generator parity (79957 / 56124 / 23109 / 13472) | **[x]** | `evidence/qa-gates/postchange-generator-parity.2026-08-10T23-15.md` |
| AC-2 | pre-change figure `LinesValid = 161086` | **[x]** | `evidence/baseline/prechange-generator-parity.2026-08-10T22-30.md` |
| AC-3 | package-filtered A/B (62345 / 53013 / 0.850317) | **[x]** | `evidence/qa-gates/postchange-package-filtered.2026-08-10T23-15.md` |
| AC-4 | per-file merged rate (F3, `'0.6'`, five ascending lines) | **[x]** | `evidence/regression-testing/pass-after-f1-f6.2026-08-10T22-55.md` |
| AC-5 | branch counts deduplicated (F2, 2 / 1) | **[x]** | `evidence/regression-testing/pass-after-f1-f6.2026-08-10T22-55.md` |
| AC-6 | helper contract | **[x]** | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:161-259`; `evidence/other/helper-branch-test-map.2026-08-10T23-10.md` |
| AC-7 | defect removed at its one site | **[x]** | P2-T4 fixed-string search: `'.//lines/line'` -> 0 matches |
| AC-8 | correct site untouched (`:217-268` byte-identical) | **[x]** | `evidence/qa-gates/union-builder-byte-identity.2026-08-10T22-55.md` (incl. the P4-T10 post-format re-verification) |
| AC-9 | delegation replaced | **[x]** | P2-T4 fixed-string search: `$classSummaryXml` -> 0 matches |
| AC-10 | structure preserved (F6) | **[x]** | `evidence/regression-testing/pass-after-f1-f6.2026-08-10T22-55.md` |
| AC-11 | six fixtures present and passing | **[x]** | `evidence/regression-testing/pass-after-f1-f6.2026-08-10T22-55.md` |
| AC-12 | fail-before evidence (6/4, 4/2, `'0.75'`, 3/2) | **[x]** | `evidence/regression-testing/fail-before-f1-f4.2026-08-10T22-45.md` |
| AC-13 | helper precedence branches covered | **[x]** | `evidence/regression-testing/helper-unit-tests.2026-08-10T23-05.md` |
| AC-14 | zero existing tests broken | **[x]** | `evidence/baseline/pester-baseline.2026-08-10T22-30.md`, `evidence/qa-gates/pester-final.2026-08-10T23-10.md`, `evidence/regression-testing/pass-after-f1-f6.2026-08-10T22-55.md` (0-deletions numstat) |
| AC-15 | toolchain green (no new analyzer findings) | **[x]** | `evidence/qa-gates/poshqc-format.2026-08-10T23-10.md`, `poshqc-analyze.2026-08-10T23-10.md`, `pester-final.2026-08-10T23-10.md` |
| AC-16 | canonical evidence locations | **[x]** | `evidence/other/evidence-location-audit.2026-08-10T23-20.md` § Final sweep |
| AC-17 | no threshold re-tuned | **[x]** | `evidence/qa-gates/threshold-no-change.2026-08-10T23-10.md`, `evidence/other/threshold-handoff-494.2026-08-10T23-15.md` |
| AC-18 | scope boundary held (exactly two source files) | **[x]** | `evidence/qa-gates/scope-lock.2026-08-10T23-10.md` |
| AC-19 | file ceiling (455 < 500) | **[x]** | `evidence/qa-gates/file-size-audit.2026-08-10T23-10.md` |
| **AC-20** | **follow-ups filed (#529, #530, #531, #532)** | **[x]** | `evidence/issue-updates/followups-441.2026-08-10T23-25.md` § RESOLUTION |

## Why AC-20 was left unchecked by the executing session (historical; resolved 2026-08-10T23-50)

AC-20 asserts that the four follow-up candidates "are filed as GitHub issues through the promotion
lifecycle, **with their issue numbers recorded** in this feature's evidence."

**No issue number exists**, so checking the item off would certify a false statement.

The blocking condition is that the promotion-lifecycle MCP tools
`mcp__drm-copilot__new_potential_bug_entry` and `mcp__drm-copilot__potential_to_issue` are **not
exposed in the executing session at all**. The agent's MCP tool surface consists solely of
`run_poshqc_format`, `run_poshqc_analyze`, `run_poshqc_test` and `run_poshqc_analyze_autofix`, so
the promotion tools could not be invoked and there is no tool error text to quote. The `gh` CLI is
itself available and authenticated (account `drmoisan`, scopes `gist, read:org, repo, workflow`), so
the blockage is tool exposure rather than GitHub connectivity. `gh issue create` was deliberately
**not** used as a substitute, because the plan requires filing *through the promotion lifecycle*;
a bare `gh` issue would carry no promotion provenance.

Per the plan's § Phase 6 Availability branch this is the single sanctioned non-numeric outcome:
tasks P6-T1..P6-T4 are checked off on the basis of the recorded `POSTING BLOCKED` entry (their
obligation being to attempt the filing and record the outcome truthfully), while **only AC-20**
remains unchecked, because only AC-20 asserts that issue numbers exist.

All four candidates are prepared verbatim — title, body and suggested labels — in
`<FEATURE>/evidence/issue-updates/followups-441.2026-08-10T23-25.md`, ready to be filed unchanged.
**Recommended next action for `epic-orchestrator`:** re-run Phase 6 in a session where the promotion
MCP tools are exposed, then check off AC-20.

P6-T6 independently confirms that none of the four candidates was fixed in this change
(`evidence/qa-gates/followups-not-fixed.2026-08-10T23-25.md`), so the deferral is complete and
correct even though the filing is outstanding.
