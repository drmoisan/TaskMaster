# Feature Audit — ribbon-engine-toggle-defects (Issue #735)

- Timestamp: 2026-09-03T06-19 (UTC)
- Branch: `bug/ribbon-engine-toggle-defects-735`
- Head: `30e66833e73267327a18e58228f493e8c8e3a4dd`
- Baseline: `b13d5b7b1a6dd0aa79d51d48a7156ee67377f9d0` (`origin/main` tip and `git merge-base HEAD origin/main`)
- Diff anchor: `git -C <worktree> diff b13d5b7b HEAD`

## Work Mode and AC source resolution

`issue.md` carries `- Work Mode: full-bug`. Per `acceptance-criteria-tracking`, the authoritative
acceptance-criteria source for `full-bug` is **`spec.md` only**. `user-story.md` does not exist for
this item, which is correct for the mode. `issue.md`'s "Proposed Fix / Validation Ideas" checkboxes
are not acceptance criteria under this mode and were not treated as such.

`spec.md` carries 25 acceptance criteria under `## Acceptance Criteria`, in four groups.

## Acceptance Criteria evaluation

### Finding 1 — dead XML-to-handler bindings

| # | Criterion (abridged) | Verdict | Evidence |
|---|---|---|---|
| F1-AC1 | Five unresolved callback names before, zero after | **PASS** | Independently reproduced by this review: 84 distinct callback names / 5 unresolved at `b13d5b7b`; 83 / 0 at `HEAD`. The five are `BtnMigrateIDs_Click`, `MoveEntireConversation_Clicked`, `SaveAttachments_Clicked`, `SaveEmailCopy_Clicked`, `SavePictures_Clicked`. |
| F1-AC2 | Exactly four `_Clicked` -> `_Click` renames on the four Item Sort Settings check boxes; no viewer method added, renamed or removed | **PASS** | XML diff shows exactly four `onAction` value changes at `RibbonExplorer.xml:267, 273, 279, 285`. `RibbonViewer.cs` is not in the branch diff at all, so no method changed. |
| F1-AC3 | Exactly one element deleted: `BtnMigrateIDs`; no other element, attribute or value changed | **PASS** | XML diff is one deleted `<button id="BtnMigrateIDs" .../>` plus the four renames. No other line differs. |
| F1-AC4 | Partition is four renames plus one removal, totalling five | **PASS** | The four renamed names each resolve to an existing correctly signatured `..._Click(Office.IRibbonControl, bool)` method at `RibbonViewer.cs:180, 186, 192, 198`. `BtnMigrateIDs_Click` has no implementation anywhere in the solution, so removal is the correct disposition. |
| F1-AC5 | `RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod` exists and passes; element nodes only; includes the root load callback; correct attribute rule | **PASS** | `RibbonExplorerXmlTests.cs:371-410`. Uses `document.Descendants()`, which yields element nodes only, so `XComment` occurrences are excluded structurally; for an `XDocument` the root `customUI` element is included, so its `onLoad` is covered. Attribute rule matches the criterion exactly (`IsCallbackAttribute`, lines 347-355). Passes in `p1-t7.trx` and `p4-t3.trx`. |
| F1-AC6 | `RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters` exists and passes; asserts `void (IRibbonControl, bool)` comparing the first parameter by full type name | **PASS** | `RibbonExplorerXmlTests.cs:413-461` with the shape predicate at `:468-480`. Compares `parameters[0].ParameterType.FullName` against the `RibbonControlTypeName` constant, exactly as the criterion requires and for the stated reason (no Office interop reference in the test project). |
| F1-AC7 | Both new tests demonstrated to fail pre-fix — the first on five names, the second on four | **PASS** | `evidence/regression-testing/p1-t2/p1-t2.trx`: `total="2" executed="2" passed="0" failed="2"`, recorded as an intentional `[expect-fail]` run with `ExpectedExitCode: 1`. The build preceding it exited 0, so both are genuine assertion failures rather than compile errors. |

### Finding 2 — unguarded globals dereference in Clear Spam Manager

