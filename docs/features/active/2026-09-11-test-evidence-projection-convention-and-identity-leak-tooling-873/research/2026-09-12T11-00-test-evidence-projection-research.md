# Research — Test Evidence Projection Convention and Identity-Leak Tooling (Issue #873)

- **Issue:** #873
- **Feature folder:** `docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/`
- **Timestamp:** 2026-09-12T11-00
- **Mode:** research only. No production source, configuration, or evidence file was modified.

## Tooling constraint affecting this research

The Bash tool is **disabled entirely** in this session (`No such tool available: Bash. Bash is
disabled for this session, in subagents as well as here.`). No `git`, `gh`, or `pwsh` command could
be executed. Every finding below therefore comes from the `Read`, `Grep`, and `Glob` tools over the
working tree only. Two consequences are recorded explicitly where they matter:

- **No byte sizes were measured.** Where a byte size is reported it is quoted from a committed
  evidence artifact that recorded it, and is attributed to that artifact.
- **No git history was consulted.** Claims about commits (for example `d0955dc4`) are quoted from
  committed artifacts or agent-memory files, not verified against the object database.

Line numbers below are as rendered by the `Read` tool. Where a file's last rendered line is blank,
the last **content** line is reported and the trailing-newline ambiguity is noted as `+/-1`.

---

## R1 — JaCoCo package-level projection: schema, examples, and whether anything generates it

### R1.1 Every `.jacoco.xml` in the worktree

`Glob **/*.jacoco*` returns 17 files, all under `docs/features/*/evidence/`:

| Path | Shape |
|---|---|
| `docs/features/archive/2026-08-08-ribbon-engine-readiness-guard-503/evidence/baseline/coverage-baseline.jacoco.xml` | projection |
| `docs/features/archive/2026-08-08-ribbon-engine-readiness-guard-503/evidence/qa-gates/coverage-final.jacoco.xml` | projection |
| `docs/features/archive/2026-08-08-ribbon-engine-readiness-guard-503/evidence/qa-gates/coverage-remediation-final.jacoco.xml` | projection |
| `docs/features/archive/2026-08-08-ribbon-engine-readiness-guard-503/evidence/remediation-baseline/coverage-remediation-baseline.jacoco.xml` | projection |
| `docs/features/archive/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/baseline/coverage-baseline.jacoco.xml` | projection |
| `docs/features/archive/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/qa-gates/coverage-postchange.jacoco.xml` | projection |
| `docs/features/archive/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/baseline/powershell-baseline.jacoco.xml` | (Pester-produced, not checked here) |
| `docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/baseline/baseline-coverage.jacoco.xml` | **projection — the #646 reference** |
| `docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/qa-gates/final-coverage.jacoco.xml` | **projection — the #646 reference** |
| `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/baseline/p0-t15-coverage.jacoco.xml` | projection |
| `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/qa-gates/p2-t7-coverage.jacoco.xml` | projection |
| `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/coverage-baseline.jacoco.xml` | projection variant |
| `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/coverage-post-change.jacoco.xml` | projection variant |
| `docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/evidence/baseline/coverage-baseline.jacoco.2026-09-05T10-49.xml` | projection variant |
| `docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/evidence/qa-gates/coverage-final.jacoco.2026-09-05T10-49.xml` | projection variant |
| `docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t8-coverage-baseline.jacoco.xml` | **NOT a projection** — genuine Pester JaCoCo |
| `docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t7-coverage-final.jacoco.xml` | **NOT a projection** — genuine Pester JaCoCo |

The two `-815` files are full Pester `CodeCoverage.OutputFormat = 'JaCoCo'` reports (1,126 lines;
`<sessioninfo>`, `<class>`, `<method>`, `<sourcefile>`, `<line nr= mi= ci= mb= cb=>`, and
`INSTRUCTION`/`METHOD`/`CLASS` counters). **Do not use them as the target schema.** They are the
upstream tool's native output, not the projection this issue is about.

### R1.2 The exact target schema (the #646 / PR #718 shape)

The canonical reference is
`docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/qa-gates/final-coverage.jacoco.xml`,
62 lines. Quoted verbatim, lines 1-9 and 62:

```xml
<report name="TaskMaster">
  <package name="Mono.Reflection">
    <counter type="LINE" missed="402" covered="0" />
    <counter type="BRANCH" missed="0" covered="0" />
  </package>
  <package name="System.Interactive">
    <counter type="LINE" missed="765" covered="5" />
    <counter type="BRANCH" missed="0" covered="0" />
  </package>
...
</report>
```

Schema, stated exactly:

- **No XML declaration** and **no DOCTYPE** in the #646 pair. Line 1 is the root element.
- Root element `report`, single attribute `name`, value `TaskMaster`.
- Zero or more child `package` elements, each with a single attribute `name` holding the
  Cobertura package name (the assembly name).
- Each `package` contains exactly two `counter` children, in this order:
  1. `<counter type="LINE" missed="<int>" covered="<int>" />`
  2. `<counter type="BRANCH" missed="<int>" covered="<int>" />`
- Indentation: two spaces per level. Self-closing `counter` tags are written with a space before
  `/>`.
- No `filename`, `sourcefilename`, `class`, `method`, `sourcefile`, `line`, `INSTRUCTION`,
  `METHOD`, or `CLASS` elements or counters appear anywhere.

**Documented shape variations across the other committed projections** (the plan must choose one
and state it, because they are not identical):

| Variation | Evidence |
|---|---|
| XML declaration present (`standalone="yes"`) | `.../503/evidence/qa-gates/coverage-final.jacoco.xml:1` |
| XML declaration absent | `.../646/evidence/qa-gates/final-coverage.jacoco.xml:1` |
| Root `name` is a longer descriptive string | `.../781/evidence/qa-gates/coverage-final.jacoco.2026-09-05T10-49.xml:1` (`name="TaskMaster C# (converted from Cobertura)"`); `.../678/evidence/qa-gates/coverage-post-change.jacoco.xml:15` |
| Package element written on a single line | `.../781/evidence/qa-gates/coverage-final.jacoco.2026-09-05T10-49.xml:2-10` |
| `BRANCH` counter omitted per package, and document-level `LINE`/`BRANCH` counters added as direct `report` children | `.../678/evidence/qa-gates/coverage-post-change.jacoco.xml:16-44` |
| XML comment block preceding the root, recording derivation method | `.../678/evidence/qa-gates/coverage-post-change.jacoco.xml:2-14` |
| Packages restricted to the nine first-party assemblies | `.../503/evidence/qa-gates/coverage-final.jacoco.xml:3-38` |
| **All 15 packages retained**, including vendored third-party and the `.Test` assembly | `.../646/evidence/qa-gates/final-coverage.jacoco.xml:2-61` |
| Self-closing written as `/>` with no preceding space | `.../678/...:17` (`covered="9837"/>`) |

The #646 pair is the shape the issue names. It is **all-packages**, **no XML declaration**,
**two counters per package**, **no document-level counter**.

### R1.3 Concrete example artifact path and byte size

`docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/qa-gates/final-coverage.jacoco.xml`.

Its own substitution record states the size. From
`docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/qa-gates/coverage-artifact-substitution.2026-09-01T16-41.md:96-98`:

| File | Bytes | Lines |
|---|---|---|
| `evidence/baseline/baseline-coverage.jacoco.xml` (added) | 2,359 | 62 |
| `evidence/qa-gates/final-coverage.jacoco.xml` (added) | 2,359 | 62 |
| Replacement total | 4,718 | 124 |

The same record, lines 93-95, states the two deleted raw Cobertura files were 26,064,187 and
26,067,082 bytes (52,131,269 total, 892,256 lines). This is the 2 KB figure the issue cites.

### R1.4 Does anything in this repository GENERATE the projection?

**No. Nothing in this repository generates it.** This is the load-bearing negative finding.

- `SearchScope:` the whole worktree.
  `SearchPatterns:` case-insensitive `jacoco`, with `docs/**` and `.claude/agent-memory/**`
  excluded, `head_limit 0`.
  `SearchResult:` exactly four files — `AGENTS.md:877`,
  `.github/instructions/github-actions-ci-cd-best-practices.instructions.md:317` (both incidental
  prose about Java tooling), `.codex/hooks/validate-feature-review-coverage.ps1`, and
  `.claude/hooks/validate-feature-review-coverage.ps1`. The two hooks **read** JaCoCo; neither
  writes it.
- `SearchScope:` `scripts/`. `SearchPatterns:` `(?i)jacoco|<report|counter type`.
  `SearchResult: none`.
