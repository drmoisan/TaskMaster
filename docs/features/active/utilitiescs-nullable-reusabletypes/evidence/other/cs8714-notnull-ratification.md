# CS8714 `where TKey : notnull` — Maintainer Ratification Dossier (P6-T2)

Timestamp: 2026-07-19T10-06

## Decision required

Whether to add the generic-parameter-list constraint

    where TKey : notnull

to the four generic dictionary base types:

1. `ConcurrentObservableDictionary<TKey, TValue>`
   (`UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Dictionary/ConcurrentObservableDictionary.cs`)
2. `ScoDictionaryNew<TKey, TValue>`
   (`UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/ScoDictionaryNew.cs`)
3. `ScoDictionaryStatic`
   (`UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/ScoDictionaryStatic.cs`)
4. `ScDictionary`
   (`UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/ScDictionary.cs`)

This is a change to the PUBLIC generic parameter list of shared, cross-module base types. Per the
plan Scope Invariants and issue #366 architecture, it must be ratified by the project maintainer
before it is applied or committed. The executor is not the maintainer and does not self-approve it.

## Empirical evidence captured (real diagnostics)

Method: `#nullable enable` was temporarily added to `ConcurrentObservableDictionary.cs` only, the
per-file pragma gate was run
(`msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`,
isolated-compile methodology from P0-T5), the diagnostics were recorded, and the pragma was then
reverted. Post-revert the working tree is confirmed back to the green Phase-5 state
(0 cluster CS86xx, 0 CS8714; `git diff` clean for the file).

Result under the pragma on `ConcurrentObservableDictionary.cs`:
- CS86xx (regular nullable) diagnostics: 30 (CS8602 x4, CS8604 x10, CS8618 x14, CS8625 x2).
- CS8714 diagnostics: 0.

Root cause of the zero CS8714 count: the compilation target is net481 (.NET Framework 4.8.1). The
.NET Framework reference assemblies for `System.Collections.Concurrent.ConcurrentDictionary<TKey,
TValue>` are NOT nullable-annotated and therefore carry no `where TKey : notnull` constraint on this
target. Deriving `ConcurrentObservableDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>`
with an unconstrained `TKey`, and constructing the private `ConcurrentDictionary<TKey, ...> _observers`
field, consequently violate no constraint, so no CS8714 is emitted. (The `notnull` constraint on
`ConcurrentDictionary` exists only in .NET Core / .NET 5+ / annotated reference assemblies.)

Implication: on the CURRENT net481 toolchain the `where TKey : notnull` constraint is NOT required to
clear the per-file pragma gate for these four types. The Batch 6/8 blocker to a clean opted-in build
is the 30 regular CS86xx diagnostics above (annotation-only remediation), not CS8714.

## Rationale for the constraint (forward-looking contract)

The constraint text is exactly:

    where TKey : notnull

Runtime-behavior argument (AC3/AC5): `ConcurrentDictionary` already rejects null keys at runtime
(`ArgumentNullException` on a null key). Adding `where TKey : notnull` is therefore an IL-metadata /
compile-time-contract change only, with no runtime behavior change — it makes explicit a null-key
prohibition that the base type already enforces. As a cross-module contract for downstream Wave-1
epic consumers (and for a future migration to a nullable-annotated .NET target where CS8714 WOULD
fire), applying the constraint proactively keeps the four dictionary contracts consistent and
prevents a later breaking addition. Against applying it: on net481 it is not currently necessary,
and adding a public generic constraint could, in principle, reject a downstream consumer that today
instantiates one of these dictionaries with a nullable/oblivious `TKey`.

## Rejected alternative

`#pragma warning disable CS8714` — rejected. It suppresses rather than fixes, and (a) there is no
CS8714 to suppress on net481, and (b) if the target later becomes nullable-annotated, suppression
would hide a genuine null-key-contract violation instead of expressing the real constraint. Policy
(`.claude/rules/csharp.md`) also prefers fixing diagnostics over suppressing them.

## Decision

RATIFIED: 2026-07-19T22:14:30Z — decided by the project maintainer in-session. The maintainer
ratified adding the `where TKey : notnull` public generic-parameter-list constraint to
`ConcurrentObservableDictionary`, `ScoDictionaryNew`, `ScoDictionaryStatic`, and `ScDictionary`. The
empirical finding above stands: on net481 the BCL reference assemblies are not nullable-annotated, so
ZERO CS8714 is actually emitted; the constraint is adopted as forward-looking public-contract hygiene
consistent with the epic's cross-module-contract intent, with no runtime behavior change (the base
`ConcurrentDictionary` already rejects null keys via `ArgumentNullException`). The constraint is
applied to `ConcurrentObservableDictionary` in [P6-T3] and consistently to `ScoDictionaryNew`,
`ScoDictionaryStatic`, and `ScDictionary` in [P8-T2]; the `ConcurrentBag<T>`-based
`ConcurrentObservableBag` and `ScBag` are NOT constrained. The [P6-T2] STOP is cleared and Phases 6-9
proceed.

