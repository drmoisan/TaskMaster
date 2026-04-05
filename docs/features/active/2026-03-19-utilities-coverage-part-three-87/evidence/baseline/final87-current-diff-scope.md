# Current Mixed-Branch Diff Scope — Final Clean Issue #87

Timestamp: 2026-03-26T17:06Z
Command: git diff --name-status $(git merge-base HEAD origin/development) HEAD
EXIT_CODE: 0

Merge Base: 27f999cfe4a34cd6db03280f85ab13518d84c743

Output Summary: The mixed branch diff against origin/development contains 296 changed files across the following top-level paths:
- `.codex` (non-issue-87)
- `.github` (non-issue-87)
- `.merge_file_gXWn2v` (artifact, non-issue-87)
- `change-plan.md`
- `docs` (includes issue-87 feature folder plus issue-96/97 feature folders)
- `interop_inspect.txt` (artifact, non-issue-87)
- `missing-serializable-list.json` (non-issue-87)
- `QuickFiler` (non-issue-87, issue-96/97 scope)
- `QuickFiler.Test` (non-issue-87, issue-96/97 scope)
- `TaskMaster` (non-issue-87)
- `test-output.txt` (artifact, non-issue-87)
- `UtilitiesCS` (issue-87 scope)
- `UtilitiesCS.Test` (issue-87 scope)
- `UtilitiesSwordfish` (non-issue-87)

The clean issue #87 branch must contain only `UtilitiesCS`, `UtilitiesCS.Test`, and `docs/features/active/2026-03-19-utilities-coverage-part-three-87`.
