---
description: 'Remediate a specified scope so docstrings and intent comments comply with self-explanatory-code-commenting rules, following all repo policies.'
---

# Comment Remediation Loader

Use this command to launch the `commentary-remediator` skill on any scope (single file, folder, or glob) and enforce `.claude/skills/self-explanatory-code-commenting/SKILL.md` (or equivalent) while obeying all repo policies.

## Inputs

- **Scope** (required): Path or glob to remediate (e.g., `src/SomeModule/SomeSubsystem/**`).

## Policy Order

Apply in this sequence:
1. `CLAUDE.md`
2. `general-code-change-policy` skill
3. `general-unit-test-policy` skill
4. Language-specific code-change policy skill
5. Language-specific unit-test policy skill
6. `self-explanatory-code-commenting` policy skill

## Execution Rules

- Remediate the entire scope to add/adjust robust docstrings and intent comments for loops, branches, and multi-step blocks; avoid low-value narration.
- Keep modules cohesive, under 500 lines, strongly typed; avoid suppressed nullable warnings unless justified.
- Maintain encoding/EOL and ASCII unless the file already uses Unicode.
- Do not pause for approval or clarification once scope is set; continue across turns if needed until fully compliant.

## Workflow

1. **Context pass**: Identify files in scope and gaps; capture tasks.
2. **Remediate**: Add or refine docstrings and intent comments; refactor only when needed for clarity.
3. **Validate (loop until clean)**:
   - `dotnet tool run csharpier .`
   - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
   - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
   - `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
   Restart from the formatting step after any change or failure; resolve all errors before finishing.

## Completion Criteria

- Scope complies with self-explanatory commenting guidance and repo policies.
- Final toolchain pass is green.
- Provide a concise summary and list of commands run.

## Launch Template

```
Scope: <INSERT SCOPE PATH OR GLOB>
```
