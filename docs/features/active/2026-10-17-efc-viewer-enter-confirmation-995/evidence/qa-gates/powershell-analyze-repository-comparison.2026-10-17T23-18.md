# Issue 995 powershell-analyze-repository-comparison

Timestamp: 2026-10-17T23-18
Command: Invoke-ScriptAnalyzer broad scope with bundled settings; compare diagnostic path/line/rule/severity tuples to captured baseline
EXIT_CODE: 0
Output Summary: Analyzed files=109; findings=13.
Install-RepoDotNetSdk.ps1:59 PSAvoidUsingWriteHost File 'Install-RepoDotNetSdk.ps1' uses Write-Host. Avoid using Write-Host because it might not work in all hosts, does not work when there is no host, and (prior to PS 5.0) cannot be suppressed, captured, or redirected. Instead, use Write-Output, Write-Verbose, or Write-Information.
Install-RepoDotNetSdk.ps1:79 PSAvoidUsingWriteHost File 'Install-RepoDotNetSdk.ps1' uses Write-Host. Avoid using Write-Host because it might not work in all hosts, does not work when there is no host, and (prior to PS 5.0) cannot be suppressed, captured, or redirected. Instead, use Write-Output, Write-Verbose, or Write-Information.
Install-RepoDotNetSdk.ps1:106 PSAvoidUsingWriteHost File 'Install-RepoDotNetSdk.ps1' uses Write-Host. Avoid using Write-Host because it might not work in all hosts, does not work when there is no host, and (prior to PS 5.0) cannot be suppressed, captured, or redirected. Instead, use Write-Output, Write-Verbose, or Write-Information.
Install-RepoDotNetSdk.ps1:26 PSUseOutputTypeCorrectly The cmdlet 'Get-RepoDotNetSdkDownloadUrl' returns an object of type 'System.String' but this type is not declared in the OutputType attribute.
Install-RepoDotNetSdk.ps1:36 PSUseOutputTypeCorrectly The cmdlet 'Get-RepoDotNetSdkInstallDir' returns an object of type 'System.String' but this type is not declared in the OutputType attribute.
Install-RepoDotNetSdk.ps1:39 PSUseOutputTypeCorrectly The cmdlet 'Get-RepoDotNetSdkInstallDir' returns an object of type 'System.String' but this type is not declared in the OutputType attribute.
Invoke-MSTest.ps1:210 PSAvoidUsingWriteHost File 'Invoke-MSTest.ps1' uses Write-Host. Avoid using Write-Host because it might not work in all hosts, does not work when there is no host, and (prior to PS 5.0) cannot be suppressed, captured, or redirected. Instead, use Write-Output, Write-Verbose, or Write-Information.
Invoke-MSTest.ps1:211 PSAvoidUsingWriteHost File 'Invoke-MSTest.ps1' uses Write-Host. Avoid using Write-Host because it might not work in all hosts, does not work when there is no host, and (prior to PS 5.0) cannot be suppressed, captured, or redirected. Instead, use Write-Output, Write-Verbose, or Write-Information.
Invoke-MSTestWithCoverage.Helpers.ps1:139 PSUseSingularNouns The cmdlet 'Get-CoberturaLineConditionCoverageParts' uses a plural noun. A singular noun should be used instead.
Invoke-Restore.ps1:101 PSAvoidUsingWriteHost File 'Invoke-Restore.ps1' uses Write-Host. Avoid using Write-Host because it might not work in all hosts, does not work when there is no host, and (prior to PS 5.0) cannot be suppressed, captured, or redirected. Instead, use Write-Output, Write-Verbose, or Write-Information.
Invoke-VSBuild.ps1:245 PSAvoidUsingWriteHost File 'Invoke-VSBuild.ps1' uses Write-Host. Avoid using Write-Host because it might not work in all hosts, does not work when there is no host, and (prior to PS 5.0) cannot be suppressed, captured, or redirected. Instead, use Write-Output, Write-Verbose, or Write-Information.
Invoke-VSBuild.ps1:52 PSUseSingularNouns The cmdlet 'Get-MSBuildBuildArguments' uses a plural noun. A singular noun should be used instead.
Invoke-VSBuild.ps1:87 PSUseSingularNouns The cmdlet 'Get-RequestedMSBuildProperties' uses a plural noun. A singular noun should be used instead.
Baseline13 findings:10warnings,3informational,0errors/parseerrors; broad baseline and final analyzer exits1. Diagnostic delta=0; unchanged scripts preserved; no suppressions added.
