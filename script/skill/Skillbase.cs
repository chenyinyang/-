using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum AttrType {
    MaxHp,
    MaxHpRatio,
    CurHp,
    MaxMp,
    MaxMpRatio,
    Atk,
    Def,
    Spd,
    Luk,
    Md,
    AtkRatio,
    DefRatio,
    SpdRatio,
    LukRatio,
    MdRatio,
    None
}
public enum Skill {
    Strong,
    Wise,
    Power,
    Protect,
    Agile,
    Bless,
    Touch,
    Smash,
    Heal,
    IceImpact,
    PoisonAttack,
    Count,
    HeroPower
}
public enum SkillType {
    Self,
    Target,
    Both,
    RangeShort,
    RangeMid,
    RangeLong
}

public class buff {
    public AttrType attr;
    public float value;
    public float tickInterval;
    public float nextTick;
    public float curTick;
    
}

public class Skillbase {
    public Attacker owner;
    public string Name { get; private set; }
    public float Cost { get; private set; }
    public float CoolDown { get; private set; }
    public List<buff> selfBuff { get; private set; }
    public List<buff> targetBuff { get; private set; }
    public float Time { get; private set; }
    public float Damage { get; private set; }
    public float Healing { get; private set; }
    public int Level { get; private set; }
    public Skill SkillId { get; private set; }
    public SkillType SkillType { get; private set; }
    public string icon { get; private set; }
    public Color iconColor { get; private set; }
    private float castTime;
    private int exp = 0;

    public Skillbase(Skill sk,Attacker owner) {
        selfBuff = new List<buff>();
        targetBuff = new List<buff>();
        this.SkillId = sk;
        this.Level = 1;
        this.owner = owner;
    }

    public void AddExp(int value) {
        this.exp += value;
        int levelUpExp = this.Level * 50;
        if (exp >= levelUpExp)
        {
            LevelUp();
            exp -= levelUpExp;
        }
    }
    private void LevelUp() {
        
        foreach (var item in selfBuff)
        {
            item.value *= 1.1f;
        }
        foreach (var item in targetBuff)
        {
            item.value *= 1.1f;
        }
        Level += 1;
        Cost *= 1.2f;
        Time *= 1.2f;
        Damage *= 1.5f;
        Healing *= 1.5f;
    }
    
