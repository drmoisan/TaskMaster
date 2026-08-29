# Partition C Sweep and Per-Hit Enumeration (P1-T4) — discharges AC-6

- **Issue:** #635
- **Plan task:** [P1-T4]

Timestamp: 2026-08-29T06-28

## Output Summary

The thirteen-identifier sweep over tracked `.cs` files returned 31 hits across 12 files. Every hit is
enumerated individually below with its file, line number, matched identifier, and assigned category.
Two hits are the live preserved member `LoadItemGroupsAndViewers_02` in the affected file, matched only
because the bare stem `LoadItemGroup` is a strict prefix of it. Twenty-eight hits name
`LoadSequentialAsync`, which is the name of three live and unrelated members in the TaskMaster startup
assembly, together with their tests and doc comments. One hit is a triple-slash documentation comment
in the QuickFiler test tree naming `WireUpKeyboardHandler`. The category "genuine name-based caller of
a removed member" is empty.

PARTITION_C_HITS: 31
CAT_A: 2
CAT_B: 28
CAT_C: 1
CAT_G: 0

## Command

Command:

```
git grep -n -I -F -e WireUpKeyboardHandler -e AnyOpenDropDownsAsync -e LoadGroups_02cAsync -e LoadGroups_02bAsync -e LoadGroup_03bAsync -e LoadConversationsAndFoldersAsync -e LoadItemGroup -e LoadSequentialAsync -e LoadGroupSequential -e CacheTlpForMove -e SwapTlp -e CaptureTlpTemplate -e _templateTlp -- "*.cs"
```

EXIT_CODE: 0

Output, verbatim:

```
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs:60:        /// Issue #444 decision pin. Upstream #468 deleted the dead <c>WireUpKeyboardHandler</c>
QuickFiler/Controllers/QfcCollectionController.cs:344:            LoadItemGroupsAndViewers_02(listMailItems, template);
QuickFiler/Controllers/QfcCollectionController.cs:669:        public void LoadItemGroupsAndViewers_02(IList<MailItem> items, RowStyle template)
TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs:322:            // No-op the issue #211 Phase 3.6 live StoreWrapperInitClock read so LoadSequentialAsync
TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:187:        public void LoadSequentialAsync_KeepsComPhasesOnCallerThreadAndYieldsBetweenHeavyPhases()
TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:200:            var methodBody = ExtractMethodBody(source, "public async Task LoadSequentialAsync()");
TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:226:        public void LoadSequentialAsync_YieldsBeforeAutoFilePhase()
TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:238:            var methodBody = ExtractMethodBody(source, "public async Task LoadSequentialAsync()");
TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:253:                    "LoadSequentialAsync should yield after the ToDo phase and before the auto-file phase."
TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:274:        public void LoadSequentialAsync_OffloadsEnginesInitAsyncWithTaskRun()
TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:298:                    "LoadSequentialAsync should explicitly offload engine initialization."
TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:303:        public void LoadSequentialAsync_RunsAutoFileLoadOnCallerThread()
TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:334:        public async Task LoadSequentialAsync_ExecutesRealCoordinatorSequenceThroughPhaseWrappers()
TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:358:            await sut.LoadSequentialAsync();
TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs:14:    /// Drives the real <see cref="ApplicationGlobals.LoadSequentialAsync"/> sequence through a
TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs:27:        public async Task LoadSequentialAsync_InvokesProbeForEachBoundaryInStartupOrder()
TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs:43:        public async Task LoadSequentialAsync_InvokesProbeExactlyOncePerBoundary()
TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs:122:            // No-op the issue #211 Phase 3.6 live StoreWrapperInitClock read so LoadSequentialAsync
TaskMaster.Test/AppGlobals/TestableApplicationGlobals.cs:11:    /// the real <c>LoadSequentialAsync</c> coordinator sequence without a live Outlook/VSTO runtime.
TaskMaster.Test/AppGlobals/TestableApplicationGlobals.cs:95:        // No-op the issue #211 Phase 3.6 live StoreWrapperInitClock read so LoadSequentialAsync
TaskMaster/AppGlobals/AppAutoFileObjects.cs:60:                await LoadSequentialAsync();
TaskMaster/AppGlobals/AppAutoFileObjects.cs:84:        public async Task LoadSequentialAsync()
TaskMaster/AppGlobals/AppToDoObjects.cs:41:                await LoadSequentialAsync();
TaskMaster/AppGlobals/AppToDoObjects.cs:63:        public async Task LoadSequentialAsync()
TaskMaster/AppGlobals/ApplicationGlobals.cs:84:                await LoadSequentialAsync();
TaskMaster/AppGlobals/ApplicationGlobals.cs:144:        public async Task LoadSequentialAsync()
TaskMaster/AppGlobals/StartupDiagnosticsProbe.cs:9:    /// around the <c>Engines</c> phase in <see cref="ApplicationGlobals.LoadSequentialAsync"/>:
TaskMaster/AppGlobals/StartupDiagnosticsProbe.cs:95:        /// double)"/> (which is scoped to <c>LoadSequentialAsync</c>): it runs continuously across
TaskMaster/AppGlobals/StartupDiagnosticsProbe.cs:164:        /// for every phase in <see cref="ApplicationGlobals.LoadSequentialAsync"/>. The deltas and
TaskMaster/ThisAddIn.cs:49:            // ApplicationGlobals.LoadSequentialAsync (which only spans ~3 s of a ~108 s freeze). Each
UtilitiesCS/OutlookObjects/Store/StoreWrapperInitClock.cs:16:    /// attribution in <c>ApplicationGlobals.LoadSequentialAsync</c> sample the cost before/after each
```

