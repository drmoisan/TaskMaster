# Code Review — test-determinism-and-hygiene-debt (Issue #729)

Timestamp: 2026-09-03T07-30

- Head SHA: `e6c488bf46cec739bddcf4ee07ba070c45b85668`
- Base anchor: `8be5a6aac3b5a82c86241fbbf989fd9118602c56` (independently re-derived as the merge-base against `origin/main`)
- Scope reviewed: the full anchored branch diff — 81 paths, 56 added, 17 deleted, 8 modified.

## Summary

**Blocking findings: 0.** One out-of-scope observation confirmed pre-existing (OBS-1), three advisory code-quality notes (CR-1 through CR-3), and one positive note on the change's effect on evidence quality.

The change is well shaped for its purpose. The production edit is minimal and additive — one new overload, one type substitution, no behavioural change to the existing entry point. The test rewrites replace a timing-dependent assertion with a strictly stronger one. The two guard classes are correctly scoped to metadata reflection and correctly defended against a whole-assembly reflection failure. The 17 deletions were verified safe by direct search rather than by inference.

## 1. The production seam — `TaskMaster/AppGlobals/NonBlockingDelay.cs`

### The overload-pair decision is correct and the constraint still holds

The change declares an explicit overload pair rather than adding an optional `TimeProvider` parameter to the existing method. The stated reason is that `WaitAsync` is consumed as a method group and an optional parameter would produce CS0123 at that site. Both halves of that were verified.

Call-site enumeration (`grep -rn "NonBlockingDelay" <worktree> --include=*.cs --include=*.csproj`) returns exactly two production call sites, matching the spec's claim:

- `TaskMaster/AppGlobals/StoreRehookCoordinator.cs:102` — `_delay = delay ?? NonBlockingDelay.WaitAsync;`
- `TaskMaster/AppGlobals/AppEvents.cs:456` — `await NonBlockingDelay.WaitAsync(TimeSpan.FromMilliseconds(100));`

The first is a method-group conversion. The target type is `Func<TimeSpan, Task>?`, declared as the seventh constructor parameter of `StoreRehookCoordinator` (`StoreRehookCoordinator.cs:82`) and assigned to the `_delay` field. A method-group conversion ignores a candidate method when one or more of its optional parameters has no corresponding parameter in the target delegate type, so `WaitAsync(TimeSpan delay, TimeProvider timeProvider = null)` would leave zero applicable candidates and yield CS0123 at line 102. The constraint holds as described. Two explicit overloads restore an unambiguous one-parameter candidate.

Confirming evidence rather than reasoning alone: `evidence/qa-gates/msbuild-analyzers.2026-09-02T10-30.md` records a fixed-string search of the analyzer rebuild log returning zero `CS0123` occurrences across 75 `CoreCompile:` executions. Neither call site was edited — neither file appears in the anchored diff.

Note that the natural precedent for an optional-parameter seam, `UtilitiesCS/Threading/ThreadMonitor.cs`, is a *constructor*, and constructors are never converted to delegates. The reason that precedent could not be followed here is real and specific, not a preference.

### The type substitution is faithful

`Timer? timer = null` becomes `ITimer? timer = null`; `new Timer(callback, null, delay, Timeout.InfiniteTimeSpan)` becomes `timeProvider.CreateTimer(callback, null, delay, Timeout.InfiniteTimeSpan)`. The callback body, argument order, due time, infinite period, `TaskCompletionSource<bool>` construction, and `TaskCreationOptions.RunContinuationsAsynchronously` are all unchanged. The 1-arg overload's entire body is `return WaitAsync(delay, TimeProvider.System);`, which preserves the prior behaviour for both production call sites exactly.

The narrowly-scoped `#nullable enable annotations` / `#nullable restore annotations` pragma pair around the nullable local is preserved. That is the right call in this file: the file has no whole-file pragma and no project-level `<Nullable>` element, so the narrow scope avoids conscripting the rest of the file into nullable analysis while still suppressing CS8632 on the one annotation that needs it. The nullable rebuild records zero `CS8632` and zero `CS86xx` across 55 `CoreCompile:` executions.

