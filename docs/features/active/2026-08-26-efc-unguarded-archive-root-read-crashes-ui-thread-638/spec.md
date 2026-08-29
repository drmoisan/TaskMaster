# 2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread (Spec)

- **Issue:** #638
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-29T08-40
- **Status:** Ready for Planning
- **Version:** 1.0

> Work Mode for issue #638 is `full-bug`. Per `.claude/skills/acceptance-criteria-tracking/SKILL.md`,
> this file is the **sole** authoritative acceptance-criteria source for this issue. No
> `user-story.md` exists or should be created for #638.

## Context

`QuickFiler/Controllers/EfcDataModel.cs` reads `Globals.Ol.ArchiveRootPath` at three call sites with
no guard. When the Outlook archive root cannot be resolved, that read throws
`InvalidOperationException`, which unwinds out of the filing operation and is absorbed by a log-only
fault-boundary sink several frames later. The user is left with a hidden, undisposed form, no
message, and no filed mail.

All line and shape citations in this section were re-derived during spec authoring from the working
tree of a dedicated agent worktree checked out on branch
`bug/efc-unguarded-archive-root-read-crashes-ui-thread-638`, branched from `origin/main` at
`ecdb1c84ba8541ab67042985919cfed4df768c01`. The branch name and that merge-base SHA are the two facts
required to reproduce the citations; the worktree's filesystem location is not recorded here.

**Throw site.** `TaskMaster/AppGlobals/AppOlObjects.cs:253-267` declares the `ArchiveRootPath`
getter. It delegates to `ArchiveRootPathGuard.RequireResolvedArchiveRoot` at
`TaskMaster/AppGlobals/AppOlObjects.cs:259-263`. That helper
(`TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:32-60`) throws `InvalidOperationException`
unconditionally on either of two conditions: `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:44` when
either the composed or the resolved path is null, empty, or whitespace (no `Archive` folder resolves
in the default store), and `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:56` when the two are not
`OrdinalIgnoreCase`-equal (archive in a second store, renamed, or a delegate mailbox). Both messages
are the redacted constants at `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:13-17`, which name the
rule and withhold the path because it can carry a mailbox address (#602).

**No negative caching.** The backing field `_archiveRootPath` at
`TaskMaster/AppGlobals/AppOlObjects.cs:237` is assigned only from the helper's return value, inside
`if (_archiveRootPath is null)` at `TaskMaster/AppGlobals/AppOlObjects.cs:257-264`. On throw the field
stays null, so every subsequent read re-enters the helper and throws again. For an affected profile
the failure is permanent and reproduces on every attempt.

**Unguarded reads.** All three are `OlAncestor = Globals.Ol.ArchiveRootPath` inside an
`EmailFilerConfig` initializer:

- `QuickFiler/Controllers/EfcDataModel.cs:289` in `MoveToFolderAsync(string, bool, bool, bool, bool)`
  (`QuickFiler/Controllers/EfcDataModel.cs:259-297`).
- `QuickFiler/Controllers/EfcDataModel.cs:310` in `OpenOlFolderAsync(string)`
  (`QuickFiler/Controllers/EfcDataModel.cs:299-316`).
- `QuickFiler/Controllers/EfcDataModel.cs:328` in `OpenFsFolderAsync(string)`
  (`QuickFiler/Controllers/EfcDataModel.cs:318-334`).

`QuickFiler/Controllers/EfcDataModel.cs` contains exactly one `catch` in the whole file, at
`QuickFiler/Controllers/EfcDataModel.cs:249`, inside `TryGetFirstInSelection`. None of the three
reads is inside a `try`. In `MoveToFolderAsync` the read is reached unconditionally once the
`MailInfo is null` guard at `QuickFiler/Controllers/EfcDataModel.cs:267-270` and the OneDrive
`SpecialFolders` guard at `QuickFiler/Controllers/EfcDataModel.cs:277-281` both pass. In the two
`Open*` methods the read is reached once the OneDrive guards at
`QuickFiler/Controllers/EfcDataModel.cs:301-304` and
`QuickFiler/Controllers/EfcDataModel.cs:320-323` pass; both of those degrade with a bare `return;`
and no log.

**Propagation chain, no handler until the boundary.**

1. `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:98` calls
   `_dataModel.MoveToFolderAsync(...)` inside `EfcHomeController.MoveToFolderAsync`
   (`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:89-112`). No `try`.
2. `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:78` awaits it inside
   `ExecuteMovesCoreAsync` (`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:67-87`).
   No `try`.
3. `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:41` awaits that inside
   `ExecuteMovesAsync` (`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:32-47`), which is
   `try { ... } finally { ResetExecuteMovesState(); }` at
   `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:39-46` with **no catch**. The `finally`
   releases the `Interlocked` single-move guard
   (`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:62-65`) and the exception continues
   unchanged. This frame is the one most likely to be mistaken for a handler; it is not one.
4. `QuickFiler/Controllers/EfcFormController.cs:759` awaits `ExecuteMovesAsync()` inside
   `ActionOkAsync` (`QuickFiler/Controllers/EfcFormController.cs:738-772`). No `try`.
5. `QuickFiler/Controllers/EfcFormController.cs:469` awaits `ActionOkAsync()` inside
   `ButtonOkClickAsync` (`QuickFiler/Controllers/EfcFormController.cs:462-475`). **This frame
   catches**, at `QuickFiler/Controllers/EfcFormController.cs:471-474`, and calls
   `BoundaryErrorSink(ex.Message, ex)` at `QuickFiler/Controllers/EfcFormController.cs:473`.

**Two claims in the issue body are false on this branch head; both were re-verified during spec
authoring and are settled.**

1. **There is no rethrow.** `BoundaryErrorSink` is declared at
   `QuickFiler/Controllers/EfcFormController.cs:128-129` as
   `internal System.Action<string, System.Exception> BoundaryErrorSink { get; set; } = (message, exception) => logger.Error(message, exception);`
   — an injectable seam over the static log4net logger, with no rethrow. A grep for `throw` across
   `QuickFiler/Controllers/EfcFormController.cs` returns exactly one executable throw,
   `throw new NotImplementedException();` at `QuickFiler/Controllers/EfcFormController.cs:767`, in the
   `ActionOkAsync` else-branch for an `_initType` that is neither `Sort` nor `Find`. There is no bare
   `throw;` and no `throw ex;` anywhere in the file. The issue body's citation of a rethrow at
   `:441` refers to code that does not exist here. Consequently the issue's repro step 5 ("Observe an
   unhandled `InvalidOperationException` on the UI thread") is **not reproducible** at this head, and
   any acceptance criterion asserting it would be unsatisfiable.
2. **`EfcSelectionGuard.ResolveArchiveRootOrEmpty` no longer exists.** A repository-wide grep returns
   zero occurrences in any `.cs` file; the only hits are prose in issue and feature documents.
   `QuickFiler/Controllers/EfcSelectionGuard.cs` contains exactly two members,
   `IsValidFilingSelection` and `IsValidCreationSelection`. The cycle-2 revert removed the method
   itself, not merely its call site, so there is no dead helper to clean up.

**The residual defect that is present.** `ActionOkAsync`
(`QuickFiler/Controllers/EfcFormController.cs:738-772`) executes in this order on the `Sort` path:
`_formViewer.Hide();` at `QuickFiler/Controllers/EfcFormController.cs:756`, then
`await _homeController.ExecuteMovesAsync();` at `QuickFiler/Controllers/EfcFormController.cs:759`
(where the exception originates), then `_formViewer.Dispose();` at
`QuickFiler/Controllers/EfcFormController.cs:769` and `Cleanup();` at
`QuickFiler/Controllers/EfcFormController.cs:770`. The throw skips the last two. The observable
outcome is therefore a **silent swallow**: the form has already been hidden, the mail is not filed,
no message box appears, the only record is a log4net `Error` entry, and the viewer is left hidden and
undisposed with `Cleanup()` never run. The add-in keeps running, which satisfies half of the issue's
Expected Behavior, but it produces no user-facing diagnostic and leaks form and session state.

**Relationship to other issues.** This is distinct from issue #637, which covers producer-side
normalization of rooted paths at `BreadcrumbBridgeRouter.SelectRow`. This defect is an archive-root
*resolution failure*, not a path-rootedness problem, and it fires even for a well-formed
archive-relative stem. It is pre-existing rather than introduced by issue #614.

Environment:
- OS/version: Windows 11 Pro 10.0.26200; .NET Framework 4.8.1 VSTO add-in (legacy non-SDK projects).
- Command/flags used: static reachability tracing plus file reads against the worktree named above;
  no production source file was modified during analysis.
- Data source or fixture: repository source at
  `ecdb1c84ba8541ab67042985919cfed4df768c01`, plus the research artifact
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/research/2026-08-29T08-05-archive-root-guard-research.md`.

## Repro & Evidence

The steps below describe behavior that is actually reachable on this branch head. The profile
precondition is retained from the issue body; the observable outcome is restated as the verified
silent swallow. Step 5 of the issue body ("unhandled `InvalidOperationException` on the UI thread")
is superseded — see the Context section.

Steps to Reproduce (manual, optional confirmation only — not a required verification step):
1. Use an Outlook profile whose archive folder does not resolve to the default store's `Archive`
   folder. Any of three shapes reproduces it: no `Archive` folder in the default store (throw
   condition at `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:44`); an archive that lives in a
   second store, for example a delegate mailbox (throw condition at
   `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:56`); or a renamed archive folder (same condition).
2. Open the QuickFiler email-filer form with `InitTypeEnum.Sort`.
3. Select a valid archive-relative destination stem such as `Clients\North` — a non-rooted value that
   `EfcSelectionGuard.IsValidFilingSelection`
   (`QuickFiler/Controllers/EfcSelectionGuard.cs:41-51`) accepts.
4. Press OK.
5. Observe that the form disappears immediately and nothing further happens: no message box, no
   filed mail, no error dialog. The window vanishes because
   `QuickFiler/Controllers/EfcFormController.cs:756` hides it before the failure.
6. Inspect the log4net output. It contains one `Error` entry carrying the redacted rule text from
   `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:13-17`, written through `BoundaryErrorSink`.
7. Repeat steps 2-5. The failure recurs on every attempt, because the null backing field at
   `TaskMaster/AppGlobals/AppOlObjects.cs:237` is never assigned and there is no negative caching.

Expected:
An unresolvable archive root produces a clear, user-facing diagnostic, the filing operation returns a
failure result rather than throwing, and the form is disposed and cleaned up normally. The redacted
message text is preserved: no archive root path and no mailbox address appears in any user-visible
string.

Actual:
`Globals.Ol.ArchiveRootPath` throws `InvalidOperationException` at
`QuickFiler/Controllers/EfcDataModel.cs:289`. The exception unwinds through four frames that do not
catch it and is absorbed at `QuickFiler/Controllers/EfcFormController.cs:471-474`, which logs and
returns. The user sees the form vanish with no diagnostic;
`QuickFiler/Controllers/EfcFormController.cs:769-770` (`Dispose()` and `Cleanup()`) never run, so the
viewer is left hidden and undisposed and the session state is not released. The `Find` path
(`QuickFiler/Controllers/EfcFormController.cs:763` → `OpenOlFolderAsync`) fails the same way at
`QuickFiler/Controllers/EfcDataModel.cs:310`, as does the file-system open path at
`QuickFiler/Controllers/EfcDataModel.cs:328`.

Evidence to be captured during implementation (canonical locations per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`):
- Fail-before run of the new regression tests →
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/regression-testing/`
- Post-change toolchain logs →
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/`
- Coverage baseline →
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/`
- Coverage post-change comparison →
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/`

