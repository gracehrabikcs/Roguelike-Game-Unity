using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

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
    //private Button m_QuitButton;
    private VisualElement m_HUD;

    private VisualElement m_PauseMenu;
    private Button m_ResumeButton;
    private Button m_MainMenuButton;
    //private Button m_PauseQuitButton;
    private Label m_LevelLabel;
    public bool IsGameOver { get; private set; }
    private bool m_IsPaused;
    public bool IsPaused => m_IsPaused;

    void UpdateCameraSize()
    {
        float size = Mathf.Max(BoardManager.Width, BoardManager.Height) * 0.6f;
        MainCamera.orthographicSize = Mathf.Clamp(size, 4f, 12f);
    }


    public int GetFoodCount()
    {
        return Mathf.Max(1, baseFood - ((m_CurrentLevel - 1) * foodDecreasePerLevel));
    }

    public int GetWallCount()
    {
        return baseWalls + ((m_CurrentLevel - 1) * wallIncreasePerLevel);
    }

    public int GetEnemyCount()
    {
        return baseEnemies + ((m_CurrentLevel - 1) * enemyIncreasePerLevel);
    }

    void Update()
    {
       if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (m_MainMenu.style.display == DisplayStyle.Flex)
                return; // don't pause in main menu

            if (m_IsPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        m_IsPaused = true;

        m_PauseMenu.style.display = DisplayStyle.Flex;
        m_HUD.style.display = DisplayStyle.None;

        Time.timeScale = 0f;
    }

    void ResumeGame()
    {
        m_IsPaused = false;

        m_PauseMenu.style.display = DisplayStyle.None;
        m_HUD.style.display = DisplayStyle.Flex;

        Time.timeScale = 1f;
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
        // =========================
        // UI ROOT
        // =========================
        var root = UIDoc.rootVisualElement;

        m_MainMenu = root.Q<VisualElement>("MainMenu");
        m_HUD = root.Q<VisualElement>("HUD");
        m_GameOverPanel = root.Q<VisualElement>("GameOverPanel");
        m_PauseMenu = root.Q<VisualElement>("PauseMenu");

        if (m_MainMenu == null || m_HUD == null || m_GameOverPanel == null || m_PauseMenu == null)
        {
            Debug.LogError("One or more UI panels are missing from UI Document.");
        }

        // =========================
        // LABELS
        // =========================
        m_FoodLabel = root.Q<Label>("FoodLabel");
        m_StrengthLabel = root.Q<Label>("StrengthLabel");
        m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");
        m_LevelLabel = root.Q<Label>("LevelLabel");

        // =========================
        // BUTTONS (MAIN MENU)
        // =========================
        m_StartButton = root.Q<Button>("StartButton");
        //m_QuitButton = root.Q<Button>("QuitButton");

        // =========================
        // BUTTONS (PAUSE MENU)
        // =========================
        m_ResumeButton = m_PauseMenu.Q<Button>("ResumeButton");
        m_MainMenuButton = m_PauseMenu.Q<Button>("MainMenuButton");
        //m_PauseQuitButton = m_PauseMenu.Q<Button>("PauseQuitButton");

        // =========================
        // TURN SYSTEM
        // =========================
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;

        // =========================
        // EVENTS
        // =========================
        PlayerController.OnStatsChanged += UpdateStatsUI;

        m_StartButton.clicked += OnStartClicked;
        //m_QuitButton.clicked += OnQuitClicked;

        m_ResumeButton.clicked += ResumeGame;
        m_MainMenuButton.clicked += ReturnToMainMenu;
        //m_PauseQuitButton.clicked += OnQuitClicked;

        // =========================
        // INITIAL UI STATE
        // =========================
        m_MainMenu.style.display = DisplayStyle.Flex;
        m_HUD.style.display = DisplayStyle.None;
        m_GameOverPanel.style.display = DisplayStyle.None;
        m_PauseMenu.style.display = DisplayStyle.None;

        Debug.Log("HUD: " + m_HUD);
        Debug.Log("GameOverPanel: " + m_GameOverPanel);
        Debug.Log("GameOverMessage: " + m_GameOverMessage);

        if (m_GameOverPanel == null)
        {
            Debug.LogError("GameOverPanel not found in UI Document!");
            return;
        }

        //Debug.Log($"PauseQuitButton: {m_PauseQuitButton}");

        // =========================
        // INIT UI VALUES
        // =========================
        UpdateStatsUI();
    }

    public void NewLevel()
    {
        m_CurrentLevel++;

        BoardManager.Clean();

        int width = baseWidth + ((m_CurrentLevel - 1) * sizeIncreasePerLevel);
        int height = baseHeight + ((m_CurrentLevel - 1) * sizeIncreasePerLevel);

        BoardManager.Init(
            width,
            height,
            GetFoodCount(),
            GetWallCount(),
            GetEnemyCount()
        );

        PlayerController.Spawn(BoardManager, new Vector2Int(1,1));

        UpdateLevelUI();
        UpdateCameraSize();
    }
   void OnTurnHappen()
    {
        if (IsGameOver)
            return;
        if (PlayerController == null)
        return;

        ChangeFood(-1);
    }

    void OnStartClicked()
    {
        m_MainMenu.style.display = DisplayStyle.None;
        m_HUD.style.display = DisplayStyle.Flex;
        m_GameOverPanel.style.display = DisplayStyle.None;

        StartNewGame();
    }

    void UpdateLevelUI()
    {
        if (m_LevelLabel != null)
            m_LevelLabel.text = "Level: " + m_CurrentLevel;
    }

    // void OnQuitClicked()
    // {
    //     Application.Quit();
    // }

    void UpdateStatsUI()
    {
        m_StrengthLabel.text = "Strength: " + PlayerController.Strength;
    }

    void ReturnToMainMenu()
    {
        ResumeGame(); // unpause first

        m_PauseMenu.style.display = DisplayStyle.None;
        m_HUD.style.display = DisplayStyle.None;
        m_GameOverPanel.style.display = DisplayStyle.None;

        m_MainMenu.style.display = DisplayStyle.Flex;
    }
    // public void ChangeFood(int amount)
    // {
    //     m_FoodAmount += amount;
    //     m_FoodLabel.text = "Food : " + m_FoodAmount;

    //     if (m_FoodAmount <= 0)
    //     {
    //         PlayerController.GameOver();
    //         m_GameOverPanel.style.display = DisplayStyle.Flex;
    //         m_HUD.style.display = DisplayStyle.None;
    //         m_GameOverMessage.text = "Game Over!\n\nSurvived " + m_CurrentLevel + " days";
    //     }

    // }

    public void ChangeFood(int amount)
    {
        if (IsGameOver)
            return;

        m_FoodAmount += amount;

        if (m_FoodLabel != null)
            m_FoodLabel.text = "Food : " + m_FoodAmount;

        if (m_FoodAmount <= 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        PlayerController.GameOver();

        if (m_GameOverPanel != null)
            m_GameOverPanel.style.display = DisplayStyle.Flex;

        if (m_HUD != null)
            m_HUD.style.display = DisplayStyle.None;

        if (m_GameOverMessage != null)
            m_GameOverMessage.text =
                "Game Over!\n\nSurvived " + m_CurrentLevel + " days";

        if (TurnManager != null)
            TurnManager.OnTick -= OnTurnHappen;
    }

    public void StartNewGame()
    {
        IsGameOver = false;

        TurnManager.OnTick -= OnTurnHappen;
        TurnManager.OnTick += OnTurnHappen;
    //m_GameOverPanel.style.visibility = Visibility.Hidden;
    m_GameOverPanel.style.display = DisplayStyle.None;
    m_HUD.style.display = DisplayStyle.Flex;
    
    m_CurrentLevel = 1;
    m_FoodAmount = 20;
    m_FoodLabel.text = "Food : " + m_FoodAmount;
    
    BoardManager.Clean();
    int width = baseWidth + ((m_CurrentLevel - 1) * sizeIncreasePerLevel);
    int height = baseHeight + ((m_CurrentLevel - 1) * sizeIncreasePerLevel);

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


    UpdateLevelUI();
    UpdateStatsUI();
    UpdateCameraSize();
    }


    public int GetLevel()
    {
        return m_CurrentLevel;
    }
}