The doc comment was updated honestly rather than left stale: the banned-API paragraph now names both `TimeProvider` and `System.Threading.Timer`, and a new paragraph records the CS0123 constraint so the next reader does not "simplify" the overload pair back into an optional parameter and break `StoreRehookCoordinator`. That comment is doing real work.

### OBS-1 — `timer?.Dispose()` no-ops when the callback fires during `CreateTimer` (confirmed pre-existing, out of scope)

Location: `TaskMaster/AppGlobals/NonBlockingDelay.cs:78-83`.

```csharp
ITimer? timer = null;
timer = timeProvider.CreateTimer(
    _ =>
    {
        timer?.Dispose();
        tcs.TrySetResult(true);
    },
    null,
    delay,
    Timeout.InfiniteTimeSpan
);
```

The callback captures `timer` before the assignment completes. When the timer fires during `CreateTimer` — or, with a real `System.Threading.Timer`, on a threadpool thread that wins the race against the assignment — `timer` is still `null`, `timer?.Dispose()` short-circuits, and the timer instance is never disposed.

**Confirmed pre-existing.** `git show 8be5a6aac3b5a82c86241fbbf989fd9118602c56:TaskMaster/AppGlobals/NonBlockingDelay.cs` carries the identical construct at its lines 55-64, character for character apart from the concrete type. The change swaps `new Timer(...)` for `timeProvider.CreateTimer(...)` and `Timer?` for `ITimer?`; it does not introduce, widen, or narrow the race. **Refuted as a new defect; confirmed as pre-existing.**

One qualification worth recording, because it changes what the evidence now shows rather than what the code does. The path was previously unexercised: the baseline Cobertura records `condition-coverage="50% (1/2)"` on that line. Post-change it records `condition-coverage="100% (2/2)"`. The new `WaitAsync_ZeroDelay_CompletesWithoutPump` test uses `FakeTimeProvider`, which invokes a zero-due-time one-shot timer during `CreateTimer`, so the null branch is now taken deterministically on every run. The undisposed-timer path is therefore no longer theoretical — it is demonstrated by the suite. This does not make the change responsible for the defect, and it does not make the leak newly harmful (a `FakeTimeProvider` timer holds no OS handle). It does mean a follow-up fix now has a ready-made deterministic reproduction.

Suggested shape for the follow-up, recorded so the issue does not have to rediscover it: capture into a local before publishing, for example by assigning the `ITimer` to a local that the callback reads through a `TaskCompletionSource` continuation, or by having the callback dispose via a captured `StrongBox<ITimer>` written before `CreateTimer` returns. Any fix must keep the 1-arg signature intact for `StoreRehookCoordinator.cs:102`.

**Disposition: out-of-scope observation for follow-up. Not blocking.** The bugfix minimal-change rule in the General Code Change Policy directs opening a new issue rather than widening scope, which is exactly what the correct handling is here.

### CR-1 (advisory) — no precondition guard on `timeProvider`

Location: `TaskMaster/AppGlobals/NonBlockingDelay.cs:66` — `public static Task WaitAsync(TimeSpan delay, TimeProvider timeProvider)`.

A `null` argument produces a `NullReferenceException` at the `timeProvider.CreateTimer(...)` call rather than an `ArgumentNullException` naming the parameter. CLAUDE.md § C#4.3 ("Validate constructor and method preconditions") and the General Code Change Policy's fail-fast-and-explicitly rule both point at a guard here. The immediately adjacent `StoreRehookCoordinator` constructor throws `ArgumentNullException` for all seven of its dependencies, so the repository style is unambiguous on this point.

Severity is low: the class is `internal`, the 1-arg overload always supplies `TimeProvider.System`, and the only other caller is the test project, which always supplies a `FakeTimeProvider`. There is no reachable production path that can pass null today.

