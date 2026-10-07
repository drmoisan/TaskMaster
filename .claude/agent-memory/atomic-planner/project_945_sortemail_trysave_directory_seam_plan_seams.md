---
name: project_945_sortemail_trysave_directory_seam_plan_seams
description: #945 R0 minimal-audit plan seams - whitespace-stripped token census, compile-red fail-before, uncommitted-fix control via backup copy, no-commit plan with two-dot MERGE-BASE gates
metadata:
  type: project
---

Plan for #945 (SortEmail.TrySaveAttachmentAsync gets an `Action<string> createDirectory` overload; test rewritten). 45 tasks, 3 phases, no commits.

- Census tokens are counted over whitespace-STRIPPED file text (`[regex]::Replace($raw, "\s+", "")`), so every token is written without spaces and survives CSharpier wrapping; comments must not repeat a token (a `<see cref>` to the method would inflate the `TrySaveAttachmentAsync(` count).
- Fail-before: a compiling declaration-only stub would forward to the real `Directory.CreateDirectory` on a rooted C: path (creates a dir, or raises UnauthorizedAccessException and reaches `YesNoToAll.ShowDialog`). So tests are written first, the build is observed RED (`[expect-fail]`, gate on an error line naming the method, CS1501 only an observation because an extension call's message counts explicit arguments), and a fail-before-exception dossier is written; the side-effect-free control is the runtime equivalent.
- Uncommitted fix means `git checkout` cannot restore it: back up the fixed file to a git-ignored coverage/ path, mutate byte-preservingly (Latin-1 round trip, needle count must be 1), restore by copy, prove by SHA-256 plus the hash of the anchored `git diff` text.
- Removing the seam call fails BOTH new tests (T-B never sees its throwing seam), so the control gates both as Failed.
- RUNNER coverage route finds no assemblies from a `.claude/worktrees` worktree (issue #752): always the DIRECT route; the stall probe only selects the exclusion text.
- AC8 with no commit: two-dot `git diff --name-only <MERGE-BASE>` (working tree vs recorded SHA) paired with `git status --porcelain --untracked-files=all`; Clause A (inherited at P0) and Clause B (.claude/agent-memory/) subtracted; footprint must contain both source files (positive control).
- Write-set baseline diagnostics on a legacy 1429-line file: gate final count <= baseline count, never require 0.
- The MCP plan validator was not in this tool surface; the plan was checked by reading against the hook and gate rules.
