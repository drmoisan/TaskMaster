# ILGlobals static publication — fix design research (Issue #824)

Timestamp: 2026-09-08T23-45

- **Issue:** #824 — `ilglobals-loadopcodes-unsynchronised-static-race`
- **Branch:** `bug/ilglobals-loadopcodes-unsynchronised-static-race-824`
- **Primary source:** `docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/issue.md`
- **Scope:** research only. No source, test, or build file was modified.

All paths in this document are repository-relative. Every line citation was re-derived against the
current worktree during this research pass; none was carried over from the delegation prompt
unverified.

---

## 0. Current-state confirmation

`UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` (197 lines total; `#nullable enable` at
line 1) declares:

| Line | Declaration |
|---|---|
| 110 | `public static class ILGlobals` |
| 112 | `public static Dictionary<int, object> Cache = new Dictionary<int, object>();` |
| 114-116 | Comment describing the "populated by `LoadOpCodes()` before any read" invariant |
| 117 | `public static OpCode[] multiByteOpCodes = null!;` |
| 118 | `public static OpCode[] singleByteOpCodes = null!;` |
| 119 | `public static Module[]? modules = null;` |
| 121 | `public static void LoadOpCodes()` |
| 123 | `singleByteOpCodes = new OpCode[0x100];` |
| 124 | `multiByteOpCodes = new OpCode[0x100];` |
| 125-148 | Reflection loop over `typeof(OpCodes).GetFields()`; element writes at 137 and 145 |

There is no `lock`, no `Lazy<T>`, no `volatile`, and no explicit static constructor anywhere in the
file. The type therefore carries the `beforefieldinit` metadata flag today, because it has a static
field initializer (line 112) and no explicit `.cctor`.

Reader sites are `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs:105`
(`code = ILGlobals.singleByteOpCodes[(int)value];`) and `:110`
(`code = ILGlobals.multiByteOpCodes[(int)value];`), both unsynchronised, inside
`ConstructInstructions` (declared at `MethodBodyReader.cs:91`).

Assembly-level parallelism is confirmed at `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`
(`[assembly: Parallelize(Workers = 0, Scope = ... ExecutionScope.ClassLevel)]`).

File sizes relevant to placement decisions (measured by reading each file to its end):

| File | Lines | Headroom to the 500-line cap |
|---|---|---|
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | 197 | 303 |
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs` | 299 | 201 |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 133 | 367 |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs` | 489 | 11 |

`MethodBodyReader_Tests.cs` at 489 lines has effectively no headroom. Any new test must go into
`ILGlobals_Tests.cs` or a new file; it must not go into `MethodBodyReader_Tests.cs`.

---

## 1. Full call-site inventory

### 1.1 `ILGlobals.LoadOpCodes` — 6 sites (1 declaration, 5 invocations)

| File | Line | Kind |
|---|---|---|
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | 121 | Declaration |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 15 | Invocation (Act of `LoadOpCodes_Initializes_SingleByteOpCodes`) |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 26 | Invocation (Act of `LoadOpCodes_Initializes_MultiByteOpCodes`) |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 37 | Invocation (Act of `LoadOpCodes_PopulatesKnownSingleByteOpCodes`) |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 47 | Invocation (Act of `LoadOpCodes_PopulatesKnownOpCode_Ret`) |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs` | 364 | Invocation, inside `private static MethodBodyReader CreateReader(MethodInfo)` (declared at `:362`) |

**No production code calls `LoadOpCodes()`.** All five invocations are in test code.

### 1.2 `ILGlobals.singleByteOpCodes` — 7 sites

| File | Line | Kind |
|---|---|---|
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | 118 | Declaration |
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | 123 | **Reference assignment** (inside `LoadOpCodes`) |
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | 137 | Element write (inside `LoadOpCodes`) |
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs` | 105 | Read (indexer) |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 18 | Read (`.Should().NotBeNull()`) |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 19 | Read (`.Length`) |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 40 | Read (indexer `[0x00]`) |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 50 | Read (indexer `[0x2A]`) |

### 1.3 `ILGlobals.multiByteOpCodes` — 6 sites

