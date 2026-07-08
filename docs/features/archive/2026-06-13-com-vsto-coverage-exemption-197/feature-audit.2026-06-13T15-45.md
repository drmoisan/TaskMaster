# Feature Audit: COM/VSTO/WinForms Coverage Exemption (#197)

**Audit Date:** 2026-06-13
**Feature Folder:** `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197`
**Base Branch:** `origin/main` (merge-base `1b3f5350`)
**Head Branch:** `refactor/com-vsto-coverage-exemption-197` (`a564add0`)
**Work Mode:** `full-feature`
**Audit Type:** Initial acceptance review

---

## Scope and Baseline

- **Base branch:** `origin/main` (commit `1b3f5350065b27c538c01542eb1400f8cca20d9d`)
- **Head branch/commit:** `refactor/com-vsto-coverage-exemption-197` (commit `a564add0d274c860b5f89ce6d3386fefb94527e8`)
- **Merge base:** `1b3f5350065b27c538c01542eb1400f8cca20d9d`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/**`
  - Additional evidence: branch diff inspection (`git diff 1b3f5350..HEAD`)
- **Feature folder used:** `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197`
- **Requirements source:** `spec.md` (`## Acceptance Criteria`, AC1–AC7). `user-story.md` is absent.
- **Work mode resolution note:** `issue.md` records `- Work Mode: full-feature`. Full-feature resolves to `spec.md` and `user-story.md`. `user-story.md` does not exist in the feature folder; `spec.md` is therefore the only available authoritative AC source. This absence is recorded as a documentation gap, not an acceptance blocker, because `spec.md` carries a complete, maintainer-ratified `## Acceptance Criteria` section (AC1–AC7).
- **Scope note:** PR context artifacts were present and current for head `a564add0`; no regeneration was required. Audit scope is the full branch diff vs base, not any plan/phase subset.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `spec.md` — primary (and, given the absent `user-story.md`, the only) authoritative checkbox source.
- `user-story.md` — expected secondary source for `full-feature`, but absent; not available.

### Acceptance criteria

(Transcribed verbatim from `spec.md` `## Acceptance Criteria`.)

1. (AC1) `coverage.config` excludes the `TaskVisualization` module from instrumentation via a `ModulePaths/Exclude` `ModulePath` entry, and `TaskMaster.runsettings` contains the matching exclude.
2. (AC2) `[ExcludeFromCodeCoverage]` (class- or method-level per the design memo §2 tables) is applied to all enumerated COM/VSTO/WinForms-bound classes/members in QuickFiler, TaskMaster, ToDoModel, and Tags, and to none of the enumerated testable seams.
3. (AC3) Post-exemption coverage re-measurement confirms the `TaskVisualization` package and all annotated classes are removed from the denominator, and the enumerated testable seams (`ToDoLoader`, `IDList.GetNextToDoID`, `KbdActions<>`, `TagController` pure-logic methods, settings/path helpers, etc.) remain in the denominator.
4. (AC4) The recorded post-exemption rate is consistent with the design memo §3 estimate (~75.2%, range 73.2%–77.6%), and the figures are written to the feature evidence folder.
5. (AC5) `CLAUDE.md` (UT2 coverage section) and `.claude/rules/general-unit-test.md` (Coverage Requirements section) record the COM/VSTO exemption policy, rationale, and the testable-denominator definition per the design memo §4.
6. (AC6) The full C# toolchain passes in a single final pass: csharpier (no diff), msbuild with analyzers + code style, msbuild with nullable + warnings-as-errors, and the MSTest suite with coverage.
7. (AC7) No production behavior change: no method bodies, signatures, or public APIs are modified; only `[ExcludeFromCodeCoverage]` attributes, required `using` directives, config excludes, and policy docs change.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | TaskVisualization excluded in coverage.config + TaskMaster.runsettings | PASS | Both files add `<ModulePath>.*TaskVisualization.*</ModulePath>` in `ModulePaths/Exclude`; valid XML, no other entries changed. `coverage-postexemption-checks.md` confirms 1 match each and TaskVisualization absent from the post-change denominator. | `git diff 1b3f5350..HEAD -- coverage.config TaskMaster.runsettings` | Diff inspected directly by reviewer. |
| 2 | `[ExcludeFromCodeCoverage]` applied to enumerated COM/VSTO/WinForms targets and to no testable seam | PASS | 25 class-level attributes across QuickFiler/TaskMaster/ToDoModel/Tags + 4 method-level on `IDList` (2 Outlook ctors, 2 `RefreshIDList`). `GetNextToDoID` and `Tags/TagController.cs` confirmed unannotated. Boundary verified exact vs memo §2. | `git diff 1b3f5350..HEAD -- '*.cs'`; `grep -n ExcludeFromCodeCoverage 'ToDoModel/Data Model/ID/IDList.cs' 'Tags/TagController.cs'` | `exemption-boundary-verification.md`; independently re-verified by reviewer. |
| 3 | Post-exemption re-measurement: exempt classes removed, testable seams retained | PASS | TaskVisualization package absent from post-change deduped Cobertura; all 28 enumerated class targets absent from denominator; enumerated testable seams (`ToDoLoader`, `IDList.GetNextToDoID`, `KbdActions<>`, `TagController`, settings/path helpers) present with ≥1 class match each. | inspection of `coverage-firstparty.postexemption.cobertura.xml` | `exemption-boundary-verification.md` §(a)/(b); `coverage-postexemption-checks.md`. |
| 4 | Post-exemption rate consistent with memo §3 (~75.2%, range 73.2%–77.6%) and figures written to evidence | FAIL | Measured rate is 71.73% (37,010/51,594), 1.47 pp below the §3 lower bound (73.2%). Figures ARE written to the evidence folder (`coverage-delta.md`). The numeric-consistency clause is not met. | inspection of `coverage-delta.md`, `final-mstest-coverage.md` | Estimate-range deviation, not an implementation defect — see Notes below and §Summary. Scope/boundary are correct (AC2/AC3 PASS). Non-blocking. |
| 5 | CLAUDE.md + general-unit-test.md record exemption policy/rationale/denominator | PASS | `CLAUDE.md` UT2 section adds the testable-denominator definition, exclusion categories (a/b/c), mechanism, authority note, and not-exempt seam list. `.claude/rules/general-unit-test.md` Coverage Requirements adds the COM-host-bound exemption with maintainer-ratification note. | `git diff 1b3f5350..HEAD -- CLAUDE.md '.claude/rules/general-unit-test.md'` | Diff inspected directly. |
| 6 | Full C# toolchain green in a single final pass | PASS | csharpier check EXIT_CODE 0 (no diff); analyzer build EXIT_CODE 0; nullable+WAE build EXIT_CODE 0; MSTest 4066/4068 (2 pre-existing flaky, identical set). | `final-csharpier.md`, `final-analyzer.md`, `final-nullable.md`, `final-mstest-coverage.md` | All four gates pass; behavior parity confirmed. |
| 7 | No production behavior change (attributes/using/config/docs only) | PASS | Full diff inspection of all 29 `.cs` files: only `using System.Diagnostics.CodeAnalysis;` + `[ExcludeFromCodeCoverage]` additions; no method bodies, signatures, or visibility changed. Behavior parity confirmed by identical pre/post failing test set. | `git diff 1b3f5350..HEAD -- '*.cs'`; `test-result-parity.md` | `[ExcludeFromCodeCoverage]` is a non-behavioral diagnostic attribute. |

**AC4 disposition reasoning (explicit):** AC4 has two clauses — (a) the figures are written to the evidence folder, and (b) the recorded rate is consistent with the §3 estimate range. Clause (a) is satisfied. Clause (b) is not: 71.73% is below the 73.2% lower bound. The criterion as written is therefore not fully met and is marked FAIL and left unchecked. However, this is assessed as a **non-blocking estimate deviation**, not an implementation defect, for these reasons: (1) the exemption **scope and boundary** are verified exact against the design memo §2 (AC2 and AC3 PASS) — no testable seam was exempted and no enumerated target was missed; (2) the §3 figures are explicitly labeled "estimate" and "range" in the spec, and the deviation is fully explained in `coverage-delta.md` (more incidentally-covered lines left the denominator than the midpoint estimate assumed, lowering the numerator more than projected); (3) the maintainer-ratified spec §Risks already states the post-exemption rate is expected to be below 80% and that the roadmap increment tests (explicitly out of scope for #197) close the gap. The shortfall does not change behavior parity and does not indicate incorrect exemption work. It is recorded for maintainer awareness; no code remediation is warranted for #197.

---

## Summary

**Overall Feature Readiness:** PASS

The feature delivers a correct, non-behavioral, toolchain-green coverage exemption. Six of seven acceptance criteria PASS with directly inspected evidence. The one FAIL (AC4) is a numeric estimate-range deviation, not an implementation defect: the exemption boundary is verified exact, the figures are recorded, and the sub-range rate is a maintainer-ratified expected outcome whose remediation (roadmap increment tests) is explicitly out of scope for #197. There are no blocking findings.

**Criteria summary:**
- **PASS:** 6 criteria (AC1, AC2, AC3, AC5, AC6, AC7)
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 1 criterion (AC4 — non-blocking estimate deviation)

**Top gaps preventing PASS:**

1. AC4 numeric consistency: measured 71.73% is 1.47 pp below the §3 estimate lower bound (73.2%). Non-blocking; scope/boundary correct; figures recorded.

**Recommended follow-up verification steps:**

1. Maintainer to acknowledge the AC4 estimate deviation (71.73% actual vs ~75.2% estimated) and confirm the exemption scope as final.
2. Track the out-of-scope roadmap increment tests (memo Phases 4–8) as the path to the 80% testable-denominator floor; note the starting point is 71.73%, not the estimated ~75.2%.
3. Author `user-story.md` for full-feature provenance completeness (documentation gap; not an acceptance blocker).

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** are checked off in the authoritative source file (`spec.md`) where represented as markdown checkboxes and not already checked.
- Criteria evaluated as **PARTIAL**, **FAIL**, or **UNVERIFIED** remain unchecked.

In `spec.md`, AC1, AC2, AC3, AC5, AC6, AC7 were already checked `[x]` by the executor and the evidence confirms PASS; they remain checked. AC4 remains `[ ]` (FAIL), consistent with this evaluation. No checkbox state change was required by this audit.

### AC Status Summary

- Source: `spec.md` (`## Acceptance Criteria`)
- Total AC items: 7
- Checked off (delivered): 6
- Remaining (unchecked): 1
- Items remaining: AC4 — "The recorded post-exemption rate is consistent with the design memo §3 estimate (~75.2%, range 73.2%–77.6%), and the figures are written to the feature evidence folder." (FAIL: measured 71.73%, below the stated range; non-blocking estimate deviation, figures written.)

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 7 | 6 | 1 | Checkbox-backed; authoritative AC source. AC4 left unchecked (FAIL, non-blocking). |
| `user-story.md` | 0 | 0 | 0 | Absent. Expected secondary source for full-feature; not present. Documentation gap only. |
