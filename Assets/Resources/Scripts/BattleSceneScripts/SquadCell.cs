using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SquadManagerNamespace;

public class SquadCell : MonoBehaviour
{
    public string positionKey; // Уникальный ключ клетки (например, "PA1")
    private TextMeshProUGUI textDisplay;
    private Image characterImage; // UI Image для спрайта персонажа
    private TurnManager turnManager;
    private SquadManager squadManager;
    private Button cellButton;

    void Start()
    {
        textDisplay = GetComponentInChildren<TextMeshProUGUI>();
        characterImage = GetComponent<Image>();
        cellButton = GetComponent<Button>();

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
        }
        else
        {
            textDisplay.text = "";
            characterImage.sprite = null;
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

    // Метод-обработчик клика по кнопке клетки
    public void OnCellClicked()
    {
        turnManager.HandleCellClick(positionKey);
    }
}
