# Feature Audit — Issue #871, QfcQueue enqueue-path injectable seams

- Date: 2026-09-13
- Reviewer: feature-review agent
- Work mode: `full-bug` (marker at `issue.md` line 12)
- Acceptance-criteria source: `spec.md`, section `## Acceptance Criteria`, AC1 through AC22. No other document contributes acceptance criteria under this work mode.

## Scope and Baseline

Baseline for the comparison is the anchor the caller supplied and the item recorded,
`8213826f695439e86e3ed34faa575de493a11ec7`. Head is `8277b0c4c`. The PR base branch is `main`.

The anchored branch diff carries 89 paths: nine code and project paths, three feature documents, 74
evidence artifacts and three tracked files under `.claude/agent-memory/orchestrator/`. The nine code and
project paths are three production files added (`QuickFiler/Controllers/QfcQueue.Tlp.cs`,
`QuickFiler/Controllers/QfcQueue.UiIdle.cs`, `QuickFiler/Interfaces/IUiIdleDispatcher.cs`), two
production files modified (`QuickFiler/Controllers/QfcQueue.cs`,
`QuickFiler/Controllers/QfcQueue.Enqueue.cs`), two test files added
(`QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs`,
`QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs`) and both project manifests modified.
1271 insertions against 264 deletions.

Baseline measurements carried forward from the item's Phase 0 records: 1395 tests;
`QfcQueue.Enqueue.cs` line rate 0.152941 over 13 of 85; pre-split `QfcQueue.cs` line rate 0.496795 over
155 of 312; QuickFiler package line rate 0.810521 over 10,215 of 12,603; `QfcQueue.cs` 507 physical
lines, already seven over the 500-line ceiling before any seam was added.

Method. Every criterion below was evaluated against the file contents in the item worktree read
directly, with the committed evidence artifacts used as a second source. Where the two could disagree I
state which I relied on. The Bash tool was withheld for this review by binding caller directive, so no
git command was run; the diff facts are the caller-supplied anchored listing, cross-checked path for
path against the committed listing in `evidence/qa-gates/p6-t6-scope-lock.2026-09-12T10-25.md`, which
agrees with it exactly.

## Acceptance Criteria Inventory

| Source | Section | Count |
|---|---|---|
| `spec.md` | `## Acceptance Criteria` | 22 |
| `user-story.md` | not an AC source under `full-bug` | 0 |
| `issue.md` | not an AC source under `full-bug` | 0 |

Total acceptance criteria in scope: 22 (AC1 through AC22), at spec lines 541, 550, 567, 574, 582, 589,
597, 605, 619, 630, 634, 638, 643, 647, 653, 658, 664, 671, 682, 694, 704 and 714.

## Acceptance Criteria Evaluation

