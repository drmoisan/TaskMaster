# Handoff — Bug #418 (`svg-renderer-null-document-nre`)

- Handoff authored: 2026-08-04
- Branch: `bug/svg-renderer-null-document-nre-418` (pushed to `origin`)
- Last commit at handoff: `fd28d0fb` — `docs(418): add feature folder and wire SVGControl.Test into solution`
- Issue: <https://github.com/drmoisan/TaskMaster/issues/418>
- Work mode: `minor-audit` — the `## Acceptance Criteria` section of `issue.md` is the sole requirements source
- Route: `small`
- Complexity band: `C3` (floor `C3`, signal `cross_module_contract_change`); every delegation resolves to `opus` under `fable_policy: available`

## Why this handoff exists

Execution paused because the originating machine lacks the VSTO runtime assemblies `Microsoft.Office.Tools.Outlook.v4.0.Utilities` and `Microsoft.Office.Tools.Common.v4.0.Utilities` (`Version=10.0.0.0`, `PublicKeyToken=b03f5f7f11d50a3a`). Their absence produces `MSB3245` plus four `CS0234` errors in `TaskMaster/ThisAddIn.Designer.cs`, which prevents the analyzer and nullable solution builds from ever returning `EXIT_CODE: 0` and prevents `TaskMaster.Test` and `UtilitiesCS.Test` from producing build output at all.

**The receiving machine has those binaries installed.** That changes two things materially, both covered under "Mandatory first actions" below.

## Orchestrator state is NOT in the repository

`artifacts/` is gitignored (`.gitignore:57`), so `artifacts/orchestration/orchestrator-state.json` did not travel with the branch. The receiving orchestrator must reconstruct it from this document. All values needed to do so are in `## Checkpoint reconstruction` below.

---

## Mandatory first actions on the receiving machine

### 1. Re-run Phase 0 baseline capture — the committed baselines are invalid here

Every artifact under `evidence/baseline/` was captured on a host without the VSTO runtime. On a host that has it, those numbers are wrong and must not be used as the comparison basis. Specifically:

| Baseline | Value captured without VSTO | Expected on a complete host |
|---|---|---|
| Analyzer build | `EXIT_CODE: 1` — 4 errors / 44 warnings | Expected `EXIT_CODE: 0` — 0 errors |
| Nullable build | `EXIT_CODE: 1` — 5 errors / 5 warnings | Expected `EXIT_CODE: 0` — 0 errors |
| Repo-wide line coverage | 25.5305% (24628/96465) | Approximately 71% — prior sessions recorded that figure |
| Repo-wide branch coverage | 20.6824% (4910/23740) | Higher; 6 of 8 test assemblies ran here |
| Test assemblies discovered | 6 | 8 — `TaskMaster.Test` and `UtilitiesCS.Test` were absent |

Re-execute plan tasks `[P0-T5]` through `[P0-T10]` to capture host-correct baselines before resuming Phase 1. `[P0-T1]` (toolchain bootstrap) and `[P0-T2]`–`[P0-T4]` (policy and context reads) may be re-run or accepted as-is at the receiving orchestrator's discretion; the bootstrap is idempotent.

Write the new baselines to the same `evidence/baseline/` directory using a fresh ISO-8601 timestamp so the without-VSTO set remains auditable alongside the corrected set. Do not delete the existing artifacts.

### 2. Revert the AC-6 amendment

`issue.md` AC-6 currently reads "measured against the recorded baseline" with an amendment paragraph explaining that the absolute `EXIT_CODE: 0` form is unreachable. **That amendment is specific to the originating machine and must be reverted on the receiving machine**, where the absolute form is reachable.

Restore AC-6 to the absolute form:

> **AC-6 — Toolchain passes in a single clean pass.** CSharpier, the .NET analyzer build, the nullable/`TreatWarningsAsErrors` build, and `vstest.console.exe` all pass in one consecutive pass with no auto-fixes and no new diagnostics, per the C# toolchain order in `CLAUDE.md`.

Correspondingly, drop `human_interaction.requirements[]` entry `H-3` (it was resolved by `scope_change` only because the manual dependency could not be removed on the originating host; on this host there is no manual dependency to remove).

If the analyzer or nullable build still does not return `EXIT_CODE: 0` on the receiving machine after a clean restore, stop and report rather than re-applying the relative amendment — that would indicate a different, unexamined problem.

### 3. Confirm the executor's Phase 1 progress is intact