| File | Line | Kind |
|---|---|---|
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | 117 | Declaration |
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | 124 | **Reference assignment** (inside `LoadOpCodes`) |
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | 145 | Element write (inside `LoadOpCodes`) |
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs` | 110 | Read (indexer) |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 29 | Read (`.Should().NotBeNull()`) |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 30 | Read (`.Length`) |

### 1.4 `ILGlobals.Cache` — 2 sites

| File | Line | Kind |
|---|---|---|
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | 112 | Declaration with initializer |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 130 | Read (`.Should().NotBeNull()`) |

`Cache` is never written after its initializer anywhere in the repository, and is never read by
production code.

### 1.5 `ILGlobals.modules` — 1 site

| File | Line | Kind |
|---|---|---|
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | 119 | Declaration with initializer |

`ILGlobals.modules` has **zero** references anywhere else in the repository. The `modules`
identifiers at `MethodBodyReader.cs:249`, `:250` and `:254` are a **local variable** declared at
`:249` (`Module[] modules = Assembly.Load(...).GetModules();`) inside `GetRefferencedOperand`, not
the `ILGlobals` field. This is an explicit exclusion in the cross-check enumeration below.

### 1.6 Assignment question (asked explicitly)

**No call site other than `LoadOpCodes()` itself assigns to `singleByteOpCodes` or
`multiByteOpCodes`.** The only writes in the entire repository are `ILGlobals.cs:123`, `:124`
(reference assignment) and `:137`, `:145` (element writes), all inside the body of `LoadOpCodes()`.
`Cache` and `modules` are assigned only by their field initializers at `:112` and `:119`.

**Consequence:** both opcode-table fields can be narrowed from `public static` mutable to
`public static readonly` with no caller edits at all, because every non-`LoadOpCodes` reference is a
read. Every one of the eight read sites (`MethodBodyReader.cs:105`, `:110`; `ILGlobals_Tests.cs:18`,
`:19`, `:29`, `:30`, `:40`, `:50`) compiles unchanged against a `readonly` field of the same name and
type. This is the single most important finding for the fix design.

---

## 2. Publication mechanism options

### 2.0 Toolchain constraints actually in force

| Property | Value | Source |
|---|---|---|
| `UtilitiesCS` `LangVersion` | `12.0` | `UtilitiesCS/UtilitiesCS.csproj:10` |
| `UtilitiesCS` `TargetFrameworkVersion` | `v4.8.1` | `UtilitiesCS/UtilitiesCS.csproj:16` |
| `UtilitiesCS.Test` `LangVersion` | `Latest` | `UtilitiesCS.Test/UtilitiesCS.Test.csproj:18` |
| `UtilitiesCS.Test` `TargetFrameworkVersion` | `v4.8.1` | `UtilitiesCS.Test/UtilitiesCS.Test.csproj:17` |

`LangVersion` is explicitly set in both projects, so no default applies. Nothing in the candidate
designs is gated on a C# version: `static readonly`, explicit static constructors, `lock`, and
`Lazy<T>` are all available at C# 12 / net481.

One net481 constraint is relevant to option (c): `System.Threading.Lock` (the C# 13 `lock` object
type that `.editorconfig:13` expresses a suggestion-level preference for) does not exist on .NET
Framework 4.8.1. Option (c) would have to use a plain `private static readonly object` gate.

### 2.a Static-constructor publication of `static readonly` fields — RECOMMENDED

Shape:

```
private static readonly OpCode[] single = new OpCode[0x100];   // local in the .cctor
// ... fill both locals from the reflection loop ...
// assign each public static readonly field exactly once at the end of the .cctor
```

with the public surface becoming `public static readonly OpCode[] singleByteOpCodes;` and
`public static readonly OpCode[] multiByteOpCodes;`.

- **Does it remove the observation window entirely?** Yes, on two independent grounds.
  1. **CLR type-initialization guarantee.** ECMA-335 Partition I §8.9.5 requires that a type
     initializer runs at most once for a type and that the runtime serialise concurrent triggering
     accesses, so a second thread that triggers initialization blocks until the initializer
     completes rather than observing the type mid-initialization. Combined with the completion
     barrier, a thread that reads `singleByteOpCodes` after initialization sees the fully written
     array.
  2. **Build-in-a-local-then-assign-once.** Assigning the field only after the local array is fully
     filled means that even a same-thread reentrant read during the `.cctor` (there is none here —
     the builder touches only `System.Reflection.Emit.OpCodes`) would observe `null`, never a
     partially filled array. This is the specific and only value of the "build in a local" step; it
     is defence against `.cctor` reentrancy and cross-type initializer cycles, not against ordinary
     cross-thread reads, which guarantee 1 already covers.
- **Public API surface change:** the field names, types, accessibility and read syntax are all
  unchanged. Only the `readonly` modifier is added. **Zero caller edits required** (see §1.6). This
  is a binary-compatible-shaped source change for readers and a compile-time break only for
  writers — and there are no writers outside `LoadOpCodes()`.
- **Residual honesty:** `readonly` on an array field prevents reassignment of the *reference*, not
  mutation of *elements*. `ILGlobals.singleByteOpCodes[5] = default;` remains legal for any caller.
  This is acceptable because the defect is specifically the reassign-then-fill window, and no code
  in the repository writes elements outside `LoadOpCodes()`. Full element immutability would require
  changing the public type to `IReadOnlyList<OpCode>` or `ReadOnlyCollection<OpCode>`, which *would*
  break the indexer reads at `MethodBodyReader.cs:105`/`:110` semantically (they would still compile
  for `IReadOnlyList<OpCode>`) and is a larger change than the defect warrants. Recommend recording
  it as an explicit non-goal rather than silently ignoring it.
- **`beforefieldinit`, precisely.** Adding an *explicit* static constructor clears the
  `beforefieldinit` type flag. What that buys and does not buy:
  - It does **not** add thread safety. The once-only + blocking guarantee applies to
    `beforefieldinit` types too. Anyone who claims `beforefieldinit` is the race is mistaken; the
    race here is that `LoadOpCodes()` is an ordinary method called repeatedly, not a type
    initializer at all.
  - It **does** change *when* initialization is triggered from "at or any time before the first
    access to any static field of the type" (relaxed, `beforefieldinit`) to "precisely at the first
    access to any static field, or the first call to any static or instance method or constructor of
    the type" (`ILGlobals` is a static class, so: first access to any static member). This is the
    property that makes an otherwise-empty `LoadOpCodes()` reliably force initialization — see §3.
  - The cost is that the JIT must honour an initialization check on static accesses it could
    otherwise elide. For a table read once per decoded IL instruction this is negligible and was not
    measured.
  - Alternative that keeps `beforefieldinit`: initialize each field from a static helper method in a
    field initializer (`public static readonly OpCode[] singleByteOpCodes = BuildSingleByteTable();`).
    Equally race-free, but it needs two reflection passes over `typeof(OpCodes).GetFields()` or an
    awkward shared-tuple field, and it breaks the guarantee that a call to `LoadOpCodes()` triggers
    initialization. Not recommended.

### 2.b `static readonly Lazy<OpCode[]>` with property accessors

- **Removes the window entirely?** Yes, if constructed with the default
  `LazyThreadSafetyMode.ExecutionAndPublication`. The factory runs once under a lock and the value is
  published only on completion.
- **API surface:** this is the larger change. Each field becomes a property
  (`public static OpCode[] singleByteOpCodes => _single.Value;`). Read *syntax* at all eight read
  sites is unchanged, so no caller edits are strictly required — but the repository naming rule
  `dotnet_naming_rule.non_field_members_should_be_pascal_case` (`.editorconfig:625-627`, applying to
  `property` at `:639`) means a property should be `SingleByteOpCodes`, which *would* require editing
  all eight read sites. Severity is `suggestion`, so it would not fail the build, but leaving a
  camelCase public property is a deliberate style deviation.
- **Cost:** two extra `Lazy<T>` allocations, a `.Value` indirection on the hot instruction-decode
  path at `MethodBodyReader.cs:105`/`:110`, and more moving parts than the problem needs. `Lazy<T>`
  earns its keep when initialization is expensive or may not be needed; here it is a single
  reflection pass over roughly 220 fields that every consumer needs.
- **Verdict:** correct but over-engineered for this defect. Rejected on Simplicity-first grounds
  (General Code Change Policy §1.1).

### 2.c `lock` in `LoadOpCodes()` plus `lock` around the reads in `MethodBodyReader`

- **Removes the window entirely?** **No — it only narrows it.** The fields remain `public static`
  and mutable, so the guarantee holds only for readers that opt into the same lock. The four
  unsynchronised test reads at `ILGlobals_Tests.cs:18`, `:19`, `:40`, `:50` and the two at `:29`,
  `:30` would still race against a concurrent `LoadOpCodes()` from `MethodBodyReader_Tests.cs:364`
  unless every one of them also took the lock. Any future reader that forgets the lock reintroduces
  the defect with no compile-time signal.
- **API surface:** the fields stay `public static` mutable, so nothing breaks — which is precisely
  the problem: the mutable window remains expressible.
- **Cost:** a lock acquisition per decoded IL instruction on the `ConstructInstructions` loop, plus
  a cross-type coupling (`MethodBodyReader` must know about `ILGlobals`'s lock object, or `ILGlobals`
  must expose accessor methods).
- **Verdict:** rejected. It suppresses the observed symptom without eliminating the class of defect,
  and it is strictly worse than (a) on every axis: more code, more coupling, worse performance,
  weaker guarantee.

### 2.d Recommendation

**Option (a):** convert both fields to `public static readonly`, populate them from an explicit
private static constructor that builds each table in a local and assigns each field exactly once.
It is the only option that (i) removes the window entirely, (ii) requires zero caller edits, (iii)
makes the defect *inexpressible* at compile time (any future assignment outside the `.cctor` is a
CS0198 error), and (iv) adds no runtime cost on the read path.

### Rejected alternatives (brief)

- `Lazy<OpCode[]>` pair with property accessors — correct, but adds indirection and a naming-rule
  decision for no benefit over (a).
- `lock` in `LoadOpCodes()` plus locked reads — narrows rather than removes; leaves the mutable
  public fields in place; costs a lock per decoded instruction.
- `volatile` on the fields — not evaluated in depth because it is unsound for this defect: `volatile`
  orders the *reference* publication but does nothing about the element writes at `ILGlobals.cs:137`
  and `:145` that happen after the reference is already published at `:123`/`:124`. Recording it
  explicitly so it is not revisited.

---

## 3. `LoadOpCodes()` disposition

Three candidate dispositions, against the five invocation sites in §1.1:

| Disposition | Effect on the 5 call sites | Assessment |
|---|---|---|
| **Remove entirely** | All 5 stop compiling; `MethodBodyReader_Tests.cs:364` inside `CreateReader` must be edited, and that file has 11 lines of headroom | Rejected. It forces an edit to the near-cap test file and destroys the only place where the "must not republish" invariant can be asserted (see §4, gate G1). |
| **Empty no-op body** | All 5 compile unchanged | Workable but weaker. An empty public method is a code smell (Sonar S1186 "Methods should not be empty", held at `suggestion` by `.editorconfig:241`, so it cannot fail the build), and it relies on the explicit `.cctor` to guarantee that calling it triggers initialization. |
| **Force type initialization** — body becomes `RuntimeHelpers.RunClassConstructor(typeof(ILGlobals).TypeHandle);` | All 5 compile unchanged | **Recommended.** One line, idempotent, self-documenting, does not depend on whether the type is `beforefieldinit`, and is not an empty method. |

**Recommendation:** keep `LoadOpCodes()` as a public method whose body forces type initialization.
This preserves source compatibility for all five call sites, keeps `MethodBodyReader_Tests.cs`
untouched (respecting its 11-line headroom), and — critically — keeps the method available as the
subject of the deterministic regression gate in §4.

### 3.1 What happens to the four `LoadOpCodes_*` tests

The four tests at `ILGlobals_Tests.cs:11-51` have `ILGlobals.LoadOpCodes();` as their entire Act:

| Test | Line | Current Assert |
|---|---|---|
| `LoadOpCodes_Initializes_SingleByteOpCodes` | 12 | `singleByteOpCodes` not null; `Length == 0x100` (`:18-19`) |
| `LoadOpCodes_Initializes_MultiByteOpCodes` | 23 | `multiByteOpCodes` not null; `Length == 0x100` (`:29-30`) |
| `LoadOpCodes_PopulatesKnownSingleByteOpCodes` | 34 | `singleByteOpCodes[0x00] == OpCodes.Nop` (`:40`) |
| `LoadOpCodes_PopulatesKnownOpCode_Ret` | 44 | `singleByteOpCodes[0x2A] == OpCodes.Ret` (`:50`) |

After the fix these four still **pass**, and their assertions are still **true statements about the
published tables** — so they are not literally vacuous. What they become is **mis-attributed**: the
Act no longer causes the asserted state; the type initializer does. A reader would reasonably
conclude that `LoadOpCodes()` is what populates the table, which is exactly the wrong mental model
after the fix and is how the current defect was written in the first place.

Recommended disposition of the four:

1. Drop `ILGlobals.LoadOpCodes()` from the Act of all four and rename them to describe what they
   actually assert about the published tables, for example
   `SingleByteOpCodes_IsPublishedWithFullLength`, `MultiByteOpCodes_IsPublishedWithFullLength`,
   `SingleByteOpCodes_ContainsNopAtZero`, `SingleByteOpCodes_ContainsRetAt0x2A`. Their Arrange
   becomes empty; the first read of the static field triggers initialization, which is the property
   under test.
2. Replace the two spot checks (`Nop`, `Ret`) with the exhaustive population test described in §4.3,
   which subsumes both and is a materially stronger assertion.
3. Add the identity-stability test of §4.1, which becomes the only test that meaningfully exercises
   `LoadOpCodes()` after the fix.

If instead `LoadOpCodes()` were removed entirely, all four tests plus `MethodBodyReader_Tests.cs:364`
would have to be edited to compile at all, and the "must not republish" invariant would have no
subject. That is the concrete reason to keep the method.

---

## 4. A deterministic test of the publication mechanism

The failure was observed **once in ten** consecutive full-suite runs at 24 class-level workers
(`issue.md:75-77`). Any acceptance condition keyed to a repeated-run count, or to the exit code of a
single run of a known-intermittent test, cannot fail in one direction and is worthless as a gate.
The gate must test the *publication property*, not sample the race.

Required property of an acceptable gate, stated up front: **it must FAIL deterministically against
the current unfixed tree, in every possible test-execution ordering, and PASS deterministically
against the fixed tree.**

### 4.1 Gate G1 (behavioural, PRIMARY) — `LoadOpCodes()` does not republish the tables

```
// Arrange
var singleBefore = ILGlobals.singleByteOpCodes;
var multiBefore = ILGlobals.multiByteOpCodes;

