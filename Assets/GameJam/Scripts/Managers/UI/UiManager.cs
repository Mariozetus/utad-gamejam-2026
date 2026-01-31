using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace DeadLine
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [SerializeField] private GameObject mainMenuRoot;
        [SerializeField] private GameObject levelsMenuRoot;
        [SerializeField] private GameObject settingsRoot;
        [SerializeField] private GameObject pauseMenuRoot;
        [SerializeField] private GameObject creditsRoot;
        [SerializeField] private GameObject popupRoot;

        [Header("Main Menu")]
        [SerializeField] private CanvasGroup mainMenuCanvasGroup;
        [SerializeField] private float mainMenuShowDelay = 0.2f;
        [SerializeField] private float mainMenuFadeInDuration = 0.35f;
        [SerializeField] private bool autoBindMainMenuCanvasGroup = true;

        [Header("Screen Tip")]
        [SerializeField] private GameObject screenTipRoot;
        [SerializeField] private TMP_Text screenTipText;

        [Header("Fade")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float defaultFadeDuration = 1f;

        [Header("Loading")]
        [SerializeField] private GameObject loadingRoot;
        [SerializeField] private CanvasGroup loadingCanvasGroup;
        [SerializeField] private float loadingFadeDuration = 0.15f;

        [Header("Loading")]
        [SerializeField] private float forcedLoadingMinTime = 0.35f;
        [SerializeField] private bool defaultToMainMenuIfNoneActive = true;

        [Header("Intro Credits")]
        [SerializeField] private GameObject introCreditsRoot;
        [SerializeField] private bool autoRebindUiRefsByName = true;
        [SerializeField] private string introCreditsName;
        [SerializeField] private float introFadeFromBlackDuration = -1f;

        [Header("Outro/Credits Cinematic Open")]
        [SerializeField] private float creditsFadeToBlackDuration = -1f;
        [SerializeField] private float creditsFadeFromBlackDuration = -1f;
        

        [SerializeField] private GameObject currentView;

        private readonly Stack<GameObject> _history = new Stack<GameObject>();
        private GameObject[] _views;

        private Coroutine _fadeCoroutine;
        private Coroutine _loadingFadeCoroutine;

        private Coroutine _introSequenceCoroutine;
        private Coroutine _creditsOpenCoroutine;

        private bool _pauseApplied;
        private float _timeScaleBeforePause = 1f;
        private bool _introShownThisLaunch;

        private int _screenTipRefCount;
        private string _lastScreenTipText;

        private InputManager _im;
        private bool _inputSubscribed;

        private bool _suppressAutoDefaultToMainMenu;

        private float _loadingForceUntilUnscaled;
        private bool _loadingWantsVisible;
        private Coroutine _loadingForceHideCoroutine;

        private Coroutine _mainMenuFadeCoroutine;

        public float LoadingFadeDuration => loadingFadeDuration;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;

            EnsureUiRefsIfNeeded();
            RefreshViewsCache();

            SetupCanvasGroups();
            SetupInitialUiState();

            BindInput();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                UnbindInput();
            }
        }

        private void Start()
        {
            EnsureUiRefsIfNeeded();
            RefreshViewsCache();

            currentView = DetectActiveView();

            if (!_suppressAutoDefaultToMainMenu &&
                currentView == null &&
                defaultToMainMenuIfNoneActive &&
                mainMenuRoot != null &&
                mainMenuRoot.scene.isLoaded)
            {
                _history.Clear();
                currentView = mainMenuRoot;
            }

            SyncViewsActiveState();
            ApplyPauseIfNeeded();
            UpdateInputContext();

            if (currentView == mainMenuRoot)
                StartMainMenuFadeIn(true);

            ShowIntroCreditsOncePerLaunch();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureUiRefsIfNeeded();
            RefreshViewsCache();

            if (currentView != null && !currentView.scene.isLoaded)
                currentView = null;

            var cleaned = new Stack<GameObject>();
            while (_history.Count > 0)
            {
                var v = _history.Pop();
                if (v != null && v.scene.isLoaded)
                    cleaned.Push(v);
            }
            while (cleaned.Count > 0)
                _history.Push(cleaned.Pop());

            if (_suppressAutoDefaultToMainMenu)
            {
                SyncViewsActiveState();
                ApplyPauseIfNeeded();
                UpdateInputContext();

                if (currentView == mainMenuRoot)
                    StartMainMenuFadeIn(true);
                return;
            }

            var active = DetectActiveView();
            if (active != null)
            {
                currentView = active;
            }
            else if (defaultToMainMenuIfNoneActive && mainMenuRoot != null && mainMenuRoot.scene.isLoaded)
            {
                currentView = mainMenuRoot;
            }

            SyncViewsActiveState();
            ApplyPauseIfNeeded();
            UpdateInputContext();

            if (currentView == mainMenuRoot)
                StartMainMenuFadeIn(true);
        }

        private void BindInput()
        {
            if (_inputSubscribed) return;

            _im = InputManager.Instance;
            if (_im == null) return;

            _im.PausePressed += OnPausePressed;
            _im.UiEscPressed += OnUiEscPressed;

            _inputSubscribed = true;
        }

        private void UnbindInput()
        {
            if (!_inputSubscribed) return;

            if (_im != null)
            {
                _im.PausePressed -= OnPausePressed;
                _im.UiEscPressed -= OnUiEscPressed;
            }

            _inputSubscribed = false;
            _im = null;
        }

        private void OnPausePressed()
        {
            HandleEscape();
        }

        private void OnUiEscPressed()
        {
            HandleEscape();
        }

        private void EnsureUiRefsIfNeeded()
        {
            if (!autoRebindUiRefsByName) return;

            if (introCreditsRoot == null && !string.IsNullOrWhiteSpace(introCreditsName))
            {
                var found = GameObject.Find(introCreditsName);
                if (found != null) introCreditsRoot = found;
            }

            if (autoBindMainMenuCanvasGroup && mainMenuCanvasGroup == null && mainMenuRoot != null)
            {
                mainMenuCanvasGroup = mainMenuRoot.GetComponentInChildren<CanvasGroup>(true);
            }
        }

        private void RefreshViewsCache()
        {
            _views = new[]
            {
                mainMenuRoot,
                levelsMenuRoot,
                settingsRoot,
                pauseMenuRoot,
                creditsRoot
            };
        }

        private void SetupCanvasGroups()
        {
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.gameObject.SetActive(true);
                fadeCanvasGroup.alpha = 0f;
                fadeCanvasGroup.blocksRaycasts = false;
                fadeCanvasGroup.interactable = false;
            }

            if (loadingCanvasGroup != null)
            {
                loadingCanvasGroup.alpha = 0f;
                loadingCanvasGroup.blocksRaycasts = false;
                loadingCanvasGroup.interactable = false;
            }
        }

        private void SetupInitialUiState()
        {
            if (loadingRoot != null)
                loadingRoot.SetActive(false);

            if (screenTipRoot != null)
                screenTipRoot.SetActive(false);
        }

        private GameObject DetectActiveView()
        {
            if (_views == null) RefreshViewsCache();

            foreach (var v in _views)
            {
                if (v != null && v.activeSelf)
                    return v;
            }
            return null;
        }

        public void NavigateTo(GameObject target, bool pushCurrent = true, bool clearHistory = false)
        {
            _suppressAutoDefaultToMainMenu = false;

            if (clearHistory)
                _history.Clear();

            if (target == currentView)
                return;

            if (pushCurrent && currentView != null)
                _history.Push(currentView);

            currentView = target;

            SyncViewsActiveState();
            ApplyPauseIfNeeded();
            UpdateInputContext();

            if (currentView == mainMenuRoot)
                StartMainMenuFadeIn(true);
            else
                StopMainMenuFadeIfRunning();
        }

        public void GoBack()
        {
            if (_history.Count == 0)
                return;

            currentView = _history.Pop();

            SyncViewsActiveState();
            ApplyPauseIfNeeded();
            UpdateInputContext();

            if (currentView == mainMenuRoot)
                StartMainMenuFadeIn(true);
            else
                StopMainMenuFadeIfRunning();
        }

        public void ClearHistory() => _history.Clear();

        public void ClearCurrentView()
        {
            _suppressAutoDefaultToMainMenu = true;
            currentView = null;
            SyncViewsActiveState();
            ApplyPauseIfNeeded();
            UpdateInputContext();

            StopMainMenuFadeIfRunning();
        }

        public void HandleEscape()
        {
            EnsureUiRefsIfNeeded();

            if (introCreditsRoot != null && introCreditsRoot.activeSelf)
                return;

            if (popupRoot != null && popupRoot.activeSelf)
            {
                HidePopup();
                UpdateInputContext();
                return;
            }

            if (_history.Count > 0)
            {
                GoBack();
                return;
            }

            if (IsPausedContext())
            {
                HidePauseMenu();
                return;
            }

            if (currentView == mainMenuRoot && mainMenuRoot != null && mainMenuRoot.scene.isLoaded)
                return;

            ShowPauseMenu();
        }

        public void ShowMainMenu(bool useDelayThenFadeIn = true)
        {
            _suppressAutoDefaultToMainMenu = false;
            NavigateTo(mainMenuRoot, false, true);

            if (useDelayThenFadeIn)
                StartMainMenuFadeIn(true);
            else if (mainMenuCanvasGroup != null)
            {
                mainMenuCanvasGroup.alpha = 1f;
                mainMenuCanvasGroup.blocksRaycasts = true;
                mainMenuCanvasGroup.interactable = true;
            }
        }

        public void ShowLevels(bool pushCurrent = true) => NavigateTo(levelsMenuRoot, pushCurrent);
        public void ShowSettings(bool pushCurrent = true) => NavigateTo(settingsRoot, pushCurrent);
        public void ShowCredits(bool pushCurrent = true) => NavigateTo(creditsRoot, pushCurrent);

        public void ShowLevelsMenuFromMainMenu() => ShowLevels(true);
        public void ShowSettingsFromMainMenu() => ShowSettings(true);

        public void ShowCreditsFromMainMenu() => OpenCreditsWithFade(true);

        public void OpenCreditsWithFade(bool pushCurrent = true)
        {
            if (creditsRoot == null) return;

            if (_creditsOpenCoroutine != null)
                StopCoroutine(_creditsOpenCoroutine);

            _creditsOpenCoroutine = StartCoroutine(CreditsOpenSequence(pushCurrent));
        }

        private IEnumerator CreditsOpenSequence(bool pushCurrent)
        {
            float toBlack = creditsFadeToBlackDuration > 0f ? creditsFadeToBlackDuration : defaultFadeDuration;
            float fromBlack = creditsFadeFromBlackDuration > 0f ? creditsFadeFromBlackDuration : defaultFadeDuration;

            yield return FadeToBlackYield(toBlack);

            NavigateTo(creditsRoot, pushCurrent);

            yield return FadeFromBlackYield(fromBlack);

            _creditsOpenCoroutine = null;
        }

        public void ShowPauseMenu()
        {
            if (pauseMenuRoot == null) return;
            NavigateTo(pauseMenuRoot, true);
        }

        public void HidePauseMenu()
        {
            if (currentView == pauseMenuRoot)
            {
                if (_history.Count > 0)
                    currentView = _history.Pop();
                else
                    currentView = null;

                SyncViewsActiveState();
                ApplyPauseIfNeeded();
                UpdateInputContext();

                if (currentView == mainMenuRoot)
                    StartMainMenuFadeIn(true);
                else
                    StopMainMenuFadeIfRunning();
            }
        }

        public void ShowPopup() => SetActiveUi(popupRoot, true);
        public void HidePopup() => SetActiveUi(popupRoot, false);

        public void ShowScreenTip(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            _screenTipRefCount++;
            _lastScreenTipText = text;

            if (screenTipText != null)
                screenTipText.text = text;

            SetActiveUi(screenTipRoot, true);
        }

        public void HideScreenTip()
        {
            if (_screenTipRefCount > 0)
                _screenTipRefCount--;

            if (_screenTipRefCount > 0)
            {
                if (screenTipText != null && !string.IsNullOrEmpty(_lastScreenTipText))
                    screenTipText.text = _lastScreenTipText;

                SetActiveUi(screenTipRoot, true);
                return;
            }

            _lastScreenTipText = null;
            SetActiveUi(screenTipRoot, false);
        }

        public void ShowIntroCreditsOncePerLaunch()
        {
            if (_introShownThisLaunch) return;
            _introShownThisLaunch = true;

            EnsureUiRefsIfNeeded();
            if (introCreditsRoot == null) return;

            if (_introSequenceCoroutine != null)
                StopCoroutine(_introSequenceCoroutine);

            _introSequenceCoroutine = StartCoroutine(IntroSequence());
        }

        private IEnumerator IntroSequence()
        {
            ForceBlackInstant();

            introCreditsRoot.SetActive(true);
            UpdateInputContext();

            float dur = introFadeFromBlackDuration > 0f ? introFadeFromBlackDuration : defaultFadeDuration;
            yield return FadeFromBlackYield(dur);

            _introSequenceCoroutine = null;
        }

        public void HideIntroCredits()
        {
            EnsureUiRefsIfNeeded();
            if (introCreditsRoot == null) return;

            introCreditsRoot.SetActive(false);
            UpdateInputContext();
        }

        private void ForceBlackInstant()
        {
            if (fadeCanvasGroup == null) return;

            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.interactable = true;
        }

        public void ShowLoading(bool fade = true, float minVisibleSeconds = -1f)
        {
            if (loadingRoot == null) return;

            _loadingWantsVisible = true;

            float minT = (minVisibleSeconds > 0f) ? minVisibleSeconds : forcedLoadingMinTime;
            if (minT > 0f)
                _loadingForceUntilUnscaled = Mathf.Max(_loadingForceUntilUnscaled, Time.unscaledTime + minT);

            if (_loadingForceHideCoroutine != null)
            {
                StopCoroutine(_loadingForceHideCoroutine);
                _loadingForceHideCoroutine = null;
            }

            loadingRoot.SetActive(true);

            if (!fade || loadingCanvasGroup == null)
            {
                if (loadingCanvasGroup != null) loadingCanvasGroup.alpha = 1f;
                return;
            }

            StartLoadingFade(loadingCanvasGroup.alpha, 1f, loadingFadeDuration, null);
        }

        public void HideLoading(bool fade = true)
        {
            HideLoadingInternal(fade, false);
        }

        private void HideLoadingInternal(bool fade, bool ignoreForceTime)
        {
            if (loadingRoot == null) return;

            _loadingWantsVisible = false;

            if (!ignoreForceTime)
            {
                float now = Time.unscaledTime;
                if (now < _loadingForceUntilUnscaled)
                {
                    float wait = _loadingForceUntilUnscaled - now;

                    if (_loadingForceHideCoroutine != null)
                        StopCoroutine(_loadingForceHideCoroutine);

                    _loadingForceHideCoroutine = StartCoroutine(ForceHideAfterDelay(wait, fade));
                    return;
                }
            }

            if (_loadingForceHideCoroutine != null)
            {
                StopCoroutine(_loadingForceHideCoroutine);
                _loadingForceHideCoroutine = null;
            }

            if (!fade || loadingCanvasGroup == null)
            {
                if (loadingCanvasGroup != null) loadingCanvasGroup.alpha = 0f;
                loadingRoot.SetActive(false);
                return;
            }

            StartLoadingFade(loadingCanvasGroup.alpha, 0f, loadingFadeDuration, () =>
            {
                loadingRoot.SetActive(false);
            });
        }

        private IEnumerator ForceHideAfterDelay(float seconds, bool fade)
        {
            yield return new WaitForSecondsRealtime(seconds);

            _loadingForceHideCoroutine = null;

            if (_loadingWantsVisible)
                yield break;

            HideLoadingInternal(fade, true);
        }

        private void StartLoadingFade(float from, float to, float duration, Action onComplete)
        {
            if (loadingCanvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (_loadingFadeCoroutine != null)
                StopCoroutine(_loadingFadeCoroutine);

            _loadingFadeCoroutine = StartCoroutine(LoadingFadeRoutine(from, to, duration, onComplete));
        }

        private IEnumerator LoadingFadeRoutine(float from, float to, float duration, Action onComplete)
        {
            loadingCanvasGroup.blocksRaycasts = true;
            loadingCanvasGroup.interactable = true;

            float t = 0f;
            loadingCanvasGroup.alpha = from;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float lerp = Mathf.Clamp01(t / duration);
                loadingCanvasGroup.alpha = Mathf.Lerp(from, to, lerp);
                yield return null;
            }

            loadingCanvasGroup.alpha = to;

            if (Mathf.Approximately(to, 0f))
            {
                loadingCanvasGroup.blocksRaycasts = false;
                loadingCanvasGroup.interactable = false;
            }

            onComplete?.Invoke();
            _loadingFadeCoroutine = null;
        }

        public void FadeOut(float duration = -1f, Action onComplete = null)
        {
            if (duration <= 0f) duration = defaultFadeDuration;
            StartFade(0f, 1f, duration, onComplete);
        }

        public void FadeIn(float duration = -1f, Action onComplete = null)
        {
            if (duration <= 0f) duration = defaultFadeDuration;
            StartFade(1f, 0f, duration, onComplete);
        }

        public void FadeToBlack(float duration = -1f, Action onComplete = null) => FadeOut(duration, onComplete);
        public void FadeFromBlack(float duration = -1f, Action onComplete = null) => FadeIn(duration, onComplete);

        private IEnumerator FadeToBlackYield(float duration)
        {
            bool done = false;
            FadeToBlack(duration, () => done = true);
            while (!done) yield return null;
        }

        private IEnumerator FadeFromBlackYield(float duration)
        {
            bool done = false;
            FadeFromBlack(duration, () => done = true);
            while (!done) yield return null;
        }

        private void StartFade(float from, float to, float duration, Action onComplete)
        {
            if (fadeCanvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = StartCoroutine(FadeRoutine(from, to, duration, onComplete));
        }

        private IEnumerator FadeRoutine(float from, float to, float duration, Action onComplete)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.interactable = true;

            float t = 0f;
            fadeCanvasGroup.alpha = from;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float lerp = Mathf.Clamp01(t / duration);
                fadeCanvasGroup.alpha = Mathf.Lerp(from, to, lerp);
                yield return null;
            }

            fadeCanvasGroup.alpha = to;

            if (Mathf.Approximately(to, 0f))
            {
                fadeCanvasGroup.blocksRaycasts = false;
                fadeCanvasGroup.interactable = false;
            }

            onComplete?.Invoke();
            _fadeCoroutine = null;
        }

        private void SyncViewsActiveState()
        {
            if (_views == null || _views.Length == 0)
                RefreshViewsCache();

            foreach (var v in _views)
            {
                if (v == null) continue;
                bool shouldShow = (currentView != null && v == currentView);
                if (v.activeSelf != shouldShow) v.SetActive(shouldShow);
            }
        }

        private bool IsPausedContext()
        {
            if (currentView == pauseMenuRoot)
                return true;

            foreach (var v in _history)
            {
                if (v == pauseMenuRoot)
                    return true;
            }

            return false;
        }

        private void ApplyPauseIfNeeded()
        {
            bool shouldPause = IsPausedContext();

            if (shouldPause)
            {
                if (!_pauseApplied)
                {
                    _timeScaleBeforePause = Time.timeScale;
                    _pauseApplied = true;
                }

                Time.timeScale = 0f;
            }
            else
            {
                if (_pauseApplied)
                {
                    Time.timeScale = _timeScaleBeforePause;
                    _pauseApplied = false;
                }
            }
        }

        private void UpdateInputContext()
        {
            var im = InputManager.Instance;
            if (im == null) return;

            bool introActive = introCreditsRoot != null && introCreditsRoot.activeSelf;
            bool popupActive = popupRoot != null && popupRoot.activeSelf;
            bool anyViewActive = currentView != null;

            bool wantUi = introActive || popupActive || anyViewActive;

            if (wantUi)
                im.EnableUiInputActions();
            else
                im.EnablePlayerInputActions();
        }

        private void SetActiveUi(GameObject go, bool active)
        {
            if (go == null) return;
            if (go.activeSelf == active) return;
            go.SetActive(active);
        }

        private void StopMainMenuFadeIfRunning()
        {
            if (_mainMenuFadeCoroutine != null)
            {
                StopCoroutine(_mainMenuFadeCoroutine);
                _mainMenuFadeCoroutine = null;
            }
        }

        private void StartMainMenuFadeIn(bool useDelay)
        {
            EnsureUiRefsIfNeeded();

            if (mainMenuRoot == null) return;
            if (currentView != mainMenuRoot) return;
            if (mainMenuCanvasGroup == null) return;

            StopMainMenuFadeIfRunning();
            _mainMenuFadeCoroutine = StartCoroutine(MainMenuFadeInRoutine(useDelay));
        }

        private IEnumerator MainMenuFadeInRoutine(bool useDelay)
        {
            mainMenuCanvasGroup.gameObject.SetActive(true);

            mainMenuCanvasGroup.alpha = 0f;
            mainMenuCanvasGroup.blocksRaycasts = false;
            mainMenuCanvasGroup.interactable = false;

            if (useDelay && mainMenuShowDelay > 0f)
                yield return new WaitForSecondsRealtime(mainMenuShowDelay);

            float dur = Mathf.Max(0.01f, mainMenuFadeInDuration);
            float t = 0f;

            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float lerp = Mathf.Clamp01(t / dur);
                mainMenuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, lerp);
                yield return null;
            }

            mainMenuCanvasGroup.alpha = 1f;
            mainMenuCanvasGroup.blocksRaycasts = true;
            mainMenuCanvasGroup.interactable = true;

            _mainMenuFadeCoroutine = null;
        }
    }
}
