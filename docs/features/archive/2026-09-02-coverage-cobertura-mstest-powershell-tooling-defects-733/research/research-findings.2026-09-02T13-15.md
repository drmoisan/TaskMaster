# Research Findings — Issue #733 (coverage-cobertura-mstest-powershell-tooling-defects)

## 1. Current State Analysis

### Files read in full (current `origin/main`-derived worktree state)
- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (492 lines)
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (350 lines)
- `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` (390 lines)
- `scripts/vscode/Invoke-MSTest.ps1` (132 lines)
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` (498 lines)
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` (443 lines)
- `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` (459 lines)
- `.claude/rules/powershell.md`

### Key abstractions and existing rate-computation pattern
- `Get-CoberturaClassLineSummary` (Helpers.ps1:162-260) is the **only** pure, reusable per-class line/branch summarizer. It de-duplicates a class's `<lines>` rollup against its `<methods>/<method>/<lines>` view, keyed by line number, resolving collisions by max-hits / branch-OR / richest-condition-coverage.
- `Get-CoberturaCoverageSummary` (Helpers.ps1:99-139) computes the **document-level** (root `<coverage>`) rates: it walks `//packages` → every `<package>` → every `<class>` via `Get-CoberturaClassLineSummary`, accumulating totals across **all** packages, then converts to a rate with `[math]::Round(covered/total, 6)` and a `'0'` string fallback on a zero denominator. **No existing helper computes a rate scoped to one `<package>`.**
- `Merge-CoberturaClassesByFilename` (Helpers.ps1:262-391) merges `<class>` nodes that share a `filename` within one `<package>`. It sets the merged class's own `line-rate`/`branch-rate` (Helpers.ps1:371-375) by duplicating the same rounding expression as `Get-CoberturaCoverageSummary`, with an explicit code comment (Helpers.ps1:367-370) stating this duplication is deliberate because "the spec specifies exactly one new helper" — i.e., a prior work item intentionally avoided adding a second helper for the merged-class rate. That constraint does not extend to a package-scoped helper, which is a different aggregation (across all classes in a package, not one merged class).
- `Invoke-MSTestWithCoverageMain` (Invoke-MSTestWithCoverage.ps1:248-345) is a **testable wrapper function** around the whole coverage pipeline (discovery → collect → post-process). Its assembly-discovery block (lines 296-302) is **already wrapped in `@(...)`**: `$testAssemblies = @(Get-ChildItem ... | Where-Object {...} | Select-Object -ExpandProperty FullName)`. This confirms the issue's own instruction: finding 7's fix does **not** apply to this script, only to `Invoke-MSTest.ps1`.
- `Invoke-MSTest.ps1` has **no such wrapper function** — its body (lines 80-131) is bare top-level script code that runs unconditionally on dot-source or direct invocation (there is no `$MyInvocation.InvocationName -ne '.'` guard, unlike the coverage script). Its discovery block (lines 107-113) is **not** wrapped in `@(...)`, confirming finding 7 as stated. Its `vswhere.exe` invocation (line 102, `& $vswherePath -latest ...`) is also called **directly**, not through a mockable wrapper — unlike the coverage script, which has `Invoke-VsWhereExe` specifically for testability. This is a structural asymmetry between the two scripts (not one of the seven findings, noted here only because it affects test-strategy design for finding 7, below).
- `ClosureFilter.ps1`'s `Get-CoberturaInstrumentedMemberName` (lines 134-209) builds a presence hashtable keyed by `"$declaringType|$filename"` → `HashSet<string>` of **bare member names**, admitted from two sources: (1) plain `<method name="X">` on a non-synthesized class where X doesn't start with `<`; (2) the `<Member>` token parsed from an async/iterator state-machine class name `Type.<Member>d__<N>`. `<Member>g__Local|N_M` (local functions) are explicitly and deliberately **not** admitted (documented rationale at lines 154-157).
- `Get-CoberturaClosureDeclaringMemberName` (lines 38-97) is the **consumer-side** regex lookup used when walking closure classes: it recovers only a bare member-name token from four Roslyn name shapes (`<M>b__...`, `<M>g__Local|N_M`, `Type.<M>d__N`, `...<<M>b__K>d`). **None of its capture groups ever recover a parameter signature or count** — Roslyn's closure/lambda naming convention does not encode the enclosing member's signature.