// Act
ILGlobals.LoadOpCodes();

// Assert
ILGlobals.singleByteOpCodes.Should().BeSameAs(singleBefore);
ILGlobals.multiByteOpCodes.Should().BeSameAs(multiBefore);
```

**Does it fail on the current tree?** Yes, deterministically, and — this is the load-bearing
point — **in both possible orderings**:

- If no other class has yet called `LoadOpCodes()` in the process, `singleBefore` is `null`
  (`ILGlobals.cs:118` initializes to `null!`) and the post-Act value is a fresh non-null array
  allocated at `ILGlobals.cs:123`. `BeSameAs(null)` fails.
- If another class has already called `LoadOpCodes()`, `singleBefore` is a non-null array and the
  Act allocates a *different* array at `ILGlobals.cs:123`. `BeSameAs` fails on reference identity.

There is no third ordering. The test is therefore order-independent RED on the unfixed tree.

**Does it pass on the fixed tree?** Yes. Reading `ILGlobals.singleByteOpCodes` in Arrange triggers
the type initializer (first access to a static field of a non-`beforefieldinit` type), producing a
non-null array. `LoadOpCodes()` then only forces an already-completed initialization, so the
post-Act reference is identical. Deterministic GREEN in every ordering.

**Policy conformance:** single-threaded, no sleep, no retry, no timing tolerance, no temporary file,
no external dependency, no mutable global state written by the test. MSTest + FluentAssertions per
`CUT1`/`CUT2`. It directly encodes the invariant whose violation is the defect.

**This is the recommended primary gate.**

### 4.2 Gate G2 (structural, SECONDARY) — the fields are `InitOnly`

```
var single = typeof(ILGlobals).GetField(nameof(ILGlobals.singleByteOpCodes),
    BindingFlags.Public | BindingFlags.Static);
