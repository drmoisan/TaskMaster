# P4-T2 Host-token context survey (research section 14 item 2)

Timestamp: 2026-09-29T19-59
Command: the P4-T2 pwsh payload (host contexts written to SCRATCH\host-contexts.txt; HOST-FORMS, HOST-HYPHEN, HOST-EMBEDDED, EMBEDDED-TOKENS, ACCOUNT-EMBEDDED and NON-ACCOUNT-PROFILE-FILES probes, every token derived at run time from the environment and passed through [regex]::Escape); the executor then read SCRATCH\host-contexts.txt
EXIT_CODE: 0
Output Summary:
- HOST-CONTEXT-LINES=139
- HOST-FORMS=1 (one case variant; the text is not recorded)
- HOST-HYPHEN=0
- HOST-EMBEDDED=0
- EMBEDDED-TOKENS=2
- ACCOUNT-EMBEDDED=0
- NON-ACCOUNT-PROFILE-FILES=22 (recorded, not gated; the plan expected 19, a difference of +3; rule 8 covers every gate-four match whatever the count, per D17)
- The three boundary counts that would leave a residual the substring gates report (HOST-HYPHEN, HOST-EMBEDDED, ACCOUNT-EMBEDDED) are all 0, so STOP: IDENTIFIER OUTSIDE RULE BOUNDS does not apply.

HOST-CONTEXT-VERDICT: ALL-HOST-REFERENCES
- Basis: all 139 lines use the host token as a machine identifier. 137 lines quote default vstest or dotnet-coverage output file names, in which the account and host tokens are joined by underscores (for example `<user>_<host>_<timestamp>.coverage`, `.cobertura.xml` or `_net481.trx`), inside coverage-attachment paths, merge commands or TRX paths. The remaining 2 lines are review and redaction notes that name the host token as a local identifier to be searched for or avoided. No line uses the token as an ordinary word.
- SCRATCH\host-contexts.txt stays outside the repository (C5); no line of it is copied here.
