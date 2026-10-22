# [P0-T15] Source-fact baseline

Timestamp: 2026-08-26T08-25

Command: `wc -l QuickFiler/Controllers/QfcCollectionController.cs QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs`
Command: `grep -n "" QuickFiler.Test/QuickFiler.Test.csproj | sed -n '114,121p'`
Command: `grep -n -E "<the fourteen literals>" QuickFiler/Controllers/QfcCollectionController.cs`
Command: `grep -c "\[TestMethod\]" QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

All three line counts match the plan's stated expectations exactly. All thirteen `#468` identifiers
and the `(QfcFormController)_parent` literal are present with non-zero counts. One **stale line
number** was found in the plan (D13 / P0-T15's `116 through 118` window); it is recorded in full
below and is non-blocking.

### 1. Line counts

| File | Measured | Plan expected | Match |
|---|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | **2349** | 2349 | yes |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | **500** | 500 | yes |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` | **155** | 155 | yes |

`QfcCollectionControllerTests.cs` is exactly at the 500-line cap in
`.claude/rules/general-code-change.md`, which is why D12 forbids adding any new test method to it
(AC-22) and why five new test files are used instead.

### 2. `QuickFiler.Test/QuickFiler.Test.csproj` — the csproj insertion point

Lines **116 through 118**, verbatim, as the task text names them:

```
116:    <Compile Include="Controllers\EfcHomeControllerSeamTests.cs" />
117:    <Compile Include="Controllers\QfcCollectionControllerTests.cs" />
118:    <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
```

Surrounding context for unambiguity:

```
114:    <Compile Include="Controllers\BayesianPerformanceControllerTests.cs" />
115:    <Compile Include="Controllers\BayesianPerformanceController.TestSupport.cs" />
116:    <Compile Include="Controllers\EfcHomeControllerSeamTests.cs" />
117:    <Compile Include="Controllers\QfcCollectionControllerTests.cs" />
118:    <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
119:    <Compile Include="Controllers\QfcDatamodelTests.cs" />
120:    <Compile Include="Controllers\QfcDatamodelLivenessTests.cs" />
121:    <Compile Include="Controllers\QfcExplorerController.ConversationViewTests.cs" />
```

#### PLAN DEFECT — stale line numbers in D13 (non-blocking)

Decision D13 states: "Verified at the base commit: line 116 is the `QfcCollectionControllerTests.cs`
entry, line 117 the `QfcCollectionControllerDarkModeTests.cs` entry, line 118 the
`QfcDatamodelTests.cs` entry." P0-T15 repeats the same `116 through 118` window, and
`research/test-harness-feasibility.md:18` and `:709` likewise cite `:116-117`.

Measured on **this branch's** base commit `61edc19b`, those three entries are at lines **117, 118,
and 119** — one lower than stated.

Cause, established by `git log --oneline 988e819b..HEAD -- QuickFiler.Test/QuickFiler.Test.csproj`:

```
c39db103 feat(efcviewer): preserve archive lineage for breadcrumb navigation
```

That commit inserted `<Compile Include="Controllers\EfcHomeControllerSeamTests.cs" />` at line 116,
shifting the whole `QfcCollectionController` family block down by one. The research was written
against base commit `988e819b`; this branch is cut from `origin/epic/quickfiler-bug-family-integration`
at `61edc19b`, which already contains `c39db103`.

**Impact: none on executability.** D13, P2-T2, and P2-T5 all specify the insertion point by
*content* — "immediately after the existing `Controllers\QfcCollectionControllerDarkModeTests.cs`
entry and immediately before the existing `Controllers\QfcDatamodelTests.cs` entry" — which is
unambiguous and correct. Only the parenthetical line numbers are stale. The executor will insert by
content, and the resulting order will be:

```
    <Compile Include="Controllers\QfcCollectionControllerTests.cs" />
    <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
    <Compile Include="Controllers\QfcCollectionController.TestSupport.cs" />
    <Compile Include="Controllers\QfcCollectionControllerDefects468Tests.cs" />
    <Compile Include="Controllers\QfcCollectionControllerDefects468MoveTests.cs" />
    <Compile Include="Controllers\QfcCollectionControllerDefects468ConversationTests.cs" />
    <Compile Include="Controllers\QfcCollectionControllerLayout.StaTests.cs" />
    <Compile Include="Controllers\QfcDatamodelTests.cs" />
```

The plan's line numbers are not modified; this artifact records the correction. Later tasks must not
assert against the literal numbers 116/117/118.

### 3. Per-identifier hit counts in `QuickFiler/Controllers/QfcCollectionController.cs`

Fixed-string matching, scoped to that single file. All thirteen `#468` identifiers are **non-zero**
at baseline, which is the contrast P1-T3 asserts each zero against.

| # | Identifier | Baseline hits | Line numbers |
|---|---|---|---|
| 1 | `WireUpKeyboardHandler` | **1** | 1254 |
| 2 | `AnyOpenDropDownsAsync` | **1** | 1324 |
| 3 | `LoadGroups_02cAsync` | **1** | 587 |
| 4 | `LoadGroups_02bAsync` | **2** | 402, 635 |
| 5 | `LoadGroup_03bAsync` | **2** | 647, 654 |
| 6 | `LoadConversationsAndFoldersAsync` | **1** | 761 |
| 7 | `LoadItemGroup(` | **2** | 772, 776 |
| 8 | `LoadSequentialAsync` | **1** | 827 |
| 9 | `LoadGroupSequential` | **2** | 838, 842 |
| 10 | `CacheTlpForMove` | **2** | 865, 872 |
| 11 | `SwapTlp` | **1** | 870 |
| 12 | `CaptureTlpTemplate` | **1** | 1991 |
| 13 | `_templateTlp` | **3** | 70, 1994, 1995 |

Verbatim matched lines:

```
  70:        private TableLayoutPanel _templateTlp;
 402:            //await LoadGroups_02bAsync(items, template, _tlpStates);
 587:        public async Task LoadGroups_02cAsync(
 635:        public async Task LoadGroups_02bAsync(
 647:                    (mailItem, i) => LoadGroup_03bAsync(template, mailItem, i, digits, tlpStates)
 654:        private async Task<QfcItemGroup> LoadGroup_03bAsync(
 761:        public async Task LoadConversationsAndFoldersAsync()
 772:                .ForEachAsync(async x => await LoadItemGroup(x.i, x.grp));
 776:        internal async Task LoadItemGroup(int i, QfcItemGroup group)
 827:        public async Task LoadSequentialAsync()
 838:                .ForEachAsync(async x => await LoadGroupSequential(x.i, x.grp));
 842:        public async Task LoadGroupSequential(int i, QfcItemGroup grp)
 865:        internal void CacheTlpForMove()
 870:        internal void SwapTlp(TableLayoutPanel tlp)
 872:            CacheTlpForMove();
1254:        public void WireUpKeyboardHandler()
1324:        internal async Task<bool> AnyOpenDropDownsAsync(bool close, CancellationToken token)
1991:        internal void CaptureTlpTemplate()
1994:            _templateTlp = _formViewer.L1v0L2L3v_TableLayout.Clone();
1995:            _templateTlp.Name = "TemplateTableLayout";
```

Every declaration line in this table agrees with `research/qfc-collection-controller-defects.md` §2,
including the commented-out reference at `:402` that P1-T2 must also delete.

Disambiguation confirmed by measurement:

- Identifier 7 is deliberately spelled `LoadItemGroup(` with the open parenthesis. The bare stem
  `LoadItemGroup` would also match the **live** `LoadItemGroupsAndViewers_02`, making a later
  zero-hit assertion unsatisfiable. With the parenthesis it matches only the dead member's
  declaration (`:776`) and its dead self-call (`:772`).
- Identifier 2 is `AnyOpenDropDownsAsync`, matching only `:1324`. The **live** non-async overload
  `AnyOpenDropDowns(` at `:1319` is a distinct literal and is preserved per D3.
- `SwapTlp` at `:870` is the dead wrapper; the live `ActivateQueuedTlp` it wraps is a distinct
  identifier and is preserved per D3/AC-3.

### 4. `(QfcFormController)_parent` hit count

| Literal | Baseline hits | Line numbers | Plan expected |
|---|---|---|---|
| `(QfcFormController)_parent` | **1** | 1232 | 1, at `:1232` |

Verbatim:

```
1232:                    await ((QfcFormController)_parent).SkipGroupAsync();
```

This is the non-zero baseline that P2-T9 contrasts its zero against (AC-14's search half).

Supporting facts for P2-T7 and P2-T8, measured on the same base:

```
QuickFiler/Controllers/QfcCollectionController.cs:35:            IFilerFormController parent,
QuickFiler/Controllers/QfcCollectionController.cs:64:        private IFilerFormController _parent;
QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs:45:            var mockParent = new Mock<IFilerFormController>();
```

All three agree with the plan and the research: constructor parameter at `:35`, field at `:64`, and
the dark-mode mock at `:45`. `IFilerFormController` occurs at exactly those two lines in `<CTRL>` and
that one line in the dark-mode test, so P2-T7's and P2-T8's edits are fully enumerated.

### 5. `[TestMethod]` occurrence count

| File | `[TestMethod]` count |
|---|---|
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | **13** |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` | 2 (recorded for context) |

**13** is the baseline that P4-T5 and P14-T11 compare against. AC-22 requires this number to be
unchanged at the end of the feature: `QfcCollectionControllerTests.cs`'s only permitted change is
the `_itemGroupsToMove` injection type at `:66-71`.

The current injection, verbatim (`QfcCollectionControllerTests.cs:66-71`):

```csharp
            var dict = new ConcurrentDictionary<QfcItemGroup, int>();
            dict.TryAdd(itemGroup, 0);

            typeof(QfcCollectionController)
                .GetField("_itemGroupsToMove", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(controller, dict);
```

### Acceptance verification

- Three line counts recorded: 2349, 500, 155 — all matching the plan's expectations.
- The three verbatim csproj lines recorded (with the stale-line-number defect documented).
- Thirteen non-zero identifier hit counts recorded, with line numbers.
- A non-zero `(QfcFormController)_parent` count recorded: 1, at `:1232`.
- The baseline `[TestMethod]` count recorded: 13.

Result: PASS, with one non-blocking plan defect reported (stale csproj line numbers in D13).