single.Should().NotBeNull();
single!.IsInitOnly.Should().BeTrue("...");   // equivalently: Attributes.HasFlag(FieldAttributes.InitOnly)
```
plus the same for `multiByteOpCodes`.

**Does it fail on the current tree?** Yes, deterministically. `ILGlobals.cs:117-118` declare plain
mutable fields, so `IsInitOnly` is `false` and `Attributes` does not carry `FieldAttributes.InitOnly`.

**Does it pass on the fixed tree?** Yes, deterministically, once the fields are `static readonly`.

**What it buys beyond G1:** G1 proves the *current* implementation does not republish. G2 proves a
*future edit cannot* reintroduce a mutable window without either failing this test or deliberately
deleting it. Note that the real protection is the compiler: with `readonly`, an assignment outside
the `.cctor` is CS0198 and the build fails before any test runs. G2 is the machine-checkable evidence
of that protection inside the test suite, and it is what catches a silent revert of the `readonly`
modifier.

I verified there is **no existing precedent** for a reflection-over-`FieldAttributes` structural test
in this repository: a Grep for `IsInitOnly|FieldAttributes\.InitOnly|IsLiteral` across all `*.cs`
files returned zero matches. G2 would be the first of its kind here. That is not an objection, but it
is worth stating so the reviewer is not looking for a house style that does not exist.

**Do NOT extend G2 to assert `!typeof(ILGlobals).Attributes.HasFlag(TypeAttributes.BeforeFieldInit)`.**
Such an assertion would also go RED-then-GREEN (the type is `beforefieldinit` today, and an explicit
`.cctor` clears the flag), but it over-constrains the implementation: the field-initializer form of
§2.a is equally race-free and would fail that assertion for no correctness reason. Gate on the
property that matters, not on the chosen encoding of it.

### 4.3 Supporting test G3 (NOT a gate) — exhaustive opcode population

Enumerate every `public static` field of type `OpCode` on `typeof(OpCodes)` by reflection; for each,
assert that `singleByteOpCodes[value]` (when `value < 0x100`) or `multiByteOpCodes[value & 0xff]`
(when `(value & 0xff00) == 0xfe00`) equals that opcode.

**Does it fail on the current tree? NO.** On the unfixed tree, a single-threaded test observes a
fully populated table (either the class's own `LoadOpCodes()` call, or a prior class's, has already
completed). It passes on the buggy code. **It is therefore not a gate for this defect and must not be
presented as one.** Its value is different and real: it is the correct replacement for the two
weak spot-checks at `ILGlobals_Tests.cs:40` and `:50`, and it is what would catch a *botched fix*
that publishes the tables safely but fills them wrongly. Include it; do not count it as the gate.

### 4.4 Rejected: the concurrency-sampling test

A test that spins N threads through `LoadOpCodes()` and/or the accessors and asserts every
observation is fully populated is **disqualified as the primary gate**, for three independent
reasons:

1. **It does not discriminate.** Against the current tree it passes most of the time. The observed
   base rate of the real race is 1 in 10 *full-suite* runs; a bounded in-test thread burst offers no
   guarantee of hitting the window even once. The test would report GREEN on defective code, which
   is the exact failure mode the delegation prompt warns against.
2. **Making it fail more often requires banned mechanisms.** Raising the hit rate means iteration
   counts tuned by observation, spin loops, or `Thread.Sleep`/`Task.Delay` to widen the window —
   all prohibited by `.claude/rules/general-unit-test.md` ("Banned APIs in test code") and by the
   instruction that no sleep, retry, or timing tolerance may be used to stabilise a test. A
   `Barrier`/`ManualResetEventSlim` start gate is not itself a sleep, but it does not make the
   discrimination deterministic either; the outcome still depends on interleaving luck.
3. **It asserts nothing after the fix.** With the tables published by the type initializer,
   `LoadOpCodes()` is a forced-initialization call and every concurrent observation is trivially
   complete. The test would be permanently, uninformatively green.

**Disqualified.** Do not include it.

### 4.5 Considered and set aside: fresh-`AppDomain` behavioural gate

.NET Framework 4.8.1 supports `AppDomain.CreateDomain`, and statics are per-AppDomain. A test that
creates a child domain and, inside it, reads `ILGlobals.singleByteOpCodes[0x72]` **without** calling
`LoadOpCodes()` would deterministically throw `NullReferenceException` on the current tree and
succeed on the fixed tree. It is genuinely deterministic and order-independent.

It is set aside in favour of G1 because it costs materially more (a `MarshalByRefObject` helper type,
cross-domain assembly resolution, and unverified interaction with `/EnableCodeCoverage`
instrumentation and `/InIsolation` under vstest) for a strictly weaker discrimination than G1 already
provides at near-zero cost. I did not execute this approach, so its behaviour under the repository's
coverage collector is **unverified**; do not adopt it without a spike.

### 4.6 Summary of gate proposals

| Proposal | Deterministic | Fails on current tree | Passes on fixed tree | Verdict |
|---|---|---|---|---|
| G1 identity stability (§4.1) | Yes | Yes, in every ordering | Yes | **Primary gate** |
| G2 `IsInitOnly` structural (§4.2) | Yes | Yes | Yes | **Secondary gate** (anti-regression) |
| G3 exhaustive population (§4.3) | Yes | **No** | Yes | Supporting test only, not a gate |
| Concurrency sampling (§4.4) | No | Stochastically | Yes | **Disqualified** |
| Fresh-AppDomain read (§4.5) | Yes | Yes | Yes | Viable but unverified; not recommended |

Placement: G1, G2 and G3 all belong in
`UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` (133 lines, 367 lines of
headroom). Adding them there requires **no `.csproj` edit**, because that file is already listed at
`UtilitiesCS.Test/UtilitiesCS.Test.csproj:264`. Nothing should be added to
`MethodBodyReader_Tests.cs` (489 lines).

---

## 5. `[DoNotParallelize]` assessment

**Recommendation: do not add `[DoNotParallelize]` to `ILGlobals_Tests` or `MethodBodyReader_Tests`.**
It is not necessary once the publication fix lands, and adding it would create exactly the kind of
undocumented, unjustified attribute that sibling feature 825 exists to remove.

Reasoning, from the verified inventory in §1:

1. After the fix, the two opcode tables are assigned exactly once, by the type initializer, under the
   CLR's once-only-with-blocking guarantee. There is no window in which one class's activity can be
   observed by another mid-write.
2. `ILGlobals.Cache` (`ILGlobals.cs:112`) is public static mutable, but it is **read** at exactly one
   place in the repository (`ILGlobals_Tests.cs:130`) and **written** nowhere after its initializer.
   Neither test class nor `MethodBodyReader` mutates it. It is not a live race between these two
   classes.
3. `ILGlobals.modules` (`ILGlobals.cs:119`) has zero references anywhere. It cannot participate in a
   race.
4. `MethodBodyReader` instances are per-test (`MethodBodyReader_Tests.cs:365`), and the private
   helpers `SetIl` (`:453`) and `InvokePrivate` (`:438`) mutate only the instance under test.

So after the fix there is **no shared mutable state remaining between `ILGlobals_Tests` and
`MethodBodyReader_Tests`**, and no verified reason for `[DoNotParallelize]` exists. If it were added
anyway, feature 825 would be correct to sweep it, and I can state no documented reason that would
survive that sweep.

Two caveats recorded honestly:

- Neither class carries `[DoNotParallelize]` today, so this is a recommendation *not to add* one, not
  a recommendation to remove one. No overlap with 825's write set.
- `ILGlobals.Cache` remains an unsynchronised public mutable static of the same shape as the fields
  being fixed. It is dormant, not safe. Recommend filing it as a separate follow-up (dead/unsafe
  public static surface: `Cache` at `:112` unused by production, `modules` at `:119` entirely unused)
  rather than expanding #824's scope, consistent with the Bugfix Workflow rule to open a new issue
  instead of widening scope.

---

## 6. Nullable and analyzer implications

### 6.1 Annotations and comment

`ILGlobals.cs:1` is `#nullable enable`. Lines 114-116 currently read:

