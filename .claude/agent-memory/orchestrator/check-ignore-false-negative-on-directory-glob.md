---
name: check-ignore-false-negative-on-directory-glob
description: git check-ignore returns "not ignored" for a directory-only pattern when the directory does not exist yet, and grepping .gitignore for a literal name misses a glob; together they manufacture a false footprint finding
metadata:
  type: feedback
---

Two probes that look conclusive both return the wrong answer for a directory that a build step is
*about to* create, and an agent that runs them together will confidently report a footprint violation
that does not exist.

**Probe 1: `git check-ignore <name>` without a trailing slash.** A `.gitignore` pattern ending in `/`
matches directories only. Git cannot classify a path that does not exist on disk yet, so the
directory-only rule does not fire and `check-ignore` exits 1 — indistinguishable from "no rule matches".

**Probe 2: `grep -i <name> .gitignore`.** A glob need not contain the literal name it matches.

Verified 2026-09-01 on issue #285. `.gitignore` line 350 is `.dotnet*/`. The repo-local SDK bootstrap
`scripts/vscode/Install-RepoDotNetSdk.ps1` creates `.dotnet-sdk/` at the worktree root:

```
git check-ignore -v .dotnet-sdk        -> exit 1   (directory absent; looks unignored)
git check-ignore -v ".dotnet-sdk/"     -> exit 0   .gitignore:350:.dotnet*/  .dotnet-sdk/
git check-ignore -v ".dotnet-sdk/x.txt"-> exit 0   .gitignore:350:.dotnet*/  .dotnet-sdk/x.txt
grep -i sdk .gitignore                 -> no match (the glob has no literal "sdk")
```

An `atomic-executor` preflight pass ran probes 1 and 2, concluded `.dotnet-sdk/` would appear in every
`git status --porcelain`, and raised a **blocking** finding demanding that three terminal footprint
gates add a `.dotnet-sdk/` carve-out. Applying it would have weakened all three gates and written a
false justification into the plan.

**How to apply.** Before accepting any "path X is not ignored" claim, re-probe with a trailing slash
AND with a nested path under it. Either exiting 0 settles it. Treat a bare-name `check-ignore` exit 1
on a not-yet-created directory as *no evidence*, not as evidence of absence. Never corroborate with a
literal grep of `.gitignore`; read the file around the plausible glob instead.

This is the general shape worth remembering: a carve-out request is a request to weaken a gate, so the
premise deserves more scrutiny than a finding that tightens one. The reviewer's other nine findings on
the same pass were sound and were applied — the lesson is to verify each claim on its own evidence, not
to discount the reviewer.

Related: [[grep-cr-empty-pattern-false-crlf]] (same family: a probe whose null result is an artifact of
the probe), [[feedback_verify_subagent_capability_claims]], [[preflight-catches-vacuous-gates]].
