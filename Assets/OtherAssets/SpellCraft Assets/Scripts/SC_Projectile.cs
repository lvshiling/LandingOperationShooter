using UnityEngine;

public class SC_Projectile : MonoBehaviour
{
    [Header("Config")]
    private bool isMoving = false;

    public float moveSpeed = 5.0f;
    public bool isBezier = false;

    [HideInInspector]
    public Transform target;

    private Hero ownHero;
    private UnitAI enemy;
    private float damageF;
    private float damageM;
    private float timer;

    private Vector3 startPos;
    private float lenMove;

    private void Update()
    {
        if (isMoving)
        {
            timer += Time.deltaTime;
            if (timer >= 5)
            {
                Destroy(gameObject);
            }
            if (!isBezier)
            {
                try
                {
                    transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * moveSpeed);
                    transform.LookAt(target);
                    if ((transform.position - target.position).magnitude < 0.05f)
                    {
                        ownHero.MakeRangeAttack();
                        Destroy(gameObject);
                    }
                }
                catch
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                UpdateBezier();
            }
        }
    }

    public void FireProjectile(Hero source, UnitAI hr, float dmgM, float dmgF)
    {
        ownHero = source;
        enemy = hr;
        damageF = dmgF;
        damageM = dmgM;
        this.target = hr.transform;

        startPos = transform.position;

        isMoving = true;
    }

    private void UpdateBezier()
    {
        Vector3 vec = target.position - startPos;
        float len1 = Mathf.Sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
        vec.Normalize();
        lenMove += Time.deltaTime * moveSpeed;
        Vector3 vecMove = vec * lenMove;

        float len2 = Mathf.Sqrt(vecMove.x * vecMove.x + vecMove.y * vecMove.y + vecMove.z * vecMove.z);

        float t = len2 / len1;

        float hight = len1 / 2;
        Vector2 point = GetPointBezier(new Vector2(startPos.x, startPos.y), new Vector2(target.position.x, 0),
            new Vector2((target.position.x - startPos.x) / 2 + startPos.x, (target.position.y - startPos.y) / 2 + startPos.y + hight), t);
        vecMove.y = point.y - startPos.y;
        transform.LookAt(startPos + vecMove);
        transform.position = startPos + vecMove;

        if (t >= 1.0f)
        {
            enemy.DamageTake(damageM, damageF);
            ownHero.MakeRangeAttack();
            Destroy(gameObject);
        }
    }

    public Vector2 GetPointBezier(Vector2 start, Vector2 end, Vector2 point, float t)
    {
        float x = Mathf.Pow((1 - t), 2) * start.x + 2 * t * (1 - t) * point.x + Mathf.Pow(t, 2) * end.x;
        float y = Mathf.Pow((1 - t), 2) * start.y + 2 * t * (1 - t) * point.y + Mathf.Pow(t, 2) * end.y;
        return new Vector2(x, y);
    }
}