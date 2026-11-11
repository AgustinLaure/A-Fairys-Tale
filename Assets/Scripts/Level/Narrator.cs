using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Narrator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CanvasGroup textCanvasGroup;

    private GameManager gameManager;
    private EventBus eventBus;

    [Header("Configs")]
    [SerializeField] private float textShownTime;
    [SerializeField] private float timeToSpeech;
    [SerializeField] private float textFadeTime;

    private string[] phrases;

    private Coroutine showTextCoroutine = null;
    private Coroutine speechCoroutine = null;

    private int currentPhraseIndex = 0;

    private WaitForSeconds timeToSpeechTimer;
    private WaitForSeconds textShowTimer;

    private void Awake()
    {
        timeToSpeechTimer = new WaitForSeconds(timeToSpeech);
        textShowTimer = new WaitForSeconds(textShownTime);
    }

    private void Start()
    {
        ServiceLocator serviceLocator = ServiceLocator.Instance;
        eventBus = serviceLocator.GetService<EventBus>();
        gameManager = serviceLocator.GetService<GameManager>();

        phrases = gameManager.levelData.narratorPhrases;

        eventBus.Subscribe<OnNarratorSpeech>((Action<OnNarratorSpeech>)HandleSpeech);
    }

    private void Update()
    {
        if (speechCoroutine == null)
        {
            speechCoroutine = StartCoroutine(SpeechCoroutine());
        }
    }

    private void HandleSpeech(OnNarratorSpeech data)
    {
        if (showTextCoroutine != null)
        {
            if (speechCoroutine != null)
            {
                StopCoroutine(speechCoroutine);
            }

            if (data.phraseIndex > 0 && data.phraseIndex < phrases.Length)
            {
                currentPhraseIndex = data.phraseIndex;
            }

            if (showTextCoroutine == null)
            {
                showTextCoroutine = StartCoroutine(ShowTextCoroutine());
            }
        }
    }

    private IEnumerator ShowTextCoroutine()
    {
        text.text = phrases[currentPhraseIndex];

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / textFadeTime;
            textCanvasGroup.alpha = t;

            yield return null;
        }
        t = 1f;

        yield return textShowTimer;

        t = 1f;
        while (t > 0)
        {
            t -= Time.deltaTime / textFadeTime;
            textCanvasGroup.alpha = t;

            yield return null;
        }
        t = 0f;

        currentPhraseIndex++;
        if (currentPhraseIndex >= phrases.Length)
        {
            currentPhraseIndex = 0;
        }

        showTextCoroutine = null;
    }

    private IEnumerator SpeechCoroutine()
    {
        yield return timeToSpeechTimer;

        if (showTextCoroutine == null)
        {
            showTextCoroutine = StartCoroutine(ShowTextCoroutine());
        }

        speechCoroutine = null;
    }
}
