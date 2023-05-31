using System;
using System.Collections;
using UnityEngine;

public enum ProcessingState
{
    initial, pending, complete, failed
}

public class CoroutineWithData<T>
{
    public Coroutine coroutine { get; private set; }
    public object current;
    public T result = default;
    private IEnumerator target;
    public event Action<T> completed;
    private MonoBehaviour owner = GameController.Instance;
    public ProcessingState state = ProcessingState.initial;
    public CoroutineWithData(IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        this.state = ProcessingState.pending;
        while (target.MoveNext())
        {
            current = target.Current;
            yield return current;
        }
        result = (T)current;
        this.state = ProcessingState.complete;
        yield return result;
        if(completed != null) {
            completed(this.result);
        }
    }
}