using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;  // ��� ������������� EventTrigger
using SquadManagerNamespace;

public class SquadCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string positionKey; // ���������� ���� ������ (��������, "PA1")
    private TextMeshProUGUI textDisplay;
    private Image characterImage; // UI Image ��� ������� ���������
    private Button cellButton;
    private CharacterInfoDisplay characterInfoDisplay; // ������ �� ��������� ����������� ���������� � ���������
    private TurnManager turnManager;
    private SquadManager squadManager;

    void Start()
    {
        textDisplay = GetComponentInChildren<TextMeshProUGUI>();
        characterImage = GetComponent<Image>();
        cellButton = GetComponent<Button>();
        characterInfoDisplay = FindObjectOfType<CharacterInfoDisplay>(); // ���� ������ ����������� ���������� � ���������

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
            textDisplay.text = $"{member.displayName}\n����������: {member.initiative}\n��������: {member.actions}";

            // ���� �������� ���������, ������� ����� �����
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

            // ������������ ��������� ���������
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
            // ���� ������ �����, �� ���������� ���� � ������
            textDisplay.text = "";
            characterImage.sprite = null;
            // �� ������ ���� ��� ������ ������
             characterImage.color = Color.white;  // ��� ������ ������ �� �����
        }
    }


    // �����-���������� ����� �� ������ ������
    public void OnCellClicked()
    {
        turnManager.HandleCellClick(positionKey);

        // �������� ���������� � ��������� ��� ����� �� ������
        if (squadManager.squadMembers.ContainsKey(positionKey) && squadManager.squadMembers[positionKey] != null)
        {
            var character = squadManager.squadMembers[positionKey];
            characterInfoDisplay.ShowCharacterInfo(character);
        }
    }

    // ���������� ���������� ��� ��������� ������� ����
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
