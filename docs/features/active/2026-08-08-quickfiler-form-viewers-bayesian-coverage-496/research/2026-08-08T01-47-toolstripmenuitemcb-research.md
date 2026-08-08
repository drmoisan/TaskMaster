## `QuickFiler/Viewers/ToolStripMenuItemCb.cs` (87 lines)

- **Epic child:** F15 (`quickfiler-form-viewers-bayesian-coverage`, issue #496), parent epic #136.
- **Measured baseline (epic manifest, indicative):** 61.5% line / 50.0% branch, 39 Cobertura-visible lines; the paired `ToolStripMenuItemCb.Designer.cs` is separately at 72.7% line / 75.0% branch, 40 lines.
- **Classification (F1 ledger rules applied directly):** `testable`. No `[ExcludeFromCodeCoverage]` attribute today (verified). It derives from `System.Windows.Forms.ToolStripMenuItem`, a lightweight `Component`, not a `Form` — the CLAUDE.md WinForms exemption ground is aimed at "form-derived and Designer-generated code," and a `ToolStripMenuItem` is neither a `Form` nor purely Designer-generated (this file is 87 lines of hand-written logic plus a thin Designer partial). No existing test file was found for this class (`QuickFiler.Test/Viewers/ToolStripMenuItemCb*` does not exist), so the gap is a genuine authoring gap, not a hidden-existing-suite situation.

### Current structure

`public partial class ToolStripMenuItemCb : ToolStripMenuItem`, decorated with `[RefreshProperties]`/`[Browsable]`/`[EditorBrowsable]` (no coverage impact — these are metadata attributes, not executable). Members:

- `ToolStripMenuItemCb()` — calls `InitializeComponent()`, then `if (Checked) { Image = Properties.Resources.CheckBoxChecked; }`, then `base.Invalidate()`. **One branch** (`Checked` true/false at construction time). Two lines are commented out (`if (CheckOnClick) { base.Click += ... }`), dead/commented code, not executable, no coverage impact.
- `Checked` property (`new bool`, hides `ToolStripMenuItem.Checked`) — setter has an `if (value) {...} else {...}` branch (sets/clears `base.Image`), then unconditionally raises `CheckedChanged?.Invoke(this, new EventArgs())` (a second, implicit branch: subscribed vs. unsubscribed) and calls `base.Invalidate()`. **Two branch points** in the setter.
- `ToolStripMenuItemCb_Click(object sender, EventArgs e)` — private handler, single line: `Checked = !Checked;`. No branch of its own, but its execution is what makes the `Checked` setter's branches reachable through the click path specifically (as opposed to setting `Checked` directly).
- `CheckedChanged` event (`new event EventHandler`) — declaration only, no branch.
- `CheckOnClick` property (`new bool`) — setter: `if (_checkOnClick) { base.Click -= X; base.Click += X; } else { base.Click -= X; }`. **One branch point**, plus the implicit "was already subscribed vs. not" state that `-=`/`+=` idempotently handle (no separate branch, but a state-transition worth a dedicated test per the `.claude/rules/general-unit-test.md` "state transitions" scenario category).
- `Image` property (`private new Image`) — trivial forward to `base.Image`, no branch.

Total real branch points: 1 (constructor) + 2 (`Checked` setter) + 1 (`CheckOnClick` setter) = 4, consistent with the reported "50.0% branch" on a small denominator (roughly 2 of 4 taken).

Dependencies: pure WinForms (`ToolStripMenuItem`, `Image`, `EventArgs`), plus `Properties.Resources.CheckBoxChecked` (an embedded bitmap resource, resolved via the generated `Resources.Designer.cs` — accessing it does not require Outlook/COM and is deterministic once the assembly's resources are loaded, which they are in-process for any test host). No Outlook Interop dependency at all.

### What is already tested vs. the coverage gap

Nothing — there is no existing test file for this class. The full member surface is a gap, but note the epic's framing that F15 needs "untaken-guard and error-path coverage, not more happy-path tests": for this file the actual gap is symmetric (no tests exist at all yet), so the priority is scenario completeness across all four branch points, not merely one:

1. Constructor with `Checked == false` at construction time (the default) vs. constructed with `Checked` pre-set to `true` before/via a derived-class or designer-serialized value — note `Checked` cannot be set before the constructor body runs from outside, so in practice the constructor's own `if (Checked)` branch is only reachable with `Checked` true if the base `ToolStripMenuItem.Checked` (which this property hides but does not fully replace as backing storage — see Latent Observation below) already reports `true` at the moment `InitializeComponent()` returns. This needs verification during planning (see below).
2. `Checked` setter, both `true` and `false` assignments, and the resulting `Image` value (`Resources.CheckBoxChecked` vs. `null`) and `CheckedChanged` event firing — with and without a subscriber attached (`?.Invoke` short-circuit).
3. `ToolStripMenuItemCb_Click` — asserting that invoking it toggles `Checked` (this exercises the setter through the click path specifically, which is the production entry point when `CheckOnClick` is wired).
4. `CheckOnClick` setter, both `true` and `false` assignments, and — critically — the state transition of subscribing, then unsubscribing, then re-subscribing (`true` -> `false` -> `true`), asserting via a direct `PerformClick()`/click-simulation or a raised `Click` event that `ToolStripMenuItemCb_Click` fires only when `CheckOnClick` is `true`.

### Latent observation (not a defect to fix, but relevant to test design)

`Checked` is declared with the `new` modifier, meaning it **hides**, not overrides, `ToolStripMenuItem.Checked`. The getter reads a private backing field `_checked`, not `base.Checked`. This means `base.Checked` (inherited `CheckState`-driven checked-state used by the WinForms rendering pipeline for a normal checkable menu item) and this class's own `_checked` field are **two independent pieces of state** that can disagree if code accesses the instance through a `ToolStripMenuItem`-typed reference rather than a `ToolStripMenuItemCb`-typed one (compile-time dispatch on `new` hides, not virtual dispatch). This is exactly the kind of "why, not what" fact a test-writer needs: tests must set/read `Checked` through the `ToolStripMenuItemCb`-typed variable to exercise this class's logic; going through a `ToolStripMenuItem` base reference would silently exercise unrelated framework code instead. Not a defect — no behavior change is implied — but worth a one-line comment in the eventual test file per `.claude/rules/general-code-change.md` "comment why, not what."

### Proposed seams

**No seam is needed.** Every member is directly constructible and directly reachable:

- `ToolStripMenuItemCb` has a public parameterless constructor and no COM/Outlook dependency, so a test can `new ToolStripMenuItemCb()` directly.
- `Checked`, `CheckOnClick`, and `Image` are `public`/`private` properties reachable either publicly or (for `Image`) not needed at all (it is never read back by any test scenario above; only `Checked`'s side effect on `base.Image`, which IS observable via the public inherited `Image` property read through the base type, matters).
- `ToolStripMenuItemCb_Click` is `private`, but it is reachable without reflection by simulating the click through the public WinForms API: `PerformClick()` on `ToolStripMenuItem` invokes the `Click` event, which — once `CheckOnClick = true` has wired `base.Click += ToolStripMenuItemCb_Click` — will invoke the private handler as a normal subscriber. This needs no reflection and no seam.
- `CheckedChanged` is a `public` event; a test can subscribe a recorder delegate directly to assert it fires (or does not fire, for the "no subscriber" branch, which trivially always passes but should still be asserted via "no exception thrown").

### Construction-without-STA determination (required before planning; not a Form)

`ToolStripMenuItem` (and its base `ToolStripItem`) does **not** require a `Form` or a message loop to construct off-screen — it is a `Component`, and this repository's existing `QuickFiler.Test` suite already constructs bare WinForms controls (`TableLayoutPanel`, `Control`, `Control.ControlCollection`) in the default apartment per F6's spec (`## Seam constraints the design must respect` → "STA last-resort clause: not invoked"). `PerformClick()` on a `ToolStripMenuItem` that is not part of a shown `ToolStrip`/`ContextMenuStrip` is a known-safe, non-message-pumping operation (it directly invokes the `Click` event delegate chain; it does not require a window handle). **The STA/DEC-1 last-resort clause does not apply to this file** — it is not a `Form`, and no test needs to show it or pump a message loop. If execution proves any specific member apartment-sensitive (unlikely, but the epic's own precedent instructs verifying rather than assuming), only that member's test moves to a dedicated `*.StaTests.cs` file per the epic's STA clause conditions; this is a contingency, not the expected outcome.

### Zero-branch caveat

Not applicable to `ToolStripMenuItemCb.cs` — it has 4 real branch points as counted above, so a genuine, non-N/A branch percentage applies.
