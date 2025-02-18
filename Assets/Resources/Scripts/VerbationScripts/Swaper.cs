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
    [SerializeField] private GameObject[] characterSlots = new GameObject[12]; // 12 слотов для персонажей
    [SerializeField] private TMP_Text squadNameText; // Название отряда

    // Индекс выбранного персонажа (если ни один не выбран, = -1)
    private int selectedIndex = -1;

    // Список персонажей; если слот пустой – значение null.
    private List<SquadManagerNamespace.SquadManager.Member> squadMembers = new List<SquadManagerNamespace.SquadManager.Member>();

    void Start()
    {
        // Получаем ссылку на SquadManager и загружаем отряд
        SquadManager squadManager = FindObjectOfType<SquadManager>();
        if (squadManager == null)
        {
            Debug.LogError("SquadManager не найден!");
            return;
        }

        squadMembers = new List<SquadManagerNamespace.SquadManager.Member>(squadManager.squadMembers.Values); // Извлекаем члены отряда из словаря

        // Гарантируем, что длина списка равна количеству слотов
        AdjustSquadMembersList();

        // Обновляем UI для отображения отряда
        UpdateSquadUI();
    }

    /// <summary>
    /// Корректирует список squadMembers, дополняя или усекая его до числа слотов.
    /// </summary>
    void AdjustSquadMembersList()
    {
        while (squadMembers.Count < characterSlots.Length)
            squadMembers.Add(null);
        while (squadMembers.Count > characterSlots.Length)
            squadMembers.RemoveAt(squadMembers.Count - 1);
    }

    /// <summary>
    /// Обновляет отображение всех слотов:
    /// - Если в слоте есть персонаж – выводится его спрайт и имя.
    /// - Если слот выбран (selectedIndex) – подсвечивается жёлтым.
    /// - Для каждого слота назначается обработчик клика.
    /// </summary>
    void UpdateSquadUI()
    {
        for (int i = 0; i < characterSlots.Length; i++)
        {
            characterSlots[i].SetActive(true);
            Image slotImage = characterSlots[i].GetComponent<Image>();
            TMP_Text slotNameText = characterSlots[i].GetComponentInChildren<TMP_Text>();
            Button slotButton = characterSlots[i].GetComponent<Button>();

            // Назначаем обработчик клика с передачей индекса
            slotButton.onClick.RemoveAllListeners();
            int index = i;
            slotButton.onClick.AddListener(() => OnSlotClicked(index));

            // Если слот пустой
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
                // Если в слоте есть персонаж, загружаем его спрайт и имя
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
    /// Обработчик клика по слоту.
    /// Если ни один персонаж не выбран, при клике по занятой клетке выбираем персонажа.
    /// Если персонаж уже выбран, то клик по другому слоту приводит к перемещению (если клетка пуста)
    /// или обмену (swap) с персонажем в этой клетке.
    /// После выполнения действия обновляется UI и отряд сохраняется.
    /// </summary>
    /// <param name="index">Индекс нажатого слота</param>
    void OnSlotClicked(int index)
    {
        // Если ещё не выбран персонаж для перемещения
        if (selectedIndex == -1)
        {
            // Если в слоте есть персонаж, выбираем его
            if (squadMembers[index] != null)
            {
                selectedIndex = index;
                UpdateSquadUI();
            }
            // Если нажали на пустой слот без выбранного персонажа – ничего не делаем
        }
        else
        {
            // Если кликнули по тому же слоту, сбрасываем выделение
            if (selectedIndex == index)
            {
                selectedIndex = -1;
                UpdateSquadUI();
                return;
            }

            // Если выбран персонаж и кликнули по другому слоту:
            if (squadMembers[index] == null)
            {
                // Перемещаем выбранного персонажа в пустой слот
                MoveCharacter(selectedIndex, index);
            }
            else
            {
                // Если в слоте есть персонаж, меняем их местами (swap)
                SwapCharacters(selectedIndex, index);
            }

            // Сбрасываем выделение после действия
            selectedIndex = -1;
            UpdateSquadUI();
            SaveSquadData();
        }
    }

    /// <summary>
    /// Перемещает персонажа из слота fromIndex в пустой слот toIndex.
    /// </summary>
    void MoveCharacter(int fromIndex, int toIndex)
    {
        squadMembers[toIndex] = squadMembers[fromIndex];
        squadMembers[fromIndex] = null;
    }

    /// <summary>
    /// Меняет местами персонажей между слотами index1 и index2.
    /// </summary>
    void SwapCharacters(int index1, int index2)
    {
        SquadManagerNamespace.SquadManager.Member temp = squadMembers[index1];
        squadMembers[index1] = squadMembers[index2];
        squadMembers[index2] = temp;
    }

    /// <summary>
    /// Сохраняет данные отряда в JSON-файл.
    /// Отряду сохраняются имена персонажей для каждого из 12 слотов.
    /// </summary>
    void SaveSquadData()
    {
        SquadManager squadManager = FindObjectOfType<SquadManager>();
        if (squadManager == null)
        {
            Debug.LogError("SquadManager не найден!");
            return;
        }

        // Путь для сохранения (например, в Application.persistentDataPath)
        string squadJsonPath = Application.persistentDataPath + "/Squad.json";
        List<string> squadData = new List<string>();

        foreach (var member in squadMembers)
        {
            squadData.Add(member != null ? member.characterName : null);
        }

        // Формируем объект для сохранения (можно расширить по необходимости)
        var saveObj = new
        {
            squadName = squadNameText != null ? squadNameText.text : "Squad",
            members = squadData
        };

        File.WriteAllText(squadJsonPath, JsonConvert.SerializeObject(saveObj, Formatting.Indented));
        Debug.Log("Отряд сохранён в " + squadJsonPath);
    }
}
