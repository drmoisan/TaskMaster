# Research — Issue #638: EFC unguarded archive-root read

- **Issue:** #638
- **Feature folder:** `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638`
- **Branch:** `bug/efc-unguarded-archive-root-read-crashes-ui-thread-638` (worktree branched from `origin/main` at `ecdb1c84ba8541ab67042985919cfed4df768c01`)
- **Timestamp:** 2026-08-29T08-05
- **Type:** Research only. No production source file was modified.

## 0. Method and tool limitations

All line citations below were read from the working tree of this worktree during this session and are
current as of the branch head named above.

Two limitations must be recorded because they bound the confidence of specific claims:

1. **No shell was available in this session.** The delegation prompt directed `gh issue view 638`.
   This agent had only `Read`, `Grep`, `Glob`, `Write`, `Edit`, and `WebFetch`; no `Bash` tool was
   present, so neither `gh` nor `git` could be executed. The issue text used here is the local
   mirror at `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/issue.md`,
   which the orchestrator stated carries the same verified trace, cross-checked against the
   identical text at `docs/features/potential/promoted/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread.md`
   and `.../spec.md`. Any claim about the *remote* issue body is therefore unverified.
2. **The artifact timestamp was not read from a system clock.** It is derived from the session date
   (2026-08-29) and sequenced after the existing `plan.2026-08-29T07-41.md` in the same folder. The
   format follows `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` (`yyyy-MM-ddTHH-mm`).

## 1. Ground-truth verification of the trace (research question 1)

### 1.1 Confirmed unchanged

| Element | Current location | Issue body claim | Status |
|---|---|---|---|
| `AppOlObjects.ArchiveRootPath` property | `TaskMaster/AppGlobals/AppOlObjects.cs:253-267` | `:253-267` | Confirmed |
| Guard call from the property | `TaskMaster/AppGlobals/AppOlObjects.cs:259-263` | — | Confirmed |
| Backing field `_archiveRootPath` | `TaskMaster/AppGlobals/AppOlObjects.cs:237` | — | Confirmed |
| `RequireResolvedArchiveRoot` throw #1 (unresolvable) | `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:44` | `:44` | Confirmed |
| `RequireResolvedArchiveRoot` throw #2 (cross-store / renamed) | `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:56` | `:56` | Confirmed |
| Unguarded read in `MoveToFolderAsync(string, …)` | `QuickFiler/Controllers/EfcDataModel.cs:289` | `:289` | Confirmed |
| Unguarded read in `OpenOlFolderAsync` | `QuickFiler/Controllers/EfcDataModel.cs:310` | `:310` | Confirmed |
| Unguarded read in `OpenFsFolderAsync` | `QuickFiler/Controllers/EfcDataModel.cs:328` | `:328` | Confirmed |
| `MoveToFolderAsync(string, …)` body | `QuickFiler/Controllers/EfcDataModel.cs:259-297`, no try/catch | `:259-297` | Confirmed |
| Early guard 1 (`MailInfo is null`) | `QuickFiler/Controllers/EfcDataModel.cs:267-270` | `:267-270` | Confirmed |
| Early guard 2 (OneDrive special folder) | `QuickFiler/Controllers/EfcDataModel.cs:277-281` | `:277-281` | Confirmed |

Method signature of the guard: `ArchiveRootPathGuard.RequireResolvedArchiveRoot(string composedArchiveRootPath, string resolvedArchiveFolderPath, Action<string> logDiagnostic)` at
`TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:32-60`.

Throw condition 1 (`:38-45`) fires when *either* `composedArchiveRootPath` or
`resolvedArchiveFolderPath` is null, empty, or whitespace. Throw condition 2 (`:47-57`) fires when
the two are not `OrdinalIgnoreCase`-equal.

**No negative caching — confirmed.** `AppOlObjects.cs:257-264` assigns `_archiveRootPath` only from
the helper's return value inside `if (_archiveRootPath is null)`. On throw the field stays null, so
every subsequent read re-enters the helper and throws again.

### 1.2 Line-number drift (issue body is stale)

| Element | Issue body claim | Verified current location | Drift |
|---|---|---|---|
| `EfcHomeController.ExecuteMovesAsync` | `EfcHomeController.ExecuteMoves.cs:31-46` | `:32-47` | +1 |
| `EfcHomeController.ExecuteMovesCoreAsync` | `:64-84` | `:67-87` | +3 |
| `EfcHomeController.MoveToFolderAsync` | `:86-109` | `:89-112` | +3 |
| `ButtonOK_Click` | `EfcFormController.cs:429-443` | `:460` (one-line expression body) | +31, shape changed |
| `Button.Click` wiring for OK | `EfcFormController.cs:389` | `:418` (`_formViewer.Ok.Click += ButtonOK_Click;`) | +29 |
| "rethrow" | `EfcFormController.cs:441` | **does not exist** | see §1.4 |
| `EfcSelectionGuard.ResolveArchiveRootOrEmpty` | `EfcFormController.cs:708` | **does not exist** | see §2.3 |

### 1.3 Propagation chain, verified frame by frame

1. `QuickFiler/Controllers/EfcDataModel.cs:289` — throw site (property read). No try/catch in the
   enclosing method (`:259-297`).
