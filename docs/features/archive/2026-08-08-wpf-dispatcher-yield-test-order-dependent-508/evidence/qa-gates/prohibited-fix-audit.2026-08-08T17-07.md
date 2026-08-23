# Prohibited-Fix Audit

Timestamp: 2026-08-08T17-07

Task: [P2-T13]

AC served: AC2 (strict contract preserved, assertion not weakened), AC5 (none of the prohibited
approaches used).

## Command

Command: `git diff -- UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`

EXIT_CODE: 0

```
SCOPED_DIFF_LINE_COUNT=270
```

## Why the diff is scoped, not unscoped

The task text binds this check to the **scoped** diff. An unscoped `git diff` would produce a false
positive: `.claude/agent-memory/atomic-planner/MEMORY.md` is tracked, is already modified at branch
head, and its prose contains the literal token `DoNotParallelize` (in a memory entry about MSTest
parallelization), which has nothing to do with this fix.

Scoping loses no coverage of this check, because P1-T15 independently proved that these two files
are the **entire** `.cs`/`.csproj`/`.sln` diff
(`<FEATURE>/evidence/other/scope-boundary.2026-08-08T16-33.md`). Every changed source line in the
repository is therefore inside the 270 lines audited here.

## Grep results — ZERO hits on every prohibited pattern

| Pattern | Prohibited fix it would indicate | Hits |
|---|---|---|
| `DoNotParallelize` | Disabling parallelization as the mechanism of the fix | **0** |
| `Ignore]` | `[Ignore]`-ing the test | **0** |
| `Thread.Sleep` | Sleep / timing hack | **0** |
| `Task.Delay` | Async sleep / timing hack | **0** |
| `Retry` | Retry-until-green | **0** |
| `GetField(` | Reflection mutation of process-global state | **0** |
| `BindingFlags` | Reflection mutation of process-global state | **0** |

Any hit within the scoped diff would fail this task. There are none.

## Assertion integrity — the strict contract is NOT weakened

Command: grep of `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` for
`ThrowAsync<InvalidOperationException>()`

```
ThrowAsync<InvalidOperationException>() OCCURRENCES=1
    line 134: .ThrowAsync<InvalidOperationException>();
```

The assertion in `YieldAsync_WithoutDispatcher_RemainsStrict` is still exactly
`ThrowAsync<InvalidOperationException>()` — the same assertion as the pre-change file captured at
`<FEATURE>/evidence/baseline/source-under-test.2026-08-08T16-12.md`. It was not softened to
`NotThrowAsync`, to a base `Exception` type, to an `Or` condition, or to any predicate that would
hold regardless of the precondition.

The production contract is likewise unchanged: the `InvalidOperationException` message text at
`WpfDispatcherYield.cs:64-66` is byte-identical to the pre-change text, and the
`if (dispatcher is null) throw` guard is intact.

## Against the `## Prohibited Fixes` list in `issue.md`

| Prohibited fix (`issue.md:116-120`) | Used? | Evidence |
|---|---|---|
| Disabling parallelization (`[DoNotParallelize]`) as the mechanism | NO | 0 grep hits; `AssemblyInfo.cs` `Parallelize(Workers = 0, Scope = ClassLevel)` is untouched and out of the diff |
| Adding a retry, sleep, or other timing hack | NO | 0 hits for `Retry`, `Thread.Sleep`, `Task.Delay` |
| `[Ignore]`-ing or deleting the test | NO | 0 hits for `Ignore]`; test count rose 6293 -> 6295, and `YieldAsync_WithoutDispatcher_RemainsStrict` still exists and passes |
| Weakening the assertion | NO | assertion is still `ThrowAsync<InvalidOperationException>()`, verbatim |
| Creating temporary files in tests | NO | P1-T14 grep found no `Path.GetTempFileName`/`Path.GetTempPath`; the test uses only in-memory delegates and an owned thread |

## What was used instead

The injectable delegate seam sanctioned by `.claude/rules/csharp.md` "DI Seams" preference 2 (a
narrow `Func<>` for a single call path where a full interface is excessive). The test now
**arranges** the dispatcher-free precondition by passing providers that return null, instead of
**inheriting** it from ambient thread and process state. The `[Timeout(30000)]` used during the
P0-T12 fail-before probe was temporary and was fully reverted (P0-T14); it does not appear in the
final diff.

Output Summary: PASS. All seven prohibited-fix patterns return **zero hits** across the 270-line
scoped diff of the two in-scope files, and the assertion in
`YieldAsync_WithoutDispatcher_RemainsStrict` remains exactly
`ThrowAsync<InvalidOperationException>()` (1 occurrence, line 134) — not weakened. The diff is
scoped per the task text to avoid the known false positive from the tracked, already-dirty
`.claude/agent-memory/atomic-planner/MEMORY.md`, which contains the literal token
`DoNotParallelize`; scoping loses nothing because P1-T15 proved these two files are the entire
source diff. None of the five approaches in the issue's `## Prohibited Fixes` list was used.
