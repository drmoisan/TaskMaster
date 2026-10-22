# P4-T2 — AC3 Descendant-Axis Gate (Controlled Comparison)

Timestamp: 2026-09-09T11-14
Task: [P4-T2]
ExpectedExitCode: 1

The `ExpectedExitCode: 1` declaration applies to command 2, the case-sensitivity control, whose
expected outcome is that `git grep` finds nothing and therefore exits 1. Commands 1 and 3 are
expected to exit 0 and carry their own `EXIT_CODE:` lines below. The evidence schema permits one
expectation per artifact file, so the file-level expectation is set to the value the zero-match gate
requires.

All three commands ran after P4-T1 staged and committed both folders, so `git grep` can see the two
files this feature creates.

## Command 1 — the positive control and the negative assertion in one run

Command: `git grep -c -F -e './/line' -- scripts/vscode tests/scripts/vscode`
EXIT_CODE: 0

```
tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1:1
```

Exactly one line. Its path is
`tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1` with count 1, which is the
single occurrence inside the AC6 differential helper. **No line of the output names any path under
`scripts/vscode/`.**

## Command 2 — the case-sensitivity control

Command: `git grep -c -F -e './/LINE' -- scripts/vscode tests/scripts/vscode`
EXIT_CODE: 1

```
(no output)
```

## Command 3 — the case-insensitive variant of command 2

Command: `git grep -c -i -F -e './/LINE' -- scripts/vscode tests/scripts/vscode`
EXIT_CODE: 0

```
tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1:1
```

The same single line command 1 produced.

## What the three commands establish together

1. **The search finds the literal when it is present.** Command 1 returns the deliberate occurrence
   in the differential test helper. A broken search — wrong quoting, wrong path, or a regex
   interpretation of the leading dot — would have returned nothing here, so the zero result under
   `scripts/vscode/` is a real observation rather than an artifact of a failed query. `git grep -F`
   is used rather than `Select-String` precisely because the leading `.` of the literal is a regex
   metacharacter and `-F` takes it as a fixed string.
2. **No occurrence survives under `scripts/vscode/`.** The delivered production file re-derives no
   part of the selection; it delegates to `Get-CoberturaPackageLineSummary`. This matches the
   verified pre-change baseline of zero recorded in
   `evidence/baseline/p0-t10-descendant-axis-baseline.md`, so the change neither introduced nor left
   an occurrence in the production tree.
3. **The search is case-sensitive.** Command 2 produces nothing while command 3, differing only by
   `-i`, produces the same single line command 1 produced. That pair is the demonstration.

Output Summary: The case-sensitive search returns exactly one line, in
`tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1` with count 1, and no line
naming any path under `scripts/vscode/`. The upper-case variant returns nothing and exits 1; its
case-insensitive twin returns the same single line. AC3 is discharged: the descendant-axis selection
is absent from the delivered scripts, and its only occurrence under the tests tree is the
differential helper AC6 requires.
