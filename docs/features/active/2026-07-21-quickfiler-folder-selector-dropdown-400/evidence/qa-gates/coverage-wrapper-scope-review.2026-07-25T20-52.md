# Coverage wrapper scope review

Timestamp: 2026-07-25T20-52Z

Command: `git show d16b5dcc^..d16b5dcc -- scripts/vscode/Invoke-MSTestWithCoverage.ps1 tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1; git status --short; Get-FileHash -Algorithm SHA256 scripts/vscode/Invoke-MSTestWithCoverage.ps1,tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1,coverage.config,TaskMaster.runsettings,scripts/vscode/TaskMaster.cli.runsettings,scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1; Get-ChildItem -Recurse -Filter '*.Test.dll'`

EXIT_CODE: 0

Output Summary: PASS. Commit `d16b5dcc` changed exactly one production PowerShell file and one existing Pester file. The implementation retains every canonical instrumentation exclusion, adds exactly one test-assembly-only exclusion, forwards all eight discovered test assemblies to VSTest, and removes only the output-adjacent derived settings file in `finally` on successful and failed collection. The final PoshQC pass introduced no formatter delta, no analyzer regression, and no failing or skipped Pester test. Canonical configuration and related persistent settings remain unchanged.

## Authorized scope and file integrity

The implementation commit's PowerShell scope changed only these two files:

| Classification | Path | P8-T36 SHA-256 | P8-T36 lines | Current SHA-256 | Current lines |
|---|---|---|---:|---|---:|
| Production | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | `4782C4E3F00CEA7F852AC884387AE9FDD15615F888F132CB7E71F2F1D9868E26` | 186 | `73E1A76E17C901D3E0A5BA254CA3025D4EFF1D0F5455921B6E5BA9CB6125D6B2` | 312 |
| Test | `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | `835D3F4890C7D896B09D43330F414A815ACB7670AD0A385CC042F33720EE7F5E` | 169 | `4FD01E3EF23A43F5B3E7FC304B96656F65FF8192267FECB94DD6343B6350DC93` | 328 |

Both files remain below the repository's 500-line limit. The fresh P8-T39 formatter inventory recorded the same current hashes and line counts before and after formatting.

Commit `d16b5dcc` also checkpoints earlier authorized C# and feature-evidence work. Those files are outside the P8-T38 PowerShell correction; the commit contains no additional production or test PowerShell change.

## Zero-regression PoshQC reconciliation

| Gate | Baseline | Final | Delta | Result |
|---|---:|---:|---:|---|
| Formatter file changes | 0 | 0 | 0 | PASS |
| Folder-scan analyzer findings | 22 | 16 | -6 | PASS |
| Authorized-file analyzer findings | 0 | 0 | 0 | PASS |
| Focused Pester failures | 0 | 0 | 0 | PASS |
| Focused Pester skipped tests | 0 | 0 | 0 | PASS |

The required final MCP Pester run executed 30 tests with 30 passing test-case nodes, zero failures, zero errors, zero disabled tests, and zero skipped tests. The supplementary P8-T36 baseline had passed all 11 then-existing focused tests; the 19 additional cases include the four derived-settings lifecycle regressions.

Current gate artifacts:

- `coverage-wrapper-poshqc-format.2026-07-25T20-30.md`
- `coverage-wrapper-poshqc-analyze.2026-07-25T20-30.md`
- `coverage-wrapper-poshqc-test.2026-07-25T20-30.md`

## Configuration and derived instrumentation scope

Canonical `coverage.config` remained byte-for-byte unchanged:

- P8-T36 SHA-256: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`
- Current SHA-256: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`

`ConvertTo-DerivedCoverageSettingsXml` retains the canonical `ModulePath` exclusions and permits exactly one additional instrumentation exclusion:

`.*\.Test\.dll$`

The exclusion applies only to dynamic instrumentation. It does not remove test assemblies from discovery or from the VSTest argument list.

The derived path is deterministic and uniquely scoped to the requested coverage output:

`<requested-cobertura-output>.effective-coverage.config`

`Invoke-DotnetCoverageCollection` resolves both paths, requires the derived file's parent directory to equal the requested output's parent directory, and rejects a derived path equal to canonical `coverage.config`.

## Eight-assembly retention

The current Debug inventory contains exactly these eight assemblies:

1. `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`
2. `Tags.Test\bin\Debug\Tags.Test.dll`
3. `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll`
4. `TaskTree.Test\bin\Debug\TaskTree.Test.dll`
5. `TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll`
6. `ToDoModel.Test\bin\Debug\ToDoModel.Test.dll`
7. `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`
8. `VBFunctions.Test\bin\Debug\VBFunctions.Test.dll`

`Get-DotnetCoverageArgumentList` places `--`, then the VSTest executable, then the complete `TestAssembly` array. The focused test `uses the derived settings path and preserves all eight test assemblies after the vstest boundary` compares that forwarded segment with the original eight-element array and passed in the final Pester gate. No assembly filter was added.

## Cleanup proof

`Invoke-DotnetCoverageCollection` marks the verified derived path for cleanup before writing it, executes coverage inside `try`, and removes that exact path from `finally`.

The following deterministic in-memory Pester cases passed:

- `removes the derived settings after successful collection without writing the canonical file`
- `removes the derived settings after failed collection without writing the canonical file`

Each case proves one derived-file write, no write to canonical `coverage.config`, and one removal of the same derived path. The failure case injects an exception from the executable wrapper and verifies cleanup without replacing the original exception. The tests mock `Set-Content` and `Remove-Item`; they do not create a real temporary file. A fresh workspace search found zero retained `*.effective-coverage.config` files.

## Prohibited-change review

The implementation commit contains no package, project, persisted-setting, runsettings, filter, threshold, or coverage-policy file change.

| Protected behavior or file | Verification |
|---|---|
| Package/project configuration | No package manifest, project file, props file, or targets file changed in `d16b5dcc`. |
| `TaskMaster.runsettings` | Unchanged; current SHA-256 `199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA`. |
| `scripts/vscode/TaskMaster.cli.runsettings` | Unchanged; current SHA-256 `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57`. |
| LiveOutlook filter | Preserved exactly as `/TestCaseFilter:TestCategory!=LiveOutlook`. |
| VSTest isolation | Preserved exactly as `/InIsolation`. |
| Coverage thresholds and helpers | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` is unchanged; current SHA-256 `9D129B0F22CEAC6B535059769CF3AA345E3B5F3B081C351553E495843E4DD2A1`. |
| Cobertura post-processing | The existing output normalization, source injection, and Koverage copy remain after collection with no substantive change. |
| Canonical configuration | `coverage.config` is read only and retains SHA-256 `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`. |

Result: PASS for P8-T42.
