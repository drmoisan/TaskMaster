# Baseline: csharpier check — issue #877

Timestamp: 2026-09-13T10-44
Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation"; dotnet tool run csharpier check . 2>&1 | Tee-Object -Variable out | Out-Null; Write-Host "EXIT_CODE=$LASTEXITCODE"; $out | Select-Object -Last 40'`
EXIT_CODE: 0
Output Summary: Observed exit code 0. Tool summary line: `Checked 1626 files in 4835ms.` Zero lines of the captured output contain the token `Was not formatted`. Class (b) list: EMPTY. Run under an acquired build lock, released immediately after the command returned.

## Class (b) baseline list

Class (b) list: EMPTY.

The repository is clean under the manifest-pinned CSharpier 1.2.6 at baseline, so there are no pre-existing unformatted paths for the mandatory repo-wide `format .` in [P2-T2] to repair incidentally. Consequently:

- [P2-T4] permits only the write-set paths in its scope check and must record `Class (b) list: EMPTY`.
- [P2-T14] has no class-(b) path under `SVGControl` or `SVGControl.Test` to consider; that STOP AND REPORT branch is unreachable from this baseline.
- [P2-T15] permits only the write-set paths.
- [P2-T16] excludes nothing from its token search.
- [P2-T28] makes no separate class-(b) commit.

## Expectation field

No `ExpectedExitCode:` row is declared, because the observed exit code is 0 and the default expectation is 0.
