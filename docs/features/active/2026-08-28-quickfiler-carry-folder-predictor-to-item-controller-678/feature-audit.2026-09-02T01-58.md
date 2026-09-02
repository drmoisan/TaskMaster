# Feature Audit — issue #678, carry the folder predictor to the item controller (closing audit, post remediation cycle 1)

- Timestamp: 2026-09-02T01-58
- Head: `bd57dc9d400ac269317d2397c1ad649deac426de`
- Base: `807fb0bb6e5e49f43efa6b256b05960bf078ca19`
- Work mode: `minor-audit` (marker read from `issue.md:13`)
- AC source: `issue.md`, the `## Acceptance Criteria` section only, AC1 through AC23
- Supersedes: `feature-audit.2026-09-01T23-35.md` (round 1, head `d1f51e3a`)

## AC source resolution

The work-mode marker at `issue.md:13` reads `minor-audit`, so `issue.md` alone is the authoritative
AC source and only its explicit `## Acceptance Criteria` section counts. `spec.md` and
`user-story.md` are absent by design for this mode; their absence is not a finding and no other
checkbox section in `issue.md` — the Logs, Impact, Proposed Fix or Next Step lists — was treated as
an acceptance criterion.

The section spans `issue.md:62-118` and contains exactly **23** checkbox items, AC1 through AC23.
This reviewer counted them directly rather than relying on the register.

**Criterion-text integrity.** `issue.md` at head is byte-identical to the Phase 0 preimage recorded
for this remediation cycle at `evidence/remediation-baseline/issue-ac-preimage.md`. No criterion was
reworded, added, removed or renumbered to accommodate the remediation, and the checkbox state is
unchanged at 22 checked and one unchecked. This matters because two of the round-1 findings (NB-4 and
NB-8) are defects in the criteria text itself, and editing that text would have been the cheapest way
to make them disappear.

## Per-criterion evaluation, AC1 through AC23