Prior status (superseded): BLOCKED pending maintainer ratification. The executor did not apply or
commit the constraint before ratification.

## Post-ratification empirical correction (2026-07-19T22-40) — EXECUTION BLOCKER

When the ratified `where TKey : notnull` constraint was actually applied to
`ConcurrentObservableDictionary<TKey, TValue>` and the full UtilitiesCS assembly was rebuilt
(isolated-compile methodology per P0-T5), the build emitted 4 CS8714 errors — NOT zero. The
ratification's empirical premise ("ZERO CS8714 is actually emitted on net481") was measured only
against the base-class derivation (`: ConcurrentDictionary<TKey, TValue>`) in isolation. It did not
account for a first-party downstream consumer that is already under `#nullable enable`.

Emitted diagnostics (constraint applied):

    UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs(24,61): error CS8714
    UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs(33,63): error CS8714
    UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs(195,63): error CS8714
    UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs(207,66): error CS8714

Root cause: sibling child #367 ("fix(367): remediate nullable-reference-type debt in
UtilitiesCS/NewtonsoftHelpers via per-file #nullable enable", commit c9284b30) is ALREADY merged
onto this feature branch. `WrapperScoDictionary<TDerived, TKey, TValue>` (a NewtonsoftHelpers file)
now carries `#nullable enable`, declares an unconstrained `TKey`, and constructs / references
`ConcurrentObservableDictionary<TKey, TValue>` at the four sites above. Under nullable annotation,
an unconstrained `TKey` does not satisfy the new `notnull` constraint, so CS8714 fires. (On `main`
WrapperScoDictionary.cs has no pragma, so this consumer was null-oblivious and no CS8714 fired —
which is why the earlier isolated measurement missed it.) The same conflict compounds in [P8-T2]:
constraining `ScoDictionaryNew<TKey, TValue>` will additionally break WrapperScoDictionary's
`where TDerived : ScoDictionaryNew<TKey, TValue>` clause.

Conflict (three simultaneously-unsatisfiable directives):
1. [P6-T3]/[P8-T2] ratified: APPLY `where TKey : notnull` to the four dictionary bases.
2. Scope invariant: do NOT touch any NewtonsoftHelpers file (sibling child; out of scope).
3. Gate requirement: the per-file pragma gate and the solution-wide [P9-T3] gate must reach 0 CS8714.

Applying (1) violates (3) via a file that can only be fixed by violating (2). Verified: with the
constraint REMOVED, the cluster is 0 CS86xx / 0 CS8714 and the whole assembly emits 0 CS8714 (only
the 15 pre-existing, out-of-scope CS0168/CS0618 warnings-as-errors remain). All other Batch 6
annotations are correct and green without the constraint.

