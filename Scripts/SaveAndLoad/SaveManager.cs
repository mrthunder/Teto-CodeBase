using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;


public class SaveManager : Singleton<SaveManager>
{

    const string filePath = "/teto.save";

    public Game m_currentGame = new Game();

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start()
    {
        
        
    }

   


    public void SaveGame()
    {
       
            FileStream file = new FileStream(Application.persistentDataPath + filePath, FileMode.Create);
            BinaryFormatter binary = new BinaryFormatter();
            binary.Serialize(file, m_currentGame);
            file.Close();
       
    }

    public bool LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + filePath))
        {
            FileStream file = new FileStream(Application.persistentDataPath + filePath, FileMode.Open);
            BinaryFormatter binary = new BinaryFormatter();
            Game loadedGame = binary.Deserialize(file) as Game;
            file.Close();
            if (loadedGame!=null)
            {
                m_currentGame = loadedGame;
                return true;
            }
          
        }
        return false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
       
    }

    public void OnApplicationQuit()
    {
        SaveGame();
    }


}
