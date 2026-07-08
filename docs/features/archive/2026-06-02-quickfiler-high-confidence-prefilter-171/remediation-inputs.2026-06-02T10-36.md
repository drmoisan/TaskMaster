# Remediation Inputs — quickfiler-high-confidence-prefilter (Issue #171)

- Date: 2026-06-02T10-36
- Source audits:
  - `policy-audit.2026-06-02T10-36.md`
  - `code-review.2026-06-02T10-36.md`
  - `feature-audit.2026-06-02T10-36.md`
- Base: `development` @ `5e944344041b10becb98c56d358176fc9e7b8ee9`
- Head: `ae7eb670ee7738640cab2b41bc7226255224f7ca`

## Trigger

Remediation is triggered by 1 blocking finding plus supporting findings:

- BLOCKING: the mandatory canonical C# coverage artifact `artifacts/csharp/coverage.xml` is absent. Coverage verification is mandatory for every language with changed files (C# is the only changed language), and the agent verifies from the canonical artifact rather than re-running generation.

## Remediation-Required Findings

### R1 (BLOCKING) — Produce the canonical C# coverage artifact

- Problem: `artifacts/csharp/coverage.xml` does not exist. Coverage for Issue #171 exists only as human-readable text/markdown under `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/coverage/`.
- Expected behavior: A machine-readable C# coverage report exists at the canonical path `artifacts/csharp/coverage.xml` (Cobertura/JaCoCo-style XML with per-`LINE` counters) covering the in-scope test assemblies, so the feature-review coverage gate can verify repo-wide and per-file line coverage.
- Files / locations:
  - Output: `artifacts/csharp/coverage.xml`
  - Source data: vstest `/EnableCodeCoverage` output over `QuickFiler.Test.dll` and `UtilitiesCS.Test.dll`.
- Verification commands:
  - `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`
  - Convert the resulting `.coverage` to Cobertura XML and write it to `artifacts/csharp/coverage.xml` (e.g., via `Microsoft.CodeCoverage.Console.exe` or `dotnet-coverage merge ... -f cobertura -o artifacts/csharp/coverage.xml`).
  - Confirm the artifact parses and that `QfcHighConfidencePreFilter.cs` line coverage >= 90% and changed-file coverage shows no regression vs baseline.
- Acceptance: `artifacts/csharp/coverage.xml` exists, parses, and the new-file/changed-file/repo-wide gates are confirmed from it.

### R2 (SUPPORTING) — Resolve / verify repo-wide module coverage floor

- Problem: reported `QuickFiler.dll` module line coverage is 24.32%, below the documented 80% per-language repo-wide floor. Documented as pre-existing (COM/WinForms-bound controllers ~3-7% at baseline, unchanged), but unverified against the canonical artifact.
- Expected behavior: From `artifacts/csharp/coverage.xml`, confirm that the changed lines introduced by Issue #171 are covered or are legitimately COM/WinForms boundaries, and document the repo-wide figure with an explicit pre-existing-condition justification or a coverage improvement.
- Verification: parse `artifacts/csharp/coverage.xml`; compare per-file covered/total for the six touched files against `evidence/coverage/coverage-baseline-171.2026-06-02T14-05.txt`.
- Acceptance: no changed-line coverage regression; the repo-wide module figure is documented with verifiable evidence.

### R3 (LOW) — Revert or justify the `TaskMaster.csproj` reformat

- Problem: `TaskMaster/TaskMaster.csproj` was reformatted (multi-line attributes collapsed; trailing newline removed). This is unrelated to Issue #171 behavior and conflicts with the C# policy guidance against using formatters that rewrite `.csproj` files for legacy VSTO projects.
- Expected behavior: the `.csproj` is either restored to its base-branch form (keeping only any intentional, minimal #171-required change) or the reformat is explicitly justified; restore the trailing newline.
- Verification: `git diff development -- TaskMaster/TaskMaster.csproj` shows only intentional, justified changes; CSharpier no longer reports a new error introduced by this branch.
- Acceptance: the `.csproj` diff is minimal and justified; no trailing-newline regression.

## Do Not Do

- Do not re-run or weaken the existing Issue #171 unit tests to inflate coverage numbers.
- Do not add live Outlook COM, network I/O, or temporary files to unit tests.
- Do not broaden scope beyond Issue #171; do not refactor the oversized controllers as part of this remediation (pre-existing condition).
- Do not silently delete or relax the `[ExcludeFromCodeCoverage]` boundary on `FolderScoringService` to change the percentage.
- Do not modify acceptance-criteria text in `spec.md` / `user-story.md`.
- Do not edit policy documents.

## Handoff

- Authoritative spec for remediation: this file.
- Remediation plan target: `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/remediation-plan.2026-06-02T10-36.md`.
- Delegate plan creation to `atomic_planner` (or `csharp-atomic-planner`) with `${spec}` = this file and `${file}` = the remediation plan target, requiring a deterministic atomic plan with `[P#-T#]` IDs.
