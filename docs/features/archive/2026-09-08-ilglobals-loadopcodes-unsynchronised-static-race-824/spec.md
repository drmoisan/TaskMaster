# 2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race (Spec)

- **Issue:** #824
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-09T00-10
- **Status:** Approved
- **Version:** 0.1

> Work Mode for this feature is `full-bug`. This document is the sole authoritative
> acceptance-criteria source. No `user-story.md` exists for #824 and none is to be created.

## Context
An unsynchronised static-mutation race in `ILGlobals.LoadOpCodes()` is the sole reason acceptance
criterion AC4 of issue #811 is unmet. `LoadOpCodes()` reassigns a shared static opcode table to a
freshly allocated, all-default array and then fills it in a loop with no lock, no `Lazy<T>`, no
`volatile`, and no static constructor. Two MSTest classes call it from class-level-parallel test
runs with no `[DoNotParallelize]` guard, so a reader on one worker can observe the table between
the reassignment and the fill. This entry records the defect so it can be tracked and fixed under
its own issue; no source change is made on the #811 branch.

Environment:
- OS/version: Windows 11 Pro 10.0.26200 locally; GitHub Actions windows runner in CI
- Runtime: .NET Framework 4.8.1 test host, VSTest 18.9.0, MSTest 4.4.0, class-level parallelism
- Command/flags used: `vstest.console.exe <nine assemblies> /EnableCodeCoverage /InIsolation`
- Data source or fixture: no live Outlook or filesystem dependency; the race is between two MSTest
  classes running under `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: while this is open, the required `mstest-coverage` check can still fail intermittently on
unrelated pull requests, which is the class of harm issue #811 exists to remove. The observed
frequency is low (once in ten runs) and no production code path is affected, but the failure is
non-deterministic and its recorded signature could be mistaken for a genuine `MethodBodyReader`
defect by a future investigator who does not have this entry.

This feature is child F824 of the epic `review-residuals-2026-09-08`. Its design input is the
completed research record at
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/research/ilglobals-static-publication-2026-09-08T23-45.md,
whose recommendations were reviewed and accepted. Every line citation in this document was
re-derived against the current worktree while authoring the spec.

## Repro & Evidence
Steps to Reproduce:
Read the cited lines below. The failure is intermittent and was observed once in ten consecutive
full-suite runs at 24 class-level workers; it does not reproduce deterministically on demand.

Expected:
1. `ILGlobals`'s opcode tables are either published once, immutably, before any reader can observe
   them, or every read and write is synchronised.
2. Two MSTest classes that both call `LoadOpCodes()` do not run concurrently unless the shared
   state they mutate is safe under concurrent access.
3. A test failure caused by a partially filled table is distinguishable, at the point it is first
   investigated, from a genuine `MethodBodyReader` defect.

Actual:
1. **Mechanism.** `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`:123 reassigns
   `singleByteOpCodes` to a freshly allocated all-default `OpCode[0x100]` and then fills it in a
   reflection loop at line 137. Lines 117-118 declare both opcode tables as plain mutable
   `public static` fields. There is no `lock`, no `Lazy<T>`, no `volatile`, and no static
   constructor guarding either table.

2. **Reader sites.** `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs`:105 and :110
   read the tables with no synchronisation, inside `ConstructInstructions` declared at :91.

3. **Participants.** `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` calls
   `LoadOpCodes()` at lines 15, 26, 37, 47. `MethodBodyReader_Tests.cs` calls it at line 364 inside
   the private `CreateReader` helper (declared at :362) that every test in that class routes
   through. Neither class carries `[DoNotParallelize]`, and the assembly runs at
   `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`
   (`UtilitiesCS.Test/Properties/AssemblyInfo.cs`:18-21). `ClassLevel` scope serialises tests
   within a class, so the second participant in any observed race must be the sibling class.

4. **Observed signature (AC4 run 7).** `GetBodyCode_ReturnsConcatenatedInstructions` failed with
   `Expected bodyCode ... to contain "ldstr"`. Only the entry at IL offset 0001 (`ldstr`, opcode
   `0x72`) degraded; `nop`, `stloc.0`, `br.s`, `ldloc.0` and `ret` all resolved correctly. The two
   spaces where the opcode name belongs are the empty `Name` of `default(OpCode)`, whose
   `OperandType` is `InlineBrTarget` (0), which drives the numeric-operand branch and stores a raw
   metadata token instead of calling `module.ResolveString(...)`. The printed value 1879067923 lies
   in the `0x70……` String-metadata-token range, so the token was read correctly and merely not
   resolved. That combination is explained only by a partially filled table read
   mid-initialisation, not by a defect in token resolution itself.

5. **Frequency.** Observed once in ten consecutive full-suite runs at 24 class-level workers. The
   base rate is not established, and two clean baseline runs do not establish absence.

## Scope & Non-Goals

> Path-formatting convention for this document: a repository path appears inside a Markdown code
> span only when this feature owns it. Every out-of-scope path below is written deliberately as
> bare prose. Do not "fix" that formatting; a downstream tool derives this feature's change
> footprint by harvesting backticked path tokens, and backticking a sibling-owned path would widen
> the apparent blast radius of the epic fan-in.

- In scope — files owned by F824:
  - `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` — modified. Field declarations,
    the comment above them, and the body of `LoadOpCodes()`.
  - `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` — modified. New gate and
    supporting tests; the four existing `LoadOpCodes_*` tests reworked.
  - `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs` — owned so no sibling edits it
    concurrently; expected to be unchanged by this fix (see AC9).
  - `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs` — owned; expected to
    be unchanged. It is 489 lines against the 500-line repository cap, so it has 11 lines of
    headroom and must not absorb new tests.
  - `UtilitiesCS.Test/Properties/AssemblyInfo.cs` — owned; expected to be unchanged. The
    assembly-level `Parallelize` attribute at :18-21 is not weakened, narrowed, or removed.
  - `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — conditionally modified, and only if the executor
    adds a new test file rather than extending the existing test class. The preferred outcome is
    no edit at all (see Data / API / Config Impact).