```
// Invariant: these are populated by LoadOpCodes() before any read of the tables;
// annotated null! (rather than nullable) to preserve the non-null contract that
// consumers (e.g. MethodBodyReader) already rely on. Behavior unchanged.
```

Both parts of that comment become **false** under the fix: the tables are no longer populated by
`LoadOpCodes()`, and no `null!` suppression remains. The `= null!` initializers at `:117` and `:118`
must be deleted, not merely re-annotated.

Recommended replacement shape (documentation content, not final formatting — CSharpier owns
formatting):

```
/// <summary>
/// Multi-byte (0xFE-prefixed) opcode table, indexed by the low byte of the opcode value.
/// Published once by the static constructor and never reassigned. <c>readonly</c> prevents
/// reassignment of the reference; it does not prevent element mutation, so callers must treat
/// the contents as read-only.
/// </summary>
public static readonly OpCode[] multiByteOpCodes;
public static readonly OpCode[] singleByteOpCodes;
```

No nullable suppression is required: a non-nullable `static readonly` field that is definitely
assigned in the static constructor satisfies the compiler's null-state analysis, so CS8618 is not
raised. That is a claim about C# definite-assignment rules for static constructors; it was not
verified by running a build in this research pass, and the executor should confirm it at the
`/p:TreatWarningsAsErrors=true` gate.

