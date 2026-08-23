# Feature Audit: Folder-tree dispatcher thread affinity

## Scope and Baseline

- Base: `origin/main` at `ce0c91e686bf7e060aaab6f185ee6883269e4fd4`.
- Head: uncommitted working tree on `bug/folder-tree-dispatcher-thread-affinity-420`.
- Canonical context: `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`; the appendix's working-tree inventory was used because the summary has no committed range.
- Work mode: `full-bug`; the authoritative acceptance-criteria source is `docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/spec.md`.

## Acceptance Criteria Inventory

| ID | Criterion | Checked in source |
| --- | --- | --- |
| AC1 | Worker cold request completes without the dispatcher-free `WpfDispatcherYield` exception. | Yes |
| AC2 | Composition, notification-sink construction, live adapter accesses, and post-yield continuations for cold build or refresh run on the captured STA. | Yes |
| AC3 | Production traversal remains strict WPF dispatch with no `Task.Yield` or worker-local fallback. | Yes |
| AC4 | One service instance retains coalescing, cancellation, state, invalidation, publication, and disposal behavior. | Yes |
| AC5 | FilterOlFolders cold initialization awaits without blocking UI and wires after snapshot acquisition. | Yes |
| AC6 | Deterministic no-external-dependency MSTest coverage proves the listed behavior. | Yes |
| AC7 | Final C# toolchain passes and changed behavior meets coverage policy. | Yes |
| AC8 | Documentation records final decisions, validation, and approved scope deviations. | Yes |

## Acceptance Criteria Evaluation

| ID | Result | Evidence and rationale |
| --- | --- | --- |
| AC1 | PASS | Worker-originated cold-build regression and strict `WpfDispatcherYield` regression are recorded as passing. |
| AC2 | PARTIAL | Construction and cold-build tests exist, but no test proves notification-triggered refresh dispatch, notification cleanup affinity, UI/worker first-composition ordering, or disposal behavior. |
| AC3 | PASS | Diff inspection found no `Task.Yield` fallback; strict-yield test passes. |
| AC4 | FAIL | Service may publish after disposal and dispose the notification sink off the STA. The service gate may deadlock with synchronous UI invocation and UI disposal. |
| AC5 | FAIL | `CreateAsync` success path is covered, but public construction fire-and-forgets initialization and late `FormClosed` subscription allows close-during-load wiring to a closed viewer. |
| AC6 | PARTIAL | Existing deterministic tests are appropriate but omit the new concurrency, disposal, fault, and closed-view cases. |
| AC7 | FAIL | CSharpier and both builds pass, and recorded MSTest passes; however, the explicit `>=90%` new-method requirement is not met and the coverage comparison scope is invalid. |
| AC8 | FAIL | The spec and final QA delta state coverage compliance and no scope deviation, which conflicts with the final coverage artifact and review findings. |

## Summary

The original root-cause correction is substantially implemented: worker-originated cold builds are dispatched to an STA-hosted path, cooperative WPF dispatcher yielding remains in place, and the FilterOlFolders path has an asynchronous factory. The full-bug delivery is incomplete because high-severity lifecycle defects remain, acceptance criteria AC4, AC5, AC7, and AC8 are not met, and AC2 and AC6 are only partially evidenced.

## Acceptance Criteria Check-off

No acceptance criteria were checked off by this review. `spec.md` currently marks all eight items complete, but AC2 and AC6 are PARTIAL and AC4, AC5, AC7, and AC8 are FAIL; the review must not preserve those completed checkboxes as verified evidence. The remediation executor must reconcile source checkboxes after the corrective implementation and final QA.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/spec.md`
- Total AC items: 8
- Verified PASS: 2
- PARTIAL: 2
- FAIL: 4
- Remaining verification: AC2, AC4, AC5, AC6, AC7, AC8

## PR Readiness Recommendation

No-go. Do not open or approve a PR until the remediation plan addresses the service initialization/disposal races, FilterOlFolders lifecycle and fault handling, and policy-compliant coverage measurement with the required new-method coverage.