Executor action: per the run directive ("If any invariant cannot be satisfied, STOP and report the
exact blocker rather than working around it"), execution is HALTED at [P6-T3]. The four dictionary
bases have NOT been constrained and nothing has been committed. All annotation-only Batch 6 work is
applied and green. A maintainer/orchestrator decision is required among:
  (A) Amend scope to permit the mechanically-necessary `where TKey : notnull` on the NewtonsoftHelpers
      consumer(s) (`WrapperScoDictionary<TDerived, TKey, TValue>`), consistent with the ratification's
      stated cross-module-contract intent; or
  (B) Withdraw the constraint from [P6-T3]/[P8-T2] (on net481 it is a no-op; the cluster is already
      green without it) and defer it to a coordinated cross-child integration change; or
  (C) Re-sequence so the constraint lands in a single integration change that also updates the
      NewtonsoftHelpers consumers.

## Epic-layer resolution — OPTION A EXTENDED TO TWO FILES (2026-07-19T23:10:00Z)

Timestamp: 2026-07-19T23-40

The epic layer (epic-orchestrator; the epic owns cross-child boundaries; user informed in-session,
may override) resolved both escalations by extending the Option-A scope-boundary waiver to EXACTLY
TWO #367-owned NewtonsoftHelpers files, adding one `where TKey : notnull` line to each and nothing
else:

1. `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs` — add `where TKey : notnull` to
   `public class WrapperScoDictionary<TDerived, TKey, TValue>`. Clears the 4 CS8714 at lines
   24, 33, 195, 207.
2. `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs` — add `where TKey : notnull` to
   `ScoDictionaryConverter<TDerived, TKey, TValue>`. Clears the 3 CS8714 at lines 27, 28, 40.
   `ScoDictionaryConverter` carries `where TDerived : ScoDictionaryNew<TKey, TValue>` and
   constructs/deserializes `WrapperScoDictionary<TDerived, TKey, TValue>`, so it independently fails
   the `notnull` constraint once WrapperScoDictionary (and, in [P8-T2], `ScoDictionaryNew`) are
   constrained.

Static cascade bound (verified, MUST NOT be edited under this waiver):
- `PeopleScoConverter.cs` — SAFE (the `WrapperScoDictionary` reference is commented-out; active code
  uses concrete type arguments).
- `WrapperPeopleScoDictionaryNew.cs` — SAFE (concrete `ScoDictionaryNew<string,string>` /
  `ConcurrentObservableDictionary<string,string>`).

The cascade is statically bounded at these two files. No other cross-child file may be modified under
this waiver. If a THIRD cross-child consumer surfaces during execution, HALT and re-escalate to the
epic orchestrator; the waiver must NOT be widened unilaterally.

This resolution supersedes the earlier one-file Option-A authorization. It is enacted in the revised
plan tasks [P6-T3] (apply to both #367 files + `ConcurrentObservableDictionary`), [P9-T9]
(verify constraint present on both #367 consumers and no other NewtonsoftHelpers file modified), and
[P9-T10] (both one-line additive constraints expected in the AC5 diff review). It is also recorded in
the child checkpoint `epic_decisions` and `human_interaction` blocks, and is documented in the
constraint-propagation commit message and the PR #380 body per the epic documentation requirement.

## Epic-layer resolution — OPTION A-PRIME EXTENDED TO A THIRD FILE (2026-07-20T00:20 escalation, authorized)

Timestamp: 2026-07-20T01-10

During [P8-T2] the ratified `where TKey : notnull` was applied to the fourth ratified base
`ScDictionary<TKey, TValue>` (`UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/ScDictionary.cs`).
The isolated pragma-gate rebuild then surfaced CS8714 at
`UtilitiesCS/NewtonsoftHelpers/WrapperScDictionary.cs(18,38)` — a THIRD #367-owned NewtonsoftHelpers
consumer distinct from the two files already covered by the two-file waiver above.
`WrapperScDictionary<TDerived, TKey, TValue>` declares `where TDerived : ScDictionary<TKey, TValue>`
with an unconstrained `TKey` under `#nullable enable`, so it independently fails the `notnull`
constraint once `ScDictionary` is constrained. The child orchestrator HALTED per the two-file
waiver's third-consumer re-escalation clause and did NOT widen the waiver unilaterally.

The epic layer (which owns cross-child boundaries) AUTHORIZED extending the waiver to this THIRD
#367-owned file — Option A-prime (A'):

3. `UtilitiesCS/NewtonsoftHelpers/WrapperScDictionary.cs` — add `where TKey : notnull` to
   `public class WrapperScDictionary<TDerived, TKey, TValue>` (declaration at line 18). Clears the
   CS8714 at line 18,38.

THREE-FILE WAIVER TOTAL (one `where TKey : notnull` line each, nothing else):
1. `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs`  [applied in Batch 6]
2. `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs` [applied in Batch 6]
3. `UtilitiesCS/NewtonsoftHelpers/WrapperScDictionary.cs`   [applied under A' in Batch 8 constraint completion]

No other #367-owned (or any other cross-child) file may be modified under this waiver. If a FOURTH
cross-child consumer surfaces, STOP and re-escalate to the epic layer; do NOT widen the waiver
unilaterally.

### Factual correction — ScoDictionaryStatic is non-generic (plan-wording deviation)

`ScoDictionaryStatic` is a NON-GENERIC `static class` of `Type` extension methods with no `TKey`
type parameter. The [P6-T2] ratification's "four generic bases" wording is mechanically inaccurate
for this file: the `where TKey : notnull` constraint is INAPPLICABLE there (nothing to constrain;
0/0 diagnostics regardless). NET EFFECT: the ratified constraint applies to the THREE truly generic
bases — `ConcurrentObservableDictionary`, `ScoDictionaryNew`, and `ScDictionary` — plus their three
wrapper/converter consumers listed above. `ScBag` (`ConcurrentBag<T>`-based) and
`ConcurrentObservableBag` are correctly left unconstrained. This plan-wording deviation is documented
here and in the PR #380 body rather than failing final QC on the literal "four bases" plan text; the
[P9-T9] verification confirms the constraint on the three truly generic bases and on the three
NewtonsoftHelpers waiver consumers.

This resolution supersedes the two-file waiver above only by adding the third file; the two-file
enactment remains in force. It is enacted in the revised plan tasks [P8-T2], [P9-T9], and [P9-T10],
recorded in the child checkpoint `epic_decisions` and `human_interaction` blocks, and documented in
the constraint-propagation commit message and the PR #380 body.
