---
name: coverage-threshold-reconciliation-494
description: "Issue #494 research: 85/75 is un-reconciled foreign-import leakage reintroduced after #178 rejected it; the only numeric coverage gate is evadable by withholding its input; repo-wide coverage has a plus/minus 15-point run-to-run spread that #441/#457 do not fix"
metadata:
  type: project
---

Research for issue #494 (coverage threshold + exclusion policy reconciliation, epic
`build-ci-coverage-gate-fidelity` wave 2) completed 2026-08-10. Artifact:
`docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/research/2026-08-10T15-40-coverage-threshold-policy-reconciliation-research.md`.

**Why these matter:** each item below is invisible from a single file read and changed the framing
of the decision. Re-verify file:line before acting — features #441/#457/#512 edit adjacent surfaces.

**How to apply:** consult before any work that reads, sets, or audits a coverage threshold, or that
touches `.claude/rules/general-unit-test.md`, `quality-tiers.md`, or `CLAUDE.md` § UT2.

1. **The 85/75 cluster is un-reconciled foreign-import leakage.** `.claude/rules/general-unit-test.md`
   § "Coverage Exclusion Policy" lists only TypeScript/Node paths (`dist/**`, `node_modules/**`,
   `jest.config.cjs`, "any path under `src/`") — this repo has no `src/`, no `package.json`, and
   zero `.ts` files. Sibling `.claude/rules/architecture-boundaries.md` bans
   `Microsoft.Office.Interop.Outlook` in a VSTO Outlook add-in.
   `.claude/rules/orchestrator-state.md` names the foreign origin outright
   (`drmoisan.github.io/mix-calculator/`). Agent memory for #178 records 85/75, the T1-T4 tier
   system, `architecture-boundaries.md`, and `benchmark-baselines.md` as **deliberately excluded**
   under "keep current policy, adapt mechanism" — yet all four are present today. The commit that
   reintroduced them is unidentified (needs `git log -L`).

2. **`docs/ci.research.md` does not exist**, so `.claude/rules/quality-tiers.md:9` cites a missing
   source of truth, on top of the already-known missing `quality-tiers.yml` and missing
   `tier-classification` CI stage. Its tier examples name projects absent from `TaskMaster.sln`
   (`TaskMaster.Domain`, `TaskMaster.Application`, Graph adapter, Office.js).

3. **The only numeric coverage gate is evadable by withholding its input.**
   `.claude/hooks/validate-feature-review-coverage.ps1` reads **JaCoCo** from
   `artifacts/csharp/coverage.xml`; `Get-LanguageRepoCoverage` returns `$null` when the file is
   absent and both numeric branches are skipped. Committed memory at
   `.claude/agent-memory/feature-review/coverage-hook-forces-fail-below-floor-despite-exemption.md`
   records "deliberately not producing coverage.xml is a valid tactic". Also: **no committed script
   or workflow produces `artifacts/csharp/coverage.xml`** — the only recorded producer is an inline
   scratchpad Python converter that was never committed. The C# toolchain emits **Cobertura** to
   `coverage/coverage.cobertura.xml` instead.

4. **The hook's branch check blocks unconditionally.** In `Test-LanguageCoverageRow`, the line check
   only requires a FAIL token in the policy-audit text; the branch check returns `Ok = $false`
   whenever branch < 75 without inspecting the audit at all, despite its own message saying the
   audit "must record FAIL".

5. **Repo-wide coverage has a plus/minus 15-point run-to-run spread that neither #441 nor #457
   fixes.** Two runs of `Invoke-MSTestWithCoverage.ps1` 26 hours apart on essentially the same tree:
   70.19% / 79,957 lines-valid vs 85.65% / 110,849. #424's own evidence attributes it to
   non-deterministic assembly instrumentation. A repo-wide numeric floor is not reproducible today.

6. **Per-`<package>` figures are more trustworthy than the root figures.**
   `ConvertTo-KoverageCoberturaXml` rewrites only the root `<coverage>` attributes and
   `Merge-CoberturaClassesByFilename` only class-level rates; nothing recomputes `<package>`
   attributes, so they are `dotnet-coverage`'s own output and escape the #441 descendant-axis
   double count. Latest committed per-assembly set (2026-08-08): VBFunctions 100, TaskTree 95.5,
   Tags 92.7, TaskVisualization 89.8, UtilitiesCS 89.5, QuickFiler 80.8, TaskMaster 71.0,
   ToDoModel 57.3, SVGControl 47.3. **Three of nine fail an 80% bar; five of nine fail 85%.**

7. **`.agents/` is a stale snapshot, not a mirror.** It is the canonical Codex runtime surface per
   its own README, and three of its files state the opposite camp from their `.claude/`
   counterparts: `.agents/skills/powershell/SKILL.md:64-65` (80/90 vs `.claude/rules/powershell.md`
   85/75), `.agents/skills/powershell-qa-gate/SKILL.md:45`, and
   `.agents/skills/feature-review-workflow/SKILL.md:101-103` (90/80 vs `.claude/` 85/75).

8. **`.claude/agents/feature-review.md` contradicts itself internally** — 85/75 at lines 112-114,
   90/80 at lines 127-128, in the same numbered procedure.

9. **`tests/scripts/vscode/` already exists** with four Pester files; `tests/scripts/powershell/`
   does not, and the general-unit-test rule is a *mirroring* rule, not a fixed literal path. The
   proven no-temp-file test pattern is an inline `@'…'@` here-string XML fixture fed to a
   dot-sourced pure function (`Invoke-MSTestWithCoverage.Helpers.Tests.ps1`).

10. **Two decisions here are maintainer decisions, not agent decisions:** the governing numbers
    (changing them reverses the recorded #178 decision) and whether the COM/VSTO/WinForms
    testable-denominator exemption survives (`CLAUDE.md:303` reserves ratification to the
    maintainer, and [[qfc-item-controller-227-r2-denial]] shows the reservation is live).

Related: [[qfc-item-controller-227-r2-denial]], [[feedback-exemption-audit-check-proven-techniques]].
