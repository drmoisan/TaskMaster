# Baseline J1 Prohibited-Timing Source Quote (Cycle 7)

Timestamp: 2026-06-09T18-00
Source File: UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs

This is the pre-conversion baseline of the J1 test
`GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry`.
It retains a residual `Thread.Sleep(20)` (line 1287) and invokes the method via a
4-parameter reflection signature `{ Explorer, CancellationToken, int, int }` with
`timeoutMs: 5` (last argument). Cycle 7 converts this to a deterministic
injectable-timeout seam (no Thread.Sleep).

## Quoted lines (1260-1311)

```csharp
        [TestMethod]
        public async Task GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry()
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockTableView = new Mock<Outlook.TableView>();
            var mockExplorer = new Mock<Outlook.Explorer>();
            var callCount = 0;

            mockTableView
                .Setup(v => v.GetTable())
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        // ... documented PARTIAL improvement comment (cycle 6) ...
                        Thread.Sleep(20);
                    }

                    return mockTable.Object;
                });
            mockExplorer.Setup(e => e.CurrentView).Returns(mockTableView.Object);

            var result = await InvokeAsyncResult(
                "GetTableInViewAsync",
                new[]
                {
                    typeof(Outlook.Explorer),
                    typeof(CancellationToken),
                    typeof(int),
                    typeof(int),
                },
                mockExplorer.Object,
                CancellationToken.None,
                0,
                5
            );

            result.Should().BeSameAs(mockTable.Object);
            callCount.Should().Be(1);
        }
```

## Conversion baseline (exact prohibited line)

- Prohibited line to remove (line 1287): `                        Thread.Sleep(20);`
- Current reflection parameter types: `{ typeof(Outlook.Explorer), typeof(CancellationToken), typeof(int), typeof(int) }`
- Current arguments: `mockExplorer.Object, CancellationToken.None, 0, 5` (`timeoutMs: 5`)
- Assertions to preserve: `result.Should().BeSameAs(mockTable.Object)` and `callCount.Should().Be(1)`
