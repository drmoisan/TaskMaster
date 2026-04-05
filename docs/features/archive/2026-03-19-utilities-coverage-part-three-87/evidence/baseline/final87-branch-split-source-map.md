# Branch Split Source Map — Final Clean Issue #87

Source: `.git/branch_analysis_issue87.txt` and `artifacts/research/20260326-issue87-unstacking-sequence-research.md`

## Clean Issue #87 Direct Cherry-Pick Commits

These commits are issue-87-only and can be cherry-picked directly:

1. `078fd77` — fix(sco-collection): restore items when loading or replacing lists
2. `3206593` — fix(serializable-list): capture writer delegate before async serialization
3. `cce7c5a` — bug: removed file system dependency from ScoCollection_Tests
4. `fff20c7` — fix(serializable-list): capture file-system seams before queued IO
5. `d65320b` — test(utilitiescs): add early UtilitiesCS coverage phases and baseline evidence
6. `2326734` — fix(InputBoxViewer): guard DpiAware against already-initialized WinForms
7. `5f90762` — test(utilitiescs): add SubjectMap and DfDeedle coverage tests
8. `27639bf` — test(utilitiescs): add helper and config coverage tests
9. `5afe10d` — test(utilitiescs): add EmailIntelligence and Threading coverage tests
10. `ee9e4d9` — test(utilitiescs): expand coverage for classifier and helper flows
11. `4009d1c` — test(utilitiescs): expand coverage for progress, store, stream, and classifiers
12. `5661a47` — fix(utilitiescs): harden coverage edge cases across UtilitiesCS
13. `4830958` — feat: final qc
14. `6e5d01d` — feat: code review and remediation plan 1st draft

## Issue-#87-Only Bootstrap Paths from Mixed Commits

The following mixed commits contain issue-87 files alongside non-87 files. Use `git restore --source <sha> -- <paths>` to extract only the issue-87 side:

- `ee92dd6` — Restore: `UtilitiesCS`, `UtilitiesCS.Test`, `docs/features/active/2026-03-19-utilities-coverage-part-three-87`
  (Excludes: `QuickFiler/Controllers/QfcHomeController.cs`, `missing-serializable-list.json`)
- `a8d24b2` — Restore: `UtilitiesCS`, `UtilitiesCS.Test`, `docs/features/active/2026-03-19-utilities-coverage-part-three-87`
  (Excludes: `change-plan.md`, `TaskMaster/TaskMaster.csproj`)
- `5fb07f7` — Restore: `UtilitiesCS`, `UtilitiesCS.Test`, `docs/features/active/2026-03-19-utilities-coverage-part-three-87`
  (Excludes: `docs/features/active/2026-03-13-utilities-coverage-65/`)
- `221e76f` — Restore: `UtilitiesCS.Test`, `docs/features/active/2026-03-19-utilities-coverage-part-three-87`
  (Excludes: `TaskMaster/Ribbon/RibbonExplorer.xml`)

## Commits NOT Included in Issue #87

These commits belong to other issues or residual work and must NOT be replayed:
- Issue #97: `a19ac86`, `ad4ae95`
- Issue #96: `bd8fc03`, `3b472b2`
- Merge commit: `c448819`
- Residual excluded work: `52742b8`, `4d5f476`, `60408b0`, `16d7d5d`, `0c9a045`, `66220df`, `ea0206e`
- Mixed non-87: `4634ac5` (contains `TaskMaster/AppGlobals/AppAutoFileObjects.cs`), `5a7831b`, `77546ac`, `dbdce98`, `4010818`, `c853a88`, `da0ed13`, `cc3009f`
