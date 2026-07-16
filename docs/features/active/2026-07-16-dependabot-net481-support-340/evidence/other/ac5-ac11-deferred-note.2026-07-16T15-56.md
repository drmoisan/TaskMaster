# AC-5 / AC-11 Deferred Note

- Timestamp: 2026-07-16T15-56
- Issue: #340

## Confirmation

`spec.md` was read back and confirmed to still contain, unchanged:

- `- [ ] AC-5: Pre-decided fallback — if the post-merge check in AC-11 shows Dependabot scanning fewer directories than expected...`
- `- [ ] AC-11: After merge, a maintainer manually confirms via the repository's Insights → Dependency graph → Dependabot tab...`

Both lines are intentionally left unchecked by this plan.

## Reason

- **AC-5** is a pre-decided contingency triggered only by a future AC-11 result. It documents the fallback (the literal 16-entry `directories:` list in `spec.md` Appendix A) that would be adopted only if AC-11's post-merge check shows under-coverage. Since AC-11 has not yet been executed (it is a manual, post-merge check), AC-5's trigger condition has not occurred, and no follow-up commit is warranted at this time. AC-5 remains a documented fallback, not an active task in this plan.
- **AC-11** has been resolved by the orchestrator as `scope_change`: it is a deferred, manual, post-merge check (confirming via the repository's Insights → Dependency graph → Dependabot tab that at least one of the 16 directories is scanned) and is not a blocking Definition-of-Done item in this plan. The README documentation section added in Phase 5 (content point 4) records the runbook-note text pointing a future maintainer to this manual check.

Output Summary: AC-5 and AC-11 confirmed unchecked in spec.md by design; reasons recorded per plan Scope Note.
