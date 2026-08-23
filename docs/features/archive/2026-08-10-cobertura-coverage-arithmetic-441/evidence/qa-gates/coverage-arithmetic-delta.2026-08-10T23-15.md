# Consolidated Coverage-Arithmetic A/B Delta (P5-T3)

Timestamp: 2026-08-10T23-15

Tabulates pre-change versus post-change for both deterministic A/B experiments, using the concrete
integers captured in P0-T11, P0-T12, P5-T1 and P5-T2. Both experiments hold their input fixed —
neither involves a test run — so the deltas below isolate the arithmetic change and are not
confounded by `dotnet-coverage` denominator nondeterminism.

Command: see the four source artifacts, each of which records its exact command and full output:

- `<FEATURE>/evidence/baseline/prechange-generator-parity.2026-08-10T22-30.md` (P0-T11)
- `<FEATURE>/evidence/baseline/prechange-package-filtered.2026-08-10T22-30.md` (P0-T12)
- `<FEATURE>/evidence/qa-gates/postchange-generator-parity.2026-08-10T23-15.md` (P5-T1)
- `<FEATURE>/evidence/qa-gates/postchange-package-filtered.2026-08-10T23-15.md` (P5-T2)

EXIT_CODE: 0

Output Summary:

```
Experiment A (raw baseline document, generator parity):
  pre  161086 / 113219 / 46218 / 26944
  post  79957 /  56124 / 23109 / 13472   == the document's own root attributes
Experiment B (post-processed document, package-filtered):
  pre  110849 / 94937 / 0.856453
  post  62345 / 53013 / 0.850317
Every pre-change figure is strictly greater than its post-change counterpart.
```

---

## Experiment A — generator parity over the raw baseline document

Input:
`.../424/evidence/baseline/coverage-baseline.cobertura.xml`
(raw `dotnet-coverage` output; its own root attributes are ground truth).

| Quantity | PRE-change | POST-change | Ground truth | Delta | Pre > Post? |
| --- | --- | --- | --- | --- | --- |
| `LinesValid` | **161086** | **79957** | 79957 | **-81129** | **yes** |
| `LinesCovered` | **113219** | **56124** | 56124 | **-57095** | **yes** |
| `BranchesValid` | **46218** | **23109** | 23109 | **-23109** | **yes** |
| `BranchesCovered` | **26944** | **13472** | 13472 | **-13472** | **yes** |

Every post-change figure lands **exactly** on ground truth. The pre-change branch figures were
precisely double ground truth, the signature of the descendant-axis double count.

## Experiment B — package-filtered reprocessing of the post-processed document

Input:
`.../424/evidence/qa-gates/coverage-final.cobertura.xml`.

| Quantity | PRE-change | POST-change | Delta | Pre > Post? |
| --- | --- | --- | --- | --- |
| `lines-valid` | **110849** | **62345** | **-48504** | **yes** |
| `lines-covered` | **94937** | **53013** | **-41924** | **yes** |
| `line-rate` | **0.856453** | **0.850317** | **-0.006136** (-0.61 pp) | **yes** |
| `branches-valid` | 27848 | 15828 | -12020 | yes |
| `branches-covered` | 22001 | 12445 | -9556 | yes |
| `branch-rate` | 0.790039 | 0.786265 | -0.003774 (-0.38 pp) | yes |

## Statement required by the task

**Each pre-change figure is strictly greater than its post-change counterpart**, in both experiments
and for every quantity measured — lines, branches and the derived rates alike. The reductions are
the removal of double counting, not a loss of coverage: not one `<line>` or `<method>` element was
altered by this change, and no `hits` value differs from its input (pinned by fixture F6).

The `-48504` reduction in Experiment B's `lines-valid` equals exactly the method-level `<line>`
element count the research measured for that document (62345 class-level + 48504 method-level =
110849), so the accounting closes independently.
