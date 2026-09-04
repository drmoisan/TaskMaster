# Research Findings — Issue #752: coverage assembly discovery excludes own worktree root

- Timestamp: 2026-09-03T00-00
- Scope: research only, no source changes made
- Worktree: `<repo-root>` (branch `bug/coverage-assembly-discovery-excludes-own-worktree-root-752`)
- All line numbers below were re-derived directly from the current worktree state, not trusted from the issue text.

## 1. Current State — `Invoke-MSTestWithCoverage.ps1`

File: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (350 lines total, verified via full read).

### 1.1 The exclusion predicate and its pipeline

The discovery block is at **lines 296–303**, inside `Invoke-MSTestWithCoverageMain` (function body starts at line 248):

```powershell
296  $testAssemblies = @(Get-ChildItem -Path $resolvedSearchRoot -Recurse -Filter '*.Test.dll' |
297          Where-Object {
298              $_.FullName -match "\\bin\\$Configuration\\" -and
299              $_.FullName -notmatch '\\obj\\' -and
300              $_.FullName -notmatch '\\ref\\' -and
301              $_.FullName -notmatch '\\\.claude\\'
302          } |
303              Select-Object -ExpandProperty FullName)
```

- The defect predicate is **line 301**: `$_.FullName -notmatch '\\\.claude\\'`. This matches the issue text's cited line number exactly.
- Sibling exclusions in the same `Where-Object`: `\bin\<Configuration>\` (positive match, line 298), `\obj\` (line 299), `\ref\` (line 300). All four clauses test the same `$_.FullName` (absolute path) property; none of the other three are affected by the search-root-location defect because they match path *segments* that only ever appear inside a project's own build-output tree, not as a prefix contributed by the search root itself.
- The whole pipeline is wrapped in `@(...)` at the assignment site (line 296/303), which the `Invoke-MSTest.ps1` sibling script's docstring (see §1.2) explains is required so a zero-match run yields an empty array rather than `$null`, and a one-match run yields a one-element array rather than a bare string, under `Set-StrictMode -Version Latest` (declared at line 245).

### 1.2 Call sites and factoring

This discovery predicate appears **exactly once** in the file — inline in the `Where-Object` at lines 297–302, directly inside `Invoke-MSTestWithCoverageMain`. It is **not** factored into a named function/filter in this script, unlike the sibling `Invoke-MSTest.ps1`, whose equivalent pipeline is factored into `Get-MSTestAssemblyPathList` (see §2). There is only one call site to edit for the fix; no other code path in this file constructs or filters `$testAssemblies`.

### 1.3 `$resolvedSearchRoot` computation

Lines 271–276:

```powershell
271  $repoRoot = (Resolve-Path (Join-Path $ScriptRoot '..\..')).Path
272  $resolvedSearchRoot = Join-Path $repoRoot $SearchRoot
273
274  if (-not (Test-Path $resolvedSearchRoot)) {
275      throw "Search root not found: $resolvedSearchRoot"
276  }
```

- `$repoRoot` passes through `Resolve-Path`, so it is an absolute, fully normalized path (no `..`, no trailing `.`).
- `$resolvedSearchRoot` does **not** pass through `Resolve-Path`/`GetFullPath` — it is a raw `Join-Path $repoRoot $SearchRoot`. When `$SearchRoot` is left at its default `.` (set at lines 263–265 when the caller passes nothing), `$resolvedSearchRoot` is literally `"$repoRoot\."` with a trailing `\.` segment. This is independently confirmed by the existing test assertion in `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:413`, which expects the thrown message `'Search root not found: C:\repo\.'` for the equivalent coverage-script code path.
- This trailing-`.` shape is not itself a defect for the proposed fix: `[System.IO.Path]::GetRelativePath(relativeTo, path)` in .NET (the runtime backing PowerShell 7+, confirmed as the required version by `.claude/rules/powershell.md:24`) resolves both of its arguments through `GetFullPath` internally before computing the relative path, so a trailing `\.` on `$resolvedSearchRoot` does not need to be stripped before calling `GetRelativePath`.
- `Get-ChildItem -Path $resolvedSearchRoot -Recurse` (line 296) guarantees every candidate `FullName` is a descendant of `$resolvedSearchRoot`, so `GetRelativePath($resolvedSearchRoot, $candidatePath)` will not produce a leading `..` in the ordinary case (no symlink/junction escape scenario was found in this script).

### 1.4 Sibling script comparison — `Invoke-MSTest.ps1`

File: `scripts/vscode/Invoke-MSTest.ps1` (203 lines).

- Its discovery pipeline is factored into a named function, `Get-MSTestAssemblyPathList` (lines 97–127), called once at line 179 (`Get-MSTestAssemblyPathList -SearchRoot $resolvedSearchRoot -Configuration $Configuration`).
- That function's `Where-Object` (lines 121–125) has only **three** clauses — `\bin\<Configuration>\`, `-notmatch '\\obj\\'`, `-notmatch '\\ref\\'` — and **no `\.claude\` clause at all**. It therefore does not exhibit the self-exclusion defect (it has no `.claude` predicate to be over-broad), but it also does not exclude sibling `.claude\worktrees\` subtrees, which is a documented, deliberately out-of-scope gap: the #733 research artifact (`docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/research/research-findings.2026-09-02T13-15.md:77`) states explicitly: *"`Invoke-MSTest.ps1`'s own discovery block ... has the identical unfiltered shape and would benefit from the same clause for parity, but finding 3 as scoped in the issue names only `Invoke-MSTestWithCoverage.ps1`; expanding to the sibling script would be a scope decision for the `prd-feature`/planning stage, not this research."* This gap in `Invoke-MSTest.ps1` remains unaddressed on `main` as of this research and is out of scope for #752 unless the planner explicitly decides to extend the fix's relative-path approach there too (same rationale as #733's own scoping note).
- The extraction of `Get-MSTestAssemblyPathList` in this script was driven by a **different** bug (issue #733 "finding 7": an un-wrapped assignment producing `$null`/bare-string shapes that break under `Set-StrictMode`), not by a testability gap — confirming §5 below.

## 2. Sibling-defect search — other absolute-`FullName`-vs-`.claude` predicates

Searched every `*.ps1`/`*.sh` file under `scripts/` (repo-wide `\\\.claude\\` grep, plus a targeted `-notmatch|"FullName -match"` grep across `scripts/vscode/`).

**Result: no other production predicate matches an absolute path against `\.claude\` or an equivalent sibling-worktree pattern anywhere in the repo.** The only production hit for the literal pattern `\\\.claude\\` is `scripts/vscode/Invoke-MSTestWithCoverage.ps1:301` itself; every other hit for that regex across the repo is in `docs/**` (issue/plan/research/evidence markdown) or `.claude/settings.json` (unrelated hook configuration), not executable path-filtering logic.

Specifically checked and cleared:
- `scripts/vscode/Invoke-MSTest.ps1` — no `.claude` clause present (§1.4).
- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — the only other `Where-Object`/`FullName` filter in this family is `Get-KoverageProjectAllowlist` (lines 14–19), which filters `*.csproj`/`*.vbproj`/`*.fsproj` files by `\bin\`, `\obj\`, `\packages\`. It has **no `.claude` exclusion at all**, so it cannot exhibit the same absolute-vs-relative self-exclusion defect (there is no predicate to be over-broad). It may over-include sibling-worktree project files into the Koverage allowlist when run from the main checkout, but that is a distinct, unreported concern outside the scope of #752 and not evidence of "the same defect."
- `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`, `Invoke-MSTestWithCoverage.PackageRate.ps1`, `Invoke-MSTestWithCoverage.Threshold.ps1` — no `claude` or `notmatch`/`FullName -match` path-filtering hits at all.
- `scripts/bash/shell_qc_lib.sh:335` references `.claude/lib/bash` but as an **inclusion** root for shell-QC coverage scope, not an absolute-path exclusion predicate; unrelated mechanism, unrelated tool, not in scope.

Conclusion for Q3: **no sibling defect of the same class exists elsewhere in the repo.** The fix is confined to the one predicate at line 301.

## 3. Existing Pester coverage — critical finding

Test files under `tests/scripts/vscode/` (verified via glob):
`Install-RepoDotNetSdk.Tests.ps1`, `Invoke-MSTest.AssemblyDiscovery.Tests.ps1`, `Invoke-MSTest.Main.Tests.ps1`, `Invoke-MSTest.RunSettings.Tests.ps1`, `Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`, `Invoke-MSTestWithCoverage.Helpers.Tests.ps1`, `Invoke-MSTestWithCoverage.Merge.Tests.ps1`, `Invoke-MSTestWithCoverage.PackageRate.Tests.ps1`, `Invoke-MSTestWithCoverage.Threshold.Tests.ps1`, `Invoke-VSBuild.Tests.ps1`.

The relevant test file is **`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`** (488 lines, verified by count — this despite its name also covering both `Invoke-MSTest.ps1` and `Invoke-MSTestWithCoverage.ps1` symbols).

### 3.1 Import/fixture mechanism (no temp files)

`BeforeAll` (lines 3–26):
- Dot-sources `Invoke-MSTest.ps1` directly (its top-level wiring is guarded by `if ($MyInvocation.InvocationName -ne '.')`, so dot-sourcing imports only definitions).
- Parses `Invoke-MSTestWithCoverage.ps1` via `[System.Management.Automation.Language.Parser]::ParseFile(...)` into an AST, then dot-sources `$coverageAst.GetScriptBlock()` — this is how both same-named functions (`Resolve-RunSettingsPath`, etc.) coexist without one script's top-level guard executing.
- Also dot-sources `Invoke-MSTestWithCoverage.Helpers.ps1`.

All fixtures are **in-memory `[pscustomobject]` records** (e.g. `[pscustomobject]@{ FullName = 'C:\repo\...\A.Test.dll' }`) returned from `Mock Get-ChildItem { ... }`. No real files or temp files are created anywhere in this file, consistent with CLAUDE.md's prohibition on temp files in tests.

### 3.2 The existing regression test that must be preserved/updated

`Describe 'Invoke-MSTestWithCoverageMain'` (starting line 346) has a `BeforeEach` (lines 347–373) that mocks every external seam (`Resolve-Path`, `Test-Path`, `Resolve-RunSettingsPath`, `Invoke-VsWhereExe`, `Get-Command`, `Get-ChildItem`, `Invoke-DotnetCoverageCollection`, `Get-Content`, `ConvertTo-KoverageCoberturaXml`, `Set-Content`), then exercises `Invoke-MSTestWithCoverageMain` directly (not a factored-out discovery function).

The existing test at **lines 416–442**, `'excludes assemblies discovered under a .claude worktree segment'`, is the #733/#748-added regression pin for the original fix:

```powershell
416  It 'excludes assemblies discovered under a .claude worktree segment' {
      ...
420      Mock Get-ChildItem {
421          @(
422              [pscustomobject]@{ FullName = 'C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' },
423              [pscustomobject]@{ FullName = 'C:\repo\.claude\worktrees\agent-1\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' }
424          )
425      }
      ...
440      $script:capturedTestAssembly |
441          Should -Be @('C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll')
442  }
```

In this test's fixed `BeforeEach`, `Resolve-Path` is mocked to always return `Path = 'C:\repo'` regardless of input, so `$resolvedSearchRoot` in this test resolves to `'C:\repo\.'` — the search root itself is **not** located under `.claude`, only the second candidate is. This test does not currently exercise the self-exclusion scenario (search root itself under `.claude\worktrees\...`) and will need a **new**, additional case, not a modification of this one, since this one still correctly documents the "exclude a nested sibling worktree" behavior that must be preserved by any relative-path fix.

### 3.3 Testability of the inline predicate — no factoring required

Because `Invoke-MSTestWithCoverageMain` is itself a fully mockable, directly callable function (already proven functional by the existing test above), the discovery pipeline at lines 296–303 is **already reachable and already tested by Pester without any code extraction**, by mocking `Get-ChildItem` and calling `Invoke-MSTestWithCoverageMain` with `-NoExecute` or with the rest of the `BeforeEach` mocks in place. This is independently confirmed by the #733 research artifact (`docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/research/research-findings.2026-09-02T13-15.md:73`), which states for this exact block: *"already inside a testable function — no extraction needed."*

This directly answers Q7: **the directed fix (relativize the `.claude` match against `$resolvedSearchRoot`) does not require factoring the predicate into a separate function to be reachable by Pester.** The existing `BeforeEach`/mock harness in `Invoke-MSTest.RunSettings.Tests.ps1` already exercises this exact code path end-to-end. A new `It` block in the same `Describe 'Invoke-MSTestWithCoverageMain'` (or a new sibling test file, see §3.4) mocking `Get-ChildItem` to return an assembly path located *under* a `.claude\worktrees\agent-N\` search root — with `Resolve-Path` mocked to return that same `.claude`-rooted path as `$repoRoot` — is sufficient to pin the fix without any production-side extraction.

That said, extraction into a small pure function (mirroring `Get-MSTestAssemblyPathList` in the sibling script) is a legitimate alternative the planner may still choose for consistency/DRY reasons; it is not blocked by any testability gap, only a style/consistency tradeoff (see §6).

### 3.4 File-size constraint on where new tests can land

`Invoke-MSTest.RunSettings.Tests.ps1` is currently **488 lines** (verified by line count), against the repo's 500-line cap for test files (`.claude/rules/general-code-change.md` "File Size Limit" and `.claude/rules/powershell.md:35`, "Keep scripts cohesive and under 500 lines"). Adding the two new regression cases named in the issue's own proposed-fix section (self-exclusion when the search root is under `.claude\worktrees\`, and continued exclusion of a nested sibling worktree beneath the search root) as new `It` blocks in this file would very likely push it over the 500-line cap (each comparable existing `It` block in this file runs roughly 15–30 lines). This is a concrete planning constraint: the new assembly-discovery regression tests should either go into a **new, dedicated test file** (e.g. `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`, mirroring the naming convention already used for the sibling script's `Invoke-MSTest.AssemblyDiscovery.Tests.ps1`) or require trimming/relocating existing content in `Invoke-MSTest.RunSettings.Tests.ps1` to stay under the cap.

## 4. Testing conventions for this area

Confirmed from `.claude/rules/powershell.md` (authoritative) and the existing test files:

- Framework: **Pester v5.x**. Files named `*.Tests.ps1` under `tests/scripts/vscode/` mirroring `scripts/vscode/`.
- Structure: `Describe`/`Context`/`It`, one behavior per `It` (`Invoke-MSTest.RunSettings.Tests.ps1` uses `Describe` + flat `It`s for most blocks, and one `Describe`/`Context`/`It` nesting for the derived-settings lifecycle at lines 175–332).
- No `InModuleScope` — these are dot-sourced scripts, not modules, so `Mock` targets the dot-sourced script-scope functions directly.
- Mocking rules from `.claude/rules/powershell.md` §"Mocking Rules": never mock external executables directly (mock the wrapper seam, e.g. `Invoke-VsWhereExe`, `Invoke-DotnetCoverageExe`); mock signatures must match production named parameters exactly; register mocks before code-under-test resolves commands; when importing via AST/`ScriptBlock`, dot-source the returned `ScriptBlock` in test scope and import dependencies/wrapper seams before mocking them.
- Fixtures are always in-memory `[pscustomobject]` / here-strings — never real or temp files, consistent with the repo-wide temp-file prohibition.
- Design-seam guidance (`.claude/rules/powershell.md` §"Design Seams (Minimal DI)"): introduce the smallest seam that enables reliable mocking, preferring a wrapper-function seam only where an external executable call exists; adapter seams for filesystem/environment/clock. A pure path-filter predicate (no I/O) does not require any of these seam categories — it is directly testable by mocking `Get-ChildItem`'s return shape, as already demonstrated.

## 5. Sufficiency of the directed fix (Q7 answer)

The fix as directed — compute `[System.IO.Path]::GetRelativePath($resolvedSearchRoot, $candidatePath)` and apply the `\.claude\` `-notmatch` clause to that relative string instead of `$_.FullName` — is **sufficient by itself** to be both correct and independently Pester-testable, because:

1. `Invoke-MSTestWithCoverageMain` is already a directly callable, fully mockable function (§3.3), and the existing test suite already exercises this exact discovery block through it with real regression coverage (§3.2).
2. No new external dependency or I/O boundary is introduced by switching from `.FullName` to a `GetRelativePath(...)`-derived string; `GetRelativePath` is a pure, deterministic static method with no seam requirement under the repo's "Design Seams (Minimal DI)" policy.
3. `$resolvedSearchRoot` is already available as a local variable in scope at the point of the `Where-Object` (line 272), requiring no plumbing changes to reach it from inside the pipeline's script block.

The **only mandatory non-production-code work** is adding the two new regression cases named in the issue (self-exclusion negative case; nested-sibling-worktree positive-exclusion case), and, per §3.4, ensuring they land in a file that will not push `Invoke-MSTest.RunSettings.Tests.ps1` over the repo's 500-line test-file cap — most cleanly by adding a new sibling test file rather than extending the existing one further.

## 6. Candidate approaches

**Approach A — in-place predicate edit only (no extraction).** Change line 301 in place: introduce a local variable (e.g. `$relativeCandidatePath = [System.IO.Path]::GetRelativePath($resolvedSearchRoot, $_.FullName)`) is not directly usable inside a `Where-Object { }` script block without recomputing per item, so the fix would compute the relative path inline in the predicate: `([System.IO.Path]::GetRelativePath($resolvedSearchRoot, $_.FullName)) -notmatch '\\\.claude\\'`. No new function; the existing `Describe 'Invoke-MSTestWithCoverageMain'` mock harness already exercises it (§3.3).
- Advantages: smallest possible diff; matches the issue's own directed fix wording exactly; no risk of behavior drift from a refactor; consistent with the #733 research note that this block was "already inside a testable function — no extraction needed."
- Limitations: keeps the `Invoke-MSTestWithCoverage.ps1` discovery pipeline inline and un-factored, unlike the sibling `Invoke-MSTest.ps1`'s `Get-MSTestAssemblyPathList`, so there remains no single pure function whose relative-path logic can be unit-tested in complete isolation from `Invoke-MSTestWithCoverageMain`'s other mocks (vswhere, dotnet-coverage, etc.); every test of the predicate must still route through the full main-function mock harness.

**Approach B — extract a small pure filter function (e.g. `Test-DiscoveredAssemblyPath` or fold the whole pipeline into a `Get-CoverageTestAssemblyPathList` mirroring `Get-MSTestAssemblyPathList`), then call it from `Invoke-MSTestWithCoverageMain`.**
- Advantages: consistency with the sibling script's established pattern; enables a dedicated, narrowly-scoped `Describe` block that tests the relative-path predicate in complete isolation (mocking only `Get-ChildItem`, no `vswhere`/`dotnet-coverage`/`Get-Content` mocks needed); slightly reduces `Invoke-MSTestWithCoverageMain`'s size.
- Limitations: a larger diff than the issue's directed fix calls for; not required for testability (§3.3, §5); `.claude/rules/powershell.md` "Prohibited Behaviors" warns against "broad refactors across unrelated scripts or modules," and this function is the *only* call site (§1.2), so extraction buys isolation but not reuse.

**Recommendation:** Approach A. The issue's "Required fix direction" section states the redesign is "already decided by the reporter, not open for your redesign," directing exactly this in-place relative-path substitution. Section 3.3 and the #733 research artifact both independently confirm no extraction is needed for testability. Approach B remains available to the planner as a discretionary style improvement, not a requirement.

## 7. Behavior semantics (success/failure conditions)

- **Success condition (positive case, self-exclusion fix):** when `$resolvedSearchRoot` itself contains a `.claude\worktrees\agent-<id>\` path segment (i.e., the script's own checkout lives under `.claude/worktrees/`), test assemblies discovered directly beneath that root (with no *further* nested `.claude\worktrees\` segment past the root) must be included in `$testAssemblies`.
- **Success condition (regression, must not break):** when a candidate assembly's path contains a `.claude\worktrees\...` segment that appears *after* `$resolvedSearchRoot` (i.e., a sibling agent worktree nested beneath the search root, as in the existing test at lines 416–442), that assembly must still be excluded.
- **Failure condition (current bug):** `$_.FullName -notmatch '\\\.claude\\'` evaluated against the absolute path unconditionally excludes every candidate whenever the checkout's own path contains `.claude`, anywhere in the string — including at or before the search root — producing an empty `$testAssemblies` and the misleading `"No test assemblies found under '$resolvedSearchRoot' for configuration '$Configuration'. Build first."` error at line 306.
- **Ordering/edge cases to cover:**
  - Search root itself under `.claude\worktrees\agent-N\` with assemblies directly beneath it → must be found (new case).
  - Search root not under `.claude` at all, with a nested sibling worktree beneath it → must still exclude that sibling (existing case, must not regress).
  - Search root itself under `.claude\worktrees\agent-N\` **and** containing its own further-nested sibling worktree beneath it (e.g., a doubly-nested agent worktree) → the nested one must still be excluded, while the root-level assemblies are retained. This double-nested case is not currently covered by any existing test and is worth flagging to the planner as the most rigorous edge case for the relative-path fix, though the issue's own "Proposed Fix / Validation Ideas" only names the two simpler cases (§ "Unit coverage areas" in issue.md).

## 8. Requirements mapping (design sketch, non-binding)

No numeric acceptance-criterion count is proposed by this research (none is warranted by the issue), so the Numeric Derivation Evidence protocol does not apply.

Concrete file changes implied by Approach A (recommendation):
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1`: replace line 301's absolute-path match with a `GetRelativePath($resolvedSearchRoot, $_.FullName)`-based match, evaluated relative to the already-in-scope `$resolvedSearchRoot` variable (line 272).
- A new or extended Pester test file under `tests/scripts/vscode/` (recommend a new file per §3.4) adding, at minimum, the two `It` cases named in the issue's own proposed-fix section, exercised through `Invoke-MSTestWithCoverageMain` using the same `BeforeEach` mock pattern already established in `Invoke-MSTest.RunSettings.Tests.ps1` lines 346–373.
- No change to `scripts/vscode/Invoke-MSTest.ps1` (no `.claude` clause present there to begin with; out of scope per #733's own prior scoping decision, §1.4).
- No change to `Invoke-MSTestWithCoverage.Helpers.ps1`'s `Get-KoverageProjectAllowlist` (different mechanism, no matching defect present, §2).

## 9. Testing implications

- Test strategy: Pester v5.x, in the existing dot-source/AST-parse/mock pattern already used in `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`. No new seam or wrapper function is required for the recommended Approach A.
- No temp files; all fixtures are in-memory `[pscustomobject]` records for `Get-ChildItem` mocks, consistent with `.claude/rules/general-unit-test.md` and `.claude/rules/powershell.md`.
- Coverage/regression gate: per `.claude/rules/powershell.md`, line coverage must remain >= 85% (no branch-coverage gate for PowerShell); changed lines must not regress coverage. The predicate change touches one existing covered line; the new tests should keep it covered under both the positive (self-root) and negative (nested sibling) paths.
- Toolchain order for this change: PoshQC format → PoshQC analyze → Pester test (via the MCP commands named in `.claude/rules/powershell.md` §"Toolchain"), restarting from format on any failure or file change.
