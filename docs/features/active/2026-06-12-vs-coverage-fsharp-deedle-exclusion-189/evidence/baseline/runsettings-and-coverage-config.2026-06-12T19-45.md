# Phase 0 — Pre-edit runsettings and coverage.config (P0-T3)

Timestamp: 2026-06-12T19-45

Command: `Read TaskMaster.runsettings` and `Read coverage.config` (verbatim capture)

EXIT_CODE: 0

Output Summary:
- Pre-edit `TaskMaster.runsettings` contains ONLY the `<MSTest><Parallelize>` block and NO `<DataCollectionRunSettings>` / `<DataCollectors>` block.
- `coverage.config` lists the seven `<ModulePath>` exclusions to be mirrored.

## TaskMaster.runsettings (pre-edit, verbatim)

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <MSTest>
    <Parallelize>
      <Workers>0</Workers>
      <Scope>ClassLevel</Scope>
    </Parallelize>
  </MSTest>
</RunSettings>
```

## coverage.config exclusion list (verbatim, seven ModulePath entries)

```xml
<ModulePath>.*Deedle.*</ModulePath>
<ModulePath>.*FSharp.*</ModulePath>
<ModulePath>.*Castle\.Core.*</ModulePath>
<ModulePath>.*FluentAssertions.*</ModulePath>
<ModulePath>.*Moq.*</ModulePath>
<ModulePath>.*Microsoft\.Testing.*</ModulePath>
<ModulePath>.*MSTest.*</ModulePath>
```

Seven exclusions confirmed: `.*Deedle.*`, `.*FSharp.*`, `.*Castle\.Core.*`, `.*FluentAssertions.*`, `.*Moq.*`, `.*Microsoft\.Testing.*`, `.*MSTest.*`.
