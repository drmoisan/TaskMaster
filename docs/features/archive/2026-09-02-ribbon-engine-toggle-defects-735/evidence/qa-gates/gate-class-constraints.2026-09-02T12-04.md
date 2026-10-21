# Finding 2 — Host-Neutrality Constraints on the Gate Class (P2-T4)

Timestamp: 2026-09-03T02-03
Task: [P2-T4]
Command: line-oriented pattern counts over `TaskMaster/Ribbon/SpamManagerResetGate.cs`, using `Get-Content -LiteralPath` and four anchored regular expressions.
EXIT_CODE: 0

File measured: `TaskMaster/Ribbon/SpamManagerResetGate.cs`, 141 lines.

## The four required zero counts

| Check | Pattern | Count |
|---|---|---|
| No coverage-exemption attribute | `^\s*\[ExcludeFromCodeCoverage\]` | **0** |
| No Office interop using | `^using Microsoft\.Office` | **0** |
| No WinForms using | `^using System\.Windows\.Forms` | **0** |
| No logging dependency | `log4net` (anywhere in the file) | **0** |

## Why the attribute check is anchored to the attribute form

The file deliberately NAMES the coverage attribute in prose, in the XML-doc paragraph that records
its absence as intentional. An unanchored search for the attribute name would match that sentence
and would report a violation on a file that has no attribute at all. The check is therefore anchored
to `^\s*\[ExcludeFromCodeCoverage\]`, which matches only the attribute applied on its own line.

The one line in the file that names the attribute, quoted so the distinction is auditable:

```
    /// This type is deliberately NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is host-neutral
```

That line begins with whitespace followed by `///`, so it cannot match the anchored attribute
pattern, and it is a documentation comment rather than an attribute application. This mirrors the
equivalent paragraph on `TaskMaster/Ribbon/EngineReadinessGate.cs`.

## Using directives actually present

```
using System;
using System.Globalization;
using System.Threading.Tasks;
using UtilitiesCS;
```

Exactly the four the plan specifies and nothing else. The classifier manager type
(`ManagerAsyncLazy`) and both dependency interfaces (`IAppAutoFileObjects`, `IAppItemEngines`) all
live in the `UtilitiesCS` namespace, so no additional using and no new project reference is
required.

## Exactly one type is declared

```
    internal sealed class SpamManagerResetGate
```

No nested type, no companion type, no partial. The declaration is `internal sealed` in namespace
`TaskMaster`, matching every sibling in the ribbon directory.

Output Summary: All four constraint checks return zero — no coverage-exemption attribute in
attribute form, no Office interop using, no WinForms using and no log4net reference. The file
declares exactly one type, `internal sealed class SpamManagerResetGate`, and carries exactly the
four specified using directives. The XML-doc sentence recording the deliberate absence of the
coverage attribute is quoted above and is the reason the attribute check is anchored.
