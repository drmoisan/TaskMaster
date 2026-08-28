# P2-T9 — QuickFiler/Viewers/ToolStripMenuItemCb.cs is untouched

Timestamp: 2026-08-28T00-31
Command: git diff --numstat cecd78130a489fcfdc2ddac7970f344256f4a75a -- QuickFiler/Viewers/ToolStripMenuItemCb.cs
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

The command produces **no output row at all**. `git diff --numstat` emits one row per changed path,
so an empty result is the positive proof that this file differs in no line from `BASELINE_SHA`
(`cecd78130a489fcfdc2ddac7970f344256f4a75a`). The file is not in the Phase 2 diff and was not
reformatted by any CSharpier invocation.

## The two regions P2-T9 names, read back from the current working tree

`Checked` setter (`:32-50`) — unchanged:

```
        public new bool Checked
        {
            get => _checked;
            set
            {
                _checked = value;
                if (value)
                {
                    base.Image = Properties.Resources.CheckBoxChecked;
                }
                else
                {
                    base.Image = null;
                }
                //base.CheckedChanged?.Invoke(sender, e);
                CheckedChanged?.Invoke(this, new EventArgs());
                base.Invalidate();
            }
        }
```

`CheckOnClick` setter (`:63-79`) — the `base.Click -= …; base.Click += …;` idempotent-subscribe
pattern is unchanged, including the unconditional `-=` in the `else` branch.

## No assignment to base.Checked was introduced

Command: `git grep -n -E "base\.Checked\s*=" -- QuickFiler/Viewers/ToolStripMenuItemCb.cs`
Result: **zero matches** (exit 1, which is `git grep`'s no-match exit).

This matters because the shadowing design is deliberate: `ToolStripMenuItemCb` declares
`public new bool Checked` backed by its own `_checked` field and a `public new event EventHandler
CheckedChanged`. Writing through to the base `ToolStripMenuItem.Checked` would raise the *base*
`CheckedChanged` in addition to the shadowed one and would reintroduce, from a different direction,
the double-notification the Phase 2 deletions removed. The only base member the setter writes is
`base.Image`, and the only base member `CheckOnClick` touches is `base.Click`.

Output Summary: `QuickFiler/Viewers/ToolStripMenuItemCb.cs` is **unchanged**.
`git diff --numstat <BASELINE_SHA>` produces no output row, so the file differs in no line from the
baseline commit. Its `Checked` setter at `:32-50` and the `base.Click -= …; base.Click += …;`
pattern in the `CheckOnClick` setter at `:63-79` were read back and are byte-identical to baseline,
and a `git grep` for an assignment to `base.Checked` returns zero matches, so the shadowed-property
design that makes the deleted `MenuItem_CheckedChanged` handlers redundant is intact.