No evidence artifact exists in this feature folder yet; the `evidence` directory has not been
created.

## Scope & Non-Goals

- In scope:
  - Guarding the three archive-root reads at `QuickFiler/Controllers/EfcDataModel.cs:289`,
    `QuickFiler/Controllers/EfcDataModel.cs:310` and `QuickFiler/Controllers/EfcDataModel.cs:328`
    against `InvalidOperationException`, so that `MoveToFolderAsync` returns `false` and the two
    `Open*` methods return after reporting, instead of propagating.
  - Adding an injectable user-diagnostic seam on `EfcDataModel` so the two `Open*` paths, which
    return `Task` and therefore cannot report through a result, can surface a message that is
    assertable in a unit test.
  - New MSTest regression tests in `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`.
  - Registering that new file in `QuickFiler.Test/QuickFiler.Test.csproj`.
  - Updating this spec's status and the feature folder's evidence artifacts.

- Out of scope / non-goals. The three items below are verified findings that must survive this
  issue. Each warrants a **separate follow-up issue**, filed through the repository's promotion
  lifecycle rather than left as prose in this folder.
  - **(a) `COMException` from the live COM calls inside the `ArchiveRootPath` getter.** The getter
    dereferences `Root.FolderPath` at `TaskMaster/AppGlobals/AppOlObjects.cs:260` and
    `ArchiveRoot?.FolderPath` at `TaskMaster/AppGlobals/AppOlObjects.cs:261`; `ArchiveRoot`
    (`TaskMaster/AppGlobals/AppOlObjects.cs:270`) lazily runs `LoadArchiveRoot`
    (`TaskMaster/AppGlobals/AppOlObjects.cs:272-276`) → `FolderPredictor.GetFolder`. Those are live
    COM calls and can raise `System.Runtime.InteropServices.COMException`, which the deliberately
    narrow catch in this fix will not absorb. Widening the catch here is rejected; see Root Cause
    Analysis.
  - **(b) The generic log-only boundary-sink gap at the `async void` boundaries.** All five
    `async void` handlers in `QuickFiler/Controllers/EfcFormController.cs` — `ButtonCancel_Click`
    (`:442-443`), `ButtonOK_Click` (`:460`), `ButtonRefresh_Click` (`:477-478`), `ButtonCreate_Click`
    (`:495-496`) and `ButtonDelete_Click` (`:557-558`) — delegate to an `Async` method whose catch
    calls `BoundaryErrorSink` and returns. Any unexpected exception on the OK path therefore
    produces a vanished form with no user feedback and with `Dispose()`/`Cleanup()` skipped,
    independently of the archive-root cause. This is a boundary-behavior defect, not an
    archive-root defect.
  - **(c) The archive-root reads inside `QuickFiler/Controllers/EfcFormController.cs`.** Five exist:
    `:529` and `:539` in `ButtonCreateClickAsync` (`:498-555`, caught at `:551-554`); `:836` and
    `:846` in `CreateFolderAsync` (`:815-858`, which has **no local try/catch** and is reached from
    the keyboard `'N'` binding at `:630` and `:698` via `KbdExecuteAsync` (`:894-898`, no try/catch)
    into `KeyboardHandler.KeyboardHandler_KeyDownAsync`
    (`QuickFiler/Controllers/KeyboardHandler.cs:133-148`), whose catch at `:141-147` logs only); and
    `:987` in `BindBreadcrumbRowsAsync` (`:980-997`, caught at `:993-996`, logs only). These sit in a
    different class with a different reporting surface and are excluded to keep the fix minimal per
    the Bugfix Workflow.
  - Note on formatting: the paths in this non-goals list are given as concrete, verified citations so
    a later reader can re-derive them. They are **not** part of the change footprint. The change
    footprint is exactly the four files listed under "In scope" above and repeated under
    "Files/modules to change".

