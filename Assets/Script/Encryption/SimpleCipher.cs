using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCipher : MonoBehaviour
{
    public void StartCipher(string emailToEncode)
    {
        ScanEmailChar(emailToEncode);
    }

    private void ScanEmailChar(string emailToEncode)
    {
        foreach(var chars in emailToEncode)
        {
            if((chars >= 97 && chars <= 122) || (chars >= 65 && chars <= 90))
            {
                //is an alphabet
            }
        }
    }
}
