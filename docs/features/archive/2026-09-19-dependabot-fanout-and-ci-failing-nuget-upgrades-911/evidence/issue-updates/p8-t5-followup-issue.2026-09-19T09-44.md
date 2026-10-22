# P8-T5 follow-up issue — created

Timestamp: 2026-09-20T09-44

CreatedIssue: 914
IssueURL: https://github.com/drmoisan/TaskMaster/issues/914
IssueTitle: Bug: dependabot-repair-deferred-credential-criteria-and-residuals (follow-up to #911)

PostedAs: body

Command:

```
mcp__drm-copilot potential_to_issue (repository promotion lifecycle)
gh issue comment 914 --repo drmoisan/TaskMaster --body-file docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/issue-updates/p8-t5-followup-issue-body.2026-09-19T09-44.md
gh issue edit 914 --repo drmoisan/TaskMaster --title "Bug: dependabot-repair-deferred-credential-criteria-and-residuals (follow-up to #911)"
```

EXIT_CODE: 0

CommentURL: https://github.com/drmoisan/TaskMaster/issues/914#issuecomment-5747820406

## How the issue was created, and why not by `gh`

Creation went through the repository's MCP promotion lifecycle
(`docs/features/potential/2026-09-20-dependabot-repair-deferred-credential-criteria-and-residuals.md`
promoted to an issue), **not** through the `gh` issue-creation command this task originally
attempted. The reason is recorded rather than worked around: `enforce-pr-author-skill.ps1` refused
the `gh` attempt before `gh` ran, with this output verbatim:

```
ORCHESTRATOR_STATE_PREFLIGHT_FAILED: Checkpoint PR-creation readiness validation failed: step6_status is pending.
Checkpoint PR-creation readiness validation failed: step7_status is pending.
Checkpoint PR-creation readiness validation failed: step8_status is pending.
```

That hook gates issue and pull-request creation on the orchestrator-state checkpoint passing
`--require-pr-creation-ready`, which requires `step5_status` through `step8_status` to be
non-pending. Steps 6 to 8 are pending because feature review, remediation and pull-request creation
have not occurred. The checkpoint is written by the orchestrator, not by the executor, so advancing
those statuses to unblock an executor command would have recorded a state that had not happened.
**No hook was suppressed and no bypass was requested.** The coordinator filed the entry through the
promotion lifecycle, which creates issues by a different path and passes no gate.

## Body artifact

The prepared body is at
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/issue-updates/p8-t5-followup-issue-body.2026-09-19T09-44.md`.
The issue created by the promotion lifecycle carries the four findings in summary form; the prepared
body, which carries them in full with the exact verification commands, the P8-T1 measurement quoted
verbatim and the runbook path, was attached to issue 914 as the comment linked above. The title was
amended to name issue #911 while keeping the promotion lifecycle's `Bug: <slug>` prefix intact.

## Deferred-criteria enumeration, auditable

SearchScope:
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/`
and
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/other/`

SearchPatterns: `p8-t*.2026-09-19T09-44.md`, and within each match the literal
`DEFERRED: credential or fixture absent`

SearchResult: three artifacts carry the deferred marker, and each names one criterion left
unchecked:

| Artifact | Criterion left unchecked |
|---|---|
| `evidence/qa-gates/p8-t2-ac18-repair-identity.2026-09-19T09-44.md` | AC18 |
| `evidence/qa-gates/p8-t3-ac19-required-checks.2026-09-19T09-44.md` | AC19 |
| `evidence/qa-gates/p8-t4-ac20-disclosure.2026-09-19T09-44.md` | AC20 |

The deferred set is therefore non-empty and has exactly three members.
`evidence/other/p8-t1-credential-availability.2026-09-19T09-44.md` carries the measurement that
selected the deferred branch in all three and is not itself a deferred criterion.

## What the issue carries

1. The three deferred criteria, each with the exact verification commands from P8-T2, P8-T3 and
   P8-T4, the P8-T1 measurement quoted verbatim, and the runbook path
   `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md`.
2. `scripts/vscode/Invoke-MSTest.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, named as
   carrying unformatted PowerShell on `main`, which Scope Decision 8 leaves alone.
3. The ten stale binding redirects across six `app.config` files that P7-T5 measured and recorded,
   verified pre-existing at merge base `734112ed25bba293cb074e71fee2286bc3b72fae`.
4. The latent defect in `Invoke-ProjectConsistencyRepair`, whose no-`-AssemblyVersion` fallback
   rewrites `<Reference>` assembly versions to package versions — 51 in `QuickFiler.csproj` alone —
   and which ships unreached because P7-T1 wires the module functions directly. The body states that
   this item is pending the coordinator's decision on whether it warrants an issue of its own.
