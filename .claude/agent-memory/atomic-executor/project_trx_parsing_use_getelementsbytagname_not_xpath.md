---
name: trx-parsing-use-getelementsbytagname-not-xpath
description: Parse TRX counters/results with XmlDocument.GetElementsByTagName, not a local-name() XPath — the XPath needs embedded double quotes that cannot survive a bash -> pwsh -Command string
metadata:
  type: project
---

When reading vstest TRX evidence from a `pwsh -NoProfile -Command '...'` invocation issued through
the Bash tool, use `$doc.GetElementsByTagName("Counters")[0]` and
`$doc.GetElementsByTagName("UnitTestResult")` rather than
`$doc.SelectSingleNode('//*[local-name()="ResultSummary"]/*[local-name()="Counters"]')`.

**Why:** two problems compound.

1. TRX has a default namespace (`http://microsoft.com/schemas/VisualStudio/TeamTest/2010`), so a
   plain `//Counters` XPath matches nothing and a `local-name()` predicate is the usual workaround.
2. That predicate needs literal double quotes *inside* the PowerShell string. The outer Bash
   argument is already single-quoted (to stop Bash expanding `$doc`, `$env:`, `${env:...}`), so the
   inner string must be double-quoted, and `\"` is NOT a PowerShell escape — PowerShell uses a
   backtick. The command dies with `ParserError: Missing ')' in method call.` before any test data
   is read.

`GetElementsByTagName` matches on the unqualified tag name and sidesteps both problems in one step.

**How to apply:** in any `[expect-fail]` or baseline task that must read TRX
`ResultSummary/Counters` or enumerate `UnitTestResult` outcomes. Read the failure text with
`$r.GetElementsByTagName("Message")[0].InnerText` and truncate it before printing, because a Moq
`MockException` message runs to dozens of lines. Related: [[project_pwsh_command_quoting_from_bash]],
[[project_pester_strictmode_xml_attribute_property_access]],
[[project_vstest_testcasefilter_or_operator_and_env_setup]].
