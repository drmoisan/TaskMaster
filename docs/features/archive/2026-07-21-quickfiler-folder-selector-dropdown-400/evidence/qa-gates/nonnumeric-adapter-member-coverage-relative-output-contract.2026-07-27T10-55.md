# P9-T56 Relative CoverageOutput Contract

Timestamp: 2026-07-27T11:04:06.6580946Z to 2026-07-27T11:04:09.9206873Z

CoverageOutput: `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-nonnumeric-adapter-member-coverage-relative-output.2026-07-27T10-55.cobertura.xml`

The argument is workspace-relative and non-rooted: no drive, leading separator, or `..` segment. `Join-Path $repoRoot $CoverageOutput` resolved to:

`C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\qa-gates\coverage-nonnumeric-adapter-member-coverage-relative-output.2026-07-27T10-55.cobertura.xml`

The resolved target is under the workspace root.

## Command-Shape Gate

```powershell
scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-nonnumeric-adapter-member-coverage-relative-output.2026-07-27T10-55.cobertura.xml -NoExecute
```

- Terminal exit: `0`
- Discovered assemblies: `8`
- Observed wrapper process: `pwsh.exe` PID `237168`
- Observed coverage/test child processes: none
- Stdout: `nonnumeric-adapter-member-coverage-relative-output-contract.2026-07-27T10-55.stdout.txt`, SHA-256 `62315CA643768F6735D3F6C3D1740764B39F1ECDFDF471E190FDB1362CF9F5CD`
- Stderr: `nonnumeric-adapter-member-coverage-relative-output-contract.2026-07-27T10-55.stderr.txt`, SHA-256 `E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855`
- Cobertura output after command: absent
- Derived effective-settings output after command: absent
- `coverage.config` SHA-256 before and after: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`
- `scripts/vscode/TaskMaster.cli.runsettings` SHA-256 before and after: `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57`
