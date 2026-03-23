using CG.Client.UserData;
using ResourceAssets;

namespace DebugTools
{
    public static class Common
    {
        /// <summary>
        /// Checks if an item's Unlock Entry does not exist or is permanently locked.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        public static bool IsItemLocked(GUIDUnion GUID)
        {
            return !UnlockContainer.Instance.TryGetByGuid(GUID, out UnlockItemDef value) || value.unlockOptions.UnlockCriteria == UnlockCriteriaType.Never;
        }
    }
}
