# Coverage delta — repository and changed-line

Timestamp: 2026-09-03T14-38

Tasks: [P5-T6] and [P5-T7]
Issue: #731

---

## [P5-T6] — post-change numeric coverage extraction

Command: the `[P0-T10]` extraction rule applied to `coverage/postchange.cobertura.processed.xml`. Every `class` element whose `filename` attribute ends with a directory separator immediately followed by the target filename was enumerated (all of them, not the first); within each, `./lines/line` was enumerated first and then `./methods/method/lines/line`; every line was keyed by its `number` attribute; and a repeated key was resolved by keeping the maximum `hits`. The descendant-axis `.//line` selection was not used. All XML attributes were read through `GetAttribute('...')`. The separator anchor is an ordinal `EndsWith` test against the target filename prefixed by `[char]92`, and alternatively by the forward slash, so it cannot degrade into an unanchored match.

EXIT_CODE: 0

### Output Summary

| Value | Figure |
|---|---|
| Baseline repository line-rate | **0.854194** |
| Post-change repository line-rate | **0.854146** |
| Baseline lines-valid | **64668** |
| Post-change lines-valid | **64688** |
| Baseline SetupDisposal line coverage percent | **70.70** |
| Post-change SetupDisposal line coverage percent | **74.73** |

None of these six values is the text `UNVERIFIED`.

The baseline figures are the ones `[P0-T10]` recorded in `EVIDENCE/baseline/mstest-coverage.md`. The post-change SetupDisposal percentage is derived from the de-duplicated per-line map for `QfcFormController.SetupDisposal.cs`: 136 covered entries out of 182 total, which is 74.7253 percent, recorded to two decimal places as 74.73.

**Is the post-change repository line-rate greater than or equal to the baseline one?** No. It is **0.000048 below** the baseline in rate units (0.854194 minus 0.854146). This is stated explicitly rather than glossed. Whether that difference is admissible is decided by the branch below.

### Axis C row

**C1**, determined by Input T alone, which `[P0-T9]` recorded as 0. No Axis C row forces an Axis D branch; Axis D is measured and applied identically in rows C1 and C3.

### Axis D resolution and branch selection

- Baseline `lines-valid`: 64668
- Post-change `lines-valid`: 64688
- Absolute difference: **20**
- One percent of the baseline figure: 646.68
- 20 is at most 646.68, so the two figures differ by at most 1 percent of the baseline figure.

**Axis D row: D-COMPARABLE. Branch A is taken.**

#### Branch A

The bar is that the post-change repository line-rate must be **no more than 0.005 below** the baseline value recorded by `[P0-T10]`. The bar is stated in **rate units**, because the Cobertura `line-rate` attribute is a fraction in the closed interval 0 to 1 and not a percentage: `Assert-CoberturaLineCoverageThreshold` range-checks it with `if ($lineRate -lt 0 -or $lineRate -gt 1)` at `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1:47-48` and multiplies by 100 to obtain a percentage at `:51`. Comparing the two recorded fractions against 0.5 would be a bar no measurement on this harness could breach.

- Bar: at most **0.005** below baseline, in rate units.
- Observed difference: **0.000048** below baseline, in the same units.
- 0.000048 is at most 0.005.

**Branch A PASSES.**

Branch B was not taken, so no denominator-mismatch statement and no deferral of the no-regression judgment to `[P5-T7]` is recorded by this task.

### Cobertura document state

- `[P0-T10]` capture: **processed**
- This run: **processed**

Both recorded values are `processed`, so the coverage comparison proceeds rather than being blocked. Each value was derived only from the `class` elements the separator-anchored match selected, over the union of the elements selected for the five tracked filenames, as `[P0-T10]` requires; a document-wide scan is prohibited there because `ConvertTo-KoverageRelativePath` returns unchanged any path outside the repository root, so a correctly processed document can still carry drive-letter filenames for such sources. On this run the informational whole-document residual absolute-filename count is 0.

