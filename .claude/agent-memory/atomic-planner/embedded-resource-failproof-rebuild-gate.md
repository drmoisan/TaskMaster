---
name: embedded-resource-failproof-rebuild-gate
description: Fail-proof mutations of an embedded resource need edit -> rebuild -> assert-embedded-bytes -> run-test; without the assert step the "failing" run silently passes against a stale assembly
metadata:
  type: feedback
---

When a plan proves a test non-vacuous by temporarily mutating an **embedded resource** (e.g. `TaskMaster\Ribbon\RibbonExplorer.xml`, read via `assembly.GetManifestResourceStream(...)`), the mutation is invisible to the test until the owning assembly is rebuilt and re-copied into the test output directory. Structure the phase as four separate tasks: edit → rebuild → **assert the embedded byte content** → run the `[expect-fail]` test. Make the assert step a hard gate with an explicit `/t:Rebuild` fallback, and make the restoration (`git checkout -- <path>`) its own verified task.

Assert the embedded content from a byte-loaded assembly so no file lock blocks the next rebuild:

```powershell
$asm = [System.Reflection.Assembly]::Load([System.IO.File]::ReadAllBytes($dll))
$text = (New-Object System.IO.StreamReader($asm.GetManifestResourceStream($resourceName))).ReadToEnd()
```

**Why:** Without the assert step, an edit-then-run sequence reads a stale assembly and reports Pass. That converts the fail-proof itself into a second vacuous check — exactly the defect the cycle exists to remove (#503 F1, remediation cycle 2026-08-08T14-26). `LoadFrom` would lock the DLL and break the subsequent rebuild.

**How to apply:** Any `[expect-fail]` task whose subject is a resource, embedded asset, satellite file, or generated artifact rather than a `.cs` file. Also choose a mutation target that is already in the shape you want (a whole-line deletion from an already-multi-line element) so the mutation's `git diff --numstat` gate is exactly `0 1` and does not entangle with a sibling finding's edit. Related: [[diff-gates-need-a-commit-task]], [[project-503-ribbon-readiness-plan-seams]].
