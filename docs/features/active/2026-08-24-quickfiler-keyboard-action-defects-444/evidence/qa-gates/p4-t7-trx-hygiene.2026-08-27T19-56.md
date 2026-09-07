# [P4-T7] Test-result artifact hygiene

Timestamp: 2026-08-27T19-56
Command: in-place normalization of `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t6/p4-t6-final.trx`, then verification with `Select-String -LiteralPath <trx> -SimpleMatch -Pattern ':\Users\' -AllMatches`, the same for `$env:COMPUTERNAME` and `$env:USERNAME`, and a `computerName="host"` versus `computerName="` occurrence comparison
EXIT_CODE: 0
Output Summary: all four host-identifying value classes normalized. Post-normalization
`Select-String -SimpleMatch` counts: `:\Users\` = 0, `$env:COMPUTERNAME` = 0, `$env:USERNAME` = 0.
`computerName="host"` occurs 6713 times, equal to the total `computerName="` count of 6713, which is
greater than 0.

Host-identifying values are referred to below as `<user>` (the account name) and `<host>` (the
machine name) and are never spelled out, so this artifact does not reintroduce what it removes.

## The four counts, before and after

| # | Value class | Before | After | Normalization applied |
| --- | --- | --- | --- | --- |
| 1 | Absolute filesystem path prefix (`:\Users\`, matched case-insensitively) | 13444 | **0** | every absolute worktree-root path prefix replaced with the literal `<repo-root>` |
| 2 | `computerName` attribute on `<UnitTestResult>` | 6713 occurrences, 0 of them `computerName="host"` | 6713 occurrences, **6713** of them `computerName="host"` | every attribute value set to `host` |
| 3 | `runUser` attribute on `<TestRun>` | 1, value `<host>\<user>` | 1, value `host\user` | attribute value set to `host\user` |
| 4 | `name` attribute on `<TestRun>` | 1, value `<user>@<host> 2026-08-27 15:52:04` | 1, value `p4-t6-final` | rewritten to contain neither the account name nor the machine name |

Two supporting token counts, recorded because the four attribute classes above do not by themselves
prove the tokens are gone from free text such as `<StdOut>` and `runDeploymentRoot`:

| Token | Before (case-insensitive) | After |
| --- | --- | --- |
| machine name | 6719 | **0** |
| account name | 13448 | **0** |

`runDeploymentRoot`, which carried both tokens, was set to `host_run`.

## Why the path scrub needed two passes

A first, case-sensitive replacement of the worktree root removed 6741 occurrences but left 6703
behind. `vstest.console.exe` writes the `storage=` attribute of every `<UnitTest>` element in
**all lower case**, so a case-sensitive substitution of the worktree root misses it entirely while
the case-insensitive `Select-String -SimpleMatch` assertion in this task's acceptance still counts
it. The residual 6703 were removed by a case-insensitive replacement of the same root. A sample
`storage=` value after normalization:

```
storage="<repo-root>\utilitiescs.test\bin\debug\utilitiescs.test.dll"
```

The count difference between the case-sensitive figure (6741) and the case-insensitive figure
(13444) is exactly this lower-case `storage=` population.

The file was rewritten as UTF-8 with a byte-order mark, matching the encoding
`vstest.console.exe` produced, so the TRX remains well-formed XML.

## Acceptance

For the TRX at
`docs\features\active\quickfiler-keyboard-action-defects-444\evidence\qa-gates\p4-t6\p4-t6-final.trx`:

- `Select-String -SimpleMatch` count for `:\Users\` is `0` — met.
- count for `$env:COMPUTERNAME` is `0` — met.
- count for `$env:USERNAME` is `0` — met.
- count of `computerName="host"` occurrences equals the total count of `computerName="` occurrences
  (6713 = 6713) and is greater than `0` — met.

The binary `.coverage` attachments written into the same results directory retain
host-derived file names, but `.gitignore:140` (`*.coverage`) excludes them from the repository, so
no host-identifying value enters a tracked file.

## Addendum: the placeholder is XML-escaped (2026-08-27T20-03)

A first application of the path substitution inserted the placeholder as the raw five-plus
characters `<` `repo-root` `>` directly into XML text nodes and attribute values. XML forbids a
raw `<` in both positions, so the document stopped being well-formed: a tag-stack scan reported
`</StdOut> at line 638 closes <repo-root> opened at line 638`, the parser having read the
placeholder as an element start tag.

All 13444 occurrences were re-encoded as `&lt;repo-root&gt;`, which is how the **literal text**
`<repo-root>` is spelled in XML. The stored value is unchanged — an XML reader decodes it back to
the literal `<repo-root>` — so the required substitution is satisfied while the document parses.
Verified after the re-encoding:

| Check | Result |
| --- | --- |
| `xml.etree.ElementTree.parse` | parses without error |
| `<UnitTestResult>` elements | 6713 |
| distinct `outcome` values | `{Passed: 6713}` |
| distinct `computerName` values | `{host}` |
| `<TestRun>` `runUser` (decoded) | `host\user` |
| `<TestRun>` `name` (decoded) | `p4-t6-final` |
| sample `<UnitTest>` `storage` (decoded) | `<repo-root>\utilitiescs.test\bin\debug\utilitiescs.test.dll` |

The four acceptance counts were re-run after the re-encoding and are unchanged: `:\Users` = 0,
`$env:COMPUTERNAME` = 0, `$env:USERNAME` = 0, and `computerName="host"` = 6713 = the total
`computerName="` count. Escaping `<` and `>` does not affect any of those four patterns.