`ILGlobals.cs:119` (`public static Module[]? modules = null;`) needs no change for this fix. It is
already correctly annotated nullable; it is simply unused (§1.5).

### 6.2 Analyzer rules

**CA2211 does not currently fire and would not newly fire.** `UtilitiesCS.csproj` references exactly
five analyzer assemblies, at `UtilitiesCS.csproj:1308-1317`:

- `Meziantou.Analyzer.3.0.203`
- `Roslynator.Analyzers.5.0.0` (four assemblies)
- `AsyncFixer.2.1.0`
- `Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0` (two assemblies)
- `SonarAnalyzer.CSharp.10.33.0.1635`

There is **no** `Microsoft.CodeAnalysis.NetAnalyzers` reference, and the project declares no
`EnableNETAnalyzers`, `AnalysisLevel` or `AnalysisMode` property (Grep over
`UtilitiesCS/UtilitiesCS.csproj` for those names returned no matches). `UtilitiesCS.csproj` is a
legacy non-SDK project, so the `/p:EnableNETAnalyzers=true` switch in the repository toolchain
command has no analyzer assembly to enable for it — that property is honoured by the .NET SDK
targets, which this project does not import. Consequently no `CA`-prefixed diagnostic can be produced
for this file. I did not run a build to confirm this empirically; the conclusion rests on the absence
of any `CA` analyzer assembly in the `<Analyzer Include>` list.

**Configured severity for rules that could apply to public mutable static fields.** There is no
`.globalconfig` in the repository (Glob for `**/.globalconfig` returned no files). The only
configuration is `.editorconfig`, where:

- `.editorconfig:27` — `dotnet_analyzer_diagnostic.severity = suggestion` is a global catch-all that
  holds **every** analyzer diagnostic at `suggestion` unless a more specific rule ID overrides it.
- `.editorconfig:29` — `dotnet_diagnostic.MSTEST0032.severity = warning` is the **only** rule
  configured above `suggestion` anywhere in the file. It is an MSTest assertion-argument rule and is
  irrelevant here.
- Neither `S2223` ("Non-constant static fields should not be visible") nor `S1104` ("Fields should
  not have public accessibility") appears explicitly in `.editorconfig` (Grep for `S2223|S1104|MA0069`
  matched only `MA0069` at `:99` and `RCS1104` at `:389`, neither of which is the rule in question).
  Both are therefore governed by the catch-all at `:27` and held at `suggestion`.

**Net effect:** no analyzer rule concerning public mutable static fields can fail the
`/p:EnableNETAnalyzers=true` or `/p:TreatWarningsAsErrors=true` gates on the current tree, and none
will newly fire under the fix. If anything, adding `readonly` makes the fields *less* likely to be
flagged by S2223/S1104-class rules. I did not verify whether S2223 or S1104 are enabled-by-default in
SonarAnalyzer 10.33.0.1635; the catch-all severity makes that question moot for the build gate.

One rule worth naming for the §3 decision: Sonar **S1186 "Methods should not be empty"** is
explicitly configured at `suggestion` (`.editorconfig:241`). An empty `LoadOpCodes()` body would not
fail the build. The `RuntimeHelpers.RunClassConstructor` body recommended in §3 sidesteps the
question entirely.

---

## 7. Build-file impact

Both projects are legacy non-SDK with explicit `<Compile Include>` items. Existing entries for the
four in-scope files, reproduced verbatim (four-space indent, backslash separators, self-closing tag
with a single leading space before `/>`; the production folder name contains a space):

```
UtilitiesCS/UtilitiesCS.csproj:799
    <Compile Include="NewtonsoftHelpers\SDIL Reader\ILGlobals.cs" />

UtilitiesCS/UtilitiesCS.csproj:801
    <Compile Include="NewtonsoftHelpers\SDIL Reader\MethodBodyReader.cs" />

UtilitiesCS.Test/UtilitiesCS.Test.csproj:264
    <Compile Include="NewtonsoftHelpers\SDILReader\ILGlobals_Tests.cs" />

UtilitiesCS.Test/UtilitiesCS.Test.csproj:535
    <Compile Include="NewtonsoftHelpers\SDILReader\MethodBodyReader_Tests.cs" />
```

Note the asymmetry in folder naming, which is easy to get wrong: the **production** folder is
`SDIL Reader` (with a space), the **test** folder is `SDILReader` (no space).

**If a new test file is added** — for example `ILGlobalsPublication_Tests.cs` — the required new
entry text, matching the surrounding style verbatim, is:

```
    <Compile Include="NewtonsoftHelpers\SDILReader\ILGlobalsPublication_Tests.cs" />
```

Recommended insertion point: immediately after `UtilitiesCS.Test.csproj:265`
(`<Compile Include="NewtonsoftHelpers\SDILReader\ILInstruction_Tests.cs" />`), which is where the
other `SDILReader` entries are grouped. The item list is **not** globally sorted — note that
`MethodBodyReader_Tests.cs` sits far away at `:535` — so alphabetical placement across the whole list
is not a constraint; local grouping is.

