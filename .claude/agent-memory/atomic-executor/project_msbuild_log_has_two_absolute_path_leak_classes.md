---
name: msbuild-log-has-two-absolute-path-leak-classes
description: A raw MSBuild file-logger log carries TWO distinct absolute host paths — the worktree root AND the main-checkout root via csc.exe /analyzerconfig: — so a worktree-root-only sanitisation leaves ~36 leaks per log
metadata:
  type: project
---

Before committing a raw MSBuild file-logger log as evidence, sanitise **two** absolute-path
prefixes, not one:

1. **Worktree root** — printed on effectively every diagnostic line (path to the `.sln` and to
   every project file).
2. **Main-checkout root** — embedded by every `csc.exe` compiler-invocation line via an
   `/analyzerconfig:` argument pointing at the *ancestor* checkout's `.editorconfig`. MSBuild
   emits two `/analyzerconfig:` arguments back to back for the same file: the main-checkout one
   and the worktree-rooted one.

**Why:** when running inside a linked worktree, the main-checkout root is an *ancestor* of the
worktree root, so it is not itself prefixed by the worktree-root string and a worktree-root-only
substitution never matches it. Measured on issue #730 (2026-09-02): 36 `csc.exe` invocations per
log x 4 logs = 144 residual account-token matches survived a sanitisation that reported the
worktree-root class fully clean.

**How to apply:**
- Derive the main-checkout root as `Split-Path -Parent (git rev-parse --path-format=absolute
  --git-common-dir)`. Do not ascend a hardcoded number of levels; the worktree nesting depth is
  not a stable contract.
- Run the **worktree-root substitution first**. The main-checkout-root string is a *prefix* of the
  worktree-root string, so applying it first truncate-matches and corrupts every worktree-root
  occurrence.
- Use **distinct placeholders** (`<repo-root>` vs `<main-checkout-root>`). Collapsing both makes
  the log falsely assert the two roots are the same path and destroys its own record that MSBuild
  emitted two separate `/analyzerconfig:` arguments.
- A single case-insensitive sweep for the account token (`Split-Path -Leaf $env:USERPROFILE`)
  covers both classes, because it is agnostic to which root the token sat in.
- Both substitutions are pure in-line substring replacements, so line counts are preserved and
  every existing line-number citation against the log stays valid. Assert that
  (`(Get-Content).Count` before == after) rather than assuming it.

Also relevant: the repo `.gitignore`'s blanket `*.log` rule silently excludes these artifacts from
a plain `git add`; they need `git add -f`.

Related: [[_shared_no_absolute_host_paths]], [[project_vstest_trx_evidence_needs_sanitisation_task]],
[[project_msbuild_filelogger_double_counts_each_warning]]
