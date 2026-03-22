param(
    [string]$CoberturaPath = "coverage\coverage.cobertura.xml",
    [string]$OutputPath = "docs\features\active\2026-03-19-utilities-coverage-part-three-87\evidence\baseline\baseline-per-file-coverage.md"
)

[xml]$cob = Get-Content $CoberturaPath
$pkg = $cob.coverage.packages.package | Where-Object { $_.name -eq 'UtilitiesCS' }
$classes = $pkg.classes.class

$below80 = @()
$atOrAbove80 = @()
foreach ($c in $classes) {
    $lr = [double]$c.'line-rate'
    $pct = [math]::Round($lr * 100, 1)
    $fn = $c.filename -replace '.*\\UtilitiesCS\\', ''
    $obj = [PSCustomObject]@{ File = $fn; Pct = $pct; LineRate = $lr }
    if ($lr -lt 0.80) { $below80 += $obj } else { $atOrAbove80 += $obj }
}

$below80 = $below80 | Sort-Object Pct

# Categorize by difficulty per research doc
$easy = @(); $medium = @(); $hard = @(); $skip = @()
foreach ($f in $below80) {
    $fn = $f.File
    if ($fn -match '\.Designer\.cs$') { $skip += $f; continue }
    if ($fn -match 'ConversationHelper|MailItemHelper|StoreWrapperController|OlTable|ClassifierGroup|Engine|Triage\.cs|ManagerAsyncLazy|InputBox|MyBox|NotImplemented|MyBoxViewer|DelegateButton|FunctionButton|YesNoToAll|ControlPosition|ControlResizer|TableLayoutHelper|ScreenHelper|MouseDownFilter|ImageHelper|OlvExtension|Theme\.cs|ThemeControlGroup|TipsController|FilterOlFolders|FolderRemap|ConfigController|DispatchUtility|ShellUtilities\.cs|ComStreamWrapper|OneDriveDownloader|IdleActionQueue|IdleAsyncQueue|ApplicationIdleTimer|UiThread|DvgForm|AutoFile|EmailDataMiner|EmailFiler|MethodBodyReader|ProgressPane|ProgressViewer|ProgressMultiStepViewer|ProgressTrackerPane|FolderInfoViewer|FolderSelector|ConfigViewer|ConfigGroupBox|SubjectMapMetrics|OSBrowser|Viewer|SelectionSorter|FolderRemapTree') {
        $hard += $f; continue
    }
    if ($fn -match 'Converter|ScDictionary|SCODictionary|ScoCollection|ScoSortedDictionary|ScoStack|ScoDictionaryNew|ScBag|SerializableList|SloLinkedList|SmartSerializable|BayesianClassifier|BayesianPerformance|BayesianSerialization|Corpus|CorpusInherit|OutlookItem|AttachmentHelper|AttachmentSerializable|CreateCategory|RecipientStatic|UserDefinedFields|StoreWrapper\.cs|FileInfoWrapper|DirectoryInfoWrapper|FileSystemInfoWrapper|FilePathHelper\.cs|ProgressTracker|AsyncMultiTasker|ThreadMonitor|FlagTranslator|IntelligenceConfig|SubjectMapEncoder|SubjectMapSco|PeopleScoDictionary|RecentsList|LockingObservable|TimedDiskWriter|SystemThemeDetector|QfcTipsDetails|ShellUtilitiesStatic|ClassifierGroupUtilities|Triage_OlLogic|DrawingExtensions|ImageExtensions|AsyncSerialization|DfDeedle|DfMLNet|MonoExtension|ILGlobals|ILInstruction|WrapperPeople|OlItemPseudo') {
        $medium += $f; continue
    }
    $easy += $f
}

$sb = [System.Text.StringBuilder]::new()
[void]$sb.AppendLine("# Baseline Per-File Coverage — UtilitiesCS")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("Timestamp: $(Get-Date -Format 'yyyy-MM-ddTHH-mm')")
[void]$sb.AppendLine("Source: ``coverage/coverage.cobertura.xml``")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("## Summary")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("- Total UtilitiesCS classes in coverage report: $($classes.Count)")
[void]$sb.AppendLine("- Files below 80% line coverage: $($below80.Count)")
[void]$sb.AppendLine("- Files at or above 80%: $($atOrAbove80.Count)")
[void]$sb.AppendLine("- UtilitiesCS package line-rate: $([math]::Round([double]$pkg.'line-rate' * 100, 2))%")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("## Categorization")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("- Easy: $($easy.Count) files")
[void]$sb.AppendLine("- Medium: $($medium.Count) files")
[void]$sb.AppendLine("- Hard: $($hard.Count) files")
[void]$sb.AppendLine("- Skip candidates: $($skip.Count) files")
[void]$sb.AppendLine("")

function Write-Category($sb, $title, $items) {
    [void]$sb.AppendLine("### $title ($($items.Count) files)")
    [void]$sb.AppendLine("")
    if ($items.Count -eq 0) {
        [void]$sb.AppendLine("_(none)_")
    } else {
        [void]$sb.AppendLine("| File | Line Rate |")
        [void]$sb.AppendLine("|---|---|")
        foreach ($f in $items) {
            [void]$sb.AppendLine("| $($f.File) | $($f.Pct)% |")
        }
    }
    [void]$sb.AppendLine("")
}

Write-Category $sb "Easy" $easy
Write-Category $sb "Medium" $medium
Write-Category $sb "Hard" $hard
Write-Category $sb "Skip Candidates" $skip

Set-Content -Path $OutputPath -Value $sb.ToString() -Encoding UTF8
Write-Host "Written to $OutputPath"
Write-Host "Total below 80%: $($below80.Count)"
Write-Host "Easy: $($easy.Count) | Medium: $($medium.Count) | Hard: $($hard.Count) | Skip: $($skip.Count)"