Not raised as a required change, for two reasons. First, adding a guard line would add an uncovered line to a file the spec requires at 100% coverage unless a matching negative test were also added, which is scope the plan did not authorize. Second, the bugfix minimal-change rule argues against opportunistic hardening in a defect fix. Recorded as advisory so a future edit to this file can pick it up.

## 2. The two guard classes

Both files were read in full. `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` and `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` are byte-identical apart from the namespace line and a message improvement.

### Verified: metadata only, nothing instantiated

The method body is:

```csharp
Type formType = typeof(System.Windows.Forms.Form);
Assembly executing = Assembly.GetExecutingAssembly();
string[] formDerivedTypeNames = GetLoadableTypes(executing)
    .Where(candidate => formType.IsAssignableFrom(candidate))
    .Select(candidate => candidate.FullName)
    .OrderBy(name => name, StringComparer.Ordinal)
    .ToArray();
```

`typeof`, `Assembly.GetExecutingAssembly()`, `Assembly.GetTypes()`, `Type.IsAssignableFrom`, `Type.FullName` — every operation is metadata-level. No constructor is invoked, no `Form` handle is created, and no message pump is required. The scope is correctly limited to the executing assembly, so a referenced assembly's `Form` types cannot produce a false failure. `OrderBy(..., StringComparer.Ordinal)` makes the failure message deterministic. **Verified.**

### Verified: `ReflectionTypeLoadException` fallback

```csharp
private static Type[] GetLoadableTypes(Assembly assembly)
{
    try { return assembly.GetTypes(); }
    catch (ReflectionTypeLoadException ex)
    {
        return ex.Types.Where(candidate => candidate != null).ToArray();
    }
}
```

Present and correct in both files. `Assembly.GetTypes()` throws for the whole assembly when any single type's dependencies fail to resolve, and `ex.Types` carries the partially-loaded set with `null` entries for the failures. Filtering the nulls degrades the guard to the loadable subset rather than leaving it permanently red for a reason unrelated to what it measures. The `catch` is narrowly typed to `ReflectionTypeLoadException` rather than broad, which satisfies the error-handling rule, and the accompanying comment explains *why* the degradation is preferred rather than restating what the code does. **Verified.**

The degradation is a deliberate and correctly documented trade-off: a `Form`-derived type that fails to load would be silently excluded from the check. That is the right choice for a guard whose purpose is to prevent regression rather than to prove a negative under adversarial conditions, and it matches the existing precedent.

### Fidelity to the precedent

```
diff QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs
```

Two differences: the namespace, and the `because` argument. The new copies append `string.Join(", ", formDerivedTypeNames)` to the message. That is a real improvement, and a necessary one — `evidence/regression-testing/svgcontrol-guard-fail-before.2026-09-02T10-30.md` shows FluentAssertions 8.10.0 rendering only one representative item (`but found at least one item {"SVGControl.Test.Form1"}`), so without the enumeration the second offending type could never have been named. AC11 requires the red-before run to name both types; the message change is what makes that requirement satisfiable rather than a matter of luck.

### CR-2 (advisory) — one async test does not use the fake-timer facility

Location: `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs:104-127`, `WaitAsync_SingleArgumentOverload_CompletesOnSystemTimeProvider`.

The test awaits `NonBlockingDelay.WaitAsync(TimeSpan.Zero)`, which routes through `TimeProvider.System` and therefore a real `System.Threading.Timer`. `.claude/rules/general-unit-test.md` § Determinism Infrastructure states that async tests must use the framework's fake-timer facility to advance simulated time deterministically; this test advances no time and uses no fake.

Assessed as advisory rather than a violation:

