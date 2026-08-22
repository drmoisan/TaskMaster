# Research — QfcExplorerController latent defects (Issue #449)

- **Timestamp:** 2026-08-21T18-20
- **Issue:** #449 (`quickfiler-explorer-controller-latent-defects`)
- **Epic:** `quickfiler-suite-determinism-foundation` (wave 0, C3)
- **Worktree:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a78a924c87d7f1f73`
- **Authoritative requirements:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a78a924c87d7f1f73\docs\features\potential\promoted\2026-08-07-quickfiler-explorer-controller-latent-defects.md`
- **Mode:** research-only. No C# file, `.csproj`, or `.claude/**` file was modified.

All line numbers in this document were re-derived by reading the files in this worktree, per the epic's
"Known-Stale Potential-Document References" constraint.

---

## 0. Corrections and discrepancies found in supplied facts

Every orchestrator-supplied fact was re-verified. All were confirmed. Three additions and two
discrepancies are recorded here.

### 0.1 Confirmed as supplied

| Supplied fact | Verification |
| --- | --- |
| `QfcExplorerController.cs` is 323 lines | Confirmed (line count = 323). |
| `[ExcludeFromCodeCoverage]` at line 20, `internal class ... : IQfcExplorerController` at line 21 | Confirmed. |
| `ExplConvView_Cleanup()` at lines 61-64 throws `NotImplementedException` (line 63) | Confirmed. |
| Declared on the interface at `IQfcExplorerController.cs:12` | Confirmed. |
| Second `ActiveExplorer()` at line 140, inside private `NavigateToOutlookFolder(MailItem)` (lines 133-143); `OpenQFItem` at 146-181 calls it at 149 | Confirmed. |
| Constructor captures `_activeExplorer` at line 35 | Confirmed. |
| `#region Email Sorting To Rewrite` spans 183-321 | Confirmed (`#region` at 183, `#endregion` at 321). |
| `QfcExplorerController` is the only implementer; every test reference is `Mock<IQfcExplorerController>` | Confirmed. |
| `ExplConvView_Cleanup` has zero production callers; only other call site is `QuickFiler/Legacy/QuickFileController.cs:673` | Confirmed. |
| `QuickFileController.cs` and `Notes/notes_interfaces.cs` are not compiled | Confirmed, and broadened — see 0.2. |
| Legacy implementation at `QuickFileController.cs:851-869` | Confirmed exactly. |
| `Mock<Explorer>` and `Mock<Outlook.View>` are proven patterns | Confirmed; see 5.2 for the citation set. |
| `[assembly: InternalsVisibleTo("QuickFiler.Test")]` present | Confirmed at `QuickFiler/Properties/AssemblyInfo.cs:5`. |
| No `quality-tiers.yml`; no `scripts/dev_tools/`; no Poetry manifest | Confirmed. |

### 0.2 Additions (broader than supplied)

1. **No file under `QuickFiler/Legacy/` is compiled at all.** `QuickFiler/QuickFiler.csproj` contains
   zero `<Compile Include="Legacy\...">` entries (grep for `Compile Include="Legacy` returns no match).
   The supplied fact named two files; the correct statement is that the entire `Legacy/` folder plus
   `Notes/` is out of the build. This strengthens the deletion-safety argument in §3 and removes any
   doubt about `QfcGroupOperationsLegacy.cs` and `QfcController.cs`, which also reference
   `ExplConvView_*` members.

2. **The concrete class is constructed in two production sites**, not zero:
   - `QuickFiler/Controllers/QfcHomeController.cs:182` — `new QfcExplorerController(initType, globals, homeController);`
   - `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:155` — `return new QfcExplorerController(initType, globals, homeController);`

   Both go through a replaceable factory delegate, which is why every existing test binds
   `Mock<IQfcExplorerController>`.

3. **The legacy type initialises `_objViewMem` in its constructor; the modern type does not.**
   `QuickFileController.cs:145-147`:
   ```csharp
   BlShowInConversations = CurrentConversationState;
   if (BlShowInConversations)
       _objViewMem = ((Outlook.View)_activeExplorer.CurrentView).Name;
   ```
   `QfcExplorerController`'s constructor (lines 27-37) sets only `_initType`, `_globals`,
   `_activeExplorer`, `_parent`. This is load-bearing for Q1 and is analysed in §1.2.

### 0.3 Discrepancies to record in the spec

1. **Feature-folder name mismatch with the epic.** `docs/features/epics/quickfiler-suite-determinism-foundation/epic.md:33`
   declares `feature_folder: 2026-08-21-quickfiler-explorer-controller-latent-defects-449`. The folder
   that exists on disk — and the one this research artifact was written into per the orchestrator's
   non-overridable path — is `docs/features/active/2026-08-07-quickfiler-explorer-controller-latent-defects-449`.
   No `2026-08-21-*` folder exists. The epic manifest and the on-disk folder disagree. Flagging, not
   resolving: the epic file is outside this issue's scope, and the on-disk folder already carries
   `issue.md`, `spec.md` and `plan.2026-08-21T18-09.md`.

2. **The potential document's coverage-denominator claim is false.** It asserts that deleting the dead
   region "removes roughly 139 lines of uncoverable filesystem-I/O code from the coverage denominator."
   Because of the pre-existing class-level `[ExcludeFromCodeCoverage]`, those lines are already absent
   from the denominator. Deletion changes the measured denominator by exactly zero. See §6.1.

---

## 1. Q1 — `ExplConvView_Cleanup` contract decision

### 1.1 The legacy body, verbatim (`QuickFiler/Legacy/QuickFileController.cs:851-869`)

```csharp
public void ExplConvView_Cleanup()
{
    ObjView = _activeExplorer.CurrentFolder.Views[_objViewMem];
    try
    {
        ObjView.Apply();
        ObjViewTemp?.Delete();
        BlShowInConversations = false;
    }
    catch (System.Exception)
    {
        ObjViewTemp = GetSiblingView(
            (Outlook.View)_activeExplorer.CurrentView,
            "tmpNoConversation"
        );

        ObjViewTemp?.Delete();
    }
}
```

Its sole legacy call site is `QuickFileController.cs:667-680` (`ButtonCancel_Click`), guarded by
`if (BlShowInConversations)`. Semantically: on cancel, restore the remembered view and delete the
temporary `tmpNoConversation` view; on failure, best-effort locate and delete the temporary view.

### 1.2 The four specific points raised

**(a) `Views[_objViewMem]` sits outside the `try`. Is a null `_objViewMem` an uncaught throw hazard?**

Yes, and in the modern type the hazard is strictly worse than in the legacy type.

- `_objViewMem` is `private string` (`QfcExplorerController.cs:44`), default `null`.
- It is assigned in exactly one place in the modern type: `ExplConvView_ToggleOff()` at lines 88-90.
- The legacy type additionally assigns it in its constructor (`QuickFileController.cs:145-147`), so a
  legacy instance created while conversations were grouped already had a non-null value before any
  toggle ran. **The modern constructor does not do this.**
- The `Views` indexer parameter is `object` (production code passes a `string` at
  `QfcExplorerController.cs:127` and `QuickFileController.cs:853, 926`). Passing `null` compiles.
  What Outlook's COM implementation does with a null index cannot be determined statically and was
  not verified — no live Outlook process is available in this environment. The plausible outcomes are
  a `System.ArgumentException` or a `System.Runtime.InteropServices.COMException`; either is thrown
  from outside the `try` and therefore propagates uncaught.

Conclusion: a verbatim port would introduce a reachable `NullReferenceException`-class failure into a
public API on the very first call made before `ExplConvView_ToggleOff()`. Any port **must** guard
`_objViewMem` (and should move the resolution inside the protected region), which means the port is
not a port — it is a redesign of behaviour that has no caller.

**(b) The `catch` does not set `BlShowInConversations = false` while the `try` path does. Intentional or defect?**

The asymmetry is defensible as intentional and should **not** be "corrected" inside this issue.

Reading that makes it intentional: the flag means "a conversation-view restore is still owed." On the
success path the restore happened, so the debt is cleared. On the failure path `ObjView.Apply()` did
not succeed, the explorer is still showing the temporary non-conversation view, and the debt stands.
Leaving the flag `true` keeps `ExplConvView_ReturnState()` (`QfcExplorerController.cs:66-70`) willing
to retry.

Reading that makes it a defect: the retry runs `ExplConvView_ToggleOn()`
(`QfcExplorerController.cs:123-131`), whose first statement is the identical
`_activeExplorer.CurrentFolder.Views[_objViewMem]` resolution that just failed, so a retry is expected
to fail identically. The flag then leaks a permanently-true state.

