---
name: project-791-hc-deadline-cancel-teardown-plan-seams
description: "#791 QuickFiler High Confidence deadline + Cancel teardown planning seams — QfcDatamodel is [ExcludeFromCodeCoverage] so two Write Set files are unmeasurable; the retargeting surface is 7 gate tests not the 4 the spec names; IFilerFormController forbids an optional ActionCancelAsync parameter; QfcHomeController.cs has only 31 lines of headroom"
metadata:
  type: project
---

Seams re-derived while authoring the issue #791 plan in worktree `TaskMaster-wt/2026-09-06T09-59` at `7c8ac9ae`.

**Why:** each of these makes a plausible-looking acceptance condition unsatisfiable, vacuous, or uncompilable, and none is
visible from the spec or the research artifact.

**How to apply:** re-check each before planning further work in `QuickFiler/Controllers`.

1. **`QuickFiler/Controllers/QfcDatamodel.cs:25` carries `[ExcludeFromCodeCoverage]` on the partial class.** It applies to the
   whole type, so `QfcDatamodel.QueueProcessing.cs` (`public partial class QfcDatamodel` at `:12`) is excluded too. Any
   changed-line coverage AC over those two files is structurally unmeasurable. `QfcScanProgressBandMapper.cs:12` states the
   same fact in prose, which is a cheap corroboration. Plan a decidable class-element-count determination against the
   baseline Cobertura, and anchor the trailing-filename match on a separator — an unanchored `QfcDatamodel.cs` suffix also
   selects `IQfcDatamodel.cs`. Same trap as [[project-781-excludefromcodecoverage-guard-plan-seams]] item 1.

2. **Making the first-batch deadline advisory breaks SEVEN gate tests, not the four `spec.md` names.** The spec's Test
   Strategy retargeting list omits `QfcStreamingDequeueConfidenceGateTests.Part2.cs:76-121`
   (`..._LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline`), `:124-144`
   (`..._DeadlineExpiresWithZeroAccepted_ReturnsEmptyListAtTheBound`), `:205-228`
   (`..._AfterDeadlineReturn_StopsTakingAndLeavesUnscannedCandidates`), `:346-385`
   (`..._DeadlineExpiry_EmitsOneExpiryLineAndKeepsPerCandidateLogging`) and `Part3.cs:92-127`
   (`..._ProgressCallback_StopsReportingOnceTheMethodReturns`). Read every test that passes `firstBatchDeadline:` before
   trusting a spec-supplied retarget list.

3. **`Part2.cs:384` is a TOTAL-count assertion on the injected `debugLog` list** (`logs.Should().HaveCount(4, ...)`).
   Adding any new line through `_debugLog` breaks it. The sibling at `Part1.cs:172-179` uses a *filtered* `ContainSingle`
   and is unaffected. Before adding a log line behind an injected sink, grep the test tree for total-count assertions on
   that sink, not just for the literal being changed.

4. **`QuickFiler/Interfaces/IFilerFormController.cs:11` declares `Task ActionCancelAsync();`.** A method with an all-optional
   extra parameter does NOT implement a zero-parameter interface member in C#, so a `trigger` parameter is a compile break,
   and the interface file is outside the #791 Write Set so AC5 forbids editing it. Supply the discriminator as call-site
   logging instead. Always grep the whole repo (not just the obvious `IQfc*` file) before adding an optional parameter to a
   controller method.

5. **`QuickFiler/Controllers/QfcHomeController.cs` is 469 lines — 31 to the ceiling.** Rewriting `Cleanup()` (`:370-379`) with
   three separate `try`/`catch` blocks plus a `finally` measures out at roughly 505 lines. Two guarded blocks fit. Budget the
   guarded-block count against the ceiling *before* specifying the shape, and state the grouping rationale in the plan so a
   reviewer does not read it as a weakened requirement.

