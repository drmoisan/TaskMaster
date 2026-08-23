# quickfiler-keyboard-action-contract-defects (Spec)

- **Issue:** #445
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-21T18-45
- **Status:** Approved
- **Version:** 1.0

## Context

Three related contract defects in the QuickFiler keyboard-action types (`KaStringAsync`, `KaChar`,
`KaKey`, and the `IKbdAction<T, U>` interface). The defects were first recorded in `issue.md` against
base commit `56ca1cea` and were re-verified line by line in this worktree on 2026-08-21 by the
research artifact
`docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/research/keyboard-action-contract-defects.2026-08-21T18-20.md`.
Where the two disagree, the research artifact is authoritative; its corrections are carried into this
spec.

The three defects are:

1. **Inconsistent `Activated` gating in `KaStringAsync.KeyEquals`.** Branches 1 and 2 gate their side
   effects on `Activated`; branch 3 (`other.Length > 1`) invokes `Update` without the gate.
2. **`KeyEquals("")` has no defined contract.** `Key.Contains("")` is `true` for every string, so an
   empty probe enters branch 1 and, when `Activated && Update is not null`, evaluates
   `Key.Substring(-1, 1)` and throws `ArgumentOutOfRangeException`. When the guard is false it
   silently returns `true`, so an empty probe "matches" every registered action.
3. **`DelegateType` reports the wrong type on `KaChar`.** `KaChar` stores an `Action<char>` but
   `DelegateType` returns `typeof(Action<Keys>)`. `DelegateType` and a dead `Update` property are
   orphaned public API on several implementers, and the corresponding interface members are commented
   out in `IKbdAction.cs`.

Work mode for this issue is `full-bug`. This `spec.md` is the sole authoritative acceptance-criteria
source; no `user-story.md` exists or will be created.

### Correction carried from research — no characterization tests exist

`issue.md` states (section "Why these were not fixed in issue #430") that #430's tests characterize
the current defective behavior and that this work must "replace the characterization tests added by
#430". **That premise is false and is superseded by the research artifact.** No committed test
asserts any of the three defects:

- `KeyEquals_MultiCharNonMatch_InvokesUpdateWithFirstCharAndReturnsFalse`
  (`QuickFiler.Test/Controllers/KaStringAsyncTests.cs:133-152`) sets `ka.Activated = true` at `:141`,
  so it exercises branch 3 with the gate **satisfied**. It does not distinguish gated from ungated
  and passes unchanged after the fix. Its name implies coverage of the ungated case that it does not
  provide, so it is renamed.
- No committed test passes an empty string to `KeyEquals`.
- No committed test references `DelegateType`.

Consequences for this work: nothing needs replacing or deleting; both regression tests are authored
fresh; each will be genuinely red before the fix and green after; and no fail-before exception
dossier is required, because real red-before-green runs are available.

## Repro & Evidence

- **Steps to reproduce (with data/flags/inputs):**
  - *Defect 1 (ungated branch 3).* Construct `new KaStringAsync("src", "abc", func, update, toggle)`
    with a non-null `update` callback. Leave `Activated` at its default `false`
    (`KaStringAsync.cs:50`). Call `KeyEquals("zz")`. The `Update` callback is invoked with `"a"`
    despite `Activated` being `false`, because the branch-3 guard at `KaStringAsync.cs:72` reads
    `if (Update is not null)` with no `Activated` conjunct.
  - *Defect 2 (empty probe).* On the same instance, set `Activated = true` and call `KeyEquals("")`.
    `Key.Contains("")` is `true`, so control enters branch 1 and `KaStringAsync.cs:62` evaluates
    `Key.Substring(-1, 1)`, which throws `ArgumentOutOfRangeException`. With `Activated == false` or
    `Update == null`, the same call returns `true` without throwing, so
    `KbdActions<string, KaStringAsync, ...>.FilterKeys("")` returns every registered action and
    `Find("")` throws `InvalidOperationException` from `KbdActions.cs:67` whenever two or more
    actions are registered.
  - *Defect 3 (`DelegateType`).* Read `KaChar.cs:11` (`KaChar : IKbdAction<char, Action<char>>`),
    `KaChar.cs:37` (`public Action<char> Delegate`), and `KaChar.cs:43-46`
    (`DelegateType => typeof(Action<Keys>)`). The reported type does not match the stored delegate
    type.
- **Expected vs actual behavior:**
  - Defect 1 — Expected: no `KeyEquals` side effect fires while `Activated` is `false`. Actual:
    branch 3's `Update` fires regardless of `Activated`.
  - Defect 2 — Expected: an empty probe is either rejected explicitly or defined to match nothing.
    Actual: undefined; either a low-level `ArgumentOutOfRangeException` or a silent
    "matches-everything" result depending on unrelated state.
  - Defect 3 — Expected: a type-reporting member reports the stored delegate type, or does not
    exist. Actual: `KaChar.DelegateType` reports `Action<Keys>` for a stored `Action<char>`.
- **Logs/screenshots/error snippets:** none. All three defects are established by direct file read,
  not by a captured runtime failure. Defect 2's exception was reproduced by reading the argument
  arithmetic (`other.Length - 1 == -1`), not observed in a running session.
- **Frequency / determinism (always, intermittent, data-dependent):** All three are deterministic
  functions of the arguments and of instance state; none is timing- or data-dependent. All three are
  currently **latent in production**: the only five-argument construction site,
  `QuickFiler/Controllers/QfcCollectionController.cs:1376-1383`, passes `null` for both `update`
  (`:1381`) and `toggleControl` (`:1382`), and the other registration path,
  `KbdActions.Add(string, TKey, VDelegate)`, builds its element with `UClass instance = new()` at
  `KbdActions.cs:99` — the parameterless constructor at `KaStringAsync.cs:12`, which assigns neither
  callback. Every `Update is not null` and `ToggleControl is not null` guard in `KeyEquals` therefore
  evaluates `false` on every production evaluation today.

## Scope & Non-Goals