Plan tasks `[P1-T1]` through `[P1-T5]` are complete and checked off in `plan.2026-08-04T14-36.md`, and their file changes are in commit `fd28d0fb`:

- `[P1-T1]` — `SVGControl.Test` added to `TaskMaster.sln`: one `Project(...)` entry plus exactly twelve `{13AC39E6-DE06-4337-8EB0-41CE674A4C3B}.` configuration mappings across all six solution configurations. File remains UTF-8 with BOM, CRLF.
- `[P1-T2]` — `SVGControl.Test/app.config` ExCSS redirect corrected from the non-existent `4.2.4.0` to `4.3.1.0`. **Satisfies AC-10.**
- `[P1-T3]` — package restore succeeded via the **primary** route, not the retarget contingency. Once `[P1-T1]` made the project a solution member, `msbuild /t:Restore /p:RestorePackagesConfig=true` installed all seven pinned versions from nuget.org (`Installed: 7 package(s)`, `0 Error(s)`). No substitutions were made. Evidence: `evidence/other/package-restore-decision.2026-08-04T14-36.md`.
- `[P1-T4]` — compile-time `Svg 3.4.7` reference added to `SVGControl.Test` (`packages.config` entry plus `<Reference Include="Svg, Version=3.4.0.0, ...>` with a HintPath that resolves on disk).
- `[P1-T5]` — `SVGControl.Test` builds standalone, `EXIT_CODE: 0`. `EnsureNuGetPackageBuildImports` no longer fires. `SVGControl.Test/bin/Debug/SVGControl.Test.dll` exists. Evidence: `evidence/qa-gates/svgcontrol-test-build.2026-08-04T14-36.md`.

These are host-independent and should not need redoing. Verify them rather than re-executing them.

---

## The open blocker — resume point

Execution halted at **`[P1-T6]`** (solution analyzer build) with `SCOPE_EXCEEDED`.

### What happened

The build reproduced the baseline exactly on errors and introduced **zero new analyzer diagnostics**, but emitted one warning absent from the baseline:

```
warning MSB3277: Found conflicts between different versions of
"System.Runtime.CompilerServices.Unsafe" that could not be resolved.
```

Sole emitter: `SVGControl.Test\SVGControl.Test.csproj`.

### Root cause — a pre-existing pin divergence

| Project | `packages.config` pin | `<Reference>` `Version=` |
|---|---|---|
| `SVGControl/SVGControl.csproj` | `System.Runtime.CompilerServices.Unsafe` **6.1.2** | `6.0.3.0` |
| `SVGControl.Test/SVGControl.Test.csproj` | **6.0.0** | `6.0.0.0` |

`SVGControl/bin/Debug/System.Runtime.CompilerServices.Unsafe.dll` was verified on disk as assembly version `6.0.3.0`. The divergence pre-exists in the test project and becomes observable only because `[P1-T1]` made that project build for the first time. It is an unavoidable consequence of delivering AC-9.

`MSB3277` is an MSBuild `ResolveAssemblyReferences` diagnostic. It cannot be cleared from any of the eight `.cs` files in the Scope Lock's pre-existing-`SVGControl.Test`-files list, so the executor correctly refused to improvise and escalated.

**This warning is host-independent and will reproduce on the receiving machine.** Re-baselining does not make it go away.

### Decision already taken — apply it

The originating orchestrator evaluated the executor's two options and chose to **align the pin rather than accept the warning**. Accepting it is not risk-free: the `SVGControl` ProjectReference copies the `6.0.3.0` assembly into the test output directory while `SVGControl.Test/app.config:35` carries `<bindingRedirect oldVersion="0.0.0.0-6.0.0.0" newVersion="6.0.0.0" />` for that same assembly — a redirect naming a version that is not in the output directory. That is the same defect class as bug #418 itself, and it would sit inside the very test run that proves AC-1.

This widens the Scope Lock, so it goes through `atomic-planner` and re-preflight rather than being improvised by the executor. **The exact planner delta was authored but not yet applied.** It is reproduced verbatim in `## Pending planner delta` below.

---

## Pending planner delta (not yet applied)

Delegate to `atomic-planner` with `DIRECTIVE: MINIMAL-AUDIT PLAN REQUIRED — REVISION PASS 2 (targeted Scope Lock delta)`, revising `plan.2026-08-04T14-36.md` **in place** (no timestamped sibling, per the Plan-Path Continuity Contract). Apply exactly the following and nothing more.