- Explicitly excluded systems, integrations, or datasets:
  - `TaskMaster/AppGlobals/AppOlObjects.cs` and `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs`: the
    throw contract stays exactly as it is. It is pinned by
    `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs`, which must continue to
    pass unmodified.
  - `UtilitiesCS`: `IOlObjects.ArchiveRootPath` (`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:15`)
    is read, not changed. No interface member is added, removed, or retyped.
  - The wider `async void` surface in the QuickFiler assembly outside
    `QuickFiler/Controllers/EfcFormController.cs` (handlers in `QfcFormController.EventHandlers.cs`,
    `QfcItemController.EventHandlers.cs`, `QfcItemController.EventWiring.cs`, `QfcDatamodel.cs`,
    `KeyboardHandler.cs`, `BreadcrumbBridgeRouter.cs` and `ConversationResolver.Loading.cs`) is not
    reached by this issue and must not be swept in.
  - Live Outlook, live COM, network, database, and filesystem: none is touched by the fix or by any
    test added for it.

## Root Cause Analysis

- Confirmed root cause:
  `QuickFiler/Controllers/EfcDataModel.cs` treats `Globals.Ol.ArchiveRootPath` as a total function.
  Since the #614 D6 change, the getter is a **partial** function: it throws
  `InvalidOperationException` by contract when the archive root cannot be validated
  (`TaskMaster/AppGlobals/AppOlObjects.cs:250-252` documents the exception;
  `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:30-31` documents it on the helper). `EfcDataModel`
  was never updated to honour that contract, so a documented, expected failure mode escapes as an
  exception through a call chain that has no handler until a log-only fault boundary. The result is
  not a crash on this head — it is a silent swallow with skipped cleanup.

- Signals/evidence supporting it:
  - The three reads are syntactically inside object initializers
    (`QuickFiler/Controllers/EfcDataModel.cs:282-291`, `:306-312`, `:324-330`), where a `try` cannot
    be placed without restructuring, which is why the omission is easy to miss in review.
  - The adjacent OneDrive condition in the same method is guarded
    (`QuickFiler/Controllers/EfcDataModel.cs:277-281`), demonstrating that the class already models
    "a required filing root did not resolve" as a `return false`, not as an exception. The archive
    root is the same class of condition and is the only one left unguarded.
  - The whole of `QuickFiler/Controllers/EfcDataModel.cs` contains a single `catch`, at `:249`, in an
    unrelated method.
  - `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs` already pins both throw
    conditions in isolation, so the throwing behavior is intentional and must not be reversed at the
    source.
  - Every caller of `MoveToFolderAsync(string, ...)` already handles a `false` result:
    `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:98` returns it to `:78`, which passes
    it to `HandleMoveResult` at `:86`, which routes `false` to `MoveFailureMessageAction` at
    `:132-136`; and the `MAPIFolder` overload at `QuickFiler/Controllers/EfcDataModel.cs:346` shows a
    message box at `:353-356`. No caller silently discards the bool, so returning `false` is already
    a fully supported outcome.

- Affected components/modules (paths, services, pipelines):
  - `QuickFiler/Controllers/EfcDataModel.cs` — the defect site and the only production file changed.
  - `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` — new regression tests.
  - `QuickFiler.Test/QuickFiler.Test.csproj` — explicit compile registration for the new test file.
  - Unchanged but load-bearing on the outcome: `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs`
    (routes a `false` result to the user) and `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs`
    (defines the exception contract being honoured).

## Proposed Fix

### Design summary (what changes where):

Add one private helper to `EfcDataModel` that performs the archive-root read inside a narrow
`try`/`catch (InvalidOperationException)` and reports failure through a `bool` return, then route all
three reads through it. Add one injectable `Action<string>` diagnostic seam so the two `Open*`
methods, which return `Task` and cannot report through a result, can still surface a message that a
unit test can assert. Both shapes already exist in this codebase and are reused rather than invented:
the `return false` degrade mirrors the OneDrive guard in the same method
(`QuickFiler/Controllers/EfcDataModel.cs:277-281`), and the injectable action mirrors
`EfcHomeController.MoveFailureMessageAction`
(`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:23-24`), which is already unit-tested at
`QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs:160-175`.

### Boundaries and invariants to preserve:

- **Guard ordering (verified, load-bearing).** The archive-root guard must be placed **after** the
  existing OneDrive `SpecialFolders` read in all three methods, never before it.
  `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:217` asserts
  `probe.FileSystem.SpecialFoldersAccessCount.Should().Be(2)` in
  `OpenFolderMethods_DelegateToDataModelWithoutExternalServices`
  (`QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:207-218`), after calling
  `OpenOlFolderAsync` and `OpenFsFolderAsync` once each. The counter is incremented by the
  `SpecialFolders` getter at `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:414-423`.
  Two independent breakages follow from putting the new guard first: the count would drop to 0
  because both methods would return before the `SpecialFolders` read; and the probe's
  `FakeApplicationGlobals.Ol` returns `null`
  (`QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:388`), so an earlier
  `Globals.Ol.ArchiveRootPath` would raise `NullReferenceException`, which a
  `catch (InvalidOperationException)` does not absorb. The probe seeds an empty
  `ConcurrentDictionary` (`QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:245-247`),
  which is why the existing test never reaches the archive-root read today.
