---
name: project-469-comment-accuracy-plan-seams
description: Issue #469 plan seams — a defect-number SWAP makes whole-file token gates vacuous; single-character edits make exact numstat derivable; spec line counts were off by one
metadata:
  type: project
---

Issue #469 turned out to be documentation-accuracy only: three of four defects were already merged,
and the fourth's residual action is open issue #629. The plan is comment/XML-doc/`because:`-string
edits with zero executable-line change.

**A renumbering SWAP makes every whole-file token gate vacuous.** Both `Issue #469 defect 1` and
`Issue #469 defect 2` already existed in BOTH edited files at branch head, so "the file contains
`Issue #469 defect 2`" passes before any work. Every gate had to become a combined single-line token
pairing the defect number with its distinguishing text (`Issue #469 defect 2: exactly one diagnostics
line`), plus the complementary must-become-zero token. This generalises to any A-to-B relabelling
where both labels are already present.

**Why:** a swap conserves the multiset of tokens; only their pairing with surrounding text changes.
**How to apply:** for any swap/rename plan, gate on the PAIRING, and always author the zero-match
companion alongside the one-match assertion.

**A single-character substitution makes exact `--numstat` derivable.** All eight renumbering sites
were one-digit changes on one physical line each, so line length and line count are invariant and the
plan could assert exactly `2 2` and `6 6` per file. Verify the digit-only property by reading each
line before promising an exact numstat; a rewrap would void it.

**Spec line counts were off by one and research line counts were wrong.**
`QfcCollectionControllerDefects468MoveTests.cs` is 497, not the spec's 498;
`QfcHomeController.Metrics.cs` is 216, not the research doc's "232, approximate"; and the research
cited `:351` for a site that is actually `:352`. Re-derive every count and citation even when two
upstream documents agree. See [[verify-test-provenance-before-planning-deletion]].

**Local facts confirmed this pass:** the CSharpier manifest is `dotnet-tools.json` at the repository
ROOT (there is no `.config/` directory); `packages/` and `QuickFiler.Test/bin/Debug/` are absent from
a fresh agent worktree so restore-then-build must precede any test-count baseline;
`Invoke-MSTestWithCoverage.ps1` calls `Assert-CoberturaLineCoverageThreshold`, which throws below 80%
BEFORE the Koverage post-processing writes the XML, so a baseline task must record the thrown
percentage and continue rather than treating it as this change's failure. Related:
[[project_494_threshold_reconciliation_plan_seams]], [[reference_invoke_mstest_with_coverage_script]].
