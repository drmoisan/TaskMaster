# [P1-T8] Full `QuickFiler.Test` suite

Timestamp: 2026-08-26T08-45

Command:

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /Logger:"trx;LogFileName=p1-t8.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p1-t8
```

Resolved `vstest.console.exe`:
`C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
(VSTest version 18.8.0, x64).

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

**Test Run Successful. Total tests: 938, Passed: 938, Failed: 0. Equal to the QuickFiler.Test
baseline of 938, so not lower.**

```
Test Run Successful.
Total tests: 938
     Passed: 938
 Total time: 18.7771 Seconds
```

Independently confirmed from the console log by counting result lines: 938 matching `^  Passed `,
**0** matching `^  Failed `, 0 matching `^  Skipped `.

### TRX artifact

`docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p1-t8/p1-t8.trx`

Exactly **one** TRX exists in that directory. Its `<Counters>` element:

```xml
<Counters total="938" executed="938" passed="938" failed="0" error="0" timeout="0" aborted="0"
          inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
          disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

`<ResultSummary outcome="Completed">`.

### Acceptance verification

| Condition | Required | Measured | Met |
|---|---|---|---|
| Exit code | 0 | **0** | yes |
| Failed count | exactly 0 | **0** | yes |
| Passed count | not lower than the P0-T14 QuickFiler.Test baseline | **938** vs baseline **938** | yes |

The comparable baseline is the QuickFiler.Test-only figure from P0-T14, **938 passed / 0 failed**,
not the nine-assembly aggregate of 6482. The reason the plan's literal wording is not usable here is
recorded as a plan defect in `evidence/baseline/p0-t14-tests-coverage.2026-08-26T08-25.md`: the
plan's `### Full-suite regression command` runs one assembly while its
`### Full-suite coverage command` runs nine, so a one-assembly passed count can never reach the
nine-assembly aggregate and the condition as literally worded could never be satisfied.

### Test-set identity, not just count parity

Count parity alone would not exclude one test disappearing while another appeared. The two test-name
sets were compared directly:

```
$ comm -13 <baseline names> <p1-t8 names>   # in P1-T8 but not baseline
(none)
$ comm -23 <baseline names> <p1-t8 names>   # in baseline but not P1-T8
(none)
```

The symmetric difference is empty: the 938 tests that passed at baseline are exactly the 938 that
pass now. **The 241-line dead-code removal caused no regression and removed no test.**

This comparison is also what identified the **937 -> 938** correction to the P0-T14 baseline figure;
see the CORRECTION section of that artifact. In short, vstest's console output is not strictly
ordered at assembly boundaries when nine assemblies run in one invocation, and one QuickFiler.Test
result line was emitted one line after the next assembly's marker, so the line-window segmentation
originally undercounted by one. The single-assembly P1-T8 run has no such ambiguity.

### TRX host-path sanitisation (mandatory before commit)

vstest embeds the operator's account and machine name in a TRX. All of it was removed before commit.
This section names the *class* of token each substitution targeted instead of quoting the raw values.
Quoting them here would reintroduce into a committed artifact exactly the identifiers the
sanitisation removed from the TRX, which the repository's artifact-hygiene rule forbids outright.

Substitutions applied, case-insensitively, in this order:

| # | Token class replaced | To | Rationale |
|---|---|---|---|
| 1 | the absolute workspace-root prefix of this worktree: drive letter, user-profile path, `repos\TaskMaster\.claude\worktrees\`, and the agent worktree identifier | `<repo-root>` | workspace-root prefix in `<TestRun>`, `<Deployment>`, and every `<UnitTest storage="...">` |
| 2 | the absolute user-profile path: drive letter, `Users`, and the account name | `<user-profile>` | any residual profile path |
| 3 | the machine name, matched in every case spelling it appears in | `<host>` | `computerName` attribute, the run `name`, and the domain component of `runUser` |
| 4 | the account name, standing alone | `<user>` | `runUser`, run `name`, `runDeploymentRoot` |

The order is load-bearing: 1 must precede 2, and 2 must precede 4. Applied in any other order, the
shorter token consumes the leading portion of the longer one and leaves a malformed remainder that
no later substitution matches.

Header, after sanitisation. The pre-sanitisation spellings are deliberately not reproduced; each
`AFTER` line below shows every placeholder that replaced a raw identifier at that position, so the
substitution remains auditable without the raw value being present.

```
AFTER:  <TestRun id="..." name="<user>@<host> 2026-08-26 08:53:51" runUser="<host>\<user>" ...>
```

```
AFTER:  <Deployment runDeploymentRoot="<user>_<host>_2026-08-26_08_53_51" />
```

```
AFTER:  <UnitTest name="..." storage="<repo-root>\quickfiler.test\bin\debug\quickfiler.test.dll" ...
```

Residual scan of the committed file. Each pattern was applied case-insensitively. The patterns are
described by class rather than quoted, for the same reason given above:

| Pattern (case-insensitive) | Hits |
|---|---|
| the account name | **0** |
| the machine name with its trailing digit dropped, so the match is a superset of the machine name itself | **0** |
| the drive-letter-plus-`Users` absolute-path prefix | **0** |
| the 8.3 short-name form of the account name | **0** |
| `<repo-root>` | 939 |

File: 1,222,587 bytes, UTF-8 with BOM, CRLF preserved.

**`LogFileName=` and `/ResultsDirectory:` were both passed**, as the plan's Conventions make
mandatory. Without `LogFileName=` vstest would have named the file
`<account>_<HOST>_<timestamp>.trx`, putting the account and machine name in the committed *filename*
where no content sanitisation could reach it. Without `/ResultsDirectory:` the TRX would have landed
in `TestResults\`, which `.gitignore` hides, making "the TRX exists at the evidence path"
unverifiable.

### Two execution notes (environment, not plan defects)

1. **`/InIsolation` must not be passed through the Bash tool.** The first attempt ran vstest directly
   from Git Bash and failed with:

   ```
   The test source file "C:/Program Files/Git/InIsolation" provided was not found.
   ```

   MSYS path conversion rewrote the leading-slash switch into a filesystem path. Re-running the
   identical command through `pwsh -NoProfile -Command` succeeded. This matches the delegation
   directive's rule that C# tooling is invoked through `pwsh`, never the Bash tool.

2. **`sed` cannot perform the TRX path substitution under MSYS.** A `sed 's#C:\\Users\\...#...#g'`
   pass silently matched nothing — verified on a minimal fixture, where both the doubled-backslash
   and single-backslash forms left the input unchanged — because MSYS rewrites an argument that
   looks like a Windows path before `sed` ever parses the expression. The substitution is therefore
   done with `[regex]::Replace(..., IgnoreCase)` in PowerShell. Case-insensitivity is required
   because vstest writes the mixed-case path in `<TestRun>`/`<Deployment>` but an all-lowercase path
   in each `<UnitTest storage="...">` attribute; a case-sensitive pass leaves 938 of them behind.

   The sanitizer script lives in the system temp directory, **not** under `<FEATURE>/`, because a
   retained `.ps1` anywhere under the feature folder forces the downstream feature-review coverage
   gate to FAIL.

Result: PASS. Toolchain step 4 (Testing) is green. The full toolchain loop completed in order —
format (P1-T5), lint (P1-T6), type-check (P1-T7), test (P1-T8) — with no step failing and no step
changing a file, so no restart is required.
