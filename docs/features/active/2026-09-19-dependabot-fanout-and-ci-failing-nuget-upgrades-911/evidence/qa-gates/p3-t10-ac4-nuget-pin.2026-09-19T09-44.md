# P3-T10 — AC4: the NuGet CLI version is pinned everywhere it is selected

Timestamp: 2026-09-20T00-46

Commands:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/DependabotConfig.Tests.ps1"); $c.Filter.FullName = "*AC4-*"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p3-t10-ac4-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

```
CMD-ACTIONLINT:
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; & "<execution-worktree-root>\scripts\dev-tools\run-actionlint.ps1"'
```

EXIT_CODE: 0 for both.

The suite was extended with the `Edit` tool. No heredoc and no shell redirection was used.

## Pester result, verbatim

```
PESTER Passed=2 Failed=0 Skipped=0 Total=7
```

```
Describing Dependabot configuration consolidation
 Context NuGet CLI version pinning
   [+] AC4- enumerates at least one workflow step that uses the setup-nuget action
   [+] AC4- pins every setup-nuget step to an exact three-part version literal
```

A note on the two numbers, so `Total=7` is not misread. Pester 5's `TotalCount` counts every test
**discovered** in the run path, including those a filter left unrun; `PassedCount` counts those
that actually executed and passed. The file now carries 7 tests — the 5 `AC1-` cases and the 2
`AC4-` cases — so a `*AC4-*` filtered run discovers 7 and runs 2. The acceptance clause `Total` at
least 2 is satisfied at 7, and the sharper figure is `Passed=2 Failed=0` over exactly the two
`AC4-` cases, both named in the `Detailed` output above.

The trailing hyphen in the filter token is load-bearing here for the first time in this file: the
file now holds both `AC1-` and `AC4-` cases, and `*AC4-*` selected the two `AC4-` cases and no
`AC1-` case.

## The independent enumeration of setup-nuget steps

Taken outside the test, over `.github/workflows/*.yml`:

```
SETUP_NUGET_STEP_COUNT=3
  _build-analyzers.yml line 31 nuget-version=7.9.0 threePart=True
  _build-nullable.yml line 31 nuget-version=7.9.0 threePart=True
  _mstest-coverage.yml line 47 nuget-version=7.9.0 threePart=True
```

**Exactly 3**, as the acceptance requires, each declaring the exact three-part literal `7.9.0`.
The count is over the 8 workflow YAML files; the only other occurrence of the token `setup-nuget`
in the directory is prose in `.github/workflows/README.md`, which the `*.yml` filter excludes.

The greater-than-zero assertion inside the test is what prevents a broken enumerator from passing
the pinning assertion vacuously: an enumerator returning an empty set makes every per-step
assertion trivially true, so the first `It` asserts a non-empty enumeration before the second
asserts anything about its members. The second `It` repeats that guard before its loop, because a
`foreach` over an empty collection executes no assertion at all.

The version regex is anchored, `^\d+\.\d+\.\d+$`. A floating selector such as `latest`, a
two-part `7.9`, or a quoted value the parse failed to unquote would each fail it. That the two
tests pass is itself evidence the parse strips the surrounding quotes correctly, since the raw
line text is `nuget-version: '7.9.0'` and the anchored pattern does not admit the quote
characters.

## actionlint

```
$ pwsh -NoProfile -Command 'Set-Location "<W>"; & "<W>\scripts\dev-tools\run-actionlint.ps1"'
EXIT=0
--- stdout bytes ---
0
```

Captured stdout is **empty**, 0 bytes. Per gate rule 10 actionlint prints nothing at all on a
clean run — no file count and no summary line — so **no count of any kind can be read from its
output**. The figure of 8 workflow YAML files recorded above is an **independent filesystem
enumeration** taken with `Get-ChildItem`, not actionlint output.

The script resolves `actionlint-bin\actionlint.exe` relative to the repository root and throws
when it is absent, so an absent binary would be a task failure rather than a silent pass. Exit 0
with no throw therefore establishes the binary ran.

## Coverage document

Written to `coverage/p3-t10-ac4-coverage.xml`, under `coverage/`, which `.gitignore:144` ignores.
This task records no aggregate JaCoCo LINE figure and is not one of the six tasks the gate rule 12
standing-in obligation falls on.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| Pester `EXIT_CODE` | 0 | 0 | PASS |
| Pester `Failed` | 0 | 0 | PASS |
| Pester `Total` | at least 2 | 7 discovered, 2 run and passed | PASS |
| Enumerated setup-nuget step count | exactly 3 | 3 | PASS |
| CMD-ACTIONLINT `EXIT_CODE` | 0 | 0, stdout empty | PASS |

## Acceptance criterion checked off

**AC4 — The NuGet CLI version is pinned everywhere it is selected** is checked off in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.

Output Summary: `tests/scripts/dependencies/DependabotConfig.Tests.ps1` was extended with a
`NuGet CLI version pinning` context carrying 2 `AC4-` cases. The `*AC4-*` filtered run returned
EXIT_CODE 0 with `Passed=2 Failed=0`, `TotalCount` 7 across the file's now-7 discovered tests. An
independent enumeration over `.github/workflows/*.yml` finds **exactly 3** steps using the
setup-nuget action — `_build-analyzers.yml:31`, `_build-nullable.yml:31` and
`_mstest-coverage.yml:47` — each declaring the exact three-part literal `7.9.0` and each matching
the anchored `^\d+\.\d+\.\d+$` pattern. CMD-ACTIONLINT returned EXIT_CODE 0 with 0 bytes of
stdout; the 8-file workflow count recorded here is an independent filesystem enumeration and not
actionlint output. AC4 is checked off in `spec.md`.
