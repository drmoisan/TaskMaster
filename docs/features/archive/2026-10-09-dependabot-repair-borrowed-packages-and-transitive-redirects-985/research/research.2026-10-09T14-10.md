# Research: Dependabot repair for borrowed packages and transitive binding redirects (Issue #985)

- Date: 2026-10-09T14-10
- Branch: `bug/dependabot-repair-borrowed-packages-and-transitive-redirects-985` (worktree HEAD = main `9911fe138`)
- Inputs read: `issue.md`, `spec.md` (Draft), `plan.2026-10-09T13-06.md` (template only)
- Evidence markers: `[V]` verified by reading or searching tracked files in this worktree; `[V-web]` verified from the public GitHub web UI (no `gh` or Bash was available to this agent); `[U]` not verified, stated as a risk.

## 1. Summary of the recommendation

1. **Borrowed hint paths:** declare each borrowed package in the test project's own `packages.config`. On main there are exactly four borrowed (project, package) pairs: QuickFiler.Test with WebView2 and with ObjectListView, UtilitiesCS.Test with WebView2, and TaskTree.Test with ObjectListView. Also delete the duplicate `Microsoft.Web.WebView2.Core` `<Reference>` in `QuickFiler.Test.csproj`. No other csproj edit is required.
2. **Transitive binding redirects:** add a solution-wide redirect-sync pass in a new module, `scripts/dependencies/BindingRedirectSync.psm1`. The composition root `Repair-PackageManifestConsistency.ps1` calls it unconditionally, so the pass does not depend on `-CandidateUpgrade`. The pass rewrites any `bindingRedirect` whose `newVersion` matches no csproj `<Reference>` version. The target is the sibling project's own reference version if it has one; otherwise it is the highest version referenced anywhere in the solution.
3. **Gate on main:** add one `It` block to `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`. It calls the existing `Find-OrphanedHintPath` for every project directory and expects 0 findings.
4. **No workflow change.** `dependabot-repair.yml` checks out the Dependabot branch and runs that branch's copy of the script. An unconditional pass therefore reaches every Dependabot PR after a rebase or recreate, with no YAML diff. A YAML diff would trigger `modified-workflow-needs-green-run`. That rule cannot be satisfied before merge for a `workflow_run`-only workflow.
5. After merge, comment `@dependabot recreate` on PR #984. `rebase` is not enough: Dependabot refuses to rebase a branch that carries the repair bot's commit.

## 2. Current state analysis

### 2.1 Repair composition root and modules (line counts, last content line) [V]