### 1. Scope Lock — widen two existing bullets

To the `SVGControl.Test/SVGControl.Test.csproj` and `SVGControl.Test/packages.config` bullets, append:

> …and, for `[P1-T6]` / `[P1-T7]` diagnostic remediation only, the `<package>` version and the corresponding `<Reference>` `Version=` and `<HintPath>` for a package whose pin diverges from the version pinned by `SVGControl/SVGControl.csproj`, aligned to the version verified present under `packages/` and verified on disk in `SVGControl/bin/Debug/`.

To the `SVGControl.Test/app.config` bullet — which currently reads "ExCSS binding redirect only, line 23" — append:

> …and, for `[P1-T6]` / `[P1-T7]` diagnostic remediation only, the `<bindingRedirect>` `oldVersion` upper bound and `newVersion` for a package realigned under the clause above, set to the assembly version verified on disk. A pin realignment that leaves a stale redirect is not an acceptable end state.

### 2. New task `[P1-T6a]`, inserted between `[P1-T6]` and `[P1-T7]`

Use the suffixed ID `P1-T6a` specifically so `[P1-T7]` through `[P1-T24]` keep their current numbers. `[P1-T1]`–`[P1-T5]` are already checked off against the current numbering and cross-references throughout the plan point at the current IDs; renumbering now would invalidate both.

```
- [ ] [P1-T6a] Align the diverged `System.Runtime.CompilerServices.Unsafe` pin in
  `SVGControl.Test` to the version that `SVGControl` actually deploys, clearing the
  MSB3277 conflict introduced by `SVGControl.Test` entering the solution build.
  Before editing, verify on disk: (a) the assembly version of
  `SVGControl/bin/Debug/System.Runtime.CompilerServices.Unsafe.dll`, and (b) the
  `packages/System.Runtime.CompilerServices.Unsafe.<version>/lib/net462/` folder that
  supplies it. Then set, in `SVGControl.Test`: the `packages.config` `<package>`
  version, the `<Reference>` `Version=` and `<HintPath>`, and the `app.config`
  `<bindingRedirect>` `oldVersion` upper bound and `newVersion`, all consistent with
  those verified values and with `SVGControl/SVGControl.csproj`. Edit no other project.
  - Acceptance: `evidence/qa-gates/unsafe-pin-alignment.<ts>.md` created
    containing `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording
    the verified on-disk assembly version, the verified `packages/` folder name, the
    before and after values of all four edited settings, and a re-run of the
    `[P1-T6]` solution analyzer build showing `MSB3277 count: 0` and
    `New diagnostics vs baseline: 0`
```

### 3. `[P1-T6]` and `[P1-T7]` — resolve the reading ambiguity

`[P1-T6]`'s text says "introduces no **analyzer diagnostic** that was absent from the baseline", under which the task passes because `MSB3277` is an MSBuild diagnostic rather than an analyzer diagnostic. The delegation prompt said "any error or warning not present in the baseline". Add a clarifying clause to both tasks:

> "New diagnostic" for this task means any error or warning — analyzer, compiler, or MSBuild — whose code was absent from the corresponding Phase 0 baseline artifact. An MSBuild diagnostic that cannot be cleared from a Scope Lock `.cs` file is remediated under `[P1-T6a]` when it originates in `SVGControl.Test`, and is reported as `SCOPE_EXCEEDED` when it originates anywhere else.

### 4. Record as Design Decision 10

> **Design Decision 10 — the diverged `Unsafe` pin is aligned, not accepted.** `SVGControl.Test` pinned `System.Runtime.CompilerServices.Unsafe` 6.0.0 while `SVGControl` pins 6.1.2 (assembly version 6.0.3.0). Accepting the resulting MSB3277 was rejected because the ProjectReference copies 6.0.3.0 into the test output while `SVGControl.Test/app.config:35` redirected to 6.0.0.0 — a redirect naming a version absent from the output directory, which is the same defect class as bug #418. The pin, reference, HintPath, and binding redirect are aligned together under `[P1-T6a]`. Scope is confined to `SVGControl.Test`; the equivalent audit of other projects' redirects is out of scope and tracked with the deferred Fizzler finding.

### 5. Add to the Explicitly-out-of-scope list

> Auditing or correcting `System.Runtime.CompilerServices.Unsafe` binding redirects in any project other than `SVGControl.Test`. Several project `app.config` files carry redirects whose `newVersion` may not match the deployed assembly; that audit is the same class as the deferred Fizzler finding and belongs to a separate issue.

### 6. Add to the risk or open-questions section

> `SVGControl.Test` pins `MSTest.TestAdapter` / `MSTest.TestFramework` **3.1.1** while `UtilitiesCS.Test` uses **4.2.2**. `[P2-T6]` runs `Invoke-MSTestWithCoverage.ps1 -SearchRoot .`, placing both adapter versions in a single `vstest.console.exe` run. If that run misbehaves, this mismatch is the first thing to check, and the remedy is the `[P1-T3]` retarget mechanism. Flagged, not pre-emptively changed.

### 7. Informational, no task change

`[P1-T3]` took the primary route, not the anticipated retarget contingency (see above). Update any plan text that presumes the contingency was taken.

### Do not change

Everything else: Design Decisions 1-9, all other task bodies and acceptance clauses, all evidence paths, the three-phase structure, the AC-11 non-executable handoff encoding, Phase 2 unconditionality, and the existing `[x]` state on Phase 0 and `[P1-T1]`–`[P1-T5]`.

### After the delta

1. Re-run `mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"`.
2. Re-run `DIRECTIVE: PREFLIGHT VALIDATION ONLY` through `atomic-executor` until `PREFLIGHT: ALL CLEAR`.
3. Resume execution at `[P1-T6]`.

