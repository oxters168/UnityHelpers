using System;
using System.Collections;
using UnityEngine;

public static class CommonRoutines
{
    /// <summary>
    /// Keeps on doing an action until a condition has been met.
    /// </summary>
    /// <param name="action">The action to be repeated</param>
    /// <param name="pred">The condition that will stop the loop</param>
    /// <param name="timeBetween">The time to wait between each repeated action</param>
    /// <param name="postAction">The action to be done after completion</param>
    /// <param name="timeout">If set will cause the loop to end if the condition has not been met in time</param>
    /// <returns>Coroutine enumerator</returns>
    public static IEnumerator DoUntil(Action<float> action, Func<bool> pred, float timeBetween = 0, Action postAction = null, float timeout = -1)
    {
        float startTime = Time.time;

        bool predicateOutput = pred != null ? pred() : false;
        while ((timeout < 0 || Time.time - startTime <= timeout) && !predicateOutput)
        {
            action.Invoke(Time.time - startTime);

            if (timeBetween > 0)
                yield return new WaitForSeconds(timeBetween);
            else
                yield return null;

            if (pred != null)
                predicateOutput = pred();
        }
        postAction?.Invoke();
    }
    /// <summary>
    /// Waits to do action using either a timer or condition.
    /// </summary>
    /// <param name="action">The action to do after done waiting</param>
    /// <param name="timeout">The time to wait until invoking action; if set to 0 or less, will not time out</param>
    /// <param name="pred">The condition that will cause the action to be invoked; if not set, will rely on timeout</param>
    /// <param name="onFail">These actions will be invoked instead when a condition has been set but does not happen before time out</param>
    /// <returns>Coroutine enumerator</returns>
    public static IEnumerator WaitToDoAction(Action action, float timeout = 5, Func<bool> pred = null, Action onFail = null)
    {
        float startTime = Time.time;

        bool predicateOutput = pred != null ? pred() : false;
        while ((timeout < 0 || Time.time - startTime <= timeout) && !predicateOutput)
        {
            yield return null;
            if (pred != null)
                predicateOutput = pred();
        }

        if (predicateOutput || pred == null)
        {
            action?.Invoke();
        }
        else
            onFail?.Invoke();
    }
}
