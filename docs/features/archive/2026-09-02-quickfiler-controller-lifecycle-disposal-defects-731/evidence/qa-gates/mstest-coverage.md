# Final QA gate 5 — full suite with coverage

Timestamp: 2026-09-03T14-33

Task: [P5-T5]
Issue: #731

## Command

The COVERAGE COLLECTION PROCEDURE declared in the plan was executed, with `-OutputPath` set to the post-change raw path. `scripts\vscode\Invoke-MSTestWithCoverage.ps1` was **not** invoked as a script: its assembly-discovery predicate at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:301` excludes every assembly whose absolute path contains a `\.claude\` segment, which is every assembly in this worktree, so the script would throw at `:305-307` and produce no document (repository issue #752).

1. Build:

```
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

Exit code 0, 0 warnings, 0 errors. MSBuild executable actually invoked: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`.

2. Dot-source commands (both files, as the procedure requires):

```
. scripts/vscode/Invoke-MSTestWithCoverage.ps1
. scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
```

3. Collection call, with all five mandatory parameter values:

```
Invoke-DotnetCoverageCollection `
    -OutputPath        coverage/postchange.cobertura.raw.xml `
    -CoverageConfig    coverage.config `
    -VsTestPath        C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe `
    -TestAssembly      <the nine repository-relative assemblies listed below> `
    -RunSettingsPath   scripts/vscode/TaskMaster.cli.runsettings
```

The `vstest.console.exe` path is recorded in full under the Evidence path-hygiene rule's stated exception for an external build-tool executable outside this worktree, under `Program Files`, containing no account name. It was resolved with `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`, taking the first result.

The call was wrapped in `try`/`catch` and its full console output was redirected with `*>&1` through `Tee-Object -FilePath`, a streaming writer, so the vstest summary lines were on disk before any throw could reach the `catch`. Assigning the redirected output to a variable is prohibited by the procedure and was not done.

4. Post-processing, run unconditionally outside the `try`/`catch` (step 9 of the procedure):

```
ConvertTo-KoverageCoberturaXml -XmlContent <raw document content> -RepoRoot <worktree root>
```

with the returned string written to `coverage/postchange.cobertura.processed.xml`. The raw document was not overwritten. Neither `coverage/baseline.cobertura.raw.xml` nor `coverage/baseline.cobertura.processed.xml` was written by this task; only the two `postchange.` paths were.

