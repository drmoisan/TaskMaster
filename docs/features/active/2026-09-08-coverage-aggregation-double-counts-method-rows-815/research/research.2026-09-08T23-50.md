# Issue #815 — Coverage aggregation double-counts method rows (Research)

- **Issue:** #815
- **Spec of record:** `docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/spec.md`
- **Timestamp:** 2026-09-08T23-50
- **Author:** task-researcher

---

## 1. Verdict (one line)

**Distinct site, and the issue's stated magnitude is refuted:** the `.//line` aggregation pinned in the issue-809 plan is *plan prose only* (no committed script code carries it), and measurement on two committed Cobertura documents shows it inflates the four **counts** by roughly 2x while leaving the derived **percentages** within 0.00 to 0.46 percentage points — so it cannot have produced the claimed 79.38 versus 77.03 branch swing of 2.35 points.

---

## 2. Tooling constraint affecting this research (stated up front)

The Bash tool is disabled for this session, including `pwsh`. The delegation brief asked me to execute both aggregations with `pwsh`. I could not.

**Substitute method used, and why it is exact rather than approximate.** Every committed Cobertura document in this repository is pretty-printed at two spaces per element depth, with exactly one `<line .../>` element per physical line. Element depth therefore maps one-to-one onto leading-space count:

| Node | Path | Leading spaces |
|---|---|---|
| `<package>` | `/coverage/packages/package` | 4 |
| `<class>` | `.../package/classes/class` | 8 |
| class-level `<line>` | `.../class/lines/line` | 12 |
| method-level `<line>` | `.../class/methods/method/lines/line` | 16 |

An anchored `Grep` in `count` mode over `^ {12}<line number=` and `^ {16}<line number=` therefore returns exact node counts for the two narrower selections, and their sum is the all-descendant `.//line` count.

**The method is self-validating.** Each document carries its own root `lines-covered` / `lines-valid` / `branches-covered` / `branches-valid` attributes, produced by an implementation independent of my tally (`dotnet-coverage` itself for a raw document; `Get-CoberturaCoverageSummary` for a post-processed one). In every case below my tally reproduces those root attributes exactly, which is a stronger oracle than re-running the pinned snippet would have been. Where a figure could not be validated against an independent oracle, it is marked as such.

No throwaway measurement script was written; no file outside the research path was created or modified.

---

## 3. CONFIRMED / REFUTED table for the five supplied findings

