using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Attacker : MonoBehaviour {
    private object locker = new object();
    public string MonsterName;
    public int Level { get; protected set; }
    public int Kill { get; protected set; }
    public int HP = 0;
    public bool IsDead { get { return healPoint!=null?healPoint.CurValue <= 0:true; } }
    public CharacterAttri healPoint;
    public CharacterAttri manaPoint;
    public CharacterAttri expPoint;
    public CharacterAttri attack;
    public CharacterAttri deffence;
    public CharacterAttri speed;
    public CharacterAttri luck;
    public CharacterAttri mind;
    protected ChracterBase chara;
    public Attacker Target { get; protected set; }
    protected Coroutine attackAct;
    protected bool attacking = false;
    public List<Skillbase> skills;
    public List<Skillbase> passiveSkill;
    protected Dictionary<Skillbase, float> selfBuffTime;
    protected Dictionary<Skillbase, float> enemyBuffTime;
    public int SoulPoint { get; protected set; }
    protected int talentPoint;

    protected float attackCooldown;

    protected int soulBorrow;

    private float recover5;
    private float running1;
    private float nextEscapeTime;
    protected Vector3 initialSCale;
    private void Awake()
    {

        InitialStatus();
        this.chara = this.GetComponent<ChracterBase>();

        this.skills = new List<Skillbase>();
        this.passiveSkill = new List<Skillbase>();
        this.selfBuffTime = new Dictionary<Skillbase, float>();
        this.enemyBuffTime = new Dictionary<Skillbase, float>();
        initialSCale = this.transform.localScale;
    }
    public virtual void InitialStatus() {
        this.Level = 1;
        this.SoulPoint = 100 + Random.Range(0, 50);
        talentPoint = this.SoulPoint;
        MonsterName = this.name;
        this.expPoint = new CharacterAttri(100,99999);
        this.expPoint.AddValue(-this.expPoint.MaxValue);
        int rnd = Random.Range(0, Mathf.Min(20, talentPoint));
        this.healPoint = new CharacterAttri(2000 + rnd * 50,99999);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(20, talentPoint));
        this.manaPoint = new CharacterAttri(500 + rnd * 20,99999);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(20, talentPoint));
        this.attack = new CharacterAttri(200 + rnd,9999);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(20, talentPoint));
        this.deffence = new CharacterAttri(150 + rnd,9999);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(20, talentPoint));
        this.speed = new CharacterAttri(100 + rnd,500,50);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(20, talentPoint));
        this.luck = new CharacterAttri(10 + (int)Mathf.Ceil(rnd * 0.1f),50);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(20, talentPoint));
        this.mind = new CharacterAttri(75 + (int)Mathf.Ceil(rnd * 0.1f),100);
        talentPoint -= rnd;
    }
    // Use this for initialization
    void Start () {
        InitialSkill();
        UpgradeInitialSkill();
        GameManager.gameMain.OnMonsterCreate(this);
    }
    protected virtual void InitialSkill() {
        int skillCount = (int)Skill.Count;

        int newSkillCount = Mathf.Min(talentPoint / 20,4);
        for (int i = 0; i < newSkillCount; i++)
        {
            int sk = Random.Range(0, skillCount);
            Skillbase skB = skills.Find((s) => { return (int)s.SkillId == sk; });
            if (skB != null)
            {
                skB.AddExp(20);
                continue;
            }
            else
            {
                this.skills.Add(Skillbase.CreateSKill((Skill)sk,this));
            }
            talentPoint -= 20;
            this.chara.ShowHUDText("I Learn " + ((Skill)sk).ToString(), Color.white, 1f);
            //Debug.Log(this.MonsterName + " learned skill : " + ((Skill)sk).ToString());
        }
    }
    protected void UpgradeInitialSkill()
    {
        while (talentPoint >= 25)
        {
            int sk = Random.Range(0, skills.Count+passiveSkill.Count);
            if (sk >= skills.Count)
                passiveSkill[sk - skills.Count].AddExp(50);
            else
                skills[sk].AddExp(50);
            talentPoint -= 25;
        }
    }
	// Update is called once per frame
	void Update () {
        if (this.IsDead)
            return;
        float now = Time.time;
        List<Skillbase> removeBuff = new List<Skillbase>();
        foreach (var item in selfBuffTime)
        {
            if (item.Value < now)
            {
                removeBuff.Add(item.Key);
            }
            if (item.Key.selfBuff.Count > 0)
            {
                item.Key.selfBuff.ForEach((b) =>
                {
                    if (b.tickInterval>0 && b.nextTick < Time.time) {
                        RemoveAttrBuff(b.attr, b.value);
                        b.curTick++;
                        AddAttrBuff(b.attr, b.value * b.curTick);
                        b.nextTick = Time.time+b.tickInterval;
                    }
                });
            }
        }
        foreach (var item in removeBuff)
        {
            selfBuffTime.Remove(item);
            foreach (var buf in item.selfBuff)
            {
                RemoveAttrBuff(buf.attr, buf.value*buf.curTick);
                buf.curTick = 0;
            }
        }
        removeBuff.Clear();
        foreach (var item in enemyBuffTime)
        {
            if (item.Value < now)
            {
                removeBuff.Add(item.Key);
            }
            if (item.Key.targetBuff.Count > 0)
            {
                item.Key.targetBuff.ForEach((b) =>
                {
                    if (b.tickInterval > 0 && b.nextTick < Time.time)
                    {
                        RemoveAttrBuff(b.attr, b.value);
                        b.curTick++;
                        AddAttrBuff(b.attr, b.value * b.curTick);
                        b.nextTick = Time.time + b.tickInterval;
                    }
                });

                if (this.healPoint.CurValue <= 0)
                {
                    this.healPoint.AddValue(1);
                }
            }
        }
        foreach (var item in removeBuff)
        {
            enemyBuffTime.Remove(item);
            foreach (var buf in item.targetBuff)
            {
                RemoveAttrBuff(buf.attr, buf.value);
                buf.curTick = 0;
            }
        }
        if (this.chara.Running)
        {
            if (running1 <= Time.time)
            {
                this.manaPoint.AddValue(-10);
                this.speed.Exp += 1;
                this.manaPoint.Exp += 1;
                running1 = Time.time + 1;
            }
        }
        if (recover5 <= Time.time && this.Target==null)
        {
            this.manaPoint.AddValue(this.healPoint.MaxValue/100);
            this.healPoint.AddValue(this.manaPoint.MaxValue/100);
            recover5 = Time.time + 5f;
        }
    }

    public void SetAttackTarget(Transform target) {
        if (target == null)
            this.Target = null;
        else
            this.Target = target.GetComponent<Attacker>();
    }

    public void StartAttack() {
        attacking = true;
        if (attackAct == null)
        {
            OnStartAttack();
            attackAct = StartCoroutine(Attack());
        }
    }
    public virtual void OnStartAttack() { }

    public void StopAttack()
    {
        attacking = false;
        if(attackAct!=null)
            StopCoroutine(attackAct);
        attackAct = null;
    }
    public IEnumerator Attack() {

        while (attacking)
        {
            if (attackCooldown > Time.time) {
                yield return new WaitForSeconds(0.03f);
                continue;
            }
            attackCooldown = Time.time + 200f / this.speed.MaxValue;
            beforeAttack();
            this.chara.DoAttack();

            bool doAttack = true;
            if (this.skills.Count>0 && Random.Range(0, 100) < 20)
            {
                int castSkId = Random.Range(0, this.skills.Count);
                Skillbase castSkill = this.skills[castSkId];
                if (castSkill.Cast(this))
                {
                    this.manaPoint.Exp += 1;
                    this.chara.OnSkillCast(castSkill);
                    doAttack = false;
                }
            }
            if (doAttack)
            {
                int floatDmg = Random.Range(this.luck.MaxValue / -5, this.luck.MaxValue / 5);
                int atk = this.attack.MaxValue +floatDmg;
                this.attack.Exp += (floatDmg>0)?1:0;
                this.luck.Exp += (floatDmg == this.luck.MaxValue / 5 ? -1 : floatDmg == (this.luck.MaxValue / 5 - 1) ? 1 : 0);
                this.Target.OnAttack(this, this.attack.MaxValue, this.luck.MaxValue);
            }
            yield return new WaitForSeconds(200f / this.speed.MaxValue);
        }
    }
    protected virtual void beforeAttack() { }
    public void OnAttack(Attacker attacker,int atk, int luck) {
        if (this.IsDead)
            return;
        int rollTimes = 0;
        while (Random.Range(0, 100) < luck)
        {   
            this.luck.Exp -= 1;
            atk = (int)(atk*1.2f);
            rollTimes++;
            if (rollTimes >= 5)
                break;
        }
        rollTimes = 0;
        int def = (this.deffence.MaxValue + Random.Range(this.luck.MaxValue / -5, this.luck.MaxValue / 5));
        while (Random.Range(0, 100) < this.luck.MaxValue)        
        {
            this.deffence.Exp += 1;
            this.luck.Exp += 1;
            def = (int)(def * 1.2f);
            rollTimes++;
            if (rollTimes >= 5)
                break;
        }
        
        int dmg = atk - def ;
        if (dmg <= 0)
            dmg = 1;
        this.healPoint.AddValue(-1 * dmg);
        this.healPoint.Exp += 1;
        
        this.chara.OnDamage(-dmg);
        if (this.healPoint.CurValue < this.healPoint.MaxValue / 2)
        {
            this.chara.OnHitDown();
        }
        if (((float)this.healPoint.CurValue / this.healPoint.MaxValue *100) < (100-this.mind.MaxValue))
        {
            if (this.chara.AiMode != AIMode.Escape && nextEscapeTime<Time.time && Random.Range(0, 100) > this.mind.MaxValue)
            {
                this.chara.Escape(Random.Range(0, 100-this.mind.MaxValue)/2);
                this.nextEscapeTime = Time.time + this.mind.MaxValue;
            }
            else
            {
                this.mind.Exp += 1;
            }
        }
        HP = this.healPoint.CurValue;
        if (this.healPoint.CurValue <= 0)
        {        
            attacker.EnemyKill(this);
            this.chara.OnDead();
        }
    }

    public void OnSkillCast(Skillbase skill,Attacker caster)
    {
        lock (locker)
        {
            if (caster == this)
            {
                if (skill.selfBuff.Count > 0)
                {

                    if (!this.selfBuffTime.ContainsKey(skill))
                    {
                        foreach (var item in skill.selfBuff)
                        {
                            this.AddAttrBuff(item.attr, item.value);
                            item.nextTick = Time.time + item.tickInterval;
                            item.curTick = 0;
                        }
                    }
                    this.selfBuffTime[skill] = Time.time + skill.Time;
                }
                if (skill.Healing > 0)
                {
                    this.healPoint.AddValue((int)skill.Healing);
                    this.chara.OnDamage((int)skill.Healing);
                }
            }
            else
            {
                if (skill.targetBuff.Count > 0)
                {

                    if (!this.enemyBuffTime.ContainsKey(skill))
                    {
                        foreach (var item in skill.targetBuff)
                        {
                            this.AddAttrBuff(item.attr, item.value);
                            item.nextTick = Time.time + item.tickInterval;
                            item.curTick = 0;
                        }
                    }
                    this.enemyBuffTime[skill] = Time.time + skill.Time;
                }
                if (skill.Damage > 0)
                {
                    this.healPoint.AddValue(-(int)skill.Damage);
                    this.chara.OnDamage(-(int)skill.Damage);
                    if (this.healPoint.CurValue <= 0)
                    {
                        caster.EnemyKill(this);
                        this.chara.OnDead();
                    }
                }
            }
        }
    }

    private void AddAttrBuff(AttrType type, float value)
    {
        switch (type)
        {
            case AttrType.MaxHp:
                this.healPoint.AddBuff(value, CharacterAttri.buffType.abs);
                break;
            case AttrType.MaxHpRatio:
                this.healPoint.AddBuff(value, CharacterAttri.buffType.ratio);
                break;
            case AttrType.CurHp:
                this.healPoint.AddValue((int)value);
                break;
            case AttrType.MaxMp:
                this.manaPoint.AddBuff(value, CharacterAttri.buffType.abs);
                break;
            case AttrType.MaxMpRatio:
                this.manaPoint.AddBuff(value, CharacterAttri.buffType.ratio);
                break;
            case AttrType.Atk:
                this.attack.AddBuff(value, CharacterAttri.buffType.abs);
                break;
            case AttrType.AtkRatio:
                this.attack.AddBuff(value, CharacterAttri.buffType.ratio);
                break;
            case AttrType.Def:
                this.deffence.AddBuff(value, CharacterAttri.buffType.abs);
                break;
            case AttrType.DefRatio:
                this.deffence.AddBuff(value, CharacterAttri.buffType.ratio);
                break;
            case AttrType.Spd:
                this.speed.AddBuff(value, CharacterAttri.buffType.abs);
                break;
            case AttrType.SpdRatio:
                this.speed.AddBuff(value, CharacterAttri.buffType.ratio);
                break;
            case AttrType.Luk:
                this.luck.AddBuff(value, CharacterAttri.buffType.abs);
                break;
            case AttrType.LukRatio:
                this.luck.AddBuff(value, CharacterAttri.buffType.ratio);
                break;
            case AttrType.Md:
                this.mind.AddBuff(value, CharacterAttri.buffType.abs);
                break;
            case AttrType.MdRatio:
                this.mind.AddBuff(value, CharacterAttri.buffType.ratio);
                break;
            default:
                break;
        }
    }
    private void RemoveAttrBuff(AttrType type, float value)
    {
        switch (type)
        {
            case AttrType.MaxHp:
                this.healPoint.RemoveBuff(value, CharacterAttri.buffType.abs);
                break;
            case AttrType.MaxHpRatio:
                this.healPoint.RemoveBuff(value, CharacterAttri.buffType.ratio);
                break;
            case AttrType.MaxMp:
                this.manaPoint.RemoveBuff(value, CharacterAttri.buffType.abs);
                break;
            case AttrType.MaxMpRatio:
                this.manaPoint.RemoveBuff(value, CharacterAttri.buffType.ratio);
                break;
            case AttrType.Atk:
                this.attack.RemoveBuff(value, CharacterAttri.buffType.abs);
                break;
            case AttrType.AtkRatio:
                this.attack.RemoveBuff(value, CharacterAttri.buffType.ratio);
                break;
            case AttrType.Def:
                this.deffence.RemoveBuff(value, CharacterAttri.buffType.abs);
                break;
            case AttrType.DefRatio:
                this.deffence.RemoveBuff(value, CharacterAttri.buffType.ratio);
                break;
            case AttrType.Spd:
                this.speed.RemoveBuff(value, CharacterAttri.buffType.abs);
                break;
            case AttrType.SpdRatio:
                this.speed.RemoveBuff(value, CharacterAttri.buffType.ratio);
                break;
            case AttrType.Luk:
                this.luck.RemoveBuff(value, CharacterAttri.buffType.abs);
                break;
            case AttrType.LukRatio:
                this.luck.RemoveBuff(value, CharacterAttri.buffType.ratio);
                break;
            case AttrType.Md:
                this.mind.RemoveBuff(value, CharacterAttri.buffType.abs);
                break;
            case AttrType.MdRatio:
                this.mind.RemoveBuff(value, CharacterAttri.buffType.ratio);
                break;
            default:
                break;
        }
    }

    public virtual void EnemyKill(Attacker enemy) {        
        this.StopAttack();       

        int getSoul = (int)(enemy.SoulPoint*(1-GameManager.SoulRecycle));
        this.GetSoul((int)(getSoul* ((enemy is HeroAttacker || GameManager.gameMain.GetBoss()==this.transform)?1.5f:1)));
        this.Kill += 1;
    }
    public virtual void GetSoul(int getSoul) {
        if (soulBorrow > 0)
        {
            int returnSoul = Mathf.Min(soulBorrow,getSoul);
            soulBorrow -= returnSoul;
            getSoul -= returnSoul;            
        }
        if (getSoul <= 0)
            return;
        this.SoulPoint += getSoul;
        this.talentPoint += getSoul;
        while (getSoul > 0)
        {
            int nextExpNeed = this.expPoint.MaxValue - this.expPoint.CurValue;
            if (getSoul > nextExpNeed)
            {
                this.expPoint.AddValue(nextExpNeed);
                getSoul -= nextExpNeed;
                this.LevelUp();
            }
            else
            {
                this.expPoint.AddValue(getSoul);
                getSoul = 0;
            }
        }
    }
    
    private void LevelUp() {
        int soulBonus = Random.Range(30, 50);
        this.SoulPoint += soulBonus;
        this.talentPoint += soulBonus;
        
        this.expPoint.AddValue(-this.expPoint.MaxValue);
        this.Level += 1;
        this.transform.localScale = initialSCale*(1 + 0.03f*Level);
        //this.chara.AttackRange += 0.5f;
        this.expPoint.AddMaxValue((int)(this.expPoint.MaxValue *Random.Range(5,20)/100f));
        Dictionary<CharacterAttri, float> attrs = new Dictionary<CharacterAttri, float>();
        attrs.Add(this.healPoint, 10);
        attrs.Add(this.manaPoint, 10);
        attrs.Add(this.attack, 0.3f);
        attrs.Add(this.deffence, 0.3f);
        if (speed.MaxValue <= 300)
        {
            attrs.Add(this.speed, 0.2f);
        }
        if (luck.MaxValue <= 50)
            attrs.Add(this.luck, 0.05f);
        if (mind.MaxValue <= 100)
            attrs.Add(this.mind, 0.05f);
        int attrCount = attrs.Count;
        for (int i = 0; i < attrCount; i++)
        {
            CharacterAttri attr = attrs.Keys.ToArray()[Random.Range(0, attrs.Count)];            
            int upgradeValue = Random.Range(0, talentPoint/5);            
            attr.AddMaxValue( (int)Mathf.Ceil(upgradeValue * attrs[attr]));
            talentPoint -= upgradeValue;
            attrs.Remove(attr);
        }
        this.healPoint.AddValue(this.healPoint.MaxValue/2);
        this.manaPoint.AddValue(this.healPoint.MaxValue/2); 
        int skillCount = this.skills.Count;

        for (int j = 0; j < skillCount; j++)
        {
            Skillbase sk = skills[Random.Range(0, this.skills.Count)];
            int skillExp = Random.Range(0, talentPoint);
            sk.AddExp(skillExp);
            talentPoint -= skillExp;
        }
        int newSkillCount = talentPoint / 25;
        for (int i=0;i< newSkillCount;i++)
        {
            int sk = Random.Range(0, skillCount);
            if (skills.Exists((s) => { return (int)s.SkillId == sk; }))
            {
                skills[sk].AddExp(50);
                talentPoint -= 25;
                continue;
            }
            this.skills.Add(Skillbase.CreateSKill((Skill)sk,this));
            talentPoint -= 25;
            this.chara.ShowHUDText("I Learn " + ((Skill)sk).ToString(), Color.white, 1f);
            //Debug.Log(this.MonsterName + " learned skill : " + ((Skill)sk).ToString());
        }
        if (Level >= 30)
            Revolation();
    }
    public int rebirthTimes = 0;
    public void Rebirth() {
        rebirthTimes++;
        float soulCast = this.SoulPoint *(1-GameManager.SoulRecycle);
        this.GetSoul((int)soulCast);
        soulBorrow += (int)soulCast;
        StartCoroutine(RebirthRecover());
    }
    IEnumerator RebirthRecover() {
        for (int i = 0; i < 5; i++)
        {
            this.healPoint.AddValue(this.healPoint.MaxValue/5);
            this.manaPoint.AddValue(this.manaPoint.MaxValue/5);
            yield return new WaitForSeconds(1f);
        }
    }

    protected int general = 0;
    protected void Revolation() {
        general++;
        this.Level = 1;
        int genSoul = 100 * general + Random.Range(0, 50) + (int)(this.SoulPoint * 0.02f * general);
        GameManager.gameMain.RecycleSoul(this.SoulPoint - genSoul);
        this.SoulPoint = genSoul;
        this.soulBorrow = 0;
        talentPoint += SoulPoint;
        MonsterName = this.name.Split('_')[0] + "_" + GetGenrealText(general);
        
        this.transform.localScale = initialSCale;
        this.expPoint.ResetTo(100);
        this.expPoint.AddValue(-this.expPoint.MaxValue);
        int rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.healPoint.ResetTo(this.healPoint.MaxValue- this.healPoint.BuffValue +rnd*50);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.manaPoint.ResetTo(this.manaPoint.MaxValue - this.manaPoint.BuffValue+ rnd * 50);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.attack.ResetTo(this.attack.MaxValue - this.attack.BuffValue + rnd);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.deffence.ResetTo(this.deffence.MaxValue - this.deffence.BuffValue + rnd);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.speed.ResetTo(this.speed.MaxValue - this.speed.BuffValue + rnd);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.luck.ResetTo(this.luck.MaxValue - this.luck.BuffValue + (int)Mathf.Ceil(rnd * 0.1f));
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.mind.ResetTo(this.mind.MaxValue - this.mind.BuffValue + (int)Mathf.Ceil(rnd * 0.1f));
        talentPoint -= rnd;
        evoType e = GetEvoType();
        GetEvoBonus(e);
        if (e != evoType.Normal)
        {
            this.MonsterName = e.ToString() + " " + MonsterName;
        }
        this.gameObject.name = MonsterName;
    }
    protected string GetGenrealText(int general)
    {
        switch (general)
        {
            case 0: return "";
            case 1:
                return "I";
            case 2:
                return "II";
            case 3:
                return "IV";
            case 4:
                return "V";
            case 5:
                return "I";
            case 10:
                return "X";
            default:
                int dec = general / 10;
                string ret = "";
                for (int i = 0; i < dec; i++)
                {
                    ret += "X";
                }
                int d = general % 10;
                for (int i = 0; i < d / 5; i++)
                {
                    ret += "V";
                }
                ret += GetGenrealText(d % 5);
                return ret;
        }
    }

    enum evoType {
        Strong,
        Wise,
        Power,
        Protect,
        Speed,
        Lucky,
        Rare,
        Epic,
        Boss,
        Normal,
    }
    evoType GetEvoType() {
        int rnd = Random.Range(0, 100);
        if (rnd < 1)
            return evoType.Boss;//1%
        if (rnd < 4)
            return evoType.Epic;//3%
        if (rnd < 10)
            return evoType.Rare;//6%
        if (rnd < 20)
            return evoType.Power;//10%
        if (rnd < 30)
            return evoType.Protect;//10%
        if (rnd < 40)
            return evoType.Speed;//10%
        if (rnd < 50)
            return evoType.Lucky;//10%
        if (rnd < 60)
            return evoType.Strong;//10%
        if (rnd < 70)
            return evoType.Wise;//10%
        return evoType.Normal;//30%
    }
    void GetEvoBonus(evoType e) {
        switch (e)
        {
            case evoType.Strong:
                this.healPoint.AddMaxValue(500);
                break;
            case evoType.Wise:
                this.manaPoint.AddMaxValue(500);
                break;
            case evoType.Power:
                this.attack.AddMaxValue(30);
                break;
            case evoType.Protect:
                this.deffence.AddMaxValue(30);
                break;
            case evoType.Speed:
                this.speed.AddMaxValue(20);
                break;
            case evoType.Lucky:
                this.luck.AddMaxValue(10);
                break;
            case evoType.Rare:
                this.healPoint.AddMaxValue(300);
                this.manaPoint.AddMaxValue(300);
                this.attack.AddMaxValue(20);
                this.deffence.AddMaxValue(20);
                this.speed.AddMaxValue(10);
                this.luck.AddMaxValue(5);
                break;
            case evoType.Epic:
                this.healPoint.AddMaxValue(500);
                this.manaPoint.AddMaxValue(500);
                this.attack.AddMaxValue(30);
                this.deffence.AddMaxValue(30);
                this.speed.AddMaxValue(20);
                this.luck.AddMaxValue(10);
                break;
            case evoType.Boss:
                this.healPoint.AddMaxValue(1000);
                this.manaPoint.AddMaxValue(1000);
                this.attack.AddMaxValue(50);
                this.deffence.AddMaxValue(50);
                this.speed.AddMaxValue(30);
                this.luck.AddMaxValue(20);
                break;
            case evoType.Normal:
                this.healPoint.AddMaxValue(100);
                this.manaPoint.AddMaxValue(100);
                this.attack.AddMaxValue(10);
                this.deffence.AddMaxValue(10);
                this.speed.AddMaxValue(2);
                break;
            default:
                break;
        }
    }
}
