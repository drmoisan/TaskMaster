# Changed-file inventory (P7-T5)

Timestamp: 2026-09-03T00-10

EXIT_CODE: 0

## Base re-derivation (D11)

```
$base = (git merge-base origin/main HEAD).Trim()
```

Observed `$base`: `8be5a6aac3b5a82c86241fbbf989fd9118602c56`, equal to the `BaseRef:` recorded by
P0-T14.

## Commands

```
git diff --name-status $base HEAD
git status --porcelain
```

The anchored diff reports 57 entries: 32 `A`, **17 `D`**, and 8 `M`. The porcelain status reports
10 entries.

## Comparison against the plan's Complete file-write inventory

### Production source (exactly one file)

| Plan inventory entry | Observed | Agrees |
|---|---|---|
| `TaskMaster/AppGlobals/NonBlockingDelay.cs` — modified | `M` | yes |

### Test sources modified

| Plan inventory entry | Observed | Agrees |
|---|---|---|
| `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` | `M` | yes |
| `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` | `M` | yes |
| `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` | `M` | yes |

### Test sources created

| Plan inventory entry | Observed | Agrees |
|---|---|---|
| `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` | `A` | yes |
| `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` | `A` | yes |

### Test project files and package manifests modified

| Plan inventory entry | Observed | Agrees |
|---|---|---|
| `TaskMaster.Test/TaskMaster.Test.csproj` | `M` | yes |
| `TaskMaster.Test/packages.config` | `M` | yes |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | `M` | yes |
| `SVGControl.Test/SVGControl.Test.csproj` | `M` | yes |

### Files deleted (17)

| # | Plan inventory entry | Observed |
|---|---|---|
| 1 | `UtilitiesCS.Test/ResourceTests.cs` | `D` |
| 2 | `UtilitiesCS.Test/Form1.cs` | `D` |
| 3 | `UtilitiesCS.Test/Form1.Designer.cs` | `D` |
| 4 | `UtilitiesCS.Test/Form1.resx` | `D` |
| 5 | `UtilitiesCS.Test/Form2.cs` | `D` |
| 6 | `UtilitiesCS.Test/Form2.Designer.cs` | `D` |
| 7 | `UtilitiesCS.Test/Form2.resx` | `D` |
| 8 | `UtilitiesCS.Test/Form3.cs` | `D` |
| 9 | `UtilitiesCS.Test/Form3.Designer.cs` | `D` |
| 10 | `UtilitiesCS.Test/Form3.resx` | `D` |
| 11 | `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs` | `D` |
| 12 | `SVGControl.Test/Form1.cs` | `D` |
| 13 | `SVGControl.Test/Form1.Designer.cs` | `D` |
| 14 | `SVGControl.Test/Form1.resx` | `D` |
| 15 | `SVGControl.Test/Form2.cs` | `D` |
| 16 | `SVGControl.Test/Form2.Designer.cs` | `D` |
| 17 | `SVGControl.Test/Form2.resx` | `D` |

The observed `D` set and the plan's deletion list agree element for element, and the observed `D`
count is exactly seventeen. The anchored diff contains no `D` entry outside this list.

### Feature documentation and evidence written

Tracked and committed, appearing as `A` entries in the anchored diff:

```
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/issue.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/plan.2026-09-02T08-59.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md
docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/no-fail-before-rationale.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/unauthorized-artifact-removal.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/build-taskmaster-test.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/coverage-delta.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/coverage-final.cobertura.xml
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/csharpier-check.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/csharpier-format.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/file-size-audit.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/msbuild-analyzers.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/msbuild-nullable.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/mstest-coverage.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/nuget-restore-after-package-edit.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/seam-build-analyzers.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/toolchain-single-pass.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/donotparallelize-classes.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/donotparallelize-diff.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/fail-before-exception.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/nonblockingdelay-tests.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/nonblockingdelay-zero-delay-observation.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-build-after.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-build-before.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-guard-fail-before.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/svgcontrol-guard-pass-after.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/utilitiescs-build.2026-09-02T10-30.md
docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/utilitiescs-guard-pass.2026-09-02T10-30.md
```

Uncommitted at this point, reported by `git status --porcelain`:

```
 M docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/plan.2026-09-02T08-59.md
 M docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md
?? docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/
?? docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/scope-recap.2026-09-02T10-30.md
?? docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/scope-boundary-ac17.2026-09-02T10-30.md
```

`git status --porcelain` collapses the untracked `evidence/baseline/` tree to a single directory
entry. Its fifteen files, enumerated with `git ls-files -o --exclude-standard`, are:

