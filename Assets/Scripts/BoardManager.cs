using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
   public class CellData
   {
       public bool Passable;
       public CellObject ContainedObject;

   }
   

   private CellData[,] m_BoardData;
   private Tilemap m_Tilemap;
   private Grid m_Grid;
  
//    public int Width;
//    public int Height;
    public int Width { get; private set; }
    public int Height { get; private set; }
   public Tile[] GroundTiles;
   public Tile[] WallTiles;
   public FoodObject FoodPrefab;
   private List<Vector2Int> m_EmptyCellsList;
   public WallObject WallPrefab;
   public ExitCellObject ExitCellPrefab;
   public Enemy EnemyPrefab;
   public Weapon WeaponPrefab;

    public void Init(int width, int height, int foodCount, int wallCount, int enemyCount)
    {

    Width = width;
    Height = height;
    m_Tilemap = GetComponentInChildren<Tilemap>();
    m_Grid = GetComponentInChildren<Grid>();
    //Initialize the list
    m_EmptyCellsList = new List<Vector2Int>();
    
    m_BoardData = new CellData[Width, Height];


    for (int y = 0; y < Height; ++y)
    {
        for(int x = 0; x < Width; ++x)
        {
            Tile tile;
            m_BoardData[x, y] = new CellData();
            
            if(x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
            {
                tile = WallTiles[Random.Range(0, WallTiles.Length)];
                m_BoardData[x, y].Passable = false;
            }
            else
            {
                tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                m_BoardData[x, y].Passable = true;
                
                //this is a passable empty cell, add it to the list!
                m_EmptyCellsList.Add(new Vector2Int(x, y));
            }
            
            m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        }
    }
    
    //remove the starting point of the player! It's not empty, the player is there
    m_EmptyCellsList.Remove(new Vector2Int(1, 1));

    Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
    AddObject(Instantiate(ExitCellPrefab), endCoord);
    m_EmptyCellsList.Remove(endCoord);

    // Vector2Int coord = new Vector2Int(3, 3); // temp test position
    // AddObject(Instantiate(EnemyPrefab), coord);

    for (int i = 0; i < enemyCount; i++)
    {
        int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
        Vector2Int coord = m_EmptyCellsList[randomIndex];

        m_EmptyCellsList.RemoveAt(randomIndex);
        Enemy newEnemy = Instantiate(EnemyPrefab);
        newEnemy.Damage = 1 + (GameManager.Instance.GetLevel() / 5);
        AddObject(newEnemy, coord);
    }

    int weaponCount = Mathf.Max(1, enemyCount / 2);

    for (int i = 0; i < weaponCount; i++)
    {
        if (m_EmptyCellsList.Count == 0)
            break;

        int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
        Vector2Int coord = m_EmptyCellsList[randomIndex];

        m_EmptyCellsList.RemoveAt(randomIndex);

        Weapon newWeapon = Instantiate(WeaponPrefab);
        AddObject(newWeapon, coord);
    }

    GenerateWall(wallCount);
    GenerateFood(foodCount);
    }

   public Vector3 CellToWorld(Vector2Int cellIndex)
   {
       return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
   }

   public CellData GetCellData(Vector2Int cellIndex)
   {
       if (cellIndex.x < 0 || cellIndex.x >= Width
           || cellIndex.y < 0 || cellIndex.y >= Height)
       {
           return null;
       }

       return m_BoardData[cellIndex.x, cellIndex.y];
   }


    void GenerateFood(int foodCount)
    {
    //int foodCount = 5;
    for (int i = 0; i < foodCount; ++i)
    {
        int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
        Vector2Int coord = m_EmptyCellsList[randomIndex];

        m_EmptyCellsList.RemoveAt(randomIndex);
        FoodObject newFood = Instantiate(FoodPrefab);
        AddObject(newFood, coord);
    }
    }

    void GenerateWall(int wallCount)
    {
    //int wallCount = Random.Range(6, 10);
    for (int i = 0; i < wallCount; ++i)
    {
        int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
        Vector2Int coord = m_EmptyCellsList[randomIndex];

        m_EmptyCellsList.RemoveAt(randomIndex);
        WallObject newWall = Instantiate(WallPrefab);
        AddObject(newWall, coord);
    }
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
    m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
 
    }

    void AddObject(CellObject obj, Vector2Int coord)
    {
    CellData data = m_BoardData[coord.x, coord.y];
    obj.transform.position = CellToWorld(coord);
    data.ContainedObject = obj;
    obj.Init(coord);
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x,     cellIndex.y, 0));
    }

    public void Clean()
    {
        //no board data, so exit early, nothing to clean
    if(m_BoardData == null)
        return;


    for (int y = 0; y < Height; ++y)
    {
        for (int x = 0; x < Width; ++x)
        {
            var cellData = m_BoardData[x, y];

            if (cellData.ContainedObject != null)
            {
                //CAREFUL! Destroy the GameObject NOT just cellData.ContainedObject
                //Otherwise what you are destroying is the JUST CellObject COMPONENT
                //and not the whole gameobject with sprite
                Destroy(cellData.ContainedObject.gameObject);
            }

            SetCellTile(new Vector2Int(x,y), null);
        }
    }
    }
}