## The mechanical category tests, applied in order

- **Category A** — a hit whose path ends Controllers/QfcCollectionController.cs.
- **Category B** — a hit under the TaskMaster, TaskMaster.Test, or UtilitiesCS trees whose matched
  identifier is `LoadSequentialAsync`.
- **Category C** — a hit whose line's first non-whitespace token is `//` or `///`.
- **Category G** — a hit matched by none of the above; the "genuine name-based caller of a removed
  member" category, which must be empty.

The tests are applied in that order, so a hit satisfying more than one test takes the earliest. Ten of
the twenty-eight category B rows are comment lines that would also satisfy the category C test in
isolation; because B is applied before C they are category B, and the counts below reflect that
ordering.

## Per-hit enumeration, 31 rows

| # | File | Line | Matched identifier | Category |
|---|---|---|---|---|
| 1 | QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs | 60 | `WireUpKeyboardHandler` | C |
| 2 | QuickFiler/Controllers/QfcCollectionController.cs | 344 | `LoadItemGroup` (bare stem, inside `LoadItemGroupsAndViewers_02`) | A |
| 3 | QuickFiler/Controllers/QfcCollectionController.cs | 669 | `LoadItemGroup` (bare stem, inside `LoadItemGroupsAndViewers_02`) | A |
| 4 | TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs | 322 | `LoadSequentialAsync` | B |
| 5 | TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs | 187 | `LoadSequentialAsync` | B |
| 6 | TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs | 200 | `LoadSequentialAsync` | B |
| 7 | TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs | 226 | `LoadSequentialAsync` | B |
| 8 | TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs | 238 | `LoadSequentialAsync` | B |
| 9 | TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs | 253 | `LoadSequentialAsync` | B |
| 10 | TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs | 274 | `LoadSequentialAsync` | B |
| 11 | TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs | 298 | `LoadSequentialAsync` | B |
| 12 | TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs | 303 | `LoadSequentialAsync` | B |
| 13 | TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs | 334 | `LoadSequentialAsync` | B |
| 14 | TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs | 358 | `LoadSequentialAsync` | B |
| 15 | TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs | 14 | `LoadSequentialAsync` | B |
| 16 | TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs | 27 | `LoadSequentialAsync` | B |
| 17 | TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs | 43 | `LoadSequentialAsync` | B |
| 18 | TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs | 122 | `LoadSequentialAsync` | B |
| 19 | TaskMaster.Test/AppGlobals/TestableApplicationGlobals.cs | 11 | `LoadSequentialAsync` | B |
| 20 | TaskMaster.Test/AppGlobals/TestableApplicationGlobals.cs | 95 | `LoadSequentialAsync` | B |
| 21 | TaskMaster/AppGlobals/AppAutoFileObjects.cs | 60 | `LoadSequentialAsync` | B |
| 22 | TaskMaster/AppGlobals/AppAutoFileObjects.cs | 84 | `LoadSequentialAsync` | B |
| 23 | TaskMaster/AppGlobals/AppToDoObjects.cs | 41 | `LoadSequentialAsync` | B |
| 24 | TaskMaster/AppGlobals/AppToDoObjects.cs | 63 | `LoadSequentialAsync` | B |
| 25 | TaskMaster/AppGlobals/ApplicationGlobals.cs | 84 | `LoadSequentialAsync` | B |
| 26 | TaskMaster/AppGlobals/ApplicationGlobals.cs | 144 | `LoadSequentialAsync` | B |
| 27 | TaskMaster/AppGlobals/StartupDiagnosticsProbe.cs | 9 | `LoadSequentialAsync` | B |
| 28 | TaskMaster/AppGlobals/StartupDiagnosticsProbe.cs | 95 | `LoadSequentialAsync` | B |
| 29 | TaskMaster/AppGlobals/StartupDiagnosticsProbe.cs | 164 | `LoadSequentialAsync` | B |
| 30 | TaskMaster/ThisAddIn.cs | 49 | `LoadSequentialAsync` | B |
| 31 | UtilitiesCS/OutlookObjects/Store/StoreWrapperInitClock.cs | 16 | `LoadSequentialAsync` | B |

