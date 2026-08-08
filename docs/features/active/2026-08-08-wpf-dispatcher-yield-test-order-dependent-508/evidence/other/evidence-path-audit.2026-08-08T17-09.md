# Evidence-Path Audit

Timestamp: 2026-08-08T17-09

Task: [P2-T15]

Verifies compliance with the non-overridable evidence-location scheme in
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.

## Command

Command: `find <FEATURE>/evidence -type f | sort`
EXIT_CODE: 0

39 artifacts found. Every one resolves under
`docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/<kind>/`
with `<kind>` in the canonical set.

## Inventory by kind

| Kind | Count | Artifacts |
|---|---|---|
| `baseline/` | 14 | `coverage-baseline.cobertura.xml`, `csharpier.…16-15.md`, `msbuild-analyzers.…16-17.md`, `msbuild-nullable.…16-19.md`, `nuget-restore.…16-16.md`, `phase0-completeness.…16-29.md`, `phase0-instructions-read.md`, `probe-teardown.…16-28.md`, `repo-state.…16-11.md`, `requirements-source.…16-10.md`, `seam-preconditions.…16-13.md`, `source-under-test.…16-12.md`, `tests-coverage.…16-22.md`, `wpfdispatcheryield-coverage.…16-24.md` |
| `regression-testing/` | 3 | `fail-before.…16-26.md`, `fail-before-method.…16-27.md`, `preexisting-failure-attribution.…16-52.md` |
| `qa-gates/` | 19 | `coverage-postchange.cobertura.xml`, `coverage-changed-lines.…17-06.md`, `coverage-delta.…17-04.md`, `csharpier-check.…16-36.md`, `csharpier-check.…16-48.md`, `csharpier-format.…16-35.md`, `csharpier-format.…16-48.md`, `msbuild-analyzers.…16-37.md`, `msbuild-analyzers.…16-49.md`, `msbuild-nullable.…16-38.md`, `msbuild-nullable.…16-50.md`, `no-behavior-change.…17-08.md`, `prohibited-fix-audit.…17-07.md`, `repeat-run-1.…16-58.md`, `repeat-run-2.…17-00.md`, `repeat-run-3.…17-02.md`, `repeat-run-comparison.…17-03.md`, `tests-coverage.…16-55.md`, `tests-coverage-pass1-failed.…16-42.md` |
| `other/` | 3 | `implementation-handoff.…16-30.md`, `scope-boundary.…16-33.md`, `evidence-path-audit.…17-09.md` (this file) |
| `issue-updates/` | 0 at time of audit | `ac-reconciliation.<ts>.md` is written by P2-T25 |

The duplicate-name pairs under `qa-gates/` (`csharpier-format`, `csharpier-check`,
`msbuild-analyzers`, `msbuild-nullable`) are the loop's earlier pass and the final clean pass,
distinguished by ISO-8601 timestamp per the naming convention. Retaining both is deliberate: the
`…16-35`/`…16-36`/`…16-37`/`…16-38` set is loop pass 1 and the `…16-48`/`…16-49`/`…16-50` set is the
attested clean pass 4.

## Forbidden paths — all clear

| Forbidden path | State |
|---|---|
| `artifacts/baselines/` | does not exist |
| `artifacts/baseline/` | does not exist |
| `artifacts/qa/` | does not exist |
| `artifacts/qa-gates/` | does not exist |
| `artifacts/evidence/` | does not exist |
| `artifacts/coverage/` | does not exist |
| `artifacts/regression-testing/` | does not exist |
| `artifacts/post-change/` | does not exist |

`ls artifacts/` returns only `orchestration/`, which is the single allowed non-evidence
`artifacts/` sub-path and was not written to by this plan.

`ls coverage/` returns empty. The temporary attribution report written there during the P2-T5
causality experiment (`coverage/attribution-baseline.cobertura.xml`) was deleted immediately
afterward, confirmed by the empty listing.

## Working-tree check

Command: `git status --porcelain` (agent-memory lines filtered)

```
 M UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs
 M UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs
?? docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/
```

The only untracked path this execution created is the feature folder itself. No stray file was
written anywhere else in the repository. (The `.claude/agent-memory/**` entries filtered out of this
listing are tracked and were already dirty at branch head — see
`<FEATURE>/evidence/baseline/repo-state.2026-08-08T16-11.md`.)

## Override rejections

`EVIDENCE_LOCATION_OVERRIDE_REJECTED`: **none**. No delegation prompt, plan task, or caller
instruction supplied a non-canonical evidence path during this execution. The plan's
`## Path Aliases` section already fixed the canonical scheme and explicitly forbade
`artifacts/baseline*`, `artifacts/qa*`, `artifacts/coverage/`, and `artifacts/evidence/`, and the
execution directive restated it. Nothing required correction.

## Scratch files

Temporary scripts and logs used during execution were written to the session scratchpad at
`C:\Users\DANMOI~1\AppData\Local\Temp\claude\…\scratchpad\`, outside the repository, per the
scratchpad convention. None is an evidence artifact and none is inside the workspace.

Output Summary: PASS. All 39 evidence artifacts reside under
`<FEATURE>/evidence/<kind>/` with `<kind>` in {`baseline` (14), `regression-testing` (3),
`qa-gates` (19), `other` (3)}. None of the eight forbidden `artifacts/` sub-paths exists;
`artifacts/` contains only the allowed `orchestration/`, and `coverage/` is empty after the
temporary attribution report was removed. The feature folder is the only untracked path created. No
`EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose.
