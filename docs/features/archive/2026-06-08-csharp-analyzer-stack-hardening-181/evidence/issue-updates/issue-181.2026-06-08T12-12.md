# Issue #181 — Status Update Mirror

Timestamp: 2026-06-08T13-41
PostedAs: unknown

POSTING NOTE: This is a local mirror of the intended issue-status update. The executor does not author the PR or post to GitHub; final commit and any GitHub posting are deferred to the orchestrator (per the execution directive: "leave the final commit to the orchestrator; do not commit"). PostedAs is recorded as `unknown` because no GitHub post was performed by the executor.

## Intended update text

The csharp-analyzer-stack-hardening work (Issue #181, revised plan v2.0) is implemented and verified locally.

Acceptance criteria AC1–AC8 are all PASS (see `evidence/qa-gates/acceptance-summary.2026-06-08T12-12.md`). The issue.md `## Acceptance Criteria` checkboxes have been updated to `[x]` to reflect verified status.

Summary of delivered change:
- Adopted a FIVE-analyzer static-analysis stack (Meziantou.Analyzer, SonarAnalyzer.CSharp, Roslynator.Analyzers, AsyncFixer, Microsoft.CodeAnalysis.BannedApiAnalyzers), wired first-party only across the 15 first-party projects; the 4 vendored projects are excluded.
- SecurityCodeScan.VS2019 was evaluated and DEFERRED (documented decision, not silent omission): version 5.6.7 is incompatible with this repo's Roslyn 5.6 (VS18) analyzer loader, emitting CS8032 (FileNotFoundException for YamlDotNet 11.0.0.0) which cannot be set to suggestion via .editorconfig and breaks the protected nullable gate under TreatWarningsAsErrors. No CS8032 suppression and no substitute security analyzer were introduced. The deferral is recorded in `.claude/rules/csharp.md`.
- BannedApiAnalyzers + repo-root `BannedSymbols.txt` enforce the 5 banned symbols (DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep, Task.Delay); RS0030 held at `suggestion` for initial rollout (verified to fire: 60 RS0030 diagnostics in UtilitiesCS when temporarily elevated for verification).
- `.editorconfig` carries the 5-analyzer severities (all `suggestion`) plus naming preferences; SCS severities removed.
- `.claude/rules/csharp.md` adds TimeProvider/FakeTimeProvider seam guidance (guidance-only, no runtime change), documents the 5-analyzer mechanism, and records the SecurityCodeScan deferral — while retaining MSTest/Moq, 80/90 line coverage, and msbuild + vstest commands.

Verification (local toolchain, same commands as CI):
- CSharpier format: at Phase 0 baseline (only the 1 pre-existing unrelated `.cs` file remains flagged; all in-scope project files pass).
- nuget restore: EXIT 0.
- Analyzer/code-style build: EXIT 0, 0 errors, 0 CS8032.
- Nullable TreatWarningsAsErrors build: 84 errors = Phase 0 baseline (all in the two vendored projects), 0 first-party errors, 0 CS8032 — NO REGRESSION.
- MSTest with coverage: 4054 passed; 7 failed (all in the known-flaky wall-clock-timer family, matching the baseline; unrelated to analyzer adoption); coverage 58.99% (>= 58.89% baseline). Canonical Cobertura at `artifacts/csharp/coverage.xml`.

## Worktree state
The worktree contains all intended, saved edits. Per the directive, the executor did NOT commit; the final commit (and any GitHub issue/PR posting) is the orchestrator's step. `git status` therefore shows the intended modified/added files (15 `.csproj`, 15 `packages.config`, `.editorconfig`, `BannedSymbols.txt`, `.claude/rules/csharp.md`, plan + evidence artifacts under the feature folder, and `artifacts/csharp/coverage.xml`).