- `SearchScope:` `tests/`. `SearchPatterns:` `(?i)jacoco`. `SearchResult: none`.
- `SearchScope:` whole worktree excluding `docs/**`, `.claude/agent-memory/**`, `**/*.xml`.
  `SearchPatterns:` `&lt;report|"&lt;report|'&lt;report|report name=`. `SearchResult: none`.

Every existing projection instance was produced by a **throwaway converter written to the session
scratchpad outside the repository and never committed.** This is stated in the record itself at
`docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/qa-gates/coverage-artifact-substitution.2026-09-01T16-41.md:27-30`:

> `Convert-CoberturaToJacoco.ps1` was written to the session scratchpad outside the repository
> and is not committed, per the repository rule that no helper script is written under
> `evidence/`.

The #503 precedent is the same pattern:
`docs/features/archive/2026-08-08-ribbon-engine-readiness-guard-503/remediation-plan.2026-08-08T14-26.md:92-99`
specifies "Helper script B — Cobertura to first-party JaCoCo projection ... Written to
`<SCRATCH>\ConvertCoberturaToJacoco.ps1`", and line 99 of that plan is the only prose in the
repository that states the target schema:

> Emit the exact shape of the existing `<FEATURE>\evidence\qa-gates\coverage-final.jacoco.xml`: a
> `<report name="TaskMaster">` root containing one `<package name="...">` element per included
> package, each holding a `<counter type="LINE" missed="..." covered="..." />` and a
> `<counter type="BRANCH" missed="..." covered="..." />`.

### R1.5 The #646 conversion method, as recorded

From `coverage-artifact-substitution.2026-09-01T16-41.md:39-69`:

1. Streamed with `System.Xml.XmlReader`; the 26 MB sources were never loaded into a DOM.
2. The `package` element places `name` **after** `line-rate`, so a literal text search for
   `package name=` returns zero matches; `XmlReader.GetAttribute` reads by name regardless of
   order (line 44-47).
3. Within each `class`, `line` elements are collected into a map keyed by the `number` attribute;
   Cobertura repeats them across `method` blocks and the class-level `lines` block, so
   deduplication by line number within the class is required or the totals do not reconcile;
   where a line number recurs the **maximum `hits`** is kept (lines 48-51).
4. `hits > 0` counts covered, `hits == 0` counts missed (lines 52-53).
5. Branch counters are derived from the `(covered/total)` pair inside each `condition-coverage`
   attribute (lines 54-55).
6. A **mandatory reconciliation gate**: the summed `LINE` counters across all packages must equal
   the source Cobertura root's `lines-covered` and `lines-valid` exactly (lines 71-87). Neither
   source file was deleted until its reconciliation passed.
7. `BRANCH` counters were retained at `missed="0" covered="0"` rather than omitted "so the JaCoCo
   shape stays uniform across packages" (line 69). In that run the collector genuinely emitted no
   branch data (lines 59-68).
8. Incidental benefit recorded at lines 141-145: the projection carries no `filename` attribute,
   which is what removes the absolute host paths the Cobertura `class` elements carried.

This method is identical in substance to what `Get-CoberturaClassLineSummary` already implements
in production (see R2), which is the material fact for the plan.

---

## R2 — What the existing helpers already compute

All three files under `scripts/vscode/` were read end to end.

### `Invoke-MSTestWithCoverage.PackageRate.ps1` (65 content lines)

`Get-CoberturaPackageLineSummary` — line 3.

- Parameter: `-PackageNode` (`[System.Xml.XmlElement]`, mandatory), a Cobertura `<package>`
  element (lines 39-42).
- Body: iterates `$PackageNode.SelectNodes('.//class')` and accumulates
  `Get-CoberturaClassLineSummary` (lines 49-55).
- Output (lines 57-64): a `pscustomobject` with `LineRate`, `BranchRate`, `LinesCovered`,
  `LinesValid`, `BranchesCovered`, `BranchesValid`. **Every value is a string.**

**This is the function that already yields per-package line and branch counts.**
`LinesCovered`/`LinesValid` and `BranchesCovered`/`BranchesValid` are exactly the four numbers a
JaCoCo projection writer needs per package, once converted to the JaCoCo `missed`/`covered`
parameterisation.

### `Invoke-MSTestWithCoverage.Helpers.ps1` (470 content lines, +/-1)

Dot-sources the other four part files at lines 2-5.

- `Get-KoverageProjectAllowlist` — line 7. Parameter `-RepoRoot` (optional, defaults to
  `(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path`). Scans `*.csproj`/`*.vbproj`/`*.fsproj`,
  excludes `bin`/`obj`/`packages`, reads `<AssemblyName>` or falls back to the filename, and drops
  any name ending `.Test` (lines 15-50). Returns the sorted first-party assembly names.
- `ConvertTo-KoverageRelativePath` — line 53. Parameters `-Path`, `-RepoRoot`, `-PathSeparator`.
- `Get-CoberturaCoverageSummary` — line 102. Parameter `-XmlDocument` (`[xml]`). Throws
  `'Cobertura XML does not contain a <packages> node.'` when `//packages` is absent (lines
  115-118). Sums `Get-CoberturaPackageLineSummary` over `./package` **unconditionally, with no
  allowlist** (lines 120-126). Same six-field string output (lines 128-135).
- `Get-CoberturaLineConditionCoverageParts` — line 138. Parameter `-LineNode`. Parses
  `condition-coverage="NN% (c/t)"` into `Covered`/`Total` integers (lines 146-156).
- `Get-CoberturaClassLineSummary` — line 159. Parameter `-ClassNode`. **This is the
  de-duplication rule.** It enumerates `./lines/line` then `./methods/method/lines/line`
  (lines 194-195), keys by line number, resolves a repeated key by taking the maximum `hits`,
  treats the line as a branch if either entry is a branch, and retains the condition-coverage of
  the entry with the larger denominator (lines 197-233). Outputs `LineMap`, `TotalLines`,
  `CoveredLines`, `TotalBranches`, `CoveredBranches` as **integers** (lines 250-256).
- `Merge-CoberturaClassesByFilename` — line 259. Parameter `-XmlDocument`.
- `ConvertTo-KoverageCoberturaXml` — line 406. Parameters `-XmlContent` (string), `-RepoRoot`,
  `-ProjectNames` (defaults to `(Get-KoverageProjectAllowlist)`), `-PathSeparator`. Removes
  non-allowlisted packages (lines 430-434), rewrites `class/@filename` to repo-relative
  (lines 436-438), applies the closure filter and the merge (lines 440-441), injects `<sources>`
  (lines 443-452), recomputes the six root attributes (lines 454-460), and serialises with an
  indented `XmlTextWriter` (lines 462-469).

### `Invoke-MSTestWithCoverage.FirstParty.ps1` (162 content lines)

- `Get-CoberturaFirstPartyCoverageSummary` — line 3. Parameters `-XmlDocument` (`[xml]`,
  mandatory) and `-ProjectNames` (`[string[]]`, optional, default
  `(Get-KoverageProjectAllowlist)`). Selects `/coverage/packages/package`, skips any package whose
  `name` is outside `ProjectNames` (lines 69-74), accumulates `Get-CoberturaPackageLineSummary`
  (lines 76-81). Output adds `LinePercent` and `BranchPercent` as two-decimal invariant-culture
  strings to the six-field shape (lines 83-92).
- `Format-CoberturaFirstPartyCoverageSummary` — line 95. Parameter `-Summary`. Returns
  exactly `"First-party coverage: lines C/V (P%), branches C/V (P%)"` (lines 117-120).
