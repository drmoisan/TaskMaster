# 2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed (Spec)

- **Issue:** #637
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-29T12-20
- **Status:** Draft
- **Version:** 0.1

## Context
Issue #614 established the invariant that `SelectedFolderPath` is an archive-relative stem, and
created `ArchiveStemContract` to express it. The invariant is enforced at the filing boundary, where
`EmailFilerConfig.ResolvePaths` calls `RequireArchiveRelativeStem`, but it is not enforced at the
producer. `BreadcrumbBridgeRouter.SelectRow` still commits a rooted filing target verbatim when that
target is at or under the bound archive root, so a rooted value can still become
`SelectedFolderPath`. That is defect D1 half-closed: the store-root and cross-store leaks are
stopped, but rootedness as such still escapes the producer.

Two things make this worth fixing rather than tolerating.

First, it left a live trap that has already fired once. During #614 remediation cycle 1 the OK-path
guard was widened to accept rooted under-root values so that it would agree with `SelectRow`. Because
nothing between the guard and the filing boundary normalizes the value, the accepted value reached
`RequireArchiveRelativeStem` and threw. `ButtonOK_Click` is `async void` and rethrows, and
`ExecuteMovesAsync` wraps its core in try/finally with no catch, so the `ArgumentException` became an
unhandled UI-thread exception after the form had already been hidden. The re-audit caught it and the
change was reverted. The underlying asymmetry that made the widening look reasonable is still
present.

Second, the D8 normalizer is only half-wired. `EfcDataModel.ToArchiveRelativeStem` exists and is
correct, but it is called only from the `MAPIFolder` overload of `MoveToFolderAsync`. The `string`
overload assigns `DestinationOlStem = folderpath` verbatim, so it performs no normalization at all.
Any rooted value arriving through that overload depends entirely on the boundary throw.

The fix is to normalize at the producer: in `SelectRow`, when `TryMakeArchiveRelative` succeeds with
a non-empty stem, commit the stem rather than the rooted input; when it succeeds with an empty stem
the value is the archive root itself, which `SelectHierarchyPath` already treats as a deterministic
non-selection and `SelectRow` should too. Once the producer cannot emit a rooted value, the OK guard
and the filing boundary agree by construction rather than by coincidence, and the composition test
added during remediation keeps them agreeing.

This also requires updating the existing test that asserts a rooted input survives selection, so that
it asserts the stem instead. That is a deliberate spec correction of the same kind #614 already
applied twice, and should be recorded as such rather than treated as a weakened test.

Environment:
- OS/version: Windows 11 Pro 10.0.26200; .NET Framework 4.8.1 VSTO add-in.
- Python version: Not applicable; this is C#.
- Command/flags used: Static tracing during the issue #614 remediation re-audit, plus the failing
  path reproduced by remediation cycle 1.
- Data source or fixture: Repository source on the issue #614 branch.

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

No user-visible defect on the shipped code: a rooted selection is rejected at the OK guard with a
clear dialog. Severity is Medium because the asymmetry is a live trap for future changes, as
demonstrated by remediation cycle 1, and because the half-wired D8 normalizer means one filing
overload relies entirely on a throw.


## Repro & Evidence
Steps to Reproduce:
1. Bind breadcrumb rows with an archive root, and present a suggestion row whose filing target is a
   rooted path at or under that root. `FolderPredictor.ProjectSuggestionPath` strips the archive
   prefix only when the suggestion is strictly under it, so a suggestion whose folder is the archive
   root is returned rooted and verbatim.
2. Select that row. `BreadcrumbBridgeRouter.SelectRow` commits the rooted value to
   `SelectedFolderPath`.
3. Observe that the value reaching the filing boundary is rooted, and is rejected there by
   `RequireArchiveRelativeStem` rather than having been normalized at the producer.

Expected:
`SelectedFolderPath` is always an archive-relative stem. The producer normalizes; the boundary guard
is a backstop that never fires in normal operation. A row whose filing target is the archive root
itself is a non-selection, consistently with `SelectHierarchyPath`.

Actual:
`SelectRow` commits a rooted value verbatim. The invariant is enforced only at the boundary, where
violating it is an exception rather than a corrected value.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: not applicable; established by source tracing. See `BreadcrumbBridgeRouter.SelectRow`, the
  `string` overload of `EfcDataModel.MoveToFolderAsync`, and
  `ArchiveStemContract.RequireArchiveRelativeStem`.


## Scope & Non-Goals

Evidence base: `research/research.2026-08-29T12-30.md`, produced against this branch at
`ecdb1c84ba8541ab67042985919cfed4df768c01`. Every count and line citation reused below was
independently re-verified against the working tree while authoring this spec; the two corrections
found during that re-verification are recorded in "Corrections to the research file" under Root
Cause Analysis.

### In scope — four changes (A-D)

**A. Producer normalization in `SelectRow`.**
In `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`, bind the currently discarded
`out _` of `ArchiveStemContract.TryMakeArchiveRelative` (line 99), commit the stem when it is
non-empty, and treat an empty stem — the archive-root-exact case — as a deterministic
non-selection: an early `return` that leaves the prior selection untouched, raises no
`SelectedFolderPathChanged` event, and emits a value-free diagnostic, exactly as
`SelectHierarchyPath` already does at lines 119-126.

This change must remain **nested inside the existing `ArchiveStemContract.IsFullOutlookPath(selection)`
arm** of the guard at lines 96-100. A method-wide rewrite of the form "commit only when
`TryMakeArchiveRelative` succeeds" would reject every ordinary relative suggestion target and the
`Trash to Delete` pseudo-row, because `TryMakeArchiveRelative` returns `false` for both (verified:
the `StartsWith` test at ArchiveStemContract.cs:129-135 fails for a relative value). The existing
no-bound-root pass-through short-circuit — the first conjunct `_boundRoot.Length != 0` at line 97 —
is preserved unchanged.

**B. Normalization in the `string` overload of `MoveToFolderAsync`.**
In `QuickFiler/Controllers/EfcDataModel.cs`, the `string` overload (declared at :259-265) assigns
`DestinationOlStem = folderpath` verbatim at :287. The normalization is introduced behind a small,
pure, directly unit-testable `internal static` helper on `EfcDataModel` that takes the candidate
path and the archive ancestor and returns the value to assign; the assignment site at :287 calls
that helper.

The helper is **gated on `IsFullOutlookPath`**: a value that is not a full Outlook path — which
covers every ordinary relative stem and the `"Trash to Delete"` sentinel the same method branches
on at :272 — is returned verbatim and byte-identical.

A helper rather than an inline call is required for testability. The enclosing overload constructs a
real `EmailFiler` and awaits `SortAsync` (EfcDataModel.cs:293-294), so it cannot be driven
headlessly; a pure static helper can, through the existing `InternalsVisibleTo("QuickFiler.Test")`
seam that `EfcDataModelIssue614Tests` already uses.

**C. Test spec correction.**
The single existing assertion that pins the defect —
`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs:165`,
`router.SelectedFolderPath.Should().Be(fullTarget);` inside
`Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` (:118-166) — is
corrected to assert the stem `@"Clients\North"`. The test name and the arrange comment at :121-122
are narrowed so they no longer assert the superseded spec.

This is recorded as a **deliberate spec correction**, not a weakened test. The issue #439 criterion
that a rooted target survives selection is superseded by issue #614's archive-relative-stem
invariant, which #614 enforced on the `SelectHierarchyPath` half (Selection.cs:119-128) and at the
filing boundary (EmailFilerConfig.ResolvePaths) but not on the `SelectRow` half. The companion
assertion in the same test at :161-164 — `provider.Verify(p => p.ResolveLeafKeyAsync(fullTarget, ...))`
— pins a different and still-correct property, that the provider lookup uses the original rooted
path, and must be preserved.

**D. Stale-comment cleanup.**
Three locations record that producer-side normalization is deferred to issue #637 and become
inaccurate the moment this merges. Verified locations (re-derived, see the census below):

| # | Location | Current text |
|---|---|---|
| 1 | `QuickFiler/Controllers/EfcSelectionGuard.cs:30` | `/// normalization in BreadcrumbBridgeRouter.SelectRow is deferred to issue #637.` |
| 2 | `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:146` | `// RC-1 inversion: rooted values are never filing stems here; normalization is deferred to issue #637.` |
| 3 | `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:152` | the `because` string `"...and producer-side normalization is deferred to issue #637"` |

