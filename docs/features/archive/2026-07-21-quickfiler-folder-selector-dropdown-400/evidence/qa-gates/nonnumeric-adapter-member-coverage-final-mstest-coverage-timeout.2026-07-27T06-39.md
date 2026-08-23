# P9-T45 One-Shot Coverage Timeout Failure Evidence

Timestamp: 2026-07-27T06:39:00-04:00

## Single Invocation

```powershell
$coverageOutput = 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-nonnumeric-adapter-member-coverage-final.2026-07-27T06-33.cobertura.xml'
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput $coverageOutput
```

The command was invoked once. Its external reader timed out after `184.2` seconds while the inner wrapper remained active. No retry was started.

## Preconditions and Integrity

- `coverage.config` SHA-256 before and after: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`
- Root-level `effective-coverage.config` count before invocation: `0`
- No diff in `coverage.config` or `TaskMaster.cli.runsettings` before invocation
- Canonical Cobertura output after termination: absent

## Live Invocation and Evidence Gap

Before cleanup, the verified invocation tree was:

1. Wrapper `pwsh.exe` PID `68864` (`Invoke-MSTestWithCoverage.ps1`)
2. `dotnet-coverage.exe` PID `270696`
3. `vstest.console.exe` PID `248344` over all eight Debug test assemblies with `TestCategory!=LiveOutlook`
4. `testhost.exe` PID `204948`
5. Child `conhost.exe` PID `276836`

At the terminal assessment, no Cobertura file had been produced, the outer reader was gone, and the wrapper did not retain a terminal exit status, full test totals, TRX, logger output, or other reliable recovery artifact. Therefore the required exact `6,075 / 6,075` totals and terminal status cannot be established. Cobertura alone would not have been sufficient even if it had appeared.

## Verified Cleanup

The verified descendant chain was terminated child-first in this order: `testhost.exe` PID `204948`, `vstest.console.exe` PID `248344`, `dotnet-coverage.exe` PID `270696`, then wrapper `pwsh.exe` PID `68864`.

- Remaining verified tree processes after cleanup: none
- Canonical Cobertura output: absent
- Derived effective configuration: retained at `coverage-nonnumeric-adapter-member-coverage-final.2026-07-27T06-33.cobertura.xml.effective-coverage.config`, 820 bytes, SHA-256 `69509401502CFFF110C4EA8A72663E2A6A562C9DBCBA78D2E6E5BC682AF422F1`

## Result

P9-T45 remains unchecked. This is the task's one-shot timeout failure. P9-T46 and P9-T47 were not executed. An in-place plan revision is required; do not weaken the coverage gate or retry this invocation.
