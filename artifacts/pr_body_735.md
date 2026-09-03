# fix(ribbon): repair dead Explorer callbacks, guard Clear Spam Manager, and make toggle-state writes compare-and-apply

## Summary

- Repairs three defects in the Outlook Explorer ribbon consolidated under issue #735: four check-box `onAction` bindings that named methods which do not exist, an unguarded globals dereference in the Clear Spam Manager command, and a last-writer race that could leave an engine toggle displaying the opposite of the engine's real state.
- Makes the toggle-state cache versioned and compare-and-apply. Each writer takes a monotonic ticket immediately before its activation read and stores through a compare-and-swap that refuses any write whose observation began earlier than one already recorded.
- Extracts the Clear Spam Manager readiness decision into a new host-neutral `SpamManagerResetGate`, so the logic that previously sat inside a coverage-exempt, COM-bound method is now unit-testable and fully covered.
- Adds 27 tests across four fixtures, including six deterministic race reproductions driven by held completion sources rather than by timing.
- Removes a ribbon button whose callback was never implemented anywhere in the solution.

## Why

Three findings from the issue, each with a distinct root cause:

**Finding 1 — dead XML-to-handler bindings.** The Explorer CustomUI document declared five callback names that resolve to no public method on the ribbon viewer type. Four were a spelling drift: the XML said `MoveEntireConversation_Clicked` while the handler is `MoveEntireConversation_Click`. Office binds callbacks by name at ribbon-load time, so each of the four check boxes silently did nothing when clicked. The fifth, `BtnMigrateIDs_Click`, has no implementation anywhere and no design document proposes the behaviour, so the button is removed rather than implemented.

**Finding 2 — unguarded globals dereference.** `ClearSpamManagerAsync` dereferenced the globals object three times with no readiness check. Invoked before add-in initialization completes, it raised a `NullReferenceException`. An inline null guard was explicitly disrecommended on the predecessor issue, because it would place the guard permanently inside the ribbon controller's pre-existing type-level coverage exemption where it can never be tested.

**Finding 3 — last-writer toggle-state race.** Two writers populate the pressed-state cache: the user-initiated toggle and a lazy prime started from a cache miss during a ribbon paint. Both stored unconditionally, and completion order does not track observation order, so a prime that began earlier could land after a toggle and overwrite the fresh value with stale data. The checkbox then showed the opposite of the engine's actual state until the next invalidation.

## What Changed

**Core logic (4 production files, 3 new)**

- `TaskMaster/Ribbon/RibbonExplorer.xml` — four `onAction` values corrected to the `_Click` spelling; the `BtnMigrateIDs` button element removed whole. Verified reflow-independently as exactly four attribute-value renames plus one element removal, by comparing element and attribute multisets rather than lines.
- `TaskMaster/Ribbon/SpamManagerResetGate.cs` (new) — `internal sealed class` holding the readiness decision. Resolves the auto-file objects, the classifier manager and the engines facade; emits a not-ready notice exactly once and returns a completed task when any is unavailable; otherwise returns the caller's reset invocation directly. Carries no coverage-exemption attribute, deliberately.
- `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` (new) — the versioned cache: an `Interlocked` monotonic ticket source and a `TryApplyState` compare-and-swap that applies a write only when its ticket is strictly newer than the stored one.
- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` — both writers now take a ticket before their activation read and invalidate the control only when their write actually landed; prime completion treats any outcome other than ran-to-completion as a failure, so a cancelled prime clears its in-flight marker and logs rather than silently wedging.
- `TaskMaster/Ribbon/RibbonController.Intelligence.cs` — the Clear Spam Manager call site now defers its engine-touching work through the gate. The synchronization-context preamble and the confirmation dialog are unchanged and in their original order.

**Tests (4 fixtures, 27 new tests)**

- `SpamManagerResetGateTests.cs` (new, 9 tests) — constructor guards, null-reset ordering, three not-ready paths, the success path, and fault propagation.
- `EngineToggleStateCoordinatorTests.Race.cs` (new, 6 tests) — prime-after-toggle, toggle-versus-toggle, an uncontended control against over-suppression, the invalid-operation guard, and two cancelled-prime cases.
- `EngineTogglePressedStateCacheTests.cs` (new, 10 tests) — ticket monotonicity and the compare-and-apply accept/reject matrix.
- `RibbonExplorerXmlTests.cs` (+2 tests) — every callback name in the document resolves to a public viewer method, and check-box action callbacks take the ribbon-control interface followed by a bool.

**Project files** — three compile-item registrations. Both projects are legacy non-SDK and enumerate every source file explicitly.

## Architecture / How It Fits Together

Office polls `getPressed` synchronously, so the coordinator must answer from a cache and can never await on that path. The cache is therefore the correctness boundary, and freshness is defined by when a read *began*, not by when its write lands:

```
click ──► ExecuteToggleAsync ──► toggle engine ──► ticket ──► read activation ──┐
                                                                                ├──► TryApplyState ──► invalidate iff applied
paint ──► getPressed (miss) ──► ApplyPrimeAsync ──► ticket ──► read activation ─┘

