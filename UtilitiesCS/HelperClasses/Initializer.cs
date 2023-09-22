using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public static class Initializer
    {
        /// <summary>
        /// Sets the value of a private variable passed as reference. It also sets the value of the corresponding 
        /// property in an underlying object such as but not limited to <seealso cref="OutlookItem"/>. A condition
        /// function may be passed to determine whether to write to the underlying object"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable">Private variable caching the value</param>
        /// <param name="value">Value to be saved</param>
        /// <param name="objectSetter">Action that sets an object property to the value</param>
        /// <param name="objectSaver">Action to save the object</param>
        /// <param name="strict">If true throws exception, else it skips execution</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SetAndSave<T>(ref T variable, T value, Action<T> objectSetter, System.Action objectSaver, Func<bool> condition, bool strict)
        {
            variable = value;
            SetAndSave(value, objectSetter, objectSaver, condition, strict);
        }

        public static void SetAndSave<T>(ref T variable, T value, Action<T> objectSetter, Func<bool> condition, bool strict)
        {
            variable = value;
            SetAndSave(value, objectSetter, condition, strict);
        }

        public static void SetAndSave<T>(ref T variable, T value, Action<T> objectSetter)
        {
            variable = value;
            SetAndSave(value, objectSetter);
        }

        public static void SetAndSave<T>(T value, Action<T> objectSetter, System.Action objectSaver, Func<bool> condition, bool strict)
        {
            if (!DependenciesNotNull(strict, condition) || condition())
            {
                if (DependenciesNotNull(strict, objectSetter)) { objectSetter(value); }
                if (DependenciesNotNull(strict, objectSaver)) { objectSaver(); }
            }
        }

        public static void SetAndSave<T>(T value, Action<T> objectSetter, Func<bool> condition, bool strict)
        {
            if (!DependenciesNotNull(strict, condition) || condition())
            {
                if (DependenciesNotNull(strict, objectSetter)) { objectSetter(value); }
            }
        }

        public static void SetAndSave<T>(T value, Action<T> objectSetter)
        {
            if (DependenciesNotNull(false, objectSetter)) { objectSetter(value); }
        }

        public static T GetOrLoad<T>(ref T variable, Func<T> loader)
        {
            if (EqualityComparer<T>.Default.Equals(variable, default(T))) { variable = loader(); }
            return variable;
        }

        public static T GetOrLoad<T>(ref T variable, Func<T> loader, bool strict, params object[] dependencies)
        {
            if (DependenciesNotNull(strict, dependencies)) { return GetOrLoad(ref variable, loader); }
            else { return default(T); }
        }

        public static T GetOrLoad<T>(ref T variable, T defaultValue, Func<T> loader, params object[] dependencies)
        {
            if (!DependenciesNotNull(false, dependencies))
            {
                variable = defaultValue;
                return variable;
            }
            else
            {
                try
                {
                    if (EqualityComparer<T>.Default.Equals(variable, default(T))) { variable = loader(); }
                    if (EqualityComparer<T>.Default.Equals(variable, default(T))) { variable = defaultValue; }
                }
                catch (System.Exception)
                {
                    variable = defaultValue;
                }

                return variable;
            }
        }

        public static T GetOrLoad<T>(ref T variable, T defaultValue, Func<T> loader, Action<T> defaultSetAndSaver, params object[] dependencies)
        {
            if (!DependenciesNotNull(false, dependencies))
            {
                variable = defaultValue;
                return variable;
            }
            else
            {
                try
                {
                    // If no value is set try to load it
                    if (EqualityComparer<T>.Default.Equals(variable, default(T))) { variable = loader(); }
                    // Repeat check in case the loader returned default. In that case load defaultValue
                    if (EqualityComparer<T>.Default.Equals(variable, default(T)))
                    {
                        variable = defaultValue;
                        defaultSetAndSaver(variable); // function is never reached if it was passed as null
                    }
                }
                // If the loader throws an exception, load defaultValue
                catch (System.Exception)
                {
                    variable = defaultValue;
                    defaultSetAndSaver(variable); // function is never reached if it was passed as null
                }

                return variable;
            }
        }

        public static T Load<T>(Func<T> loader, bool strict, params object[] dependencies)
        {
            if (DependenciesNotNull(strict, dependencies)) { return loader(); }
            else { return default(T); }
        }

        public static T Load<T>(Func<T> loader, T defaultValue, params object[] dependencies)
        {
            if (!DependenciesNotNull(false, dependencies)) { return defaultValue; }
            else { return loader(); }
        }

        public static bool DependenciesNotNull(bool strict, params object[] dependencies)
        {
            if (dependencies is null) 
            { 
                var caller = new StackFrame(1, false).GetMethod().Name;
                var message = $"Method {caller} failed the dependency check because {nameof(dependencies)} " +
                    "was passed as a null array";
                return strict ? throw new ArgumentNullException(nameof(dependencies), message) : false;
            }
            if (dependencies.Count() == 0)
            {
                var caller = new StackFrame(1, false).GetMethod().Name;
                var message = $"Method {caller} failed the dependency check because {nameof(dependencies)} " +
                    "was empty";
                return strict ? throw new ArgumentNullException(message) : false;
            }
            if (dependencies.Any(x => x is null))
            {
                var errors = dependencies.FindIndices(x => x is null).Select(x => x.ToString()).ToArray().SentenceJoin();
                var caller = new StackFrame(1, false).GetMethod().Name;
                var message = $"Method {caller} failed the dependency check because {nameof(dependencies)} " +
                    $"contains a null value at position {errors}";
                return strict ? throw new ArgumentNullException(message) : false;
            }
            return true;
        }
    }
}
