---
name: project-797-folder-settings-persistence-plan-seams
description: "#797 Folder Settings persistence planning seams — coverage runner hard-codes its TestCaseFilter; #752 worktree fix HAS landed; Directory.Build.props now exists (spec says it does not); serializer test harness is private-nested; R3: a red baseline admitted in one gate makes every sibling exit-0 demand unsatisfiable, coverage collection propagates the inner test exit code, coverage.config excludes third-party only, a pure move inflates its own changed-line denominator"
metadata:
  type: project
---

Seams re-derived while authoring the issue #797 plan in worktree `.claude/worktrees/agent-a46162b1321fb4a50`
at `c431dc32`.

**Why:** each of these either makes a plausible acceptance condition unsatisfiable, or contradicts a
premise carried in the spec or in an earlier memory.

**How to apply:** re-check each before planning further work in `UtilitiesCS/OutlookObjects/Store`,
`UtilitiesCS/ReusableTypeClasses/NewSmartSerializable`, or any C# coverage gate.

1. **`scripts/vscode/Invoke-MSTestWithCoverage.ps1` hard-codes `/TestCaseFilter:TestCategory!=LiveOutlook`
   at `:76` inside `Get-DotnetCoverageArgumentList` and exposes no filter parameter.** Neither the script
   entry point (`param(` at `:1`, four parameters) nor `Invoke-DotnetCoverageCollection` (`:172`) accepts
   an override. A plan that must exclude the four shell-icon hang classes from a coverage run therefore
   cannot use the script or that seam; it must build the `dotnet-coverage` argument list itself. Extends
   [[project-local-shell-icon-tests-hang-shgetfileinfo]] and the #656 note in
   [[project-656-closecompleted-guard-plan-seams]].

2. **The #752 relative-path anchor fix HAS landed — the runner no longer self-blocks in an agent
   worktree.** `:296-303` now filters on
   `([System.IO.Path]::GetRelativePath($resolvedSearchRoot, $_.FullName)) -notmatch '(^|\\)\.claude\\'`,
   and `$resolvedSearchRoot` is the worktree root, so no built assembly fails the predicate. The blanket
   "dot-source it, never run it" rule in [[project-731-r6-coverage-runner-bypass-seams]] is superseded on
   that specific point. The script still writes the processed XML at `:342` before asserting the floor at
   `:344`, so a sub-floor run still yields an artifact.

3. **`Directory.Build.props` and `Directory.Build.targets` now exist at the repository root.** #797's
   spec.md asserts in its Write Set section that neither exists anywhere in the repository. The props file
   sets one property, `RxUseUnsupportedPackagesConfig`, under issue #730. A caller- or spec-supplied
   premise of the form "no repository-root build property file exists" is itself a citation and must be
   re-derived; record the correction in the plan rather than reproducing the stale claim.

4. **The pinned tool manifest is `dotnet-tools.json` at the repository ROOT, not `.config/dotnet-tools.json`.**
   `Glob` for `.config/dotnet-tools.json` returns nothing. It pins csharpier to 1.2.6 with
   `rollForward: false`.

5. **`SmartSerializableHarness` and `TestSmartItem` are `private sealed` nested classes inside
   `SmartSerializable_Tests`** (`UtilitiesCS.Test/ReusableTypeClasses/SmartSerializable_Tests.cs:771` and
   `:809`). A NEW serializer test file in the same project cannot reuse either; it must declare its own
   harness exposing `SetCreateStreamWriter`/`SetTimerFactory` and its own minimal item type. The file is
   896 lines, so appending to it is not an option either.

6. **`StoreWrapperController_Tests` and `StoreWrapperControllerTests` are two different classes.** The
   underscore form (`StoreWrapperController_Tests.cs:15`) is `public partial` with `private static`
   `CreateController` (`:154`) and `CreateControllerWithViewer` (`:160`) helpers, so a new `.Display.cs`
   partial of that same class reaches them. The no-underscore form (`StoreWrapperControllerTests.cs:19`)
   is a separate non-partial class holding the junk-folder doubles `OlObjectsStubBase` (`:139`),
   `RecordingOlObjects` (`:196`) and `NoApplyOlObjects` (`:214`).

7. **`StoreWrapperController.cs:302` carries a commented-out single-ampersand occurrence** inside the dead
   block relocated with `PopulateWithCurrent`, alongside the live one at `:464`. An occurrence-count gate
   over the file for `&` cannot discriminate the fix. Gate on the four `GetRelativeFsPath` tests instead.
   Same class as [[project-442-quickfiler-metrics-plan-seams]].