| AC | Criterion (abbreviated) | Verdict | Evidence |
|---|---|---|---|
| AC1 | `QfcPreScoredItem` carries an `IFolderSearchHandler` alongside its existing members, which keep their names, types and non-null contracts | **PASS** | `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:148` declares `public IFolderSearchHandler FolderHandler { get; }`. The carried type is the narrow seam, not the concrete `FolderPredictor`. `MailItem` and `PredeterminedFolder` keep their names and types, and the constructor still coerces `PredeterminedFolder` to `string.Empty`, preserving its non-null contract. |
| AC2 | `IFolderScoringService.ScoreAsync` and `FolderScoringService` publish the handler; the exclusion attribute and its justification are retained | **PASS** | `ScoreAsync` returns a three-element tuple whose third element is the initialised handler. `FolderScoringService` retains `[ExcludeFromCodeCoverage]` and its justification comment; this reviewer confirmed zero added and zero removed occurrences of the attribute across the whole three-dot diff. |
| AC3 | The handler reaches the datamodel boundary through the gate's `scoreLoader`, its acceptance projection and `ScoreRemainingQueueMailItemAsync`; every production construction site populates the new member and the set is re-derived and recorded | **PASS** — with a recorded qualification | The forwarding chain is intact and the handler is present on `QfcGateBatch.Accepted` and `QfcDequeueBatch.PreScored`. This reviewer re-derived the production construction sites at head and found **three**, not the two the Phase 0 register records: `QfcHighConfidencePreFilter.cs:90`, `QfcStreamingDequeueConfidenceGate.cs:212`, and `QfcHighConfidencePreFilter.cs:219`. The first two populate the member. The third was added by R1 and deliberately does not: it is the reconciliation fallback for a surviving item that has **no** carrier, where no handler exists to forward. Populating it would mean fabricating a handler for an item that was never scored, which would suppress the item controller's fallback scoring pass — the R1 test explicitly asserts `loaded[0].FolderHandler` is null for exactly that item. The criterion's purpose (no production path silently drops an available handler) is fully met. The Phase 0 register at `evidence/baseline/carrier-construction-sites.md` correctly describes the base ref and is not stale for its own scope; the head-state re-derivation is recorded here instead. |
| AC4 | `RunAsync` obtains carriers from the outcome-returning dequeue and selects the carrier overload in enabled mode, the `IList<MailItem>` overload in disabled mode | **PASS** | `QfcHomeController.cs:299-326`. Enabled mode calls `DequeueNextItemGroupWithOutcomeAsync` and then `_formController.LoadItemsAsync(preScored)`; disabled mode calls `LoadItemsAsync(listEmail)`. Pinned from both directions by `QfcHomeControllerIssue218Tests.cs:198-202` and `:283-287`, which verify the plain `IList<MailItem>` overload is used `Times.Never` in enabled mode. R1 changed what `preScored` holds but not which overload is selected. `RunAsync` measures 39/39 = 100% line coverage. |
| AC5 | `QfcItemGroup` carries the handler; `EncapsulateItemGroup` and the carrier overload of `LoadControlsAndHandlers_01Async` thread it to the `QfcItemController` constructor, which stores it | **PASS** | `QfcItemGroup.cs` gains the carried member; `QfcCollectionController.CarrierLoad.cs` threads it through; `QfcItemController.Initialization.cs:55` and `:116` assign `_carriedFolderHandler`, declared at `QfcItemController.cs:259`. Constructor storage is pinned by `QfcItemController.InitializationTests`. |
| AC6 | `IterateQueueAsync` forwards `batch.PreScored` into `QfcQueue`, which carries the handler through `EnqueueAsync` to the controllers it constructs; any seam is the injectable-delegate form with no new interface | **PASS** | `QfcHomeController.Iteration.cs:35` forwards the carriers; `IQfcQueue.EnqueueAsync` takes them as a required third parameter. The seam is `ItemControllerFactory`, a delegate field with a production default, matching the existing `_folderPredictorFactory` and `ScoringServiceFactory` patterns. No new interface was introduced. The end-to-end composition remains proved in two halves rather than executed, which is recorded as NB-7. |
| AC7 | `LoadFolderHandlerAsync` adopts a carried handler inside the `varList is null` branch only; neither the factory nor `InitAsync` is invoked for a carried item | **PASS** | `QfcItemController.FolderHandling.cs:68-86`. The adoption is inside the `varList is null` branch and returns before the `try` that constructs a predictor. Pinned by `LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory` with a Moq `Times.Never` assertion; confirmed passing in the retained TRX. |
| AC8 | With no carried handler the method behaves exactly as before; the existing un-carried test passes unmodified | **PASS** | The `_carriedFolderHandler is null` path falls through to the unchanged `Task.Run` block at `:88-131`. The existing test is unmodified — this reviewer confirmed the only assertion changed anywhere in the cycle is the R2-authorised one in `QfcItemController.FolderHandlingTests.Part2.cs`. |
| AC9 | The `FromArrayOrString` branches of both members are unchanged and never adopt a carried handler; a negative test proves it | **PASS** | The `else` branch at `:133` onward is untouched, and `LoadFolderHandler` is entirely untouched. `LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory` is the negative test; confirmed passing in the retained TRX. R3's throw is inside the carried branch, so it cannot fire on a `FromArrayOrString` call. |
| AC10 | The carried handler is released in cleanup alongside `_folderHandler` so it does not outlive the row | **PASS** | `QfcItemController.ViewerSetup.cs:466` sets `_carriedFolderHandler = null` with a comment naming the issue. It is a null assignment rather than a dispose, so a handler shared by two rows could not be double-disposed (see NB-11). |
| AC11 | The preselected entry is identical to what the pre-change code preselects, for the predetermined-folder case and the index fallback cases; `FolderArray`, `Suggestions` and `FolderRowArray` come from the carried result with the same values | **PASS** — read as the general rule that AC12 specialises | `AssignFolderComboBox` is unchanged except for the projection call at `:230-234`. The three collections are read from `_folderHandler`, which now holds the carried instance produced by the same `InitAsync(FromField)` sequence, so the values are the same by construction. AC11 and AC12 cannot both hold literally for the archive-rooted case; the delivered code implements AC11 as the general rule and AC12 as the more specific one, which is the only coherent reading. R2 widened the set of inputs AC12 governs without changing that structure. The tension is a defect in the criteria text and is recorded as NB-8. |
| AC12 | The raw-versus-projected mismatch is resolved deliberately and stated in the change description; the carried folder and `FolderArray` use the same normalisation so `FolderContains` matches; a test covers an archive-rooted suggestion and fails against the unnormalised form | **PASS** — strengthened by R2 | `ProjectPredeterminedFolder` at `QfcItemController.FolderHandling.cs:272-286` is now character-identical in body to `FolderPredictor.ProjectSuggestionPath` at `FolderPredictor.cs:845-858`, and its guard corresponds exactly to that member's `_globals is null` guard. `FolderPredictor.cs` is unmodified, so the parity is real rather than arranged. Two tests pin the boundary: the original archive-rooted test (red against the unnormalised form, recorded at `evidence/regression-testing/ac12-path-normalisation.md`) and the R2 test `AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder`. Both pass. `ProjectPredeterminedFolder` measures 11/11 = 100%. |
| AC13 | `FilterAsync` stays dormant, `HighConfidencePreFilterLoader` uninvoked; the `Times.Never` and `preFilterInvoked` assertions are preserved verbatim | **PASS** | Re-verified at head. `QfcHomeControllerRunAsyncHighConfidenceTests.cs` retains `Times.Never` at `:254`, `:295` and `:326` and the `preFilterInvoked` block at `:276-287`; `QfcHomeControllerIssue218Tests.cs` retains `preFilterInvoked` at `:167-176` and `Times.Never` at `:200` and `:285`. The remediation touched neither file's assertions. |
| AC14 | `QfcDequeueStop` handling and the empty-batch early return are unchanged; the carrier overload returns early on the same condition as the `IList<MailItem>` overload (null, not empty) | **PASS** | `QfcFormController.Actions.cs:116-125` returns early on `preScored is null`, not on empty, matching the plain overload. This reviewer specifically re-checked this criterion because R1's `ReconcileCarriersToItems` never returns null and could in principle have suppressed an early return that previously fired. It cannot: `QfcDatamodel.QueueProcessing.cs:197` projects `accepted` into `nodes` and would throw on a null `accepted` before the batch is built, so `PreScored` could never be null on this path either. An empty accepted set produced an empty list before the change and produces one after it. `QfcDequeueStop` handling in `IterateQueueAsync` is untouched. |
| AC15 | The accepted behavioural delta — freezing `CtfMap` suggestions at scan time — is stated in the change description for both legs | **PASS** | `evidence/other/change-description.md` states the delta with a per-leg severity analysis. The remediation did not alter it and did not introduce a second undeclared delta: the three behaviour-changing edits are each analysed under "Did the remediation introduce anything new?" in `code-review.2026-09-02T01-58.md`. |
| AC16 | A new MSTest test asserts the single-initialisation invariant with a Moq `Times` assertion; it fails against the pre-change code and passes after | **PASS** | `LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory` uses `Times.Never` on the predictor-construction seam. RED-first evidence at `evidence/regression-testing/ac16-red.md` records a scoped run at exit 1 with `Total tests: 1, Failed: 1`, the sentinel exception named by type and message, and a preceding exit-0 build ruling out a stale assembly. Green confirmed in the retained TRX. |
| AC17 | The two verifications constraining the carrier overload are rewritten rather than deleted, so they assert the carrier overload is selected; no test is weakened or removed, and every changed test carries a recorded reason | **PASS** | `QfcHomeControllerIssue218Tests.cs:198-202` and `:283-287` are rewritten to `Verify(m => m.LoadItemsAsync(It.IsAny<IList<MailItem>>()), Times.Never, ...)` with reason strings naming issue #678. Reasons are recorded in `evidence/other/test-reconciliation.md`. Round 1 additionally confirmed the rewritten pinning assertion at `QfcHomeControllerRunAsyncHighConfidenceTests.cs:231-256` retains discriminating power rather than being trivially satisfied. The remediation weakened nothing: its single assertion change is the one R2 authorises, and it is a correction of a claim that was false. |
| AC18 | All new and modified tests use MSTest, Moq and FluentAssertions, create no temporary files, and require no live Outlook COM | **PASS** | Verified across all 20 changed test paths including the three tests added by the remediation. `[TestMethod]` throughout, all doubles are `Mock<T>`, all assertions use `.Should()`. Reviewer grep finds no `Path.GetTempFileName`, `Path.GetTempPath` or `File.Create`. `MailItem` is always a Moq double; the R1 test drives the `TryUnhookOrReplace` throw branch entirely through a mocked move monitor. |
| AC19 | The full C# toolchain passes in order on the final pass, each gate with its own evidence artifact recording `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:` | **PASS** | Four gates, all exit 0, in policy order: `csharpier check` (1575 files), analyzer `/t:Rebuild` (5 warnings / 0 errors, `CoreCompile` 57), nullable `/t:Rebuild` (zero `CS86`, `CoreCompile` 72), MSTest with coverage (6949/6949 passed). Each has an artifact under `evidence/qa-gates/remediation-*.md` carrying all four required fields. This reviewer re-ran the format gate at head and reproduced `Checked 1575 files`, exit 0, and corroborated the builds against assembly mtimes of 01:33:18-01:33:24. Full gate table in `policy-audit.2026-09-02T01-58.md`. |
| AC20 | Coverage does not regress on changed lines; every new or modified member reaches at least 90% line coverage; baseline and post-change figures recorded numerically; no exclusion attribute added or removed | **PARTIAL** — remains unchecked | Three of four clauses pass and one fails. **No regression on changed lines**: PASS — the remediation cycle's added executable production lines measure 34/34 = 100.00%, reproduced independently by this reviewer, and repository-wide line and branch rates both rose against the same-session baseline (85.3964 -> 85.3967 and 79.4373 -> 79.4522). **Figures recorded numerically**: PASS — `evidence/qa-gates/remediation-coverage-delta.md` records baseline and post-change values for every attribute. **No exclusion attribute added or removed**: PASS — zero added and zero removed across the diff, confirmed by this reviewer. **Every new or modified member at >= 90%**: FAIL — `QfcQueue.EnqueueAsync` (0/46) and `QfcQueue.LoadControllersViewersAsync` (0/24) remain at zero. Both are host-bound bodies relocated from `QfcQueue.cs`, both were at zero at the base ref, and their uncovered line count is unchanged at exactly 72. The seven members this cycle actually authored or modified are all at or above 90% (100.00, 100.00, 100.00, 100.00, 100.00, 90.62, 94.67), independently reproduced. Dispositioned non-blocking with five recorded grounds in the policy audit; recorded as NB-4. **This criterion stays unchecked in `issue.md`.** |
| AC21 | No source file exceeds 500 lines as a result of the change; additions to files already at or over the limit go into new partial parts | **PASS** | Re-measured at head. No changed file crossed the limit. The remediation added `QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs` at 247 lines rather than extending an existing file, following the pattern the first cycle established. Three files remain over the limit, all pre-existing and all smaller than at the base ref (2446->2336, 827->792, 610->505); `QfcItemController.ViewerSetup.cs` sits at exactly 500, at the cap and not over it. Recorded as NB-6. |
| AC22 | The items the research places out of scope are not changed; any confirmed real defect among them is reported for separate promotion rather than fixed here | **PASS** | None of the six named items was touched: the synchronous `LoadFolderHandler` is entirely unmodified at head; no coverage-exempt class was de-exempted; no oversized file was split; `IFolderSearchHandler` gained no `InitAsync`; the dormant post-display filter still exists; the duplicated `MailItemHelper.FromMailItemAsync` calls are unconsolidated. `evidence/other/out-of-scope-register.md` records the confirmed defects for separate promotion. The remediation respected the same boundary and, where R2 could have been closed by editing `FolderPredictor.cs`, it aligned the caller instead and left the target unmodified. |
| AC23 | The change is confined to `QuickFiler`, `QuickFiler.Test` and this feature folder; no change to `.claude/rules/`, `CLAUDE.md`, any policy document, or anything under `UtilitiesCS` | **PASS** | Re-derived at head from `git diff --numstat 807fb0bb...HEAD`: 122 changed paths, of which 16 are under `QuickFiler/`, 20 under `QuickFiler.Test/` and 86 under this feature folder. Zero paths under any other prefix, confirmed including `UtilitiesCS/`, `.claude/`, `CLAUDE.md` and `artifacts/orchestration/`. |

