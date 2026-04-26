using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
   public static GameManager Instance { get; private set; }
  
   public BoardManager BoardManager;
   public PlayerController PlayerController;

   public TurnManager TurnManager { get; private set;}
   private int m_FoodAmount = 100;

   public UIDocument UIDoc;
    private Label m_FoodLabel;
    private int m_CurrentLevel = 1;

    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;

    public int baseWidth = 8;
    public int baseHeight = 8;
    public int sizeIncreasePerLevel = 2;

    public int baseFood = 6;
    public int baseWalls = 6;
    public int baseEnemies = 1;

    public int foodDecreasePerLevel = 1;
    public int enemyIncreasePerLevel = 1;
    public int wallIncreasePerLevel = 1;

    private Label m_StrengthLabel;

    public Camera MainCamera;

    private VisualElement m_MainMenu;
    private Button m_StartButton;
    private Button m_QuitButton;
    private VisualElement m_HUD;

    void UpdateCameraSize()
    {
        float size = Mathf.Max(BoardManager.Width, BoardManager.Height) * 0.6f;
        MainCamera.orthographicSize = Mathf.Clamp(size, 4f, 12f);
    }


    public int GetFoodCount()
    {
        return Mathf.Max(1, baseFood - (m_CurrentLevel * foodDecreasePerLevel));
    }

    public int GetWallCount()
    {
        return baseWalls + (m_CurrentLevel * wallIncreasePerLevel);
    }

    public int GetEnemyCount()
    {
        return baseEnemies + (m_CurrentLevel * enemyIncreasePerLevel);
    }

   private void Awake()
   {
       if (Instance != null)
       {
           Destroy(gameObject);
           return;
       }
      
       Instance = this;
   }
  
   void Start()
    {
    TurnManager = new TurnManager();
    TurnManager.OnTick += OnTurnHappen;
    
    m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
    m_StrengthLabel = UIDoc.rootVisualElement.Q<Label>("StrengthLabel");

    m_MainMenu = UIDoc.rootVisualElement.Q<VisualElement>("MainMenu");
    m_StartButton = UIDoc.rootVisualElement.Q<Button>("StartButton");
    m_QuitButton = UIDoc.rootVisualElement.Q<Button>("QuitButton");

    PlayerController.OnStatsChanged += UpdateStatsUI;
    m_StartButton.clicked += OnStartClicked;
    m_QuitButton.clicked += OnQuitClicked;
    m_HUD = UIDoc.rootVisualElement.Q<VisualElement>("HUD");
    UpdateStatsUI();
    
    m_GameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
    m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");


    //StartNewGame();
    m_MainMenu.style.display = DisplayStyle.Flex;
    m_HUD.style.display = DisplayStyle.None;
    m_GameOverPanel.style.display = DisplayStyle.None;
    }

    public void NewLevel()
    {
    BoardManager.Clean();
    int width = baseWidth + (m_CurrentLevel * sizeIncreasePerLevel);
    int height = baseHeight + (m_CurrentLevel * sizeIncreasePerLevel);

    BoardManager.Init(
        width,
        height,
        GetFoodCount(),
        GetWallCount(),
        GetEnemyCount()
    );
    PlayerController.Spawn(BoardManager, new Vector2Int(1,1));

    m_CurrentLevel++;
    UpdateCameraSize();
    }
   void OnTurnHappen()
    {
        // m_FoodAmount -= 1;
        // m_FoodLabel.text = "Food : " + m_FoodAmount;
        ChangeFood(-1);

    }

    void OnStartClicked()
    {
        m_MainMenu.style.display = DisplayStyle.None;
        m_HUD.style.display = DisplayStyle.Flex;
        m_GameOverPanel.style.display = DisplayStyle.None;

        StartNewGame();
    }

    void OnQuitClicked()
    {
        Application.Quit();
    }

    void UpdateStatsUI()
    {
        m_StrengthLabel.text = "Strength: " + PlayerController.Strength;
    }

    public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        if (m_FoodAmount <= 0)
        {
            PlayerController.GameOver();
            m_GameOverPanel.style.display = DisplayStyle.Flex;
            m_HUD.style.display = DisplayStyle.None;
            m_GameOverMessage.text = "Game Over!\n\nSurvived " + m_CurrentLevel + " days";
        }

    }

    public void StartNewGame()
    {
    //m_GameOverPanel.style.visibility = Visibility.Hidden;
    m_GameOverPanel.style.display = DisplayStyle.None;
    m_HUD.style.display = DisplayStyle.Flex;
    
    m_CurrentLevel = 1;
    m_FoodAmount = 20;
    m_FoodLabel.text = "Food : " + m_FoodAmount;
    
    BoardManager.Clean();
    int width = baseWidth + (m_CurrentLevel * sizeIncreasePerLevel);
    int height = baseHeight + (m_CurrentLevel * sizeIncreasePerLevel);

    BoardManager.Init(
        width,
        height,
        GetFoodCount(),
        GetWallCount(),
        GetEnemyCount()
    );
    
    PlayerController.Init();
    //PlayerController.Spawn(BoardManager, new Vector2Int(1,1));
    Vector2Int spawn = new Vector2Int(1, 1);
    PlayerController.Spawn(BoardManager, spawn);
    UpdateStatsUI();
    UpdateCameraSize();
    }


    public int GetLevel()
    {
        return m_CurrentLevel;
    }
}