- The due time is `TimeSpan.Zero`, so no wall-clock duration is waited on.
- The assertion is `waitTask.Status.Should().Be(TaskStatus.RanToCompletion)` — a completion assertion, not a duration assertion. The banned pattern the rule targets is a test whose *outcome* depends on elapsed real time; this test's outcome does not.
- `[Timeout(5000)]` bounds the failure mode to a diagnosable timeout rather than a hang.
- The test is required by AC6. `StoreRehookCoordinatorTests` supplies an explicit `delay` at both construction sites and never reaches the `NonBlockingDelay.WaitAsync` fallback, so without this test the 1-arg overload body would be uncovered and the file could not reach the 100% the spec requires.
- The alternative — mocking `TimeProvider.System` — is not available, and injecting a fake into the 1-arg overload would defeat the purpose of testing the 1-arg overload's own delegation.

The residual risk is that a threadpool starved for more than five seconds would fail this test. That is a real but small exposure, and it is strictly smaller than the 30 ms `Stopwatch` comparison it replaced. The XML doc comment on the test states the reasoning explicitly, so a later reader will not mistake it for an oversight.

No change requested. Recorded so the deviation is on the record rather than discovered later as an unexplained inconsistency.

### CR-3 (advisory) — three identical copies of the guard class

Locations: `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs`, `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs`, `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` — 56 lines each, identical apart from the namespace and (in the two new copies) the message enumeration.

The General Code Change Policy's reusability principle argues against copy-paste. Three mitigating facts argue that duplication is the right answer here:

1. `Assembly.GetExecutingAssembly()` must be evaluated *inside* the assembly under test. A shared helper would have to accept the assembly as a parameter, and the calling test method would still have to live in each assembly to supply it — so the shared portion would be the `GetLoadableTypes` helper and the LINQ query, roughly 25 of the 56 lines.
2. There is no shared test-support assembly that all three test projects reference. Creating one is a structural change well outside a minimal bugfix.
3. The duplication follows an existing in-repo precedent rather than establishing a new one. `QuickFiler.Test` already carried this exact file.

A drift risk exists: the two new copies already differ from the original in their `because` argument, so the three are no longer identical. If a fourth assembly needs the guard, extracting a shared `NoLiveFormGuard.Assert(Assembly)` helper into a test-support library would be the point to do it. Recorded as advisory, not requested for this change.

## 3. The `[DoNotParallelize]` additions

Locations: `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs:8-14` and `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs:9-15`.

Each adds exactly seven lines — a six-line hazard comment and the attribute. The diff confirms no test body, assertion, or method name changed in either file.

### The target-class selection was verified independently

The spec claims these are the only two compiled `UtilitiesCS.Test` classes that capture, restore, and assert on `Console.Out` without the attribute. That is a substantive claim and it was checked rather than accepted:

```
grep -rln "Console.SetOut" UtilitiesCS.Test --include=*.cs   -> 28 files
grep -rln "StringWriter"   UtilitiesCS.Test --include=*.cs   -> 10 files
```

The intersection is five files: `BayesianSerializationHelper_Tests.cs`, `PrettyPrint_Tests.cs`, `OlTableExtensions_Tests.cs`, `DASLFilterParserTests.cs`, `StackGeek_Tests.cs`. Inspecting each:

- `BayesianSerializationHelper_Tests.cs` calls `Console.SetOut(new DebugTextWriter())` once in its initializer for noise suppression and never restores or asserts; its `StringWriter` uses at lines 510 and 522 are unrelated to console capture. Not a member of the set.
- `PrettyPrint_Tests.cs` (lines 196, 218) and `OlTableExtensions_Tests.cs` (lines 1640, 1645) capture, restore, and assert — and both already carried `[DoNotParallelize]` before this change. They are the two precedents the hazard comments cite.
- `DASLFilterParserTests.cs` (lines 109, 118) and `StackGeek_Tests.cs` (lines 153, 162) capture, restore, and assert — and were the only two without the attribute.

The remaining 23 `Console.SetOut` files use the one-way `Console.SetOut(new DebugTextWriter())` initializer pattern and never assert on captured text. `NLogTraceWriter_Test.cs` is the one near-miss: it captures `originalOut` at line 22 and restores it at line 56, but it asserts through Moq callbacks rather than on captured console text, so it has no failing mode of its own. The spec records that exclusion explicitly at its out-of-scope section. **The selection is exactly right.**

