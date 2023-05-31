using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class LocalizationData : ScriptableObject, ISerializationCallbackReceiver //Serializable classes to store text game data in a separate file
{
    public Category[] tasksData; //Stores tasks info
    public LocalizationItem[] gameItems = new LocalizationItem[] {
        new LocalizationItem(LocalizationItemType.play_button,"Play"),
        new LocalizationItem(LocalizationItemType.about_button,"About"),
        new LocalizationItem(LocalizationItemType.quit_button,"Quit"),
        new LocalizationItem(LocalizationItemType.back_button,"Back"),
        new LocalizationItem(LocalizationItemType.about_text,"Get ready to solve some picture puzzles! Get stuck? Use hints! Earn coins by completing tasks or watching short videos! Have fun!"),
        new LocalizationItem(LocalizationItemType.locked_text,"locked"),
        new LocalizationItem(LocalizationItemType.reveal_one_letter,"Reveal one letter"),
        new LocalizationItem(LocalizationItemType.remove_excess,"Remove excess"),
        new LocalizationItem(LocalizationItemType.get_answer,"Get the answer"),
        new LocalizationItem(LocalizationItemType.get_coins,"Get coins"),
        new LocalizationItem(LocalizationItemType.watch_a_video,"Watch a video"),
        new LocalizationItem(LocalizationItemType.buy_for,"Buy for"),
        new LocalizationItem(LocalizationItemType.buy,"Buy"),
        new LocalizationItem(LocalizationItemType.@continue,"Continue"),
        new LocalizationItem(LocalizationItemType.main_menu,"Main menu"),
        new LocalizationItem(LocalizationItemType.level_complete,"Level complete!"),
        new LocalizationItem(LocalizationItemType.rate_text, "How do you like the gameplay so far? Please give us a feedback by rating the game in the store!"),
        new LocalizationItem(LocalizationItemType.rate, "Rate!"),
        new LocalizationItem(LocalizationItemType.later, "Not now"),
        new LocalizationItem(LocalizationItemType.never, "Never"),
        new LocalizationItem(LocalizationItemType.data_collection_consent, "Data collection consent"),
        new LocalizationItem(LocalizationItemType.personalized_ads, "Personalized advertisements"),
        new LocalizationItem(LocalizationItemType.ads_dialog, "Allow AD networks too use collected data and show personalized advertisements"),
        new LocalizationItem(LocalizationItemType.yes, "Yes"),
        new LocalizationItem(LocalizationItemType.no, "No"),
        new LocalizationItem(LocalizationItemType.saved_progress_message, "Looks like you have a better progress!"),
        new LocalizationItem(LocalizationItemType.wish_to_load, "Do you wish to load it?"),
        new LocalizationItem(LocalizationItemType.gdpr_dialog, "Hello there! Just to let you know, we use your gameplay and device data with your consent to improve our game!"),
        new LocalizationItem(LocalizationItemType.gdpr_dialog_ads, "Do you want us to present you relevant advertising that fits your interests?"),
        new LocalizationItem(LocalizationItemType.confirm, "Confirm"),
        new LocalizationItem(LocalizationItemType.learn_more, "Learn more"),
        new LocalizationItem(LocalizationItemType.remove_one_option, "Remove one option"),
        new LocalizationItem(LocalizationItemType.remove_two_options, "Remove two options"),
        new LocalizationItem(LocalizationItemType.chanse_to_mistake, "Chance to mistake"),
        new LocalizationItem(LocalizationItemType.sharing_text, "Please help me to complete the level in the category «{{category}}»!"),
        new LocalizationItem(LocalizationItemType.sharing_title, "Ask a friend for help"),
        new LocalizationItem(LocalizationItemType.sharing_button, "Ask friends"),
        new LocalizationItem(LocalizationItemType.bet, "Bet"),
        new LocalizationItem(LocalizationItemType.education_hints,"Get stuck? Use hints!"),
        new LocalizationItem(LocalizationItemType.education_clear,"Press the letter to clear. Swipe the to clear the field."),
        new LocalizationItem(LocalizationItemType.education_erasure,"Scrape the film to reveal the image!"),
        new LocalizationItem(LocalizationItemType.education_pixelate_button,"Press to make the image more clear!"),
        new LocalizationItem(LocalizationItemType.education_aim_button,"Press to start aiming. Press a second time to shoot!"),
        new LocalizationItem(LocalizationItemType.education_task_description,"Press to get usefull info about the task!"),
        new LocalizationItem(LocalizationItemType.education_bet,"How difficult this task is? Make a bet! If you answer incorrectly, then you will lose the bet, otherwise it will double"),
        new LocalizationItem(LocalizationItemType.more_games,"More games"),
        new LocalizationItem(LocalizationItemType.retry,"Retry"),
        new LocalizationItem(LocalizationItemType.settings, "Settings"),
        new LocalizationItem(LocalizationItemType.googleplaygames, "GP games"),
        new LocalizationItem(LocalizationItemType.privacy, "Privacy"),
        new LocalizationItem(LocalizationItemType.privacy_policy, "Privacy policy"),
        new LocalizationItem(LocalizationItemType.language, "Language"),
        new LocalizationItem(LocalizationItemType.sounds, "Sounds"),
        new LocalizationItem(LocalizationItemType.music, "Music"),
        new LocalizationItem(LocalizationItemType.ads_settings, "Ads settings"),
        new LocalizationItem(LocalizationItemType.advertising, "Advertising"),
        new LocalizationItem(LocalizationItemType.my_data, "My data"),
        new LocalizationItem(LocalizationItemType.policy, "Policy"),
        new LocalizationItem(LocalizationItemType.cloud_save, "Cloud save"),
        new LocalizationItem(LocalizationItemType.no_thanks, "No, thanks"),
        new LocalizationItem(LocalizationItemType.reload, "Reload"),
        new LocalizationItem(LocalizationItemType.unable_to_continue, "Unable to continue"),
        new LocalizationItem(LocalizationItemType.get, "Get"),
        new LocalizationItem(LocalizationItemType.coins, "coins"),
        new LocalizationItem(LocalizationItemType.bonus, "bonus"),
        new LocalizationItem(LocalizationItemType.select_category, "Select a category"),
        new LocalizationItem(LocalizationItemType.suggest_gpgames, "Do you want to use GooglePlayGames to save your game progress to keep it always sync?"),
        new LocalizationItem(LocalizationItemType.education_chance_to_mistake, "You can answer incorrectly and the game will continue. If you answer correctly, you will get back the spent coins"),
        new LocalizationItem(LocalizationItemType.empty, ""),
        };
    public SubCategory[] subCategories;
    public char[] randomLetters = new char[] { 'y', 'b', 'k', 'w', 't', 'r', 'j', 'v', 'u', 'c', 'q', 'i', 'z', 'x', 'l', 'p' }; //Stores random letters to be implemented

    public void OnAfterDeserialize()
    {
        subCategories = subCategories
            .Select(sc =>
            {
                if (!string.IsNullOrEmpty(sc.iconName) && string.IsNullOrEmpty(sc.Name))
                {
                    sc.Name = sc.iconName;
                }
                return sc;
            })
            .ToArray();
    }

    public void OnBeforeSerialize()
    {

    }
}