8. **`UtilitiesCS` grants `InternalsVisibleTo` to `UtilitiesCS.Test`, `ToDoModel.Test` and
   `DynamicProxyGenAssembly2` only — never to `TaskMaster`** (`UtilitiesCS/Properties/AssemblyInfo.cs:18-20`).
   Two consequences for #797: a new interface implemented by a TaskMaster type must be `public`, and a
   spec that describes a helper as `private static` while its own criterion-to-evidence map demands direct
   pure-function tests over that helper forces `internal static`. Record that as a mechanical accessibility
   reading, not a reopened design decision.

9. **`PopulateWithCurrent_ShowsCurrentJunkSelectionsInViewer` (`StoreWrapperControllerTests.cs:33-48`)
   constructs the controller as `new StoreWrapperController(null!)` with a real `StoreWrapperViewer`.**
   Every new dereference added to the populate path — including an AC6 retry through
   `Globals?.Ol` — must be null-conditional or that test throws. Same shape as the #791 loose-mock trap.

10. **The enforced repo line-coverage floor is 80 percent, and it has an executable source.**
    `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1:52` throws below 80, matching CLAUDE.md.
    `.claude/rules/general-unit-test.md` states 85/75. Gate on 80, cite CLAUDE.md as rank 1 in the policy
    compliance order, and record 85/75 as an observation. Related:
    [[project-coverage-threshold-conflict-claude-md-vs-general-unit-test]] and
    [[superseding-a-coverage-floor-must-name-claude-md]].

11. **A bare-toolchain task is the recurring offender for the hook's explicit-path check.** Four of 72
    task lines in the first draft — the two csharpier tasks, the bootstrap task and the format task — had
    no path on the opening line, because their command is `dotnet tool run csharpier check .`. Fix by
    naming the evidence artifact path or the before/after capture path on the task's own opening line.
    Related: [[validate-planner-output-hook-line-anchored-gotchas]].

12. **`/flp:Verbosity=detailed;LogFile=...` must be double-quoted in PowerShell**, or the semicolon is read
    as a statement separator and the log file is never written — while msbuild still exits 0, so the
    missing log is invisible until the acceptance condition tries to read it.

13. **`.gitignore` ignores `artifacts/` (`:57`) and `coverage/*` (`:144`), and `[Tt]est[Rr]esult*/` (`:39`)
    ignores a directory, not a `.trx` by name.** Writing a `.trx` under a `coverage/` subdirectory keeps it
    out of git without relying on the TestResults pattern, which avoids the `runUser`/`computerName` leak
    entirely rather than sanitizing it. Related: [[trx-carries-host-tokens-in-two-casings]].

## Revision round 1 additions

14. **This worktree is unbootstrapped and nothing in the plan bootstrapped it.** `global.json` pins SDK
    `8.0.205` and lists `.dotnet-sdk` FIRST in `paths`, and that directory does not exist (it is not in
    `.gitignore`, so a `Glob` miss is conclusive). `packages/` does not exist. `scripts/vscode/Install-RepoDotNetSdk.ps1`
    is the repo's own installer and `global.json:10` names it in its `errorMessage`. Plan a four-step
    Phase 0 bootstrap: SDK install, `dotnet tool restore --tool-manifest dotnet-tools.json`, a
    `dotnet-coverage --version` probe, then `msbuild /t:Restore /p:RestorePackagesConfig=true`.

15. **`dotnet-coverage` is a GLOBAL tool, not manifest-pinned.** `Invoke-MSTestWithCoverage.ps1:292`
    throws when `Get-Command dotnet-coverage` misses, and `dotnet-tools.json` pins csharpier alone. A
    plan that runs any coverage step must probe for it and carry an install fallback.

16. **Neither `msbuild` nor `vstest.console.exe` is on PATH.** Both come from vswhere:
    `Invoke-VSBuild.ps1:137-144` uses `-find 'MSBuild\**\Bin\MSBuild.exe'` and
    `Invoke-MSTestWithCoverage.ps1:284-287` uses `-find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`.
    A plan that resolves only vstest leaves every `msbuild` task with no resolution mechanism.

