# Baseline B1-B3 Prohibited-Timing Source Quote (Cycle 7)

Timestamp: 2026-06-09T18-00
Source File: UtilitiesCS.Test/ReusableTypeClasses/TimerWrapper_Tests.cs

Pre-conversion baseline of B1 (`StartTimer_RaisesElapsedEvent`),
B2 (`StopTimer_PreventsPendingElapsedEvent`), and B3
(`StartNew_ConfiguresAutoResetAndInvokesCallback`). Each relies on a real
`System.Timers.Timer` plus a `ManualResetEventSlim` with a bounded
`signal.Wait(<timeout>)`. Cycle 7 converts these to a deterministic inner-timer
seam (no signal.Wait).

## B1 — StartTimer_RaisesElapsedEvent (lines 32-46)

```csharp
        [TestMethod]
        public void StartTimer_RaisesElapsedEvent()
        {
            // Arrange
            using var signal = new ManualResetEventSlim(false);
            using var timer = new TimerWrapper(TimeSpan.FromMilliseconds(20));
            timer.AutoReset = false;
            timer.Elapsed += (_, _) => signal.Set();

            // Act
            timer.StartTimer();

            // Assert
            signal.Wait(500).Should().BeTrue();
        }
```

Prohibited line (B1): `            signal.Wait(500).Should().BeTrue();`

## B2 — StopTimer_PreventsPendingElapsedEvent (lines 48-63)

```csharp
        [TestMethod]
        public void StopTimer_PreventsPendingElapsedEvent()
        {
            // Arrange
            using var signal = new ManualResetEventSlim(false);
            using var timer = new TimerWrapper(TimeSpan.FromMilliseconds(150));
            timer.AutoReset = false;
            timer.Elapsed += (_, _) => signal.Set();

            // Act
            timer.StartTimer();
            timer.StopTimer();

            // Assert
            signal.Wait(250).Should().BeFalse();
        }
```

Prohibited line (B2): `            signal.Wait(250).Should().BeFalse();`

## B3 — StartNew_ConfiguresAutoResetAndInvokesCallback (lines 65-81)

```csharp
        [TestMethod]
        public void StartNew_ConfiguresAutoResetAndInvokesCallback()
        {
            // Arrange
            using var signal = new ManualResetEventSlim(false);

            // Act
            using var timer = TimerWrapper.StartNew(
                TimeSpan.FromMilliseconds(20),
                autoReset: false,
                callback: signal.Set
            );

            // Assert
            timer.AutoReset.Should().BeFalse();
            signal.Wait(500).Should().BeTrue();
        }
```

Prohibited line (B3): `            signal.Wait(500).Should().BeTrue();`

## Conversion baseline summary (all three prohibited waits)

- B1 (line 45): `signal.Wait(500)` — remove; assert outer Elapsed forwarding via fake.
- B2 (line 62): `signal.Wait(250)` — remove; assert stop-suppression via fake.
- B3 (line 80): `signal.Wait(500)` — remove; assert AutoReset + callback via fake.
- Assertion intent to preserve: B1 raises Elapsed; B2 suppresses Elapsed after stop; B3 `timer.AutoReset.Should().BeFalse()` + callback invoked on fire.
