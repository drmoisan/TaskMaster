# P6-T4 — Default Account-And-Host Name Scan (acceptance MET)

Timestamp: 2026-09-13T15-02

Task: [P6-T4]

EXIT_CODE: 0

Output Summary: Neither end-to-end run produced a test-result document bearing the default account-and-host file name. The load-bearing observation, a direct listing of the two results directories the P6-T2 and P6-T3 runs actually wrote to, returns zero matches in both. The complementary observation, a search over the union of the paths reported by a porcelain status and a name-listing diff anchored to the recorded base commit, also returns zero matches. This task's acceptance is MET.

VERDICT: PASS.

## Token Handling

The account token and the host token are derived at run time from the environment and are never written into this artifact. Counts only are recorded, in keeping with the evidence-hygiene rule this delivery itself authors. The searched composition is the default test-result document name: the account token, an underscore, the host token, a space, and a timestamp, with a `.trx` extension.

Two searches were run over every listing:

- STRICT: the full default composition as a single anchored pattern.
- LOOSE: any name containing both the account token and the host token, independent of ordering or of a timestamp. The loose search is the wider of the two and is reported so a reader can see that the zero result does not depend on the exact composition pattern being right.

## Observation 1 — Results-Directory Listings (load-bearing)

Command form: `pwsh -NoProfile -Command "Get-ChildItem -LiteralPath <each results directory> -File | Select-Object -ExpandProperty Name"`

### P6-T2 results directory: `coverage/test-results`

```
mstest-coverage-run.summary.txt
mstest-coverage-run.trx
```

STRICT_COMPOSITION_MATCHES: 0

LOOSE_ACCOUNT_AND_HOST_MATCHES: 0

### P6-T3 results directory: `coverage/tm873-external-output`

```
coverage.cobertura.jacoco.xml
mstest-coverage-run.summary.txt
mstest-coverage-run.trx
```

STRICT_COMPOSITION_MATCHES: 0

LOOSE_ACCOUNT_AND_HOST_MATCHES: 0

Both test-result documents carry the explicit name `mstest-coverage-run.trx` rather than the default composition, which is the direct consequence of this delivery passing `/ResultsDirectory:` and `/Logger:trx;LogFileName=` on the inner test-console invocation. That is the behaviour this criterion exists to observe.

### Subdirectory note, recorded rather than omitted

SUBDIRECTORY_COUNT for `coverage/test-results` after the P6-T2 re-run: 0
SUBDIRECTORY_COUNT for `coverage/tm873-external-output` after the P6-T3 run: 0

Neither of the two runs recorded here produced an MSTest deployment directory. The aborted first P6-T2 attempt did produce one, and its name carried the account token while a directory nested one level beneath it carried the host token. That observation is recorded in the P6-T2 artifact. It is out of this task's scope on two independent grounds: the scan is over files, and a deployment directory is a directory; and the whole `coverage` tree is ignored, so nothing beneath it is committed. It is named here so that a reviewer who reads the P6-T2 artifact finds the disposition already stated rather than absent.

## Observation 2 — Changed-Path Union (complementary)

Commands, run in this same task:

1. `git status --porcelain --untracked-files=all`
2. `git diff --name-status refs/base-anchor-873`

STATUS_PATH_COUNT: 0

DIFF_PATH_COUNT: 195

UNION_PATH_COUNT: 195

The porcelain status reports nothing because the worktree is clean at the point this scan was taken; the union is therefore the diff set.

UNION_STRICT_COMPOSITION_MATCHES: 0

UNION_LOOSE_ACCOUNT_AND_HOST_MATCHES: 0

UNION_TRX_COUNT: 0

UNION_COBERTURA_COUNT: 0

The union contains no `.trx` document and no `.cobertura.xml` document at all, so there is no candidate for the searched composition regardless of naming. That is the stronger statement and it is the one the counts above support.

### Why this observation is complementary and not load-bearing

