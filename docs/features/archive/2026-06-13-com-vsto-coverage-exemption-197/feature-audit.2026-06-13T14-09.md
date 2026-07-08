# Feature Audit: com-vsto-coverage-exemption (Issue #197) — Re-audit R4

**Audit Date:** 2026-06-13
**Feature Folder:** `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/`
**Base Branch:** `origin/main` (merge-base `1b3f5350`)
**Head Branch:** `refactor/com-vsto-coverage-exemption-197` @ `05c5828e`
**Work Mode:** `full-feature`
**Audit Type:** Post-remediation acceptance verification (R4, following maintainer-directed scope change)

---

## Scope and Baseline

- **Base branch:** `origin/main` (commit `1ea289f7`)
- **Head branch/commit:** `refactor/com-vsto-coverage-exemption-197` (commit `05c5828e`)
- **Merge base:** `1b3f5350`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/**`
  - Additional evidence: `artifacts/csharp/coverage-firstparty.r2-classlevel.cobertura.xml`, `artifacts/csharp/coverage-firstparty.phase8.cobertura.xml`; independent `git diff 1b3f5350..HEAD` inspection
- **Feature folder used:** `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/`
- **Requirements source:** `spec.md` (`## Acceptance Criteria`). `user-story.md` is not present in this feature folder; `spec.md` is the authoritative AC source for the full-feature mode here.
- **Work mode resolution note:** `issue.md` carries `- Work Mode: full-feature`. Per the work-mode contract, full-feature AC sources are `spec.md` and `user-story.md`; only `spec.md` exists, so it is the sole authoritative source.
- **Scope note:** The audit scope is the full branch diff `1b3f5350..HEAD`, comprising 43 attribute-only C# files across 5 assemblies plus policy/doc/evidence files. The PR-context summary's "Core logic changes: 0 files" classification was not used as a scope source; scope was derived from `git diff --name-status`. PR context was refreshed and current for this cycle.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/spec.md` — only authoritative source (`## Acceptance Criteria`)
- `user-story.md` — not present; no check-off target

### Acceptance criteria

1. `coverage.config` and `TaskMaster.runsettings` no longer exclude the `TaskVisualization` module via a `ModulePaths/Exclude` `ModulePath` entry (revision 1.1 reversed the assembly-level exclude in favor of class-level `[ExcludeFromCodeCoverage]`); TaskVisualization is present in the first-party denominator.
2. `[ExcludeFromCodeCoverage]` (class- or method-level per the design memo §2 tables) is applied to all enumerated COM/VSTO/WinForms-bound classes/members in QuickFiler, TaskMaster, ToDoModel, Tags, and (revision 1.1) TaskVisualization, and to none of the enumerated testable seams.
3. Post-exemption coverage re-measurement confirms the annotated classes are removed from the denominator (and, revision 1.1, that the `TaskVisualization` package is back in the denominator carrying only its preserved testable seams), and the enumerated testable seams (`ToDoLoader`, `IDList.GetNextToDoID`, `KbdActions<>`, `TagController` pure-logic methods, settings/path helpers, `FlagChangeItem`, `FlagChangeTrainingQueue` testable paths, etc.) remain in the denominator.
4. The recorded post-exemption rate is consistent with the design memo §3 estimate (~75.2%, range 73.2%–77.6%), and the figures are written to the feature evidence folder.
5. `CLAUDE.md` (UT2 coverage section) and `.claude/rules/general-unit-test.md` (Coverage Requirements section) record the COM/VSTO exemption policy, rationale, and the testable-denominator definition per the design memo §4.
6. The full C# toolchain passes in a single final pass: csharpier (no diff), msbuild with analyzers + code style, msbuild with nullable + warnings-as-errors, and the MSTest suite with coverage.
7. No production behavior change: no method bodies, signatures, or public APIs are modified; only `[ExcludeFromCodeCoverage]` attributes, required `using` directives, config excludes, and policy docs change.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | TaskVisualization no longer assembly-excluded; present in denominator | PASS | `coverage.config`/`TaskMaster.runsettings` net-zero diff vs base; 0 `TaskVisualization` matches; `TaskVisualization.*` classes present in R2 Cobertura | `git diff 1b3f5350..HEAD -- coverage.config TaskMaster.runsettings`; `grep TaskVisualization coverage.config TaskMaster.runsettings` | Assembly exclude fully reversed in revision 1.1 |
| 2 | `[ExcludeFromCodeCoverage]` on all enumerated COM/WinForms classes/members; none on testable seams | PASS | 9 TaskVisualization classes class-level exempt; FlagChangeGroup 4 members method-level; IDList Outlook members method-level; FlagChangeItem/FlagChangeTrainingQueue/TryEnqueue/GetNextToDoID unannotated | `grep -rn ExcludeFromCodeCoverage TaskVisualization/`; `git diff 1b3f5350..HEAD -- '*.cs'` | Boundary independently verified; other 4 assemblies unchanged this cycle |
| 3 | Re-measurement confirms annotated classes removed, TaskVisualization back with preserved seams | PASS | phase8 Cobertura had 13 TaskVisualization classes; R2 Cobertura has only FlagChangeGroup, FlagChangeItem, FlagChangeTrainingQueue, TipsController | `grep -oE 'name="TaskVisualization\.[A-Za-z]*' artifacts/csharp/coverage-firstparty.{phase8,r2-classlevel}.cobertura.xml` | TipsController is a tested UtilitiesCS class attributed to TV in the deduped merge; genuinely measured |
| 4 | Recorded rate consistent with §3 estimate (~75.2%, 73.2%–77.6%) | PARTIAL | Measured 71.65%, 1.55 pp below the §3 lower bound; figures recorded in `coverage-delta-r2.md` | n/a (measured) | Figures ARE written to evidence; the rate is below the stated range. Documented deviation; maintainer-acknowledged open item. See AC4 disposition below. |
| 5 | `CLAUDE.md` and `general-unit-test.md` record exemption policy, rationale, denominator definition | PASS | Both files contain the testable-denominator definition, exclusion categories (a)/(b)/(c), mechanisms, maintainer-authority note, and explicit not-exempt seams | `git diff 1b3f5350..HEAD -- CLAUDE.md .claude/rules/general-unit-test.md` | Matches design memo §4 |
| 6 | Full C# toolchain passes in a single final pass | PASS | csharpier 1040 files/0 unformatted (re-run); analyzer EXIT_CODE 0; nullable EXIT_CODE 0; MSTest 4068/4068 | `dotnet tool run csharpier check .`; `final-r2-analyzer.md`/`final-r2-nullable.md`/`final-r2-mstest-coverage.md` | Clean single pass at 2026-06-13T13-46 |
| 7 | No production behavior change (attributes/using/config/docs only) | PASS | All `.cs` additions are attribute/using/comment; zero removed lines; zero executable additions; test parity 4068 pre/post | `git diff 1b3f5350..HEAD -- '*.cs'` filtered; `test-result-parity-r2.md` | Behavior invariant holds |

### AC4 disposition (blocking vs acceptable authority-scoped deviation)

AC4 is assessed as a **non-blocking, acceptable authority-scoped deviation**, marked PARTIAL rather than FAIL, for the following reasons:

- The §3 figures are explicitly labeled estimates ("~75.2%, range 73.2%–77.6%"). The criterion's second clause — "the figures are written to the feature evidence folder" — is satisfied (`coverage-delta-r2.md`, `coverage-delta.md`). Only the "consistent with the estimate" clause is unmet, by 1.55 pp.
- The exemption SCOPE is verified correct (AC2/AC3 PASS); the deviation cause is a measurement refinement, not a scope or policy error: more covered lines correctly left the denominator than the midpoint estimate assumed, and the class-level treatment correctly re-includes lightly-covered TaskVisualization seams (13/71 covered) that the §3 assembly-removal had excluded entirely.
- The spec, issue, scope-change directive, and `coverage-delta-r2.md` all consistently designate AC4 as "a separate open maintainer-acknowledgement item." The criterion is intentionally left unchecked in `spec.md` pending maintainer sign-off; this is the documented disposition, not an unhandled failure.
- Reaching the 80% floor is explicitly out of scope (spec §Non-Goals: roadmap increment tests). The feature delivers the exemption mechanism and raises the testable-denominator rate by +12.62 pp (59.03% -> 71.65%); it does not claim to reach the estimate or the floor.

Conclusion: AC4 does not block PR readiness. It requires the maintainer's separate acknowledgement of the measured-rate-vs-estimate gap, which the repository process already routes through the documented open-item channel.

---

## Summary

**Overall Feature Readiness:** PASS (with AC4 as a maintainer-acknowledged open item, non-blocking)

**Criteria summary:**
- **PASS:** 6 criteria (1, 2, 3, 5, 6, 7)
- **PARTIAL:** 1 criterion (4 — measured rate below the design estimate range; documented, maintainer-acknowledged)
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing full PASS:**

1. AC4: measured production-only rate (71.65%) is 1.55 pp below the design §3 lower bound (73.2%). Documented deviation; requires maintainer acknowledgement of the estimate gap, not code change.

**Recommended follow-up verification steps:**

1. Maintainer acknowledgement of the AC4 measured-rate deviation (the exemption scope is correct; the §3 figures were estimates).
2. Schedule the out-of-scope roadmap increment tests to raise the testable-denominator rate toward the 80% floor.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as PASS may be checked off in the authoritative source file if represented as checkboxes and not already checked.
- AC1, AC2, AC3, AC5, AC6, AC7 are already checked `[x]` in `spec.md` and remain correctly checked (re-verified PASS this cycle).
- AC4 is evaluated PARTIAL and remains unchecked `[ ]` in `spec.md`, consistent with its documented maintainer-acknowledgement status. No source-file change is made by this audit.

### AC Status Summary

- Source: `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/spec.md`
- Total AC items: 7
- Checked off (delivered): 6
- Remaining (unchecked): 1
- Items remaining: AC4 — "The recorded post-exemption rate is consistent with the design memo §3 estimate (~75.2%, range 73.2%–77.6%), and the figures are written to the feature evidence folder." (figures are written; the rate is 1.55 pp below the lower bound — maintainer-acknowledgement open item)

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 7 | 6 | 1 | Checkbox-backed; AC4 left unchecked by design (maintainer-acknowledgement item). No change made by this audit. |

No source-file checkbox change was made: the 6 PASS criteria were already checked in a prior cycle and re-verified; AC4 remains correctly unchecked.
