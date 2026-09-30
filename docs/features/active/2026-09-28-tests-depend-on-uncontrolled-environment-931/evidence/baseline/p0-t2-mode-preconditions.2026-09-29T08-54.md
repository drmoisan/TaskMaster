# P0-T2 Full-Bug Mode Preconditions

Timestamp: 2026-09-29T08-54
Command: pwsh -NoProfile -Command (read-only checks over FEATURE/issue.md and FEATURE/spec.md: exact-line match for the work-mode marker and the AC heading, the regex `^- \[[ x]\] AC([1-9]|1[0-9])\. ` over spec.md, Test-Path for FEATURE/user-story.md, and the backticked entries of the spec's `## Write Set` section)
EXIT_CODE: 0

Output Summary:
- WORKMODE-LINE: 1 (FEATURE/issue.md contains the exact line `- Work Mode: full-bug`, line 12)
- AC-HEADING: 1 (FEATURE/spec.md contains the exact heading line `## Acceptance Criteria`, line 234)
- AC-INVENTORY: 19 (the box-state-independent inventory regex matches 19 lines, spec lines 236 to 254)
- AC-UNCHECKED: 19 (every inventory line begins `- [ ] `; none is checked before execution)
- USER-STORY-EXISTS: False
- Spec Write Set code and project paths (six, matching this plan's Write Set; no other code path):
  - QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
  - QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs
  - QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs
  - QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
  - QuickFiler.Test/QuickFiler.Test.csproj
  - UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs
- The remaining three Write Set entries are the three evidence directories under the feature folder (not code paths).
- Result: all five preconditions hold; MODE PRECONDITION FAILED did not fire.
