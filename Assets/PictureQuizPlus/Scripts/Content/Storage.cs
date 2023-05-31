using System.Collections;
using System.Linq;
using System.IO;
using UnityEngine;

public abstract class ResourcesManager
{
    public ResourcesManager()
    {

    }
    protected abstract void LoadSync<T, TObject>(ref T resource)
        where T : Resource<TObject>
        where TObject : UnityEngine.Object;
    protected abstract IEnumerator LoadAsync<T, TObject>(T resource)
        where T : Resource<TObject>
        where TObject : UnityEngine.Object;

    public T Get<T, TObject>()
        where T : Resource<TObject>, new()
        where TObject : UnityEngine.Object
    {
        T resource = new T();
        this.LoadSync<T, TObject>(ref resource);
        return resource;
    }

    public T Get<T, TObject>(IResource resObj)
        where T : Resource<TObject>, IResourceWithObject, new()
        where TObject : UnityEngine.Object
    {
        T resource = new T();
        ((IResourceWithObject)resource).SetResourceObject(resObj);
        this.LoadSync<T, TObject>(ref resource);
        return resource;
    }

    private IEnumerator DummyCoroutine<T>(T res)
    {
        yield return res;
    }
    public CoroutineWithData<T> GetAsync<T, TObject>(bool trySync = true)
        where T : Resource<TObject>, new()
        where TObject : UnityEngine.Object
    {
        if (trySync)
        {
            T res = this.Get<T, TObject>();
            if (res.payload.Count() > 0)
            {
                // Debug.Log("Found locally  " + Path.Combine(res.path.Append(res.names.FirstOrDefault()).ToArray()));
                return new CoroutineWithData<T>(this.DummyCoroutine<T>(res));
            }
        }
        T resource = new T();
        return new CoroutineWithData<T>(this.LoadAsync<T, TObject>(resource));
    }

    public CoroutineWithData<T> GetAsync<T, TObject>(IResource resObj, bool trySync = true)
        where T : Resource<TObject>, IResourceWithObject, new()
        where TObject : UnityEngine.Object
    {
        if (trySync)
        {
            T res = this.Get<T, TObject>(resObj);
            if (res.payload.Count() > 0)
            {
                // Debug.Log("Found locally " + Path.Combine(res.path.Append(res.names.FirstOrDefault()).ToArray()));
                return new CoroutineWithData<T>(this.DummyCoroutine<T>(res));
            }
        }
        T resource = new T();
        ((IResourceWithObject)resource).SetResourceObject(resObj);
        return new CoroutineWithData<T>(this.LoadAsync<T, TObject>(resource));
    }

}