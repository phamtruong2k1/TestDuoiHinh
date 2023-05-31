using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

class RemoteResourcesManager : ResourcesManager
{
    string remoteDetailsUrl, remoteContentUrl;
    string detailsLocalPath, contentLocalPath;
    string platformName;
    public const string downloads = "Downloads";

    IEnumerable<string> downloadedAssets = new List<string>();
    Dictionary<string, AssetBundle> cachedBundlesByPath = new Dictionary<string, AssetBundle>();
    Dictionary<string, Sprite> cachedSpritesByFilename = new Dictionary<string, Sprite>();

    public RemoteResourcesManager()
    {
#if UNITY_ANDROID
        platformName = "Android";
#elif UNITY_IOS
        platformName = "iOS";
#endif
        detailsLocalPath = Path.Combine(Application.persistentDataPath, downloads, platformName);
        contentLocalPath = Path.Combine(Application.persistentDataPath, downloads);
        remoteDetailsUrl = $"{GameController.Instance.HttpStorageUrl}/{downloads}/{platformName}";
        remoteContentUrl = $"{GameController.Instance.HttpStorageUrl}/{downloads}";
    }

    protected override void LoadSync<T, TObject>(ref T resource)
    {
        foreach (var name in resource.names)
        {
            TObject unityresource;
            if (typeof(T).IsSubclassOf(typeof(ContentResource<TObject>)))
            {
                string fileName = $"{name}.{resource.extension}";
                string path = Path.Combine(resource.path.Append(fileName).Prepend(contentLocalPath).ToArray());
                if (cachedSpritesByFilename.ContainsKey(path))
                {
                    unityresource = cachedSpritesByFilename[path] as TObject;
                }
                else
                {
                    try
                    {
                        Sprite sprite = CreateSpriteFromFile(path);
                        cachedSpritesByFilename.Add(path, sprite);
                        unityresource = sprite as TObject;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.Log(ex.Message);
                        unityresource = null;
                    }
                }

            }
            else if (typeof(T).IsSubclassOf(typeof(DetailsResource<TObject>)))
            {
                string path = Path.Combine(resource.path.Prepend(detailsLocalPath).ToArray());
                AssetBundle bndl;
                try
                {
                    if (cachedBundlesByPath.ContainsKey(path))
                    {
                        bndl = cachedBundlesByPath[path];
                    }
                    else
                    {
                        bndl = AssetBundle.LoadFromFile(path);
                        cachedBundlesByPath.Add(path, bndl);
                    }
                    unityresource = bndl.LoadAsset<TObject>(name);

                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"{ex.Message} path");
                    unityresource = null;
                }
            }
            else
            {
                unityresource = null;
            }
            if (unityresource == null)
            {
                resource.payload = new List<TObject>() { };
                break;
            }
            else resource.payload = resource.payload.Append(unityresource);
        }
    }

    protected override IEnumerator LoadAsync<T, TObject>(T resource)
    {
        foreach (var name in resource.names)
        {
            if (typeof(T).IsSubclassOf(typeof(ContentResource<TObject>)))
            {
                string path = string.Join("/", resource.path.Prepend(remoteContentUrl));
                string timestamp = DateTime.Now.ToFileTime().ToString();
                string fileName = $"{name}.{resource.extension}";
                string resultUri = string.Join("/", new string[] { path, fileName });

                // Debug.Log($"download {resultUri}");

                using (UnityWebRequest unityWebRequest = UnityWebRequest.Get($"{resultUri}?v={timestamp}"))
                {
                    yield return unityWebRequest.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
                    bool isNetWorkError = unityWebRequest.result != UnityWebRequest.Result.Success;
#else
                    bool isNetWorkError = unityWebRequest.isHttpError || unityWebRequest.isNetworkError;
#endif  
                    if (isNetWorkError)
                    {
                        Debug.LogError($"HTTP error: {resultUri}");
                        resource = null;
                        break;
                    }
                    yield return new WaitUntil(() => unityWebRequest.downloadHandler.isDone);
                    byte[] data = unityWebRequest.downloadHandler.data;
                    string pathToSave = Path.Combine(resource.path.Prepend(contentLocalPath).ToArray());
                    DirectoryInfo dirInfo = Directory.CreateDirectory(pathToSave);

                    string filePath = Path.Combine(dirInfo.FullName, fileName);
                    // Debug.Log($"saving {filePath}");
                    using (FileStream SourceStream = File.Create(filePath))
                    {
                        SourceStream.Seek(0, SeekOrigin.End);
                        Task task = SourceStream.WriteAsync(data, 0, data.Length);
                        yield return new WaitUntil(() => task.IsCanceled || task.IsCompleted || task.IsFaulted);
                        if (task.IsCanceled || task.IsFaulted)
                        {
                            Debug.LogError($"FileStream error: {filePath}");
                            resource = null;
                            break;
                        }
                    }

                    this.LoadSync<T, TObject>(ref resource);

                }
            }
            else if (typeof(T).IsSubclassOf(typeof(DetailsResource<TObject>)))
            {

                string path = string.Join("/", resource.path.Prepend(remoteDetailsUrl));
                string timestamp = DateTime.Now.ToFileTime().ToString();

                if (downloadedAssets.Contains(path))
                {
                    this.LoadSync<T, TObject>(ref resource);
                    break;
                }
                using (UnityWebRequest unityWebRequest = UnityWebRequest.Get($"{path}?v={timestamp}"))
                {
                    yield return unityWebRequest.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
                    bool isNetWorkError = unityWebRequest.result != UnityWebRequest.Result.Success;
#else
                    bool isNetWorkError = unityWebRequest.isHttpError || unityWebRequest.isNetworkError;
#endif  
                    if (isNetWorkError)
                    {
                        Debug.LogError($"HTTP error: {path}");
                        resource = null;
                        break;
                    }

                    yield return new WaitUntil(() => unityWebRequest.downloadHandler.isDone);

                    byte[] data = unityWebRequest.downloadHandler.data;
                    IEnumerable<string> folder = resource.path.Take(resource.path.Count() - 1);
                    string pathToSave = Path.Combine(folder.Prepend(detailsLocalPath).ToArray());
                    DirectoryInfo dirInfo = null;
                    try
                    {
                        dirInfo = Directory.CreateDirectory(pathToSave);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError(ex.Message);
                        resource = null;
                        break;
                    }
                    string fileName = resource.path.Last();
                    string filePath = Path.Combine(dirInfo.FullName, fileName);
                    Debug.Log($"Saving file: {filePath}");

                    using (FileStream SourceStream = File.Create(filePath))
                    {
                        SourceStream.Seek(0, SeekOrigin.End);
                        Task task = SourceStream.WriteAsync(data, 0, data.Length);
                        yield return new WaitUntil(() => task.IsCanceled || task.IsCompleted || task.IsFaulted);
                        if (task.IsCanceled || task.IsFaulted)
                        {
                            Debug.LogError($"FileStream error: {filePath}");
                            resource = null;
                            break;
                        }
                    }
                    downloadedAssets.Append(path);
                    this.LoadSync<T, TObject>(ref resource);
                }
            }

        }
        if (resource != null && resource.payload.Count() < 1)
        {
            Debug.LogError("Empty resource payload");
            yield return null;
        }
        else yield return resource;
        // }
    }

    private Sprite CreateSpriteFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                var fileData = File.ReadAllBytes(filePath);
                Texture2D newtask = new Texture2D(1, 1); ;
                newtask.LoadImage(fileData);
                return Sprite.Create(newtask, new Rect(0, 0, newtask.width, newtask.height), new Vector2(0.5f, 0.5f));
            }
            catch (Exception)
            {
                throw new ReadingFileException();
            }
        }
        else
        {
            throw new FileNotFoundException(filePath);
        }
    }
}