# P8-T4 — Manual validation against a live Outlook profile (#614; AC26)

Timestamp: 2026-08-26T18-55

## Overall result

Every one of the five spec `## Test Strategy` manual steps is recorded as **NOT EXECUTED**, with
the reason below. No step is silently omitted. The spec explicitly permits recorded non-execution
with a reason.

## Reason (applies to all five steps)

This change was implemented by an automated agent session. That session has no interactive Windows
desktop, no signed-in Outlook profile, and no running `outlook.exe` process it may drive. Live
Outlook interaction is additionally excluded from automated tests by repository policy: the
suite-wide filter is `/TestCaseFilter:TestCategory!=LiveOutlook`, and the general unit test policy
prohibits tests that depend on external processes. There is therefore no mechanism available to
this executor for performing an interactive filing operation against a real mailbox.

## Per-step record

| # | Step | Result | Reason |
| --- | --- | --- | --- |
| 1 | File an item to a normal archive subfolder; it succeeds. | NOT EXECUTED | No interactive live Outlook session available to the executor. |
| 2 | Activate a store-root ancestor segment; the selection is left unchanged and a diagnostic is emitted. | NOT EXECUTED | No interactive live Outlook session available to the executor. |
| 3 | Press OK with the archive root selected; it fails fast with a redacted message. | NOT EXECUTED | No interactive live Outlook session available to the executor. |
| 4 | File to a folder whose name contains `.`; it succeeds. | NOT EXECUTED | No interactive live Outlook session available to the executor. |
| 5 | Confirm no message, dialog, or log line from steps 2-4 contains a real identifier. | NOT EXECUTED | Depends on steps 2-4, which were not executed. |

## Automated coverage standing in for each step

Each manual step has a headless automated counterpart that WAS executed and passed. These do not
replace the live-profile validation, but they record what is already verified:

| # | Automated counterpart | Evidence |
| --- | --- | --- |
| 1 | `SegmentActivate_UnderRootAncestor_SetsTheRelativeStem`, `RenderedChildActivate_UnderRootChild_SetsTheRelativeStem`, `ResolvePaths_WithCurrentFolder_SetsDerivedPropertiesAndLeavesDestinationNullWhenUnresolved`, `Issue609_ResolvePaths_PrefixesAtMailboxArchiveRootExactlyOnce` | `p3-t5-router-tests`, `p4-t3-boundary-tests` |
| 2 | `SegmentActivate_StoreRootAncestor_LeavesSelectionUnchangedAndDiagnoses` (asserts the selection is unchanged and non-null AND that a rejection diagnostic reaches a log4net MemoryAppender) | `p3-t5-router-tests` |
| 3 | `SegmentActivate_ArchiveRootExactly_IsTreatedAsNonSelection`, `EfcSelectionGuardTests` (all nine cases, including the store-rooted, single-separator-leading and drive-rooted rejections), `Issue614_ResolvePaths_RejectsEmptyStem` | `p3-t5-router-tests`, `p4-t6-guard-tests`, `p4-t3-boundary-tests` |
| 4 | `ToFsFolderpath_DerivedSegmentContainingADot_Succeeds`, `ToFsFolderpath_DottedAndHyphenatedFilesystemRoot_Succeeds`, `ToFsFolderpath_DerivedSegmentWithInteriorDot_Succeeds` | `p5-t7-converter-tests` |
| 5 | Message-content assertions in `ArchiveStemContractTests`, `AppOlObjectsArchiveRootValidationTests`, `AppFileSystemFolderPathsOneDriveResolutionTests`, `FolderConverterIssue614Tests.ToFsFolderpath_InvalidSegment_MessageLeaksNeitherMailboxNorFsAncestor`, `EfcDataModelIssue614Tests.ToArchiveRelativeStem_StoreRootFolder_ThrowsWithoutLeakingIdentifiers`, and the primary regression test's message assertions; plus the P8-T3 redaction sweep over every changed file. | `p2-t3-contract-tests`, `p7-t5-appglobals-tests`, `p5-t7-converter-tests`, `p6-t3-datamodel-tests`, `p4-t3-boundary-tests`, `p8-t3-redaction-sweep` |

## Redaction check outcome

The redaction half of step 5 could not be observed against live dialogs. Its static equivalent WAS
performed and passed: see `evidence/qa-gates/p8-t3-redaction-sweep.2026-08-26T18-50.md`, which
records a host-name search returning `none` over all 64 changed files, an address search returning
`none` over all 2276 added lines, and an account-name search whose single hit is a pre-existing
`<PublishUrl>` element that no hunk of this change touches.

## Recommended follow-up

A maintainer with an interactive Outlook profile should execute steps 1 through 5 before release
and append the observed results to this artifact.
