# Phase 0 — Baseline File Inventory (P0-T2)

Timestamp: 2026-07-19T08-52

Command: `find UtilitiesCS/ReusableTypeClasses -name "*.cs"` (recursive), with per-file `wc -l`
and `grep -n "#nullable enable"`.

Total `.cs` files under `UtilitiesCS/ReusableTypeClasses/` (recursive): 54
- In scope: 51
- Exempt (WinForms exemption (b), do NOT opt in): 3
- Files currently carrying `#nullable enable`: 0 (greenfield — every file is null-oblivious at baseline)

## Exempt files (3) — MUST NOT receive `#nullable enable`
| Lines | Path |
|---|---|
| 3733 | NewSmartSerializable/Config/ConfigViewer.Designer.cs |
| 147 | NewSmartSerializable/Config/ConfigViewer.cs |
| 42 | NewSmartSerializable/Config/ConfigGroupBox.cs |

## In-scope files (51) — line count, pragma at baseline, batch mapping
| Lines | Pragma@baseline | Batch | Path |
|---|---|---|---|
| 23 | no | 1 | Concurrent/Observable/Bag/BagChangedEventArgs.cs |
| 7 | no | 1 | Concurrent/Observable/Bag/ISimpleActionBagObserver.cs |
| 19 | no | 1 | Concurrent/Observable/Bag/SimpleActionBagObserver.cs |
| 30 | no | 1 | Concurrent/Observable/Dictionary/DictionaryChangedEventArgs.cs |
| 21 | no | 1 | Concurrent/Observable/Dictionary/SimpleActionDictionaryObserver.cs |
| 36 | no | 1 | Locking/ILockingLinkedList.cs |
| 9 | no | 1 | Locking/Observable/LinkedList/ILockingLinkedListObserver.cs |
| 26 | no | 1 | Locking/Observable/LinkedList/LockingObservableLinkedListChangedEventArgs.cs |
| 27 | no | 1 | Locking/Observable/LinkedList/SimpleActionLockingLinkedListObserver.cs |
| 31 | no | 1 | Observable/ObservableCollectionBatchUpdate.cs |
| 43 | no | 1 | Observable/ObserverHelper.cs |
| 43 | no | 1 | Other/AbstractCloneable.cs |
| 63 | no | 1 | Concurrent/Observable/Collection/IConcurrentObservableCollectionSeams.cs |
| 40 | no | 2 | Other/AsyncQueue.cs |
| 159 | no | 2 | AsyncLazy/AsyncLazy.cs |
| 62 | no | 2 | LazyTry/LazyTry.cs |
| 195 | no | 2 | Other/StackGeek.cs |
| 196 | no | 2 | Other/StackObjectCS.cs |
| 339 | no | 2 | Other/TreeNodeOfT.cs |
| 56 | no | 2 | Matrices/DataConverter2d.cs |
| 184 | no | 3 | Matrices/DenMatrix.cs |
| 188 | no | 3 | Matrices/JaggedMatrix.cs |
| 157 | no | 3 | Matrices/Matrix.cs |
| 185 | no | 4 | TimedActions/TimerWrapper.cs |
| 123 | no | 4 | TimedActions/TimedAsyncTask.cs |
| 102 | no | 4 | TimedActions/TimedBatchAction.cs |
| 369 | no | 4 | TimedActions/TimedQueueOfActions.cs |
| 363 | no | 4 | TimedActions/TimedDiskWriter.cs |
| 124 | no | 5 | Locking/LockingLinkedListNode.cs |
| 456 | no | 5 | Locking/LockingLinkedList.cs |
| 126 | no | 5 | Locking/Observable/LinkedList/LockingObservableLinkedListNode.cs |
| 522 | no | 5 | Locking/Observable/LinkedList/LockingObservableLinkedList.cs (pre-existing >500) |
| 252 | no | 6 | Concurrent/Observable/Bag/ConcurrentObservableBag.cs |
| 169 | no | 6 | Concurrent/Observable/Collection/ConcurrentObservableCollection.cs |
| 405 | no | 6 | Concurrent/Observable/Collection/ConcurrentObservableCollection.Serialization.cs |
| 375 | no | 6 | Concurrent/Observable/Dictionary/ConcurrentObservableDictionary.cs |
| 834 | no | 6 | Observable/ObservableDictionary.cs (pre-existing >500) |
| 278 | no | 7 | NewSmartSerializable/Config/NewSmartSerializableConfig.cs |
| 534 | no | 7 | NewSmartSerializable/SmartSerializableBase.cs (pre-existing >500) |
| 596 | no | 7 | NewSmartSerializable/SmartSerializable.cs (pre-existing >500) |
| 107 | no | 7 | NewSmartSerializable/SmartSerializableStatic.cs |
| 104 | no | 7 | NewSmartSerializable/SmartSerializableNonTyped.cs |
| 205 | no | 7 | NewSmartSerializable/SmartSerializableLoader.cs |
| 154 | no | 7 | NewSmartSerializable/Config/ConfigController.cs |
| 575 | no | 8 | Serializable/SerializableList.cs (pre-existing >500) |
| 325 | no | 8 | Serializable/Concurrent/ScBag.cs |
| 49 | no | 8 | SerializableNew/Concurrent/Observable/ScoDictionaryStatic.cs |
| 281 | no | 8 | SerializableNew/Concurrent/Observable/ScoDictionaryNew.cs |
| 168 | no | 8 | SerializableNew/Concurrent/Observable/SloLinkedList.cs |
| 260 | no | 8 | SerializableNew/Concurrent/Observable/SloStack.cs |
| 449 | no | 8 | SerializableNew/Concurrent/ScDictionary.cs |

Batches 1-5 (this run's scope): 13 + 7 + 3 + 5 + 4 = 32 in-scope files.
Batches 6-8 (out of scope for this run, gated on CS8714 ratification): 5 + 7 + 7 = 19 files.

Output Summary: 54 files enumerated; exactly 3 exempt and 51 in scope; zero files carry
`#nullable enable` at baseline (greenfield). Six in-scope files exceed the 500-line limit
(pre-existing; not split by this annotation-only feature).
