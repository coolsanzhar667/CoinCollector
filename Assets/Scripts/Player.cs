
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float jumpForce = 7f;
    public float fastFallForce = 15f;

    [Header("Health")]
    public int health = 100;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text healthText;
    public Slider healthSlider;
    public TMP_Text resultText;

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("Coins")]
    public int totalCoins = 5;
    public AudioClip coinSound;

    private int score = 0;
    private float horizontalInput;
    private bool isGrounded = false;
    private bool isGameOver = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color startColor;

    // НАЧАЛО ИГРЫ
    void Start()
    {
        Time.timeScale = 1f;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        startColor = sr.color;

        health = 100;
        score = 0;
        isGameOver = false;

        // Обновляем интерфейс
        scoreText.text = "Ұпай: 0";
        healthText.text = "Өмір: 100";
        healthSlider.value = 100;

        // Скрываем панели
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
    }

    // УПРАВЛЕНИЕ ПЕРСОНАЖЕМ
    void Update()
    {
        if (isGameOver || Time.timeScale == 0f)
            return;

        // A / D - движение
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // W / Space / стрелка вверх - прыжок
        if ((Input.GetKeyDown(KeyCode.W) ||
             Input.GetKeyDown(KeyCode.Space) ||
             Input.GetKeyDown(KeyCode.UpArrow))
             && isGrounded)
        {
            isGrounded = false;

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                0f
            );

            rb.AddForce(
                Vector2.up * jumpForce,
                ForceMode2D.Impulse
            );
        }
    }

    // ФИЗИКА ДВИЖЕНИЯ
    void FixedUpdate()
    {
        if (isGameOver || Time.timeScale == 0f)
            return;

        rb.linearVelocity = new Vector2(
            horizontalInput * speed,
            rb.linearVelocity.y
        );

        // S - ускоренное падение
        if (Input.GetKey(KeyCode.S) &&
            !isGrounded &&
            rb.linearVelocity.y < 0f)
        {
            rb.AddForce(
                Vector2.down * fastFallForce
            );
        }
    }

    // СТОЛКНОВЕНИЯ
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем касание земли
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact
                     in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        // Получение урона от Spike
        if (collision.gameObject.CompareTag("Enemy")
            && !isGameOver)
        {
            TakeDamage(20);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    // ПОЛУЧЕНИЕ УРОНА
    void TakeDamage(int damage)
    {
        health -= damage;

        // HP не может быть меньше 0
        health = Mathf.Max(health, 0);

        // Обновляем здоровье и полоску HP
        healthText.text = "Өмір: " + health;
        healthSlider.value = health;

        Debug.Log("Өмір: " + health);

        // Мигание персонажа
        StartCoroutine(Blink());

        if (health <= 0)
        {
            GameOver();
        }
    }

    // СБОР МОНЕТ
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin") && !isGameOver)
        {
            score++;

            scoreText.text = "Ұпай: " + score;

            Debug.Log("Ұпай: " + score);

            // Звук монеты
            if (coinSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    coinSound,
                    transform.position
                );
            }

            // Удаляем собранную монету
            Destroy(other.gameObject);

            // Проверяем победу
            if (score >= totalCoins)
            {
                WinGame();
            }
        }
    }

    // ПОБЕДА И СОХРАНЕНИЕ РЕКОРДА
    void WinGame()
    {
        isGameOver = true;

        // Время прохождения уровня
        float t = Time.timeSinceLevelLoad;

        // Отдельный рекорд для каждого уровня
        string key = "BestTime_" +
            SceneManager.GetActiveScene().name;

        // Получаем старый рекорд
        float best = PlayerPrefs.GetFloat(
            key,
            999f
        );

        // Если новое время лучше - сохраняем
        if (t < best)
        {
            best = t;

            PlayerPrefs.SetFloat(key, best);
            PlayerPrefs.Save();
        }

        // Выводим время и рекорд на экран
        if (resultText != null)
        {
            resultText.text =
                "Уақыт: " + t.ToString("F1") + " с";

            resultText.text +=
                "\nРекорд: " +
                best.ToString("F1") + " с";
        }
        else
        {
            Debug.LogWarning(
                "ResultText не подключён!"
            );
        }

        Debug.Log("Жеңіс!");
        Debug.Log("Уақыт: " + t.ToString("F1"));
        Debug.Log("Рекорд: " + best.ToString("F1"));

        // Показываем экран победы
        winPanel.SetActive(true);

        // Останавливаем игру
        Time.timeScale = 0f;
    }

    // КОНЕЦ ИГРЫ
    void GameOver()
    {
        isGameOver = true;

        Debug.Log("Ойын аяқталды!");

        gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // МИГАНИЕ ПРИ УРОНЕ
    IEnumerator Blink()
    {
        sr.color = Color.red;

        yield return new WaitForSecondsRealtime(0.2f);

        sr.color = startColor;
    }
}
