# Phase 2 — Consumer-side catch boundary unchanged (AC15 precondition)

Timestamp: 2026-09-09T13-03
Task: [P2-T5]

## Span 1 — catch-block scan, the `[P0-T14]` command re-run unchanged

Command:

```text
pwsh -NoProfile -Command 'Select-String -SimpleMatch -Path "QuickFiler/Controllers/QfcHomeController.cs" -Pattern "catch (System.Exception e)"'
```

EXIT_CODE: 0

Verbatim output:

```text
count=2
382: catch (System.Exception e)
399: catch (System.Exception e)
```

| Baseline (`[P0-T14]`) | After the Site A edit | Unmoved |
|---|---|---|
| 2 matches, at 382 and 399 | **2 matches, at 382 and 399** | yes |

Exactly two matches, still at lines 382 and 399, unmoved. This is expected: the edit region begins at
line 405, below both catch blocks, so nothing above it shifted. Neither catch was widened to enclose
the `finally`, and the two remain separate blocks.

## Span 2 — anchored name-only diff, filtered for the excluded consumer file

Command: `git diff --name-only (git merge-base HEAD origin/main)`, filtered for
`QfcFormController.SetupDisposal.cs`.
EXIT_CODE: 0

Result: **0 matching paths.** `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` does not
appear in the anchored diff.

## Span 3 — porcelain status companion, filtered for the same file

Command: `git status --porcelain --untracked-files=all`, filtered for
`QfcFormController.SetupDisposal.cs`.
EXIT_CODE: 0

Result: **0 matching paths.** The porcelain companion is required because a name-listing diff
enumerates tracked changes only and cannot report an untracked path; this span closes that gap.

## Finding recorded here for the Phase 5 footprint gates

Span 2 was run in full, not only filtered, and it returned a **large inherited path set** that this
feature did not author. The cause is structural: this execution branch descends from the epic
integration branch `epic/review-residuals-2026-09-08-integration`, so
`git merge-base HEAD origin/main` resolves to `6f08302a4f0af0061f27856e8a654f819df902aa` — below every
sibling-feature commit the integration branch already carries. The anchored two-dot diff therefore
lists sibling features 813, 815, 817, 823, 824, 825 and 826, the epic documents, the promoted
potential records, entries under `.claude/agent-memory/`, and files including
`UtilitiesCS.Test/UtilitiesCS.Test.csproj`, `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`
and `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.

The plan's decision 6 anticipated this mechanism but scoped it to four feature-folder documents; the
inherited set is materially larger. None of it is authored by this feature. The discriminating
observation, run at the same moment:

Command: `git diff --name-only HEAD`
EXIT_CODE: 0

Verbatim output:

```text
QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
QuickFiler/Controllers/EfcHomeController.cs
QuickFiler/Controllers/QfcHomeController.cs
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/plan.2026-09-08T23-50.md
```

That span — tracked modifications against `HEAD`, paired with the untracked entries in the porcelain
span — is this feature's authored change and contains no `.csproj` and no out-of-scope path. `[P5-T6]`
and `[P5-T7]` record both spans and evaluate AC19 and AC20 against the authored change while
disclosing the inherited set in full.

Output Summary: exactly two `catch (System.Exception e)` matches are reported, still at lines 382 and
399 and therefore unmoved from baseline, and
`QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` appears in neither the anchored diff span
nor the porcelain span. The acceptance condition for this task is met. A separate finding about the
anchored diff's inherited path set is recorded above for the Phase 5 footprint gates.
