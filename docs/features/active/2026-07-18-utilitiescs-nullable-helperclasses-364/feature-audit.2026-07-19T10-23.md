# Feature Acceptance Audit — Issue #364 (utilitiescs-nullable-helperclasses)

- Timestamp: 2026-07-19T10-23
- Reviewer: feature-reviewer
- Work Mode: full-feature (AC sources: `spec.md` `## Definition of Done` + `## Seeded Test Conditions`,
  and `user-story.md` `## Acceptance Criteria`, evaluated independently)
- Branch: `feature/utilitiescs-nullable-helperclasses-364`
- Base: `origin/epic/utilitiescs-nullable-remediation-integration` (merge-base `6d4da8bb`)
- Head: `2edda572b87593446c2ef5546eef71f660a0a35f`

## Summary

All 9 spec Definition-of-Done items, all 3 Seeded Test Conditions, and all 7 user-story Acceptance
Criteria evaluate PASS against the delivered evidence and independent verification of the branch
diff. The single item with a documented caveat (full toolchain / DoD #5, AC #5) is adjudicated PASS
for the in-scope obligation: the pragma-only type-check compiles all 42 opted-in HelperClasses files
with zero CS86xx, while the full-solution `TreatWarningsAsErrors` exit-1 is a pre-existing,
out-of-scope condition (vendored SVGControl CS0649 and non-HelperClasses CS0618/CS0168) that the
maintainer scope lock forbids remediating in this child. See the policy audit for the full
adjudication. No acceptance criterion is FAIL, PARTIAL, or UNVERIFIED.

## Scope and Baseline

Baseline for acceptance is the integration base
`origin/epic/utilitiescs-nullable-remediation-integration` (merge-base `6d4da8bb`). This is an epic
child; `ci.yml` triggers only on PRs to `main`/`development`, so the child→integration PR carries no
required CI checks. Absent child CI is expected and is not treated as a finding. Coverage baseline
and post-change captures are the feature-evidence Cobertura files under `evidence/`.

## Acceptance Criteria Inventory

- spec.md `## Definition of Done`: 9 checkbox items (all `[x]` in source).
- spec.md `## Seeded Test Conditions`: 3 checkbox items (all `[x]` in source).
- user-story.md `## Acceptance Criteria`: 7 checkbox items (all `[x]` in source).
- issue.md carries only an early-draft AC list; under full-feature mode the authoritative sources are
  spec.md and user-story.md (issue.md draft ACs are not evaluated as the source of truth).

## Acceptance Criteria Evaluation

### A. spec.md — Definition of Done (9)

| # | DoD item | Verdict | Evidence (file + verifying command/output) |
|---|---|---|---|
| 1 | Every `.cs` emitting CS86xx carries `#nullable enable`; zero CS86xx under per-file pragma + TWAE | PASS | `grep -rl '#nullable enable' UtilitiesCS/HelperClasses/` = 42 of 43; `evidence/qa-gates/final-nullable-build.2026-07-19T10-07.md` isolated build EXIT 0, 0 CS86xx |
| 2 | No project/solution `<Nullable>` element; `UtilitiesCS.csproj` retains none | PASS | `grep -nE '<Nullable>' UtilitiesCS/UtilitiesCS.csproj` → no match; csproj absent from diff; `evidence/qa-gates/csproj-no-nullable.2026-07-19T10-07.md` |
| 3 | Annotation/null-safety only; no behavior/API-semantics/refactor change | PASS | Diff review of FilePathHelper/Initializer/TraceUtility/Theme/PhysicalFileInfoAdapter shows only pragmas, `?`/`!`+`// why`, and deliberate `T?` returns; 4511/4511 tests green |
| 4 | All existing MSTest pass; no coverage regression on changed lines | PASS | `evidence/qa-gates/final-coverage.2026-07-19T10-07.md` (4511 passed); `coverage-delta.2026-07-19T10-07.md` (HelperClasses 92.07%→92.08%, no regression) |
| 5 | Full C# toolchain passes on final pass, pragma-only type-check | PASS (in-scope) | CSharpier EXIT 0; analyzer build EXIT 0; isolated pragma-only type-check EXIT 0 / 0 CS86xx; coverage EXIT 0. Full-solution TWAE exit 1 adjudicated pre-existing/out-of-scope in the policy audit |
| 6 | PhysicalFileInfoAdapter injectable-delegate seam preserved exactly | PASS | `grep` confirms all four seam fields, both ctors, `?? throw` guards present; `git diff` shows no seam line changed; `evidence/other/maintainer-flags.2026-07-19T09-35.md` |
| 7 | FileSystem adapter root-boundary `!` with `// why`; latent root-throws flagged not fixed | PASS | `PhysicalFileInfoAdapter.cs` diff shows `_fileInfo.Directory!` / `DirectoryName!` with `// why`; latent throw flagged in `maintainer-flags.2026-07-19T09-35.md` |
| 8 | DvgForm.Designer.cs handling + epic-scope conflict documented; Designer not hand-edited | PASS | File absent from diff; 0 pragmas in file; `evidence/other/maintainer-flags.2026-07-19T09-40.md` |
| 9 | PrettyPrint.cs 500-line pre-existing violation flagged not fixed | PASS | `wc -l` = 680; `evidence/other/maintainer-flags.2026-07-19T10-05.md`; not split |

### spec.md — Seeded Test Conditions (3)

| Condition | Verdict | Evidence |
|---|---|---|
| Existing MSTest suite passes post-annotation | PASS | `final-coverage.2026-07-19T10-07.md` (4511/4511) |
| No coverage regression on changed lines | PASS | `coverage-delta.2026-07-19T10-07.md` (HelperClasses 92.07%→92.08%) |
| Nullable gate passes for opted-in files using pragma-only build | PASS | `final-nullable-build.2026-07-19T10-07.md` (0 CS86xx across 42 opted-in files, isolated EXIT 0) |

### B. user-story.md — Acceptance Criteria (7)

| # | AC item | Verdict | Evidence |
|---|---|---|---|
| 1 | Every `.cs` emitting CS86xx carries `#nullable enable`; zero CS86xx under pragma + TWAE | PASS | as DoD #1 |
| 2 | No project/solution `<Nullable>` element | PASS | as DoD #2 |
| 3 | Annotation/null-safety only | PASS | as DoD #3 |
| 4 | All existing MSTest pass; no coverage regression on changed lines | PASS | as DoD #4 |
| 5 | Full C# toolchain passes on final pass, pragma-only type-check | PASS (in-scope) | as DoD #5; full-solution TWAE exit 1 adjudicated pre-existing/out-of-scope |
| 6 | DvgForm.Designer.cs handling + epic-scope conflict documented; Designer not hand-edited | PASS | as DoD #8 |
| 7 | PrettyPrint.cs 500-line pre-existing violation flagged not fixed | PASS | as DoD #9 |

## Acceptance Criteria Check-off

All items in `spec.md` (`## Definition of Done` and `## Seeded Test Conditions`) and `user-story.md`
(`## Acceptance Criteria`) were already `[x]` in the source files when this audit began (checked off
by the executor during delivery). Each has been independently re-verified PASS in this audit; no
additional check-off changes were required. No item was left unchecked, so none needed to be
downgraded.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-07-18-utilitiescs-nullable-helperclasses-364/spec.md`,
  `docs/features/active/2026-07-18-utilitiescs-nullable-helperclasses-364/user-story.md`
- Total AC items: 19 (9 DoD + 3 Seeded Test Conditions + 7 user-story AC)
- Checked off (delivered): 19
- Remaining (unchecked): 0
- Items remaining: none

## Verdict

Feature acceptance verdict: PASS. All 19 acceptance items PASS. Blocking findings: 0. PARTIAL: 0.
FAIL: 0. UNVERIFIED: 0.
