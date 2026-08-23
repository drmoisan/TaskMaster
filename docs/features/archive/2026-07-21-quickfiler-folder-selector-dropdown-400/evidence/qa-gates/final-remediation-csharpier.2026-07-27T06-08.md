# P9-T1 final remediation CSharpier gate

The superseded `final-remediation-csharpier.2026-07-27T04-48.md` remains historical evidence. This fresh run used merge base `e63ddc7c18ca71e2c968b3329e42d965d45af1eb` plus the allowed untracked C# paths, ordered with `StringComparer.OrdinalIgnoreCase`.

The authorized C# ledger contains 65 paths and LF-joined SHA-256 `ACAE6BE16F5893475F0BA6B6147FB62AB7766A87DB85955C9976B21CBB5DA1B7`, matching P8-T61.

Commands:

```powershell
csharpier format @authorized
csharpier check @authorized
```

Both commands exited `0`; neither changed any authorized C# file. Protected hashes remained unchanged:

- `coverage.config`: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`
- `.csharpierignore`: `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25`
