# P0-T2: Current Mixed-Branch Diff Scope

Timestamp: 2026-03-26T16:02

Command: git diff --name-status $(git merge-base HEAD origin/development) HEAD

EXIT_CODE: 0

Output Summary:
The current mixed-branch diff against origin/development contains paths across the following top-level scopes:

**Residual excluded work (in scope for this pass):**
- `.codex/**` — agents, skills, prompts, web-setup files (A/M)
- `.github/**` — agents, skills.zip, codex-web-setup-test.yml (A/M)
- `QuickFiler/**` — controller modifications (M)
- `QuickFiler.Test/**` — new and modified controller tests (A/M)
- `TaskMaster/**` — AppGlobals/AppAutoFileObjects.cs, Ribbon/RibbonExplorer.xml, TaskMaster.csproj (M)
- `UtilitiesSwordfish/**` — ConcurrentObservableBase.cs (M)
- `missing-serializable-list.json` (A)

**Issue #87 work (NOT in scope for this pass):**
- `UtilitiesCS/**` — production code modifications (M/D)
- `UtilitiesCS.Test/**` — new and modified test files (A/M)
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/**` — feature docs, evidence, plans (A/M/R)

**Issue #96 work (NOT in scope for this pass):**
- `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/**` (A)

**Issue #97 work (NOT in scope for this pass):**
- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/**` (A)

**Other excluded paths:**
- `docs/features/active/2026-03-13-utilities-coverage-65/**` — previous feature docs (M)
- `docs/features/potential/**` — potential feature entries (A)
- `change-plan.md` (A)
- deleted files: `.merge_file_gXWn2v`, `interop_inspect.txt`, `test-output.txt` (D)
