# P1-T13 — Debug build output the Phase 2 runbook consumes

Timestamp: 2026-09-07T14-26
Task: [P1-T13]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command 'Get-ChildItem TaskMaster\bin\Debug\*.dll | ForEach-Object { $_.Name + " " + $_.LastWriteTimeUtc.ToString("o") }'
```

EXIT_CODE: 0

## Acceptance

QuickFiler.dll is present in TaskMaster/bin/Debug.

| Quantity | Value |
|---|---|
| QuickFiler.dll LastWriteTimeUtc | 2026-09-07T14:24:31.6560560Z |
| `RunStartedUtc:` recorded in evidence/qa-gates/p1-t10-nullable-rebuild.md | 2026-09-07T14:24:20.9418220Z |
| At or later than that RunStartedUtc | yes, by 10.7 seconds |

The output the human will run against is therefore the instrumented output produced
by the P1-T10 rebuild, not a stale copy.

## Full listing

```
AngleSharp.dll 2026-09-03T07:01:56.0000000Z
Apache.Arrow.dll 2026-05-03T03:36:34.0000000Z
Apache.Arrow.Scalars.dll 2026-05-03T03:36:28.0000000Z
C.math.dll 2016-10-19T19:25:40.0000000Z
Deedle.dll 2023-01-17T20:56:28.0000000Z
ExCSS.dll 2026-07-23T23:21:12.0000000Z
FSharp.Core.dll 2026-02-20T01:58:44.0000000Z
Generic.Math.dll 2017-08-26T05:48:52.0000000Z
log4net.dll 2026-08-18T20:25:00.0000000Z
log4net.Ext.Json.dll 2024-12-03T08:19:52.0000000Z
Microsoft.Bcl.AsyncInterfaces.dll 2026-07-24T15:52:40.0000000Z
Microsoft.Bcl.Memory.dll 2026-07-24T15:52:40.0000000Z
Microsoft.Bcl.TimeProvider.dll 2026-07-24T15:53:10.0000000Z
Microsoft.Data.Analysis.dll 2025-11-08T03:30:32.0000000Z
Microsoft.IO.RecyclableMemoryStream.dll 2024-06-11T20:23:46.0000000Z
Microsoft.ML.DataView.dll 2025-11-08T03:29:38.0000000Z
Microsoft.Office.Tools.Common.v4.0.Utilities.dll 2026-05-27T13:42:53.6920325Z
Microsoft.Office.Tools.Outlook.v4.0.Utilities.dll 2026-05-27T13:42:53.6960325Z
Microsoft.Web.WebView2.Core.dll 2026-08-23T23:21:22.0000000Z
Microsoft.Web.WebView2.WinForms.dll 2026-08-23T23:21:00.0000000Z
Mono.Reflection.dll 2019-11-26T22:39:12.0000000Z
Newtonsoft.Json.dll 2025-09-16T08:04:22.0000000Z
ObjectListView.dll 2016-05-05T23:35:46.0000000Z
QuickFiler.dll 2026-09-07T14:24:31.6560560Z
Svg.dll 2026-07-22T18:29:58.0000000Z
SVGControl.dll 2026-09-07T14:24:21.7909598Z
System.Buffers.dll 2025-03-19T20:55:38.0000000Z
System.Collections.Immutable.dll 2026-07-24T15:54:02.0000000Z
System.Diagnostics.DiagnosticSource.dll 2026-07-24T15:53:14.0000000Z
System.Interactive.Async.dll 2026-04-17T15:49:16.0000000Z
System.Interactive.dll 2026-04-17T15:49:16.0000000Z
System.Linq.Async.dll 2026-04-17T15:49:22.0000000Z
System.Memory.dll 2025-04-03T23:00:54.0000000Z
System.Numerics.Vectors.dll 2025-03-19T20:55:42.0000000Z
System.Reactive.Async.dll 2023-05-31T09:22:34.0000000Z
System.Reactive.dll 2026-07-17T09:44:36.0000000Z
System.Runtime.CompilerServices.Unsafe.dll 2025-04-03T23:00:52.0000000Z
System.Text.Encoding.CodePages.dll 2026-07-24T15:56:34.0000000Z
System.Threading.Tasks.Dataflow.dll 2026-07-24T15:56:38.0000000Z
System.Threading.Tasks.Extensions.dll 2025-04-03T23:00:52.0000000Z
Tags.dll 2026-09-07T14:24:27.4196276Z
TaskMaster.dll 2026-09-07T14:24:33.2382042Z
TaskTree.dll 2026-09-07T14:24:28.9331312Z
TaskVisualization.dll 2026-09-07T14:24:29.3973892Z
Tesseract.dll 2022-11-30T08:37:18.0000000Z
ToDoModel.dll 2026-09-07T14:24:28.3818566Z
UtilitiesCS.dll 2026-09-07T14:24:26.7788304Z
```

Every first-party assembly (QuickFiler, SVGControl, Tags, TaskMaster, TaskTree,
TaskVisualization, ToDoModel, UtilitiesCS) carries a LastWriteTimeUtc within the
P1-T10 rebuild window. The remaining entries are third-party dependencies copied from
the packages directory and carry their own package timestamps, which is expected.

Output Summary: QuickFiler.dll is present in TaskMaster/bin/Debug with
LastWriteTimeUtc 2026-09-07T14:24:31.6560560Z, later than the P1-T10 RunStartedUtc of
2026-09-07T14:24:20.9418220Z.
