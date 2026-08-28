---
name: 488-review-residuals
description: '#488/#475 review PASS/0 blocking at d9ed9eb2; fan-in owes: TRX host-token sanitization (partial-sanitize precedent adjudicated non-blocking), 21.4MB Cobertura pair maintainer decision, C6 stale-enumeration promotion; #670 filed for D5 unobserved fault'
metadata:
  type: project
---

2026-08-28 review of `bug/itemviewer-breadcrumb-lifecycle-defects-488` (base = epic integration `12465043`): PASS, 0 Blocking, 6 Non-blocking, all 54 AC verified. Artifacts at `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/*.2026-08-28T06-44.md`.

**Why:** residuals transfer to the quickfiler-bug-family fan-in and later reviews on this surface.

**How to apply:**
- **Host-token adjudication precedent:** 488's 19 committed TRX embed `c:\users\<account>\...` in `storage`/`codeBase` and `Megalodon4` in `runUser` while name/user/computerName were placeholder-sanitized. Adjudicated **Non-blocking** because identical tokens already exist in merged sibling evidence (501, 608, 439) on the integration branch — the branch neither introduces nor can cure the class. Sibling 489 achieved FULL sanitization, so full is the current standard; check `runUser` and lowercase `storage` paths specifically, sanitizers miss them. Fan-in owes one sed pass over `evidence/**/*.trx` + a repo-wide cleanup entry.
- **Cobertura pair:** two 10.7 MB raw XMLs committed plan-faithfully (P0-T14/P8-T6 named the paths); they are what enabled independent per-(filename,line) re-derivation. Maintainer decision owed at fan-in; replacement if removed = derived md + root element + per-file hit tables.
- **C6 stale:** the D4 construction-site enumeration is 19 executable sites, not 13; `EfcItemController.CleanupTests.cs:41` installs no sync context (file names no guarded member; suite green). `ItemViewer` ctor no longer calls `TaskScheduler.FromCurrentSynchronizationContext()` (489 removed `UiScheduler`), so null-`UiSyncContext` viewers are constructible without reflection. Promotion of a test-hygiene note owed.
- **#670** (OPEN): QFC `InitializeWebViewAsync` fault unobserved at 3 of 4 call sites; D5 guard delivered unweakened. Watch for it in 484-surface reviews.
- D1/D2 accepted residual: `SetTheme` landing between D1's synchronous dispose and the ConfigureHost post throws ObjectDisposedException — documented, intended, unreachable on the inline-post UI thread.
- `BreadcrumbItemViewerLifecycleCoordinator.cs` is at 497/500; next edit there needs a split.
- Hook note: absent `artifacts/pr_context.summary.txt` was hand-authored per the #269 fallback (`- path (+N/-N)` bullets); hook simulation via dot-source passed with the C# PASS row reading figures from committed feature-evidence Cobertura ([[project_feature-evidence-cobertura-counts-as-coverage-artifact]]).
