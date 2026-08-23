# Discharged Issue-Creation Tasks — Remediation Cycle 1

Timestamp: 2026-08-23T19-08

Orchestrator Decision 4 of `remediation-inputs.2026-08-23T20-57.md` verified against GitHub on
2026-08-23T20-57 that every residual this feature would otherwise have filed already has an open
issue. The original plan's two `gh issue create` tasks (P6-T1 and P6-T20) are therefore recorded as
already-satisfied with the issue number cited, and are not re-executed. No `gh issue create` is
executed anywhere in this cycle.

| Residual | Issue | State |
| --- | --- | --- |
| Load-induced 60,000 ms `PumpTimeoutMs` expiry cascade under machine load — the genuine defect behind the #511 report | #592 | OPEN |
| Three pre-existing `UtilitiesCS.Test` flakes blocking any suite-wide zero gate | #594 | OPEN |
| Repository-wide analyzer version skew (original plan task P6-T20) | #597 | OPEN |

Issues #511 and #571 are both CLOSED as NOT_PLANNED (2026-08-23T19:07), superseded by #592, with the
premise-correction comments already posted to both. This cycle makes no repair claim for either
superseded issue, files no duplicate, and creates no GitHub issue of any kind.

Task mapping:

- Original plan `[P6-T1]` — file the follow-up issue for #511's visible-window half. Discharged by
  the pre-existing issue #592. Recorded in
  `docs/features/active/winformspumphost-suite-determinism-511/plan.2026-08-21T18-10.md` by
  remediation task P2-T5.
- Original plan `[P6-T20]` — file the follow-up issue for the repository-wide analyzer version skew.
  Discharged by the pre-existing issue #597. Recorded in the same plan file by remediation task
  P2-T5.

Two further residuals recorded in Decision 4 remain unfiled and are promoted through the MCP
promotion lifecycle by the orchestrator, outside this plan's execution scope: the orchestrator
checkpoint `blocked_reason` enum's inability to express a substantive halt, and repository-wide
host-identifier sanitization of the files outside this feature.