| File | Lines | Headroom to 500 |
|---|---|---|
| `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | 475 | 25 |
| `scripts/dependencies/ConsistencyVerifier.psm1` | 499 | 1 (full) |
| `scripts/dependencies/BindingRedirectVerification.psm1` | 139 | 361 |
| `scripts/dependencies/ProjectConsistency.psm1` | 381 | 119 |
| `scripts/dependencies/AnalyzerItemRepair.psm1` | 402 | 98 |
| `scripts/dependencies/PackageGraph.psm1` | 465 | 35 |
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | 429 | 71 |
| `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` | 152 | 348 |

### 2.2 How the existing redirect pass works [V]

- `Repair-PackageManifestConsistency.ps1:424-440`: inside the per-manifest loop, the app.config beside the manifest is reconciled only when `@($upgrade.Applied).Count -gt 0` (line 425). `Applied` is populated only by `Invoke-CandidateUpgrade` (lines 154-202), and only for ids in `-CandidateUpgrade`. The workflow passes no candidate (`dependabot-repair.yml:80`), so the pass never runs from `workflow_run`. The workflow comment at lines 87-94 records this.
- Even when `-CandidateUpgrade` is supplied, the pass covers only an app.config that sits beside a manifest declaring the upgraded package. Its target version comes from `Resolve-ReferenceAssemblyVersion` against that project's own text. A test project that receives log4net only transitively has no manifest entry, so the pass would skip it even with `-CandidateUpgrade` supplied. Supplying `-CandidateUpgrade` from the workflow therefore does not fix the defect.
- The rewrite primitive `ProjectConsistency.psm1:271-375` (`Invoke-BindingRedirectReconciliation`) is pure over text. It does a regex replace per `<dependentAssembly>` block (byte-exact outside the substituted attributes). It writes the resolved version into the `oldVersion` upper bound, keeping the lower bound, and into `newVersion`. It is reusable as is.
- `BindingRedirectVerification.psm1` provides `ConvertTo-ReferenceVersionMap` (lines 31-72), which builds a union over every csproj `Reference Include="Name, Version=..."`. It also provides `Find-StaleBindingRedirect` (lines 74-134).

### 2.3 Invariant the pester gate asserts [V]

`tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`:

- Lines 275-318: over every root-level `*/app.config`, every redirect `newVersion` must be a member of the solution-wide union map from `ConvertTo-ReferenceVersionMap` over every root-level `*/*.csproj`. The expected known-debt set is empty (line 293). The only expected unverifiable name is `netstandard` (line 294). This is the assertion that fails on CI run #1059 with `log4net|3.4.0.0`.
- Lines 320-369: for 16 named assemblies, `oldVersion` must equal `'0.0.0.0-' + newVersion`, and `newVersion` must be in the map.
- Lines 246-273: 13 Fizzler redirects at `0.0.0.0-1.3.1.0` / `1.3.1.0`.
- Property to note: the check is membership in a **union**. A stale `<Reference>` left in a test csproj keeps an old version in the map and hides a stale redirect. This is the case in PR #984 for the WebView2 redirects in QuickFiler.Test and UtilitiesCS.Test (`QuickFiler.Test/app.config:286-287`, `UtilitiesCS.Test/app.config:274-275`, both `1.0.4191.47`). The hint-path fix in section 4.1 removes that masking.

### 2.4 Workflow mechanics that shape the design [V]

- Trigger: `workflow_run` on CI `completed` (lines 19-22). The job checks out `head_branch` (line 58) and runs `"$env:GITHUB_WORKSPACE\scripts\dependencies\Repair-PackageManifestConsistency.ps1"` (line 80). The script that runs is the Dependabot branch's copy. The YAML that runs is the default branch's copy (GitHub `workflow_run` semantics). A script change therefore reaches a Dependabot PR once its branch contains main's commit (rebase or recreate).
- The commit step stages `git add --update -- '*.csproj' '*/packages.config' '*/app.config'` (line 136), so app.config writes are pushed. The push gate reads `written-count` from `WrittenPath` (lines 106, 112), so a write with no repair record is still pushed.
- `beyond-known-weak` (line 95) counts `Verification[].Report.Repair` kinds other than `Analyzer`. If the new pass emits records into `Verification[].Report.Repair`, the clause the comment says was removed "becomes load-bearing again" (lines 93-94), which would require a YAML change. The design below keeps the new records in a separate result property, so no YAML change is needed.
- `modified-workflow-needs-green-run` (`.claude/skills/feature-review-workflow/SKILL.md:68-70`) is Blocking for any diff under `.github/workflows/**`, including `.github/workflows/README.md`. This workflow cannot produce a green run against a feature branch head: it has a `dependabot/` branch filter and no `workflow_dispatch`. Agent memory records the same disposition for issue #952. Conclusion: change no file under `.github/workflows/**`.

### 2.5 Formatting gate [V]

`.csharpierignore:15-18` excludes `**/packages.config` and `**/app.config` (and lines 12-14 exclude `*.csproj/*.props/*.targets`). `_format-check.yml:41` runs `dotnet csharpier check .`, which therefore never inspects these files. Format-check passed on run #1059 [V-web]. That run included a packages.config whose BOM Dependabot had removed. The repository's canonical manifest form is the NuGet inline form enforced by `Invoke-ManifestNormalization` (`PackageGraph.psm1:395-455`, invoked at `Repair-PackageManifestConsistency.ps1:445-447`). The new pass must therefore:
- rewrite only attribute values inside a matched block (reuse `Invoke-BindingRedirectReconciliation`), leaving line terminators and every other byte unchanged;
- run before normalization, so that normalization remains the last writer.

