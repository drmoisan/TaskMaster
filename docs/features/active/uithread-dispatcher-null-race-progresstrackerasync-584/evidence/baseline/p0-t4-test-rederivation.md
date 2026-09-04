# P0-T4 — Re-derivation of the test-side facts

Timestamp: 2026-09-03T08-20

Command:
```text
cat -n UtilitiesCS.Test/Threading/UiThread_Tests.cs
wc -l UtilitiesCS.Test/Threading/UiThread_Tests.cs
sed -n '135,170p' UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs
cat -n UtilitiesCS.Test/Properties/AssemblyInfo.cs
```

EXIT_CODE: 0 (all four commands)

## Output Summary

### UtilitiesCS.Test/Threading/UiThread_Tests.cs

- Total line count: **104** (`wc -l`).
- Namespace: **`UtilitiesCS.Test.Threading`**, declared on line 6.
- Existing `using` directives — exactly four, in this order:
  1. line 1 `using System;`
  2. line 2 `using System.Threading;`
  3. line 3 `using FluentAssertions;`
  4. line 4 `using Microsoft.VisualStudio.TestTools.UnitTesting;`

  **`using System.Reflection;` is absent.** This is the fact P1-T1 acts on: it inserts that directive
  as the second entry, giving five directives in the order `System`, `System.Reflection`,
  `System.Threading`, `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`.
- The file declares one `[TestClass]`, `SynchronizationContextAwaiter_Tests`, whose attribute is on
  line 8 and whose closing brace is on line 103; the namespace's closing brace is on line 104. That
  is the insertion point P1-T2 appends after.
- The file carries no `[DoNotParallelize]` attribute.

### Reflection idiom used to reach the private static backing field

Taken from the helper region of `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`
(`DispatcherField` / `ForceDispatcherNull` / `RestoreDispatcher`), the idiom is:

```csharp
        private static FieldInfo DispatcherField()
        {
            return typeof(UiThread).GetField(
                "_dispatcher",
                BindingFlags.NonPublic | BindingFlags.Static
            );
        }
```

It takes the field by the name `_dispatcher` with non-public static binding flags. `ForceDispatcherNull()`
captures the prior value with `field.GetValue(null)` before writing null with `field.SetValue(null, null)`,
so the caller can restore it. P1-T2's new class reuses this idiom verbatim.

### UtilitiesCS.Test/Properties/AssemblyInfo.cs

Line 18 carries the assembly-level parallelisation attribute:

```csharp
[assembly: Parallelize(
    Workers = 0,
    Scope = Microsoft.VisualStudio.TestTools.UnitTesting.ExecutionScope.ClassLevel
)]
```

Classes in this assembly therefore run concurrently by default at class-level scope. That is the
justification for the `[DoNotParallelize]` attribute P1-T2 places on the new class and for P1-T5's
attribute-only edits to the three existing files that write the process-global
`UiThread._dispatcher`.

All five values asserted by P0-T4 match: 104 total lines; namespace `UtilitiesCS.Test.Threading`;
`System.Reflection` using directive absent; the reflection idiom takes `_dispatcher` with
`BindingFlags.NonPublic | BindingFlags.Static`; the assembly-level parallelisation attribute is
present on line 18 of `UtilitiesCS.Test/Properties/AssemblyInfo.cs`.
