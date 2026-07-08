# Feature Audit — Issue #267 (ci-quality-gates-speedup)

- Review timestamp: 2026-07-08T01-41
- Work mode: `minor-audit`
- AC source: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/issue.md`, `## Acceptance Criteria` section only (per work-mode routing; `spec.md`/`user-story.md` are absent from the feature folder, consistent with `minor-audit`)

## Scope and Baseline

- Resolved base branch: `main` (`origin/main`)
- Merge-base SHA: `5c4bf31e25210eb850827f2668c74cd72d5fa231` (independently recomputed via `git merge-base HEAD origin/main`; matches the caller-supplied value)
- Branch head: `refactor/ci-quality-gates-speedup-267` @ `7ffc96cc67e85983d6034632d4fd1fd466deda5c`
- Diff scope: 22 files changed, 774 insertions(+), 2 deletions(-); one production file (`.github/workflows/ci.yml`, +18/-2) and 21 markdown feature/evidence files.
- Baseline for AC1–AC4 comparison: the pre-edit `.github/workflows/ci.yml` at the merge-base commit, as inventoried line-for-line in `evidence/baseline/investigation-notes.2026-07-07T20-45.md` and independently cross-checked against `git diff` in this review.

## Acceptance Criteria Inventory

| ID | Criterion (verbatim from `issue.md`) |
|---|---|
| AC1 | `.github/workflows/ci.yml` restores NuGet packages from a cache keyed on `**/packages.config` and falls back to a restore on cache miss. |
| AC2 | `.github/workflows/ci.yml` caches the CSharpier tool restore keyed on `dotnet-tools.json`. |
| AC3 | The msbuild invocation(s) pass `/m` for parallel project builds. |
| AC4 | The analyzer/code-style enforcement and the nullable `TreatWarningsAsErrors` enforcement are both preserved (consolidated into one build pass or retained as two), with no reduction in enforced diagnostics. |
| AC5 | `actionlint` passes on the modified workflow. |
| AC6 | A green CI run against the branch head is produced (the `modified-workflow-needs-green-run` gate) before merge. |

## Acceptance Criteria Evaluation

### AC1 — NuGet package cache with restore-on-miss fallback

**PASS.** Independently confirmed by reading the full modified `.github/workflows/ci.yml`: a `Cache NuGet packages` step (`actions/cache@v4`, `path: packages`, `key: nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}`, `restore-keys: nuget-${{ runner.os }}-`) is inserted between "Setup NuGet" and "Restore solution." The "Restore solution" step (`nuget restore $env:SOLUTION_PATH`) carries no `if:` guard, so it executes unconditionally on both cache hit and cache miss. `packages/` is confirmed as the repo's actual restore target directory (exists at repo root). Evidence: `evidence/qa-gates/cache-placement-check.2026-07-07T22-00.md`; independently re-verified.

### AC2 — CSharpier tool-restore cache keyed on `dotnet-tools.json`

**PASS.** A `Cache dotnet tools` step (`path: ~/.nuget/packages`, `key: dotnet-tools-${{ runner.os }}-${{ hashFiles('dotnet-tools.json') }}`, `restore-keys: dotnet-tools-${{ runner.os }}-`) is inserted between "Restore solution" and "Setup CSharpier." "Setup CSharpier" (`dotnet tool restore`) carries no `if:` guard. `dotnet-tools.json` exists at repo root and is confirmed as the correct `hashFiles()` target. Evidence: `evidence/qa-gates/cache-placement-check.2026-07-07T22-00.md`; independently re-verified. (See `code-review.2026-07-08T01-41.md` for a non-blocking observation on cache-path breadth.)

### AC3 — `/m` on the msbuild invocation(s)

**PASS.** Both retained `msbuild ... /t:Build` invocations carry `/m` immediately after `/t:Build` (lines 98 and 106 of the modified file); `grep -c "/t:Build"` confirms exactly two invocations exist (not one, not three). Evidence: `evidence/qa-gates/parallel-build-flag-check.2026-07-07T22-00.md`; independently re-verified by reading the full file.

### AC4 — Both enforcement passes preserved, no reduction in enforced diagnostics

