# Research: breadcrumb dispatcher message wording and handoff record attribution (Issue #941)

Timestamp: 2026-09-29T23-05
Scope: research only. All line numbers observed in the item-941 worktree.

## Summary of conclusions

1. Only one of the two message sites misdescribes its mechanism. The `Dispatch` site (`BreadcrumbUiDispatcher.cs:101`) is reached only when the caller is off the owner thread, so "cross-thread" is accurate there. The `DispatchValue` site (`BreadcrumbUiDispatcher.cs:183`) is reached for every caller outside an executing dispatcher callback, including a caller on the owner thread, so "cross-thread" is inaccurate there. Recommended replacement for line 183 only: `The owner-thread-only test dispatcher cannot run value-producing UI work outside an executing Dispatch callback.` Assertable token: `outside an executing Dispatch callback`. The issue's proposed fix says "at both sites"; the evidence supports changing one.
2. Wrong sentences in the p5-t14 handoff record: lines 23-25 (Summary sentence covering "Two tests" against the owner-thread-id check) and lines 40-42 (citation bullet for `BreadcrumbUiThreadDispatchTests.cs:298-307`). The first cited test reaches `Dispatch` and the owner-thread-id check; the second reaches `DispatchValue`, which never reads `_ownerThreadId`. Corrected text is in section 2.
3. Tests that match the message text: exactly two. `BreadcrumbUiThreadDispatchTests.cs:305` (`DispatchValue` site, line 183) and `BreadcrumbPopupBoundaryCoverageTests.cs:86` (`Dispatch` site, line 101). Non-test files that quote the text are listed in section 3.2.
4. If only line 183 is narrowed: the assertion at `BreadcrumbUiThreadDispatchTests.cs:305` fails and must change to `*outside an executing Dispatch callback*`; the assertion at `BreadcrumbPopupBoundaryCoverageTests.cs:86` passes unchanged. If line 101 were also narrowed, line 86 would need a matching change.
5. Minimal fix touches 3 tracked files: `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` (285 lines), `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` (480 lines), and the p5-t14 handoff record. No file exceeds 500 lines; the test file is 20 lines from the limit.

## 1. Mechanism per message site

### Site A: `Dispatch`, `BreadcrumbUiDispatcher.cs:97-105`

Governing conditions, in order:
- `:78` `if (IsCurrentBoundary())` runs inline and returns. Not reached by the throw.
- `:97` `if (_context == null)` followed by `:99-103` `Report(new InvalidOperationException("The owner-thread-only test dispatcher cannot marshal cross-thread UI work."))` and `:104` `return Task.CompletedTask;`.

For `_context == null` (test dispatcher, created by `CreateForCurrentThreadTests`, `:62-65`), `IsCurrentBoundary()` (`:255-278`) returns true if `_executingDispatcher` is this instance (`:258`) or, at `:276-277`, `_ownerThreadId.HasValue && Environment.CurrentManagedThreadId == _ownerThreadId.Value`. So `:97` is reached only when the caller is neither inside an executing callback nor on the owner thread. On the owner thread it never reaches `:97`. The description "cannot marshal cross-thread UI work" matches: the caller is on another thread. Verdict: accurate; no change needed. It reports through the sink and does not fault a task (returns `Task.CompletedTask`).

### Site B: `DispatchValue`, `BreadcrumbUiDispatcher.cs:180-188`

Governing conditions:
- `:166` `if (ReferenceEquals(_executingDispatcher, this))` runs inline and returns.
- `:180` `if (_context == null)` followed by `:182-184` `var failure = new InvalidOperationException("The owner-thread-only test dispatcher cannot marshal cross-thread UI work.");`, `:185-186` conditional `Report(failure)`, `:187` `return Task.FromException<T>(failure);`.