| AC | Verdict | What was required | Evidence verified | Notes |
|---|---|---|---|---|
| AC1 | PASS | `internal IEmailMoveMonitor MoveMonitor` over the retained `_moveMonitor` field, null-rejecting setter, field and its per-owner comment surviving verbatim, six existing reflection tests still passing | `QfcQueue.cs:42` field and comment intact; `QfcQueue.cs:56-60` property with `?? throw new ArgumentNullException`; `MoveMonitor_SeamContract_...` at `QfcQueueEnqueueTests.cs:29`; six `SetPrivateField(..., "_moveMonitor", ...)` call sites survive, three in `QfcQueueCoverageExpansionTests.cs` (119, 145, 207) and three in `QfcQueuePurePathsTests.cs` (126, 176, 244) | The field is not renamed and not converted to an auto-property, which is the property those six tests depend on. I counted the six sites myself. |
| AC2 | PASS | Interface with exactly three members, adapter reproducing the three bodies argument for argument at `ContextIdle`, double await and `Task.Yield` retained, no `DispatcherPriority.Normal` in the Write Set, three forwards present, named call sites unedited | `IUiIdleDispatcher.cs:16-34` declares exactly three members; `QfcQueue.UiIdle.cs:77-106` holds the three bodies with `ContextIdle` at 81, 89 and 102 and the double await plus `await Task.Yield()` at 95-100; forwards at 53-60; a search for `DispatcherPriority.Normal` across `QuickFiler` returns one hit, in `QfcItemController.Conversation.cs:205`, which is outside the Write Set and unmodified | I ran the `DispatcherPriority.Normal` search myself rather than relying on the artifact. The call-site line numbers cited in AC2 are anchor-relative; they resolve correctly against the anchor. |
| AC3 | PASS | `internal Func<CancellationToken, ItemViewer> ItemViewerFactory` defaulted to the `ItemViewerQueue.Dequeue` method group, `AddAsync` calling it, one test asserting the default's `Method.Name` and `Method.DeclaringType` without invoking, one test asserting the substitute receives the queue's token | `QfcQueue.Tlp.cs:64` and `:76-80`; `AddAsync` calls `ItemViewerFactory(_token)` at `:147`; `ItemViewerFactory_Default_IsTheViewerQueueDequeueMethodGroup` at `QfcQueueEnqueueTests.cs:113-121`; `AddAsync_WithSubstitutedViewerSeams_...:386` asserts the captured token equals `_tokenSource.Token` | The inspect-never-invoke discipline is correct: invoking the default would reach the process-wide dispatcher. |
| AC4 | PASS | `internal Action<TableLayoutPanel, ItemViewer, int> ViewerRowPlacer` as a lazy `??=` defaulted to the `AddViewerToTlp` method group, called inside the same wrapper, tests for non-null default and for the exact argument tuple | `QfcQueue.Tlp.cs:94-98` lazy getter; `:149` `await UiIdleCallAsync(() => ViewerRowPlacer(tlp, viewer, indexNumber));`; `ViewerRowPlacer_SeamContract_...` at `QfcQueueEnqueueTests.cs:62`; `_rowPlacements[0]` asserted on all three components at `:387-390` | The lazy form is forced: `AddViewerToTlp` is an instance method and a field initializer cannot reference the instance. |
| AC5 | PASS | `internal Func<TableLayoutPanel, MailItem, int, Task<QfcItemGroup>> ItemGroupFactory` as a lazy `??=` defaulted to `AddAsync`, the loader routing through it, tests for non-null default and for the `items[i - start]` mapping at non-zero start | `QfcQueue.Tlp.cs:114-118`; `QfcQueue.Enqueue.cs:176` `await ItemGroupFactory(tlp, items[i - start], i)`; `ItemGroupFactory_SeamContract_...` at `:71`; `LoadControllersViewersAsync_WithNonZeroStart_MapsIndexAndWidensDigits` at `:357-369` asserts the recorded index is 9 and the mail item is `items[0]` | The AC cites the call site as line 177, which is the anchor line; it is 176 after the formatter re-wrapped the statement. |
| AC6 | PASS | `internal Func<TableLayoutPanel, TableLayoutPanel> BackgroundTlpFactory` defaulted to `template => template.Clone(name: "BackgroundTableLayout")`, the enqueue call site routing through it, one test proving the returned panel is the same reference that reaches the dequeued entry | `QfcQueue.Tlp.cs:120-121` and `:132-136`; `QfcQueue.Enqueue.cs:97` `var tlp = await UiIdleCallAsync(() => BackgroundTlpFactory(_tlpTemplate));`; `EnqueueAsync_WithSubstitutedTemplate_FlowsThatPanelToTheDequeuedEntry` at `:415-423` asserts `BeSameAs(_alternateTlp)` | The named argument is reproduced exactly. This test is the strongest anti-tautology evidence in the suite; one robustness weakness in it is recorded as a Low code-review finding. |
| AC7 | PASS | All six setters reject null with `ArgumentNullException`, all six getters return non-null on a fresh queue, one named test per seam, plus a headless-construction test proving no static WPF dispatcher read and no Outlook COM call | Six `?? throw new ArgumentNullException(nameof(value))` setters at `QfcQueue.cs:59`, `QfcQueue.UiIdle.cs:50`, `QfcQueue.Tlp.cs:79`, `:97`, `:117`, `:135`; six seam-contract tests at `QfcQueueEnqueueTests.cs:29, 38, 50, 62, 71, 83`; `Construction_InHeadlessHost_SucceedsAndDefaultsToProductionAdapter` at `:98-105` | The no-throw assertion is a real discriminator, not a vacuous one: `UiThread.Dispatcher` throws `InvalidOperationException` until `UiThread.Init()` runs on an STA thread, and `UiThread.Init` is called nowhere in `QuickFiler.Test`. I verified both facts directly. |
| AC8 | PASS | Every production and test file in the Write Set strictly under 500 physical lines, measured after the final format | `evidence/qa-gates/p5-t6-line-counts-final.2026-09-12T10-25.md`: 269, 200, 329, 108, 35, 425, 343. Corroborated by reading each of the seven files | The tightest is the new test-class part at 425. `QfcQueue.cs` was 507 before the change, which is why the split was a precondition. |
| AC9 | PASS | A `<Compile Include>` item for each of the three new production files and the two new test files, verified positively rather than by absence | `QuickFiler/QuickFiler.csproj` lines 350, 351 and 371; `QuickFiler.Test/QuickFiler.Test.csproj` lines 121 and 122; analyzer build exit 0 with zero diagnostics; the suite references `UiThreadIdleDispatcher` from the UiIdle part, `IUiIdleDispatcher` from the interface file and five seam members declared in the Tlp part, so a missing manifest entry would fail the compile | I read the five manifest entries directly rather than accepting the artifact's summary. |
| AC10 | PASS | Named tests proving `ArgumentNullException` for a null item list and `ArgumentException` for an empty one, using FluentAssertions async throw assertions | `QfcQueueEnqueueTests.cs:125-133` and `:137-145`, against the guards at `QfcQueue.Enqueue.cs:79-86` | The empty-list assertion uses the assignable rather than the exact form and would also accept `ArgumentNullException`; recorded as a Low code-review finding, not an AC failure, because the criterion's wording is satisfied. |
| AC11 | PASS | One page enqueued with all seams substituted, asserting count 1, the dequeued tuple carrying the substituted panel, and the item groups in input order | `EnqueueAsync_WithOnePage_QueuesTheTemplatePanelAndGroupsInInputOrder` at `:152-163`, asserting `Count == 1`, `entry.Tlp.Should().BeSameAs(_sentinelTlp)` and `Select(g => g.MailItem).Should().Equal(items)` | `Equal` is order-sensitive, so the input-order clause is genuinely pinned. |
| AC12 | PASS | Two tests making `ItemGroupFactory` throw `OperationCanceledException` and `InvalidOperationException`, with no propagation, queue count 0 and the counter back at 0 | `:187-190` and `:197-200` delegating to `AssertLoaderFailureIsContainedAsync` at `Harness.cs:270-283`, which asserts `NotThrowAsync`, one recorded factory call, `Count == 0` and `JobsRunning == 0` | The throw is raised from inside the `try`, which is what makes the `finally` the mechanism under test. |
| AC13 | PASS | A test capturing the running-jobs count from inside a seam callback, and the count asserted 0 after each of the three outcomes | `EnqueueAsync_WhenItRuns_...` at `:170-180` asserts mid-flight 1 through `_itemGroupObserver` and 0 after the success outcome; the two AC12 tests assert 0 after each failure outcome | The success outcome's post-count assertion lives in the counter test rather than in the AC11 test, which still satisfies "after each of the three outcomes". |
| AC14 | PASS | One test asserting exactly one `Add` notification, and a second running the same flow with no subscriber and asserting no exception | `:204-215` asserts `ContainSingle()` and `NotifyCollectionChangedAction.Add`; `:219-229` asserts `NotThrowAsync` and `Count == 1` | Both arms of the null-conditional invocation at `QfcQueue.Enqueue.cs:131` are therefore exercised. The AC cites that line as 133, which is its anchor position. |
| AC15 | PASS | A `MockBehavior.Strict` move monitor through S1, `HookItem` verified once per item with the item and a non-null `Action<MailItem>`, the captured delegate not invoked | `EnqueueAsync_WithStrictMoveMonitor_HooksEachItemExactlyOnce` at `:237-256`: per-item `Times.Once` verification plus `captured.Should().HaveCount(2).And.NotContainNulls()`; no invocation of the captured delegates appears anywhere in either file | The strict behaviour is meaningful here: the test never calls `Dequeue`, so no un-set-up `UnhookItem` call can occur. |
| AC16 | PASS | Both arms of the digit ternary at totals 9, 10 and 11; carrier-found and carrier-absent; the nine-argument pass-through with every argument captured and asserted; one awaited `InitializeAsync` per row verified `Times.Once` | `[DataTestMethod]` rows at `:262-277`; `:284-298` and `:302-310`; `:317-335`; `:339-350` | Literally satisfied. Two of the nine argument assertions compare null against null and have no discriminating power against an argument-order defect; that is recorded as a Low code-review finding rather than an AC shortfall, because the criterion asks for capture and assertion, both of which are present. |
| AC17 | PASS | A test leaving `ItemGroupFactory` at its default, substituting only S3 and S4, calling `AddAsync` directly, asserting the returned group carries the supplied mail item, that the viewer factory received the queue's token and that the placer received the exact tuple | `AddAsync_WithSubstitutedViewerSeams_BuildsTheGroupAndPlacesTheViewer` at `:377-391` using `NewProductionItemGroupQueue()` at `Harness.cs:127-138`, which does not assign `ItemGroupFactory` | This is the criterion that prevents a coarse seam from relocating the uncovered region instead of closing it, and the coverage measurement confirms it worked: `AddAsync`'s body lines report hits. |
| AC18 | PASS | Verbatim relocation apart from named substitutions; no `#nullable` added to relocated code; the `CS0618` pair intact; regions balanced per file; the two catch blocks, the `logger.Error` call and its message unchanged; no public member added, removed, retyped or re-signed | `evidence/qa-gates/p6-t5-diff-review.2026-09-12T10-25.md` verdicts 1 through 8, all PASS, with the underlying mechanical classification in `p6-t2-new-code-coverage...md`; corroborated by reading the four files: exactly one `#nullable` across the Write Set and it is in the brand-new interface file; `#pragma warning disable CS0618` at `QfcQueue.Enqueue.cs:171` and `restore` at `:196`; region and endregion counts equal in all four files | Two byte-level deviations are recorded rather than glossed: a CSharpier re-wrap of one statement, captured before and after, and the loss of a UTF-8 byte-order mark from `QfcQueue.cs`. Both are output of the formatter command CLAUDE.md mandates, and CLAUDE.md states the formatter wins where a diff disagrees with it. Neither changes behaviour and both rebuild gates are clean afterwards. |
| AC19 | PASS | New code at or above 90 percent, repository-wide at or above 80 percent, the enqueue part strictly above 0.152941, the combined three parts not below the recorded base-part baseline, verified by a pre-change and a post-change Cobertura artifact named in the completion report | New code 0.958333 over 23 of 24; repository-wide projected 0.857898; enqueue part 1 over 85 of 85; combined 0.540299 over 181 of 335, above both the spec literal 0.503205 and the measurement 0.496795. `evidence/qa-gates/p6-t1-coverage-file-rates...md`, `p6-t2-new-code-coverage...md`, `p6-t4-repo-wide-projection...md` | PASS with two disclosed substitutions. First, the two raw Cobertura documents were deleted after their last consumer under the ratified issue-671 evidence-hygiene convention, which forbids committing them; the figures survive in five committed Markdown projections and the substitution is disclosed in the plan, in P5-T5, in P6-T4 and in P7-T24. Second, the repository-wide figure is a projection over the committed item-825 reference document, because a whole-solution local run is blocked on this host. I independently re-derived the reference pair from that document and read `lines-covered="56029" lines-valid="65402"`, matching the projection's inputs exactly. |
| AC20 | PASS | A committed document naming every remaining uncovered region on the enqueue path with a reason and a citation, addressing at minimum the reflection-driven clone and the template setter, the dead commented-out member, and any zero-hit statement in `AddAsync` or the enqueue part | `evidence/regression-testing/residual-uncovered-regions.2026-09-12T10-25.md`: 27 contiguous zero-hit runs totalling 154 lines across the four production files, each attributed to a member and explained, plus the three clone overloads in the utilities extensions file; the three named minimum topics are sections 1a/1b, 2 and 5a | Unusually complete. It also records the sub-line residual at `QfcQueue.Enqueue.cs:91`, the uninvoked async-void hook lambda, which the deduplicating convention would otherwise have hidden behind a covered enclosing statement. Naming a residual the convention would have concealed is the behaviour this criterion is trying to produce. |
| AC21 | PASS | The counter increment still outside the `try` whose `finally` decrements it, no hunk touching that control flow, and no test codifying the leak as correct | Verified directly in the current tree: increment at `QfcQueue.Enqueue.cs:94`, `try` at 101, `finally` at 126, decrement at 128. The anchored diff of that file is three hunks at 91, 97 and 175, none of which contains those lines (`evidence/qa-gates/p3-t7-out-of-scope-untouched...md`). The only injected exception is `_itemGroupFailure`, consumed solely by `RecordItemGroup`, which the loader calls from inside the `try`; no test makes `BackgroundTlpFactory` or the move-monitor hook throw | Both clauses verified independently of the evidence artifact. The follow-up link is present in the spec's Rollout and Follow-up section at line 776. |
| AC22 | PARTIAL | No change to any file outside the Write Set; in particular the three existing QfcQueue test files, the UtilitiesCS threading types and the UtilitiesCS control-clone extension unmodified, with the full suite passing | Second clause verified and holding: `QfcQueueTests.cs`, `QfcQueueCoverageExpansionTests.cs`, `QfcQueuePurePathsTests.cs`, `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS/Threading/WpfUiDispatcher.cs` and `UtilitiesCS/Extensions/WinFormsExtensions.cs` appear in neither the anchored diff nor the porcelain status, and the assembly passes 1423 of 1423. First clause not holding: the branch diff carries `.claude/agent-memory/orchestrator/MEMORY.md` plus two new memory records beside it, and the spec's Write Set at lines 519-537 lists no agent-memory path | The plan admitted these three paths through a pre-declared scope-lock clause, but a plan clause cannot amend a spec criterion. Un-checked. The shortfall is confined to three tracked Markdown agent-memory records; no production, test, project or third-party file outside the Write Set changed. Remedy is a one-line spec amendment, not a code change. Not Blocking. |

