# Feature Audit - folder-tree-cache-and-refresh (Issue #214)

- Date: 2026-06-24T19-23
- Reviewer: feature-reviewer agent
- Work Mode: full-feature
- Base: `origin/main` @ `168eba0ba1f79290be9eda29edc4332ac1ce2061`
- Head: `refactor/folder-tree-cache-and-refresh-214` @ `c2423376f0e37e61737aba57a788b3be3bdd0bf4`
- PR context: `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`

## Scope and Baseline

The audit reviewed the committed feature branch `refactor/folder-tree-cache-and-refresh-214` at `c2423376` against `main` / `origin/main` at merge base `168eba0`. The active feature folder is `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214`.

Primary sources:

- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/issue.md`
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/spec.md`
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/user-story.md`
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/plan.2026-06-24T15-42.md`
- Final QA and other evidence under `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/`

Baseline and scope checks:

- Working tree was clean before writing review artifacts.
- PR context is fresh for head `c2423376`.
- `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` has no branch diff.
- Active feature docs do not add out-of-scope issue references. The PR context appendix mirrors the original GitHub issue #214 body, which contains related startup-background references outside the active feature artifacts.
- Startup-specific junk-folder path navigation was not reimplemented in the changed branch files reviewed.

## Acceptance Criteria Inventory

Authoritative full-feature AC sources:

- `spec.md`: AC1 through AC15, all currently marked `[x]`.
- `user-story.md`: AC1 through AC16, all currently marked `[x]`.

The `issue.md` source also contains unchecked issue-level acceptance criteria. Per the repository AC tracking skill, full-feature mode uses `spec.md` and `user-story.md` as authoritative AC sources.

## Acceptance Criteria Evaluation

