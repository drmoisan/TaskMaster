# Baseline Per-Class Coverage — `WpfDispatcherYield`

Timestamp: 2026-08-08T16-24

Task: [P0-T11]

Source report: `<FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml` (P0-T10, EXIT_CODE 0,
6293/6293 passing, repo line-rate 0.858162).

## Measured state: ABSENT

The class is **not present** in the pre-change Cobertura report. Recorded as observed, per the task
text's instruction to state genuine absence explicitly rather than reporting it as zero.

Aggregation query (the same method P2-T12 uses — every `<class>` element whose `filename` is the
target file, including compiler-generated nested types):

```
TARGET_FILENAME=UtilitiesCS\OutlookObjects\Folder\WpfDispatcherYield.cs
MATCHED_CLASS_ELEMENT_COUNT=0
RESULT=ABSENT
NAME_LIKE_MATCH_COUNT=0
```

Independent confirmation — raw substring count over the whole report:

Command: `grep -c "WpfDispatcherYield" coverage-baseline.cobertura.xml`
EXIT_CODE: 1 (no match)

```
0
```

Zero occurrences of the token anywhere in the report: no named class element, no
`<YieldAsync>d__*` state machine, no `<>c*` display class.

## The absence is not a query artifact

The `filename` attribute convention in this report uses Windows backslash separators and repo-root
relative paths, and the query string matches that convention. Peer classes from the same directory
are present, which proves the query shape is correct and the directory is instrumented:

```
filename="UtilitiesCS\OutlookObjects\Folder\BreadcrumbHtmlRenderer.cs"
filename="UtilitiesCS\OutlookObjects\Folder\DeadlineClock.cs"
filename="UtilitiesCS\OutlookObjects\Folder\FolderHierarchyBuilder.cs"
filename="UtilitiesCS\OutlookObjects\Folder\FolderNavigator.cs"
... (20+ peers in the same folder)
```

## Correction to the plan's stated expectation

The task text predicted that `[ExcludeFromCodeCoverage]` was likely **not** honored, reasoning that
`coverage.config` supplies a custom `<Configuration><CodeCoverage>` block with no `<Attributes>`
element, which would replace the dotnet-coverage default attribute-exclude set.

The measurement contradicts that prediction. `coverage.config` in this checkout contains only a
`<ModulePaths><Exclude>` block (7 third-party module patterns: Deedle, FSharp, Castle.Core,
FluentAssertions, Moq, Microsoft.Testing, MSTest) and no `<Attributes>` element:

```xml
<Configuration>
  <CodeCoverage>
    <ModulePaths>
      <Exclude>
        <ModulePath>.*Deedle.*</ModulePath>
        ... 6 more ...
      </Exclude>
    </ModulePaths>
  </CodeCoverage>
</Configuration>
```

In this dotnet-coverage version the omitted `<Attributes>` element does **not** clear the default
attribute-exclude set; the default (which includes `ExcludeFromCodeCoverageAttribute`) remains in
force. `[ExcludeFromCodeCoverage]` at `WpfDispatcherYield.cs:13` is therefore honored, and the class
is excluded from the report entirely.

The task text explicitly directs recording "whichever state is actually observed", so this is a
recorded measurement, not a plan deviation. No gate is weakened: the P0-T11 baseline comparand is
simply "absent", and P2-T12's >= 90% aggregated gate on the post-change report is unaffected.

## Consequences carried forward

1. **P2-T12 (changed-class gate)** is unaffected: it measures the post-change report, in which the
   class will be present because P1-T7 removes `[ExcludeFromCodeCoverage]`. The >= 90% aggregated
   line-coverage requirement stands unchanged.
2. **P2-T11 (repo-wide non-regression)** must account for a denominator change. Removing the
   attribute adds `WpfDispatcherYield.cs` lines to `lines-valid` for the first time. Direction of
   effect: the baseline repo rate is 0.858162, and the class is required by P2-T12 to land at
   >= 0.90, so admitting it should move the repo-wide rate slightly **up**, not down. The magnitude
   will be small (a ~30-line class against `lines-valid=111021`). P2-T11 records the measured
   figures; if the movement is material or negative, it is escalated rather than absorbed.
3. **Coverage Exclusion Policy compliance**: because the attribute is genuinely honored, the
   pre-change file was in fact excluded from measurement, which is precisely what
   `.claude/rules/general-unit-test.md` "Coverage Exclusion Policy" prohibits for a production
   file. P1-T7's removal of the attribute is therefore a policy correction with a measurable
   effect, not a cosmetic edit.

Output Summary: The `WpfDispatcherYield` class is genuinely ABSENT from the pre-change Cobertura
report (0 matched class elements, 0 substring occurrences), while 20+ peer classes from the same
directory are present, so the absence is real and not a query artifact. `[ExcludeFromCodeCoverage]`
IS honored in this configuration, contradicting the plan's stated expectation; recorded as observed
per the task's own instruction. Baseline comparand for the changed class is therefore "absent /
unmeasured", and removing the attribute in P1-T7 adds the class to the repo-wide denominator for
the first time — a movement P2-T11 must measure.
