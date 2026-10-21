# [P1-T1] Issue #468 residual-risk search (AC-16)

Timestamp: 2026-08-26T08-25

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

**Search (a) returns zero hits across 398 build-input files. Search (b) enumerates 42 `GetMethod(`
hits and 0 `InvokeMember(` hits; none passes any of the twelve identifiers. No genuine reference to
a removed `QfcCollectionController` member exists. The task does not block.**

---

## Search (a) — build-input file types only

Command:

```
grep -rn --include=*.csproj --include=*.resx --include=*.config --include=*.xaml \
        --include=*.json --include=*.settings \
        --exclude-dir=docs --exclude-dir=.claude --exclude-dir=packages --exclude-dir=TestResults \
        -e 'WireUpKeyboardHandler' -e 'AnyOpenDropDownsAsync' -e 'LoadGroups_02cAsync' \
        -e 'LoadGroups_02bAsync' -e 'LoadGroup_03bAsync' -e 'LoadConversationsAndFoldersAsync' \
        -e 'LoadItemGroup' -e 'LoadSequentialAsync' -e 'LoadGroupSequential' \
        -e 'CacheTlpForMove' -e 'SwapTlp' -e 'CaptureTlpTemplate' .
```

Verbatim output:

```
(no output)
```

Exit code 1 (grep's "no lines selected"). **Zero hits.**

Note the search term for identifier 7 is the bare stem `LoadItemGroup`, not `LoadItemGroup(`. This
is deliberately **broader** than the removal target: a zero result over the wider term is a strictly
stronger negative than a zero over the narrower one.

### Non-vacuity of search (a)

A zero-hit search proves nothing if its scope is empty. The scope was measured:

```
find . -type f \( -name "*.csproj" -o -name "*.resx" -o -name "*.config" -o -name "*.xaml" \
        -o -name "*.json" -o -name "*.settings" \) \
     -not -path "./docs/*" -not -path "./.claude/*" -not -path "./packages/*" \
     -not -path "./TestResults/*" | wc -l
```

```
    157 config
     18 csproj
    148 json
     63 resx
      7 settings
      5 xaml
=== total ===
398
```

**398 files** were searched and **all six** declared extensions are represented, including all 18
`.csproj` files and all 63 `.resx` files. The zero is a real negative.

---

## Search (b) — reflective call sites in the `QuickFiler` and `QuickFiler.Test` trees

Commands:

```
grep -rnF --include=*.cs -e 'GetMethod(' -e 'InvokeMember(' QuickFiler QuickFiler.Test \
  | grep -v "/bin/\|/obj/"
```

Counts:

| Pattern | Hits (excluding `bin/` and `obj/`) |
|---|---|
| `GetMethod(` in `QuickFiler` **production** tree | **0** |
| `GetMethod(` in `QuickFiler.Test` tree | **42** |
| `InvokeMember(` in either tree | **0** (grep exit 1, no output) |

Every one of the 42 hits is in the **test** project. The `QuickFiler` production assembly performs
no reflective method lookup at all, so no production code path can late-bind to a removed member.

### Per-hit enumeration

For each hit the resolved method-name argument is given. **None is one of the twelve identifiers.**
Paths are abbreviated `QFT/` for `QuickFiler.Test/`.

| # | Hit | Method-name argument | One of the twelve? |
|---|---|---|---|
| 1 | `QFT/Controllers/EfcDataModelTests.cs:399` | `"Count"` | No |
| 2 | `QFT/Controllers/EfcHomeControllerTests.cs:118` | `"CaptureSelectionSnapshot"` | No |
| 3 | `QFT/Controllers/EfcHomeControllerTests.cs:140` | `"BuildFirstSelectionTimingContext"` | No |
| 4 | `QFT/Controllers/EfcHomeControllerTests.cs:165` | `"LogFirstSelectionTiming"` | No |
| 5 | `QFT/Controllers/QfcDatamodelTests.cs:263` | `"ToggleOfflineMode"` | No |
| 6 | `QFT/Controllers/QfcDatamodelTests.cs:298` | `"WaitForQueue"` | No |
| 7 | `QFT/Controllers/QfcFormControllerTests.cs:156` | `"MaximizeFormViewer"` | No |
| 8 | `QFT/Controllers/QfcHomeControllerRunAsyncTests.cs:348` | `"Worker_RunWorkerCompleted"` | No |
| 9 | `QFT/Controllers/QfcItemController.EventWiringTests.cs:262` | `"OnPreviewKeyDown"` | No |
| 10 | `QFT/Controllers/QfcItemController.EventWiringTests.cs:266` | `"OnKeyDown"` | No |
| 11 | `QFT/Controllers/QfcItemController.EventWiringTests.cs:270` | `"OnMouseEnter"` | No |
| 12 | `QFT/Controllers/QfcItemController.EventWiringTests.cs:342` | `"OnPreviewKeyDown"` | No |
| 13 | `QFT/Controllers/QfcItemController.TestSupport.cs:72` | variable `name` — see (V1) | No |
| 14 | `QFT/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:167` | `"OnBreadcrumbUnhandledArrow"` | No |
| 15 | `QFT/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:293` | `"ConfigureBreadcrumbDropDown"` | No |
| 16 | `QFT/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:311` | `"ConfigureBreadcrumbDropDown"` | No |
| 17 | `QFT/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:193` | `"DequeueAsync"` | No |
| 18 | `QFT/Helper Classes/ConversationResolverTests.cs:432` | `"Count"` | No |
| 19 | `QFT/Helper Classes/QfcThemeHelperTests.cs:279` | variable `methodName` — see (V2) | No |
| 20 | `QFT/Viewers/BreadcrumbCoordinatorLifecycleTests.cs:381` | `"PostRenderAndSelectorAsync"` | No |
| 21 | `QFT/Viewers/BreadcrumbDropDownHostTests.cs:346` | `"OpenAsync"` | No |
| 22 | `QFT/Viewers/BreadcrumbDropDownHostTests.cs:354` | `"Close"` | No |
| 23 | `QFT/Viewers/BreadcrumbDropDownHostTests.cs:360` | variable `method` — see (V3) | No |
| 24 | `QFT/Viewers/BreadcrumbDropDownHostTests.cs:364` | variable `method` — see (V3) | No |
| 25 | `QFT/Viewers/BreadcrumbDropDownHostTests.cs:370` | `"CompleteClose"` | No |
| 26 | `QFT/Viewers/BreadcrumbDropDownIntegrationTests.cs:386` | `"ConfigureBreadcrumbDropDown"` | No |
| 27 | `QFT/Viewers/BreadcrumbDropDownIntegrationTests.cs:414` | `"AttachBreadcrumbMessenger"` | No |
| 28 | `QFT/Viewers/BreadcrumbDropDownIntegrationTests.cs:450` | `"SetBreadcrumbTheme"` | No |
| 29 | `QFT/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs:257` | `"OnDropDownClosed"` | No |
| 30 | `QFT/Viewers/BreadcrumbDropDownLifecycleTests.cs:143` | variable `method` — see (V3) | No |
| 31 | `QFT/Viewers/BreadcrumbDropDownLifecycleTests.cs:206` | `"OpenAsync"` | No |
| 32 | `QFT/Viewers/BreadcrumbDropDownLifecycleTests.cs:222` | `"Close"` | No |
| 33 | `QFT/Viewers/BreadcrumbMessengerHubTests.cs:348` | `"Attach"` | No |
| 34 | `QFT/Viewers/BreadcrumbMessengerHubTests.cs:351` | `"Detach"` | No |
| 35 | `QFT/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs:452` | `"OnDropDownClosed"` | No |
| 36 | `QFT/Viewers/BreadcrumbPopupPlacementTests.cs:145` | `"Calculate"` | No |
| 37 | `QFT/Viewers/BreadcrumbSelectorCoordinatorTests.cs:402` | variable `method` — see (V3) | No |
| 38 | `QFT/Viewers/BreadcrumbSubfolderActivationTests.cs:407` | `"Serialize"` | No |
| 39 | `QFT/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:35` | `"ConfigureBreadcrumbDropDown"` | No |
| 40 | `QFT/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:55` | `"ConfigureBreadcrumbDropDown"` | No |
| 41 | `QFT/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:83` | `"SetFolderDroppedDown"` | No |
| 42 | `QFT/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:110` | `"OpenBreadcrumbDropDownAsync"` | No |

The 28 distinct literals actually appearing are: `Attach`, `AttachBreadcrumbMessenger`,
`BuildFirstSelectionTimingContext`, `Calculate`, `CaptureSelectionSnapshot`, `Close`,
`CompleteClose`, `ConfigureBreadcrumbDropDown`, `Count`, `DequeueAsync`, `Detach`,
`LogFirstSelectionTiming`, `MaximizeFormViewer`, `OnBreadcrumbUnhandledArrow`,
`OnDropDownClosed`, `OnKeyDown`, `OnMouseEnter`, `OnPreviewKeyDown`, `OpenAsync`,
`OpenBreadcrumbDropDownAsync`, `PostRenderAndSelectorAsync`, `Serialize`, `SetBreadcrumbTheme`,
`SetFolderDroppedDown`, `ToggleOfflineMode`, `Uncommitted`, `WaitForQueue`,
`Worker_RunWorkerCompleted`. None matches any of the twelve.

### The five variable-argument hits

- **(V1)** `QfcItemController.TestSupport.cs:72` — `typeof(QfcItemController).GetMethod(name, ...)`.
  The receiver type is `QfcItemController`, not `QfcCollectionController`, so no value of `name`
  can resolve a `QfcCollectionController` member.
- **(V2)** `QfcThemeHelperTests.cs:279` — `typeof(Control).GetMethod(methodName, ...)`. The receiver
  is `System.Windows.Forms.Control`.
- **(V3)** `BreadcrumbDropDownHostTests.cs:360` and `:364`,
  `BreadcrumbDropDownLifecycleTests.cs:143`, `BreadcrumbSelectorCoordinatorTests.cs:402` — all are
  `<instance>.GetType().GetMethod(method)` where the instance is a breadcrumb popup host or
  selector coordinator. None is a `QfcCollectionController`.

Independently of the receiver-type argument, the sweep below rules out any value of these variables.

---

## Corroborating scoped identifier sweep (decisive)

The per-hit enumeration above depends on reading each call site. A single scoped sweep settles the
question mechanically. Command:

```
grep -rn --include=*.cs -e '<each of the twelve>' -e '_templateTlp' QuickFiler QuickFiler.Test \
  | grep -v "/bin/\|/obj/"
```

Verbatim output — **every** hit, without exception, is inside
`QuickFiler/Controllers/QfcCollectionController.cs` itself:

```
QuickFiler/Controllers/QfcCollectionController.cs:70:        private TableLayoutPanel _templateTlp;
QuickFiler/Controllers/QfcCollectionController.cs:287:            LoadItemGroupsAndViewers_02(listMailItems, template);
QuickFiler/Controllers/QfcCollectionController.cs:402:            //await LoadGroups_02bAsync(items, template, _tlpStates);
QuickFiler/Controllers/QfcCollectionController.cs:587:        public async Task LoadGroups_02cAsync(
QuickFiler/Controllers/QfcCollectionController.cs:635:        public async Task LoadGroups_02bAsync(
QuickFiler/Controllers/QfcCollectionController.cs:647:                    (mailItem, i) => LoadGroup_03bAsync(template, mailItem, i, digits, tlpStates)
QuickFiler/Controllers/QfcCollectionController.cs:654:        private async Task<QfcItemGroup> LoadGroup_03bAsync(
QuickFiler/Controllers/QfcCollectionController.cs:740:        public void LoadItemGroupsAndViewers_02(IList<MailItem> items, RowStyle template)
QuickFiler/Controllers/QfcCollectionController.cs:761:        public async Task LoadConversationsAndFoldersAsync()
QuickFiler/Controllers/QfcCollectionController.cs:772:                .ForEachAsync(async x => await LoadItemGroup(x.i, x.grp));
QuickFiler/Controllers/QfcCollectionController.cs:776:        internal async Task LoadItemGroup(int i, QfcItemGroup group)
QuickFiler/Controllers/QfcCollectionController.cs:827:        public async Task LoadSequentialAsync()
QuickFiler/Controllers/QfcCollectionController.cs:838:                .ForEachAsync(async x => await LoadGroupSequential(x.i, x.grp));
QuickFiler/Controllers/QfcCollectionController.cs:842:        public async Task LoadGroupSequential(int i, QfcItemGroup grp)
QuickFiler/Controllers/QfcCollectionController.cs:865:        internal void CacheTlpForMove()
QuickFiler/Controllers/QfcCollectionController.cs:870:        internal void SwapTlp(TableLayoutPanel tlp)
QuickFiler/Controllers/QfcCollectionController.cs:872:            CacheTlpForMove();
QuickFiler/Controllers/QfcCollectionController.cs:1254:        public void WireUpKeyboardHandler()
QuickFiler/Controllers/QfcCollectionController.cs:1324:        internal async Task<bool> AnyOpenDropDownsAsync(bool close, CancellationToken token)
QuickFiler/Controllers/QfcCollectionController.cs:1991:        internal void CaptureTlpTemplate()
QuickFiler/Controllers/QfcCollectionController.cs:1994:            _templateTlp = _formViewer.L1v0L2L3v_TableLayout.Clone();
QuickFiler/Controllers/QfcCollectionController.cs:1995:            _templateTlp.Name = "TemplateTableLayout";
```

Consequences:

- **Zero hits anywhere in `QuickFiler.Test`.** No test file contains any of the twelve identifiers
  as source text, so no `[DataRow]`, constant, or variable in that project can carry one as a
  string. This closes the five variable-argument hits (V1)-(V3) mechanically.
- **Zero hits elsewhere in the `QuickFiler` production tree.** The only in-file callers are the dead
  members' own dead self-calls (`:402`, `:647`, `:772`, `:838`, `:872`), all of which P1-T2 deletes
  in the same edit.
- The only two lines that are **not** in the P0-T15 baseline table are `:287` and `:740`. Both match
  the broad stem `LoadItemGroup` and belong to the **live** member `LoadItemGroupsAndViewers_02`,
  which D3 and AC-3 require to be preserved. This is exactly the naming trap the plan's narrower
  literal `LoadItemGroup(` is designed to avoid, and it is why P1-T3's assertion uses the
  parenthesised form.

---

## Why a repository-wide identifier sweep is deliberately NOT performed

Recorded as the plan's `### Literals asserted by acceptance conditions` convention requires.

A repository-wide zero-hit condition for these identifiers is **unsatisfiable by construction**:

1. `LoadSequentialAsync` names three unrelated **live** members outside `QuickFiler`, in
   `TaskMaster/AppGlobals/` — `ApplicationGlobals.cs:139`, `AppToDoObjects.cs:63`, and
   `AppAutoFileObjects.cs:84`, plus their tests (27 out-of-file hits, per
   `research/qfc-collection-controller-defects.md` §2 "Disambiguation notes"). Removing
   `QfcCollectionController.LoadSequentialAsync` does not and must not affect them.
2. `docs/features/**` quotes every one of the twelve identifiers — this feature's own `spec.md`,
   `issue.md`, both research documents, the plan, and this artifact all name them in prose.

Every search in this plan is therefore scoped to a named file or a named directory. The scoping used
here (`QuickFiler` and `QuickFiler.Test` trees for search (b); six build-input extensions with
`docs/`, `.claude/`, `packages/` and `TestResults/` excluded for search (a)) is exactly the scoping
AC-16 specifies.

---

## Acceptance verification

- Both commands recorded with verbatim output.
- **Search (a) returns zero hits**, over a measured non-empty scope of 398 files spanning all six
  extensions.
- **Every hit from search (b) is enumerated** (42 rows above), with a per-hit statement that its
  method-name argument is not one of the twelve identifiers. `InvokeMember(` yields zero hits.
- The note on the deliberately-omitted repository-wide sweep is present.
- **No hit in (a) or (b) is a genuine reference to a `QfcCollectionController` member.** The task
  does not block and nothing is escalated.

Result: PASS. The `#468` residual risk identified in `research/qfc-collection-controller-defects.md`
§2 "Residual risk" and §11 question 1 is settled: the twelve members have no build-input reference
and no reflective caller.
