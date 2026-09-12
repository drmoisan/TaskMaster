# Research — QfcQueue enqueue-path injectable seams (issue #871)

- **Issue:** #871 (bug, work mode full-bug), parallel run `bugs-2026-09-11`
- **Feature folder:** `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/`
- **Worktree:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-acf29b54f5a64e04b`
- **Timestamp:** 2026-09-12T10-35
- **Tooling available to this agent:** Read, Grep, Glob only. No Bash, no git, no compiler, no test runner.
  Nothing in this document was verified by executing a command. Every claim is a static read of a
  file in the worktree, cited by repository-relative path and line number.

---

## 0. Summary of findings that change the plan

1. **`QfcQueue` is already constructible in a unit test today.** All three existing test files build a
   real instance through the real primary constructor, passing a literal `(QfcHomeController)null`.
   No reflection, no `FormatterServices.GetUninitializedObject` for the queue itself. The whole seam
   plan is reachable. (§Q1)
2. **A constructor-parameter seam for the move monitor is illegal.** `IEmailMoveMonitor` is `internal`
   and `QfcQueue`'s primary constructor is on a `public` class, so adding an `IEmailMoveMonitor`
   parameter produces an inconsistent-accessibility error. The seam must be an `internal` member. (§Q2)
3. **The `IUiDispatcher` interface already in `UtilitiesCS.Threading` cannot be reused as-is**: it has
   no priority-taking overload for the `Func<T>` and `Func<Task<T>>` shapes, so routing `QfcQueue`
   through it would silently drop `DispatcherPriority.ContextIdle`. Extending it would break one
   production implementer and nine hand-written test implementers on a target framework with no
   default interface members. (§Q4)
4. **A C# language constraint forces the seam shape.** An instance field or auto-property initializer
   cannot reference `this`, so any seam whose production default is an *instance* method of `QfcQueue`
   (`AddAsync`, `AddViewerToTlp`) cannot use the `ItemControllerFactory` initializer form. A lazy
   `??=` property getter is required for those. (§Q3)
5. **A single `Func<TableLayoutPanel,MailItem,int,Task<QfcItemGroup>>` seam is NOT sufficient.** It
   makes `LoadControllersViewersAsync` coverable but relocates the uncovered region into `AddAsync`,
   which stays at 0%. Two finer seams inside `AddAsync` remove the residual. (§Q3, §Q7)
6. **A latent hang defect sits on this exact path.** `Interlocked.Increment(ref _jobsRunning)` is
   *outside* the `try` whose `finally` decrements it. Any throw between the increment and the `try`
   permanently leaks the counter and makes `CompleteAddingAsync`, `TryDequeueAsync`, and
   `JobsToFinish` spin. Recommend a follow-up issue, not an in-scope fix. (§Q7, §6)
7. **`quality-tiers.yml` does not exist at the repository root**, so QuickFiler has no tier
   classification. (§Q8)

---

## 1. Numeric Derivation Evidence

The single numeric population this research proposes for promotion into an approved `spec.md`
acceptance criterion is the set of `QfcQueue` production source files and their physical line counts.
The spec currently asserts "`QfcQueue.cs` is at 439 lines" (`spec.md:56`); that assertion is wrong and
must be replaced, so the replacement is derived here in full.

- **Complete Family:** every tracked C# source file in the repository whose file name begins with
  `QfcQueue`, partitioned into the production partition (under `QuickFiler/`) and the test partition
  (under `QuickFiler.Test/`). The family includes every partial part of `QuickFiler.Controllers.QfcQueue`
  and every `[TestClass]` that targets it. It excludes no member of either partition.
- **Exhaustive Search Scope:** the entire worktree at
  `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-acf29b54f5a64e04b`, both partitions,
  all directories, not restricted to any one directory or to any one named file.
- **Inclusion Rules:** file name matches `QfcQueue*.cs`; file is tracked in the worktree; file
  compiles into either `QuickFiler` or `QuickFiler.Test`.
- **Exclusion Rules:** `.bak` files are excluded (they are not compiled); documentation, evidence
  XML, and agent-memory files that merely mention `QfcQueue` are excluded (they are not C# sources);
  files declaring types *used by* `QfcQueue` but not named `QfcQueue*` (for example
  `QfcItemGroup.cs`, `IQfcQueue.cs`) are excluded because they are not parts of the family.
- **Primary Search Strategy or Query Expression:** two Glob invocations over the whole worktree —
  `QuickFiler/**/QfcQueue*.cs` and `QuickFiler.Test/**/QfcQueue*.cs` — followed by a full `Read` of
  each returned path, taking the last line number the reader emitted as the physical line count.
- **Primary Member Set:**
  | Path | Partition | Physical lines (last numbered line read) |
  |---|---|---|
  | `QuickFiler/Controllers/QfcQueue.cs` | production | 507 |
  | `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | production | 200 |
  | `QuickFiler.Test/Controllers/QfcQueueTests.cs` | test | 67 |
  | `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | test | 418 |
  | `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs` | test | 290 |
- **Primary Count:** 2 production files, 3 test files, 5 total.
- **Cross-check Search Strategy or Query Expression:** a different mechanism that does not read the
  filesystem layout at all — the explicit MSBuild compile manifests. Grep for
  `Compile Include="Controllers\\QfcQueue` in `QuickFiler/QuickFiler.csproj` and for
  `<Compile Include="Controllers\\QfcQueue` in `QuickFiler.Test/QuickFiler.Test.csproj`. Because both
  projects are legacy non-SDK projects with no globbing (§Q5), the manifest is an independent and
  authoritative enumeration of the compiled family. Corroborated by a third, orthogonal query: Grep
  for `filename="QuickFiler\\Controllers\\QfcQueue` across `**/*.cobertura.xml`, which enumerates the
  production partition as the coverage tool observed it at build time.
- **Cross-check Member Set:**
  - `QuickFiler/QuickFiler.csproj:348` → `<Compile Include="Controllers\QfcQueue.cs" />`
  - `QuickFiler/QuickFiler.csproj:349` → `<Compile Include="Controllers\QfcQueue.Enqueue.cs" />`
  - `QuickFiler.Test/QuickFiler.Test.csproj:119` → `<Compile Include="Controllers\QfcQueueCoverageExpansionTests.cs" />`
  - `QuickFiler.Test/QuickFiler.Test.csproj:120` → `<Compile Include="Controllers\QfcQueuePurePathsTests.cs" />`
  - `QuickFiler.Test/QuickFiler.Test.csproj:215` → `<Compile Include="Controllers\QfcQueueTests.cs" />`
  - Cobertura corroboration: `docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/coverage-postchange.cobertura.xml:41897`
    (`filename="QuickFiler\Controllers\QfcQueue.cs"`) and `:42890`
    (`filename="QuickFiler\Controllers\QfcQueue.Enqueue.cs"`) — exactly two production members, no third.
- **Cross-check Count:** 2 production files, 3 test files, 5 total.
- **Member-set Comparison:** normalizing both sets to repository-relative paths with forward slashes,
  the primary set and the cross-check set are element-for-element identical in both partitions
  (`QuickFiler/Controllers/QfcQueue.cs`, `QuickFiler/Controllers/QfcQueue.Enqueue.cs`;
  `QuickFiler.Test/Controllers/QfcQueueTests.cs`,
  `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs`,
  `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs`). No member appears in one set and
  not the other. The counts agree at 2 / 3 / 5. The line counts are therefore safe to assert, and the
  `spec.md:56` figure of 439 for `QfcQueue.cs` must be corrected to 507.

**Consequence for the plan:** `QuickFiler/Controllers/QfcQueue.cs` at 507 lines is already 7 lines over
the 500-line hard ceiling in `CLAUDE.md` § "Module & File Structure" item 1 and
`.claude/rules/general-code-change.md` § "File Size Limit". A split is mandatory before any seam is
added, not optional. `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` at 418 lines has only
82 lines of headroom, which is insufficient for the new test suite; a new test file is required.

---

## 2. Q1 — Can a test construct `QfcQueue` at all today?

**Answer: yes, and it already does, three times, with no reflection on the constructor.**

The declaration under test, `QuickFiler/Controllers/QfcQueue.cs:20-24`:

```csharp
    public partial class QfcQueue(
        CancellationToken token,
        QfcHomeController homeController,
        IApplicationGlobals appGlobals
    ) : IQfcQueue
```

Verbatim construction code in each existing test file:

`QuickFiler.Test/Controllers/QfcQueueTests.cs:40-41`
```csharp
                var appGlobals = new Mock<IApplicationGlobals>().Object;
                var queue = new QfcQueue(cts.Token, (QfcHomeController)null, appGlobals);
```

`QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs:35-39`
```csharp
        private static QfcQueue NewQueue(CancellationToken token)
        {
            var globals = new Mock<IApplicationGlobals>().Object;
            return new QfcQueue(token, (QfcHomeController)null, globals);
        }
```

`QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs:25-29`
```csharp
        private static QfcQueue NewQueue()
        {
            var globals = new Mock<IApplicationGlobals>().Object;
            return new QfcQueue(CancellationToken.None, (QfcHomeController)null, globals);
        }
```

So the pattern is: **a real instance through the real primary constructor, with a null concrete
`QfcHomeController` and a loose Moq `IApplicationGlobals`.** `FormatterServices.GetUninitializedObject`
is used in these files only for `QfcDatamodel` (`QfcQueuePurePathsTests.cs:41-42`), never for
`QfcQueue`. Post-construction state is then injected by private-field reflection through the shared
helper `SetPrivateField` (`QfcQueuePurePathsTests.cs:44-51`, `QfcQueueCoverageExpansionTests.cs:92-99`),
which asserts the field exists before setting it:

```csharp
            FieldInfo field = target.GetType().GetField(name, NonPublicInstance);
            field
                .Should()
                .NotBeNull($"private field '{name}' should exist on {target.GetType().Name}");
            field.SetValue(target, value);
```