Recommendation: preserve the legacy asymmetry if the member is implemented, record the two readings in
an XML doc comment, and do not change it. Changing it is a behaviour change to a path with no caller,
which the Bugfix Workflow's "change only what is needed" rule forbids. Under the recommended decision
(§1.4) the question does not arise at all.

**(c) `catch (System.Exception)` versus `.claude/rules/general-code-change.md`.**

The rule text is: "Do not use broad catch-all handlers unless you immediately re-raise or propagate
with added context." `CLAUDE.md` C#4.1 repeats it: "Avoid catching broad `Exception` unless at a clear
boundary and with added context." The legacy body swallows silently and adds nothing, so a verbatim
port is a policy violation on its face.

A policy-compliant shape for this body, given the try block is pure COM interop
(`View.Apply()`, `View.Delete()`) plus a missing-view-name lookup:

```csharp
catch (System.Exception ex) when (ex is COMException || ex is ArgumentException)
{
    log.Warn(
        $"Could not restore Outlook view '{_objViewMem}'; removing the temporary view instead.",
        ex
    );
    ObjViewTemp = GetSiblingView((Outlook.View)_activeExplorer.CurrentView, "tmpNoConversation");
    ObjViewTemp?.Delete();
}
```

Two named exception types (`System.Runtime.InteropServices.COMException`, `System.ArgumentException`)
plus a log call satisfies "added context" and is not a catch-all. `COMException` requires adding
`using System.Runtime.InteropServices;` to the file's using block. Note the residual risk: an Outlook
PIA can also surface `System.UnauthorizedAccessException` and `System.InvalidCastException` from these
call paths, and neither would be caught by the narrowed filter, so narrowing changes runtime
behaviour relative to the legacy body. This is a further argument that "port the legacy semantics" is
not achievable without behaviour change.

**(d) Does every piece the port needs exist on the modern type?**

| Legacy member | Modern equivalent | Status |
| --- | --- | --- |
| `ObjView` (public field, `:42`) | `_objView` (private field, `:43`) | Present, renamed and narrowed. |
| `_objViewMem` (`:43`) | `_objViewMem` (`:44`) | Present. **Never initialised by the constructor** — see (a). |
| `ObjViewTemp` (public field, `:44`) | `ObjViewTemp` (public field, `:45`) | Present, identical. |
| `GetSiblingView(View, string)` (`:871-884`) | `GetSiblingView(View, string)` (`:108-121`) | Present, byte-identical body. |
| `BlShowInConversations` (`:185`) | `BlShowInConversations` (`:49-53`) | Present. |
| `_activeExplorer` (`:145` etc.) | `_activeExplorer` (`:42`) | Present. |
| `CurrentConversationState` (`:170`, private) | `CurrentConversationState` (`:55-58`, internal) | Present but **never referenced anywhere in the repository** — zero call sites in `QuickFiler` and zero in `QuickFiler.Test`. |

Nothing the port needs is missing. The only gap is behavioural, not structural: constructor-time
initialisation of `_objViewMem`.

**(e) Is there a logging pattern in place, and is `log` referenced?**

Yes and no, respectively.

`QfcExplorerController.cs:23-25` declares:
```csharp
private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
);
```
A repository-scoped grep for `\blog\b` inside this file returns **exactly one hit — line 23, the
declaration itself.** The field is declared and never used. Any implementation added by this issue
should use it (that is the project's logging pattern and satisfies the "added context" requirement in
(c)); removing the member instead leaves the field unused, which is the status quo and produces no
new diagnostic (the field is `static readonly` with a method-call initialiser, so neither CS0169 nor
CS0414 applies).

### 1.3 Rejected alternative — implement the legacy semantics

Rejected. Summary of why, with the fallback implementation retained in §1.5 in case the planner or a
reviewer overrides this recommendation:

- The body cannot be ported verbatim without violating `.claude/rules/general-code-change.md` (broad
  catch) and without importing an uncaught null-index hazard that the legacy type mitigated in its
  constructor and the modern type does not.
