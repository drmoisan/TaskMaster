---
name: project-927-evidence-identity-hygiene-sweep-plan-seams
description: "#927 (repository-wide raw-document removal, identifier redaction, CI hygiene guard) plan seams - AC1 per-file arithmetic forces one profile-path finding per file and a git-grep-I-equivalent binary skip; the wrapper-throw test needs a pure Assert-GitExitCode seam (never invoke git); the removal list comes from the guard's own raw-document lines; new *cobertura*.xml ignore pattern collides with the runner's default projection name; CITATION paths need a slash (.gitignore cannot be cited); backticked non-path tokens with slashes (i/-text, scan_folders arrays, regexes) are harvested as blast radius"
metadata:
  type: project
---

Seams found authoring the #927 plan (2026-09-28, worktree agent-a02e6a7cf1d674eb9, branch
bug/evidence-and-identity-hygiene-sweep-927, base 177b6d78e), a full-bug item with 20 ACs, 104 tasks.

**Why:** each seam made a spec clause unsatisfiable as literally read, or would have drawn a preflight
or blast-radius finding.

**How to apply:** any plan that builds a content-classifying guard over `git ls-files`, redacts
identifiers repository-wide, or wires a new CI callee.

1. **AC arithmetic fixes the guard's counting unit.** Spec AC1 said findings = raw-document population
   + profile-path FILE population; the guard's function returns per-LINE matches. Resolve by emitting one
   profile-path finding per file (first line number) and record it as a decision; a raw document that
   also carries a path contributes two findings, matching the overlap of the two gate populations.
2. **Binary handling must mirror `git grep -I` or AC equality breaks.** Skip `ls-files --eol` records
   reading i/-text unless the first two bytes are a UTF-16 BOM; decode by BOM otherwise; never use a
   throwing UTF-8 decoder (a Latin-1 tracked file would become a permanent finding). Make the
   `unreadable` rule fire only when the injected content delegate throws, pinned by a seventh test.
3. **"throws when the git wrapper reports a non-zero exit" cannot invoke git.** Split the throw into a
   pure `Assert-GitExitCode -ExitCode -GitArgs` called by `Invoke-GitExe`; the test exercises the pure
   function; the three splat lines stay uncovered like every existing wrapper.
4. **Derive the removal list from the guard's own output** (`HYGIENE raw-document <path>` lines from the
   expect-fail pre-sweep run, saved to scratch, driven through `git --literal-pathspecs rm
   --pathspec-from-file`), not from a list switch: no default-behaviour change and the CI classifier is
   the one that drove the removal. `git grep` reads the working tree for modified tracked files, so the
   post-rm and post-redaction gates measure the uncommitted state correctly.
5. **`*cobertura*.xml` in .gitignore also matches the coverage runner's default projection file name**
   (coverage.cobertura.jacoco.xml). None of the 18 retained projections carries the marker (all end
   .jacoco.xml or start coverage-/p0-/p2-/p5-/p9-), so it is safe now, but a future evidence copy must
   be renamed. Record it as a decision.
6. **The planner-output hook's CITATION path regex requires at least one slash**
   (`(?:/[^/\\|\s]+)+`), so a root file such as .gitignore cannot be a CITATION line; cite it only in
   the prose enumeration.
7. **Blast-radius harvest catches backticked non-paths.** `i/-text`, `["tests/scripts/vscode"]`,
   `@("scripts/hygiene")`, `.claude/`, `/out:obj\Debug\`, `coverage/*`, `!coverage/.gitkeep`, a regex
   with `[\s>]`, a branch name with a slash - all whitespace-free backticked tokens with a separator.
   Sweep with `` `[^`\s]*[\\/][^`\s]*` `` after drafting and unbacktick everything that is not a Write
   Set or evidence path.
8. **Keep the scope gate's Write Set parser bounded to the appendix section** (from `## Write Set` to
   the next `## ` heading). The self-review record that follows the appendix carries backticked tokens
   the parser would otherwise admit as exact paths. Build the backtick regex from `[char]96` so the
   command span itself carries no backticks.
9. **`$env:` and `$GitArgs` inside a double-quoted probe literal expand to nothing**; compose them with
   `[char]36`. A trailing space in `).Count` (`). Count`) is a parse error; re-read every probe.
10. **The MSTest coverage route DID complete locally on 2026-09-13** (873 evidence: 7222 tests, 0
    failed, 85.71 / 79.87 first-party), so the 2026-09-04 shell-icon hang memory is not a standing
    fact; plan the route with a 45-minute stall branch that records STALLED and defers to the CI
    context the spec names as authoritative, never an unconditional exit-0 demand.
11. **The Pester fail-before run's non-zero count rests on Pester marking every It in a block whose
    `BeforeAll` dot-source throws as Failed**; hedge the exit with `FailedCount -gt 0 -or PassedCount
    -eq 0` and enumerate the failed names.
12. **P4 stop conditions belong in the conventions list** (host token ambiguous, redaction outside the
    Write Set, format outside the Write Set) or C12's "stops exist only in Phase 0" contradicts the tasks.

Related: [[project_602_host_identifier_sweep_plan_seams]], [[_shared_no_absolute_host_paths]],
[[validate-planner-output-hook-line-anchored-gotchas]], [[poshqc-mcp-and-msbuild-invocation-facts]],
[[msbuild-task-csc-literal-needs-detailed-verbosity]], [[pester-invoke-does-not-exit-nonzero]].
