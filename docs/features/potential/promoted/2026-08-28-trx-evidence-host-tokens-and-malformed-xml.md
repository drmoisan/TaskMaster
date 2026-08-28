# trx-evidence-host-tokens-and-malformed-xml (Issue #671)

- Date captured: 2026-08-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/trx-evidence-host-tokens-and-malformed-xml/ (Issue #671)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #671
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/671
- Last Updated: 2026-08-28
## Summary

Committed `.trx` test evidence across the repository carries host identity tokens, and the ad-hoc
redaction agents apply to remove them silently produces **XML that is not well-formed**, because the
placeholder is written with angle brackets directly into an XML attribute value. Separately, the coverage
step commits raw Cobertura documents at roughly 10.7 MB each. All three problems come from the same gap:
there is no defined convention for what test-run evidence should look like once it is committed.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (.NET Framework 4.8.1; vstest.console.exe TRX logger, AltCover/Cobertura output)
- Command/flags used: `vstest.console.exe ... /Logger:trx;LogFileName=<name>.trx`, and
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput <path>.cobertura.xml`
- Data source or fixture: committed evidence trees under `docs/features/active/*/evidence/`

## Steps to Reproduce

1. Run any repository test gate that writes a TRX under a feature's `evidence/` tree.
2. Inspect the emitted file: `runUser` carries `<machine>\<account>`, `computerName` carries the host
   name, and `storage`/`codeBase` carry the absolute checkout path including the account name. Note that
   vstest **lowercases** the `storage` path, so a case-sensitive search for the account name misses it.
3. Apply the redaction agents have been applying, substituting a placeholder written as
   `<worktree-root>` into those attributes.
4. Parse the result with any XML parser. It fails.

## Expected Behavior

Committed test evidence should carry no host, account, or absolute-path token, and should remain
well-formed XML so that a reviewer or a gate can parse it and read its counters. Committed coverage
evidence should be small enough that retaining it is not a repository-size decision.

## Actual Behavior

Three distinct failures, all observed on `epic/quickfiler-bug-family-integration`:

1. **Host tokens survive.** Feature 488's review (`policy-audit.2026-08-28T06-44.md`, finding PA-1) found
   the absolute path and the `runUser` domain token in all 19 of its committed TRX files, and noted the
   same tokens are already present in previously merged sibling evidence (features 501, 608, 439).
2. **Redaction breaks the XML.** A raw `<` is not legal in an XML attribute value. Substituting
   `<worktree-root>` into `storage`/`codeBase` made **all 19 of feature 488's committed TRX files
   unparseable**. This was verified against the committed blobs and went undetected through a full
   feature review, because the review re-derived its coverage figures from the Cobertura documents rather
   than from the TRX. Evidence that cannot be parsed cannot be audited mechanically.
3. **Raw Cobertura is large.** Feature 488 committed `coverage-baseline.cobertura.xml` and
   `coverage-final.cobertura.xml` at roughly 10.7 MB each, 21.4 MB for one feature. Deleting them later
   does not reclaim the space, because the blobs are already in history; the decision is only ever
   prospective.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: the two token forms, as observed before remediation —
  `storage="c:\users\<account>\repos\taskmaster\...\quickfiler.test.dll"` (note the lowercasing) and
  `runUser="<machine>\<account>"`. Remediation for 488 is recorded under finding PA-1 in
  `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/policy-audit.2026-08-28T06-44.md`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium rather than High: the leaked tokens are a developer account and machine name in a private
repository, not a credential. The parseability defect is the more consequential half, because it silently
degrades the audit trail every gate depends on, and it defeats exactly the mechanical verification that
would otherwise catch a fabricated result.

## Suspected Cause / Notes

There is no convention, so each agent invents one under time pressure, and the two obvious placeholder
spellings are both wrong: an angle-bracket token breaks XML, and a bare account name is easy to miss
because vstest lowercases one of the three attributes that carries it.

The durable fix is to stop relying on after-the-fact redaction:

- vstest's TRX logger already accepts an explicit `/ResultsDirectory:` and `LogFileName=`; controlling
  those removes the default `<account>_<HOST>_<timestamp>.trx` naming at the source.
- A tiny sanitizer used by every gate would be better than per-agent `sed`, and it must emit
  bracket-free placeholders and re-parse the file afterwards to prove it is still well-formed.
- For coverage, commit a compact package-level summary rather than the full per-line Cobertura document.
  The 488 review identified the exact subset a reviewer actually consumes: the root `coverage` element
  plus per-file hit tables for the measured files.

Files to inspect: `scripts/vscode/Invoke-MSTest.ps1`,
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`,
and the committed evidence trees under `docs/features/active/*/evidence/`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a test asserting the sanitizer leaves the document well-formed and preserves every `Counters` attribute
- [ ] Integration scenario to retest: run a gate end to end and assert the committed artifact parses and contains no host, account, or absolute-path token under either casing
- [ ] Manual verification notes: sweep the existing committed evidence for the pre-existing instances in features 501, 608, and 439, and decide whether to rewrite them or leave them and fix the convention going forward

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
