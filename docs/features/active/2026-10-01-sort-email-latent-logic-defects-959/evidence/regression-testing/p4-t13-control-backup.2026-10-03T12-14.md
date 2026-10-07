# P4-T13 Control Backup of the Fixed A

Timestamp: 2026-10-03T12-14
Command: CMD-CONTROL-BACKUP (byte copy of UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs to the git-ignored coverage\control-959\SortEmail.AttachmentSaving.fixed.bak, then the SHA-256 of both)
EXIT_CODE: 0 (the payload's process exit code; the payload prints no exit label)
Output Summary: The fixed A was copied to the git-ignored backup. The backup hash equals the source hash, and both equal the AFTER hash of A that P4-T10 recorded.

```
FIX-HASH-A: 9F1A8D46B77388DE545B8A9D611A431C13749205C6CF5B7A3B078E57D72418C2
BACKUP-HASH-A: 9F1A8D46B77388DE545B8A9D611A431C13749205C6CF5B7A3B078E57D72418C2
```

## Acceptance (P4-T13, both required)

1. `BACKUP-HASH-A:` equals `FIX-HASH-A:`: met.
2. `FIX-HASH-A:` equals the `AFTER` hash of A recorded by P4-T10 (9F1A8D46B77388DE545B8A9D611A431C13749205C6CF5B7A3B078E57D72418C2, qa-gates/p4-t10-rr-fix.2026-10-03T12-11.md): met.
