using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public class Prediction<T>: IComparable<Prediction<T>>
    {
        public Prediction() { }

        public Prediction(T @class, double probability)
        {
            _class = @class;
            _probability = probability;
        }

        private T _class;
        public T Class { get => _class; set => _class = value; }

        private double _probability;
        public double Probability { get => _probability; set => _probability = value; }

        public int CompareTo(Prediction<T> other)
        {
            if (other is null) { return 1; }
            else { return _probability.CompareTo(other._probability); }
        }        
    }
}
