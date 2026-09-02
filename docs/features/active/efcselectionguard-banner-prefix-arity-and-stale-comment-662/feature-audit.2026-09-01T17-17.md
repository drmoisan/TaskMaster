# Feature Audit — issue 662 (efcselectionguard-banner-prefix-arity-and-stale-comment)

Timestamp: 2026-09-01T17-17
Reviewer: feature-review agent
Base: `origin/main` @ `43dcc800e5c75ab1d1033f0eac0e4b61ac919b59`
Head: `8a40a587970f9143e15969e3e233be7dd6b62114`

## Acceptance-Criteria Source Resolution

`issue.md:3` carries `- Work Mode: minor-audit`. Under the work-mode routing rules, the sole
acceptance-criteria source is the explicit `## Acceptance Criteria` section in `issue.md`
(`:115-182`). `spec.md` and `user-story.md` do not exist in the feature folder, which is the
required state for `minor-audit`. Confirmed by directory listing: the folder contains
`issue.md`, `plan.2026-08-31T20-11.md`, `research/`, and `evidence/`, and no `spec.md` or
`user-story.md`.

No other checkbox section in `issue.md` was treated as an acceptance criterion. The
`## Evidence Checklist` at `:197-201` is tracked separately below.

Ten criteria are declared: AC1, AC2, AC3, AC4, AC5, AC5b, AC6, AC7, AC8, AC9.

## Verification Method

Every criterion was re-verified by running its own stated verification command in this worktree
against the head commit, or by reading the primary artifact it names. Executor prose summaries
were used only as pointers, never as evidence. Where a criterion names a search, the search was
run verbatim including its `-- '*.cs'` pathspec, which the issue justifies at `:117-119` and
`:81-93`: `"==="` is a substring of `"===="`, so an unanchored or unscoped search cross-matches
and also picks up closed-feature audit records under
`docs/features/active/efc-controller-surface-defects-464/`.

## Criteria Evaluation

