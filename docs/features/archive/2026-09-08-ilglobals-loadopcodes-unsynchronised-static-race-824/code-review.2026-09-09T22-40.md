# Code Review — Issue #824 (ILGlobals.LoadOpCodes unsynchronised static race)

- **Issue:** #824
- **Base branch:** `epic/review-residuals-2026-09-08-integration` (base commit `553f874a`)
- **Reviewer:** feature-review agent
- **Timestamp:** 2026-09-09T22-40
- **Files reviewed:** 2 source files (1 production, 1 test)
- **Disposition:** **APPROVE.** 0 blocking findings, 4 non-blocking observations.

Verification note: the Bash tool was banned for this review, so no git command was run. Source
content, line counts, greps, Cobertura documents, and toolchain logs were all read directly from the
working tree. Claims that could not be re-derived without git are labelled as such.

---

## 1. Correctness of the fix

### 1.1 The mechanism is the right one

The defect is a reassign-then-fill window: the old `LoadOpCodes()` published a freshly allocated
all-default `OpCode[0x100]` to a public static field and only then filled it, so any concurrent
reader could observe `default(OpCode)` at an index the loop had not yet reached.

The delivered fix removes the window structurally rather than narrowing it:

```csharp
static ILGlobals()
{
    OpCode[] singleTable = new OpCode[0x100];
    OpCode[] multiTable = new OpCode[0x100];
    ...                                    // fill the locals
    singleByteOpCodes = singleTable;       // ILGlobals.cs:167
    multiByteOpCodes = multiTable;         // ILGlobals.cs:168
}
```

Two properties combine, and the spec is correct at `:196-202` that neither suffices alone:

- **Build-in-a-local, publish-once** eliminates the mid-fill window for any same-thread reentrant
  read during type initialization.
- **`static readonly` plus static-constructor publication** makes cross-thread reads safe, because
  ECMA-335 Partition I §8.9.5 requires the type initializer to run at most once per type and
  serialises concurrent triggering accesses. A thread that observes a non-null field therefore
  observes the completed array.

The rejected alternatives are rejected for the right reasons. A `lock` inside `LoadOpCodes()` would
leave the fields publicly mutable and the six unsynchronised reads still racing, and a future caller
forgetting the lock would reintroduce the defect with no compile-time signal. `volatile` would order
the reference publication but does nothing about the element writes that follow it.

### 1.2 `readonly` is load-bearing, not decorative

Because both fields are `readonly`, any assignment outside the static constructor is **CS0198**, a
compile error rather than a runtime race. Re-derived: a repository-wide `Grep` for
`(singleByteOpCodes|multiByteOpCodes)\s*=` over `*.cs` returns exactly two matches, both at
`ILGlobals.cs:167-168`, and both fall after the reflection loop's closing brace at :166 and before
the constructor's closing brace at :169. Every other reference in the repository is a read.

### 1.3 `LoadOpCodes()` is safe and non-recursive

`RuntimeHelpers.RunClassConstructor(typeof(ILGlobals).TypeHandle)` inside a static method of
`ILGlobals` is not a re-entrancy hazard. Calling any static member of a non-`beforefieldinit` type
already triggers the type initializer before the body runs, so by the time `RunClassConstructor`
executes the initializer has completed; on an already-initialized type the call is a no-op. There is
no deadlock and no recursion path. The method is correctly documented as safe to call repeatedly from
any thread.

A consequence worth stating explicitly, because it affects how a future reader should treat the
method: after this change `LoadOpCodes()` is **redundant for correctness**. Any static access
triggers the serialised initializer on its own. The method exists to keep its remaining call sites
compiling and to give AC2 a subject against which the must-not-republish invariant can be asserted.
The doc comment says exactly this, which is good.

### 1.4 Behaviour differences, all recorded

- The `throw new Exception("Invalid OpCode.")` guard at `ILGlobals.cs:159-162` is preserved verbatim.
  It would now surface wrapped in a `TypeInitializationException` rather than raised directly. The
  path is unreachable over the fixed `System.Reflection.Emit.OpCodes` set. Recorded at
  `spec.md:256-261`.
- Tables are now allocated once per process instead of once per `LoadOpCodes()` call, a strict
  reduction.