The repair writes through `[System.IO.File]::WriteAllText` (line 371), which emits UTF-8 without BOM. Dependabot does the same (BOM removal seen in the PR #984 diff [V-web]). Neither format-check nor hygiene objects (`scripts/hygiene` has no manifest-encoding rule [V]).

## 3. Candidate approaches

### 3.1 Borrowed hint paths

- **Selected: declare in the test project's own `packages.config`, plus a Pester gate.** Dependabot's packages.config updater then updates the test project in the same grouped PR (section 4.1). Separately, `Invoke-ProjectFileRepair` (`Repair-PackageManifestConsistency.ps1:228-246`) already reconciles every HintPath, Import and Error folder segment to the manifest version for every declared package, independently of `-CandidateUpgrade`. That reconciliation acts as a backstop.
- Rejected: have the repair script synthesize the missing manifest entries automatically. This is more complex, and it repairs after Dependabot has already chosen not to update those projects. Their `<Reference Include ... Version=>` would stay stale, because assemblies not named for the package (`Microsoft.Web.WebView2.Core`) are left alone by `Get-RewrittenReferenceVersionLine` (`ProjectConsistency.psm1:108-122`).

### 3.2 Transitive redirects

- **Selected: an unconditional, solution-wide sync pass in a new module, called from the composition root.**
- Rejected: pass `-CandidateUpgrade` from the workflow. This needs a YAML change (section 2.4). The existing pass is also sibling-manifest-scoped and would still miss the five transitive configs (section 2.2).
- Rejected: add log4net to the five test `packages.config` files. This declares packages the projects do not compile against, and each future transitive bump would need another manual edit (for example `Microsoft.Graph.Core`, which 16 app.configs redirect but only 2 projects declare [V]). The issue requires no manual step.
- Rejected: delete the transitive redirects. This changes runtime binding for the test hosts, and the redirects were curated by issue #973.

## 4. Requirements mapping and design

### 4.1 Declaration edits (question 1)

Enumeration on main (complete over all 18 projects; see Numeric Derivation Evidence):

| Project | Borrowed package (folder) | HintPath lines | Production sibling declaring it |
|---|---|---|---|
| QuickFiler.Test | `Microsoft.Web.WebView2.1.0.4191.47` | `QuickFiler.Test.csproj:389`, `:392`, `:530` | `QuickFiler/packages.config:19`, `UtilitiesCS/packages.config:57` |
| QuickFiler.Test | `ObjectListView.Official.2.9.1` | `QuickFiler.Test.csproj:407` | `QuickFiler/packages.config:22` (and 5 others) |
| UtilitiesCS.Test | `Microsoft.Web.WebView2.1.0.4191.47` | `UtilitiesCS.Test.csproj:815`, `:818` | `UtilitiesCS/packages.config:57` |
| TaskTree.Test | `ObjectListView.Official.2.9.1` | `TaskTree.Test.csproj:178` | `TaskTree/packages.config:8` |

Not borrowed (verified): `UtilitiesCS.Test` hint-paths ObjectListView at line 827 and declares it at `UtilitiesCS.Test/packages.config:70`. No project has a borrowed `<Import>`, `<Error>` or `<Analyzer>` element (section 7).

Edits (manifests keep the inline NuGet form, sorted by id, with every entry carrying `targetFramework="net481"`):

- `QuickFiler.Test/packages.config`: insert `  <package id="Microsoft.Web.WebView2" version="1.0.4191.47" targetFramework="net481" />` between line 41 (`Microsoft.TestPlatform.ObjectModel`) and line 42 (`Moq`). Insert `  <package id="ObjectListView.Official" version="2.9.1" targetFramework="net481" />` between line 45 (`MSTest.TestFramework`) and line 46 (`OpenTelemetry`).
- `UtilitiesCS.Test/packages.config`: insert the WebView2 line between line 63 (`Microsoft.TestPlatform.ObjectModel`) and line 64 (`Mono.Reflection`).
- `TaskTree.Test/packages.config`: insert the ObjectListView line between line 40 (`MSTest.TestFramework`) and line 41 (`OpenTelemetry`).
- Version rule: use the version the production sibling declares at the time of the edit (1.0.4191.47 and 2.9.1 on main). A different version would itself produce an orphan at the existing hint paths.
- `QuickFiler.Test/QuickFiler.Test.csproj`: delete the second `ItemGroup` at lines 528-532, which holds a duplicate `<Reference Include="Microsoft.Web.WebView2.Core, Version=1.0.4191.47 ...">` of line 388. This is the same hand-borrowing defect. The duplicate also makes NuGet's uninstall/reinstall of the Reference non-deterministic. No other csproj edit is needed: the existing HintPaths already name the declared folder and version.

Will Dependabot then update the test projects in the same grouped PR?

- `dependabot.yml:4-5` uses `directories: ["/*"]` with one `all-nuget-updates` group [V].
- PR #984 changed 53 files: 18 csproj plus the packages.config and app.config of every project that declares a bumped package [V-web]. Examples: `QuickFiler.Test/app.config`'s log4net redirect moved to 3.5.0.0 because QuickFiler.Test declares log4net. Tags.Test does not declare it and was not touched. This shows that Dependabot updates every project directory that declares the dependency.
- Once WebView2 and ObjectListView are declared in those test projects, they join the group update for those ids. Expected side effects of NuGet's packages.config update in the test projects:
  - the Reference/HintPath rewrite (seen for production `QuickFiler.csproj` in #984: `processorArchitecture` dropped, `<Private>True</Private>` added) [V-web];
  - likely an added `Microsoft.Web.WebView2.targets` `<Import>`/`<Error>` and a `Microsoft.Web.WebView2.Wpf` reference [U]. Both are benign for a test project, and the `<Import>` is declared, so the Import gate at `RepositoryTreeConsistency.Tests.ps1:65-90` stays green.

### 4.2 Redirect-sync pass (question 2)

**Placement:** a new module, `scripts/dependencies/BindingRedirectSync.psm1`.

- `ConsistencyVerifier.psm1` is full (499).
- `ProjectConsistency.psm1` (381) would reach roughly 490 with about 110 lines.
- `BindingRedirectVerification.psm1` is documented as the detection layer (header lines 6-13), so adding a writer there mixes concerns.
- The composition root has 25 lines of headroom, so it receives only the call and the result wiring (about 12-15 lines).
- The new module imports `PackageGraph.psm1`, `BindingRedirectVerification.psm1` and `ProjectConsistency.psm1`, without `-Force`, matching the repo's pattern (`ConsistencyVerifier.psm1:26-31`). None of these imports the new module, so no cycle is created.

**Exported functions (proposed contracts):**

1. `Invoke-BindingRedirectSync -AppConfigText <string> -DeployedVersionProvider <scriptblock> [-PreferredVersionProvider <scriptblock>]`. Pure over text. Returns `BindingRedirectSync.Result` with fields `Text`, `Repair[]` (Kind `BindingRedirectSync`, AssemblyName, From, To, Rule), `Unverifiable[]`, `Unresolvable[]` and `ExaminedCount`. For each record from `ConvertFrom-AppConfigText` with a non-empty `NewVersion`:
   - `deployed` = distinct non-empty versions from `DeployedVersionProvider`. If the set is empty, add the name to `Unverifiable` and change nothing. This covers `netstandard` and matches `Find-StaleBindingRedirect` semantics.
   - If `deployed -contains NewVersion`, change nothing. This is the idempotence rule, and it preserves curated ranges such as Fizzler's.
   - Otherwise, choose the target:
     - (a) the single version from `PreferredVersionProvider` (the sibling csproj's own `Reference` versions for that name), if exactly one exists and it is in `deployed`;
     - (b) otherwise, the maximum of `deployed` by `[System.Version]`;
     - (c) if any candidate fails `[System.Version]::TryParse`, add the name to `Unresolvable` and change nothing.
   - Rationale for (a) and (b): this mirrors MSBuild ResolveAssemblyReference conflict resolution, where a primary reference wins and otherwise the highest version wins [U: documented MSBuild behaviour, not re-verified in this session].
   - Apply the chosen target with `Invoke-BindingRedirectReconciliation -AssemblyName -AssemblyVersion`. That sets the `oldVersion` upper bound and `newVersion`, which satisfies both invariants in section 2.3.
2. `Invoke-SolutionBindingRedirectSync -DirectoryLister -TextReader -TextWriter [-ProjectTextOverride <hashtable>]` with `SupportsShouldProcess`. This mirrors `Invoke-ManifestNormalization`.
   - Build the map with `ConvertTo-ReferenceVersionMap` over every `*.csproj` path the lister returns, preferring the post-repair text in `-ProjectTextOverride` keyed by path.
   - For every `Get-PackageManifestPath -Kind AppConfig` path, call 1 with a preferred provider read from the csproj in the same directory.
   - Write only when the text changed and `ShouldProcess` approves.
   - Return `ChangedPath[]`, `Repair[]`, `Unverifiable[]`, `Unresolvable[]` and `ExaminedAppConfig`.
3. `Format-BindingRedirectSyncReport -Repair <object[]>` returns a `## Binding redirects synchronised` block, or an empty string when there are no repairs. This keeps report text out of the 475-line composition root.

**Composition-root changes** (`Repair-PackageManifestConsistency.ps1`):

- `Import-Module` the new module.
- Inside the main loop, record `$repaired.Text` per project path in a hashtable. This is one line after line 418, so that `-WhatIf` runs see the repaired references.
- After the loop (after line 441) and before normalization (line 445), call `Invoke-SolutionBindingRedirectSync ... -WhatIf:$WhatIfPreference`. The explicit pass-through is needed for the same module-preference reason as lines 443-444. Add `ChangedPath` to `$written`.
- Add `RedirectSync` (Repair, Unverifiable, Unresolvable) to the result, and append the formatted block to `Body`.
- Do **not** add the records to `Verification[].Report.Repair`. That keeps `beyond-known-weak` semantics and the workflow comment at lines 87-94 accurate, so no YAML diff is needed.
- Unverifiable and Unresolvable names are non-fatal and reported. They do not affect `IsSuccess`, so a push of the other repairs is never blocked. The CI pester gate remains the backstop.

**Relationship to the existing pass:** it complements the existing pass and does not replace it. The existing CandidateUpgrade pass (lines 424-440) uses restored-package identity evidence and still serves local runs that pass `-CandidateUpgrade`. When both run, the existing pass writes first, and the sync pass then finds `newVersion` already in the map and changes nothing. The existing context at `Repair-PackageManifestConsistency.Tests.ps1:140-230` keeps its expectations.

**Idempotence and loop safety:** a second run produces zero writes, because every rewritten `newVersion` is now in the map. This matters because each repair push re-runs CI, which re-fires the workflow.

**Ambiguity cases:**
- Multiple referenced versions and the stale version not among them: rule (a), then (b).
- Multiple referenced versions and the current version among them: no change.
- No referenced version: no change; the name is reported as unverifiable.
- Unparsable version: no change; the name is reported as unresolvable.

**Expected effect on PR #984 after recreate:**
- The log4net redirects in Tags.Test, TaskTree.Test, TaskVisualization.Test, ToDoModel.Test and VBFunctions.Test move from `0.0.0.0-3.4.0.0/3.4.0.0` to `0.0.0.0-3.5.0.0/3.5.0.0`. The map after Dependabot's update contains only `3.5.0.0` for log4net: all 11 referencing projects also declare log4net, so Dependabot rewrites every Reference [V: 11 Reference lines, 11 declaring manifests].
- No other redirect changes. All other bumped assemblies are declared wherever they are redirected, except `Microsoft.Graph.Core`, which #984 does not bump.

### 4.3 Main-branch gate (question 4)

Add one `It` to the `Describe 'Repository tree consistency (issue 929)'` block in `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`:

- Reuse the pair-discovery shape of lines 67-73: every root-level directory with a `packages.config` and exactly one csproj.
- Call `Find-OrphanedHintPath` (`ConsistencyVerifier.psm1:122-154`) for each pair.
- Assert that the summed `ExaminedCount` is greater than 0, and that the finding count is 0. List the findings as `<project>: line <n> <PackageFolder>`.
- The module is already imported at line 5. The file stays near 180 lines.

Pre-fix expectation on main: 7 findings in 3 projects (section 4.1 table). This is the required fail-first regression test. Post-fix expectation: 0. Every other HintPath in the 18 projects names a folder whose `Id.Version` is declared in the same project's manifest (Numeric Derivation Evidence). `Find-OrphanedHintPath` compares `Id + '.' + Version` with `-contains`, which is case-insensitive, so the expectation holds.

### 4.4 Workflow (question 5)

- No change to `.github/workflows/dependabot-repair.yml`. The new pass is unconditional inside the script that the workflow already runs (line 80), and its writes reach the push through `WrittenPath` (lines 106, 112, 136).
- No change to `_pester.yml`: its `Run.Path` and `CodeCoverage.Path` already cover `tests/scripts/dependencies` and `scripts/dependencies` (lines 41, 45), so the new module and tests are picked up automatically.
- With no workflow diff, `modified-workflow-needs-green-run` does not fire. Do not edit `.github/workflows/README.md` either: the rule's path pattern covers it. Document the new pass in the module help and in the feature folder.

### 4.5 Error handling and logging

- Pure functions throw only through the parser: `ConvertFrom-AppConfigText` rejects non-configuration text.
- The I/O wrapper reports skipped names in the result object and emits one `Write-Information` summary line with `-InformationAction Continue`. This follows the precedent at `Repair-PackageManifestConsistency.ps1:95-104`, which explains why the verbose stream is invisible in the workflow.

## 5. Other failure modes in PR #984 (question 6)

| Area | Change in #984 | Status after this fix |
|---|---|---|
| WebView2 1.0.4191.47 to 1.0.4258.31 | Production updated; tests borrow 4191.47 | Fixed by 4.1. Dependabot updates the test manifests and csproj. The repair's HintPath reconciliation is a backstop. |
| log4net 3.4.0 to 3.5.0 | 5 transitive redirects stay at 3.4.0.0 | Fixed by 4.2 |
| Analyzer `Include` paths (Meziantou 3.0.296, Roslynator 5.0.1, Sonar 10.35, MSTest.Analyzers 4.5.1) | Stale items | Already repaired by the existing pass (fff92fe83) |
| New analyzer rules in bumped analyzers | Possible new diagnostics | `.editorconfig:27` sets `dotnet_analyzer_diagnostic.severity = suggestion`, so new rules cannot become errors under `/p:TreatWarningsAsErrors=true` (`_build-nullable.yml:62-64`). Exception: `MSTEST0032` is pinned to `warning` (`.editorconfig:29`). If MSTest.Analyzers 4.5.1 widens that rule, build-nullable fails [U]. |
| MSTest.TestFramework 4.5.1 | Possible new `[Obsolete]` APIs | A compiler `CS0618` warning is not covered by the analyzer default and would fail build-nullable [U] |
| `processorArchitecture` removed, `<Private>True</Private>` added | Production csproj | Benign. Format-check excludes csproj; the build passed for those projects except the borrowing tests [V-web: only the 4 known jobs failed] |
| packages.config BOM removed | QuickFiler.Test | Benign; format-check passed on #1059 [V-web] |
| New WebView2 redirect blocks (QuickFiler app.config) | `0.0.0.0-1.0.4258.31` | Consistent with the new Reference versions; pester gate satisfied |
| Dropped `<Compile>` items | Not observable: the GitHub diff view did not render 48 of 53 files [V-web] | Unverified [U] |

Mandatory first task for the plan (P0): the orchestrator has `gh`, so run `gh run view 37959420911 --log-failed` and `gh pr diff 984`. Confirm that:
- (a) the build-analyzers and build-nullable errors are only `CS0012`/`CS0234`/`CS0246` in QuickFiler.Test and UtilitiesCS.Test, with no `CS0618` and no `MSTEST0032`;
- (b) no `-    <Compile Include=` line appears in the diff;
- (c) the mstest-coverage failure is only the build cascade.

If any of these fails, add it to scope before planning. The rows marked [U] above become in-scope items.

**`@dependabot recreate`:** this is the right command. `@dependabot rebase` is refused once a non-Dependabot commit (fff92fe83) is on the branch. After recreate:
1. Dependabot recomputes the group from main, now including the test-project declarations, and pushes.
2. CI runs. It may fail on the transitive redirects.
3. `dependabot-repair` runs the branch's script, which is now main's script. It reconciles, syncs the 5 redirects and pushes.
4. CI re-runs; the expected result is green.
5. `dependabot-repair` re-fires and writes nothing (idempotent), so no push happens.

The [U] analyzer/obsolete risks above remain until step 4 is observed.

## 6. Testing implications (question 7)

All fixtures are in-memory strings and hashtable stores; no temporary files, as in the existing suites. Tests go in Pester 5 under `tests/scripts/dependencies`, and the toolchain runs through PoshQC MCP: format, then analyze, then test.

- **New `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1`:**
  - `Invoke-BindingRedirectSync`:
    - a stale transitive redirect is rewritten in both positions, with the lower bound preserved;
    - a single-version `oldVersion` is replaced outright;
    - a current redirect is unchanged and a second pass produces zero repairs (idempotence);
    - an unknown name is unverifiable and unchanged;
    - with several deployed versions, the preferred own reference wins;
    - with several deployed versions and no own reference, the highest `[System.Version]` wins (use `3.10.0.0` vs `3.9.0.0` to prove numeric rather than string ordering);
    - an unparsable version is unresolvable and unchanged;
    - a block with no `bindingRedirect` is skipped;
    - empty text examines 0;
    - non-configuration text throws;
    - CRLF bytes outside the substituted span are byte-identical (`-ceq` on the remainder).
  - `Invoke-SolutionBindingRedirectSync`: two-project store (Prod csproj referencing `log4net, Version=3.5.0.0`; Test directory with only an app.config at 3.4.0.0); the override is preferred over the reader text; `-WhatIf` writes nothing; `ChangedPath` is exact.
  - `Format-BindingRedirectSyncReport`: empty input gives an empty string; non-empty input gives a heading plus one line per repair.
- **Composition-root integration:** a new file `Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` keeps the existing test file (429 lines) under 500. It reuses the `Get-RepairFixture` store pattern (`Repair-PackageManifestConsistency.Tests.ps1:86-120`). It asserts that:
  - a run with no `-CandidateUpgrade` rewrites the transitive redirect;
  - `WrittenPath` contains the app.config;
  - `Body` carries the new block;
  - `Verification[].Report.Repair` has no `BindingRedirectSync` kind;
  - a second run writes nothing.

  The existing contexts must stay green without edits, notably the agreeing tree (`WrittenPath` 0, line 241), `-WhatIf` (line 277), and the SVGControl-scoped `-WhatIf` run (line 378).
- **Repository gate:** the new `It` in `RepositoryTreeConsistency.Tests.ps1` (section 4.3). Fail-first evidence: run it before the manifest edits (7 findings), then after (0).
- **Coverage:** `_pester.yml` enforces line coverage >= 80% over `scripts/dependencies` (line 71); the repo policy target is >= 85%. Target >= 90% line coverage on the new module (new-code rule) and no regression on the changed lines of the composition root.

## 7. Numeric Derivation Evidence

### Claim N1: borrowed HintPath elements on main = 7 lines, in 4 (project, package) pairs, across 3 projects

- **Complete Family:** every `<HintPath>` element in every csproj of `TaskMaster.sln` (18 C# projects, `TaskMaster.sln:6-48`) whose path contains `packages\<Id>.<Version>\`. Each is compared with the `<package id version>` entries of the `packages.config` in the same directory.
- **Exhaustive Search Scope:** `*/*.csproj` (18 files) and `*/packages.config` (18 files) at the worktree root. A recursive `**/*.csproj` glob returned the same 18. A grep for `<HintPath>` under `packages\` followed by anything other than `lib\` returned no matches, so all package HintPaths are under `lib`.
- **Inclusion Rules:** a HintPath whose `Id.Version` folder is not exactly declared in the sibling manifest. This mirrors `Find-OrphanedHintPath`.
- **Exclusion Rules:** HintPaths that do not reference `packages\`, and non-HintPath kinds (covered separately by N1-aux).
- **Primary Search Strategy:** grep `packages\\[^\\]+\\lib` with `-o` over the test csproj (608 lines) and the production csproj, and grep `id="[^"]+" version="[^"]+"` with `-o` over each manifest. Then a per-project manual set difference.
- **Primary Member Set:**
  - QuickFiler.Test: lines 389, 392 and 530 (WebView2 1.0.4191.47); line 407 (ObjectListView 2.9.1)
  - UtilitiesCS.Test: lines 815 and 818 (WebView2)
  - TaskTree.Test: line 178 (ObjectListView)
- **Primary Count:** 7 lines, 4 pairs, 3 projects. The other 6 test projects and all 9 production projects have 0.
- **Cross-check Search Strategy:** a manifest-side query.
  - (i) A version-pinned alternation regex over the 53 ids shared by every test csproj. It returned count = 53 in each of the 9 test manifests.
  - (ii) A grep of the remaining distinctive ids per test manifest (log4net, log4net.Ext.Json, Bcl.TimeProvider, TimeProvider.Testing, Newtonsoft.Json, ExCSS, Svg, Castle.Core, FluentAssertions, Moq, AdapterUtilities, MSTest.TestFramework, WebView2, ObjectListView).
  - (iii) A targeted grep `packages\\(Microsoft\.Web\.WebView2|ObjectListView\.Official)\.` over `*.Test/*.csproj` (8 lines), against `id="(Microsoft\.Web\.WebView2|ObjectListView\.Official)"` over all manifests (9 declarations; UtilitiesCS.Test is the only test project declaring either id).
- **Cross-check Member Set:** the 8 lines from (iii) minus `UtilitiesCS.Test:827` (declared) = {QuickFiler.Test 389, 392, 407, 530; UtilitiesCS.Test 815, 818; TaskTree.Test 178}. (i) and (ii) show that every other test HintPath id is declared at its exact version.
- **Cross-check Count:** 7 lines, 4 pairs, 3 projects.
- **Member-set Comparison:** identical after normalisation to (project, line, folder).
- **Limitation:** the cross-check is exhaustive for the 9 test projects. For the 9 production projects it reuses the primary enumeration (manual comparison of each production HintPath list against its manifest), so it is not independent there.
- **Disposition:** withhold `7` as an approved spec acceptance criterion. State the AC as "the `Find-OrphanedHintPath` gate reports 0 findings over every project". Record the gate's pre-fix output, expected to be 7, as the executable cross-check in `evidence/regression-testing/`.

### Claim N1-aux: no borrowed Import, Error or Analyzer element on main

- Import/Error: 230 matched lines. They reconcile as 202 standard-package lines (MSTest.TestAdapter/TestFramework 4.4.1, Microsoft.Testing.* 2.4.1, Meziantou 3.0.290, ValueTuple 4.6.2), plus 10 `System.Reactive.7.0.0` lines, plus 18 lines for NETStandard.Library, Microsoft.ML, CpuMath, Tesseract and WebView2, all in projects that declare them.
- Analyzer: 162 items in 17 projects, all matching the six analyzer packages at their declared versions. The count from the version-pinned regex equals the count from an unfiltered `<Analyzer Include=` grep: 162 = 162.
- Meziantou Import/Error lines: 32 = 16 projects x 2, which is exactly the 16 projects that declare Meziantou.

### Claim N2: transitive log4net redirects that go stale under a log4net bump = 5 app.configs

- **Complete Family:** every `*/app.config` (17 files) carrying a `dependentAssembly` named `log4net`.
- **Exhaustive Search Scope:** `*/app.config` at the root (17 files).
- **Inclusion Rules:** the config's sibling project neither declares log4net in `packages.config` nor carries `Reference Include="log4net, Version=..."`.
- **Exclusion Rules:** configs whose project declares or references log4net, because Dependabot updates those.
- **Primary Search Strategy:** grep `name="log4net"` over `*/app.config` gives 16 configs. Grep `id="log4net"` over `*/packages.config` gives 11 manifests. Set difference.
- **Primary Member Set:** {VBFunctions.Test, TaskTree.Test, Tags.Test, TaskVisualization.Test, ToDoModel.Test}
- **Primary Count:** 5
- **Cross-check Search Strategy:** grep `Include="log4net, Version=[^,]+` over `*/*.csproj` gives 11 projects. Set difference against the 16 redirecting configs.
- **Cross-check Member Set:** {VBFunctions.Test, TaskTree.Test, Tags.Test, TaskVisualization.Test, ToDoModel.Test}
- **Cross-check Count:** 5
- **Member-set Comparison:** identical. It also matches the issue's list.

## 8. Complete list of production files to change

1. `QuickFiler.Test/packages.config`: add WebView2 1.0.4191.47 and ObjectListView.Official 2.9.1.
2. `UtilitiesCS.Test/packages.config`: add WebView2 1.0.4191.47.
3. `TaskTree.Test/packages.config`: add ObjectListView.Official 2.9.1.
4. `QuickFiler.Test/QuickFiler.Test.csproj`: remove the duplicate WebView2.Core Reference ItemGroup (lines 528-532).
5. `scripts/dependencies/BindingRedirectSync.psm1`: new.
6. `scripts/dependencies/Repair-PackageManifestConsistency.ps1`: import the module, call it, wire the result and body (must stay at or under 500 lines; currently 475).

Production PowerShell files: 2 (items 5 and 6). This is within the direct-mode budget of 1-3. Tests:
- `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` (new)
- `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` (new)
- `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` (one new `It`)

Explicitly unchanged: every file under `.github/**`, `ConsistencyVerifier.psm1` (full), `ProjectConsistency.psm1` and `BindingRedirectVerification.psm1`.

## Automation Feasibility

- **Fully automatic after merge, per Dependabot PR:**
  - the Dependabot group update now includes the test projects, which declare the packages they compile against;
  - `dependabot-repair` runs the branch's script, which reconciles analyzer items, HintPaths and the redirect sync, and pushes;
  - CI re-runs; the repair re-fires with no writes.
  - No YAML change, secret, label or human edit is involved.
- **One-time manual action:** a `@dependabot recreate` comment on PR #984 after this fix merges. The orchestrator can post it with `gh pr comment 984 --body "@dependabot recreate"`, so no human is needed. It is required because a branch created before the merge carries the pre-fix script. A future Dependabot PR opened after the merge needs nothing.
- **Regression prevention:**
  - the orphaned-HintPath Pester gate fails main if a test project borrows a package again;
  - the existing redirect invariant (`BindingRedirectVerification.Tests.ps1:275-318`) plus the unconditional sync covers every future transitive redirect, for example `Microsoft.Graph.Core`, which 16 configs redirect but only 2 projects declare.
- **Residual non-automatable risk:** an upstream package release that introduces a warning-severity compiler diagnostic (`CS0618`) or widens `MSTEST0032` would fail build-nullable. Neither the repair nor this design can fix that automatically. It must be confirmed absent for #984 by the P0 log inspection (section 5) and observed on the recreated PR.
- **Pre-merge verification limit:** the end-to-end workflow path cannot be exercised before merge (`workflow_run` uses the default-branch YAML, filters on `dependabot/` branches, and has no `workflow_dispatch`). Local evidence is limited to:
  - the Pester suites;
  - a `-WhatIf` run of `Repair-PackageManifestConsistency.ps1` against a working copy of PR #984's tree, fetched with `git fetch origin dependabot/nuget/QuickFiler.Test/all-nuget-updates-2542c001c9` into a separate worktree. That run should report the 5 log4net redirect changes and zero orphaned HintPaths beyond the borrowed WebView2 entries that predate the declaration.

The post-merge proof is the green CI run on the recreated PR #984.

## Rejected alternatives (brief)

- Supply `-CandidateUpgrade` from the workflow: needs a YAML change that cannot be green-run, and still misses transitive configs.
- Declare log4net in five test manifests: wrong semantics, and needs a manual edit per future transitive bump.
- Delete the transitive redirects: changes runtime binding, and undoes issue #973's curation.
- Auto-synthesise manifest entries for borrowed HintPaths: more complex, and leaves the Reference versions stale.
- Fail the run on ambiguous redirect versions: blocks unrelated repairs from being pushed; the RAR-equivalent rule plus a report was chosen instead.