`DispatchValue` does not call `IsCurrentBoundary()` and never reads `_ownerThreadId` (the field is read only at `:276-277`; verified by reading the whole file). The comment at `:164-165` states the intent: only a currently executing dispatcher callback proves inline access is safe, and ambient context and thread identity are not trusted. Therefore with `_context == null` the fault is raised for every caller not inside an executing callback, on any thread, including the owner thread. The message word "cross-thread" is inaccurate: the condition is "outside an executing Dispatch callback", not "on a different thread". Verdict: misdescribes the mechanism; change wording.

Corroboration in the repo: `WebView2BreadcrumbHost.cs:203-205` remark says the value-returning overload "runs inline only from inside an already-executing `Dispatch` callback and faults on an owner-thread-only test dispatcher". The #931 spec triage table (`2026-09-28-tests-depend-on-uncontrolled-environment-931/spec.md:172`) and research (`.../research/2026-09-28T20-15-tests-depend-on-uncontrolled-environment-research.md:72`) reach the same conclusion.

### Proposed wording (site B only)

```
"The owner-thread-only test dispatcher cannot run value-producing UI work outside an executing Dispatch callback."
```

- Single string literal, no concatenation, no braces, no quote or escape characters, so a FluentAssertions wildcard pattern is not affected.
- Distinctive token for tests: `outside an executing Dispatch callback`. It appears nowhere in the repository today (grep of `*.cs`).
- It deliberately omits the substring `cannot marshal`, so the two sites remain distinguishable by message and the test at `BreadcrumbPopupBoundaryCoverageTests.cs:86` can never be satisfied by site B.
- CSharpier does not wrap string literals, so the longer literal is stable under `csharpier format`. Existing literal is 20 spaces of indent plus 80 characters; the new one is about 20 plus 112.

Site A unchanged. If the maintainer prefers the issue's literal "both sites", site A would need a token that is still accurate (for example "off the owner thread"); the evidence does not require it.

## 2. Handoff record errors

File: `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md`.

Wrong statements:

- Lines 23-25 (Entry 1 Summary): "Two tests assert that a cross-thread call fails, while obtaining their "different thread" from a `Task.Run` delegate, against `BreadcrumbUiDispatcher`'s owner-thread-id check rather than against `ItemViewer`'s boundary guard." Only the first test is against the owner-thread-id check.
- Lines 26-31 (continuation of that summary paragraph): the inlining consequence ("at which point the owner check passes and the expected failure never occurs") applies only to the `Dispatch` site. At the `DispatchValue` site the fault is produced on the owner thread as well, so inlining does not change the outcome.
- Lines 40-42 (citation bullet): "`QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:298-307`: creates a dispatcher for the current thread and then awaits a `Task.Run` delegate expected to throw a cross-thread marshalling error." At that location the delegate at line 301 calls `testDispatcher.DispatchValue(() => 1)`, which does not consult the owner-thread-id check. It is not exposed to the #900 inlining hazard.
- Lines 43-44 also cite `BreadcrumbUiDispatcher.cs:40`, `:54`, `:64` as "the owner check compares `Environment.CurrentManagedThreadId` against `_ownerThreadId`". Those lines are the field assignment (`:40`) and the two factory arguments (`:54`, `:64`); the comparison is at `:276-277` in the current file. This applies to the `Dispatch` path only.

Correct attribution: line 301 of `BreadcrumbUiThreadDispatchTests.cs` reaches `DispatchValue` (`BreadcrumbUiDispatcher.cs:180-188`), governed by "not inside an executing callback and `_context == null`", not by the owner-thread-id check. `BreadcrumbPopupBoundaryCoverageTests.cs` line 82 (formerly cited as 58-61) reaches `Dispatch` and the owner-thread-id check at `:276-277`. Note that the first site in the record was subsequently reworked under #931 (dedicated worker thread, now lines 66-87); the record is a historical artifact and only the attribution should be corrected.

Exact corrected sentences:

Replacement for lines 23-25 first sentence:

> Two tests assert that a cross-thread call fails, while obtaining their "different thread" from a `Task.Run` delegate. Only the first is exercising `BreadcrumbUiDispatcher`'s owner-thread-id check (through `Dispatch`); the second reaches `DispatchValue`, which never reads the owner thread id.

