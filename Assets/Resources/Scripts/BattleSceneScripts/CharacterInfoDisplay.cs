using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterInfoDisplay : MonoBehaviour
{
    public GameObject infoPanel; // ������ � �����������
    public TMP_Text nameText;
    public TMP_Text healthText;
    public TMP_Text attackText;
    public TMP_Text defenseText;
    public TMP_Text initiativeText;
    public TMP_Text actionsText;
    public Image characterImage;

    private void Start()
    {
        infoPanel.SetActive(false); // �� ��������� ������ ������
    }

    public void ShowCharacterInfo(SquadManagerNamespace.SquadManager.Member character)
    {
        if (character == null) return;

        infoPanel.SetActive(true);

        nameText.text = $"���: {character.characterName}";
        healthText.text = $"��������: {character.health}";
        attackText.text = $"�����: {character.attackPower}";
        defenseText.text = $"������: {character.defense}";
        initiativeText.text = $"����������: {character.initiative}";
        actionsText.text = $"��������: {character.actions}/{character.initialActions}";

        // ��������� ������
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

    public void HideCharacterInfo()
    {
        infoPanel.SetActive(false);
    }
}
