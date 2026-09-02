---
name: project-662-banner-prefix-arity-plan-seams
description: "#662 minimal-audit planning seams — AC5 is a prefix of AC5b; a repo-wide csharpier format pass endangers two zero-diff ACs; the closing quote+paren anchors (\"===\") against (\"====\"); the coverage runner throws at <80% before it rewrites the Cobertura"
metadata:
  type: project
---

Seams found while authoring the issue #662 minimal-audit plan (`EfcSelectionGuard` banner-prefix rename
plus stale-comment fix). All re-derived against the tree at base `2b85134b`.

- **`AC5` is a prefix of `AC5b`.** The issue carries AC1–AC9 *plus* AC5b. A check-off edit of
  `- [ ] AC5` would also match the AC5b line. Anchor every check-off on the identifier followed by a
  space and the em dash (`- [ ] AC5 —` versus `- [ ] AC5b —`). Same class as the AC1/AC10 trap in
  [[project_469_comment_accuracy_plan_seams]].
- **A repo-wide `csharpier format .` is a live threat to a zero-diff AC.** AC5b and AC7 assert
  `git diff <base> --stat -- <path>` is empty for `BreadcrumbRowBuilder.cs` and
  `EfcFormControllerTests.cs`. CLAUDE.md mandates the repo-wide form, so pre-existing drift in either
  protected file would be repaired by the mandated command and make the AC unsatisfiable. Remedy that
  costs nothing: a Phase 0 task running `dotnet tool run csharpier check <path>` on each protected file,
  so the condition is visible before any edit rather than after the format pass. See
  [[repo-wide-csharpier-format-breaks-zero-diff-acs]].
- **`("===")` is NOT a substring of `("====")`.** The closing quote plus paren is the anchor: matching
  `(`,`"`,`=`,`=`,`=`,`"`,`)` against `(`,`"`,`=`,`=`,`=`,`=`,`"`,`)` fails at the fourth `=`. That makes
  a paired two-count gate (`("===")` → 2 and `("====")` → 2) a clean shape pin for a test asserting both
  arities. It only works if the plan mandates four explicit assertions; a `foreach` over
  `new[] { "===", "====" }` produces zero matches of either pattern.
- **`Invoke-MSTestWithCoverage.ps1` throws at <80% repo-wide line coverage**
  (`Invoke-MSTestWithCoverage.Helpers.ps1:487-490`), and the throw sits *between*
  `ConvertTo-KoverageCoberturaXml` and the `Set-Content` that writes the post-processed file. So on a
  sub-threshold run `coverage/coverage.cobertura.xml` is left as the RAW dotnet-coverage output. Make the
  baseline task's acceptance "the artifact records the numeric `line-rate`/`lines-covered`/`lines-valid`",
  not "the script exits 0" — the numbers are readable either way and the task stays satisfiable.
- **Exactly ONE `[TestCategory("LiveOutlook")]` attribute exists**, at
  `TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs:72`. A research artifact for this issue
  claimed "three times in two files"; the other hits are doc-comment prose. Neither `QuickFiler.Test` nor
  `UtilitiesCS.Test` carries one, so the exclusion filter is a no-op for those two assemblies.
- **An agent worktree under `.claude/worktrees/` contains no nested `.claude/worktrees/`,** so recursive
  `*.Test.dll` discovery from ITS root cannot pick up a sibling worktree. Verify with a glob before
  writing the `\.claude\` exclusion into a task; in this worktree the exclusion was unnecessary.
- **CSharpier print width is 100 and no `.csharpierrc` exists.** Renaming `BannerPrefix` to
  `BannerRejectionPrefix` takes the two `StartsWith` call-site lines from 83 to 92 characters, so they
  stay on one line and a `StartsWith(BannerRejectionPrefix` search remains a single-line match. Compute
  the post-rename column count before writing an identifier-bearing search as an acceptance condition.
- **The regex `const +string +[A-Za-z_]*BannerPrefix` does not match `BannerRejectionPrefix`,** because
  the inserted word breaks the required `Banner`+`Prefix` adjacency. That is what makes the declaration
  inventory fall from three to one on a rename alone, with no deletion at the guard site.

**Why:** the issue's own Expected Behavior reads as though the arities should be unified upward to four
characters; that direction relaxes a merged filing guard and fails `EfcFormControllerTests.cs:463` while
`:462` — the assertion that *looks* like the consistency guard — still passes. The plan must state the
prohibited direction in prose, not only in an AC.

**How to apply:** on any follow-up in this EFC surface, re-derive the three anchored counts
(`= "===";` → 1, `= "====";` → 2 pre-change, declaration regex → 3 pre-change) with the `-- '*.cs'`
pathspec. Unscoped, the same text appears in closed-feature records under
`docs/features/active/efc-controller-surface-defects-464/` and in the feature's own documents, so the
unscoped figure grows as the feature is authored and must never be asserted.
