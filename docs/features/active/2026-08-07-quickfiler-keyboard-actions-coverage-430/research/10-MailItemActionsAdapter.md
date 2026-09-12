# Per-File Coverage Research — `QuickFiler/Interfaces/MailItemActionsAdapter.cs`

Timestamp: 2026-08-07T22-00
Feature: `quickfiler-keyboard-actions-coverage` (child F3, issue #430)
Parent epic: `quickfiler-per-file-coverage` (issue #136)
Branch: `feature/quickfiler-keyboard-actions-coverage`

---

## 1. File Under Research

| Attribute | Value |
| --- | --- |
| Path | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aafcc2531072ca96b\QuickFiler\Interfaces\MailItemActionsAdapter.cs` |
| Line count | 47 lines of source |
| Compiled | Yes — `QuickFiler/QuickFiler.csproj:368` `<Compile Include="Interfaces\MailItemActionsAdapter.cs" />` |
| `[ExcludeFromCodeCoverage]` status | **Absent.** Grep for `ExcludeFromCodeCoverage` across `QuickFiler\Interfaces\` returned no matches. (Historical note: a stale exemption on this type was removed during issue #227 cycle-2; see `.claude/agent-memory/task-researcher/feedback_exemption_audit_check_proven_techniques.md:18–20`, which records that the type's claimed "COM barrier" was false because `MailItem` is an interop **interface** and was already fully `Mock<MailItem>`-tested.) |
| Namespace / type | `QuickFiler.Interfaces.MailItemActionsAdapter` — `public sealed class`, implements `IMailItemActions` |
| Existing tests | `QuickFiler.Test\Controllers\MailItemActionsAdapterTests.cs` (7 `[TestMethod]`s), registered at `QuickFiler.Test\QuickFiler.Test.csproj:148` |
| Exemption-status authority | **F1's `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`.** Recommended classification recorded in §4. |

### Executable-behavior determination

**The file contains executable behavior and is squarely in the coverage denominator.** It declares
one constructor and eight accessor/method bodies, all expression-bodied 1:1 forwards to a
`Microsoft.Office.Interop.Outlook.MailItem`. The Cobertura instrumenter emits a class entry with
12 statement lines (enumerated in §3).

---

## 2. Structural Inventory

| Lines | Member | Signature | Body | Dependencies |
| --- | --- | --- | --- | --- |
| 1 | using directive | `using Microsoft.Office.Interop.Outlook;` | n/a | Outlook PIA |
| 12 | type | `public sealed class MailItemActionsAdapter : IMailItemActions` | n/a | `IMailItemActions` |
| 14 | field | `private readonly MailItem _mail;` | no initializer → emits no line | `MailItem` |
| 17–20 | ctor | `MailItemActionsAdapter(MailItem mail)` | `_mail = mail;` | `MailItem`. **No null guard.** |
| 23 | method | `public MailItem Reply()` | `=> _mail.Reply();` | `MailItem.Reply()` |
| 26 | method | `public MailItem ReplyAll()` | `=> _mail.ReplyAll();` | `MailItem.ReplyAll()` |
| 29 | method | `public MailItem Forward()` | `=> _mail.Forward();` | `MailItem.Forward()` |
| 32 | method | `public void Display()` | `=> _mail.Display();` | `MailItem.Display(object Modal)` — the optional `Modal` argument is **omitted** at the call site |
| 35–39 | property | `public bool UnRead` | get (37) `=> _mail.UnRead;` / set (38) `=> _mail.UnRead = value;` | `MailItem.UnRead` |
| 42 | method | `public void Save()` | `=> _mail.Save();` | `MailItem.Save()` |
| 45 | property | `public string EntryID` | get `=> _mail.EntryID;` | `MailItem.EntryID` |

**Branch inventory: zero.** There is no `if`, no ternary, no `switch`, no null-conditional operator,
no `??`, no loop, and no `try`/`catch` anywhere in the file. Cobertura reports
`branch-rate="1"` with `complexity="1"` for every method, corroborating a cyclomatic complexity of 1
throughout.

### Is the COM delegation behind an injectable seam? (explicitly asked)

**Yes, and the seam is the constructor parameter itself.** `MailItem` is declared in the Outlook
Primary Interop Assembly as a COM **interface**, not a class. Moq can therefore create a proxy for it
directly with `new Mock<MailItem>()` — no wrapper, no live Outlook process, no STA thread. The
existing test file does exactly that at `MailItemActionsAdapterTests.cs:17–21`:

```csharp
private static (MailItemActionsAdapter adapter, Mock<MailItem> mail) Build()
{
    var mail = new Mock<MailItem>();
    return (new MailItemActionsAdapter(mail.Object), mail);
}
```

This is the single most important fact for this file: because `MailItem` is mockable, the adapter's
COM boundary imposes **no coverage barrier at all**. Any claim that this file requires a live Outlook
host is false and has already been adjudicated as false in this repository.

---

## 3. Existing Test Coverage (static analysis)

### 3a. Does the existing suite EXERCISE the delegate bodies, or only verify a mock? (explicitly asked)

**It exercises the real bodies.** `Build()` constructs the genuine `MailItemActionsAdapter` (not a
mock of it) and each test calls a real adapter member; the mock sits *beneath* the adapter as the
collaborator, not in place of it. Every assertion therefore runs the adapter's own IL.

This is confirmed empirically rather than inferred. The committed Cobertura report at
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml:14448–14513`
records:

```
<class line-rate="1" branch-rate="1" complexity="1"
       name="QuickFiler.Interfaces.MailItemActionsAdapter"
       filename="QuickFiler\Interfaces\MailItemActionsAdapter.cs">
```

with all twelve lines — 17, 18, 19, 20, 23, 26, 29, 32, 37, 38, 42, 45 — recorded at `hits="1"`, and
all nine methods (`.ctor`, `Reply`, `ReplyAll`, `Forward`, `Display`, `get_UnRead`, `set_UnRead`,
`Save`, `get_EntryID`) at `line-rate="1"`.

**Current measured state: 12/12 lines, 100% line coverage; 0 branches, 100% branch coverage.**

### 3b. Member-by-member / branch-by-branch delta

| Member (line) | Cobertura hits | Exercised by test method (by name) | Assertion actually made | Unreached aspect |
| --- | --- | --- | --- | --- |
| `.ctor` (17–20) | 1 | `Build()` helper, invoked by all 7 tests | Construction with a non-null mock succeeds | **Null argument behavior never exercised** (see G1) |
| `Reply()` (23) | 1 | `Reply_ForwardsToUnderlyingMailItem` | Returns the same instance the mock returns; `Verify(Times.Once())` | Null return; throwing collaborator (G2, G3) |
| `ReplyAll()` (26) | 1 | `ReplyAll_ForwardsToUnderlyingMailItem` | Same shape as `Reply` | Throwing collaborator (G3) |
| `Forward()` (29) | 1 | `Forward_ForwardsToUnderlyingMailItem` | Same shape as `Reply` | Throwing collaborator (G3) |
| `Display()` (32) | 1 | `Display_ForwardsToUnderlyingMailItem` | `mail.Verify(m => m.Display(It.IsAny<object>()), Times.Once())` | **The `Modal` argument value is never asserted** — `It.IsAny<object>()` matches a modal and a non-modal call identically (G4). Throwing collaborator (G3) |
| `get_UnRead` (37) | 1 | `UnRead_GetAndSet_ForwardToUnderlyingMailItem` | `SetupGet(...).Returns(true)` then `.Should().BeTrue()` | **Only the `true` value is asserted**; the `false` return is never exercised (G5) |
| `set_UnRead` (38) | 1 | `UnRead_GetAndSet_ForwardToUnderlyingMailItem` | `adapter.UnRead = false;` then `VerifySet(m => m.UnRead = false, Times.Once())` | **Only the `false` assignment is asserted**; `true` never set (G5) |
| `Save()` (42) | 1 | `Save_ForwardsToUnderlyingMailItem` | `Verify(m => m.Save(), Times.Once())` | Throwing collaborator (G3) |
| `get_EntryID` (45) | 1 | `EntryID_ForwardsToUnderlyingMailItem` | `SetupGet(...).Returns("entry-99")` then `.Should().Be("entry-99")` | **Null / empty EntryID never exercised** (G6) |
| Type contract | n/a | none | — | **No test asserts `MailItemActionsAdapter` is assignable to `IMailItemActions`** (G7) |

**Numeric measurement statement (required by the epic):** the figures above are read from a committed
Cobertura artifact produced by a prior feature's run. Definitive per-file numeric coverage for this
child will be re-measured at execution time with **F1's per-file coverage report harness**, derived
from the Cobertura output of `Invoke-MSTestWithCoverage.ps1`, and recorded under
`docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/evidence/qa-gates/`.

---

## 4. Coverage Gaps

### Line and branch gaps: NONE

The file is at 100% line coverage and has no branches. It already exceeds the epic's >= 80% per-file
floor and the `.claude/rules/csharp.md:40` >= 90% new-code floor. **Issue #136's non-duplication
mandate therefore forbids writing any further "forwards correctly" test for this file** — that work is
complete and re-asserting it would move no number.

### Genuine scenario gaps (UT2 / `.claude/rules/general-unit-test.md` § Scenario Completeness)

Coverage percentage is explicitly "a supporting metric, not the sole quality gate; untested critical
behavior is not acceptable even if the overall percentage looks good"
(`.claude/rules/general-unit-test.md:27`). The scenario-completeness matrix requires positive,
negative, boundary, and error-handling flows. Measured against that matrix, the existing suite covers
**positive flows only**. The following are real, non-duplicative scenario gaps:

| ID | Gap | Category | Evidence |
| --- | --- | --- | --- |
| **G1** | Constructor accepts `null` silently. `MailItemActionsAdapter.cs:19` is a bare `_mail = mail;` with no guard, so a null argument produces a `NullReferenceException` at some later, unrelated call site rather than at construction. This violates `CLAUDE.md` § C#4.3 ("Validate constructor and method preconditions") and § 3 ("Enforce invariants at construction/initialization time"). | Negative / invalid input | `MailItemActionsAdapter.cs:17–20` |
| **G2** | `Reply()` / `ReplyAll()` / `Forward()` returning `null` from the underlying item is never exercised. F10's `QfcItemController.Navigation.cs:90–103` immediately calls `reply.Display()` on the result, so a null return is a live production concern for the consumer. | Boundary | `QfcItemController.Navigation.cs:88–103` |
| **G3** | No test asserts that a COM failure from the underlying `MailItem` **propagates** rather than being swallowed. Today no `try`/`catch` exists, so propagation is correct; there is no regression guard preventing a future change from adding a silent catch. | Error handling | absence of `catch` in `MailItemActionsAdapter.cs:23–45` |
| **G4** | `Display_ForwardsToUnderlyingMailItem` verifies with `It.IsAny<object>()`, which cannot distinguish a modal from a non-modal display. In a VSTO add-in a modal `Display` blocks the Outlook UI thread; the adapter's omission of the `Modal` argument is a deliberate behavior that no assertion currently pins. | Boundary / behavioral assertion | `MailItemActionsAdapterTests.cs:63` |
| **G5** | `UnRead` is asserted only for `get == true` and `set == false`. The complementary values are never exercised, so a transposed getter/setter (e.g. `get => !_mail.UnRead`) would not necessarily fail. F10 relies on the setter at `QfcItemController.FocusAndTheme.cs:322` (`_mailActions.UnRead = false`). | Boundary | `MailItemActionsAdapterTests.cs:80–84` |
| **G6** | `EntryID` is asserted only for a non-empty string. F10 consumes it at `QfcItemController.MailActions.cs:32` inside `_convOriginID != "" ? _convOriginID : _mailActions.EntryID`, where a null/empty result changes downstream behavior. | Boundary | `MailItemActionsAdapterTests.cs:88–94` |
| **G7** | No contract test asserts the adapter satisfies `IMailItemActions`. The repository has an established precedent for exactly this test on a sibling thin adapter: `QuickFiler.Test\Controllers\WebView2CoreInitializerTests.cs:17–23` (`Construction_YieldsAnIWebViewCoreInitializer`). | Positive / contract | precedent file |

**Framing for the planner:** G1–G7 do not raise the line-coverage number (it is already 100%). They
raise defect-detection strength on a file whose consumers are all F10-owned. If the atomic planner's
budget is constrained, this file should be treated as **already meeting the epic's stated acceptance
criterion** and G1–G7 as quality hardening ranked below the seam work on `KeyboardHandler.cs` (414
lines, currently `[ExcludeFromCodeCoverage]` with zero tests), which is this child's genuine coverage
problem.

### Recommended ledger classification (for F1 to ratify)

**`testable` — already at 100% line coverage, no exemption required, no `[ExcludeFromCodeCoverage]`
attribute present or warranted.**

Tested against the epic's irreducible-remainder standard (Shared Design §1): there is no remainder.
The CLAUDE.md § UT2 qualifier "without an injectable seam" does not apply, because `MailItem` is a
mockable interop interface and the seam is the constructor parameter. Any future attempt to re-apply
`[ExcludeFromCodeCoverage]` to this type must be treated as a **Blocking** finding under the epic's
policy reconciliation — this exact mistake was already made and corrected once (issue #227 cycle-2).

---

## 5. Seam Requirements

### For the file as it stands: none required — it is directly testable

The file is already at the terminal position of the seam hierarchy. Restating the hierarchy from
`.claude/rules/csharp.md:49–54` against this file:

| Level | Seam form | Applied here? | Justification |
| --- | --- | --- | --- |
| 1 (preferred) | **Interface seam** | **Yes — this file exists to realize `IMailItemActions`.** | The narrow `IMailItemActions` interface (7 members, scoped to exactly what `QfcItemController` uses) is the level-1 seam that lets F10's controller be tested with a mock. |
| 2 | Injectable delegate | Not needed | A level-2 `Func<>`/`Action<>` seam would be *lower* priority than the level-1 interface already in place, and would fragment a 7-member cohesive surface into 7 delegates. Explicitly rejected. |
| 3 | Adapter for static/third-party API | **Yes — this file IS the level-3 adapter**, and it is the correct residual: something must eventually touch the real COM object. | The adapter is the thinnest possible wiring layer, which is exactly what `.claude/rules/general-unit-test.md:35` prescribes ("leave only the thinnest possible wiring in the host-bound entry point"). Unusually for a host-bound adapter, even this thinnest layer is fully coverable, because the wrapped type is an interface. |

**No new seam is needed to reach any of G1–G7.** Every proposed test in §7 is reachable with the
existing `Mock<MailItem>` collaborator through the existing constructor parameter. This is the reason
this file scores 100% today.

### For gap G1 only: a two-line production guard (recommended)

G1 is the one gap that cannot be closed by a test alone, because the behavior it targets does not
exist. Two options were evaluated:

- **Option A (RECOMMENDED) — add a fail-fast null guard to the constructor.**
  Replace `_mail = mail;` with a `throw new ArgumentNullException(nameof(mail))` guard followed by the
  assignment. Cover it with test T1 (§7).
  - *Seam level:* not a seam change at all — no signature change, no new type, no new parameter. The
    public constructor shape `MailItemActionsAdapter(MailItem)` is byte-identical.
  - *Behavior-change analysis (against the epic NFR "No behavior change to end-user QuickFiler
    flows"):* the guard is **provably unreachable in production**. The sole production construction
    site is `QuickFiler\Controllers\QfcItemController.Initialization.cs:392–394`:
    ```csharp
    _mailActions ??= mailItem is null
        ? null
        : new QuickFiler.Interfaces.MailItemActionsAdapter(mailItem);
    ```
    The call site already null-checks before constructing, so the guard can never fire on any live
    path. A repository-wide grep for `new MailItemActionsAdapter` returned exactly two hits: that
    line and the test helper at `MailItemActionsAdapterTests.cs:20`. Observable behavior is therefore
    unchanged, and the change converts a latent, deferred `NullReferenceException` into an explicit
    documented contract, satisfying `CLAUDE.md` § C#4.3.
  - *Coverage effect:* adds 2 statement lines and 1 branch; both are covered by T1, so the file
    remains at 100% line and 100% branch coverage.
  - *Cross-child effect:* additive. No sibling-owned file changes.

- **Option B (rejected) — write no production change and add a characterization test asserting that
  `new MailItemActionsAdapter(null)` does not throw.**
  Rejected because it pins behavior the repository's own policy calls a defect (silent acceptance of an
  invalid precondition), making the correct future fix a "breaking" test change. It also leaves the
  deferred-`NullReferenceException` failure mode in place.

If the atomic planner judges any production edit out of budget for a coverage-only child, **Option C —
do nothing to the production file and drop T1** is acceptable: the file already meets every stated
acceptance criterion of issue #430 without it. Option C must be recorded as an explicit deferral, not
an omission.

---

## 6. Cross-Child Contract Impact

### Implementers of / dependents on this type

| Relationship | Site | Owning child |
| --- | --- | --- |
| Implements `IMailItemActions` | `MailItemActionsAdapter.cs:12` (this file) | **F3 (this child)** |
| Sole production construction site | `QuickFiler\Controllers\QfcItemController.Initialization.cs:392–394` | **F10** (`quickfiler-item-controller-coverage`) |
| Test construction site | `QuickFiler.Test\Controllers\MailItemActionsAdapterTests.cs:20` | F3 (test code) |

A repository-wide grep for `MailItemActionsAdapter` found no other production reference. In
particular, F10's controller code consumes the **interface** (`_mailActions` field, declared
`IMailItemActions` at `QfcItemController.cs:68`) at every call site — `EventHandlers.cs:135`,
`FocusAndTheme.cs:322–323`, `MailActions.cs:32,43`, `Navigation.cs:90,96,102` — and never the concrete
adapter type. F10's tests inject `Mock<IMailItemActions>` (`SeamCoreTests.cs:34,40,87,155`;
`SeamDispatcherTests.cs:161,333`; `MailActionsTests.cs:165`) and never touch the adapter.

### Additive-vs-breaking determination

**The recommended change set (Option A guard + §7 tests) is ADDITIVE.** Specifically:

- No public signature changes: the constructor, all 5 methods, and both properties keep identical
  names, parameter lists, and return types.
- No interface change: `IMailItemActions` is untouched (see `09-IMailItemActions.md`).
- No sibling-owned file is edited. The only F10-owned file mentioned, `QfcItemController.Initialization.cs`,
  is read for evidence and **not modified**; it already null-checks, so it needs no adjustment.
- No compile impact on F9 (`EfcItemController`), F14 (`ItemViewer`), or any other child — none of them
  reference this type.
- No change to `coverage.config` or any shared build property file (F1/epic-root-owned).
- One additive edit is required to a **test project** file: `QuickFiler.Test\QuickFiler.Test.csproj`
  needs no change if the new test methods are added to the existing
  `Controllers\MailItemActionsAdapterTests.cs` (already registered at line 148). If a new test file is
  created instead, a new `<Compile Include>` entry is required — see R2 in §8.

**No breaking change is proposed and none is necessary.**

---

## 7. Proposed Test Cases

All tests are MSTest `[TestMethod]`s in the `QuickFiler.Controllers.Tests` namespace, use
`Mock<MailItem>` (Moq) as the sole collaborator and FluentAssertions for assertions, follow
Arrange–Act–Assert, construct no forms, show no popups, touch no UI thread, use no
`Thread.Sleep`/`Task.Delay`/wall-clock wait, create no temporary files, and reach no external service.
Each is individually nameable and becomes its own atomic plan task per the epic's per-file mandate.

**Target test file for all cases:**
`QuickFiler.Test\Controllers\MailItemActionsAdapterTests.cs` (existing; already registered in
`QuickFiler.Test.csproj:148`). Adding to the existing `[TestClass]` keeps one cohesive fixture and
reuses the existing `Build()` helper at lines 17–21. See R2 in §8 for the test-tree-mirroring
deviation this inherits.

**Cross-reference to §3: none of the cases below duplicates an existing test.** The seven existing
methods each assert a positive-path forward; every case below targets a distinct scenario category
(invalid input, boundary value, error handling, or type contract) that §3 marks as unreached.

### Priority 1 — recommended for this child

#### T1 — `Constructor_WithNullMailItem_ThrowsArgumentNullException`
- **Gap:** G1 (invalid input). **Requires** the Option A production guard from §5.
- **Seam/mock needed:** none — no collaborator is constructed.
- **Arrange:** define `System.Action act = () => new MailItemActionsAdapter(null);`
- **Act:** invoke through the assertion.
- **Assert:** `act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mail");`
- **Note:** if the planner selects Option C (no production change), drop T1 and record the deferral.

#### T2 — `Construction_YieldsAnIMailItemActions`
- **Gap:** G7 (type contract). **Precedent:** `WebView2CoreInitializerTests.cs:17–23`.
- **Seam/mock needed:** `Mock<MailItem>`.
- **Arrange:** `var mail = new Mock<MailItem>();`
- **Act:** `IMailItemActions actions = new MailItemActionsAdapter(mail.Object);`
- **Assert:** `actions.Should().NotBeNull(); actions.Should().BeAssignableTo<IMailItemActions>();`

#### T3 — `Reply_WhenUnderlyingMailItemThrows_PropagatesException`
- **Gap:** G3 (error handling). Representative of the whole forwarding family; guards against a future
  silent `catch` being introduced.
- **Seam/mock needed:** `Mock<MailItem>` with a throwing setup.
- **Arrange:** `var (adapter, mail) = Build(); var boom = new InvalidOperationException("com-failure"); mail.Setup(m => m.Reply()).Throws(boom);`
- **Act:** `System.Action act = () => adapter.Reply();`
- **Assert:** `act.Should().Throw<InvalidOperationException>().Which.Should().BeSameAs(boom);`
- **Precedent for the throwing-collaborator shape:** `UtilitiesCS.Test\OutlookObjects\Item\OutlookItemTryTests.cs:253–256`.

#### T4 — `UnRead_Get_ReturnsFalse_WhenUnderlyingMailItemIsRead`
- **Gap:** G5 (boundary; complements the existing `true`-only getter assertion).
- **Seam/mock needed:** `Mock<MailItem>`.
- **Arrange:** `var (adapter, mail) = Build(); mail.SetupGet(m => m.UnRead).Returns(false);`
- **Act:** `bool unread = adapter.UnRead;`
- **Assert:** `unread.Should().BeFalse();`

#### T5 — `UnRead_Set_True_ForwardsToUnderlyingMailItem`
- **Gap:** G5 (boundary; the existing test only sets `false`).
- **Seam/mock needed:** `Mock<MailItem>`.
- **Arrange:** `var (adapter, mail) = Build();`
- **Act:** `adapter.UnRead = true;`
- **Assert:** `mail.VerifySet(m => m.UnRead = true, Times.Once());`

#### T6 — `EntryID_WhenUnderlyingMailItemReturnsNull_ReturnsNull`
- **Gap:** G6 (boundary). Documents the pass-through that F10 depends on at
  `QfcItemController.MailActions.cs:32`.
- **Seam/mock needed:** `Mock<MailItem>`.
- **Arrange:** `var (adapter, mail) = Build(); mail.SetupGet(m => m.EntryID).Returns((string)null);`
- **Act:** `string id = adapter.EntryID;`
- **Assert:** `id.Should().BeNull();`

#### T7 — `Reply_WhenUnderlyingMailItemReturnsNull_ReturnsNull`
- **Gap:** G2 (boundary). Documents the pass-through that F10's
  `QfcItemController.Navigation.cs:90–91` consumes without a null check.
- **Seam/mock needed:** `Mock<MailItem>`.
- **Arrange:** `var (adapter, mail) = Build(); mail.Setup(m => m.Reply()).Returns((MailItem)null);`
- **Act:** `MailItem result = adapter.Reply();`
- **Assert:** `result.Should().BeNull();`

#### T8 — `Display_InvokesUnderlyingMailItemNonModally`
- **Gap:** G4 (boundary / behavioral assertion). The existing test's `It.IsAny<object>()` matcher
  cannot distinguish modal from non-modal.
- **Seam/mock needed:** `Mock<MailItem>` with an argument-capturing callback.
- **Arrange:** `var (adapter, mail) = Build(); object captured = new object(); mail.Setup(m => m.Display(It.IsAny<object>())).Callback<object>(arg => captured = arg);`
- **Act:** `adapter.Display();`
- **Assert:** `captured.Should().NotBe(true);` — i.e. the adapter never requests a modal display.
  Asserting *inequality to boxed `true`* rather than equality to a specific sentinel keeps the test
  robust regardless of whether the omitted COM optional argument materializes as `System.Type.Missing`
  or `null`. See Q1 in §8.

### Priority 2 — optional; add only if the planner wants symmetric error-handling coverage

| # | Proposed method name | Gap | Shape |
| --- | --- | --- | --- |
| T9 | `ReplyAll_WhenUnderlyingMailItemThrows_PropagatesException` | G3 | identical to T3 with `m.ReplyAll()` |
| T10 | `Forward_WhenUnderlyingMailItemThrows_PropagatesException` | G3 | identical to T3 with `m.Forward()` |
| T11 | `Display_WhenUnderlyingMailItemThrows_PropagatesException` | G3 | identical to T3 with `m.Display(It.IsAny<object>())` |
| T12 | `Save_WhenUnderlyingMailItemThrows_PropagatesException` | G3 | identical to T3 with `m.Save()` |

**Count: 12 enumerated, 8 recommended (T1–T8).** T9–T12 are structurally identical to T3 and are
marked optional because the bodies they guard are single expression-bodied forwards with no catch;
T3 alone establishes the regression guard for the pattern.

---

## 8. Risks and Open Questions

| # | Item | Assessment |
| --- | --- | --- |
| R1 | **This file is not where F3's coverage risk lies.** It is at 100%; `KeyboardHandler.cs` (414 lines, `[ExcludeFromCodeCoverage]`, zero tests) is. | Ensure the atomic plan does not spend budget here at the expense of the seam work `issue.md:24–29` identifies as this child's central problem. Treat T1–T8 as hardening. |
| R2 | **Test-tree mirroring deviation (pre-existing).** `.claude/rules/general-unit-test.md:76–80` requires the test tree to mirror the production tree, but the production file lives in `QuickFiler\Interfaces\` while its test lives in `QuickFiler.Test\Controllers\`. `QuickFiler.Test` has no `Interfaces\` folder. | **Recommendation: add the new methods to the existing file and do not move it.** Moving the file would (a) require a `<Compile Include>` path edit in the legacy non-SDK `QuickFiler.Test.csproj`, (b) create a rename-vs-edit merge conflict on the integration branch, and (c) deliver zero coverage benefit. Record the deviation as a pre-existing condition for F16 (capstone) to adjudicate across the whole project rather than fixing it piecemeal in F3. |
| R3 | **`[ExcludeFromCodeCoverage]` regression risk.** This type previously carried a false COM-barrier exemption that was removed during issue #227 cycle-2. | Re-adding it must be treated as Blocking. The evidence that no barrier exists — `MailItem` is a mockable interop interface, and the file measures 100% — is recorded in §2 and §3 for any future auditor. |
| R4 | **Option A's guard adds a branch that must stay covered.** | T1 covers it. If T1 is dropped (Option C), the guard must also be dropped; shipping the guard without T1 would take the file off 100% branch coverage and would itself be a coverage regression on changed lines (`.claude/rules/csharp.md:41`). The two are a single atomic unit. |
| R5 | **Merge-conflict surface with F10.** F10 will be editing `QfcItemController.Initialization.cs` around line 392 during the same wave. | F3 must **read but not write** that file. No F3 change touches it, so the conflict surface is nil provided Option A is confined to `MailItemActionsAdapter.cs`. |
| Q1 | **What value does the omitted COM optional `Modal` argument materialize as at the call site `_mail.Display()`?** For a `tlbimp`-generated `[Optional]` parameter with no `DefaultParameterValue`, the C# compiler substitutes `System.Type.Missing`; for a parameter carrying an explicit default it substitutes that default. This was not verified against the PIA metadata during this research. | T8 is deliberately written as `captured.Should().NotBe(true)` so it is correct under either outcome. If the implementer verifies the actual value (e.g. by asserting once and reading the failure message), the assertion may be tightened to `captured.Should().Be(System.Type.Missing)`. Do not tighten it speculatively. |
| Q2 | **Should `Reply()`/`ReplyAll()`/`Forward()` return `IMailItemActions` instead of the COM `MailItem`?** | That would improve host-neutrality for the long-term VSTO exit (`epic.md:126–129` prefers host-neutral extraction where a seam choice is open), but it is a **BREAKING** change to `IMailItemActions` and to F10-owned call sites at `QfcItemController.Navigation.cs:90,96,102`, which call `reply.Display()` on the result. Out of scope for F3; record as a migration observation only. |
| Q3 | **Will F1's harness attribute this file's coverage by filename or by class name?** The committed Cobertura reports show both relative (`QuickFiler\Interfaces\MailItemActionsAdapter.cs`) and absolute (`C:\...\QuickFiler\Interfaces\MailItemActionsAdapter.cs`) `filename` attributes depending on the run — compare `coverage-final.cobertura.xml:14448` with `coverage-baseline.cobertura.xml:14529` in the same #424 evidence folder. | F3 should confirm F1's harness normalizes paths before attributing per-file results; otherwise the same file can appear twice or be missed. Report as a defect to F1 if observed. |

---

## 9. Sources

| Source | Lines cited |
| --- | --- |
| `QuickFiler\Interfaces\MailItemActionsAdapter.cs` | 1–47 (read in full) |
| `QuickFiler\Interfaces\IMailItemActions.cs` | 1–35 (read in full) |
| `QuickFiler.Test\Controllers\MailItemActionsAdapterTests.cs` | 1–96 (read in full); esp. 17–21 (`Build()`), 23–94 (the 7 existing test methods), 63 (`It.IsAny<object>()` matcher), 80–84, 88–94 |
| `QuickFiler.Test\Controllers\WebView2CoreInitializerTests.cs` | 1–25 (read in full) — precedent for the T2 contract test |
| `QuickFiler.Test\QuickFiler.Test.csproj` | 148 (`<Compile Include="Controllers\MailItemActionsAdapterTests.cs" />`), 92–96 (Ka*/Kbd* registrations, showing the explicit-include convention) |
| `QuickFiler\QuickFiler.csproj` | 14 (`<LangVersion>preview</LangVersion>`), 368 (`<Compile Include="Interfaces\MailItemActionsAdapter.cs" />`) |
| `QuickFiler\Controllers\QfcItemController.cs` | 25–29, 62–68 (seam field declarations) |
| `QuickFiler\Controllers\QfcItemController.Initialization.cs` | 40, 59, 375–398 (sole production construction site, incl. the pre-existing null check at 392–394) |
| `QuickFiler\Controllers\QfcItemController.EventHandlers.cs` | 135 |
| `QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs` | 322–323 |
| `QuickFiler\Controllers\QfcItemController.MailActions.cs` | 32, 43 |
| `QuickFiler\Controllers\QfcItemController.Navigation.cs` | 88–103 |
| `QuickFiler.Test\Controllers\QfcItemController.SeamCoreTests.cs` | 15, 17, 34, 40, 87, 155, 161 |
| `QuickFiler.Test\Controllers\QfcItemController.SeamDispatcherTests.cs` | 161, 333 |
| `QuickFiler.Test\Controllers\QfcItemController.MailActionsTests.cs` | 140, 160, 165 |
| `UtilitiesCS.Test\OutlookObjects\Item\OutlookItemTryTests.cs` | 253–256 — precedent for the throwing-collaborator setup used by T3 |
| `docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\qa-gates\coverage-final.cobertura.xml` | 14448–14513 (100% class entry, all 9 methods, all 12 lines) |
| `docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\baseline\coverage-baseline.cobertura.xml` | 14529 (absolute-path `filename` variant, cited for Q3) |
| `docs\features\epics\quickfiler-per-file-coverage\epic.md` | 1–419 (read in full); esp. 126–129, 132–192, 267–274 |
| `docs\features\active\2026-08-07-quickfiler-keyboard-actions-coverage-430\issue.md` | 1–95 (read in full); esp. 24–29, 36–46, 63–79 |
| `.claude\rules\general-unit-test.md` | 21–29, 31–46, 48–57, 59–67, 69–74, 76–80 |
| `.claude\rules\csharp.md` | 31–41 (Testing Standards, coverage floors), 47–54 (DI seam hierarchy) |
| `.claude\agent-memory\task-researcher\feedback_exemption_audit_check_proven_techniques.md` | 13–20 (recorded finding that this type's prior COM-barrier exemption was false) |
| `CLAUDE.md` | § UT2 COM/VSTO/WinForms coverage exemption; § C#4.3 (constructor precondition validation); § CUT1–CUT3 |
