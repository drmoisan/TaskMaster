---
name: nullable-cs8714-not-on-net481
description: CS8714 (notnull constraint) does NOT fire for ConcurrentDictionary-derived types under #nullable enable on net481 — the BCL reference assemblies are not nullable-annotated
metadata:
  type: project
---

Under a per-file `#nullable enable` pragma on net481 (.NET Framework 4.8.1), deriving from
`ConcurrentDictionary<TKey, TValue>` with an unconstrained `TKey` (e.g.
`ConcurrentObservableDictionary<TKey,TValue> : ConcurrentDictionary<TKey,TValue>`, `ScoDictionaryNew`,
`ScoDictionaryStatic`, `ScDictionary`) emits ZERO CS8714 diagnostics.

**Why:** net481's `System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>` reference
assemblies are NOT nullable-annotated, so the `where TKey : notnull` constraint (which exists only in
.NET Core / .NET 5+ / annotated refs) is absent. With no constraint to violate, no CS8714 is emitted.
Empirically verified on #366 (utilitiescs-nullable-reusabletypes): temporarily enabling the pragma on
`ConcurrentObservableDictionary.cs` produced 30 regular CS86xx (CS8602/8604/8618/8625) but 0 CS8714.

**How to apply:** the epic plan/orchestrator assumed CS8714 requires a `where TKey : notnull`
ratification (a modern-.NET assumption). On net481 it does NOT — the constraint is a forward-looking
contract choice for a future annotated target, not a compile requirement. Do not fabricate a CS8714
diagnostic in a ratification dossier; capture the real (zero) result and frame the constraint as
optional/forward-looking, still maintainer-gated because it is a public generic-parameter-list change.
The real Batch-6/8 blocker is the 30 regular CS86xx, not CS8714. See
[[nullable-pragma-gate-net481-mechanics]].