    public bool Cast(Attacker caster)
    {
        if(this.SkillType == SkillType.Target && caster.Target==null)
        {
            return false;
        }
        if (caster.manaPoint.CurValue < this.Cost)
            return false;
        if (this.castTime != 0 && this.castTime + this.CoolDown > UnityEngine.Time.time)
            return false;
        this.castTime = UnityEngine.Time.time;
        caster.manaPoint.AddValue(-(int)this.Cost);
        int range = this.SkillType == SkillType.RangeShort?20:this.SkillType== SkillType.RangeMid?50:100;
        switch (this.SkillType)
        {
            case SkillType.Self:
                caster.OnSkillCast(this, caster);
                break;
            case SkillType.Target:
                caster.Target.OnSkillCast(this, caster);
                break;
            case SkillType.Both:
                caster.Target.OnSkillCast(this, caster);
                caster.OnSkillCast(this, caster);
                break;

            case SkillType.RangeShort:
            case SkillType.RangeMid:
            case SkillType.RangeLong:
                ScanEnemy se = caster.GetComponent<ScanEnemy>();
                se.Enemies.ForEach((trans) => {
                    trans.GetComponent<Attacker>().OnSkillCast(this, caster);
                });
                break;
            default:
                break;
        }

        AddExp( Random.Range(5, 15));
       
        return true;
    }
    public void DoTick() {

    }
    public static Skillbase CreateSKill(Skill skill,Attacker owner)
    {
        Skillbase sk = new Skillbase(skill, owner);
        switch (skill)
        {
            case Skill.Strong:
                sk.Name = skill.ToString();
                sk.Cost = 30;                
                sk.selfBuff.Add(new buff() { attr = AttrType.MaxHp, value = 100f });
                sk.selfBuff.Add(new buff() { attr = AttrType.MaxHpRatio, value = 0.2f });
                sk.Time = 30f;
                sk.CoolDown = 45f;
                sk.SkillType = SkillType.Self;
                sk.icon = "32px/Elements_Life";
                sk.iconColor = Color.green;
                break;
            case Skill.Wise:
                sk.Name = skill.ToString();
                sk.Cost = 30;

                sk.selfBuff.Add(new buff() { attr = AttrType.MaxMp, value = 100f });
                sk.selfBuff.Add(new buff() { attr = AttrType.MaxMpRatio, value = 0.2f });
                sk.Time = 30f;
                sk.CoolDown = 45f;
                sk.SkillType = SkillType.Self;

                sk.icon = "32px/Weapons_Staff";
                sk.iconColor = Color.blue;
                break;
            case Skill.Power:
                sk.Name = skill.ToString();
                sk.Cost = 25;                
                sk.selfBuff.Add(new buff() { attr = AttrType.AtkRatio, value = 0.25f });
                sk.Time = 15f;
                sk.CoolDown = 30f;
                sk.SkillType = SkillType.Self;
                sk.icon = "32px/Weapons_Sword";
                sk.iconColor = Color.white;
                break;
            case Skill.Protect:
                sk.Name = skill.ToString();
                sk.Cost = 25;
                sk.selfBuff.Add(new buff() { attr = AttrType.Def, value = 10f });
                sk.Time = 15f;
                sk.CoolDown = 30f;
                sk.SkillType = SkillType.Self;
                sk.icon = "32px/Equipment_Shield";
                sk.iconColor = Color.white;
                break;
            case Skill.Agile:
                sk.Name = skill.ToString();
                sk.Cost = 25;
                sk.selfBuff.Add(new buff() { attr = AttrType.SpdRatio, value = 0.1f });
                sk.Time = 15f;
                sk.CoolDown = 30f;
                sk.SkillType = SkillType.Self;
                sk.icon = "32px/Equipment_Boots";
                sk.iconColor = Color.blue;
                break;
            case Skill.Bless:
                sk.Name = skill.ToString();
                sk.Cost = 25;                
                sk.selfBuff.Add(new buff() { attr = AttrType.LukRatio, value = 0.2f });
                sk.Time = 15f;
                sk.CoolDown = 30f;
                sk.SkillType = SkillType.Self;
                sk.icon = "32px/Rewards_Star";
                sk.iconColor = Color.yellow;
                break;
            case Skill.Touch:
                sk.Name = skill.ToString();
                sk.Cost = 25;
                sk.selfBuff.Add(new buff() { attr = AttrType.MdRatio, value = 0.2f });
                sk.Time = 15f;
                sk.CoolDown = 30f;
                sk.SkillType = SkillType.Self;
                sk.icon = "32px/Elements_Ground";
                sk.iconColor = Color.yellow;
                break;
            case Skill.Smash:
                sk.Name = skill.ToString();
                sk.Cost = 15;
                sk.Damage = 350;
                sk.targetBuff.Add(new buff() { attr = AttrType.Def, value = -50f });
                sk.selfBuff.Add(new buff() { attr = AttrType.Atk, value = 50f });
                sk.Time = 10f;
                sk.CoolDown = 20f;
                sk.SkillType = SkillType.Both;
                sk.icon = "32px/Tools_Hammer";
                sk.iconColor = Color.red;
                break;
            case Skill.IceImpact:
                sk.Name = skill.ToString();
                sk.Cost = 15;
                sk.Damage = 100;
                sk.targetBuff.Add(new buff() { attr = AttrType.SpdRatio, value = -0.5f });
                sk.Time = 15f;
                sk.CoolDown = 30f;
                sk.SkillType = SkillType.Target;
                sk.icon = "32px/Elements_Ice";
                sk.iconColor = Color.blue;
                break;
            case Skill.Heal:
                sk.Name = skill.ToString();
                sk.Cost = 50;
                sk.Healing = 250;
                sk.Time = 15f;
                sk.CoolDown = 30f;
                sk.SkillType = SkillType.Self;
                sk.icon = "32px/Elements_Water";
                sk.iconColor = Color.blue;
                break;
            case Skill.HeroPower:
                sk.Name = skill.ToString();
                sk.Cost = 0;
                sk.Healing = 1500;
                sk.selfBuff.Add(new buff() { attr = AttrType.MaxHp, value = 500});
                sk.selfBuff.Add(new buff() { attr = AttrType.MaxMp, value = 200});
                sk.selfBuff.Add(new buff() { attr = AttrType.Atk,   value = 100});
                sk.selfBuff.Add(new buff() { attr = AttrType.Def,   value = 100 });
                sk.selfBuff.Add(new buff() { attr = AttrType.Luk,   value = 30 });
                sk.selfBuff.Add(new buff() { attr = AttrType.Md,    value = 100 });
                sk.selfBuff.Add(new buff() { attr = AttrType.Spd, value = 50 });
                sk.selfBuff.Add(new buff() { attr = AttrType.MaxHpRatio, value = .2f});
                sk.selfBuff.Add(new buff() { attr = AttrType.MaxMpRatio, value = .2f });
                sk.selfBuff.Add(new buff() { attr = AttrType.AtkRatio, value = .5f});
                sk.selfBuff.Add(new buff() { attr = AttrType.DefRatio, value = .5f});
                sk.selfBuff.Add(new buff() { attr = AttrType.SpdRatio, value = .2f });
                sk.Time = 15f;
                sk.CoolDown = 30f;
                sk.SkillType = SkillType.Self;
                sk.icon = "32px/Modifiers_LevelUp";
                sk.iconColor = Color.yellow;
                break;
            case Skill.PoisonAttack:
                sk.Name = skill.ToString();
                sk.Cost = 0;
                sk.Damage = 150;
                sk.targetBuff.Add(new buff() { attr = AttrType.Def, value = -1f, tickInterval=3f });
                sk.targetBuff.Add(new buff() { attr = AttrType.CurHp, value = -50, tickInterval =1f });                
                sk.targetBuff.Add(new buff() { attr = AttrType.Atk, value = -2,  tickInterval=3f });                
                sk.targetBuff.Add(new buff() { attr = AttrType.Spd, value = -2, tickInterval=3f });
                
                sk.Time = 15f;
                sk.CoolDown = 30f;
                sk.SkillType = SkillType.Target;
                sk.icon = "32px/Elements_Death";
                sk.iconColor = Color.green;
                break;
            default:
                break;
        }
        return sk;
    }

}