**PASS.** Per the documented Scope Decision (2026-07-07), the two original steps ("Build with analyzers and code style enforcement," "Build with nullable warnings treated as errors") are retained as two separate steps (the "retained as two" branch of AC4) rather than consolidated. Each retained step's property set is unchanged from the pre-edit baseline except for the added `/m`. `evidence/qa-gates/build-diagnostic-parity.2026-07-07T22-00.md` compares final diagnostic counts against both pre-edit baselines and finds no enforced diagnostic dropped (pass 1: 33→72 warnings, attributable to incremental-build-state variance at capture time, not a property change; pass 2: 0/0 in both baseline and final, an exact reproduction of a pre-existing MSBuild incremental short-circuit). The Scope Decision's rationale for retaining two passes rather than consolidating is sound: a consolidated pass was shown to surface 84 pre-existing nullable defects in vendored `SVGControl`/`UtilitiesSwordfish.NET.General` that the current two-pass sequence's incremental short-circuit hides, and consolidating would therefore not be behavior-neutral. That discovered gap is correctly tracked as a separate, out-of-scope follow-up (`docs/features/potential/2026-07-07-ci-nullable-check-skipped-vendored-projects.md`) rather than folded into this feature. The two full-solution `msbuild` passes were not independently re-executed in this review (build-time cost); the executor's evidence was inspected instead and found detailed and internally consistent.

### AC5 — `actionlint` passes on the modified workflow

**PASS.** Independently re-run in this review: `./actionlint-bin/actionlint.exe .github/workflows/ci.yml` exits 0 with zero findings, matching the executor's own final evidence (`evidence/qa-gates/actionlint-final.2026-07-07T22-00.md`, `EXIT_CODE: 0`).

### AC6 — Green CI run against the branch head (`modified-workflow-needs-green-run`)

**FAIL — unmet, correctly left unchecked.** No evidence of a green GitHub Actions run (or `workflow_dispatch` run) against branch head `7ffc96cc67e85983d6034632d4fd1fd466deda5c` exists in this repository or working tree. `gh` (GitHub CLI) is unavailable in this environment, so live CI status cannot be queried directly; no local substitute evidence (workflow-run JSON, status badge capture, etc.) is present either. `issue.md` and the plan's own evidence correctly record AC6 as an intentionally out-of-band gate rather than falsely claiming completion — this is accurate self-reporting, not a documentation defect — but the criterion itself remains unmet as of this review. This is a Blocking finding per the `modified-workflow-needs-green-run` policy rule (see `policy-audit.2026-07-08T01-41.md` § 5.3) and is carried into `remediation-inputs.2026-07-08T01-41.md`.

## Summary

- AC1–AC5: PASS (5 of 6), each independently re-verified where practical in this review (actionlint re-run; full workflow file read; extension-level diff-scope check).
- AC6: FAIL (unmet) — a green CI run against the branch head has not yet been produced; this is a process gate that occurs after the PR is opened, and is expected to remain unmet until that step is completed. It is correctly recorded as out-of-band in `issue.md` rather than falsely checked off.
- No other acceptance criteria, test conditions, or documented constraints were found unmet.
- Overall feature-audit disposition: **PARTIAL** — five of six criteria pass; the sixth is a known, correctly-tracked, not-yet-executed gate rather than a defect in the delivered change.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/issue.md`
- Total AC items: 6
- Checked off (delivered): 5 (AC1, AC2, AC3, AC4, AC5 — already checked in `issue.md` prior to this review, with supporting evidence independently re-verified in this cycle)
- Remaining (unchecked): 1
- Items remaining: AC6 — "A green CI run against the branch head is produced (the `modified-workflow-needs-green-run` gate) before merge."

## Acceptance Criteria Check-off

No new check-off action was taken in `issue.md` during this review: AC1–AC5 were already checked off (`[x]`) by the executor prior to this review, each with supporting evidence that this review independently re-verified (actionlint re-run, full-file read, diff-scope extension check) and confirms as accurate. AC6 remains correctly unchecked (`[ ]`) — it evaluates to FAIL in this audit and must not be checked off until a qualifying green CI run against the branch head exists.
