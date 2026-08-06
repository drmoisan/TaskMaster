---
name: stale-build-output-is-not-evidence-of-existence
description: Never infer that a project/source file exists from obj/ or bin/ cache filenames — this repo carries untracked build output for projects torn down long ago; verify with git ls-files or a glob on the file itself
metadata:
  type: feedback
---

Before writing any existence claim into plan acceptance text, verify the file itself with a glob on its actual path or `git ls-files <dir>`. Filenames inside `obj/` and `bin/` are **not** evidence that the thing they name still exists.

**Why:** In #418 cycle 2 I told the executor to record that `UtilitiesSwordfish.Test`'s project file is `UtilitiesSwordfish.NET.Test.csproj`. No such file exists anywhere in the repo — the project was torn down by the commit titled `refactor(swordfish): tear down vendored UtilitiesSwordfish structural surface (#308)`, but its untracked `obj/` tree survived, containing `UtilitiesSwordfish.NET.Test.csproj.AssemblyReference.cache` and `.dtbcache.json`. I read those cache filenames and inferred a live project file. Preflight blocked the plan: the acceptance would have compelled an executor to assert a nonexistent file as verified fact in an audit artifact, which the plan's own fail-closed evidence rule and the evidence-first audit convention both forbid, and which a reaudit would surface as a finding — reopening a cycle over prose.

**How to apply:** This repo has several `*.Test` directories that are wholly untracked build residue; a `*.Test` directory count will exceed the real project count and the coverage runner's discovered-assembly count. When a task must justify a count or an exclusion, use grounds that are each independently checkable — `git ls-files <dir>` returning zero, a glob on `**/*<Name>*.csproj` returning nothing, absence from `TaskMaster.sln`, or the absence of a `*.Test.dll` in `bin/Debug` — and cite a tear-down commit by **title**, not SHA, per [[never-pin-head-sha-as-plan-expectation]]. Related: [[research-claims-as-acceptance-clauses]] (same failure shape: an unverified claim promoted into an acceptance clause).
