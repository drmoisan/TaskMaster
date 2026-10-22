# Production-Tree System.Reflection Classification (P2-T2)

- **Issue:** #635
- **Plan task:** [P2-T2]

Timestamp: 2026-08-29T06-32

## Output Summary

All 39 `System.Reflection` occurrences in the QuickFiler production tree were enumerated and each was
assigned to exactly one of five classes by a test applied in order. Twenty-six are the log4net
logger-declaration idiom, three are `using System.Reflection;` directives, three are comments or
commented-out code, seven are tracked non-source project or package-manifest entries, and none is a
call site taking a member-name argument. The five class counts sum to 39, which is the production value
[P2-T1] printed for `System.Reflection`.

L1: 26
L2: 3
L3: 3
L4: 7
L5: 0

## Command

Command:

```
git grep -n -I -F -e "System.Reflection" -- "QuickFiler/*"
```

EXIT_CODE: 0

Output, verbatim:

```
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:22:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/EfcDataModel.cs:24:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/EfcFormController.cs:124:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/EfcHomeController.cs:5:using System.Reflection;
QuickFiler/Controllers/EfcItemController.cs:157:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/EmailSorter.cs:10:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/FilerQueue.cs:17:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/KbdActions.cs:18:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/KeyboardHandler.cs:26:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/QfcCollectionController.cs:25:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/QfcDatamodel.cs:29:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/QfcDatamodel.cs:98:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/QfcDatamodel.cs:463:            //var item = _globals.Ol.App.Session.GetItemFromID(entryID, System.Reflection.Missing.Value);
QuickFiler/Controllers/QfcExplorerController.cs:13:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/QfcExplorerController.cs:55:        // log4net.ILog and System.Reflection.MethodBase above.
QuickFiler/Controllers/QfcFormController.cs:22:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/QfcFormController.cs:68:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:26:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/QfcHomeController.cs:22:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/QfcItemController.cs:31:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/QfcQueue.cs:27:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:45:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Helper Classes/ConversationResolver.cs:33:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Helper Classes/EmailMoveMonitor.cs:21:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Legacy/QfcController.cs:8:using System.Reflection;
QuickFiler/Legacy/QuickFileController.cs:20:        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
QuickFiler/Properties/AssemblyInfo.cs:1:﻿using System.Reflection;
QuickFiler/QuickFiler.csproj:214:    <Reference Include="System.Reflection, Version=4.1.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL">
QuickFiler/QuickFiler.csproj:215:      <HintPath>..\packages\System.Reflection.4.3.0\lib\net462\System.Reflection.dll</HintPath>
QuickFiler/QuickFiler.csproj.bak:167:    <Reference Include="System.Reflection, Version=4.1.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL">
QuickFiler/QuickFiler.csproj.bak:168:      <HintPath>..\packages\System.Reflection.4.3.0\lib\net462\System.Reflection.dll</HintPath>
QuickFiler/Viewers/EfcViewer.cs:33:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Viewers/QfcFormViewer.cs:29:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Viewers/QfcFormViewerDark.cs:25:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Viewers/QfcFormViewerExpanded.cs:25:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/Viewers/WebView2BreadcrumbHost.cs:38:            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
QuickFiler/packages.config:77:  <package id="System.Reflection" version="4.3.0" targetFramework="net481" />
QuickFiler/packages.config:78:  <package id="System.Reflection.Extensions" version="4.3.0" targetFramework="net481" />
QuickFiler/packages.config:79:  <package id="System.Reflection.Primitives" version="4.3.0" targetFramework="net481" />
```

## The five classes and their tests, applied in order

- **L1** — the log4net logger-declaration idiom: a line containing `MethodBase.GetCurrentMethod()`
  whose first non-whitespace token is not `//`.
- **L2** — a `using System.Reflection;` directive.
- **L3** — a comment or commented-out code: a line whose first non-whitespace token is `//`, or that
  lies inside a block comment.
- **L4** — a tracked non-source file entry: a project-file assembly reference, a hint path, or a
  package-manifest entry.
- **L5** — a call site taking a member-name argument.

The tests are applied in that order, so a line satisfying more than one takes the earliest. One line
turns on that ordering and is discussed under class L3 below.

## Per-occurrence classification, 39 rows

