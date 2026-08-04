---
name: csharpier-format-not-pipe-files-gate
description: C# formatting-gate plan tasks must use `csharpier format` + scoped `csharpier check` exit 0, never `csharpier pipe-files`, which writes to stdout only and is non-enforcing
metadata:
  type: feedback
---

When planning any C# CSharpier formatting gate task, require `csharpier format` (mutates files on disk) followed by a scoped `csharpier check` returning `EXIT_CODE: 0`. Never let a formatting/verification gate rely on `csharpier pipe-files`.

**Why:** In #400 P5, prior formatting gates invoked `csharpier pipe-files`, which writes formatted output to stdout only and never modifies files on disk. The evidence recorded a false "stable/no change" result. When authoritative `csharpier check`/`format` was finally run, exit code was 1 and two committed test files expanded past the 500-line hard limit once genuinely formatted (395->514 and 479->562 lines), forcing partial-class splits. A gate that never mutates and never asserts exit 0 cannot enforce formatting or line-limit policy.

**How to apply:** In plan task text for every remaining formatting task (all phases), spell out `csharpier format` then scoped `csharpier check` exit 0, and explicitly prohibit `pipe-files` as a gate. A single "Fixed execution rules" bullet can bind all remaining formatting tasks at once; still tighten any task whose wording is the ambiguous "Run CSharpier on ...". Note that `csharpier format` can expand line counts, so line-limit assertions must be evaluated post-format, not on committed source. Related: [[legacy-csproj-explicit-compile-include]] (partial-class split files each need one adjacent Compile include), [[plan-validator-task-id-sequential-constraint]] (inserting split-correction tasks mid-phase forces renumbering the unchecked tail + cross-refs).
