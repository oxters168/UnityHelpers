using UnityEngine;
using UnityHelpers;

public class LocalToLocalTest : MonoBehaviour
{
    public Transform parentFrom, parentTo, child;

    void Update()
    {
        if (parentFrom != null && parentTo != null && child != null)
            Debug.Log(child.name + "'s local position in " + parentTo.name + " would be: " + parentFrom.TransformPointToAnotherSpace(parentTo, child.localPosition));
    }
}