```
evidence/baseline/analyzer-path-audit.2026-09-02T10-30.md
evidence/baseline/base-ref.2026-09-02T10-30.md
evidence/baseline/citation-verification.2026-09-02T10-30.md
evidence/baseline/coverage-baseline.cobertura.xml
evidence/baseline/csharpier-check.2026-09-02T10-30.md
evidence/baseline/dotnet-coverage-tool.2026-09-02T10-30.md
evidence/baseline/dotnet-sdk-bootstrap.2026-09-02T10-30.md
evidence/baseline/dotnet-tool-restore.2026-09-02T10-30.md
evidence/baseline/msbuild-analyzers.2026-09-02T10-30.md
evidence/baseline/msbuild-nullable.2026-09-02T10-30.md
evidence/baseline/mstest-coverage.2026-09-02T10-30.md
evidence/baseline/nonblockingdelay-coverage-baseline.2026-09-02T10-30.md
evidence/baseline/nuget-restore.2026-09-02T10-30.md
evidence/baseline/phase0-instructions-read.2026-09-02T10-30.md
evidence/baseline/preexisting-worktree-state.2026-09-02T10-30.md
```

Every one of them is named in Phase 0 of the plan and is therefore covered by the inventory entry
"all artifacts named in Phases 0–8 under `.../evidence/`". The three later scope-boundary
artifacts this phase writes (`scope-boundary-ac18`, `scope-boundary-ac19`, `scope-boundary-ac20`)
and this inventory itself are also named in Phase 7 and covered by the same entry; they are
created after the two commands above ran, so they do not appear in either output.

## Deltas:

Six differences, every one of them explained. None is unexplained.

1. **`spec.md` Block L insertion authored by P7-T7 — not yet present.** P7-T7 runs after this
   task, so the five-line Block L insertion under the Finding 4 out-of-scope bullet cannot appear
   in this task's diff. `spec.md` is already named in the Complete file-write inventory, which
   states that the insertion is written by P7-T7.

2. **Phase 8 acceptance-criteria checkbox edits to `spec.md` — not yet present.** Phase 8 runs
   after this task, so the AC1 through AC21 checkbox transitions cannot appear in this task's
   diff either. `spec.md` is already named in the inventory on the same terms.

3. **`docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/issue.md` appears as
   an `A` entry rather than as an untracked path.** It was authored before this plan and is
   already tracked and committed on this branch, so the anchored diff reports it as an addition
   relative to `$base` while `git status --porcelain` reports nothing for it. This plan does not
   edit its content.

4. **`docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md`
   appears as an `A` entry rather than as an untracked path.** Same mechanism and same
   explanation as item 3. P7-T9 will append a correction note to it after this task.

5. **`spec.md` and
   `docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md`
   appear as `A` entries for the same reason.** Both are tracked and committed on this branch
   before Phase 7 runs. `spec.md` additionally appears as an `M` entry in
   `git status --porcelain`, carrying the revision-round-14 corrections applied before execution
   resumed; it is already named in the Complete file-write inventory and so is not an unexplained
   difference. The promotion record is deliberate output of this work rather than agent scratch,
   which is the classification P7-T2 also applies to it.

6. **Five `.claude/agent-memory/**` paths appear in `git status --porcelain` and in no inventory
   entry.** They are written by the persistent-memory systems of delegated agents outside any task
   in this plan, which is what D10 records and what P0-T15 documents as pre-existing. No task in
   this plan writes, stages, or commits any of them, so their absence from a *file-write*
   inventory is correct rather than a discrepancy. The five are:
   `.claude/agent-memory/atomic-executor/project_doubled_backslash_dedoubles_bash_to_native_exe.md`,
   `.claude/agent-memory/atomic-planner/MEMORY.md`,
   `.claude/agent-memory/task-researcher/MEMORY.md`,
   `.claude/agent-memory/atomic-planner/project_729_dirty_tree_and_host_leak_plan_seams.md`, and
   `.claude/agent-memory/task-researcher/project_test_determinism_debt_729.md`.

Output Summary: The observed changed-file set agrees with the plan's Complete file-write inventory
element for element. The anchored diff carries exactly seventeen `D` entries and they match the
plan's seventeen-file deletion list exactly. Six differences are recorded in `Deltas:` and every
one is explained: the two `spec.md` changes that later tasks write, the four already-tracked
feature-documentation paths that surface as `A` entries in the anchored diff rather than as
untracked paths, and the five agent-memory scratch paths that no plan task writes.
