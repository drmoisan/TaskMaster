# Per-File Research — `QuickFiler/Controllers/QfcItemController.Conversation.cs`

- Feature: `quickfiler-item-controller-coverage` (issue #453), epic child F10 of epic #136
- Branch: `feature/quickfiler-item-controller-coverage`
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a359b62de7a79b16e`
- Production file: `QuickFiler/Controllers/QfcItemController.Conversation.cs` (235 lines)
- Coverage report used for the baseline:
  `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
  (indicative, captured on another feature's branch; F1's harness on this branch remains authoritative)

---

## 1. Corrected coverage baseline

### 1.1 The emitted numbers double-count

The Cobertura `<class>` element for this file is at report line 24004. It contains **both** a
`<methods>` block (6 method entries, 34 `<line>` children in total) **and** a class-level `<lines>`
block (102 `<line>` children). The emitted `line-rate` divides by the sum of both.

Arithmetic proof, from the report:

- Emitted `line-rate="0.911765"`. `124 / 136 = 0.911765`.
- `136 = 102` (class-level `<lines>`) `+ 34` (sum of the six `<method>/<lines>` children).
- `124 = 90` (covered class-level lines) `+ 34` (all method lines are covered).
- Emitted `branch-rate="0.961538"`. `25 / 26 = 0.961538`, where `26 = 18` (class-level conditions)
  `+ 8` (method-level conditions) and `25 = 17 + 8`.

Every line number appearing under `<methods>` also appears in the class-level `<lines>` block, so the
class-level block is the de-duplicated union. This is verified line by line: method lines
`33,34,36,38 / 41,42,43,44 / 161,163,171 / 174,175,176,177 / 180-184,187-192 / 222,224-227,231-233`
are all present in the class-level list.

### 1.2 Corrected figures

| Metric | Emitted (`line-rate`/`branch-rate`) | **Corrected (de-duplicated)** | Divergence | Gate | Verdict |
| --- | --- | --- | --- | --- | --- |
| Line | 91.18% (124/136) | **88.24% (90/102)** | -2.94 pts | >= 80% | PASS on both |
| Branch | 96.15% (25/26) | **94.44% (17/18)** | -1.71 pts | >= 75% | PASS on both |

**The epic's indicative table figure of 91.2% for this file is inflated.** The corrected line rate is
88.2%. No gate flips in either direction for this file, so there is no false pass here — unlike
`MailActions.cs`, where the divergence crossed the branch floor.

### 1.3 Multi-class union rule

The epic directs the harness to union multiple `<class>` elements sharing one `filename`, taking max
hits per line. A grep of the report for `filename="QuickFiler\Controllers\QfcItemController.Conversation.cs"`
returns exactly **one** `<class>` element (report line 24004), so the union is a no-op for this file.
The rule is still load-bearing for the sibling `FolderHandling.cs`, where the class-level list already
records `max(statement, lambda)` for a shared line — see that artifact.

### 1.4 Uncovered inventory (corrected, class-level `<lines>`)

Uncovered lines (12 of 102):

| Lines | Member | Report evidence |
| --- | --- | --- |
| 130, 131, 133, 134, 135, 136, 137, 138, 139 | `PopulateConversationAsync(ConversationResolver, CancellationToken, bool)` | report lines 24132-24140, all `hits="0"` |
| 212, 213, 214 | zero-count block inside the `RenderConversationCountAsync` dispatch lambda | report lines 24200-24202, all `hits="0"` |

Uncovered branch outcomes (1 of 18): source line 211 (`if (count == 0)` inside the
`RenderConversationCountAsync` lambda) is `condition-coverage="50% (1/2)"` (report line 24195). Only
the non-zero path is exercised.

---

## 2. Member inventory

All members are on `internal partial class QfcItemController` (declared
`QuickFiler/Controllers/QfcItemController.cs:25`). This file declares no fields, no properties, no
constructors, no events, and no nested types.

| # | Member | Lines | Accessibility | Exempt? |
| --- | --- | --- | --- | --- |
| 1 | `void PopulateConversation()` | 32-38 | public | no |
| 2 | `void PopulateConversation(ConversationResolver resolver)` | 40-44 | public | no |
| 3 | `async Task LoadConversationResolverAsync(CancellationTokenSource, CancellationToken, bool)` | 46-73 | public | no |
| 4 | `virtual Task<ConversationResolver> DoLoadConversationResolverCoreAsync(CancellationTokenSource, CancellationToken, bool)` | 75-92 (expression body) | protected virtual | **YES — `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` at line 79 — RATIFIED, see §7 (Correction, 2026-08-07)** |
| 5 | `async Task PopulateConversationAsync(CancellationTokenSource, CancellationToken, bool)` | 94-123 | public | no |
| 6 | `async Task PopulateConversationAsync(ConversationResolver, CancellationToken, bool)` | 125-139 | public | no |
| 7 | `void PopulateConversation(int count)` | 160-171 (lambda body 164-170) | public | no |
| 8 | `void RenderConversationCount()` | 173-177 | public | no |
| 9 | `void RenderConversationCount(int count)` | 179-192 | public | no |
| 10 | `async Task RenderConversationCountAsync(int, CancellationToken, bool)` | 194-219 (lambda body 208-215) | public | no |
| 11 | `void SetTopicThread(List<MailItemHelper>)` | 221-233 | public | no |

Lines 141-152 are a commented-out `PopulateConversation(DataFrame)` overload (dead comment block, no
IL).

The exemption at line 79 is **method-level**, confirming the brief: the measured denominator of 102
lines already excludes member #4. Quantification of its cost is in §7.

### Interface surface

`QuickFiler/Interfaces/IQfcItemController.cs` declares members 1, 2, 3, 5, 7 (as
`PopulateConversation(int countOnly)`), 8, and 9. It does **not** declare member 6
(`PopulateConversationAsync(ConversationResolver, ...)`), member 10, or member 11 — those are public
only on the concrete internal class.

---

## 3. What is already covered

Existing tests in `QuickFiler.Test/Controllers/`. Do not duplicate any of these.

| # | Member | Status | Covering test(s) |
| --- | --- | --- | --- |
| 1 | `PopulateConversation()` | COVERED (4/4 lines) | `QfcItemController.SeamFactoryTests.cs:35` `PopulateConversation_UsesResolverFactoryAndRendersCount` |
| 2 | `PopulateConversation(ConversationResolver)` | COVERED (4/4) | `QfcItemController.ConversationTests.cs:189` `PopulateConversation_WithResolver_StoresResolver` |
| 3 | `LoadConversationResolverAsync` | COVERED (all lines 51-73, branch n/a) | `ConversationTests.cs:77` (cancellation rethrow, line 66), `:100` (fault swallow, lines 68-72), `:318` (success, lines 56-62); also `QfcItemControllerTests.cs:116` and `:144` |
| 4 | `DoLoadConversationResolverCoreAsync` | NOT MEASURED (`[ExcludeFromCodeCoverage]`) — **ratified exemption, retained, see §7** | Overridden by `SeamController` in `ConversationTests.cs:37` and by the fixture in `QfcItemControllerTests.cs:46`; the base body is never executed |
| 5 | `PopulateConversationAsync(CTS, CT, bool)` | COVERED (lines 99-123, branches at 102 and 110 both 2/2) | `ConversationTests.cs:56` (null-resolver guard), `:318` (#255 deferred publish, `loadAll == false`), `QfcItemControllerTests.cs:116`, `:144` |
| 6 | `PopulateConversationAsync(ConversationResolver, CT, bool)` | **UNCOVERED (0/9 lines)** | none |
| 7 | `PopulateConversation(int)` | COVERED (10/10 incl. lambda, branch at 166 2/2) | `QfcItemController.SeamDispatcherTests.cs:42` (non-zero), `:54` (zero) |
| 8 | `RenderConversationCount()` | COVERED (4/4, branch 175 2/2) | `ConversationTests.cs:212` (null resolver -> 0), `:228` (resolver -> SameFolder) |
| 9 | `RenderConversationCount(int)` | COVERED (11/11, branches 181 and 188 both 2/2) | `ConversationTests.cs:126`, `:142`, `:158` |
| 10 | `RenderConversationCountAsync` | **PARTIALLY COVERED (18/21 lines; branch 211 at 1/2)** | `SeamDispatcherTests.cs:65` (background priority, non-zero count) and, indirectly, `ConversationTests.cs:318` (normal priority via `loadAll == false`), which is why line 203 is already 2/2 |
| 11 | `SetTopicThread` | COVERED (8/8, branch 224 2/2) | `ConversationTests.cs:249` (direct), `:266` (InvokeRequired marshal) |

---

## 4. The gap list

Two gaps, both small and both reachable with the **existing** harness.

**Gap A — member 6 is entirely uncovered and has no production call site.**
Lines 130-139 (9 lines). A solution-wide grep for `PopulateConversation` shows the only production
call to a resolver-taking overload is `QuickFiler/Controllers/QfcCollectionController.cs:1898`, which
calls the **synchronous** `PopulateConversation(resolver)` (member 2), not member 6. Member 6 is not on
`IQfcItemController`, so it is unreachable through the interface. It is currently dead production code.
See LD-1 in §10.

**Gap B — the zero-count branch of `RenderConversationCountAsync`.**
Lines 212-214 plus the true side of the `count == 0` test at line 211. The equivalent zero-count block
in the synchronous siblings (members 7, 8, 9) is covered; only the async variant is not.

**Branch-heavy members.** This file is not branch-dense: 18 conditions across 9 decision points, all
simple null/zero/`InvokeRequired` guards. There is no ranking or grouping logic in this file —
conversation *grouping* lives in `QfcCollectionController.EnumerateConversationMembers`
(`QfcCollectionController.cs:1875`, owned by F11) and conversation *resolution* lives in
`ConversationResolver.Loading.cs` (F4). The branch-dense surface the brief anticipated is not here.

---

## 5. `ConversationResolver` boundary (cross-child contract with F4 / #434)

This is the highest-risk coupling in the F10 assignment. F10 must not edit any file under
`QuickFiler/Helper Classes/`.

### 5.1 Construction sites reachable from the F10 file set

| Site | Call | Positional argument list | Resolved overload |
| --- | --- | --- | --- |
| `QfcItemController.Conversation.cs:34` | `_conversationResolverFactory(Mail)` | 1 arg: `MailItem` | delegate `Func<MailItem, ConversationResolver>` (field declared `QfcItemController.cs:69`) |
| `QfcItemController.Initialization.cs:382-388` (default factory body, F10 file set, **not** one of my three files) | `new ConversationResolver(_globals, mail, _tokenSource, Token, SetTopicThread)` | 5 positional: `IApplicationGlobals`, `MailItem`, `CancellationTokenSource`, `CancellationToken`, `Action<List<MailItemHelper>>` | `ConversationResolver.cs:70-84` |
| `QfcItemController.Conversation.cs:85-92` | `ConversationResolver.LoadAsync(_globals, ItemHelper, tokenSource, token, loadAll, SetTopicThread)` | 6 positional: `IApplicationGlobals`, `MailItemHelper`, `CancellationTokenSource`, `CancellationToken`, `bool`, `Action<List<MailItemHelper>>` | static `ConversationResolver.cs:126-133` (the `MailItemHelper` overload, **not** the `MailItem` overload at `:86`) |

No other construction site exists in `QfcItemController.Conversation.cs` or
`QfcItemController.FolderHandling.cs`.

### 5.2 Current F4-owned signatures F10 depends on

From `QuickFiler/Helper Classes/ConversationResolver.cs`:

- `:62` `private ConversationResolver()` — parameterless, private.
- `:64` `public ConversationResolver(IApplicationGlobals appGlobals, MailItem mailItem)` — used by the
  **existing tests** (`ConversationTests.cs:183`, `:303`; `SeamFactoryTests.cs:42`;
  `SeamDispatcherTests.cs:165`).
- `:70-76` `public ConversationResolver(IApplicationGlobals, MailItem, CancellationTokenSource, CancellationToken, System.Action<List<MailItemHelper>> updateUI = null)` — the production default factory target.
- `:86-93` `public static async Task<ConversationResolver> LoadAsync(IApplicationGlobals, MailItem, CancellationTokenSource, CancellationToken, bool loadAll, System.Action<List<MailItemHelper>> updateUI = null)`.
- `:126-133` `public static async Task<ConversationResolver> LoadAsync(IApplicationGlobals, MailItemHelper, CancellationTokenSource, CancellationToken, bool loadAll, System.Action<List<MailItemHelper>> updateUI = null)` — **the overload this file binds to**.
- `:164-170` a third `LoadAsync(IApplicationGlobals, IEnumerable<MailItem>, ...)` overload — not used by F10.

Members read from the resolver by this file:

- `.Count.SameFolder` (`Conversation.cs:36, 43, 105, 135, 175`) — `Count` is
  `public Pair<int> Count { get; internal set; }` at `ConversationResolver.Loading.cs:265-271`, a lazy
  `Initializer.GetOrLoad` over `LoadCount()`.
- `.ConversationInfo.Expanded` (`Conversation.cs:121`) — `public Pair<List<MailItemHelper>> ConversationInfo { get; set; }` at `ConversationResolver.Loading.cs:20-35`, also lazy via `Initializer.GetOrLoad(..., LoadConversationInfo, ...)`.
- Assignment to the controller's own `ConversationResolver` property (`QfcItemController.cs:110-114`),
  typed as the **concrete** `ConversationResolver`, not `IConversationResolver`.

### 5.3 `IConversationResolver` is not consumed by F10

`QuickFiler/Helper Classes/IConversationResolver.cs` declares `ConversationInfo`, `ConversationItems`,
`Count`, `Df`, `UpdateUI`, `FullyLoaded`, `Parent`, `PropertyChanged`, and four load methods. **F10
does not reference this interface anywhere.** The controller field and property
(`QfcItemController.cs:109-114`) and the factory delegate (`QfcItemController.cs:69`) are all typed to
the concrete class. The `IConversationResolver` surface is therefore not part of F10's contract; only
the concrete constructor/`LoadAsync`/`Count`/`ConversationInfo` shapes above are.

### 5.4 Can F10 reach its targets against the current shape?

**Yes, with no upstream change.** Every test proposed in §8 uses the two-argument public constructor
`ConversationResolver(IApplicationGlobals, MailItem)` at `:64` plus the `internal set` on `Count` and
the public setter on `ConversationInfo`, exactly as the existing `BuildResolverWithCount` and
`BuildResolverWithConversation` helpers in `ConversationTests.cs:179` and `:299` already do. No new
resolver member, no signature change, and no `IConversationResolver` addition is required.

### 5.5 Cross-child contract note (binding on F4 / #434)

- F10 depends on the **current** shape of `ConversationResolver.cs:64` (two-arg ctor),
  `:70` (five-arg ctor), `:126` (`MailItemHelper` `LoadAsync` overload),
  `Loading.cs:265` (`Count`, with an `internal set` visible to `QuickFiler.Test`), and
  `Loading.cs:20` (`ConversationInfo`, public get/set).
- F4 may **append parameters with defaults** to any of those signatures. It must not reorder, retype,
  or remove existing positional parameters, must not tighten `Count`'s setter accessibility below
  `internal`, and must not make `ConversationInfo`'s setter non-public.
- F10 will not edit `ConversationResolver.cs`, `ConversationResolver.Loading.cs`, or
  `IConversationResolver.cs`. Any change F10 would want there is recorded here as a note, not made.

---

## 6. Seam analysis

For each uncovered member, what blocks a deterministic unit test today.

| Uncovered target | Blocker | Minimum seam required |
| --- | --- | --- |
| Member 6, lines 130-139 | **Nothing.** `token.ThrowIfCancellationRequested()`, a field assignment, and a call to member 10 which already routes through the injected `_uiDispatcher`. No COM, no WinForms, no static state, no UI thread. | **None.** Existing harness suffices. |
| Member 10, lines 211-214 | **Nothing.** The lambda body writes `_itemViewer.ConversationCountText` and `.ConversationCountBackColor`, both `IItemViewer` members already mocked in existing tests. Dispatch is through `_uiDispatcher.InvokeAsync(Action, DispatcherPriority, CancellationToken)`, already stubbed by `QfcItemControllerTestSupport.BuildSyncDispatcher()` (`TestSupport.cs:114-127`). | **None.** Existing harness suffices. |
| Member 4 (informational only — **not planned**, see §7) | Static `ConversationResolver.LoadAsync` cannot be mocked. But the call site itself is a single sequence point that executes before the callee faults, so the line is reachable in principle. | **None new** would be required if this were ever revisited. The `protected virtual` seam already exists; the base body could be invoked by reflection on a `HarnessController`. This row is retained only so the reachability fact is not lost; F10 does not act on it. |

**Confirmation of the peer finding.** The peer researcher's conclusion that the existing harness
(`IItemViewer`, `IUiDispatcher`, `BuildSyncDispatcher`, `HarnessController`) is sufficient with zero
production change **holds for this file**. No new interface seam, no new injectable delegate, no
adapter, and no STA-bound test are required. `*.StaTests.cs` is not needed.

**Moq non-virtual caveat.** No uncovered path in this file calls a non-virtual method on a concrete
class. The only concrete-class calls are to `ConversationResolver` property getters, which are set
directly on a real instance rather than mocked. The Moq limitation does not bite here.

**Harness components used, with file:line:**

- `HarnessController` — `QfcItemController.TestSupport.cs:25-29` (exposes the `protected
  QfcItemController()` ctor at `QfcItemController.Initialization.cs:27`).
- `QfcItemControllerTestSupport.SetField` — `TestSupport.cs:37-47`.
- `QfcItemControllerTestSupport.InvokeNonPublic` — `TestSupport.cs:66-80` (needed only for the
  optional member-4 test; note it uses `GetMethod(name, NonPublic | Instance)` and will resolve
  `DoLoadConversationResolverCoreAsync` unambiguously since there is exactly one overload).
- `QfcItemControllerTestSupport.BuildSyncDispatcher` — `TestSupport.cs:102-137`.

`EnsureUiThreadDispatcher` and `EnableHandlelessThemeInvoke` are **not** needed for this file; nothing
here touches `UiThread.Dispatcher` or `Theme`.

---

## 7. The exemption at line 79 — RATIFIED, retained (Correction, 2026-08-07)

**Governing authority.** This exemption is not a candidate for removal. It is one of the 19
`[ExcludeFromCodeCoverage]` sites the project maintainer formally ratified on 2026-07-02 under issue
#227, after five remediation cycles that reduced the family's exemption boundary from 103 members to
19. Authoritative sources, both read in full for this correction:

- `docs/features/archive/2026-06-29-qfc-item-controller-testability-227/maintainer-decision.2026-07-02.md`
  — "**Decision:** RATIFIED. The 19-member `[ExcludeFromCodeCoverage]` boundary ... is accepted."
- `docs/features/archive/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-07-02T17-00.md`
  §"3. Deliberate virtual test seams (3, unchanged)" — the entry for `DoLoadConversationResolverCoreAsync`
  reads verbatim: *"Deliberate `virtual` override point; production body is intentionally never
  exercised because tests override it — a testing pattern, not a barrier."*

This artifact previously (as of the initial pass on 2026-08-07) classified this site as
`removable-with-seam` and recommended removing the attribute plus adding CT-4. **That recommendation
is withdrawn.** The maintainer has already adjudicated this exact seam and accepted the exact
rationale this artifact independently re-derived; re-litigating it and proposing removal would
contradict a ratified decision rather than surface new information. The cross-cutting companion
artifact (`cross-cutting-exemption-and-coverage-analysis.md` §1.2, site 1) still carries the
superseded `removable-with-seam` classification — that classification is stale as of this
ratification finding and should not be used to plan a de-exemption task. That companion file is not
one of my three files, so it is flagged here rather than edited.

### 7.1 Verification that the ratified rationale still holds against the current code (2026-08-07)

The ratification is not treated as a standing grant regardless of drift. Three checks, each against
the current tree:

| Check | Ratified claim | Current-code evidence | Holds? |
| --- | --- | --- | --- |
| Still `virtual`? | "Deliberate `virtual` override point" | `QfcItemController.Conversation.cs:80` reads `protected virtual Task<ConversationResolver> DoLoadConversationResolverCoreAsync(...)`. Read directly, 2026-08-07. | Yes |
| Still overridden by tests, not called directly? | "tests override it" | Exactly two `protected override` declarations exist solution-wide: `QfcItemController.ConversationTests.cs:37` and `QfcItemControllerTests.cs:46`. A solution-wide grep for `DoLoadConversationResolverCoreAsync` finds no third override, no `base.DoLoadConversationResolverCoreAsync(...)` call, and no reflection-based `InvokeNonPublic`/`GetMethod` targeting it outside this research artifact's own (unexecuted) CT-4 proposal. | Yes |
| Production body still genuinely unexercised by design? | "production body is intentionally never exercised" | The sole production call site, `LoadConversationResolverAsync` (`:57`), invokes the member through ordinary virtual dispatch. Because the only subclasses that exist anywhere in the solution (`SeamController`, the `QfcItemControllerTests` fixture) both override it, and the production `QfcItemController` type is otherwise constructed directly (not through a further subclass), the base expression at line 85 has no code path by which a test exercises it. | Yes |

**Finding: no drift. The ratified rationale holds exactly as written.** No basis exists to reopen
this exemption.

### 7.2 Denominator cost, quantified for budgeting either way (per request, not a recommendation to act)

Member 4 is expression-bodied:

```csharp
protected virtual Task<ConversationResolver> DoLoadConversationResolverCoreAsync(
    CancellationTokenSource tokenSource,
    CancellationToken token,
    bool loadAll
) =>
    ConversationResolver.LoadAsync(
        _globals, ItemHelper, tokenSource, token, loadAll, SetTopicThread
    );
```

An expression-bodied member spanning multiple source lines emits **exactly one** Cobertura `<line>`
entry, at the line where the expression begins. Positive control from the same report:
`get_TopFolderScore` (report line 22930) covers `QfcItemController.cs:251-254` in source and emits a
single `<line number="254">`.

**Quantified impact if ever de-exempted: +1 line to the denominator.** 102 -> 103. (This is the
precise Cobertura-emission-based figure, derived from the positive control above; the companion
cross-cutting artifact's coarser `Δlines +8` estimate for this site counts raw physical span lines
80-92 rather than actual emitted `<line>` entries and overstates the true cost for this
expression-bodied member. Use +1, not +8, if this figure is ever needed for a ledger row.)

- If ever de-exempted and left uncovered: 90/103 = 87.38% (down 0.86 pts from 88.24%).
- If ever de-exempted and covered by the CT-4 sketch in §8.3: 91/103 = 88.35%.

**Revised recommendation (supersedes the 2026-08-07 initial pass): RETAIN the attribute. Do not
remove it, and do not schedule CT-4 as an F10 task.** The original recommendation in this section
argued the barrier was false because the line is reachable and faults deterministically — that
technical observation is correct, but it addresses reachability, not the maintainer's actual
rationale. The ratified position in `exemption-boundary.2026-07-02T17-00.md` §3 is not "this cannot
be reached"; it is "the override point IS the test seam by design, so the base body is intentionally
left unexercised as a matter of test architecture, not blocked by a host dependency." Reachability
and intentional non-exercise are not in tension, and §7.1 confirms the design rationale still holds.
The figures above are retained here only so a future plan can budget the +1 line either way, per the
orchestrator's request; they are not a call to action.

---

## 8. State-transition invariants and proposed test cases

### 8.1 Invariants this file holds

| ID | Invariant | Where | Pinned by |
| --- | --- | --- | --- |
| I-1 | Cancellation is observable, never swallowed: an `OperationCanceledException` from the load seam propagates to the caller. | `Conversation.cs:63-67` | existing `ConversationTests.cs:77` |
| I-2 | A non-cancellation load fault is logged and swallowed, and `ConversationResolver` is left null. | `Conversation.cs:68-72` | existing `ConversationTests.cs:100` |
| I-3 | A null resolver after load short-circuits before any render. | `Conversation.cs:102-103` | existing `ConversationTests.cs:56` |
| I-4 | Load ordering: resolve, then re-check the token, then render, then (deferred path only) publish the fast list. | `Conversation.cs:100-122` | existing `ConversationTests.cs:318` (#255 regression) |
| I-5 | Re-entrancy: `PopulateConversation(resolver)` and both async overloads overwrite `ConversationResolver` unconditionally; the last writer wins and the rendered count always matches the stored resolver. | `Conversation.cs:42-43, 57, 133-138` | existing `:189` for the sync overload; **CT-1** for the async overload |
| I-6 | Cancellation precedes state mutation in the resolver-taking async overload: a cancelled token must throw before `ConversationResolver` is reassigned. | `Conversation.cs:131` precedes `:133` | **CT-2 (new)** |
| I-7 | Zero-count rendering marks the badge red on every render path (sync, sync-parameterless, and async). | `Conversation.cs:167-169, 189-191, 211-214` | existing `:142`, `:212`, `SeamDispatcherTests.cs:54`; **CT-3 (new)** closes the async path |
| I-8 | UI marshaling: every viewer write is either behind `InvokeRequired` or routed through `_uiDispatcher`; no member writes the viewer from an arbitrary thread. | `Conversation.cs:163, 181-185, 207-218, 224-228` | existing `:158`, `:266`, `SeamDispatcherTests.cs:65` |

There is no dispose/teardown guard in this file (`Cleanup` lives elsewhere in the family), so there is
no "act after dispose" invariant to pin here.

### 8.2 Determinism requirements

- **No wall-clock read.** The file contains no `DateTime.Now`, `DateTime.UtcNow`, `Stopwatch`, or
  `Environment.TickCount`. Verified by full read of all 235 lines. No banned-API finding.
- **No randomness.** No `Random`, `Random.Shared`, or `Guid.NewGuid`.
- **No thread-pool offload.** Unlike the sibling `FolderHandling.cs`, this file never calls
  `Task.Run`. All asynchrony is `await` over a caller-supplied or seam-supplied `Task`.
- **How conversation loading is scheduled, precisely:** `LoadConversationResolverAsync` (`:57`) awaits
  the overridable seam `DoLoadConversationResolverCoreAsync`. In tests the seam is replaced by a
  `Func<Task<ConversationResolver>>` returning `Task.FromResult`, `Task.FromException`, or a
  pre-completed task — see `SeamController` at `ConversationTests.cs:27-42`. Rendering is scheduled
  through `_uiDispatcher.InvokeAsync(Action, DispatcherPriority, CancellationToken)` (`:207`), which
  `BuildSyncDispatcher` executes inline and returns `Task.CompletedTask`. **A test therefore drives
  the whole pipeline synchronously by awaiting the returned `Task`; no fake timer, no polling, no
  `Thread.Sleep`, no `Task.Delay` is needed or permitted.**
- `DispatcherPriority.Background` vs `.Normal` (`:203-205`) is a value passed to the mock and asserted
  on, not a scheduling behavior the test depends on.

### 8.3 Proposed test cases

Each is an independently verifiable atomic task. All are MSTest `[TestMethod]`, Moq for doubles,
FluentAssertions for assertions, Arrange-Act-Assert, no temporary files, no live forms, no popups.

| ID | Target member | Scenario | Fixture | Covers |
| --- | --- | --- | --- | --- |
| **CT-1** | 6 — `PopulateConversationAsync(ConversationResolver, CancellationToken, bool)` | positive | `HarnessController`; `BuildSyncDispatcher()` into `_uiDispatcher`; `Mock<IItemViewer>` into `_itemViewer`; resolver built with `new ConversationResolver(Mock<IApplicationGlobals>, Mock<MailItem>) { Count = new Pair<int>(7, 7) }`. Act: `await controller.PopulateConversationAsync(resolver, CancellationToken.None, loadAll: false)`. Assert `controller.ConversationResolver` is same-as the resolver and `viewer.VerifySet(v => v.ConversationCountText = "7", Times.Once())`. | lines 130, 131, 133, 134-138, 139 (9 lines) |
| **CT-2** | 6 | negative / ordering (I-6) | Same fixture; a `CancellationTokenSource` cancelled before the call. Act: `Func<Task> act = () => controller.PopulateConversationAsync(resolver, cts.Token, false)`. Assert `await act.Should().ThrowAsync<OperationCanceledException>()` **and** `controller.ConversationResolver.Should().BeNull()` (proving the guard at 131 precedes the assignment at 133) **and** `dispatcher.Verify(d => d.InvokeAsync(It.IsAny<Action>(), It.IsAny<DispatcherPriority>(), It.IsAny<CancellationToken>()), Times.Never())`. | no new lines; pins I-6 and the 131 throw path |
| **CT-3** | 10 — `RenderConversationCountAsync` | edge (zero count) | `HarnessController`; `BuildSyncDispatcher()`; `Mock<IItemViewer>`. Act: `await controller.RenderConversationCountAsync(0, CancellationToken.None, backgroundLoad: false)`. Assert `ConversationCountText = "0"` once and `ConversationCountBackColor = Color.Red` once, and that `InvokeAsync` was called with `DispatcherPriority.Normal`. | lines 212, 213, 214 (3 lines) + line 211 true side (1 condition) |
| **CT-4** | 4 — `DoLoadConversationResolverCoreAsync` | **NOT PLANNED under F10.** Retained below only as a reference sketch in case issue #227's ratified boundary is ever reopened by the maintainer; see §7. | `HarnessController` with `_globals` = `Mock<IApplicationGlobals>.Object` and `ItemHelper` left null. Act: invoke the protected method via `QfcItemControllerTestSupport.InvokeNonPublic(controller, "DoLoadConversationResolverCoreAsync", cts, cts.Token, false)`, cast to `Task<ConversationResolver>`. Assert the task faults with `NullReferenceException` — `ConversationResolver.LoadAsync(IApplicationGlobals, MailItemHelper, ...)` at `ConversationResolver.cs:135-138` assigns `resolver.MailHelper = helper` then dereferences `helper.Item`. | line 85 (1 line), only if ever de-exempted |

**Coverage projection.** The plan of record is CT-1 through CT-3 only. CT-4 and the removal row are
kept purely for budgeting reference per §7.2 and are not part of F10's scope.

| State | Lines | Line % | Branch | Branch % |
| --- | --- | --- | --- | --- |
| Today (corrected) | 90/102 | 88.24% | 17/18 | 94.44% |
| **+ CT-1, CT-2, CT-3 (plan of record)** | **102/102** | **100.00%** | **18/18** | **100.00%** |
| (reference only, not planned) attribute removed + CT-4 | 103/103 | 100.00% | 18/18 | 100.00% |
| (reference only, not planned) attribute removed, CT-4 omitted | 102/103 | 99.03% | 18/18 | 100.00% |

Both gates (80% line, 75% branch) are cleared by CT-1 and CT-3 alone; CT-2 pins an invariant rather
than closing a gate. The ratified exemption at line 79 is left in place; it costs nothing against
either gate because the exempted line is outside the denominator by design.

---

## 9. File-size and creation impact

### Production

`QfcItemController.Conversation.cs` is 235 lines. **The exemption at line 79 is retained (ratified,
see §7), so no production edit at that line is planned** and the file stays at 235 lines. (For
reference only: removing the one-line attribute would take it to 234 — not applicable under the
current plan.) No production file is created and no `QuickFiler/QuickFiler.csproj` edit is needed for
this file (`Compile Include="Controllers\QfcItemController.Conversation.cs"` already exists at
`QuickFiler.csproj:331`). No new ledger row is required for this file beyond recording the existing
ratified-exempt status.

### Tests

| Test file | Current lines | Headroom to 500 | Can it absorb the new tests? |
| --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.ConversationTests.cs` | 352 | 148 | Technically yes for CT-1..CT-3 (~110 lines), but it would land near 465 and leave almost nothing for a remediation cycle. |
| `QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs` | 352 | 148 | Same caveat. |
| `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` | 284 | 216 | Possible but topically wrong (this is the factory-seam file). |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 365 | 135 | Shared helpers only; no new tests. |

**Recommendation: create one new test file**
`QuickFiler.Test/Controllers/QfcItemController.ConversationAsyncTests.cs` holding CT-1 through CT-4
(~140 lines projected). This keeps every existing file well clear of 500 and isolates F10's additions
from the concurrent siblings editing the other `QfcItemController.*Tests.cs` files.

**Mandatory csproj edit.** `QuickFiler.Test/QuickFiler.Test.csproj` is a legacy non-SDK project with
no globbing — every test file is an explicit `<Compile Include=...>` (the `QfcItemController.*`
entries occupy lines 90 and 132-147). The new file **must** be added as
`<Compile Include="Controllers\QfcItemController.ConversationAsyncTests.cs" />` adjacent to line 133.
This confirms the brief's correction: **epic.md names only `QuickFiler/QuickFiler.csproj` and is
incomplete** — the test csproj carries the same constraint.

**CRLF.** Both csproj files are CRLF-terminated. Use the Edit tool or `perl -0777` with explicit
`\r\n`. A git-bash `sed -i` will strip CRLF and produce a whole-file diff that is guaranteed to
conflict at fan-in.

---

## 10. Latent defects for promotion (do not fix under F10)

| ID | File:line | Description | Severity |
| --- | --- | --- | --- |
| **LD-1** | `QuickFiler/Controllers/QfcItemController.Conversation.cs:125-139` | `PopulateConversationAsync(ConversationResolver, CancellationToken, bool)` has **no production call site** (the only resolver-taking production call, `QfcCollectionController.cs:1898`, uses the synchronous overload) and is absent from `IQfcItemController`. It is also missing the issue-#255 fast-list publication that the sibling overload gained at line 121, so if it were ever wired into the deferred path it would reproduce the #255 symptom (non-zero count badge, empty conversation list). Either delete it or bring it to parity with `:110-122`. | Medium |
| **LD-2** | `QuickFiler/Controllers/QfcItemController.Conversation.cs:68-72` | A non-cancellation load fault is logged and swallowed, leaving `ConversationResolver` null; `PopulateConversationAsync` then returns at line 103 without rendering. The user sees a stale or blank conversation badge with no error indication. Intentional per the in-code comment, but there is no user-visible failure signal. | Low |
| **LD-3** | `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:120-148` (test-policy, sibling file, recorded here for completeness) | `ReadControllerSource` reads production source from disk via `File.ReadAllText` on a path derived from `AppDomain.CurrentDomain.BaseDirectory`. This is a filesystem dependency in a unit test and a brittle relative-path assumption. Full write-up in the `FolderHandling` artifact. | Medium (test policy) |

Per epic.md §"Latent Defect Promotion", these must be promoted to GitHub issues via the MCP promotion
lifecycle during F10's execution, not left as prose in this folder.

**Duplicate-issue check (2026-08-07, cycle 2).** Checked against the confirmed open-issue set (#230,
#426, #427, #438, #440, #441, #444, #457, #463 and the full F1-F15 child list). None of LD-1, LD-2, or
LD-3 names a member, symptom, or file already covered by an open issue: LD-1/LD-2 concern
`PopulateConversationAsync(ConversationResolver, ...)` and the swallowed-fault path, which no open
issue references; LD-3 concerns `FolderHandlingTests.cs`'s filesystem read, which is also not named by
any open issue. None of these three is a duplicate of #441 (Cobertura double-count), #457
(`[ExcludeFromCodeCoverage]` lambda leak), or #463 (WebView2 en-dash arg) — those are unrelated
defects already filed and are not re-promoted here.

---

## 11. Sibling boundaries — files this child must not edit

| Dependency | Owner | F10 action |
| --- | --- | --- |
| `QuickFiler/Helper Classes/ConversationResolver.cs`, `ConversationResolver.Loading.cs`, `IConversationResolver.cs` | **F4 (#434)** | Read-only. Contract recorded in §5.5. |
| `QuickFiler/Helper Classes/cInfoMail.cs`, `QfEnums.cs`, and the rest of `QuickFiler/Helper Classes/` | **F4 (#434)** | Not touched. |
| `QuickFiler/Controllers/KeyboardHandler.cs` and the `Ka*`/`Kbd*` family | **F3 (#430)** | Not touched by this file. |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | **F5** | Not referenced by this file. |
| `QuickFiler/Viewers/IItemViewer.cs` | **F14** | Read-only. F10 consumes `ConversationCountText`, `ConversationCountBackColor`, `InvokeRequired`, `Invoke`, `SetConversationItems`, `SortConversationByDate`. No addition required. |
| `UtilitiesCS/Threading/IUiDispatcher` | UtilitiesCS (outside the epic) | Read-only; already sufficient. |
| `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs` | **UtilitiesCS, not F4** — verified: the type is defined outside `QuickFiler/Helper Classes/`. | Not used by this file. |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` (default resolver factory at `:382`) | **F10 itself**, but assigned to a concurrent peer researcher | Read-only from this artifact; the construction shape is documented in §5.1 so the two artifacts agree. |
| `QuickFiler/Controllers/QfcCollectionController.cs` | **F11** | Read-only; cited only to establish that member 6 has no call site. |

No boundary crossing is proposed. The only cross-child dependency is the read-only
`ConversationResolver` shape contract in §5.5.

---

## 12. Out-of-scope observation flagged for the orchestrator (not acted on here)

Reading the ratification evidence for §7 surfaced a parallel case that is **not** in my file
assignment: `exemption-boundary.2026-07-02T17-00.md` §3's "Deliberate virtual test seams" bucket also
ratifies `ToggleExpansion(Enums.ToggleState)` and `ToggleExpansionAsync(Enums.ToggleState)`
(`QfcItemController.Navigation.cs`) under the same rationale as this file's site. The cross-cutting
companion artifact (`cross-cutting-exemption-and-coverage-analysis.md` §1.2, sites 15-16) classifies
both as `removable-as-is` and the `file-QfcItemController.Navigation.md` artifact (not one of my
three files) recommends de-exempting them. That recommendation may rest on the same "reachable
therefore removable" reasoning this artifact withdrew in §7 for the Conversation.cs site. This is
flagged here only because it surfaced incidentally while verifying my own file; it is Navigation.md's
and the orchestrator's decision to make, not mine to correct.
