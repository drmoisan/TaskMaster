# Phase 0 — Baseline test run and coverage collection

Timestamp: 2026-09-03T13-28

Task: [P0-T9]
Issue: #731

## Command

The COVERAGE COLLECTION PROCEDURE declared in the plan was executed. `scripts\vscode\Invoke-MSTestWithCoverage.ps1` was **not** invoked as a script: its assembly-discovery predicate at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:301` excludes every assembly whose absolute path contains a `\.claude\` segment, which is every assembly in this worktree, so the script would throw at `:305-307` and produce no document (repository issue #752).

1. Build:

```
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

2. Dot-source commands (both files, as the procedure requires):

```
. scripts/vscode/Invoke-MSTestWithCoverage.ps1
. scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
```

3. Collection call, with all five mandatory parameter values:

```
Invoke-DotnetCoverageCollection `
    -OutputPath        coverage/baseline.cobertura.raw.xml `
    -CoverageConfig    coverage.config `
    -VsTestPath        C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe `
    -TestAssembly      <the nine repository-relative assemblies listed below> `
    -RunSettingsPath   scripts/vscode/TaskMaster.cli.runsettings
```

The `vstest.console.exe` path is recorded in full under the Evidence path-hygiene rule's stated exception for an external build-tool executable that lives outside this worktree under `Program Files` and contains no account name. It was resolved with `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`, taking the first result.

4. Post-processing, run unconditionally outside the `try`/`catch` (step 9 of the procedure):

```
ConvertTo-KoverageCoberturaXml -XmlContent <raw document content> -RepoRoot <worktree root>
```

with the returned string written to `coverage/baseline.cobertura.processed.xml`. The raw document was not overwritten.

EXIT_CODE: 0

`Invoke-DotnetCoverageCollection` returned without throwing, so the recorded value is `0`. This value is **Input T** of the DEGRADED-RUN STATE MODEL. Input T = 0, so **Axis C resolves to row C1**: [P5-T5] must record a test outcome of 0 with a failed count of 0, [P5-T6] and [P5-T7] operate in row C1, and the Axis C conjunct of [P6-T18]'s AC17 check-off is satisfied.

## Output Summary

vstest summary lines, verbatim as observed:

```
Test Run Successful.
Total tests: 6985
     Passed: 6985
 Total time: 33.6995 Seconds
```

- Total tests: **6985**
- Passed: **6985**
- Failed: **0**
- Skipped: **0**

The failed count is 0 and the skipped count is 0, so vstest emitted no `Failed:` and no `Skipped:` summary line and there is no failing-test name list and no skipped-test name list to record. The run reported `Test Run Successful.`

The runner's final line named the coverage artifact; rewritten to its repository-relative remainder under the Evidence path-hygiene rule, it is `coverage/baseline.cobertura.raw.xml`.

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

Every listed path is repository-relative. The list was built mechanically by the procedure's step 4 rule: every `*.Test.dll` under the worktree root whose full path matches the `bin/Debug` separator-anchored pattern and matches neither the `obj` nor the `ref` pattern, with the nested-worktree exclusion applied to the path **relative to the worktree root** rather than to the absolute path.

## Document existence after this task

- `coverage/baseline.cobertura.raw.xml` exists: True
- `coverage/baseline.cobertura.processed.xml` exists: True

---

## Numeric baseline coverage ([P0-T10])

Timestamp: 2026-09-03T13-31
Source document: `coverage/baseline.cobertura.processed.xml` (the processed document written by [P0-T9]; the raw document was not read for these figures).
Command: a PowerShell extraction applying the plan's separator-anchored filename match and de-duplicated per-line map rule. Every `class` element whose `filename` attribute ends with a directory separator immediately followed by `QfcFormController.SetupDisposal.cs` was enumerated (all of them, not the first); within each, `./lines/line` was enumerated first and then `./methods/method/lines/line`; every line was keyed by its `number` attribute; and a repeated key was resolved by keeping the maximum `hits`. All XML attributes were read through `GetAttribute('...')`. The descendant-axis `.//line` selection was not used.
EXIT_CODE: 0

Repository line-rate: 0.854194
Repository lines-valid: 64668
SetupDisposal covered lines: 111
SetupDisposal total lines: 157
SetupDisposal line coverage percent: 70.70

Cobertura document state: processed

That value is an audit assertion rather than a discriminator. It was derived only from the `class` elements the separator-anchored match selected for this task: exactly one such element was selected, and its `filename` attribute does not begin with a drive-letter prefix. Step 9 of the COVERAGE COLLECTION PROCEDURE runs unconditionally outside the `try`/`catch`, so this document is `processed` whatever the test outcome was, and the recorded value confirms the procedure ran as written.

