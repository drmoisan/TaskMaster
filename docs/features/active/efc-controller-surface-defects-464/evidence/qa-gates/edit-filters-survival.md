# Edit Filters survival evidence

Task: [P2-T7] (Phase-2 state); re-verified and appended by [P9-T7] after Phases 5 through 7

---

## Phase-2 record

Timestamp: 2026-08-28T00-20
Command: `grep -n 'EditFiltersMenuItem.Click' QuickFiler/Controllers/EfcFormController.cs`; `grep -n 'public void EditFiltersMenuItem_Click' QuickFiler/Controllers/EfcFormController.cs`; `git show 002335989830ba9f3ad802858ef0b794f6281750:QuickFiler/Controllers/EfcFormController.cs | sed -n '398p'` compared byte-for-byte against `sed -n '398p' QuickFiler/Controllers/EfcFormController.cs`; `git diff 002335989830ba9f3ad802858ef0b794f6281750 -- QuickFiler/Viewers/EfcViewer.Designer.cs`
EXIT_CODE: 0

### The subscription

Delivered source line performing the Edit Filters subscription in `EfcFormController.WireEventHandlers`,
at line **398**:

```csharp
            _formViewer.EditFiltersMenuItem.Click += EditFiltersMenuItem_Click;
```

### Byte-identity with the pre-change text

The same line was extracted from `BASELINE_SHA` at `EfcFormController.cs:398` and compared with the
delivered line, both rendered with `cat -A` so that trailing whitespace and line endings are visible. The
two renderings are identical, including leading indentation and the terminal `$`:

```
            _formViewer.EditFiltersMenuItem.Click += EditFiltersMenuItem_Click;$
```

The subscription statement is therefore **byte-identical to its pre-change text and still at line 398**.

### The target method

`EfcFormController.EditFiltersMenuItem_Click` is declared at line **559**:

```csharp
        public void EditFiltersMenuItem_Click(object sender, EventArgs e)
```

This is the live route the controller subscribes to directly, bypassing the viewer. It was pinned green
both before and after the Phase 2 deletion by
`FormEditFiltersMenuItemClick_IsStillDeclaredOnEfcFormController`, recorded in
`evidence/regression-testing/466-viewer-fail.md` and `evidence/regression-testing/466-viewer-pass.md`.

### `EfcViewer.Designer.cs`

```
git diff 002335989830ba9f3ad802858ef0b794f6281750 -- QuickFiler/Viewers/EfcViewer.Designer.cs
```

produces **0 lines of output**: the Designer file is unmodified. This matters because the Designer never
wired `EditFiltersMenuItem.Click` to the viewer-side handler, which is exactly why that handler was
unreachable; leaving the Designer untouched means the deletion removed the trap without changing any
wiring.

Output Summary: At the end of Phase 2 the Edit Filters subscription is at `EfcFormController.cs:398`,
byte-identical to its pre-change text, and its target `EditFiltersMenuItem_Click` is declared at `:559`.
`QuickFiler/Viewers/EfcViewer.Designer.cs` shows an empty diff against BASELINE_SHA.

---

## Post-Phase-7 record

Reserved for `[P9-T7]`, which re-reads the delivered subscription line number and re-checks byte-identity
after Phases 5 through 7 have edited `EfcFormController.cs`, and appends its findings below. Until that
task runs, the location recorded above is the Phase-2 location and may shift.
