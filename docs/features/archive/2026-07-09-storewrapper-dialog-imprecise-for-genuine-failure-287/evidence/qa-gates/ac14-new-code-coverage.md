Timestamp: 2026-09-01T06-30
Command: pwsh -NoProfile -Command '$x = [xml](Get-Content -Raw "coverage/post-change.cobertura.xml"); $cls = @($x.GetElementsByTagName("class") | Where-Object { $_.GetAttribute("name") -eq "UtilitiesCS.OutlookObjects.Store.StoreLaunchReadinessEvaluator" }); "classes=" + $cls.Count; foreach ($c in $cls) { "class-line-rate=" + $c.GetAttribute("line-rate"); foreach ($m in $c.GetElementsByTagName("method")) { "method=" + $m.GetAttribute("name") + " line-rate=" + $m.GetAttribute("line-rate") }; "uncovered-count=" + (@($c.SelectNodes("lines/line") | Where-Object { $_.GetAttribute("hits") -eq "0" }).Count); "uncovered-lines=" + ((@($c.SelectNodes("lines/line") | Where-Object { $_.GetAttribute("hits") -eq "0" }) | ForEach-Object { $_.GetAttribute("number") }) -join ",") }'
EXIT_CODE: 0
Output Summary:
classes=1
class-line-rate=1
method=Evaluate line-rate=1
method=BuildUnavailableMessage line-rate=1
method=BuildUnavailableTitle line-rate=1
uncovered-count=0
uncovered-lines=

Exactly one class element found. Methods BuildUnavailableMessage and BuildUnavailableTitle are both present with line-rate 1 (>= 0.9). Class line-rate is 1 (>= 0.9). uncovered-lines= is empty (transcribed verbatim); uncovered-count=0. No number falls within either method's line span (the list is empty). BuildUnavailableMessage spans lines 56-70 and BuildUnavailableTitle spans lines 82-93 in the post-change StoreLaunchReadinessEvaluator.cs.

Command: pwsh -NoProfile -Command 'git status --porcelain; git diff --name-only 09eae2e85cd586c092fb1977a76cd9e895ec0a3b..HEAD -- "coverage.config" "TaskMaster.runsettings" "scripts/vscode/TaskMaster.cli.runsettings"'
EXIT_CODE: 0
Output Summary: the exclusion-configuration diff prints no lines. No production source file was added to a coverage exclusion. AC14 satisfied.
