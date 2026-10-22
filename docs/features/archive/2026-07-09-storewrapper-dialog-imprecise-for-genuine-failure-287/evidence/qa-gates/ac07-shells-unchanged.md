Timestamp: 2026-09-01T06-02
Command: pwsh -NoProfile -Command 'git grep -n -F "[ExcludeFromCodeCoverage]" -- "UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs" "UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs"; git grep -n -F "readiness.State != StoreLaunchReadinessState.Ready" -- "UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs" "UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs"; git grep -n -F "MessageBoxButtons.OK" -- "UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs" "UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs"; git grep -n -F "MessageBoxIcon.Warning" -- "UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs" "UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs"'
EXIT_CODE: 0
Output Summary:
[ExcludeFromCodeCoverage]: DisabledStoresController.cs:161; StoreWrapperController.cs:116, 438 (3 lines total, exactly one in DisabledStoresController.cs)
readiness.State != StoreLaunchReadinessState.Ready: DisabledStoresController.cs:165; StoreWrapperController.cs:120 (2 lines, one per file)
MessageBoxButtons.OK: DisabledStoresController.cs:133, 170; StoreWrapperController.cs:125 (3 lines total, exactly two in DisabledStoresController.cs)
MessageBoxIcon.Warning: DisabledStoresController.cs:134, 171; StoreWrapperController.cs:126 (3 lines total, exactly two in DisabledStoresController.cs)

All four counts match the acceptance condition exactly. AC7 satisfied (positive half; see P5-T8 for the negative half).

---

Timestamp: 2026-09-01T06-05
Command: pwsh -NoProfile -Command 'git status --porcelain; git diff -U0 09eae2e85cd586c092fb1977a76cd9e895ec0a3b..HEAD -- "UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs" "UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs"'
EXIT_CODE: 0
Output Summary: BASE_SHA = 09eae2e85cd586c092fb1977a76cd9e895ec0a3b. Complete removed-line list across both files:
- DisabledStoresController.cs: three summary lines ("not ready it shows the same warning as the single-store editor and leaves", "<see cref=\"Viewer\"/> null, otherwise constructs the viewer, populates the list, and shows", "the dialog modally. WinForms shell; excluded from coverage.") and two dialog-argument literal lines ("Store settings are not available yet. Please try again after startup completes.", "Store Settings Unavailable").
- StoreWrapperController.cs: two dialog-argument literal lines (the same two literals).

No removed line contains `ExcludeFromCodeCoverage`, `MessageBoxButtons.OK`, `MessageBoxIcon.Warning`, or `readiness.State !=`. The removed lines are limited to the four dialog literal lines and the rewritten summary lines, as required. This is evidence-only; no AC checkbox is set by this task (P5-T8 is the stated exception in the plan's Phase 5 preamble).