### Why the constructor is safe to call headlessly

The primary constructor body is empty; all work is in field initializers at
`QuickFiler/Controllers/QfcQueue.cs:32-42` plus the `ItemControllerFactory` initializer at
`QuickFiler/Controllers/QfcQueue.Enqueue.cs:33-56`. The only non-trivial initializer is
`_moveMonitor = new EmailMoveMonitor()` (`QfcQueue.cs:42`). That constructor,
`QuickFiler/Helper Classes/EmailMoveMonitor.cs:42-46`, is:

```csharp
        public EmailMoveMonitor(Action<System.Action> marshalToSta = null)
        {
            _marshalToSta = marshalToSta ?? (action => UiThread.Dispatcher.Invoke(action));
            SetupBeforeItemMove();
        }
```

The default `_marshalToSta` is a *lazy* lambda — `UiThread.Dispatcher` is not read at construction
time, only when `HookItem`/`UnhookItem`/`UnhookAll` later invoke the delegate. `SetupBeforeItemMove`
(`EmailMoveMonitor.cs:208-227`) only assigns a delegate field. So constructing `QfcQueue` touches
neither the WPF dispatcher nor Outlook COM. This is consistent with the three test files passing
today without any dispatcher fixture.

### Is `QfcHomeController` mockable?

`QuickFiler/Controllers/QfcHomeController.cs:19` declares `public partial class QfcHomeController :
IQfcHomeController` — public and non-sealed. `QfcHomeController.cs:27` is a `private
QfcHomeController() { }` and `:29-33` is a public `QfcHomeController(IApplicationGlobals globals,
System.Action parentCleanup)` whose body sets exactly two properties. Therefore:

- Moq **could** construct `new Mock<QfcHomeController>(globals, action)` because the class is public,
  non-sealed, and has an accessible constructor. Whether any member is overridable is **not verified**
  — I did not enumerate the virtuality of its members, and doing so is unnecessary (below).
- **Mocking is not required.** `_homeController` is dereferenced in exactly one place on any path
  reachable from this item: `QuickFiler/Controllers/QfcQueue.cs:373`,
  `await _homeController.DataModel.DequeueNextItemGroupAsync(...)`, inside `ChangeIterationSize`.
  That method is not on the `EnqueueAsync` path. `EnqueueAsync` only *passes* `_homeController`
  (`QfcQueue.Enqueue.cs:108`) into `LoadControllersViewersAsync`'s `IFilerHomeController homeController`
  parameter (`:156`), which forwards it to `ItemControllerFactory` (`:183`). A substituted factory
  ignores it, so `null` is safe for every branch this item needs.
- If a test wants to assert that a *non-null* home controller reaches the factory, it cannot supply a
  Moq `IFilerHomeController`, because the queue's field is typed as the concrete `QfcHomeController`
  (`QfcQueue.cs:33`). The cheap alternative is a real `new QfcHomeController(globals, () => { })`,
  whose constructor sets only two properties and performs no I/O.

**Conclusion for Q1:** the seam plan is fully reachable. No new construction affordance is required.

Supporting fact for internal seams: `QuickFiler/Controllers/QfcHomeController.cs:15` carries
`[assembly: InternalsVisibleTo("QuickFiler.Test")]`, so `internal` seam members on `QfcQueue` are
visible to the test assembly. This is what already makes `ItemControllerFactory` (internal) assertable
at `QfcQueuePurePathsTests.cs:382`.

---

## 3. Q2 — The move-monitor seam

### Current state

`QuickFiler/Controllers/QfcQueue.cs:41-42`:

```csharp
        // Deliberately one monitor instance per owner, not a shared singleton: EmailMoveMonitor.BeforeItemMove dispatches at most one action per MailItem via FirstOrDefault, and UnhookAll is instance-scoped and clears the whole hook list, so a shared instance would both drop sibling owners' actions and unhook them all on any one owner's teardown (issue #731 finding 1, issue #620).
        private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();
```

The invariant is corroborated at the implementation: `QuickFiler/Helper Classes/EmailMoveMonitor.cs:17-22`
repeats it in the class header comment, `:48` holds the per-instance `private List<EmailMoveAction>
_hookedItems = []`, `:216-218` dispatches via `FirstOrDefault`, and `:189-204` `UnhookAll` clears the
whole instance list.

### Type accessibility blocks the constructor-parameter form

`QuickFiler/Interfaces/IEmailMoveMonitor.cs:13` declares `internal interface IEmailMoveMonitor`.
`QfcQueue` is `public partial class` (`QfcQueue.cs:20`) and its primary constructor is therefore a
public constructor. Adding a parameter of an `internal` type to a public constructor is an
inconsistent-accessibility error (CS0051). **The constructor-parameter seam form named in the spec's
root-cause section (`spec.md:52`, "a constructor or settable-property seam is sufficient") is only
half available: the settable-property half.** The alternatives — widening `IEmailMoveMonitor` to
public, or adding a second non-primary constructor overload — both enlarge the public API surface for
no test benefit, and widening would also require widening `EmailMoveMonitor` (`EmailMoveMonitor.cs:22`,
`internal class`) or leaving a public interface with no public implementation.

A further constraint specific to primary constructors: a primary constructor has no body, so a new
parameter could only be consumed by a *field initializer*. A `?? new EmailMoveMonitor()` default in an
initializer is expressible, but it does not remove the CS0051 problem.

### Recommended form

Keep the field and its load-bearing comment exactly as they are, and add an `internal` property
wrapper on the same part:

```csharp
        /// <summary>
        /// Issue #871 seam over <see cref="_moveMonitor"/>. The default remains the per-owner
        /// instance created by the field initializer above, so the one-instance-per-owner invariant
        /// (issue #731 finding 1, issue #620) is unchanged: this property never creates, shares, or
        /// caches an instance across owners; it only lets a test replace this owner's instance.
        /// </summary>
        internal IEmailMoveMonitor MoveMonitor
        {
            get => _moveMonitor;
            set => _moveMonitor = value ?? throw new ArgumentNullException(nameof(value));
        }
```

Rationale, in order of weight:

1. **It preserves the invariant by construction.** The default is still produced once per `QfcQueue`
   instance by the existing field initializer. A `Func<IEmailMoveMonitor>` factory seam would *also*
   preserve it, but only if the implementation is careful to invoke it exactly once per owner; the
   property form makes over-sharing impossible rather than merely unlikely. Given that the comment at
   `QfcQueue.cs:41` exists precisely because a prior change shared an instance, the form that cannot
   regress is preferable.
2. **It does not break the three existing reflection injection sites.** `QfcQueueCoverageExpansionTests.cs:119`,
   `:145`, and `:207` all call `SetPrivateField(queue, "_moveMonitor", moveMonitor.Object)`. Converting
   the field to an auto-property would rename the backing field to `<MoveMonitor>k__BackingField`,
   `GetField("_moveMonitor", ...)` would return null, and `SetPrivateField`'s `.Should().NotBeNull()`
   assertion (`QfcQueueCoverageExpansionTests.cs:95-97`) would fail all three tests. **Keeping the
   explicit `_moveMonitor` field is load-bearing for the existing suite.**
3. **It is `internal`, which is mandatory** (the property type is an internal interface; a `public`
   property of an internal type on a public class is CS0053). `internal` matches `ItemControllerFactory`
   (`QfcQueue.Enqueue.cs:33`) and is visible to `QuickFiler.Test` via the `InternalsVisibleTo` at
   `QfcHomeController.cs:15`.

Rejected alternative: `internal Func<IEmailMoveMonitor> MoveMonitorFactory { get; set; } = () => new
EmailMoveMonitor();` with the field becoming lazily initialized. This is form 2 of the DI guidance and
mirrors `QfcDatamodel.ScoringServiceFactory` more literally, but it converts a fixed per-owner instance
into a per-call one, which is exactly the failure mode the `QfcQueue.cs:41` comment warns about unless
the call site memoizes. Not worth the risk for zero additional test power.

### Test consumption

`Mock<IEmailMoveMonitor>(MockBehavior.Strict)` with `Setup(x => x.HookItem(item, It.IsAny<Action<MailItem>>()))`.
The existing suite already mocks this interface strictly (`QfcQueueCoverageExpansionTests.cs:113-115`,
`QfcQueuePurePathsTests.cs:120-122`), so the pattern is established. Note that `EnqueueAsync` passes an
`async (x) => await RemoveItem(x)` lambda as the `Action<MailItem>` argument
(`QfcQueue.Enqueue.cs:91`) — that is an async-void lambda; a test must not invoke it, only capture it.

---

## 4. Q3 — The `AddAsync` / viewer-construction seam

### The code under discussion

`QuickFiler/Controllers/QfcQueue.cs:264-277`:

```csharp
        internal async Task<QfcItemGroup> AddAsync(
            TableLayoutPanel tlp,
            MailItem mailItem,
            int indexNumber
        )
        {
            //TraceUtility.LogMethodCall(tlp, mailItem, indexNumber);

            var grp = new QfcItemGroup(mailItem);
            var viewer = ItemViewerQueue.Dequeue(_token);
            grp.ItemViewer = viewer;
            await UiIdleCallAsync(() => AddViewerToTlp(tlp, viewer, indexNumber));
            return grp;
        }
```

### What `ItemViewerQueue` is

**Declaring file and line: `QuickFiler/Helper Classes/ItemViewerQueue.cs:9`** —
`public static class ItemViewerQueue`, namespace `QuickFiler`. It is a **static class**, not an
instance property. It is registered in the compile manifest at `QuickFiler/QuickFiler.csproj:357`.

The member called from `AddAsync` is `ItemViewerQueue.cs:46-55`:

```csharp
        public static ItemViewer Dequeue(CancellationToken token)
        {
            return _core.Dequeue(
                token,
                DispatcherPriority.Render,
                1,
                1,
                DispatcherPriority.ContextIdle
            );
        }
```

