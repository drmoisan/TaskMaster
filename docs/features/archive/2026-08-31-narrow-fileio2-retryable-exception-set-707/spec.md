# narrow-fileio2-retryable-exception-set (Spec)

- **Issue:** #707
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T09-30
- **Status:** Draft
- **Version:** 0.2

## Write Set
`UtilitiesCS/To Depricate/FileIO2.cs` (contains a space)
`UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs`

## Context
`FileIO2.WriteTextFileAsync` retries on every `IOException`. `DirectoryNotFoundException` derives from `IOException`, so an absent target folder consumes the full 100-attempt, 100-millisecond retry window even though no attempt in that window can succeed.

Environment:
- OS/version: Windows 11, .NET Framework 4.8.1
- Python version: not applicable
- Command/flags used: not applicable; reached through any caller of `UtilitiesCS.FileIO2.WriteTextFileAsync`
- Data source or fixture: `UtilitiesCS/To Depricate/FileIO2.cs`

Impact / Severity:
- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Severity is Low because the one production caller that could reach the case guards against it: QuickFiler/Controllers/QfcHomeController.Metrics.cs calls `Globals.FS.SpecialFolders.TryGetValue("MyDocuments", ...)` before writing. The stall is therefore latent rather than observed.


## Repro & Evidence
Steps to Reproduce:
1. Call `FileIO2.WriteTextFileAsync` with a `folderpath` that does not exist on disk.
2. Observe that the writer factory throws `DirectoryNotFoundException` on every attempt.
3. Observe that the method spends roughly ten seconds in the retry loop before returning `false`.

Expected:
A failure that cannot be resolved by waiting should not consume the retry budget. The method should distinguish transient contention failures, for which retrying is the correct response, from structural failures such as a missing directory, and should return promptly on the latter.

Actual:
The catch clause is `catch (IOException ex)`. `DirectoryNotFoundException` is an `IOException`, so the loop performs all 100 attempts and awaits 99 delays before reporting failure.

Logs / Screenshots:
- [x] Attached minimal logs or snippet
- Snippet: the retry-exhaustion log line reads `after {attempts} attempts.` with `attempts` equal to 100, once per call against a missing directory.


## Scope & Non-Goals
- In scope:
  - Inserting a new `catch (DirectoryNotFoundException ex)` block immediately before the existing `catch (IOException ex)` block in the internal seam overload of `WriteTextFileAsync` in `UtilitiesCS/To Depricate/FileIO2.cs`, so a missing target directory is treated as a terminal (non-retryable) failure: log and `return false` on the first occurrence, without incrementing `attempts` and without calling `delayAsync`.
  - Adding one new regression test to `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` that drives the existing `writerFactory`/`delay` injectable seam with a factory that always throws `DirectoryNotFoundException`, asserting a writer-factory invocation count of exactly 1 and a delay-delegate invocation count of exactly 0.
- Out of scope / non-goals:
  - `PathTooLongException` handling. It also derives from `IOException` and is structurally non-retryable (research §2, §3 Approach B), but neither the issue text nor this spec's Expected Behavior names it, and it is not reachable from either in-repo production caller (both build `filepath` from a resolved special-folder path plus a short, fixed filename). It is a candidate for a separate future potential-doc item, not part of this fix.
  - Any change to caller-side code. Both production callers (TaskMaster/AppGlobals/AppOlObjects.cs line 315 and QuickFiler/Controllers/QfcHomeController.Metrics.cs) already consume `Task<bool>` and already handle a `false` result; the new catch path returns through the same `false` result they already handle.
  - Any change to the `opened`-terminal-failure branch, the retry-exhaustion branch, or the general `catch (IOException ex)` body — all established by issue #647 and unaffected by this narrowing.
- Explicitly excluded systems, integrations, or datasets:
  - QuickFiler/Controllers/QfcHomeController.Metrics.cs — cited only as caller context; modifying it is out of scope for this feature and owned by a separate workstream.
  - the Claude runtime tree at .claude (all contents), the Codex mirror tree at .codex (all contents), the dot-agents tree at .agents (all contents), config/blast-radius.json, and config/orchestration-routing.json — governance/config surfaces unrelated to this bugfix.

## Root Cause Analysis
Deferred from issue #647 as an explicit non-goal. Narrowing the caught set is a behavior change beyond that issue's stated Expected Behavior, so it was recorded for separate treatment rather than folded in. The relevant code is the catch clause in the `internal static` seam overload of `WriteTextFileAsync` in `UtilitiesCS/To Depricate/FileIO2.cs`.


## Proposed Fix