- The `MailInfo is null` guard at `QuickFiler/Controllers/EfcDataModel.cs:267-270` must remain the
  first check in `MoveToFolderAsync`.
- The exception contract of `IOlObjects.ArchiveRootPath` is not changed. The archive root must never
  be degraded to an empty or synthesized value: an empty `OlAncestor` flows into `EmailFilerConfig`
  and downstream stem composition, which is precisely the #614 store-root-leak failure mode.
- Public and internal signatures are unchanged:
  `Task<bool> MoveToFolderAsync(string, bool, bool, bool, bool)`, `Task OpenOlFolderAsync(string)`,
  `Task OpenFsFolderAsync(string)`.
- Redaction discipline is preserved. No user-visible message may interpolate the archive root path,
  the destination folder path, or a mailbox address, matching
  `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:13-17` and the assertion style already used at
  `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs:47-59`.
- `QuickFiler/Controllers/EfcDataModel.cs` is currently 423 lines against the 500-line cap in the
  General Code Change Policy. The change must not exceed the cap.

### Dependencies or blocked work:

- No blocking dependency. The fix is self-contained within the `QuickFiler` and `QuickFiler.Test`
  projects.
- Related but independent: issue #637 (producer-side rooted-path normalization) and issue #614
  (store-root leak). Neither must land first.
- The three non-goals above should be filed as follow-up issues; filing them does not block this fix.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

