# P0-T9 Baseline line counts

Timestamp: 2026-10-02T03-08
Command: Grep pattern `^` count mode on each file (CMD-LINECOUNT); Glob `**/BindingRedirectVerification*` under `scripts/dependencies` and under `tests/scripts/dependencies`; Glob `**/*` under each of the two folders to confirm the Glob tool is not blind there (paths rooted at `<execution-worktree-root>`)
EXIT_CODE: 0

Line counts:

```text
LINECOUNT scripts/dependencies/PackageGraph.psm1 = 465 (expected 465)
LINECOUNT scripts/dependencies/ProjectConsistency.psm1 = 381 (expected 381)
LINECOUNT scripts/dependencies/ConsistencyVerifier.psm1 = 499 (expected 499)
LINECOUNT tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1 = 152 (expected 152)
```

No LINECOUNT-DRIFT.

New-file absence:

- Glob `**/BindingRedirectVerification*` under `scripts/dependencies`: no files found.
- Glob `**/BindingRedirectVerification*` under `tests/scripts/dependencies`: no files found.
- Control: Glob `**/*` under `scripts/dependencies` lists 6 files (AnalyzerItemRepair.psm1, ConsistencyVerifier.psm1, PackageCompatibility.psm1, PackageGraph.psm1, ProjectConsistency.psm1, Repair-PackageManifestConsistency.ps1); under `tests/scripts/dependencies` it lists 8 files (AnalyzerItemRepair, ConsistencyVerifier, DependabotConfig, PackageCompatibility, PackageGraph, ProjectConsistency, Repair-PackageManifestConsistency, RepositoryTreeConsistency, each with the `.Tests.ps1` suffix). The CMD-HASHSET population is therefore 14 files.

Acceptance: the four counts equal the stated figures and both new-file Globs return nothing; no PREEXISTING-FILE stop.

Output Summary: PackageGraph 465, ProjectConsistency 381, ConsistencyVerifier 499, RepositoryTreeConsistency.Tests 152; neither new file exists.
