# Baseline — Named Test Source (Conversion Baseline)

Timestamp: 2026-06-09T11-31
Source File: UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableBase_Tests.cs
Test Method: Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite

Current working-tree edit by the user introduced the prohibited wall-clock waits that this
cycle converts. The exact baseline lines (research rows A1 line 571, A2 line 575):

Line 571 (A1):
```
            Thread.Sleep(50);
```

Line 575 (A2):
```
            signal.Wait(5000).Should().BeTrue();
```

Full method body at baseline (lines 553-577):
```csharp
        [TestMethod]
        public void Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            var instance = new BaseTestItem();
            using var signal = new ManualResetEventSlim(false);
            instance.Config.Disk.FilePath = Path.Combine(@"C:\SmartBase", "queued-trigger.json");
            sut.SetCreateStreamWriter(_ =>
            {
                signal.Set();
                return new StreamWriter(new MemoryStream(), Encoding.UTF8, 1024, leaveOpen: false);
            });

            // Act
            sut.Serialize(instance);

            // Wait briefly for timer to be created, then accelerate it
            Thread.Sleep(50);
            AcceleratePrivateTimer(sut, typeof(SmartSerializableBase));

            // Assert
            signal.Wait(5000).Should().BeTrue();
            StopPrivateTimer(sut, typeof(SmartSerializableBase));
        }
```

Conversion target: replace `Thread.Sleep(50)` and `signal.Wait(5000)` with deterministic
`ManualFireTimerWrapper` injection via the new `TimerFactory` seam (S1). The assertion intent
(the deferred write callback fires and invokes `CreateStreamWriter`, observed via `signal`) is
preserved by calling `timerStub.FireElapsed()` after confirming the timer was created, then
asserting `signal.IsSet.Should().BeTrue()`.

Note: The harness `SmartSerializableBaseHarness` (lines 603-639) uses the `Set*` accessor
pattern for protected members; the S1 `TimerFactory` injection point will follow the same pattern.
