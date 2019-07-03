using UnityEngine;
using System.Collections;

public class HeroAttacker : Attacker
{

    
    public override void InitialStatus()
    {
        
        this.Level = 1;
        this.SoulPoint = 400 + Random.Range(0, 50);
        talentPoint = this.SoulPoint;
        MonsterName = this.name;
        this.expPoint = new CharacterAttri(100,99999);
        this.expPoint.AddValue(-this.expPoint.MaxValue);
        int rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.healPoint = new CharacterAttri(1500 + rnd * 50,99999);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.manaPoint = new CharacterAttri(500 + rnd * 20,99999);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.attack = new CharacterAttri(150 + rnd,9999);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.deffence = new CharacterAttri(150 + rnd,9999);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.speed = new CharacterAttri(100 + rnd,500,50);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.luck = new CharacterAttri(15 + (int)Mathf.Ceil(rnd * 0.1f),50);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.mind = new CharacterAttri(80 + (int)Mathf.Ceil(rnd * 0.1f),100);
        talentPoint -= rnd;
    }
    private void ResetStatus() {
        this.Level = 1;
        this.SoulPoint = 400 + Random.Range(0, 50) +(int)(this.SoulPoint*0.02f*general);
        this.soulBorrow = 0;
        talentPoint = this.SoulPoint;
        MonsterName = this.name.Split('_')[0]+"_"+ GetGenrealText(general);
        this.gameObject.name = MonsterName;
        this.transform.localScale = initialSCale;
        this.expPoint.ResetTo(100);
        this.expPoint.AddValue(-this.expPoint.MaxValue);
        int rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.healPoint.ResetTo(1500 + rnd * 100);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.manaPoint.ResetTo(500 + rnd * 50);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.attack.ResetTo(150 + rnd);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.deffence.ResetTo(150 + rnd);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.speed.ResetTo(100 + rnd);
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.luck.ResetTo(15 + (int)Mathf.Ceil(rnd * 0.1f));
        talentPoint -= rnd;
        rnd = Random.Range(0, Mathf.Min(50, talentPoint));
        this.mind.ResetTo(80 + (int)Mathf.Ceil(rnd * 0.1f));
        talentPoint -= rnd;
    }

    protected override void InitialSkill()
    {
        base.InitialSkill();
        if (skills.Find((sk) => { return sk.SkillId == Skill.IceImpact; }) == null)
        {
            this.skills.Add(Skillbase.CreateSKill(Skill.IceImpact,this));
        }

        this.passiveSkill.Add(Skillbase.CreateSKill(Skill.HeroPower,this));        
        this.chara.ShowHUDText("I Learn " + Skill.HeroPower.ToString(), Color.blue, 1f);
    }
    public override void OnStartAttack()
    {
        if (this.Target.transform == GameManager.gameMain.GetBoss() && this.healPoint.CurValue < this.healPoint.MaxValue / 2)
        {
            Skillbase heroPower = this.passiveSkill.Find((sk) => { return sk.SkillId == Skill.HeroPower; });
            if (heroPower != null)
                heroPower.Cast(this);
        }
    }
    protected override void beforeAttack()
    {
        if (this.Target.transform == GameManager.gameMain.GetBoss() && this.healPoint.CurValue<this.healPoint.MaxValue/2)
        {
            Skillbase heroPower = this.passiveSkill.Find((sk) => { return sk.SkillId == Skill.HeroPower; });
            if (heroPower != null )
                heroPower.Cast(this);
        }
    }
    public override void GetSoul(int getSoul)
    {        
        base.GetSoul(getSoul*2);
    }
    public override void EnemyKill(Attacker enemy)
    {
        base.EnemyKill(enemy);
        if(enemy.transform == GameManager.gameMain.GetBoss())
        {
            this.chara.ShowHUDText("Mission Complete...!", Color.red, 2f);
            GameManager.gameMain.HeroKillBoss(this);
            this.rebirthTimes = 0;
            this.chara.OnDead();
            general += 1;
            this.ResetStatus();

            this.skills.Clear();
            this.passiveSkill.Clear();
            InitialSkill();
        }
    }
}