Replacement for lines 26-31 last sentence pair, appended after the existing inlining explanation:

> The inlining hazard described here applies to the `Dispatch` site only. `DispatchValue` on an owner-only dispatcher faults for every caller outside an executing dispatcher callback, on any thread, so the second site's outcome does not depend on which thread runs the delegate.

Replacement for lines 40-42:

> - `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:298-307`: creates a dispatcher for the current thread and then awaits a `Task.Run` delegate (line 301) that calls `DispatchValue`, expected to fault with a marshalling error. `DispatchValue` does not use the owner-thread-id check (`BreadcrumbUiDispatcher.cs:180-188`); it faults for every caller outside an executing callback, including the owner thread, so this site is not exposed to the inlining hazard.

Replacement for lines 43-44:

> - `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:40`, `:54`, `:64`: where `_ownerThreadId` is stored and supplied; the owner check compares `Environment.CurrentManagedThreadId` against it at `:276-277`, and is used by `Dispatch` only.

Source of truth for the correction: the #931 handoff (`2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/p4-t13-follow-up-handoff.2026-09-29T09-46.md`, Follow-ups 3 at lines 19-23 and 4 at lines 25-29) and #931 spec triage row at `spec.md:172` and item 4 at `spec.md:278`. Both documents exist in the worktree at those paths.

## 3. Message text census

### 3.1 Tests

Primary strategy: Grep `cannot marshal|owner-thread-only|cross-thread UI` (case-insensitive) over `*.cs` in the whole worktree. Cross-check strategy: Grep `WithMessage\(|Message\.Should\(\)|\.Message\b` over `QuickFiler.Test/*.cs` and manual review of each hit; plus Grep `CreateForCurrentThreadTests` over `*.cs` to enumerate every user of the test dispatcher.

Primary member set (test files): 2.
1. `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:305`: `.WithMessage("*cannot marshal cross-thread UI work*")`, inside `ProductionCaptureWithoutUiContext_FailsFast` (test method starts at `:277`). Exercises site B (`DispatchValue`, `:183`) through `testDispatcher.DispatchValue(() => 1)` at `:301`.
2. `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:86`: `errors.Should().ContainSingle().Which.Message.Should().Contain("cannot marshal");`, inside `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` (`:66-87`). Exercises site A (`Dispatch`, `:101`) via `dispatcher.Dispatch(() => executions++)` at `:82`.

Cross-check member set: the message-assertion hits in QuickFiler.Test reviewed include `BreadcrumbUiThreadDispatchTests.cs:291` (`*owning UI synchronization context*`, `CaptureCurrent` message, unrelated) and `:305`, and `BreadcrumbPopupBoundaryCoverageTests.cs:86`; all other `WithMessage`/`.Message` hits are unrelated strings (for example `*already*`, `*returned no surface*`, `messenger`, `missing`). Users of `CreateForCurrentThreadTests` in tests: `FolderBreadcrumbAssetContractTests.cs:184`, `BreadcrumbUiThreadDispatchTests.cs:299`, `BreadcrumbSelectorCoordinatorTests.cs:462`, `BreadcrumbPopupControlDispatchTests.cs:175` (the `BreadcrumbPopupUiOperations` variant), `BreadcrumbDuplicateIdentityIntegrationTests.cs:151`, `BreadcrumbDropDownReadinessTests.cs:314`, `BreadcrumbBridgeCoordinatorProbabilityTests.cs:150`. Of these only `:299` and the boundary test assert message text. Multi-line `.Message.Should()` hits in other files were not individually verified to be unrelated beyond the primary text grep, which found no other file containing the fragments.

Member-set comparison: primary set (2 tests) is consistent with the cross-check; no additional test matches the fragments. Count: 2. Other test projects: the primary grep covered the whole worktree and found no `*.cs` hit outside QuickFiler.Test and QuickFiler.

No test in the repository asserts on `owner-thread-only` alone.