The plan states the reason and it is restated here because it is what makes the scan honest. Both results directories sit beneath the already-ignored repository coverage tree, so a porcelain status and a name-listing diff are both blind to a document written there. A scan built from them alone would return zero matches whatever the entry points did. The directory listing above is the observation that can actually fail; this one cannot. Both are recorded, and the task's acceptance requires both.

A whole-working-tree scan is explicitly not used. More than one hundred test-result documents are already tracked from earlier features and at least one still carries an unredacted default name, so a tree-wide scan would fail no matter what this delivery does. Removing those is the repository-wide sweep item's scope.

## The Union Spans A POST-MERGE Tree

This is the material difference between this scan and the pre-merge P7-T9 inventory, and it is stated first rather than left for a reviewer to infer.

The union above was measured against a tree into which `origin/main` has been merged. The measurement is therefore wider than the delivery's own footprint by exactly the content that merge brought in.

| Fact | Value |
|---|---|
| Merge commit created by this phase | `e0b4a8c2f83ced8514f00b8e0ba1825327ae4111` |
| `origin/main` tip merged | `a5622ab9123a88bfa3ec5b8fccfdc613e74c4df5` |
| `refs/base-anchor-873` | `5cc7dcd6330b44e3d2238ffe7fa1e5b3317d9ee1` |
| Anchor is a strict ancestor of the merged tip | yes |
| Commits `main` advanced since the anchor | 27 |

The ancestry claim was verified rather than assumed: `git merge-base --is-ancestor refs/base-anchor-873 origin/main` returned exit code 0, and the anchor is not equal to the merged tip.

The consequence for this task's acceptance condition is that the condition itself is unchanged. The gate was run exactly as the plan writes it, against the anchor the plan names. Only the tree it evaluates is wider. The verdict is unaffected, and the reason is recorded below rather than asserted: none of the inherited paths is a candidate for the searched composition.

## Inherited Foreign Paths

INHERITED_FOREIGN_PATH_COUNT: 91

OWN_DELIVERY_PATH_COUNT: 104

91 + 104 = 195, which is the union count above.

A path is classified as inherited when it appears in `git diff --name-only refs/base-anchor-873 origin/main`, that is, when the merge rather than this delivery introduced it.

### Characterisation of the 91 inherited paths

| Category | Count |
|---|---|
| Item 839 feature folder, `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/` | 44 |
| Item 877 feature folder, `docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877/` | 34 |
| C# source files | 5 |
| Project files | 2 |
| Agent-memory Markdown | 5 |
| Promoted potential entry | 1 |
| **Total** | **91** |

INHERITED_TRX_COUNT: 0

INHERITED_STRICT_COMPOSITION_MATCHES: 0

Stated explicitly, as the acceptance requires: **none of the 91 inherited paths is a `.trx` document, and none matches the searched account-host-timestamp composition.** 78 of the 91 are Markdown evidence and planning documents under two foreign feature folders; 7 are C# source or project files; 5 are agent-memory Markdown; 1 is a promoted potential entry. No category contains a test-result document.

### The 13 inherited paths that are not documentation

```
.claude/agent-memory/atomic-executor/project_new_cs_files_guarantee_a_format_loop_restart.md
.claude/agent-memory/orchestrator/MEMORY.md
.claude/agent-memory/orchestrator/delta-application-is-itself-a-defect-source.md
.claude/agent-memory/orchestrator/prd-feature-deny-string-blames-the-marker-not-the-cwd.md
.claude/agent-memory/orchestrator/pwsh-starts-in-session-worktree-not-yours.md
QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/SetupAssemblyInitializer.cs
QuickFiler/Controllers/QfcHomeController.cs
TestSupport/TestAssemblyResolver.cs
UtilitiesCS.Test/TestAssemblyInitializer.cs
UtilitiesCS.Test/UtilitiesCS.Test.csproj
```

None of these is named by this plan, and none is authored by this delivery. They are listed in full rather than summarised, because an inventory that reported only a count of paths it had classified would not be checkable.

## Both Merged Items Are Represented

The provenance is complete rather than attributed wholly to one item. Two items merged into `main` between the anchor and the merged tip, and both left code in this tree.