2. `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:98` — `_dataModel.MoveToFolderAsync(...)`
   inside `EfcHomeController.MoveToFolderAsync` (`:89-112`). **No try.**
3. `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:78` — the awaited call inside
   `ExecuteMovesCoreAsync` (`:67-87`). **No try.**
4. `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:41` — inside `ExecuteMovesAsync`
   (`:32-47`), which is `try { … } finally { ResetExecuteMovesState(); }` at `:39-46`. **No catch.**
   The `finally` releases the `Interlocked` single-move guard (`:62-65`) and the exception continues
   unchanged. Grep for `catch` across the whole file returns zero matches. This frame is the one most
   likely to be mistaken for a handler, and the issue body's warning about it is correct.
5. `QuickFiler/Controllers/EfcFormController.cs:759` — `await _homeController.ExecuteMovesAsync();`
   inside `ActionOkAsync` (`:738-772`). **No try/catch.**
6. `QuickFiler/Controllers/EfcFormController.cs:469` — `await ActionOkAsync();` inside
   `ButtonOkClickAsync` (`:462-475`). **This frame catches.** `catch (System.Exception ex)` at
   `:471-474` calls `BoundaryErrorSink(ex.Message, ex)` at `:473`.

**Explicit answer to the question asked:** no frame between the throw site at `EfcDataModel.cs:289`
and `EfcFormController.ButtonOkClickAsync` catches the exception. The first and only handler is
`EfcFormController.cs:471-474`.

### 1.4 Material correction: the rethrow no longer exists on this branch head

The issue body states that the boundary catch "logs and then **rethrows** at
`QuickFiler/Controllers/EfcFormController.cs:441`". That is **not true of this branch head.**

- `BoundaryErrorSink` is declared at `EfcFormController.cs:128-129` as
  `internal System.Action<string, System.Exception> BoundaryErrorSink { get; set; } = (message, exception) => logger.Error(message, exception);`
  — an injectable seam over the static log4net logger, with no rethrow.
- A repo-wide grep for `throw` inside `QuickFiler/Controllers/EfcFormController.cs` returns exactly
  one executable throw: `throw new NotImplementedException();` at `:767`, in the `ActionOkAsync`
  else-branch for an `_initType` that is neither `Sort` nor `Find`. There is no bare `throw;` and no
  `throw ex;` anywhere in the file.

Consequence for scoping: **the symptom described in the issue's step 5 ("Observe an unhandled
`InvalidOperationException` on the UI thread") is not reproducible at this head.** The exception is
caught and logged. The residual defect is different and is described in §1.5.

### 1.5 The residual defect that *is* present

`ActionOkAsync` (`EfcFormController.cs:738-772`) executes in this order on the Sort path:

- `:756` `_formViewer.Hide();`
- `:759` `await _homeController.ExecuteMovesAsync();`  ← throws here
- `:769` `_formViewer.Dispose();`  ← **skipped**
- `:770` `Cleanup();`  ← **skipped**

So on an unresolvable archive root the user sees the EFC form vanish, the mail is not filed, no
message box appears, the only record is a log4net `Error` entry, and the viewer is left hidden and
undisposed with `Cleanup()` never run. The add-in keeps running (satisfying half of the issue's
Expected Behavior) but produces no user-facing diagnostic (failing the other half) and leaks the
form/session state.

This is a real, user-visible defect and is a sufficient basis for the fix. The acceptance criteria
should be restated against this observed behavior rather than against the stale crash symptom.

### 1.6 Other archive-root reads reachable from the EFC surface

Recorded for completeness; none is in the recommended minimal scope.

| Read | Enclosing method | Handler status |
|---|---|---|
| `EfcFormController.cs:529` | `ButtonCreateClickAsync` (`:498-555`) | caught at `:551-554` → `BoundaryErrorSink` |
| `EfcFormController.cs:539` | `ButtonCreateClickAsync` | same |
| `EfcFormController.cs:836` | `CreateFolderAsync` (`:815-858`) | **no local try/catch**; reached from keyboard `'N'` at `:630` and `:698` via `KbdExecuteAsync` (`:894-898`, no try/catch) → `KeyboardHandler.KeyboardHandler_KeyDownAsync` (`QuickFiler/Controllers/KeyboardHandler.cs:133-148`), whose `catch` at `:141-147` logs only |
| `EfcFormController.cs:846` | `CreateFolderAsync` | same |
| `EfcFormController.cs:987` | `BindBreadcrumbRowsAsync` (`:980-997`) | caught at `:993-996`, logs only |

Every EFC entry point therefore terminates in a log-only sink. There is no remaining unhandled
UI-thread path for this exception on this head.

## 2. Existing degrade-and-report patterns (research question 2)

### 2.1 (a) User-facing diagnostic from a filing operation

Three shapes exist. In descending order of testability:

1. **Injectable action seam — `EfcHomeController.MoveFailureMessageAction`.**
   `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:23-24`:
   `internal Action<string> MoveFailureMessageAction { get; set; } = text => MessageBox.Show(text);`
   Consumed by `HandleMoveResult` (`:125-145`): `if (!result) { MoveFailureMessageAction($"Cannot move to folderpath {selectedFolder}"); return; }` (`:132-136`).
   Already unit-tested at `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs:160-175`
   (`HandleMoveResult_WhenMoveFails_RoutesMessageThroughInjectedAction`).
