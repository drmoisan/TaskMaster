# Precedent Capture

Timestamp: 2026-06-13T00-31

## (a) Affected test — RunWithTimeout_FuncT1TResult_ShouldReturnResult
Source: UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs lines 127–144

```csharp
        [TestMethod]
        public async Task RunWithTimeout_FuncT1TResult_ShouldReturnResult()
        {
            // Arrange
            Func<int, string> function = arg => $"result-{arg}";

            // Act
            var result = await function.RunWithTimeout(
                42,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: true
            );

            // Assert
            result.Should().Be("result-42");
        }
```

## (b) [TestClass] / partial class declaration carrying the attribute
Source: UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs lines 9–10

```csharp
    [TestClass]
    public partial class TimeOutTask_Tests
```

## (c) [DoNotParallelize] precedent
Source: UtilitiesCS.Test/Threading/ApplicationIdleTimer_Tests.cs lines 16–17

```csharp
    [TestClass]
    [DoNotParallelize]
    public class ApplicationIdleTimer_Tests
```

Notes:
- `TimeOutTask_Tests` is a partial class; `[TestClass]` appears on exactly one declaration (TimeOutTask_Tests.cs line 9). A class-level `[DoNotParallelize]` placed there governs the whole partial class.