Residual absolute filename count: 0

Informational only, not a gate. On this run no `class/@filename` value anywhere in the processed document begins with a drive-letter prefix. Only the presence flag and this count are recorded; no absolute value is written, so the Evidence path-hygiene rule is satisfied.

The derived percentage 111 / 157 = 70.70 percent reproduces the issue-#683 baseline figure of 70.70 percent recorded by [P0-T4], and the uncovered count 157 - 111 = 46 reproduces its figure of 46 uncovered lines. The extraction rule is therefore consistent with the measurement that produced the #683 baseline.

---

## Baseline per-line hits ([P0-T11])

Timestamp: 2026-09-03T14-05

Task: [P0-T11]
Issue: #731

Command: the [P0-T10] extraction rule applied to `coverage/baseline.cobertura.processed.xml` — the processed document [P0-T9] wrote, never the raw one — separately for each of the five production filenames this plan changes. For each filename, every `class` element whose `filename` attribute ends with a directory separator immediately followed by that filename was enumerated (all such elements, not the first); within each, `./lines/line` was enumerated first and then `./methods/method/lines/line`; every line was keyed by its `number` attribute; and a repeated key, whether within one `class` element or across several, was resolved by keeping the maximum `hits`. The descendant-axis `.//line` selection was not used. All XML attributes were read through `GetAttribute('...')` because dot-sourcing the repository helpers sets `Set-StrictMode -Version Latest` in the calling scope.

The separator anchor is expressed as an ordinal `EndsWith` test against the target filename prefixed by `[char]92` and, alternatively, by the forward slash. Constructing the separator from its character code rather than writing a backslash literal means the anchor cannot silently degrade into the unanchored match this task forbids, which is the failure mode the plan's Backslash-literal authoring rule exists to prevent.

EXIT_CODE: 0

Output Summary: 494 de-duplicated per-line rows were recorded across the three instrumented filenames — 312 for `QfcQueue.cs`, 157 for `QfcFormController.SetupDisposal.cs`, and 25 for `QfcRemainingQueueAdmission.cs`. The remaining two filenames have no `class` element in the document and are recorded under `Uninstrumented files` below. `coverage/baseline.cobertura.processed.xml` exists on disk at the end of this task; no later task in this plan writes to it or to `coverage/baseline.cobertura.raw.xml`, because [P5-T5] writes only the two `postchange.` paths.

Only the bare filename is recorded in each row, never the Cobertura `filename` attribute's full value. No recorded row contains a drive-letter prefix or a directory separator, so the Evidence path-hygiene rule is satisfied. The five filenames are distinct and the anchored match condition stated above establishes which source file each row belongs to. A given `<filename>:<number>` pair appears on at most one row, because each row is one entry of the de-duplicated map.

### Uninstrumented files

Two of the five filenames select zero `class` elements under the separator-anchored match, so zero `hits=` rows exist for them. This is a recorded baseline fact and not a failure of this task: no executor action can produce a row for a file the coverage tool did not instrument.

| Filename | Reason | Attribute location |
|---|---|---|
| `QfcCollectionController.cs` | class-level `[ExcludeFromCodeCoverage]`, which `dotnet-coverage` honours | `QuickFiler/Controllers/QfcCollectionController.cs:21`, with `using System.Diagnostics.CodeAnalysis;` at `:4` |
| `QfcDatamodel.cs` | class-level `[ExcludeFromCodeCoverage]`, which `dotnet-coverage` honours | `QuickFiler/Controllers/QfcDatamodel.cs:25`, with `using System.Diagnostics.CodeAnalysis;` at `:6` |

Both attributes are pre-existing on `origin/main`. Neither is introduced by this change and correcting either is out of scope for issue #731. The absence of `hits=` rows for these two filenames is expected. [P5-T7] records every changed executable line in these two files under its `Uninstrumented, not comparable` sub-heading.

Confirmation that this is not a post-processing artefact: neither filename appears as a `class/@filename` value in `coverage/baseline.cobertura.raw.xml` either, so the exclusion originates in instrumentation rather than in `ConvertTo-KoverageCoberturaXml`.

### Filename match audit

Two integers per filename, for all five whether instrumented or not: the number of `class` elements selected by the separator-anchored match, and the number the unanchored bare-filename match would have selected. Where the two differ, the additional elements the unanchored match would have added are named by their `name` attribute only, which is a type name and carries no path.