---

## Settled design decisions — do not relitigate

These were decided by the user and are recorded in `issue.md` and in the plan's Design Decisions section.

**D1 — Failure mode (AC-3, AC-4).** The byte-array `SvgRenderer` constructors **must not throw**. They log the cause and degrade, leaving `_doc` null without dereferencing it, and `_original` becomes `Size.Empty`. Rationale: `PictureBoxSVG` is constructed by designer-generated code in eleven forms including `QuickFiler/Viewers/ItemViewer`, which runs inside the Outlook add-in; throwing would convert a blank-icon degradation into a control-construction failure for end users. A separate fail-fast API (`TryGetSvgDocument` plus `GetSvgDocumentOrThrow`) exists for callers that want it, and `GetSvgDocument` keeps its tolerant `null` contract.

**D1a — Dual diagnostic channel (AC-3).** The diagnostic must reach both `log4net` **and** `System.Diagnostics.Trace`. `SVGControl` declares a `log4net` logger but there is no evidence an appender is configured inside `devenv.exe`, so a `log4net`-only message may surface nowhere the operator can see it. `Trace` output appears in the Visual Studio Output window.

**D2 — Test scope (AC-9, AC-10).** `SVGControl.Test` is repaired within this change rather than deferred, and the tests live in the project that owns the code.

**D3 — Designer verification (AC-11, AC-7).** Resolved as a permitted `exception` with a runbook at `runbooks/verify-winforms-designer-load.runbook.md`. AC-11 is **not** executor-satisfiable; `[P2-T10]` is record-only and its acceptance requires `- [ ] **AC-11` to still be present in `issue.md`.

## Confirmed root cause — do not re-derive

Established by `research/2026-08-04T15-05-svg-renderer-null-document-research.md` and verified against assembly metadata:

1. `packages/Svg.3.4.7/lib/net481/Svg.dll` (identity `Svg, Version=3.4.0.0`) carries an assembly reference to **`ExCSS, Version=4.2.3.0`**. The only ExCSS deployed anywhere is **`4.3.1.0`**.
2. The WinForms designer loads `SVGControl.dll` into `devenv.exe` (legacy in-process, because this is `net481`, not `DesignToolsServer.exe`). `devenv.exe.config` has no ExCSS entry, so the bind fails with `FileNotFoundException`.
3. `SvgRenderer.GetSvgDocument` catches `Exception` and returns `null`; the byte-array constructors then dereference it at `SvgRenderer.cs:129` and `:138`.
4. The existing `AssemblyResolve` fallback at `SvgRenderer.cs:44-104` **is reached but returns `null`**. Strategy 1 fails because the failing request is the first ExCSS load in the AppDomain. Strategy 2's `Assembly.Load(new AssemblyName("ExCSS"))` binds against the host AppDomain's `ApplicationBase` — the Visual Studio directory — not the directory holding `SVGControl.dll`. AC-8 fixes this with ordered directory probing via `Assembly.LoadFrom`.
5. **There is no `TaskMaster.exe`.** `TaskMaster.csproj` is `OutputType=Library` with the VSTO project GUID. Production is an add-in inside `OUTLOOK.EXE`; the VSTO runtime's per-add-in AppDomain uses `TaskMaster.dll.config`, which redirects correctly. **Production does not reproduce this bug.**

