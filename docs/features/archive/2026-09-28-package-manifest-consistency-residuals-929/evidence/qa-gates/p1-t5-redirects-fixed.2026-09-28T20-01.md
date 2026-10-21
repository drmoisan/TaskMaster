# P1-T5 — SVGControl binding redirects corrected

Timestamp: 2026-09-30T10-07
Command: Edit SVGControl/app.config lines 15 and 19; git diff --numstat 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- SVGControl/app.config; git diff 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- SVGControl/app.config; CMD-REDIRECT-OBSERVE (all in one pwsh invocation beginning with Set-Location "<execution-worktree-root>")
EXIT_CODE: 0
Output Summary:
- NUMSTAT=2	2	SVGControl/app.config (2 added, 2 deleted)
- New lines, verbatim from git diff <BASE-SHA> -- SVGControl/app.config (hunk @@ -12,11 +12,11 @@):
  - line 15: `+        <bindingRedirect oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0" />`
  - line 19: `+        <bindingRedirect oldVersion="0.0.0.0-6.0.3.0" newVersion="6.0.3.0" />`
- Indentation unchanged; the edit touched only the two bindingRedirect lines.
- FIZZLER_REPAIRS=0 UNSAFE_REPAIRS=0 EXAMINED=4 (P0-T18 measured 1, 1 and 4 on the same command)
- The Fizzler value 1.3.1.0 equals FIZZLER_ASM and the Unsafe value 6.0.3.0 equals UNSAFE_ASM from P0-T18.
