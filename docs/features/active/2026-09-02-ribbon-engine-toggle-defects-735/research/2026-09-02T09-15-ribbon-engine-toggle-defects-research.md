# Research — ribbon-engine-toggle-defects (Issue #735)

- Date: 2026-09-02
- Work mode: full-bug
- Feature folder: `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/`
- Scope: three consolidated defects in `TaskMaster/Ribbon/`
- Verification basis: fresh reads of the worktree at
  `<repo-root>/.claude/worktrees/agent-a3324f355df219b0e` (branch `TaskMaster-wt-2026-09-02T08-47`,
  HEAD `5ebaaf10`). Every file/line citation below was re-read in this session. No `git` or shell
  tool was available in this session, so no claim in this document rests on commit history.

---

## 0. Summary of decisions

| # | Defect | Decision |
|---|---|---|
| 1 | Dead XML→handler bindings | Rename the four `_Clicked` `onAction` values to `_Click` in the XML; **delete** the `BtnMigrateIDs` button element. Add one exhaustive name-resolution regression test plus one `checkBox` arity test. |
| 2 | Unguarded `Globals` deref in `ClearSpamManagerAsync` | New host-neutral, fully-tested `SpamManagerResetGate` in `TaskMaster/Ribbon/`, taking `Func<IAppAutoFileObjects>` + `Func<IAppItemEngines>` + `Action<string>`; `ClearSpamManagerAsync` defers its engine-touching body into a lambda passed to `RunAsync`. |
| 3 | Toggle-state last-writer race | Per-write monotonic sequence ticket captured immediately before `EngineActiveAsync`, plus a compare-and-apply cache write. `_pressedState` becomes `ConcurrentDictionary<string, PressedState>`. CR-2 (canceled prime) and CR-3 (untested guard) are **in scope**. |

---

## 1. Current state analysis

### 1.1 Ribbon subsystem inventory (verified)

`TaskMaster/Ribbon/` contains 14 files:

- Host-neutral, **not** `[ExcludeFromCodeCoverage]`, unit-tested: `EngineCommandCatalog.cs`,
  `EngineCommandRefreshPlanner.cs`, `EngineGatedCommandRunner.cs`, `EngineReadinessGate.cs`,
  `EngineToggleCatalog.cs`, `EngineToggleStateCoordinator.cs`.
- COM/VSTO shims carrying a **type-level** `[ExcludeFromCodeCoverage]`:
  `RibbonController.cs` (attribute at line 36), its partials
  `RibbonController.EngineCommands.cs`, `RibbonController.FolderTree.cs`,
  `RibbonController.Intelligence.cs`; and `RibbonViewer.cs` (attribute at line 32) with its
  partial `RibbonViewer.EngineCommands.cs`.
- `RibbonExplorer.xml` (the embedded Explorer CustomUI document), `TryFunctionalityInConstruction.cs`.

The "one attribute covers every partial file" convention is documented in-repo at
`TaskMaster/Ribbon/RibbonController.EngineCommands.cs:12-17` and
`TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs:11-15`. **Confirmed** — the background statement
is accurate.

### 1.2 Established seam pattern

`EngineReadinessGate` (`TaskMaster/Ribbon/EngineReadinessGate.cs`) is the canonical shape:
`internal sealed`, one `Func<IAppItemEngines>` accessor validated in the constructor, no COM, no
`Microsoft.Office.*`, no `MessageBox`, no logger field, and an explicit XML-doc paragraph
(lines 25-28) recording that it is deliberately **not** `[ExcludeFromCodeCoverage]`.
`EngineGatedCommandRunner` (same directory) adds the deferred-invocation shape: `RunAsync(id,
Func<Task> action)` which notifies through an injected `Action<string>` and returns
`Task.CompletedTask` when the gate is closed, never catching. Both are wired from
`RibbonController.EngineCommands.cs:39-77` via `??=` lazily-built private properties whose
accessors are `() => Globals?.Engines!`.

### 1.3 Toolchain constraints that shape the design

1. **Legacy non-SDK projects.** `TaskMaster/TaskMaster.csproj` and
   `TaskMaster.Test/TaskMaster.Test.csproj` both use the 2003 MSBuild XML namespace and list every
   source file explicitly (`TaskMaster.csproj:458-470`, `TaskMaster.Test.csproj:314-324`). **Every
   new `.cs` file requires a `<Compile Include=...>` entry.** See the scope note in section 7.
2. **500-line file ceiling** (`.claude/rules/general-code-change.md`) applies to test code too.
   Current sizes: `EngineToggleStateCoordinator.cs` 389, `RibbonController.Intelligence.cs` 412,
   `RibbonExplorerXmlTests.cs` 323, `EngineToggleStateCoordinatorTests.cs` 459,
   `RibbonViewerEngineCallbackShapeTests.cs` 365. The coordinator test file at 459 lines is the
   binding constraint and drives the partial-class decision in section 5.3.
3. **CSharpier formats the ribbon XML.** `.csharpierignore` excludes only `**/evidence/**`,
   coverage/TRX artifacts, and `*.csproj|*.props|*.targets`. `RibbonExplorer.xml` is **not**
   excluded, which is why its elements are attribute-per-line wrapped. Any XML edit must be
   followed by `dotnet tool run csharpier format .` and the reflowed result accepted.
4. **No Office PIA reference in `TaskMaster.Test.csproj`.** Reflection tests must compare parameter
   types by `Type.FullName` against the literal `"Microsoft.Office.Core.IRibbonControl"`. Precedent:
   `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs:285-318` and
   `TaskMaster.Test/Ribbon/RibbonViewerEngineCallbackShapeTests.cs:26-32,43`.

---

## 2. Finding 1 — dead XML-to-handler bindings

### 2.1 Verified current state

`TaskMaster/Ribbon/RibbonExplorer.xml`:

- Line 82: `<button id="BtnMigrateIDs" onAction="BtnMigrateIDs_Click" label="MigrateToDoIDs" />`
- Line 268: `onAction="MoveEntireConversation_Clicked"` (control `MoveEntireConversationDefault`, line 265)
- Line 274: `onAction="SaveAttachments_Clicked"` (control `SaveAttachmentsDefault`, line 271)
- Line 280: `onAction="SaveEmailCopy_Clicked"` (control `SaveEmailCopyDefault`, line 277)
- Line 286: `onAction="SavePictures_Clicked"` (control `SavePicturesDefault`, line 283)

All four are `<checkBox>` elements inside `<menu id="ItemSortSettings">` (lines 258-288), and all
four already declare a correctly-resolving `getPressed`.

