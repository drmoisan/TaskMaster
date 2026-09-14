# 2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production (Plan)

- **Issue:** #879
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-14T19-40
- **Status:** Ready for preflight (revision R7)
- **Version:** 1.3
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

**Evidence timestamp token (binding).** Every artifact filename this plan names carries the fixed token
`2026-09-13T18-22`, which is the plan's own token and is deliberately NOT the `Last Updated` value above.
Revisions R2, R3 and R4 rename no artifact. An executor that invents a new token for an artifact this
plan names has written to a path no task reads.

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
6. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (two new `Compile Include` items; the first registers item
   5 and was added by `[P2-T4]`, the second registers item 14 and is added by `[P4-T14]`. Revision R7
   raised this count from one to two. No other line of that project file is written by any task in this
   plan, and `[P4-T14]` gates the `FSharp.Core` `HintPath` at line 598 as unchanged.)
7. `TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs` (new)
8. `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` (new)
9. `TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs` (new)
10. `TaskMaster.Test/TaskMaster.Test.csproj` (three new `Compile Include` items)

Documents and evidence:

11. `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md`
12. `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/plan.2026-09-13T18-22.md`
13. Any path under
    `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/`

Added by Revision R7 (tests):

14. `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs` (new). This is a SIBLING of item
    5, not a replacement for it. Item 5 is not written by any Revision R7 task and its two standing guards
    in `[P2-T3]` are left intact. `## R7` records why a sibling file rather than an amendment of item 5.

Conditional, only if the Phase 0 build-output premise check fails and the recorded substitution is taken.
Revision R7 renumbered this entry from 14 to 15 when it appended item 14 above; its internal references to
items 7, 8, 9 and 10 are unchanged and still point at the same entries, because Revision R7 renumbered no
entry in the range 1 to 13:

15. `ToDoModel.Test/Bootstrap/ChildDomainBindProbe.cs` and
    `ToDoModel.Test/Bootstrap/NetstandardBindChildDomainTests.cs` replace items 7 and 8, and
    `ToDoModel.Test/ToDoModel.Test.csproj` joins the write set carrying those two new
    `Compile Include` items. Item 10 (`TaskMaster.Test/TaskMaster.Test.csproj`) is NOT replaced: it
    stays in the write set and gains one `Compile Include` item rather than three, because item 9
    (`TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs`) is NOT moved and stays in
    `TaskMaster.Test`. The reason item 9 stays is that
    `ToDoModel.Test/ToDoModel.Test.csproj` carries ProjectReferences to
    `ToDoModel` (line 310) and `UtilitiesCS` (line 314) only and does not reference `TaskMaster`, so
    `typeof(ThisAddIn)` would not compile there, and `TaskMaster.dll.config` is not in that project's
    build output. Revision R5 makes this substitution's domain configuration independent of the host:
    under either host the child domains' `ApplicationBase` is the `QuickFiler.Test` build output
    directory and their `ConfigurationFile` is `QuickFiler.Test.dll.config` in that directory, the probe
    assembly is loaded by absolute path with `CreateInstanceFromAndUnwrap`, and the file-name prohibition
    in `[P2-T6]` binds to `TaskMaster.dll.config` exactly as it does under the primary host. This
    substitution is a recorded amendment (task `[P0-T14]`), never a silent change. `[P0-T14]` is left
    checked by Revision R5, which moves the child domains' `ApplicationBase` and not the project that
    hosts the harness.

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

**The guarantee is made checkable by six falsifiable assertions, not by assumption:**

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
6. `ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory` — the child domain reports an
   `ApplicationBase` equal to the `QuickFiler.Test` build output directory, and that directory holds
   `Deedle.dll`, `FSharp.Core.dll` and `QuickFiler.Test.dll.config`. Revision R5 added this assertion
   because the `ApplicationBase`, and not the Deedle call shape, is what determines whether the bind under
   test is reachable at all. Without it a silent regression of the `ApplicationBase` back to the host test
   assembly's own directory would make every positive result in this harness vacuous again, which is
   exactly the failure that consumed two fail-before rounds.

**The negative control is the load-bearing criterion.** It is the only thing that distinguishes a fixed
build from an unfixed one. It has its own task with its own acceptance condition, and it is verified in
Phase 2 — before the production fix exists — where it cannot be confounded by the fix. **If the load in
the negative-control domain ever succeeds without a code change, that is, if
`NegativeControl_WithoutInstall_Netstandard21Throws` ever fails because no `FileNotFoundException` was
raised, isolation has been lost, every positive assertion in the harness is vacuous, and no positive
result from this harness may be trusted.** The test carries that statement as an in-file comment.

**Verification of the `ApplicationBase` and `ConfigurationFile` choice, checked against the tree at
revision R5.** Both child domains set `ApplicationBase` to the `QuickFiler.Test` build output directory
and `ConfigurationFile` to `QuickFiler.Test.dll.config` in that same directory. Revision R5 moved both
values off the host test assembly's own directory; `## R6` records the measurement that forced the move.
This was checked rather than inherited:

- `QuickFiler.Test/app.config` carries the `FSharp.Core` `assemblyIdentity` at line 46 and contains zero
  occurrences of the string `netstandard`, measured on the file itself. Its deployed image
  `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll.config` likewise carries the `FSharp.Core`
  `assemblyIdentity` at line 46 with the `bindingRedirect` at line 47 and zero occurrences of
  `netstandard`, measured on the deployed file the child domain actually reads. `QuickFiler.Test/app.config`
  is named in the non-goals as an unmodifiable file, so it cannot gain a `netstandard` entry during this
  work.
- `TaskMaster.dll.config`, the deployed image of `TaskMaster/app.config`, **does** gain the `netstandard`
  redirect in Phase 3. It is present in `TaskMaster.Test/bin/Debug` and absent from
  `QuickFiler.Test/bin/Debug`, both measured. Selecting it by mistake would silently void the negative
  control, so the file-name prohibition on `TaskMaster.dll.config` in `[P2-T6]` is retained unchanged
  across the re-rooting, and criterion 3 above fails loudly if the selected file carries a `netstandard`
  entry.
- The harness assembly `TaskMaster.Test.dll` is **not** present in `QuickFiler.Test/bin/Debug`, measured.
  A child domain rooted there therefore cannot resolve it by display name, which is why `[P2-T6]` loads
  the probe with `CreateInstanceFromAndUnwrap` over `typeof(ChildDomainBindProbe).Assembly.Location`
  rather than with `CreateInstanceAndUnwrap`.

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

## R4 — REVISION R2: two verified defects folded into one round

Phases 0, 1 and 2 were executed against version 1.0 of this plan, and 31 of the 32 tasks those three
phases contained in version 1.0 completed: Phase 0's 16, Phase 1's 4, and 11 of Phase 2's 12.
`[P2-T11]` did not complete, and the executor halted correctly rather than adapting. Two defects were
then verified against the tree at branch `bug/deedle-netstandard-21-bind-unsatisfiable-in-production-879`
after `origin/main` was merged. Both are defects in this plan, not in the executor's work.

### R4.1 — Defect 1: acceptance criterion AC10 was vacuous

`[P2-T5]` specified the probe member `DeedleTypeInitializerOutcome` as "obtains the type
`Deedle.Reflection` and forces its class constructor through `RuntimeHelpers.RunClassConstructor`". The
executor implemented exactly that, at `TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs` lines 146-151 in
the tree as it stood at the halt. In the `[P2-T11]` fail-before run, against a build carrying NO fix, that
probe returned the success token and `AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Passed` was
recorded at
`docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-run.2026-09-13T18-22.md`
line 31. The sibling test in the same run and the same child domain recorded
`AfterInstall_BothNetstandardVersionsBind OUTCOME=Failed` (line 22 of that artifact) with a
`FileNotFoundException` on the `2.1.0.0` identity, so the bind genuinely is unsatisfiable in that domain
and the probe simply does not reach it.

A criterion that is satisfied with no fix present measures nothing. `CLAUDE.md`'s Bugfix Workflow requires
a failing regression test first and states that the test must fail before the fix and pass after, so an
AC whose test passes on the unfixed tree violates that requirement directly.

**The equivalence that produced the defect is empirically false and must not be re-adopted.**
`Deedle.Reflection..cctor()` appears in the production chain, yet `RunClassConstructor` on that exact type
returned the success token in the child domain. Forcing a class constructor is therefore NOT equivalent to
invoking the member, in this harness. The evidenced path is the member invocation at the deepest caller
frame of the trace captured by sibling item 877 at
`docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877/evidence/regression-testing/m3-fail-before.2026-09-13T09-14.md`
lines 22-31, excerpted here with the wrapped continuation line and the two `QuickFiler.Test` frames
omitted:

```
System.TypeInitializationException: The type initializer for 'Deedle.Reflection' threw an exception.
 ---> System.TypeInitializationException: The type initializer for '<StartupCode$Deedle>.$FrameUtils' threw an exception.
 ---> System.IO.FileNotFoundException: Could not load file or assembly
      'netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
   at <StartupCode$Deedle>.$FrameUtils..cctor()
   at Deedle.Reflection..cctor()
   at Deedle.Reflection.convertRecordSequence[T](IEnumerable`1 data)
