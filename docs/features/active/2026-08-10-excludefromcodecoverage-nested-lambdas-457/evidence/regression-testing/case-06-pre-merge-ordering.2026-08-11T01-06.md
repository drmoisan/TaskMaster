# Regression case 6 — pre-merge ordering (end-to-end)

Timestamp: 2026-08-11T01-06
Task: `[P1-T10]` `[expect-fail]`
Test file: `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` (extended, not created)
Fully-qualified Pester name:
`ConvertTo-KoverageCoberturaXml.removes exempt closure lines before the filename merge collapses the closure class`

## Fixture (verbatim, inline here-string)

The case-1 shape, driven end-to-end through `ConvertTo-KoverageCoberturaXml` with `-RepoRoot`, an
explicit `-ProjectNames` list so the assertion does not depend on the production allowlist, and an
explicit `-PathSeparator '\'` so no fixture depends on the host `DirectorySeparatorChar`.

```xml
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
        <class name="Ns.T" filename="C:\repo\Ns\T.cs" line-rate="0" branch-rate="0" complexity="1"><methods><method name="Visible" signature="()" line-rate="0" branch-rate="0"><lines><line number="10" hits="1" branch="False" /><line number="11" hits="1" branch="False" /></lines></method></methods><lines><line number="10" hits="1" branch="False" /><line number="11" hits="1" branch="False" /></lines></class>
        <class name="Ns.T.&lt;&gt;c__DisplayClass41_0" filename="C:\repo\Ns\T.cs" line-rate="0" branch-rate="0" complexity="1"><methods><method name="&lt;Exempt&gt;b__0" signature="()" line-rate="0" branch-rate="0"><lines><line number="406" hits="0" branch="False" /><line number="409" hits="0" branch="False" /></lines></method></methods><lines><line number="406" hits="0" branch="False" /><line number="409" hits="0" branch="False" /></lines></class>
      </classes></package></packages>
</coverage>
```

## Assertion (verbatim)

```powershell
[xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\' -ProjectNames @('Ns')
$merged = @($resultXml.SelectNodes('//class[@filename="Ns\T.cs"]'))

$merged.Count | Should -Be 1
((@($merged[0].SelectNodes('./lines/line')) | ForEach-Object { $_.number }) -join ',') | Should -Be '10,11'
$resultXml.coverage.'lines-valid' | Should -Be '2'
```

The single merged `<class>` for that filename must contain none of the exempt closure lines, and the
document `lines-valid` must count only the declaring member's two lines.

## Why this is the ordering constraint's regression guard

`Merge-CoberturaClassesByFilename` groups `<class>` elements by `filename`, selects as primary the
first member whose `name` does not match `<`, unions the group's class-level `<lines>`, and keeps only
the primary's `<methods>`. A closure type always shares its declaring type's `filename`, so the merge
always collapses it and the surviving node is named `Ns.T`, carries no `.<>c` marker, and no longer
contains the `<Exempt>b__0` method the filter resolves against. A filter placed after the merge is
therefore not merely worse — it is a no-op. This is corroborated independently by the `[P0-T1]`
blended-denominator check, which measured the merge retaining the closure's lines in the merged
class-level `<lines>`.

## Observed pre-implementation failure

EXIT_CODE: 1 (file result: Passed=19, Failed=1)

```
FAIL: removes exempt closure lines before the filename merge collapses the closure class
Expected strings to be the same, but they were different.
Expected length: 5
Actual length:   13
Strings differ at index 5.
Expected: '10,11'
But was:  '10,11,406,409'
           -----^
```

This is the expected `[expect-fail]` reason for case 6 specifically: an **assertion failure showing
the exempt closure lines still present in the merged class**, not a `CommandNotFoundException`.
`ConvertTo-KoverageCoberturaXml` already exists and merely does not yet call the filter, so the
pipeline runs to completion and the merged rollup contains `10,11,406,409`.

## File size

`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`: 468 lines before, **490** lines
after, against the 500-line ceiling — 10 lines of headroom.

A first draft of this test measured 499 lines, leaving 1 line of headroom, which would have made
`[P3-T10]`'s post-format ceiling check fragile. The test was compacted before this artifact was
written — the comment block reduced from 5 to 4 lines, the XML declaration removed, each `<class>`
placed on a single line (matching the compact fixture style already used by the #441 tests in this
file), and the intermediate `$mergedLines` variable inlined. No assertion was weakened or removed;
the assertion text above is the compacted form and is byte-identical to what is in the file. The plan
authorizes no split of this file, so creating headroom inside the new test was the available remedy.

## Passing result

Recorded by `[P3-T1]` (`<FEATURE>/evidence/regression-testing/pass-after-run.2026-08-11T01-30.md`).

- Timestamp: 2026-08-11T01-30
- Result: **Passed** (9ms)
- Test: `ConvertTo-KoverageCoberturaXml.removes exempt closure lines before the filename merge collapses the closure class`
- Run EXIT_CODE: 0 (Passed=29, Failed=0, Skipped=0 across both test files)