- Out of scope / non-goals — sibling-owned files in epic `review-residuals-2026-09-08`. These
  paths are intentionally unbackticked:
  - Child 813: QuickFiler/Controllers/QfcItemController.FolderHandling.cs
  - Child 821: QuickFiler/Controllers/QfcHomeController.cs and UtilitiesCS/Threading/ProgressViewer.cs
  - Child 823: UtilitiesCS/OutlookObjects/Store/StoreWrapperController*.cs and QuickFiler/Viewers/Breadcrumb*
  - Child 825: UtilitiesCS/OutlookObjects/Table/OlTableExtensions.*, UtilitiesCS/Threading/TimeOutTask.cs,
    UtilitiesCS/Extensions/DfDeedle.cs
  - Child 817: UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs
  - Child 826: .editorconfig, BannedSymbols.txt, and every Console.SetOut restore in test classes

- Out of scope / non-goals — latent items identified by the research but not fixed here:
  - `ILGlobals.Cache` (`UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`:112) is an
    unsynchronised public mutable static. It is written nowhere after its field initializer and is
    read at exactly one place in the repository, `ILGlobals_Tests.cs`:130; no production code reads
    it. It is dormant rather than safe, and is not remediated by #824.
  - `ILGlobals.modules` (`UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`:119) has zero
    references anywhere in the repository outside its own declaration. The `modules` identifiers at
    `MethodBodyReader.cs`:249, :250 and :254 are a local `Module[]` variable inside
    `GetRefferencedOperand`, not this field. It is not removed by #824.
  - Both are to be filed as a separate follow-up issue rather than widening this fix, per the
    Bugfix Workflow rule in CLAUDE.md to open a new issue instead of expanding scope.

- Explicitly excluded systems, integrations, or datasets:
  - No production caller behaviour changes. `LoadOpCodes()` has zero production invocations; all
    five invocation sites are in test code.
  - No element-level immutability of the opcode tables. Changing the public field type to
    `IReadOnlyList<OpCode>` or `ReadOnlyCollection<OpCode>` is a larger change than this defect
    warrants and is explicitly not attempted.
  - No `[DoNotParallelize]` attribute is added to any test class (see AC8).
  - No change to any coverage threshold, analyzer severity, `.editorconfig` entry, or policy
    requirement.
  - No concurrency-sampling test, no `Thread.Sleep`, no `Task.Delay`, no retry, and no timing
    tolerance anywhere in the delivered tests.

## Root Cause Analysis
Both opcode tables are declared as ordinary mutable `public static` fields with no publication
guard. `LoadOpCodes()` is called repeatedly, once per test class instantiation path, rather than
once per process, so under `ClassLevel` parallelism two test classes can interleave: one thread's
reassignment-then-fill sequence in `ILGlobals.cs`:123-137 is visible to a second thread's read in
`MethodBodyReader.cs`:105/:110 at any point during the fill, including the instant right after
reassignment when the array is still all-default.

## Proposed Fix

### Design summary (what changes where):

**Invariant established by this fix, in one sentence:** each of `ILGlobals.singleByteOpCodes` and
`ILGlobals.multiByteOpCodes` is assigned exactly once, from the explicit static constructor of
`ILGlobals`, only after the `OpCode[0x100]` it refers to has been fully populated in a local, so
that no execution — on any thread, in any ordering — can observe either field holding a
partially populated array, and any future assignment outside that static constructor is a
compile-time error (CS0198) rather than a runtime race.

**Trace of the one observed value through the current code and through the fix.** The path chosen
is the one with no guard anywhere between the write window and the observation, which is what makes
the fix load-bearing:

1. **Publication point (unguarded).** `ILGlobals.cs`:123 assigns `singleByteOpCodes` a freshly
   allocated `OpCode[0x100]` whose 256 elements are all `default(OpCode)`. The field is now
   non-null and readable by every thread. Nothing at this line — no lock, no flag, no `volatile`,
   no initialization sentinel — prevents a read.
