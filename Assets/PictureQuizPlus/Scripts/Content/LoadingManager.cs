using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LoadingManager : MonoBehaviour
{
    private GameObject loadingScreen;
    private Text loadingText;
    private GameObject loadingBar;
    private GameObject currentBar;
    private Transform loadingBarRoot;
    private float prevProgress;
    private string startingMessage;
    private Queue<IEnumerable<ILoadingStep>> loadingQueue = new Queue<IEnumerable<ILoadingStep>>();

    private IEnumerable<ILoadingStep> currentSteps = new ILoadingStep[] { };
    public event Action<ILoadingStep> stepCompleted;

    private bool isReady;

    public bool IsReady => isReady /* && loadingScreen == null */;

    public bool LoadingScreenClosed => loadingScreen == null;

    public bool IsError { get; private set; }
    private bool isResourcesEnqueued = false;
    private int stepsCount = 0;
    private int completedStepsCount = 0;

    private IEnumerator CheckConnection(string url)
    {
        // Debug.Log($"CheckConnection {url}");
        using (UnityWebRequest unityWebRequest = UnityWebRequest.Head(url))
        {
            yield return unityWebRequest.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
            bool isSucceeded = unityWebRequest.result == UnityWebRequest.Result.Success;
#else
            bool isSucceeded = !unityWebRequest.isNetworkError && !unityWebRequest.isHttpError;
#endif  
            yield return isSucceeded;
        }
    }

    private void EnqueueStepsBatch(IEnumerable<ILoadingStep> batch)
    {
        stepsCount += batch.Count();
        loadingQueue.Enqueue(batch);
    }

    private bool DequeueBatch()
    {
        if (currentSteps.Count() > 0)
        {
            completedStepsCount += currentSteps.Count();
            foreach (ILoadingStep item in currentSteps)
            {
                if (stepCompleted != null && !item.IsFired)
                {
                    item.IsFired = true;
                    stepCompleted(item);
                }
            }
        }
        if (loadingQueue.Count > 0)
        {
            currentSteps = loadingQueue.Dequeue();
            currentSteps.ToList().ForEach(step => step.Start());
            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerable<ILoadingStep> PeekBatch()
    {
        return loadingQueue.Peek();
    }

    public void Initialize()
    {
        if (GameController.Instance.IsContentStoredOnWebServer)
        {
            EnqueueStepsBatch(new ILoadingStep[] {
                new LoadingStep<bool>() {
                    data = GetLoadingStepData(LoadingStepType.internet_connection),
                    CreateCoroutineWithData = () => new CoroutineWithData<bool>(CheckConnection(GameController.Instance.CheckInternetConnectionUrl)),
                },
                new LoadingStep<bool>() {
                    data = GetLoadingStepData(LoadingStepType.host_connection),
                    CreateCoroutineWithData = () => new CoroutineWithData<bool>(CheckConnection(GameController.Instance.HttpStorageUrl + $"/{RemoteResourcesManager.downloads}/" + GameController.Instance.CheckHostConnectionFile)),
                }
            });
        }

        EnqueueStepsBatch(new ILoadingStep[] {
                new LoadingStep<GameSettingsResource>() {
                    data = GetLoadingStepData(LoadingStepType.settings),
                    CreateCoroutineWithData = () => GameController.Instance.ResourcesManager.GetAsync<GameSettingsResource, GameSettings>(false),
                },

        });
        EnqueueStepsBatch(new ILoadingStep[] {
                new LoadingStep<LocalizationDataResource>() {
                    data = GetLoadingStepData(LoadingStepType.game_data),
                    CreateCoroutineWithData = () => GameController.Instance.ResourcesManager.GetAsync<LocalizationDataResource, LocalizationData>(GameController.Instance.CurrentLocalization, false),
                },
        });


        loadingScreen = Instantiate(Utils.CreateFromPrefab("LoadingScreen"), Utils.getRootTransform());
        loadingText = loadingScreen.transform.Find("Text").GetComponent<Text>();
        loadingText.text = GameController.Instance.LoadingSettings.getDefaultLoadingBarMessage(GameController.Instance.CurrentLocalization.filename);
        loadingBarRoot = loadingScreen.transform.Find("Bar");
        loadingBar = loadingScreen.transform.Find("Bar/LoadingBar").gameObject;
        // currentBar = Instantiate(loadingBar, loadingBarRoot);
        startingMessage = GameController.Instance.LoadingSettings.getStartingMessage(GameController.Instance.CurrentLocalization.filename);

        DequeueBatch();

    }

    private LoadingStepData GetLoadingStepData(LoadingStepType type)
    {
        return GameController.Instance.LoadingSettings
            .getByTypeAndLanguage(type, GameController.Instance.CurrentLocalization.filename);
    }

    private void EnqueueCategory(Category category, LoadingStepData data)
    {
        if (category.HasNoImage)
        {
            EnqueueStepsBatch(new ILoadingStep[] { new LoadingStep<ContentIconResource>()
                {
                    data = data,
                    CreateCoroutineWithData = () => GameController.Instance.ResourcesManager.GetAsync<ContentIconResource, Sprite>(category),
                }});
        }
        else
        {
            if (category.currentLevel.index != 0)
            {
                EnqueueStepsBatch(new ILoadingStep[] { new LoadingStep<LevelImagesResource>()
                {
                    data = data,
                    CreateCoroutineWithData = () => GameController.Instance.ResourcesManager.GetAsync<LevelImagesResource, Sprite>(category.currentLevel),
                }});
            }

            if (category.previousLevel.index != 0)
            {
                EnqueueStepsBatch(new ILoadingStep[] { new LoadingStep<LevelImagesResource>()
                {
                    data = data,
                    CreateCoroutineWithData = () => GameController.Instance.ResourcesManager.GetAsync<LevelImagesResource, Sprite>(category.previousLevel),
                }});
            }
        }

    }
    private void EnqueueResources()
    {
        LoadingStepData data = GetLoadingStepData(LoadingStepType.resources);

        if (GameController.Instance.EnableSubcategories)
        {
            foreach (SubCategory item in GameController.Instance.SubCategories)
            {
                EnqueueStepsBatch(new ILoadingStep[] { new LoadingStep<ContentIconResource>()
                {
                    data = data,
                    CreateCoroutineWithData = () => GameController.Instance.ResourcesManager.GetAsync<ContentIconResource, Sprite>(item),
                }});
            }
        }
        foreach (Category category in GameController.Instance.Categories)
        {
            EnqueueCategory(category, data);
        }

        if (GameController.Instance.LoadingSettings.localizations.Length > 1)
        {
            foreach (Localization item in GameController.Instance.LoadingSettings.localizations)
            {
                EnqueueStepsBatch(new ILoadingStep[] { new LoadingStep<LocalizationIconResource>()
                {
                    data = data,
                    CreateCoroutineWithData = () => GameController.Instance.ResourcesManager.GetAsync<LocalizationIconResource, Sprite>(item, false),
                }});
            }
        }
    }

    public void Update()
    {
        if (IsError || isReady /* || currentSteps == null */)
        {
            if (isReady && loadingBar != null)
            {
                loadingBar.transform.localScale = Vector3.Lerp(loadingBar.transform.localScale, new Vector3(1f, 1f, 1f), 0.1f);
            }
            return;
        }

        if (!isResourcesEnqueued && GameController.Instance.Categories != null)
        {
            EnqueueResources();
            isResourcesEnqueued = true;
            return;
        }

        if (currentSteps.Any(s => s.IsError))
        {
            IsError = true;
            GameController.Instance.errorMessage = currentSteps.FirstOrDefault(s => s.IsError).ErrorMessage;
            GameController.Instance.popup.Open<ErrorPopup>(
                new PopupSettings()
                {
                    color = ThemeColorEnum.Negative,
                    title = GameController.Instance.LoadingSettings.getErrorPopupTitle(GameController.Instance.CurrentLocalization.filename),
                    unableToClose = true
                }
            );
            GameController.Instance.popup.CanBeClosed = false;
            return;
        }

        if (currentSteps.Any(s => s.IsPending))
        {
            string loadingMessage = currentSteps.LastOrDefault(s => s.IsPending).LoadingMessage;
            loadingText.text = string.IsNullOrEmpty(loadingMessage) ? startingMessage : loadingMessage;
            float progress = (float)completedStepsCount / stepsCount;
            if (!isResourcesEnqueued)
            {
                progress /= 2;
                progress = Math.Min(0.5f, progress);
            }
            else
            {
                progress = Math.Max(0.5f, progress);
            }
            loadingBar.transform.localScale = Vector3.Lerp(loadingBar.transform.localScale, new Vector3(progress, 1f, 1f), 0.1f);
            if (progress >= 1f)
            {
                isReady = true;
                Destroy(loadingScreen, 0.5f);
            }
        }

        if (currentSteps.All(s => s.IsReady))
        {
            DequeueBatch();
        }

    }

}