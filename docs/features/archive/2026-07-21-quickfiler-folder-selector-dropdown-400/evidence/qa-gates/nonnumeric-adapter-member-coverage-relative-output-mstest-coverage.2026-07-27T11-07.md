# P9-T57 Relative-Output Full Coverage Evidence

Timestamp: 2026-07-27T11:07:15.7128950Z to 2026-07-27T11:08:28.1400682Z

## Exact CoverageOutput Contract

`docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-nonnumeric-adapter-member-coverage-relative-output.2026-07-27T10-55.cobertura.xml`

This string is identical to the P9-T56 `CoverageOutput` value and was supplied to the wrapper without conversion to an absolute path.

## Owning Unbuffered Invocation

The owning runner used an outer tool timeout of `1,260,000 ms` and redirected stdout/stderr from process start:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-nonnumeric-adapter-member-coverage-relative-output.2026-07-27T10-55.cobertura.xml
```

- Terminal exit: `0`
- Timed out: `False`
- Runner PID: `245440`
- Terminated descendants: none
- Residual relevant coverage/test processes: `0`

Observed process tree:

`pwsh.exe` `245440` → `dotnet-coverage.exe` `269168` → `vstest.console.exe` `212244` → `testhost.exe` `264280` → `conhost.exe` `256496`.

The wrapper discovered eight Debug test assemblies: `QuickFiler.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`, and `VBFunctions.Test`.

## Test and Artifact Results

- Total tests: `6075`
- Passed: `6075`
- Failed: `0`
- Skipped: `0`
- Test duration: `56.1652 seconds`
- Cobertura: `coverage-nonnumeric-adapter-member-coverage-relative-output.2026-07-27T10-55.cobertura.xml`
- Cobertura SHA-256: `89DB6AC8BA9974515AF7D07A07B13F6BEAA08854DA645382005189F77971034C`
- Stdout SHA-256: `0BA0CFAF0E928932954DA13422DB01FEE7272512FD145DAECB3FBBB41C6EBB66`
- Stderr SHA-256: `E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855`
- Derived effective-settings artifact after completion: absent (wrapper cleanup complete)

## Configuration, Filter, and Coverage Gates

- Filter: `TestCategory!=LiveOutlook`
- `coverage.config` SHA-256 before and after: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`
- `scripts/vscode/TaskMaster.cli.runsettings` SHA-256 before and after: `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57`
- Repository line coverage: `92380 / 109252 = 84.5568%` (meets the 80% threshold and exceeds the P9-T34 84.5207% baseline)
- P9-T34 changed host-neutral member coverage is not regressed: `SetBridgeCoordinator` `13/13`; `AttachMessenger` `16/16`; `ThrowIfDisposed` `5/5`; `NavigateToDocument` `8/8`; `NavigateToDocumentCore` `7/7` (each 100%).

The unchanged coverage configuration retains the required exclusions and thresholds. P9-T59 performs the separate filename/source-range recomputation.
