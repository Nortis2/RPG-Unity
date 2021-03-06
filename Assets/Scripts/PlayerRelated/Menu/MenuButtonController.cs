using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class MenuButtonController : MonoBehaviour
{
    [SerializeField] Item itemObject;
    [SerializeField] Slime slime;
    public void ChooseOperation(string operation)
    {
        switch (operation)
        {
            case "newGame":
                NewGame();
                break;
            case "loadGame":
                LoadGame();
                break;
            case "saveGame":
                SaveGame();
                break;
        }
        this.GetComponent<Menu>().Move();
    }

    private void NewGame()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().PlayerReset();
        if(File.Exists(Application.dataPath + "/save.txt"))
        {
            File.Delete(Application.dataPath + "/save.txt");
        }
        Clean();
    }

    private void LoadGame()
    {
        if(File.Exists(Application.dataPath + "/save.txt"))
        {
            //Reseting player and his items
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().PlayerReset();
            Clean();

            string loaded = File.ReadAllText(Application.dataPath + "/save.txt");
            SaveObject load = JsonUtility.FromJson<SaveObject>(loaded);
            //Player
            GameObject.Find("Player").transform.position = load.playerPosition;
            GameObject.Find("Player").GetComponent<PlayerController>().healthSystem.SetHealth(load.playerHealth);
            GameObject.Find("Player").GetComponent<PlayerController>().healthSystem.SetMaxHealth(load.playerMaxHealth);
            GameObject.Find("Player").GetComponent<PlayerController>().levelingSystem.currentExp = load.exp;
            GameObject.Find("Player").GetComponent<PlayerController>().levelingSystem.level = load.level;
            ((Slider)GameObject.FindObjectsOfType(typeof(Slider))[0]).GetComponent<PlayerBars>().UpdateHealthBar(load.playerHealth, load.playerMaxHealth); //updating health bar
            ((Slider)GameObject.FindObjectsOfType(typeof(Slider))[0]).GetComponent<PlayerBars>().UpdateExpBar(load.exp, GameObject.Find("Player").GetComponent<PlayerController>().levelingSystem.maxExp[load.level]); //updating EXP bar

            //Items
            for(int i=0; i<load.itemAmount.Count;i++)
            {
                itemObject.GetComponent<Item>().Constructor(load.itemName[i], load.itemAmount[i], load.itemDamage[i], load.itemTag[i]);
                GameObject.Find("InventoryUI").GetComponent<Inventory>().AddItem(itemObject,false);
            }
            //Ground items
            for (int i = 0; i < load.groundItemAmount.Count; i++)
            {
                Item newGroundItem;
                newGroundItem = Instantiate(itemObject, load.groundItemPos[i], Quaternion.identity);
                newGroundItem.GetComponent<Item>().Constructor(load.groundItemName[i], load.groundItemAmount[i], load.groundItemDamage[i], load.groundItemTag[i]);
            }
            //Enemies
            for (int i = 0; i < load.enemyID.Count; i++)
            {
                foreach(GameObject spawner in GameObject.FindGameObjectsWithTag("Spawner"))
                {
                    if(spawner.GetComponent<MobSpawner>().id == load.enemyID[i])
                    {
                        spawner.GetComponent<MobSpawner>().ForceSpawn(load.enemyHealth[i],load.enemyPos[i]);
                    }
                }
            }
        }
    }

    private void SaveGame()
    {
        SaveObject saveObject = new SaveObject();
        //Player
        saveObject.playerPosition = GameObject.Find("Player").transform.position;
        saveObject.playerHealth = GameObject.Find("Player").GetComponent<PlayerController>().healthSystem.GetHealth();
        saveObject.playerMaxHealth = GameObject.Find("Player").GetComponent<PlayerController>().healthSystem.GetMaxHealth();
        saveObject.exp = GameObject.Find("Player").GetComponent<PlayerController>().levelingSystem.currentExp;
        saveObject.level = GameObject.Find("Player").GetComponent<PlayerController>().levelingSystem.level;
        //Items
        foreach(GameObject itemSlot in GameObject.Find("InventoryUI").GetComponent<Inventory>().itemSlots)
        {
            if(itemSlot.GetComponent<ItemSlot>().isOccupied())
            {
                saveObject.itemAmount.Add(itemSlot.GetComponent<ItemSlot>().item.GetAmount());
                saveObject.itemDamage.Add(itemSlot.GetComponent<ItemSlot>().item.GetDamage());
                saveObject.itemTag.Add(itemSlot.GetComponent<ItemSlot>().item.GetTag());
                saveObject.itemName.Add(itemSlot.GetComponent<ItemSlot>().item.GetName());
            }
        }

        foreach (GameObject groundItem in GameObject.FindGameObjectsWithTag("Item"))
        {
            saveObject.groundItemAmount.Add(groundItem.GetComponent<Item>().itClass.GetAmount());
            saveObject.groundItemDamage.Add(groundItem.GetComponent<Item>().itClass.GetDamage());
            saveObject.groundItemTag.Add(groundItem.GetComponent<Item>().itClass.GetTag());
            saveObject.groundItemName.Add(groundItem.GetComponent<Item>().itClass.GetName());
            saveObject.groundItemPos.Add(groundItem.transform.position);
        }

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            saveObject.enemyPos.Add(enemy.transform.position);
            saveObject.enemyHealth.Add(enemy.GetComponent<Slime>().healthSystem.GetHealth());
            saveObject.enemyID.Add(enemy.GetComponent<Slime>().id);
        }

        string save = JsonUtility.ToJson(saveObject);

        File.WriteAllText(Application.dataPath + "/save.txt", save);
    }

    private class SaveObject
    {
        //Player
        public Vector3 playerPosition;
        public int playerHealth;
        public int playerMaxHealth;
        public int level;
        public int exp;
        //Items in inventory
        public List<int> itemAmount = new List<int>();
        public List<int> itemDamage = new List<int>();
        public List<string> itemTag = new List<string>();
        public List<string> itemName = new List<string>();
        //Items on ground
        public List<Vector3> groundItemPos = new List<Vector3>();
        public List<int> groundItemAmount = new List<int>();
        public List<int> groundItemDamage = new List<int>();
        public List<string> groundItemTag = new List<string>();
        public List<string> groundItemName = new List<string>();
        //Enemies
        public List<Vector3> enemyPos = new List<Vector3>();
        public List<int> enemyHealth = new List<int>();
        public List<int> enemyID = new List<int>();
    }

    private void Clean()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if(enemy.name == "Slime(Clone)")
            {
                enemy.GetComponent<Slime>().GetSpawner().GetComponent<MobSpawner>().ReduceCounter();
            }
            Destroy(enemy);
        }

        foreach (GameObject item in GameObject.FindGameObjectsWithTag("Item"))
        {
            Destroy(item);
        }
    }
}
