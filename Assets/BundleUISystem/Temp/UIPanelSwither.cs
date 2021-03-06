﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;
using BundleUISystem;

public class UIPanelSwither : MonoBehaviour {
    public string panelName;
    public bool destroyOnOpen;
    public void OpenSend(object data)
    {
        UIGroup.Open(panelName,null,data);
        if(destroyOnOpen)
        {
            Destroy(gameObject);
        }
    }
    public void Open()
    {
        UIGroup.Open(panelName);
        if (destroyOnOpen)
        {
            Destroy(gameObject);
        }
    }
    public void Close()
    {
        UIGroup.Close(panelName);
    }

    public void Hide()
    {
        UIGroup.Hide(panelName);
    }
    private void OnDestroy()
    {
        if (!destroyOnOpen)
        {
            UIGroup.Close(panelName);
        }
    }
}