- **In scope:**
  - `QuickFiler/Controllers/KaStringAsync.cs` — apply the `Activated` gate to branch 3; add a
    fail-fast argument guard for `null` and empty `other`; add an XML doc comment recording both
    contracts.
  - `QuickFiler/Controllers/KaChar.cs` — delete `DelegateType`; delete the dead `Update` property
    from `KaChar` and `KaCharAsync`; delete the then-unused `using System.Windows.Forms;`.
  - `QuickFiler/Controllers/KaKey.cs` — delete `DelegateType`; delete the dead `Update` property from
    `KaKey` and `KaKeyAsync`.
  - `QuickFiler/Interfaces/IKbdAction.cs` — delete the two commented-out members at `:15-16`.
  - `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` — rename one existing test; add the four new
    tests described in **Test Strategy**.
- **Out of scope / non-goals:**
  - **A fourth latent defect at `KaStringAsync.cs:62` is deliberately excluded.** Branch 1's guard is
    `Key.Contains(other)` — a substring test — but its argument `Key.Substring(other.Length - 1, 1)`
    is only meaningful when `other` is a **prefix** of `Key`. For `Key = "abc"` and `other = "b"`,
    `Contains` is `true` and the expression yields `"a"`, which is neither the matched character nor
    the following one. It is reachable in principle whenever the digit width is 2 (registered keys
    `"01"`..`"12"`; typing `"1"` matches `"01"` at index 1, not as a prefix). It has no observable
    effect today because `Update` is null in production. Fixing it correctly requires deciding
    whether branch 1 should test `StartsWith` instead of `Contains` — a keyboard-filtering behavior
    change that `QuickFiler.Test/Controllers/KbdActionsTests.cs:71-76` currently pins to substring
    semantics. Per CLAUDE.md Bugfix Workflow section 2, it is recorded and promoted to a new issue
    rather than widening this bugfix. The existing assertion at `KaStringAsyncTests.cs:89-91`
    (`.Be("b")` for `Key = "abc"`, `other = "ab"`) pins the prefix case and is left exactly as-is.
  - `QuickFiler/Controllers/KbdActions.cs` — not modified. Its LINQ re-enumeration behavior is
    diagnostic input to the gating decision, not a target of this fix; parts of this file are owned
    by other issues in a later epic.
  - `QuickFiler/Controllers/KeyboardHandler.cs` — not modified. It is `[ExcludeFromCodeCoverage]` at
    `KeyboardHandler.cs:22` and is read only to establish call-order and probe-length facts.
  - `QuickFiler/Controllers/QfcCollectionController.cs` — not modified. At 2349 lines it is a
    pre-existing violation of the 500-line rule; reading `:1363-1385` is sufficient and this work
    does not touch it or attempt to remediate its size.
  - `QuickFiler.Test/QuickFiler.Test.csproj` — not modified. All five relevant test files already
    carry `<Compile Include>` entries at `:96-100`, and a sibling epic child owns this file.
  - No change to the `Contains`-based matching semantics of `KeyEquals`.
  - No coverage exemption, no `coverage.config` change, no `[ExcludeFromCodeCoverage]` addition.
- **Explicitly excluded systems, integrations, or datasets:**
  - No file under `.claude/**` is edited. Rule and policy files are cited as the standard this fix is
    measured against, never as edit targets.
  - No file under `docs/features/potential/**` is written by this work.
  - No Outlook, COM, or Microsoft Graph interaction. The changed types are pure value objects.
  - There is no Python toolchain in this repository (no `scripts/dev_tools/`, no Poetry manifest), so
    any step naming `poetry run python -m scripts.dev_tools.*` is unrunnable by absence and must be
    reported as such rather than executed or simulated.

## Root Cause Analysis

- **Current hypothesis or confirmed root cause:**
  - *Defect 1 — confirmed.* `KaStringAsync.cs:72` omits the `Activated &&` conjunct that `:61` and
    `:67` and `:74` all carry. There is no comment explaining the omission and no test pinning it, so
    it is treated as an omission rather than an intentional asymmetry.
  - *Defect 2 — confirmed.* `KeyEquals` has no argument validation. `string.Contains("")` returning
    `true` for every receiver makes the empty probe enter branch 1, where the offset arithmetic
    `other.Length - 1` becomes `-1`.
  - *Defect 3 — confirmed.* `KaChar.DelegateType` was written with the same body as
    `KaKey.DelegateType` (`typeof(Action<Keys>)`), which is correct for `KaKey` and wrong for
    `KaChar`. Because `DelegateType` is not on `IKbdAction`, the compiler never cross-checks it.
- **Signals/evidence supporting it:**
  - **Invocation-count evidence for defect 1.** `KbdActions.Find` (`KbdActions.cs:53-69`) builds a
    deferred `Where` query at `:55` and then re-enumerates it: `.Count()` at `:56`, then `.First()`
    at `:62`. `Enumerable.Where` returns an iterator with no `ICollection<T>` fast path, so `Count()`
    walks the whole sequence. A single `Find` therefore invokes `KeyEquals` roughly `N + k + 1` times
    for a list of `N` elements whose first match is at 0-based index `k`. Because branch 1 returns at
    `KaStringAsync.cs:63` **before** the `Activated = false` reset at `:77`, the gated side effects
    are self-limiting to one invocation per non-matching element per keystroke, while the ungated
    `Update` at `:73` fires once per enumeration pass. The ungated call's invocation count is
    therefore determined by a LINQ implementation detail inside a different class rather than by user
    intent.
  - **Ownership evidence for defect 3.** A repository-wide search over `*.cs` for `DelegateType`
    returns exactly three hits, none of them a read: the comment at `IKbdAction.cs:16` and the two
    declarations at `KaChar.cs:43` and `KaKey.cs:43`. A repository-wide search for `Update` shows it
    declared on five types but read only at `KaStringAsync.cs:61,62,72,73` and written only at
    `KaStringAsync.cs:25`. On `KaChar`, `KaCharAsync`, `KaKey`, and `KaKeyAsync` it is
    write-never/read-never dead API.
  - **Reachability evidence.** `KeyboardHandler.cs:180` appends to the filter before every probe, so
    every probe at `:181`, `:188`, and `:194` has length `>= 1`. No production caller can pass an
    empty string. `Activated` is re-armed for all elements only when the filter length is 1
    (`KeyboardHandler.cs:186-187`), after the `ContainsKey` pass at `:181` has already run.
