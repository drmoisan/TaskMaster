# Feature Audit — ribbon-engine-readiness-guard (Issue #503)

- Audit timestamp: 2026-08-08T15-40
- Cycle: **re-audit following remediation cycle 1**

## Scope and Baseline

- Base branch: `main`
- Merge-base: `003c5715055d7d1933db68a742531332756e30b2` (recomputed in-session with `git merge-base HEAD origin/main`, not taken on trust from the caller)
- Feature branch: `bug/ribbon-engine-readiness-guard-503`
- Head: `85ff0ee4f0579a3622f2da3a21a6e942b3e4cd12`
- Work mode marker in `issue.md`: `- Work Mode: full-bug`
- Resolved AC source: **`spec.md` only** (per `.claude/skills/acceptance-criteria-tracking/SKILL.md`; `full-bug` excludes `user-story.md`, and no `user-story.md` exists for this issue)
- Baseline comparison: full branch diff versus the merge-base, comprising 13 `.cs`, 1 `.xml`, 2 `.csproj`, and 107 documentation/evidence files

Cycle 1 changed exactly one source file (`TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`, +12/-3) and altered no acceptance criterion's state. Every criterion below was nonetheless re-evaluated against the current tree rather than carried forward from the prior audit.

## Acceptance Criteria Inventory

`spec.md` `## Acceptance Criteria` defines **30** criteria, AC1 through AC30, in markdown checkbox form.

| Group | Criteria | Requirement traceability |
|---|---|---|
| Readiness signal | AC1–AC4 | R1 |
| Ribbon XML wiring and callback binding | AC5–AC9 | R2 |
| Click guards and notification | AC10–AC14 | R3, A1 |
| Protected-path preservation | AC15, AC16 | R4, R6, A2 |
| Post-initialization refresh | AC17, AC18 | R5c, A4 |
| Live-Outlook verification (**MANUAL-ONLY**) | AC19–AC21 | R5b, R5c, A2, A3, A4 |
| Toolchain, coverage, file size | AC22–AC25 | R5a |
| Structural guarantees | AC26–AC28 | architecture and determinism |
| Process | AC29, AC30 | scope discipline and documentation |