## 2. Existing Pester Test Conventions (verified by reading both files in full)

- Both existing Helper/ClosureFilter test files dot-source the production script directly in `BeforeAll` (`. $helperScriptPath`) — no module manifest, no `Import-Module`.
- Cobertura fixtures are inline here-strings (`[xml]$doc = @'...'@` or `$inputXml = @'...'@` passed to `ConvertTo-KoverageCoberturaXml`), always minimal, single-purpose, and heavily commented with an explicit "Regression case N (Issue #NNN, ...)" note tying the fixture to the specific direction of the defect it pins.
- Assertions use FluentAssertions-style Pester `Should` (this is native Pester `Should`, not a C# FluentAssertions port — the repo's C# assertion-library rule does not apply to PowerShell).
- `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` is the **only** existing test file that imports **both** `Invoke-MSTest.ps1` (via `. $script:mstestScript -NoExecute`) and `Invoke-MSTestWithCoverage.ps1` (via `[System.Management.Automation.Language.Parser]::ParseFile(...).GetScriptBlock()`, dot-sourced, because the coverage script's own trailing invocation guard would otherwise run `Invoke-MSTestWithCoverageMain` for real). It already contains a `Describe 'Invoke-MSTestWithCoverageMain'` block (lines 345-414) that mocks `Resolve-Path`, `Test-Path`, `Resolve-RunSettingsPath`, `Invoke-VsWhereExe`, `Get-Command`, `Get-ChildItem`, `Invoke-DotnetCoverageCollection`, `Get-Content`, `ConvertTo-KoverageCoberturaXml`, `Set-Content`, and calls `Invoke-MSTestWithCoverageMain` directly with `-NoExecute`/`-ScriptRoot`. This is the natural, already-proven scaffold for finding 3's regression test.
- No file currently named `Invoke-MSTestWithCoverage.Tests.ps1` or `Invoke-MSTest.Tests.ps1` exists. Given the discovery above, **new dedicated files are not required** for findings 3 or 7 — `Invoke-MSTest.RunSettings.Tests.ps1` already dot-sources both target scripts and already exercises the exact function (`Invoke-MSTestWithCoverageMain`) or exact top-level body (`Invoke-MSTest.ps1`) that findings 3 and 7 touch. Extending this existing file, rather than creating new ones, follows the file's own established, working pattern and avoids duplicating the mock/import scaffolding.

## 3. Per-Finding Fix Proposals

### Finding 1 — package-level `line-rate`/`branch-rate` never recomputed
**Function/location:** `Merge-CoberturaClassesByFilename`, `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (merge loop ~262-391).

No existing helper computes a rate scoped to a single `<package>` — `Get-CoberturaClassLineSummary` is class-scoped and `Get-CoberturaCoverageSummary` is document-scoped (sums over *all* packages). A new small pure helper is needed. Recommended shape, placed beside `Get-CoberturaCoverageSummary` in Helpers.ps1:

```
function Get-CoberturaPackageLineSummary {
    param([Parameter(Mandatory = $true)][System.Xml.XmlElement]$PackageNode)
    # Same accumulation loop as Get-CoberturaCoverageSummary's inner `foreach ($cls in ...)`,
    # scoped to one <package> via $PackageNode.SelectNodes('.//class'), returning the same
    # pscustomobject shape (LineRate, BranchRate, LinesCovered, LinesValid, BranchesCovered, BranchesValid).
}
```

`Get-CoberturaCoverageSummary` should then be refactored to call this new helper once per package and sum its outputs — this removes the current inline duplication and gives the new package-level helper the same "already proven by the document-level totals" trust the class-level helper has. In `Merge-CoberturaClassesByFilename`, after the inner `foreach ($filename in $filenameGroups.Keys)` loop finishes for a given `$packageNode` (i.e., immediately before the outer `foreach ($packageNode in ...)` closing brace), call the new helper on `$packageNode` and `SetAttribute('line-rate', ...)` / `SetAttribute('branch-rate', ...)` on the package node itself, mirroring the exact rounding/zero-fallback expression already used for both class-level and document-level rates.

**Test target:** `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` — add a new `Describe 'Get-CoberturaPackageLineSummary'` block (mirroring the existing `Describe 'Get-CoberturaClassLineSummary'` block's style), plus extend the existing `ConvertTo-KoverageCoberturaXml` merge tests (e.g. the "merges duplicate class entries..." or "computes the merged per-file line-rate..." fixtures) with an assertion on the resulting `<package>` node's `line-rate`/`branch-rate` attributes.

### Finding 2 — merged class drops non-primary group members' `<methods>`
**Function/location:** `Merge-CoberturaClassesByFilename`, same file, lines 295-301 (methods-node handling) inside the per-filename-group loop.

Fix: after ensuring `$methodsNode` exists, iterate the **other** members of `$group` (every class node except `$primaryNode`, whose subtree — including its own `<methods>` — is already present via `CloneNode($true)`) and append a deep clone of each `./methods/method` child into `$methodsNode`:

```
foreach ($classNode in $group) {
    if ($classNode -eq $primaryNode) { continue }
    foreach ($methodNode in @($classNode.SelectNodes('./methods/method'))) {
        [void]$methodsNode.AppendChild($methodNode.CloneNode($true))
    }
}
```

No dedup key is proposed: Roslyn generates distinct method-name tokens per closure/lambda/local-function/state-machine, so two different group members (declaring class + its `<>c`/`<>c__DisplayClassN_M` closures) cannot legitimately share an identical `<method name=...>` value in the same filename group under normal compiler output. This assumption should be verified by the atomic-plan/test author with a 3+-way merge fixture (declaring class + two distinct closure classes, each contributing a differently-named method) rather than assumed silently.

**Materially important:** this fix will **change the outcome of an existing, currently-passing regression test.** `Invoke-MSTestWithCoverage.Helpers.Tests.ps1`'s test `'preserves the primary class methods subtree and every hits value when merging'` (lines 316-349) contains the comment *"Locks the decision not to merge or strip `<methods>`."* and asserts `$methodNodes.Count | Should -Be 1` with only method `'M'` present — i.e., it currently asserts, as an intentional prior design decision, exactly the behavior finding 2 identifies as a defect (dropping the closure class's method `'N'`). Fixing finding 2 requires **updating this existing test's assertions** (methodNodes.Count should become 2, containing both `'M'` and `'N'`), not merely adding a new one. Per CLAUDE.md §7.3 ("Treat existing unit tests as part of the spec"), this reversal of a previously locked-in decision should be called out explicitly to the downstream `prd-feature`/atomic-planner authors as a deliberate, spec-approved behavior change, not a silent edit.

**Test target:** same file — modify the existing test above, and add a new isolated test for a 3-member merge group exercising the union/no-dedup-collision case.

### Finding 3 — no `.claude\` exclusion in `Invoke-MSTestWithCoverage.ps1` discovery filter
**Function/location:** `Invoke-MSTestWithCoverageMain`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, lines 296-302 (already inside a testable function — no extraction needed).

Fix: add a fourth `-and` clause to the existing `Where-Object` predicate: `-and $_.FullName -notmatch '\\\.claude\\'` (backslash-escaped literal dot, consistent with the existing `\\bin\\`, `\\obj\\`, `\\ref\\` clauses' style).

Note (informational only, not proposed for this issue): `Invoke-MSTest.ps1`'s own discovery block (lines 107-113) has the identical unfiltered shape and would benefit from the same clause for parity, but finding 3 as scoped in the issue names only `Invoke-MSTestWithCoverage.ps1`; expanding to the sibling script would be a scope decision for the `prd-feature`/planning stage, not this research.

**Test target:** `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, extending the existing `Describe 'Invoke-MSTestWithCoverageMain'` block. Add an `It` that mocks `Get-ChildItem` to return two items — one ordinary `...\bin\Debug\Foo.Test.dll` path and one under `...\.claude\worktrees\...\bin\Debug\Bar.Test.dll` — and mocks `Invoke-DotnetCoverageCollection` to capture its `-TestAssembly` parameter (same capture pattern already used for `Invoke-DotnetCoverageExe`/`Invoke-VsWhereExe` elsewhere in this file), asserting only the non-`.claude` path is forwarded.

### Finding 4 — no dedicated fixture for the "second-seen strictly higher hits" merge branch
**Function/location:** `Merge-CoberturaClassesByFilename`, line 329 (`SetAttribute('hits', Max(...))`).

**Conclusion: this is a test-only gap, not a code defect**, but the existing coverage is less direct than it first appears and should be clarified rather than assumed adequate:
- The existing focused test `'deduplicates a repeated line number by taking the maximum hits value'` (Helpers.Tests.ps1:273-294) exercises `Get-CoberturaClassLineSummary`'s **own**, separately-implemented max-hits logic (plain property assignment `$existing.Hits = $hits`, not `SetAttribute`) — its fixture has only one `<class>` element, so `Merge-CoberturaClassesByFilename`'s per-group merge is skipped entirely (`$group.Count -le 1` guard, line 286-288) for that fixture. It does not exercise line 329 at all.
- The multi-purpose test `'merges duplicate class entries that point to the same source file'` (Helpers.Tests.ps1:53-95) **does** incidentally exercise line 329 in the exact "second-seen strictly higher" direction (class1's line 11 hits=0 is first-seen in `$group` document order; class2's line 11 hits=1 is second-seen and higher), and asserts `$line11.hits | Should -Be '1'`. However this fixture conflates that assertion with several unrelated behaviors (path normalization, branch promotion, condition-coverage, complexity summation), so it is not an isolated, single-purpose regression pin per the repo's own testing standard ("Write focused tests exercising a single function or behavior," `.claude/rules/powershell.md`).

Recommendation: no production code change; add one new, minimal, focused fixture to `Helpers.Tests.ps1` with exactly two classes sharing a filename, exactly one overlapping line number, and only the hits value varying (second class strictly higher), asserting the merged line's `hits` attribute. This closes the audit gap identified by the static review without touching production code.

### Finding 5 — local-function exclusion policy documented but unratified
**Function/location:** `Get-CoberturaInstrumentedMemberName`, `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`, lines 154-157 (doc comment) / 194 (enforcement — the `foreach` over `./methods/method` only calls `.Add($methodName)` when the class is non-synthesized, which structurally cannot include a `g__` local-function name emitted on the declaring type's own class without deliberately special-casing it, which the code does not do).

**Conclusion: no code change required.** Evidence:
- The current behavior is already pinned by an existing, passing test: `ClosureFilter.Tests.ps1`'s `'removes a closure class outright when every method resolves to an absent member'` (lines 152-190), Part B, whose comment explicitly states *"a `g__` local function on the declaring type does not admit 'Exempt'."*
- Direction-of-failure analysis: not admitting `g__` tokens pushes outcomes toward **exclusion** (dropping coverage) whenever a closure's declaring member resolves only via a local-function token. For a genuinely `[ExcludeFromCodeCoverage]`-attributed outer member, this is the correct, desired outcome. A counter-example where this would cause **over-exclusion** (the one forbidden failure direction per the function's own documented fail-safe invariant) requires a non-exempt method that emits **only** a `g__` local-function method entry and no plain top-level `<method>` element of its own — no such counter-example was found or constructed during this research, and none is cited in the issue.
- The issue's own framing agrees: it is flagged as a policy whose correctness is "unverified," not as a demonstrated behavior defect, and explicitly does not request a policy change.

Recommendation: treat as a documentation clarification only. Add a short addendum to the existing docstring (lines 154-157) noting that the exclusion is an asserted design choice, not independently verified against a live counter-example, and that it should be revisited if a genuine non-exempt-method-with-only-a-`g__`-entry case is ever observed. No new test is strictly required (existing Part B already pins current behavior), though a one-line comment cross-referencing issue #733 on the existing test would help future readers understand why the policy exists.

### Finding 6 — presence-set keyed by bare member name, not full signature
**Function/location:** `Get-CoberturaInstrumentedMemberName`, same file, lines 178-205 (presence-set construction, `HashSet[string]` of bare names).

**Data-availability analysis (required before proposing any re-keying):**
- **Producer side** (`Get-CoberturaInstrumentedMemberName`, source 1): a `signature` XML attribute **is** present on real Cobertura `<method>` elements (confirmed directly in existing test fixtures, e.g. `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` line 281: `<method name=".ctor" signature="()" ...>` and `<method name=".ctor" signature="(int)" ...>`). This attribute is not read by the current code, so signature-based keying is technically available on this side without new regex parsing.
- **Producer side, source 2** (async/iterator state-machine class names, `Type.<Member>d__<N>`): no signature is available at all — this shape is derived purely from a class-name regex match and carries no parameter information.
- **Consumer side** (`Get-CoberturaClosureDeclaringMemberName`, used by `Remove-CoberturaExemptClosureCoverage` to resolve which declaring member a closure belongs to): **none** of its four regex capture groups (`^<(?<m>...)>b__`, `^<(?<m>...)>g__`, `<<(?<m>...)>b__\d+>d`, `<(?<m>...)>d__\d+`) ever recover a parameter signature or count. Roslyn's closure/lambda/local-function naming convention encodes only the enclosing member's *name*, never its signature.

**Conclusion: a signature-based re-key is not achievable with the data actually available**, because even if the producer-side presence set were keyed by `"name|signature"`, the consumer-side lookup can only ever supply a bare name — `.Contains()` would then never match anything for **any** member (not just overloaded ones), flipping the outcome from the current "collision causes wrong retention" to "every lookup fails, causing mass over-exclusion of every closure in the file." That would violate the function's own explicit, documented fail-safe invariant ("over-exclusion is not an acceptable failure mode... every failure mode of the key is in the under-exclusion direction"), which is a materially worse regression than the defect finding 6 describes.

**Direction-of-failure analysis for the current (bare-name) behavior:** because `[ExcludeFromCodeCoverage]`-attributed members emit no `<method>` element at all, a name collision between an exempt overload and a non-exempt overload of the same name causes the presence set to contain the name (from the non-exempt overload) — so the exempt overload's closures are wrongly **retained** (kept in the denominator, permanently uncovered) rather than wrongly **excluded**. This is the safe, already-fail-safe direction ("a file measures no better than it truly is"), not the forbidden over-exclusion direction. It is a real accuracy defect (unfairly penalizes coverage percentage for an exempt overload) but not a correctness-hiding one.

Recommendation: do not attempt a functional re-keying fix (infeasible given available data on the consumer side, and any attempt risks flipping the fail-safe direction). Instead: (a) document this residual limitation directly in the `Get-CoberturaInstrumentedMemberName` docstring (mirroring the existing style used for the local-function exclusion note), explicitly naming the direction of the effect (safe/under-exclusion, not over-exclusion) so a future reader does not attempt an unsafe fix; (b) add one focused pinning regression test demonstrating the current, documented, safe-direction outcome for a same-name-overload collision (one exempt overload, one non-exempt overload, same declaring type/file) so the behavior cannot silently drift in the unsafe direction without a test failing.

**This conclusion materially contradicts spec.md's current seeded test-strategy line** ("re-key the presence set by full member signature instead of bare name" — spec.md Test Strategy, item 3). The `prd-feature`/atomic-planner stage should be made aware of this before finalizing acceptance criteria, since the seeded approach is not implementable with the data available in the Cobertura report as currently consumed by this script.

### Finding 7 — `Invoke-MSTest.ps1` unwrapped discovery pipeline throws under StrictMode
**Function/location:** `Invoke-MSTest.ps1`, lines 107-113 (top-level script body, no wrapper function).

Fix (minimal): wrap the pipeline in `@(...)`, exactly matching the pattern already used in the sibling script:
```
$testAssemblies = @(Get-ChildItem -Path $resolvedSearchRoot -Recurse -Filter '*.Test.dll' |
    Where-Object { ... } |
    Select-Object -ExpandProperty FullName)
```

**Testability constraint discovered:** unlike `Invoke-MSTestWithCoverage.ps1`, this script's discovery block is bare top-level code, not inside a callable function, and its `vswhere.exe` invocation (line 102) is a direct `&` call with no mockable wrapper (unlike the coverage script's `Invoke-VsWhereExe`). A test that dot-sources the *entire* script to reach line 115 would need `Test-Path` to return `$true` for the vswhere-exists check (line 98) in order to proceed to discovery, which then causes the real, unmocked `& $vswherePath ...` call at line 102 to execute for real — there is no way to intercept it via `Mock` alone, since `Mock` matches by command name and the invocation target is resolved from a variable at runtime, not a literal command name Pester can bind to.

To make finding 7 both fixed and reliably regression-tested without expanding scope into a broader script refactor, extract only the discovery-and-count logic (lines 107-117) into a small, testable function, following this file's own established wrapper-function pattern (`Get-VsTestArgumentList`, `Invoke-VsTestExe`) and the repo's documented Design Seams guidance ("introduce the smallest seam that enables reliable mocking," `.claude/rules/powershell.md`):

```
function Get-MSTestAssemblyPathList {
    param(
        [Parameter(Mandatory = $true)][string]$SearchRoot,
        [Parameter(Mandatory = $true)][string]$Configuration
    )
    return @(Get-ChildItem -Path $SearchRoot -Recurse -Filter '*.Test.dll' |
        Where-Object {
            $_.FullName -match "\\bin\\$Configuration\\" -and
            $_.FullName -notmatch '\\obj\\' -and
            $_.FullName -notmatch '\\ref\\'
        } |
        Select-Object -ExpandProperty FullName)
}
```
with line 107 replaced by `$testAssemblies = Get-MSTestAssemblyPathList -SearchRoot $resolvedSearchRoot -Configuration $Configuration`. This keeps the fix itself minimal (the `@(...)` wrap) while making it directly and deterministically testable in isolation (mock `Get-ChildItem` to return exactly one item; assert the returned array's `.Count` is `1` without throwing), rather than requiring a fragile whole-script dot-source with an unmockable external-executable call in the path. This mirrors, at file-appropriate scale, the same testability the coverage script already has for its own (already-correct) discovery block.

**Test target:** `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, which already dot-sources `Invoke-MSTest.ps1`'s definitions in `BeforeAll`. Add a new `Describe 'Get-MSTestAssemblyPathList'` block with `It`s for: zero matches, exactly one match (the StrictMode regression case — must not throw and `.Count` must equal `1`), and multiple matches, using `Mock Get-ChildItem`.

## 4. Toolchain Commands (verified against `.claude/rules/powershell.md`)

1. **Format:** PoshQC via MCP `mcp__drm-copilot__run_poshqc_format` (Invoke-Formatter under the hood; do not substitute VS Code task wrappers).
2. **Lint:** PoshQC analyzer via MCP `mcp__drm-copilot__run_poshqc_analyze` (PSScriptAnalyzer with repo settings); optional autofix `mcp__drm-copilot__run_poshqc_analyze_autofix`.
3. **Type-check:** not applicable for PowerShell — skip directly to testing.
4. **Test:** Pester v5.x via MCP `mcp__drm-copilot__run_poshqc_test`, using repo config `scripts/powershell/PoshQC/settings/pester.runsettings.psd1`.

Run format → analyze → test, in that order; restart from step 1 if any step fails or changes files. This is the CLAUDE.md-mandated toolchain order (formatting → linting → type-checking → testing) applied to the PowerShell-specific tools; no separate architecture-boundary/contract/integration stages apply to these scripts.

## 5. Scope Compliance Notes

- No numeric coverage threshold, CI gate, or `Assert-CoberturaLineCoverageThreshold` value change is proposed anywhere above (per the binding scope constraint).
- Finding 3's `.claude\` exclusion is treated strictly as a discovery-filter change, not a threshold change, per the issue's own framing.
- All proposed test files are under `tests/scripts/vscode/`, mirroring `scripts/vscode/` per repo convention; no new test file paths outside that tree are proposed.
- No change is proposed to any file under `.claude/**`, a Codex mirror tree, a dot-agents tree, or `config/blast-radius.json` / `config/orchestration-routing.json`.
- The `## Numeric Derivation Evidence` protocol does not apply to this research: no proposal above asserts a numeric count, enumeration, or population figure requiring exhaustive-family derivation.
