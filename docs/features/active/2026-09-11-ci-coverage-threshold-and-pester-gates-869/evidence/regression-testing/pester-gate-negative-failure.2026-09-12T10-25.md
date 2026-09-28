# Negative path — Pester gate on a failing test case (P8-T4)

Timestamp: 2026-09-14T20-42

ExpectedExitCode: 1

## Method

With the tree committed and clean at `1e255062`, one deliberately failing test case was appended to `tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1`:

```
    It 'TEMPORARY deliberately failing case for the P8-T4 negative-path proof' { $true | Should -BeFalse }
```

The gate body was then run exactly as `.github/workflows/_pester.yml` runs it, with the coverage output path pointed at a separate file so the measurement of record written by P6-T4 was not overwritten. The file was restored afterwards with `git checkout`.

EXIT_CODE: 1

## Printed result lines, verbatim

```
PESTER Passed=174 Failed=1 Skipped=0 Total=175
COVERAGE LinePercent=83.93 Covered=731 Total=871
```

## Acceptance observations

- Recorded exit code: **1**, matching `ExpectedExitCode: 1`.
- Recorded failed count: **1**.

The measured LINE percentage on this run is **83.93**, which is comfortably **above** the 80 floor. That is what makes this proof discriminating: the coverage comparison could not have produced the non-zero exit, so the only statement that can have produced it is the explicit `exit 1` guarded by the failure count. Together with P8-T3, where the failure count was 0 and the coverage figure was below the floor, the two proofs isolate each of the gate's two exit conditions independently.

This proves the explicit exit statement is reached and is not masked by the configuration default, under which a pwsh step would otherwise exit zero regardless of the test outcome. `New-PesterConfiguration` defaults the configuration's exit option to false, and a pwsh step exits 0 unless the script calls `exit`, so a Pester job without an explicit exit on failure is a green-no-matter-what gate.

The counts also confirm the failure was the appended case and nothing else: the total rose from 174 to 175 and the passed count stayed at 174, so exactly one case failed and it is the one that was added.

## Restore confirmation

Command: `git -C "<repo-root>" checkout -- tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1`
EXIT_CODE: 0

Command: `git -C "<repo-root>" status --porcelain=v1 --untracked-files=all -- tests/scripts/vscode`
EXIT_CODE: 0
Output verbatim: empty.

The test root is clean after the restore, so the deliberately failing case left no residue in the tree. The separate coverage document this proof wrote lives under the gitignored `coverage/` directory and was deleted afterwards.

Output Summary: the Pester gate exits 1 on a single failing test case while its coverage figure is above the floor, proving the explicit failure exit is reached and is not masked by the configuration default. The tree was restored to a clean state.
