# P9-T4 final remediation coverage gate

Command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final-remediation.2026-07-27T06-25.cobertura.xml
```

Exit code: `0`. The wrapper discovered all eight Debug test assemblies and VSTest completed 6,056/6,056 with zero failures or skips.

Cobertura result: 91,894/108,736 lines, `84.5111%`. Artifact SHA-256: `38B6EB6E7872B17AA9C5862E162F83C2DDCEB551A2DEA16BD9DDEF980F5B19A6`.

Canonical `coverage.config` was unchanged before and after: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`. The wrapper reported post-processing completion and no retained effective-settings artifact under `scripts/vscode`.
