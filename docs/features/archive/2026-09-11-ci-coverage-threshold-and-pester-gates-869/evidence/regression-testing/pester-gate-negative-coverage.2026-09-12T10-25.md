# Negative path — Pester gate on reduced coverage (P8-T3)

Timestamp: 2026-09-14T20-34

ExpectedExitCode: 1

## Method

The gate body was run exactly as `.github/workflows/_pester.yml` runs it, with one change: the run path was set to the single file `tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1` while the coverage path stayed the whole directory `scripts/vscode`. The coverage output path was pointed at a separate file so the measurement of record written by P6-T4 was not overwritten.

This reduces the covered set to one small script's worth of lines and is deterministic. Deleting one arbitrary test file instead would cross the floor only if the final margin happened to be smaller than that file's contribution, which is not a property this proof should depend on.

**The tree was not modified.** No test file was deleted, renamed or edited.

EXIT_CODE: 1

## Printed result lines, verbatim

```
PESTER Passed=2 Failed=0 Skipped=0 Total=2
COVERAGE LinePercent=7.46 Covered=65 Total=871
```

## Acceptance observations

- Recorded exit code: **1**, matching `ExpectedExitCode: 1`.
- Recorded LINE percentage: **7.46**, which is below 80.
- Recorded failed count: **0**.

The failed count of 0 is what makes the non-zero exit attributable to the coverage floor and not to a test failure. The gate body evaluates the failure count first and the coverage floor second, so with zero failures the only statement that can produce `exit 1` is the line-percentage comparison.

The denominator is unchanged at 871, the same total the P6-T4 measurement of record records. Only the numerator moved, from 731 to 65, which is the intended effect of restricting the run path.

## Tree-state confirmation

Command: `git -C "<repo-root>" status --porcelain=v1 --untracked-files=all -- tests/scripts/vscode scripts/vscode`
EXIT_CODE: 0
Output verbatim: empty.

The separate coverage document this proof wrote lives under the gitignored `coverage/` directory and was deleted afterwards.

## Tolerance margin

Computed from the P6-T4 figures as the covered count minus the ceiling of 0.80 times the total:

- Covered: 731
- Total: 871
- Ceiling of 0.80 × 871: **697**
- **Margin: 731 − 697 = 34 covered lines**

The suite may lose up to 34 covered lines before the gate turns red. That margin is the quantity a future reviewer should watch when removing or narrowing a test.

## Defect found and corrected during this task

The first execution of the gate body failed before reaching the coverage comparison, with:

```
ParentContainsErrorRecordException: The property 'counter' cannot be found on this object. Verify that the property exists.
```

Diagnosis, measured rather than inferred. A JaCoCo document declares `<!DOCTYPE report PUBLIC ...>`, so under PowerShell's XML adapter `$jacoco.report` resolves to a **two-element array** holding the `XmlDocumentType` node and the `XmlElement`. A direct probe confirmed it:

```
TYPE=System.Object[]
COUNT=2
ITEM=System.Xml.XmlElement | name=report
```

Without `Set-StrictMode`, member access across that array silently enumerates and yields the element's counters, which is why the selector recorded in the P0-T9 artifact worked in every measurement task in this plan: none of those tasks sets strict mode. The gate step body authored in P7-T4 **does** set `Set-StrictMode -Version Latest`, per the repository's established workflow-step preamble, and under strict mode the same access throws.

This was a real defect in the delivered gate: on a GitHub runner the step would have thrown before evaluating the floor, failing the job for a reason unrelated to coverage. It was found only because this negative-path proof runs the gate body rather than asserting over its text.

Correction applied to `.github/workflows/_pester.yml`: the counter is now selected by XPath, which names the report element's own counter children unambiguously and is strict-safe:

```
$lineCounter = @($jacoco.SelectNodes('/report/counter')) | Where-Object { $_.type -eq 'LINE' }
```

This reads the same report-level LINE counter as the selector recorded in P0-T9 and changes no figure. The corrected body is what produced the result recorded above, and `.github/workflows/_pester.yml` was re-linted after the change.

Output Summary: the Pester gate exits 1 on a sub-threshold coverage figure with zero test failures, at a measured LINE percentage of 7.46 against the 80 floor. The tolerance margin on the real suite is 34 covered lines. A strict-mode defect in the gate body's counter selector was found by running the body and was corrected.
