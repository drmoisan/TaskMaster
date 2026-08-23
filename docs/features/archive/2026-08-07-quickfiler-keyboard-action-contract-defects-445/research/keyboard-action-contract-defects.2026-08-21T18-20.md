# Research — QuickFiler Keyboard-Action Contract Defects (Issue #445)

- **Timestamp:** 2026-08-21T18-20
- **Issue:** #445
- **Branch:** `bug/quickfiler-keyboard-action-contract-defects-445`
- **Base:** `origin/epic/quickfiler-suite-determinism-foundation-integration`
- **Feature folder:** `docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/`
- **Scope:** research only; no source file was modified.

All line numbers below were re-derived by direct file read in this worktree on 2026-08-21. The
`file:line` citations in `issue.md` were captured against base commit `56ca1cea` and several have
drifted; corrections are recorded in section 1.

---

## 0. Toolchain Availability (recorded before any procedural claim)

**There is no Python toolchain in this repository.**

- `SearchScope:` repository root of the worktree.
- `SearchPatterns:` `scripts/dev_tools/**`, `pyproject.toml`, `poetry.lock`, `**/pyproject.toml`
- `SearchResult:` none.

Any skill or plan step naming `poetry run python -m scripts.dev_tools.*` is **unrunnable by
absence** in this repository. No such command was executed and no result is fabricated.

Related: `spec.md:76` reads "Unit tests (pytest) for the fixed behavior and boundaries". This is an
unedited template artifact. The applicable framework is MSTest per CLAUDE.md CUT1.

The applicable toolchain is the four-stage C# loop in CLAUDE.md CUT3:
`dotnet tool run csharpier format .` → analyzer `msbuild /t:Rebuild` → nullable `msbuild /t:Rebuild`
→ `vstest.console.exe <assemblies> /EnableCodeCoverage`.

---

## 1. Corrected Line Numbers (issue.md citations re-derived)

