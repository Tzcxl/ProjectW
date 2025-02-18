using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;  // Для использования EventTrigger
using SquadManagerNamespace;

public class SquadCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string positionKey; // Уникальный ключ клетки (например, "PA1")
    private TextMeshProUGUI textDisplay;
    private Image characterImage; // UI Image для спрайта персонажа
    private Button cellButton;
    private CharacterInfoDisplay characterInfoDisplay; // Ссылка на компонент отображения информации о персонаже
    private TurnManager turnManager;
    private SquadManager squadManager;

    void Start()
    {
        textDisplay = GetComponentInChildren<TextMeshProUGUI>();
        characterImage = GetComponent<Image>();
        cellButton = GetComponent<Button>();
        characterInfoDisplay = FindObjectOfType<CharacterInfoDisplay>(); // Ищем объект отображения информации о персонаже

        if (cellButton != null)
        {
            cellButton.onClick.AddListener(OnCellClicked);
        }

        squadManager = FindObjectOfType<SquadManager>();
        turnManager = FindObjectOfType<TurnManager>();

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (squadManager == null || textDisplay == null || characterImage == null)
            return;

        var squadMembers = squadManager.squadMembers;
        if (squadMembers.ContainsKey(positionKey) && squadMembers[positionKey] != null)
        {
            var member = squadMembers[positionKey];
            textDisplay.text = $"{member.displayName}\nИнициатива: {member.initiative}\nДействия: {member.actions}";

            // Если персонаж неактивен, выводим текст серым
            if (member.inactive)
            {
                textDisplay.color = Color.gray;
            }
            else
            {
                textDisplay.color = Color.white;
            }

            if (!string.IsNullOrEmpty(member.spritePath))
            {
                Sprite characterSprite = Resources.Load<Sprite>(member.spritePath);
                if (characterSprite != null)
                {
                    characterImage.sprite = characterSprite;
                }
            }

            // Подсвечиваем активного персонажа
            if (turnManager.IsCurrentCharacter(positionKey))
            {
                characterImage.color = Color.yellow;
            }
            else
            {
                characterImage.color = Color.white;
            }
        }
        else
        {
            // Если клетка пуста, не сбрасывать цвет и спрайт
            textDisplay.text = "";
            characterImage.sprite = null;
            // Не меняем цвет для пустой клетки
             characterImage.color = Color.white;  // Эта строка теперь не нужна
        }
    }


    // Метод-обработчик клика по кнопке клетки
    public void OnCellClicked()
    {
        turnManager.HandleCellClick(positionKey);

        // Показать информацию о персонаже при клике на клетку
        if (squadManager.squadMembers.ContainsKey(positionKey) && squadManager.squadMembers[positionKey] != null)
        {
            var character = squadManager.squadMembers[positionKey];
            characterInfoDisplay.ShowCharacterInfo(character);
        }
    }

    // Реализация интерфейса для обработки событий мыши
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (squadManager.squadMembers.ContainsKey(positionKey) && squadManager.squadMembers[positionKey] != null)
        {
            var character = squadManager.squadMembers[positionKey];
            characterInfoDisplay.ShowCharacterInfo(character);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        characterInfoDisplay.HideCharacterInfo();
    }
}
