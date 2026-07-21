# Pre-Fix Defect State (Issue #270)

Timestamp: 2026-07-07T22-05

File: `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs`

## Defect 1 — `OlToDoItems_ItemChange` (lines 63-73)

```csharp
        private async void OlToDoItems_ItemChange(object item)
        {
            try
            {
                await ToDoEvents.OlToDoItems_ItemChange(item, OlToDoItems, Globals);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
```

The `catch (System.Exception) { throw; }` inside this `async void` method reschedules any fault onto the ThreadPool with no captured `SynchronizationContext`, terminating `outlook.exe`.

## Defect 2 — `OlInboxItems_ItemAdd` (lines 75-85)

```csharp
        internal async void OlInboxItems_ItemAdd(object item)
        {
            try
            {
                await ProcessMailItemAsync(item);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
```

The `catch (System.Exception) { throw; }` inside this `async void` method has the same defect: a fault from `ProcessMailItemAsync` is rethrown on a ThreadPool worker and terminates the host process.

Output Summary: Both `async void` COM event handlers in `AppEvents.ReadinessHookup.cs` currently contain `catch (System.Exception) { throw; }` at lines 63-73 and 75-85 respectively. This is the defect targeted by issue #270 AC1/AC2.