- `Get-CoberturaFirstPartyCoverageReport` — line 123. Parameters `-CoberturaXml` (string,
  mandatory) and `-ProjectNames`. Composes the two above (lines 158-161). This is the
  one-line summary the issue names, and it is already wired at
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1:345`.

### What a projection writer would additionally need

Reported as fact, not design:

1. **A per-package enumeration that survives the allowlist decision.**
   `Get-CoberturaFirstPartyCoverageSummary` iterates packages but discards the per-package
   summaries into running totals (lines 76-81); it returns only the aggregate. No existing
   function returns a *collection* of `(packageName, summary)` pairs. `Get-CoberturaCoverageSummary`
   has the same shape (lines 120-126). So the per-package loop exists twice but its per-package
   output is not exposed to any caller.
2. **A `missed` value.** Every helper returns `LinesCovered`/`LinesValid` and
   `BranchesCovered`/`BranchesValid`. JaCoCo's `missed` is `valid - covered`. That subtraction
   exists nowhere today.
3. **XML serialisation of a new document.** `ConvertTo-KoverageCoberturaXml` mutates and
   re-serialises the *input* Cobertura document (lines 424, 462-469). No helper constructs a new
   `report`/`package`/`counter` document.
4. **The reconciliation assertion.** The mandatory gate described in R1.5 (summed package LINE
   counters must equal the Cobertura root `lines-covered`/`lines-valid`) has no production
   implementation. `Assert-CoberturaLineCoverageThreshold` asserts a *threshold*, not a
   *reconciliation* (see R12).

Note one arithmetic caveat the plan must be aware of: `ConvertTo-KoverageCoberturaXml` writes the
root `lines-covered`/`lines-valid` attributes from `Get-CoberturaCoverageSummary`
(`Invoke-MSTestWithCoverage.Helpers.ps1:454-460`), which is the *same* deduplicated rule the
per-package helper uses. So a projection built from `Get-CoberturaPackageLineSummary` over the
post-processed document reconciles to that document's root attributes by construction. It would
**not** reconcile to a *raw* collector document's root attributes, because the raw root is written
by `dotnet-coverage` under its own (class-level-only) rule.

---

## R3 — Every vstest invocation reachable from `scripts/vscode/`, and the runsettings

### R3.1 The two invocation sites

**Site 1 — `scripts/vscode/Invoke-MSTest.ps1` (plain run).**

- Argument array built by `Get-VsTestArgumentList`, declared at line 37.
- Parameters: `-TestAssembly` (`[string[]]`), `-RunSettingsPath` (`[string]`), both mandatory
  (lines 46-52).
- The entire returned array, line 54:
  `return @($TestAssembly) + @("/Settings:$RunSettingsPath", '/InIsolation', '/TestCaseFilter:TestCategory!=LiveOutlook')`
- **Neither `/Logger` nor `/ResultsDirectory` is passed.** Neither.
- Executed through the mockable seam `Invoke-VsTestExe` (line 57, body at line 74
  `& $VsTestPath @VsTestArgs`), called from `Invoke-MSTestMain` at line 194. The argument array is
  built at line 188.
- Executable resolved by `Get-VsTestConsolePath` (line 77, vswhere at line 93).
- Wired to the VS Code task `test: MSTest (vstest.console)` at `.vscode/tasks.json:172-191`.

**Site 2 — `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (coverage run, vstest nested under
`dotnet-coverage`).**

- Argument array built by `Get-DotnetCoverageArgumentList`, declared at line 41.
- Parameters: `-OutputPath`, `-CoverageConfig`, `-VsTestPath`, `-TestAssembly` (`[string[]]`),
  `-RunSettingsPath`, all mandatory (lines 51-66).
- The entire returned array, lines 70-77:
  ```
  return @(
      'collect',
      '--output', $OutputPath,
      '--output-format', 'cobertura',
      '--settings', $CoverageConfig,
      '--', $VsTestPath
  ) + @($TestAssembly) + @("/Settings:$RunSettingsPath", '/InIsolation', '/TestCaseFilter:TestCategory!=LiveOutlook')
  ```
- **Neither `/Logger` nor `/ResultsDirectory` is passed.** Neither.
- Executed through the mockable seam `Invoke-DotnetCoverageExe` (line 138, body at line 153
  `& dotnet-coverage @DotnetCoverageArgs`), called from `Invoke-DotnetCoverageCollection` at
  line 233; the array is built at lines 225-230.
- Executable resolved by `Invoke-VsWhereExe` (line 156), called at lines 284-287.
- Wired to the VS Code task `test: MSTest with Coverage (Koverage)` at
  `.vscode/tasks.json:193-212`.

No third site exists under `scripts/vscode/`.
`SearchScope:` whole worktree excluding `docs/**`, `.claude/agent-memory/**`, `**/*.trx`,
`**/*.xml`. `SearchPatterns:` `(?i)vstest`. `SearchResult:` the only other executable references
are `scripts/vscode/TestProcessCleanup.ps1:1,10,22,70` (a process-kill helper that never launches
vstest), `.github/workflows/_mstest-coverage.yml:81-99`, and `.codex/codex-web-setup.sh:293-295`.

### R3.2 The CI invocation, for contrast

`.github/workflows/_mstest-coverage.yml:98-99`:

```
New-Item -ItemType Directory -Path 'TestResults' -Force | Out-Null
& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"
```

CI passes `/Logger:trx` with **no `LogFileName=`** and **no `/ResultsDirectory:`**. It therefore
produces the default `<account>_<HOST>_<timestamp>.trx` filename under `TestResults/`, and uploads
it as an artifact (lines 104-112). The CI runner's account and host are the GitHub runner's, not
the developer's, so this is a different risk class; it is recorded because the issue's expected
behaviour names "every vstest invocation in `scripts/vscode/`" and CI is outside that scope.

### R3.3 What the runsettings say

Both files were read in full.

**`scripts/vscode/TaskMaster.cli.runsettings` — 9 lines, entire file:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <MSTest>
    <Parallelize>
      <Workers>0</Workers>
      <Scope>ClassLevel</Scope>
    </Parallelize>
  </MSTest>