The guard-surface claims those strings make remain factually true — the guard still rejects rooted
values, and `EfcSelectionGuard` behavior is not changed by this work. Only the deferral wording is
stale and must be replaced with a statement that the producer now normalizes.

### Out of scope / non-goals — promoted to GitHub issue #695

1. **The `Globals.Ol.ArchiveRootPath` benign-degrade item** listed as the third checkbox in the
   issue's proposed-fix list, together with the unhandled keyboard entry points to `ActionOkAsync`
   and the half-completed form teardown on the button path.
2. **The verbatim `DestinationOlStem` assignments in `EfcDataModel.OpenOlFolderAsync` (:308) and
   `OpenFsFolderAsync` (:326).** These share the defect class of change B but are reached from
   different callers, are not part of the `MoveToFolderAsync` family, and are not required to close
   D1.

**Factual correction that drove exclusion 1.** Issue #637 asserts that an `InvalidOperationException`
from `ArchiveRootPath` "becomes an unhandled UI-thread exception" because `ButtonOK_Click` is
`async void` and rethrows. Research §10 establishes, and this spec's author re-verified at
EfcFormController.cs:460-475, that the premise is inaccurate against the current tree:

```csharp
460   public async void ButtonOK_Click(object sender, EventArgs e) => await ButtonOkClickAsync();
462   internal async Task ButtonOkClickAsync()
463   {
464       try { ... await ActionOkAsync(); }
471       catch (System.Exception ex)
473       { BoundaryErrorSink(ex.Message, ex); }
475   }
```

`ButtonOK_Click` delegates to `ButtonOkClickAsync`, which catches all exceptions into the injectable
`BoundaryErrorSink` (default at EfcFormController.cs:127-129), so on the button path the exception
is **logged, not unhandled**. The genuine defects on that chain are different in kind — a
half-completed teardown on the button path (`_formViewer.Hide()` at EfcFormController.cs:756 runs
before the throwing await at :759, while `Dispose()`/`Cleanup()` at :769-770 never run), and two
genuinely uncaught keyboard entry points (EfcFormController.cs:392 `KaKeyAsync(... Keys.Return, k =>
ActionOkAsync())`, and `KbdExecuteAsync(ActionOkAsync)` at :623 and :683, whose declarations at
:894-904 contain no try/catch). Each needs its own user-experience decision about what aborting a
filing operation should look like, and its own tests. Tracked in issue #695. Evidence: research §10.

### Explicitly excluded systems, integrations, and datasets

- `EfcSelectionGuard` predicate logic (`IsValidFilingSelection`, `IsValidCreationSelection`) —
  comment-only change; both predicates keep rejecting rooted values.
- `ArchiveStemContract` (UtilitiesCS) — no production change; it already implements the required
  semantics.
- `EmailFilerConfig.ResolvePaths` / `RequireArchiveRelativeStem` — the boundary backstop is
  unchanged; this work makes it stop firing rather than removing it.
- The `MAPIFolder` overload of `MoveToFolderAsync` (EfcDataModel.cs:336-357) and
  `EfcDataModel.ToArchiveRelativeStem` (:372-386) — unchanged.
- `EfcHomeController.ExecuteMovesAsync` / `ExecuteMovesCoreAsync` — no new catch clause; that is
  issue #695.
- `TaskMaster/AppGlobals/AppOlObjects.cs` and `ArchiveRootPathGuard.cs` — unchanged.
- The Family-B breadcrumb surface (`SelectRow(int index)` on `BreadcrumbStateModel`,
  `BreadcrumbSelectionSession`, `FolderBreadcrumbBridgeRouter`, `BreadcrumbBridgeCoordinator`) —
  an unrelated ItemViewer drop-down selector with no `SelectedFolderPath`, no `_boundRoot`, and no
  `ArchiveStemContract` reference. Not touched.
- The Outlook object model, Graph, and all filesystem/CSV outputs — no change.

## Root Cause Analysis

### The mechanism in `SelectRow`

`QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs:83-107` (verbatim, re-read against the
tree):

```csharp
 90            string selection =
 91                row.Kind == BreadcrumbRowKind.TrashPseudoRow
 92                    ? BreadcrumbRowBuilder.TrashRowText
 93                    : row.FilingTarget;
 94            // #614 D2: reject only an out-of-root FULL Outlook target; a rooted target at or
 95            // under the root passes verbatim (#439) and no bound root leaves the row unguarded.
 96            if (
 97                _boundRoot.Length != 0
 98                && ArchiveStemContract.IsFullOutlookPath(selection)
 99                && !ArchiveStemContract.TryMakeArchiveRelative(selection, _boundRoot, out _)
100            )
101            {
102                log.Error("Breadcrumb row rejected: target is outside the archive root.");
103                return;
104            }
105
106            CommitSelection(row, selection);
```

The guard is a **three-term conjunction, and the third term is negated**:

1. `_boundRoot.Length != 0` — the deliberate #439 pass-through mode when no archive root is bound.
2. `ArchiveStemContract.IsFullOutlookPath(selection)` — restricts the guard to rooted values.
3. `!ArchiveStemContract.TryMakeArchiveRelative(selection, _boundRoot, out _)` — **this is the term
   that lets a rooted at-or-under-root value through.** `TryMakeArchiveRelative` returns `true` for
   any path at or under the root, so its negation is `false` and the conjunction fails: the guard
   does not fire and control falls through to `CommitSelection(row, selection)` at line 106, which
   commits the **input**, not the stem. The stem is computed and then discarded into `out _`.

`CommitSelection` (:131-139) is the sole non-clearing write site: it sets `SelectedFolderPath =
selection` (:134) and raises `SelectedFolderPathChanged` (:138).

### Truth table of `TryMakeArchiveRelative` outcomes that matters for the fix

Derived from ArchiveStemContract.cs:106-145, re-read against the tree.

| `selection` shape | term 1 | `IsFullOutlookPath` | `TryMakeArchiveRelative` | `stem` | guard fires? | committed today | required after fix |
|---|---|---|---|---|---|---|---|
| Rooted, strictly under root | true | true | **true** (:143-144) | non-empty | no | rooted value, verbatim | **the stem** |
| Rooted, exactly equal to root | true | true | **true** (:124-127) | `string.Empty` | no | rooted root, verbatim | **deterministic non-selection** |
| Rooted, out of root / cross-store | true | true | false (:131) | `string.Empty` | **yes** | nothing (rejected) | unchanged — still rejected |
| Rooted, separator-boundary near miss (`\Archive2\...`) | true | true | false (:137-141) | `string.Empty` | **yes** | nothing (rejected) | unchanged — still rejected |
| Relative stem (`Clients\North`) | true | **false** | not evaluated (short-circuit) | n/a | no | relative value, verbatim | unchanged — verbatim |
| `Trash to Delete` pseudo-row | true | **false** | not evaluated | n/a | no | `Trash to Delete` | unchanged — byte-identical |
| Any value, no bound root | **false** | not evaluated | not evaluated | n/a | no | verbatim | unchanged — verbatim |

Two rows of this table are load-bearing in opposite directions. Row 2 returning `true` with an
empty stem is the exact mechanism by which the archive-root-exact value escapes the negated third
conjunct. Rows 5 and 6 returning `false` are the exact reason the fix must stay nested inside the
`IsFullOutlookPath` arm rather than becoming a method-wide rewrite.

The out-parameter contract that makes the fix safe: `TryMakeArchiveRelative` assigns
`stem = string.Empty` unconditionally on entry (ArchiveStemContract.cs:112), so `stem` is definitely
assigned on every exit path and is never the input value.

### The mechanism in the `string` overload of `MoveToFolderAsync`

`QuickFiler/Controllers/EfcDataModel.cs:282-291` builds the `EmailFilerConfig` with
`DestinationOlStem = folderpath` at :287 — verbatim, with no `ToArchiveRelativeStem` call and no
`ArchiveStemContract` call anywhere in the overload. The `MAPIFolder` overload (:336-357) does
normalize, at :345, and then delegates to the `string` overload at :346-352; `ToArchiveRelativeStem`
(:372-386) has exactly one caller. Any rooted value arriving directly at the `string` overload
therefore depends entirely on the boundary throw in `RequireArchiveRelativeStem`.

### Why the two halves are one defect

