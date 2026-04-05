# P4-T60: FileIO2 Return To Implementation

## File
`UtilitiesCS\To Depricate\FileIO2.cs`

## Source Decision
Return To Implementation

## Coverage Result
`RATE=0.8333333333333334 COVERED=105 TOTAL=126`

## Exact Test Method Names
- `DeleteTextFile_WhenTargetIsMissing_ShouldNotThrow`
- `WriteTextFile_WhenDevicePathIsUsed_ShouldThrowNotSupportedException`
- `WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing`
- `CsvReaders_WithFixtureAndMissingFiles_ShouldRespectHeaderOptions`
- `SplitArrayTo2D_ShouldSupportZeroAndOneBasedLayouts`
- `CsvReadTo2D_AndCsvReadToJagged_ShouldProjectFixtureRows`
