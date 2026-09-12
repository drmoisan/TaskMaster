---
Timestamp: 2026-08-07T20-50
Feature: quickfiler-qfc-home-controller-coverage (epic child F7, issue #433)
Epic: quickfiler-per-file-coverage (parent issue #136)
Target file: QuickFiler/Controllers/IQfcHomeController.cs
Target file (absolute): C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-afcf27830d48e5590\QuickFiler\Controllers\IQfcHomeController.cs
Line count: 20
Worktree: C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-afcf27830d48e5590
Base commit: 74be1964
Coverage classification authority: docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md (child F1, wave 0 — verified ABSENT from disk at research time)
Coverage evidence mechanism: F1's per-file line-coverage harness derived from the Cobertura output of Invoke-MSTestWithCoverage.ps1
Research method: static read of the file, csproj inspection, committed-Cobertura inspection, repository-wide grep. No msbuild, no vstest, no coverage run performed.
---

# Research — `QuickFiler/Controllers/IQfcHomeController.cs` (F7, issue #433)

## 0. Upstream contract consumed (F1, wave 0)

This artifact is written to **consume** F1's contract, not to substitute for it.

1. **Classification authority.** Whether this file is `testable` or `interface-only / not-measured` is
   decided by the ratified ledger at
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

`QuickFiler.Controllers.IQfcHomeController` is the **QuickFiler-specific** extension of the shared
filer home-controller contract. It is a pure abstract interface declaration: 5 `using` directives, a
namespace, an interface header with one base interface, and 8 member declarations. There is no
class, no field, no constant, no attribute, and no member body.

| Line | Member | Kind |
| --- | --- | --- |
| 9 | `public interface IQfcHomeController : IFilerHomeController` | interface header; inherits `QuickFiler.Interfaces.IFilerHomeController` |
| 11 | `IQfcDatamodel DataModel { get; }` | read-only property |
| 12 | `IQfcHomeController Init();` | method |
| 13 | `void Iterate();` | method |
| 14 | `void Iterate2();` | method |
| 15 | `Task IterateQueueAsync();` | method |
| 16 | `void SwapStopWatch();` | method |
| 17 | `Task WriteMetricsAsync(string filename);` | method |
| 18 | `bool WorkerComplete { get; }` | read-only property |

**Contract meaning.** The base `IFilerHomeController` carries the members common to both filer
front-ends (QuickFiler and the Explorer Filer). `IQfcHomeController` adds the four QuickFiler-only
capabilities that the Explorer Filer does not have: an `IQfcDatamodel` (the EFC controller uses a
different, EFC-specific data model type — see §6), a fluent `Init()` that returns the initialized
controller, the iteration/stopwatch surface consumed by `QfcFormController`, and the async metrics
write.

**Namespace/file-name collision (finding, report-only).** A second file named
`IQfcHomeController.cs` exists in the working tree at
`C:\...\QuickFiler\Interfaces\IQfcHomeController.cs`. It declares an unrelated
`QuickFiler.Interfaces.IQfcHomeController` with different members (`ExplCtrlr`, `FrmCtrlr`,
`KbdHndlr`, `ExecuteMoves()`, `cStopWatch StopWatch`). It is **not compiled**:
`QuickFiler/QuickFiler.csproj:304` includes only `Controllers\IQfcHomeController.cs`; the
`Interfaces\IQfcHomeController.cs` entry survives only in the stale
`QuickFiler/QuickFiler.csproj.bak:244`. The orphan is therefore outside the 121-file coverage
denominator and outside this child's file set (the epic assigns `Controllers/IQfcHomeController.cs`
at `epic.md:303`). It is a name-collision hazard for any grep-driven planning task; it must not be
edited, deleted, or added to the ledger by this child. Recommend promoting the dead-file cleanup as
a separate issue if the maintainer wants it removed.

---

## 2. Executable-content analysis

### 2.1 Exhaustive check for IL-producing constructs

Every construct that could put an executable line in the coverage denominator was checked against
the full 20-line file.

| Construct checked | Present? | Evidence |
| --- | --- | --- |
| Default interface implementation (C# 8+ member body) | **No** | Every member on lines 11-18 terminates in `;`. No `{ }` body, no `=>` expression body, anywhere in the file. |
| `static` member with a body (C# 8+ static interface member) | **No** | No `static` keyword appears in the file. |
| Constant / field initializer | **No** | Interfaces cannot declare instance fields; no `const` declaration is present. |
| Attribute with a computed argument | **No** | The file contains no attribute of any kind — no `[ExcludeFromCodeCoverage]`, no assembly attribute, nothing in square brackets. |
| Nested type with a body | **No** | The only type declared is the interface itself. |
| Property with an accessor body | **No** | Lines 11 and 18 are `{ get; }` auto-declarations, which in an interface are abstract accessor declarations, not bodies. |
| Static constructor / module initializer | **No** | None. |
| Explicit interface re-declaration with body | **No** | Line 9 is a base-list entry only. |

### 2.2 Target-framework evidence (default interface implementations are unavailable)

`QuickFiler/QuickFiler.csproj`:

```
13:    <TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>
14:    <LangVersion>preview</LangVersion>
```

The project targets **.NET Framework 4.8.1**. Default interface implementations (C# 8) require
runtime support that the .NET Framework CLR does not provide; the Roslyn compiler rejects them on
this target regardless of `LangVersion`. `LangVersion=preview` therefore does **not** open the
door to a DIM in this file: even if a future edit attempted one, the build would fail rather than
silently add executable lines.

This is a belt-and-braces argument. The primary evidence is §2.1: the file contains no member body
of any kind, so the DIM question is moot for the file as it stands. The framework fact matters for
the ledger's forward-looking classification — an interface file in this project **cannot** acquire
executable content by way of a DIM, so the `interface-only` classification is stable across future
edits in a way it would not be on a .NET 8 target.

### 2.3 Direct Cobertura evidence (a compiled-and-instrumented artifact says zero)

The committed artifact
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
was produced by `Invoke-MSTestWithCoverage.ps1` against a build of this same source at
substantially the current content, and it instruments QuickFiler (verified: the sibling class
`QuickFiler.Controllers.QfcHomeController` is present at line 21643 with
`filename="QuickFiler\Controllers\QfcHomeController.cs"`).

Observed results:

| Query | Result |
| --- | --- |
| Any `<class ... filename="QuickFiler\Controllers\IQfcHomeController.cs">` | **No match.** |
| Any `<class name="QuickFiler.Controllers.I...">` or `<class name="QuickFiler.Interfaces.I...">` (regex `class name="QuickFiler\.[A-Za-z.]*I[A-Z]`) | **No match** anywhere in the file. |
| Any `<class>` whose `filename` starts `QuickFiler\Interfaces\` | Exactly **one**: line 14448, `QuickFiler.Interfaces.MailItemActionsAdapter` — a concrete *class* that happens to live in the `Interfaces` folder. No interface type appears. |
| Textual occurrences of `IQfcHomeController` in the artifact | 10, **all** inside `signature="..."` attributes of methods belonging to other classes (constructor parameter types), e.g. line 19397. None is a `<class>` element. |

**Conclusion from the artifact:** the instrumentation emitted no class entry, no method entry, and
no line entry for `IQfcHomeController.cs`. The file contributes **zero lines** to both the numerator
and the denominator of the per-file coverage metric. This is direct, reproducible evidence, not an
inference from source reading.

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

**Metadata is emitted, IL is not.** The compiler does emit type metadata for the interface (that is
why the type name appears in other classes' method signatures in §2.3), and Moq emits real IL for
proxies of this interface at run time — but into the dynamic `DynamicProxyGenAssembly2` assembly,
never attributed to this source file. Neither fact places a coverable line in this file.

---

## 3. Recommended F1 ledger classification and rationale

**Recommended classification: `interface-only / not-measured`.**

Rationale, in the order F1's ledger should record it:

1. **Zero executable lines, evidenced two independent ways** — exhaustive source construct check
   (§2.1) and a committed instrumented Cobertura artifact with no class entry (§2.3).
2. **Rule-text match** — the file is precisely the "C# interface-only file" the
   `.claude/rules/general-unit-test.md` carve-out names.
3. **The carve-out is the correct instrument here, not the COM/VSTO exemption.** The epic's Shared
   Design §1 reconciliation ("refactor first, exempt only the irreducible remainder") governs files
   whose lines are *executable but hard to reach*. This file has no executable lines at all, so
   there is nothing to refactor and no exemption to ratify. Classifying it under the COM/VSTO
   exemption would be a category error and would wrongly imply a testability debt.
4. **No `[ExcludeFromCodeCoverage]` disposition is owed.** The file carries no attribute (§2.1), so
   it is not one of the 33 attributes F1 must dispose of.
5. **Stability** — because the project targets .NET Framework 4.8.1, this file cannot acquire
   executable content through a default interface implementation (§2.2). The classification will not
   silently become wrong.

**F1's ledger is the authority.** This is a recommendation with evidence attached, not a decision.
If F1 instead classifies the file `testable`, every recommendation in §4 is void and this artifact
must be re-run — but note that a `testable` classification would be unsatisfiable on the present
content, because a file with a zero-line denominator has no line rate to raise; the only honest
response would be for F1 to record `0/0` and treat the target as vacuously met.

**Consequence for this child's acceptance criteria.** Issue #136 measures per-file line coverage.
Under the recommended classification, `IQfcHomeController.cs` is outside this child's >= 80%
obligation. The child still owes the ledger row and the numeric harness output that demonstrates
zero measurable lines.

---

## 4. Required work for this child

**No test work. Record the ledger classification and the numeric harness output as evidence.**

That is the complete disposition. Concretely, the atomic plan should contain, for this file, exactly
two non-test tasks:

| Task | Deliverable |
| --- | --- |
| W1 | Confirm F1's ledger row for `QuickFiler/Controllers/IQfcHomeController.cs` reads `interface-only / not-measured` (or, if F1 chose otherwise, halt and re-run this research). Consuming the ledger is a Phase-0 read, not an edit — F1 owns `coverage-ledger.md`. |
| W2 | Run F1's per-file harness as part of the child's normal coverage run and commit the numeric per-file result — expected to be "file absent from the report" / zero measurable lines — to `<FEATURE>/evidence/qa-gates/`. This is the evidence that the classification is factually correct at execution time, not merely asserted. |

**No production edit to this file is required** (see §6 for the blast-radius argument, and §7 for the
partial-split question).

### 4.1 Rejected: shape-assertion tests

A reflection-based test that asserts the interface's shape — for example
`typeof(IQfcHomeController).GetMethods().Select(m => m.Name).Should().Contain("IterateQueueAsync")`,
or `typeof(IQfcHomeController).Should().Implement<IFilerHomeController>()` — was considered and is
**explicitly rejected**. Four independent reasons:

1. **It buys zero coverage.** The file emits no IL (§2). A reflection test executes lines in the
   *test* assembly, which is excluded from coverage measurement by policy
   (`.claude/rules/general-unit-test.md` § Coverage Requirements). It cannot move the numerator or
   the denominator for `IQfcHomeController.cs` by a single line. It is coverage theatre.
2. **It duplicates the compiler.** If `IterateQueueAsync()` were removed from line 15,
   `QfcHomeController` would still compile (an extra public member is legal), but every consumer
   listed in §5 — `QfcFormController.EventHandlers.cs:162`, `:199`, `:373` — would fail to compile
   with CS1061. The build is already a stronger, faster, and unavoidable check than any runtime
   reflection assertion. A shape test would fail *after* the build already failed.
3. **It violates the general unit-test policy's isolation and intent requirements.** UT1 requires
   each test to target "a single function, method, or unit of behavior." An interface declaration
   has no behavior; a shape assertion has no unit under test and cannot produce an actionable
   failure message beyond what the compiler already produced.
4. **It creates a maintenance liability with negative value.** A member-name assertion pins the
   contract against renames that siblings F6/F8 might legitimately need to coordinate (§6), turning
   a compile-time negotiation into a run-time test failure in F7's suite.

No amount of interface-shape assertion is acceptable coverage work for this child.

### 4.2 Also rejected

- **Adding XML documentation comments to the interface members** to "do something useful with the
  file." Documentation comments are not executable and would change nothing about the classification
  while adding diff surface to a file three sibling children read (§6). If the maintainer wants XML
  docs on QuickFiler's public interface surface, that is a separate documentation initiative, not a
  coverage child's work.
- **Removing the apparently-unused `using` directives.** `using System.Diagnostics;` (line 1),
  `using System.Threading;` (line 2) and `using UtilitiesCS;` (line 5) do not appear to be required
  by any name used in the file (`Task` needs line 3; `IQfcDatamodel` needs line 4). This is a
  low-confidence observation — it has not been confirmed against analyzer output, and no analyzer
  diagnostic for it was produced in this session because no build was run. Even if confirmed, the
  removal has zero coverage value and produces a diff on a file whose contract three siblings
  consume. Report-only; do not act inside this child.

---

## 5. Implementer and consumer inventory

Scope of search: the entire worktree, all `*.cs`. Legacy/Notes paths are marked because they are not
`<Compile Include=...>` in `QuickFiler.csproj` and are therefore outside the coverage denominator.

### 5.1 Implementers (exactly one)

| Implementer | File : line | Owning child |
| --- | --- | --- |
| `QfcHomeController` | `QuickFiler\Controllers\QfcHomeController.cs:22` — `public partial class QfcHomeController : IQfcHomeController` | **F7 (this child)** |

There is no second implementer in production and no hand-written test double. All test usage is
`Mock<IQfcHomeController>` (§5.3), which produces a run-time proxy, not a source implementer.

### 5.2 Member-by-member implementation map

Every member is implemented on the `QfcHomeController` partial family, all three files of which are
F7-owned.

| Interface member (line) | Implementation | Owning child |
| --- | --- | --- |
| `DataModel` (11) | `QfcHomeController.cs:428-433` (backing field + `internal` setter) | F7 |
| `Init()` (12) | `QfcHomeController.cs:89-109` | F7 |
| `Iterate()` (13) | `QfcHomeController.Iteration.cs:55` | F7 |
| `Iterate2()` (14) | `QfcHomeController.Iteration.cs:70` | F7 |
| `IterateQueueAsync()` (15) | `QfcHomeController.Iteration.cs:11` | F7 |
| `SwapStopWatch()` (16) | `QfcHomeController.Iteration.cs:79-84` | F7 |
| `WriteMetricsAsync(string)` (17) | `QfcHomeController.Metrics.cs:90-155` | F7 |
| `WorkerComplete` (18) | `QfcHomeController.cs:472-477` | F7 |
| inherited base members | see the companion artifact for `IFilerHomeController.cs` | F7 / F8 |

**No member of `IQfcHomeController` is implemented by a sibling-owned type.** This is the decisive
blast-radius fact for this file and is developed in §6.

### 5.3 Consumers

**Production, inside QuickFiler:**

| Consumer | File : line | Member(s) used | Owning child |
| --- | --- | --- | --- |
| `QfcFormController` constructor parameter | `QuickFiler\Controllers\QfcFormController.cs:33` — `IQfcHomeController parent` | type reference | **F6** |
| `QfcFormController` field | `QfcFormController.cs:81` — `private IQfcHomeController _parent;` | type reference | **F6** |
| `QfcFormController` ctor body | `QfcFormController.cs:47` — `WriteMetrics = parent.WriteMetricsAsync;` | `WriteMetricsAsync` (17) | **F6** |
| `QfcFormController` ctor body | `QfcFormController.cs:48` — `Iterate = parent.Iterate;` (bound into the private `IterateDelegate Iterate` field at `:85`) | `Iterate` (13) | **F6** |
| `QfcFormController.SetupDisposal.cs:225` | `Iterate = null;` | releases the (13) binding | **F6** |
| `QfcFormController.EventHandlers.cs:142`, `:191`, `:372` | `_parent.SwapStopWatch()` / `_parent?.SwapStopWatch()` | `SwapStopWatch` (16) | **F6** |
| `QfcFormController.EventHandlers.cs:162`, `:199`, `:373` | `await _parent.IterateQueueAsync()` / `UiThread.Dispatcher.InvokeAsync(_parent.IterateQueueAsync)` (`:173` is commented out) | `IterateQueueAsync` (15) | **F6** |
| `QfcFormController.EventHandlers.cs:196` | `_parent.DataModel` | `DataModel` (11) | **F6** |
| `QfcFormController.EventHandlers.cs:252` | `while (!_parent.WorkerComplete)` | `WorkerComplete` (18) | **F6** |
| `QfcHomeController.cs:89` | return type of `public IQfcHomeController Init()` | `Init` (12) | F7 (self) |

**Production, outside QuickFiler:**

| Consumer | File : line | Detail |
| --- | --- | --- |
| `RibbonController.LoadQuickFiler` | `TaskMaster\Ribbon\RibbonController.cs:104-108` | `_quickFiler = new QuickFiler.Controllers.QfcHomeController(Globals, ReleaseQuickFiler).Init(); _quickFiler.Run();` — consumes `Init()` (12) and immediately **upcasts** its `IQfcHomeController` return to the `IFilerHomeController _quickFiler` field (`:42`). This is the only cross-project consumer, and it is in the `TaskMaster` project, outside the epic's file set. |

**Test consumers:**

| Consumer | File : line | Member(s) |
| --- | --- | --- |
| `QfcFormControllerTests` | `QuickFiler.Test\Controllers\QfcFormControllerTests.cs:25` (`Mock<IQfcHomeController> _mockParent`), `:110`, `:151` (`GetPrivateField<IQfcHomeController>(controller, "_parent")`), `:474`, `:491` (`Setup(x => x.WorkerComplete)`) | type + (18) |
| `QfcFormControllerSeamTests` | `QfcFormControllerSeamTests.cs:31`, `:127`, `:225` (`SetupGet(p => p.WorkerComplete)`) | type + (18) |

**Notable non-consumer.** `QfcQueue` (F2) holds the **concrete** type, not the interface:
`QuickFiler\Controllers\QfcQueue.cs:22` primary-constructor parameter `QfcHomeController homeController`
and `:33` `private QfcHomeController _homeController = homeController;`. Its `DataModel` access at
`QfcQueue.cs:476` therefore binds to the class member, not through `IQfcHomeController`. F2 is not an
`IQfcHomeController` consumer.

### 5.4 Members with no production consumer (report-only)

| Member | Status |
| --- | --- |
| `Iterate()` (13) | Bound at `QfcFormController.cs:48` into the private delegate field `Iterate` (`:85`) and nulled at `SetupDisposal.cs:225`, but the delegate is **never invoked** anywhere in compiled code. The binding is the only consumption. Exercised directly on the concrete type by `QfcHomeControllerIterationTests.cs:340`, `:389`. |
| `Iterate2()` (14) | **Zero consumers.** Repository-wide, `Iterate2` appears at exactly four places: this declaration (line 14), the implementation (`QfcHomeController.Iteration.cs:70`), and two lines of one test (`QfcHomeControllerIterationTests.cs:405`, `:424`). No production call site exists. |

Both are interface-segregation observations, not defects this child should fix. Removing either
member would be a contract change touching F6's compile surface (`Iterate`) and would be out of
scope for a coverage child. Recorded for the capstone F16 / a future cleanup issue.

---

## 6. Cross-child contract notes and blast-radius assessment

### 6.1 Does this child need to MODIFY `IQfcHomeController.cs`? **No.**

This is the most consequential finding for the planner, and it is settled by the ground truth
established in the sibling artifacts.

The sibling research artifacts propose exactly these seams for the F7 production files:

| Seam | Proposed in | Proposed shape | Touches this interface? |
| --- | --- | --- | --- |
| S1 `ShowUserMessage` | `QfcHomeController.cs` research §5 | `internal Action<string> ShowUserMessage { get; set; }` | No |
| S2 `MetricsFileWriter` | `QfcHomeController.cs` research §5 | `internal Func<string,string[],string,CancellationToken,Task> MetricsFileWriter { get; set; }` | No |
| S3 `IUiDispatcher` (optional) | `QfcHomeController.cs` research §5 | `internal IUiDispatcher UiDispatcher { get; set; }` | No |
| S4 `LaunchCoreAsync` (Tier C) | `QfcHomeController.cs` research §5 | `internal async Task<QfcHomeController> LaunchCoreAsync(...)` | No |
| S5a/S5b viewer + scheduler loaders (Tier C) | `QfcHomeController.cs` research §5 | `internal Func<IQfcFormViewer>` / `internal Func<TaskScheduler>` | No |
| Metrics S1 `MetricsAdder` | `QfcHomeController.Metrics.cs` research §5 | `internal Func<string,int,CancellationToken,bool>` | No |
| Metrics S2 `MetricsLineWriter` | `QfcHomeController.Metrics.cs` research §5 | `internal Action<string,string[],string>` | No |
| Metrics S3 `BuildDurationTexts` | `QfcHomeController.Metrics.cs` research §5 | `internal static (double,string,string) BuildDurationTexts(...)` | No |
| Metrics S5 visibility widening | `QfcHomeController.Metrics.cs` research §5 | `private` → `internal` on both `NonBlockingProducer` overloads | No |

**Every one of the nine proposed seams is `internal` on the class.** The enabling mechanism is
already in place: `[assembly: InternalsVisibleTo("QuickFiler.Test")]` at
`QuickFiler\Controllers\QfcHomeController.cs:18` makes every `internal` member directly reachable
from the test project without reflection, and
`QuickFiler\Controllers\QfcHighConfidencePreFilter.cs:11` additionally declares
`[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` so Moq can proxy internal QuickFiler
types where needed.

Adding an `internal` member to `QfcHomeController` does **not** require a corresponding declaration
on any interface. C# interfaces cannot declare `internal` members that a public implementer must
satisfy, and no consumer reaches the seams through an interface reference — the tests hold the
concrete `QfcHomeController`. The seven existing loader seams
(`QfcDataModelLoader`, `QfcAsyncDataModelLoader`, `QfcExplorerControllerLoader`,
`QfcKeyboardHandlerLoader`, `QfcQueueLoader`, `QfcFormControllerLoader`,
`HighConfidencePreFilterLoader`, at `QfcHomeController.cs:159-244`) plus the injectable
`TimeProvider` (`QfcHomeController.Metrics.cs:17`) are the ratified in-repo precedent: **none of them
appears on `IQfcHomeController` or `IFilerHomeController`.** The pattern this child needs is already
proven to work without an interface edit.

**Conclusion: interface modification is unnecessary for every seam the sibling F7 research artifacts
actually propose.** The planner should treat `IQfcHomeController.cs` as a read-only file for this
child.

### 6.2 Blast radius if the interface were modified anyway

Recorded so the planner can price the option it should not take.

| Change | Immediate compile impact | Children affected |
| --- | --- | --- |
| **Add** a member | `QfcHomeController` (F7) must implement it — that is the only implementer (§5.1), so the direct cost is F7-local. **But**: `Mock<IQfcHomeController>` in `QfcFormControllerTests.cs` and `QfcFormControllerSeamTests.cs` (F6-owned test files) would silently start returning `default` for the new member; any F6 code path that later consumes it would get a null/false without a `Setup`. Low compile risk, non-zero behavioral risk to F6's suite. | F7 direct, **F6** latent |
| **Remove or rename** a member | Breaks `QfcFormController.cs:47`, `:48` and `QfcFormController.EventHandlers.cs:142,162,191,196,199,252,372,373` with CS1061 — all **F6-owned**. Also breaks `RibbonController.cs:107` for `Init()`. | **F6** hard break, TaskMaster |
| **Change a member signature** | Same F6 surface, plus the two F6-owned test files' `Setup`/`SetupGet` expressions. | **F6** hard break |
| **Widen the base list** (e.g. add a second base interface) | Forces `QfcHomeController` to satisfy it. If the added base were satisfied only by EFC-shaped members, it would collide with §6.3. | F7, possibly **F8** |

### 6.3 Sibling-owned types named in this contract (type references, not implementations)

`IQfcHomeController` references one sibling-owned type in a member signature:

| Referenced type | Line | Owning child | Note |
| --- | --- | --- | --- |
| `IQfcDatamodel` (`QuickFiler/Interfaces/IQfcDatamodel.cs`) | 11 | **F5** | Return type of the `DataModel` property. If F5 changes `IQfcDatamodel`'s *members*, this file is unaffected (only the type name is referenced). If F5 **renamed or removed** `IQfcDatamodel`, line 11 would break. **No addition is requested of F5.** F7 asks only that the type name remain. |

`Task` (lines 15, 17) is BCL. `IQfcHomeController` (line 12) is self-referential.

### 6.4 CROSS-CHILD CONTRACT NOTE — CC-A1

**Do not add a member to `IQfcHomeController` unilaterally.** Although `QfcHomeController` (F7) is
the sole implementer, the compile surface that would break on any *removal or signature change* is
entirely **F6**-owned (`QfcFormController` and its four partials, plus two F6 test files), and
`Init()`'s return participates in an upcast consumed by `TaskMaster/Ribbon/RibbonController.cs:107`
outside the epic. Any change here must be coordinated with F6 through the epic orchestrator, not
executed inside F7. Per §6.1 no such change is needed.

---

## 7. Partial-split impact assessment

The `QfcHomeController.cs` research recommends **Split 1 (mandatory)**: moving the entire
`#region Public Properties` block, source lines 406-485 of `QfcHomeController.cs`, into a new partial
file `QuickFiler/Controllers/QfcHomeController.Properties.cs`. Two members of **this** interface are
in that block:

| `IQfcHomeController` member | Current location | Moves under Split 1? |
| --- | --- | --- |
| `DataModel` (line 11) | `QfcHomeController.cs:428-433` | **Yes** |
| `WorkerComplete` (line 18) | `QfcHomeController.cs:472-477` | **Yes** |
| `Init()` (12) | `QfcHomeController.cs:89-109` | No |
| `Iterate()`, `Iterate2()`, `IterateQueueAsync()`, `SwapStopWatch()` (13-16) | `QfcHomeController.Iteration.cs` | No |
| `WriteMetricsAsync` (17) | `QfcHomeController.Metrics.cs:90-155` | No |
| (`Loaded`, from the base interface, at `QfcHomeController.cs:399-404`) | stays in the main file per the stated 406-485 range | No |

### 7.1 Effect on the interface contract: **none.** Reasoning stated explicitly.

1. **A partial class is one type.** The C# language specification treats the parts of a partial type
   declaration as a single declaration: the member set is the union of all parts, and the base-class
   / interface list is the union of all parts' base lists. Compilation produces exactly one
   `TypeDef` for `QuickFiler.Controllers.QfcHomeController` regardless of how many `.cs` files
   declare parts of it.
2. **Interface implementation mapping is per-type, not per-file.** The CLR `MethodImpl` /
   interface-map metadata binds `IQfcHomeController.get_DataModel` to
   `QfcHomeController.get_DataModel` by type and member, with no notion of source file. Relocating
   the accessor's source text changes only the sequence points recorded in the PDB — that is,
   *which file the coverage report attributes the lines to* — not the emitted contract.
3. **Consumers are unaffected.** Every consumer in §5.3 binds through `IQfcHomeController` or through
   the `QfcHomeController` type name. None references a file. `QfcFormController.EventHandlers.cs:196`
   (`_parent.DataModel`) and `:252` (`_parent.WorkerComplete`) compile identically before and after.
4. **`InternalsVisibleTo` is assembly-scoped, not file-scoped.** The `internal` setter on `DataModel`
   and the `private` setter on `WorkerComplete` retain their visibility semantics after the move; the
   `[assembly: InternalsVisibleTo("QuickFiler.Test")]` attribute at `QfcHomeController.cs:18` applies
   to the assembly and must simply stay in *some* compiled file (the sibling research already flags
   keeping it in `QfcHomeController.cs`).
5. **The existing reflection-based tests are unaffected.** They resolve members via
   `_controller.GetType()`, which returns the single `QfcHomeController` type object regardless of
   source partitioning.

**Therefore: Split 1 requires no edit to `IQfcHomeController.cs`, and this artifact raises no
objection to it on contract grounds.**

### 7.2 Would the new partial file itself be `testable` or interface-only under the F1 ledger?

**`testable`.** `QfcHomeController.Properties.cs` would be a *class* file containing real property
accessors, backing fields, and the `CreateCancellationToken()` method body — all IL-producing. It
would appear in the Cobertura report as source lines attributed to
`QuickFiler.Controllers.QfcHomeController` with `filename="QuickFiler\Controllers\QfcHomeController.Properties.cs"`,
exactly as the three existing partials do today. The interface-only carve-out in
`.claude/rules/general-unit-test.md` does not apply to it. The `QfcHomeController.cs` research sizes
it at 22 coverable lines, 18 covered today, reaching 100% once its proposed TC2 covers
`CreateCancellationToken`.

### 7.3 CROSS-CHILD CONTRACT NOTE — CC-A2 (ledger mechanics for a file that does not exist yet)

F1's ledger is being authored now against the **121 files currently compiled**.
`QfcHomeController.Properties.cs` will not be in it. The planner must decide, and F1 must be asked,
how a child adds a ledger row for a file it creates mid-wave — otherwise the capstone F16
("every one of the 121 compiled files is either at >= 80% or on the ledger") will encounter a file
with no row. This affects several wave-1 children simultaneously (F9, F11 and F13 are all expected
to create partial files). Recorded here because it is the direct downstream consequence of §7.1
being answered "yes, the split is safe."

Related, from the sibling artifact: any new partial requires a `<Compile Include=...>` entry in
`QuickFiler/QuickFiler.csproj` near lines 325-327, a known merge-conflict hotspot for wave 1.

---

## 8. Risks and open questions

### Risks

- **R1 — F1 has not landed.** The ledger and the harness are both upstream and absent (verified: the
  epic directory contains only `epic.md`). Gate the plan's Phase 0 on reading the ledger row for this
  file. If F1 classifies it `testable`, §3 and §4 are void.
- **R2 — the file-name collision is a real planning hazard.** Two files named `IQfcHomeController.cs`
  exist (§1). An agent that greps by file name and edits the wrong one would modify a **dead,
  uncompiled** file and produce a silently-green toolchain with no effect, or would edit the live
  one believing it to be dead. Any plan task that names this file must use the full path
  `QuickFiler/Controllers/IQfcHomeController.cs`.
- **R3 — "no work" tasks attract make-work.** A file with a zero-line denominator invites an
  implementer to invent a test to make the child look complete. §4.1 rejects shape-assertion tests on
  the record so that a reviewer has a citable basis to reject one if it appears in a PR.
- **R4 — indirect exposure through F6.** F7 does not edit this file, but F6 is refactoring
  `QfcFormController` (epic F6, 10 files including `QfcFormController.EventHandlers.cs`, which is the
  heaviest consumer of this interface). If F6's refactor changes how it consumes
  `SwapStopWatch`/`IterateQueueAsync`/`WorkerComplete`, F7's `QfcHomeController` tests that pin those
  members' behavior could be affected at integration-merge time even though neither child edits this
  file. Mitigation: the epic orchestrator rebases the integration branch before each wave; treat this
  as a merge-time watch item, not a planning blocker.
- **R5 — Cobertura evidence is indicative, not F1 harness output.** §2.3 reads a committed artifact
  produced by a different feature (#424). It is strong evidence of the *structural* fact (the
  instrumenter emits nothing for this file), and that fact does not depend on which tests ran. It is
  nevertheless not F1 harness output and must not be cited as acceptance evidence.

### Open questions for the planner / F1

1. **Ledger classification.** Does F1 classify `QuickFiler/Controllers/IQfcHomeController.cs` as
   `interface-only / not-measured`? (Recommended; evidence in §2-§3.)
2. **Ledger row format for zero-line files.** Does F1 want a per-file coverage number recorded for
   interface-only files (`0/0`, or "not measured"), and does the harness emit a row for a file the
   Cobertura report omits entirely? This determines what W2 in §4 actually commits as evidence.
3. **New-file ledger rows (CC-A2).** How does a wave-1 child register `QfcHomeController.Properties.cs`
   in a ledger authored against the pre-existing 121 files?
4. **Orphan-file disposition.** Should `QuickFiler/Interfaces/IQfcHomeController.cs` (uncompiled, §1)
   be promoted as a cleanup issue, and if so does it belong to F7, F1, or the capstone F16?
5. **`Iterate2()` dead member (§5.4).** Report-only here. Confirm the orchestrator wants it promoted
   as an interface-segregation cleanup issue rather than removed inside a coverage child.