`_core` is a private static `ViewerQueueCore<ItemViewer>` (`ItemViewerQueue.cs:29`). Critically, the
static class **already carries its own test seams**: `SetCoreForTesting(ViewerQueueCore<ItemViewer>)`
at `:69-72`, `ResetCoreForTesting()` at `:77-81`, `ResetProductionCoreDefaultsForTesting()` at `:83-91`,
and four settable `internal static` production delegates at `:11-27`. `QuickFiler.Test` already drives
them: `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:105`, `:141`, with a
`[DoNotParallelize]` class attribute at `:11` and a `[TestCleanup]` restoring both queues at `:15-22`.
The fake viewers that suite supplies are built by `CreateUninitialized<ItemViewer>()`
(`ViewerQueueStaticWrapperTests.cs:97`, `:128`, `:330`), i.e. `GetUninitializedObject`.

So `ItemViewerQueue.Dequeue` is *already* substitutable without a new seam — but only via
**process-global mutable static state**, which is why that suite is `[DoNotParallelize]`. Relying on it
from `QfcQueue` tests would import that global-state coupling into a second test class.

### What `QfcItemGroup` requires

`QuickFiler/Controllers/QfcItemGroup.cs:16-60`. It is a plain `public class` with a parameterless
constructor (`:18`) and a `QfcItemGroup(MailItem mailItem)` constructor (`:20-23`) that only assigns a
field. All five members are simple auto-or-backed properties: `MailItem` (`:26`, internal),
`ItemViewer` (`:32`, internal, typed as the **concrete WinForms `ItemViewer`**), `ItemController`
(`:39`, internal `IQfcItemController`), `PredeterminedFolder` (`:50`), `CarriedFolderHandler` (`:59`).
A test can construct one freely — `QfcQueueCoverageExpansionTests.cs:66-69` already does:

```csharp
        private static QfcItemGroup NewGroup(MailItem mailItem, IQfcItemController controller)
        {
            return new QfcItemGroup(mailItem) { ItemController = controller };
        }
```

`ItemViewer` may be left null. The downstream consumer, `LoadControllersViewersAsync`
(`QfcQueue.Enqueue.cs:185`), passes `x.grp.ItemViewer` into the `IItemViewer` parameter of
`ItemControllerFactory`; a null reference converts implicitly and a substituted factory never
dereferences it. **No `ItemViewer` instance is needed anywhere on the seamed path.**

### Is one delegate seam sufficient?

**No.** A single `Func<TableLayoutPanel, MailItem, int, Task<QfcItemGroup>>` makes every line of
`LoadControllersViewersAsync` coverable, but the production default it displaces is `AddAsync` itself,
which then remains at 0% forever. Per the coverage baseline (§Q8), `AddAsync`'s body is currently
uncovered; a single seam relocates the hole rather than closing it, and §Q7's "still unreachable"
column would list `AddAsync`. Since `AddAsync` is only 5 executable statements, closing it is cheap.

### Recommended seam set (two finer seams plus the coarse one)

```csharp
        // Default is a public static method group — no `this` capture, so a plain
        // property initializer is legal (see the `this`-capture note below).
        internal Func<CancellationToken, ItemViewer> ItemViewerFactory { get; set; } =
            ItemViewerQueue.Dequeue;

        // Default is an INSTANCE method, so it cannot appear in a property initializer.
        // Bound lazily on first read; the default is byte-for-byte the previous call.
        private Action<TableLayoutPanel, ItemViewer, int> _viewerRowPlacer;
        internal Action<TableLayoutPanel, ItemViewer, int> ViewerRowPlacer
        {
            get => _viewerRowPlacer ??= AddViewerToTlp;
            set => _viewerRowPlacer = value ?? throw new ArgumentNullException(nameof(value));
        }
```

and `AddAsync`'s body becomes:

```csharp
            var grp = new QfcItemGroup(mailItem);
            var viewer = ItemViewerFactory(_token);
            grp.ItemViewer = viewer;
            await UiIdleCallAsync(() => ViewerRowPlacer(tlp, viewer, indexNumber));
            return grp;
```

With `ItemViewerFactory` returning `null` and `ViewerRowPlacer` a no-op recorder, a test covers all
five lines of `AddAsync` without `ItemViewerQueue`'s global state, without an `ItemViewer`, and without
touching a real `TableLayoutPanel`'s control collection.

**Additionally** keep a coarse seam so `LoadControllersViewersAsync` can be exercised in isolation from
`AddAsync`:

```csharp
        private Func<TableLayoutPanel, MailItem, int, Task<QfcItemGroup>> _itemGroupFactory;
        internal Func<TableLayoutPanel, MailItem, int, Task<QfcItemGroup>> ItemGroupFactory
        {
            get => _itemGroupFactory ??= AddAsync;
            set => _itemGroupFactory = value ?? throw new ArgumentNullException(nameof(value));
        }
```

with `QfcQueue.Enqueue.cs:177` changed from `await AddAsync(tlp, items[i - start], i)` to
`await ItemGroupFactory(tlp, items[i - start], i)`.

### The `this`-capture constraint — why the lazy getter is required

`ItemControllerFactory` (`QfcQueue.Enqueue.cs:33-56`) can use a plain property initializer because its
default lambda captures nothing from the instance — it only calls `new QfcItemController(...)` with the
lambda's own parameters. The coverage evidence confirms the compiler emitted it as a cached, closure-free
lambda: `coverage-postchange.cobertura.xml:42927` names the method `<.ctor>b__0_0`, and
`:42892` shows `.ctor` itself covering only line 45.

A default of `AddAsync` or `AddViewerToTlp` is different: those are *instance* methods, so the method
group conversion captures `this`, and C# forbids `this` in an instance field or auto-property
initializer. The lazy `??=` getter is the standard workaround: property accessors do have `this` in
scope. It also yields a non-null default on first read, so a test can assert
`queue.ItemGroupFactory.Should().NotBeNull()` in the same shape as
`QfcQueuePurePathsTests.cs:364-368` asserts for `ItemControllerFactory`.

`??=` requires C# 8 or later; `QuickFiler/QuickFiler.csproj:14` sets `<LangVersion>preview</LangVersion>`,
which is also why the primary constructor (`QfcQueue.cs:20`) and the collection expression `[]`
(`QfcQueue.cs:38-39`) already compile. No language-version change is needed.

### Note on issue #781 exposure

`AddAsync`'s production default reaches `ItemViewerQueue.Dequeue` → `ViewerQueueCore` →
`UiThread.Dispatcher.Invoke`, and the resulting `ItemViewer` captures a `DispatcherSynchronizationContext`.
That is the documented #781/#784 hazard (`docs/features/potential/promoted/2026-09-05-uithread-synccontext-awaiter-always-posts-for-dispatcher-built-viewers.md:25`).
Because the recommended seams leave the production default unchanged, this item neither introduces nor
mitigates that hazard. See §Q4 for the direct check inside `QfcQueue`.

---

## 5. Q4 — The UI idle-call marshalling seam

### Current state

`QuickFiler/Controllers/QfcQueue.cs:474-503`, three members inside `#region Helper Methods`:

```csharp
        internal async Task UiIdleCallAsync(System.Action action)
        {
            await UiThread.Dispatcher.InvokeAsync(
                action,
                System.Windows.Threading.DispatcherPriority.ContextIdle
            );
        }

        internal async Task<T> UiIdleCallAsync<T>(Func<T> func)
        {
            return await UiThread.Dispatcher.InvokeAsync(
                func,
                System.Windows.Threading.DispatcherPriority.ContextIdle
            );
        }

        internal async Task<T> UiIdleAsyncCallAsync<T>(Func<Task<T>> func)
        {
            T result = await await UiThread.Dispatcher.InvokeAsync(
                async () =>
                {
                    T result = await func();
                    await Task.Yield();
                    return result;
                },
                System.Windows.Threading.DispatcherPriority.ContextIdle
            );
            return result;
        }
```

Call sites inside the class: `QfcQueue.cs:197` (`RemoveItem`), `QfcQueue.cs:275` (`AddAsync`),
`QfcQueue.Enqueue.cs:97` and `:105` (`EnqueueAsync`). Four sites, three shapes.

### What `UiThread` and `Dispatcher` are

`UtilitiesCS/Threading/UiThread.cs:17` — `public static class UiThread`. Its `Dispatcher` property is
`UiThread.cs:251-269`, typed `System.Windows.Threading.Dispatcher` (a WPF dispatcher, not a WinForms
one), backed by `private static Dispatcher? _dispatcher` at `:270`. The accessor is **deliberately not
lazy** and **throws** when unset:

```csharp
                Dispatcher? captured = _dispatcher;
                if (captured is null)
                {
                    throw new InvalidOperationException(DispatcherNotInitializedMessage);
                }
                return captured;
```

with the rationale documented at `:239-246`. This is the hard blocker: in a unit test with no host
startup, every one of the four call sites throws `InvalidOperationException` at the first
`UiThread.Dispatcher` read. That is why `EnqueueAsync` is untestable today regardless of the other
boundaries.

### Why the existing `IUiDispatcher` cannot be reused unmodified

`UtilitiesCS/Threading/IUiDispatcher.cs:15-42` already exists and is exactly the right *kind* of seam.
Its members are `Invoke(Action)` (`:18`), `InvokeAsync(Action)` (`:21`),
`InvokeAsync(Action, DispatcherPriority, CancellationToken)` (`:27`), `BeginInvoke(Action)` (`:30`),
`InvokeAsync<TResult>(Func<TResult>)` (`:35`), and `InvokeAsync<TResult>(Func<Task<TResult>>)` (`:41`).

Two mismatches make direct reuse a behaviour change rather than a refactor:

