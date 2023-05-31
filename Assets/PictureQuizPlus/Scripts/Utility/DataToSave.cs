using System;
using System.Collections.Generic;
using System.Linq;

//Structures to hold levels states in the data file
[Serializable]
public struct DataToSave
{
    public int coinsCount;
    public bool ads;
    public List<LevelInfo> levelsInfo;
    public DataToSave(int coins)
    {
        ads = true;
        coinsCount = coins;
        levelsInfo = new List<LevelInfo>();
    }
    public override bool Equals(object obj) //Overriding "equals" method to calculate is the data from GP saves fresher than local stored data
    {
        DataToSave temp = (DataToSave)obj;
        var x = from a in levelsInfo
                from b in temp.levelsInfo
                where a.directoryName == b.directoryName
                select a.currentLevel < b.currentLevel;

        bool w = temp.levelsInfo.Select(u => u.directoryName).Except(levelsInfo.Select(u => u.directoryName)).Count() > 0;
        return !(x.Contains(true) || w);
    }

    public override int GetHashCode()
    {
        var hashCode = -1935336738;
        hashCode = hashCode * -1521134295 + coinsCount.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<List<LevelInfo>>.Default.GetHashCode(levelsInfo);
        return hashCode;
    }
}

[Serializable]
public struct FailedLevel
{
    public FailedLevel(int num, int coins)
    {
        orderNumber = num;
        this.coins = coins;
    }
    public int orderNumber;
    public int coins;
}

[Serializable]
public struct LevelInfo
{
    public string directoryName;
    public int currentLevel;
    public int lettersOppened;
    public bool isLettersRemoved;
    public int openedPictures;
    public int[] openedPlanks;
    public string maskPath;
    private List<string> disclosedAnswers;
    public bool chanseUsed;
    [System.NonSerialized]
    public bool isNewRecord;
    public int levelBet;
    public List<FailedLevel> failedLevels;

    public List<string> DisclosedAnswers
    {
        get
        {
            if (disclosedAnswers == null)
            {
                disclosedAnswers = new List<string>();
            }
            return disclosedAnswers;
        }
        set => disclosedAnswers = value;
    }

    public void ResetHints()
    {
        lettersOppened = lettersOppened = levelBet = openedPictures = default;
        isLettersRemoved = chanseUsed = default;
        DisclosedAnswers = default;
        openedPlanks = default;
        maskPath = default;
    }

}

//Classes to hold game data in the GameController
[Serializable]
public struct Level : UnityEngine.ISerializationCallbackReceiver, IResource
{
    public Level(int index)
    {
        wrongAnswers = new string[] { };
        rightAnswer = "?";
        this.index = index;
        gameType = GameMode.Default;
        this.imageText = default(string);
        this.imageDescription = default(string);
        this.cost = reward = default(int);
        this.number = default(int);
        this.isComplete = default(bool);
        this.parents = default(IEnumerable<string>);
        noImage = default;
    }

    public int index;
    public GameMode gameType;
    public string rightAnswer;
    public bool noImage;
    [UnityEngine.HideInInspector]
    public int cost;
    public int reward;
    public string imageText;
    public string[] wrongAnswers;
    public string imageDescription;

    [UnityEngine.HideInInspector]
    public int number;
    [System.NonSerialized]
    [UnityEngine.HideInInspector]
    public bool isComplete;

    private IEnumerable<string> parents;
    public string Name { get => index.ToString(); }


    [UnityEngine.HideInInspector]
    public IEnumerable<string> Parents
    {
        get { return parents ?? new string[] { }; }
        set { parents = value; }
    }

    [UnityEngine.HideInInspector]
    public AnswerType AnswerType { get { return wrongAnswers == null || wrongAnswers.Length < 3 ? AnswerType.Letters : AnswerType.Variants; } }

    public GameMode GameType => gameType;

    public void OnAfterDeserialize()
    {
        if (this.number != default(int) && this.index == default(int))
        {
            this.index = this.number;
        }
        if (this.cost != default(int) && this.reward == default(int))
        {
            this.reward = this.cost;
        }
    }
    public void OnBeforeSerialize()
    {

    }

    /* internal string GetPath()
    {
        return string.Join("/", parents.Append(this.index.ToString()));
    } */
}

public interface ICategory : IResource
{
    int Length { get; }
    int CompletedLength { get; }

    string LocalizedName { get; }
    bool IsComplete { get; }
    bool IsUnlocked { get; }
    IEnumerable<ICategory> Children { get; }
}

public interface IResource
{
    IEnumerable<string> Parents { get; }
    string Name { get; }

