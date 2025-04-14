using Blazorise;
using Blazorise.Charts;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syrna.QuartzAdmin.Blazor.Components;
using Syrna.QuartzAdmin.ExecutionLog;
using Syrna.QuartzAdmin.ExecutionLog.Dtos;
using Syrna.QuartzAdmin.Localization;
using Syrna.QuartzAdmin.Scheduler;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Blazor.Pages.QuartzAdmin.Overview
{
    public partial class Overview : IDisposable
    {
        [Inject] protected new IStringLocalizer<QuartzAdminResource> L { get; set; }
        const string UptimeKey = "Uptime";
        const string StatusKey = "Status";
        const string STARTED = "Started";
        const string STARTING = "Starting";
        const string STANDBY = "Standby";
        const string SHUTDOWN = "Shutdown";

        static double[] EmptyData = { 0, 0, 0, 0 };

        [Inject] IExecutionLogAppService LogSvc { get; set; } = null!;
        [Inject] ISchedulerAppService SchSvc { get; set; } = null!;

        private DataGrid<ExecutionLogDto> table = null!;
        private ExecutionLogFilter _errorExecutionLogFilter = new()
        {
            ErrorOnly = true,
            LogTypes = new HashSet<LogType> { LogType.ScheduleJob },
        };

        private DateTimeOffset? RunningSince;

        protected List<ExecutionLogDto> ErrorLogPagedList { get; set; }
        //private long _firstLogId;
        private OrderedDictionary SchedulerInfo = [];

        private bool IsPauseResumeDisabled;
        private bool IsStartStandbyDisabled;
        private bool IsStartButtonVisible;
        private bool IsShutdown;

        #region charts
        private int JobCount;
        private int TriggerCount;
        private int ExecutingCount;
        private int SysJobCount;
        private int SysTriggerCount;
        private int TotalLogDays { get; set; }
        protected int PageSize { get; set; } = 10;
        protected int ErrorLogTotalItems { get; set; } = 0;

        private string[] Labels;
        private List<string> borderColors = [
            ChartColor.FromRgba(75, 255, 192, 0.2f),
            ChartColor.FromRgba(255, 75, 132, 0.2f),
            ChartColor.FromRgba(54, 162, 235, 0.2f),
            ChartColor.FromRgba(255, 206, 86, 0.2f),
            ChartColor.FromRgba(153, 102, 255, 0.2f),
            ChartColor.FromRgba(255, 159, 64, 0.2f)
        ];
        private List<string> backgroundColors = [
            ChartColor.FromRgba(75, 255, 192, 1f),
            ChartColor.FromRgba(255, 75, 132, 1f),
            ChartColor.FromRgba(54, 162, 235, 1f),
            ChartColor.FromRgba(255, 206, 86, 1f),
            ChartColor.FromRgba(153, 102, 255, 1f),
            ChartColor.FromRgba(255, 159, 64, 1f) ];

        DoughnutChartOptions chartOptions = new()
        {
            AspectRatio = 1.5,
            Plugins = new ChartPlugins()
            {
                Tooltips = new ChartTooltips()
                {
                    Enabled = true,
                    UsePointStyle = true,
                    //Callbacks = new ChartTooltipCallbacks
                    //{
                    //	Title = (items) => "Custom title: " + items[0].Parsed,
                    //	Label = (item) => "Custom label: " + item.Parsed,
                    //}
                }
            }
        };

        Chart<double> allTimeChart = null!;

        private async Task HandleAllTimeChartRedraw()
        {
            await allTimeChart!.Clear();

            await allTimeChart!.AddLabelsDatasetsAndUpdate(Labels, GetAllTimeChartDataset());
        }

        private DoughnutChartDataset<double> GetAllTimeChartDataset()
        {
            return new()
            {
                Label = "# All Times",
                Data = AllTimeLogData.ToList(),
                BackgroundColor = backgroundColors,
                BorderColor = borderColors,
                BorderWidth = 1
            };
        }

        Chart<double> todaysChart = null!;
        private async Task HandleTodaysChartRedraw()
        {
            await todaysChart!.Clear();

            await todaysChart!.AddLabelsDatasetsAndUpdate(Labels, GetTodaysChartDataset());
        }

        private DoughnutChartDataset<double> GetTodaysChartDataset()
        {
            return new()
            {
                Label = "# Today",
                Data = TodaysLogData.ToList(),
                BackgroundColor = backgroundColors,
                BorderColor = borderColors,
                BorderWidth = 1
            };
        }

        protected Chart<double> yesterdaysChart = null!;
        private async Task HandleYesterdaysChartRedraw()
        {
            await yesterdaysChart!.Clear();

            await yesterdaysChart!.AddLabelsDatasetsAndUpdate(Labels, GetYesterdaysChartDataset());
        }

        private DoughnutChartDataset<double> GetYesterdaysChartDataset()
        {
            return new()
            {
                Label = "# Yesterday",
                Data = [.. YesterdaysLogData],
                BackgroundColor = backgroundColors,
                BorderColor = borderColors,
                BorderWidth = 1
            };
        }

        protected double[] TodaysLogData { get; set; } = EmptyData;

        protected double[] YesterdaysLogData { get; set; } = EmptyData;

        protected double[] AllTimeLogData { get; set; } = EmptyData;

        #endregion charts

        private Timer _refreshTimer;
        private Timer _trackTimer;
        private const int REFRESH_IN_MS = 10000;
        private bool AutoRefresh = true;

        public Overview()
        {
            LocalizationResource = typeof(QuartzAdminResource);
        }

        //protected override async Task OnInitializedAsync()
        //{
        //}

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Labels = [L["Success"], L["Failed"], L["Working"], L["Vetoed"]];
                await LoadInfo();
                await Task.WhenAll(RefreshErrorLogs(),
                RefreshSchedulesCount(),
                RefreshLogSummary(),
                LoadYesterdaysLogSummary());

                _trackTimer = new Timer(async (_) =>
                {
                    await InvokeAsync(async () =>
                    {
                        await RefreshStatus();

                        // Update the UI
                        StateHasChanged();
                    });
                }, null, REFRESH_IN_MS, REFRESH_IN_MS);

                _refreshTimer = new Timer(async (_) =>
                {
                    await InvokeAsync(async () =>
                    {
                        await Task.WhenAll(RefreshErrorLogs(),
                            RefreshSchedulesCount(),
                            RefreshLogSummary());

                        // Update the UI
                        StateHasChanged();
                    });
                }, null,
                IsPauseResumeDisabled ? Timeout.Infinite : REFRESH_IN_MS, REFRESH_IN_MS);
            }
        }

        private async Task OnSchedulerStarting()
        {
            await InvokeAsync(() =>
            {
                SchedulerInfo[StatusKey] = STARTING;
                IsStartStandbyDisabled = true;

                StateHasChanged();
            });
        }

        private async Task OnSchedulerStarted()
        {
            await InvokeAsync(async () =>
            {
                await Notify.Info(L["SchedulerStarted"]);
                IsStartStandbyDisabled = false;
                StartAutoRefresh();

                StateHasChanged();
            });
        }

        private async Task OnSchedulerShutdown()
        {
            await InvokeAsync(async () =>
            {
                await Notify.Info(L["SchedulerWasShutdown"]);
                StopAutoRefresh();

                StateHasChanged();
            });
        }

        private async Task OnSchedulerInStandbyMode()
        {
            await InvokeAsync(async () =>
            {
                await Notify.Info(L["SchedulerInStandbyMode"]);
                StopAutoRefresh();

                StateHasChanged();
            });

        }

        private async Task RefreshLogSummary()
        {
            var todayDateUtc = DateTime.Now.Date.ToUniversalTime();
            var today = await LogSvc.GetJobExecutionStatusSummary(new JobExecutionStatusSummaryReadArgs { StartTimeUtc = todayDateUtc });
            var allTime = await LogSvc.GetJobExecutionStatusSummary(new JobExecutionStatusSummaryReadArgs { EndTimeUtc = null });
            var nowDate = DateTimeOffset.Now.Date.ToUniversalTime();

            if (today.Data.Count == 0)
            {
                TodaysLogData = EmptyData;
            }
            else
            {
                var chartData = ConvertToChartData(today.Data);
                TodaysLogData = chartData.Item1;
            }

            if (nowDate != allTime.StartDateTimeUtc.Date)
            {
                await LoadYesterdaysLogSummary();
            }

            if (allTime.Data.Count == 0)
            {
                AllTimeLogData = EmptyData;
                TotalLogDays = 0;
            }
            else
            {
                var chartData = ConvertToChartData(allTime.Data);
                AllTimeLogData = chartData.Item1;

                TotalLogDays = (int)Math.Round(DateTime.Now.Subtract(
                    DateTime.SpecifyKind(allTime.StartDateTimeUtc, DateTimeKind.Utc).ToLocalTime()).TotalDays);
            }

            await InvokeAsync(StateHasChanged);

            await HandleTodaysChartRedraw();
            if (TotalLogDays > 1)
                await HandleAllTimeChartRedraw();
        }

        /// <summary>
        /// Yesterday's log summary. Separated from <see cref="RefreshLogSummary"/> since only need to
        /// call this when <see cref="lastCaptureDate"/> is different
        /// </summary>
        /// <returns></returns>
        private async Task LoadYesterdaysLogSummary()
        {
            var todayDateUtc = DateTime.Now.Date.ToUniversalTime();
            var yesterdayDateUtc = DateTime.Now.Date.AddDays(-1).ToUniversalTime();
            var yesterday = await LogSvc.GetJobExecutionStatusSummary(new JobExecutionStatusSummaryReadArgs { StartTimeUtc = yesterdayDateUtc, EndTimeUtc = todayDateUtc.AddMilliseconds(-1) });
            if (!yesterday.Data.Any())
            {
                YesterdaysLogData = EmptyData;
            }
            else
            {
                var chartData = ConvertToChartData(yesterday.Data);
                YesterdaysLogData = chartData.Item1;
            }
            await HandleYesterdaysChartRedraw();
        }

        private static (double[], string[]) ConvertToChartData(List<KeyValue<JobExecutionStatus, int>> data)
        {
            var values = new double[4];
            var labels = new string[4];
            var dict = data.ToDictionary(x => (int)x.Key, x => x.Value);
            //var names = Enum.GetNames<JobExecutionStatus>().ToList();
            for (var i = 0; i < 4; i++)
            {
                var name = Enum.GetName(typeof(JobExecutionStatus), i);
                if (!dict.ContainsKey(i))
                {
                    values[i] = 0;
                    labels[i] = $"{name} (0)";
                    continue;
                }
                var entry = dict[i];
                values[i] = entry;
                labels[i] = $"{name} ({entry})";
            }

            return (values, labels);
        }

        private async Task RefreshErrorLogs()
        {
            PageMetadata pageMeta;
            var state = await table.GetState();
            if (ErrorLogPagedList == null)
            {
                pageMeta = PageMetadata.New(0, state.PageSize);
            }
            else
            {
                pageMeta = new PageMetadata { Page = state.CurrentPage - 1, PageSize = state.PageSize };
            }
            var args = new ExecutionLogReadArgs
            {
                Filter = _errorExecutionLogFilter,
                PageMetadata = pageMeta,
                //FirstLogId = _firstLogId
            };
            var data = await LogSvc.GetExecutionLogs(args);
            ErrorLogPagedList = [.. data.Items];
            //if (pageMeta.Page == 0)
            //{
            //    _firstLogId = ErrorLogPagedList.FirstOrDefault()?.Id ?? 0;
            //}

            ErrorLogTotalItems = (int)data.TotalCount;
        }

        private async Task RefreshSchedulesCount()
        {
            var items = await SchSvc.GetScheduledJobSummary();
            foreach (var item in items)
            {
                switch (item.Key)
                {
                    case "Jobs":
                        JobCount = item.Value;
                        break;
                    case "Triggers":
                        TriggerCount = item.Value;
                        break;
                    case "Executing":
                        ExecutingCount = item.Value;
                        break;
                    case "System Jobs":
                        SysJobCount = item.Value;
                        break;
                    case "System Triggers":
                        SysTriggerCount = item.Value;
                        break;
                }
            }
            JobCount -= SysJobCount;
            TriggerCount -= SysTriggerCount;
        }
        public TimeSpan Uptime { get; set; } = TimeSpan.Zero;
        private async Task RefreshStatus()
        {
            Uptime = RunningSince.HasValue ? DateTimeOffset.UtcNow.Subtract(RunningSince.Value) : TimeSpan.Zero;
            SchedulerInfo[UptimeKey] = Uptime.ToHumanTimeString();
            if (!metadataLoaded)
            {
                await LoadInfo();
            }
            var metadata = await SchSvc.GetMetadataAsync();
            if (metadata == null)
            {
                return;
            }
            var newStatus = metadata.Shutdown ? SHUTDOWN : metadata.InStandbyMode ? STANDBY : metadata.Started ? STARTED : "Unknown";
            if (newStatus != Status)
            {
                Status = newStatus;
                await OnStatusChanged(newStatus);
            }

            IsStartButtonVisible = metadata.InStandbyMode || metadata.Shutdown;
            IsShutdown = metadata.Shutdown;
            IsPauseResumeDisabled = IsStartButtonVisible || IsShutdown;
        }

        public string Status { get; set; }
        Background GetStatusColor(string status)
        {
            return status switch
            {
                "Started" => Background.Success,
                "Shutdown" => Background.Danger,
                "Standby" => Background.Warning,
                "Starting" => Background.Secondary,
                _ => Background.Dark
            };
        }
        async Task OnStatusChanged(string status)
        {
            switch (status)
            {
                case "Started":
                    await OnSchedulerStarted();
                    break;
                case "Shutdown":
                    await OnSchedulerShutdown();
                    break;
                case "Standby":
                    await OnSchedulerInStandbyMode();
                    break;
                case "Starting":
                    await OnSchedulerStarting();
                    break;
                default:
                    break;
            }
        }

        private bool metadataLoaded = false;
        private async Task LoadInfo()
        {
            var metadata = await SchSvc.GetMetadataAsync();
            if (metadata == null)
            {
                return;
            }
            metadataLoaded = true;
            SchedulerInfo.Clear();
            var newStatus = metadata.Shutdown ? SHUTDOWN : metadata.InStandbyMode ? STANDBY : metadata.Started ? STARTED : "Unknown";
            if (newStatus != Status)
            {
                Status = newStatus;
                await OnStatusChanged(newStatus);
            }

            IsStartButtonVisible = metadata.InStandbyMode || metadata.Shutdown;
            IsShutdown = metadata.Shutdown;
            IsPauseResumeDisabled = IsStartButtonVisible || IsShutdown;

            RunningSince = metadata.RunningSince;
            SchedulerInfo.Add("QuartzVersion", metadata.Version);
            SchedulerInfo.Add("QuartzAdminVersion", typeof(Overview).Assembly.GetName().Version);
            SchedulerInfo.Add(StatusKey, Status);
            SchedulerInfo.Add(UptimeKey, RunningSince.HasValue ?
                DateTimeOffset.UtcNow.Subtract(RunningSince.Value).ToHumanTimeString() : "--");
            SchedulerInfo.Add("SchedulerInstanceId", metadata.SchedulerInstanceId);
            SchedulerInfo.Add("SchedulerName", metadata.SchedulerName);
            SchedulerInfo.Add("SchedulerRemote", metadata.SchedulerRemote ? "Yes" : "No");
            SchedulerInfo.Add("SchedulerType", metadata.SchedulerTypeName);
            SchedulerInfo.Add("JobStoreType", metadata.JobStoreTypeName);
            SchedulerInfo.Add("SupportPersistence", metadata.JobStoreSupportsPersistence ? "Yes" : "No");
            SchedulerInfo.Add("Clustered", metadata.JobStoreClustered ? "Yes" : "No");
            SchedulerInfo.Add("ThreadPoolSize", metadata.ThreadPoolSize);
            SchedulerInfo.Add("ThreadPoolType", metadata.ThreadPoolTypeName);
        }

        ExecutionDetailsDialog ExecutionDetailsDialogRef;
        private async Task OnMoreDetails(ExecutionLogDto log, string titleSuffix)
        {
            await ExecutionDetailsDialogRef.OpenModalAsync(log, titleSuffix);
        }

        #region Action buttons
        private async Task OnStartScheduler()
        {
            await SchSvc.StartScheduler();
        }

        private async Task OnStandbyScheduler()
        {
            await SchSvc.StandbyScheduler();
        }

        private async Task OnShutdownScheduler()
        {
            await SchSvc.ShutdownScheduler();
        }

        private async Task OnPauseAllSchedules()
        {
            try
            {
                await SchSvc.PauseAllSchedules();
                await Notify.Info(L["PauseAllSchedules"]);
            }
            catch (Exception ex)
            {
                await Notify.Error(string.Format(L["Error:PauseAllSchedules"], ex.Message));
            }
        }

        private async Task OnResumeAllSchedules()
        {
            try
            {
                await SchSvc.ResumeAllSchedules();
                await Notify.Info(L["ResumeAllSchedules"]);
            }
            catch (Exception ex)
            {
                await Notify.Error(string.Format(L["Error:ResumeAllSchedules"], ex.Message));
            }
        }
        #endregion Action buttons

        #region Auto refresh
        private void StartAutoRefresh()
        {
            _refreshTimer?.Change(0, REFRESH_IN_MS);
        }

        private void StopAutoRefresh()
        {
            _refreshTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void OnCheckAutoRefresh(bool flag)
        {
            AutoRefresh = flag;
            if (AutoRefresh)
            {
                StartAutoRefresh();
            }
            else
            {
                StopAutoRefresh();
            }
        }
        #endregion Auto refresh

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //modalRef?.Dispose();
                _trackTimer?.Dispose();
                _refreshTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