```

The member is a generic method definition with one type parameter and one `IEnumerable<T>` parameter. The
repository reaches it through `Deedle.Frame.FromRecords`, at
`UtilitiesCS/Extensions/DfDeedle.cs` lines 123 and 237 in production and at
`QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` line 86 in the sibling reproduction.

**What the revision changes.** The probe member is repointed from a class-constructor run to a closed
generic invocation of that member, its outcome contract becomes a three-class structured string, and the
`spec.md` AC10 text is rewritten by the planner to describe what is actually tested. `[P2-T11]`'s
acceptance still requires `AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Failed`, so the
demonstration against the unfixed tree is preserved rather than removed.

**Why the replacement cannot be vacuous in the other direction.** Five properties are specified in
`[P2-T5]` and each is separately checkable:

1. A member-lookup miss throws `InvalidOperationException` naming the type and the member, and that
   exception is deliberately re-thrown past the classifying catch, so a miss can never be reported as
   success.
2. The generic method definition is closed over a concrete type declared in the probe file and supplied a
   one-element `IEnumerable<T>`, so the input is a well-formed record sequence of the same shape
   production passes to `Frame.FromRecords` rather than an empty or degenerate sequence.
3. `MethodInfo.Invoke` wraps the real exception in `TargetInvocationException`, so the probe unwraps that
   wrapper before naming a failure class. Without the unwrap every failure would be reported as
   `TargetInvocationException` and the outcome would carry no information.
4. The outcome names the failure CLASS, so a `netstandard` bind failure is distinguishable from an
   unrelated functional exception. Both the class assertion and the completion assertion are made, in
   that order, so a bind failure and an unrelated throw produce different and unambiguous failure
   messages.
5. The success token is returned on the statement immediately following the invocation and on no other
   path, so it cannot be returned when the invocation did not execute.

**The existing `[P2-T11]` artifact is superseded, not deleted.** It currently records
`Acceptance Condition: NOT MET`. `[P1-T5]` copies it to a superseded-named artifact with a header stating
why, before `[P2-T11]` overwrites the plan-named path on its re-run. Both the original observation and
the re-run therefore remain auditable.

### R4.2 — Defect 2: a pinned literal forced a signature that contradicts the contract

`[P2-T1]` pinned the literal `internal static Assembly Resolve(` as an acceptance condition, which pins a
non-nullable return on a method whose contract is to return null for every name it cannot resolve.
`UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` carries `#nullable enable` at line 1, so the executor
satisfied the literal with `return null!;` at lines 103, 109 and 114 and
`return CreateProductionLadder().Resolve(requested)!;` at line 120. That is null-forgiving on the dominant
path, not a boundary suppression.

The inner `AssemblyBindingLadder` in the same file already has the correct shape:
`internal Assembly? Resolve(AssemblyName requested)` at line 223 and `private Assembly? From...` at lines
242, 254, 266 and 278, each returning a plain `null`. Only the two outer static members deviate. AC1 at
`spec.md` lines 473-476 requires "a public `Install()` and an internal `Resolve` seam" and pins no return
type, and `spec.md` line 315 likewise names the seam without a return type, so this correction needs no
spec amendment.

**Resolution of the `OnAssemblyResolve` question, with the rationale stated rather than assumed.**
`Resolve` becomes `Assembly?`. `OnAssemblyResolve` does NOT. The two members occupy different roles:
`Resolve` is the internal seam the unit tests drive and the ladder feeds, where null is the ordinary
"not resolved" result on the dominant path; `OnAssemblyResolve` is the genuine `ResolveEventHandler`
boundary, and on net48 that delegate is declared in reference assemblies that carry no nullable
annotations, so its return type is oblivious. Keeping the handler's declared return type `Assembly` with
three `null!` returns is therefore a boundary suppression against an un-annotated framework contract,
which is the case the rule carves out, rather than a suppression on a dominant path.

Whether the conversion is warning-free under `[P5-T6]`'s `/p:TreatWarningsAsErrors=true` gate is settled
by a measurement already on disk rather than by assumption. `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs`
carries no `#nullable enable` directive and already assigns the ladder's `Assembly?`-returning `Resolve`
to a plain `Assembly` local at lines 57, 81, 111, 131, 149, 169 and 256, and the nullable baseline at
`.../evidence/baseline/nullable-baseline.2026-09-13T18-22.md` records `EXIT_CODE: 0` with
`0 Warning(s)`. A nullable-oblivious file therefore takes an `Assembly?` result into an `Assembly` local
with no diagnostic in this solution today, which is exactly what line 187 of that test file will do once
`AssemblyBindingFallback.Resolve` becomes `Assembly?`. The nullable conversion requires no test file
change; `[P2-T3]` amends that same file for an unrelated reason, adding an eleventh test method and
rewriting the class-level comment, and neither edit introduces a nullable diagnostic in a file that
carries no `#nullable enable` directive. The eight test-file line numbers this paragraph cites — 57, 81,
111, 131, 149, 169, 187 and 256 — describe the file as it stands at branch head `260005b6a`, before
`[P2-T3]` runs, and `[P2-T3]` shifts them. They are recorded as the measured grounds for the
nullable-obliviousness argument and no acceptance condition in this plan keys on them, so the shift
invalidates no gate.

One site inside the production file does change with it: line 158 declares
`Assembly resolved = Resolve(new AssemblyName(args.Name));` inside the nullable-ENABLED region, which
would become CS8600 once `Resolve` returns `Assembly?`, and `[P5-T6]` promotes that warning to a build
error. The declaration becomes `Assembly? resolved`. The `return resolved;` at line 165 stays warning-free
because the preceding `if (resolved is null)` return makes the flow state not-null at that point.

### R4.3 — Nullable-annotation constraint on the two test-project files

`TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs` and
`TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` carry NO `#nullable enable` directive, and
this revision adds none. Any `?` annotation on a reference type written into either file would raise
CS8632, which `[P5-T6]` promotes to a build error. Every member this revision specifies in those two
files is therefore written without nullable reference annotations. This constraint is stated here once and
is binding on `[P2-T5]` and `[P2-T6]`.

### R4.4 — Checklist state changed by this revision

Unchecked and to be re-executed, with the reason for each:

- `[P2-T1]` — the seam signature is amended (Defect 2). The file exists; the task is now an amendment of
  four named sites in it.
- `[P2-T5]` — the probe surface is repointed (Defect 1). The file exists; the task is now an amendment of
  one member plus one new nested record type.
- `[P2-T6]` — the harness test that consumes the repointed member is amended (Defect 1).
- `[P2-T10]` — the targeted rebuild must be re-run, because `[P2-T1]`, `[P2-T5]` and `[P2-T6]` all change
  compiled source after the recorded build.
- `[P2-T12]` — it reads the `[P2-T11]` artifact, which is regenerated, so its five isolation outcomes must
  be re-confirmed against the regenerated artifact rather than against the superseded one.

Left checked and NOT re-executed, because both defects leave them untouched: `[P0-T1]` through
`[P0-T16]`, `[P1-T1]` through `[P1-T4]`, `[P2-T2]`, `[P2-T4]`, `[P2-T7]`, `[P2-T8]` and
`[P2-T9]`. `[P2-T11]` was already unchecked and stays unchecked. `[P2-T3]` was also left checked by
Revision R2 on the same ground and was subsequently unchecked by Revision R3 for an unrelated reason
recorded in `## R5`.

`[P2-T4]`'s cited anchor line is corrected from 193 to 194 without unchecking the task: the merge of
`origin/main` at `a49c9729e` moved `<Compile Include="Extensions\DfDeedle_Tests.cs" />` in
`UtilitiesCS.Test/UtilitiesCS.Test.csproj` by one line, and the task's own product,
`<Compile Include="Bootstrap\AssemblyBindingFallbackTests.cs" />`, is present at line 190. The work is
done; only the citation was stale.

### R4.5 — Evidence artifacts this revision does not touch

`[P0-T6]` and `[P0-T7]` committed two console logs,
`.../evidence/baseline/analyzer-baseline-console.2026-09-13T18-22.txt` and
`.../evidence/baseline/nullable-baseline-console.2026-09-13T18-22.txt`. Neither defect touches those two
tasks and this revision does not re-run them, so both files are left exactly as committed and no task in
this revision rewrites, projects or deletes them. **Superseded by Revision R5**, which adds `[P5-T11]` and
`[P5-T12]`: both files are projected and then removed, for the reason `## R6.5` records. Neither task
re-runs `[P0-T6]` or `[P0-T7]`, and both tasks remain checked.

### R4.6 — Constraints re-checked against the post-merge tree and deliberately left unchanged

- **`-CoverageOutput` stays at its default** in `[P0-T8]` and `[P5-T7]`. Issue #873's
  `Test-RawCoverageDocumentRetained` deletes the raw Cobertura document unless its parent directory is
  exactly the repository `coverage` directory, by equality and not containment. No task in this revision
  changes that parameter or introduces a subdirectory under it.
- **Issue #891 is named rather than worked around.** `Assert-CoberturaLineCoverageThreshold`, invoked at
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 386, throws unless the DOCUMENT-LEVEL Cobertura
  line-rate clears a hard-coded threshold, so a `-SearchRoot`-scoped invocation of that runner cannot exit
  0 however well its tests do. That is issue #891, it is not this item's defect and this plan does not fix
  it. Both runner invocations in this plan pass `-SearchRoot .`, which is the solution-wide scope, and
  both already record a non-zero exit against a matching `ExpectedExitCode:` rather than treating it as a
  failure. No task added by Revision R2 invokes that runner at all: `[P2-T11]`, `[P4-T3]`, `[P4-T6]` and
  `[P4-T7]` call `vstest.console.exe` directly, which applies no coverage threshold.
- **No `artifacts/csharp/coverage.xml` is created.** A repository hook activates an 85 percent floor only
  when that file exists, and repository-wide raw coverage is far below it, so creating the file would
  manufacture a failure. No task in this plan names that path. The governing thresholds remain `CLAUDE.md`'s:
  line 80 percent on the testable denominator, new code 90 percent, no regression on changed lines.
- **Every diff anchor is `origin/main`.** No task uses local `main` or `git merge-base HEAD main`. Local
  `main` in a worktree-per-item run is stale, and Revision R2 introduces no new diff anchor of any kind.
- **One Phase 0 figure is now stale and is deliberately not re-run.** `[P0-T16]` recorded the line counts
  of the five files this plan edits in place, before `origin/main` was merged. The merge changed at least
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, whose `<Compile Include="Extensions\DfDeedle_Tests.cs" />`
  anchor moved from line 193 to line 194. No acceptance condition in this plan compares a later count
  against the `[P0-T16]` figures — `[P4-T12]` and `[P5-T8]` each assert an absolute ceiling of 500 lines
  and nothing else — so no gate is affected and the task is left checked rather than re-run.

---

## R5 — REVISION R3: four blocking and three minor deltas, no new task and no renumbering

Revision R3 edits task bodies only. It adds no task, removes no task, and renumbers nothing, so every
`[P#-T#]` identifier this plan and its evidence artifacts cite is unchanged. It unchecks exactly one
task, `[P2-T3]`.

### R5.1 — TRX selection is pinned by name (blocking)

Four read-backs selected their TRX with `-Filter "*.trx"` followed by `[0]`. `Get-ChildItem` returns
results in name-ascending order rather than write-time order, and `TestResults/p2-expect-fail` already
holds `DanMoisan_MEGALODON4_2026-09-13_23_35_52_net481.trx` from the superseded version 1.0 run — the
run that recorded the vacuous `AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Passed` and that
carries no `DEEDLE_RECORD_CONVERSION_OUTCOME=` line. A TRX written on 2026-09-14 sorts after that name,
so the unpinned selection would have read the superseded run and reported a false negative
indistinguishable from a genuine one. Each producing run now passes
`"/Logger:trx;LogFileName=<fixed name>.trx"` and each reader filters on that fixed name and emits
`TRX_MATCH_COUNT=`, which its acceptance gates at `1`. The four producing tasks are `[P2-T11]`
(`p2-expect-fail.trx`), `[P4-T2]` (`p4-ladder.trx`), `[P4-T3]` (`p4-harness.trx`) and `[P4-T7]`
(`p4-shape.trx`); the two additional readers of the `[P4-T3]` TRX, `[P4-T6]` and `[P4-T10]`, are
repointed to `p4-harness.trx`.

This treatment is deliberately NOT applied to the `[P0-T8]` and `[P5-T7]` runner reads.
`Invoke-MSTestWithCoverage.ps1` writes a single fixed-name `mstest-coverage-run.trx` under
`coverage/test-results/` and overwrites it, so those reads are already deterministic and pinning them
would add a literal the runner does not control.

### R5.2 — `[P5-T7]` is gated against the recorded baseline failures, not against zero (blocking)

`[P0-T8]` recorded `test failed = 2`. Both failures are declared at
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` lines 204 and 237, a file no task
in this plan writes and which `[P6-T1]`'s pathspec does not name, so they survive this work unrepaired.
A gate demanding zero failures would have been unsatisfiable and its stated remedy, restarting the
toolchain loop, would not have terminated. `[P5-T7]` now gates on the failing-name list being a SUBSET
of those two, so a third failure blocks and an intermittent pass of either does not.

### R5.3 — `[P4-T7]` states its command (blocking)

`[P4-T7]` named a results directory and a test-case filter but no assembly operand, no `/InIsolation`
and no logger, while its acceptance demanded `OUTCOME=` lines that only a TRX can supply. The task now
names the full command shape it inherits from `[P2-T11]` and the one operand that differs.

### R5.4 — `[P6-T24]` is conditional on the human gate (blocking)

`spec.md` lines 554-556 state that the live-Outlook gate is a human gate and that an unrecorded result
does not discharge it, and `[P6-T4]` permits `RESULT: PENDING-MAINTAINER`. `[P6-T24]` nevertheless
marked AC19 delivered unconditionally, which would have made the plan's own artifact assert something
false. It now branches on the recorded `RESULT:` value and, on the withholding branch, appends a
`Check-Off Withheld:` field instead of checking the criterion off.

### R5.5 — Minor deltas

- `[P5-T10]` additionally records `BASELINE MEASURED ON A DIFFERENT BASE`. The `[P0-T8]` baseline carries
  `Timestamp: 2026-09-13T23-15` and predates the merge of `origin/main` at `a49c9729e` into this branch
  at `f02cee3fe`, so the two document-level rates measure different code bases as well as different
  denominators. The baseline is not re-captured, because it already records
  `Cobertura Document State: RAW-COLLECTOR-OUTPUT` and the no-regression judgment therefore already
  rests on `NEW_MODULE_LINE_PERCENT` and `[P5-T9]`.
- `[P2-T5]` item 2 no longer claims the probe record mirrors production's shape. `EmailRecord` at
  `UtilitiesCS/Extensions/DfDeedle.cs` line 266 is a `private struct` exposing six public fields at
  lines 290-295, whereas the probe type is a sealed class exposing auto-properties. The design is
  unchanged — preflight invoked `Deedle.Reflection.convertRecordSequence` closed over a property-only
  type without exception — but the justification is now accurate.
- `[P6-T5]` records that `origin/main` is its anchor and why, rather than `spec.md` line 536 being
  re-worded. AC14's text is left alone because every `[P6-T#]` check-off addresses `spec.md` by line
  number and a re-wrap at line 536 would move the five criteria that follow it.

### R5.6 — `[P2-T3]` gains an eleventh test and is unchecked (coverage-floor risk closed)

`[P5-T9]` asserts a 90 percent line floor on `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs`. None of
the ten tests the version 1.0 execution wrote reaches `OnAssemblyResolve` at lines 149-174 or
`CreateProductionLadder` at lines 134-143, because both are reachable only through a real failed bind
raising `AppDomain.AssemblyResolve`; the ten drive `AssemblyBindingLadder` through injected delegates.
An eleventh test is added rather than leaving the risk open, because a Phase 5 coverage failure would
cost a full round plus a rebuild, whereas Phase 2 is already being re-executed for Revision R2.
`[P2-T3]` is therefore unchecked, its acceptance count rises from ten to eleven `[TestMethod]`
occurrences — the file carries exactly ten today, which keeps the count discriminating — and `[P4-T2]`'s
passed floor rises from 10 to 11. `spec.md` AC4 at lines 484-490 is NOT amended: AC4 requires that its
enumerated scenarios be covered as separately named test methods, and an additional test method beyond
that enumeration does not falsify it. Leaving AC4 alone also preserves its seven-line block and every
subsequent `spec.md` line number this plan cites.

---

## R6 — REVISION R5: the `ApplicationBase` is the discriminator, not the Deedle call shape

Revision R2 repointed the probe from a forced class-constructor run to a member invocation, and `[P2-T11]`
was re-run. It recorded `DEEDLE_RECORD_CONVERSION_OUTCOME=INVOKED-NO-EXCEPTION` at
`.../evidence/regression-testing/expect-fail-run.2026-09-13T18-22.md` line 54, against a build carrying no
fix. That is the same vacuity the original probe had, on a different member. A bounded investigation then
established the cause by measurement rather than by reasoning, and the cause is not the member.

### R6.1 — What was measured

`packages/FSharp.Core.11.0.100` ships two binaries and this repository's six `HintPath` values are split
between them. Every line below was re-derived against the tree in this worktree:

| Project | `HintPath` line | Flavour | `netstandard` reference |
|---|---|---|---|
| `QuickFiler/QuickFiler.csproj` | 52 | `lib/netstandard2.1` | **2.1.0.0** |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 259 | `lib/netstandard2.1` | **2.1.0.0** |
| `ToDoModel/ToDoModel.csproj` | 42 | `lib/netstandard2.1` | **2.1.0.0** |
| `UtilitiesCS/UtilitiesCS.csproj` | 70 | `lib/netstandard2.0` | 2.0.0.0 |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | 598 | `lib/netstandard2.0` | 2.0.0.0 |
| `ToDoModel.Test/ToDoModel.Test.csproj` | 96 | `lib/netstandard2.0` | 2.0.0.0 |

`Deedle.dll` 3.0.0.0 references `netstandard 2.0.0.0` and `FSharp.Core 4.5.0.0`, so **Deedle is never the
source of the `2.1.0.0` request; `FSharp.Core` is.** The same conclusion is stated independently, and was
reached independently by sibling item 877, in the XML documentation of
`TestSupport/TestAssemblyResolver.cs` lines 20 to 27: "The requirement enters the closure through that
FSharp.Core redirect and NOT through Deedle, which asks only for netstandard 2.0.0.0."

`TaskMaster.Test/bin/Debug` received the `netstandard2.0` flavour, so nothing rooted there ever requests
`netstandard 2.1.0.0` on its own and every Deedle probe rooted there succeeds. With one fresh child domain
per observation and no fix installed, the measured outcomes were:

| Observation | rooted at `TaskMaster.Test/bin/Debug` | rooted at `QuickFiler.Test/bin/Debug` |
|---|---|---|
| `Frame.FromRecords<struct with public fields>` | `INVOKED-NO-EXCEPTION` | **`netstandard 2.1.0.0` chain** |
| `Frame.FromRecords<class with auto-properties>` | `INVOKED-NO-EXCEPTION` | **`netstandard 2.1.0.0` chain** |
| `convertRecordSequence<either shape>` | `INVOKED-NO-EXCEPTION` | **`netstandard 2.1.0.0` chain** |
| `RunClassConstructor(Deedle.Reflection)` | `OK` | **`netstandard 2.1.0.0` chain** |
| `Assembly.Load` `netstandard 2.0.0.0` | `LOADED` from the GAC | `LOADED` from the GAC |
| `Assembly.Load` `netstandard 2.1.0.0` | `FileNotFoundException` | `FileNotFoundException` |

The record shape is irrelevant and all three probe designs were correct. They were pointed at the wrong
directory. In the failing domain the exception chain is byte-for-byte the reported production one, ending
`FileNotFoundException [netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51]`.
The pass-after control was measured as well: in a `QuickFiler.Test/bin/Debug`-rooted domain, after a
handler returning the `2.0.0.0` facade for any `netstandard` request is subscribed, the same call returns
`INVOKED-NO-EXCEPTION`. That is a genuine fail-before / pass-after pair.

### R6.2 — Decision: every child domain re-roots, not only the Deedle one

**Decision: all child domains created by `NetstandardBindChildDomainTests` are rooted at the
`QuickFiler.Test` build output directory.** The alternative considered was re-rooting only the domain the
Deedle observation runs in and leaving the other observations on the host test assembly's own directory.
It was rejected on the following grounds, each checked rather than assumed.

- **A split root breaks the harness's own design argument.** `## R1` requires
  `ChildDomain_HasNoSvgControlAssemblyLoaded` and `ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall`
  to observe the domain **in the state in which the bind is measured**; that is why
  `ChildDomain_HasNoSvgControlAssemblyLoaded` calls `InstallProductionFallback()` before it counts. Under
  a split root those two observations would describe a differently configured domain from the one the
  Deedle result comes from, and criterion 3, which asserts over "the configuration file supplied to both
  child domains", would have two different files to assert over. The isolation evidence would stop being
  evidence about the positive domain.
- **The other direction is not a loss.** `AfterInstall_BothNetstandardVersionsBind` currently fails in the
  host-rooted domain only because it issues an explicit `Assembly.Load` of the `2.1.0.0` identity, a
  request nothing in that directory makes on its own. The measurement above shows that identity raises
  `FileNotFoundException` in **both** directories, so re-rooting preserves that criterion's fail-before
  exactly and additionally makes the request one the directory's own closure would raise.

The three risks named against re-rooting were measured:

1. **`SVGControl`.** `SVGControl.dll` **is** present in `QuickFiler.Test/bin/Debug`. It is **equally**
   present in `TaskMaster.Test/bin/Debug`, so re-rooting changes that exposure by nothing.
   `ChildDomain_HasNoSvgControlAssemblyLoaded` asserts that no assembly named `SVGControl` is **loaded**,
   not that the file is absent. `UtilitiesCS/UtilitiesCS.csproj` line 1126 carries a `ProjectReference` to
   `SVGControl`, so `UtilitiesCS.dll` references it in both directories alike; a CLR assembly reference is
   resolved on first use of a type from it, and `AssemblyBindingFallback.Install()` uses no `SVGControl`
   type. The empirical demonstration is already on disk: the `[P2-T11]` run recorded
   `ChildDomain_HasNoSvgControlAssemblyLoaded OUTCOME=Passed` in a domain whose directory contained that
   same file. The assertion remains the falsifiable check and is not weakened.
2. **`NegativeControl_HasNoUtilitiesCsAssemblyLoaded`.** `UtilitiesCS.dll` is present in both directories,
   again identically, and the property depends on `InstallProductionFallback()` never being JIT-compiled
   in that domain rather than on the directory. One new consideration arises: `TaskMaster.Test.dll` is
   **not** present in `QuickFiler.Test/bin/Debug`, so the probe assembly is loaded into the child by
   absolute path. Loading `TaskMaster.Test` does not load `UtilitiesCS`, because assembly references
   resolve lazily on first use. The control therefore still holds and remains falsifiable.
3. **The item 877 resolver.** `TestSupport/TestAssemblyResolver.cs` is `Compile`-linked into
   `QuickFiler.Test/QuickFiler.Test.csproj` at line 234 and installed from
   `QuickFiler.Test/SetupAssemblyInitializer.cs` line 20 inside the `[AssemblyInitialize]` at line 14.
   Two independent measured reasons put it out of reach of the probe domain. First, `Install()` at
   `TestSupport/TestAssemblyResolver.cs` line 45 subscribes to `AppDomain.CurrentDomain.AssemblyResolve`,
   which is the **current domain's** event; the "process-wide" wording in that file's XML summary at line 8
   describes intent and not mechanism, and an `AssemblyResolve` subscription does not cross an `AppDomain`
   boundary. Second, the only assembly operand in `[P2-T11]`, `[P4-T3]` and `[P4-T7]` is
   `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`, so `QuickFiler.Test`'s `[AssemblyInitialize]` does not
   run in the host process at all, and nothing in the child requests `QuickFiler.Test.dll`. Re-rooting puts
   that assembly on the child's probing path without anything asking for it. The harness already carries
   the falsifiable check for exactly this: `ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall` asserts
   the assembly-resolution invocation list is empty **before** the installer runs, so a resolver active in
   the probe domain would fail that assertion rather than silently masking the bind.

### R6.3 — Mechanical consequences

- `ApplicationBase` is `Path.GetFullPath` of the host base directory joined with `..`, `..`, `..`,
  `QuickFiler.Test`, `bin`, `Debug`. The host base directory is `<repo>\TaskMaster.Test\bin\Debug\` and
  both projects declare `<OutputPath>bin\Debug\</OutputPath>`, measured at
  `QuickFiler.Test/QuickFiler.Test.csproj` line 36, so three parent steps reach the repository root. The
  derivation is a fixed expression in the test file rather than a value the executor selects.
- `ConfigurationFile` is `QuickFiler.Test.dll.config` in that directory.
- The probe is created with `CreateInstanceFromAndUnwrap` over
  `typeof(ChildDomainBindProbe).Assembly.Location`, because `TaskMaster.Test.dll` is absent from the new
  `ApplicationBase` and a display-name creation would raise `FileNotFoundException`.
- The probe's Deedle surface is repointed from `Deedle.Reflection.convertRecordSequence` to
  `Deedle.Frame.FromRecords`, which is production's actual entry point at
  `UtilitiesCS/Extensions/DfDeedle.cs` lines 123 and 237, both re-read in this pass and both reading
  `var df = Frame.FromRecords(records);`. `Deedle.Frame` also carries an overload taking two generic
  arguments, so a name-only `Type.GetMethod` lookup would raise `AmbiguousMatchException`; `[P2-T5]`
  therefore selects the overload by shape and `[P1-T6]` is re-pointed to measure that overload set from
  metadata before `[P2-T5]` is authored against it.
- A ninth harness test, `ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory`, makes the re-rooting
  itself falsifiable. It is criterion 6 in `## R1`.
- **The build-output premise moves with the root, and is re-established rather than left stale.**
  `[P0-T10]` recorded the existence of `Deedle.dll`, `TaskMaster.Test.dll.config` and
  `TaskMaster.dll.config` under `TaskMaster.Test/bin/Debug`, and `[P0-T14]` selected
  `HOST=TaskMaster.Test` from that record. Both remain checked and both remain true: Revision R5 moves the
  child domains' `ApplicationBase`, not the project that hosts the harness, and `[P2-T8]`'s
  `AppConfig_DeclaresNetstandardRedirect` still resolves `TaskMaster.dll.config` from the PARENT domain's
  base directory, which is still `TaskMaster.Test/bin/Debug`. The premise the re-rooted child domains need
  is a different one, and it is established twice rather than assumed: `[P1-T6]` gates
  `DEEDLE_DLL_PRESENT=True` on `QuickFiler.Test/bin/Debug/Deedle.dll`, and `[P2-T6]`'s fail-loud
  precondition helper throws `InvalidOperationException` at run time if that directory, its
  `QuickFiler.Test.dll.config`, its `Deedle.dll` or its `FSharp.Core.dll` is missing.

### R6.4 — OPEN RISK: which `FSharp.Core` flavour reaches `TaskMaster/bin/Debug` is nondeterministic

The root-cause fix is to align all six `FSharp.Core` `HintPath` values on `lib/netstandard2.0`. **That is
out of scope for this item and is tracked as a separate issue.** `QuickFiler/QuickFiler.csproj`,
`QuickFiler.Test/QuickFiler.Test.csproj` and `ToDoModel/ToDoModel.csproj` are outside this plan's
authorised write set and may be owned by sibling items; under the hook-satisfaction rule, editing them is a
stop-and-report condition and not a judgement call. **No task in this plan changes any `.csproj`
`HintPath`.**

The risk this leaves open, named rather than worked around:

- **The nondeterminism.** Two projects in the add-in's reference closure resolve `FSharp.Core` to different
  files of the same assembly identity. Which copy reaches `TaskMaster/bin/Debug` is MSB3277-class
  last-writer-wins behaviour across a parallel build, not a declared outcome. The reported production trace
  proves the add-in did fail, so `TaskMaster/bin/Debug` carried the `netstandard2.1` flavour at report time;
  it carries the `netstandard2.0` flavour now. Either flavour can be deployed by any subsequent rebuild.
- **The remedy does not depend on which flavour is currently deployed.** `AssemblyBindingFallback` resolves
  any `netstandard` request by simple name plus public key token and, at rung 3, loads the facade from
  `RuntimeEnvironment.GetRuntimeDirectory()` by absolute path. The `TaskMaster/app.config` redirect covers
  `0.0.0.0-2.1.0.0` declaratively. Together they make the add-in robust under either deployment. That is
  defence in depth against a nondeterministic input, not a workaround for a defect this item declines to
  fix.
- **What this item therefore does and does not close.** It closes the add-in's exposure to an unsatisfiable
  `netstandard` bind. It does not close the `HintPath` split that produces the exposure, and issue 879 must
  not be reported as having fixed that split.

### R6.5 — Evidence hygiene: console dumps are replaced by projections

`.../evidence/regression-testing/expect-fail-build-console.2026-09-13T18-22.txt` is 11,961 lines and 7,747
of those lines contain the absolute worktree path including the host user name, both counted in this pass.
The two Phase 0 console logs are 5,030 lines
(`.../evidence/baseline/analyzer-baseline-console.2026-09-13T18-22.txt`) and 11,842 lines
(`.../evidence/baseline/nullable-baseline-console.2026-09-13T18-22.txt`); both were already redacted to
`<repo-root>` for the repository path and carry zero occurrences of the host user name, but each retains
about 140 lines carrying absolute toolchain paths beginning `C:\Program Files`. The standing directive and
the issue 671 decision require projections, and require artifacts to carry no absolute host paths.

`[P5-T11]` writes one projection per console log and `[P5-T12]` deletes the raw logs, after every gate that
reads a raw log has run. No raw `.trx` and no raw `.cobertura.xml` is added to git by any task in this plan.

### R6.6 — Checklist state changed by this revision

Unchecked and to be re-executed, with the reason for each:

- `[P1-T6]` — its measurement is repointed from `Deedle.Reflection.convertRecordSequence` in
  `TaskMaster.Test/bin/Debug/Deedle.dll` to the `Deedle.Frame.FromRecords` overload set in
  `QuickFiler.Test/bin/Debug/Deedle.dll`. Its recorded artifact measures a member `[P2-T5]` no longer uses.
- `[P1-T7]` — it verifies the AC10 rewrite by token search, and AC10 is rewritten again with different
  tokens, so its gates must be re-derived against the new text.
- `[P2-T5]` — the probe surface gains `ApplicationBaseDirectory()` and its Deedle member is repointed.
- `[P2-T6]` — the domain configuration, the instantiation call and the test list all change.
- `[P2-T10]` — the targeted rebuild must be re-run, because `[P2-T5]` and `[P2-T6]` change compiled source
  after the recorded build.
- `[P2-T12]` — it reads the `[P2-T11]` artifact, which is regenerated, and it gains a sixth outcome.

`[P2-T11]` was already unchecked and stays unchecked.

Left checked and NOT re-executed, because this revision leaves them untouched: `[P0-T1]` through
`[P0-T16]`, `[P1-T1]` through `[P1-T5]`, `[P2-T1]`, `[P2-T2]`, `[P2-T3]`, `[P2-T4]`, `[P2-T7]`, `[P2-T8]`
and `[P2-T9]`. `[P2-T1]` is untouched because Revision R5 changes nothing in
`UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs`; the seam signature correction it carries from
Revision R2 is already applied. `[P2-T7]`'s `ISOLATION-LOST-INVARIANT` comment survives the `[P2-T6]`
amendment and `[P2-T6]`'s acceptance re-asserts its count.

### R6.7 — Constraints re-checked and deliberately left unchanged

- `-CoverageOutput` stays at its default in `[P0-T8]` and `[P5-T7]`, for the issue 873 reason recorded at
  `## R4.6`. Revision R5 changes neither task.
- Every diff anchor remains `origin/main`. Revision R5 introduces no new diff anchor.
- No `artifacts/csharp/coverage.xml` is created, and no task in this plan names that path.
- Every `vstest.console.exe` span keeps its explicit `/ResultsDirectory:`, its pinned `LogFileName=` inside
  a double-quoted `"/Logger:trx;LogFileName=..."`, its `TRX_MATCH_COUNT=1` gate and its `/InIsolation`.
  Revision R5 changes no vstest command line: the re-rooting is inside the test source, not in the runner
  invocation.
- The live-Outlook step at `[P6-T4]` remains a manual stop-and-report gate with a `PENDING-MAINTAINER`
  branch.

---

## R7 — REVISION R7: the new module misses AC17's own 90 percent floor, and how that is closed

`[P5-T9]` ran the command this plan pins against the post-processed `coverage/coverage.cobertura.xml` and
recorded `ASSEMBLYBINDINGFALLBACK_CLASS_ROWS=1`, `ASSEMBLYBINDINGFALLBACK_LINES_VALID=201`,
`ASSEMBLYBINDINGFALLBACK_LINES_COVERED=166`, a computed 82.59 percent against AC17's floor of 90. The task
is correctly left unchecked and the verdict is BLOCKED. AC17 at `spec.md` line 547 states the floor, so the
plan must meet it; **no task in this revision amends any acceptance-criterion text and `spec.md` is not
written by this revision.**

### R7.1 — The uncovered set, re-derived from the Cobertura document rather than carried forward

Every line below was re-derived in this pass from `coverage/coverage.cobertura.xml`, whose single `class`
row for `UtilitiesCS.Bootstrap.AssemblyBindingFallback` begins at document line 112946 and carries
`line-rate="0.825871"`, which agrees with 166/201. The per-method `lines-valid` figures sum to 201 and the
`hits="0"` entries sum to 35, so the enumeration is complete rather than a sample.

| Member | Uncovered lines | Count | Reachable by an injected-delegate or direct-invocation test? |
|---|---|---|---|
| `Install` boundary `catch` | 83, 84, 87, 88 | 4 | **No.** `Interlocked.Exchange` and the event subscription cannot be made to throw from a test. |
| `Resolve` null-identity guard | 105, 106 | 2 | Yes — `Resolve(null)`. |
| `Resolve` empty-simple-name guard | 111, 112 | 2 | Yes — `Resolve(new AssemblyName())`. |
| `OnAssemblyResolve` null-or-empty-name guard | 157, 158 | 2 | Yes — invoke the handler delegate directly with degenerate `ResolveEventArgs`. |
| `OnAssemblyResolve` boundary `catch` | 170, 171, 174, 175 | 4 | **Not with certainty.** The only throw source is `new AssemblyName(args.Name)`, whose throwing inputs were not observed in this pass. Excluded deliberately. |
| `AssemblyBindingLadder.Resolve` null guard | 229, 230 | 2 | Yes — `ladder.Resolve(null)`. |
| Rung 3 `!_fileExists(path)` early return | 322, 323 | 2 | Yes — `fileExists` returning false on a `netstandard` request. |
| Rung 3 rung-local `catch` | 328, 329, 331, 332 | 4 | Yes — `loadFromPath` throwing on the runtime-directory path. |
| Rung 4 empty-simple-name guard | 345, 346 | 2 | Yes — `ladder.Resolve(new AssemblyName())`. |
| Rung 4 empty-directory guard | 355, 356 | 2 | **No.** `Path.GetDirectoryName(typeof(AssemblyBindingFallback).Assembly.Location)` is not injectable and is non-empty for a file-loaded assembly. |
| Rung 4 success path | 365 | 1 | Yes — `fileExists` true and `loadFromPath` returning a sentinel. |
| Rung 4 rung-local `catch` | 367, 368, 370, 371 | 4 | Yes — `fileExists` true and `loadFromPath` throwing. |
| `TokensAreEqual` null guard | 434, 435 | 2 | **No.** Its only caller passes a non-null requested token, and `Assembly.GetName().GetPublicKeyToken()` returns a zero-length array rather than `null` for an unsigned assembly. |
| `TokensAreEqual` length guard | 439, 440 | 2 | Yes — a three-byte requested token against the eight-byte `mscorlib` token. |

Reachable with certainty: 105, 106, 111, 112, 157, 158, 229, 230, 322, 323, 328, 329, 331, 332, 345, 346,
365, 367, 368, 370, 371, 439, 440. That is **23 lines**. The twelve lines declared unreachable or uncertain
— 83, 84, 87, 88, 170, 171, 174, 175, 355, 356, 434, 435 — are named here rather than quietly omitted, so a
reader can check the claim instead of taking it.

### R7.2 — The arithmetic, stated so it can be checked rather than trusted

- Floor: 90 percent of 201 instrumented lines. `ceil(0.90 x 201) = 181` covered lines.
- Measured: 166 covered. Shortfall to the floor: **15** lines.
- Projected: 166 + 23 = **189** covered, which is `189 / 201 = 94.03` percent.
- Margin above the floor: 8 lines. The plan clears 90 percent even if any 8 of the 23 lines fail to be
  recorded as covered for a reason this pass did not anticipate.
- The DENOMINATOR does not move. All 23 lines are existing production lines in
  `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs`; Revision R7 adds no production line and deletes none,
  so `ASSEMBLYBINDINGFALLBACK_LINES_VALID` remains 201 and the floor is not reached by shrinking the
  denominator.

### R7.3 — Decision: a SIBLING test file, not an amendment of `AssemblyBindingFallbackTests.cs`

Two options were put to this planner. Option (a) re-opens `[P2-T3]` and raises its pinned test count.
Option (b) adds a new task later in the plan that amends the same test file while leaving Phase 2's
checklist untouched. **Revision R7 takes option (b), realised as a new SIBLING FILE rather than as an
amendment of item 5.** The reasoning, with two inbound premises corrected first:

1. **Correction of the first inbound premise.** `[P2-T3]`'s acceptance reads "the file exists and contains
   **at least** eleven occurrences of `[TestMethod]`", not "exactly eleven". Re-derived in this pass at the
   line that carries it. A twelfth test method therefore falsifies neither that standing guard nor
   `[P4-T2]`'s, whose floor likewise reads "the `passed` value is at least 11". Neither guard is the reason
   to avoid amending item 5, and neither needs converting from a standing guard to a point-in-time reading.
   `[P2-T3]`'s second guard, exactly 0 hits for `reaches the GAC`, is a fixed-string absence gate that no
   sibling file can disturb.
2. **The actual binding constraint is the 500-line ceiling.** `AssemblyBindingFallbackTests.cs` is 383
   lines post-format, measured in this pass, leaving 117 lines of headroom against the ceiling that
   `[P4-T12]` and `[P5-T8]` audit. The ten tests specified below cost roughly 150 to 200 lines in the
   existing file's documented Arrange-Act-Assert style, and the file also carries a class-level comment
   that an amendment would have to rewrite. Amending item 5 would land it between 490 and 500 lines before
   `[P5-T2]` reformats it, and CSharpier reflow of a single long statement would then breach the ceiling in
   a task, `[P5-T8]`, that runs after the whole toolchain loop. A sibling file removes that exposure
   entirely rather than managing it.
3. **The sibling file preserves the fail-before evidence by construction.** `[P2-T10]` and `[P2-T11]` are
   NOT unchecked by this revision and nothing in it causes them to re-run. `[P2-T11]` holds this item's only
   fail-before measurement, `DEEDLE_RECORD_CONVERSION_OUTCOME=NETSTANDARD-BIND-FAILURE:TypeInitializationException`,
   taken against a build carrying no fix; Phase 3 has since landed the fix, so a re-run would record a PASS
   and destroy the single artifact this second orchestration attempt exists to produce. Revision R7 writes
   no file in `TaskMaster.Test`, changes no task in Phases 0 through 3, and appends every new task to the
   END of Phase 4, which is after `[P2-T11]` in execution order and cannot reach back to it.
4. **It also keeps AC4 out of scope.** AC4 at `spec.md` lines 484-490 names
   `UtilitiesCS.Test.Bootstrap.AssemblyBindingFallbackTests` specifically. A sibling class is not that
   class, so AC4's enumerated scenario list and its closing injected-delegate sentence are untouched, and
   `[P6-T9]`'s pinned reading line, `AC4 READING: ten delegate-driven tests, eleventh test excluded`,
   remains exactly correct with no edit.
5. **The cost is one `Compile Include` line.** `UtilitiesCS.Test/UtilitiesCS.Test.csproj` uses explicit
   `Compile` items with no wildcard glob — re-derived in this pass, `[P2-T4]`'s own product sits at line
   190 among a hand-maintained list — so an unregistered file silently does not build. This is the same
   remedy `[P4-T12]`'s own acceptance condition already prescribes for a file-size overflow: "split the
   offending type into a second file, record the split as a write-set amendment ... and register the new
   file in the owning `csproj`". Revision R7 applies that prescribed remedy before the overflow rather than
   after it.

### R7.4 — The `.csproj` boundary, stated mechanically rather than promised

The `FSharp.Core` `HintPath` split recorded at `## R6.1` and `## R6.4` is the root cause and is OUT OF
SCOPE, tracked as a separate issue. Revision R7's single `.csproj` edit is a `Compile Include` registration
and touches no `Reference`, no `HintPath` and no package version. That boundary is made checkable rather
than asserted: `[P4-T14]` gates that `UtilitiesCS.Test/UtilitiesCS.Test.csproj` line 598 still reads
`<HintPath>..\packages\FSharp.Core.11.0.100\lib\netstandard2.0\FSharp.Core.dll</HintPath>`, which is the
value re-derived from that file in this pass; that the `lib\netstandard2.1` spelling of that same
`HintPath` is absent from the file; and that a before-and-after `git diff --numstat --cached` reading of
that file reports zero deletions and an addition count exactly one greater after the insertion than before
it. `[P4-T11]` and `[P5-T8]` already gate `FSHARP_REDIRECT_LINES` against the `[P0-T15]` baseline and
`NETSTANDARD_DLL_IN_PROJECTS=0`, and neither figure is affected by a `Compile Include` line.

### R7.5 — The ten new tests, each named with the construct it covers

All ten live in the new file `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs`, in one
`[TestClass]` named `AssemblyBindingFallbackEdgeCaseTests` in namespace `UtilitiesCS.Test.Bootstrap`. The
namespace matters: `[P4-T2]` and `[P4-T17]` filter on `FullyQualifiedName~UtilitiesCS.Test.Bootstrap`, so
the class is discovered by that filter without any runner change.

| # | Test method | Production lines it covers | Mechanism |
|---|---|---|---|
| 1 | `Resolve_WhenRequestedIdentityIsNull_ReturnsNullBeforeAnyRung` | 105, 106 | direct call to the internal static seam |
| 2 | `Resolve_WhenRequestedSimpleNameIsAbsent_ReturnsNullBeforeAnyRung` | 111, 112 | direct call to the internal static seam |
| 3 | `LadderResolve_WhenRequestedIdentityIsNull_ReturnsNull` | 229, 230 | injected delegates |
| 4 | `LadderResolve_WhenRuntimeFacadeFileIsAbsent_RungThreeDeclinesWithoutLoading` | 322, 323 | injected delegates |
| 5 | `LadderResolve_WhenRuntimeFacadeLoadThrows_RungThreeAbsorbsAndDeclines` | 328, 329, 331, 332 | injected delegates |
| 6 | `LadderResolve_WhenRequestedSimpleNameIsAbsent_RungFourDeclinesOnTheNameGuard` | 345, 346 | injected delegates |
| 7 | `LadderResolve_WhenProbeDirectoryHoldsTheAssembly_RungFourLoadsItByPath` | 365 | injected delegates |
| 8 | `LadderResolve_WhenProbeDirectoryLoadThrows_RungFourAbsorbsAndDeclines` | 367, 368, 370, 371 | injected delegates |
| 9 | `LadderResolve_WhenLoadedTokenLengthDiffers_RungOneRejectsTheLoadedAssembly` | 439, 440 | injected delegates |
| 10 | `OnAssemblyResolve_WhenEventArgsCarryNoName_ReturnsNullWithoutResolving` | 157, 158 | direct delegate invocation, no bind |

Tests 7 and 8 are both retained although test 8 alone would enter line 365 on its way to throwing. The
duplication is deliberate and is the reason the 23-line claim does not depend on how the collector records
a line whose statement throws part-way through: test 7 reaches 365 on a non-throwing path.

**Constraint compliance, stated per constraint.**

- **Injected delegates only, and the eleventh test stays the single stated exception.** Tests 3 to 9 drive
  `AssemblyBindingLadder` through its five constructor delegates and reach no rung by any other route.
  Tests 1, 2 and 10 never reach a rung at all: each returns at a guard clause before the ladder is
  constructed. No test in the new file calls `Assembly.Load`, `Assembly.LoadFrom` or `File.Exists`, so
  none touches the GAC or the filesystem, and
  `Install_ThenLoadOfUnresolvableName_LeavesTheLoadFailingWithoutHandlerThrowing` in item 5 remains the
  only test in this work that drives a real bind.
- **AC16 determinism.** No test creates, writes or deletes a file; none uses `Thread.Sleep`, `Task.Delay`,
  a wall-clock read or any other token on `[P4-T9]`'s banned list, in code or in a comment. `[P4-T18]`
  re-runs that sweep with the new file added to the path list, so the claim is measured and not asserted.
- **The 500-line ceiling.** The new file is a fresh file and is budgeted at roughly 200 lines. `[P4-T18]`
  audits it pre-format and `[P5-T8]` audits it post-format, both against the same 500-line ceiling.
- **Order independence.** No new test mutates a static of the type under test: tests 1, 2 and 10 return at
  a guard before `_resolvingSimpleName` is written at line 120, and none calls `Install()`. The new class
  therefore needs no `[TestCleanup]` and cannot interfere with item 5's class under the `ClassLevel`
  parallel scope both files run under.

### R7.6 — What must re-run, stated rather than left to be inferred

The new tests change the compiled `UtilitiesCS.Test` assembly. A completed toolchain loop measured a tree
that no longer exists, so:

- **The entire Phase 5 loop restarts from step 1.** `[P5-T1]` through `[P5-T8]` are unchecked by this
  revision and re-execute in order: Outlook gate, format, format verification, config side-effect check,
  analyzers, nullable, tests with coverage, post-format audits. `[P5-T9]` and `[P5-T10]` then re-run; both
  were already unchecked.
- **The ladder unit-test run is re-taken at `[P4-T17]` with a floor raised to match.** `[P4-T2]` is left
  CHECKED and is not re-executed in place. Re-executing it in place was considered and rejected on an
  ordering ground: `[P4-T2]` sits at numeric position 2 of Phase 4 and the tasks that author the new tests
  are appended at positions 13 to 18, so an executor working the list in order would re-run `[P4-T2]`
  against an assembly that does not yet contain the new tests, and a raised floor there would be
  unsatisfiable at the moment it ran. `[P4-T17]` performs the same measurement after the new tests exist,
  against its own pinned TRX name, its own results directory and its own artifact, with a floor of 21
  passed and 0 failed. `[P4-T2]`'s recorded "at least 11" acceptance remains true and is not weakened;
  `[P4-T17]` supersedes it as the ladder suite's measurement of record.
- **The two path-list sweeps are re-taken at `[P4-T18]`.** `[P4-T9]`'s determinism sweep names four test
  files and `[P4-T12]`'s size audit names six files; both are left checked with their recorded four- and
  six-path outputs intact, and `[P4-T18]` runs the five-path and seven-path versions that include the new
  file. `[P5-T8]` is repointed to re-run `[P4-T18]`'s commands post-format rather than `[P4-T9]`'s and
  `[P4-T12]`'s.

### R7.7 — Checklist state changed by this revision

Unchecked and to be re-executed: `[P5-T1]`, `[P5-T2]`, `[P5-T3]`, `[P5-T4]`, `[P5-T5]`, `[P5-T6]`,
`[P5-T7]` and `[P5-T8]` — eight tasks, all for the single reason that a source change invalidates a
completed toolchain loop. `[P5-T9]` through `[P5-T12]` and all of Phase 6 were already unchecked.

Added and unchecked: `[P4-T13]` through `[P4-T18]` — six tasks, all appended to the END of Phase 4 so that
no existing `[P#-T#]` identifier is renumbered and every identifier this plan and its evidence artifacts
already cite is unchanged.

**Left checked and NOT re-executed:** `[P0-T1]` through `[P0-T16]`, `[P1-T1]` through `[P1-T7]`, `[P2-T1]`
through `[P2-T12]`, `[P3-T1]` through `[P3-T5]`, and `[P4-T1]` through `[P4-T12]`. `[P2-T10]` and
`[P2-T11]` are named explicitly in that set: `[P2-T11]` is the only fail-before measurement this item has,
it was taken against a build carrying no fix, and re-running it now would record a PASS and destroy it.
`[P2-T10]` is the targeted rebuild `[P2-T11]` depends on, so unchecking it would force `[P2-T11]` to
re-run as well. Revision R7 writes no file either task compiles or runs: the new test file is in
`UtilitiesCS.Test`, and `[P2-T10]` and `[P2-T11]` operate on `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`
under a `FullyQualifiedName~TaskMaster.Test.Bootstrap` filter, which discovers no class in
`UtilitiesCS.Test`. Four task bodies gain Revision R7 cross-reference notes without changing state —
`[P2-T3]`, `[P4-T2]`, `[P4-T9]` and `[P4-T12]` — because a body note that adds no acceptance condition
cannot falsify a recorded result.

### R7.8 — Constraints re-checked and deliberately left unchanged

- The harness re-rooting at `QuickFiler.Test/bin/Debug`, the probe, the pre-run TRX removal spans and their
  `PRERUN_TRX_COUNT=0` clauses, `-CoverageOutput` at its default, the `origin/main` diff anchors, the manual
  live-Outlook gate at `[P6-T4]` and the conditional `[P6-T24]` are all unchanged by Revision R7.
- No acceptance-criterion text is amended and `spec.md` is not written. Every `spec.md` line number this
  plan cites — 473, 477, 481, 484, 491, 495, 499, 504, 509, 515, 521, 528, 534, 536, 543, 545, 547, 550 and
  554 — is unchanged, because no write to that file occurs in this revision.
- `[P4-T17]` and `[P4-T18]` introduce no new diff anchor and invoke no coverage runner, so issue #891's
  document-level threshold is not engaged by either.

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
  An executor launched without worktree isolation inherits a different checkout as its working directory,
  in which case every repository-relative path in this plan resolves into the wrong tree silently. Before
  `[P0-T1]`, and again after any tool or shell restart, confirm the working directory with
  `git rev-parse --show-toplevel` and require the result to end with the item worktree directory name
  `bugs-2026-09-11-item-879`. If it does not, report blocked rather than adapting the paths: a plan whose
  evidence was written into another checkout cannot be audited.

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
- [x] [P0-T7] LOCK-ACQUIRE, then capture the nullable baseline with the console log redirected to the
      evidence tree. Then LOCK-RELEASE. Same pwsh shape as `[P0-T6]` with the final msbuild line replaced
      by:

      ```
      & $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true *> "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/nullable-baseline-console.2026-09-13T18-22.txt"
      ```

      Write `.../evidence/baseline/nullable-baseline.2026-09-13T18-22.md` with the four required fields
      plus `ExpectedExitCode:`.
      Acceptance: both artifacts exist and the `.md` records the integer baseline error count.
- [x] [P0-T8] LOCK-ACQUIRE, then capture the coverage-bearing test baseline:
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
- [x] [P0-T9] Record the baseline for the changed-line and new-module coverage obligations. Append to the
      `[P0-T8]` artifact a `Coverage Obligations:` section stating: repository-wide line coverage floor
      per `CLAUDE.md` is `>= 80%` on the testable denominator; new modules target `>= 90%`; changed lines
      must not regress; `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` does not yet exist, so its
      baseline per-file line coverage is recorded as `NOT PRESENT AT BASELINE`.
      Acceptance: that section exists and carries that exact status string for the new file.
- [x] [P0-T10] Close the build-output premise for the harness host. Using the build produced by `[P0-T7]`,
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
- [x] [P0-T11] Record the premise that `TaskMaster.Test/app.config` carries no `netstandard` entry, which
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
- [x] [P0-T12] Indicative probe of the private `AppDomain` assembly-resolution field. Command:

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
- [x] [P0-T13] Record the fail-loud rule for the isolation assertion, so no later task can weaken it.
      Append to the `[P0-T12]` artifact a `Fail-Loud Rule:` section stating: the child-domain probe
      resolves the field by the name `_AssemblyResolve` with `BindingFlags.Instance | BindingFlags.NonPublic`;
      if the lookup returns `null` the probe throws `InvalidOperationException` naming the field; it must
      never call `Assert.Inconclusive`, never return a sentinel that the test treats as success, and never
      skip. A silently skipped isolation check makes every positive test in the harness vacuous.
      Acceptance: that section exists and contains the literal token `InvalidOperationException`.
- [x] [P0-T14] Write-set decision record for the harness host.
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
- [x] [P0-T15] Record the baseline scope-boundary state. Command:

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
- [x] [P0-T16] Record the baseline line counts of the five files this plan will edit in place, so the
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

### Phase 1 — Spec Correction, Acceptance-Criteria Inventory, and Revision R2 Preconditions

- [x] [P1-T1] Confirm the planner's spec correction is present. The acceptance criterion at `spec.md`
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
- [x] [P1-T2] Record the acceptance-criteria inventory by `spec.md` line number, so every later check-off
      task names a determinate target. Write
      `.../evidence/other/ac-inventory.2026-09-13T18-22.md` listing exactly these nineteen pairs:
      AC1 line 473, AC2 line 477, AC3 line 481, AC4 line 484, AC5 line 491, AC6 line 495, AC7 line 499,
      AC8 line 504, AC9 line 509, AC10 line 515, AC11 line 521, AC12 line 528, AC13 line 534, AC14
      line 536, AC15 line 543, AC16 line 545, AC17 line 547, AC18 line 550, AC19 line 554.
      Acceptance: the artifact exists and carries exactly nineteen `AC` entries, and a spot check confirms
      that each named line currently begins with the six characters `- [ ] `.
- [x] [P1-T3] Confirm no non-canonical evidence directory remains in `spec.md`. Command:

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
- [x] [P1-T4] Lock the scope. Append to the `[P1-T2]` artifact a `Scope Lock:` section reproducing this
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
- [x] [P1-T5] Preserve the superseded `[P2-T11]` run artifact before the re-run overwrites it. The
      existing artifact records `Acceptance Condition: NOT MET` and is the only record of the measurement
      that identified Defect 1, so it is copied rather than lost. Command:

      ```
      pwsh -NoProfile -Command '
      $src = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-run.2026-09-13T18-22.md"
      $dst = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-run-superseded-probe-surface.2026-09-13T18-22.md"
      Copy-Item -LiteralPath $src -Destination $dst -Force
      Write-Output ("SUPERSEDED_COPY_PRESENT=" + (Test-Path -LiteralPath $dst))
      Write-Output ("SUPERSEDED_COPY_LINES=" + @(Get-Content -LiteralPath $dst).Count)
      '
      ```

      Then edit the copy only, never the original, inserting immediately after its `Timestamp:` line this
      one line:
      `Superseded: yes - revision R2 repointed the probe member that [P2-T5] specifies, and [P2-T11] was re-run against the repointed probe.`
      The literal `Superseded: yes` is quoted here in prose because it is absent from the tree until this
      task runs.
      Acceptance: the copy exists at the path above, contains the literal `Superseded: yes` exactly once,
      and contains the literal `AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Passed` exactly once,
      which is the vacuous-pass observation this copy exists to retain. The original
      `expect-fail-run.2026-09-13T18-22.md` is left byte-identical by this task.
- [x] [P1-T6] Measure the Deedle member surface from metadata, before `[P2-T5]` is authored against it.
      **Revision R5 repoints this task twice** and it is therefore unchecked and re-run: the file read moves
      from `TaskMaster.Test/bin/Debug/Deedle.dll` to `QuickFiler.Test/bin/Debug/Deedle.dll`, which is the
      file the re-rooted child domain loads, and the member measured moves from
      `Deedle.Reflection.convertRecordSequence` to the `Deedle.Frame.FromRecords` overload set, which is
      production's entry point at `UtilitiesCS/Extensions/DfDeedle.cs` lines 123 and 237. The artifact at
      the path named below is overwritten by this re-run.
      The read is metadata-only through `System.Reflection.Metadata`: it never executes Deedle code, never
      runs a type initializer and never resolves `FSharp.Core`, so this measurement cannot itself trip the
      bind under test, and it works regardless of the target framework of the deployed `Deedle.dll`.
      Command:

      ```
      pwsh -NoProfile -Command '
      $p = "QuickFiler.Test/bin/Debug/Deedle.dll"
      Write-Output ("DEEDLE_DLL_PRESENT=" + (Test-Path -LiteralPath $p))
      $fs = [System.IO.File]::OpenRead((Resolve-Path -LiteralPath $p).Path)
      $pe = New-Object System.Reflection.PortableExecutable.PEReader($fs)
      $md = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
      Write-Output ("TYPEDEF_COUNT=" + @($md.TypeDefinitions).Count)
      $defs = 0
      $arity1 = 0
      foreach ($h in $md.TypeDefinitions) {
      $td = $md.GetTypeDefinition($h)
      $ns = $md.GetString($td.Namespace)
      $n = $md.GetString($td.Name)
      if (($ns -eq "Deedle") -and $n.StartsWith("Frame")) { Write-Output ("DEEDLE_FRAME_LIKE_TYPE=" + $ns + "." + $n) }
      if (($ns -eq "Deedle") -and ($n -eq "Frame")) {
      Write-Output ("TYPE_FOUND=Deedle.Frame")
      Write-Output ("TYPE_ATTRS=" + $td.Attributes)
      foreach ($mh in $td.GetMethods()) {
      $m = $md.GetMethodDefinition($mh)
      if ($md.GetString($m.Name) -eq "FromRecords") {
      $g = @($m.GetGenericParameters()).Count
      $defs = $defs + 1
      if ($g -eq 1) { $arity1 = $arity1 + 1 }
      Write-Output ("MEMBER_ATTRS=" + $m.Attributes)
      Write-Output ("FROMRECORDS_MEMBER_GENERIC_PARAM_COUNT=" + $g) } } } }
      Write-Output ("FROMRECORDS_DEFINITIONS=" + $defs)
      Write-Output ("FROMRECORDS_GENERIC_ARITY_1_COUNT=" + $arity1)
      $pe.Dispose()
      $fs.Dispose()
      '
      ```

      `TYPEDEF_COUNT` is the positive control: it proves the reader opened a real assembly and the name
      comparison mechanism is live, so a zero member count would be an observation rather than an artefact
      of an unreadable file. The `DEEDLE_FRAME_LIKE_TYPE=` lines are a recorded diagnostic and are
      deliberately NOT gated: F# compiles a module whose name collides with a type to a suffixed name, so
      if the static surface this repository calls as `Frame.FromRecords` is not spelled `Deedle.Frame` in
      metadata, these lines name what it is actually spelled and the correction can be made in one round
      rather than two.

      One bounded adaptation is authorised here and nowhere else in this plan. `System.Reflection.Metadata`
      ships in the .NET shared framework, but a pwsh host resolves a type name only against assemblies it
      has already loaded. If either `[System.Reflection.PortableExecutable.PEReader]` or
      `[System.Reflection.Metadata.PEReaderExtensions]` reports that the type cannot be found, prepend
      `[void][System.Reflection.Assembly]::Load("System.Reflection.Metadata")` as the first statement of
      the payload, re-run, and record that the adaptation was taken in the artifact's `Command:` field.
      Nothing else about the command or the acceptance condition may change. Write
      `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/other/deedle-member-surface.2026-09-13T18-22.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:` and an `Output Summary:` reproducing every emitted line
      verbatim.
      Acceptance: the artifact records `DEEDLE_DLL_PRESENT=True`, a `TYPEDEF_COUNT` greater than 0, exactly
      one `TYPE_FOUND=Deedle.Frame` line, and `FROMRECORDS_GENERIC_ARITY_1_COUNT=1`. A value of `0` blocks,
      because `[P2-T5]` cannot be authored against a member that does not exist; a value greater than `1`
      also blocks, because `[P2-T5]`'s shape-filtered lookup would then select more than one candidate and
      the plan would have to name the overload by parameter type. `FROMRECORDS_DEFINITIONS` is recorded and
      deliberately not gated: `Deedle.Frame` is expected to carry more than one `FromRecords` overload, and
      `[P2-T5]`'s selector discriminates on generic arity and parameter count rather than on the total.
      `MEMBER_ATTRS` is recorded and not gated: `[P2-T5]`'s lookup passes
      `BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static`, which covers either
      accessibility this line can report. The literals `TYPE_FOUND=Deedle.Frame`,
      `FROMRECORDS_GENERIC_ARITY_1_COUNT=`, `FROMRECORDS_DEFINITIONS=` and `DEEDLE_FRAME_LIKE_TYPE=` are
      quoted here in prose because they are absent from the tree until this task runs.
- [x] [P1-T7] Confirm the planner's AC10 rewrite is present and that it displaced no sibling criterion.
      **Revision R5 rewrote AC10 a second time** and this task is therefore unchecked and re-run against
      the new text; its two content tokens change, so the recorded Revision R2 result no longer describes
      the file. The planner rewrote acceptance criterion AC10 at `spec.md` lines 515-520 in place, in
      exactly six lines, so every acceptance-criterion line number recorded by `[P1-T2]` and consumed by
      `[P6-T6]` through `[P6-T24]` is unchanged. Revision R5 additionally rewrote two sibling regions of
      `spec.md` that the re-rooting invalidated, each line for line: the build-output assumption at lines
      392-394 and the Test Strategy domain-configuration paragraph at lines 431-434. Neither is an
      acceptance criterion and neither changed the file's line count. Command:

      ```
      pwsh -NoProfile -Command '
      $p = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md"
      $lines = @(Get-Content -LiteralPath $p)
      Write-Output ("AC10_LINE_515_PREFIX=[" + $lines[514].Substring(0,6) + "]")
      Write-Output ("AC11_LINE_521_PREFIX=[" + $lines[520].Substring(0,6) + "]")
      Write-Output ("AC_HEADING_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "## Acceptance Criteria").Count)
      Write-Output ("NETSTANDARD21_FLAVOUR_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "netstandard2.1").Count)
      Write-Output ("FROMRECORDS_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "Deedle.Frame.FromRecords").Count)
      Write-Output ("QUICKFILER_TEST_CONFIG_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "QuickFiler.Test.dll.config").Count)
      Write-Output ("RUNCLASSCONSTRUCTOR_HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "RunClassConstructor").Count)
      '
      ```

      `netstandard2.1` and `Deedle.Frame.FromRecords` are each a single-line token introduced by the
      Revision R5 rewrite and present nowhere else in `spec.md`; each is one identifier or one hyphenated
      word rather than a multi-word phrase, so no re-wrap of the surrounding sentence can split it.
      `QuickFiler.Test.dll.config` is the token introduced by the sibling Test Strategy rewrite at lines
      431-434 and is likewise present nowhere else. All three had zero hits in `spec.md` before the
      rewrite, which is what makes their expected counts discriminating rather than already satisfied.
      `AC_HEADING_HITS` is the positive control proving the file path and the search mechanism are live.
      `RUNCLASSCONSTRUCTOR_HITS` is recorded and deliberately NOT gated: the literal never appeared in
      `spec.md`, so a zero-hit assertion on it could not fail whatever the executor does. Append the output
      to
      `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/other/ac-inventory.2026-09-13T18-22.md`
      under an `AC10 Revision R5 Rewrite:` heading. The heading names R5 rather than R2 so the earlier
      Revision R2 block in the same artifact is preserved alongside it rather than overwritten.
      Acceptance: the heading exists, `AC10_LINE_515_PREFIX=[- [ ] ]`, `AC11_LINE_521_PREFIX=[- [ ] ]`,
      `AC_HEADING_HITS=1`, `NETSTANDARD21_FLAVOUR_HITS=1`, `FROMRECORDS_HITS=1` and
      `QUICKFILER_TEST_CONFIG_HITS=1`. Any other value for the two prefix lines blocks: it would mean the
      rewrite changed the line count and every `spec.md` line number this plan cites would be stale.

### Phase 2 — Regression Harness First (fails before the fix)

The bugfix workflow requires the regression test before the fix. A C# regression test cannot reference a
type that does not exist, so Phase 2 lands a **declaration-complete, behaviour-empty** seam: the public
and internal surface of `AssemblyBindingFallback` compiles, and every ladder rung returns `null`. The
harness therefore fails at **runtime**, not at compile time, which is the fail-before evidence this issue
needs. Phase 3 supplies the behaviour.

Phase 2 runs a targeted build and a targeted test run only. It does not run the analyzer gate, the
nullable gate or the full suite: those gates would be evaluated against a deliberately incomplete seam.

- [x] [P2-T1] Amend `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` in place — the file already exists
      from the version 1.0 execution of this plan and must NOT be recreated — so that it declares
      `public static class AssemblyBindingFallback` in namespace `UtilitiesCS.Bootstrap`, with:
      a public `static void Install()` guarded for idempotence by `Interlocked.Exchange` on a private
      `int` field; an `internal static Assembly? Resolve(AssemblyName requested)` seam; a private
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
      `#nullable enable` at line 1 and XML documentation on the public surface. No
      WinForms type, no Outlook Interop type, no log4net reference.

      **Revision R2 amendment (Defect 2), four named sites and nothing else.** The version 1.0 text of
      this task pinned `internal static Assembly Resolve(`, a non-nullable return on a method whose
      contract is to return null for every name it cannot resolve, and the executor satisfied it with
      null-forgiving operators on the dominant path. The rationale and the measured evidence are in
      `## R4.2`. Change exactly these four sites and no other line of the file:

      1. The declaration at line 99 becomes `internal static Assembly? Resolve(AssemblyName requested)`.
      2. The three `return null!;` statements inside `Resolve`, at lines 103, 109 and 114, become
         `return null;`.
      3. Line 120 becomes `return CreateProductionLadder().Resolve(requested);`. The trailing
         null-forgiving operator is unnecessary once the declared return type is nullable, because
         `AssemblyBindingLadder.Resolve` at line 223 already returns `Assembly?`.
      4. Line 158 becomes `Assembly? resolved = Resolve(new AssemblyName(args.Name));`. Without this the
         nullable gate at `[P5-T6]` fails with CS8600 promoted to an error.

      `OnAssemblyResolve` keeps its declared return type `Assembly` and its three `null!` returns at lines
      155, 162 and 172, for the role-based reason stated in `## R4.2`: it is the `ResolveEventHandler`
      boundary and that delegate is oblivious on net48, so the suppression there is a boundary suppression
      rather than a dominant-path one. The `<returns>` prose at lines 94-98 currently justifies the
      non-nullable return as mirroring `ResolveEventHandler`; rewrite that prose so it describes the
      nullable seam instead, or the file contradicts itself. `return resolved;` at line 165 needs no
      change: the preceding `if (resolved is null)` return makes the flow state not-null there.

      Acceptance: the file exists, and `Select-String -SimpleMatch -CaseSensitive` on
      `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` returns: at least one hit each for
      `public static void Install()`, `internal sealed class AssemblyBindingLadder` and `[ThreadStatic]`;
      exactly 1 hit for `internal static Assembly? Resolve(`; exactly 0 hits for
      `internal static Assembly Resolve(`; exactly 1 hit for `private static Assembly OnAssemblyResolve(`;
      exactly 3 hits for `null!`; exactly 0 hits for `Resolve(requested)!`; exactly 1 hit for
      `Assembly? resolved =`; and zero hits for `System.Windows.Forms`, `Microsoft.Office.Interop` and
      `log4net`. The three surviving `null!` hits are the three inside `OnAssemblyResolve` and are the
      declared boundary suppression. The literals `internal static Assembly? Resolve(` and
      `Assembly? resolved =` are quoted here in prose because they are absent from the tree until this
      task runs; `internal static Assembly Resolve(` is present in the tree now, which is what makes its
      zero-hit assertion discriminating rather than vacuous.
- [x] [P2-T2] Register the new production file. Insert
      `<Compile Include="Bootstrap\AssemblyBindingFallback.cs" />` into the `ItemGroup` in
      `UtilitiesCS/UtilitiesCS.csproj` that already contains
      `<Compile Include="Extensions\DfDeedle.cs" />` at line 996. This project uses explicit `Compile`
      items; an unregistered file silently does not build.
      Acceptance: `Select-String -SimpleMatch -Pattern "Bootstrap\AssemblyBindingFallback.cs"` on
      `UtilitiesCS/UtilitiesCS.csproj` returns exactly 1 hit.
- [x] [P2-T3] Amend `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs` in place — the file
      already exists from the version 1.0 execution of this plan, carrying exactly ten `[TestMethod]`
      occurrences, and must NOT be recreated — so that it remains MSTest with Moq and
      FluentAssertions, one `[TestClass]` named `AssemblyBindingFallbackTests` in namespace
      `UtilitiesCS.Test.Bootstrap`, with one separately named `[TestMethod]` for each of: already-loaded
      match wins over a fresh load; the full-display-name rung; the runtime-directory load-from-path rung;
      a mismatched public key token is rejected; a null requested token; an empty requested token; the
      re-entrance guard returns null on a nested request for the same simple name; `Install()` called
      twice attaches one handler; a rung that throws internally is absorbed and the ladder continues; an
      unresolvable name returns null without throwing. Every ladder rung in those ten tests is driven
      through the injected
      delegates, so those tests touch neither the GAC nor the filesystem. A `[TestCleanup]` resets any
      mutated seam. The class carries Arrange-Act-Assert structure and a short intent comment per test.

      **Revision R3 amendment: one added test, closing the `[P5-T9]` coverage-floor risk.** Add an
      eleventh `[TestMethod]`, separately named, asserting that the subscribed handler returns null for
      an unresolvable name, driven by attempting `Assembly.Load` of a display name that no rung can
      satisfy after `Install()` has run. This is the only test in the class that goes through the real
      CLR binder rather than through the injected delegates, and it is added deliberately: the ten
      existing tests drive `AssemblyBindingLadder` directly and none of them executes
      `AssemblyBindingFallback.OnAssemblyResolve` at lines 149-174 or
      `AssemblyBindingFallback.CreateProductionLadder` at lines 134-143, both of which are reachable only
      through a real failed bind raising `AppDomain.AssemblyResolve`. Those two members are roughly
      twenty-five lines of the new module, so leaving them uncovered risks `[P5-T9]`'s 90 percent
      per-file floor failing in Phase 5, after the toolchain loop has already run.
      Constraints on the added test: the display name is a fixed literal that names no assembly in the
      repository, in the GAC or beside the test assembly, so the outcome is deterministic; the test
      asserts the `Assembly.Load` call throws rather than inspecting the handler's return value
      directly, because the CLR consumes that return value; and the test creates, writes and deletes no
      file, uses no `Thread.Sleep`, no `Task.Delay` and no wall-clock wait, so AC16 at `spec.md` line 545
      is unaffected. The prose sentence above about touching neither the GAC nor the filesystem is
      scoped to the ten delegate-driven tests for this reason; this eleventh test reads the binder's
      probing paths, which is what makes it cover the production entry point. The class-level XML
      documentation comment at `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs` lines 11-15
      states that every rung is driven through the ladder's injected delegates so no test
      reaches the GAC, the filesystem or a real bind. That sentence becomes false once the eleventh test
      exists, so this task rewrites it: the claim is scoped to the ten delegate-driven ladder tests, and
      a second sentence states that one further test drives the subscribed handler through the real CLR
      binder in order to cover the production entry point. The rewritten comment must not contain any
      token on `[P4-T9]`'s banned list, whose sweep is textual and covers comments.
      Acceptance: the file exists and contains at least eleven occurrences of `[TestMethod]`.
      Additionally, `Select-String -SimpleMatch -CaseSensitive` on that file returns exactly 0 hits for
      `reaches the GAC`, which is a single-line fragment that no CSharpier pass reflows.

      Both conditions are stated as standing guards rather than as new measurements, because this task
      has executed and its own product already satisfies them, so no reader mistakes them for evidence
      that this task ran. The discriminating readings that justified them were taken before execution:
      the file then carried exactly ten `[TestMethod]` occurrences, which is the figure the body of this
      task records above, and it then carried `reaches the GAC` at line 14. The post-execution readings,
      taken against the tree at `fce5994c6`, are eleven `[TestMethod]` occurrences and 0 hits for
      `reaches the GAC` in a file of 383 lines. The pre-execution figures are retained in the body above
      as the record of why each condition was authored and are not restated here as current
      measurements; the guards themselves are retained so that a later edit to this file cannot regress
      them.

      **Revision R7 note — no state change, no acceptance change.** Both guards are retained verbatim and
      this task remains checked. The `[TestMethod]` guard reads "at least eleven", not "exactly eleven",
      so it is not a ceiling and it does not forbid a later task from adding test methods to this file.
      Revision R7 nonetheless adds no test method here: the ten coverage tests it specifies live in a
      sibling file, `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs`, written by
      `[P4-T13]`. The reason is the 500-line ceiling rather than either guard — this file is 383 lines
      post-format and ten tests in its documented style would land it within a CSharpier reflow of the
      ceiling that `[P5-T8]` audits after the whole toolchain loop has run. `## R7.3` records the full
      reasoning. No Revision R7 task writes this file, so neither guard is re-measured and neither
      `[P2-T10]` nor `[P2-T11]` is disturbed.
- [x] [P2-T4] Register the new unit-test file. Insert
      `<Compile Include="Bootstrap\AssemblyBindingFallbackTests.cs" />` into the `ItemGroup` in
      `UtilitiesCS.Test/UtilitiesCS.Test.csproj` that already contains
      `<Compile Include="Extensions\DfDeedle_Tests.cs" />` at line 194. The citation read 193 in version
      1.0 of this plan; the merge of `origin/main` at `a49c9729e` moved the anchor by one line. This task
      remains complete: its own product,
      `<Compile Include="Bootstrap\AssemblyBindingFallbackTests.cs" />`, is present at line 190 of that
      project file. Only the anchor citation was stale.
      Acceptance: `Select-String -SimpleMatch -Pattern "Bootstrap\AssemblyBindingFallbackTests.cs"` on
      `UtilitiesCS.Test/UtilitiesCS.Test.csproj` returns exactly 1 hit.
- [x] [P2-T5] Amend `TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs` in place — the file already
      exists from the version 1.0 execution of this plan and must NOT be recreated — so that it declares
      `public sealed class ChildDomainBindProbe : MarshalByRefObject` in namespace
      `TaskMaster.Test.Bootstrap`. It exposes separate public methods so that the negative-control path
      never JIT-resolves `UtilitiesCS`: `CountLoadedAssembliesNamed(string simpleName)`;
      `CountAssemblyResolveHandlers()` which resolves the private `AppDomain` field `_AssemblyResolve`
      with `BindingFlags.Instance | BindingFlags.NonPublic`, throws `InvalidOperationException` naming the
      field when the lookup returns null, returns 0 when the field value is null, and otherwise returns
      the length of `Delegate.GetInvocationList()`; `InstallProductionFallback()` which calls
      `UtilitiesCS.Bootstrap.AssemblyBindingFallback.Install()` and nothing else;
      `TryLoadDisplayName(string displayName)` returning a marshalled outcome string that is either
      `LOADED` or the exception type name; `DeedleRecordConversionOutcome(string deedleDllPath)`, whose
      full specification is the Revision R2 block below and which REPLACES the version 1.0 member
      `DeedleTypeInitializerOutcome` and the `OkOutcome` constant it returned;
      `ApplicationBaseDirectory()`, added by Revision R5, which returns
      `AppDomain.CurrentDomain.BaseDirectory` read **inside** the child domain and nothing else, and is what
      makes the re-rooting falsifiable rather than asserted; and
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

      **Revision R2 amendment (Defect 1), as further repointed by Revision R5: the probe member.** The
      Revision R2 rationale is in `## R4.1` and the Revision R5 measurement that supersedes its choice of
      member is in `## R6.1`. Revision R5 changes the member the probe invokes from
      `Deedle.Reflection.convertRecordSequence` to `Deedle.Frame.FromRecords`, which is production's actual
      entry point at `UtilitiesCS/Extensions/DfDeedle.cs` lines 123 and 237. The measurement in `## R6.1`
      records that both members reach the same bind, so this change is for fidelity to production rather
      than for discrimination; what supplies the discrimination is the `ApplicationBase` that `[P2-T6]`
      sets. The file carries no `#nullable enable` directive and this task adds none,
      so no member specified here is written with a nullable reference annotation; a `?` on a reference
      type in this file raises CS8632, which `[P5-T6]` promotes to a build error. Specify:

      1. Three public string constants replacing `OkOutcome`, named exactly `InvokedOutcome`,
         `BindFailurePrefix` and `OtherFailurePrefix`, whose values are exactly `INVOKED-NO-EXCEPTION`,
         `NETSTANDARD-BIND-FAILURE:` and `OTHER-FAILURE:` in that order. The identifiers are fixed here
         because `[P2-T6]` references `InvokedOutcome` and `BindFailurePrefix` by name rather than
         repeating their values. `LoadedOutcome` and its value `LOADED` are unchanged and still used by
         `TryLoadDisplayName`.
      2. A public nested type `public sealed class DeedleProbeRecord` carrying exactly two public
         auto-properties, a `string` and a `double`. It is a record-shaped input of the same kind
         production supplies to `Deedle.Frame.FromRecords` at `UtilitiesCS/Extensions/DfDeedle.cs`
         lines 123 and 237. It is not a copy of production's shape: `EmailRecord` at `DfDeedle.cs`
         line 266 is a `private struct` exposing public fields, whereas this probe type is a sealed
         class exposing auto-properties. Deedle's member accepts either, and a class with
         auto-properties is used here because the probe type must be public for the cross-domain proxy
         to close the generic method over it. It carries
         no `DateTime` member, because `[P4-T9]`'s determinism sweep is textual and bans `DateTime.Now`
         and `DateTime.UtcNow` in this file.
      3. `public string DeedleRecordConversionOutcome(string deedleDllPath)`, which:
         checks `string.IsNullOrEmpty` and `File.Exists` BEFORE its try block and throws
         `InvalidOperationException` naming the path when either fails;
         builds a one-element array of `DeedleProbeRecord`;
         then, inside the try, calls `Assembly.LoadFrom(deedleDllPath)`, obtains the type
         `Deedle.Frame` with `throwOnError: true`, resolves the member through the helper in item 4,
         closes it with `MakeGenericMethod` over `typeof(DeedleProbeRecord)`, invokes it through
         `MethodInfo.Invoke` with the one-element array as the single argument, and returns the
         `INVOKED-NO-EXCEPTION` constant on the statement immediately after that invocation and on no
         other path. The invocation's return value is discarded rather than marshalled: it is a
         `Deedle.Frame<int,string>`, which is not serialisable across the domain boundary, and the
         measurement is whether the invocation completed rather than what it produced;
         carries `catch (InvalidOperationException) { throw; }` as its FIRST catch clause, so a
         fail-loud lookup miss escapes past the classifier and can never be reported as an outcome;
         and carries a general `catch (Exception)` whose only action is to return the classifier in
         item 5.
      4. A private static helper that resolves the member by SHAPE and fails loudly. `Deedle.Frame`
         carries more than one `FromRecords` overload, so a name-only `Type.GetMethod` lookup raises
         `AmbiguousMatchException` and cannot be used. The helper calls
         `Type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)`, which
         covers either accessibility `[P1-T6]` may have recorded, then keeps only the members whose name is
         `FromRecords`, which are generic method definitions, whose `GetGenericArguments()` length is
         exactly 1, and whose `GetParameters()` length is exactly 1. It throws `InvalidOperationException`,
         naming the type, the member and the surviving count, unless exactly one member survives the
         filter. `GetMethods` does not raise `AmbiguousMatchException`, so no catch for that type is
         written and none may be. The surviving count is the same figure `[P1-T6]` gates as
         `FROMRECORDS_GENERIC_ARITY_1_COUNT=1`, so the plan and the probe select the same member by the
         same rule. Resolving a `MethodInfo` does not run a class constructor, which is why this helper can
         sit inside the try without itself triggering the bind; the invocation in item 3 is what triggers
         it.
      5. A private static classifier taking the thrown exception and returning one structured string.
         It first unwraps `TargetInvocationException` — `MethodInfo.Invoke` wraps the real exception, so
         without the unwrap every failure would be reported as `TargetInvocationException` and the
         outcome would carry no information. It then walks the whole `InnerException` chain of the
         ORIGINAL exception looking for a `FileNotFoundException` whose `FileName` or `Message` contains
         `netstandard`, compared with `StringComparison.OrdinalIgnoreCase`; the chain rather than the
         outermost exception is walked because the production shape is
         `TargetInvocationException` wrapping `TypeInitializationException` wrapping
         `TypeInitializationException` wrapping `FileNotFoundException`. On a match it returns the
         `NETSTANDARD-BIND-FAILURE:` prefix followed by the unwrapped exception's simple type name;
         otherwise it returns the `OTHER-FAILURE:` prefix followed by that same name. Both `FileName` and
         `Message` are inspected because the CLR does not guarantee `FileName` is populated on every
         binding failure, and the failing display name appears verbatim in the message text in either
         case.
      6. `public string ApplicationBaseDirectory()` returns `AppDomain.CurrentDomain.BaseDirectory`, read
         inside the child domain, and does nothing else. It takes no argument, touches no file, and loads
         no assembly, so it is safe to call in the installer-free domain as well as the positive one. It
         exists so that `[P2-T6]`'s ninth test can assert the re-rooting rather than assume it.

      The Deedle member and `ApplicationBaseDirectory` are the only parts of the probe that change.
      `CountLoadedAssembliesNamed`, `CountAssemblyResolveHandlers`, `InstallProductionFallback`,
      `TryLoadDisplayName` and `ConfigurationFileNetstandardEntryCount` are left exactly as they stand, and
      the fail-loud rule recorded at `[P0-T13]` continues to bind `CountAssemblyResolveHandlers`.

      Acceptance: the file exists, contains `: MarshalByRefObject`, and
      `Select-String -SimpleMatch -CaseSensitive` on `TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs`
      returns each of the following exact counts.

      Discriminating zero-hit assertions. Each of these four literals is present in this file in the tree
      as it stands now, at the line given, so a zero count is a measurement of this task's work rather than
      a condition that was already satisfied: exactly 0 hits for `Deedle.Reflection`, present at lines 165
      and 193; exactly 0 hits for `convertRecordSequence`, present at lines 165 and 226; exactly 0 hits for
      `AmbiguousMatchException`, present at line 236; and exactly 0 hits for `ResolveConvertRecordSequence`,
      present at lines 194 and 224.

      Discriminating at-least-one assertions. Each of these three literals is absent from this file in the
      tree as it stands now, and is quoted here in prose for that reason: at least 1 hit for `Deedle.Frame`,
      at least 1 hit for `FromRecords`, and at least 1 hit for `ApplicationBaseDirectory`.

      Standing guards, carried forward and stated as guards rather than as new measurements because the
      tree already satisfies them: at least 1 hit each for `DeedleRecordConversionOutcome`,
      `InvokedOutcome`, `BindFailurePrefix`, `OtherFailurePrefix`, `INVOKED-NO-EXCEPTION`,
      `NETSTANDARD-BIND-FAILURE:`, `OTHER-FAILURE:`, `TargetInvocationException` and `MakeGenericMethod`;
      at least 2 hits for `DeedleProbeRecord`; at least 3 hits for `InvalidOperationException`; and exactly
      0 hits for `DeedleTypeInitializerOutcome`, `RunClassConstructor`, `OkOutcome`,
      `System.Runtime.CompilerServices`, `FluentAssertions` and `Microsoft.VisualStudio.TestTools`. These
      eleven guards were discriminating in Revision R2 and were discharged by the Revision R2 execution;
      they are retained so that a re-run of this task cannot regress them, and they are labelled as guards
      so no reader mistakes them for evidence that this task ran.

      Each asserted token is a single identifier, a single dotted identifier or a single short string
      literal rather than a multi-word phrase, so no CSharpier reflow of the surrounding statement can
      split it across two lines.
- [x] [P2-T6] Amend `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` in place — the file
      already exists from the version 1.0 execution of this plan and must NOT be recreated — declaring
      `[TestClass] public class NetstandardBindChildDomainTests` in namespace `TaskMaster.Test.Bootstrap`,
      creating each child domain with `AppDomain.CreateDomain`, and unloading every created domain in
      `[TestCleanup]` with `AppDomain.Unload`.

      **Revision R5 amendment: the child domains are re-rooted.** The rationale and the measurement are in
      `## R6.1` and `## R6.2`. Every child domain this class creates uses an `AppDomainSetup` whose
      `ApplicationBase` is the `QuickFiler.Test` build output directory and whose `ConfigurationFile` is
      `QuickFiler.Test.dll.config` in that same directory. Specify, exactly:

      - A private static read-only expression that computes the probe application base as
        `Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..",
        "QuickFiler.Test", "bin", "Debug"))`. The host base directory is `<repo>\TaskMaster.Test\bin\Debug\`
        and `QuickFiler.Test/QuickFiler.Test.csproj` line 36 declares `<OutputPath>bin\Debug\</OutputPath>`,
        so three parent steps reach the repository root. The expression is fixed here and is not the
        executor's to choose.
      - A private helper run before every `AppDomain.CreateDomain` call that throws
        `InvalidOperationException` naming the missing path when the computed directory does not exist, or
        when `QuickFiler.Test.dll.config`, `Deedle.dll` or `FSharp.Core.dll` is missing from it. It fails
        loudly and never skips: a skipped precondition here would make every result in this class vacuous.
      - The probe is instantiated with
        `domain.CreateInstanceFromAndUnwrap(typeof(ChildDomainBindProbe).Assembly.Location,
        typeof(ChildDomainBindProbe).FullName)`, **not** with `CreateInstanceAndUnwrap`.
        `TaskMaster.Test.dll` is not present in the new `ApplicationBase`, measured, so a display-name
        creation would raise `FileNotFoundException` before any observation was taken.
      - The two paths the class passes to the probe are re-pointed to the same directory: the
        configuration path handed to `ConfigurationFileNetstandardEntryCount` becomes
        `QuickFiler.Test.dll.config` under the computed probe application base, and the path handed to
        `DeedleRecordConversionOutcome` becomes `Deedle.dll` under that same directory. Both currently
        resolve against `AppDomain.CurrentDomain.BaseDirectory`, which is the host test assembly's own
        output directory, and leaving either there would measure a different directory from the one the
        child domain is rooted at.

      The file must not name `TaskMaster.dll.config`, which after Phase 3 carries the `netstandard`
      redirect and whose selection would void the negative control. That prohibition is retained unchanged
      across the re-rooting and is the reason `QuickFiler.Test.dll.config` is named explicitly rather than
      derived from the host assembly's own name. Test methods, named exactly:
      `ChildDomain_HasNoSvgControlAssemblyLoaded`,
      `ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall`,
      `ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect`,
      `ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory`,
      `AfterInstall_BothNetstandardVersionsBind`, `AfterInstall_DeedleTypeInitializerSucceeds`,
      `NegativeControl_WithoutInstall_Netstandard21Throws`,
      `NegativeControl_Netstandard20Observation_IsRecorded`, and
      `NegativeControl_HasNoUtilitiesCsAssemblyLoaded`, which runs in the installer-free domain and
      asserts that `CountLoadedAssembliesNamed("UtilitiesCS")` returns 0. This is the checkable form
      of the design claim that keeping the installer call in its own probe method prevents
      `UtilitiesCS` from being JIT-resolved in that domain; without it the claim is prose.

      `ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory` is new in Revision R5 and is the ninth
      method. It runs in the POSITIVE domain, before `InstallProductionFallback()` because the installer
      cannot change an `ApplicationBase`, and asserts three things: that
      `probe.ApplicationBaseDirectory()` trimmed of any trailing directory separator equals the computed
      probe application base trimmed the same way, compared with `StringComparison.OrdinalIgnoreCase`
      because Windows paths are case-insensitive; and, in the PARENT domain with `File.Exists`, that
      `Deedle.dll` and `FSharp.Core.dll` are both present in that directory. The two file checks are made
      in the parent rather than in the child so that no filesystem helper is added to the child domain's
      loaded set. This is criterion 6 in `## R1`: without it, a silent regression of the `ApplicationBase`
      back to the host assembly's own directory would make every positive result in this class vacuous
      again, which is the failure that consumed two fail-before rounds.

      Domain assignment is fixed by this plan and is not the executor's choice.
      `ChildDomain_HasNoSvgControlAssemblyLoaded`,
      `ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall` and
      `ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect` run in the POSITIVE domain, and
      `ChildDomain_HasNoSvgControlAssemblyLoaded` calls `InstallProductionFallback()` first and
      `CountLoadedAssembliesNamed("SVGControl")` second, in that order, so it observes the domain in
      the state in which `AfterInstall_BothNetstandardVersionsBind` measures the bind. Asserting it
      before the installer call, or in the installer-free domain, would pass because `UtilitiesCS` was
      never loaded, and would establish nothing about the domain the positive result comes from.
      `ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall` is the one method that observes a handler
      count in the positive domain BEFORE `InstallProductionFallback()`, which is what its name states.
      `NegativeControl_WithoutInstall_Netstandard21Throws` and
      `NegativeControl_Netstandard20Observation_IsRecorded` run in the installer-free domain.
      `ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory` runs in the POSITIVE domain and does not
      call `InstallProductionFallback()` at all: an `ApplicationBase` is fixed at domain creation and no
      later call can change it, so ordering relative to the installer is not merely unconstrained here but
      meaningless, and omitting the call keeps that domain's loaded set at its minimum for this
      observation.

      `AfterInstall_BothNetstandardVersionsBind` and `AfterInstall_DeedleTypeInitializerSucceeds` run in
      the POSITIVE domain, and each calls `InstallProductionFallback()` before the observation it makes,
      in that order. `ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect` runs in the POSITIVE
      domain and its position relative to `InstallProductionFallback()` is deliberately unconstrained,
      because it reads the configuration file supplied at domain creation and the installer neither
      reads nor writes that file. Every one of the nine methods therefore has a stated domain, and
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
      **Revision R2 amendment (Defect 1): one test body, and nothing else in this file.** Only
      `AfterInstall_DeedleTypeInitializerSucceeds` changes. Its name, its POSITIVE domain assignment and
      its `InstallProductionFallback()`-before-observation ordering are all unchanged, which is why AC10,
      `[P2-T11]`, `[P2-T12]`, `[P4-T3]` and `[P4-T6]` continue to name it without amendment. Inside that
      method:

      1. The observation call becomes `probe.DeedleRecordConversionOutcome(DeedlePath)`. The version 1.0
         call to `DeedleTypeInitializerOutcome` no longer compiles, which is deliberate: renaming the
         probe member makes a half-applied amendment a build failure rather than a silently stale
         observation.
      2. The outcome string is written to the TRX standard output with
         `TestContext.WriteLine("DEEDLE_RECORD_CONVERSION_OUTCOME=" + outcome);`, before the assertions,
         so the observed value is recoverable from the TRX whether the test passes or fails. `[P4-T6]`
         reads exactly that line.
      3. Two assertions are made, in this order. First
         `outcome.Should().NotStartWith(ChildDomainBindProbe.BindFailurePrefix, ...)`, which is the
         load-bearing class assertion: it is what fails on the unfixed tree and what distinguishes a
         `netstandard` bind failure from every other outcome. Second
         `outcome.Should().Be(ChildDomainBindProbe.InvokedOutcome, ...)`, which is the completion
         assertion. The order matters and is fixed here rather than left open: FluentAssertions reports
         the first failing assertion, so a bind failure produces a message naming the bind class, while
         an unrelated functional exception passes the first assertion and fails the second with a message
         carrying the `OTHER-FAILURE:` token and the real exception type. Neither failure can be mistaken
         for the other. Asserting only the second would make a bind failure and an unrelated throw
         indistinguishable; asserting only the first would let a remedy that satisfies the bind but
         cannot complete the conversion report as a pass.
      4. The method's XML summary is updated to describe a member invocation rather than a class
         constructor run, so the file does not contradict itself.

      The `ISOLATION-LOST-INVARIANT` comment block that `[P2-T7]` placed immediately above
      `NegativeControl_WithoutInstall_Netstandard21Throws` is not touched by this amendment and must
      survive it; the acceptance condition below re-asserts it so a re-run of this task cannot drop it.

      Acceptance: the file exists, contains exactly those nine method names, and contains
      `AppDomain.CreateDomain` and `AppDomain.Unload`. All counts below are taken with
      `Select-String -SimpleMatch -CaseSensitive` on
      `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`.

      Discriminating assertions on the Revision R5 re-rooting. Each literal's count in the tree as it
      stands now is stated, so each condition is a measurement of this task's work rather than a condition
      already satisfied: exactly 1 hit for `QuickFiler.Test.dll.config`, absent from this file now and
      quoted here in prose for that reason; exactly 1 hit for
      `ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory`, likewise absent now and quoted here;
      at least 1 hit for `ApplicationBaseDirectory`, likewise absent now and quoted here; exactly 1 hit for
      `CreateInstanceFromAndUnwrap`, likewise absent now and quoted here; exactly 0 hits for
      `TaskMaster.Test.dll.config`, present at line 47 now; and exactly 0 hits for
      `CreateInstanceAndUnwrap`, present at line 324 now. The two zero-hit literals and
      `CreateInstanceFromAndUnwrap` do not collide as substrings: `CreateInstanceFromAndUnwrap` does not
      contain `CreateInstanceAndUnwrap`, and `TaskMaster.Test.dll.config` does not contain
      `TaskMaster.dll.config`.

      Standing guards, carried forward and stated as guards because the tree already satisfies them; the
      acceptance condition is that this amendment leaves every one of them at its stated count: exactly 0
      hits for `TaskMaster.dll.config`; exactly 1 hit for `[DoNotParallelize]`; exactly 1 hit for
      `CountLoadedAssembliesNamed("SVGControl")`; exactly 2 hits for
      `CountLoadedAssembliesNamed("UtilitiesCS")`; exactly 2 hits for `CountAssemblyResolveHandlers()`;
      exactly 1 hit for `ISOLATION-LOST-INVARIANT`, which `[P2-T7]` created and which this amendment must
      not drop; at least 1 hit for `DeedleRecordConversionOutcome`; exactly 1 hit for
      `DEEDLE_RECORD_CONVERSION_OUTCOME=`; at least 1 hit each for `BindFailurePrefix` and
      `InvokedOutcome`; and exactly 0 hits for `DeedleTypeInitializerOutcome` and for `OkOutcome`.

      Every asserted token is a single identifier, a single dotted file name, a single attribute literal or
      a single short call expression written on one source line, so no CSharpier reflow can split one
      across two lines.
- [x] [P2-T7] Add the in-file isolation warning required by the spec's negative-control criterion, worded
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
- [x] [P2-T8] Create `TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs` declaring
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
- [x] [P2-T9] Register the three new harness files. Insert
      `<Compile Include="Bootstrap\ChildDomainBindProbe.cs" />`,
      `<Compile Include="Bootstrap\NetstandardBindChildDomainTests.cs" />` and
      `<Compile Include="Bootstrap\AddInEagerInstallShapeTests.cs" />` into the `ItemGroup` in
      `TaskMaster.Test/TaskMaster.Test.csproj` that already contains
      `<Compile Include="Ribbon\RibbonCommandBoundaryTests.cs" />` at line 323.
      Acceptance: on `TaskMaster.Test/TaskMaster.Test.csproj`,
      `Select-String -SimpleMatch -CaseSensitive` returns exactly 1 hit for each of the three file names
      `ChildDomainBindProbe.cs`, `NetstandardBindChildDomainTests.cs` and
      `AddInEagerInstallShapeTests.cs`.
- [x] [P2-T10] Re-run after the Revision R5 amendments to `[P2-T5]` and `[P2-T6]`, both of which change
      compiled source after the build recorded by the Revision R2 execution of this task. `[P2-T1]` is
      unchanged by Revision R5 and is not a reason for this re-run.
      The artifact at the path named below is overwritten by this re-run.
      LOCK-ACQUIRE, then create the evidence/regression-testing directory this task and `[P2-T11]` redirect into,
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
- [x] [P2-T11] [expect-fail] LOCK-ACQUIRE, then run the harness class alone and capture the TRX, then
      LOCK-RELEASE.

      First, remove every TRX already in the results directory and record the emptied count:

      ```
      pwsh -NoProfile -Command '
      $d = "TestResults/p2-expect-fail"
      foreach ($f in @(Get-ChildItem -LiteralPath $d -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue)) { Remove-Item -LiteralPath $f.FullName -Force }
      Write-Output ("PRERUN_TRX_COUNT=" + @(Get-ChildItem -LiteralPath $d -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue).Count)
      '
      ```

      The removal is a per-file `Remove-Item` over the enumerated `.trx` matches and deliberately not a
      recursive delete of the directory, so nothing other than the files that reader selects can be
      removed. `-ErrorAction SilentlyContinue` on both enumerations makes the span succeed and still emit
      `PRERUN_TRX_COUNT=0` when the directory is absent.

      Then run the harness class:

      ```
      pwsh -NoProfile -Command '
      $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
      $vstest = @(& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe")[0]
      & $vstest "TaskMaster.Test/bin/Debug/TaskMaster.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~TaskMaster.Test.Bootstrap" "/Logger:trx;LogFileName=p2-expect-fail.trx" /ResultsDirectory:TestResults/p2-expect-fail
      $LASTEXITCODE
      '
      ```

      The `;` inside `"/Logger:trx;LogFileName=p2-expect-fail.trx"` must stay inside the double quotes.
      Unquoted, PowerShell reads `;` as a statement separator and would terminate the `& $vstest` call
      at `/Logger:trx`, so the run would write an ambient-named TRX and then attempt to execute
      `LogFileName=p2-expect-fail.trx` as a command.

      Then read the pinned TRX in `TestResults/p2-expect-fail` and record the per-test outcomes:

      ```
      pwsh -NoProfile -Command '
      $m = @(Get-ChildItem -LiteralPath "TestResults/p2-expect-fail" -Filter "p2-expect-fail.trx" -Recurse)
      Write-Output ("TRX_MATCH_COUNT=" + $m.Count)
      $trx = $m[0]
      $x = [xml](Get-Content -LiteralPath $trx.FullName -Raw)
      foreach ($r in @($x.TestRun.Results.UnitTestResult)) { Write-Output ($r.testName + " OUTCOME=" + $r.outcome) }
      '
      ```

      Then, in a third span, extract the fail-before failure CLASS from the same TRX, so the artifact
      records why the Deedle line failed and not merely that it failed:

      ```
      pwsh -NoProfile -Command '
      $m = @(Get-ChildItem -LiteralPath "TestResults/p2-expect-fail" -Filter "p2-expect-fail.trx" -Recurse)
      Write-Output ("TRX_MATCH_COUNT=" + $m.Count)
      $trx = $m[0]
      $x = [xml](Get-Content -LiteralPath $trx.FullName -Raw)
      foreach ($r in @($x.TestRun.Results.UnitTestResult)) {
      if ($r.testName -eq "AfterInstall_DeedleTypeInitializerSucceeds") {
      foreach ($line in @(($r.Output.StdOut -split "`r?`n"))) {
      if ($line.StartsWith("DEEDLE_RECORD_CONVERSION_OUTCOME=")) { Write-Output $line } } } }
      '
      ```

      **Why the TRX name is pinned, and why the directory is emptied first.** `TestResults/p2-expect-fail`
      currently holds two superseded TRX files. The first is
      `DanMoisan_MEGALODON4_2026-09-13_23_35_52_net481.trx`, written by the version 1.0 run, which
      recorded the vacuous `AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Passed` and carries no
      `DEEDLE_RECORD_CONVERSION_OUTCOME=` line. `Get-ChildItem` returns results in name-ascending rather
      than write-time order, so an unpinned `-Filter "*.trx"` followed by `[0]` would select it. The
      second is `p2-expect-fail.trx` itself, written by the Revision R2 re-run on 2026-09-14, recording
      `AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Passed` and
      `DEEDLE_RECORD_CONVERSION_OUTCOME=INVOKED-NO-EXCEPTION` and no
      `ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory` result. Pinning the name does not
      discriminate against that second file: `TRX_MATCH_COUNT=1` reads `1` whether this run wrote a fresh
      TRX or wrote none, and whether `LogFileName=` overwrites has not been observed in this worktree,
      because the Revision R2 run created that file rather than overwriting one. The `.trx` files are
      therefore removed before the run, so `TRX_MATCH_COUNT=1` afterwards can only count a file this run
      wrote. The literal `p2-expect-fail.trx` is quoted here in prose because it is removed before this
      task's run writes it.

      Write `.../evidence/regression-testing/expect-fail-run.2026-09-13T18-22.md` with `Timestamp:`,
      `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 1` and an `Output Summary:` reproducing every
      `OUTCOME=` line verbatim, the `PRERUN_TRX_COUNT=` line emitted by the first span, the
      `TRX_MATCH_COUNT=` line, and the single `DEEDLE_RECORD_CONVERSION_OUTCOME=` line.

      **This is a Revision R5 re-run and it overwrites the artifact at that path.** `[P1-T5]` must
      already have copied the superseded version 1.0 artifact to
      `.../evidence/regression-testing/expect-fail-run-superseded-probe-surface.2026-09-13T18-22.md`. If
      that copy is absent, this task does not run: the record of the vacuous pass that produced Defect 1
      would be destroyed. The Revision R2 run's own artifact, which recorded
      `DEEDLE_RECORD_CONVERSION_OUTCOME=INVOKED-NO-EXCEPTION` and `Acceptance Condition: NOT MET`, is the
      current content of the plan-named path and is overwritten here; the observation it carries is
      reproduced in `## R6.1` and in the Revision R2 measurement table there, so no measurement is lost.

      Acceptance: the artifact records `PRERUN_TRX_COUNT=0`, taken before the run, which is what makes
      the `TRX_MATCH_COUNT=1` below a measurement of this run rather than of a residue; the artifact
      records `TRX_MATCH_COUNT=1`,
      `ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory OUTCOME=Passed`,
      `AfterInstall_BothNetstandardVersionsBind OUTCOME=Failed`,
      `AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Failed`, and exactly one
      `DEEDLE_RECORD_CONVERSION_OUTCOME=` line whose value begins with `NETSTANDARD-BIND-FAILURE:`. These
      are the fail-before observations: they fail at runtime against a behaviour-empty installer, not at
      compile time. The first of the four is the Revision R5 addition and it is ordered first
      deliberately: if the child domain is not rooted at the `QuickFiler.Test` build output directory then
      the other three describe a domain in which the bind under test is not reachable, which is exactly
      how two earlier fail-before attempts produced a success token against an unfixed build. The
      `DEEDLE_RECORD_CONVERSION_OUTCOME=` condition is what makes the `OUTCOME=Failed` condition above it
      non-vacuous: it states that the Deedle line failed because the `netstandard` bind is unsatisfiable
      and not because of an unrelated exception. The run's own exit code is recorded against
      `ExpectedExitCode: 1` and is not itself a gate. If the `DEEDLE_RECORD_CONVERSION_OUTCOME=` value
      begins with `OTHER-FAILURE:` the executor halts and reports blocked rather than adapting: the probe
      reached an exception that is not the bind, and the plan, not the run, needs correcting. If
      `ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory` records anything other than `Passed` the
      executor halts and reports blocked for the same reason, without evaluating the other three
      conditions as evidence.
- [x] [P2-T12] Decisive net481 isolation check, taken before the fix exists so it cannot be confounded by
      it. Read the `[P2-T11]` artifact. This task is re-run under Revision R5 and MUST read the
      REGENERATED `[P2-T11]` artifact, not the superseded copy `[P1-T5]` preserved and not the Revision R2
      content of the plan-named path: the isolation outcomes are properties of the run, and the run
      changed. This task's own artifact at the path named below is overwritten by the re-run.
      Acceptance: it records all six of
      `ChildDomain_HasNoSvgControlAssemblyLoaded OUTCOME=Passed`,
      `ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall OUTCOME=Passed`,
      `ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect OUTCOME=Passed`,
      `ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory OUTCOME=Passed`,
      `NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed` and
      `NegativeControl_HasNoUtilitiesCsAssemblyLoaded OUTCOME=Passed`. The fourth is the Revision R5
      addition and corresponds to criterion 6 in `## R1`. Write
      `.../evidence/regression-testing/isolation-field-decisive-check.2026-09-13T18-22.md` with
      `Timestamp:`, `Command: (read of the [P2-T11] artifact)`, `EXIT_CODE: 0` and an `Output Summary:`
      reproducing those six lines plus the sentence
      `The private AppDomain assembly-resolution field is present and readable on net481; the isolation
      assertion did not skip.` If any of the six is not `Passed`, the executor halts and reports blocked:
      the harness has no isolation and no later positive result would mean anything.

### Phase 3 — Minimal Production Fix

- [x] [P3-T1] Implement the resolution ladder in
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
- [x] [P3-T2] Implement `Install()` in the same file so that it subscribes the handler to
      `AppDomain.CurrentDomain.AssemblyResolve` exactly once per AppDomain, guarded by
      `Interlocked.Exchange` on the private counter, and so that it never throws: an exception escaping it
      becomes a `TypeInitializationException` on `ThisAddIn` and would take the whole add-in down. The
      subscription and the `Interlocked.Exchange` guard are already present from `[P2-T1]`, which is
      what makes `[P2-T6]`'s handler-count control observable in the `[P2-T11]` run; this task's
      obligation is that `Install()` attaches exactly one handler however many times it is called and
      never throws, which is measured by the two named tests rather than by inspection.
      Acceptance: the idempotence test and the never-throws test in
      `UtilitiesCS.Test.Bootstrap.AssemblyBindingFallbackTests` pass in `[P4-T2]`.
- [x] [P3-T3] Add the eager installation point. In `TaskMaster/ThisAddIn.cs`, inside
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
- [x] [P3-T4] Add the declarative hardening. In `TaskMaster/app.config`, inside the existing
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
- [x] [P3-T5] Record that the hardening is not the fix. Verify that `spec.md` already states the ground,
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

- [x] [P4-T1] LOCK-ACQUIRE, rebuild the solution with the plain Debug configuration, LOCK-RELEASE. Same
      pwsh shape as `[P2-T10]`, redirecting to
      `.../evidence/regression-testing/pass-after-build-console.2026-09-13T18-22.txt`.
      Acceptance: the console log contains at least one line matching `^\s+0 Error\(s\)$`.
- [x] [P4-T2] LOCK-ACQUIRE, run the `UtilitiesCS.Test` ladder unit tests alone, LOCK-RELEASE.

      First, remove every TRX already in the results directory and record the emptied count:

      ```
      pwsh -NoProfile -Command '
      $d = "TestResults/p4-ladder"
      foreach ($f in @(Get-ChildItem -LiteralPath $d -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue)) { Remove-Item -LiteralPath $f.FullName -Force }
      Write-Output ("PRERUN_TRX_COUNT=" + @(Get-ChildItem -LiteralPath $d -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue).Count)
      '
      ```

      `TestResults/p4-ladder` does not exist in the tree as it stands, so on a first execution this span
      enumerates nothing and emits `PRERUN_TRX_COUNT=0` — `-ErrorAction SilentlyContinue` on both
      enumerations is what makes an absent directory a zero count rather than an error. The span is
      nonetheless required, because the state that matters is the one this task is in on a RE-execution,
      when the directory holds this task's own earlier TRX under the pinned name. Pinning the name does
      not discriminate against that file: `TRX_MATCH_COUNT=1` reads `1` whether this run wrote a fresh
      TRX or wrote none, so a run that failed to emit for any reason — testhost crash, build-lock
      timeout, discovery failure — would leave the reader replaying the earlier run's outcomes as though
      they were this run's. That is exactly the exposure `[P2-T11]` was found to carry after its own
      re-run. Removing first makes `TRX_MATCH_COUNT=1` a count of a file this run wrote. The removal is a
      per-file `Remove-Item` over the enumerated `.trx` matches and deliberately not a recursive delete of
      the directory.

      Then run the ladder tests:

      ```
      pwsh -NoProfile -Command '
      $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
      $vstest = @(& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe")[0]
      & $vstest "UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~UtilitiesCS.Test.Bootstrap" "/Logger:trx;LogFileName=p4-ladder.trx" /ResultsDirectory:TestResults/p4-ladder
      $LASTEXITCODE
      '
      ```

      The `;` inside `"/Logger:trx;LogFileName=p4-ladder.trx"` must stay inside the double quotes, for
      the reason `[P2-T11]` states: unquoted, PowerShell reads it as a statement separator.

      Read the TRX in `TestResults/p4-ladder` with the same pinned reader `[P2-T11]` uses, substituting
      `TestResults/p4-ladder` and `p4-ladder.trx` for its two literals, so the reader is
      `@(Get-ChildItem -LiteralPath "TestResults/p4-ladder" -Filter "p4-ladder.trx" -Recurse)` with the
      same `TRX_MATCH_COUNT=` emission before `[0]` is taken. A `-Filter "*.trx"` selection is
      prohibited here for the reason `[P2-T11]` records: `Get-ChildItem` orders by name, not by write
      time, so a re-run leaves the task reading whichever TRX name sorts first. Write
      `.../evidence/regression-testing/pass-after-ladder.2026-09-13T18-22.md` with the four required
      fields plus the `PRERUN_TRX_COUNT=` line emitted by the first span, the `TRX_MATCH_COUNT=` line and
      every `OUTCOME=` line. The literal
      `p4-ladder.trx` is quoted here in prose because it is absent from the tree until this task runs.
      Acceptance: `EXIT_CODE: 0`, the artifact records `PRERUN_TRX_COUNT=0`, taken before the run, which
      is what makes the `TRX_MATCH_COUNT=1` below a measurement of this run rather than of a residue; the
      artifact records `TRX_MATCH_COUNT=1`, the TRX `ResultSummary`
      `outcome` is `Completed`, the `Counters` `failed` value is `0`, and the `passed` value is at
      least 11.

      **Revision R7 note — no state change, no acceptance change.** This task remains checked and is NOT
      re-executed. Its recorded run measured the ladder suite as it stood before Revision R7 added a
      sibling test class, and its "at least 11" floor is still satisfied by that assembly, so nothing it
      recorded has become false. Re-executing it in place was considered and rejected on an ordering
      ground: this task sits at numeric position 2 of Phase 4 while the tasks that author the new tests
      are appended at positions 13 to 18, so an executor working the list in order would re-run it against
      an assembly that does not yet contain those tests, and a raised floor here would be unsatisfiable at
      the moment it ran. `[P4-T17]` re-takes the same measurement after the new tests exist, with a floor
      of 21 passed, against its own pinned TRX name, its own results directory and its own artifact, and
      supersedes this task as the ladder suite's measurement of record. `## R7.6` records the reasoning.
- [x] [P4-T3] LOCK-ACQUIRE, run the child-domain harness class alone, LOCK-RELEASE.

      First, remove every TRX already in the results directory and record the emptied count:

      ```
      pwsh -NoProfile -Command '
      $d = "TestResults/p4-harness"
      foreach ($f in @(Get-ChildItem -LiteralPath $d -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue)) { Remove-Item -LiteralPath $f.FullName -Force }
      Write-Output ("PRERUN_TRX_COUNT=" + @(Get-ChildItem -LiteralPath $d -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue).Count)
      '
      ```

      `TestResults/p4-harness` does not exist in the tree as it stands, so on a first execution this span
      enumerates nothing and emits `PRERUN_TRX_COUNT=0`; `-ErrorAction SilentlyContinue` on both
      enumerations is what makes an absent directory a zero count rather than an error. The span is
      required for the re-execution state, in which the directory holds this task's own earlier TRX under
      the pinned name and `TRX_MATCH_COUNT=1` reads `1` whether this run wrote a fresh TRX or wrote none.
      The consequence here is larger than in `[P4-T2]`: `[P4-T4]`, `[P4-T5]` and `[P4-T6]` all read this
      task's artifact or this same TRX, so a silently replayed earlier run would propagate into four
      acceptance conditions. Removing first makes `TRX_MATCH_COUNT=1` a count of a file this run wrote.
      The removal is a per-file `Remove-Item` over the enumerated `.trx` matches and deliberately not a
      recursive delete of the directory.

      Then run the harness class. Same command shape as
      `[P2-T11]`, which means the same `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll` assembly operand,
      the same `/InIsolation`, the same `/TestCaseFilter:"FullyQualifiedName~TaskMaster.Test.Bootstrap"`,
      and the same `/Settings:scripts/vscode/TaskMaster.cli.runsettings` operand and not the
      repository-root `TaskMaster.runsettings`. The shape is no longer identical in one operand, so that
      operand is named here rather than inherited: the logger operand is
      `"/Logger:trx;LogFileName=p4-harness.trx"`, with `/ResultsDirectory:TestResults/p4-harness`. The
      `;` must stay inside the double quotes, for the reason `[P2-T11]` states.
      Read the TRX with the same pinned reader `[P2-T11]` uses, substituting `TestResults/p4-harness`
      and `p4-harness.trx` for its two literals, so the reader is
      `@(Get-ChildItem -LiteralPath "TestResults/p4-harness" -Filter "p4-harness.trx" -Recurse)` with
      the same `TRX_MATCH_COUNT=` emission before `[0]` is taken. A `-Filter "*.trx"` selection is
      prohibited here for the reason `[P2-T11]` records. Write
      `.../evidence/regression-testing/pass-after-harness.2026-09-13T18-22.md` with the four required
      fields plus the `PRERUN_TRX_COUNT=` line emitted by the first span, the `TRX_MATCH_COUNT=` line and
      every `OUTCOME=` line. The literal `p4-harness.trx`
      is quoted here in prose because it is absent from the tree until this task runs.
      Acceptance: `EXIT_CODE: 0`, the artifact records `PRERUN_TRX_COUNT=0`, taken before the run, which
      is what makes the `TRX_MATCH_COUNT=1` below a measurement of this run rather than of a residue; the
      artifact records `TRX_MATCH_COUNT=1`, the `Counters` `failed`
      value is `0`, and the artifact records `OUTCOME=Passed` for all nine method names listed in
      `[P2-T6]`, which includes the Revision R5 addition
      `ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory`. Nine rather than eight: a run reporting
      eight passes and no ninth result would mean the re-rooting assertion did not execute, which is the
      state in which every other result in this run is vacuous.
- [x] [P4-T4] Verify the load-bearing negative control specifically. Read the `[P4-T3]` artifact.
      Acceptance: it records `NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed`. If it
      records any other outcome, the executor halts and reports blocked, because the load in the
      negative-control domain succeeded without a code change, which means isolation has been lost and
      every positive result in `[P4-T3]` is vacuous.
- [x] [P4-T5] Verify the both-versions criterion specifically. Read the `[P4-T3]` artifact.
      Acceptance: it records `AfterInstall_BothNetstandardVersionsBind OUTCOME=Passed`. This is the
      criterion that forbids reporting a `2.1.0.0`-only remedy as a fix.
- [x] [P4-T6] Verify the Deedle end-to-end criterion specifically. Read the `[P4-T3]` artifact, then
      extract the outcome VALUE the probe reported, so this criterion is recorded as a measured class and
      not only as a pass mark. Command:

      ```
      pwsh -NoProfile -Command '
      $m = @(Get-ChildItem -LiteralPath "TestResults/p4-harness" -Filter "p4-harness.trx" -Recurse)
      Write-Output ("TRX_MATCH_COUNT=" + $m.Count)
      $trx = $m[0]
      $x = [xml](Get-Content -LiteralPath $trx.FullName -Raw)
      foreach ($r in @($x.TestRun.Results.UnitTestResult)) {
      if ($r.testName -eq "AfterInstall_DeedleTypeInitializerSucceeds") {
      foreach ($line in @(($r.Output.StdOut -split "`r?`n"))) {
      if ($line.StartsWith("DEEDLE_RECORD_CONVERSION_OUTCOME=")) { Write-Output $line } } } }
      '
      ```

      Append the emitted line to
      `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/pass-after-harness.2026-09-13T18-22.md`
      under a `Deedle Record Conversion Outcome:` heading, together with the `TRX_MATCH_COUNT=` line
      this span emits. The TRX name is pinned rather than selected with `-Filter "*.trx"` for the
      reason `[P2-T11]` records: `Get-ChildItem` orders by name, not by write time.
      Acceptance: the appended `TRX_MATCH_COUNT=` line reads `TRX_MATCH_COUNT=1`, the `[P4-T3]` artifact
      records `AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Passed`, the heading exists, and it
      carries exactly one `DEEDLE_RECORD_CONVERSION_OUTCOME=` line whose value is exactly
      `INVOKED-NO-EXCEPTION`. A `NotExecuted` or `Inconclusive` outcome is a failure of this task, not a
      pass: the test is required to fail rather than skip when `Deedle.dll` or the member is absent from
      the domain's `ApplicationBase`. The recorded value is gated rather than merely observed because the
      pre-fix value recorded by `[P2-T11]` begins with `NETSTANDARD-BIND-FAILURE:` and the post-fix value
      is the completion token: the pair of recorded values is the fail-before/pass-after evidence for
      AC10, and a value beginning `OTHER-FAILURE:` would mean the remedy satisfied the bind but the
      conversion still could not complete, which is a stop-and-report condition rather than a pass.
      The pair is evidence only because both readings come from a child domain rooted at the
      `QuickFiler.Test` build output directory, which is what `## R6.1` measured and what
      `ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory` asserts in both runs. This task therefore
      additionally requires that the `[P4-T3]` artifact records
      `ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory OUTCOME=Passed`; without it the post-fix
      reading is not comparable with the pre-fix one.
- [x] [P4-T7] LOCK-ACQUIRE, run the `AddInEagerInstallShapeTests` class alone, LOCK-RELEASE.

      First, remove every TRX already in the results directory and record the emptied count:

      ```
      pwsh -NoProfile -Command '
      $d = "TestResults/p4-shape"
      foreach ($f in @(Get-ChildItem -LiteralPath $d -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue)) { Remove-Item -LiteralPath $f.FullName -Force }
      Write-Output ("PRERUN_TRX_COUNT=" + @(Get-ChildItem -LiteralPath $d -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue).Count)
      '
      ```

      `TestResults/p4-shape` does not exist in the tree as it stands, so on a first execution this span
      enumerates nothing and emits `PRERUN_TRX_COUNT=0`; `-ErrorAction SilentlyContinue` on both
      enumerations is what makes an absent directory a zero count rather than an error. The span is
      required for the re-execution state, in which the directory holds this task's own earlier TRX under
      the pinned name and `TRX_MATCH_COUNT=1` reads `1` whether this run wrote a fresh TRX or wrote none.
      Removing first makes `TRX_MATCH_COUNT=1` a count of a file this run wrote. The removal is a per-file
      `Remove-Item` over the enumerated `.trx` matches and deliberately not a recursive delete of the
      directory.

      Then run the class with
      `/Settings:scripts/vscode/TaskMaster.cli.runsettings`, and not the repository-root
      `TaskMaster.runsettings`, with
      `/ResultsDirectory:TestResults/p4-shape` and `/TestCaseFilter:"FullyQualifiedName~AddInEagerInstallShapeTests"`.
      Same command shape as `[P2-T11]`, which means the same assembly operand
      `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`, the same `/InIsolation`, and
      `"/Logger:trx;LogFileName=p4-shape.trx"` with `/ResultsDirectory:TestResults/p4-shape`. The `;`
      must stay inside the double quotes, for the reason `[P2-T11]` states. Read the TRX with the same
      pinned reader `[P2-T11]` uses, substituting `TestResults/p4-shape` and `p4-shape.trx`, so the
      reader is `@(Get-ChildItem -LiteralPath "TestResults/p4-shape" -Filter "p4-shape.trx" -Recurse)`
      with the same `TRX_MATCH_COUNT=` emission before `[0]` is taken, and record `TRX_MATCH_COUNT` in
      the artifact. The literal `p4-shape.trx` is quoted here in prose because it is absent from the
      tree until this task runs.
      Write `.../evidence/regression-testing/pass-after-shape.2026-09-13T18-22.md` with the
      four required fields plus the `PRERUN_TRX_COUNT=` line emitted by the first span, the
      `TRX_MATCH_COUNT=` line and every `OUTCOME=` line.
      Acceptance: `EXIT_CODE: 0`, the artifact records `PRERUN_TRX_COUNT=0`, taken before the run, which
      is what makes the `TRX_MATCH_COUNT=1` below a measurement of this run rather than of a residue; the
      artifact records `TRX_MATCH_COUNT=1`, and the artifact records
      `ThisAddIn_HasExplicitStaticConstructor OUTCOME=Passed` and
      `AppConfig_DeclaresNetstandardRedirect OUTCOME=Passed`.
- [x] [P4-T8] Static shape checks for the criteria a test cannot carry. Command:

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
- [x] [P4-T9] Determinism and no-filesystem-write sweep over exactly the four files this work adds under
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

      **Revision R7 note — no state change, no acceptance change.** This task remains checked with its
      four-path list and its recorded 68 banned-token lines intact. Revision R7 adds a fifth test file,
      `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs`, which this recorded run did not
      sweep because it did not exist. `[P4-T18]` runs the five-path version of exactly this command, and
      `[P5-T8]` is repointed to re-run `[P4-T18]`'s five-path version post-format rather than this
      four-path one, so the new file is swept twice and no recorded output is retrospectively re-labelled.
- [x] [P4-T10] Record the free `2.0.0.0` measurement that narrows the open risk. The `[P4-T3]` TRX
      reader emits `testName` and `outcome` only, so the observation is extracted from the TRX
      directly. Command:

      ```
      pwsh -NoProfile -Command '
      $m = @(Get-ChildItem -LiteralPath "TestResults/p4-harness" -Filter "p4-harness.trx" -Recurse)
      Write-Output ("TRX_MATCH_COUNT=" + $m.Count)
      $trx = $m[0]
      $x = [xml](Get-Content -LiteralPath $trx.FullName -Raw)
      foreach ($r in @($x.TestRun.Results.UnitTestResult)) {
      if ($r.testName -eq "NegativeControl_Netstandard20Observation_IsRecorded") {
      foreach ($line in @(($r.Output.StdOut -split "`r?`n"))) {
      if ($line.StartsWith("NETSTANDARD_2_0_0_0_NEGATIVE_DOMAIN_RESULT=")) { Write-Output $line } } } }
      '
      ```

      Write
      `.../evidence/other/netstandard-2-0-0-0-child-domain-observation.2026-09-13T18-22.md` with
      `Timestamp:`, `Command:`, `EXIT_CODE:` and an `Output Summary:` containing the `TRX_MATCH_COUNT=`
      line and exactly one line of the
      form `NETSTANDARD_2_0_0_0_NEGATIVE_DOMAIN_RESULT=` followed by the marshalled outcome string the
      probe returned, which is either `LOADED` or the exception type name. The TRX name is pinned rather
      than selected with `-Filter "*.trx"` for the reason `[P2-T11]` records.
      Acceptance: the artifact records `TRX_MATCH_COUNT=1` and carries exactly one such line with a
      non-empty value. The `NETSTANDARD_2_0_0_0_NEGATIVE_DOMAIN_RESULT` value
      itself is an observation, not a gate: either value is recorded and neither blocks.
- [x] [P4-T11] No-new-deployment sweep. Confirm no `netstandard.dll` entered any project or any
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
- [x] [P4-T12] File-size audit of every file this work creates or edits. Command:

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

      **Revision R7 note — no state change, no acceptance change.** This task remains checked with its
      six-path list and its recorded pre-format counts intact. Revision R7 adds a seventh file,
      `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs`. `[P4-T18]` runs the seven-path
      version of exactly this command, and `[P5-T8]` is repointed to re-run `[P4-T18]`'s seven-path version
      post-format rather than this six-path one. The remedy clause above — split into a second file, record
      a write-set amendment, register the new file in the owning `csproj` — is the mechanism Revision R7
      applies at `[P4-T13]`, `[P4-T14]` and `[P4-T15]`, before an overflow rather than after one.
- [x] [P4-T13] Create `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs`. This is a NEW
      file and a SIBLING of `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs`. **Do not write,
      reformat or otherwise touch that sibling file in this task or in any other Revision R7 task**: it is
      pinned by two standing guards in `[P2-T3]`, it is 383 lines against a 500-line ceiling, and it is
      compiled into the assembly `[P2-T11]`'s fail-before evidence does not depend on. `## R7.3` records
      why a sibling file rather than an amendment.

      Declare one `[TestClass]` named `AssemblyBindingFallbackEdgeCaseTests` in namespace
      `UtilitiesCS.Test.Bootstrap`. The namespace is load-bearing: `[P4-T17]` and `[P4-T2]` filter on
      `FullyQualifiedName~UtilitiesCS.Test.Bootstrap`, so a class in any other namespace is not discovered
      by either run. Use MSTest attributes, FluentAssertions for every assertion, and Arrange-Act-Assert
      structure with a short intent comment on each test, matching the sibling file's style.

      The file carries NO `#nullable enable` directive and no `?` annotation on any reference type, for the
      reason `## R4.3` states for the two `TaskMaster.Test` files: `[P5-T6]` promotes CS8632 to a build
      error in a file that has not opted in.

      **The class-level XML documentation comment must name `AppDomain.AssemblyResolve`**, stating that the
      tests below cover the decline paths of the resolution ladder and of the handler attached to that
      event, and that every one of them reaches its target through injected delegates or through a guard
      clause rather than through a real bind. This is not decoration: `[P4-T18]`'s determinism sweep carries
      a `CONTROL_AppDomain` positive control that counts occurrences of the token `AppDomain` in each swept
      file, and a file carrying none would make that control read zero and the sweep unsatisfiable. The
      comment is the natural place for the token because the event is what the tested code attaches to. Do
      not write the token `File.Exists` anywhere in the file, including in a comment, because the acceptance
      condition below gates it at zero and that search is textual.

      **Private helpers.** Declare, as `private static` members of the new class, a `CreateLadder` factory
      whose five parameters default to miss behaviours exactly as the sibling file's does, a `NameOf`
      factory that builds a fully specified `AssemblyName` from a simple name, a version and a token, a
      `Sentinel` assembly property returning `typeof(Uri).Assembly`, a `LoadedMscorlib` property returning
      `typeof(string).Assembly`, and the three constants for the `netstandard` token `cc7b13ffcd2ddd51`,
      the `mscorlib` token `b77a5c561934e089` and a fake runtime directory literal. These deliberately
      mirror the sibling file's private helpers rather than reusing them; add a one-line comment giving the
      reason, which is that the sibling's copies are `private` and that file must not be edited. Do not
      widen the sibling's members to `internal` to avoid the duplication.

      **The ten tests, named, with the construct each exists to cover.** Every one of them drives either the
      ladder's injected delegates or a guard clause that returns before the ladder is constructed. None
      calls `Assembly.Load`, `Assembly.LoadFrom` or `File.Exists`, so none touches the GAC or the file
      system, and the sibling file's
      `Install_ThenLoadOfUnresolvableName_LeavesTheLoadFailingWithoutHandlerThrowing` remains the only test
      in this work that drives a real bind.

      1. `Resolve_WhenRequestedIdentityIsNull_ReturnsNullBeforeAnyRung` — call the internal static seam
         `AssemblyBindingFallback.Resolve(null)` and assert the result is null. Covers the null-identity
         guard at `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` lines 105-106.
      2. `Resolve_WhenRequestedSimpleNameIsAbsent_ReturnsNullBeforeAnyRung` — call
         `AssemblyBindingFallback.Resolve(new AssemblyName())`, whose `Name` is null, and assert the result
         is null. Covers the empty-simple-name guard at lines 111-112. The parameterless `AssemblyName`
         constructor is the mechanism; do not attempt `new AssemblyName("")`, which throws.
      3. `LadderResolve_WhenRequestedIdentityIsNull_ReturnsNull` — `CreateLadder().Resolve(null)` returns
         null. Covers the ladder's own null guard at lines 229-230.
      4. `LadderResolve_WhenRuntimeFacadeFileIsAbsent_RungThreeDeclinesWithoutLoading` — a ladder whose
         load-by-display-name delegate throws `FileNotFoundException`, whose file-exists delegate returns
         false and whose load-from-path delegate records its invocations; resolve a `netstandard` identity
         at `Version=2.1.0.0` with the `netstandard` token. Assert the result is null and that the
         load-from-path delegate was never invoked. Covers the rung-3 early return at lines 322-323.
      5. `LadderResolve_WhenRuntimeFacadeLoadThrows_RungThreeAbsorbsAndDeclines` — a ladder whose
         load-by-display-name delegate throws, whose load-from-path delegate throws
         `BadImageFormatException`, and whose file-exists delegate returns true ONLY for a path beginning
         with the fake runtime directory and false otherwise; resolve the same `netstandard` identity.
         Assert the call does not throw and returns null. Covers the rung-3 rung-local catch at lines
         328-332. The path-discriminating file-exists delegate is what keeps rung 4 out of this test, so
         the covered set is exactly rung 3's catch.
      6. `LadderResolve_WhenRequestedSimpleNameIsAbsent_RungFourDeclinesOnTheNameGuard` —
         `CreateLadder().Resolve(new AssemblyName())` returns null after rungs 1 to 3 decline. Covers the
         rung-4 empty-simple-name guard at lines 345-346.
      7. `LadderResolve_WhenProbeDirectoryHoldsTheAssembly_RungFourLoadsItByPath` — a ladder whose
         file-exists delegate returns true and whose load-from-path delegate records the path and returns
         the sentinel assembly; resolve a NON-`netstandard` identity carrying the `mscorlib` token, so rung
         3 declines on its identity check. Assert the result is the sentinel and that the recorded path
         ends with the requested simple name plus `.dll`. Covers the rung-4 success path at line 365.
      8. `LadderResolve_WhenProbeDirectoryLoadThrows_RungFourAbsorbsAndDeclines` — identical to test 7
         except the load-from-path delegate throws `BadImageFormatException`. Assert the call does not
         throw and returns null. Covers the rung-4 rung-local catch at lines 367-371.
      9. `LadderResolve_WhenLoadedTokenLengthDiffers_RungOneRejectsTheLoadedAssembly` — build an
         `AssemblyName` for `mscorlib` and call `SetPublicKeyToken` with a three-byte array, so the request
         carries a non-empty token of a length no real assembly has; supply the already-loaded set as the
         single element `LoadedMscorlib`, whose token is eight bytes. Assert the result is null. Covers the
         token-length guard at lines 439-440.
      10. `OnAssemblyResolve_WhenEventArgsCarryNoName_ReturnsNullWithoutResolving` — obtain the private
          static `OnAssemblyResolve` method by reflection, bind it to a `ResolveEventHandler` with
          `Delegate.CreateDelegate` exactly as the sibling file's detach helper already does, then invoke
          it once with a null second argument and once with a `ResolveEventArgs` carrying
          `string.Empty`, asserting null both times. Covers the null-or-empty-name guard at lines 157-158.
          Both invocations return at that guard, so neither reaches a bind, a rung or the production ladder
          factory.

      Tests 7 and 8 are both required even though test 8 alone enters line 365 on its way to throwing: test
      7 reaches that line on a non-throwing path, which is what makes the covered-line claim independent of
      how the collector records a line whose statement throws part-way through.

      No test in this file mutates a static of the type under test. Tests 1, 2 and 10 return at a guard
      before `_resolvingSimpleName` is assigned at line 120, and no test calls `Install()`, so the class
      needs no `[TestCleanup]` and cannot interfere with the sibling class under the `ClassLevel` parallel
      scope both run in.

      Acceptance: the file exists. All counts below are taken with `Select-String -SimpleMatch
      -CaseSensitive` on `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs`. Exactly 10
      hits for `[TestMethod]`. At least 1 hit for `class AssemblyBindingFallbackEdgeCaseTests`. Exactly 1
      hit for `namespace UtilitiesCS.Test.Bootstrap`. At least 1 hit for each of the ten method names, each
      of which is a single identifier written on one source line that no CSharpier reflow can split. The
      "at least" form on the ten names and on the class name is deliberate: a doc comment on one test may
      legitimately name a sibling test, and an equality there would turn ordinary documentation into a
      failure. The ten names are:
      `Resolve_WhenRequestedIdentityIsNull_ReturnsNullBeforeAnyRung`,
      `Resolve_WhenRequestedSimpleNameIsAbsent_ReturnsNullBeforeAnyRung`,
      `LadderResolve_WhenRequestedIdentityIsNull_ReturnsNull`,
      `LadderResolve_WhenRuntimeFacadeFileIsAbsent_RungThreeDeclinesWithoutLoading`,
      `LadderResolve_WhenRuntimeFacadeLoadThrows_RungThreeAbsorbsAndDeclines`,
      `LadderResolve_WhenRequestedSimpleNameIsAbsent_RungFourDeclinesOnTheNameGuard`,
      `LadderResolve_WhenProbeDirectoryHoldsTheAssembly_RungFourLoadsItByPath`,
      `LadderResolve_WhenProbeDirectoryLoadThrows_RungFourAbsorbsAndDeclines`,
      `LadderResolve_WhenLoadedTokenLengthDiffers_RungOneRejectsTheLoadedAssembly` and
      `OnAssemblyResolve_WhenEventArgsCarryNoName_ReturnsNullWithoutResolving`. Exactly 0 hits for each of
      `Assembly.Load(`, `Assembly.LoadFrom(`, `File.Exists` and `#nullable enable`, which together are the
      no-real-bind, no-filesystem and no-nullable-opt-in gates. At least 1 hit for `AppDomain`, which is the
      token `[P4-T18]`'s `CONTROL_AppDomain` positive control counts in this file; a count of zero here
      would make that later sweep unsatisfiable, so it is gated at the point the file is authored rather
      than discovered two tasks later. Every literal in this acceptance condition
      is absent from the tree until this task runs and is quoted here in prose for that reason; the file
      itself does not exist yet, which is what makes every count above a measurement of this task's product
      rather than a condition already satisfied.
- [x] [P4-T14] Register the new unit-test file, and gate the `.csproj` boundary mechanically. Insert
      `<Compile Include="Bootstrap\AssemblyBindingFallbackEdgeCaseTests.cs" />` into the `ItemGroup` in
      `UtilitiesCS.Test/UtilitiesCS.Test.csproj` that already contains
      `<Compile Include="Bootstrap\AssemblyBindingFallbackTests.cs" />` at line 190. This project uses
      explicit `Compile` items with no wildcard glob; an unregistered file silently does not build.
      **Insert one line and change nothing else in this file.** In particular, do not touch the
      `FSharp.Core` `Reference` at lines 597-599: the `HintPath` split between `lib/netstandard2.0` and
      `lib/netstandard2.1` recorded at `## R6.1` is the root cause of this defect and is OUT OF SCOPE,
      tracked as a separate issue, and `## R6.4` records why.

      **Take the BEFORE reading first, before the insertion.** The gate below is a delta rather than an
      absolute, because the planner could not observe the file's current committed-versus-working state in
      its own session and will not assert a figure it has not measured. Run these two commands BEFORE
      editing the file and record their output as `PRE_INSERT`:

      ```
      git add -- UtilitiesCS.Test/UtilitiesCS.Test.csproj
      git diff --numstat --cached -- UtilitiesCS.Test/UtilitiesCS.Test.csproj
      ```

      `--numstat` prints added and deleted line counts as the first two tab-separated columns; `--stat`
      prints a graph rather than two readable integers. The `git add` span is what makes the `--cached`
      diff a reading of the current working state rather than of the index as it happened to stand, and it
      is also the companion a name-listing diff would need. When the command prints nothing, the file is
      identical to `HEAD` and both `PRE_INSERT` counts are `0`; record that as `PRE_INSERT ADDED=0
      DELETED=0` rather than as an empty field.

      Then insert the single `<Compile Include>` line, then take the AFTER readings:

      ```
      pwsh -NoProfile -Command '
      $p = "UtilitiesCS.Test/UtilitiesCS.Test.csproj"
      Write-Output ("NEW_COMPILE_ITEM=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "Bootstrap\AssemblyBindingFallbackEdgeCaseTests.cs").Count)
      Write-Output ("EXISTING_COMPILE_ITEM=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "Bootstrap\AssemblyBindingFallbackTests.cs").Count)
      Write-Output ("FSHARP_CORE_NETSTANDARD20_HINTPATH=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "packages\FSharp.Core.11.0.100\lib\netstandard2.0\FSharp.Core.dll").Count)
      Write-Output ("FSHARP_CORE_NETSTANDARD21_HINTPATH=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "packages\FSharp.Core.11.0.100\lib\netstandard2.1\FSharp.Core.dll").Count)
      '
      ```

      ```
      git add -- UtilitiesCS.Test/UtilitiesCS.Test.csproj
      git diff --numstat --cached -- UtilitiesCS.Test/UtilitiesCS.Test.csproj
      git diff --numstat origin/main...HEAD -- UtilitiesCS.Test/UtilitiesCS.Test.csproj
      ```

      Record the second command's output as `POST_INSERT`. The third command is recorded as an observation
      and is NOT gated: a merge-base diff compares two commits and is blind to a change that has not been
      committed, and nothing in this plan commits source before `[P6-T1]`, so it may legitimately print
      nothing here. It is retained because it is the only span that shows what this branch has already
      committed to that file. Append all four `Select-String` counts, the `PRE_INSERT` pair, the
      `POST_INSERT` pair and the merge-base output to
      `.../evidence/other/scope-and-determinism-checks.2026-09-13T18-22.md` under a
      `Revision R7 Project-File Registration:` heading.

      Acceptance: `NEW_COMPILE_ITEM=1`, `EXISTING_COMPILE_ITEM=1`,
      `FSHARP_CORE_NETSTANDARD20_HINTPATH=1`, `FSHARP_CORE_NETSTANDARD21_HINTPATH=0`; the `PRE_INSERT`
      deletion count is `0` and the `POST_INSERT` deletion count is `0`; and the `POST_INSERT` addition
      count is **exactly one greater** than the `PRE_INSERT` addition count. The delta form is what makes
      this gate independent of whether `[P2-T4]`'s line is already committed. A `POST_INSERT` deletion
      count above zero, or an addition delta other than one, means a line was rewritten or more than one
      line was added, which is outside this task's authority: restore the file with
      `git checkout -- UtilitiesCS.Test/UtilitiesCS.Test.csproj`, re-apply the single insertion and
      re-measure. `NEW_COMPILE_ITEM` reads 0 in the tree as it stands and
      `FSHARP_CORE_NETSTANDARD20_HINTPATH` reads 1 at line 598, both re-derived in this pass, which is what
      makes the first assertion discriminating and the third a real guard rather than a restatement.
- [x] [P4-T15] Record the write-set amendment, as `[P4-T12]`'s own remedy clause requires. Append to
      `.../evidence/baseline/write-set-decision.2026-09-13T18-22.md` a section headed
      `Revision R7 Write-Set Amendment:` recording: that
      `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs` is added as item 14 of
      `## Authorised Write Set`; that `UtilitiesCS.Test/UtilitiesCS.Test.csproj` now carries two
      `Compile Include` items added by this plan rather than one; that
      `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs` is NOT written by any Revision R7 task
      and its `[P2-T3]` standing guards are intact; and the reason for the split, which is that the
      existing file is 383 lines against a 500-line ceiling and ten further tests in its style would land
      it within a CSharpier reflow of that ceiling. Include the line
      `R7 WRITE-SET AMENDMENT: sibling test file added, existing test file untouched`.
      Acceptance: the heading exists in that artifact, all four recorded statements are present, and the
      artifact contains the line
      `R7 WRITE-SET AMENDMENT: sibling test file added, existing test file untouched` exactly once. That
      literal is absent from the tree until this task runs and is quoted here in prose for that reason.
- [x] [P4-T16] LOCK-ACQUIRE, rebuild the solution so the new test file is compiled, LOCK-RELEASE. Identical
      to `[P2-T10]`'s msbuild span in every respect except the console-log path, which is
      `TestResults/r7-build/r7-build-console.txt`. That path is under the git-ignored `TestResults/` scratch
      tree — the `[Tt]est[Rr]esult*/` pattern at `.gitignore` line 39 — deliberately, so this task adds no
      raw console dump to the feature folder's `evidence/` tree and neither `[P5-T11]` nor `[P5-T12]` gains
      a seventh log to project and remove.

      ```
      pwsh -NoProfile -Command '
      $d = "TestResults/r7-build"
      if (-not (Test-Path -LiteralPath $d)) { New-Item -ItemType Directory -Path $d -Force > $null }
      Write-Output ("R7_BUILD_LOG_DIR_PRESENT=" + (Test-Path -LiteralPath $d))
      '
      ```

      ```
      pwsh -NoProfile -Command '
      $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
      $msb = @(& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\amd64\MSBuild.exe")[0]
      & $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" *> "TestResults/r7-build/r7-build-console.txt"
      $LASTEXITCODE
      '
      ```

      ```
      pwsh -NoProfile -Command '
      $log = "TestResults/r7-build/r7-build-console.txt"
      Write-Output ("ZERO_ERRORS_LINES=" + @(Select-String -LiteralPath $log -Pattern "^\s+0 Error\(s\)$").Count)
      Write-Output ("SKIPPED_CORECOMPILE=" + @(Select-String -LiteralPath $log -Pattern "Skipping target .CoreCompile.").Count)
      Write-Output ("CONTROL_BUILD_OUTPUT=" + @(Select-String -LiteralPath $log -SimpleMatch -Pattern "UtilitiesCS.Test.dll").Count)
      '
      ```

      `CONTROL_BUILD_OUTPUT` is the positive control that the log is a real build log of the project this
      revision changes and that the search mechanism is live, so a zero `SKIPPED_CORECOMPILE` count is
      evidence rather than an artefact of an empty or unreadable file. Write
      `.../evidence/qa-gates/r7-build.2026-09-13T18-22.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`,
      `ExpectedExitCode: 0` and an `Output Summary:` carrying the four counts.
      Acceptance: `R7_BUILD_LOG_DIR_PRESENT=True`, `EXIT_CODE: 0`, `ZERO_ERRORS_LINES` greater than 0,
      `SKIPPED_CORECOMPILE=0`, and `CONTROL_BUILD_OUTPUT` greater than 0. A compile failure here is a defect
      in the new test file: fix it and re-run this task before proceeding to `[P4-T17]`.
- [x] [P4-T17] LOCK-ACQUIRE, re-take the ladder unit-test measurement now that the new tests exist,
      LOCK-RELEASE. This task supersedes `[P4-T2]` as the ladder suite's measurement of record; `## R7.6`
      records why `[P4-T2]` is not re-executed in place.

      First, remove every TRX already in the results directory and record the emptied count:

      ```
      pwsh -NoProfile -Command '
      $d = "TestResults/p4-ladder-r7"
      foreach ($f in @(Get-ChildItem -LiteralPath $d -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue)) { Remove-Item -LiteralPath $f.FullName -Force }
      Write-Output ("PRERUN_TRX_COUNT=" + @(Get-ChildItem -LiteralPath $d -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue).Count)
      '
      ```

      `TestResults/p4-ladder-r7` does not exist in the tree as it stands, so on a first execution this span
      enumerates nothing and emits `PRERUN_TRX_COUNT=0`; `-ErrorAction SilentlyContinue` on both
      enumerations is what makes an absent directory a zero count rather than an error. The span is
      required for the re-execution state, for the reason `[P4-T2]` records: `TRX_MATCH_COUNT=1` reads `1`
      whether this run wrote a fresh TRX or wrote none, so without the removal a run that failed to emit
      would leave the reader replaying an earlier run's outcomes.

      Then run the tests:

      ```
      pwsh -NoProfile -Command '
      $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
      $vstest = @(& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe")[0]
      & $vstest "UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~UtilitiesCS.Test.Bootstrap" "/Logger:trx;LogFileName=p4-ladder-r7.trx" /ResultsDirectory:TestResults/p4-ladder-r7
      $LASTEXITCODE
      '
      ```

      The `;` inside `"/Logger:trx;LogFileName=p4-ladder-r7.trx"` must stay inside the double quotes, for
      the reason `[P2-T11]` states: unquoted, PowerShell reads it as a statement separator. The filter is
      character-for-character `[P4-T2]`'s, which is why the new class must be in namespace
      `UtilitiesCS.Test.Bootstrap`.

      Read the TRX with the same pinned reader `[P4-T2]` uses, substituting `TestResults/p4-ladder-r7` and
      `p4-ladder-r7.trx` for its two literals, so the reader is
      `@(Get-ChildItem -LiteralPath "TestResults/p4-ladder-r7" -Filter "p4-ladder-r7.trx" -Recurse)` with
      the same `TRX_MATCH_COUNT=` emission before `[0]` is taken. A `-Filter "*.trx"` selection is
      prohibited here for the reason `[P2-T11]` records. Write
      `.../evidence/regression-testing/pass-after-ladder-r7.2026-09-13T18-22.md` with the four required
      fields plus the `PRERUN_TRX_COUNT=` line, the `TRX_MATCH_COUNT=` line and every `OUTCOME=` line. The
      literals `p4-ladder-r7.trx` and `TestResults/p4-ladder-r7` are quoted here in prose because both are
      absent from the tree until this task runs.
      Acceptance: `EXIT_CODE: 0`; the artifact records `PRERUN_TRX_COUNT=0`, taken before the run, which is
      what makes the `TRX_MATCH_COUNT=1` below a measurement of this run rather than of a residue; the
      artifact records `TRX_MATCH_COUNT=1`; the TRX `ResultSummary` `outcome` is `Completed`; the
      `Counters` `failed` value is `0`; the `passed` value is at least 21; and the artifact's `OUTCOME=`
      lines include one `Passed` line for each of the ten method names `[P4-T13]` pins. The floor of 21 is
      eleven existing methods in `AssemblyBindingFallbackTests` plus the ten `[P4-T13]` adds; it is stated
      as a floor rather than an equality so that a later task adding a further test does not make this
      condition unsatisfiable. It is discriminating against the pre-Revision-R7 assembly, which carries
      eleven.
- [x] [P4-T18] Re-take the determinism sweep and the file-size audit over the path lists that include the
      new test file. Two commands, run in this order:

      ```
      pwsh -NoProfile -Command '
      $paths = @("UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs","UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs","TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs","TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs","TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs")
      $banned = @("Thread.Sleep","Task.Delay","File.WriteAllText","File.WriteAllLines","File.WriteAllBytes","File.AppendAllText","File.Create","File.Delete","File.Move","File.Copy","Directory.CreateDirectory","Directory.Delete","Path.GetTempFileName","Path.GetTempPath","StreamWriter","DateTime.Now","DateTime.UtcNow")
      foreach ($p in $paths) { foreach ($b in $banned) { Write-Output ($p + " " + $b + " HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern $b).Count) } }
      foreach ($p in $paths) { Write-Output ($p + " CONTROL_AppDomain HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "AppDomain").Count) }
      '
      ```

      ```
      pwsh -NoProfile -Command '
      $paths = @("UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs","UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs","UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs","TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs","TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs","TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs","TaskMaster/ThisAddIn.cs")
      foreach ($p in $paths) { Write-Output ($p + " LINES=" + @(Get-Content -LiteralPath $p).Count) }
      '
      ```

      The first is `[P4-T9]`'s command with the new file added, giving five paths and 85 banned-token
      lines; the second is `[P4-T12]`'s command with the new file added, giving seven paths. The trailing
      `CONTROL_AppDomain` lines remain the positive control that each path is live and the search mechanism
      works, so a zero banned-token count is evidence rather than an artefact. In the new file the
      `AppDomain` token is carried by the class-level XML documentation comment, which `[P4-T13]` requires
      to name `AppDomain.AssemblyResolve` and whose presence `[P4-T13]`'s own acceptance gates at a count of
      at least 1. That ordering is deliberate: the control is guaranteed by the task that writes the file
      rather than discovered to be missing by this one, which would make this sweep unsatisfiable through no
      fault of its own.

      Append the first output to `.../evidence/other/scope-and-determinism-checks.2026-09-13T18-22.md`
      under a `Revision R7 Five-Path Sweep:` heading, and the second to
      `.../evidence/other/file-size-audit.2026-09-13T18-22.md` under a
      `Revision R7 Seven-Path Pre-Format Line Counts:` heading.
      Acceptance: both headings exist; under the first, every banned-token line ends with `HITS=0` and
      every `CONTROL_AppDomain` line ends with a count greater than 0, across all five paths; under the
      second, every `LINES=` value is at most 500, across all seven paths. If any `LINES=` value exceeds
      500 the offending type is split into a further file, the split is recorded in
      `.../evidence/baseline/write-set-decision.2026-09-13T18-22.md`, and the new file is registered in
      the owning `csproj`.

### Phase 5 — Full Four-Step C# Toolchain Loop

Run steps 1 to 4 in this exact order. If any step fails, or changes any file, fix and restart from step 1.
Each attempt overwrites its own artifact; the committed artifact is the final, clean pass.

**Revision R7 restart, stated so it is not left to be inferred.** This phase completed once, through
`[P5-T8]`, against a tree that no longer exists: `[P4-T13]` and `[P4-T14]` change compiled source in
`UtilitiesCS.Test` after that loop ran. A source change invalidates a completed loop, so **`[P5-T1]`
through `[P5-T8]` are unchecked and the loop restarts from step 1** — Outlook gate, format, format
verification, config side-effect check, analyzers, nullable, tests with coverage, post-format audits — and
`[P5-T9]` and `[P5-T10]` then re-run. Each re-executed task overwrites its own artifact; no artifact path
changes and no task is renumbered.

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
      Acceptance: `EXIT_CODE: 0` and every path the `Tree Observation:` field lists is one of the eleven
      repository-relative paths in items 1 to 10 and item 14 of `## Authorised Write Set`. Item 14 is the
      Revision R7 sibling test file `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs`,
      which `[P4-T13]` creates before this phase re-runs and which CSharpier therefore formats in this
      pass. A formatter rewrite of
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
      `line-rate`, `lines-valid` and `lines-covered` read from `coverage/coverage.cobertura.xml`, the
      total, failed and passed test counts read from the TRX under `coverage/test-results`, and a
      `Failing Test Names:` field listing every failed test by name, recorded as the literal `NONE` when
      the failed count is zero. The literal `Failing Test Names:` is quoted here in prose because it is
      absent from the tree until this task runs.
      Acceptance: the artifact exists; all six numeric values are present as numbers; the
      `Output Summary:` carries a `Failing Test Names:` field listing every failed test by name; and that
      list is a subset of the two failures recorded at `[P0-T8]`, namely
      `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` and
      `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic`. Any third name, or any name
      outside those two, blocks and restarts the loop from step 1. Those two were failing on this tree
      before any change from this plan was applied, as `[P0-T8]`'s artifact records under its
      `## Baseline Test Failures (recorded, not repaired)` heading. They are declared at
      `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` lines 204 and 237, a file no
      task in this plan writes to and which `[P6-T1]`'s pathspec does not name, so they survive this work
      unrepaired and requiring zero failures here would make this task unsatisfiable rather than
      strict. AC18's phrase
      "no failures" is therefore read against the baseline-failure set recorded at `[P0-T8]`, and
      `[P6-T23]` checks AC18 off on that reading. `spec.md` is not amended for this: amending AC18 in
      place risks changing the line count and every `spec.md` line number this plan cites.
      The subset formulation rather than an equality on the count is deliberate: if the intermittent
      pair happens to pass, the task must not block for that reason either.
      The runner's repository-wide threshold assertion runs after the Cobertura
      document has been written, so a non-zero exit caused by that assertion is recorded against a
      matching `ExpectedExitCode:` and does not block.
- [ ] [P5-T8] Post-format file-size audit. Repeat the SEVEN-path file-size command stated at `[P4-T18]`,
      not the six-path one at `[P4-T12]`, after the final format pass, and append the result to
      `.../evidence/other/file-size-audit.2026-09-13T18-22.md` under a `Post-Format Line Counts:` heading.
      Revision R7 repoints this task from `[P4-T12]` to `[P4-T18]` because the seven-path list is the one
      that includes `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs`, and a post-format
      ceiling audit that omits the file this revision adds would gate nothing about it.
      Acceptance: the heading exists and every `LINES=` value under it is at most 500, across all seven
      paths.

      In the same task, re-run the three pre-format sweeps whose acceptance conditions describe the
      terminal state rather than the Phase 4 state, because `[P5-T2]` rewrites tracked source across
      the whole tree and CSharpier 1.2.6 processes `packages.config` as well as `*.cs` and `*.xml`:
      re-run the `[P4-T8]` command and append its seven `NAME=count` lines to
      `.../evidence/other/static-shape-checks.2026-09-13T18-22.md` under a `Post-Format Shape Checks:`
      heading; re-run the FIVE-path determinism command stated at `[P4-T18]`, not the four-path one at
      `[P4-T9]`, and the `[P4-T11]` command, and append both outputs to
      `.../evidence/other/scope-and-determinism-checks.2026-09-13T18-22.md` under a
      `Post-Format Sweep:` heading.
      Acceptance: the `Post-Format Line Counts:` heading exists and every `LINES=` value under it is at
      most 500, across all seven paths; the `Post-Format Shape Checks:` heading records the same seven
      values `[P4-T8]` requires; and the `Post-Format Sweep:` heading records `PACKAGES_CONFIG_CHANGED=0`,
      `PORCELAIN_PACKAGES_CONFIG=0`, `NETSTANDARD_DLL_IN_PROJECTS=0`, a `FSHARP_REDIRECT_LINES` value
      equal to the `BASELINE_FSHARP_REDIRECT_LINES` integer recorded at `[P0-T15]`, every banned-token
      line ending `HITS=0` across all five paths, and every `CONTROL_AppDomain` line ending with a count
      greater than 0 across all five paths.
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

      **Revision R7 note — the gate is unchanged; the code that must clear it is not.** The first execution
      of this task recorded `ASSEMBLYBINDINGFALLBACK_CLASS_ROWS=1`,
      `ASSEMBLYBINDINGFALLBACK_LINES_VALID=201`, `ASSEMBLYBINDINGFALLBACK_LINES_COVERED=166` and a computed
      82.59 percent, and blocked correctly. The floor is AC17's own and is NOT lowered, the aggregation is
      NOT re-scoped, and no more favourable denominator is substituted. Revision R7 closes the shortfall by
      adding tests: `[P4-T13]` covers 23 of the 35 uncovered lines, which projects 189 of 201 covered, or
      94.03 percent. `## R7.1` enumerates every uncovered line and states which of them the new tests reach;
      `## R7.2` states the arithmetic. `ASSEMBLYBINDINGFALLBACK_LINES_VALID` is expected to remain 201,
      because Revision R7 adds no production line to
      `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` and deletes none. A materially different
      `LINES_VALID` on this re-run means the production file was edited by something outside this
      revision's authority: stop and report rather than adapting the figure.
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
      The artifact additionally records `BASELINE MEASURED ON A DIFFERENT BASE` with the baseline
      artifact's `Timestamp:` value, which is `2026-09-13T23-15`, and the merge commit `f02cee3fe`,
      stating that `[P0-T8]` was captured before `origin/main` at `a49c9729e` was merged into this branch
      and that the two document-level rates therefore measure different code bases as well as different
      denominators. The baseline is not re-captured: it already records
      `Cobertura Document State: RAW-COLLECTOR-OUTPUT`, so the no-regression judgment already rests on
      `NEW_MODULE_LINE_PERCENT` and `[P5-T9]` rather than on the rate comparison. The literal
      `BASELINE MEASURED ON A DIFFERENT BASE` is quoted here in prose because it is absent from the tree
      until this task runs.

      **Revision R7 addition — the two rates also differ in DOCUMENT STATE, and the concrete pairs are
      named.** This is a recording change and not a gate change: no acceptance condition below is added,
      removed or relaxed by it. The artifact additionally records a heading
      `Document State Flip:` carrying, verbatim, these three statements and the two figure pairs that
      support them.

      First, the two runs took different exit paths through
      `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and therefore produced documents in different states.
      `[P0-T8]` ran with two failing tests, so the runner threw at line 262 on a non-zero collection exit
      code, BEFORE the post-processing at lines 383-384, and the document it left behind is
      `RAW-COLLECTOR-OUTPUT`: all modules including third-party, with `line-rate = 0.7125753506415995`,
      `lines-valid = 83775` and `lines-covered = 59696`. `[P5-T7]` ran with zero failures, so the throw did
      not occur, post-processing ran, and the document is `POSTPROCESSED`: first-party only, with
      `line-rate = 0.858327`, `lines-valid = 65616` and `lines-covered = 56320`, and branches
      `13613 / 17022 = 79.97` percent. The two documents therefore differ in DENOMINATOR, in DOCUMENT
      STATE and in BASE COMMIT, three independent ways, and a direct comparison of their document-level
      rates measures none of the three cleanly.

      Second, issue #891 did not fire in the `[P5-T7]` run, and the reason is recorded rather than left
      implicit: `Assert-CoberturaLineCoverageThreshold` at line 386 asserts against the document-level
      rate of the POST-PROCESSED document, which is the first-party `0.858327`, and that clears its
      hard-coded 80 percent threshold. Issue #891 remains unfixed by this plan; this run simply did not
      meet its failing condition.

      Third, the no-regression judgment is unaffected by all of the above, because it already rests on
      `NEW_MODULE_LINE_PERCENT` and on `[P5-T9]` rather than on the document-level comparison.

      Acceptance addition: the `Document State Flip:` heading exists, names the baseline document state as
      `RAW-COLLECTOR-OUTPUT` and the post-change one as `POSTPROCESSED`, and carries both figure pairs —
      `0.7125753506415995` over `83775`, and `0.858327` over `65616` — as numbers rather than as
      descriptions. The literal `Document State Flip:` is quoted here in prose because it is absent from
      the tree until this task runs.
- [ ] [P5-T11] Project every raw build console log this plan produces under the feature folder's
      `evidence/` tree. **This task and `[P5-T12]` run after the four-step loop has completed cleanly and
      are not part of it**; they are placed here rather than in Phase 6 so they precede `[P6-T1]`, which is
      the first task that commits. Every gate that reads a raw console log — `[P0-T6]`, `[P0-T7]`,
      `[P2-T10]`, `[P4-T1]`, `[P5-T5]` and `[P5-T6]` — has already run and has already recorded its figure
      in its own `.md` artifact, so projecting and then removing the raw logs invalidates no acceptance
      condition in this plan.

      The six raw logs are named here rather than discovered, so the executor selects nothing:
      `.../evidence/baseline/analyzer-baseline-console.2026-09-13T18-22.txt`,
      `.../evidence/baseline/nullable-baseline-console.2026-09-13T18-22.txt`,
      `.../evidence/regression-testing/expect-fail-build-console.2026-09-13T18-22.txt`,
      `.../evidence/regression-testing/pass-after-build-console.2026-09-13T18-22.txt`,
      `.../evidence/qa-gates/analyzer-final-console.2026-09-13T18-22.txt` and
      `.../evidence/qa-gates/nullable-final-console.2026-09-13T18-22.txt`.

      Command, run once. The repository root is derived at run time with `(Resolve-Path .).Path` rather
      than written as a literal, because this plan contains no absolute host path other than the two
      build-lock paths:

      ```
      pwsh -NoProfile -Command '
      $root = (Resolve-Path .).Path
      $base = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence"
      $logs = @("$base/baseline/analyzer-baseline-console.2026-09-13T18-22.txt","$base/baseline/nullable-baseline-console.2026-09-13T18-22.txt","$base/regression-testing/expect-fail-build-console.2026-09-13T18-22.txt","$base/regression-testing/pass-after-build-console.2026-09-13T18-22.txt","$base/qa-gates/analyzer-final-console.2026-09-13T18-22.txt","$base/qa-gates/nullable-final-console.2026-09-13T18-22.txt")
      foreach ($log in $logs) {
      $out = [System.IO.Path]::ChangeExtension($log, ".projection.md")
      $lines = @(Get-Content -LiteralPath $log)
      $body = New-Object System.Collections.Generic.List[string]
      $body.Add("# Console log projection")
      $body.Add("")
      $body.Add("Timestamp: 2026-09-13T18-22")
      $body.Add("Command: (projection of the raw console log named below)")
      $body.Add("EXIT_CODE: 0")
      $body.Add("SOURCE_LOG=" + $log)
      $body.Add("SOURCE_LINES=" + $lines.Count)
      $body.Add("SKIPPING_CORECOMPILE_COUNT=" + @($lines.Where({ $_.Contains("Skipping target ""CoreCompile""") })).Count)
      $body.Add("DIAGNOSTIC_LINE_COUNT=" + @($lines.Where({ $_.Contains(" error ") -or $_.Contains(" warning ") })).Count)
      $body.Add("HOST_PATH_LINE_COUNT=" + @($lines.Where({ $_.Contains($root) })).Count)
      $kept = @($lines.Where({ ($_ -match "^MSBuild version") -or ($_ -match "^\s+\d+ Warning\(s\)$") -or ($_ -match "^\s+\d+ Error\(s\)$") -or ($_ -match "^(Build succeeded|Build FAILED)") }))
      $body.Add("SUMMARY_LINES_KEPT=" + $kept.Count)
      $body.Add("")
      $body.Add("Output Summary:")
      $body.Add("")
      $body.Add("``````")
      foreach ($l in $kept) { $body.Add($l.Replace($root, "<repo-root>")) }
      $body.Add("``````")
      Set-Content -LiteralPath $out -Value $body -Encoding UTF8
      Write-Output ("PROJECTION_WRITTEN=" + $out)
      Write-Output ("PROJECTION_ABSOLUTE_PATH_HITS=" + @(@(Get-Content -LiteralPath $out).Where({ $_.Contains("C:\") })).Count)
      Write-Output ("PROJECTION_SOURCE_LINES=" + $lines.Count)
      Write-Output ("PROJECTION_SUMMARY_LINES_KEPT=" + $kept.Count)
      Write-Output ("PROJECTION_TOTAL_LINES=" + @(Get-Content -LiteralPath $out).Count) }
      '
      ```

      Every retained line is passed through `.Replace($root, "<repo-root>")` before it is written, and the
      only other values written are counts and the repository-relative log path, so no projection can
      carry the absolute worktree path. The literals `PROJECTION_WRITTEN=`, `PROJECTION_SOURCE_LINES=`,
      `PROJECTION_SUMMARY_LINES_KEPT=`, `PROJECTION_TOTAL_LINES=`, `SOURCE_LOG=`, `SOURCE_LINES=`,
      `SUMMARY_LINES_KEPT=`, `SKIPPING_CORECOMPILE_COUNT=`, `DIAGNOSTIC_LINE_COUNT=` and
      `HOST_PATH_LINE_COUNT=` are quoted here in prose because they are absent from the tree until this
      task runs, as is `PROJECTION_ABSOLUTE_PATH_HITS=`. Write
      `.../evidence/other/console-log-projections.2026-09-13T18-22.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:` and an `Output Summary:` reproducing every `PROJECTION_WRITTEN=`,
      `PROJECTION_ABSOLUTE_PATH_HITS=`, `PROJECTION_SOURCE_LINES=`, `PROJECTION_SUMMARY_LINES_KEPT=` and
      `PROJECTION_TOTAL_LINES=` line verbatim.
      Acceptance: the artifact records exactly six `PROJECTION_WRITTEN=` lines; every
      `PROJECTION_ABSOLUTE_PATH_HITS=` value is `0`, which is the no-absolute-host-path invariant measured
      on the written projection rather than assumed from the substitution; every
      `PROJECTION_TOTAL_LINES=` value is at most 500; every `PROJECTION_SOURCE_LINES=` value is greater
      than 0; and every `PROJECTION_SUMMARY_LINES_KEPT=` value is greater than 0. The last of these is the
      positive control on the line-matching mechanism: it proves the four retained-line patterns matched
      real content, so a zero `SKIPPING_CORECOMPILE_COUNT` in the same projection is an observation rather
      than an artefact of a projection that matched nothing. A `PROJECTION_SOURCE_LINES=0` for any log
      blocks: it would mean the projection read an empty or absent file. Additionally, the two `qa-gates`
      projections must each record `SKIPPING_CORECOMPILE_COUNT=0`, which is the figure AC18 requires and
      the figure `[P5-T5]` and `[P5-T6]` already gated on the raw logs; any other value blocks, because it
      would mean the analyzer or nullable gate was vacuous.
- [ ] [P5-T12] Remove the six raw build console logs from the feature folder's `evidence/` tree, now that
      `[P5-T11]` has projected each of them and every gate that reads one has run. The reason is the
      standing directive that evidence artifacts carry projections rather than raw dumps and carry no
      absolute host path:
      `.../evidence/regression-testing/expect-fail-build-console.2026-09-13T18-22.txt` is 11,961 lines of
      which 7,747 carry the absolute worktree path including the host user name, and the two Phase 0 logs
      are 5,030 and 11,842 lines each retaining about 140 lines of absolute toolchain paths beginning
      `C:\Program Files`. Command:

      ```
      pwsh -NoProfile -Command '
      $base = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence"
      $logs = @("$base/baseline/analyzer-baseline-console.2026-09-13T18-22.txt","$base/baseline/nullable-baseline-console.2026-09-13T18-22.txt","$base/regression-testing/expect-fail-build-console.2026-09-13T18-22.txt","$base/regression-testing/pass-after-build-console.2026-09-13T18-22.txt","$base/qa-gates/analyzer-final-console.2026-09-13T18-22.txt","$base/qa-gates/nullable-final-console.2026-09-13T18-22.txt")
      foreach ($log in $logs) { if (Test-Path -LiteralPath $log) { Remove-Item -LiteralPath $log -Force } }
      Write-Output ("RESIDUAL_TXT_COUNT=" + @(Get-ChildItem -LiteralPath $base -Filter "*.txt" -Recurse).Count)
      Write-Output ("PROJECTION_COUNT=" + @(@(Get-ChildItem -LiteralPath $base -Filter "*.md" -Recurse).Where({ $_.Name.EndsWith(".projection.md") })).Count)
      '
      ```

      Append the output to `.../evidence/other/console-log-projections.2026-09-13T18-22.md` under a
      `Raw Console Log Removal:` heading. The deletions are staged by `[P6-T1]`, whose `git add` pathspec
      already covers the whole feature folder, so no `git rm` is needed and none is written here.
      Acceptance: the heading exists, the appended output records `RESIDUAL_TXT_COUNT=0` and
      `PROJECTION_COUNT=6`. No task in this plan adds a raw `.trx` or a raw `.cobertura.xml` to git, and
      this task adds none: `TestResults/` is git-ignored by the `[Tt]est[Rr]esult*/` pattern at
      `.gitignore` line 39 and `coverage/` by `coverage/*` at line 144, and no task copies a file out of
      either directory into the feature folder.

### Phase 6 — Open Risk, Manual Gates, Commit, and Acceptance Check-Off

- [ ] [P6-T1] Commit the source and test changes together with the evidence produced so far. Commands:

      ```
      git add -- UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs UtilitiesCS/UtilitiesCS.csproj TaskMaster/ThisAddIn.cs TaskMaster/app.config UtilitiesCS.Test/Bootstrap UtilitiesCS.Test/UtilitiesCS.Test.csproj TaskMaster.Test/Bootstrap TaskMaster.Test/TaskMaster.Test.csproj docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879
      git commit -m "fix(879): install an eager self-sufficient assembly-binding fallback in production" -- UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs UtilitiesCS/UtilitiesCS.csproj TaskMaster/ThisAddIn.cs TaskMaster/app.config UtilitiesCS.Test/Bootstrap UtilitiesCS.Test/UtilitiesCS.Test.csproj TaskMaster.Test/Bootstrap TaskMaster.Test/TaskMaster.Test.csproj docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879
      ```

      Revision R7 widened one pathspec from the single file
      `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs` to the directory
      `UtilitiesCS.Test/Bootstrap`, mirroring the `TaskMaster.Test/Bootstrap` operand already present, so
      that the sibling file `[P4-T13]` creates is committed. A file-level pathspec would have left the new
      file untracked and the acceptance condition below would have failed on it. That directory holds only
      the two files this plan authors.

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
      invisible to it until it is committed, and `[P6-T1]` has committed it. AC14 at `spec.md` line 536
      names "the merge base with `main`"; this task anchors on `origin/main` instead, and the artifact
      records that substitution, because in a worktree-per-item run the local `main` ref is stale — it
      stands at `03d2ece20` while `origin/main` stands at `a49c9729e`, and `a49c9729e` is already merged
      into this branch at `f02cee3fe`, so `origin/main...HEAD` is the merge-base diff AC14 describes,
      whereas anchoring on the stale local ref would additionally list every change `origin/main` gained
      since `03d2ece20`. The AC
      text is not amended: `spec.md` line 536 is the first line of a seven-line criterion spanning lines
      536-542, every `[P6-T#]` check-off addresses `spec.md` by line number, and a re-wrap at line 536
      would move the five criteria that follow it — `spec.md` lines 543, 545, 547, 550 and 554, cited by
      `[P6-T20]`, `[P6-T21]`, `[P6-T22]`, `[P6-T23]` and `[P6-T24]` — to correct one word.
      Write
      `.../evidence/other/scope-boundary-diff.2026-09-13T18-22.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:` and an `Output Summary:` reproducing the full diff list and the full porcelain
      output verbatim, empty output recorded as the literal `NONE`, and the sentence
      `ANCHOR: origin/main, substituted for the stale local main ref`, which is quoted here in prose
      because it is absent from the tree until this task runs.
      Acceptance: `git rev-parse --verify origin/main` exits 0; the artifact records the sentence
      `ANCHOR: origin/main, substituted for the stale local main ref`; the porcelain span returns no
      output; and
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
- [ ] [P6-T9] Mark the acceptance criterion at `spec.md` line 484 as complete, as in `[P6-T6]`. AC4's
      closing sentence at `spec.md` lines 489-490 — that all rungs are exercised through injected
      delegates, touching neither the GAC nor the filesystem — is read as a property of the ten
      delegate-driven tests that exercise the ladder rungs, and not of the eleventh test `[P2-T3]` adds,
      which drives the subscribed handler through the real CLR binder to cover
      `AssemblyBindingFallback.OnAssemblyResolve` and `AssemblyBindingFallback.CreateProductionLadder`.
      Under that reading all four rungs retain injected-delegate coverage and AC4 is satisfied.
      `spec.md` lines 447-448 note that only pure ladder logic belongs in this assembly because it is
      masked by the PR #880 handler; that ground does not reach the eleventh test, whose asserted
      outcome is that a display name matching nothing fails to load, which no masking handler can
      change. `spec.md` is not amended: AC4 spans lines 484-490 and a re-wrap would move the fifteen
      criteria that follow it, every one of which this plan cites by line number. Append this reading to
      `.../evidence/other/ac-inventory.2026-09-13T18-22.md` under an `AC4 Reading:` heading, together
      with the line `AC4 READING: ten delegate-driven tests, eleventh test excluded`, which is quoted
      here in prose because it is absent from the tree until this task runs.
      **Revision R7 note — the reading and its pinned literal are unchanged.** AC4 names the class
      `UtilitiesCS.Test.Bootstrap.AssemblyBindingFallbackTests` specifically. The ten tests `[P4-T13]` adds
      live in a different class, `AssemblyBindingFallbackEdgeCaseTests`, so they are outside AC4's subject
      and neither extend nor contradict its enumerated scenario list. The reading above therefore still
      describes exactly eleven tests in exactly the class AC4 names, and the pinned literal below is
      unchanged. For completeness the artifact also records that the ten sibling tests are themselves
      delegate-driven or guard-clause tests and that none of them drives a real bind, so the eleventh test
      remains the single exception this reading carves out across the whole work.
      Acceptance: `spec.md` line 484 begins with the six characters `- [x] `, the `AC4 Reading:` heading
      exists in that artifact, and that artifact contains the line
      `AC4 READING: ten delegate-driven tests, eleventh test excluded` exactly once.
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
- [ ] [P6-T24] Conditionally mark the acceptance criterion at `spec.md` line 554. Read the `RESULT:`
      field of `.../evidence/other/manual-live-outlook-gate.2026-09-13T18-22.md`. When that value is
      `DEEDLE LOADED AND DATA MODEL POPULATED`, change line 554's leading `- [ ] ` to `- [x] `, changing
      no other character. When it is `FAILED` or `PENDING-MAINTAINER`, leave line 554 as `- [ ] ` and
      append to that artifact a `Check-Off Withheld:` field carrying the observed `RESULT:` value and the
      sentence `AC19 is not checked off because the human gate is not discharged.`
      Acceptance: exactly one of the two branches is taken and is evidenced. Either `spec.md` line 554
      begins with the six characters `- [x] ` and the artifact's `RESULT:` is
      `DEEDLE LOADED AND DATA MODEL POPULATED`; or `spec.md` line 554 begins with `- [ ] ` and the
      artifact carries a `Check-Off Withheld:` field. Any other combination blocks. The literal
      `Check-Off Withheld:` is quoted here in prose because it is absent from the tree until this task
      runs. `spec.md` lines 554-556 state that this is a human gate and that an unrecorded result does
      not discharge it, so an unconditional check-off would make the plan's own artifact assert
      something false about a gate no executor can perform.
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

Counted mechanically over lines matching either task prefix pattern, `- [ ] [P#-T#]` or `- [x] [P#-T#]`,
and re-derived after the Revision R7 delta rather than carried forward:

- Phase 0: 16 tasks, all 16 complete
- Phase 1: 7 tasks, all 7 complete
- Phase 2: 12 tasks, all 12 complete
- Phase 3: 5 tasks, all 5 complete
- Phase 4: 18 tasks, 12 complete — the twelve complete are `[P4-T1]` through `[P4-T12]`. `[P4-T13]`
  through `[P4-T18]` were APPENDED and left unchecked by Revision R7
- Phase 5: 12 tasks, 0 complete — `[P5-T1]` through `[P5-T8]` were unchecked by Revision R7 because a
  source change invalidates a completed toolchain loop; `[P5-T9]` through `[P5-T12]` were already
  unchecked, `[P5-T9]` having blocked on the coverage floor
- Phase 6: 27 tasks, 0 complete
- Total: 97 tasks, 52 complete

Revision history of these figures. Revision R2 added three tasks and removed none, so the total moved from
86 to 89. Revision R3 added no task and removed none and unchecked exactly one, `[P2-T3]`. Revision R4
added no task, removed none, and checked or unchecked none.

Revision R5 adds two tasks, `[P5-T11]` and `[P5-T12]`, and removes none, so the total moves from 89 to 91.
Both were APPENDED to the end of Phase 5 rather than inserted, so no task is renumbered and every
`[P#-T#]` identifier this plan and its evidence artifacts already cite is unchanged. Revision R5 unchecks
six tasks — `[P1-T6]`, `[P1-T7]`, `[P2-T5]`, `[P2-T6]`, `[P2-T10]` and `[P2-T12]` — and checks none, so
the complete count moves from 34, which was the checklist state on disk before this revision, to 28. The
figure 25 recorded for the previous revision described the checklist as Revision R4 left it; the executor
subsequently completed `[P1-T6]`, `[P1-T7]`, `[P2-T1]`, `[P2-T3]`, `[P2-T5]`, `[P2-T6]`, `[P2-T10]` and
`[P2-T12]`, which is how 25 became 34, and that state was re-counted mechanically in this pass rather than
carried forward. The per-phase figures above were likewise re-counted mechanically after the Revision R5
edits: Phase 0 16, Phase 1 7, Phase 2 12, Phase 3 5, Phase 4 12, Phase 5 12, Phase 6 27, which sum to 91.

Revision R6 adds no task, removes none, and checks or unchecks none. The per-phase figures above were
re-counted mechanically after the Revision R6 edits and were: Phase 0 16, Phase 1 7, Phase 2 12,
Phase 3 5, Phase 4 12, Phase 5 12, Phase 6 27, summing to 91, of which 28 were complete at that time.

Revision R7 adds six tasks, `[P4-T13]` through `[P4-T18]`, and removes none, so the total moves from 91 to
97. All six were APPENDED to the end of Phase 4 rather than inserted, so no task is renumbered and every
`[P#-T#]` identifier this plan and its evidence artifacts already cite is unchanged. Revision R7 unchecks
eight tasks — `[P5-T1]` through `[P5-T8]` — and checks none.

The complete count was re-derived mechanically from the checkboxes on disk in this pass rather than
carried forward from the figure 28, which described the checklist as Revision R6 left it. The executor
subsequently completed all of Phases 1 through 4 and the first eight tasks of Phase 5, so the on-disk state
before this revision was 60 complete of 91: Phase 0 16, Phase 1 7, Phase 2 12, Phase 3 5, Phase 4 12,
Phase 5 8, Phase 6 0. Unchecking the eight Phase 5 tasks moves that to 52, and appending six unchecked
tasks leaves 52 complete of 97. The per-phase totals above were likewise re-counted after the Revision R7
edits: 16 + 7 + 12 + 5 + 18 + 12 + 27 = 97, of which 16 + 7 + 12 + 5 + 12 + 0 + 0 = 52 are complete.

## Acceptance-Criteria Traceability

| AC | spec.md line | Implementation task | Test or check task | Evidence task |
|---|---|---|---|---|
| AC1 | 473 | P2-T1, P2-T2 | P2-T1, P2-T2 | P6-T6 |
| AC2 | 477 | P3-T3 | P4-T7 | P4-T7 |
| AC3 | 481 | P3-T3 | P4-T8 | P4-T8 |
| AC4 | 484 | P2-T3, P3-T1, P3-T2 | P4-T2, P4-T17 | P4-T2, P4-T17, P6-T9 |
| AC5 | 491 | P2-T5, P2-T6 | P4-T3 | P4-T3 |
| AC6 | 495 | P2-T6 | P2-T12, P4-T3 | P2-T12 |
| AC7 | 499 | P2-T5, P2-T6 | P2-T12, P4-T3 | P2-T12 |
| AC8 | 504 | P2-T5, P2-T6 | P2-T12, P4-T3 | P2-T12 |
| AC9 | 509 | P3-T1 | P4-T5 | P4-T3 |
| AC10 | 515 | P2-T5, P2-T6, P3-T1 | P2-T11, P4-T6 | P1-T6, P1-T7, P2-T11, P4-T3, P4-T6 |
| AC11 | 521 | P2-T6, P2-T7 | P2-T12, P4-T4 | P2-T12 |
| AC12 | 528 | P3-T4 | P4-T7, P4-T8 | P4-T8 |
| AC13 | 534 | P3-T5 | P3-T5 | P3-T5 |
| AC14 | 536 | P1-T4 | P6-T5 | P6-T5 |
| AC15 | 543 | P3-T4 | P4-T11 | P4-T11 |
| AC16 | 545 | P2-T3, P2-T5, P2-T6, P2-T8, P4-T13 | P4-T9, P4-T18 | P4-T9, P4-T18, P5-T8 |
| AC17 | 547 | P3-T1, P4-T13, P4-T14 | P5-T9 | P4-T17, P5-T9, P5-T10 |
| AC18 | 550 | P5-T2 to P5-T7 | P5-T5, P5-T6, P5-T7 | P5-T5, P5-T6, P5-T7, P5-T11 |
| AC19 | 554 | P6-T4 | P6-T4 | P6-T4 |

AC19 is the only conditionally discharged criterion in this table. `[P6-T24]` checks it off only when
`.../evidence/other/manual-live-outlook-gate.2026-09-13T18-22.md` records
`RESULT: DEEDLE LOADED AND DATA MODEL POPULATED`. When that artifact records `FAILED` or
`PENDING-MAINTAINER`, `[P6-T24]` takes its withholding branch and the acceptance-criteria status summary
reported at plan completion must list AC19 as REMAINING, naming the observed `RESULT:` value as the
reason. Reporting AC19 as delivered on the strength of a `PENDING-MAINTAINER` result would assert
something false about a gate no executor can perform.

AC18 is discharged on the reading `[P5-T7]` states: "no failures" is read against the two baseline
failures recorded at `[P0-T8]`, which are outside this plan's authorised write set. `[P6-T23]` checks
AC18 off on that reading, and the status summary reports it as delivered with that qualification named.

AC18 carries a second stated reading, introduced by Revision R5. Its phrase "with console logs captured
under the feature folder's `evidence/qa-gates/` directory" is discharged by the projections `[P5-T11]`
writes, not by the raw console dumps `[P5-T12]` removes. The operative half of the criterion — that the
analyzer and nullable logs each show zero `Skipping target "CoreCompile"` occurrences — is carried
forward literally: `[P5-T5]` and `[P5-T6]` each gate that figure on the raw log while it exists, and each
`qa-gates` projection records it as `SKIPPING_CORECOMPILE_COUNT=0`, which `[P5-T11]` gates a second time.
AC18's text is NOT amended, for the line-number reason `[P6-T5]` records for AC14: AC18 spans `spec.md`
lines 550-553 and a re-wrap would move AC19 and every `[P6-T#]` citation that follows it. The status
summary reports AC18 as delivered with this qualification named alongside the `[P5-T7]` one.

AC4 is discharged on the reading `[P6-T9]` states: its closing sentence about injected delegates is read
as a property of the ten tests that exercise the ladder rungs, not of the eleventh test `[P2-T3]` adds to
cover the production entry point. The status summary reports AC4 as delivered with that qualification
named. Revision R7 does not extend that reading. AC4 names
`UtilitiesCS.Test.Bootstrap.AssemblyBindingFallbackTests` specifically, and the ten tests `[P4-T13]` adds
are in a different class, so they are outside the criterion's subject; they are nonetheless delegate-driven
or guard-clause tests and none of them drives a real bind, so the eleventh test remains the single stated
exception across the whole work.

AC17 is the criterion Revision R7 exists to satisfy. Its 90 percent floor was measured at 82.59 percent on
the first `[P5-T9]` run and the task blocked correctly. The floor is not lowered and the measurement is not
re-scoped: `[P4-T13]` and `[P4-T14]` add and register ten tests that cover 23 of the 35 uncovered lines,
projecting 189 of 201 covered or 94.03 percent, and `[P5-T9]` re-runs its unchanged command against the
unchanged 201-line denominator. `## R7.1` and `## R7.2` carry the line-by-line inventory and the
arithmetic.

## Planner Notes

- **Revision R7 recorded: AC17's coverage floor closed by new tests, no criterion text amended.** Revision
  R7 amends no acceptance-criterion text and writes no `spec.md` edit, so every `spec.md` line number this
  plan cites — 473, 477, 481, 484, 491, 495, 499, 504, 509, 515, 521, 528, 534, 536, 543, 545, 547, 550 and
  554 — is unchanged; the absence of a write to that file is the ground, and `spec.md` was re-read at lines
  470 through 554 in this pass to confirm the inventory is still nineteen and still at those line numbers
  rather than inferring it from the absence of a write. Revision R7 adds six tasks, `[P4-T13]` through
  `[P4-T18]`, all appended to the end of Phase 4; unchecks eight, `[P5-T1]` through `[P5-T8]`; adds one
  path to `## Authorised Write Set` as item 14 and renumbers the conditional entry from 14 to 15; and adds
  Revision R7 cross-reference notes to `[P2-T3]`, `[P4-T2]`, `[P4-T9]`, `[P4-T12]`, `[P5-T9]` and
  `[P6-T9]` without changing any of their checklist states or acceptance conditions.
- **Two inbound premises corrected, per the authentication rule.** First, the delegation brief stated that
  Revision R6 relabelled `[P2-T3]`'s acceptance as "a standing guard pinning exactly eleven `[TestMethod]`
  occurrences" and that "a twelfth test falsifies both" that guard and `[P4-T2]`'s floor. Re-derived from
  the task text in this pass, `[P2-T3]`'s condition reads "contains **at least** eleven occurrences of
  `[TestMethod]`" and `[P4-T2]`'s reads "the `passed` value is at least 11". Both are floors, not
  ceilings, and neither is falsified by an additional test. The `[P5-T9]` evidence artifact repeats the
  same misreading in its own prose. The correction does not change the option this revision takes: the
  binding constraint on amending
  `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs` is the 500-line ceiling against a measured
  383 lines, not either guard, and `## R7.3` records that. Second, the brief listed rung-4 lines 355-356
  among the cheapest lines to cover. They are not reachable: line 351 reads
  `Path.GetDirectoryName(typeof(AssemblyBindingFallback).Assembly.Location)`, which is not an injected
  delegate and is non-empty for a file-loaded assembly, so no test can drive the guard at line 354 to its
  true branch. Those two lines are excluded from the covered-line claim and named as unreachable in
  `## R7.1`. The brief's other cited lines were confirmed against the Cobertura document.
- **One deviation from the brief, with its reason.** The brief directed that `[P4-T2]` be re-run with a
  raised floor. `[P4-T2]` is instead left checked and superseded by a new task, `[P4-T17]`, carrying the
  same command shape with a floor of 21. The reason is an ordering defect the directed form would have
  created: `[P4-T2]` is at numeric position 2 of Phase 4 while the tasks that author the new tests are
  appended at positions 13 to 18, so an executor working the checklist in order would have re-run
  `[P4-T2]` against an assembly that does not yet contain the new tests, and a floor of 21 would have been
  unsatisfiable at the moment it ran. `[P4-T17]` achieves the brief's actual objective — a ladder-suite run
  that includes and passes the new tests, gated at a floor raised to match — in a position where the
  condition can be satisfied.
- **One `.csproj` edit is planned, and it is not the excluded one.** The brief's "Do not change" list ends
  "and no `.csproj` — the `FSharp.Core` HintPath skew is the root cause but is OUT OF SCOPE and tracked as
  issue #895." Revision R7 plans exactly one `.csproj` change, at `[P4-T14]`: a single
  `<Compile Include>` line in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` registering the new test file. It
  touches no `Reference`, no `HintPath` and no package version, and `[P4-T14]` gates that the `FSharp.Core`
  `HintPath` at line 598 still reads the `lib\netstandard2.0` value and that the `lib\netstandard2.1` value
  is absent, plus a `--numstat` deletion count of zero. The edit is unavoidable if a sibling file is used
  at all: that project uses explicit `Compile` items with no wildcard glob, re-derived in this pass, so an
  unregistered file silently does not build. It is also the remedy `[P4-T12]`'s own acceptance condition
  already prescribes for a file-size overflow. This is flagged rather than assumed: if the brief intended a
  blanket prohibition on every `.csproj` line, the only remaining route is to amend
  `AssemblyBindingFallbackTests.cs` in place and accept a post-format file of roughly 490 to 500 lines
  against a ceiling `[P5-T8]` audits after the whole toolchain loop has run.

- **Revision R6 recorded: TRX residue removal, and no criterion or task change.** Revision R6 amends no
  acceptance-criterion text, writes no `spec.md` edit, adds and removes no task, and changes no task's
  checklist state. It makes three changes. First, `[P2-T11]` gains a pre-run span that removes the `.trx`
  files in `TestResults/p2-expect-fail` and emits `PRERUN_TRX_COUNT=`, because that directory holds two
  superseded TRX files and the second of them carries the pinned name `p2-expect-fail.trx`, against which
  the `TRX_MATCH_COUNT=1` guard added in Revision R3 does not discriminate. Second, `[P4-T2]`, `[P4-T3]`
  and `[P4-T7]` gain the same span against their own results directories; those directories are absent
  from the tree today, so the span is a guard against the re-execution state rather than a correction of
  an observed residue. `[P4-T6]` needs no span of its own: it reads the TRX that `[P4-T3]` emptied and
  rewrote earlier in the same phase. Third, `[P2-T3]`'s acceptance conditions are relabelled as standing
  guards, matching the label `[P2-T5]` and `[P2-T6]` already carry, and the pre-execution figure quoted
  in that acceptance is replaced by a post-execution reading: the file now carries eleven `[TestMethod]`
  occurrences and zero hits for `reaches the GAC`. `[P2-T3]` is checked and does not re-run, so no gate
  changes; the body's pre-execution figure of ten is retained as the record of why the condition was
  authored.
- **Revision R2 spec amendment recorded; its AC10 text is SUPERSEDED by Revision R5.** The planner
  rewrote acceptance criterion AC10 at `spec.md` lines 515-520 in place, in exactly six lines, so every
  acceptance-criterion line number this plan cites is unchanged. The text it replaced asserted that "a
  Deedle type initializes without `TypeInitializationException`", which the measured `[P2-T11]` run
  satisfied against a build carrying no fix. The Revision R2 replacement asserted that invoking
  `Deedle.Reflection.convertRecordSequence`, closed over a concrete record type and given a one-element
  `IEnumerable<T>`, raises no `netstandard` bind failure. That replacement was itself satisfied against a
  build carrying no fix, for the reason `## R6.1` measures, and it is no longer the text of AC10; the
  current text is the Revision R5 one described in the Revision R5 note above. This note is retained as
  the record of the intermediate state and must not be read as a description of the criterion as it now
  stands. The criterion is amended by the planner and verified read-only by `[P1-T7]`; no executor task
  edits acceptance-criterion text. No criterion was added or removed and the inventory remains nineteen.
  AC1 at `spec.md` lines 473-476 and the write-set entry at `spec.md`
  line 315 both name the internal `Resolve` seam without a return type, so Defect 2 required no spec
  amendment; that was checked rather than assumed.
- **Spec correction recorded.** One acceptance criterion directed the coverage artifact to
  `evidence/coverage/`, which is not a canonical evidence kind. The planner changed that single directory
  reference to `evidence/qa-gates/`, changed nothing else in the criterion's text, added no criterion and
  removed none. No other non-canonical evidence directory appears in `spec.md`.
- **Revision R3 amends no acceptance criterion and moves no `spec.md` line.** Revision R3 edits
  `plan.2026-09-13T18-22.md` only. `spec.md` is not written by this revision, so every acceptance-criterion
  line number the plan cites — 473, 477, 481, 484, 491, 495, 499, 504, 509, 515, 521, 528, 534, 536, 543,
  545, 547, 550 and 554 — is unchanged, and that was verified by re-reading `spec.md` lines 468-556 after
  the revision's edits rather than inferred from the absence of a write. Two candidate `spec.md`
  amendments were considered and both were declined: re-wording AC14 at line 536 from `main` to
  `origin/main`, declined in favour of recording the anchor substitution in `[P6-T5]`; and extending AC4's
  enumerated list at lines 484-490 to name the eleventh test, declined because an additional test method
  beyond the enumeration does not falsify AC4.
- **One inbound figure corrected.** The Revision R3 delta described AC4 at `spec.md` lines 484-490 as a
  six-line block. It is seven lines: 484 through 490 inclusive, with AC5 beginning at line 491. The
  constraint the delta drew from that figure — preserve the block's line count if it is amended — is
  unaffected, and the question is moot because AC4 is not amended.
- **Revision R4 amends no acceptance criterion and moves no `spec.md` line.** Revision R4 edits
  `plan.2026-09-13T18-22.md` only. `spec.md` is not written by this revision, so every
  acceptance-criterion line number the plan cites — 473, 477, 481, 484, 491, 495, 499, 504, 509, 515,
  521, 528, 534, 536, 543, 545, 547, 550 and 554 — is unchanged, and that was verified by re-reading
  `spec.md` lines 440-494 after the revision's edits rather than inferred from the absence of a write.
  Revision R4 closes one defect that survived two rounds: Revision R3 scoped the claim that the tests
  touch neither the GAC nor the filesystem to the ten delegate-driven tests in the plan's own copy of the
  sentence, but the same claim lives in two further places. `[P2-T3]` now rewrites the copy in the test
  file's class-level XML documentation comment at
  `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs` lines 11-15, and `[P6-T9]` records the
  reading for the copy in AC4's closing sentence at `spec.md` lines 489-490. The `spec.md` copy is not
  edited, for the line-number reason `[P6-T5]` records for AC14: AC4 spans lines 484-490 and a re-wrap
  would move the fifteen criteria that follow it.
- **Revision R5 amends acceptance criterion AC10 and two non-criterion `spec.md` regions, and moves no
  `spec.md` line.** AC10 at `spec.md` lines 515-520 is rewritten in place in exactly six lines, so AC11
  still begins at line 521 and every acceptance-criterion line number this plan cites — 473, 477, 481,
  484, 491, 495, 499, 504, 509, 515, 521, 528, 534, 536, 543, 545, 547, 550 and 554 — is unchanged. That
  was verified after the edit by re-listing every line in `spec.md` that begins with the six characters
  `- [ ] ` and comparing the resulting line numbers against the `[P1-T2]` inventory, rather than inferred
  from the shape of the replacement. The prior AC10 text asserted that invoking
  `Deedle.Reflection.convertRecordSequence` raises no `netstandard` bind failure, without naming the
  domain configuration that determines whether that bind is reachable; the measured `[P2-T11]` run
  satisfied it against a build carrying no fix. The new text names the `ApplicationBase` as the
  load-bearing element and names `Deedle.Frame.FromRecords`, which is production's entry point. Two
  sibling regions the re-rooting invalidated were rewritten line for line in the same pass: the
  build-output assumption at lines 392-394, which asserted an unverified fact about `TaskMaster.Test`
  output and now records the measured `QuickFiler.Test` one; and the Test Strategy domain-configuration
  paragraph at lines 431-434, which stated the superseded `ApplicationBase` rule. Both replacements have
  the same line count as the text they replace. No criterion was added or removed and the inventory
  remains nineteen. The criteria are amended by the planner and verified read-only by `[P1-T7]`; no
  executor task edits acceptance-criterion text.
- **The `FSharp.Core` `HintPath` alignment is declined as out of scope, not overlooked.** Aligning all six
  `HintPath` values on `lib/netstandard2.0` is the root-cause fix and it is tracked as a separate issue.
  Three of the six files are outside this plan's authorised write set and may be owned by sibling items,
  so editing them is a stop-and-report condition rather than a judgement call. `## R6.4` records the
  nondeterminism this leaves open, records that the remedy does not depend on which flavour is currently
  deployed, and records that issue 879 must not be reported as having closed the split.
- **Validator status.** The `mcp__drm-copilot__validate_orchestration_artifacts` MCP tool is not present
  in this planner session's tool surface, so the validator gate was NOT RUN by the planner. It must be run
  before the plan is treated as approved.
- **Two inbound figures corrected, per the authentication rule.** First, the delegation brief gave the
  branch head as `cdbe96835`. The loose ref at
  `.git/worktrees/bugs-2026-09-11-item-879` resolves
  `refs/heads/bug/deedle-netstandard-21-bind-unsatisfiable-in-production-879` to
  `e6a24be68a7d5607eb9eb5427f10b00e2ea061be`, so every citation in this revision was re-derived against
  the working tree as it stands rather than against the named commit, and the tree is the authority for
  every line number this revision cites. Second, the brief stated that the two Phase 0 console logs carry
  absolute host paths in the same way the Phase 2 one does. They do not: both were already redacted to
  `<repo-root>` for the repository path and contain zero occurrences of the host user name, while
  `expect-fail-build-console.2026-09-13T18-22.txt` contains 7,747 such lines out of 11,961. Each Phase 0
  log does retain about 140 lines of absolute toolchain paths beginning `C:\Program Files`, so both are
  still in scope for `[P5-T11]` and `[P5-T12]`, but for size and for toolchain paths rather than for the
  host user name.
- **Issue source.** The Bash tool is disabled in this session, so `gh issue view 879` could not be run.
  The material content of issue 879 and its two comments was taken from the spec's
  `### Update since issue.md was written` section, which the delegation brief authorises as a faithful
  reproduction.
