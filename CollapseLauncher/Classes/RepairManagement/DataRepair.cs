﻿using Hi3Helper.Http;
using Hi3Helper.Shared.ClassStruct;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static CollapseLauncher.Pages.RepairData;
using static Hi3Helper.Data.ConverterTool;
using static Hi3Helper.Locale;
using static Hi3Helper.Shared.Region.LauncherConfig;

namespace CollapseLauncher.Pages
{
    public sealed partial class RepairPage : Page
    {
        int RepairedFilesCount;

        byte DownloadThread = (byte)GetAppConfigValue("DownloadThread").ToInt();
        int MultipleDownloadSizeStart = 4 << 20;

        private async void StartGameRepair(object sender, RoutedEventArgs e)
        {
            RepairFilesBtn.IsEnabled = false;
            CancelBtn.IsEnabled = true;

            try
            {
                await CategorizeRepairAction();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Repairing Cancelled!");
            }
            catch (Exception ex)
            {
                ErrorSender.SendException(ex);
                Console.WriteLine($"{ex}");
            }
        }

        private async Task CategorizeRepairAction()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                RepairTotalProgressBar.Value = 0;
            });

            using (_httpClient = new Http())
            {
                foreach (var BrokenFile in BrokenFileIndexesProperty)
                {
                    FilePath = Path.Combine(GameBasePath, NormalizePath(BrokenFile.N));
                    if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

                    switch (BrokenFile.FT)
                    {
                        case FileType.Generic:
                        case FileType.Audio:
                            await RepairGenericAudioFiles(BrokenFile);
                            break;
                        case FileType.Blocks:
                            await RepairBlockFiles(BrokenFile);
                            break;
                    }
                }
            }

            DispatcherQueue.TryEnqueue(() =>
            {
                sw.Stop();
                CancelBtn.IsEnabled = false;
                CheckFilesBtn.Visibility = Visibility.Visible;
                CheckFilesBtn.IsEnabled = true;
                RepairFilesBtn.Visibility = Visibility.Collapsed;
                RepairStatus.Text = Lang._GameRepairPage.Status7;
                RepairPerFileStatus.Text = Lang._GameRepairPage.StatusNone;
                RepairTotalStatus.Text = Lang._GameRepairPage.StatusNone;
                RepairPerFileProgressBar.Value = 0;
                RepairTotalProgressBar.Value = 0;
            });
        }

        FileInfo RepairFileInfo;
        FileStream RepairFileStream;
        private async Task RepairGenericAudioFiles(FilePropertiesRemote input)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                RepairStatus.Text = string.Format(Lang._GameRepairPage.Status8, input.N);
                RepairTotalStatus.Text = string.Format(Lang._GameRepairPage.PerProgressSubtitle2, RepairedFilesCount, BrokenFilesCount);
                RepairTotalProgressBar.Value = GetPercentageNumber(RepairedFilesCount, BrokenFilesCount, 2);
            });

            RepairFileInfo = new FileInfo(FilePath);

            if (input.FT == FileType.Generic)
            {
                FileURL = GameBaseURL + input.N;
            }
            else
            {
                FileURL = GameBaseURL + Path.GetDirectoryName(input.N).Replace('\\', '/') + $"/{input.RN}";
            }

            if (input.S == 0)
                File.Create(FilePath).Dispose();
            else
            {
                _httpClient.DownloadProgress += GenericFilesDownloadProgress;

                if (input.S > MultipleDownloadSizeStart)
                {
                    await _httpClient.Download(FileURL, FilePath, DownloadThread, true, cancellationTokenSource.Token);
                    await _httpClient.Merge();
                }
                else
                {
                    await _httpClient.Download(FileURL, FilePath, true, null, null, cancellationTokenSource.Token);
                }

                _httpClient.DownloadProgress -= GenericFilesDownloadProgress;
            }
            DispatcherQueue.TryEnqueue(() =>
            {
                NeedRepairListUI.RemoveAt(0);
            });
            RepairedFilesCount++;
        }

        private async Task RepairBlockFiles(FilePropertiesRemote input)
        {
            string BlockBasePath = NormalizePath(input.N);
            foreach (var block in input.BlkC)
            {
                FileURL = GameBaseURL + input.N + $"/{block.BlockHash}.wmv";
                FilePath = Path.Combine(GameBasePath, BlockBasePath, block.BlockHash + ".wmv");
                RepairFileInfo = new FileInfo(FilePath);

                long FileExistingLength = RepairFileInfo.Exists ? RepairFileInfo.Length : 0;

                if (RepairFileInfo.Exists && RepairFileInfo.Length != block.BlockSize)
                    RepairFileInfo.Delete();

                if (!RepairFileInfo.Exists)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        RepairStatus.Text = string.Format(Lang._GameRepairPage.Status9, block.BlockHash);
                        RepairTotalStatus.Text = string.Format(Lang._GameRepairPage.PerProgressSubtitle2, RepairedFilesCount, BrokenFilesCount);
                        RepairTotalProgressBar.Value = GetPercentageNumber(RepairedFilesCount, BrokenFilesCount, 2);
                    });

                    _httpClient.DownloadProgress += GenericFilesDownloadProgress;

                    if (block.BlockSize > MultipleDownloadSizeStart)
                    {
                        await _httpClient.Download(FileURL, FilePath, DownloadThread, true, cancellationTokenSource.Token);
                        await _httpClient.Merge();
                    }
                    else
                    {
                        await _httpClient.Download(FileURL, FilePath, true, null, null, cancellationTokenSource.Token);
                    }

                    _httpClient.DownloadProgress -= GenericFilesDownloadProgress;

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        NeedRepairListUI.RemoveAt(0);
                    });

                    RepairedFilesCount++;
                }
                else
                {
                    using (RepairFileStream = RepairFileInfo.OpenWrite())
                    {
                        foreach (var chunk in block.BlockContent)
                        {
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                RepairStatus.Text = string.Format(Lang._GameRepairPage.Status10, block.BlockHash, chunk._startoffset.ToString("x8"), chunk._filesize.ToString("x8"));
                                RepairTotalStatus.Text = string.Format(Lang._GameRepairPage.PerProgressSubtitle2, RepairedFilesCount, BrokenFilesCount);
                                RepairTotalProgressBar.Value = GetPercentageNumber(RepairedFilesCount, BrokenFilesCount, 2);
                            });

                            using (MemoryStream RepairMemoryStream = new MemoryStream())
                            {
                                _httpClient.DownloadProgress += GenericFilesDownloadProgress;
                                await _httpClient.Download(FileURL, RepairMemoryStream,
                                    chunk._startoffset, chunk._startoffset + chunk._filesize, cancellationTokenSource.Token);
                                _httpClient.DownloadProgress -= GenericFilesDownloadProgress;

                                RepairFileStream.Position = chunk._startoffset;
                                await RepairFileStream.WriteAsync(RepairMemoryStream.GetBuffer(), 0,
                                    (int)chunk._filesize, cancellationTokenSource.Token);
                            }

                            DispatcherQueue.TryEnqueue(() =>
                            {
                                NeedRepairListUI.RemoveAt(0);
                            });

                            RepairedFilesCount++;
                        }
                    }
                }
            }
        }

        private void GenericFilesDownloadProgress(object sender, DownloadEvent e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                RepairPerFileStatus.Text = string.Format(Lang._Misc.Speed, SummarizeSizeSimple(e.Speed));
                RepairPerFileProgressBar.Value = Math.Round(UnInfinity(e.ProgressPercentage), 2);
            });
        }
    }
}
