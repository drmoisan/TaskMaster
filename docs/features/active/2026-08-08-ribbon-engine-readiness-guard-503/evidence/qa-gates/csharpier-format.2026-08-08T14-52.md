# Phase 3 QC Step 1 — CSharpier Format (Scope-Locked) (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P3-T1]
Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' format TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs"`
EXIT_CODE: 0

## Phase 3 restart disclosure

This is the **second** execution of P3-T1. The first Phase 3 attempt was aborted at P3-T2, whose repo-wide `csharpier check .` failed on `TaskMaster\Ribbon\RibbonExplorer.xml`. The cause was the P2-T1 collapse: CSharpier 1.3.0 formats XML and mandates the multi-line form for those three lines once the `getEnabled` attribute pushes them from 78 to 116 characters against a 100-column print width. The collapse was reverted and the phase restarted from P3-T1, per the phase's own loop semantics. Full analysis: `evidence/qa-gates/f2-formatter-conflict.2026-08-08T14-52.md`.

The record below is the restarted pass. The tree at the start of this pass contains the F1 fix only; `RibbonExplorer.xml` takes a zero-line diff.

## Output Summary

```text
Formatted 1 files in 419ms.
```

**CSharpier did not rewrite the file.** The message reports the number of files *processed*, not the number changed. The content hash is identical on both sides of the invocation:

| Measurement | Before | After | Changed |
|---|---|---|---|
| `git hash-object` | `7d422ef399d5be44176acb629a0199bddcf6ff93` | `7d422ef399d5be44176acb629a0199bddcf6ff93` | **no** |
| Physical line count | 318 | 318 | no |

Because no file changed on disk, the Phase 3 loop does **not** restart from this step. The P1-T1 edit was already CSharpier-conformant as authored.

## Scope-guard compliance

The command was invoked with the explicit scope-locked path list from plan section 4.5 — exactly one `.cs` file:

```text
TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs
```

- **Never invoked repo-wide.** A repo-wide `csharpier format .` would reformat any file unformatted at the merge-base and break the AC15 zero-line-diff requirement. The read-only repo-wide `csharpier check .` gate is run separately at P3-T2.
- **`TaskMaster\AppGlobals\AppItemEngines.cs` and `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs` did not appear in the argument list.**
- **`TaskMaster\Ribbon\RibbonExplorer.xml` did not appear in the argument list.** Note that, contrary to plan section 3 rule 6, CSharpier 1.3.0 *does* format XML; the file is nevertheless excluded from the mutating pass because it is outside the scope-locked `.cs` list and is hand-edited.
- **`csharpier pipe-files` was not used.** It writes to stdout only, never mutates, and would produce a false "stable" result; it is prohibited as a gate.

Binary outcome satisfied: `EXIT_CODE: 0`.
