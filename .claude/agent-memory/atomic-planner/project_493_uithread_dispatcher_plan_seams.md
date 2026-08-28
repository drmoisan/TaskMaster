---
name: project-493-uithread-dispatcher-plan-seams
description: Seams found planning #493 (QuickFiler.Test UiThread dispatcher restore scope) — signature-change fail-before needs a staged Compile Include, the coverage script IS the parallelized run, and the two doomed grep tokens live all over docs/
metadata:
  type: project
---

Three reusable findings from planning issue #493 (`docs/features/active/quickfiler-test-uithread-dispatcher-493`).

**1. A signature-change bug CAN have a real executed fail-before run — stage the `<Compile Include>`.**
The regression tests could not compile at `HEAD` (`EnsureUiThreadDispatcher` returned `void`; the new
fixture type did not exist), so the delegation proposed a prose-only "note that it does not compile".
A real red build is available instead: author the new *tests* file and add ONLY its `<Compile Include>`
line, run `msbuild /t:Rebuild`, and capture the diagnostics. Defer the *fixture* file's `<Compile
Include>` to the next phase — otherwise MSBuild fails with a missing-SOURCE-FILE error (CS2001) rather
than the intended missing-TYPE error, and the artifact proves the wrong thing. Record
`ExpectedExitCode: 1` on the artifact.

**Why:** it converts a prose dossier into an executed gate with a non-zero exit code and named
diagnostics, which is what the Bugfix Workflow actually asks for.

**How to apply:** whenever the fix is a return-type or signature change, split the csproj wiring across
two phases and put the red build between them. Related: [[legacy-csproj-explicit-compile-include]].

**2. `Invoke-MSTestWithCoverage.ps1` already runs class-level-parallelized, so it doubles as the
parallelized supporting run.** It reads `scripts/vscode/TaskMaster.cli.runsettings`
(`<Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope>`). A plan that needs BOTH a CI-parity
sequential gating run and a parallelized supporting run does not need three test tasks: the
`vstest.console.exe` run with no `/Settings:` is the gating one, and the coverage script invocation is
the parallelized one. `-SearchRoot QuickFiler.Test` scopes discovery to one project (the param is
joined to the repo root), which also sidesteps the stale `.claude/worktrees` discovery defect.

**Why:** #493 planning initially carried a third, redundant ~20-minute run.

**How to apply:** state the double duty in the coverage task's text so a reviewer does not read the
missing parallel run as a gap. Related: [[reference-invoke-mstest-with-coverage-script]].

**3. `SwapUiThreadDispatcher` and `UiThreadDispatcherGate` occur in ~16 files under `docs/`** —
including #493's own `spec.md`, `research/`, and plan, plus the #511 and #230 feature folders. A
repo-wide "grep returns no hits" gate on either token is unsatisfiable by construction. Scope every
such gate with `-- QuickFiler.Test/`. Conversely the `"_dispatcher"` reflection literal has exactly
three source sites (`QfcItemController.TestSupport.cs`, `...InitializationTests.Part2.cs`,
`WpfUiDispatcherTests.cs`), and the third is an accepted residual risk that must SURVIVE — so the
"exactly one implementation" gate asserts two remaining hits, not zero.

Related: [[zero-hit-grep-gates-need-carveouts]], [[agent-memory-is-tracked-scope-git-gates]].
