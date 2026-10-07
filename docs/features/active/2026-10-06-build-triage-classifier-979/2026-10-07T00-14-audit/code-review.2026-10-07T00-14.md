# Code Review: Build Triage Classifier (#979)

Review date: 2026-10-07
Reviewer: Codex feature reviewer
Feature folder: `docs/features/active/2026-10-06-build-triage-classifier-979`
Feature folder selection rule: The PR context and issue #979 requirements resolve this active folder.
Base branch: `origin/main` at `5ddf7f03d6b92b2981cd0d5d74f10a0733e80964`
Head branch: `feature/build-triage-classifier-979` at `95f9bab63319ff15ba0e5008c484879b392317be`
Review type: Remediation pass 3 full-feature re-review

## Executive Summary

The complete feature branch was reviewed against its resolved merge base without narrowing scope. The range contains 146 changed files: 13 C# files, 3 C# project files, 1 ribbon XML file, and 129 feature-planning, review, remediation, and evidence documents. The product change preserves nullable Triage values, rebuilds the classifier from exact A/B/C mined-mail labels, initializes and publishes rebuilt state through existing infrastructure, adds the requested ribbon command, and resolves an absent/disabled-engine dispatch defect. The tests cover the model, mappings, filtering, state, persistence, replacement, ribbon location, callback, and lazy engine resolution.

No Blocker, Major, Minor, or Nit finding remains. The history-isolation remediation removes every `.agents` and `.codex` path from the current feature range while retaining exact patch identity for the three issue commits. The whitespace remediation passes the complete-range and final-commit checks. CR-979-3 and CR-979-4 are resolved. Existing analyzer, compiler, and 5,495-test evidence remains applicable because the product patches are byte-for-byte equivalent under `git range-diff`; the reviewer independently reran the formatter and Git scope/hygiene checks.

**What changed:**

- Added nullable Triage preservation to mined-mail construction, copy, JSON, and item projection.
- Added mined-mail Triage classifier reconstruction with exact-label filtering, aggregate initialization, persistence, and manager replacement.
- Added `Build Triage Classifier` at `TaskMaster -> Settings -> Folder Classifier` and awaited rebuild dispatch through the viewer/controller path.
- Added focused MSTest coverage and explicit legacy-project inclusions.
- Isolated the feature history from unrelated policy/harness work and corrected prior artifact whitespace.

**Top review considerations:**

1. GitHub CI has not run because no pull request exists; this review relies on the accepted local QA evidence and independent check-only verification.
2. Coverage is below the standing aggregate threshold, but the user's one-time authorization applies to every issue #979 coverage requirement; all numeric results remain recorded.
3. The working tree contains a pre-existing Cycle 3 plan checkbox update outside the committed review boundary; no C# or project file has an uncommitted change.

**PR readiness recommendation:** **Go** — all complete-range code, policy, functional-test, acceptance, scope, and diff-hygiene gates pass, with coverage handled by the explicit issue #979-only authorization.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `.agents/**`; `.codex/**` | Complete feature range | CR-979-3 is resolved: no policy or harness path remains in the isolated feature range. | Preserve the backup and snapshot refs through normal PR completion. | The isolated branch is cohesive while the former branch state remains recoverable. | `git diff --name-only 5ddf7f03..95f9bab6 -- .agents .codex` returns no path; `refs/heads/backup/issue-979-pre-isolation-f09f2ae2` resolves to `f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7`. |
| Info | Prior review/remediation Markdown | Complete feature range and `95f9bab6` | CR-979-4 is resolved: the 19 documented trailing spaces were removed without semantic edits. | No further action. | Both relevant Git whitespace gates now pass. | `git diff --check 5ddf7f03..95f9bab6` and `git diff-tree --check 95f9bab6^ 95f9bab6` exit 0; `p3-t2-whitespace-only-content-proof.2026-10-06T23-59.md`. |
| Info | C# feature scope | Coverage | Aggregate and changed-code coverage are recorded, and all issue #979 coverage requirements are covered by the user's one-time exception. | Keep the exception confined to issue #979. | Coverage does not block this review, while all noncoverage gates remain mandatory and pass. | Baseline 65.1433%; post-change 65.2006%; feature methods 93.55% to 100%; `coverage-exception.2026-10-06T21-37.md`. |

No Blocker or Major findings. No noncoverage finding requires remediation.

## Implementation Audit

### C# implementation audit

#### What changed well

- `MinedMailInfo` models Triage as nullable data and carries the value through its constructor, deep copy, serializer, and `EmailDataMiner` projections.
- `Triage.MinedMailRebuild.cs` limits training data to the existing exact `A`, `B`, and `C` class names. Null, empty, lowercase, and out-of-contract labels do not affect aggregate counts or the token-base input.
- The rebuild creates classifiers through the existing factory, assigns the total valid count, creates the shared token base, rebuilds each class, persists the current group configuration, replaces the manager entry through `ToAsyncLazy`, and publishes the rebuilt group only after successful reconstruction.
- Missing AppData, null input, and invalid-only input return `false` without publishing or persisting replacement state.
- Ribbon XML places the exact label in the requested menu. The viewer callback awaits the controller. The controller uses the active engine when available and otherwise awaits `TriageAsync`, so a disabled or absent engine still reaches the rebuild.
- Focused test extraction keeps every issue-specific file within the 500-line repository limit and retains explicit inclusion for the legacy project format.