### Design summary (what changes where):
Insert one new catch block, `catch (DirectoryNotFoundException ex)`, ahead of the existing `catch (IOException ex)` block in the retry loop of the internal seam overload of `WriteTextFileAsync` (`UtilitiesCS/To Depricate/FileIO2.cs`, currently at line 126). The new block mirrors the existing `opened`-terminal-failure shape at lines 128-135: log the causing exception and `return false` immediately, without incrementing `attempts` and without calling `delayAsync`. This is additive only — no signature change, no new parameters, no change to either `WriteTextFileAsync` overload's declaration.

### Boundaries and invariants to preserve:
- Catch-order constraint: `DirectoryNotFoundException` derives from `IOException`, so C# requires the more-derived catch block to appear textually before the less-derived `catch (IOException ex)` block in the same `try`; reversing the order is a compile-time error (CS0160).
- The existing tests in `FileIO2_Tests.cs` must remain green unchanged — none of them throws `DirectoryNotFoundException`, so none of their assertions are affected by adding a more specific catch clause ahead of the general one.
- The `opened`-flag terminal-failure path (mid-write `IOException` after the writer opened) is unchanged; the new catch block is only reachable in the pre-open state, since `DirectoryNotFoundException` is documented only against the `StreamWriter` constructor, not against `TextWriter.WriteLineAsync`.
- The general `catch (IOException ex)` retry-exhaustion path (100-attempt budget, 100 ms delay via `delayAsync`) is unchanged for all other `IOException` cases, e.g. sharing violations raised as a bare `IOException`.
- Cancellation still takes priority: `token.ThrowIfCancellationRequested()` runs before `createWriter` on each iteration and is unaffected by which catch branch a prior attempt took.

### Dependencies or blocked work:
None. This fix is additive to the shape already established by issue #647 (which is already merged into this branch's `FileIO2.cs`: `Task<bool>` return, bound `ex`, `opened` terminal-failure branch, internal seam overload with `InternalsVisibleTo("UtilitiesCS.Test")` already declared). No other in-flight feature blocks or is blocked by this change.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `UtilitiesCS/To Depricate/FileIO2.cs` — insert the new `catch (DirectoryNotFoundException ex)` block immediately before the existing `catch (IOException ex)` block.
- `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` — add one new `[TestMethod]` regression test following the existing `writerFactory`/`delay` seam pattern.

#### Functions/classes/CLI commands impacted:
- `FileIO2.WriteTextFileAsync` — internal seam overload (`Task<bool> WriteTextFileAsync(string, string[], string, CancellationToken, Func<string, TextWriter>? writerFactory, Func<int, CancellationToken, Task>? delay)`). The public overload is unaffected because it only forwards to the internal seam with production defaults.

#### Data flow and validation changes:
None. No new inputs, outputs, or validation rules are introduced; the change only adds a new terminal exit from the existing retry loop, reached when `opened == false` and the specific exception type is `DirectoryNotFoundException`.

#### Error handling and logging updates:
The new catch block logs via the same `logger.Error(message, ex)` two-argument `log4net.ILog.Error(object, Exception)` overload already used by the sibling `catch (IOException ex)` block, with a message identifying the target directory as missing (e.g. `Failed to write to {filepath}: the target directory does not exist.`) rather than the generic retry-exhaustion message. No new logging categories or log levels are introduced.

#### Rollback/feature-flag considerations (if applicable):
No feature flag is warranted for a narrow, additive catch-block insertion. Rollback is a straightforward revert of the single commit; no data migration or state to unwind.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
No change to `WriteTextFileAsync`'s public or internal signatures, parameter types, or return type (`Task<bool>`). The only externally observable difference is behavioral: a `DirectoryNotFoundException` from the writer factory now returns `false` after exactly one factory invocation instead of up to 100.

#### Required configuration keys and defaults:
None. No configuration is introduced or changed.

#### Backward-compatibility expectations:
Fully backward compatible. Both production callers already handle a `Task<bool>` result and already branch on `false`; the new catch path returns through the same `false` result they already handle, so no caller-side code changes are required or expected.

#### Performance constraints (latency/throughput/memory):
The fix improves latency for the missing-directory case: it eliminates up to 99 unnecessary `delayAsync(100, ...)` awaits (roughly ten seconds) that the current general `IOException` retry path performs before returning `false`. No new performance constraint is introduced; no measurable regression is expected since the change adds a single conditional branch evaluated only on exception dispatch.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - Target environment remains Windows 11 / .NET Framework 4.8.1, matching the documented exception hierarchy for `StreamWriter(String, Boolean, Encoding)` verified against Microsoft Learn (`DirectoryNotFoundException : IOException`).
  - The production writer factory default (`p => new StreamWriter(p, true, System.Text.Encoding.UTF8)`) is unchanged; the fix depends on this specific constructor overload's documented exception set.
  - UtilitiesCS/Properties/AssemblyInfo.cs already declares `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]`, so no new visibility attribute is required for the test to reach the internal seam overload.
