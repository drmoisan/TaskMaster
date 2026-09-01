# TRX XML well-formedness repair (orchestrator verification finding)

Timestamp: 2026-09-01T17-09
Command: python3 - (escape angle-bracket placeholder tokens in the six committed `.trx` files), then `xml.etree.ElementTree.parse` over all eight committed XML-family evidence files
EXIT_CODE: 0

## Finding

The artifact-hygiene rule in the plan's Fail-Closed Evidence Rules substitutes four placeholder
tokens written with angle brackets: `<repo-root>`, `<user-profile>`, `<user>` and `<host>`. That
substitution is correct and necessary for redaction, and it succeeded — a case-insensitive
fixed-string sweep of the feature folder for the account name and the machine name returns zero
matching files.

However, `vstest.console.exe` writes those redacted values into XML *attribute values* in the TRX
header, for example the `name` and `runUser` attributes of the `<TestRun>` element on line 2. A raw
`<` or `>` character is not permitted inside an XML attribute value, so the substitution left all
six committed TRX files not well-formed. Every one failed to parse at line 2, column 57.

This was found by the orchestrator during post-execution verification, not by the executor's own
hygiene gate. The gate as written in the plan checks only `ResidualMatchCount=`, which measures
whether the identifiers were removed. It does not check whether the file it rewrote is still
parseable, so a sweep that corrupts every XML artifact it touches still reports success.

## Repair

Each placeholder token occurrence inside the six `.trx` files was replaced with its XML-escaped
form, so `<user>` became `&lt;user&gt;` and likewise for the other three tokens. The parsed
attribute value is therefore still exactly the placeholder text, the redaction is unchanged, and
the document is well-formed. Only `.trx` files were touched. The two Cobertura `.xml` evidence
files were already well-formed and were not modified: their rewritten `filename` attributes are
repository-relative paths carrying no angle brackets.

A blind textual replacement is safe here because no XML element in a TRX document is named
`repo-root`, `user-profile`, `user` or `host`, so every occurrence of those bracketed tokens is a
placeholder introduced by the sweep rather than real markup.

## Output Summary

Before repair: 6 of 6 `.trx` files not well-formed; 2 of 2 `.xml` files well-formed.
After repair: 8 of 8 well-formed, 0 broken.
Redaction after repair: account-name matching files 0, machine-name matching files 0.

Counters read from the repaired documents, which corroborate AC6, AC7 and AC8 directly from the
primary artifacts rather than from any prose summary:

| Artifact | total | passed | failed |
|---|---|---|---|
| `evidence/baseline/p0-t11/quickfiler-baseline.trx` | 1286 | 1286 | 0 |
| `evidence/qa-gates/p2-t7/quickfiler-postchange.trx` | 1287 | 1287 | 0 |
| `evidence/baseline/p0-t12/utilitiescs-baseline.trx` | 4783 | 4783 | 0 |
| `evidence/qa-gates/p2-t8/utilitiescs-postchange.trx` | 4783 | 4783 | 0 |
| `evidence/regression-testing/p2-t5/ac6-scoped.trx` | 1 | 1 | 0 |
| `evidence/regression-testing/p2-t6/ac7-scoped.trx` | 1 | 1 | 0 |

The QuickFiler passed count rises by exactly one between baseline and post-change, which is the
single test method AC6 adds, and no assembly regresses. AC8's comparison therefore holds when read
from the counters rather than from the summary prose.

## Plan defect recorded

The artifact-hygiene rule should either use placeholder tokens that carry no angle brackets, or
escape them when the target file is XML, and its verification should additionally assert that every
rewritten XML-family file still parses. As written the rule is guaranteed to corrupt any TRX it
redacts. This is a defect in the plan's shared hygiene rule, not an executor error: the executor
applied the rule exactly as written.
