# Phase 0 — Pre-existing 500-line-cap state

Timestamp: 2026-09-07T01-01
Task: [P0-T12]
Issue: #798

## The pre-existing violation

`UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` is at 882 lines at the base commit
`c431dc32`, as measured and recorded by P0-T11. The repository's General Code Change Policy sets a
500-line cap on any production, test or reusable script file. That file therefore exceeds the cap by
382 lines **before** this change touches it. The violation is pre-existing and is not created by
this work.

## Why this change does not bring the file under the cap

This item is a bugfix. The bugfix workflow in the repository-root instruction file requires the
minimal, targeted fix and prohibits opportunistic refactors, directing that deeper design problems
be opened as new issues rather than widening scope.

Bringing `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` under 500 lines would require splitting
it into at least one additional file. That additional file would be a seventeenth path, outside the
sixteen-path write set that spec.md fixes for this change and that AC13 pins. Creating it would both
widen the scope beyond the fix and break the write-set gate at P7-T2, which requires the diff to
touch exactly sixteen paths with no extra path.

The only edit this change makes to that file is the reflection-test repair in P1-T11, which deletes
the helper `GetAddQfcColumnsAsyncMethod` and its two local bindings and replaces two reflective
invocations with direct calls. That edit reduces the file's line count.

## How AC13 is satisfied for this one file

AC13 holds every file created or modified by this change to the absolute 500-line cap, with one
exception for a file already over the cap at the base commit. For
`UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`, AC13 as written holds the file to a **strictly
decreasing** line count relative to its base-commit value of 882, not to the absolute cap.

This is recorded as **satisfying AC13**, not as a deviation from it. P1-T11's acceptance requires
the file's line count to be strictly less than 882, and P7-T4 and P8-T9 re-verify the same condition
after each formatting pass. No deviation note is added beside the AC13 checkbox in spec.md, and
P9-T13 checks AC13 off on this basis.

## Every other file is held to the absolute cap

Every other file created or modified by this change is held to the absolute 500-line cap. The
relevant baseline headroom, from P0-T11:

- `UtilitiesCS/Extensions/DfDeedle.cs` starts at 410 and loses lines to the new partial in P1-T2.
- `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` starts at 154.
- `QuickFiler/Controllers/QfcDatamodel.cs` starts at 483, the narrowest headroom at 17 lines. Its
  only edit, P5-T2, converts two `throw e;` statements to `throw;` and adds no line.
- `TaskMaster/Ribbon/RibbonViewer.cs` starts at 388 and gains one field, one static failure sink and
  three rewritten one-line handlers in P6-T3 and P6-T4.
- The six files this change creates start at zero lines and are authored under the cap.

P7-T4 audits all eleven write-set `.cs` files against this rule, and P8-T9 re-verifies after the
final formatting pass, because formatting changes line counts.

## Follow-up promotion required by AC13's final clause

AC13's final clause requires the pre-existing violation to be promoted as a follow-up so it is not
lost when this feature folder is archived. That promotion is tracked by P9-T16, which records three
findings, the third being this 500-line-cap violation in
`UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` together with the observed post-change line
count of that file. P9-T13 may not check AC13 off until that entry is present.

Output Summary: `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` stands at 882 lines at base
commit `c431dc32`, already over the repository's 500-line cap before this change. Bringing it under
the cap would require a seventeenth file outside the write set that spec.md fixes, which the bugfix
workflow prohibits as an opportunistic refactor. AC13 therefore holds this one file to a strictly
decreasing count relative to 882, which this change delivers through the P1-T11 repair; this
satisfies AC13 rather than deviating from it. Every other created or modified file is held to the
absolute 500-line cap. The pre-existing violation is promoted as a follow-up by P9-T16.
