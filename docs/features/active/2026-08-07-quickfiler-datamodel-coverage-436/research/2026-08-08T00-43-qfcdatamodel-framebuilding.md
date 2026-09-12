# Research: `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs`

- Feature: `quickfiler-datamodel-coverage` (issue #436), child F5 of epic `quickfiler-per-file-coverage` (#136)
- Target file: `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` — 154 lines, no `[ExcludeFromCodeCoverage]` of its own
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a923053598cf4ccea`
- Created: 2026-08-08T00-43
- Scope: this one production file. Sibling partials `QfcDatamodel.cs` and `QfcDatamodel.QueueProcessing.cs`,
  and `EfcDataModel.cs`, are researched separately and appear here only where a cross-file consequence
  is unavoidable.
- Companion artifacts (read first, built upon here, not re-derived):
  - `.../research/2026-08-08T00-43-qfcdatamodel.md`
  - `.../research/2026-08-08T00-43-qfcdatamodel-queueprocessing.md`
  - `.../research/2026-08-08T00-43-efcdatamodel.md`

---

## 0. Executive summary

**1. DECISIVE ANSWER (AC2): removing the type-scoped `[ExcludeFromCodeCoverage]` at
`QfcDatamodel.cs:25` is SAFE. This file claims no irreducible remainder and needs no member-level
exemption.** After the two delegate seams in §5, every one of this file's ~35 post-refactor
instrumented lines is reachable by a deterministic unit test. All four F5 production files now agree,
and the last unknown blocking the clean attribute removal is cleared.

**2. CORRECTION — the premise in `issue.md:80-82` is factually wrong, and it propagated into the
delegation prompt.** `issue.md` states *"WinForms layout. `QfcDatamodel.FrameBuilding.cs` interacts
with WinForms layout."* **It does not.** "Frame" here means the Deedle data frame
`Deedle.Frame<int, string>`, not a WinForms layout frame. Verified by reading all 154 lines: the
using block (lines 1–7) is `System`, `System.Linq`, `System.Threading.Tasks`, `Deedle`,
`Microsoft.Office.Interop.Outlook`, `QuickFiler.Interfaces`, `UtilitiesCS` — **there is no
`System.Windows.Forms` import and no WinForms type, fully-qualified or otherwise, anywhere in the
file.** Consequences, all load-bearing for planning:
   - **The STA last-resort clause (epic.md §3) does not apply to this file. Zero members need STA.
     `QuickFiler.Test` still has no `*.StaTests.cs` and this phase does not create the first one.**
   - `QuickFiler/Helper Classes/TlpCellSnapShot.cs` (a `System.Windows.Forms`
     `TableLayoutPanel` cell-state helper, `TlpCellSnapShot.cs:6,12`, owned by **F4** per
     epic.md:282) is unrelated to this file and must not be pulled into F5.
   - `spec.md` should carry a correction note so reviewers do not look for a WinForms seam that
     does not exist.

**3. The real host boundary is Outlook COM reached through two `DfDeedle` static calls, and it is a
hard blocker — verified, not inferred.** Driving `InitDf` or `GetEmailsInViewDfAsync` without a seam
reaches `DfDeedle.AddQfcColumns` (`DfDeedle.cs:296-316`), which calls
`DfDeedle.MessageBoxInvoker` and **pops two real modal dialogs** before throwing
`InvalidOperationException`. `MessageBoxInvoker` is `internal static` (`DfDeedle.cs:54-60`) and
`UtilitiesCS/Properties/AssemblyInfo.cs:19-20` grants `InternalsVisibleTo` only to `UtilitiesCS.Test`
and `ToDoModel.Test` — **not `QuickFiler.Test`**, so this test assembly cannot neutralize the dialog.
Two injectable-delegate seams (§5, **S5** and **S6**) are therefore mandatory and fully justified
against the seam hierarchy.

**4. Roughly 40% of this file is already testable today with zero production change.**
`SortTriageDate` (112–132) and `MostRecentByConversation` (134–152) are pure `Frame`-to-`Frame`
functions, are `public`, and are reachable on a `FormatterServices.GetUninitializedObject` instance
using the exact fixture pattern already in `QfcInitEmailQueueZeroBatchTests.cs:63-87`. No seam, no
COM, no clock.

**5. `ToggleOfflineMode` already has 6 of 6 lines covered** by the existing
`QfcDatamodelTests.cs:250`. Only one branch arm is open. Do not re-test it; §7 adds exactly one case.

**6. Host-neutral extraction is recommended and severable.** §4 proposes lifting the pure shaping
logic into a new `internal static class QfcEmailFrameShaper`, satisfying
`.claude/rules/general-unit-test.md` § Coverage Exclusion Policy and the epic Non-Goal at
epic.md:128-129. It buys design quality and portability, **not** coverage percentage — S5/S6 alone
satisfy AC2. The plan may sever it without re-opening the §0.1 conclusion.

**7. Five latent defects found; all recorded as promote-to-issue, none fixed** (AC7 forbids behavior
change), following the precedent both siblings set.

---

## 1. Method and evidence basis

Every claim is grounded in a file read in this session. Claims not verifiable without building or
running are marked **INFERRED** with the reason.

Files read in full or in the cited range:

| Path | Purpose |
| --- | --- |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` (all 154 lines) | subject |
| `UtilitiesCS/Extensions/DfDeedle.cs` (all 411 lines) | the COM boundary and the MessageBox blocker |
| `UtilitiesCS/Properties/AssemblyInfo.cs` | the `InternalsVisibleTo` wall |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs:16-35` | `GetTableInView` COM chain |
| `QuickFiler/Controllers/EmailSorter.cs` (all 86 lines) | sort-key semantics; **F2-owned, read only** |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` (all 59 lines) | `SortOptionsEnum.Default == 42`; cross-child contract |
| `QuickFiler/Properties/AssemblyInfo.cs` | `InternalsVisibleTo("QuickFiler.Test")` at line 5 |
| `UtilitiesCS/Threading/ProgressTracker.cs` (member signatures) | mockability of the progress sink |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs:220-317` | the existing `ToggleOfflineMode` test and helper conventions |
| `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs:1-80` | helper conventions, bounded-wait pattern |
| `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` (all 213 lines) | the `CreateTwoRowEmailFrame` fixture this phase reuses |
| `QuickFiler/QuickFiler.csproj:312-315,361`, `QuickFiler.Test/QuickFiler.Test.csproj:90-145` | explicit `<Compile Include>` item lists |
| `QuickFiler/Helper Classes/TlpCellSnapShot.cs:1-30` | confirming it is WinForms and unrelated |
| `Tags.Test/CheckBoxControllerWiring.StaTests.cs:1-60`, `Tags.Test/packages.config:110-111`, `QuickFiler.Test/packages.config:113-119` | STA precedent and package availability |
| `.claude/rules/csharp.md:45-63`, `.claude/rules/general-unit-test.md`, `CLAUDE.md` | policy |
| `docs/features/epics/quickfiler-per-file-coverage/epic.md`, `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/issue.md` | contract |
| the three sibling research artifacts | composition baseline |

Coverage-reality method: F1's per-file harness does not exist on disk yet (expected — F1 is prepared
concurrently). Current coverage is derived by (a) reading every test that can reach this file and
mapping it to the lines it drives, and (b) an independent search of the committed Cobertura report
from issue #424. F1's harness remains the authority; the plan must record its numeric output under
`<FEATURE>/evidence/qa-gates/`.

---

## 2. Member inventory

`public partial class QfcDatamodel`, namespace `QuickFiler.Controllers`. Usings 1–7; namespace/class
9–12; closing braces 153–154. Six members, no fields, no properties.

| # | Member | Lines | Vis. | Behavior (one line) |
| --- | --- | --- | --- | --- |
| B1 | `InitDf(Explorer activeExplorer)` | 13–27 | public | Synchronous frame build: fetch the in-view email table via `DfDeedle.GetEmailDataInView`, filter to `MessageClass == "IPM.Note"`, dedupe to the most recent email per conversation, sort by triage then date, return. Sole caller: `QfcDatamodel.cs:49` (the public ctor). |
| B2 | `ToggleOfflineMode(bool offline)` | 34–46 | private async | When `offline` is **false**, reads `_activeExplorer.CommandBars` and issues `ExecuteMso("ToggleOnline")`, then awaits a 5 ms delay through the injected `TimeProvider`. Returns its own argument unchanged. Callers: `GetEmailsInViewDfAsync` lines 77, 90, 99, 104. |
| B3 | `InitDfAsync(Explorer, ProgressTracker)` | 48–67 | public async | Asynchronous frame build. Awaits B4; when the result is non-null, applies the same three-step pipeline as B1 and assigns `_frame`, then reports 100% progress. Sole caller: `QfcDatamodel.cs:70` (`LoadAsync`). |
| B4 | `GetEmailsInViewDfAsync(Explorer, ProgressTracker)` | 69–110 | private async | Reads the current offline state from `_globals.Ol.NamespaceMAPI.Offline`, toggles online→offline via B2, fetches through `DfDeedle.GetEmailDataInViewAsync`, restores the prior state, returns the frame. `TaskCanceledException` → restore and return null. Any other exception → restore, log, `throw e`. |
| B5 | `SortTriageDate(Frame<int, string> df)` | 112–132 | public | **Pure.** Clones the input, computes a composite `NewKey` per row from `EmailSorter.GetSortKey(triage, sentOn)`, sorts ascending on it, reverses via `IndexRowsWith(Range(0, N).Reverse())` + `SortRowsByKey()`, drops `NewKey`, returns. Net effect: important triage first, most recent first within a triage. |
| B6 | `MostRecentByConversation(Frame<int, string> df)` | 134–152 | public | **Pure.** For each distinct `ConversationId`, keeps the single row with the maximum `SentOn` (first match on a tie), and rebuilds an ordinally-keyed frame via `Frame.FromRows`. |

### 2.1 Consumer survey — this file is self-contained

A repository-wide `*.cs` search for `InitDf`, `InitDfAsync`, `GetEmailsInViewDfAsync`,
`SortTriageDate`, and `MostRecentByConversation` returns **only** the declarations in this file plus
two call sites in the sibling partial (`QfcDatamodel.cs:49` and `QfcDatamodel.cs:70`).

- **No member of this file appears on `IQfcDatamodel`** (verified against all 59 lines of
  `IQfcDatamodel.cs`).
- **No consumer outside the `QfcDatamodel` partial family exists** — neither F7 (`QfcHomeController`)
  nor F11 (`QfcCollectionController`) touches any of these six members.
- `B5` and `B6` are `public` but are effectively internal to the partial family. Narrowing them is
  possible but **not recommended**: it is churn with no benefit, and §7's tests call them directly.

**Cross-child consequence: this file can be changed freely without any contract impact. No
`spec.md` cross-child breaking-change note is required from this file.**

### 2.2 The two seam-relevant signatures the sibling phase pins

The `QfcDatamodel.cs` artifact's seams **S3** and **S4** bind to members of this file as method
groups:

- S3 defaults its frame-builder to `InitDf` — so **B1's signature `Frame<int,string> InitDf(Explorer)`
  must not change.**
- S4 defaults its initializer to `(m, e, p) => m.InitDfAsync(e, p)` — so **B3's signature
  `Task InitDfAsync(Explorer, ProgressTracker)` must not change.**

Both are preserved by every proposal below. S3/S4 and this file's S5/S6 are **complementary, not
redundant**: S3/S4 let a test construct a `QfcDatamodel` *without* building a frame; S5/S6 let a test
drive B1/B3/B4's *bodies*.

---

## 3. Precise UI-coupling and host-coupling map

The delegation brief asked for a WinForms-coupling map. **There is no WinForms coupling to map.** The
equivalent and actually load-bearing analysis is the Outlook-COM coupling map, given below in the
requested three-category form. Category (a) — pure computation taking and returning values — is the
prize, and here it is unusually large.

| Member | Host type touched | Category | Detail |
| --- | --- | --- | --- |
| **B5 `SortTriageDate`** | **none** | **(a) pure computation** | Touches only `Deedle.Frame`/`Series`, `System.Linq`, `DateTime`, and `EmailSorter` (a plain `internal class`, `EmailSorter.cs:7`, no COM). Takes a `Frame`, returns a `Frame`. Reads no instance field. |
| **B6 `MostRecentByConversation`** | **none** | **(a) pure computation** | Same. Takes a `Frame`, returns a `Frame`. Reads no instance field. |
| **B1 `InitDf`** lines 18, 21, 24, 26 | none | (a) pure computation | The Deedle filter and the two calls into B5/B6. |
| **B1 `InitDf`** line 15 | `Outlook.Explorer` → `DfDeedle.GetEmailDataInView` | **(c) read of host state** | The only impure line. Reaches `activeExplorer.GetTableInView()` (`OlTableExtensions.TableAccess.cs:18-29`), `activeExplorer.CurrentFolder`, `.StoreID`, and `AddQfcColumns` — see §3.1. |
| **B3 `InitDfAsync`** lines 56, 59, 63 | none | (a) pure computation | Same three-step pipeline as B1, duplicated. Line 63 writes the private field `_frame`. |
| **B3 `InitDfAsync`** line 65 | `ProgressTracker` (plain class, all members `virtual`) | (b) mutation of a collaborator | `progress.Report(100)`. Mockable — no host dependency. |
| **B3 `InitDfAsync`** line 50 | via B4 | (c) read of host state | Delegated. |
| **B2 `ToggleOfflineMode`** line 38 | `Explorer.CommandBars` | (c) read of host state | Interop interface; `Mock<Explorer>` proxies it — **already proven** at `QfcDatamodelTests.cs:257-261`. |
| **B2 `ToggleOfflineMode`** line 41 | `Office.CommandBars.ExecuteMso` | **(b) mutation of host state** | Toggles Outlook's online/offline mode. Mockable and already verified in the same existing test. |
| **B2 `ToggleOfflineMode`** line 43 | `System.TimeProvider` | (a) via existing seam | Already behind the `TimeProvider` seam (`QfcDatamodel.cs:112`). |
| **B4 `GetEmailsInViewDfAsync`** line 77 | `IApplicationGlobals.Ol.NamespaceMAPI.Offline` | (c) read of host state | All three hops are interfaces (`IApplicationGlobals`, `IOlObjects`, `Outlook.NameSpace`). Mockable — precedent `TaskVisualization.Test/MoqOlToDo.cs:223-230` mocks `IOlObjects.NamespaceMAPI` over a `Mock<NameSpace>`. |
| **B4 `GetEmailsInViewDfAsync`** lines 82–89 | `DfDeedle.GetEmailDataInViewAsync` | **(c) read of host state — hard blocker** | See §3.1. |
| **B4** lines 90, 99, 104 | via B2 | (b) mutation of host state | Restore of the prior online/offline state. |
| **B4** lines 85–87 | `Token`, `TokenSource`, `ProgressTracker` | (a)/(b) | `progress.Increment(3).SpawnChild(78)`; all `virtual` (`ProgressTracker.cs:109,218`). |

### 3.1 The hard blocker, verified end to end

Both `DfDeedle` entry points funnel into `AddQfcColumns` (`DfDeedle.cs:296-316`), directly for the
synchronous path (`DfDeedle.cs:92`) and via `AddQfcColumnsAsync` for the async path
(`DfDeedle.cs:164, 318-343`). Trace with a loose `Mock<MAPIFolder>`:

1. `AddQfcColumns` → `EnsureTriageColumnExists(folder)` (`DfDeedle.cs:298, 345`).
2. `EnsureTriageColumnExists` → `HasUserDefinedProperty(folder, "Triage")` (`DfDeedle.cs:352, 392`).
   A loose mock returns `null` for `folder.UserDefinedProperties`, so line 394 returns `false`.
3. Back at `DfDeedle.cs:357`, **`MessageBoxInvoker(...)` shows a real modal Yes/No dialog.**
4. Not `Yes` → `return false` (line 365).
5. Back at `DfDeedle.cs:300`, **`MessageBoxInvoker(...)` shows a second real modal dialog**, then
   line 307 throws `InvalidOperationException`.

The seam that would neutralize this is `DfDeedle.MessageBoxInvoker`, declared **`internal static`** at
`DfDeedle.cs:54-60`. `UtilitiesCS/Properties/AssemblyInfo.cs:18-20` grants `InternalsVisibleTo` to
`DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, and `ToDoModel.Test` only. **`QuickFiler.Test` is not
granted access and cannot set it.** Even if it could, step 5 still throws, so the happy path remains
unreachable without a live Outlook folder carrying a `Triage` user-defined property.

This is the complete and sufficient justification for seams S5 and S6. It also explains why the
existing `QfcInitEmailQueueZeroBatchTests.cs:23-31` remarks block warns so emphatically about
accidentally reaching live COM/modal UX from this type.

---

## 4. The extraction proposal (host-neutral pure logic)

### 4.1 Recommendation

**Extract B5 and B6, plus the duplicated three-step pipeline, into a new host-neutral static class**
`QuickFiler/Controllers/QfcEmailFrameShaper.cs`, and leave B5/B6 as one-line delegating wrappers so
the public surface is byte-identical.

Grounds, in priority order:

1. `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy: *"extract all logic into
   host-neutral, testable modules and leave only the thinnest possible wiring in the host-bound
   entry point."*
2. epic.md:128-129 (Non-Goals): *"Where a seam choice is open, prefer host-neutral extraction that a
   future WebView2/Office.js port can reuse."* Triage/date ordering and most-recent-per-conversation
   dedup are domain rules any port needs; today they are welded to a COM-bound partial class.
3. `CLAUDE.md` § General Code Change Policy, Design Principle 4 (separate pure logic from I/O) and
   Principle 2 (reusability — the three-step pipeline is currently duplicated at lines 18/21/24 and
   56/59/63).

### 4.2 Proposed shape

New file `QuickFiler/Controllers/QfcEmailFrameShaper.cs` (~50 lines, `internal static`, zero COM,
zero WinForms, zero datamodel state):

```csharp
internal static class QfcEmailFrameShaper
{
    internal const string MailItemMessageClass = "IPM.Note";

    /// <summary>Filter to mail items, keep the most recent per conversation, then sort.</summary>
    internal static Frame<int, string> Shape(Frame<int, string> df);

    internal static Frame<int, string> FilterToMailItems(Frame<int, string> df);
    internal static Frame<int, string> MostRecentByConversation(Frame<int, string> df);
    internal static Frame<int, string> SortTriageDate(Frame<int, string> df);
}
```

Residual wiring in `QfcDatamodel.FrameBuilding.cs`:

```csharp
public Frame<int, string> SortTriageDate(Frame<int, string> df) =>
    QfcEmailFrameShaper.SortTriageDate(df);

public Frame<int, string> MostRecentByConversation(Frame<int, string> df) =>
    QfcEmailFrameShaper.MostRecentByConversation(df);
```

and B1 line 18–26 / B3 line 56–63 each collapse to one `QfcEmailFrameShaper.Shape(df)` call.
`internal` rather than `public` keeps the public surface minimal (`.claude/rules/csharp.md`
§ public surface); `QuickFiler/Properties/AssemblyInfo.cs:5` makes it visible to `QuickFiler.Test`.

**Behavior preservation.** The three steps run in the same order with the same arguments; B5/B6 are
non-virtual and have no external caller (§2.1), so no dispatch behavior changes. This is the one
place where care is required: `Shape` must apply **filter → dedup → sort**, matching lines 18/21/24
and 56/59/63 exactly. Test 15 in §7 pins that order.

### 4.3 Honest accounting

- **Coverage payoff: none.** B5/B6 are already reachable today (§6). Extraction moves ~21
  easily-covered lines out of the COM-bound partial into a new file that will sit near 100%. The
  residual `FrameBuilding.cs` shrinks from ~55 to ~35 instrumented lines, all still reachable via
  S5/S6, so both files clear 80% either way.
- **Costs:** one new production file (a 122nd compiled file the F16 capstone must account for, the
  same note both siblings raised), one `<Compile Include>` entry, and a smaller per-file denominator
  that makes `FrameBuilding.cs` percentage-fragile — at ~35 lines, four uncovered lines is 89%.
- **Severability: high.** The §0.1 attribute-removal conclusion rests on S5/S6 alone. If the plan's
  change budget is tight, sever §4 and retarget tests 1–15 at the instance methods
  (`model.SortTriageDate(...)` / `model.MostRecentByConversation(...)` on an uninitialized instance)
  with no other change. Every test scenario in §7 survives the severing unchanged.

### 4.4 Required csproj entries

| File | csproj | Entry | Placement |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcEmailFrameShaper.cs` | `QuickFiler/QuickFiler.csproj` | `<Compile Include="Controllers\QfcEmailFrameShaper.cs" />` | beside lines 312–315 |

Verified constraint: `QuickFiler.csproj:312-315` lists `EmailSorter.cs`, `QfcDatamodel.cs`,
`QfcDatamodel.FrameBuilding.cs`, `QfcDatamodel.QueueProcessing.cs` as explicit `<Compile Include>`
items. A new `.cs` file without an entry **silently will not build**.

---

## 5. Seam-hierarchy determination, per member

Walked in the mandatory order from `.claude/rules/csharp.md:49-53`: interface seam → injectable
delegate → adapter → (only then) STA last resort.

| Member / touch | 1. Interface seam | 2. Injectable delegate | 3. Adapter | Verdict |
| --- | --- | --- | --- | --- |
| **B5 `SortTriageDate`** | n/a | n/a | n/a | **No seam needed.** Pure; already testable. |
| **B6 `MostRecentByConversation`** | n/a | n/a | n/a | **No seam needed.** Pure; already testable. |
| **B2 line 38/41 `Explorer.CommandBars.ExecuteMso`** | **Level 1 already satisfied.** `Outlook.Explorer` and `Office.CommandBars` are interop interfaces Moq proxies directly — proven at `QfcDatamodelTests.cs:257-261`. | not reached | not reached | **Existing interface seam. Add nothing.** |
| **B2 line 43 delay** | — | Existing `TimeProvider` (`QfcDatamodel.cs:112`) | — | **Reuse as-is.** |
| **B4 line 77 `_globals.Ol.NamespaceMAPI.Offline`** | **Level 1 already satisfied.** `IApplicationGlobals` → `IOlObjects` → `Outlook.NameSpace` are all interfaces; precedent `TaskVisualization.Test/MoqOlToDo.cs:223-230`. | not reached | not reached | **Existing interface seam. Add nothing.** |
| **B1 line 15 `DfDeedle.GetEmailDataInView`** | Level 1 fails — see §5.1 | **Level 2 succeeds → S5** | not reached | **Injectable delegate (S5).** |
| **B4 lines 82–89 `DfDeedle.GetEmailDataInViewAsync`** | Level 1 fails — see §5.1 | **Level 2 succeeds → S6** | not reached | **Injectable delegate (S6).** |
| **B3 `InitDfAsync`** | — | Covered transitively by S6 (its only impure dependency is B4) | — | **No seam of its own.** |
| **STA last resort** | — | — | — | **Never reached. Zero members. See §5.3.** |

### 5.1 Why the interface seam is rejected at the two `DfDeedle` call sites

A rank-1 `IEmailDataFrameSource` interface with `GetEmailDataInView` / `GetEmailDataInViewAsync`
would require a production implementation — a new `DfDeedleEmailDataFrameSource` class whose entire
body is two one-line COM-delegating calls. That is rejected on the same grounds a sibling already
recorded and had accepted (`efcdatamodel.md` §5.3):

- It **relocates** the uncovered lines rather than removing them: the adapter is itself untestable
  and would immediately need its own exemption, which epic.md §1 makes a Blocking finding.
- It adds a further compiled file to the epic denominator for F16 to account for, on top of the one
  §4 already adds.
- `.claude/rules/csharp.md:49` requires "the smallest seam that enables reliable unit testing", and
  :52 explicitly authorizes a delegate "for a single call path when a full interface is excessive".
  Each `DfDeedle` entry point is exactly one call path with one call shape.

The delegate keeps the permanently-untestable residual to **two null-coalescing fallback arms** —
zero additional source lines, since each shares a line with the call it guards.

### 5.2 The two proposed seams

Both are additive `internal` property-injected delegates with a null-means-production default,
matching the house style already established at `QfcDatamodel.cs:112` (`TimeProvider`) and
`QfcDatamodel.cs:128` (`RemainingEmailLoader`), and matching sibling seams S1/S2.

**S5 — synchronous email-data-frame source (for B1).**

```csharp
/// <summary>
/// Testability seam for the in-view email data-frame fetch. Null means the production
/// <see cref="DfDeedle.GetEmailDataInView(Explorer)"/>. Exists because that call reaches
/// DfDeedle.AddQfcColumns, which shows modal dialogs this assembly cannot suppress.
/// </summary>
internal Func<Explorer, Frame<int, string>> EmailDataInViewProvider { get; set; }
```

Call site, `QfcDatamodel.FrameBuilding.cs:15`:

```csharp
var df = (EmailDataInViewProvider ?? DfDeedle.GetEmailDataInView)(activeExplorer);
```

**S6 — asynchronous email-data-frame source (for B4, and transitively B3).**

```csharp
/// <summary>
/// Testability seam for the asynchronous in-view email data-frame fetch. Null means the
/// production <see cref="DfDeedle.GetEmailDataInViewAsync"/>.
/// </summary>
internal Func<
    Explorer,
    CancellationToken,
    CancellationTokenSource,
    ProgressTracker,
    Task<Frame<int, string>>
> EmailDataInViewAsyncProvider { get; set; }
```

Call site, `QfcDatamodel.FrameBuilding.cs:82-89`:

```csharp
var fetch = EmailDataInViewAsyncProvider ?? DfDeedle.GetEmailDataInViewAsync;
df = await fetch(activeExplorer, Token, TokenSource, progress.Increment(3).SpawnChild(78))
    .ConfigureAwait(false);
```

Null-coalescing rather than a property initializer, because every existing datamodel test constructs
via `FormatterServices.GetUninitializedObject`, which bypasses initializers
(`QfcDatamodelTests.cs:231`). The `.ConfigureAwait(false)` at line 89 must be preserved verbatim —
invariant **I8** in the QueueProcessing artifact records that dropping a `ConfigureAwait(false)`
anywhere in this awaited chain deadlocks the Outlook UI thread.

`using System.Threading;` must be added to the file for `CancellationToken` /
`CancellationTokenSource` if S6's declaration lands here; if it lands in
`QfcDatamodel.Construction.cs` (recommended, §5.4), that file already has it.

### 5.3 STA determination — explicit and negative

**No member of this file requires an STA test. The count is zero.** The file constructs no WinForms
control, references no `System.Windows.Forms` type, and never touches the UI thread. The epic §3
last-resort clause is not reached because the hierarchy terminates at level 2 for every impure touch.

For completeness, since the brief asked what enabling the first `*.StaTests.cs` in `QuickFiler.Test`
would entail (it is **not** required, and this phase must not do it):

- **Attribute source:** `[STATestClass]`/`[STATestMethod]` ship in `MSTest.TestFramework`.
  `QuickFiler.Test/packages.config:119` already pins `MSTest.TestFramework` **4.3.3** — the same
  version `Tags.Test/packages.config:111` uses for `Tags.Test/CheckBoxControllerWiring.StaTests.cs:20`.
  **No package addition would be needed.**
- **Runsettings:** none. The `Tags.Test` and `TaskVisualization.Test` precedents are attribute-only;
  a repository-wide search for `apartmentState` / `ExecutionApartmentState` returns no matches.
- **csproj:** one `<Compile Include="...StaTests.cs" />` entry.

So the barrier is low — but the justification is absent, and epic §3 requires each STA-bound test to
document why no seam is feasible. Here a seam is feasible for every member, so an STA test would be
a policy violation, not a fallback.

### 5.4 Where the seam declarations should live

The `QfcDatamodel.cs` artifact §7 proposes a new `QuickFiler/Controllers/QfcDatamodel.Construction.cs`
holding all DI seam declarations, with a coordination note that sibling phases add their seams there
rather than to their own files, to keep one DI surface.

**Recommendation: follow that note.** Declare S5 and S6 in `QfcDatamodel.Construction.cs`; this
phase's production edit to `QfcDatamodel.FrameBuilding.cs` is then limited to the two call sites
(plus the §4 extraction, if taken).

**Consequence the plan must record: this creates a phase-ordering dependency — the
`QfcDatamodel.cs` phase must land `QfcDatamodel.Construction.cs` before this phase's tests can
compile.** Fallback if the plan sequences this phase first: declare S5/S6 in
`QfcDatamodel.FrameBuilding.cs` and have the `QfcDatamodel.cs` phase relocate them. The fallback
costs a move diff; the recommended order costs nothing. Prefer the recommended order.

### 5.5 Additivity confirmation

| Seam / change | Touches `IQfcDatamodel`? | Touches a public signature? | Cross-child impact |
| --- | --- | --- | --- |
| S5 | No | No | None |
| S6 | No | No | None |
| §4 extraction (B5/B6 become one-line wrappers) | No | No — signatures byte-identical | None |
| B1 / B3 signatures | No | Unchanged — required by sibling S3/S4 | `QfcDatamodel.cs:49,70` unaffected |

**No cross-child contract note for `spec.md` is required from this file** beyond the §0.2 factual
correction to `issue.md:80-82`.

---

## 6. Current coverage reality

### 6.1 The file is outside the denominator today — independently verified

An independent search of
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
performed in this session returns **exactly one** occurrence of the string `QfcDatamodel` in the
entire report. Per both siblings that single hit is an unrelated `set_DataModel` method signature at
line 21903, not a class entry.

`[ExcludeFromCodeCoverage]` at `QfcDatamodel.cs:25` is applied to the **type**, and a partial type's
attributes are unioned across its declarations, so this file is excluded even though it carries no
attribute of its own. **Measured coverage for `QfcDatamodel.FrameBuilding.cs` today is not 0% — it is
absent.** This is the third independent confirmation of the sibling finding.

### 6.2 Test-to-member map (read-derived)

Exactly **one** existing test reaches this file.

| Test | Members / lines driven | Confidence |
| --- | --- | --- |
| `QfcDatamodelTests.cs:250` `ToggleOfflineMode_WhenOnline_AwaitsInjectedFiveMillisecondDelay` | **B2 in full on the `offline == false` path**: lines 36, 38, 39, 41, 43, 45 — **all six of B2's lines**. Asserts the delay is sourced from the injected `TimeProvider` and that `ExecuteMso("ToggleOnline")` ran once. | high |

Confirmed **not** reaching this file: the five `TryQueueRemainingMailItemAsync_*` tests at
`QfcDatamodelTests.cs:21-219` (they construct `QfcRemainingQueueAdmission`, sibling F2's file — the
misleading-name trap both siblings flagged), `QfcDatamodelLivenessTests.cs`,
`QfcInitEmailQueueZeroBatchTests.cs`, and `QfcQueuePurePathsTests.cs`.

### 6.3 Genuinely untested members

| Member | Uncovered | Note |
| --- | --- | --- |
| **B1 `InitDf`** | lines 15, 18, 21, 24, 26 — **all** | No test calls it. |
| **B2 `ToggleOfflineMode`** | **no uncovered lines**; the **true arm of line 36** is an uncovered branch | Line coverage already 6/6. Needs exactly one new test (§7 case 19) for branch completeness. |
| **B3 `InitDfAsync`** | lines 50, 52, 56, 59, 63, 65 — **all** | No test calls it. |
| **B4 `GetEmailsInViewDfAsync`** | lines 74, 77, 82–89, 90, 94, 96, 99, 100, 102, 104, 105–107, 108 — **all** | No test calls it. |
| **B5 `SortTriageDate`** | lines 114–131 — **all** | **Uncovered but not blocked** — pure and public today. |
| **B6 `MostRecentByConversation`** | lines 136–151 — **all** | **Uncovered but not blocked** — pure and public today. |

Read-derived line arithmetic: approximately **55 instrumented source lines** in the file, of which
**6 are reached** — roughly **11%**.

**Confidence: medium.** This is a hand count of source lines, not a measurement. Two specific
uncertainties: (a) the multi-line statement at lines 82–89 may be attributed to one line or to
eight, which moves the denominator by ~7; (b) compiler-generated async state-machine and lambda
attribution is not modelled. F1's harness is the authority and the plan must record its numeric
output under `<FEATURE>/evidence/qa-gates/`.

### 6.4 Projected coverage after this phase — **INFERRED**

| File | Instrumented (est.) | Reached by §7 | Projected |
| --- | --- | --- | --- |
| `QfcDatamodel.FrameBuilding.cs` (after §4 extraction) | ~35 | all | **~100%** (residual: two null-coalescing fallback *branch* arms, zero lines) |
| `QfcDatamodel.FrameBuilding.cs` (if §4 severed) | ~55 | all | **~100%** |
| `QfcEmailFrameShaper.cs` (new, if §4 taken) | ~25 | all | **~100%** |

Both configurations clear the 80% floor with margin and clear the >= 90% new/changed-code target in
`CLAUDE.md` § UT2. **This projection must be replaced by F1's harness output before AC1 or AC2 is
checked off.**

---

## 7. Enumerated test cases

Each numbered item is intended to become a single atomic plan task. All use MSTest
`[TestClass]`/`[TestMethod]`, Moq, FluentAssertions, and Arrange–Act–Assert. None uses
`Thread.Sleep`, `Task.Delay`, a real wall-clock wait, a temporary file, an external service, a live
form, a modal dialog, the UI thread, or an STA apartment. All timing is driven by `FakeTimeProvider`
(`Microsoft.Extensions.TimeProvider.Testing`, `QuickFiler.Test/packages.config:85`).

**Conventions reused, not reinvented:** `CreateUninitializedDatamodel` and `SetPrivateField`
duplicated per test file per the convention documented at `QfcDatamodelLivenessTests.cs:18-24`; the
frame fixture adapted from `CreateTwoRowEmailFrame` (`QfcInitEmailQueueZeroBatchTests.cs:63-87`),
which builds a `Frame.FromRecords` of anonymous records with exactly the six `IEmailSortInfo` columns
(`EntryId`, `MessageClass`, `SentOn`, `ConversationId`, `Triage`, `StoreId`). Mocks are **loose**
except where a `Times.Never` assertion makes strict clearer.

**Determinism note specific to this file.** Sibling finding 5 warns that a forgotten `TimeProvider`
assignment fails *silently* (the confidence gate falls back to `TimeProvider.System`). **In this file
it fails loudly**: `TimeProvider.Delay` at line 43 is an extension method, so a null `TimeProvider`
throws immediately. Tests should still assign a `FakeTimeProvider` uniformly, but no silent
wall-clock trap exists here.

**Seam dependency legend:** `[S5]` / `[S6]` = requires that seam declared (see §5.4 for the
ordering dependency on `QfcDatamodel.Construction.cs`). `[§4]` = targets `QfcEmailFrameShaper` if the
extraction is taken, otherwise the equivalent public instance method on an uninitialized model — the
scenario is identical either way.

### T-file A1 — `QuickFiler.Test/Controllers/QfcEmailFrameShaperSortTests.cs` (new)

`SortTriageDate` / B5. No seam, no COM, no clock. Estimated ~300 lines.

| # | Test method | Member | Cat | Arrange / Act / Assert sketch |
| --- | --- | --- | --- | --- |
| 1 | `SortTriageDate_WithMixedTriageValues_OrdersImportantTriageFirst` | B5 `[§4]` | positive | **A:** three-row frame, `Triage` values `"Z"`, `"A"`, `"B"`, distinct `EntryId`s, all same `SentOn`. **Act:** sort. **Assert:** resulting `EntryId` column in row-key order is the `"A"` row, then `"B"`, then `"Z"`. Pins the composite key at `EmailSorter.cs:52-54` plus the reverse-index descending trick at lines 126–128. |
| 2 | `SortTriageDate_WithinTheSameTriage_OrdersMostRecentFirst` | B5 `[§4]` | positive | **A:** two rows, both `Triage = "A"`, `SentOn` 2026-01-01 and 2026-01-05. **Assert:** the 2026-01-05 row is first. Pins `GetDateKey` (`EmailSorter.cs:70-73`) as the low-order component. |
| 3 | `SortTriageDate_ReindexesResultRowKeysFromZeroAscending` | B5 `[§4]` | state-transition | **A:** three-row frame. **Assert:** `result.RowKeys` equals `[0, 1, 2]`. Pins the `IndexRowsWith(Range(0, N).Reverse())` + `SortRowsByKey()` pair at lines 126–128; without both, the caller's `_frame` row keys would not be a dense ascending range, which `InitEmailQueue` depends on. |
| 4 | `SortTriageDate_RemovesTheTemporarySortKeyColumnFromTheResult` | B5 `[§4]` | positive | **Assert:** `result.ColumnKeys` does not contain `"NewKey"` and equals the input's column set. Covers line 130. |
| 5 | `SortTriageDate_DoesNotMutateTheInputFrame` | B5 `[§4]` | positive/isolation | **A:** capture the input's `ColumnKeys` and `RowKeys` before the call. **Assert:** afterwards the input still lacks `"NewKey"` and its row keys are unchanged. Pins the defensive `df.Clone()` at line 116 — the invariant that makes B5 safe to call on a caller-owned frame. |
| 6 | `SortTriageDate_WithUnrecognizedTriageValue_ThrowsKeyNotFound` | B5 `[§4]` | error-handling | **A:** one row with `Triage = "Q"` (outside `{A,B,C,Z}` per `EmailSorter.cs:29-35`). **Assert:** `Invoking(...).Should().Throw<KeyNotFoundException>()`. Pins the log-and-rethrow at `EmailSorter.cs:57-65`. Cross-child note: incidentally covers an **F2**-owned file — see §9 R5. |
| 7 | `SortTriageDate_WithSingleRow_ReturnsThatRowAtKeyZero` | B5 `[§4]` | boundary | **A:** one-row frame. **Assert:** `RowCount == 1`, `RowKeys == [0]`, `EntryId` preserved. Degenerate case of the reverse-index arithmetic. |
| 8 | `SortTriageDate_WithMissingTriageColumn_Throws` | B5 `[§4]` | invalid-input | **A:** frame built without a `Triage` column. **Assert:** the call throws. **Characterization test** — Deedle's exact exception type for a missing column key was not verified from source (§9 R1); the implementer records the observed type in the test's doc comment and asserts it explicitly. Covers the failure path of line 119. |

### T-file A2 — `QuickFiler.Test/Controllers/QfcEmailFrameShaperConversationTests.cs` (new)

`MostRecentByConversation` / B6, the message-class filter, and the pipeline ordering. Estimated ~280 lines.

| # | Test method | Member | Cat | Arrange / Act / Assert sketch |
| --- | --- | --- | --- | --- |
| 9 | `MostRecentByConversation_WithSeveralEmailsPerConversation_KeepsOnlyTheLatestOfEach` | B6 `[§4]` | positive | **A:** four rows across two `ConversationId`s, two each, distinct `SentOn`. **Assert:** `RowCount == 2`; the surviving `EntryId`s are the later of each pair. Covers lines 136–151. |
| 10 | `MostRecentByConversation_WithOneEmailPerConversation_ReturnsEveryRow` | B6 `[§4]` | boundary | **A:** three rows, three distinct conversations. **Assert:** all three survive. Pins that the dedup is not lossy in the common case. |
| 11 | `MostRecentByConversation_WithTiedMaximumSentOn_KeepsTheFirstMatchingRow` | B6 `[§4]` | boundary | **A:** two rows sharing a `ConversationId` **and** an identical `SentOn`, with distinct `EntryId`s. **Assert:** exactly one row survives and its `EntryId` is the first in input order. Pins the `Rows.FirstValue()` tie-break at line 142 — currently an undocumented and unpinned determinism guarantee. |
| 12 | `MostRecentByConversation_ReturnsOrdinalRowKeysStartingAtZero` | B6 `[§4]` | state-transition | **A:** three-conversation frame. **Assert:** `result.RowKeys` equals `[0, 1, 2]`. Pins the `Frame.FromRows` ordinal re-keying at line 150 (§9 R2). |
| 13 | `MostRecentByConversation_WithMissingConversationIdColumn_Throws` | B6 `[§4]` | invalid-input | **A:** frame without a `ConversationId` column. **Assert:** the call throws. Characterization test, same handling as case 8. Covers the failure path of line 136. |
| 14 | `FilterToMailItems_DropsRowsWhoseMessageClassIsNotIpmNote` | pipeline `[§4]` | positive | **A:** frame with `MessageClass` values `"IPM.Note"`, `"IPM.Appointment"`, `"IPM.Note"`. **Assert:** two rows survive, both `"IPM.Note"`. Covers the filter at lines 18 / 56. If §4 is severed this scenario has no standalone entry point and folds into cases 16 and 22. |
| 15 | `Shape_FiltersNonMailItemsBeforeSelectingTheMostRecentPerConversation` | pipeline `[§4]` | ordering | **A:** one conversation containing an `"IPM.Appointment"` row with the **latest** `SentOn` and an `"IPM.Note"` row with an earlier `SentOn`. **Assert:** the surviving row is the `"IPM.Note"` one. **Highest-value case in this file** — it is the only test that distinguishes filter→dedup from dedup→filter, and it is the guardrail for the §4 extraction. If §4 is severed, target `InitDf` via S5 instead. |

### T-file B — `QuickFiler.Test/Controllers/QfcDatamodelInitDfTests.cs` (new)

`InitDf` / B1 and the open `ToggleOfflineMode` branch. Estimated ~200 lines.

| # | Test method | Member | Cat | Arrange / Act / Assert sketch |
| --- | --- | --- | --- | --- |
| 16 | `InitDf_ReturnsTheShapedFrameFromTheInjectedDataSource` | B1 **[S5]** | positive | **A:** `CreateUninitializedDatamodel()`; `Mock<Explorer>`; `EmailDataInViewProvider = _ => frame` where `frame` has one `"IPM.Appointment"` row and two `"IPM.Note"` rows sharing a `ConversationId`. **Act:** `model.InitDf(explorer.Object)`. **Assert:** result has one row, it is the later of the two notes, and its row key is 0. Covers lines 15, 18, 21, 24, 26 — the whole member. |
| 17 | `InitDf_PassesTheSuppliedExplorerToTheDataSource` | B1 **[S5]** | positive | **A:** provider captures its argument. **Assert:** the captured `Explorer` is the same instance passed to `InitDf`. Pins the argument flow at line 15 that sibling seam S3 depends on. |
| 18 | `InitDf_WhenTheDataSourceThrows_PropagatesWithoutSwallowing` | B1 **[S5]** | error-handling | **A:** `EmailDataInViewProvider` throws `InvalidOperationException("fetch failed")`. **Assert:** `Invoking(...).Should().Throw<InvalidOperationException>().WithMessage("fetch failed")`. Pins that B1 has no catch — a swallow here would hand the public constructor (`QfcDatamodel.cs:49`) a null `_frame`. |
| 19 | `ToggleOfflineMode_WhenAlreadyOffline_ReturnsTrueWithoutTouchingCommandBars` | B2 | boundary | **A:** uninitialized model; `model.TimeProvider = new FakeTimeProvider()`; `_activeExplorer` set to a `Mock<Explorer>(MockBehavior.Strict)` with **no** setups. **Act:** reflection-invoke `ToggleOfflineMode(true)` (private; established pattern at `QfcDatamodelTests.cs:263`). **Assert:** the task is already completed (no clock advance needed), the result is `true`, and `explorer.VerifyGet(x => x.CommandBars, Times.Never)`. Covers the currently-uncovered **true** arm of line 36. **The only new test B2 needs** — its six lines are already covered by `QfcDatamodelTests.cs:250`; do not duplicate that test. |

### T-file C — `QuickFiler.Test/Controllers/QfcDatamodelInitDfAsyncTests.cs` (new)

`InitDfAsync` / B3. Estimated ~230 lines.

Shared arrangement helper for this file and T-file D: `CreateModelWithFakeClock(out FakeTimeProvider
fake)` returning an uninitialized model with `TimeProvider` assigned, plus
`CreateProgressMock()` returning a `Mock<ProgressTracker>(new CancellationTokenSource())` whose
`Increment(It.IsAny<double>())` and `SpawnChild(It.IsAny<int>())` both return the mock itself (§9 R4).

| # | Test method | Member | Cat | Arrange / Act / Assert sketch |
| --- | --- | --- | --- | --- |
| 20 | `InitDfAsync_WithRowsReturned_AssignsTheShapedFrameAndReportsCompletion` | B3 **[S6]** | positive | **A:** model with fake clock; globals mocked so `Ol.NamespaceMAPI.Offline` is `true` (so B2 short-circuits and no clock advance is needed); `EmailDataInViewAsyncProvider` returns a three-row frame; progress mock. **Act:** `await model.InitDfAsync(explorer.Object, progress.Object)`. **Assert:** `_frame` read by reflection has the deduped, sorted shape; `progress.Verify(p => p.Report(100), Times.Once)`. Covers lines 50, 52, 56, 59, 63, 65. |
| 21 | `InitDfAsync_WhenTheDataSourceReturnsNull_LeavesTheFrameUnchangedAndDoesNotReportCompletion` | B3 **[S6]** | invalid-input | **A:** `_frame` pre-seeded with a sentinel frame; provider returns `Task.FromResult<Frame<int, string>>(null)`. **Assert:** `_frame` is still the sentinel instance; `progress.Verify(p => p.Report(100), Times.Never)`. Covers the false arm of line 52 — the cancellation-tolerant path that must not clobber an existing frame. |
| 22 | `InitDfAsync_DropsNonMailItemRowsBeforeAssigningTheFrame` | B3 **[S6]** | positive | **A:** provider returns a frame containing an `"IPM.Appointment"` row. **Assert:** `_frame` excludes it. Covers line 56 specifically — a distinct instrumented line from B1's line 18, so case 16 does not subsume it. |
| 23 | `InitDfAsync_WhenTheDataSourceThrows_PropagatesAndLeavesTheFrameUnchanged` | B3 **[S6]** | error-handling | **A:** `_frame` pre-seeded with a sentinel; provider throws `InvalidOperationException`. **Assert:** `await act.Should().ThrowAsync<InvalidOperationException>()`; `_frame` still the sentinel. Pins that B3 adds no catch of its own on top of B4's — a swallow here would let `LoadAsync` (`QfcDatamodel.cs:70`) return a model with a stale frame. |

### T-file D — `QuickFiler.Test/Controllers/QfcDatamodelEmailsInViewTests.cs` (new)

`GetEmailsInViewDfAsync` / B4, invoked by reflection (private). Estimated ~330 lines.

| # | Test method | Member | Cat | Arrange / Act / Assert sketch |
| --- | --- | --- | --- | --- |
| 24 | `GetEmailsInViewDfAsync_WhenOutlookIsOnline_TogglesBeforeTheFetchAndRestoresAfter` | B4 | ordering | **A:** `Ol.NamespaceMAPI.Offline` returns `false`; `Mock<Explorer>` with `CommandBars.ExecuteMso` recorded into an ordered log; fake clock; provider appends `"fetch"` to the same log and returns a frame. **Act:** reflection-invoke; the returned task parks on line 43's 5 ms delay, so `fake.Advance(5ms)`, then it parks again after line 90's toggle, so `fake.Advance(5ms)`, then `await`. **Assert:** the log is `["ToggleOnline", "fetch", "ToggleOnline"]`. Covers lines 74, 77, 90 and the ordering contract that keeps Outlook offline for the duration of the fetch. Fully deterministic — every wait is a `FakeTimeProvider` advance. |
| 25 | `GetEmailsInViewDfAsync_WhenOutlookIsAlreadyOffline_NeverTouchesCommandBars` | B4 | boundary | **A:** `Offline` returns `true`. **Assert:** result is the provider's frame; `explorer.VerifyGet(x => x.CommandBars, Times.Never)`; no clock advance was required. Covers the already-offline flow through lines 77 and 90 and the true arm of line 36 in its real calling context. |
| 26 | `GetEmailsInViewDfAsync_ReturnsTheFrameFromTheDataSourceUnmodified` | B4 | positive | **A:** `Offline == true` to keep the arrangement minimal. **Assert:** the result is the **same reference** the provider returned — B4 performs no shaping; shaping is B3's job. Covers line 94. |
| 27 | `GetEmailsInViewDfAsync_PassesTokenTokenSourceAndAChildProgressTrackerToTheDataSource` | B4 **[S6]** | positive | **A:** assign `model.Token` and `model.TokenSource` (public properties, `QfcDatamodel.cs:146-158`); progress mock. **Assert:** the provider received the model's token and token source; `progress.Verify(p => p.Increment(3), Times.Once)` and `progress.Verify(p => p.SpawnChild(78), Times.Once)`. Covers lines 85–87 and pins the 3/78 progress allocation the startup progress band depends on. |
| 28 | `GetEmailsInViewDfAsync_WhenTheFetchIsCancelled_RestoresOnlineStateAndReturnsNull` | B4 **[S6]** | error-handling | **A:** `Offline == false`; provider returns a task faulted with `TaskCanceledException`. **Assert:** the result is `null` (not an exception); `ExecuteMso("ToggleOnline")` was invoked **twice**, proving the restore at line 99 ran. Covers lines 96, 99, 100. |
| 29 | `GetEmailsInViewDfAsync_WhenTheFetchFailsUnexpectedly_RestoresOnlineStateThenRethrows` | B4 **[S6]** | error-handling | **A:** provider throws `InvalidOperationException("boom")`. **Assert:** `await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom")`; `ExecuteMso("ToggleOnline")` invoked twice. Covers lines 102, 104, 105–107, 108 — the last uncovered lines in the file. |
| 30 | `GetEmailsInViewDfAsync_WhenTheOfflineProbeThrows_PropagatesWithoutFetchingOrRestoring` | B4 **[S6]** | error-handling | **A:** `Mock<IOlObjects>.NamespaceMAPI` getter set up to throw `System.Runtime.InteropServices.COMException`. **Assert:** the exception escapes; the provider was **never** invoked; `ExecuteMso` never called. Pins the currently-unpinned asymmetry that line 77 sits **outside** the `try` at line 80, so an offline-probe failure is neither logged nor state-restored while a fetch failure is both. Characterization of current behavior — see §8 D4. |

### 7.1 Scenario-completeness check

Against `.claude/rules/general-unit-test.md` § Scenario Completeness and `issue.md` AC5:

- **Positive:** 1, 2, 9, 10, 14, 16, 17, 20, 22, 24, 25, 26, 27
- **Invalid input:** 8, 13, 21
- **Boundary:** 7, 10, 11, 19, 25
- **Error handling:** 6, 18, 23, 28, 29, 30
- **State transition:** 3, 12, 20, 21
- **Concurrency / ordering:** 15, 24 (both driven by `FakeTimeProvider` and an ordered call log; no new threading primitive is introduced)

### 7.2 Required csproj entries — all five test files

`QuickFiler.Test/QuickFiler.Test.csproj` uses explicit `<Compile Include>` items
(verified at lines 90–145; `QfcDatamodelTests.cs` at :114, `QfcDatamodelLivenessTests.cs` at :115).
Each new file silently will not build without its own entry:

```xml
<Compile Include="Controllers\QfcEmailFrameShaperSortTests.cs" />
<Compile Include="Controllers\QfcEmailFrameShaperConversationTests.cs" />
<Compile Include="Controllers\QfcDatamodelInitDfTests.cs" />
<Compile Include="Controllers\QfcDatamodelInitDfAsyncTests.cs" />
<Compile Include="Controllers\QfcDatamodelEmailsInViewTests.cs" />
```

Add beside line 116. Every proposed file is estimated at or under 330 lines, well inside the
500-line ceiling.

---

## 8. Latent defects found — promote to issue, do not fix

Following the precedent both siblings set (AC7 forbids behavior change; prose in a feature folder
disappears at merge, so each must go through the MCP promotion lifecycle into a real issue).

| ID | Defect | Evidence | Impact | Recommended handling |
| --- | --- | --- | --- | --- |
| **D1** | **Unreachable nested condition.** Line 39 re-tests `if (!offline)` inside the block already guarded by line 36. Its false arm can never execute. | `QfcDatamodel.FrameBuilding.cs:36,39` | One permanently-uncoverable **branch** arm. **No line-coverage impact** — line 39 itself is covered by the existing test. | Promote as a code-clarity issue. Removing it is behavior-preserving but is still an unrelated edit; keep it out of F5's diff so review does not read it as scope creep. |
| **D2** | **`throw e;` resets the stack trace.** Line 108 rethrows the caught exception by value instead of using a bare `throw;`, discarding the original stack. | `QfcDatamodel.FrameBuilding.cs:108`; the same pattern exists in `QfcDatamodel.cs` (sibling M19) | Diagnostics only, but it degrades every fetch-failure report from the startup path. | Promote **one** issue covering both sites. Changing it alters observable exception state, so it is out of scope here. Case 29 asserts only the type and message, so it passes before and after a future fix. |
| **D3** | **XML doc contradicts behavior.** Lines 29–33 claim the method will "save the state and toggle it to offline mode". It neither saves state (the caller does, line 77) nor sets offline mode directly — it issues the `ToggleOnline` **toggle** command and returns its argument unchanged. | `QfcDatamodel.FrameBuilding.cs:29-33` vs. 34–46 | Misleads future maintainers about a method that gates Outlook's connection state. | Promote as a documentation-defect issue. |
| **D4** | **Asymmetric error handling around the offline probe.** `_globals.Ol.NamespaceMAPI.Offline` (line 77) is read **before** the `try` at line 80, so a COM failure there escapes unlogged and without restoring state, while an identical failure inside the fetch is logged and restored. | `QfcDatamodel.FrameBuilding.cs:77,80,102-108` | A transient COM failure probing the offline state produces a silent, unlogged startup failure. | Promote. Case 30 pins current behavior so a future fix is a deliberate, visible change. |
| **D5** | **Restore failure masks the original exception.** If `ToggleOfflineMode(offline)` at line 99 or 104 itself throws, the original `TaskCanceledException` / fetch exception is lost and replaced. | `QfcDatamodel.FrameBuilding.cs:96-108` | Narrow (requires a second COM failure during teardown), but it destroys the diagnostic for the first failure. | Promote at low priority, bundled with D2 if the maintainer prefers one exception-handling issue. |

None of these is caused by this feature, and no test in §7 pins D1, D2, D3, or D5.

---

## 9. Risks and open questions

| ID | Item | Impact | Handling |
| --- | --- | --- | --- |
| **R1** | **Deedle's exact exception type for a missing column key was not verified from source.** Cases 8 and 13 assert on it. `Deedle` 3.0.0 is a binary package (`QuickFiler.Test/packages.config:7`); its source was not read. **INFERRED.** | Two of thirty tests; the assertion shape, not the scenario, is at risk. | Write both as characterization tests: the implementer runs once, records the observed exception type in the test's doc comment, and asserts it explicitly. Do not use a bare `Should().Throw<Exception>()` — that would pass on an NRE and pin nothing. |
| **R2** | **`Frame.FromRows(rows)` ordinal re-keying at line 150 is inferred**, from the fact that the file compiles and the method's declared return type is `Frame<int, string>`. Deedle's overload set was not read. **INFERRED.** | Case 12 exists precisely to pin it. | If case 12 shows a different keying, the test records actual behavior and §4's extraction is unaffected (it moves the call verbatim). |
| **R3** | **Seam declaration location creates a phase-ordering dependency.** §5.4 recommends declaring S5/S6 in the sibling-owned new file `QfcDatamodel.Construction.cs`. | This phase's tests cannot compile until the `QfcDatamodel.cs` phase lands that file. | Sequence the `QfcDatamodel.cs` phase first. Fallback documented in §5.4 (declare locally, sibling relocates). Record the edge in `spec.md` and the plan. |
| **R4** | **`Mock<ProgressTracker>` needs explicit configuration.** `Increment` and `SpawnChild` return `ProgressTracker`; an unconfigured loose mock returns null and line 87 NREs. Verified all four members used are `virtual` (`ProgressTracker.cs:109,121,141,218`). Note a bare `new ProgressTracker(cts)` without `Initialize()` NREs inside `Report(double)`, so tests must mock rather than construct — the same finding the `QfcDatamodel.cs` artifact recorded as its R7. | Test-arrangement complexity only. | Configure `Increment(It.IsAny<double>())` and `SpawnChild(It.IsAny<int>())` to return the mock itself, in the shared `CreateProgressMock()` helper (T-file C/D). |
| **R5** | **Cross-child incidental coverage.** Cases 1, 2, 6, 7 exercise `EmailSorter.GetSortKey` / `GetDateKey`, and `EmailSorter.cs` is assigned to **F2** (epic.md:262, `QuickFiler.csproj:312`). | F2's per-file evidence may double-count these lines; the epic's disjointness assumption is about *production file ownership*, which is not violated. | Do **not** move or modify `EmailSorter.cs`. Record as a cross-child observation in `spec.md`, mirroring the sibling's R6 handling of the mislabelled `QfcDatamodelTests.cs` tests. |
| **R6** | **F1 ledger disagreement.** This artifact asserts `QfcDatamodel.FrameBuilding.cs` = `testable`, zero exempt members. F1's ledger does not exist on disk yet (expected — F1 is prepared concurrently). | If F1 ratified an exemption for this file, §0.1 and §6.4 change and issue.md AC2 changes with them. | Treat the ledger as authoritative on arrival. A ratified exemption on a file this seamable would be inconsistent with epic.md §1 and should be escalated rather than accepted. |
| **R7** | **Attribute-removal sequencing (now unblocked).** The sibling artifact made removal conditional on this file having either seams or member-level exemptions. | This artifact supplies the seams, so the condition is satisfied. | The removal task remains the **last** production task of the feature. It must be sequenced after this phase's S5/S6 land, and after the `QueueProcessing` phase's tests, because the attribute admits all three partials at once. |
| **R8** | **Per-file percentage fragility if §4 is taken.** Extraction shrinks `FrameBuilding.cs` to ~35 instrumented lines, where four uncovered lines is 89% and eight is 77%. | Medium. A small denominator amplifies any miss. | Measure with F1's harness after the phase's final task, not by projection. If the margin is thin, sever §4 (the file returns to ~55 lines) — §4.3 confirms this is a clean severing. |
| **Q1** | Should B5/B6 be narrowed from `public` to `internal` now that §2.1 proves they have no external consumer? | Public-surface minimality. | **No.** Recommend keeping them `public`. Narrowing is churn with no coverage or contract benefit, and §7's cases call them directly. Revisit only if F16 audits public surface. |
| **Q2** | Should the duplicated three-step pipeline at lines 18/21/24 and 56/59/63 be deduplicated even if §4 is severed? | Reusability (CLAUDE.md Design Principle 2). | Only as part of §4. A private helper on the partial class would achieve the dedup without the new file, but forfeits the host-neutrality that is §4's primary justification. |

---

## 10. Files this phase would touch

| Path | Action |
| --- | --- |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | Edit two call sites (lines 15, 82–89) for S5/S6; if §4 is taken, replace B5/B6 bodies with one-line wrappers and collapse the two pipelines to `QfcEmailFrameShaper.Shape` |
| `QuickFiler/Controllers/QfcEmailFrameShaper.cs` | **New** (only if §4 is taken). Host-neutral pure shaping logic |
| `QuickFiler/Controllers/QfcDatamodel.Construction.cs` | **Sibling-owned.** Add the S5 and S6 declarations there per the one-DI-surface note (§5.4) |
| `QuickFiler.Test/Controllers/QfcEmailFrameShaperSortTests.cs` | **New.** Cases 1–8 |
| `QuickFiler.Test/Controllers/QfcEmailFrameShaperConversationTests.cs` | **New.** Cases 9–15 |
| `QuickFiler.Test/Controllers/QfcDatamodelInitDfTests.cs` | **New.** Cases 16–19 |
| `QuickFiler.Test/Controllers/QfcDatamodelInitDfAsyncTests.cs` | **New.** Cases 20–23 |
| `QuickFiler.Test/Controllers/QfcDatamodelEmailsInViewTests.cs` | **New.** Cases 24–30 |
| `QuickFiler/QuickFiler.csproj` | One `<Compile Include>` entry (only if §4 is taken) |
| `QuickFiler.Test/QuickFiler.Test.csproj` | Five `<Compile Include>` entries (§7.2) |

Explicitly **not** touched: `coverage.config`, any shared build property file,
`QuickFiler/Controllers/QfcDatamodel.cs`, `QfcDatamodel.QueueProcessing.cs`, `IQfcDatamodel.cs`,
`EfcDataModel.cs`, `EmailSorter.cs` (F2), `QfcStreamingDequeueConfidenceGate.cs` (F2),
`QuickFiler/Helper Classes/**` (F4), any `Viewers/**` file (F14/F15), `UtilitiesCS/Extensions/DfDeedle.cs`,
`UtilitiesCS/Properties/AssemblyInfo.cs`, or any existing test file.

**File-size projection.** `QfcDatamodel.FrameBuilding.cs`: 154 lines today → ~128 with §4 taken
(−40 for the two moved bodies, +12 for wrappers with XML docs, +2 for the call-site edits) or ~158
with §4 severed. `QfcEmailFrameShaper.cs` ≈ 50. Every file in the partial family stays well under
500: `QfcDatamodel.cs` ~311, `QfcDatamodel.Construction.cs` ~178 (sibling estimate + S5/S6),
`QfcDatamodel.QueueProcessing.cs` 177, `QfcDatamodel.FrameBuilding.cs` ~128.

---

## 11. Mechanics the plan must not omit

1. **Every new file needs an explicit `<Compile Include>` entry** — both csproj files are legacy
   non-SDK projects with explicit item lists (verified: `QuickFiler.csproj:312-315`,
   `QuickFiler.Test.csproj:90-145`). Omission fails silently.
2. **Coverage evidence goes to
   `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/evidence/qa-gates/`** per
   `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Do not write to `artifacts/`.
3. **Toolchain order is fixed:** `csharpier .` → analyzer msbuild → nullable msbuild →
   `vstest.console.exe … /EnableCodeCoverage`, restarting from step 1 on any failure or file change.
4. **Preserve `.ConfigureAwait(false)`** at lines 50 and 89 verbatim when editing the S6 call site —
   invariant I8 in the QueueProcessing artifact records that dropping it deadlocks the Outlook UI
   thread.
5. **Correct `issue.md:80-82`** (the WinForms-layout claim) in `spec.md`, and record that the STA
   determination for this file is negative with a count of zero.
6. **The attribute-removal task stays last** and must follow this phase (§9 R7).