1. **Priority is only expressible for the `Action` shape.** `InvokeAsync<TResult>(Func<TResult>)` and
   `InvokeAsync<TResult>(Func<Task<TResult>>)` take no `DispatcherPriority`. The production adapter
   `UtilitiesCS/Threading/WpfUiDispatcher.cs:56-61` forwards them as
   `Dispatcher.InvokeAsync(func).Task` and `Dispatcher.InvokeAsync(func).Task.Unwrap()` — i.e. at the
   WPF default `DispatcherPriority.Normal`. Routing `QfcQueue` through it would silently promote the
   two generic call sites from `ContextIdle` to `Normal`, changing when background page construction
   runs relative to input and rendering. That violates the spec's own constraint that "production
   defaults must reproduce the previous construction expressions exactly" (`spec.md:105`).
2. **The `Func<Task<TResult>>` body differs.** `QfcQueue.UiIdleAsyncCallAsync` wraps the caller's
   func in an inner async lambda that performs `await Task.Yield()` before returning
   (`QfcQueue.cs:495-497`). `WpfUiDispatcher.cs:60-61` has no yield. The yield changes the
   continuation scheduling for the enqueue path.

Adding priority overloads to `IUiDispatcher` is possible but expensive: `QuickFiler.Test`,
`UtilitiesCS.Test`, and `TaskMaster.Test` contain **nine hand-written implementations** of the
interface —
`TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs:219`,
`TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.Coverage.cs:210`,
`UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerRefreshDisposalTests.cs:274` and `:365`,
`UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerInitializationTests.cs:321`,
`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceTraversalCancellationTests.cs:353`,
`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceInvalidationTests.cs:379`,
`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceDisposalTests.cs:384`,
`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs:165`
— plus the single production implementer `UtilitiesCS/Threading/WpfUiDispatcher.cs:17`. On
`TargetFrameworkVersion v4.8.1` there are no default interface members, so every one of the ten must be
edited. That is a ten-file blast radius in three assemblies for a change whose only beneficiary is
`QfcQueue`. **Reject.**

### Why a delegate seam per shape does not work

A field or property cannot be generic over a method type parameter. `UiIdleCallAsync<T>(Func<T>)` and
`UiIdleAsyncCallAsync<T>(Func<Task<T>>)` are generic methods; there is no `Func<>` type that can hold
"a function that, for any `T`, maps `Func<T>` to `Task<T>`". The only delegate-based encodings are
(a) a non-generic `Func<Func<object>, Task<object>>` with boxing and casts at every call site, or
(b) one delegate per concrete `T` actually used — currently `TableLayoutPanel` (`Enqueue.cs:97`) and
`List<QfcItemGroup>` (`:105`), which is the combinatorial explosion the task brief asks to avoid and
which would grow with every new call site. **Reject.**

### Recommended seam: a narrow new interface (form 1, the preferred tier)

`.claude/rules/csharp.md:49-53`, verbatim:

> Introduce the smallest seam that enables reliable unit testing. Apply in this order of preference:
>
> 1. **Interface seam (preferred)** — extract boundary calls into narrow purpose-specific interfaces (for example, `IProcessRunner`, `IFileSystem`, `IClock`). Keep interfaces minimal.
> 2. **Injectable delegate seam** — use a narrow `Func<>`/`Action<>` delegate for a single call path when a full interface is excessive. Default behavior must remain safe and deterministic.
> 3. **Adapter seam for static or third-party APIs** — wrap the static or third-party call behind a small adapter so tests can mock the adapter with Moq.

The generic-method requirement rules out tier 2, and tier 1 and tier 3 coincide here (a narrow
interface plus a static-wrapping adapter). New production files:

`QuickFiler/Interfaces/IUiIdleDispatcher.cs`
```csharp
    internal interface IUiIdleDispatcher
    {
        Task InvokeIdleAsync(Action action);
        Task<T> InvokeIdleAsync<T>(Func<T> func);
        Task<T> InvokeIdleAsync<T>(Func<Task<T>> func);
    }
```

`QuickFiler/Helper Classes/UiThreadIdleDispatcher.cs` — the production implementation, holding the
three current bodies **verbatim**, including the `ContextIdle` priority on all three and the
`await await` plus `await Task.Yield()` in the third. This satisfies requirement (b) of the brief:
byte-for-byte behavioural equivalence, because the bodies are moved rather than rewritten.

`QfcQueue` then gains, on the part that owns the helpers:

```csharp
        private IUiIdleDispatcher _uiIdleDispatcher;
        internal IUiIdleDispatcher UiIdleDispatcher
        {
            get => _uiIdleDispatcher ??= new UiThreadIdleDispatcher();
            set => _uiIdleDispatcher = value ?? throw new ArgumentNullException(nameof(value));
        }
```

and the three existing `UiIdleCallAsync` / `UiIdleAsyncCallAsync` members are **kept as one-line
forwards** to it. Keeping them means the four internal call sites (`QfcQueue.cs:197`, `:275`,
`QfcQueue.Enqueue.cs:97`, `:105`) are not edited at all, which minimizes review surface and keeps the
"no unintended behavior changes" acceptance criterion easy to evidence. The default is lazy, so
`UiThread.Dispatcher` is still never read at construction time — preserving the Q1 property that
`new QfcQueue(...)` is headless-safe.

Requirement (a) of the brief — a synchronous pass-through — is served by a hand-written test fake
rather than Moq, because Moq's handling of generic methods whose return type depends on the type
parameter is awkward. The repository already writes such fakes by hand nine times for `IUiDispatcher`
(list above), so this is the established pattern, not a new one:

```csharp
        private sealed class InlineUiIdleDispatcher : IUiIdleDispatcher
        {
            public Task InvokeIdleAsync(Action action) { action(); return Task.CompletedTask; }
            public Task<T> InvokeIdleAsync<T>(Func<T> func) => Task.FromResult(func());
            public Task<T> InvokeIdleAsync<T>(Func<Task<T>> func) => func();
        }
```

This is fully deterministic: no timer, no dispatcher, no thread hop, satisfying
`.claude/rules/general-unit-test.md` § "Determinism Infrastructure".

### Issue #781 reference-equality check — result

**Nothing in `QfcQueue` compares a `SynchronizationContext` by reference, or reads one at all.** A Grep
for `SynchronizationContext` restricted to `QuickFiler/Controllers/QfcQueue*.cs` returns no matches.
The #781 hazard (`Dispatcher.Invoke` installing a throwaway `DispatcherSynchronizationContext`, so a
reference-equality UI guard fails on the UI thread) therefore does not apply inside the members this
item changes. It applies downstream, in the viewer that `ItemViewerQueue.Dequeue` constructs — see the
end of §Q3. Because the recommended seams preserve production defaults, the recommended change neither
adds nor removes #781 exposure. A test using `InlineUiIdleDispatcher` bypasses the dispatcher entirely
and so cannot reproduce or mask it.

---

## 6. Q5 — The mandatory split of `QuickFiler/Controllers/QfcQueue.cs`

### Why a split is mandatory

507 lines (§1) against a 500-line hard ceiling stated twice: `CLAUDE.md` § "Module & File Structure"
item 1 ("Do not exceed 500 lines for any one file") and `.claude/rules/general-code-change.md`
§ "File Size Limit" ("No production code, test code, or reusable script file may exceed **500 lines**").
Neither exception clause (throwaway agent scripts, raw text fixtures, Markdown) applies. The file is
over budget *before* any seam is added, and the seams in §Q2–§Q4 add roughly 25 more lines.

`QuickFiler/Controllers/QfcQueue.Enqueue.cs` at 200 lines has ample headroom and does not need to move;
its own header comment (`QfcQueue.Enqueue.cs:14-22`) records that it was created for exactly this reason
and ends with the constraint the brief cites: "The primary constructor stays on the base part."

### C# legality of moving members off the primary-constructor part

The primary constructor parameters `token`, `homeController`, `appGlobals` (`QfcQueue.cs:21-23`) are
referenced in exactly three places, all field initializers on the same part:

- `QfcQueue.cs:32` — `private CancellationToken _token = token;`
- `QfcQueue.cs:33` — `private QfcHomeController _homeController = homeController;`
- `QfcQueue.cs:35` — `private IApplicationGlobals _globals = appGlobals;`

I read `QfcQueue.cs:230-505` in full. Every member in the two regions proposed for relocation refers to
the **fields** `_token` (`:273`, `:327`), `_homeController` (`:373`), `_globals` (`:381`), `_queue`,
`_jobsRunning`, `_tlpTemplate`, `_tlpStates` — never to the parameters. **No relocated member captures a
primary-constructor parameter**, so the C# scoping question does not arise for this split.

For completeness: whether a primary-constructor parameter is in scope in a *different* partial part is
**not verified** by this research. I have no compiler and did not find a repository precedent. The
recommended split is chosen so that the question never has to be answered. If a future change wants to
move a field initializer such as `:32`, that question must be settled first.

`#region` structure: both relocated blocks are complete `#region` … `#endregion` pairs, so the regions
move intact and no region is left unbalanced on either side. The load-bearing comments at `QfcQueue.cs:41`
(move-monitor invariant), `:213` (`EnqueueAsync` lives in the Enqueue part), `:255` (commented-out
`_templateViewer`), and `:317-318` (`LoadControllersViewersAsync` lives in the Enqueue part) all sit
*outside* the two relocated regions except `:255` and `:317-318`, which travel with the
`Tlp Manipulation` region and must be carried verbatim.

### Recommended split — exact files and members

**New file 1: `QuickFiler/Controllers/QfcQueue.Tlp.cs`**

Moves the whole `#region Tlp Manipulation` block, `QfcQueue.cs:230-453` inclusive (224 lines):

| Member | Current lines |
|---|---|
| `private TableLayoutPanel _tlpTemplate;` + `public TableLayoutPanel TlpTemplate` | 232-244 |
| `internal void ActivateTlpTemplate(TableLayoutPanel tlp)` | 246-253 |
| commented-out `//private QfcFormViewer _templateViewer = new();` | 255 |
| `private TlpCellStates _tlpStates;` + `public TlpCellStates TlpStates` | 257-262 |
| `internal async Task<QfcItemGroup> AddAsync(...)` | 264-277 |
| `internal void AddViewerToTlp(...)` | 279-290 |
| `internal void AdjustTlp(...)` | 292-315 |
| the two-line `LoadControllersViewersAsync lives in …` comment | 317-318 |
| `public async Task ChangeIterationSize(...)` | 320-401 |
| `public void RenumberGroups(...)` | 403-411 |
| `public void GrowEntry(...)` | 413-451 |

