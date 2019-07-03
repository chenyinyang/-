using UnityEngine;
using System.Collections;

public class ChracterBase : MonoBehaviour {

    public float AttackRange = 5f;
    protected ScanEnemy scanEnemy;
    protected Attacker attacker;
    protected Transform trans;
    protected Animator ani;
    protected HUDText hd;
    private object locker = new object();
    public bool Running
    {
        get
        {
            return this.ani.GetBool("running");
        }
        protected set
        {
            this.ani.SetBool("running", value);
        }
    }
    protected bool Walk
    {
        get
        {
            return this.ani.GetBool("walk");
        }
        set
        {
            this.ani.SetBool("walk", value);
        }
    }
    protected float MoveSpeed
    {
        get
        {
            return this.attacker.speed.MaxValue/5 * (this.Running ? 2f : (this.Walk ? 1f : 0f));
        }
    }

    
    public Transform target
    {
        get
        {
            if (this.attacker.Target == null)
                return null;
            return this.attacker.Target.transform;
        }
    }
    protected bool hasHitdown=false;
    protected bool aiEnable;
    protected float escapeTo;
    protected Coroutine aiAction;
    protected AIMode _preAiMode;
    public AIMode _aiMode = AIMode.NoAI;
    public AIMode AiMode {
        get { return this._aiMode; }
        protected set {
            lock (locker)
            {
                if (!aiEnable)
                    return;
                if (this._aiMode == value)
                    return;
                this._preAiMode = this._aiMode;
                this._aiMode = value;

                if (aiAction != null)
                {
                    StopCoroutine(aiAction);
                    this.attacker.StopAttack();
                }
                switch (this._aiMode)
                {
                    case AIMode.Random:
                        aiAction = StartCoroutine(this.AIRandomMove());
                        break;
                    case AIMode.Attack:
                        aiAction = StartCoroutine(this.AIAttack());
                        break;
                    case AIMode.Chase:
                        aiAction = StartCoroutine(this.AIChase());
                        break;
                    case AIMode.Escape:
                        aiAction = StartCoroutine(this.AIEscape());
                        break;
                    case AIMode.NoAI:
                    default:
                        break;
                }
            }
        }
    }
    private void Awake()
    {
        this.trans = this.transform;
        this.ani = this.GetComponent<Animator>();
        this.attacker = this.GetComponent<Attacker>();
        this.scanEnemy = this.GetComponentInChildren<ScanEnemy>();
        aiEnable = true;
        this.Walk = true;
        this.Running = false;
    }
    // Use this for initialization
    void Start () {
       
        this.AiMode = AIMode.Random;
    }
	
	// Update is called once per frame
	void Update () {
        
    }
    public virtual float PlayEnterCombat()
    {
        this.ani.Play("Shout");
        return 2f;
    }
    public virtual float PlayOnHit()
    {
        this.ani.Play("Get_Hit");
        return 2f;
    }
    public virtual void PlayAttack()
    {

        this.ani.Play("Attack");
    }
    public virtual void PlayDeath()
    {

        this.ani.Play("Death1");
    }
    protected virtual IEnumerator AIRandomMove() {
        this.Running = false;
        this.Walk = true;
        while (aiEnable)
        {            
            int rnd = Random.Range(0, 100);
            if (rnd <= 10)
            {
                for (int i = 1; i < 10+rnd*5; i++)
                {
                    this.trans.Rotate(Vector3.up, 1);
                    yield return new WaitForSeconds(0.03f);
                }
            }
            else if (rnd <= 20)
            {
                
                for (int i = 1; i < 10+(rnd-10)*5; i++)
                {
                    this.trans.Rotate(Vector3.up, -1);
                    yield return new WaitForSeconds(0.03f);
                }
            }
            else if (rnd <= 50)
            {
                this.Walk = !this.Walk;
                this.Running = !this.Running;

            }
            for (int i = 0; i < 100; i++)
            {
                this.trans.position += this.trans.forward * this.MoveSpeed / 100;
                yield return new WaitForSeconds(0.05f);
            }
            
        }                
    }

