# Feature Audit — Issue #449, QuickFiler Explorer Controller Latent Defects

- **Timestamp:** 2026-08-22T10-58
- **Reviewer:** feature-review agent
- **Branch:** `bug/quickfiler-explorer-controller-latent-defects-449-exec` at `af6531ed`
- **Baseline:** merge-base `c551eabab0aa0a6b1a284252811a2e1de819634e` (epic integration tip), independently confirmed via `git merge-base`.
- **Work mode:** `full-bug` — per `.claude/skills/acceptance-criteria-tracking/SKILL.md`, `spec.md` is the sole authoritative AC source. `issue.md`'s early-draft criteria are explicitly superseded by the spec's own header note and were not audited.

## AC Evaluation Table

All sixteen criteria were found already checked off by the executor (commit `af6531ed`). Each was re-verified independently by this reviewer; "Reviewer verification" below states what this audit ran or read, distinct from the executor's evidence.

| AC | Verdict | Reviewer verification |
| --- | --- | --- |
| **AC-1** (D1 removal) | **PASS** | Ran `git grep -n "ExplConvView_Cleanup" -- "*.cs"`: exactly 3 hits, all in uncompiled `QuickFiler/Legacy/QuickFileController.cs` (:673, :851) and `QuickFiler/Notes/notes_interfaces.cs` (:58). Interface and implementation hits are gone. Analyzer and nullable builds recorded EXIT 0. |
| **AC-2** (knowledge preservation) | **PASS** | Read `spec.md`: the section headed exactly `## Removed contract — legacy semantics for future restoration` is present with the verbatim legacy body, semantic summary, member-equivalence table, fallback implementation, and both catch-asymmetry readings. |
| **AC-3** (D2 fix + named regression test) | **PASS** | Read `QfcExplorerController.cs:139`: `_activeExplorer.CurrentFolder = (MAPIFolder)mailItem.Parent;`. The named test exists in the new test file; `expect-fail-defect2.2026-08-22T09-16.md` records it failing (EXIT 1, verbatim Moq.MockException) against unfixed code and `pass-after-defect2.2026-08-22T09-16.md` records it passing after a `/t:Rebuild`; it appears in the post-change full-suite ADDED set. |
| **AC-4** (no residual re-resolution) | **PASS** | Ran `git grep -n "ActiveExplorer()" -- QuickFiler/Controllers/QfcExplorerController.cs`: exactly one line (line 24, the constructor capture). |
| **AC-5** (documentation correction) | **PASS** | Read spec Root Cause Analysis and D2: the `NavigateToOutlookFolder` location correction, the only-re-resolution fact, and the inapplicability of the "document why a fresh call is required" branch are all recorded. |
| **AC-6** (dead region deleted) | **PASS** | Ran the AC's exact grep over `QuickFiler QuickFiler.Test`: zero matches (exit 1). The dossier's non-vacuity check shows 12 matches at merge-base for the identical pattern and scope. |
| **AC-7** (no-behavior-change evidence) | **PASS** | Read `suite-comparison-before-after.2026-08-22T09-16.md`: 6,437 → 6,452 executed and passed, 0 failed/skipped in both, set comparison shows exactly 15 ADDED (the 14 new methods, one contributing two DataRow cases) and zero REMOVED. |
| **AC-8** (using hygiene, self-verifying) | **PASS** | Read the post-change using block: the six retained directives are exactly those listed (`System.Threading.Tasks`, `System.Windows.Forms`, `Microsoft.Office.Interop.Outlook`, `QuickFiler.Interfaces`, `UtilitiesCS`, `Outlook =`); all nine listed removals are gone. Final analyzer and nullable builds EXIT 0 with zero errors prove no removed directive was required. |
| **AC-9** (attribute removed) | **PASS** | Ran `git grep -n "ExcludeFromCodeCoverage" -- QuickFiler/Controllers/QfcExplorerController.cs`: zero matches (exit 1). |
| **AC-10** (dialog seam) | **PASS** | Read lines 56-63 (seam with `MessageBox.Show` default) and 167 (routed call). Ran `git grep -n "MessageBox.Show" -- QuickFiler/Controllers/QfcExplorerController.cs`: exactly one hit, line 63, inside the seam default initialiser. Three seam tests (invoked-once with all four arguments asserted verbatim, Yes displays, No does not) pass in the recorded runs; no dialog is displayed under test. |
| **AC-11** (coverage measured and reported) | **PASS** | Baseline under `evidence/baseline/`, post-change under `evidence/qa-gates/` — both canonical kinds; no `evidence/coverage/` folder exists. All required figures reported numerically, and this reviewer recomputed each from the raw Cobertura reports: repo-wide 85.3290% → 85.3571%; `QuickFiler` package 80.9163% → 80.9898%; `QfcExplorerController` absent-from-report → 87.8261% (101/115 aggregated over all four `<class>` elements — baseline match count is genuinely zero classes, confirming "absent" rather than 0%). The 80% gate is not lowered; no exclusion attribute restored (AC-9 re-confirmed). No shortfall exists to state. |
| **AC-12** (test file + project entry) | **PASS**, with recorded supersession | The test file exists with the required class/namespace, MSTest + Moq + FluentAssertions. The csproj diff is a single hunk (lines 117-123) adding **two** entries adjacent to `QfcDatamodelLivenessTests`, CRLF, with the `Form1` compile region and `Form1.resx` `EmbeddedResource` untouched — verified independently from the diff. The divergence from "exactly one appended line" is the [P6-T14] split consequence, pre-authorized by the spec's Constraints section and fully evidenced in `evidence/other/test-file-size.2026-08-22T09-16.md`. Judged properly evidenced; the criterion's substance (partitioned append, sibling region untouched) holds. |
| **AC-13** (deterministic tests) | **PASS** | Reviewer ran the banned-API scan over **both** new test files (the AC's command names only the base file; the reviewer's scan extends it to the continuation file): zero matches. Two consecutive full-suite runs with identical pass sets are recorded. No temp files, live forms, message pumps, or `MessageBox.Show` in test code. |
| **AC-14** (fail-before dossiers) | **PASS** | Both dossiers exist under `evidence/regression-testing/` with every required field (`Timestamp:`, `Command:`, `EXIT_CODE:`, `WhyFailingRunImpossible:`, `SearchScope:`, `SearchPatterns:`, `SearchResult:`). Reviewer reproduced the defect-1 hit set (3 surviving uncompiled hits) and the defect-3 zero-match result against the working tree; both `Command:` entries reproduce their recorded `SearchResult:`. Exception adjudication: justified — see policy audit Section 5.2. |
| **AC-15** (clean full-toolchain pass) | **PASS** | Final QA artifacts under `evidence/qa-gates/` record steps 1-5 all EXIT 0 in a single pass: tool restore, `csharpier format` (zero changes) + `check` (1,519 files, zero unformatted), analyzer `/t:Rebuild` (0 errors, `CoreCompile` skip count 0 proving analyzers ran), nullable `/t:Rebuild` (0 errors, no `/p:Nullable=enable`), vstest with `/EnableCodeCoverage`-equivalent collection, `/InIsolation`, `TestCategory!=LiveOutlook`, and `\.claude\` excluded from discovery. |
| **AC-16** (file-size cap attribution) | **PASS**, with recorded supersession | Reviewer-measured line counts: 182 / 14 / 486 / 387 / 205 — every file in the diff under 500. Neither `SortEmail.cs` nor `QuickFiler/Legacy/QuickFileController.cs` appears in the diff (confirmed from `git diff --stat`). The 485 → 486 figure supersession is recorded in the same reconciliation artifact as AC-12's. The epic kickoff's 1,065-line prediction conflated the uncompiled legacy file, which is not in the diff; no cap finding attaches. |

## Epic NFR Verification

"Coverage of `QuickFiler.csproj` is retained or improved at every child merge": **MET** — package line rate 80.9163% → 80.9898% (+0.0735 pp), independently recomputed by this reviewer from the raw Cobertura reports. The improvement occurred despite the class entering the denominator for the first time, because the class enters at 87.8261%, above the package average, and the 139 unreachable lines were removed permanently.

## Shared-Surface Verification (sibling #491)

Verified from the diff itself: the entire `QuickFiler.Test.csproj` change is one hunk at lines 117-123; the `Form1` compile region and `Form1.resx` `EmbeddedResource` are untouched. The epic's partition ("#449 owns appended `Compile Include` entries in the `Controllers` item group; #491 owns the `Form1` region") is honored with roughly 40 lines of separation from #491's region.

## Newly Checked-Off Items

None. All 16 AC items were already checked off by the executor with per-item evidence; this audit independently re-verified each and confirms every check-off. No AC item was unchecked or reverted.

## Residual Items for the Orchestrator (non-blocking)

1. The promotion document for issue #584 (the disclosed `ProgressTrackerAsync` flake) is untracked in this worktree and cannot ride this child branch under the epic's `docs/features/potential/**` prohibition. Route it through the epic close or a direct commit, or accept the GitHub issue body as the durable record. (Policy audit NB-2.)
2. Two unused `using` directives in the base test file (code review CR-1) — opportunistic cleanup only.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-08-07-quickfiler-explorer-controller-latent-defects-449/spec.md`
- Total AC items: 16
- Checked off (delivered): 16
- Remaining (unchecked): 0
- Items remaining: none

## Verdict

All 16 acceptance criteria **PASS** (two with properly evidenced numeric supersessions recorded at check-off time). The epic NFR is met, the shared-surface partition is honored, and the disclosed items are correctly handled. **Blocking findings: 0.** No remediation cycle is required.
