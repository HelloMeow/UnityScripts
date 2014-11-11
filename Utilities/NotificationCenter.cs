using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Notification {
    public string Name = null;
    public object Data = null;

    public Notification(string name) {
        this.Name = name;
    }

    public Notification(string name, object data) {
        this.Name = name;
        this.Data = data;
    }
}

public class NotificationCenter {
    public delegate void OnNotification(Notification notification);

    #region Publish

    public void Publish(Notification notification) {
        if (notification == null) return;
        if (notification.Name == null || notification.Name.Length <= 0) return;
        if (!mDelegates.ContainsKey(notification.Name)) return;
        if (mDelegates[notification.Name] == null) return;
        ((OnNotification)mDelegates[notification.Name])(notification);
    }

    public void Publish(string name) {
        Notification notification = new Notification(name);
        Publish(notification);
    }

    public void Publish(string name, object data) {
        Notification notification = new Notification(name, data);
        Publish(notification);
    }

    #endregion

    #region Subscription

    public void Subscribe(string name, OnNotification callback) {
        if (name == null || name.Length <= 0) return;
        if (callback == null) return;

        if (!mDelegates.ContainsKey(name)) {
            mDelegates.Add(name, callback);
        } else {
            mDelegates[name] = (OnNotification)mDelegates[name] + callback;
        }
    }

    public void Unsubscribe(string name, OnNotification callback) {
        if (name == null || name.Length <= 0) return;
        if (callback == null) return;
        if (!mDelegates.ContainsKey(name)) return;

        mDelegates[name] = (OnNotification)mDelegates[name] - callback;
    }

    #endregion

    #region Instance

    private static NotificationCenter instance;

    private NotificationCenter() {
        mDelegates = new Dictionary<string, Delegate>();
    }

    public static NotificationCenter Instance {
        get {
            if (instance == null) {
                instance = new NotificationCenter();
            }
            return instance;
        }
    }

    #endregion

    private Dictionary<string, Delegate> mDelegates;

    internal void Publish(Notification notification, ItemBaseInfo itemBaseInfo) {
        throw new NotImplementedException();
    }
}

