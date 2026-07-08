---
name: cobertura-runsettings-attributes-override
description: A custom <CodeCoverage> config in vstest runsettings silently overrides the collector's default <Attributes> excludes, so [ExcludeFromCodeCoverage] stops being honored unless you re-add the block
metadata:
  type: project
---

When you supply a custom `<CodeCoverage>` element (e.g. to set `<ModulePaths><Exclude>`) inside a
vstest Code Coverage DataCollector runsettings, that custom config REPLACES the collector's entire
default configuration — including the default `<Attributes><Exclude>` set. The consequence: the
`[ExcludeFromCodeCoverage]` attribute (and compiler-generated closures) STOP being honored, so
host-shell/VSTO/WinForms methods you deliberately marked exempt get counted as uncovered lines,
deflating per-file new-code coverage (e.g. a ThreadMonitor whose Run/Tick/GetStackTrace are all
`[ExcludeFromCodeCoverage]` showed only 74.5% because those lines were counted).

**Why:** the Microsoft.CodeCoverage collector merges nothing — a provided `<CodeCoverage>` block is
authoritative, so omitting `<Attributes>` means "exclude no attributes."

**How to apply:** whenever a runsettings has a custom `<CodeCoverage>` block and you rely on
`[ExcludeFromCodeCoverage]` for the CLAUDE.md COM/VSTO/WinForms coverage exemption, explicitly
re-add:
```xml
<Attributes><Exclude>
  <Attribute>^System\.Diagnostics\.CodeAnalysis\.ExcludeFromCodeCoverageAttribute$</Attribute>
  <Attribute>^System\.Diagnostics\.DebuggerHiddenAttribute$</Attribute>
  <Attribute>^System\.Diagnostics\.DebuggerNonUserCodeAttribute$</Attribute>
  <Attribute>^System\.Runtime\.CompilerServices\.CompilerGeneratedAttribute$</Attribute>
  <Attribute>^System\.CodeDom\.Compiler\.GeneratedCodeAttribute$</Attribute>
</Exclude></Attributes>
```
Also note: csharpier v1.x formats `packages.config` (XML), not just `.cs` — a single-line
`<package .../>` you add by hand will fail `csharpier check` until reformatted to its multi-line
style. Relates to [[project_qfc227_coverage_tooling]].
