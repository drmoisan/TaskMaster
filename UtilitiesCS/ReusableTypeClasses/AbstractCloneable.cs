using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.ReusableTypeClasses
{
    // From https://stackoverflow.com/questions/21116554/proper-way-to-implement-icloneable
    public abstract class AbstractCloneable: ICloneable
    {
        public object Clone() 
        { 
            var clone = (AbstractCloneable) this.MemberwiseClone();
            HandleCloned(clone);
            return clone;
        }

        protected virtual void HandleCloned(AbstractCloneable clone)
        {
            //Nothing particular in the base class, but maybe useful for children.
            //Not abstract so children may not implement this if they don't need to.
        }
    }

    
    // Example of how to use the AbstractClonable class.
    class ConcreteCloneableExample : AbstractCloneable
    {
        protected override void HandleCloned(AbstractCloneable clone)
        {
            //Get whathever magic a base class could have implemented.
            base.HandleCloned(clone);

            //Clone is of the current type.
            ConcreteCloneableExample obj = (ConcreteCloneableExample)clone;

            //Here you have a superficial copy of "this". You can do whathever 
            //specific task you need to do.
            //e.g.:
            //obj.SomeReferencedProperty = this.SomeReferencedProperty.Clone();
        }
    }

}