Issue #614 established the invariant, created `ArchiveStemContract`, enforced it at the boundary,
and enforced it on `SelectHierarchyPath` — but left the `SelectRow` producer and the `string` filing
overload unnormalized. The result is an asymmetry, not a user-visible fault: a rooted selection is
refused at the OK guard with a dialog. The asymmetry is nonetheless a live trap, demonstrated by
#614 remediation cycle 1, in which the OK guard was widened to agree with `SelectRow` and the
accepted value then threw at `RequireArchiveRelativeStem`. Closing the producer makes the guard and
the boundary agree by construction.

### Corrections to the research file

Two claims in `research/research.2026-08-29T12-30.md` did not survive re-verification. The tree
wins in both cases; this spec is written against the corrected facts.

1. **Research §11 states that no dedicated `EfcDataModelTests.cs` exists and calls it a coverage
   gap. That is wrong.** Both `QuickFiler.Test/Controllers/EfcDataModelTests.cs` (409 lines) and
   `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs` (123 lines) exist on this branch. The
   latter is a dedicated `ToArchiveRelativeStem` suite added by issue #614 with **8** `[TestMethod]`
   members (:21, :34, :48, :62, :72, :87, :100, :111), reaching the internal member through the
   assembly's existing `InternalsVisibleTo("QuickFiler.Test")`. It is the natural home for the
   change-B helper tests, and no new test infrastructure is required for change B.
2. **File-count arithmetic in two census headers.** Research §6 reports the `MoveToFolder` family as
   "16 matching lines across 6 files"; the tree gives 16 lines across **5** files (the line count
   agrees; the file count does not). Research §7 reports the `SelectedFolderPath` population as
   "9 files: 2 production, 7 test"; the tree gives 9 files split **3 production / 6 test** (§7's own
   later sentence, "both place all production occurrences in exactly 3 files", is the correct one).
   Neither correction changes any conclusion; both are recorded so no downstream artifact inherits
   the wrong figure.

### Correction to the issue's third premise

The `## Context` section above reproduces the issue narrative verbatim, including the claim that an
unhandled UI-thread exception arises because `ButtonOK_Click` is `async void` and rethrows. That
claim is inaccurate against the current tree; see the non-goals section for the verified evidence at
EfcFormController.cs:460-475 and the disposition in issue #695.


## Proposed Fix

### Design summary (what changes where):

Normalize at the producer, so that the OK guard and the filing boundary agree by construction
rather than by coincidence.

- **A.** `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` — inside the existing
  `IsFullOutlookPath` arm of `SelectRow`, bind the stem instead of discarding it into `out _`,
  commit the stem when non-empty, and return without touching the selection when the stem is empty.
- **B.** `QuickFiler/Controllers/EfcDataModel.cs` — add one `internal static` pure helper and call
  it from the `DestinationOlStem` assignment in the `string` overload of `MoveToFolderAsync`.
- **C.** `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` — correct one
  assertion, one test name, and one arrange comment.
- **D.** `QuickFiler/Controllers/EfcSelectionGuard.cs` and
  `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` — replace three stale deferral strings.

Changes A and B are independent. A alone closes D1; B alone makes the second filing overload
self-consistent. Both are required to make "a rooted value cannot reach the filing boundary" true
of the whole chain rather than of one branch of it.

### Boundaries and invariants to preserve:

These must **not** change. Each is separately asserted in the acceptance criteria.