</RunSettings>
```

No `RunConfiguration`, no `ResultsDirectory`, no `LoggerRunSettings`.

**`TaskMaster.runsettings` (repo root) — 30 lines.** Lines 3-8 are the same `MSTest/Parallelize`
block; lines 9-29 are a `DataCollectionRunSettings` block carrying the Code Coverage collector with
seven `ModulePath` excludes. No `RunConfiguration`, no `ResultsDirectory`, no `LoggerRunSettings`.

**Conclusion for R3:** nothing currently sets a results directory or a TRX log file name, at any
layer — not the argument builders, not either runsettings file. Note also that
`scripts/vscode/Invoke-MSTestWithCoverage.ps1:15-39` documents deliberately that the CLI
runsettings is the *off-root* file and carries no data collector; Visual Studio auto-detects the
repo-root one. Any `ResultsDirectory` added to the repo-root file would apply to Visual Studio runs
but not to either script, because neither script passes it.

---

## R4 — TRX default filename and its identifiers

### R4.1 A committed TRX proving the default filename

`Glob **/*.trx` returns more than 100 tracked files. One carries the **unredacted default
filename**: a file directly under
`docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/regression-testing/`
whose base name has the form `<account>_<HOST>_<yyyy-MM-dd>_<HH>_<mm>_<ss>_net481.trx`. The
identifier values are not reproduced here. Two sibling files in the same directory
(`p2-t2-expect-fail.trx`, `p4-t1-post-fix-confirm.trx`) are the redacted copies of the same runs.

### R4.2 Attributes that carry each identifier kind

Verified by grep over that directory. Attribute **names** only:

| Identifier kind | Element | Attribute | Line (in the default-named file) |
|---|---|---|---|
| account + host, combined | `TestRun` | `name` (value is `<account>@<HOST> <timestamp>`) | 2 |
| host + account, domain form | `TestRun` | `runUser` (value is `<HOST>\<account>`) | 2 |
| host | `UnitTestResult` | `computerName` | 8 |
| account + host, in a directory name | `Deployment` | `runDeploymentRoot` | 5 |
| absolute checkout path, **lowercased** | `UnitTest` | `storage` | 42 |
| absolute checkout path, original casing | `TestMethod` | `codeBase` | 44 |

The lowercasing of `storage` versus the preserved casing of `codeBase` is confirmed in the
committed pair at lines 42 and 44 and is documented in
`.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md:17-20` and in
`.claude/agent-memory/_shared_no_absolute_host_paths.md:58-68`.

### R4.3 Placeholder escaping — the XML-attribute trap, already resolved once

The redacted sibling at
`.../813/evidence/regression-testing/p2-t2-expect-fail.trx:2` reads
`name="&lt;user&gt;@&lt;host&gt; ..."` and `runUser="&lt;host&gt;\&lt;user&gt;"`, and line 61 reads
`storage="&lt;repo-root&gt;\quickfiler.test\..."`. The angle brackets are **XML-escaped**, so the
document parses and the parsed attribute value is exactly the placeholder. This is the repair
prescribed at `.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md:42-52`
and at `.claude/agent-memory/_shared_no_absolute_host_paths.md:70-88`. Raw `<` in an attribute value
made all 19 TRX files on feature 488 unparseable, and the same rule mandated in the #662 plan
produced six more corrupt files.

### R4.4 What a minimal pass/fail summary must read

Counts: `ResultSummary/Counters`, a single element with the whole tally as attributes. At
`.../813/evidence/regression-testing/p4-t1-post-fix-confirm.trx:54-55`:

```xml
<ResultSummary outcome="Completed">
  <Counters total="1" executed="1" passed="1" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

The issue's four required numbers map as: `passed` -> `@passed`, `failed` -> `@failed`,
`total` -> `@total`. **There is no `skipped` attribute.** The nearest are `notExecuted` and
`inconclusive`; a "skipped" figure must be defined in terms of one of those, or as
`total - executed`.

The run-level verdict is `ResultSummary/@outcome` (`Completed` at line 54 of the passing file;
`Failed` at line 73 of the failing one).

Failed-test names: each `Results/UnitTestResult` element carries `@testName` and `@outcome`
(`.../p2-t2-expect-fail.trx:8`). The failed set is the `UnitTestResult` elements whose `@outcome`
is `Failed`. Failure detail, if wanted, is under
`UnitTestResult/Output/ErrorInfo/Message` and `.../StackTrace` (lines 38-39 onward). Class names
come from `TestDefinitions/UnitTest/TestMethod/@className` (line 63), not from the result element.

One namespace caveat for any XPath the plan writes: the root declares the default namespace
`xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"` (line 2), so an unprefixed XPath
such as `//Counters` selects nothing. A namespace manager or a local-name predicate is required.

---

## R5 — File-size headroom under the 500-line ceiling

Counted with the `Read` tool's line numbering. "Last content line" is the last line bearing
non-whitespace text; the `+/-1` column notes whether `Read` rendered an additional trailing blank
line number (a trailing-newline artefact I could not disambiguate without `wc`).

| File | Last content line | Trailing blank rendered | Headroom to 500 |
|---|---|---|---|
| `scripts/vscode/Invoke-MSTest.ps1` | 202 | yes (203) | **297 (conservatively 297)** |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 351 | yes (352) | **148** |
| `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 413 | yes (414) | **86** |
| `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` | 162 | yes (163) | **337** |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 470 | yes (471) | **29** |
| `scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` | 65 | no | **435** |
| `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | 56 | no | **444** |

`Invoke-MSTestWithCoverage.Helpers.ps1` has **29 lines of headroom** and cannot host a new
function. Its own doc comments say so twice, independently:
`Invoke-MSTestWithCoverage.Threshold.ps1:14-17` ("that file reached the repository's 500-line
ceiling once issue #733's fixes landed") and `Invoke-MSTestWithCoverage.FirstParty.ps1:24-27`
("already within a few lines of the repository's 500-line ceiling"). The established remedy is a
new part file dot-sourced from `Helpers.ps1` line 2-5; four such part files already exist.

`Invoke-MSTestWithCoverage.ps1` has 148 lines of headroom and is the entry point that would need
the projection/deletion wiring.

---

## R6 — The in-memory test fixture pattern

`tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1`, 271 content lines, read in
full. The pattern has four parts.

**1. Strict mode, then a single dot-source of `Helpers.ps1`.** Lines 1-5:

```powershell
Set-StrictMode -Version Latest

BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    . (Join-Path $script:repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1')
```

`Helpers.ps1` dot-sources the other four part files at its own lines 2-5, so this single line
resolves every function. The repo root is reached by three `..` levels from
`tests/scripts/vscode/`. The identical two lines appear in
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1:3-7` (using a local
`$repoRoot` rather than `$script:repoRoot`).

**2. Test-only helper functions are declared inside `BeforeAll`, not at file scope.** Lines 7-8
give the reason verbatim:

```
# Defined inside BeforeAll rather than at file scope because Pester 5 runs each It block in a
# child scope of the containing block, so only a function defined here is resolvable from one.
```

`Get-DescendantAxisCoverageTally` (lines 9-78) is the example.

**3. Cobertura documents are supplied as PowerShell here-strings assigned to `$script:` variables,
then cast to `[xml]` inside the `It` block.** No file is ever written. Lines 85-119 declare
`$script:duplicateRowFixture`; lines 123-168 declare `$script:allowlistFixture`. Consumption at
line 176: `[xml]$document = $script:duplicateRowFixture`. Two smaller fixtures are declared inline
inside their own `It` blocks as here-strings (lines 219-222 and 231-240). Every fixture is a
complete `<?xml ...?><coverage ...><packages>...` document. The fixture design rationale is stated
at lines 80-84 — non-uniform duplication multiplicities, because "a uniformly duplicated fixture
leaves every ratio unchanged and so cannot discriminate a correct aggregation from the defective
one."

**4. `Get-KoverageProjectAllowlist` is NOT overridden — it is bypassed, and its default is asserted
from the AST.** This is the part the issue prompt phrases as an override, and the tree does not
support that phrasing.

- Every call passes `-ProjectNames @('Ns')` explicitly (lines 178, 197, 224, 242, 255, 267), so the
  default is never evaluated.
- The default itself is verified at lines 203-214 by reading the function's AST, never by invoking
  it. Lines 204-207 state the reason:

  ```
  # AC4. Read from the function AST rather than by invoking the function: the default is
  # evaluated at parameter binding and derives its names from the tracked project files, so
  # it can never contain the fixture package name, and invoking it would additionally
  # perform a recursive repository scan that a unit test must not do.
  ```

  The assertion, line 213:
  `$parameterAst.DefaultValue.Extent.Text | Should -Be '(Get-KoverageProjectAllowlist)'`

- `SearchScope:` `tests/`. `SearchPatterns:` `Get-KoverageProjectAllowlist`.
  `SearchResult:` seven hits, in two files only. In
  `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` it is invoked for real (lines 391, 399, and with
  `-RepoRoot 'C:\fake'` at line 420). **No `Mock Get-KoverageProjectAllowlist` exists anywhere in
  the repository.** The pattern to reuse is explicit-parameter injection plus AST assertion, not
  mocking.

Mocking, where used at all in this tree, targets the named wrapper seams
(`Mock Invoke-VsTestExe` at `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:81-85`, with
a mock `param()` block matching production exactly). That is the pattern for asserting a new
`/ResultsDirectory:` / `/Logger:` argument without launching an executable.

---

## R7 — Identifier leak sites

Identifier **kinds** and line numbers only; no literal values are reproduced. The "substitution
target" column is the load-bearing one, per the corruption precedent in R4.3.

| # | File | Line | Identifier kind(s) | Substitution target |
|---|---|---|---|---|
| 1 | `TaskMaster/TaskMaster.csproj` | 37 | absolute user-profile path, account name, **employer organization name** (inside a OneDrive tenant folder name) | **XML element TEXT** — `<PublishUrl>...</PublishUrl>`. Element text, not an attribute value. A raw `<` here would still be illegal XML; escape or, better, use a relative value (see R8). |
| 2 | `.vscode/settings.json` | 27 | absolute user-profile path (forward-slash form, lowercase drive letter), account name | **JSON string** — an element of the `powerquery.client.additionalSymbolsDirectories` array. JSON has no markup characters, so `<...>` placeholders are safe here, but the value is consumed by a VS Code extension as a real directory path; a placeholder makes the setting non-functional rather than corrupt. |
| 3 | `.claude/agent-memory/epic-orchestrator/feedback_measure_whole_volume_before_blaming_worktrees.md` | 14 | absolute user-profile path (a `AppData\Local\Temp` probe file), account name | **Markdown prose**, inside an inline-code span. Safe for `<user-profile>`. |
| 4 | `.claude/agent-memory/feature-review/project_464-review-residuals.md` | 17 | absolute user-profile path (a `.git/info/exclude` path), account name | **Markdown prose**, inside an inline-code span near the end of the "Operational lesson" paragraph. Safe for `<repo-root>` / `<user-profile>`. |
| 5 | `.claude/agent-memory/feature-review/project_488-review-residuals.md` | 13 | **host/machine name only** (the account is already written as a placeholder on the same line) | **Markdown prose**, inside an inline-code span. Safe for `<host>`. |
| 6 | `.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md` | 18 | account name, appearing as the literal argument of a quoted `grep` command | **Markdown prose**, inside an inline-code span. Safe for `<user>`. Note the sentence's *point* is the case-sensitivity contrast between two greps, so substituting both occurrences with the same placeholder destroys the sentence's meaning; it needs a rewrite, not a token swap. |
| 7 | `.claude/agent-memory/orchestrator/collect-pr-context-lands-in-main-checkout.md` | 10 | absolute user-profile path (an `artifacts\` path in the primary checkout), account name | **Markdown prose**, inside an inline-code span. Safe for `<repo-root>` / `<user-profile>`. |

**Six of the seven are Markdown prose; one (`TaskMaster.csproj:37`) is XML element text and one
(`.vscode/settings.json:27`) is a JSON string value. No angle-bracket placeholder would land in an
XML attribute value in this issue's scope.** That is the distinction the prompt asks about, and the
answer is favourable: the #662 corruption mode is not reachable from these seven sites. It *is*
reachable from any TRX the plan touches (R4.3), so the rule still has to be written down.

### R7.1 The five-file claim under-counts the repository

`SearchScope:` `.claude/agent-memory/`.
`SearchPatterns:` `(?i)danmoisan|megalodon|real good food|C:\\Users\\|c:/Users/`.
`SearchResult:` **32 occurrences across 23 files.** The five named in `issue.md:30` are all in that
set. The other eighteen files include, among others:

- `.claude/agent-memory/_shared_no_absolute_host_paths.md` (1)
- `.claude/agent-memory/task-researcher/MEMORY.md` (1)
- `.claude/agent-memory/parallel-planner/reference_parallel_artifact_authoring_gotchas.md` (1)
- `.claude/agent-memory/atomic-executor/project_bash_heredoc_collapses_doubled_backslashes.md` (2)
- `.claude/agent-memory/atomic-executor/project_msbuild_log_has_two_absolute_path_leak_classes.md` (4)
- `.claude/agent-memory/orchestrator/bash-tool-collapses-double-backslash-in-sed.md` (5)
- `.claude/agent-memory/feature-review/project_680-review-residuals.md` (2)
- `.claude/agent-memory/epic-orchestrator/feedback_region_ownership_is_a_prefix_claim.md` (1)
- `.claude/agent-memory/orchestrator/preparation-child-cwd-is-session-root-not-item-worktree.md` (1)
- `.claude/agent-memory/atomic-planner/worktree-root-breaks-dotclaude-exclusion.md` (1)

Most of these are *pedagogical* occurrences: files whose subject matter is the sweep itself, where
the literal token is the example being discussed (for example
`.claude/agent-memory/atomic-executor/project_selftest_probe_literal_trips_the_next_sweep_pass.md`,
whose title says exactly that). My pattern is broader than the issue's (it includes the
`C:\Users\` path form, not only the account/host tokens), which explains part of the gap. The
discrepancy is recorded so the plan does not assert "five files" as a complete population without
re-deriving it under its own stated pattern.

---

## R8 — `PublishUrl` semantics and consequences of emptying it

### What it is

`PublishUrl` is the ClickOnce / VSTO publish destination property. In a VSTO add-in project it is
consumed by the `Publish` MSBuild target family (`GenerateBootstrapper`, `_DeploymentUrl`) to
stamp the deployment location into the generated `.vsto` manifest and bootstrapper. It is a
**publish-time** property. It participates in no compile, analyze, nullable, or test target.

The surrounding lines confirm the ClickOnce context: `TaskMaster/TaskMaster.csproj:35-36`
(`<IsWebBootstrapper>False</IsWebBootstrapper>`, `<BootstrapperEnabled>true</BootstrapperEnabled>`),
line 38 `<InstallUrl />`, lines 40-44 (`ApplicationVersion`, `UpdateEnabled`, `UpdateInterval`),
and lines 52-54 (`BootstrapperPackage Include=".NETFramework,Version=v4.8.1"`).

### Does anything in this worktree read it?

**Nothing does.**

- `SearchScope:` whole worktree excluding `docs/**` and `**/*.trx`.
  `SearchPatterns:` `PublishUrl|InstallUrl|BootstrapperEnabled|IsWebBootstrapper`.
  `SearchResult:` seven lines, in exactly two files — `UtilitiesCS.Test/UtilitiesCS.Test.csproj:29`
  (`<PublishUrl>publish\</PublishUrl>`), `:41`, `:43`; and `TaskMaster/TaskMaster.csproj:35`, `:36`,
  `:37`, `:38`. No workflow, script, task, or target references it.
- `SearchScope:` whole worktree excluding `docs/**`.
  `SearchPatterns:` `(?i)/t:Publish|-Target[ =]+.?Publish|ClickOnce|PublishDir|GenerateBootstrapper`.
  `SearchResult:` **one** hit, and it is prose:
  `.claude/agent-memory/atomic-executor/project_msbuild_log_has_two_absolute_path_leak_classes.md:24-25`,
  which records `_DeploymentUrl` echoing "a OneDrive folder under the user profile (a ClickOnce
  publish URL carried by the VSTO project)" as a residual leak class in committed MSBuild logs.
  **No `Publish` target is invoked anywhere in this repository.**
- The four CI workflows that build (`_build-analyzers.yml`, `_build-nullable.yml`,
  `_format-check.yml`, `_mstest-coverage.yml`) invoke only `/t:Build` and `vstest.console.exe`.
  `Glob .github/workflows/*.yml` returns seven files; none names a publish step.
- `scripts/vscode/Invoke-VSBuild.ps1` is the only MSBuild driver under `scripts/`; the target is a
  parameter and the tasks in `.vscode/tasks.json:159-161` pass `Rebuild`.

### Consequences of an empty value

- **Build, analyzers, nullable, and tests:** none. No target in the invoked graph reads the
  property.
- **A local `Publish` run:** an empty `PublishUrl` on a `BootstrapperEnabled=true` VSTO project
  would leave the deployment URL unset. `UtilitiesCS.Test/UtilitiesCS.Test.csproj:29` establishes
  the in-repo precedent for the relative form `publish\`, which is the lower-risk replacement — it
  keeps the property well-formed and leaks nothing.
- **Secondary benefit:** removing the value also removes the `_DeploymentUrl` residual leak class
  from every future MSBuild diagnostic log (the fourth class named in
  `.claude/agent-memory/atomic-executor/project_msbuild_log_has_two_absolute_path_leak_classes.md:24-31`),
  which is otherwise unreachable by any repo-root path substitution because it sits outside every
  repository root.

---

## R9 — Push-down ownership

### The mechanism, as documented in this worktree

There is **no machine-readable manifest in this worktree** enumerating push-down-owned paths.

- `SearchScope:` whole worktree.
  `SearchPatterns:` `**/{push-down,pushdown,sync}*` (Glob).
  `SearchResult:` three unrelated files under `docs/features/archive/.../233/evidence/`.
- `SearchScope:` `.claude/skills/`, `.claude/agents/`.
  `SearchPatterns:` `(?i)absolute host|host path|account name|machine name|hygiene|redact|<repo-root>|<user-profile>`.
  `SearchResult: none`.

The authoritative in-repo statements are prose, in two places.

**1. `.claude/agent-memory/parallel-planner/reference_drm_copilot_upstream.md:8-20`** names the
upstream repository as the source for "TaskMaster's `.claude` governance surface" and enumerates
what is distributed:

> - `.claude/rules/`, `.claude/skills/`, `.claude/agents/`, `.claude/hooks/`, `.claude/lib/` — the
>   canonical copies that get distributed into consumer repos.
> - `extensions/drm-copilot/resources/claude-customizations/.claude/**` — the packaged copy shipped
>   by the VS Code extension.
> - `config/blast-radius.json` — the blast-radius truth table

`.claude/agent-memory/` is **not** in that list.

**2. `.claude/rules/parallel-orchestration.md:394-418`** describes the publication path
(`extensions/drm-copilot/resources/claude-customizations/config/blast-radius.json`) and states that
a destination's copy is overwritten from the bundle. Two library modules carry the same marker in
their headers: `.claude/lib/hook-payload/HookPayload.psm1:39` and
`.claude/lib/cleanup-manifest/CleanupWorktreeManifest.psm1:31` ("byte-identically under
`extensions/drm-copilot/resources/claude-customizations/`").

A third, corroborating statement:
`.claude/agent-memory/parallel-orchestrator/project_mandate_reads_omits_scripts_vscode.md:36-37`
— "Do NOT edit `config/blast-radius.json` here either — it is push-down-owned from drm-copilot and
is overwritten wholesale."

### Verdict per path

| Path | Exists in worktree | Push-down owned? | Basis |
|---|---|---|---|
| `.claude/agent-memory/**` | yes (23+ subdirectories) | **NO** | Absent from the distributed list at `reference_drm_copilot_upstream.md:13-14`. Contents are TaskMaster-specific (they cite TaskMaster issue numbers throughout) and accumulate across sessions, which is incompatible with wholesale overwrite. `issue.md:30` asserts the same. Corroborating, but not proof: `.claude/skills/cleanup-merged-worktrees/SKILL.md:156-157, 275` treats `.claude/agent-memory/**` as first-party content worth rescuing from abandoned branches. |
| `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` | yes | **YES** | `.claude/skills/` is named explicitly at `reference_drm_copilot_upstream.md:13`. |
| `.claude/skills/atomic-plan-contract/SKILL.md` | yes | **YES** | Same. |
| `.claude/settings.json` | yes | **YES** | Listed as a `shared_surfaces` entry at `config/blast-radius.json:4`, alongside the other two known push-down artifacts. `issue.md:63` states it directly and excludes it from this item. |
| `.claude/hooks/**` | yes (39 `.ps1` files) | **YES** | `.claude/hooks/` is named explicitly at `reference_drm_copilot_upstream.md:13`. |
| `config/orchestration-routing.json` | yes | **YES** | `config/blast-radius.json:6` lists it in `shared_surfaces` with `.claude/settings.json` and `config/blast-radius.json`; `.codex/config.toml:87` grants it `read`; `.codex/scripts/epic-child-launch-runtime.ps1:7` and `.codex/scripts/resume-epic-child.ps1:90` treat it as a runtime-published file alongside `.codex`, `.agents`, and `AGENTS.md`, all of which are independently established as push-down mirrors. |

**Caveat, stated plainly:** `shared_surfaces` in `config/blast-radius.json` is a *contention*
list, not an ownership declaration; the ownership inference for `.claude/settings.json` and
`config/orchestration-routing.json` rests on the prose plus the `.codex` runtime treatment, not on
a manifest. The `.claude/agent-memory/**` verdict rests on an absence from an enumeration plus the
content argument. Both would be settled definitively by inspecting the upstream bundle directory,
which this session's scope excludes.

### Consequence for the "write the convention down once" obligation

`issue.md:42` asks for the convention to live "in the evidence-and-timestamp-conventions skill or
its nearest TaskMaster-owned equivalent." Per the table, that skill file **is push-down owned**, so
an edit there is reverted on the next push-down and the convention is lost. The obligation cannot
be satisfied inside `.claude/skills/` at all.

I read `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` in full (176 lines). It
specifies evidence *locations*, ISO-8601 *naming*, discovery *order*, and a machine-checkable
*schema* (`Timestamp:` / `Command:` / `EXIT_CODE:` / optional `ExpectedExitCode:`, lines 106-124).
It says **nothing** about the permitted *format* of a coverage or test-result artifact. That is the
gap this issue fills.

**Nearest TaskMaster-owned homes, in order of fit:**

1. **`CLAUDE.md` (repo root).** It is TaskMaster-specific on its face — it pins CSharpier 1.2.6 via
   `dotnet-tools.json`, names `TaskMaster.sln`, and cites `.github/workflows/_format-check.yml`,
   `_build-analyzers.yml`, and `_build-nullable.yml` by filename. It already carries the four
   embedded policies and the C# toolchain order, including step 4
   (`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`, at lines 390 and 408) — the
   exact command the new `/ResultsDirectory:` and `/Logger:trx;LogFileName=` rule must attach to.
   **This is the best fit.** Contrast with `AGENTS.md`, which declares itself generated and is a
   push-down mirror (see `.claude/agent-memory/task-researcher/project_toolchain_gate_fidelity_512.md:11-21`).
2. **A new file under `docs/`.** `docs/` currently holds only `LCPPN_doc1.md`, `LCPPN_doc2.md`,
   `docs/research/` (4 files), `docs/features/`, and `docs/migration/`. There is **no**
   `docs/ci.research.md` despite `.claude/rules/quality-tiers.md` naming it as a tier source of
   truth, and no `quality-tiers.yml` at the repo root despite the same rule requiring it
   (`Glob {README.md,CONTRIBUTING.md,CLAUDE.md,AGENTS.md,quality-tiers.yml,change-plan.md,coverage.config}`
   returned no `quality-tiers.yml`). A new `docs/<name>.md` is unambiguously TaskMaster-owned but
   has no existing sibling convention and no existing reader.
3. **A doc comment in `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.** TaskMaster-owned and
   co-located with the code, but not discoverable by an agent that never opens the file.

`issue.md:42` also asks that the convention be "cited by the atomic-plan contract's evidence
tasks." `.claude/skills/atomic-plan-contract/SKILL.md` is push-down owned, so that citation cannot
be added here either; it is an upstream change.

---

## R10 — The redaction sweep

**There is no redaction sweep implemented as executable code anywhere in this repository.**

- `SearchScope:` whole worktree excluding `docs/**`, `.claude/agent-memory/**`, `**/*.trx`,
  `**/*.xml`, `actionlint-bin/**`.
  `SearchPatterns:` `(?i)sanitiz|sanitis|redact|scrub`.
  `SearchResult:` 40 hits, none of which is a sweep. They are: generic security advice in
  `AGENTS.md:2327`, `.github/instructions/typescript-code-change.instructions.md:168`,
  `.github/prompts/breakdown-feature-implementation.prompt.md:122`,
  `.github/agents/tdd-refactor.agent.md:38`; a one-line instruction in
  `.claude/skills/handoff/SKILL.md:14` and `.agents/skills/handoff/SKILL.md:14` ("Redact any
  sensitive information..." — no mechanism); unrelated identifier sanitisation inside
  `.claude/hooks/enforce-powershell-batch-budget.ps1:20, 118`; and the remainder are C# production
  and test symbols (`FolderConverter.SanitizeFilename`, `ArchiveRootPathGuard`'s "redacted
  diagnostic", `SortItemsToExistingFolder.SanitizeArrayLineTSV`).
- `SearchScope:` `.claude/skills/` and `.claude/agents/`.
  `SearchPatterns:` `(?i)absolute host|host path|account name|machine name|hygiene|redact|<user-profile>|<repo-root>`.
  `SearchResult: none` in both.
- `Glob .claude/hooks/*` returns 39 `.ps1` files; none is named for hygiene, redaction, or
  sanitisation, and the `sanitiz` grep above found no hook body performing one.

**Where the rule actually lives:** `.claude/agent-memory/_shared_no_absolute_host_paths.md`, a
103-line memory file shared across agents. It is a *rule document read by agents*, not a script.
Per R9 it is **not** push-down owned, so it is editable here.

**The exact plan-file exclusion mechanism the promoted record refers to.** It is described at
`.claude/agent-memory/_shared_no_absolute_host_paths.md:90-93`:

> Related structural gap seen on the same issue: a hygiene sweep that excludes `plan.md` from its
> residual scan by path cannot detect a host path reintroduced into the plan, and a sweep that
> rewrites only `evidence/` while scanning the whole feature folder detects residuals it cannot
> fix.

So the exclusion is **by path**, and it is **per-plan**: each atomic plan authors its own hygiene
task, and the one authored for item #662 scoped its residual scan to exclude the plan file. The
same file, lines 85-88, records the companion defect:

> `ResidualMatchCount=0` is not a sufficient hygiene gate. It measures only that the identifiers
> were removed, never that the file it rewrote still parses, so a sweep that corrupts every XML
> artifact it touches still reports success.

And `.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md:33-40` records
that the #662 defect was **codified in the approved plan** — "the artifact-hygiene rule in the
approved plan *mandated* the four angle-bracket placeholders ... so the executor produced six
unparseable TRX files by following the plan correctly."

**Consequence:** "the redaction sweep scans the plan file" (`issue.md:41`) cannot be delivered as
a code change to a sweep, because no sweep exists as code. It is delivered either by amending the
rule text in `_shared_no_absolute_host_paths.md`, or by creating a sweep as code for the first
time — a materially larger scope than the issue's other items.

---

## R11 — Consumers of committed `.cobertura.xml` / `.trx` under a feature `evidence/` tree

### The two named hooks: unaffected

`.claude/hooks/validate-feature-review-coverage.ps1` (SubagentStop hook for `feature-review`):

- `Get-JacocoBranchCoverage` — line 186. Sums `//counter[@type="BRANCH"]` `missed`/`covered`
  (lines 193-205). Returns `$null` when the file is absent or no BRANCH counter exists.
- `Get-JacocoRepoCoverage` — line 221. Sums `//counter[@type="LINE"]` (lines 228-241).
- The **only** paths it reads are fixed, and none is under `evidence/`:
  `Get-LanguageBranchCoverage` (lines 208-219) -> `coverage/lcov.info`,
  `artifacts/python/lcov.info`, `artifacts/pester/powershell-coverage.xml`,
  `artifacts/csharp/coverage.xml`.
  `Get-LanguageRepoCoverage` (lines 243-256) -> the same four.

`.codex/hooks/validate-feature-review-coverage.ps1`: the same four fixed paths, `RepoRoot`-joined,
at lines 144-145 (documented at lines 33-34).

- `SearchScope:` `.claude/` excluding `.claude/agent-memory/**`, and `.codex/`.
  `SearchPatterns:` `cobertura|\.trx`.
  `SearchResult: none` in both.

**Verdict: both hooks would continue to function unchanged.** Neither reads a feature `evidence/`
tree, and neither reads Cobertura or TRX at all. They read JaCoCo — which is precisely what the new
convention would produce, only at a different path.

### Full consumer enumeration

`SearchScope:` whole worktree excluding `docs/**`, `.claude/agent-memory/**`, `**/*.trx`,
`**/*.cobertura.xml`, `test-output.txt`, `actionlint-bin/**`.
`SearchPatterns:` `(?i)cobertura|\.trx\b|coverage\.xml`.
`SearchResult:` the complete set of non-`docs` references:

| Consumer | Reference | Reads feature `evidence/`? | Survives the change? |
|---|---|---|---|
| `.claude/hooks/validate-feature-review-coverage.ps1` | `artifacts/csharp/coverage.xml`, `artifacts/pester/powershell-coverage.xml` (lines 215-216, 252-253) | no | **yes** |
| `.codex/hooks/validate-feature-review-coverage.ps1` | same two, `RepoRoot`-joined (lines 144-145) | no | **yes** |
| VS Code Koverage extension | `.vscode/settings.json:20-25` — `coverageFileNames: ["coverage.cobertura.xml"]`, `coverageFilePaths: ["coverage"]` | no (reads `coverage/`, gitignored) | **yes** |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | produces and post-processes `coverage\coverage.cobertura.xml` (lines 9, 73, 256, 334-345) | no | it is the producer being changed |
| `.github/workflows/_mstest-coverage.yml:110` | uploads `TestResults/**/*.trx` as a CI artifact | no | **yes** |
| `.csharpierignore:4-8` | excludes `**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx` from the format gate | n/a | **yes** — `**/evidence/**` at line 4 already covers any new artifact placed there, whatever its extension |
| `.gitignore:143-145` | `coverage/*` with a `.gitkeep` exception | no | **yes** |

**No hook, script, or workflow in this repository reads a committed `.cobertura.xml` or `.trx`
under a feature `evidence/` tree.** The only consumer of those files is a **human or agent
reviewer**.

### The real constraint: the feature-review reviewer, not a hook

`.claude/skills/feature-review-workflow/SKILL.md` is push-down owned (R9) and imposes obligations
a package-level projection cannot satisfy from committed evidence alone:

- Line 112: "New code files (added in this feature): line coverage >= 85%, and branch coverage
  >= 75% for branch-capable languages. Flag as FAIL otherwise."
- Line 113: "Modified files (changed but previously existing): line coverage >= 85%, branch
  coverage >= 75% ..., **and no regression on changed lines relative to baseline**."
- Line 115: "If coverage artifacts already exist from the executor run, **inspect them instead of
  re-running**."

A package-level projection carries **no per-file and no per-line detail**. The #646 record
acknowledges exactly this at
`coverage-artifact-substitution.2026-09-01T16-41.md:131-139`: "The one class of detail the
projection does not retain is per-line and per-class granularity ... The conversion is lossless
with respect to the LINE counters and lossy only with respect to per-class and per-line detail."

Two mitigations are already established in the tree and should be noted rather than invented:

- The #646 record preserves the per-line `hits` values it needed **verbatim in the prose of the
  dependent artifact** (`coverage-delta-verification.2026-08-31T20-04.md`), and records the
  sequencing proof that the gates ran against the raw reports **before** deletion
  (`coverage-artifact-substitution.2026-09-01T16-41.md:104-127`).
- `.claude/agent-memory/feature-review/project_614-review-residuals.md:70` records the reviewer's
  workaround when only a package-level artifact is committed: per-file figures come from the
  **gitignored** `coverage/coverage.cobertura.filtered.*.xml` pair, deduplicating `<class>` nodes
  by `filename` with max hits per line.

So: nothing breaks mechanically, but the per-file acceptance criteria become verifiable only
while the raw document still exists on disk. The deletion **must** be sequenced after both the
threshold assertion *and* every per-file figure has been recorded — which is exactly what
`issue.md:35` specifies and what the #646 precedent did.

---

## R12 — Sibling contention

### `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` — current contents in full

**56 content lines.** Headroom to the 500-line ceiling: **444 lines.**

- Line 1: `Set-StrictMode -Version Latest`
- Line 3: `function Assert-CoberturaLineCoverageThreshold {`
- Lines 4-24: the comment-based help. Lines 14-17 record why the function lives in its own file:
  "`Invoke-MSTestWithCoverage.Helpers.ps1` ... reached the repository's 500-line ceiling once issue
  #733's fixes landed. Helpers.ps1 dot-sources this file, so a caller that dot-sources Helpers.ps1
  alone still resolves this function."
- Lines 25-29: `[CmdletBinding()]` and a single mandatory `[string]$CoberturaXml` parameter.
- Lines 31-33: loads the document, selects `/coverage`, reads `line-rate` via `GetAttribute`.
- Lines 34-36: throws `'Cobertura line-rate is missing.'` on null/whitespace.
- Lines 38-45: `[decimal]::TryParse` with `NumberStyles::Float` and `InvariantCulture`; throws
  `'Cobertura line-rate must be numeric.'`.
- Lines 47-49: throws `'Cobertura line-rate must be between 0 and 1.'` outside `[0,1]`.
- Lines 51-55: multiplies by 100 and throws
  `"Cobertura line coverage $formattedPercentage% is below the required 80% threshold."` below 80.

It contains exactly one function and no branch-coverage logic. The sibling item adding a
**branch**-coverage assertion will either add a second function to this file or change this one.
The file is the only function-bearing part file with ample headroom, so it is the natural target —
and therefore the file this issue's plan should **not** touch.

Its test file, `tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1`, is 16 lines
(five one-line `It` blocks at lines 10-14). The sibling will grow that file too.

### Existing test filenames under `tests/scripts/vscode/`

`Glob tests/scripts/**/*.ps1` returns exactly twelve files, all in `tests/scripts/vscode/`:

1. `Install-RepoDotNetSdk.Tests.ps1`
2. `Invoke-MSTest.AssemblyDiscovery.Tests.ps1`
3. `Invoke-MSTest.Main.Tests.ps1`
4. `Invoke-MSTest.RunSettings.Tests.ps1`
5. `Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`
6. `Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`
7. `Invoke-MSTestWithCoverage.FirstParty.Tests.ps1`
8. `Invoke-MSTestWithCoverage.Helpers.Tests.ps1`
9. `Invoke-MSTestWithCoverage.Merge.Tests.ps1`
10. `Invoke-MSTestWithCoverage.PackageRate.Tests.ps1`
11. `Invoke-MSTestWithCoverage.Threshold.Tests.ps1`
12. `Invoke-VSBuild.Tests.ps1`

The naming convention is `<ProductionFileStem>.<Aspect>.Tests.ps1`, with `<Aspect>` optional. Note
that `Invoke-MSTest.RunSettings.Tests.ps1` is **misnamed relative to its contents**: it holds
`Describe` blocks for `Get-VsTestArgumentList` and `Invoke-VsTestExe` from `Invoke-MSTest.ps1`
(lines 44, 75) *and* for `Get-DotnetCoverageArgumentList` and `Invoke-DotnetCoverageCollection`
from `Invoke-MSTestWithCoverage.ps1` (lines 100-139, 153-320). Any new test asserting
`/ResultsDirectory:` or `/Logger:` on either argument builder would naturally belong there, which
is a contention risk if the sibling also edits it. A distinct new filename avoids it.

Unused stems that collide with nothing, available for this issue's tests (illustrative, not a
recommendation): `Invoke-MSTestWithCoverage.Projection.Tests.ps1`,
`Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1`,
`Invoke-MSTest.TrxSummary.Tests.ps1`, `Invoke-MSTest.ResultsDirectory.Tests.ps1`.

### The other sibling — repository-wide host-identifier sweep

That item's blast radius by definition includes the seven files in R7 and the eighteen additional
`.claude/agent-memory/` files in R7.1. Direct collision risk on all seven R7 targets. Note
`issue.md:64-66`, which already sequences the historical sweep (#602) **after** this item so a
fresh test run does not reintroduce the prefix.

### One further contention fact, not asked but load-bearing

`config/blast-radius.json`'s `mandate_reads` list does **not** include `scripts/vscode/**`
(`.claude/agent-memory/parallel-orchestrator/project_mandate_reads_omits_scripts_vscode.md:14-16`,
which enumerates the list as covering `.claude/rules/**`, four named SKILL.md files,
`.github/instructions/**`, `artifacts/**`, `quality-tiers.yml`, `.claude/agent-memory/**`, and
`.agents/skills/**`). Because this issue **writes** files under `scripts/vscode/`, rather than
merely citing them, its overlap with any concurrent C# item on those paths is a real conflict edge,
not the spurious citation edge that memory describes. I read the first 20 lines of
`config/blast-radius.json` directly and confirmed `mandate_reads` begins at line 12 with
`.claude/rules/**`, `.claude/skills/atomic-plan-contract/SKILL.md`,
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, `.github/instructions/**`,
`artifacts/**`, `quality-tiers.yml`, and continues past line 20; I did not read the full array, so
the `.claude/agent-memory/**` membership is quoted from the memory file, not directly verified.

---

## Cross-cutting facts the plan will need

1. **No new production `.cs` file is involved.** The work is PowerShell under `scripts/vscode/`,
   Pester under `tests/scripts/vscode/`, one `.csproj` line, one `.json` line, and Markdown.
2. **`Helpers.ps1` cannot host new code** (29 lines of headroom). The repository's established
   answer is a new part file dot-sourced from `Helpers.ps1:2-5`; four precedents exist, and two of
   them document the reason in their own headers.
3. **The de-duplication rule must not be re-derived.** `Get-CoberturaClassLineSummary`
   (`Helpers.ps1:159`) is the single implementation, and
   `Invoke-MSTestWithCoverage.FirstParty.ps1:13-17` states the invariant explicitly: "exactly one
   implementation of the counting rule exists and every caller applies that same one (issue #815)."
4. **Temporary files are prohibited in tests** by `.claude/rules/general-unit-test.md` and
   `CLAUDE.md` UT4 ("Currently approved exceptions: none"). The R6 here-string pattern is the only
   compliant fixture mechanism in the tree.
5. **`.csharpierignore:4` already excludes `**/evidence/**`**, so a new artifact format placed
   under a feature `evidence/` tree needs no formatter change.
6. **The TRX default namespace** (`http://microsoft.com/schemas/VisualStudio/TeamTest/2010`) makes
   unprefixed XPath return nothing; any summary reader needs a namespace manager or `local-name()`.
7. **`Counters` has no `skipped` attribute** (R4.4). The issue's four-field summary needs a stated
   mapping for "skipped".

---

## Test strategy implications (no test code written)

Consistent with `CLAUDE.md`, `.claude/rules/general-unit-test.md`, and the R6 pattern:

- **Projection writer.** Pure function over an in-memory `[xml]` Cobertura document, exercised with
  here-string fixtures. Positive: multi-package document produces one `package` element per
  retained package with correct `missed`/`covered`. Negative: missing `//packages` node throws the
  existing wording `'Cobertura XML does not contain a <packages> node.'` (reuse, do not invent a
  second wording — the precedent is `FirstParty.ps1:32-33` and its test at
  `Invoke-MSTestWithCoverage.FirstParty.Tests.ps1:216-226`). Boundary: a package with no classes
  yields `missed="0" covered="0"`. Boundary: a document with zero branch data yields zero BRANCH
  counters rather than omitted ones (the #646 uniformity decision). Discriminating: assert the
  exact serialised string, since the acceptance is exact-shape equality.
- **Reconciliation assertion.** A separate pure function asserting that summed package LINE
  counters equal the source root `lines-covered`/`lines-valid`, with a negative test proving it
  throws on a mismatch. This is the mandatory gate from R1.5 and is currently unimplemented.
- **Summary writer (TRX).** Pure function over an in-memory TRX string. Positive: passed run
  yields the four counts and an empty failed-name list. Negative: failed run yields the failed
  test names from `UnitTestResult[@outcome='Failed']/@testName`. Edge: a TRX with zero
  `UnitTestResult` elements. Edge: namespace handling — a test that would pass with an unprefixed
  XPath only if the namespace were absent is a false green.
- **Argument construction.** Follow `Invoke-MSTest.RunSettings.Tests.ps1` exactly: call the pure
  builder directly and assert array membership and ordering (`Should -Contain`, and
  `[array]::IndexOf` comparisons as at lines 133-136). For the end-to-end path, mock only the named
  wrapper seam (`Invoke-VsTestExe`, `Invoke-DotnetCoverageExe`) with a `param()` block matching
  production, and capture the argument array into a `$script:` variable.
- **Raw-file deletion.** Test the deletion decision, not the filesystem call: extract the ordering
  (threshold -> projection -> reconciliation -> delete) behind a seam and assert the call order, or
  assert that the deletion function refuses when reconciliation has not passed. No temporary file
  may be created.
- **Regression coverage for the identifier fixes.** A test asserting the absence of a bare account
  or host token is an absence gate and is necessary-not-sufficient
  (`_shared_no_absolute_host_paths.md:85-88`). Pair it with a parse assertion for any XML-family
  file touched.
- **Do not add tests to `Invoke-MSTestWithCoverage.Threshold.Tests.ps1`** — sibling contention
  (R12).

---

## Open questions the plan must decide (not decided here)

1. Which projection shape variant is canonical: all-packages (#646) or first-party-only (#503).
   They differ in denominator and therefore in every figure.
2. Whether the projection is written from the **post-processed** document (reconciles to the root
   by construction) or the **raw** collector output (does not — see the R2 caveat).
3. The mapping for "skipped" in the TRX summary, given `Counters` has no such attribute.
4. Whether "the redaction sweep scans the plan file" is delivered as rule text or as new code
   (R10) — the two have very different scopes.
5. Where the convention document lives, given `.claude/skills/` is push-down owned (R9).

---

## Numeric Derivation Evidence

This section derives the one numeric population the acceptance criteria depend on: the complete set of
argument-builder functions reachable from the repository's editor task scripts that construct a
command line for the Visual Studio test console, and which therefore must each carry an explicit
results directory and an explicit log file name. The count is derived twice by two independent
methods and the two member sets are compared.

- Complete Family: Get-VsTestArgumentList, Get-DotnetCoverageArgumentList
- Exhaustive Search Scope: The entire repository working tree was searched, covering every tracked and untracked file with no directory excluded.
- Inclusion Rules: A member qualifies when it is a PowerShell function whose return value is the argument array handed to the Visual Studio test console executable, either directly or through a coverage collector that receives the console path after an argument separator.
- Exclusion Rules: A definition is rejected when it only resolves an executable path, only terminates processes, or builds a command line for a continuous-integration workflow file rather than for an editor task script.
- Primary Search Strategy or Query Expression: Case-insensitive content sweep over every file in the working tree for the test console executable token and for the Visual Studio test switches, then a second sweep for each of Get-VsTestArgumentList and Get-DotnetCoverageArgumentList by name, keeping every hit that returns an argument array.
- Primary Member Set: Get-VsTestArgumentList, Get-DotnetCoverageArgumentList
- Primary Count: 2
- Cross-check Search Strategy or Query Expression: Structural enumeration by glob of every PowerShell file beneath the repository script directories, each candidate read from beginning to end, recording each function that returns an argument array destined for the test console, which independently produced Get-DotnetCoverageArgumentList and Get-VsTestArgumentList without relying on any textual token match.
- Cross-check Member Set: Get-DotnetCoverageArgumentList, Get-VsTestArgumentList
- Cross-check Count: 2
- Member-set Comparison: The primary and cross-check member sets are identical ignoring order and case, and both counts are equal at 2, so the family is complete.

Supporting citations for both derivations are in R3.1 above: `scripts/vscode/Invoke-MSTest.ps1:37`
declares the first member and `scripts/vscode/Invoke-MSTestWithCoverage.ps1:41` declares the second.
R3.1 also records the rejected candidates, namely `scripts/vscode/TestProcessCleanup.ps1`, which never
launches the console, and `.github/workflows/_mstest-coverage.yml:98-99`, which is a workflow rather
than an editor task script and is outside this issue's stated scope.
