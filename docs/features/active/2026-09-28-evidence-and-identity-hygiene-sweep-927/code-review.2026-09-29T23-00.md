# Code Review: 2026-09-28-evidence-and-identity-hygiene-sweep-927

- Issue: #927
- Branch: bug/evidence-and-identity-hygiene-sweep-927 at 32d93a367, reviewed against origin/main (merge base ddbab26a0)
- Review timestamp: 2026-09-29T23-00
- Scope: the 27 non-docs paths of the branch diff (3 new PowerShell production files, 3 new Pester files, 1 modified Pester file, 1 new and 2 modified workflow files, the workflow README, .gitignore, 13 C# test files, the deleted root run log) plus the docs sweep (this feature folder, 6 promoted records, 1 research note, and roughly 1,052 rewritten and 625 removed evidence files), assessed through the recorded fidelity gates rather than by reading each diff.

## Executive Summary

No blocking finding. The guard is a small, well-separated design: pure rule functions in one part file, the git seam and the byte adapter in another, and an orchestrating entry point that emits path-and-line findings only. The tests mock the wrapper rather than the executable, assemble every violating fixture at run time, and touch no file. The workflow wiring follows the existing callee shape exactly. The C# changes are literal-only fixture rebasing with every assertion untouched. Four Low or Informational findings are recorded below; none requires remediation before merge.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | scripts/hygiene/Test-RepositoryHygiene.ps1 | lines 42 to 49 | Every tracked record's bytes are read through the content adapter, including index-binary records, whose bytes are then discarded unless they carry a UTF-16 mark. The spec's performance paragraph describes the content read as extension-gated so that most tracked files are never opened; the implementation gates only the classifier's content, not the read. | Read only the leading bytes for an index-binary record (the mark is in the first two), or skip the read for index-binary records whose extension is neither xml nor a text form, if runtime ever approaches the callee timeout. No change required now. | Post-sweep runtime is 39 s against a 600 s timeout (guard-post-sweep-run.md), and reading the whole file is what makes the UTF-16 branch correct for index-binary files, so the trade is defensible. | Test-RepositoryHygiene.ps1 line 45; Test-RepositoryHygiene.Git.ps1 lines 124 and 141 to 145 |
| Low | tests/scripts/hygiene/Test-RepositoryHygiene.Git.Tests.ps1; tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1 | It "parses a NUL-separated eol listing into path records" (lines 12 to 35); It "returns a zero exit decision over clean content" (lines 108 to 135) | Two It blocks carry more than one behaviour: the first also pins the malformed-record throw; the second bundles empty, null, byte-order-marked, big-endian, empty-byte, empty-xml, non-raw-xml and prose-xml records. A failure inside either names a broader unit than the rule intends. | Split into separate It blocks in a follow-up when the PowerShell batch budget allows (the plan reserved the third test slot for the helper test, which is why the coverage-raising cases were folded in). | The powershell rule asks for one behaviour per It; the deviation is recorded in p1-t9-hygiene-coverage-interim.md with its reason, and every case still passes and is named by -Because clauses. | p1-t9-hygiene-coverage-interim.md, "Deviation recorded" paragraph |
| Informational | .gitignore | line 147 | The pattern *cobertura*.xml also matches the default projection stem the coverage route writes (coverage.cobertura.jacoco.xml). A permitted package-level projection copied into an evidence tree under that default name is silently ignored by git add unless forced or renamed. The eighteen retained projections use other stems, so nothing on this branch is affected. | Note the rename requirement in the evidence convention, or narrow the pattern in a follow-up (for example to names ending in .cobertura.xml) once the tracked corpus confirms no other shape needs the broad marker. | The spec mandates the marker-anywhere shape because at least one tracked name placed a timestamp after the marker; the content guard remains the real protection, as the ignore-file comment states. | .gitignore lines 143 to 147; identifier-and-raw-document-baseline.md PROJECTION list |
| Informational | .github/workflows/_hygiene.yml | line 24 | The step runs the guard as a script invocation from the runner's wrapper script; the non-zero exit reaches GitHub Actions through the exit code the invoked script sets and the runner's appended LASTEXITCODE check, the same mechanism the format-check callee relies on. No local toolchain stage executes a workflow step, so the propagation is proven only by a run on the runner. | None beyond the green run the modified-workflow rule already requires (PENDING-CI). | The ci-workflows rule requires an explicit reset only for a deliberately failing nested command; this step has none. | ci-workflows.md; P5-T1 probe LASTEXIT=0 |

## Detailed Notes by File

### scripts/hygiene/Test-RepositoryHygiene.Rules.ps1

- Get-UserProfilePathPattern returns the spec's pattern verbatim; it is valid in both the .NET and POSIX ERE dialects, contains no digit class, no word boundary and no lookaround, and its last character class cannot match a placeholder (which begins with a less-than sign). The pattern text is not itself a match of the pattern (the character before the colon is a closing bracket), which is why the spec, the plan and this review can quote it.
- Find-UserProfilePathMatch returns records with a single LineNumber property, so no caller can echo matched text; the early IsMatch over the whole text avoids the per-line split for the common clean file.
- Get-RawEvidenceDocumentKind applies the extension gate before reading content, strips the byte-order mark, skips declaration, comments, processing instructions and DOCTYPE with one anchored prolog regex, and reads the first element name terminated by whitespace, greater-than or slash. The switch is case-sensitive, matching the spec's root-element names. The jacoco-raw test is a whole-document search for class, sourcefile, method or line elements, which is the spec's rule.

### scripts/hygiene/Test-RepositoryHygiene.Git.ps1

- Invoke-GitExe matches the rule's wrapper-seam signature exactly (Invoke-GitExe -GitArgs [string[]], splat, 2>&1, throw on non-zero through Assert-GitExitCode). Assert-GitExitCode is separated so the throw is unit-testable without running git.
- Get-TrackedFileRecord parses the NUL-separated eol listing and fails fast on a record without the attribute-path tab; IsBinaryInIndex is a substring test on the attribute segment.
- Read-TrackedFileText decodes by byte-order mark (UTF-8 with mark, UTF-16 little- and big-endian) and otherwise as UTF-8 without throwing; an index-binary record without a UTF-16 mark returns null so it is not scanned. The unary-comma comment on the default delegate explains a non-obvious PowerShell pipeline behaviour, which is the right use of a comment.

### scripts/hygiene/Test-RepositoryHygiene.ps1

- Governance exclusion is an ordinal StartsWith on the listing path, not a pathspec, so it is unit-testable in memory; the prefix is the only path literal in the guard (P6-T12 PREFIX-LITERAL=1, ALLOW=0).
- Output contract: one raw-document line per classified file, one profile-path line per file with the first matching line number, one unreadable line per throwing read, and a trailing Findings line; the exit decision is one when the count is non-zero. The script-entry guard tests $MyInvocation.InvocationName against the dot operator, the pattern other scripts in this repository use.
- The only try/catch converts a reader throw into an unreadable finding and continues to the next record, which is the spec's "never skipped silently" requirement.

### tests/scripts/hygiene/*.Tests.ps1

- Every violating fixture is assembled from separate literals (drive letter, colon, separator, profile parent, segment), so the tracked test files contain no contiguous profile path and pass the guard they test (guard-post-sweep-run.md: Findings=0 with the test files in the enumeration).
- XML fixtures are here-strings inside .ps1 files, which the extension gate keeps out of rule A; one test pins exactly that.
- The orchestration file mocks Invoke-GitExe in BeforeEach with the parity signature and keys injected content by path; a byte-array value is returned with the unary comma so the adapter receives it whole.

### .github/workflows/_hygiene.yml, ci.yml, _pester.yml, README.md

- The callee copies the format-check shape: workflow_call plus workflow_dispatch, contents read, one job on ubuntu-latest, checkout with fetch-depth 1, one pwsh step, 10-minute timeout, no concurrency block. The orchestrator gains a three-line uses-job with no needs edge. The Pester callee's Run.Path and CodeCoverage.Path arrays gain the hygiene folders in alphabetical position. The README table row, the corrected Pester row, the seventh context and the ruleset follow-up paragraph are present.

### .gitignore

- Six lines added after the existing coverage and coveragexml patterns and before the coverage/* rule: three comment lines naming the content guard as the real protection, the trx pattern and the cobertura-marker pattern. git check-ignore -v confirms both hypothetical shapes match lines 146 and 147. No existing line removed.

### C# test files (13)

- Every hunk replaces a drive-rooted profile-parent fixture root with the fixtures root, keeping the user-segment token (testuser, test, Test, user) and the suffixes (OneDrive variants, AppData, Google Workspace Sync, Google Apps Sync). The three-letter real-account prefix in the three Store tests is replaced by testuser. csharpier reflowed three single-line dictionary initializers in AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs; the check passes. No attribute, using, test name or assertion changed; the NotContain assertions on testuser, OneDrive, Contoso, fsAncestor and the mailbox token are present and unchanged in the post-change files.

### tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1

- Two It blocks rebase the worktree root and the canonical root onto the bare drive-rooted repo segment already used elsewhere in the file, keeping the stamped worktree suffix; both pass (P3-T17). The file stays at 494 lines.

### docs sweep

- Assessed through the recorded gates rather than by reading 1,052 diffs: eol parity 1052/1052, byte-order-mark parity 1052/1052, multiset line comparison unmatched 0 with an in-memory negative control of 1, second helper run zero substitutions with equal numstat hashes, no XML-family file rewritten (redaction-fidelity.md). Dangling Markdown links into removed raw documents are tolerated by the spec and are not findings. The one research note's three citations now use the user-profile placeholder with the repository suffix retained (verified in the diff).

## Positive Observations

- The identifier-secrecy constraint is honoured end to end: the guard contains no identifier, the tests contain none, the evidence records counts only, and this review's own gate re-runs derived every identifier from the environment and printed counts only.
- The expect-fail observations (31 failing Its before the production files existed; 1839 guard findings before the sweep) and the matching pass observations (31 passing; 0 findings) give the bugfix workflow a real red-then-green record.
- The evidence tree obeys the committed-evidence format: Markdown projections and summaries only; ADDED-RAW=0 on the three-dot diff.
