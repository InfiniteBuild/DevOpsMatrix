using DevOpsMatrix.Interface;

namespace DevOpsMatrix.Tfs.Server
{
    public class TfsChangeItemFactory : IChangeItemFactory
    {
        public IDevOpsChangeRequest GetChangeItem(string itemtype)
        {
            IDevOpsChangeRequest changeItem = new TfsWorkItem();
            changeItem.SetField<string>("System.WorkItemType", itemtype);

            return changeItem;
        }
    }
}
