using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterInfoDisplay : MonoBehaviour
{
    public GameObject infoPanel; // Панель с информацией
    public TMP_Text nameText;
    public TMP_Text healthText;
    public TMP_Text attackText;
    public TMP_Text defenseText;
    public TMP_Text initiativeText;
    public TMP_Text actionsText;
    public Image characterImage;

    private void Start()
    {
        infoPanel.SetActive(false); // По умолчанию панель скрыта
    }

    // Метод для отображения информации о персонаже
    public void ShowCharacterInfo(SquadManagerNamespace.SquadManager.Member character)
    {
        if (character == null) return;

        infoPanel.SetActive(true);

        nameText.text = $"Имя: {character.characterName}";
        healthText.text = $"Здоровье: {character.health}";
        attackText.text = $"Атака: {character.attackPower}";
        defenseText.text = $"Защита: {character.defense}";
        initiativeText.text = $"Инициатива: {character.initiative}";
        actionsText.text = $"Действия: {character.actions}/{character.initialActions}";

        // Загружаем спрайт
        Sprite characterSprite = Resources.Load<Sprite>(character.spritePath);
        if (characterSprite != null)
        {
            characterImage.sprite = characterSprite;
            characterImage.enabled = true;
        }
        else
        {
            characterImage.enabled = false;
        }
    }

    // Метод для скрытия панели с информацией
    public void HideCharacterInfo()
    {
        infoPanel.SetActive(false);
    }
}
