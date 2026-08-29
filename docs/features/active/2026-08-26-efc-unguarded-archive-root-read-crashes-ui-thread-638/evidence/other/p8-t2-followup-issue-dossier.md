# [P8-T2] Follow-up issue dossier for the three non-goals (Issue 638)

Timestamp: 2026-08-29T12-46

Command: citation re-derivation only — `Select-String` over the three cited files; no build
or test command was run.

EXIT_CODE: 0

Output Summary: three ready-to-file follow-up issues, one per non-goal recorded in the
spec's Scope & Non-Goals section. Each carries a title, a one-paragraph body, its verified
citations and a `ProposedLabels:` line. Filing is an orchestrator responsibility under
`.claude/skills/feature-promotion-lifecycle/SKILL.md` and is not performed by this plan.

## Non-goal (a) — Guard the archive-root getter against COM failure, not only against an unresolvable root

Body. `IOlObjects.ArchiveRootPath` is implemented by `AppOlObjects` as a lazily computed
property whose getter makes two live Outlook COM calls before it can validate anything. When
either call fails — a disconnected or restarting Outlook process, a store that has gone
offline, an RPC server that is unavailable — the getter raises a `COMException`, not the
`InvalidOperationException` that the archive-root validator raises for an unresolvable or
cross-store root. Issue 638 deliberately narrowed its guard to `InvalidOperationException`
and added a regression test,
`MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates`, that pins the
COM failure as still propagating, so the narrowing is a documented decision rather than an
oversight. The follow-up is to decide, at the `AppOlObjects` boundary rather than at each
call site, whether a COM failure reading the archive root should surface as a redacted
user-facing diagnostic in the same shape the validator already uses, or should continue to
propagate. Deciding it at the boundary avoids duplicating the choice across the eight
archive-root call sites that exist across `EfcDataModel` and `EfcFormController`.

Verified citations.

- `TaskMaster/AppGlobals/AppOlObjects.cs:253-267` — the `ArchiveRootPath` getter.
- `TaskMaster/AppGlobals/AppOlObjects.cs:260` — `Path.Combine(Root.FolderPath, "Archive")`,
  a live COM read of `Root.FolderPath`.
- `TaskMaster/AppGlobals/AppOlObjects.cs:261` — `ArchiveRoot?.FolderPath`, a second live COM
  read.
- `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:44` and `:56` — the only two throw sites,
  both raising `InvalidOperationException`.
- `QuickFiler/Controllers/EfcDataModel.cs:287` — the narrowed
  `catch (InvalidOperationException ex)` added by issue 638.

ProposedLabels: bug, quickfiler, outlook-interop, follow-up

## Non-goal (b) — Review the log-only `async void` boundary sinks in `EfcFormController`

Body. `EfcFormController` exposes five `async void` WinForms click handlers, each of which
immediately awaits an `internal async Task` sibling that wraps its work in
`try { ... } catch (System.Exception ex) { BoundaryErrorSink(ex.Message, ex); }`. The sink
defaults to `(message, exception) => logger.Error(message, exception)`, so every failure
reaching one of these boundaries is written to the log and nothing is shown to the user; the
button appears to do nothing. That is the same silent-swallow symptom issue 638 addressed
inside `EfcDataModel`, but at a different layer and with a different remedy, so it was
excluded from 638's scope. The follow-up is to decide which of these boundaries should also
raise a redacted user-facing diagnostic, and whether `BoundaryErrorSink` should gain a
user-notification arm rather than each handler growing its own.

Verified citations.

- `QuickFiler/Controllers/EfcFormController.cs:442`, `:460`, `:477`, `:495`, `:557` — the
  five `async void` click handlers.
- `QuickFiler/Controllers/EfcFormController.cs:445-458` — the representative sibling
  `ButtonCancelClickAsync`, showing the `catch` at `:454-457` and the sink call at `:456`.
- `QuickFiler/Controllers/EfcFormController.cs:129` —
  `(message, exception) => logger.Error(message, exception)`, the default sink, which is
  log-only.

ProposedLabels: bug, quickfiler, ui-diagnostics, follow-up

## Non-goal (c) — Guard the five archive-root reads in `EfcFormController`

Body. Issue 638 guarded the three unguarded `ArchiveRootPath` reads in `EfcDataModel` and
left `EfcFormController` untouched, because widening the change would have taken the
diff outside the footprint AC18 pins and would have required a different test arrangement
for each site. `EfcFormController` still reads `_globals.Ol.ArchiveRootPath` at five places,
none of them guarded, so an unresolvable or cross-store archive root still raises
`InvalidOperationException` from each. Four of the five are absorbed by the boundary sinks
described in non-goal (b) and therefore present as a silent no-op; the fifth sits on the
breadcrumb bind path. The follow-up is to route all five through the same
`TryGetArchiveRoot`-shaped helper issue 638 introduced, or through a shared helper promoted
to a location both controllers can reach, and to add the equivalent regression tests.

Verified citations.

- `QuickFiler/Controllers/EfcFormController.cs:529` — `_globals.Ol.ArchiveRootPath,`
- `QuickFiler/Controllers/EfcFormController.cs:539` — `_globals.Ol.ArchiveRootPath,`
- `QuickFiler/Controllers/EfcFormController.cs:836` — `_globals.Ol.ArchiveRootPath,`
- `QuickFiler/Controllers/EfcFormController.cs:846` — `_globals.Ol.ArchiveRootPath,`
- `QuickFiler/Controllers/EfcFormController.cs:987` —
  `await _router.BindRowsAsync(rows, scores, _globals.Ol.ArchiveRootPath, Token);`
- `QuickFiler/Controllers/EfcDataModel.cs:280-297` — `TryGetArchiveRoot`, the helper shape
  issue 638 introduced and the one these five sites would adopt.

ProposedLabels: bug, quickfiler, outlook-interop, follow-up

REMEDIATION-REQUIRED: AC20 unmet — three follow-up issues not yet filed. This dossier is the
ready-to-file input: it carries one section per non-goal with a title, a body, verified
citations and proposed labels. The filing route is the promotion lifecycle defined by
`.claude/skills/feature-promotion-lifecycle/SKILL.md`, which the orchestrator runs after
this plan returns. AC20 is left unchecked in `spec.md` until the three issue numbers are
recorded in the spec's Rollout & Follow-up section.
