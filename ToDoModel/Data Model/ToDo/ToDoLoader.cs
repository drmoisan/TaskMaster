using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;

namespace ToDoModel.Data_Model.ToDo
{
    internal class ToDoLoader
    {
        public ToDoLoader(System.Action olSaver, Func<bool>isReadOnly)
        {
            //() => FlaggableItem.Save() is olSaver
            OlSaver = olSaver;
            _isReadOnly = isReadOnly;
        }

        private System.Action _olSaver;
        internal System.Action OlSaver { get => _olSaver; set => _olSaver = value; }

        protected Func<bool> _isReadOnly;
        private bool _readonly { get => _isReadOnly(); }


        internal void SetAndSave<T>(ref T variable, T value, Action<T> objectSetter)
        {
            SetAndSave(ref variable, value, objectSetter, OlSaver);
        }

        /// <summary>
        /// Sets the value of a local private variable. If the item is not readonly, it also
        /// sets the value of the corresponding property in the <seealso cref="OutlookItem"/> object"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable">Private variable caching the value</param>
        /// <param name="value">Value to be saved</param>
        /// <param name="objectSetter">Action that sets an object property to the value</param>
        /// <param name="objectSaver">Action to save the object</param>
        /// <exception cref="ArgumentNullException"></exception>
        internal void SetAndSave<T>(ref T variable, T value, Action<T> objectSetter, System.Action objectSaver)
        {
            variable = value;
            if (!_readonly)
            {
                if (objectSetter is null) { throw new ArgumentNullException($"Method {nameof(SetAndSave)} failed because {nameof(objectSetter)} was passed as null"); }
                objectSetter(value);
                if (objectSaver is not null) { objectSaver(); }
            }
        }

        /// <summary>
        /// Sets the value of an <seealso cref="OutlookItem"/> property using a delegate. 
        /// Value is not cached in a local variable in this overload. <seealso cref="OutlookItem.Save()"/> is called
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Value to be saved</param>
        /// <param name="objectSetter">Action that sets an object property to the value</param>
        internal void SetAndSave<T>(T value, Action<T> objectSetter)
        {
            SetAndSave(value, objectSetter, OlSaver);
        }

        internal void SetAndSave<T>(T value, Action<T> objectSetter, System.Action objectSaver)
        {
            if (!_readonly)
            {
                if (objectSetter is null) { throw new ArgumentNullException($"Method {nameof(SetAndSave)} failed because {nameof(objectSetter)} was passed as null"); }
                objectSetter(value);
                if (objectSaver is not null) { objectSaver(); }
            }
        }

        internal T GetOrLoad<T>(ref T value, Func<T> loader)
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T))) { value = loader(); }
            return value;
        }

        internal T GetOrLoad<T>(ref T value, Func<T> loader, params object[] dependencies)
        {
            if (dependencies is null) { throw new ArgumentNullException($"Method {nameof(GetOrLoad)} failed the dependency check because {nameof(dependencies)} was passed as a null array"); }
            if (dependencies.Any(x => x is null))
            {
                var errors = dependencies.FindIndices(x => x is null).Select(x => x.ToString()).ToArray().SentenceJoin();
                throw new ArgumentNullException($"Method {nameof(GetOrLoad)} failed the dependency check because {nameof(dependencies)} contains a null value at position {errors}");
            }
            return GetOrLoad(ref value, loader);
        }

        internal T GetOrLoad<T>(ref T value, T defaultValue, Func<T> loader, params object[] dependencies)
        {
            if (dependencies is null || dependencies.Any(x => x is null))
            {
                value = defaultValue;
                return value;
            }
            else
            {
                try
                {
                    if (EqualityComparer<T>.Default.Equals(value, default(T))) { value = loader(); }
                    if (EqualityComparer<T>.Default.Equals(value, default(T))) { value = defaultValue; }
                }
                catch (System.Exception)
                {
                    value = defaultValue;
                }

                return value;
            }
        }

        internal T GetOrLoad<T>(ref T value, T defaultValue, Func<T> loader, Action<T> defaultSetAndSaver, params object[] dependencies)
        {
            if (dependencies is null || dependencies.Any(x => x is null))
            {
                value = defaultValue;
                return value;
            }
            else
            {
                try
                {
                    if (EqualityComparer<T>.Default.Equals(value, default(T))) { value = loader(); }
                    if (EqualityComparer<T>.Default.Equals(value, default(T)))
                    {
                        value = defaultValue;
                        defaultSetAndSaver(value);
                    }
                }
                catch (System.Exception)
                {
                    value = defaultValue;
                    defaultSetAndSaver(value);
                }

                return value;
            }
        }

        internal T Load<T>(Func<T> loader, params object[] dependencies)
        {
            if (dependencies is null) { throw new ArgumentNullException($"Method {nameof(GetOrLoad)} failed the dependency check because {nameof(dependencies)} was passed as a null array"); }
            if (dependencies.Any(x => x is null))
            {
                var errors = dependencies.FindIndices(x => x is null).Select(x => x.ToString()).ToArray().SentenceJoin();
                throw new ArgumentNullException($"Method {nameof(GetOrLoad)} failed the dependency check because {nameof(dependencies)} contains a null value at position {errors}");
            }
            return loader();
        }

        internal T Load<T>(Func<T> loader, T defaultValue, params object[] dependencies)
        {
            if (dependencies is null || dependencies.Any(x => x is null)) { return defaultValue; }
            else { return loader(); }
        }

    }
}