`TaskMaster/Ribbon/RibbonViewer.cs` defines the intended handlers with the `_Click` spelling and the
correct Office `checkBox` `onAction` signature `void (Office.IRibbonControl, bool)`:
`MoveEntireConversation_Click` (line 180), `SaveAttachments_Click` (186), `SaveEmailCopy_Click`
(192), `SavePictures_Click` (198). The same shape is used by the two working checkboxes
`ToggleDarkMode_Click` (`RibbonViewer.cs:170`) and `SpamBayesEnabled_Click` /
`TriageEnabled_Click` (`RibbonViewer.EngineCommands.cs:169,279`).

No method named `BtnMigrateIDs_Click` exists in the assembly. A case-insensitive repo-wide grep for
`MigrateIDs|MigrateToDoIDs|MigrateToDoId|MigrateID` returns only `RibbonExplorer.xml:82` plus
documentation files; a `Migrate` grep restricted to `*.cs` returns exactly one hit, an unrelated
`//TODO:` comment at `UtilitiesCS/EmailIntelligence/OlFolderTools/OlFolderHelper/SmithWaterman.cs:49`.

**Disagreement with the background:** none on the five names or their line numbers. The background
described the precedent test as being "around line 293"; it is
`RibbonExplorerXml_GetEnabledCallbackMatchesOfficeSignatureOnRibbonViewer` declared at
`TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs:294`.

### 2.2 Decision — direction of the four renames

**Edit the XML, not the code.** Rationale:

- `_Click` is the repository-wide convention: all 93 callback methods on `RibbonViewer` use it, and
  the three already-working `checkBox` `onAction` bindings (`ToggleDarkMode_Click`,
  `SpamBayesEnabled_Click`, `TriageEnabled_Click`) prove the XML side is the outlier.
- Adding four `_Clicked` methods would introduce four new `public` members on a `ComVisible(true)`
  type that duplicate four existing ones — a public-surface expansion, the opposite of the minimal
  fix.
- The XML change is four attribute values; the code change would be four new methods plus their
  documentation.

An archived plan already records this asymmetry as known
(`docs/features/archive/2026-06-01-quickfiler-high-confidence-filter-169/plan.2026-06-01T12-29.md:71`).

### 2.3 Decision — `BtnMigrateIDs`

**Delete the whole `<button id="BtnMigrateIDs" .../>` element at line 82.** Evidence for removal
rather than implementation:

- No implementation exists anywhere in the solution (section 2.1 greps).
- No design document, plan, spec, or potential-feature entry proposes a "MigrateToDoIDs" behavior.
  The only mentions are the defect records themselves (#504, #505, #735) and this feature folder.
- The predecessor investigation left the choice open
  (`docs/features/potential/promoted/2026-08-08-ribbon-dead-callback-names.md:76`) rather than
  asserting implementation was pending.
- Implementing an unspecified data-migration command is a feature, not a bugfix, and would exceed
  this issue's declared scope.

**Verification limitation to record:** no `git` tool was available in this session, so I did not
inspect the commit that introduced line 82. If the atomic plan wants stronger evidence,
`git log -S "BtnMigrateIDs" -- TaskMaster/Ribbon/RibbonExplorer.xml` is the check to run; nothing in
the working tree contradicts removal.

### 2.4 Decision — scope of the regression test

Recommend the **exhaustive name-resolution test**, not five hard-coded assertions, plus one
narrowly-scoped arity test. Justification within the minimal-fix policy: the enumeration is not a
refactor of production code — it is a single test method whose input set is derived from the
document rather than hand-listed, so it costs about the same as five literal assertions while being
immune to the exact drift that produced the defect. #504 recommended it
(`docs/features/potential/promoted/2026-08-08-ribbon-dead-callback-names.md`), and the file already
hosts an analogous XML↔`RibbonViewer` reflection pin.

Two test methods, both added to `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`:

1. `RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod`
   - Collect callback attribute values with this rule: an attribute is a callback iff its local
     name is `onAction`, `onChange`, or `onLoad`, **or** begins with `get`. This rule is exact for
     the Office 2009 CustomUI schema (every `get*` attribute is a callback) and future-proof against
     a newly introduced `getVisible`/`getImage`.
   - Enumerate over `document.Descendants()` — element nodes only. XML comments are `XComment`
     nodes and carry no attributes, so the eight commented-out `onAction` occurrences (lines
     361, 362, 369, 370, 371, 378, 397, 398) are excluded automatically, with no regex needed.
   - Include the root `customUI/@onLoad` (line 2, `Ribbon_Load`); `Descendants()` from the
     `XDocument` includes the root element, so no special case is required.
   - Assert each distinct value matches some
     `typeof(RibbonViewer).GetMethods(BindingFlags.Public | BindingFlags.Instance)` name.
     Fail with the full list of unresolved names so one run reports all five.
   - **Name-only**: no Office type is referenced, so this test needs no PIA workaround.
2. `RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters`
   - For every `<checkBox>` element's `onAction`, resolve the method and assert
     `void (Microsoft.Office.Core.IRibbonControl, bool)`, comparing the first parameter by
     `Type.FullName`. This pins the exact shape whose silent mis-binding created the defect. Adds
     one private `const string RibbonControlTypeName` to the fixture (the literal already appears
     inline at line 316 of that file and may be hoisted).

Both tests fail on the current tree (test 1 on five names; test 2 on four unresolvable names) and
pass after the XML edits. Size impact: `RibbonExplorerXmlTests.cs` 323 → roughly 410 lines.

**Rejected placement.** `RibbonViewerEngineCallbackShapeTests.cs` already owns
`AssertCheckBoxOnActionParameters` (line 263) and `GetPublicInstanceMethod` (line 339), so test 2
could reuse them with zero duplication. Rejected because that fixture's documented charter is the
#505/#506/#518 *engine* toggle callbacks (its class doc, lines 13-37), and widening it would mix
concerns; keeping both new tests in `RibbonExplorerXmlTests.cs` (whose charter is XML↔code
consistency) costs one duplicated string constant and keeps the write set one file smaller.

### 2.5 Interaction with Finding 2 (must be recorded, not fixed here)

Repairing the four `onAction` names makes four previously-dead callbacks live. They route to
`RibbonController.Intelligence.cs:31-51` (`ToggleMoveEntireConversation`, `ToggleSaveAttachments`,
`ToggleSaveEmailCopy`, `ToggleSavePictures`), which dereference `Globals.InternalQfSettings` with no
guard — the same defect class as Finding 2, and explicitly listed in #524's site table
(`docs/features/potential/promoted/2026-08-08-ribbon-controller-intelligence-unguarded-globals-deref.md:47`).

This does **not** open a new crash window: the sibling `getPressed` callbacks for the same four
controls (`RibbonViewer.cs:177,183,189,195` → `IsMoveEntireConversationActive()` etc. at
`RibbonController.Intelligence.cs:29,36,43,48`) already dereference `Globals.QfSettings` unguarded
and already fire when the menu is opened in the pre-`SetGlobals` window. The fix adds a second entry
point into an already-reachable surface, not a new one.

Recommendation: keep it out of this issue's scope (issue #735 finding 2 is scoped to
`ClearSpamManagerAsync`) and promote the eight QuickFiler-settings sites as a follow-up issue
referencing #524. Section 8 records this.

