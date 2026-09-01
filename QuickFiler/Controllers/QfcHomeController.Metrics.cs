using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    public partial class QfcHomeController
    {
        /// <summary>
        /// Injectable time/delay seam. Defaults to <see cref="TimeProvider.System"/> so production
        /// timestamps and delays are unchanged; tests assign a mock/fake provider to make
        /// time-dependent output and async delays deterministic.
        /// </summary>
        internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

        /// <summary>
        /// Injectable seam for the session-metrics file write. Parameter order is filename, lines,
        /// folder root, cancellation token. Defaults to <see cref="FileIO2.WriteTextFileAsync"/> so
        /// production behaviour is unchanged; tests assign a capturing delegate to observe the
        /// flush without touching the filesystem. Mirrors the EmailFiler precedent at
        /// <c>EfcHomeControllerDependencies.cs:78</c>.
        /// </summary>
        internal Func<
            string,
            string[],
            string,
            CancellationToken,
            Task<bool>
        > MetricsFileWriter { get; set; } = FileIO2.WriteTextFileAsync;

        public void QuickFileMetrics_WRITE(string filename)
        {
            string durationText,
                durationMinutesText;

            string dataLineBeg;

            // Create a line of comma seperated valued to store data
            var now = TimeProvider.GetLocalNow().LocalDateTime;
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

            double duration = _stopWatchMoved.Elapsed.TotalSeconds;
            var endTime = now;
            var startTime = endTime.Subtract(_stopWatchMoved.Elapsed);

            var emailsLoaded = _formController.Groups.EmailsToMove;

            if (emailsLoaded > 0)
            {
                duration /= emailsLoaded;
            }

            durationText = duration.ToString("##0", CultureInfo.InvariantCulture);
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable durationText = " & durationText

            durationMinutesText = (duration / 60d).ToString("##0.00", CultureInfo.InvariantCulture);

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
            var now = TimeProvider.GetLocalNow().LocalDateTime;
            curDateText = now.ToString("MM/dd/yyyy");

            curTimeText = now.ToString("hh:mm");

            dataLineBeg = curDateText + "," + curTimeText + ",";

            if (!Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var myDocuments))
            {
                return;
            }
            LOC_TXT_FILE = Path.Combine(myDocuments, filename);

            Duration = _stopWatchMoved.Elapsed.TotalSeconds;
            OlEndTime = now;
            // Subtract the measured interval directly rather than reconstructing it from a
            // truncated integer cast, so the calendar span agrees with the reported duration.
            OlStartTime = OlEndTime.Subtract(_stopWatchMoved.Elapsed);

            var emailsLoaded = _formController.Groups.EmailsToMove;

            if (emailsLoaded > 0)
            {
                Duration /= emailsLoaded;
            }

            durationText = Duration.ToString("##0", CultureInfo.InvariantCulture);
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable durationText = " & durationText

            durationMinutesText = (Duration / 60d).ToString("##0.00", CultureInfo.InvariantCulture);
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

            // The call is made through IQfcCollectionController.GetMoveDiagnostics, which carries
            // no XML documentation and therefore no non-null element guarantee, so this filter
            // defends the interface contract rather than a known producer defect.
            var lines = strOutput.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            if (lines.Length == 0)
            {
                return;
            }

            // CancellationToken.None, never the session Token: the dispatcher continuation that
            // carries this write is not awaited to completion, so a session cancellation can be
            // raised while the write is in flight and must not destroy the metrics.
            bool metricsWritten = await MetricsFileWriter(
                filename,
                lines,
                myDocuments,
                CancellationToken.None
            );
            if (!metricsWritten)
            {
                logger.Error(
                    $"Session metrics were not written to {LOC_TXT_FILE}. The writer exhausted its "
                        + "retry budget or failed after opening the file."
                );
            }
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
    }
}