| Filename | Anchored | Unanchored | Additional elements under the unanchored match (by `name`) |
|---|---|---|---|
| `QfcCollectionController.cs` | 0 | 0 | none |
| `QfcDatamodel.cs` | 0 | 1 | `QuickFiler.Interfaces.QfcDequeueBatch` |
| `QfcQueue.cs` | 1 | 1 | none |
| `QfcFormController.SetupDisposal.cs` | 1 | 1 | none |
| `QfcRemainingQueueAdmission.cs` | 1 | 1 | none |

The `QfcDatamodel.cs` row is direct confirmation that the anchoring is load-bearing in fact rather than only in principle. The unanchored bare-filename match selects `QuickFiler.Interfaces.QfcDequeueBatch`, the `public readonly struct` declared at `QuickFiler/Interfaces/IQfcDatamodel.cs:49`, whose executable lines would have been folded into the `QfcDatamodel.cs` map and would have attributed real coverage to a file that has none of its own. The anchored match correctly excludes it. The anchoring must not be relaxed in [P5-T6], [P5-T7] or [P5-T8].

No line of `QuickFiler/Interfaces/IQfcDatamodel.cs`, `QuickFiler/Controllers/IQfcQueue.cs`, or `QuickFiler/Interfaces/IQfcCollectionController.cs` appears under the `QfcDatamodel.cs`, `QfcQueue.cs`, or `QfcCollectionController.cs` rows below, because every row was built from the anchored selection.

### De-duplicated per-line map totals

| Filename | Total map entries | Entries with hits greater than 0 |
|---|---|---|
| `QfcQueue.cs` | 312 | 157 |
| `QfcFormController.SetupDisposal.cs` | 157 | 111 |
| `QfcRemainingQueueAdmission.cs` | 25 | 23 |

No `class` element selected for any of the five filenames carries a `filename` attribute beginning with a drive-letter prefix, so the union-scoped `Cobertura document state:` value for this task is `processed`.

### Per-line rows

One `<filename>:<number> hits=<hits>` row per de-duplicated map entry, 494 rows in total.