## Summary of verdicts

| Verdict | Count | Criteria |
|---|---:|---|
| PASS | 22 | AC1-AC19, AC21, AC22, AC23 |
| PARTIAL | 1 | AC20 |
| FAIL | 0 | — |
| UNVERIFIED | 0 | — |

Every criterion was evaluated against the source and the measured artifacts at head. No criterion is
recorded as unverified, and no criterion was evaluated by reading the executor's own claim without
independent confirmation.

## Position on AC20

AC20 is the single criterion that does not fully pass, and it should stay that way.

Its per-member clause is failed by two members, `QfcQueue.EnqueueAsync` and
`QfcQueue.LoadControllersViewersAsync`, both at zero line coverage. The clause is failed on the
literal text: both are "new or modified members" in the sense that they appear as added lines in the
branch diff.

The substance is weaker than the letter. Both members were **relocated**, not written: they were
moved out of `QfcQueue.cs` into a new partial part so that additions would not extend a file already
over the 500-line limit — which AC21 requires. Round 1 verified independently that both were at zero
at the base ref: every `EnqueueAsync` reference in the test project is a Moq setup or verification on
the `IQfcQueue` interface, and `LoadControllersViewersAsync` is private with no reference of any
kind. Neither was reachable, so neither could have been covered. Their bodies are host-bound —
`EnqueueAsync` clones a `TableLayoutPanel` through the UI-idle marshal and hooks an
`EmailMoveMonitor`; `LoadControllersViewersAsync` dequeues a real `ItemViewer` — and covering them
would require a live window, which `.claude/rules/general-unit-test.md` prohibits, or an exclusion
attribute, which AC20's own fourth clause prohibits. The criterion is therefore self-limiting on
these two members: it cannot be satisfied by any means it permits.

