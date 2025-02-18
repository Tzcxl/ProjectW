using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SquadManagerNamespace.SquadManager.Member character;
    private CharacterInfoDisplay infoDisplay;

    private void Start()
    {
        infoDisplay = FindObjectOfType<CharacterInfoDisplay>(); // ����� ��������� �� �����
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (character != null)
            infoDisplay.ShowCharacterInfo(character);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        infoDisplay.HideCharacterInfo();
    }
}