2. **Direct `MessageBox.Show`** — `EfcDataModel.cs:353-356`, in the `MAPIFolder` overload of
   `MoveToFolderAsync`: `if (!result) { MessageBox.Show($"Cannot move to folderpath {folderpath}"); }`.
   Not injectable, therefore not assertable without a UI.
3. **Log plus message box inside a catch** — `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:95-107`
   (`TryOpenOlFolder`): `catch (System.Exception ex) { logger.Error(ex); MessageBox.Show($"Error opening folder \n{ex.Message}"); }`.

### 2.2 (b) Logging pattern

log4net, obtained per type. The canonical declaration is repeated verbatim across the repo:

- `QuickFiler/Controllers/EfcDataModel.cs:23-25`
- `QuickFiler/Controllers/EfcFormController.cs:123-125`
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:38-40`

`ArchiveRootPathGuard` deliberately does not take a logger; it takes
`Action<string> logDiagnostic` (`ArchiveRootPathGuard.cs:35`) and is wired to `logger.Error` at the
call site (`AppOlObjects.cs:262`). `EfcFormController` layers `BoundaryErrorSink` (`:128-129`) over
the static logger for the same reason.

There is no ad-hoc `Console.WriteLine` in any of these files.

### 2.3 (c) Degrading on an unresolvable required root

**`EfcSelectionGuard.ResolveArchiveRootOrEmpty` does not exist on this branch head.** A repo-wide
`Grep` over `*.cs` for `ResolveArchiveRootOrEmpty` returns zero matches. `EfcSelectionGuard`
(`QuickFiler/Controllers/EfcSelectionGuard.cs`, 79 lines) contains exactly two members:
`IsValidFilingSelection` (`:41-51`) and `IsValidCreationSelection` (`:66-77`). The issue body's
statement that "the cycle-2 partial revert removes that call site" is confirmed, and the revert went
further: the method itself is gone, so there are zero call sites and no dead helper to clean up.

The pattern that *does* exist, and that is the closest possible precedent, is the **adjacent
OneDrive miss in the same method**:

```
// QuickFiler/Controllers/EfcDataModel.cs:277-281
if (!Globals.FS.SpecialFolders.TryGetValue("OneDrive", out var folderRoot))
{
    logger.Warn($"Cannot sort without OneDrive location");
    return false;
}
```

and its two siblings in the Open* methods, which degrade silently:

- `EfcDataModel.cs:301-304` — `OpenOlFolderAsync`, bare `return;`, no log.
- `EfcDataModel.cs:320-323` — `OpenFsFolderAsync`, bare `return;`, no log.

### 2.4 Recommendation for pattern reuse

Reuse §2.3's OneDrive shape for the guarded read, and §2.1 item 1's injectable-action shape for the
user message on the two `Open*` paths. Do not invent a new abstraction.

Rationale: the OneDrive guard is in the same method, guards the same class of condition (a required
filing root that could not be resolved), returns the same `false`, and its `false` already reaches
the user through `HandleMoveResult` → `MoveFailureMessageAction`. Matching it satisfies the
"match existing style" requirement of the General Code Change Policy §7.1 with the smallest diff.

## 3. Return-contract impact (research question 3)

### 3.1 `EfcDataModel.MoveToFolderAsync(string, bool, bool, bool, bool)` → `Task<bool>` (`:259-297`)

| Caller | Location | What it does with the result |
|---|---|---|
| `EfcHomeController.MoveToFolderAsync` | `EfcHomeController.ExecuteMoves.cs:98` | returns the `Task<bool>` unchanged to `:78` |
| `EfcDataModel.MoveToFolderAsync(MAPIFolder, …)` | `EfcDataModel.cs:346` | `if (!result) MessageBox.Show($"Cannot move to folderpath {folderpath}")` at `:353-356` |

The first path continues: `ExecuteMovesCoreAsync:78` assigns `result`, then `:86` calls
`HandleMoveResult(result, …)`, which at `:132-136` routes a false result to
`MoveFailureMessageAction`.

**Assessment:** returning `false` instead of throwing is already fully supported. Both callers
surface a message to the user. **No caller silently swallows the bool.** Returning `false` is
strictly better than the current behavior because it additionally allows `ActionOkAsync:769-770`
(`Dispose()` / `Cleanup()`) to run, which the throw currently skips.

One accuracy caveat: the existing message text, "Cannot move to folderpath {selectedFolder}",
attributes the failure to the destination folder rather than to the archive root. It is not wrong
(the move did not happen) but it is not cause-specific. Changing it is optional; see §5.3.

### 3.2 `EfcDataModel.OpenOlFolderAsync(string)` → `Task` (`:299-316`)

| Caller | Location | What it does |
|---|---|---|
| `EfcHomeController.OpenOlFolderAsync` | `EfcHomeController.cs:429` (method `:427-430`) | `await`s; returns `Task` |
| `EfcFormController.ActionOkAsync` | `EfcFormController.cs:763` | `await`s in the `InitTypeEnum.Find` branch; no result to inspect |

### 3.3 `EfcDataModel.OpenFsFolderAsync(string)` → `Task` (`:318-334`)

| Caller | Location | What it does |
|---|---|---|
| `EfcHomeController.OpenFsFolderAsync` | `EfcHomeController.cs:434` (method `:432-435`) | `await`s; returns `Task` |
| `EfcFormController.ButtonCreateClickAsync` | `EfcFormController.cs:513` | `await`s in the `Find` branch |
| `EfcFormController.CreateFolderAsync` | `EfcFormController.cs:823` | `await`s in the `Find` branch |

Test-only caller of both: `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:214-215`.

**Assessment:** these two return `Task`, so a failure result is unobservable today. Widening them to
`Task<bool>` would require changing the two `internal` `EfcHomeController` wrappers and would leave
**all five** production call sites discarding the value — that is, it would *manufacture* the
silent-swallow problem the question asks about, in five places at once. Reject the signature change.

Because `Open*` failures cannot be reported through a return value, the diagnostic must be emitted at
the point of failure. Note that unlike the Sort path, a silent `return` from `Open*` still lets
`ActionOkAsync:769-770` run, so nothing leaks; the only defect is the absent user feedback.

## 4. Testability seam (research question 4)

### 4.1 `Globals` in `EfcDataModel` is not the static COM accessor

The delegation prompt's premise that "`Globals.Ol` is a static COM-bound accessor" does not hold for
this class. In `EfcDataModel`, `Globals` resolves to the **instance property** declared at
`EfcDataModel.cs:148-153`:

```
private IApplicationGlobals _globals;
public IApplicationGlobals Globals { get => _globals; protected set => _globals = value; }
```

It is assigned from the constructor parameter at `:57` (public ctor) and `:85` (private ctor). The
read chain is therefore `IApplicationGlobals.Ol` → `IOlObjects.ArchiveRootPath`, and
`IOlObjects.ArchiveRootPath` is an interface member at
`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:15`.

**Conclusion: no new seam is required.** Moq can already make the getter throw:
`olObjects.SetupGet(x => x.ArchiveRootPath).Throws(new InvalidOperationException(...))`. This is the
same technique already used to make it *return* in
`TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs:118-130`.

### 4.2 `EfcDataModel` is already unit-testable today

Verified facts:

- `QuickFiler/Properties/AssemblyInfo.cs:5` — `[assembly: InternalsVisibleTo("QuickFiler.Test")]`,
  so the `internal` class and its `internal` members are reachable.
- `QuickFiler.Test/Controllers/EfcDataModelTests.cs:220-228` builds
  `Mock<IApplicationGlobals>(MockBehavior.Strict)` with `Mock<IOlObjects>(MockBehavior.Strict)`.
- `EfcDataModelTests.cs:166-171` and `:200-205` construct the real object via the public constructor
  `new EfcDataModel(globals.Object, mailItem.Object, new CancellationTokenSource(), CancellationToken.None)`
  and drive it to a loaded `ConversationResolver` with only Moq'd interop objects.
- `EfcDataModel.cs:233` — `MailInfo => ConversationResolver?.MailHelper`. The public ctor path sets
  `ConversationResolver` at `:67`, and `QuickFiler/Helper Classes/ConversationResolver.cs:82` assigns
  `MailHelper = new MailItemHelper(mailItem, _globals)`. So `MailInfo` is non-null after that ctor,
  which is what the `:267-270` guard requires in order to reach the archive-root read.
- `IFileSystemFolderPaths.SpecialFolders` is a concrete `ConcurrentDictionary<string, string>`
  (`UtilitiesCS/Interfaces/IGlobals/IFileSystemFolderPaths.cs:7`), so seeding `"OneDrive"` to let the
  `:277-281` guard pass is trivial.
- With `moveConversation: false`, `:273-275` takes the `new List<MailItemHelper> { MailInfo }` branch
  and never touches `ConversationResolver.ConversationInfo`.

The RED test therefore reaches `:289` and observes the throw, without a live Outlook process and
without constructing `EmailFiler` (`:293`), which is only reached after the guarded read.

### 4.3 Comparable harness already in the repo

`QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs` contains a purpose-built probe worth
copying rather than reinventing:

- `LifecycleProbe` (`:220-372`)
- `FakeApplicationGlobals` (`:374-403`) — hand-written `IApplicationGlobals` stub
- `FakeFileSystemFolderPaths` (`:405-433`) — counts `SpecialFolders` reads
- `LifecycleProbe.CreateDataModelWithGlobals` (`:350-358`) — real `EfcDataModel` over fake globals
- `CreateUninitialized<T>` (`:367-371`) — `FormatterServices.GetUninitializedObject`, the repo's
  established ctor-free construction technique
- `OpenFolderMethods_DelegateToDataModelWithoutExternalServices` (`:207-218`) — already exercises
  `OpenOlFolderAsync` and `OpenFsFolderAsync` end-to-end with no external services

Caveat: `FakeApplicationGlobals.Ol` returns `null` (`:388`). To use this probe for the archive-root
tests it must either gain a fake `IOlObjects` or be replaced by the Moq-based globals from
`EfcDataModelTests.CreateGlobals` (`:220-228`). Prefer the Moq route for the new tests, because
`SetupGet(...).Throws(...)` expresses the failure directly and `Times`-verification is available.

### 4.4 Test project and directory

**Recommended location:** `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`

Verified basis:
- `EfcDataModel` tests already live at `QuickFiler.Test/Controllers/EfcDataModelTests.cs` and
  `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`.
- Both are registered explicitly in `QuickFiler.Test/QuickFiler.Test.csproj` at lines 114 and 115.
- `QuickFiler.csproj:2` shows the legacy non-SDK project format
  (`ToolsVersion="15.0"`, `Microsoft.Common.props` import, `TargetFrameworkVersion v4.8.1`); the test
  project enumerates every `.cs` the same way. **A new test file will not compile until an explicit
  `<Compile Include="Controllers\EfcDataModelArchiveRootTests.cs" />` is added to
  `QuickFiler.Test.csproj`.** This is a frequent omission and should be an explicit plan task.
- Namespace precedent is inconsistent between the two existing files
  (`QuickFiler.Controllers.Tests` in `EfcDataModelTests.cs:13`, `QuickFiler.Test.Controllers` in
  `EfcDataModelIssue614Tests.cs:8`). Prefer `QuickFiler.Test.Controllers`, matching the newer file.

**Policy conflict that must be surfaced, not silently resolved.**
`.claude/rules/general-unit-test.md` § "Test File Location" states that test files must live in a
`tests/` tree mirroring production source and that colocation is "not permitted". Verified against
the tree: `tests/` contains only five PowerShell Pester files under `tests/scripts/vscode/`. Every
C# test in this repository lives in a sibling `<Project>.Test/` project
(`QuickFiler.Test`, `TaskMaster.Test`, `UtilitiesCS.Test`, `SVGControl.Test`). The C# Unit Test
Policy embedded in `CLAUDE.md` — which the "Policy Compliance Order" section ranks above the rules
files — imposes no `tests/` layout requirement.

Recommendation: follow the repository's actual C# convention (`QuickFiler.Test/Controllers/`), and
record the deviation from `.claude/rules/general-unit-test.md` explicitly in the plan and PR. Per
`CLAUDE.md` ("If you encounter **any** conflicting instructions, halt and notify the user"), the
orchestrator should surface this conflict rather than have an executor decide it silently.

## 5. Recommended approach

### 5.1 Design

Add one private helper to `EfcDataModel` and route the three reads through it.

```
private bool TryGetArchiveRoot(out string archiveRoot)
{
    try
    {
        archiveRoot = Globals.Ol.ArchiveRootPath;
        return true;
    }
    catch (InvalidOperationException ex)
    {
        archiveRoot = null;
        logger.Warn("Cannot file without a resolvable Outlook archive root.", ex);
        return false;
    }
}
```

Call-site changes:

- `EfcDataModel.cs:289` — hoist to a guard beside the OneDrive guard:
  `if (!TryGetArchiveRoot(out var olAncestor)) { return false; }`, then `OlAncestor = olAncestor`.
- `EfcDataModel.cs:310` — `if (!TryGetArchiveRoot(out var olAncestor)) { UserDiagnosticAction(...); return; }`
- `EfcDataModel.cs:328` — same shape.

Plus one injectable report seam, mirroring `EfcHomeController.MoveFailureMessageAction`
(`EfcHomeController.ExecuteMoves.cs:23-24`) exactly:

```
internal System.Action<string> UserDiagnosticAction { get; set; } = text => MessageBox.Show(text);
```

`System.Windows.Forms` is already imported at `EfcDataModel.cs:10` and `MessageBox.Show` is already
used at `:355`, so this adds no new dependency.

### 5.2 Ordering constraint (verified, load-bearing)

The archive-root guard must be placed **after** the existing OneDrive guard in all three methods, not
before it. `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:217` asserts
`probe.FileSystem.SpecialFoldersAccessCount.Should().Be(2)` after calling `OpenOlFolderAsync` and
`OpenFsFolderAsync` once each. Inserting an earlier-returning guard ahead of the `SpecialFolders`
read would drop that count and break an existing test that is part of the spec.

### 5.3 Exception breadth

Catch `InvalidOperationException` only. That is precisely the documented contract of the property
(`AppOlObjects.cs:250-252`) and of the guard (`ArchiveRootPathGuard.cs:30-31`), and it keeps the
change minimal per the Bugfix Workflow.

Recorded residual, **not** in scope: the getter also evaluates `Root.FolderPath` (`AppOlObjects.cs:260`)
and `ArchiveRoot?.FolderPath` (`:261`), where `ArchiveRoot` (`:270`) lazily runs `LoadArchiveRoot`
(`:272-276`) → `FolderPredictor.GetFolder`. Those are live COM calls and can raise
`System.Runtime.InteropServices.COMException`, which the narrow catch will not absorb. Recommend
promoting this to a separate issue rather than widening the catch here.

### 5.4 Message text

Leave `HandleMoveResult`'s existing "Cannot move to folderpath {selectedFolder}" unchanged for the
Sort path (it already fires and requires no edit), and let `UserDiagnosticAction` carry a
cause-specific, non-identifying message on the two `Open*` paths. Any user-visible message must reuse
the redaction discipline already established for this failure class: `ArchiveRootPathGuard`'s two
constants (`ArchiveRootPathGuard.cs:13-17`) deliberately name the rule and withhold the path because
it carries a mailbox address (#602). Do not interpolate the archive root or the folder path into a
new message.

### 5.5 File-size headroom

`QuickFiler/Controllers/EfcDataModel.cs` is currently 423 lines. The General Code Change Policy caps
any file at 500 lines. The proposed change adds roughly 30-45 lines, landing near 455-470. It fits,
but with limited headroom; any further growth of this file should trigger a split.

### 5.6 Rejected alternatives (brief)

- **Make `AppOlObjects.ArchiveRootPath` return empty instead of throwing.** Rejected: it reverses the
  deliberate #614 D6 decision, changes the contract for every consumer of `IOlObjects.ArchiveRootPath`
  (production reads exist in `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs:67,95,107,109,228`,
  `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:304,375,686,748,852,910`,
  `FolderConverter.cs:334,339`, `FolderNavigator.cs:48`, `MailItemHelper.Loading.cs:124`,
  `MeetingItemHelper.cs:262`, `SortEmail.cs:1337`), reintroduces the unverified-root bug class, and is
  pinned by `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs:64-111`.
- **Reinstate an `EfcSelectionGuard.ResolveArchiveRootOrEmpty`-style empty-root degrade.** Rejected: an
  empty `OlAncestor` flows into `EmailFilerConfig` (`EfcDataModel.cs:282-291`) and downstream stem
  composition, which is the #614 store-root-leak failure mode. Cycle-2 already deleted this helper.
- **Catch at `EfcFormController.ActionOkAsync`.** Rejected: the form is already hidden at `:756`
  before the failure, so the catch cannot restore a clean state without extra work; and it covers
  neither the `Find` path nor the two `Open*` reads.
- **Widen `OpenOlFolderAsync` / `OpenFsFolderAsync` to `Task<bool>`.** Rejected: see §3.2 — all five
  production call sites discard, so the change creates silent swallow rather than removing it.

## 6. `async void` rethrow scope decision (research question 5)

### 6.1 Complete inventory in `EfcFormController.cs`

| `async void` handler | Line | Delegates to | `catch` | Sink call | Rethrow? |
|---|---|---|---|---|---|
| `ButtonCancel_Click` | `:442-443` | `ButtonCancelClickAsync` `:445-458` | `:454-457` | `:456` | No |
| `ButtonOK_Click` | `:460` | `ButtonOkClickAsync` `:462-475` | `:471-474` | `:473` | No |
| `ButtonRefresh_Click` | `:477-478` | `ButtonRefreshClickAsync` `:480-493` | `:489-492` | `:491` | No |
| `ButtonCreate_Click` | `:495-496` | `ButtonCreateClickAsync` `:498-555` | `:551-554` | `:553` | No |
| `ButtonDelete_Click` | `:557-558` | `ButtonDeleteClickAsync` `:560-570` | `:566-569` | `:568` | No |

Related non-`async void` boundaries in the same file, for completeness:
`PopulateFolderCombobox` `:1119-1140` (catch `:1136-1139`, sink `:1138`) and
`BindBreadcrumbRowsAsync` `:980-997` (catches `:989-992` and `:993-996`, `logger` directly).

### 6.2 Decision

**No work item. The requested change is already present on this branch head.** Every one of the five
`async void` boundaries logs through `BoundaryErrorSink` and returns; none rethrows. The issue's
suggestion to reconsider the rethrow at `EfcFormController.cs:441` refers to code that does not
exist here. Neither an in-scope change nor a separate issue for "remove the rethrow" is warranted.

Record the correction in the issue rather than acting on it.

### 6.3 Blast radius, had a change been needed

For completeness: `BoundaryErrorSink` is `internal` with a settable default, and it is already
asserted in three tests (`QuickFiler.Test/Controllers/EfcFormControllerTests.cs:261`, `:283-290`,
`:310`). Any change to the five catch bodies would be confined to `EfcFormController.cs` plus those
three assertions. The wider `async void` surface in the QuickFiler assembly (13 further sites, listed
by grep in `QfcFormController.EventHandlers.cs`, `QfcItemController.EventHandlers.cs`,
`QfcItemController.EventWiring.cs`, `QfcDatamodel.cs:173`, `KeyboardHandler.cs:133/238/266`,
`BreadcrumbBridgeRouter.cs:291`, `ConversationResolver.Loading.cs:304`) is not reached by this issue
and must not be swept in.

**Separately promotable observation (report only, do not fix here).** All five EFC boundary sinks are
log-only, so *any* unexpected exception on the OK path now produces a silently vanished form with no
user feedback and with `Dispose()`/`Cleanup()` skipped. That is a generic boundary-behavior gap
distinct from #638's archive-root cause and should be promoted to its own issue.

## 7. Coverage and toolchain (research question 6)

### 7.1 Projects touched

| Project | File | Role |
|---|---|---|
| `QuickFiler` | `QuickFiler/Controllers/EfcDataModel.cs` | production change |
| `QuickFiler.Test` | `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` (new) + `QuickFiler.Test.csproj` | tests |

`TaskMaster` (`AppOlObjects.cs`, `ArchiveRootPathGuard.cs`) and `UtilitiesCS` are **not** modified by
the recommended approach.

### 7.2 Tier classification

`quality-tiers.yml` **does not exist** anywhere in this repository. A `Glob` for `quality-tiers.yml`
across the whole tree returns zero matches, and a `Grep` for `tier-classification` / `quality-tiers`
under `.github/` returns zero matches, so the CI stage that `.claude/rules/quality-tiers.md`
describes is not present either. The T1–T4 tier system therefore has no source of truth in this repo
and no tier can be cited for `QuickFiler`. Record this as an unmet documented precondition; it is not
a blocker for this change, and this agent recommends **not** creating the file as part of #638.

### 7.3 Coverage exemption interaction

- `coverage.config` (repo root) excludes only third-party modules: `Deedle`, `FSharp`, `Castle.Core`,
  `FluentAssertions`, `Moq`, `Microsoft.Testing`, `MSTest` (`coverage.config:12-22`). No first-party
  assembly is excluded, and `QuickFiler` is not excluded.
- `QuickFiler/Controllers/EfcDataModel.cs` carries **no** `[ExcludeFromCodeCoverage]` attribute; the
  file was read end to end.
- The COM/VSTO/WinForms exemption in `CLAUDE.md` § UT2 does not apply: `EfcDataModel` is not
  form-derived, is not Designer-generated, is not a VSTO lifecycle class, and takes its Outlook
  dependency through the injectable `IApplicationGlobals` seam — which `CLAUDE.md` explicitly names
  as the disqualifier ("without an injectable seam").

Consequence: the changed lines are in the measured denominator and must be covered. All of them are
coverable by the tests in §8.

Note on evidence interpretation, from prior work in this repo: an exempt member emits no `<method>`
element in the Cobertura output at all, so absence is the exemption signal, not a zero rate.

### 7.4 Toolchain commands (verbatim from `CLAUDE.md` § "C# Toolchain (run in this exact order)")

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Run `dotnet tool restore` once per worktree before the first csharpier invocation. Do not add
`/p:Nullable=enable` to step 3 and do not substitute `/t:Build` for `/t:Rebuild`; `CLAUDE.md`
documents both as load-bearing.

CI parity notes verified in this tree:
- `.github/workflows/_format-check.yml:41` runs `dotnet csharpier check .`
- `.github/workflows/_build-nullable.yml:54-59` runs the `TreatWarningsAsErrors` rebuild
- `.github/workflows/_mstest-coverage.yml:70-83` discovers `*.Test.dll` recursively and runs
  `vstest.console.exe $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`

Two consequences of that last line: (a) local runs should add `/InIsolation` and exclude
`.claude/worktrees` assemblies to match CI, and (b) **CI already excludes any test marked
`[TestCategory("LiveOutlook")]`**, which is directly relevant to §9.

## 8. Testing implications (no test code written)

All proposed tests are MSTest + Moq + FluentAssertions, deterministic, no filesystem, no temp files,
no live Outlook.

Regression (RED before the fix, GREEN after) — `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`:

1. `MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing`
   Arrange a `Mock<IOlObjects>` whose `ArchiveRootPath` getter throws `InvalidOperationException`, a
   `SpecialFolders` dictionary containing `"OneDrive"`, and a constructed `EfcDataModel` with a
   non-null `MailInfo`. Act: `await MoveToFolderAsync(stem, …, moveConversation: false)`.
   Assert: returns `false`; does not throw. Fails today with `InvalidOperationException`.
2. `OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns`
   Assert: does not throw, and the injected `UserDiagnosticAction` received exactly one message that
   does not contain the mailbox address or archive path.
3. `OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns` — same shape.

Boundary and edge coverage:

4. `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` — `VerifyGet(..., Times.Once)`,
   pinning that the fix does not double-read the (COM-backed) property.
5. `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot` — `Times.Never`,
   pinning guard order at `:267-270`.
6. `MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot` — `Times.Never`,
   pinning the §5.2 ordering constraint from the production side as well as the test side.
7. `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress` — redaction, matching
   the discipline already tested at
   `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs:48-59`.

Existing tests that must keep passing without modification (treat as spec):
- `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:207-218` (the
  `SpecialFoldersAccessCount == 2` assertion — see §5.2)
- `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs:160-175`
- `QuickFiler.Test/Controllers/EfcDataModelTests.cs` (all)
- `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs` (all)
- `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs` (all — the throw contract
  must remain intact)

Fail-before evidence: test 1 is a genuine failing-run artifact, so no fail-before exception dossier
is required for this issue.

## 9. Automation Feasibility

**Verdict: fully automatable. No human interaction with a third-party UI is required for the fix, its
verification, or its regression tests.**

### 9.1 Requirement-by-requirement assessment

| Requirement | Needs a live third-party UI? | Resolution |
|---|---|---|
| Reading and verifying the trace | No | Done in this session with file reads only |
| Implementing the guard in `EfcDataModel.cs` | No | Pure source edit |
| Regression tests 1-7 (§8) | No | `IOlObjects.ArchiveRootPath` is an interface member (`IOlObjects.cs:15`); Moq raises the exact `InvalidOperationException` the guard produces. Verified: existing tests already construct `EfcDataModel` against `Mock<IApplicationGlobals>` with no Outlook process (`EfcDataModelTests.cs:166-171`, `:200-205`) |
| Toolchain steps 1-4 (§7.4) | No | Command-line only |
| Coverage evidence | No | `vstest.console.exe … /EnableCodeCoverage` |
| The issue's literal repro steps 1-5 | **Yes** | See §9.2 |

### 9.2 The one unautomatable item, and how it is removed by scope change

The issue's "Steps to Reproduce" require an Outlook profile whose archive folder does not resolve to
the default store's `Archive` folder (no `Archive` folder, an archive in a second store, or a renamed
archive), then opening the EFC form with `InitTypeEnum.Sort` and pressing OK. That requires Outlook
desktop plus a specially constructed mail profile. It cannot be automated in CI, and CI already
declines to try: `.github/workflows/_mstest-coverage.yml:83` filters with
`/TestCaseFilter:"TestCategory!=LiveOutlook"`.

**This requirement is removed by scope change, not by exception.** The defect is provable at the unit
level because the failure is injected at an interface boundary, not at a COM boundary:

- The profile condition's *only* effect on the code under test is that
  `IOlObjects.ArchiveRootPath` throws `InvalidOperationException` with one of two fixed messages
  (`ArchiveRootPathGuard.cs:13-17`, thrown at `:44` and `:56`).
- Both throw conditions are already unit-tested in isolation, without Outlook, at
  `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs:64-111`. The pure decision
  helper was extracted for exactly this reason (`ArchiveRootPathGuard.cs:5-10`).
- `EfcDataModel` consumes that property only through `IApplicationGlobals` / `IOlObjects` (§4.1), so
  a Moq `SetupGet(...).Throws(...)` is behaviorally indistinguishable from the live profile at the
  seam under test.

Therefore the regression test proves the defect and the fix through the seam, and the live-profile
repro is downgraded from a required verification step to an optional manual confirmation.

### 9.3 Recommended handling of the manual step

Record the live-profile walkthrough as an **optional post-merge manual confirmation**, owned by the
maintainer, and do not make any acceptance criterion depend on it. Do not add a
`[TestCategory("LiveOutlook")]` test for this issue: it would not run in CI, and the unit-level proof
is strictly stronger because it is deterministic and covers both throw conditions.

### 9.4 Blockers

- **Automation blockers: none.**
- **Exceptions required: none.**
- **Halt conditions: none.**

Two non-blocking items require an orchestrator decision before implementation:
1. The `tests/` versus `<Project>.Test/` policy conflict in §4.4 (`CLAUDE.md` directs a halt-and-notify
   on conflicting instructions).
2. The acceptance criteria must be restated against the observed behavior in §1.5 (silent swallow,
   hidden and undisposed form) rather than the stale "unhandled UI-thread exception" symptom, or the
   AC will be unsatisfiable by any change.

## 10. Items recommended for promotion to separate issues

1. **COMException from the archive-root getter's COM calls** is not absorbed by the recommended
   narrow catch (§5.3). `AppOlObjects.cs:260-261` dereferences `Root.FolderPath` and lazily loads
   `ArchiveRoot` via `FolderPredictor.GetFolder` (`:272-276`).
2. **EFC boundary sinks are log-only** (§6.2), so any unexpected exception on the OK path leaves a
   hidden, undisposed form with no user feedback and `Cleanup()` never run.
3. **`quality-tiers.yml` is absent** (§7.2) while `.claude/rules/quality-tiers.md` and
   `.claude/rules/general-unit-test.md` both treat it as the source of truth for a CI gate that does
   not exist in `.github/workflows/`.
4. **`.claude/rules/general-unit-test.md` "Test File Location" contradicts the repository's actual
   C# test layout** (§4.4). Note that `.claude/**` in this repository is push-down-owned from an
   upstream repo, so the correction belongs upstream rather than here.

## 11. Summary of corrections to the issue body

| Issue body claim | Verified status |
|---|---|
| Throw site, guard conditions, three unguarded reads, both early guards | Accurate, line numbers unchanged |
| No negative caching | Accurate |
| `ExecuteMoves*` chain has no catch | Accurate; line numbers drifted by +1 to +3 |
| `EfcFormController.ActionOkAsync` has no try/catch | Accurate (`:738-772`) |
| Catch "logs and then **rethrows** at `EfcFormController.cs:441`" | **False at this head.** No rethrow exists anywhere in the file; the catch at `:471-474` calls `BoundaryErrorSink` and returns |
| "unhandled `InvalidOperationException` on the UI thread" | **Not reproducible at this head.** The residual is a silent swallow with a hidden, undisposed form |
| `EfcSelectionGuard.ResolveArchiveRootOrEmpty` at `EfcFormController.cs:708` | **Removed entirely.** Zero occurrences repo-wide; `EfcSelectionGuard` has only two predicates |
| `ButtonOK_Click` at `:429-443`, wired at `:389` | Drifted to `:460`, wired at `:418` |
