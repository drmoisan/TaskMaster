# P4-T13 Follow-Up Handoff Record (for the orchestrator)

Timestamp: 2026-09-29T09-46

The orchestrator files each entry below as a potential entry through the promotion lifecycle's MCP tool (D-9). The executor created nothing under docs/features/potential/. Each body is drawn from the spec's Rollout & Follow-up list (spec lines 274 to 278) and the spec's out-of-scope findings (spec lines 65 to 66).

## Follow-up 1

short_name: physicalfilesystemadapters-tests-open-repository-solution-file

UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs carries the same defect class as issue #906. The file defines its own solution-file locator (called at lines 173 and 318 and declared at lines 373 to 376), swallows contention in the `catch (IOException)` blocks at lines 43 and 195 (the spec also cites the swallowing span at lines 186 to 198), and opens the real repository solution file for reading at lines 213 to 234. Its outcome therefore depends on whether another process holds the solution file, which is uncontrolled environment state. The remedy follows the #931 pattern for FileInfoWrapper_Tests.cs: a test-owned stream over the test host's own loaded assembly image through an injectable seam, with no repository file opened. This entry is not fixed under #931.

## Follow-up 2

short_name: directoryinfowrapper-tests-enumerate-repository-solution-file

UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs lines 60, 79 and 381 assert that TaskMaster.sln is enumerated from the repository root. The tests therefore depend on the repository layout and on the real file system at run time, which is the same defect class as issue #906. The remedy is to assert enumeration over a controlled seam, or over a fixture directory the test owns without creating temporary files, instead of over the repository root. This entry is not fixed under #931.

## Follow-up 3

short_name: breadcrumb-dispatchvalue-message-wording-broader-than-mechanism

QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs line 305 asserts the message "cannot marshal cross-thread UI work". That wording describes the mechanism more broadly than `DispatchValue` implements: `DispatchValue` on an owner-only dispatcher faults for every caller outside an executing callback, on any thread, including the owner thread (spec Triage table row for line 301). The change is to the wording only. No behaviour change is needed. This entry is not fixed under #931.

## Follow-up 4

short_name: issue-900-handoff-misattributes-dispatchvalue-site-to-owner-thread-id-check

The #900 follow-up handoff record under docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/ attributes BreadcrumbUiThreadDispatchTests.cs line 301 to the owner-thread-id check. The #931 research record shows that line 301 reaches `DispatchValue`, which never reads the owner thread id. The correction is to the documentation only. This entry is not fixed under #931.
