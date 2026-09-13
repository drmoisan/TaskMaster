# raw-evidence-paths-tracked-on-main-unowned (Issue #884)

- Date captured: 2026-09-13
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/raw-evidence-paths-tracked-on-main-unowned/ (Issue #884)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #884
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/884
- Last Updated: 2026-09-13
## Summary

`main` carries a large number of tracked raw evidence paths — raw coverage-collector documents (`*.cobertura.xml` and similar) and raw test-platform documents (`*.trx`) committed under feature folders' `evidence/` trees. CLAUDE.md now carries a `## Committed Test Evidence Format` section (line 412) that prohibits committing either document type in any form, including under a feature folder's evidence tree, because both carry absolute host paths and machine-specific identifiers. The repository does not conform to its own policy, and the sweep that was meant to remediate it has no current owner.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable; this is a repository-hygiene defect spanning committed Markdown/XML/TRX artifacts
- Command/flags used: `git ls-files -- 'docs/features/**/*.trx'`, `git ls-files -- 'docs/features/**/*.coverage' 'docs/features/**/*cobertura*.xml'`, run against `main` (HEAD `e6d86049e`)
- Data source or fixture: tracked files under `docs/features/active/**/evidence/**` on `main`

## Steps to Reproduce

1. Check out `main` (or any worktree at the same HEAD, e.g. `e6d86049e`).
2. Run `git ls-files -- 'docs/features/**/*.trx'`. This independently returned 333 tracked `.trx` paths under feature evidence trees (verified 2026-09-13).
3. Run `git ls-files -- 'docs/features/**/*.coverage' 'docs/features/**/*cobertura*.xml'`. This independently returned a large additional set of tracked raw Cobertura documents under feature evidence trees (e.g. `docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/baseline/issue-439-baseline.cobertura.xml`).
4. Compare against CLAUDE.md line 412, `## Committed Test Evidence Format`: "A raw coverage collector document and a raw test-platform document are both prohibited. Neither may be added to git in any form, including under a feature folder's evidence tree."
5. Item 743's own measurement (referenced by this filing) independently found 572 tracked raw evidence paths at its own HEAD and 572 on `main`, a set difference of zero, meaning no in-flight item added any of them — all 572 predate the policy.

## Expected Behavior

`main` should carry zero raw coverage-collector documents and zero raw test-platform documents under any feature folder's evidence tree. Only the two permitted projections (a package-level JaCoCo projection of the post-processed Cobertura document, and the one-line first-party coverage summary) and the trx-derived test-result summary should be committed.

## Actual Behavior

`main` carries hundreds of raw `.trx` files and raw `.cobertura.xml` files under `docs/features/active/**/evidence/**`, predating the CLAUDE.md policy that now prohibits them. This was the job of issue 602 (`Repository-wide host-identifier leakage: absolute user-profile paths, account and host names in tracked files` — the same class of defect, since these evidence documents also carry absolute host paths and machine-specific identifiers). Item 602 was **withdrawn** from the parallel run `bugs-2026-09-11` before execution (confirmed 2026-09-13 in `docs/features/parallel/bugs-2026-09-11/parallel-status.md` on the `parallel/bugs-2026-09-11-plan` branch: row `| 602 | docs/features/active/2026-09-12-host-identifier-leakage-sweep-602 | - | C3 | opus | withdrawn | not_started | - | - |`). No other item in that run or elsewhere has since picked up the sweep, so it currently has no owner.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `git ls-files -- 'docs/features/**/*.trx'` against `chor/defect-filings-2026-09-13` (HEAD `e6d86049e`, same as `main`) returned 333 lines, first entry `docs/features/active/2026-08-24-breadcrumb-coordinator-hub-defects-501/evidence/baseline/trx/p0-t17/p0-t17.trx`. A second query for `*.coverage`/`*cobertura*.xml` returned dozens more raw Cobertura documents across multiple feature folders (`efcviewer-missing-lineage-...-439`, `breadcrumb-coordinator-hub-defects-501`, `breadcrumb-router-navigation-defects-498`, `qfc-collection-controller-defects-468`, `quickfiler-bug-family-446`, `webview2-host-initializer-defects-476`, and others).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High because the repository is provably out of compliance with its own committed-evidence policy at scale (hundreds of files), the tracked files carry absolute host paths and machine-specific identifiers as the policy itself notes, and there is currently no owner driving remediation after item 602's withdrawal.

## Suspected Cause / Notes

The `## Committed Test Evidence Format` policy in CLAUDE.md is recent; the raw evidence predates it and was never sweep-cleaned because the one item scoped to do the sweep (602, `host-identifier-leakage-sweep`) was withdrawn from `bugs-2026-09-11` before it ran. No replacement item currently owns this scope. This is a documentation/process gap (no owner), not a code defect in any single feature's evidence.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: not applicable (documentation/evidence hygiene, not source code)
- [x] Integration scenario to retest: a repository-wide sweep that (a) converts remaining raw `.trx`/`.cobertura.xml` evidence to the permitted JaCoCo/summary projections where the underlying figures are still needed, or (b) deletes the raw documents where the feature is already archived and the figures are preserved elsewhere, then verifies `git ls-files -- 'docs/features/**/*.trx' 'docs/features/**/*.cobertura.xml'` returns zero.
- [x] Manual verification notes: re-run the same `git ls-files` queries after the sweep and confirm a zero count; also confirm no new raw evidence is introduced going forward (this should already be caught by the existing feature-review policy-audit process for new work).

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
