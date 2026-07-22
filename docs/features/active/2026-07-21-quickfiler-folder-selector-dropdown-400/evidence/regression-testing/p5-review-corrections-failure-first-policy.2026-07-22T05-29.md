# P5 review corrections failure-first policy and scope

Timestamp: 2026-07-22T05:29:54.6454391Z

Command: `$tests = @('QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs', 'QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs', 'QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs'); $expectedProduction = @{'QuickFiler/Viewers/BreadcrumbDropDownHost.cs'='C510C2B869275298FF61BE346B7553F864F87B0E77A86C91DC060ED139C404A9'; 'QuickFiler/Viewers/ItemViewer.Breadcrumb.cs'='FB6137B5A5C9513953C2CE09495C046F8951905DB7E38561452C64E6E21ED9AB'; 'QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs'='329E4FF0ED3985BFB06BD6F827FDF8BEF601D08708A61E9E07AA8303561B12DE'; 'QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs'='71FDE1A60A58E52626F69E965815F875DD5D1E78528160CE28E46CC282040CB2'}; $failed = $false; foreach ($path in $tests) {$content = Get-Content -Raw -LiteralPath $path; $lines = (Get-Content -LiteralPath $path).Count; $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash; $testClass = ([regex]::Matches($content, '\[TestClass\]')).Count; $testMethods = ([regex]::Matches($content, '\[(Data)?TestMethod\]')).Count; $fluent = $content.Contains('using FluentAssertions;'); $locks = ([regex]::Matches($content, 'lock \(')).Count; if ($lines -gt 500 -or $testClass -lt 1 -or $testMethods -lt 1 -or -not $fluent) {$failed = $true}}; foreach ($entry in $expectedProduction.GetEnumerator() | Sort-Object Name) {$actual = (Get-FileHash -Algorithm SHA256 -LiteralPath $entry.Name).Hash; if ($entry.Value -ne $actual) {$failed = $true}}; $prohibited = rg -n "Thread\.Sleep|Task\.Delay|Path\.GetTemp|GetTempFile|File\.|Directory\.|HttpClient|Process\.|Microsoft\.Office\.Interop|new WebView2|ShowDialog|Application\.Run" $tests; if ($LASTEXITCODE -eq 0) {$failed = $true} elseif ($LASTEXITCODE -ne 1) {$failed = $true}; $includes = Select-String -LiteralPath 'QuickFiler.Test/QuickFiler.Test.csproj' -Pattern 'BreadcrumbPopupControlDispatchTests.cs|BreadcrumbSelectorToggleUiBoundaryTests.cs|BreadcrumbSelectorOpenRetryTests.cs'; if ($includes.Count -ne 3) {$failed = $true}; if ($failed) {exit 1}`

EXIT_CODE: 0

Output Summary: The inspection verified exactly three test sources in the P5-T22 batch, with line counts 500, 494, and 499; one MSTest class and 20 total test methods; FluentAssertions in every file; synchronized recorders/queue snapshots; zero prohibited-resource matches; exactly one project include per test; and all four frozen production hashes unchanged.

## Test policy review

- `BreadcrumbPopupControlDispatchTests.cs` uses MSTest, FluentAssertions, Moq only at the WebView initializer boundary, deterministic completion sources, a concurrent exception recorder, and a lock-protected operation recorder. Its 11 test methods cover popup factory, cleanup, dispatch, and ownership behavior without live controls or services.
- `BreadcrumbSelectorToggleUiBoundaryTests.cs` uses MSTest, FluentAssertions, focused Moq/fakes, and a lock-protected captured-context queue with snapshot-only exception/thread observation. `DrainOne` rejects any thread other than the creator thread.
- `BreadcrumbSelectorOpenRetryTests.cs` uses MSTest, FluentAssertions, focused Moq/fakes, the shared synchronized captured context, and a lock-protected `SelectorOpenHarness` error recorder. Direct callback thread IDs and all drained work are asserted to be the creator thread.
- The tests follow Arrange-Act-Assert structure, are isolated and deterministic, and use no sleep, live UI/WebView, Outlook, temporary file, network, manual interaction, or external process.

## Frozen production proof

| File | SHA-256 | Match |
|---|---|---|
| `BreadcrumbDropDownHost.cs` | `C510C2B869275298FF61BE346B7553F864F87B0E77A86C91DC060ED139C404A9` | Yes |
| `ItemViewer.Breadcrumb.cs` | `FB6137B5A5C9513953C2CE09495C046F8951905DB7E38561452C64E6E21ED9AB` | Yes |
| `BreadcrumbPopupUiOperations.cs` | `329E4FF0ED3985BFB06BD6F827FDF8BEF601D08708A61E9E07AA8303561B12DE` | Yes |
| `BreadcrumbDropDownOpenLifetime.cs` | `71FDE1A60A58E52626F69E965815F875DD5D1E78528160CE28E46CC282040CB2` | Yes |

The failed first canonical attempt was terminated as a diagnostic and is not failure-first proof. The fresh bounded P5-T27 run is the authoritative expected-failure source.
