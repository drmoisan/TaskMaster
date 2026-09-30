# P1-T12 — Read-only verifier run on the fixed tree (CMD-VERIFIER-WHATIF)

Timestamp: 2026-09-30T10-18
Command: CMD-VERIFIER-WHATIF (pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $files = @(Get-ChildItem -Path "*/packages.config","*/app.config","*/*.csproj" -File); ...; $r = & ".\scripts\dependencies\Repair-PackageManifestConsistency.ps1" -WhatIf; ...')
EXIT_CODE: 0
Output Summary:
- Result line: ABSENT=0 DISAGREE=0 WRITTEN=0 SUCCESS=True PROJECTS=18 FILES=53 HASHES_EQUAL=True
- ABSENT=0 (P0-T17 measured 2)
- WRITTEN=0; HASHES_EQUAL=True (nothing written)
- PROJECTS=18; FILES=53 (equal to the P0-T17 value)
- DISAGREE=0 (equal to the P0-T17 value; this change moves no version); SUCCESS=True (recorded as measured)
- Information line (printed five times): "Manifest discovery: enumerated directories 35, returned files 53"
- -WhatIf messages: the same 17 "Rewrite in canonical inline form" targets P0-T17 recorded, each <execution-worktree-root>\<project>\packages.config for QuickFiler.Test, QuickFiler, SVGControl.Test, Tags.Test, Tags, TaskMaster.Test, TaskMaster, TaskTree.Test, TaskTree, TaskVisualization.Test, TaskVisualization, ToDoModel.Test, ToDoModel, UtilitiesCS.Test, UtilitiesCS, VBFunctions.Test and VBFunctions.