2. **Fill point (later, element by element).** `ILGlobals.cs`:137 writes
   `singleByteOpCodes[0x72] = OpCodes.Ldstr` at some iteration of the reflection loop that begins
   at :125. Between step 1 and this write, index `0x72` holds `default(OpCode)`.
3. **Observation point (unguarded).** A second worker executing `MethodBodyReader_Tests` reaches
   `MethodBodyReader.cs`:105, `code = ILGlobals.singleByteOpCodes[(int)value]` with `value == 0x72`,
   and receives `default(OpCode)`. `ClassLevel` parallelism at `AssemblyInfo.cs`:18-21 permits the
   two classes to run concurrently, and neither read site validates the returned `OpCode`.
4. **Absorption point (why the failure is misleading).** `default(OpCode)` has `Name` equal to the
   empty string and `OperandType` equal to `InlineBrTarget` (numeric value 0). Control therefore
   takes the numeric-operand branch of the `switch` on `code.OperandType` beginning at
   `MethodBodyReader.cs`:117 instead of the string branch, so the correctly read metadata token
   1879067923 is stored raw and `module.ResolveString(...)` is never called. No exception is raised
   at any point; the corruption surfaces only as a downstream string assertion failure in
   `GetBodyCode_ReturnsConcatenatedInstructions`, which reads as a `MethodBodyReader` defect.
5. **Where the fix removes the window.** After the fix there is no step 1: the field is assigned
   only at the end of the static constructor, after the local table is fully filled. The CLR's
   type-initialization guarantee (ECMA-335 Partition I §8.9.5) requires the initializer to run at
   most once per type and serialises concurrent triggering accesses, so a thread that observes a
   non-null field observes the completed array. Step 3 can therefore only read the fully populated
   table, and step 4 becomes unreachable.

**Why neither half of the fix suffices alone.** Building each table in a local before assigning is
what removes the mid-fill window from any same-thread reentrant read during type initialization;
it is not what makes cross-thread reads safe. The `static readonly` modifier plus static-constructor
publication is what makes cross-thread reads safe and what makes reintroduction a compile error;
it would not by itself prevent a reentrant read if the code assigned the field first and filled it
afterwards. Both halves are required.

### Boundaries and invariants to preserve:

- The public surface of `ILGlobals` keeps the same member names, types, and accessibility. Only the
  `readonly` modifier is added to the two opcode-table fields. Every existing read site compiles
  unchanged.
- `LoadOpCodes()` remains a `public static void` method with the same name and empty parameter list.
  All five existing invocation sites compile unchanged.
- The reads at `MethodBodyReader.cs`:105 and :110 keep their exact current expression form. No lock
  is introduced on the instruction-decode path, and no consumer-side catch anywhere in
  `MethodBodyReader.cs` may be broadened as part of this fix.
- `#nullable enable` at `ILGlobals.cs`:1 is retained.
- The assembly-level `Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)` setting at
  `AssemblyInfo.cs`:18-21 is retained unchanged.
- `MethodBodyReader_Tests.cs` stays within the 500-line file cap; at 489 lines it must not gain
  content.

### Dependencies or blocked work:

- None. F824 has no dependency on any sibling child of epic `review-residuals-2026-09-08` and no
  sibling depends on it. The owned file set does not intersect any sibling's.
- Issue #811 AC4 remains unmet until this fix lands; #811 is the consumer of this outcome, not a
  blocker of it.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

- `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`
- `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` only in the fallback case where a new test file is
  created instead of extending the existing test class.

#### Functions/classes/CLI commands impacted:

- `SDILReader.ILGlobals` — fields `multiByteOpCodes` (:117) and `singleByteOpCodes` (:118) become
  `public static readonly`; the `= null!` initializers are deleted; a new explicit private static
  constructor is added that runs the reflection loop currently at :125-148 against two local
  `OpCode[0x100]` arrays and assigns each field exactly once after the loop completes. The
  `throw new Exception("Invalid OpCode.")` behaviour at :143 is preserved.
- `SDILReader.ILGlobals.LoadOpCodes()` (:121) — retained; its body becomes a single call to
  `RuntimeHelpers.RunClassConstructor(typeof(ILGlobals).TypeHandle);`, which requires a
  `System.Runtime.CompilerServices` using directive. It is kept rather than removed because five
  call sites depend on it and because it is the only subject against which the "must not republish"
  invariant can be asserted.
- `UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.ILGlobals_Tests` — gains the gate and supporting
  tests named in the Test Strategy; the four `LoadOpCodes_*` tests at :11-51 are reworked.
- No CLI command is added or changed.

#### Data flow and validation changes:

The only data-flow change is when the opcode tables are built: at first access to any static member
of `ILGlobals` (type-initializer semantics of a type with an explicit static constructor) instead of
at each call to `LoadOpCodes()`. The contents produced by the reflection loop are byte-for-byte the
same. Validation is unchanged: an opcode whose value is at or above `0x100` and whose high byte is
not `0xfe` still raises `new Exception("Invalid OpCode.")`, now from the type initializer, where it
would surface as a `TypeInitializationException` with that exception as its inner exception. This is
a genuine behaviour difference in an unreachable-in-practice path over the fixed
`System.Reflection.Emit.OpCodes` set and is recorded here rather than silently accepted.

