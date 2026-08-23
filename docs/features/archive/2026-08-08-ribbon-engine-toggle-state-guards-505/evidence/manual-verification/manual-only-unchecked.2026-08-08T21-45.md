# P6-T3 — AC-22 Remains Unchecked (rule 13)

Timestamp: 2026-08-08T21-45

Command:

```
pwsh -NoProfile -Command "Select-String -Path 'spec.md' -Pattern '\*\*AC-22' | ForEach-Object { '{0}: {1}' -f $_.LineNumber, $_.Line }"
```

run from `<FEATURE>`.

EXIT_CODE: 0

## Output Summary

The AC-22 criterion line in `<FEATURE>\spec.md`, quoted verbatim from line **653**:

```
- [ ] **AC-22 (iAC9) - MANUAL-ONLY.** In a live Outlook session with "Show add-in user interface
```

The line still begins `- [ ] **AC-22`. The checkbox is **unchecked**.

## Statement

AC-22 is MANUAL-ONLY. Checking it off from automated evidence — unit tests, source inspection, the
coverage documents, or any artifact this delivery produced — is a **policy violation**. It requires
recorded live-Outlook verification, and it is the only criterion in this spec that automated
evidence cannot satisfy: VSTO does not report a signature-incompatible callback, so the fact that
the corrected `getPressed` actually **binds** is observable only inside a running Outlook process.

The maintainer checklist for it is at
`<FEATURE>\evidence\manual-verification\ac22-checklist.2026-08-08T21-44.md`, carrying
`Status: PENDING MAINTAINER EXECUTION`. AC-22 may be checked off only by the maintainer, citing a
completed run of that checklist.

Binary outcome: PASS — AC-22 verified still unchecked at the end of this delivery.
