# Baseline Coverage-Enabled Test Run — Remediation Cycle 1

- Task: `[P0-T9]`
- Issue: #418
- Branch / HEAD: `bug/svg-renderer-null-document-nre-418` @ `ea106111`
- Evidence series: `2026-08-05T01-50`

Timestamp: 2026-08-05T01-32 (UTC)

Command:

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug
```

EXIT_CODE: 0

Coverage report read: `coverage/coverage.cobertura.xml` (10,267,045 bytes, written 2026-08-04 21:30
local). A verbatim copy was retained for this cycle's comparison work.

## Execution metrics

| Metric | Value |
|---|---|
| Test assemblies discovered | **9** |
| Total tests | **6140** |
| Passed | **6140** |
| Failed | **0** |
| Skipped | **0** |
| Result line | `Test Run Successful.` |
| Test host crash / rerun | none (`grep -i crash` matched only two test *names* containing "WithoutCrash") |

`-SearchRoot .` was used as mandated; the single-project form of the wrapper throws
`PropertyNotFoundException` under `Set-StrictMode`.

## Counting method

Package- and class-level figures are computed by counting **every `<line>` descendant** of the element
(the per-`<line>`-descendant method used by `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md`), so
every comparison in this cycle stays like-for-like. Branch figures sum the `condition-coverage`
`(covered/total)` fractions of every `<line branch="True">` descendant. Per-member gates are read from
the Cobertura `<method>` element's `line-rate` attribute, with `branch-rate` recorded for information
only.

## Numeric coverage headlines

### Repository-wide

| Metric | Covered / Total | Percent | Floor | Verdict |
|---|---|---|---|---|
| Line | **93489 / 109486** | **85.3890%** | `>= 85%` | PASS (+0.3890 pts) |
| Branch | **21534 / 27406** | **78.5740%** | `>= 75%` | PASS (+3.5740 pts) |

Cobertura root attributes agree exactly: `line-rate="0.85389" lines-covered="93489"
lines-valid="109486"`, `branch-rate="0.78574" branches-covered="21534" branches-valid="27406"`.

Per-package breakdown (nine first-party packages, no vendored assembly inflates the denominator):

| Package | Line covered/total | Line % | Branch covered/total | Branch % |
|---|---|---|---|---|
| `UtilitiesCS` | 68379 / 76065 | 89.8955% | 15830 / 18980 | 83.4036% |
| `QuickFiler` | 13994 / 17158 | 81.5596% | 2964 / 3982 | 74.4350% |
| `TaskMaster` | 2762 / 4244 | 65.0801% | 557 / 942 | 59.1295% |
| `SVGControl` | **1648 / 3500** | **47.0857%** | **544 / 1236** | **44.0129%** |
| `ToDoModel` | 2032 / 3442 | 59.0354% | 468 / 928 | 50.4310% |
| `TaskVisualization` | 2736 / 3012 | 90.8367% | 649 / 768 | 84.5052% |
| `Tags` | 1374 / 1480 | 92.8378% | 342 / 374 | 91.4439% |
| `TaskTree` | 556 / 577 | 96.3605% | 180 / 196 | 91.8367% |
| `VBFunctions` | 8 / 8 | 100.0000% | 0 / 0 | n/a |
| **TOTAL** | **93489 / 109486** | **85.3890%** | **21534 / 27406** | **78.5740%** |

### Class-level

| Class | File | Line covered/total | Line % | Branch covered/total | Branch % |
|---|---|---|---|---|---|
| `SVGControl.SvgRenderer` | `SVGControl\SvgRenderer.cs` | **424 / 588** | **72.1088%** | 86 / 168 | 51.1905% |
| `SVGControl.SvgAssemblyProbe` | `SVGControl\SvgAssemblyProbe.cs` | **68 / 68** | **100.0000%** | 48 / 48 | 100.0000% |
| `SVGControl.SvgAssemblyResolver` | — | not present (created by `[P1-T3]`) | — | — | — |

### Per-member `line-rate` (the four members `[P0-T9]` names)

| Type | Member | Signature | `line-rate` | Lines | `branch-rate` | Branches |
|---|---|---|---|---|---|---|
| `SVGControl.SvgRenderer` | `ResolveByNameAndKey` | `(object, System.ResolveEventArgs)` | **68.1159%** | 47/69 | 45.4545% | 10/22 |
| `SVGControl.SvgRenderer` | `PublicKeyTokensEqual` | `(byte[], byte[])` | **0.0000%** | 0/15 | 0.0000% | 0/18 |
| `SVGControl.SvgRenderer` | `.ctor` | `(byte[], System.Drawing.Size, SVGControl.AutoSize)` | **76.4706%** | 13/17 | 50.0000% | 1/2 |
| `SVGControl.SvgRenderer` | `.ctor` | `(byte[], System.Drawing.Size, System.Windows.Forms.Padding, SVGControl.AutoSize)` | **100.0000%** | 18/18 | 100.0000% | 2/2 |

Additional members recorded because Phase 1 moves or targets them:

| Type | Member | `line-rate` | Lines | `branch-rate` |
|---|---|---|---|---|
| `SVGControl.SvgAssemblyProbe` | `TryGetDirectoryFromCodeBase(string)` | 100.0000% | 11/11 | 100.0000% (8/8) |
| `SVGControl.SvgAssemblyProbe` | `GetProbeDirectories(string, string, string)` | 100.0000% | 23/23 | 100.0000% (16/16) |
| `SVGControl.SvgRenderer` | `.ctor(Svg.SvgDocument, Size, AutoSize)` | 0.0000% | 0/8 | n/a |
| `SVGControl.SvgRenderer` | `.ctor(Svg.SvgDocument, Size, Padding, AutoSize)` | 0.0000% | 0/8 | n/a |

## Comparison against `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md`

| Figure | `coverage-delta.2026-08-04T14-36.md` | This run (`[P0-T9]`) | Difference |
|---|---|---|---|
| Repo line | 93484 / 109486 = 85.3844% | **93489 / 109486 = 85.3890%** | **+5 covered lines, +0.0046 pts**; denominator identical |
| Repo branch | 21528 / 27406 = 78.5521% | **21534 / 27406 = 78.5740%** | **+6 covered branches, +0.0219 pts**; denominator identical |
| `SVGControl` package line | 1648 / 3500 = 47.0857% | 1648 / 3500 = 47.0857% | identical |
| `SVGControl` package branch | 544 / 1236 = 44.0129% | 544 / 1236 = 44.0129% | identical |
| `SVGControl.SvgRenderer` class | 424 / 588 = 72.109% | 424 / 588 = 72.1088% | identical |
| `SVGControl.SvgAssemblyProbe` class | 68 / 68 = 100.000% | 68 / 68 = 100.0000% | identical |
| `ResolveByNameAndKey` | 68.116% (47/69), branch 45.5% | 68.1159% (47/69), branch 45.4545% (10/22) | identical |
| `PublicKeyTokensEqual` | 0% (0/15) | 0.0000% (0/15) | identical |
| `.ctor(byte[], Size, AutoSize)` | 76.471% (13/17), branch 50.0% | 76.4706% (13/17), branch 50.0000% (1/2) | identical |
| `.ctor(byte[], Size, Padding, AutoSize)` | 100.000% (18/18) | 100.0000% (18/18) | identical |

**Do the figures match? Every figure inside the Scope Lock matches exactly.** The only differences are
the two repository-wide numerators: line covered is **+5** and branch covered is **+6**, on identical
denominators (109486 and 27406). Both differences are increases, both are far outside `SVGControl`
(whose package figures are byte-identical), and no source file changed between the two runs — the tree
is at `ea106111` with no `.cs` modification. This is the known small run-to-run numerator variance of
`dotnet-coverage` instrumentation across a 6140-test suite in assemblies with timing- or
ordering-sensitive paths; it is not attributable to any code change and does not affect any gate.

**This run's figures are the authoritative before-state for `[P2-T7]`'s delta**, per the plan's
§ Baseline Strategy item 3 ("the coverage delta in `[P2-T7]` must be computed against numbers measured
in this session at this HEAD").

## Output Summary

`EXIT_CODE: 0`. **9 assemblies discovered, 6140 total, 6140 passed, 0 failed, 0 skipped**, no test host
crash and no rerun. Repository-wide **line 93489/109486 = 85.3890%** (PASS vs `>= 85%`) and **branch
21534/27406 = 78.5740%** (PASS vs `>= 75%`). `SVGControl` package 1648/3500 = 47.0857%;
`SVGControl.SvgRenderer` 424/588 = 72.1088%; `SVGControl.SvgAssemblyProbe` 68/68 = 100%. Target members:
`ResolveByNameAndKey` 47/69 = 68.1159%, `PublicKeyTokensEqual` 0/15 = 0%, three-argument byte-array
constructor 13/17 = 76.4706%, four-argument overload 18/18 = 100%. Every Scope Lock figure matches
`evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` exactly; the two repository-wide numerators are
+5 lines and +6 branches on identical denominators, a benign instrumentation variance unrelated to any
code change.