#### Error handling and logging updates:

None. `ILGlobals` performs no logging today and none is added; the class is a pure lookup table with
no I/O. No `try`/`catch` is added, widened, or removed in either production file.

#### Rollback/feature-flag considerations (if applicable):

No feature flag. The change is a single-commit revert if required: restoring the two field
declarations and the original `LoadOpCodes()` body returns the tree to its current behaviour. No
data migration, no persisted state, no configuration key is involved.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- Input: the `public static` fields of type `OpCode` declared on `System.Reflection.Emit.OpCodes`,
  read by reflection. Unchanged from the current implementation.
- Output: `ILGlobals.singleByteOpCodes`, an `OpCode[]` of length `0x100` indexed by opcode value for
  values below `0x100`; and `ILGlobals.multiByteOpCodes`, an `OpCode[]` of length `0x100` indexed by
  the low byte of `0xfe`-prefixed opcode values. Both non-null from first observation onwards.
- Residual honesty: `readonly` on an array field prevents reassignment of the reference, not
  mutation of elements. `ILGlobals.singleByteOpCodes[5] = default;` remains legal for any caller.
  This is accepted because the defect is specifically the reassign-then-fill window and no code in
  the repository writes elements outside `LoadOpCodes()`; full element immutability is recorded as a
  non-goal above.

#### Required configuration keys and defaults:

None. No configuration key, environment variable, or settings entry is read or added.

#### Backward-compatibility expectations:

Source-compatible for every reader. Adding `readonly` is a compile-time break only for a writer, and
the research established that no code anywhere in the repository assigns to either field except
`LoadOpCodes()` itself: the only writes are `ILGlobals.cs`:123, :124 (reference assignments) and
:137, :145 (element writes), all within that method body. Zero caller edits are therefore required.
This is not a published-package API, so no version bump applies.

#### Performance constraints (latency/throughput/memory):

- No lock, no `Lazy<T>` indirection, and no allocation is added on the read path at
  `MethodBodyReader.cs`:105/:110.
- Adding an explicit static constructor clears the `beforefieldinit` type flag, so the JIT must
  honour an initialization check on static accesses it could otherwise elide. For a table read once
  per decoded IL instruction this cost is expected to be negligible; it has not been measured and no
  performance budget is asserted.
- Memory: the two `OpCode[0x100]` arrays are now allocated once per process instead of once per
  `LoadOpCodes()` call, a strict reduction.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - `UtilitiesCS` targets .NET Framework 4.8.1 at `LangVersion` 12.0 and `UtilitiesCS.Test` targets
    .NET Framework 4.8.1 at `LangVersion` Latest. `static readonly`, explicit static constructors,
    and `RuntimeHelpers.RunClassConstructor` are all available at those levels.
  - A non-nullable `static readonly` field definitely assigned in the static constructor satisfies
    the compiler's null-state analysis, so no CS8618 is raised and no `null!` suppression is needed.
    The research states this as a claim about C# definite-assignment rules and did not verify it by
    running a build; it must be confirmed at the nullable msbuild gate (AC6, AC11).
  - Nothing in this feature has been executed yet. No build, test, or coverage run has been
    performed on this branch, and no result is asserted anywhere in this document.
- Constraints (budget, performance, compatibility):
  - No file may exceed 500 lines. `MethodBodyReader_Tests.cs` is at 489 and must not grow.
  - Tests must use MSTest, Moq where mocking is needed, and FluentAssertions, per CLAUDE.md.
  - Sleeps, retries, timing tolerances, temporary files, and repeated-run counting are prohibited as
    stabilisation mechanisms, by the epic's stated NFRs and by the repository general unit test
    policy at .claude/rules/general-unit-test.md.
  - Locally, `vstest.console.exe` additionally requires `/InIsolation` and exclusion of assemblies
    under a `.claude` worktree path, as recorded in the Environment block above. That is a local
    invocation detail; the criterion in AC11 quotes the CLAUDE.md command form.
- External dependencies (services, libraries, releases):
  - None added. No new NuGet package, no new analyzer, no new project reference.

## Data / API / Config Impact
- User-facing or API changes: none. `ILGlobals` is an internal helper of the SDIL reader; no
  add-in surface, ribbon action, settings screen, or persisted document is affected.
- Data or migration considerations: none. No persisted state, no schema, no serialized settings.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): no CLI flag, config schema, or
  version identifier changes.
