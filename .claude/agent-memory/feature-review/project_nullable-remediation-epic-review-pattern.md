---
name: nullable-remediation-epic-review-pattern
description: recurring review pattern for the utilitiescs-nullable-remediation epic's 12 Wave children (#363 and siblings) — per-file #nullable enable, net481 constraints, epic-integration diff base
metadata:
  type: project
---

The `utilitiescs-nullable-remediation` epic ships ~12 Wave children (Wave 0 = #363
utilitiescs-nullable-extensions), each an annotation-only per-file `#nullable enable`
remediation of one directory cluster.

**Why:** the CI nullable gate (repaired by PR #361 to use `/t:Rebuild`) can only be enforced
after ~2131 pre-existing CS86xx diagnostics are remediated cluster-by-cluster under a per-file
opt-in (no project/solution `<Nullable>`).

**How to apply (recurring checks for each child):**
- Diff base is the EPIC INTEGRATION branch (e.g. `origin/epic/utilitiescs-nullable-remediation-integration`), NOT main. Re-confirm with `git merge-base`.
- PR-context artifacts are typically ABSENT in-session; hand-author `artifacts/pr_context.summary.txt` from `git diff --numstat` in the hook's `- <path> (+N/-N)` bullet format (space-free .cs paths) so Get-ChangedLanguageSet enumerates C#.
- net481 constraints to verify honored: NO nullable post-condition attributes ([NotNullWhen] etc.) and NO System.Diagnostics.CodeAnalysis polyfill (added-line grep = 0); no file splits; no struct->record conversions (net481 lacks IsExternalInit -> CS0518).
- The literal solution-wide `/t:Rebuild ... /p:TreatWarningsAsErrors=true` gate exits non-zero on PRE-EXISTING non-nullable warnings (CS0649 vendored SVGControl; CS0168/CS0618 UtilitiesCS). The definitive AC1 proof is the per-project `UtilitiesCS.csproj /t:Rebuild ... /p:BuildProjectReferences=false` build where CS86xx = 0. Don't treat the solution-wide non-zero exit as an AC1 failure.
- Operative coverage gate is changed-line non-regression (AC4), verified from feature-scope Cobertura (`lines-valid` identical baseline vs post-change proves zero new executable lines). Repo-wide line coverage sits ~83.78% — below the rules.md 85% floor but above CLAUDE.md's 80%; this is the pre-existing [[coverage-hook-forces-fail-below-floor-despite-exemption]] 80-vs-85 conflict, not introduced by these children.
- Canonical `artifacts/csharp/coverage.xml` is absent; per [[deletion-only-pr-absent-coverage-artifact-309]] mark C# canonical-artifact presence FAIL dispositioned non-blocking, verify from feature-evidence Cobertura instead.
- WinFormsExtensions `Clone<T>` overloads must stay non-null (downstream #374 contract) — check this whenever WinFormsExtensions.cs is in a child's diff.
