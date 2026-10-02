---
name: project-953-r1-pwsh-refused-and-count-recheck-seams
description: "#953 R1 preflight deltas - the Bash isolation guard refuses pwsh so every pwsh one-liner needs an od/rm fallback and git -C <root>; a -A 1 block count must be re-derived per file (one app.config carried no block); Glob counts over two folders must be re-run not recalled; a count-0 gate on a token the module's own comment repeats cannot pass; -o on a pattern ending at Version= prints no version; 'packages.config' pathspec is root-only; Mandatory [string[]] needs [AllowEmptyString()] for empty elements; git ls-files --eol fields are space-padded with one tab before the path"
metadata:
  type: project
---

Round 1 preflight deltas on the issue #953 plan (worktree `.claude/worktrees/agent-a5292122c820774d3`, 2026-10-02). Eleven defects, no task-count change (47). Each one was a figure or command shape I carried forward from the orchestrator's directive or from reasoning instead of observing it in this worktree.

**Why:** every delta was detectable before handoff by one Grep, one Glob, or one read of the tool's actual output.

**How to apply:** before any handoff, re-run every census count the plan asserts (per file, not from the directive), and for every shell command either observe its success-case output or write the fallback the executor will need when the Bash tool refuses the binary.

1. **pwsh is refused by the Bash worktree-isolation guard, in one-liner form too.** `od -A n -t u1 -N 3 <abs path>` prints ` 239 187 191` for a BOM file, `rm -f <abs path>` exits 0, `git -C <root> ...` runs. Write the fallback into the CMD definition, make every file operand absolute (`<execution-worktree-root>/<path>`), state in C5 that every `git` is `git -C <execution-worktree-root>`, and have the executor record `PWSH-REFUSED: <tool message>` so the fallback is not a deviation.
2. **A `-A 1` block census must be counted per file.** `name="System.ClientModel"` over `*/app.config` gives 16 blocks (6 at 1.3.0.0, 10 at 1.16.0.0); SVGControl/app.config (23 lines) carries none. I had written 17/11 by assuming every one of the 17 configs carried the block. The shared-string count (17 = 11 Fizzler + 6 ClientModel) was right and is a different figure; search the plan for both when correcting one.
3. **Glob the hash-set folders; do not recall the count.** `scripts/dependencies` has 6 `.ps1/.psm1` and `tests/scripts/dependencies` 8 (14, 16 after two new files); I wrote 13/15.
4. **A count-0 gate on `-Force` fails on the module's own comment** that explains why `-Force` is omitted. Gate the statement line instead: `^Import-Module ` count 1 and `^Import-Module .*Force` count 0.
5. **`-o` on a pattern that ends at `Version=` prints no version.** Extend the pattern through the value (`Version=[^,"]+`) so the printed match carries the figure the gate asserts. Multiline count mode reports one count per file: assert "N files, each count 1" rather than a bare total.
6. **`git ls-files --eol` output is `i/lf    w/crlf  attr/text=auto<TAB>path`** - the three fields are space-padded, only the path is tab-preceded. Describe the gate as "second whitespace-separated field", not tab fields.
7. **Pathspec `'packages.config'` matches only the repository root.** Use `'*packages.config'` (git pathspec `*` crosses `/`).
8. **Mandatory `[string[]]` rejects an empty element at binding** unless `[AllowEmptyString()]` is present, so help text claiming "the parser rejects empty text" is false without it. Add the attribute and state the real two-step behaviour; cite the parser's own test (PackageGraph.Tests.ps1 line 268) instead of adding an It that would shift every test-count gate.
9. **Pin the test-file import form.** The repo form is `Import-Module (Join-Path $script:RepoRoot '<module>') -Force` (RepositoryTreeConsistency.Tests.ps1 line 5); quote the exact two lines in the spec and gate them with anchored patterns, and require BeforeAll fixtures to be `$script:` variables.
10. **Ratchet drift must be detectable at fail-before, not after all edits.** Pester renders `Should -Be` failures as `Expected ..., because <text>, but got ...`, so a `-Because` that appends `observed: <sorted list -join '; '>` lets the fail-before task gate the exact 16-entry list between `observed: ` and `, but got`.
11. **A repository-level ratchet test is not an edge test for the helper it calls.** Map each exported function to its own It numbers (10 positive, 11 negative, 12 edge), not to the integration test.
12. **Coverage Evidence Contract exception must be named where the figures would sit.** When no local coverage route instruments the folder, cite the design decision (D8) in the baseline test task and the handoff task with a literal line stating the exception and the reason.

Related: [[project-953-fizzler-redirect-sweep-and-ratchet-plan-seams]] (round 0 seams), [[project_pwsh_refused_in_isolated_worktree_agents]], [[powershell-gate-observables]], [[verify-line-spans-and-computed-literals]].
