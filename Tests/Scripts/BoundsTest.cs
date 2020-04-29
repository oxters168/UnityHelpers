using UnityEngine;
using UnityHelpers;

public class BoundsTest : MonoBehaviour
{
    public bool showLocalSingleBounds = true;
    public bool showLocalTotalBounds = true;
    public bool showGlobalSingleBounds = true;
    public bool showGlobalTotalBounds = true;

    private void OnDrawGizmos()
    {
        var localBounds = transform.GetBounds(Space.Self);
        var totalLocalBounds = transform.GetTotalBounds(Space.Self);
        var globalBounds = transform.GetBounds(Space.World);
        var totalGlobalBounds = transform.GetTotalBounds(Space.World);

        if (showGlobalSingleBounds)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(globalBounds.center, globalBounds.size);
        }
        if (showGlobalTotalBounds)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(totalGlobalBounds.center, totalGlobalBounds.size);
        }

        Gizmos.matrix = transform.localToWorldMatrix;
        if (showLocalSingleBounds)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(localBounds.center, localBounds.size);
        }
        if (showLocalTotalBounds)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(totalLocalBounds.center, totalLocalBounds.size);
        }
    }
}