- Build-file impact: both UtilitiesCS.csproj and `UtilitiesCS.Test/UtilitiesCS.Test.csproj` are
  legacy non-SDK projects with explicit `<Compile Include>` item lists rather than globbing, so a
  file that is not listed does not compile. The existing entries are
  UtilitiesCS.csproj:799 for the production file, UtilitiesCS.csproj:801 for MethodBodyReader.cs,
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj`:264 for the test class being extended, and
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj`:535 for MethodBodyReader_Tests.cs. Note the folder
  naming asymmetry: the production folder is `SDIL Reader` with a space, the test folder is
  `SDILReader` without one.
  - Preferred outcome: no build-file edit at all. `ILGlobals_Tests.cs` is 133 lines with 367 lines
    of headroom and is already registered at :264, so placing the new tests there requires no
    `<Compile Include>` entry.
  - Fallback: if a new test file is created, exactly one `<Compile Include>` line is added to
    `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, matching the surrounding style verbatim
    (four-space indent, backslash separators, self-closing tag with one leading space before `/>`),
    inserted adjacent to the other `SDILReader` entries after :265. Only this feature's own entry
    may be added: no reordering, no reformatting, no whitespace normalisation, and no touching of
    any other item, so that the epic fan-in merge remains a clean union of sibling additions.
  - UtilitiesCS.csproj is not modified: the fix edits an already-registered file in place and adds
    no new production file. It is named here unbackticked for that reason.

## Test Strategy
Seeded from issue:

Candidate fixes, in order of preference:

1. Publish both tables once and immutably — a static constructor or a `Lazy<OpCode[]>` pair — and
   make `LoadOpCodes()` a no-op or remove it. `LoadOpCodes()` is idempotent and the tables are
   conceptually constant, so this removes the mutable window entirely.
2. Take a `lock` inside `LoadOpCodes()` and around the reads in `MethodBodyReader`.
3. Interim stopgap only: add `[DoNotParallelize]` to both `ILGlobals_Tests` and
   `MethodBodyReader_Tests`. This suppresses rather than eliminates the race and should not be the
   final state, by the same reasoning `spec.md` applies to the console stopgap under AC3 of
   issue #811.

Resolution: candidate 1 is accepted, in its static-constructor form with `static readonly` fields
and `LoadOpCodes()` retained as a forced-initialization call. Candidate 2 is rejected because it
narrows rather than removes the window — the fields stay publicly mutable, the six unsynchronised
test reads would still race, and a future reader that forgets the lock reintroduces the defect with
no compile-time signal. Candidate 3 is rejected because sibling feature 825 is removing
`[DoNotParallelize]` attributes that carry no documented, verified reason, and no such reason exists
here once the tables are published immutably. A `volatile` modifier was also considered and rejected
as unsound: it would order the reference publication but does nothing about the element writes at
:137 and :145 that occur after the reference is already published.

Gate qualification. The failure is intermittent — observed once in ten consecutive full-suite runs
at 24 class-level workers, and not reproducible on demand. An acceptance test keyed to the exit code
of a single run of a known-intermittent test, or to a count of consecutive green runs, cannot fail in
one direction and is therefore not used as a gate here. Each gate below instead tests the
publication property directly and is required to fail deterministically on the current tree and pass
deterministically on the fixed tree.

- Regression tests to add or update, all in
  `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs`:
  - `LoadOpCodes_DoesNotRepublishPublishedTables` (primary behavioural gate, AC2). Arrange captures
    `ILGlobals.singleByteOpCodes` and `ILGlobals.multiByteOpCodes` into locals; Act calls
    `ILGlobals.LoadOpCodes()`; Assert uses `Should().BeSameAs(...)` on each field against its
    captured local.
  - `SingleByteOpCodes_FieldIsInitOnly` and `MultiByteOpCodes_FieldIsInitOnly` (secondary structural
    gates, AC3). Each resolves the field via
    `typeof(ILGlobals).GetField(nameof(...), BindingFlags.Public | BindingFlags.Static)`, asserts the
    `FieldInfo` is not null, and asserts `IsInitOnly` is true with an explanatory reason string.
  - `OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes` (supporting test, AC4). Enumerates every
    `public static` field of type `OpCode` on `typeof(OpCodes)` by reflection and asserts, for each,
    that `singleByteOpCodes[value]` equals it when `value` is below `0x100`, or that
    `(value & 0xff00) == 0xfe00` and `multiByteOpCodes[value & 0xff]` equals it otherwise.
  - `LoadOpCodes_Initializes_SingleByteOpCodes` (:12) is renamed
    `SingleByteOpCodes_IsPublishedWithFullLength` and its `ILGlobals.LoadOpCodes()` Act at :15 is
    removed; `LoadOpCodes_Initializes_MultiByteOpCodes` (:23) is renamed
    `MultiByteOpCodes_IsPublishedWithFullLength` and its Act at :26 is removed. In both, the first
    read of the static field is what triggers publication, which is the property under test.
  - `LoadOpCodes_PopulatesKnownSingleByteOpCodes` (:34) and `LoadOpCodes_PopulatesKnownOpCode_Ret`
    (:44) are deleted; `OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes` subsumes both spot checks
    with a strictly stronger assertion.
- Unit tests for the fixed behavior and boundaries: covered by the five tests above. Boundary
  indices `0x00` and `0xFF` of both tables are exercised by the exhaustive enumeration.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): the
  `Exception("Invalid OpCode.")` path is unreachable over the fixed `OpCodes` set and is not tested;
  the exhaustive test asserts the `(value & 0xff00) == 0xfe00` classification for every multi-byte
  opcode, which is the same condition that guards that throw.
- Error handling and logging verification: not applicable. No logging exists in `ILGlobals` and none
  is added; no exception handling is added, widened, or removed.
- Coverage impact and targets for changed lines/modules: no coverage regression on changed lines is
  permitted. The reflection loop moves from `LoadOpCodes()` into the `.cctor`, which executes once
  per process and remains covered; the residual `LoadOpCodes()` body is exercised by all five
  existing call sites plus the primary gate. The nearest committed measurement for these classes,
  per section 8.2 of the research record, reports `SDILReader.ILGlobals` at line-rate
  0.9459459459459459 / branch-rate 0.875 and `SDILReader.MethodBodyReader` at line-rate
  0.9732620320855615 / branch-rate 0.8947368421052632. That artifact was produced by a different
  feature's run (feature 729, 2026-09-02) at a different commit and is indicative context, not this
  branch's baseline; the research also records that no coverage artifact for #824 exists in this
  worktree, so a fresh baseline must be captured before any change.
- Toolchain commands to run (format → lint → type-check → test): the four CLAUDE.md commands quoted
  verbatim in AC11, in that order, restarting from the first on any failure or auto-fix.
- Manual validation steps (if required): none. No Outlook interaction, no UI, no filesystem access
  is involved.

## Acceptance Criteria

Criteria are numbered AC1..AC12 for downstream planning and audit reference. Each is verifiable by a
named test, a named command, or a stated file-and-line observation.

- [x] **AC1 — Publication mechanism.** In `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`,
      the declarations currently at :117 (`public static OpCode[] multiByteOpCodes = null!;`) and
      :118 (`public static OpCode[] singleByteOpCodes = null!;`) are both `public static readonly`
      with no initializer, and an explicit private static constructor of `ILGlobals` builds each
      `OpCode[0x100]` in a local, runs the reflection loop over `typeof(OpCodes).GetFields()` to
      completion against those locals, and assigns each field exactly once after the loop. Verified
      by reading the declarations and the static constructor in that file, and by a repository-wide
      `Grep` for `singleByteOpCodes\s*=` and `multiByteOpCodes\s*=` over `*.cs` returning matches
      only inside the static constructor of `ILGlobals.cs`. The delivered implementation matches the
      five-step trace in the Proposed Fix section, including the requirement that the field
      assignment follows the fill rather than preceding it.

- [x] **AC2 — Primary behavioural gate: `LoadOpCodes()` does not republish the tables.** The test
      `LoadOpCodes_DoesNotRepublishPublishedTables` exists in
      `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs`, captures
      `ILGlobals.singleByteOpCodes` and `ILGlobals.multiByteOpCodes` into locals, calls
      `ILGlobals.LoadOpCodes()`, and asserts each field is reference-identical to its captured local
      via FluentAssertions `BeSameAs`. This test fails on the current tree and passes on the fixed
      tree. It fails on the current tree in every execution ordering: if no prior test class has
      called `LoadOpCodes()` the captured local is null (`ILGlobals.cs`:118 initializes to `null!`)
      and the post-call value is a freshly allocated array from `ILGlobals.cs`:123, so `BeSameAs`
      fails against null; if a prior class has called it, the call allocates a different array at
      `ILGlobals.cs`:123, so `BeSameAs` fails on reference identity. There is no third ordering, so
      the criterion is order-independent. The test contains no sleep, no retry, no timing tolerance,
      no repeated-run loop, and no additional thread.

- [x] **AC3 — Secondary structural gate: both fields are `InitOnly`.** The tests
      `SingleByteOpCodes_FieldIsInitOnly` and `MultiByteOpCodes_FieldIsInitOnly` exist in
      `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs`, resolve each field via
      `typeof(ILGlobals).GetField(nameof(ILGlobals.singleByteOpCodes), BindingFlags.Public | BindingFlags.Static)`
      and the `multiByteOpCodes` equivalent, assert the returned `FieldInfo` is not null, and assert
      `FieldInfo.IsInitOnly` is true. Both fail on the current tree, where `ILGlobals.cs`:117-118
      declare plain mutable fields and `IsInitOnly` is false, and both pass on the fixed tree. These
      tests must not additionally assert anything about `TypeAttributes.BeforeFieldInit`, which
      would over-constrain the implementation encoding rather than the property that matters.

- [x] **AC4 — Supporting test: exhaustive opcode-table population.** The test
      `OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes` exists in
      `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` and, for every `public
      static` field of type `OpCode` on `typeof(OpCodes)`, asserts that `singleByteOpCodes[value]`
      equals that opcode when `value` is below `0x100`, and otherwise that
      `(value & 0xff00) == 0xfe00` and `multiByteOpCodes[value & 0xff]` equals that opcode. This
      test passes on the current unfixed tree as well as on the fixed tree, because a single-threaded
      test observes a fully populated table either way. It is therefore a supporting test, not a gate
      for this defect, and must not be reported or relied upon as evidence that the race is fixed.
      Its value is that it replaces two weak spot checks and would catch a fix that publishes the
      tables safely but fills them wrongly.

- [x] **AC5 — `LoadOpCodes()` retained as a forced-initialization call.**
      `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` still declares
      `public static void LoadOpCodes()` and its body is
      `RuntimeHelpers.RunClassConstructor(typeof(ILGlobals).TypeHandle);` with the corresponding
      `System.Runtime.CompilerServices` using directive present. The method is neither removed nor
      left with an empty body. `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs`
      is byte-for-byte unchanged, confirmed by `git diff --stat` listing no entry for it, so its
      `ILGlobals.LoadOpCodes()` call at :364 inside `CreateReader` (declared at :362) compiles
      unchanged and the file remains at 489 lines against the 500-line cap.

- [x] **AC6 — Corrected nullable annotations and comment.** In
      `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`, the `= null!` suppressions currently
      at :117 and :118 are deleted, and the three-line comment currently at :114-116 — which asserts
      that the fields are "populated by LoadOpCodes() before any read" and that they are "annotated
      null!" — is replaced by documentation that states the tables are published once by the static
      constructor, are never reassigned, and that `readonly` prevents reassignment of the reference
      but not mutation of elements, so callers must treat the contents as read-only. Verified by a
      `Grep` for `null!` over that file returning no match on either field declaration and by a
      `Grep` for `populated by LoadOpCodes` over that file returning no match. `#nullable enable`
      remains at :1, and the nullable msbuild command in AC11 completes with zero CS86xx
      diagnostics, in particular no CS8618 on either field.