Plus the three new seam members from §Q3 (`ItemViewerFactory`, `ViewerRowPlacer`, `ItemGroupFactory`,
about 22 lines with XML docs).

Required `using` directives for this part: `System`, `System.Collections.Generic`,
`System.Threading`, `System.Threading.Tasks`, `System.Windows.Forms`,
`Microsoft.Office.Interop.Outlook`, `QuickFiler.Interfaces`, `UtilitiesCS`. (`System.Drawing` is not
needed: `:301` and `:447` fully qualify `System.Drawing.Size`.)

**Predicted size:** 224 (moved) + 22 (seams) + 8 (usings) + 4 (namespace/class/braces) + 12 (XML doc
header explaining the split, mirroring `QfcQueue.Enqueue.cs:14-22`) ≈ **270 lines**.

**New file 2: `QuickFiler/Controllers/QfcQueue.UiIdle.cs`**

Moves the whole `#region Helper Methods` block, `QfcQueue.cs:472-505` inclusive (34 lines): the three
UI marshalling members, converted to one-line forwards, plus the `UiIdleDispatcher` seam property from
§Q4 (about 12 lines with its XML doc).

Required `using` directives: `System`, `System.Threading.Tasks`, `QuickFiler.Interfaces`, `UtilitiesCS`.

**Predicted size:** 34 → about 22 after the bodies become forwards, + 12 (seam) + 4 (usings) +
4 (namespace/class/braces) + 10 (XML doc header) ≈ **52 lines**.

**Modified: `QuickFiler/Controllers/QfcQueue.cs`**

Retains: usings (1-16), class declaration and logger (18-28), `#region Constructors and Private Members`
(30-44) plus the `MoveMonitor` seam from §Q2 (about 11 lines with its XML doc),
`#region Queue Functions` (46-228), the `EnqueueAsync lives in …` comment (213 — inside that region),
and `#region INotify` (455-470).

**Predicted size:** 507 − 224 − 34 + 11 ≈ **260 lines**.

**Unchanged: `QuickFiler/Controllers/QfcQueue.Enqueue.cs`** — 200 lines, plus at most a few lines if
`:177` gains an explanatory comment for the `ItemGroupFactory` substitution. Under 210.

**New file 3: `QuickFiler/Interfaces/IUiIdleDispatcher.cs`** ≈ 40 lines.
**New file 4: `QuickFiler/Helper Classes/UiThreadIdleDispatcher.cs`** ≈ 65 lines.

Every predicted file is well under 500, with the largest (`QfcQueue.Tlp.cs`, ≈270) at 54% of budget.

### csproj: explicit `<Compile Include>` is required — evidence

`QuickFiler/QuickFiler.csproj` is a **legacy, non-SDK project**. Line 2:

```xml
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
```

There is no `Sdk=` attribute and therefore no implicit `**/*.cs` glob. The existing parts are listed
explicitly at `QuickFiler/QuickFiler.csproj:348-349`:

```xml
    <Compile Include="Controllers\QfcQueue.cs" />
    <Compile Include="Controllers\QfcQueue.Enqueue.cs" />
```

and `ItemViewerQueue` at `:357`. **All four new production files must be added as `<Compile Include>`
items**, or they will not compile and their absence will present as "the seam property does not exist"
rather than as a missing-file error.

`QuickFiler.Test/QuickFiler.Test.csproj` is likewise non-SDK and enumerates the three existing test
files at `:119`, `:120`, `:215`. **Any new test file must also be added there.**

CSharpier will not disturb these edits: per `CLAUDE.md` § C#1 item 1, `*.csproj` is kept out of the
check by `.csharpierignore` (the file exists at the repository root; I confirmed its presence but did
not read its contents).

### Nullable directive

`QuickFiler/Controllers/QfcQueue.cs` has no `#nullable enable` (line 1 is `using System;`). Per
`CLAUDE.md` § C#1 item 3, nullable enforcement is per-file opt-in. **Do not add `#nullable enable` to
the relocated parts**: it would conscript verbatim-moved existing code into `CS86xx`-as-error under the
step-3 gate for no benefit, and the move must stay verbatim to be reviewable. The two brand-new files
(`IUiIdleDispatcher.cs`, `UiThreadIdleDispatcher.cs`) are new code and may opt in, following
`UtilitiesCS/Threading/IUiDispatcher.cs:1` and `WpfUiDispatcher.cs:1`, which both begin with
`#nullable enable`.

---

## 7. Q6 — The existing seam pattern to copy

### `QfcQueue.ItemControllerFactory`

`QuickFiler/Controllers/QfcQueue.Enqueue.cs:25-56`. The doc comment at `:26-27` names the pattern
explicitly: "Issue #678 injectable-delegate seam (form 2 of `.claude/rules/csharp.md`, mirroring the
existing `QfcDatamodel.ScoringServiceFactory` pattern)". The declaration:

```csharp
        internal Func<
            IApplicationGlobals,
            IFilerHomeController,
            IQfcCollectionController,
            IItemViewer,
            int,
            int,
            MailItem,
            TlpCellStates,
            IFolderSearchHandler,
            IQfcItemController
        > ItemControllerFactory { get; set; } =
            (globals, home, parent, viewer, position, digits, mail, tlpStates, carriedHandler) =>
                new QfcItemController(
                    appGlobals: globals,
                    homeController: home,
                    parent: parent,
                    itemViewer: viewer,
                    viewerPosition: position,
                    itemNumberDigits: digits,
                    mail,
                    tlpStates,
                    carriedFolderHandler: carriedHandler
                );
```

Shape characteristics to copy: `internal`, auto-property with `get; set;`, a **non-null production
default** that reproduces the previous construction expression argument-for-argument, and a narrowed
parameter type where possible (the doc at `:31` and the test at `QfcQueuePurePathsTests.cs:353-357`
record that `IItemViewer` was chosen over the concrete `ItemViewer` precisely so a Moq double suffices).

### `.claude/rules/csharp.md` DI-seam guidance, verbatim

`.claude/rules/csharp.md:49-53` (quoted in full in §Q4 above). "Form 2" is:

> 2. **Injectable delegate seam** — use a narrow `Func<>`/`Action<>` delegate for a single call path when a full interface is excessive. Default behavior must remain safe and deterministic.

### `QfcDatamodel.ScoringServiceFactory` — same shape, confirmed

`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:376-383`:

```csharp
        /// Injectable factory for the master-queue admission scorer. Defaults to a fresh
        /// <see cref="FolderScoringService"/> so production behaviour is unchanged; tests assign a
        /// factory returning a mock so <see cref="ScoreRemainingQueueMailItemAsync"/> can be driven
        /// without a live Outlook session, which
        /// <c>.claude/rules/general-unit-test.md</c> UT4 requires.
        /// </summary>
        internal Func<IFolderScoringService> ScoringServiceFactory { get; set; } =
            () => new FolderScoringService();
```

consumed at `:391` (`var scoringService = ScoringServiceFactory();`). **Yes — identical shape**:
`internal`, auto-property, `Func<>`, non-null production default constructing the real collaborator.
Test consumption at `QfcQueuePurePathsTests.cs:178` and `:247` (`model.ScoringServiceFactory = () =>
scoringService.Object;`).

**Where the new seams must deviate, and why:** `ScoringServiceFactory` and `ItemControllerFactory` both
have `this`-free defaults, so the initializer form works. The `AddAsync` and `AddViewerToTlp` seams do
not (§Q3), and the UI seam is generic (§Q4). Those two deviations are forced by the language and should
be documented in-code so a reviewer does not read them as gratuitous inconsistency.

---

## 8. Q7 — Branch-by-branch reachability after the proposed seams

Notation: **N** = reachable with no new seam; **M** = needs the `MoveMonitor` seam (§Q2);
**U** = needs the `UiIdleDispatcher` seam (§Q4); **G** = needs the `ItemGroupFactory` /
`ItemViewerFactory` / `ViewerRowPlacer` seams (§Q3); **T** = needs a template-clone seam (new, §8.1).

### `EnqueueAsync` (`QuickFiler/Controllers/QfcQueue.Enqueue.cs:71-138`)

