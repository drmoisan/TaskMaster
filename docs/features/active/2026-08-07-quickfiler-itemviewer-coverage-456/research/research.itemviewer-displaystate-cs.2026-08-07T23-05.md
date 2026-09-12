# Research — `QuickFiler/Viewers/ItemViewer.DisplayState.cs`

- Feature: F14 `quickfiler-itemviewer-coverage` (issue #456), child of epic #136 `quickfiler-per-file-coverage`
- Timestamp: 2026-08-07T23-05
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5e4b635834feedd7`
- Target file: `QuickFiler/Viewers/ItemViewer.DisplayState.cs` (81 physical lines)
- Compile entry: `QuickFiler/QuickFiler.csproj:415-418` (`<DependentUpon>ItemViewer.cs</DependentUpon>`, `<SubType>UserControl</SubType>`)

Claims are marked **[V]** (verified by direct file read, grep, or fetched issue text) or **[I]**
(inferred). No Bash tool and therefore no `gh` was available; GitHub issue text was obtained by WebFetch
and is marked **[V-web]**.

---

## 0. Premise verification

| # | Supplied premise | Verdict | Evidence |
|---|---|---|---|
| P1 | `ItemViewer` is a `UserControl`, not a `Form` | **CONFIRMED [V]** | `QuickFiler/Viewers/ItemViewer.cs:21` |
| P2 | This file carries no real `[ExcludeFromCodeCoverage]`; `:9` only mentions it | **CONFIRMED [V]** | Full read of all 81 lines. The token appears only inside the comment at `:6-10`, which reads *"The whole ItemViewer type is [ExcludeFromCodeCoverage] via its primary partial in ItemViewer.cs; the attribute is not (and cannot be) repeated here (CS0579, non-AllowMultiple)."* The in-file note about **CS0579** independently corroborates the sibling artifact's finding that a partial type cannot carry the attribute twice (`research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md:88-91`). |
| P3 | No `ItemViewer.*` partial in the committed Cobertura report | **CONFIRMED [V]** | Recorded and re-checked in `research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md:20` against `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`. |
| P4 | Assume ~0% measured coverage | **CONFIRMED for this file [V]** | Repo-wide grep for the 12 member names returns production writers (`QfcItemController.ViewerSetup.cs:363-368`, `QfcItemController.cs:203-232`, `QfcItemController.Conversation.cs:165-213`, `QfcItemController.EventWiring.cs:75`, `QfcItemController.MailActions.cs:64`) and `Mock<IItemViewer>` setups — **no test executes a line of this file**. A `Mock<IItemViewer>` never enters the production forwarder. |
| P5 | `InternalsVisibleTo("QuickFiler.Test")` granted by QuickFiler | **CONFIRMED [V]** `QuickFiler/Properties/AssemblyInfo.cs:5`. Not needed here — all 12 members are already `public`. |
| P6 | `TimeProvider`/`FakeTimeProvider` unavailable on net481 | **DISPROVED [V]** (recorded by the sibling artifact and not re-derived): `QuickFiler.Test/packages.config:18` and `:85-88` pin `Microsoft.Bcl.TimeProvider 10.0.10` and `Microsoft.Extensions.TimeProvider.Testing 10.8.0`; production reference at `QuickFiler/QuickFiler.csproj:68-69`. **Moot for this file** — it reads no clock (§4). |
| P7 | Issue #441 corrupts `<class>` `line-rate` | **CONFIRMED [V]** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:121-122` (descendant `.//lines/line` axis). |

---

## 1. What the file is

Twelve members: ten forwarding properties, one forwarding event, one forwarding method. Every member
projects onto a Designer-backed control obtained through the public wrapper properties in
`ItemViewer.cs`'s `#region Field to Property for Interface` (`ItemViewer.cs:207-430`). The file's own
comment (`:6-8`) is accurate: *"Forwarding implementations for the narrowed IItemViewer display-state
intent members (Seam B, Cluster 2a). Each member forwards to the existing Designer-backed control so the
underlying WinForms controls remain private to the view while the controller consumes intent."*

Backing control types **[V]** (`ItemViewer.cs:209-283`):

| Member | Backing wrapper | Wrapper declared at | Control type |
|---|---|---|---|
| `SenderText` | `LblSender` | `ItemViewer.cs:214` | `System.Windows.Forms.Label` |
| `SubjectText`, `FocusSubject()` | `LblSubject` | `:244` | `Label` |
| `BodyText`, `BodyDoubleClick` | `TxtboxBody` | `:279` | `System.Windows.Forms.TextBox` |
| `TriageText` | `LblTriage` | `:224` | `Label` |
| `SentOnText` | `LblSentOn` | `:239` | `Label` |
| `ActionableText` | `LblActionable` | `:234` | `Label` |
| `ItemNumberText` | `LblItemNumber` | `:209` | `Label` |
| `FolderText` | `LblFolder` | `:259` | `Label` |
| `ConversationCountText`, `ConversationCountBackColor` | `LblConvCt` | `:249` | `Label` |

All wrappers have **public setters**, which enables the seam-free fixture in §5.

---

## 2. Q1 — Member-by-member classification (exhaustive)

**Zero COM-bound members.** The file's `using` set is `System` and `System.Drawing` only (`:1-2`); a grep
for `Microsoft.Office` returns nothing.

| # | Member | Lines | Coverable lines | Branches | Class | Rationale |
|---|---|---|---|---|---|---|
| 1 | `SenderText` get/set | 13-17 | 15, 16 | 0 | **thin wiring** | `LblSender.Text` |
| 2 | `SubjectText` get/set | 19-23 | 21, 22 | 0 | thin wiring | `LblSubject.Text` |
| 3 | `BodyText` get/set | 25-29 | 27, 28 | 0 | thin wiring | `TxtboxBody.Text` |
| 4 | `TriageText` get/set | 31-35 | 33, 34 | 0 | thin wiring | `LblTriage.Text` |
| 5 | `SentOnText` get/set | 37-41 | 39, 40 | 0 | thin wiring | `LblSentOn.Text` |
| 6 | `ActionableText` get/set | 43-47 | 45, 46 | 0 | thin wiring | `LblActionable.Text` |
| 7 | `ItemNumberText` get/set | 49-53 | 51, 52 | 0 | thin wiring | `LblItemNumber.Text` |
| 8 | `FolderText` get/set | 55-59 | 57, 58 | 0 | thin wiring | `LblFolder.Text` |
| 9 | `ConversationCountText` get/set | 61-65 | 63, 64 | 0 | thin wiring | `LblConvCt.Text` |
| 10 | `ConversationCountBackColor` get/set | 67-71 | 69, 70 | 0 | thin wiring | `LblConvCt.BackColor` |
| 11 | `BodyDoubleClick` add/remove | 73-77 | 75, 76 | 0 | thin wiring | `TxtboxBody.DoubleClick` |
| 12 | `FocusSubject()` | 79 | 79 | 0 | thin wiring | `LblSubject.Focus()` |

**Totals: 23 coverable lines, 0 branch points, 0 pure/host-neutral members, 12 thin-wiring members,
0 COM-bound members.**

Structural finding, identical in kind to `ItemViewer.Commands.cs`: **no `if`, no `?.`, no `??`, no
`&&`/`||`, no ternary, no switch, no loop, no lambda.** The `>= 75%` branch gate is therefore vacuous for
this file, and the line gate is the only real gate. F1's harness must render a zero-condition class as
branch `N/A`/`100%`, never `0%` (see the harness recommendation in §9).

---

## 3. Q3 — Display-state transitions

### 3.1 The supplied premise is disproved

The brief describes this file as *"a state-holding surface"* and asks for states, legal transitions, and
invariants. **The file holds no state and defines no state machine. [V]** Evidence:

- It declares **zero fields**. Full read of all 81 lines: there is no `private`/`internal`/`static`
  field, no auto-property with a backing field, and no `readonly`.
- Every property is a pair of expression-bodied accessors over a *foreign* object's property. The state
  lives in `System.Windows.Forms.Label.Text` / `TextBox.Text` / `Control.BackColor`, i.e. in the
  framework, not here.
- There is no validation, no guard clause, no ordering requirement between members, and no member whose
  legal set of inputs depends on the value of any other member. Any member may be written at any time in
  any order.

`.claude/rules/general-unit-test.md` § Scenario Completeness requires "state transitions for stateful
components." **This file is not a stateful component**, and a test suite that manufactures a state
machine for it would assert framework behaviour rather than repository behaviour. What the policy
requirement resolves to, correctly applied here, is **round-trip and normalisation coverage of each
projection**, which is what §3.2 enumerates and §5 turns into cases.

If the epic's capstone (F16) expects a "state transition" artefact for this file, the correct answer to
record is: *the display state of an `ItemViewer` is a 10-tuple of independent, unconstrained control
property values; the transition relation is the full cross-product; there are no illegal transitions
because there are no invariants to violate.*

### 3.2 The observable state model, and the four real semantics a test must pin

Although there is no state machine, three of the twelve projections have **non-identity** round-trip
semantics inherited from WinForms. These are the only places where a naive `set X; get X.Should().Be(X)`
assertion can fail, and they are exactly what an atomic test task must be told about in advance.

| # | Semantic | Members affected | Behaviour | Status |
|---|---|---|---|---|
| S1 | **Initial state** | all nine `*Text` members | `Control.Text` on a freshly constructed `Label`/`TextBox` is `string.Empty`, not `null`. So `SenderText` on a viewer with a fresh `LblSender` returns `""`. | **[I]** — documented .NET Framework behaviour; the test must observe it rather than assume it |
| S2 | **Null normalisation** | all nine `*Text` members | `Control.Text` normalises an assigned `null` to `string.Empty`. Therefore `SenderText = null; SenderText` returns `""`, **not `null`**. This is a genuine asymmetry a controller could depend on: `QfcItemController.ViewerSetup.cs:363-368` assigns `itemInfo.SenderName`/`.Subject`/`.Body`/`.Triage`/`.SentOn`/`.Actionable` directly, any of which may be null from a COM read. | **[I]** — same status; **assert the observed behaviour, and comment that the normalisation is the framework's, not this file's** |
| S3 | **Idempotent write** | all nine `*Text` members | `Control.Text` short-circuits when the assigned value equals the current value, so `TextChanged` is not re-raised. Relevant because `BodyText` writes to a `TextBox`. | **[I]** — do not assert `TextChanged` counts unless a case needs them |
| S4 | **`BackColor` is not a plain field** | `ConversationCountBackColor` (`:67-71`) | `Control.BackColor`'s getter returns the *effective* colour: when nothing was explicitly assigned it returns the parent's or `Control.DefaultBackColor`, so the initial getter value is `SystemColors.Control`, **not `Color.Empty`**. Assigning `Color.Empty` **resets** to the inherited value rather than storing `Color.Empty`. Production only ever assigns a concrete colour (`QfcItemController.Conversation.cs:168,190,213` assign `Color.Red`). | **[I]** — the reset-on-`Color.Empty` behaviour is a real edge case worth one case; **do not** assert on `Color.Transparent` (it throws `ArgumentException` on controls without `SupportsTransparentBackColor`, unverified for `Label`) |

### 3.3 `FocusSubject()` — the one member with a host-dependent outcome

`ItemViewer.DisplayState.cs:79` is `public void FocusSubject() => LblSubject.Focus();`.

- **Production trigger [V]:** exactly one call site, `QfcItemController.MailActions.cs:64`.
- **Headless behaviour [I]:** `Control.Focus()` calls `Control.FocusInternal()`, which calls the Win32
  `SetFocus` only when `CanFocus` is true, and `CanFocus` requires `IsHandleCreated`. A `Label`
  constructed in a test and never parented or shown has no handle, so `Focus()` performs no Win32 call
  and returns `false`. **No exception, no window creation, no message pump.** This is what makes line 79
  coverable with no seam and no STA.
- **Do not assert `FocusSubject()` returns/achieves focus.** The method returns `void` and discards the
  `bool`; the only assertable outcome headless is "does not throw" plus "`LblSubject.Focused` remains
  `false`".
- See LD-2 for the design defect this member carries.

---

## 4. Q2 — Seam recommendation and determinism

**Recommendation: introduce NO seam. No new production type, no new production file, no change to
`QuickFiler/QuickFiler.csproj`.**

Applying the epic hierarchy (`epic.md:227-232`) requires first asking whether any line is unreachable
without a seam. **None is.** All 23 lines are reachable from a plain `[TestClass]` because:

1. The nine backing controls are injectable today through `public` setters on `ItemViewer`
   (`ItemViewer.cs:209-283`), and the in-repo precedent uses precisely this route for three of them:
   `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:250-255` assigns `viewer.LblItemNumber`,
   `viewer.LblSender`, `viewer.LblSubject`, `viewer.TxtboxBody` on a `CreateUninitialized<ItemViewer>()`
   (`:249`, helper at `:331-335`).
2. `Label`, `TextBox`, `Label.Text`, `TextBox.Text`, `Control.BackColor`, `Control.DoubleClick`, and
   `Control.Focus()` all operate without a window handle, a parent, or a message pump.
3. Nine existing `QuickFiler.Test` classes already construct a full `new QuickFiler.ItemViewer()` in a
   plain `[TestClass]` (enumerated in the `ItemViewer.Commands.cs` artifact §3), which is strictly more
   host machinery than this file's tests need.

Extracting the nine text projections into a host-neutral `ItemViewerDisplayProjection` class would
require passing nine `Control` references into it — relocating the WinForms dependency without removing
it, adding a file to the denominator under the `>= 90%` new-file rule (`epic.md:583-585`), and adding
indirection the General Code Change Policy § Design Principle 1 disfavours. **Rejected.**

Retyping the wrapper properties to `ILabel`/`ITextBox` abstractions is also **rejected**: the wrappers
are consumed by concrete type in `QfcThemeHelperTests.cs:250-255` and by the theming code, and the
sibling artifact records a pinned-contract precedent that makes retyping of `ItemViewer`'s control
wrappers a red test by construction (`research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md:256-266`).

### 4.1 Determinism

**No clock, no timer, no randomness, no async. [V]** Full read of all 81 lines: zero occurrences of
`DateTime`, `DateTimeOffset`, `Stopwatch`, `Timer`, `Task`, `async`, `await`, `Random`,
`Thread.Sleep`, or `Task.Delay`. No `TimeProvider` seam is needed and none should be introduced. Tests
for this file are synchronous and deterministic by construction.

---

## 5. Q6 — Test plan

### 5.1 Fixture

```
U = FormatterServices.GetUninitializedObject(typeof(ItemViewer)) cast to ItemViewer,
    then assign only the Label/TextBox the case needs via the public wrapper setter.
```

Precedent: `QfcThemeHelperTests.cs:247-265` and helper `:331-335`. No `SynchronizationContext` is
required (the constructor is not run, so `ItemViewer.cs:26-27` never executes). No `Dispose` obligation.

`BodyDoubleClick` verification uses reflection on the protected `Control.OnDoubleClick(EventArgs)`,
mirroring `QfcThemeHelperTests.cs:277-285` (`InvokeControlEvent`), which already does this for
`OnMouseEnter`/`OnMouseLeave`.

MSTest `[TestClass]`/`[TestMethod]`, Moq (none needed), FluentAssertions, Arrange–Act–Assert, no temp
files, no external services, no live Form, no popup, no `Thread.Sleep`/`Task.Delay`/wall-clock wait.

**Proposed home:** `QuickFiler.Test/Viewers/ItemViewerDisplayStateForwardingTests.cs` (new). Requires one
`<Compile Include="Viewers\ItemViewerDisplayStateForwardingTests.cs" />` entry in
`QuickFiler.Test/QuickFiler.Test.csproj` (CRLF preserved, minimal adjacent hunk). Projected size at 16
cases with a shared fixture region: ~300 lines — under the 500-line limit.

### 5.2 Case inventory

Gate arithmetic: 23 coverable lines; `>= 80%` line means `>= 19` lines. The 16 cases below cover
**23/23 = 100%**. Branch gate vacuous (§2).

| # | Test name | Production lines covered | Semantic pinned | Seam | Mocks |
|---|---|---|---|---|---|
| D1 | `SenderText_RoundTripsSenderLabelText` | 15, 16 | S1 + identity round-trip | `LblSender` setter (`ItemViewer.cs:217`) | none |
| D2 | `SubjectText_RoundTripsSubjectLabelText` | 21, 22 | identity | `LblSubject` setter | none |
| D3 | `BodyText_RoundTripsBodyTextBoxText` | 27, 28 | identity | `TxtboxBody` setter | none |
| D4 | `TriageText_RoundTripsTriageLabelText` | 33, 34 | identity | `LblTriage` setter | none |
| D5 | `SentOnText_RoundTripsSentOnLabelText` | 39, 40 | identity | `LblSentOn` setter | none |
| D6 | `ActionableText_RoundTripsActionableLabelText` | 45, 46 | identity | `LblActionable` setter | none |
| D7 | `ItemNumberText_RoundTripsItemNumberLabelText` | 51, 52 | identity; use the two formats production writes (`"7"` and `"07"`, from `QfcItemController.cs:203-232`) | `LblItemNumber` setter | none |
| D8 | `FolderText_RoundTripsFolderLabelText` | 57, 58 | identity | `LblFolder` setter | none |
| D9 | `ConversationCountText_RoundTripsConversationCountLabelText` | 63, 64 | identity | `LblConvCt` setter | none |
| D10 | `TextProjections_AssignedNull_ReadBackAsEmptyString` | 15, 16 (re-hit) | **S2** — asserts the framework normalisation on one representative member (`SenderText`), with an in-code comment that `Control.Text` performs the normalisation and this file adds none. **One case, not nine.** | `LblSender` setter | none |
| D11 | `TextProjections_InitialState_AreEmptyStringNotNull` | 15 (re-hit) | **S1** on one representative member | `LblSender` setter | none |
| D12 | `ConversationCountBackColor_RoundTripsLabelBackColor` | 69, 70 | assigns `Color.Red` — the exact value production assigns at `QfcItemController.Conversation.cs:168,190,213` | `LblConvCt` setter | none |
| D13 | `ConversationCountBackColor_AssignedColorEmpty_ResetsToInheritedBackColor` | 69, 70 (re-hit) | **S4** — the non-identity edge case; asserts the getter returns the label's default rather than `Color.Empty` | `LblConvCt` setter | none |
| D14 | `BodyDoubleClick_AddThenRemove_SubscribesAndUnsubscribesTextBoxDoubleClick` | 75, 76 | subscribe → reflect `OnDoubleClick` → expect 1; unsubscribe → reflect again → expect still 1 | `TxtboxBody` setter | none |
| D15 | `FocusSubject_OnHeadlessViewer_DoesNotThrowAndLeavesSubjectUnfocused` | 79 | §3.3 — the only assertable headless outcome | `LblSubject` setter | none |
| D16 (optional) | `DisplayStateMembers_OnViewerWithUnassignedControls_ThrowNullReference` | none new | documents in **one** case that this file has no null guards by design | — | none |

### 5.3 Cases the planner should NOT author

- **Do not** author nine separate null-normalisation cases (one per `*Text` member). They exercise
  framework behaviour, add zero lines beyond what D1-D9 already cover, and would add nine atomic tasks
  for no measurable gain. D10 covers the semantic once.
- **Do not** author twelve `NullReferenceException` cases. Same reasoning as the `ItemViewer.Commands.cs`
  artifact §5.3: the throwing line is already covered by the positive case, and asserting the throw pins
  an unguarded dereference as a contract. D16 covers it once if a reviewer requires a negative flow.
- **Do not** assert that `FocusSubject()` moves focus. §3.3.

### 5.4 STA determination

**No case requires the STA clause.** `Label`, `TextBox`, `Control.Text`, `Control.BackColor`,
`Control.DoubleClick`, and a handle-less `Control.Focus()` need no apartment, no handle, and no message
pump. The empirical proof is stronger than the argument: nine existing `QuickFiler.Test` classes already
construct the entire `ItemViewer` Designer tree in plain `[TestClass]`es, and `QfcThemeHelperTests.cs`
already constructs and drives `Label`/`TextBox` instances assigned into an `ItemViewer` the same way.
**Do not create the first `*.StaTests.cs` in `QuickFiler.Test` for this file.**

---

## 6. Q7 — 500-line rule

- Current: **81 physical lines**. Limit 500. Headroom **419 lines**.
- Projected additions: **zero** — §4 recommends no seam.
- **Projected post-refactor: 81 lines. No split. No `<Compile Include>` addition to
  `QuickFiler/QuickFiler.csproj`. No mid-wave ledger row for a new production file.**

---

## 7. Cross-child notes

**F14 requires zero changes from any sibling child for this file.**

| Symbol | Declared in | Owner | Sufficient as-is? |
|---|---|---|---|
| `LblSender`, `LblSubject`, `LblTriage`, `LblSentOn`, `LblActionable`, `LblItemNumber`, `LblFolder`, `LblConvCt`, `TxtboxBody` wrapper properties | `ItemViewer.cs:209-283` | **F14 (own)** | yes — `public` get/set |
| `IItemViewer` members `:43-54` | `QuickFiler/Viewers/IItemViewer.cs` | **F14 (own)** | yes; the sibling artifact `research.iitemviewer-cs.2026-08-07T21-40.md` records that F14 proposes **no edit** to that file, and this artifact does not contradict it |
| `QfcItemController.ViewerSetup.cs:363-368`, `QfcItemController.cs:203-232`, `QfcItemController.Conversation.cs:165-213`, `QfcItemController.EventWiring.cs:75`, `QfcItemController.MailActions.cs:64` | `QuickFiler/Controllers/` | **F10** (issue #453) | consumer only; **no edit requested to any `QfcItemController.*` file** |

No dependency on F3 (keyboard actions), F12 (breadcrumb bridge), F13 (breadcrumb drop-down / WebView2),
or F15. No `UtilitiesCS` internal is required, so the `epic.md:619-631` constraint is not engaged and
`UtilitiesCS/Properties/AssemblyInfo.cs` is not touched.

**X-D1 (intra-F14 ordering, not cross-child).** As with every `ItemViewer` partial, no per-file number
exists until the type-level `[ExcludeFromCodeCoverage]` at `ItemViewer.cs:20` is removed. That single
decision governs all six hand-written partials plus `ItemViewer.Designer.cs`; it is analysed in
`research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md:69-102` and is not re-decided here. Note that open
issue **#457** (§9) is new evidence bearing on it.

---

## 8. Latent defect promotion candidates

Promotion candidates for the MCP lifecycle per `epic.md:538-543`. Out of scope to fix under F14's
no-behavior-change NFR.

### LD-1 — `ItemViewer` exposes ten mutable display projections with no consistency guarantee, allowing a partially-applied item render

`ItemViewer.DisplayState.cs:13-71` exposes `SenderText`, `SubjectText`, `BodyText`, `TriageText`,
`SentOnText`, `ActionableText`, `ItemNumberText`, `FolderText`, `ConversationCountText`, and
`ConversationCountBackColor` as ten independently settable properties with no transactional grouping.
`QfcItemController.ViewerSetup.cs:363-368` writes six of them in sequence from one `itemInfo`, and
`QfcItemController.Conversation.cs:165-213` writes `ConversationCountText` and
`ConversationCountBackColor` from a different code path at a different time. Any exception thrown
mid-sequence — for example a COM read of `itemInfo.Body` failing — leaves the viewer showing a mixture of
the new item's sender and the previous item's body, with no indication to the user. Because `ItemViewer`
instances are pooled and reused (`QfcItemController.ViewerSetup.cs:396` calls `ResetBreadcrumb()` on
reuse, and there is no corresponding `ResetDisplayState()`), the stale fields persist across items. A
single `ApplyItemInfo(...)` intent member that writes all ten atomically, or an explicit clear-before-fill,
would close this. This is a design defect surfaced by the coverage read, not a reported failure.

### LD-2 — `FocusSubject()` targets a `Label`, which is a non-selectable control, and its result is discarded

`ItemViewer.DisplayState.cs:79` is `public void FocusSubject() => LblSubject.Focus();`, and `LblSubject`
is a `System.Windows.Forms.Label` (`ItemViewer.cs:244`). `Label` sets
`ControlStyles.Selectable` to `false`, so it is excluded from tab order and from
`Control.SelectNextControl` traversal; focusing it via `Control.Focus()` puts keyboard focus on a control
that cannot meaningfully receive keystrokes and that the user cannot leave by pressing Tab in the normal
way. `Control.Focus()` returns a `bool` reporting whether focus was actually taken, and this call
**discards it** — so a failed focus is silent. The single production call site is
`QfcItemController.MailActions.cs:64`. Note the asymmetry with the sibling member in the same type:
`ItemViewer.FolderSearch.cs:72` marshals its focus call through `TxtboxSearch.Invoke(...)` while this one
does not, so the two focus members of one control have different threading discipline (see the
`ItemViewer.FolderSearch.cs` artifact, LD-2 there). Both the target-control choice and the discarded
result should be reviewed together with issue **#438**, which is about focus behaviour in the same
viewer.

### LD-3 — Stale exemption comment at `ItemViewer.DisplayState.cs:9-10`

The comment asserts that the type is `[ExcludeFromCodeCoverage]`. It is true today and becomes false the
moment F14 removes `ItemViewer.cs:20`. Identical stale comments exist at `ItemViewer.Commands.cs:10` and
`ItemViewer.FolderSearch.cs:17`. These three comments are the direct cause of the epic's original
33-file over-count (`epic.md:121-130`). **In-scope for F14's own execution** — update all three in the
same change that removes the attribute. Not a promotion candidate; listed so the planner schedules the
task. The parenthetical about **CS0579** at `:10` is accurate and should be *retained* (moved, if the
comment is rewritten) because it documents why per-partial exemption is impossible — a fact the plan
depends on.

---

## 9. Open-issue scan

**Method:** WebFetch against the public GitHub issue pages for `drmoisan/TaskMaster` (no Bash tool, so no
`gh`). Terms run: `ItemViewer`, `focus OR viewer OR "folder search"`, `coverage`. Issues fetched in
full: #438, #444, #445, #457.

| Issue | Title | Bearing on `ItemViewer.DisplayState.cs` |
|---|---|---|
| **#457** | `excludefromcodecoverage-does-not-suppress-nested-lambdas` **[V-web]** | **Indirect, but bears on F14's shared attribute decision.** The issue establishes that `[ExcludeFromCodeCoverage]` does not suppress compiler-hoisted lambdas, so attribute-based suppression is not a reliable instrument. **This file contains no lambda** (§2), so its own numbers are unaffected. Cite it in support of the filename-based Designer-exclusion recommendation in `research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md:93-101`. |
| **#441** | Cobertura post-processing double-counts `<line>` nodes | Direct. Annotate any quoted `line-rate` as "#441 — unreliable"; use F1's recomputed figure. |
| **#432** | `quickfiler-coverage-ledger` (F1) | Direct — this file needs a `testable` ledger row. **Harness requirement discovered here and in the `ItemViewer.Commands.cs` artifact:** a class with zero `<condition>` children must report branch `N/A`/`100%`, never `0%`. `epic.md:533-536` states the analogous rule for the line denominator only; recommend F1 extend it to branches. |
| **#438** | `quickfiler-search-keystroke-focus-steal` **[V-web]** | **Adjacent, worth cross-referencing.** #438 is about focus leaving `TxtboxSearch`; the fix will touch focus routing inside `ItemViewer`. `FocusSubject()` (`:79`) is the other focus member on the same control and carries LD-2. Whoever schedules #438 should review LD-2 at the same time. No edit to this file is implied by #438 today. |
| **#444**, **#445** | keyboard-action contract defects | **No bearing.** A grep of the whole `QuickFiler/Viewers/` folder for `KbdActions`, `KaChar`, `KaKey`, `KaStringAsync`, `IMailItemActions` returns **no matches**. |
| **#230** | WinForms message-pump test seam | Not needed — §5.4. |
| **#254 / #269** family (dark-mode stale labels; closed/historical) | — | Contextually adjacent: those defects concerned theming of `LblSender`/`LblSubject`, the same two labels this file projects. No live dependency; recorded so the planner does not re-derive the connection. |

No open issue targets `ItemViewer.DisplayState.cs` directly.

---

## 10. Verified vs inferred

**Verified:**

- The file's full contents, its 12 members, its two `using` directives, its zero fields, its zero branch
  points, its zero lambdas, and its lack of a real `[ExcludeFromCodeCoverage]`.
- The concrete backing control types and the `public` get/set visibility of all nine wrappers
  (`ItemViewer.cs:209-283`).
- The single production call site of `FocusSubject()` (`QfcItemController.MailActions.cs:64`) and the
  production writers of the other eleven members.
- That no test currently executes any line of this file (all existing exposure is through
  `Mock<IItemViewer>`).
- That `QuickFiler/Viewers/` contains no reference to the F3-owned keyboard-action types.
- That nine `QuickFiler.Test` classes construct a real `ItemViewer` in a plain `[TestClass]`, and that
  `QfcThemeHelperTests.cs:247-265,277-294,331-335` supplies the uninitialised-viewer fixture, the
  protected-event reflection helper, and the private-field setter.
- The compile entry at `QuickFiler.csproj:415-418`.

**Inferred** (reasoning from framework semantics; not executed, because no code-execution tool was
available):

- S1-S4 in §3.2 (`Control.Text` initial value, null normalisation, idempotent write; `Control.BackColor`
  effective-value getter and `Color.Empty` reset). Each is stated as the *expected* behaviour that cases
  D10, D11, D12, and D13 must **observe and assert**, not assume.
- That a handle-less `Label.Focus()` returns `false` without throwing (§3.3), which is what makes D15
  safe. If this proves wrong at execution time, D15 becomes the single case that would need the STA
  clause — and even then, dropping D15 costs 1 of 23 lines (95.7% remaining), so the gate survives
  without it.
- That the 16 cases measure 23/23 lines; the exact sequence-point mapping for expression-bodied
  accessors was not confirmed against a Cobertura report for this file (none exists yet).
