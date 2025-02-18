using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SquadManagerNamespace;

public class TurnManager : MonoBehaviour
{
    private SquadManager squadManager;
    private List<string> turnOrder = new List<string>(); // —писок позиций, где наход€тс€ персонажи (только активные)
    private int currentTurnIndex = 0;

    void Start()
    {
        squadManager = FindObjectOfType<SquadManager>();
        CalculateTurnOrder();
    }

    // ѕересчитываем пор€док ходов, учитыва€ только персонажей, у которых !inactive
    public void CalculateTurnOrder()
    {
        turnOrder = squadManager.squadMembers
            .Where(kvp => kvp.Value != null && !kvp.Value.inactive)
            .OrderByDescending(kvp => kvp.Value.initiative)
            .Select(kvp => kvp.Key)
            .ToList();
        currentTurnIndex = 0;
        UpdateCells();
    }

    // ¬озвращает true, если клетка с данным ключом €вл€етс€ активной (текущий ход)
    public bool IsCurrentCharacter(string positionKey)
    {
        if (turnOrder.Count == 0)
            return false;
        return turnOrder[currentTurnIndex] == positionKey;
    }

    // ќбработка клика по клетке; вызываетс€ из SquadCell
    public void HandleCellClick(string clickedPosition)
    {
        var squadMembers = squadManager.squadMembers;
        if (turnOrder.Count == 0)
            return;

        string activePosition = turnOrder[currentTurnIndex];
        SquadManager.Member activeMember = squadMembers[activePosition];

        // ≈сли активного персонажа нет, либо он уже не может ходить, переходим к следующему
        if (activeMember == null || activeMember.inactive || activeMember.actions <= 0)
        {
            NextTurn();
            return;
        }

        // ≈сли клик по активной клетке Ц персонаж тратит 1 действие
        if (activePosition == clickedPosition)
        {
            activeMember.actions = Mathf.Max(0, activeMember.actions - 1);
            activeMember.CheckInactive();
            if (activeMember.actions == 0)
            {
                NextTurn();
            }
            squadManager.SaveSquadData();
            UpdateCells();
            CheckRoundComplete();
            return;
        }

        // ≈сли клетка с кликом отсутствует в данных, ничего не делаем
        if (!squadMembers.ContainsKey(clickedPosition))
            return;

        // ≈сли целева€ клетка пуста Ц перемещаем активного персонажа туда
        if (squadMembers[clickedPosition] == null)
        {
            squadMembers[clickedPosition] = activeMember;
            squadMembers[activePosition] = null;
            turnOrder[currentTurnIndex] = clickedPosition; // ќбновл€ем позицию активного персонажа
            activeMember.actions = Mathf.Max(0, activeMember.actions - 1);
            activeMember.CheckInactive();
        }
        else
        {
            // ≈сли целева€ клетка зан€та Ц мен€ем местами активного персонажа с персонажем в целевой клетке
            SquadManager.Member targetMember = squadMembers[clickedPosition];
            squadMembers[clickedPosition] = activeMember;
            squadMembers[activePosition] = targetMember;

            // ќбновл€ем позиции обоих персонажей в turnOrder
            int activeMemberIndex = turnOrder.IndexOf(activePosition);
            int targetMemberIndex = turnOrder.IndexOf(clickedPosition);

            // ѕеремещаем индексы в turnOrder
            turnOrder[activeMemberIndex] = clickedPosition;
            turnOrder[targetMemberIndex] = activePosition;

            // ѕосле изменени€ местами нужно обновить текущий индекс активного персонажа
            currentTurnIndex = activeMemberIndex;

            activeMember.actions = Mathf.Max(0, activeMember.actions - 1);
            activeMember.CheckInactive();
        }

        // ≈сли после выполнени€ действи€ у активного персонажа действий 0, переходим к следующему
        if (activeMember.actions == 0)
        {
            NextTurn();
        }

        squadManager.SaveSquadData();
        UpdateCells();
        CheckRoundComplete();
    }

    // ѕереход к следующему персонажу (пропускаем тех, у кого 0 действий или inactive = true)
    void NextTurn()
    {
        if (turnOrder.Count == 0)
            return;

        int startIndex = currentTurnIndex;
        do
        {
            currentTurnIndex = (currentTurnIndex + 1) % turnOrder.Count;
            SquadManager.Member m = squadManager.squadMembers[turnOrder[currentTurnIndex]];
            if (m != null && !m.inactive && m.actions > 0)
                break;
        }
        while (currentTurnIndex != startIndex);
    }

    // ќбновление отображени€ всех клеток (вызываетс€ из SquadCell)
    void UpdateCells()
    {
        SquadCell[] cells = FindObjectsOfType<SquadCell>();
        foreach (SquadCell cell in cells)
        {
            cell.UpdateDisplay();
        }
    }

    // ≈сли у всех персонажей actions равны 0 (дл€ активных), восстанавливаем их и сбрасываем флаг inactive
    void CheckRoundComplete()
    {
        bool roundComplete = true;
        foreach (var kvp in squadManager.squadMembers)
        {
            if (kvp.Value != null && !kvp.Value.inactive && kvp.Value.actions > 0)
            {
                roundComplete = false;
                break;
            }
        }

        if (roundComplete)
        {
            foreach (var kvp in squadManager.squadMembers)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.actions = kvp.Value.initialActions;
                    kvp.Value.inactive = false;
                }
            }
            CalculateTurnOrder();
        }
    }
}