- `QuickFiler/Controllers/EfcDataModel.cs` — production change.
- `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` — new test file.
- `QuickFiler.Test/QuickFiler.Test.csproj` — add an explicit
  `<Compile Include="Controllers\EfcDataModelArchiveRootTests.cs" />` entry.
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/spec.md` —
  acceptance-criteria check-off and status update.

#### Functions/classes/CLI commands impacted:

- `EfcDataModel.MoveToFolderAsync(string, bool, bool, bool, bool)` — the read at `:289` becomes a
  guarded assignment; an unresolvable root returns `false`.
- `EfcDataModel.OpenOlFolderAsync(string)` — the read at `:310` becomes a guarded assignment; an
  unresolvable root reports through the diagnostic seam and returns.
- `EfcDataModel.OpenFsFolderAsync(string)` — same, for the read at `:328`.
- New private helper on `EfcDataModel` (for example `TryGetArchiveRoot(out string)`), catching
  `InvalidOperationException` only and logging through the existing static `log4net` logger declared
  at `QuickFiler/Controllers/EfcDataModel.cs:23-25`.
- New internal settable seam on `EfcDataModel` (for example
  `internal System.Action<string> UserDiagnosticAction { get; set; }`) defaulting to
  `MessageBox.Show`. `System.Windows.Forms` is already imported at
  `QuickFiler/Controllers/EfcDataModel.cs:10` and `MessageBox.Show` is already used at
  `QuickFiler/Controllers/EfcDataModel.cs:355`, so this adds no dependency.
- No CLI command is affected; this project ships no CLI surface.

#### Data flow and validation changes:

- `EmailFilerConfig.OlAncestor` is assigned only from a value that the guard has confirmed was
  produced without throwing. It is never assigned an empty or synthesized value.
- The failure path short-circuits before `new EmailFiler(config)` at
  `QuickFiler/Controllers/EfcDataModel.cs:293`, `:314` and `:332`, so no partially populated
  `EmailFilerConfig` is ever handed to the filer.
- Validation order inside `MoveToFolderAsync` becomes: `MailInfo` null check → OneDrive
  `SpecialFolders` lookup → archive-root resolution. Inside each `Open*` method: OneDrive
  `SpecialFolders` lookup → archive-root resolution.

#### Error handling and logging updates:

- Catch breadth is exactly `InvalidOperationException`. That is the documented contract of the
  property (`TaskMaster/AppGlobals/AppOlObjects.cs:250-252`) and of the guard
  (`TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:30-31`). Broader catches are rejected: a
  `COMException` from the live COM calls in the getter must continue to propagate so it remains
  visible for non-goal (a), and `catch (Exception)` would violate the General Code Change Policy's
  fail-fast rule.
- One `logger.Warn` entry is written per guarded failure, using the existing static logger at
  `QuickFiler/Controllers/EfcDataModel.cs:23-25`. This matches the shape of the adjacent OneDrive
  guard's `logger.Warn($"Cannot sort without OneDrive location")` at
  `QuickFiler/Controllers/EfcDataModel.cs:279`. No `Console.WriteLine` or other ad-hoc output is
  added.
- On the `Sort` path the user message continues to be emitted by
  `EfcHomeController.HandleMoveResult` → `MoveFailureMessageAction`
  (`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:132-136`). Its existing text,
  "Cannot move to folderpath {selectedFolder}", is left unchanged: it already fires, it is pinned by
  `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs:174`, and changing it would
  widen the diff without changing the outcome.
- On the two `Open*` paths the user message is emitted at the point of failure through the new
  diagnostic seam, because those methods return `Task` and have no result channel.

#### Rollback/feature-flag considerations (if applicable):

- No feature flag. The change is a behavior-narrowing guard inside three methods of one class; the
  repository has no feature-flag mechanism and adding one for this would be disproportionate.
- Rollback is a straight revert of the commit. Because no interface, no signature, and no persisted
  data shape changes, a revert restores the prior behavior exactly, with no migration or cleanup.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- Input to the guarded read: `IApplicationGlobals.Ol.ArchiveRootPath`, a `string` declared at
  `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:15`, which either returns a validated full Outlook
  archive path or throws `InvalidOperationException`.
- `MoveToFolderAsync(string, bool, bool, bool, bool)` returns `Task<bool>`; `true` means the filing
  operation completed, `false` means it did not. The new contract clause: `false` now additionally
  covers "the archive root could not be resolved".
- `OpenOlFolderAsync(string)` and `OpenFsFolderAsync(string)` return `Task` (no value). Their new
  contract clause: on an unresolvable archive root they complete normally after invoking the
  diagnostic seam exactly once.
- User-visible diagnostic format: a single plain-text sentence naming the rule, with no path and no
  mailbox address interpolated.

#### Required configuration keys and defaults:

- None. This fix introduces no configuration key, no app setting, and no entry in `coverage.config`.
- The one new default is the diagnostic seam's initial value, `text => MessageBox.Show(text)`, chosen
  to match `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:23-24` exactly. Tests replace it
  with a capturing delegate; production never assigns it.

#### Backward-compatibility expectations:

- No breaking change. `EfcDataModel` is `internal`
  (`QuickFiler/Controllers/EfcDataModel.cs:21`) and is reachable from tests only through
  `[assembly: InternalsVisibleTo("QuickFiler.Test")]` at `QuickFiler/Properties/AssemblyInfo.cs:5`.
  There is no external consumer.
- Widening `OpenOlFolderAsync` or `OpenFsFolderAsync` to `Task<bool>` is explicitly rejected. All
  five production call sites — `QuickFiler/Controllers/EfcHomeController.cs:429`,
  `QuickFiler/Controllers/EfcHomeController.cs:434`,
  `QuickFiler/Controllers/EfcFormController.cs:763`,
  `QuickFiler/Controllers/EfcFormController.cs:513` and
  `QuickFiler/Controllers/EfcFormController.cs:823` — would discard the value, which would
  manufacture a silent-swallow problem in five places rather than remove one.
- Callers that already branch on a `false` result keep working unchanged; no caller needs editing.

#### Performance constraints (latency/throughput/memory):

- The guard adds one `try` frame per call on a path that already performs Outlook COM work and disk
  I/O through `EmailFiler`. The added cost is not measurable against that baseline.
- The success path must read `ArchiveRootPath` exactly once per call, as it does today. The property
  is COM-backed on first resolution (`TaskMaster/AppGlobals/AppOlObjects.cs:272-276`), so a
  double-read would add a real round trip on the first call; this is pinned by a test.
- The failure path is strictly cheaper than today: it returns before constructing `EmailFiler`.
- No new allocation on the success path beyond the existing string reference.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access):
  - `EfcDataModel.Globals` is an **injected instance**, not a static COM accessor. It is the property
    at `QuickFiler/Controllers/EfcDataModel.cs:148-153`
    (`public IApplicationGlobals Globals { get => _globals; protected set => _globals = value; }`),
    assigned from a constructor parameter. The read chain is therefore
    `IApplicationGlobals.Ol` → `IOlObjects.ArchiveRootPath`, both interface members, so no new seam
    is required to test the failure.
  - `IFileSystemFolderPaths.SpecialFolders` is a concrete `ConcurrentDictionary<string, string>`, so
    seeding `"OneDrive"` in a test to let the guard at
    `QuickFiler/Controllers/EfcDataModel.cs:277-281` pass is straightforward.
  - A developer machine has `msbuild`, `vstest.console.exe` and the .NET Framework 4.8.1 targeting
    pack available; `dotnet tool restore` has been run once in this worktree before the first
    CSharpier invocation.
  - The manual live-Outlook walkthrough in Repro & Evidence is optional confirmation owned by the
    maintainer. No acceptance criterion depends on it.
- Constraints (budget, performance, compatibility):
  - `QuickFiler/Controllers/EfcDataModel.cs` must stay at or under 500 lines. It is 423 lines today;
    the change is estimated at 30-45 added lines, landing near 455-470. Headroom is limited, so no
    opportunistic additions to this file.
  - Both `QuickFiler.csproj` and `QuickFiler.Test.csproj` are legacy non-SDK projects that enumerate
    every source file. A new `.cs` file does not compile until it is registered; the existing
    `EfcDataModel` test files are registered at `QuickFiler.Test/QuickFiler.Test.csproj:114-115`.
  - Target framework is .NET Framework 4.8.1. Language features that require `IsExternalInit`
    (`init` accessors, `record`, `record struct`) are unavailable.
  - MSTest, Moq and FluentAssertions are the only permitted test libraries; no new dependency may be
    added.
- External dependencies (services, libraries, releases):
  - None added. The fix uses only `System`, `System.Windows.Forms` (already imported at
    `QuickFiler/Controllers/EfcDataModel.cs:10`) and the existing `log4net` logger.
  - No release coordination is required; the change ships with the next add-in build.

## Data / API / Config Impact

- User-facing or API changes:
  - Behavior change only. On an unresolvable archive root the user now sees a message instead of a
    form that vanishes silently, and the form is disposed and cleaned up.
  - No public API surface changes. `EfcDataModel` is `internal`; no interface member is added,
    removed, or retyped.
- Data or migration considerations:
  - None. No persisted data, no schema, no stored settings, and no serialized shape is read or
    written by this change. No migration is required and none is possible to require.
- Logging/telemetry updates (if any):
  - One additional `logger.Warn` entry per guarded failure, from
    `QuickFiler/Controllers/EfcDataModel.cs`, using the existing static log4net logger at `:23-25`.
  - The log4net `Error` entry currently produced by `BoundaryErrorSink` on this path disappears,
    because the exception no longer reaches the boundary. That is the intended outcome, not a loss of
    signal: the new `Warn` entry names the same redacted rule at the point of failure.
  - No telemetry counter, metric, or event is added. `QuickFileMetrics_WRITE`
    (`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:144`) is unchanged and continues to run
    only on a successful move.
- Compatibility notes (CLI flags, config schemas, versioning):
  - No CLI flag, no config schema, no version bump. `coverage.config` is unchanged; it excludes only
    third-party modules (`coverage.config:12-22`) and no first-party assembly.

## Test Strategy

- Test stack and framework (mandated by `CLAUDE.md` §§ CUT1-CUT2):
  - **MSTest** (`Microsoft.VisualStudio.TestTools.UnitTesting`) with `[TestClass]`/`[TestMethod]`.
  - **Moq** for the `IApplicationGlobals` / `IOlObjects` seams.
  - **FluentAssertions** for all assertions.
  - No other framework or assertion library may be introduced.

- Test location decision (D1, recorded so it is not re-litigated):
  New tests go in `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`, **not** in a
  `tests/` mirror tree. Rationale: `.claude/skills/policy-compliance-order/SKILL.md` ranks
  `CLAUDE.md` above `.claude/rules/general-unit-test.md`; the General and C# Unit Test Policies
  embedded in `CLAUDE.md` impose no `tests/` mirroring requirement; and the General Code Change
  Policy § 7.1 requires matching existing repository style, which for every C# test project here is a
  sibling project named after the production project with a `.Test` suffix — `QuickFiler.Test`,
  `TaskMaster.Test`, `UtilitiesCS.Test`, `SVGControl.Test`. The `tests/` tree in this repository
  contains only PowerShell Pester files under `tests/scripts/vscode/`. The
  existing `EfcDataModel` tests already live at `QuickFiler.Test/Controllers/EfcDataModelTests.cs`
  and `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`. Use namespace
  `QuickFiler.Test.Controllers`, matching the newer of the two. The new file **must** be registered
  as an explicit `<Compile Include="Controllers\EfcDataModelArchiveRootTests.cs" />` entry in
  `QuickFiler.Test/QuickFiler.Test.csproj`, which is a legacy non-SDK project that enumerates every
  source file; without that entry the tests silently do not exist.

- Unit-level reproducibility of the defect (this is what makes the fix verifiable without Outlook):
  `EfcDataModel.Globals` is an **injected `IApplicationGlobals` instance**
  (`QuickFiler/Controllers/EfcDataModel.cs:148-153`), not a static COM accessor, and
  `ArchiveRootPath` is an interface member
  (`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:15`). The defect is therefore reproducible at the
  unit level by configuring the injected `IOlObjects.ArchiveRootPath` getter to throw
  `InvalidOperationException` —
  `olObjects.SetupGet(x => x.ArchiveRootPath).Throws(new InvalidOperationException(...))`. This is
  the same seam already used to make the property *return* in
  `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs`. The internal class is
  reachable through `[assembly: InternalsVisibleTo("QuickFiler.Test")]`
  (`QuickFiler/Properties/AssemblyInfo.cs:5`), and
  `QuickFiler.Test/Controllers/EfcDataModelTests.cs:220-228` already builds a strict
  `Mock<IApplicationGlobals>` over a strict `Mock<IOlObjects>` and constructs a real `EfcDataModel`
  through the public constructor with no Outlook process
  (`QuickFiler.Test/Controllers/EfcDataModelTests.cs:166-171` and `:200-205`).

- **No `[TestCategory("LiveOutlook")]` test may be added for this issue.** CI filters that category
  out: `.github/workflows/_mstest-coverage.yml:83` runs
  `vstest.console.exe $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`.
  A `LiveOutlook` test would never execute in CI and would create the appearance of coverage without
  the substance. The unit-level proof above is strictly stronger: it is deterministic and it covers
  both throw conditions.

- Regression tests to add or update (all new, in
  `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`; each must fail before the fix and
  pass after):
  1. `MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing` — arrange a
     `Mock<IOlObjects>` whose `ArchiveRootPath` getter throws `InvalidOperationException`, a
     `SpecialFolders` dictionary containing `"OneDrive"`, and an `EfcDataModel` with a non-null
     `MailInfo`; act with `moveConversation: false`; assert the result is `false` and nothing is
     thrown. Fails today with `InvalidOperationException`.
  2. `OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns` — assert the method
     completes without throwing and the injected diagnostic seam received exactly one message.
  3. `OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns` — same shape for the
     file-system open path.

- Unit tests (MSTest) for the fixed behavior and boundaries:
  4. `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` — `VerifyGet(..., Times.Once())`,
     pinning that the guard does not introduce a second read of the COM-backed property.
  5. `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot` —
     `VerifyGet(..., Times.Never())`, pinning the guard at
     `QuickFiler/Controllers/EfcDataModel.cs:267-270` as still first.
  6. `MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot` —
     `VerifyGet(..., Times.Never())`, pinning the ordering constraint from the production side.

- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
  7. `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates` — configure the getter to
     throw `System.Runtime.InteropServices.COMException` and assert it is **not** absorbed, proving
     the catch is narrow and that non-goal (a) remains visible rather than silently swallowed.
  8. Both throw conditions are exercised, not just one: the unresolvable-root message
     (`TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:44`, constant at `:13-14`) and the
     cross-store/renamed message (`:56`, constant at `:16-17`).
  9. `OpenOlFolderAsync` and `OpenFsFolderAsync` with a missing `"OneDrive"` key still return early
     without reading the archive root.

- Error handling and logging verification:
  10. `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress` — assert the
      captured diagnostic string contains neither a mailbox address (for example
      `mailbox@example.com`) nor the archive root path, matching the redaction assertion style at
      `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs:47-59`.
  11. The diagnostic seam is asserted to be invoked exactly once per failing `Open*` call, so a
      future refactor cannot produce duplicate message boxes.

- Existing tests that must keep passing **without modification** (treat as part of the spec):
  - `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs` — in particular
    `OpenFolderMethods_DelegateToDataModelWithoutExternalServices` (`:207-218`) and its
    `SpecialFoldersAccessCount == 2` assertion at `:217`.
  - `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs` — in particular
    `HandleMoveResult_WhenMoveFails_RoutesMessageThroughInjectedAction` (`:160-175`).
  - `QuickFiler.Test/Controllers/EfcDataModelTests.cs` (all).
  - `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs` (all).
  - `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` (all).
  - `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs` (all — the throw contract
    must remain intact).

- Policy conformance of the new tests: independent, isolated, fast, deterministic; Arrange-Act-Assert
  with a summary comment on each test; no external service, no network, no database, no live Outlook,
  no temporary file, no `Thread.Sleep` or `Task.Delay`, no mutable global state.

- Coverage impact and targets for changed lines/modules:
  - `QuickFiler/Controllers/EfcDataModel.cs` carries no `[ExcludeFromCodeCoverage]` attribute and
    `QuickFiler` is not excluded by `coverage.config` (`coverage.config:12-22` excludes only
    third-party modules). The `CLAUDE.md` § UT2 COM/VSTO/WinForms exemption does not apply:
    `EfcDataModel` is not form-derived, is not Designer-generated, is not a VSTO lifecycle class, and
    takes its Outlook dependency through the injectable `IApplicationGlobals` seam, which
    `CLAUDE.md` names as the disqualifier ("without an injectable seam"). The changed lines are
    therefore in the measured denominator and must be covered.
  - Target: **>= 90%** line coverage on the new helper, the new seam, and every changed line, per
    `CLAUDE.md` § UT2 ("any new modules, classes, or methods added must target >= 90% coverage").
  - No regression on changed lines relative to the merge-base baseline.
  - Repository-wide line coverage against the testable denominator (`CLAUDE.md` § UT2, with the
    COM/VSTO/WinForms/Outlook-Interop exemptions applied) is a **record-and-report** obligation, not
    a blocking gate for this change. No baseline coverage evidence exists in this feature folder
    yet, so no repo-wide figure can be asserted in advance; the implementer must capture the
    merge-base baseline first and report both figures, and must show the change does not lower them.
  - Evidence interpretation note: an exempt member emits no `<method>` element in the Cobertura
    output at all, so absence is the exemption signal, not a zero rate.

- Toolchain commands to run (format → lint → type-check → test). Quoted verbatim from `CLAUDE.md`
  § "C# Toolchain (run in this exact order)"; run `dotnet tool restore` once per worktree before the
  first CSharpier invocation, and restart from step 1 if any step fails or changes files:
  1. `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`; always via `dotnet tool run`, never a global install)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

  Notes on running steps 2-4 honestly. Do **not** add `/p:Nullable=enable` to step 3 and do **not**
  substitute `/t:Build` for `/t:Rebuild` in steps 2 or 3; `CLAUDE.md` §§ C#1.2-C#1.3 document both as
  load-bearing, because MSBuild's up-to-date check does not invalidate on a command-line `/p:`
  change and a warm `/t:Build` returns exit 0 having skipped `CoreCompile` on every project. Prove
  non-vacuity by confirming the build log contains zero occurrences of
  `Skipping target "CoreCompile"`, and read the error count from MSBuild's own `N Error(s)` line
  rather than by grepping for `error CS`. For step 4, match CI by adding `/InIsolation` and
  `/TestCaseFilter:"TestCategory!=LiveOutlook"`, and exclude any test assembly under
  `.claude/worktrees` from the discovered set.

- Manual validation steps (if required):
  - **Not required.** No acceptance criterion depends on a live-Outlook step. The live-profile
    walkthrough in Repro & Evidence is recorded as an optional post-merge confirmation owned by the
    maintainer, because the profile condition's only effect on the code under test is that
    `IOlObjects.ArchiveRootPath` throws `InvalidOperationException` with one of two fixed messages,
    and that is injected directly at the interface seam in the unit tests.

## Acceptance Criteria

Every criterion below is independently checkable by the named MSTest test or the named toolchain
command. None depends on a live-Outlook manual step. All tests named as new live in
`QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` unless stated otherwise.

- [ ] AC1 — `MoveToFolderAsync(string, bool, bool, bool, bool)` returns `false` instead of
  propagating when the injected `IOlObjects.ArchiveRootPath` getter throws
  `InvalidOperationException`. Verified by `MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing`.
- [ ] AC2 — `OpenOlFolderAsync(string)` completes without throwing and invokes the injected
  user-diagnostic seam exactly once when the archive root is unresolvable. Verified by
  `OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns`.
- [ ] AC3 — `OpenFsFolderAsync(string)` completes without throwing and invokes the injected
  user-diagnostic seam exactly once when the archive root is unresolvable. Verified by
  `OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns`.
- [ ] AC4 — The user-visible diagnostic contains neither a mailbox address nor the archive root
  path. Verified by `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress`.
- [ ] AC5 — The archive-root guard sits **after** the OneDrive `SpecialFolders` read in all three
  methods: `ArchiveRootPath` is never read when the `"OneDrive"` key is absent. Verified by
  `MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot`
  (`VerifyGet(..., Times.Never())`) **and** by
  `OpenFolderMethods_DelegateToDataModelWithoutExternalServices` in
  `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs` still passing unmodified with its
  `SpecialFoldersAccessCount == 2` assertion at `:217`.
- [ ] AC6 — The `MailInfo is null` guard remains the first check in `MoveToFolderAsync`:
  `ArchiveRootPath` is not read when `MailInfo` is null. Verified by
  `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot`
  (`VerifyGet(..., Times.Never())`).
- [ ] AC7 — The success path reads `ArchiveRootPath` exactly once per call; the guard introduces no
  second COM-backed read. Verified by `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`
  (`VerifyGet(..., Times.Once())`).
- [ ] AC8 — The catch is narrowed to `InvalidOperationException`: a
  `System.Runtime.InteropServices.COMException` raised by the same getter still propagates and is not
  absorbed. Verified by `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates`.
- [ ] AC9 — Both documented throw conditions are covered, not only one: a test exercises the
  unresolvable-root message and a test exercises the cross-store/renamed message, matching the
  constants at `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:13-17`.
- [ ] AC10 — Public and internal signatures are unchanged:
  `Task<bool> MoveToFolderAsync(string, bool, bool, bool, bool)`, `Task OpenOlFolderAsync(string)`,
  `Task OpenFsFolderAsync(string)`. Verified by
  `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs`,
  `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs`,
  `QuickFiler.Test/Controllers/EfcDataModelTests.cs`,
  `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`,
  `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` and
  `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs` all compiling and passing
  with **zero** edits.
- [ ] AC11 — `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` is registered as an
  explicit `<Compile Include="Controllers\EfcDataModelArchiveRootTests.cs" />` entry in
  `QuickFiler.Test/QuickFiler.Test.csproj`, and the new tests appear in the executed test list of the
  step-4 `vstest.console.exe` run.
- [ ] AC12 — Fail-before / pass-after evidence exists for the three regression tests (AC1-AC3): a
  captured run showing them failing against unmodified production code, and a captured run showing
  them passing after the fix, both written under
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/regression-testing/`.
- [ ] AC13 — Format gate: `dotnet tool run csharpier check .` reports no unformatted files, run
  through `dotnet tool run` against the manifest-pinned version.
