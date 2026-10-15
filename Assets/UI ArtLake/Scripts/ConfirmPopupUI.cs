using System;
using UnityEngine;
using UnityEngine.UI;

namespace ArtLake.UI
{
    public class ConfirmPopupUI : BaseUI
    {
        [Header("References")]
        [SerializeField] private Text messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private Action onConfirmAction;

        private void Reset()
        {
            panelType = PanelType.Popup;
        }

        private void OnEnable()
        {
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
            if (cancelButton != null) cancelButton.onClick.AddListener(OnCancel);
        }

        private void OnDisable()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveListener(OnConfirm);
            if (cancelButton != null) cancelButton.onClick.RemoveListener(OnCancel);
        }

        public void ShowPopup(string message, Action onConfirm)
        {
            if (messageText != null) messageText.text = message;
            onConfirmAction = onConfirm;
        }

        private void OnConfirm()
        {
            UIManager.Instance.Hide(this);
            onConfirmAction?.Invoke();
        }

        private void OnCancel()
        {
            UIManager.Instance.Hide(this);
        }
    }
}
