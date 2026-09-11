---
name: analyzer-control-site-can-be-uncompiled-not-just-commented
description: An analyzer "control" call site can emit no diagnostic because no <Compile Include> item names its file — TaskMaster's legacy non-SDK csproj files have explicit items and no wildcard, so a live source line can be invisible to the compiler
metadata:
  type: project
---

A positive-observation gate for an analyzer rule needs a control: a site with a known live usage that
must fire in the same run, or the channel is unproven. The obvious way a control dies is that the
"live" line turns out to be inside a `//` comment. There is a second way, and it is harder to see.

**TaskMaster's 18 project files are legacy non-SDK** (`<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">`,
no `Sdk=` attribute) with **explicit `<Compile Include>` items and no wildcard include**. A `.cs` file
can therefore sit in the tree, be tracked by git, contain live uncommented code, and be named by no
project at all. The compiler never sees it, so it produces no diagnostic on any channel.

**Verified 2026-09-09:** `QuickFiler/Legacy/QuickFileController.cs` carries three live `DateTime.Now`
reads at lines 1010, 1013 and 1021, and a repository-wide search of every `*.csproj` for the token
`QuickFileController` returns **zero** matches. It had been selected as the RS0030 control for the
`QuickFiler` project across two planning rounds and two preflight rounds before anyone checked. Had
it shipped, the control count would have been 0 on every channel, the plan's own rule would have
rejected every channel, and AC10 would have been unreachable.

The working substitute is `QuickFiler/Controllers/EfcHomeControllerDependencies.cs` line 77,
`MetricsNowFactory = metricsNowFactory ?? (() => DateTime.Now);`, registered at `QuickFiler.csproj`
line 302. It is the **only** live compiled `DateTime.Now` read in that project: of the 17 occurrences
under `QuickFiler/`, three are in the uncompiled file and thirteen are inside `//` comments.

**How to apply.** Verifying a control site takes two greps, not one:
1. the line is live code, not inside a comment or an `#if`;
2. the file is named by a `<Compile Include>` item in a project that the gate's build actually builds.

Do the second check for the sites *under test* as well, not only the controls — a site that cannot
emit is indistinguishable from a rule that does not fire, which is the same absence-proves-nothing
shape as [[suggestion-severity-diagnostics-invisible-to-msbuild]] and
[[absence-from-failure-list-is-not-a-pass-gate]].

Related: [[msbuild-analyzer-gate-vacuous-without-rebuild]], [[csharp-analyzer-packages-config-quirks]].