- [x] **AC7 — The four existing `LoadOpCodes_*` tests no longer mis-attribute published state to a
      `LoadOpCodes()` Act.** In `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs`,
      a `Grep` for `LoadOpCodes` returns exactly one invocation of `ILGlobals.LoadOpCodes()`, and it
      is the Act of `LoadOpCodes_DoesNotRepublishPublishedTables` from AC2. A `Grep` for
      `LoadOpCodes_Initializes_SingleByteOpCodes|LoadOpCodes_Initializes_MultiByteOpCodes|LoadOpCodes_PopulatesKnownSingleByteOpCodes|LoadOpCodes_PopulatesKnownOpCode_Ret`
      over that file returns zero matches. The tests
      `SingleByteOpCodes_IsPublishedWithFullLength` and `MultiByteOpCodes_IsPublishedWithFullLength`
      exist and assert non-null and `Length == 0x100` for their respective fields with no
      `LoadOpCodes()` call, preserving the assertions previously at :18-19 and :29-30. The two spot
      checks previously at :40 and :50 are deleted, their coverage subsumed by AC4.

- [x] **AC8 — No `[DoNotParallelize]` on either test class.** A `Grep` for `DoNotParallelize` over
      `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` and
      `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs` returns zero matches
      in both files, matching their current state. Neither class gains the attribute as a
      stabilisation measure, because after AC1 no shared mutable state remains between them and no
      documented, verified reason for the attribute exists. `UtilitiesCS.Test/Properties/AssemblyInfo.cs`
      still carries `Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)` at :18-21
      unchanged, confirmed by `git diff --stat` listing no entry for that file.

