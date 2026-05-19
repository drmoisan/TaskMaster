# Code Review — outlook-recipient-com-cross-thread-crash-124 (2026-04-08T12-14)

- **Feature folder:** `docs/features/active/2026-04-08-outlook-recipient-com-cross-thread-crash-124/`
- **Feature folder selection rule:** Used the user-specified active feature folder because it exists on disk, matches issue suffix `-124`, and contains `issue.md`, the approved plan, and canonical evidence.
- **Current branch inspected:** `bug/outlook-recipient-com-cross-thread-crash-124`
- **Base branch:** `development`
- **Work mode:** `minor-audit`
- **Primary diff evidence:** refreshed `artifacts/pr_context.appendix.txt` working-tree diff plus current branch status

## Executive Summary

This branch implements a focused Outlook COM-safety fix in two production files and two targeted test files. `MailItemHelper` now materializes COM-backed tokenization dependencies before background token access, and `RecipientStatic` now catches Exchange directory failures and falls back to safe recipient display/address data. The live review-time QA loop passed, the two focused regressions passed, and no file-local diagnostics remain in the changed C# files.

**Top risks**

1. **Live Outlook manual verification remains external to this review.** The automated tests prove the helper and recipient fallback behaviors, but a human still needs to confirm the add-in no longer crashes in a real Outlook session.
2. **The branch is still uncommitted relative to `development`.** PR context summary cannot show a commit-range diff until the working-tree changes are committed, so this review relies on refreshed appendix diff evidence instead.
3. **Repository aggregate coverage remains below 80%.** This fix improves the baseline slightly and gives both changed production files >80% line coverage, but the wider repository still sits below the global aspirational floor.

**Go/No-Go recommendation:** **Go** for PR preparation against `development`. No blockers or major defects were identified in the reviewed implementation.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Minor | `docs/features/active/2026-04-08-outlook-recipient-com-cross-thread-crash-124/issue.md` | `## Acceptance Criteria` vs. live product behavior | The issue's four acceptance criteria are all satisfied by code and automated QA, but the real Outlook add-in scenario is still only indirectly validated by test doubles. | Perform the planned manual Outlook repro/verification before merge or release promotion. | COM apartment/thread-affinity bugs can still surface only in a live Outlook host even when the unit-level fallback logic is correct. | `issue.md`; focused regressions; full QA loop results |
| Nit | `artifacts/pr_context.summary.txt` | `Base/Head` section | Refreshed PR context summary shows `development` and HEAD at the same commit because the feature changes are still uncommitted in the working tree. | Commit the reviewed changes before opening a PR so future audit/PR tooling can use a normal commit-range diff. | This is a process limitation, not a code defect, but it weakens commit-range PR summaries until the branch state is recorded. | Refreshed `artifacts/pr_context.summary.txt`; `artifacts/pr_context.appendix.txt` |
| Nit | Repository-wide coverage | QA evidence | Aggregate line coverage remains `78.18%`, below the repo-wide aspirational `>= 80%` floor, even though this bug fix improved coverage and raised both touched production files above 80%. | Treat as non-blocking for this scoped bug fix, but continue the repository-wide coverage uplift work separately. | The change did not regress coverage and directly tests the modified behavior; resolving the remaining aggregate gap is outside this issue's approved small-path scope. | `evidence/qa-gates/csharp-mstest-coverage.2026-04-08T12-02.md`; `evidence/qa-gates/csharp-coverage-summary.2026-04-08T12-02.md` |

No Blocker or Major findings were identified.

## Changed Scope Review

### Production files

- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`
  - `FromMailItemAsync` now constructs the helper on the caller thread and eagerly materializes tokenization dependencies before returning.
  - `TokenizeAsync` also materializes dependencies before dispatching tokenization work to `Task.Run`.
- `UtilitiesCS/OutlookObjects/Recipient/RecipientStatic.cs`
  - `GetRecipientName` and `GetRecipientAddress` now catch Exchange directory lookup failures and route through explicit fallback helpers.
  - Helper methods centralize Exchange-address detection plus recipient-name/address fallback behavior.

### Test files

- `UtilitiesCS.Test/OutlookObjects/MailItem/MailItemHelperCoreTests.cs`
  - Adds a regression proving background token access does not cause additional COM-backed property reads after helper creation.
- `UtilitiesCS.Test/OutlookObjects/Recipient/RecipientStaticTests.cs`
  - Adds a regression proving COM exceptions in Exchange-user property access degrade to safe recipient display data rather than surfacing an unhandled crash.

## C# Review Notes

### Correctness and design

- The fix remains minimal and aligns with the issue's root-cause chain: avoid off-thread COM-backed lazy evaluation, and make recipient fallback resilient when Exchange directory access fails.
- `MailItemHelper.FromMailItemAsync` preserves the public signature and changes only the internal execution timing.
- `RecipientStatic` follows the existing sender-helper pattern of try/catch plus fallback rather than introducing broader structural change.

### Type safety and diagnostics

- Live analyzer build passed with `0 Warning(s)` and `0 Error(s)`.
- Live nullable-as-errors build passed with `0 Warning(s)` and `0 Error(s)`.
- Editor diagnostics for all four changed C# files report no errors.

### Security and safety

- No secrets or credentials were introduced.
- No subprocess or dynamic execution surface was added.
- The change reduces runtime crash risk by isolating COM-affine access to safer call sites and by catching Outlook directory exceptions.

## Typed Python Audit

Not applicable. No Python files changed in this feature branch scope.

## Test Quality Audit

| Check | Status | Notes |
|---|---|---|
| Deterministic and isolated | PASS | Tests use Moq-backed COM stubs only; no external services or temp files. |
| Focused on one behavior each | PASS | One test targets recipient fallback; one targets mail-helper materialization before background token access. |
| Clear diagnostics | PASS | Assertions verify the exact fallback values and the no-extra-COM-read behavior. |
| Coverage contribution | PASS | Full coverage run added 2 tests and improved overall coverage slightly; both changed production files are above 80% line coverage. |

## Security / Correctness Checks

| Check | Status | Notes |
|---|---|---|
| No secrets in code | PASS | No credentials or tokens were added. |
| No unsafe subprocess usage | PASS | No process-spawning changes were introduced. |
| Input / boundary safety | PASS | Defensive fallback around Outlook Exchange directory properties is now explicit. |
| Crash-risk reduction | PASS | The new helper materialization and fallback helpers directly address the reported COM crash mode. |

## Recommendation

**No-Go for remediation; Go for PR preparation.**

The implementation is consistent with the approved small-path scope, code quality is acceptable, and the validated evidence supports the explicit bug-fix acceptance criteria. The remaining items are informational process caveats rather than code defects.