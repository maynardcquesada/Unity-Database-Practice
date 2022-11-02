using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BookTitleScrollViewController : MonoBehaviour
{
    [SerializeField] private GameObject bookTitleButtonPref;
    [SerializeField] private Transform bookTitleButtonParent;
    [SerializeField] private WebServerSend webServerSend;

    // Start is called before the first frame update
    private void OnEnable()
    {
        LoadBookTitleButton();
    }

    private void LoadBookTitleButton()
    {
        for(int i = 0; webServerSend.getBooksByAuthor.Count > i; i++)
        {
            GameObject bookTitleButtonObj = Instantiate(bookTitleButtonPref, bookTitleButtonParent) as GameObject;
            bookTitleButtonObj.GetComponent<BookTitleItem>().bookTitleIndex = webServerSend.getBooksByAuthor[i];
            bookTitleButtonObj.GetComponent<BookTitleItem>().bookTitleScrollViewController = this;
        }
    }

    public void OnBookTitleClick(string bookTitleToFetch)
    {
        //insert code for fetching book information
        webServerSend.BookTitleSelected(bookTitleToFetch, webServerSend.currentSelectedAuthor);
    }
}