| AC | Criterion (abridged) | Verdict | Evidence re-derived by reviewer |
|---|---|---|---|
| AC1 | Guard's rejection breadth unchanged; constant still three characters. `git grep -n -F -- '= "===";' -- '*.cs'` returns exactly one line, in `EfcSelectionGuard.cs`. | **PASS** | Command returned exactly 1 line: `QuickFiler/Controllers/EfcSelectionGuard.cs:38: private const string BannerRejectionPrefix = "===";` |
| AC2 | Constant renamed to `BannerRejectionPrefix` and used at both `StartsWith` call sites. Declaration regex returns 1 line in `BreadcrumbRowBuilder.cs`; call-site search returns exactly 2 lines. | **PASS** | Declaration regex returned 1 line: `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19`. Call-site search returned exactly 2 lines: `EfcSelectionGuard.cs:72` and `:98`. |
| AC3 | XML doc states three things: proper prefix of `BreadcrumbRowBuilder.BannerPrefix`; rejects a strict superset; must not be widened, naming the AC6 test. | **PASS** | All three present and read in source. Proper prefix: `:17-20`. Strict superset: `:23-25`. Prohibition plus reason plus test name: `:28-35`, naming `BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates`. The `<see cref="BreadcrumbRowBuilder.BannerPrefix"/>` resolves via `using UtilitiesCS.OutlookObjects.Folder;` at `:2`, against a `public sealed class` with a `public const`. |
| AC4 | `SelectedFolder` comment no longer asserts a four-character second rejection; describes the composition the code implements. | **PASS** | `EfcFormController.cs:318-320` now reads that `IsValidSelection` routes to `IsSelectableFolder`, which composes `IsBannerRow` matching the producers' `"===="` prefix with the guard's deliberately broader three-character rejection. Each clause checked against `:1143-1155`. The old false claim ("keeps its `\"====\"` rejection as a second guard") is gone. |
| AC5 | `FolderSuggestionTree.cs` declares no banner-prefix constant; its single reader `IsBanner` references `BreadcrumbRowBuilder.BannerPrefix`. Four-character search returns 1 line; `BannerPrefix` search in that file returns exactly 1 line. | **PASS** | `git grep -n -F -- '= "====";' -- '*.cs'` returned 1 line, `BreadcrumbRowBuilder.cs:19`. `git grep -n 'BannerPrefix' -- UtilitiesCS/.../FolderSuggestionTree.cs` returned exactly 1 line, `:198`, which is the qualified reference inside `IsBanner`. The declaration was deleted, not aliased — confirmed in the diff. |
| AC5b | `BreadcrumbRowBuilder.cs` is NOT modified. | **PASS** | Verified twice. (a) The criterion's own command, run verbatim with its pinned anchor: `git diff 2b85134b… --stat -- UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` returned empty. (b) Against the correct merge base: `git diff 43dcc800… ...HEAD --stat -- <same file>` returned empty. The file is absent from `git diff --name-only origin/main...HEAD`. See follow-up FU-1 on the pinned anchor. |
| AC6 | New MSTest method `BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates` asserting both predicates reject `"==="` and `"===="`, with a `because` naming the prohibited direction. Scoped run reports `Passed: 1`, `Failed: 0`. | **PASS** | Method present at `EfcSelectionGuardTests.cs:294-313` with exactly four assertions covering both predicates against both inputs, all sharing one `because` constant containing "must not be widened to the producers' four-character prefix". `evidence/regression-testing/p2-t5/ac6-scoped.trx` parses well-formed and records `<Counters total="1" executed="1" passed="1" failed="0" …>` with `testName` equal to the required name and `outcome="Passed"`. |
| AC7 | `IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically` in `EfcFormControllerTests.cs` is unmodified and still passes. | **PASS** | Unmodified: the criterion's own pinned-anchor command returned empty, and `git diff 43dcc800… ...HEAD --stat -- QuickFiler.Test/Controllers/EfcFormControllerTests.cs` also returned empty. Still passes: `evidence/regression-testing/p2-t6/ac7-scoped.trx` records `total="1" passed="1" failed="0"` with that exact `testName` and `outcome="Passed"`. |
| AC8 | No behavioural change reaches `FolderSuggestionTree.IsBanner`, `BreadcrumbRowBuilder`, or `EfcFormController.IsBannerRow`. Full-assembly runs report `Failed: 0` with `Passed:` no lower than baseline. | **PASS** | Counters read directly from the four full-assembly TRX documents: `QuickFiler.Test` 1286 -> 1287 passed, 0 failed; `UtilitiesCS.Test` 4783 -> 4783 passed, 0 failed. The reviewer additionally diffed the test-name multisets: the only difference in either direction is the single AC6 test gained. Behaviour preservation is also established by value identity — the guard's constant is byte-identical and `BreadcrumbRowBuilder.BannerPrefix` equals the deleted local literal. |
| AC9 | Full C# toolchain passes in one clean pass in the order format, analyze, type-check, test, using the exact CLAUDE.md commands, each with an evidence artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. The format artifact records the CSharpier summary line, not the exit code alone. | **PASS** | All four fields present in all six gate artifacts (verified by grep). Order and timestamps: format 15:59 -> check 16:00 -> analyze 16:01 -> type-check 16:02 -> test 16:24 and 16:35. All `EXIT_CODE: 0`. The format artifact transcribes `Checked 1566 files in 4734ms.` verbatim. Commands match CLAUDE.md exactly, including `/t:Rebuild` (not `/t:Build`) and the deliberate omission of `/p:Nullable=enable`. |

**Result: 10 PASS, 0 PARTIAL, 0 FAIL, 0 unverifiable.**

## AC Check-Off State

All ten criteria are already marked `- [x]` in `issue.md`. Each mark is corroborated by this
audit's independent re-verification, so no check-off change is required and `issue.md` was not
modified by this review.

### Acceptance Criteria Status

```
- Source: docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/issue.md
- Total AC items: 10
- Checked off (delivered): 10
- Remaining (unchecked): 0
- Items remaining: none
```

## Evidence Checklist (`issue.md:197-201`)

| Item | Marked | Verified present |
|---|---|---|
| baseline | `[x]` | yes — 18 files under `evidence/baseline/`, including both baseline TRX and `coverage-baseline.cobertura.xml` |
| targeted verification | `[x]` | yes — nine `evidence/qa-gates/ac*-verification.md` artifacts plus two scoped-run TRX under `evidence/regression-testing/` |
| end-state | `[x]` | yes — six gate artifacts, two post-change TRX, `coverage-postchange.cobertura.xml`, `coverage-delta.md`, `scope-and-commit.md` |

## Implementation Intent Verification

The issue's `## Implementation Intent` (`:61-76`) names five required steps and one prohibited
direction. All six were checked against the head commit.