1. **The no-bound-root pass-through mode (#439).** When `_boundRoot.Length == 0`, every value —
   including a rooted one — is committed verbatim. In `SelectRow` this is the first conjunct at
   Selection.cs:97; in `SelectHierarchyPath` it is the short-circuit at :111-115. Two existing tests
   pin it and must pass unmodified.
2. **The `Trash to Delete` sentinel value.** `BreadcrumbRowBuilder.TrashRowText` must remain
   byte-identical end to end. It is not `IsFullOutlookPath`, so it never enters the normalized arm
   in change A; and it must be returned verbatim by the change-B helper, because
   `EfcDataModel.MoveToFolderAsync` compares `folderpath != "Trash to Delete"` at :272 to decide
   whether attachments are saved. Any mutation of that value silently flips attachment behavior.
3. **The provider lookup using the original rooted path.** `ToHierarchyPath`
   (BreadcrumbBridgeRouter.cs:152-167) is not modified; the hierarchy provider continues to receive
   the presented full path. The `provider.Verify(... ResolveLeafKeyAsync(fullTarget ...))` assertion
   at `BreadcrumbBridgeRouterIssue439Tests.cs:161-164` stays exactly as written.
4. **The out-of-root rejection behavior.** A rooted target outside the bound root, and a
   separator-boundary near miss such as `\Archive2\Clients`, are still rejected with the existing
   value-free message `"Breadcrumb row rejected: target is outside the archive root."` and still
   leave the prior selection untouched.
5. **Value-free diagnostics.** No new log message may embed the path or the archive root. The
   existing test helper `AssertRejectionDiagnosticWithoutIdentifiers`
   (BreadcrumbBridgeRouterIssue614Tests.cs:310-326) asserts that no message containing the queried
   fragment contains `@`.
6. **Non-nulling rejection (#499).** A rejection is an early `return`. `SelectedFolderPath` is never
   set to `null` by a rejected selection, and `SelectedFolderPathChanged` is not raised. Only the
   clear-on-rebind path (BreadcrumbBridgeRouter.cs:143-146) nulls the property.
7. **`SelectHierarchyPath` is not modified.** Its four behaviors are the model for change A, not a
   refactoring target. Its three call sites are unaffected.
8. **The guard/boundary contract is not relaxed.** `EfcSelectionGuard` still rejects rooted values
   and `RequireArchiveRelativeStem` is still called at the boundary. This work removes the
   producer's ability to emit a rooted value; it does not remove any backstop.
9. **No new throw site.** The change-B helper must not introduce an exception path that the current
   verbatim assignment does not have. See "Error handling" below.

### Dependencies or blocked work:

- Depends on issue #614 (merged): `ArchiveStemContract`, the `SelectHierarchyPath` normalization,
  and the boundary guard already exist. No new contract type is introduced.
- Issue #695 owns the `ArchiveRootPath` benign degrade, the keyboard entry points, the button-path
  teardown, and the `OpenOlFolderAsync`/`OpenFsFolderAsync` assignments. This work must not
  pre-empt those decisions.
- No external service, package, or release dependency. No new NuGet package.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

| File | Change | Current size | Limit headroom |
|---|---|---|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | A — normalize inside the rooted arm of `SelectRow` | 209 lines | 291 |
| `QuickFiler/Controllers/EfcDataModel.cs` | B — new `internal static` helper + call at the `DestinationOlStem` assignment | 424 lines | 76 |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` | D — one XML-doc sentence | 79 lines | 421 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | C — one assertion, one name, one comment (substitutions only) | 694 lines (already over the 500-line limit) | none — must not grow |
| `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` | D — two rationale strings | in-place substitution | n/a |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs` | new file — router regression tests for A | new | 500 |
| `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs` | new tests for the change-B helper | 123 lines | 377 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `<Compile Include="Controllers\BreadcrumbBridgeRouterIssue637Tests.cs" />` | — | — |

`QuickFiler.Test.csproj` is a non-SDK-style net481 project with explicit `<Compile Include>` items
(for example `Controllers\EfcDataModelIssue614Tests.cs` at :114 and
`Controllers\BreadcrumbBridgeRouterIssue614Tests.cs` at :62). A new test file that is not registered
there does not compile into the assembly and its tests silently never run.

`EfcDataModel.cs` at 424 lines has 76 lines of headroom. The change-B helper plus its XML
documentation must fit inside that budget; if it does not, the helper moves to its own file rather
than the 500-line limit being exceeded.

#### Functions/classes/CLI commands impacted:

Selection family (Family A), the complete population — 2 declarations and 7 call sites, all
production, no interface declaration and no overload anywhere in the repository:

| Kind | Location | Impacted by this fix |
|---|---|---|
| declaration | `BreadcrumbBridgeRouter.Selection.cs:83` `private void SelectRow(BreadcrumbRow row)` | **modified (A)** |
| declaration | `BreadcrumbBridgeRouter.Selection.cs:109` `private void SelectHierarchyPath(BreadcrumbRow, string)` | not modified |
| call | BreadcrumbBridgeRouter.cs:201 — `SelectFirstRow()` | behavior changes for a rooted at-or-under-root row 0 |
| call | BreadcrumbBridgeRouter.cs:286 — `ProcessInboundAsync`, `rowSelected` arm | same condition |
| call | BreadcrumbBridgeRouter.Arrows.cs:153 — `HandleUpArrow` | same condition |
| call | BreadcrumbBridgeRouter.Arrows.cs:161 — `MoveSelection` (Down arrow) | same condition |
| call | `BreadcrumbBridgeRouter.Selection.cs:33` — `ActivateSegment` → `SelectHierarchyPath` | no change |
| call | `BreadcrumbBridgeRouter.Selection.cs:47` — `ActivateChild` → `SelectHierarchyPath` | no change |
| call | BreadcrumbBridgeRouter.Arrows.cs:138 — `TryRightTreeTransitionAsync` (#440) → `SelectHierarchyPath` | no change |

All four `SelectRow` call sites share one implementation, so the behavior change is uniform; there
is no per-call-site divergence to reason about. Two indirect entry points reach the family:
`_host.MessageReceived += OnHostMessageReceived` (BreadcrumbBridgeRouter.cs:55) and
`_router?.SelectFirstRow()` (EfcFormController.cs:438).

`MoveToFolder` family, the complete population — 3 declarations across 2 declaring types and 6 call
sites:

| Kind | Location | Impacted |
|---|---|---|
| declaration | `EfcDataModel.cs:259-265` — the `string` overload (M1) | **modified (B)** |
| declaration | `EfcDataModel.cs:336-343` — the `MAPIFolder` overload (M2) | not modified |
| declaration | EfcHomeController.ExecuteMoves.cs:89-95 — same-named forwarder / test seam, not an overload | not modified |
| call | EfcHomeController.ExecuteMoves.cs:78 | unchanged |
| call | EfcHomeController.ExecuteMoves.cs:98 | unchanged |
| call | `EfcDataModel.cs:346` — M2 delegating to M1 | unchanged; M2 still normalizes first via `ToArchiveRelativeStem` at :345 |
| call | EfcFormController.cs:537 | unchanged |
| call | EfcFormController.cs:844 | unchanged |
| call | QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs:87 | unchanged |

New member introduced: exactly one `internal static` helper on `EfcDataModel`. No public API is
added, removed, or changed anywhere in this work.

#### Data flow and validation changes:

Before (change A): `row.FilingTarget` → guard (rejects only out-of-root) → `CommitSelection(row,
selection)` → `SelectedFolderPath` = rooted value → `EfcFormController.SelectedFolder` → OK guard
refuses with a dialog.

After (change A): `row.FilingTarget` → guard (rejects out-of-root, unchanged) → inside the rooted
arm, `TryMakeArchiveRelative(selection, _boundRoot, out stem)`; if `stem.Length == 0`, log and
return with no state mutation; otherwise `CommitSelection(row, stem)` → `SelectedFolderPath` = stem
→ OK guard accepts → boundary `RequireArchiveRelativeStem` accepts.

Before (change B): `folderpath` → `DestinationOlStem = folderpath` (EfcDataModel.cs:287) →
`EmailFilerConfig.ResolvePaths` → `RequireArchiveRelativeStem` throws on a rooted value.

After (change B): `folderpath` → helper; if not `IsFullOutlookPath`, returned verbatim
(byte-identical, covering relative stems and `"Trash to Delete"`); if `IsFullOutlookPath` and
`TryMakeArchiveRelative` succeeds with a non-empty stem, the stem is returned; in every other case
the input is returned unchanged so the existing boundary guard decides, exactly as it does today.

No data migration, no persisted schema, no wire format, and no configuration key is affected. The
value flowing through is an in-process string.

#### Error handling and logging updates:

- **Change A** adds exactly one new diagnostic: a value-free `log.Error` on the archive-root-exact
  non-selection, in the same shape as Selection.cs:124. It must embed neither the selection nor the
  bound root. No new exception type, no new catch, no rethrow.
- **Change B** adds no exception path. The helper's contract is total: it returns a string for every
  input and never throws.
- **`ToArchiveRelativeStem` versus the change-B helper — the semantic choice, stated explicitly.**
  The two available semantics differ on the archive-root-exact input.
  `ArchiveStemContract.TryMakeArchiveRelative` returns `true` with an empty stem for that input
  (ArchiveStemContract.cs:124-127), whereas `EfcDataModel.ToArchiveRelativeStem` **throws**
  `ArgumentException` for it, because `RequireArchiveRelativeStem(stem, ...)` at
  `EfcDataModel.cs:384` rejects an empty value. That throw is pinned by
  `ToArchiveRelativeStem_ArchiveRootItself_Throws`
  (`QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs:62-69`).

  **The change-B helper adopts neither throw semantics: it is total and never throws.** For the
  archive-root-exact input, and for any rooted input that `TryMakeArchiveRelative` rejects, it
  returns the input verbatim and lets `RequireArchiveRelativeStem` at the filing boundary decide,
  which is precisely what happens today. Rationale:

  1. **Non-regression.** A throwing helper would convert an input that currently produces a
     redacted `ArgumentException` at the boundary into an `ArgumentException` raised earlier, from a
     different frame, on a call chain whose only handler is the button-path
     `BoundaryErrorSink`; the keyboard entry points have no handler at all. Change B would then have
     altered failure behavior on a path issue #695 explicitly owns.
  2. **Scope.** Deciding what an aborted filing operation should look like to the user is issue
     #695's work item, not this one.
  3. **Sufficiency.** Change B's purpose is to stop a *normalizable* rooted value from reaching the
     boundary unnormalized. Non-normalizable values are already handled correctly by the backstop.
  4. **Asymmetry with `ToArchiveRelativeStem` is correct.** That method's input is always a full
     Outlook `MAPIFolder.FolderPath` supplied by the create paths, so an unconditional throw is
     right there. The `string` overload's input is a *presented selection* that is normally already
     relative and may legitimately be the `"Trash to Delete"` sentinel; an unconditional
     `ToArchiveRelativeStem` call at :287 would throw on every ordinary filing operation.

  Net effect: the helper can only convert a value the boundary would have rejected into one it
  accepts. It can never turn an accepted value into a rejected one, and it never adds a throw.

#### Rollback/feature-flag considerations (if applicable):

No feature flag. The change is a small, self-contained behavior correction whose rollback is a
revert of the branch. A flag would reintroduce the exact producer/guard asymmetry this issue exists
to remove.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- `SelectRow(BreadcrumbRow row)` — unchanged signature, `private void`. Input: the row whose
  `FilingTarget` (or `BreadcrumbRowBuilder.TrashRowText` for the trash pseudo-row) is the candidate
  selection. Output: a mutation of `SelectedFolderPath` plus a render post and a
  `SelectedFolderPathChanged` event, or no observable effect at all.
- The change-B helper — `internal static string`, two `string` parameters (the candidate path and
  the archive ancestor), total function, no side effects, no I/O, no logging. Returns the value to
  assign to `DestinationOlStem`.
- `SelectedFolderPath` — `public string? { get; private set; }` (BreadcrumbBridgeRouter.cs:59).
  Type unchanged. The **value domain narrows**: after this work the only values the producer can
  emit are an archive-relative stem, the `Trash to Delete` sentinel, `null` (clear-on-rebind), or —
  in no-bound-root mode only — the presented value verbatim.

#### Required configuration keys and defaults:

None. No new configuration key, app setting, or default is introduced. The archive root continues to
be supplied to the router as the fourth argument of the internal `BindRowsAsync` overload
(BreadcrumbBridgeRouter.cs:92-97), which normalizes it with `TrimEnd('\\','/')` at :107-109 —
so a separator-only value such as `@"\"` yields `_boundRoot.Length == 0` and selects the
pass-through mode.

#### Backward-compatibility expectations:

- No public API signature changes. `SelectedFolderPath`'s setter is `private`, so no external caller
  can be broken by the narrowed value domain.
- The single cross-type consumer is `EfcFormController.SelectedFolder` (EfcFormController.cs:321),
  and every downstream reader either improves or is unaffected — see Data / API / Config Impact.
- Persisted artifacts: the QuickFile metrics CSV column (EfcHomeController.Metrics.cs:56) will carry
  a stem rather than a rooted path for the affected case. There are no in-repo readers of that CSV,
  and a stem is strictly less identifying than a rooted store path.

#### Performance constraints (latency/throughput/memory):

None beyond the existing budget. Change A binds an out-parameter that is already being computed and
discarded — no additional call, no additional allocation beyond the substring
`TryMakeArchiveRelative` already produces. Change B adds one predicate call and at most one
substring per filing operation, on a path that already awaits Outlook I/O. Both are on user-gesture
paths measured in single-digit microseconds.

## Assumptions, Constraints, Dependencies

- **Assumptions (environment, data, access):**
  - Windows 11, .NET Framework 4.8.1 VSTO add-in; `net481` test projects.
  - The archive root supplied to `BindRowsAsync` is a full Outlook path, and `_boundRoot` has
    already had trailing separators trimmed (BreadcrumbBridgeRouter.cs:107-109).
  - Path comparison is `OrdinalIgnoreCase` throughout `ArchiveStemContract`; both `\` and `/` are
    accepted as separators.
  - `QuickFiler.Test` can reach `internal` members of `QuickFiler` through the existing
    `InternalsVisibleTo("QuickFiler.Test")`; no assembly-attribute change is needed.
  - No Outlook process is available in the test environment; every new test must be headless.

- **Constraints (budget, performance, compatibility):**
  - No production, test, or reusable script file may exceed **500 lines**.
    `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` is already 694 lines and
    must not grow; `QuickFiler/Controllers/EfcDataModel.cs` is 424 lines and has 76 lines of
    headroom.
  - Nullable enforcement is per-file opt-in via `#nullable enable`.
    `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` carries it at line 1, so `CS86xx`
    diagnostics become build errors there under `/p:TreatWarningsAsErrors=true`;
    `QuickFiler/Controllers/EfcDataModel.cs` does **not** carry it and does not participate in
    nullable analysis. `SelectedFolderPath` is `string?` while `CommitSelection` takes a
    non-nullable `string`, so no nullable temporary may be introduced in change A.
  - Toolchain order is fixed and restarts from step 1 on any failure or file change:
    1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
    2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
    4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

    `/p:Nullable=enable` must not be added and `/t:Build` must not be substituted for `/t:Rebuild`.
  - Tests are MSTest with Moq and FluentAssertions. No temporary files, no wall-clock waits, no
    external dependencies.
  - Coverage: repository line coverage must stay at or above the policy floor, changed lines must
    not lose coverage, and the new helper must reach the new-code coverage target.

- **External dependencies (services, libraries, releases):** none. No new package. Existing test
  package versions in `QuickFiler.Test/packages.config` (FluentAssertions 8.10.0, Moq 4.20.72,
  MSTest 4.3.3, all `net481`) are sufficient.

## Data / API / Config Impact

- **User-facing or API changes:**

  No public API signature changes. One `internal static` helper is added to `EfcDataModel`. The
  user-visible change is that selecting a breadcrumb row whose filing target is a rooted path at or
  under the bound archive root now files successfully instead of being refused at OK with "Please
  select a valid folder."

  **Blast radius of `SelectedFolderPath` changing from a rooted value to a stem.** The complete
  production surface is 9 lines across 3 files: 2 write sites
  (`BreadcrumbBridgeRouter.Selection.cs:134` in `CommitSelection`, and BreadcrumbBridgeRouter.cs:145
  the #499 clear-on-rebind), 3 read sites (BreadcrumbBridgeRouter.cs:143 the #499 change-detection
  guard, `BreadcrumbBridgeRouter.Selection.cs:138` the event payload, and EfcFormController.cs:321
  the sole cross-type read), 1 property declaration (BreadcrumbBridgeRouter.cs:59), 1 doc reference
  (:61), and 2 event-only lines (:62, :146). Only the `CommitSelection` write is reached by
  `SelectRow`.

  Downstream consumers through `EfcFormController.SelectedFolder`, classified (research §7):

  | Consumer | Classification |
  |---|---|
  | `EfcFormController.IsValidSelection` → `EfcSelectionGuard.IsValidCreationSelection` | **Improves.** A rooted value is rejected at EfcSelectionGuard.cs:76, so the New-Folder gesture currently reports "Please select a valid folder" even for an in-archive folder. A stem passes. |
  | `EfcFormController.ActionOkAsync` → `EfcSelectionGuard.IsValidFilingSelection` | **Improves.** Rooted values are rejected at EfcSelectionGuard.cs:50, so a rooted-under-root breadcrumb selection is refused at OK today. A stem passes the guard and files. |
  | `ButtonCreateClickAsync` / `CreateFolderAsync` → `FolderHelper.CreateFolder(Async)` | **Improves.** These concatenate the selection beneath the archive root, so a stem is the correct input and a rooted value was never valid. |
  | `ActionOkAsync` → `OpenOlFolderAsync` / `OpenFsFolderAsync` → `DestinationOlStem` → `ResolvePaths` | **Improves.** A rooted value throws at `RequireArchiveRelativeStem`; a stem does not. (Their own verbatim assignments remain a non-goal, issue #695.) |
  | `EfcHomeController.ExecuteMovesCoreAsync` → the `string` overload → `DestinationOlStem` → `ResolvePaths` | **Improves.** This is the D1 leak the fix closes. |
  | `EfcHomeController.HandleMoveResult` failure text | Cosmetic improvement. The message embeds the value; a stem is strictly less identifying than a rooted store path. |
  | `EfcHomeController.QuickFileMetrics_WRITE` CSV column | Cosmetic. Zero in-repo readers. |
  | `EfcDataModel` trash-sentinel comparison at :272 | **No change.** `Trash to Delete` is not `IsFullOutlookPath`. |
  | Every consumer reached with an empty `_boundRoot` | **No change.** The pass-through mode is untouched. |
  | The #499 clear-on-rebind pair (BreadcrumbBridgeRouter.cs:143-145) | **No change.** Writes `null`, reads for null-ness only. |
  | `EfcItemController.SelectedFolder` | **No change.** Reads `_itemViewer.GetSelectedFolder()`, a different source. |
  | The Family-B breadcrumb surface | **No change.** It has no `SelectedFolderPath` member. |

  Summary of the classification: **every consumer that observes a change improves**, because a
  rooted selection is currently refused by the selection guard with a dialog. No consumer requires a
  rooted value. The single direction that could be called a behavior loss is the archive-root-exact
  case becoming a deterministic non-selection: today it produces a rooted value that
  `IsValidFilingSelection` rejects with a dialog; afterwards it produces no selection at all and the
  prior selection survives. Both outcomes refuse to file the archive root; the new one is quieter
  and matches `SelectHierarchyPath`.

- **Data or migration considerations:** none. No persisted state, schema, or stored path is
  affected. The QuickFile metrics CSV gains a shorter value in one column; it has no in-repo
  readers and no schema contract.

- **Logging/telemetry updates (if any):** one new value-free `log.Error` diagnostic for the
  archive-root-exact non-selection in `SelectRow`. No new logger, appender, or category. No message
  may embed a path, mailbox address, or the archive root.

- **Compatibility notes (CLI flags, config schemas, versioning):** none. No CLI surface, no config
  schema, no versioned contract. `QuickFiler.Test.csproj` gains one `<Compile Include>` item for the
  new test file.

## Test Strategy

Framework: **MSTest** + **Moq** + **FluentAssertions** (`QuickFiler.Test`, `net481`). No temporary
files, no Outlook process, no wall-clock waits.

Seeded from the issue, with dispositions (these are inputs, not acceptance criteria; the
authoritative criteria are under `## Acceptance Criteria` below):

- Normalize in `SelectRow` — **in scope, change A.**
- Wire normalization into the `string` overload of `MoveToFolderAsync` — **in scope, change B**, via
  a gated pure helper rather than a direct `ToArchiveRelativeStem` call.
- Benign degrade for the OK-path read of `ArchiveRootPath` — **out of scope; issue #695**, and its
  premise is corrected under Root Cause Analysis.
- Update the issue #439 rooted-target test to assert the stem — **in scope, change C**, recorded as
  a deliberate spec correction.
- Unit coverage areas: `SelectRow` for rooted under-root, rooted root-exact, rooted out-of-root,
  relative, and empty-bound-root inputs, plus the `string` overload's normalization — **in scope**;
  the unresolvable-archive-root degrade — out of scope.
- The #614 remediation composition test must still pass — **in scope as a preserved test.**
- Manual verification of the archive-root row — **in scope.**

### Fixture patterns to reuse (do not invent a new one)

Two established shapes exist; new tests must use one of them.

- **Shape 1 — per-test local construction.** Strict `Mock<IFolderHierarchyProvider>` and
  `Mock<IBreadcrumbWebHost>` built inline, bound through the **internal 4-argument** `BindRowsAsync`
  overload whose fourth positional argument is the archive root. Reference implementation:
  `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs:123-158`, with local helpers
  `Key(string)` (:668-671), `Chain(string, string, string)` (:673-687), and
  `Segment(string, string, bool)` (:689-692).
- **Shape 2 — `[TestInitialize]` fixture with a `BindChain` helper and a log4net `MemoryAppender`.**
  Required whenever a test asserts the rejection *diagnostic* rather than only the selection value.
  Reference implementation: BreadcrumbBridgeRouterIssue614Tests.cs — `Setup()` :38-52, `Cleanup()`
  :54-58, `BindStandardChain()` :224-234, `BindChain(...)` :236-262 (which passes `ArchiveRoot` as
  the fourth `BindRowsAsync` argument at :257), `Inbound(json)` :264-267, `RowSelected(int)`
  :288-291, `RenderedMessages()` :304-307, `AssertRejectionDiagnosticWithoutIdentifiers(fragment)`
  :310-326, appender attach/detach :338-356.

The `internal` 4-argument `BindRowsAsync` overload is already visible to `QuickFiler.Test`, so no
`InternalsVisibleTo` change is required. To produce `_boundRoot.Length == 0` in a test, pass
`string.Empty`, `null`, whitespace, **or a separator-only value** such as `@"\"` —
`BindRowsAsync` applies `TrimEnd('\\','/')` at BreadcrumbBridgeRouter.cs:107-109, which is exactly
what `BreadcrumbBridgeRouterIssue439Tests.cs:645` relies on.

### Destination files (500-line limit respected)

Verified current sizes: `BreadcrumbBridgeRouterIssue439Tests.cs` **694 lines** (already over the
limit), BreadcrumbBridgeRouterIssue614Tests.cs 358, `EfcDataModelIssue614Tests.cs` 123,
EfcDataModelTests.cs 409, `BreadcrumbBridgeRouter.Selection.cs` 209, `EfcDataModel.cs` 424.

| Tests | Destination | Reason |
|---|---|---|
| New change-A router regression tests | **`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs` (new file, Shape 2)** | `BreadcrumbBridgeRouterIssue439Tests.cs` is 694 lines and must not grow; BreadcrumbBridgeRouterIssue614Tests.cs at 358 lines cannot absorb a full new suite plus fixture without approaching the limit. The new file must be registered in `QuickFiler.Test/QuickFiler.Test.csproj` with a `<Compile Include>` item. |
| New change-B helper tests | **`QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`** | It is the existing dedicated suite for the sibling normalizer `ToArchiveRelativeStem`, has 377 lines of headroom, and already reaches the internal member through `InternalsVisibleTo("QuickFiler.Test")`. Its 8 existing test methods stay unchanged. |
| Corrected assertion (change C) | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | Substitution only — one assertion, one method name, one comment. Net line count must not increase. |
| Stale rationale strings (change D) | `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` | Substitution only. |

### Regression tests to add or update

**Change A — `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs` (new):**

1. `RowSelected_ArchiveRootExactFilingTarget_IsNotSelected` — bind a row whose `FilingTarget` equals
   the bound archive root; assert `SelectedFolderPath` is unchanged from its prior value, that no
   `SelectedFolderPathChanged` event is raised, and that a value-free rejection diagnostic was
   logged (`AssertRejectionDiagnosticWithoutIdentifiers`).
2. `RowSelected_ArchiveRootExactFilingTarget_PreservesAPriorValidSelection` — establish a valid stem
   selection first, then select the archive-root row; assert the prior stem survives and is never
   nulled (#499).
3. `RowSelected_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem` — assert
   `SelectedFolderPath` is the stem, not the rooted input.
4. `RowSelected_RootedTargetUnderArchiveRoot_CaseInsensitiveAndTrailingSeparatorRoot_CommitsTheStem`
   — root with differing case and a trailing separator; assert the same stem.
5. `RowSelected_RelativeFilingTarget_CommitsTheValueVerbatim` — assert byte-identical commit of an
   ordinary relative suggestion target (the regression a method-wide rewrite would cause).
6. `RowSelected_TrashPseudoRow_CommitsTheSentinelVerbatim` — assert the committed value is exactly
   `Trash to Delete`.
7. `RowSelected_OutOfRootRootedTarget_IsStillRejected` and
   `RowSelected_SeparatorBoundaryNearMissTarget_IsStillRejected` (`\Archive2\Clients` against
   `\Archive`) — assert the pre-existing rejection message and no state mutation.
8. `RowSelected_RootedTargetWithNoBoundArchiveRoot_PassesThroughVerbatim` — bind with `@"\"`;
   assert the rooted value is committed verbatim.
9. `SelectFirstRow_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem` — proves the
   normalization is on the shared implementation and therefore reaches all four `SelectRow` call
   sites, not only the `rowSelected` inbound message.

**Change B — `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs` (added to the existing
class or a sibling class in the same file):**

1. Rooted path strictly under the ancestor returns the stem.
2. Rooted path under an ancestor differing in case returns the stem.
3. Relative stem (`Clients\North`) is returned **verbatim and byte-identical**.
4. `"Trash to Delete"` is returned verbatim and byte-identical.
5. Archive-root-exact input is returned verbatim and **does not throw** (the explicit divergence
   from `ToArchiveRelativeStem_ArchiveRootItself_Throws`).
6. Out-of-root / cross-store rooted input is returned verbatim and does not throw.
7. Null, empty, and whitespace ancestor, and a separator-only ancestor, return the input verbatim
   and do not throw.
8. Null or empty candidate path returns the input verbatim and does not throw.

**Change C — corrected assertion:** in
`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`, line 165 becomes
`router.SelectedFolderPath.Should().Be(@"Clients\North");`. Derivation of the expected value: with
`archiveRoot = @"\Archive"` (:123) and `fullTarget = @"\aRcHiVe\Clients\North"` (:124),
`TryMakeArchiveRelative` matches through `StartsWith(OrdinalIgnoreCase)` (ArchiveStemContract.cs:131),
the boundary character at index 8 is `\` (:137-141), and
`stem = fullTarget.Substring(8).TrimStart('\\','/')` = `Clients\North`. The method is renamed so it
no longer claims the target "RemainsUnchanged", and the arrange comment at :121-122 is narrowed to
the provider claim it still supports. The `provider.Verify(...)` assertion at :161-164 is preserved
verbatim.

### Existing tests that must pass unmodified

- The two no-bound-root pass-through assertions:
  `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs:665`
  (`Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection`, bound with `@"\"` at :645) and
  BreadcrumbBridgeRouterIssue614Tests.cs:221
  (`SegmentActivate_WithNoBoundArchiveRoot_PreservesThePassThroughMode`, 3-argument bind at :213).
  Both reach `SelectHierarchyPath` with an empty `_boundRoot` and are unaffected.
- `RowSelected_OutOfRootFilingTarget_DoesNotStoreAFullOutlookPath`
  (BreadcrumbBridgeRouterIssue614Tests.cs:169).
- The composition test `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary`
  (`QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:167-213`), which filters a 10-element
  candidate matrix through `IsValidFilingSelection` and asserts `EmailFilerConfig.ResolvePaths()`
  does not throw for anything the guard accepts. It exercises no router, so it is structurally
  untouched; the fix makes it more meaningful, because the producer now emits only values in the
  accepted class.
- The 8 `ToArchiveRelativeStem` tests in `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`,
  including `ToArchiveRelativeStem_ArchiveRootItself_Throws` (:62-69).
- All `ArchiveStemContractTests` in `UtilitiesCS.Test`.

### Edge cases and negative scenarios

Covered by the lists above: archive-root-exact (`TryMakeArchiveRelative` returns `true` with an
empty stem); trailing-separator and differing-case roots; forward-slash boundary; the `Archive2`
separator-boundary near miss; out-of-root and cross-store values; already-relative values; the
trash sentinel; null / empty / whitespace / separator-only archive root; null and empty candidate
path; and a prior valid selection surviving a rejected one.

### Error handling and logging verification

- The new archive-root-exact diagnostic must be asserted **value-free**: reuse
  `AssertRejectionDiagnosticWithoutIdentifiers`, which requires that no message containing the
  queried fragment contains `@`.
- Assert that a rejection raises no `SelectedFolderPathChanged` event and performs no write.
- Assert that the change-B helper never throws for any input in the matrix above, including inputs
  for which `ToArchiveRelativeStem` throws.

### Coverage impact and targets for changed lines/modules

- The change-B helper is a new method and must meet the new-code coverage target, with every branch
  of its `IsFullOutlookPath` / `TryMakeArchiveRelative` gate exercised.
- Change A adds one branch inside an existing method; both its outcomes (empty stem, non-empty stem)
  are covered by the new tests.
- Repository line coverage must not fall below the policy floor, and no changed line may lose
  coverage relative to the base commit.
- Coverage evidence is produced by step 4 of the toolchain and written under the canonical evidence
  kinds defined by `evidence-and-timestamp-conventions`: the pre-change capture under
  `<FEATURE>/evidence/baseline/` and the post-change capture under `<FEATURE>/evidence/qa-gates/`.
  `<FEATURE>/evidence/coverage/` is not a canonical evidence kind and must not be used.

### Toolchain commands to run (format → lint → type-check → test)

1. `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Restart from step 1 on any failure or any file the tools change. Do not add `/p:Nullable=enable`;
do not substitute `/t:Build` for `/t:Rebuild`.

### Manual validation steps (if required)

1. Bind breadcrumb rows with a real archive root and present a suggestion row whose filing target is
   the archive root itself. Select it. Confirm it is a deterministic non-selection: the prior
   selection is still shown, no error dialog appears, and a value-free entry is written to the log.
2. Present a suggestion row whose filing target is rooted and strictly under the archive root.
   Select it, then press OK. Confirm the item files successfully rather than being refused with
   "Please select a valid folder."
3. Select the `Trash to Delete` pseudo-row and confirm the trash behavior, including attachment
   handling, is unchanged.


## Acceptance Criteria

Every number appearing below was re-derived against the working tree while authoring this spec,
using a search strategy independently constructed from the one in the research file, and the two
results agreed. Where they did not agree, the tree value is used and the disagreement is recorded
under "Corrections to the research file".

### Change A — producer normalization in `SelectRow`

- [x] AC1. Selecting a row whose `FilingTarget` is a full Outlook path **exactly equal** to the
      bound archive root is a deterministic non-selection: `SelectRow` returns early,
      `SelectedFolderPath` is not written, `SelectedFolderPathChanged` is not raised, and any prior
      valid selection survives unchanged. Verified by a named test in
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs`.
- [x] AC2. Selecting a row whose `FilingTarget` is a full Outlook path **strictly under** the bound
      archive root commits the archive-relative stem, not the rooted input — including when the
      root differs in case and when the root carries a trailing separator. Verified by named tests
      in `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs`.
- [x] AC3. The new behavior is nested inside the existing
      `ArchiveStemContract.IsFullOutlookPath(selection)` arm of the guard in
      `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`. An ordinary **relative**
      suggestion target is still committed byte-identically, proven by a test that would fail under
      a method-wide "commit only when `TryMakeArchiveRelative` succeeds" rewrite.
- [x] AC4. The no-bound-root pass-through mode is preserved: with `_boundRoot.Length == 0` every
      value, including a rooted one, is committed verbatim. The **2** existing tests that pin this
      pass through unmodified — `BreadcrumbBridgeRouterIssue439Tests.cs:665` (bound with `@"\"` at
      :645) and BreadcrumbBridgeRouterIssue614Tests.cs:221 (3-argument bind at :213) — and a new
      test covers the `SelectRow` path in the same mode.
- [x] AC5. The `Trash to Delete` pseudo-row still commits `BreadcrumbRowBuilder.TrashRowText`
      byte-identically, and `EfcDataModel.MoveToFolderAsync`'s `folderpath != "Trash to Delete"`
      comparison at `EfcDataModel.cs:272` continues to take the same branch as before.
- [x] AC6. Out-of-root rejection is unchanged: a rooted target outside the bound root, and a
      separator-boundary near miss such as `\Archive2\Clients`, are still rejected with the existing
      message `"Breadcrumb row rejected: target is outside the archive root."`, leave the prior
      selection untouched, and raise no event.
      `RowSelected_OutOfRootFilingTarget_DoesNotStoreAFullOutlookPath`
      (BreadcrumbBridgeRouterIssue614Tests.cs:169) passes unmodified.
- [x] AC7. The archive-root-exact non-selection emits a **value-free** diagnostic that embeds
      neither the selection nor the archive root, asserted with the existing
      `AssertRejectionDiagnosticWithoutIdentifiers` helper shape (no message containing the queried
      fragment contains `@`).
- [x] AC8. `SelectHierarchyPath` (`BreadcrumbBridgeRouter.Selection.cs:109-129`) and
      `CommitSelection` (:131-139) are not modified.
- [x] AC9. The selection family is unchanged in shape: still exactly **2** declarations
      (`BreadcrumbBridgeRouter.Selection.cs:83` and `:109`) and **7** call sites (4 to `SelectRow`
      at BreadcrumbBridgeRouter.cs:201, :286 and BreadcrumbBridgeRouter.Arrows.cs:153, :161; 3 to
      `SelectHierarchyPath` at `BreadcrumbBridgeRouter.Selection.cs:33`, `:47` and
      BreadcrumbBridgeRouter.Arrows.cs:138). No new declaration, no overload, no interface member,
      and no new call site is introduced, and no member of the unrelated Family-B `SelectRow(int)`
      surface is touched.
- [x] AC10. The normalization is on the shared implementation, so all four `SelectRow` call sites
      observe it. Proven by at least one regression test that reaches `SelectRow` through
      `SelectFirstRow()` rather than through the `rowSelected` inbound message.

### Change B — normalization in the `string` overload of `MoveToFolderAsync`

- [x] AC11. `QuickFiler/Controllers/EfcDataModel.FilingStem.cs`, a new partial-class file of
      `EfcDataModel`, declares exactly one new `internal static`
      helper that takes the candidate path and the archive ancestor and returns the value to assign,
      and the `DestinationOlStem` assignment in the `string` overload (currently
      `EfcDataModel.cs:337`) calls it. The member is therefore `EfcDataModel.ToFilingStemOrVerbatim`,
      unchanged by the file split. The helper is pure: no I/O, no logging, no static mutable
      state, and it is invoked directly by unit tests without constructing an `EmailFiler`.
- [x] AC12. The helper is gated on `ArchiveStemContract.IsFullOutlookPath`. Any value that is not a
      full Outlook path — every ordinary relative stem and the `"Trash to Delete"` sentinel — is
      returned **verbatim and byte-identical**, asserted by named tests.
- [x] AC13. For a rooted value at or strictly under the ancestor, the helper returns the
      archive-relative stem.
- [x] AC14. The helper is **total and never throws**, for any input including archive-root-exact,
      out-of-root, cross-store, null/empty candidate, and null/empty/whitespace/separator-only
      ancestor; in each of those cases it returns the input verbatim so the existing boundary guard
      decides exactly as it does today. This is a deliberate divergence from
      `EfcDataModel.ToArchiveRelativeStem`, which throws on the archive-root-exact input; the
      rationale is recorded under "Error handling and logging updates".
- [x] AC15. `EfcDataModel.ToArchiveRelativeStem` (`EfcDataModel.cs:421-448`, declaration at `:434`),
      the `MAPIFolder`
      overload (`:398-419`), and its call to `ToArchiveRelativeStem` at `:407` are unmodified, and
      the **8** existing `ToArchiveRelativeStem` tests in
      `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs` (methods at :21, :34, :48, :62,
      :72, :87, :100, :111) pass unchanged — including
      `ToArchiveRelativeStem_ArchiveRootItself_Throws`.
- [x] AC16. The `MoveToFolder` family is unchanged in shape apart from the helper call: still
      **3** declarations (`EfcDataModel.cs:303`, `EfcDataModel.cs:398`, and the same-named forwarder
      at EfcHomeController.ExecuteMoves.cs:89) and **7** call sites (EfcHomeController.ExecuteMoves.cs:78
      and :98, `EfcDataModel.cs:408`, EfcFormController.cs:537 and :844,
      QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs:87, and
      QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs:314). The family-stem search over
      `*.cs` returns **23** lines across **6** files, and the syntax-anchored search returns **10**
      lines across **5** files. No new overload and no signature change.
- [x] AC17. `EfcDataModel.OpenOlFolderAsync` (`:349-372`) and `OpenFsFolderAsync` (`:374-396`) are
      **not** modified, and the guarded `Globals.Ol.ArchiveRootPath` read at `EfcDataModel.cs:284`
      together with the `UserDiagnosticAction(ArchiveRootUnavailableMessage)` degrade at `:358` and
      `:382`, both introduced by issue #638, are preserved unchanged. The remaining benign-degrade
      work is a non-goal owned by issue #695.

### Change C — test spec correction

- [x] AC18. `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs:165` asserts
      `router.SelectedFolderPath.Should().Be(@"Clients\North");`, the enclosing test method is
      renamed so it no longer asserts that a rooted target "RemainsUnchanged", and the arrange
      comment at :121-122 is narrowed to the provider claim it still supports.
- [x] AC19. The companion assertion at `BreadcrumbBridgeRouterIssue439Tests.cs:161-164`
      (`provider.Verify(p => p.ResolveLeafKeyAsync(fullTarget, ...), Times.Once)`) is preserved
      verbatim, and `ToHierarchyPath` (BreadcrumbBridgeRouter.cs:152-167) is unmodified, so the
      provider lookup still uses the original rooted path.
- [x] AC20. Exactly **1** existing test assertion changes its expected value across the entire
      repository — the one at `BreadcrumbBridgeRouterIssue439Tests.cs:165`. No other existing
      assertion in any test project is modified, weakened, disabled, or deleted.
- [x] AC21. The change is recorded in the change description as a **deliberate spec correction**:
      the issue #439 criterion that a rooted target survives selection is superseded by issue #614's
      archive-relative-stem invariant, which #614 enforced on the `SelectHierarchyPath` half and at
      the filing boundary but not on the `SelectRow` half. It is explicitly not a weakened test.

### Change D — stale-comment cleanup

- [x] AC22. All **3** stale deferral records are corrected to state that producer-side normalization
      is implemented: `QuickFiler/Controllers/EfcSelectionGuard.cs:30`,
      `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:146`, and the `because` string at
      `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:152`. After the change, a repository
      grep for `deferred to issue #637` across `*.cs` returns **0** matches.
- [x] AC23. `EfcSelectionGuard` behavior is unchanged — `IsValidFilingSelection` and
      `IsValidCreationSelection` still reject rooted values — and every test in
      `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` passes, including the composition test
      `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary` (:167-213).

### Cross-cutting

- [x] AC24. The `SelectedFolderPath` production surface is unchanged in shape: still **9** lines
      across **3** files, with **2** write sites (`BreadcrumbBridgeRouter.Selection.cs:134`,
      BreadcrumbBridgeRouter.cs:145) and **3** read sites (BreadcrumbBridgeRouter.cs:143,
      `BreadcrumbBridgeRouter.Selection.cs:138`, EfcFormController.cs:321). No new write site, no
      new public API member, and the property's `private set` is preserved.
- [x] AC25. File-size limits hold. `QuickFiler/Controllers/EfcDataModel.cs` remains at or under 500
      lines (485 before the change, leaving 15 lines of headroom, which is why the change-B helper is
      declared in its own partial-class file as this document's implementation section authorizes);
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` does not grow beyond its
      current 694 lines; and every new or modified file, including
      `QuickFiler/Controllers/EfcDataModel.FilingStem.cs`,
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs` and
      `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`, is at or under 500 lines.
- [x] AC26. `QuickFiler.Test/QuickFiler.Test.csproj` contains a `<Compile Include>` item for
      `Controllers\BreadcrumbBridgeRouterIssue637Tests.cs`, and the new tests are observed executing
      in the vstest run output (a test file absent from this non-SDK project compiles into nothing
      and silently never runs).
- [x] AC27. Nullable posture is respected: `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`
      keeps its `#nullable enable` directive, the edited lines introduce no `CS86xx` diagnostic under
      `/p:TreatWarningsAsErrors=true`, and no nullable temporary is passed to `CommitSelection`,
      whose parameter is a non-nullable `string`.
- [x] AC28. Full C# toolchain pass completed in order with no failures in the final pass, using
      exactly these commands: `dotnet tool run csharpier format .` (verified with
      `dotnet tool run csharpier check .`);
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`;
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`;
      `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. `/p:Nullable=enable` was not
      added and `/t:Build` was not substituted.
- [x] AC29. Coverage evidence is captured under the canonical evidence kinds — the pre-change
      capture under `<FEATURE>/evidence/baseline/` and the post-change capture under
      `<FEATURE>/evidence/qa-gates/`, per `evidence-and-timestamp-conventions`: repository line
      coverage is at or above the policy floor, no changed line loses coverage relative to the base
      commit, and the new change-B helper meets the new-code coverage target with both sides of its
      gate exercised.
- [x] AC30. No behavior outside changes A-D is altered. Specifically unchanged:
      `ArchiveStemContract`, `EmailFilerConfig.ResolvePaths` / `RequireArchiveRelativeStem`,
      `EfcHomeController.ExecuteMovesAsync`, EfcFormController.cs, the Family-B breadcrumb surface,
      and all `UtilitiesCS.Test` suites.

## Risks & Mitigations

- **Technical or operational risks:**
  1. *Over-broad normalization in `SelectRow`.* Applying the new rule outside the
     `IsFullOutlookPath` arm would reject every ordinary relative suggestion target and the trash
     pseudo-row, because `TryMakeArchiveRelative` returns `false` for both. This is the single
     highest-severity failure mode of change A.
  2. *A throwing change-B helper.* Adopting `ToArchiveRelativeStem`'s throw semantics would add a
     new exception path on the OK filing chain, whose only handler is the button-path
     `BoundaryErrorSink` and whose keyboard entry points have none — pre-empting issue #695.
  3. *Silent mutation of the trash sentinel.* Any normalization applied upstream of the
     `folderpath != "Trash to Delete"` comparison at `EfcDataModel.cs:272` would silently flip
     attachment-saving behavior.
  4. *Perception of a weakened test.* Change C changes an assertion that a prior issue deliberately
     added; without the recorded rationale a reviewer could read it as coverage loss.
  5. *A new test file that never runs.* `QuickFiler.Test.csproj` is a non-SDK project with explicit
     `<Compile Include>` items; an unregistered file compiles into nothing and reports no failures.
  6. *Test file line-limit pressure.* The most natural home for the change-A tests is already 694
     lines, over the 500-line limit.

- **Mitigations and rollbacks:**
  1. AC3 requires a byte-identical relative-target commit test that fails under the over-broad
     rewrite; AC5 requires the same for the trash sentinel.
  2. AC14 fixes the helper's total, non-throwing contract and requires tests for every input class,
     including those on which `ToArchiveRelativeStem` throws.
  3. AC5 pins the sentinel through both the router and the filing comparison.
  4. AC21 requires the spec correction to be recorded explicitly in the change description, with the
     superseding-invariant rationale.
  5. AC26 requires both the `<Compile Include>` item and observed execution in the vstest output.
  6. AC25 pins the current sizes and routes new router tests to a new file.
  - Rollback is a revert of the branch. No feature flag, no data migration, and no persisted state
    means a revert restores the prior behavior exactly.

## Rollout & Follow-up

- **Release/rollout steps:** ordinary branch → PR → merge. The change ships inside the VSTO add-in
  build; there is no separate deployment step, configuration change, or migration. No user
  communication is required beyond the change description, since the observable effect is that a
  previously refused in-archive selection now files.
- **Post-fix monitoring or clean-up tasks:**
  - Confirm after merge that no log entry from the new archive-root-exact diagnostic contains a path
    or mailbox address.
  - Issue #695 remains open for the `ArchiveRootPath` benign degrade, the two uncaught keyboard
    entry points, the half-completed button-path teardown, and the verbatim `DestinationOlStem`
    assignments in `OpenOlFolderAsync` / `OpenFsFolderAsync`.
  - Once #695 lands, re-evaluate whether the change-B helper should become the single normalization
    funnel for all three `EmailFilerConfig` construction sites in `EfcDataModel`.
- **Links:**
  - Issue: https://github.com/drmoisan/TaskMaster/issues/637
  - Parent invariant: issue #614 (`ArchiveStemContract`, the boundary guard, and the
    `SelectHierarchyPath` normalization), plus its remediation cycle 1 revert.
  - Prior producer spec: issue #439 (rooted-target pass-through), superseded in part by change C.
  - Follow-up: issue #695 (excluded surfaces and the OK-chain error handling).
  - Research: `research/research.2026-08-29T12-30.md`.