| # | Criterion (abridged) | Verdict | Evidence |
|---|---|---|---|
| F2-AC1 | `internal sealed class SpamManagerResetGate` in namespace `TaskMaster`, three dependencies, `ArgumentNullException` naming each | **PASS** | `SpamManagerResetGate.cs:47` declaration; constructor at `:72-88` throws `ArgumentNullException(nameof(...))` for each of the three. Tests 1-3 in the gate fixture assert the parameter name via `WithParameterName`. |
| F2-AC2 | `RunAsync` null-reset check before any accessor; exactly one notification and a completed task when manager or engines is null, without invoking reset; otherwise returns the reset invocation directly with no await and no catch | **PASS** | `SpamManagerResetGate.cs:104-125`. The null-reset guard precedes both accessor calls; `RunAsync_WithNullReset_ThrowsArgumentNullExceptionBeforeProbingAccessors` proves this using accessors that throw if invoked. The fault-propagation limb is proven by `RunAsync_WhenResetFaults_PropagatesUnchangedAndDoesNotNotify` asserting `BeSameAs(failure)`. |
| F2-AC3 | Gate carries no `ExcludeFromCodeCoverage`, no `Microsoft.Office` using, no `System.Windows.Forms` using, no logger field; XML doc records the deliberate absence | **PASS** | Independently verified: the file's only occurrence of the attribute name is the prose sentence at `:40` recording that it is deliberately NOT applied. Its `using` block is `System`, `System.Globalization`, `System.Threading.Tasks`, `UtilitiesCS` only. No logger field. |
| F2-AC4 | `ClearSpamManagerAsync` retains the synchronization-context preamble and confirmation dialog unchanged and in order; routes only engine-touching statements through the gate lambda; no inline ad-hoc null guard | **PASS** | `RibbonController.Intelligence.cs:231-264`. The preamble and `MessageBox.Show` block are byte-identical in the diff. The branch was converted from `if (== Yes) { body }` to `if (!= Yes) return;` plus the gated call, which is semantically identical. No `?.` or `if (x == null)` guard was introduced in the method. |
| F2-AC5 | All nine gate tests pass, with the stated composition | **PASS** | `evidence/regression-testing/p2-t8/p2-t8.trx`: `total="9" executed="9" passed="9" failed="0"`. Composition verified from source: 3 constructor cases, 1 strict-accessor null-reset case, 3 not-ready cases, 1 identity-passthrough success case, 1 faulting-reset case. |
| F2-AC6 | Line coverage for the new gate class at least 90% | **PASS** | Recomputed from `coverage-final...cobertura.xml`: `TaskMaster.SpamManagerResetGate` 33/33 lines = **100%**, 14/14 branches = **100%**. |
| F2-AC7 | No new `ExcludeFromCodeCoverage` attribute anywhere in the diff; no existing exemption widened | **PASS** | Independently reproduced: `git diff b13d5b7b HEAD \| grep "^[+-].*ExcludeFromCodeCoverage"` returns only XML-doc prose lines (`+    /// This type is deliberately NOT marked ...`) and evidence-document text. Zero lines match the attribute form `^[+-]\s*\[ExcludeFromCodeCoverage\]`. `git show b13d5b7b:TaskMaster/Ribbon/RibbonController.cs` has the attribute at line 36, identical to `HEAD`. |
| F2-AC8 | The change description records the manual verification (not-ready notice instead of NRE pre-initialization; reset still runs end to end after) | **OPEN — OPERATOR ACTION REQUIRED** | `evidence/other/manual-verification-clear-spam-manager.2026-09-02T12-04.md` records `ManualVerificationStatus: OPERATOR-ACTION-REQUIRED` and leaves both observation fields explicitly unfilled, with the reason: no live Outlook host, and the unit-test policy independently forbids starting a message pump or an external process. The full two-step procedure is written out for the operator. Correctly left unchecked in `spec.md`. |

### Finding 3 — toggle-state last-writer race

