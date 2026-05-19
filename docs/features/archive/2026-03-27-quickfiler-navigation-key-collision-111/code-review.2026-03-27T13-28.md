# Code Review — quickfiler-navigation-key-collision-111 (2026-03-27T13-28)

- **Feature folder:** `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/`
- **Feature folder selection rule:** Used the user-specified active feature folder because it exists, matches issue suffix `-111`, and is the only local folder aligned to the requested QuickFiler duplicate-key review.
- **Branch:** `bug/quickfiler-navigation-key-collision-111` @ `40f176c1cd207a5a5971698d0e9ae762080de926`
- **Base branch:** `main` @ `cb6a6edd11590c245d36ccba16ca5c4c6732ce8f`
- **Work mode:** `minor-audit`
- **Supersedes:** `code-review.2026-03-27T13-11.md`

## 1. Executive summary

**What changed:**
- `QuickFiler/Controllers/KbdActions.cs` now separates stored-key identity from runtime keyboard matching by introducing `StoredKeyEquals()` and using exact equality for storage operations (`Add`, `Add(UClass)`, `Remove`) while leaving `ContainsKey`, `FilterKeys`, `Find`, and `FindIndex` on the existing `KeyEquals` path.
- `QuickFiler.Test/Controllers/KbdActionsTests.cs` adds three focused MSTest regressions covering distinct-key coexistence, exact-duplicate rejection, and preservation of live keyboard filtering semantics.
- `QuickFiler.Test/QuickFiler.Test.csproj` now compiles the new test file.
- The branch diff relative to `main` is now a single scoped commit plus the matching feature-folder evidence/docs. No `QfcCollectionController.cs` change was required.

**Top 3 risks:**

1. **Explicit `01` storage case is inferred rather than directly asserted (Low risk):**
   The issue text names `1`, `01`, and `10`. The implementation uses `EqualityComparer<string>.Default.Equals`, so `01` is covered by the same exact-equality rule as `10`, but there is no dedicated test for that literal.

2. **`Remove` changed with no direct regression test (Low risk):**
   The fix correctly switches `Remove` to exact stored-key equality, but the focused regression set exercises `Add` and lookup/filter behavior rather than `Remove` directly.

3. **Canonical `pr_context` artifacts are stale outside the feature folder (Informational):**
   Review correctness depended on live git commands rather than the stale shared `artifacts/pr_context.*` bundle because the collector command was unavailable in this tool environment.

**Go/No-Go recommendation:** **Go.** No blocker or major code-quality issue remains for the scoped QuickFiler duplicate-key fix.

## 2. Findings table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Minor | `QuickFiler.Test/Controllers/KbdActionsTests.cs` | `Add_WhenSourceAndStoredKeysAreDistinct_DoesNotTreatSubstringAsDuplicate` | The regression proves `10` and `1` coexist, but it does not explicitly cover `01`, which is named in the issue acceptance text. | Consider a follow-up assertion or dedicated test using `01` if the team wants literal parity with the issue wording. | Exact string equality already generalizes to `01`, so this is a completeness improvement, not a defect. | `issue.md` acceptance criterion 1; `KbdActions.cs` `StoredKeyEquals`; `KbdActionsTests.cs` |
| Minor | `QuickFiler/Controllers/KbdActions.cs` | `Remove(string sourceId, TKey key)` | `Remove` now uses exact equality, but the focused regression suite does not call `Remove` directly. | Consider a follow-up targeted test proving `Remove("Collection", "1")` does not remove a stored `"10"` action. | The implementation appears correct, but a direct test would harden the behavior against future regressions. | Diff hunk in `KbdActions.cs`; absence of a remove-specific test in `KbdActionsTests.cs` |
| Nit | Shared review process | `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt` | The shared PR-context bundle is stale for this branch/base pair. | Refresh the canonical bundle before opening the PR if the collector command becomes available. | Not blocking here because the live branch diff is a single clean commit and the working tree is clean. | Direct inspection plus live `git` baseline commands |

No Blockers. No Major findings.

## 3. Test quality audit

| Criterion | Status | Notes |
|---|---|---|
| Framework: MSTest | PASS | The new test file uses `[TestClass]` / `[TestMethod]` from MSTest. |
| Assertions: FluentAssertions | PASS | Assertions use FluentAssertions with explicit `because:` clauses and exception-type/message checks. |
| Mocking: Moq | N/A | These regressions are pure in-memory collection tests and do not require mocks. |
| Arrange-Act-Assert structure | PASS | All three tests use clear `// Arrange`, `// Act`, and `// Assert` sections. |
| Independence | PASS | Each test creates a fresh `KbdActions<string, KaStringAsync, Func<string, Task>>` instance. |
| Isolation | PASS | No Outlook, COM, filesystem, network, or external process dependency is involved in the tests themselves. |
| Determinism | PASS | The test inputs are fixed literals and deterministic collection operations. |
| Failure messages | PASS | `because:` messages explain the storage-vs-filtering distinction clearly. |
| Fail-before evidence | PASS | `evidence/regression-testing/p1-t2-kbdactions-distinct-keys.2026-03-27T13-01.md` records the pre-fix duplicate-key failure and the repository-script fallback used to surface it. |
| Pass-after evidence | PASS | `evidence/qa-gates/p2-t4-tests-with-coverage.2026-03-27T13-08.md` records a successful coverage-enabled MSTest run after the fix. |
| Coverage for changed behavior | PASS | The regressions directly exercise distinct-key add, exact-duplicate add rejection, and retained `FilterKeys`/`ContainsKey` behavior. |

## 4. Typed Python audit

**N/A** — no Python files changed in this feature branch.

## 5. Security / correctness checks

| Check | Status | Notes |
|---|---|---|
| No secrets in code | PASS | The changed code and feature docs contain no credentials or sensitive material. |
| No unsafe subprocess usage | PASS | The feature adds no process-launching behavior. |
| Input validation at boundaries | N/A | `KbdActions` is an internal helper for keyboard registrations rather than a user-input boundary. |
| Storage identity vs runtime matching contract | PASS | `StoredKeyEquals()` is used only for storage operations, while the keyboard filtering path continues to use `KeyEquals`. |
| Public API stability | PASS | No public method signatures changed; behavior is refined internally. |
| Scope discipline | PASS | `QfcCollectionController.cs` was intentionally left unchanged because the issue investigation confirmed compatibility there was already sufficient. |

## 6. Research log

None. The review relied on repository-local policies, direct git diff evidence, feature-folder artifacts, and fresh QA execution.

## 7. Review conclusion

**Go for PR review.**

The implementation is minimal, correctly targeted, and well-supported by focused regressions plus a green QA loop. The remaining observations are completeness improvements only and do not justify another remediation cycle.