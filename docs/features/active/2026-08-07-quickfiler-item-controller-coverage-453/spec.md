# quickfiler-item-controller-coverage — Spec

- **Issue:** #453 (https://github.com/drmoisan/TaskMaster/issues/453)
- **Parent epic:** #136 QuickFiler Per-File 80% Coverage — child F10 (wave 1, complexity band C3)
- **Integration branch:** `epic/quickfiler-per-file-coverage-integration`
- **Feature branch:** `feature/quickfiler-item-controller-coverage`
- **Owner:** drmoisan
- **Work Mode:** `full-feature` (this file and `user-story.md` are the authoritative acceptance-criteria sources)
- **Last Updated:** 2026-08-07
- **Status:** Ready for Planning
- **Version:** 1.0

---

## 1. Objective

Bring the `QfcItemController` partial-class family to at least 80% line coverage and 75% branch
coverage per production file, measured with epic child F1's per-file harness on this child's own
branch, while **reconciling** — not overturning — the maintainer-ratified `[ExcludeFromCodeCoverage]`
boundary that already governs this family.

Two sentences define the shape of the work and should be read before anything else in this document:

1. **This is a gap-closure exercise, not a build-from-zero exercise.** 17 test files and 166
   `[TestMethod]`s already target this family
   (`research/cross-cutting-exemption-and-coverage-analysis.md` §3). Re-testing already-covered
   members is duplicated work and is explicitly out of scope.
2. **This is not an exemption-elimination exercise.** 18 of the family's 19 attributes were formally
   ratified by the project maintainer on 2026-07-02. F10 re-verifies them, resolves the one
   unratified attribute, and deletes the dead members that carry three of them. The realistic
   outcome is **19 attributes reduced to at most 15**, not 19 reduced to 0. Section 3 is the
   governing section of this specification.

No observable behavior change to QuickFiler flows. The epic's no-behavior-change NFR binds every
task in this child.

---

## 2. Scope

### 2.1 In scope — 11 files, one partial-class family

Ten `QuickFiler/Controllers/QfcItemController*.cs` partials plus one interface file. Line counts
verified on this branch during research.

| # | File | Lines | Attributes today |
| --- | --- | ---: | ---: |
| 1 | `QuickFiler/Controllers/QfcItemController.cs` | 323 | 0 |
| 2 | `QuickFiler/Controllers/QfcItemController.Initialization.cs` | 466 | 7 |
| 3 | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 426 | 3 |
| 4 | `QuickFiler/Controllers/QfcItemController.Conversation.cs` | 235 | 1 |
| 5 | `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 235 | 0 |
| 6 | `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 391 | 1 |
| 7 | `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | 219 | 5 |
| 8 | `QuickFiler/Controllers/QfcItemController.Navigation.cs` | 228 | 2 |
| 9 | `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 326 | 0 |
| 10 | `QuickFiler/Controllers/QfcItemController.MailActions.cs` | 224 | 0 |
| 11 | `QuickFiler/Interfaces/IQfcItemController.cs` | 107 | 0 (and must stay 0 — see §6) |
| | **Total** | **3,180** | **19** |

The 19 attribute sites were re-verified on this branch by direct grep and sit at
`Initialization.cs:138, 168, 200, 260, 291, 403, 436`; `ViewerSetup.cs:38, 132, 253`;
`Navigation.cs:173, 191`; `EventHandlers.cs:60, 83, 97, 111, 125`; `EventWiring.cs:99`;
`Conversation.cs:79`. All 19 are **member-level**; none sits on a `partial class` declaration.

Also in scope, as supporting edits:

- `QuickFiler.Test/Controllers/QfcItemController*.cs` — new and extended test fixtures.
- `QuickFiler/QuickFiler.csproj` — one `<Compile Include=...>` entry for the one new production file
  (§8.2). No other change to that file.
- `QuickFiler.Test/QuickFiler.Test.csproj` — one `<Compile Include=...>` entry per new test file.
- The F1 coverage ledger — rows for the 11 in-scope files plus the one new production file.
- `<FEATURE>/evidence/` — coverage evidence and the fresh exemption-boundary artifact.

### 2.2 Out of scope — files this child must not edit

