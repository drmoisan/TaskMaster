using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    public partial class QfcHomeController
    {
        public void QuickFileMetrics_WRITE(string filename)
        {
            string durationText,
                durationMinutesText;

            string dataLineBeg;

            // Create a line of comma seperated valued to store data
            var now = DateTime.Now;
            //var curDateText = DateTime.Now.ToString("MM/dd/yyyy");
            //var curTimeText = DateTime.Now.ToString("hh:mm");
            //dataLineBeg = curDateText + "," + curTimeText + ",";
            dataLineBeg = $"{now:MM/dd/yyyy},{now:hh:mm},";

            if (!Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var folderRoot))
            {
                logger.Debug(
                    $"{nameof(QuickFileMetrics_WRITE)} aborted due to lack of MyDocuments location"
                );
                return;
            }
            var filepath = Path.Combine(folderRoot, filename);

            double duration = _stopWatchMoved.Elapsed.Seconds;
            var endTime = now;
            var startTime = endTime.Subtract(_stopWatchMoved.Elapsed);

            var emailsLoaded = _formController.Groups.EmailsToMove;

            if (emailsLoaded > 0)
            {
                duration /= emailsLoaded;
            }

            durationText = duration.ToString("##0");
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable durationText = " & durationText

            durationMinutesText = (duration / 60d).ToString("##0.00");

            var olEmailCalendar = UtilitiesCS.Calendar.GetCalendar(
                "Email Time",
                Globals.Ol.App.Session
            );
            AppointmentItem olAppointment = null;
            if (olEmailCalendar is not null)
            {
                olAppointment = (AppointmentItem)olEmailCalendar.Items.Add();
                olAppointment.Subject = $"Quick Filed {emailsLoaded} emails";
                olAppointment.Start = startTime;
                olAppointment.End = endTime;
                olAppointment.Categories = "@ Email";
                olAppointment.ReminderSet = false;
                olAppointment.Sensitivity = OlSensitivity.olPrivate;
                olAppointment.Save();
            }

            string[] strOutput = _formController.Groups.GetMoveDiagnostics(
                durationText,
                durationMinutesText,
                duration,
                dataLineBeg,
                endTime,
                ref olAppointment
            );

            if (Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var myDocuments))
            {
                FileIO2.WriteTextFile(filename, strOutput, myDocuments);
            }
        }

        public async Task WriteMetricsAsync(string filename)
        {
            //TraceUtility.LogMethodCall(filename);

            string LOC_TXT_FILE;
            string curDateText,
                curTimeText,
                durationText,
                durationMinutesText;
            double Duration;
            string dataLineBeg;
            DateTime OlEndTime;
            DateTime OlStartTime;
            AppointmentItem OlAppointment;
            Folder OlEmailCalendar;

            // Create a line of comma seperated valued to store data
            curDateText = DateTime.Now.ToString("MM/dd/yyyy");

            curTimeText = DateTime.Now.ToString("hh:mm");

            dataLineBeg = curDateText + "," + curTimeText + ",";

            if (!Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var myDocuments))
            {
                return;
            }
            LOC_TXT_FILE = Path.Combine(myDocuments, filename);

            //Duration = _stopWatchMoved.Elapsed.Seconds;
            Duration = StopWatch.Elapsed.Seconds;
            OlEndTime = DateTime.Now;
            OlStartTime = OlEndTime.Subtract(new TimeSpan(0, 0, 0, (int)Duration));

            var emailsLoaded = _formController.Groups.EmailsToMove;

            if (emailsLoaded > 0)
            {
                Duration /= emailsLoaded;
            }

            durationText = Duration.ToString("##0");
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable durationText = " & durationText

            durationMinutesText = (Duration / 60d).ToString("##0.00");
            WriteMoveToCalendar(
                OlEndTime,
                OlStartTime,
                emailsLoaded,
                out OlAppointment,
                out OlEmailCalendar
            );

            string[] strOutput = _formController.Groups.GetMoveDiagnostics(
                durationText,
                durationMinutesText,
                Duration,
                dataLineBeg,
                OlEndTime,
                ref OlAppointment
            );

            _fileName = filename;
            await NonBlockingProducer(strOutput, Token);
        }

        private void WriteMoveToCalendar(
            DateTime OlEndTime,
            DateTime OlStartTime,
            int emailsLoaded,
            out AppointmentItem OlAppointment,
            out Folder OlEmailCalendar
        )
        {
            //TraceUtility.LogMethodCall(OlEndTime, OlStartTime, emailsLoaded);

            OlEmailCalendar = UtilitiesCS.Calendar.GetCalendar(
                "Email Time",
                Globals.Ol.App.Session
            );
            if (OlEmailCalendar is null)
            {
                OlAppointment = null;
            }
            else
            {
                OlAppointment = (AppointmentItem)OlEmailCalendar.Items.Add();
                {
                    OlAppointment.Subject = $"Quick Filed {emailsLoaded} emails";
                    OlAppointment.Start = OlStartTime;
                    OlAppointment.End = OlEndTime;
                    OlAppointment.Categories = "@ Email";
                    OlAppointment.ReminderSet = false;
                    OlAppointment.Sensitivity = OlSensitivity.olPrivate;
                    OlAppointment.Save();
                }
            }
        }

        private async Task NonBlockingProducer(string[] lines, CancellationToken ct)
        {
            //TraceUtility.LogMethodCall(lines, ct);

            foreach (string line in lines)
            {
                ct.ThrowIfCancellationRequested();
                await NonBlockingProducer(line, ct);
            }
        }

        private async Task NonBlockingProducer(string line, CancellationToken ct)
        {
            bool success = false;

            do
            {
                // Cancellation causes OCE. We know how to handle it.
                try
                {
                    // A shorter timeout causes more failures.
                    success = _metrics.TryAdd(line, 20, ct);
                }
                catch (OperationCanceledException)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }
                    else
                    {
                        //logger.Debug($"Timeout adding {line}");
                        await Task.Delay(20);
                    }
                }
            } while (!success);
            if (Interlocked.CompareExchange(ref _metricsConsumers, 0, 2) == 2)
            {
                Interlocked.Decrement(ref _metricsConsumers);
                var timer = new System.Timers.Timer(2000);
                timer.Elapsed += TimedConsumerAsync;
            }
        }
    }
}