- Adding an explicit static constructor clears `beforefieldinit`, so the JIT must honour an
  initialization check on static accesses it could otherwise elide. Recorded at `spec.md:305-308` as
  an accepted, unmeasured cost with no performance budget asserted. Assessment: negligible for a
  table read once per decoded IL instruction, and the flag clearance is what makes the guarantee in
  §1.3 hold. The alternative that preserves the flag — field initializers calling a build helper —
  was considered and rejected at `spec.md:602-605` because it needs two reflection passes and breaks
  the property AC2 depends on. That reasoning is sound.

---

## 2. Design and structure

| Aspect | Assessment |
|---|---|
| Simplicity | The simplest construct that provides the guarantee. No `Lazy<T>`, no lock, no double-checked locking, no flag, no indirection. |
| Public surface | Unchanged in names, types, and accessibility. Only `readonly` was added. Source-compatible for every reader; zero caller edits required. |
| Separation of concerns | `ILGlobals` remains a pure lookup table with no I/O. Nothing was added to the instruction-decode read path in `MethodBodyReader.cs`. |
| Error handling | No `try`/`catch` added, widened, or removed in either file. Zero `catch` clauses in the change. |
| Naming | `singleTable` / `multiTable` are clear camelCase locals. The pre-existing non-conforming field names were deliberately preserved for source compatibility, which is the right trade. |
| File size | `ILGlobals.cs` 228 lines, `ILGlobals_Tests.cs` 270 lines. Both well under the 500-line cap; re-derived by reading each file to its last line. |
| Dependencies | One BCL `using`. Nothing added. |

### 2.1 Documentation quality

The XML doc blocks are a genuine improvement over what they replaced. The old three-line comment
asserted an invariant that the code did not enforce — "populated by `LoadOpCodes()` before any read"
was precisely the false belief that produced the defect. The replacement states the enforced
invariant, and states the residual honestly in the place a caller will actually read it:

> `readonly` prevents reassignment of the array reference; it does not prevent element mutation, so
> callers must treat the contents as read-only.

Putting that in the source rather than only in `spec.md` is the right call: `spec.md` is archived at
feature close, the source comment is not.

---

## 3. Test review

### 3.1 Determinism — checked specifically, and clean

A `Grep` over the entire `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/` directory for
`Thread\.Sleep|Task\.Delay|Stopwatch|DateTime\.Now|DateTime\.UtcNow|Random|await |Parallel\.|new Thread|ThreadPool|Sleep\(|TimeSpan|Retry|\[DoNotParallelize\]`
returned **zero matches**.

The delivered gates rest on exactly two deterministic seams:

- **Reference identity** — `ILGlobals.singleByteOpCodes.Should().BeSameAs(singleBefore, ...)` at
  `ILGlobals_Tests.cs:53-60` and `:61-68`.
- **Reflection over field metadata** — `field!.IsInitOnly.Should().BeTrue(...)` at `:87-93` and
  `:111-117`.

**No test asserts a probabilistic property.** There is no repeated-run loop, no consecutive-green
count, no additional thread, no timing tolerance, and no retry. This matters more than usual here:
the underlying defect is a race, and the obvious wrong answer — a test that spawns threads and hopes
to catch the window — would have been both non-deterministic and unable to fail in one direction.
`spec.md:386-391` reasons through this explicitly and reaches the right conclusion.

### 3.2 The gates can actually fail — verified

This was checked rather than assumed, because a gate that cannot go red proves nothing.

**AC2** — `evidence/regression-testing/ac2-fail-before.2026-09-09T15-25.md` records
`ExpectedExitCode: 1` and `EXIT_CODE: 1`, with `TOTAL=13 / PASSED=12 / FAILED=1` and a complete
thirteen-row per-test outcome table. The failure site is named from the captured stack trace as
`ReferenceTypeAssertions.BeSameAs` raised from
`ILGlobals_Tests.LoadOpCodes_DoesNotRepublishPublishedTables()`, so the failing assertion is the
reference-identity check the criterion states and not an incidental error such as a compile failure or
a missing type.

**AC3** — `evidence/regression-testing/ac3-fail-before.2026-09-09T15-30.md` records
`ExpectedExitCode: 1`, `EXIT_CODE: 1`, `TOTAL=15 / PASSED=12 / FAILED=3`, with both
`SingleByteOpCodes_FieldIsInitOnly` and `MultiByteOpCodes_FieldIsInitOnly` listed as **Failed** and
the third failure correctly identified as the still-red AC2 gate.