| # | Branch / statement | Lines | Seam | Reachable after? | Assertion |
|---|---|---|---|---|---|
| 1 | `items is null` → `throw new ArgumentNullException` | 79-82 | **N** | Yes, today | `.Should().ThrowAsync<ArgumentNullException>()` |
| 2 | `items.Count == 0` → `throw new ArgumentException("items is empty")` | 83-86 | **N** | Yes, today | `.Should().ThrowAsync<ArgumentException>()` |
| 3 | guards fall through; `_qfcCollectionController = qfcCollectionController` | 88 | M,U,T,G | Yes | read back via reflection |
| 4 | `await Task.Run(() => items.ForEach(item => _moveMonitor.HookItem(...)))` | 90-92 | **M** | Yes | `moveMonitor.Verify(x => x.HookItem(item, It.IsAny<Action<MailItem>>()), Times.Once)` |
| 5 | `Interlocked.Increment(ref _jobsRunning)` | 94 | M,U,T,G | Yes, but only observable mid-flight | have a seam callback capture `queue.JobsRunning` at invocation time |
| 6 | `await UiIdleCallAsync(() => _tlpTemplate.Clone(name: "BackgroundTableLayout"))` | 97-99 | **U + T** | Yes only with T — see §8.1 | assert the returned tlp is the one the factory produced |
| 7 | happy path: `UiIdleAsyncCallAsync(... LoadControllersViewersAsync ...)`; `_queue.Add((tlp, itemGroups))` | 105-116 | U,G | Yes | `queue.Count.Should().Be(1)` then `Dequeue()` |
| 8 | `catch (OperationCanceledException)` (empty body) | 118-121 | U,G | Yes — make `ItemGroupFactory` throw `OperationCanceledException` | `.Should().NotThrowAsync()` plus `queue.Count == 0` |
| 9 | `catch (System.Exception e)` → `logger.Error(...)` | 122-127 | U,G | Yes — make `ItemGroupFactory` throw `InvalidOperationException` | `.Should().NotThrowAsync()` plus `queue.Count == 0` |
| 10 | `finally` → `Interlocked.Decrement(ref _jobsRunning)` | 128-131 | M,U,T,G | Yes | `queue.JobsRunning.Should().Be(0)` after each of 7/8/9 |
| 11 | `finally` → `CollectionChanged?.Invoke(..., NotifyCollectionChangedAction.Add, _queue)`, subscriber present | 133-136 | M,U,T,G | Yes | subscribe and capture `NotifyCollectionChangedEventArgs`, assert `Action == Add`, exactly as `QfcQueueCoverageExpansionTests.cs:120,126-127` already does for `Remove` |
| 12 | same line, **no** subscriber (the null-conditional's other arm) | 133 | M,U,T,G | Yes — run one case without subscribing | absence of throw |

`logger` is a real `log4net.ILog` static (`QfcQueue.cs:26-28`). With no appender configured in the test
host, `logger.Error` is a no-op; no test infrastructure is needed for branch 9. The repository has
precedent: `QuickFiler.Test` has no log4net configuration and existing tests drive `logger.Error`
paths in `TryDequeueAsync` (`QfcQueue.cs:155-160`).

### `LoadControllersViewersAsync` (`QuickFiler/Controllers/QfcQueue.Enqueue.cs:154-198`)

| # | Branch / statement | Lines | Seam | Reachable after? | Assertion |
|---|---|---|---|---|---|
| 13 | `digits = start + items.Count >= 10 ? 2 : 1` — the `2` arm | 166 | G | Yes (`start=0`, 10 items, or `start=9`, 1 item) | captured `digits` argument on `ItemControllerFactory` |
| 14 | same, the `1` arm | 166 | G | Yes (`start=0`, 1 item) | as above |
| 15 | `Enumerable.Range(start, items.Count) … SelectAwait(async i => (i, grp: await AddAsync(tlp, items[i - start], i)))` — now `ItemGroupFactory` | 174-177 | G | Yes | capture `(tlp, mailItem, index)` per invocation; assert the `i - start` index mapping with a non-zero `start` |
| 16 | `x.grp.CarriedFolderHandler = ResolveCarriedHandler(preScored, x.grp.MailItem)` — carrier found | 180 | G | Yes | assert `CarriedFolderHandler` on the returned group |
| 17 | same — carrier absent / `preScored` null | 180 | G | Yes | assert null |
| 18 | `x.grp.ItemController = ItemControllerFactory(...)` — 9-argument pass-through | 181-191 | G | Yes | capture all nine arguments |
| 19 | `await x.grp.ItemController.InitializeAsync()` | 192 | G | Yes | `Mock<IQfcItemController>` with `Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask)` and `Verify(..., Times.Once)` |
| 20 | `return x.grp` / `.ToListAsync()` / `return itemTasks` | 193-197 | G | Yes | returned list order and count |

`IQfcItemController.InitializeAsync()` is declared `Task InitializeAsync();` at
`QuickFiler/Interfaces/IQfcItemController.cs:23`, with `ItemNumber` and `ItemNumberDigits` at `:45`
and `:47` — all mockable, and `QfcQueueCoverageExpansionTests.cs:53-59` already builds such a mock.

### 8.1 Branches that are STILL UNREACHABLE — the most important part

**(A) `_tlpTemplate.Clone(name: "BackgroundTableLayout")` (`Enqueue.cs:98`) — unreachable with only the
Q2–Q4 seams. Recommend a fourth seam.**

`Clone` here resolves to `UtilitiesCS/Extensions/WinFormsExtensions.cs:272-278`:

```csharp
        public static T Clone<T>(this T controlToClone, string name, bool deep = false)
            where T : Control
        {
            T instance = controlToClone.Clone<T>(deep);
            instance.Name = name;
            return instance;
        }
```

which delegates to `:280-295`, a **reflection-driven copy of every public instance property** of the
`Control` (`:283-285`, `:289-292`), excluding only `WindowTarget`, `Name`, and `Parent`. Whether that
succeeds against a headless `TableLayoutPanel` — it enumerates and copies properties such as `Handle`,
`Region`, `Site`, `BindingContext` — is **not verified** by this research and cannot be verified without
running it. It is the single highest-risk statement on the path. Note also that `set_TlpTemplate`
(`QfcQueue.cs:236-243`) performs the *same* reflection clone and is reported at 0% coverage
(`coverage-postchange.cobertura.xml:41947`), so there is no existing evidence that this clone runs
outside a live host.

Recommended fourth seam, on the `QfcQueue.Tlp.cs` part (the default is `this`-free, so the plain
initializer form works — it is form 2 exactly like `ItemControllerFactory`):

```csharp
        internal Func<TableLayoutPanel, TableLayoutPanel> BackgroundTlpFactory { get; set; } =
            template => template.Clone(name: "BackgroundTableLayout");
```

with `Enqueue.cs:97-99` becoming `await UiIdleCallAsync(() => BackgroundTlpFactory(_tlpTemplate))`.
This is the only way to make branches 3, 5, 7-12 reachable **deterministically**. Without it, the whole
`EnqueueAsync` body past line 97 is gated on an unproven reflection clone, and the test would also have
to inject a real `TableLayoutPanel` into `_tlpTemplate`. Note that `Clone` is invoked on `_tlpTemplate`,
so a null template throws `NullReferenceException` at `:98` — see (C).

**(B) `AddAsync`'s own body (`QfcQueue.cs:272-276`) — unreachable if only the coarse `ItemGroupFactory`
seam is adopted.** A test that substitutes `ItemGroupFactory` never executes the production default,
so `AddAsync` stays at 0%. The finer `ItemViewerFactory` + `ViewerRowPlacer` pair in §Q3 closes it. If
the plan adopts only the coarse seam, `AddAsync` must be recorded as an accepted residual and
`spec.md` must say so, because otherwise the item ships claiming to have removed an untestable region
that it has merely moved.

The alternative route — driving the real `AddAsync` through `ItemViewerQueue.SetCoreForTesting`
(`ItemViewerQueue.cs:69`) with a `CreateUninitialized<ItemViewer>()` fake — is **likely to fail**:
`AddViewerToTlp` (`QfcQueue.cs:283-289`) sets `viewer.Parent`, `viewer.AutoSize`,
`viewer.AutoSizeMode`, `viewer.BorderStyle`, `viewer.Dock` on the fake, and an object produced by
`GetUninitializedObject` has none of `Control`'s internal state initialized. Whether each setter
tolerates that is **not verified**. It would also import `[DoNotParallelize]` plus the global static
reset obligation (`ViewerQueueStaticWrapperTests.cs:11`, `:15-22`) into the `QfcQueue` test class.
Recommend against this route.

**(C) A latent hang defect, out of scope, recommend a follow-up issue.** In
`QfcQueue.Enqueue.cs`, `Interlocked.Increment(ref _jobsRunning)` is at line **94** and the `try` whose
`finally` decrements it opens at line **103**. Lines 90-92 (the `Task.Run` hook loop, which can throw
out of `_moveMonitor.HookItem` — `EmailMoveMonitor.cs:58-59` dereferences `mail.Parent` and
`folder.EntryID`) and lines 97-99 (the template clone, which throws `NullReferenceException` on a null
`_tlpTemplate`) are **outside** that `try`. A throw at line 97-99 therefore leaves `_jobsRunning`
permanently at a non-zero value for the lifetime of the instance. The consequences are concrete and
observable:

- `CompleteAddingAsync` (`QfcQueue.cs:56`) loops on `while (_jobsRunning > 0)` until the caller-supplied
  timeout, then throws `OperationCanceledException`.
- `TryDequeueAsync` (`QfcQueue.cs:117`) keeps `_queue.Count + _jobsRunning > 0` true forever.
- `JobsToFinish` (`QfcQueue.cs:217`) never returns, and `ChangeIterationSize` (`:327`) and `RemoveItem`
  (`:177`) both await it.

A test with a throwing `BackgroundTlpFactory` would *prove* the leak, i.e. it would be a failing test
against current production behaviour. Fixing it means widening the `try` to enclose lines 90-99, which
is a behaviour change outside this item's stated scope (`spec.md:120`, "No unintended behavior changes
outside the defined scope"). **Recommendation: do not fix it here; promote it to its own issue through
the normal promotion lifecycle and reference it from `spec.md`.** Do not write a test that asserts the
leaking behaviour as correct — that would codify the bug, the failure mode recorded for
`FolderConverterTests.cs:329` in prior research.

**(D) `ActivateTlpTemplate` (`QfcQueue.cs:246-253`) has an entirely commented-out body.** It is called
from nowhere on this path (the only call site, `Enqueue.cs:101`, is itself commented out). It is dead
code. Out of scope, but note that relocating it to `QfcQueue.Tlp.cs` moves dead code rather than
deleting it; deletion would be a separate, larger decision.

---

## 9. Q8 — Toolchain and coverage reality

### Test project and assembly path

- Project: `QuickFiler.Test/QuickFiler.Test.csproj`.
- `<AssemblyName>QuickFiler.Test</AssemblyName>` (`QuickFiler.Test.csproj:17`).
- `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` (`:18`).
- Debug `<OutputPath>bin\Debug\</OutputPath>` (`:36`).
- Therefore the assembly a `vstest.console.exe` run consumes is
  **`QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`**.
- There is a stale `QuickFiler.Test/QuickFiler.Test.csproj.bak` pinned to v4.7.2 (`:14`); it is not the
  build input. Do not edit it.

CI's discovery and invocation, `.github/workflows/_mstest-coverage.yml:86-99`: it globs `*.Test.dll`
under `\bin\Debug\`, excluding `\obj\` and `\ref\`, and runs

```
& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"
```

`/InIsolation` and the `TestCategory!=LiveOutlook` filter are both load-bearing for local parity.

### Coverage thresholds — the two figures disagree, and CI enforces neither

| Source | Line | Branch | New code |
|---|---|---|---|
| `CLAUDE.md` § UT2 ("Comprehensive Coverage (within reason)") | **>= 80%** repository-wide | not stated | **>= 90%** for new modules/classes/methods |
| `.claude/rules/general-unit-test.md` § "Coverage Requirements" | **>= 85%** across T1-T4 | **>= 75%** across T1-T4 | not stated |
| `.claude/rules/quality-tiers.md` § "Uniform across all tiers" | **>= 85%** | **>= 75%** | not stated |
| `.github/workflows/_mstest-coverage.yml` | **none** | **none** | **none** |

`_mstest-coverage.yml` has exactly five steps after checkout (setup, restore, build, run, upload). The
final step (`:104-112`) uploads `TestResults/**/*.trx` and `TestResults/**/*.coverage` as an artifact.
**There is no step that parses coverage, compares it to a threshold, or fails the job on coverage.** The
only way that job fails on tests is the non-zero exit code check at `:100-102`. So **no numeric coverage
gate is enforced by CI.**

On which written figure governs: `CLAUDE.md` § "Policy Compliance Order" lists the authoritative order
as (1) CLAUDE.md, (2) General Code Change Policy, (3) General Unit Test Policy, (4) the C# policies —
all four of which are embedded in `CLAUDE.md` itself. The `.claude/rules/*.md` files are not named in
that order. The governing figures for this item are therefore **>= 80% repository-wide and >= 90% for
new code**, which is also what `spec.md:36` already assumes ("Both members reach the 90% new-code
floor as written"). Report the 85/75 figures as the conflicting second source, and do not silently
adopt them.

### Tier classification

**`quality-tiers.yml` does not exist at the repository root.** A Glob for `quality-tiers*` across the
whole worktree returns exactly one file: `.claude/rules/quality-tiers.md`. That document states
(`quality-tiers.md` § "Source of Truth") that "`quality-tiers.yml` at repo root maps every project to
one tier" and that "Adding a project without a tier classification fails CI" — neither of which is true
of this repository as it stands. **QuickFiler has no tier classification, and no CI stage validates
one.** Any plan step that says "confirm the QuickFiler tier" cannot be satisfied and should be dropped
or reworded.

### Coverage baseline for the changed files

The most recent committed Cobertura in the tree covering these files is
`docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/coverage-postchange.cobertura.xml`
(duplicated as `.../evidence/baseline/coverage-baseline.cobertura.xml`):

- `:41897` — `QuickFiler\Controllers\QfcQueue.cs`, `line-rate="0.503205"`, `branch-rate="0.555556"`,
  `complexity="79"`.
- `:42890` — `QuickFiler\Controllers\QfcQueue.Enqueue.cs`, `line-rate="0.152941"`,
  `branch-rate="0.25"`, `complexity="21"`.

**This corrects the spec.** `spec.md:11` says `EnqueueAsync` is "0 of 46 lines covered" and
`LoadControllersViewersAsync` "0 of 24". At *member* level the zero is right, but at *file* level
`QfcQueue.Enqueue.cs` is at 15.3%, not 0%, because three members are covered by the #678 tests:

- `:42892` — `.ctor` signature `(CancellationToken, QfcHomeController, IApplicationGlobals)`,
  `line-rate="1"`, covering line 45 (the `ItemControllerFactory` initializer).
- `:42901` — `ResolveCarriedHandler`, `line-rate="1"`, covering line 152.
- `:42927` — `<.ctor>b__0_0` (the `ItemControllerFactory` default lambda), `line-rate="1"`, covering
  lines 46-56.

That is 13 covered lines against a file denominator of 85 (13/85 = 0.15294, matching the reported rate
exactly). The uncovered members are:

- `:42910` — `LoadControllersViewersAsync`, 9 lines (163, 166, 174, 175, 176, 178, 195, 197, 198), all
  `hits="0"`; line 166 is the `digits` ternary with `condition-coverage="0% (0/2)"`.
- `:42965` — the `EnqueueAsync` async state machine `MoveNext`, 36 lines (76 through 138), all
  `hits="0"`; four branch points at 79, 83, 133 and one at 91.
- `:42942`, `:42951`, `:42960` — the `<EnqueueAsync>b__0`, `b__1`, `b__3` lambdas, lines 91 and 98.
- `:43016`, `:43021`, `:43034`, `:43039` — four nested lambda state machines covering lines 91,
  106-114, 177, and 179-194.

Net: **72 uncovered lines in `QfcQueue.Enqueue.cs`** (85 total minus 13 covered), which is the honest
figure to put in the spec in place of "0 of 46" and "0 of 24".

`AddAsync`, `UiIdleCallAsync`, and `UiIdleAsyncCallAsync` do **not** appear as named `<method>` elements
in the `QfcQueue.cs` class block — a Grep for `name="AddAsync"`, `name="UiIdleCallAsync"`, and
`name="UiIdleAsyncCallAsync"` in that file returns nothing, while `name="set_TlpTemplate"` (`:41947`)
and `name="AddViewerToTlp"` (`:41971`) are present and both at `line-rate="0"`. This is the expected
Cobertura behaviour for `async` members: they compile to compiler-generated `d__` state machines whose
entry point is named `MoveNext`, so their lines are attributed to one of the many `MoveNext` elements
rather than to a method bearing the source name. Do not read the absence of an `AddAsync` element as an
exemption signal.

---

## 10. Recommended approach (single recommendation)

Adopt **five seams on `QfcQueue`, one new narrow interface with one production adapter, and a two-file
split**, in this shape:

| # | Seam | Declared on | Form | Default | Why this form |
|---|---|---|---|---|---|
| S1 | `internal IEmailMoveMonitor MoveMonitor { get; set; }` | `QfcQueue.cs` (base part) | property over the retained `_moveMonitor` field | existing field initializer, unchanged | Constructor form is CS0051-illegal; retaining `_moveMonitor` keeps three existing tests green; instance-scoped default preserves the #731/#620 invariant |
| S2 | `internal IUiIdleDispatcher UiIdleDispatcher { get; set; }` | `QfcQueue.UiIdle.cs` | lazy `??=` property, interface seam | `new UiThreadIdleDispatcher()` | Generic method shapes cannot be held in a delegate field; the existing `IUiDispatcher` drops `ContextIdle`; lazy default keeps the constructor headless-safe |
| S3 | `internal Func<CancellationToken, ItemViewer> ItemViewerFactory { get; set; }` | `QfcQueue.Tlp.cs` | plain initializer (form 2) | `ItemViewerQueue.Dequeue` method group | Default is `this`-free, so the `ItemControllerFactory` initializer form works; avoids importing `ItemViewerQueue`'s global static state into the test class |
| S4 | `internal Action<TableLayoutPanel, ItemViewer, int> ViewerRowPlacer { get; set; }` | `QfcQueue.Tlp.cs` | lazy `??=` property | `AddViewerToTlp` method group | Instance-method default cannot appear in an initializer |
| S5 | `internal Func<TableLayoutPanel, MailItem, int, Task<QfcItemGroup>> ItemGroupFactory { get; set; }` | `QfcQueue.Tlp.cs` | lazy `??=` property | `AddAsync` method group | Same; lets `LoadControllersViewersAsync` be exercised without `AddAsync` |
| S6 | `internal Func<TableLayoutPanel, TableLayoutPanel> BackgroundTlpFactory { get; set; }` | `QfcQueue.Tlp.cs` | plain initializer (form 2) | `template => template.Clone(name: "BackgroundTableLayout")` | Removes the only remaining hard blocker on `EnqueueAsync` lines 97-138 (§8.1 A) |

New production files: `QuickFiler/Interfaces/IUiIdleDispatcher.cs`,
`QuickFiler/Helper Classes/UiThreadIdleDispatcher.cs`,
`QuickFiler/Controllers/QfcQueue.Tlp.cs`, `QuickFiler/Controllers/QfcQueue.UiIdle.cs` — **all four need
`<Compile Include>` entries in `QuickFiler/QuickFiler.csproj`.**

### Rejected alternatives (brief)

- **Coverage exemption via `[ExcludeFromCodeCoverage]`.** Excluded by the maintainer decision on #727
  sub-finding 4 dated 2026-09-11 and by `.claude/rules/general-unit-test.md` § "Coverage Exclusion
  Policy" ("No production file may be excluded from coverage measurement"). `CLAUDE.md` § UT2's
  COM/VSTO exemption explicitly does not cover "testable seams within otherwise-COM-bound assemblies".
- **Extending `UtilitiesCS.Threading.IUiDispatcher` with `ContextIdle` overloads.** Ten implementers
  across three test assemblies plus one production adapter must all be edited on a framework with no
  default interface members. Disproportionate for one caller. (§Q4)
- **Constructor-parameter seam for the move monitor.** CS0051 (internal parameter type on a public
  constructor), and a primary constructor has no body in which to apply a default. (§Q2)
- **Driving the real `AddAsync` through `ItemViewerQueue.SetCoreForTesting`.** Imports process-global
  mutable static state and `[DoNotParallelize]`, and depends on an uninitialized `ItemViewer`
  tolerating five `Control` property setters — unverified and likely to throw. (§8.1 B)
- **Converting `_moveMonitor` to an auto-property.** Breaks three existing tests that reflect on the
  `_moveMonitor` field name. (§Q2)
- **A single coarse `ItemGroupFactory` seam only.** Relocates the uncovered region into `AddAsync`
  rather than closing it. (§8.1 B)

---

## 11. Test strategy (no test code written)

New test file: **`QuickFiler.Test/Controllers/QfcQueueEnqueueSeamTests.cs`**, registered in
`QuickFiler.Test/QuickFiler.Test.csproj`. A new file rather than an addition to an existing one because
`QfcQueuePurePathsTests.cs` is at 418 of 500 lines and `QfcQueueCoverageExpansionTests.cs` at 290; the
suite described below will not fit in the former's 82-line headroom, and it is a distinct concern.
If the file approaches 500 lines, split it as
`QfcQueueEnqueueSeamTests.cs` + `QfcQueueEnqueueSeamTests.Part2.cs` declaring
`public partial class QfcQueueEnqueueSeamTests`, with `[TestClass]` on the base file only (repeating it
is CS0579).

Framework and libraries, per `CLAUDE.md` § CUT1/CUT2: **MSTest** attributes, **Moq** for
`IApplicationGlobals`, `MailItem`, `IEmailMoveMonitor`, `IQfcItemController`, `IQfcCollectionController`,
`IFolderSearchHandler`; **FluentAssertions** for every assertion.

Shared arrange helpers to add to the new file:
- `NewQueue()` mirroring `QfcQueueCoverageExpansionTests.cs:25-29` verbatim.
- `InlineUiIdleDispatcher` — the hand-written synchronous `IUiIdleDispatcher` fake from §Q4, following
  the nine existing hand-written `IUiDispatcher` fakes.
- A recording `ItemGroupFactory` that captures `(tlp, mailItem, index)` tuples and returns
  `new QfcItemGroup(mailItem)` with `ItemViewer` left null.
- A recording `ItemControllerFactory` that captures all nine arguments and returns a
  `Mock<IQfcItemController>` whose `InitializeAsync()` returns `Task.CompletedTask`.
- A `BackgroundTlpFactory` returning a plain `new TableLayoutPanel()` (or a sentinel the test can
  identify by reference), so no reflection clone runs.

Scenario coverage, mapped to `.claude/rules/general-unit-test.md` § "Scenario Completeness":

- **Negative / invalid input:** rows 1 and 2 of §Q7 — null `items`, empty `items`. These need no seam
  and should be written first as the cheapest proof that the test class is wired correctly.
- **Positive flow:** row 7 — one page enqueued, `Count == 1`, the dequeued tuple carries the factory's
  tlp and the expected groups in order.
- **Boundary:** rows 13 and 14 — `digits` at `start + items.Count` equal to 9, 10, and 11, asserting
  1, 2, 2 respectively. Include a non-zero `start` case to pin the `items[i - start]` index mapping
  (row 15), which is the kind of off-by-one a refactor would silently break.
- **Error handling:** rows 8 and 9 — `OperationCanceledException` and a general exception raised from
  the `ItemGroupFactory`, each asserting that `EnqueueAsync` does not propagate, that `Count` stays 0,
  and that `JobsRunning` returns to 0.
- **State transitions:** rows 5, 10, 11, 12 — capture `JobsRunning` from inside a seam callback to prove
  the increment, assert it is 0 after every one of the three outcomes to prove the `finally` decrement,
  and assert one `CollectionChanged` event with `Action == Add` in each of the three outcomes. Include
  one case with no subscriber to cover the null-conditional's other arm.
- **Boundary collaboration:** row 4 — `Mock<IEmailMoveMonitor>(MockBehavior.Strict)` verifying
  `HookItem` once per item. Strict mode is the established convention here
  (`QfcQueueCoverageExpansionTests.cs:113`, `QfcQueuePurePathsTests.cs:120`).
- **Seam defaults:** one test per seam asserting the production default is non-null on first read,
  mirroring `QfcQueuePurePathsTests.cs:360-368`'s treatment of `ItemControllerFactory`. For `S3`
  additionally assert the default is the `ItemViewerQueue.Dequeue` method group by comparing
  `Method.Name`/`Method.DeclaringType`, without invoking it (invoking it would touch
  `UiThread.Dispatcher`).
- **`AddAsync` proper:** with `ItemGroupFactory` left at its default and `S3`/`S4` substituted, call
  `AddAsync` directly and assert the returned group carries the supplied `MailItem`, that
  `ItemViewerFactory` received `_token`, and that `ViewerRowPlacer` received `(tlp, viewer, index)`.

Determinism, per `.claude/rules/general-unit-test.md` § "Determinism Infrastructure": no
`Thread.Sleep`, no `Task.Delay`, no real wall-clock wait anywhere in the new file. The
`InlineUiIdleDispatcher` removes the only asynchrony that would otherwise need a timer.
`await Task.Run(...)` at `Enqueue.cs:90` is awaited by production code, so it needs no fake timer.
`FakeTimeProvider` is available in the project (`QfcQueuePurePathsTests.cs:9`, `:209`) but is not needed
by any scenario above — `QfcQueue` on this path reads no clock.

No temporary files, no filesystem, no network, no Outlook process — satisfying UT4 and
`.claude/rules/general-code-change.md` § "I/O Boundaries".

Toolchain, per `CLAUDE.md` § CUT3, run in order and restarted from step 1 on any failure or auto-fix:

1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`

Do not add `/p:Nullable=enable` to step 3 and do not substitute `/t:Build` for `/t:Rebuild` in steps 2
or 3 — `CLAUDE.md` § C#1 items 2 and 3 record why both substitutions break the gate.

---

## 12. Open questions and explicitly-unknown items

1. **Does `WinFormsExtensions.Clone<T>` succeed against a headless `TableLayoutPanel`?** Unknown; not
   verifiable without executing it. The `BackgroundTlpFactory` seam (S6) makes the answer irrelevant to
   the test suite, which is why it is recommended. It remains relevant to whether
   `set_TlpTemplate` (`QfcQueue.cs:236-243`) can ever be covered — out of scope for this item.
2. **Are primary-constructor parameters in scope in other partial parts of the same class?** Unknown;
   not verified, and no in-repo precedent found. The recommended split avoids needing an answer,
   because no relocated member references a parameter (§Q5).
3. **Are `QfcHomeController`'s members virtual enough to be Moq-mockable?** Not enumerated. Not needed:
   `null` and the cheap real constructor both work (§Q1).
4. **Will an uninitialized `ItemViewer` tolerate `AddViewerToTlp`'s five property setters?** Unknown;
   the recommendation avoids the question by seaming `AddViewerToTlp` itself (S4).
5. **Exact post-change line counts.** The counts in §Q5 are arithmetic predictions from the measured
   pre-change counts plus estimated new content. They must be re-measured after the edit, before the
   file-size acceptance criterion is checked off.

## Numeric Derivation Evidence

This section restates the derivation of section 1 in the single-line, unemphasised label form that
`.claude/hooks/validate-prd-feature-output.ps1` parses. Section 1 above is the human-readable
narrative; this section is the machine-checkable record of the same enumeration, re-run by the
orchestrator against base commit 2405a829d on 2026-09-12.

Complete Family: QfcQueue.cs, QfcQueue.Enqueue.cs, QfcQueueTests.cs, QfcQueuePurePathsTests.cs, QfcQueueCoverageExpansionTests.cs
Exhaustive Search Scope: the entire tracked source tree of the repository at base commit 2405a829d, enumerated in full with no path filter applied before matching.
Inclusion Rules: the path is tracked by git in that checkout, its basename begins with the characters QfcQueue, and its extension is .cs, so both production parts of the partial class and every test file addressing it are admitted.
Exclusion Rules: untracked and ignored paths, build output under bin and obj, any .bak or .orig sibling because no project compiles one, and every non-.cs artifact such as documentation, coverage XML and evidence mirrors that merely mentions the type name.
Primary Search Strategy or Query Expression: enumerate every path tracked by git across the whole checkout, which returned 15435 entries, then retain each entry whose basename begins with QfcQueue and whose extension is .cs; the retained entries are QuickFiler/Controllers/QfcQueue.cs, QuickFiler/Controllers/QfcQueue.Enqueue.cs, QuickFiler.Test/Controllers/QfcQueueTests.cs, QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs and QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs.
Primary Member Set: QfcQueue.cs, QfcQueue.Enqueue.cs, QfcQueueTests.cs, QfcQueuePurePathsTests.cs, QfcQueueCoverageExpansionTests.cs
Primary Count: 5
Cross-check Search Strategy or Query Expression: read the two legacy project manifests instead of the index, collect every Compile Include item they declare, and keep the items whose file name begins with QfcQueue; QuickFiler/QuickFiler.csproj contributes Controllers\QfcQueue.cs at line 348 and Controllers\QfcQueue.Enqueue.cs at line 349, and QuickFiler.Test/QuickFiler.Test.csproj contributes Controllers\QfcQueueCoverageExpansionTests.cs at line 119, Controllers\QfcQueuePurePathsTests.cs at line 120 and Controllers\QfcQueueTests.cs at line 215.
Cross-check Member Set: QfcQueueCoverageExpansionTests.cs, QfcQueuePurePathsTests.cs, QfcQueueTests.cs, QfcQueue.Enqueue.cs, QfcQueue.cs
Cross-check Count: 5
Member-set Comparison: the two member sets are identical once each entry is reduced to its bare file name and order is disregarded, so the index-based enumeration and the project-manifest enumeration agree on the complete family; the agreement matters because the manifests are hand-maintained in these non-SDK projects, and a file present in the index but absent from a manifest would compile in neither configuration.

Two derived figures rest on that family and are used as acceptance-criteria inputs. Measured with
`git grep -c "" HEAD` against the same commit: QuickFiler/Controllers/QfcQueue.cs is 507 lines and
QuickFiler/Controllers/QfcQueue.Enqueue.cs is 200 lines. The first figure exceeds the repository's
500-line ceiling before any change is made, which is what makes the split mandatory rather than
discretionary; the issue text's competing figure of 439 is stale and must not be used.