17. **The planner hook's explicit-path regex requires a directory separator.**
    `validate-planner-output.ps1:95` is `(?:[.\w*?-]+[\\/])+[.\w*?-]+` and `:290` feeds it the task's
    OPENING line only. Bare filenames — `global.json`, `dotnet-tools.json`, `packages.config` — do not
    match. When a reviewer supplies verbatim replacement text whose opening line carries only bare
    filenames, merge the following `Commands, ...` sentence onto that same line so a slash-bearing path
    lands there. Extends [[validate-planner-output-hook-line-anchored-gotchas]].

18. **An absolute coverage floor cannot be asserted over a denominator the plan itself narrows.** Scoping
    coverage to two test assemblies makes the repo's 80 percent full-suite floor a different measurement.
    Gate on no-regression against a same-scope baseline plus the 90 percent changed-line figure, and
    record the 80 percent floor as an observation with a pre-existing-condition branch. Related:
    [[repo-wide-cobertura-line-rate-is-nondeterministic]].

19. **`BASE-SHA: ` is ten characters**, not eleven. A prefix-length assertion in a plan is itself a
    citation and must be counted, not estimated.

## Revision round 2 additions

20. **A log4net logger initialised from `MethodBase.GetCurrentMethod().DeclaringType` inside a GENERIC
    type has no reliable closed-type attachment point.** `SmartSerializable.cs:26-28` is that shape, and
    the repo's own appender helper attaches by name (`TaskMaster.Test/AppGlobals/AppEventsTests.Helpers.cs:234`
    uses `targetType.FullName`). `GetCurrentMethod` reports the generic type DEFINITION, whose logger name
    carries no type argument, so an appender attached to a closed constructed type's full name is a
    different, non-ancestral logger and captures nothing — the test then fails permanently. Attach to
    `hierarchy.Root` instead and select events by level plus a message fragment unique to the test file,
    and require the production task to emit that fragment. Correct under either reflection resolution.

21. **`scripts/vscode/TaskMaster.cli.runsettings` sets `<Scope>ClassLevel</Scope>` with `<Workers>0</Workers>`,
    so NO exact-log-event-count assertion is safe on a shared static logger.** `[DoNotParallelize]` on one
    class does not exclude writers in sibling classes. `StoreWrapperController.cs:74` is a static logger on
    a non-generic type shared by `StoreWrapperController_Tests` (4 partial files) and
    `StoreWrapperControllerTests`. Assert existence plus a unique message fragment plus a paired
    behavioural assertion (no invocation / no timer armed), never a count.

22. **An inline-backticked path span on a task line is a WRITE CLAIM under a backtick-harvest convention,
    and the planner hook does not need the backticks.** `validate-planner-output.ps1:95` is
    `(?i)(?:[.\w*?-]+[\\/])+[.\w*?-]+` over raw text, so an UNBACKTICKED `scripts/vscode/Foo.ps1` satisfies
    the explicit-path check. Never backtick a bootstrap command containing `TaskMaster.sln`: it claims the
    solution file and serializes the item against every concurrent sibling. Supersedes note 17's phrasing.

23. **A conditional `ExpectedExitCode` keyed on a red baseline declares FAILURE for an improvement.**
    "Expect 1 when the baseline was red" fails the gate when the run comes back green. Write it as
    "equal to the exit code this run actually produced, and that exit code is either 0, or 1 with every
    failing test a member of the recorded baseline set". Pair it with a loop-restart rule that exempts a
    step whose observed code matches its declared non-zero expectation, or the QA loop cannot terminate.