The enumerated row count is 31, one row per printed line, which equals the recorded
`PARTITION_C_HITS` value.

## Category counts and their sum

```
CAT_A 2 + CAT_B 28 + CAT_C 1 + CAT_G 0 = 31 = PARTITION_C_HITS
```

- **CAT_A: 2** — rows 2 and 3, both in QuickFiler/Controllers/QfcCollectionController.cs. Both are the
  live, preserved member `LoadItemGroupsAndViewers_02`: row 3 is its declaration and row 2 is a call to
  it. Neither is a removed member. They are matched only because the search supplies identifier 7 as
  the bare stem `LoadItemGroup`, which is a strict prefix of `LoadItemGroupsAndViewers_02`. No
  declaration or call of any of the twelve removed methods, and no occurrence of `_templateTlp`,
  appears anywhere in the QuickFiler production tree.
- **CAT_B: 28** — rows 4 through 31. Every one matches `LoadSequentialAsync` and every one lies under
  the TaskMaster, TaskMaster.Test, or UtilitiesCS trees. The name belongs to three live and unrelated
  members in the TaskMaster startup assembly, declared at
  TaskMaster/AppGlobals/ApplicationGlobals.cs line 144,
  TaskMaster/AppGlobals/AppToDoObjects.cs line 63, and
  TaskMaster/AppGlobals/AppAutoFileObjects.cs line 84. The remaining rows are calls to those members,
  their tests, and doc comments referring to them. None of the three declaring types is
  `QfcCollectionController` and none of the twenty-eight rows lies in either QuickFiler tree.
- **CAT_C: 1** — row 1, QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
  line 60, whose first non-whitespace token is `///`.
- **CAT_G: 0** — no row is matched by none of the preceding tests. The category is empty.

## The statement the Phase 2 closure argument consumes

No string literal anywhere in the QuickFiler test tree equals one of the thirteen identifiers.

Row 1 of the enumeration is the sole occurrence of any of the thirteen identifiers anywhere in the
QuickFiler test tree. It is a triple-slash documentation comment, not a string literal. Since it is the
only occurrence, and it is not a string literal, there is no string literal in that tree equal to any
of the thirteen. This is the input the Phase 2 closure argument in [P2-T3] consumes to bound the values
the member-name variables at the receiver-scoped reflection call sites can take.

## Stability of the 31 figure

The 31 figure is stable for this plan's execution because this plan writes no file with a `.cs`
extension. `git grep -- "*.cs"` searches tracked `.cs` files only, and the item's entire change set is
Markdown under the feature folder. [P0-T5] independently corroborates this: `TRACKED_CS` measured 1,599
at HEAD, matching the specification's base-commit reference value exactly, so no `.cs` file has been
added or removed on this branch.
