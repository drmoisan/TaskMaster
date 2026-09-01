---
name: project-662-round3-trx-hygiene-and-verbatim-seams
description: Issue #662 round-3 revision seams — TRX files are not gitignored so vstest evidence leaks account/machine/worktree identifiers, a hygiene sweep's own zero-hit verification is unsatisfiable while the plan file holds the worktree root, and a reviewer-supplied delta must be applied verbatim with disagreements reported rather than rewritten
metadata:
  type: project
---

Round 3 of the issue #662 atomic plan surfaced four seams that generalize.

**1. `.gitignore` covers `*.coverage` and `*.coveragexml` but not `*.trx`.** Verified in this
repository at `.gitignore:140-141`; the only other test-result entry is `TestResult.xml` at `:44`.
A `vstest.console.exe /Logger:trx` run whose `/ResultsDirectory:` points into a feature's
`evidence/` tree therefore produces a committed artifact carrying `runUser`, `computerName`,
`runDeploymentRoot`, and the worktree root in the `storage` attribute of every unit-test element.
`.csharpierignore` does list `*.trx` (`:8`) and `*.cobertura.xml` (`:5`), which is a formatting
exclusion and not a commit exclusion — do not read one as the other.

**Why:** the recurrence class recorded for issues #511 and #468. Every plan that routes a TRX into
`evidence/` reintroduces it unless a sanitisation task budgets for it.

**How to apply:** when a plan commits tool output, check `.gitignore` for that extension
specifically, and add a substitution sweep before each `git add` that stages the evidence tree.

**2. The sweep must be case-insensitive, and its rewrite scope must exclude gitignored binaries.**
`vstest.console.exe` writes the `storage` attribute in lower case while the Windows worktree root is
mixed case, so a case-sensitive pass clears the TRX header and leaves one path per test intact. And
a `/EnableCodeCoverage` run drops a binary `.coverage` file into the same results directory; a
`ReadAllText`/`WriteAllText` rewrite corrupts it. Filter the rewrite with `git check-ignore -q`
(read-only, so it does not violate a no-write-git-commands constraint) rather than by extension.

**3. A hygiene sweep whose verification is "no file in the feature folder matches the account
name" is unsatisfiable when the plan file itself is allowed to name the worktree root.** The plan's
"Working Directory and Base Commit" section is an established, deliberate reference. Scope the
verification search to the feature folder *minus the plan file*, and say why in the plan. Otherwise
the gate cannot pass and therefore gates nothing.

**4. Derive the account and machine names at run time; never write them into the plan.**
`$account = Split-Path -Leaf $env:USERPROFILE` and `$machine = $env:COMPUTERNAME` give the search
values without the artifact naming them. Apply substitutions longest-value-first (worktree root,
user-profile path, machine, account) so a shorter value nested in a longer one does not fragment it.

**5. Substituting your own wording for a reviewer-supplied delta item costs a round.** The round-2
reviewer recorded that five of fourteen supplied items had been re-worded, and that substituted text
is unreviewed until the next round. The correct handling is: apply supplied text verbatim,
reassemble any command or path the artifact wrapped across lines, add concrete renderings as clearly
labelled *additional* text rather than edits to the supplied sentences, and report any disagreement
explicitly in the handoff for the orchestrator to adjudicate.

Related: [[project_662_banner_prefix_revision_round_seams]], [[project_662_banner_prefix_arity_plan_seams]], [[runtime-derived-account-token-pattern]], [[zero-hit-grep-gates-need-carveouts]], [[agent-memory-is-tracked-scope-git-gates]], [[project_511_r1_preflight_delta_seams]].
