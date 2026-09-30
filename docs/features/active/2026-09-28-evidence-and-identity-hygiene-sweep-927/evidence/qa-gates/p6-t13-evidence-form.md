# P6-T13 Evidence-form clauses (AC18)

Timestamp: 2026-09-29T22-25
Command: the P6-T13 pwsh payload of plan revision 1.16, verbatim, run from the item worktree root (tracked plus untracked artifacts under this feature folder's evidence tree; four-field check; anchored three-dot diff origin/main...HEAD for added raw documents; untracked raw documents).
EXIT_CODE: 0
Output Summary:
- ARTIFACTS=55 (before this artifact was written)
- NON-MD=0
- MISSING-FIELDS=0 (the write-set inventory under evidence/other carries the four fields, so no exemption is recorded)
- MERGE-BASE-NOW=ddbab26a0149bf2ca5d0256e60686ad79e74d90c
- ADDED-RAW=0
- ADDED-RAW-UNTRACKED=0
- The artifacts this task and later tasks write are Markdown carrying Timestamp:, Command:, EXIT_CODE: and Output Summary: by construction.
- The redaction helper, its log, its saved originals and the downloaded CI artifact live under the scratch expression Join-Path $env:TEMP "hygiene-927", outside the repository; the raw tool outputs (build logs, the TRX document, the Cobertura documents, the Pester JUnit and coverage documents) live under the ignored coverage/ and artifacts/ directories. None is committed.