---

## 3. Finding 2 — unguarded `Globals` dereference in `ClearSpamManagerAsync`

### 3.1 Verified current state

`TaskMaster/Ribbon/RibbonController.Intelligence.cs`, `ClearSpamManagerAsync` occupies
**lines 206-233** (the background said "starts around line 206, body runs to ~231"; the closing
brace is at 233, and the issue text's "216-231" is off by ten at the start). Body:

```
206  internal async Task ClearSpamManagerAsync()
...
212      var response = MessageBox.Show( ... "Clear Spam Manager", MessageBoxButtons.YesNo);
217      if (response == DialogResult.Yes)
219          if ((await Globals.AF.Manager.Configuration).TryGetValue(SpamBayes.GroupName, out var loader))
226              var classifier = await SpamBayes.CreateSpamClassifiersAsync();
227              classifier.Config.CopyFrom(loader.Config, true);
228              classifier.Serialize();
229              Globals.AF.Manager[SpamBayes.GroupName] = classifier.ToAsyncLazy();
230              await Globals.Engines.RestartEngineAsync(SpamBayes.GroupName);
```

Three unguarded links, each independently null in a real window:

| Link | Declared type | Why it can be null |
|---|---|---|
| `Globals` | `ApplicationGlobals` (`RibbonController.cs:40`, `protected internal ... { get; set; }`) | Assigned only by `SetGlobals` (`RibbonController.cs:51`); the ribbon is constructed earlier. |
| `Globals.AF` | `IAppAutoFileObjects` (`ApplicationGlobals.cs:449`, `=> _autoFileObjects`) | `_autoFileObjects` is null until the basic load runs — proven by `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:64-73`, which asserts the sibling backing fields are null before `ForceBasicLoad`. |
| `Globals.AF.Manager` | `ManagerAsyncLazy` (`TaskMaster/AppGlobals/AppAutoFileObjects.cs:615`, `{ get; internal set; }`) | Auto-property with no initializer; assigned only inside `LoadParallelAsync` (line 68) / `LoadSequentialAsync` (line 86). |
| `Globals.Engines` | `IAppItemEngines` (`ApplicationGlobals.cs:466`, `{ get; private set; }`) | Same pre-load window; `RibbonController.Engines` at `RibbonController.Intelligence.cs:204` already concedes this with `Globals?.Engines`. |

`ClearSpam` is **not** a member of `EngineCommandCatalog`: `RibbonExplorer.xml:152` declares
`onAction="ClearSpam_Click"` with no `getEnabled`, unlike its fourteen catalog siblings. That is
correct — `InboxEngines` readiness is the wrong predicate for a command whose real dependency is
`AF.Manager` — so routing this through `EngineGatedCommandRunner` is not an option.

### 3.2 Testability of `ApplicationGlobals` (checked as instructed)

`ApplicationGlobals` **is** constructible in tests
(`TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:51-56` uses the real three-argument
constructor with an Outlook stub), but every existing test that reaches into it does so through
`BindingFlags.NonPublic` reflection on private fields and private-setter properties (see also
`TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs:52-64`). Taking a real `ApplicationGlobals`
into the new guard would therefore drag a heavyweight, reflection-driven fixture into a
decision-logic unit test.

**Conclusion: the guard must take narrow accessors, not `ApplicationGlobals`.** Both dependency
types are interfaces (`IAppAutoFileObjects`, `IAppItemEngines`) and mock cleanly with Moq. The one
concrete type on the boundary, `ManagerAsyncLazy`, is proven test-constructible by existing repo
code: `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage_Tests.ManagerAndAdditional.cs:23-25`
does exactly `new ManagerAsyncLazy(mockGlobals.Object)` and then
`mockAf.Setup(a => a.Manager).Returns(manager)`. Its constructor is cheap: it calls
`ResetConfigAsyncLazy()`, which is `Configuration = new(ReadConfiguration)`
(`UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs:94`) — an `AsyncLazy`
assignment that does not execute the factory. No disk, no COM.

### 3.3 Selected design — `SpamManagerResetGate`

**New file:** `TaskMaster/Ribbon/SpamManagerResetGate.cs`
**Namespace:** `TaskMaster` (matching every sibling in the directory)
**Declaration:** `internal sealed class SpamManagerResetGate`
**Attributes:** none. It must carry an XML-doc paragraph, mirroring `EngineReadinessGate.cs:25-28`,
stating that it is deliberately not `[ExcludeFromCodeCoverage]`.
**Usings:** `System`, `System.Globalization`, `System.Threading.Tasks`, `UtilitiesCS`. No
`Microsoft.Office.*`, no `System.Windows.Forms`, no logger field.

Fields:

```
private readonly Func<IAppAutoFileObjects> _autoFileAccessor;
private readonly Func<IAppItemEngines> _enginesAccessor;
private readonly Action<string> _notifyNotReady;
```

Constructor:

```
internal SpamManagerResetGate(
    Func<IAppAutoFileObjects> autoFileAccessor,
    Func<IAppItemEngines> enginesAccessor,
    Action<string> notifyNotReady)
```

Throws `ArgumentNullException` naming the offending parameter for each of the three, using the
`?? throw new ArgumentNullException(nameof(x))` form used at `EngineReadinessGate.cs:47-48` and
`EngineGatedCommandRunner.cs:62-64`.

Public surface — exactly one method plus one message builder:

```
internal Task RunAsync(Func<ManagerAsyncLazy, IAppItemEngines, Task> reset)
```

Behavior contract:

1. `reset is null` → throw `ArgumentNullException(nameof(reset))` **before** any accessor is
   invoked (matches `EngineGatedCommandRunner.RunAsync`, lines 99-102).
2. Evaluate `var autoFile = _autoFileAccessor();`, `var manager = autoFile?.Manager;`,
   `var engines = _enginesAccessor();`.
3. If `manager is null || engines is null` → invoke `_notifyNotReady(BuildNotReadyMessage())`
   exactly once and return `Task.CompletedTask`. `reset` is never invoked.
4. Otherwise return `reset(manager, engines)` — no `await`, no `try`/`catch`. A fault from the
   deferred work propagates unchanged, matching the "suppresses invocation, never errors"
   invariant documented at `EngineGatedCommandRunner.cs:21-25`.

```
private static string BuildNotReadyMessage()
```
Returns a `CultureInfo.CurrentCulture`-formatted constant along the lines of
"The Spam Manager cannot be cleared yet because the classifier manager is still loading. Please try
again once initialization completes." (Exact wording is the planner's to fix; it must not name a
control id, because unlike `EngineGatedCommandRunner` this gate serves one command.)