Recording the state as an audit assertion rather than a discriminator is what caught the post-processing defect noted in `EVIDENCE/qa-gates/mstest-coverage.md`: the first execution of step 9 passed `-RepoRoot` with forward slashes, no filename was relativised, and the audit value would have read `raw`. Step 9 was re-run against the same on-disk raw document with the native root, and the corrected document changed no measurement.

---

## [P5-T7] — no coverage regression on changed lines

Command:

```
git diff --unified=0 35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e -- QuickFiler/Controllers/QfcCollectionController.cs QuickFiler/Controllers/QfcDatamodel.cs QuickFiler/Controllers/QfcQueue.cs QuickFiler/Controllers/QfcFormController.SetupDisposal.cs QuickFiler/Controllers/QfcRemainingQueueAdmission.cs
```

The `<DIFF-BASE>` operand is the 40-character SHA recorded on the `Diff base:` line of `EVIDENCE/baseline/tree-invariants.md` by `[P0-T2]`, substituted verbatim: `35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e`. It is byte-identical to that recorded value. The literal ref `origin/main` was not used.

EXIT_CODE: 0

### Observed hunks

| File | Hunk | Shape |
|---|---|---|
| `QfcCollectionController.cs` | `@@ -82,0 +83,2 @@` | pure insertion, 2 non-executable lines |
| `QfcCollectionController.cs` | `@@ -991 +993 @@` | one-to-one, 1 executable line |
| `QfcDatamodel.cs` | `@@ -102,0 +103,2 @@` | pure insertion, 2 non-executable lines |
| `QfcDatamodel.cs` | `@@ -354,2 +355,0 @@` | pure deletion, 2 pre-image lines removed, none added |
| `QfcFormController.SetupDisposal.cs` | `@@ -206,0 +207,3 @@` | pure insertion, post lines 207-209 |
| `QfcFormController.SetupDisposal.cs` | `@@ -218 +221,29 @@` | one-line-replaced-by-many, post lines 221-249 |
| `QfcQueue.cs` | `@@ -39,0 +40,2 @@` | pure insertion, 2 non-executable lines |
| `QfcRemainingQueueAdmission.cs` | `@@ -5 +4,0 @@`, `@@ -16,2 +14,0 @@`, `@@ -23,5 +19,0 @@` | pure deletions, no line added |

### Axis C row

**C1.** The changed-line comparison below is in force in both Axis C rows; there is no state reachable under this plan in which it is suspended.

### Cobertura document state

- `[P0-T10]` capture: **processed**
- This run: **processed**

Both are `processed`, so baseline and post-change `hits` values are like-for-like.

### [P0-T11] cross-check

The per-line maps for the five filenames were re-derived from `coverage/baseline.cobertura.processed.xml` with the same anchored, de-duplicated rule and compared against the `Baseline per-line hits` rows `[P0-T11]` recorded in `EVIDENCE/baseline/mstest-coverage.md`. The document is present and its 494 rows across the three instrumented filenames — 312 for `QfcQueue.cs`, 157 for `QfcFormController.SetupDisposal.cs`, 25 for `QfcRemainingQueueAdmission.cs` — reproduce exactly, with no `<filename>:<number>` pair disagreeing between the two sources. **Cross-check outcome: PASS.** The preserved baseline document is neither corrupted nor replaced.

### Scope of this gate

Stated plainly so that no reader takes this gate to cover all five production paths. After excluding the two uninstrumented files `QuickFiler/Controllers/QfcCollectionController.cs` and `QuickFiler/Controllers/QfcDatamodel.cs`, and the comment-only `QuickFiler/Helper Classes/EmailMoveMonitor.cs`, the changed-line comparison operates on `QuickFiler/Controllers/QfcQueue.cs`, `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` and `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs`, of which only `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` receives substantive new executable lines.

