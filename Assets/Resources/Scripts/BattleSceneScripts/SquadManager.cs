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
            public Dictionary<string, string> members = new Dictionary<string, string>(); // Словарь именованных клеток
        }

        [System.Serializable]
        public class Member
        {
            public string characterName { get; set; }
            public int health { get; set; }
            public int attackPower { get; set; }
            public int defense { get; set; }
            public int initiative { get; set; }
            public int actions { get; set; }
            public int initialActions { get; set; }
            public string spritePath { get; set; }

            // Конструктор копирования
            public Member(Member other)
            {
                characterName = other.characterName;
                health = other.health;
                attackPower = other.attackPower;
                defense = other.defense;
                initiative = other.initiative;
                actions = other.actions;
                initialActions = other.initialActions;
                spritePath = other.spritePath;
            }

            // Конструктор по умолчанию
            public Member() { }
        }

        // Пути к файлам для редактора Unity
        private string squadFilePath = "Assets/Resources/Squad.json";
        private string charactersFilePath = "Assets/Resources/Characters.json";

        // Хранение загруженных персонажей
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
            Debug.Log("Loaded Characters Data");
        }

        public void LoadSquadData()
        {
            if (!File.Exists(squadFilePath))
            {
                Debug.LogError("Squad file not found: " + squadFilePath);
                return;
            }

            string squadJson = File.ReadAllText(squadFilePath);

            // Если у вас было ожидание десериализовать List<string>, но на самом деле вам нужен Dictionary<string, string>
            Squad squad = JsonConvert.DeserializeObject<Squad>(squadJson);

            Debug.Log("Loaded Squad: " + squad.squadName);

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
                    squadMembers[position] = new Member(allCharacters[memberName]); // Используем конструктор копирования
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

            string savePath = squadFilePath; // Путь для сохранения в Assets/Resources
            File.WriteAllText(savePath, JsonConvert.SerializeObject(squad, Formatting.Indented));
            Debug.Log("Squad saved to: " + savePath);
        }
    }
}
