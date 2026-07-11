# Code Quality Review — swordfish-interface-project-teardown (#308), epic child F5

- **Timestamp:** 2026-07-11T13-36
- **Base / head:** `1b65f7a7` → `0ec111b2`
- **Scope:** full branch diff (90 files). Non-documentation code changes are: 3 interface deletions, 1 dead-method removal, 6 comment/XML-doc rewordings, 3 test-file deletions, 9 `ProjectReference` removals, 2 `.sln` project/config removals, and the wholesale `git rm -r` of two vendored project folders.

## Executive Summary

The change set is a disciplined structural teardown with no new executable production code. Every
non-deletion `.cs` edit is either the removal of a never-called dead method
(`QfcExplorerController.UpdateForMove`) or a comment/XML-doc reword; no production behavior is
altered. Project-file edits are pure removals with no substituted build elements, and XML integrity
was spot-checked. Design-principle adherence is good: the disposition is removal (not migration) of
interfaces with zero surviving implementers/consumers, which is the simplest correct outcome. No
best-practice defect rises to blocking. The single observation is an evidence-completeness gap
(raw Cobertura XML not committed), which is a documentation matter, not a code defect.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | QuickFiler/Controllers/QfcExplorerController.cs | removed lines ~271–281 | The dead `private static UpdateForMove(...)` (sole `ISubjectMapSco` reference, no call site) is removed. Correct and clean; leaves no dangling symbol. | None. | Removing verifiably dead code reduces surface and satisfies AC-6; build is green and repo-wide grep confirms no residual `ISubjectMapSco`. | policy-audit re-verification greps; `evidence/qa-gates/finalqc-build-green.md` |
| Info | UtilitiesCS/.../ConcurrentObservableCollection.cs; SloStack.cs; AppAutoFileObjects.cs; 3 test files | XML-doc / inline comments | Six comment-only rewordings ("Swordfish-free" → "vendored-dependency-free"; dropped dangling `<c>Swordfish.NET...</c>` clause). No code/behavior change. | None; keep the explanatory intent as reworded. | Necessary to satisfy AC-13's literal code-glob zero-match; verified comment-only in the diff. | branch diff; `evidence/regression-testing/repo-wide-swordfish-zero.md` |
| Info | UtilitiesCS.csproj + 8 sibling csprojs | `<ProjectReference>` blocks | Nine `UtilitiesSwordfish.NET.General.csproj` references removed; `.csproj` diff additions are empty (pure removals). Preceding/closing ItemGroup elements intact. | None. | Pure removals with confirmed XML integrity avoid dangling references and broken builds. | `evidence/regression-testing/wi2-projectreference-zero.md`; diff-additions filter empty |
| Info | TaskMaster.sln | project declarations + config rows | Both vendored project declarations and their `ProjectConfigurationPlatforms` rows removed; no orphaned configuration rows for either GUID. | None. | Clean solution teardown keeps the solution loadable; grep for both GUIDs returns zero. | `evidence/regression-testing/wi3-*.md`; re-verification grep |
| Info | UtilitiesCS.Test test suite | 3 removed test files | WI-4 test removals target types deleted wholesale; surviving `ConcurrentObservableCollection_Tests.cs` retains sender-identity coverage. | Track lock-recursion re-expression via issue #317 (already filed). | Removing tests for deleted types is correct; the one behavioral gap (lock-recursion) is correctly deferred, not silently dropped. | `evidence/other/f2-regression-coverage-confirmation.md`; issue #317 OPEN |
| Low | evidence/ (feature folder) | coverage artifacts | Raw `*.cobertura.xml` / canonical `artifacts/csharp/coverage.xml` not committed; only derived Markdown coverage summaries are present. | Commit the Cobertura XML alongside future coverage summaries for independent auditability. | Independent re-parse of coverage numbers was not possible; verdict relied on Markdown evidence plus independent build/grep re-verification. Non-blocking documentation gap. | absence check in this review |

## Best-Practices Assessment

- **Simplicity first (CLAUDE.md §1, general-code-change):** Removal over migration is the simplest
  correct disposition given zero surviving consumers/implementers. Compliant.
- **Separation of concerns / API stability:** No public API break — the removed interfaces had no
  first-party implementers or consumers (build green confirms). Compliant.
- **Error handling / logging:** No production logic paths changed. Not applicable to this teardown.
- **File-size limit (500 lines):** No file gained meaningful lines; all edits are deletions or
  single-line comment rewords. Compliant.
- **Naming / comments:** The reworded comments remain descriptive and explain "why" (the type was a
  former vendored dependency), preserving intent while removing dangling code references. Compliant.
- **Tests (general + C# unit-test policy):** Test removals are justified by wholesale type deletion;
  no surviving production behavior loses its only test. The lock-recursion behavioral gap is tracked
  in issue #317 rather than authored against a type F5 does not own, matching the spec Non-Goals.

## Verdict

Code quality: acceptable. Blocking findings: **0**. One Low, non-blocking evidence-completeness
recommendation (commit raw Cobertura XML).
