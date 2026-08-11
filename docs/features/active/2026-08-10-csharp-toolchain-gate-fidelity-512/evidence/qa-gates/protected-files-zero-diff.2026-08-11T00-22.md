# AC9 — protected files verified unmodified against the merge base ([P5-T13])

Timestamp: 2026-08-11T00-22
Command: `git diff <MERGE_BASE> -- .claude/rules/general-unit-test.md .claude/rules/quality-tiers.md`; `sha256sum` of both files; the [P3-T6] § UT2 extract comparison re-run; `git diff <MERGE_BASE> --name-only`
EXIT_CODE: 0

`MERGE_BASE` = `a5e336e5ae3443d4197caf5f87036fae1d538f89`
(`FEATURE/evidence/baseline/baseline-git-context.2026-08-10T22-35.md`).

## 1. Zero-line diff for the two protected rule files

```
$ git diff a5e336e5ae3443d4197caf5f87036fae1d538f89 -- .claude/rules/general-unit-test.md .claude/rules/quality-tiers.md
(no output)
```

**Zero output lines.**

SHA-256 fingerprints, compared against the [P0-T3] pre-change capture:

| File | [P0-T3] SHA-256 | Final SHA-256 | Identical |
|---|---|---|---|
| `.claude/rules/general-unit-test.md` | `8c30af5a659b8bfa28195f77c90f712c8e0c2a6a6932c93ed143a475ee4f68b0` | `8c30af5a659b8bfa28195f77c90f712c8e0c2a6a6932c93ed143a475ee4f68b0` | **YES** |
| `.claude/rules/quality-tiers.md` | `25c79d4380d208364534f75b234b2e4bdc35619e342301c02093fd0e8ec49654` | `25c79d4380d208364534f75b234b2e4bdc35619e342301c02093fd0e8ec49654` | **YES** |

## 2. `CLAUDE.md` § UT2 extract re-verified against the final working tree

The [P3-T6] comparison was re-run against the final tree, using the byte-exact SHA-256 method (not
the PowerShell `Compare-Object` method, which produced an encoding false positive recorded in
`FEATURE/evidence/qa-gates/claudemd-ut2-guard.2026-08-10T23-42.md`):

```
$ git show <MERGE_BASE>:CLAUDE.md | sed -n '/^### UT2. Coverage and Scenarios/,/^### UT3. Test Structure and Diagnostics/p' | sha256sum
d4d95bbfc15578320e4996cf0d9872c4cee1ea6d9a9ae08cc282acdf62dbdf68

$ sed -n '/^### UT2. Coverage and Scenarios/,/^### UT3. Test Structure and Diagnostics/p' CLAUDE.md | sha256sum
d4d95bbfc15578320e4996cf0d9872c4cee1ea6d9a9ae08cc282acdf62dbdf68
```

**Byte-identical.** 28 lines / 2262 bytes on both sides. `CLAUDE.md` § UT2 is untouched by this
feature, which is what allows sibling feature #494 to edit that section without a merge conflict.

## 3. No excluded system appears in the changed-file set

`git diff <MERGE_BASE> --name-only`:

```
.claude/rules/csharp.md
.claude/skills/csharp-qa-gate/SKILL.md
.vscode/tasks.json
CLAUDE.md
docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/plan.2026-08-10T14-08.md
scripts/vscode/Invoke-VSBuild.ps1
tests/scripts/vscode/Invoke-VSBuild.Tests.ps1
```

| Excluded path (SD1 / SD4) | Present in the changed-file set? |
|---|---|
| `AGENTS.md` | **no** |
| `.agents/` | **no** |
| `.github/instructions/` | **no** |
| `.github/agents/` | **no** |
| `.codex/` | **no** |
| `.github/workflows/ci.yml` | **no** |
| `.claude/rules/general-unit-test.md` | **no** |
| `.claude/rules/quality-tiers.md` | **no** |

## Output Summary

AC9 is satisfied. `git diff <MERGE_BASE>` produces **zero** output lines for
`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`, and both files' SHA-256
fingerprints are unchanged from the [P0-T3] pre-change capture. The `CLAUDE.md` § UT2 extract is
byte-identical to its merge-base text (identical SHA-256 `d4d95bbf...`, 28 lines, 2262 bytes). No
SD1- or SD4-excluded path appears in `git diff <MERGE_BASE> --name-only`.
