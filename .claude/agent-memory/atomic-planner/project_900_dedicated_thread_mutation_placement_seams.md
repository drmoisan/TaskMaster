---
name: project-900-dedicated-thread-mutation-placement-seams
description: "#900 planning seams (breadcrumb thread-affinity tests, Task.Run vs dedicated thread) — a guard-disabling mutation placed before the precondition tests the precondition's null-safety instead of the boundary assertion; the injected-operations overload reaches CaptureCurrent at ItemViewer.Breadcrumb.cs:80, not via :74-76/:364; potential entries are MCP-only so the executor hands off; backslash-before-quote corrupts -Command payloads; wrapped spec markers break single-line count gates"
metadata:
  type: project
---

Authored 2026-09-17 for `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900` (plan `plan.2026-09-16T23-27.md`, 46 tasks in six phases, spec amended to 0.3 by the planner).

**1. A mutation that disables the guard must not disable the precondition first.** The spec put
`ClearViewerDispatcher(scope.Viewer)` "immediately after constructing each ViewerScope", but the
rewritten tests read `scope.Viewer.UiDispatcher.CheckAccess()` inside the worker delegate as their
distinct-thread precondition. Nulling `_uiDispatcher` first makes that read throw
`NullReferenceException`, so the observed failure is `BeOfType` reporting an NRE and the boundary
assertions are never reached — the mutation then proves the precondition's null-safety, not the
non-vacuity the AC asks for. Place the insertion inside the delegate, after the precondition and
before the guarded call. **How to apply:** for every planned mutation, trace which assertion fails
FIRST under the mutated state and check it is the assertion the AC names; a mutation that fails
an earlier assertion is a different experiment.

**2. `EnsureBreadcrumbLifecycle(() => operations)` never reaches `CaptureCurrent()`.** Research and
spec cited `ItemViewer.Breadcrumb.cs:74-76` and `:364` as the throw site for a first-time
`InitializeBreadcrumbPipeline(provider, operations)` on a context-less thread. The factory at `:364`
returns the injected operations; the real throw site is `:80`, `BreadcrumbUiDispatcher.CaptureCurrent()`
evaluated as a `new BreadcrumbBridgeCoordinator(...)` argument (77-81). The three-argument
`ConfigureBreadcrumbDropDown` does reach it through `:241-243`, `:364`, `BreadcrumbPopupUiOperations.cs:80-81`.
`SynchronizationContext.Current` is read under `QuickFiler/Viewers/` only by viewer constructors and
`BreadcrumbUiDispatcher.cs:47`/`:271`, so nothing earlier on either path throws differently.

**3. Potential entries cannot be filed by the executor.** `feature-promotion-lifecycle` permits only
`mcp__drm-copilot__new_potential_bug_entry`, and the executor has no MCP surface. A spec sentence
that assigns filing to the executor is amended to "orchestrator, from the executor's handoff record",
and the plan's terminal phase writes that record (short_name + body per entry) instead of files under
`docs/features/potential/`.

**4. Backslash before a closing double quote corrupts a `-Command` payload.** `TrimEnd("\")` and
`-match "\\bin\\Debug\\"` are valid in a `.ps1`, but through `pwsh -NoProfile -Command '<payload>'`
the native argv parser reads `\"` as an escaped quote. Use `[char]92` and `-like "*\bin\Debug\*"`
(wildcard ends the string, not a backslash). Also filter TRX failures in PowerShell
(`if ($r.GetAttribute("outcome") -eq "Failed")`) rather than XPath, so no payload needs an embedded
quote at all. Payloads: no single quote anywhere, double-quoted literals only, one statement per line.

**5. A spec marker that wraps across lines defeats a single-line occurrence gate.** Two of five
`amended 2026-09-17 during planning` markers wrapped at the spec's 100-column reflow and the count
read 3; rewrap the marker onto one line before gating on its count. Same class as G6.

**6. The count `ClearViewerDispatcher(scope.Viewer);` 1 -> 3 -> 1 and `action();` 0 -> 1 -> 2 -> 1**
are the mechanical revert proofs for the two mutations, paired with `git diff --exit-code HEAD -- FILE`
after committing the fix mid-plan (so the revert is anchored) and SHA-256 equality with `FIX-HASH:`.
Committing the fix before the mutation phase is what makes `git checkout -- FILE` a safe recovery.

**Also verified:** this planner subagent has no `mcp__drm-copilot__validate_orchestration_artifacts`
tool (file-only surface); the hook `.claude/hooks/validate-planner-output.ps1` phase regex uses the
em-dash (line 238). `ApartmentThreadRunner.RunOnThread` (`UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs:79-104`)
is the helper shape to copy as a private member.

Related: [[project_781_excludefromcodecoverage_guard_plan_seams]], [[expect-fail-needs-a-synchronous-seam]],
[[acceptance-edits-must-be-false-before-true-after]], [[pwsh-command-payload-quoting]],
[[terminal-phase-planner-traps]], [[project_planner_mcp_validator_not_in_tool_surface]].
