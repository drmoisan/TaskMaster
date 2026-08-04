# Member coverage branch-scope ledger restart

Timestamp: 2026-07-27T04-05
Command: Re-derived the live merge-base C# set with `git diff --name-only origin/main...HEAD -- '*.cs'` plus untracked authorized C# paths, ordered with `StringComparer.OrdinalIgnoreCase`, after the P8-T63 in-scope test-compilation correction.
EXIT_CODE: 0
Output Summary: The restarted C# ledger remains 65 paths and LF-joined SHA-256 `ACAE6BE16F5893475F0BA6B6147FB62AB7766A87DB85955C9976B21CBB5DA1B7`. The ordered path ledger intentionally hashes paths rather than file content. `QuickFiler.Test/QuickFiler.Test.csproj` remains an adjacent authorized non-C# entry and is excluded from that hash.
