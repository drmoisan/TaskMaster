# agent-memory-cobertura-dedup-generalization-wrong (Issue #532)

- Date captured: 2026-08-10
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/agent-memory-cobertura-dedup-generalization-wrong/ (Issue #532)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #532
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/532
- Last Updated: 2026-08-11
## Summary

Stored agent memory records an incorrect generalization about Cobertura root-attribute deduplication: the claim holds only for raw `dotnet-coverage` output, not for post-processed `ConvertTo-KoverageCoberturaXml` artifacts.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (documentation defect)
- Command/flags used: n/a
- Data source or fixture: `.claude/agent-memory/atomic-executor/project_coverage_delta_reproduce_baseline_counting_method.md:34-36`

## Steps to Reproduce

1. Read `.claude/agent-memory/atomic-executor/project_coverage_delta_reproduce_baseline_counting_method.md:34-36`.
2. Apply its stated rule to a post-processed `ConvertTo-KoverageCoberturaXml` artifact.
3. Compare the resulting figure against the class-level rollup count.

## Expected Behavior

The memory should state the distinction between raw generator output and post-processed artifacts explicitly, so a future agent does not skip a needed adjustment.

## Actual Behavior

The memory asserts that the repository-wide root `<coverage>` attributes "are already deduped and match a per-package all-descendant sum in this repo, so repo-level figures need no adjustment." That is true only of raw `dotnet-coverage` output. It was false for any post-processed `ConvertTo-KoverageCoberturaXml` artifact, where (before #441 was fixed) the root attributes *were* the all-descendant sum — the very defect #441 corrects.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: measured on the two committed samples —
  - raw `coverage-baseline.cobertura.xml`: class-level `<line>` count 79957 **equals** its own `lines-valid="79957"`; the all-descendant count is 161086.
  - post-processed `coverage-final.cobertura.xml`: the all-descendant count 110849 **equals** its emitted `lines-valid="110849"`; the class-level count is 62345.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

A future agent following the memory verbatim could skip a needed adjustment when reading a post-processed artifact. No production code is affected.

## Suspected Cause / Notes

The memory was written from observations of raw generator output only and generalized beyond the evidence. Now that #441 has landed, the post-processed root attributes are deduplicated too, so the correction should record the historical distinction and the date it stopped applying rather than simply deleting the claim.

Recorded as follow-up candidate 4 in `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md` § Rollout & Follow-up.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: n/a (documentation)
- [x] Integration scenario to retest: n/a
- [x] Manual verification notes: rewrite lines 34-36 to distinguish raw `dotnet-coverage` output from post-processed `ConvertTo-KoverageCoberturaXml` artifacts and cite the measured figures above.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
