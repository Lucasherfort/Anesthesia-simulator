// GENERATED AUTOMATICALLY FROM 'Assets/Config/Controls/Controls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Controls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""Apps"",
            ""id"": ""cce3e72e-81fa-4afa-87f2-aed8d72fe798"",
            ""actions"": [
                {
                    ""name"": ""Quit"",
                    ""type"": ""Button"",
                    ""id"": ""6264e53b-334a-4d23-b5f8-b59a07fa754c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""cc9e4e1a-4e95-4a64-8f00-3a9366355aaa"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Quit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Apps
        m_Apps = asset.FindActionMap("Apps", throwIfNotFound: true);
        m_Apps_Quit = m_Apps.FindAction("Quit", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Apps
    private readonly InputActionMap m_Apps;
    private IAppsActions m_AppsActionsCallbackInterface;
    private readonly InputAction m_Apps_Quit;
    public struct AppsActions
    {
        private @Controls m_Wrapper;
        public AppsActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Quit => m_Wrapper.m_Apps_Quit;
        public InputActionMap Get() { return m_Wrapper.m_Apps; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(AppsActions set) { return set.Get(); }
        public void SetCallbacks(IAppsActions instance)
        {
            if (m_Wrapper.m_AppsActionsCallbackInterface != null)
            {
                @Quit.started -= m_Wrapper.m_AppsActionsCallbackInterface.OnQuit;
                @Quit.performed -= m_Wrapper.m_AppsActionsCallbackInterface.OnQuit;
                @Quit.canceled -= m_Wrapper.m_AppsActionsCallbackInterface.OnQuit;
            }
            m_Wrapper.m_AppsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Quit.started += instance.OnQuit;
                @Quit.performed += instance.OnQuit;
                @Quit.canceled += instance.OnQuit;
            }
        }
    }
    public AppsActions @Apps => new AppsActions(this);
    public interface IAppsActions
    {
        void OnQuit(InputAction.CallbackContext context);
    }
}
