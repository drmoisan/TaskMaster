---
name: coverage-and-claude-paths-pollute-derived-radii
description: Derived radii routinely admit gitignored build output (coverage/**), cited-not-written .claude/hooks and .claude/settings.json, literal FEATURE/ placeholders, framework paths and line-locator tokens; measure whether the over-report changes a verdict before spending a correction round
metadata:
  type: project
---

Measured 2026-09-17 on the `bugs-2026-09-17` run (items 895 and 900).

**The fact.** A test-only item whose approved write set was ONE file derived a 35-path radius plus
one shared surface. A project-file item whose write set was 7 files derived 54 paths. Neither
triggered a V1, V2 or V3 finding, so no rule in the landed contract catches this.

**Why:** `config/blast-radius.json` `mandate_reads` lists `.claude/rules/**` but NOT
`.claude/hooks/**` or `.claude/settings.json`. A plan that documents the pre-implementation gate its
execution session must satisfy therefore records those as WRITE claims. On item 900 that resolved
`.claude/settings.json` as a declared SHARED SURFACE, which is the expensive kind of false entry:
any later item genuinely writing it would serialize against 900.

Five junk classes observed, none of which any configured exclusion removes:

1. **Gitignored build output.** `coverage/coverage.cobertura.xml`, `coverage/logs/*`,
   `coverage/plan900-helper.ps1` (`.gitignore:144`, `.gitignore:348`). Every C# item writes these,
   in its OWN worktree, so an overlap here is never real contention.
2. **Cited-not-written `.claude/**` paths** — the gate hooks, `settings.json`, a lifecycle SKILL.
3. **Literal `FEATURE/` stand-ins** — `FEATURE/issue.md`, `FEATURE/spec.md`, `FEATURE/user-story.md`.
   The placeholder-shape rejection catches `<...>` and `${...}` but not a bare capitalised stand-in.
4. **Framework source paths** — `WindowsBase/System/Windows/Threading/Dispatcher.cs`.
5. **Line-locator tokens** — `QuickFiler/Viewers/ItemViewer.cs:20`, admitted ALONGSIDE the same path
   without the locator, so the path is double-counted.

**How to apply.** Do not narrow a radius to suppress an edge — that stays prohibited, and none of
these tripped a blocking rule so no re-plan round is owed. Instead do the counterfactual: run
`Test-BlastRadiusConflict` for the item against every real sibling and check whether any verdict
would change under a clean radius. On this run the only pair verdict was `False` either way, so a
correction round bought nothing and I recorded the over-report as a manifest advisory instead.

**Two consequences to carry forward regardless of the measurement.** Drift detection compares
declared against observed, so an over-broad declared radius fails OPEN — a genuine escape into
`.claude/**` would not be reported. And an over-broad radius poisons `open` mode specifically: with
`.claude/**` and `packages/**` declared, almost any `/parallel-add` item collides. If the operator
wants open mode, tighten first.

**The counterweight that kept this run parallel:** `mergeable_paths` now carries `**/*.csproj`, and
the module map carries only `config`. The two items' radii intersected on exactly
`QuickFiler.Test/QuickFiler.Test.csproj` and still produced NO edge, so one cohort held both. State
that explicitly in the manifest, or the absent edge later reads as a narrowed radius.

These are push-down-owned defects. Fix in drm-copilot; see [[drm-copilot-upstream]].
