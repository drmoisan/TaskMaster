# P5 Open Coordinator Preservation CSharpier

Timestamp: 2026-07-22T10:23:00Z

Command: `$files=@((Resolve-Path 'QuickFiler/Viewers/BreadcrumbDropDownHost.cs').Path,(Resolve-Path 'QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs').Path); $tool='C:\Users\DanMoisan\.dotnet\tools\csharpier.exe'; @($files) | & $tool pipe-files`

EXIT_CODE: 0

Output Summary: PASS. CSharpier ran twice against exactly the authorized Host and integration-test files. The first pass retained Host SHA-256 `17E186B7EE7F684A2310BD06A9787D29884F3CE6B4D25BD83EDB3000EC718C4A` at 472 physical lines and integration-test SHA-256 `B614351681956E2A9427412807FD6F22B270A6C7B6C6F2D331468241D4BFD990` at exactly 500 physical lines. The second pass returned exit code 0 and retained both hashes, proving stable formatter output. No file outside the exact two-file scope was passed to CSharpier.