- [x] **AC9 — No synchronisation primitive introduced, and `MethodBodyReader.cs` unchanged.**
      `git diff --stat` against the merge base lists no entry for
      `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs`, so the reads at :105 and :110
      keep their exact current expression form and no consumer-side `catch` in that file is
      broadened. A `Grep` for `lock\s*\(|volatile|Lazy<` over
      `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` returns zero matches, confirming the
      rejected candidates 2 and the `volatile` variant were not partially adopted.

- [x] **AC10 — Build-file discipline.** `git diff` for `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
      shows either no change at all (the preferred outcome, achieved by placing the new tests in the
      already-registered `ILGlobals_Tests.cs`) or exactly one added line and zero removed lines, that
      line being a single `<Compile Include>` entry for this feature's own new test file, matching
      the surrounding style verbatim. `git diff --stat` lists no entry for UtilitiesCS.csproj. No
      reordering, reformatting, or whitespace normalisation of any existing item appears in either
      diff.

- [x] **AC11 — Clean full C# toolchain pass, in the repository-standard order.** The four commands
      below, quoted from CLAUDE.md, are run in this order, and the final pass completes all four with
      no failure and no file modified by the formatter:
      1. `dotnet tool run csharpier format .`, then `dotnet tool run csharpier check .` reporting
         zero files needing formatting.
      2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
         exiting 0 with zero errors.
      3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
         exiting 0 with zero errors.
      4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` with zero failed tests, and
         the tests named in AC2, AC3, AC4 and AC7 present in the run's test list.
      Each msbuild log is asserted non-vacuous by containing zero occurrences of
      `Skipping target "CoreCompile"`, so the analyzer and nullable gates are proven to have actually
      compiled. Any failure or formatter auto-fix restarts the loop at step 1. Logs for the final
      pass are written under the feature's evidence directory at
      docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/.