**Discovery-count controls are present and real.** `TOTAL=13` is explained as 12 baseline plus one
added by P1-T1; `TOTAL=15` as 12 plus one plus two. This is the property that distinguishes a genuine
red run from a run in which the named test was merely absent from a pass list: a filter that
discovered nothing, or a subset, would report a different total. Both fail-before artifacts pair the
total with a full per-test name list, so neither a silently dropped test nor an unaccounted total can
hide.

**Pass-after** — `evidence/regression-testing/ac2-ac3-pass-after.2026-09-09T15-38.md` records
`TOTAL=15 / PASSED=15 / FAILED=0`. The test population is held constant at 15 across the P1-T6 and
P2-T5 runs, so the red-to-green transition is attributable to the fix rather than to a change in what
was discovered. This is the correct control and it is stated explicitly.

### 3.3 AC4 is correctly labelled and never over-claimed

`OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes` passes on the unfixed tree, because a
single-threaded test observes a fully populated table either way. Every place it appears is checked:

| Location | Treatment |
|---|---|
| `ILGlobals_Tests.cs:120-125` (its own XML doc) | "Supporting test, not a gate for issue #824. It passes on the unfixed tree as well..." |
| `evidence/regression-testing/ilglobals-tests-after-rework.2026-09-09T15-44.md:57-60` | "Its presence in this all-green list is not evidence that the race is fixed." |
| `evidence/regression-testing/ac2-ac3-pass-after.2026-09-09T15-38.md:65-66` | "is a supporting test only." |
| `evidence/other/ac-checkoff-notes.2026-09-09T16-19.md:45-55` | "AC4 is checked off because the test exists, is correctly implemented, and passes — not because its passing demonstrates anything about the race." |

**No artifact presents AC4 as evidence that the race is fixed.** No blocking finding on this axis.
Carrying the qualification in the source file itself, not only in the audit trail, is the stronger
choice, since the audit trail does not survive feature-folder archival.

### 3.4 Test quality details

- The AC4 test closes with `assertedCount.Should().BeGreaterThan(0, ...)` at
  `ILGlobals_Tests.cs:181-187`. This non-vacuity guard is not required by the criterion and is a
  genuine improvement: without it, a reflection enumeration that silently returned nothing would make
  the test pass while asserting nothing at all.
- The AC3 tests assert `field.Should().NotBeNull(...)` before dereferencing with `field!`, so a
  renamed or removed field produces a clear failure rather than a `NullReferenceException`.
- Every assertion carries a `because` reason string, and the reason strings explain the *why* —
  "because a concurrent reader would otherwise observe a freshly allocated array before the fill loop
  has populated it" — rather than restating the assertion.
- The two renamed publication tests correctly perform **no Act**, with an XML comment explaining that
  reading the field is what triggers publication and is therefore the property under test. Removing
  the `LoadOpCodes()` Act is the substantive half of AC7: the old names actively taught the false
  mental model that produced the defect.
- One minor asymmetry, not a defect: the test enumerates with
  `typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)` while the production
  constructor uses the default-binding `typeof(OpCodes).GetFields()`. Both enumerate the same set for
  this type, and the test's explicit flags are the clearer form.

---

## 4. Assessment of the two recorded executor deviations

Both are documented at `evidence/other/executor-deviations.2026-09-09T15-28.md` and judged
independently below.

### 4.1 D-1 — [P0-T15] left unchecked, halt branch not taken

**Judgement: proceeding was the right call. Leaving the task unchecked was the right way to record
it.**

The plan's Decision D5 anchors every diff on `git merge-base HEAD origin/main`. For an epic child
that resolves to `6f08302a`, which predates five already-merged siblings, so the merge-base-anchored
listing attributes 277 inherited paths to this feature. P0-T15's acceptance is genuinely not met, and
marking it `[x]` would have been false.

The halt branch should not have been taken, for three reasons this reviewer verified are present in
the record rather than merely asserted:

1. The condition is **environmental, not a property of the change**. The worktree was fast-forwarded
   to the integration tip after the plan cleared preflight.
