# Code Review: Build Triage Classifier (#979)

Review date: 2026-10-06
Reviewer: Codex feature reviewer
Feature folder: `docs/features/active/2026-10-06-build-triage-classifier-979`
Base branch: `main` at merge base `c76e830c18976221b5730f84b8d88aebbfc4f04b`
Head branch: `feature/build-triage-classifier-979` at `f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7`
Review type: Final post-remediation re-review

## Executive Summary

The issue #979 product change is implemented and supported by passing final C# evidence. `MinedMailInfo` retains nullable Triage values, the rebuild accepts only exact A/B/C labels, classifier state is initialized and published through existing persistence and manager paths, and the requested ribbon command resolves both enabled and disabled Triage-engine states. The focused test extraction resolves the previous 500-line finding and preserves all assertions.

The full feature-vs-base diff contains two branch-level blockers outside the product logic. Inherited commit `35e748279` changes eight policy files and 26 Codex harness files that predate the user feature request, and four prior review/remediation documents contain 19 trailing-whitespace errors. These findings are not coverage requirements and are not covered by the issue #979 exception.

What changed:

- Added nullable Triage preservation to mined mail and its copy/mapping paths.
- Added mined-mail Triage classifier reconstruction, persistence, and manager replacement.
- Added `Build Triage Classifier` at the requested ribbon location and repaired absent-engine initialization.
- Extracted feature tests into compliant focused files with legacy project inclusion.

Top risks:

1. The PR would include inherited orchestration policy and Codex harness changes from `35e748279`.
2. The committed branch currently fails `git diff --check` with 19 Markdown whitespace errors.
3. CI has not yet run because the branch has not passed review and reached PR creation.

PR readiness recommendation: **Needs Revision** — preserve the inherited branch state, replay only the three issue #979 commits onto clean `main`, and correct the 19 whitespace diagnostics before repeating review.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocker | `.agents/skills/**` and `.codex/**` | Commit `35e748279` | The issue #979 branch includes 34 inherited harness files; eight are policy documents whose modification is prohibited in the feature diff. | Create and verify a backup ref for the current branch, then isolate the issue #979 commits onto current `main` without editing or reverting the inherited policy files. | This preserves others' work, produces a cohesive feature PR, and avoids direct policy-file mutation. | `git show --stat 35e748279`; `git diff --name-only c76e830..f09f2ae -- .agents/skills` lists eight files; `git diff 35e748279..f09f2ae -- .agents .codex` is empty. |
| Blocker | `docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T23-00-audit/*.md`; `2026-10-06T23-01-remediation/remediation-inputs.2026-10-06T23-01.md` | Metadata lines | Four committed audit/remediation artifacts contain 19 trailing-whitespace errors. | Remove only the reported trailing spaces and rerun the exact merge-base-to-head diff check. | The final branch must pass the repository's diff-hygiene gate; the current check exits 2. | `git diff --check c76e830c18976221b5730f84b8d88aebbfc4f04b..f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7`. |

## Implementation Audit

### C# implementation audit

#### What changed well

- The model uses nullable Triage data and preserves values through construction, deep copy, JSON, and mining projections.
- The rebuild filters exact A/B/C labels and leaves null, empty, lowercase, and invalid-only inputs without a state mutation.
- Aggregate counts and shared token state are initialized before persistence and active manager replacement.
- The ribbon controller awaits lazy Triage resolution when the active engine is absent, then invokes the mined-mail rebuild.
- The second remediation places issue-specific tests in focused files and retains explicit legacy `.csproj` inclusion.

#### Type safety and API notes

- CSharpier, analyzer, and warnings-as-errors evidence is clean.
- Optional values are explicitly nullable, and the injected delegates are internal test seams.
- No external dependency or public API break was introduced.

#### Error handling and logging

- Missing AppData, null collections, and invalid-only data return `false` without persistence or replacement.
- No broad exception handling or ad hoc production console output was introduced.

### PowerShell implementation audit

#### What changed well

- The seven scripts passed canonical PoshQC format, analysis, and test gates at the prior reviewed head.
- The script blobs are unchanged between `3a355e14` and `f09f2ae`, and all remain below 500 lines.

#### API and safety notes

- No functional PowerShell defect was identified.
- The entire PowerShell scope belongs to inherited commit `35e748279`; history isolation should exclude it from the issue #979 PR while preserving it under the backup ref.

#### Error handling and logging

- PoshQC analysis reported no blocking diagnostic for the hook scope.

## Test Quality Audit

The final suites pass 5,017 UtilitiesCS tests and 478 TaskMaster standard-QC tests. The tests cover model preservation, serialization, strict item projection, exact valid-label filtering, invalid and empty inputs, aggregate classifier state, persistence, manager replacement, ribbon XML location, callback dispatch, and disabled-engine lazy initialization. The tests use fixed in-memory inputs and repository-standard mocks without Outlook, network, or temporary-file dependencies.

The one-time user exception covers all issue #979 coverage requirements. Coverage failures remain documented but do not trigger remediation. It does not waive either branch-level finding in this review.

### Reviewed test and QA artifacts

- `evidence/qa-gates/p3-t2-format-check-retry.2026-10-06T23-28.md` — final CSharpier check.
- `evidence/qa-gates/p3-t3-analyzers-retry.2026-10-06T23-28.md` and `p3-t4-nullable-retry.2026-10-06T23-29.md` — zero-diagnostic builds.
- `evidence/qa-gates/p3-t5-utilities-vstest.2026-10-06T23-29.md` — 5,017 tests passed.
- `evidence/qa-gates/p3-t6-taskmaster-vstest.2026-10-06T23-30.md` — 478 tests passed with the standard `LiveOutlook` exclusion.
- `evidence/qa-gates/p3-t7-file-size-and-diff-hygiene.2026-10-06T23-30.md` — extraction line counts; its pre-commit diff-hygiene conclusion is superseded by the exact committed-head check in this review.

Quality assessment:

- Determinism: fixed inputs, mocks, and injected delegates avoid external dependencies.
- Isolation: each focused class targets a model, rebuild, mapping, XML, or callback boundary.
- Speed: final suites completed in approximately 20 seconds combined.
- Diagnostics: scenario-specific names and FluentAssertions provide clear failures.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Full diff and PR context contain no credential addition. |
| No unsafe subprocess or command construction | PASS | The C# feature adds no subprocess execution; PoshQC passed for the unchanged hooks. |
| Input validation at boundaries | PASS | Only exact A/B/C labels become classifier training input. |
| Error handling remains explicit | PASS | Empty or unavailable data returns without publishing replacement state. |
| Configuration / path handling is safe | PASS | Staged loading uses the existing AppData/Bayesian path and manager configuration. |
| Policy ownership | FAIL | Eight inherited policy documents are present in the feature-vs-base diff. |

## Research Log

No external research was required. The review used repository policy, fresh PR context, the full committed diff, feature evidence, and local check-only verification.

## Verdict

**Needs Revision.** The requested functionality and all five issue acceptance criteria pass, and the test-file extraction resolves the prior finding. PR creation remains blocked until the feature history is safely isolated from inherited commit `35e748279` and the 19 trailing-whitespace diagnostics are corrected. Both are autonomous remediation items; coverage is not part of the required remediation.
