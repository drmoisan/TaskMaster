# Code Review — invoke-mstestwithcoverage-threshold-before-setcontent (#565)

- Timestamp: 2026-09-03T12-15
- Scope: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`
  (full `origin/main...HEAD` diff, `b13d5b7b` merge-base)

## Summary

The change is a two-statement reorder inside `Invoke-MSTestWithCoverageMain`: `Set-Content`
(persisting the post-processed Cobertura XML) now runs immediately before
`Assert-CoberturaLineCoverageThreshold` (the threshold gate that can throw). Previously the
assertion ran first, so a failing (sub-threshold) run left the raw, un-post-processed
`dotnet-coverage` output on disk instead of the document the assertion actually judged. No logic,
parameter, or return-value change; no new function, class, or file.

```diff
-    Assert-CoberturaLineCoverageThreshold -CoberturaXml $processedXmlContent
-
     Set-Content -Path $resolvedOutputPath -Value $processedXmlContent -Encoding UTF8 -NoNewline
+
+    Assert-CoberturaLineCoverageThreshold -CoberturaXml $processedXmlContent
```

## Design Principles (`.claude/rules/general-code-change.md`)

- **Simplicity first**: the fix is the simplest possible resolution — a pure statement swap, no
  added indirection, no `try`/`finally` wrapper (the spec explicitly prohibits one, and the diff
  correctly avoids it). Assessed: compliant.
- **Separation of concerns**: unaffected — `Set-Content` (I/O) and `Assert-...Threshold` (pure
  read-and-throw) remain two independent statements with no new coupling.
- **Fail fast and explicitly**: preserved — `Assert-CoberturaLineCoverageThreshold` still throws
  under the identical three conditions (missing, non-numeric, out-of-range/sub-threshold
  line-rate) with unchanged message text, confirmed unmodified in
  `Invoke-MSTestWithCoverage.Threshold.ps1`.

## Correctness of the Fix

The reorder is behaviorally sound for the stated defect: `$processedXmlContent` is already fully
computed before either statement runs (both call orders read the identical string), and
`Assert-CoberturaLineCoverageThreshold` operates on its own local `[xml]` copy parsed from the
string argument — it has no side effect that `Set-Content` could observe, and vice versa. Swapping
the two statements is therefore safe: the passing-run behavior is unchanged (both statements
execute either way), and the failing-run artifact-on-disk is now the post-processed document
instead of the raw one. No case was found where the reorder could change behavior beyond the
stated fix.

## Test Quality (`.claude/rules/general-unit-test.md`, `.claude/rules/powershell.md`)

The new test (`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:416-422`):

```powershell
It 'persists the post-processed Cobertura document before the threshold assertion can throw on a sub-threshold run' {
    Mock ConvertTo-KoverageCoberturaXml { '<coverage line-rate="0.5" />' }

    { Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir } | Should -Throw

    Should -Invoke Set-Content -Times 1 -Exactly
}
```

- **Correctly proves ordering, not just presence**: `Assert-CoberturaLineCoverageThreshold` is
  deliberately left unmocked in this `It` (the `BeforeEach` never mocks it; only one other `It`
  block mocks it, and Pester mocks do not leak across `It`s), so it genuinely evaluates the
  sub-threshold fixture and throws. If the statement order regressed to pre-fix, the throw would
  occur before `Set-Content` is reached and the `Should -Invoke -Times 1 -Exactly` assertion would
  fail with "called 0 times" — exactly what the `expect-fail-run` evidence artifact demonstrates.
  This is a load-bearing regression test, not a tautology.
- **Independence/isolation**: follows the file's existing `BeforeEach` mocking conventions
  (Resolve-Path, Test-Path, Invoke-VsWhereExe, Get-Command, Get-ChildItem,
  Invoke-DotnetCoverageCollection, Get-Content, ConvertTo-KoverageCoberturaXml, Set-Content all
  mocked); no external process, network, or filesystem dependency; no temp files.
- **Determinism**: fixture is a fixed literal XML string; no randomness or wall-clock dependency.
- **Naming/documentation**: the `It` name states the scenario and expected outcome in full
  sentence form, consistent with the surrounding `Describe` block's style.
- **Correct mock-parameter targeting**: the spec called out that `Set-Content` in this file is
  invoked with `-Path` (not `-LiteralPath`) — `Should -Invoke Set-Content -Times 1 -Exactly` with
  no parameter filter is a valid, unambiguous assertion here because the mock intercepts the
  cmdlet regardless of which path parameter is used; no filter mismatch risk.

One observation, non-blocking: the assertion only checks that `Set-Content` was invoked, not that
it was invoked with the pre-throw value of `$processedXmlContent` specifically (i.e., it does not
assert on the argument value). This is acceptable because `ConvertTo-KoverageCoberturaXml` is
mocked to a single fixed return value in this test, so there is no other value `Set-Content` could
plausibly have been called with; a stricter `-ParameterFilter` would add no discriminating power
here.

## Test Insertion Point

The new test is inserted between the pre-existing `It 'fails when the search root cannot be
found'` and the #733-added `It 'excludes assemblies discovered under a .claude worktree segment'`,
matching the plan's primary anchor. Independently confirmed via
`git diff origin/main...HEAD -- tests/.../Invoke-MSTest.RunSettings.Tests.ps1`: a single
insertion-only hunk, no other test's body touched. The #733 test's mocks and assertions are
identical before and after this insertion.

## Style / Formatting / Linting

- PoshQC format: 0 auto-fixes on both files (`git status --porcelain` empty before and after).
- PSScriptAnalyzer (via `run_poshqc_analyze`): 0 diagnostics on both files.
- No new PSScriptAnalyzer suppressions introduced.

## File Size

`Invoke-MSTestWithCoverage.ps1`: 350 lines (well under 500). `Invoke-MSTest.RunSettings.Tests.ps1`:
496 lines — under the 500-line cap but only 4 lines of headroom remain. This is not a defect
introduced by this PR (the file was already large pre-branch, and #733 added several tests to it
independently of this fix), but future additions to this specific test file should consider
splitting it, since the next similarly-sized addition would exceed the limit.

## Naming, Comments, Documentation

No new identifiers were introduced. The existing block comment above the reordered statements
(`# Post-process the Cobertura XML for Koverage compatibility: ...`) still accurately describes
the three-step post-processing pipeline and required no update, since the reorder does not change
what the three enumerated steps do — only when the threshold check runs relative to persistence.

## Findings

No blocking findings. No non-blocking code-quality findings beyond the file-size headroom note
above (informational only).
