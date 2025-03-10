using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{

    public static Manager instance;

    public Web web;
    public UserInfo UserInfo;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        web = GetComponent<Web>();
        UserInfo = GetComponent<UserInfo>();

    }


}
