# Final remediation CSharpier gate

- Timestamp (UTC): 2026-07-27T04:48Z
- Task: P9-T1
- Live merge base: `e63ddc7c18ca71e2c968b3329e42d965d45af1eb`
- Authorized C# scope: 65 paths from live merge-base diff plus untracked authorized C# paths, sorted with `StringComparer.OrdinalIgnoreCase`.
- LF-joined path SHA-256: `ACAE6BE16F5893475F0BA6B6147FB62AB7766A87DB85955C9976B21CBB5DA1B7`.
- P8-T61 comparison: matched its required 65-path count and `ACAE6BE16F5893475F0BA6B6147FB62AB7766A87DB85955C9976B21CBB5DA1B7` ledger value.
- Protected hashes before and after the CSharpier gate:
  - `coverage.config`: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`
  - `.csharpierignore`: `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25`

Commands on the exact authorized path set:

1. `csharpier format @authorized` — `EXIT_CODE=0`.
2. `csharpier check @authorized` — `EXIT_CODE=0`.
3. Stable verification: `csharpier format @authorized`, then `csharpier check @authorized` — both `EXIT_CODE=0`; aggregate path/content SHA-256 before and after formatting was `C83724468F097936E7E81D9249BE0FBC45EC59B10EB6BDA7DD68A35D8B763BD9`.

Result: no C# content delta after the stable formatting pass; all 65 authorized C# files pass CSharpier check.
