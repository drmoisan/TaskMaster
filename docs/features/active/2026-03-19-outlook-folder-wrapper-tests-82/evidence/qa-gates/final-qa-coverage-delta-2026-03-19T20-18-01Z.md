Timestamp: 2026-03-19T20:18:01.5302456Z
Command: pwsh -NoProfile -Command "$baselineRepo = [double](([regex]::Match((Get-Content (Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/baseline/baseline-test-*.md' | Sort-Object Name | Select-Object -Last 1).FullName -Raw), 'Repo Line Coverage: ([0-9]+(?:\.[0-9]+)?)%')).Groups[1].Value); [xml]$coverage = Get-Content 'coverage/coverage.cobertura.xml'; $repo = [math]::Round([double]$coverage.coverage.'line-rate' * 100, 2); $targets = @('UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs','UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs','UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs','UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs','UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs','UtilitiesCS/OutlookObjects/Folder/FolderTree.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameCountSizeComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeContentsComparer.cs','UtilitiesCS/OutlookObjects/Folder/MsgToMime/MAPIMethods.cs'); ... if ($repo -lt 80 -or $under.Count -gt 0 -or -not $changedCoverageMet) { exit 1 }"
EXIT_CODE: 1
Output Summary:
UtilitiesCS\OutlookObjects\Folder\FolderConverter.cs=95.95%
UtilitiesCS\OutlookObjects\Folder\FolderMinimalWrapper.cs=91.11%
UtilitiesCS\OutlookObjects\Folder\FolderNavigator.cs=100%
UtilitiesCS\OutlookObjects\Folder\FolderPredictor.cs=81.84%
UtilitiesCS\OutlookObjects\Folder\FolderScorer.cs=93.34%
UtilitiesCS\OutlookObjects\Folder\FolderTree.cs=85.18%
UtilitiesCS\OutlookObjects\Folder\FolderWrapper .cs=81.58%
UtilitiesCS\OutlookObjects\Folder\FolderWrapperNameAndParentNameComparer.cs=97.73%
UtilitiesCS\OutlookObjects\Folder\FolderWrapperNameComparer.cs=100%
UtilitiesCS\OutlookObjects\Folder\FolderWrapperNameCountSizeComparer.cs=100%
UtilitiesCS\OutlookObjects\Folder\FolderWrapperNodeComparer.cs=82.42%
UtilitiesCS\OutlookObjects\Folder\FolderWrapperNodeContentsComparer.cs=92.86%
UtilitiesCS\OutlookObjects\Folder\MsgToMime\MAPIMethods.cs=100%
BaselineRepo=42.2%
FinalRepo=44.56%
AnyFileUnder80=False
ChangedProductionCoverageMet=True
GateExitCode=1
Notes:
- All 13 in-scope folder production files now meet or exceed 80% line coverage.
- The gate still fails because the approved plan requires repository-wide line coverage >= 80%, while the latest verified repository coverage is 44.56%.
- The inline command transcription used forward slashes in the plan text; the actual Cobertura XML stores Windows-style backslash paths.