**Preferred outcome: no csproj edit at all.** `ILGlobals_Tests.cs` has 367 lines of headroom and is
already registered at `:264`. Putting G1, G2 and G3 there avoids touching either build file. Adding a
new file is the fallback only if the executor judges the cohesion of `ILGlobals_Tests.cs` to be at
risk.

No change is required to `UtilitiesCS.csproj`: the fix modifies `ILGlobals.cs` in place and adds no
new production file.

---

## 8. Coverage

### 8.1 Negative claim, with search scope

**No coverage artifact for issue #824 exists in this worktree.** Auditable search scope and patterns:

| Search | Tool and expression | Result |
|---|---|---|
| Feature folder contents | Glob `docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/**` | 3 files: `issue.md`, `spec.md`, `plan.2026-09-08T23-51.md`. No `evidence/` directory, no coverage file. |
| Repository coverage staging directory | Glob `coverage/**` | 1 entry: `coverage\.gitkeep`. Empty of artifacts. |
| All coverage XML in the tree | Glob `**/*coverage*.xml` | Over 100 matches, all under `docs/features/{active,archive}/<other-feature>/evidence/...`. None under the #824 folder. |
| Cobertura documents naming the type | Grep `ILGlobals` with glob `*.cobertura.xml` | Matches only in other features' evidence folders. |
| Cobertura documents naming the namespace | Grep `SDIL` with glob `*.xml` | Matches only in other features' evidence folders. |

A fresh baseline must therefore be captured for #824 before any change; the figures below are
indicative context only, not a baseline.

### 8.2 Nearest committed measurement

The most recent committed repository-wide Cobertura document containing these classes is
`docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/coverage-final.cobertura.xml`:

| Line | Element | line-rate | branch-rate |
|---|---|---|---|
| 34098 | `<class name="SDILReader.ILGlobals" filename="UtilitiesCS\NewtonsoftHelpers\SDIL Reader\ILGlobals.cs">` | 0.9459459459459459 | 0.875 |
| 34100 | `<method name="LoadOpCodes" signature="()">` | 0.9166666666666666 | 0.875 |
| 34508 | `<class name="SDILReader.MethodBodyReader" filename="UtilitiesCS\NewtonsoftHelpers\SDIL Reader\MethodBodyReader.cs">` | 0.9732620320855615 | 0.8947368421052632 |

Caveats, stated so this is not mistaken for a baseline:

- The artifact was produced by a different feature's run at a different commit (feature 729, dated
  2026-09-02). It is not this branch's baseline.
- Per repository memory on Cobertura arithmetic, raw and post-processed root totals are not
  comparable; these are per-`class` `line-rate` attributes read directly from the document, which is
  the comparable axis.
- The method line numbers in that document (`LoadOpCodes` lines beginning at 122-123) are consistent
  with the current `ILGlobals.cs`, so the file has not moved since that run.

### 8.3 Coverage impact of the recommended fix

Both files already sit well above the repository floors. The fix moves the reflection loop from
`LoadOpCodes()` into a static constructor. Static constructors are emitted as a `.cctor` method and
are executed exactly once per process, so the moved lines should remain covered; the residual
`LoadOpCodes()` body (a single `RunClassConstructor` call) is exercised by all five existing call
sites plus G1. Adding G1, G2 and G3 raises the executed-line count for `ILGlobals.cs` rather than
lowering it. No coverage regression is anticipated, but this is a prediction, not a measurement, and
must be confirmed against a fresh baseline/post-change pair.

---

## Numeric Derivation Evidence

Numeric claims made in §1 that could be promoted into a `spec.md` acceptance criterion:

- N1: `LoadOpCodes` has exactly **5** invocation sites, all in test code, and **0** in production code.
- N2: `singleByteOpCodes` and `multiByteOpCodes` are assigned at exactly **4** sites
  (`ILGlobals.cs:123`, `:124`, `:137`, `:145`), all inside `LoadOpCodes()`, and at **0** sites
  outside it.
- N3: `ILGlobals.modules` has exactly **0** references outside its declaration.

### Record for N1, N2, N3

- **Complete Family:** every textual reference in the repository to any of the five `ILGlobals`
  members named in the delegation prompt — `LoadOpCodes`, `singleByteOpCodes`, `multiByteOpCodes`,
  `Cache`, `modules` — in both production and test code, including the declarations themselves.
  `LoadOpCodes` has exactly one signature (`()`), confirmed at `ILGlobals.cs:121`; there are no
  overloads to miss. The fields are single declarations with no partial-class duplicates: a Glob for
  `**/ILGlobals*.cs` returned exactly two files (`UtilitiesCS\NewtonsoftHelpers\SDIL Reader\ILGlobals.cs`
  and `UtilitiesCS.Test\NewtonsoftHelpers\SDILReader\ILGlobals_Tests.cs`), so there is only one
  declaring file.
- **Exhaustive Search Scope:** all `*.cs` files in the worktree, unrestricted by directory, covering
  every project (production and test) in the solution. Both searches used `head_limit: 0`
  (unlimited), so neither enumeration was truncated.
- **Inclusion Rules:** any line containing a reference to one of the five member names, whether
  qualified (`ILGlobals.X`) or unqualified (in-class use inside `ILGlobals.cs`); declaration lines
  included; comment lines mentioning a member name included and then classified.
- **Exclusion Rules:** non-`.cs` artifacts are excluded from the member-reference counts —
  specifically `UtilitiesCS.Test\UtilitiesCS.Test.csproj:264` and `UtilitiesCS\UtilitiesCS.csproj:799`
  (build-file `<Compile Include>` entries, not code references), `test-output.txt:1606-1609` (a
  captured vstest log), and `scripts\temp-extract-coverage.ps1:30` (a PowerShell helper). Also
  excluded: the **local variable** `modules` declared at `MethodBodyReader.cs:249` and used at `:250`
  and `:254`, which is `Module[] modules = Assembly.Load(...).GetModules();` and is unrelated to
  `ILGlobals.modules`. Also excluded from the reference counts: `ILGlobals.cs:114`, a comment line
  mentioning `LoadOpCodes()`.