| Intent | Result |
|---|---|
| 1. Keep the guard's rejection breadth exactly as it is; value stays `"==="` | Held. Value byte-identical. |
| 2. Rename to `BannerRejectionPrefix` and document that it is deliberately a proper prefix and must not be widened | Done, `EfcSelectionGuard.cs:14-38`. |
| 3. Correct the `SelectedFolder` comment | Done, `EfcFormController.cs:318-320`. |
| 4. Remove the duplicated four-character literal in `FolderSuggestionTree` by referencing `BreadcrumbRowBuilder.BannerPrefix` | Done, declaration deleted at former `:16`, reader qualified at `:198`. |
| 5. Add an MSTest regression test pinning the relationship with an explanatory message | Done, `EfcSelectionGuardTests.cs:294-313`. |
| Prohibited: widening the guard to `"===="` | Not done, correctly. Re-derived independently below. |

Post-condition asserted at `issue.md:74-76` — "after the change there are two banner-prefix
declarations, not three: one producer constant shared by both producers, and one deliberately
broader classifier constant" — is satisfied. Measured declaration inventory:

| Declaration | Value | Role | Consumers |
|---|---|---|---|
| `BreadcrumbRowBuilder.BannerPrefix` (`:19`, `public const`) | `"===="` | producer constant | `FolderSuggestionTree.IsBanner`, `EfcFormController.IsBannerRow` |
| `EfcSelectionGuard.BannerRejectionPrefix` (`:38`, `private const`) | `"==="` | classifier rejection constant | `IsValidFilingSelection`, `IsValidCreationSelection` |

Two declarations, distinct names, distinct roles, documented asymmetry. The divergence condition
the issue names — three declarations under one name carrying two different values — no longer
exists.

## Independent Verification of the Prohibited Direction

The reviewer did not accept the "do not widen" conclusion from the issue, the plan, or the
research document. It was re-derived by reading `QuickFiler/Controllers/EfcFormController.cs`
and `QuickFiler/Controllers/EfcSelectionGuard.cs` directly.

There are two EFC classification sites, and for the row `"==="` each rejects it through exactly
one term:

- **Filing site** (`EfcFormController.cs:745-749`): `IsBannerRow("===")` is `false` because
  `"==="` does not start with the producers' `"===="`. `ArchiveStemContract.IsFullOutlookPath("===")`
  is `false`. `IsValidFilingSelection` carries no minimum-length rule. The only term that rejects
  is `!"===".StartsWith(BannerRejectionPrefix)`.
- **Creation site** (`EfcFormController.cs:1151-1153`): `!IsBannerRow("===")` is `true`;
  `"===".Length >= MinimumCreationLength` is `3 >= 3`, `true`, so the length rule rejects nothing.
  Again the only rejecting term is the three-character prefix test.

Widening the constant to `"===="` would therefore make both `IsValidFilingSelection("===")` and
`IsSelectableFolder("===")` return `true`. That is a behavioural relaxation, not a consistency
fix, and the reviewer confirms the branch correctly did not make it.

The reviewer also confirmed the trap in the pre-existing merged test at
`EfcFormControllerTests.cs:453`: under the widening edit, for the row `"==="`, `creationPath` and
`filingPath` both become `true`, so `:462` `creationPath.Should().Be(filingPath, …)` — the
assertion that reads like the consistency guard — **still passes**, and only `:463`
`creationPath.Should().BeFalse(…)` fails. The AC6 test asserts `BeFalse` directly on all four
inputs and does not share that blind spot. This is the substantive reason AC6 was worth adding
even though AC7's test already existed.

## Reachability of the Underlying Defect

The defect this feature addresses is **latent, not live**. No producer emits a three-character
banner row: both producers write `BreadcrumbRowBuilder.BannerPrefix`, which is `"===="`. There is
therefore no input reaching either classification site today for which the guard's broader
rejection changes the outcome, and no user-visible misbehaviour was reachable before this change.

The cost was entirely to the next maintainer: a constant named `BannerPrefix` holding a value
different from the two other constants of the same name, plus a comment actively asserting the
opposite of what the code did. Both are now removed. The change is correctly characterised in
`issue.md:35-36` and is not overstated anywhere in the branch's artifacts.

## Follow-Ups (recorded, not promoted)

Per the delegation constraint and the footprint acceptance criteria AC5b, AC7 and the four-file
scope boundary, no potential entry was created and no issue was opened from this branch. Each
item below is recorded for consolidated filing after merge.

### FU-1 — `issue.md` AC5b and AC7 cite the superseded diff anchor

