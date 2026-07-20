#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.EmailIntelligence.Bayesian.Performance;
using UtilitiesCS.Extensions;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public partial class EmailDataMiner
    {
        #region ETL - TRANSFORM For Data Mining

        public delegate Task<T> FolderGroupTransformer<T>(
            FolderWrapper[] folders,
            int batch,
            int totalBatches,
            ProgressTrackerPane progress,
            CancellationToken token
        );

        [ExcludeFromCodeCoverage]
        public async Task Transform(
            FolderWrapper[][] folderChunks,
            FolderGroupTransformer<IItemInfo[]> transformer,
            bool withValidation
        )
        {
            var (_, token, progress, _) = await ProgressPackage
                .CreateAsTuplePaneAsync(progressTrackerPane: _globals.AF.ProgressTracker)
                .ConfigureAwait(false);
            _globals.AF.ProgressPane.Visible = true;
            var message =
                $"Transforming from {typeof(FolderWrapper[][]).Name} to {typeof(IItemInfo[])}";
            progress!.Report(0, message);

            if (!_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                return;
            }
            var folderPath = Path.Combine(folderRoot, "Bayesian");
            var (completed, chunkCount) = await EmailDataMiner.DeserializeAsync<(int, int)>(
                folderPath,
                "FolderGroupCompleted"
            );
            if (folderChunks.Count() != chunkCount)
            {
                //logger.Debug($"FolderChunks count {folderChunks.Count()} does not match chunkCount {chunkCount}. Restarting transformation with new data");
                chunkCount = folderChunks.Count();
                completed = 0;
            }
            var progressPerChunk = 100 / (double)chunkCount;

            for (int i = 0; i < chunkCount; i++)
            {
                if (i < completed)
                {
                    if (withValidation)
                    {
                        if (
                            await ValidateJson<IItemInfo[]>(
                                typeof(IItemInfo[]).Name,
                                i.ToString("0000")
                            )
                        )
                        {
                            progress.Report(
                                (int)((i + 1) * progressPerChunk),
                                $"Validated group {i + 1} of {chunkCount}"
                            );
                            continue;
                        }
                    }
                    else
                    {
                        progress.Report(
                            (int)((i + 1) * progressPerChunk),
                            $"Skipping group {i + 1} of {chunkCount}"
                        );
                        continue;
                    }
                }

                var result = await transformer(
                    folderChunks[i],
                    i,
                    chunkCount,
                    progress.SpawnChild(progressPerChunk),
                    token
                );
                SerializeAndSave(result, result.GetType().Name, i.ToString("0000"));

                var processed = (completed: i + 1, chunkCount);
                SerializeAndSave(processed, "FolderGroupCompleted");
            }

            progress.Report(100);
            _globals.AF.ProgressPane.Visible = false;
        }

        [ExcludeFromCodeCoverage]
        public async Task<IItemInfo[]> ToIItemInfoArray(
            FolderWrapper[] folders,
            int batch,
            int totalBatches,
            ProgressTrackerPane progress,
            CancellationToken token
        )
        {
            var sw = await Task.Run(() => new SegmentStopWatch().Start());
            var mailTuples = QueryMailTuples(folders).ToArray();
            sw.LogDuration("QueryMailTuples");

            var count = mailTuples.Count();
            if (count == 0)
            {
                progress.Report(100);
                return default!;
            }

            var cBag = await AsyncMultiTasker.AsyncMultiTaskChunker(
                mailTuples,
                async (mailTuple) => await ToIItemInfo(mailTuple, token),
                progress,
                $"Mining Mail Batch {batch} of {totalBatches} ",
                token
            );

            cBag.ForEach(x =>
            {
                sw.MergeDurations(x.Sw.Durations);
                x.Sw.Stop();
                x.Sw = null;
            });
            sw.WriteToLog(clear: true);

            progress.Report(100);

            return cBag.ToArray();
        }

        [ExcludeFromCodeCoverage]
        public async Task<IItemInfo> ToIItemInfo(
            (MailItem Mail, FolderWrapper FolderInfo) mailTuple,
            CancellationToken cancel
        )
        {
            var mailInfo = await CreateMailItemHelperAsync(mailTuple.Mail, cancel);

            mailInfo.FolderInfo = mailTuple.FolderInfo;

            await mailInfo.TokenizeAsync();
            var serializable = mailInfo.ToSerializableObject();
            serializable.Sw = mailInfo.Sw;
            serializable.Sw!.LogDuration("ToSerializableObject");

            foreach (var attachment in serializable.AttachmentsInfo!)
            {
                if (!attachment.IsImage)
                {
                    attachment.AttachmentData = null;
                }
            }
            return serializable;
        }

        internal Task<IItemInfo> ToIItemInfo(
            MailItem mail,
            FolderWrapper folderInfo,
            CancellationToken cancel
        )
        {
            return ToIItemInfo((mail, folderInfo), cancel);
        }

        internal virtual Task<MailItemHelper> CreateMailItemHelperAsync(
            MailItem mailItem,
            CancellationToken cancel
        )
        {
            return MailItemHelper.FromMailItemAsync(mailItem, _globals, cancel, true);
        }

        [ExcludeFromCodeCoverage]
        public async Task Transform<Tin, Tout>(Func<Tin, Task<Tout>> transformer)
        {
            var (_, token, progress, _) = await ProgressPackage
                .CreateAsTuplePaneAsync(progressTrackerPane: _globals.AF.ProgressTracker)
                .ConfigureAwait(false);
            _globals.AF.ProgressPane.Visible = true;
            var message = $"Transforming from {typeof(Tin).Name} to {typeof(Tout)}";
            progress!.Report(0, message);

            var tInName = FolderConverter.SanitizeFilename(typeof(Tin).Name);
            var tOutName = FolderConverter.SanitizeFilename(typeof(Tout).Name);
            if (!_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                logger.Debug($"AppData Folder Not Found. Aborting method {nameof(ToMinedMail)}");
                return;
            }
            var folderPath = Path.Combine(folderRoot, "Bayesian");
            (_, var count) = await EmailDataMiner
                .DeserializeAsync<(int, int)>(folderPath, "FolderGroupCompleted")
                .ConfigureAwait(false);
            var completed = await EmailDataMiner
                .DeserializeAsync<int>(folderPath, $"{tOutName}Completed")
                .ConfigureAwait(false);
            var completedPerChunk = 100 / (double)count;
            var serializer = new BayesianSerializationHelper(_globals);

            for (int i = 0; i < count; i++)
            {
                if (i < completed)
                {
                    try
                    {
                        var objOut = await DeserializeAsync<Tout>(
                                folderPath,
                                $"{tOutName}_{i:0000}"
                            )
                            .ConfigureAwait(false);
                        progress.Report(
                            (int)((i + 1) * completedPerChunk),
                            $"{message}. Validated {i + 1} of {count}"
                        );
                        continue;
                    }
                    catch (System.Exception e)
                    {
                        logger.Error(
                            $"Error deserializing {tOutName}_{i:0000}.json. Rebuilding ...\n{e.Message}",
                            e
                        );
                    }
                }

                Tin obj = (
                    await serializer
                        .DeserializeAsync<Tin>(
                            progress.SpawnChild(completedPerChunk),
                            $"{tInName}_{i:0000}"
                        )
                        .ConfigureAwait(false)
                )!;
                Tout result = await transformer(obj);
                //if (count == 1)
                //    SerializeAndSave(result, tOutName);
                //else
                SerializeAndSave(result, $"{tOutName}_{i:0000}");
                SerializeAndSave(i + 1, $"{tOutName}Completed");
                //progress.Report((int)((i + 1) * completedPerChunk), $"{message}. Transformed {i + 1} of {count}");
            }

            progress.Report(100);
            _globals.AF.ProgressPane.Visible = false;
        }

        public async Task<MinedMailInfo[]> ToMinedMail(IItemInfo[] items)
        {
            return (
                await Task.Run(() =>
                    items?.Select(item => new MinedMailInfo(item))?.ToArray() ?? null
                )
            )!;
        }

        public async Task<MinedMailInfo[]> FilterExcluded(MinedMailInfo[] items)
        {
            return await Task.Run(() =>
                items
                    .Where(x =>
                        !_globals.TD.FilteredFolderScraping.ContainsKey(x.FolderInfo!.RelativePath)
                    )
                    .ToArray()
            );
        }

        public async Task<MinedMailInfo[]> RemapFolderPaths(MinedMailInfo[] items)
        {
            await Task.Run(() =>
            {
                foreach (var item in items)
                {
                    if (_globals.TD.FolderRemap.ContainsKey(item.FolderInfo!.RelativePath))
                    {
                        item.FolderInfo!.RelativePath = _globals.TD.FolderRemap[
                            item.FolderInfo!.RelativePath
                        ];
                    }
                }
                //items.ForEach(x => x.FolderPath = _globals.TD.DictRemap.ContainsKey(x.FolderPath) ?
                //           _globals.TD.DictRemap[x.FolderPath] : x.FolderPath);
            });
            return items;
        }

        [ExcludeFromCodeCoverage]
        public async Task<MinedMailInfo> ToMinedMail(MailItem mailItem, CancellationToken cancel)
        {
            var mailInfo = await CreateMailItemHelperAsync(mailItem, cancel);

            await mailInfo.TokenizeAsync();

            var minedInfo = new MinedMailInfo(mailInfo);
            return minedInfo;
        }

        [ExcludeFromCodeCoverage]
        public async Task Transform<Tin, Tout>(Func<Tin[], Task<Tout>> transformer)
        {
            var (_, token, progress, _) = await ProgressPackage
                .CreateAsTuplePaneAsync(progressTrackerPane: _globals.AF.ProgressTracker)
                .ConfigureAwait(false);
            _globals.AF.ProgressPane.Visible = true;
            var message = $"Transforming from {typeof(Tin).Name} to {typeof(Tout)}";
            progress!.Report(0, message);

            var tInName = FolderConverter.SanitizeFilename(typeof(Tin).Name);
            var tOutName = FolderConverter.SanitizeFilename(typeof(Tout).Name);
            var (_, count) = Deserialize<(int, int)>("FolderGroupCompleted");
            List<Tin> list = [];
            for (int i = 0; i < count; i++)
            {
                Tin? obj = await Task.Run(() => Deserialize<Tin>($"{tInName}_{i:0000}"));
                if (obj is not null)
                {
                    list.Add(obj);
                }
            }
            Tout result = await transformer([.. list]);
            SerializeAndSave(result, tOutName);

            progress.Report(100);
            _globals.AF.ProgressPane.Visible = false;
        }

        public async Task<MinedMailInfo[]> Consolidate(MinedMailInfo[][] jagged)
        {
            var combined = await Task.Run(() => jagged.SelectMany(x => x).ToArray());
            combined = await Task.Run(() => FilterExcluded(combined));
            combined = await Task.Run(() => RemapFolderPaths(combined));
            return combined;
        }

        [ExcludeFromCodeCoverage]
        public async Task ToMinedMail(
            FolderWrapper[] folders,
            int batch,
            int totalBatches,
            ProgressTracker progress,
            CancellationToken token
        )
        {
            var mailItems = QueryMailItems(folders.Select(x => x.OlFolder!)).ToArray();

            var count = mailItems.Count();
            if (count == 0)
            {
                progress.Report(100);
                return;
            }

            var cBag = await AsyncMultiTasker.AsyncMultiTaskChunker(
                mailItems,
                async (mailItem) => await ToMinedMail(mailItem, token),
                progress,
                $"Mining Mail Batch {batch} of {totalBatches} ",
                token
            );

            progress.Report(100);

            if (!_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                logger.Debug($"AppData Folder Not Found. Aborting method {nameof(ToMinedMail)}");
                return;
            }

            var minedBag = new ScBag<MinedMailInfo>(cBag)
            {
                FolderPath = Path.Combine(folderRoot, "Bayesian"),
                FileName = $"MinedMailInfo_{batch:000}.json",
            };

            minedBag.Serialize();
        }

        #endregion ETL - TRANSFORM For Data Mining

        #region ETL - LOAD To Data Mining

        public static async Task<T> Load<T>(string folderPath, string fileName = "")
        {
            var tName = FolderConverter.SanitizeFilename(typeof(T).Name);
            if (fileName.IsNullOrEmpty())
            {
                fileName = tName;
            }
            T result = (await EmailDataMiner.DeserializeAsync<T>(folderPath, fileName))!;

            return result;
        }

        #endregion ETL - LOAD To Data Mining
    }
}
