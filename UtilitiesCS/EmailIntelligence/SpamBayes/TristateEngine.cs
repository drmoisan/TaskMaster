using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.EmailIntelligence
{
    public abstract class TristateEngine()
    {
        /// <summary>
        /// Delegate Function that extracts an array of string tokens from an object
        /// </summary>
        public Func<object, string[]> Tokenize { get => _tokenize; set => _tokenize = value; }
        private Func<object, string[]> _tokenize;

        /// <summary>
        /// Async Delegate Function that extracts an array of string tokens from an object
        /// </summary>
        public Func<object, Task<string[]>> TokenizeAsync { get => _tokenizeAsync; set => _tokenizeAsync = value; }
        private Func<object, Task<string[]>> _tokenizeAsync;

        /// <summary>
        /// Delegate Function that calculates the probability that an array of string tokens belongs to a class
        /// </summary>
        public Func<string[], double> CalculateProbability { get => _calculateProbability; set => _calculateProbability = value; }
        private Func<string[], double> _calculateProbability;

        /// <summary>
        /// Async Delegate Function that calculates the probability that an array of string tokens belongs to a class
        /// </summary>
        public Func<string[], Task<double>> CalculateProbabilityAsync { get => _calculateProbabilityAsync; set => _calculateProbabilityAsync = value; }
        private Func<string[], Task<double>> _calculateProbabilityAsync;

        /// <summary>
        /// Delegate Function that maps a probability to one of three states true, false, or null. 
        /// True means the probability belongs to the class, false means it does not, and null means the 
        /// probability is inconclusive.
        /// </summary>
        //public Func<double, bool?> GetTristate { get => _getTristate; set => _getTristate = value; }
        //private Func<double, bool?> _getTristate;

        /// <summary>
        /// Async Delegate Function that maps a probability to one of three states true, false, or null. 
        /// True means the probability belongs to the class, false means it does not, and null means the 
        /// probability is inconclusive.
        /// </summary>
        public Func<double, Task<bool?>> GetTristateAsync { get => _getTristateAsync; set => _getTristateAsync = value; }
        private Func<double, Task<bool?>> _getTristateAsync;

        public Action<object> Callback { get => _callback; set => _callback = value; }
        private Action<object> _callback;

        public Func<object, bool, Task> CallbackAsync { get => _callbackAsync; set => _callbackAsync = value; }
        private Func<object, bool, Task> _callbackAsync;

        public TristateThreshhold Threshhold { get => _threshhold; set => _threshhold = value; }
        private TristateThreshhold _threshhold;

        public abstract void Train(string[] tokens, bool state);
        public void Train(object item, bool state)
        {
            Tokenize.ThrowIfNull($"{nameof(Tokenize)} delegate function cannot be null to Train classifier");
            item.ThrowIfNull($"{nameof(item)} cannot be null to Train classifier");
            var tokens = Tokenize(item);
            Train(tokens, state);
            if (Callback is not null) { Callback(item); }
            
        }

        public abstract Task TrainAsync(string[] tokens, bool state);
        
        public async Task TrainAsync(object item, bool state)
        {
            Tokenize.ThrowIfNull($"{nameof(Tokenize)} delegate function cannot be null to Train classifier");
            item.ThrowIfNull($"{nameof(item)} cannot be null to Train classifier");
            var tokens = await TokenizeAsync(item);
            await TrainAsync(tokens, state);
            if (CallbackAsync is not null) { await CallbackAsync(item, state); }
        }

        public bool? GetTristate(double probability)
        {
            Threshhold.ThrowIfNull($"{nameof(Threshhold)} cannot be null when calling GetTristate");
            if (probability > Threshhold.MinimumTrue) return true;
            else if (probability < Threshhold.MaximumFalse) return false;
            else return null;
        }
    }

    public class TristateThreshhold(double minimumTrue, double maximumFalse)
    {
        public double MinimumTrue = minimumTrue;
        public double MaximumFalse = maximumFalse;
    }

}