### The hazard comment corrects a stale citation, and the correction is accurate

The two precedent comments cite `TaskMaster.runsettings` as the source of the class-level parallel scope. The new comments deliberately do not repeat that, citing `UtilitiesCS.Test/Properties/AssemblyInfo.cs` lines 18-21 instead and stating that the CI vstest invocation passes no `/Settings:` argument.

Both halves verified:

- `UtilitiesCS.Test/Properties/AssemblyInfo.cs` lines 18-21 are the `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]` declaration. Line numbers are correct.
- `.github/workflows/_mstest-coverage.yml:83` is `& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`. No `/Settings:` argument. The claim is accurate.

Declining to propagate a stale citation into new code is the right call, and stating the correction in the comment rather than silently diverging from the precedent is the right way to do it.

### On the absence of a red-before run

`evidence/regression-testing/fail-before-exception.2026-09-02T10-30.md` argues that no deterministic red run is producible for Finding 3, because the failure requires a specific `Console.SetOut` interleaving across two threads that cannot be forced from test source, and because the remedy's effect is the *absence* of a nondeterministic failure, which no single run demonstrates.

That reasoning is sound and the dossier is the right disposition. It states plainly that none is claimed rather than manufacturing a weak proxy, and it substitutes the correct alternative evidence: two in-repo precedent classes that were marked in response to this same hazard, quoted with file and line. This is a case where "no red run exists" is the honest answer and the artifact says so.

## 4. The deletions

17 files, verified individually. The two groups have materially different character and the change treats them correctly as different.

**`SVGControl.Test` (6 files) — the real defect.** `Form1.cs`, `Form1.Designer.cs`, `Form1.resx`, `Form2.cs`, `Form2.Designer.cs`, `Form2.resx` were genuinely compiled: the csproj diff removes four `<Compile>` entries (two carrying `<SubType>Form</SubType>`) and two `<EmbeddedResource>` entries. These are live `Form`-derived types in a unit-test assembly, which is what the guard now forbids. No test in that assembly referenced either type — confirmed by the empty grep across `SVGControl.Test`.

**`UtilitiesCS.Test` (11 files) — orphans.** `ResourceTests.cs`, the three `Form` triplets, and `OutlookObjects/DASLFilterParser_Tests.cs` were on disk but absent from the explicit `<Compile Include>` list in a csproj with no wildcard globbing. The `Form1 frm = new Form1();` at `ResourceTests.cs:20` that the issue cites could never execute, because the file was never compiled. The csproj diff for `UtilitiesCS.Test` is a single added line — the guard registration — with no removals, which is itself the proof that nothing was un-registered because nothing was registered.

The change is honest about this distinction rather than papering over it: `evidence/other/scope-recap.2026-09-02T10-30.md` states that acting on the issue's literal citation alone "would satisfy the letter of the issue while leaving the actual defect untouched," and expands scope to `SVGControl.Test` for that reason. That is the correct call and the correct way to record it.

Safety confirmed by direct search rather than inference: `grep -rn "Form1|Form2|Form3|ResourceTests"` across both test projects with `--include=*.cs --include=*.csproj --include=*.resx --include=*.config` returns empty output. The only surviving `DASLFilterParser` reference in `UtilitiesCS.Test.csproj` is line 271, pointing at the retained `OutlookObjects\Filter DASL\DASLFilterParserTests.cs`.

## 5. Package and project file changes

`TaskMaster.Test/TaskMaster.Test.csproj` gains two `<Reference>` elements and `TaskMaster.Test/packages.config` two `<package>` entries, for `Microsoft.Bcl.TimeProvider` 10.0.11 and `Microsoft.Extensions.TimeProvider.Testing` 10.9.0.

