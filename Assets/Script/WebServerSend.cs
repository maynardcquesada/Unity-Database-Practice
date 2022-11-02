using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Linq;

public class WebServerSend : MonoBehaviour
{
    #region CANVASES
    [Header("MODULES")]
    [SerializeField] private GameObject registerModule;
    [SerializeField] private GameObject loginModule;
    [SerializeField] private GameObject bookSearchModule;

    #region SELECTED SEARCH RESULT
    [Header("SELECTED SEARCH RESULT")]
    [SerializeField] private GameObject selectedSearchResultModule;
    [SerializeField] private GameObject bookTitleModule;
    [SerializeField] private GameObject bookAuthorModule;
    #endregion

    #endregion

    #region Login Module Inputs
    [Header("Login Module Settings")]
    [SerializeField] private TMP_InputField loginCredentialsInput;
    [SerializeField] private TMP_InputField loginPassInput;
    #endregion

    #region Search Module
    [Header("Search Module Settings")]
    [SerializeField] private TMP_Dropdown dropDown;
    #endregion

    #region Book Title Module Elements
    [Header("Book Title Module Elements")]
    [SerializeField] private TMP_Text bookTitleTextField;
    [SerializeField] private TMP_Text bookAuthorTextField;
    [SerializeField] private TMP_Text bookSummaryTextField;
    #endregion

    #region Book Author Module Elements
    [Header("Book Author Module Elements")]
    [SerializeField] private TMP_Text booksByAuthorTextField;
    [SerializeField] private GameObject booksByAuthorContents;
    #endregion

    private bool loginValid;
    private List<string> bookTitleAuthor = new List<string>();
    private List<string> searchBar = new List<string>();
    [HideInInspector] public List<string> getBooksByAuthor = new List<string>();
    [HideInInspector] public bool booksByAuthorIsFetched;
    [HideInInspector] public string currentSelectedAuthor;

    private TMP_InputField searchModuleInput;

    private TMP_InputField[] registrationModuleInputs;


    [SerializeField]private GmailSender gmailSender;

