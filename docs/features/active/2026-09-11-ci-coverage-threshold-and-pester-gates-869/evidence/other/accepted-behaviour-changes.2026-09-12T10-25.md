# Accepted behaviour changes from adopting the local script in CI (P8-T6)

Timestamp: 2026-09-14T20-48

Two behaviour changes arrive with replacing the inline vstest block in `.github/workflows/_mstest-coverage.yml` with an invocation of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`. Both were decided deliberately under settled decision D9 of the specification and are recorded here rather than discovered later.

## 1. MSTest class-level parallelization arrives in CI

The script passes a runsettings file that the previous CI step did not pass. That file is `scripts/vscode/TaskMaster.cli.runsettings`, which is **9 lines**. The two element values this change relies on, quoted verbatim from it:

```
      <Workers>0</Workers>
```

```
      <Scope>ClassLevel</Scope>
```

`Workers` set to `0` means one worker per core, and `Scope` set to `ClassLevel` means test classes run in parallel with one another. The previous CI step passed no `/Settings:` argument at all, and `vstest.console.exe` does not auto-detect a runsettings file, so CI has not applied either setting until now.

**This is accepted for the first run.** Any new failure that appears is treated as a **finding**, not as a flake: a test class that fails only under class-level parallelization is sharing process-wide state, and that is a defect to be fixed rather than a condition to be suppressed. The alternative considered was a CI-specific runsettings file with `Workers` set to `1`; it was rejected because it would diverge the CI route from the local route, and running the same route as the local tooling is the stated intent of the change.

## 2. The trx logger is lost

The previous CI step passed `/Logger:trx`. The script's argument builder does not pass it, so no trx document is produced by the CI run. The previous upload step globbed `TestResults/**/*.trx` and `TestResults/**/*.coverage` with `if-no-files-found: warn`, under which the artifact would have become silently empty and the job would still have been green.

**Remedy, performed in P7-T2:** the upload step was repointed at the Cobertura document with `if-no-files-found: error`:

```
      - name: Upload coverage document
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: coverage/coverage.cobertura.xml
          if-no-files-found: error
```

This requires no production change and publishes the artifact the new gate actually reads. Adding `/Logger:trx` to the script's argument builder was the alternative; it was rejected because it is a production change whose argument list is pinned by assertions in `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`.

## 3. Upload-target decision: the projection document is deliberately not uploaded

The merged evidence-projection item's route writes a **second** document alongside the Cobertura one, at `coverage/coverage.cobertura.jacoco.xml`. It is a package-level JaCoCo projection of the post-processed Cobertura document, and the run that produced this delivery's P0-T7 baseline wrote it, as that artifact's captured output records:

```
Coverage projection: <repo-root>\coverage\coverage.cobertura.jacoco.xml
```

Adding that path to the upload step was **considered and deliberately not taken**. The reason is that doing so would widen the declared artifact contract that acceptance criterion AC-10 and the specification's write set describe: AC-10 names the coverage document the job uploads, and the write set names the paths this delivery changes. Publishing a second document is a change to what the job promises its consumers, and it belongs to the item that introduced the projection rather than to this one. The decision is recorded here rather than acted on, exactly as P7-T2 directs.

The consequence is bounded and stated plainly: the projection document is produced on every CI run and is discarded with the runner. A future item that wants it published should add it to the upload path and update AC-10's wording and the write set together.

## Related note on document retention

The same merged item added `Test-RawCoverageDocumentRetained`, which discards the coverage document at the resolved output path unless that path's parent directory is exactly the repository root joined with `coverage`. The CI invocation supplies no `-CoverageOutput`, so the entry point uses its declared default `coverage\coverage.cobertura.xml` and the parent is exactly that directory. The predicate therefore returns true, the document is retained, and `if-no-files-found: error` on the upload step is a live gate rather than a guaranteed failure. The P0-T7 and P10-T8 artifacts record this check and its clear result against a real run.
