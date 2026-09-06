---
name: net48-nominal-record-compiles-only-init-fails
description: CORRECTION — on this net481 target a nominal sealed record with get-only ctor-initialized properties DOES compile; only the init accessor (and therefore positional records / record struct) fails CS0518
metadata:
  type: reference
---

A widely-repeated shorthand in this project's memory is "net48 has no `IsExternalInit`, so
`init`/`record`/`record struct` all fail CS0518 — use a plain readonly struct". **The `record` half
of that is wrong and has been costing real design options.**

**Verified 2026-08-29 against the working tree.** `UtilitiesCS/OutlookObjects/Store/StoreRehookResult.cs:59`
declares `public sealed record StoreRehookResult` with constructor-initialized `{ get; }`
properties. It is a compiled item (`UtilitiesCS/UtilitiesCS.csproj:747`) and its own `<remarks>`
block states the rule precisely:

> Declared as a `sealed record` with get-only properties initialized through the constructor (not
> `init` accessors) ... because `init` accessors require
> `System.Runtime.CompilerServices.IsExternalInit`, which is not available on this .NET Framework 4.8
> target (CS0518).

**The precise rule.** `IsExternalInit` is required by the **`init` accessor**, not by the `record`
keyword. Therefore on this target:

- allowed: nominal `record` / `sealed record` with `{ get; }` properties set in a constructor
- allowed: plain `readonly struct` with `{ get; }` auto-properties (e.g. `ResourceTimingRow`)
- **not** allowed: any `{ get; init; }` accessor
- **not** allowed: a *positional* record (`record Foo(int Bar);`) — the compiler synthesizes `init`
  accessors for the positional parameters
- **not** allowed: `record struct`

`TargetFrameworkVersion` is `v4.8.1` (`UtilitiesCS.csproj:16`, `UtilitiesCS.Test.csproj:17`), and no
`IsExternalInit` polyfill exists in any production `.cs` file.

**How to apply:** when weighing a small result/outcome type, do not discard `record` on the strength
of the shorthand. Discard `init` and positional syntax. Copy the `StoreRehookResult` shape, which
already carries the explanatory remark a reviewer will look for. The user-memory entry
`reference_net48_no_init_record_struct.md` states the imprecise form and should be read with this
correction in mind.
