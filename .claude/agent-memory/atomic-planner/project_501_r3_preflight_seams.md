---
name: project-501-r3-preflight-seams
description: "#501 preflight round 3: repo-wide 0-skipped gates unsatisfiable (5 sibling [Ignore]s in UtilitiesCS.Test); BASELINE_FAILURE_SET subset pattern; Task.CompletedTask singleton breaks reference-inequality assertions"
metadata:
  type: project
---

Three seams from #501 preflight round 3 (plan `plan.2026-08-24T09-40.md`), generalizable to any TaskMaster plan.

1. **Repo-wide "0 skipped / 0 failed" full-suite gates are unsatisfiable.** UtilitiesCS.Test carries 5 pre-existing ACTIVE `[Ignore]` attributes (`InputBox_Test.cs:11`, `ResourceTests.cs:17`, `:25`, `:108`, `YesNoToAll_Test.cs:10`), so any repo-wide run reports skipped >= 5, and scope locks forbid editing sibling files to remove them. Pattern that works: baseline task records observed `EXIT_CODE:` (non-zero recorded, not remediated) plus an enumerated `BASELINE_FAILURE_SET` of failing FQNs (explicitly-empty is valid); final-QC gates 0 failed/0 skipped WITHIN the owned test assembly only, and for every other `*.Test.dll` requires the failing set be a SUBSET of `BASELINE_FAILURE_SET` (no new failures). Skips outside the owned assembly are recorded, not gated, with the five `[Ignore]` sites named as justification.

**Why:** absolute suite-wide gates deflate to unsatisfiable the moment any sibling assembly carries an active `[Ignore]` or a pre-existing failure; the subset relation still detects regressions introduced by the plan.

2. **`Task.CompletedTask` is a process-wide singleton.** A test asserting "post-call `SuggestionsUpgrade` is NOT reference-equal to the captured handle" is unsatisfiable if the handle was captured at initial state (`= Task.CompletedTask`), because a later `Task.CompletedTask` assignment is the SAME object. The arrange must first make the property genuinely pending (strict `Mock<IFolderHierarchyProvider>` gated on an uncompleted `TaskCompletionSource<FolderTreeNodeKey>` — precedent `BreadcrumbCoordinatorLifecycleTests.cs:340-346` with its `Configure(provider, path, gate.Task, key)` helper), call the population entry point, capture the pending task, and assert `IsCompleted == false` before the act step. Never complete the gating TCS: keeps the test single-threaded/no-wait.

**How to apply:** whenever an acceptance clause asserts reference-inequality on a Task-typed property, check whether the "before" value could be the `CompletedTask` singleton; if so, add an explicit pending-state arrangement plus an `IsCompleted == false` pre-assert to the task text. Related: [[feedback_postformat_file_size_audit]] (the same round also required an explicit post-format 500-line re-audit task — a "re-runs after Phase 7" sentence with no owning task is a preflight defect; give the post-format leg its own task and cite BOTH legs in the AC check-off).
