using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class GlobalEventManager : MonoBehaviour
    {
        private readonly Dictionary<string, Action<GameObject, List<object>>> simpleEventDictionary = new Dictionary<string, Action<GameObject, List<object>>>();
        private readonly Dictionary<string, Action> simplestEventDictionary = new Dictionary<string, Action>();

        public void StartListening(string eventName, Action<GameObject, List<object>> listener)
        {
            if (simplestEventDictionary.ContainsKey(eventName))
            {
                Debug.LogWarning($"Duplicate event {eventName} in dictionary with other parameters, recommended to use unique event names");
            }
            if (simpleEventDictionary.ContainsKey(eventName))
            {
                simpleEventDictionary[eventName] += listener;
            }
            else
            {
                simpleEventDictionary.Add(eventName, listener);
            }
        }

        public void StartListening(string eventName, Action listener)
        {
            if (simpleEventDictionary.ContainsKey(eventName))
            {
                Debug.LogWarning($"Duplicate event {eventName} in dictionary with other parameters, recommended to use unique event names");
            }
            if (simplestEventDictionary.ContainsKey(eventName))
            {
                simplestEventDictionary[eventName] += listener;
            }
            else
            {
                simplestEventDictionary.Add(eventName, listener);
            }
        }

        public void StopListening(string eventName, Action<GameObject, List<object>> listener)
        {
            if (simplestEventDictionary.ContainsKey(eventName))
            {
                Debug.LogWarning("Duplicate event in dictionary with other parameters, recommended to use unique event names");
            }
            if (simpleEventDictionary.ContainsKey(eventName))
            {
                simpleEventDictionary[eventName] -= listener;
                if (simpleEventDictionary[eventName] == null)
                {
                    simpleEventDictionary.Remove(eventName);
                }
            }
        }

        public void StopListening(string eventName, Action listener)
        {
            if (simpleEventDictionary.ContainsKey(eventName))
            {
                Debug.LogWarning($"Duplicate event {eventName} in dictionary with other parameters, recommended to use unique event names");
            }
            if (simplestEventDictionary.ContainsKey(eventName))
            {
                simplestEventDictionary[eventName] -= listener;
                if (simplestEventDictionary[eventName] == null)
                {
                    simplestEventDictionary.Remove(eventName);
                }
            }
        }

        public void TriggerEvent(string eventName, GameObject invoker = null, List<object> parameters = null)
        {
            parameters = parameters == null ? new List<object>() : parameters;
            Action<GameObject, List<object>> thisSimpleEvent;
            Action thisSimplestEvent;
            if (simpleEventDictionary.TryGetValue(eventName, out thisSimpleEvent))
            {
                thisSimpleEvent.Invoke(invoker, parameters);
            }
            else if (simplestEventDictionary.TryGetValue(eventName, out thisSimplestEvent))
            {
                thisSimplestEvent.Invoke();
            }
        }
    }
}