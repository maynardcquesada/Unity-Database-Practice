using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BookTitleItem : MonoBehaviour
{
    [HideInInspector] public string bookTitleIndex;
    [HideInInspector] public BookTitleScrollViewController bookTitleScrollViewController;
    //
    [SerializeField] TMP_Text bookTitleButtonText;

    private void Start()
    {
        bookTitleButtonText.text = bookTitleIndex;
    }

    public void OnBookTitleClick()
    {
        bookTitleScrollViewController.OnBookTitleClick(bookTitleIndex);
        //Destroy(gameObject);
    }

}
