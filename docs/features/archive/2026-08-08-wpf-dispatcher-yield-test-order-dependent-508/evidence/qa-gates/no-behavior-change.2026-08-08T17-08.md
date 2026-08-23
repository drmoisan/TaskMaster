# No Runtime Behavior Change

Timestamp: 2026-08-08T17-08

Task: [P2-T14]

AC served: AC4 (production change is minimal and preserves the existing runtime resolution order
and exception contract for all existing call sites; no call-site changes required).

Comparand: `<FEATURE>/evidence/baseline/source-under-test.2026-08-08T16-12.md` (verbatim pre-change
capture at merge-base `003c5715`).

## Command

Command: `git diff -- UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`
EXIT_CODE: 0

The complete production diff is four hunks: remove one `using`, remove one attribute, add the
fields and two constructors, and swap the two `??` operands for the seam calls. Nothing else in the
file changed.

## 1. Public surface — gained ONLY the explicit parameterless constructor

Accessibility inventory of the post-change file:

```
12:    public sealed class WpfDispatcherYield : IDispatcherYield
14:        private readonly Func<Dispatcher?> _currentThreadDispatcherProvider;
15:        private readonly Func<Dispatcher?> _fallbackDispatcherProvider;
21:        public WpfDispatcherYield()
37:        internal WpfDispatcherYield(
49:        public async Task YieldAsync(CancellationToken cancellationToken)
```

| Member | Pre-change | Post-change | Assessment |
|---|---|---|---|
| `public sealed class WpfDispatcherYield : IDispatcherYield` | present | unchanged | no change |
| parameterless constructor | implicit `public` | explicit `public` | **binary-compatible; same public signature** |
| `public async Task YieldAsync(CancellationToken)` | present | unchanged | no change |
| seam constructor | — | `internal` | not public surface |
| two provider fields | — | `private readonly` | not public surface |

The **public** API surface is byte-identical in signature terms. The pre-change class declared no
constructor, so it had an implicit public parameterless constructor; the explicit
`public WpfDispatcherYield()` restores exactly that signature, which is mandatory because adding any
constructor removes the implicit one.

## 2. Seam constructor is `internal`, not `public` — VERIFIED

Line 37: `internal WpfDispatcherYield(`.

`internal` is sufficient because `UtilitiesCS/Properties/AssemblyInfo.cs:19` already declares
`[assembly: InternalsVisibleTo("UtilitiesCS.Test")]` (confirmed at P0-T5). This is the strongest
available answer to AC4's "minimal": the testability seam adds nothing to the public API.

## 3. Default delegates match the pre-change expressions exactly

| Operand | Pre-change expression (baseline capture, lines 27-28) | Post-change default (lines 42-46) |
|---|---|---|
| 1 (thread-affinitized) | `Dispatcher.FromThread(Thread.CurrentThread)` | `() => Dispatcher.FromThread(Thread.CurrentThread)` |
| 2 (process-global fallback) | `UtilitiesCS.UiThread.Dispatcher` | `() => UtilitiesCS.UiThread.Dispatcher` |

Both are the same expressions, wrapped in a lambda and selected by `?? ` when the corresponding
constructor argument is null. `new WpfDispatcherYield()` passes `null, null`, so production
behavior is identical to pre-change.

The fallback reads the `UiThread.Dispatcher` property only — a plain static field read at
`UtilitiesCS/Threading/UiThread.cs:135-140`. It does **not** touch `UiThread.UiSyncContext` or
`UiThread.AutoScaleFactor`, both of which call `Init()` and would show a form (P1-T4 acceptance).

## 4. Resolution order preserved inside `YieldAsync`

```csharp
-            Dispatcher dispatcher =
-                Dispatcher.FromThread(Thread.CurrentThread) ?? UtilitiesCS.UiThread.Dispatcher;
+            Dispatcher? dispatcher =
+                _currentThreadDispatcherProvider() ?? _fallbackDispatcherProvider();
```

The `??` remains, in the same place, in the same order: thread-affinitized first, process-global
fallback second, with the fallback evaluated only when the first returns null (C# `??` short-circuit
semantics). The resolution stayed **inside** `YieldAsync` rather than being hoisted into the
constructor, so the test still verifies the ordering rather than replacing it. Line 60's measured
100% (2/2) condition coverage (P2-T12) confirms both directions execute.

The only other change on these lines is `Dispatcher` -> `Dispatcher?` on the local, required for
correct nullable flow analysis (P1-T6). This is a compile-time annotation with no runtime effect.

## 5. Exception contract byte-identical

The `if (dispatcher is null)` guard and the message are untouched — they appear in the diff only as
unchanged context lines:

```
             if (dispatcher is null)
             {
                 throw new InvalidOperationException(
```

Message text, unchanged:
`"The UI dispatcher has not been captured. Call UiThread.Init() before yielding folder tree work."`

The trailing `await dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background, cancellationToken);`
and the post-yield `cancellationToken.ThrowIfCancellationRequested();` are likewise unchanged.

## 6. No call site changed

The two out-of-scope `new WpfDispatcherYield()` call sites identified at P0-T5 are unmodified and
absent from the diff:

| Call site | Status |
|---|---|
| `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs:365` | unchanged, zero edits |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs:55` | unchanged, zero edits |

Confirmed two ways: P1-T15's scoped diff lists only the two in-scope files, and the solution
analyzer build (P2-T3) compiled `TaskMaster.csproj` and `UtilitiesCS.Test.csproj` with 0 errors,
which it could not do if either call site had broken.

`OutlookFolderTreeServiceConcurrencyTests.GetSnapshotAsync_WorkerOriginatedColdBuild_UsesCapturedStaDispatcher`
— the existing test that exercises the parameterless constructor through to dispatcher resolution —
passed in all three repeat runs and in the full-suite pass-4 run, which is behavioral confirmation
that the default path is unchanged.

## 7. Attribute removal is a policy correction, not a behavior change

`[ExcludeFromCodeCoverage]` (line 13 pre-change) and its `using System.Diagnostics.CodeAnalysis;`
(line 3 pre-change) were removed per P1-T7.
`[ExcludeFromCodeCoverage]` affects coverage instrumentation only and has no runtime semantics.
Required by `.claude/rules/general-unit-test.md` "Coverage Exclusion Policy" (no production file may
be excluded from coverage measurement), and now meaningful because P0-T11 established the attribute
was genuinely being honored.

Output Summary: PASS. The production public surface gained only the explicit
`public WpfDispatcherYield()` constructor, which reproduces the signature of the implicit
constructor that adding the seam removed; the seam constructor is `internal` (reachable via the
pre-existing `InternalsVisibleTo("UtilitiesCS.Test")`). The two default delegates reproduce the
pre-change `??` operands exactly, the resolution order and short-circuit remain inside `YieldAsync`,
and the `InvalidOperationException` guard and message text are byte-identical. Neither of the two
out-of-scope call sites changed, proven by the scoped diff and by a 0-error solution build. The only
other edits are the nullable annotation on a local (no runtime effect) and removal of
`[ExcludeFromCodeCoverage]` plus its `using` (instrumentation only). No runtime behavior change.