2. The authorising delegation, quoted verbatim at `evidence/baseline/base-inertness.2026-09-09T15-19.md:85-89`,
   records this exact base state as verified and expected and names the integration tip
   `553f874a287261af0dd42e4f9270d31ac475308a` explicitly. Halting on a state the authorising layer had
   already accounted for would have ended the run for nothing.
3. The property the affected gates exist to establish remained fully verifiable. Only the choice of
   anchor was wrong for it.

**On the adaptation itself — sound, but strictly weaker than an alternative that was available.** The
executor substituted a symbolic `HEAD` anchor, reasoning that a `HEAD`-anchored diff shows exactly
this run's own footprint and excludes only commits the run did not make. That reasoning is valid
**only while the run has made no commit**: once work is committed, `git diff HEAD` stops showing it.
This reviewer checked the timeline and the constraint holds — every HEAD-anchored gate ran between
15:46 and 16:15 and the first commit was at approximately 16:19 — and the post-commit interval is
covered separately by the pre-commit and post-commit porcelain observations in
`evidence/qa-gates/closure.2026-09-09T16-25.md`. So nothing was actually missed.

The stronger alternative was available and was not identified: anchoring on the integration tip
`553f874a`, which the delegation had already named in text the executor quotes. That would have
satisfied the plan's intent exactly and would have survived the commits. The executor chose `HEAD`
to honour plan D5's no-pinned-SHA rule, which is a defensible reading, but D5 itself is the defect —
an epic-child plan must resolve its base from the epic integration branch, not `origin/main`. That
belongs upstream in the planner, not on this branch.

**Is any acceptance criterion left genuinely unverified? No.** The four criteria that depend on an
anchored diff each carry an independent secondary observation:

