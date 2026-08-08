# Research: `QuickFiler/Interfaces/IQfcDatamodel.cs`

- Feature: `quickfiler-datamodel-coverage` (issue #436), child F5 of epic `quickfiler-per-file-coverage` (#136)
- Target file: `QuickFiler/Interfaces/IQfcDatamodel.cs` — 59 lines, no `[ExcludeFromCodeCoverage]`
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a923053598cf4ccea`
- Created: 2026-08-08T00-43
- Scope: this one production file. It is the cross-child contract file of F5.
- Companion artifacts (read first, built upon and in two places corrected here):
  - `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/research/2026-08-08T00-43-qfcdatamodel.md`
  - `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/research/2026-08-08T00-43-qfcdatamodel-queueprocessing.md`
  - `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/research/2026-08-08T00-43-efcdatamodel.md`

---

## 0. Executive summary

1. **Recommendation: F5 makes zero production edits to this file.** Every seam proposed by every sibling
   F5 artifact lands on the concrete `QfcDatamodel` class (or its proposed new partial) or on
   `EfcDataModel`. None requires an interface member. Verified by construction in §5, not assumed.
2. **Question A — coverage classification, answered from measurement, not speculation.** The committed
   Cobertura report contains **no `<class>` element** for `QuickFiler.Interfaces.IQfcDatamodel` and **no
   `<class>` element** for `QuickFiler.Interfaces.SortOptionsEnum`. `SortOptionsEnum` appears in that
   report only as a *parameter type name* inside two `EmailSorter` method signatures (report lines 19260,
   19288). The file's coverage today is therefore **not 0% — it is absent from the report entirely**, and
   the enum does not change that (§2.1). Recommended ledger disposition: a **third** category,
   `not-measurable (declaration-only)`, distinct from both `testable` and `ratified-exempt`, and it must
   **not** be recorded under the CLAUDE.md § UT2 COM/VSTO exemption (§2.3).
3. **Question B — the "no contract note required" conclusion is CONFIRMED, but the evidence base it
   rested on was incomplete.** The `QfcDatamodel.cs` agent named `QfcHomeController.cs:163` and `:173`.
   Those two are verified correct (§4.1). It **missed two production consumers of interface members**:
   `QfcQueue.cs:476` (sibling **F2**) and `QfcFormController.EventHandlers.cs:196` (sibling **F6**).
   Neither is broken by seams S1–S5, so the conclusion survives — but the constraint set in §4.4 is wider
   than the two call sites that agent cited.
4. **issue.md contains a factual error that the planner must not carry forward.** `issue.md:73-74` states
   `IQfcDatamodel` is consumed by "the home controller (sibling F7) and the collection controller
   (sibling F11)". A grep of `QuickFiler/Controllers/QfcCollectionController.cs` (2,349 lines) for
   `DataModel|Datamodel|_datamodel` returns **zero matches**. **F11 is not a consumer.** The real
   unanticipated consumers are **F2** and **F6** (§3.3).
5. **`IQfcDatamodel` has exactly ONE compiled implementer** — `QfcDatamodel` (`QfcDatamodel.cs:26`) —
   which F5 itself owns. `EfcDataModel` does not implement it (independently verified, §3.2). So adding an
   interface member would not break a sibling *at compile time*; the reason to prohibit it is different and
   is stated precisely in §4.4.
6. **`SortOptionsEnum` is a second, undocumented cross-child contract — with F2, not F7 or F11.** Its only
   interpreter is `EmailSorter.cs:45-48` (**F2**); its only production caller is
   `QfcDatamodel.FrameBuilding.cs:114` (**F5, own**). `Default = 42` decomposes to `32 + 8 + 2` and that
   decomposition is **load-bearing and pinned by no test** (§2.4). This is the one thing in this file that
   warrants test authoring.
7. **Three test cases are warranted** (§7) — all characterization tests of the enum's numeric contract.
   They earn **zero line-coverage credit for this file**, and the artifact says so explicitly rather than
   implying a coverage benefit.
8. **Three observations are recorded as promote-to-issue, not fix** (§8), following the precedent both
   sibling agents set under AC7. Two are dead interface members; one is a misleading identifier in an
   F2-owned file.

---

## 1. Method and evidence basis

Every claim is grounded in a file read or a grep executed in this session. Anything not verifiable without
building or running is marked **INFERRED** with the reason.

| Path | Purpose |
| --- | --- |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` (all 59 lines) | subject |
| `QuickFiler/Controllers/EmailSorter.cs` (all 85 lines) | the only interpreter of `SortOptionsEnum` flags (F2) |
| `QuickFiler.Test/Controllers/EmailSorterTests.cs` (all 89 lines) | existing enum-adjacent tests |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:95-154` | the only production caller of `SortOptionsEnum.Default` |
| `QuickFiler/Controllers/QfcHomeController.cs:150-189, 248-307` | the two ctor/factory bind sites and the 4-arg overload call site |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs:10-71` | three interface-member call sites |
| `QuickFiler/Controllers/QfcQueue.cs:465-489` | the F2 consumer the sibling artifact missed |
| `QuickFiler/Controllers/IQfcHomeController.cs` (all 20 lines) | the interface that re-exposes `IQfcDatamodel` |
| `docs/.../424/evidence/qa-gates/coverage-final.cobertura.xml` | measured coverage representation of interfaces and enums |
| `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj` | explicit `<Compile Include>` verification |
| `CLAUDE.md`, `.claude/rules/general-unit-test.md`, `docs/features/epics/quickfiler-per-file-coverage/epic.md`, `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/issue.md` | policy and contract |
| the three sibling F5 research artifacts | composition baseline |

Greps executed (all `*.cs` unless noted):

- `IQfcDatamodel` — 30 hits across 15 files.
- `SortOptionsEnum` — **12 hits across exactly 4 `.cs` files** (count-mode grep).
- `DequeueNextItemGroup|InitEmailQueue|MovedItems|UndoMove|\.Complete\b|\.Cleanup\(\)` scoped to `QuickFiler/**/*.cs`.
- `MoveEmailsAsync|_movedItems|MovedMails` scoped to `QuickFiler/**/*.cs`.
- `ConversationUniqueOnly|TriageIgnore|DateOldestFirst|TriageImportantLast|SortTriageDate|MostRecentByConversation` scoped to `QuickFiler*/**/*.cs`.
- `DataModel|Datamodel|_datamodel` scoped to `QuickFiler/Controllers/QfcCollectionController.cs` — **zero matches**.
- Four structural greps against the Cobertura report (§2.1).

Upstream F1 (`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` and the per-file harness
derived from `scripts/vscode/Invoke-MSTestWithCoverage.ps1`) does not exist on disk. That is expected — F1 is
prepared concurrently — and is not a blocker for this file, because §2 resolves the classification question
from measured data rather than from the harness.

---

## 2. Question A — coverage classification

### 2.1 What the coverage tool actually does with interfaces and enums — measured

Four structural searches of
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
(a committed full-suite report; `main` at `74be1964` has only documentation merges since, so it is treated as
current):

| Search | Result |
| --- | --- |
| `QuickFiler\.Interfaces\.(IQfcDatamodel\|SortOptionsEnum)` and bare `SortOptionsEnum` | 3 hits, **all three are method-signature text**: `.ctor` signature `(QuickFiler.Interfaces.SortOptionsEnum)` at report line 19260, `set_Options` signature `(QuickFiler.Interfaces.SortOptionsEnum)` at 19288 (both inside the `EmailSorter` class element), and `set_DataModel` signature `(QuickFiler.Interfaces.IQfcDatamodel)` at 21903. **No `<class>` element for either type.** |
| `filename="QuickFiler\\Interfaces\\` | **exactly one** hit — `<class ... name="QuickFiler.Interfaces.MailItemActionsAdapter" filename="QuickFiler\Interfaces\MailItemActionsAdapter.cs">` at report line 14448. That is a concrete adapter class with executable behavior, not a declaration-only file. |
| `name="QuickFiler\.[A-Za-z.]*(IQfc\|IEmail\|IItem\|IKbd\|IMail\|IConversation\|IBreadcrumb\|IWebView\|IFiler)[A-Za-z]*"` | **no matches.** Not one QuickFiler interface type appears as a `<class>` element anywhere in the report. |
| `Enum" filename=\|Options" filename=\|Mode" filename=` | **no matches.** No enum type appears as a `<class>` element anywhere in the report. |

The report is genuinely instrumenting this assembly — `<class ... name="QuickFiler.Controllers.EmailSorter"
filename="QuickFiler\Controllers\EmailSorter.cs" line-rate="0.9591836734693877">` is present at report line
19238, and the report header (line 2) records `lines-valid="110849"`. So the absence is a property of the
declaration kind, not of the assembly.

**Verified conclusion.** The instrumenter emits `<class>` elements only for types with method bodies. An
interface declaration and a `[Flags]` enum declaration produce no instrumented lines, so
`QuickFiler/Interfaces/IQfcDatamodel.cs` contributes **no filename key at all** to the Cobertura output.
Any per-file report F1 derives from that Cobertura output by grouping on `filename` will therefore have **no
row for this file** — not a 0% row.

Consequence for F5's acceptance criteria: `issue.md:59` requires "every `testable` file … reaches at least
80% line coverage, verified with F1's per-file coverage harness". This file cannot produce a number to
verify. It can neither pass nor fail an 80% line-coverage gate.

### 2.2 The policy clause, quoted

`.claude/rules/general-unit-test.md` § Coverage Requirements, final bullet, verbatim:

> Type-only / interface-only modules with no executable behavior may be omitted from coverage measurement.
> Examples: Python `Protocol`-only modules consumed only under `TYPE_CHECKING`, TypeScript interface/type-only
> files, and C# interface-only files. Such modules legitimately report 0% executable coverage and may be
> excluded from measurement. This is a clarification only; it does not lower any coverage threshold.

Two points matter for the planner:

- The clause names **"C# interface-only files"** explicitly. This file is one.
- The clause's mechanism is *omission from measurement*, and it closes with "This is a clarification only; it
  does not lower any coverage threshold." That wording is deliberate: omitting a file with no executable
  behavior does not weaken the metric, because it removes nothing from the numerator or the denominator. It is
  a different mechanism from the § Coverage Exclusion Policy prohibition in the same file ("No production file
  may be excluded from coverage measurement"), which targets `exclude` entries that hide **production runtime
  code** from an otherwise-instrumented tool. Nothing is being hidden here — the tool emits nothing to hide.

The measured evidence in §2.1 shows the tool already implements this clause implicitly: it never instruments
a QuickFiler interface or enum in the first place.

### 2.3 Does the enum change the answer? No — and the distinction that matters

The enum adds no executable behavior. A C# enum compiles to a `System.Enum`-derived type whose members are
`static literal` fields resolved at compile time; it has no method bodies and no sequence points. §2.1's
fourth grep confirms this empirically for the whole report: no enum type appears as a `<class>` element
anywhere in 110,849 instrumented lines.

Two specific traps to avoid:

- **The enum is not a reason to reclassify the file as `testable`.** A test that reads
  `SortOptionsEnum.Default` executes IL in the *test* assembly (and in `System.Enum.HasFlag`), never in this
  file. No test can raise this file's measured line coverage, because there is no line to raise.
- **This file must NOT be recorded under the CLAUDE.md § UT2 COM/VSTO/WinForms exemption.** That exemption
  covers "(c) Outlook Interop event handler classes … that directly depend on
  `Microsoft.Office.Interop.Outlook.Application`, `MailItem`, `Store`, or `MAPIFolder` without an injectable
  seam". This file *does* reference `MailItem` (via `using Microsoft.Office.Interop.Outlook;` at line 6, in
  the signatures at lines 26, 40, 46, 49, 50), which makes the mis-classification tempting. It does not
  qualify: it is not a class, it is not an event handler, and it has no behavior to seam. Recording it under
  § UT2 would (a) invoke the maintainer-ratification requirement for no reason, and (b) invite an
  `[ExcludeFromCodeCoverage]` attribute that excludes nothing. Under epic.md § Shared Design 1, an
  `[ExcludeFromCodeCoverage]` on a testable seam is a Blocking finding; an attribute here is not that, but it
  is noise on a reviewable surface and is prohibited by §4.4 below.

### 2.4 `Default = 42` — decomposition verified, and it is load-bearing

Declaration (`IQfcDatamodel.cs:12-22`):

```csharp
[Flags]
public enum SortOptionsEnum
{
    Default = 42,
    TriageIgnore = 1,
    TriageImportantFirst = 2,
    TriageImportantLast = 4,
    DateRecentFirst = 8,
    DateOldestFirst = 16,
    ConversationUniqueOnly = 32,
}
```

Decomposition **verified**: `42 = 0b101010` sets bits 1, 3 and 5, i.e. `2 + 8 + 32` =
`TriageImportantFirst | DateRecentFirst | ConversationUniqueOnly`. It does **not** include `TriageIgnore`
(1), `TriageImportantLast` (4), or `DateOldestFirst` (16).

**How `Default` is actually consumed** — the question that decides whether a characterization test is
warranted:

- **Sole production caller:** `QfcDatamodel.FrameBuilding.cs:114` —
  `var sorter = new EmailSorter(SortOptionsEnum.Default);` inside
  `public Frame<int, string> SortTriageDate(Frame<int, string> df)` (line 112). `SortTriageDate` is invoked
  unconditionally on both frame-build paths: `FrameBuilding.cs:24` (`InitDf`) and `:63` (`InitDfAsync`).
- **Sole interpreter of the flags:** `EmailSorter.GetSortKey` (`EmailSorter.cs:43-68`). Its only predicate is
  the conjunction at lines 45-48:

  ```csharp
  if (
      _options.HasFlag(SortOptionsEnum.TriageImportantFirst)
      && _options.HasFlag(SortOptionsEnum.DateRecentFirst)
  )
  ```

  If the conjunction holds it returns `100000000000000 * _triageImportantLast[triage] + GetDateKey(dateTime)`;
  otherwise it returns `-1` (line 67).
- **Therefore `Default`'s two low bits are exactly the two bits the sorter requires.** `2` and `8` are both
  set, so the default path produces real composite sort keys.

**Regression shape if `Default` changed.** Every row's key becomes `-1`. `SortTriageDate` then runs
`SortRows("NewKey")` on an all-equal column, reassigns the row index to `Range(0, RowCount).Reverse()`
(line 126) and re-sorts by that key (line 128) — an unconditional reversal. So the frame would not merely
lose its triage/date ordering; it would emerge in reverse of whatever tie order Deedle produced. The exact
resulting order is **INFERRED** (Deidle's tie-break stability was not verified), but the conclusion that the
intended triage/date ordering is silently lost is verified from the code above.

**Nothing pins this today.** `EmailSorterTests.cs:19` asserts only `sorter.Options.Should().Be(SortOptionsEnum.Default)`
— a tautology against whatever `Default` happens to be. Every test that exercises `GetSortKey`
(`EmailSorterTests.cs:62` and `:78`) constructs `TriageImportantFirst | DateRecentFirst` **explicitly rather
than using `Default`**, so the connection between `Default` and the sorter's predicate is asserted nowhere.
That gap is the justification for §7.

**Third bit is inert.** `ConversationUniqueOnly` (32) is inside `Default` but is never read: a repo-wide grep
for `ConversationUniqueOnly` returns only the declaration at `IQfcDatamodel.cs:21`. The unique-by-conversation
filter it names, `MostRecentByConversation`, runs **unconditionally** at `FrameBuilding.cs:21` and `:59` with
no flag test. Recorded as observation O2 (§8).

### 2.5 Recommended ledger disposition

Record in `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`:

| Field | Value |
| --- | --- |
| File | `QuickFiler/Interfaces/IQfcDatamodel.cs` |
| Lines | 59 |
| Classification | **`not-measurable (declaration-only)`** |
| Basis | `.claude/rules/general-unit-test.md` § Coverage Requirements, "Type-only / interface-only modules with no executable behavior may be omitted from coverage measurement … C# interface-only files." |
| Evidence | The committed Cobertura report emits no `<class>` element for `QuickFiler.Interfaces.IQfcDatamodel` or `QuickFiler.Interfaces.SortOptionsEnum`, and no `<class>` element for **any** QuickFiler interface or for any enum (§2.1). |
| 80% obligation | **None.** The file yields no `filename` key, so no percentage exists to gate. |
| `[ExcludeFromCodeCoverage]` | **Must not be added.** Nothing to exclude; would be reviewable noise on a public contract file. |
| § UT2 COM/VSTO exemption | **Does not apply.** Not a class, not an event handler, no behavior to seam — despite the `MailItem` references in its signatures (§2.3). |
| Owning child for the disposition | F5 asserts it; **F1's ledger is authoritative on arrival**. |

Two notes for F1 specifically:

1. `not-measurable (declaration-only)` should be a distinct third category, not a variant of
   `ratified-exempt`. Collapsing it into `ratified-exempt` would inflate the exemption ledger with roughly
   **24 files** (epic.md § Scope: "~24 are interface-only declarations with no executable behavior") that need
   no maintainer ratification and carry no irreducible-remainder argument, obscuring the exemptions that do.
2. F1's harness should treat "file present in `QuickFiler.csproj` but absent from the Cobertura `filename`
   set" as a classification signal to be reconciled against the ledger, not as 0%. Reporting these ~24 files
   as 0% would create ~24 permanent, unfixable gate failures. Flagged as an open question for F1 in §9 (Q1).

---

## 3. Question B.1–B.2 — the contract surface and the true consumer/implementer map

### 3.1 Full current contract surface

`namespace QuickFiler.Interfaces`. `public interface IQfcDatamodel` (line 24). **Nine members.** Usings at
lines 1-8 (`System`, `System.Collections.Generic`, `System.ComponentModel`, `System.Threading`,
`System.Threading.Tasks`, `Microsoft.Office.Interop.Outlook`, `UtilitiesCS`,
`UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable`).

| # | Member | Line(s) | Implementing file | Documented / verified behavior |
| --- | --- | --- | --- | --- |
| C1 | `Task<IList<MailItem>> DequeueNextItemGroupAsync(int quantity, int timeOut)` | 26 | `QueueProcessing.cs:66-76` | Delegates to C2 with `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline` and a `null` progress sink. No XML doc on the member itself; the relationship is documented on C2. |
| C2 | `Task<IList<MailItem>> DequeueNextItemGroupAsync(int quantity, int timeOut, TimeSpan firstBatchDeadline, Action<int,int,int> progress)` | 40-45, with the XML doc block at 28-39 | `QueueProcessing.cs:78-99` | **The issue-#424 overload.** Its XML doc is the only prose contract in the file and is quoted in full below. |
| C3 | `IList<MailItem> DequeueNextItemGroup(int quantity)` | 46 | `QueueProcessing.cs:132-143` | Synchronous sibling of C2. |
| C4 | `void UndoMove()` | 47 | `QueueProcessing.cs:24-27` | Unconditionally throws `NotImplementedException`; carries a `//TODO` at `QueueProcessing.cs:23`. **Zero production consumers** (§3.3). |
| C5 | `SloStack<IMovedMailInfo> MovedItems { get; }` | 48 | `QfcDatamodel.cs:141-144` | `=> _globals.AF.MovedMails`. Get-only. **Zero production consumers** (§3.3). |
| C6 | `IList<MailItem> InitEmailQueue(int batchSize, BackgroundWorker worker)` | 49 | `QfcDatamodel.cs:241-285` | `batchSize <= 0` short-circuits (issue #244) and returns an empty list after starting the worker. |
| C7 | `Task<IList<MailItem>> InitEmailQueueAsync(int batchSize, BackgroundWorker worker, CancellationToken token, CancellationTokenSource tokenSource)` | 50-55 | `QfcDatamodel.cs:287-303` | Throws if `token` is already cancelled; stores `_token`/`_tokenSource`/`_worker`; runs C6 on `Task.Run`. |
| C8 | `bool Complete { get; set; }` | 56 | `QfcDatamodel.cs:134-139` | Get/**set** — the only settable member on the interface. Plain backing field. |
| C9 | `void Cleanup()` | 57 | `QfcDatamodel.cs:75-91` | Cancels the token source and worker, unsubscribes `NewMailEx`, calls `_moveMonitor.UnhookAll()`, then nulls seven fields. Not idempotent (sibling artifact risk R5). |

The C2 XML doc block (`IQfcDatamodel.cs:28-39`), verbatim — this is the documented relationship between the
two overloads that the delegation task must preserve:

```
/// Issue #424 overload carrying the dequeue-gate first-batch deadline and an optional
/// incremental progress sink. The two-argument overload delegates here with
/// <c>QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline</c> and a null sink.
/// <param name="firstBatchDeadline">
/// Overall budget for assembling the first batch. <c>Timeout.InfiniteTimeSpan</c> disables it.
/// </param>
/// <param name="progress">
/// Optional sink invoked once per scored candidate with <c>(scanned, accepted, quantity)</c>.
/// Exceptions thrown by the sink propagate. Ignored outside high-confidence mode.
/// </param>
```

Each of the four documented claims is verified against the implementation:

- "The two-argument overload delegates here with `DefaultFirstBatchDeadline` and a null sink" —
  `QueueProcessing.cs:70-75`. Matches invariant I1 in the QueueProcessing artifact.
- "`Timeout.InfiniteTimeSpan` disables it" — the parameter flows `QueueProcessing.cs:92` → `:124` → the gate
  constructor. Verified as flow, not as gate semantics (the gate is F2-owned and was read only by the
  sibling agent). **Partly INFERRED** on the gate side.
- "invoked once per scored candidate with `(scanned, accepted, quantity)`" — same flow; gate-side semantics
  per the sibling artifact's I17.
- "Ignored outside high-confidence mode" — verified: `QueueProcessing.cs:97-98` routes to
  `DequeueDirectAsync(quantity)`, which takes only `quantity`, dropping both extra arguments. Matches I18.

**Sole production consumer of C2 is `QfcHomeController.cs:299-304`** (F7):

```csharp
var scanProgress = new QfcScanProgressBandMapper(progress.Report);
listEmail = await _datamodel.DequeueNextItemGroupAsync(
    itemsPerIteration,
    200,
    QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline,
    scanProgress.Report
);
```

Every other production dequeue call site uses C1 or C3.

### 3.2 Implementer map — exactly one compiled implementer

Repo-wide grep for `IQfcDatamodel` (30 hits, 15 files), partitioned:

| Kind | Location | Compiled? | Notes |
| --- | --- | --- | --- |
| **Declaration** | `QuickFiler/Interfaces/IQfcDatamodel.cs:24` | Yes — `QuickFiler.csproj:361` `<Compile Include="Interfaces\IQfcDatamodel.cs" />` | subject |
| **Implementer** | `QuickFiler/Controllers/QfcDatamodel.cs:26` — `public partial class QfcDatamodel : IQfcDatamodel` | Yes | **the only one**; F5-owned |
| Stale duplicate declaration | `QuickFiler/Notes/notes_interfaces.cs:26` — `public interface IQfcDatamodel` | **No** | `Notes/**` is absent from `QuickFiler.csproj` (epic.md § Scope, lines 108-110); confirmed by grep of the csproj for `notes_interfaces` — no hit. Outside the 121-file denominator. |
| Runtime-generated implementers | `Mock<IQfcDatamodel>` at `QfcHomeControllerTests.cs:122,188`; `QfcHomeControllerRunAsyncTests.cs:122,190,241`; `QfcHomeControllerRunAsyncHighConfidenceTests.cs:33,124,304,407`; `QfcHomeControllerPropertyTests.cs:83,181`; `QfcHomeControllerIterationTests.cs:81,127,190,261,318,369,408`; `QfcHomeControllerIssue218Tests.cs:89,193` | n/a (Castle DynamicProxy at runtime) | 19 sites in 6 test files, all F7-territory. **Moq generates the proxy at runtime, so adding an interface member does not break their compilation** — it changes their *behavior* (§4.4). |

**`EfcDataModel` does not implement `IQfcDatamodel` — independently verified.** The grep for `IQfcDatamodel`
across all `*.cs` returns no hit in `EfcDataModel.cs` or in any `Efc*` file. Combined with the EFC artifact's
read of `internal class EfcDataModel` (`EfcDataModel.cs:20`) declaring no base list, the two types share no
abstraction. **The true implementer count is 1, not 2** — narrower than F5's five-file scope table suggests.

### 3.3 Consumer map — verified, with two corrections to the feature documents

Production consumers, from the greps in §1:

| # | Consumer site | Members used | Owning epic child (epic.md § Feature File Assignments) |
| --- | --- | --- | --- |
| U1 | `QuickFiler/Controllers/IQfcHomeController.cs:11` — `IQfcDatamodel DataModel { get; }` | the type, as a property type | **F7** (`Controllers/IQfcHomeController.cs` (20), epic.md:303) |
| U2 | `QuickFiler/Controllers/QfcHomeController.cs:162` — return type of `Func<IApplicationGlobals, CancellationToken, IQfcDatamodel> QfcDataModelLoader` | the type | **F7** (epic.md:302) |
| U3 | `QuickFiler/Controllers/QfcHomeController.cs:170` — return type of `Func<…, Task<IQfcDatamodel>> QfcAsyncDataModelLoader` | the type | **F7** |
| U4 | `QuickFiler/Controllers/QfcHomeController.cs:428-429` — `private IQfcDatamodel _datamodel;` / `public IQfcDatamodel DataModel` | the type | **F7** |
| U5 | `QuickFiler/Controllers/QfcHomeController.cs:254` | **C6** `InitEmailQueue(initializationBatchSize, _formViewer.Worker)` | **F7** |
| U6 | `QuickFiler/Controllers/QfcHomeController.cs:261` | **C1** `DequeueNextItemGroupAsync(itemsPerIteration, 1000)` then `.GetAwaiter().GetResult()` | **F7** |
| U7 | `QuickFiler/Controllers/QfcHomeController.cs:284-289` | **C7** `InitEmailQueueAsync(batch, worker, Token, TokenSource)` | **F7** |
| U8 | `QuickFiler/Controllers/QfcHomeController.cs:299-304` | **C2** — the only production consumer of the 4-argument overload | **F7** |
| U9 | `QuickFiler/Controllers/QfcHomeController.cs:390` | **C9** `Cleanup()` | **F7** |
| U10 | `QuickFiler/Controllers/QfcHomeController.Iteration.cs:15` | **C8** `Complete` (read) | **F7** (epic.md:303) |
| U11 | `QuickFiler/Controllers/QfcHomeController.Iteration.cs:21-24` | **C1** `DequeueNextItemGroupAsync(ItemsPerIteration, 2000)`; result dereferenced at `:25` as `listObjects.Count` **with no null guard** | **F7** |
| U12 | `QuickFiler/Controllers/QfcHomeController.Iteration.cs:62-65` | **C1**, blocked with `.GetAwaiter().GetResult()` | **F7** |
| U13 | `QuickFiler/Controllers/QfcHomeController.Iteration.cs:66` | **C3** `DequeueNextItemGroup(ItemsPerIteration)` | **F7** |
| **U14** | `QuickFiler/Controllers/QfcQueue.cs:476-479` — `await _homeController.DataModel.DequeueNextItemGroupAsync(newRowCount - entry.ItemGroups.Count, 1000)`; result dereferenced at `:480` as `items.Count` **with no null guard** | **C1** | **F2** (`Controllers/QfcQueue.cs` (610), epic.md:257) — **not anticipated by `issue.md`** |
| **U15** | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:196` — `if (!_parent.DataModel.Complete)` | **C8** `Complete` (read) | **F6** (`Controllers/QfcFormController.EventHandlers.cs` (399), epic.md:294) — **not anticipated by `issue.md`** |
| U16 | `QuickFiler/Interfaces/IFilerHomeController.cs:29` — `//IQfcDatamodel DataModel { get; }` | none (commented out, inert) | **F7** (`Interfaces/IFilerHomeController.cs` (45), epic.md:304) |
| — | `QuickFiler/Interfaces/IQfcHomeController.cs:11` | the type | **NOT COMPILED.** `QuickFiler.csproj` contains only `<Compile Include="Controllers\IQfcHomeController.cs" />` (line 304); a grep of the csproj for `Interfaces\IQfcHomeController` returns no hit. An uncompiled duplicate of U1; correctly unassigned in epic.md. |
| — | `QuickFiler/Notes/notes_interfaces.cs:29` (`UndoMove`) | — | **NOT COMPILED.** |

**Correction 1 (must reach `spec.md`): F11 is not a consumer.** `issue.md:73-74` states the interface is
consumed by "the home controller (sibling F7) and the collection controller (sibling F11)". A grep of
`QuickFiler/Controllers/QfcCollectionController.cs` — the entire 2,349-line file that constitutes essentially
all of F11 — for `DataModel|Datamodel|_datamodel` returns **zero matches**. F11's other file,
`Interfaces/IQfcCollectionController.cs`, mentions `SloStack<IMovedMailInfo>` at line 50
(`Task MoveEmailsAsync(SloStack<IMovedMailInfo> StackMovedItems)`), but that is the same *UtilitiesCS* type
C5 returns, reached independently: the caller `QfcFormController.EventHandlers.cs:225` passes `_movedItems`,
which `QfcFormController.cs:49` obtains as `_globals.AF.MovedMails` — **not** via `IQfcDatamodel.MovedItems`.
So there is no path from F11 to this interface.

**Correction 2: the real unanticipated consumers are F2 and F6.** Both consume interface *members* through
`IQfcHomeController.DataModel`, so neither appears in a grep for `IQfcDatamodel` — which is precisely why the
sibling artifact missed them and why `issue.md` mis-attributed the risk.

**`MovedItems` (C5) and `UndoMove` (C4) have zero production consumers.** Verified by the two targeted greps
in §1: the only `MovedItems` hits in `QuickFiler/**/*.cs` are the declaration (`IQfcDatamodel.cs:48`), the
implementation (`QfcDatamodel.cs:141`), and two unrelated *parameter* names
(`IQfcCollectionController.cs:50`, `QfcCollectionController.cs:2206`); the only `UndoMove` hits are the
declaration (`:47`), the implementation (`QueueProcessing.cs:24`), the uncompiled
`notes_interfaces.cs:29`, and an unrelated `IMovedMailInfo.UndoMove()` /
`UndoMoveMessage` pair at `QfcFormController.Actions.cs:218,273`. Recorded as observation O1 (§8).

### 3.4 `SortOptionsEnum` consumer map — a second cross-child contract, with F2

A count-mode grep confirms `SortOptionsEnum` appears in **exactly 4 `.cs` files, 12 occurrences**:

| Site | Use | Owning child |
| --- | --- | --- |
| `QuickFiler/Interfaces/IQfcDatamodel.cs:13` | declaration | **F5** (this file) |
| `QuickFiler/Controllers/EmailSorter.cs:15` | `public EmailSorter(SortOptionsEnum options)` | **F2** (epic.md:262) |
| `QuickFiler/Controllers/EmailSorter.cs:20` | `private SortOptionsEnum _options = SortOptionsEnum.Default;` — the parameterless ctor's effective default | **F2** |
| `QuickFiler/Controllers/EmailSorter.cs:37` | `public SortOptionsEnum Options { get; set; }` | **F2** |
| `QuickFiler/Controllers/EmailSorter.cs:46-47` | `HasFlag(TriageImportantFirst) && HasFlag(DateRecentFirst)` — **the only interpretation of the flags anywhere in the repository** | **F2** |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:114` | `new EmailSorter(SortOptionsEnum.Default)` — the only production use of `Default` | **F5** (own) |
| `QuickFiler.Test/Controllers/EmailSorterTests.cs:19,26,27,62,78` | 5 test occurrences | F2 test territory |

This dependency is **absent from `issue.md`'s "Cross-child contract" constraint**, which names only F7 and
F11 (both wrong or partial). It is the one genuine cross-child coupling this file has beyond F7: the enum is
declared in an F5 file and interpreted exclusively in an F2 file. It is not a *breaking-change* risk — see
§6 — but it is a coordination note.

---

## 4. Question B.3–B.4 — verdict on the sibling conclusion, and hard planner constraints

### 4.1 The two call sites the `QfcDatamodel.cs` agent cited — verified correct

**`QfcHomeController.cs:159-163`** (read this session):

```csharp
internal Func<
    IApplicationGlobals,
    CancellationToken,
    IQfcDatamodel
> QfcDataModelLoader { get; set; } = (globals, cancel) => new QfcDatamodel(globals, cancel);
```

Seam **S3** replaces the body of `public QfcDatamodel(IApplicationGlobals, CancellationToken)` with a
`: this(appGlobals, token, null)` chain to a new `internal` 3-parameter constructor. The public
constructor's **arity, parameter types, order, and accessibility are unchanged**, so the lambda's
`new QfcDatamodel(globals, cancel)` binds to the same member with the same metadata signature.
**Verified additive.**

**`QfcHomeController.cs:165-173`**:

```csharp
internal Func<
    IApplicationGlobals,
    CancellationToken,
    CancellationTokenSource,
    ProgressTracker,
    Task<IQfcDatamodel>
> QfcAsyncDataModelLoader { get; set; } =
    async (globals, cancel, cancelSource, progress) =>
        await QfcDatamodel.LoadAsync(globals, cancel, cancelSource, progress);
```

Seam **S4** retains the `public static` 4-parameter `LoadAsync` verbatim as a delegating wrapper and adds an
`internal static` 5-parameter overload. The 4-argument call site is an exact-arity match to the retained
public member. **Verified additive.**

One constraint the sibling artifact did not state, which the planner must carry (§4.4 R4): the additive
overloads must use **distinct arity with no optional parameters**. If S4's fifth parameter were given a
default, `LoadAsync(globals, cancel, cancelSource, progress)` would become a 2-candidate resolution. C#
overload resolution does prefer the candidate with no omitted optional parameters, so it would still compile
against the intended member — but the outcome then depends on a tie-break rule rather than on arity, which is
a needless robustness cost on a cross-child bind site. Mandate distinct arity.

### 4.2 The call sites the sibling agent missed

The `QfcDatamodel.cs` artifact's § "Additivity confirmation" table cites only `QfcHomeController.cs:163`
(F7) and `:173` (F7). It does not mention:

- **U14 — `QfcQueue.cs:476` (F2)**, which calls **C1** through `_homeController.DataModel`.
- **U15 — `QfcFormController.EventHandlers.cs:196` (F6)**, which reads **C8** through `_parent.DataModel`.
- **U1 — `IQfcHomeController.cs:11` (F7)**, which re-exposes the interface as a public property type, making
  `IQfcDatamodel` part of `IQfcHomeController`'s own public surface.

The QueueProcessing artifact's § 6.2 does list `QfcQueue.cs:476` among "verified consumers that must not
break", so the epic-level knowledge exists — it is just absent from the file that drew the
"no contract note required" conclusion.

### 4.3 Verdict

**The `QfcDatamodel.cs` agent's conclusion — "No cross-child contract note for `spec.md` is required. No
breaking change is proposed." — is CONFIRMED for the interface.** Independently derived, not inherited:

1. Seams S1, S2 and S5 add `internal` instance properties to the concrete class. An interface is unaffected
   by members its implementer adds.
2. Seams S3 and S4 add `internal` overloads of a constructor and a static factory. **Neither the constructor
   nor the static factory is an `IQfcDatamodel` member** — the interface has no constructor and no static
   member (§3.1, C1–C9). So S3/S4 cannot touch the interface even in principle.
3. The QueueProcessing file needs **no production edit at all** (independently confirmed there), so it adds
   zero pressure.
4. The EFC file's seams land on a type that does not implement this interface (§3.2).
5. All nine members C1–C9 keep byte-identical signatures under every proposed seam, so U1–U16 — including the
   two missed consumers — bind unchanged.

**Qualification.** The conclusion is correct; the reasoning was incomplete. The constraint set the planner
must enforce is §4.4, which is derived from all sixteen consumer sites, not from two.

### 4.4 Additive-change rules — hard planner constraints

**F5 MUST NOT:**

- **R1. Modify, rename, reorder, or remove any of the nine members C1–C9**, or change any parameter type,
  parameter name, or return type. Sixteen production sites plus 19 `Mock<IQfcDatamodel>` setups in six
  F7-owned test files bind to this surface. Parameter *names* are included because they are part of a public
  interface's API even though no consumer currently uses named arguments (verified: U5–U15 are all
  positional).
- **R2. Add a member to `IQfcDatamodel`.** The compile cost is genuinely low — exactly one implementer, and
  F5 owns it (§3.2) — so this prohibition rests on three other grounds:
  1. **A silent cross-child test hazard.** Moq generates the proxy at runtime, so the six F7-owned test files
     would keep compiling, but every `Mock<IQfcDatamodel>` would return `default` for the new member —
     `null` for a reference type, `false` for `bool`, `null` (not `Task.CompletedTask`) for a `Task`. Any
     F7 test whose production path reaches the new member would fail with an NRE that points at F7's file,
     not at F5's change. This is exactly the kind of fan-in failure the epic's disjoint-file-set design
     (epic.md § Decomposition Rationale) exists to avoid.
  2. **Public-surface minimality.** `IQfcDatamodel` is `public`, and via U1 it is part of the public surface
     of `public interface IQfcHomeController`. Widening a public contract to enable a unit test inverts the
     seam hierarchy in epic.md § Shared Design 2 (interface seam > injectable delegate > adapter): the
     "interface seam" tier means *depending on* an abstraction, not *growing a shipped contract*.
  3. **It is unnecessary.** §5 shows every F5 testability need is met without it.
- **R3. Change `SortOptionsEnum` in any way** — no renamed members, no changed numeric values, no reordering,
  no new members, and specifically **no change to `Default = 42`**. F5's own `FrameBuilding.cs:114` and F2's
  `EmailSorter.cs:46-47` jointly depend on `Default` carrying bits 2 and 8 (§2.4). A new member would also
  need an explicit power-of-two value: because `Default = 42` is declared *first*, an appended member without
  an explicit initializer would take `ConversationUniqueOnly + 1 = 33`, colliding with three existing bits
  and breaking `HasFlag` for every consumer.
- **R4. Introduce an additive overload using optional parameters** on any member of `QfcDatamodel` bound by
  U2/U3 (see §4.1). Use distinct arity.
- **R5. Add `[ExcludeFromCodeCoverage]` to this file**, at type or member level (§2.3, §2.5).
- **R6. Modify any sibling-owned file to accommodate this one** — specifically `QfcQueue.cs`,
  `EmailSorter.cs`, `EmailSorterTests.cs` (F2); `QfcFormController.EventHandlers.cs` (F6);
  `QfcHomeController*.cs`, `IQfcHomeController.cs`, `IFilerHomeController.cs` and the six
  `QfcHomeController*Tests.cs` files (F7); `QfcCollectionController.cs` (F11); `coverage.config` or any
  shared build property file (F1).

**F5 MAY:**

- **A1. Make no change at all — this is the recommendation.** The file is 59 lines (no size pressure against
  the 500-line limit), needs no seam, and yields no coverage number.
- **A2. Add or extend XML documentation comments** on C1 and C3–C9. Zero behavioral and zero binary-contract
  risk; C2 already has the only doc block in the file. This is optional, is not required by any acceptance
  criterion, and adds diff surface to a cross-child contract file during a 14-child parallel wave. **Not
  recommended** unless the planner wants the dead-member observations O1 documented in code, which §8 argues
  should be a GitHub issue instead.
- **A3. Author the characterization tests in §7**, which touch only new test files plus the test csproj.

### 4.5 Would adding an interface member break the implementers? — verified count

Verified: **one** compiled implementer (`QfcDatamodel.cs:26`), F5-owned. So a new member would be a
one-file compile fix, entirely inside F5's own scope. That is a real finding and it is why R2 is justified on
the three *other* grounds above rather than on "it would break siblings" — a planner told only "it would break
siblings" would discover the claim is false and might then proceed. The correct constraint is: it compiles,
and it is still prohibited.

---

## 5. Question B.5 — the seam strategy that avoids this file entirely

**A strategy that never touches `IQfcDatamodel.cs` exists, and it is the one the sibling artifacts already
propose.** Evaluated honestly against what each F5 file actually needs:

| F5 production file | Its stated seam need | Where the seam lands | Interface touched? |
| --- | --- | --- | --- |
| `QfcDatamodel.cs` | S1 `internal IFolderScoringService ScoringService` (reuses the interface already at `QfcHighConfidencePreFilter.cs:130`) | `internal` instance property on the concrete class | No |
| `QfcDatamodel.cs` | S2 `internal Func<string, DialogResult> MessageBoxInvoker` | `internal` instance property | No |
| `QfcDatamodel.cs` | S3 `internal QfcDatamodel(globals, token, Func<Explorer, Frame<int,string>>)` + public `: this(...)` chain | additive `internal` constructor — **not an interface member** | No |
| `QfcDatamodel.cs` | S4 `internal static LoadAsync(..., dataFrameInitializer)` + public delegating wrapper | additive `internal static` overload — **not an interface member** | No |
| `QfcDatamodel.cs` | S5 `NewMailEx` subscribe/unsubscribe delegates (contingency only, if Moq cannot proxy the interop event) | `internal` instance properties | No |
| `QfcDatamodel.QueueProcessing.cs` | **none** — zero COM dereference; reuses S1, `TimeProvider`, `IEmailMoveMonitor` | n/a | No |
| `QfcDatamodel.FrameBuilding.cs` | not yet researched; the `DfDeedle`-bound members sit behind an `InternalsVisibleTo` wall (`UtilitiesCS/Properties/AssemblyInfo.cs:19-20` grants only `UtilitiesCS.Test` and `ToDoModel.Test`) | expected to be `internal` seams on the concrete class, or member-level exemptions | No — its testable members (`SortTriageDate`, `MostRecentByConversation`) are `public` on the concrete class and absent from the interface |
| `EfcDataModel.cs` | `internal` property-injected seams following the house style | a type that does not implement `IQfcDatamodel` | No |

Why widening the interface would be the wrong tool even if R2 did not exist: the interface exposes only the
nine *queue-orchestration* operations F7/F2/F6 consume. Every seam above injects a *collaborator* (a scorer, a
message-box invoker, a frame builder, a clock). Collaborator injection belongs on the concrete type's
construction surface, not on the consumer-facing contract — the two audiences are disjoint. Note that
`QfcDatamodel` already establishes exactly this house style: `internal TimeProvider TimeProvider`
(`QfcDatamodel.cs:108-112`) and `internal Func<CancellationToken, Task<bool>> RemainingEmailLoader`
(`:114-128`) are both `internal` property seams on the concrete class, and neither is on the interface.

A separate *new* interface (for example an `IQfcFrameSource`) was considered and is rejected: nothing needs
polymorphism across implementations here, and `.claude/rules/csharp.md` § DI Seams directs the smallest seam
that enables reliable unit testing. `internal` members plus the existing
`InternalsVisibleTo("QuickFiler.Test")` grant (`QuickFiler/Properties/AssemblyInfo.cs:5`) already suffice.

---

## 6. Question B.6 — unavoidable breaking change

**There is none. No breaking change to `IQfcDatamodel` or `SortOptionsEnum` is required, proposed, or
permitted by F5.** No cross-child contract note recording a breaking change is needed for `spec.md`.

Two **non-breaking** items should nevertheless be recorded in `spec.md`, because both correct or extend what
`issue.md` says. Ready to paste:

> ### Cross-child observations — `IQfcDatamodel` contract surface (F5)
>
> **No breaking change.** All nine `IQfcDatamodel` members and all seven `SortOptionsEnum` members keep
> byte-identical shapes. `QuickFiler/Interfaces/IQfcDatamodel.cs` receives **zero production edits** in this
> feature. Every F5 seam (S1–S5 on `QfcDatamodel`, plus the `EfcDataModel` seams) is an `internal` member,
> an additive `internal` constructor, or an additive `internal static` overload on a concrete class — none is
> an interface member.
>
> **Correction to this feature's `issue.md` § Constraints & Risks.** `issue.md` states that `IQfcDatamodel`
> is consumed by "the home controller (sibling F7) and the collection controller (sibling F11)". The F11
> claim is incorrect: a grep of `QuickFiler/Controllers/QfcCollectionController.cs` (2,349 lines) for
> `DataModel|Datamodel|_datamodel` returns zero matches. The verified consumer set is:
>
> | Consumer | Members used | Child |
> | --- | --- | --- |
> | `QfcHomeController.cs:162,170,254,261,284,299,390,428-429`; `QfcHomeController.Iteration.cs:15,21,63,66`; `IQfcHomeController.cs:11` | all of `DequeueNextItemGroupAsync` (both overloads), `DequeueNextItemGroup`, `InitEmailQueue`, `InitEmailQueueAsync`, `Complete`, `Cleanup` | **F7** |
> | `QfcQueue.cs:476` | `DequeueNextItemGroupAsync(int,int)` via `_homeController.DataModel` | **F2** |
> | `QfcFormController.EventHandlers.cs:196` | `Complete` via `_parent.DataModel` | **F6** |
> | — | none | **F11 — not a consumer** |
>
> F2 and F6 reach the contract indirectly through `IQfcHomeController.DataModel`, which is why a grep for
> `IQfcDatamodel` does not surface them. `QfcHomeController.cs:299` is the sole production consumer of the
> issue-#424 four-argument overload.
>
> **`SortOptionsEnum` is a second cross-child contract, with F2 rather than F7.** The enum is declared in
> `QuickFiler/Interfaces/IQfcDatamodel.cs:12-22` (F5) and is interpreted in exactly one place:
> `EmailSorter.GetSortKey` (`QuickFiler/Controllers/EmailSorter.cs:45-48`, **F2**), whose only predicate is
> `HasFlag(TriageImportantFirst) && HasFlag(DateRecentFirst)`. `SortOptionsEnum.Default = 42` decomposes to
> `TriageImportantFirst | DateRecentFirst | ConversationUniqueOnly` (`2 + 8 + 32`), so the default satisfies
> that predicate. F5's own `QfcDatamodel.FrameBuilding.cs:114` is the only production caller
> (`new EmailSorter(SortOptionsEnum.Default)`), reached unconditionally from both frame-build paths
> (`FrameBuilding.cs:24` and `:63`). **Coordination requirement:** if F2 restructures `GetSortKey`'s flag
> predicate while raising `EmailSorter.cs` coverage, F5's frame sort order changes. F5 pins the enum side of
> this contract with the characterization tests below; F2 owns the predicate side. Neither side may change
> `Default`.

---

## 7. Enumerated test cases

**Three test cases are warranted.** They are characterization tests of the enum's numeric contract, not
coverage tests, and the artifact states plainly what they do and do not achieve:

- **They earn zero line-coverage credit for `QuickFiler/Interfaces/IQfcDatamodel.cs`.** Per §2.1 the file
  emits no instrumented lines, so no test can change its measured percentage.
- They are justified by CLAUDE.md § UT2: "Coverage is a supporting metric, not the sole quality gate;
  untested critical behavior is not acceptable even if the overall percentage looks good." §2.4 establishes
  that `Default`'s composition is behavior that is load-bearing for an F5-owned production path and pinned by
  no existing test.
- All three assert only on `SortOptionsEnum` constants and `System.Enum.HasFlag`. **They deliberately do not
  construct `EmailSorter`**, so they execute no IL in the F2-owned `EmailSorter.cs` and cannot be read as F5
  claiming coverage credit on a sibling's file. (A variant that drives `EmailSorter.GetSortKey` with
  `SortOptionsEnum.Default` would assert the same contract end-to-end but would entangle F5's evidence with
  F2's coverage numbers and would duplicate `EmailSorterTests.cs:55-71`. Rejected for that reason.)

All three are MSTest `[TestClass]`/`[TestMethod]` with FluentAssertions, Arrange–Act–Assert, fully
deterministic, no Moq needed, no clock, no `Thread.Sleep`/`Task.Delay`/wall-clock wait, no temp file, no
external service, no live form, no COM object.

**Target test file (all three): `QuickFiler.Test/Interfaces/SortOptionsEnumTests.cs` — NEW.**
Namespace `QuickFiler.Interfaces.Tests`. Estimated ~85 lines, well under 500.

**Required csproj entry (verified constraint — `QuickFiler.Test/QuickFiler.Test.csproj` uses explicit item
lists; e.g. `<Compile Include="Controllers\EmailSorterTests.cs" />` at line 108 and
`<Compile Include="Controllers\QfcDatamodelTests.cs" />` at line 114):**

```xml
<Compile Include="Interfaces\SortOptionsEnumTests.cs" />
```

**Placement note for the planner.** `QuickFiler.Test/` currently has no `Interfaces/` folder; a grep of the
test csproj for `<Compile Include="Interfaces` returns no hit, and the test for the sibling production file
`QuickFiler/Interfaces/MailItemActionsAdapter.cs` lives at
`QuickFiler.Test/Controllers/MailItemActionsAdapterTests.cs` — i.e. the existing convention flattens
`Interfaces/` into `Controllers/`. `.claude/rules/general-unit-test.md` § Test File Location requires the
test tree to mirror the production tree, which points to `QuickFiler.Test/Interfaces/`. **Recommendation:
follow the rule and create `QuickFiler.Test/Interfaces/SortOptionsEnumTests.cs`**, which requires only the
one csproj line above (no folder registration is needed in a non-SDK project). If the planner prefers
consistency with the `MailItemActionsAdapterTests.cs` precedent, use
`QuickFiler.Test/Controllers/SortOptionsEnumTests.cs` with
`<Compile Include="Controllers\SortOptionsEnumTests.cs" />` instead; either satisfies every other constraint.

---

### Test 1 — `Default_DecomposesToTriageImportantFirstDateRecentFirstAndConversationUniqueOnly`

- **Member under test:** `SortOptionsEnum.Default` (`IQfcDatamodel.cs:15`) against the six flag members
  (`:16-21`).
- **Category:** characterization / boundary (pins a magic constant).
- **Arrange:** none — all operands are compile-time constants.
- **Act:** read `SortOptionsEnum.Default`.
- **Assert (two assertions, one intent):**
  - `((int)SortOptionsEnum.Default).Should().Be(42, "the composite default is written as a bare magic number in IQfcDatamodel.cs:15");`
  - `SortOptionsEnum.Default.Should().Be(SortOptionsEnum.TriageImportantFirst | SortOptionsEnum.DateRecentFirst | SortOptionsEnum.ConversationUniqueOnly);`
- **Why it is warranted:** `42` appears in source with no comment explaining its composition. The second
  assertion is the executable documentation of what the number means, and it fails loudly if any of the three
  contributing flag *values* is changed even when `Default` itself is left at 42.
- **Atomic task note:** this is the task that creates the file and adds the csproj entry.

### Test 2 — `Default_SatisfiesBothFlagsRequiredForTriageDateSortKeyGeneration`

- **Member under test:** `SortOptionsEnum.Default`, characterized against the consumer predicate at
  `EmailSorter.cs:45-48`.
- **Category:** characterization of a cross-child contract (highest value of the three).
- **Arrange:** none.
- **Act:** evaluate `SortOptionsEnum.Default.HasFlag(SortOptionsEnum.TriageImportantFirst)` and
  `SortOptionsEnum.Default.HasFlag(SortOptionsEnum.DateRecentFirst)`.
- **Assert:** both `.Should().BeTrue(...)`, each with a message naming the dependency, for example:
  `"EmailSorter.GetSortKey (EmailSorter.cs:45-48) returns -1 unless Default carries both flags, which would silently destroy the frame ordering built at QfcDatamodel.FrameBuilding.cs:114"`.
- **Why it is warranted:** this is the only assertion in the repository that connects `Default` to the
  predicate that interprets it. `EmailSorterTests.cs:62` and `:78` construct
  `TriageImportantFirst | DateRecentFirst` **explicitly instead of using `Default`**, so the connection is
  currently unpinned in both directions. Regression shape is documented in §2.4: every sort key collapses to
  `-1`, `SortTriageDate` degenerates to an unconditional reversal of Deedle's tie order, and no test anywhere
  fails.
- **Overlap disclosure:** arithmetically implied by Test 1. Kept separate because it documents a different
  intent (a consumer obligation, not a numeric identity) and because its failure message points a future
  maintainer at the F2 call site. A planner optimising for minimality may merge Tests 1 and 2 into one method
  with three assertions; that is a defensible reduction to two tasks and loses only the distinct failure
  message.

### Test 3 — `FlagMembers_AreDistinctSingleBitValues`

- **Member under test:** the six individual flag members `TriageIgnore`, `TriageImportantFirst`,
  `TriageImportantLast`, `DateRecentFirst`, `DateOldestFirst`, `ConversationUniqueOnly`
  (`IQfcDatamodel.cs:16-21`).
- **Category:** invariant / guard against a future edit.
- **Arrange:** an array of the six values (a `[DataRow]`-per-member shape is equally acceptable and gives
  sharper failure attribution).
- **Act:** for each value `v`, compute `(int)v`.
- **Assert:** every value is `> 0`; every value satisfies `(v & (v - 1)) == 0` (single bit set); the six
  values are pairwise distinct (`.Should().OnlyHaveUniqueItems()`).
- **Why it is warranted:** `Default = 42` is declared **first**, before the powers of two. A future member
  appended after `ConversationUniqueOnly = 32` without an explicit initializer receives the implicit value
  `33`, which sets bits 0 and 5 and therefore reports `HasFlag(TriageIgnore) == true` and
  `HasFlag(ConversationUniqueOnly) == true`. On a `[Flags]` enum that silently corrupts every consumer
  predicate. This test converts that trap into a build-time-visible failure.
- **Lower priority than Tests 1 and 2**, because it guards a hypothetical future edit rather than a current
  unpinned behavior. Include it; it is 15 lines and has no arrangement cost.

### Scenario-completeness note

Against `.claude/rules/general-unit-test.md` § Scenario Completeness: a `[Flags]` enum declaration has no
positive/negative input flows, no error handling, no concurrency, and no state transitions — it has only
constant values and their bit relationships. The applicable categories are **boundary/invariant
characterization**, which Tests 1–3 cover completely (composite value, consumer-required bits, member-value
well-formedness). **No further tests are warranted for this file, and none should be invented to reach a
count.**

---

## 8. Observations — promote to issue, do not fix

Following the precedent both sibling F5 agents set: AC7 (`issue.md:69`) forbids behavior change, so each item
below is promoted through the MCP promotion lifecycle as its own GitHub issue and recorded in `spec.md`, not
fixed here.

**O1 — Two `IQfcDatamodel` members have zero production consumers, and one of them cannot be called
successfully.**
- `void UndoMove()` (`IQfcDatamodel.cs:47`) is implemented at `QfcDatamodel.QueueProcessing.cs:24-27` as an
  unconditional `throw new NotImplementedException()` with a `//TODO` at line 23. Verified: no production
  call site (§3.3).
- `SloStack<IMovedMailInfo> MovedItems { get; }` (`:48`) is implemented at `QfcDatamodel.cs:141-144` as
  `=> _globals.AF.MovedMails`. Verified: no production consumer. The only code that needs the moved-mail
  stack, `QfcFormController.cs:49`, reads `_globals.AF.MovedMails` directly, making the interface member a
  redundant second path to the same object.
- **Impact:** two members of a public contract that no caller uses. `UndoMove` in particular advertises a
  capability the type does not have; a future consumer that trusts the contract gets a
  `NotImplementedException` at runtime.
- **Why not fixed here:** removing a member from a `public` interface is a breaking change to a contract
  re-exposed through `public interface IQfcHomeController` (U1), it is not a coverage improvement, and R1
  prohibits it. Note that the QueueProcessing artifact's test 1 (`UndoMove_IsNotImplemented_Throws`) already
  plans to *pin* the throw, which is the correct AC7-compliant treatment.

**O2 — Four of the six `SortOptionsEnum` flags are dead, and one of them is misleadingly inside `Default`.**
- A repo-wide `.cs` grep for `TriageIgnore|TriageImportantLast|DateOldestFirst|ConversationUniqueOnly`
  returns **only the four declaration lines** (`IQfcDatamodel.cs:16,18,20,21`). No consumer reads any of
  them.
- `ConversationUniqueOnly` (32) is a component of `Default = 42`, which reads as though the
  unique-by-conversation filter were configurable. It is not: `MostRecentByConversation`
  (`QfcDatamodel.FrameBuilding.cs:134`) is invoked **unconditionally** at `FrameBuilding.cs:21` and `:59`
  with no flag test anywhere.
- **Impact:** a maintainer clearing `ConversationUniqueOnly` from `Default` to disable conversation
  de-duplication would observe no change in de-duplication, but would change `Default` from 42 to 10 —
  harmlessly, since only bits 2 and 8 are read. The reverse edit is the dangerous one (Test 2 guards it).
- **Why not fixed here:** R3 forbids changing the enum; and deciding whether to delete the dead flags or wire
  `ConversationUniqueOnly` to the existing filter is a design decision spanning F5 and F2.

**O3 — `EmailSorter.GetSortKey` indexes `_triageImportantLast` inside its `TriageImportantFirst` branch
(F2-owned; likely a misleading identifier, not a defect).**
- `EmailSorter.cs:46` tests `HasFlag(SortOptionsEnum.TriageImportantFirst)` but line 53 indexes
  `_triageImportantLast` (declared `:29-35`, mapping `A→4, B→3, C→2, Z→1`) rather than
  `_triageImportantFirst` (`:21-27`, mapping `A→1, B→2, C→3, Z→4`), which is never read anywhere.
- **Assessment: behavior appears correct; the name is inverted.** `SortTriageDate`
  (`FrameBuilding.cs:112-132`) sorts *ascending* by the key (line 124), then re-indexes with
  `Range(0, RowCount).Reverse()` (line 126) and re-sorts by that index (line 128) — a net **descending**
  order. Descending by `4 * 1e14 + dateKey` puts triage `A` first and, within a triage band, the most recent
  date first, which is exactly "triage important first, date recent first". So the `_triageImportantLast`
  table is the correct one to consume under a descending sort; only its name suggests otherwise.
  `EmailSorterTests.cs:51-54` pins the current mapping (`A → 420260706180705L`, the largest key), confirming
  the intended direction.
- **Impact:** readability only. The risk is that a future maintainer "fixes" the apparent mismatch by
  switching to `_triageImportantFirst` and inverts the production sort order in a change no test catches
  (the existing DataRows would fail, so the test suite does guard it — noted).
- **Why not addressed here:** `EmailSorter.cs` is F2-owned (R6), the dictionary rename is a pure-cosmetic
  change with no coverage benefit, and AC7 forbids behavior change. Promote as a low-priority
  readability issue against F2's file.

---

## 9. Risks and open questions

| ID | Item | Impact | Handling |
| --- | --- | --- | --- |
| **R1** | **F1 ledger may classify this file as `testable`.** F1's ledger does not exist on disk (expected). If it lists `IQfcDatamodel.cs` as `testable` with an 80% obligation, the feature acquires an unsatisfiable acceptance criterion. | Would block AC1 (`issue.md:59`) permanently — no test can raise a percentage that the tool never emits. | Treat F1's ledger as authoritative on arrival and re-read §2.5 at plan time. If it says `testable`, escalate to the epic with the §2.1 measured evidence rather than attempting to comply. The same argument applies to the other ~23 declaration-only files (epic.md § Scope). |
| **R2** | **Per-file harness representation of declaration-only files.** The harness derives per-file numbers from Cobertura `filename` grouping (`scripts/vscode/Invoke-MSTestWithCoverage.ps1` post-processes the Cobertura XML at lines 333-340). A harness that seeds its file list from `QuickFiler.csproj` and defaults missing files to 0% would report ~24 permanent failures. | Would make the epic's per-file gate unclosable for F16. | Raise with F1 as Q1 below. **INFERRED** — the harness does not exist yet, so its behavior on a missing `filename` key could not be verified. |
| **R3** | **Fan-in coupling on `SortOptionsEnum` with F2.** F2 owns `EmailSorter.cs` (the only flag interpreter) and `EmailSorterTests.cs`. F2 is raising coverage on that file in the same wave. | If F2 restructures `GetSortKey`'s predicate, F5's `SortTriageDate` ordering changes, and F5's Tests 1-3 would still pass because they assert only the enum side. | Record the §6 coordination note in `spec.md`. Tests 1-3 pin the F5 side of the contract; F2's `EmailSorterTests.cs:51-54` DataRows pin the F2 side. Neither side may change `Default`. |
| **R4** | **`issue.md` names the wrong sibling (F11).** A planner that carries this forward would coordinate with the wrong child and would miss F2 and F6. | Mis-targeted coordination; a real consumer (`QfcQueue.cs:476`) left unconsidered. | The §6 correction block must land in `spec.md` before planning. |
| **Q1** | **For F1:** should `not-measurable (declaration-only)` be a distinct third ledger category, and should the harness omit such files rather than report 0%? | Affects ~24 of 121 files and the closability of F16's gate. | Recommended: yes to both (§2.5 notes 1-2). F1 decides; F5 records the recommendation and the measured evidence. |
| **Q2** | Should `QuickFiler.Test/Interfaces/` be created, or should the tests follow the existing flattened `Controllers/` convention (`MailItemActionsAdapterTests.cs`)? | Cosmetic; one csproj line either way. | Recommend `QuickFiler.Test/Interfaces/` per `.claude/rules/general-unit-test.md` § Test File Location. Either is acceptable; §7 gives both csproj lines. |
| **Q3** | Should Tests 1 and 2 be merged into a single method? | One fewer atomic task; loses a distinct failure message. | Keep separate (§7 Test 2 "Overlap disclosure"). A planner optimising for minimality may merge; do not merge Test 3 in. |

---

## 10. Files this phase would touch

| Path | Action |
| --- | --- |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | **No change.** No seam, no split, no attribute, no signature edit. 59 lines, unchanged. |
| `QuickFiler.Test/Interfaces/SortOptionsEnumTests.cs` | **New.** Tests 1-3, ~85 lines. |
| `QuickFiler.Test/QuickFiler.Test.csproj` | Add `<Compile Include="Interfaces\SortOptionsEnumTests.cs" />` (legacy non-SDK project; explicit item lists are verified at csproj lines 108 and 114 — a new `.cs` file silently will not build without it). |
| `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/spec.md` | Add the §6 cross-child observations block (the `IQfcDatamodel` consumer-map correction and the `SortOptionsEnum`/F2 coordination note). |

Explicitly **not** touched: `coverage.config`; any shared build property file; `QuickFiler/QuickFiler.csproj`
(no production file is added or removed by this phase); `EmailSorter.cs`, `EmailSorterTests.cs`, `QfcQueue.cs`
(F2); `QfcFormController.EventHandlers.cs` (F6); `QfcHomeController*.cs`,
`Controllers/IQfcHomeController.cs`, `Interfaces/IFilerHomeController.cs`, and the six
`QfcHomeController*Tests.cs` files (F7); `QfcCollectionController.cs`,
`Interfaces/IQfcCollectionController.cs` (F11); `QuickFiler/Interfaces/IQfcHomeController.cs` and
`QuickFiler/Notes/notes_interfaces.cs` (both uncompiled); `QfcDatamodel.cs`,
`QfcDatamodel.QueueProcessing.cs`, `QfcDatamodel.FrameBuilding.cs`, `EfcDataModel.cs` (other F5 phases).
