# P1-T4 EDIT-FIZZLER QuickFiler/app.config (lines 50-51)

Timestamp: 2026-10-02T03-21
Command: Edit tool on `QuickFiler/app.config` (two-line old_string, identity line plus redirect line); git -C <execution-worktree-root> diff --numstat 860d67bf4fddecb929e0d6c166065fd1ee752feb -- QuickFiler/app.config; git -C <execution-worktree-root> diff 860d67bf4fddecb929e0d6c166065fd1ee752feb -- QuickFiler/app.config; git -C <execution-worktree-root> ls-files --eol -- QuickFiler/app.config; pwsh -NoProfile -Command '(Get-Content -LiteralPath <execution-worktree-root>/QuickFiler/app.config -AsByteStream -TotalCount 3) -join [char]44'; Grep `name="Fizzler"` -A 1 and Grep `name="System.ClientModel"` -A 1 on the file
EXIT_CODE: 0

```text
NUMSTAT  1	1	QuickFiler/app.config
DIFF     -        <bindingRedirect oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0" />
         +        <bindingRedirect oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0" />
EOL      i/lf    w/crlf  attr/text=auto  QuickFiler/app.config
BOM-BYTES QuickFiler/app.config=239,187,191   (P0-T7 reference 239,187,191; equal)
Fizzler  50: <assemblyIdentity name="Fizzler" ... />
         51: <bindingRedirect oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0" />
ClientModel 102: <assemblyIdentity name="System.ClientModel" ... />
         103: <bindingRedirect oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0" />   (unchanged from P0-T5)
```

Acceptance (all six): (1) numstat `1	1`; (2) one `-` and one `+` content line with the required text; (3) `w/crlf`; (4) BOM bytes equal the P0-T7 value; (5) Fizzler redirect reads 1.3.1.0 in both attributes; (6) System.ClientModel redirect unchanged at 1.3.0.0.

Output Summary: One-line Fizzler redirect edit applied to QuickFiler/app.config; CRLF and BOM preserved; System.ClientModel redirect unchanged.
