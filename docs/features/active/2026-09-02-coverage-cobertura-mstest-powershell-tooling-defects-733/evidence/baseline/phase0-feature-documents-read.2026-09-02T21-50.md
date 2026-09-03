# Phase 0 — Feature Documents and Target Files Read (P0-T2)

Timestamp: 2026-09-02T21-50

Task: [P0-T2]

Work Mode: full-bug
AC Source: FEATURE/spec.md (sole source)

FEATURE = docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733

## Files Read (explicit list)

Requirement documents:

1. docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/issue.md
2. docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/spec.md
3. docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/research/research-findings.2026-09-02T13-15.md

Production files:

4. scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
5. scripts/vscode/Invoke-MSTestWithCoverage.ps1
6. scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1
7. scripts/vscode/Invoke-MSTest.ps1

Test files:

8. tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
9. tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1
10. tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1

## Acceptance-Criteria Inventory (from spec.md, the sole AC source)

The spec.md `## Acceptance Criteria` section carries 8 unchecked checkbox items:

- AC1: Repro steps now produce the expected behavior in all documented environments.
- AC2: Regression test(s) added and passing (list file path and test name).
- AC3: Edge cases and invalid inputs are handled with correct errors or fallbacks.
- AC4: No unintended behavior changes outside the defined scope.
- AC5: Required logs/telemetry updated and validated (if applicable).
- AC6: Performance constraints met or explicitly waived with rationale.
- AC7: Full toolchain pass completed (format -> lint -> type-check -> test).
- AC8: Docs/config references updated to match the new behavior.

No AC item is checked off in this delegation. AC check-off is Phase 5 work
(P5-T6 through P5-T13) and is out of scope for the Phase 0 / Phase 1 run.

## Observations Relevant to Phase 1

- Get-CoberturaCoverageSummary (Invoke-MSTestWithCoverage.Helpers.ps1) currently accumulates
  per-class summaries inline in a nested loop over `//packages` child elements and then each
  class, with the rate/zero-denominator fallback expression written directly in the returned
  pscustomobject literal.
- Merge-CoberturaClassesByFilename ensures a `<methods>` node exists on the cloned primary node
  but appends no method from any non-primary group member, and never writes the enclosing
  `<package>` node's own line-rate / branch-rate attributes.
- The existing test "preserves the primary class methods subtree and every hits value when
  merging" in tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 currently asserts
  `$methodNodes.Count | Should -Be 2` is false: it asserts a count of 1 and only method 'M'. Its
  comment states it locks the decision not to merge or strip `<methods>`. P1-T4 reverses this
  assertion, which spec.md's Risks & Mitigations section approves explicitly.
- The existing test "computes the merged per-file line-rate from the merged rollup alone"
  operates on a fixture whose `<package>` node carries `line-rate="0" branch-rate="0"`, so a
  post-merge package-rate assertion is currently unsatisfied by any code path (finding 1).
- tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1's BeforeAll dot-sources only
  scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1, so any new sibling production file must
  be reachable transitively through that single dot-source (the requirement P1-T9 satisfies).

## Output Summary

All 10 documents and files read in full. Work Mode confirmed as full-bug from issue.md's
`- Work Mode: full-bug` marker; spec.md is the sole acceptance-criteria source and carries
8 AC items, all currently unchecked.
