﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using BundleUISystem.Internal;

namespace BundleUISystem
{
    public class UIGroup : MonoBehaviour
    {
        public List<UIBundleInfo> bundles = new List<UIBundleInfo>();
        public List<BundleInfo> rbundles = new List<BundleInfo>();
        public List<PrefabInfo> prefabs = new List<PrefabInfo>();
        public List<UIGroupObj> groupObjs = new List<UIGroupObj>();
        public string assetUrl;
        public string menu;
        private EventHold eventHold = new EventHold();
        private List<IUILoadCtrl> currLoadCtrls = new List<IUILoadCtrl>();
        private event UnityAction onDestroy;
        private event UnityAction onEnable;
        private event UnityAction onDisable;
        private const string addClose = "close";

        private static List<IUILoadCtrl> controllers = new List<IUILoadCtrl>();
        private static List<EventHold> eventHolders = new List<EventHold>();
        public static UnityEngine.Events.UnityAction<string> MessageNotHandled;

        void Awake()
        {
            eventHolders.Add(eventHold);
            RegistBaseUIEvents();
            RegistSubUIEvents();
        }

        private void OnEnable()
        {
            if (onEnable != null)
            {
                onEnable.Invoke();
            }
        }
        private void OnDisable()
        {
            if (onDisable != null)
            {
                onDisable.Invoke();
            }
        }
        private void OnDestroy()
        {
            if (onDestroy != null)
            {
                onDestroy.Invoke();
            }
            foreach (var item in currLoadCtrls)
            {
                if (item != null)
                    controllers.Remove(item);
            }
            eventHolders.Remove(eventHold);
        }

        private void RegistBaseUIEvents()
        {
            if (prefabs.Count > 0)
            {
                var prefabLoadCtrl = new UIPrefabLoadCtrl(transform);
                controllers.Add(prefabLoadCtrl);
                RegisterBundleEvents(prefabLoadCtrl, prefabs.ConvertAll<ItemInfoBase>(x => x));
            }

            if (bundles.Count > 0)
            {
                var uibundleLoadCtrl = new UIBundleLoadCtrl(transform);
                controllers.Add(uibundleLoadCtrl);
                RegisterBundleEvents(uibundleLoadCtrl, bundles.ConvertAll<ItemInfoBase>(x => x));
            }

            if (rbundles.Count > 0)
            {
                var remoteLoadCtrl = new UIBundleLoadCtrl(assetUrl, menu, transform);
                controllers.Add(remoteLoadCtrl);
                RegisterBundleEvents(remoteLoadCtrl, rbundles.ConvertAll<ItemInfoBase>(x => x));
            }
        }

