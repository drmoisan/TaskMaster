# P0-T17 — Pre-fix altcover state and read-only verifier baseline

Timestamp: 2026-09-30T09-49
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $m = @(git grep -i -n altcover -- "*.csproj" "*/packages.config"); "ALTCOVER_LINES=$($m.Count)"; $m' then CMD-VERIFIER-WHATIF
EXIT_CODE: 0
Output Summary:
- ALTCOVER_LINES=2
  - QuickFiler.Test/QuickFiler.Test.csproj:8:  <Import Project="..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.props" Condition="Exists('..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.props')" />
  - QuickFiler.Test/QuickFiler.Test.csproj:537:  <Import Project="..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.targets" Condition="Exists('..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.targets')" />
- CMD-VERIFIER-WHATIF result line: ABSENT=2 DISAGREE=0 WRITTEN=0 SUCCESS=True PROJECTS=18 FILES=53 HASHES_EQUAL=True
- DISAGREE (0) and SUCCESS (True) recorded as measured with no expectation: DISAGREE is computed over the in-memory repaired project text (Repair-PackageManifestConsistency.ps1 lines 411 to 422).
- Information line (printed five times): "Manifest discovery: enumerated directories 35, returned files 53"
- -WhatIf messages (operation "Rewrite in canonical inline form"), target paths with prefix replaced:
  - <execution-worktree-root>\QuickFiler.Test\packages.config
  - <execution-worktree-root>\QuickFiler\packages.config
  - <execution-worktree-root>\SVGControl.Test\packages.config
  - <execution-worktree-root>\Tags.Test\packages.config
  - <execution-worktree-root>\Tags\packages.config
  - <execution-worktree-root>\TaskMaster.Test\packages.config
  - <execution-worktree-root>\TaskMaster\packages.config
  - <execution-worktree-root>\TaskTree.Test\packages.config
  - <execution-worktree-root>\TaskTree\packages.config
  - <execution-worktree-root>\TaskVisualization.Test\packages.config
  - <execution-worktree-root>\TaskVisualization\packages.config
  - <execution-worktree-root>\ToDoModel.Test\packages.config
  - <execution-worktree-root>\ToDoModel\packages.config
  - <execution-worktree-root>\UtilitiesCS.Test\packages.config
  - <execution-worktree-root>\UtilitiesCS\packages.config
  - <execution-worktree-root>\VBFunctions.Test\packages.config
  - <execution-worktree-root>\VBFunctions\packages.config
- HASHES_EQUAL=True: the what-if run wrote nothing (FILES=53, at least 50).
