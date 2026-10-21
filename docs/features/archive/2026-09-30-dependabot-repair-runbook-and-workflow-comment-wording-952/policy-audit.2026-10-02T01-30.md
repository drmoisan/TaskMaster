# Policy Audit - Issue #952 (dependabot-repair runbook and workflow comment wording)

- Branch: bug/dependabot-repair-runbook-and-workflow-comment-wording-952
- Base: origin/main (34c2ed88cbb009f2f231453db87bc64d45a9bd51); two-dot `git diff origin/main` forms used
- Branch head reviewed: 1639eda78a39b6afc04e095713a5b7ea6c4e4db6
- Work mode: minor-audit (AC source: `## Acceptance Criteria` in issue.md)
- Review timestamp: 2026-10-02T01-30

## Executive Summary

Overall verdict: PASS. Blocking findings: 0.

The branch changes two production-surface files: one line in the installation-token runbook (`App ID location` to `Client ID location`) and the header comment of `.github/workflows/dependabot-repair.yml` (rewrapped from four lines to five, wording unchanged). The workflow diff adds 4 and removes 3 lines, all comment lines. No C#, PowerShell, TypeScript, or Python file is in the branch diff, so no coverage artifact is required. The `modified-workflow-needs-green-run` rule applies to the workflow file; its evidence is a green CI run on the branch head, which does not exist yet and is recorded as PENDING (acceptance criterion 5), not as a defect, per the coordinator ruling.

## Rejected Scope Narrowing

None. The caller prompt did not narrow scope; the full branch diff against origin/main was audited. The caller statements that Pester counts are deferred to CI and that no coverage artifacts are required were checked against the diff (no language files changed) and are consistent with it; they are not narrowings.

## Evidence Location Compliance

