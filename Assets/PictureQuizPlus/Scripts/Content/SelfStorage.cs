using System;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

class LocalResourcesManager : ResourcesManager
{
    public LocalResourcesManager()
    {

    }
    protected override void LoadSync<T, TObject>(ref T resource)
    {
       string path = Path.Combine(resource.path.ToArray());
       foreach (var name in resource.names)
       {
           string absolutePath = Path.Combine(path, name);
           TObject unityresource = Resources.Load(absolutePath, resource.unityObjectType) as TObject;
           resource.payload = resource.payload.Append(unityresource);
       }
    }

    protected override IEnumerator LoadAsync<T, TObject>(T resource)
    {
       string path = Path.Combine(resource.path.ToArray());
       foreach (var name in resource.names)
       {
           string absolutePath = Path.Combine(path, name);
           ResourceRequest req = Resources.LoadAsync(absolutePath, resource.unityObjectType);
           yield return new WaitUntil(() => req.isDone);
           TObject unityresource = req.asset as TObject;
           resource.payload = resource.payload.Append(unityresource);
       }
       yield return resource;
    }
}