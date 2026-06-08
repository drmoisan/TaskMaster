# Policy Audit — Issue #166 (worktrees-missing-claude-dir)

- Generated: 2026-05-27T11-47
- Reviewer: feature-review agent
- Work Mode (from issue.md): minor-audit
- Base branch (resolved): development
- Merge-base SHA: b7bd81626a512c70c264a8badad5fa5691ca1c16
- Head SHA: ca531c67b1a3605562894a5fe49d7cd38b382819
- Range: b7bd81626a512c70c264a8badad5fa5691ca1c16..ca531c67b1a3605562894a5fe49d7cd38b382819
- PR context summary: artifacts/pr_context.summary.txt
- PR context appendix: artifacts/pr_context.appendix.txt

> Template note: The workflow specifies resolving review templates through the MCP
> `resolve_policy_audit_template_asset` tool. That MCP tool and the shared template/skill
> assets referenced by the workflow (`policy-audit-template-usage`,
> `evidence-and-timestamp-conventions`, etc.) are not present in this branch's
> `.claude/skills/` tree. This artifact is therefore constructed to the required artifact
> shape documented in the feature-review-workflow SKILL contract. This deviation is recorded
> as UNVERIFIED template provenance, not a silently substituted source.

## Overall Verdict

FAIL (remediation required).

The functional fix is correct and minimal, but the branch diff contains 17 newly-tracked
PowerShell production files for which mandatory coverage verification fails: the only
PowerShell coverage artifact present is stale and reports 0% repo-wide line coverage, well
below the 80% floor. Coverage verification is mandatory for every language with changed
files in the branch diff.

## Scope Determination (feature-vs-base, authoritative)

Scope is the full branch diff against the resolved base `development` at merge-base
`b7bd81626a512c70c264a8badad5fa5691ca1c16`. Determined via
`git diff --name-status <merge-base>..<head>`.

Changed files: 70 total.

| Category | Count | Notes |
|---|---|---|
| Markdown (`.md`) | 45 | `.claude/agents/*`, `.claude/rules/*`, `.claude/skills/**`, feature docs |
| PowerShell (`.ps1`) | 17 | all under `.claude/hooks/`, all status `A` (added/newly tracked) |
| Text evidence (`.txt`) | 9 | under feature `evidence/` subtree |
| JSON (`.json`) | 1 | `.claude/settings.json` (added) |
| `.gitignore` | 1 | modified (the single hand-edited production change) |

Mechanism: the change un-ignores the `.claude/` subtree. Because the orchestrator pre-review
`git add -A` step then stages the previously-untracked tooling, the entire `.claude/` tree
appears as added files in the branch-vs-base diff. The executor's own evidence
(`evidence/qa/166-git-add-dryrun.txt`) confirms 17 `.ps1` hook files become tracked by this
change.

## Rejected Scope Narrowing

The following narrowing assertions were detected in the feature's own scoping documents and
the executor's QA evidence. Per the non-negotiable scope invariant, they are rejected for
audit purposes; the audit proceeds against the full branch diff.

1. From `plan.2026-05-27T11-32.md` (Scope and Constraints):
   > "Single production file: `.gitignore` (repository root) is the only code/config file changed by this plan."
   > "Out of scope: No edits to any `.claude/` file."
   Justification for rejection: the branch-vs-base diff carries 70 added files including 17
   PowerShell production files; audit scope is the diff, not the plan's self-described edit set.

2. From `evidence/qa/166-toolchain-summary.txt` (P4-T4):
   > "Change set: single edit to ...\.gitignore ... No source files changed."
   > "(e) Coverage — N/A. No production C# code changed; repository-wide coverage is unaffected."
   Justification for rejection: 17 `.ps1` PowerShell source files are added in the branch diff.
   Coverage for PowerShell is mandatory and cannot be marked N/A for a language with changed files.

3. From `issue.md` (Resolution / Documented exceptions):
   > "C# toolchain N/A. No `*.cs` ... files changed".
   This C#-only N/A is accepted (no `.cs` files are in the diff), but it does not address the
   PowerShell files that are in the diff. The absence of any PowerShell coverage treatment is
   the gap.