The branch diff contains no path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` (checked with `git diff origin/main --name-only -- artifacts`, empty output). All feature evidence is under `docs/features/active/2026-09-30-dependabot-repair-runbook-and-workflow-comment-wording-952/evidence/` in the `baseline/`, `qa-gates/`, and `other/` kinds. Verdict: PASS. No EVIDENCE_LOCATION_OVERRIDE_REJECTED entries.

## 1. General Unit Test Policy Compliance

No test files were added or modified. Verdict: PASS (not triggered).

### Coverage Evidence Checklist

- C# baseline coverage artifact: `N/A - out of scope`
- C# post-change coverage artifact: `N/A - out of scope`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: zero changed files on the branch; no comparison applicable. Baseline: N/A; Post-change: N/A; Disposition: N/A (zero files).
- PowerShell: zero changed files on the branch; no comparison applicable. Baseline: N/A; Post-change: N/A; Disposition: N/A (zero files).

### 1.2.2 Coverage Artifact State

| Language | Changed files | Coverage artifact required | Verdict |
|---|---|---|---|
| C# | 0 | No | N/A (zero files) |
| PowerShell | 0 | No | N/A (zero files) |
| TypeScript | 0 | No | N/A (zero files) |
| Python | 0 | No | N/A (zero files) |

Changed file types on the branch (from `git diff origin/main --stat`): Markdown (runbook, plan, issue, evidence, promoted potential entry, agent-memory notes) and one YAML workflow. The agent-memory `.md` files under `.claude/agent-memory/` are memory notes, not policy documents under `.claude/rules/` or `.github/instructions/`.

## 2. General Code Change Policy Compliance

| Policy item | Verdict | Evidence |
|---|---|---|
| Minimal, targeted change | PASS | Runbook: 1 line changed. Workflow: 4 added and 3 removed comment lines. Footprint evidence p2-t7 reports OUTSIDE-COUNT=0. |
| Wording preserved in workflow comment | PASS | Diff shows identical words re-flowed; only the line breaks moved. |
| File size limit (500 lines) | PASS | Markdown documentation is exempt; the workflow file was not extended beyond 5 comment lines in total. |
| Comment width consistent with file (100 chars) | PASS | A search for lines over 100 characters in the workflow returns only non-comment lines 80, 106, 127, 132, and 151 (pre-existing PowerShell and script lines); comment lines 3-17 are all 100 characters or fewer. |
| No non-comment YAML change | PASS | Evidence p2-t4: ADDED=4 REMOVED=3 CHANGED=7 NONCOMMENT=0; reproduced here from the two-dot diff read directly. |
| Tonality policy | PASS | Review of the changed text shows neutral wording. |

## 3. Language-Specific Code Change Policy Compliance

No C#, PowerShell, TypeScript, or Python file changed. Verdict: PASS (not triggered).

Workflow-specific checks:

- actionlint 1.7.7 via `scripts/dev-tools/run-actionlint.ps1`: exit 0, zero output lines (evidence p2-t2). PASS.
- `modified-workflow-needs-green-run` (`.claude/skills/feature-review-workflow/SKILL.md`): the diff modifies `.github/workflows/dependabot-repair.yml`. The workflow is `workflow_run`-triggered, filtered to `dependabot/` head branches, and defines no `workflow_dispatch` trigger (evidence p2-t8, 0 `workflow_dispatch` lines). No run of that workflow can occur on this branch. The qualifying evidence is a green CI run (including its `actionlint` job) on the branch head, to be produced by the pull request's own CI run. State: PENDING (deferred to PR-time CI per coordinator ruling). This is not a Blocking finding at this stage; it must be confirmed green on the PR before merge.

## 4. Language-Specific Unit Test Policy Compliance

Not triggered; no test code changed. Verdict: PASS (not triggered). The Pester counts in the P0-T9 and P2-T5 evidence are deferred to CI under the user directive that prohibits raw `Invoke-Pester`; this is accepted and is not a defect.

## 5. Test Coverage Detail

No production code in a coverage language changed; no coverage figures apply.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 0 | N/A | N/A | N/A | N/A | N/A |
| PowerShell | 0 | N/A | N/A | N/A | N/A | N/A |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |

## 6. Test Execution Metrics

No test run applies to this change. The PoshQC `ok:true` local gate and the actionlint gate were recorded by the executor (evidence p2-t2, p2-t5, toolchain-pass.md). Pester counts: deferred to CI by directive; accepted.

## 7. Code Quality Checks

| Check | Command or method | Result |
|---|---|---|
| Runbook token search | Grep for `App ID location` in the runbook | 0 matches. PASS |
| Runbook line change | `git diff origin/main` on the runbook | One line, `App ID location` to `Client ID location`, matching steps 10 and 22. PASS |
| Workflow comment width | Grep for lines over 100 characters | Comment lines all within 100; five pre-existing code lines exceed it and are outside the AC. PASS |
| Workflow change scan | `git diff origin/main` on the workflow | Comment lines only. PASS |
| Suppression scan (added lines) | Diff read | No suppressions added. PASS |
| Confidentiality masking scan | Grep of the feature folder for account names and drive-letter user paths | 0 matches. PASS |
| Absolute host path scan | Same search | 0 matches in committed feature artifacts. PASS |

## Appendix A: Test Inventory

No tests were added or modified.

## Appendix B: Toolchain Commands Reference

Commands relied on (recorded by the executor, inspected here): `scripts/dev-tools/run-actionlint.ps1` (actionlint 1.7.7), PoshQC MCP `ok:true` gate, `git diff origin/main` forms. The reviewer ran `git diff origin/main` (stat and path-scoped), `git rev-parse`, and Grep searches only; no mutation commands.

## Template Provenance Deviation

The policy-audit template MCP was not invoked; the headings of the full template were authored by hand. Sections that do not apply are marked not triggered.

## Remediation Triggers

None. Blocking findings: 0. Pending item: acceptance criterion 5 (green CI run on the branch head), to be satisfied by the pull request CI run.