`QuickFiler/Helper Classes/EmailMoveMonitor.cs` is changed by `[P1-T4]` but is deliberately absent from the `git diff --unified=0` path list above and is excluded from this comparison, because its only change is the replacement of one comment line, which carries no Cobertura entry on either side.

### Changed-line coverage

One row per changed executable line in an instrumented file, in the form `<file>:<post_line> baseline_line=<n> baseline_hits=<n> post_hits=<n>`. Only the bare filename is recorded, never the Cobertura `filename` attribute's full value. Every `post_hits` value below was read from the de-duplicated per-line map built from `coverage/postchange.cobertura.processed.xml`.

The added post lines in the instrumented file are 207-209 and 221-249. Of those, 207, 208, 209, 223, 229 and 236 carry no Cobertura entry and are therefore not executable lines: 207 is the doc comment, 208 the uninitialised field declaration, 209 a blank line, and 223, 229 and 236 are brace-only lines. The remaining 26 are listed here.

```
QfcFormController.SetupDisposal.cs:221 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:222 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:224 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:225 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:226 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:227 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:228 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:230 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:231 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:232 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:233 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:234 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:235 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:237 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:238 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:239 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:240 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:241 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:242 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:243 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:244 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:245 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:246 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:247 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:248 baseline_line=none baseline_hits=n/a post_hits=1
QfcFormController.SetupDisposal.cs:249 baseline_line=none baseline_hits=n/a post_hits=1
```

Every one of the 26 rows carries `baseline_line=none baseline_hits=n/a`. The reason is hunk shape, not a missing document: the whole of the rewritten `Cleanup()` body arises from the one-line-replaced-by-many hunk `@@ -218 +221,29 @@`, whose pre-image range removes one line while its post-image range adds twenty-nine, and the `_undoQueueDisposal` field and its doc comment arise from the pure insertion hunk `@@ -206,0 +207,3 @@`, whose pre-image range is empty. Neither shape maps one-to-one, so no added line has a baseline counterpart.

The replaced line's baseline hits are deliberately **not** attributed to the lines that replaced it. `_undoQueue?.Dispose();` was covered at baseline, so a hunk-level mapping would have reported every uncovered new line in the rewritten `Cleanup()` as a regression on code that did not exist at baseline.

Every one of the 26 new executable lines is covered, with `post_hits=1`. That is a favourable observation about the new code but it is **not** a no-regression finding, for the reason recorded under `Comparable changed-line population:` below.

#### Newly added, no baseline counterpart

All 26 rows listed above fall under this sub-heading. A row carrying `baseline_line=none` cannot be a regression by construction and is excluded from the regression count.

#### Uninstrumented, not comparable

Changed executable lines in a file whose separator-anchored match selects **no** `class` element. Such a line has neither a baseline `hits` value nor a post-change one, because the coverage tool did not instrument the file at all.

| File | Post-change line | Reason |
|---|---|---|
| `QfcCollectionController.cs` | 993 | class-level `[ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcCollectionController.cs:21` |
| `QfcDatamodel.cs` | none — its only executable change is the pure deletion of pre-image lines 354-355, which has no post-change line number | class-level `[ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcDatamodel.cs:25` |

The `QfcCollectionController.cs` row is the reentrancy guard, at baseline line 991 and post-change line 993. **No coverage judgment of any kind is available for these lines** — neither that they regressed nor that they were already uncovered — because there is no Cobertura entry on either side to compare. They are excluded from the regression count **and** from the `Pre-existing uncovered, no regression` count.

`QfcDatamodel.cs` is recorded here explicitly rather than left unmentioned. It is uninstrumented for the same reason, and independently of that it adds no executable line at all: its construction-site change removes two pre-image lines and adds none, so it contributes no row with a post-change line number under any heading.

`QfcCollectionController.cs` is explicitly **not** recorded under `Pre-existing uncovered, no regression`. An earlier revision of this plan expected it there on the reasoning that the only tests entering `RemoveSpecificControlGroupAsync` throw several statements before the guard; that reasoning presumes a `hits=0` entry, and no such entry exists.

