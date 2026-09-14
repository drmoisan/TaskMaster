# Negative path — C# gate with nothing to measure (P8-T2)

Timestamp: 2026-09-14T20-28

ExpectedExitCode: 1

Command: `pwsh -NoProfile -Command '<worktree prologue>; . "<repo-root>/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1"; $q = [char]34; $doc = "<coverage branch-rate=" + $q + "0.95" + $q + " branches-valid=" + $q + "0" + $q + " />"; Assert-CoberturaBranchCoverageThreshold -CoberturaXml $doc'`
EXIT_CODE: 1

## Input document

```
<coverage branch-rate="0.95" branches-valid="0" />
```

The branch rate of 0.95 is comfortably **above** the 75 floor. That is the point of this case: the only reason to reject this document is that it has nothing to measure. A gate that checked the rate alone would read this document as a pass.

## Recorded error text

The terminating error propagated out of the function and out of the pwsh process, which exited 1. The thrown message, verbatim and exactly as required:

```
Cobertura branch coverage has no valid branches.
```

The throw originates at line 118 of `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, which sits before the below-floor comparison, so the zero-branch guard fires first and its distinct message is what a reader sees.

## Complementary proof: withholding the input does not produce a green run

The zero-branch guard closes the case where the document exists but measures nothing. The `if-no-files-found: error` setting on the upload steps closes the complementary case where the document does not exist at all. Both upload steps are quoted verbatim.

From `.github/workflows/_mstest-coverage.yml`:

```
      - name: Upload coverage document
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: coverage/coverage.cobertura.xml
          if-no-files-found: error
```

From `.github/workflows/_pester.yml`:

```
      - name: Upload coverage document
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: pester-coverage
          path: coverage/pester-coverage.xml
          if-no-files-found: error
```

Both carry `if: always()`, so the upload runs even when the gate step has already failed, and both carry `if-no-files-found: error`, so an absent coverage document fails the job rather than warning. The previous setting on the MSTest callee was `if-no-files-found: warn`, under which a silently empty artifact produced a green run; the P7-T7 artifact records the before count of 1 and the after count of 0 for that literal.

Taken together the two mechanisms mean the gate cannot be evaded by withholding its input: an absent document fails the upload, and a present document with no branches fails the assertion.

Output Summary: the zero-branch guard fails closed even on an above-floor branch rate. The pwsh process exited 1, matching `ExpectedExitCode: 1`, and the thrown text is exactly `Cobertura branch coverage has no valid branches.` Both upload steps are quoted, each carrying `if-no-files-found: error`.