    protected IEnumerator AIChase()
    {
        yield return new WaitForSeconds(0.03f);
        this.Running = false;
        this.Walk = false;
        if (this._preAiMode == AIMode.Random)
        {            
            yield return new WaitForSeconds(PlayEnterCombat());
        }
        this.Running = true;

        while (aiEnable)
        {
            if (target == null || target.GetComponent<Attacker>().IsDead)
            {                
                break;
            }
            //this.trans.LookAt(this.target);
            Vector3 dir = this.target.position - this.trans.position;
            float ang = Vector3.Dot(dir,this.trans.right);
            if (ang > 1)
            {
                this.trans.Rotate(Vector3.up, 1 * (1 + this.MoveSpeed / 100));
            }
            else if (ang < -1)
            {
                this.trans.Rotate(Vector3.up, -1*(1+ this.MoveSpeed/100));
            }
            else
            {
                this.trans.LookAt(this.target);
                this.trans.position += this.trans.forward * this.MoveSpeed / 100;
                if (Vector3.Distance(this.trans.position, this.target.position) < (this.AttackRange+attacker.Level*.5f))
                {
                    this.AiMode = AIMode.Attack;
                    break;
                }
            }
            yield return new WaitForSeconds(0.03f);
        }
    }

    protected IEnumerator AIAttack()
    {
        yield return new WaitForSeconds(0.03f);
        this.Running = false;
        this.Walk = false;
        this.attacker.StartAttack();
        while (aiEnable)
        {
            if (this.target == null)
            {
                this.AiMode = AIMode.Random;
                break;
            }
            this.trans.LookAt(this.target);
            //this.trans.position += this.trans.forward * this.MoveSpeed / 100;
            if (Vector3.Distance(this.trans.position, this.target.position) > (this.AttackRange + attacker.Level * .5f))
            {
                this.attacker.StopAttack();
                
                this.AiMode = AIMode.Chase;
                break;
            }
            yield return new WaitForSeconds(0.03f);
            
        }
    }

    protected IEnumerator AIEscape()
    {
        this.ani.Play("Idle");
        this.Running = true;
        this.Walk = false;
        this.trans.Rotate(Vector3.up, 180);
        while (aiEnable)
        {   
            this.trans.position += this.trans.forward * this.MoveSpeed / 100;
            if (escapeTo < Time.time)
            {
                if (this.target)
                {
                    this.trans.LookAt(this.target);
                    AiMode = AIMode.Chase;
                }
                else
                {
                    AiMode = AIMode.Random;
                }
                break;
            }
            yield return new WaitForSeconds(0.03f);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "wall")
        {
            this.trans.Rotate(Vector3.up, 180);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "wall") {
            this.trans.Rotate(Vector3.up, 180);
        }
    }


    public void SetAttackTarget(Transform target)
    {
        if (this.attacker.IsDead)
            return;
        if (this.AiMode == AIMode.Random)
        {
            this.attacker.SetAttackTarget(target);
            if(target!=null)
                this.AiMode = AIMode.Chase;
        }
    }

    public void OnDamage(int dmg)
    {
        if(dmg < 0)
            this.ShowHUDText(dmg.ToString(), Color.white, 1f);
        else if(dmg>0)
            this.ShowHUDText(dmg.ToString(), Color.green, 1f);
    }
    public void OnHitDown()
    {
        if (!hasHitdown)
        {
            PlayOnHit();
            hasHitdown = true;
            this.ShowHUDText("Shit", Color.red, 1f);
        }
    }

    public void OnSkillCast(Skillbase skill) {
        this.ShowHUDText(skill.Name, skill.Healing > 0 ? Color.green : skill.Damage > 0 ? Color.red : Color.blue,1f);
    }

    public void Escape(int sec)
    {
        this.attacker.StopAttack();
        this.ShowHUDText("No~~~", Color.red, 1f);
        this.escapeTo = Time.time + sec;
        this.AiMode = AIMode.Escape;
        
    }
    public void DoAttack()
    {
        this.ani.SetFloat("atkSpeed", 2f / ((200f / this.attacker.speed.CurValue) - 0.1f));
        PlayAttack();
    }
    public virtual void OnDead()
    {
        GameManager.gameMain.OnMonsterDie(this.attacker);
        PlayDeath();
        this.aiEnable = false;
        this.AiMode = AIMode.NoAI;
        this.attacker.StopAttack();
        this.attacker.enabled = false;
        this.scanEnemy.enabled = false;
        this.ShowHUDText("I'm dead...?", Color.gray, 1f);
        Destroy(this.scanEnemy.gameObject);
        Destroy(this.gameObject, 2f);
    }
    
    public void OnKillDie(Transform target)
    {
        this.scanEnemy.RemoveTarget(target);
        if (this.target == target)
        {
            this.AiMode = AIMode.Random;
            this.SetAttackTarget(null);
            this.scanEnemy.GetNextTarget();
        }
    }
    public void OnTargetLeave()
    {
        
        this.AiMode = AIMode.Random;
        this.scanEnemy.GetNextTarget();
    }

    public void SetHudText(HUDText hd) {
        this.hd = hd;
    }

    public void ShowHUDText(string text,Color color,float duration)
    {
        if(this.hd!=null)
            this.hd.Add(text, color, duration);
        
    }
}