| Asset | Owner | Why F10 must not touch it |
| --- | --- | --- |
| `QuickFiler/Helper Classes/**` (`ConversationResolver*.cs`, `QfcThemeHelper.cs`, `TlpCellSnapShot.cs`, `IConversationResolver.cs`, …) | **F4 (#434)** | Consumed read-only; contract notes in §10 |
| `QuickFiler/Controllers/KeyboardHandler.cs`, `KbdActions.cs`, `Ka*.cs`; `QuickFiler/Interfaces/IMailItemActions.cs`, `MailItemActionsAdapter.cs` | **F3 (#430)** | Consumed only through `IQfcKeyboardHandler` / `IMailItemActions` |
| `QuickFiler/Interfaces/IQfcDatamodel.cs`, `QfcDatamodel*.cs` | **F5 (#436)** | **Not referenced at all by the F10 file set** — see §11 deviation D3 |
| `QuickFiler/Controllers/QfcCollectionController.cs`, `Interfaces/IQfcCollectionController.cs` | **F11 (#454)** | Consumed through `Mock<IQfcCollectionController>` |
| `QuickFiler/Viewers/ItemViewer*.cs`, `IItemViewer.cs` | **F14 (#456)** | Consumed by interface and by headless construction only |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs` and the WebView2 trio | **F13 (#455)** | Consumed through `IWebViewCoreInitializer` |
| `UtilitiesCS/**`, including `Properties/AssemblyInfo.cs` | Outside every child's assignment | F10 does **not** hit the `InternalsVisibleTo` wall (§9.4); no grant change is to be proposed |
| `TaskVisualization/FlagTasks.cs` | Outside epic #136 | Seamed F10-locally instead (§8.1) |

Any change F10 would want in one of these files is recorded as a **cross-child contract note**
(§10), never made as an edit.

### 2.3 Out of scope — work

- Building the issue **#230** WinForms message-pump test seam. See §3.4.
- Fixing any latent defect found during research. See §12.
- Widening `IQfcItemController`. `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:340-460`
  contains a hand-written full implementation of the interface used as an F4-side test double;
  adding a member to `IQfcItemController` breaks that file's compilation and forces F10 to edit
  inside F4's blast radius.
- Any change to repository-wide coverage thresholds.
- Re-testing members already at 100%
  (`research/cross-cutting-exemption-and-coverage-analysis.md` §3.2 enumerates them).

---

## 3. Exemption governance — the central section

### 3.1 The boundary is already maintainer-ratified

On **2026-07-02**, under issue **#227**, the project maintainer formally ratified a 19-member
`[ExcludeFromCodeCoverage]` boundary for this family. Authority:

- `docs/features/archive/2026-06-29-qfc-item-controller-testability-227/maintainer-decision.2026-07-02.md`
  — "**Decision:** RATIFIED. The 19-member `[ExcludeFromCodeCoverage]` boundary ... is accepted."
- `docs/features/archive/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-07-02T17-00.md`
  — the ratified boundary itself, with a per-member justification.

The ratification was not casual. Five remediation cycles reduced the boundary from **103** members
(cycle 1, ratification **denied** 2026-07-01) to 41, to 24, to 19, with the maintainer explicitly
rejecting each intermediate count and asking whether it was genuinely the floor. The decision record
documents this pattern deliberately so that future reviewers can see it.

**The ratified 19 comprise:**

| Bucket | Count | Composition |
| --- | ---: | --- |
| 1. Concrete control-tree orchestration blocked by the unbuilt WinForms message-pump test seam | 9 | `Initialize(9-arg)`, `Initialize(bool)`, `InitializeAsync`, `InitializeGraphicsAsync`, `InitializeSequentialAsync`, `CreateAsync`, `CreateSequentialAsync`, `InitializeWebViewAsync`, `ResolveControlGroupsAsync` — tracked as **open issue #230**, explicitly declared NOT a merge condition |
| 2. Deliberate `virtual` test seams | 3 | `DoLoadConversationResolverCoreAsync`, `ToggleExpansion(ToggleState)`, `ToggleExpansionAsync(ToggleState)` |
| 3. `async void` WinForms event-signature shells whose `*Core` bodies are already tested | 6 | `BtnPopOut_Click`, `BtnReply_Click`, `BtnReplyAll_Click`, `BtnForward_Click`, `TxtboxBody_DoubleClick`, `WebView2Control_CoreWebView2InitializationCompleted` |
| 4. Genuine external-runtime dependency | 1 | `WebView2CoreInitializer` — **in F13's file set, not F10's** |
| | **19** | |

Because bucket 4's single member lives in `QuickFiler/Viewers/WebView2CoreInitializer.cs`, the
ratified boundary **within F10's file set is 18 members**. The 19th attribute in F10's files is
unratified drift and is dealt with in §3.3.

### 3.2 Consequence: epic AC2 as literally written cannot be satisfied, and must be reconciled

Epic #136's second acceptance criterion, as carried into this child's `issue.md` draft, reads: "the
count of QuickFiler files carrying `[ExcludeFromCodeCoverage]` on a testable seam falls to zero."
For this family that criterion cannot be met and must not be pursued as written.

- **F1's coverage ledger has no authority to overturn a maintainer decision.** F1 delivers a
  classification ledger and a measurement harness. It is not a ratification body.
- **F10 has no such authority either.** A child feature cannot re-adjudicate a decision the
  maintainer personally made after denying three weaker versions of it.
- **Therefore the ledger RECORDS the #227 ratification as the governing authority for this family.**
  F1's per-member disposition rows for the `QfcItemController` family must cite
  `maintainer-decision.2026-07-02.md` and `exemption-boundary.2026-07-02T17-00.md` as the basis for
  every retained attribute, rather than re-deriving a disposition from first principles.

This reconciliation is recorded here, in the child's authoritative acceptance-criteria source, so
that neither the executor, the reviewer, nor the F16 capstone treats a retained ratified attribute as
a Blocking finding.

### 3.3 Exactly one attribute is unratified drift

`EnsureBreadcrumbPipeline` at `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:132` is
**not** in the #227 boundary. It entered the file afterwards, via breadcrumb work (#351/#400), and
never went through the boundary process. It is the one attribute F10 must reduce or justify on its
own authority.

Its first statement is a type test — `if (!(_itemViewer is ItemViewer viewer)) { return; }`
(`ViewerSetup.cs:135-138`, verified on this branch) — so the early-return branch is reachable under
a `Mock<IItemViewer>` today, with zero host risk. The remainder is 20 lines of state management (a
null check, a provider construction from `Mock<IOlObjects>.FolderTreeService`, a reference
comparison, and an event subscribe/unsubscribe swap), all reachable through the headless real-viewer
fixture that already exists at
`QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:365-383` (`ViewerScope`)
and is already relied on by six passing tests. The stated in-code rationale ("Skipped for mock
viewers") describes the early-return branch, not a barrier.

**Disposition: remove the attribute and cover the member.** If execution finds a barrier that
research did not, the fallback is to retain it with a member-specific, evidence-backed rationale
recorded in the fresh exemption-boundary artifact on F10's own authority — never by appeal to #227,
which does not cover it.

### 3.4 F10 must NOT build the #230 message-pump seam

Issue **#230** ("Build a WinForms message-pump test seam (`Application.Run()` background thread) to
unblock 9 `QfcItemController` orchestration members", opened 2026-07-03, open) is the single shared
root cause of the ratified bucket-1 exemptions. The #227 maintainer decision states plainly that
#230 is "exploratory future work, not committed to any timeline, and is explicitly **NOT** a
condition of merging #227."

**Attempting to build that seam under F10 is a scope breach, not diligence.** It is materially
larger, distinct test infrastructure that would live in shared `QuickFiler.Test` infrastructure or
in `UtilitiesCS` — outside F10's file assignment — and the epic's decomposition did not budget for
it. F10 cites #230 by number as the externally-tracked justification for the bucket-1 attributes it
retains, and stops there.

### 3.5 All 18 ratified attributes must be re-verified against current code

The ratification is five weeks old and the code has moved. F10 therefore **re-verifies each of the
18** against the current tree and records the result. Re-verification is a three-question check per
member, following the method the #227 cycles themselves used:

1. Does the member still exist with the same shape (still `virtual`, still `async void`, still
   awaiting `UiSyncContext`)?
2. Is the barrier the ratified rationale cites still present, or has a later change defeated it?
3. Is there now a technique already proven elsewhere in this repository that reaches the member?

**Remove a ratified attribute only where its ratified rationale has demonstrably lapsed**, with the
evidence recorded in a fresh exemption-boundary artifact under `<FEATURE>/evidence/other/`. Two
outcomes are already anticipated by research:

- **`Navigation.cs:173` and `:191` are RETAINED. The stale artifact is the in-code comment, not the
  ratified rationale.** This distinction is load-bearing and an earlier draft of this spec got it
  wrong, so it is recorded explicitly.

  The **in-code comment** at `Navigation.cs:171-172` and `:189-190` reads "Made virtual so tests can
  override the (`TlpCellSnapShot`-bound, out-of-scope) state-taking body". That comment IS stale: the
  `IContainerControlLocal` retrofit means `TlpCellSnapShot.ApplyState` /
  `TlpCellSnapShotList.ApplyState` now accept `IContainerControlLocal`, `IItemViewer` derives from it
  (`QuickFiler/Viewers/IItemViewer.cs:15`), `Navigation.cs:209` / `:219` call
  `ApplyState(_itemViewer)` with **no `(ItemViewer)` cast**, and
  `QfcItemController.NavigationTests.cs:292` / `:345` already exercise `ToggleExpansionOff` /
  `ToggleExpansionOn` against a `Mock<IItemViewer>`.

  But the **ratified rationale is a different claim** and has NOT lapsed. The #227 boundary artifact
  (§3, bucket "Deliberate virtual test seams") reads: "`virtual`, made so tests can override the
  state-taking body (**now de-exempted at the leaf via R2**; the parent `virtual` dispatcher remains a
  deliberate test seam per its own design)." The parenthetical shows the maintainer ratified these two
  attributes *with the R2 retrofit already in view* — R2 was delivered by the very same cycle-5 that
  produced the ratified boundary. A retrofit that the ratification explicitly accounts for cannot be
  the evidence that the ratification lapsed.

  **Required action:** retain both attributes; correct the two stale comments so they state the
  ratified rationale ("deliberate `virtual` override point; the body is intentionally unexercised
  because tests override it") instead of the false barrier claim; and record in the fresh
  exemption-boundary artifact the observation that, post-R2, the "deliberate virtual seam" argument is
  materially weaker for these two members than for `DoLoadConversationResolverCoreAsync`, since the
  dispatch bodies are now individually reachable. **Surface that for maintainer re-review; do not act
  on it.** Overturning a ratified exemption is the maintainer's decision, not F10's.
- **`Conversation.cs:79` (`DoLoadConversationResolverCoreAsync`) re-verified as holding.** Research
  checked all three questions against the current tree: still `protected virtual`
  (`Conversation.cs:80`); exactly two `protected override` declarations exist solution-wide
  (`ConversationTests.cs:37`, `QfcItemControllerTests.cs:46`) and no direct or reflective call to
  the base body exists; the production body remains unexercised by design. No drift. **Retain.**
  (Note that `research/cross-cutting-exemption-and-coverage-analysis.md` §1.2 site 1 still carries a
  superseded `removable-with-seam` classification for this member; the per-file artifact
  `research/file-QfcItemController.Conversation.md` §7 supersedes it. Do not plan a de-exemption
  task from the stale classification.)

The `EventHandlers.cs` bucket-3 shells warrant a specific note because the two research artifacts
reach different conclusions. `research/cross-cutting-exemption-and-coverage-analysis.md` §1.2
classifies the five `async void` shells `removable-as-is` on an in-file-inconsistency argument
(`BtnDelItem_Click` and `BtnFlagTask_Click` have the same shape and are not exempt).
`research/file-QfcItemController.EventHandlers.md` §1 reaches the opposite conclusion on a narrower
reading: the structural difference is `void` versus `async void`, the shell cannot be awaited, and
removing the five attributes adds roughly 35 lines of which only the guard portion is reachable —
lowering measured coverage for no behavioral benefit. **The per-file analysis governs, and the #227
ratification stands: retain the five, and record the re-verified rationale as "`async void` cannot
be awaited deterministically" rather than "the routing is untestable", because the routing is
already proven.**

### 3.6 Expected outcome and the arithmetic

| Step | Attributes | Basis |
| --- | ---: | --- |
| Today, in the F10 file set | **19** | Verified by grep on this branch |
| Less 3 dead members deleted (`Initialization.cs:138, 403, 436`) | **16** | §7 — deletion, not de-exemption; ratified boundary within F10 falls 18 → 15 |
| Less the 1 unratified attribute resolved (`ViewerSetup.cs:132`) | **15** | §3.3 |

**The acceptance bar is 15, and 15 is also the expected outcome.** The two `Navigation.cs`
attributes are RETAINED per §3.5: their ratified rationale explicitly anticipated the R2 retrofit and
has not lapsed. F10 corrects their stale comments and refers the weakened-rationale observation to the
maintainer; it does not remove them.

Any reduction below 15 during execution requires the same evidence standard applied to every other
member — a ratified rationale shown to have demonstrably lapsed, documented per member in the fresh
exemption-boundary artifact — and, where the member is one of the 18 ratified in #227, a maintainer
decision. An executor MUST NOT reduce the count below 15 on its own authority.

### 3.7 The fresh exemption-boundary artifact

F10 produces one artifact at `<FEATURE>/evidence/other/exemption-boundary.<ISO-8601>.md` that:

1. States the #227 ratified boundary as the baseline (18 members within F10's file set).
2. Records, per member, the re-verification outcome: `holds` (with the current-code evidence that
   confirms it), or `lapsed` (with the current-code evidence that defeats it, and the covering
   test).
3. Records the three deletions separately, as removals of dead members rather than de-exemptions.
4. Records the disposition of the one unratified attribute on F10's own authority.
5. States the final count and reconciles it against the 19 → ≤15 arithmetic above.
6. Cites `maintainer-decision.2026-07-02.md` and `exemption-boundary.2026-07-02T17-00.md` by path.

This artifact is the input F1's ledger and the F16 capstone consume for this family.

---

## 4. Corrected coverage baseline

### 4.1 The measured baseline is unreliable in BOTH directions

The committed Cobertura report used for planning
(`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`)
double-counts: the emitted `<class>` `line-rate` / `branch-rate` are computed over the union of the
per-method `<lines>` blocks **and** the class-level `<lines>` block, so every line appearing in both
is counted twice. This is **open issue #441**; it is already filed and must not be re-filed.

The arithmetic was proved exactly, twice, in research:

- `Conversation.cs` — class block 102 entries (12 uncovered) = 88.24%; methods block 34 entries (0
  uncovered); combined `(34+102-12)/(34+102) = 124/136 = 0.911765`, bit-for-bit the emitted
  `line-rate`.
- `Initialization.cs` — class block 134 entries (11 uncovered) = 91.79%; methods block 139 entries
  (16 uncovered); combined `246/273 = 0.901099`, bit-for-bit the emitted `line-rate`.

**The error direction is data-dependent, so no correction factor exists.** `Conversation.cs` is
over-reported (91.18% emitted vs 88.24% true) because covered lines are the ones duplicated;
`Initialization.cs` is under-reported (90.11% emitted vs 91.79% true) because the class-level union
masks uncovered closure entries by taking max hits. #441's title asserts inflation only; the
deflation direction is a refinement worth adding as a comment on that issue.

A second proof is arithmetic on its face: `FocusAndTheme.cs` emits `line-rate=0.756032`, which is
exactly `282/373`, for a file that is **326 lines long**. A 326-line file cannot have 373 coverable
lines.

**A file can FALSELY PASS on the emitted number.** `MailActions.cs` emits `branch-rate="0.75"`,
appearing to sit exactly on the 75% floor, when the true de-duplicated rate is **72.7%** — below it.
This is the concrete reason the corrected method is mandatory rather than merely preferable.

**Method:** every figure must be recomputed from the class-level `<line>` children — counting unique
`<line>` entries and `hits="0"` entries for lines, and summing `condition-coverage="p% (c/t)"`
numerators and denominators for branches. This is what epic.md's "Directives for F1's Ledger and
Harness" already requires (aggregate per file with max hits per line; decide the denominator on
`<line>` child count, never on `line-rate`).

**#441 posture for F10:** commit **both** the harness figure and the class-level-union figure side
by side in `<FEATURE>/evidence/qa-gates/`, with an explicit note. That satisfies the epic's "use
F1's harness" directive while remaining truthful about the instrument's known defect.

### 4.2 Corrected per-file position (indicative)

Captured on another branch (#424's). **Indicative only.** F1's harness run on F10's own branch is
the acceptance authority; these figures are planning inputs and must not be cited as acceptance
evidence.

| File | Line | Branch | Gate status |
| --- | ---: | ---: | --- |
| `QfcItemController.cs` | 100% | 78.6% | passes both |
| `QfcItemController.Initialization.cs` | 91.8% | 96.2% | passes today; projects to ~62% after de-exemption |
| `QfcItemController.ViewerSetup.cs` | 72.5% | 55.6% | **FAILS BOTH** |
| `QfcItemController.EventWiring.cs` | 82.0% | 65.6% | fails branch |
| `QfcItemController.EventHandlers.cs` | 79.6% | 65.0% | **FAILS BOTH** |
| `QfcItemController.Navigation.cs` | 89.1% | 76.7% | passes branch by ONE condition |
| `QfcItemController.FocusAndTheme.cs` | 74.3% | 58.8% | **FAILS BOTH** |
| `QfcItemController.MailActions.cs` | 76.8% | 72.7% | **FAILS BOTH** |
| `QfcItemController.Conversation.cs` | 88.2% | 94.4% | passes both |
| `QfcItemController.FolderHandling.cs` | 87.8% | 63.3% | fails branch |
| `IQfcItemController.cs` | N/A | N/A | interface-only, not measured (§6) |

Two per-file branch figures are disputed between the research artifacts and must be settled by the
first harness run rather than assumed: `Navigation.cs` (76.7% emitted-basis in
`research/file-QfcItemController.Navigation.md` §0 versus 81.8% class-level-union basis in
`research/cross-cutting-exemption-and-coverage-analysis.md` §2.2) and `FocusAndTheme.cs` (58.8%
versus 60.6% on the same split). Both readings leave the conclusion unchanged: `Navigation.cs` has a
thin margin and must be treated as at risk, and `FocusAndTheme.cs` fails.

### 4.3 Branch coverage is the binding gate

**Branch coverage binds on seven of the ten measured partials.** Only `Initialization.cs`,
`Navigation.cs`, and `Conversation.cs` clear 75% branch today, and `Navigation.cs` clears it by a
single condition.

The framing in the brief that opened this child — that the work is four sub-floor files
(`ViewerSetup`, `FocusAndTheme`, `MailActions`, `EventHandlers`) on **line** coverage — is wrong.
`FolderHandling.cs` (87.8% line / 63.3% branch) and `EventWiring.cs` (82.0% line / 65.6% branch)
pass the line gate and fail the branch gate; the epic listed both as compliant. Plans, evidence, and
acceptance must report line and branch independently for every file, and branch is the harder gate
for this family.

---

## 5. The denominator inverts when exemptions come off — atomic removal is mandatory

The attributes are **method-level**, so an exempt method's body sits outside its file's denominator
today. Removing an attribute adds that body at **zero hits** before any new test exists.

| File | Now covered/total | Now % | Δ lines added | After removal, no new tests | After % |
| --- | ---: | ---: | ---: | ---: | ---: |
| `QfcItemController.cs` | 73 / 73 | 100.00 | 0 | 73 / 73 | 100.00 |
| `.Initialization.cs` | 123 / 134 | 91.79 | **+76** | 123 / 210 | **58.57** |
| `.ViewerSetup.cs` | 116 / 160 | 72.50 | **+80** | 116 / 240 | 48.33 |
| `.Conversation.cs` | 90 / 102 | 88.24 | +8 | 90 / 110 | **81.82** |
| `.FolderHandling.cs` | 129 / 147 | 87.76 | 0 | 129 / 147 | 87.76 |
| `.EventWiring.cs` | 247 / 303 | 81.52 | +3 | 247 / 306 | **80.72** |
| `.EventHandlers.cs` | 74 / 93 | 79.57 | **+35** | 74 / 128 | 57.81 |
| `.Navigation.cs` | 107 / 118 | 90.68 | **+24** | 107 / 142 | **75.35** |
| `.FocusAndTheme.cs` | 176 / 237 | 74.26 | 0 | 176 / 237 | 74.26 |
| `.MailActions.cs` | 96 / 125 | 76.80 | 0 | 96 / 125 | 76.80 |
| **Family total** | **1231 / 1492** | **82.51** | **+226** | **1231 / 1718** | **71.65** |

Removing all 19 with no new tests moves the family from **82.51% to 71.65%** and pushes **four
currently-passing files** (`Initialization`, `Conversation`, `EventWiring`, `Navigation`) at or below
the 80% line floor. `Initialization.cs` is the extreme case: 91.8% today, projecting to roughly 62%
once its seven exemptions come off.

**Requirement — atomic removal.** Every attribute removal MUST land in the **same atomic task** as
the tests that cover the newly exposed lines. Sequencing removal separately from coverage drives a
file below the gate mid-flight, which the executor's per-task verification will correctly refuse.
The same rule applies to the two `Navigation.cs` attributes (removal without coverage drops that
file from 89.07% to roughly 77%) and to `EnsureBreadcrumbPipeline`.

**Corollary — plan against the post-removal denominator, never the current one.** A test budget
computed against today's 134-line `Initialization.cs` denominator is wrong the moment an attribute
comes off.

**Related instrumentation fact that changes the arithmetic.** `[ExcludeFromCodeCoverage]` on a
method does **not** propagate to lambdas declared inside it. Proof:
`coverage-final.cobertura.xml:23308-23332` emits four closure methods under `Initialization.cs`
(`<InitializeAsync>b__115_0`, `<InitializeGraphicsAsync>b__116_0/1`,
`<InitializeSequentialAsync>b__117_0`) at `hits="0"` even though every containing method is exempt.
The same holds in `ViewerSetup.cs` (the `WebResourceRequested` lambda at 82-102, the
`ResolveControlGroupsAsync` LINQ lambdas at 276-306) and `Navigation.cs` (197, 202). Those lambda
lines are **already** in the denominator and **already** counted uncovered — removing an attribute
does not add them; it adds only the enclosing method's own statement lines. That is why the Δ column
above is +226 and not +271. It also means that in `ViewerSetup.cs`, 34 of the 44 uncovered lines
(77%) sit inside exempt methods and cost the file its gate whether or not the attributes are
removed. This should be reported to F1 as a ledger/harness note.

---

## 6. `IQfcItemController.cs` is `interface-only / not-measured`

`QuickFiler/Interfaces/IQfcItemController.cs` (107 lines, `QuickFiler.csproj:365`) has **zero
coverable lines**. Full read of all 107 lines establishes: 57 bodiless interface member declarations
(24 methods, 22 properties, 11 overloads), every one terminating in `;`; no default interface
implementation; no `static` or `static abstract` member; no `const`; no field; no nested type; no
attribute; no static constructor; no event with accessor bodies.

Absence from the Cobertura report is proven to mean "no coverable lines" rather than "not
instrumented" by a positive control: the sibling file in the same directory,
`QuickFiler/Interfaces/MailItemActionsAdapter.cs`, appears in the report at line 14448 with
`line-rate="1"`. Instrumentation therefore reached `QuickFiler\Interfaces\`. Corroborating platform
constraint: `QuickFiler.csproj:13` sets `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`,
which cannot support a default interface implementation even if one were added.

Per epic.md's third ledger bucket ("A third ledger bucket: `interface-only / not-measured`"), this
file is:

- reported **N/A, never 0%**, and never counted as a failure;
- **never** given `[ExcludeFromCodeCoverage]`;
- **never** given tests. **Shape-assertion tests — reflection tests asserting that
  `IQfcItemController` declares N members, that a member has a given signature, or that
  `QfcItemController` implements the interface — are PROHIBITED.** They manufacture no coverage for
  this file (they would attribute to the test assembly and to `QfcItemController.*.cs`) and epic.md
  bans them outright.

**No test is to be written for this file, and no atomic plan task may propose one.** The only F10
deliverable touching it is its ledger row. The file is not deleted or trimmed; it is a live
production contract.

---

## 7. Dead-code deletion, and the public-API reduction

Solution-wide call-site grep found **five members with zero call sites**:

| Member | Location | Accessibility | Exempt? |
| --- | --- | --- | ---: |
| `Initialize(IApplicationGlobals, IFilerHomeController, IQfcCollectionController, IItemViewer, int, int, MailItem, TlpCellStates, bool)` | `Initialization.cs:138-163` | `private` | Yes (`:138`) |
| `CreateAsync(...)` | `Initialization.cs:403-431` | **`public static`** | Yes (`:403`) |
| `CreateSequentialAsync(...)` | `Initialization.cs:436-464` | **`public static`** | Yes (`:436`) |
| `GetItemSummary()` | `ViewerSetup.cs:423-424` | `internal` | No |
| `PopulateConversationAsync(ConversationResolver, CancellationToken, bool)` | `Conversation.cs:125-139` | `public` on the class, **absent from `IQfcItemController`** | No |

**Deletion is in scope and is strictly better than exempting or testing them.** Writing a test for
provably unreachable production code manufactures coverage, which the epic prohibits;
`[ExcludeFromCodeCoverage]` on dead code is not an "irreducible remainder" and does not survive the
epic's own ratification standard.

Deletion is behavior-preserving by construction (the code is unreachable), subject to one
precondition carried from the #447 precedent: **confirm there is no reflection-based caller** before
deleting each member.

Deleting the first three also legitimately reduces the ratified boundary within F10's file set from
**18 to 15** without weakening it, at **zero coverage cost** — their lines are outside the
denominator today, so deletion neither adds nor removes denominator lines. It also takes
`Initialization.cs` from 466 to roughly **402** lines, resolving that file's 34-line headroom
against the 500-line limit (§8.3).

### 7.1 Public-API reduction — explicit callout

`CreateAsync` and `CreateSequentialAsync` are declared **`public static`**. Deleting them is a
public-API reduction, which the General Code Change Policy ("Avoid breaking public APIs. If a
breaking change is necessary, call it out clearly") requires be called out explicitly. The mitigating
facts, all verified:

- The declaring type is `internal partial class QfcItemController`, so the `public` modifier confers
  no visibility beyond the `QuickFiler` assembly plus `QuickFiler.Test` (granted by
  `QuickFiler/Properties/AssemblyInfo.cs:5`).
- Neither member is declared on `IQfcItemController`, so no interface contract changes.
- Solution-wide grep finds zero call sites in any project, and zero references in `QuickFiler.Test`.

This deletion carries **its own acceptance criterion** (§14, AC-7) so that review sees it in
isolation rather than folded into the exemption count.

`GetItemSummary()` and the `PopulateConversationAsync(ConversationResolver, …)` overload are
lower-stakes: research offers deletion or coverage for each. The plan must pick one disposition per
member and record the reason; if either is retained, it must be covered by a behavioural test rather
than exempted.

---

## 8. Production changes in scope

Most files in this family need **zero** production change. The existing
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` harness (`HarnessController`,
`SetField`/`GetField`, `InvokeNonPublic`, `BuildSyncDispatcher`, `InjectThemes`, `BuildColorTheme`,
`BuildDispatchableTheme`, `StartRunningDispatcher`/`ShutdownDispatcher`) already provides sufficient
seams. **Do not invent seams that research concluded are unnecessary.** The seam hierarchy is
interface seam > injectable delegate > adapter; STA-constructed never-shown WinForms controls are a
LAST RESORT only, in dedicated `*.StaTests.cs` files.

**Research concluded that no STA file is needed anywhere in F10.** Every per-file artifact reaches
this independently. Note that the two existing headless real-`ItemViewer` tests
(`ViewerSetupTests.cs:379`, `EventWiringTests.cs:229`/`:320`) run in plain
`[TestClass]`/`[TestMethod]` with no STA attribute; that pre-existing convention gap should be
reconciled with F1 rather than propagated silently, but it does not create an STA requirement.

### 8.1 Required F10-local seams

| # | Seam | Location | Why it is required |
| --- | --- | --- | --- |
| S1 | `Func<int, Task> _delayAsync = ms => Task.Delay(ms);` field on `QfcItemController.cs`; call site becomes `await _delayAsync(newDelay)` | replaces `await Task.Delay(newDelay);` at `EventWiring.cs:135` | The polling loop at `EventWiring.cs:121-137` reaches its timeout only after **14 iterations totalling 10,500 ms of real wall-clock time**. `.claude/rules/general-unit-test.md` bans real waits in tests and net481 has no `FakeTimeProvider`. One field plus a 1:1 line replacement converts 11 uncovered lines and the file's only 0/2 branch into deterministic coverage. |
| S2 | `Func<FlagTasks, bool, DialogResult> _flagTasksRunner`, defaulted in `SaveParameters` with `??= (ft, modal) => ft.Run(modal)`; call sites at `MailActions.cs:176` and `:194` | `QfcItemController.cs` field + `Initialization.cs` default + two 1:1 call-site replacements | **Moq cannot intercept non-virtual members.** `TaskVisualization.FlagTasks.Run(bool)` (`FlagTasks.cs:89`) is non-virtual and calls `_viewer.ShowDialog()` at `:95` — a modal dialog, prohibited in unit tests. Its constructor also calls `globals.Ol.App.ActiveExplorer()`. `TaskVisualization` is outside every child's assignment, so the remedy must be F10-local. Without S2, `MailActions.cs` branch coverage stalls near 72.7%, below the gate. |

Both follow the seam pattern already used seven times in `SaveParameters`
(`Initialization.cs:380-397`), are additive and non-breaking, and change no behavior (each default
reproduces the current expression exactly).

**Optional, recommended, not gate-bearing:**

- `Action<string> _showUserMessage` defaulted to `MessageBox.Show`, called at `MailActions.cs:119` —
  unblocks the `MoveMailAsync` catch path (7 lines) which is otherwise permanently uncovered behind
  a popup.
- `Func<SynchronizationContext> _uiSyncContextFactory` plus a single extracted
  `EnsureUiSynchronizationContext()` helper, replacing the guard block duplicated verbatim seven
  times in `EventHandlers.cs` (lines 29-32, 51-54, 63-66, 74-77, 86-89, 100-103, 128-131). This
  removes 21 lines of duplication and makes the guard's true branch testable without installing a
  real `WindowsFormsSynchronizationContext` on the MSTest thread (which would break test
  independence for every subsequent test on that thread).
- `Func<TimerCallback, int, IDisposable> _readTimerFactory` replacing the direct
  `new System.Threading.Timer(...)` + `Change(4000, Infinite)` at `Navigation.cs:223-224`. Covering
  those lines as written arms a **live 4-second wall-clock timer on the thread pool** that outlives
  the test method and fires `ApplyReadEmailFormat` against Moq stubs during an unrelated later test
  — a determinism and independence violation. The field `_emailIsReadTimer` is referenced at exactly
  five places, all F10-owned (`QfcItemController.cs:53`; `Navigation.cs:211, 213, 223, 224`;
  `ViewerSetup.cs:420`), so the blast radius is fully contained.

### 8.2 One new production file — `QfcCidImageResolver`

`ViewerSetup.cs` is the only file in the family where **test-only work cannot reach the line floor**:
covering `ResolveImageMimeType`, `GetItemSummary`, and the null-argument throw plateaus at 78.8%.
The remaining 34 uncovered lines are lambda bodies inside exempt async members. A production change
is therefore unavoidable here, and the correct one is the host-neutral extraction that
`.claude/rules/general-unit-test.md` § Coverage Exclusion Policy prescribes ("extract all logic into
host-neutral, testable modules and leave only the thinnest possible wiring in the host-bound entry
point") and that epic.md's Non-Goals prefer ("prefer host-neutral extraction that a future
WebView2/Office.js port can reuse").

**New file: `QuickFiler/Controllers/QfcCidImageResolver.cs`** — a pure static class plus a small
return DTO:

- `internal static QfcCidImage Resolve(string requestUri, IAttachment[] attachments)` — null when
  unresolvable.
- `internal static string ResolveMimeType(string fileExtension)` — moved out of
  `ViewerSetup.cs:194-202`.
- `internal sealed class QfcCidImage` (or a plain `readonly struct`) carrying `Data` and `MimeType`.

Constraints:

- **No `record`, no `record struct`, no `init`-only setter.** `QuickFiler.csproj:13-14` targets
  `v4.8.1`; those constructs require an `IsExternalInit` shim whose reachability from `QuickFiler`
  is unverified. Use a plain sealed class or `readonly struct` with an ordinary constructor.
- `UtilitiesCS.CidImageResolver.BuildContentIdMap` stays where it is and is **called**, not moved.
  No `UtilitiesCS` edit.
- The extraction is behavior-preserving: same URI parsed, same map built at request time (preserving
  the pooled-viewer semantics documented at `ViewerSetup.cs:71-75`), same MIME defaults.

Obligations that must land in the **same change** that creates the file:

1. `<Compile Include="Controllers\QfcCidImageResolver.cs" />` in `QuickFiler/QuickFiler.csproj`,
   adjacent to the existing `Controllers\QfcItemController*` entries at lines 328-337.
2. An F1 ledger row for the new file, bucket `testable`, target **>= 90% line** (epic.md "Mid-Wave
   File Creation" rule 4).
3. CRLF preservation on the csproj edit (§8.4).

The extraction also takes `ViewerSetup.cs` from 426 to roughly **399** lines.

### 8.3 500-line limit

No production file in scope needs a split, **provided the dead members are deleted**.

| File | Now | After planned work | Note |
| --- | ---: | ---: | --- |
| `QfcItemController.Initialization.cs` | 466 | ~402 | Only 34 lines of headroom today. Deletion of the three dead members is what makes the seam defaults in §8.1 safe to add. **Verify the line count immediately before any edit** — if a sibling task in this same child has already grown it, the seam defaults must move rather than push it past 500. |
| `QfcItemController.ViewerSetup.cs` | 426 | ~399 | Extraction is net −27 |
| `QfcItemController.EventHandlers.cs` | 219 | ~205 | If the optional sync-context helper lands |
| `QfcItemController.cs` | 323 | ~331 | Seam fields, +8 at most |
| All others | ≤ 391 | unchanged | |

If deletion is declined, the pre-planned fallback for `Initialization.cs` is a new partial
`QfcItemController.InitializationSeams.cs` carrying the seam-default block at lines 377-397, with
the same csproj-entry and ledger-row obligations. **This split is not recommended**; deletion is
cheaper and more honest.

### 8.4 Both csproj files are shared, CRLF-terminated, and non-globbing

epic.md "Cross-Child Constraints" §1 names only `QuickFiler/QuickFiler.csproj`. **That is
incomplete.** `QuickFiler.Test/QuickFiler.Test.csproj` is also a legacy non-SDK project with no
globbing — it carries 107 explicit `<Compile Include=...>` entries, of which 17 are the
`QfcItemController*` test files (lines 90 and 132-147, verified on this branch). **Every child that
adds a test file must edit it.** Because most children add test files and only some add production
files, it is a **higher-conflict shared surface than the production csproj**.

Rules for both files:

- Edit **only** to add `<Compile Include=...>` entries for files this child owns. No property
  changes, no reference changes, no reordering of unrelated entries.
- Keep each edit to one minimal adjacent hunk so concurrent children collide on as few lines as
  possible.
- **Preserve CRLF.** Use the **Edit tool** or `perl -0777` with explicit `\r\n`. **Never** a
  git-bash `sed -i`, which strips CRLF and produces a whole-file diff guaranteed to conflict at
  fan-in.
- Expect additive conflicts at fan-in; the correct resolution is to keep both sides.

This omission is recorded as documented deviation **D1** (§11) and must be propagated as a note to
F1 and to the epic manifest.

---

## 9. Test-suite obligations

### 9.1 Test-file size — two files are effectively full

| Test file | Lines | Headroom | Consequence |
| --- | ---: | ---: | --- |
| `QfcItemController.FolderHandlingTests.cs` | 498 | **2** | **New test file MANDATORY** for any `FolderHandling.cs` test |
| `QfcItemController.FocusAndThemeTests.cs` | 497 | **3** | **New test file MANDATORY** for any `FocusAndTheme.cs` test |
| `QfcItemController.EventHandlersTests.cs` | 438 | 62 | Split planned up front; 12 new tests will breach |
| `QfcItemController.ViewerSetupTests.cs` | 407 | 93 | Near the limit after Group A |
| `QfcItemController.NavigationTests.cs` | 391 | 109 | 14 new tests will breach; split planned |
| `QfcItemController.EventWiringTests.cs` | 374 | 126 | Split planned |
| `QfcItemController.TestSupport.cs` | 365 | 135 | Absorbs promoted shared helpers |
| `QfcItemController.ConversationTests.cs` | 352 | 148 | New file recommended rather than filling it |

The 500-line limit applies to test files. Every new test file needs its own explicit
`<Compile Include="Controllers\....cs" />` entry in `QuickFiler.Test/QuickFiler.Test.csproj` under
the CRLF rule in §8.4. Precedent: open issue **#450**
(`quickfiler-formcontroller-tests-file-size-split`) — already filed, do not re-file.

### 9.2 Shared-fixture consolidation

The headless real-`ItemViewer` construction pattern is currently implemented **three times**: as the
`private sealed class ViewerScope` at `QfcItemControllerBreadcrumbDropDownTests.cs:365-383`, and
inline in `EventWiringTests` and `ViewerSetupTests`. Promote a `HeadlessViewerScope` and a
complementary `NullSynchronizationContextScope` into the shared
`QfcItemController.TestSupport.cs` rather than adding a fourth copy. Likewise promote
`BuildAllThemes`, `BuildFocusController`, `BuildExecutingViewer`, `EnableHandlelessThemeInvoke`
(currently file-local in `FocusAndThemeTests.cs`) and a shared `FakeFolderSearchHandler`, so new
test files can consume them. All of these are F10-owned test files; no sibling coordination is
needed.

### 9.3 Determinism — the existing suite has zero violations; F10 must not introduce the first

Research audited all 17 existing test files for `DateTime.Now`/`UtcNow`, `Random`, `Thread.Sleep`,
`Task.Delay`, real wall-clock waits, temp files, `MessageBox`, `ShowDialog`, live form construction,
xUnit/NUnit, and files over 500 lines. **Result: zero hard violations.**

Binding rules for every test F10 adds:

- **Injected clock and fake timers only.** `Thread.Sleep`, `Task.Delay`, and real wall-clock waits
  are PROHIBITED in tests. Where production forces a wait, seam it (S1, and the optional
  `_readTimerFactory`) — do not wait.
- No temporary files, no filesystem writes, no external services, no live forms, no popups.
- MSTest `[TestClass]`/`[TestMethod]`, Moq for doubles, FluentAssertions for new assertions,
  Arrange–Act–Assert, descriptive names.
- Any test that mutates ambient `SynchronizationContext` must capture and restore it in a `finally`
  (pattern at `EventWiringTests.cs:305-308`).
- No test may arm the real `_emailIsReadTimer`, start a real WebView2 core, or reach
  `FlagTasks.Run` / `MessageBox.Show`.
- Culture sensitivity: `ViewerSetup.cs:424` formats dates with no `CultureInfo`. A test asserting a
  literal date string could pass locally and fail on a differently-configured runner. Assert using
  the same culture-dependent format calls or on stable substrings. Do **not** mutate
  `Thread.CurrentThread.CurrentCulture`.

Three pre-existing test-policy **risks** (not violations) are in scope for this child's own
execution, per epic.md's precedent that test-policy items in existing tests are not deferred:

1. `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` (`TestSupport.cs:238-249`) writes the
   static `UiThread._dispatcher` and never restores it; `GetDedicatedDispatcher()` parks a
   process-lifetime background STA thread in a static field. Guarded and idempotent, but it makes
   assembly-wide state order-dependent. Wrap in a save/restore scope or document the deviation
   explicitly in F10's policy audit.
2. `EnsureSynchronizationContext()` (`TestSupport.cs:87-93`) mutates ambient thread state without
   restoring it. Convert to a disposable scope and add the complementary null-context scope.
3. `QfcItemController.FolderHandlingTests.cs:120-148` — `ReadControllerSource` calls
   `File.ReadAllText` on a path derived from `AppDomain.CurrentDomain.BaseDirectory`, and
   `LoadFolderHandler_ProbabilityDebugLog_…` asserts on the production file's **source text**. This
   is a filesystem dependency in a unit test and asserts on source rather than behavior. Directly
   parallel to the `MailItemInfoTests.cs` finding epic.md ruled in scope for F4's own execution.

### 9.4 `InternalsVisibleTo` — F10 does not hit the wall

`UtilitiesCS/Properties/AssemblyInfo.cs:18-20` grants internals to `DynamicProxyGenAssembly2`,
`UtilitiesCS.Test`, and `ToDoModel.Test` — **not** to `QuickFiler.Test`. Every `UtilitiesCS` surface
the F10 file set depends on was enumerated and is **public** (`IApplicationGlobals`,
`MailItemHelper`, `FolderPredictor`, `IFolderSearchHandler`, `EmailFiler`, `Theme`, `IUiDispatcher`,
`UiThread`, `CidImageResolver`, `IAttachment`, `OutlookFolderHierarchyProvider`,
`IContainerControlLocal`). Where tests reach non-public state they do so by reflection over public
types, which `InternalsVisibleTo` does not govern.

`QuickFiler/Properties/AssemblyInfo.cs:5` **does** grant `InternalsVisibleTo("QuickFiler.Test")`, so
`internal partial class QfcItemController` and all its internal members are directly reachable.

**No grant change is needed and none is to be proposed.** F10 follows F3's precedent (build a local
seam; do not widen the internals grant) but does not need to reach into `UtilitiesCS` at all.

---

## 10. Cross-child contract notes

Recorded as notes; **F10 makes no sibling edit**. Research established that F10 reaches both gates on
all ten measured files against the **current** shapes of every one of these, with **no upstream
change required**.

### 10.1 To F4 (#434) — `ConversationResolver`

F10 binds the **concrete** `ConversationResolver` type at three positional sites:

| Site | Call | Binds |
| --- | --- | --- |
| `Conversation.cs:34` | `_conversationResolverFactory(Mail)` | `Func<MailItem, ConversationResolver>` field at `QfcItemController.cs:69` |
| `Initialization.cs:382-388` | `new ConversationResolver(_globals, mail, _tokenSource, Token, SetTopicThread)` | `ConversationResolver.cs:70-76` (5 positional) |
| `Conversation.cs:85-92` | `ConversationResolver.LoadAsync(_globals, ItemHelper, tokenSource, token, loadAll, SetTopicThread)` | the `MailItemHelper` overload at `ConversationResolver.cs:126-133` (6 positional; overload selection depends on `ItemHelper` being a `MailItemHelper`) |

Tests additionally use the inert two-argument constructor `ConversationResolver(IApplicationGlobals,
MailItem)` at `ConversationResolver.cs:64`, the `internal set` on `Count`
(`ConversationResolver.Loading.cs:265-271`), and the public setter on `ConversationInfo`
(`ConversationResolver.Loading.cs:20-35`).

**F4 may APPEND parameters with defaults. F4 must not reorder, retype, or remove existing positional
parameters; must not tighten `Count`'s setter below `internal`; must not make `ConversationInfo`'s
setter non-public; and must not add a `LoadAsync` overload that makes the `:126` binding ambiguous.**

**Retyping `ConversationResolver` to `IConversationResolver` is a THREE-CHILD breaking change**, not
a two-child one: F4 owns the type, F10 passes it (`QfcItemController.cs:110-114`;
`IQfcItemController.cs:69` declares `void PopulateConversation(ConversationResolver resolver)`), and
F11 receives it (`IQfcCollectionController.ToggleUnGroupConv`, called from `MailActions.cs:41-46`).
F10 does not reference `IConversationResolver` anywhere.

### 10.2 To F4 (#434) — `QfcThemeHelper` and `TlpCellSnapShot`

- `QfcThemeHelper.SetupThemes(IQfcItemController, ItemViewer, Action<Enums.ToggleState>, IUiDispatcher)`
  is called from `Initialization.cs:175, 209, 266, 299`. Its four-argument shape must survive. An
  **additive, non-breaking** `IItemViewer`-accepting overload would let F10 remove exemption site
  `Initialization.cs:168` — **F10 must not add it itself**; the file is F4's.
- `TlpCellSnapShotList.ApplyState(IContainerControlLocal)` (`TlpCellSnapShot.cs:72`, `:192`) is
  consumed at `Navigation.cs:209, 219`. **Reverting that signature to a concrete `Control` would
  re-block the two `Navigation.cs` de-exemptions** described in §3.5.

### 10.3 To F14 (#456) — `ItemViewer` / `IItemViewer`

F10 depends on (a) `IItemViewer` continuing to derive from `IContainerControlLocal`
(`IItemViewer.cs:15`); (b) `new QuickFiler.ItemViewer()` remaining constructible headlessly under a
plain `SynchronizationContext` — already relied on by six passing tests; (c) the concrete members
reached by cast keeping their names and signatures (`L0v2h2_WebView2`, `L0vhBreadcrumb_WebView2`,
`TopicThread`, `LblItemNumber`, `GetAllChildren()`, `ForAllControls(...)`, `BreadcrumbCoordinator`,
`InitializeBreadcrumbPipeline`, `BreadcrumbUnhandledArrow`, `ResetBreadcrumb`,
`ConfigureBreadcrumbDropDown`, `SetBreadcrumbTheme`, `AttachBreadcrumbWebViewAsync`); and (d) the
private field name `_context` (`ItemViewer.cs:59`) if the optional `ResolveControlGroupsAsync`
de-exemption is attempted.

Note that `IItemViewer.UiDispatcher` is a concrete, sealed `System.Windows.Threading.Dispatcher`
(`IItemViewer.cs:36`), not the injectable `IUiDispatcher`. That is why
`QfcItemControllerTestSupport.StartRunningDispatcher()` exists. **F10 keeps using that helper and
does not propose a re-type** — re-typing `IItemViewer` is F14's decision.

### 10.4 To F3 (#430) — keyboard actions

`IQfcKeyboardHandler` must keep the four `KbdActions<...>` properties with exactly their current
three type arguments, and `KbdActions<TKey, UClass, VDelegate>` must keep `Add(string, TKey,
VDelegate)`, `Remove(string, TKey)`, `ContainsKey(TKey)`, and the indexer
`public VDelegate this[TKey key]` (`KbdActions.cs:36-47`). **The indexer is load-bearing**: it is
the only way a test retrieves a registered lambda and invokes it, which is how the 32
registered-lambda lines in `EventWiring.cs` become covered.

Conditional note: if F3's fix for **#444** makes `KbdActions.Add` idempotent, F10's re-entrancy
tests change from "throws `ArgumentException`" to "no-op" and must be updated.

### 10.5 To F11 (#454)

`IQfcCollectionController` is consumed only through mocks. The cross-variant expansion-registry
defect (promoted as **#482**) most likely belongs at `QfcCollectionController.cs:1439` — an `async`
method calling the synchronous `ToggleExpansion()` — not in an F10 file.

### 10.6 `IQfcDatamodel` — correction

The brief listed `IQfcDatamodel` (F5) as an F10 sibling dependency. **It is a false positive.**
Verified: zero occurrences of `IQfcDatamodel`, `QfcDatamodel`, or `EfcDataModel` in any of the ten
`QfcItemController*.cs` files or in `Interfaces/IQfcItemController.cs`. The datamodel is consumed by
`QfcHomeController` (F7) and `QfcCollectionController` (F11). **No contract, no risk; the plan must
not carry a phantom dependency.**

---

## 11. Documented deviations from the brief and from epic.md

Each deviation below is a correction established against this branch's source. Each must be
propagated to F1 and to the epic manifest so a later child does not repeat the error.

| ID | Claim | Source | Correction | Evidence |
| --- | --- | --- | --- | --- |
| **D1** | `QuickFiler/QuickFiler.csproj` is the only unavoidable shared file | epic.md "Cross-Child Constraints" §1 | **Incomplete.** `QuickFiler.Test/QuickFiler.Test.csproj` is also legacy non-SDK with no globbing, 107 explicit entries; every child adding a test file must edit it, making it a **higher-conflict** surface than the production csproj | `QuickFiler.Test.csproj:90, 132-147` (17 `QfcItemController*` entries verified on this branch) |
| **D2** | Known conflict risks are #400 and #424, "active on `main` concurrently" | epic.md "Known Conflict Risks" | **Stale.** Both are **Closed**; their feature folders merely remain under `docs/features/active/`. The live risks for F10 are **#230, #427, #438, #440, #441** — none of which epic.md names. #230 predates the epic and is the single largest determinant of F10's achievable exemption count | `research/open-issues-and-sibling-boundaries.md` §2.1, §2.2 |
| **D3** | `IQfcDatamodel` is an F10 sibling dependency | delegation brief | **False positive.** Zero references in the F10 file set | §10.6 |
| **D4** | "six of ten partials are currently exempted"; `[X]` marks on six F10 files | epic.md:395-399 | **Wrong at file level.** All 19 attributes are **member-level**; none sits on a `partial class` declaration; **all ten partials are instrumented** and all ten appear in the Cobertura report as `<class>` elements. The `[X]` markers mean "contains at least one exempted member". F1's ledger must record disposition **per member** for this family | grep on this branch returns 19 hits, all at 8-space indent immediately preceding a method; report lines 22740, 23126, 23519, 24004, 24222, 24601, 25411, 25754, 26058, 26662 |
| **D5** | The work is four sub-floor files on **line** coverage | delegation brief; epic.md:174-178 | **Wrong framing.** Branch coverage is the binding gate on **seven of ten** files, including `FolderHandling.cs` and `EventWiring.cs`, which the epic listed as compliant | §4.2, §4.3 |
| **D6** | The epic's per-file percentages (ViewerSetup 74.4%, FocusAndTheme 75.6%, MailActions 77.8%, EventHandlers 79.7%) | epic.md:174-178 | **Right files, wrong numbers.** Those are emitted `<class line-rate>` values, which double-count. True per-file union figures are 72.5%, 74.3%, 76.8%, 79.6%. Also: the epic's "373 lines" for the 326-line `FocusAndTheme.cs` and "189 lines" for the 224-line `MailActions.cs` are double counts, not line counts | §4.1 |
| **D7** | The Cobertura report's rates can be trusted for gate decisions | implicit in epic.md's baseline table | **No.** Open issue **#441**. The defect **inflates AND deflates** (data-dependent), so no correction factor exists, and `MailActions.cs` **falsely passes** the branch gate on the emitted figure (0.75 emitted vs 72.7% true). #441's title asserts inflation only; the deflation refinement should be added as a comment on that issue | §4.1 |
| **D8** | Every exempted member is live production code | implicit in the brief and epic.md | **Three of the 19 sites are on dead members** with zero call sites solution-wide. Correct disposition is deletion, not testing | §7 |
| **D9** | `[ExcludeFromCodeCoverage]` removes the member from measurement | implicit | **It does not propagate to lambdas declared inside the exempt method.** Every exempt method containing a lambda silently contributes permanently-uncovered lines to its file's denominator — epic-wide, not F10-specific. Report to F1 as a ledger/harness note | §5, closing paragraph |
| **D10** | Coverage evidence may go under `artifacts/` | any caller instruction | **Non-overridable.** All evidence goes to `<FEATURE>/evidence/<kind>/` per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Coverage evidence: `<FEATURE>/evidence/qa-gates/`. Exemption boundary: `<FEATURE>/evidence/other/` | Policy |

---

## 12. Out of scope but tracked

### 12.1 Latent defects already promoted by the orchestrator — do NOT re-file, do NOT fix

The epic's no-behavior-change NFR prohibits fixing these under F10.

| Issue | Subject |
| --- | --- |
| **#480** | `ToggleNavigation` toggles `_itemPositionTips.Toggle(false)` unconditionally at `FocusAndTheme.cs:170` and again in both branches, so the navigation tips return to their original state — the feature is inert |
| **#481** | No event unwiring path exists: all 22 subscriptions made by `WireIntentEvents`/`WireControlTreeEvents` survive `Cleanup()`, which unsubscribes only `BreadcrumbUnhandledArrow` |
| **#482** | Sync and async expansion variants maintain disjoint keyboard registries while `_expanded` is a single shared flag; production mixes them (`QfcCollectionController.cs:1439`), and the next same-variant expand throws `ArgumentException` |
| **#483** | `MailActions` error handling: `MoveMailAsync`'s catch swallows every exception without rethrowing, and raises `MessageBox.Show` from a potentially non-UI thread |
| **#484** | Cleanup timer and stale fields: `Cleanup()` nulls `_emailIsReadTimer` without disposing it (`ViewerSetup.cs:420`), and `SaveParameters`' `??=` leaves `_mailActions` bound to the previous `MailItem` |
| **#485** | WebView2 handler unguarded inputs: `new Uri(e.Request.Uri)` and `new MemoryStream(match.AttachmentData)` are unguarded inside a `WebResourceRequested` handler with no `try`/`catch` in the chain |

Where an existing test pins a defective behavior, F10 **characterises** it (documents the current
behavior in an executable test) rather than changing it. `#480`'s existing test at
`FocusAndThemeTests.cs:310` uses `Times.AtLeastOnce()`, which masks the double toggle; a
characterisation test should use an exact count so the defect is visible without being fixed.

### 12.2 Already filed by siblings or upstream — do NOT re-file

**#441** (Cobertura double-count), **#457**, **#463**, **#444** (`KbdActions` enumerable ctor
bypasses the duplicate guard), **#450** (formcontroller test-file split precedent), **#230**
(WinForms message-pump seam), **#427**, **#438**, **#440**.

Two of those constrain how F10 writes tests, so that a later fix stays possible:

- **#438** (search keystroke focus steal): do **not** change `TextBoxSearch_TextChanged`'s behavior,
  and do **not** add further tests that pin `SetFolderDroppedDown(true)` at
  `EventHandlers.cs:177`.
- **#440** (breadcrumb arrow navigation): cover `OnBreadcrumbUnhandledArrow`'s existing routing
  exactly as `QfcItemControllerBreadcrumbDropDownTests.cs:156` already does, and add nothing that
  constrains `BreadcrumbArrowFallThrough`'s semantics.
- **#427** (post-show duplicate scoring): note in the plan that `LoadFolderHandlerAsync`'s branch
  structure may change under a #427 fix; do not pre-empt or partially implement it.

### 12.3 New latent defects found during F10 research

Research surfaced further defects not covered by #480-#485 (for example: the en-dash in
`CoreWebView2EnvironmentOptions("–incognito ")` at `ViewerSetup.cs:52` silently disabling the
Chromium switch; `throw (initException)` at `EventWiring.cs:117` capable of throwing `null`; the
`ToggleSaveAttachments` `'A'` keyboard action bound to an entirely commented-out body; the
`ToggleConversationCheckbox` switch not being flag-aware while the enum is used as flags elsewhere
in the class; `ApplyReadEmailFormat` writing the unread state and saving twice; and
`PopulateFolderComboBoxAsync` double-wrapping `Task.Run`). Any such defect that is not already
covered by #480-#485 must be promoted to a GitHub issue via the MCP promotion lifecycle **before
F10 completes** — prose in a feature folder is lost when the folder moves to `completed/`.

---

## 13. Risks

| # | Risk | Impact | Mitigation |
| --- | --- | --- | --- |
| R1 | An executor treats a **ratified** attribute as a Blocking finding and removes it, or a reviewer flags a correctly-retained attribute as a policy violation | Contradicts a maintainer decision; wasted remediation cycles | §3 is the authoritative reconciliation and is carried into AC-2/AC-3; the fresh exemption-boundary artifact makes each retention auditable |
| R2 | An attribute is removed in one task and its tests land in another | The file sits below gate between tasks; the executor's per-task verification fails | §5 atomic-removal requirement, restated as AC-4 |
| R3 | Acceptance evidence is computed from the defective emitted `line-rate`/`branch-rate` | `MailActions.cs` falsely passes the branch gate | §4.1 method + the dual-figure #441 posture, restated as AC-5 |
| R4 | Scope creep into the #230 message-pump seam | Materially larger than the child's budget; outside its file assignment | §3.4; AC-3 records the retention rather than the removal |
| R5 | Fan-in conflict on `QuickFiler.Test.csproj` (higher traffic than the production csproj) | Merge conflicts across concurrent wave-1 children | §8.4 — minimal adjacent hunks, CRLF preserved, keep-both-sides resolution; D1 propagated to F1/epic |
| R6 | `Initialization.cs` at 466/500 is pushed past the limit by a seam default before the dead members are deleted | 500-line policy breach mid-flight | Sequence the three deletions first; verify the line count immediately before any edit to that file (§8.3) |
| R7 | A sibling (F4/F14) changes a consumed contract mid-wave | F10 breaks at compile time or at fan-in | §10 contract notes; F10 pins each shape with a direct `new`/call so a change surfaces as a build break rather than a silent behavior change |
| R8 | An invented seam where research concluded none is needed | Unnecessary production change under a no-behavior-change NFR; gratuitous public surface | §8 preserves the "most files need zero production change" conclusion; only S1, S2, and `QfcCidImageResolver` are required |
| R9 | A test manufactures coverage (shape-assertion tests for `IQfcItemController.cs`, or tests for dead members) | Prohibited by epic.md; misleading metrics | §6 and §7 prohibitions, restated as AC-6 and AC-7 |
| R10 | A new test introduces the first determinism violation in a clean suite (real timer armed, 10.5 s delay awaited, modal shown) | Flaky or hanging suite; policy breach | §9.3; seams S1/S2 exist precisely to make these paths reachable without waiting |
| R11 | The optional `ResolveControlGroupsAsync` de-exemption is attempted via test-side reflection into `ItemViewer._context` and destabilises the suite | Cross-child coupling to an F14-private field; flaky marshalling | Treat as optional stretch only; not gate-bearing; not required by any acceptance criterion |

---

## 14. Acceptance Criteria

Authoritative for this child together with `user-story.md`. Check off per
`.claude/skills/acceptance-criteria-tracking/SKILL.md` only after the work is implemented **and**
verified.

### Coverage

- [ ] **AC-1.** Every measured production file in scope (the ten `QfcItemController*.cs` partials)
      reaches **>= 80% line** and **>= 75% branch** coverage, measured with F1's per-file harness on
      this child's branch, with the per-file result committed to
      `<FEATURE>/evidence/qa-gates/`. Line and branch are reported **independently for every file**;
      a line figure at or above 80% is never accepted as evidence for the branch gate.

- [ ] **AC-5.** Coverage evidence is recomputed from the class-level `<line>` children (unique
      `<line>` entries and `hits="0"` for lines; summed `condition-coverage` numerators and
      denominators for branches), **not** from the emitted `line-rate` / `branch-rate` attributes.
      Both the harness figure and the class-level-union figure are committed side by side with an
      explicit note citing open issue **#441**. No new issue is filed for #441.

- [ ] **AC-9.** Repository-wide line coverage is measured before and after and is **retained or
      improved** against the measured baseline (epic.md "Coverage-Target Reconciliation" — retained,
      not met against an absolute floor). Evidence committed to `<FEATURE>/evidence/qa-gates/`.

### Exemption governance

- [ ] **AC-2.** All **18** `[ExcludeFromCodeCoverage]` attributes in the F10 file set that are
      covered by the #227 maintainer ratification are **re-verified against current source**, and
      the outcome per member (`holds` with confirming evidence, or `lapsed` with the current-code
      evidence that defeats the ratified rationale plus the covering test) is recorded in a fresh
      exemption-boundary artifact under `<FEATURE>/evidence/other/`. The artifact cites
      `maintainer-decision.2026-07-02.md` and `exemption-boundary.2026-07-02T17-00.md` by path and
      names the #227 ratification as the governing authority for every retained attribute.

- [ ] **AC-3.** The unratified attribute at
      `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:132` (`EnsureBreadcrumbPipeline`) is
      either removed with `EnsureBreadcrumbPipeline` covered (including the `Mock<IItemViewer>`
      early-return branch at `:135-138`, the coordinator-creation path, the subscribe/unsubscribe
      swap, and the post-`Cleanup()` no-op), **or** retained with a member-specific,
      evidence-backed rationale asserted on F10's own authority and recorded in the same artifact —
      never by appeal to #227, which does not cover it.

- [ ] **AC-4.** Every `[ExcludeFromCodeCoverage]` removal lands in the **same atomic task** as the
      tests that cover the newly exposed lines. No task in the executed plan removes an attribute
      without its covering tests, and no per-file coverage measurement taken between tasks shows a
      file below either floor as a result of a de-exemption.

- [ ] **AC-8.** The count of `[ExcludeFromCodeCoverage]` attributes in the F10 file set is reduced
      from **19 to 15**, with the arithmetic reconciled in the exemption-boundary artifact
      (19 − 3 dead members deleted − 1 unratified resolved = 15). **No task attempts to build the
      issue-#230 WinForms message-pump test seam**, and #230 is cited by number as the
      externally-tracked justification for every bucket-1 attribute retained. An executor MUST NOT
      reduce the count below 15 on its own authority: each of the remaining 15 is ratified under #227,
      and overturning a ratified exemption requires a maintainer decision, not an executor's judgement
      that a seam is now conceivable.

- [ ] **AC-21.** The two stale in-code justification comments at
      `QuickFiler/Controllers/QfcItemController.Navigation.cs:171-172` and `:189-190` are corrected to
      state the ratified rationale ("deliberate `virtual` override point; the body is intentionally
      unexercised because tests override it") rather than the false `TlpCellSnapShot`-bound barrier
      claim, **and both attributes are retained**. The exemption-boundary artifact records the
      observation that, post-R2, the deliberate-virtual-seam argument is materially weaker for these
      two members than for `DoLoadConversationResolverCoreAsync`, and refers that observation to the
      maintainer for re-review without acting on it.

### Dead code and public API

- [ ] **AC-7.** The three dead exempt members — `Initialize(9-arg private)` at
      `Initialization.cs:138-163`, `CreateAsync` at `:403-431`, and `CreateSequentialAsync` at
      `:436-464` — are **deleted** after confirming no reflection-based caller exists. The deletion
      of `CreateAsync` and `CreateSequentialAsync` is called out explicitly in the change
      description as a **`public static` API reduction**, with the mitigating facts stated (the
      declaring type is `internal`; neither member is on `IQfcItemController`; zero call sites
      solution-wide). **No test is written for any deleted member.** The dispositions of
      `GetItemSummary()` (`ViewerSetup.cs:423`) and the
      `PopulateConversationAsync(ConversationResolver, CancellationToken, bool)` overload
      (`Conversation.cs:125-139`) are each decided and recorded — deleted, or retained and covered by
      a behavioural test; neither is exempted.

### Interface-only file

- [ ] **AC-6.** `QuickFiler/Interfaces/IQfcItemController.cs` is classified
      `interface-only / not-measured` in the F1 ledger, reported **N/A and never 0%**, receives **no**
      `[ExcludeFromCodeCoverage]`, and has **no tests written for it**. No shape-assertion or
      reflection-over-interface test appears anywhere in the delivered change set. The ledger row
      records the positive-control evidence
      (`QuickFiler/Interfaces/MailItemActionsAdapter.cs` present at `line-rate="1"`).

### Production change discipline

- [ ] **AC-10.** Production change is limited to: the three deletions (AC-7); the two required
      F10-local seams (the `Func<int, Task>` delay delegate replacing `Task.Delay` at
      `EventWiring.cs:135`, and the `Func<FlagTasks, bool, DialogResult>` runner replacing the
      non-virtual `FlagTasks.Run` calls at `MailActions.cs:176`/`:194`); the attribute removals
      permitted by AC-2/AC-3; the `QfcCidImageResolver` extraction (AC-11); and any optional seam
      explicitly justified in the plan. **No file listed in §2.2 is edited**, and no member is added
      to `IQfcItemController` or `IItemViewer`. Every required upstream change is recorded as a
      cross-child contract note in this spec, not as an edit.

- [ ] **AC-11.** The new production file `QuickFiler/Controllers/QfcCidImageResolver.cs` (pure
      static resolver plus DTO; **no `record`, no `record struct`, no `init`-only setter** under
      `TargetFrameworkVersion v4.8.1`) lands together with, in the **same change**: its
      `<Compile Include="Controllers\QfcCidImageResolver.cs" />` entry in
      `QuickFiler/QuickFiler.csproj`; an F1 ledger row classified `testable` at **>= 90% line**; and
      measured coverage of **>= 90% line** for the file itself. `UtilitiesCS.CidImageResolver` is
      called, not moved, and no `UtilitiesCS` file is edited.

- [ ] **AC-12.** No production file in scope exceeds **500 lines** after the change.
      `Initialization.cs` is verified at or below 500 immediately before and after each edit to it,
      and `ViewerSetup.cs` is verified after the extraction.

### Test suite

- [ ] **AC-13.** New tests for `QfcItemController.FolderHandling.cs` and
      `QfcItemController.FocusAndTheme.cs` go into **new test files** (their existing fixtures are
      at 498/500 and 497/500 lines). Every new test file has an explicit
      `<Compile Include="Controllers\....cs" />` entry in `QuickFiler.Test/QuickFiler.Test.csproj`,
      **CRLF preserved** (Edit tool or `perl -0777` with explicit `\r\n`; never git-bash `sed -i`),
      in a single minimal adjacent hunk. No test file in the delivered change set exceeds 500 lines.

- [ ] **AC-14.** All new and modified tests use **MSTest**, **Moq**, and **FluentAssertions**, follow
      Arrange–Act–Assert, and are independent, isolated, fast, and deterministic. **No
      `Thread.Sleep`, no `Task.Delay`, no real wall-clock wait, no temporary file, no external
      service, no live form, and no popup** appears in any test. No test arms the real
      `_emailIsReadTimer`, starts a real WebView2 core, reaches `FlagTasks.Run` or
      `MessageBox.Show`, or constructs a real `WindowsFormsSynchronizationContext` on the MSTest
      thread. Every test that mutates ambient `SynchronizationContext` restores it in a `finally`.

- [ ] **AC-15.** The three pre-existing test-policy risks are resolved or an explicit, reasoned
      deviation is recorded in the policy audit: the unrestored static `UiThread._dispatcher` write
      in `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` (`TestSupport.cs:238-249`); the
      unrestored ambient-context mutation in `EnsureSynchronizationContext()` (`TestSupport.cs:87-93`);
      and the filesystem-reading, source-text-asserting test at
      `QfcItemController.FolderHandlingTests.cs:120-148`. The triplicated headless-`ItemViewer`
      fixture is consolidated into `QfcItemController.TestSupport.cs` rather than copied a fourth
      time.

### Behavior, defects, and toolchain

- [ ] **AC-16.** **No observable behavior change** to QuickFiler flows. Defects encountered are
      **characterised** by tests that document current behavior, never fixed. None of #480, #481,
      #482, #483, #484, #485 is fixed under this child, and none of #441, #457, #463, #444, #450,
      #230, #427, #438, #440 is re-filed. `TextBoxSearch_TextChanged` behavior is unchanged and no
      new test pins `SetFolderDroppedDown(true)`; no new test constrains
      `BreadcrumbArrowFallThrough` semantics beyond the routing already asserted.

- [ ] **AC-17.** Every latent defect found during execution that is not already covered by
      #480-#485 is promoted to a GitHub issue via the MCP promotion lifecycle **before this child
      completes**. The instrumentation finding that `[ExcludeFromCodeCoverage]` does not propagate to
      lambdas declared inside the exempt method is reported to F1 as a ledger/harness note, and the
      "#441 can deflate as well as inflate" refinement is added as a comment on **#441**.

- [ ] **AC-18.** The full C# toolchain passes in order in a single final pass, with the commands
      run stated explicitly: `csharpier .`; `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug
      /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`;
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable
      /p:TreatWarningsAsErrors=true`; `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`.

- [ ] **AC-19.** All evidence produced by this child is written to `<FEATURE>/evidence/<kind>/` —
      coverage to `<FEATURE>/evidence/qa-gates/`, the exemption-boundary artifact to
      `<FEATURE>/evidence/other/`. **No evidence is written to `artifacts/qa-gates/`,
      `artifacts/baselines/`, `artifacts/coverage/`, or any other non-canonical `artifacts/` path**,
      regardless of any instruction to the contrary.

- [ ] **AC-20.** The documented deviations D1-D10 (§11) are propagated: D1 (the
      `QuickFiler.Test.csproj` omission) and D4 (member-level, not file-level, exemptions) are
      recorded as notes to F1 and to the epic manifest; D2, D3, D5, D6, D7, D8, D9 are recorded in
      this child's completion summary so no later child repeats them.

---

## 15. Definition of Done

- [ ] All acceptance criteria in this file and in `user-story.md` are checked off with evidence.
- [ ] Per-file line and branch coverage evidence for all ten measured partials is committed under
      `<FEATURE>/evidence/qa-gates/`, in both the harness and class-level-union forms.
- [ ] The fresh exemption-boundary artifact is committed under `<FEATURE>/evidence/other/`.
- [ ] F1 ledger rows exist for all 11 in-scope files plus `QfcCidImageResolver.cs`.
- [ ] The full C# toolchain passed in order in one final pass, with commands stated.
- [ ] Cross-child contract notes (§10) have been sent to F4, F14, F3, and F11.
- [ ] Latent defects promoted; nothing left as prose in this folder.
- [ ] `git status` is clean — all evidence and artifacts committed.
