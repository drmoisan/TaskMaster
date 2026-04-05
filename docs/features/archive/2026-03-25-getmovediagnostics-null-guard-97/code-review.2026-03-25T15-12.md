# Code Review — getmovediagnostics-null-guard-97 (2026-03-25T15-12)

- **Feature folder:** `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/`
- **Current branch inspected:** `getmovediagnostics-null-guard-97` @ `66220df0089cc10e6a32f4ed29aa7558f5cc2596`
- **Base branch:** `origin/feature/utilities-coverage-part-three-87` @ `3b472b211b0066000f7b0f6582c5eb977dd2ba69`
- **Comparison source:** corrected `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`
- **Feature folder selection rule:** Used the user-supplied `plan.2026-03-25T12-00.md` in the `#97` feature folder; the branch suffix and `issue.md` both resolve to issue `#97`.
- **Supersedes:** the earlier `*.2026-03-25T14-57.md` review set that used the wrong base comparison.

## Executive summary

**What changed relative to `origin/feature/utilities-coverage-part-three-87`**
- `QuickFiler/Controllers/QfcCollectionController.cs` adds a null guard around `olAppointment.Body` access in `GetMoveDiagnostics(...)`.
- `QuickFiler/Controllers/QfcHomeController.cs` adds a null guard around `olEmailCalendar.Items.Add()` in `QuickFileMetrics_WRITE(...)`.
- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` adds direct regression coverage for null `AppointmentItem` handling.
- `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` adds regression coverage for the missing-calendar path.
- `QuickFiler.Test/QuickFiler.Test.csproj` now compiles the new controller test file.
- The same corrected diff still includes unrelated `.codex/*` files and `.github/skills.zip`.

**Top 3 risks**
1. **Blocker — unrelated `.codex` / tooling content remains in the corrected upstream diff.**
2. **Major — the corrected PR-context summary does not agree with the corrected appendix about what changed.**
3. **Major — canonical feature evidence and plan state remain out of sync (`plan.md` vs `plan.2026-03-25T12-00.md`, plus missing Phase 2 QA artifacts).**

**Go / No-Go recommendation:** **No-Go for PR readiness against `origin/feature/utilities-coverage-part-three-87`.**
The `#97` bug fix itself is sound, but the branch and feature-doc state are not yet clean enough for review approval.

## Findings table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocker | `artifacts/pr_context.appendix.txt` | lines 265–272, 324–330 | The corrected upstream diff still includes unrelated `.codex` agent/skill files and `.github/skills.zip`. | Remove or split the `.codex` and skills-archive changes into their own branch or stacked child before reviewing `#97`. | These files are not required to deliver the Outlook null-guard bug fix. | `artifacts/pr_context.appendix.txt` |
| Major | `artifacts/pr_context.summary.txt` | lines 97–110 | The corrected summary still reports `Core logic changes: 0 files` and classifies the diff as docs/tooling-only, even though the appendix lists modified `QuickFiler` production and test files. | Regenerate or repair the summary so it accurately reflects the appendix and the real changed code/test files. | Review automation relies on the summary as the primary source of truth; inconsistent artifacts weaken audit confidence. | `artifacts/pr_context.summary.txt`; `artifacts/pr_context.appendix.txt` |
| Major | `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/` | feature root and `evidence/qa-gates/` | The feature folder still lacks `qc-nullable.md`, `qc-regression-tests.md`, and `qc-coverage.md`, while the active plan leaves the corresponding tasks unchecked and the legacy `plan.md` is still present in corrected diff evidence. | Canonicalize the active plan filename, create the missing QA artifacts, and then synchronize the plan checklist to the artifacts on disk. | Minor-audit reviews require deterministic evidence, not just successful commands in a terminal session. | `plan.2026-03-25T12-00.md`; folder listing; corrected appendix lines 34, 62, 83, 288 |
| Minor | `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | `CreateControllerWithOneGroup(...)` | The direct regression harness uses `FormatterServices.GetUninitializedObject(...)` and reflection to inject private state. | Keep the test for now, but treat it as a legacy seam and prefer constructor-safe test hooks in future refactors. | The current controller constructor is WinForms-heavy, so the approach is understandable but brittle. | `QfcCollectionControllerTests.cs` |
| Minor | `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` | `GetMoveDiagnostics_NullAppointment_DoesNotThrow()` | This integration-path test validates null propagation through `QuickFileMetrics_WRITE(...)`, but it mocks `IQfcCollectionController.GetMoveDiagnostics(...)` rather than exercising the concrete implementation. | Keep it as a boundary test and rely on `QfcCollectionControllerTests.GetMoveDiagnostics_WhenAppointmentIsNull_DoesNotThrow()` for the direct implementation regression. | The branch still has adequate direct concrete coverage through the dedicated collection-controller test file. | `QfcHomeControllerTests.cs`; `QfcCollectionControllerTests.cs` |

## C# review notes

### Design and null-safety

- The production fix is minimal and correctly targets the two actual dereference points.
- No public API signatures changed.
- The fix preserves existing behavior when the Outlook calendar folder exists.
- The null path now degrades gracefully by skipping appointment creation and body mutation when the folder is absent.

### Test review

- The new tests are deterministic and avoid network or temporary-file usage.
- `QfcCollectionControllerTests.GetMoveDiagnostics_WhenAppointmentIsNull_DoesNotThrow()` directly exercises the concrete controller behavior.
- `QfcHomeControllerTests.QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow()` covers the missing-calendar path in the home controller.
- `QuickFiler.Test.csproj` explicitly includes `Controllers\QfcCollectionControllerTests.cs`, so the new test file is compiled.

### Security and correctness checks

- No secrets or credentials were introduced.
- No subprocess or shell execution was added.
- No new external runtime dependency was added.
- The fix reduces the likelihood of COM-null crashes in the metrics path.

## Typed Python audit

No Python files are changed in the corrected `#97` feature scope. Python-specific typed-audit findings are **not applicable**.

## Test quality audit

| Criterion | Status | Notes |
|---|---|---|
| Deterministic and isolated | PASS | New tests use mocks and in-memory state only. |
| MSTest / Moq / FluentAssertions usage | PASS | Matches repository C# test policy. |
| Fail-before evidence | PASS | `fail-before-evidence.2026-03-25T00-00.md` records the pre-fix `NullReferenceException` reproductions. |
| Pass-after evidence | PASS | Current session QA evidence plus `coverage/coverage.cobertura.xml` show the changed test and production paths are exercised. |
| Coverage signal on changed paths | PASS | `QfcCollectionControllerTests.cs` and `QfcHomeControllerTests.cs` both show strong post-run coverage; `QfcHomeController.cs` improved to 78.71% line coverage. |

## Review conclusion

The `#97` null-guard change is technically correct and adequately tested. The reason this review remains a no-go is not the fix itself; it is the extra `.codex` / tooling content in the corrected diff, the summary/appendix mismatch in the PR-context artifacts, and the incomplete canonical evidence set in the feature folder.
