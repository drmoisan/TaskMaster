# Phase 3 QC Step 3 — Post-Format File-Size Audit (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P3-T3]
Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; git status --porcelain | ForEach-Object { $_.Substring(3) } | ForEach-Object { if (Test-Path $_) { '{0}={1}' -f $_, (Get-Content $_ | Measure-Object -Line).Lines } }"`
EXIT_CODE: 0

Corroborating command: `wc -l TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs TaskMaster/Ribbon/RibbonExplorer.xml`

## The only source path in the change set

| Path | `Measure-Object -Line` | Physical (`wc -l`) | 500-line cap | Verdict |
|---|---|---|---|---|
| `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | 288 | **318** | 500 | **PASS** — 182 lines of headroom |

This is the **only** `.cs`, `.csproj`, `.xml`, or `.sln` path present in the working-tree change set. The binding figure is the physical count of **318**, per the measurement-method reconciliation recorded in `evidence/remediation-baseline/file-line-counts.2026-08-08T14-52.md`: `Measure-Object -Line` contributes zero for an empty string and therefore under-reports a `.cs` file by its blank-line count (30 here).

## `TaskMaster\Ribbon\RibbonExplorer.xml` — recorded explicitly

The task text requires this path be recorded against both the 519-line merge-base figure accepted by AC25 and the 527-line gate for this cycle.

| Reference | Lines |
|---|---|
| Merge-base `003c5715` (the figure AC25 accepts as a pre-existing exception) | 519 |
| Post-implementation-cycle | 539 |
| F2 gate ceiling for this cycle | 527 |
| **Measured now** | **539** |

The path **does not appear in the change set** because it takes a **zero-line diff** from this remediation cycle. The P2-T1 collapse that would have brought it to 524 was reverted at [P3-T2]: CSharpier 1.3.0 formats XML and mandates the multi-line form once the required `getEnabled` attribute pushes those lines from 78 to 116 characters against a 100-column print width, so the F2 target of 527 or below is unreachable while the mandatory format gate must pass. The measured root cause and the escalation are in `evidence/qa-gates/f2-formatter-conflict.2026-08-08T14-52.md`.

The file therefore **remains at 539 lines and above the 527 gate**. This is recorded as an unmet F2 objective, escalated to the orchestrator, and explicitly **not** reported as a pass. It is not a regression introduced by this cycle: 539 is the state this cycle inherited and the state it leaves.

## Exemptions applied

Per `.claude/rules/general-code-change.md`, the 500-line cap applies to production code, test code, and reusable script files. Markdown documentation files are exempt. Every other entry in the change set is a Markdown documentation, evidence, or agent-memory file under `docs/features/` or `.claude/agent-memory/` and is exempt on that basis. The largest is `remediation-plan.2026-08-08T14-26.md` at 245 counted lines (331 physical) — a Markdown plan document, exempt.

Full change-set listing:

```text
.claude/agent-memory/atomic-executor/MEMORY.md=82
.claude/agent-memory/atomic-executor/project_preflight_mergebase_diff_gates_need_commit_cadence.md=14
.claude/agent-memory/atomic-planner/MEMORY.md=45
.claude/agent-memory/feature-review/MEMORY.md=57
.claude/agent-memory/feature-review/project_pr-context-summary-misclassifies-cs.md=9
TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs=288
.claude/agent-memory/atomic-planner/embedded-resource-failproof-rebuild-gate.md=14
.claude/agent-memory/feature-review/project_nullable_build_gate_is_vacuous.md=14
.claude/agent-memory/feature-review/project_package-counter-delta-corroborates-new-type-coverage.md=14
.claude/agent-memory/feature-review/project_two-vstest-binaries-binding-redirect.md=14
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/code-review.2026-08-08T14-15.md=33
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/other/phase1-build-postrestore.2026-08-08T14-52.md=33
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/other/phase1-build-premutation.2026-08-08T14-52.md=30
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/other/phase2-build.2026-08-08T14-52.md=34
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/qa-gates/csharpier-check.2026-08-08T14-52.md=37
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/qa-gates/csharpier-format.2026-08-08T14-52.md=28
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/qa-gates/f2-formatter-conflict.2026-08-08T14-52.md=63
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/qa-gates/f2-xml-line-count.2026-08-08T14-52.md=27
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/qa-gates/f2-xml-wellformed.2026-08-08T14-52.md=18
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/regression-testing/f1-assertion-shape.2026-08-08T14-52.md=65
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/regression-testing/f1-fail-proof.2026-08-08T14-52.md=48
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/regression-testing/f1-green-before-mutation.2026-08-08T14-52.md=30
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/regression-testing/f1-mutated-assembly.2026-08-08T14-52.md=29
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/regression-testing/f1-mutation-applied.2026-08-08T14-52.md=37
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/regression-testing/f1-mutation-restored.2026-08-08T14-52.md=22
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/regression-testing/f1-pass-after-restore.2026-08-08T14-52.md=36
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/regression-testing/f2-ribbon-xml-tests.2026-08-08T14-52.md=37
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/remediation-baseline/=0
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/feature-audit.2026-08-08T14-15.md=73
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/policy-audit.2026-08-08T14-15.md=167
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/remediation-inputs.2026-08-08T14-26.md=32
docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/remediation-plan.2026-08-08T14-26.md=245
docs/features/potential/promoted/2026-08-08-nullable-gate-cannot-fail-incremental-build.md=57
```

Note on one benign diagnostic: `git status --porcelain` collapses the wholly-untracked `evidence/remediation-baseline/` into a single directory entry, so `Get-Content` emitted `Unable to get content because it is a directory` for it and the loop recorded `=0`. This is a property of the porcelain's directory collapsing, not a missing or empty file; the directory holds ten evidence artifacts, all Markdown or JaCoCo XML and all exempt from the cap.

## Binary outcome

- **Every `.cs` path is at or under 500 lines** — satisfied: the single `.cs` path is at 318.
- **`TaskMaster\Ribbon\RibbonExplorer.xml` at or under 527 and strictly below 539** — **NOT satisfied.** The file is at 539. Recorded here as an unmet F2 objective with its measured cause, rather than reported as a pass. See `evidence/qa-gates/f2-formatter-conflict.2026-08-08T14-52.md`.
