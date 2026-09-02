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

**Recurrence on issue #662, with a new mechanism: the defect was CODIFIED IN THE PLAN.** The
artifact-hygiene rule in the approved plan *mandated* the four angle-bracket placeholders
`<repo-root>`, `<user-profile>`, `<user>`, `<host>`, so the executor produced six unparseable TRX
files by following the plan correctly. This is worse than an executor slip: the plan's own gate
asserts only `ResidualMatchCount=0`, which measures whether identifiers were *removed* and is fully
satisfied by a rewrite that destroys the document. A sweep rule that names bracketed placeholders
will corrupt every XML artifact it touches, on every run, and its gate will report success each time.
Treat `ResidualMatchCount=0` as necessary and NOT sufficient; the gate needs a companion parse check.

**Repairing an already-committed scrub: escape, do not re-scrub.** Switching to bracket-free tokens
is right when authoring the rule, but once the redaction is committed the cheap fix is to XML-escape
the placeholders in the `.trx` files only — `<user>` becomes `&lt;user&gt;`. The parsed attribute
value is then exactly the placeholder, so redaction is byte-for-byte unchanged and the residual sweep
still returns zero, while the document parses. A blind textual replace is safe because no TRX element
is named `repo-root`, `user-profile`, `user` or `host`, so every bracketed occurrence is a
placeholder rather than markup. Expect a large diff and do not mistake it for a line-ending flip:
`vstest` writes `storage="<repo-root>\..."` on EVERY test element, so a 33k-line TRX legitimately
changes ~14k lines. Confirm by comparing line COUNTS on both sides before assuming CRLF damage.
Cobertura `.xml` is unaffected — its rewritten `filename` attributes are repo-relative paths with no
angle brackets.

**The parse check is also free corroboration.** Parsing the repaired TRX let me read the
`<Counters/>` elements directly and confirm the added test raised the passed count by exactly one,
which verified three acceptance criteria from primary data instead of from a subagent's prose.
