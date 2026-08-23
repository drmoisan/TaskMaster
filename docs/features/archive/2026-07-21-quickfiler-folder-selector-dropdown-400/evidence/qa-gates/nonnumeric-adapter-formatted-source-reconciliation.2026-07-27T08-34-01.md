# P9-T12 formatted-source reconciliation retry 1

Timestamp: 2026-07-27T08:34:01Z
Predecessor evidence: `evidence/qa-gates/nonnumeric-adapter-final-csharpier.2026-07-27T08-31.md`
Inspection: retained formatter hash ledger; static popup source and exclusion review; `git diff --check`.
Commands not run: CSharpier, build, analyzer, nullable, VSTest, and coverage commands.

## Formatter delta

The retained P9-T16 artifact records identical before/after SHA-256 values for
the coordinator, ItemViewer breadcrumb, and both P9-T13 test sources. It
records a one-file delta for `BreadcrumbPopupUiOperations.cs`:

| Path | Before SHA-256 | After SHA-256 | Current match |
| --- | --- | --- | --- |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | `0BE8FAAE1A774332A2B8E0B3A2C99292996D8C5165058D1A7D7B4717EFDD7F8D` | `A7CCB93C9F40D236A278DACD890807CECA371ECB886B343E50272AA4E054D108` | yes |

The command recorded in the retained artifact is CSharpier `format`, followed
by CSharpier `check`, both with exit code 0. The live popup hash equals the
recorded post-format hash, and static review found no semantic defect in the
retained source. This one-file delta is formatting-only.

## Static popup contract

- `BreadcrumbPopupUiOperations.cs` is 476 physical lines, within the 500-line cap.
- The popup has exactly seven exclusions, at lines 97, 372, 375, 382, 386,
  404, and 439.
- `NavigateToDocument` and `NavigateToDocumentCore` are unexcluded. The latter
  owns null validation and delegates to `BindProductionNavigation` only after
  validation succeeds.
- The excluded binding method contains the direct CoreWebView2/owner event
  handler subscription and exact matching unsubscription delegates. The
  host-neutral lifecycle, readiness, validation, and cleanup branches remain
  in `BreadcrumbPopupLifecycleOperations` and unexcluded popup members.
- `git diff --check` exited 0. Its line-ending advisories did not report a
  whitespace error.

`P9-T12` remains checked because the corrected P9-T12 source contract passes
this bounded review. The post-format popup hash changed; a fresh P9-T14
Debug/Any CPU build and focused-test gate is required before any later QA
gate. P9-T16 and downstream gates remain subject to their existing restart
requirements.

RESULT: PASS