24. **A slash-bearing inline span survives a naive backtick sweep when an adjacent span absorbs the regex
    match.** `` `[^`]*[/\\][^`]*` `` matches the text BETWEEN two spans, so a real offender such as
    `` `## Proposed Fix / Validation Ideas` `` (issue.md:92) goes unreported while a plain-prose path is
    falsely reported. Inspect every reported line by eye; count lines, not matches.

25. **`TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs:19` has NO class-level `[ExcludeFromCodeCoverage]`**
    (only a member-level one at `:152`), so an explicit interface implementation added there IS measurable
    and contributes a near-zero changed-line row when only doubles drive it. State that the aggregate
    changed-line figure is the gate and a low per-file row is expected.

## Revision round 3 additions

26. **A plan that admits a red baseline in one gate and demands exit 0 in another makes the QA loop
    non-terminating.** Round 3 was almost entirely this one root cause, in five places. The general
    form: a Phase 0 artifact records a pre-existing failure or pre-existing format drift, a later step
    reverts or tolerates it, and a sibling step still demands `EXIT_CODE: 0`. Sweep EVERY phase's
    acceptance for unconditional exit-0 the moment any baseline-red branch is introduced anywhere.
    Extends note 23.

27. **A revert-the-drift step plus a whole-tree formatter check cannot both pass.** If P5-T1 reverts
    the paths a Phase 0 csharpier baseline enumerated as pre-existing drift, those paths are
    unformatted by construction, so the read-only `csharpier check .` that follows fails, the loop
    restarts, the formatter reformats them, the revert runs again. The check step needs an explicit
    subset-of-the-recorded-drift branch carrying `ExpectedExitCode:`.

28. **`Invoke-MSTestWithCoverage.ps1:232-237` propagates the inner vstest exit code and throws.**
    `$global:LASTEXITCODE = 0` then `Invoke-DotnetCoverageExe` then `if ($coverageExitCode -ne 0) { throw }`.
    So a coverage COLLECTION step is not exit-code-independent of the test run: with a red baseline,
    `Acceptance: EXIT_CODE: 0` on any coverage step is unsatisfiable.

29. **`coverage.config` excludes third-party modules ONLY; the test-assembly exclusion is derived in
    memory.** The file (repo root, 24 lines) lists Deedle, FSharp, Castle.Core, FluentAssertions, Moq,
    Microsoft.Testing, MSTest. `ConvertTo-DerivedCoverageSettingsXml` (`:99-113`) appends
    `.*\.Test\.dll$` to the Exclude node and writes a derived document beside the output (`:198-223`),
    removed in a finally. A plan-authored helper that passes the canonical file UNMODIFIED instruments
    both test assemblies and puts test code in the denominator, which the general unit test policy
    forbids. Require the helper to perform the same derivation.

30. **A pure-move relocation inflates its own changed-line denominator.** An anchored
    `git diff --unified=0` reports every relocated line as an addition. `PopulateWithCurrent` (`:279-314`),
    `BindExcludeStoreCheckbox` (`:316-346`) and `GetRelativeFsPath` (`:456-474`) are ~86 lines of
    pre-existing, partially covered code. A hard 90 percent changed-line gate then measures code the
    change did not modify. Enumerate the relocated lines and exclude them from the denominator, keeping
    lines that later phases actually edit.

31. **The repo's log4net appender helper mutates process-wide state and restores none of it.**
    `TaskMaster.Test/AppGlobals/AppEventsTests.Helpers.cs:230-238` sets `logger.Level` and
    `logger.Repository.Configured = true`; `DetachMemoryAppender` (`:241-246`) removes the appender
    only. Under `<Scope>ClassLevel</Scope>` (note 21) those survive the test and reach sibling classes.
    A plan that copies this shape must require the finally block to restore the previous level and the
    previous configured flag. Worse on the ROOT logger, which note 20 forces for a generic type.

32. **Classify fail-before RED/GREEN per test CASE, not per criterion, when the seam is a pass-through
    placeholder.** A declaration-only `TrimStorePrefix` that returns its argument unchanged makes four
    of six AC7 cases (no leading backslash, single backslash, empty, null) GREEN from the moment they
    are written, because each expects its argument unchanged. Only the two cases whose input carries
    the leading pair are red. Same trap on an AC6 fallback table: the "primary lookup succeeds" case is
    green pre-fix. A criterion-level "the six AC7 cases are red" claim is false.

33. **A green vstest run prints no `Skipped` line and the TRX not-executed counter is zero.** Derive a
    skipped count as total minus executed from the results-file counters. A console-derived figure is
    unreadable on exactly the run a green baseline expects.

34. **A terminal porcelain gate that admits only two residual classes needs a Phase 0 tracked-ness
    probe.** If the preparation documents (issue, spec, research, plan, promoted entry) are not already
    tracked in HEAD, the final `git status --porcelain --untracked-files=all` gate fails at the very end
    with nothing left to do about it. Add a `git ls-files --error-unmatch` check over the five paths in
    Phase 0. Related: [[existence-is-not-retention-gate-committed-artifacts]].

Related: [[project-731-r6-coverage-runner-bypass-seams]], [[project-791-hc-deadline-cancel-teardown-plan-seams]],
[[project-752-relative-path-anchor-plan-seams]], [[declaration-only-seam-task-for-fail-before]].
