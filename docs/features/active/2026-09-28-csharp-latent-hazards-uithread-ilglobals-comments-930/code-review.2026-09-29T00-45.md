# Code Review: csharp-latent-hazards-uithread-ilglobals-comments (Issue #930)

- Timestamp (caller-supplied artifact stamp): 2026-09-29T00-45
- Branch: `bug/csharp-latent-hazards-uithread-ilglobals-comments-930` against `origin/main` (self-anchor `ac819907f479ee18026993054e714dc2e056142f`)
- Scope: the full branch diff, six code files (four production, two test); feature-folder documents read in full.
- Method: Read of every changed file at head; comparison against the caller-supplied diff and the executor's fix-applied artifacts; Grep-based verification of claims.

## Executive Summary

The change set is small, targeted and consistent with the bugfix workflow. Each production edit is the minimal form the issue asked for: one boolean operand plus a one-line why-comment in `UiThread.cs`; two field deletions in `ILGlobals.cs`; two numeral removals in XML doc comments. The tests are discriminating (both regression tests were recorded failing on unmodified source), follow the existing patterns in their classes, use FluentAssertions with because-clauses, and add no serialization, sleep, retry or temporary file. No Blocking finding. One Non-blocking finding concerns committed evidence text rather than code (an absolute Program Files path on two lines, contradicting AC7's literal text). The remaining findings are informational: a now-unused `using` directive in `ILGlobals.cs`, the PR-body obligation for the public API removal, and pre-existing conditions outside the footprint.

Counts: Blocking 0; Non-blocking 1; Informational 5.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Non-blocking | evidence/baseline/baseline-04-mstest-coverage.md; evidence/qa-gates/final-06-mstest-coverage.md | line 11; line 12 | Committed evidence carries the absolute path `VS-INSTALL-ROOT\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`. | Replace the prefix with a placeholder (for example `PROGRAM-FILES\...` or `VSTEST-CONSOLE`) on both lines; commit in the exempt docs-only form; re-check AC7. | AC7 states "no committed file contains an absolute host path"; the plan's executor note requires placeholders for values outside the repository. No account or host name is disclosed, so the finding is not identity-bearing. | Grep of the feature folder for `[A-Za-z]:[\\/][A-Za-z]` returned exactly these two lines. |
| Informational | UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs | line 3 | `using System.Collections.Generic;` appears unused after the `Dictionary<int, object> Cache` deletion. | Remove on a later touch of the file, or now if the folder is revised for the Non-blocking finding; not required for merge. | Minimal-fix rule permits leaving it; no enabled analyzer flags it (0 warnings). | Grep of the file for `Dictionary|IList|List<|IEnumerable|ICollection|HashSet|KeyValuePair|Queue<|Stack<|IReadOnly|Comparer<|EqualityComparer` returned zero matches. |
| Informational | UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs | former lines 113 and 131 | Removal of two public static fields is a breaking public API change with no in-repo consumer. | Name `ILGlobals.Cache` and `ILGlobals.modules` in the PR body as removed public members. | General Code Change Policy section 7: call out breaking changes clearly in the change description. | 863-build-green.md (0 errors solution-wide); 863-reference-search.md; reviewer Grep over `*.cs` for `ILGlobals\.(Cache|modules)\b`: zero matches. |
| Informational | UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs | lines 292 to 313 | `PublicStaticFields_AreExactlyTheTwoOpCodeTables` pins the public static field surface by name, so any future public static field addition (including a readonly one) fails this test. | None required; the XML doc states this intent. Whoever adds a public static field later must update the expected-name array deliberately. | Deliberate surface pin; documented; consistent with the issue's Expected Behavior 2. | Test source lines 288 to 313. |
| Informational | UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs | whole file | The file hosts two test classes (`UiThreadPredicateHardening_Tests`, `UiThreadApartmentMeasurement_Tests`); its name matches only the second. Pre-existing from #816. | Optional later split into `UiThreadPredicateHardening_Tests.cs`; outside this branch's scope. | File cohesion guidance; the plan placed the new test in the existing class deliberately (Decision D12) to reuse the class-level `[DoNotParallelize]`. | File lines 22 to 24 and 153 to 155. |
| Informational | UtilitiesCS/UtilitiesCS.csproj; VBFunctions.csproj; SVGControl.Test.csproj | `<Analyzer Include>` items (UtilitiesCS.csproj line 1316) | Analyzer HintPath versions (Meziantou.Analyzer 3.0.235, MSTest.Analyzers 4.4.0) differ from packages.config (3.0.290, 4.4.1); pre-existing on origin/main; unchanged by this branch. | Open a follow-up issue to align the `<Analyzer Include>` versions with packages.config. | Fresh worktrees fail the analyzer Rebuild with CS0006 until the older packages are installed manually. | baseline-02-analyzers.md; reviewer Grep of UtilitiesCS.csproj (line 3 imports 3.0.290 props; line 1316 references 3.0.235). |

## Detailed Review

### UtilitiesCS/Threading/UiThread.cs (lines 193 to 203 at head)

- The dispatcher exit now reads `_context is DispatcherSynchronizationContext && _dispatcher is not null && ReferenceEquals(Dispatcher.FromThread(Thread.CurrentThread), _dispatcher)`. With `_dispatcher` null and the executing thread owning no dispatcher, the former two-operand form evaluated `ReferenceEquals(null, null)` to `true`; the guard closes that path. The operand order (type test, null test, reference comparison) matches the captured-context exit at lines 182 to 189, so the two exits now share one shape.
- The inserted comment ("The null test mirrors the captured-context exit: null must never match null.") states the reason, not the mechanics, and sits beside the existing fully-qualified-name comment. Acceptable.
- Nullable flow: `_dispatcher` is `Dispatcher?`; the `is not null` pattern is the idiomatic guard under `#nullable enable`. The TreatWarningsAsErrors Rebuild reported 0 CS86xx.
- Behavioral surface: the exit can only become more restrictive (an additional conjunct). All twelve pre-existing `IsCompleted` tests still pass, including the two true-returning dispatcher-exit tests, so no legitimate inline-continuation case was lost.

### UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs

- `Cache` had exactly one reader (a not-null test assertion) and `modules` had none; deletion rather than privatization follows the repository's dead-code guidance and the issue's own Proposed Fix. The remaining public static fields are `readonly` and published once from the static constructor (unchanged since #824).
- `using System.Reflection;` is still required (`FieldInfo`); `using System.Collections.Generic;` is now unused (informational finding above).

### QuickFiler/Viewers/*.Search.cs

- Both comments now read "Held on a second partial-class part so `<c>...</c>` stays clear of the repository's 500-line ceiling" with the numeral removed and the explanation intact. CSharpier did not reflow the comment (the write-mode format pass left the patch unchanged). The bridge coordinator's third partial part, which carries a historical sentence with a past line count, was not edited; the anchored numstat over `QuickFiler/Viewers` lists exactly the two Search parts.

### UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs (new test, lines 99 to 138)

- Arrangement satisfies the five conditions AC1 enumerates: `SetDispatcher(null)` (no captured dispatcher); `SetUiThreadId(Thread.CurrentThread.ManagedThreadId)` on the executing MTA thread; awaiter context `new DispatcherSynchronizationContext(foreignHost.Dispatcher)` (a dispatcher context that is not the captured UI context); ambient context `new SynchronizationContext()` (non-null, differs from the awaiter context); executing thread created as MTA by `ApartmentThreadRunner`, which owns no WPF dispatcher.
- The dispatcher-taking `DispatcherSynchronizationContext` constructor is used deliberately so no dispatcher is created on the test worker thread (the XML doc explains this; the parameterless constructor would create one and never shut it down).
- Assertions: `thrown.Should().BeNull()` guards against a swallowed exception being read as `false`; `observed.Should().BeFalse()` is the behavioral assertion. `observed` is initialized to `true` so an unexecuted delegate cannot pass. Good defensive shape, consistent with the two sibling tests.
- Fail-before evidence: the test failed with "Expected observed to be False, but found True" against unmodified source, which is precisely the defect.

### UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs (new tests, lines 264 to 313)

- `PublicStaticFields_AreAllInitOnly`: `NotBeEmpty` first (so an empty reflection result cannot pass vacuously), then `OnlyContain(field => field.IsInitOnly, ...)`. Fail-before output named both offending fields by type and name.
- `PublicStaticFields_AreExactlyTheTwoOpCodeTables`: `BeEquivalentTo` on the name array is order-insensitive, which is correct for reflection output. Uses `nameof(...)` so a rename fails at compile time rather than at run time.
- "Arrange & Act" combined into one comment is acceptable for a single reflection call.

### Concurrency and determinism

- `ADDED_DoNotParallelize=0`, `ADDED_Thread.Sleep=0`, `ADDED_Task.Delay=0`, `ADDED_Workers=0`, `ADDED_Retry=0`, `ADDED_GetTempFileName=0`, `ADDED_GetTempPath=0` over 93 added diff lines; runsettings hash unchanged (Workers 0, ClassLevel). The new #889 test relies on the pre-existing class-level `[DoNotParallelize]`, which is appropriate because it mutates process-global `UiThread` statics.

### Evidence integrity notes

- The executor's disclosure that several Phase 0 `Timestamp:` values were estimates later replaced by file write times affects no measured figure and no acceptance criterion; the sequence is monotone and consistent with the later phases. Recorded in the policy audit as I-1.
- Both committed JaCoCo projections re-sum exactly to the runner's first-party summary lines; the per-file Cobertura parse figures reconcile with the package-level delta except for six lines elsewhere in UtilitiesCS that flipped missed-to-covered between runs (run-to-run variance, 0.009 points).