### Item 877, merged as pull request 880 — five files

Authored by commit `509af24ed`, `fix(877): install QuickFiler.Test's own AssemblyResolve fallback from a shared TestSupport source file`:

```
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/SetupAssemblyInitializer.cs
TestSupport/TestAssemblyResolver.cs
UtilitiesCS.Test/TestAssemblyInitializer.cs
UtilitiesCS.Test/UtilitiesCS.Test.csproj
```

This is the assembly-resolution change. It is the repair for the `netstandard, Version=2.1.0.0` bind failure that aborted the first P6-T2 attempt, and it is the reason the P6-T2 re-run recorded here completed.

### Item 839, merged as pull request 876 — a production change and its test

Authored by commit `3b6cd70b4`, `fix(quickfiler): call CreateCancellationToken first in QfcHomeController.Init (#839)`:

```
QuickFiler/Controllers/QfcHomeController.cs
QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
```

`QuickFiler/Controllers/QfcHomeController.cs` is production code. It is named explicitly so that the provenance of the inherited paths is not attributed entirely to item 877's assembly-resolution change. Item 839's test change is also the reason the suite total rose from 7221 to 7222 between the two P6-T2 attempts.

## Reconciliation Against The Committed P7-T9 Inventory

The P7-T9 inventory at `evidence/qa-gates/p7-t9-changed-file-inventory.md` records a different number for what is nominally the same measurement. The discrepancy is named here, with its size and its cause, so that a reviewer comparing the two artifacts finds the explanation already written rather than having to derive it.

| Artifact | Measured | UNION_PATH_COUNT | Tree state |
|---|---|---|---|
| P7-T9 | 2026-09-13T07-19 | 100 | pre-merge, mid-Phase-7 |
| P6-T4, this artifact | 2026-09-13T15-02 | 195 | post-merge |

DISCREPANCY_SIZE: +95 paths.

The discrepancy decomposes exactly, with no remainder:

| Component | Count | Cause |
|---|---|---|
| Inherited from merging `origin/main` `a5622ab9` | +91 | content item 839 and item 877 added to `main` after the anchor |
| Own artifacts authored after P7-T9 ran | +4 | Phase 7 tasks that execute after P7-T9 |
| **Total** | **+95** | |

100 + 91 + 4 = 195.

### The 4 own paths P7-T9 could not have counted

P7-T9's own measurement is internally consistent and was correct when taken. It stages the feature folder and then measures, so it cannot count an artifact that a later task writes. Four feature-folder paths post-date it:

```
docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t9-changed-file-inventory.md
docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t13-no-temporary-files-review.md
docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/issue-updates/issue-873.md
docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t16-delivery-commit.md
```

The first is P7-T9's own artifact, which it wrote after its own staging span had already run. The other three carry recorded `Timestamp:` values of 07-22, 07-26 and 07-28, all later than P7-T9's 07-19.

Corroboration by category count: P7-T9 records 78 paths beneath this feature folder. The same category measured against the last pre-merge commit `a1b16d597` is 82. 78 + 4 = 82, and the own-path total at that commit is 104, matching the figure above.

### A correction to a figure carried in the delegation

The delegating instruction anticipated 97 inherited paths. The measured value is 91. The delegation's figure is not reproduced here as if it were measured; 91 is what `git diff --name-only refs/base-anchor-873 origin/main` reports, and it agrees independently with the `91 files changed` summary the merge itself printed. The verdict the delegation predicted is unchanged — zero matches — and the direction of the correction makes the inherited set smaller, not larger.

## Negative-Claim Auditability

SearchScope: `coverage/test-results`, `coverage/tm873-external-output`, and the 195-path changed-path union described above.

SearchPatterns: the strict default-composition pattern and the loose account-and-host containment pattern, both described under "Token Handling" and both applied to every listing.

SearchResult: none. Every one of the four searches returned 0.

## Acceptance

The search over each results-directory listing returns zero matches: confirmed, twice. The search over the changed-path union returns zero matches: confirmed. This task's acceptance is MET.
