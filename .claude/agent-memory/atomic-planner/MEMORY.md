# Atomic Planner Memory Index

- [Coverage Evidence Path Normalization](evidence-path-normalization.md) — specs sometimes name evidence/coverage/; normalize to canonical baseline/ + qa-gates/
- [Manager AsyncLazy shared seam](project_manager_asynclazy_shared_seam.md) — Globals.AF.Manager is shared across all classifier subsystems; use a key-specific accessor, never retype the dictionary value for one key
- [Folder predictor AF holder seam](project_folder_predictor_af_holder_seam.md) — #177 F1: route flag-on LCPPN predictor through a Folder-only holder on IAppAutoFileObjects (globals.AF), not per-instance OlFolderClassifierGroup state
- [Plan validator phase-heading constraint](plan-validator-phase-heading-constraint.md) — MCP plan validator requires exact `### Phase N — <Title>`; no tokens between Phase N and em-dash; H1 title line is exempt
- [Legacy csproj explicit Compile Include](project_legacy_csproj_explicit_compile_include.md) — new .cs in UtilitiesCS/TaskMaster.Test (packages.config, no glob) needs <Compile Include> wiring in scope-lock + task AC
- [#211 startup-lifetime heartbeat seam](project_211_startup_lifetime_heartbeat_seam.md) — Phase 3.3 [startup-lifetime-heartbeat] DispatcherTimer in ThisAddIn.cs (exempt), pure logic in StartupDiagnosticsProbe; AC15
