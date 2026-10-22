# PR-body AC-27 verification

Timestamp: 2026-08-26T21-32

Command:

```
gh pr create --repo drmoisan/TaskMaster --base epic/quickfiler-bug-family-integration \
  --head bug/qfc-collection-controller-defects-468 --title <title> --body-file artifacts/pr_body_468.md
grep -icE 'improv[a-z]* the coverage denominator|depress[a-z]* the measured coverage' artifacts/pr_body_468.md
grep -icE 'unrelated sibling|neither is a superset' artifacts/pr_body_468.md
grep -c '#473 defect 1 is latent under the current call graph' artifacts/pr_body_468.md
grep -c '#474 is latent in the current single-implementation configuration' artifacts/pr_body_468.md
grep -c '^| #4[67][0-9]' artifacts/pr_body_468.md
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

The pull request is open at <https://github.com/drmoisan/TaskMaster/pull/636>, targeting
`epic/quickfiler-bug-family-integration`. Its body was authored inline with the `pr-author` skill and
persisted as `artifacts/pr_body_468.md` with the sibling provenance receipt
`artifacts/pr_body_468.receipt.json`, both staged at the session working directory because
`enforce-pr-author-skill.ps1` resolves `artifacts/` relative to the session cwd rather than the
feature worktree. The recorded SHA-256 is
`d0a8ef3a0379584c30729d20b8d14f4f4f3bcf6de6137802ae3a78ab530a52b4`, verified against the body bytes.

All five constraints recorded at `evidence/other/pr-accuracy-constraints.2026-08-26T16-27.md` are
satisfied. AC-27 is checked off on this evidence.

## Constraint-by-constraint result

| # | Constraint | Check | Result |
|---|---|---|---|
| 1 | Do not repeat the #468 coverage-denominator rationale | search for `improv* the coverage denominator` / `depress* the measured coverage` | **0 hits.** The body states the opposite explicitly: "This removal is not offered as a coverage improvement — `QfcCollectionController` carries `[ExcludeFromCodeCoverage]` at line 21, so every line of the type sits outside both the numerator and the denominator ... and removing lines from it cannot move any coverage number in either direction." |
| 2 | Do not repeat the #474 unrelated-sibling-interfaces premise | search for `unrelated sibling` / `neither is a superset` | **0 hits.** The body states the true relationship: `IQfcFormController` **derives from** `IFilerFormController` at `QuickFiler/Controllers/IQfcFormController.cs:13` and is a strict superset, so there were no parallel interfaces to consolidate and the fix is a narrow retype. |
| 3 | State that #473 defect 1 is latent under the current call graph | exact-phrase search | **1 hit**, in the `## Why` section, with the three supporting facts (both `Add` pairs precede their `WhenAll` in the same method body; no other member adds to the bag; all three construction sites create a fresh awaited controller) and an explicit caution that nothing should be read as fixing an intermittent hang. |
| 4 | State that #474 is latent in the current single-implementation configuration | exact-phrase search | **1 hit**, in the `## Why` section, with the supporting fact that `QfcFormController` is the only production implementation so the downcast could not throw today. |
| 5 | Cite specific test names per defect in place of a coverage delta | count per-defect rows in the verification table | **13 defect rows** naming 27 MSTest methods, plus the `#286` row and an explicit `#468` row recording that a removal has no test by construction. The body states that changed-line coverage is *undefined* rather than unmeasured, and quotes no coverage figure as per-defect evidence. |

## Auto-close block

The body carries exactly seven `- Closes #NNN` lines, for #286, #468, #469, #470, #471, #473 and
#474 — the verified set, all seven confirmed OPEN before authorship. #232, #444 and #454 appear in
the feature documents as references rather than closures and are deliberately absent from the block.

No closing keyword appears in any commit message on this branch; the keywords exist only in the PR
body.

## Scope note on when closure actually occurs

This pull request targets the epic integration branch. GitHub registers closing references only for
pull requests targeting the default branch, so none of the seven issues closes at this merge. That is
the subject of AC-28, which remains unchecked for exactly this reason. AC-27 concerns only the
accuracy of the body's content, which is satisfied.
