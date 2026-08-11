# research-doc-cohort-library-false-negative (Issue #546)

- Date captured: 2026-08-11
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/research-doc-cohort-library-false-negative/ (Issue #546)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #546
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/546
- Last Updated: 2026-08-11
## Summary

`docs/research/2026-08-10-parallel-bug-flighting-and-surface-blockers.md` records the cohort-computation library as absent from both TaskMaster and drm-copilot and instructs the next reader to treat it as new work. The library exists in both repositories. The document's unblock checklist therefore sends a reader to reimplement code that is already present.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a for the TaskMaster entry point (bash); the upstream authority is Python
- Command/flags used: `git grep -in "compute_cohorts|welsh"` (the original, defective verification)
- Data source or fixture: `docs/research/2026-08-10-parallel-bug-flighting-and-surface-blockers.md` at commit `41213a1c`

## Steps to Reproduce

1. Read §2.1 and §7 item 1 of `docs/research/2026-08-10-parallel-bug-flighting-and-surface-blockers.md`.
2. Observe the claim that `compute_cohorts` (Welsh-Powell coloring) does not exist in TaskMaster or in drm-copilot, and that closing the blocker is "new work, not a port."
3. Check `.claude/lib/bash/compute-cohorts.sh` in TaskMaster.
4. Check `scripts/dev_tools/parallel_cohort_computation.py` in drm-copilot.

## Expected Behavior

The research document's blocker inventory should reflect the actual state of both repositories, so that a reader resuming the plan closes only the blockers that are genuinely open.

## Actual Behavior

Both files exist:

- TaskMaster carries a bash entry point at `.claude/lib/bash/compute-cohorts.sh` (present at commit `2073f717`, executable, 4463 bytes). It requires neither Python nor Poetry, so the skill's `poetry run python -c "from scripts.dev_tools..."` invocation form is not the applicable one here.
- The upstream authority is `C:\Users\DanMoisan\repos\drm-copilot\scripts\dev_tools\parallel_cohort_computation.py` (commit `663d71ee`, issue #445).

The P5 recomputation-parity concern recorded alongside the blocker is also resolved: the bash port documents itself as reproducing the Python authority's output byte-for-byte, so a parity check compares two independent implementations rather than a module against itself.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet:

  ```
  $ ls -la .claude/lib/bash/compute-cohorts.sh
  -rwxr-xr-x 1 DanMoisan 197121 4463 Aug 11 09:19 .claude/lib/bash/compute-cohorts.sh
  ```

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High rather than Blocker: the document is a resumable plan of record explicitly intended to be read instead of re-deriving the analysis. A false "does not exist, build it" entry in its unblock checklist causes duplicated implementation work and misdirects attention away from the one blocker that is genuinely open (the blast-radius truth table).

## Suspected Cause / Notes

The verification used `git grep -in "compute_cohorts|welsh"` without `-E`. `git grep` defaults to basic regular expressions, so the `|` was matched as a literal pipe character and the search could not match either token. The negative result was recorded as a confirmed absence.

Sections to correct:

- §2.1 (surface-blocker analysis)
- §7 item 1 (unblock checklist)
- Any P5 recomputation-parity note that depends on the absence claim

## Proposed Fix / Validation Ideas

- [ ] Correct §2.1 and §7 item 1 to record both files as present, with their paths and commits.
- [ ] Note the `git grep` basic-regex pitfall inline so the correction is self-justifying to a later reader.
- [ ] Update the P5 parity note to reflect that two independent implementations exist.
- [ ] Restate the remaining open blocker (the TaskMaster-specific `config/blast-radius.json` truth table) as the single item gating the parallel surface.
- [ ] Unit coverage areas: n/a — documentation-only change.
- [ ] Integration scenario to retest: n/a.
- [ ] Manual verification notes: confirm both cited paths resolve at the recorded commits before publishing the correction.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