`ManagerAsyncLazy` lives in namespace `UtilitiesCS`
(`UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs:26,28`), which
`RibbonController.Intelligence.cs` already imports at line 12, so no new project reference is
needed on either side. `TaskMaster.Test` already has a `ProjectReference` to `UtilitiesCS`
(`TaskMaster.Test.csproj:343-345`).

### 3.4 Exact change to `ClearSpamManagerAsync`

Add, inside the existing `#region Spam Manager` in `RibbonController.Intelligence.cs` immediately
above `ClearSpamManagerAsync`:

```
private SpamManagerResetGate _spamManagerResetGate;

private SpamManagerResetGate SpamManagerReset =>
    _spamManagerResetGate ??= new SpamManagerResetGate(
        () => Globals?.AF!,
        () => Globals?.Engines!,
        NotifyEngineCommandNotReady);
```

`NotifyEngineCommandNotReady` is `private` on the same partial class
(`RibbonController.EngineCommands.cs:158`) and is therefore accessible. The `!` null-forgiving
operators match the precedent at `RibbonController.EngineCommands.cs:44,73` and must carry the same
explanatory comment: a null result is a supported input the gate treats as "not ready".

Rewrite lines 217-232 so the engine-touching work becomes the deferred lambda:

```
if (response != DialogResult.Yes)
{
    return;
}

await SpamManagerReset.RunAsync(async (manager, engines) =>
{
    if ((await manager.Configuration).TryGetValue(SpamBayes.GroupName, out var loader))
    {
        var classifier = await SpamBayes.CreateSpamClassifiersAsync();
        classifier.Config.CopyFrom(loader.Config, true);
        classifier.Serialize();
        manager[SpamBayes.GroupName] = classifier.ToAsyncLazy();
        await engines.RestartEngineAsync(SpamBayes.GroupName);
    }
});
```

No other line of the method changes. The `SynchronizationContext` preamble (lines 208-211) and the
confirmation `MessageBox` (212-216) stay exactly where they are.

**Deliberately not changed:** the confirmation dialog still runs *before* the gate. Showing the gate
message first would be marginally better UX, but it reorders user-visible behavior on the
already-working path for no defect-driven reason. Recorded as a rejected alternative.

Size impact: `RibbonController.Intelligence.cs` 412 → roughly 440 lines, under the 500 ceiling.

### 3.5 Rejected alternatives for Finding 2

- **Ad-hoc `?.` / `is null` guards inline.** Explicitly disrecommended by the maintainer on #518 and
  restated in #524's "Suggested approach"
  (`.../2026-08-08-ribbon-controller-intelligence-unguarded-globals-deref.md:75`). The guard would
  also sit inside the type-level `[ExcludeFromCodeCoverage]` region and be permanently untestable.
- **A `TryGetResetTargets(out ManagerAsyncLazy, out IAppItemEngines)` predicate instead of
  `RunAsync`.** Equivalent decision content, but the notification then has to be issued by the
  exempt call site, moving the "when do we notify" decision back into uncovered code. The
  deferred-lambda form keeps decision + message inside the covered class, matching
  `EngineGatedCommandRunner` exactly.
- **Extending `EngineCommandCatalog` / `EngineGatedCommandRunner` to cover `ClearSpam`.** Wrong
  predicate (`InboxEngines` readiness, not `AF.Manager` availability) and it would break the
  set-equality assertion `RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls`
  (`RibbonExplorerXmlTests.cs:229`) unless the XML also gained a `getEnabled`.

---

## 4. Finding 3 — toggle-state last-writer race

### 4.1 Verified current state

`TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (389 lines). The two unconditional writers:

- `ExecuteToggleAsync` (declared line 207): `await ToggleEngineAsync` (223) → `await
  EngineActiveAsync` (224) → `_pressedState[engineName] = active;` (**226**) → `_invalidateControl(controlId);` (227).
- `ApplyPrimeAsync` (declared line 303): `await EngineActiveAsync` (309) →
  `_pressedState[engineName] = active;` (**310**) → `_invalidateControl(controlId);` (311).

Cache: `private readonly ConcurrentDictionary<string, bool> _pressedState` (lines 68-69).
Reader: `GetPressed` (135), `_pressedState.TryGetValue(engineName, out var cached); return cached;`
(142-145). `System.Threading` is already imported (line 4), so `Interlocked` needs no new using.

The prime marker `_primeTasks` (76-79) is never cleared on success, so a stale prime write persists
for the session — exactly as #525 describes. All background claims for this finding verified
without disagreement.

### 4.2 Selected mechanism — monotonic sequence ticket + compare-and-apply

The freshness of a cached value is determined by **when its underlying `EngineActiveAsync`
observation began**, not by when the write lands. So: every writer takes a globally monotonic ticket
immediately before invoking `EngineActiveAsync`, and a write is applied only if its ticket is
strictly greater than the ticket already stored for that key.

A single process-wide counter is sufficient even though the cache is per-key, because tickets are
only ever compared within a key.

New members on `EngineToggleStateCoordinator`:

```
/// Monotonic ticket source. Read and written only through Interlocked.
private long _stateSequence;

/// One cached observation: the value plus the ticket of the read that produced it.
/// A reference type so ConcurrentDictionary.TryUpdate compares by reference identity,
/// which is exactly the compare-and-swap semantics TryApplyState needs.
private sealed class PressedState
{
    internal PressedState(bool active, long sequence)
    {
        Active = active;
        Sequence = sequence;
    }

    internal bool Active { get; }

    internal long Sequence { get; }
}
```

Changed field (line 68-69):

```
private readonly ConcurrentDictionary<string, PressedState> _pressedState =
    new ConcurrentDictionary<string, PressedState>(StringComparer.Ordinal);
```

New private helpers:

```
private long NextSequence() => Interlocked.Increment(ref _stateSequence);

