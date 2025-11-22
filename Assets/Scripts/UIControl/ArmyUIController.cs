
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace BattleYouCan
{
    // public class MainView : MonoBehaviour
    public class ArmyUIController : MonoBehaviour
    {
        [SerializeField]
        VisualTreeAsset m_ListEntryTemplate;
        UIDocument uiDocument;
        CharacterListController characterListController;
        Button backButton;
        List<CharacterData> _characterData;

        private void Awake()
        {
             // // Initialize the character list controller
             // 함수 초기화에서 유니티 객체들을 사용하는 방법은 불가능하다..
            
        } 

        void Start()
        {
        }
        void OnEnable()
        {
            //GameManager.instance.bCharListOpen = true;
            uiDocument = GetComponent<UIDocument>();
            backButton = uiDocument.rootVisualElement.Q<Button>("backButton");
            backButton.clicked += OnBackButtonClicked;
            characterListController = new CharacterListController();
            characterListController.InitializeCharacterList(uiDocument.rootVisualElement, m_ListEntryTemplate, _characterData); 
        
            ShowListOfArmy(true);
        }
        void OnDisable()
        {
            if (backButton != null)
                backButton.clicked -= OnBackButtonClicked;
        }
        public void ShowListOfArmy(bool bshow)
        {
            // The UXML is already instantiated by the UIDocument component
            gameObject.SetActive(true);
            this.enabled = bshow;
            
            
            
        }

        void OnBackButtonClicked()
        {
            Debug.Log("버튼클릭됨");
            gameObject.SetActive(false);
        }

        public void SetCharacterData(List<CharacterData> characterData)
        {
            _characterData = characterData;
        }
    }
}