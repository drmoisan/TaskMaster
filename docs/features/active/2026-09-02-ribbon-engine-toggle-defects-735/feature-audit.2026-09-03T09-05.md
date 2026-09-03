# Feature Audit — ribbon-engine-toggle-defects (#735)

- **Timestamp:** 2026-09-03T09-05
- **Branch:** `bug/ribbon-engine-toggle-defects-735`
- **HEAD:** `30e66833e73267327a18e58228f493e8c8e3a4dd`
- **Work mode:** `full-bug` (marker `- Work Mode: full-bug` in `issue.md`)
- **AC source:** `spec.md` **only**. `user-story.md` is correctly absent for `full-bug`; `issue.md` is not an AC source this cycle.
- **Baseline:** `origin/main @ b13d5b7b` (equivalently merge base `a679cd08` for the item's four implementation commits)

**Result: 24 of 25 acceptance criteria PASS. 1 accepted, disclosed gap (F2-AC8, operator action required). 0 FAIL. 0 UNVERIFIED.**

---

## Check-Off Integrity

Two integrity questions were asked of the AC source before evaluating the criteria themselves.

**Was any criterion text altered rather than merely checked off?** No. `git diff 044551f0..3e45428e -- spec.md`
returns a diff consisting *exclusively* of `- [ ]` -> `- [x]` transitions. Every criterion string is
byte-identical on both sides of the diff. No criterion was weakened, softened, deleted or added to
fit the delivery. This is the strongest single signal in the audit.

**Was any criterion checked off ahead of its evidence?** No. Check-offs are distributed across the
four implementation commits and each lands in the same commit as its supporting artifact:

| Commit | Time | ACs flipped | Evidence committed alongside |
|---|---|---|---|
| `a3bfb865` | 01:31 | F1-AC1..AC7 (7) | `evidence/regression-testing/p1-t2/p1-t2.trx` |
| `88fc3bfc` | 01:39 | Finding 2 ACs | gate fixture TRX `p2-t8` |
| `a68c8598` | 01:47 | Finding 3 ACs | `p3-t5` (fail-before), `p3-t11`, `p3-t12` |
| `3e45428e` | 02:04 | F2-AC6, F2-AC7, X-AC1..AC4 | `coverage-final...cobertura.xml`, `p4-t3.trx` |

The two coverage-dependent criteria (F2-AC6 gate coverage, F2-AC7 no-new-exemption) and all four
cross-cutting criteria were flipped only in the final commit, which is the commit carrying the
coverage artifact. Nothing was checked off speculatively.

---

## Finding 1 — Dead XML-to-handler bindings

| # | Criterion (abridged) | Verdict | Evidence |
|---|---|---|---|
| F1-AC1 | 5 unresolved callback names before; 0 after | **PASS** | `p1-t2.trx` failure message: "these **5 of 84** bound names do not: BtnMigrateIDs_Click, MoveEntireConversation_Clicked, SaveAttachments_Clicked, SaveEmailCopy_Clicked, SavePictures_Clicked". Post-fix: the same test passes in `p4-t3.trx` (134/134). |
| F1-AC2 | Exactly 4 `_Clicked` -> `_Click` renames; no viewer method added/renamed/removed | **PASS** | XML diff shows exactly 4 `onAction` value changes on `MoveEntireConversationDefault`, `SaveAttachmentsDefault`, `SaveEmailCopyDefault`, `SavePicturesDefault`. `RibbonViewer.cs` is absent from the diff (verified by name-only filter). |
| F1-AC3 | Exactly 1 element deleted (`BtnMigrateIDs`); nothing else changed | **PASS** | XML diff is 9 lines: one `<button id="BtnMigrateIDs" .../>` removal plus the four rename pairs. No other attribute or element touched; no CSharpier reflow beyond the edits. |
| F1-AC4 | Partition is 4 renames + 1 removal = the 5 defective names | **PASS** | The five names in the `p1-t2.trx` message map exactly onto the four renames and the one deletion. |
| F1-AC5 | `..._EveryCallbackNameResolvesToAPublicRibbonViewerMethod` exists and passes; element nodes only; includes root `onLoad`; predicate as specified | **PASS** | `RibbonExplorerXmlTests.cs:+53..+95`. Uses `document.Descendants()` (element nodes only, so `XComment` occurrences are excluded structurally); `XDocument.Descendants()` includes the root `customUI`; `IsCallbackAttribute` implements the stated `onAction`/`onChange`/`onLoad`/`StartsWith("get")` rule. Passes in `p4-t3.trx`. |
| F1-AC6 | `..._CheckBoxOnActionCallbacksTakeControlAndPressedParameters` exists and passes; first parameter compared by full type name | **PASS** | Same file. `HasCheckBoxActionShape` asserts `void` return, arity 2, `parameters[0].ParameterType.FullName == "Microsoft.Office.Core.IRibbonControl"`, `parameters[1] == typeof(bool)`. Passes in `p4-t3.trx`. |
| F1-AC7 | Both new tests fail pre-fix, reporting 5 and 4 respectively | **PASS** | `p1-t2.trx`: both `outcome="Failed"`; messages report "**5** of 84" and "these **4** are not". Exact match to the criterion's stated counts. |

---

## Finding 2 — Unguarded globals dereference in Clear Spam Manager

| # | Criterion (abridged) | Verdict | Evidence |
|---|---|---|---|
| F2-AC1 | `internal sealed class SpamManagerResetGate` in namespace `TaskMaster`; 3 dependencies; `ArgumentNullException` naming each | **PASS** | `SpamManagerResetGate.cs:47` declaration; `:72-84` constructor with three `?? throw new ArgumentNullException(nameof(x))` guards. Three constructor tests pass in `p2-t8.trx`. |
| F2-AC2 | `RunAsync` null-reset throws before any accessor; notifies once and returns completed task when manager or engines is null; otherwise returns the reset invocation directly with no await/catch | **PASS** | `:104-122`. The `reset is null` guard at `:106-109` precedes the first accessor call at `:111`. `:115-119` notifies once and returns `Task.CompletedTask`. `:121` is `return reset(manager, engines);` — no `await`, and the type contains no `catch`. Confirmed by `RunAsync_WithNullReset_ThrowsArgumentNullExceptionBeforeProbingAccessors` and `RunAsync_WhenResetFaults_PropagatesUnchangedAndDoesNotNotify`, both passing. |
| F2-AC3 | No `ExcludeFromCodeCoverage`, no `Microsoft.Office` using, no `System.Windows.Forms` using, no logger field; XML doc records the omission as deliberate | **PASS** | Usings at `:1-4` are exactly `System`, `System.Globalization`, `System.Threading.Tasks`, `UtilitiesCS`. No attribute on the type. XML doc `:40-45`: "This type is deliberately NOT marked `[ExcludeFromCodeCoverage]`...". No logger field among the three readonly fields at `:49-51`. |
| F2-AC4 | `ClearSpamManagerAsync` retains preamble and dialog unchanged and in order; routes only engine-touching statements through the deferred lambda; no inline ad-hoc null guard | **PASS** | `RibbonController.Intelligence.cs` diff: the `SynchronizationContext.Current is null` preamble and the `MessageBox` confirmation are untouched context lines in their original order. The confirmation is inverted to an early return; the four engine-touching statements move verbatim into the lambda with `manager`/`engines` substituted. No `?.` or `if (x == null)` guard was added inside the method. |
| F2-AC5 | All nine gate tests pass, in the stated composition | **PASS** | `p2-t8.trx`: 9 tests, all `outcome="Passed"`. Names map one-to-one onto the criterion: 3 constructor null cases, `RunAsync_WithNullReset_ThrowsArgumentNullExceptionBeforeProbingAccessors`, 3 not-ready cases (`WhenAutoFileAccessorReturnsNull`, `WhenManagerIsNull`, `WhenEnginesAccessorReturnsNull`), `WhenAllDependenciesAvailable_InvokesResetWithResolvedManagerAndEngines`, `WhenResetFaults_PropagatesUnchangedAndDoesNotNotify`. `grep -c "[TestMethod]"` on the fixture returns 9. |
| F2-AC6 | Gate class line coverage >= 90% | **PASS** | `coverage-final...cobertura.xml:193415` — `<class line-rate="1" branch-rate="1" ... name="TaskMaster.SpamManagerResetGate">`. **100% line, 100% branch.** |
| F2-AC7 | No new `ExcludeFromCodeCoverage` anywhere; none widened; residual lines inside the pre-existing exemption; no coverage credit claimed | **PASS** | Diff grep for added lines matching `\[ExcludeFromCodeCoverage` returns only two XML-doc prose lines (both "deliberately NOT marked"). `RibbonController.cs:36` carries the attribute and is verified pre-existing at the merge base. `grep -n "RibbonController"` on the final Cobertura returns **no match** — the type is absent from measurement entirely, which positively confirms that no coverage credit is claimed for the residual lines. |
| F2-AC8 | Change description records the manual live-Outlook verification | **OPERATOR-ACTION-REQUIRED** (unchecked) | Left `- [ ]` deliberately. `evidence/other/manual-verification-clear-spam-manager.2026-09-02T12-04.md` documents the procedure and records that it could not be executed: the executor has no live Outlook host. **Treated as an accepted, disclosed gap, not a failure.** The criterion is genuinely un-automatable — it requires clicking Clear Spam Manager in a running Outlook during the pre-initialization window. |

---

## Finding 3 — Toggle-state last-writer race

| # | Criterion (abridged) | Verdict | Evidence |
|---|---|---|---|
| F3-AC1 | Concurrent dictionary of a private nested reference type carrying flag + monotonic ticket, keyed ordinally; sequence via interlocked only | **PASS** | `EngineTogglePressedStateCache.cs:46-47` — `ConcurrentDictionary<string, PressedState>(StringComparer.Ordinal)`. `:142-155` — `private sealed class PressedState` with `Active` and `Sequence`. `:40` `private long _stateSequence`, mutated only at `:57` via `Interlocked.Increment`. No other read or write of the field exists in the file. |
| F3-AC2 | Both writers capture a ticket immediately before the activation read (toggle path: after the engine toggle completes), apply through compare-and-apply, invalidate only when applied | **PASS** | Toggle: `EngineToggleStateCoordinator.cs:224-234` — `await ToggleEngineAsync` -> `NextSequence()` -> `await EngineActiveAsync` -> `if (TryApplyState(...)) _invalidateControl(...)`. Prime: `:318-323` — `NextSequence()` on the line immediately preceding `EngineActiveAsync`, same conditional invalidation. Both match plan tasks P3-T7 (`plan:261`) and P3-T8 (`plan:263`) exactly. |
| F3-AC3 | Reader keeps `bool` return; never awaits, blocks or throws; existing ordering test passes unmodified | **PASS** | `GetPressed` at `:136-150` returns `bool` and performs a catalog lookup plus a dictionary read (`TryGetActive`, itself a single `TryGetValue`). No `await`, no lock, no throw on any path. The existing fixture is unmodified apart from the `partial` keyword, and all its tests pass in `p4-t3.trx`. |
| F3-AC4 | Prime completion treats any non-ran-to-completion outcome as failure: marker cleared, failure logged, cancellation synthesized; faulted path preserved | **PASS** | `:334-352`. Guard is now `completed.Status == TaskStatus.RanToCompletion`. On any other status the marker is removed and `_logError` is called with `completed.Exception?.GetBaseException() ?? new TaskCanceledException(completed)`. The faulted path still passes the unwrapped base exception, preserving the existing reference-identity assertion. |
| F3-AC5 | All six race tests pass; the three defect-reproducing tests fail pre-fix | **PASS** | Pre-fix `p3-t5.trx`: exactly 3 Failed / 3 Passed. The three failures are precisely the prime-after-toggle race, the toggle-versus-toggle race and the canceled-prime logging case, with assertion messages naming the defect semantics rather than any compile or harness error. Post-fix: all six pass within `p4-t3.trx` (134/134). This reviewer predicted the 3-of-6 split independently from the pre-fix source before opening the TRX; all six predictions matched. |
| F3-AC6 | Existing fixture changes by exactly one added `partial` keyword | **PASS** | The diff for `EngineToggleStateCoordinatorTests.cs` is a single line: `public class` -> `public partial class`. Line count is unchanged at 459 on both sides. |

---

## Cross-cutting

| # | Criterion (abridged) | Verdict | Evidence |
|---|---|---|---|
| X-AC1 | New source files registered as compile items in both legacy non-SDK project files; solution builds | **PASS** | `TaskMaster.csproj` +2 (`EngineTogglePressedStateCache.cs`, `SpamManagerResetGate.cs`); `TaskMaster.Test.csproj` +3 (`SpamManagerResetGateTests.cs`, `EngineToggleStateCoordinatorTests.Race.cs`, `EngineTogglePressedStateCacheTests.cs`). Five entries for five new files. *Note:* the criterion says "three new source files" because it was authored before the P4-T3 size contingency added two more; all **five** are in fact registered, so the criterion is met and exceeded. Build confirmed by the passing analyzer and nullable rebuilds and by 6982 executed tests. |
| X-AC2 | Every changed file under 500 lines after formatting; counts verified for the four named files | **PASS** | Independently measured with `git grep -c ""` at the item tip: coordinator 415, Intelligence partial 444, XML fixture 496, race file 277. All new files: gate 141, cache 157, gate tests 326, cache tests 213. Maximum is 496 (see NB-4 in the code review — 4 lines of headroom). |
| X-AC3 | Full toolchain passes in order in a single pass, no failures and no auto-fixes | **PASS** | `evidence/qa-gates/toolchain-loop-closure...md` plus the per-gate files record one clean pass: CSharpier format -> check -> analyzer rebuild -> nullable rebuild -> vstest with coverage. Final run `EXIT_CODE: 0`, `Total tests: 6982`, `Passed: 6982`. Analyzer/nullable at the baseline 5 warnings, 0 errors. The caller re-ran both builds against the post-`Directory.Build.props` merged tree with exit 0 and 0 warnings; that re-run is recorded as caller-attested rather than independently verified, as this reviewer has no build tools. |
| X-AC4 | No behaviour outside the three findings changes; the 8 QuickFiler-settings members, the orphaned folder-classifier handler and the 3 not-implemented handlers left untouched | **PASS** | The `RibbonController.Intelligence.cs` diff contains exactly two hunks: the gate field/property addition at `@@ -203,6 +203,31 @@` and the `ClearSpamManagerAsync` body at `@@ -214,22 +239,29 @@`. `TestSpamVerbose` appears only as unchanged trailing context. `RibbonViewer.cs` is absent from the diff entirely. No QuickFiler-settings member, no `BuildFolderClassifier_Click`, and none of the three `NotImplementedException` handlers was modified. |

---

## Test Population Reconciliation

Performed independently because the executor's own reconciliation is defective (code review NB-3).

| Fixture | New tests | Source of count |
|---|---|---|
| `RibbonExplorerXmlTests.cs` | 2 | Both new methods in the `#735` region |
| `SpamManagerResetGateTests.cs` | 9 | `grep -c "[TestMethod]"` = 9 |
| `EngineToggleStateCoordinatorTests.Race.cs` | 6 | `grep -c` and the 6 results in `p3-t5.trx` |
| `EngineTogglePressedStateCacheTests.cs` | **10** | `grep -c "[TestMethod]"` = **10** |
| **Total** | **27** | |

This reconciles exactly against both recorded populations:

- Full suite: `6955` (P0-T9 baseline) + 27 = **6982** (final run). Matches.
- Ribbon subset: `107` (P0-T8 baseline TRX, verified by `grep -c 'testName="'`) + 27 = **134** (P4-T3 counters). Matches.

No test was removed and none was skipped. The executor's evidence file counts the cache fixture as 9
and then attributes the leftover test to a baseline filter artifact; that attribution is false — the
named test is present in the P0-T8 baseline TRX. See code review NB-3. **The delivery is unaffected;
only the narrative is wrong.**

---

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md
- Total AC items: 25
- Checked off (delivered): 24
- Remaining (unchecked): 1
- Items remaining:
  - "The change description records the manual verification: the not-ready notice is observed
     instead of a NullReferenceException when Clear Spam Manager is confirmed before initialization
     completes, and the reset still runs end to end when repeated after initialization completes."
     (F2-AC8 — OPERATOR-ACTION-REQUIRED; requires a live Outlook host)
```

No criterion was newly checked off by this review. All 24 already-checked criteria were
independently re-verified against artifacts and source and are confirmed correctly checked. The one
remaining criterion is correctly left unchecked.

---

## Disposition

**Recommend GO.** Zero Blocking findings. The three defects in issue #735 are fixed, each with
regression coverage that was demonstrated to fail against the pre-fix tree where the defect was
behavioural. Coverage clears every applicable floor. No prohibited path was touched, no coverage
exemption was introduced or widened, and no acceptance criterion was weakened to fit the delivery.

Two items are owed outside this branch and should not block it:

1. **F2-AC8** — a maintainer with a live Outlook host must perform the manual verification and record the outcome in the PR description.
2. **NB-3** — correct the test count in `evidence/qa-gates/vstest-coverage-run.2026-09-02T12-04.md` from 9 to 10 for the cache fixture and remove the false filter-mismatch paragraph. Documentation-only.

Three follow-ups the spec already commits to promoting separately remain outstanding and are
correctly out of scope here: the eight QuickFiler-settings unguarded-globals sites (referencing
#524), the orphaned `BuildFolderClassifier_Click` handler, and the three `NotImplementedException`
bound handlers (`TestSpamVerbose`, `SpamMetrics`, `SpamInvestigateErrors`).