    GameMode GameType { get; }
}

[Serializable]
public struct Category : UnityEngine.ISerializationCallbackReceiver, ICategory
{
    public Category(string name, int lvls, int index)
    {
        orderNumber = default(int);
        savedData = default(LevelInfo);
        isComplete = forceNoImage = isUnlocked = useMixedTypes = default(bool);
        parents = default(IEnumerable<string>);
        currentLevel = default(Level);
        previousLevel = default(Level);
        gameType = GameMode.Default;

        sortingIndex = index;
        this.folderName = name;
        this.name = name;
        localizedName = name;
        Levels = new Level[lvls];
        for (int i = 0; i < Levels.Length; i++)
        {
            Levels[i] = new Level(i + 1);
        }
    }
    [UnityEngine.HideInInspector]
    public string name;
    public string folderName;
    public string localizedName;
    public GameMode gameType;
    public bool useMixedTypes;
    public int sortingIndex;
    public bool forceNoImage;
    [UnityEngine.HideInInspector]
    public int orderNumber;
    public Level[] Levels;
    [UnityEngine.HideInInspector]
    [System.NonSerialized]
    public LevelInfo savedData;
    [UnityEngine.HideInInspector]
    [System.NonSerialized]
    public bool isUnlocked;
    [UnityEngine.HideInInspector]
    [System.NonSerialized]
    public bool isComplete;
    [UnityEngine.HideInInspector]
    [System.NonSerialized]
    public Level currentLevel;
    [UnityEngine.HideInInspector]
    [System.NonSerialized]
    public Level previousLevel;
    private IEnumerable<string> parents;

    [UnityEngine.HideInInspector]
    public IEnumerable<string> Parents
    {
        get { return parents ?? new string[] { }; }
        set { parents = value; }
    }

    public Level[] LVLs { get => Levels ?? new Level[] { }; set => Levels = value; }
    public string Name { get => folderName; set => folderName = value; }

    public GameMode GameType => gameType;

    public bool HasNoImage => this.forceNoImage || this.Levels.All(l => l.noImage);

    public void OnBeforeSerialize()
    {
    }

    /*  public string GetPath()
     {
         return string.Join("/", Parents.Concat(new string[] { name }).Where(s => !string.IsNullOrEmpty(s)));
     } */
    /* public string GetIconPath()
    {
        return string.Join("/", Parents.Concat(new string[] { name, "icon" }).Where(s => !string.IsNullOrEmpty(s)));
    } */
    public void OnAfterDeserialize()
    {
        if (this.orderNumber != default(int) && this.sortingIndex == default(int))
        {
            this.sortingIndex = this.orderNumber;
        }
        if (this.name != default(string) && this.folderName == default(string))
        {
            this.folderName = this.name;
        }
    }

    public int Length => this.Levels.Length;

    public int CompletedLength => this.Levels.Count(l => l.isComplete);

    public string LocalizedName => this.localizedName;

    public bool IsComplete => this.isComplete;


    public bool IsUnlocked => this.isUnlocked;

    public IEnumerable<ICategory> Children => new ICategory[] { };

}

[Serializable]
public struct SubCategory : ICategory
{
    public SubCategory(int index, string name, string[] subCategories)
    {
        this.sortingIndex = index;
        this.name = name;
        this.iconName = default(string);
        this.categories = default(List<Category>);
        this.isUnlocked = this.isComplete = default(bool);
        localizedTitle = name;
        subcategories = subCategories;
    }
    [UnityEngine.HideInInspector]
    public string iconName;
    public string name;
    public string localizedTitle;
    public string[] subcategories;
    public int sortingIndex;

    [System.NonSerialized]
    [UnityEngine.HideInInspector]
    private List<Category> categories;

    [System.NonSerialized]
    [UnityEngine.HideInInspector]
    public bool isUnlocked;
    [System.NonSerialized]
    [UnityEngine.HideInInspector]
    public bool isComplete;

    public List<Category> Categories { get => categories ?? new List<Category>(); set => categories = value; }
    public string Name { get => name; set => name = value; }

    public int Length => this.Categories.Count();

    public int CompletedLength => this.Categories.Count(c => c.isComplete);

    public string LocalizedName => this.localizedTitle;

    public bool IsComplete => this.isComplete;

    public bool IsUnlocked => this.isUnlocked;

    public IEnumerable<ICategory> Children => this.Categories.Select(c => c as ICategory);

    public IEnumerable<string> Parents
    {
        get { return new string[] { }; }
    }

    public GameMode GameType => GameMode.Default;
}