Three criteria (AC19, AC20, AC21) are explicitly designated MANUAL-ONLY by the spec, which states they "must **never** be checked off on the strength of unit tests".

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC1 | PASS | `EngineReadinessGate.cs` exists, is `internal sealed` (line 30), carries no `[ExcludeFromCodeCoverage]` attribute (only a doc comment stating it is deliberately absent), imports only `System` and `UtilitiesCS`, and implements all four readiness conditions at lines 78-101 |
| AC2 | PASS | Named tests cover every listed case: null accessor, null `InboxEngines`, empty dictionary, missing key, null value, and a `[DataTestMethod]` over null/empty/whitespace. `IsEngineReady_IsOrdinalCaseSensitive` asserts `"spam"` is not `"Spam"` |
| AC3 | PASS | `IsEngineReady_AfterDictionaryPopulated_ReturnsTrue` mutates the same `ConcurrentDictionary` between two calls; no `Thread.Sleep` or `Task.Delay` present. Source confirms nothing is cached — every query re-invokes the accessor |
| AC4 | PASS | `Constructor_WithNullAccessor_ThrowsArgumentNullException`, asserting `WithParameterName("enginesAccessor")`; source line 47-48 |
| AC5 | PASS | All eight `<button>` elements carry `getEnabled="EngineCommand_GetEnabled"`, verified directly in the XML diff. **The verifying assertion is now non-vacuous** — this is the F1 remediation, independently re-verified (see below) |
| AC6 | PASS | `RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls` asserts set equality against the catalog. The XML diff confirms `menu id="OtherSpamActions"`, `menu id="OtherTriageActions"`, and every `group`/`tab` are unmodified |
| AC7 | PASS | `RibbonExplorerXml_EngineBackedControlsAreSchemaLegalForGetEnabled` asserts each catalog id resolves to a `button` |
| AC8 | PASS | `RibbonExplorerXml_GetEnabledCallbackMatchesOfficeSignatureOnRibbonViewer` pins public, instance, `bool` return, single `Microsoft.Office.Core.IRibbonControl` parameter. Source: `RibbonViewer.EngineCommands.cs:38-39` |
| AC9 | PASS | `EngineCommandCatalog.Map` contains exactly eight entries — `Spam` ×3, `Triage` ×5 — built with `StringComparer.Ordinal`; `TryGetEngineName` returns `false` for null/empty; `ControlIds` is a `ReadOnlyCollection` over the dictionary keys, so duplicate-free by construction |
| AC10 | PASS | Source inspection of all eight handlers in `RibbonViewer.EngineCommands.cs` confirms every engine dereference sits inside a `Func<Task>` lambda passed to `RunEngineCommandAsync`. `RunAsync_WhenEngineNotReady_DoesNotThrowNullReferenceException` asserts `invoked` is false |
| AC11 | PASS | Both exception types covered by separate named tests: `..._DoesNotThrowNullReferenceException` and `..._DoesNotThrowKeyNotFoundException`, the latter reproducing the dictionary-indexer shape |
| AC12 | PASS | `RunAsync_WhenEngineNotReady_EmitsExactlyOneNotificationContainingControlIdAndEngineName` asserts `ContainSingle()` and that the message contains both `"TriageSetA"` and `"Triage"`. No `Form`, `MessageBox`, or message pump in the test |
| AC13 | PASS | `RunAsync_WithNullAction_ThrowsArgumentNullException` (also asserting no notification is emitted, proving the precondition precedes the gate query) and `RunAsync_WithUnknownControlId_DoesNotInvokeAction` |
| AC14 | PASS | `RunAsync_WhenActionThrows_PropagatesException`. Grep over all added `.cs` lines for `catch` returns only XML-doc prose — zero new catch clauses anywhere in the diff |
| AC15 | PASS | `git diff --numstat <merge-base>..<head>` over `AppItemEngines.cs` and `IAppItemEngines.cs` returns **empty output**. Verified independently in this session. `ApplicationGlobals.cs` likewise zero-line |
| AC16 | PASS | Diffed all eight handlers against the merge-base: every relocated expression is character-for-character identical inside the lambda. `RunAsync_WhenEngineReady_InvokesActionExactlyOnce` and `RunAsync_WhenEngineReady_AwaitsActionToCompletion` (driven by a synchronously completed `TaskCompletionSource`) |
| AC17 | PASS | `InvalidateAll_InvokesDelegateOnceForEachEngineBackedControlId` uses `BeEquivalentTo` (set equality, not sequence) plus a count assertion; `InvalidateAll_WithNullDelegate_ThrowsArgumentNullException` |
| AC18 | PASS | `RibbonViewer.EngineCommands.cs:63-81` returns early when `_ribbon` is null and marshals through `UiThread.Dispatcher` with a `CheckAccess()` guard. `ThisAddIn.cs:77-82` invokes `RefreshEngineCommands()` exactly once, immediately after `await _globals.LoadAsync(false)`, with a why-comment |
| **AC19** | **UNVERIFIED (MANUAL-ONLY, by design)** | Requires a live Outlook process and mail profile. Checklist present at `evidence/manual-verification/ac19-ac21-checklist.2026-08-08T15-00.md`, `Status: PENDING MAINTAINER EXECUTION`. Correctly left unchecked |
| **AC20** | **UNVERIFIED (MANUAL-ONLY, by design)** | As AC19 |
| **AC21** | **UNVERIFIED (MANUAL-ONLY, by design)** | As AC19. Office's callback-caching behaviour is internal to the host and not locally observable |
| AC22 | PASS | Format gate **re-executed in this session**: `csharpier check .` exit 0 over 1498 files. Analyzer build exit 0 (warnings match the merge-base baseline), nullable build exit 0, 6338/6338 tests passed with 0 failed and 0 skipped, all five steps in one uninterrupted pass with a fingerprint proof of no intervening source change. Restart history disclosed rather than omitted |
| AC23 | PASS | All four new types at **100%** line coverage against a 90% floor. Independently corroborated: the `TaskMaster` package gained exactly 186 valid lines (= 48+48+72+18) and covered all 186, with `missed` byte-identical at 1464 |
| AC24 | PASS | Merge-base baseline captured under `evidence/baseline/` before implementation; comparison recorded. Line rate 85.8477% → 85.8561%, branch 79.2370% → 79.2702% — both **up**. No changed line lost coverage. Absolute repo-wide figure recorded and reported as required |
| AC25 | PASS (with recorded exception) | Every changed `.cs` file measured under 500 lines; `RibbonViewer.cs` split from 487 to 388. `RibbonExplorer.xml` at 539 is the pre-existing 519-line exception the criterion itself accepts and declines to remediate; the +20 growth was independently verified as formatter-mandated |
| AC26 | PASS | Grep over the four new decision files for `[ExcludeFromCodeCoverage]` returns only XML-doc prose stating the attribute is deliberately absent — zero actual attributes |
| AC27 | PASS | Grep confirms zero real `Microsoft.Office.*` references in the four decision types (all matches are doc prose). The single new Office-typed member is `EngineCommand_GetEnabled` on the pre-existing `RibbonViewer`. No new `[ComVisible(true)]` attribute added |
| AC28 | PASS | Grep over the five new/changed test files for temp-file APIs, `Thread.Sleep`, `Task.Delay`, wall-clock reads, `Form`, `MessageBox`, and message-pump entry points returns zero matches |
| AC29 | PASS | Six entries under `docs/features/potential/promoted/` covering the research §9 defects (orphan callbacks, `getPressed` signatures, fire-and-forget `ToggleEngineAsync`, non-null-safe `RibbonController.Engines`) plus two found during execution. None fixed inside #503 |
| AC30 | PASS | `spec.md` carries `## Delivery Notes and Deviations` with four disclosed deviations and a `## Remediation Cycle 1` section; `issue.md` carries `## Delivered Outcome`; the manual checklist is present with outcomes pending |

