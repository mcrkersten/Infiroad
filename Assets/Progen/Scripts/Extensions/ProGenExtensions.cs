using System.Linq;
public static class ProGenExtensions
{
    public static bool IsValid(this ProGen progen)
    {
        return progen.Theme != null 
        && progen.Theme.wallPrefab != null
        && progen.Theme.roofPrefabs?.Length > 0
        && !progen.Theme.roofPrefabs.Any(r => r == null)
        && progen.Theme.windowPrefabs?.Length > 0
        && !progen.Theme.windowPrefabs.Any(w => w == null)
        && progen.Theme.doorPrefab != null 
        && 
            ((progen.Theme.allowCornerWalls && progen.Theme.cornerPrefab != null)
                ||
            (!progen.Theme.allowCornerWalls));
    }
}