Mirroring against `UtilitiesCS.Test` was verified attribute by attribute. `UtilitiesCS.Test.csproj` lines 592-593 and 644-645 carry the same `Version`, `PublicKeyToken`, `processorArchitecture`, and `lib\net462\` `HintPath` values; `UtilitiesCS.Test/packages.config` lines 23 and 91 carry the same `version` and `targetFramework="net481"`. The `net462` lib path under a `net481` target framework matches the existing `Microsoft.Bcl.AsyncInterfaces` entry, so it follows repository practice rather than introducing a new pattern.

`TaskMaster.Test/app.config` is correctly unmodified. It already carried a `Microsoft.Bcl.TimeProvider` binding redirect at line 267 before this change — verified by grep, not assumed from the spec's assertion. `Microsoft.Extensions.TimeProvider.Testing` needs no redirect and none was added. Given that two `vstest` binaries in this repository differ in how they honour binding redirects, an unnecessary redirect edit here would have been a real risk; not making one is the right outcome.

Both new packages are test-only. No production project file changed.

## 6. Positive note — the evidence quality is unusually high

Two things are worth recording because they are not the norm and they materially reduced the cost of this review:

`evidence/qa-gates/coverage-delta.2026-09-02T10-30.md` carries a "What this artifact does not claim" section that explicitly disclaims a root cause for the `-6` covered-line movement in `PropertyStore.cs` and states that clause 3 is decided on write-set attribution rather than on causation. That is the correct handling of an unexplained observation, and it is the difference between an artifact that can be trusted and one that has to be re-derived from scratch.

`evidence/regression-testing/svgcontrol-guard-fail-before.2026-09-02T10-30.md` explains *why* only one type name appears in the FluentAssertions "found at least one item" clause and why the `because` argument had to enumerate the collection to satisfy AC11. That is a message-shape limitation that would have looked like a discrepancy to a reviewer reading the failure text against the AC; pre-empting it saved a false finding.

## 7. Consolidated Findings Table

| ID | Location | Rule or expected behaviour | Severity | Disposition |
|---|---|---|---|---|
| OBS-1 | `TaskMaster/AppGlobals/NonBlockingDelay.cs:78-83` | `timer?.Dispose()` no-ops when the callback fires during `CreateTimer`; the `ITimer` is not disposed on that path | Observation | Confirmed pre-existing on `8be5a6aa` character for character. Out of scope for this bugfix; recommend a follow-up issue. Now has a deterministic reproduction via the zero-delay test. **Not blocking.** |
| CR-1 | `TaskMaster/AppGlobals/NonBlockingDelay.cs:66` | CLAUDE.md § C#4.3 — validate method preconditions; a null `timeProvider` yields NRE rather than `ArgumentNullException` | Advisory | No reachable production path can pass null; adding a guard would add an uncovered line against a 100% requirement. **Not blocking.** |
| CR-2 | `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs:104-127` | `.claude/rules/general-unit-test.md` § Determinism Infrastructure — async tests use the fake-timer facility | Advisory | Required by AC6 to cover the 1-arg overload; `TimeSpan.Zero` due time, completion-only assertion, `[Timeout(5000)]` bound. Deviation is documented in the test's own doc comment. **Not blocking.** |
| CR-3 | Three copies of `NoLiveFormInTestAssemblyTests.cs` | General Code Change Policy § reusability — avoid copy-paste | Advisory | `GetExecutingAssembly()` must evaluate in-assembly; no shared test-support library exists; follows existing precedent. Extract if a fourth assembly needs it. **Not blocking.** |

Documentation-accuracy findings DOC-1 through DOC-4 are recorded in `policy-audit.2026-09-03T07-30.md` § 12 rather than duplicated here. All four are non-blocking.

## Verdict

**Approve. 0 blocking findings.**

The production change is minimal, additive, correctly motivated by a real language constraint, and fully covered. The test changes remove a genuine wall-clock dependency and replace it with a stronger assertion. The structural guards are correctly scoped and correctly defended. The deletions are verified safe. The four advisory items are recorded for future work and none of them warrants holding this change.
