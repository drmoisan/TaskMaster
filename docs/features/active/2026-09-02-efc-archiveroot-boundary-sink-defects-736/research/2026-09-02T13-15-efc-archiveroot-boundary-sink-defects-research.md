# Research — efc-archiveroot-boundary-sink-defects (Issue #736)

- **Issue:** #736
- **Branch:** `bug/efc-archiveroot-boundary-sink-defects-736` (cut from `origin/main`)
- **Workspace root:** `<repo-root>/.claude/worktrees/agent-a9f3f171e35df71ef`
- **Artifact timestamp:** 2026-09-02T13-15
- **Scope of this item:** findings 1, 2, 4, 5, 6. Finding 3 (`ActionOkAsync` hide-before-dispose
  ordering) is **owned by a different item in this parallel run** and is out of scope here.

---

## 0. Method and tool constraints (read this before trusting any count below)

**The `Bash` tool is disabled in this session** ("Error: No such tool available: Bash. Bash is
disabled for this session, in subagents as well as here."). Two direct consequences:

1. **`gh issue view 736` / `gh issue view 699` could not be executed.** The issue text used here is
   the promotion-lifecycle copy checked into the repository, which the promotion tooling maps
   verbatim into the GitHub issue body:
   - #736: `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/issue.md`
     (identical prose also at `docs/features/potential/promoted/2026-09-02-efc-archiveroot-boundary-sink-defects.md`).
   - #699: `docs/features/potential/promoted/2026-08-29-efcdatamodel-success-path-test-uses-incidental-crash-as-barrier.md`.
   The GitHub-side closure state of #699 ("closed as NOT_PLANNED, superseded by #736") is **reported
   by the orchestrator and not independently verified in this session.** Everything else about #699
   below is verified against the checked-in body.
2. **`wc -l` could not be executed.** Line counts below are derived from the `Read` tool, which
   reports a file's line array length and renders a trailing empty element for a file ending in a
   newline. Derivation is stated per file so a later reader can re-derive with `wc -l`.

| File | Last content line (Read) | Derived `wc -l` | Evidence |
|---|---|---|---|
| `QuickFiler/Controllers/EfcFormController.cs` | 1216 (`}`) | **1216** | `Read(offset=1214)` renders 1214–1216 then an empty 1217 |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 494 (`}`) | **494** | `Read(offset=600)` returned "The file has 495 lines"; `Read(offset=488)` shows 493–494 then empty 495 |

The issue's own line citations are stale because #726 landed unrelated changes to
`EfcFormController.cs` after the sweep. **A stale scope note claiming 1189 lines is wrong; the file
is 1216 lines.** Every citation in this document is at the **current worktree HEAD** line number.

---

## 1. Current state analysis

### 1.1 The archive-root resolution chain (`TaskMaster/AppGlobals/AppOlObjects.cs`)

`AppOlObjects` is `public partial class AppOlObjects : IOlObjects, IDisposable` (`:26`), so new
members can be added in a sibling partial file. There is already precedent for that:
`TaskMaster/AppGlobals/AppOlObjects.StoreRehook.cs`.

The getter under finding 1, at `AppOlObjects.cs:257-271`:

```csharp
public string ArchiveRootPath
{
    get
    {
        if (_archiveRootPath is null)
        {
            _archiveRootPath = ArchiveRootPathGuard.RequireResolvedArchiveRoot(
                Path.Combine(Root.FolderPath, "Archive"),
                ArchiveRoot?.FolderPath,
                message => logger.Error(message)
            );
        }
        return _archiveRootPath;
    }
}
```

Both arguments are evaluated **before** `RequireResolvedArchiveRoot` is entered. What each one
actually executes across the live COM boundary:

- `Root.FolderPath` — `Root` (`:206-214`) is itself a lazy COM read:
  `_root = (Folder)App.Session.DefaultStore.GetRootFolder();`. So the argument performs
  `App.Session`, `.DefaultStore`, `.GetRootFolder()`, then `.FolderPath` — four COM crossings on a
  cold cache.
- `ArchiveRoot?.FolderPath` — `ArchiveRoot` (`:274`) is
  `Initializer.GetOrLoad(ref _archiveRoot, LoadArchiveRoot)`. `LoadArchiveRoot()` (`:276-280`) is:
  ```csharp
  var folderHandler = new FolderPredictor(_globals);
  return folderHandler.GetFolder(Root.Folders, "Archive");
  ```
  `new FolderPredictor(IApplicationGlobals)` (`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:35-40`)
  reads `AppGlobals.Ol.App`. `GetFolder(Folders, string)` (`FolderPredictor.cs:522-533`) executes
  `children.Cast<MAPIFolder>().Select(x => x.Name).ToList()` — a full enumeration of the root's
  child-folder collection with one `.Name` COM read per child — then the `children[childName]`
  indexer. **None of it is wrapped in any exception handler.**

The getter's XML doc (`:243-256`) documents **only** `<exception cref="InvalidOperationException">`.
`COMException` is undocumented and unhandled. **Finding 1 confirmed as stated.**

`TaskMaster/AppGlobals/ArchiveRootPathGuard.cs` is a 62-line pure static helper with a single method
`RequireResolvedArchiveRoot(string, string, Action<string>)` and two constant rule strings. It is
deliberately free of Outlook COM types. **It must not change** (see §1.5).

`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:15` declares `string ArchiveRootPath { get; }` with
**no XML documentation at all** — the interface documents neither `InvalidOperationException` nor
anything else. The issue's implication that the interface "currently documents only
`InvalidOperationException`" is **imprecise**: that documentation exists on the *implementation*
(`AppOlObjects.cs:243-256`), not on the interface member.

### 1.2 Keyboard dispatch (finding 2) — the issue's causal claim is wrong

`EfcFormController.KbdExecuteAsync` has exactly two overloads, at `EfcFormController.cs:921-925`
and `:927-931`. Neither has a try/catch. **That part of finding 2 is confirmed** (the issue's cited
`:894-903` is stale; the current range is `:921-931`).

The issue's consequence claim — "propagates uncaught from a keyboard-input dispatch path", escalated
in Impact/Severity to "can crash the EFC form outright" — is **not supported by the reachable call
chain.** Traced end to end:

1. The `'N'` character action `KbdExecuteAsync(CreateFolderAsync)` is registered in **two**
   dictionaries: the async one via `GetAsyncCharacterActions()` / `CharacterAsyncActions`
   (`:635-666`, the `'N'` entry at `:657`) and the sync one via `GetKbdActions()` /
   `CharacterActions` (`:692-740`, the `'N'` entry at `:722-726`).
2. `CharacterAsyncActions` is pushed into `KeyboardHandler.CharActionsAsync` by
   `ToggleOnNavigationAsync()` (`:1069-1076`), reached from
   `KeyboardHandler.ToggleKeyboardDialogAsync()` (`KeyboardHandler.cs:225-236`).
3. **The live `KeyDown` wiring for the EFC form is a single site**, `EfcFormController.cs:435-437`:
   ```csharp
   x.KeyDown += new System.Windows.Forms.KeyEventHandler(
       _homeController.KeyboardHandler.KeyboardHandler_KeyDownAsync
   );
   ```
   Verified by repo-wide `Grep(pattern="KeyboardHandler_KeyDown", glob="*.cs")`. Every other
   `KeyDown +=` in the EFC/QFC controllers targets `KeyboardHandler_KeyDownAsync`
   (`EfcItemController.cs:660`, `QfcItemController.EventWiring.cs:45` and `:422`,
   `QfcFormController.SetupDisposal.cs:164` and `:193`). The **sync**
   `KeyboardHandler.KeyboardHandler_KeyDown` (`KeyboardHandler.cs:114-131`, no try/catch) appears in
   the QuickFiler controllers **only inside commented-out lines** (`EfcItemController.cs:658`,
   `QfcItemController.EventWiring.cs:43`, `QfcFormController.SetupDisposal.cs:162` and `:191`). Its
   two live callers are `QfcFormViewerExpanded.cs:48` and `QfcFormViewerDark.cs:48` — **QFC** form
   viewers, reached from their own `ProcessCmdKey`, not from `EfcViewer`. **The sync handler is
   never wired to a live event for the EFC form.**
4. `KeyboardHandler_KeyDownAsync` (`KeyboardHandler.cs:133-148`) **does** have a try/catch:
   ```csharp
   try { await KeyDownTaskAsync(sender, e); }
   catch (System.Exception ex)
   { logger.Error($"Error in {nameof(KeyboardHandler_KeyDownAsync)} for key {e.KeyValue}. {ex.Message}", ex); }
   ```
   It catches `System.Exception`, logs, and **surfaces nothing to the user**; it does **not** route
   through `EfcFormController.BoundaryErrorSink` / `TryReportBoundaryFault`.

**Correct characterization of finding 2:** an exception from `KbdExecuteAsync(CreateFolderAsync)` on
the live wired path is **caught and silently logged at the `KeyboardHandler` boundary**, not a
process crash. The defect is a *silent, undiagnosed swallow at the wrong boundary* — a violation of
the fail-fast / diagnosable-failure requirements in CLAUDE.md §3 and `.claude/rules/general-code-change.md`
("Error Handling and Logging") — but the issue's "crash the EFC form outright" framing is
**factually incorrect for this path** and must not be carried into the spec's acceptance criteria.

`CreateFolderAsync` (`EfcFormController.cs:842-885`) has **no local try/catch of its own**, and reads
`_globals.Ol.ArchiveRootPath` twice, at `:863` and `:873`. Confirmed exactly.

**Additional verified finding not in the issue (adjacent, same boundary):**
`KeyboardHandler.ToggleKeyboardDialogAsync(object sender, KeyEventArgs e)` (`KeyboardHandler.cs:238-245`)
is `async void` with **no try/catch**, unlike its sibling `KeyboardHandler_KeyDownAsync`. It is
reached live from `EfcViewer.ProcessCmdKey` (`QuickFiler/Viewers/EfcViewer.cs:106-117`) on a bare-Alt
chord, and it awaits `ToggleKeyboardDialogAsync()` → `FormController.ToggleOnNavigationAsync()`. An
exception there **is** a genuine unobserved async-void fault. This is a real second keyboard-boundary
gap; it is **not** what finding 2 describes and should be raised as its own follow-up rather than
silently folded into this fix.

**Coverage constraint on any KeyboardHandler-side fix:** `KeyboardHandler` is decorated
`[ExcludeFromCodeCoverage]` (`KeyboardHandler.cs:22`). Logic placed there is unmeasurable and
untestable in the harness. Any fix for finding 2 must therefore land in
`EfcFormController.KbdExecuteAsync`, which is **not** coverage-exempt and **is** reachable by the
existing headless harness (§4.1).

### 1.3 The boundary sink (finding 4)

The default at `EfcFormController.cs:127-129` is unchanged and log-only:

```csharp
/// <summary>Fault-boundary sink; an injectable seam over the static logger above.</summary>
internal System.Action<string, System.Exception> BoundaryErrorSink { get; set; } =
    (message, exception) => logger.Error(message, exception);
```

**Finding 4's substance is confirmed.** Two corrections to its supporting detail:

1. Issue #726 already added `TryReportBoundaryFault(string, System.Exception)`
   (`EfcFormController.cs:131-156`), which null-checks the sink and wraps invocation in its own
   try/catch. This changes *robustness of sink invocation*, **not** the log-only default. Finding 4
   remains valid.
2. **The call-site count is 6, not the 4 the issue claims**, and none of the four cited line numbers
   (456, 473, 491, 553) is current. Current sites and their owners:

   | Line | Enclosing method | Reached from |
   |---|---|---|
   | 483 | `ButtonCancelClickAsync` (`:472-485`) | `ButtonCancel_Click` async void (`:469-470`), wired at `:448` |
   | 500 | `ButtonOkClickAsync` (`:489-502`) | `ButtonOK_Click` async void (`:487`), wired at `:445` |
   | 518 | `ButtonRefreshClickAsync` (`:507-520`) | `ButtonRefresh_Click` async void (`:504-505`), wired at `:449` |
   | 580 | `ButtonCreateClickAsync` (`:525-582`) | `ButtonCreate_Click` async void (`:522-523`), wired at `:450` |
   | 595 | `ButtonDeleteClickAsync` (`:587-597`) | `ButtonDelete_Click` async void (`:584-585`), wired at `:451` |
   | 1165 | `PopulateFolderCombobox` (`:1146-1167`) | two fire-and-forget call sites (`:115`, and via `InitializeDataFields`) |

   `EfcFormController.cs` contains **9** `catch (` clauses in total; the 3 that do **not** route to
   the sink are `:151` (inside `TryReportBoundaryFault` itself), `:973` (breadcrumb WebView2
   initialization — log-only), and `:1016`/`:1020` (`BindBreadcrumbRowsAsync` — log-only).

### 1.4 The five reads (finding 5)

All five line numbers in finding 5 are stale. Current state, with the enclosing handler verified:

| # | Line | Enclosing member | Local exception handling |
|---|---|---|---|
| 1 | 556 | `ButtonCreateClickAsync` (`:525-582`) | **Yes** — `catch (System.Exception ex)` at `:578-581` → `TryReportBoundaryFault` |
| 2 | 566 | `ButtonCreateClickAsync` | same catch at `:578-581` |
| 3 | 863 | `CreateFolderAsync` (`:842-885`) | **None.** No try/catch anywhere in the method |
| 4 | 873 | `CreateFolderAsync` | **None** |
| 5 | 1014 | `BindBreadcrumbRowsAsync` (`:1007-1024`) | **Yes** — `catch (OperationCanceledException)` at `:1016` and `catch (System.Exception ex)` at `:1020-1023`, `logger.Error` only, **not** routed to the sink |

**Correction:** the issue's blanket claim that all five "read `_globals.Ol.ArchiveRootPath` directly
with no guard" is only literally true for reads 3 and 4. Reads 1 and 2 are inside a catch that
contains the fault but reports it log-only (finding 4's defect); read 5 is inside a catch that is
log-only and bypasses the sink entirely. The accurate statement is: **two of five reads are
genuinely unguarded; the other three are guarded only to the level of a log line with no user-facing
diagnostic.**

### 1.5 Constraints that must survive this fix

- **`ArchiveRootPathGuard`'s throw contract is frozen.** `#638`'s spec excludes it explicitly
  (`docs/features/active/2026-08-26-.../spec.md:236-240`). It is pinned by
  `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs`, **which still exists at
  that exact path** — 6 `[TestMethod]`s, all pure (no `AppOlObjects` instance is constructed in any
  of them). Consequence worth noting: **these tests pin the guard and the `IOlObjects` seam, but
  they do not cover the `ArchiveRootPath` getter body at all.** Any finding-1 fix that adds logic to
  the getter adds uncovered lines unless the logic is extracted into a testable helper.
- **`#638` explicitly rejected widening the `EfcDataModel` catch to `COMException`**
  (`spec.md:206-213` non-goal (a); `spec.md:771`: "Catch widened to `Exception` or to `COMException`
  … would silently absorb non-goal (a)"). That rejection is **pinned by a live test**:
  `EfcDataModelArchiveRootTests.MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates`
  (`:248-262`) asserts `await act.Should().ThrowAsync<COMException>()`. Because that test throws the
  `COMException` from a `Mock<IOlObjects>` — i.e. *at the interface seam, above `AppOlObjects`* — a
  fix confined to the `AppOlObjects` getter body does **not** break it. A fix that adds a
  `COMException` catch at the `EfcDataModel` call sites **would** break it and is out of bounds.
- **`#638`'s three deferred non-goals are exactly this issue's findings 1/2/4/5** (`spec.md:203-234`):
  (a) `COMException` from the getter's live COM calls = finding 1; (b) the log-only boundary-sink gap
  at the five `async void` handlers = finding 4; (c) the archive-root reads inside
  `EfcFormController.cs` = finding 5, with the keyboard chain of finding 2 described inline. #736 is
  the correct place to land them.
- **File-size ceiling (500 lines, `.claude/rules/general-code-change.md`).**
  `EfcFormController.cs` is **1216** lines — already far over, and it is `internal class`, **not
  `partial`** (`:26`), so lines cannot be relieved into a sibling file without a declaration change.
  `AppOlObjects.cs` is **494** lines — 6 lines of headroom, but the class **is** `partial`, and
  `AppOlObjects.StoreRehook.cs` is the existing precedent for a sibling partial. **Any non-trivial
  addition to `AppOlObjects` belongs in a new partial file (e.g. `AppOlObjects.ArchiveRoot.cs`), not
  in `AppOlObjects.cs`.**

---

## 2. Finding 6 — the issue's causal claim is factually wrong

This is the most important correction in this document.

### 2.1 What the test actually is

`QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs:164-186`:

```csharp
/// <summary>
/// Scenario: the archive root resolves normally, so the guard must not change the
/// success path.
/// Expected outcome: the archive root is read exactly once. The move still fails deeper
/// in the filer with a null reference, because the test mail helper carries no folder
/// information; that is the barrier that stops any second archive-root read.
/// </summary>
[TestMethod]
public async Task MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce()
{
    var olObjects = CreateOlObjects();
    olObjects.SetupGet(value => value.ArchiveRootPath).Returns(ArchiveRootLiteral);   // :176 — RESOLVES
    ...
    await act.Should().ThrowAsync<NullReferenceException>();                          // :182
    olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Once());                // :185
}
```

The archive root **resolves successfully** in this test (`:176` returns a value; it does not throw).
The `NullReferenceException` is raised several frames downstream:

- `MoveAsync` (`:312-321`) calls `EfcDataModel.MoveToFolderAsync(...)`.
- `EfcDataModel.MoveToFolderAsync` (`EfcDataModel.cs:303-347`) succeeds through `TryGetArchiveRoot`
  (`:327`), builds the config including `OlAncestor = olAncestor` (`:339`), and calls
  `new EmailFiler(config).SortAsync(mailHelpers)` (`:343-344`).
- `EmailFiler.SortAsync(IList<MailItemHelper>)`
  (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:128-135`) then evaluates, at
  **`:133`**:
  ```csharp
  ResolvePaths((Folder)MailHelpers.FirstOrDefault()!.FolderInfo!.OlFolder!);
  ```
  `TestableEfcDataModel` (`:377-387`) supplies a parameterless `MailItemHelper`, whose `FolderInfo`
  (`IFolderWrapper?`, settable — see `MailItemHelper.cs:219`, `ItemInfo.cs:58`) is **null**. The
  `.OlFolder` dereference of that null is the `NullReferenceException`.

**This has nothing to do with archive-root resolution.** Fixing findings 1 and 5 changes code in
`AppOlObjects.cs` and `EfcFormController.cs`; this test touches neither (it mocks `IOlObjects`
directly and drives `EfcDataModel`). **The `NullReferenceException` will still occur after findings 1
and 5 land.**

### 2.2 The discrepancy, stated plainly

| Source | Claim about this test |
|---|---|
| **#736 finding 6** (`issue.md:45`) | "…asserts `ThrowAsync<NullReferenceException>()` as the test's pass condition — using an unhandled-exception crash as the expected/passing outcome … This test would need to change once findings 1 and 5 are fixed with proper guards (**the `NullReferenceException` it currently expects should no longer occur**)." |
| **#699** (`docs/features/potential/promoted/2026-08-29-efcdatamodel-success-path-test-uses-incidental-crash-as-barrier.md:39-43`) | "That exception is not a property of the unit under test: it is the `EmailFiler` collaborator dereferencing a `MailItemHelper` whose folder information is null, several frames past the code issue 638 touched. **Once `EmailFiler` no longer throws there**, the test fails with a message about a missing `NullReferenceException` and points at the wrong subsystem." |

**#699 is correct; #736 finding 6's causal sentence is wrong.** #699 also correctly grades the item
**Low** severity and describes it as **latent** ("does not reproduce against the current tree",
`:48`), whereas #736 folds it into a **High** severity narrative and asserts the crash-as-pass
condition "is the one test that exercises this path [and] treats the crash as correct behavior"
(`issue.md:59`) — which it does not, because the path it exercises is the *success* path.

Per the orchestrator, #699 is closed as NOT_PLANNED with the comment "Superseded by consolidated
issue #736". **#699's body is therefore the authoritative statement of the real defect**, and the
`spec.md` for #736 must restate finding 6 in #699's terms rather than #736's.

### 2.3 The correct fix for finding 6, per #699's own Expected Behavior

> "The test terminates the success path deliberately and asserts only the invariant it exists to pin,
> which is `olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Once())`. A failure reports a
> problem with the archive-root read count." (`:33-35`)

Two hard constraints #699 attaches (`:67-70`):

- **This test is the only one reaching `EfcDataModel.cs:339` (`OlAncestor = olAncestor,`).** Deleting
  it rather than replacing it would have dropped #638's changed-line coverage from 93.10% to ~89.7%,
  below the 90% floor for new/changed code (`.claude` / CLAUDE.md General Unit Test Policy UT2).
  **Any fix must keep line 339 covered.**
- The whole class must stay green: **11 of 11** `[TestMethod]`s.

### 2.4 Concrete mechanisms available to a later implementer (options, not a design)

Verified facts that bound the option space:

- `EmailFiler.SortAsync(IList<MailItemHelper>)` (`EmailFiler.cs:128`) is **not `virtual`**. The
  parameterless `SortAsync()` (`:137`) *is* `public virtual`, and `ResolvePaths(Folder)` (`:377`) is
  `protected internal virtual` — but **neither is reachable before `:133` throws**, because the NRE
  occurs while evaluating the *argument* to `ResolvePaths`. **Subclassing `EmailFiler` alone cannot
  fix this test.**
- `EfcDataModel` is `internal partial class` (`:21`) and already carries injectable/overridable
  seams: `UserDiagnosticAction` (`:154`, `internal Action<string>` defaulting to `MessageBox.Show`)
  and `protected set` accessors on `Globals` (`:160`), `Token` (`:167`), `TokenSource` (`:174`),
  `FolderHelper` (`:185`), `ConversationResolver` (`:227`). `TestableEfcDataModel` already exploits
  the last of these.
- `MailItemHelper.FolderInfo` is a settable `IFolderWrapper?` property (`MailItemHelper.cs:219`;
  declared on `ItemInfo.cs:58`).

**Option A — filer-invocation seam on `EfcDataModel` (this is #699's own proposal, `:74`).** Extract
the `new EmailFiler(config); await sorter.SortAsync(mailHelpers)` pair
(`EfcDataModel.cs:343-344`) behind a `protected internal virtual Task<bool>` member on
`EfcDataModel`. `TestableEfcDataModel` overrides it to return `Task.FromResult(true)`. The config
object — including `OlAncestor = olAncestor` at `:339` — is still constructed, so **coverage of line
339 is preserved**, and the test asserts only `VerifyGet(..., Times.Once())`. Cleanest fit with the
existing seam style of the class.

**Option B — supply a non-null `FolderInfo`.** Set `MailHelper.FolderInfo` to a
`Mock<IFolderWrapper>` whose `OlFolder` returns null, so `:133` evaluates to `(Folder)null` and
`ResolvePaths(null)` is entered. **Not recommended without further verification:**
`EmailFilerConfig.ResolvePaths(Folder)` (`EmailFilerConfig.cs:182-203`) then calls
`IsDeleteRelevant(currentFolder)`, `currentFolder.FolderPath` (`:201`), and
`TryResolveDestinationFolder()` (`:199`), the last of which reads `Globals!.Ol.App` (`:225`) — an
unstubbed member on a `MockBehavior.Strict` mock. This trades one incidental exception for a
different incidental exception and does not satisfy #699's "terminates deliberately" requirement.

**Option C — do nothing but re-document.** Rejected: it leaves the misdirecting failure message #699
was raised about, and #736 has already consolidated #699, so declining to act closes the item with
the defect intact.

---

## 3. Numeric Derivation Evidence

Required before any numeric claim reaches an approved `spec.md` acceptance criterion.

### N1 — `EfcFormController.cs` contains exactly **5** reads of `_globals.Ol.ArchiveRootPath`

- **Complete family:** every read expression of the `ArchiveRootPath` member of `IOlObjects` reached
  through the `_globals.Ol` chain within `QuickFiler/Controllers/EfcFormController.cs`, in any
  syntactic position (argument, assignment, interpolation, aliased local). `ArchiveRootPath` is a
  single get-only property on `IOlObjects` (`IOlObjects.cs:15`) with **no overloads**, so the family
  has exactly one member name; the family is closed by also covering the neighbouring `ArchiveRoot`
  property, which is the only other member that could carry the same value.
- **Exhaustive search scope:** the whole file, lines 1–1216 (see §0 for the count derivation).
- **Inclusion rules:** any occurrence of the identifier that evaluates the property. Occurrences in
  active (non-commented) code only.
- **Exclusion rules:** commented-out lines; occurrences in any other file; the `_router.BindRowsAsync`
  parameter name (not a read).
- **Primary search strategy:** repo-wide `Grep(pattern="ArchiveRootPath", glob="*.cs", output_mode=content)`
  across the whole worktree, then filtered to the `EfcFormController.cs` rows.
- **Primary member set:** `{556, 566, 863, 873, 1014}`.
- **Primary count:** **5**.
- **Cross-check search strategy (distinct expression, distinct scope):**
  `Grep(pattern="ArchiveRoot", path=".../QuickFiler/Controllers/EfcFormController.cs")` — a broader
  stem restricted to the single file, which would also surface `ArchiveRoot`, `ArchiveRootLiteral`,
  or any alias the narrower pattern would miss, and which scans all 1216 lines rather than relying
  on the section-by-section reads used for context.
- **Cross-check member set:** `{556, 566, 863, 873, 1014}`.
- **Cross-check count:** **5**.
- **Member-set comparison:** normalized sets are **identical** (`{556, 566, 863, 873, 1014}`); no
  member appears in one and not the other; the broader stem surfaced no additional `ArchiveRoot`
  occurrence. **Assertion accepted.**

### N2 — `EfcFormController.cs` contains exactly **6** call sites of `TryReportBoundaryFault`

- **Complete family:** every *invocation* of the fault-reporting boundary in
  `EfcFormController.cs`, i.e. every site that reports a caught exception through the controller's
  sink. `TryReportBoundaryFault` has a single declaration (`:138`) and **no overloads**; the family
  is closed by also enumerating direct `BoundaryErrorSink(...)` invocations, since a call site could
  bypass the wrapper.
- **Exhaustive search scope:** the whole file, lines 1–1216.
- **Inclusion rules:** invocation expressions only.
- **Exclusion rules:** the method declaration (`:138`), the property declaration (`:128-129`), the
  local `var sink = BoundaryErrorSink;` read (`:140`), the `<see cref>` in the XML doc (`:132`), and
  the `sink(message, exception)` invocation *inside* the wrapper (`:149`), which is the wrapper's own
  body rather than a call site.
- **Primary search strategy:** `Grep(pattern="TryReportBoundaryFault|BoundaryErrorSink", path=".../QuickFiler")`,
  then removal of the excluded declaration/doc rows.
- **Primary member set:** `{483, 500, 518, 580, 595, 1165}`.
- **Primary count:** **6**.
- **Cross-check search strategy (structurally different — enumerate the handlers, not the callee):**
  `Grep(pattern="catch \\(", path=".../EfcFormController.cs", -A 3)`, enumerating **all** exception
  handlers in the file and classifying each by its body. This is exhaustive over the family because
  every call site of the reporter sits inside a catch block, so enumerating all catch blocks cannot
  miss one.
- **Cross-check member set:** 9 catch clauses at `{151, 481, 498, 516, 578, 593, 973, 1016, 1020}`;
  those whose bodies invoke the reporter are `{481→483, 498→500, 516→518, 578→580, 593→595,
  1163→1165}`, i.e. reporter invocations at `{483, 500, 518, 580, 595, 1165}`. The three
  non-reporting handlers are `151` (inside the wrapper), `973` (WebView2 init, `logger.Error`), and
  `1016`/`1020` (`BindBreadcrumbRowsAsync`, `logger.Debug`/`logger.Error`).
- **Cross-check count:** **6**.
- **Member-set comparison:** normalized sets are **identical** (`{483, 500, 518, 580, 595, 1165}`).
  The cross-check additionally proves the set is *exhaustive* (no reporter invocation exists outside
  a catch block, and no catch block reports through a path the primary query missed).
  **Assertion accepted — and the issue's figure of 4 is superseded.**

### N3 — `EfcFormController` declares exactly **2** `KbdExecuteAsync` overloads, **0** of them with a try/catch

- **Complete family:** every declaration of a member named `KbdExecuteAsync` on the
  `EfcFormController` type, across all arities and generic arities.
- **Exhaustive search scope:** repo-wide `*.cs`, then narrowed to `EfcFormController.cs` (the type is
  non-`partial`, `:26`, so no other file can declare its members — this is what makes the narrowing
  exhaustive rather than merely convenient).
- **Inclusion rules:** method declarations. **Exclusion rules:** call sites; commented-out lines
  (`:680-684`); declarations on other types (`EfcItemController.cs:1095`,
  `QfcItemController.Navigation.cs:63` and `:72`).
- **Primary search strategy:** repo-wide `Grep(pattern="KbdExecuteAsync", glob="*.cs")`, then
  classification of each hit as declaration vs. call site vs. comment vs. other type.
- **Primary member set:** `{921 (Func<Task>), 927 (System.Action)}`. Call sites in the same file at
  `{650, 651, 655, 657, 658, 662, 710, 715, 720, 725, 730, 736}`; commented-out at `{680, 681, 682,
  683, 684}`; other types at `{EfcItemController.cs:1095, QfcItemController.Navigation.cs:63,
  QfcItemController.Navigation.cs:72}`.
- **Primary count:** **2**.
- **Cross-check search strategy:** `Grep(pattern="Task KbdExecuteAsync", path=".../EfcFormController.cs")`
  — a signature-shaped pattern that matches only a return-type-plus-name sequence, which no call site
  or comment in the file can produce, run against the single file.
- **Cross-check member set:** `{921, 927}`.
- **Cross-check count:** **2**.
- **Member-set comparison:** normalized sets are **identical** (`{921, 927}`). Bodies read directly
  (`:921-931`): both are two statements (`await ToggleKeyboardDialogAsync(); await action();` /
  `... ; action();`) with no `try`. **Assertion accepted.**

### N4 — `EfcDataModelArchiveRootTests` contains exactly **11** test methods

- **Complete family:** every MSTest test-method attribute in the file — `[TestMethod]` **and**
  `[DataTestMethod]` (a `[DataTestMethod]` would contribute one method but N `[DataRow]` results, so
  both attributes must be in scope for the enumeration to be exhaustive).
- **Exhaustive search scope:** `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`, lines
  1–389.
- **Inclusion rules:** attribute occurrences on method declarations. **Exclusion rules:** none
  applicable (no commented-out attributes present).
- **Primary search strategy:** `Grep(pattern="\\[TestMethod\\]|\\[DataTestMethod\\]", path=<file>)`.
- **Primary member set:** `{46, 69, 92, 117, 142, 171, 193, 223, 248, 268, 290}` — all `[TestMethod]`;
  zero `[DataTestMethod]`.
- **Primary count:** **11**.
- **Cross-check search strategy:** full sequential `Read` of the file (1–389) and enumeration of the
  `public async Task <Name>()` declarations by name — a name-based enumeration independent of the
  attribute text.
- **Cross-check member set:** `MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing` (47),
  `MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing` (70),
  `OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns` (93),
  `OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns` (118),
  `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress` (143),
  `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` (172),
  `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot` (194),
  `MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot` (224),
  `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates` (249),
  `OpenOlFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot` (269),
  `OpenFsFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot` (291).
- **Cross-check count:** **11**.
- **Member-set comparison:** each attribute line in the primary set is immediately followed by the
  correspondingly-numbered declaration in the cross-check set (46→47, 69→70, 92→93, 117→118,
  142→143, 171→172, 193→194, 223→224, 248→249, 268→269, 290→291). Sets correspond one-to-one with no
  residue. **Assertion accepted — and it matches #699's independent "11 of 11" figure.**

---

## 4. Candidate approaches and recommendation

### 4.1 Existing, reusable test harness (governs which approach is viable)

`QuickFiler.Test/Controllers/EfcFormControllerTests.cs` already provides everything needed to test
`EfcFormController` **without Outlook COM, WinForms, WebView2, or a UI pump**:

- `CreateMinimalController()` (`:24-34`) — invokes the private no-arg constructor via reflection,
  producing an all-fields-null controller.
- `SetPrivateField(object, string, object)` (`:467`) — injects mocks into private fields
  (`_globals`, `_router`, `_formViewer`, `_folderRows`, `_parentCleanup` are all already used this way).
- `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow` (`:245-280`) — a `[DataTestMethod]` whose
  five `[DataRow]`s pin **5 of the 6** sink call sites; `PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault`
  (`:299-328`) pins the 6th. **All 6 sites are already covered.** Both inject a custom sink, so they
  are unaffected by a change to the *default* sink.

**Blocking constraint on finding 4, verified:** `BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing`
(`:282-294`) invokes the **default** delegate directly and asserts it does not throw. If finding 4's
fix gives the default a modal user-facing surface (`MessageBox.Show`, as the issue's proposed-fix
checklist suggests), **this test will display a modal dialog in the test host and hang the run.**
Any finding-4 fix must keep the default non-blocking *or* update this test in the same change.

**Gap, verified:** `EfcFormController.KbdExecuteAsync` has **zero** test references anywhere in the
repo (repo-wide `Grep(pattern="KbdExecuteAsync", glob="*.cs")` returns no `QuickFiler.Test` hit for
`EfcFormController`; only `QfcItemController.NavigationTests.cs` tests the *QFC* equivalents).
Likewise, `TryReportBoundaryFault`'s null-sink branch (`:141-145`) and throwing-sink branch
(`:151-155`) have no test — no test in the repo sets `BoundaryErrorSink = null` or to a throwing
delegate.

### 4.2 In-repo precedent for guarding a live COM read

The repository already has a settled idiom; **the fix must reuse it, not invent one.** The strongest
precedent is in the *same class*:

- `AppOlObjects.ResolveCurrentUserEmailAddress()` (`AppOlObjects.cs:360-383`) — wraps a chain of live
  COM reads in `try { ... } catch (COMException e) { logger.Warn(...); return string.Empty; }`.
- `AppOlObjects.TryGetSmtpAddress(AddressEntry)` (`:385-413`) — `internal static`, COM-guarded,
  returns null on failure. This is the shape that makes a COM-guarded read **unit-testable**:
  logic hoisted to a static member taking already-resolved inputs.
- `AppOlObjects.EmitPerStoreInboxAttribution(...)` (`:146+`) — the fully-developed version of the
  same idea: the COM boundary is expressed entirely through injectable `Func<>` delegates so a fake
  can drive the method without live COM, while the COM-touching caller
  (`ResolveInboxForStore`, `AppOlObjects.StoreRehook.cs:66+`) carries `[ExcludeFromCodeCoverage]`
  with a written by-inspection justification (`:58-64`).
- `OutlookReadinessGate.IsReady()` / `IsReady(Store?)`
  (`UtilitiesCS/OutlookObjects/OutlookReadinessGate.cs:62-93`) — non-throwing COM probes.
- `OutlookReadinessGate.IsTransientError(COMException)` (`:101+`) plus the public HRESULT constants
  `TransientStoreNotReadyHResult` (`:30`), `TransientOperationFailedHResult` (`:36`),
  `TransientStartupReadinessHResult` (`:42`) — the repo's existing transient/permanent split.

There is **no** `catch (COMException` anywhere under `QuickFiler/` (verified by
`Grep(pattern="catch \\(COMException|catch \\(System\\.Runtime\\.InteropServices\\.COMException", path=".../QuickFiler")`
→ no matches). The idiom lives in `TaskMaster/AppGlobals` and `UtilitiesCS`. **This reinforces
placing the finding-1 fix in `AppOlObjects`, where the idiom already exists, rather than in the
QuickFiler controller.**

### 4.3 Recommended approach

**Guard at the source (`AppOlObjects`), extracted into a testable seam, in a new partial file;
report at the boundary (`EfcFormController.KbdExecuteAsync`) through the existing sink.**

Concretely:

1. **Finding 1 —** In a **new** `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` partial file
   (`AppOlObjects.cs` has only 6 lines of headroom under the 500-line ceiling), move the
   archive-root composition behind a COM-guarded read that follows the
   `ResolveCurrentUserEmailAddress` / `TryGetSmtpAddress` shape: a thin COM-touching member that
   catches `COMException` from `Root.FolderPath` and from `ArchiveRoot?.FolderPath`, plus a
   `static`/delegate-driven member carrying the decision logic so it is unit-testable without COM.
   Convert the caught `COMException` into the **already-documented** `InvalidOperationException`
   contract (reusing `ArchiveRootPathGuard.UnresolvableRule` semantics and the #602 redaction rule —
   the message must name the rule and withhold the path), preserving the inner exception. Update the
   getter's XML doc accordingly.
   *Why this direction:* every existing consumer — `EfcDataModel.TryGetArchiveRoot`
   (`EfcDataModel.cs:280-297`) among them — already handles `InvalidOperationException` and
   deliberately does not handle `COMException`. Normalizing at the source therefore fixes all
   consumers at once **without touching the `EfcDataModel` catch that #638 explicitly froze**, and
   leaves `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates` green (it injects the
   `COMException` at the `IOlObjects` mock, above the layer being changed).
2. **Findings 2 + 5 (reads 3 and 4) —** Add a `try`/`catch` to both `KbdExecuteAsync` overloads
   (`:921-931`) routing through `TryReportBoundaryFault`, so the keyboard dispatch path reports at
   the *same* boundary as the six button/populate paths instead of being silently absorbed by the
   coverage-exempt `KeyboardHandler`. This is the minimal change that covers `CreateFolderAsync`'s
   two unguarded reads without adding a second, redundant handler inside `CreateFolderAsync` — and
   it is testable, whereas a `KeyboardHandler`-side fix is not.
3. **Finding 5 (read 5) —** Route `BindBreadcrumbRowsAsync`'s `catch (System.Exception ex)`
   (`:1020-1023`) through `TryReportBoundaryFault` instead of a bare `logger.Error`, so the last
   archive-root read reports at the same boundary. (`catch (OperationCanceledException)` at `:1016`
   stays as-is — cancellation is not a fault.)
4. **Finding 4 —** Give the default sink a **non-blocking** user-facing surface while keeping
   `BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing` green. A modal `MessageBox.Show` in
   the default is **not** acceptable (§4.1). Prefer routing to whatever non-modal notification the
   repo already uses, or introduce the surface as an injectable second seam whose default stays
   non-blocking in the test host.
5. **Finding 6 —** Implement #699's Option A (§2.4): a filer-invocation seam on `EfcDataModel`, then
   drop the `ThrowAsync<NullReferenceException>` assertion and keep only
   `VerifyGet(..., Times.Once())`. Re-measure that `EfcDataModel.cs:339` stays covered.

**Rejected alternatives (brief):**
- *Guard at each of the five `EfcFormController` call sites.* Rejected: duplicates the same handler
  five times, adds ~40 lines to a file already 716 lines over the ceiling, and leaves every other
  `ArchiveRootPath` consumer in the repo (`FolderPredictor`, `FolderConverter`, `MailItemHelper`,
  `SortItemsToExistingFolder`, `QfcItemController`, `SortEmail`, `MeetingItemHelper`) unguarded.
- *Widen the `EfcDataModel` catch to `COMException`.* Rejected — explicitly rejected by #638
  (`spec.md:212, 771`) and pinned red by an existing test (`:248-262`).
- *Let `COMException` propagate and catch it at the `async void` rims.* Rejected: the rims already
  catch `System.Exception`, so this changes nothing about the observed behavior while leaving the
  getter's documented contract still wrong and every non-EFC consumer still exposed.
- *Fix finding 2 inside `KeyboardHandler`.* Rejected: `[ExcludeFromCodeCoverage]`
  (`KeyboardHandler.cs:22`) makes the fix unmeasurable, and the class is shared with QFC, widening
  blast radius beyond this issue.

---

## 5. Behavior semantics

- **Success:** `ArchiveRootPath` returns the validated path; the cached `_archiveRootPath` is
  populated exactly once; no diagnostic is emitted. (Pinned today by
  `AppOlObjectsArchiveRootValidationTests.RequireResolvedArchiveRoot_ResolvedRootMatchesComposedPath_ReturnsIt`.)
- **Failure — unresolvable / cross-store:** unchanged. `InvalidOperationException` carrying
  `UnresolvableRule` or `CrossStoreRule`; the diagnostic is logged **before** the throw; neither the
  message nor the diagnostic contains a path or mailbox address (#602 redaction).
- **Failure — transient COM (new):** the `COMException` is normalized to `InvalidOperationException`
  at the `AppOlObjects` layer, with the `COMException` preserved as `InnerException` and the message
  obeying the same redaction rule. Consumers that already absorb `InvalidOperationException`
  (`EfcDataModel.TryGetArchiveRoot`) then behave identically to the unresolvable case:
  `MoveToFolderAsync` returns `false`; `OpenOlFolderAsync` / `OpenFsFolderAsync` report once through
  `UserDiagnosticAction` and return.
- **Ordering:** the guard must not be entered with an argument whose evaluation has already thrown.
  The COM guard therefore wraps the *argument evaluation*, not the guard call.
- **Idempotence / caching:** the `_archiveRootPath is null` memoization means a failure is **not**
  cached — a subsequent read retries the COM chain. This is existing behavior and must be preserved;
  a fix that caches a sentinel on failure would be a behavior change.
- **Keyboard dispatch:** an exception thrown by any action dispatched through `KbdExecuteAsync` must
  be reported exactly once through `TryReportBoundaryFault` and must not propagate out of
  `KbdExecuteAsync`. `ToggleKeyboardDialogAsync()` runs before the action; a failure in the toggle
  itself must be reported by the same handler.
- **Edge cases to pin:** null `action` argument to either `KbdExecuteAsync` overload; a
  `BoundaryErrorSink` that is null; a `BoundaryErrorSink` that throws (both `TryReportBoundaryFault`
  branches are currently untested); `OperationCanceledException` inside `KbdExecuteAsync` (should it
  report as a fault, or be treated like the cancellation arm at `:1016`? — this must be decided in
  the spec, not left implicit).

---

## 6. Requirements mapping — files a fix will touch

| Change | File | Current size | Note |
|---|---|---|---|
| COM-guarded archive-root read + testable seam | **new** `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` | n/a | class is already `partial` (`AppOlObjects.cs:26`); precedent `AppOlObjects.StoreRehook.cs` |
| Getter delegates to the new seam; XML doc updated | `TaskMaster/AppGlobals/AppOlObjects.cs` | 494 | **6 lines of headroom** — keep the edit net-neutral or negative |
| `try`/`catch` → `TryReportBoundaryFault` in both `KbdExecuteAsync` overloads (`:921-931`); `BindBreadcrumbRowsAsync` catch (`:1020-1023`) rerouted to the sink; default sink surface (`:128-129`) | `QuickFiler/Controllers/EfcFormController.cs` | 1216 | **already 716 over the ceiling and non-`partial`.** Pre-existing violation; keep the addition as small as possible and call it out explicitly in the PR |
| Filer-invocation seam (finding 6) | `QuickFiler/Controllers/EfcDataModel.cs` | — | `internal partial class` (`:21`) with existing `protected set` seams |
| Test updates | `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` (`:182` assertion, `TestableEfcDataModel` `:377-387`), `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` (new `KbdExecuteAsync` + sink tests; possibly `:282-294`) | — | — |
| **Must NOT change** | `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs`; `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs`; `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs` member signature; the `EfcDataModel.TryGetArchiveRoot` catch clause; anything in QuickFiler collection/form-controller **disposal** (binding scope constraint — finding 3 is another item's) | — | — |

Note that any **new** `.cs` file added to `QuickFiler.Test` or `TaskMaster.Test` must also be
registered in the corresponding legacy `.csproj` (`#638` had to do exactly this — its in-scope list
includes "Registering that new file in `QuickFiler.Test/QuickFiler.Test.csproj`").

---

## 7. Testing implications (strategy only — no test code)

Framework is fixed by policy: **MSTest + Moq + FluentAssertions**, `tests` mirroring production
layout, no temp files, no live COM.

1. **Finding 1 — new tests in `TaskMaster.Test/AppGlobals/`.** Because `AppOlObjects` cannot be
   constructed without live Outlook, the new tests must target the extracted static/delegate-driven
   seam (the `EmitPerStoreInboxAttribution` pattern): drive it with `Func<string>` delegates that
   throw `COMException` and assert the normalized `InvalidOperationException`, its preserved
   `InnerException`, and that the message contains neither a path nor a mailbox address.
   `AppOlObjectsArchiveRootValidationTests.cs` stays **unmodified** and must still pass 6/6.
2. **Finding 2 — new tests in `EfcFormControllerTests.cs`.** Use `CreateMinimalController()`; the
   all-fields-null state makes `_homeController.KeyboardHandler.ToggleKeyboardDialogAsync()` throw,
   which is the same fault-injection technique `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow`
   already uses. Assert: does not throw; sink invoked exactly once. Cover **both** overloads
   (`Func<Task>` and `System.Action`) — this is a two-member family, and a single test covers only
   one of them.
3. **Finding 4.** Regression-guard `BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing`
   (`:282-294`) against the modal-dialog hazard. Add the two missing `TryReportBoundaryFault`
   branch tests (null sink; throwing sink) — both branches are currently uncovered, and both are in
   the neighbourhood this change touches, so changed-line coverage will require them.
4. **Finding 5.** Extend `Issue439BindBreadcrumbRowsAsync_SubmitsArchiveRootToRealRouter`
   (`:61-159`) with a negative sibling: `ol.SetupGet(ArchiveRootPath).Throws(InvalidOperationException)`
   → assert no throw and sink invoked once.
5. **Finding 6.** Replace the `ThrowAsync<NullReferenceException>` assertion at `:182` with the
   deliberate stop from §2.4 Option A; keep `VerifyGet(..., Times.Once())`. **Re-measure changed-line
   coverage and confirm `EfcDataModel.cs:339` remains covered** — #699 records that losing it drops
   #638's changed-line figure from 93.10% to ~89.7%, below the 90% floor. All **11** tests in the
   class must stay green.
6. **Bugfix Workflow order.** Every one of the above is a defect, so each needs a **failing
   regression test first**, then the minimal fix, then the full toolchain
   (`dotnet tool run csharpier format .` → `msbuild /t:Rebuild ... /p:EnableNETAnalyzers=true
   /p:EnforceCodeStyleInBuild=true` → `msbuild /t:Rebuild ... /p:TreatWarningsAsErrors=true` →
   `vstest.console.exe ... /EnableCodeCoverage`), restarting from step 1 on any failure or auto-fix.
7. **Local vstest note.** Per prior sessions in this repo, local runs need both the `\.claude\`
   worktree exclusion and CI's `/InIsolation`; empty-message sub-millisecond failures indicate an
   assembly-load problem, not a regression.

---

## 8. Summary of corrections to the issue text

| # | Issue's claim | Verified state |
|---|---|---|
| — | `EfcFormController.cs` ~1189 lines (stale scope note) | **1216** |
| 1 | Getter reads unguarded; doc names only `InvalidOperationException` | **Confirmed.** Refinement: `IOlObjects.cs:15` has **no** XML doc at all; the doc is on the implementation |
| 2 | `:894-903`; exception "propagates uncaught"; "can crash the EFC form outright" | Lines are now **`:921-931`**; overloads have no try/catch (**confirmed**), but the live path is caught and **silently logged** by `KeyboardHandler_KeyDownAsync` (`KeyboardHandler.cs:137-147`). **Not a crash.** Separate genuine async-void gap found at `KeyboardHandler.cs:238-245` |
| 4 | Default is log-only; "four call sites (456, 473, 491, 553)" | Default confirmed. **Six** call sites: 483, 500, 518, 580, 595, 1165. #726 added `TryReportBoundaryFault` (`:138-156`), which does not change the log-only default |
| 5 | Five reads at 529, 539, 836, 846, 987, all "with no guard" | Five reads confirmed, at **556, 566, 863, 873, 1014**. Only **two** (863, 873, in `CreateFolderAsync`) are genuinely unguarded; the other three sit in catch blocks that are log-only |
| 6 | The NRE is caused by the unresolvable archive root and "should no longer occur" once 1 and 5 are fixed | **False.** The archive root **resolves** in that test (`:176`); the NRE comes from `EmailFiler.cs:133` dereferencing a null `MailItemHelper.FolderInfo`. **#699 is authoritative**; the correct fix is a deliberate stopping point preserving `EfcDataModel.cs:339` coverage |