| Source | AC | Verdict | Evidence |
|---|---|---|---|
| spec | AC1 - Work mode and scope | PASS | Full-feature mode, issue #214 folder, branch, and PR context match the requested scope. |
| spec | AC2 - Shared lazy cache | FAIL | `GetSnapshotAsync` returns `_snapshot` whenever state is `Current` without checking request scope; `EmailDataMiner.FolderExtraction.cs` retains direct `FolderTree` helpers. |
| spec | AC3 - STA-safe traversal | PARTIAL | No evidence of `Task.Run(() => new FolderTree)` in in-scope searched files, but live traversal remains synchronous before first dispatcher yield. |
| spec | AC4 - Cooperative responsiveness | FAIL | `FolderTreeSnapshotBuilder.BuildSnapshotAsync` calls `_reader.ReadFolders(...)` before any `YieldIfNeededAsync` call; production reader fully walks Outlook folders synchronously. |
| spec | AC5 - No Application.DoEvents | PASS | Banned API evidence and reviewer added-line search found no added `Application.DoEvents`. |
| spec | AC6 - Bounded build | FAIL | Cancellation is checked before `ReadFolders`, but the live reader performs full synchronous traversal with cancellation checks only inside the reader and no dispatcher yield during COM enumeration. |
| spec | AC7 - Cache invalidation correctness | FAIL | Production notification sink creates no live Outlook subscriptions; store-scoped refresh can publish a partial snapshot as current. |
| spec | AC8 - Multiple-store correctness | FAIL | A store-scoped current snapshot can be returned for later incompatible requests; store-scoped refresh can drop unaffected stores. |
| spec | AC9 - Notification lifecycle | FAIL | `OutlookFolderNotificationSink(Outlook.NameSpace)` uses an empty subscription list, so production store/folder event sources are not subscribed. |
| spec | AC10 - PropertyChanged lifecycle | PASS | Compatibility view and filter controller dispose/unsubscribe handlers; tests cover handler-count behavior. |
| spec | AC11 - Concurrency and staleness | FAIL | Concurrent in-flight behavior is tested, but request-scope and store-scoped publication can produce stale/incomplete current snapshots. |
| spec | AC12 - Caller-local selection | PASS | Selection overlay and compatibility view keep caller selection outside shared immutable snapshot nodes. |
| spec | AC13 - Testability seam | PARTIAL | Fake seams exist and tests pass, but tests miss production notification subscription and request-scope defects. |
| spec | AC14 - Out-of-scope protection | PASS | `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` has no diff; startup-specific junk-folder path navigation was not modified. |
| spec | AC15 - Toolchain and coverage | PASS | Final QA evidence reports CSharpier, analyzers, nullable, MSTest coverage, coverage comparison, and file-size checks passing. |
| user-story | AC1 - Full-feature scope is preserved | PASS | Scope, folder, branch, and issue identifiers match issue #214. |
| user-story | AC2 - Shared lazy cache | FAIL | Cache does not validate request coverage before returning current snapshot; direct EmailDataMiner `FolderTree` construction remains. |
| user-story | AC3 - Direct construction retired for in-scope callers | FAIL | `EmailDataMiner.FolderExtraction.cs` has `FolderTree tree = new(...)` in `GetOlFolderTree` helpers called by `ScrapeEmailsCore`. |
| user-story | AC4 - STA-safe COM traversal | PARTIAL | Traversal is not offloaded by `Task.Run`, but cooperative behavior during traversal is incomplete. |
| user-story | AC5 - Cooperative responsiveness | FAIL | Dispatcher yield occurs after synchronous reader enumeration, not during live hierarchy traversal. |
| user-story | AC6 - No `Application.DoEvents` | PASS | Banned API evidence passes for added issue #214 C# lines. |
| user-story | AC7 - Bounded traversal | FAIL | The main live enumeration loop in the reader has no dispatcher-yield cadence. |
| user-story | AC8 - Folder invalidation | FAIL | Production notification subscriptions are empty; store-scoped refresh can overwrite the cache with a partial snapshot. |
| user-story | AC9 - Multiple-store correctness | FAIL | Current snapshot scope is not tracked or checked; multi-store callers can receive incomplete snapshots. |
| user-story | AC10 - Notification disposal | FAIL | Disposal exists for subscription objects, but the production constructor creates no subscription objects to own or dispose. |
| user-story | AC11 - Node handler disposal | PASS | Handler disposal is implemented and covered for compatibility views. |
| user-story | AC12 - Concurrency and staleness | FAIL | In-flight coalescing exists, but staleness and current publication can be wrong for store-scoped requests. |
| user-story | AC13 - Caller-local selection state | PASS | Selection overlays do not mutate shared snapshot nodes. |
| user-story | AC14 - Testability without live Outlook | PARTIAL | Unit tests avoid live Outlook COM, but miss production subscription construction and request-scope cache correctness. |
| user-story | AC15 - Issue #214 startup-scope exclusion | PASS | `AppOlObjects.JunkFolders.cs` has no diff; startup-specific junk-folder paths were not modified. |
| user-story | AC16 - Toolchain and coverage | PASS | Final QA and coverage evidence pass. |

## Summary

Feature verdict: REMEDIATION_REQUIRED.

The feature has substantial structure and passing QA evidence, but multiple core behavior requirements are not met. The implementation does not yet provide production notification invalidation, cooperative yielding during live hierarchy enumeration, request-scope-safe cache retrieval, or multi-store-safe store refresh. The EmailDataMiner migration evidence is incomplete and the added partial still contains direct `FolderTree` construction paths.

## Acceptance Criteria Check-off

No acceptance criteria were newly checked off during this review. The authoritative AC items in `spec.md` and `user-story.md` were already marked `[x]` before the review, but this audit finds several checked items are not supported by the implementation evidence.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/spec.md`
- Total AC items: 15
- Checked off in source: 15
- Verified PASS: 5
- Verified PARTIAL: 2
- Verified FAIL: 8
- Items not fully verified: spec AC2, AC3, AC4, AC6, AC7, AC8, AC9, AC11, AC13

### Acceptance Criteria Status

- Source: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/user-story.md`
- Total AC items: 16
- Checked off in source: 16
- Verified PASS: 6
- Verified PARTIAL: 2
- Verified FAIL: 8
- Items not fully verified: user-story AC2, AC3, AC4, AC5, AC7, AC8, AC9, AC10, AC12, AC14