#### Type safety and API notes

- Nullable intent is explicit on the model field and the injected delegates.
- Test seams are `internal`; the feature does not add a breaking public API or external dependency.
- Final analyzer and compiler/nullability evidence records zero warnings and zero errors.
- `dotnet tool run csharpier check .` passed independently at the reviewed head.

#### Error handling and logging

- The implementation validates availability and usable input before mutating active state.
- It introduces no broad exception catch, suppressed error, ad hoc console output, unsafe resource ownership, or subprocess boundary.
- Existing serialization and manager behavior remain the error-propagation boundaries.

## Test Quality Audit

The feature tests are deterministic and use fixed in-memory inputs, Moq, and internal delegates. They cover A/B/C/null preservation; exact valid-label inclusion; null, empty, lowercase, and invalid-label exclusion; aggregate state; shared token state; persistence; active replacement; staged-file loading; missing AppData; exact ribbon XML location; callback shape; awaited dispatch; injected dispatch; and absent-engine lazy initialization. No feature test depends on Outlook, network access, or a temporary file.

The final accepted suites report 5,017 UtilitiesCS tests and 478 TaskMaster standard-QC tests passing. Analyzer and compiler/nullability builds are clean. Patch identity is independently established by three exact `=` mappings and no changed, added, or removed mappings, so those functional results remain applicable after history isolation. The current reviewer check confirms 1,650 C# files are CSharpier-clean.

### Reviewed test and QA artifacts

- `evidence/qa-gates/p3-t2-format-check-retry.2026-10-06T23-28.md` — final implementation formatter evidence.
- `evidence/qa-gates/p3-t3-analyzers-retry.2026-10-06T23-28.md` — zero-warning, zero-error analyzer rebuild.
- `evidence/qa-gates/p3-t4-nullable-retry.2026-10-06T23-29.md` — zero-warning, zero-error compiler/nullability rebuild.
- `evidence/qa-gates/p3-t5-utilities-vstest.2026-10-06T23-29.md` — 5,017 UtilitiesCS tests passed.
- `evidence/qa-gates/p3-t6-taskmaster-vstest.2026-10-06T23-30.md` — 478 TaskMaster tests passed with the standard `LiveOutlook` exclusion.
- `evidence/regression-testing/p2-t3-range-diff-identity.2026-10-06T23-58.md` — exact identity for all three issue patches after replay.
- `evidence/qa-gates/p4-t3-final-diff-hygiene-and-scope.2026-10-07T00-00.md` — final full-range scope and hygiene inventory.
- `evidence/other/p4-t4-cycle3-acceptance-summary.2026-10-07T00-00.md` — 12/12 authoritative criteria and 5/5 issue cross-checks supported.

### Quality assessment

- **Determinism:** Fixed labels, records, token inputs, mocks, and injected delegates avoid variable external state.
- **Isolation:** Focused test classes target model mapping, classifier reconstruction, XML shape, or dispatch behavior.
- **Speed:** The accepted UtilitiesCS and TaskMaster suites completed in approximately 20 seconds combined.
- **Diagnostics:** Scenario-specific MSTest names and FluentAssertions identify the failed behavior and expected state.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Complete diff and PR context inspection show no credential or secret material. |
| No unsafe subprocess or command construction | PASS | The product change creates no subprocess or command string. |
| Input validation at boundaries | PASS | Only exact A/B/C labels become classifier training records; unavailable and invalid-only inputs return `false`. |
| Error handling remains explicit | PASS | State is not published before validation, reconstruction, and persistence; existing exceptions are not suppressed. |
| Configuration and path handling is safe | PASS | Loading uses the established AppData/Bayesian location and existing manager configuration entry. |
| Policy ownership | PASS | The isolated feature range contains no `.agents` or `.codex` path. |
| Diff hygiene | PASS | Full-range and final-commit whitespace checks exit 0. |
| Patch preservation | PASS | Three feature patches map exactly under `git range-diff`; backup and snapshot refs remain present. |

## Research Log

No external research was required. The review used repository policy, the canonical PR-context summary and appendix, the complete base-to-head diff, authoritative feature requirements, all Cycle 3 evidence, prior review findings, accepted QA evidence, and independent local check-only commands.

## Verdict

**Go.** The complete feature branch is ready for normal PR flow at reviewed head `95f9bab63319ff15ba0e5008c484879b392317be`. CR-979-3 and CR-979-4 are resolved, no noncoverage blocker or meaningful partial result remains, and all 12 authoritative acceptance criteria plus all five `issue.md` cross-checks are supported. The issue #979-only coverage authorization is the sole exception. This review did not create or push a pull request.
