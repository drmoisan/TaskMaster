# P4-T4 — AC2 Invariant And Trace-Unreachability Gate

Timestamp: 2026-09-09T11-16
Task: [P4-T4]
EXIT_CODE: 0

## Command 1 — the invariant is stated in the delivered code

Command: `git grep -c -F -e 'pair is counted exactly once' -- scripts/vscode`
EXIT_CODE: 0

```
scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1:1
```

Exactly one line, naming `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` with count 1. The
sentence carrying it, on a single physical line of the comment-based help for
`Get-CoberturaFirstPartyCoverageSummary`, is: the counting invariant this function establishes is
that each (class, source line number) pair is counted exactly once, so LinesValid, LinesCovered,
BranchesValid and BranchesCovered do not depend on whether a source line also appears under a method
element. That is the invariant stated in the Root Cause Analysis section of `spec.md`, verbatim in
substance.

## Command 2 — the new code re-derives no branch parsing

Command: `git grep -c -F -e 'condition-coverage' -- scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1 scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
EXIT_CODE: 0

```
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:6
```

Exactly one line, naming `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` with count 6. That
hit is the **positive control**: it proves the search reports an occurrence when one is present, so
the absence of a line naming the new file is a real observation rather than a failed query. The six
occurrences in Helpers.ps1 are the pre-existing branch-parsing implementation in
`Get-CoberturaLineConditionCoverageParts` and `Get-CoberturaClassLineSummary`, which this feature does
not modify.

**No line names `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1`**, so the delivered
aggregation parses no branch attribute of its own.

## Command 3 — the new code reaches the de-duplicating helper by delegation

Command: `git grep -c -F -e 'Get-CoberturaPackageLineSummary' -- scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1`
EXIT_CODE: 0

```
scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1:3
```

One line with count 3, which is 1 or greater. The three occurrences are the call in the accumulation
loop and two references in comment-based help.

## Why no step of the Root Cause Analysis trace remains reachable

The four numbered trace steps in `spec.md` describe the defect the delivered implementation removes.
Step 2, the selection point, and step 3, the accumulation, are the two steps where the error is
committed, and both are absent from the new code path:

- Step 2 requires a descendant-axis selection over `<line>`. P4-T2 established that the literal does
  not occur anywhere under `scripts/vscode/`, and the new function selects
  `/coverage/packages/package` and then hands each package to `Get-CoberturaPackageLineSummary`. It
  never enumerates `<line>` elements at all.
- Step 3 requires the loop to parse each row's branch attribute and add the pair unconditionally.
  Command 2 above shows the new file contains no branch-attribute parsing, so there is no such loop
  to reach. The per-class figures come from `Get-CoberturaClassLineSummary` through
  `Get-CoberturaPackageLineSummary`, which keys by line number and resolves a repeated key by the
  precedence rule already documented on that function.
- Step 4, reporting, is now reached through a committed, tested function rather than through a
  pasted snippet, which is the exposure `spec.md` identifies as the actual root cause.

Because the new code delegates rather than re-implements, exactly one implementation of the counting
rule exists in the repository and every caller applies that same one.

Output Summary: The invariant literal appears exactly once, in the new production file. The
branch-parsing literal appears only in `Invoke-MSTestWithCoverage.Helpers.ps1`, with count 6 as the
positive control, and not in the new file. The delegation literal
`Get-CoberturaPackageLineSummary` appears 3 times in the new file. AC1 and AC2 are discharged: the
counting rule is stated as an invariant, is not re-derived, and no step of the Root Cause Analysis
trace is reachable from the new code path.
