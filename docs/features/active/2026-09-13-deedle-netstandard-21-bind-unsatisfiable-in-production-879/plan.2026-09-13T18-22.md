# 2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production (Plan)

- **Issue:** #879
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-13T18-22
- **Status:** Ready for preflight
- **Version:** 1.0
- **Work Mode:** full-bug (spec.md is the sole acceptance-criteria source; `user-story.md` is correctly absent)
- **Complexity band:** C3
- **Branch:** `bug/deedle-netstandard-21-bind-unsatisfiable-in-production-879`

**Fail-closed evidence rule:** every baseline, QA-gate and coverage task below names the exact artifact it
must produce. If a named artifact is absent, or is present but missing a required field, the task is not
complete and the verdict is BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** every evidence-producing task records `Timestamp:`, `Command:`,
`EXIT_CODE:`, and `Output Summary:` in its artifact. Where a gate is expected to exit non-zero, the
artifact also carries `ExpectedExitCode:` with the integer the gate is expected to produce.

**Evidence path shorthand (binding).** Throughout this plan the prefix `.../evidence/` is an abbreviation
for, and resolves to, exactly
`docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/`.
Every evidence artifact this plan names therefore lives under that feature folder's `evidence/` subtree,
in one of the six canonical kinds `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other`
and `remediation-baseline`. No path under `artifacts/` is a valid evidence location, and no task in this
plan names one. Where a shell command needs the path it is written out in full, because the shorthand is
a reading convenience for this document and never a literal a command may contain.

---

## Defect Statement

`Deedle.dll` references `FSharp.Core 4.5.0.0`. `TaskMaster/app.config` redirects `FSharp.Core` to
`11.0.0.0` (`assemblyIdentity` at line 69, `bindingRedirect` at line 70). `FSharp.Core 11.0.0.0`
references `netstandard, Version=2.1.0.0, PublicKeyToken=cc7b13ffcd2ddd51`. No `*.config` file in the
repository contains the string `netstandard`, `netstandard.dll` is deployed to no build output, and the
only `netstandard` in the machine GAC is `2.0.0.0`. The bind is satisfiable only through a process-global
`AppDomain.CurrentDomain.AssemblyResolve` fallback matching simple name plus public key token, which the
two versions share.

Production carries exactly one such handler, installed from the `SVGControl.SvgRenderer` static
constructor (`SVGControl/SvgRenderer.cs` lines 25-28 calling `SvgAssemblyResolver.Install()`, which
subscribes at `SVGControl/SvgAssemblyResolver.cs` line 41). It is therefore present only after something
has rendered SVG. Nothing declares or enforces that ordering. The maintainer reproduced the failure on
`main` from the ribbon entry point `QuickFilerHighConfidence_Click`
(`TaskMaster/Ribbon/RibbonViewer.cs` line 159).

## Non-Goals (binding; reproduced from spec.md `## Scope & Non-Goals`)

