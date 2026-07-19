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

BLOCKED: awaiting project-maintainer ratification of the `where TKey : notnull` public
generic-parameter-list change on `ConcurrentObservableDictionary`, `ScoDictionaryNew`,
`ScoDictionaryStatic`, and `ScDictionary`. The executor has NOT applied or committed the constraint.
Note the empirical finding above: the constraint is not required to clear the net481 pragma gate, so
the maintainer decision is whether to add it as a forward-looking contract (consistent with the
epic's cross-module-contract intent) or to defer it until the target is nullable-annotated. Phases
6-9 (which depend on this decision) are out of scope for this execution run and remain unstarted; the
[P6-T2] plan checkbox is left UNCHECKED.
