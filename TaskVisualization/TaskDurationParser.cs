using System;

namespace TaskVisualization
{
    /// <summary>
    /// Host-neutral parser for the task-duration text box. Reproduces the exact
    /// observable semantics of the controller's former inline <c>CaptureDuration</c>
    /// parse/validate logic, isolated from WinForms and Outlook Interop so it can be
    /// unit-tested directly.
    /// </summary>
    /// <remarks>
    /// Behavior (preserved exactly):
    /// <list type="bullet">
    /// <item>A non-negative integer (including zero) parses successfully.</item>
    /// <item>A negative integer returns a not-ok result carrying the message of
    /// <see cref="ArgumentOutOfRangeException"/> ("Duration cannot be negative"),
    /// exactly as the former negative-path <c>MessageBox</c> displayed.</item>
    /// <item>A non-integer, empty, or whitespace input lets <see cref="int.Parse(string)"/>
    /// throw <see cref="FormatException"/>, and an overflow lets it throw
    /// <see cref="OverflowException"/> — both propagate uncaught, exactly as the
    /// former code (whose <c>catch (InvalidCastException)</c> branch was dead, since
    /// <see cref="int.Parse(string)"/> never throws <see cref="InvalidCastException"/>).</item>
    /// </list>
    /// The dead <c>catch (InvalidCastException)</c> branch is intentionally not
    /// reproduced; dropping unreachable code preserves observable behavior.
    /// </remarks>
    public static class TaskDurationParser
    {
        /// <summary>
        /// Parses the supplied duration text.
        /// </summary>
        /// <param name="durationText">The raw duration text from the viewer.</param>
        /// <returns>
        /// A tuple where <c>ok</c> is true and <c>minutes</c> holds the parsed value for a
        /// non-negative integer; <c>ok</c> is false with a populated <c>error</c> message
        /// for a negative integer. <c>error</c> is empty on the ok path.
        /// </returns>
        /// <exception cref="FormatException">
        /// Thrown (propagated from <see cref="int.Parse(string)"/>) when the input is not a
        /// valid integer, is empty, or is whitespace.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown (propagated from <see cref="int.Parse(string)"/>) when the input overflows
        /// <see cref="int"/>.
        /// </exception>
        public static (bool ok, int minutes, string error) Parse(string durationText)
        {
            int duration = int.Parse(durationText);
            if (duration < 0)
            {
                return (
                    false,
                    0,
                    new ArgumentOutOfRangeException("Duration cannot be negative").Message
                );
            }

            return (true, duration, string.Empty);
        }
    }
}
