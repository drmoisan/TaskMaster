# Issue #608 C# policy-conflict reconciliation

Timestamp: 2026-08-25T12-50
Command: Read `AGENTS.md`, `.agents/skills/csharp/SKILL.md`, `.agents/skills/csharp-qa-gate/SKILL.md`, `scripts/vscode/Invoke-VSBuild.ps1`, and the failed global-nullable evidence.
EXIT_CODE: 0
Output Summary: The generated AGENTS instruction that globally enables nullable analysis conflicts with the current executable C# QA contract. The local per-file nullable gate is selected; global `/p:Nullable=enable` is prohibited.

The failed record `evidence/qa-gates/csharp-nullable.2026-08-25T12-33.md` reports exit code 1 and 195 legacy diagnostics after `/p:Nullable=enable` was applied globally. It identifies no Issue #608 file and does not authorize repair or suppression of those unrelated diagnostics.

Selected executable local type/nullable gate:

```text
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

Reconciliation basis:

- `.agents/skills/csharp/SKILL.md` requires the selected rebuild and explicitly prohibits `/p:Nullable=enable` because nullable is enabled per file.
- `.agents/skills/csharp-qa-gate/SKILL.md` repeats the selected command and prohibition.
- `scripts/vscode/Invoke-VSBuild.ps1` makes `-EnableNullable` a deprecated no-op and deliberately omits `/p:Nullable=enable`.

Change verification: this reconciliation changes no policy, code, test, project, or configuration file.
