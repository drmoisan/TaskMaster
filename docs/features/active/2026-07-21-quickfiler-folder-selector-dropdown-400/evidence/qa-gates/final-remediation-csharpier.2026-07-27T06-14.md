# P9-T1 final remediation CSharpier gate after AC-18 reconciliation

Superseded pre-reconciliation formatter evidence: `final-remediation-csharpier.2026-07-27T04-48.md` and `final-remediation-csharpier.2026-07-27T06-08.md`.

The fresh authorized ledger contains 65 paths, LF-joined SHA-256 `ACAE6BE16F5893475F0BA6B6147FB62AB7766A87DB85955C9976B21CBB5DA1B7`, and both required SpamBayes paths.

```powershell
csharpier format @authorized
csharpier check @authorized
```

Both commands exited `0`; neither changed an authorized C# file. `coverage.config` remained `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`, and `.csharpierignore` remained `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25`.
