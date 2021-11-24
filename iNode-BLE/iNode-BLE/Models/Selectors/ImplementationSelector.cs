using Autofac.Features.Indexed;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERGBLE.Models.Selectors
{
    public class ImplementationSelector<TKey, TImplementation> : IImplementationSelector<TKey, TImplementation>
    {
        public IIndex<TKey, TImplementation> Implementations { get; }
        public ImplementationSelector(IIndex<TKey, TImplementation> implementations)
        {
            Implementations = implementations;
        }

        public TImplementation Select(TKey key)
        {
            return Implementations.TryGetValue(key, out TImplementation value)
                ? value
                : default;
        }
    }
}