- Constraints (budget, performance, compatibility):
  - Minimal, targeted diff per the repository's Bugfix Workflow: one new catch block and one new test, no broader refactor.
  - Catch-order is a hard compiler constraint (CS0160), not a style preference.
  - File size limit (500 lines) applies to both changed files; the insertion is small enough not to approach it.
- External dependencies (services, libraries, releases):
  - None. No new NuGet package, library, or external service is introduced.

## Data / API / Config Impact
- User-facing or API changes:
  - None. No public API signature changes. The only observable difference is that calls against a missing target directory now fail fast instead of stalling for the full retry budget.
- Data or migration considerations:
  - None. No persisted data format, schema, or migration is affected.
- Logging/telemetry updates (if any):
  - One new `logger.Error` call site in the new catch block, using the existing `log4net.ILog.Error(object, Exception)` overload and logger instance already used elsewhere in this method. No new logging infrastructure, category, or telemetry pipeline is introduced.
- Compatibility notes (CLI flags, config schemas, versioning):
  - Not applicable. `WriteTextFileAsync` has no CLI surface, config schema, or versioning concern.

## Test Strategy
Seeded from issue:

- [ ] Unit coverage areas: drive the existing `writerFactory` seam with a factory that throws `DirectoryNotFoundException` and assert a writer-factory invocation count of exactly 1 and a delay-delegate invocation count of exactly 0.
- [ ] Integration scenario to retest: the `QfcHomeController` metrics flush and the `AppOlObjects` timed disk writer, both of which consume the boolean result.
- [ ] Manual verification notes: confirm that `UnauthorizedAccessException` is not an `IOException` and is therefore already outside the retry set, so no separate handling is needed for it.

- Regression tests to add or update:
  - Add `WriteTextFileAsync_WhenDirectoryDoesNotExist_ShouldReturnFalseWithoutRetrying` (or equivalent name) to `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs`, mirroring the naming and structure of `WriteTextFileAsync_WhenWriteFailsAfterOpen_ShouldReturnFalseWithoutRetrying`.
    - **Arrange:** a `writerFactory` delegate that increments a call counter and always `throw new DirectoryNotFoundException("Simulated missing directory.")`; a `delay` delegate that increments a separate call counter and returns `Task.CompletedTask`.
    - **Act:** `await FileIO2.WriteTextFileAsync(filename, strOutput, folderpath, token, writerFactory: ..., delay: ...)` using the internal seam overload.
    - **Assert:** result is `false`; writer-factory invocation count is exactly `1`; delay-delegate invocation count is exactly `0`.
  - No existing test is modified. This test must fail against the pre-fix source (factory invoked up to 100 times, delay invoked up to 99 times) and pass once the new catch block is added, satisfying the Bugfix Workflow's "create a failing regression test first" step.
- Unit tests (pytest) for the fixed behavior and boundaries:
  - Not applicable — this is a C# fix. See "Regression tests to add or update" above; the repository's MSTest + Moq + FluentAssertions stack applies (CUT1/CUT2).
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
  - `DirectoryNotFoundException` on the first attempt: covered by the new test (factory calls = 1, delay calls = 0).
  - `UnauthorizedAccessException`: confirmed by research to derive from `SystemException`, not `IOException`; it is already outside the retry set and requires no new test since no behavior changes for it.
  - Cancellation before and during retry: already covered by `WriteTextFileAsync_WhenTokenAlreadyCancelled_ShouldThrowBeforeOpening` and `WriteTextFileAsync_WhenCancelledDuringRetryWindow_ShouldThrowPromptly`; unaffected by this change and re-verified as part of the full suite run.
  - Mid-write failure after `opened = true`: already covered by `WriteTextFileAsync_WhenWriteFailsAfterOpen_ShouldReturnFalseWithoutRetrying`; a `DirectoryNotFoundException` is not reachable in this state for the production `StreamWriter` factory, so no new mid-write variant is required.
- Error handling and logging verification:
  - The new catch block's `logger.Error` call is exercised implicitly by the new test (the test does not assert on log output directly, consistent with the existing tests in this file, none of which assert on logger calls).
- Coverage impact and targets for changed lines/modules:
  - `UtilitiesCS.csproj` compiles `FileIO2.cs`; `coverage.config` does not exclude the `To Depricate` folder, so the new catch block's lines are in the coverage denominator. The new test must exercise every line of the new catch block to avoid a changed-lines coverage regression, consistent with the >= 90% target for new code under the C# Unit Test Policy.
