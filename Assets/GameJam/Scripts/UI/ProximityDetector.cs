using UnityEngine;
using System.Collections.Generic;
public class ProximityDetector : MonoBehaviour
{
        [SerializeField] private ProximityPrompt promptPrefab;
        [SerializeField] private Transform promptParent;

        private readonly HashSet<ObjectBase> _candidates = new();
        private ProximityPrompt _promptInstance;
        private ObjectBase _currentTarget;
        private Camera _camera;

        private bool _inputSubscribed;
        private Coroutine _subscribeCoroutine;
        
        private void OnEnable()
        {
            if (_subscribeCoroutine != null) StopCoroutine(_subscribeCoroutine);
            _subscribeCoroutine = StartCoroutine(SubscribeWhenReady());
        }

        private void OnDisable()
        {
            if (_subscribeCoroutine != null)
            {
                StopCoroutine(_subscribeCoroutine);
                _subscribeCoroutine = null;
            }

            UnsubscribeIfNeeded();
        }

        private void Awake()
        {
            _camera = Camera.main;
        }

        public void Register(ObjectBase interactableObject)
        {
            if (interactableObject != null)
                _candidates.Add(interactableObject);
        }


        public void Unregister(ObjectBase interactableObject)
        {
            if (interactableObject != null)
                _candidates.Remove(interactableObject);
        }


        public HashSet<ObjectBase> GetCandidates()
        {

            _candidates.RemoveWhere(interactableObject => interactableObject == null);
            return _candidates;
        }


        public bool HasCandidates()
        {
            _candidates.RemoveWhere(interactableObject => interactableObject == null);
            return _candidates.Count > 0;
        }

        public void ClearCandidates()
        {
            _candidates.Clear();
            SetCurrentTarget(null);
        }

        private System.Collections.IEnumerator SubscribeWhenReady()
        {
            while (InputManager.Instance == null)
                yield return null;

            SubscribeIfNeeded();
        }

        private void SubscribeIfNeeded()
        {
            if (_inputSubscribed) return;
            var inputManager = InputManager.Instance;
            if (inputManager == null) return;

            inputManager.InteractPressed += OnInteractionPressed;
            _inputSubscribed = true;
        }

        private void UnsubscribeIfNeeded()
        {
            if (!_inputSubscribed) return;
            var inputManager = InputManager.Instance;
            if (inputManager != null)
                inputManager.InteractPressed -= OnInteractionPressed;
            _inputSubscribed = false;
        }

        private void OnInteractionPressed()
        {
            if (_currentTarget != null)
            {
                _currentTarget.Interact(gameObject);
                ClearCandidates();
                return;
            }

            ClearCandidates();
        }

        private void Update()
        {
            UpdateTarget();
        }

        private void UpdateTarget()
        {
            _candidates.RemoveWhere(interactableObject => interactableObject == null);
            if (_candidates.Count == 0)
            {
                SetCurrentTarget(null);
                return;
            }

            ObjectBase best = null;
            float bestSqr = float.MaxValue;
            Vector3 pos = transform.position;
            foreach (var candidate in _candidates)
            {
                if (candidate == null) continue;
                float sqr = (candidate.transform.position - pos).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = candidate;
                }
            }

            SetCurrentTarget(best);
        }

        private void SetCurrentTarget(ObjectBase target)
        {
            if (_currentTarget == target)
            {
                if (_currentTarget == null)
                    EnsurePromptHidden();
                return;
            }

            _currentTarget = target;
            if (_currentTarget == null)
            {
                EnsurePromptHidden();
                return;
            }

            EnsurePrompt();
            if (_promptInstance != null)
                _promptInstance.Show(_currentTarget.GetPromptAnchor(), _camera);
        }

        private void EnsurePrompt()
        {
            if (_promptInstance != null) return;
            if (promptPrefab == null) return;

            _promptInstance = Instantiate(promptPrefab, promptParent);
            _promptInstance.Hide();
        }

        private void EnsurePromptHidden()
        {
            if (_promptInstance != null)
                _promptInstance.Hide();
        }

}
