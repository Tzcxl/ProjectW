using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SquadManagerNamespace;
using System.IO;
using Newtonsoft.Json;

public class Swaper : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject[] characterSlots = new GameObject[12]; // 12 ������ ��� ����������
    [SerializeField] private TMP_Text squadNameText; // �������� ������

    // ������ ���������� ��������� (���� �� ���� �� ������, = -1)
    private int selectedIndex = -1;

    // ������ ����������; ���� ���� ������ � �������� null.
    private List<SquadManagerNamespace.SquadManager.Member> squadMembers = new List<SquadManagerNamespace.SquadManager.Member>();

    void Start()
    {
        // �������� ������ �� SquadManager � ��������� �����
        SquadManager squadManager = FindObjectOfType<SquadManager>();
        if (squadManager == null)
        {
            Debug.LogError("SquadManager �� ������!");
            return;
        }

        squadMembers = new List<SquadManagerNamespace.SquadManager.Member>(squadManager.squadMembers.Values); // ��������� ����� ������ �� �������

        // �����������, ��� ����� ������ ����� ���������� ������
        AdjustSquadMembersList();

        // ��������� UI ��� ����������� ������
        UpdateSquadUI();
    }

    /// <summary>
    /// ������������ ������ squadMembers, �������� ��� ������ ��� �� ����� ������.
    /// </summary>
    void AdjustSquadMembersList()
    {
        while (squadMembers.Count < characterSlots.Length)
            squadMembers.Add(null);
        while (squadMembers.Count > characterSlots.Length)
            squadMembers.RemoveAt(squadMembers.Count - 1);
    }

    /// <summary>
    /// ��������� ����������� ���� ������:
    /// - ���� � ����� ���� �������� � ��������� ��� ������ � ���.
    /// - ���� ���� ������ (selectedIndex) � �������������� �����.
    /// - ��� ������� ����� ����������� ���������� �����.
    /// </summary>
    void UpdateSquadUI()
    {
        for (int i = 0; i < characterSlots.Length; i++)
        {
            characterSlots[i].SetActive(true);
            Image slotImage = characterSlots[i].GetComponent<Image>();
            TMP_Text slotNameText = characterSlots[i].GetComponentInChildren<TMP_Text>();
            Button slotButton = characterSlots[i].GetComponent<Button>();

            // ��������� ���������� ����� � ��������� �������
            slotButton.onClick.RemoveAllListeners();
            int index = i;
            slotButton.onClick.AddListener(() => OnSlotClicked(index));

            // ���� ���� ������
            if (squadMembers[i] == null)
            {
                if (slotImage != null)
                {
                    slotImage.sprite = null;
                    slotImage.color = (selectedIndex == i) ? Color.yellow : Color.white;
                }
                if (slotNameText != null)
                    slotNameText.text = "";
            }
            else
            {
                // ���� � ����� ���� ��������, ��������� ��� ������ � ���
                SquadManagerNamespace.SquadManager.Member character = squadMembers[i];
                Sprite characterSprite = Resources.Load<Sprite>(character.spritePath);
                if (slotImage != null)
                {
                    slotImage.sprite = characterSprite;
                    slotImage.color = (selectedIndex == i) ? Color.yellow : Color.white;
                }
                if (slotNameText != null)
                    slotNameText.text = $"Name: {character.characterName}";
            }
        }
    }

    /// <summary>
    /// ���������� ����� �� �����.
    /// ���� �� ���� �������� �� ������, ��� ����� �� ������� ������ �������� ���������.
    /// ���� �������� ��� ������, �� ���� �� ������� ����� �������� � ����������� (���� ������ �����)
    /// ��� ������ (swap) � ���������� � ���� ������.
    /// ����� ���������� �������� ����������� UI � ����� �����������.
    /// </summary>
    /// <param name="index">������ �������� �����</param>
    void OnSlotClicked(int index)
    {
        // ���� ��� �� ������ �������� ��� �����������
        if (selectedIndex == -1)
        {
            // ���� � ����� ���� ��������, �������� ���
            if (squadMembers[index] != null)
            {
                selectedIndex = index;
                UpdateSquadUI();
            }
            // ���� ������ �� ������ ���� ��� ���������� ��������� � ������ �� ������
        }
        else
        {
            // ���� �������� �� ���� �� �����, ���������� ���������
            if (selectedIndex == index)
            {
                selectedIndex = -1;
                UpdateSquadUI();
                return;
            }

            // ���� ������ �������� � �������� �� ������� �����:
            if (squadMembers[index] == null)
            {
                // ���������� ���������� ��������� � ������ ����
                MoveCharacter(selectedIndex, index);
            }
            else
            {
                // ���� � ����� ���� ��������, ������ �� ������� (swap)
                SwapCharacters(selectedIndex, index);
            }

            // ���������� ��������� ����� ��������
            selectedIndex = -1;
            UpdateSquadUI();
            SaveSquadData();
        }
    }

    /// <summary>
    /// ���������� ��������� �� ����� fromIndex � ������ ���� toIndex.
    /// </summary>
    void MoveCharacter(int fromIndex, int toIndex)
    {
        squadMembers[toIndex] = squadMembers[fromIndex];
        squadMembers[fromIndex] = null;
    }

    /// <summary>
    /// ������ ������� ���������� ����� ������� index1 � index2.
    /// </summary>
    void SwapCharacters(int index1, int index2)
    {
        SquadManagerNamespace.SquadManager.Member temp = squadMembers[index1];
        squadMembers[index1] = squadMembers[index2];
        squadMembers[index2] = temp;
    }

    /// <summary>
    /// ��������� ������ ������ � JSON-����.
    /// ������ ����������� ����� ���������� ��� ������� �� 12 ������.
    /// </summary>
    void SaveSquadData()
    {
        SquadManager squadManager = FindObjectOfType<SquadManager>();
        if (squadManager == null)
        {
            Debug.LogError("SquadManager �� ������!");
            return;
        }

        // ���� ��� ���������� (��������, � Application.persistentDataPath)
        string squadJsonPath = Application.persistentDataPath + "/Squad.json";
        List<string> squadData = new List<string>();

        foreach (var member in squadMembers)
        {
            squadData.Add(member != null ? member.characterName : null);
        }

        // ��������� ������ ��� ���������� (����� ��������� �� �������������)
        var saveObj = new
        {
            squadName = squadNameText != null ? squadNameText.text : "Squad",
            members = squadData
        };

        File.WriteAllText(squadJsonPath, JsonConvert.SerializeObject(saveObj, Formatting.Indented));
        Debug.Log("����� ������� � " + squadJsonPath);
    }
}
