# P1-T14 EDIT-FIZZLER UtilitiesCS.Test/app.config (lines 46-47)

Timestamp: 2026-10-02T03-21
Command: Edit tool on `UtilitiesCS.Test/app.config` (two-line old_string, identity line plus redirect line); git -C <execution-worktree-root> diff --numstat 860d67bf4fddecb929e0d6c166065fd1ee752feb -- <the ten remaining Write Set configs, one invocation>; git -C <execution-worktree-root> diff -U0 860d67bf4fddecb929e0d6c166065fd1ee752feb -- <the same ten paths>; git -C <execution-worktree-root> ls-files --eol -- <the same ten paths>; pwsh -NoProfile -Command '(Get-Content -LiteralPath <execution-worktree-root>/UtilitiesCS.Test/app.config -AsByteStream -TotalCount 3) -join [char]44'; Grep `name="Fizzler"` -A 1 and Grep `name="System.ClientModel"` -A 1 (glob `**/app.config`, path `<execution-worktree-root>`; the 17-file result is read for this path)
EXIT_CODE: 0

```text
NUMSTAT  1	1	UtilitiesCS.Test/app.config
DIFF     -        <bindingRedirect oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0" />   (diff -U0 hunk @@ -47 +47 @@)
         +        <bindingRedirect oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0" />
EOL      i/lf    w/crlf  attr/text=auto  UtilitiesCS.Test/app.config
BOM-BYTES UtilitiesCS.Test/app.config=239,187,191   (P0-T7 reference 239,187,191; equal)
Fizzler  46: <assemblyIdentity name="Fizzler" ... />
         47: <bindingRedirect oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0" />
ClientModel 122-123: <bindingRedirect oldVersion="0.0.0.0-1.16.0.0" newVersion="1.16.0.0" />   (unchanged from P0-T5)
```

Acceptance (all six): numstat `1	1`; one `-` and one `+` content line with the required text; `w/crlf`; BOM bytes equal P0-T7; Fizzler 1.3.1.0 in both attributes; System.ClientModel unchanged at 1.16.0.0.

Output Summary: One-line Fizzler redirect edit applied to UtilitiesCS.Test/app.config; CRLF and BOM preserved; System.ClientModel redirect unchanged.
