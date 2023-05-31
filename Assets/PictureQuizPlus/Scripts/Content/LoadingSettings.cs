using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

[CreateAssetMenu]
public class LoadingSettings : ScriptableObject
{
    public Localization[] localizations = new Localization[] { new Localization() };
    public bool isContentStoredOnWebServer = false;
    public string httpStorageUrl = "https://.github.io/";
    public string checkInternetConnectionUrl = "https://dns.google/";
    public string checkHostConnectionFile = "index.html";
    public string remoteImagesExtension = "png";

    public LoadingStepData getByTypeAndLanguage(LoadingStepType type, string language)
    {
        return localizations.FirstOrDefault(l => l.filename == language).messages.localizedLoadingSteps
            .FirstOrDefault(s => s.type == type);
    }

    public string getStartingMessage(string language)
    {
        return localizations.FirstOrDefault(l => l.filename == language).messages.startingMessage;
    }

    public string getErrorPopupTitle(string language)
    {
        return localizations.FirstOrDefault(l => l.filename == language).messages.errorPopupTitle;
    }

    public string getErrorPopupReload(string language)
    {
        return localizations.FirstOrDefault(l => l.filename == language).messages.errorPopupReload;
    }
    public string getErrorPopupQuit(string language)
    {
        return localizations.FirstOrDefault(l => l.filename == language).messages.errorPopupQuit;
    }

    public string getFileNotFoundMessage(string language)
    {
        return localizations.FirstOrDefault(l => l.filename == language).messages.fileNotFoundMessage;
    }
    public string getUnknownErrorMessage(string language)
    {
        return localizations.FirstOrDefault(l => l.filename == language).messages.unknownErrorMessage;
    }
    public string getDefaultLoadingBarMessage(string language)
    {
        return localizations.FirstOrDefault(l => l.filename == language).messages.defaultLoadingBarMessage;
    }
}
