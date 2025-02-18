using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SquadManagerNamespace;

public class TurnManager : MonoBehaviour
{
    private SquadManager squadManager;
    private List<string> turnOrder = new List<string>(); // ������ �������, ��� ��������� ��������� (������ ��������)
    private int currentTurnIndex = 0;

    void Start()
    {
        squadManager = FindObjectOfType<SquadManager>();
        CalculateTurnOrder();
    }

    // ������������� ������� �����, �������� ������ ����������, � ������� !inactive
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

    // ���������� true, ���� ������ � ������ ������ �������� �������� (������� ���)
    public bool IsCurrentCharacter(string positionKey)
    {
        if (turnOrder.Count == 0)
            return false;
        return turnOrder[currentTurnIndex] == positionKey;
    }

    // ��������� ����� �� ������; ���������� �� SquadCell
    public void HandleCellClick(string clickedPosition)
    {
        var squadMembers = squadManager.squadMembers;
        if (turnOrder.Count == 0)
            return;

        string activePosition = turnOrder[currentTurnIndex];
        SquadManager.Member activeMember = squadMembers[activePosition];

        // ���� ��������� ��������� ���, ���� �� ��� �� ����� ������, ��������� � ����������
        if (activeMember == null || activeMember.inactive || activeMember.actions <= 0)
        {
            NextTurn();
            return;
        }

        // ���� ���� �� �������� ������ � �������� ������ 1 ��������
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

        // ���� ������ � ������ ����������� � ������, ������ �� ������
        if (!squadMembers.ContainsKey(clickedPosition))
            return;

        // ���� ������� ������ ����� � ���������� ��������� ��������� ����
        if (squadMembers[clickedPosition] == null)
        {
            squadMembers[clickedPosition] = activeMember;
            squadMembers[activePosition] = null;
            turnOrder[currentTurnIndex] = clickedPosition; // ��������� ������� ��������� ���������
            activeMember.actions = Mathf.Max(0, activeMember.actions - 1);
            activeMember.CheckInactive();
        }
        else
        {
            // ���� ������� ������ ������ � ������ ������� ��������� ��������� � ���������� � ������� ������
            SquadManager.Member targetMember = squadMembers[clickedPosition];
            squadMembers[clickedPosition] = activeMember;
            squadMembers[activePosition] = targetMember;

            // ��������� ������� ����� ���������� � turnOrder
            int activeMemberIndex = turnOrder.IndexOf(activePosition);
            int targetMemberIndex = turnOrder.IndexOf(clickedPosition);

            // ���������� ������� � turnOrder
            turnOrder[activeMemberIndex] = clickedPosition;
            turnOrder[targetMemberIndex] = activePosition;

            // ����� ��������� ������� ����� �������� ������� ������ ��������� ���������
            currentTurnIndex = activeMemberIndex;

            activeMember.actions = Mathf.Max(0, activeMember.actions - 1);
            activeMember.CheckInactive();
        }

        // ���� ����� ���������� �������� � ��������� ��������� �������� 0, ��������� � ����������
        if (activeMember.actions == 0)
        {
            NextTurn();
        }

        squadManager.SaveSquadData();
        UpdateCells();
        CheckRoundComplete();
    }

    // ������� � ���������� ��������� (���������� ���, � ���� 0 �������� ��� inactive = true)
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

    // ���������� ����������� ���� ������ (���������� �� SquadCell)
    void UpdateCells()
    {
        SquadCell[] cells = FindObjectsOfType<SquadCell>();
        foreach (SquadCell cell in cells)
        {
            cell.UpdateDisplay();
        }
    }

    // ���� � ���� ���������� actions ����� 0 (��� ��������), ��������������� �� � ���������� ���� inactive
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