Two findings that constrain the fix:

- `SvgDocument.Open<T>` **returns `null` without throwing** for element-free input (`Array.Empty<byte>()`). There are therefore two null-producing paths, and only the throwing one can carry an `InnerException`.
- The ExCSS reference is bound at **JIT time** for the whole `SvgDocument.Create<T>` method body regardless of whether the SVG has a `<style>` element. Removing `<style>` from the default SVG is **not** a viable remedy.

Eliminated: the default SVG payload is well-formed and pure ASCII, so `Encoding.ASCII.GetBytes` is faithful and malformed XML is not the cause.

## Explicitly out of scope for #418

- **Fizzler binding redirects.** Thirteen `app.config` files across nine projects redirect Fizzler to `newVersion="1.3.0.0"` while `1.3.1.0` is deployed. Research classified this as latent and currently inert — nothing in the deployed graph carries a Fizzler assembly reference, and the `using Fizzler;` at `SVGControl/PictureBoxSVG.cs:14` is unused and emits no `AssemblyRef`. **A follow-up issue for this has not yet been filed.**
- `System.Runtime.CompilerServices.Unsafe` redirects in any project other than `SVGControl.Test`.
- Removing `<style>` from the default SVG (see above).
- Installing Office Developer Tools (moot on the receiving machine).

## Known repository defect observed, not fixed

`.claude/hooks/validate-planner-output.ps1:121` uses the phase pattern `'^### Phase (?<Phase>\d+)\s+-\s+(?<Title>.+)$'`, which requires an ASCII hyphen. The `atomic-plan-contract` skill, the `atomic-planner` agent definition, and every existing plan in this repository use an **em-dash** in phase headings, which that pattern cannot match. It did not block this run. The correct remedy is to reconcile the hook with the contract, not to convert plans to ASCII hyphens.

## Process note

`mcp__drm-copilot__potential_to_issue` reported a `destination_path` of `docs/features/potential/promoted/2026-08-04-svg-renderer-null-document-nre.md` but removed the source file without creating that destination. No content was lost — it is fully preserved in this folder's `issue.md` and in GitHub issue #418. The reconstructed checkpoint's `relativeFile` therefore names a path that does not exist on disk.

---

## Checkpoint reconstruction

Recreate `artifacts/orchestration/orchestrator-state.json` with these values. The MCP orchestrator-state validator is stricter than the SubagentStop hook — see `.claude/agent-memory/orchestrator/orchestrator-state-validator-divergence.md`. In particular: `delegation_receipts` must be an **array** of rich entries (`step`, `agent_name`, `agent_id`, `skill_source`, `started_at`, `completed_at`, `result_signal`, `artifact_paths`); keep promotion MCP receipts under a separate key such as `delegation_receipts_promotion`; `relativeFile` is a required key; and `step5_status` through `step10_status` must use the enum `{not-applicable, pending, delegated, verified, blocked}`.

| Field | Value |
|---|---|
| `objective` | Investigate the preliminary diagnosis of a NullReferenceException from SVGControl/SvgRenderer.cs and orchestrate a fix. |
| `route_id` / `path_selected` | `small` |
| `promotion-type` | `bug` |
| `short-name` | `svg-renderer-null-document-nre` |
| `long-name` | `2026-08-04-svg-renderer-null-document-nre` |
| `issue-num` / `issue_num` | `418` |
| `work-mode` | `minor-audit` |
| `feature-folder` | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418` |
| `plan-path` | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md` |
| `branch` | `bug/svg-renderer-null-document-nre-418` |
| `relativeFile` | `docs/features/potential/promoted/2026-08-04-svg-renderer-null-document-nre.md` (see process note — file absent) |
| `completed_steps` | `S1_scale_assessment`, `S2_change_budget_routing`, `S3_promotion`, `S3a_research`, `S3b_human_exception_runbook`, `S4_atomic_planning`, `S5_preflight` |
| `next_step` | `S6_atomic_execution` |
| `blocked_reason` | `none` |
| `model_budget.fable_policy` | `available` (from `config/orchestration-routing.json`) |

Complexity assessments — one entry per phase, all `band: C3`, `floor: C3`, `signals_present: ["cross_module_contract_change"]`: `S3a_research`, `S3b_human_exception_runbook`, `S4_atomic_planning`, `S5_preflight`, `S6_atomic_execution`.

