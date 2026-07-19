---
name: nullable-remediation-annotation-patterns
description: Reusable net481/C#12 nullable-annotation patterns for the UtilitiesCS remediation epic — no post-condition attrs, EmailRecord struct = default!, ToString()! for string cells, overload-resolution gotcha
metadata:
  type: project
---

Patterns proven on #363 (UtilitiesCS/Extensions) that recur across the nullable-remediation epic. See [[nullable-pragma-gate-mechanics]].

**Why:** net481/C#12 has NO nullable post-condition attributes ([NotNullWhen], [MaybeNullWhen], etc.) and no polyfill; they MUST NOT be used/added. `#nullable enable` goes just inside the namespace brace (repo convention: pragma + blank line before the class), and is file-scoped so BOTH files of a partial class need it.

**How to apply (recurring fixes):**
- Unconstrained-generic null-state: `out TValue?`, `T?` returns, `default(T)!` for intentional null-substitution (e.g. CastNullSafe iterator, FlattenArrayTree). Keep `where T : struct`/`where T : Enum` overloads free of reference-nullable annotations.
- Reflection returns are nullable: `MethodBase.GetCurrentMethod()!.DeclaringType!`, `Type.FullName` -> string? (use `!` if a non-null string is required), `Activator.CreateInstance(...)!`, `PropertyInfo.GetValue(...)!`, `ParameterInfo.Name` -> string?.
- `object.ToString()` returns string? -> assigning to a `string[]`/`string[,]` cell needs `!` (e.g. `array[i,j]!.ToString()!`), commented as behavior-preserving.
- `struct` records that must stay plain (Deedle EmailRecord): keep `private struct`, convert `public string X = default;` -> `= default!` (never record/record struct/init -> CS0518 on net481).
- Overload gotcha: after making a `string` param nullable, `methodName.IsNullOrEmpty()` may rebind from `StringExtensions.IsNullOrEmpty(this string?)` to `NullExtensions.IsNullOrEmpty<char>(IEnumerable<char>)` (a string IS IEnumerable<char>) and emit CS8604; replace with `string.IsNullOrEmpty(methodName)` (behavior-identical).
- Flow-state from defensive `x?.M()` / `if (null != x)` on a non-null-contract value makes later `x.M()` warn CS8602/CS8604; use `x!.M()` to preserve the original unconditional call.
- Verify each batch with a NORMAL `msbuild TaskMaster.sln -t:Build` (no TWAE) -> must be exit 0 to confirm the nullable-annotated public signatures break no existing caller before running tests.
