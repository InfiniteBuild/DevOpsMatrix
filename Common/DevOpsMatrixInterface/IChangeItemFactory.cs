using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsInterface
{
    public interface IChangeItemFactory
    {
        IDevOpsChangeRequest GetChangeItem(string itemtype);
    }
}
