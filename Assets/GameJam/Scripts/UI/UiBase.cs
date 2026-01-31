using System.Collections;
using UnityEngine;

namespace DeadLine
{
    public abstract class UiBase : MonoBehaviour
    {
        private bool _isSubscribed;
        private Coroutine _subscriptionCoroutine;

        protected virtual void OnEnable()
        {
            if (_subscriptionCoroutine != null) StopCoroutine(_subscriptionCoroutine);
            _subscriptionCoroutine = StartCoroutine(SubscribeWhenReady());
        }

        protected virtual void OnDisable()
        {
            if (_subscriptionCoroutine != null)
            {
                StopCoroutine(_subscriptionCoroutine);
                _subscriptionCoroutine = null;
            }

            UnsubscribeIfNeeded();
        }

        private IEnumerator SubscribeWhenReady()
        {
            while (InputManager.Instance == null)
                yield return null;

            SubscribeIfNeeded();
        }

        private void SubscribeIfNeeded()
        {
            if (_isSubscribed) return;

            var inputManager = InputManager.Instance;
            if (inputManager == null) return;

            inputManager.UiEscPressed += OnEsc;
            inputManager.UiEnterPressed += OnEnter;
            inputManager.UiSpacePressed += OnSpace;
            inputManager.UiTabPressed += OnTab;

            _isSubscribed = true;
        }

        private void UnsubscribeIfNeeded()
        {
            if (!_isSubscribed) return;

            var im = InputManager.Instance;
            if (im == null)
            {
                _isSubscribed = false;
                return;
            }

            im.UiEscPressed -= OnEsc;
            im.UiEnterPressed -= OnEnter;
            im.UiSpacePressed -= OnSpace;
            im.UiTabPressed -= OnTab;

            _isSubscribed = false;
        }

        protected virtual void OnEsc() { }
        protected virtual void OnEnter() { }
        protected virtual void OnSpace() { }
        protected virtual void OnTab() { }
    }
}