- [x] **AC12 — Coverage captured and not regressed on changed lines.** A pre-change baseline coverage
      document is written under
      docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/
      and a post-change coverage document captured from the AC11 step 4 run is written under
      docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/.
      These are the canonical evidence kinds defined by the evidence-and-timestamp-conventions skill;
      no coverage document is written to any other location. The per-class `line-rate` and `branch-rate` for `SDILReader.ILGlobals` and
      `SDILReader.MethodBodyReader` in the post-change document are greater than or equal to the
      corresponding values in the baseline document. Comparison is on the per-`class` attributes
      read directly from the Cobertura documents, which is the comparable axis; raw and
      post-processed root totals are not compared. No coverage threshold, exclusion list, or
      analyzer severity is lowered, weakened, or deleted anywhere in the change.

## Risks & Mitigations
- Technical or operational risks:
  1. **CS8618 on the `static readonly` fields.** If the compiler does not treat definite assignment
     in the static constructor as satisfying null-state analysis, the nullable gate in AC11 step 3
     fails. This claim was not verified by a build during research.
  2. **`beforefieldinit` removal changes initialization timing.** Any code that depended on the
     relaxed timing would see initialization occur at first static-member access instead. No such
     dependency is known: the type has no cross-type initializer cycle and the static constructor
     touches only `System.Reflection.Emit.OpCodes`.
  3. **Exception surfacing changes shape.** The `Exception("Invalid OpCode.")` path, if it ever
     executed, would now surface wrapped in a `TypeInitializationException`. The path is unreachable
     over the fixed `OpCodes` set.
  4. **The intermittent failure is not proven absent by this fix.** The observed base rate is one in
     ten full-suite runs, and clean runs do not establish absence. If the same signature recurs after
     this fix, the cause is elsewhere.
  5. **A future edit silently reverts `readonly`.** Without a structural guard the mutable window
     could be reintroduced with no test signal.
  6. **Epic fan-in conflict.** A sibling child editing a file this feature owns, or this feature
     touching a sibling-owned file, would produce a merge conflict at integration.
- Mitigations and rollbacks:
  1. Confirmed at the AC11 step 3 gate before the change is reported complete. If CS8618 is raised,
     the correct response is to keep the fields non-nullable and adjust the static constructor so
     every path assigns them, not to reintroduce a `null!` suppression.
  2. Accepted risk, bounded by inspection of the static constructor's dependencies. The alternative
     that preserves `beforefieldinit` — field initializers calling a build helper — was considered
     and rejected because it requires two reflection passes and breaks the guarantee that
     `LoadOpCodes()` triggers initialization, which AC2 depends on.
  3. Recorded in the Proposed Fix section as a known behaviour difference rather than left implicit.
  4. AC2 gates the publication property directly rather than sampling the race, so it does not
     depend on the intermittent signature to demonstrate the fix. Recurrence after the fix would be
     a new investigation, not a regression of this one.
  5. AC3 provides the machine-checkable structural guard, backing the compiler's CS0198 error.
  6. The owned file list in Scope & Non-Goals is disjoint from every sibling's, and AC5, AC8, AC9
     and AC10 each assert `git diff --stat` emptiness for a specific owned-but-unmodified file, so
     accidental widening is caught before integration.
- Rollback: revert the single commit. Restoring the two field declarations and the original
  `LoadOpCodes()` body returns the tree to its current behaviour; no data or configuration migration
  is involved.

## Rollout & Follow-up
- Release/rollout steps: standard branch-to-pull-request flow from
  bug/ilglobals-loadopcodes-unsynchronised-static-race-824, then fan-in to the epic
  `review-residuals-2026-09-08` integration branch alongside the sibling children. No staged
  rollout, no feature flag, no deployment step. The change is source-only and affects no runtime
  configuration.
- Post-fix monitoring or clean-up tasks:
  - Report the outcome to issue #811 so its AC4 can be re-evaluated against a tree that no longer
    contains this race.
  - File a follow-up issue for the two latent items recorded as non-goals: `ILGlobals.Cache` at
    `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`:112, an unsynchronised public mutable
    static unused by production code, and `ILGlobals.modules` at :119, entirely unreferenced
    repo-wide.
  - Watch subsequent full-suite runs for recurrence of the
    `GetBodyCode_ReturnsConcatenatedInstructions` "to contain ldstr" signature. Recurrence after
    this fix indicates a distinct cause and warrants a new investigation rather than reopening #824.
- Links:
  - Issue: https://github.com/drmoisan/TaskMaster/issues/824
  - Related issue: #811 (acceptance criterion AC4 blocked by this defect)
  - Epic: `review-residuals-2026-09-08`
  - Promoted bug record:
    docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/issue.md
  - Design research:
    docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/research/ilglobals-static-publication-2026-09-08T23-45.md
