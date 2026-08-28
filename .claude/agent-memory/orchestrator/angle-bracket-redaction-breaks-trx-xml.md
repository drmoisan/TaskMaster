---
name: angle-bracket-redaction-breaks-trx-xml
description: Redacting host paths into XML attribute values with a <placeholder> token produces non-well-formed XML; it survived a full feature review undetected, and vstest lowercases the storage path so a case-sensitive account grep misses it
metadata:
  type: project
---

Scrubbing host identity out of committed `.trx` evidence has two traps that both bit on feature 488,
and the second one defeated a full feature review.

**Trap 1 — the placeholder breaks the XML.** A raw `<` is not legal in an XML attribute value.
Substituting `<worktree-root>` into `storage=` / `codeBase=` made all 19 committed TRX files
unparseable. Use a bracket-free token: `REDACTED-WORKTREE-ROOT`, `REDACTED-DOMAIN-USER`. Always
re-parse afterwards to prove well-formedness, and diff the `Counters` attributes to prove you changed
only identity fields.

**Trap 2 — vstest LOWERCASES the storage path.** `codeBase=` keeps the original casing but `storage=`
is lowercased, so `grep 'DanMoisan'` returns clean while `grep -i danmoisan` finds 19 files. Always
sweep case-insensitively, and sweep for the machine/domain token separately — `runUser` carries
`<machine>\<account>` even after `computerName` has been redacted.

**Why:** feature 488's executor hit both. Its scrub redacted `computerName` and `codeBase` but missed
the lowercased `storage` and the `runUser` domain half, and its own placeholder silently destroyed
well-formedness. The feature review flagged the residual tokens as Non-blocking PA-1 but did NOT catch
the parseability defect, because it re-derived coverage from the Cobertura documents rather than the
TRX. Unparseable evidence defeats exactly the mechanical verification that would otherwise catch a
fabricated test result.

**How to apply:** after any evidence scrub, run three checks — case-insensitive account sweep,
machine/domain token sweep, and an XML parse of every touched file with a counter comparison. Tracked
repo-wide as issue #671. See [[_shared_no_absolute_host_paths]].
