using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SquadManagerNamespace
{
    public class SquadManager : MonoBehaviour
    {
        [System.Serializable]
        public class Squad
        {
            public string squadName;
            public Dictionary<string, string> members = new Dictionary<string, string>(); // Ключи: позиции, значения: имена персонажей
        }

        [System.Serializable]
        public class Member
        {
            public string characterName { get; set; }
            public string displayName { get; set; }
            public int health { get; set; }
            public int attackPower { get; set; }
            public int defense { get; set; }
            public int initiative { get; set; }
            public int actions { get; set; }
            public int initialActions { get; set; }
            public string spritePath { get; set; }
            public bool inactive { get; set; }  // Флаг неактивности

            // Конструктор копирования
            public Member(Member other)
            {
                characterName = other.characterName;
                displayName = other.displayName;
                health = other.health;
                attackPower = other.attackPower;
                defense = other.defense;
                initiative = other.initiative;
                actions = other.actions;
                initialActions = other.initialActions;
                spritePath = other.spritePath;
                inactive = other.inactive;
            }

            // Конструктор по умолчанию
            public Member() { }

            // Проверка: если действий 0, персонаж становится неактивным
            public void CheckInactive()
            {
                if (actions <= 0)
                {
                    inactive = true;
                }
            }
        }

        // Пути к JSON файлам
        private string squadFilePath = "Assets/Resources/Squad.json";
        private string charactersFilePath = "Assets/Resources/Characters.json";

        // Загруженные данные
        public Dictionary<string, Member> allCharacters = new Dictionary<string, Member>();
        public Dictionary<string, Member> squadMembers = new Dictionary<string, Member>();

        void Awake()
        {
            LoadCharactersData();
            LoadSquadData();
        }

        public void LoadCharactersData()
        {
            if (!File.Exists(charactersFilePath))
            {
                Debug.LogError("Characters file not found: " + charactersFilePath);
                return;
            }

            string charactersJson = File.ReadAllText(charactersFilePath);
            allCharacters = JsonConvert.DeserializeObject<Dictionary<string, Member>>(charactersJson);
        }

        public void LoadSquadData()
        {
            if (!File.Exists(squadFilePath))
            {
                Debug.LogError("Squad file not found: " + squadFilePath);
                return;
            }

            string squadJson = File.ReadAllText(squadFilePath);
            Squad squad = JsonConvert.DeserializeObject<Squad>(squadJson);

            squadMembers.Clear();

            foreach (var entry in squad.members)
            {
                string position = entry.Key;
                string memberName = entry.Value;

                if (string.IsNullOrEmpty(memberName) || !allCharacters.ContainsKey(memberName))
                {
                    squadMembers[position] = null;
                }
                else
                {
                    // По умолчанию персонаж активен
                    Member m = new Member(allCharacters[memberName]);
                    m.inactive = false;
                    squadMembers[position] = m;
                }
            }
        }

        public void SaveSquadData()
        {
            Squad squad = new Squad { squadName = "Default Squad", members = new Dictionary<string, string>() };

            foreach (var entry in squadMembers)
            {
                squad.members[entry.Key] = entry.Value != null ? entry.Value.characterName : "";
            }

            string savePath = squadFilePath;
            File.WriteAllText(savePath, JsonConvert.SerializeObject(squad, Formatting.Indented));
        }
    }
}
