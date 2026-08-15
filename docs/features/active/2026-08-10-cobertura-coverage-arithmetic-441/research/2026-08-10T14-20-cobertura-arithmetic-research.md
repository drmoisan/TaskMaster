# Cobertura Coverage Arithmetic — Research (#441, #478)

Timestamp: 2026-08-10T14-20

- **Feature:** `2026-08-10-cobertura-coverage-arithmetic-441`
- **Issues:** #441 (descendant-axis double count), #478 (blended merge denominator)
- **Worktree root:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a953f53c75b721348`
- **Scope:** PowerShell only. Toolchain per `.claude/rules/powershell.md:13-20` (PoshQC format -> PSScriptAnalyzer analyze -> Pester test, via MCP).

## 0. Method and tool limitations (read first)

This research session had **no shell/Bash tool available**. Every count below was produced with the
`Grep` tool (ripgrep) or by direct enumeration of `Read` tool output. Each count states the exact
ripgrep pattern and target file, and each structural claim cites `file:line`. Anything that would
have required executing PowerShell is marked `UNVERIFIED` with the reason.

### 0.1 Prior-research documents are ABSENT from this worktree — `UNVERIFIED`

The delegation prompt directed me to read and cite two prior documents. Neither exists in this
worktree:

- `docs/features/active/2026-08-07-quickfiler-coverage-ledger-432/research/2026-08-07T22-15-quickfiler-coverage-ledger-research.md` — **not present**
- `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/coverage-harness-contract.md` — **not present**

Evidence: `Glob docs/features/active/**/issue.md` returns 11 feature folders and neither `-432` nor
`-454` is among them. `Glob docs/features/**/*432*` and
`Glob docs/features/**/coverage-harness-contract.md` both return "No files found". Those feature
branches have presumably not been merged into this worktree's base (`a682c7a2`).

**Consequence:** the root-cause analysis below was re-derived independently from the source and from
the committed sample reports. It is not a repetition of unread prior work. The planner should not
assume those two documents are reachable from this branch.

The one cross-check document named in the prompt **is** present and was read:
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-delta.2026-08-07T00-48.md:10`
records the correct recipe verbatim — *"per-line dedup by `(filename, line number)`, because Cobertura
repeats each line under both `<method><lines>` and the class-level `<lines>`"* — and `:41` records
`QfcHomeController.cs` at 68.40% (171/250). Note `:41` is `QfcHomeController.cs`, a **different file**
from `QfcHomeController.Iteration.cs`, which is the file the issue's arithmetic table concerns.

### 0.2 Correction to the issue statement's line references

`issue.md:23` and `issue.md:38` attribute the descendant-axis selection to
`Get-CoberturaCoverageSummary` "(`:98`)" and `Merge-CoberturaClassesByFilename` "(`:167`)". Both of
those line numbers are **function declaration lines**, not selections:

- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:98` — `function Get-CoberturaCoverageSummary {`
- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:167` — `function Merge-CoberturaClassesByFilename {`

The actual situation is materially different and the planner must not miss it:

| Site | Axis | Verdict |
|---|---|---|
| `Helpers.ps1:122` — `foreach ($line in $cls.SelectNodes('.//lines/line'))` | descendant | **The one and only defective selection in the repository.** |
| `Helpers.ps1:219` — `foreach ($lineNode in @($classNode.SelectNodes('./lines/line')))` | child | **Already correct.** This is the union builder; it must NOT be "fixed". |

`Merge-CoberturaClassesByFilename` does not itself select over the descendant axis. The descendant
axis reaches it **indirectly**, through the `$classSummaryXml` temporary-document trick at
`Helpers.ps1:270-273`, which calls `Get-CoberturaCoverageSummary` on the merged class. That single
delegation is what makes #478 a consequence of #441 rather than an independent defect. A planner who
edits `:219` instead of `:122` will destroy the correct union and leave both bugs in place.

---

## 1. Exact structural shape of the real Cobertura documents

### 1.1 Two committed samples, two different provenances

| File | Provenance | Root `lines-valid` |
|---|---|---|
| `.../424/evidence/baseline/coverage-baseline.cobertura.xml` | **RAW** `dotnet-coverage` output. Absolute filenames (`:6`), no `<sources>` element. | `79957` (`:2`) |
| `.../424/evidence/qa-gates/coverage-final.cobertura.xml` | **POST-PROCESSED** by `ConvertTo-KoverageCoberturaXml`. Relative filenames (`:9`), `<sources><source>.</source></sources>` at `:3-5`. | `110849` (`:2`) |

This distinction is the single most important structural fact in this research and is developed in
§1.3.

### 1.2 Element nesting and indentation

The emitted document is indented by `XmlTextWriter` with `Formatting.Indented` (`Helpers.ps1:349-354`),
producing a fixed two-space-per-level layout. That makes indentation-anchored ripgrep patterns a
reliable proxy for the XPath axis:

```
coverage(0) > packages(2) > package(4) > classes(6) > class(8)
  class(8) > methods(10) > method(12) > lines(14) > line(16)      <- METHOD-level line
  class(8) > lines(10) > line(12)                                  <- CLASS-level rollup line
```

Verified against `coverage-final.cobertura.xml:22612-22739` (the `QfcHomeController.Iteration.cs`
class) and `:174373-174394`.

### 1.3 Counted evidence — `coverage-final.cobertura.xml` (post-processed)

| # | ripgrep pattern (`Grep` tool, `output_mode: count`) | Result |
|---|---|---|
| C1 | `<line number=` | **110849** |
| C2 | `^            <line number=` (12 sp = class-level) | **62345** |
| C3 | `^                <line number=` (16 sp = method-level) | **48504** |
| C4 | `^            <line number="[0-9]+" hits="0"` | **9332** |
| C5 | `^                <line number="[0-9]+" hits="0"` | **6580** |
| C6 | `^        <class ` | **534** |
| C7 | `^          <lines( />\|>)` | **534** |
| C8 | `^          <lines />$` | **0** |
| C9 | `^          <methods( />\|>)` | **534** |
| C10 | `^          <methods />$` | **0** |
| C11 | `^            <method ` | **6330** |
| C12 | `.` (every line) | **186913** (total file lines) |

Reconciliations:

- C2 + C3 = 62345 + 48504 = **110849** = C1. The two axes partition the `<line>` population exactly;
  no third nesting depth exists.
- C1 = **110849** = the emitted `lines-valid` attribute (`coverage-final.cobertura.xml:2`). The
  emitted denominator is the *raw both-axes* count, exactly as `issue.md:31-32` states.
- C4 + C5 = 9332 + 6580 = 15912 = 110849 - 94937 = `lines-valid` - `lines-covered`. Consistent.
- C6 = C7 = C9 = 534, and C8 = C10 = 0: **every class in this report carries both a non-empty
  class-level `<lines>` element and a non-empty `<methods>` element.** There is no class with only
  method-level lines.

### 1.4 The decisive structural proof — `coverage-baseline.cobertura.xml` (raw)

| # | ripgrep pattern | Result |
|---|---|---|
| B1 | `<line number=` | **161086** |
| B2 | `^            <line number=` (class-level) | **79957** |
| B3 | `^            <line number="[0-9]+" hits="0"` | **23833** |

The raw file's own root attributes (`coverage-baseline.cobertura.xml:2`) are
`lines-valid="79957" lines-covered="56124"`.

- B2 = **79957** = `lines-valid`, **exactly**.
- B2 - B3 = 79957 - 23833 = **56124** = `lines-covered`, **exactly**.
- B1 = 161086, which is **not** `lines-valid`.

**Conclusion (high confidence):** `dotnet-coverage`'s own Cobertura writer defines
`lines-valid` / `lines-covered` as the **class-level rollup only**. The class-level `<lines>` element
is the authoritative per-class line set; `<methods>/<method>/<lines>/<line>` is a redundant
per-method view of the same lines.

Therefore `ConvertTo-KoverageCoberturaXml` is not merely "counting differently" — it **overwrites a
correct root summary with an incorrect one**. `Helpers.ps1:341-347` unconditionally replaces the
generator's `line-rate`, `branch-rate`, `lines-covered`, `lines-valid`, `branches-covered`,
`branches-valid` with the doubled figures.

This gives the fix an unusually strong correctness oracle: **the corrected
`Get-CoberturaCoverageSummary`, run over the raw baseline document, must reproduce that document's own
root attributes exactly** (79957 / 56124 / 23109 / 13472).

### 1.5 Is the method-level line set a subset of the class-level rollup?

Three spot checks, read directly:

1. **`coverage-final.cobertura.xml:174373-174394`** — `ToDoModel.Properties.Settings`, unmerged.
   Methods `get_Default` (lines 21, 22, 23) and `.cctor` (line 18). Class-level `<lines>` at
   `:174388-174393` = `18, 21, 22, 23`. **Exactly the sorted distinct union.** 4 = 3 + 1.
2. **`coverage-final.cobertura.xml:185673-185694`** — `TaskMaster.Properties.Settings`, unmerged.
   Identical shape: methods `{21,22,23}` and `{18}`, class-level `{18,21,22,23}`. **Exact union.**
3. **`coverage-final.cobertura.xml:22612-22739`** — `QuickFiler.Controllers.QfcHomeController`
   (filename `QuickFiler\Controllers\QfcHomeController.Iteration.cs`), **merged**. Methods contribute
   `{56..58, 60..68}` (12), `{71..77}` (7), `{80..84}` (5) = 24 line numbers. Class-level `<lines>` at
   `:22660-22738` carries 56 line numbers, a **strict superset** containing all 24 plus 32 line
   numbers (`12..53`) contributed by sibling classes that the merge unioned in.

The class-level rollup is a sorted, distinct union; the method-level view is the same lines grouped
by method. Case 3 shows the post-merge asymmetry that produces #478.

**`UNVERIFIED`:** I could not exhaustively prove across all 534 classes that no `<method>` carries a
line number absent from its class-level rollup — that requires executing a script, and no shell was
available. Two mitigations: (a) the raw-document reconciliation in §1.4 shows the generator itself
*defines* the total from the rollup, so matching the rollup is by definition matching the generator;
(b) the recommended remediation in §4 unions both axes with dedup, which is correct whether or not
the subset property holds universally.

**Answer to Q1:** For every class in the committed samples, the class-level `<lines>` set is present,
non-empty, and is a superset of (unmerged: exactly equal to) the union of its `<method>` line numbers.
The fix *can* safely use the child axis `./lines/line`; the safer formulation is to dedup by line
number across both axes, which reduces to the child axis on all observed data.

---

## 2. Independent verification of the #478 arithmetic — CONFIRMED EXACTLY

Target: `QuickFiler\Controllers\QfcHomeController.Iteration.cs` in
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`.

Located via `Grep` pattern `QfcHomeController\.Iteration\.cs` -> single hit at `:22612`
(only one class element bears that filename, because the merge already collapsed the group).

Class element: `:22612` -> `:22739`. Emitted attributes: `line-rate="0.8625" branch-rate="0.666667"`.

### 2.1 Class-level `<lines>` (`:22660-22738`), enumerated

| Group | Line numbers | Count | hits |
|---|---|---|---|
| A | 12, 13, 15, 16, 17, 20, 21, 22, 23, 24, 25, 26, 28, 29, 30, 31, 33, 35, 36, 37 | 20 | 1 |
| B | 38, 39, 41, 42, 43, 44, 45, 47, 49, 50, 52 | 11 | **0** |
| C | 53 | 1 | 1 |
| D | 56, 57, 58, 60, 61, 62, 63, 64, 65, 66, 67, 68 | 12 | 1 |
| E | 71, 72, 73, 74, 75, 76, 77 | 7 | 1 |
| F | 80, 81, 82, 83, 84 | 5 | 1 |

Total = 20 + 11 + 1 + 12 + 7 + 5 = **56**. Covered = 56 - 11 = **45**.

### 2.2 Method-level `<lines>` (`:22613-22659`)

| Method | Decl. line | Line numbers | Count | hits |
|---|---|---|---|---|
| `Iterate ()` | `:22614` | 56, 57, 58, 60, 61, 62, 63, 64, 65, 66, 67, 68 | 12 | all 1 |
| `Iterate2 ()` | `:22639` | 71, 72, 73, 74, 75, 76, 77 | 7 | all 1 |
| `SwapStopWatch ()` | `:22650` | 80, 81, 82, 83, 84 | 5 | all 1 |

Total = **24**, all covered.

### 2.3 Result

| Quantity | Claim in `issue.md:49-50` | **Measured** | Match |
|---|---|---|---|
| Class-level union | 45 / 56 = 0.8036 | **45 / 56 = 0.803571** | **YES** |
| Emitted `line-rate` | 0.8625 = 69 / 80 | **0.8625**, and 45 + 24 = **69**, 56 + 24 = **80**, 69/80 = **0.8625** | **YES** |

The issue's reproducible-arithmetic table is **confirmed exactly**. The regression fixture may assert
against `45 / 56 = 0.803571` (corrected) versus `69 / 80 = 0.8625` (defective).

### 2.4 Branch arithmetic for the same class — a warning for fixture design

Class-level branch lines (`branch="True"`), read directly:

| Line | Source line | `condition-coverage` |
|---|---|---|
| `:22663` | 15 | `100% (2/2)` |
| `:22675` | 25 | `100% (2/2)` |
| `:22694` | 44 | `0% (0/2)` |
| `:22708` | 60 | `50% (2/4)` |
| `:22714` | 61 | `100% (2/2)` |

Class-level totals: covered 2+2+0+2+2 = **8**, total 2+2+2+4+2 = **12** -> **8/12 = 0.666667**.

Method-level branch lines: `:22619` (line 60, `50% (2/4)`) and `:22625` (line 61, `100% (2/2)`) ->
covered 4, total 6.

Both-axes totals: covered 12, total 18 -> **12/18 = 0.666667**, which equals the emitted
`branch-rate="0.666667"`.

**This is the trap:** for this class the *ratio* is unchanged by the double count while the *counts*
are inflated 50%. A regression test that asserts only on `branch-rate` can pass against the defective
code. Branch fixtures must assert on `branches-valid` and `branches-covered`, not on `branch-rate`
alone.

---

## 3. Full call-site inventory of the defective axis

Search performed with the `Grep` tool over the whole worktree, pattern
`lines/line|SelectNodes\('\.//line|GetElementsByTagName\('line|//line\[`, glob `!docs/**`.
A second search covered the emitted attributes:
`lines-valid|lines-covered|line-rate|branches-valid|branches-covered|branch-rate`.

### 3.1 Selections over a line axis (production code)

| # | Site | Expression | Must change? |
|---|---|---|---|
| S1 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:122` | `$cls.SelectNodes('.//lines/line')` | **YES — this is the defect.** Descendant axis; matches both the class-level rollup and every method-level copy. |
| S2 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:219` | `$classNode.SelectNodes('./lines/line')` | **NO — already the child axis and already correct.** This builds the merged union. Changing it breaks #478's correct half. |
| S3 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:273` | `Get-CoberturaCoverageSummary -XmlDocument $classSummaryXml` | **INDIRECT — carries S1's defect into the merged per-class `line-rate`/`branch-rate` set at `:275-276`.** Fixed automatically when S1 is fixed; see §4c for the recommended simplification. |
| S4 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:121` | `$pkg.SelectNodes('.//class')` | **NO.** Cobertura nests classes only at `package/classes/class`, so `.//class` is equivalent to `./classes/class` here. Optional tightening; not required. |
| S5 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:254`, `:258` | `SelectNodes('./conditions')` | **NO.** Child axis on `<line>`, unrelated. |
| S6 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:324` | `$xml.SelectNodes('//class[@filename]')` | **NO.** Filename rewrite only. |

**`scripts/vscode/Invoke-MSTestWithCoverage.ps1` contains no line-axis selection at all.** Verified:
the repo-wide grep returned zero hits in that file. Its only involvement is the call at `:340`,
`ConvertTo-KoverageCoberturaXml -XmlContent $xmlContent -RepoRoot $repoRoot`. `issue.md:77-78` scopes
it "only if it independently selects over the same descendant axis" — **it does not.** No change
required there.

### 3.2 Selections in test code

| # | Site | Expression | Must change? |
|---|---|---|---|
| T1 | `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:86` | `//class[@filename="..."]/lines/line[@number="11"]` | NO — child axis, asserts merged output. |
| T2 | same file `:87` | `.../lines/line[@number="12"]` | NO. |
| T3 | same file `:126` | `.../lines/line[@number="11"]` | NO. |

### 3.3 Consumers of the emitted `line-rate` / `lines-valid` / `lines-covered` — blast radius

| Consumer | Location | What it reads | Impact of the fix |
|---|---|---|---|
| Koverage VS Code extension | `.vscode/settings.json:20-25` (`koverage.coverageFileNames` = `coverage.cobertura.xml`, `koverage.coverageFilePaths` = `coverage`) | The whole document; gutters are driven by the class-level `<lines>` `hits` values. | **None to gutters** (the recommended fix does not alter `<lines>` content). Summary percentages it derives from the root attributes become correct. |
| `scripts/temp-extract-coverage.ps1:13` | `[double]$c.'line-rate'` per class | Per-class `line-rate`. | Values become correct **for merged classes only**; unmerged classes were already correct (they retain the generator's own attribute — the post-processing never rewrites them). |
| `scripts/temp-extract-coverage.ps1:47` | `$pkg.'line-rate'` | **Package-level** `line-rate`. | **No change — and this is a separate latent defect.** `ConvertTo-KoverageCoberturaXml` never recomputes package-level attributes (it writes only root attributes at `:342-347` and merged-class attributes at `:275-281`), so after package filtering and class merging every `<package line-rate=...>` is stale. Out of scope for #441/#478; recommend promoting to a new issue. |
| Committed evidence artifacts | e.g. `.../424/evidence/qa-gates/coverage-delta.2026-08-07T00-48.md:58-61` | Root `line-rate`, `lines-covered`, `lines-valid`, `branches-*`. | Historical artifacts become non-comparable with post-fix figures. See §3.5. |
| GitHub Actions CI | `.github/workflows/ci.yml:118-160` | **Nothing.** CI runs `vstest ... /EnableCodeCoverage` (`:147`) and uploads raw `TestResults/**/*.coverage` (`:159`). It never produces or reads a Cobertura file and has no coverage threshold gate. | **None. CI is not affected by this change.** |
| Feature-review coverage gate | `.claude/hooks/validate-feature-review-coverage.ps1:229-253` | JaCoCo `//counter[@type="LINE"]` from `artifacts/csharp/coverage.xml` | **None** — different format, different artifact, different producer (`dotnet test --collect:"XPlat Code Coverage"` per `.claude/skills/feature-review-workflow/SKILL.md:110`). Do not conflate. |
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:370,396` | Mocks `ConvertTo-KoverageCoberturaXml` entirely | Unaffected. |

Artifact location: `.gitignore:143-145` ignores `coverage/*` (except `.gitkeep`), so the working
report is untracked; only deliberately copied evidence snapshots are committed.

### 3.4 Corrected repository-wide figure for the committed sample

From C2/C4: class-level valid = 62345, class-level uncovered = 9332, therefore class-level covered =
**53013**.

| Metric | Emitted (defective) | Corrected (class-level) | Delta |
|---|---|---|---|
| `lines-valid` | 110849 | **62345** | -48504 |
| `lines-covered` | 94937 | **53013** | -41924 |
| `line-rate` | 0.856453 (85.6453%) | **0.850317** (85.0317%) | **-0.61 pp** |

**Handoff note to #494 (do not act on it here):** against `.claude/rules/general-unit-test.md`'s
uniform >= 85% line floor, the corrected figure for this particular committed report is 85.0317% — a
margin of **0.03 percentage points**. The corrected repository-wide number is materially closer to the
floor than the reported one. This is a threshold-reconciliation input for #494, not a reason to change
any threshold in this feature. (Note also that this figure is package-filtered and is not the same
denominator as the raw 79957 figure; see §3.5.)

### 3.5 A prior evidence artifact is misinterpreted, and a stored memory is wrong

`coverage-delta.2026-08-07T00-48.md:60` compares baseline `56124 / 79957` against post-change
`94937 / 110849` and `:65` attributes the +38.6% denominator growth to "known `dotnet-coverage`
denominator instability". That attribution is **incorrect**: 79957 is a **raw** figure computed the
correct (class-level) way by the generator, and 110849 is a **post-processed** figure computed the
defective (both-axes) way. The two numbers were produced by two different formulas over two different
package sets. The apparent +15.5-point line-rate improvement was largely an artifact of #441, not of
instrumentation variance.

Relatedly, `.claude/agent-memory/atomic-executor/project_coverage_delta_reproduce_baseline_counting_method.md:34-36`
asserts *"The repo-wide root `<coverage>` attributes are already deduped and match a per-package
all-descendant sum in this repo, so repo-level figures need no adjustment."* The first clause is true
**only for raw `dotnet-coverage` output**; it is false for any post-processed
`ConvertTo-KoverageCoberturaXml` artifact, where the root attributes are the all-descendant sum. That
memory should be corrected once this fix lands.

---

## 3a. Branch-rate exposure

### 3a.1 Is the same double count present in `branches-valid` / `branches-covered`?

**Yes, structurally and by measurement.** The branch accumulator at
`Helpers.ps1:128-131` sits **inside** the same `foreach ($line in $cls.SelectNodes('.//lines/line'))`
loop opened at `:122`. Every `<line>` matched twice contributes its `condition-coverage` fraction
twice.

Measured branch-line populations in `coverage-final.cobertura.xml`:

| ripgrep pattern | Result |
|---|---|
| `^            <line number="[0-9]+" hits="[0-9]+" branch="True" condition-coverage=` (class-level) | **6333** |
| `^                <line number="[0-9]+" hits="[0-9]+" branch="True" condition-coverage=` (method-level) | **4902** |

Class-level denominator bucketing (`coverage-final.cobertura.xml`, patterns of the form
`^            <line .*condition-coverage="[^"]*\([0-9]+/N\)"`):

| Denominator | Count | Contribution |
|---|---|---|
| `/2` | 5248 | 10496 |
| `/4` | 805 | 3220 |
| `/6` | 156 | 936 |
| `/8` | 66 | 528 |
| single-digit total | 6293 | — (so 18 lines have odd denominators 5/7/9; `/3` count is 0) |
| `/1x` (10-19) | 34 | 340..646 |
| `/2x`-`/9x` (20-99) | 6 | 120..594 |

Accounting closes exactly: 6293 + 34 + 6 = **6333**. Class-level `branches-valid` is therefore
bounded in **[15730, 16582]** against an emitted `branches-valid="27848"`. Exact value `UNVERIFIED`
(summing per-line denominators requires script execution).

### 3a.2 Proof that the generator also defines branches class-level-only

On the **raw** `coverage-baseline.cobertura.xml` (root `branches-valid="23109"`,
`branches-covered="13472"`):

| ripgrep pattern | Result |
|---|---|
| `^            <line .*condition-coverage=` (class-level) | **9597** |
| `^                <line .*condition-coverage=` (method-level) | **9627** |
| class-level `/2` | 8211 |
| class-level `/4` | 1052 |
| class-level `/6` | 199 |
| class-level `/8` | 70 |
| class-level two-digit denominators | 44 |

Class-level even-single-digit contribution = 8211*2 + 1052*4 + 199*6 + 70*8 = **22384**. Remaining
lines: 9597 - 9532 - 44 = 21 odd-single-digit, plus 44 two-digit. Required remainder to reach 23109 is
**725**, which falls inside the achievable range [503, 4545] for those 65 lines (mean ≈ 11.2 — entirely
plausible for a mix of `/5`,`/7`,`/9` and `/1x` denominators).

The **both-axes** hypothesis is decisively falsified: method-level branch lines alone (9627) contribute
at least 9627 * 2 = 19254, so a both-axes total would be >= 22384 + 19254 = **41638**, versus the actual
23109.

**Conclusion:** `dotnet-coverage` computes `branches-valid` / `branches-covered` from the class-level
rollup only, identically to lines.

### 3a.3 Correct deduplication rule for branches, and scope verdict

Rule: **per class, per distinct line number, contribute exactly one `condition-coverage` fraction.**
When the same line number appears on both axes, prefer the entry with the larger `Total`, tie-broken by
the larger `Covered`. This is not a new invention — it is precisely the precedence already implemented
for the merge at `Helpers.ps1:240-245`, backed by the existing pure helper
`Get-CoberturaLineConditionCoverageParts` (`Helpers.ps1:146-165`). On all observed data the two axes
carry byte-identical `condition-coverage` strings for the same line (verified at
`coverage-final.cobertura.xml:22619` vs `:22708` — both `50% (2/4)` for line 60; and `:22625` vs
`:22714` — both `100% (2/2)` for line 61), so the rule reduces to "take the class-level value".

**Scope verdict: branch arithmetic is necessarily IN SCOPE.** It cannot be deferred, for three reasons:

1. The branch accumulator is physically inside the loop being fixed (`Helpers.ps1:122-132`). There is
   no way to change the iteration set without changing branch totals.
2. Leaving branches doubled while lines are corrected would emit an internally inconsistent report
   (root `lines-valid` class-level, root `branches-valid` both-axes) — strictly worse than today.
3. `branch-rate` is written to the root (`Helpers.ps1:343`) and to every merged class
   (`Helpers.ps1:276`), so the same blended-denominator defect (#478) applies to branches.

---

## 4. Recommended remediation

### 4.0 Selected approach

Introduce **one new pure helper** that reduces a class element to a deduplicated per-line map, and use
it from both existing call paths.

```
Get-CoberturaClassLineSummary -ClassNode <XmlElement> -> [pscustomobject]
    LineMap  : hashtable  (int line number -> { Node; Hits; Covered; Total })
    TotalLines, CoveredLines, TotalBranches, CoveredBranches
```

Construction rule (satisfies `issue.md:68-71` literally):

1. Enumerate `./lines/line` (the class-level rollup) **then** `./methods/method/lines/line`.
2. Key by `[int]$node.number`. On a repeat key: `hits = max(existing, candidate)`; `branch = True` if
   either is `True`; `condition-coverage` taken from the entry with the larger `Total`, tie-broken by
   larger `Covered` — reusing `Get-CoberturaLineConditionCoverageParts`.
3. `TotalLines` = distinct key count; `CoveredLines` = keys with `hits > 0`;
   `TotalBranches` / `CoveredBranches` = sums over distinct keys.

Then:

- **`Get-CoberturaCoverageSummary` (`Helpers.ps1:98-144`)** — replace the body of the inner loop at
  `:122-132` with one call to the helper per class and accumulate the four returned totals. The
  descendant-axis selection at `:122` is deleted.
- **`Merge-CoberturaClassesByFilename` (`Helpers.ps1:167-292`)** — keep the union builder at `:217-268`
  exactly as it is (it is already correct), but **replace `:270-273`** with a direct call to the new
  helper on `$mergedClassNode` (see §4c).

Because method-level lines are a subset of the class-level rollup on all observed data (§1.5), the
union collapses to the class-level set and the result equals the generator's own arithmetic. The union
formulation is nonetheless preferred over a bare child-axis switch because it cannot silently drop a
line if the subset property ever fails, and because it matches the AC wording in `issue.md:68-69`
("deduplicating by line number with `max(hits)`") literally.

Expected outcomes:
- `QfcHomeController.Iteration.cs` merged `line-rate`: `0.8625` -> `0.803571` (45/56).
- Root of the #424 sample: `lines-valid` 110849 -> 62345; `line-rate` 0.856453 -> ~0.850317.
- Root over the **raw** baseline document: exactly 79957 / 56124 / 23109 / 13472.

### 4a. Is the class-level rollup always present? (naive child-axis switch risk)

**In the committed data, yes, unconditionally.** Evidence C6/C7/C8: 534 classes, 534 class-level
`<lines>` elements, **0** empty `<lines />`. There is no class in the sample whose lines exist only
under `<methods>`.

However, the *existing test fixtures* prove the opposite shape is representable: every fixture in
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` uses either no `<methods>` element
(`:17-22`) or an empty `<methods />` (`:61`, `:68`, `:105`, `:112`, `:146`, `:157`). A hand-authored or
third-party Cobertura document with method-only lines is therefore possible.

The union formulation in §4.0 removes the risk entirely — no `<lines>`-presence guard, no fallback
branch, no dead code. This is the primary reason it is preferred over a bare `./lines/line` switch.
A dedicated fixture (§5, F6) pins the behavior.

### 4b. Should `Merge-CoberturaClassesByFilename` merge the `<methods>` subtrees?

**Recommendation: NO — do not merge `<methods>` in this change.** Leave the merged class carrying the
primary class's `<methods>` unchanged.

Rationale:

1. **The AC does not require it.** `issue.md:70-71` requires the emitted per-file `line-rate` to equal
   the rate over the merged class-level `<lines>` alone. With the §4.0 union that holds exactly,
   because the primary's method lines are a subset of the merged union (verified in §1.5 case 3:
   24 subset of 56).
2. **`CLAUDE.md` Bugfix Workflow step 2 mandates the minimal targeted fix** ("Change only what is
   needed... avoid opportunistic refactors. If you uncover deeper design problems, open a new issue").
3. **Merging carries a real hazard.** Sibling classes sharing a filename are compiler-generated
   partners (`Foo` and `Foo.<>c`, async state machines). Both routinely declare a method named `.ctor`
   with signature `()`. Appending sibling `<method>` elements would produce a class with two
   `name=".ctor" signature="()"` children. Cobertura imposes no uniqueness constraint, but any
   consumer that keys methods by `(name, signature)` — including the per-method `line-rate` technique
   documented in `.claude/agent-memory/atomic-executor/project_coverage_delta_reproduce_baseline_counting_method.md:22-24`
   — would be broken. Deduplicating by `(name, signature)` would be worse still: it would silently
   discard genuinely distinct methods.
4. **Stripping `<methods>` is rejected outright.** It destroys per-method `line-rate` data that
   coverage-delta work in this repository actively relies on (the same memory file, `:22-24`, notes
   per-`<method>` figures are the *only* rollup-method-insensitive numbers available).

**Downstream usefulness is preserved.** Koverage gutters and VS Code coverage decoration are driven by
the class-level `<lines>` `hits` values, which this change does not touch. The recommended fix alters
**only** four root attributes and two attributes on merged class elements; not one `<line>` or
`<method>` element changes.

Record as a **follow-up issue candidate** (not this feature): "merged Cobertura class retains only the
primary class's `<methods>`, so the emitted document's methods do not account for all of its class-level
lines" — together with the stale package-level `line-rate` finding from §3.3.

### 4c. Interaction with the `$classSummaryXml` temporary-document trick (`:270-273`)

Current code:

```powershell
$classSummaryXml = [xml]"<coverage><packages><package><classes /></package></packages></coverage>"
$classSummaryClasses = $classSummaryXml.SelectSingleNode('//classes')
[void]$classSummaryClasses.AppendChild($classSummaryXml.ImportNode($mergedClassNode, $true))
$classSummary = Get-CoberturaCoverageSummary -XmlDocument $classSummaryXml
```

This exists solely because `Get-CoberturaCoverageSummary` accepts an `[xml]` document and throws
without a `//packages` node (`Helpers.ps1:111-114`). It builds a synthetic document and deep-imports
the merged class per merged filename group.

**Recommendation: delete `:270-273` and call the new per-class helper directly** on
`$mergedClassNode`, then set the attributes at `:275-276` from its result.

- Correctness: `$mergedClassNode` is an orphan clone (`$primaryNode.CloneNode($true)` at `:200`) owned
  by `$XmlDocument` but not yet attached — it is not re-parented into the tree until `:283`. XPath
  child-axis selections on an orphan element work normally, so no import is required.
- Cost: removes one document construction plus one deep `ImportNode` per merged filename group.
- Risk: low, and it eliminates the indirect coupling (S3 in §3.1) that made #478 a *consequence* of
  #441 rather than an independent bug. After this change the two functions share a helper instead of
  one calling the other's document-level entry point.

If the planner prefers a strictly smaller diff, `:270-273` can be left intact — it will yield the
correct answer once `Get-CoberturaCoverageSummary` is fixed, because the synthetic document contains
exactly one class. The simplification is recommended but not required for correctness.

### 4d. Rejected alternatives (brief)

- **Bare child-axis switch** (`.//lines/line` -> `./lines/line` at `:122`, nothing else). Simplest
  possible diff and exactly reproduces the generator's definition. Rejected as the primary because it
  silently drops any method-only line and does not implement the `max(hits)` dedup the AC states
  (`issue.md:68-69`). Its result is identical to the recommendation on all observed data.
- **Strip `<methods>` from merged classes.** Rejected: destroys per-method data actively used for
  coverage-delta analysis (§4b item 4).
- **Merge `<methods>` subtrees.** Rejected for this change: duplicate `(name, signature)` hazard and
  out-of-scope per the minimal-fix mandate (§4b item 3). Recorded as a follow-up issue candidate.
- **Recompute package-level `line-rate` too.** Rejected as out of scope; it is a genuine but separate
  latent defect (§3.3) and touching it widens the diff without serving #441 or #478.

---

## 5. Regression fixture design

### 5.1 Do not use the committed 186,913-line sample as a Pester fixture

`coverage-final.cobertura.xml` is **186,913 lines** (ripgrep count of all lines) carrying 110,849
`<line>` elements, 6,330 `<method>` elements and 534 `<class>` elements. Estimated on-disk size
~9-11 MB (byte size `UNVERIFIED` — no shell to stat the file). A `[xml]` cast materializes a full DOM;
for this document that is on the order of hundreds of megabytes of managed objects and multiple
seconds of parse time per `It` block.

Reject it as an in-suite fixture for three reasons:

1. **Speed.** `.claude/rules/general-unit-test.md` requires fast execution supporting frequent runs;
   `.claude/rules/powershell.md:60` requires focused single-behavior tests. A multi-second parse per
   assertion violates both.
2. **Path fragility.** It lives under `docs/features/active/2026-08-06-.../evidence/qa-gates/`. Per
   `.claude/skills/feature-promotion-lifecycle/SKILL.md` the feature folder moves out of `active/`
   when #424 is completed, silently breaking the test.
3. **Determinism of intent.** A 534-class document cannot express a targeted assertion; a failure
   would not identify the faulty unit (`general-unit-test.md`, Isolation).

**Use it instead as a one-time evidence input**, outside the test suite — see §7.1. That is where its
value is highest: it is real data with an independently known correct answer.

### 5.2 Recommended inline fixtures

Match the existing style in `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`:
single-quoted here-strings (`@'` ... `'@`) declared inside the `It` block, cast with
`[xml]$resultXml = ConvertTo-KoverageCoberturaXml ...`, `ProjectNames` supplied explicitly whenever
the package name is not a real repo assembly (see the comment at `:29-30` explaining why). No files on
disk; no mocks required for the arithmetic paths.

All six fixtures below are new `It` blocks; none modifies an existing block.

| ID | Target | Fixture shape | Assertions (post-fix) | Pre-fix result |
|---|---|---|---|---|
| **F1** | #441 lines | One package, one class. `<methods>` with one `<method>` carrying lines 10 (`hits=1`), 11 (`hits=0`), 12 (`hits=1`); class-level `<lines>` carrying the identical three. | `lines-valid` = `'3'`, `lines-covered` = `'2'`, `line-rate` = `'0.666667'` | 6 / 4 / `'0.666667'` — **note the rate alone does not discriminate; assert the counts** |
| **F2** | #441 branches | As F1 plus line 12 `branch="True" condition-coverage="50% (1/2)"` on **both** axes, with a `<conditions>` child. | `branches-valid` = `'2'`, `branches-covered` = `'1'` | 4 / 2 |
| **F3** | #478 merge | Two classes, same `filename`. Primary `Ns.Foo`: `<methods>` with lines 56,57,58 (`hits=1`) and class-level `<lines>` 56,57,58. Sibling `Ns.Foo.<>c`: `<methods>` with lines 12,13 (`hits=0`) and class-level `<lines>` 12,13. | merged class `line-rate` = `'0.6'` (3/5); merged class-level `<lines>` has exactly 5 `line` children, numbers `12,13,56,57,58` in ascending order | `'0.75'` (6/8) — the miniature of the confirmed `QfcHomeController.Iteration.cs` case |
| **F4** | max(hits) dedup | One class; line 5 appears in `.ctor ()` with `hits=1` **and** in `.ctor (int)` with `hits=0`; class-level `<lines>` has line 5 `hits=1`. | `lines-valid` = `'1'`, `lines-covered` = `'1'` | 3 / 2 |
| **F5** | rollup-absent guard (§4a) | One class with `<methods>` carrying lines 20,21 (`hits=1`,`hits=0`) and **no class-level `<lines>` element at all**. | `lines-valid` = `'2'`, `lines-covered` = `'1'` — the lines must NOT be dropped | 2 / 1 (unchanged; this fixture guards against a regression introduced by a naive child-axis switch) |
| **F6** | structure preservation | Reuse the F3 document. | merged class still has a `<methods>` element with exactly the primary's 1 `<method>` child; no `<line>` element's `hits` attribute differs from the input | passes today; locks §4b |

Notes for the implementer:

- F1 and F2 exercise `Get-CoberturaCoverageSummary` through the public
  `ConvertTo-KoverageCoberturaXml` surface, consistent with every existing test in the file. If the
  new helper is exposed, add one direct unit test per branch of its precedence rule
  (`Total` greater; `Total` equal and `Covered` greater; neither) — that is where scenario
  completeness (`general-unit-test.md`, Scenario Completeness) is cheapest to achieve.
- Package names in fixtures must either be real production assemblies (so
  `Get-KoverageProjectAllowlist` retains them) or be passed via `-ProjectNames`. `UtilitiesCS` and
  `ToDoModel` are already used this way at `:58` and `:102`.
- Determinism: no clock, no randomness, no filesystem. F1-F6 satisfy
  `.claude/rules/general-unit-test.md` Determinism Infrastructure trivially.

---

## 6. Existing-test regression risk — per `It` block

Read in full: `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` (223 lines).

| `It` block | Line | Fixture `<methods>` shape | Assertions | Verdict under the §4.0 fix |
|---|---|---|---|---|
| `preserves backslash separators for nested Windows paths...` | `:10-38` | **No `<methods>` element**; class-level `<lines>` with lines 10, 11 (`:18-21`) | `filename`, no `/`, `sources/source` = `'.'` (`:35-37`) | **PASS unchanged.** No arithmetic asserted; both axes yield the same 2 lines anyway. |
| `strips active and stale TaskMaster roots while preserving already relative paths` | `:40-51` | n/a — calls `ConvertTo-KoverageRelativePath` directly | 3 path equalities (`:45-50`) | **PASS unchanged.** Pure path function, untouched. |
| `merges duplicate class entries that point to the same source file` | `:53-95` | `<methods />` empty on both classes (`:61`, `:68`) | `classNodes.Count` = 1, `name`, `complexity` = `'5'`, `line11.hits` = `'1'`, `line12.branch` = `'True'`, `line12.'condition-coverage'` = `'50% (1/2)'` (`:89-94`) | **PASS unchanged.** Merged union = {10,11,12}; empty `<methods>` means the union across both axes is identical to the class-level union. Condition-coverage precedence at `Helpers.ps1:240-245` is reused verbatim, so `'50% (1/2)'` still wins. No `line-rate` is asserted. |
| `normalizes stale TaskMaster roots before merging duplicate production class entries` | `:97-133` | `<methods />` empty on both classes (`:105`, `:112`) | `classNodes.Count` = 1, `line11.hits` = `'1'`, **`lines-covered` = `'3'`, `lines-valid` = `'3'`, `line-rate` = `'1'`** (`:128-132`) | **PASS unchanged.** This is the block the delegation prompt flagged. Union = {10,11,12}, hits max = 1,1,1 -> 3/3, rate `'1'`. Because `<methods />` is empty, the descendant and child axes coincide **today**, which is exactly why this assertion never caught #441. |
| `excludes .Test packages from the report and from the aggregate covered/valid line totals` | `:135-182` | `<methods />` empty on both classes (`:146`, `:157`) | package retention/exclusion, **`lines-covered` = `'1'`, `lines-valid` = `'2'`** (`:180-181`) | **PASS unchanged.** Single production class, 2 class-level lines, empty `<methods>`. |
| `excludes projects that resolve to a .Test assembly name` | `:186-192` | n/a | allowlist shape | **PASS unchanged.** |
| `retains non-test production projects in the allowlist` | `:194-199` | n/a | allowlist contains `UtilitiesCS` | **PASS unchanged.** |
| `applies the .Test exclusion to the project-file base-name fallback` | `:201-221` | n/a — mocks `Get-ChildItem`/`Get-Content` | allowlist fallback | **PASS unchanged.** |

**Verdict: zero existing tests are expected to break, including the `lines-valid | Should -Be '3'`
assertion at `:131`.**

The reason is itself the headline finding of §6: **every existing fixture uses either no `<methods>`
element or an empty `<methods />`.** On such documents `.//lines/line` and `./lines/line` select the
identical node set, so the defective and correct implementations are indistinguishable. The existing
suite is structurally incapable of detecting #441 or #478. That is why the fix must ship with F1-F6
from §5.2, all of which populate `<methods>` with real `<line>` children.

Secondary check: `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` mocks
`ConvertTo-KoverageCoberturaXml` outright (`:370`) and asserts only invocation count (`:396`).
Unaffected.

---

## 7. Repository-wide coverage baseline re-capture

### 7.1 PRIMARY (recommended) — deterministic A/B over a fixed input, no test run

This change alters **only post-processing arithmetic**. Re-running the test suite to obtain a
"pre-change" and "post-change" figure would confound the fix's effect with the documented
`dotnet-coverage` denominator nondeterminism
(`.claude/agent-memory/atomic-executor/project_dotnet_coverage_denominator_nondeterminism.md`;
also `coverage-delta.2026-08-07T00-48.md:65`). The correct experiment holds the input fixed.

Use the committed **raw** report as the fixed input. It is raw (absolute filenames, no `<sources>`),
so it carries the generator's own correct root attributes as ground truth.

```powershell
# Run from the worktree root.
# Pre-change: check out the unmodified Helpers.ps1. Post-change: after the fix.
pwsh -NoProfile -Command @'
$root = "C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a953f53c75b721348"
. (Join-Path $root "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
$raw = Get-Content -LiteralPath (Join-Path $root "docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\baseline\coverage-baseline.cobertura.xml") -Raw -Encoding UTF8
[xml]$doc = $raw
"INPUT  root: lines-valid={0} lines-covered={1} branches-valid={2} branches-covered={3}" -f `
    $doc.coverage.'lines-valid', $doc.coverage.'lines-covered', $doc.coverage.'branches-valid', $doc.coverage.'branches-covered'
Get-CoberturaCoverageSummary -XmlDocument $doc | Format-List
'@
```

| | `LinesValid` | `LinesCovered` | `BranchesValid` | `BranchesCovered` |
|---|---|---|---|---|
| Input document's own root attributes (ground truth) | 79957 | 56124 | 23109 | 13472 |
| **Expected AFTER the fix** | **79957** | **56124** | **23109** | **13472** |
| Expected BEFORE the fix | 161086 | (inflated) | (inflated) | (inflated) |

`LinesValid = 161086` pre-fix is verified by ripgrep count B1 (§1.4). The exact pre-fix covered and
branch figures are `UNVERIFIED` (they require execution) but must be strictly greater than the
post-fix values.

This is the strongest available acceptance evidence: **the corrected post-processor reproduces, to the
line, the figures the instrumentation tool itself computed.** Record it under
`docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/evidence/qa-gates/`.

A second, package-filtered A/B can be run the same way over
`.../424/evidence/qa-gates/coverage-final.cobertura.xml` reprocessed through
`ConvertTo-KoverageCoberturaXml`; expected corrected root figures are `lines-valid=62345`,
`lines-covered=53013`, `line-rate≈0.850317` per §3.4.

### 7.2 SECONDARY — live repository-wide re-capture (if the plan requires a fresh figure)

CI produces **no** Cobertura report (`.github/workflows/ci.yml:118-160` runs
`vstest ... /EnableCodeCoverage` and uploads raw `.coverage` binaries only), so the repository-wide
Cobertura figure comes exclusively from the local VS Code task
`test: MSTest with Coverage (Koverage)` (`.vscode/tasks.json:189-209`), which invokes
`scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`.

```powershell
# Step 0 — clear any residual pipeline (mandatory; see caveat 3)
Get-CimInstance Win32_Process -Filter "Name='pwsh.exe'" |
    Where-Object { $_.CommandLine -match 'Invoke-MSTest' } |
    ForEach-Object { Stop-Process -Id $_.ProcessId -Force }
Get-Process vstest.console,testhost,testhost.x86,dotnet-coverage -ErrorAction SilentlyContinue |
    Stop-Process -Force

# Step 1 — restore (fresh worktree only) and build
nuget restore TaskMaster.sln
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /nodeReuse:false

# Step 2 — MANDATORY discovery assertion: no agent-worktree assemblies
Get-ChildItem -Path . -Recurse -Filter '*.Test.dll' |
    Where-Object {
        $_.FullName -match '\\bin\\Debug\\' -and
        $_.FullName -notmatch '\\obj\\' -and
        $_.FullName -notmatch '\\ref\\' -and
        $_.FullName -notmatch '\\\.claude\\'
    } | Select-Object -ExpandProperty FullName
# Expect 9 assemblies. If any path contains \.claude\, HALT.

# Step 3 — capture
pwsh -NoProfile -ExecutionPolicy Bypass `
    -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 `
    -SearchRoot . -Configuration Debug `
    -CoverageOutput docs\features\active\2026-08-10-cobertura-coverage-arithmetic-441\evidence\baseline\coverage-pre-change.cobertura.xml
```

**Caveat 1 — the `\.claude\` exclusion is NOT implemented in the script.**
`scripts/vscode/Invoke-MSTestWithCoverage.ps1:296-302` filters only `\bin\<Configuration>\`, `\obj\`,
and `\ref\`. There is no `\.claude\` guard. Running with `-SearchRoot .` from the **main** checkout
(`C:\Users\DanMoisan\repos\TaskMaster`) therefore descends into `.claude\worktrees\agent-*\**` and
picks up stale sibling-worktree assemblies, producing bogus `AssemblyInitialize` signature failures
(documented in `.claude/agent-memory/atomic-planner/reference_invoke_mstest_with_coverage_script.md:12`).
In **this** worktree the hazard is currently inert — `Glob .claude/worktrees/**/*.md` returns nothing,
i.e. there is no nested worktree tree here — but the Step 2 assertion must still run, because the
script's own discovery is unguarded. Do **not** silently modify the script's filter: that is a
production behavior change outside this feature's scope (`issue.md:75-79`) and belongs in its own
issue.

**Caveat 2 — the aggregate run is known-unstable.** Documented in
`.claude/agent-memory/orchestrator/vstest-aggregate-crash-isolate-per-assembly.md:8-17`: the
9-assembly single-process run intermittently aborts with

```
The active test run was aborted. Reason: Test host process crashed
Test Run Aborted.  Total tests: Unknown
```

observed twice on #505 at 1476 and 1840 tests in. This is environmental (load-driven, concentrated in
the `QuickFiler.Test` `WinFormsPumpHost` message-pump family, tracked as **#511**), not a test failure,
and `Total tests: Unknown` means no verdict can be read from it. The documented recovery is to loop the
9 assemblies through `vstest.console.exe <dll> /InIsolation` — which on #505 gave a clean
**6435 passed, 1 skipped, 0 failed**. Note that a per-assembly isolation loop yields nine separate
`.coverage` files and therefore does **not** directly reproduce the single-document repository-wide
Cobertura figure without a `dotnet-coverage merge` step. This is a further reason to prefer §7.1.

**Caveat 3 — residual processes cause deterministic-looking hangs.** Per
`.claude/agent-memory/atomic-executor/project_timedout_mstest_leaves_detached_runner.md:8-25`: a
timed-out run leaves a detached `pwsh` runner that respawns testhosts; two concurrent pipelines then
contend over the machine-global `user.config`, producing a spurious
`ConfigurationErrorsException` in an unrelated assembly and an anomalously low `branch-rate`
(~0.61 vs ~0.76 normal). Always run Step 0 first and verify zero surviving processes.

**Expected runtime.** The same memory records the full suite at **5,702 tests, ~37 s** uninstrumented,
and recommends a **>= 8 minute** timeout for the instrumented run so it never times out and re-stacks.
`UNVERIFIED` — I could not execute the suite to measure the current instrumented wall-clock; the
9-assembly population is now ~6,436 tests. Budget >= 8 minutes and treat a shorter completion as
suspicious.

### 7.3 Threshold handling

Per `issue.md:85-87` and the delegation constraints, **no threshold may be re-tuned in this feature.**
Record the corrected figures numerically. The §3.4 observation that the corrected repository-wide line
rate for the #424 sample sits at ~85.03% — 0.03 pp above the `general-unit-test.md` 85% floor — is a
**handoff to #494**, stated as fact in evidence and nothing more.

---

## 8. Consolidated change surface for the planner

| File | Change | Est. scope |
|---|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | Add `Get-CoberturaClassLineSummary`; rewrite the inner loop of `Get-CoberturaCoverageSummary` (`:116-134`) to use it; replace `:270-273` with a direct helper call. | 1 production file, ~50-70 lines net |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | Add F1-F6 (§5.2) plus direct unit tests for the helper's precedence branches. No existing block modified. | 1 test file |
| `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/evidence/` | §7.1 A/B evidence (pre/post figures against the raw baseline ground truth). | evidence only |

Within the `.claude/rules/powershell.md:37-41` change budget (<= 2 production PowerShell files plus
tests). `Helpers.ps1` is 357 lines today; a ~60-line addition keeps it under the 500-line ceiling
(`.claude/rules/general-code-change.md`, File Size Limit). No change to
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`.

Follow-up issue candidates surfaced by this research (do not fix here):

1. Package-level `line-rate` / `branch-rate` are never recomputed after package filtering and class
   merging (§3.3), leaving stale values consumed by `scripts/temp-extract-coverage.ps1:47`.
2. A merged class retains only the primary class's `<methods>`, so its methods do not account for all
   of its class-level lines (§4b).
3. `scripts/vscode/Invoke-MSTestWithCoverage.ps1:296-302` lacks a `\.claude\` discovery exclusion
   (§7.2 caveat 1).
4. `.claude/agent-memory/atomic-executor/project_coverage_delta_reproduce_baseline_counting_method.md:34-36`
   records an incorrect generalization about root attributes (§3.5) and should be corrected once this
   fix lands.
