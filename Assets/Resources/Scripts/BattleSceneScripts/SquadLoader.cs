using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using SquadManagerNamespace;

public class SquadLoader : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject[] characterSlots = new GameObject[12];
    public TMP_Text squadNameText;

    private int lastCheckedIndex = 0;
    private List<SquadManager.Member> squadMembers = new List<SquadManager.Member>();
    private Dictionary<string, SquadManager.Member> allCharacters;
    private List<(int initiative, int actions, int index, SquadManager.Member character)> sortedInitiative = new List<(int, int, int, SquadManager.Member)>();
    public int currentTurnIndex = 0;

    void Start()
    {
        LoadCharacterDatabase();
        LoadSquadData();
        AdjustSquadMembersList();
        InitializeTurnOrder();
        UpdateSquadUI();
    }

    void LoadCharacterDatabase()
    {
        string path = "Assets/Resources/Characters.json"; // Путь относительно папки Assets/Resources
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            allCharacters = JsonConvert.DeserializeObject<Dictionary<string, SquadManager.Member>>(json);
        }
        else
        {
            Debug.LogError("Characters.json not found!");
            allCharacters = new Dictionary<string, SquadManager.Member>();
        }
    }

    void LoadSquadData()
    {
        string path = "Assets/Resources/Squad.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            // Используем правильный класс для десериализации
            var squadData = JsonConvert.DeserializeObject<SquadManager.Squad>(json);

            if (squadData == null)
            {
                Debug.LogError("Failed to deserialize Squad data!");
                return;
            }

            squadNameText.text = squadData.squadName;
            squadMembers.Clear();

            // Порядок обработки слотов
            string[] slotOrder = {
            "PA1", "PA2", "PA3", "PA4",
            "PB1", "PB2", "PB3", "PB4",
            "PC1", "PC2", "PC3", "PC4"
        };

            foreach (string slotKey in slotOrder)
            {
                if (squadData.members.TryGetValue(slotKey, out string characterName)
                    && !string.IsNullOrEmpty(characterName))
                {
                    if (allCharacters.TryGetValue(characterName, out SquadManager.Member character))
                    {
                        squadMembers.Add(character);
                    }
                    else
                    {
                        Debug.LogWarning($"Character '{characterName}' not found in database!");
                        squadMembers.Add(null);
                    }
                }
                else
                {
                    squadMembers.Add(null);
                }
            }
        }
        else
        {
            Debug.LogError("Squad.json not found!");
        }
    }


    void AdjustSquadMembersList()
    {
        while (squadMembers.Count < characterSlots.Length)
            squadMembers.Add(null);
        while (squadMembers.Count > characterSlots.Length)
            squadMembers.RemoveAt(squadMembers.Count - 1);
    }

    void InitializeTurnOrder()
    {
        sortedInitiative.Clear();
        for (int i = 0; i < squadMembers.Count; i++)
        {
            if (squadMembers[i] != null && squadMembers[i].actions > 0)
                sortedInitiative.Add((squadMembers[i].initiative, squadMembers[i].actions, i, squadMembers[i]));
        }
        sortedInitiative.Sort((x, y) => y.initiative.CompareTo(x.initiative));
        currentTurnIndex = 0;
    }

    void UpdateSquadUI()
    {
        for (int i = 0; i < characterSlots.Length; i++)
        {
            characterSlots[i].SetActive(true);
            Image slotImage = characterSlots[i].GetComponent<Image>();
            TMP_Text slotNameText = characterSlots[i].GetComponentInChildren<TMP_Text>();
            Button slotButton = characterSlots[i].GetComponent<Button>();

            if (i >= squadMembers.Count || squadMembers[i] == null)
            {
                if (slotImage != null) slotImage.sprite = null;
                if (slotNameText != null) slotNameText.text = "";
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => OnSlotClicked(i));
                continue;
            }

            SquadManager.Member character = squadMembers[i];
            Sprite characterSprite = Resources.Load<Sprite>(character.spritePath);
            if (slotImage != null) slotImage.sprite = characterSprite;
            if (slotNameText != null) slotNameText.text = $"{character.characterName}\nInit: {character.initiative}\nAct: {character.actions}";
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => OnSlotClicked(i));
            slotImage.color = (sortedInitiative.Count > 0 && currentTurnIndex < sortedInitiative.Count && i == sortedInitiative[currentTurnIndex].index) ? Color.yellow : Color.white;
        }
    }

    void OnSlotClicked(int index)
    {
        if (index < 0 || index >= characterSlots.Length) return;
        lastCheckedIndex = index;
        if (sortedInitiative.Count == 0) return;

        int activeSlot = sortedInitiative[currentTurnIndex].index;
        if (index == activeSlot)
        {
            squadMembers[index].actions--;
        }
        else if (squadMembers[index] == null)
        {
            squadMembers[index] = squadMembers[activeSlot];
            squadMembers[activeSlot] = null;
            squadMembers[index].actions--;
        }
        else
        {
            SquadManager.Member temp = squadMembers[index];
            squadMembers[index] = squadMembers[activeSlot];
            squadMembers[activeSlot] = temp;
            squadMembers[index].actions--;
        }
        SaveSquadData();
        CheckIfCurrentTurnFinished();
    }

    void CheckIfCurrentTurnFinished()
    {
        if (sortedInitiative.Count == 0 || squadMembers[sortedInitiative[currentTurnIndex].index].actions <= 0)
            NextTurn();
        UpdateSquadUI();
    }

    void NextTurn()
    {
        currentTurnIndex = (currentTurnIndex + 1) % sortedInitiative.Count;
        UpdateSquadUI();
    }

    void SaveSquadData()
    {
        string squadJsonPath = "Assets/Resources/Squad.json"; // Путь для сохранения в Assets/Resources

        // Создаем новый объект с именем отряда и членами
        var squadData = new SquadData
        {
            squadName = squadNameText.text,
            members = squadMembers
                .Select((member, index) => new { Slot = GetSlotByIndex(index), Member = member })
                .Where(x => x.Member != null)
                .ToDictionary(x => x.Slot, x => x.Member.characterName)
        };

        File.WriteAllText(squadJsonPath, JsonConvert.SerializeObject(squadData, Formatting.Indented));
    }

    string GetSlotByIndex(int index)
    {
        // В данном случае вы можете настроить свои слоты для индексов
        // Для примера вернем "PA1", "PA2" и так далее.
        return index < 4 ? $"PA{index + 1}" :
               index < 8 ? $"PB{index - 4 + 1}" :
               $"PC{index - 8 + 1}";
    }
}

[System.Serializable]
public class SquadData
{
    public string squadName;
    public Dictionary<string, string> members; // Словарь с именами слотов и персонажей
}
