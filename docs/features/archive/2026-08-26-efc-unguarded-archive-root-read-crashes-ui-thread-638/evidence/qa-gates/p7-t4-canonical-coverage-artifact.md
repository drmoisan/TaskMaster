# [P7-T4] Canonical review coverage artifact decision (Issue 638)

Timestamp: 2026-08-29T12-44

Command:

```
# read the decision inputs from [P7-T1]
# then, because the decision is WRITTEN, emit the JaCoCo-shaped file:
$x = [xml](Get-Content 'coverage/coverage.cobertura.xml')
$covered = [int]$x.coverage.'lines-covered'; $valid = [int]$x.coverage.'lines-valid'
$missed = $valid - $covered
# write artifacts/csharp/coverage.xml with <report><counter type="LINE" missed="..." covered="..."/></report>
([xml](Get-Content 'artifacts/csharp/coverage.xml')).report.counter
```

EXIT_CODE: 0

MEASURED_REPO_LINE_COVERAGE_PERCENT: 85.33

COVERAGE_XML_MODE: koverage-processed

Decision: WRITTEN

Output Summary:

## Decision derivation

`COVERAGE_XML_MODE:` reads `koverage-processed`, so the `NOT WRITTEN — measured figure is a
raw denominator that includes test assemblies` branch does not apply. The measured value
85.33 is greater than or equal to 85.0, so the `NOT WRITTEN — measured figure below 85.0`
branch does not apply either. The file is therefore written.

This three-way decision exists because
`.claude/hooks/validate-feature-review-coverage.ps1:313` applies a hard-coded 85.0 floor
whenever `artifacts/csharp/coverage.xml` exists, while this repository's policy floor is
80 percent on the testable denominator. Emitting the file while the measured figure sat
between those two values would force a false failure downstream. Here the measured figure
clears the hook's own floor, so emitting it is safe.

## Derivation of the emitted counter values

Source attributes, from the root `coverage` element of `coverage/coverage.cobertura.xml`
(post-processed, so these are the first-party values recomputed by
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:442-445`):

```
lines-covered = 54802
lines-valid   = 64221
```

Emitted counter values:

```
covered = lines-covered            = 54802
missed  = lines-valid - covered    = 64221 - 54802 = 9419
```

## The emitted file

`artifacts/csharp/coverage.xml`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<report name="TaskMaster">
  <counter type="LINE" missed="9419" covered="54802"/>
</report>
```

It is JaCoCo-shaped, not Cobertura-shaped: the hook reads it with `Get-JacocoRepoCoverage`
at `.claude/hooks/validate-feature-review-coverage.ps1:253`, which selects
`//counter[@type="LINE"]` at `:229` and sums the `missed` and `covered` attributes. A
Cobertura-shaped file would yield no counters and the hook would read nothing.

## Verification

```
([xml](Get-Content artifacts/csharp/coverage.xml)).report.counter  -> returns a node
  type=LINE  missed=9419  covered=54802
```

The `Decision:` line reads `WRITTEN` and `artifacts/csharp/coverage.xml` is present on disk,
so the two agree. Recomputing the hook's own figure from the emitted counter gives
`54802 / (9419 + 54802) = 85.33` percent, matching `MEASURED_REPO_LINE_COVERAGE_PERCENT:`
and clearing the hook's 85.0 floor.

`git check-ignore -v` reports `.gitignore:57:artifacts/`, so the file is gitignored and
stays outside this change's diff, as the plan's Change Footprint records.
