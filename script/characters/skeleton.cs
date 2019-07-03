using UnityEngine;
using System.Collections;

public class skeleton : ChracterBase {


    public override float PlayEnterCombat()
    {
        this.ani.Play("Skill");
        return 2f;
    }
    public override float PlayOnHit()
    {
        this.ani.Play("Knockback");
        return 2f;
    }
    public override void PlayAttack()
    {

        this.ani.Play("Attack");
    }
    public override void PlayDeath()
    {

        this.ani.Play("Death");
    }
    public override void OnDead()
    {
        GameManager.gameMain.OnMonsterDie(this.attacker);
        PlayDeath();
        this.AiMode = AIMode.NoAI;
        this.aiEnable = false;
        this.attacker.StopAttack();
        //this.attacker.enabled = false;
        this.ShowHUDText("I'll rebirth...", Color.gray, 1f);
        Invoke("Rebirth", 5f);
    }
    private void Rebirth() {
        this.attacker.Rebirth();
        this.ani.SetTrigger("rebirth");
        this.aiEnable = true;
        this.AiMode = AIMode.Random;        
        //this.attacker.enabled = true;
        this.ShowHUDText("Revenge...!", Color.gray, 1f);
    }

    protected override IEnumerator AIRandomMove()
    {
        this.Running = false;
        this.Walk = true;
        while (aiEnable)
        {
            int rnd = Random.Range(0, 100);
            if (rnd <= 10)
            {
                for (int i = 1; i < 10 + rnd * 5; i++)
                {
                    this.trans.Rotate(Vector3.up, 1);
                    yield return new WaitForSeconds(0.03f);
                }
            }
            else if (rnd <= 20)
            {

                for (int i = 1; i < 10 + (rnd - 10) * 5; i++)
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
            else if (rnd <= 60)
            {
                this.trans.LookAt(GameManager.gameMain.GetBoss());
            }
            for (int i = 0; i < 100; i++)
            {
                this.trans.position += this.trans.forward * this.MoveSpeed / 100;
                yield return new WaitForSeconds(0.05f);
            }

        }
    }
}
