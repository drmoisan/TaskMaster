# P4-T9 — AC13 Check-Off

Timestamp: 2026-09-13T06-18
Task: [P4-T9]

Criterion checked off: AC13 — Results directory is beneath the ignored coverage tree. Exactly one
criterion was checked off by this task.

## Supporting test results

Both entry points now have an abstract-syntax-tree test that reads the `ResultsDirectory` parameter
default rather than invoking the entry point, and both are recorded as passed:

- Coverage entry point: `defaults the coverage entry-point results directory beneath the repository
  coverage directory`, from P3-T6, asserting the default text is `'coverage\test-results'`.
- Plain entry point: `defaults the entry-point results directory beneath the repository coverage
  directory`, from P4-T5, asserting the same default text.

Both were re-observed passing in this phase's whole-folder run, recorded in
`evidence/qa-gates/p4-t6-phase4-toolchain.md` as 131 passed, 0 failed, 0 skipped.

## Ignore file unchanged

Command: `git diff --name-status refs/base-anchor-873`
EXIT_CODE: 0

The anchored name-listing diff enumerates 61 entries. Tracked source and test changes:

```
A	scripts/vscode/Invoke-MSTest.TrxSummary.ps1
M	scripts/vscode/Invoke-MSTest.ps1
M	scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
A	scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1
M	scripts/vscode/Invoke-MSTestWithCoverage.ps1
M	tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1
M	tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
A	tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1
M	tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
A	tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1
A	tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1
```

The remaining 50 entries are this feature folder's own documents and evidence artifacts plus one rename
of the potential-feature entry into the promoted subdirectory.

REPOSITORY_IGNORE_FILE_ENTRY_COUNT: 0. No entry for `.gitignore` appears in the diff.

Command: `git status --porcelain --untracked-files=all`
EXIT_CODE: 0

```
 M docs/features/active/<feature-folder>/plan.2026-09-12T10-26.md
 M scripts/vscode/Invoke-MSTest.ps1
 M tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
?? docs/features/active/<feature-folder>/evidence/qa-gates/p4-t1-batch-open.md
?? docs/features/active/<feature-folder>/evidence/qa-gates/p4-t6-phase4-toolchain.md
?? docs/features/active/<feature-folder>/evidence/qa-gates/p4-t7-line-counts.md
?? tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1
```

The porcelain status carries no entry for the repository ignore file either. The pairing is required
because a name-listing diff enumerates tracked changes only and cannot report an untracked addition, so
neither observation alone would close the criterion.

The Write Set is not offered as evidence for this. The default needs no ignore-file change because the
repository already ignores the coverage tree, and the results directory is placed beneath it precisely so
that no ignore-file change is needed.

Output Summary: AC13's checkbox is marked. The abstract-syntax-tree default tests for both entry points
are recorded as passed, and the anchored name-listing diff paired with the porcelain status in this same
task show zero entries for the repository ignore file.
