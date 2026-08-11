# poshqc-test-coverage-capture-records-zero (Issue #536)

- Date captured: 2026-08-10
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/poshqc-test-coverage-capture-records-zero/ (Issue #536)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #536
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/536
- Last Updated: 2026-08-11
## Summary

The bundled `mcp__drm-copilot__run_poshqc_test` coverage capture writes `artifacts/pester/powershell-coverage.xml` with **zero covered lines for every file in the repository**, making the canonical repo-wide PowerShell coverage artifact unusable as a gate input.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (PowerShell / Pester 5)
- Command/flags used: `mcp__drm-copilot__run_poshqc_test` with `scan_folders = ["scripts/vscode", "tests/scripts/vscode"]`
- Data source or fixture: `artifacts/pester/powershell-coverage.xml`, written 2026-08-10 22:58

## Steps to Reproduce

1. Run `mcp__drm-copilot__run_poshqc_test` against a workspace whose Pester suite passes and demonstrably exercises production code.
2. Read the generated `artifacts/pester/powershell-coverage.xml`.
3. Aggregate its JaCoCo `<counter>` elements by type.

## Expected Behavior

The emitted JaCoCo counters should reflect the lines actually executed by the passing suite. For the run in question, `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` was exercised by 19 passing tests and independently measured at 183/202 = 90.59% LINE coverage by a direct `Invoke-Pester` capture at the same head.

## Actual Behavior

Every counter records zero covered. Aggregated over all 1227 `<counter>` elements in the file:

```
CLASS        covered 0  / missed 168
INSTRUCTION  covered 0  / missed 21800
LINE         covered 0  / missed 16075
METHOD       covered 0  / missed 1445
```

A reading of literal zero repo-wide, for a run whose suite passed and whose direct-Pester capture at the same head reports 90.59% on the primary file, is a measurement defect in the capture rather than a property of the code.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: the aggregation above, reproduced from `artifacts/pester/powershell-coverage.xml` (gitignored producer output via `.gitignore:57`, so it is not committed).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

The canonical repo-wide PowerShell coverage artifact reads 0.00% against an `>= 85%` floor. Any gate that consumes it fails unconditionally, and any agent that trusts it will either report a false blocker or route around the canonical artifact. Discovered during feature review of #441 (recorded there as finding NF-2 and dispositioned non-blocking for that bugfix, because the defect is pre-existing and #441 changes no coverage-capture tooling, hook, or configuration).

## Suspected Cause / Notes

Likely a coverage-path or `-CodeCoverage` scoping mismatch inside the bundled PoshQC test invocation: the instrumented file set appears to be resolved independently of the files the tests actually load, so nothing the suite executes is attributed. Compare the bundled invocation against the direct `Invoke-Pester` capture used as evidence in #441, which produces correct non-zero counters over the same tree.

Candidate to fold into sibling feature #512 (toolchain gate fidelity) rather than fix standalone.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: a check asserting that a known-covered file reports non-zero LINE covered after a bundled `run_poshqc_test` run.
- [x] Integration scenario to retest: run the bundled tool and a direct `Invoke-Pester -CodeCoverage` capture over the same tree and assert the two agree within tolerance.
- [x] Manual verification notes: aggregate the JaCoCo counters and confirm covered > 0 repo-wide.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
