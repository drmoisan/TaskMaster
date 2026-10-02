# P0-T8 Comment-Only Filter Control

Timestamp: 2026-10-02T01-10
Command: CMD-COMMENT-ONLY-CONTROL (pwsh -NoProfile -Command applying the CMD-COMMENT-ONLY filter to three literal samples; no file is read)
EXIT_CODE: 0
Output Summary:
NEG CHANGED=3 NONCOMMENT=1 VERDICT=1
POS CHANGED=2 NONCOMMENT=0 VERDICT=0
EMPTY CHANGED=0 NONCOMMENT=0 VERDICT=1
(The filter rejects a diff with a code line, accepts a comment-only diff, and rejects a diff with no body line.)
