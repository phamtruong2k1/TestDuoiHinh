using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public abstract class Resource<T> where T : UnityEngine.Object
{
    public IEnumerable<string> path = new string[] { };
    public IEnumerable<string> names = new string[] { };
    public string extension;
    public IEnumerable<T> payload = new List<T>() { };
    public Type unityObjectType = typeof(T);

    public T GetFirst()
    {
        if (payload.Count() > 0)
        {
            return payload.First();
        }
        else
        {
            return null;
        }
    }

    public IEnumerable<T> GetAll()
    {
        return payload;
    }
}

public abstract class DetailsResource<T> : Resource<T> where T : UnityEngine.Object
{
    public DetailsResource()
    {
        this.path = this.path.Concat(new string[] { "details" });
    }
}

public abstract class SettingsResource<T> : DetailsResource<T> where T : UnityEngine.Object
{
    public SettingsResource() : base()
    {
        this.path = this.path.Concat(new string[] { "settings" });
    }
}

public interface IResourceWithObject
{
    void SetResourceObject(IResource resObj);
}

public abstract class ContentResource<T> : Resource<T>, IResourceWithObject where T : UnityEngine.Object
{

    public ContentResource()
    {
        this.path = this.path.Concat(new string[] { "content" });
        if (GameController.Instance != null)
        {
            this.extension = GameController.Instance.RemoteImagesExtension;
        }
    }

    public virtual void SetResourceObject(IResource resObj)
    {
        this.path = path.Concat(resObj.Parents);
    }
}
public class LocalizationIconResource : DetailsResource<Sprite>, IResourceWithObject
{
    public LocalizationIconResource() : base()
    {
        this.path = this.path.Concat(new string[] { "flags" });
    }
    public void SetResourceObject(IResource resObj)
    {
        this.names = new string[] { resObj.Name };
    }
}

public class LocalizationDataResource : DetailsResource<LocalizationData>, IResourceWithObject
{
    public LocalizationDataResource() : base()
    {
        this.path = this.path.Concat(new string[] { "localizations" });
    }
    public void SetResourceObject(IResource resObj)
    {
        this.names = new string[] { resObj.Name };
    }
}

public class GameSettingsResource : SettingsResource<GameSettings>
{
    public GameSettingsResource() : base()
    {
        this.names = new string[] { "Game" };
    }
}

public class LoadingSettingsResource : SettingsResource<LoadingSettings>
{
    public LoadingSettingsResource() : base()
    {
        this.names = new string[] { "Loading" };
    }
}

public class LevelImagesResource : ContentResource<Sprite>, IResourceWithObject
{
    public LevelImagesResource() : base()
    {
    }

    public override void SetResourceObject(IResource resObj)
    {
        base.SetResourceObject(resObj);
        if (resObj.GameType == GameMode.FourImages)
        {
            this.names = new string[] { resObj.Name, $"{resObj.Name}_2", $"{resObj.Name}_3", $"{resObj.Name}_4", };
        }
        else this.names = new string[] { resObj.Name };
    }
}

public class ContentIconResource : ContentResource<Sprite>
{
    public ContentIconResource() : base()
    {
        this.names = new string[] { "icon" };
    }

    public override void SetResourceObject(IResource resObj)
    {
        base.SetResourceObject(resObj);
        this.path = path.Append(resObj.Name);
    }
}