- Toolchain commands to run (format → lint → type-check → test):
  1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <UtilitiesCS.Test assembly path> /EnableCodeCoverage`
  - Run in this exact order; restart from step 1 if any step fails or auto-fixes files.
- Manual validation steps (if required):
  - None required beyond the automated toolchain and regression test; this fix has no UI or manual-only surface.


## Acceptance Criteria
- [x] `WriteTextFileAsync` (internal seam overload, `UtilitiesCS/To Depricate/FileIO2.cs`) catches `DirectoryNotFoundException` ahead of the existing `catch (IOException ex)` block, and a `DirectoryNotFoundException` thrown by the writer factory now returns `false` after exactly 1 writer-factory invocation and 0 delay-delegate invocations (was: up to 100 factory invocations and up to 99 delay invocations before this fix).
- [x] The new catch block logs the failure via `logger.Error` before returning `false`, without incrementing `attempts` and without calling `delayAsync`.
- [x] A new regression test in `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` (e.g. `WriteTextFileAsync_WhenDirectoryDoesNotExist_ShouldReturnFalseWithoutRetrying`) asserts result `false`, writer-factory call count `1`, and delay-delegate call count `0` for a `DirectoryNotFoundException`-throwing factory, and fails against the pre-fix source.
- [x] All pre-existing tests in `FileIO2_Tests.cs` still pass unmodified.
- [x] `UnauthorizedAccessException` behavior is unchanged (already outside the retry set, no new handling needed) and no test regresses this.
- [x] The general `catch (IOException ex)` retry-exhaustion path (100-attempt budget, 100 ms delay) is unchanged for non-`DirectoryNotFoundException` `IOException` cases.
- [x] `PathTooLongException` is explicitly not handled by this fix (out of scope; see Scope & Non-Goals) and no test asserts behavior for it.
- [x] Neither production caller (TaskMaster/AppGlobals/AppOlObjects.cs line 315, QuickFiler/Controllers/QfcHomeController.Metrics.cs) requires a code change; both already consume `Task<bool>` and already handle a `false` result.
- [x] Full C# toolchain passes clean in a single pass: `dotnet tool run csharpier check .`, `msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true`, and `vstest.console.exe` against `UtilitiesCS.Test` with all tests green.

## Risks & Mitigations
- Technical or operational risks:
  - Catch-clause ordering error (placing the new block after the general `catch (IOException ex)`) would fail the build immediately with CS0160, so this risk is self-detecting at compile time via the analyzer/type-check toolchain steps, not something that could reach production.
  - A caller that currently depends on the missing-directory case retrying (e.g., expecting the directory to be created by a concurrent process during the retry window) would observe an earlier `false` return. Research found no such caller: both production callers pre-resolve `myDocuments` via `TryGetValue("MyDocuments", ...)` before ever reaching `WriteTextFileAsync`, and neither retries or otherwise depends on the prior stall behavior.
  - Coverage regression on the new catch block's lines if the new test is omitted or incomplete; mitigated by the explicit assertion requirements in Test Strategy and Acceptance Criteria.
- Mitigations and rollbacks:
  - The change is a single additive catch block plus one test; a straightforward `git revert` of the commit fully restores prior behavior with no data or state to unwind.
  - No feature flag is needed given the narrow, low-severity, easily reversible nature of the change.

## Rollout & Follow-up
- Release/rollout steps:
  - Standard PR merge through the repository's normal review and CI process; no phased rollout, feature flag, or migration step is needed.
- Post-fix monitoring or clean-up tasks:
  - None required. If `PathTooLongException`'s analogous retry-budget stall is judged worth fixing later, record it as its own potential-doc item (mirroring how this issue itself was recorded from #647's deferred note) rather than reopening this issue.
- Links: issue #707 (https://github.com/drmoisan/TaskMaster/issues/707); sibling issue #647 (folder docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647), which established the `Task<bool>`/`opened`-flag shape this fix extends and originally deferred this narrowing; research artifact at path docs/features/active/2026-08-31-narrow-fileio2-retryable-exception-set-707/research/2026-09-02T09-15-narrow-fileio2-retryable-exception-set-research.md.
- Outcome: All 9 acceptance criteria (AC1-AC9) delivered and verified; see `evidence/qa-gates/p6-t10-acceptance-summary.md` for the per-criterion verifying task and evidence artifact. The fix (one `catch (DirectoryNotFoundException ex)` block) and its regression test were committed at `194773ffae955747d47621b60323132eccc7170a`.
