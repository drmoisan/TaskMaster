Timestamp: 2026-09-03T01-24

Command: pwsh -File scripts\vscode\Invoke-VSBuild.ps1 -Target Rebuild -TreatWarningsAsErrors

EXIT_CODE: 0

Output Summary: "Build succeeded." followed by "5 Warning(s)" and "0 Error(s)". Same 5
pre-existing `System.Reactive.PackagesConfigCheck.targets` warnings observed in the
P0-T11 analyzer-rebuild baseline (one per project referencing System.Reactive 7.0.0);
no nullable (CS86xx) diagnostics were promoted to errors, confirming a clean nullable
baseline with `/p:TreatWarningsAsErrors=true`.
