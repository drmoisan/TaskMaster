# [P1-T4] — PA-7 Instance 3 Redacted (policy audit, line 483)

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P1-T4]
File: `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/policy-audit.2026-08-29T23-06.md`, line 483
EXIT_CODE: 0

Redaction note: this artifact reproduces no raw absolute-path text, no account token, and no
mail local-part. The redacted content is described generically.

## The edit

Line 483 is the `- **Verification:**` line of the PA-7 finding, a two-line bullet continuing
onto line 484. Before this edit it quoted a `grep -nE` command whose alternation named, in
order, a Windows absolute path built with doubled-backslash separators and containing the
account name, the bare account name, and the account's mail local-part.

Line 483 was replaced with exactly the single-line inline-code literal the plan mandates. That
literal is backtick-quoted in the plan rather than fenced, so there is no nested block whose
column could be misplaced; the plan's two-space list-continuation indentation is prose
formatting and is not part of the literal.

Line 483 on disk after the edit, verbatim:

```
- **Verification:** a case-insensitive search for the account name and the account's mail local-part returns that
```

Line 484 was left in place unchanged. Joined, the bullet reads as a single grammatical
sentence:

> **Verification:** a case-insensitive search for the account name and the account's mail
> local-part returns that single line; the same scan over the rest of the feature folder
> returns nothing.

Both measured figures the bullet already recorded are retained: the single matched line in the
research artifact, and the empty result over the rest of the feature folder. This edit alters
wording only. The finding, its verdict, its `Location:` citation, and both measured figures are
preserved, satisfying the inputs' constraint not to alter any finding, verdict, or measured
figure.

## Scope containment

One line was replaced by one line. The file is 634 lines before and after. No other line was
changed.

Encoding integrity was verified at byte level after the edit, because a console rendering of
the surrounding lines through the shell pipe displayed the document's em-dashes as hyphens. An
`od -c` dump of line 486 shows the byte sequence `342 200 224`, the UTF-8 encoding of U+2014
EM DASH, still present, and a whole-file scan counts 36 non-ASCII characters, all em-dashes.
The apparent substitution was a console encoding artifact of the output pipe, not a change to
the file.

## Acceptance clauses

All three clauses hold. Measured before this edit, as the plan states: clause 1's count is `1`;
clause 2's count is `0`; clause 3's count is `3`.

### Clause 1 — account token eliminated from this file

Command: `$t=[regex]::Escape((Split-Path -Leaf $env:USERPROFILE)); @(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\policy-audit.2026-08-29T23-06.md -Pattern "(?i)$t").Count`
EXIT_CODE: 0
Before: `1`   After: `0`   Required: `0`   Result: PASS

### Clause 2 — the line still exists, carrying the mandated wording

Command: `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\policy-audit.2026-08-29T23-06.md -SimpleMatch -Pattern '- **Verification:** a case-insensitive search').Count`
EXIT_CODE: 0
Before: `0`   After: `1`   Required: `1`   Result: PASS

The bare bullet prefix `- **Verification:**` is deliberately not used as this clause's token:
it is the bullet prefix of three findings in this document, so a count over it returns `3` both
before and after the edit and the clause could never fail. The token used here includes the
mandated wording, so it is `0` before and `1` after.

### Clause 3 — no `Verification:` bullet deleted anywhere in the document

Command: `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\policy-audit.2026-08-29T23-06.md -SimpleMatch -Pattern '- **Verification:**').Count`
EXIT_CODE: 0
Before: `3`   After: `3`   Required: `3`   Result: PASS

The three bullets are at lines 385, 395, and 483 after the edit — the same three line numbers
they occupied before it, confirming the one-line-for-one-line replacement preserved document
numbering.

## Supplementary observations

- Shape pattern over this file: `0`, unchanged from the post-`[P1-T3]` state. Line 483 was
  never a shape-pattern match, because its path was written with doubled backslashes.
- A literal search for the account's mail local-part over this file returns `0` matches after
  the edit. This confirms the third alternation term the removed `grep` pattern named is gone,
  which neither the shape pattern nor the account-token pattern would report on its own.

## Output Summary

PA-7 instance 3 redacted to the mandated wording. All three acceptance clauses pass: the
account-token count for this file fell from `1` to `0`; the corrected wording is present
exactly once, up from `0`; and the `Verification:` bullet count held at `3` across lines 385,
395, and 483, confirming no bullet was deleted. A literal check for the account's mail
local-part also returns `0`. Wording changed only; no finding, verdict, or measured figure was
altered. File encoding verified intact at byte level.
