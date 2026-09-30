# P2-T10 Fix Commit

Timestamp: 2026-09-29T09-21
Command: git add -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs QuickFiler.Test/QuickFiler.Test.csproj UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931; git commit -m "test(931): remove scheduler and file-handle dependence from breadcrumb affinity and FileInfoWrapper tests" (plus the session attribution paragraphs Co-Authored-By and Claude-Session in bare form); git ls-files --eol -- (the six code paths) (before and after); git diff --exit-code HEAD -- QuickFiler.Test UtilitiesCS.Test; git show --name-only --format= HEAD; git rev-parse HEAD; Get-FileHash -Algorithm SHA256 of the six code files
EXIT_CODE: 0

Output Summary:
- EXIT_CODE is scoped to the git diff --exit-code HEAD -- QuickFiler.Test UtilitiesCS.Test span (exit 0: the committed state equals the working tree for both test projects).
- COMMIT-EXIT: 0 (no PreToolUse refusal)
- FIX-HEAD: 2956820e1be115c9e473e2be2c083ff60a0f08a8
- EOL-RESTORED: NONE (every entry already read w/crlf; the P2-T7 scoped format had converted the two new files to CRLF)
- FIX-HASH-AFF: BAD939900B47AE0AC963D203F621D28C959CC7F3E492119F1BD8272197EF5940
- FIX-HASH-PART2: D6A35090996E48283CA04D8EE2C441F8ED8946BAECC1D06D9F16931FF767976E
- FIX-HASH-HELPER: 986838E4FD72A7E233B26A1BE03B5912DFBB4016980F515F5C01680A0ABF688E
- FIX-HASH-BND: 4F8B8AFFA9387043AAE3A66EF2028D96AB4F0FA77AFCE244C9044E67B9B8E0FE
- FIX-HASH-CSPROJ: 5538F090CDBC43F57E03000C1776D9881E147302E513D1D03D58C6102E4DC39F
- FIX-HASH-FIW: 142BCB8E95D710890A71F325A65801ED8EB53A87A339B10018D889491ECEF596
- Each of the five .cs hashes equals its P2-T7 (ITERATION: 1) post-format hash; no path was restored.
- Acceptance: COMMIT-EXIT 0; EXIT_CODE 0; git show lists exactly the six code paths plus paths under the feature folder; every EOL-AFTER entry reads i/lf and w/crlf - HOLD.

## EOL-BEFORE

    i/lf    w/crlf  attr/text=auto        	QuickFiler.Test/QuickFiler.Test.csproj
    i/lf    w/crlf  attr/text=auto        	QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs
    i/lf    w/crlf  attr/text=auto        	QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
    i/lf    w/crlf  attr/text=auto        	QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs
    i/lf    w/crlf  attr/text=auto        	QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
    i/lf    w/crlf  attr/text=auto        	UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs

## EOL-AFTER

    i/lf    w/crlf  attr/text=auto        	QuickFiler.Test/QuickFiler.Test.csproj
    i/lf    w/crlf  attr/text=auto        	QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs
    i/lf    w/crlf  attr/text=auto        	QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
    i/lf    w/crlf  attr/text=auto        	QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs
    i/lf    w/crlf  attr/text=auto        	QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
    i/lf    w/crlf  attr/text=auto        	UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs

## git show --name-only --format= HEAD

    QuickFiler.Test/QuickFiler.Test.csproj
    QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs
    QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
    QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs
    QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
    UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t1-helper-census.2026-09-29T09-10.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t2-csproj-census.2026-09-29T09-11.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t3-part2-census.2026-09-29T09-12.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t4-primary-census.2026-09-29T09-13.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t5-boundary-census.2026-09-29T09-14.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t6-fileinfowrapper-census.2026-09-29T09-15.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t7-csharpier-scoped.2026-09-29T09-16.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t8-build-after-fix.2026-09-29T09-18.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t9-pass-before-controls.2026-09-29T09-20.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/plan.2026-09-28T20-01.md
