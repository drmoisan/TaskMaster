# QA Gate: Determinism and Library Tokens of the New Partial (P2-T12)

Timestamp: 2026-10-01T18-11
Task: P2-T12
Command: CMD-TOKEN-COUNT (FILE TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs; TOKEN "Thread.Sleep", "Task.Delay", "GetTempPath", "GetTempFileName", "File.", "Directory.", "DoNotParallelize", "using Microsoft.VisualStudio.TestTools.UnitTesting;", "using Moq;", "using FluentAssertions;")
EXIT_CODE: 0

Output Summary:
- Prohibited tokens each 0: Thread.Sleep, Task.Delay, GetTempPath, GetTempFileName, File., Directory., DoNotParallelize.
- Library tokens each exactly 1: using Microsoft.VisualStudio.TestTools.UnitTesting; (line 4), using Moq; (line 5), using FluentAssertions; (line 3). These three positive tokens show the scan read the file.
- Result: P2-T12 acceptance holds (MSTest, Moq and FluentAssertions; no sleeps, delays, temporary files or serialization attributes).

```
TOKEN [Thread.Sleep] = 0
TOKEN [Task.Delay] = 0
TOKEN [GetTempPath] = 0
TOKEN [GetTempFileName] = 0
TOKEN [File.] = 0
TOKEN [Directory.] = 0
TOKEN [DoNotParallelize] = 0
TOKEN [using Microsoft.VisualStudio.TestTools.UnitTesting;] = 1
TOKEN [using Moq;] = 1
TOKEN [using FluentAssertions;] = 1
FIRST-LINE [Thread.Sleep] = 0
FIRST-LINE [Task.Delay] = 0
FIRST-LINE [GetTempPath] = 0
FIRST-LINE [GetTempFileName] = 0
FIRST-LINE [File.] = 0
FIRST-LINE [Directory.] = 0
FIRST-LINE [DoNotParallelize] = 0
FIRST-LINE [using Microsoft.VisualStudio.TestTools.UnitTesting;] = 4
FIRST-LINE [using Moq;] = 5
FIRST-LINE [using FluentAssertions;] = 3
```
