---
Timestamp: 2026-08-07T20-50
Feature: quickfiler-qfc-home-controller-coverage (epic child F7, issue #433)
Epic: quickfiler-per-file-coverage (parent issue #136)
Target file: QuickFiler/Interfaces/IFilerHomeController.cs
Target file (absolute): C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-afcf27830d48e5590\QuickFiler\Interfaces\IFilerHomeController.cs
Line count: 45
Worktree: C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-afcf27830d48e5590
Base commit: 74be1964
Coverage classification authority: docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md (child F1, wave 0 — verified ABSENT from disk at research time)
Coverage evidence mechanism: F1's per-file line-coverage harness derived from the Cobertura output of Invoke-MSTestWithCoverage.ps1
Research method: static read of the file, csproj inspection, committed-Cobertura inspection, repository-wide grep. No msbuild, no vstest, no coverage run performed.
---

# Research — `QuickFiler/Interfaces/IFilerHomeController.cs` (F7, issue #433)

## 0. Upstream contract consumed (F1, wave 0)

This artifact is written to **consume** F1's contract, not to substitute for it.

1. **Classification authority.** Whether this file is `testable` or `interface-only / not-measured`
   is decided by the ratified ledger at
   `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. That file does not exist at
   research time (verified: the epic directory contains only `epic.md`). This artifact recommends a
   classification and supplies the evidence for it; it does not assert one.
2. **Measurement authority.** F1's per-file line-coverage harness is the only accepted evidence
   mechanism. No substitute harness is proposed here.
3. **Scope gate.** This child's acceptance criteria (per-file >= 80% line coverage) apply **only to
   files F1 classifies `testable`.** If F1 classifies this file `interface-only / not-measured`, the
   child owes no coverage number for it — only the ledger row and the harness output that
   demonstrates the file produces no measurable lines.

---

## 1. File purpose and the contract it declares

`QuickFiler.Interfaces.IFilerHomeController` is the **shared** home-controller contract for both
filer front-ends in the QuickFiler assembly: `QfcHomeController` (Quick Filer, F7-owned) and
`EfcHomeController` (Explorer Filer, F8-owned). It is a pure abstract interface declaration: 7
`using` directives, a namespace, an interface header with no base interface, three `#region` groups,
12 live member declarations, and 3 commented-out member declarations. There is no class, no field,
no constant, no attribute, and no member body.

| Line | Member | Kind | Region |
| --- | --- | --- | --- |
| 11 | `public interface IFilerHomeController` | interface header, no base list | — |
| 15 | `void Run();` | method | Constructors, Initializers, and Destructors |
| 16 | `Task RunAsync(ProgressTracker progress);` | method | same |
| 17 | `void Cleanup();` | method | same |
| 23 | `SynchronizationContext UiSyncContext { get; }` | read-only property | Public Properties |
| 24 | `CancellationTokenSource TokenSource { get; }` | read-only property | same |
| 25 | `CancellationToken Token { get; }` | read-only property | same |
| 26 | `bool Loaded { get; }` | read-only property | same |
| 27 | `Stopwatch StopWatch { get; }` | read-only property | same |
| **29** | `//IQfcDatamodel DataModel { get; }` | **commented out** | same |
| 30 | `IQfcExplorerController ExplorerController { get; set; }` | read/write property | same |
| 31 | `IFilerFormController FormController { get; }` | read-only property | same |
| 32 | `IQfcKeyboardHandler KeyboardHandler { get; set; }` | read/write property | same |
| 33 | `FilerQueue FilerQueue { get; }` | read-only property | same |
| **34** | `//QfcFormViewer FormViewer { get; }` | **commented out** | same |
| **40** | `//void Iterate();` | **commented out** | Major Actions |
| 41 | `void QuickFileMetrics_WRITE(string filename);` | method | Major Actions |

**Contract meaning.** This is the "lowest common denominator" surface that a filer session exposes to
its collaborators: lifecycle (`Run`/`RunAsync`/`Cleanup`), cancellation and threading state
(`UiSyncContext`, `TokenSource`, `Token`), readiness (`Loaded`), timing (`StopWatch`), the three
collaborator handles the item/collection/keyboard layers reach back through (`ExplorerController`,
`FormController`, `KeyboardHandler`), the shared queue (`FilerQueue`), and one metrics entry point.
The QuickFiler-specific extensions (`IQfcDatamodel DataModel`, `Init()`, the iteration surface,
`WriteMetricsAsync`) live on the derived `IQfcHomeController` — see the companion artifact.

The three commented-out members are load-bearing negative space; §5.4 evidences that each one is
commented out because `EfcHomeController` cannot satisfy it.

---

## 2. Executable-content analysis

### 2.1 Exhaustive check for IL-producing constructs

Every construct that could put an executable line in the coverage denominator was checked against the
full 45-line file.

| Construct checked | Present? | Evidence |
| --- | --- | --- |
| Default interface implementation (C# 8+ member body) | **No** | Every live member (15-17, 23-27, 30-33, 41) terminates in `;`. No `{ }` body and no `=>` expression body appears anywhere in the file. |
| `static` member with a body | **No** | The keyword `static` does not appear in the file. |
| Constant / field initializer | **No** | Interfaces cannot declare instance fields; no `const` declaration is present. |
| Attribute with a computed argument | **No** | The file contains no attribute of any kind — no `[ExcludeFromCodeCoverage]`, no assembly attribute, nothing in square brackets. |
| Nested type with a body | **No** | The only type declared is the interface itself. |
| Property with an accessor body | **No** | Lines 23-27 and 30-33 are `{ get; }` / `{ get; set; }` abstract accessor declarations, not bodies. |
| Static constructor / module initializer | **No** | None. |
| `#region` / `#endregion` directives (13, 19, 21, 36, 38, 43) | Present, but **not** IL-producing | Preprocessor directives emit no code and no sequence point. |
| Commented-out members (29, 34, 40) | Present, but **not** IL-producing | Comments emit nothing. They cannot be counted covered or uncovered. |

### 2.2 Target-framework evidence (default interface implementations are unavailable)

`QuickFiler/QuickFiler.csproj`:

```
13:    <TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>
14:    <LangVersion>preview</LangVersion>
```

The project targets **.NET Framework 4.8.1**. Default interface implementations (C# 8) require CLR
support that the .NET Framework runtime does not provide; Roslyn rejects them on this target
regardless of `LangVersion`. `LangVersion=preview` therefore does **not** enable a DIM in this file.

This matters more for this file than for its derived sibling, because `IFilerHomeController` is the
one place in the F7 file set where a "default implementation" would be superficially attractive: it
would appear to solve the `NotImplementedException` problem documented in §6.3. It is not available,
and §6.4 explains why it would be the wrong fix even if it were. The primary evidence remains §2.1:
the file contains no member body, so the question is moot for the file as it stands.

### 2.3 Direct Cobertura evidence (a compiled-and-instrumented artifact says zero)

The committed artifact
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
was produced by `Invoke-MSTestWithCoverage.ps1` against a build of this same source, and it
instruments QuickFiler (verified: `QuickFiler.Controllers.QfcHomeController` is present at line 21643
with `filename="QuickFiler\Controllers\QfcHomeController.cs"`).

Observed results:

| Query | Result |
| --- | --- |
| Any `<class ... filename="QuickFiler\Interfaces\IFilerHomeController.cs">` | **No match.** |
| Any `<class name="QuickFiler.Interfaces.I...">` (regex `class name="QuickFiler\.[A-Za-z.]*I[A-Z]`) | **No match** anywhere in the file. |
| Any `<class>` whose `filename` starts `QuickFiler\Interfaces\` | Exactly **one**: line 14448, `QuickFiler.Interfaces.MailItemActionsAdapter` — a concrete *class* that lives in the `Interfaces` folder. **No interface type from that folder appears at all**, even though the folder contains a dozen interface files. |
| Textual occurrences of `IFilerHomeController` in the artifact | Present only inside `signature="..."` attributes of methods belonging to *other* classes — e.g. line 22993 (`QfcItemController..ctor`), 23236 (`SaveParameters`), 27262 (`LoadControllersViewersAsync`). None is a `<class>` element. |

**Conclusion from the artifact:** the instrumentation emitted no class entry, no method entry and no
line entry for `IFilerHomeController.cs`. The file contributes **zero lines** to both the numerator
and the denominator of the per-file coverage metric. This is direct, reproducible evidence, not an
inference from source reading. The `MailItemActionsAdapter` control case is especially useful: it
proves the instrumenter *does* reach files under `QuickFiler\Interfaces\`, so the absence of the
interface files is a property of interfaces, not of the folder or of a coverage-config exclusion.

### 2.4 Answer to central questions 1 and 2

1. **Does the file contain any executable IL-producing construct?** **No.** Verified exhaustively
   against the construct list in §2.1, with the .NET Framework 4.8.1 target (§2.2) foreclosing the
   only C#-language route to one, and with a committed instrumented Cobertura artifact reporting no
   class/method/line entry for the file (§2.3).
2. **Does the file therefore have zero executable lines and qualify as a legitimate interface-only
   module?** **Yes.** It matches the `.claude/rules/general-unit-test.md` § Coverage Requirements
   carve-out verbatim — "Type-only / interface-only modules with no executable behavior may be
   omitted from coverage measurement. Examples: ... and C# interface-only files. Such modules
   legitimately report 0% executable coverage and may be excluded from measurement."

**Metadata is emitted, IL is not.** The compiler emits type metadata for the interface (that is why
the type name appears in other classes' method signatures in §2.3), and Moq emits real IL for
proxies of this interface at run time — but into the dynamic `DynamicProxyGenAssembly2` assembly,
never attributed to this source file. Neither fact places a coverable line here.

---

## 3. Recommended F1 ledger classification and rationale

**Recommended classification: `interface-only / not-measured`.**

Rationale, in the order F1's ledger should record it:

1. **Zero executable lines, evidenced two independent ways** — exhaustive source construct check
   (§2.1) and a committed instrumented Cobertura artifact with no class entry, alongside a positive
   control (`MailItemActionsAdapter`) proving the folder is instrumented (§2.3).
2. **Rule-text match** — the file is precisely the "C# interface-only file" the
   `.claude/rules/general-unit-test.md` carve-out names.
3. **The carve-out is the correct instrument, not the COM/VSTO exemption.** The epic's Shared Design
   §1 reconciliation ("refactor first, exempt only the irreducible remainder") governs files whose
   lines are *executable but hard to reach*. This file has no executable lines, so there is nothing
   to refactor and no exemption to ratify. Classifying it under the COM/VSTO exemption would be a
   category error implying a testability debt that does not exist.
4. **No `[ExcludeFromCodeCoverage]` disposition is owed.** The file carries no attribute (§2.1), so
   it is not one of the 33 attributes F1 must dispose of.
5. **Stability** — because the project targets .NET Framework 4.8.1, this file cannot acquire
   executable content through a default interface implementation (§2.2).

**F1's ledger is the authority.** This is a recommendation with evidence attached, not a decision.
If F1 instead classifies the file `testable`, every recommendation in §4 is void and this artifact
must be re-run — but note that a `testable` classification would be unsatisfiable on the present
content, because a file with a zero-line denominator has no line rate to raise; the only honest
response would be for F1 to record `0/0` and treat the target as vacuously met.

**Consequence for this child's acceptance criteria.** Issue #136 measures per-file line coverage.
Under the recommended classification, `IFilerHomeController.cs` is outside this child's >= 80%
obligation. The child still owes the ledger row and the numeric harness output that demonstrates
zero measurable lines.

---

## 4. Required work for this child

**No test work. Record the ledger classification and the numeric harness output as evidence.**

That is the complete disposition. Concretely, the atomic plan should contain, for this file, exactly
two non-test tasks:

| Task | Deliverable |
| --- | --- |
| W1 | Confirm F1's ledger row for `QuickFiler/Interfaces/IFilerHomeController.cs` reads `interface-only / not-measured` (or, if F1 chose otherwise, halt and re-run this research). Consuming the ledger is a Phase-0 read, not an edit — F1 owns `coverage-ledger.md`. |
| W2 | Run F1's per-file harness as part of the child's normal coverage run and commit the numeric per-file result — expected to be "file absent from the report" / zero measurable lines — to `<FEATURE>/evidence/qa-gates/`. |

**No production edit to this file is required** (see §6 for the blast-radius argument, and §7 for the
partial-split question). This is a stronger statement here than for the derived interface, because
this file is implemented by an F8-owned type: an edit is not merely unnecessary, it is a cross-child
change this child has no mandate to make (§6.4).

### 4.1 Rejected: shape-assertion tests

A reflection-based test that asserts the interface's shape — for example
`typeof(IFilerHomeController).GetProperties().Select(p => p.Name).Should().Contain("FilerQueue")`,
or `typeof(EfcHomeController).Should().Implement<IFilerHomeController>()` — was considered and is
**explicitly rejected**. Four independent reasons:

1. **It buys zero coverage.** The file emits no IL (§2). A reflection test executes lines in the
   *test* assembly, which is excluded from coverage measurement by policy
   (`.claude/rules/general-unit-test.md` § Coverage Requirements). It cannot move the numerator or
   the denominator for `IFilerHomeController.cs` by a single line. It is coverage theatre.
2. **It duplicates the compiler, and does so worse.** If `FilerQueue` were removed from line 33, the
   build would already break at `QfcItemController.MailActions.cs:111`
   (`_homeController.FilerQueue`) and `QfcFormController.EventHandlers.cs:167`, `:193`
   (`_parent.FilerQueue`) with CS1061. If a *member were added*, both implementers would fail to
   compile with CS0535. The compiler check is stronger (it covers both directions), faster, and
   unavoidable. A shape test would fail only after the build already failed.
3. **It violates the general unit-test policy's isolation and intent requirements.** UT1 requires
   each test to target "a single function, method, or unit of behavior." An interface declaration has
   no behavior; a shape assertion has no unit under test and produces no failure message more
   actionable than the compiler's.
4. **It would actively obstruct a needed cross-child change.** §6.3 records an
   interface-segregation smell that a future issue may resolve by *narrowing* this interface. A
   member-name assertion in F7's suite would turn that legitimate, coordinated change into a spurious
   F7 test failure, giving F8 a reason to route around F7 rather than through the epic orchestrator.

**A special case worth naming and also rejecting:** a test asserting that
`EfcHomeController.QuickFileMetrics_WRITE(string)` throws `NotImplementedException` (§6.3). It is
tempting because it *is* behavioral and *would* execute a real line. It is rejected for this child on
two grounds: (a) `EfcHomeController.Metrics.cs` is **sibling F8-owned**, and this child must not
author tests that pin a sibling's production behavior; (b) pinning a `NotImplementedException` as
expected behavior entrenches the very defect §6.3 recommends promoting as an issue. If such a
characterization test is wanted, it belongs to F8.

### 4.2 Also rejected

- **Uncommenting any of the three commented-out members (29, 34, 40).** §5.4 proves each one would
  break `EfcHomeController` (F8-owned) at compile time. Out of scope, and a cross-child change.
- **Deleting the commented-out members** as "dead code cleanup." They carry design information (they
  document exactly which QuickFiler capabilities the Explorer Filer lacks) and their removal has zero
  coverage value while producing a diff on a file two children implement and eight consume.
- **Removing the apparently-unused `using` directives.** `using System;` (line 1) and
  `using ToDoModel;` (line 6) do not appear to be required by any name used in the file
  (`Stopwatch` needs line 2, `SynchronizationContext`/`CancellationToken*` need line 3, `Task` needs
  line 4, `FilerQueue` needs line 5, `ProgressTracker` needs line 7). This is a low-confidence
  observation — it has not been confirmed against analyzer output, and no build was run this session.
  Even if confirmed, the removal has zero coverage value and produces a diff on a shared contract.
  Report-only; do not act inside this child.

---

## 5. Implementer and consumer inventory

Scope of search: the entire worktree, all `*.cs`. `QuickFiler/Legacy/**` and `QuickFiler/Notes/**` are
excluded from the tables below because they are not `<Compile Include=...>` in `QuickFiler.csproj`
and are therefore outside the coverage denominator.

### 5.1 Implementers (two — one of them sibling-owned)

| Implementer | File : line | Owning child |
| --- | --- | --- |
| `QfcHomeController` | `QuickFiler\Controllers\QfcHomeController.cs:22` — `public partial class QfcHomeController : IQfcHomeController`, which derives from `IFilerHomeController` at `Controllers\IQfcHomeController.cs:9` | **F7 (this child)** — *indirect* implementer |
| `EfcHomeController` | `QuickFiler\Controllers\EfcHomeController.cs:18` — `public partial class EfcHomeController : IFilerHomeController` | **F8 (sibling)** — *direct* implementer |

**This is the decisive difference from the derived `IQfcHomeController`, which has exactly one
implementer, all F7-owned.** Every member of `IFilerHomeController` is implemented twice, once by
each child.

### 5.2 Member-by-member implementation map, with sibling ownership flagged

Members implemented by a **sibling-owned type** are flagged; a signature change to any of them breaks
F8.

| Member (line) | F7 implementation (`QfcHomeController` family) | **F8 implementation (`EfcHomeController` family)** |
| --- | --- | --- |
| `Run()` (15) | `QfcHomeController.cs:248-272` | **`EfcHomeController.cs:308`** |
| `RunAsync(ProgressTracker)` (16) | `QfcHomeController.cs:274-324` | **`EfcHomeController.cs:325`** — declared with a default argument (`progress = null`), which still satisfies the contract |
| `Cleanup()` (17) | `QfcHomeController.cs:388-397` | **`EfcHomeController.cs:342`** |
| `UiSyncContext` (23) | `QfcHomeController.cs:479-483` | **`EfcHomeController.cs:412-415`** |
| `TokenSource` (24) | `QfcHomeController.cs:460-464` | **`EfcHomeController.cs:400-403`** |
| `Token` (25) | `QfcHomeController.cs:466-470` | **`EfcHomeController.cs:406-409`** |
| `Loaded` (26) | `QfcHomeController.cs:399-404` | **`EfcHomeController.cs:391`** — `public bool Loaded => throw new NotImplementedException();` |
| `StopWatch` (27) | `QfcHomeController.cs:443-448` | **`EfcHomeController.cs:383-387`** |
| `ExplorerController` (30) | `QfcHomeController.cs:408-413` | **`EfcHomeController.cs:356-361`** |
| `FormController` (31) | `QfcHomeController.cs:415-419` | **`EfcHomeController.cs:363-367`** (returns the private `EfcFormController` field as `IFilerFormController`) |
| `KeyboardHandler` (32) | `QfcHomeController.cs:421-426` | **`EfcHomeController.cs:369-374`** |
| `FilerQueue` (33) | `QfcHomeController.cs:435` | **`EfcHomeController.cs:417`** — `public FilerQueue FilerQueue => throw new NotImplementedException();` |
| `QuickFileMetrics_WRITE(string)` (41) | `QfcHomeController.Metrics.cs:19-88` | **`EfcHomeController.Metrics.cs:26-29`** — `public void QuickFileMetrics_WRITE(string filename) { throw new NotImplementedException(); }` |

**Verification of the two facts the delegation brief asked to confirm:**

1. **`IFilerHomeController` is implemented by the EFC home-controller family owned by sibling F8** —
   **CONFIRMED** by direct read at `QuickFiler\Controllers\EfcHomeController.cs:18`.
2. **`EfcHomeController.Metrics.cs:26-29` implements `QuickFileMetrics_WRITE` by throwing
   `NotImplementedException`** — **CONFIRMED** by direct read. The exact text is:

   ```
   26:        public void QuickFileMetrics_WRITE(string filename)
   27:        {
   28:            throw new NotImplementedException();
   29:        }
   ```

   Two further `NotImplementedException` implementations of this same interface were found that the
   brief did not name: `EfcHomeController.cs:391` (`Loaded`) and `EfcHomeController.cs:417`
   (`FilerQueue`). **Three of the twelve live members are satisfied by a throw on the F8 side.**

### 5.3 Consumers

**Production, inside QuickFiler** (all are type references or member reads through an
`IFilerHomeController`-typed variable):

| Consumer | File : line | Member(s) used | Owning child |
| --- | --- | --- | --- |
| `QfcItemController` field | `QuickFiler\Controllers\QfcItemController.cs:48` — `private IFilerHomeController _homeController;` | type | **F10** |
| `QfcItemController` initialization overloads | `QfcItemController.Initialization.cs:31`, `:88`, `:113`, `:141`, `:348`, `:406`, `:439` — `IFilerHomeController homeController` parameter | type | **F10** |
| `QfcItemController.Initialization.cs:372-375` | `_homeController.KeyboardHandler`, `.ExplorerController`, `.Token`, `.TokenSource` | 32, 30, 25, 24 | **F10** |
| `QfcItemController.Navigation.cs:67`, `:76` | `_homeController.KeyboardHandler` | 32 | **F10** |
| `QfcItemController.MailActions.cs:111` | `_homeController.FilerQueue` | **33** | **F10** |
| `QfcItemController.MailActions.cs:174`, `:192` | `_homeController.FormController` | 31 | **F10** |
| `QfcQueue.LoadControllersViewersAsync` | `QuickFiler\Controllers\QfcQueue.cs:383` — `IFilerHomeController homeController` parameter | type | **F2** |
| `QfcCollectionController` ctor + field | `QfcCollectionController.cs:34`, `:63` | type | **F11** |
| `QfcCollectionController.cs:49` | `_homeController.KeyboardHandler` | 32 | **F11** |
| `QfcCollectionController.cs:617`, `:958` | `_homeController.Token` | 25 | **F11** |
| `QfcExplorerController` ctor + field | `QfcExplorerController.cs:30`, `:41` | type | **F6** |
| `QfcExplorerController.cs:148` | `_parent.FormController.MinimizeFormViewer()` | 31 | **F6** |
| `KeyboardHandler` ctors + field | `KeyboardHandler.cs:29`, `:35`, `:41` | type | **F3** |
| `KeyboardHandler.cs:107`, `:136`, `:153`, `:241` | `_parent.UiSyncContext` | 23 | **F3** |
| `KeyboardHandler.cs:210`, `:214`, `:229`, `:233` | `_parent.FormController` | 31 | **F3** |
| `EfcItemController` ctors + field | `EfcItemController.cs:32`, `:46`, `:61`, `:373` (`:116` is commented out) | type | **F9** |
| `QfcHomeController.cs:178`, `:186` | `IFilerHomeController` as a **type argument** in the `QfcExplorerControllerLoader` and `QfcKeyboardHandlerLoader` `Func<>` seam signatures | type | F7 (self) |

**Production, outside QuickFiler:**

| Consumer | File : line | Detail |
| --- | --- | --- |
| `RibbonController` | `TaskMaster\Ribbon\RibbonController.cs:42` — `private IFilerHomeController _quickFiler;` | The VSTO ribbon holds the *session* through this interface. |
| `RibbonController.LoadQuickFiler` | `:101` `loaded = _quickFiler.Loaded;` | member **26** |
| `RibbonController.LoadQuickFiler` | `:104-108` `_quickFiler = new QfcHomeController(...).Init(); _quickFiler.Run();` | member **15**, plus an upcast from `IQfcHomeController` |
| `RibbonController.LoadQuickFilerAsync` / `...HighConfidenceAsync` | `:118-121`, `:139-142` | assigns the result of `QfcHomeController.LaunchAsync` to the `IFilerHomeController` field |

This makes `IFilerHomeController` the **only** QuickFiler home-controller contract that crosses a
project boundary. It is a genuinely public API of the QuickFiler assembly.

**Test consumers** (all `Mock<IFilerHomeController>` or a parameter of that type):

| File : line | Owning child of the test's subject |
| --- | --- |
| `QuickFiler.Test\Controllers\QfcItemController.InitializationTests.cs:24`, `:33`, `:48`, `:81`, `:116`, `:149` | F10 |
| `QfcItemController.ViewerSetupTests.cs:355` | F10 |
| `QfcItemController.SeamFactoryTests.cs:79`, `:218` | F10 |
| `QfcItemController.NavigationTests.cs:26` (local `NavController(IFilerHomeController)` test double), `:38` | F10 |
| `QfcItemController.EventHandlersTests.cs:288` | F10 |
| `QfcCollectionControllerDarkModeTests.cs:42` | F11 |

**Notable non-consumer.** `QfcQueue`'s *field* is the concrete type
(`QfcQueue.cs:22`, `:33` — `QfcHomeController homeController` / `private QfcHomeController _homeController`),
so its `DataModel` access at `QfcQueue.cs:476` binds to the class, not to this interface. Only
`QfcQueue.LoadControllersViewersAsync`'s parameter (`:383`) uses the interface.

### 5.4 The three commented-out members — does any consumer depend on their absence?

**Yes. All three. Each would break the F8-owned `EfcHomeController` at compile time if uncommented.**

| Commented-out member | Why it is absent — evidenced |
| --- | --- |
| `//IQfcDatamodel DataModel { get; }` (29) | `EfcHomeController.cs:376-381` declares `private EfcDataModel _dataModel;` and `internal EfcDataModel DataModel { get; set; }`. Uncommenting line 29 would fail on **two** counts: the member is `internal` (an interface implementation must be `public`), and its type is `EfcDataModel`, not `IQfcDatamodel` — CS0535 / CS0738. The QuickFiler-side `DataModel` is instead declared on the derived `IQfcHomeController:11`, which `EfcHomeController` does not implement. **The comment is the deliberate mechanism by which the two data models are kept apart.** |
| `//QfcFormViewer FormViewer { get; }` (34) | `EfcHomeController.cs:265` declares `internal EfcViewer FormViewer`. Uncommenting would fail on the same two counts — `internal` accessibility and the wrong type (`EfcViewer` vs `QfcFormViewer`). It would additionally drag `QuickFiler.Viewers.QfcFormViewer`, a **WinForms `Form`-derived, F15-owned, currently `[ExcludeFromCodeCoverage]`** concrete class, into a shared abstract contract — the opposite of the seam direction the epic mandates. |
| `//void Iterate();` (40) | `EfcHomeController` declares no `Iterate()` member anywhere in its six files (verified by grep across `EfcHomeController*.cs`). Uncommenting would produce CS0535 on the F8-owned type. `Iterate()` is instead declared on `IQfcHomeController:13`. |

**Conclusion:** no consumer depends on their absence, but the *implementer* `EfcHomeController` (F8)
does, decisively. The commented-out lines are not vestigial clutter; they are the record of a
segregation that the compiler enforces today only because they remain commented.

---

## 6. Cross-child contract notes and blast-radius assessment

### 6.1 Does this child need to MODIFY `IFilerHomeController.cs`? **No.**

The sibling F7 research artifacts propose nine seams across `QfcHomeController.cs` and
`QfcHomeController.Metrics.cs`:

| Seam | Proposed shape | Touches this interface? |
| --- | --- | --- |
| S1 `ShowUserMessage` | `internal Action<string>` property | No |
| S2 `MetricsFileWriter` | `internal Func<string,string[],string,CancellationToken,Task>` property | No |
| S3 `IUiDispatcher` (optional) | `internal IUiDispatcher` property | No |
| S4 `LaunchCoreAsync` (Tier C) | `internal async Task<QfcHomeController>` method | No |
| S5a `QfcFormViewerLoader` (Tier C) | `internal Func<IQfcFormViewer>` property | No |
| S5b `UiSchedulerLoader` (Tier C) | `internal Func<TaskScheduler>` property | No |
| Metrics S1 `MetricsAdder` | `internal Func<string,int,CancellationToken,bool>` property | No |
| Metrics S2 `MetricsLineWriter` | `internal Action<string,string[],string>` property | No |
| Metrics S3 `BuildDurationTexts` | `internal static` pure method | No |
| Metrics S5 | `private` → `internal` on `NonBlockingProducer` ×2 | No |

**Every one is `internal` on the class.** `[assembly: InternalsVisibleTo("QuickFiler.Test")]` at
`QuickFiler\Controllers\QfcHomeController.cs:18` makes them directly reachable from the test project,
and `QuickFiler\Controllers\QfcHighConfidencePreFilter.cs:11` declares
`[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` so Moq can proxy internal types where
needed.

C# interfaces cannot declare `internal` members that implementers must satisfy, so an `internal` seam
**cannot** be routed through this interface even if someone wanted to. The seven existing loader
seams and the injectable `TimeProvider` are the ratified precedent: none of them appears on
`IFilerHomeController` or on `IQfcHomeController`.

**Conclusion: interface modification is unnecessary for every seam the sibling F7 research artifacts
actually propose.** The planner should treat `IFilerHomeController.cs` as a read-only file for this
child.

### 6.2 Blast radius if the interface were modified anyway

| Change | Compile impact | Children affected |
| --- | --- | --- |
| **Add** a member | Both implementers must satisfy it: `QfcHomeController` (F7) **and** `EfcHomeController` (**F8**) — CS0535 on the F8-owned type otherwise. Additionally, the six F10-owned and one F11-owned `Mock<IFilerHomeController>` test fixtures would silently return `default` for the new member. | **F7 + F8** hard, **F10 / F11** latent |
| **Remove or rename** a member | Breaks the consumer surface in §5.3: **F10** (7 call sites), **F3** (8), **F11** (3), **F6** (1), **F2** (1 signature), **F9** (4 signatures), plus **TaskMaster/Ribbon/RibbonController.cs:101, 108** outside the epic. | **F2, F3, F6, F9, F10, F11**, TaskMaster |
| **Change a member signature** | Same as removal, plus both implementers. | **F7, F8**, + all of the above |
| **Uncomment line 29, 34 or 40** | Breaks **`EfcHomeController` (F8)** immediately — §5.4 gives the exact CS0535/CS0738 reason for each. Line 34 additionally couples the contract to **F15**-owned `QfcFormViewer`. | **F8**, (F15) |

Sibling-owned **types named in this contract's member signatures** (type references, not
implementations — a rename or removal of any of them breaks this file):

| Referenced type | Line | Owning child |
| --- | --- | --- |
| `IQfcExplorerController` (`QuickFiler/Interfaces/IQfcExplorerController.cs`) | 30 | **F6** |
| `IFilerFormController` (`QuickFiler/Interfaces/IFilerFormController.cs`) | 31 | **F6** |
| `IQfcKeyboardHandler` (`QuickFiler/Interfaces/IQfcKeyboardHandler.cs`) | 32 | **F3** |
| `FilerQueue` (`QuickFiler/Controllers/FilerQueue.cs`) | 33 | **F2** |
| `IQfcDatamodel` | 29 (commented) | **F5** |
| `QfcFormViewer` | 34 (commented) | **F15** |
| `ProgressTracker` (`UtilitiesCS`) | 16 | outside the epic |

**No addition or change is requested of any of these children.** F7 asks only that the type names
remain stable through wave 1. This is worth stating in the plan because F2, F3 and F6 are all
performing seam extraction on the very types named here during the same wave; a rename by any of them
would break this file even though F7 never touches it.

### 6.3 CROSS-CHILD CONTRACT NOTE — CC-B1 (the one the brief asked for)

**Adding a member to `IFilerHomeController` is a cross-child change and MUST NOT be planned
unilaterally by F7.**

The reason is not stylistic. `IFilerHomeController` has **two** implementers (§5.1), and one of them
— `EfcHomeController` — is owned by sibling child **F8**
(`quickfiler-efc-home-controller-coverage`, wave 1, running in parallel with F7). Adding any member
to this interface produces CS0535 on `EfcHomeController.cs` unless F8 simultaneously implements it.
Because F7 and F8 execute concurrently on separate branches and fan in at integration, a unilateral
addition by F7 would either (a) break F8's build after the rebase, or (b) force F7 to edit an
F8-owned file, which the epic's disjoint-file-set decomposition explicitly forbids
(`epic.md` § Decomposition Rationale).

The same logic applies with equal force to member removal, rename and signature change (§6.2), and to
uncommenting any of lines 29/34/40 (§5.4).

**Required protocol if such a change is ever genuinely needed:** raise it to the epic orchestrator as
a cross-child contract change, coordinate the F7 and F8 edits into one atomic task set, and re-run
both children's research. Per §6.1, no such change is needed for anything F7's research proposes.

### 6.4 Interface-segregation assessment (report-only)

**Finding.** Three of the twelve live members of `IFilerHomeController` are satisfied on the F8 side
by `throw new NotImplementedException()`:

| Member | F8 implementation | Consumers of the member (§5.3) |
| --- | --- | --- |
| `Loaded` (26) | `EfcHomeController.cs:391` | `RibbonController.cs:101` — reached only on the QuickFiler path, so the throw is not currently hit |
| `FilerQueue` (33) | `EfcHomeController.cs:417` | `QfcItemController.MailActions.cs:111`, `QfcFormController.EventHandlers.cs:167`, `:193` — all QuickFiler-side |
| `QuickFileMetrics_WRITE(string)` (41) | `EfcHomeController.Metrics.cs:26-29` | **no production caller at all** — the F7-side implementation at `QfcHomeController.Metrics.cs:19-88` is itself dead in production (verified in the companion Metrics artifact, finding D10) |

**Diagnosis.** This is a textbook Interface Segregation Principle violation: a client
(`EfcHomeController`) is forced to depend on methods it does not use. The `NotImplementedException`
bodies are the standard symptom — the compiler demands an implementation the type has no meaning for,
so the author supplies a throw. Two aggravating specifics:

1. **The `Loaded` throw is a live landmine.** `RibbonController.cs:100-101` reads
   `_quickFiler.Loaded` through the `IFilerHomeController`-typed field. Today that field is only ever
   assigned a `QfcHomeController`, so the EFC throw is unreachable. If any future change assigns an
   `EfcHomeController` to that field — which the field's declared type explicitly permits — the ribbon
   throws `NotImplementedException` on a user click. The type system currently advertises a capability
   the object does not have.
2. **`QuickFileMetrics_WRITE(string)` is dead on both sides.** Neither implementation has a production
   caller. The member exists solely so that the interface can declare it. It is the clearest candidate
   for removal.

**Recommended disposition (not for this child).** The proportionate fix is to *narrow* the shared
interface — move `Loaded`, `FilerQueue` and `QuickFileMetrics_WRITE(string)` down onto
`IQfcHomeController` (where `QfcHomeController` already implements them and where the QuickFiler-side
consumers already have a reference), leaving `IFilerHomeController` as the genuinely common surface.
That would delete three `NotImplementedException` bodies from F8's file set and remove three
uncoverable throws from F8's coverage denominator, which is directly aligned with the epic's goal.

It is **not** in scope for F7: it edits an F8-owned file, edits a shared contract, and is a behavior-
adjacent refactor rather than coverage work. Per the repository's standing practice, promote it
through the MCP issue lifecycle as its own issue rather than leaving it as prose in a feature folder,
and record it as an input to the capstone F16. Note also that a default interface implementation is
**not** an available alternative fix on this target framework (§2.2), and would be the wrong fix in
any case — it would hide the segregation problem behind a silently-inherited body instead of
resolving it.

---

## 7. Partial-split impact assessment

The `QfcHomeController.cs` research recommends **Split 1 (mandatory)**: moving the entire
`#region Public Properties` block, source lines 406-485 of `QfcHomeController.cs`, into a new partial
file `QuickFiler/Controllers/QfcHomeController.Properties.cs`. **Eight of the twelve live members of
this interface** have their F7-side implementation inside that block:

| `IFilerHomeController` member | Current location in `QfcHomeController.cs` | Moves under Split 1? |
| --- | --- | --- |
| `ExplorerController` (30) | 408-413 | **Yes** |
| `FormController` (31) | 415-419 | **Yes** |
| `KeyboardHandler` (32) | 421-426 | **Yes** |
| `FilerQueue` (33) | 435 | **Yes** |
| `StopWatch` (27) | 443-448 | **Yes** |
| `TokenSource` (24) | 460-464 | **Yes** |
| `Token` (25) | 466-470 | **Yes** |
| `UiSyncContext` (23) | 479-483 | **Yes** |
| `Loaded` (26) | 399-404 | No — outside the stated 406-485 range |
| `Run()` (15) | 248-272 | No |
| `RunAsync(ProgressTracker)` (16) | 274-324 | No |
| `Cleanup()` (17) | 388-397 | No |
| `QuickFileMetrics_WRITE(string)` (41) | `QfcHomeController.Metrics.cs:19-88` | No |

This is the largest concentration of interface-member implementations affected by the proposed split,
which is why the question is directed here.

### 7.1 Effect on the interface contract: **none.** Reasoning stated explicitly.

1. **A partial class is one type.** The C# language specification treats the parts of a partial type
   declaration as a single declaration: the member set is the union of all parts, and the base-class /
   interface list is the union of all parts' base lists. Compilation emits exactly one `TypeDef` for
   `QuickFiler.Controllers.QfcHomeController` no matter how many `.cs` files declare parts of it.
2. **Interface implementation mapping is per-type, not per-file.** The CLR interface map binds
   `IFilerHomeController.get_UiSyncContext` to `QfcHomeController.get_UiSyncContext` by type and
   member. Source file has no representation in that metadata. Relocating an accessor's source text
   changes only the sequence points recorded in the PDB — i.e. *which file the coverage report
   attributes the lines to* — not the emitted contract, not the interface map, not the vtable layout.
3. **`EfcHomeController` is entirely unaffected.** Split 1 touches only `QfcHomeController`. F8's
   implementation of the same interface (§5.2) is not read, moved, or recompiled differently. **The
   split is therefore not a cross-child change**, which is the specific reassurance CC-B1 requires.
4. **Consumers are unaffected.** Every consumer in §5.3 binds through `IFilerHomeController` or
   through a concrete type name. None references a file. `KeyboardHandler.cs:107` (`_parent.UiSyncContext`),
   `QfcItemController.MailActions.cs:111` (`_homeController.FilerQueue`),
   `QfcCollectionController.cs:617` (`_homeController.Token`) and the rest compile byte-identically
   before and after.
5. **Accessibility is preserved.** The `internal` setter on `DataModel` and the `private` setter on
   `WorkerComplete` (both from the derived interface) keep their semantics; `[assembly:
   InternalsVisibleTo("QuickFiler.Test")]` at `QfcHomeController.cs:18` is assembly-scoped and must
   simply remain in *some* compiled file (the sibling research already flags keeping it where it is).
6. **The existing reflection-based tests are unaffected.** They resolve members via
   `_controller.GetType()`, which returns the single `QfcHomeController` type object regardless of
   source partitioning.

**Therefore: Split 1 requires no edit to `IFilerHomeController.cs`, imposes no obligation on F8, and
this artifact raises no objection to it on contract grounds.**

### 7.2 Would the new partial file itself be `testable` or interface-only under the F1 ledger?

**`testable`.** `QfcHomeController.Properties.cs` would be a *class* file containing real property
accessors, backing fields, and the `CreateCancellationToken()` method body — all IL-producing. It
would appear in the Cobertura report as source lines attributed to
`QuickFiler.Controllers.QfcHomeController` with
`filename="QuickFiler\Controllers\QfcHomeController.Properties.cs"`, exactly as the three existing
partials do today. The interface-only carve-out in `.claude/rules/general-unit-test.md` does not apply
to it, and the epic's Shared Design §1 "refactor first" standard does. The `QfcHomeController.cs`
research sizes it at 22 coverable lines, 18 covered today, reaching 100% once its proposed TC2 covers
`CreateCancellationToken`.

Stated plainly for the planner: **moving interface-member implementations out of a class file does not
turn the destination file into an interface-only file.** The destination holds implementations, which
are code; the interface files hold declarations, which are not. The two categories do not mix.

### 7.3 CROSS-CHILD CONTRACT NOTE — CC-B2 (ledger mechanics for a file that does not exist yet)

F1's ledger is being authored against the **121 files currently compiled**.
`QfcHomeController.Properties.cs` will not be in it. The planner must ask F1 how a child registers a
ledger row for a file it creates mid-wave — otherwise the capstone F16 ("every one of the 121
compiled files is either at >= 80% or on the ratified ledger") will encounter a file with no row.
F9, F11 and F13 are all expected to create partial files in the same wave, so this is a shared
mechanism question, not an F7-local one. Any new partial also requires a `<Compile Include=...>` entry
in `QuickFiler/QuickFiler.csproj` near lines 325-327, a known merge-conflict hotspot for wave 1.

---

## 8. Risks and open questions

### Risks

- **R1 — F1 has not landed.** The ledger and the harness are both upstream and absent (verified: the
  epic directory contains only `epic.md`). Gate the plan's Phase 0 on reading the ledger row for this
  file. If F1 classifies it `testable`, §3 and §4 are void.
- **R2 — this is the highest-fan-in file in F7's set.** Eight of the fifteen wave-1 children
  (F2, F3, F6, F9, F10, F11, plus F7 and F8 as implementers) touch this contract, and one consumer
  (`TaskMaster/Ribbon/RibbonController.cs`) is outside the epic entirely. An "obvious" one-line edit
  here has the largest blast radius of any file assigned to F7. The mitigation is §6.1: no edit is
  needed. The plan should state that as an explicit constraint on the executing agent, not leave it
  implicit.
- **R3 — sibling rename exposure.** Six of this file's member signatures name types owned by other
  wave-1 children (§6.2 table). F2 (`FilerQueue`), F3 (`IQfcKeyboardHandler`) and F6
  (`IQfcExplorerController`, `IFilerFormController`) are all performing seam extraction on those very
  types during the same wave. A rename by any of them breaks this file even though F7 never edits it.
  This is an integration-merge watch item; the epic orchestrator's pre-wave rebase is the control.
- **R4 — "no work" tasks attract make-work.** A file with a zero-line denominator invites an
  implementer to invent a test so the child looks complete. §4.1 rejects shape-assertion tests, and
  the `NotImplementedException` characterization test specifically, on the record so a reviewer has a
  citable basis for rejecting one in a PR.
- **R5 — the `Loaded` landmine (§6.4).** `RibbonController.cs:101` reads a member that one of the two
  implementers throws on. It is unreachable today only because of an assignment convention, not a type
  constraint. This is a latent production defect, not a coverage issue; it must be promoted as an
  issue rather than fixed inside a coverage child.
- **R6 — Cobertura evidence is indicative, not F1 harness output.** §2.3 reads a committed artifact
  produced by a different feature (#424). It is strong evidence of the *structural* fact (the
  instrumenter emits nothing for interface files, with `MailItemActionsAdapter` as the positive
  control), and that fact does not depend on which tests ran. It is nevertheless not F1 harness output
  and must not be cited as acceptance evidence.

### Open questions for the planner / F1

1. **Ledger classification.** Does F1 classify `QuickFiler/Interfaces/IFilerHomeController.cs` as
   `interface-only / not-measured`? (Recommended; evidence in §2-§3.)
2. **Ledger row format for zero-line files.** Does F1 want a per-file coverage number recorded for
   interface-only files (`0/0`, or "not measured"), and does the harness emit a row for a file the
   Cobertura report omits entirely? This determines what W2 in §4 actually commits as evidence.
3. **How many of the ~24 interface-only files does F1 expect to classify this way?** The epic manifest
   states ~24 of the 121 compiled files are interface-only. F7 owns two of them
   (`Controllers/IQfcHomeController.cs`, `Interfaces/IFilerHomeController.cs`). Confirming the count
   and the exact classification label F1 uses would let all fifteen children write identical ledger
   rows rather than fifteen variants.
4. **New-file ledger rows (CC-B2).** How does a wave-1 child register `QfcHomeController.Properties.cs`
   in a ledger authored against the pre-existing 121 files?
5. **ISP defect promotion (§6.4).** Should the three `NotImplementedException` implementations —
   `EfcHomeController.cs:391` (`Loaded`), `:417` (`FilerQueue`), `EfcHomeController.Metrics.cs:26-29`
   (`QuickFileMetrics_WRITE`) — be promoted as one issue proposing the interface narrowing, and does
   the orchestrator want that issue raised before F8 executes (so F8 can absorb the fix and drop three
   uncoverable throws from its own denominator) or after wave 1 closes?
6. **Cross-child change protocol (CC-B1).** Confirm the epic orchestrator's mechanism for a
   coordinated F7+F8 change to this interface, so the protocol exists on paper before anyone needs it.
