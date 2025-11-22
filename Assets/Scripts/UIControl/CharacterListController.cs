using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BattleYouCan
{
    public class CharacterListController
    {
        // UXML template for list entries
        VisualTreeAsset m_ListEntryTemplate;

        // UI element references
        ListView m_CharacterList;
        Label m_CharNameLabel;
        Label m_CharPowLabel;
        Label m_CharIntLabel;
        Label m_CharHealLabel;
        Label m_CharlukLabel;
        VisualElement m_CharPortrait;
        public List<CharacterData> m_TroopCharaters;
        public void InitializeCharacterList(VisualElement root, VisualTreeAsset listElementTemplate, List<CharacterData> characterDatas = null)
        {
            //m_TroopCharaters.Clear();
            m_TroopCharaters = characterDatas;
            //EnumerateAllCharacters();

            // Store a reference to the template for the list entries
            m_ListEntryTemplate = listElementTemplate;

            // Store a reference to the character list element
            m_CharacterList = root.Q<ListView>("character-list");

            // Store references to the selected character info elements
            m_CharNameLabel = root.Q<Label>("character-name");
            m_CharPowLabel = root.Q<Label>("character-pow_value");
            m_CharIntLabel = root.Q<Label>("character-int_value");
            m_CharHealLabel = root.Q<Label>("character-heal_value");
            m_CharlukLabel = root.Q<Label>("character-luk_value");

            m_CharPortrait = root.Q<VisualElement>("character-portrait");

            FillCharacterList();

            // Register to get a callback when an item is selected
            m_CharacterList.selectionChanged += OnCharacterSelected;
        }

        void EnumerateAllCharacters(bool autoFill = true)
        {
            if (autoFill)
            {
                
                //m_TroopCharaters.AddRange(GameManager.instance.AllCharacters);
            }
            else
            {
                m_TroopCharaters.AddRange(Resources.LoadAll<CharacterData>("Characters"));

            }
        }

        void FillCharacterList()
        {
            // Set up a make item function for a list entry
            m_CharacterList.makeItem = () =>
            {
                // Instantiate the UXML template for the entry
                var newListEntry = m_ListEntryTemplate.Instantiate();

                // Instantiate a controller for the data
                var newListEntryLogic = new CharacterListEntryController();

                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;

                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);

                // Return the root of the instantiated visual tree
                return newListEntry;
            };


            // Set up bind function for a specific list entry
            m_CharacterList.bindItem = (item, index) =>
            {
                // CharacterData aCharData = m_TroopCharaters[index];
                // if(aCharData.enable)
                (item.userData as CharacterListEntryController)?.SetCharacterData(m_TroopCharaters[index]);
            };

            // Set a fixed item height matching the height of the item provided in makeItem. 
            // For dynamic height, see the virtualizationMethod property.
            m_CharacterList.fixedItemHeight = 45;

            // Set the actual item's source list/array
            m_CharacterList.itemsSource = m_TroopCharaters;
        }

        void OnCharacterSelected(IEnumerable<object> selectedItems)
        {
            // Get the currently selected item directly from the ListView
            var selectedCharacter = m_CharacterList.selectedItem as CharacterData;

            // Handle none-selection (Escape to deselect everything)
            if (selectedCharacter == null)
            {
                // Clear
                m_CharNameLabel.text = "";
                m_CharPortrait.style.backgroundImage = null;

                return;
            }

            // Fill in character details
            m_CharNameLabel.text = selectedCharacter.CharacterName;
            m_CharPowLabel.text = selectedCharacter.power.ToString();
            m_CharIntLabel.text = selectedCharacter.intel.ToString();
            m_CharHealLabel.text = selectedCharacter.health.ToString();
            m_CharlukLabel.text = selectedCharacter.luck.ToString();
            m_CharPortrait.style.backgroundImage = new StyleBackground(selectedCharacter.PortraitImage);
        }

    }
}