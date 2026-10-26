# Final Toolchain: Format (Issue #1007, remediation cycle 1, P5-T1)

Timestamp: 2026-10-26T00-00
Command: git status --porcelain --untracked-files=all; dotnet tool run csharpier format .; git status --porcelain --untracked-files=all; dotnet tool run csharpier check .
EXIT_CODE: 0
Output Summary: Loop pass 2 is clean. `csharpier format .` rewrote nothing: the porcelain captures before and after are identical, and so is a SHA-256 over the porcelain, the `git diff HEAD` text, and the untracked-file hashes (`701284063eb682b7493435799d5ecd0d473c21a398bab71eb6acccc2ef2daa18` both times). `csharpier check .` exited 0 with `Checked 1700 files in 4943ms.`

## Loop history

- Pass 1: `csharpier format .` reformatted test code that this cycle added. The tree hash changed while the list of changed paths stayed the same, so the rewrite stayed inside WRITE-SET files and touched no other file. Per the loop rule, the loop restarted at P5-T1.
- Pass 2 (this artifact): format rewrote nothing; the check passed.

## Porcelain capture before format (identical after format)

```
 M QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.DoubleClick.cs
 M QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs
 M QuickFiler.Test/QuickFiler.Test.csproj
 M QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.cs
 M QuickFiler.Test/Viewers/FolderBreadcrumbKeysIssue1007Tests.cs
 M QuickFiler/Resources/FolderBreadcrumb.html
 M UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs
 M UtilitiesCS.Test/UtilitiesCS.Test.csproj
?? QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.Unhandled.cs
?? UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.Unanchored.cs
?? docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/evidence/other/preflight-fixes.2026-10-25T23-50.md
?? docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/evidence/other/test-renames.2026-10-25T23-59.md
?? docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/evidence/regression-testing/n1-direct-tests.2026-10-25T23-58.md
?? docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/evidence/regression-testing/qfc-pending-style-fail-before.2026-10-25T23-55.md
?? docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/evidence/regression-testing/qfc-pending-style-pass-after.2026-10-25T23-55.md
?? docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/evidence/remediation-baseline/baseline-coverage.2026-10-25T23-53.md
?? docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/evidence/remediation-baseline/baseline-file-sizes.2026-10-25T23-53.md
?? docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/evidence/remediation-baseline/baseline-format.2026-10-25T23-51.md
?? docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/evidence/remediation-baseline/baseline-typecheck.2026-10-25T23-51.md
?? docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/evidence/remediation-baseline/phase0-instructions-read.2026-10-25T23-50.md
?? docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/remediation-plan.2026-10-25T23-50.md
```

## Porcelain capture after format

Identical to the capture above (same 21 lines; tree hash unchanged).
