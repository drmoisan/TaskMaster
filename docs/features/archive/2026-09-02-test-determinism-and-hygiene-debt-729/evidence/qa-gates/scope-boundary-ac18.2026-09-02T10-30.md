# Scope boundary AC18 — exactly one production source file changed (P7-T2)

Timestamp: 2026-09-03T00-05

EXIT_CODE: 0

## Base re-derivation (D11)

```
$base = (git merge-base origin/main HEAD).Trim()
```

Observed `$base`: `8be5a6aac3b5a82c86241fbbf989fd9118602c56`, equal to the `BaseRef:` recorded by
P0-T14. This task proceeds on the recorded anchor.

## Commands

```
git diff --name-status $base HEAD
git status --porcelain
```

The anchored diff reports 57 entries: 32 `A`, 17 `D`, and 8 `M`. The porcelain status reports 10
entries, 5 of which name paths the anchored diff already lists. The union classified below is 65
distinct paths.

## Bucket 1 — production source (exactly one path)

| Path | Source |
|---|---|
| `TaskMaster/AppGlobals/NonBlockingDelay.cs` | `M` in anchored diff |

That is the complete bucket. No other non-test, non-documentation source file appears in either
output.

## Bucket 2 — test project asset (26 paths)

`SVGControl.Test` (8):

```
D  SVGControl.Test/Form1.Designer.cs
D  SVGControl.Test/Form1.cs
D  SVGControl.Test/Form1.resx
D  SVGControl.Test/Form2.Designer.cs
D  SVGControl.Test/Form2.cs
D  SVGControl.Test/Form2.resx
A  SVGControl.Test/NoLiveFormInTestAssemblyTests.cs
M  SVGControl.Test/SVGControl.Test.csproj
```

`TaskMaster.Test` (3):

```
M  TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs
M  TaskMaster.Test/TaskMaster.Test.csproj
M  TaskMaster.Test/packages.config
```

`UtilitiesCS.Test` (15):

```
D  UtilitiesCS.Test/Form1.Designer.cs
D  UtilitiesCS.Test/Form1.cs
D  UtilitiesCS.Test/Form1.resx
D  UtilitiesCS.Test/Form2.Designer.cs
D  UtilitiesCS.Test/Form2.cs
D  UtilitiesCS.Test/Form2.resx
D  UtilitiesCS.Test/Form3.Designer.cs
D  UtilitiesCS.Test/Form3.cs
D  UtilitiesCS.Test/Form3.resx
A  UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs
D  UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs
M  UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs
D  UtilitiesCS.Test/ResourceTests.cs
M  UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs
M  UtilitiesCS.Test/UtilitiesCS.Test.csproj
```

Every path in this bucket is inside a `*.Test` project directory, so none of them is production
source.

## Bucket 3 — feature documentation and evidence (33 paths)

From the anchored diff (30):

```
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/issue.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/plan.2026-09-02T08-59.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/no-fail-before-rationale.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/unauthorized-artifact-removal.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/build-taskmaster-test.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/coverage-delta.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/coverage-final.cobertura.xml
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/csharpier-check.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/csharpier-format.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/file-size-audit.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/msbuild-analyzers.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/msbuild-nullable.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/mstest-coverage.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/nuget-restore-after-package-edit.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/seam-build-analyzers.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/toolchain-single-pass.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/donotparallelize-classes.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/donotparallelize-diff.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/fail-before-exception.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/nonblockingdelay-tests.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/nonblockingdelay-zero-delay-observation.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-build-after.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-build-before.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-guard-fail-before.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-guard-pass-after.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/utilitiescs-build.2026-09-02T10-30.md
A  docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/utilitiescs-guard-pass.2026-09-02T10-30.md
A  docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md
```

From `git status --porcelain` (5 entries, of which 3 name paths the diff does not list):

```
 M docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/plan.2026-09-02T08-59.md
 M docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md
?? docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/
?? docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/scope-recap.2026-09-02T10-30.md
?? docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/scope-boundary-ac17.2026-09-02T10-30.md
```

Four paths are classified from the anchored diff rather than from porcelain status, exactly as
this task states: `issue.md`, `spec.md`, `research/research-729.2026-09-02T09-30.md`, and
`docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md` were
committed to this branch before Phase 7 runs, so each appears as an `A` entry. All four belong in
this third bucket.

`spec.md` additionally appears as an `M` entry in porcelain, carrying the revision-round-14
corrections that stay uncommitted until P8-T22 and the Block L insertion P7-T7 has not yet
written. That `M` entry also belongs in this third bucket.

`plan.2026-09-02T08-59.md` appears as both an `A` entry in the anchored diff and an `M` entry in
porcelain; the `M` is this plan's own task check-off state, written to disk as each task's
acceptance is met and committed by P8-T22. It is feature documentation and belongs in this third
bucket.

Per this task's explicit instruction, the promotion record
`docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md` is
excluded from the fourth bucket and placed here even though P0-T15 lists it as already tracked;
it is deliberate output of this work. For the same reason, every path under
`docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729` is placed here rather than
in the fourth bucket: the P0-T15 clause admits a path to the fourth bucket, it does not compel
one, and the fourth bucket's subject is agent-memory scratch.

## Bucket 4 — agent-memory scratch (5 paths)

| Path | Which allowance covers it |
|---|---|
| `.claude/agent-memory/atomic-executor/project_doubled_backslash_dedoubles_bash_to_native_exe.md` | Both: listed in the P0-T15 `PreExistingPaths:` set, and under `.claude/agent-memory/` |
| `.claude/agent-memory/atomic-planner/MEMORY.md` | Both: listed in the P0-T15 `PreExistingPaths:` set, and under `.claude/agent-memory/` |
| `.claude/agent-memory/task-researcher/MEMORY.md` | Both: listed in the P0-T15 `PreExistingPaths:` set, and under `.claude/agent-memory/` |
| `.claude/agent-memory/atomic-planner/project_729_dirty_tree_and_host_leak_plan_seams.md` | Both: listed in the P0-T15 `PreExistingPaths:` set, and under `.claude/agent-memory/` |
| `.claude/agent-memory/task-researcher/project_test_determinism_debt_729.md` | Both: listed in the P0-T15 `PreExistingPaths:` set, and under `.claude/agent-memory/` |

All five are `.claude/agent-memory/**` paths written by the persistent-memory systems of delegated
agents rather than by any task in this plan, which is what D10 records. None of them is staged or
committed by any task in this plan.

## Bucket totals

| Bucket | Count |
|---|---|
| 1 — production source | 1 |
| 2 — test project asset | 26 |
| 3 — feature documentation and evidence | 33 |
| 4 — agent-memory scratch | 5 |
| Total distinct paths | 65 |

Every path in the union of the two command outputs is assigned to exactly one bucket.

Output Summary: Exactly one path is classified as production source, and it is
`TaskMaster/AppGlobals/NonBlockingDelay.cs`. The remaining 64 distinct paths are 26 test project
assets, 33 feature-documentation and evidence paths, and 5 agent-memory scratch paths, each of the
last five covered by both the P0-T15 `PreExistingPaths:` allowance and the `.claude/agent-memory/`
allowance. AC18 holds.
