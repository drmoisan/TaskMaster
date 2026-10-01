---
name: 945-review-residuals
description: '#945 (SortEmail attachment test created a directory) minor-audit review 2026-09-30T13-00 PASS 8/8 AC, 0 blocking, 6 non-blocking; delegate-seam fix; Read/Grep-only review; reflog epochs as clock'
metadata:
  type: project
---

Minor-audit review (parallel cohort bugs-2026-09-28, worktree `agent-ae0b12a78ae0b0726`, head `cb502eeec`): PASS, 8/8 AC,
0 blocking. Same no-Bash mechanics as [[944-review-residuals]] (branch ref file -> HEAD; worktree `logs/HEAD` reflog epochs
as the clock: commit 1790786706 = 12:44 local, so executor labels were local-time consistent).

**Reusable points:**
- Both `TrySaveAttachmentAsync` overloads are method-level `[ExcludeFromCodeCoverage]`, so changed lines are outside the
  Cobertura denominator; the "new code" figure used was per-file `SortEmail.cs` 24/25 (96.0%), unit disclosed in the audit.
- Package-level drift (-6 covered lines, -3 branches, equal denominators, `SortEmail.cs` per-file delta 0) treated as collector
  variance inside the orchestrator-ratified 0.10-point band; CI is the repo-wide gate.
- `SortEmail.cs` pre-existing 500-line breach (1429 -> 1454): recorded as non-blocking, not introduced.
- Raw Cobertura not committed by policy; coverage claims are evidence-attested from the committed JaCoCo projections.

**Follow-ups owed (not filed):** split `SortEmail.cs`; extract the `ShowDialog` read-only prompt behind a seam and drop the
attribute; audit other `SortEmail` helpers for direct `Directory`/`File` use.