- **Affected components/modules (paths, services, pipelines):**
  - `QuickFiler/Controllers/KaStringAsync.cs` (95 lines)
  - `QuickFiler/Controllers/KaChar.cs` (99 lines)
  - `QuickFiler/Controllers/KaKey.cs` (99 lines)
  - `QuickFiler/Interfaces/IKbdAction.cs` (18 lines)
  - `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` (168 lines)
  - Read-only context: `QuickFiler/Controllers/KbdActions.cs`,
    `QuickFiler/Controllers/KeyboardHandler.cs`,
    `QuickFiler/Controllers/QfcCollectionController.cs`.

## Proposed Fix

### Design summary (what changes where):

Three decisions are fixed and are not open for re-litigation during implementation.

**Decision 1 — the `Activated`-gating contract: gate all three branches.**
Add the `Activated &&` conjunct to the branch-3 guard at `KaStringAsync.cs:72`, so that all three
branches of `KeyEquals` gate uniformly. The contract, to be recorded verbatim in an XML doc comment
on `KeyEquals`, is:

> `Activated` is a per-keystroke latch. Every observable side effect of `KeyEquals` — both `Update`
> and `ToggleControl` — fires only while `Activated` is true. A matching probe (branch 1)
> deliberately does NOT clear the latch; a non-matching probe clears it at `KaStringAsync.cs:77`.
> Consequently each element's side effects fire at most once per keystroke, regardless of how many
> times a LINQ predicate is re-enumerated.

*Precision note (does not alter the decision).* The "at most once per keystroke" limit is exact for
the non-matching branches (2 and 3), whose element clears the latch at `:77` on its first pass. A
**matching** element takes branch 1 and returns at `:63` without clearing the latch, by design (see
the invariant below), so its idempotent `Update` may be re-executed on later enumeration passes
within the same keystroke. That repetition is intentional and load-bearing; it is what allows the
label to advance across the three passes of a single keystroke.

*Rationale.* Gating branch 3 makes every non-matching element's side-effect count independent of LINQ
re-enumeration. Today the ungated `Update` fires once per enumeration pass — a count set by `Find`'s
internal `Count()`-then-`First()` sequence, not by user intent — and this is harmless only because
the callback happens to be an idempotent assignment, a property no signature enforces and no test
asserts.

*Counter-argument, recorded fairly.* `ToggleControl` is non-idempotent — invoking it twice restores
the original state — and therefore genuinely must be latched, whereas `Update` is an idempotent
assignment. `Activated` may therefore have been intended to latch only the toggle, which would argue
for Option B: ungate `Update` in every branch and let only `ToggleControl` consult `Activated`.
Option B is **rejected** because it leaves the matching row's `Update` count re-enumeration-dependent
and because it inverts the currently-passing test
`KeyEquals_ContainsMatchWhileNotActivated_ReturnsTrueWithoutUpdate`
(`KaStringAsyncTests.cs:98-114`), turning a passing test into a contradiction.

*Production impact: none.* Both callbacks are `null` on every `KaStringAsync` instance production
creates (`QfcCollectionController.cs:1376-1383` passes `null`/`null`; `KbdActions.cs:99` uses the
parameterless constructor), so `Update is not null` evaluates `false` on every production evaluation.
The change is observable only in tests and in any future code that supplies a non-null `Update`.

**Decision 2 — the empty-argument contract: explicit fail-fast guard.**
Add a guard clause at the **top** of `KeyEquals`, before the `Key.Contains(other)` test:

- `other == null` → `throw new ArgumentNullException(nameof(other))`.
- `other` empty (`string.Empty`) → `throw new ArgumentException(...)` with a documented message
  explaining that an empty probe would otherwise match every registered action, and with the
  parameter name supplied.

*Rationale.* The reachable production misbehaviour today is not the exception but the silent
"empty matches everything" semantics, because production always has `Update == null`. Those semantics
make `FilterKeys("")` return every registered action and make `Find("")` throw
`InvalidOperationException` from `KbdActions.cs:67`. No production caller can pass an empty string
(`KeyboardHandler.cs:180` appends before every probe), so no caller breaks. The guard satisfies the
second limb of the issue's own acceptance criterion ("or rejects it with an explicit, documented
argument exception") and matches CLAUDE.md General Code Change Policy section 3 and C#4.1,
"fail fast and explicitly".

*Fallback, recorded.* Option 1 — early-return `false` for an empty probe — is acceptable if a
reviewer prefers a total, non-throwing predicate. Both options are neutral against the existing
suite, since no committed test passes an empty string. The acceptance criteria below encode the
fail-fast option; adopting the fallback would require amending this spec first.

**Decision 3 — `DelegateType` and the commented-out members: remove, do not restore.**

- Delete `DelegateType` from `KaChar.cs:43-46` and `KaKey.cs:43-46`. Zero read sites exist
  repository-wide.
- Delete the dead `Update` property (and its backing field) from `KaChar` (`KaChar.cs:50-55`),
  `KaCharAsync` (`KaChar.cs:92-97`), `KaKey` (`KaKey.cs:50-55`), and `KaKeyAsync`
  (`KaKey.cs:92-97`).
- **Retain `Update` on `KaStringAsync` (`KaStringAsync.cs:81-86`).** It is genuinely read at `:61`,
  `:62`, `:72`, `:73` and written by the five-argument constructor at `:25`.