### 3.2 Non-test files that quote the text (would go stale or are historical)

Production:
- `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:101` and `:183` (message literals); `:59` (XML summary "owner-thread-only boundary"); `:274` (comment). The `:59-61` summary "Cross-thread work is reported instead of being scheduled on a generic context" is accurate for `Dispatch`; `DispatchValue` behavior is described by the `:164-165` comment.
- `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:205` (remark; already accurate).

Documentation and evidence quoting the full message (would be stale after a site-B change; most are dated records that should not be edited):
- `docs/features/active/2026-08-25-itemviewer-breadcrumb-lifecycle-defects-488/spec.md:163` and `.../research/2026-08-25T10-00-itemviewer-breadcrumb-lifecycle-defects-research.md:365` (both describe a faulted `DispatchValue` task with this message).
- `docs/features/active/2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455/research/05-BreadcrumbUiDispatcher.md:109`.
- `docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/research/research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md:397`.
- `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/research/2026-09-16T23-50-breadcrumb-thread-affinity-tests-taskrun-distinct-thread-research.md:92`.
- `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/` spec.md lines 37, 66, 87, 236, 277; research line 72; plan lines 94, 98, 148, 386, 408, 609, 872, 936; feature-audit line 50; code-review line 44; many `evidence/` census files (token counts of `cannot marshal`, including `mutation-owner-only-dispatcher-guard.md:19`).
- `docs/features/archive/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/p5-uidispatch-assertion-inventory-before.2026-07-22T15-07.md:113`.
- `docs/features/potential/promoted/2026-09-29-breadcrumb-dispatch-message-and-handoff-record-inaccurate.md:35` and this feature's `issue.md:37` (the defect statement itself).
- `docs/features/active/2026-08-24-webview2-host-initializer-defects-476/` records use "owner-thread-only test dispatcher" without the full message.

Recommendation: leave dated evidence, spec, and research records unchanged; they record what the message said at the time. #931 census evidence counts the token `cannot marshal` by design and is frozen.

## 4. Effect of narrowing on existing assertions

| Assertion | Site | If site B narrowed only | If site A also narrowed |
|---|---|---|---|
| `BreadcrumbUiThreadDispatchTests.cs:305` `*cannot marshal cross-thread UI work*` | B (`:183`) | Fails. Change to `.WithMessage("*outside an executing Dispatch callback*")` | Same change |
| `BreadcrumbPopupBoundaryCoverageTests.cs:86` `Contain("cannot marshal")` | A (`:101`) | Passes unchanged | Fails unless message retains `cannot marshal`, else change to the new site A token |

Additional test change worth considering (optional): the discriminating property of the new wording is that the fault occurs on the owner thread too. A regression test calling `testDispatcher.DispatchValue(() => 1)` directly on the owner thread and asserting the same token would prove the narrowing; note that the test file is 480 lines, so adding more than about 20 lines would exceed the 500-line limit, and the boundary test file is 386 lines. A short addition to `BreadcrumbPopupBoundaryCoverageTests.cs` is preferable, or extend the existing test at `:298-308` to also assert the owner-thread call with a small helper. Under the bugfix workflow the changed assertion at `:305` is itself the failing-first test (it fails against the old text once changed).

## 5. Files a minimal fix modifies

1. `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` (285 lines): message literal at `:183` only.
2. `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` (480 lines): `WithMessage` pattern at `:305`.
3. `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md` (181 lines): lines 23-31 and 40-44.

No file over 500 lines is touched. Repository toolchain: csharpier, analyzers, nullable build, and the MSTest run apply to items 1 and 2; `BreadcrumbUiDispatcher.cs` carries `#nullable enable` and the change introduces no nullable flow.

## Rejected alternative

Narrowing both sites (as written in the issue's Proposed Fix): rejected because site A's condition is exactly an off-owner-thread caller, for which the current message is accurate, and it would force a second assertion change at `BreadcrumbPopupBoundaryCoverageTests.cs:86` with no accuracy gain.