```
QfcQueue.cs:20 hits=1
QfcQueue.cs:21 hits=1
QfcQueue.cs:22 hits=1
QfcQueue.cs:23 hits=1
QfcQueue.cs:24 hits=1
QfcQueue.cs:26 hits=1
QfcQueue.cs:27 hits=1
QfcQueue.cs:28 hits=1
QfcQueue.cs:32 hits=1
QfcQueue.cs:33 hits=1
QfcQueue.cs:35 hits=1
QfcQueue.cs:37 hits=1
QfcQueue.cs:38 hits=1
QfcQueue.cs:39 hits=1
QfcQueue.cs:40 hits=1
QfcQueue.cs:47 hits=1
QfcQueue.cs:48 hits=1
QfcQueue.cs:49 hits=1
QfcQueue.cs:50 hits=1
QfcQueue.cs:53 hits=1
QfcQueue.cs:54 hits=1
QfcQueue.cs:55 hits=1
QfcQueue.cs:57 hits=1
QfcQueue.cs:58 hits=0
QfcQueue.cs:59 hits=1
QfcQueue.cs:60 hits=1
QfcQueue.cs:61 hits=1
QfcQueue.cs:62 hits=1
QfcQueue.cs:63 hits=1
QfcQueue.cs:64 hits=1
QfcQueue.cs:65 hits=1
QfcQueue.cs:66 hits=1
QfcQueue.cs:67 hits=1
QfcQueue.cs:68 hits=1
QfcQueue.cs:69 hits=1
QfcQueue.cs:71 hits=1
QfcQueue.cs:74 hits=1
QfcQueue.cs:75 hits=1
QfcQueue.cs:76 hits=1
QfcQueue.cs:77 hits=1
QfcQueue.cs:78 hits=1
QfcQueue.cs:79 hits=1
QfcQueue.cs:80 hits=1
QfcQueue.cs:81 hits=1
QfcQueue.cs:82 hits=1
QfcQueue.cs:88 hits=1
QfcQueue.cs:91 hits=1
QfcQueue.cs:93 hits=1
QfcQueue.cs:94 hits=1
QfcQueue.cs:96 hits=1
QfcQueue.cs:99 hits=1
QfcQueue.cs:100 hits=1
QfcQueue.cs:101 hits=1
QfcQueue.cs:102 hits=1
QfcQueue.cs:103 hits=1
QfcQueue.cs:105 hits=1
QfcQueue.cs:106 hits=1
QfcQueue.cs:108 hits=1
QfcQueue.cs:110 hits=1
QfcQueue.cs:111 hits=1
QfcQueue.cs:112 hits=1
QfcQueue.cs:113 hits=1
QfcQueue.cs:114 hits=1
QfcQueue.cs:115 hits=1
QfcQueue.cs:116 hits=1
QfcQueue.cs:117 hits=1
QfcQueue.cs:118 hits=1
QfcQueue.cs:119 hits=1
QfcQueue.cs:121 hits=1
QfcQueue.cs:122 hits=1
QfcQueue.cs:123 hits=1
QfcQueue.cs:124 hits=1
QfcQueue.cs:125 hits=0
QfcQueue.cs:126 hits=0
QfcQueue.cs:127 hits=0
QfcQueue.cs:128 hits=1
QfcQueue.cs:129 hits=1
QfcQueue.cs:130 hits=1
QfcQueue.cs:131 hits=1
QfcQueue.cs:132 hits=1
QfcQueue.cs:133 hits=1
QfcQueue.cs:134 hits=1
QfcQueue.cs:135 hits=1
QfcQueue.cs:136 hits=1
QfcQueue.cs:137 hits=1
QfcQueue.cs:138 hits=1
QfcQueue.cs:139 hits=1
QfcQueue.cs:140 hits=1
QfcQueue.cs:141 hits=1
QfcQueue.cs:142 hits=1
QfcQueue.cs:143 hits=0
QfcQueue.cs:144 hits=0
QfcQueue.cs:145 hits=0
QfcQueue.cs:146 hits=0
QfcQueue.cs:147 hits=0
QfcQueue.cs:149 hits=1
QfcQueue.cs:150 hits=1
QfcQueue.cs:151 hits=1
QfcQueue.cs:152 hits=1
QfcQueue.cs:153 hits=0
QfcQueue.cs:154 hits=0
QfcQueue.cs:155 hits=0
QfcQueue.cs:156 hits=0
QfcQueue.cs:157 hits=0
QfcQueue.cs:158 hits=0
QfcQueue.cs:160 hits=1
QfcQueue.cs:161 hits=1
QfcQueue.cs:164 hits=1
QfcQueue.cs:174 hits=1
QfcQueue.cs:175 hits=1
QfcQueue.cs:176 hits=0
QfcQueue.cs:177 hits=1
QfcQueue.cs:178 hits=1
QfcQueue.cs:179 hits=1
QfcQueue.cs:180 hits=1
QfcQueue.cs:181 hits=1
QfcQueue.cs:182 hits=1
QfcQueue.cs:185 hits=0
QfcQueue.cs:187 hits=0
QfcQueue.cs:189 hits=0
QfcQueue.cs:191 hits=0
QfcQueue.cs:192 hits=0
QfcQueue.cs:193 hits=0
QfcQueue.cs:194 hits=0
QfcQueue.cs:195 hits=0
QfcQueue.cs:196 hits=0
QfcQueue.cs:197 hits=0
QfcQueue.cs:198 hits=0
QfcQueue.cs:199 hits=0
QfcQueue.cs:200 hits=0
QfcQueue.cs:201 hits=0
QfcQueue.cs:202 hits=0
QfcQueue.cs:203 hits=0
QfcQueue.cs:204 hits=0
QfcQueue.cs:205 hits=0
QfcQueue.cs:206 hits=0
QfcQueue.cs:208 hits=0
QfcQueue.cs:209 hits=1
QfcQueue.cs:214 hits=1
QfcQueue.cs:215 hits=1
QfcQueue.cs:216 hits=1
QfcQueue.cs:217 hits=1
QfcQueue.cs:218 hits=0
QfcQueue.cs:219 hits=0
QfcQueue.cs:220 hits=1
QfcQueue.cs:222 hits=1
QfcQueue.cs:224 hits=1
QfcQueue.cs:233 hits=0
QfcQueue.cs:235 hits=0
QfcQueue.cs:236 hits=0
QfcQueue.cs:237 hits=0
QfcQueue.cs:241 hits=0
QfcQueue.cs:245 hits=0
QfcQueue.cs:251 hits=0
QfcQueue.cs:258 hits=0
QfcQueue.cs:259 hits=0
QfcQueue.cs:267 hits=0
QfcQueue.cs:270 hits=0
QfcQueue.cs:271 hits=0
QfcQueue.cs:272 hits=0
QfcQueue.cs:273 hits=0
QfcQueue.cs:274 hits=0
QfcQueue.cs:275 hits=0
QfcQueue.cs:278 hits=0
QfcQueue.cs:281 hits=0
QfcQueue.cs:282 hits=0
QfcQueue.cs:283 hits=0
QfcQueue.cs:284 hits=0
QfcQueue.cs:285 hits=0
QfcQueue.cs:286 hits=0
QfcQueue.cs:287 hits=0
QfcQueue.cs:288 hits=0
QfcQueue.cs:291 hits=1
QfcQueue.cs:292 hits=1
QfcQueue.cs:293 hits=1
QfcQueue.cs:294 hits=1
QfcQueue.cs:295 hits=1
QfcQueue.cs:296 hits=1
QfcQueue.cs:297 hits=1
QfcQueue.cs:298 hits=1
QfcQueue.cs:299 hits=1
QfcQueue.cs:300 hits=1
QfcQueue.cs:301 hits=1
QfcQueue.cs:302 hits=1
QfcQueue.cs:303 hits=1
QfcQueue.cs:305 hits=0
QfcQueue.cs:306 hits=0
QfcQueue.cs:307 hits=0
QfcQueue.cs:308 hits=0
QfcQueue.cs:309 hits=0
QfcQueue.cs:310 hits=0
QfcQueue.cs:311 hits=0
QfcQueue.cs:312 hits=1
QfcQueue.cs:313 hits=1
QfcQueue.cs:323 hits=0
QfcQueue.cs:325 hits=0
QfcQueue.cs:328 hits=0
QfcQueue.cs:331 hits=0
QfcQueue.cs:334 hits=0
QfcQueue.cs:335 hits=0
QfcQueue.cs:338 hits=0
QfcQueue.cs:340 hits=0
QfcQueue.cs:341 hits=0
QfcQueue.cs:343 hits=0
QfcQueue.cs:344 hits=0
QfcQueue.cs:345 hits=0
QfcQueue.cs:346 hits=0
QfcQueue.cs:347 hits=0
QfcQueue.cs:348 hits=0
QfcQueue.cs:349 hits=0
QfcQueue.cs:350 hits=0
QfcQueue.cs:351 hits=0
QfcQueue.cs:352 hits=0
QfcQueue.cs:353 hits=0
QfcQueue.cs:354 hits=0
QfcQueue.cs:356 hits=0
QfcQueue.cs:357 hits=0
QfcQueue.cs:358 hits=0
QfcQueue.cs:359 hits=0
QfcQueue.cs:360 hits=0
QfcQueue.cs:362 hits=0
QfcQueue.cs:363 hits=0
QfcQueue.cs:364 hits=0
QfcQueue.cs:365 hits=0
QfcQueue.cs:366 hits=0
QfcQueue.cs:367 hits=0
QfcQueue.cs:369 hits=0
QfcQueue.cs:370 hits=0
QfcQueue.cs:371 hits=0
QfcQueue.cs:372 hits=0
QfcQueue.cs:373 hits=0
QfcQueue.cs:374 hits=0
QfcQueue.cs:375 hits=0
QfcQueue.cs:376 hits=0
QfcQueue.cs:377 hits=0
QfcQueue.cs:378 hits=0
QfcQueue.cs:379 hits=0
QfcQueue.cs:380 hits=0
QfcQueue.cs:381 hits=0
QfcQueue.cs:382 hits=0
QfcQueue.cs:383 hits=0
QfcQueue.cs:384 hits=0
QfcQueue.cs:385 hits=0
QfcQueue.cs:386 hits=0
QfcQueue.cs:387 hits=0
QfcQueue.cs:388 hits=0
QfcQueue.cs:389 hits=0
QfcQueue.cs:390 hits=0
QfcQueue.cs:393 hits=0
QfcQueue.cs:396 hits=0
QfcQueue.cs:397 hits=0
QfcQueue.cs:399 hits=0
QfcQueue.cs:402 hits=1
QfcQueue.cs:403 hits=1
QfcQueue.cs:404 hits=1
QfcQueue.cs:405 hits=1
QfcQueue.cs:406 hits=1
QfcQueue.cs:407 hits=1
QfcQueue.cs:408 hits=1
QfcQueue.cs:409 hits=1
QfcQueue.cs:417 hits=1
QfcQueue.cs:418 hits=1
QfcQueue.cs:419 hits=1
QfcQueue.cs:421 hits=1
QfcQueue.cs:423 hits=1
QfcQueue.cs:424 hits=0
QfcQueue.cs:425 hits=0
QfcQueue.cs:428 hits=1
QfcQueue.cs:429 hits=1
QfcQueue.cs:430 hits=1
QfcQueue.cs:431 hits=1
QfcQueue.cs:432 hits=1
QfcQueue.cs:433 hits=1
QfcQueue.cs:434 hits=1
QfcQueue.cs:435 hits=1
QfcQueue.cs:436 hits=1
QfcQueue.cs:437 hits=1
QfcQueue.cs:438 hits=1
QfcQueue.cs:439 hits=1
QfcQueue.cs:440 hits=1
QfcQueue.cs:441 hits=1
QfcQueue.cs:443 hits=1
QfcQueue.cs:445 hits=1
QfcQueue.cs:446 hits=1
QfcQueue.cs:447 hits=1
QfcQueue.cs:448 hits=1
QfcQueue.cs:449 hits=1
QfcQueue.cs:458 hits=0
QfcQueue.cs:459 hits=0
QfcQueue.cs:460 hits=0
QfcQueue.cs:461 hits=0
QfcQueue.cs:462 hits=0
QfcQueue.cs:463 hits=0
QfcQueue.cs:473 hits=0
QfcQueue.cs:474 hits=0
QfcQueue.cs:475 hits=0
QfcQueue.cs:476 hits=0
QfcQueue.cs:477 hits=0
QfcQueue.cs:478 hits=0
QfcQueue.cs:481 hits=0
QfcQueue.cs:482 hits=0
QfcQueue.cs:483 hits=0
QfcQueue.cs:484 hits=0
QfcQueue.cs:485 hits=0
QfcQueue.cs:486 hits=0
QfcQueue.cs:489 hits=0
QfcQueue.cs:490 hits=0
QfcQueue.cs:491 hits=0
QfcQueue.cs:497 hits=0
QfcQueue.cs:498 hits=0
QfcQueue.cs:499 hits=0
QfcQueue.cs:501 hits=0
QfcFormController.SetupDisposal.cs:23 hits=1
QfcFormController.SetupDisposal.cs:24 hits=1
QfcFormController.SetupDisposal.cs:25 hits=1
QfcFormController.SetupDisposal.cs:26 hits=1
QfcFormController.SetupDisposal.cs:27 hits=1
QfcFormController.SetupDisposal.cs:28 hits=1
QfcFormController.SetupDisposal.cs:29 hits=1
QfcFormController.SetupDisposal.cs:32 hits=1
QfcFormController.SetupDisposal.cs:33 hits=1
QfcFormController.SetupDisposal.cs:34 hits=1
QfcFormController.SetupDisposal.cs:35 hits=1
QfcFormController.SetupDisposal.cs:37 hits=1
QfcFormController.SetupDisposal.cs:39 hits=1
QfcFormController.SetupDisposal.cs:40 hits=1
QfcFormController.SetupDisposal.cs:41 hits=1
QfcFormController.SetupDisposal.cs:42 hits=1
QfcFormController.SetupDisposal.cs:45 hits=1
QfcFormController.SetupDisposal.cs:46 hits=1
QfcFormController.SetupDisposal.cs:49 hits=1
QfcFormController.SetupDisposal.cs:50 hits=1
QfcFormController.SetupDisposal.cs:51 hits=1
QfcFormController.SetupDisposal.cs:52 hits=1
QfcFormController.SetupDisposal.cs:53 hits=1
QfcFormController.SetupDisposal.cs:54 hits=1
QfcFormController.SetupDisposal.cs:55 hits=1
QfcFormController.SetupDisposal.cs:56 hits=1
QfcFormController.SetupDisposal.cs:60 hits=0
QfcFormController.SetupDisposal.cs:62 hits=0
QfcFormController.SetupDisposal.cs:65 hits=0
QfcFormController.SetupDisposal.cs:66 hits=0
QfcFormController.SetupDisposal.cs:67 hits=0
QfcFormController.SetupDisposal.cs:68 hits=0
QfcFormController.SetupDisposal.cs:69 hits=0
QfcFormController.SetupDisposal.cs:70 hits=0
QfcFormController.SetupDisposal.cs:71 hits=0
QfcFormController.SetupDisposal.cs:72 hits=0
QfcFormController.SetupDisposal.cs:73 hits=1
QfcFormController.SetupDisposal.cs:76 hits=1
QfcFormController.SetupDisposal.cs:77 hits=1
QfcFormController.SetupDisposal.cs:78 hits=1
QfcFormController.SetupDisposal.cs:79 hits=1
QfcFormController.SetupDisposal.cs:82 hits=0
QfcFormController.SetupDisposal.cs:83 hits=0
QfcFormController.SetupDisposal.cs:84 hits=0
QfcFormController.SetupDisposal.cs:85 hits=1
QfcFormController.SetupDisposal.cs:90 hits=1
QfcFormController.SetupDisposal.cs:91 hits=1
QfcFormController.SetupDisposal.cs:92 hits=1
QfcFormController.SetupDisposal.cs:93 hits=1
QfcFormController.SetupDisposal.cs:94 hits=1
QfcFormController.SetupDisposal.cs:95 hits=0
QfcFormController.SetupDisposal.cs:96 hits=0
QfcFormController.SetupDisposal.cs:99 hits=1
QfcFormController.SetupDisposal.cs:100 hits=1
QfcFormController.SetupDisposal.cs:101 hits=1
QfcFormController.SetupDisposal.cs:102 hits=1
QfcFormController.SetupDisposal.cs:104 hits=1
QfcFormController.SetupDisposal.cs:105 hits=1
QfcFormController.SetupDisposal.cs:106 hits=1
QfcFormController.SetupDisposal.cs:107 hits=0
QfcFormController.SetupDisposal.cs:108 hits=0
QfcFormController.SetupDisposal.cs:109 hits=0
QfcFormController.SetupDisposal.cs:110 hits=0
QfcFormController.SetupDisposal.cs:112 hits=1
QfcFormController.SetupDisposal.cs:113 hits=1
QfcFormController.SetupDisposal.cs:114 hits=1
QfcFormController.SetupDisposal.cs:115 hits=1
QfcFormController.SetupDisposal.cs:116 hits=1
QfcFormController.SetupDisposal.cs:117 hits=1
QfcFormController.SetupDisposal.cs:120 hits=1
QfcFormController.SetupDisposal.cs:124 hits=1
QfcFormController.SetupDisposal.cs:125 hits=1
QfcFormController.SetupDisposal.cs:126 hits=1
QfcFormController.SetupDisposal.cs:127 hits=1
QfcFormController.SetupDisposal.cs:128 hits=1
QfcFormController.SetupDisposal.cs:130 hits=1
QfcFormController.SetupDisposal.cs:131 hits=1
QfcFormController.SetupDisposal.cs:132 hits=1
QfcFormController.SetupDisposal.cs:133 hits=1
QfcFormController.SetupDisposal.cs:134 hits=1
QfcFormController.SetupDisposal.cs:135 hits=0
QfcFormController.SetupDisposal.cs:136 hits=1
QfcFormController.SetupDisposal.cs:137 hits=1
QfcFormController.SetupDisposal.cs:141 hits=0
QfcFormController.SetupDisposal.cs:142 hits=0
QfcFormController.SetupDisposal.cs:143 hits=0
QfcFormController.SetupDisposal.cs:144 hits=0
QfcFormController.SetupDisposal.cs:145 hits=0
QfcFormController.SetupDisposal.cs:146 hits=0
QfcFormController.SetupDisposal.cs:147 hits=0
QfcFormController.SetupDisposal.cs:150 hits=1
QfcFormController.SetupDisposal.cs:151 hits=1
QfcFormController.SetupDisposal.cs:152 hits=1
QfcFormController.SetupDisposal.cs:153 hits=1
QfcFormController.SetupDisposal.cs:156 hits=1
QfcFormController.SetupDisposal.cs:157 hits=1
QfcFormController.SetupDisposal.cs:158 hits=0
QfcFormController.SetupDisposal.cs:159 hits=0
QfcFormController.SetupDisposal.cs:160 hits=0
QfcFormController.SetupDisposal.cs:161 hits=0
QfcFormController.SetupDisposal.cs:162 hits=1
QfcFormController.SetupDisposal.cs:163 hits=0
QfcFormController.SetupDisposal.cs:164 hits=0
QfcFormController.SetupDisposal.cs:165 hits=0
QfcFormController.SetupDisposal.cs:166 hits=0
QfcFormController.SetupDisposal.cs:167 hits=1
QfcFormController.SetupDisposal.cs:168 hits=1
QfcFormController.SetupDisposal.cs:170 hits=1
QfcFormController.SetupDisposal.cs:171 hits=1
QfcFormController.SetupDisposal.cs:172 hits=1
QfcFormController.SetupDisposal.cs:173 hits=1
QfcFormController.SetupDisposal.cs:174 hits=1
QfcFormController.SetupDisposal.cs:175 hits=1
QfcFormController.SetupDisposal.cs:176 hits=1
QfcFormController.SetupDisposal.cs:179 hits=1
QfcFormController.SetupDisposal.cs:180 hits=1
QfcFormController.SetupDisposal.cs:181 hits=1
QfcFormController.SetupDisposal.cs:182 hits=1
QfcFormController.SetupDisposal.cs:185 hits=1
QfcFormController.SetupDisposal.cs:186 hits=1
QfcFormController.SetupDisposal.cs:187 hits=0
QfcFormController.SetupDisposal.cs:188 hits=0
QfcFormController.SetupDisposal.cs:189 hits=0
QfcFormController.SetupDisposal.cs:190 hits=0
QfcFormController.SetupDisposal.cs:191 hits=1
QfcFormController.SetupDisposal.cs:192 hits=0
QfcFormController.SetupDisposal.cs:193 hits=0
QfcFormController.SetupDisposal.cs:194 hits=0
QfcFormController.SetupDisposal.cs:195 hits=0
QfcFormController.SetupDisposal.cs:196 hits=1
QfcFormController.SetupDisposal.cs:197 hits=1
QfcFormController.SetupDisposal.cs:199 hits=1
QfcFormController.SetupDisposal.cs:200 hits=1
QfcFormController.SetupDisposal.cs:201 hits=1
QfcFormController.SetupDisposal.cs:202 hits=1
QfcFormController.SetupDisposal.cs:203 hits=1
QfcFormController.SetupDisposal.cs:204 hits=1
QfcFormController.SetupDisposal.cs:205 hits=1
QfcFormController.SetupDisposal.cs:211 hits=1
QfcFormController.SetupDisposal.cs:212 hits=1
QfcFormController.SetupDisposal.cs:213 hits=0
QfcFormController.SetupDisposal.cs:214 hits=0
QfcFormController.SetupDisposal.cs:215 hits=0
QfcFormController.SetupDisposal.cs:217 hits=1
QfcFormController.SetupDisposal.cs:218 hits=1
QfcFormController.SetupDisposal.cs:219 hits=1
QfcFormController.SetupDisposal.cs:220 hits=1
QfcFormController.SetupDisposal.cs:221 hits=1
QfcFormController.SetupDisposal.cs:222 hits=1
QfcFormController.SetupDisposal.cs:223 hits=1
QfcFormController.SetupDisposal.cs:224 hits=1
QfcFormController.SetupDisposal.cs:225 hits=1
QfcFormController.SetupDisposal.cs:226 hits=1
QfcFormController.SetupDisposal.cs:227 hits=1
QfcFormController.SetupDisposal.cs:228 hits=1
QfcFormController.SetupDisposal.cs:229 hits=1
QfcFormController.SetupDisposal.cs:230 hits=1
QfcRemainingQueueAdmission.cs:15 hits=1
QfcRemainingQueueAdmission.cs:16 hits=1
QfcRemainingQueueAdmission.cs:17 hits=1
QfcRemainingQueueAdmission.cs:18 hits=1
QfcRemainingQueueAdmission.cs:19 hits=1
QfcRemainingQueueAdmission.cs:20 hits=1
QfcRemainingQueueAdmission.cs:21 hits=1
QfcRemainingQueueAdmission.cs:22 hits=1
QfcRemainingQueueAdmission.cs:23 hits=1
QfcRemainingQueueAdmission.cs:24 hits=0
QfcRemainingQueueAdmission.cs:25 hits=0
QfcRemainingQueueAdmission.cs:28 hits=1
QfcRemainingQueueAdmission.cs:29 hits=1
QfcRemainingQueueAdmission.cs:30 hits=1
QfcRemainingQueueAdmission.cs:31 hits=1
QfcRemainingQueueAdmission.cs:32 hits=1
QfcRemainingQueueAdmission.cs:35 hits=1
QfcRemainingQueueAdmission.cs:36 hits=1
QfcRemainingQueueAdmission.cs:38 hits=1
QfcRemainingQueueAdmission.cs:39 hits=1
QfcRemainingQueueAdmission.cs:40 hits=1
QfcRemainingQueueAdmission.cs:43 hits=1
QfcRemainingQueueAdmission.cs:44 hits=1
QfcRemainingQueueAdmission.cs:45 hits=1
QfcRemainingQueueAdmission.cs:46 hits=1
```

End of the `Baseline per-line hits` record. [P5-T7] re-reads `coverage/baseline.cobertura.processed.xml` with the same rule and cross-checks its own extraction against these rows; a disagreement on any `<filename>:<number>` pair present in both sources, or the absence of that document, is a blocked coverage comparison rather than a value to proceed on.
