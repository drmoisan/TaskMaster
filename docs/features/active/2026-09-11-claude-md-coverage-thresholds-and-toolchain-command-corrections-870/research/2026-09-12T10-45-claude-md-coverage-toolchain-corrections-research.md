# Research: CLAUDE.md coverage thresholds and toolchain command corrections (Issue #870)

- Date: 2026-09-12
- Feature folder: `docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870`
- Scope: documentation-only correction to the repository-root `CLAUDE.md`. No source, config, or governance rules file is a write target.

## 1. The actual coverage route

### 1.1 Invocation shape

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` builds an **outer `dotnet-coverage collect`** call whose trailing arguments (after a `--` separator) are an **inner `vstest.console.exe`** invocation:

- `Get-DotnetCoverageArgumentList` (`Invoke-MSTestWithCoverage.ps1:41-77`) returns:
  ```
  collect --output <OutputPath> --output-format cobertura --settings <CoverageConfig>
  -- <VsTestPath> <TestAssembly...> /Settings:<RunSettingsPath> /InIsolation
  /TestCaseFilter:TestCategory!=LiveOutlook
  ```
  (verbatim array construction at `Invoke-MSTestWithCoverage.ps1:70-77`.)
- `Invoke-DotnetCoverageCollection` (`Invoke-MSTestWithCoverage.ps1:172-243`) is the caller: it derives a per-output-adjacent effective coverage-settings file (lines 198-223), then calls `Get-DotnetCoverageArgumentList` and splats the result into `Invoke-DotnetCoverageExe` (lines 225-233), which itself runs `& dotnet-coverage @DotnetCoverageArgs` (line 153).
- `Invoke-MSTestWithCoverageMain` (`Invoke-MSTestWithCoverage.ps1:248-347`) is the top-level entry point: it resolves `vstest.console.exe` via `vswhere` (lines 279-290), requires `dotnet-coverage` on PATH (lines 292-294), discovers `*.Test.dll` assemblies under the configured `bin\<Configuration>\` tree while excluding `obj`, `ref`, and any `.claude\` path (lines 296-307), and then calls `Invoke-DotnetCoverageCollection` (lines 327-332) with `coverage.config` as the outer settings file (line 321) and `TaskMaster.cli.runsettings` as the inner `/Settings:` file (resolved by `Resolve-RunSettingsPath`, lines 15-39, called at line 278).

### 1.2 Why the inner vstest never gets the built-in coverage collector switch

The inner runsettings file, `scripts/vscode/TaskMaster.cli.runsettings` (full contents, lines 1-9), carries only an `<MSTest><Parallelize>` section — no `<DataCollectionRunSettings>`/`<DataCollector friendlyName="Code Coverage">` block. This is explained in the docstring of `Resolve-RunSettingsPath` (`Invoke-MSTestWithCoverage.ps1:15-27`): "It carries the MSTest parallelization only and no coverage data collector, so the inner vstest invocation never activates the Code Coverage collector; instrumentation comes solely from the outer `dotnet-coverage --settings coverage.config` path." A repeated inline comment at lines 68-69 reiterates: "The outer dotnet-coverage `--settings` is the effective instrumentation-exclude file; the inner vstest `/Settings:` is the MSTest runsettings." Running `vstest.console.exe ... /EnableCodeCoverage` (the CLAUDE.md-documented command) would invoke the separate, built-in "Code Coverage" data collector that the repo-root `TaskMaster.runsettings` configures (see 1.3) — a second, independent instrumentation path from the one `dotnet-coverage collect` performs. The two are not designed to run together in the CLI script; the actual script never passes `/EnableCodeCoverage` to the inner vstest call at all.

By contrast, the repo-root `TaskMaster.runsettings` (full contents, lines 1-30) *does* carry a `<DataCollectionRunSettings><DataCollectors><DataCollector friendlyName="Code Coverage">` block with the same `ModulePaths/Exclude` list as `coverage.config`. Its own docstring purpose is stated in the CLI script's comment (line 26): "Visual Studio continues to auto-detect the separate repo-root `TaskMaster.runsettings` (which carries the coverage exclusions)." That is, the repo-root file is the one Visual Studio's IDE-integrated Test Explorer / Live Unit Testing auto-discovers and uses with its own `/EnableCodeCoverage`-equivalent UI action; the CLI script deliberately uses a different, coverage-collector-free runsettings file (`TaskMaster.cli.runsettings`) because instrumentation is handled by the outer `dotnet-coverage collect` process instead.

### 1.3 Where instrumentation exclusions come from

Both `TaskMaster.runsettings` (repo root, lines 14-23) and `coverage.config` (repo root, lines 12-22) list an identical `ModulePaths/Exclude` set of seven regex patterns: `.*Deedle.*`, `.*FSharp.*`, `.*Castle\.Core.*`, `.*FluentAssertions.*`, `.*Moq.*`, `.*Microsoft\.Testing.*`, `.*MSTest.*`. `coverage.config`'s header comment (lines 2-9) states its purpose: "Excludes third-party and F#/mixed-mode assemblies from instrumentation to prevent coverage from breaking tests that depend on those libraries (e.g. Deedle, FSharp.Core). Third-party packages that still get instrumented are stripped from the Cobertura output during post-processing in Invoke-MSTestWithCoverage.ps1." For the CLI route, this `coverage.config` file is read in-memory by `ConvertTo-DerivedCoverageSettingsXml` (`Invoke-MSTestWithCoverage.ps1:79-116`), which adds exactly one further exclusion — a `.*\.Test\.dll$` pattern for the test assemblies themselves (lines 99-113) — before writing a derived, output-adjacent settings file that is what actually gets passed to `dotnet-coverage collect --settings` (`Invoke-DotnetCoverageCollection`, lines 198-230). The derived file is removed in a `finally` block after the run (lines 238-241). So exclusions originate in `coverage.config` (repo root) and are supplemented, not replaced, at runtime with a test-assembly exclusion.

### 1.4 Default coverage output path and format

The script's default `-CoverageOutput` parameter is `coverage\coverage.cobertura.xml` (`Invoke-MSTestWithCoverage.ps1:9` and restated at line 256), and `Get-DotnetCoverageArgumentList` passes `--output-format cobertura` (line 73). After the primary `dotnet-coverage collect` run, the script does further in-place post-processing of that same Cobertura XML file (`Invoke-MSTestWithCoverageMain`, lines 334-346): rewriting absolute paths to repo-relative paths, injecting a `<sources><source>.</source></sources>` element required by cobertura-parse/Koverage, and stripping `<package>` elements for third-party assemblies dotnet-coverage instrumented but that are not part of the solution (via `ConvertTo-KoverageCoberturaXml`, `Invoke-MSTestWithCoverage.Helpers.ps1:406-470`).

### 1.5 Thresholds enforced today, and branch-figure status

`scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` defines `Assert-CoberturaLineCoverageThreshold` (lines 3-56). It reads only the `/coverage` element's `line-rate` attribute (line 33), validates it is present, numeric, and in `[0,1]` (lines 34-49), and throws if the resulting percentage is below **80** (lines 52-55; the literal `80` appears at line 52 and again in the docstring at line 6: "at or above 80 percent"). This function is invoked once, at `Invoke-MSTestWithCoverage.ps1:344`, immediately after the Koverage post-processing step.

**No branch-coverage threshold is asserted anywhere in this script family.** `Assert-CoberturaLineCoverageThreshold` reads and checks `line-rate` only; it never reads `branch-rate`. However, `branch-rate` *is* computed and written into the coverage document: `Get-CoberturaCoverageSummary` (`Invoke-MSTestWithCoverage.Helpers.ps1:102-136`) accumulates `BranchesValid`/`BranchesCovered` per package and returns a `BranchRate`, and `ConvertTo-KoverageCoberturaXml` sets `$xml.coverage.SetAttribute('branch-rate', $coverageSummary.BranchRate)` at line 456. So the artifact carries a real, computed document-level branch rate — it is simply never gated by this script today.

### 1.6 The wrapping VS Code task

`.vscode/tasks.json` contains a task with label **`test: MSTest with Coverage (Koverage)`** (`.vscode/tasks.json:193`), whose `command` is `pwsh` and whose `args` invoke `scripts/vscode/Invoke-MSTestWithCoverage.ps1` with `-SearchRoot .` and `-Configuration Debug` (lines 194-206). It has a `dependsOn: ["build: TaskMaster.sln (VS MSBuild)"]` (lines 207-209). This is the task name a corrected CLAUDE.md could cite as the developer-facing entry point to the actual coverage route.

## 2. The analyzer-severity source

- `Glob` for `**/.globalconfig` across the entire repository returned **no files found**. No `.globalconfig` exists anywhere in this repository.
- `.editorconfig` exists at the repository root (677 lines) and carries extensive analyzer-severity configuration under a `[*.cs]` section, for example: the global catch-all default at line 27 (`dotnet_analyzer_diagnostic.severity = suggestion`), the single meaningful non-suggestion override at line 29 (`dotnet_diagnostic.MSTEST0032.severity = warning`), and a long enumerated list of per-rule severities for Meziantou.Analyzer (MA0001-MA0202, lines 32-231), SonarAnalyzer (S-prefixed IDs, lines 237-290), Roslynator (RCS/ROS-prefixed IDs, lines 293-535), AsyncFixer (lines 538-543), and BannedApiAnalyzers (RS0030/RS0031/RS0035, lines 555-557). This confirms `.editorconfig`, not any `.globalconfig`, is the actual and sole analyzer-severity configuration source in this repository.
- CLAUDE.md currently cites `.globalconfig` as an analyzer-severity source at exactly two locations, both verified by direct `Read` of CLAUDE.md and cross-checked with `Grep`:
  - **CLAUDE.md:197** (section C#1, item 2): `- C# code must pass Roslyn/.NET analyzer diagnostics configured by \`.editorconfig\`, \`.globalconfig\`, and project properties.`
  - **CLAUDE.md:273** (section C#7): `- Prefer built-in .NET SDK analyzers and configuration through \`.editorconfig\` / \`.globalconfig\`.`

## 3. The CLAUDE.md edit sites

All line numbers below were obtained by directly reading the full repository-root `CLAUDE.md` (448 lines) and cross-checked with a `Grep` pass; both agree.

### 3.1 CUT3 C# Toolchain Command Selection, step 4

- **CLAUDE.md:390**: `4. \`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage\``
- Must change to name the actual route: the wrapping VS Code task `test: MSTest with Coverage (Koverage)` and/or the script `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (outer `dotnet-coverage collect` wrapping the inner vstest call), with a note that `/EnableCodeCoverage` is deliberately never passed to the inner vstest invocation because instrumentation is performed by the outer `dotnet-coverage collect` process instead (see section 1.2 above for the exact mechanism to summarize).

### 3.2 "C# Toolchain (run in this exact order)" step 4

- **CLAUDE.md:408**: `4. **Test**: \`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage\``
- Same correction as 3.1: this is a second, near-identical statement of the same wrong command later in the file and must be corrected identically for consistency between the two toolchain restatements.

### 3.3 UT2 Coverage and Scenarios block — every line stating a coverage percentage

- **CLAUDE.md:303**: `  - Repository-wide line coverage must remain \`>= 80%\`.`
  - Corrected text must retain the 80% C# line floor (the settled figure is unchanged at 80%) but the surrounding block must also state explicitly, per the maintainer's 2026-09-11 decision: C# branch coverage floor 75%, and PowerShell line coverage floor 80% with Pester measuring no branch coverage. Today's text states only a single undifferentiated "repository-wide line coverage" figure and no branch figure at all — the correction must add the missing branch and PowerShell-specific figures, not merely restate an unchanged number.
- **CLAUDE.md:304**: `  - **COM/VSTO/WinForms coverage exemption (testable denominator).** The 80% floor applies to the **testable denominator** — production-only first-party code, after excluding:`
  - The three enumerated exemption classes that follow (lines 305-307) are unchanged per the maintainer decision and must be preserved verbatim: (a) VSTO add-in lifecycle classes, (b) WinForms form-derived classes and Designer-generated code, (c) Outlook Interop event handler classes in the five named namespaces without an injectable seam.
- **CLAUDE.md:309**: `    These classes are formally exempted from the 80% floor. Exemption is applied via \`[ExcludeFromCodeCoverage]\` attributes in source code (reviewable in PRs) or via \`coverage.config\` assembly-level excludes for near-wholly-untestable assemblies. **Authority**: This exemption must be ratified by the project maintainer and is tracked in \`feature/csharp-coverage-uplift\`. Testable seams within otherwise-COM-bound assemblies (e.g., \`ToDoLoader\`, \`IDList\` arithmetic, \`KbdActions<>\`, path/settings helpers) are explicitly NOT exempt and must meet the \`>= 80%\` floor.`
  - The corrected text must add one sentence recording that the coverage figures were settled on 2026-09-11 under issue #563, and that the corresponding figures in the governance rules tree (85%/75%) are upstream-owned (push-down governed) and not authoritative for this repository's actual gates. The "must meet the >= 80% floor" language at the end of this line stays consistent with the unchanged 80% C# line figure.
- **CLAUDE.md:310**: `  - Any new modules, classes, or methods added must target \`>= 90%\` coverage.`
  - Unchanged: the settled "new code" figure is 90%, matching what CLAUDE.md already states. No edit required at this specific line beyond ensuring it remains consistent with the newly added branch/PowerShell figures nearby.

### 3.4 The two analyzer-severity citations

- **CLAUDE.md:197** (section C#1, item 2) and **CLAUDE.md:273** (section C#7): both currently read `.editorconfig`, `.globalconfig` (or `.editorconfig` / `.globalconfig`). Both occurrences of `.globalconfig` must be removed since no such file exists in this repository (section 2 above); each line should read `.editorconfig` alone as the analyzer-severity source, with the surrounding sentence structure otherwise preserved (e.g., line 197 keeps "and project properties"; line 273 keeps the "built-in .NET SDK analyzers and configuration through" framing).

## 4. The settled figures

The feature folder's own `issue.md` (lines 16, 28, 34, 70) and `spec.md` (lines 11, 29, 34) already record the settled figures and their provenance in the repository's own tracked documentation for this issue:

- C# line coverage floor: 80% (unchanged from current CLAUDE.md text).
- C# branch coverage floor: 75% (currently absent from CLAUDE.md; must be added).
- PowerShell line coverage floor: 80%, with Pester measuring no branch coverage (currently absent from CLAUDE.md; must be added, along with the no-branch-measurement caveat).
- New code target: 90% (unchanged from current CLAUDE.md text).
- The three UT2 exemption classes (VSTO lifecycle, WinForms/Designer, Outlook Interop event handlers without an injectable seam) are unchanged and must be preserved verbatim.
- `issue.md` line 70 states: "Closes #828 and #563 on merge," and line 16 attributes the three findings to issues #828 (toolchain command), #563 (coverage threshold divergence, decision recorded 2026-09-11), and #727 sub-finding 5 (the `.globalconfig` citation).

**Corroboration path used**: this agent has no Bash tool available in its tool set (only Read, Grep, Glob, WebFetch, Write, Edit are provided), so `gh issue view` could not be invoked to independently fetch issues #563, #828, or #727 from GitHub. The figures above were corroborated instead by reading this feature's own tracked `issue.md` and `spec.md` files, which already record the maintainer's 2026-09-11 decision text and issue cross-references, and by reading the currently-checked-out `.claude/rules/general-unit-test.md`-derived project instructions supplied in this session's system context, which independently state the 85%/75% figures the maintainer decision is diverging from. No live GitHub API corroboration was performed; the settled figures are treated as given per the task's explicit fallback instruction.

## 5. The enforcement half (tracked upstream, out of scope)

The numeric coverage gates actually enforced today by the repository's rules tree state a stricter pair of figures than the ones CLAUDE.md will state after this fix: eighty five percent line coverage and seventy five percent branch coverage, applied uniformly across every module rigor tier. This wording appears in the general unit test rule file, the module rigor tiers rule file, and the C# rule file that live under the dot-claude rules directory, and the same eighty five and seventy five figures are also checked by the feature review coverage validation hook that runs during feature review. All of these files are published into this repository by an automated push-down process from an upstream governance repository, and that push-down applies zero templating, so any local edit made to one of them here would simply be overwritten the next time the push-down runs. For that reason, none of those files may be edited as part of issue eight seven zero, and this research explicitly does not propose editing any of them. The gap between the eighty and seventy five figures CLAUDE.md will state and the eighty five and seventy five figures the rules tree and the review hook actually enforce is a known, tracked divergence that belongs to the upstream governance repository to resolve, not to this repository's local documentation fix.

## 6. Risk notes for the downstream plan

- The only write target for this fix is the repository-root `CLAUDE.md`. No other file — in particular, none of the governance rules files named in section 5 — is in scope, and none should appear in an implementation plan's write set.
- Because the change is documentation-only, no build, format, analyzer, or test toolchain command is required to validate it. The General Code Change Policy's mandatory toolchain loop (formatting, linting, type-checking, testing) has no applicable target here: there is no C# source or config file being changed, so `csharpier`, `msbuild`, and `vstest.console.exe` invocations are not warranted by this change.
- The Bash tool in this agent's environment refuses PowerShell (`pwsh`) inside the isolation sandbox, and this research agent additionally has no Bash tool at all in its granted tool set. Any verification step a downstream plan proposes (e.g., re-confirming line numbers after the edit, or confirming zero remaining `.globalconfig`/`/EnableCodeCoverage` occurrences in CLAUDE.md) must be expressible purely with the Grep and Read tools, or with `git` (e.g., `git diff`), and must not depend on running any PowerShell script such as `Invoke-MSTestWithCoverage.ps1` or `Invoke-MSTestWithCoverage.Threshold.ps1` to validate the documentation change itself.

## Numeric Derivation Evidence

- Complete Family: /EnableCodeCoverage, .globalconfig
- Exhaustive Search Scope: The entire repository working tree, every tracked and untracked path under the worktree root.
- Inclusion Rules: An occurrence counts only when it is a literal textual match located inside the repository-root CLAUDE.md, because that file is the sole write target of issue 870.
- Exclusion Rules: Occurrences located in any other file are excluded from the member set, including the governance rules tree, the GitHub instruction files, workflow files, agent memory files, and every other feature folder.
- Primary Search Strategy or Query Expression: Grep tool over the whole worktree root with output_mode content and the regular expression /EnableCodeCoverage|\.globalconfig , then restrict the resulting hit list to the repository-root CLAUDE.md and record one member per matching line number.
- Primary Member Set: CLAUDE.md:197, CLAUDE.md:273, CLAUDE.md:390, CLAUDE.md:408
- Primary Count: 4
- Cross-check Search Strategy or Query Expression: An independent second pass using the Read tool to load the repository-root CLAUDE.md in full and walking every line looking for the literal tokens /EnableCodeCoverage and .globalconfig without any regular expression, recording one member per matching line number.
- Cross-check Member Set: CLAUDE.md:390, CLAUDE.md:408, CLAUDE.md:197, CLAUDE.md:273
- Cross-check Count: 4
- Member-set Comparison: The primary and cross-check member sets are equal when order is ignored; both enumerate the same four lines.

## Rejected alternatives

No competing implementation approaches were evaluated for this research: the fix is a documentation-only text correction with no design alternatives (there is exactly one file to edit and the corrected text is fully determined by the verified facts above). This section is intentionally brief because the "candidate approaches" workflow step does not apply to a single-file prose correction with no architectural choice to make.
