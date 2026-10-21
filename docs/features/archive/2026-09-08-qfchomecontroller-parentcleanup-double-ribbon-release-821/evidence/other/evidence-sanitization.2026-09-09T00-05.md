# Phase 6 — Evidence tree sanitization

Timestamp: 2026-09-09T14-06
Task: [P6-T12]

Both search tokens are derived at run time so that neither this plan nor any artifact it produces
carries a host identifier as a literal. The account token comes from the environment's user-name
variable; the worktree token comes from the leaf of the repository top-level path. Every field of
this artifact, including `Command:`, records the command in that derived form and contains neither
token as a literal — an artifact that quoted its own search patterns would be found by its own re-run
and the zero-hit condition could never be met.

Command:

```text
pwsh -NoProfile -Command '$tokens = @($env:USERNAME, (Split-Path -Leaf (git rev-parse --show-toplevel))); for ($i = 0; $i -lt $tokens.Count; $i++) { $hits = @(Get-ChildItem -Path "docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence" -Recurse -File | Select-String -SimpleMatch -Pattern $tokens[$i]); "token-{0} hits={1}" -f $i, $hits.Count }'
```

EXIT_CODE: 0

## Before-and-after hit counts

| Sweep | token-0 | token-1 |
|---|---|---|
| Before replacement | **0** | **0** |
| After replacement | **0** | **0** |

## Why the two counts are identical

No replacement was necessary. Every artifact in this evidence tree was authored with a `<repo-root>`
placeholder already substituted for the absolute repository path at the point of writing, rather than
written with host paths and cleaned afterwards. The pre-sweep therefore already reported `hits=0` for
both tokens, and the post-sweep re-run of the identical command confirms the state is unchanged.

This is recorded as an equal-count pair rather than presented as a reduction, because claiming a
reduction that did not occur would misstate the evidence. The condition this task gates is that the
**re-run reports zero**, and it does.

Specific places where the substitution was applied at authoring time, all of which would otherwise
have carried an absolute host path:

- the msbuild output tails in the analyzer and nullable artifacts, both baseline and final,
- the vstest output tails in the two coverage-run artifacts, which print the Cobertura artifact path
  on the `Code coverage results:` and `Done. Coverage artifact:` lines,
- the repo-local SDK install line in the toolchain bootstrap artifact,
- the `Done Building Project` lines in the build artifacts.

The coverage-tool probe in `[P0-T6]` additionally recorded the resolved tool's **leaf file name only**
and never its absolute path, because a global tool resolves under the operator's user profile.

## Widened sweep over the whole feature folder

The same two-token search was run over the entire feature folder rather than only its `evidence/`
subtree — covering `spec.md`, `issue.md`, `research/`, the plan file and `evidence/`:

```text
feature-folder token-0 hits=0
feature-folder token-1 hits=0
```

**Zero for both tokens across the whole feature folder.**

The sweep is content-based, not filename-based. No artifact filename in this plan contains either
token, so a filename-only check could not have failed and would have gated nothing; the check above
reads file contents.

Output Summary: the re-run reports `hits=0` for both `token-0` and `token-1` over the evidence tree,
and the widened sweep reports zero for both over the whole feature folder. No host-identifying token
is present in any committed artifact.
