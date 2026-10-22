# P0-T3 Orchestration checkpoint read (read-only)

Timestamp: 2026-09-29T08-52
Command: pwsh -NoProfile -Command '$p = "artifacts/orchestration/orchestrator-state.json"; if (-not (Test-Path $p)) { "CHECKPOINT=ABSENT"; exit 0 }; $j = Get-Content -LiteralPath $p -Raw | ConvertFrom-Json; "CHECKPOINT=PRESENT"; "KEYS=" + (($j.PSObject.Properties.Name | Sort-Object) -join ","); "LIFECYCLE-READY=" + [bool]$j.lifecycle_ready; $in = ([string]$j."issue-num").Trim(); "ISSUE-NUM=" + [bool]$in; $ff = ([string]$j."feature-folder").Trim(); "FEATURE-ACTIVE=" + $ff.StartsWith("docs/features/active/"); "ROUTE=" + [bool]($j.route_id -or $j.path_selected)'
EXIT_CODE: 0
Output Summary:
- CHECKPOINT=PRESENT
- LIFECYCLE-READY=True
- ISSUE-NUM=True
- FEATURE-ACTIVE=True
- ROUTE=True
- No stop condition: the pre-implementation gate is seeded and the checkpoint is ready. The file was read only; no value is recorded and the file was not written.

KEYS (names only; values never recorded):

```text
blocked_reason,branch,change_budget_estimate,cohort_index,completed_steps,complexity_assessments,delegation_receipts,feature-folder,issue_num,issue-num,last_updated,lifecycle_ready,lifecycle_ready_evidence,long-name,model_budget,model_routing_preflight,model_routing_receipts,next_step,notes,objective,parallel_mode,parallel_slug,path_selected,plan-path,promotion-type,relativeFile,route_id,short-name,step10_status,step5_status,step6_status,step7_status,step8_status,step9_status,work-mode
```