- [ ] AC14 — Analyzer gate:
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  completes with `0 Error(s)` on MSBuild's own summary line, and the log contains zero occurrences of
  `Skipping target "CoreCompile"`.
- [ ] AC15 — Type-check gate:
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  completes with `0 Error(s)` on MSBuild's own summary line, with zero occurrences of
  `Skipping target "CoreCompile"`, and without `/p:Nullable=enable` and without substituting
  `/t:Build`.
- [ ] AC16 — Test gate: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` (with CI's
  `/InIsolation` and `/TestCaseFilter:"TestCategory!=LiveOutlook"`, excluding assemblies under
  `.claude/worktrees`) reports zero failed tests across `QuickFiler.Test` and `TaskMaster.Test`. No
  test in the change carries `[TestCategory("LiveOutlook")]`.
- [ ] AC17 — Coverage: the new helper, the new diagnostic seam, and every changed line in
  `QuickFiler/Controllers/EfcDataModel.cs` reach **>= 90%** line coverage, and coverage on changed
  lines does not regress against the merge-base baseline. The repository-wide figure against the
  testable denominator is captured and reported for the baseline run under
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/`
  and for the post-change run under
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/`,
  and the change is shown not to lower it; the repo-wide figure itself is recorded, not used as a
  blocking threshold, because no baseline evidence existed when this spec was written.
- [ ] AC18 — Change footprint is exactly `QuickFiler/Controllers/EfcDataModel.cs`,
  `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`,
  `QuickFiler.Test/QuickFiler.Test.csproj`, and this feature folder's documents and evidence. No
  other production file is modified; in particular
  `QuickFiler/Controllers/EfcFormController.cs`, `TaskMaster/AppGlobals/AppOlObjects.cs`,
  `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs` and
  `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs` are untouched. Verified by `git diff --name-only`
  against the merge base.
- [ ] AC19 — `QuickFiler/Controllers/EfcDataModel.cs` remains at or under 500 lines after the change
  (423 lines before). Verified by a line count of the file.
- [ ] AC20 — The three non-goals in Scope & Non-Goals — (a) `COMException` from the archive-root
  getter's COM calls, (b) the log-only `async void` boundary sinks in
  `QuickFiler/Controllers/EfcFormController.cs`, and (c) the five archive-root reads inside
  `QuickFiler/Controllers/EfcFormController.cs` — are each filed as a separate follow-up issue
  through the repository's promotion lifecycle, with the issue numbers recorded in the Rollout &
  Follow-up section of this file.

## Risks & Mitigations

- Technical or operational risks:
  - **Guard placed before the OneDrive read.** This is the single highest-probability implementation
    error, because "check the archive root first" reads as the natural ordering. It breaks
    `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:217` in two ways at once (count
    drops to 0; `FakeApplicationGlobals.Ol` is null at `:388`, giving a `NullReferenceException` that
    the narrow catch does not absorb).
  - **Catch widened to `Exception` or to `COMException`.** This would silently absorb non-goal (a)
    and would violate the General Code Change Policy's fail-fast rule, converting a visible COM
    failure into an invisible one.
  - **Degrading to an empty archive root instead of returning early.** An empty `OlAncestor` flows
    into `EmailFilerConfig` and downstream stem composition, which reintroduces the #614
    store-root-leak failure mode.
  - **New test file not registered in the legacy `.csproj`.** The tests would silently not exist, and
    a green run would prove nothing.
  - **Behavior change surprises a user who relied on the current silence.** Low: the current
    behavior is a vanished window with no message, which is not a behavior anyone can rely on.
  - **File-size cap.** `QuickFiler/Controllers/EfcDataModel.cs` has roughly 77 lines of headroom
    before the 500-line cap.

- Mitigations and rollbacks:
  - The ordering risk is mitigated by AC5, which pins it from both sides — a new `Times.Never()`
    assertion on the production side and the existing, unmodified `SpecialFoldersAccessCount == 2`
    assertion on the test side.
  - The catch-breadth risk is mitigated by AC8, which asserts a `COMException` still propagates.
  - The empty-root risk is mitigated by the invariant in Boundaries and by AC1-AC3, which assert an
    early `false`/return rather than a filing attempt with a degraded root.
  - The registration risk is mitigated by AC11, which requires the new tests to appear in the
    executed test list, not merely to exist on disk.
  - The file-size risk is mitigated by AC19 and by the constraint prohibiting opportunistic additions
    to this file.
  - Rollback: revert the single commit. No interface, signature, persisted data shape, or
    configuration key changes, so a revert is complete and requires no migration or cleanup.

## Rollout & Follow-up

- Release/rollout steps:
  1. Land the change on `bug/efc-unguarded-archive-root-read-crashes-ui-thread-638` with the full
     four-step toolchain green in a single pass (AC13-AC16) and all evidence committed.
  2. Open a pull request to `main` with the change description, the fail-before/pass-after regression
     evidence, and the coverage comparison.
  3. Ship with the next add-in build. No staged rollout, no feature flag, and no configuration change
     is required, because the change is a behavior-narrowing guard inside one internal class.

- Post-fix monitoring or clean-up tasks:
  - After merge, the maintainer may optionally run the live-profile walkthrough in Repro & Evidence
    on a profile whose archive folder does not resolve, and confirm that a message appears and the
    form closes cleanly. This is confirmation only; no acceptance criterion depends on it.
  - Watch the log4net output for the new `Warn` entry from `QuickFiler/Controllers/EfcDataModel.cs`.
    A sustained rate of that entry indicates profiles in the field whose archive root does not
    resolve, which is useful input for non-goal (a).
  - File the three follow-up issues required by AC20 and record their numbers here.
  - No temporary scaffolding, feature flag, or migration needs removing afterwards.

- Links: issue, PRs, related docs
  - Issue: #638 — https://github.com/drmoisan/TaskMaster/issues/638
  - Feature folder:
    `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638`
  - Promoted issue body:
    `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/issue.md`
    (two of its claims are corrected in the Context section above)
  - Research artifact (authoritative where it contradicts the issue body, because it was verified
    against this branch head):
    `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/research/2026-08-29T08-05-archive-root-guard-research.md`
  - Plan:
    `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/plan.2026-08-29T07-41.md`
  - Related issues: #614 (store-root leak; introduced the throwing contract this fix honours), #637
    (producer-side rooted-path normalization), #602 (redaction of mailbox addresses in diagnostics).
  - PRs: to be recorded when opened.
  - Follow-up issues for the three non-goals: to be recorded when filed (AC20).

## Correction Log

- **2026-08-29T08-40 — acceptance criteria and repro restated against verified head behavior.**
  The template's eight generic placeholder criteria were replaced with 20 concrete, individually
  verifiable criteria (D2). The originals were: "Repro steps now produce the expected behavior in all
  documented environments", "Regression test(s) added and passing (list file path and test name)",
  "Edge cases and invalid inputs are handled with correct errors or fallbacks", "No unintended
  behavior changes outside the defined scope", "Required logs/telemetry updated and validated (if
  applicable)", "Performance constraints met or explicitly waived with rationale", "Full toolchain
  pass completed (format → lint → type-check → test)", and "Docs/config references updated to match
  the new behavior". The first of those was unsatisfiable as written, because the issue body's repro
  step 5 asserts an unhandled UI-thread exception that cannot occur on this branch head. Repro &
  Evidence was rewritten so its steps and its Expected/Actual describe the verified silent-swallow
  symptom instead.
- **2026-08-29T08-40 — repo-wide coverage floor demoted from a blocking gate to a
  record-and-report obligation (AC17).** No baseline coverage evidence exists in this feature folder,
  so a repo-wide threshold could not be shown to be satisfiable in advance. The blocking conditions
  are change-scoped: >= 90% on the new and changed lines, and no regression on changed lines.
- **2026-08-29T10-05 — non-canonical coverage evidence sub-path replaced.** The Repro & Evidence
  section and AC17 both named a `coverage` sub-directory beneath this feature folder's `evidence`
  directory. The literal spelling of that removed sub-path is not repeated in this entry, because a
  verification step greps the spec for it and expects zero matches.
  That sub-path is not among the canonical evidence sub-paths enumerated by
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, which is the single source of truth
  for evidence locations and is non-overridable. Both references were replaced: the baseline coverage
  figure is recorded under
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/`
  and the post-change coverage figure under
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/`.
  The substitution changes only the location at which AC17's evidence is written; the measurement
  obligations AC17 states are unchanged. Authority:
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.
- **2026-08-29T11-20 — absolute host path removed from the Context section.** The provenance sentence
  that opens the Context section's citation note identified the worktree in which the citations were
  re-derived by its full filesystem location, which carried the account name of the machine that ran
  the authoring session. That sentence now identifies the worktree by its branch name and the
  `origin/main` merge-base SHA alone, which are the two facts a reader needs in order to reproduce the
  citations. The removed text is not quoted in this entry, because a verification step greps this file
  for its leading segment and expects zero matches. No criterion text changed, no evidence location
  changed, and no citation changed.
