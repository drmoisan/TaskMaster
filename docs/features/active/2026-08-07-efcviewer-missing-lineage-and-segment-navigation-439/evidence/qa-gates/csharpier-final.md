Timestamp: 2026-08-24T19:35:00-04:00
Legacy Attempt Record: `evidence/baseline/csharpier-baseline.md`
Legacy Command: `dotnet tool run csharpier .`
Legacy Exit Code: `1`
Legacy Result: CSharpier 1.2.6 rejected the positional directory argument because the required `format` subcommand was absent; no formatting was performed by that legacy attempt.
Final Compatible Command: `dotnet tool run csharpier format .`
Final Exit Code: `0`
Output Summary: `Formatted 1520 files in 1557ms.` The final repeat was idempotent: tracked-diff hash remained `200cc98c468a5fa46e3a901c034941a69f5f3c8c`, and `BreadcrumbBridgeRouterIssue439Tests.cs` SHA256 remained `E33A8C86AFF61363017C253251182842484A6DB220A12E58387980DCA451BA3C`; no formatting change occurred in the final pass.

---
Restart Timestamp: 2026-08-24T19:46:00-04:00
Restart Reason: Review remediation changed the archive-root overload from public to internal; P4 restarted at P4-T2.
Command: `dotnet tool run csharpier format .`
EXIT_CODE: `0`
Output Summary: `Formatted 1520 files in 3947ms.` The restarted final pass was idempotent: tracked-diff hash remained `7cf9f54ce20fded5fbfdf29bf1eaed10928e96c4`, and `BreadcrumbBridgeRouterIssue439Tests.cs` SHA256 remained `E33A8C86AFF61363017C253251182842484A6DB220A12E58387980DCA451BA3C`; no formatting change occurred.

---
Restart Timestamp: 2026-08-24T20:20:30-04:00
Restart Reason: P3-T7/P3-T8 added C# test content after the prior QA attempt.
Command: `dotnet tool run csharpier format .`
EXIT_CODE: `0`
Output Summary: `Formatted 1520 files in 2465ms.` The formatter changed newly added Issue #439 test formatting, so the P4 loop restarts at P4-T1.

---
Timestamp: 2026-08-24T20:21:40-04:00
Legacy Attempt Record: `evidence/baseline/csharpier-baseline.md`
Legacy Command: `dotnet tool run csharpier .`
Legacy Exit Code: `1`
Final Compatible Command: `dotnet tool run csharpier format .`
Command: `git diff --binary | git hash-object --stdin; dotnet tool run csharpier format .; git diff --binary | git hash-object --stdin`
EXIT_CODE: `0`
Output Summary: `Formatted 1520 files in 1573ms.` The tracked-diff hash was unchanged (`f402884b41dbaecfec2408195fd0a3588f01666a`) before and after formatting; no formatting change occurred.

---
Timestamp: 2026-08-24T20:31:37-04:00
Legacy Attempt Record: `evidence/baseline/csharpier-baseline.md`
Legacy Command: `dotnet tool run csharpier .`
Legacy Exit Code: `1`
Final Compatible Command: `dotnet tool run csharpier format .`
Command: `dotnet tool run csharpier format .`
EXIT_CODE: `0`
Output Summary: `Formatted 1520 files in 1515ms.` No formatting change occurred: tracked-diff hash remained `6a7e701f918ec2de48b1c38a6d66034230f9998a`, `BreadcrumbBridgeRouterIssue439Tests.cs` SHA256 remained `1FFE35B42E89D7B4C0408196E07E6C8D41C9811655CB6D8DC77DDD66269A31C3`, and `BreadcrumbBridgeRouterQueueTests.cs` SHA256 remained `F97866B6E064F9F91D7A24CAE29AD7C72A860EC43EC2FEDAF2FA5A2BC8ACD686`.
