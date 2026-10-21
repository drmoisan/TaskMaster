# P3-T5 — item-group call site routed through the seam (pre-format capture)

Timestamp: 2026-09-13T15-42

Command: pwsh -NoProfile -Command '$p="QuickFiler/Controllers/QfcQueue.Enqueue.cs"; $l=@(Get-Content -LiteralPath $p); $a=@($l | Select-String -SimpleMatch "ItemGroupFactory("); $b=@($l | Select-String -SimpleMatch "AddAsync("); "COUNT-ItemGroupFactory=$($a.Count)"; "COUNT-AddAsync=$($b.Count)"; foreach ($x in $a) { "LINE=$($x.LineNumber) WIDTH=$($x.Line.Length)"; "TEXT=[$($x.Line)]" }'

EXIT_CODE: 0

Output Summary:
- COUNT-ItemGroupFactory=1 — the single-line token `ItemGroupFactory(` occurs exactly once.
- COUNT-AddAsync=0 — the token `AddAsync(` occurs zero times in this file.
- The sole pre-substitution occurrence of `AddAsync(` in this file was the per-row construction at
  line 177 of the recorded anchor, which is the call this task replaced. No other occurrence
  existed, so the zero-occurrence condition is satisfied without deleting any unrelated call.
- The index expression passed to the seam is unchanged: `items[i - start]` for the mail item and
  `i` for the index, exactly as the replaced call passed them.
- Measured column width of the substituted statement before formatting: 102.

PreFormatStatement:                 .SelectAwait(async i => (i: i, grp: await ItemGroupFactory(tlp, items[i - start], i)))

Note on the width. The statement measured 94 columns at the recorded anchor; substituting
`ItemGroupFactory` for `AddAsync` adds eight characters, giving the measured 102 and taking the
statement past CSharpier's 100-column default. The re-wrap by CMD-FORMAT-SCOPED in P3-T8 is
therefore expected rather than a defect. This pre-format capture is retained only to make that
re-wrap auditable; the authoritative form that AC5 evidence cites is the `PostFormatStatement:`
line of the P3-T8 artifact.
