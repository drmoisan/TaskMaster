# Acceptance-Criteria Check-off Summary (P5-T9)

- **Timestamp:** 2026-07-11T13-32
- **Feature:** swordfish-interface-project-teardown (#308), F5
- **AC sources (full-feature mode):** `spec.md` AND `user-story.md` — all 16 marked `[x]` in both.

| AC | Description | Evidence artifact |
|---|---|---|
| AC-1 | Zero production `Swordfish` `*.cs` refs (else halt) | evidence/regression-testing/wi0-preflight-precondition.md |
| AC-2 | `TraceUtility.cs` has no `UtilitiesSwordfish.NET.*` literal | evidence/regression-testing/wi0-preflight-precondition.md |
| AC-3 | `IScoCollection.cs` removed | evidence/regression-testing/wi1-interface-symbol-zero.md |
| AC-4 | `IScoCollection2.cs` removed | evidence/regression-testing/wi1-interface-symbol-zero.md |
| AC-5 | `ISubjectMapSco.cs` removed | evidence/regression-testing/wi1-interface-symbol-zero.md |
| AC-6 | Dead `UpdateForMove` removed, no dangling symbol | evidence/regression-testing/wi1-interface-symbol-zero.md |
| AC-7 | All nine ProjectReferences removed (+ stale evidence) | evidence/regression-testing/wi2-stale-reference-search.md, wi2-projectreference-zero.md |
| AC-8 | `.sln` Project declarations removed | evidence/regression-testing/wi3-sln-declarations-removed.md |
| AC-9 | `.sln` config rows removed | evidence/regression-testing/wi3-sln-config-rows-removed.md |
| AC-10 | Both project folders deleted | evidence/regression-testing/wi3-solution-folder-teardown.md |
| AC-11 | Three direct-Swordfish test files removed | evidence/regression-testing/wi4-test-swordfish-zero.md |
| AC-12 | F2 sender/lock-recursion coverage confirmed (issue if absent) | evidence/other/f2-regression-coverage-confirmation.md (issue #317) |
| AC-13 | Repo-wide `Swordfish` code-glob search returns zero | evidence/regression-testing/repo-wide-swordfish-zero.md |
| AC-14 | Solution builds green, no unresolved refs | evidence/qa-gates/finalqc-build-green.md |
| AC-15 | Full toolchain green single pass | evidence/qa-gates/finalqc-single-pass.md |
| AC-16 | Coverage thresholds / no first-party regression | evidence/qa-gates/coverage-delta-verification.md |

## Status

- **Total AC items:** 16
- **Checked off (delivered):** 16
- **Remaining (unchecked):** 0

## Notes

- AC-12: sender-identity coverage present in surviving `ConcurrentObservableCollection_Tests.cs`;
  lock-recursion behavioral coverage absent after the WI-4 removal, new issue
  https://github.com/drmoisan/TaskMaster/issues/317 raised (F5 does not author the coverage).
- AC-15: format/analyzer/nullable green in a single pass; the MSTest step's only failures are 22
  pre-existing environmental Deedle "eng" language-model failures, byte-identical to the baseline and
  unrelated to F5 (F5 introduces zero new failures).