- **Primary Search Strategy or Query Expression:** Grep, content mode, pattern
  `LoadOpCodes|singleByteOpCodes|multiByteOpCodes|ILGlobals`, glob `*.cs`, `head_limit: 0`. This is a
  name-alternation search over the whole `.cs` corpus.
- **Primary Member Set:**
  - `LoadOpCodes` — decl `ILGlobals.cs:121`; invocations `ILGlobals_Tests.cs:15,26,37,47`,
    `MethodBodyReader_Tests.cs:364`; comment mention `ILGlobals.cs:114`.
  - `singleByteOpCodes` — `ILGlobals.cs:118,123,137`; `MethodBodyReader.cs:105`;
    `ILGlobals_Tests.cs:18,19,40,50`.
  - `multiByteOpCodes` — `ILGlobals.cs:117,124,145`; `MethodBodyReader.cs:110`;
    `ILGlobals_Tests.cs:29,30`.
  - (`Cache` and `modules` are not matched by this expression and are covered by the cross-check.)
- **Primary Count:** `LoadOpCodes` invocations = 5 (4 in `ILGlobals_Tests.cs`, 1 in
  `MethodBodyReader_Tests.cs`), production invocations = 0. Assignments to the two tables = 4
  (`:123`, `:124`, `:137`, `:145`), all within `LoadOpCodes()`; assignments outside `LoadOpCodes()` = 0.
- **Cross-check Search Strategy or Query Expression:** two structurally different expressions, run
  independently of the primary:
  1. Grep, content mode, pattern `ILGlobals\s*\.` (member-access on the type, whitespace-tolerant),
     glob `*.cs`, `head_limit: 0`. This enumerates the complete family of *qualified* member accesses
     regardless of member name, so it cannot miss a member the primary alternation forgot to list.
  2. Grep, content mode, pattern `\b(singleByteOpCodes|multiByteOpCodes|LoadOpCodes|Cache|modules)\b`
     with word boundaries, glob `**/SDIL*/*.cs`, `head_limit: 0`. This enumerates the complete family
     of *unqualified* in-class uses inside both SDIL folders, which a qualified-access search cannot
     see.
- **Cross-check Member Set:**
  - From expression 1 (qualified accesses, 27 lines): `LoadOpCodes` at `MethodBodyReader_Tests.cs:364`
    and `ILGlobals_Tests.cs:15,26,37,47`; `singleByteOpCodes` at `ILGlobals_Tests.cs:18,19,40,50` and
    `MethodBodyReader.cs:105`; `multiByteOpCodes` at `ILGlobals_Tests.cs:29,30` and
    `MethodBodyReader.cs:110`; `Cache` at `ILGlobals_Tests.cs:130`; `ProcessSpecialTypes` at
    `ILGlobals_Tests.cs:57,67,77,87,97,107,120` and `ILInstruction.cs:60,62,76,78,94,109`. **No
    qualified reference to `ILGlobals.modules` exists anywhere.**
  - From expression 2 (unqualified, both SDIL folders): adds the declaration sites
    `ILGlobals.cs:112` (`Cache`), `:117`, `:118`, `:119` (`modules`), `:121`, the in-class writes
    `:123`, `:124`, `:137`, `:145`, and the comment at `:114`; plus the excluded local-variable
    matches at `MethodBodyReader.cs:249,250,254`.
- **Cross-check Count:** `LoadOpCodes` invocations = 5; production invocations = 0. Table assignments
  = 4, all inside `LoadOpCodes()`; outside = 0. `ILGlobals.modules` references outside its
  declaration at `:119` = 0.
- **Member-set Comparison:** the normalized union of the two cross-check member sets is identical to
  the primary member set for `LoadOpCodes`, `singleByteOpCodes` and `multiByteOpCodes` — same files,
  same line numbers, no additions and no omissions in either direction. The cross-check additionally
  covers `Cache` (2 sites: `ILGlobals.cs:112`, `ILGlobals_Tests.cs:130`) and `modules` (1 site:
  `ILGlobals.cs:119`), which the primary alternation did not target by name; those two members are
  reported only from the cross-check and are not in dispute between the two records. The three
  documented exclusions (build-file entries, the vstest log and PowerShell helper, and the
  `MethodBodyReader.cs:249` local variable) account for every match present in the broader
  unrestricted search but absent from the counts above. **The two records agree. N1, N2 and N3 are
  safe to assert.**

---

## 9. Consolidated recommendation

1. **Publication (§2):** convert `multiByteOpCodes` and `singleByteOpCodes` at `ILGlobals.cs:117-118`
   to `public static readonly`, populated by an explicit private static constructor that builds each
   table into a local and assigns each field exactly once. Zero caller edits. Delete the `= null!`
   suppressions and rewrite the comment at `:114-116`.
2. **`LoadOpCodes()` (§3):** keep it public; body becomes
   `RuntimeHelpers.RunClassConstructor(typeof(ILGlobals).TypeHandle);`. All five call sites compile
   unchanged and `MethodBodyReader_Tests.cs` (489/500 lines) is not touched.
3. **Gates (§4):** G1 identity-stability is the primary deterministic gate — it fails on the current
   tree in every execution ordering and passes on the fixed tree. G2 `IsInitOnly` is the
   anti-regression structural gate. G3 exhaustive population is a supporting test, explicitly not a
   gate because it passes on the unfixed tree. The concurrency-sampling test is disqualified.
4. **`[DoNotParallelize]` (§5):** do not add it. After the fix, no shared mutable state remains
   between the two classes, and no reason exists that would survive feature 825's sweep.
5. **Out of scope, worth filing separately (§5):** `ILGlobals.Cache` (`:112`) is an unsynchronised
   public mutable static read by exactly one test and unused by production; `ILGlobals.modules`
   (`:119`) is entirely unreferenced. Both are latent, not live. File as a follow-up rather than
   widening #824.