6. **`QfcFormControllerTests.cs:392-403` (`ButtonCancel_Click_ShouldCancelAction`) is a vacuous test that becomes a real
   constraint.** It awaits `ActionCancelAsync()` against loose mocks where `IQfcHomeController.KeyboardHandler` and
   `.DataModel` are both null, so every new dereference on the Cancel path must be null-conditional and the awaited
   `QuiesceLoaderAsync` must be captured into a local and awaited only when non-null. `QfcFormControllerSeamTests.cs:162-179`
   adds a second constraint: the parent-token cancel must stay ahead of the first `await`, because the test asserts
   cancellation by the time `Mock.Raise` returns.

7. **Once every teardown stage is individually caught, there is no throw source left for a "does not rethrow" test.** Drive
   `ButtonCancel_Click_ActionThrows_DoesNotRethrow` from the handler's own body instead — nulling `_formViewer` makes
   `SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext)` (`EventHandlers.cs:74`) raise inside the
   handler's `try`, which is false-before and true-after against exactly the `throw;` at `:80`.

8. **`ParkFocusOffWebView2` extraction invalidates its own remark.** `QfcFormController.Deactivate.cs:24` states a null-viewer
   branch "would be unreachable code" because the routine is reachable only via `FormDeactivated`. Calling it from the Cancel
   path falsifies that sentence and requires the guard. Single-line gate token that exists today:
   `a null-viewer branch would be unreachable code`.

9. **A Phase-1 declaration seam that stores a constructor parameter in a `private readonly` field raises CS0414** ("assigned
   but never used") until Phase 2 reads it, and `/p:TreatWarningsAsErrors=true` promotes it to an error. Use an `internal`
   get-only auto-property instead: its compiler-generated backing field is read by the getter, so the seam is warning-clean
   at every point in the plan.

10. **This worktree is only half bootstrapped.** `.dotnet-sdk/sdk/8.0.205` exists, but `packages/` is absent and there is no
    `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`. No `nuget.exe` exists anywhere in the repo and none of the repo scripts
    invokes one, so plan the packages.config restore as
    `msbuild TaskMaster.sln /t:Restore /m /p:RestorePackagesConfig=true` rather than `nuget restore`. Extends
    [[agent-worktrees-need-sdk-and-nuget-bootstrap]] and [[project-731-lifecycle-disposal-plan-seams]] round 4.

11. **`QuiesceLoaderAsync`'s "and logs" assertion has no existing convention to borrow.** There is no `MemoryAppender` or
    `log4net.Config` usage anywhere in the C# test tree, and attaching one mutates a process-global logger repository.
    The gate's injected `Action<string> debugLog` is the established alternative, so mirror it with an `internal
    Action<string>` seam on `QfcDatamodel` rather than asserting over log4net.

12. **`artifacts/` is git-ignored (`.gitignore:57`) and `artifacts/csharp/` is explicitly permitted by
    `enforce-evidence-locations.ps1:22-26`.** So an AC demanding `artifacts/csharp/coverage.xml` be *produced* is satisfiable
    on-disk but can never be satisfied by a `git ls-files` retention clause. Gate on existence plus the recorded root
    counters, and say so. Related: [[existence-is-not-retention-gate-committed-artifacts]].

13. **AC5-style "the branch diff touches no file outside the Write Set" is unsatisfiable read over the whole tree**, because
    the plan must write evidence artifacts and check off AC boxes in `spec.md`. Scope every such gate to a source pathspec
    (`'*.cs' '*.csproj'`) and record the reading and its rationale in the plan, so the narrower evaluation is not read as an
    unstated relaxation.

Observed command outputs reused from issue #782 (do not re-derive):
`dotnet tool run csharpier check .` prints `Checked <N> files in <M>ms.` and exits 0 on a clean tree;
`dotnet tool run csharpier format .` prints `Formatted <N> files in <M>ms.` whether or not it rewrote anything, so it needs a
before/after tree observation; the coverage aggregation snippet prints
`LINES_COVERED=<n> LINES_VALID=<n> BRANCHES_COVERED=<n> BRANCHES_VALID=<n>`.

Related: [[project-731-lifecycle-disposal-plan-seams]], [[project-781-excludefromcodecoverage-guard-plan-seams]],
[[reference-vstest-scoped-run-command]], [[repo-wide-cobertura-line-rate-is-nondeterministic]].
