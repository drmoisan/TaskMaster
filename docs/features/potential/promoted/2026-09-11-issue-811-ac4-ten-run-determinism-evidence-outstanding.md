# issue-811-ac4-ten-run-determinism-evidence-outstanding (Issue #864)

- Date captured: 2026-09-11
- Author: Dan Moisan

- Status: Promoted -> docs/features/active/issue-811-ac4-ten-run-determinism-evidence-outstanding/ (Issue #864)

- Issue: #864
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/864
- Last Updated: 2026-09-11
## Problem / Why

Issue #811 closed with four of its five acceptance criteria checked. AC4 is still unchecked on `main`:

```
docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/spec.md:295
- [ ] AC4: A full nine-assembly /InIsolation run with TestCategory!=LiveOutlook reports zero
      failures on ten consecutive runs, recorded as evidence.
```

AC1, AC2, AC3 and AC5 are all `[x]`. AC4 is the gate that proves the determinism work actually eliminated the races rather than merely suppressing them, so leaving it open leaves the issue's central claim unverified.

AC4 was deferred for a stated reason, not overlooked. The remediation cycle 1 exit reaudit recorded that AC4 should be re-evaluated once the `ILGlobals` race was fixed under its own issue, because that race was a known source of cross-class interference in a full-suite run. That precondition is now discharged: issue #824 was delivered and closed, and its feature audit is on `main` at `docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/feature-audit.2026-09-09T22-40.md`.

Nothing has re-evaluated AC4 since. This entry exists so that re-evaluation is tracked somewhere durable rather than living only in a closed feature folder.

## Proposed Behavior

Re-run the AC4 gate against current `main` now that #824 has shipped, then either check AC4 off with the evidence attached, or record a specific, reproducible failure that justifies keeping it open and names what still needs fixing.

The AC4 gate as `spec.md` states it: ten consecutive full-suite `/InIsolation` runs over the assembly set the `mstest-coverage` workflow discovers (every `*.Test.dll` under the debug output tree, excluding intermediate and reference directories), filtered with `TestCategory!=LiveOutlook`, all reporting zero failures, recorded as committed evidence.

## Acceptance Criteria (early draft)

- [ ] Ten consecutive full-suite runs are executed against current `main` under the filter and isolation mode `spec.md` names, and each run's result is captured as committed evidence.
- [ ] AC4 in `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/spec.md` is either checked off with a citation to that evidence, or left unchecked with a recorded failure naming the specific test and the specific interference.
- [ ] If any run fails, the failing test and its suspected cause are captured as a separate potential entry rather than fixed under this one.

## Constraints & Risks

- **This is an evidence-gathering task, not a code change.** If it passes, the only edit is a checkbox and an evidence file. Resist widening it into a fix; a failure belongs in its own issue.
- **Two known local-environment hazards will distort the result if ignored.** Four `UtilitiesCS.Test` shell-icon test classes stall `vstest` on at least one development machine through `SHGetFileInfo`, reproducing on `main` and unrelated to #811; and a local run needs both the `\.claude\` worktree-path exclusion and CI's `/InIsolation` to avoid loading stale assemblies out of agent worktrees. A run that omits either produces failures that look like determinism regressions and are not. CI is the more trustworthy venue for this gate.
- **Ten consecutive runs is a long wall-clock commitment.** Consider whether CI can carry the repetition rather than a developer machine.
- The related flaky `TryAddValuesAsync` test is tracked separately under issue #780.

## Test Conditions to Consider

- [x] Unit coverage areas: no new coverage is required; this re-runs the existing suite.
- [x] Integration scenarios: the full nine-assembly suite under `/InIsolation` with `TestCategory!=LiveOutlook`, repeated ten times.
- [ ] CLI/API examples: n/a.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/issue-811-ac4-ten-run-determinism-evidence-outstanding/` folder from the template