[System.Serializable]
public class LocalizationItem
{
    public LocalizationItem(LocalizationItemType key, string value)
    {
        this.key = key;
        this.value = value;
    }
    public LocalizationItemType key;
    public string value;
}

[System.Serializable]
public class Localization : IResource
{
    public string name = "English";
    public string filename = "english";
    public LocalizedLoadingSteps messages = new LocalizedLoadingSteps();

    public IEnumerable<string> Parents => new string[] { };

    public string Name => filename;

    public GameMode GameType => GameMode.Default;

}

public enum LocalizationItemType
{
    play_button, about_button, quit_button, back_button, about_text, locked_text, reveal_one_letter, remove_excess, get_answer,
    get_coins, watch_a_video, buy, @continue, main_menu, level_complete, rate_text, rate, never, later,
    data_collection_consent, personalized_ads, ads_dialog, yes, no, saved_progress_message, wish_to_load, gdpr_dialog, gdpr_dialog_ads, confirm, learn_more,
    remove_one_option, remove_two_options, chanse_to_mistake, sharing_text, sharing_title, sharing_button, bet, education_hints, education_clear, education_erasure,
    education_pixelate_button, education_pixelate_image, education_aim_button, education_aim_image, education_task_description, education_bet, more_games,
    retry, sharing_message, settings, googleplaygames, privacy, language, sounds, music, privacy_policy, ads_settings, advertising, my_data, policy, cloud_save,
    no_thanks, empty, reload, unable_to_continue, buy_for, get, coins, select_category, suggest_gpgames, education_chance_to_mistake, bonus,
}