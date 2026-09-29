# Baseline Census ([P0-T14])

Timestamp: 2026-09-29T09-08
Command: CMD-CENSUS (pwsh -NoProfile -Command 'foreach ($p in "UtilitiesCS/Threading/UiThread.cs", ... ) { "LINES $p=..." }; "STALE_487=..."; ...; "RUNSETTINGS_HASH=..."', verbatim from the plan Command Reference)
Command: CMD-REF-SOURCE (git grep -n -I -F over six patterns, excluding docs, .claude and artifacts)
Command: CMD-REF-REPO (git grep -n -I -F over six patterns, repository-wide, classified by first path component)
EXIT_CODE: 0
REF_SOURCE_GREP_EXIT=0
REF_REPO_GREP_EXIT=0
Output Summary:
- LINES UtilitiesCS/Threading/UiThread.cs=306
- LINES UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs=228
- LINES QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs=102
- LINES QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs=45
- LINES UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs=164
- LINES UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs=270
- STALE_487=1, STALE_481=1, CEILING_BRIDGE=1, CEILING_LIFECYCLE=1
- DNP_HARDENING_FILE=2, DNP_ILGLOBALS_FILE=0
- BASELINE-RUNSETTINGS-HASH: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
- CMD-REF-SOURCE hits (exactly two):
  - UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs:267:            ILGlobals.Cache.Should().NotBeNull();
  - config/blast-radius.json:32:  "modules": {   (the blast-radius module map key, not a reference to ILGlobals)
- CMD-REF-REPO: REF_REPO_TOTAL=129, REF_REPO_DOCS=120, REF_REPO_GOVERNANCE=7, REF_REPO_OTHER=2; the two `$rest` entries are exactly the two CMD-REF-SOURCE lines above.
- Governance hits recorded by path (not classified): .claude/agent-memory/atomic-planner/MEMORY.md; .claude/agent-memory/task-researcher/project_ilglobals_static_publication_824.md; .claude/lib/blast-radius/BlastRadius.psm1; .claude/lib/blast-radius/BlastRadiusConfig.psm1; .claude/lib/blast-radius/BlastRadiusValidation.psm1; .claude/skills/parallel-plan/SKILL.md (7 hit lines across these 6 files).