| # | Criterion (abridged) | Verdict | Evidence |
|---|---|---|---|
| F3-AC1 | Cache is a concurrent dictionary of a private nested reference type carrying an activation flag and a monotonic ticket, keyed ordinally; sequence source touched only through interlocked operations | **PASS** | `EngineTogglePressedStateCache.cs:46-47` (`ConcurrentDictionary<string, PressedState>` with `StringComparer.Ordinal`), `:142-155` (private sealed reference type with `Active` and `Sequence`), `:57` (`Interlocked.Increment(ref _stateSequence)` is the only access to the field). |
| F3-AC2 | Both writers capture a ticket immediately before the activation read — on the toggle path after the toggle completes — and store through compare-and-apply, invalidating only when applied | **PASS** | `EngineToggleStateCoordinator.cs:222-234` (toggle: `await ToggleEngineAsync` then `NextSequence()` then `await EngineActiveAsync` then conditional invalidate) and `:310-334` (prime: `NextSequence()` then read then conditional invalidate). `TryApplyState` at `EngineTogglePressedStateCache.cs:98-136`. |
| F3-AC3 | Reader keeps `bool` return, never awaits, blocks or throws; existing update-before-invalidate ordering test passes unmodified | **PASS** | `GetPressed` at `:136-147` is a catalog lookup plus `TryGetActive`, which is a dictionary read only. `EngineToggleStateCoordinatorTests.cs` diff contains exactly one changed line (the `partial` keyword), so the ordering test is textually unmodified, and `p3-t12.trx` shows 24/24 passing. |
| F3-AC4 | Prime completion treats any non-ran-to-completion outcome as failure: marker cleared, failure logged, cancellation synthesized when there is no exception; faulted-path base-exception unwrapping by reference preserved | **PASS** | `EngineToggleStateCoordinator.cs:341-353`. Status test replaces the exception test; `new TaskCanceledException(completed)` supplies the synthesized exception. The faulted limb still calls `GetBaseException()`, so the pre-existing reference assertion at `EngineToggleStateCoordinatorTests.cs:233` (`BeSameAs(failure)`) continues to hold — confirmed by 24/24 and 134/134 green runs. |
| F3-AC5 | All six new race tests pass, and the three defect reproductions fail against the pre-fix tree | **PASS** | `evidence/regression-testing/p3-t5/p3-t5.trx` shows exactly three failures, and their names are exactly the three nominated reproductions: `ApplyPrimeAsync_WhenPrimeResolvesAfterToggle_DoesNotOverwriteToggleResult`, `ExecuteToggleAsync_WhenOlderObservationCompletesLast_DoesNotOverwriteNewerResult`, `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker`. The other three correctly passed pre-fix because they pin behavior that already held. `p3-t11.trx` shows 6/6 passing post-fix. |
| F3-AC6 | Existing coordinator test class changes by exactly one added `partial` keyword, no other edit | **PASS** | The diff for `EngineToggleStateCoordinatorTests.cs` is a single hunk changing `public class` to `public partial class`. Nothing else. |

### Cross-cutting

| # | Criterion (abridged) | Verdict | Evidence |
|---|---|---|---|
| CC-AC1 | All new source files registered as compile items in their legacy non-SDK project files, and the solution builds | **PASS with deviation** | `TaskMaster.csproj` gains `Ribbon\EngineTogglePressedStateCache.cs` and `Ribbon\SpamManagerResetGate.cs`; `TaskMaster.Test.csproj` gains `Ribbon\SpamManagerResetGateTests.cs`, `Ribbon\EngineToggleStateCoordinatorTests.Race.cs`, `Ribbon\EngineTogglePressedStateCacheTests.cs`. Both rebuild gates exit 0. Deviation: the criterion says "three new source files"; five were delivered, because the branch-B contingency added two. All five are registered, so the substance is satisfied and the wording is stale. See policy audit NB-6. |
| CC-AC2 | Every file created or modified is under the 500-line ceiling after formatting; counts verified for the coordinator source, the Intelligence partial, the XML fixture and the race file | **PASS** | Recounted with `awk 'END{print NR}'`: coordinator 415, Intelligence partial 444, XML fixture 496, race file 277. Also gate 141, cache 157, gate tests 326, cache tests 213, existing coordinator fixture 459. Maximum is 496. |
| CC-AC3 | Full toolchain passes in order in a single pass with no failures and no auto-fixes: format, analyzers, nullable, tests with coverage | **PASS** | `evidence/qa-gates/toolchain-loop-closure` reconciles ten steps, all exit 0, in the required order. `csharpier check .` exit 0 across 1576 files. Both `/t:Rebuild` gates: 5 warnings / 0 errors, equal to baseline. Test run 6982/6982. The mid-loop branch-B extraction was followed by the re-run of the format and line-count steps that branch B mandates, so every later gate observed the final tree. |
| CC-AC4 | No behavior outside the three findings changes; the eight QuickFiler-settings members, the orphaned folder-classifier handler and the three not-implemented bound handlers left untouched and recorded as follow-ups | **PASS** | `RibbonViewer.cs`, `RibbonViewer.EngineCommands.cs`, `RibbonViewerEngineCallbackShapeTests.cs`, `TaskMaster/AppGlobals/AppOlObjects.cs` and `TaskMaster/AppGlobals/NonBlockingDelay.cs` are all absent from the branch diff. The follow-ups are recorded in `spec.md` `## Rollout & Follow-up`. |