- Correcting both problems means authoring roughly 20 lines of new, previously-nonexistent production
  behaviour for an API with zero callers, which the Bugfix Workflow explicitly scopes out ("change
  only what is needed"; "if you uncover deeper design problems, open a new issue").
- Those 20 lines then need tests, and the tests would assert behaviour no production path consumes.

### 1.4 Recommendation — **remove the member**

Remove `void ExplConvView_Cleanup();` from `QuickFiler/Interfaces/IQfcExplorerController.cs:12` and
remove the implementation and its `//PRIORITY:` comment from `QuickFiler/Controllers/QfcExplorerController.cs:60-64`.

Evidence supporting removal:

1. **Zero callers.** Repository-wide grep for `ExplConvView_Cleanup` across `*.cs` returns four hits:
   `QuickFiler/Interfaces/IQfcExplorerController.cs:12` (declaration),
   `QuickFiler/Controllers/QfcExplorerController.cs:61` (the throwing implementation),
   `QuickFiler/Legacy/QuickFileController.cs:673` and `:851` (uncompiled), and
   `QuickFiler/Notes/notes_interfaces.cs:58` (uncompiled duplicate interface). No compiled production
   or test code calls it.
2. **Zero mock setups.** No file under `QuickFiler.Test` sets up or verifies `ExplConvView_Cleanup`, so
   removing it from the interface breaks no `Mock<IQfcExplorerController>`.
3. **Exactly one implementer**, so the "update all implementers" clause of the acceptance criterion is
   a one-line edit.
4. **Policy alignment.** `CLAUDE.md` §4.2 ("Make the public surface area small and intentional") and
   C#5.2 ("Keep public surface area intentional and minimal") both favour removal of an unimplemented,
   uncalled member. The general policy's compatibility clause ("Avoid breaking public APIs. If a
   breaking change is necessary, update all callers in-repo and call it out clearly") is satisfied:
   there are no callers, and the change is called out here and in the PR body.
5. **It eliminates the trap rather than papering it.** The potential document's own framing is "a live
   trap for the next caller." After removal a would-be caller gets a compile error at authoring time
   instead of a `NotImplementedException` at runtime.

Consequential edits required by removal:
- `QuickFiler/Controllers/QfcExplorerController.cs` — delete lines 60-64.
- `QuickFiler/Interfaces/IQfcExplorerController.cs` — delete line 12.
- Do **not** edit `QuickFiler/Notes/notes_interfaces.cs`. It is not compiled and is outside this
  issue's file set; its duplicate `IQfcExplorerController` declaration at `:52-59` is a documentation
  artefact.
- Removal makes `using System;` (line 1) orphaned — see §4.

Knowledge preservation: the epic forbids any child writing under `docs/features/potential/**`
(Recorded Preconditions), so the legacy body must be preserved in this feature folder instead. Record
the verbatim legacy body and the semantic summary from §1.1 in the feature's `spec.md` under a
"Removed contract — legacy semantics for future restoration" heading, and reference it from the PR
body. Do not rely on the uncompiled `Legacy/` file as the record; it is a deletion candidate for a
later epic.

### 1.5 Fallback implementation, if the decision is overridden to "implement"

```csharp
/// <summary>
/// Restores the Outlook view remembered by <see cref="ExplConvView_ToggleOff"/> and removes the
/// temporary "tmpNoConversation" view. On failure the temporary view is still removed, but
/// <see cref="BlShowInConversations"/> is deliberately left set: the restore did not happen, so
/// the caller still owes one. This asymmetry is inherited from the legacy implementation.
/// </summary>
public void ExplConvView_Cleanup()
{
    if (string.IsNullOrEmpty(_objViewMem))
    {
        // No view was remembered, so there is nothing to restore. Guarding here rather than
        // letting the Views indexer throw: the legacy type initialised _objViewMem in its
        // constructor and this type does not, so a null value is reachable on the first call.
        ObjViewTemp?.Delete();
        BlShowInConversations = false;
        return;
    }

    try
    {
        _objView = _activeExplorer.CurrentFolder.Views[_objViewMem];
        _objView.Apply();
        ObjViewTemp?.Delete();
        BlShowInConversations = false;
    }
    catch (System.Exception ex) when (ex is COMException || ex is ArgumentException)
    {
        log.Warn($"Could not restore Outlook view '{_objViewMem}'.", ex);
        ObjViewTemp = GetSiblingView((Outlook.View)_activeExplorer.CurrentView, "tmpNoConversation");
        ObjViewTemp?.Delete();
    }
}
```
Requires adding `using System.Runtime.InteropServices;`. Note that this is not the legacy behaviour:
the guard is new, the exception filter is narrower, and the resolution moved inside the `try`.

---

## 2. Q2 — Defect 2 remedy

### 2.1 The remedy

At `QuickFiler/Controllers/QfcExplorerController.cs:140`, replace

```csharp
_globals.Ol.App.ActiveExplorer().CurrentFolder = (MAPIFolder)mailItem.Parent;
```

with

```csharp
_activeExplorer.CurrentFolder = (MAPIFolder)mailItem.Parent;
```

Confirmed as the correct and complete remedy. It is a one-line change inside the private helper
`NavigateToOutlookFolder(MailItem)` (lines 133-143).

### 2.2 Every other `_globals` use in the file

Repository grep for `_globals` within `QfcExplorerController.cs` returns six hits:

| Line | Use | Assessment |
| --- | --- | --- |
| 34 | `_globals = appGlobals;` | Constructor assignment. Unchanged. |
| 35 | `_activeExplorer = _globals.Ol.App.ActiveExplorer();` | The single authoritative capture. Unchanged. |
| 40 | `private IApplicationGlobals _globals;` | Field declaration. Unchanged. |
| 90 | `_objViewMem = _globals.Ol.ViewWide;` | Reads a settings string, not a COM re-resolution. Unchanged. |
| 140 | `_globals.Ol.App.ActiveExplorer().CurrentFolder = ...` | **The defect.** |
| 162 | `//MAPIFolder drafts = _globals.Ol.NamespaceMAPI...` | Commented out. Unchanged. |

**Line 140 is the only re-resolution in the file.** No other member re-derives the explorer.

### 2.3 Is there a behavioural dependency on the fresh call?

No. Analysis:

- `_activeExplorer` is assigned once, at line 35, and is never reassigned anywhere in the file (no
  other `_activeExplorer =` occurrence exists).
- Nothing in `QfcExplorerController` subscribes to Outlook explorer lifecycle events, and no public
  member accepts a replacement explorer.
- Every other COM operation in the type — `CurrentConversationState` (line 57),
  `ExplConvView_ToggleOff` (74, 77, 81), `ExplConvView_ToggleOn` (127),
  `NavigateToOutlookFolder`'s own guard (line 136), `AutoFile.AreConversationsGrouped` (141, 152),
  `IsItemSelectableInView`/`ClearSelection`/`AddToSelection` (156, 158, 159) — already uses
  `_activeExplorer`.
- Line 136 reads `_activeExplorer.CurrentFolder.FolderPath` and line 140 writes
  `ActiveExplorer().CurrentFolder`. As written, the guard and the assignment can address **different
  Explorer objects**, which is exactly the internal-inconsistency hazard the potential document
  describes. The fix makes read and write address the same object, which is the stronger correctness
  argument, ahead of the saved COM round-trip.

There is therefore no code path requiring the fresh call, and no in-code documentation of one is
needed. The acceptance criterion's alternative branch ("or the reason a fresh `ActiveExplorer()` call
is required is documented in code") does not apply.

---

## 3. Q3 — Defect 3 deletion safety

Repository-wide grep over `*.cs` for the six identifiers. Results split by location.

### 3.1 References inside `QuickFiler/Controllers/QfcExplorerController.cs`

All hits fall inside the region 183-321, with none outside it.

| Symbol | Declaration | In-file call sites |
| --- | --- | --- |
| `SanitizeArrayLineTSV` | 185 (`private static`) | none |
| `StripTabsCrLf` | 203 (`internal static`) | 193, 264 |
| `WriteCSV_StartNewFileIfDoesNotExist` | 216 (`private static`) | none (comment at 215) |
| `SanitizeArray` | 249 (`private static`) | 241 |
| `SaveMessageAsMSG` | 272 (`private static`) | none (comment at 271) |
| `GetCurrentExplorerFolder` | 278 (`private static`) | none (comment at 277) |

Three of the six (`SanitizeArrayLineTSV`, `SaveMessageAsMSG`, `GetCurrentExplorerFolder`) have **zero**
call sites even inside the region. Two (`WriteCSV_StartNewFileIfDoesNotExist` is the only entry point,
and it is itself uncalled) form a closed island.

### 3.2 References outside `QfcExplorerController.cs`

Every external hit binds to a different type. Grouped by owning type:

- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` — declares its own
  `SaveMessageAsMSG` (`:1092`, `internal static`, different signature: `(MailItem, string)`),
  `SanitizeArrayLineTSV` (`:1344`), `StripTabsCrLf` (`:1361`),
  `WriteCSV_StartNewFileIfDoesNotExist` (`:1374`, `public static`), `SanitizeArray` (`:1407`).
  Internal call sites at `:491`, `:1338`, `:1353`, `:1399`, `:1420`.
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` — declares instance copies:
  `SanitizeArrayLineTSV` (`:211`, `private`), `StripTabsCrLf` (`:224`, `internal`). Call sites at
  `:196`, `:218`.
- `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` — declares its own copies of all six:
  `SanitizeArrayLineTSV` (`:255`), `StripTabsCrLf` (`:273`),
  `WriteCSV_StartNewFileIfDoesNotExist` (`:285`), `SanitizeArray` (`:317`), `SaveMessageAsMSG` (`:350`),
  `GetCurrentExplorerFolder` (`:355`). Call sites at `:77`, `:84`, `:125`, `:223`, `:229`, `:263`,
  `:310`, `:330`.
- `TaskMaster/AppGlobals/AppOlObjects.cs:279` — calls `SortEmail.WriteCSV_StartNewFileIfDoesNotExist(...)`,
  explicitly type-qualified to `SortEmail`.

None of these resolve to a `QfcExplorerController` member. `QfcExplorerController` is `internal` to the
`QuickFiler` assembly, and none of `UtilitiesCS`, `ToDoModel`, or `TaskMaster` references `QuickFiler`
for these symbols; the type qualification at `AppOlObjects.cs:279` removes any residual ambiguity.

### 3.3 `StripTabsCrLf` — the `internal static` case, explicitly

`StripTabsCrLf` at `QfcExplorerController.cs:203` is `internal static`, so it is reachable from
anywhere in the `QuickFiler` assembly and, via `[assembly: InternalsVisibleTo("QuickFiler.Test")]`
(`QuickFiler/Properties/AssemblyInfo.cs:5`), from `QuickFiler.Test`.

**No file under `QuickFiler.Test` references any of the six symbols.** The only test references in the
repository are:
- `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs:141, 147, 158, 164, 277, 295, 302, 307, 320, 324` — all against `SortEmail.*`.
- `UtilitiesCS.Test/EmailIntelligence/EmailFiler_Tests.cs:43, 47` — against an `EmailFiler` instance.

Additional hits appear in `docs/features/archive/.../vstest-final.txt` and in the potential document
itself; both are text artefacts, not code.

**Conclusion: the deletion plan is unchanged. No test edit is required and no `QuickFiler` production
file other than `QfcExplorerController.cs` is affected.** Deleting lines 183-321 (139 lines) is
behaviour-neutral for the `QuickFiler` assembly, and takes the file from 323 to approximately 184
lines.

### 3.4 The two latent defects inside the block

Both confirmed, both unreachable, both disappear with the deletion:
- `WriteCSV_StartNewFileIfDoesNotExist` line 223 — `File.Exists(Path.Combine(strFileName, strFileLocation))`
  transposes the arguments relative to line 242's `FileIO2.WriteTextFile(strFileName, strOutput, folderpath: strFileLocation)`.
- `SanitizeArray` line 221/241/259 — `strOutput` is initialised to `null` at line 221 and passed
  `ref` into `SanitizeArray`, which writes `strOutput[j]` at line 259 without allocating. This throws
  `NullReferenceException` if reached.

Neither can fire today. Do not "fix" them; delete them.

---

## 4. Q4 — Orphaned `using` directives

### 4.1 Per-directive determination

The using block is lines 1-16. The determination below enumerates every type reference in lines 1-182
and, separately, the effect of removing `ExplConvView_Cleanup` (§1.4).

| Line | Directive | Required by lines 1-182? | Verdict after region deletion |
| --- | --- | --- | --- |
| 1 | `using System;` | `NotImplementedException` at line 63 **only**. `System.Reflection.MethodBase` at line 24 is fully qualified; `log4net.ILog` at 23 is fully qualified. | **Retained** if `ExplConvView_Cleanup` is implemented (§1.5) or kept. **ORPHANED** under the recommended removal (§1.4), because line 63 is the last `System`-namespace reference. |
| 2 | `using System.Collections.Generic;` | No. Only use is `IList<MailItem>` in `SaveMessageAsMSG` (line 272). The `foreach` at line 112 needs no using. | **ORPHANED** |
| 3 | `using System.Diagnostics;` | No. Only use is `Debug.WriteLine` (line 253). | **ORPHANED** |
| 4 | `using System.Diagnostics.CodeAnalysis;` | Yes — `[ExcludeFromCodeCoverage]` at line 20. | **Retained** if the attribute stays or is narrowed (§6.3). Orphaned only if the attribute is removed outright. |
| 5 | `using System.IO;` | No. Only uses are `File.Exists` and `Path.Combine` (line 223). | **ORPHANED** |
| 6 | `using System.Linq;` | No. Only uses are `.Where`/`.Select`/`.ToArray` at 192-194 and 263-265. | **ORPHANED** |
| 7 | `using System.Text;` | No — and no use anywhere in the file, including the dead region. No `StringBuilder`, no `Encoding`. | **Already orphaned before this change** (pre-existing). |
| 8 | `using System.Text.RegularExpressions;` | No. Only use is `Regex` at lines 205 and 209. | **ORPHANED** |
| 9 | `using System.Threading.Tasks;` | Yes — `Task` at 146, 154, 158, 159, 180. | **Retained** |
| 10 | `using System.Windows.Forms;` | Yes — `DialogResult` (168), `MessageBox` (168), `MessageBoxButtons` (171), `MessageBoxIcon` (172). | **Retained** |
| 11 | `using Microsoft.Office.Interop.Outlook;` | Yes — `Explorer` (42), `MailItem` (133, 146), `MAPIFolder` (136, 140), `Views` (111), `OlViewSaveOption` (99). | **Retained** |
| 12 | `using QuickFiler.Interfaces;` | Yes — `IQfcExplorerController` (21), `IFilerHomeController` (30, 41). | **Retained** |
| 13 | `using ToDoModel;` | No — see §4.2. | **Already orphaned before this change** (pre-existing). |
| 14 | `using UtilitiesCS;` | Yes — `IApplicationGlobals` (29, 40), `AutoFile` (141, 152). | **Retained** |
| 15 | `using UtilitiesCS.OutlookExtensions;` | No — see §4.2. | **Already orphaned before this change** (pre-existing). |
| 16 | `using Outlook = Microsoft.Office.Interop.Outlook;` | Yes — `Outlook.View` at 43, 45, 77, 93, 97, 101, 108, 110, 112, 123. | **Retained** |

### 4.2 Namespace resolution for the specific symbols asked about

| Symbol | Declaring namespace | Which directive supplies it |
| --- | --- | --- |
| `QfEnums` | `QuickFiler` (`QuickFiler/Helper Classes/QfEnums.cs:1-3`) | **None.** The file's namespace is `QuickFiler.Controllers`, so `QuickFiler` is reachable by enclosing-namespace lookup. `using ToDoModel;` does **not** supply it. |
| `AutoFile` | `UtilitiesCS` (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/AutoFile.cs:11-13`) | `using UtilitiesCS;` |
| `IApplicationGlobals` | `UtilitiesCS` (`UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs:5-7`) | `using UtilitiesCS;` |
| `IFilerHomeController` | `QuickFiler.Interfaces` (`QuickFiler/Interfaces/IFilerHomeController.cs:9-11`) | `using QuickFiler.Interfaces;` |
| `GetPressedMso` | **Not an extension method.** It is a native member of `Microsoft.Office.Core.CommandBars`. Proof: `UtilitiesCS.Test/EmailIntelligence/AutoFile_Tests.cs:57` does `mockCommandBars.Setup(cb => cb.GetPressedMso("ShowInConversations"))` — Moq can only `Setup` an interface or virtual member, never an extension method. | **None.** Member access off a returned value requires no using. |
| `IsItemSelectableInView` | **Not an extension method.** Native member of `Microsoft.Office.Interop.Outlook.Explorer`. Proof: `TaskTree.Test/TaskTreeControllerActivateTests.cs:57` does `explorer.Setup(e => e.IsItemSelectableInView(It.IsAny<object>()))`. | **None.** |
| `IsInitialized` | `UtilitiesCS` — `UtilitiesCS/Extensions/ArrayExtensions.cs:9` (namespace) / `:193` (the `T[]` overload used at line 187). | `using UtilitiesCS;` — **not** `UtilitiesCS.OutlookExtensions`. |
| `SliceRow` | `UtilitiesCS` — `UtilitiesCS/Extensions/ArrayExtensions.cs:102`. | `using UtilitiesCS;` |
| `FileIO2` (line 242) | `UtilitiesCS` — `UtilitiesCS/To Depricate/FileIO2.cs:12`. | `using UtilitiesCS;` |

This is why `using ToDoModel;` and `using UtilitiesCS.OutlookExtensions;` are already unused today:
the two extension methods the region uses live in the root `UtilitiesCS` namespace, and the two
Outlook members that look like extensions are native PIA members.

**`System.Text` is already unused before the deletion.** So are `ToDoModel` and
`UtilitiesCS.OutlookExtensions`. Three of the ten directives listed in the question are pre-existing
orphans; five more become orphans as a consequence of the deletion; `System` becomes an orphan only
under the recommended Q1 decision; `System.Diagnostics.CodeAnalysis` depends on the Q6 decision.

**Caveat:** this determination is symbol-level, not compiler-verified. Extension-method resolution can
surprise. The removal is self-verifying, though: if any directive is actually required, the analyzer
build fails with CS0246/CS1061 and the executor restores it. Removal is therefore low-risk; retention
is zero-risk.

### 4.3 Is an unused `using` enforced by the build here?

**No. Leaving an orphaned using would not fail either gate. Removal is hygiene, not a gate requirement.**

Evidence:

1. **No `IDE0005` severity is configured.** Grep of the repo-root `.editorconfig` for `IDE0` returns
   zero hits. There is no `.globalconfig` in the repository.
2. **The analyzer that produces `IDE0005` is not wired into this project.** `QuickFiler/QuickFiler.csproj`
   is a legacy non-SDK project (`<Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props">`
   at line 4, `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` at line 13,
   `<Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />` at line 567). Its
   `<Analyzer Include>` set (lines 582-591) is Meziantou, Roslynator, AsyncFixer,
   `Microsoft.CodeAnalysis.BannedApiAnalyzers`, and `SonarAnalyzer.CSharp`. Neither
   `Microsoft.CodeAnalysis.NetAnalyzers` nor `Microsoft.CodeAnalysis.CSharp.CodeStyle` — the packages
   that carry `IDE0005` — is referenced. The command-line `/p:EnableNETAnalyzers=true`
   `/p:EnforceCodeStyleInBuild=true` properties are SDK-project properties and do not inject an
   analyzer into a non-SDK project.
3. **The compiler's own `CS8019` ("unnecessary using directive") is a hidden diagnostic**, not a
   warning, so `/p:TreatWarningsAsErrors=true` does not promote it.
4. **Empirical confirmation.** `using System.Text;`, `using ToDoModel;` and
   `using UtilitiesCS.OutlookExtensions;` are unused in this file *today*, on `main`, and both the
   analyzer gate and the nullable gate are green there. If any wired analyzer (for example Sonar
   `S1128`, which is absent from the `.editorconfig` severity list and therefore keeps its package
   default) reported unused usings at `warning` severity, the type-check gate would already be red.

Recommendation: remove the eight-to-ten orphaned directives anyway. `CLAUDE.md` C#5.3 ("Prefer
explicit `using` directives at file scope") and the general policy's "Prefer clear, explicit imports"
both favour it, `csharpier` will not reorder or remove them for you, and doing it in the same commit
as the region deletion keeps the diff coherent. State in the PR body that this is hygiene, not a gate
fix, so a reviewer does not read it as an unrelated refactor.

---

## 5. Q5 — Test harness design

### 5.1 File, class, and project-file entry

- **Path:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a78a924c87d7f1f73\QuickFiler.Test\Controllers\QfcExplorerControllerTests.cs`
- **Class:** `QfcExplorerControllerTests`
- **Namespace:** `QuickFiler.Controllers.Tests` — matches `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:20`.

Recommended without reservation. Verified no collision: no file under `QuickFiler.Test/Controllers/`
matches `*Explorer*`; the closest names are `EfcHomeController*Tests.cs`, none of which conflicts.

**Project-file entry.** `QuickFiler.Test/QuickFiler.Test.csproj` needs one appended line. The
`Controllers` compile entries run from `:58` to `:158`, and `:158` is the last of them
(`<Compile Include="Controllers\QfcQueueTests.cs" />`). Append immediately after line 158:

```xml
<Compile Include="Controllers\QfcExplorerControllerTests.cs" />
```

This satisfies the epic's Shared-Surface Coordination partition exactly: it does not touch the `Form1`
region at `:161-166` (owned by #491) nor the `Form1.resx` `EmbeddedResource` at `:180-182`. The file is
currently 484 lines; the append makes it 485, still under the 500-line cap. Note for coordination:
#491's removal of the `Form1` entries reduces it by 8 lines, so the two children move it in opposite
directions with net headroom.

### 5.2 The constructor's mock graph

`QfcExplorerController(QfEnums.InitTypeEnum, IApplicationGlobals, IFilerHomeController)` reaches COM at
line 35 only: `_globals.Ol.App.ActiveExplorer()`.

Chain, with the declaring definitions:
- `UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs:11` — `IOlObjects Ol { get; }`
- `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:13` — `Application App { get; }`
  (`Microsoft.Office.Interop.Outlook.Application`)
- `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:28` — `string ViewWide { get; }` (needed by
  `ExplConvView_ToggleOff` at line 90)
- `QuickFiler/Interfaces/IFilerHomeController.cs:31` — `IFilerFormController FormController { get; }`
- `QuickFiler/Interfaces/IFilerFormController.cs:17` — `void MinimizeFormViewer();`

**Existing test that already builds this exact chain:** `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:39-47`,
using a `MockRepository(MockBehavior.Strict)` and Moq's recursive `SetupGet(x => x.Ol.App)`:

```csharp
this._mockRepository = new MockRepository(MockBehavior.Strict);
this._mockApplicationGlobals = this._mockRepository.Create<IApplicationGlobals>();
this._mockOlApp = this._mockRepository.Create<Outlook.Application>();
this._mockExplorer = this._mockRepository.Create<Explorer>();
this._mockOlApp.Setup(x => x.ActiveExplorer()).Returns(_mockExplorer.Object);
this._mockApplicationGlobals.SetupGet(x => x.Ol.App).Returns(_mockOlApp.Object);
```

A second precedent for the same chain shape is `QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs:54`
and `QfcHomeControllerIssue218Tests.cs:36`.

Minimal fixture for this issue:

```csharp
var repo = new MockRepository(MockBehavior.Loose);

var commandBars = repo.Create<Microsoft.Office.Core.CommandBars>();
commandBars.Setup(c => c.GetPressedMso("ShowInConversations")).Returns(false);

var explorer = repo.Create<Outlook.Explorer>();
explorer.Setup(e => e.CommandBars).Returns(commandBars.Object);

var olApp = repo.Create<Outlook.Application>();
olApp.Setup(a => a.ActiveExplorer()).Returns(explorer.Object);

var globals = repo.Create<IApplicationGlobals>();
globals.SetupGet(g => g.Ol.App).Returns(olApp.Object);
globals.SetupGet(g => g.Ol.ViewWide).Returns("Wide");   // only for ToggleOff tests

var formController = repo.Create<IFilerFormController>();
var parent = repo.Create<IFilerHomeController>();
parent.SetupGet(p => p.FormController).Returns(formController.Object);

var controller = new QfcExplorerController(
    QfEnums.InitTypeEnum.Find,      // deliberately NOT Sort — see 5.4
    globals.Object,
    parent.Object
);
```

Assembly references are already present: `QuickFiler.Test/QuickFiler.Test.csproj:278-280` references
`Microsoft.Office.Interop.Outlook` and `:326-328` references `office` (the `Microsoft.Office.Core`
PIA), both with `<EmbedInteropTypes>False</EmbedInteropTypes>`, which is what Moq requires.
`Mock<CommandBars>` is proven at `UtilitiesCS.Test/EmailIntelligence/AutoFile_Tests.cs:56-59`.

### 5.3 Mockability audit of the specific members asked about

| Member | Declaring type | Interface member? | Mockable? | Evidence |
| --- | --- | --- | --- | --- |
| `Explorer.CurrentFolder` (get and set) | `Microsoft.Office.Interop.Outlook.Explorer` | Yes | **Yes** | Production assigns it at `QfcExplorerController.cs:140`, so the setter exists. `Mock<Explorer>` proven at `QuickFiler.Test/Controllers/QfcDatamodelTests.cs:259` and `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:529`. `VerifySet` is the natural assertion for defect 2. |
| `MAPIFolder.Views` | `MAPIFolder` | Yes | **Yes** | `Mock<MAPIFolder>` proven at `TaskMaster.Test/Ribbon/RibbonControllerTests.cs:365` and `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs:57`. |
| `Views` indexer | `Views` | Yes (C# indexer; production binds it at `QfcExplorerController.cs:127` and `QuickFileController.cs:853, 926`) | **Yes, with one confirmation step** | No `Mock<Views>` exists in the repo yet. Indexer mocking is proven on another Outlook collection at `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs:64-65` (`mockRecipients.Setup(x => x[It.IsAny<int>()])`). **Confirm at implementation time** that the PIA indexer parameter is `object` rather than a typed overload; the setup is then `views.Setup(v => v[It.IsAny<object>()]).Returns(view.Object)`. If the compiler rejects that shape, the parameter type is the only thing to adjust. |
| `View.Apply()` | `Outlook.View` | Yes | **Yes** | `Mock<Outlook.View>` proven at `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:530-531` (sets `v.Name`). `Apply()`, `Save()`, `Copy(string, OlViewSaveOption)`, `XML`, `Parent` are on the same interface. |
| `View.Delete()` | `Outlook.View` | Yes | **Yes** | Same interface. Only needed if Q1 is decided as "implement". |
| `Views` enumeration (for `GetSiblingView`, line 112) | `Views : IEnumerable` | Yes | **Yes** | Two proven forms in-repo: direct, `mockUDPs.Setup(u => u.GetEnumerator()).Returns(list.GetEnumerator())` at `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs:55` and `:838`; and the `.As<IEnumerable>()` fallback at `:581-583`. |
| `Explorer.CurrentView` | `Explorer` | Yes, returns `object` | **Yes** | `mockExplorer.Setup(e => e.CurrentView).Returns(mockView.Object)` at `OlTableExtensions_Tests.cs:532`. |
| `Explorer.CommandBars` / `CommandBars.GetPressedMso` | `Explorer` / `Microsoft.Office.Core.CommandBars` | Yes | **Yes** | `AutoFile_Tests.cs:56-59`. |
| `Explorer.IsItemSelectableInView`, `ClearSelection`, `AddToSelection` | `Explorer` | Yes | **Yes** | `TaskTree.Test/TaskTreeControllerActivateTests.cs:57` for the first. |
| `MailItem.Parent` | `MailItem` | Yes, returns `object` | **Yes** | Cast to `MAPIFolder` at lines 136, 140. |

**Nothing in the changed paths is unmockable.** No sealed class, no static, no non-virtual concrete
member stands in the way. This is the same evidence that undercuts the coverage exemption in §6.2.

### 5.4 Untestable members, and how to avoid them

The tests must not create a live form, must not start a message pump, must not use temporary files,
and must not call `MessageBox.Show`. Assessment:

- **`OpenQFItem`'s else branch, lines 166-178**, is the only truly untestable region. Line 168 calls
  `MessageBox.Show(...)` — a modal WinForms dialog — and line 176 calls `mailItem.Display()`. This
  branch **must be left uncovered**. It is reached only when `_activeExplorer.IsItemSelectableInView(mailItem)`
  returns `false` (line 156).
- **Everything else in `OpenQFItem` is testable**, because the branch is selectable: set
  `explorer.Setup(e => e.IsItemSelectableInView(It.IsAny<object>())).Returns(true)` and the method
  takes the `ClearSelection`/`AddToSelection` path (158-159) instead.
- **`_parent.FormController.MinimizeFormViewer()` (line 148) is not a barrier.** `IFilerFormController`
  is an interface (`QuickFiler/Interfaces/IFilerFormController.cs:17`) and `MinimizeFormViewer()` is a
  `void` member on it. The mock chain is two lines (see 5.2). `MinimizeFormViewer` has a real
  implementation at `QfcFormController.Actions.cs:197` that touches a form, but the test never
  constructs it. **`OpenQFItem` should therefore be IN scope for tests, not excluded.**
- **`Task.Run` at lines 154, 158, 159, 180** is production async, not a test timing hack. The method is
  `await`-ed by the test, so the result is deterministic. No `Task.Delay` and no `Thread.Sleep` is
  introduced, so `.claude/rules/general-unit-test.md`'s banned-API list is respected. Moq mocks are
  invoked from the thread-pool thread, which is safe.
- **`CurrentConversationState` (lines 55-58)** is `internal` and testable via `InternalsVisibleTo`; it
  needs only the `CommandBars` setup.

Branch control for a defect-2 test: pass `QfEnums.InitTypeEnum.Find` (value 2, per
`QuickFiler/Helper Classes/QfEnums.cs:8`) so `_initType.HasFlag(QfEnums.InitTypeEnum.Sort)` is false
at lines 151 and 179. Note that both use the **non-short-circuiting `&`**, so
`AutoFile.AreConversationsGrouped(_activeExplorer)` is still evaluated and the `CommandBars` setup
remains mandatory. That is a real behavioural detail worth a comment in the test.

### 5.5 Recommended test set

| # | Test | Target | Notes |
| --- | --- | --- | --- |
| 1 | `OpenQFItem_WhenActiveExplorerChangesAfterConstruction_UsesTheConstructorCapturedExplorer` | Defect 2 | The fail-before test. See §8.2. |
| 2 | `OpenQFItem_WhenMailIsAlreadyInTheCurrentFolder_DoesNotChangeCurrentFolder` | Defect 2 guard (lines 135-137) | Same `FolderPath` on both sides. |
| 3 | `OpenQFItem_WhenItemIsSelectableInView_ClearsAndAddsSelection` | lines 156-159 | Positive path. |
| 4 | `ExplConvView_ToggleOn_WhenFlagSet_AppliesRememberedView` | lines 123-131 | Requires the `Views` indexer mock. |
| 5 | `ExplConvView_ToggleOn_WhenFlagClear_DoesNothing` | line 125 negative branch | |
| 6 | `ExplConvView_ToggleOff_WhenConversationsNotGrouped_DoesNothing` | line 74 negative branch | |
| 7 | `ExplConvView_ToggleOff_WhenSiblingViewMissing_CopiesAndSavesTemporaryView` | lines 95-103 | Exercises `GetSiblingView` returning null plus `View.Copy`. |
| 8 | `GetSiblingView_WhenNamedViewPresent_ReturnsIt` / `_WhenAbsent_ReturnsNull` | lines 108-121 | Uses the `GetEnumerator` precedent. |
| 9 | `CurrentConversationState_ReflectsCommandBarPressedState` | lines 55-58 | Two cases. |
| 10 | `ExplConvView_ReturnState_WhenFlagSet_TogglesOn` | lines 66-70 | |
| 11 | `Contract_ExplConvView_Cleanup_IsNotDeclaredOnTheInterface` | Defect 1, **optional** | Reflection assertion; see §8.1 for why the dossier is preferred instead. |

Keep the file under 500 lines. If tests 1-11 exceed it, split into
`QfcExplorerControllerTests.cs` and `QfcExplorerController.ConversationViewTests.cs` and append two
csproj lines rather than one — still within the partitioned region.

### 5.6 One pre-existing policy tension to flag in the spec

`.claude/rules/general-unit-test.md` ("Test File Location") requires test files to live in a `tests/`
directory tree mirroring the production source. This repository's entire C# corpus uses
`<Project>.Test/` sibling projects instead, and `CLAUDE.md`'s C# Unit Test Policy — which sits above
the rule summaries in the compliance order — does not restate the `tests/` requirement. Placing the
new file at `QuickFiler.Test/Controllers/` matches the repository and matches the epic's explicit
instruction ("#449 owns one appended `Compile Include` ... It appends to the `Controllers` item
group"). Record this in the spec so `feature-review` does not raise it as a new violation.

---

## 6. Q6 — Coverage story

### 6.1 Does the class-level attribute make the deletion coverage-neutral? **Yes.**

The tooling evidence is in-repo and direct. `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1:217-222`
states, as the premise of the whole filter:

> "A method-level `[ExcludeFromCodeCoverage]` attribute suppresses the attributed member but not the
> lambdas declared inside it ... This filter removes them by inferring exemption from the declaring
> member's absence from the instrumented method set of the same declaring type and source file."

That is, the tool's own design depends on the fact that an exempt member emits **no `<method>`
element** in the Cobertura output. A *class*-level attribute suppresses every member, so
`QfcExplorerController` contributes no `<class>` and no lines to the report at all.

Corroborating configuration:
- `coverage.config` (24 lines) excludes only third-party module paths (`Deedle`, `FSharp`,
  `Castle.Core`, `FluentAssertions`, `Moq`, `Microsoft.Testing`, `MSTest`). It contains no
  `QuickFiler` entry and no source-level exclusion, so the attribute is the only mechanism in play.
- `Directory.Build.targets` (30 lines) concerns VSTO manifest and assembly signing only. It has no
  coverage content.

**Conclusion: the potential document's claim is false.** Deleting lines 183-321 changes the coverage
denominator by zero while the class-level attribute is present. The genuine benefits of the deletion
are: removal of duplicated code that can drift from the maintained `UtilitiesCS` copies, removal of
two latent defects, and a 139-line reduction toward the file-size cap. The spec should restate the
benefit in those terms and explicitly correct the potential document's wording.

### 6.2 Does `CLAUDE.md` clause (c) still apply to this class?

**No, on two independent grounds.** Clause (c) exempts:

> "Outlook Interop event handler classes in `TaskVisualization`, `QuickFiler`, `TaskMaster`,
> `ToDoModel`, and `Tags` that directly depend on `Microsoft.Office.Interop.Outlook.Application`,
> `MailItem`, `Store`, or `MAPIFolder` **without an injectable seam**."

1. **`QfcExplorerController` is not an event handler class.** It subscribes to no Outlook event, wires
   no `Explorer` or `Application` event, and declares no event handler method. It is a command
   controller.
2. **It has an injectable seam.** `IApplicationGlobals` is constructor-injected (line 29), and every
   COM object it touches is reached through that seam or through the `Explorer` captured from it at
   line 35. §5.3 demonstrates that all ten relevant members are mockable, with a proven in-repo
   precedent for each. The clause's own qualifying condition is therefore not satisfied.

Clause (c) also carries a counter-clause that points the same way: "Testable seams within otherwise
COM-bound assemblies ... are explicitly NOT exempt and must meet the `>= 80%` floor."

### 6.3 The `.claude/rules/general-unit-test.md` conflict, and the recommended reading

`.claude/rules/general-unit-test.md` (Coverage Exclusion Policy) states flatly: "No production file may
be excluded from coverage measurement," and instructs feature-review agents to treat any `exclude`
entry matching a production source path as **Blocking**. Its enumerated enforcement target is
tooling-config `exclude` entries, not source attributes — but `CLAUDE.md` UT2 itself treats the two as
the same instrument ("Exemption is applied via `[ExcludeFromCodeCoverage]` attributes in source code
(reviewable in PRs) **or** via `coverage.config` assembly-level excludes").

**Recommended reading**, and the one the spec should record:

> The class-level `[ExcludeFromCodeCoverage]` on `QfcExplorerController` is not grounded in either
> policy. `CLAUDE.md` clause (c) does not reach it (not an event handler; has an injectable seam), and
> `.claude/rules/general-unit-test.md` forbids excluding a production file from measurement outright.
> The attribute is a pre-existing, unratified exclusion.

**Risk under `feature-review`:** touching a file that carries an unratified production-file exclusion
invites the reviewer to raise the exclusion as Blocking on this PR, even though this change did not
introduce it (it was added 2026-06-13 in commit `a564add0d`). Leaving the attribute entirely
untouched is the option most likely to draw that finding; narrowing it, with the reasoning above
written into the PR body, is the option that pre-empts it.

Also record for the reviewer: **the only machine-enforced numeric coverage gate in this repository is
a repo-wide 80% line rate.** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-489` throws
when `line-rate * 100 < 80`, reading the root `line-rate` attribute of the merged Cobertura document.
There is no per-file gate, no per-assembly gate, and no branch-coverage gate anywhere in
`scripts/`. The uniform 85%/75% thresholds in `.claude/rules/general-unit-test.md` and
`.claude/rules/quality-tiers.md` are not enforced by any script found in this worktree, and
`quality-tiers.yml` — which `quality-tiers.md` names as its source of truth — does not exist. State
this in the spec so the plan does not gate on an unenforceable number.

### 6.4 Recommendation — **NARROW the attribute**

Three options, weighed against the epic NFR "Coverage of `QuickFiler.csproj` is retained or improved
at every child merge."

| Option | Denominator effect | NFR effect | Assessment |
| --- | --- | --- | --- |
| **KEEP** the class-level attribute | Zero. Class stays invisible. | Trivially satisfied (nothing changes). | Safest for the gate, but leaves an unratified exclusion on a file this PR touches, and makes every test written in §5 invisible to the metric. The tests still run and still prove the fix; they just earn nothing. |
| **REMOVE** it outright | Whole class enters the denominator, including `OpenQFItem`'s untestable `MessageBox` branch (lines 166-178). | At risk. After deletion the class is ~184 lines; the modal-dialog branch is 8-10 executable lines. Reaching the aspirational 85% line figure on the class is marginal, though the enforced repo-wide 80% gate is unaffected by a single small class. | Most policy-pure, highest scope cost, and the only option that can plausibly move a number in the wrong direction. |
| **NARROW** — remove the class-level attribute and apply `[ExcludeFromCodeCoverage]` to `OpenQFItem` alone | Everything except `OpenQFItem` enters the denominator. `NavigateToOutlookFolder` is a *separate private method*, so it stays instrumented and is covered through `OpenQFItem` calls made by the tests. | **Improved.** The newly-measured members are all fully coverable (§5.3), so they enter at a high covered ratio and raise `QuickFiler.csproj`'s figure. | **Recommended.** |

**Recommendation: NARROW.** Specifically:

1. Delete `[ExcludeFromCodeCoverage]` from line 20.
2. Add `[ExcludeFromCodeCoverage]` immediately above `public async Task OpenQFItem(MailItem mailItem)`
   (currently line 146), with an in-code comment recording the exact reason: the else branch calls
   `MessageBox.Show`, a modal WinForms dialog that cannot be exercised in a headless unit test, and no
   modal-dialog seam is reachable from `QuickFiler.Test` (see §6.5).
3. Keep `using System.Diagnostics.CodeAnalysis;` (line 4) — still required.
4. Note that this preserves coverage of the defect-2 fix site: `NavigateToOutlookFolder` (lines
   133-143) is not attributed and remains in the denominator, covered by tests 1-3 of §5.5.

If the planner judges even this to be out of scope for a defect-fix issue, **KEEP** is the acceptable
fallback; **REMOVE** should not be chosen, because it puts an untestable modal-dialog branch into the
denominator with no seam available to retire it.

### 6.5 Rejected alternative for `OpenQFItem`'s dialog

`UtilitiesCS` has a modal-dialog seam — `MyBox.DialogInvoker`
(`UtilitiesCS/Dialogs/MyBox.cs:41-45`), exercised at `UtilitiesCS.Test/Dialogs/MyBox_ShowDialog_Tests.cs`
and `UtilitiesCS.Test/EmailIntelligence/AutoFile_Tests.cs:43`. Rejected for two reasons:

1. `DialogInvoker` is declared `internal`, and `UtilitiesCS/Properties/AssemblyInfo.cs:18-20` grants
   `InternalsVisibleTo` only to `DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, and `ToDoModel.Test`.
   `QuickFiler.Test` is not on the list, so using the seam would require editing a shared surface
   outside this issue's file set.
2. Replacing `MessageBox.Show` with `MyBox.ShowDialog` changes the dialog the user sees. That is a
   behaviour change beyond the three defects and is forbidden by the Bugfix Workflow's minimal-fix
   rule.

---

## 7. Q7 — File-size cap

`.claude/rules/general-code-change.md` caps production, test, and reusable-script files at **500 lines**.
Measured line counts in this worktree:

| File | Lines | Over cap? | Touched by this change? |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcExplorerController.cs` | **323** | No | **Yes** — drops to approximately 184 after the region deletion, 179 after also removing `ExplConvView_Cleanup`. |
| `QuickFiler/Interfaces/IQfcExplorerController.cs` | **15** | No | **Yes** — one line removed (14). |
| `QuickFiler/Legacy/QuickFileController.cs` | **1,065** | **Yes, 2.1x** | **No.** Read-only reference. Not compiled (no `<Compile Include="Legacy\...">` entry anywhere in `QuickFiler.csproj`). Pre-existing violation, not caused or worsened by this change. |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` | **465** | No | **No** — see below. |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | **1,429** | **Yes, 2.9x** | **No** — see below. Pre-existing violation, not caused or worsened by this change. |
| `QuickFiler.Test/QuickFiler.Test.csproj` | **484** | No | **Yes** — one appended line (485). 15 lines of headroom remain. |
| `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs` | 0 (new) | Must stay under 500 | **Yes** — new file. |
| `QuickFiler/QuickFiler.csproj` | 595 | Above 500, but a generated non-SDK project file, not authored source | **No** — no production csproj edit is required. |

### `EmailFiler.cs` and `SortEmail.cs` need NO edit — orchestrator's reading CONFIRMED

Verified by symbol-level inspection (§3.2). The two files declare their own independent copies of the
helpers, called only from within their own types, and carry their own tests in
`UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` and `EmailFiler_Tests.cs`. There is no
reference in either direction between them and `QfcExplorerController`. Deleting the `QuickFiler`
region leaves them untouched and unbroken.

They are the *surviving maintained copies* of the duplicated helpers, exactly as the orchestrator read
it. The spec should say so explicitly and should **not** propose consolidating the three copies — that
is a separate refactor, larger than this issue, and would drag two 500-line-cap violations into a
defect-fix PR.

### Cap-violation attribution guidance for the PR body

`feature-review` raises the 500-line cap against files in the diff. Only `QfcExplorerController.cs`
(323 → ~179), `IQfcExplorerController.cs` (15 → 14), `QuickFiler.Test.csproj` (484 → 485) and the new
test file will be in the diff, and none is over the cap. `SortEmail.cs` and `QuickFileController.cs`
should not appear in the diff at all. State this in the PR body pre-emptively.

---

## 8. Q8 — Regression-test-first sequencing

`CLAUDE.md`'s Bugfix Workflow requires a failing regression test first. Assessment per defect.

### 8.1 Defect 1 — `ExplConvView_Cleanup`

**Under the recommended decision (remove the member): a behavioural fail-before test is structurally
impossible.** There is no observable behaviour to assert, because the member has no callers and,
after the change, does not exist. Two candidate mechanisms and their assessment:

- *Reflection contract test* — `typeof(IQfcExplorerController).GetMethod("ExplConvView_Cleanup").Should().BeNull()`.
  This genuinely fails before and passes after, and the general policy does list "Contract / schema
  tests" as a category. But it asserts the *absence* of a member, which permanently blocks a future
  restoration and encodes no behaviour. Listed as optional test 11 in §5.5; **not recommended**.
- *Compiler as the gate* — removing a member from an interface with one implementer is enforced by the
  build itself, and the absence of callers is provable by grep. **Recommended.**

Record a **`fail-before-exception` dossier** at
`<FEATURE>/evidence/regression-testing/fail-before-exception.<timestamp>.md`, per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Required content:

```
Timestamp: <ISO-8601>
Command: git grep -n "ExplConvView_Cleanup" -- "*.cs"
EXIT_CODE: 0

WhyFailingRunImpossible: The remedy removes a member that no compiled production or test code
calls, so there is no observable behaviour whose change a test could detect. A test asserting the
member's absence would assert the non-existence of an API rather than a behaviour, and would
permanently block restoration.

Absence-of-caller proof:
  SearchScope: entire repository, *.cs
  SearchPatterns: ExplConvView_Cleanup
  SearchResult:
    QuickFiler/Interfaces/IQfcExplorerController.cs:12   (declaration, removed by this change)
    QuickFiler/Controllers/QfcExplorerController.cs:61   (implementation, removed by this change)
    QuickFiler/Legacy/QuickFileController.cs:673, :851   (NOT COMPILED — no <Compile Include="Legacy\...">
                                                          entry exists in QuickFiler/QuickFiler.csproj)
    QuickFiler/Notes/notes_interfaces.cs:58              (NOT COMPILED — same proof)
  Mock-setup proof: no file under QuickFiler.Test/ references ExplConvView_Cleanup.
  Compiler proof: the interface has exactly one implementer, so the build enforces the paired edit.
```

**If the decision is overridden to "implement":** a fail-before test *is* constructible and should be
written — `System.Action act = () => controller.ExplConvView_Cleanup(); act.Should().NotThrow<NotImplementedException>();`
fails today (line 63 throws) and passes after. Follow it with behavioural assertions on
`View.Apply()` and `View.Delete()` via `VerifyAll`.

### 8.2 Defect 2 — `OpenQFItem` re-resolves the explorer

**A genuinely failing-before test IS constructible.** This is the strongest of the three and should
carry the plan's `[expect-fail]` task.

Mechanism — make the two explorers distinguishable by sequencing `ActiveExplorer()`:

```csharp
olApp.SetupSequence(a => a.ActiveExplorer())
     .Returns(capturedExplorer.Object)   // consumed by the constructor, line 35
     .Returns(driftedExplorer.Object);   // what line 140 would resolve today
```

Arrange so the guard at lines 135-137 is entered: `capturedExplorer.CurrentFolder` returns a folder
whose `FolderPath` is `@"\\Mailbox\A"`, and `mailItem.Parent` returns a folder whose `FolderPath` is
`@"\\Mailbox\B"`. Set `IsItemSelectableInView` to `true` so the `MessageBox` branch is never reached,
and construct with `QfEnums.InitTypeEnum.Find` so neither `HasFlag(Sort)` conjunct is true.

Assert:
```csharp
capturedExplorer.VerifySet(e => e.CurrentFolder = destination.Object, Times.Once());
driftedExplorer.VerifySet(e => e.CurrentFolder = It.IsAny<MAPIFolder>(), Times.Never());
```

Before the fix, line 140 assigns `driftedExplorer.CurrentFolder`, so both assertions fail. After the
fix, both pass. Use `MockBehavior.Loose` for `driftedExplorer` so the pre-fix failure surfaces as a
clean FluentAssertions message rather than a Moq strict-mode exception.

### 8.3 Defect 3 — dead-code deletion

**A fail-before test is NOT constructible.** The block is unreachable from every compiled entry point
(§3), so no input to any public or internal API can cause any of its 139 lines to execute. There is no
observable behaviour that differs before and after. A reflection assertion that
`typeof(QfcExplorerController).GetMethod("StripTabsCrLf", BindingFlags.NonPublic | BindingFlags.Static)`
is `null` would fail before and pass after, but it asserts the absence of a private implementation
detail and is brittle; it is not recommended.

Record a **`fail-before-exception` dossier** with:

```
Timestamp: <ISO-8601>
Command: git grep -n -E "SanitizeArrayLineTSV|StripTabsCrLf|WriteCSV_StartNewFileIfDoesNotExist|SanitizeArray|SaveMessageAsMSG|GetCurrentExplorerFolder" -- "*.cs"
EXIT_CODE: 0

WhyFailingRunImpossible: The change deletes six private/internal statics that no compiled entry
point can reach, so no test input can execute any of the deleted lines. There is no observable
behaviour to assert before or after.

Absence-of-reference proof: every reference to the six identifiers outside lines 183-321 of
QuickFiler/Controllers/QfcExplorerController.cs binds to an independent copy in
UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs,
UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs, or
ToDoModel/Email Utilities/SortItemsToExistingFolder.cs. No file under QuickFiler.Test references
any of the six.

Alternative proof of no behaviour change: the full nine-assembly suite passes identically before
and after. Record both runs under <FEATURE>/evidence/qa-gates/.
```

The acceptance criterion for defect 3 asks for "a test run confirming no behavior change," which is
satisfied by the before/after suite comparison rather than by a new test.

### 8.4 Recommended plan sequencing

1. Phase 2 (`[expect-fail]`): write **only** the defect-2 test (§8.2) plus the non-fail-before
   characterisation tests from §5.5 that already pass. Run; confirm the defect-2 test fails and the
   others pass.
2. Phase 3a: apply the one-line defect-2 fix (line 140). Re-run; defect-2 test passes.
3. Phase 3b: remove `ExplConvView_Cleanup` from the interface and the class. Build is the gate.
4. Phase 3c: delete lines 183-321 and the orphaned `using` directives. Build is the gate.
5. Phase 3d: narrow the `[ExcludeFromCodeCoverage]` attribute (§6.4).
6. Phase 4: full toolchain, in order, and both fail-before-exception dossiers.

Order matters: the defect-2 test must be written and observed failing **before** any deletion, because
deleting the region and the orphaned usings changes the file's line numbering and would make the
pre-change observation harder to reconstruct.

---

## 9. Toolchain and environment notes carried from the epic

Restated here so the plan does not re-derive them. These are the epic's Hard Constraints, verified
against this worktree where verifiable.

1. **`vstest` requires `/InIsolation`.** Use
   `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`.
   Without it, binding redirects in each assembly's `app.config` are ignored and roughly 1,695 phantom
   failures appear.
2. **Exclude `\.claude\` from recursive `*.Test.dll` discovery.** Stale agent worktrees exist under
   `.claude/worktrees/`, including this one; a CI-style recursive search would load stale assemblies.
3. **Do not edit anything under `.claude/**`.** Push-down-owned; local edits are destroyed by sync.
4. **No Python toolchain exists.** There is no `scripts/dev_tools/` and no Poetry manifest (verified).
   Any skill step naming `poetry run python -m scripts.dev_tools.*` is unrunnable by absence — report
   it as such; do not fabricate a result. PowerShell equivalents are under `.claude/lib/`.
5. **`quality-tiers.yml` does not exist** at the repository root (verified). No QuickFiler tier
   classification is available to cite.
6. **Evidence paths are non-overridable:** `<FEATURE>/evidence/<kind>/` only.
7. **Analyzer gate must use `/t:Rebuild`,** not `/t:Build`; the nullable gate must **not** add
   `/p:Nullable=enable`. Both per `CLAUDE.md` C#1.2 and C#1.3.

---

## 10. Decision summary for the spec

| Question | Decision |
| --- | --- |
| **Q1** | **Remove** `ExplConvView_Cleanup()` from `IQfcExplorerController.cs:12` and `QfcExplorerController.cs:60-64`. Zero callers, zero mock setups, one implementer. Preserve the legacy body verbatim in `spec.md` for future restoration. Fallback implementation retained at §1.5 if overridden. |
| **Q2** | Replace `_globals.Ol.App.ActiveExplorer().CurrentFolder = ...` at line 140 with `_activeExplorer.CurrentFolder = ...`. Line 140 is the only re-resolution in the file. No behavioural dependency on the fresh call exists; no in-code justification is needed. |
| **Q3** | Delete lines 183-321 unconditionally. All six statics are referenced only within that region; every external reference binds to `SortEmail`, `EmailFiler`, or `SortItemsToExistingFolder`. **No `QuickFiler.Test` file references any of the six.** No test edit required. |
| **Q4** | Eight directives become orphaned: lines 2, 3, 5, 6, 8 (by deletion), 1 (by the Q1 removal), plus lines 7, 13, 15 which are **already** orphaned today. Retained: 4 (attribute), 9, 10, 11, 12, 14, 16. **Unused usings are not enforced by either gate here** — `IDE0005`'s analyzer is not wired into this non-SDK project, no `IDE0005` severity is configured, and `CS8019` is hidden. Removal is hygiene; do it, and label it as such. |
| **Q5** | `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs`, class `QfcExplorerControllerTests`, namespace `QuickFiler.Controllers.Tests`. Append one `<Compile Include>` after `QuickFiler.Test.csproj:158`. Mock chain per §5.2, modelled on `QfcHomeControllerTests.cs:39-47`. **Every relevant COM member is mockable** with an in-repo precedent. `OpenQFItem` is IN scope; only its `MessageBox.Show` else branch (lines 166-178) is untestable and must stay uncovered. |
| **Q6** | The class-level `[ExcludeFromCodeCoverage]` means the deletion is **coverage-neutral** — the potential document's denominator claim is false. `CLAUDE.md` clause (c) does **not** apply (not an event handler; has an injectable seam). **Recommendation: NARROW** — remove the class-level attribute, apply it to `OpenQFItem` only. This satisfies the epic NFR positively. `KEEP` is the acceptable fallback; `REMOVE` is not recommended. |
| **Q7** | Over-cap and **untouched**: `SortEmail.cs` (1,429), `QuickFileController.cs` (1,065) — both pre-existing, neither in the diff. In the diff and all under cap: `QfcExplorerController.cs` 323 → ~179, `IQfcExplorerController.cs` 15 → 14, `QuickFiler.Test.csproj` 484 → 485. **`EmailFiler.cs` and `SortEmail.cs` need NO edit** — confirmed. |
| **Q8** | Defect 2: genuine fail-before test via `SetupSequence` on `ActiveExplorer()` (§8.2). Defects 1 and 3: fail-before structurally impossible; record `fail-before-exception.<timestamp>.md` dossiers under `<FEATURE>/evidence/regression-testing/` with the absence-of-reference proofs given in §8.1 and §8.3. |

---

## 11. Open items the spec must resolve, not inherit

1. The feature-folder name disagreement between `epic.md:33` and the on-disk folder (§0.3.1).
2. The Q6 attribute decision is a judgment call with a real `feature-review` risk in either direction.
   Whichever is chosen, write the reasoning from §6.2 and §6.3 into the PR body so the reviewer sees
   that the exclusion's policy grounding was examined rather than ignored.
3. The `Views` indexer parameter type (§5.3) is the single unverified compile-level detail in the test
   design. It is a one-token adjustment if wrong and cannot invalidate the harness.
4. Whether the catch-asymmetry reading in §1.2(b) should be recorded as a latent defect. Under the
   recommended Q1 removal the code disappears and the question is moot; the epic forbids writing a new
   potential document, so if it is judged worth tracking it must go through the issue-promotion path
   after this child merges, not into `docs/features/potential/**`.