| Criterion | Anchor-independent corroboration | Basis |
|---|---|---|
| AC5 (`MethodBodyReader_Tests.cs` unchanged) | 489 lines, matching baseline exactly; `ILGlobals.LoadOpCodes()` still present at :364 | Re-derived by this reviewer |
| AC8 (`AssemblyInfo.cs` unchanged) | `Parallelize(Workers = 0, Scope = ...ClassLevel)` intact at :18-21 | Re-derived by this reviewer |
| AC9 (`MethodBodyReader.cs` unchanged) | 299 lines matching baseline; Cobertura class rates **byte-identical** across baseline and post-change (`0.9732620320855615` / `0.8947368421052632`, complexity 38) | Re-derived by this reviewer |
| AC10 (project files unchanged) | The four inherited `<Compile Include>` lines all name `FolderPredictorTests` partial-class files under `OutlookObjects\Folder\`, visibly a sibling's owned set | Evidence-attested, named not summarised |

The caller additionally re-measured the footprint against the true base `553f874a` and reports it
equals the Owned Write Set. The substantive conclusion is confirmed from two directions.

### 4.2 D-2 — [P1-T4]'s `IsInitOnly` count of 2 is arithmetically unreachable

**Judgement: the executor's handling was correct, and the alternative would have been worse.**

The task demanded a `Grep` for `IsInitOnly` returning exactly 2 matches over `ILGlobals_Tests.cs`.
The same task mandates the test names `SingleByteOpCodes_FieldIsInitOnly` and
`MultiByteOpCodes_FieldIsInitOnly`, and `IsInitOnly` is a substring of `FieldIsInitOnly`. Every
conforming implementation therefore returns 4: two declaration lines plus two assertion lines. The
condition is unsatisfiable for any implementation that obeys the rest of the same task.

The executor verified the substantive property — exactly two `FieldInfo.IsInitOnly` assertions, one
per field — with the discriminating pattern `\.IsInitOnly`, which matches member-access sites and not
declaration names. **Re-derived by this reviewer: `Grep` for `\.IsInitOnly` over
`ILGlobals_Tests.cs` returns exactly 2.**

The two ways to reach a literal count of 2 would both have been worse:

- Renaming the tests away from the names the same task mandates, breaking a different condition of the
  same task and diverging from the names AC3 quotes.
- Replacing `FieldInfo.IsInitOnly` with `Attributes.HasFlag(FieldAttributes.InitOnly)`, a form AC3
  does not name, purely to dodge a substring collision.

Changing the implementation to satisfy an arithmetic artifact of a grep pattern, rather than fixing
the pattern, is the failure mode here, and the executor avoided it. The disposition also correctly
notes that this is the same identifier-collision class the plan itself guards against elsewhere, where
`**AC1` prefixes `**AC10`, `**AC11`, and `**AC12` — so the pattern-collision hazard was known to the
plan author and simply missed at this one task.

---

## 5. Residual risk

### 5.1 `readonly` does not prevent element mutation — accepted, and correctly recorded

`ILGlobals.singleByteOpCodes[5] = default;` remains legal for any caller. This is recorded at
`spec.md:283-287` as residual honesty and, more importantly, in the source itself at
`ILGlobals.cs:118-120` and `:127-128`.

**Agreed as scoped.** The defect is specifically the reassign-then-fill window, and no code in the
repository writes elements outside the static constructor — re-derived, the repo-wide assignment sweep
returns only the two reference assignments. Full element immutability would require changing the
public field type to `IReadOnlyList<OpCode>` or `ReadOnlyCollection<OpCode>`, a public-API change to
six read sites during an epic fan-in, which is disproportionate to the defect. Recording it in the
source comment is the right mitigation for a risk that is not being eliminated.

### 5.2 `Cache` and `modules` — recorded, not silently dropped; scoping agreed

Both remain unsynchronised public mutable statics and both are **recorded rather than dropped**.
`evidence/other/follow-up-latent-statics.2026-09-09T16-21.md` names each member, re-derives its
current line number, explains the shift and its cause, gives the declaration verbatim, and enumerates
every reference:

- `ILGlobals.Cache` at `ILGlobals.cs:113` — one read site repo-wide, which this reviewer confirmed
  directly at `ILGlobals_Tests.cs:267` (`ILGlobals.Cache.Should().NotBeNull();`). No production reader.
  Dormant rather than safe.
- `ILGlobals.modules` at `ILGlobals.cs:131` — zero references repo-wide. The artifact correctly
  excludes the `modules` identifiers in `MethodBodyReader.cs`, which are a local `Module[]` inside
  `GetRefferencedOperand` and would mislead a naive unqualified search.

**I agree with the scoping.** Remediating them here would change the public surface of a type mid
epic fan-in for members that are dormant or entirely unreferenced, against the CLAUDE.md Bugfix
Workflow rule to open a new issue rather than widen scope. The recommended disposition in the
artifact — a single follow-up covering both, plausibly deleting `modules` outright — is the right
shape.

**One owed item:** the artifact states filing is owned by the epic layer and is not a plan task, and
the issue is not yet filed. Prose in a feature folder does not survive feature-folder archival. This
must be promoted into a real GitHub issue before #824 closes. The same applies to the `spec.md`
rollout commitment to report the outcome to issue #811 so its AC4 can be re-evaluated.

### 5.3 The intermittent failure is not proven absent

Correctly recorded as risk 4 at `spec.md:591-593`. The gates establish the publication property, not
the absence of the observed signature. Clean runs do not establish absence, and a recurrence of the
`GetBodyCode_ReturnsConcatenatedInstructions` "to contain ldstr" signature after this fix would
indicate a distinct cause warranting a new investigation rather than reopening #824. This is the
honest framing and it is applied consistently across the artifacts.

---

## 6. Findings

| ID | Severity | Location | Finding |
|---|---|---|---|
| N-1 | Low | `ILGlobals.cs:172-173` | Doc comment says "Retained for the **five** existing call sites"; a repo-wide `Grep` for `LoadOpCodes` over `*.cs` at head returns exactly **two** invocations (`ILGlobals_Tests.cs:50`, `MethodBodyReader_Tests.cs:364`). The count was correct pre-change; this feature's own AC7 rework removed three of the four test call sites, so the comment went stale within the same change. Violates "keep comments synchronized with behavior" (CLAUDE.md §C#6.3). No behavioural impact and no AC affected. Recommend correcting "five" to "two" in a follow-up rather than reopening the file here, which would force a full toolchain loop restart for a comment. |
| N-2 | Medium (process, upstream) | `plan.2026-09-08T23-51.md:176` | Plan Decision D5 anchors on `origin/main`, which is the wrong base for an epic child and is what made [P0-T15] unsatisfiable. Fix in the planner, not on this branch. See §4.1. |
| N-3 | Medium (owed) | `evidence/other/follow-up-latent-statics.2026-09-09T16-21.md` | Follow-up issue for `Cache` and `modules` is recorded but not filed; must be promoted to a real GitHub issue before feature-folder archival. Same for the #811 outcome report. See §5.2. |
| N-4 | Low | `spec.md:562-564`, `:570-571` | Two literal evidence-location clauses satisfied by substitution (raw logs and Cobertura to gitignored `coverage/`, markdown extracts committed). Documented and justified at `evidence/other/ac-checkoff-notes.2026-09-09T16-19.md:57-98` with verifiable reasons (`.gitignore:84` is `*.log`; a full-solution Cobertura is ~10 MB with host paths). Preferable to the literal reading. Residual: the independent re-derivation this reviewer performed from those raw files will not be repeatable from merged history alone. |

**Blocking findings: 0.**

---

## 7. What was verified independently rather than accepted

Recorded so a future reviewer knows which claims rest on this review and which rest on the executor's
artifacts.

| Claim | Re-derived by this reviewer | Method |
|---|---|---|
| Repo-wide line 85.6241 %, branch 79.8129 % | Yes | `coverage/post-change.cobertura.xml:2` read directly |
| `SDILReader.ILGlobals` 0.9459459459459459 → 0.95, branch 0.875 unchanged | Yes | `coverage/baseline.cobertura.xml:44010` and `coverage/post-change.cobertura.xml:44010` |
| `SDILReader.MethodBodyReader` unchanged on both metrics | Yes | Both documents; byte-identical rates and complexity |
| Changed-code coverage 9 / 9 | Yes | Only `hits="0"` lines in the class are 160 and 161, neither in the added set; all 9 added instrumented lines carry `hits="1"` |
| Tests 7212 passed / 0 failed / 0 skipped | Yes | `coverage/coverage-post-change.log:7233-7239` |
| Analyzer gate: skipped 0, CoreCompile 12, errors 0, warnings 0 | Yes | `coverage/msbuild-analyzers-final.log` |
| Nullable gate: skipped 0, CoreCompile 13, errors 0, CS86xx 0 | Yes | `coverage/msbuild-nullable-final.log` |
| CSharpier check clean | Yes | `coverage/csharpier-check.log:1` — `Checked 1622 files in 4670ms.` |
| File line counts 228 / 270 / 299 / 489 | Yes | Read each file to its last line |
| Exactly 2 field assignments repo-wide | Yes | `Grep` over `*.cs` |
| Exactly 2 `LoadOpCodes()` invocations repo-wide | Yes | `Grep` over `*.cs` — this is what surfaced N-1 |
| `\.IsInitOnly` = 2 | Yes | `Grep` over the test file |
| Zero determinism-banned APIs in the SDILReader test tree | Yes | `Grep` over the directory |
| No `null!`, `lock`, `volatile`, `Lazy<`, `ExcludeFromCodeCoverage` in `ILGlobals.cs` | Yes | `Grep` over the file |
| `Parallelize` intact at `AssemblyInfo.cs:18-21` | Yes | Read directly |
| No files under `artifacts/{baselines,qa,evidence,coverage}/` | Yes | `Glob` |
| Fail-before runs were real red runs | Evidence-attested | `ExpectedExitCode: 1` + `EXIT_CODE: 1` + discovery totals + per-test tables + named failing assertion |
| `MethodBodyReader.cs` / `MethodBodyReader_Tests.cs` / `AssemblyInfo.cs` / csproj byte-for-byte unchanged | Evidence-attested, strongly corroborated | Git unavailable; corroborated by line counts and identical Cobertura class rates |

---

## 8. Verdict

**APPROVE.** The fix is minimal, correct, and matched to the defect. The invariant is enforced by the
construct that actually provides it rather than by a lock that would narrow the window without
closing it, and `readonly` converts reintroduction from a silent runtime race into a compile error.
The tests are deterministic, both gates are demonstrably red before and green after with
discovery-count controls, and the one test that cannot serve as a gate is labelled as such in four
places including the source. Coverage improved on both changed-file and repo-wide axes and the
changed-line figure is 100 %.

The four non-blocking findings are a stale doc-comment count, an upstream planner base-resolution
defect, an owed follow-up filing, and two documented evidence-location substitutions. None requires
remediation on this branch.