getPressed (hit) ──► TryGetActive ──► bool          (no await, no block, no throw)
```

`TryApplyState` stores only when the incoming ticket is strictly newer than the stored one. The cached entry is a reference type on purpose: `ConcurrentDictionary.TryUpdate` compares the comparand by reference identity for a type with no equality override, which is the compare-and-swap semantic required. A value tuple would degrade the comparison to structural equality, so an unrelated writer holding an equal value would satisfy the comparand check and the guard would silently weaken to "the value looked the same".

`SpamManagerResetGate` follows the constructor-guard shape of the existing `EngineReadinessGate` and the deferred-invocation shape of `EngineGatedCommandRunner`, so the ribbon now has one consistent readiness idiom rather than three.

## Verification

**Completed**

| Gate | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier check .` | exit 0, 1576 files, none unformatted |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0 |
| Nullable | `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` | exit 0 |
| Tests + coverage | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | exit 0, 6982 run, 6982 passed, 0 failed |

The analyzer and nullable rebuilds were re-run after the final `main` merge introduced `Directory.Build.props`, and both still exit 0.

Red-before-green is recorded for the two findings where it is possible. Finding 1: both new tests fail against the pre-fix tree (0 of 2 passed). Finding 3: exactly the three nominated reproductions fail pre-fix, with the other three passing by design as controls. Finding 2 carries a schema-valid fail-before exception dossier instead, because the defective statements show a message box, install a WinForms synchronization context, and reach disk-backed classifier creation, so no deterministic unit test can execute them.

Coverage, recomputed per file from the two committed Cobertura documents:

| File | Line coverage |
|---|---|
| `SpamManagerResetGate.cs` | 100% (new-module rule requires 90%) |
| `EngineTogglePressedStateCache.cs` | 94.87% |
| `EngineToggleStateCoordinator.cs` | 98.52% → 100% |
| Lines this change added to the coordinator | 18 / 18 |
| Repository-wide | 85.41% line, 79.50% branch |

Two independent feature-review passes were run over the branch. Both returned **zero blocking findings**.

**Recommended**

- Re-run the four gates above on a clean checkout.
- The one manual step this change cannot automate is under Follow-ups.

## Backward Compatibility / Migration Notes

- No public API changes. Both new types are `internal`.
- One user-visible removal: the `BtnMigrateIDs` button no longer appears in the Explorer ribbon. It previously raised a callback-not-found condition on every click, so no working behaviour is lost.
- The four repaired check boxes begin functioning for the first time. Users who assumed those settings were inert may see behaviour they have not seen before; this is the intended repair.
- No database, settings-schema, or serialization changes. Rollback is a straight revert.

## Risks and Mitigations

- **The concurrency fix is the highest-risk element.** Mitigated by six deterministic race tests driven by held completion sources rather than timing, and by the pre-existing update-before-invalidate ordering test continuing to pass unmodified.
- **Conditional invalidation could in principle suppress a needed repaint.** A rejected write means a newer writer already stored its value and already invalidated. An explicit uncontended test guards against over-suppression.
- **`SpamManagerResetGate` cannot be exercised end to end without a live Outlook host.** The decision logic it extracts is fully unit-tested; only the residual wiring inside the coverage-exempt controller method is unverified, and no coverage credit is claimed for it.

## Review Guide

Suggested order:

1. `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` — the compare-and-apply loop is the core of the change; the class remarks explain why the cached entry must be a reference type.
2. `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` — confirm ticket capture sits before each activation read and that invalidation is inside the conditional.
3. `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` — the interleavings under test.
4. `TaskMaster/Ribbon/SpamManagerResetGate.cs` and its call site in `RibbonController.Intelligence.cs`.
5. `TaskMaster/Ribbon/RibbonExplorer.xml` — a nine-line diff.

Mechanical and safe to skim: the three compile-item registrations, and the one-word `partial` addition to the existing coordinator fixture.

**Large artifacts.** The feature folder carries two Cobertura documents of roughly 194,000 lines each, about 21.6 MB combined. A squash merge is recommended. Note that the automated PR-context classifier reports "Core logic changes: 0 files" for this branch: its changed-file list is ordered by churn and is saturated by those two documents, which pushes all twelve source files out of the window. The real source footprint is 12 files, +1383 / -34.

## Follow-ups

- **Manual verification outstanding.** One acceptance criterion (24 of 25 are satisfied) requires a live Outlook host: confirm that clicking Clear Spam Manager before initialization completes now shows the not-ready notice instead of raising a `NullReferenceException`, and that the reset still runs end to end afterwards. Recorded as `OPERATOR-ACTION-REQUIRED` rather than asserted.
- **Latent race in prime completion**, identified during review and pre-existing: `CompletePrime` removes the in-flight marker outside the prime lock, so if the prime never suspends, the continuation can remove the marker before the registration assignment lands. Not introduced here. Worth its own issue.
- **`BuildNotReadyMessage` calls `string.Format` with a constant format string and no arguments.** Inert today, but a latent `FormatException` if a brace is ever added to the message. Left as-is deliberately: both the call shape and the `System.Globalization` using were fixed by the approved plan, and changing them would require re-running the full toolchain to re-verify an inert defect.
- Three defects deferred by the spec remain open and are tracked separately: the eight unguarded QuickFiler-settings globals sites, the orphaned folder-classifier handler, and three bound handlers that raise `NotImplementedException`.

## GitHub Auto-close

- Closes #735

Note on scope: the PR-context bundle listed several other tokens under author-asserted autoclose, including `#CR-2`, `#CR-3` and `#SHA-256`, which are not issue numbers at all. That list is a text scrape of the feature documents produced while the GitHub CLI was unavailable to the collector, not a verified set. Issue 735 is the only closing target, and it was confirmed open independently before this body was written.