Both attributes are pre-existing on `origin/main`, neither is introduced by this change, and correcting either is out of scope for issue #731. This handling applies only to files with no `class` element; no line in an instrumented file was routed to this sub-heading.

#### Pre-existing uncovered, no regression

None. No changed executable line in an instrumented file had a baseline `hits` value at all, so no line qualifies for this sub-heading.

#### Comment-only added lines and deleted lines

These carry no Cobertura entry and are excluded from both counts. Listed separately:

| File | Change | Kind |
|---|---|---|
| `QfcCollectionController.cs` | post lines 83-84 added | one blank line plus one comment line |
| `QfcDatamodel.cs` | post lines 103-104 added | one blank line plus one comment line |
| `QfcDatamodel.cs` | pre-image lines 354-355 removed | deletions only |
| `QfcQueue.cs` | post lines 40-41 added | one blank line plus one comment line |
| `QfcFormController.SetupDisposal.cs` | post lines 207, 208, 209, 223, 229, 236 added | doc comment, uninitialised field declaration, blank line, and brace-only lines |
| `QfcRemainingQueueAdmission.cs` | pre-image line 5 and lines 16-17 and 23-27 removed | deletions only |

In each of the three comment insertions the blank line is formatter-mandated: CSharpier requires a blank line between a member declaration and a following comment. This was verified directly rather than assumed — the blank line was removed from `QuickFiler/Controllers/QfcQueue.cs` and `dotnet tool run csharpier check` reported the file as not formatted, showing the blank line in its expected output; the same held when the comment was rewritten in `///` doc-comment form.

### Comparable changed-line population: 0

That integer is the count of `Changed-line coverage` rows whose `baseline_hits` field carries a number rather than `n/a`. All 26 rows carry `n/a`, so the count is 0.

**Because that integer is 0, the recorded regression count of 0 is a consequence of the comparable population being empty, and this gate therefore produced no coverage observation on this change.** It did not find that there was no regression: an empty population supports no such finding, and a reader must not take the zero regression count as evidence that a comparison was made.

The emptiness is a property of the change shape rather than of the gate, and matches what the plan anticipated. The only one-to-one-shaped changed executable line in this plan is the reentrancy guard in `QuickFiler/Controllers/QfcCollectionController.cs`, and that file is uninstrumented; every added line in `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` arises from the one-line-replaced-by-many rewrite or from a pure insertion and so carries `baseline_line=none`; the `QuickFiler/Controllers/QfcQueue.cs` change is a single comment line; and the `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` change adds no line at all.

### Recorded regression count: 0

The count of changed executable lines whose `post_hits` is 0 while the corresponding baseline line had `baseline_hits` greater than 0 is **0**, computed over the comparable population described above, which is empty.

### Deferral reconciliation

`EVIDENCE/qa-gates/mstest-coverage.md` and `EVIDENCE/qa-gates/coverage-delta.md` were read for a recorded deferral of the no-regression judgment to this task. **Neither records one:** `[P5-T5]` recorded `Absolute floor result: PASS`, so its `FAIL` branch — the only branch that defers — was not taken; and `[P5-T6]` resolved Axis D to **D-COMPARABLE** and took Branch A, so Branch B, the only other branch that defers, was not taken. No deferral was made, and therefore no deferral landed on an empty population.

Supporting signal exists independently of this gate: the repository-wide comparison in `[P5-T6]` was admissible (Branch A) and passed its 0.005 bar, the whole-file `QfcFormController.SetupDisposal.cs` figure rose from 70.70 percent to 74.73 percent, and all 26 new executable lines in the rewritten `Cleanup()` are covered. Those are repository-wide, whole-file and new-code observations, not per-changed-line no-regression observations, and they are recorded as supporting evidence rather than as a substitute for the changed-line gate.
