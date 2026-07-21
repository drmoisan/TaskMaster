# Immutable Scope Baseline

Timestamp: 2026-07-16T16-18

Command: `pwsh -NoProfile -Command '& { $expectedHead="a22530c11dd9d2f3c94c74531840d889268b8d53"; $expectedMergeBase="0eb0b39abd206d8347f84d7fe438944a8d4d788e"; $head=(git rev-parse HEAD).Trim(); $mergeBase=(git merge-base bump-release HEAD).Trim(); if($head -ne $expectedHead -or $mergeBase -ne $expectedMergeBase){"HEAD=$head"; "MERGE_BASE=$mergeBase"; exit 1}; $expected=@("UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs","UtilitiesCS/Threading/ProgressViewer.cs"); $branchCs=@(git diff --name-only bump-release...HEAD -- "*.cs" | Sort-Object); if(Compare-Object $expected $branchCs){$branchCs; exit 1}; "BASE=bump-release"; "HEAD=$head"; "MERGE_BASE=$mergeBase"; foreach($p in $expected){"CS_SHA256=$p|$((Get-FileHash -Algorithm SHA256 $p).Hash.ToLowerInvariant())"}; foreach($p in @("docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/csharp-coverage-baseline.2026-07-16T12-39.cobertura.xml","docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/csharp-coverage-final.2026-07-16T12-39.cobertura.xml")){"COVERAGE_SHA256=$p|$((Get-FileHash -Algorithm SHA256 $p).Hash.ToLowerInvariant())"}; "BRANCH_CSHARP_COUNT=$($branchCs.Count)"; $branchCs }'`

EXIT_CODE: 0

Output Summary:

BASE=bump-release
HEAD=a22530c11dd9d2f3c94c74531840d889268b8d53
MERGE_BASE=0eb0b39abd206d8347f84d7fe438944a8d4d788e
CS_SHA256=UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs|64857226b2c7c248e8f90a76f03160b9af7fdd9dbb1fb5e5157fec5a1bf58dec
CS_SHA256=UtilitiesCS/Threading/ProgressViewer.cs|4ac9b2cf1d35e3a6b1e87390c88d4cb4179154d41cec187f23118a29f91269dd
COVERAGE_SHA256=docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/csharp-coverage-baseline.2026-07-16T12-39.cobertura.xml|ee64e724484f9f3430c0c7e69111f0e726963c49e205f8f3211854168537d915
COVERAGE_SHA256=docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/csharp-coverage-final.2026-07-16T12-39.cobertura.xml|5d03d792b74543f9e5ee7b9d08ae649ac923dda633ea4c72f40db0a31f2ce092
BRANCH_CSHARP_COUNT=2
UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs
UtilitiesCS/Threading/ProgressViewer.cs