- Delete both commented-out lines at `IKbdAction.cs:15-16`.
- Remove the then-unused `using System.Windows.Forms;` at `KaChar.cs:6`. `Keys` appears in
  `KaChar.cs` only inside `DelegateType` (`:45`). `KaKey.cs` keeps its `using System.Windows.Forms;`
  because `Keys` is its key type throughout.

*Rationale.* Restoring `DelegateType` to `IKbdAction` **will not compile**: `KaStringAsync`
(`KaStringAsync.cs:10`), `KaCharAsync` (`KaChar.cs:58`), and `KaKeyAsync` (`KaKey.cs:58`) do not
declare it. Restoring `Update` to the interface would force all five implementers to keep a member
that is dead on four of them. Removal resolves defect 3 without requiring a decision on the correct
`DelegateType` value.

*Public-API notice (CLAUDE.md General Code Change Policy section 7.2).* Deleting `DelegateType` from
two public classes and `Update` from four public classes is a **breaking change to the public API
surface** of `QuickFiler.Controllers`. All in-repo consumers were enumerated by repository-wide
search over `*.cs`: `DelegateType` has zero read sites and `Update` has read sites only inside
`KaStringAsync` itself. The removal is called out explicitly here so that the change description and
the pull-request body carry the same notice.

### Boundaries and invariants to preserve:

- **HARD CONSTRAINT — do not make branch 1 fall through to the `Activated = false` reset at
  `KaStringAsync.cs:77` for symmetry.** The early `return true` at `KaStringAsync.cs:63` is
  load-bearing and must be preserved verbatim. `KeyboardHandler` re-arms `Activated` only at filter
  length 1 (`KeyboardHandler.cs:186-187`) and then performs three passes within one keystroke:
  `ContainsKey` (`:181`), `FilterKeys` (`:188`), and the indexer/`Find` (`:194`). If branch 1 cleared
  the latch, the `ContainsKey` pass would consume the activation and the label advance would stop.
  This constraint is encoded as an explicit anti-regression acceptance criterion.
- The `Contains`-based matching semantics of branch 1 are unchanged. `KbdActionsTests.cs:71-76` pins
  substring matching and must continue to pass without modification.
- The offset expression at `KaStringAsync.cs:62`, `Key.Substring(other.Length - 1, 1)`, is unchanged.
  The out-of-scope fourth defect concerns that expression and is deferred to a follow-up issue.
- Branch 2 (`other.Length == 1`) legitimately invokes no `Update`. That omission is correct under the
  reconstructed design intent — a row that has never matched is already displaying `Key[0]` — and is
  not a fourth inconsistency to "fix".
- `KaStringAsync.Activated` remains a public settable property. `KeyboardHandler.cs:187` is the only
  external write site in the repository and is not modified.
- `IKbdAction<T, U>`'s four live members at `IKbdAction.cs:11-14` are unchanged. No member is added
  to the interface, so no implementer signature changes.
- Test-file and production-file sizes remain under the 500-line cap
  (`.claude/rules/general-code-change.md`, "File Size Limit").

### Dependencies or blocked work:

- No external dependency, package, or service. No new NuGet reference.
- `QuickFiler.Test/QuickFiler.Test.csproj` is owned by a sibling epic child and must not be edited.
  Because every new test lands in an existing file, no `<Compile Include>` entry is needed.
- `QuickFiler/Controllers/KbdActions.cs` is partly owned by other issues in a later epic and is not
  modified here.
- The out-of-scope fourth defect must be filed as a new GitHub issue before this work is considered
  complete (see **Rollout & Follow-up**).

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

