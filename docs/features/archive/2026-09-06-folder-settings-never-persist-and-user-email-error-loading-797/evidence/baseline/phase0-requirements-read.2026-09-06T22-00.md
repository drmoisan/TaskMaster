# Phase 0 — Requirements Sources Read (Issue #797)

Timestamp: 2026-09-07T09-11

Command: `Get-FileHash -Algorithm SHA256 -Path <three requirements sources>`

EXIT_CODE: 0

## Sources read and their SHA-256 digests

| Source (repository-relative) | SHA-256 |
|---|---|
| docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md | D1C87062FCF7FF6D61C2B4BD099C366B50A5587920298E3CD0E9322998493C23 |
| docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md | 7404868CE8EF64EC9353077362049F6231F4237046F8F8810D453DA83DD27EEB |
| docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/research/research-folder-settings-persistence.md | E73FE0CD8CAC6482E37D7488A305B5506DDC0E6D8D677BB9BBE993761AF0D3AA |

All three files were read in full.

## Acceptance-criteria section confirmation

spec.md contains a section headed exactly `## Acceptance Criteria`. That section holds exactly eight
checkbox criteria, identified AC1 through AC8, each unchecked at the time of this read:

1. AC1 — fresh-build path adopts the resource-defined disk configuration.
2. AC2 — the serializer logs an error rather than returning silently on an empty or null path.
3. AC3 — a saved value survives an Outlook restart (manual verification).
4. AC4 — an explicit Save is not lost inside the three-second deferred-write window.
5. AC5 — the junk-folder double-persistence path fails loudly and the reflection lookup is replaced by
   a typed seam.
6. AC6 — User Email shows the SMTP address, with a specific failure message, a fallback chain, and a
   retry on dialog open.
7. AC7 — Inbox and Root Folder are displayed without the leading backslash pair.
8. AC8 — a null current store selection renders the placeholder text instead of throwing.

The identical eight criteria appear in issue.md under the `## Proposed Fix / Validation Ideas`
heading. Work Mode is `full-bug`, recorded in the issue.md metadata block, so spec.md is the single
authoritative acceptance-criteria source and issue.md carries the mirror.

Output Summary: Three requirements sources read and digested. spec.md carries exactly eight criteria
AC1 through AC8 under a `## Acceptance Criteria` heading; issue.md carries the matching mirror.
