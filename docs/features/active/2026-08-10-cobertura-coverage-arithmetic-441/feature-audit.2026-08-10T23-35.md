# Feature Audit — 2026-08-10-cobertura-coverage-arithmetic-441

- **Timestamp:** 2026-08-10T23-35
- **Reviewer:** feature-review agent
- **Branch:** `bug/cobertura-coverage-arithmetic-441` at `3b8d43fb` vs base `edf3d34c`
- **Work mode:** `full-bug` — AC source is `spec.md` § Acceptance Criteria **only** (`user-story.md` exists but is not an AC source in this mode; `issue.md` § Acceptance Criteria is an explicit pointer, not a second source)
- **Spec version:** 1.1, with two accepted preparation-time amendments (AC-15 no-new-findings gate; AC-16 schema scoped to command-step artifacts) — applied as written, not re-litigated

## Summary

All 20 acceptance criteria in `spec.md` are verified **PASS**. Every item was independently verified rather than accepted from its check mark: the four headline arithmetic claims (AC-1, AC-2, AC-3, and the fail-before values behind AC-12) were re-executed by this reviewer from the committed inputs and the extracted base-revision code; the structural claims (AC-6 through AC-10, AC-18, AC-19) were verified against the diff and by byte comparison; the process claims (AC-11 through AC-17, AC-20) were verified by re-running the suite and analyzer, scanning the evidence tree, and confirming the four follow-up issues on GitHub. No AC required unchecking. 0 blocking findings.

## Scope and Baseline

- Adjudicated diff: `git diff edf3d34c..HEAD` (full branch diff, 118 files). Base verified as an ancestor of HEAD.
- Source changes: exactly `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`. All other changed paths are feature documents, evidence, sibling epic-preparation documents already on the integration branch within the diff window, promoted potential entries, and agent-memory markdown.
- Baseline figures used for comparison (independently reproduced): pre-change `LinesValid = 161086` over the #424 raw generator document; pre-change package-filtered figures 110849 / 94937 / 0.856453; baseline file coverage 146/165 = 88.48%; baseline file sizes 357 (production) and 222 (tests) lines.

## Acceptance Criteria Inventory

- Source: `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md` § Acceptance Criteria
- Total AC items: 20 (AC-1 .. AC-20), all checked `[x]` at review start
- Format: markdown checkboxes; no phantom criteria added; criterion text unmodified

## Acceptance Criteria Evaluation