| # | File | Change | Defect |
|---|---|---|---|
| 1 | `QuickFiler/Controllers/KaStringAsync.cs` (branch-3 guard, `:72`) | add the `Activated &&` conjunct | 1 |
| 2 | `QuickFiler/Controllers/KaStringAsync.cs` (top of `KeyEquals`, `:57`) | add the null/empty guard clause | 2 |
| 3 | `QuickFiler/Controllers/KaStringAsync.cs` (above `KeyEquals`) | add the XML doc comment recording both contracts | 1, 2 |
| 4 | `QuickFiler/Controllers/KaChar.cs:43-46` | delete `DelegateType`; delete `using System.Windows.Forms;` at `:6` | 3 |
| 5 | `QuickFiler/Controllers/KaChar.cs:50-55`, `:92-97` | delete the dead `Update` property and backing field from `KaChar` and `KaCharAsync` | related |
| 6 | `QuickFiler/Controllers/KaKey.cs:43-46` | delete `DelegateType` | 3 |
| 7 | `QuickFiler/Controllers/KaKey.cs:50-55`, `:92-97` | delete the dead `Update` property and backing field from `KaKey` and `KaKeyAsync` | related |
| 8 | `QuickFiler/Interfaces/IKbdAction.cs:15-16` | delete both commented-out members | related |
| 9 | `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | rename one test (`:134`); add four new tests | 1, 2 |

All line numbers are as read in this worktree on 2026-08-21 and will shift as edits are applied.

#### Functions/classes/CLI commands impacted:

- `KaStringAsync.KeyEquals(string)` — behavior and contract change (gating, argument validation,
  documentation).
- `KaChar.DelegateType`, `KaKey.DelegateType` — deleted.
- `KaChar.Update`, `KaCharAsync.Update`, `KaKey.Update`, `KaKeyAsync.Update` — deleted.
- `KaStringAsync.Update` — retained unchanged.
- `IKbdAction<T, U>` — two comment lines deleted; live surface unchanged.
- No CLI command exists for this component.

#### Data flow and validation changes:

- `KeyEquals` gains an input-validation stage that runs before any matching logic. The validated
  precondition is: `other` is non-null and non-empty.
- No persisted data, serialization format, or wire format is involved. `KaStringAsync`,
  `KaChar`, and `KaKey` are in-memory value objects.
- Downstream `KbdActions<TKey, UClass, VDelegate>` methods (`ContainsKey`, `FilterKeys`, `Find`,
  `FindIndex`, and the indexer) inherit the new precondition when `TKey` is `string`: an empty key
  argument now surfaces an `ArgumentException` from the predicate rather than matching every element.
  This is a deliberate consequence and is documented in the XML comment.

#### Error handling and logging updates:

- Two explicit exceptions are added at the `KeyEquals` boundary: `ArgumentNullException` for `null`
  and `ArgumentException` for empty. Both are thrown from the guard clause, not from library
  internals, so the exception origin names the offending parameter.
- No logging is added. `KaStringAsync` has no logger and introducing one would exceed the minimal
  scope of a bugfix. `KbdActions` already logs its own duplicate-key and multiple-match conditions
  via log4net (`KbdActions.cs:17-19`, `:85`, `:96`, `:117`) and is unchanged.
- No broad `catch` is introduced anywhere.

#### Rollback/feature-flag considerations (if applicable):

- No feature flag. The change is a small, self-contained source edit; rollback is a revert of the
  pull request.
- Rollback risk is low because both callbacks are `null` in production today, so the gating change
  and the guard clause are unobservable through the shipping code path.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

`bool KaStringAsync.KeyEquals(string other)`

| Input | Precondition | Result | Side effects |
|---|---|---|---|
| `other == null` | violated | throws `ArgumentNullException(nameof(other))` | none |
| `other == string.Empty` | violated | throws `ArgumentException` naming `other` | none |
| `Key.Contains(other)` (branch 1) | satisfied | `true`, returned at `:63` without clearing `Activated` | `Update(Key.Substring(other.Length - 1, 1))` when `Activated && Update is not null` |
| non-match, `other.Length == 1` (branch 2) | satisfied | `false`; `Activated` cleared | `ToggleControl()` when `Activated && ToggleControl is not null` |
| non-match, `other.Length > 1` (branch 3) | satisfied | `false`; `Activated` cleared | `Update(Key.Substring(0, 1))` **and** `ToggleControl()`, each when `Activated` and the respective callback is non-null |

`Key` is normalized to lower case by both the constructor (`:23`) and the setter (`:40`); that
behavior is unchanged.

#### Required configuration keys and defaults:

None. This component reads no configuration. `coverage.config` is not modified.

#### Backward-compatibility expectations:

- **Source-breaking for external consumers of the removed members.** `KaChar.DelegateType`,
  `KaKey.DelegateType`, and the `Update` property on `KaChar`, `KaCharAsync`, `KaKey`, and
  `KaKeyAsync` are deleted. No in-repo consumer reads any of them; a repository-wide search over
  `*.cs` established this. There is no published external consumer of these types.
- **Behavior-compatible in production.** Because `Update` and `ToggleControl` are `null` on every
  production instance, neither the gating change nor the empty-argument guard alters any observable
  QuickFiler keyboard flow today.
- `IKbdAction<T, U>`'s live contract is unchanged, so no implementer outside the four listed files is
  affected.

#### Performance constraints (latency/throughput/memory):

No performance requirement applies and none is introduced. The gating change adds one boolean test on
a branch that already evaluates a null check; the guard clause adds one null test and one length test
per call. `KeyEquals` is invoked on the order of `N + k + 1` times per `Find` for small `N` (the
number of visible QuickFiler rows), executed on a keystroke. No allocation is added.

## Assumptions, Constraints, Dependencies

- **Assumptions (environment, data, access):**
  - The full C# toolchain is available on the executing machine: `dotnet` with the manifest-pinned
    CSharpier 1.2.6, `msbuild`, and `vstest.console.exe`.
  - `dotnet tool restore` has been run once for this worktree before the first CSharpier invocation.
  - `KaStringAsync.Key` is non-empty in production. `GenerateStringKbdAction`
    (`QfcCollectionController.cs:1363-1385`) assigns from a digit width of 1 or 2, so `:1369` or
    `:1373` always assigns and the `key = ""` initialization at `:1366` is never observed.
  - Existing committed tests are treated as part of the spec (CLAUDE.md General Code Change Policy
    section 7.3); the only permitted change to them is the single rename recorded below.
- **Constraints (budget, performance, compatibility):**
  - No file may exceed 500 lines. Post-change estimates: `KaStringAsync.cs` about 110 lines,
    `KaChar.cs` about 88, `KaKey.cs` about 90, `IKbdAction.cs` 16, `KaStringAsyncTests.cs` about 225.
    All are far under the cap; three production files shrink.
  - `QuickFiler.Test/QuickFiler.Test.csproj` must not be edited.
  - No file under `.claude/**` or `docs/features/potential/**` may be written by this work.
  - Evidence artifacts are written only under
    `docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/<kind>/`,
    with `yyyy-MM-ddTHH-mm` timestamps.
- **External dependencies (services, libraries, releases):**
  - Test libraries already referenced by `QuickFiler.Test`: MSTest, Moq, FluentAssertions. No new
    package is added.
  - `System.Interactive` 7.0.1 supplies `EnumerableEx.ForEach` used at `KeyboardHandler.cs:187`
    (`QuickFiler/packages.config:59`). It is read-only context; no version change.

## Data / API / Config Impact

- **User-facing or API changes:** No user-facing change. The public C# API of
  `QuickFiler.Controllers` loses six members (`DelegateType` on two types, `Update` on four types) and
  `KeyEquals` gains two documented argument exceptions. See the public-API notice above.
- **Data or migration considerations:** None. No persisted state, no schema, no migration.
- **Logging/telemetry updates (if any):** None. No logging statement is added, removed, or changed.
- **Compatibility notes (CLI flags, config schemas, versioning):** No CLI flag, no config schema, no
  version bump. `coverage.config` is unchanged and continues to exclude none of the five in-scope
  files; its only `<ModulePaths><Exclude>` block (`:14-20`) lists seven third-party module patterns.

## Test Strategy

- **Framework:** MSTest (`[TestClass]` / `[TestMethod]` from
  `Microsoft.VisualStudio.TestTools.UnitTesting`), Moq where a mock is warranted, and
  FluentAssertions for assertions, per CLAUDE.md CUT1 and CUT2. The scaffold template that this
  document replaces named "pytest"; that was a template artifact and is corrected here. There is no
  Python toolchain in this repository.

- **Regression tests to add or update:**
  - **Rename (one existing test).** `KeyEquals_MultiCharNonMatch_InvokesUpdateWithFirstCharAndReturnsFalse`
    (`QuickFiler.Test/Controllers/KaStringAsyncTests.cs:134`) becomes
    `KeyEquals_MultiCharNonMatchWhileActivated_InvokesUpdateWithFirstCharAndReturnsFalse`. The test
    body is unchanged: it sets `ka.Activated = true` at `:141` and therefore exercises branch 3 with
    the gate satisfied, passing unchanged after the fix. The rename removes the false implication
    that it covers the ungated case.
  - **New test (a) — defect 1 regression, ungated branch 3.** Arrange a `KaStringAsync` with
    `Key = "abc"` and a non-null `Update` callback, leaving `Activated` at `false`. Act:
    `KeyEquals("zz")`. Assert `Update` was **not** invoked and the result is `false`. This test is
    red before the fix and green after.
  - **New test (b) — latch survives the match-to-non-match transition.** A row that matches at depth
    1 and then fails at depth 2 still receives its `Key[0]` reset, because branch 1 returns without
    clearing `Activated`. This pins the reasoning behind the anti-regression invariant and would fail
    if branch 1's early return were removed.
  - **New test (c) — defect 2, empty probe.** `KeyEquals("")` throws `ArgumentException`. Include the
    `Activated = true` / non-null `Update` variant so that the previous
    `ArgumentOutOfRangeException` path is explicitly closed.
  - **New test (d) — defect 2, null probe.** `KeyEquals(null)` throws `ArgumentNullException`.
  - All new tests live in `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` (168 lines today,
    growing to roughly 225 — comfortably under the 500-line cap). Reuse the existing `NewKa` helper at
    `:20-25`, which already supplies optional `update` and `toggle` callbacks.

- **Unit tests (MSTest) for the fixed behavior and boundaries:** For `KeyEquals`, cover each of the
  three branches at both `Activated` states and at both null and non-null `Update`, plus the empty
  and null argument boundaries. That is the matrix the committed suite leaves half-covered. The
  deletions in changes 4 through 8 need no new test; their safety is established by the zero-read-site
  evidence and is proven by the analyzer and nullable builds compiling.

- **Edge cases and negative scenarios (invalid inputs, missing data, boundary values):**
  `other == null`; `other == string.Empty`; `other` of length 1 matching and non-matching; `other`
  of length greater than 1 matching and non-matching; `Update` null and non-null; `ToggleControl`
  null and non-null; `Activated` true and false. The existing test
  `KeyEquals_NullDelegatesAreToleratedInNonMatchBranches` (`:154-166`) already covers the
  both-callbacks-null case and passes unchanged.

- **Error handling and logging verification:** Assert the exception **type** and that the thrown
  `ArgumentException` names the `other` parameter, using FluentAssertions' `Should().Throw<T>()`.
  Assert against the parameter name rather than the full message text so that message wording can be
  refined without breaking the test. No logging assertion is required because no logging is added.

- **Bugfix ordering:** Per CLAUDE.md Bugfix Workflow section 1, author the regression tests first and
  observe them fail before the production edit. Both defect-1 and defect-2 regression tests will be
  genuinely red, so no fail-before exception dossier is required. Record the red run and the
  subsequent green run under
  `docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/qa-gates/`
  using `yyyy-MM-ddTHH-mm` timestamps.

- **Determinism:** The types under test are pure value objects. Tests use captured locals and simple
  callbacks — no clock, no timer, no temporary file, no external dependency, no mutable global state.
  Do not attempt to drive these branches through `KeyboardHandler`, which is
  `[ExcludeFromCodeCoverage]` at `KeyboardHandler.cs:22` and depends on Outlook Interop and WinForms
  event arguments.

- **Coverage impact and targets for changed lines/modules:** `coverage.config` excludes none of the
  five in-scope files. CLAUDE.md UT2 names `KbdActions<>` explicitly as a testable seam that is **not**
  exempt from the coverage floor; `KaStringAsync`, `KaChar`, `KaKey`, and `IKbdAction` are pure value
  objects with no COM dependency and fall outside every limb of the COM/VSTO/WinForms exemption. No
  exemption is sought and none is needed: this change adds tests and deletes dead members, so coverage
  of the touched files should rise. A threshold divergence exists between CLAUDE.md UT2 (80 percent
  repository-wide, 90 percent for new code) and `.claude/rules/general-unit-test.md` /
  `.claude/rules/quality-tiers.md` (85 percent line, 75 percent branch). The divergence is
  pre-existing, is not adjudicated by this issue, and does not change the outcome here, because
  coverage of the touched files rises under either figure.

- **Toolchain commands to run (format → lint → type-check → test):** run in this exact order; if any
  step fails or modifies a file, restart from step 1.
  1. `dotnet tool restore`
  2. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  4. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  5. `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`

  Constraints on those commands:
  - Use `/t:Rebuild`, never `/t:Build`. MSBuild's up-to-date check does not invalidate on a
    command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped and runs
    no analyzers.
  - Never add `/p:Nullable=enable`. No project carries a `<Nullable>` element and there is no
    `Directory.Build.props`, so the property conscripts files that never adopted the pragma. CI omits
    it deliberately.
  - `/InIsolation` is mandatory. Without it, roughly 1,695 phantom failures appear as a Moq
    `TypeInitializationException`.
  - Exclude paths containing `\.claude\` from recursive `*.Test.dll` discovery, so that stale builds
    in agent worktrees are not collected.
  - There is no Python toolchain. Any step naming `poetry run python -m scripts.dev_tools.*` is
    unrunnable by absence and must be reported as such rather than executed or reported as passing.

- **Manual validation steps (if required):** None required. All three defects are unobservable through
  the shipping keyboard flow because both callbacks are `null` in production, so a manual Outlook
  session would produce no signal either before or after the fix. A reviewer may confirm the
  production no-op by reading `QfcCollectionController.cs:1376-1383` and `KbdActions.cs:99`.

## Acceptance Criteria

- [x] **AC1 — Branch-3 gating applied.** In `QuickFiler/Controllers/KaStringAsync.cs`, the branch-3
      guard (at `:72` as read on 2026-08-21) reads `if (Activated && Update is not null)`, so all
      three branches of `KeyEquals` gate their `Update` and `ToggleControl` side effects on
      `Activated`. No other guard in the method is weakened.
- [x] **AC2 — Contract documented in-code.** `KaStringAsync.KeyEquals` carries an XML documentation
      comment that states the `Activated` latch contract: every observable side effect fires only
      while `Activated` is true; a matching probe (branch 1) deliberately does not clear the latch;
      a non-matching probe clears it at the trailing `Activated = false`. The comment also documents
      the null and empty argument contract of AC4 and AC5.
- [x] **AC3 — Anti-regression: the early return is preserved.** Branch 1 of `KeyEquals` still returns
      `true` immediately (at `:63` as read on 2026-08-21) and does **not** fall through to the
      trailing `Activated = false` reset. The existing test
      `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue`
      (`QuickFiler.Test/Controllers/KaStringAsyncTests.cs:76-96`), which asserts
      `ka.Activated.Should().BeTrue()` after a matching probe, passes unmodified.
- [x] **AC4 — Null argument rejected explicitly.** `KaStringAsync.KeyEquals(null)` throws
      `ArgumentNullException` naming `other`, thrown from a guard clause placed above the
      `Key.Contains(other)` test rather than from inside `string.Contains`.
- [x] **AC5 — Empty argument rejected explicitly.** `KaStringAsync.KeyEquals("")` throws
      `ArgumentException` naming `other`, with a message explaining that an empty probe would
      otherwise match every registered action.
- [x] **AC6 — The `ArgumentOutOfRangeException` path is closed.** `KeyEquals("")` throws
      `ArgumentException` (AC5) for every combination of instance state, including `Activated = true`
      with a non-null `Update`; `Key.Substring(other.Length - 1, 1)` is never evaluated with a
      negative start index.
- [x] **AC7 — `DelegateType` removed from both implementers.** The `DelegateType` property is deleted
      from `QuickFiler/Controllers/KaChar.cs` (`:43-46`) and `QuickFiler/Controllers/KaKey.cs`
      (`:43-46`). A repository-wide search over `*.cs` for `DelegateType` returns zero hits. No
      `DelegateType` member is added to `QuickFiler/Interfaces/IKbdAction.cs`.
- [x] **AC8 — Dead `Update` removed from four implementers.** The `Update` property and its backing
      field are deleted from `KaChar` (`KaChar.cs:50-55`), `KaCharAsync` (`KaChar.cs:92-97`), `KaKey`
      (`KaKey.cs:50-55`), and `KaKeyAsync` (`KaKey.cs:92-97`).
- [x] **AC9 — `Update` retained on `KaStringAsync`.** The `Update` property remains on
      `QuickFiler/Controllers/KaStringAsync.cs` (`:81-86`) and the five-argument constructor still
      assigns it (`:25`), because it is read at `:61`, `:62`, `:72`, and `:73`.
- [x] **AC10 — Unused `using` removed from `KaChar.cs` only.** `using System.Windows.Forms;` is
      removed from `QuickFiler/Controllers/KaChar.cs:6`, and `QuickFiler/Controllers/KaKey.cs`
      retains its `using System.Windows.Forms;` because `Keys` remains its key type.
- [x] **AC11 — Commented-out interface members removed.** Both commented-out lines at
      `QuickFiler/Interfaces/IKbdAction.cs:15-16` are deleted. The four live members at `:11-14` are
      byte-identical to their pre-change text, and no implementer signature changes.
- [x] **AC12 — Test renamed.** `KeyEquals_MultiCharNonMatch_InvokesUpdateWithFirstCharAndReturnsFalse`
      (`QuickFiler.Test/Controllers/KaStringAsyncTests.cs:134`) is renamed to
      `KeyEquals_MultiCharNonMatchWhileActivated_InvokesUpdateWithFirstCharAndReturnsFalse`, and its
      body is otherwise unchanged.
- [x] **AC13 — Defect-1 regression test added, red before and green after.** A new test in
      `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` arranges `Activated = false` with a non-null
      `Update` and a multi-character non-matching probe, then asserts `Update` is not invoked and the
      result is `false`. The test is observed **failing** against the unmodified production code and
      **passing** after the fix; both runs are recorded under
      `docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/qa-gates/`.
- [x] **AC14 — Latch-survives-transition test added.** A new test asserts that a row which matches at
      depth 1 and then fails at depth 2 still receives its `Key[0]` reset, pinning the behavior that
      AC3 protects.
- [x] **AC15 — Defect-2 regression tests added, red before and green after.** New tests assert
      `ArgumentException` for `KeyEquals("")` (including the `Activated = true` / non-null `Update`
      variant) and `ArgumentNullException` for `KeyEquals(null)`. Each is observed failing before the
      guard clause is added and passing after; both runs are recorded under the same `evidence/qa-gates/`
      directory.
- [x] **AC16 — No pre-existing test is deleted or weakened.** The other seven tests in
      `KaStringAsyncTests.cs` and every test in `KbdActionsTests.cs`,
      `KbdActionsRemainingBranchesTests.cs`, `KaCharTests.cs`, and `KaKeyTests.cs` pass without
      modification. The only permitted change to committed test code is the AC12 rename and the
      addition of the AC13 through AC15 tests.
- [x] **AC17 — No test-project file edit.** `git diff` reports no change to
      `QuickFiler.Test/QuickFiler.Test.csproj`. All new tests land in existing files, so no
      `<Compile Include>` entry is required.
- [x] **AC18 — Scope boundaries respected.** No file under `.claude/**` and no file under
      `docs/features/potential/**` is modified. `QuickFiler/Controllers/KbdActions.cs`,
      `QuickFiler/Controllers/KeyboardHandler.cs`, and
      `QuickFiler/Controllers/QfcCollectionController.cs` are unmodified.
- [x] **AC19 — Out-of-scope fourth defect not fixed, and filed.** `KaStringAsync.cs:62` still reads
      `Update(Key.Substring(other.Length - 1, 1))`, branch 1 still tests `Key.Contains(other)`, and
      the assertion at `KaStringAsyncTests.cs:89-91` (`.Be("b")` for `Key = "abc"`, `other = "ab"`) is
      unchanged. A new GitHub issue is filed for the non-prefix `Substring` defect and its number is
      recorded in **Rollout & Follow-up**.
- [x] **AC20 — File-size limit respected.** No changed file exceeds 500 lines.
      `QuickFiler/Controllers/KaChar.cs`, `QuickFiler/Controllers/KaKey.cs`, and
      `QuickFiler/Interfaces/IKbdAction.cs` are each shorter after the change than before.
- [x] **AC21 — Full C# toolchain green.** In one final uninterrupted pass, in this order:
      `dotnet tool run csharpier check .` reports no file needing formatting;
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
      succeeds; `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
      succeeds (no `/p:Nullable=enable`, no `/t:Build`); and
      `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
      reports zero failures. Command transcripts are recorded under
      `docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/qa-gates/`.

## Risks & Mitigations

- **Technical or operational risks:**
  - *An implementer "completes" the symmetry by removing branch 1's early return.* This is the single
    highest-impact risk in the change: it would silently stop the QuickFiler label from advancing,
    because the `ContainsKey` pass at `KeyboardHandler.cs:181` would consume the activation before
    `FilterKeys` at `:188` ran. Mitigated by the hard constraint in **Boundaries and invariants to
    preserve**, by AC3 as an explicit anti-regression criterion, and by the AC14 test.
  - *The empty-argument guard breaks an unenumerated caller.* Mitigated by a repository-wide search
    that found no caller able to supply an empty string, and by the structural argument that
    `KeyboardHandler.cs:180` appends before every probe. If a reviewer still objects, the recorded
    fallback (early-return `false`) is available, but adopting it requires amending AC5, AC6, and
    AC15 first.
  - *The public-member deletions break an out-of-repo consumer.* Mitigated by the zero-read-site
    evidence and by the explicit public-API notice; these types are internal to the QuickFiler
    add-in and have no published external surface.
  - *The gating change is claimed to have production effect and is over-tested or over-reviewed.*
    Mitigated by recording, in the spec and in the pull-request body, the verified fact that both
    callbacks are `null` on every production instance.
  - *Line-number drift.* Every `file:line` citation in this spec is as read on 2026-08-21 and will
    shift as edits land. Mitigated by phrasing acceptance criteria in terms of the observable code
    text and behavior, with line numbers as locators only.
- **Mitigations and rollbacks:**
  - Regression tests are authored before the production edit and observed red, so each criterion has
    a failing-then-passing witness rather than an assertion of correctness.
  - Rollback is a single revert of the pull request; there is no data migration, no feature flag, and
    no persisted state to unwind.
  - If any toolchain stage fails or rewrites a file, the loop restarts from formatting, per CLAUDE.md
    General Code Change Policy section 8.1.

## Rollout & Follow-up

- **Release/rollout steps:**
  1. Author the AC13 through AC15 regression tests and observe them fail.
  2. Apply the nine changes listed under **Files/modules to change**.
  3. Run the full toolchain in order until one pass completes clean, recording transcripts under
     `docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/qa-gates/`.
  4. Check off each acceptance criterion in this file as its evidence lands, per the
     `acceptance-criteria-tracking` protocol. This `spec.md` is the sole acceptance-criteria source
     for work mode `full-bug`; no `user-story.md` exists.
  5. Open the pull request with the public-API notice from **Design summary** reproduced in the body.
- **Post-fix monitoring or clean-up tasks:**
  - **Recommended follow-up issue (required by AC19):** the non-prefix `Substring` defect at
    `QuickFiler/Controllers/KaStringAsync.cs:62`. Branch 1 guards on `Key.Contains(other)` but
    computes `Key.Substring(other.Length - 1, 1)`, which is only meaningful when `other` is a prefix
    of `Key`. Resolving it requires deciding between `Contains` and `StartsWith` for branch 1, which
    would change keyboard-filtering behavior currently pinned by `KbdActionsTests.cs:71-76`. Per
    CLAUDE.md Bugfix Workflow section 2, this is filed as a new issue rather than folded into #445.
    Record the issue number here once filed.
  - **Awareness only, not owned by this issue:** `QuickFiler/Controllers/QfcCollectionController.cs`
    is 2349 lines, a pre-existing violation of the 500-line rule; and
    `QuickFiler/Controllers/KeyboardHandler.cs` is 414 lines with limited headroom.
  - **Awareness only:** the coverage-threshold divergence between CLAUDE.md UT2 (80 / 90 percent) and
    `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` (85 percent line,
    75 percent branch) is pre-existing and is not adjudicated here.
  - No production monitoring applies; the fix is unobservable through the shipping code path.
- **Links: issue, PRs, related docs**
  - Issue: https://github.com/drmoisan/TaskMaster/issues/445
  - Requirements source: `docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/issue.md`
  - Research (authoritative over `issue.md` where they conflict):
    `docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/research/keyboard-action-contract-defects.2026-08-21T18-20.md`
  - Plan: `docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/plan.2026-08-21T18-09.md`
  - Related: issue #430 (`quickfiler-keyboard-actions-coverage`, child F3 of epic #136), whose
    no-behavior-change acceptance criterion is why these defects were deferred.
  - Follow-up issue for the non-prefix `Substring` defect: **#583** —
    https://github.com/drmoisan/TaskMaster/issues/583 (filed 2026-08-22, satisfies AC19).