This reviewer re-measured and confirms the position did not deteriorate. The file's ratio moved from
28.00% to 15.29%, which looks worse and is not: the uncovered line count is unchanged at exactly 72,
in the same two bodies. The ratio fell only because R1 removed 15 lines that were all covered,
relocating that logic to `QfcHighConfidencePreFilter.cs` where it measures 100%.

**Recommended disposition: leave AC20 unchecked and do not open a further remediation cycle for it.**
The two remaining routes are a maintainer-ratified coverage exemption under the COM/VSTO clause of
`CLAUDE.md`, or a refactor extracting testable logic out of the two host-bound bodies. Both are
larger than this `minor-audit` bug fix and neither is authorised by the criteria in scope. The right
home is the consolidated follow-up issue already planned, alongside NB-6, NB-7 and NB-8.

An unchecked AC20 is the honest record: the criterion is genuinely not fully met, no repository
policy floor is breached, and the gap is documented with reproduced figures rather than dispositioned
into a pass.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/issue.md
- Total AC items: 23
- Checked off (delivered): 22
- Remaining (unchecked): 1
- Items remaining: AC20 (coverage does not regress on changed lines; every new or modified member reaches at least 90% line coverage)
```

No criterion was newly checked off by this review. The 22 already-checked items were each
re-evaluated and each independently confirmed as PASS, so none required a change. AC20 was evaluated
PARTIAL and left unchecked, per the check-off protocol's rule that PARTIAL, FAIL and UNVERIFIED items
are not checked. `issue.md` was not modified by this review.

## Verdict

The delivered change satisfies 22 of 23 acceptance criteria. The single shortfall, AC20, fails one of
its four clauses on two relocated host-bound members that were already at zero coverage before this
branch existed, and it is dispositioned non-blocking against repository policy floors that are all
met and all improved.

Blocking findings: **0**. The remediation cycle's exit gate is satisfied.
