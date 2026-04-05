# Remediation Inputs — outlook-folder-wrapper-tests-82 (2026-03-19T17-56)

## Required fixes

1. **Isolate the `#82` branch scope to the documented folder-wrapper feature changes**
   - **Files / locations:** Branch-level diff relative to `development`, as captured in `artifacts/pr_context.appendix.txt` under `Commits in range`, `Changed files (name-status)`, and `Diff shortstat`
   - **Expected behavior:** The branch should contain only the folder-wrapper feature files documented in the active feature folder: the two production seam files, the folder test files, the `UtilitiesCS.Test.csproj` compile-include update, and the scoped feature docs/evidence
   - **Acceptance criteria:** A refreshed `artifacts/pr_context.appendix.txt` for the cleaned branch no longer reports hundreds of unrelated files or ancestry from `feature/outlook-objects-test-coverage-67`
   - **Verification commands / tasks:**
     - Refresh canonical PR context artifacts for the cleaned branch vs `origin/development`
     - `git diff --name-status origin/development...HEAD`
     - `git log --oneline origin/development..HEAD`

2. **Resolve or explicitly approve the `MSB3277` dependency-conflict warning in `UtilitiesCS.Test`**
   - **Files / locations:** `UtilitiesCS.Test/UtilitiesCS.Test.csproj` and its transitive package graph, as surfaced by the live review-time MSBuild output
   - **Expected behavior:** Analyzer and nullable builds should either be warning-free for this conflict or have a documented, repo-approved exception explaining why the warning is intentionally accepted
   - **Acceptance criteria:** Review-time analyzer and nullable build output no longer report `MSB3277` for `System.Reflection.Metadata`, or the warning is explicitly documented as an accepted exception in the relevant review / feature evidence
   - **Verification commands / tasks:**
     - `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
     - `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`

## Do not do

- Do **not** widen the production scope beyond the documented folder-wrapper feature.
- Do **not** weaken the feature acceptance criteria or coverage gate to make the branch look smaller than it is.
- Do **not** silently skip `pr_context` refresh after branch cleanup.
- Do **not** broaden the non-public seam pattern into public API changes.

## Acceptance criteria status

All scoped feature acceptance criteria in `spec.md` and `user-story.md` are currently met.

The remediation here is about **PR readiness relative to `development`**, specifically:

- narrowing the branch diff so it matches the documented `#82` feature scope, and
- cleaning up the remaining build-warning signal in `UtilitiesCS.Test`.

## Minimum changes required

- Repackage the folder-wrapper implementation onto a clean branch from `development`.
- Refresh canonical PR context artifacts after that cleanup.
- Re-run the repo validation loop and refresh the audit artifacts against the cleaned branch.
