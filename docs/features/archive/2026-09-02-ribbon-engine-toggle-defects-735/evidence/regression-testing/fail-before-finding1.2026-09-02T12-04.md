# Finding 1 — Fail-Before Run (P1-T2) [expect-fail]

Timestamp: 2026-09-03T01-23
Task: [P1-T2]
ExpectedExitCode: 1
EXIT_CODE: 1

A non-zero exit is the required outcome of this task. The two new tests are asserted against the
PRE-FIX CustomUI document, so both must fail here; P1-T7 re-runs them after the fix and requires
both to pass.

## Commands

Build first (the tests read the embedded CustomUI resource, so the assembly must be current):

```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

BUILD_EXIT_CODE: 0

`/t:Build` is correct here rather than `/t:Rebuild`: this task changed a source file, so
`CoreCompile` is not up to date and does run. A green build is what makes the failures below genuine
assertion failures rather than compile errors, which is also what verifies P1-T1's compilation
acceptance.

Then the scoped test run:

```
& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll `
  /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
  "/TestCaseFilter:FullyQualifiedName~RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod|FullyQualifiedName~RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters" `
  "/Logger:trx;LogFileName=p1-t2.trx" `
  /ResultsDirectory:docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\regression-testing\p1-t2
```

Command: the two invocations above, run in that order.

## Counts read from the TRX `ResultSummary/Counters` element

| Counter | Value |
|---|---|
| total | 2 |
| executed | 2 |
| passed | 0 |
| failed | 2 |

Console summary agreed: `Test Run Failed. Total tests: 2  Failed: 2`.

## Results directory contents

Exactly one TRX file and no other entry:

```
p1-t2.trx
```

Cleanup micro-action recorded for audit: the failing run left behind an empty MSTest deployment
scratch directory inside the results directory. Its directory names are generated from the local
account name and the machine name, which the Phase 5 sanitisation gate requires to be absent from
every name under the evidence tree, and this task's own acceptance requires the results directory to
hold exactly one TRX and no others. The directory tree contained no files and no evidence. It was
removed with `[System.IO.Directory]::Delete(path, true)` immediately after the run, and a
re-scan of the whole evidence tree for names containing either token then returned a count of zero.
The token values are not written here; they are derived at run time from `Split-Path -Leaf
$env:USERPROFILE` and `$env:COMPUTERNAME`.

## Failure message 1 — `RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod`

Quoted verbatim from the run:

```
Expected unresolved to be empty because every CustomUI callback name must resolve to a public
instance method on RibbonViewer; these 5 of 84 bound names do not: BtnMigrateIDs_Click,
MoveEntireConversation_Clicked, SaveAttachments_Clicked, SaveEmailCopy_Clicked,
SavePictures_Clicked, but found at least one item {"BtnMigrateIDs_Click"}.
```

All **five** unresolved names are reported in the one message, as required:

1. `BtnMigrateIDs_Click`
2. `MoveEntireConversation_Clicked`
3. `SaveAttachments_Clicked`
4. `SaveEmailCopy_Clicked`
5. `SavePictures_Clicked`

The count of 84 distinct bound callback names in the denominator independently reproduces the
research record's Claim B figure of 84 distinct callback names, derived there by two independent
enumerations.

Note on the trailing `but found at least one item {...}` clause: that fragment is FluentAssertions'
own boilerplate and names only the first item. The complete list is carried by the `because` text
this test supplies, which is why the assertion was written to join the full list into the reason
rather than relying on the collection rendering.

## Failure message 2 — `RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters`

Quoted verbatim from the run:

```
Expected defects to be empty because every checkBox onAction callback must be void
(Microsoft.Office.Core.IRibbonControl, bool); these 4 are not: MoveEntireConversationDefault:
'MoveEntireConversation_Clicked' resolves to no public instance method; SaveAttachmentsDefault:
'SaveAttachments_Clicked' resolves to no public instance method; SaveEmailCopyDefault:
'SaveEmailCopy_Clicked' resolves to no public instance method; SavePicturesDefault:
'SavePictures_Clicked' resolves to no public instance method, but found at least one item
{"MoveEntireConversationDefault: 'MoveEntireConversation_Clicked' resolves to no public instance
method"}.
```

All **four** unresolvable check-box callbacks are reported, each paired with its control id:

1. `MoveEntireConversationDefault` → `MoveEntireConversation_Clicked`
2. `SaveAttachmentsDefault` → `SaveAttachments_Clicked`
3. `SaveEmailCopyDefault` → `SaveEmailCopy_Clicked`
4. `SavePicturesDefault` → `SavePictures_Clicked`

The partition these two messages establish is the evidence for F1-AC4: of the five defective names,
the four with the `_Clicked` spelling each have a correctly signatured `_Click` twin on the viewer
type and are therefore renames, while `BtnMigrateIDs_Click` appears only in message 1 and in no
check-box, has no twin, and is therefore the removal.

Output Summary: EXPECTED FAILURE achieved. Build exited 0; the scoped run exited 1 with TRX counters
total 2, passed 0, failed 2. The first test reported all five unresolved callback names out of 84
bound names; the second reported all four unresolvable check-box callbacks with their control ids.
