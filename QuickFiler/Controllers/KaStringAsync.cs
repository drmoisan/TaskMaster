using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers
{
    public class KaStringAsync : IKbdAction<string, Func<string, Task>>
    {
        public KaStringAsync() { }

        public KaStringAsync(
            string sourceId,
            string key,
            Func<string, Task> function,
            Action<string> update,
            System.Action toggleControl
        )
        {
            SourceId = sourceId;
            Key = key.ToLower();
            Delegate = function;
            Update = update;
            ToggleControl = toggleControl;
        }

        private string _sourceId;
        public string SourceId
        {
            get => _sourceId;
            set => _sourceId = value;
        }

        private string _key;
        public string Key
        {
            get => _key;
            set => _key = value.ToLower();
        }

        private Func<string, Task> _function;
        public Func<string, Task> Delegate
        {
            get => _function;
            set => _function = value;
        }

        private bool _activated = false;
        public bool Activated
        {
            get => _activated;
            set => _activated = value;
        }

        /// <summary>
        /// Tests whether this action's <see cref="Key"/> matches a keyboard filter probe, and fires
        /// this action's gated side effects.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Latch contract.</b> <c>Activated</c> is a per-keystroke latch that gates every
        /// observable side effect of <c>KeyEquals</c> — both <c>Update</c> and <c>ToggleControl</c>.
        /// A <b>matching</b> probe (branch 1) deliberately does not clear the latch and returns
        /// early, so a matching element's <c>Update</c> continues to fire on each pass
        /// <c>KeyboardHandler</c> makes within one keystroke; that repetition is intentional and is
        /// what advances the item-number label. A <b>non-matching</b> probe (branches 2 and 3)
        /// clears the latch, so a non-matching element's side effects fire at most once per
        /// keystroke regardless of how many times a LINQ predicate is re-enumerated.
        /// </para>
        /// <para>
        /// Branch 1's early return is therefore load-bearing and must not be "completed" into a
        /// fall-through to the trailing latch reset for symmetry. <c>KeyboardHandler</c> re-arms the
        /// latch only at filter length 1 and then makes three passes within one keystroke; if a
        /// matching probe cleared the latch, the first pass would consume the activation and the
        /// item-number label would stop advancing.
        /// </para>
        /// <para>
        /// <b>Argument contract.</b> <paramref name="other"/> must be non-null and non-empty. The
        /// guard clause at the top of this method rejects both fail-fast, so branch 1's substring
        /// offset expression is never evaluated with a negative start index.
        /// </para>
        /// <para>
        /// <b>Consequence for callers.</b> <c>KbdActions</c> methods whose key type is
        /// <c>string</c> — <c>ContainsKey</c>, <c>FilterKeys</c>, <c>Find</c>, <c>FindIndex</c>, and
        /// the indexer — inherit this new precondition: an empty key argument now surfaces an
        /// <c>ArgumentException</c> from this predicate rather than matching every element.
        /// </para>
        /// </remarks>
        /// <param name="other">
        /// The keyboard filter probe to compare against <see cref="Key"/>. Must be non-null and
        /// non-empty.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when <see cref="Key"/> contains <paramref name="other"/> as a
        /// substring; otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="other"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="other"/> is empty. An empty probe would otherwise match every registered
        /// action, because <c>string.Contains(string.Empty)</c> is true for every receiver.
        /// </exception>
        public bool KeyEquals(string other)
        {
            // The null test must come first: other.Length on a null reference throws
            // NullReferenceException before any later guard could run.
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (other.Length == 0)
            {
                throw new ArgumentException(
                    "An empty probe is not a valid key. string.Contains(string.Empty) is true for "
                        + "every receiver, so an empty probe would otherwise match every registered "
                        + "action rather than none.",
                    nameof(other)
                );
            }

            if (Key.Contains(other))
            {
                if (Activated && Update is not null)
                    Update(Key.Substring(other.Length - 1, 1));
                return true;
            }
            else if (other.Length == 1)
            {
                if (Activated && ToggleControl is not null)
                    ToggleControl();
            }
            else if (other.Length > 1)
            {
                if (Activated && Update is not null)
                    Update(Key.Substring(0, 1));
                if (Activated && ToggleControl is not null)
                    ToggleControl();
            }
            Activated = false;
            return false;
        }

        private Action<string> _update;
        public Action<string> Update
        {
            get => _update;
            set => _update = value;
        }

        private System.Action _toggleControl;
        public System.Action ToggleControl
        {
            get => _toggleControl;
            set => _toggleControl = value;
        }
    }
}