Model routing receipts — all `complexity_band: C3`, `fable_policy: available`, `table_model: opus`, `clamped_from: null`, `model: opus`: `task-researcher`, `human-exception-runbook`, `atomic-planner`, `atomic-executor`. Add `feature-review`, `pr-author`, and `commit-message` receipts as those delegations occur.

Preflight: `iterations: 2`, `final_status: clear`. Pass 1 returned `PREFLIGHT: REVISIONS REQUIRED` with 6 blocking and 7 non-blocking findings; pass 2 returned `PREFLIGHT: ALL CLEAR`.

Human-interaction requirements:

- `H-1` — designer load verification cannot be automated. Response `exception`; `runbook_path` = `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/runbooks/verify-winforms-designer-load.runbook.md`. Satisfies AC-11.
- `H-2` — capturing the observed exception identity in the designer host. Response `exception`; same `runbook_path`. Satisfies AC-7. The research artifact notes favourable sequencing: because the fix stops discarding the exception, one post-fix designer open satisfies both H-1 and H-2.
- `H-3` — **drop this entry on the receiving machine.** It recorded the VSTO absence as a `scope_change` amending AC-6; that condition does not hold here.

Routing-contract receipts for the `small` route (`config/orchestration-routing.json`):

- `required_agents`: `atomic-planner`, `atomic-executor`, `feature-review` — the first two have receipts; `feature-review` has not run yet.
- `required_skills`: `orchestrate`, `feature-promotion-lifecycle`, `atomic-plan-contract`, `acceptance-criteria-tracking`, `pr-context-artifacts`, `pr-base-branch-merge-base` — all read; record `evidence: "read:.claude/skills/<name>/SKILL.md"`.
- `required_mcp_tools`: `new_potential_entry`, `potential_to_issue`, `new_active_feature_folder`, `collect_pr_context`, `validate_orchestration_artifacts`.

**Known route-contract deviation:** the `small` route requires a `new_potential_entry` receipt, but this is a bug, so `feature-promotion-lifecycle` prescribes `new_potential_bug_entry` — which is the tool actually called. No `new_potential_entry` receipt exists, and fabricating one would violate the truthful-receipt rule. Routing-contract validation under `require_complete` may flag this. Separately, the `large` route's `required_skills` names `orchestrator-workflow` and `repo-automation-adapter`, neither of which exists under `.claude/skills/`; that route therefore cannot produce complete, truthful skill receipts.

---

## Remaining work after the blocker clears

1. `[P1-T6a]` pin alignment, then `[P1-T6]` and `[P1-T7]` solution gates.
2. `[P1-T8]`/`[P1-T9]` — the `[expect-fail]` regression tests, capturing the pre-fix failure as evidence (AC-1, Bugfix Workflow in `CLAUDE.md`).
3. `[P1-T10]` onward — the production fix in `SVGControl/SvgRenderer.cs` (AC-2, AC-3, AC-4) and the `AssemblyResolve` directory-probing fix (AC-8). **No production C# has been modified yet; `SVGControl/SvgRenderer.cs` is untouched.**
4. `[P1-T24]` — check off AC-1, AC-2, AC-3, AC-4, AC-7, AC-8, AC-9, AC-10.
5. Phase 2 final QC loop; `[P2-T9]` checks off AC-5 and AC-6.
6. Orchestrator: `git add -A`, delegate `Agent(commit-message)`, commit.
7. Orchestrator: delegate `Agent(feature-review)`. Supply only the resolved base branch and merge-base SHA, the feature folder path, refreshed PR-context artifact pointers, the AC source (`issue.md`), the canonical issue-number line, and a neutral instruction to execute the full `feature-review-workflow` contract. Do **not** narrow scope in the prompt.
8. Remediation loop R1–R5 until zero blocking findings (cap: 3 passes).
9. Human step: execute `runbooks/verify-winforms-designer-load.runbook.md`, capture evidence to `evidence/regression-testing/`, then check off AC-11 and complete AC-7.
10. PR creation gate: refresh PR context, run the orchestrator-state validator with `--require-pr-creation-ready`, record `pr_author_preflight`, then delegate `Agent(pr-author)`. The orchestrator must not call `gh pr create` directly.
11. S9 CI green gate against the live PR head SHA.
12. File the deferred Fizzler follow-up issue.