    private void Start()
    {
        searchModuleInput = bookSearchModule.GetComponent<TMP_InputField>();
        loginValid = false;
        booksByAuthorIsFetched = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && loginModule.activeSelf)
        {
            LoginClicked();
        }
    }

    private void LateUpdate()
    {
        if (bookSearchModule.activeSelf)
        {
            searchModuleInput.Select();
            searchModuleInput.MoveTextEnd(true);
        }
    }

    public void RegisterUserActivator()
    {
        if(loginModule.activeSelf && !registerModule.activeSelf)
        {
            registerModule.SetActive(true);
            loginModule.SetActive(false);
        }
    }

    public void RegisterUserValidation()
    {
        registrationModuleInputs = registerModule.GetComponentsInChildren<TMP_InputField>();
        StartCoroutine(RegisterUserValidationNameEmailContact(registrationModuleInputs));
    }

    public void LoginClicked()
    {
        StartCoroutine(LoginUser(loginCredentialsInput.text, loginPassInput.text));
        if (loginModule.activeSelf && !bookSearchModule.activeSelf && loginValid)
        {
            StartCoroutine(ReadData("http://localhost/UnityDatabasePractice1/ReadBooks.php"));
            loginModule.SetActive(false);
            bookSearchModule.SetActive(true);
        }
    }

    IEnumerator RegisterUserValidationNameEmailContact(TMP_InputField[] registrationInputs/*string username, string email, string contact*/)
    {
        WWWForm form = new WWWForm();
        form.AddField("userName", registrationInputs[0].text);
        form.AddField("userEmail", registrationInputs[2].text);
        form.AddField("userContact", registrationInputs[4].text);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/UnityDatabasePractice1/RegisterUserValidationNameEmailContact.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                var validationOfCredentials = www.downloadHandler.text.Split('×');
                validationOfCredentials = validationOfCredentials.Where(validationOfCredentials => validationOfCredentials != string.Empty).ToArray();
                if(validationOfCredentials.Length <= 0)
                {
                    StartCoroutine(RegisterUser(registrationInputs));
                }
                else
                {
                    yield break;
                }
            }
        }
    }

    IEnumerator RegisterUser(TMP_InputField[] registrationInputs)
    {
        WWWForm form = new WWWForm();
        form.AddField("userName", registrationInputs[0].text);
        form.AddField("userPass", registrationInputs[1].text);
        form.AddField("userEmail", registrationInputs[2].text);
        form.AddField("userAddress", registrationInputs[3].text);
        form.AddField("userContact", registrationInputs[4].text);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/UnityDatabasePractice1/RegisterUser.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                print(www.downloadHandler.text);
                gmailSender.GmailStart(registrationInputs[2].text);
            }
        }
    }

    IEnumerator LoginUser(string credentials, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("loginCredentials", credentials);
        form.AddField("loginPass", password);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/UnityDatabasePractice1/login.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                if (www.downloadHandler.text.Contains("Name does not exist"))
                {
                    loginValid = false;
                }
                if (www.downloadHandler.text.Contains("Wrong password"))
                {
                    loginValid = false;
                }
                if (www.downloadHandler.text.Contains("Login Successfully"))
                {
                    loginValid = true;
                    LoginClicked();
                }
            }
        }
    }

    IEnumerator ReadData(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    StringSplitter(webRequest.downloadHandler.text);
                    break;
            }
        }
    }

    IEnumerator GetSummary(string booktitle, string bookauthor)
    {
        WWWForm form = new WWWForm();
        form.AddField("booktitle", booktitle);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/UnityDatabasePractice1/GetSummary.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                bookTitleTextField.text = booktitle;
                bookAuthorTextField.text = bookauthor;
                bookSummaryTextField.text = www.downloadHandler.text;
            }
        }
    }

    IEnumerator GetBookByAuthor(string bookauthor)
    {
        WWWForm form = new WWWForm();
        form.AddField("bookauthor", bookauthor);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/UnityDatabasePractice1/GetBooksByAuthor.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                GetBooksByAuthorSpringSplitter(www.downloadHandler.text);
                booksByAuthorTextField.text = bookauthor;
                currentSelectedAuthor = bookauthor;
            }
        }
    }

    private void StringSplitter (string webRequestShow)
    {
        string[] splitBooks = webRequestShow.Split("<br>");
        string[] splitBookTitleAndAuthor;
        for(int i = 0; splitBooks.Length - 1 > i; i++)
        {
            splitBookTitleAndAuthor = splitBooks[i].Split(('×'));
            bookTitleAuthor.Add(splitBookTitleAndAuthor[0]);
            bookTitleAuthor.Add(splitBookTitleAndAuthor[1]);
        }
    }

    private void GetBooksByAuthorSpringSplitter(string titleRequestShow)
    {
        string[] splitTitleByAuthor = titleRequestShow.Split('×');
        for (int i = 0; splitTitleByAuthor.Length - 1 > i; i++)
        {
            getBooksByAuthor.Add(splitTitleByAuthor[i]);
        }
        booksByAuthorIsFetched = true;
        AuthorSelectedActivator();
    }

    public void DisplaySearch()
    {
        dropDown.ClearOptions();
        string bookSearchModuleText = bookSearchModule.GetComponent<TMP_InputField>().text;
        bookSearchModuleText = bookSearchModuleText.ToLower();

        if (string.IsNullOrEmpty(bookSearchModuleText) || string.IsNullOrWhiteSpace(bookSearchModuleText))
        {
            searchBar.Clear();
            dropDown.Hide();
            dropDown.ClearOptions();
        }

        else
        {
            dropDown.Hide();
            var matchingValues = bookTitleAuthor.Where(bookTitleAuthor => bookTitleAuthor.ToLower().Contains(bookSearchModuleText));
            if (matchingValues.Any())
            {
                searchBar = new List<string>(matchingValues);
                searchBar = searchBar.Distinct().ToList();
                //if (!DropDownOptionCheckDuplicate())
                //{
                dropDown.interactable = true;
                dropDown.AddOptions(searchBar);
                dropDown.Show();
                //}
            }
            else if (!matchingValues.Any())
            {
                searchBar.Clear();
                searchBar.Add("Search Result Not Found");
                dropDown.AddOptions(searchBar);
                dropDown.Show();
            }
        }

    }

    //private bool DropDownOptionCheckDuplicate()
    //{
    //    bool alreadyContains = false;

    //    for (int i = 0; dropDown.options.Count > i; i++)
    //    {
    //        if (alreadyContains)
    //        {
    //            break;
    //        }

    //        if (!alreadyContains)
    //        {
    //            for (int j = 0; searchBar.Count > j; j++)
    //            {
    //                if (dropDown.options[i].text.ToLower().Equals(searchBar[j].ToLower()))
    //                {
    //                    alreadyContains = true;
    //                    break;
    //                }
    //            }
    //        }
    //    }

    //    return alreadyContains;
    //}

    public void CheckDropDownSelected()
    {
        string bookSearchModuleText = bookSearchModule.GetComponent<TMP_InputField>().text;
        if (bookSearchModuleText != "" || bookSearchModuleText != " " || bookSearchModuleText != null || bookSearchModuleText.Length != 0)
        {
            if (dropDown.options.Count != 0)
            {
                for (int i = 0; bookTitleAuthor.Count > i; i++)
                {
                    if (dropDown.options[dropDown.value].text == bookTitleAuthor[i] && bookSearchModuleText.Length != 0)
                    {
                        if (i % 2 == 0)
                        {
                            BookTitleSelected(bookTitleAuthor[i], bookTitleAuthor[i + 1]);
                        }
                        if (i % 2 != 0)
                        {
                            BookAuthorSelected(bookTitleAuthor[i]);
                        }
                    }
                }
            }
        }
        
    }

    public void BookTitleSelected(string bookTitlePass, string bookAuthorPass)
    {
        if (bookSearchModule.activeSelf && !selectedSearchResultModule.activeSelf && !bookTitleModule.activeSelf)
        {
            StartCoroutine(GetSummary(bookTitlePass, bookAuthorPass));
            bookSearchModule.SetActive(false);
            selectedSearchResultModule.SetActive(true);
            bookTitleModule.SetActive(true);
        }
        if (selectedSearchResultModule.activeSelf && bookAuthorModule.activeSelf && !bookTitleModule.activeSelf)
        {
            StartCoroutine(GetSummary(bookTitlePass, bookAuthorPass));
            selectedSearchResultModule.SetActive(true);
            var contentDestroyer = booksByAuthorContents.GetComponentsInChildren<BookTitleItem>();
            //if (!booksByAuthorContents.activeSelf)
            //{
            foreach (var values in contentDestroyer)
            {
                Destroy(values.gameObject);
            }
            //}
            bookAuthorModule.SetActive(false);
            getBooksByAuthor.Clear();
            bookTitleModule.SetActive(true);
        }
    }

    private void BookAuthorSelected(string bookAuthorPass)
    {
        if(bookSearchModule.activeSelf && !selectedSearchResultModule.activeSelf && !bookAuthorModule.activeSelf)
        {
            StartCoroutine(GetBookByAuthor(bookAuthorPass));
        }
    }

    public void BackFromBookTitleSelected()
    {
        if(selectedSearchResultModule.activeSelf && bookTitleModule.activeSelf && !bookSearchModule.activeSelf)
        {
            ClearSearchModule();
            dropDown.ClearOptions();
            dropDown.RefreshShownValue();
            selectedSearchResultModule.SetActive(false);
            bookTitleModule.SetActive(false);
            bookSearchModule.SetActive(true);
        }
        if (selectedSearchResultModule.activeSelf && bookAuthorModule.activeSelf && !bookSearchModule.activeSelf)
        {
            ClearSearchModule();
            dropDown.ClearOptions();
            dropDown.RefreshShownValue();
            selectedSearchResultModule.SetActive(false);
            bookAuthorModule.SetActive(false);
            bookSearchModule.SetActive(true);
        }
    }

    private void ClearSearchModule()
    {
        searchModuleInput.Select();
        searchModuleInput.text = "";
    }

    public void RefreshValues()
    {
        dropDown.RefreshShownValue();
    }

    private void AuthorSelectedActivator()
    {
        if (booksByAuthorIsFetched)
        {
            selectedSearchResultModule.SetActive(true);
            bookAuthorModule.SetActive(true);
            bookSearchModule.SetActive(false);
        }
    }







}