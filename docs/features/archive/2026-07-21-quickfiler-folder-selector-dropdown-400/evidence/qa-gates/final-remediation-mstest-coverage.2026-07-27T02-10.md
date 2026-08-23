# Final Remediation MSTest Coverage Gate — Failed

- Timestamp: `2026-07-27T02-10Z`
- Run identity: `2026-07-27T02-07`
- Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final-remediation.2026-07-27T02-10.cobertura.xml`
- EXIT_CODE: `1`
- Output Summary: `The wrapper discovered and forwarded 8 Debug test assemblies. VSTest reported 6,047 total tests with 4 failures. The coverage output was produced, but the nonzero test result invalidates the final pass. Repository line coverage from this incomplete run was 69.0994%.`

## Invariants observed before failure

| Invariant | Result |
|---|---|
| Canonical `coverage.config` SHA-256 before and after | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` |
| Test assemblies discovered after the VSTest boundary | `8` |
| Coverage output | `coverage-final-remediation.2026-07-27T02-10.cobertura.xml` (17,218,438 bytes) |
| Derived effective settings path | `coverage-final-remediation.2026-07-27T02-10.cobertura.xml.effective-coverage.config` |
| Derived effective settings retained after exit | `false` |
| Source/test/script worktree delta from the wrapper | `false` |
| Cobertura repository line coverage | `69.0994%` |

The wrapper emitted the expected VSTest path and all-eight-assembly discovery message, then reported `Total tests: 6047` and `Failed: 4`. Because test failures are disallowed by P9-T4, this run is not usable as final QA evidence. It remains preserved as failure evidence; P9-T4 is unchecked and the plan requires a restart at P9-T1 after remediation.
