# Remediation Inputs — utilitiescs-test-cs2002-duplicate-compile-entry-394

- **Issue:** #394
- **Branch:** `bug/utilitiescs-test-cs2002-duplicate-compile-entry-394`
- **Timestamp:** 2026-08-10T23-45
- **Source audits:** `policy-audit.2026-08-10T23-45.md` (Section 4, Section 9), `code-review.2026-08-10T23-45.md` (Findings Table, row 1)

## Remediation-Required Finding

**Finding:** `docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/evidence/baseline/duplicate-sweep.ps1` is a newly committed PowerShell source file (confirmed via `git diff --numstat`: 27 insertions, new file, versus `origin/epic/build-ci-coverage-gate-fidelity-integration`). Its presence places PowerShell in this branch's changed-language set. No PowerShell coverage artifact (`artifacts/pester/powershell-coverage.xml`) exists in the repository, and no Pester test, PoshQC format run, or PSScriptAnalyzer run has been executed against this file. Per the mandatory Coverage Verification procedure, a language with changed files and no coverage artifact must be flagged FAIL: "coverage artifact absent for PowerShell; coverage verification is mandatory for all languages with changed files."

**Severity:** Blocking.

**Root cause:** The atomic plan (`plan.2026-08-10T14-09.md`, task P0-T8) required only that the sweep script's *output* be captured in `evidence/baseline/duplicate-sweep.<timestamp>.md`. The executor additionally chose to commit the script itself "for reproducibility" (per that same artifact's own text), which was not required by the plan and introduces an unplanned PowerShell-toolchain obligation.

## Recommended Remediation (Preferred)

Remove `evidence/baseline/duplicate-sweep.ps1` from the branch. This is the proportionate fix because:

- The script's full logic and full raw output are already durably captured, verbatim, in the accompanying `evidence/baseline/duplicate-sweep.2026-08-10T22-31.md` narrative artifact (which quotes the script's approach and its complete output).
- The plan never required the script itself to be committed — only its output.
- Removing it eliminates the PowerShell changed-file signal entirely, resolving the coverage-verification gap without requiring new Pester tests, PoshQC runs, or coverage tooling for a one-off audit helper, consistent with `general-code-change.md`'s allowance for "temporary throwaway scripts created and deleted within an agent session."
- This is a documentation/evidence-tree-only change; it does not touch `UtilitiesCS.Test/UtilitiesCS.Test.csproj` or any other file already reviewed and found compliant, so no other finding in `policy-audit.2026-08-10T23-45.md`, `code-review.2026-08-10T23-45.md`, or `feature-audit.2026-08-10T23-45.md` needs to be revisited as a result.

## Alternative Remediation (If Retention Is Desired)

If the script must remain in the repository for future reuse:

1. Run PoshQC format (`mcp__drm-copilot__run_poshqc_format`) and PSScriptAnalyzer (`mcp__drm-copilot__run_poshqc_analyze`) against the file and resolve any findings.
2. Refactor the script into an advanced function with `[CmdletBinding()]` and explicit parameters (e.g., `-CsprojPath`, `-PackagesConfigPath`) per `.claude/rules/powershell.md`'s "prefer advanced functions" standard, and add `try`/`catch` around the `Get-Content -Raw` / `[xml]` casts so a missing or malformed input file produces a clear error rather than an unhandled exception.
3. Add a Pester test (`*.Tests.ps1`, mirroring the production path per `general-unit-test.md`'s Test File Location rule) exercising the refactored function against fixture XML content (no temporary files; use in-memory XML strings).
4. Generate `artifacts/pester/powershell-coverage.xml` and confirm the file meets the repository's uniform coverage floor (>= 85% line, >= 75% branch) as a new-code file (>= 90% line per the new-code tier in `general-unit-test.md`).
5. Re-run `policy-audit`'s Coverage Verification section for PowerShell and confirm the FAIL verdict converts to PASS with a genuine, non-null coverage percentage cited.

## Non-Blocking Follow-Ups (Not Remediation-Required, Recorded for Completeness)

- `spec.md`'s Root Cause Analysis "Duplicate Sweep Result" table states `Analyzer`=9 (unqualified), `Reference`~=114, `packages.config`~=99; the captured sweep evidence reports `Analyzer`=11, `Reference`=126, `packages.config`=105. Recommend correcting `spec.md`'s figures for accuracy (does not change the duplicate-finding conclusion; not blocking).
- No dedicated `evidence/qa-gates/analyzer-not-applicable.*.md` artifact exists alongside the CSharpier and nullable-gate "not applicable" determinations. Recommend adding one for evidentiary parity (not blocking; see `feature-audit.2026-08-10T23-45.md` AC7 notes).

## Handoff

This remediation is scoped to a single evidence-tree file deletion (or, alternatively, a small PowerShell hardening task) and does not require re-opening the underlying CS2002 fix, which is fully verified and compliant. Route to `atomic-planner`/`atomic-executor` (or an equivalent lightweight direct edit) per `remediation-handoff-atomic-planner` if a formal remediation plan is required; given the small size (one file deletion), a direct edit followed by a re-run of `evidence/qa-gates/diff-scope.*.md`-style verification may be sufficient.