## Coverage Verification

Coverage is mandatory for every language with changed files in the branch diff. Verdicts
below are based on inspecting pre-existing coverage artifacts (no regeneration performed).

| Language | Changed files in diff | Coverage artifact | Artifact present | Repo-wide line coverage | Verdict |
|---|---|---|---|---|---|
| PowerShell (pester) | 17 (`.ps1`, all added) | `artifacts/pester/powershell-coverage.xml` | Yes (stale) | 0.00% (covered=0, missed=284) | FAIL |
| C# (.NET) | 0 | `artifacts/csharp/coverage.xml` | No | n/a | N/A (zero changed files) |
| Python | 0 | `artifacts/python/lcov.info` | No | n/a | N/A (zero changed files) |
| TypeScript | 0 | `coverage/lcov.info` | No | n/a | N/A (zero changed files) |

PowerShell coverage FAIL detail:

- Repo-wide: the Jacoco report total LINE counter is `missed="284" covered="0"` => 0.00% line
  coverage, below the 80% repo-wide floor. FAIL.
- Artifact staleness: the report header is dated `Pester (05/06/2026 21:49:23)`, which predates
  the feature work (2026-05-27). It does not reflect a run against this branch's tests.
- Per-file: the artifact enumerates only 5 of the 17 changed hook files
  (`check-powershell-test-purity.ps1`, `check-python-test-purity.ps1`,
  `enforce-powershell-batch-budget.ps1`, `enforce-python-batch-budget.ps1`, `validate-bash.ps1`),
  each with `covered="0"`. The remaining 12 changed hooks
  (`enforce-checkpoint-monotonic.ps1`, `enforce-evidence-locations.ps1`,
  `enforce-feature-folder-order.ps1`, `enforce-pr-author-skill.ps1`,
  `enforce-prd-feature-before-planner.ps1`, `enforce-promotion-mcp-only.ps1`,
  `validate-executor-output.ps1`, `validate-feature-review-coverage.ps1`,
  `validate-orchestrator-output.ps1`, `validate-planner-output.ps1`,
  `validate-required-artifact-output.ps1`, `validate-task-researcher-output.ps1`) have no
  coverage data at all. New files require >= 90% line coverage; observed is 0% or absent. FAIL.

This verdict is consistent with the in-repo SubagentStop hook
`.claude/hooks/validate-feature-review-coverage.ps1`, which maps `.ps1` to PowerShell from the
PR summary changed-files list and requires an explicit FAIL verdict on a PowerShell coverage
row when repo-wide coverage is below 80%.

## Toolchain Checks

Default order: formatting, lint, type check, tests, coverage. The feature touches PowerShell
(`.ps1`) production files in the branch diff, so the PowerShell toolchain (format via
PoshQC `Invoke-Formatter`, analyze via PSScriptAnalyzer, test via Pester) applies to the diff.

