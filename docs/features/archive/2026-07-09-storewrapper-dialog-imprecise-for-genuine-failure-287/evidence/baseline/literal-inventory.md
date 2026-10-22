Timestamp: 2026-09-01T02-20
Command: pwsh -NoProfile -Command 'git grep -n -F "Store settings are not available yet" -- "*.cs"; "EXIT=" + $LASTEXITCODE'
EXIT_CODE: 0
Output:
UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:167:                    "Store settings are not available yet. Please try again after startup completes.",
UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:123:                    "Store settings are not available yet. Please try again after startup completes.",
EXIT=0

Command: pwsh -NoProfile -Command 'git grep -n -F "shows the same warning as the single-store editor" -- "*.cs"; "EXIT=" + $LASTEXITCODE'
EXIT_CODE: 0
Output:
UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:156:        /// not ready it shows the same warning as the single-store editor and leaves
EXIT=0

Command: pwsh -NoProfile -Command 'git grep -n -F "Store Settings Unavailable" -- "*.cs"; "EXIT=" + $LASTEXITCODE'
EXIT_CODE: 0
Output:
UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:168:                    "Store Settings Unavailable",
UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:124:                    "Store Settings Unavailable",
EXIT=0

Output Summary: First search reports exactly two lines: StoreWrapperController.cs:123 and DisabledStoresController.cs:167, matching D1 expectation. Second search reports exactly one line, in DisabledStoresController.cs:156. Third search reports exactly two lines: StoreWrapperController.cs:124 and DisabledStoresController.cs:168 — this is the tree-verified before-count that AC6/AC10's title half (P5-T6) is measured against, and it doubles as the positive control proving the search mechanism resolves tracked C# content.
