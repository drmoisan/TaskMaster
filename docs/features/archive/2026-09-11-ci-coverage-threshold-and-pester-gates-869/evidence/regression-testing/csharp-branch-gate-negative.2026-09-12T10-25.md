# Negative path — C# branch gate on a below-floor projection (P8-T1)

Timestamp: 2026-09-14T20-26

ExpectedExitCode: 1

Command: `pwsh -NoProfile -Command '<worktree prologue>; . "<repo-root>/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1"; $q = [char]34; $doc = "<coverage branch-rate=" + $q + "0.7499" + $q + " branches-valid=" + $q + "1200" + $q + " />"; Assert-CoberturaBranchCoverageThreshold -CoberturaXml $doc'`
EXIT_CODE: 1

The document literal is built by concatenation around `[char]34` rather than with inline escaped quotes, because a backslash-escaped quote inside a single-quoted shell payload reaches PowerShell as a literal backslash and mis-binds the argument. The document actually passed was echoed by the command and is recorded verbatim below.

## Input document

```
<coverage branch-rate="0.7499" branches-valid="1200" />
```

The branch rate is below the floor and the valid-branch count is positive, so the zero-branch guard is not what fires: the below-floor comparison is.

## Recorded error text

The terminating error propagated out of the function and out of the pwsh process, which exited 1. The thrown message, verbatim:

```
Cobertura branch coverage 74.99% is below the required 75% threshold.
```

The message contains the token `is below the required 75`. It also names the measured percentage, `74.99%`, so a failing run records the figure rather than only the fact of failure.

The throw originates at line 124 of `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, which the error record identifies directly.

## The exit-code propagation guard that carries this into the job result

The gate step in `.github/workflows/_mstest-coverage.yml` carries the repository's established guard immediately after the script invocation, quoted verbatim from line 95 of that file:

```
          if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
```

The full gate step, for context:

```
      - name: Run MSTest suite with coverage
        shell: pwsh
        run: |
          ...
          & ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug
          if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
```

A terminating error inside the script makes the invoked pwsh script exit non-zero; the guard then terminates the step with that same code, and GitHub Actions interprets a non-zero step exit as a failed step and a failed job. The same guard shape appears at lines 39 and 84 of this callee for the dotnet-coverage install and the build step respectively, so the pattern is the callee's established one rather than one introduced here.

Output Summary: the branch assertion fails closed on a below-floor projection. The pwsh process exited 1, matching `ExpectedExitCode: 1`, and the thrown text carries the token `is below the required 75` together with the measured percentage. The `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` guard at line 95 of the MSTest coverage callee is what carries that non-zero exit into the job result.
