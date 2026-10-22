# AC-6 Invariant Gate — `KeyboardHandler` Unchanged (P4-T4)

Timestamp: 2026-08-28T16-05
EXIT_CODE: 0

## Commands (with the concrete BASELINE_SHA substituted)

```
git status --porcelain -- QuickFiler/Controllers/KeyboardHandler.cs
git diff 361a49b884a4e3fe192bf04bae05151c598398fa -- QuickFiler/Controllers/KeyboardHandler.cs
```

BASELINE_SHA is `361a49b884a4e3fe192bf04bae05151c598398fa`, recorded by P0-T2.

## Output Summary

Both commands produced **empty output** and exited 0.

- `git status --porcelain` on the path: empty — the file is neither modified in the working tree
  nor staged nor untracked.
- `git diff <BASELINE_SHA>` on the path: empty — the file is byte-identical to its state at the
  plan's baseline commit.

The AC-6 invariant therefore holds: `QuickFiler/Controllers/KeyboardHandler.cs` is unchanged by
this fix.

## Why the invariant is the right gate

The issue's original hypothesis was that QuickFiler's keyboard hooking is scoped wider than the
QuickFiler window. Root-cause analysis refuted that in its literal form: `KeyboardHandler` is
ordinary WinForms `PreviewKeyDown`/`KeyDown` wiring confined strictly to QuickFiler's own control
tree, instantiated per launch and never static or shared, so it cannot receive events from native
Outlook windows. The actual mechanism is focus routing. Leaving the file untouched is therefore a
positive design commitment recorded in `spec.md` Scope & Non-Goals, not merely an omission, and
this gate is what makes it auditable.

No behavior change occurs outside the focus/activation scope defined in Scope & Non-Goals: the
complete production change set is the eleven files enumerated in
`evidence/baseline/scope-lock.md`, all of which are focus-permission, deactivation-routing, or
selector-cancellation surface.
