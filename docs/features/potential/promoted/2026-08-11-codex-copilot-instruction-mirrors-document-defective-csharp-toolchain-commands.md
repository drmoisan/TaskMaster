# codex-copilot-instruction-mirrors-document-defective-csharp-toolchain-commands (Issue #535)

- Date captured: 2026-08-11
- Author: Dan Moisan
- Status: Promoted -> GitHub issue #535

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #535
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/535
- Last Updated: 2026-08-11
## Summary

Feature `2026-08-10-csharp-toolchain-gate-fidelity-512` (issues #492, #509, #512, #522) corrected the
C# format, analyzer and type-check commands in `CLAUDE.md`, `.claude/rules/csharp.md` and
`.claude/skills/csharp-qa-gate/SKILL.md`. It deliberately excluded the Codex/Copilot instruction
mirror tree under scope decision SD1. Those mirrors still document the CSharpier v0 bare-path form
(`csharpier .`) and the unpassable `/t:Build ... /p:Nullable=enable` nullable command. After that
feature merges, the mirrors become the **only** sites in the repository that disagree with
`.github/workflows/ci.yml`, so the divergence becomes host-dependent (Claude sessions read the
corrected files; Codex/Copilot sessions read the stale mirrors) and correspondingly harder to
diagnose.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1; MSBuild from Visual Studio 18 (Community); repo-pinned .NET SDK 8.0.205; CSharpier pinned to 1.2.6 in `dotnet-tools.json`
- Command/flags used: `git grep -n -E 'csharpier[[:space:]]+\.'` and `git grep -n -F 'Nullable=enable'` over tracked files
- Data source or fixture: the repository's own governance and instruction files

## Steps to Reproduce

1. From the repository root, run
   `git grep -n -E 'csharpier[[:space:]]+\.' -- ':!docs/features' ':!docs/research' ':!.claude/agent-memory'`
2. Run
   `git grep -n -E '(/t:Build.*Nullable=enable|Nullable=enable.*/t:Build)' -- ':!docs/features' ':!docs/research' ':!.claude/agent-memory'`
3. Observe that after feature 512 merges, every remaining hit is inside the mirror tree enumerated
   below.

## Expected Behavior

Every site in the repository that documents a mandatory toolchain command documents a command that
executes against the pinned toolchain, enforces the policy it names, and agrees with
`.github/workflows/ci.yml`, regardless of which agent host reads it.

## Actual Behavior

Eight mirror files still document at least one defective command. Verified by grep on 2026-08-10 and
re-verified on 2026-08-11 after feature 512's corrections
(`docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/evidence/qa-gates/site-inventory-reconciled.2026-08-11T00-18.md`):

| Path | Lines | Defect |
|---|---|---|
| `AGENTS.md` | 466 (false rationale), 469, 470 (format), 487, 488 (type-check), 660 (format), 662 (type-check) | all |
| `.github/instructions/csharp-code-change.instructions.md` | 29 (false rationale), 32, 33 (format), 50, 51 (type-check) | all |
| `.github/instructions/csharp-unit-test.instructions.md` | 45 (format), 47 (type-check) | format + type-check |
| `.agents/skills/csharp/SKILL.md` | 17 (format), 19 (type-check) | format + type-check |
| `.agents/skills/csharp-qa-gate/SKILL.md` | 32 (format), 34 (type-check) | format + type-check |
| `.github/agents/csharp-typed-engineer.agent.md` | 172 (format), 174 (type-check) | format + type-check |
| `.github/agents/csharp-atomic-executor.agent.md` | 258 (format), 260 (`dotnet build -p:Nullable=enable`) | format + type-check |
| `.codex/codex-web-setup.sh` | 342 (printed follow-up command inside the heredoc ending at line 348) | type-check |

Note on `.codex/codex-web-setup.sh:342`: it is a **documentation** carrier, not an executable one —
lines 336-347 sit inside a heredoc terminated at line 348 and are printed as "useful follow-up
commands", not executed. Feature 512 handled its practical consequence by retaining
`-EnableNullable` on `scripts/vscode/Invoke-VSBuild.ps1` as a warning-emitting no-op, so the printed
command still binds.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: after feature 512's corrections, `git grep -n -E 'csharpier[[:space:]]+\.'` over tracked
  files (excluding `docs/features`, `docs/research` and `.claude/agent-memory`) returns **10** hits,
  all inside the mirror tree; the same-line `/t:Build` + `Nullable=enable` grep returns **9** hits,
  all inside the mirror tree.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

A Codex- or Copilot-driven session reading a mirror will still run the unpassable nullable command
and can still manufacture the false blocking `CS86xx` findings that required human-level override on
deliveries #507 and #508 on 2026-08-08. It will also run a format command that returns exit 1 and
formats nothing.

## Suspected Cause / Notes

Recorded as scope decision SD1 of
`docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/spec.md`. The exclusion rests on
four grounds, in descending strength:

1. **`.github/instructions/` sits under an unsuspended hard constraint.**
   `.claude/skills/policy-compliance-order/SKILL.md` states: "Do NOT modify policy documents under
   `.claude/rules/` **or `.github/instructions/`**." The epic
   `build-ci-coverage-gate-fidelity`'s "Execution Authorization Required" section suspended that
   constraint for feature 512 only for `CLAUDE.md` and `.claude/rules/csharp.md`.
   **`.github/instructions/` therefore requires its own authorization grant, equivalent to the one
   the epic issued for `.claude/rules/csharp.md`.**
2. **`AGENTS.md` forbids manual editing and its generator does not exist here.** `AGENTS.md` lines
   3-27 declare the file generated from seventeen `.github/**` sources, say "Do not edit this file
   manually", and name `scripts/dev-tools/sync-agents-from-instructions.ps1` as the regeneration
   command. **`scripts/dev-tools/` contains exactly one file, `run-actionlint.ps1`; the named
   generator does not exist in this repository.** Hand-editing violates the file's own contract;
   regenerating is impossible here.
3. **`.agents/`, `.codex/` and `.github/agents/` are inbound artifacts of a different repository.**
   They self-describe as Codex push-down resources installed by the `drm-copilot` MCP tool
   `push_down_codex_and_agents_customizations`. **`drm-copilot` is the owning repository for
   `.agents/`, `.codex/` and `.github/agents/`.** An edit here is not durable; the next push-down
   overwrites it.
4. `.github/agents/**` is excluded on ground 3 only. If a reviewer disputes `drm-copilot` ownership
   of `.github/agents/`, this issue is the correct place to resolve it.

### Additional residuals folded into this entry

- **`.csharpierignore` comment residual.** `.csharpierignore` lines 1-3 and 9-11 repeat the same
  false premise feature 512 removed from `CLAUDE.md` ("CSharpier formats C# source only (per
  CLAUDE.md C#1)"). The ignore **rules** are correct; only the explanatory comment is wrong. The file
  is outside feature 512's enumerated documentation sites.
- **`TaskMaster/Ribbon/EngineCommandCatalog.cs:93` comment residual.** The comment reads
  `// returns false. This keeps the file clean under /p:Nullable=enable.` It cites
  `/p:Nullable=enable` as the enforcing gate, which is **false after feature 512** — the gate now
  relies on the per-file `#nullable enable` pragma plus `/p:TreatWarningsAsErrors=true`. It could not
  be corrected in feature 512 because that feature makes **no** `*.cs` change.
- **`.claude/rules/powershell.md:18` residual.** That line states the Pester step should "use repo
  config at `scripts/powershell/PoshQC/settings/pester.runsettings.psd1`". **That path does not exist
  in this checkout**: `ls scripts/` returns only `dev-tools/`, `temp-extract-coverage.ps1` and
  `vscode/`; `find . -name "pester.runsettings*"` and `find . -type d -name "PoshQC"` both return no
  matches. PoshQC is entirely MCP-server-side here. This is the same class of defect as feature 512's
  — a documented command referencing something that does not exist — in a different rule file, and
  `.claude/rules/powershell.md` was not covered by the epic's authorization. Recorded in
  `docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/evidence/baseline/baseline-powershell-toolchain.2026-08-10T15-40.md`.

**PowerShell coverage shortfall:** none to fold in. Feature 512's [P0-T16] measured line coverage of
`scripts/vscode/Invoke-VSBuild.ps1` at **85.71%**, at or above the 85% policy floor, so **no
`PREEXISTING_COVERAGE_SHORTFALL:` was recorded** and [P6-T4] had nothing to fold into this entry.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: not applicable — documentation corrections. If a toolchain-command lint is
      added, assert that every documented command in every instruction mirror is executable against
      the pinned tool versions and agrees with `.github/workflows/ci.yml`.
- [ ] Integration scenario to retest: after correction, re-run the three greps from feature 512's
      [P5-T11] and confirm zero hits outside the permitted residual classes.
- [ ] Manual verification notes: (a) obtain an explicit authorization grant for
      `.github/instructions/` equivalent to the epic's grant for `.claude/rules/csharp.md`;
      (b) correct `.github/instructions/*.instructions.md` under that grant; (c) either restore the
      `AGENTS.md` generator or agree a policy for maintaining the generated file; (d) raise the
      `.agents/`, `.codex/` and `.github/agents/` corrections upstream in `drm-copilot` so the next
      push-down carries them; (e) correct the `.csharpierignore` comment, the
      `EngineCommandCatalog.cs:93` comment, and `.claude/rules/powershell.md:18`.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
