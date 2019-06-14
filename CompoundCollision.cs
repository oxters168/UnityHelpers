using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompoundCollision : MonoBehaviour
{
    private Dictionary<GameObject, int> hits = new Dictionary<GameObject, int>();
    public event CollisionHandler onCollisionExit;
    public delegate void CollisionHandler(Collision col);

    private void OnCollisionEnter(Collision col)
    {
        if (col.rigidbody)
        {
            //Debug.Log(col.gameObject.name + " entered whose mass is " + col.rigidbody.mass);

            if (!hits.ContainsKey(col.gameObject))
                hits[col.gameObject] = 0;
            hits[col.gameObject]++;
        }
    }
    private void OnCollisionExit(Collision col)
    {
        if (col.rigidbody)
        {
            hits[col.gameObject]--;
            //Debug.Log(col.gameObject.name + " exited with mass of " + col.rigidbody.mass);
            if (hits[col.gameObject] <= 0)
            {
                //Debug.Log("Removed");
                onCollisionExit?.Invoke(col);
                hits.Remove(col.gameObject);
            }
        }
    }
}