- **Do not modify** `SVGControl/SvgAssemblyResolver.cs`, `SVGControl/SvgRenderer.cs` or
  `SVGControl/SvgAssemblyProbe.cs`. That handler has an independent reason to exist for the `devenv.exe`
  WinForms designer host (issue #418, rationale at `SVGControl/SvgAssemblyResolver.cs` lines 17-29).
- **Do not redo or widen the PR #880 test-side fix.** `TestSupport/TestAssemblyResolver.cs`,
  `QuickFiler.Test/SetupAssemblyInitializer.cs` and `UtilitiesCS.Test/TestAssemblyInitializer.cs` are out
  of scope.
- **Do not modify** `QuickFiler.Test/app.config` or `UtilitiesCS.Test/app.config`.
- **Do not** pin, downgrade or otherwise change the `FSharp.Core` version or its binding redirect in any
  config file.
- **Do not** deploy a `netstandard.dll` facade into any `bin` output, and do not add `NETStandard.Library`
  to any `packages.config`.
- **Do not** modify `scripts/vscode/TaskMaster.cli.runsettings`, `TaskMaster.runsettings`,
  `coverage.config` or any file under `.github/workflows/`.
- **Do not** modify anything under `.claude/hooks/`, `.claude/rules/` or `.github/instructions/`.
- **Do not** create a new test project.
- **Do not** rely on an existing test assembly alone as the isolation mechanism.
- **Do not** build a child-process harness.
- **Do not** return `typeof(object).Assembly` from the resolver.
- **Do not** create or use temporary files in any test.
- No change to Deedle usage, to `DfDeedle`, or to any QuickFiler/ToDoModel data-model behaviour.

Explicitly excluded systems: the `devenv.exe` WinForms designer host scenario (issue #418); the test-host
masking half of the problem (issue #877 / PR #880); ClickOnce / VSTO application-manifest content.

## Scope Boundary Statement

Issue #877 / PR #880 fixed the TEST half and deliberately changed no production code. Issue #879 is the
PRODUCTION half. This plan authorises no edit to any PR #880 artefact and no edit to any `SVGControl`
file. The boundary is enforced mechanically by the task in Phase 6 that lists the merge-base diff and
asserts every excluded path is absent from it.

## Authorised Write Set (exhaustive, repository-relative)

Production:

1. `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` (new)
2. `UtilitiesCS/UtilitiesCS.csproj` (one new `Compile Include` item)
3. `TaskMaster/ThisAddIn.cs` (add `static ThisAddIn()`)
4. `TaskMaster/app.config` (one new `dependentAssembly` block)

Tests:

5. `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs` (new)
6. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (one new `Compile Include` item)
7. `TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs` (new)
8. `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` (new)
9. `TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs` (new)
10. `TaskMaster.Test/TaskMaster.Test.csproj` (three new `Compile Include` items)

Documents and evidence:

11. `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md`
12. `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/plan.2026-09-13T18-22.md`
13. Any path under
    `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/`

Conditional, only if the Phase 0 build-output premise check fails and the recorded substitution is taken:

14. `ToDoModel.Test/Bootstrap/ChildDomainBindProbe.cs` and
    `ToDoModel.Test/Bootstrap/NetstandardBindChildDomainTests.cs` replace items 7 and 8, and
    `ToDoModel.Test/ToDoModel.Test.csproj` joins the write set carrying those two new
    `Compile Include` items. Item 10 (`TaskMaster.Test/TaskMaster.Test.csproj`) is NOT replaced: it
    stays in the write set and gains one `Compile Include` item rather than three, because item 9
    (`TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs`) is NOT moved and stays in
    `TaskMaster.Test`. The reason item 9 stays is that
    `ToDoModel.Test/ToDoModel.Test.csproj` carries ProjectReferences to
    `ToDoModel` (line 310) and `UtilitiesCS` (line 314) only and does not reference `TaskMaster`, so
    `typeof(ThisAddIn)` would not compile there, and `TaskMaster.dll.config` is not in that project's
    build output. Under this substitution the child domains' `ConfigurationFile` is
    `ToDoModel.Test.dll.config` in `AppDomain.CurrentDomain.BaseDirectory`, and the file-name
    prohibition in `[P2-T6]` binds to `TaskMaster.dll.config` exactly as it does under the primary
    host. This substitution is a recorded amendment (task `[P0-T14]`), never a silent change.

Inherited-path rule (not a list): any path the executor's own harness writes as a side effect of running
— for example agent-memory files the executor maintains under `.claude/agent-memory/` — is outside this
write set and outside every scope assertion in this plan. Scope assertions below name their pathspecs
explicitly rather than asserting over the whole tree.

Untracked scratch output permitted and never committed: `TestResults/` (git-ignored by the
`[Tt]est[Rr]esult*/` pattern at `.gitignore` line 39) and `coverage/` (git-ignored by `coverage/*` at
`.gitignore` line 144).

---

## R1 — How the no-prior-SVG precondition is guaranteed, and how that guarantee is itself checked

The acceptance harness must reach the bind with no prior SVG rendering and no prior `AssemblyResolve`
handler. If any SVG-bearing control is constructed first, `SvgRenderer`'s type initializer installs the
rescuer and masks the result. That masking is how this defect hid inside the test suite.

**Mechanism.** Every bind observation happens inside a fresh child `AppDomain` created by
`AppDomain.CreateDomain` and driven through a public `MarshalByRefObject` proxy. The parent domain's
handler does not propagate into a child domain, so the PR #880 resolver linked into `QuickFiler.Test` and
`UtilitiesCS.Test` cannot reach it, and neither can anything a sibling test class did in the parent.

**The guarantee is made checkable by five falsifiable assertions, not by assumption:**

1. `ChildDomain_HasNoSvgControlAssemblyLoaded` — inside the child domain, no loaded assembly has simple
   name `SVGControl`. `SvgRenderer`'s type initializer cannot have run if its assembly is not loaded, so
   this is a complete proof that no SVG rendering occurred.
2. `ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall` — inside the child domain, the `AppDomain`
   assembly-resolution event has an empty invocation list before the installer under test runs, and a
   non-empty one after it. The second reading is the positive control on the counting mechanism:
   without it, a reflected field that is null in every state reports an empty list unconditionally and
   the first reading proves nothing.
3. `ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect` — the configuration file supplied to both
   child domains declares no `netstandard` `dependentAssembly` entry, so a positive result is
   attributable to the installer and not to the `TaskMaster/app.config` hardening.
4. `NegativeControl_WithoutInstall_Netstandard21Throws` — in a second child domain, with the installer
   not run, the `2.1.0.0` load throws `FileNotFoundException` naming `netstandard`.
5. `NegativeControl_HasNoUtilitiesCsAssemblyLoaded` — inside the installer-free second child domain, no
   loaded assembly has simple name `UtilitiesCS`. This is the checkable form of the design claim that
   keeping the installer call in its own probe method prevents `UtilitiesCS` from being JIT-resolved in
   that domain.

**The negative control is the load-bearing criterion.** It is the only thing that distinguishes a fixed
build from an unfixed one. It has its own task with its own acceptance condition, and it is verified in
Phase 2 — before the production fix exists — where it cannot be confounded by the fix. **If the load in
the negative-control domain ever succeeds without a code change, that is, if
`NegativeControl_WithoutInstall_Netstandard21Throws` ever fails because no `FileNotFoundException` was
raised, isolation has been lost, every positive assertion in the harness is vacuous, and no positive
result from this harness may be trusted.** The test carries that statement as an in-file comment.

**Verification of the `ConfigurationFile` choice, checked against the post-hardening tree.** Both child
domains set `ApplicationBase` to the directory of the test assembly and `ConfigurationFile` to
`TaskMaster.Test.dll.config` in that same directory. This was checked rather than inherited:

- `TaskMaster.Test/app.config` exists and carries a `FSharp.Core` redirect at line 62. Its only
  `assemblyIdentity` names relevant here is `FSharp.Core`; the file contains no occurrence of the string
  `netstandard`. It is outside the write set and outside the non-goals' permitted edits, so it cannot
  gain one during this work.
- The build output directory also contains `TaskMaster.dll.config`, the deployed image of
  `TaskMaster/app.config`, which **does** gain the `netstandard` redirect in Phase 3. Selecting that file
  by mistake would silently void the negative control. The design therefore pins the file name
  `TaskMaster.Test.dll.config` explicitly, and criterion 3 above fails loudly if the selected file
  carries a `netstandard` entry.

**One design correction made here rather than inherited.** The proxy evaluates every listed condition
**inside** the child domain and marshals primitive results back; FluentAssertions and the MSTest
assertion types are used only in the parent domain. Loading an assertion library into the child domain would add assemblies to a domain whose emptiness is
the whole point of the harness. The consequence is specific and is in the positive domain rather than
the negative one: if an assertion library dragged a `netstandard 2.0.0.0` facade into the positive
domain, ladder rung 1 would return it from the already-loaded set and
`AfterInstall_BothNetstandardVersionsBind` would pass without rungs 2 or 3 ever executing. The
negative control is not at risk from this: a `2.1.0.0` full-strong-name request is not satisfied by an
already-loaded `2.0.0.0` under the default binder. For the same reason `ChildDomainBindProbe` keeps
the installer call in a method separate from the bind attempt, so `UtilitiesCS` is never JIT-resolved
in the negative-control domain, and `NegativeControl_HasNoUtilitiesCsAssemblyLoaded` makes that
checkable rather than asserted.

---

## R2 — OPEN RISK: the unexplained `netstandard 2.0.0.0` frame

**This plan cannot explain the `2.0.0.0` frame.** The maintainer's reproduced production trace shows the
chain falling back to `netstandard, Version=2.0.0.0` and failing there as well, even though `2.0.0.0` is
the version present in the machine GAC and a fully specified strong-name reference should locate it.
Nothing in this repository accounts for that, and nothing in this plan establishes why it happened.

Consequences carried deliberately into the task list:

- **The remedy covers both versions.** `AfterInstall_BothNetstandardVersionsBind` is not optional: it
  asserts that after the installer runs, `Assembly.Load` of the full display name at `Version=2.1.0.0`
  **and** at `Version=2.0.0.0` both return a non-null assembly. A remedy that resolved `2.1.0.0` alone
  cannot satisfy it.
- **The ladder's decisive rung bypasses GAC lookup entirely.** Rung 3 loads the facade from
  `RuntimeEnvironment.GetRuntimeDirectory()` joined with `netstandard.dll` by absolute path. Whatever the
  `2.0.0.0` frame turns out to mean, that rung does not depend on it.
- **A task states the limit explicitly.** `[P6-T2]` writes the statement of what this work does and does
  not establish about the `2.0.0.0` leg, and that statement is a completion condition of the task.
- **The cheapest settling measurement is a real task.** `[P6-T3]` records the Fusion binding log request
  and result. **The fix does not wait on it**, for two reasons: rung 3 makes the remedy robust to every
  explanation the log could produce, and the log requires an HKLM registry change plus a live Outlook
  session, which is a maintainer gate that cannot be executed by the executor.
- **A free measurement that narrows the question is taken anyway.** `[P4-T10]` records the observed
  outcome of a `2.0.0.0` load in the negative-control child domain. If `2.0.0.0` binds there but failed
  in Outlook, the difference is localised to the add-in AppDomain rather than to the machine.

**Issue #879 must not be reported as closed on the strength of a `2.1.0.0` result alone.** The issue
comment written by `[P6-T25]` reproduces the `2.0.0.0` limit statement verbatim.

---

## R3 — EXECUTION RISK: the full-suite runs may stall on the shell-icon test classes

`[P0-T8]` and `[P5-T7]` run the full suite through `scripts/vscode/Invoke-MSTestWithCoverage.ps1` with
`-SearchRoot .`, which discovers every `*.Test.dll` under `Debug`. There is recorded history on this
machine of four shell-icon test classes in `UtilitiesCS.Test` stalling `vstest.console.exe` inside
`SHGetFileInfo`, and the runner exposes no `TestCaseFilter` parameter and no `/Blame` parameter, so this
plan has no lever inside the runner if the stall recurs. Whether it reproduces in this worktree has not
been measured.

This is recorded as an execution risk and is deliberately **not** an acceptance condition of any task.
No task in this plan asserts anything about it, and no task adds a runner parameter that does not exist.
The stated response for the executor is: if either full-suite task fails to produce a TRX and a Cobertura
document within the executor's own timeout, stop the run, record the observation in that task's artifact
with `EXIT_CODE:` and an `Output Summary:` naming the last test that started, and report blocked to the
caller. Do not edit `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which is outside the authorised write
set, and do not substitute a narrower discovery scope for the full-suite run, which would change the
coverage denominator that `[P5-T10]` compares.

---

## Remedy Summary

- New host-neutral type `UtilitiesCS.Bootstrap.AssemblyBindingFallback` in
  `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs`, with a public idempotent `Install()` and an
  internal `Resolve(AssemblyName)` seam. Resolution ladder, first non-null wins:
  1. an already-loaded assembly with a case-insensitively matching simple name and an equal public key
     token, version deliberately not compared;
  2. `Assembly.Load` of the **full** display name, using `Version=2.0.0.0` for the `netstandard`
     identity specifically;
  3. `Assembly.LoadFrom` of `RuntimeEnvironment.GetRuntimeDirectory()` joined with `netstandard.dll`, for
     the `netstandard` identity;
  4. a directory probe for the simple name plus `.dll` next to the executing assembly.
- The installer and handler never throw, use `System.Diagnostics.Trace` only (never log4net, which can
  re-enter assembly loading), and carry a `[ThreadStatic]` re-entrance guard.
- Eager installation from `static ThisAddIn()` in `TaskMaster/ThisAddIn.cs`, one statement and nothing
  else. `ThisAddIn` keeps its `[ExcludeFromCodeCoverage]` attribute (`TaskMaster/ThisAddIn.cs` line 18),
  which is why no resolution logic may live there.
- Declarative hardening, **not the fix**: one `netstandard` `dependentAssembly` block in
  `TaskMaster/app.config` inside the existing `assemblyBinding` element (opens at line 27, closes at
  line 468), covering `0.0.0.0-2.1.0.0` to `2.0.0.0`. A binding redirect rewrites an identity and cannot
  manufacture an assembly.

---

## Run Environment Constraints

**BUILD-LOCK convention.** Every `msbuild`, `csharpier`, `dotnet` and `vstest` step in this plan acquires
the shared machine build lock before running and releases it after, using item key `879`. These two are
the only absolute paths this plan contains.

Acquire (referred to below as **LOCK-ACQUIRE**):

```
pwsh -NoProfile -Command '& ([scriptblock]::Create((Get-Content -Raw "C:/Users/DanMoisan/repos/TaskMaster-wt/parallel-build-lock/acquire.txt"))) -Item "879"'
```

Release (referred to below as **LOCK-RELEASE**):

```
pwsh -NoProfile -Command '& ([scriptblock]::Create((Get-Content -Raw "C:/Users/DanMoisan/repos/TaskMaster-wt/parallel-build-lock/release.txt"))) -Item "879"'
```

LOCK-ACQUIRE must print `ACQUIRED 879` and exit 0 before the gated command runs. LOCK-RELEASE must print
`RELEASED by 879` and exit 0 after it. If LOCK-ACQUIRE prints `TIMEOUT` the executor reports blocked and
does not force the lock.

**Outlook gate.** A running Outlook holds a lock on the add-in build output. Outlook must be CLOSED — by
closing the window, never by killing the process — before any `msbuild` step. `[P0-T3]` is the explicit
gate task; `[P5-T1]` re-gates it before the final loop.

**Shell discipline (binding on every command in this plan).**

- The executor's Bash permission engine splits a command string on `&&`, `;` and `|` and requires every
  segment to match independently. Only `git *`, `pwsh *`, `poetry run *` and three named library scripts
  are permitted.
- Therefore: never prefix a command with `cd`; never chain `grep`, `sed`, `cat`, `head`, `tail`, `ls` or
  `find` onto another command. Every pwsh payload in this plan is written with newline-separated
  statements rather than `;` for that reason. A `|` that appears inside a single-quoted
  `pwsh -NoProfile -Command '...'` payload is part of one Bash segment and is permitted; `[P0-T4]`'s
  manifest read contains one. A `|` between two commands is not permitted, and this plan contains none.
  Where a pipeline is avoidable the payloads use `@(...)` indexing and the `.Where` method instead.
- Express file inspection as a `pwsh` one-liner or as a Read/Grep tool step.
- `git commit` with zero pathspec operands is denied. Every commit task in this plan appends
  `-- <explicit paths>`.
- Paths are repository-relative and commands run from the repository root of the item worktree.

**MSBuild and vstest resolution.** `msbuild` and `vstest.console.exe` are not assumed to be on `PATH`,
and no shell variable survives between tasks, so every gated task resolves its own tool path through
`vswhere` inside its own pwsh payload. `scripts/vscode/Invoke-VSBuild.ps1` is **not** used, because it
rewrites `csproj` HintPaths.

**Toolchain command invariants.**

- `/t:Rebuild`, never `/t:Build`. MSBuild's up-to-date check does not invalidate on a command-line `/p:`
  change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and the gate
  cannot fail.
- Do **not** add `/p:Nullable=enable`. No project carries a `Nullable` element, and the
  repository's `Directory.Build.props` sets `RxUseUnsupportedPackagesConfig` only and carries no
  `Nullable` element either, so the property is a solution-wide opt-in that conscripts every file
  that has never adopted the pragma. The nullable command below is character-for-character CI's.
- A successful `msbuild` run prints the word `error` dozens of times in unrelated contexts. No acceptance
  condition in this plan asserts on a bare `error` substring count. The success signal asserted is the
  anchored summary line whose pattern is `^\s+0 Error\(s\)$`, which does not also match `10 Error(s)`.
- CSharpier is pinned to 1.2.6 by `dotnet-tools.json` at the repository root and requires a subcommand.
  Run `dotnet tool restore` once per worktree before the first invocation.

**Runsettings selection for targeted runs.** The targeted `vstest.console.exe` runs in Phases 2 and 4
pass `scripts/vscode/TaskMaster.cli.runsettings`, not the repository-root `TaskMaster.runsettings`.
The root file declares a `DataCollector` named `Code Coverage` with no `enabled` attribute, which
defaults to enabled, so it activates profiler-based instrumentation in the process hosting the child
`AppDomain` whose minimality the harness measures. The CLI file carries the MSTest parallelisation
block only and no data collector, which is why `scripts/vscode/Invoke-MSTestWithCoverage.ps1` uses it
for the inner vstest invocation. `scripts/vscode/TaskMaster.cli.runsettings` is read here and never
modified; it remains in the non-goals as an unmodifiable file.

The substitution removes the collector but not parallelism. Both files carry the same MSTest block,
`<Workers>0</Workers>` with `<Scope>ClassLevel</Scope>`, read directly from
`scripts/vscode/TaskMaster.cli.runsettings` lines 3 to 7. Class-level scope means test methods within
one class run sequentially while separate classes in the same assembly run concurrently on worker
threads of the one host process. Ordering-dependent masking of exactly that shape is what issue #877
recorded, so the question is answered explicitly rather than assumed. **This plan takes option (b): the
harness class is pinned with `[DoNotParallelize]`.** The reason is that the two mechanisms in play
separate cleanly and only one of them is closed by AppDomain isolation:

- The observations the probe makes — the loaded-assembly set read through `AppDomain.GetAssemblies()`
  and the invocation list of the `AppDomain` assembly-resolution event — are per-`AppDomain` state
  reached through the child domain's own `AppDomain` instance. A sibling class running concurrently in
  the default domain loads assemblies into the default domain and subscribes handlers to the default
  domain's event, and neither is visible through the child domain's instance. That mechanism is closed
  by isolation and needs no pinning.
- What isolation does not close is the host process itself. `AppDomain.Unload`, which `[TestCleanup]`
  calls on every created domain, suspends the runtime and aborts the threads executing in the target
  domain, and it raises `CannotUnloadAppDomainException` when a thread cannot be aborted in time. Run
  concurrently with sibling classes on a machine under load, that is a failure mode of the harness that
  has nothing to do with the bind under test, and the harness's positive and negative domain pair is
  the only load-bearing evidence this plan produces. The attribute reduces that exposure rather than
  removing it: under `Workers=0` with `Scope=ClassLevel` a `[DoNotParallelize]` class is not run in a
  phase disjoint from the parallel bucket, so sibling classes can still execute alongside it. What the
  attribute is worth is therefore bounded by how many sibling classes each run discovers, and that was
  measured per run rather than assumed. `[P4-T7]` filters on
  `FullyQualifiedName~AddInEagerInstallShapeTests`, which discovers that one class, so that run carries
  no exposure from either direction. `[P2-T11]` and `[P4-T3]` filter on
  `FullyQualifiedName~TaskMaster.Test.Bootstrap`, which also discovers
  `AddInEagerInstallShapeTests` because `[P2-T8]` declares it in that namespace, so exactly one sibling
  class can execute alongside the harness in those two runs and the attribute narrows that single
  window. The attribute is also in force for the full-suite run at `[P5-T7]`, where the sibling set is
  every test class in the solution and the window is narrowed without being closed. `[P0-T8]` runs
  before the harness file exists, so no exposure arises there.

The pinning is made checkable by `[P2-T6]`'s acceptance condition, which requires the literal
`[DoNotParallelize]` to appear exactly once in
`TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`. The literal is quoted here in prose
because it is absent from the tree until `[P2-T6]` runs.

---

### Phase 0 — Policy Reading, Baseline Capture, and Premise Closure

- [x] [P0-T1] Read, in this exact order, `CLAUDE.md`, `.claude/rules/general-code-change.md`,
      `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/csharp.md`,
      `.claude/rules/tonality.md`, and `.claude/rules/plan-acceptance-gates.md`. Write
      `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/phase0-instructions-read.2026-09-13T18-22.md`
      containing `Timestamp:`, `Policy Order:` and the explicit list of the seven files read.
      Acceptance: the artifact exists and names all seven files.
- [x] [P0-T2] Read `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/issue.md`,
      `.../spec.md` and
      `.../research/2026-09-13T19-05-deedle-netstandard-bind-research.md` in full, and append to the
      `[P0-T1]` artifact a `Requirements Sources:` section listing those three paths and the line
      `AC source: spec.md section "## Acceptance Criteria", 19 criteria`.
      Acceptance: the artifact carries that section and that exact count.
- [x] [P0-T3] Outlook-closed gate. Confirm no `OUTLOOK` process is running, by closing the Outlook window
      if one is open. Never kill the process. Command:
      `pwsh -NoProfile -Command '@(Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue).Count'`.
      Write
      `.../evidence/baseline/outlook-closed-gate.2026-09-13T18-22.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:` and `Output Summary:` recording the observed count.
      Acceptance: the recorded count is `0` and the artifact exists.
- [x] [P0-T4] LOCK-ACQUIRE, bootstrap the toolchain in four steps beginning with scripts/vscode/Install-RepoDotNetSdk.ps1, then LOCK-RELEASE. This
      worktree is fresh: `.dotnet-sdk` and `packages/` are both absent, `global.json` pins SDK
      `8.0.205` with `paths` `.dotnet-sdk` and `$host$`, and every `dotnet` invocation fails with the
      `global.json` `errorMessage` until the repo-local SDK is installed. Steps, in this order:

      ```
      pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1
      ```

      ```
      pwsh -NoProfile -Command 'dotnet tool restore'
      ```

      ```
      pwsh -NoProfile -Command '
      $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
      $msb = @(& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\amd64\MSBuild.exe")[0]
      & $msb TaskMaster.sln /t:Restore /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:RestorePackagesConfig=true
      $LASTEXITCODE
      '
      ```

      Then record the pinned CSharpier version from the manifest rather than from the restore output,
      because the restore command's success-case output has not been observed in this worktree:

      ```
      pwsh -NoProfile -Command '
      $m = Get-Content -LiteralPath "dotnet-tools.json" -Raw | ConvertFrom-Json
      Write-Output ("CSHARPIER_PINNED_VERSION=" + $m.tools.csharpier.version)
      Write-Output ("PACKAGES_DIR_PRESENT=" + (Test-Path -LiteralPath "packages"))
      '
      ```

      Write `.../evidence/baseline/toolchain-bootstrap.2026-09-13T18-22.md` with `Timestamp:`,
      `Command:` listing all four commands, `EXIT_CODE:` for each, and an `Output Summary:` carrying
      both emitted lines.
      Acceptance: every one of the four commands exits 0, and the `Output Summary:` records
      `CSHARPIER_PINNED_VERSION=1.2.6` and `PACKAGES_DIR_PRESENT=True`. A failure of any of the four
      blocks: the analyzer, nullable and test baselines below all depend on a restored SDK and a
      restored `packages` tree.
- [x] [P0-T5] LOCK-ACQUIRE, then capture the format baseline read-only:
      `pwsh -NoProfile -Command 'dotnet tool run csharpier check .'`. Then LOCK-RELEASE. Write
      `.../evidence/baseline/format-baseline.2026-09-13T18-22.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:`, `ExpectedExitCode:` and `Output Summary:` recording the number of files reported as
      unformatted. This is the read-only `check` subcommand, so its exit code does distinguish a clean
      tree from a drifted one; no file is rewritten by this task.
      Acceptance: the artifact exists and records the observed exit code and the unformatted-file count
      as an integer. A non-zero baseline is recorded, not repaired, at this point.
- [x] [P0-T6] LOCK-ACQUIRE, then capture the analyzer baseline with the console log redirected to the
      evidence tree. Then LOCK-RELEASE.

      ```
      pwsh -NoProfile -Command '
      $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
      $msb = @(& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\amd64\MSBuild.exe")[0]
      & $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true *> "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/analyzer-baseline-console.2026-09-13T18-22.txt"
      $LASTEXITCODE
      '
      ```

      Write `.../evidence/baseline/analyzer-baseline.2026-09-13T18-22.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:`, `ExpectedExitCode:` and `Output Summary:` recording the observed
      `Warning(s)`/`Error(s)` summary counts read from the console log.
      Acceptance: both the `.txt` console log and the `.md` artifact exist, and the `.md` records the
      integer baseline error count.
- [ ] [P0-T7] LOCK-ACQUIRE, then capture the nullable baseline with the console log redirected to the
      evidence tree. Then LOCK-RELEASE. Same pwsh shape as `[P0-T6]` with the final msbuild line replaced
      by:

      ```
      & $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true *> "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/nullable-baseline-console.2026-09-13T18-22.txt"
      ```

      Write `.../evidence/baseline/nullable-baseline.2026-09-13T18-22.md` with the four required fields
      plus `ExpectedExitCode:`.
      Acceptance: both artifacts exist and the `.md` records the integer baseline error count.
- [ ] [P0-T8] LOCK-ACQUIRE, then capture the coverage-bearing test baseline:
      `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`.
      Leave `-CoverageOutput` at its default, `coverage\coverage.cobertura.xml`: the runner deletes the
      raw Cobertura document unless its parent directory is exactly the repository `coverage` directory.
      Then LOCK-RELEASE.
      Note the runner's observed behaviour, which the acceptance condition is written against. There
      are two distinct non-zero-exit paths and they differ in what they leave on disk:
      (a) `Invoke-MSTestWithCoverage.ps1` line 262 throws on a non-zero collection exit code, which
      happens BEFORE post-processing at lines 383-384, so on a run with any failing test the document
      at `coverage/coverage.cobertura.xml` is the RAW collector output, carrying absolute paths and
      third-party packages;
      (b) the repository-wide 80 percent line-rate assertion at line 386 runs AFTER post-processing, so
      on that path the document on disk is the post-processed one.
      The artifact must state which path it observed, because the two carry different denominators and
      `[P5-T10]` compares this figure against a post-processed one.
      Write `.../evidence/baseline/test-coverage-baseline.2026-09-13T18-22.md` with `Timestamp:`,
      `Command:`, `EXIT_CODE:`, `ExpectedExitCode:` and an `Output Summary:` carrying these numeric
      headline values read from `coverage/coverage.cobertura.xml`: the document-level `line-rate`, the
      document-level `lines-valid`, the document-level `lines-covered`, and the total test count, failed
      count and passed count read from the TRX under `coverage/test-results`.
      Acceptance: the artifact exists, all six numeric values are present as numbers rather than
      placeholders, and the artifact carries a `Cobertura Document State:` field whose value is exactly
      one of `POSTPROCESSED` or `RAW-COLLECTOR-OUTPUT`, determined by whether
      `coverage/coverage.cobertura.xml` contains the literal `<sources>`, which post-processing
      injects and the raw document does not. A non-zero exit code caused by the runner's
      repository-wide threshold assertion is recorded with a matching `ExpectedExitCode:` and does not
      block; a missing numeric value does block.
- [ ] [P0-T9] Record the baseline for the changed-line and new-module coverage obligations. Append to the
      `[P0-T8]` artifact a `Coverage Obligations:` section stating: repository-wide line coverage floor
      per `CLAUDE.md` is `>= 80%` on the testable denominator; new modules target `>= 90%`; changed lines
      must not regress; `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` does not yet exist, so its
      baseline per-file line coverage is recorded as `NOT PRESENT AT BASELINE`.
      Acceptance: that section exists and carries that exact status string for the new file.
- [ ] [P0-T10] Close the build-output premise for the harness host. Using the build produced by `[P0-T7]`,
      record whether each of these three files exists:
      `TaskMaster.Test/bin/Debug/Deedle.dll`, `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll.config`,
      `TaskMaster.Test/bin/Debug/TaskMaster.dll.config`. Command:

      ```
      pwsh -NoProfile -Command '
      $paths = @("TaskMaster.Test/bin/Debug/Deedle.dll","TaskMaster.Test/bin/Debug/TaskMaster.Test.dll.config","TaskMaster.Test/bin/Debug/TaskMaster.dll.config")
      foreach ($p in $paths) { Write-Output ($p + " EXISTS=" + (Test-Path -LiteralPath $p)) }
      '
      ```

      Write `.../evidence/baseline/build-output-premises.2026-09-13T18-22.md` with `Timestamp:`,
      `Command:`, `EXIT_CODE:` and an `Output Summary:` reproducing the three `EXISTS=` lines verbatim.
      Acceptance: the artifact exists and carries exactly three `EXISTS=` lines.
- [ ] [P0-T11] Record the premise that `TaskMaster.Test/app.config` carries no `netstandard` entry, which
      is what keeps the negative control valid after the Phase 3 hardening lands. Command:

      ```
      pwsh -NoProfile -Command '
      $hits = @(Select-String -LiteralPath "TaskMaster.Test/app.config" -SimpleMatch -Pattern "netstandard")
      Write-Output ("TASKMASTER_TEST_APPCONFIG_NETSTANDARD_HITS=" + $hits.Count)
      $fs = @(Select-String -LiteralPath "TaskMaster.Test/app.config" -SimpleMatch -Pattern "FSharp.Core")
      Write-Output ("TASKMASTER_TEST_APPCONFIG_FSHARPCORE_HITS=" + $fs.Count)
      '
      ```

      The second search is a positive control proving the search mechanism and the file path are live; a
      zero count there would mean the first result proves nothing. Append both lines to the `[P0-T10]`
      artifact.
      Acceptance: `TASKMASTER_TEST_APPCONFIG_NETSTANDARD_HITS=0` and
      `TASKMASTER_TEST_APPCONFIG_FSHARPCORE_HITS` is greater than 0.
- [ ] [P0-T12] Indicative probe of the private `AppDomain` assembly-resolution field. Command:

      ```
      pwsh -NoProfile -Command '
      $f = [AppDomain].GetField("_AssemblyResolve", [Reflection.BindingFlags]"Instance,NonPublic")
      Write-Output ("PWSH_HOST_RUNTIME=" + [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription)
      Write-Output ("PWSH_HOST_FIELD_PRESENT=" + ($null -ne $f))
      '
      ```

      Write `.../evidence/baseline/appdomain-resolve-field-probe.2026-09-13T18-22.md` with `Timestamp:`,
      `Command:`, `EXIT_CODE:` and an `Output Summary:` reproducing both lines, followed by this exact
      sentence: `The runtime observed here is the pwsh host runtime, not net481; this observation is
      indicative only and the decisive net481 observation is task [P2-T12].`
      Acceptance: the artifact exists, carries both `PWSH_HOST_` lines, and carries that sentence.
- [ ] [P0-T13] Record the fail-loud rule for the isolation assertion, so no later task can weaken it.
      Append to the `[P0-T12]` artifact a `Fail-Loud Rule:` section stating: the child-domain probe
      resolves the field by the name `_AssemblyResolve` with `BindingFlags.Instance | BindingFlags.NonPublic`;
      if the lookup returns `null` the probe throws `InvalidOperationException` naming the field; it must
      never call `Assert.Inconclusive`, never return a sentinel that the test treats as success, and never
      skip. A silently skipped isolation check makes every positive test in the harness vacuous.
      Acceptance: that section exists and contains the literal token `InvalidOperationException`.
- [ ] [P0-T14] Write-set decision record for the harness host.
      Write `.../evidence/baseline/write-set-decision.2026-09-13T18-22.md` with `Timestamp:` and a
      `Decision:` field whose value is exactly one of `HOST=TaskMaster.Test` or `HOST=ToDoModel.Test`.
      The rule is mechanical and leaves the executor no choice: `HOST=TaskMaster.Test` if and only if the
      `[P0-T10]` artifact records `TaskMaster.Test/bin/Debug/Deedle.dll EXISTS=True` and
      `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll.config EXISTS=True`; otherwise
      `HOST=ToDoModel.Test`, and the artifact additionally carries an `Amendment:` section reproducing
      write-set item 14 of this plan verbatim and stating that items 7 and 8 are replaced, that
      `ToDoModel.Test/ToDoModel.Test.csproj` joins the write set, and that item 10 stays in the write
      set and gains one `Compile Include` item rather than three.
      Independently of the host decision, the artifact carries a `Deployed Add-In Config:` field whose
      value is the `TaskMaster.Test/bin/Debug/TaskMaster.dll.config EXISTS=` line copied verbatim from
      the `[P0-T10]` artifact. If that line reads `EXISTS=False`, the executor reports blocked before
      Phase 2 rather than proceeding: `[P2-T8]`'s `AppConfig_DeclaresNetstandardRedirect` resolves that
      file from `AppDomain.CurrentDomain.BaseDirectory` and fails loudly when it is absent, so the
      absence is a build-configuration problem to be resolved before the harness is written, not a
      test failure to be discovered at `[P4-T7]`.
      Acceptance: the artifact exists, `Decision:` carries exactly one of the two permitted values,
      `Deployed Add-In Config:` is present and reads `EXISTS=True`, and when the value is
      `HOST=ToDoModel.Test` the `Amendment:` section is present. When the value is
      `HOST=ToDoModel.Test` the `Amendment:` section must additionally state, in its
      own line, `AddInEagerInstallShapeTests REMAINS IN TaskMaster.Test` and
      `CHILD DOMAIN CONFIGURATION FILE = ToDoModel.Test.dll.config`, and every subsequent task that
      names a path beginning `TaskMaster.Test/Bootstrap/` is read with `ToDoModel.Test/Bootstrap/`
      substituted for that prefix, except
      `TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs`, which is not moved. The tasks
      affected are exactly `[P2-T5]`, `[P2-T6]`, `[P2-T7]`, `[P2-T9]`, `[P4-T9]`, `[P4-T12]`,
      `[P5-T2]`, `[P5-T8]` and `[P6-T1]`. In `[P2-T9]` and `[P6-T1]` the owning project file for the
      two moved harness files becomes `ToDoModel.Test/ToDoModel.Test.csproj` while
      `AddInEagerInstallShapeTests.cs` stays registered in `TaskMaster.Test/TaskMaster.Test.csproj`,
      and in `[P5-T2]` the write-set paths in items 1 to 10 are read with write-set item 14 applied,
      which substitutes two of them and adds `ToDoModel.Test/ToDoModel.Test.csproj`, so the permitted
      set has eleven members rather than ten. The
      `Amendment:` section reproduces this list of nine task IDs verbatim, so the substitution is
      recorded once and no later task is left naming a path that does not exist.

      That list is exhaustive for the path-prefix rule and for nothing else. Two further substitutions
      follow from the same decision without being path-prefix substitutions, and the `Amendment:`
      section reproduces them alongside the nine task IDs. First, the vstest assembly operand
      `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll` in `[P2-T11]`, and in `[P4-T3]` which takes its
      command shape from `[P2-T11]`, is read as `ToDoModel.Test/bin/Debug/ToDoModel.Test.dll`; the
      namespace `TaskMaster.Test.Bootstrap` that `[P2-T5]` and `[P2-T6]` declare is deliberately NOT
      substituted, so the `/TestCaseFilter:"FullyQualifiedName~TaskMaster.Test.Bootstrap"` operand
      still discovers the two moved classes in the substituted assembly. Second, the child-domain
      configuration file named in `[P2-T6]`, including the literal its acceptance condition searches
      for, is read as `ToDoModel.Test.dll.config`, which is the value write-set item 14 and the
      `CHILD DOMAIN CONFIGURATION FILE` line above already fix. `[P4-T7]` is in neither list: it
      filters on `FullyQualifiedName~AddInEagerInstallShapeTests`, and that class stays in
      `TaskMaster.Test` under this substitution.
- [ ] [P0-T15] Record the baseline scope-boundary state. Command:

      ```
      git rev-parse --verify origin/main
      git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs/features"
      git diff --name-only origin/main...HEAD
      ```

      The `git rev-parse --verify origin/main` exit 0 is a gate: if `origin/main` does not resolve, the
      executor reports blocked rather than substituting another ref. The porcelain span is the
      companion the name-listing diff needs, and the two mechanisms are complementary because each
      alone is wrong in one state: the anchored diff enumerates tracked committed changes only and is
      blind to a file this plan has created but not yet committed, and porcelain status goes empty once
      the change is committed. This task runs after `[P0-T1]` through `[P0-T14]`, which have already
      written baseline artifacts under this feature folder and have already marked their own entries in
      this plan file, so a span that included `docs/features` would list every one of them. The span
      therefore carries the same two exclusions `[P6-T5]` carries, which is what makes the two outputs
      comparable. The merge-base diff is separately expected to be non-empty here: the branch already
      carries the committed documentation that introduced `issue.md`, `spec.md`,
      `research/2026-09-13T19-05-deedle-netstandard-bind-research.md` and this plan file, so those four
      paths are the expected Phase 0 diff content and are recorded rather than treated as a defect.
      Append both outputs to `.../evidence/baseline/write-set-decision.2026-09-13T18-22.md`, the diff under a
      `Baseline Merge-Base Diff:` heading and the porcelain output under a
      `Baseline Porcelain Status:` heading, each empty output recorded as the literal `NONE`.

      Three-dot behaviour was verified by the planner: `origin/main` and the merge base with `HEAD`
      differ at authoring time, so `origin/main...HEAD` resolves to merge-base-to-HEAD; after a later
      merge of `main` the merge base becomes `origin/main` itself and the three-dot form degenerates to
      `origin/main..HEAD`, which still enumerates exactly the branch's own changes. The three-dot form
      is correct in both states.

      In the same task also capture the two unchanged-baseline comparators the Phase 4 sweep will
      compare against:

      ```
      pwsh -NoProfile -Command '
      Write-Output ("BASELINE_FSHARP_REDIRECT_LINES=" + @(Select-String -Path "*/app.config" -CaseSensitive -Pattern "oldVersion=.0\.0\.0\.0-11\.0\.0\.0.").Count)
      Write-Output ("BASELINE_NETSTANDARD_DLL_IN_PROJECTS=" + @(Select-String -Path "*/*.csproj" -SimpleMatch -CaseSensitive -Pattern "netstandard.dll").Count)
      '
      ```

      The pattern is a regular expression whose leading and trailing `.` match the quotation marks; the
      literal is `oldVersion="0.0.0.0-11.0.0.0"`. Append both lines to the same artifact under a
      `Baseline Comparators:` heading.
      Acceptance: `git rev-parse --verify origin/main` exits 0; both the `Baseline Merge-Base Diff:`
      and the `Baseline Porcelain Status:` headings exist with the raw output beneath them, empty
      output recorded as the literal `NONE`; the `Baseline Porcelain Status:` output is the literal
      `NONE`, or names only paths that are members of items 1 to 10 of `## Authorised Write Set`, and
      any other path blocks because it would mean the bootstrap or a baseline build wrote into the
      tracked tree; the `Baseline Merge-Base Diff:` output names only paths beginning
      `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/`,
      and a path outside that prefix blocks, except that a path beginning `.claude/` is an inherited
      path under the inherited-path rule in `## Authorised Write Set` and is recorded rather than
      blocking; and
      `BASELINE_FSHARP_REDIRECT_LINES` is recorded as an integer greater than 0. A value of 0 blocks,
      because it would mean the search mechanism found nothing at baseline and no later comparison
      against it would prove anything.
- [ ] [P0-T16] Record the baseline line counts of the five files this plan will edit in place, so the
      Phase 5 file-size audit has a comparison point. Command:

      ```
      pwsh -NoProfile -Command '
      $paths = @("TaskMaster/ThisAddIn.cs","TaskMaster/app.config","UtilitiesCS/UtilitiesCS.csproj","UtilitiesCS.Test/UtilitiesCS.Test.csproj","TaskMaster.Test/TaskMaster.Test.csproj")
      foreach ($p in $paths) { Write-Output ($p + " LINES=" + @(Get-Content -LiteralPath $p).Count) }
      '
      ```

      Append the output to `.../evidence/baseline/build-output-premises.2026-09-13T18-22.md` under a
      `Baseline Line Counts:` heading.
      Acceptance: the heading exists and carries exactly five `LINES=` lines.

### Phase 1 — Spec Correction and Acceptance-Criteria Inventory

- [ ] [P1-T1] Confirm the planner's spec correction is present. The acceptance criterion at `spec.md`
      line 547-549 previously directed the coverage artifact to `evidence/coverage/`, which is not a
      canonical evidence kind. The planner changed that one directory reference to `evidence/qa-gates/`
      and changed nothing else in that criterion. Verify with:

      ```
      pwsh -NoProfile -Command '
      $p = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md"
      Write-Output ("COVERAGE_DIR_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -Pattern "evidence/coverage/").Count)
      Write-Output ("QA_GATES_DIR_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -Pattern "evidence/qa-gates/").Count)
      '
      ```

      The literals asserted are `evidence/coverage/` and `evidence/qa-gates/`, quoted here in prose so the
      assertion is exonerated for a literal the tree is expected not to contain.
      Acceptance: `COVERAGE_DIR_HITS=0` and `QA_GATES_DIR_HITS` is greater than 1.
- [ ] [P1-T2] Record the acceptance-criteria inventory by `spec.md` line number, so every later check-off
      task names a determinate target. Write
      `.../evidence/other/ac-inventory.2026-09-13T18-22.md` listing exactly these nineteen pairs:
      AC1 line 473, AC2 line 477, AC3 line 481, AC4 line 484, AC5 line 491, AC6 line 495, AC7 line 499,
      AC8 line 504, AC9 line 509, AC10 line 515, AC11 line 521, AC12 line 528, AC13 line 534, AC14
      line 536, AC15 line 543, AC16 line 545, AC17 line 547, AC18 line 550, AC19 line 554.
      Acceptance: the artifact exists and carries exactly nineteen `AC` entries, and a spot check confirms
      that each named line currently begins with the six characters `- [ ] `.
- [ ] [P1-T3] Confirm no non-canonical evidence directory remains in `spec.md`. Command:

      ```
      pwsh -NoProfile -Command '
      $p = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md"
      $bad = @("artifacts/baselines/","artifacts/baseline/","artifacts/qa/","artifacts/qa-gates/","artifacts/evidence/","artifacts/coverage/","evidence/coverage/","evidence/post-change/")
      foreach ($b in $bad) { Write-Output ($p + " " + $b + " HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -Pattern $b).Count) }
      '
      ```

      The sweep covers `spec.md` alone and deliberately never covers this plan file. This plan file
      quotes all eight non-canonical literals — in this task's own search list and again in `[P1-T1]` —
      so a zero-hit sweep over it could not pass whatever the executor does, and would gate nothing.
      The plan side of the obligation is discharged by the binding evidence-path shorthand paragraph
      near the top of this plan, which fixes every evidence path this plan names to one of the six
      canonical kinds, and by `[P1-T4]`, which reproduces the authorised write set.
      Append the output to the `[P1-T2]` artifact under a `Non-Canonical Evidence Path Sweep:` heading.
      Acceptance: the heading exists, exactly eight lines are emitted, and every line ends with
      `HITS=0`.
- [ ] [P1-T4] Lock the scope. Append to the `[P1-T2]` artifact a `Scope Lock:` section reproducing this
      plan's `## Authorised Write Set` items 1 to 13 verbatim, plus the sentence
      `No file outside this list, and outside the inherited-path rule, may be created or modified by this plan.`
      Then read the `Decision:` field of `.../evidence/baseline/write-set-decision.2026-09-13T18-22.md`
      and append a `Host Substitution:` line whose value is that field verbatim. When the value is
      `HOST=ToDoModel.Test`, the `Scope Lock:` section additionally reproduces write-set item 14
      verbatim and states which of items 7 to 10 it replaces, so the lock and the recorded amendment do
      not contradict each other.
      Acceptance: that section exists, lists thirteen numbered items, carries the quoted sentence, and
      carries a `Host Substitution:` line holding exactly one of `HOST=TaskMaster.Test` or
      `HOST=ToDoModel.Test`. When the value is `HOST=ToDoModel.Test`, item 14 is reproduced beneath it.

### Phase 2 — Regression Harness First (fails before the fix)

The bugfix workflow requires the regression test before the fix. A C# regression test cannot reference a
type that does not exist, so Phase 2 lands a **declaration-complete, behaviour-empty** seam: the public
and internal surface of `AssemblyBindingFallback` compiles, and every ladder rung returns `null`. The
harness therefore fails at **runtime**, not at compile time, which is the fail-before evidence this issue
needs. Phase 3 supplies the behaviour.

Phase 2 runs a targeted build and a targeted test run only. It does not run the analyzer gate, the
nullable gate or the full suite: those gates would be evaluated against a deliberately incomplete seam.

- [ ] [P2-T1] Create `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` declaring
      `public static class AssemblyBindingFallback` in namespace `UtilitiesCS.Bootstrap`, with:
      a public `static void Install()` guarded for idempotence by `Interlocked.Exchange` on a private
      `int` field; an `internal static Assembly Resolve(AssemblyName requested)` seam; a private
      `[ThreadStatic]` re-entrance guard; and an `internal sealed class AssemblyBindingLadder` whose
      constructor takes five injectable delegates — get-loaded-assemblies, load-by-display-name,
      load-from-path, file-exists, and get-runtime-directory — with production defaults supplied by
      `AssemblyBindingFallback`. **In this task every ladder rung returns `null`, so the ladder resolves
      nothing, and `Install()` subscribes the handler to `AppDomain.CurrentDomain.AssemblyResolve`
      through the `Interlocked.Exchange` guard and does nothing else.** It is the ladder, not the
      subscription, that is behaviour-empty in this phase. The subscription belongs here rather than in
      Phase 3 because `[P2-T6]`'s positive control on `CountAssemblyResolveHandlers()` reads the
      invocation list after `InstallProductionFallback()` in the `[P2-T11]` run, and an `Install()` with
      an empty body would report an empty list there and make `[P2-T12]` unsatisfiable. The fail-before
      property is unaffected: a subscribed handler whose every rung returns `null` resolves no
      `netstandard` identity, so `AfterInstall_BothNetstandardVersionsBind` and
      `AfterInstall_DeedleTypeInitializerSucceeds` still fail at runtime in `[P2-T11]`. The file carries
      `#nullable enable` and XML documentation on the public surface. No
      WinForms type, no Outlook Interop type, no log4net reference.
      Acceptance: the file exists, `Select-String -SimpleMatch` on it returns at least one hit each for
      `public static void Install()`, `internal static Assembly Resolve(`,
      `internal sealed class AssemblyBindingLadder` and `[ThreadStatic]`, and zero hits for
      `System.Windows.Forms`, `Microsoft.Office.Interop` and `log4net`.
- [ ] [P2-T2] Register the new production file. Insert
      `<Compile Include="Bootstrap\AssemblyBindingFallback.cs" />` into the `ItemGroup` in
      `UtilitiesCS/UtilitiesCS.csproj` that already contains
      `<Compile Include="Extensions\DfDeedle.cs" />` at line 996. This project uses explicit `Compile`
      items; an unregistered file silently does not build.
      Acceptance: `Select-String -SimpleMatch -Pattern "Bootstrap\AssemblyBindingFallback.cs"` on
      `UtilitiesCS/UtilitiesCS.csproj` returns exactly 1 hit.
- [ ] [P2-T3] Create `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs`, MSTest with Moq and
      FluentAssertions, one `[TestClass]` named `AssemblyBindingFallbackTests` in namespace
      `UtilitiesCS.Test.Bootstrap`, with one separately named `[TestMethod]` for each of: already-loaded
      match wins over a fresh load; the full-display-name rung; the runtime-directory load-from-path rung;
      a mismatched public key token is rejected; a null requested token; an empty requested token; the
      re-entrance guard returns null on a nested request for the same simple name; `Install()` called
      twice attaches one handler; a rung that throws internally is absorbed and the ladder continues; an
      unresolvable name returns null without throwing. Every rung is driven through the injected
      delegates, so the tests touch neither the GAC nor the filesystem. A `[TestCleanup]` resets any
      mutated seam. The class carries Arrange-Act-Assert structure and a short intent comment per test.
      Acceptance: the file exists and contains at least ten occurrences of `[TestMethod]`.
- [ ] [P2-T4] Register the new unit-test file. Insert
      `<Compile Include="Bootstrap\AssemblyBindingFallbackTests.cs" />` into the `ItemGroup` in
      `UtilitiesCS.Test/UtilitiesCS.Test.csproj` that already contains
      `<Compile Include="Extensions\DfDeedle_Tests.cs" />` at line 193.
      Acceptance: `Select-String -SimpleMatch -Pattern "Bootstrap\AssemblyBindingFallbackTests.cs"` on
      `UtilitiesCS.Test/UtilitiesCS.Test.csproj` returns exactly 1 hit.
- [ ] [P2-T5] Create `TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs` declaring
      `public sealed class ChildDomainBindProbe : MarshalByRefObject` in namespace
      `TaskMaster.Test.Bootstrap`. It exposes separate public methods so that the negative-control path
      never JIT-resolves `UtilitiesCS`: `CountLoadedAssembliesNamed(string simpleName)`;
      `CountAssemblyResolveHandlers()` which resolves the private `AppDomain` field `_AssemblyResolve`
      with `BindingFlags.Instance | BindingFlags.NonPublic`, throws `InvalidOperationException` naming the
      field when the lookup returns null, returns 0 when the field value is null, and otherwise returns
      the length of `Delegate.GetInvocationList()`; `InstallProductionFallback()` which calls
      `UtilitiesCS.Bootstrap.AssemblyBindingFallback.Install()` and nothing else;
      `TryLoadDisplayName(string displayName)` returning a marshalled outcome string that is either
      `LOADED` or the exception type name; `DeedleTypeInitializerOutcome(string deedleDllPath)` which
      checks `File.Exists`, throws `InvalidOperationException` when absent, loads the assembly by absolute
      path, obtains the type `Deedle.Reflection` and forces its class constructor through
      `RuntimeHelpers.RunClassConstructor`, returning `OK` or the exception type name; and
      `ConfigurationFileNetstandardEntryCount(string configPath)` which throws
      `InvalidOperationException` when the file is absent and otherwise parses it as XML and returns the
      number of `dependentAssembly` elements whose `assemblyIdentity` name is `netstandard`.
      The type references no FluentAssertions type and no MSTest assertion type, because loading an
      assertion library into the child domain would add assemblies to a domain whose emptiness is the
      point of the harness, and a `netstandard` facade arriving that way in the POSITIVE domain would
      be returned by ladder rung 1, letting `AfterInstall_BothNetstandardVersionsBind` pass without
      rungs 2 or 3 executing. The probe's own error reporting honours the same rule: it throws
      `InvalidOperationException`, a BCL type that marshals across the domain boundary, and returns
      outcomes as plain strings.
      Acceptance: the file exists, contains `: MarshalByRefObject`, contains `InvalidOperationException`
      at least three times, and returns zero hits for `FluentAssertions` and for `Microsoft.VisualStudio.TestTools`.
- [ ] [P2-T6] Create `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` declaring
      `[TestClass] public class NetstandardBindChildDomainTests` in namespace `TaskMaster.Test.Bootstrap`,
      creating each child domain with `AppDomain.CreateDomain` using an `AppDomainSetup` whose
      `ApplicationBase` is `AppDomain.CurrentDomain.BaseDirectory` and whose `ConfigurationFile` is
      `TaskMaster.Test.dll.config` in that same directory, driving `ChildDomainBindProbe` through
      `CreateInstanceAndUnwrap`, and unloading every created domain in `[TestCleanup]` with
      `AppDomain.Unload`. The file must not name `TaskMaster.dll.config`, which after Phase 3 carries the
      `netstandard` redirect and whose selection would void the negative control. Test methods, named
      exactly: `ChildDomain_HasNoSvgControlAssemblyLoaded`,
      `ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall`,
      `ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect`,
      `AfterInstall_BothNetstandardVersionsBind`, `AfterInstall_DeedleTypeInitializerSucceeds`,
      `NegativeControl_WithoutInstall_Netstandard21Throws`,
      `NegativeControl_Netstandard20Observation_IsRecorded`, and
      `NegativeControl_HasNoUtilitiesCsAssemblyLoaded`, which runs in the installer-free domain and
      asserts that `CountLoadedAssembliesNamed("UtilitiesCS")` returns 0. This is the checkable form
      of the design claim that keeping the installer call in its own probe method prevents
      `UtilitiesCS` from being JIT-resolved in that domain; without it the claim is prose.

      Domain assignment is fixed by this plan and is not the executor's choice.
      `ChildDomain_HasNoSvgControlAssemblyLoaded`,
      `ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall` and
      `ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect` run in the POSITIVE domain, and
      `ChildDomain_HasNoSvgControlAssemblyLoaded` calls `InstallProductionFallback()` first and
      `CountLoadedAssembliesNamed("SVGControl")` second, in that order, so it observes the domain in
      the state in which `AfterInstall_BothNetstandardVersionsBind` measures the bind. Asserting it
      before the installer call, or in the installer-free domain, would pass because `UtilitiesCS` was
      never loaded, and would establish nothing about the domain the positive result comes from.
      `ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall` is the one method that observes the
      positive domain BEFORE `InstallProductionFallback()`, which is what its name states.
      `NegativeControl_WithoutInstall_Netstandard21Throws` and
      `NegativeControl_Netstandard20Observation_IsRecorded` run in the installer-free domain.

      `AfterInstall_BothNetstandardVersionsBind` and `AfterInstall_DeedleTypeInitializerSucceeds` run in
      the POSITIVE domain, and each calls `InstallProductionFallback()` before the observation it makes,
      in that order. `ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect` runs in the POSITIVE
      domain and its position relative to `InstallProductionFallback()` is deliberately unconstrained,
      because it reads the configuration file supplied at domain creation and the installer neither
      reads nor writes that file. Every one of the eight methods therefore has a stated domain, and
      every method whose result can depend on the installer has a stated order relative to it.

      `ChildDomain_HasNoSvgControlAssemblyLoaded` additionally records
      `CountLoadedAssembliesNamed("UtilitiesCS")` as a positive control on the counting mechanism and
      asserts it is greater than 0. The control is sound because `InstallProductionFallback()` cannot
      be JIT-compiled without loading `UtilitiesCS` into that domain, so after the call the count must
      be non-zero. The same helper therefore returns a non-zero count in the positive domain and 0 in
      `NegativeControl_HasNoUtilitiesCsAssemblyLoaded`, which is what makes the zero an observation
      rather than an artefact of a helper that always returns 0.

      `ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall` carries the same shape of positive control
      on its own counting mechanism. It reads `CountAssemblyResolveHandlers()` before
      `InstallProductionFallback()` and asserts the value is 0, then calls `InstallProductionFallback()`
      and reads `CountAssemblyResolveHandlers()` a second time and asserts the value is greater than 0.
      Without the second read the zero is not an observation: a reflected field whose value is null in
      every state returns 0 unconditionally, and the fail-loud rule recorded at `[P0-T13]` covers only
      the case where the field lookup itself returns null, not the case where the lookup succeeds and
      the value never becomes non-null. The first reading would then pass in a domain where a handler
      had in fact been installed. This second reading is why `[P2-T1]`'s behaviour-empty seam subscribes
      the handler: the reading is taken in the `[P2-T11]` run as well as the `[P4-T3]` run, and an
      `Install()` with an empty body would report an empty invocation list there and make `[P2-T12]`
      unsatisfiable.

      The class carries `[DoNotParallelize]`, for the reason given under
      `## Run Environment Constraints`: the runsettings in force set `<Scope>ClassLevel</Scope>`, so
      sibling classes in this assembly run concurrently in the host process, and `AppDomain.Unload` in
      `[TestCleanup]` is the one harness operation that concurrency reaches. Assertions in the parent
      domain use FluentAssertions over the primitive values the probe marshals back.
      `NegativeControl_Netstandard20Observation_IsRecorded` runs in the same installer-free second child
      domain, attempts the full display name at `Version=2.0.0.0`, writes the single line
      `NETSTANDARD_2_0_0_0_NEGATIVE_DOMAIN_RESULT=` followed by the marshalled outcome string through
      `TestContext.WriteLine` so the value lands in the TRX standard output, and asserts only that the
      outcome string is non-empty. The outcome value itself is an observation for the open `2.0.0.0`
      risk, not a gate; asserting a particular value would be asserting something this work does not
      know.
      Acceptance: the file exists, contains exactly those eight method names, contains `AppDomain.CreateDomain`
      and `AppDomain.Unload`, contains `TaskMaster.Test.dll.config`, and returns zero hits for
      `TaskMaster.dll.config`. The file additionally contains the literal `[DoNotParallelize]` exactly
      once, the literal `CountLoadedAssembliesNamed("SVGControl")` exactly
      once, the literal `CountLoadedAssembliesNamed("UtilitiesCS")` exactly twice, and the literal
      `CountAssemblyResolveHandlers()` exactly twice. Those four literals are quoted here in prose
      because they are absent from the tree until this task runs.
- [ ] [P2-T7] Add the in-file isolation warning required by the spec's negative-control criterion, worded
      unambiguously. Immediately above `NegativeControl_WithoutInstall_Netstandard21Throws` in
      `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`, add a comment stating that if the
      load in this negative-control domain ever succeeds without a code change — that is, if this test
      fails because no `FileNotFoundException` was raised — isolation has been lost, every positive
      assertion in this class is vacuous, and no positive result from this harness may be trusted. The
      comment's first line must carry the single-token sentinel `ISOLATION-LOST-INVARIANT` on its own, so
      the assertion below is a one-word search that no reflow of the surrounding prose can break.
      Acceptance: on `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`,
      `Select-String -SimpleMatch -CaseSensitive -Pattern "ISOLATION-LOST-INVARIANT"` returns exactly 1
      hit. The literal the task creates is `ISOLATION-LOST-INVARIANT`, quoted here in prose because it is
      absent from the tree until this task runs.
- [ ] [P2-T8] Create `TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs` declaring
      `[TestClass] public class AddInEagerInstallShapeTests` in namespace `TaskMaster.Test.Bootstrap`,
      with `[TestMethod] public void ThisAddIn_HasExplicitStaticConstructor()` asserting that
      `typeof(ThisAddIn).Attributes.HasFlag(TypeAttributes.BeforeFieldInit)` is `false` and
      `typeof(ThisAddIn).TypeInitializer` is not `null`; and
      `[TestMethod] public void AppConfig_DeclaresNetstandardRedirect()` which resolves
      `TaskMaster.dll.config` from `AppDomain.CurrentDomain.BaseDirectory`, fails loudly when the file is
      absent, parses it as XML, and asserts a `dependentAssembly` exists whose `assemblyIdentity` has
      `name` equal to `netstandard`, `publicKeyToken` equal to `cc7b13ffcd2ddd51` and `culture` equal to
      `neutral`, and whose `bindingRedirect` has `oldVersion` equal to `0.0.0.0-2.1.0.0` and `newVersion`
      equal to `2.0.0.0`. Assertions are on element and attribute values, never on a text phrase. This
      test reads the deployed image of `TaskMaster/app.config`, which is the file the CLR actually
      consults; an in-file comment says so.
      Acceptance: the file exists and contains both method names exactly.
- [ ] [P2-T9] Register the three new harness files. Insert
      `<Compile Include="Bootstrap\ChildDomainBindProbe.cs" />`,
      `<Compile Include="Bootstrap\NetstandardBindChildDomainTests.cs" />` and
      `<Compile Include="Bootstrap\AddInEagerInstallShapeTests.cs" />` into the `ItemGroup` in
      `TaskMaster.Test/TaskMaster.Test.csproj` that already contains
      `<Compile Include="Ribbon\RibbonCommandBoundaryTests.cs" />` at line 323.
      Acceptance: on `TaskMaster.Test/TaskMaster.Test.csproj`,
      `Select-String -SimpleMatch -CaseSensitive` returns exactly 1 hit for each of the three file names
      `ChildDomainBindProbe.cs`, `NetstandardBindChildDomainTests.cs` and
      `AddInEagerInstallShapeTests.cs`.
- [ ] [P2-T10] LOCK-ACQUIRE, then create the evidence/regression-testing directory this task and `[P2-T11]` redirect into,
      then rebuild the solution with the plain Debug configuration and no analyzer or nullable
      properties, then LOCK-RELEASE. This task is NOT tagged `[expect-fail]`: the seam is
      declaration-complete, so the expected outcome here is a successful build. The deliberately
      failing observations belong to `[P2-T11]`. The analyzer and nullable gates are deliberately not
      run in this phase: they would be evaluated against a deliberately behaviour-empty seam.

      ```
      pwsh -NoProfile -Command '
      $d = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing"
      if (-not (Test-Path -LiteralPath $d)) { New-Item -ItemType Directory -Path $d -Force > $null }
      Write-Output ("REGRESSION_EVIDENCE_DIR_PRESENT=" + (Test-Path -LiteralPath $d))
      '
      ```

      ```
      pwsh -NoProfile -Command '
      $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
      $msb = @(& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\amd64\MSBuild.exe")[0]
      & $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" *> "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-build-console.2026-09-13T18-22.txt"
      $LASTEXITCODE
      '
      ```

      Write `.../evidence/regression-testing/expect-fail-build.2026-09-13T18-22.md` with `Timestamp:`,
      `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 0` and `Output Summary:`.
      Acceptance: the directory-creation span records `REGRESSION_EVIDENCE_DIR_PRESENT=True`;
      `EXIT_CODE: 0`; and the console log contains at least one line matching `^\s+0 Error\(s\)$`. A
      compile failure here is a defect in the seam, not the fail-before signal this phase is looking
      for.
- [ ] [P2-T11] [expect-fail] LOCK-ACQUIRE, then run the harness class alone and capture the TRX, then
      LOCK-RELEASE.

      ```
      pwsh -NoProfile -Command '
      $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
      $vstest = @(& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe")[0]
      & $vstest "TaskMaster.Test/bin/Debug/TaskMaster.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~TaskMaster.Test.Bootstrap" /Logger:trx /ResultsDirectory:TestResults/p2-expect-fail
      $LASTEXITCODE
      '
      ```

      Then read the single TRX in `TestResults/p2-expect-fail` and record the per-test outcomes:

      ```
      pwsh -NoProfile -Command '
      $trx = @(Get-ChildItem -LiteralPath "TestResults/p2-expect-fail" -Filter "*.trx" -Recurse)[0]
      $x = [xml](Get-Content -LiteralPath $trx.FullName -Raw)
      foreach ($r in @($x.TestRun.Results.UnitTestResult)) { Write-Output ($r.testName + " OUTCOME=" + $r.outcome) }
      '
      ```

      Write `.../evidence/regression-testing/expect-fail-run.2026-09-13T18-22.md` with `Timestamp:`,
      `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 1` and an `Output Summary:` reproducing every
      `OUTCOME=` line verbatim.
      Acceptance: the artifact records `AfterInstall_BothNetstandardVersionsBind OUTCOME=Failed` and
      `AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Failed`. These are the fail-before observations:
      they fail at runtime against a behaviour-empty installer, not at compile time. The run's own exit
      code is recorded against `ExpectedExitCode: 1` and is not itself a gate.
- [ ] [P2-T12] Decisive net481 isolation check, taken before the fix exists so it cannot be confounded by
      it. Read the `[P2-T11]` artifact.
      Acceptance: it records all five of
      `ChildDomain_HasNoSvgControlAssemblyLoaded OUTCOME=Passed`,
      `ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall OUTCOME=Passed`,
      `ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect OUTCOME=Passed`,
      `NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed` and
      `NegativeControl_HasNoUtilitiesCsAssemblyLoaded OUTCOME=Passed`. Write
      `.../evidence/regression-testing/isolation-field-decisive-check.2026-09-13T18-22.md` with
      `Timestamp:`, `Command: (read of the [P2-T11] artifact)`, `EXIT_CODE: 0` and an `Output Summary:`
      reproducing those five lines plus the sentence
      `The private AppDomain assembly-resolution field is present and readable on net481; the isolation
      assertion did not skip.` If any of the five is not `Passed`, the executor halts and reports blocked:
      the harness has no isolation and no later positive result would mean anything.

### Phase 3 — Minimal Production Fix

- [ ] [P3-T1] Implement the resolution ladder in
      `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs`, replacing the behaviour-empty rungs from
      `[P2-T1]`. Order, first non-null wins: (1) an already-loaded assembly whose simple name matches
      case-insensitively and whose public key token is equal, version deliberately not compared;
      (2) `Assembly.Load` of the full display name composed of the requested simple name,
      `Culture=neutral`, the requested public key token, and `Version=2.0.0.0` when the requested simple
      name is `netstandard`; (3) for the `netstandard` identity only,
      `Assembly.LoadFrom` of the runtime directory joined with `netstandard.dll`; (4) a directory probe
      for the simple name plus `.dll` next to the executing assembly. Each rung is wrapped so an internal
      exception is absorbed, traced through `System.Diagnostics.Trace`, and the ladder continues. The
      handler returns `null` for any name it cannot resolve and never propagates an exception to the CLR
      binder. No log4net anywhere in the file: log4net inside an `AssemblyResolve` handler can itself
      re-enter assembly loading.
      Acceptance: `UtilitiesCS.Test.Bootstrap.AssemblyBindingFallbackTests` passes in `[P4-T2]`.
- [ ] [P3-T2] Implement `Install()` in the same file so that it subscribes the handler to
      `AppDomain.CurrentDomain.AssemblyResolve` exactly once per AppDomain, guarded by
      `Interlocked.Exchange` on the private counter, and so that it never throws: an exception escaping it
      becomes a `TypeInitializationException` on `ThisAddIn` and would take the whole add-in down. The
      subscription and the `Interlocked.Exchange` guard are already present from `[P2-T1]`, which is
      what makes `[P2-T6]`'s handler-count control observable in the `[P2-T11]` run; this task's
      obligation is that `Install()` attaches exactly one handler however many times it is called and
      never throws, which is measured by the two named tests rather than by inspection.
      Acceptance: the idempotence test and the never-throws test in
      `UtilitiesCS.Test.Bootstrap.AssemblyBindingFallbackTests` pass in `[P4-T2]`.
- [ ] [P3-T3] Add the eager installation point. In `TaskMaster/ThisAddIn.cs`, inside
      `public partial class ThisAddIn`, add an explicit static constructor whose body is exactly one
      statement calling the installer. The statement to add is
      `UtilitiesCS.Bootstrap.AssemblyBindingFallback.Install();` and the constructor declaration is
      `static ThisAddIn()`. Do not remove or move the `[ExcludeFromCodeCoverage]` attribute at line 18,
      and add no other member. Declaring an explicit static constructor also clears `beforefieldinit`,
      which is what makes the ordering precise rather than "at or before first use".
      Acceptance: `Select-String -SimpleMatch -Pattern "static ThisAddIn()"` on `TaskMaster/ThisAddIn.cs`
      returns exactly 1 hit, `Select-String -SimpleMatch -Pattern "AssemblyBindingFallback.Install();"`
      returns exactly 1 hit, and `Select-String -SimpleMatch -Pattern "[ExcludeFromCodeCoverage]"` returns
      at least 1 hit.
- [ ] [P3-T4] Add the declarative hardening. In `TaskMaster/app.config`, inside the existing
      `assemblyBinding` element that opens at line 27 and closes at line 468, add one new
      `dependentAssembly` block declaring `assemblyIdentity` with `name="netstandard"`,
      `publicKeyToken="cc7b13ffcd2ddd51"`, `culture="neutral"` and `bindingRedirect` with
      `oldVersion="0.0.0.0-2.1.0.0"` and `newVersion="2.0.0.0"`. Write the `assemblyIdentity` on one line,
      matching the existing single-line form used for `FSharp.Core` at line 69, whose attribute set and
      name length are identical. Change nothing else in the file; in particular do not touch the
      `FSharp.Core` redirect at lines 68-71.
      Acceptance: `Select-String -SimpleMatch` on `TaskMaster/app.config` returns exactly 1 hit for
      `name="netstandard"`, exactly 1 hit for `oldVersion="0.0.0.0-2.1.0.0"`, exactly 1 hit for
      `newVersion="2.0.0.0"`, and still exactly 1 hit for `oldVersion="0.0.0.0-11.0.0.0"`.
- [ ] [P3-T5] Record that the hardening is not the fix. Verify that `spec.md` already states the ground,
      and write `.../evidence/other/hardening-not-the-fix.2026-09-13T18-22.md` with `Timestamp:`,
      `Command:`, `EXIT_CODE:` and an `Output Summary:` naming the `spec.md` line numbers at which the
      literal `cannot manufacture an assembly` appears. Command:

      ```
      pwsh -NoProfile -Command '
      $p = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md"
      foreach ($m in @(Select-String -LiteralPath $p -SimpleMatch -Pattern "cannot manufacture an assembly")) { Write-Output ("LINE=" + $m.LineNumber) }
      '
      ```

      Acceptance: the artifact exists and records at least two `LINE=` values.

### Phase 4 — Targeted Verification of the Fix

- [ ] [P4-T1] LOCK-ACQUIRE, rebuild the solution with the plain Debug configuration, LOCK-RELEASE. Same
      pwsh shape as `[P2-T10]`, redirecting to
      `.../evidence/regression-testing/pass-after-build-console.2026-09-13T18-22.txt`.
      Acceptance: the console log contains at least one line matching `^\s+0 Error\(s\)$`.
- [ ] [P4-T2] LOCK-ACQUIRE, run the `UtilitiesCS.Test` ladder unit tests alone, LOCK-RELEASE.

      ```
      pwsh -NoProfile -Command '
      $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
      $vstest = @(& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe")[0]
      & $vstest "UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~UtilitiesCS.Test.Bootstrap" /Logger:trx /ResultsDirectory:TestResults/p4-ladder
      $LASTEXITCODE
      '
      ```

      Read the TRX in `TestResults/p4-ladder` with the same reader used in `[P2-T11]` and write
      `.../evidence/regression-testing/pass-after-ladder.2026-09-13T18-22.md` with the four required
      fields plus every `OUTCOME=` line.
      Acceptance: `EXIT_CODE: 0`, the TRX `ResultSummary` `outcome` is `Completed`, the `Counters` `failed`
      value is `0`, and the `passed` value is at least 10.
- [ ] [P4-T3] LOCK-ACQUIRE, run the child-domain harness class alone, LOCK-RELEASE. Same command shape as
      `[P2-T11]`, which means the same `/Settings:scripts/vscode/TaskMaster.cli.runsettings` operand and
      not the repository-root `TaskMaster.runsettings`, with
      `/ResultsDirectory:TestResults/p4-harness`. Read the TRX and write
      `.../evidence/regression-testing/pass-after-harness.2026-09-13T18-22.md` with the four required
      fields plus every `OUTCOME=` line.
      Acceptance: `EXIT_CODE: 0`, the `Counters` `failed` value is `0`, and the artifact records
      `OUTCOME=Passed` for all eight method names listed in `[P2-T6]`.
- [ ] [P4-T4] Verify the load-bearing negative control specifically. Read the `[P4-T3]` artifact.
      Acceptance: it records `NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed`. If it
      records any other outcome, the executor halts and reports blocked, because the load in the
      negative-control domain succeeded without a code change, which means isolation has been lost and
      every positive result in `[P4-T3]` is vacuous.
- [ ] [P4-T5] Verify the both-versions criterion specifically. Read the `[P4-T3]` artifact.
      Acceptance: it records `AfterInstall_BothNetstandardVersionsBind OUTCOME=Passed`. This is the
      criterion that forbids reporting a `2.1.0.0`-only remedy as a fix.
- [ ] [P4-T6] Verify the Deedle end-to-end criterion specifically. Read the `[P4-T3]` artifact.
      Acceptance: it records `AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Passed`. A
      `NotExecuted` or `Inconclusive` outcome is a failure of this task, not a pass: the test is required
      to fail rather than skip when `Deedle.dll` is absent from the domain's `ApplicationBase`.
- [ ] [P4-T7] LOCK-ACQUIRE, run the `AddInEagerInstallShapeTests` class alone with
      `/Settings:scripts/vscode/TaskMaster.cli.runsettings`, and not the repository-root
      `TaskMaster.runsettings`, with
      `/ResultsDirectory:TestResults/p4-shape` and `/TestCaseFilter:"FullyQualifiedName~AddInEagerInstallShapeTests"`,
      LOCK-RELEASE. Write `.../evidence/regression-testing/pass-after-shape.2026-09-13T18-22.md` with the
      four required fields plus every `OUTCOME=` line.
      Acceptance: `EXIT_CODE: 0` and the artifact records
      `ThisAddIn_HasExplicitStaticConstructor OUTCOME=Passed` and
      `AppConfig_DeclaresNetstandardRedirect OUTCOME=Passed`.
- [ ] [P4-T8] Static shape checks for the criteria a test cannot carry. Command:

      ```
      pwsh -NoProfile -Command '
      $cs = "TaskMaster/ThisAddIn.cs"
      $cfg = "TaskMaster/app.config"
      Write-Output ("STATIC_CTOR=" + @(Select-String -LiteralPath $cs -SimpleMatch -CaseSensitive -Pattern "static ThisAddIn()").Count)
      Write-Output ("INSTALL_CALL=" + @(Select-String -LiteralPath $cs -SimpleMatch -CaseSensitive -Pattern "AssemblyBindingFallback.Install();").Count)
      Write-Output ("EXCLUDE_ATTR=" + @(Select-String -LiteralPath $cs -SimpleMatch -CaseSensitive -Pattern "ExcludeFromCodeCoverage").Count)
      Write-Output ("NETSTANDARD_IDENTITY=" + @(Select-String -LiteralPath $cfg -CaseSensitive -Pattern "name=.netstandard.").Count)
      Write-Output ("OLD_VERSION_2_1=" + @(Select-String -LiteralPath $cfg -CaseSensitive -Pattern "oldVersion=.0\.0\.0\.0-2\.1\.0\.0.").Count)
      Write-Output ("NEW_VERSION_2_0=" + @(Select-String -LiteralPath $cfg -CaseSensitive -Pattern "newVersion=.2\.0\.0\.0.").Count)
      Write-Output ("FSHARP_REDIRECT=" + @(Select-String -LiteralPath $cfg -CaseSensitive -Pattern "oldVersion=.0\.0\.0\.0-11\.0\.0\.0.").Count)
      '
      ```

      The four config patterns are regular expressions in which each trailing and leading `.` matches the
      quotation mark the attribute value is wrapped in, which avoids embedding a quotation mark inside the
      pattern string. The literals the plan intends the executor to create are
      `static ThisAddIn()`, `AssemblyBindingFallback.Install();`, `name="netstandard"`,
      `oldVersion="0.0.0.0-2.1.0.0"` and `newVersion="2.0.0.0"`, quoted here in prose for that reason.
      Write `.../evidence/other/static-shape-checks.2026-09-13T18-22.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:` and an `Output Summary:` reproducing all seven `NAME=count` lines verbatim.
      Acceptance: the artifact records `STATIC_CTOR=1`, `INSTALL_CALL=1`, `EXCLUDE_ATTR` of at least 1,
      `NETSTANDARD_IDENTITY=1`, `OLD_VERSION_2_1=1`, `NEW_VERSION_2_0=1` and `FSHARP_REDIRECT=1`.
      `FSHARP_REDIRECT=1` is the unchanged-baseline control: the `FSharp.Core` redirect at
      `TaskMaster/app.config` line 70 must survive this work untouched.
- [ ] [P4-T9] Determinism and no-filesystem-write sweep over exactly the four files this work adds under
      a test project. Command:

      ```
      pwsh -NoProfile -Command '
      $paths = @("UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs","TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs","TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs","TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs")
      $banned = @("Thread.Sleep","Task.Delay","File.WriteAllText","File.WriteAllLines","File.WriteAllBytes","File.AppendAllText","File.Create","File.Delete","File.Move","File.Copy","Directory.CreateDirectory","Directory.Delete","Path.GetTempFileName","Path.GetTempPath","StreamWriter","DateTime.Now","DateTime.UtcNow")
      foreach ($p in $paths) { foreach ($b in $banned) { Write-Output ($p + " " + $b + " HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern $b).Count) } }
      foreach ($p in $paths) { Write-Output ($p + " CONTROL_AppDomain HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "AppDomain").Count) }
      '
      ```

      The trailing control lines are a positive control: they prove the search mechanism and each file
      path are live, so a zero count on the banned list is evidence rather than an artefact of a broken
      search. None of the banned tokens may appear in a comment either, because the search is textual.
      Write `.../evidence/other/scope-and-determinism-checks.2026-09-13T18-22.md` with the four required
      fields and the full output.
      Acceptance: every banned-token line ends with `HITS=0`, and every `CONTROL_AppDomain` line ends with
      a count greater than 0.
- [ ] [P4-T10] Record the free `2.0.0.0` measurement that narrows the open risk. The `[P4-T3]` TRX
      reader emits `testName` and `outcome` only, so the observation is extracted from the TRX
      directly. Command:

      ```
      pwsh -NoProfile -Command '
      $trx = @(Get-ChildItem -LiteralPath "TestResults/p4-harness" -Filter "*.trx" -Recurse)[0]
      $x = [xml](Get-Content -LiteralPath $trx.FullName -Raw)
      foreach ($r in @($x.TestRun.Results.UnitTestResult)) {
      if ($r.testName -eq "NegativeControl_Netstandard20Observation_IsRecorded") {
      foreach ($line in @(($r.Output.StdOut -split "`r?`n"))) {
      if ($line.StartsWith("NETSTANDARD_2_0_0_0_NEGATIVE_DOMAIN_RESULT=")) { Write-Output $line } } } }
      '
      ```

      Write
      `.../evidence/other/netstandard-2-0-0-0-child-domain-observation.2026-09-13T18-22.md` with
      `Timestamp:`, `Command:`, `EXIT_CODE:` and an `Output Summary:` containing exactly one line of the
      form `NETSTANDARD_2_0_0_0_NEGATIVE_DOMAIN_RESULT=` followed by the marshalled outcome string the
      probe returned, which is either `LOADED` or the exception type name.
      Acceptance: the artifact exists and carries exactly one such line with a non-empty value. The value
      itself is an observation, not a gate: either value is recorded and neither blocks.
- [ ] [P4-T11] No-new-deployment sweep. Confirm no `netstandard.dll` entered any project or any
      `packages.config`, and no `FSharp.Core` version changed. Command:

      ```
      pwsh -NoProfile -Command '
      Write-Output ("PACKAGES_CONFIG_CHANGED=" + @(git diff --name-only origin/main...HEAD -- **/packages.config).Count)
      Write-Output ("PORCELAIN_PACKAGES_CONFIG=" + @(git status --porcelain --untracked-files=all -- **/packages.config).Count)
      Write-Output ("NETSTANDARD_DLL_IN_PROJECTS=" + @(Select-String -Path "*/*.csproj" -SimpleMatch -CaseSensitive -Pattern "netstandard.dll").Count)
      Write-Output ("FSHARP_REDIRECT_LINES=" + @(Select-String -Path "*/app.config" -CaseSensitive -Pattern "oldVersion=.0\.0\.0\.0-11\.0\.0\.0.").Count)
      '
      ```

      The porcelain span is the companion the name-listing diff needs: the diff enumerates tracked changes
      only and cannot report a `packages.config` this work might have created, so the two spans together
      cover both states. The `FSHARP_REDIRECT_LINES` pattern is a regular expression whose leading and
      trailing `.` match the quotation marks; the literal is `oldVersion="0.0.0.0-11.0.0.0"`.
      Append the output to `.../evidence/other/scope-and-determinism-checks.2026-09-13T18-22.md`.
      Acceptance: `PACKAGES_CONFIG_CHANGED=0`, `PORCELAIN_PACKAGES_CONFIG=0`,
      `NETSTANDARD_DLL_IN_PROJECTS=0`, and `FSHARP_REDIRECT_LINES` exactly equal to the
      `BASELINE_FSHARP_REDIRECT_LINES` integer recorded at `[P0-T15]`. No absolute value is pinned here,
      because the planner did not measure it; the gate is that this work changes the figure by zero, and
      the baseline figure is the comparator. A `BASELINE_FSHARP_REDIRECT_LINES` value of `0` blocks: it
      would mean the search found nothing at baseline and the comparison would prove nothing.
- [ ] [P4-T12] File-size audit of every file this work creates or edits. Command:

      ```
      pwsh -NoProfile -Command '
      $paths = @("UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs","UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs","TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs","TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs","TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs","TaskMaster/ThisAddIn.cs")
      foreach ($p in $paths) { Write-Output ($p + " LINES=" + @(Get-Content -LiteralPath $p).Count) }
      '
      ```

      Write `.../evidence/other/file-size-audit.2026-09-13T18-22.md` with the four required fields and the
      output. This audit is repeated after the final format pass at `[P5-T8]`, because formatting changes
      line counts.
      Acceptance: every `LINES=` value is at most 500. If any value exceeds 500, split the offending type
      into a second file, record the split as a write-set amendment in
      `.../evidence/baseline/write-set-decision.2026-09-13T18-22.md`, and register the new file in the
      owning `csproj`.

### Phase 5 — Full Four-Step C# Toolchain Loop

Run steps 1 to 4 in this exact order. If any step fails, or changes any file, fix and restart from step 1.
Each attempt overwrites its own artifact; the committed artifact is the final, clean pass.

- [ ] [P5-T1] Re-gate Outlook closed before the loop, by the same command and rule as `[P0-T3]`. Append
      the observation to `.../evidence/qa-gates/outlook-closed-gate.2026-09-13T18-22.md` with the four
      required fields.
      Acceptance: the recorded running-process count is `0`.
- [ ] [P5-T2] Step 1, format. LOCK-ACQUIRE, then
      `pwsh -NoProfile -Command 'dotnet tool run csharpier format .'`, then LOCK-RELEASE. This is a
      write-mode command whose exit code is identical whether it rewrote files or not, so the acceptance
      condition observes the tree rather than the exit code.
      Immediately afterwards run the repository-wide porcelain span, excluding only the two path
      classes the inherited-path rule places outside every scope assertion in this plan:

      ```
      git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs/features"
      ```

      The exclusions are pathspec magic and are supported by the repository's git; `.claude` is excluded
      because agent-memory files the executor's own harness maintains are outside this write set, and
      `docs/features` is excluded because this plan's own evidence tree is written there continuously.
      A repository-wide span is required rather than the four-directory span: `csharpier format .`
      formats the whole tree, and `[P0-T5]` records the format baseline without repairing it, so any
      pre-existing drift outside those four directories is repaired by this task and would otherwise be
      reported by no span in this plan. Write `.../evidence/qa-gates/format-final.2026-09-13T18-22.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` and a `Tree Observation:` field
      carrying the porcelain output verbatim, or the literal `NONE` when it is empty.
      Acceptance: `EXIT_CODE: 0` and every path the `Tree Observation:` field lists is one of the ten
      repository-relative paths in items 1 to 10 of `## Authorised Write Set`. A formatter rewrite of
      any other tracked file means the format pass was not scoped as expected: restore that file with
      `git checkout --` against its own pathspec, record the restoration in the artifact, and restart
      the loop from step 1.
- [ ] [P5-T3] Step 1 verification, read-only.
      `pwsh -NoProfile -Command 'dotnet tool run csharpier check .'` under LOCK-ACQUIRE/LOCK-RELEASE.
      Append `Check EXIT_CODE:` and the reported unformatted-file count to the `[P5-T2]` artifact.
      Acceptance: `Check EXIT_CODE: 0`.
- [ ] [P5-T4] Step 1 side-effect check on the hand-edited config. Confirm the format pass did not rewrite
      any existing line of `TaskMaster/app.config`. Commands, in this order:

      ```
      git add -- TaskMaster/app.config
      git diff --numstat origin/main...HEAD -- TaskMaster/app.config
      git diff --numstat --cached -- TaskMaster/app.config
      ```

      The `git add` span is the companion the name-listing diff needs, and the second diff covers the
      not-yet-committed state that the merge-base diff cannot see at this point in the plan. `--numstat`
      prints added and deleted line counts as the first two tab-separated columns, which is what the
      acceptance condition reads; `--stat` prints a graph rather than two readable integers. Append both
      outputs to the `[P5-T2]` artifact under a `Config Numstat:` heading.
      Acceptance: the heading exists, and the union of the two outputs reports a deletion count of `0` for
      `TaskMaster/app.config` and an addition count between 1 and 12 inclusive. A non-zero deletion count
      means CSharpier rewrote an existing line in that file: rewrite the new block to match the existing
      attribute form used for `FSharp.Core` at line 69 and restart the loop from step 1.
- [ ] [P5-T5] Step 2, analyzers. LOCK-ACQUIRE, then the exact command, redirecting the console log to
      `.../evidence/qa-gates/analyzer-final-console.2026-09-13T18-22.txt`, then LOCK-RELEASE:

      ```
      pwsh -NoProfile -Command '
      $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
      $msb = @(& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\amd64\MSBuild.exe")[0]
      & $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true *> "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/qa-gates/analyzer-final-console.2026-09-13T18-22.txt"
      $LASTEXITCODE
      '
      ```

      Then measure the log:

      ```
      pwsh -NoProfile -Command '
      $log = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/qa-gates/analyzer-final-console.2026-09-13T18-22.txt"
      Write-Output ("ZERO_ERRORS_LINES=" + @(Select-String -LiteralPath $log -Pattern "^\s+0 Error\(s\)$").Count)
      Write-Output ("SKIPPED_CORECOMPILE=" + @(Select-String -LiteralPath $log -Pattern "Skipping target .CoreCompile.").Count)
      Write-Output ("CONTROL_BUILD_OUTPUT=" + @(Select-String -LiteralPath $log -SimpleMatch -Pattern "UtilitiesCS.dll").Count)
      '
      ```

      The `SKIPPED_CORECOMPILE` pattern is a regular expression in which each `.` matches one of the
      quotation marks MSBuild writes around the target name; the literal the message carries is
      `Skipping target "CoreCompile"`. `CONTROL_BUILD_OUTPUT` is the positive control that the log is a
      real build log and the search mechanism is live, so that a zero `SKIPPED_CORECOMPILE` count is
      evidence rather than an artefact of an empty or unreadable file. Write
      `.../evidence/qa-gates/analyzer-final.2026-09-13T18-22.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:` and an `Output Summary:` carrying the three counts.
      Acceptance: `EXIT_CODE: 0`, `ZERO_ERRORS_LINES` greater than 0, `SKIPPED_CORECOMPILE=0`, and
      `CONTROL_BUILD_OUTPUT` greater than 0.
- [ ] [P5-T6] Step 3, nullable. Identical to `[P5-T5]` with the msbuild property list replaced by
      `/p:TreatWarningsAsErrors=true` and the console log path replaced by
      `.../evidence/qa-gates/nullable-final-console.2026-09-13T18-22.txt`. Do not add
      `/p:Nullable=enable`. Write `.../evidence/qa-gates/nullable-final.2026-09-13T18-22.md` with the four
      required fields and the same three counts measured against the nullable log.
      Acceptance: `EXIT_CODE: 0`, `ZERO_ERRORS_LINES` greater than 0, `SKIPPED_CORECOMPILE=0`, and
      `CONTROL_BUILD_OUTPUT` greater than 0.
- [ ] [P5-T7] Step 4, tests with coverage. LOCK-ACQUIRE, then
      `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`,
      leaving `-CoverageOutput` at its default so the raw Cobertura document is retained, then
      LOCK-RELEASE. Write `.../evidence/qa-gates/test-final.2026-09-13T18-22.md` with `Timestamp:`,
      `Command:`, `EXIT_CODE:`, `ExpectedExitCode:` and an `Output Summary:` carrying the document-level
      `line-rate`, `lines-valid` and `lines-covered` read from `coverage/coverage.cobertura.xml`, and the
      total, failed and passed test counts read from the TRX under `coverage/test-results`.
      Acceptance: the artifact exists, the failed test count is `0`, and all six numeric values are
      present as numbers. The runner's repository-wide threshold assertion runs after the Cobertura
      document has been written, so a non-zero exit caused by that assertion is recorded against a
      matching `ExpectedExitCode:` and does not block; a failed test count above zero does block, and the
      loop restarts from step 1.
- [ ] [P5-T8] Post-format file-size audit. Repeat the `[P4-T12]` command after the final format pass and
      append the result to `.../evidence/other/file-size-audit.2026-09-13T18-22.md` under a
      `Post-Format Line Counts:` heading.
      Acceptance: the heading exists and every `LINES=` value under it is at most 500.

      In the same task, re-run the three pre-format sweeps whose acceptance conditions describe the
      terminal state rather than the Phase 4 state, because `[P5-T2]` rewrites tracked source across
      the whole tree and CSharpier 1.2.6 processes `packages.config` as well as `*.cs` and `*.xml`:
      re-run the `[P4-T8]` command and append its seven `NAME=count` lines to
      `.../evidence/other/static-shape-checks.2026-09-13T18-22.md` under a `Post-Format Shape Checks:`
      heading; re-run the `[P4-T9]` command and the `[P4-T11]` command and append both outputs to
      `.../evidence/other/scope-and-determinism-checks.2026-09-13T18-22.md` under a
      `Post-Format Sweep:` heading.
      Acceptance: the `Post-Format Line Counts:` heading exists and every `LINES=` value under it is at
      most 500; the `Post-Format Shape Checks:` heading records the same seven values `[P4-T8]`
      requires; and the `Post-Format Sweep:` heading records `PACKAGES_CONFIG_CHANGED=0`,
      `PORCELAIN_PACKAGES_CONFIG=0`, `NETSTANDARD_DLL_IN_PROJECTS=0`, a `FSHARP_REDIRECT_LINES` value
      equal to the `BASELINE_FSHARP_REDIRECT_LINES` integer recorded at `[P0-T15]`, every banned-token
      line ending `HITS=0`, and every `CONTROL_AppDomain` line ending with a count greater than 0.
- [ ] [P5-T9] Extract the per-file coverage figure for the new module. Command:

      ```
      pwsh -NoProfile -Command '
      $x = [xml](Get-Content -LiteralPath "coverage/coverage.cobertura.xml" -Raw)
      $rows = @($x.SelectNodes("//class")).Where({ $_.filename -and $_.filename.Replace("\","/").EndsWith("UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs") })
      $valid = 0
      $covered = 0
      foreach ($r in $rows) { foreach ($l in @($r.lines.line)) { $valid = $valid + 1
      if ([int]$l.hits -gt 0) { $covered = $covered + 1 } } }
      Write-Output ("ASSEMBLYBINDINGFALLBACK_CLASS_ROWS=" + $rows.Count)
      Write-Output ("ASSEMBLYBINDINGFALLBACK_LINES_VALID=" + $valid)
      Write-Output ("ASSEMBLYBINDINGFALLBACK_LINES_COVERED=" + $covered)
      '
      ```

      Aggregation is over every `class` row whose `filename` ends with the file path, because a single
      source file yields several `class` rows when it declares a nested type, and a per-row percentage
      would be the wrong denominator. Write
      `.../evidence/qa-gates/coverage-assemblybindingfallback.2026-09-13T18-22.md` with `Timestamp:`,
      `Command:`, `EXIT_CODE:` and an `Output Summary:` carrying the three counts and the computed line
      percentage.
      Acceptance: `ASSEMBLYBINDINGFALLBACK_CLASS_ROWS` is at least 1, `ASSEMBLYBINDINGFALLBACK_LINES_VALID`
      is greater than 0, and the computed percentage is at least 90. A class-row count of zero blocks: it
      means the file was not instrumented and the figure would be unmeasurable, which is not a pass.
- [ ] [P5-T10] Coverage delta and no-regression record. Write
      `.../evidence/qa-gates/coverage-delta.2026-09-13T18-22.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:` and an `Output Summary:` carrying five labelled figures: `BASELINE_LINE_RATE` and
      `BASELINE_LINES_VALID` copied from `.../evidence/baseline/test-coverage-baseline.2026-09-13T18-22.md`;
      `POST_CHANGE_LINE_RATE` and `POST_CHANGE_LINES_VALID` copied from
      `.../evidence/qa-gates/test-final.2026-09-13T18-22.md`; and `NEW_MODULE_LINE_PERCENT` copied from
      `.../evidence/qa-gates/coverage-assemblybindingfallback.2026-09-13T18-22.md`.
      Acceptance: all five figures are present as numbers, `NEW_MODULE_LINE_PERCENT` is at least 90, and
      the artifact states explicitly whether `POST_CHANGE_LINE_RATE` is greater than or equal to
      `BASELINE_LINE_RATE`. Because the two runs measure different denominators once a new file enters the
      tree, the comparability of the two document-level rates is stated rather than asserted: when
      `POST_CHANGE_LINES_VALID` differs from `BASELINE_LINES_VALID` the artifact records
      `DENOMINATORS DIFFER` and the no-regression judgment rests on `NEW_MODULE_LINE_PERCENT` and on the
      changed-line evidence in `[P5-T9]`.
      When the `[P0-T8]` artifact records `Cobertura Document State: RAW-COLLECTOR-OUTPUT`, the
      artifact additionally records `BASELINE DENOMINATOR NOT COMPARABLE` and the no-regression
      judgment rests solely on `NEW_MODULE_LINE_PERCENT` and on `[P5-T9]`. Comparing a raw
      document-level `line-rate` against a post-processed one would compare two different
      denominators.

### Phase 6 — Open Risk, Manual Gates, Commit, and Acceptance Check-Off

- [ ] [P6-T1] Commit the source and test changes together with the evidence produced so far. Commands:

      ```
      git add -- UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs UtilitiesCS/UtilitiesCS.csproj TaskMaster/ThisAddIn.cs TaskMaster/app.config UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs UtilitiesCS.Test/UtilitiesCS.Test.csproj TaskMaster.Test/Bootstrap TaskMaster.Test/TaskMaster.Test.csproj docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879
      git commit -m "fix(879): install an eager self-sufficient assembly-binding fallback in production" -- UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs UtilitiesCS/UtilitiesCS.csproj TaskMaster/ThisAddIn.cs TaskMaster/app.config UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs UtilitiesCS.Test/UtilitiesCS.Test.csproj TaskMaster.Test/Bootstrap TaskMaster.Test/TaskMaster.Test.csproj docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879
      ```

      Acceptance: `git status --porcelain --untracked-files=all -- UtilitiesCS TaskMaster UtilitiesCS.Test TaskMaster.Test`
      returns no output.
- [ ] [P6-T2] Write the explicit statement of what this work does and does not establish about the
      `netstandard 2.0.0.0` leg. Write
      `.../evidence/other/netstandard-2-0-0-0-open-risk.2026-09-13T18-22.md` with `Timestamp:` and a body
      containing, as its own paragraphs: that the maintainer's reproduced production trace shows the chain
      falling back to `netstandard, Version=2.0.0.0` and failing there as well; that **this plan cannot
      explain the `2.0.0.0` frame** and nothing in this repository accounts for it; what this work **does**
      establish, namely that after the installer runs both the `2.1.0.0` and the `2.0.0.0` full display
      names bind in a clean child `AppDomain`, evidenced by `AfterInstall_BothNetstandardVersionsBind`;
      what this work **does not** establish, namely why the default binder failed to locate the GAC copy
      of `2.0.0.0` inside the Outlook add-in AppDomain; the child-domain observation copied verbatim from
      `.../evidence/other/netstandard-2-0-0-0-child-domain-observation.2026-09-13T18-22.md`; and the
      sentence `Issue 879 must not be reported as closed on the strength of a 2.1.0.0 result alone.`
      Acceptance: the artifact exists and contains all six elements, including that final sentence
      verbatim.
- [ ] [P6-T3] Record the Fusion binding log measurement and its status. Write
      `.../evidence/other/fusion-binding-log-request.2026-09-13T18-22.md` with `Timestamp:` and these
      fields: `Procedure:` naming the registry key `HKLM\SOFTWARE\Microsoft\Fusion` and the values
      `EnableLog=1` and `ForceLog=1`, plus the reproduction steps (fresh Outlook session, no SVG-bearing
      surface opened first, click the QuickFiler ribbon button); `Executor Constraint:` stating that the
      executor performs no HKLM registry change and starts no Outlook session, so this measurement is a
      maintainer gate; `RESULT:` whose value is exactly one of `CAPTURED` with the relevant log excerpt
      beneath it, or `PENDING-MAINTAINER`; and `Blocking:` whose value is the literal
      `NO - the fix does not wait on this measurement`, with the reason that ladder rung 3 loads the
      facade from the runtime directory by absolute path and therefore bypasses GAC lookup entirely,
      making the remedy robust to whatever the log would show.
      Acceptance: the artifact exists and carries all four fields, with `RESULT:` holding one of the two
      permitted values.
- [ ] [P6-T4] Record the manual live-Outlook human gate. Write
      `.../evidence/other/manual-live-outlook-gate.2026-09-13T18-22.md` with `Timestamp:`, `Procedure:`
      (start a fresh Outlook session with the rebuilt add-in registered; open no SVG-bearing surface
      first, meaning no `MyBox` dialog, no config viewer, no folder-not-found dialog and no prior
      QuickFiler session; click the QuickFiler ribbon button), `RESULT:` whose value is exactly one of
      `DEEDLE LOADED AND DATA MODEL POPULATED`, `FAILED` with the observed exception text beneath it, or
      `PENDING-MAINTAINER`, and `Gate Type: human`.
      Acceptance: the artifact exists and carries all four fields. An unrecorded result does not discharge
      this gate.
- [ ] [P6-T5] Verify the scope boundary against the merge base and record it. Commands, in this order:

      ```
      git rev-parse --verify origin/main
      git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs/features"
      git diff --name-only origin/main...HEAD
      ```

      The porcelain span is the companion the name-listing diff needs, and its pathspec is
      repository-wide rather than scoped to the four source directories: every path this task's
      acceptance condition prohibits sits outside those four directories, so a span scoped to them
      could not report an uncommitted change to any of them. The two exclusions are the two path
      classes the inherited-path rule places outside every scope assertion in this plan. An anchored
      `git diff --name-only` enumerates tracked committed changes only, so a path this plan created is
      invisible to it until it is committed, and `[P6-T1]` has committed it. Write
      `.../evidence/other/scope-boundary-diff.2026-09-13T18-22.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:` and an `Output Summary:` reproducing the full diff list and the full porcelain
      output verbatim, empty output recorded as the literal `NONE`.
      Acceptance: `git rev-parse --verify origin/main` exits 0; the porcelain span returns no output; and
      the diff list contains none of `SVGControl/SvgAssemblyResolver.cs`, `SVGControl/SvgRenderer.cs`,
      `SVGControl/SvgAssemblyProbe.cs`, `TestSupport/TestAssemblyResolver.cs`,
      `QuickFiler.Test/SetupAssemblyInitializer.cs`, `UtilitiesCS.Test/TestAssemblyInitializer.cs`,
      `QuickFiler.Test/app.config`, `UtilitiesCS.Test/app.config`,
      `scripts/vscode/TaskMaster.cli.runsettings`, `TaskMaster.runsettings`, `coverage.config`, any path
      beginning `.github/`, any path beginning `.claude/hooks/`, any path beginning `.claude/rules/`, and
      any path ending `packages.config`.
- [ ] [P6-T6] Mark the acceptance criterion at `spec.md` line 473 as complete by changing its leading
      `- [ ] ` to `- [x] `. Change no other character on that line.
      Acceptance: `spec.md` line 473 begins with the six characters `- [x] `.
- [ ] [P6-T7] Mark the acceptance criterion at `spec.md` line 477 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 477 begins with `- [x] `.
- [ ] [P6-T8] Mark the acceptance criterion at `spec.md` line 481 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 481 begins with `- [x] `.
- [ ] [P6-T9] Mark the acceptance criterion at `spec.md` line 484 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 484 begins with `- [x] `.
- [ ] [P6-T10] Mark the acceptance criterion at `spec.md` line 491 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 491 begins with `- [x] `.
- [ ] [P6-T11] Mark the acceptance criterion at `spec.md` line 495 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 495 begins with `- [x] `.
- [ ] [P6-T12] Mark the acceptance criterion at `spec.md` line 499 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 499 begins with `- [x] `.
- [ ] [P6-T13] Mark the acceptance criterion at `spec.md` line 504 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 504 begins with `- [x] `.
- [ ] [P6-T14] Mark the acceptance criterion at `spec.md` line 509 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 509 begins with `- [x] `.
- [ ] [P6-T15] Mark the acceptance criterion at `spec.md` line 515 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 515 begins with `- [x] `.
- [ ] [P6-T16] Mark the acceptance criterion at `spec.md` line 521 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 521 begins with `- [x] `.
- [ ] [P6-T17] Mark the acceptance criterion at `spec.md` line 528 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 528 begins with `- [x] `.
- [ ] [P6-T18] Mark the acceptance criterion at `spec.md` line 534 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 534 begins with `- [x] `.
- [ ] [P6-T19] Mark the acceptance criterion at `spec.md` line 536 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 536 begins with `- [x] `.
- [ ] [P6-T20] Mark the acceptance criterion at `spec.md` line 543 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 543 begins with `- [x] `.
- [ ] [P6-T21] Mark the acceptance criterion at `spec.md` line 545 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 545 begins with `- [x] `.
- [ ] [P6-T22] Mark the acceptance criterion at `spec.md` line 547 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 547 begins with `- [x] `.
- [ ] [P6-T23] Mark the acceptance criterion at `spec.md` line 550 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 550 begins with `- [x] `.
- [ ] [P6-T24] Mark the acceptance criterion at `spec.md` line 554 as complete, as in `[P6-T6]`.
      Acceptance: `spec.md` line 554 begins with `- [x] `.
- [ ] [P6-T25] Mirror the issue update locally. Write
      `.../evidence/issue-updates/issue-879.2026-09-13T18-22.md` with `Timestamp:`, the exact text
      intended for the issue — a summary of the remedy, the evidence paths for the negative control and
      the both-versions criterion, the manual-gate status, and the `2.0.0.0` limit statement reproduced
      verbatim from `.../evidence/other/netstandard-2-0-0-0-open-risk.2026-09-13T18-22.md` including the
      sentence `Issue 879 must not be reported as closed on the strength of a 2.1.0.0 result alone.` —
      and `PostedAs: unknown` when it has not been posted.
      Acceptance: the artifact exists, carries `Timestamp:` and a `PostedAs:` field, and contains that
      sentence verbatim.
- [ ] [P6-T26] Commit the acceptance check-offs, the Phase 6 evidence, and the plan's task check-offs.
      Commands:

      ```
      git add -- docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879
      git commit -m "docs(879): record acceptance criteria, scope boundary, and open 2.0.0.0 risk" -- docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879
      ```

      Acceptance: `git status --porcelain --untracked-files=all -- UtilitiesCS TaskMaster UtilitiesCS.Test TaskMaster.Test docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879`
      returns at most one line, and if one line is returned its path is
      `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/plan.2026-09-13T18-22.md`.
- [ ] [P6-T27] Close the plan file. Mark `[P6-T26]` complete, then commit the plan file alone:

      ```
      git add -- docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/plan.2026-09-13T18-22.md
      git commit -m "docs(879): close the atomic plan checklist" -- docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/plan.2026-09-13T18-22.md
      ```

      Acceptance: the commit succeeds, and
      `git status --porcelain --untracked-files=all -- UtilitiesCS TaskMaster UtilitiesCS.Test TaskMaster.Test docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879`
      returns at most one line naming only this plan file. That single residual line, when present, is
      this task's own check-off mark and is the expected terminal state; it is not an uncommitted work
      product.

---

## Task Counts

Counted mechanically over lines matching the task prefix pattern `- [ ] [P#-T#]`:

- Phase 0: 16 tasks
- Phase 1: 4 tasks
- Phase 2: 12 tasks
- Phase 3: 5 tasks
- Phase 4: 12 tasks
- Phase 5: 10 tasks
- Phase 6: 27 tasks
- Total: 86 tasks

## Acceptance-Criteria Traceability

| AC | spec.md line | Implementation task | Test or check task | Evidence task |
|---|---|---|---|---|
| AC1 | 473 | P2-T1, P2-T2 | P2-T1, P2-T2 | P6-T6 |
| AC2 | 477 | P3-T3 | P4-T7 | P4-T7 |
| AC3 | 481 | P3-T3 | P4-T8 | P4-T8 |
| AC4 | 484 | P2-T3, P3-T1, P3-T2 | P4-T2 | P4-T2 |
| AC5 | 491 | P2-T5, P2-T6 | P4-T3 | P4-T3 |
| AC6 | 495 | P2-T6 | P2-T12, P4-T3 | P2-T12 |
| AC7 | 499 | P2-T5, P2-T6 | P2-T12, P4-T3 | P2-T12 |
| AC8 | 504 | P2-T5, P2-T6 | P2-T12, P4-T3 | P2-T12 |
| AC9 | 509 | P3-T1 | P4-T5 | P4-T3 |
| AC10 | 515 | P3-T1 | P4-T6 | P4-T3 |
| AC11 | 521 | P2-T6, P2-T7 | P2-T12, P4-T4 | P2-T12 |
| AC12 | 528 | P3-T4 | P4-T7, P4-T8 | P4-T8 |
| AC13 | 534 | P3-T5 | P3-T5 | P3-T5 |
| AC14 | 536 | P1-T4 | P6-T5 | P6-T5 |
| AC15 | 543 | P3-T4 | P4-T11 | P4-T11 |
| AC16 | 545 | P2-T3, P2-T5, P2-T6, P2-T8 | P4-T9 | P4-T9 |
| AC17 | 547 | P3-T1 | P5-T9 | P5-T9, P5-T10 |
| AC18 | 550 | P5-T2 to P5-T7 | P5-T5, P5-T6, P5-T7 | P5-T5, P5-T6, P5-T7 |
| AC19 | 554 | P6-T4 | P6-T4 | P6-T4 |

## Planner Notes

- **Spec correction recorded.** One acceptance criterion directed the coverage artifact to
  `evidence/coverage/`, which is not a canonical evidence kind. The planner changed that single directory
  reference to `evidence/qa-gates/`, changed nothing else in the criterion's text, added no criterion and
  removed none. No other non-canonical evidence directory appears in `spec.md`.
- **Validator status.** The `mcp__drm-copilot__validate_orchestration_artifacts` MCP tool is not present
  in this planner session's tool surface, so the validator gate was NOT RUN by the planner. It must be run
  before the plan is treated as approved.
- **Issue source.** The Bash tool is disabled in this session, so `gh issue view 879` could not be run.
  The material content of issue 879 and its two comments was taken from the spec's
  `### Update since issue.md was written` section, which the delegation brief authorises as a faithful
  reproduction.
