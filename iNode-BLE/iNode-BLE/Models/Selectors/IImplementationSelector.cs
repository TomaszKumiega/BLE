using System;
using System.Collections.Generic;
using System.Text;

namespace ERGBLE.Models.Selectors
{
    public interface IImplementationSelector<TKey, TImeplementation>
    {
        TImeplementation Select(TKey key);
    }
}
