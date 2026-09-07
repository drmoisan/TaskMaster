# P1-T14 — Phase 1 scope audit

Timestamp: 2026-09-07T14-27
Task: [P1-T14]
Issue: #796
Channel used: A
Base anchor: c7ae69f1

Commands, in the order run:

1. `pwsh -NoProfile -Command 'git add QuickFiler QuickFiler.Test docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796'`
2. `pwsh -NoProfile -Command 'git diff --cached --name-status c7ae69f1'`
3. `pwsh -NoProfile -Command 'git status --porcelain --untracked-files=all'`

EXIT_CODE: 0 for all three.

The staging span is required because a name-listing diff cannot see a file this phase
created. Two of the eight non-feature-folder paths below are creations and appear as
`A` only because they were staged first.

Step 1 emitted 25 `LF will be replaced by CRLF` warnings, one per Markdown evidence
file. These are Git line-ending normalisation notices from the repository's
`core.autocrlf` setting, not errors, and the command exited 0.

## Non-feature-folder paths the diff listed

Eight paths, every one of them on the permitted list for this point in plan order:

| Path | Diff status | Landed by |
|---|---|---|
| QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs | A | P1-T1 (created) |
| QuickFiler/Viewers/BreadcrumbDropDownHost.cs | M | P1-T2 |
| QuickFiler/QuickFiler.csproj | M | P1-T3 |
| QuickFiler/Controllers/QfcFormController.Deactivate.cs | M | P1-T4 |
| QuickFiler/Controllers/QfcItemController.EventHandlers.cs | M | P1-T4 |
| QuickFiler.Test/QuickFiler.Test.csproj | M | P1-T6 |
| QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs | A | P1-T5 (created) |
| QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs | M | P1-T7 |

The set is the union of the paths the seven preceding authoring tasks of this phase
edit or create. No other write-set path appears, which is correct, because no
behavioural phase has run yet.

`QuickFiler/Controllers/QfcItemController.EventHandlers.cs` is a member because
P1-T4 added the observational selector-open member on the concrete item controller in
that file. That edit and the deactivate-handler edit are both observational and
neither changes control flow, so the AC6 ordering constraint that Phase 1 contains no
behavioural change still holds.

## Paths that must NOT appear, and do not

| Path | Present in the diff |
|---|---|
| QuickFiler/Interfaces/IQfcItemController.cs | no |
| QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs | no |

The adopted mechanism changes no interface, so neither the interface nor its compiled
hand-written implementor in the test assembly is touched.

## Feature-folder paths the diff listed

Twenty-four further paths, all inside
docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796:
the modified plan file carrying this run's check-offs, thirteen Phase 0 baseline
artifacts, seven Phase 1 qa-gates artifacts, and two Phase 1 regression-testing
artifacts. The diff total is therefore 32 paths: 8 write-set plus 24 feature-folder.

This artifact is not among them. It is written after the diff that reports on it, so
it cannot appear in its own listing.

## Porcelain output

Every porcelain entry is either one of the eight write-set paths above or a path
inside the feature folder. No entry lies outside those two sets. The
`PRE-EXISTING-DIRTY-SET:` recorded in evidence/baseline/p0-t14-scope-baseline.md is
EMPTY, so this gate is evaluated strictly with no admitted exceptions to subtract.

Output Summary: The Phase 1 diff against c7ae69f1 lists 8 non-feature-folder paths,
all 8 on the permitted list, and the two prohibited paths are absent. Porcelain lists
no path outside the feature folder and the write set.
