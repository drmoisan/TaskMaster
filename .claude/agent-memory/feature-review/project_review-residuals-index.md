---
name: review-residuals-index
description: One-line PASS/0-blocking pointers for a cluster of closed-issue reviews, kept for lookup if any issue number resurfaces (e.g. as a follow-up or reopened)
metadata:
  type: project
---

Each of these was a full policy-audit/code-review/feature-audit cycle that closed PASS with 0 blocking
findings. Consolidated here only because the individual issue is unlikely to resurface; if one does,
search git history / the issue tracker for the full artifact set under
`docs/features/active/<issue>/` or `docs/features/archive/<issue>/` first — this line is a pointer,
not the full record.

- **#442**: AC-19 stays unchecked (ratified deviation); residuals CR-1/CR-2/CR-3 filed as #645; PA-2
  agent-memory paths; post-442 baseline 85.1255/79.2096.
- **#444**: 3 ACs deferred-pending-PR-body (472-10/482-11/482-12); OB-1 merge-up owed vs #493 fan-in;
  NavigationTests at 498/500 lines; raw Cobertura XMLs survive in executor worktree for re-parse.
- **#446**: AC28-vs-AC18 contradiction owed a maintainer amendment (71.0% ceiling); Actions.cs carve-out
  is bound by COM loaders, not MessageBox.
- **#449**: untracked #584 promotion doc owed a non-child route; unused usings in a base test file;
  AC-supersession-via-plan-provision pattern validated.
- **#457**: CR-1 rollup-rebuild drift vs the merge path; AC15 potential_to_issue owed at epic close;
  post-457 baseline 0.855355/0.790134.
- **#468**: dual-floor coverage rows (80% PASS / 85% FAIL non-blocking) hook-verified; #623 baseline was
  stale (2437); AC-27/28 deferred to default-branch merge.
- **#476**: 90% floor treated as non-binding for exemption-narrowing entrants; CR-1 Disposed-subscription
  retention promotion; post-476 baseline 85.1435/79.2018.
- **#484**: F1 ApplyReadEmailFormat TOCTOU (promoted); F4 OneDrive silent-skip; D-1/D-2 plan-provision AC
  divergences accepted.
- **#488**: TRX host tokens (runUser+storage) partial-sanitize accepted as precedent, non-blocking; a
  21.4MB Cobertura plus C6-stale promotions owed at fan-in; #670 filed.
- **#501**: "no compliant test placement" premise failed on inspection (HubCoverageTests at 478/500 lines
  unexamined by the plan); a redundant `Abandon` call counted as coverage without real assertion power;
  full-suite logs were left uncommitted; post-501 baseline 85.1448/79.2202.
- **#511** (rescope re-audit): residuals CR-1 stale RCA narrative + CR-2 AC-vs-deleted-TRX wording; the
  PR must not claim to close #511/#571 — #592/#594/#597 carry the real defects.
- **#553** (CI split, cycle 2): 18/18 AC; reviewer self-dispatched a ci.yml run to cure green-run head
  drift; the branch rebase made ALL caller-supplied SHAs stale.
- **#614** (cycle 2): exited NO-GO/1 blocking — RC-1 widened filing guard admitted an archive-root-exact
  row that `RequireArchiveRelativeStem` throws on; a post-Hide async-void crash was found; CR-1 closed.
- **#635**: Markdown-only evidence audit; drift-invariant classification identities held across a 3rd
  commit; hook payload key is `output`; the session-cwd artifact mirror was needed again.
