# Phase 6 — Toolchain step 1 verification: CSharpier check

Timestamp: 2026-09-09T14-38

Task: [P6-T2]

Command: `dotnet tool run csharpier check .`

`check` is read-only and exits non-zero on drift, so exit 0 together with the single `Checked` line
is the clean-tree observation.

EXIT_CODE: 0

Verbatim printed line:

```
Checked 1622 files in 4215ms.
```

FINAL-CSHARPIER-CHECKED-FILES: 1622
CHECKED-FILES-DELTA: 0

The delta is `FINAL-CSHARPIER-CHECKED-FILES` 1622 minus `BASELINE-CSHARPIER-CHECKED-FILES` 1622,
read directly from `evidence/baseline/p0-t8-csharpier-check.md`. No bound is gated on this delta in
either direction, and none is added: the checked-file count is not confined to the files this plan
touches, so a bound on it would be a gate on unrelated activity. It is recorded as an observation.

DELTA-COMPOSITION: The delta is 0, so no candidate for a non-zero delta materialised. Reasoning
from the two inputs read directly:

- This plan creates no new `.cs` file, so it contributes nothing to the counted set through source.
  Every one of its nine source edits is to a file that already existed at the [P0-T8] baseline.
- `coverage/823-effective-coverage.config`, written by [P0-T12] after the [P0-T8] baseline, was
  expected not to change the count and did not. CLAUDE.md records that CSharpier 1.2.6 accepts and
  processes `*.xml` and `packages.config` in addition to `*.cs`; that file is neither `*.xml` nor
  named `packages.config`, so it is outside the processed set on the CSharpier side and no
  `.csharpierignore` entry is needed to keep it out.
- The known candidates for a non-zero delta were any `*.xml` file that the [P0-T12] run writes
  under `coverage/` or `TestResults/` whose name does not end `.cobertura.xml`, `.coveragexml`,
  `.coverage` or `.trx`, because the repository-root `.csharpierignore` excludes those four by
  extension but excludes neither directory. Its eight entries are `**/evidence/**`,
  `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`, `*.csproj`, `*.props` and `*.targets`.
  No such file materialised: the observed count is unchanged, so no candidate entered the set.

Output Summary: Read-only formatter verification passed at exit 0 on the post-change tree. 1622
files checked, zero drifting paths, and a checked-file delta of 0 against the [P0-T8] baseline.
