# P6-T9 C# coverage projection and comparison (AC13, D10)

## iter1

Timestamp: 2026-09-29T22-23
Command: the second MSTEST-COVERAGE payload (Gate command reference), which printed the first-party coverage line from coverage/logs/927-mstest.log and the package-level projection coverage/coverage.cobertura.jacoco.xml, after the first payload recorded in csharp-toolchain-pass.md.
EXIT_CODE: 0
Output Summary:
- First-party coverage: lines 56479/65736 (85.92%), branches 13656/17054 (80.08%)
- Line percent 85.92 against the P0-T14 baseline 85.93: NOT MET (0.01 below).
- Branch percent 80.08 against the P0-T14 baseline 80.09: NOT MET (0.01 below).
- Denominators comparable under D10: lines-valid 65736 against 65737 (difference 1, below one percent of the baseline); branches-valid 17054 against 17052 (difference 2).
- AC13 coverage clause: NOT MET.

First-party coverage line (verbatim):

```text
First-party coverage: lines 56479/65736 (85.92%), branches 13656/17054 (80.08%)
```

Comparison table:

| Figure | Baseline (P0-T14) | Final (P6-T9 iter1) | Result |
|---|---|---|---|
| Line percent | 85.93 | 85.92 | NOT MET (below baseline) |
| Branch percent | 80.09 | 80.08 | NOT MET (below baseline) |
| Lines valid | 65737 | 65736 | comparable (difference 1) |
| Branches valid | 17052 | 17054 | comparable (difference 2) |
| Lines covered | 56486 | 56479 | 7 fewer |
| Branches covered | 13657 | 13656 | 1 fewer |

Comparability sentence (D10): the two lines-valid figures differ by 1 line, which is less than one percent of the baseline, so the denominators are comparable and the not-below comparison applies.

Observations recorded without a conclusion drawn from them:
- The whole difference sits in the UtilitiesCS package (LINE covered 39217 to 39210, missed 4207 to 4213; BRANCH covered 9473 to 9472, missed 1796 to 1799). Every other package's counters are identical to the baseline projection.
- This branch changes no production C# file (P6-T14 PROD-CS gate). The merge of origin/main ddbab26a0 added only QuickFiler.Test test files and documents. The UtilitiesCS denominator nonetheless moved (lines 43424 to 43423, branches 11269 to 11271), with no production UtilitiesCS change on either side. The run-to-run variation of the collector and the effect of the Phase 3 UtilitiesCS.Test fixture-path rewrites on which production lines execute are both possible causes. The per-class Cobertura document that would separate them was discarded by the route after it wrote the summary, so the cause is unknown. No re-run was made to obtain a different figure.

Package-level projection (coverage/coverage.cobertura.jacoco.xml, embedded verbatim):

```xml
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4213" covered="39210" />
    <counter type="BRANCH" missed="1799" covered="9472" />
  </package>
  <package name="TaskVisualization">
    <counter type="LINE" missed="143" covered="1426" />
    <counter type="BRANCH" missed="67" covered="333" />
  </package>
  <package name="SVGControl">
    <counter type="LINE" missed="977" covered="877" />
    <counter type="BRANCH" missed="338" covered="300" />
  </package>
  <package name="ToDoModel">
    <counter type="LINE" missed="762" covered="1061" />
    <counter type="BRANCH" missed="260" covered="248" />
  </package>
  <package name="Tags">
    <counter type="LINE" missed="56" covered="702" />
    <counter type="BRANCH" missed="16" covered="174" />
  </package>
  <package name="TaskMaster">
    <counter type="LINE" missed="802" covered="2443" />
    <counter type="BRANCH" missed="211" covered="517" />
  </package>
  <package name="TaskTree">
    <counter type="LINE" missed="11" covered="295" />
    <counter type="BRANCH" missed="8" covered="94" />
  </package>
  <package name="VBFunctions">
    <counter type="LINE" missed="0" covered="4" />
    <counter type="BRANCH" missed="0" covered="0" />
  </package>
</report>
```