## Behavioral verification against the issue's three reported defects

| Issue finding | Reported behavior | Delivered behavior | Verified how |
|---|---|---|---|
| 1 (source #504) | Five CustomUI controls silently do nothing when clicked | Four now bind to their existing `..._Click(IRibbonControl, bool)` handlers; the fifth control, which never had an implementation, is removed | Independent parse of both XML revisions against the reflected viewer method set; two new tests pin it going forward |
| 2 (source #524) | Unhandled `NullReferenceException` from a UI event handler when Clear Spam Manager is used before initialization completes | The three null states now produce exactly one explanatory notice and the reset is not invoked; the reset itself still runs unchanged when both dependencies resolve | Nine gate tests covering all three null states plus identity passthrough and fault propagation; live-host confirmation still owed under F2-AC8 |
| 3 (source #525) | A prime completing after a user toggle silently overwrites the toggle's result with stale data | An observation whose read began earlier can no longer overwrite a newer one; the control is invalidated only when a write is applied | Two failing-then-passing race reproductions, plus an over-suppression guard test; the ordering argument was re-derived independently in the code-review artifact |

Additionally delivered and in scope per the spec:

- **CR-2** — a canceled prime is now treated as a failure: the marker is cleared and a synthesized
  `TaskCanceledException` is logged. Previously a cancellation was silently ignored and permanently
  blocked re-priming for that key. Two tests, one of which failed pre-fix.
- **CR-3** — the previously untested `InvalidOperationException` guard on the direct toggle path is
  now covered, with a `Times.Never()` verification that no engine call precedes the throw.

## Regressions

None identified. The full first-party suite is 6982/6982 passing, up 27 from a baseline of 6955 with
no test removed or skipped. Analyzer and nullable warning counts are unchanged at 5/0 against
baseline, and the five are the pre-existing System.Reactive `packages.config` advisory. Repository
line coverage moved from 85.39% to 85.41% and branch coverage from 79.46% to 79.50%.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md (Work Mode: full-bug)
- Total AC items: 25
- Checked off (delivered): 24
- Remaining (unchecked): 1
- Items remaining:
  - F2-AC8: "The change description records the manual verification: the not-ready notice is
    observed instead of a NullReferenceException when Clear Spam Manager is confirmed before
    initialization completes, and the reset still runs end to end when repeated after
    initialization completes."
    Status: OPERATOR-ACTION-REQUIRED. Requires a live Outlook host. Procedure is written out in
    evidence/other/manual-verification-clear-spam-manager.2026-09-02T12-04.md.
```

No criterion was checked off by this review. All 24 checked items were already checked by the
executor and were verified as correctly checked; the one unchecked item was verified as correctly
left unchecked.

## Verdict

**PASS.** All 25 acceptance criteria are either satisfied (24) or correctly reported as open pending
an operator action that no code change can satisfy (1). Zero blocking findings. Thirteen non-blocking
observations are recorded in the policy audit and the code review; none of them is a defect in the
delivered behavior, and none requires a remediation cycle.

Recommended gating condition before issue #735 is closed: an operator performs the two-step Outlook
procedure for F2-AC8 and records the observations.