| Step | Language | Status | Evidence / Reason |
|---|---|---|---|
| Formatting | PowerShell | UNVERIFIED | No PoshQC format evidence for this branch. MCP `mcp__drm-copilot__run_poshqc_format` not run in this review session; no recorded format result against the 17 changed hooks. |
| Lint / analyze | PowerShell | UNVERIFIED | No PSScriptAnalyzer (`run_poshqc_analyze`) evidence for the changed hooks on this branch. |
| Type check | PowerShell | N/A | Type checking is not applicable to PowerShell per `.claude/rules/powershell.md`. |
| Tests | PowerShell | FAIL | No Pester run evidence against this branch; the only coverage artifact is stale (dated 2026-05-06) and shows 0 covered lines. |
| Tests (git behavior) | n/a | PASS | Adapted `git check-ignore` verification reproduced live: tooling subtrees print nothing and exit 1 (no longer ignored); `settings.local.json` and `agent-memory/` still print and exit 0 (Issue #149 invariant preserved). |
| Coverage | PowerShell | FAIL | See Coverage Verification section: 0.00% repo-wide. |
| C# toolchain | C# | N/A | No `.cs`/`.csproj`/`.props`/`.targets` in the diff. Accepted N/A. |

C# toolchain N/A is accepted because zero C# files changed. PowerShell toolchain steps are
either UNVERIFIED (format, lint — no evidence available in this review environment) or FAIL
(tests, coverage). Per the workflow, UNVERIFIED toolchain steps for a language with changed
files combined with a coverage FAIL are sufficient to require remediation.

## Evidence Location Compliance

The reviewer scanned the branch diff for files written under non-canonical evidence paths
(`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`).

- Result: none found. All feature evidence is correctly under
  `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/evidence/<kind>/`
  (`baselines/`, `qa/`, `regression/`).
- `validate_evidence_locations.py --root .`: UNVERIFIED — the script is not present in the
  repository (searched repo root and `scripts/`). The in-repo PreToolUse hook
  `.claude/hooks/enforce-evidence-locations.ps1` exists but is not the script named in the
  contract. Manual diff scan was performed instead and found no violations.

Verdict: PASS (no non-canonical evidence files in the branch diff), with the validator-script
gate recorded UNVERIFIED due to script absence.

## General Code Change Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Single, minimal, targeted change to fix the defect | PASS | `.gitignore` diff replaces the bare `.claude` entry with two targeted ignores; no other lines changed. |
| Comments explain "why" | PASS | The two added comment lines cite Issue #166 (tracking rationale) and Issue #149 (exclusion rationale). |
| File size limit (<= 500 lines) | PASS | `.gitignore` and the added files are documentation/config/scripts; no production file flagged over 500 lines was introduced by the hand edit. |
| Bugfix workflow: failing repro first | PARTIAL | A deterministic `git check-ignore` repro was captured pre-fix (`evidence/regression/166-pre-fix-check-ignore.txt`). A standard unit test is not applicable; the documented exception (external git process prohibited in unit tests) is reasonable for `.gitignore` behavior. |
| Toolchain run after change (format/lint/type/test) | FAIL | The PowerShell production files in the diff did not have format/lint/test/coverage run against this branch; coverage artifact is stale and 0%. |

## Unit Test Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Coverage repo-wide >= 80% per changed language | FAIL | PowerShell repo-wide line coverage is 0.00% in the only available artifact. |
| New files >= 90% coverage | FAIL | 17 added `.ps1` files; 5 show 0% covered, 12 have no coverage data. |
| No external dependencies / temp files in tests | UNVERIFIED | No new tests in the diff to evaluate; the `.gitignore` repro is a documented command-based exception, not a unit test. |

## Appendix A — Assumptions and Limitations

- The MCP template-resolution tool and several shared skills referenced by the workflow are
  not present in this branch; artifacts follow the documented required shapes instead.
- PowerShell format/lint were not executed in this review environment; they are reported
  UNVERIFIED rather than asserted PASS.
- The coverage verdict relies on the pre-existing `artifacts/pester/powershell-coverage.xml`
  per the evidence-verification model (no regeneration).

## Appendix B — Command Reference

- Scope: `git diff --name-status b7bd81626a512c70c264a8badad5fa5691ca1c16..ca531c67b1a3605562894a5fe49d7cd38b382819`
- `.gitignore` diff: `git diff <merge-base>..<head> -- .gitignore`
- Live repro (allowed): `git check-ignore .claude/skills .claude/agents .claude/hooks .claude/rules .claude/settings.json` (prints nothing, exit 1)
- Live repro (invariant): `git check-ignore .claude/settings.local.json .claude/agent-memory/orchestrator/MEMORY.md` (prints both, exit 0)
- PowerShell coverage repo-wide total: report-level `<counter type="LINE" missed="284" covered="0" />` in `artifacts/pester/powershell-coverage.xml`
- Non-canonical evidence scan: `git diff --name-only <merge-base>..<head> | grep -E '^artifacts/(baselines|qa|coverage|evidence)/'` (no matches)