| Claim in `issue.md` | Cited | **Actual (verified)** | Status |
|---|---|---|---|
| `KeyEquals` span | `KaStringAsync.cs:57-78` | **`KaStringAsync.cs:57-79`** | off by one (closing brace at :79) |
| Branch 1 gate | (implied :59-61) | **`:59` condition, `:61` gate, `:62` `Update` call** | confirmed |
| Empty-string `Substring` | `KaStringAsync.cs:62` | **`:62`** | confirmed |
| Branch 2 (`Length == 1`) | (implied) | **`:65` condition, `:67` gate, `:68` `ToggleControl`** | confirmed |
| Branch 3 (`Length > 1`) | (implied) | **`:70` condition, `:72` ungated guard, `:73` `Update` call** | confirmed |
| Branch 3 `ToggleControl` | (implied) | **`:74` gate, `:75` call** | confirmed |
| `Activated = false` | `:77` (shown as `:46` in the excerpt's local numbering) | **`:77`** | confirmed |
| `KaChar` class decl | `KaChar.cs:11` | **`:11`** | confirmed |
| `KaChar.Delegate` | `KaChar.cs:37` | **`:37`** (backing field `:36`) | confirmed |
| `KaChar.DelegateType` | `KaChar.cs:43-46` | **`:43-46`** | confirmed |
| `KaKey.DelegateType` | not cited | **`KaKey.cs:43-46`** | added |
| `IKbdAction` commented members | `IKbdAction.cs:12-16` | **`:15-16`** (the two comments); `:11-14` are the live members | narrowed |
| `KaCharAsync` decl | `KaChar.cs:58` | **`:58`** | confirmed |
| `KaKeyAsync` decl | not cited | **`KaKey.cs:58`** | added |

Additional call-site line numbers, verified:

- `KbdActions.ContainsKey` — **`KbdActions.cs:49`**
- `KbdActions.FilterKeys` — **`KbdActions.cs:51`**
- `KbdActions.Find` — **`KbdActions.cs:53-69`**
- `KbdActions.FindIndex` — **`KbdActions.cs:71-88`**
- `KbdActions` indexer — **`KbdActions.cs:36-47`** (getter calls `Find` at `:38`; setter at `:41`)
- `KeyboardHandler` string-filter loop — **`KeyboardHandler.cs:178-202`**
- Sole production `KaStringAsync` construction — **`QfcCollectionController.cs:1376-1383`**

---

## 2. Current-State Analysis

### 2.1 The type under change

`QuickFiler/Controllers/KaStringAsync.cs` (95 lines). `KeyEquals` in full, as it stands:

```csharp
// KaStringAsync.cs:57-79
public bool KeyEquals(string other)
{
    if (Key.Contains(other))                       // :59   BRANCH 1
    {
        if (Activated && Update is not null)       // :61   gated
            Update(Key.Substring(other.Length - 1, 1));  // :62
        return true;                               // :63   early return — NO Activated reset
    }
    else if (other.Length == 1)                    // :65   BRANCH 2
    {
        if (Activated && ToggleControl is not null)// :67   gated
            ToggleControl();                       // :68
    }
    else if (other.Length > 1)                     // :70   BRANCH 3
    {
        if (Update is not null)                    // :72   NOT gated
            Update(Key.Substring(0, 1));           // :73
        if (Activated && ToggleControl is not null)// :74   gated
            ToggleControl();                       // :75
    }
    Activated = false;                             // :77   reached only from branches 2 and 3
    return false;                                  // :78
}
```

Two structural facts drive everything in section 3 and are easy to miss:

1. **Branch 1 returns at `:63` before the `Activated = false` reset at `:77`.** A matching element
   therefore never clears its own activation. `Activated` behaves as a latch that only a
   *non-matching* probe can clear.
2. **The `other.Length == 0` case is unreachable as a fall-through.** `Key.Contains("")` is `true`
   for every string, so an empty `other` always enters branch 1. The `if/else if/else if` chain has
   no reachable path to `:77` other than branches 2 and 3.

### 2.2 Ownership of the mutable members

`SearchScope:` all `*.cs` in the repository worktree.
`SearchPatterns:` `Activated`, `ToggleControl`, `Update`, `\.Update\b`, `Update =`, `Update\(`
`SearchResult:`

- `Activated` — declared **only** on `KaStringAsync` (`:50-55`). Read at `:61`, `:67`, `:74`;
  written at `:77`. The **only** external write site in the entire repository is
  **`KeyboardHandler.cs:187`**.
- `ToggleControl` — declared **only** on `KaStringAsync` (`:88-93`). Assigned only by the
  five-argument constructor at `:26`. **No other assignment exists anywhere.**
- `Update` — declared on five types: `KaStringAsync.cs:81-86`, `KaChar.cs:50-55`,
  `KaCharAsync` (`KaChar.cs:92-97`), `KaKey.cs:50-55`, `KaKeyAsync` (`KaKey.cs:92-97`).
  Read **only** at `KaStringAsync.cs:61,62,72,73`. Written **only** at `KaStringAsync.cs:25`
  (the five-argument constructor).

Consequence: on `KaChar`, `KaCharAsync`, `KaKey`, and `KaKeyAsync`, `Update` is **write-never,
read-never dead API**. It is live only on `KaStringAsync`.

### 2.3 The production construction site — both callbacks are null

`QuickFiler/Controllers/QfcCollectionController.cs:1363-1385` is the **only** production code that
constructs a `KaStringAsync` with the five-argument constructor:

```csharp
// QfcCollectionController.cs:1376-1383
var stringAsyncAction = new KaStringAsync(
    "Collection",
    key,
    (s) => ChangeByIndexAsync(int.Parse(s) - 1),
    //(s) => grp.ItemViewer.LblItemNumber.Text = s,   // :1380  update  — COMMENTED OUT
    null,                                             // :1381  update
    null                                              // :1382  toggleControl
);
```

Two findings of first-rank importance:

1. **`Update` and `ToggleControl` are both `null` in production, always.** The other registration
   path, `KbdActions.Add(string, TKey, VDelegate)` (`KbdActions.cs:90-104`), builds its element with
   `UClass instance = new()` at `:99` — the parameterless constructor (`KaStringAsync.cs:12`), which
   assigns neither. There is no post-construction assignment anywhere (section 2.2). Therefore
   **every `Update is not null` and `ToggleControl is not null` guard in `KeyEquals` evaluates
   `false` in production today.**
2. **The commented-out line `:1380` recovers the design intent of `Update`.** It is
   `grp.ItemViewer.LblItemNumber.Text = s` — `Update` writes a single character into the
   per-row item-number label. This is the UI affordance the branches drive (section 3.3).

`Key` is always non-empty in production: `GenerateStringKbdAction` assigns from `Digits`
(`QfcCollectionController.cs:114-128`), which returns `_itemGroups?.Count >= 10 ? 2 : 1` — only 1 or
2, so `:1369` or `:1373` always assigns. The `key = ""` initialization at `:1366` is never observed.

### 2.4 The only keystroke-driven consumer

`QuickFiler/Controllers/KeyboardHandler.cs:178-202`, inside `KeyDownTaskAsync`:

```csharp
else if (StringActionsAsync != null)                                    // :178
{
    _filterBuilder.Append(char.ToLower((char)e.KeyValue));               // :180
    if (StringActionsAsync.ContainsKey(_filterBuilder.ToString()))       // :181   PASS A
    {
        e.SuppressKeyPress = true;
        e.Handled = true;

        if (_filterBuilder.Length == 1)
            StringActionsAsync.ForEach(x => x.Activated = true);         // :187   RE-ARM
        var actions = StringActionsAsync.FilterKeys(_filterBuilder.ToString()); // :188  PASS B
        if (actions.Length == 0)
            _filterBuilder.Length = 0;
        else if (actions.Length == 1)
        {
            var keyName = actions[0].Key;
            await StringActionsAsync[keyName](keyName);                  // :194   PASS C + D
            _filterBuilder.Length = 0;
        }
    }
    else
    {
        _filterBuilder.Length--;                                         // :200
    }
}
```

Probe-length analysis (answers "what string lengths, in what order"):

- `:180` appends **before** every probe, so **every probe at `:181` has length >= 1**. A
  non-matching keystroke is undone at `:200`, so the filter only ever grows along a matching path.
  `:190` and `:195` reset the length to 0, but the next keystroke appends first.
  **No production caller can pass an empty string to `KeyEquals`.**
- The probe at `:181` and at `:188` is the filter (`"1"`, then `"12"`, ...). The probe at `:194` is
  `actions[0].Key` — the **full registered key**, not the filter.
- **`Activated = true` is set for all elements only when `_filterBuilder.Length == 1`** (`:186-187`),
  i.e. once per filter sequence, and **after** the `ContainsKey` pass at `:181` has already run.
  `ForEach` here is `EnumerableEx.ForEach` from `System.Interactive` 7.0.1 (in
  `QuickFiler/packages.config:59`); there is no in-repo declaration
  (`SearchScope:` all `*.cs`; `SearchPatterns:` `static.{0,80}ForEach`, `(void|IEnumerable<\w+>) ForEach`;
  `SearchResult:` only the commented-out `UtilitiesCS/Extensions/IEnumerableExtensions.cs:94`).
  It performs one pass and calls no `KeyEquals`.

`KeyboardHandler` carries **`[ExcludeFromCodeCoverage]` at `KeyboardHandler.cs:22`**. The only
component that exercises these branches with real keystrokes is coverage-exempt, which is precisely
why the branch contract must be pinned by unit tests on `KaStringAsync` itself.

---

## 3. Q1 — The `Activated`-Gating Contract for `KaStringAsync.KeyEquals`

### 3.1 How many times `KeyEquals` fires per lookup (the deferred-LINQ question)

`Find` (`KbdActions.cs:53-69`) builds a deferred `Where` query at `:55` and then re-enumerates it:

| Call | Enumerations of the predicate | Count for a list of N with first match at 0-based index k |
|---|---|---|
| `ContainsKey` (`:49`, `Any`) | short-circuits at first `true` | `k+1`; `N` when no match |
| `FilterKeys` (`:51`, `ToArray`) | full | `N` |
| `Find` (`:53`) — 0 matches | `Count()` only | `N` |
| `Find` — exactly 1 match | `Count()` at `:56` **then** `First()` at `:62` | **`N + k + 1`** |
| `Find` — 2+ matches | `Count()` at `:56` then `Select(...)` at `:66` | `2N` |
| `FindIndex` (`:71`) — 1 match | `Count()` at `:74` then `_list.FindIndex` at `:80` | **`N + k + 1`** |
| Indexer get/set (`:38`, `:41`) | delegates to `Find` | as `Find` |

`Enumerable.Where` returns an iterator, not an `ICollection<T>`, so `Count()` has no fast path and
walks the whole sequence. **A single `Find` therefore invokes `KeyEquals` on the matching element
twice and on each preceding non-matching element twice.** `KeyEquals` has side effects, so this is
a semantic fact, not a performance note.

### 3.2 What the re-enumeration does to each branch — the decisive asymmetry

Combine section 3.1 with the two structural facts of section 2.1:

- **A matching element** takes branch 1, which returns at `:63` without clearing `Activated`. Its
  `Update` therefore fires on **every** enumeration pass — twice per `Find`.
- **A non-matching element** falls through to `:77` and clears `Activated` on the **first** pass.
  On every later pass its **gated** `ToggleControl` is suppressed, but its **ungated** `Update` at
  `:73` fires again.

So today: **the gated side effects are self-limiting to one invocation per lookup (the `Activated`
latch absorbs the re-enumeration), while the ungated `Update` fires once per enumeration pass — a
count determined by `Find`'s internal use of `Count()` plus `First()`, not by user intent.**

That is the strongest available characterization of the defect. It is not merely "one branch is
inconsistent"; the ungated call has an invocation count that is an artifact of a LINQ implementation
detail in a different class.

### 3.3 What `Update` means in each branch (design intent, reconstructed)

From `QfcCollectionController.cs:1380`, `Update(s)` sets a row's item-number label to the single
character `s`. The label is therefore a **"current typing depth" indicator**, showing one character
at a time. Under that model the three branches are coherent:

| Branch | Condition | `Update` argument | Affordance |
|---|---|---|---|
| 1 (`:59`) | `Key.Contains(other)` | `Key.Substring(other.Length - 1, 1)` = **`Key[other.Length-1]`** | advance the label to the character at the current depth |
| 2 (`:65`) | non-match, `Length == 1` | *(none)* | at depth 1 the row never advanced, so it already shows `Key[0]`; only toggle it off |
| 3 (`:70`) | non-match, `Length > 1` | `Key.Substring(0, 1)` = **`Key[0]`** | the row may have advanced past depth 0, so reset the label to depth 0, then toggle it off |

**Correction to the delegation brief.** Branch 1 passes `Key[other.Length - 1]` — the character at
the **last position of the matched prefix**, i.e. the character the user just matched. It is **not**
"the character AFTER the matched prefix". For `Key="abc"`, `other="ab"` it yields `"b"`, not `"c"`;
the existing test at `KaStringAsyncTests.cs:89-91` asserts exactly `"b"` and confirms this reading.

The omission of `Update` from branch 2 is not a fourth inconsistency — it is correct under this
model, because a row that has never matched is already displaying `Key[0]`.

### 3.4 The two candidate contracts

**Option A — gate all three branches (`Activated && Update is not null` at `:72`).**

**Option B — ungate branch 1 as well, so `Update` never checks `Activated` and only
`ToggleControl` does.** The argument for B is real and is recorded rather than discarded:
`ToggleControl` is a *toggle* — invoking it twice returns the control to its original state, so it
is genuinely non-idempotent and *must* be latched. `Update` is an *assignment* and is idempotent, so
it arguably needs no latch. Under B, `Activated` is redefined as "the latch that protects the
non-idempotent callback", which explains the current code exactly as written.

### 3.5 Which option preserves observable production behavior

**Neither option changes observable production behavior, and this is verifiable rather than
assumed.** `Update` and `ToggleControl` are `null` on every `KaStringAsync` instance that production
ever creates (section 2.3), so `Update is not null` at `:72` is `false` on every production
evaluation and the guarded call is unreachable. Any redistribution of the `Activated` condition
across the three branches is therefore a **no-op in production today**. The change is observable
only in tests, and in any future code that supplies a non-null `Update`.

This removes the usual "which option is safer" tie-breaker and forces the decision onto contract
quality alone.

### 3.6 Recommendation — Option A, gate all three branches

**Recommendation: add the `Activated` conjunct to branch 3 at `KaStringAsync.cs:72`, making all
three branches gate uniformly on `Activated`, and document the latch semantics in an XML comment.**

Four supporting findings:

1. **It makes every side effect's invocation count independent of LINQ re-enumeration.** Today the
   ungated `Update` fires 2x per `Find` and 1x per `ContainsKey`/`FilterKeys` pass; under Option A it
   fires at most once per element per keystroke, matching `ToggleControl`. The current behaviour is
   only harmless because the callback happens to be an idempotent assignment — a property no
   signature enforces and no test asserts.
2. **It loses no semantically meaningful label reset.** The reset in branch 3 matters only for a row
   that had *advanced* its label. Any such row executed branch 1 with `Update`, which requires
   `Activated == true` (`:61`) and returns at `:63` without clearing it. Therefore, **at the moment a
   row first stops matching, its `Activated` is still `true`**, and the gated reset still fires.
   Only *subsequent* probes of an already-reset row are suppressed — and those writes are redundant,
   re-writing `Key[0]` over `Key[0]`.
3. **It matches the acceptance criterion as written** (`issue.md:124-125`: "applied consistently
   across all three branches").
4. **It requires no change to any existing test** (section 5), whereas Option B inverts
   `KeyEquals_ContainsMatchWhileNotActivated_ReturnsTrueWithoutUpdate`
   (`KaStringAsyncTests.cs:98-114`), turning a passing characterization test into a contradiction.

**Counter-argument to Option A, stated fairly.** If `Activated` is really "the latch for the
non-idempotent callback" (section 3.4), then gating an idempotent assignment behind it is
over-application, and Option B is the more precise contract. Under Option A, a hypothetical future
`Update` that is *not* idempotent — say, one that animates or appends — would be suppressed on rows
deactivated earlier in the sequence, whereas today it would fire. The rebuttal is that finding 2
shows the *first* reset after an advance is always still gated-through, so a non-idempotent `Update`
would be *better* served by Option A (exactly one reset) than by the status quo (one reset per
enumeration pass, a count no caller can predict).

### 3.7 Implementation trap — do not "finish" the consistency by moving the reset

A natural follow-on edit is to make branch 1 also fall through to `Activated = false` at `:77`, for
symmetry. **This would break the feature.** On a keystroke of length >= 2 there is no re-arm
(`:186-187` runs only at length 1), and the call order is `ContainsKey` (`:181`) → `FilterKeys`
(`:188`) → indexer/`Find` (`:194`). If branch 1 cleared `Activated`, the `ContainsKey` pass would
consume the activation and the `FilterKeys` pass would no longer advance the label. **The early
return at `:63` is load-bearing and must be preserved verbatim.**

---

## 4. Q2 — Contract for an Empty `other`

### 4.1 Exact current behavior

`Key.Contains("")` is `true` for every string, so `KeyEquals("")` enters branch 1 and evaluates
`Key.Substring(-1, 1)` at `:62` — `ArgumentOutOfRangeException`. **The throw is conditional on
`Activated && Update is not null`.** With either false, `KeyEquals("")` returns `true` without
throwing, which makes `ContainsKey("")` true and `FilterKeys("")` return **every** registered action.
Because production always has `Update == null` (section 2.3), the *reachable* production
misbehaviour is the silent "empty matches everything", not the exception.

### 4.2 Reachability

No production caller can pass an empty string (section 2.4: `:180` appends before every probe). But
`KbdActions<TKey,...>` is a `public` generic type and `ContainsKey`, `FilterKeys`, `Find`, and the
indexer are public API. `Find("")` against a registry holding two or more actions matches all of
them and throws `InvalidOperationException` from `KbdActions.cs:67`.

### 4.3 Options

| Option | Behavior | Existing test that changes | Production caller affected |
|---|---|---|---|
| 1. Early-return `false` | empty probe matches nothing | **none** | none |
| 2. Early-return `true` | preserves `Contains("")==true`; `FilterKeys("")` still returns everything | **none** | none |
| 3. Throw `ArgumentException` | explicit rejection at the boundary | **none** | none |
| 4. Guard only the `Substring` | keeps `true`, removes the crash, leaves "matches everything" undocumented | **none** | none |

**No existing test passes an empty string**, so all four options are neutral with respect to the
current suite and each requires new tests.
`SearchScope:` `QuickFiler*/**/*.cs`. `SearchPatterns:` `KeyEquals\(""\)`, `KeyEquals\(string\.Empty\)`.
`SearchResult:` none.

**Recommendation: Option 3 — throw `ArgumentException` from a guard clause placed at the top of
`KeyEquals`, before the `Key.Contains(other)` test.** It satisfies the acceptance criterion's own
second limb (`issue.md:126-127`, "or rejects it with an explicit, documented argument exception"),
it matches CLAUDE.md General §3 and C#4.1 ("fail fast and explicitly"), and it has zero production
callers to break. Option 1 is the acceptable fallback if a reviewer prefers a total, non-throwing
function; Option 2 is the weakest because it preserves the "empty matches every action" semantics
that make `FilterKeys("")` meaningless.

**Optional hardening in the same guard.** `KeyEquals(null)` currently throws
`ArgumentNullException` from inside `string.Contains`. Promoting that to an explicit
`throw new ArgumentNullException(nameof(other))` in the same guard block costs one line and makes
the contract self-documenting. It changes the exception's origin, not its type.

### 4.4 The fourth latent defect — `Substring(other.Length - 1, 1)` is wrong for a non-prefix match

**Confirmed as a genuine, distinct defect.** Branch 1's guard is `Key.Contains(other)` — a substring
test — but its `Substring(other.Length - 1, 1)` is only meaningful when `other` is a **prefix** of
`Key`. Counter-example: `Key = "abc"`, `other = "b"`. `Contains` is `true`; the expression yields
`Substring(0, 1) == "a"`, which is neither the matched character `"b"` nor the following character
`"c"`.

It is reachable in principle whenever `Digits == 2`: with keys `"01".."12"`, typing `"1"` matches
`"01"` by `Contains` at index 1 (not a prefix), and `Update` would receive `"0"` instead of `"1"`.
It has no observable effect today because `Update` is null.

The existing test `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue`
(`KaStringAsyncTests.cs:76-96`) uses `Key="abc"`, `other="ab"` — a prefix — so it does not expose it.

**Scope recommendation: OUT of scope for #445.** It is not among the three defects the issue
enumerates, and CLAUDE.md's Bugfix Workflow §2 directs that a deeper design problem uncovered during
a fix be raised as a new issue rather than widening scope. Fixing it correctly requires deciding
whether the branch should test `StartsWith` instead of `Contains` — a behavior change to keyboard
filtering that `KbdActionsTests.cs:71-76` currently pins to substring semantics
("`KaStringAsync.KeyEquals` substring matching must remain available for keyboard filtering").
**Recommended action: promote to a new potential entry / GitHub issue.** It does not conflict with
the Q1 or Q2 changes, which touch branch 3's gate and a top-of-method guard respectively, leaving
`:62` untouched.

---

## 5. Q3 — `DelegateType` and `Update` Disposition

### 5.1 `DelegateType` has zero read sites

`SearchScope:` all `*.cs` in the worktree (a repo-wide search including `docs/**` was run first and
returned only prose hits in the feature/potential markdown, which are not code).
`SearchPatterns:` `DelegateType`
`SearchResult:` exactly three, none of them a read:

- `QuickFiler/Interfaces/IKbdAction.cs:16` — a comment, `//Type DelegateType { get; }`
- `QuickFiler/Controllers/KaKey.cs:43` — declaration (`:43-46`)
- `QuickFiler/Controllers/KaChar.cs:43` — declaration (`:43-46`)

**Confirmed: no consumer reads `DelegateType`.** It is not on the interface, so no polymorphic call
site can reach it either. Removing both declarations breaks no compilation and no test
(`KaCharTests.cs` and `KaKeyTests.cs` contain no occurrence — see section 6).

`KaKey.DelegateType` returning `typeof(Action<Keys>)` is *correct* for `KaKey` (which stores
`Action<Keys>` at `:36-41`); `KaChar.DelegateType` returning the same is *wrong* for `KaChar` (which
stores `Action<char>` at `:36-41`). Removal resolves defect 3 without needing to decide the right
value.

**Implementation note (analyzer-relevant).** `Keys` appears in `KaChar.cs` **only** at `:45`, inside
`DelegateType`. After removal, `using System.Windows.Forms;` at `KaChar.cs:6` becomes unused and
should be removed in the same edit. `KaKey.cs` uses `Keys` as its key type throughout, so its
`using` stays. Note that IDE0005 is evidently not error-level in this build: `IKbdAction.cs:1-5`
carries five `using` directives none of which the file uses, and it compiles today. Removing the
`using` is hygiene, not a build requirement.

### 5.2 `Update` should be removed from four of the five implementers

`Update` **is** read — but only inside `KaStringAsync.KeyEquals` (`:61,62,72,73`), and it is written
only by `KaStringAsync`'s own constructor (`:25`). Section 2.2 establishes there is no other read or
write anywhere in the repository.

| Type | `Update` declared at | Read anywhere? | Written anywhere? | Disposition |
|---|---|---|---|---|
| `KaStringAsync` | `KaStringAsync.cs:81-86` | **yes** (`:61,62,72,73`) | yes (`:25`) | **KEEP** |
| `KaChar` | `KaChar.cs:50-55` | no | no | **REMOVE** |
| `KaCharAsync` | `KaChar.cs:92-97` | no | no | **REMOVE** |
| `KaKey` | `KaKey.cs:50-55` | no | no | **REMOVE** |
| `KaKeyAsync` | `KaKey.cs:92-97` | no | no | **REMOVE** |

### 5.3 The two commented-out members in `IKbdAction.cs:15-16`

**Recommendation: delete both lines.**

- `//Type DelegateType { get; }` (`:16`) — restoring it **will not compile**: `KaStringAsync`
  (`KaStringAsync.cs:10`), `KaCharAsync` (`KaChar.cs:58`) and `KaKeyAsync` (`KaKey.cs:58`) do not
  declare it. Confirmed by direct read of all three. Since section 5.1 removes the only two
  declarations, the comment documents a member that will no longer exist anywhere.
- `//Action<string> Update { get; set; }` (`:15`) — restoring it would force all five implementers
  to keep `Update`, contradicting section 5.2, which removes it from four. `Update` is an
  implementation detail of `KaStringAsync`'s filtering feedback, not a contract shared by a `char`-
  or `Keys`-keyed action.

Deleting both satisfies `issue.md:129-130` ("resolved (removed or restored with all implementers
updated)"). `IKbdAction.cs` shrinks from 18 to 16 lines; its live members at `:11-14` are untouched,
so no implementer changes.

---

## 6. Q4 — Existing Characterization Tests

### 6.1 Headline finding: no existing test asserts any of the three defects

The premise recorded at `issue.md:108-111` — that #430's tests "characterize the current behavior,
including the ungated `Update` call and the empty-string throw, so that a later fix has a
red-before-green baseline" — **is not borne out by the committed tests.**

- The multi-char branch test sets **`ka.Activated = true`** (`KaStringAsyncTests.cs:141`), so it
  exercises the branch with the gate *satisfied*. It does not distinguish gated from ungated and
  **passes unchanged under Option A**.
- **No test passes an empty string** (`SearchScope:` `QuickFiler*/**/*.cs`; `SearchPatterns:`
  `KeyEquals\(""\)`, `KeyEquals\(string\.Empty\)`; `SearchResult:` none).
- **No test references `DelegateType`** (`SearchScope:` all `*.cs`; `SearchPatterns:` `DelegateType`;
  `SearchResult:` three hits, all in production files, listed in section 5.1).

**Consequence for planning:** there is no red-before-green baseline to inherit, and
`issue.md:131` ("replacing the characterization tests added by #430") describes work that does not
exist — **nothing needs replacing or deleting.** The regression tests for defects 1 and 2 must be
authored fresh, and each will be genuinely red before the fix.

### 6.2 Per-test disposition

**`QuickFiler.Test/Controllers/KaStringAsyncTests.cs` — 168 lines**

| Test method | Lines | Asserts a defect? | Disposition under Option A |
|---|---|---|---|
| `Constructor_LowercasesKeyAndStoresMembers` | 27-40 | no | unchanged |
| `KeySetter_LowercasesValue` | 42-53 | no | unchanged |
| `Delegate_AwaitsAndCompletesSynchronously` | 55-74 | no | unchanged |
| `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue` | 76-96 | no (branch 1, gated) | **unchanged**; also pins `Substring(other.Length-1,1)` via `.Be("b")` at `:89-91` — leave as-is, section 4.4 is out of scope |
| `KeyEquals_ContainsMatchWhileNotActivated_ReturnsTrueWithoutUpdate` | 98-114 | no | **unchanged** under A; would need **inversion** under Option B |
| `KeyEquals_SingleCharNonMatchWhileActivated_InvokesToggleControlAndReturnsFalse` | 116-131 | no (branch 2, gated) | unchanged |
| `KeyEquals_MultiCharNonMatch_InvokesUpdateWithFirstCharAndReturnsFalse` | 133-152 | **no** — sets `Activated = true` at `:141` | **RETAIN, RENAME** to `KeyEquals_MultiCharNonMatchWhileActivated_InvokesUpdateWithFirstCharAndReturnsFalse`; the current name implies coverage of the ungated case that it does not provide |
| `KeyEquals_NullDelegatesAreToleratedInNonMatchBranches` | 154-166 | no | unchanged |

**`QuickFiler.Test/Controllers/KbdActionsTests.cs` — 88 lines**

| Test method | Lines | Disposition |
|---|---|---|
| `Add_WhenSourceAndStoredKeysAreDistinct_DoesNotTreatSubstringAsDuplicate` | 13-29 | unchanged |
| `Add_WhenSourceAndStoredKeyAreExactDuplicate_ThrowsArgumentException` | 31-47 | unchanged |
| `FilterKeys_WhenDistinctStoredKeysCoexist_PreservesKeyboardMatchingSemantics` | 49-86 | unchanged — elements come from `Add(sourceId,key,delegate)` (`KbdActions.cs:99`, parameterless ctor), so `Update`/`ToggleControl` are null and `Activated` is false. **Note:** `:71-76` pins `Contains`-based substring matching; it is the test that would block a `StartsWith` fix for section 4.4 |

**`QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` — 181 lines**
All 11 tests use `KaKey`, whose `KeyEquals` (`KaKey.cs:48`) is `Key == other` — side-effect-free.
No test references `Update` or `DelegateType`. **All unchanged**, including after `Update` is removed
from `KaKey` (section 5.2).

**`QuickFiler.Test/Controllers/KaCharTests.cs` — 155 lines**
Nine tests, none referencing `DelegateType` or `Update`. **All unchanged.**

**`QuickFiler.Test/Controllers/KaKeyTests.cs` — 144 lines**
Seven tests, none referencing `DelegateType` or `Update`. **All unchanged.**

### 6.3 New tests required

All belong in `KaStringAsyncTests.cs` (MSTest + FluentAssertions per CLAUDE.md CUT1/CUT2):

1. **Defect 1 regression (red before fix):** `Key="abc"`, `other="zz"`, non-null `Update`,
   `Activated = false` → assert `Update` is **not** invoked and the result is `false`. This is the
   test that fails today and passes after gating `:72`.
2. **Defect 1, latch-survives-transition:** a row that matched at depth 1 then fails at depth 2
   still receives its `Key[0]` reset, proving finding 2 of section 3.6.
3. **Defect 2:** `KeyEquals("")` → assert `ArgumentException` with the documented message
   (per the section 4.3 recommendation). Add the `Activated=true`/non-null-`Update` variant so the
   old `ArgumentOutOfRangeException` path is explicitly closed.
4. **Optional:** `KeyEquals(null)` → `ArgumentNullException`.

### 6.4 Test-file sizes against the 500-line cap

| File | Lines | Headroom |
|---|---|---|
| `KaStringAsyncTests.cs` | **168** | grows by ~50-60 lines for section 6.3 → ~225. Comfortable |
| `KbdActionsRemainingBranchesTests.cs` | **181** | unchanged |
| `KaCharTests.cs` | **155** | unchanged |
| `KaKeyTests.cs` | **144** | unchanged |
| `KbdActionsTests.cs` | **88** | unchanged |

**None is at or near the 500-line cap.** No test-file split is required.

### 6.5 Test project file — confirmed, do not edit

`QuickFiler.Test/QuickFiler.Test.csproj` already carries all five compile entries:

```
96:    <Compile Include="Controllers\KbdActionsTests.cs" />
97:    <Compile Include="Controllers\KbdActionsRemainingBranchesTests.cs" />
98:    <Compile Include="Controllers\KaCharTests.cs" />
99:    <Compile Include="Controllers\KaKeyTests.cs" />
100:   <Compile Include="Controllers\KaStringAsyncTests.cs" />
```

**No `.csproj` edit is required or permitted** — a sibling epic child owns this file. Because all
new tests go into existing files, no new `<Compile Include>` entry is needed.

---

## 7. Q5 — Production File Sizes

| File | Lines | Post-change estimate | vs 500 cap |
|---|---|---|---|
| `QuickFiler/Controllers/KaStringAsync.cs` | **95** | ~110 (guard clause + XML doc) | far under |
| `QuickFiler/Controllers/KaChar.cs` | **99** | ~88 (remove `DelegateType` `:43-46`, two `Update` props, one `using`) | far under |
| `QuickFiler/Controllers/KaKey.cs` | **99** | ~90 (remove `DelegateType` `:43-46`, two `Update` props) | far under |
| `QuickFiler/Interfaces/IKbdAction.cs` | **18** | 16 | far under |
| `QuickFiler/Controllers/KbdActions.cs` | **146** | 146 (unchanged) | far under |

**Confirmed: no in-scope file approaches the 500-line cap; three of the five shrink.**

Two adjacent files, recorded for awareness only — **neither is modified by this work**:

- `QuickFiler/Controllers/KeyboardHandler.cs` — **414 lines**, under the cap but with limited
  headroom. It is `[ExcludeFromCodeCoverage]` (`:22`).
- `QuickFiler/Controllers/QfcCollectionController.cs` — **2349 lines**, a **pre-existing violation**
  of the 500-line rule. Out of scope; do not touch. Reading `:1363-1385` is sufficient.

---

## 8. Q6 — Coverage Baseline Context

### 8.1 `coverage.config` excludes none of these files

`coverage.config` (24 lines, repo root) contains a single `<ModulePaths><Exclude>` block listing
seven third-party module patterns: `Deedle`, `FSharp`, `Castle.Core`, `FluentAssertions`, `Moq`,
`Microsoft.Testing`, `MSTest` (`:14-20`). There is **no** `<Sources>` or `<Functions>` exclusion and
no QuickFiler entry.

`SearchScope:` `coverage.config` (full read).
`SearchPatterns:` `KaStringAsync`, `KaChar`, `KaKey`, `KbdActions`, `IKbdAction`, `QuickFiler`
`SearchResult:` none.

**Confirmed: none of the five production files is excluded from coverage measurement.**

### 8.2 `KbdActions<>` is explicitly NOT exempt — confirmed

CLAUDE.md UT2, COM/VSTO/WinForms coverage exemption, final sentence:

> Testable seams within otherwise-COM-bound assemblies (e.g., `ToDoLoader`, `IDList` arithmetic,
> **`KbdActions<>`**, path/settings helpers) are explicitly NOT exempt and must meet the `>= 80%`
> floor.

**Confirmed verbatim.** `KbdActions<>` is named as a non-exempt testable seam. `KaStringAsync`,
`KaChar`, `KaKey`, and `IKbdAction` are pure value objects with no Outlook/COM dependency and fall
outside every limb (a), (b), (c) of the exemption, so they too are in the testable denominator.

The one exemption in this cluster is **`KeyboardHandler`**, which carries
`[ExcludeFromCodeCoverage]` at `KeyboardHandler.cs:22` — consistent with limb (c) (it depends on
`Microsoft.Office.Interop.Outlook` via `:15` and on WinForms event args).

### 8.3 Threshold divergence, recorded without adjudication

CLAUDE.md UT2 states a repository-wide line-coverage floor of **`>= 80%`** with **`>= 90%`** for new
modules/classes/methods. `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`
state **`>= 85%` line / `>= 75%` branch** uniformly across T1-T4. This divergence is pre-existing and
is not resolved by this issue. Under CLAUDE.md's Policy Compliance Order, CLAUDE.md is applied first.
The changes proposed here add tests and delete dead members, so coverage for the touched files should
rise under either figure; **no coverage exemption is sought or needed.**

---

## 9. Consolidated Change Set and Test Strategy

### 9.1 Files to change (five production files, one test file)

| # | File | Change | Defect |
|---|---|---|---|
| 1 | `QuickFiler/Controllers/KaStringAsync.cs:72` | add `Activated &&` to the branch-3 guard | 1 |
| 2 | `QuickFiler/Controllers/KaStringAsync.cs:57` (top of method) | guard clause rejecting empty (and optionally null) `other` | 2 |
| 3 | `QuickFiler/Controllers/KaStringAsync.cs:57` | XML doc recording the `Activated` latch contract and the empty-string contract | 1, 2 |
| 4 | `QuickFiler/Controllers/KaChar.cs:43-46` | delete `DelegateType`; also delete `using System.Windows.Forms;` at `:6` | 3 |
| 5 | `QuickFiler/Controllers/KaChar.cs:50-55`, `:92-97` | delete dead `Update` from `KaChar` and `KaCharAsync` | related |
| 6 | `QuickFiler/Controllers/KaKey.cs:43-46` | delete `DelegateType` | 3 |
| 7 | `QuickFiler/Controllers/KaKey.cs:50-55`, `:92-97` | delete dead `Update` from `KaKey` and `KaKeyAsync` | related |
| 8 | `QuickFiler/Interfaces/IKbdAction.cs:15-16` | delete both commented-out members | related |
| 9 | `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | rename one test (`:134`); add the four tests of section 6.3 | 1, 2 |

**Not changed:** `KbdActions.cs` (owned in part by #472/#482 in a later epic — do not do their work),
`KeyboardHandler.cs`, `QfcCollectionController.cs`, `QuickFiler.Test.csproj`, and the four other test
files.

### 9.2 Test strategy (no test code authored here, per research-only scope)

- **Framework:** MSTest `[TestClass]`/`[TestMethod]`, Moq where a mock is warranted, FluentAssertions
  for assertions (CLAUDE.md CUT1, CUT2). The existing `KaStringAsyncTests.NewKa` helper (`:20-25`)
  already supplies optional `update`/`toggle` callbacks and should be reused.
- **Bugfix ordering (CLAUDE.md Bugfix Workflow §1):** author the defect-1 and defect-2 regression
  tests **first** and observe them fail. Section 6.1 establishes both will be genuinely red, so a
  fail-before exception dossier is **not** required.
- **Determinism:** these are pure value objects. Assertions use simple captured locals, no clock, no
  timer, no temp file, no external dependency. `[ExcludeFromCodeCoverage]` on `KeyboardHandler` means
  no test should attempt to drive the branches through it; test `KaStringAsync` directly.
- **Scenario completeness (CLAUDE.md UT2):** for `KeyEquals`, cover each of the three branches at
  both `Activated` states and at both null/non-null `Update`, plus the empty-string and null
  boundaries. That is the matrix the current suite leaves half-covered.
- **Deletion safety:** the removals in changes 4-8 need no new test; their safety is established by
  the zero-read-site evidence in section 5 and is proven by the analyzer and nullable builds
  compiling.
- **Evidence:** baseline and QA-gate artifacts go to
  `docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/<kind>/`
  per `evidence-and-timestamp-conventions`, using the `yyyy-MM-ddTHH-mm` timestamp format.

### 9.3 Open decision for the planner

Section 4.3 recommends **Option 3 (throw `ArgumentException`)** for the empty-string contract, with
Option 1 (return `false`) as the fallback. Both are test-neutral against the existing suite. The
acceptance criterion at `issue.md:126-127` permits either. This is the one place where a reviewer
preference could reasonably override the recommendation.

### 9.4 Follow-up to raise as a separate issue

The non-prefix `Substring` defect of section 4.4 (`KaStringAsync.cs:62`). Out of scope for #445;
recommend promoting to a potential entry / GitHub issue rather than widening this bugfix.