`issue.md:156` and `:170` verify AC5b and AC7 with
`git diff 2b85134b42872e405602e6064e02dc9cda6c319b --stat -- <file>`. The plan's
`## Execution Amendment` (commit `63ef2e8f`) replaced that anchor with a run-time
`git merge-base origin/main HEAD` for the corresponding plan tasks, because the pinned anchor is
an ancestor of `origin/main` and a two-dot diff from it reports everything `origin/main`
accumulated since — measured at 22 paths against an asserted union of 4. The amendment updated
the plan but not the AC text.

**Reachability: currently benign, verified.** Both AC commands were run verbatim as written and
both returned empty, because `origin/main` has not touched either file since `2b85134b`. Both ACs
also pass under the correct merge-base anchor, so AC5b and AC7 are recorded PASS on evidence that
holds under either anchor. The stale citation becomes wrong only if either protected file changes
on `origin/main`. Not merge-method-dependent.

### FU-2 — The shared artifact-hygiene rule corrupts XML it redacts

Reached on this branch and already repaired here (commit `8a40a587`, recorded at
`evidence/qa-gates/trx-xml-wellformedness-repair.md`). The rule substitutes angle-bracketed
placeholder tokens into evidence files without escaping them for XML, and verifies only
`ResidualMatchCount=`, never that the rewritten file still parses. It left all six committed TRX
files not well-formed. **Reachability for future work: any plan reusing the rule that redacts a
`.trx`, `.xml`, `.coveragexml` or `.cobertura.xml` artifact.** Fix belongs in the shared rule,
not in this branch.

### FU-3 — The hygiene sweep excludes the plan file from its own residual check

The residual scan filters out `plan.md` by path, so a host path reintroduced into the plan is
undetectable by that gate; commit `db59adfe` shows one did reach the plan at least once on this
branch. Separately, the sweep rewrites only `evidence/` but scans the whole feature folder, so a
residual outside `evidence/` is detected without an in-gate remedy. **Reachability: documentation
hygiene only, latent here** — a reviewer grep of the entire feature folder including `plan.md`
found zero account-name, machine-name and `C:\Users\` matches.

### FU-4 — `IsBanner` doc comment still restates the producers' arity as a literal

`FolderSuggestionTree.cs:193` documents `IsBanner` as "begins with `"===="`" while the body now
reads `BreadcrumbRowBuilder.BannerPrefix`. Accurate today. **Reachability: documentation only,
latent** — it is the last literal restatement of the arity in that file and reintroduces, one
layer up in prose, the drift class this issue closes. Deliberately not fixed here: the branch's
AC set makes no allowance for it and the change is otherwise complete.

### FU-5 — `EfcFormController.cs` is 1189 lines and 25.50% line covered

Both figures are byte-identical to the base commit and neither is caused or worsened by this
branch, whose change to that file is a three-line comment replacement. The file is COM- and
WinForms-bound and sits inside CLAUDE.md's ratified COM/VSTO/WinForms coverage exemption class.
**Reachability: maintainability only.** Belongs to whichever item eventually decomposes that
controller behind an injectable seam.

### FU-6 — Two 194k-line Cobertura documents enter `main`'s history

388k of the branch's 478k inserted lines. They are the primary evidence backing the coverage
verdicts, so their presence is defensible, but the cost is permanent. **This one is
merge-method-dependent to note precisely: unlike issue #648, these files are committed and
retained rather than committed-then-removed, so there is no unreachable-blob problem and
squash-merge is not a remedy — squash collapses the commits but leaves the files in the tree.**
The policy question is whether future audits should commit a per-file extract plus a retained
checksum instead.

## Merge Readiness

| Gate | State |
|---|---|
| All 10 acceptance criteria | PASS |
| Blocking findings across all three audit artifacts | 0 |
| Full C# toolchain, one clean pass in order | PASS, all `EXIT_CODE: 0` |
| Test suites | 6070 passed, 0 failed across both assemblies post-change |
| Coverage repo-wide (line / branch) | 85.3741% / 79.3761%, both above the 85% / 75% floors |
| Changed-line coverage | 3/3 statements covered; no regression on any changed line |
| Working tree | clean; `git status --porcelain` empty |
| Base is ancestor of HEAD | yes; no rebase or reconciliation merge required |
| Scope boundary (four code files) | held exactly |
| Protected files AC5b / AC7 | both unmodified |

**The branch is ready to merge.** No remediation is required and no remediation-inputs artifact
was produced. The six follow-ups above are all non-blocking and none should widen this branch.
