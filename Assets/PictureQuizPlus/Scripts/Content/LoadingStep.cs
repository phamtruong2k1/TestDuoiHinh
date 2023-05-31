using System;
using System.Linq;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public enum LoadingStepType
{
    settings, resources, game_data, internet_connection, host_connection,
}

public interface ILoadingStep
{
    string LoadingMessage { get; }
    string ErrorMessage { get; }
    bool IsPending { get; }
    bool IsError { get; }
    object PayloadObject { get; }
    bool IsFired { get; set; }
    bool IsReady { get; }
    LoadingStepType Type { get; }
    void Start();

}
[Serializable]

public struct LoadingStepData
{
    public LoadingStepType type;
    public string loadingMessage;
    public string errorMessage;
}

public struct LoadingStep<T> : ILoadingStep
{
    public LoadingStepData data;
    public LoadingStepType Type { get => data.type; }

    public string LoadingMessage { get => data.loadingMessage; }
    public string ErrorMessage { get => data.errorMessage; }
    public Func<CoroutineWithData<T>> CreateCoroutineWithData;
    public CoroutineWithData<T> coroutineWithData;
    public bool IsPending { get => coroutineWithData.state == ProcessingState.pending; }
    public bool IsReady { get => coroutineWithData.state == ProcessingState.complete; }

    // public Func<object, bool> ErrorChecker { get; internal set; }
    public bool IsError { get => IsReady && (Payload == null || Payload.Equals(default(T))); }
    public bool IsFired { get; set; }
    public T Payload { get => (T)coroutineWithData.result; }
    public object PayloadObject { get => Payload; }
    public void Start()
    {
        coroutineWithData = CreateCoroutineWithData();
    }
    public object GetPayload()
    {
        return coroutineWithData.result;
    }
}

[Serializable]
public class LocalizedLoadingSteps
{
    public string defaultLoadingBarMessage = "Loading...";
    public string startingMessage = "Let's get started!";
    public string fileNotFoundMessage = "One of the resources was not found.";
    public string unknownErrorMessage = "Something went wrong.";
    public string errorPopupTitle = "Unable to continue";
    public string errorPopupReload = "Reload";
    public string errorPopupQuit = "Quit";
    public LoadingStepData[] localizedLoadingSteps;
    public LocalizedLoadingSteps()
    {
        var stepsEnumValues = (LoadingStepType[])Enum.GetValues(typeof(LoadingStepType));
        localizedLoadingSteps = new LoadingStepData[stepsEnumValues.Length];
        foreach (var step in stepsEnumValues)
        {
            var newStep = new LoadingStepData();
            newStep.type = step;
            switch (step)
            {
                case LoadingStepType.resources:
                    newStep.loadingMessage = "Loading Resources...";
                    newStep.errorMessage = "Something went wrong while loading resources. Please try again later!";
                    break;
                case LoadingStepType.game_data:
                    newStep.loadingMessage = "Loading Game Data...";
                    newStep.errorMessage = "Something went wrong while loading game data. Please try again later!";
                    break;
                case LoadingStepType.internet_connection:
                    newStep.loadingMessage = "Establishing Internet Connection...";
                    newStep.errorMessage = "Please check your internet connection and press Reload!";
                    break;
                case LoadingStepType.host_connection:
                    newStep.loadingMessage = "Establishing Host Connection...";
                    newStep.errorMessage = "Something went wrong while establishing connection with the game host. Please try again later!";
                    break;
                case LoadingStepType.settings:
                    newStep.loadingMessage = "Loading Settings...";
                    newStep.errorMessage = "Something went wrong while loading game settings. Please try again later!";
                    break;
                default:
                    newStep.loadingMessage = "Loading Data...";
                    newStep.errorMessage = "Something went wrong while loading data. Please try again later!";
                    break;
            }
            localizedLoadingSteps[(int)step] = newStep;
        }
    }

}