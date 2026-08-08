---
name: feedback-postformat-file-size-audit
description: 500-line file-size audits must run AFTER the final csharpier format pass — pre-format counts are advisory only; also sequence async-factory fault-path tests per the fire-and-forget vs awaited-tail asymmetry
metadata:
  type: feedback
---

Place the authoritative <=500-line file-size audit task in the final-QC phase, immediately AFTER the `csharpier format` step and before lint, with restart-from-format on violation. A Phase-N audit measured before the final format pass is invalid: csharpier reflows to its print width and routinely pushes hand-written ~480-line test files past 500.

**Why:** #230 preflight B3 (2026-08-07) — the plan's Phase 7 size audit + S-AC13 check-off were rejected because `csharpier format .` did not run until Phase 8. Same cycle, B2: a factory test acceptance ("assert returned controller state") was unachievable because `CreateAsync` → `InitializeAsync` ends with an AWAITED `InitializeWebViewAsync()` that always faults under a mocked seam, while `CreateSequentialAsync`'s tail is fire-and-forget `_ = InitializeWebViewAsync();` and returns normally — the awaited-vs-discarded tail asymmetry determines whether a member can be tested to completion or only to a controlled fault (partial per-member coverage by construction; carve the coverage gate to "> 0%").

**How to apply:** (1) Keep any earlier line-count checks labeled advisory; key the S-AC check-off to the post-format artifact. (2) Before writing "assert the returned value" acceptance for an async factory/orchestrator, read its terminal statement chain: awaited faulting tail → fault-path test mirroring the exception identity; discarded tail → normal-completion test. Related: [[enumerate-condition-outcomes-before-case-list]], [[named-coverage-exception-verify-member-body]].
