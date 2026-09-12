---
name: project-497-f16-capstone-plan-seams
description: Seams and corrected facts for the #497 F16 capstone plan (epic #136) — stale research claims, manifest placeholder count, and the closure-after-QA ordering call
metadata:
  type: project
---

Planning seams for `2026-08-08-quickfiler-per-file-coverage-capstone-497` (epic #136 child F16,
capstone/wave 2). Plan at `<FEATURE>/plan.2026-08-08T00-34.md`, 15 phases / 162 tasks.

**Why:** F16 verifies a finished state across fourteen siblings. Several load-bearing facts in its
two research artifacts were already stale at planning time, and encoding them literally would have
cost a revision pass (see [[research-claims-as-acceptance-clauses]]).

**How to apply:** when re-planning or remediating F16, re-derive these rather than trusting the
research artifacts or an earlier plan revision.

- **`.claude/rules/python.md` EXISTS.** Research asserted it does not, and used that absence as the
  reason capstone tooling must be PowerShell. Do not restate the claim. The sound rationale is that
  all repo tooling lives at `scripts/vscode/` in PowerShell, F1's harness is PowerShell, and
  `.claude/rules/powershell.md` supplies the gating toolchain.
- **Three manifest placeholders remain, not five.** `epic.md` front matter now carries `1012` (F12),
  `1015` (F15), `1016` (F16) against real 495/496/497; F9/F10/F13/F14 were back-filled to
  452/453/455/456 after research ran. F16's own `depends_on` still names `1012` and `1015`.
- **Thirteen of fourteen sibling folders resolve at planning time**; only F15
  (`quickfiler-form-viewers-bayesian-coverage`, 496) is absent. Manifest `feature_folder` values are
  stale for F2, F12, and F15, so resolve by issue-number suffix first and slug second.
- **The AC8 closure phase must run AFTER the final QA loop**, contrary to the delegation's phase
  enumeration. AC8 cites the AC7 toolchain artifacts and the AC6 after-figure comparison, both
  produced by that loop; closure-first forces a forward reference. Safe because only Markdown under
  `evidence/` is written afterward and `.csharpierignore` excludes `**/evidence/**`.
- **PowerShell branch coverage is unmeasurable here.** The scoped Invoke-Pester JaCoCo output carries
  a `LINE` counter and no `BRANCH` counter, so `.claude/rules/powershell.md`'s 75% branch floor
  cannot be reported for capstone-owned scripts. Record the gap; do not plan to close it.
- **The one genuinely capstone-owned computation is the repository-wide recomputation.** F1's harness
  selects `<package name="QuickFiler">` by name and cannot emit a repo-wide figure; F1 already
  implements `UNLEDGERED` / `NO DATA` row states and a csproj-completeness Pester assertion, so
  duplicating those is waste.