Verdict distribution: 21 PASS, 1 PARTIAL, 0 FAIL, 0 UNVERIFIED.

## Acceptance Criteria Check-off

All 22 criteria were already checked in `spec.md` when this review began.

- Criteria confirmed PASS and left checked: AC1 through AC21 — 21 criteria. No criterion needed to be newly checked by this review.
- Criteria un-checked by this review: AC22, graded PARTIAL. The checkbox at `spec.md` line 714 was changed from `- [x]` to `- [ ]` under the acceptance-criteria-tracking rule that a reviewer leaves a PARTIAL, FAIL or UNVERIFIED criterion unchecked and documents the gap. The criterion text was not modified.
- No criterion text was added, removed or reworded by this review.

### Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/spec.md
- Total AC items: 22
- Checked off (delivered): 21
- Remaining (unchecked): 1
- Items remaining: AC22 — Untouched files stay untouched. Second clause verified; first clause contradicted by three tracked agent-memory records in the branch diff that the Write Set does not list. Remedy is a spec amendment.
```

## Summary

The item delivered what it set out to deliver. The enqueue path, which carried zero test coverage on
its two target members at the anchor, is now reachable from a headless MSTest host through six
`internal` seams that each preserve their previous construction expression as the production default.
`QfcQueue.Enqueue.cs` moved from 13 of 85 covered lines to 85 of 85. The combined rate for the three
parts of the split base file rose from 0.496795 to 0.540299 with zero relocated statements regressing.
Genuinely-new line coverage is 95.83 percent, above the 90 percent floor; the single uncovered
genuinely-new line is the deliberately uninvoked `BackgroundTlpFactory` default, which is recorded as a
residual rather than excluded from measurement. The 500-line ceiling, breached at the anchor by
`QfcQueue.cs` at 507 lines, is now met by all seven files with the tightest at 425. The full toolchain
passes in a single pass with the formatter rewriting nothing, both rebuild gates report zero errors and
zero warnings, and the suite reports 1423 of 1423 at `failed=0` against a baseline of 1395, arithmetic
that confirms no pre-existing case was lost.

Twenty-one criteria are earned on the evidence. AC22 is PARTIAL and has been un-checked: its universal
"nothing outside the Write Set" clause is contradicted by three tracked agent-memory records, while its
substantive clause about the pre-existing test files and the UtilitiesCS types is verified and holds.
The correct response is a one-line amendment to the spec, not a change to any code.

**Nothing found in this audit is Blocking.** Remediation inputs are not produced.
