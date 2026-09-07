# Issue #799 outcome update — local mirror

Timestamp: 2026-09-07T08-35

PostedAs: unknown

POSTING BLOCKED — this delegation is scoped to the item worktree and explicitly excludes pushing, opening a pull
request and merging; GitHub interaction for this item is handled after review. The text below was written to the
local feature `issue.md` under a new `## Outcome` heading appended after the existing `## Next Step` section. No
existing heading was altered, per that file's automation note. The same text appears in both places.

Issue URL: https://github.com/drmoisan/TaskMaster/issues/799

Command: local file edit only; no `gh` invocation was made.

EXIT_CODE: 0

ExpectedExitCode: 0

## Exact text written to `issue.md`

## Outcome

Implemented on 2026-09-07 on branch `bug/breadcrumb-lineage-below-archive-root-799`, across the three phases of
`plan.2026-09-06T22-01.md`.

**This is a specification change superseding issue #439, not a regression fix against it.** Issue #439 delivered
full root-to-leaf ancestor lineage deliberately, and that behaviour was correct against its own acceptance
criteria. This item narrows the rendered lineage to begin at the first segment below the archive root because the
mailbox and Archive segments carry no information in a system where every filing target is under the archive root,
and because they consume most of the row width and defeat the row-distinguishability goal #439 itself set out to
serve. Nothing in #439 is being repaired.

**#439's filing-target and score-key constraint is preserved and carried forward as AC3.** The trim removes only
LEADING segments from the rendered chain; the filing value and the score-lookup key remain the archive-relative
stem, substituted into the LEAF segment, exactly as #439 required. AC3 pins this explicitly rather than leaving it
as an incidental consequence, via the test
`BindRowsAsync_TrimmedChain_PreservesFilingTargetAndScoreKey`. All ten tests of the #439 partial class
`BreadcrumbBridgeRouterIssue439Tests`, across both its files, pass unmodified: neither file carries a hunk in this
change, because every test in them drives a strict provider mock that sits below the trim boundary.

### What was delivered

- AC1 and AC2: the ancestor-chain trim lives in `OutlookFolderHierarchyProvider.GetAncestorChainAsync`, the single
  seam both the QuickFiler drop-down and the Efc list route through, so one change serves both surfaces. A chain
  that does not pass through the archive root, or whose leaf IS the root, is logged once and returns an empty
  segment list, which routes each surface into its existing fallback.
- AC3: filing target and score-lookup key remain the archive-relative stem.
- AC4: `ProjectSuggestionPath` and `ProjectPredeterminedFolder` now both delegate to the new shared
  `ArchiveStemProjection.ToDisplayStem`, built on `ArchiveStemContract.TryMakeArchiveRelative`. The empty-root
  one-separator strip is eliminated. Four of the seven candidate sites were converted; three were deliberately
  left, with reasons recorded in the specification's decision D-A.
- AC5: recent-folder entries are projected at both sites, the string append and the row-model mirror, preserving
  the documented text-parity contract between the two lists.
- AC6: the Efc router adds a projected score alongside each raw score, so an archive-rooted suggestion presented as
  a stem retains its percentage and a rooted-presented row does not lose its own.
- AC7: stale labels are logged once per label per provider instance rather than once per render, on both surfaces.
  Zero-candidate labels are additionally suppressed from the rendered row set on the Efc surface.
- AC8: verified as a finding, not a fix. No renderer alters a leading underscore; the reported space was a
  transcription artifact and a renderer change would have been a defect.

### Deviations from the specification's own prose

Four, each recorded by name with its reason in `spec.md` under Rollout & Follow-up, section Outcome: AC7 row
suppression is delivered on the Efc surface only; the AC6 score projection is additive rather than substitutive;
the two #439 Efc router test files carry no hunk; and the AC7 absence classification is published through a new
small public interface rather than through a fourth member on the shared hierarchy contract.

### Verification

The final toolchain loop closed clean in a single pass: CSharpier format and check both exit 0 over 1601 files,
the analyzer gate and the nullable gate each exit 0 with 0 Warning(s) and 0 Error(s), and the coverage-enabled
nine-assembly run exits 0 with 7085 tests, 7085 passed, 0 failed and `NEWLY-FAILING: NONE` against a 7048-test
baseline. First-party line coverage moved from 84.55 to 84.58 percent and branch coverage from 79.24 to 79.28
percent on the pinned comparability index; no changed line lost coverage. Both new production types reach 100
percent line and branch coverage. Evidence is under this feature folder's `evidence/qa-gates/` and
`evidence/regression-testing/` directories.

### Acceptance criteria

The authoritative acceptance-criteria source for this `full-bug` item is `spec.md`, section Acceptance Criteria.
All eight criteria AC1 through AC8 are checked off there. The mirrored list under "Proposed Fix / Validation
Ideas" above is left as captured, because it is the intake record rather than the tracked criteria source.

## End of mirrored text

Output Summary: The issue outcome was written to the local feature `issue.md` as a new `## Outcome` section and
mirrored here verbatim. It states that this item is a specification change superseding issue #439 rather than a
regression fix against it, and that #439's filing-target and score-key constraint is preserved and carried forward
as AC3. It was not posted to GitHub, because this delegation excludes remote interaction.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact.