**Correction recorded: step 9 was run twice.** The first execution passed `-RepoRoot` as an absolute path spelled with forward slashes. `ConvertTo-KoverageRelativePath` (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:52`) strips a prefix by literal `StartsWith` against `<root>\` and `<root>/` at `:87-92`, and the Cobertura `filename` attributes use backslashes, so no candidate prefix matched and no filename was relativised. The resulting document was detectably still in the `raw` state: all 563 `class/@filename` values retained a drive-letter prefix. Step 9 is a pure transformation of the raw document, which was already on disk, so it was re-run with the repo root in its native form, `(Resolve-Path '.').Path`. **No test was re-executed and the raw document was not regenerated.** The corrected document has a residual absolute-filename count of 0. The correction changed no measurement: `line-rate`, `lines-valid`, and the per-line map totals for all five tracked filenames are identical before and after it, which confirms that the filename relativisation did not alter the per-filename merge grouping. The floor assert recorded below was re-evaluated against the corrected document.

EXIT_CODE: 0

`Invoke-DotnetCoverageCollection` returned without throwing, so the recorded value is `0`.

## Axis C resolution

**Axis C row: C1.**

The row is keyed on Input T alone, which is the outcome `[P0-T9]` recorded on the pre-change tree: **Input T = 0**, selecting row **C1**. In row C1 this task's bar is absolute: a test outcome of 0 and a recorded failed count of 0. Both hold. The row was read from the DEGRADED-RUN STATE MODEL rather than re-derived here.

Because row C1 was taken, the row C3 obligations — reproducing the `[P0-T9]` failing- and skipped-test name lists and showing this run's lists are subsets of them — do not apply. Both name lists are empty on both runs, so there is nothing to subset.

## Output Summary

vstest summary lines, verbatim as observed:

```
Test Run Successful.
Total tests: 6995
     Passed: 6995
 Total time: 39.6579 Seconds
```

- Total tests: **6995**
- Passed: **6995**
- Failed: **0**
- Skipped: **0**

The failed count is 0 and the skipped count is 0, so vstest emitted no `Failed:` and no `Skipped:` summary line, and there is no failing-test name list and no skipped-test name list to record. The run reported `Test Run Successful.`

The runner's final line named the coverage artifact; rewritten to its repository-relative remainder under the Evidence path-hygiene rule, it is `coverage/postchange.cobertura.raw.xml`.

### Total-count bar

`[P0-T9]` recorded a baseline total of **6985**. This task's bar is `>= 6985 + 10 = 6995`, where 10 is this plan's net-new test-method count: two topology methods ([P1-T5]), seven cleanup methods ([P2-T1]), one volatile proxy method ([P4-T2]), and zero net from finding 3 ([P3-T1] deletes one method and adds one), giving 2 + 7 + 1 + 0 = 10.

Observed total: **6995**. The bar is met exactly, which is the expected result: the delta of exactly 10 confirms that this plan added precisely its ten net-new methods and removed no pre-existing test.

### No newly failing or newly skipped test

Baseline: 6985 total, 6985 passed, 0 failed, 0 skipped. This run: 6995 total, 6995 passed, 0 failed, 0 skipped. The failed and skipped counts are 0 on both runs, so no test is newly failing and none is newly skipped. This is the relative bar spec.md AC18 states.

## Discovered test assemblies

Count: **9**

```
QuickFiler.Test/bin/Debug/QuickFiler.Test.dll
SVGControl.Test/bin/Debug/SVGControl.Test.dll
Tags.Test/bin/Debug/Tags.Test.dll
TaskMaster.Test/bin/Debug/TaskMaster.Test.dll
TaskTree.Test/bin/Debug/TaskTree.Test.dll
TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll
ToDoModel.Test/bin/Debug/ToDoModel.Test.dll
UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll
VBFunctions.Test/bin/Debug/VBFunctions.Test.dll
```

Every listed path is repository-relative. This list is **identical, as a set of repository-relative paths, to the `Discovered test assemblies:` list `[P0-T9]` recorded** in `EVIDENCE/baseline/mstest-coverage.md`, and the two recorded counts are equal at 9. The comparison was made by reading both recorded lists; they agree path-for-path in the same order.

That identity is what makes every count comparison this task draws against `[P0-T9]` a comparison over the same suite rather than an assertion about two different suites. It also closes the narrow failure mode in which a second run collects over a smaller assembly set while still clearing the total-count bar; the `>= [P0-T9] total + 10` bar alone catches only the gross form.

## Absolute floor assert

`Assert-CoberturaLineCoverageThreshold` (`scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1:3`) was called on the processed post-change document, wrapped in `try`/`catch`. Its `-CoberturaXml` parameter takes the document as a **string of XML content**, and the post-processed string was passed directly.

Absolute floor result: PASS

The function returned without throwing, so the repository-wide 80 percent line-coverage floor is met on the post-change tree. Observed repository `line-rate` is **0.854146**, which is 85.4146 percent.

Because the result is `PASS`, the `FAIL` branch of this task does not apply: no baseline comparison of `line-rate` and `lines-valid` is required here, no Axis D comparability measurement is applied at this step, and **no deferral of the no-regression judgment to [P5-T7] is recorded by this task**. [P5-T7] should find no deferral recorded in this artifact.

This floor is a pre-existing repository-wide property recorded as supporting evidence. It is not this task's bar and not an acceptance criterion of issue #731; spec.md AC18 at line 234 asks only for no newly failing or newly skipped tests, which is a relative bar.

## Document existence after this task

- `coverage/postchange.cobertura.raw.xml` exists: True
- `coverage/postchange.cobertura.processed.xml` exists: True

## Verdict

PASS in Axis C row **C1**: `EXIT_CODE: 0`, recorded failed count 0, total 6995 meeting the `>= 6995` bar, assembly set identical to the baseline's, and exactly one `Absolute floor result:` line whose value is `PASS`.