        private void RegistSubUIEvents()
        {
            foreach (var item in groupObjs)
            {
                if (item.prefabs.Count > 0)
                {
                    var prefabLoadCtrl = new UIPrefabLoadCtrl(transform,false);
                    controllers.Add(prefabLoadCtrl);
                    RegisterBundleEvents(prefabLoadCtrl, item.prefabs.ConvertAll<ItemInfoBase>(x => x));
                }

                if (item.bundles.Count > 0)
                {
                    var uibundleLoadCtrl = new UIBundleLoadCtrl(transform, false);
                    controllers.Add(uibundleLoadCtrl);
                    RegisterBundleEvents(uibundleLoadCtrl, item.bundles.ConvertAll<ItemInfoBase>(x => x));
                }

                if (item.rbundles.Count > 0)
                {
                    var remoteLoadCtrl = new UIBundleLoadCtrl(item.assetUrl, item.menu, transform, false);
                    controllers.Add(remoteLoadCtrl);
                    RegisterBundleEvents(remoteLoadCtrl, item.rbundles.ConvertAll<ItemInfoBase>(x => x));
                }
            }
        }
        #region 事件注册
        private void RegisterBundleEvents(IUILoadCtrl loadCtrl, List<ItemInfoBase> bundles)
        {
            for (int i = 0; i < bundles.Count; i++)
            {
                ItemInfoBase trigger = bundles[i];
                switch (trigger.type)
                {
                    case UIBundleInfo.Type.Button:
                        RegisterButtonEvents(loadCtrl, trigger);
                        break;
                    case UIBundleInfo.Type.Toggle:
                        RegisterToggleEvents(loadCtrl, trigger);
                        break;
                    case UIBundleInfo.Type.Name:
                        RegisterMessageEvents(loadCtrl, trigger);
                        break;
                    case UIBundleInfo.Type.Enable:
                        RegisterEnableEvents(loadCtrl, trigger);
                        break;
                    default:
                        break;
                }
            }
        }
        private void RegisterMessageEvents(IUILoadCtrl loadCtrl, ItemInfoBase trigger)
        {
            UnityAction<object> createAction = (x) =>
            {
                trigger.Data = x;
                loadCtrl.GetGameObjectInfo(trigger);
            };

            UnityAction<object> handInfoAction = (data) =>
            {
                trigger.Data = data;
                IPanelName irm = trigger.instence.GetComponent<IPanelName>();
                irm.HandleData(trigger.Data);
            };

            trigger.OnCreate = (x) =>
            {
                IPanelName irm = x.GetComponent<IPanelName>();
                if (irm != null)
                {
                    trigger.instence = x;
                    irm.HandleData(trigger.Data);
                    eventHold.Remove(trigger.assetName, createAction);
                    eventHold.Record(trigger.assetName, handInfoAction);
                    irm.OnDelete += () =>
                    {
                        trigger.instence = null;
                        eventHold.Remove(trigger.assetName, handInfoAction);
                        eventHold.Record(trigger.assetName, createAction);
                    };
                }
                RegisterDestoryAction(trigger.assetName, x);
            };

            eventHold.Record(trigger.assetName, createAction);

            onDestroy += () =>
            {
                eventHold.Remove(trigger.assetName, createAction);
            };
        }
        private void RegisterToggleEvents(IUILoadCtrl loadCtrl, ItemInfoBase trigger)
        {
            UnityAction<bool> CreateByToggle = (x) =>
            {
                if (x)
                {
                    trigger.toggle.interactable = false;
                    loadCtrl.GetGameObjectInfo(trigger);
                }
                else
                {
                    Destroy((GameObject)trigger.Data);
                }
            };
            trigger.toggle.onValueChanged.AddListener(CreateByToggle);

            onDestroy += () =>
            {
                trigger.toggle.onValueChanged.RemoveAllListeners();
            };

            trigger.OnCreate = (x) =>
            {
                trigger.toggle.interactable = true;

                trigger.Data = x;
                IPanelToggle it = x.GetComponent<IPanelToggle>();
                if (it != null)
                {
                    it.toggle = trigger.toggle;

                    trigger.toggle.onValueChanged.RemoveListener(CreateByToggle);

                    it.OnDelete += () =>
                    {
                        trigger.toggle.onValueChanged.AddListener(CreateByToggle);
                    };
                }
                RegisterDestoryAction(trigger.assetName, x);
            };
        }
        private void RegisterButtonEvents(IUILoadCtrl loadCtrl, ItemInfoBase trigger)
        {
            UnityAction CreateByButton = () =>
            {
                loadCtrl.GetGameObjectInfo(trigger);
            };
            trigger.button.onClick.AddListener(CreateByButton);
            onDestroy += () => { trigger.button.onClick.RemoveAllListeners(); };
            trigger.OnCreate = (x) =>
            {
                IPanelButton ib = x.GetComponent<IPanelButton>();
                if (ib != null)
                {
                    ib.Btn = trigger.button;
                    trigger.button.onClick.RemoveListener(CreateByButton);

                    ib.OnDelete += () =>
                    {
                        trigger.button.onClick.AddListener(CreateByButton);
                    };
                }
                RegisterDestoryAction(trigger.assetName, x);
            };
        }
        private void RegisterEnableEvents(IUILoadCtrl loadCtrl, ItemInfoBase trigger)
        {
            UnityAction onEnableAction = () =>
            {
                loadCtrl.GetGameObjectInfo(trigger);
            };

            trigger.OnCreate = (x) =>
            {
                trigger.Data = x;
                IPanelEnable irm = x.GetComponent<IPanelEnable>();
                if (irm != null)
                {
                    onEnable -= onEnableAction;

                    irm.OnDelete += () =>
                    {
                        onEnable += onEnableAction;
                    };
                }
                else
                {
                    onDisable += () =>
                    {
                        if (trigger.Data != null && trigger.Data is GameObject)
                        {
                            Destroy((GameObject)trigger.Data);
                        }
                    };
                }
                RegisterDestoryAction(trigger.assetName, x);
            };

            onEnable += onEnableAction;
        }
        private void RegisterDestoryAction(string assetName, GameObject x)
        {
            string key = addClose + assetName;
            eventHold.Remove(key);
            eventHold.Record(key, new UnityAction<object>((y) =>
            {
                if (x != null) Destroy(x);
            }));
        }
        #endregion

        #region 触发事件
        public static void Open(string assetName, object data = null)
        {
            bool handled = true;
            TraverseHold((eventHold) =>
            {
                handled |= eventHold.NotifyObserver(assetName, data);
            });
            if (!handled)
            {
                NoMessageHandle(assetName);
            }
        }
        public static void Open<T>(object data = null) where T : UIPanelTemp
        {
            string assetName = typeof(T).ToString();
            Open(assetName, data);
        }
        public static void Close(string assetName)
        {
            foreach (var item in controllers)
            {
                if (item != null)
                {
                    item.CansaleLoadObject(assetName);
                }
            }

            var key = (addClose + assetName);

            TraverseHold((eventHold) =>
            {
                eventHold.NotifyObserver(key);
            });
        }
        public static void Close<T>() where T : UIPanelTemp
        {
            string assetName = typeof(T).ToString();
            Close(assetName);
        }
        private static void TraverseHold(UnityAction<EventHold> OnGet)
        {
            var list = new List<EventHold>(eventHolders);
            foreach (var item in list)
            {
                OnGet(item);
            }
        }
        public static void NoMessageHandle(string rMessage)
        {
            if (MessageNotHandled == null)
            {
                Debug.LogWarning("MessageDispatcher: Unhandled Message of type " + rMessage);
            }
            else
            {
                MessageNotHandled(rMessage);
            }
        }

        #endregion

    }
}