| # | File | Line | Class |
|---|---|---|---|
| 1 | QuickFiler/Controllers/BreadcrumbBridgeRouter.cs | 22 | L1 |
| 2 | QuickFiler/Controllers/EfcDataModel.cs | 24 | L1 |
| 3 | QuickFiler/Controllers/EfcFormController.cs | 124 | L1 |
| 4 | QuickFiler/Controllers/EfcHomeController.cs | 5 | L2 |
| 5 | QuickFiler/Controllers/EfcItemController.cs | 157 | L1 |
| 6 | QuickFiler/Controllers/EmailSorter.cs | 10 | L1 |
| 7 | QuickFiler/Controllers/FilerQueue.cs | 17 | L1 |
| 8 | QuickFiler/Controllers/KbdActions.cs | 18 | L1 |
| 9 | QuickFiler/Controllers/KeyboardHandler.cs | 26 | L1 |
| 10 | QuickFiler/Controllers/QfcCollectionController.cs | 25 | L1 |
| 11 | QuickFiler/Controllers/QfcDatamodel.cs | 29 | L1 |
| 12 | QuickFiler/Controllers/QfcDatamodel.cs | 98 | L1 |
| 13 | QuickFiler/Controllers/QfcDatamodel.cs | 463 | L3 |
| 14 | QuickFiler/Controllers/QfcExplorerController.cs | 13 | L1 |
| 15 | QuickFiler/Controllers/QfcExplorerController.cs | 55 | L3 |
| 16 | QuickFiler/Controllers/QfcFormController.cs | 22 | L1 |
| 17 | QuickFiler/Controllers/QfcFormController.cs | 68 | L1 |
| 18 | QuickFiler/Controllers/QfcHighConfidencePreFilter.cs | 26 | L1 |
| 19 | QuickFiler/Controllers/QfcHomeController.cs | 22 | L1 |
| 20 | QuickFiler/Controllers/QfcItemController.cs | 31 | L1 |
| 21 | QuickFiler/Controllers/QfcQueue.cs | 27 | L1 |
| 22 | QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs | 45 | L1 |
| 23 | QuickFiler/Helper Classes/ConversationResolver.cs | 33 | L1 |
| 24 | QuickFiler/Helper Classes/EmailMoveMonitor.cs | 21 | L1 |
| 25 | QuickFiler/Legacy/QfcController.cs | 8 | L2 |
| 26 | QuickFiler/Legacy/QuickFileController.cs | 20 | L3 |
| 27 | QuickFiler/Properties/AssemblyInfo.cs | 1 | L2 |
| 28 | QuickFiler/QuickFiler.csproj | 214 | L4 |
| 29 | QuickFiler/QuickFiler.csproj | 215 | L4 |
| 30 | QuickFiler/QuickFiler.csproj.bak | 167 | L4 |
| 31 | QuickFiler/QuickFiler.csproj.bak | 168 | L4 |
| 32 | QuickFiler/Viewers/EfcViewer.cs | 33 | L1 |
| 33 | QuickFiler/Viewers/QfcFormViewer.cs | 29 | L1 |
| 34 | QuickFiler/Viewers/QfcFormViewerDark.cs | 25 | L1 |
| 35 | QuickFiler/Viewers/QfcFormViewerExpanded.cs | 25 | L1 |
| 36 | QuickFiler/Viewers/WebView2BreadcrumbHost.cs | 38 | L1 |
| 37 | QuickFiler/packages.config | 77 | L4 |
| 38 | QuickFiler/packages.config | 78 | L4 |
| 39 | QuickFiler/packages.config | 79 | L4 |

The enumerated row count is 39, one row per printed line.

## Class membership and the summation

- **L1: 26** — rows 1, 2, 3, 5, 6, 7, 8, 9, 10, 11, 12, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 32, 33,
  34, 35 and 36. Every one is the identical text
  `System.Reflection.MethodBase.GetCurrentMethod().DeclaringType`, the argument the log4net
  logger-declaration idiom passes to `LogManager.GetLogger`, one per logging class. Two files carry two
  each — QfcDatamodel.cs at lines 29 and 98, and QfcFormController.cs at lines 22 and 68 — because each
  declares two logging types.
- **L2: 3** — rows 4, 25 and 27: `using System.Reflection;` in EfcHomeController.cs, in
  Legacy/QfcController.cs, and in Properties/AssemblyInfo.cs. The AssemblyInfo.cs occurrence sits on
  line 1 behind a byte-order mark, which the printed output shows before the `using` keyword.
- **L3: 3** — rows 13, 15 and 26. Row 13 is commented-out code that would have passed
  `System.Reflection.Missing.Value` to an Outlook `GetItemFromID` call. Row 15 is a prose comment
  naming the namespace. Row 26 is a commented-out log4net logger declaration; it contains
  `MethodBase.GetCurrentMethod()` and would satisfy the L1 text test, but its first non-whitespace
  token is `//`, which the L1 test explicitly excludes, so the ordered tests place it in L3. This is the
  one row whose class depends on the ordering.
- **L4: 7** — rows 28 through 31 and 37 through 39. Rows 28 and 30 are `<Reference Include>` assembly
  references in the project file and in its tracked backup; rows 29 and 31 are the corresponding
  `<HintPath>` elements; rows 37, 38 and 39 are `<package id>` entries in the package manifest naming
  `System.Reflection`, `System.Reflection.Extensions` and `System.Reflection.Primitives`.
- **L5: 0** — no row is a call site taking a member-name argument. The class is empty.

Summation against the [P2-T1] production value:

```
L1 26 + L2 3 + L3 3 + L4 7 + L5 0 = 39
```

[P2-T1] printed `System.Reflection prod=39`. The five class counts sum to that value exactly, so every
occurrence of the printed population received exactly one class and none was left unassigned.

## Why L5: 0 is the operative finding

`MethodBase.GetCurrentMethod()` takes no member-name argument, a `using` directive resolves no member,
a comment is not compiled, and a project or package manifest entry names an assembly rather than a
member, so none of the occurrences in L1 through L4 can resolve a member of any type by name.

That sentence is what makes the non-zero production `System.Reflection` count compatible with the
sixteen `prod=0` rows of the [P2-T1] inventory rather than in tension with them. The production tree is
not free of the `System.Reflection` token; it is free of any construct that could turn a string into a
member. The distinction is recorded here rather than hidden behind an aggregate.

## Auditable-absence record for the L5 count

SearchScope: the 39 enumerated occurrences above, drawn from the tracked files matching the pathspec `QuickFiler/*`. [P2-T1] measured that scope as `QF_PROD_SCOPE_FILES=228` tracked files, so the search set is non-empty.

SearchPatterns: the fixed string `System.Reflection`, matched with `git grep -F`; then the five ordered class tests above applied to each printed line, with L5 defined as a call site taking a member-name argument.

SearchResult: none. No enumerated occurrence is a call site taking a member-name argument. The corroborating measurement is the sixteen `prod=0` rows of the [P2-T1] inventory, which searched for the call syntax of every name-resolving reflection API directly rather than for the namespace token.