### F1 re-verification (AC5's supporting assertion)

The prior review found AC5's verifying test vacuous: `...Attribute("getEnabled")?.Value.Should().Be(...)` short-circuits the entire chain including `.Should()` when the attribute is absent, so the test passed silently on exactly the regression it names. The criterion was substantively satisfied by the XML itself, but its stated verification method was not sound.

Current source binds the attribute first and asserts `NotBeNull` before dereferencing (`RibbonExplorerXmlTests.cs:201-214`). The remediation was proven, not asserted: the mutation was applied to the embedded resource inside the built assembly with the attribute count verified at 7 in the DLL before any test ran, a green control run against the unmutated resource is recorded, the test was recorded **Failed** with a verbatim message naming `NotBeNull` at line 202, and the mutation was restored with the test recorded **Passed**. The permanent tree retains no part of the mutation, confirmed independently via `git hash-object`.

AC5 is now backed by a genuinely non-vacuous assertion.

### F2 disposition (bears on AC25)

F2 asked that the three `TriageSet*` buttons be restored to single-line form while retaining `getEnabled`, targeting 527 lines. The executor reported this as not remediable. That claim was **independently reproduced in this session** rather than accepted: CSharpier 1.3.0 formats XML, `.csharpierignore` contains no `*.xml` exclusion, no `.csharpierrc` exists so the print width is 100, the collapsed form measures 116 characters, and running `csharpier check` against a probe document containing that exact collapsed element reports it unformatted while leaving a 78-character sibling untouched. AC25's accepted-exception clause therefore stands unchanged, and F2 is correctly closed as escalated rather than fixed.

## Summary

**27 of 30 acceptance criteria PASS.** Zero criteria are FAIL or PARTIAL. Three (AC19, AC20, AC21) are UNVERIFIED by design because they require a live Outlook profile that cannot exist in this environment, and the spec forbids checking them off on the strength of unit tests.

Remediation is **not** triggered. The SKILL's remediation conditions require a meaningful FAIL or PARTIAL result, a failing toolchain gate, a code-review blocker, an acceptance criterion at FAIL or PARTIAL, a coverage threshold breach, or a missing coverage artifact. None applies: every automated criterion passes, all four toolchain gates pass, the code review records no blocker, coverage clears every applicable floor, and the canonical coverage artifact is present and current for HEAD.

AC19–AC21 are deliberately excluded from that trigger. They are not unmet work awaiting a plan; they are a maintainer verification step that no automated executor could perform, since the repository has no Outlook UI-automation harness and the unit-test policy prohibits tests depending on external processes. Routing them to a remediation planner would generate a plan nothing could execute. They are instead recorded as a pre-merge gate, which is exactly how `spec.md` Rollout already frames them.

**Recommendation: GO for PR**, conditional on the maintainer executing `evidence/manual-verification/ac19-ac21-checklist.2026-08-08T15-00.md` against a live Outlook profile before merge.

## Acceptance Criteria Check-off

No check-off changes were made to `spec.md` by this review.

- All 27 automated criteria evaluated PASS were **already** checked `[x]` by the implementation cycle. No previously-unchecked criterion became eligible.
- AC19, AC20, and AC21 remain `[ ]` and **must not** be checked by this reviewer. The spec states they must never be checked off on the strength of unit tests, and no live-Outlook evidence exists. Marking them would be a false attestation.

The reviewer's check-off obligation under `acceptance-criteria-tracking` is therefore satisfied with zero edits to the AC source file.

### Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/spec.md
- Total AC items: 30
- Checked off (delivered): 27
- Remaining (unchecked): 3
- Items remaining:
  - AC19 (R5b, A3) — MANUAL-ONLY: live-Outlook click of the eight commands during initialization produces no NullReferenceException or KeyNotFoundException and shows the "still loading" indication
  - AC20 (R5b, A2, A3) — MANUAL-ONLY: after InitAsync() completes, each of the eight commands behaves exactly as before this change
  - AC21 (R5c, A4) — MANUAL-ONLY: Office visually greys the eight buttons during initialization and re-enables them after the post-InitAsync invalidation, without an add-in restart
```
