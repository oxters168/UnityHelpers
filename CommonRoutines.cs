using System;
using System.Collections;
using UnityEngine;

public static class CommonRoutines
{
    public static IEnumerator WaitToDoAction(Func<bool> pred = null, float timeout = 5, Action onFail = null, params Action[] onSucceed)
    {
        float startTime = Time.time;

        bool predicateOutput = pred != null ? pred() : false;
        while ((timeout < 0 || Time.time - startTime <= timeout) && !predicateOutput)
        {
            if (pred != null)
                predicateOutput = pred();
            yield return null;
        }

        if (predicateOutput || pred == null)
        {
            foreach (Action after in onSucceed)
                after?.Invoke();
        }
        else
            onFail?.Invoke();
    }
}
