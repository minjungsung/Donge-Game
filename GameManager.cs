using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    [Header("---------- [Core]")]

    public bool isGameOver;
    public int score;
    public int maxLevel;

    [Header("---------- [Object Pooling]")]
    public GameObject donglePrefab;
    public Transform dongleGroup;
    public List<Dongle> donglePool;
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;

    [Range(1, 30)]

    public int poolSize;
    public int poolCursor;
    public Dongle lastDongle;


    [Header("---------- [Audio]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx { LevelUp, Next, Attach, Button, GameOver };
    int sfxCursor;

    [Header("---------- [UI]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text maxScoreText;
    public Text subTextScore;

    [Header("---------- [ETC]")]
    public GameObject line;
    public GameObject bottom;

    public void GameStart()
    {
        line.SetActive(true);
        bottom.SetActive(true);

        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        bgmPlayer.Play();
        SfxPlay(Sfx.Button);
        Invoke("NextDongle", 1.5f);

    }

    void Awake()
    {
        Application.targetFrameRate = 60;
        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for (int i = 0; i < poolSize; i++)
        {
            MakeDongle();
        }

        if (!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }
        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }

    void NextDongle()
    {
        if (isGameOver)
            return;

        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine(WaitNext());
    }

    IEnumerator WaitNext()
    {
        while (lastDongle != null)
        {
            yield return null;
        }
        yield return new WaitForSeconds(2.5f);
        NextDongle();
    }

    Dongle MakeDongle()
    {
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect" + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // 동글 생성
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle" + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.manager = this;
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);

        return instantDongle;
    }

    Dongle GetDongle()
    {
        for (int i = 0; i < donglePool.Count; i++)
        {
            poolCursor = (poolCursor + 1) % donglePool.Count;
            if (donglePool[poolCursor].gameObject.activeSelf == false)
                return donglePool[poolCursor];
        }
        return MakeDongle();
    }

    public void TouchDown()
    {
        if (lastDongle == null)
            return;

        lastDongle.Drag();
    }

    public void TouchUp()
    {
        Debug.Log("TouchUp");
        if (lastDongle == null)
            return;

        lastDongle.Drop();
        lastDongle = null;
    }

    public void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].rigid.simulated = false;
        }


        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].Hide(Vector3.up * 1000);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1.0f);

        if (score > PlayerPrefs.GetInt("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", score);
            maxScoreText.text = score.ToString();
        }

        endGroup.SetActive(true);
        subTextScore.text = "점수: " + score.ToString();

        bgmPlayer.Stop();
        SfxPlay(Sfx.GameOver);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine(ResetCoroutine());
    }

    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Main");
    }

    public void SfxPlay(Sfx type)
    {
        switch (type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.GameOver:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }
        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            Application.Quit();
    }

    void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
