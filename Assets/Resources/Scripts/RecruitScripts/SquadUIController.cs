using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SquadManagerNamespace;

public class SquadUIController : MonoBehaviour
{
    [Header("UI �������� - ��������")]
    public Image[] recruitSlots;
    public Button[] recruitButtons;
    public TMP_Text moneyText;

    [Header("UI �������� - �����")]
    public GameObject[] characterSlots;
    public TMP_Text squadNameText;

    [Header("��������� ��������")]
    public int recruitmentCost = 100;
    public int maxSquadSize = 12;
    private int playerMoney = 500;

    private Dictionary<string, SquadManager.Member> availableCharacters;
    // ������� ������: ���� � ��� �����, �������� � ������ Member.
    // ���� ���� ����, �� � �������� ������������ ������, � �������� characterName == ""
    private Dictionary<string, SquadManager.Member> squadMembers;

    private string selectedSlot = null;

    void Start()
    {
        LoadCharacters();
        LoadSquadData();
        RollRecruitmentOptions();
        UpdateMovementUI();
        UpdateMoneyUI();
    }

    // ��������� ���������� �� Characters.json
    void LoadCharacters()
    {
        string path = "Assets/Resources/Characters.json"; // ������������� ����
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            availableCharacters = JsonConvert.DeserializeObject<Dictionary<string, SquadManager.Member>>(json);
        }
        else
        {
            Debug.LogError("Characters.json �� ������!");
            availableCharacters = new Dictionary<string, SquadManager.Member>();
        }
    }

    // ��������� ������ ������ �� Squad.json � ����������� ������� �� ����� � ������� Member
    void LoadSquadData()
    {
        string path = "Assets/Resources/Squad.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            // ����������� � ������ Squad, ��� members ����� ��� Dictionary<string, string>
            SquadManager.Squad squad = JsonConvert.DeserializeObject<SquadManager.Squad>(json);
            if (squad != null)
            {
                if (squadNameText != null)
                    squadNameText.text = squad.squadName;
                // ����������� Dictionary<string, string> � Dictionary<string, Member>
                squadMembers = new Dictionary<string, SquadManager.Member>();
                foreach (var kvp in squad.members)
                {
                    string slot = kvp.Key;
                    string charName = kvp.Value;
                    if (!string.IsNullOrEmpty(charName) && availableCharacters.ContainsKey(charName))
                    {
                        squadMembers.Add(slot, availableCharacters[charName]);
                    }
                    else
                    {
                        // ���� ���� ���� ��� ��� �� �������, ������ "������" ������ Member
                        squadMembers.Add(slot, new SquadManager.Member
                        {
                            characterName = "",
                            health = 0,
                            attackPower = 0,
                            defense = 0,
                            initiative = 0,
                            actions = 0,
                            initialActions = 0,
                            spritePath = ""
                        });
                    }
                }
            }
            else
            {
                Debug.LogError("�� ������� ��������������� Squad.json");
                squadMembers = new Dictionary<string, SquadManager.Member>();
            }
        }
        else
        {
            Debug.LogError("Squad.json �� ������!");
            squadMembers = new Dictionary<string, SquadManager.Member>();
        }
    }

    // ��������� ������ ������ � Squad.json (����������� ������� Member � ������� �����)
    void SaveSquadData()
    {
        string path = "Assets/Resources/Squad.json";
        Dictionary<string, string> membersToSave = new Dictionary<string, string>();
        foreach (var kvp in squadMembers)
        {
            // ���� ���� ���� (characterName ����), ��������� ������ ������
            membersToSave[kvp.Key] = string.IsNullOrEmpty(kvp.Value.characterName) ? "" : kvp.Value.characterName;
        }
        SquadManager.Squad squad = new SquadManager.Squad
        {
            squadName = squadNameText != null ? squadNameText.text : "Squad",
            members = membersToSave
        };
        File.WriteAllText(path, JsonConvert.SerializeObject(squad, Formatting.Indented));
        Debug.Log("����� ������� � " + path);
    }

    // ����������� ����������� �����
    void RollRecruitmentOptions()
    {
        List<string> recruitPool = availableCharacters.Keys.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < recruitSlots.Length; i++)
        {
            if (i < recruitPool.Count)
            {
                string key = recruitPool[i];
                if (availableCharacters.ContainsKey(key))
                {
                    SquadManager.Member character = availableCharacters[key];
                    recruitSlots[i].sprite = Resources.Load<Sprite>(character.spritePath);
                    recruitButtons[i].onClick.RemoveAllListeners();
                    recruitButtons[i].onClick.AddListener(() => RecruitCharacter(key));
                    int currentCount = squadMembers.Values.Count(m => !string.IsNullOrEmpty(m.characterName));
                    recruitButtons[i].interactable = playerMoney >= recruitmentCost && currentCount < maxSquadSize;
                }
            }
            else
            {
                recruitSlots[i].sprite = null;
                recruitButtons[i].interactable = false;
            }
        }
    }

    // ������� ��������� � ������ ������ ����
    void RecruitCharacter(string characterKey)
    {
        int currentCount = squadMembers.Values.Count(m => !string.IsNullOrEmpty(m.characterName));
        if (playerMoney < recruitmentCost || currentCount >= maxSquadSize)
            return;

        playerMoney -= recruitmentCost;

        foreach (var slot in squadMembers.Keys.ToList())
        {
            if (string.IsNullOrEmpty(squadMembers[slot].characterName))
            {
                squadMembers[slot] = availableCharacters[characterKey];
                break;
            }
        }
        UpdateMovementUI();
        UpdateMoneyUI();
        SaveSquadData();
    }

    void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = $"Money: {playerMoney}";
    }

    // ��������� UI ������ ������
    void UpdateMovementUI()
    {
        foreach (var slot in squadMembers.Keys)
        {
            GameObject slotObject = characterSlots.FirstOrDefault(s => s.name == slot);
            if (slotObject == null)
            {
                Debug.LogWarning($"������ ����� � ������ '{slot}' �� ������.");
                continue;
            }

            Image slotImage = slotObject.GetComponent<Image>();
            TMP_Text slotNameText = slotObject.GetComponentInChildren<TMP_Text>();
            Button slotButton = slotObject.GetComponent<Button>();

            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => OnMovementSlotClicked(slot));

            if (string.IsNullOrEmpty(squadMembers[slot].characterName))
            {
                slotImage.sprite = null;
                slotImage.color = selectedSlot == slot ? Color.yellow : Color.white;
                if (slotNameText != null)
                    slotNameText.text = "";
            }
            else
            {
                SquadManager.Member character = squadMembers[slot];
                slotImage.sprite = Resources.Load<Sprite>(character.spritePath);
                slotImage.color = selectedSlot == slot ? Color.yellow : Color.white;
                if (slotNameText != null)
                    slotNameText.text = character.characterName;
            }
        }
    }

    // ��������� ����� �� ����� ��� ����������� ��� ������ �����������
    void OnMovementSlotClicked(string slot)
    {
        if (selectedSlot == null)
        {
            if (!string.IsNullOrEmpty(squadMembers[slot].characterName))
            {
                selectedSlot = slot;
                UpdateMovementUI();
            }
        }
        else
        {
            if (selectedSlot == slot)
            {
                selectedSlot = null;
                UpdateMovementUI();
                return;
            }

            if (string.IsNullOrEmpty(squadMembers[slot].characterName))
            {
                squadMembers[slot] = squadMembers[selectedSlot];
                squadMembers[selectedSlot] = new SquadManager.Member
                {
                    characterName = "",
                    health = 0,
                    attackPower = 0,
                    defense = 0,
                    initiative = 0,
                    actions = 0,
                    initialActions = 0,
                    spritePath = ""
                };
            }
            else
            {
                var temp = squadMembers[selectedSlot];
                squadMembers[selectedSlot] = squadMembers[slot];
                squadMembers[slot] = temp;
            }

            selectedSlot = null;
            UpdateMovementUI();
            SaveSquadData();
        }
    }
}
