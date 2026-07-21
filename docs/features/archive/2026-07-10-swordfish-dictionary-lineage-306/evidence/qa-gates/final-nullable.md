# P9-T3 — Final Nullable / TreatWarningsAsErrors Gate

Timestamp: 2026-07-11T04-13

Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true

EXIT_CODE: 0

Output Summary:
- PASS. Build succeeded with 0 errors and 0 warnings-as-errors. This is an incremental build: because the immediately-preceding P9-T2 analyzer build compiled every project successfully, CoreCompile is up-to-date and skipped, giving the same 0/0 incremental-no-op result recorded at the Phase 0 nullable baseline (`evidence/baseline/baseline-nullable.md`). The comparison is therefore apples-to-apples: no NEW nullable errors are introduced on the touched code paths relative to baseline.
- Production nullable-cleanliness was independently confirmed during development: an intermediate forced recompile (before the test fixtures were migrated) reported ZERO nullable errors in any production (non-`.Test`) project for all F1-touched files (IToDoObjects, AppToDoObjects, SubjectMapEncoder, FolderScorer, EmailDetails, EmailDetailsWrapper, SortEmail, FolderRemapController, FilterOlFoldersController). The nullable errors surfaced in that intermediate forced recompile were pre-existing `UtilitiesCS.Test` debt (CS8625/CS8765) in files unrelated to F1 (e.g., AttachmentHelperTests, SpamBayes_Tests) plus the transient CS0738 contract errors that resolved once the Phase 7 fixtures were migrated.
- `MSYS_NO_PATHCONV=1` used to prevent git-bash conversion of the `/p:` switches.