/// Stores an observation only when no newer observation is already cached for the key.
/// Returns true when the write was applied, so the caller can invalidate only on a real change.
private bool TryApplyState(string engineName, bool active, long sequence)
{
    while (true)
    {
        if (!_pressedState.TryGetValue(engineName, out var existing))
        {
            if (_pressedState.TryAdd(engineName, new PressedState(active, sequence)))
            {
                return true;
            }

            continue;
        }

        if (existing.Sequence >= sequence)
        {
            return false;
        }

        if (_pressedState.TryUpdate(engineName, new PressedState(active, sequence), existing))
        {
            return true;
        }
    }
}
```

An explicit CAS loop is used rather than `AddOrUpdate`, because `AddOrUpdate`'s update factory may
run more than once under contention and reporting "did my write land?" out of a closure variable is
non-obvious to a reader. The loop terminates: each iteration either returns or observes a strictly
newer stored ticket.

Call-site changes:

- `GetPressed` line 142-145 becomes `if (_pressedState.TryGetValue(engineName, out var cached)) { return cached.Active; }`.
  **`GetPressed` keeps its `bool` return type and still never awaits, blocks, or throws.**
- `ExecuteToggleAsync` lines 223-227 become:
  ```
  await engines.ToggleEngineAsync(engineName).ConfigureAwait(false);
  var sequence = NextSequence();
  var active = await engines.EngineActiveAsync(engineName).ConfigureAwait(false);

  if (TryApplyState(engineName, active, sequence))
  {
      _invalidateControl(controlId);
  }
  ```
  The ticket is captured **after** `ToggleEngineAsync` and **before** `EngineActiveAsync` — that is
  the moment the observation window opens. Update-before-invalidate ordering is preserved, so the
  existing ordering test (`EngineToggleStateCoordinatorTests.cs:250`) still passes.
- `ApplyPrimeAsync` lines 309-311 become:
  ```
  var sequence = NextSequence();
  var active = await engines.EngineActiveAsync(engineName).ConfigureAwait(false);

  if (TryApplyState(engineName, active, sequence))
  {
      _invalidateControl(controlId);
  }
  ```

Conditional invalidation is correct in both writers: if the write was rejected, some other writer
already stored a newer value and already invalidated, so Office's cached `getPressed` answer is
either already correct or already scheduled for re-query. (Unconditional invalidation would also be
harmless; conditional is chosen because it keeps "invalidate iff the displayed value changed" as a
single readable invariant and issues fewer STA-marshalled COM calls.)

Interleaving check against #525's reproduction:

| Step | Ticket | Effect |
|---|---|---|
| menu open → `GetPressed` miss → prime starts, calls `EngineActiveAsync` | 1 | pending |
| user clicks → `ToggleEngineAsync` completes → toggle calls `EngineActiveAsync` | 2 | pending |
| toggle observation resolves `true`, applies | 2 | stored `(true, 2)`, invalidate |
| prime observation resolves with the stale pre-toggle `false` | 1 | `1 < 2` → **rejected**, no write, no invalidate |

Toggle-vs-toggle double click is covered by the same rule: the later `EngineActiveAsync` invocation
holds the higher ticket and wins regardless of completion order.

### 4.3 CR-2 — canceled prime silently ignored. **In scope.**

`CompletePrime` (lines 319-329) reads `completed.Exception`, which is `null` for a canceled task, so
a cancellation returns at line 324: the `_primeTasks` marker stays registered (blocking any
re-prime), the cache stays unset, and nothing is logged. Replacement body:

```
private void CompletePrime(Task completed, string engineName)
{
    if (completed.Status == TaskStatus.RanToCompletion)
    {
        return;
    }

    _primeTasks.TryRemove(engineName, out _);

    var failure =
        (Exception)completed.Exception?.GetBaseException()
        ?? new TaskCanceledException(completed);

    _logError(BuildPrimeFailedMessage(engineName), failure);
}
```

This is a restructure of the same five lines, needs no new message builder (the existing
`BuildPrimeFailedMessage` text at 367-375 reads correctly for a cancellation), and preserves the
existing faulted-path behavior exactly — including `GetBaseException()`, which is what the existing
test `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` asserts with `BeSameAs(failure)`
(`EngineToggleStateCoordinatorTests.cs:233`).

**Scope justification.** CR-2 lives in `CompletePrime`, the direct completion partner of
`ApplyPrimeAsync`, which this fix is already rewriting; it is a five-line restructure in the same
region; #525 — the source issue that #735's finding 3 consolidates — states it is "worth fixing
together" (`.../2026-08-08-engine-toggle-prime-last-writer-race.md:70-72`); and the same
`_primeTasks` re-prime invariant is load-bearing for both defects. Leaving it out would mean
re-opening the file for five lines in a later cycle, which is the exact cost #735 was consolidated
to avoid.

### 4.4 CR-3 — untested `InvalidOperationException` guard. **In scope.**

`ExecuteToggleAsync` lines 218-221 throw `InvalidOperationException(BuildUnavailableMessage(...))`
when the engines accessor returns null. This is **zero production change** — one new test method
that calls `ExecuteToggleAsync` directly on a harness with `EnginesAvailable = false`. Including it
costs nothing and closes the only uncovered lines in a class whose coverage this change already
moves.

### 4.5 Rejected alternatives for Finding 3

- **`_pressedState.TryAdd` in `ApplyPrimeAsync`** (the #525 reviewer's suggestion). Closes
  prime-vs-toggle only. It also breaks a legitimate case: a *second* prime after a failed first
  prime cleared its marker would be refused if a value were already present. Rejected in favour of
  versioning, which the reviewer themselves flagged as the complete answer.
- **A `lock` or `SemaphoreSlim` serializing all writes.** Would have to be held across an `await`
  on `EngineActiveAsync` (a configuration disk load) to actually order the observations, which is
  precisely the STA-blocking hazard the type's own header comment forbids
  (`EngineToggleStateCoordinator.cs:18-27`).
- **`ConcurrentDictionary<string, (bool active, long generation)>` (value tuple) instead of a
  reference-type `PressedState`.** `TryUpdate`'s comparand check would then use
  `EqualityComparer<ValueTuple<bool,long>>.Default` (structural equality), which weakens the CAS
  into "value looked the same" and additionally invites analyzer noise on a public-ish value type.
  A private sealed class gives reference-identity CAS for free.
- **Extracting the versioned cache into its own class** (`EngineTogglePressedStateCache`). Cleaner
  in isolation but adds a production file, a test file, and two `.csproj` entries for roughly 40
  lines of logic that has exactly one consumer. Keep inline. **Contingency:** if
  `EngineToggleStateCoordinator.cs` exceeds 500 lines after `csharpier format`, perform this
  extraction rather than trimming documentation.

Size impact: `EngineToggleStateCoordinator.cs` 389 → roughly 455-465 lines. Tight but within the
ceiling; the planner must verify with a line count after formatting.

---

## 5. Testing implications

### 5.1 Explicit testability statement per finding (repo policy requires this to be stated, not implied)

| Finding | Unit-testable? | Where the assertions live |
|---|---|---|
| 1 | **Yes, fully.** Reflection over the embedded XML resource and `RibbonViewer` type metadata. No `RibbonViewer` instance is constructed and no method is invoked, so the type's `[ExcludeFromCodeCoverage]` is irrelevant and no COM object is touched. | `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` |
| 2 | **Split.** The new `SpamManagerResetGate` is **fully unit-testable** and must not be marked `[ExcludeFromCodeCoverage]`. The ~10 changed lines inside `ClearSpamManagerAsync` are **not unit-testable**: they call `MessageBox.Show`, install a `WindowsFormsSynchronizationContext`, call `SpamBayes.CreateSpamClassifiersAsync()` (disk I/O) and `classifier.Serialize()`, and remain inside `RibbonController`'s type-level `[ExcludeFromCodeCoverage]` (`RibbonController.cs:36`) under the ratified COM/VSTO exemption. **They require a documented manual-verification note** (see 5.5). The exemption is not widened — no new attribute is added anywhere. | `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs` (new) |
| 3 | **Yes, fully.** `EngineToggleStateCoordinator` is host-neutral and stays non-exempt; every interleaving is driven by `TaskCompletionSource<bool>`. | `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` (new) |

No test in this change may sleep, poll, read the wall clock, touch the filesystem, create a
temporary file, or start a message pump — matching the existing fixture's stated discipline
(`EngineToggleStateCoordinatorTests.cs:16-21`).

### 5.2 Finding 1 — recommended test methods

In `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`, new region "Issue #735 — callback name
binding":

- `RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod`
- `RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters`

### 5.3 Finding 3 — recommended test methods

`EngineToggleStateCoordinatorTests.cs` is already 459 lines, so the new tests cannot go in it
without breaching the 500-line ceiling. Use a partial class, which is the established pattern in
this exact directory (`RibbonControllerTests.cs:33` and `RibbonControllerTests.Engines.cs:10` are
both `public partial class RibbonControllerTests`):

1. Change `public class EngineToggleStateCoordinatorTests` to
   `public partial class EngineToggleStateCoordinatorTests`
   (`EngineToggleStateCoordinatorTests.cs:23`) — a one-word edit. The private nested `Harness`
   (line 403) and `LoadedError` (446) then remain reachable from the new file with no duplication.
2. New file `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs`:

| Test method | Scenario |
|---|---|
| `ApplyPrimeAsync_WhenPrimeResolvesAfterToggle_DoesNotOverwriteToggleResult` | The #525 repro. `EngineActiveAsync` returns a held `TaskCompletionSource` on call 1 and `true` on call 2. `GetPressed` starts the prime; `await ExecuteToggleAsync` completes; then release the prime with the stale `false`; `await GetPrimeTask`. Assert `GetPressed == true`, `Invalidations` has exactly one entry, `Errors` empty. **Fails before the fix.** |
| `ExecuteToggleAsync_WhenOlderObservationCompletesLast_DoesNotOverwriteNewerResult` | Toggle-vs-toggle. `ToggleEngineAsync` returns `Task.CompletedTask`; `EngineActiveAsync` dequeues `tcs1` then `tcs2`. Start `t1`, start `t2`, complete `tcs2 = true`, `await t2`, complete `tcs1 = false`, `await t1`. Assert `GetPressed == true` and exactly one invalidation. **Fails before the fix.** |
| `ExecuteToggleAsync_WithNoCompetingWriter_CachesValueAndInvalidatesExactlyOnce` | Guards against over-suppression by the new conditional invalidation. |
| `ExecuteToggleAsync_WithNullEngines_ThrowsInvalidOperationExceptionWithoutTogglingEngine` | CR-3. `EnginesAvailable = false` + direct call. Assert the exception message contains the engine key and `ToggleEngineAsync` was never invoked. |
| `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` | CR-2. `probe.SetCanceled()`; assert exactly one logged error whose message names the engine, and that `GetPrimeTask` no longer holds the original marker (a subsequent `GetPressed` starts a second prime). **Fails before the fix** (currently nothing is logged). |
| `GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked` | Companion assertion that the cache stays unset. May be folded into the previous method if the planner prefers. |

The existing `Harness` needs no modification: `Engines` is a `Mock<IAppItemEngines>` whose setups
each test supplies, `OnInvalidate` already supports observing at invalidation time, and
`EnginesAvailable` already models the pre-`SetGlobals` window.

### 5.4 Finding 2 — recommended test methods

New file `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs`, `[TestClass] public class
SpamManagerResetGateTests`, MSTest + Moq + FluentAssertions:

| Test method | Scenario |
|---|---|
| `Constructor_WithNullAutoFileAccessor_ThrowsArgumentNullException` | `.WithParameterName("autoFileAccessor")` |
| `Constructor_WithNullEnginesAccessor_ThrowsArgumentNullException` | `.WithParameterName("enginesAccessor")` |
| `Constructor_WithNullNotifyDelegate_ThrowsArgumentNullException` | `.WithParameterName("notifyNotReady")` |
| `RunAsync_WithNullReset_ThrowsArgumentNullExceptionBeforeProbingAccessors` | Strict-mock accessors that would fail if invoked. |
| `RunAsync_WhenAutoFileAccessorReturnsNull_NotifiesOnceAndDoesNotInvokeReset` | Pre-`SetGlobals` window. |
| `RunAsync_WhenManagerIsNull_NotifiesOnceAndDoesNotInvokeReset` | Mocked `IAppAutoFileObjects` whose `Manager` is unset (Moq default null). |
| `RunAsync_WhenEnginesAccessorReturnsNull_NotifiesOnceAndDoesNotInvokeReset` | Manager present, engines absent. |
| `RunAsync_WhenAllDependenciesAvailable_InvokesResetWithResolvedManagerAndEngines` | Arrange `new ManagerAsyncLazy(new Mock<IApplicationGlobals>().Object)`; assert `BeSameAs` on both lambda arguments and that no notification was emitted. |
| `RunAsync_WhenResetFaults_PropagatesUnchangedAndDoesNotNotify` | Confirms the "suppresses invocation, never errors" invariant. |

Target coverage for the new class is `>= 90%` per the new-module rule; the above set reaches every
branch.

### 5.5 Manual-verification note required by the change description

Because the `ClearSpamManagerAsync` body stays coverage-exempt, the change description must record
a manual verification step, matching the #524 acceptance shape: launch Outlook with "Show add-in
user interface errors" enabled, click **Clear Spam Manager** before add-in initialization completes,
confirm the "Yes" prompt, and observe the not-ready notice instead of a `NullReferenceException`;
then repeat after initialization and confirm the reset still runs end to end.

---

## 6. Numeric Derivation Evidence

Required before any numeric acceptance criterion is proposed for `spec.md`.

### 6.1 Claim A — five XML callback names do not resolve to a `RibbonViewer` method

- **Complete Family:** every Office CustomUI callback-name attribute value declared on a
  non-comment node of `TaskMaster/Ribbon/RibbonExplorer.xml`, paired with the set of public instance
  method names on `TaskMaster.RibbonViewer` (declared across both partial files
  `RibbonViewer.cs` and `RibbonViewer.EngineCommands.cs`). The defect set is the values with no
  matching method name.
- **Exhaustive Search Scope:** the entire 538-line XML document including the root
  `customUI` element, and both `RibbonViewer` partial source files in their entirety. Attribute
  families are not assumed: the whole document was scanned for *any* attribute whose name matches
  `\s(get[A-Z][A-Za-z]*|on[A-Z][A-Za-z]*)=`, which returned exactly seven distinct families —
  `onLoad`, `onAction`, `onChange`, `getLabel`, `getEnabled`, `getPressed`, `getText` — and no
  others (no `getVisible`, `getImage`, `getScreentip`, `getSupertip`, `getItemCount`,
  `getSelectedItemID`). The method side is likewise unrestricted: all `public` instance methods were
  enumerated, not only `_Click`-suffixed ones, so `GetHookButtonText`, `Ribbon_Load`,
  `EngineCommand_GetEnabled`, `HighConfidenceThreshold_GetText` and
  `HighConfidenceThreshold_OnChange` are all inside the scope.
- **Inclusion Rules:** attribute local name is `onAction`, `onChange`, or `onLoad`, or begins with
  `get`. The attribute's value is the bound method name, compared ordinally.
- **Exclusion Rules:** occurrences inside XML comment nodes (lines 361, 362, 369, 370, 371, 378,
  397, 398 — verified by reading lines 356-398); non-callback attributes (`id`, `idMso`, `label`,
  `imageMso`, `size`, `itemSize`, `xmlns`); commented-out C# method declarations
  (`RibbonViewer.EngineCommands.cs:248-250`).
- **Primary Search Strategy / Query Expression:** *document-side enumeration.* Grep
  `TaskMaster/Ribbon/RibbonExplorer.xml` with the callback-attribute alternation
  `onAction=|getPressed=|getEnabled=|getText=|onChange=|getImage=|getLabel=|getSupertip=|getScreentip=|getVisible=|getSelectedItemID=|getItemCount=`,
  add the root `onLoad="Ribbon_Load"` at line 2, subtract the eight comment-node lines, then check
  each resulting value against `RibbonViewer*.cs` by name.
- **Primary Member Set (unresolved values, with the XML line that declares each):**
  1. `BtnMigrateIDs_Click` (line 82)
  2. `MoveEntireConversation_Clicked` (line 268)
  3. `SaveAttachments_Clicked` (line 274)
  4. `SaveEmailCopy_Clicked` (line 280)
  5. `SavePictures_Clicked` (line 286)
- **Primary Count:** 5.
- **Cross-check Search Strategy / Query Expression:** *code-side enumeration, opposite direction.*
  Enumerate the declared public-callback surface with
  `^\s*public\s+(async\s+)?(void|bool|string|Task[^ ]*)\s+([A-Za-z0-9_]+)\s*\(` restricted to
  `TaskMaster/Ribbon/RibbonViewer*.cs`, yielding 95 declarations (27 in
  `RibbonViewer.EngineCommands.cs`, 68 in `RibbonViewer.cs`; two of them, `SetController` and
  `GetCustomUI`, are not ribbon callbacks). Then compute the set difference
  `{XML callback values} \ {declared method names}` from that independently built method set,
  without reusing the primary's per-name lookups.
- **Cross-check Member Set:** `BtnMigrateIDs_Click` (no `BtnMigrateIDs*` declaration of any kind
  exists); `MoveEntireConversation_Clicked` (the declared members are
  `MoveEntireConversation_GetPressed` at `RibbonViewer.cs:177` and `MoveEntireConversation_Click`
  at 180); `SaveAttachments_Clicked` (declared: `SaveAttachments_GetPressed` 183,
  `SaveAttachments_Click` 186); `SaveEmailCopy_Clicked` (declared: `SaveEmailCopy_GetPressed` 189,
  `SaveEmailCopy_Click` 192); `SavePictures_Clicked` (declared: `SavePictures_GetPressed` 195,
  `SavePictures_Click` 198).
- **Cross-check Count:** 5.
- **Member-set Comparison:** after ordinal normalization the two sets are identical —
  `{BtnMigrateIDs_Click, MoveEntireConversation_Clicked, SaveAttachments_Clicked,
  SaveEmailCopy_Clicked, SavePictures_Clicked}`. No name appears in one enumeration and not the
  other. **Counts agree: 5.**

### 6.2 Claim B — the callback family the enumeration test must scan

- **Complete Family:** all callback-name attribute occurrences in `RibbonExplorer.xml` that the new
  enumeration test will evaluate.
- **Exhaustive Search Scope:** the whole document, all attribute names (see 6.1's generic
  `get[A-Z]|on[A-Z]` scan, which is what establishes that the seven-family list is complete).
- **Inclusion / Exclusion Rules:** as in 6.1.
- **Primary Search Strategy / Query Expression:** the named-alternation grep of 6.1, line-numbered:
  105 matching lines, one occurrence per line, plus `onLoad` at line 2 = **106 total occurrences**;
  minus the 8 comment-node occurrences = **98 live occurrences**.
- **Primary Member Set:** the 98 live occurrences comprise 7 attribute families — `onLoad` ×1,
  `getLabel` ×1, `getText` ×1, `onChange` ×1, `getPressed` ×7, `getEnabled` ×14, `onAction` ×73.
- **Primary Count:** 98 live occurrences.
- **Cross-check Search Strategy / Query Expression:** the *generic* pattern
  `\s(get[A-Z][A-Za-z]*|on[A-Z][A-Za-z]*)=` with `--only-matching`, which does not name any
  attribute in advance and therefore independently establishes both the family list and the count.
- **Cross-check Member Set:** 106 emitted matches in document order, decomposing as `onLoad` ×1,
  `onAction` ×81 (73 live + 8 commented), `getEnabled` ×14, `getPressed` ×7, `getLabel` ×1,
  `getText` ×1, `onChange` ×1. Removing the 8 commented `onAction` occurrences yields the same 98.
- **Cross-check Count:** 106 total, 98 live.
- **Member-set Comparison:** both enumerations produce identical family multisets and identical
  totals (106 / 98). Deduplicating by value gives **84 distinct callback names**, consistent by
  arithmetic from both sides: 98 − 13 (the 14 `EngineCommand_GetEnabled` occurrences collapse to 1)
  − 1 (`FlagAsTask_Click` appears at lines 27 and 186) = 84. **Counts agree.**

### 6.3 Claim C — one XML element removed, four attribute values renamed

- **Complete Family:** the edits to `RibbonExplorer.xml` implied by Claim A's member set.
- **Exhaustive Search Scope:** the five unresolved names of 6.1.
- **Inclusion Rules:** a name is *renamed* when a correctly-signatured `RibbonViewer` method with
  the intended spelling already exists; it is *removed* when no implementation exists anywhere in
  the solution.
- **Exclusion Rules:** no other XML element or attribute is touched.
- **Primary Search Strategy / Query Expression:** partition Claim A's member set by whether
  `<name minus "ed">` resolves in the code-side method set of 6.1's cross-check. Renamed:
  `MoveEntireConversation_Clicked`→`_Click`, `SaveAttachments_Clicked`→`_Click`,
  `SaveEmailCopy_Clicked`→`_Click`, `SavePictures_Clicked`→`_Click`. Removed: `BtnMigrateIDs_Click`.
- **Primary Count:** 4 renames, 1 element removal.
- **Cross-check Search Strategy / Query Expression:** independent existence search for a
  `BtnMigrateIDs` implementation across the whole repository, using a different pattern family —
  case-insensitive `MigrateIDs|MigrateToDoIDs|MigrateToDoId|MigrateID` repo-wide (hits: only
  `RibbonExplorer.xml:82` plus documentation), and `Migrate` restricted to `*.cs` (one hit, an
  unrelated `//TODO:` at `UtilitiesCS/.../SmithWaterman.cs:49`).
- **Cross-check Member Set:** removal candidates = {`BtnMigrateIDs_Click`}; rename candidates =
  the four `_Clicked` names, each of which has a `_Click` twin in the code-side set with the exact
  Office `checkBox` signature `void (Office.IRibbonControl, bool)` at `RibbonViewer.cs:180,186,192,198`.
- **Cross-check Count:** 4 renames, 1 removal.
- **Member-set Comparison:** identical partitions. **Counts agree: 4 + 1 = 5**, matching Claim A.

---

## 7. Write Set

### 7.1 Modify

| # | Path | Change |
|---|---|---|
| 1 | `TaskMaster/Ribbon/RibbonExplorer.xml` | Delete the `BtnMigrateIDs` `<button>` element (line 82). Rename four `onAction` values `_Clicked` → `_Click` (lines 268, 274, 280, 286). Re-run CSharpier and accept any reflow. |
| 2 | `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | Add `_spamManagerResetGate` field + `SpamManagerReset` lazy property in the Spam Manager region; rewrite the body of `ClearSpamManagerAsync` (lines 217-232) to defer through `SpamManagerReset.RunAsync`. |
| 3 | `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | Add `_stateSequence`, nested `PressedState`, `NextSequence()`, `TryApplyState()`; retype `_pressedState`; update `GetPressed`, `ExecuteToggleAsync`, `ApplyPrimeAsync`; restructure `CompletePrime` for CR-2. |
| 4 | `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | Add two test methods plus one private `const string RibbonControlTypeName`. |
| 5 | `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | Add the `partial` keyword to the class declaration at line 23. No other change. |

### 7.2 Create

| # | Path | Contents |
|---|---|---|
| 6 | `TaskMaster/Ribbon/SpamManagerResetGate.cs` | The host-neutral gate specified in 3.3. Not `[ExcludeFromCodeCoverage]`. |
| 7 | `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs` | The nine tests of 5.4. |
| 8 | `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` | The six tests of 5.3, as a second partial of `EngineToggleStateCoordinatorTests`. |

### 7.3 Scope note — files outside the permitted directories

The delegation restricted changes to `TaskMaster/Ribbon/`, `TaskMaster.Test/Ribbon/`, and
(read-only) `TaskMaster/AppGlobals/`. Two files **outside** that set must nonetheless be edited, and
this is unavoidable rather than discretionary:

| # | Path | Required change | Why unavoidable |
|---|---|---|---|
| 9 | `TaskMaster/TaskMaster.csproj` | One `<Compile Include="Ribbon\SpamManagerResetGate.cs" />` in the item group at lines 458-470 | Legacy non-SDK project: files not listed are not compiled. |
| 10 | `TaskMaster.Test/TaskMaster.Test.csproj` | Two `<Compile Include=...>` entries for items 7 and 8, in the item group at lines 314-324 | Same. |

Both files are excluded from CSharpier by `.csharpierignore` lines 12-14, so no formatting concern
arises. No other file outside the permitted directories is touched.

`TaskMaster/AppGlobals/ApplicationGlobals.cs`, `TaskMaster/AppGlobals/AppAutoFileObjects.cs`,
`UtilitiesCS/Interfaces/IGlobals/*.cs` and
`UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs` were read for investigation
only and are **not** in the write set. `TaskMaster/AppGlobals/AppOlObjects.cs` and
`TaskMaster/AppGlobals/NonBlockingDelay.cs` were not opened at all, per the concurrent-work-item
constraint.

`TaskMaster.Test/Ribbon/RibbonViewerEngineCallbackShapeTests.cs` is deliberately **not** modified
(see the rejected placement in 2.4).

---

## 8. Out-of-scope observations to promote separately

1. **Unguarded `Globals` in the eight QuickFiler-settings members** of
   `RibbonController.Intelligence.cs:29-58` (`IsMoveEntireConversationActive`,
   `ToggleMoveEntireConversation`, `IsSaveAttachmentsActive`, `ToggleSaveAttachments`,
   `IsSavePicturesActive`, `ToggleSavePictures`, `IsSaveEmailCopyActive`, `ToggleSaveEmailCopy`;
   plus `IsHighConfidenceModeActive`, `ToggleHighConfidenceMode`, `GetHighConfidenceThresholdText`,
   `SetHighConfidenceThresholdText`). Already reachable today through the four `getPressed`
   callbacks and the dark-mode/high-confidence controls; Finding 1 adds a second entry point.
   Referenced by #524's site table but not by #735's finding 2. Promote as a follow-up.
2. **`BuildFolderClassifier_Click`** (`RibbonViewer.cs:239`) is a public callback with no
   `onAction` reference anywhere in `RibbonExplorer.xml` — the inverse of Finding 1 (an orphaned
   handler rather than an orphaned binding). Harmless; the enumeration test of 5.2 does not and
   should not fail on it, because the XML→code direction is the one that produces silent user-facing
   breakage. Worth a separate hygiene issue if the maintainer wants the reverse assertion.
3. **`TestSpamVerbose`, `SpamMetrics`, `SpamInvestigateErrors`**
   (`RibbonController.Intelligence.cs:235-248`) each `throw new NotImplementedException()` and are
   bound to live ribbon buttons (`RibbonExplorer.xml:165,171,177`). They resolve correctly, so they
   are outside Finding 1, but they are user-reachable unhandled exceptions from `async void`-free
   synchronous handlers. Separate issue.

---

## 9. Toolchain expectations for the implementer

Run the full loop in order and restart from step 1 on any failure or auto-fix:

1. `dotnet tool run csharpier format .` — **required** after the XML edit; verify with
   `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Additional per-change checks:

- Line counts after formatting for `EngineToggleStateCoordinator.cs`,
  `RibbonController.Intelligence.cs`, and `RibbonExplorerXmlTests.cs` (all must stay `< 500`).
- Confirm no new `[ExcludeFromCodeCoverage]` attribute was introduced anywhere.
- Confirm `SpamManagerResetGate.cs` contains no `using Microsoft.Office`, no
  `using System.Windows.Forms`, and no `log4net` reference.