| AC | Subject | Verdict | Independent verification performed |
| --- | --- | --- | --- |
| AC-1 | Generator parity oracle 79957 / 56124 / 23109 / 13472 | **PASS** | Reviewer dot-sourced HEAD `Helpers.ps1` and ran `Get-CoberturaCoverageSummary` over the committed #424 raw baseline document: returned exactly 79957 / 56124 / 23109 / 13472, equal to the document's own root attributes (also read directly) |
| AC-2 | Pre-change figure `LinesValid = 161086`, all four counts concrete and strictly greater | **PASS** | Reviewer extracted `Helpers.ps1` at `edf3d34c` and re-ran the same procedure: 161086 / 113219 / 46218 / 26944 — each strictly greater than its AC-1 counterpart; matches `evidence/baseline/prechange-generator-parity.2026-08-10T22-30.md` verbatim |
| AC-3 | Package-filtered A/B: 62345 / 53013 / 0.850317 post-fix vs 110849 / 94937 / 0.856453 pre-fix | **PASS** | Reviewer re-ran `ConvertTo-KoverageCoberturaXml` at HEAD over the committed #424 `coverage-final.cobertura.xml`: root attributes exactly 62345 / 53013 / 0.850317; pre-fix values confirmed from `evidence/baseline/prechange-package-filtered.2026-08-10T22-30.md` and the threshold-handoff record |
| AC-4 | Merged per-file rate corrected; F3 asserts `'0.6'` with five ascending line children 12,13,56,57,58 | **PASS** | F3 read in the test file (asserts `line-rate` `'0.6'`, count 5, joined order `'12,13,56,57,58'`); passes at HEAD (19/19); fails against the base revision with actual `'0.75'` (reviewer-reproduced and recorded in fail-before evidence) |
| AC-5 | Branch counts deduplicated; F2 asserts `branches-valid`=`'2'`, `branches-covered`=`'1'`; no branch regression assertion relies on `branch-rate` alone | **PASS** | F2 read and re-run (passes at HEAD; fails at base with 4 and 2); grep confirms no new test asserts `branch-rate` as its regression signal — F2 asserts the two counts |
| AC-6 | Helper contract: name, mandatory `[System.Xml.XmlElement]` parameter, enumeration order, dedup rules, five output fields | **PASS** | Source read at `Helpers.ps1:161-259`: `[Parameter(Mandatory = $true)][System.Xml.XmlElement]$ClassNode`; enumerates `./lines/line` then `./methods/method/lines/line`; keys by `[int]` line number (via StrictMode-safe `GetAttribute('number')`, equivalent to the spec's `[int]$node.number`); max(hits) / branch-if-either / larger-Total tie-broken-by-larger-Covered via `Get-CoberturaLineConditionCoverageParts`; returns `LineMap`, `TotalLines`, `CoveredLines`, `TotalBranches`, `CoveredBranches` |
| AC-7 | `$cls.SelectNodes('.//lines/line')` gone; summary accumulates from the helper | **PASS** | Reviewer grep: 0 occurrences of `.//lines/line` in the file; the summary loop reads the four totals from `Get-CoberturaClassLineSummary` (diff inspected) |
| AC-8 | Union builder at `:217-268` byte-identical, including `./lines/line` child-axis selection | **PASS** | Reviewer `cmp` of base lines 217-268 vs HEAD lines 311-362: byte-identical; the child-axis selection survives at HEAD line 313 |
| AC-9 | `$classSummaryXml` synthetic-document block removed; merged rates set from a direct helper call | **PASS** | Reviewer grep: 0 occurrences of `$classSummaryXml`; diff shows `Get-CoberturaClassLineSummary -ClassNode $mergedClassNode` feeding `SetAttribute('line-rate', ...)` / `SetAttribute('branch-rate', ...)` |
| AC-10 | F6 passes: `<methods>` neither merged nor stripped; no `hits` value differs from input | **PASS** | F6 read (asserts exactly one primary `<method>` named `M` and the exact `number=hits` join `'12=0,13=0,56=1,57=1,58=1'`); passes at HEAD and at base (guard fixture, correctly not expect-fail) |
| AC-11 | F1-F6 present as six new `It` blocks, inline single-quoted here-strings, no file on disk, no mock in any arithmetic path, all passing | **PASS** | All six located in the test file, each an inline `@'...'@` here-string cast to `[xml]`; grep for filesystem APIs over the test file: zero matches; the only `Mock` is in a pre-existing allowlist test; suite 19/19 at HEAD (reviewer-run) |
| AC-12 | F1-F4 demonstrated failing against unmodified `Helpers.ps1` with the stated pre-fix values; recorded under `evidence/regression-testing/` | **PASS** | Evidence artifact records FailedCount 4 / PassedCount 10 / Total 14, with per-assertion actuals 6/4, 4/2, `'0.75'`, 3/2, and a same-time empty `git diff --name-only edf3d34c -- scripts` proving unmodified production code; reviewer independently reproduced the same four failures with the same values against the extracted base file (three additional scratchpad-only failures were traced to the empty project allowlist in the scratchpad tree — an environment artifact affecting only the three pre-existing tests that omit `-ProjectNames`, verified directly) |
| AC-13 | All three condition-coverage precedence branches covered by direct unit tests | **PASS** | Three dedicated `It` blocks read (candidate Total greater; Total equal and Covered greater; neither — existing retained), each asserting the resulting `TotalBranches`/`CoveredBranches`; all pass at HEAD |
| AC-14 | All eight pre-existing `It` blocks pass unmodified; diff shows no edit to any of them | **PASS** | Test-file diff is +246/-0 (numstat and a zero count of deletion lines); the 8 baseline `It` blocks (5 ConvertTo + 3 allowlist, including `lines-valid | Should -Be '3'`) all pass at HEAD |
| AC-15 | Toolchain green: format no-change, analyzer no new findings vs baseline (the one `PSUseSingularNouns` baselined), Pester FailedCount 0; recorded under `qa-gates/` | **PASS** | Reviewer re-ran the analyzer at HEAD: exactly the one baselined warning (line 146 -> 140, key-identical) on the production file, zero on tests; reviewer re-ran Pester: 19/19; format evidence records hash-identical files; all three gate artifacts present under `qa-gates/` with full schema fields |
| AC-16 | Canonical evidence locations; command-step artifacts carry Timestamp/Command/EXIT_CODE (+ Output Summary for baselines); narrative artifacts carry Timestamp and are enumerated in the final sweep; nothing under `artifacts/` | **PASS** | Reviewer field-scanned all 30 evidence markdown files: 27 command-step artifacts carry all four fields; the 3 narrative artifacts (`ac-status-summary`, `helper-branch-test-map`, `threshold-handoff-494`) carry `Timestamp:` and are enumerated or declared-in-advance in the final sweep; the full evidence tree (35 files) sits under the five canonical `<FEATURE>/evidence/` kinds; the branch diff contains zero files under any `artifacts/` path |
| AC-17 | No threshold re-tuned; 85.0317%-vs-85% recorded as a handoff to #494 and nowhere acted upon | **PASS** | Reviewer re-ran `git diff --name-only edf3d34c -- CLAUDE.md .claude/rules coverage.config`: empty; the handoff artifact states explicitly it is a record of fact, names #494 (wave 2) as owner, and its "what this feature does about it: nothing" section is borne out by the empty diff |
| AC-18 | Exactly two source files changed; `Invoke-MSTestWithCoverage.ps1` untouched including its missing `\.claude\` exclusion | **PASS** | Full-diff enumeration minus `docs/**` and `.claude/agent-memory/**` yields exactly the two files; `Invoke-MSTestWithCoverage.ps1` absent from the diff; the missing exclusion is now filed as issue #531 rather than fixed here |
| AC-19 | Production file remains under 500 lines (357 before) | **PASS** | `awk NR`: 455 lines at HEAD (baseline 357 confirmed); the test file is 468 (baseline 222) — both under the 500-line ceiling that applies to test code as well |
| AC-20 | Four follow-ups filed through the promotion lifecycle with issue numbers recorded in evidence; none fixed in this change | **PASS** | Reviewer confirmed issues #529, #530, #531, #532 exist and are OPEN on GitHub with titles matching the four candidates; the four promoted potential entries are committed under `docs/features/potential/promoted/`; the RESOLUTION section of `followups-441.2026-08-10T23-25.md` and the superseding update in `ac-status-summary` record the numbers; `followups-not-fixed` evidence plus the diff itself (package rates unrecomputed, `<methods>` un-merged per F6, discovery exclusion absent, agent-memory generalization uncorrected in the flagged file) confirm none was fixed here |

## Check-off Reconciliation

All 20 items were already checked `[x]` in `spec.md` and every one was independently verified PASS, so no check mark was changed and no item required unchecking. The AC-20 check-off history is honest: the executing session left it unchecked with a truthful `POSTING BLOCKED` record; the orchestrator later satisfied it and the audit trail of that sequence is preserved rather than rewritten.

## Executor Self-Reported Items

1. **AC-20 completed by the orchestrator:** verified genuinely satisfied; the blocked-then-resolved trail (original record retained, dated RESOLUTION appended, superseding update clearly marked) is legible and truthful.
2. **P0-T13 CORRECTION:** adequate — the retracted sentence is quoted, the invalid instrument is explained, and no downstream conclusion depended on it (all cited coverage figures come from direct `Invoke-Pester` runs writing to explicit `<FEATURE>/evidence/` paths, verified per-artifact).

### Acceptance Criteria Status

- Source: `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md`
- Total AC items: 20
- Checked off (delivered): 20
- Remaining (unchecked): 0
- Items remaining: none

## Verdict

**PASS.** All 20 acceptance criteria independently verified. 0 blocking findings; 5 non-blocking findings recorded in `policy-audit.2026-08-10T23-35.md` (NF-1..NF-5) and `code-review.2026-08-10T23-35.md`. No remediation cycle required.