| # | Supplied finding | Status | Evidence |
|---|---|---|---|
| 1 | The pinned aggregation is inline plan prose, not committed script code | **CONFIRMED** | Snippet present at `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/plan.2026-09-07T20-14.md` lines 99-119 (quoted verbatim in §4). A repository-wide `Grep` for `\.//line` returns 81 files and **zero** under `scripts/`. |
| 2 | The committed script code already de-duplicates correctly | **CONFIRMED**, with two sub-hypotheses tested (see §6) | `Get-CoberturaClassLineSummary` (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:158-256`) unions `./lines/line` with `./methods/method/lines/line` into a hashtable keyed by line number. `Get-CoberturaPackageLineSummary` (`scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1:49-55`) accumulates it over `.//class`. `Get-CoberturaCoverageSummary` (`Helpers.ps1:101-135`) sums one package summary per `./package`. No residual double-count found. |
| 3 | The raw Cobertura report for issue 809 is NOT committed | **CONFIRMED** | `Glob` over `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/**/*` returns 52 files, all `.md`. There is no `.xml` anywhere in that tree. `evidence/baseline/p0-t13-coverage.md:11` states the reason: the report was written to `coverage/`, which `.gitignore:144` excludes, and "the numeric findings recorded here are the retained evidence". |
| 4 | Committed Cobertura documents exist that can serve as a fixture basis | **CONFIRMED** (list enumerated in §5.1; more than 100 exist) | Two were measured end to end; see §5. |
| 5 | Pester tests live under `tests/scripts/vscode/`, not `tests/scripts/powershell/` | **CONFIRMED** | `Glob` `tests/scripts/**/*.ps1` returns exactly 11 files, all under `tests/scripts/vscode/`. There is no `tests/scripts/powershell/` directory. |

---

## 4. The pinned snippet, verbatim

From `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/plan.2026-09-07T20-14.md`. Line 99 carries the prose; lines 101-117 carry the fenced block; line 119 carries the trailing note.

> **Coverage counting method (pinned; every coverage task in this plan reproduces it).** Cobertura `<package>` elements produced by `dotnet-coverage` carry `line-rate` and `branch-rate` but carry no `lines-covered`, `lines-valid`, `branches-covered` or `branches-valid` attributes, so those four figures are aggregated from `<line>` elements and the denominator depends entirely on the selection. **The selection is the all-descendant `.//line` selection over each first-party `<package>`, and only that one.** The two narrower selections `classes/class/lines/line` and `classes/class/methods/method/lines/line` are rejected by name and must not be substituted. A `<line>` counts as covered when its `hits` attribute is greater than zero. Branch figures are summed from the `(numerator/denominator)` pair inside each `condition-coverage` attribute over the same line set. The first-party allowlist is the nine production assembly names `Tags`, `ToDoModel`, `TaskVisualization`, `UtilitiesCS`, `QuickFiler`, `TaskTree`, `TaskMaster`, `SVGControl`, `VBFunctions`. The aggregation snippet is:

```powershell
$doc = New-Object System.Xml.XmlDocument
$doc.Load((Resolve-Path -LiteralPath $CoberturaPath).Path)
$firstParty = @('Tags','ToDoModel','TaskVisualization','UtilitiesCS','QuickFiler','TaskTree','TaskMaster','SVGControl','VBFunctions')
$lc = 0; $lv = 0; $bc = 0; $bv = 0
foreach ($pkg in $doc.SelectNodes('/coverage/packages/package')) {
    if ($firstParty -notcontains $pkg.GetAttribute('name')) { continue }
    foreach ($ln in $pkg.SelectNodes('.//line')) {
        $lv++
        $h = $ln.GetAttribute('hits')
        if ($h -and [int]$h -gt 0) { $lc++ }
        $cc = $ln.GetAttribute('condition-coverage')
        if ($cc -and $cc -match '\((\d+)/(\d+)\)') { $bc += [int]$Matches[1]; $bv += [int]$Matches[2] }
    }
}
"LINES_COVERED=$lc LINES_VALID=$lv BRANCHES_COVERED=$bc BRANCHES_VALID=$bv"
```

> `GetAttribute` is used rather than property access so a `<line>` lacking an attribute yields an empty string instead of throwing under `Set-StrictMode`.

Note that the snippet's branch rule differs from the repository helper's in a second way beyond the axis: it accumulates a branch whenever a `condition-coverage` attribute is present, whereas `Get-CoberturaClassLineSummary:243` accumulates only when the retained entry carries `branch="True"`. On the two documents measured, every `condition-coverage` carrier also carried `branch="True"`, so this difference did not bite; it remains a latent divergence.

One procedural caveat: the brief asked me to retrieve this text with `git show origin/main:...`. Git is only reachable through the disabled Bash tool, so I read the file from the working tree instead. The session began with a clean `git status` on a branch cut from `main`, and this feature has not modified that plan, so the working-tree text and the `origin/main` text are expected to be identical — but I did not verify that equality directly, and it is recorded here as unverified.

---

## 5. The empirical core — measured side-by-side aggregations

### 5.1 Committed Cobertura documents (enumeration)

`Glob` `**/*cobertura*.xml` returns more than 100 matches and truncates. Confirmed present, among others:

- `docs/features/active/2026-08-24-breadcrumb-router-navigation-defects-498/evidence/qa-gates/p8-t5-coverage.cobertura.xml` and its `evidence/baseline/p0-t15-coverage.cobertura.xml` sibling
- `docs/features/active/2026-08-24-qfc-collection-controller-defects-468/evidence/qa-gates/coverage-final.cobertura.xml` and its baseline sibling
- `docs/features/active/2026-08-24-breadcrumb-coordinator-hub-defects-501/evidence/baseline/baseline.cobertura.2026-08-27T20-01.xml` and `evidence/qa-gates/postchange.cobertura.2026-08-27T23-31.xml`
- `docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/baseline/issue-439-baseline.cobertura.xml` plus three `.normalized.` variants under `evidence/qa-gates/`
- `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/baseline/baseline.cobertura.xml` and `evidence/qa-gates/postchange.cobertura.xml`
- `docs/features/archive/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/baseline/coverage-remeasurement-run{1,2,3}.{raw,corrected}.cobertura.xml`
- `docs/features/archive/2026-07-18-utilitiescs-nullable-helperclasses-364/evidence/baseline/coverage-baseline.2026-07-19T08-51.cobertura.xml`
- many more under `docs/features/archive/2026-07-04-coverage-gaps-test-seams-236/`, `.../2026-07-21-quickfiler-folder-selector-dropdown-400/`, `.../2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/`

**Caution on the #494 pair.** Despite their names, `coverage-remeasurement-run1.raw.cobertura.xml` and `coverage-remeasurement-run1.corrected.cobertura.xml` carry byte-identical root attributes (`lines-covered="53381" lines-valid="62401" branches-covered="12546" branches-valid="15872"`) and both are post-processed (relative filenames, `<sources><source>.</source>`). The `.raw.`/`.corrected.` naming there does not denote the raw-versus-post-processed distinction and must not be used as such a fixture pair.

**How raw and post-processed were distinguished.** A raw `dotnet-coverage` document has absolute `filename` attributes and no `<sources>` element. A post-processed `ConvertTo-KoverageCoberturaXml` document has repository-relative `filename` attributes with backslash separators and a `<sources><source>.</source></sources>` block inserted before `<packages>`.

### 5.2 Document A — POST-PROCESSED

`docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/qa-gates/postchange.cobertura.xml`

Classification: **post-processed**. Line 9 shows `filename="QuickFiler\Controllers\EfcHomeController.cs"` (relative, backslash) and lines 3-5 show `<sources><source>.</source></sources>`. Nine `<package>` elements, exactly the nine first-party names, so the whole document already equals the plan's first-party scope.

Node counts (`Grep` count mode):

| Selection | Pattern | Count |
|---|---|---|
| class-level `lines/line` | `^ {12}<line number=` | 64,406 |
| method-level `methods/method/lines/line` | `^ {16}<line number=` | 50,405 |
| all-descendant `.//line` | `<line number=` | 114,811 |

64,406 + 50,405 = 114,811, so the two narrower selections partition the descendant set exactly and no `<line>` sits at any other depth.

Aggregation results:

| Quantity | (a) Pinned `.//line` method | (b) De-duplicated | Delta (a − b) |
|---|---|---|---|
| LINES_COVERED | 98,553 | 54,988 | +43,565 |
| LINES_VALID | 114,811 | 64,406 | +50,405 |
| Line % | 85.8392 | 85.3771 | **+0.4621 pp** |
| BRANCHES_COVERED | 23,249 | 13,120 | +10,129 |
| BRANCHES_VALID | 29,162 | 16,524 | +12,638 |
| Branch % | 79.7236 | 79.3997 | **+0.3239 pp** |

The de-duplicated column is the document's own root attributes (`lines-covered="54988" lines-valid="64406" branches-covered="13120" branches-valid="16524"`, `branch-rate="0.793997"`), which `ConvertTo-KoverageCoberturaXml` wrote from `Get-CoberturaCoverageSummary`. My independent class-level tally reproduces all four exactly, which is the cross-check.

### 5.3 Document B — RAW

`docs/features/archive/2026-07-18-utilitiescs-nullable-helperclasses-364/evidence/baseline/coverage-baseline.2026-07-19T08-51.cobertura.xml`

Classification: **raw `dotnet-coverage` output**. Line 6 shows an absolute `filename` under a worktree path; there is no `<sources>` element. Contains test packages (the first `<package name="UtilitiesCS.Test">`), so the figures below are whole-document and are not first-party-filtered. That does not affect the comparison, which is between two selections over one identical population.

Node counts:

| Selection | Count |
|---|---|
| class-level | 136,359 |
| method-level | 137,194 |
| all-descendant | 273,553 |

136,359 + 137,194 = 273,553. Here the method-level set is 835 nodes *larger* than the class-level set, which is the intra-class repeated-line-number shape (one source line emitted into several methods of the same class, the field-initializer-across-constructors case confirmed at #670).

Aggregation results:

| Quantity | (a) Pinned `.//line` method | (b) Class-level rollup (= document root) | Delta (a − b) |
|---|---|---|---|
| LINES_COVERED | 197,072 | 98,272 | +98,800 |
| LINES_VALID | 273,553 | 136,359 | +137,194 |
| Line % | 72.0416 | 72.0686 | **−0.0270 pp** |
| BRANCHES_COVERED | 24,582 | 12,291 | +12,291 |
| BRANCHES_VALID | 50,732 | 25,366 | +25,366 |
| Branch % | 48.4546 | 48.4546 | **0.0000 pp** |

Root attributes: `line-rate="0.7206858366517795" branch-rate="0.4845462430024442" lines-covered="98272" lines-valid="136359" branches-covered="12291" branches-valid="25366"`. Every one of the four is reproduced exactly by the class-level tally.

On this document the branch numerator and denominator are each *exactly* doubled (12,291 → 24,582 and 25,366 → 50,732), so the branch percentage is bit-for-bit unchanged.

### 5.4 Third, independent corroboration (recorded, not measured by me)

`.claude/agent-memory/feature-review/project_791-review-residuals.md:39-43` records the same comparison for issue #791's run:

> Baseline 55587/65783 = 84.50% line, 13204/16684 = 79.14% branch.
> Post-change 55783/66009 = 84.51% line, 13292/16784 = 79.19% branch.
> The delivery's `.//line` all-descendant selection reports ~2x those counters (112551/133187) — the [[cobertura-class-line-double-count-trap]] — but **the derived percentages match to the digit under both selections**, which is the useful cross-check.

112,551 / 133,187 = 84.506% against the de-duplicated 55,783 / 66,009 = 84.508%.

### 5.5 The repository's own committed test says the same thing

`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:209-211`:

```
It 'counts each branch line once when methods repeat the class-level rollup' {
    # Regression for Issue #441 branch arithmetic. The branch RATIO is unchanged by the
    # double count, so this must assert branches-valid/branches-covered, never branch-rate.
```

The `#441` author reached the same conclusion I measured, and encoded it as the reason the existing regression test asserts counts rather than rates.

---

## 6. Answering the central question, and the branch-arithmetic question

### 6.1 Is #815 a regression of the #670 class-level trap, or a distinct site?

**Distinct site.** The #670 trap and #815 share one mechanism — Cobertura repeats each source line under `<class><lines>` and again under `<class><methods><method><lines>` — but they are at different layers and only one of them is in code:

- The #670/#441/#478 defect was in **committed script code** (`Get-CoberturaCoverageSummary` overwriting correct root totals with a descendant-axis sum). It landed and is fixed; §6.2 re-verifies the fix.
- #815's site is **prose inside one delivered plan document**, at `plan.2026-09-07T20-14.md:99-119`. It is not a regression of the code fix, because it never called the code. It is a hand-rolled aggregation that reintroduces the same axis error in a document, and which the plan then explicitly forbids substituting ("rejected by name and must not be substituted").

### 6.2 Is the committed script code free of the double-count, and does it carry any distinct residual defect?

Free of the double count: **verified**, by the identity in §5.2 — the post-processed document's root attributes, written by `Get-CoberturaCoverageSummary`, equal the class-level rollup count (64,406 / 54,988 / 16,524 / 13,120) and not the descendant count (114,811 / 98,553 / 29,162 / 23,249).

Two named sub-hypotheses, plus one found independently:

**H1 — `Get-CoberturaPackageLineSummary` uses the descendant axis `.//class`; can `<class>` nest?**
**Not a live defect.** Every `<class>` element in both measured documents sits at depth 8, i.e. directly under `classes`:

| Document | `<class ` total | `^ {8}<class ` | Nested |
|---|---|---|---|
| A (post-processed) | 560 | 560 | 0 |
| B (raw) | 5,267 | 5,267 | 0 |

`.//class` and `./classes/class` are therefore equivalent on real input. The Cobertura schema does not nest `<class>`, so this is a stylistic difference rather than a latent arithmetic risk. No change recommended.

**H2 — can one `filename` appear under more than one `<package>`, entering the document total once per package?**
**Not observed.** In Document A the nine packages occupy disjoint line ranges (`<package>` at physical lines 7, 32875, 164153, 169112, 175329, 181290, 183904, 193001, 194002). All 385 `<class>` elements whose `filename` begins `UtilitiesCS\` fall between physical lines 32,877 and 160,648 — entirely inside the `UtilitiesCS` package range of 32,875-164,152. Zero leakage.

Scope of this check, stated honestly: verified for the `UtilitiesCS\` prefix on one document. It is the highest-risk prefix (largest package, most likely to be linked into a sibling project), but the other eight prefixes and the other documents were not checked. If the fix wants to be robust against this, keying the document-level union by `(filename, line number)` rather than summing per package would close it at no cost — but there is currently **no evidence that it occurs**, and I do not recommend adding that complexity on a hypothetical.

**H3 — found independently: the pinned snippet's branch predicate is wider than the helper's.**
The snippet accumulates a branch for any `<line>` carrying a `condition-coverage` attribute. `Get-CoberturaClassLineSummary:243` accumulates only for a retained entry with `branch="True"`. On both measured documents every `condition-coverage` carrier also carried `branch="True"`, so the two agree today. This is a latent divergence in the prose method, not a defect in the script code, and it is a second reason to replace the prose with a call into the tested helper.

### 6.3 Branch arithmetic: why is the branch overstatement NOT larger than the line overstatement?

The brief asked me to explain why the branch overstatement is larger in proportional terms, or to show that it is not. **It is not — and on a raw document it is exactly zero.**

The mechanism. Under de-duplication, a line number appearing in both the class rollup and a method block collapses to one entry, and the retained `condition-coverage` pair is the one with the larger denominator. Under the naive selection, both pairs are added. When the two pairs are identical — which is the overwhelmingly common case, because the method block and the class rollup are two views of the same instrumentation — the naive method adds `(c/d)` twice where the correct answer is `(c/d)` once. Doubling numerator and denominator by the same factor leaves the ratio invariant. That is why Document B's branch rate is unchanged to the seventh decimal.

The ratio only moves when the duplication factor differs between covered and uncovered branches. That happens on Document A (a post-processed document, where `Remove-CoberturaExemptClosureCoverage` and `Merge-CoberturaClassesByFilename` have already reshaped the method blocks so the method-level set is a strict 50,405-node subset of the 64,406-node class set rather than a near-copy of it), and it moves the branch rate by +0.3239 pp — smaller, not larger, than the +0.4621 pp line movement on the same document.

**Consequence for #815's headline claim.** A 2.35-percentage-point branch swing cannot come from this mechanism. Across three independent runs (Documents A and B here, plus the #791 record) the naive-versus-de-duplicated percentage gap is 0.00, 0.32 and 0.00 points on branches. The 809 plan's reported figures are 113,361 / 134,023 lines and 26,880 / 33,880 branches — counts roughly 2x the true population, with percentages (84.58 and 79.34) that are very close to correct.

**Where 77.03 came from is unknown.** It cannot be reconstructed: the raw report is not committed (finding 3), and the recomputation method is not recorded anywhere in the issue, the spec, or the 809 evidence tree. Candidate explanations that I could not test include a different package set, a `(filename, line)` union across classes, or a figure taken after `ConvertTo-KoverageCoberturaXml` post-processing (which changes *which* lines are in the population, not merely their multiplicity, because it strips exempt-closure coverage and merges classes by filename). I am recording this as **unknown**, and recommending in §9 that the fix's acceptance not be anchored on reproducing 77.03.

---

## 7. Propagation surface (item 6), classified

`Grep` for `\.//line` across the whole repository returns **81 files**. `Grep` for `all-descendant|SelectNodes\('\.//line'\)|rejected by name` returns 42. Combined and classified:

### (c) Production script code — ZERO files

There is no `.//line` anywhere under `scripts/`. `Grep` for `\.//line|all-descendant|first-party|aggregation` under `.claude/rules/` returns only unrelated analyzer-wiring prose in `csharp.md`. `Grep` for `\.//line|SelectNodes|condition-coverage|lines-valid` in `.claude/skills/csharp-qa-gate/SKILL.md` returns no matches, and no `.claude/skills/**/SKILL.md` pins an aggregation method.

**This is the single most important propagation finding: no skill, no rule, no CI workflow and no script pins the defective method.** The write set is correspondingly small.

### (b) Live carriers that will propagate the defect into future plans

These are agent-memory records that a future planner or executor will read and act on.

| Path | Why it propagates |
|---|---|
| `.claude/agent-memory/atomic-executor/project_coverage_delta_reproduce_baseline_counting_method.md` | Lines 14-15 present "Deduped / class-level only" and "All `<line>` descendants" as two equally "defensible methods", and line 17 records that the #418 baseline used the all-descendant one. Its guidance is "reproduce whatever the baseline used", which legitimises continuing to use the wrong axis. Already carries a 2026-09-02 correction about raw versus post-processed, but not about axis correctness. |
| `.claude/agent-memory/atomic-executor/project_coverage_firstparty_denominator_method.md` | Line 13 asserts "per-line counting is the only reproducible method" and line 26 says "sum `<line>` hits/totals across every package" **without naming an axis**. Ambiguous in exactly the direction that produced #815. |

### (b-correct) Live carriers that already state the correct rule — leave in place, and cite

| Path | Content |
|---|---|
| `.claude/agent-memory/feature-review/project_cobertura-class-line-double-count-trap.md` | "Use the direct child path `lines/line` on the `<class>` node, not `.//line`." Confirmed at #670. |
| `.claude/agent-memory/atomic-executor/project_cobertura_package_rollup_must_use_repo_helper.md` | Names the remedy directly: dot-source `Helpers.ps1` and call `Get-CoberturaPackageLineSummary`. This memory is effectively the fix already written down. |
| `.claude/agent-memory/atomic-executor/project_async_state_machine_emits_no_method_element.md:12` | "`.//line` under a `<class>` double-counts." |
| `.claude/agent-memory/task-researcher/project_cobertura_root_attrs_raw_vs_postprocessed.md` | Raw-versus-post-processed root-attribute distinction. |
| `.claude/agent-memory/orchestrator/coverage-mode-raw-vs-processed-is-flake-sensitive.md:67-69` | Records the 25 = 13 + 12 partition on a committed #439 artifact. |
| `.claude/agent-memory/atomic-planner/project_678_carry_folder_predictor_plan_seams.md:33` | Section headed "Never count Cobertura `.//line`". |
| `.claude/agent-memory/atomic-planner/project_731_lifecycle_disposal_plan_seams.md`, `project_781_excludefromcodecoverage_guard_plan_seams.md`, `project_791_hc_deadline_cancel_teardown_plan_seams.md` | Per-issue planner seams that cite the trap correctly. |

**Ownership caveat.** `.claude/skills/`, `.claude/rules/` and most of `.claude/**` are push-down-owned from the `drm-copilot` repository and are overwritten with zero templating, so a defect in those must be fixed upstream rather than here. Fortunately none of them carries the pinned method, so this constraint does not bind. `.claude/agent-memory/` is agent-written and repository-local, so editing the two carriers above is in-bounds — but see §9 for why editing them is *optional* relative to the durable fix.

### (a) Historical evidence artifacts — must NOT be rewritten

The remaining roughly 70 files. These are the delivered record of what past runs actually computed; rewriting them would falsify an audit trail. They include:

- The 809 plan itself and its whole `evidence/` tree (`p0-t13-coverage.md`, `p5-t5-tests-coverage.md`, `p6-t1`, `p6-t2`, `p6-t3-aggregate-coverage.md`, `policy-audit.2026-09-08T01-35.md`)
- The whole `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/` tree (10 files)
- `docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/` (6 files)
- `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/plan.2026-09-07T21-59.md` and its `evidence/qa-gates/p7-t5-tests-coverage.md`
- `docs/features/active/2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799/plan.2026-09-06T22-01.md`
- `docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/plan.2026-09-05T10-49.md`
- `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/` plans and per-file baselines
- `docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/`, `.../735/`, `.../729/`, `.../647/`, `.../511/`
- All of `docs/features/archive/` (441, 457, 494, 503, 508)
- `docs/features/epics/build-ci-coverage-gate-fidelity/epic.md` and its audit
- `docs/features/potential/promoted/2026-08-10-agent-memory-cobertura-dedup-generalization-wrong.md` (issue #532)

**The (a)/(b) boundary that determines the write set:** an artifact under `evidence/`, or a plan/audit/review document describing a run that already happened, is history. An agent-memory file under `.claude/agent-memory/` is forward-looking instruction. Only the latter can propagate.

---

## 8. Why the durable fix belongs in script code (item 7)

### 8.1 Can `Get-CoberturaCoverageSummary` already serve the plan use case?

**No — a new function is required.** Its full parameter list (`Helpers.ps1:103-107`) is a single mandatory `[xml]$XmlDocument`. It sums every `./package` unconditionally and offers no allowlist. On a *post-processed* document that is harmless, because `ConvertTo-KoverageCoberturaXml` has already removed non-allowlist packages. But the plan use case reads the **raw** collector output directly (the 809 plan loads `$CoberturaPath` straight from `dotnet-coverage collect`), which still contains the nine `.Test` packages and every instrumented third-party assembly. Passing a raw document to `Get-CoberturaCoverageSummary` would silently include all of them.

Nor does it emit percentages: it returns `LineRate` and `BranchRate` as 0-to-1 strings, while plan artifacts quote two-decimal percentages.

### 8.2 File-size ceiling and placement

Physical line counts. Because `pwsh` was unavailable I could not run `(Get-Content -LiteralPath <path>).Count`; these are the last line numbers reported by a full `Read` of each file, which equals the physical line count for a file whose final line is terminated.

| File | Lines | Headroom to 500 |
|---|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 469 | 31 |
| `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 413 | 87 |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 351 | 149 |
| `scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` | 65 | 435 |
| `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | 56 | 444 |

`Helpers.ps1` has 31 free lines. A new function following this repository's established convention carries a comment-based help block of roughly 25-35 lines before any executable statement, so it cannot fit there.

**The repository has already solved this twice, and documented the reasoning in-code.** `PackageRate.ps1:20-23`: "This function lives in its own file rather than alongside its callers in `Invoke-MSTestWithCoverage.Helpers.ps1` because that file is already within a few lines of the repository's 500-line ceiling. Helpers.ps1 dot-sources this file, so a caller that dot-sources Helpers.ps1 alone still resolves this function." `Threshold.ps1:14-17` says the same. Follow that precedent: a new sibling file, dot-sourced from `Helpers.ps1` line 2-4 (one added line, 469 → 470).

### 8.3 Proposed function

New file `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1`:

```
Get-CoberturaFirstPartyCoverageSummary
    -XmlDocument   [xml]       (mandatory)
    -ProjectNames  [string[]]  (optional; defaults to Get-KoverageProjectAllowlist)
```

Behaviour: iterate `/coverage/packages/package`, skip any whose `name` is not in `ProjectNames`, and accumulate `Get-CoberturaPackageLineSummary` for each retained package. Emit `LineRate`, `BranchRate`, `LinesCovered`, `LinesValid`, `BranchesCovered`, `BranchesValid` in the existing string shape, plus `LinePercent` and `BranchPercent` as two-decimal strings for direct quotation in a plan artifact.

Design notes:
- It reuses `Get-CoberturaPackageLineSummary` rather than re-deriving arithmetic, so the de-duplication rule, the rounding, and the `'0'` zero-denominator fallback stay defined in exactly one place. This is the same delegation `Merge-CoberturaClassesByFilename:399` already performs.
- Defaulting `ProjectNames` to `Get-KoverageProjectAllowlist` removes the need for plan authors to hard-code the nine names, which is itself a drift hazard: the plan's list is a frozen literal, whereas the allowlist is derived from the project files on disk.
- It is pure — no I/O, no mutation of the input document — matching the contract stated for the two sibling helpers.
- It works on raw and post-processed documents alike, which is the property the plan use case needs.

Plan prose would then read: dot-source `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and call `Get-CoberturaFirstPartyCoverageSummary -XmlDocument $doc`, instead of pasting a snippet.

---

## 9. Falsifiable acceptance condition (item 8)

### 9.1 Fixture form: in-memory XML literal, not a committed artifact

Recommended: an **in-memory here-string XML literal**. Reasons:

1. It is the unanimous established idiom. All eleven assertions in `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` and both in `Invoke-MSTestWithCoverage.PackageRate.Tests.ps1` use a `@'...'@` here-string cast to `[xml]`. Not one reads a file.
2. `.claude/rules/general-unit-test.md` prohibits temporary files in tests outright, and a committed 200,000-line Cobertura artifact would make the test slow and the failure message unreadable.
3. A hand-sized fixture lets the expected numbers be stated exactly and checked by inspection, which satisfies the "clear, actionable failure messages" requirement.

The committed documents remain valuable — but as *research* corroboration (§5), not as unit-test input.

### 9.2 The fixture

The shape is the one confirmed at #670: a field-initializer line emitted into several constructors. Two branch lines are used, duplicated at *different* multiplicities, so that both the line rate and the branch rate diverge between the two methods. A fixture duplicating everything uniformly would leave the branch rate invariant (§6.3) and the test would not discriminate.

One package `Ns`, one class `Ns.Foo`, `filename="Ns\Foo.cs"`:

- class-level `<lines>`: line 20 `hits="1" branch="True" condition-coverage="100% (2/2)"`; line 30 `hits="0" branch="True" condition-coverage="0% (0/2)"`
- `<methods>`: `.ctor ()`, `.ctor (int)` and `.ctor (string)` each carrying line 20 with `hits="1" branch="True" condition-coverage="100% (2/2)"`; and `M ()` carrying line 30 with `hits="0" branch="True" condition-coverage="0% (0/2)"`

Total `<line>` nodes: 2 class-level + 3 + 1 method-level = 6.

### 9.3 Computed pre-fix and post-fix numbers

| Quantity | Pre-fix (pinned `.//line` snippet) | Post-fix (`Get-CoberturaFirstPartyCoverageSummary`) |
|---|---|---|
| LINES_COVERED | 4 | 1 |
| LINES_VALID | 6 | 2 |
| LineRate | `0.666667` | `0.5` |
| BRANCHES_COVERED | 8 | 2 |
| BRANCHES_VALID | 12 | 4 |
| BranchRate | `0.666667` | `0.5` |

Derivations. Pre-fix lines: six `<line>` nodes; four carry `hits="1"` (class line 20 plus three constructor copies of line 20), two carry `hits="0"` (class line 30 and `M`'s line 30) — so 4/6. Pre-fix branches: line 20 appears four times at `(2/2)` giving 8/8, line 30 twice at `(0/2)` giving 0/4 — so 8/12. Post-fix: the union keyed by line number is `{20, 30}`; line 20 is covered and line 30 is not, giving 1/2; the retained condition-coverage pairs are `(2/2)` and `(0/2)`, giving 2/4.

Both rates move from `0.666667` to `0.5`, so the assertion discriminates on the rate as well as on all four counts.

### 9.4 The acceptance condition

- **Test file:** `tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1`
- **Describe block:** `Get-CoberturaFirstPartyCoverageSummary`
- **Test name:** `counts a line repeated across constructor rows once, in both the line and the branch totals`
- **Assertions:** `LinesValid` is `'2'`; `LinesCovered` is `'1'`; `LineRate` is `'0.5'`; `BranchesValid` is `'4'`; `BranchesCovered` is `'2'`; `BranchRate` is `'0.5'`
- **Demonstrable pre-fix failure:** the pinned snippet applied to this fixture yields `LINES_COVERED=4 LINES_VALID=6 BRANCHES_COVERED=8 BRANCHES_VALID=12`. Every one of the six assertions fails.

A second, complementary test is recommended and is *not* satisfiable by rate alone:

- **Test name:** `excludes a package outside the supplied first-party allowlist from both totals`
- **Fixture:** the above package `Ns` plus a second `<package name="Ns.Test">` carrying three covered lines
- **Call:** `-ProjectNames @('Ns')`
- **Assertion:** `LinesValid` is still `'2'` and `LinesCovered` still `'1'` — the three test-package lines enter neither total
- **Pre-fix failure:** the pinned snippet uses a hard-coded nine-name literal that does not contain `Ns`, so it returns all-zero totals on this fixture and cannot express the parameterised behaviour at all

---

## 10. Numeric Derivation Evidence

This section covers the node-count and coverage-counter claims asserted in §5 and §9.3.

### Claim group 1 — the `<line>` node population of Document A

- **Complete Family:** every `<line>` element in `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/qa-gates/postchange.cobertura.xml`, at every depth.
- **Exhaustive Search Scope:** the whole file. Cobertura permits `<line>` only under `class/lines` and `class/methods/method/lines`; both were enumerated, and the unanchored total confirms no third location exists.
- **Inclusion Rules:** any element whose serialisation begins `<line number=`.
- **Exclusion Rules:** `</line>` closing tags and `<condition>` children are not matched by the pattern.
- **Primary Search Strategy:** depth-anchored `Grep` count at each of the two legal depths — `^ {12}<line number=` = 64,406 and `^ {16}<line number=` = 50,405, sum 114,811.
- **Primary Member Set:** the 64,406 class-level nodes and the 50,405 method-level nodes, disjoint by construction (different leading-space prefixes).
- **Primary Count:** 114,811.
- **Cross-check Search Strategy:** an unanchored, depth-agnostic `Grep` count of `<line number=` over the same file — a structurally different query that makes no assumption about indentation.
- **Cross-check Member Set:** every `<line>` element in the file regardless of depth.
- **Cross-check Count:** 114,811.
- **Member-set Comparison:** the two counts are equal, and the anchored partition sums exactly to the unanchored total (64,406 + 50,405 = 114,811). Equality of the sum with the depth-agnostic total proves the two anchored sets are disjoint and jointly exhaustive, so no node is double-counted or omitted. The same construction on Document B gives 136,359 + 137,194 = 273,553 against an unanchored 273,553.

### Claim group 2 — branch counters (BRANCHES_VALID / BRANCHES_COVERED)

- **Complete Family:** every `<line>` carrying a `condition-coverage` attribute, at each of the two depths, in both measured documents.
- **Exhaustive Search Scope:** all denominators actually present, established by partitioning rather than by sampling. For Document A: denominators 2, 4, 5, 6, 7, 8 enumerated by count, denominator 3 and 9 confirmed zero, and all double-digit denominators enumerated individually by content. For Document B: denominators 2, 4, 6, 8 by count; 3, 5, 7, 9 and all double-digit denominators enumerated individually by content (67 members, listed identically at both depths).
- **Inclusion Rules:** depth-anchored `<line ...>` with a `condition-coverage="…(n/d)"` attribute.
- **Exclusion Rules:** `<line>` elements with no `condition-coverage` attribute contribute nothing.
- **Primary Search Strategy or Query Expression:** partition by denominator, then sum `count × denominator`. Document B class level: 9,239×2 + 1,113×4 + 198×6 + 62×8 + 752 (the 67 enumerated odd/double-digit members) = 25,366.
- **Primary Member Set:** the 10,679 class-level branch lines of Document B, partitioned as 9,239 + 1,113 + 198 + 62 + 20 + 47.
- **Primary Count:** BRANCHES_VALID 25,366.
- **Cross-check Search Strategy or Query Expression:** the document's own root `branches-valid` attribute, emitted by `dotnet-coverage` (Document B) or by `Get-CoberturaCoverageSummary` (Document A) — an entirely independent implementation that never saw my partition.
- **Cross-check Member Set:** the generator's own internal branch population for the same run.
- **Cross-check Count:** `branches-valid="25366"` (Document B); `branches-valid="16524"` (Document A).
- **Member-set Comparison:** the two agree exactly in both documents. The partition is additionally self-validating: the per-denominator counts sum to the independently measured total branch-line count (Document A class level 5,432 + 848 + 15 + 166 + 3 + 75 + 42 = 6,581, matching the measured 6,581; method level 4,297 + 642 + 13 + 107 + 3 + 55 + 24 = 5,141, matching 5,141; Document B class level 9,239 + 1,113 + 198 + 62 + 20 + 47 = 10,679, matching 10,679; method level 9,285 + 1,093 + 196 + 62 + 20 + 47 = 10,703, matching 10,703). The same construction applied to numerators reproduces `branches-covered="12291"` (Document B) and `branches-covered="13120"` (Document A) exactly.

### Claim group 3 — the §9.3 fixture counters

- **Complete Family:** the six `<line>` elements of the proposed fixture.
- **Exhaustive Search Scope:** the fixture is authored in full in §9.2; every element is enumerated there.
- **Inclusion / Exclusion Rules:** as §9.2 states.
- **Primary Search Strategy:** hand evaluation of the pinned snippet's loop over all six nodes.
- **Primary Member Set / Count:** LINES 4/6, BRANCHES 8/12.
- **Cross-check Search Strategy:** hand evaluation of `Get-CoberturaClassLineSummary`'s union-by-line-number algorithm as written at `Helpers.ps1:190-247`, independently of the snippet.
- **Cross-check Member Set / Count:** union `{20, 30}`; LINES 1/2, BRANCHES 2/4.
- **Member-set Comparison:** the two member sets differ by construction — that difference is the property the test asserts. Both were derived from the fixture text independently, and the pre-fix set is a strict multiset superset of the post-fix set. These figures are **not** validated against a tool run, because `pwsh` was unavailable; they are validated only by two independent hand evaluations against the two published algorithms. The implementing agent must confirm them by execution before the test is treated as passing.

---

## 11. Recommended write set

### Production

| Path | Change | New/modified |
|---|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` | `Get-CoberturaFirstPartyCoverageSummary` per §8.3, with comment-based help following the `PackageRate.ps1` pattern | **new** |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | one added dot-source line alongside the existing three at lines 2-4; 469 → 470 lines, within the 500 ceiling | modified |

### Test

| Path | Change | New/modified |
|---|---|---|
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1` | the two tests of §9.4, plus a zero-denominator boundary case mirroring `PackageRate.Tests.ps1:48-69` | **new** |

### Documentation / agent memory (optional, lower value — see §12)

| Path | Change |
|---|---|
| `.claude/agent-memory/atomic-executor/project_coverage_delta_reproduce_baseline_counting_method.md` | replace "two defensible methods" framing with a pointer to the new function; retain the historical #418 note as history |
| `.claude/agent-memory/atomic-executor/project_coverage_firstparty_denominator_method.md` | name the axis explicitly and point at the new function |

### Explicitly NOT in the write set

- Any file under `docs/features/*/evidence/`, any delivered `plan.*.md`, `policy-audit.*.md`, `feature-audit.*.md` or `code-review.*.md` — historical record (§7a).
- `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` — see §12.
- `.claude/skills/**`, `.claude/rules/**`, `CLAUDE.md` — none pins the method, and the first two are push-down-owned.

---

## 12. Threshold interaction (item 9)

**What the script actually enforces.** `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` defines exactly one gate, `Assert-CoberturaLineCoverageThreshold`. It reads the root `/coverage` `line-rate` attribute, rejects missing / non-numeric / out-of-range values with distinct messages, and throws when `line-rate × 100 < 80`. It is called once, at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:344`, on the **post-processed** document — i.e. on a number already produced by the de-duplicated `Get-CoberturaCoverageSummary` path.

There is **no branch-coverage gate anywhere in script code.**

**Would the corrected figures breach a threshold?**

| Gate | Source | Threshold | Measured against it |
|---|---|---|---|
| Script line gate | `Threshold.ps1:52` | 80% | Document A post-processed root `line-rate` = 85.3771%. Passes. Unaffected by this fix, because the gate never consumed the plan prose. |
| `CLAUDE.md` UT2 | repository-wide line | 80% | Same figure. Passes. |
| `.claude/rules/general-unit-test.md` | line | **85%** | 809's first-party line figure is 84.62% (naive) and would be roughly the same de-duplicated. **Below the 85% floor.** |
| `.claude/rules/general-unit-test.md` / `quality-tiers.md` | branch | **75%** | 809's first-party branch figure is 79.38% naive; §6.3 shows the de-duplicated figure is within a third of a point of that. Passes with margin. |

**FINDING, recorded and not actioned:** the repository's first-party line coverage sits below the 85% line floor stated in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`, both before and after this correction, while the only automated gate enforces 80%. Two separate written line floors (80% in `CLAUDE.md`, 85% in the rules files) are in force simultaneously, and the automated gate implements the lower one. This predates #815 and is not caused by the aggregation defect. **No threshold may be lowered, weakened or deleted as part of this work.** If it is to be reconciled, that is a separate promotion — note that `docs/features/archive/2026-08-10-coverage-threshold-policy-reconciliation-494/` already covers this ground and should be read first.

**Consequence for #815's scope.** Because the branch percentage barely moves under correction (§6.3) and the enforced gate is a line gate on an already-de-duplicated number, correcting the aggregation changes **no gate verdict on any past or present item**. The value of the fix is that count-based comparisons become correct and reproducible, not that a gate starts catching something it missed.

---

## 13. Out-of-scope items

1. **The `CLAUDE.md` CUT3 step 4 wording mismatch.** `CLAUDE.md` names `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` while the actual route is `dotnet-coverage collect ... -- <vstest> ...`. The mismatch is real and is corroborated in code: `scripts/vscode/Invoke-MSTestWithCoverage.ps1:19-26` records that `/EnableCodeCoverage` is deliberately omitted because the outer `dotnet-coverage` instrumentation conflicts with the built-in collector, and `scripts/vscode/TaskMaster.cli.runsettings` carries no data collector. **This feature is under a hard constraint not to modify `CLAUDE.md`.** Recommendation: raise it through the MCP promotion lifecycle as its own issue. **Bearing on the aggregation defect: none.** The mismatch is about which command produces the report; #815 is about how the report is summed. They are independent, and bundling them would put a `CLAUDE.md` edit inside a change whose only code surface is two PowerShell files.
2. **Reconciling the 80% / 85% line-floor divergence** (§12). Separate promotion; see #494's archive first.
3. **Adding a branch-coverage gate to script code.** None exists. Adding one is a policy change, not a defect fix, and would need maintainer ratification.
4. **`.//class` versus `./classes/class`** in `Get-CoberturaPackageLineSummary` (§6.2 H1). No behavioural difference on real input; not worth a change.
5. **Cross-package `filename` duplication** (§6.2 H2). Not observed; do not add defensive complexity without evidence.
6. **Rewriting historical evidence artifacts** to restate their figures under the corrected method (§7a). This would falsify the audit trail.

---

## 14. Premises in the delegation prompt that I refuted or could not confirm

Called out prominently, as requested.

1. **REFUTED — the issue's central magnitude claim.** #815 and `spec.md:46-47` state "79.38% reported versus 77.03% recomputed, a 2.35 point overstatement on first-party branch coverage", attributing it to method-row double counting. Measurement on two committed documents shows the naive-versus-de-duplicated branch-percentage gap is **0.0000 pp** (Document B, raw — the counters double exactly) and **+0.3239 pp** (Document A, post-processed). A third, independently recorded comparison at `.claude/agent-memory/feature-review/project_791-review-residuals.md:41-43` reports that the percentages "match to the digit" under both selections. The repository's own #441 regression test comment (`Invoke-MSTestWithCoverage.Helpers.Tests.ps1:210-211`) states the same rule. The double count therefore **cannot** be the cause of a 2.35-point swing. What the defect genuinely does is report **counts roughly 2x the true statement and branch population**, which is a real defect — it invalidates any absolute-count assertion and any cross-artifact count comparison — but it is not the defect described.

2. **UNKNOWN — the provenance of 77.03%.** It cannot be reconstructed. Per finding 3 the raw report is not committed, and neither the issue, the spec, nor the 809 evidence tree records the recomputation method. Candidate explanations I could not test are listed in §6.3. **Recommendation: the fix must not be gated on reproducing 77.03%.** The spec's proposed integration check — "re-derive the coverage figures for a recently merged item and confirm the corrected method reproduces the reviewer's recomputation" — is not satisfiable as written and should be replaced by the deterministic fixture condition of §9.

3. **REFUTED — the spec's evidence citation.** `spec.md:55-56` states the recomputation was performed "from the raw Cobertura report committed under that item's `evidence/qa-gates/` tree". No such file is committed. `spec.md:21` repeats the claim ("Data source or fixture: the raw Cobertura report from issue 809's final QA gate run"). This should be corrected in `spec.md` before the plan is finalised, because it currently names a fixture that the implementing agent will not find.

4. **CORRECTED — the delegation brief's expectation about branch behaviour.** The brief asked me to "explain precisely why the branch overstatement is larger in proportional terms than any line overstatement would be, or show that it is not". It is not: on both documents the branch-rate movement is smaller than the line-rate movement (0.0000 vs 0.0270 pp on Document B; 0.3239 vs 0.4621 pp on Document A). §6.3 gives the mechanism.

5. **PARTIAL — the brief's characterisation of the propagation surface.** The brief anticipated "atomic plan templates or skills that pin the current aggregation text". **No skill, rule, workflow or script pins it.** The only forward-looking carriers are two `.claude/agent-memory/atomic-executor/` records (§7b), and even those are ambiguous rather than prescriptive. The propagation risk is therefore materially lower than the issue assumes.

6. **UNVERIFIED — plan text equality with `origin/main`.** Git is only reachable through the disabled Bash tool. The snippet in §4 was read from the working tree; see the caveat at the end of §4.

7. **NOT PERFORMED AS SPECIFIED — the `pwsh` execution of both aggregations.** See §2. An exact, independently cross-validated substitute was used instead. The §9.3 fixture figures in particular rest on hand evaluation and **must be confirmed by execution** before the acceptance condition is treated as met.
