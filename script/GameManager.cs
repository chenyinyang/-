using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Utility;
using System.Text;

public class GameManager : MonoBehaviour {
    public class HeroInfo {
        public string name;
        public int level;
        public int killed;
        public int rebirth;
        public int hp;
        public int mp;
        public int atk;
        public int def;
        public int speed;
        public int luck;
        public int mind;
        public int soul;
        public override string ToString()
        {
            return string.Format("{0} Kill Boss!\nSoul:{11}\nLevel:{1}\nKilled:{2}\nRebirth:{3}\nHp:{4}\nMp:{5}\nAtk:{6}\nDef:{7}\nSpeed:{8}\nLuck:{9}\nMind:{10}\n", name, level, killed, rebirth, hp, mp, atk, def, speed, luck, mind,soul);
        }
    }
    public List<HeroInfo> heros;
    public const float SoulRecycle = 0.9f;
    public const int SpawnPoinCount = 10;
    public const int SpawnMonsterCount = 50;
    public static GameManager gameMain;
    private int SoulLevel = 1;
    private double _soulPool;
    public double SoulPool
    {
        get { return _soulPool; }
        private set
        {
            _soulPool = value;
            if (_soulPool > SoulPoolNext) {
                SoulUpgrade();
            }
        }
    }
    public float SoulPoolNext { get; private set; }
    List<Attacker> monsters;
    Attacker hero;
    public List<GameObject> monsterPrefabs;
    public GameObject heroPrefab;
    private List<Vector3> spawnPoint;
    private List<StateUI> statusUI;
    private StateUI tarState;
    private StateUI heroState;
    private Coroutine updateCor;
    private float updateUITimeLock = 0f;
    private object locker = new object();

    public int deadCount { get; private set; }
    public int popCount { get; private set; }
    public int curdCount { get { return monsters.Count; }}
    public int heroDie { get; private set; }


    List<SpaceEvent> currentEvent;
    private void Awake()
    {
        this._soulPool = 20000;
        this.SoulPoolNext = 20000;
        gameMain = this;
        monsters = new List<Attacker>();
        spawnPoint = new List<Vector3>();
        heros = new List<HeroInfo>();
        for (int i = 0; i < SpawnPoinCount; i++)
        {
            spawnPoint.Add(new Vector3(Random.Range(250,550),0,Random.Range(250,550)));
        }
        statusUI = new List<StateUI>();
        currentEvent = new List<SpaceEvent>();
    }

    public void RegStateUI(StateUI ui) {
        GameObject button = ui.gameObject;
        UIEventListener.Get(button).onClick = StatusOnClick;
        if (ui.name == "targetStatus")
        {
            tarState = ui;
            return;
        }if (ui.name == "heroStatus")
        {
            heroState = ui;
            return;
        }
        this.statusUI.Add(ui);
        this.statusUI.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
        
    }
    // Use this for initialization
    void Start () {
        for (int i = 0;i< SpawnMonsterCount; i++)
        {
            GameObject m = this.CreateMonster(monsterPrefabs[0]);
        }
        this.CreateMonster(heroPrefab);
        StartCoroutine(SpaceEffect());
    }
    private GameObject CreateMonster(GameObject template) {
        Vector3 spPoint = spawnPoint[Random.Range(0, SpawnPoinCount)] + new Vector3(Random.Range(-50, 50), 0, Random.Range(-50, 50));
        GameObject monster = (GameObject)GameObject.Instantiate(template, spPoint, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up));
        monster.name = "";
        Attacker atk = monster.GetComponent<Attacker>();
       
        int qualitySeed = Random.Range(0, 100);
        if (qualitySeed <= 1)
        {
            atk.GetSoul(1000);
            monster.name = "King ";
            
        }
        else if (qualitySeed <= 10)
        {
            atk.GetSoul(1000 / qualitySeed);
            monster.name = "Elite ";
            
        }
        string baseName = GetName();
        if (baseName.Length > 5)
            atk.GetSoul(10 * monster.name.Length);
        if (baseName == "god")
        {
            baseName = "The God";
            atk.GetSoul(1000);
        }if (baseName == "hero")
        {
            baseName = "The Hero";
            atk.GetSoul(1000);
        }
        if (baseName == "dog")
        {
            atk.GetSoul(100);
        }
        if (baseName == "cat")
        {
            atk.GetSoul(200);
        }
        if (baseName == "pig")
        {
            atk.GetSoul(100);
        }
        if (baseName == "master")
        {
            baseName = "The Master";
            atk.GetSoul(500);
        }
        if (baseName == "lord")
        {
            baseName = "The Lord";
            atk.GetSoul(800);
        }
        if (baseName == "ghost")
        {
            baseName = "GHOST";
            atk.GetSoul(300);
        }
        monster.name += baseName;
        atk.MonsterName = monster.name;
        return monster;
    }
    public void OnMonsterCreate(Attacker monster) {
        SoulPool -= monster.SoulPoint;
        if (monster is HeroAttacker)
        {
            hero = monster;
            heroState.SetAttakcer(hero);
        }
        else
            monsters.Add(monster);
        if(SoulLevel>1)
        {
            monster.GetSoul(SoulLevel * 100);
            SoulPool -= 100*SoulLevel;
        }
        if (updateCor==null && monsters.Count == SpawnMonsterCount)
        {
            updateCor = StartCoroutine(UpdaetStatusUI());            
        }
        popCount += 1;
    }
    public void OnMonsterDie(Attacker monster) {
        //Debug.Log(monster.name);
        deadCount += 1;
        SoulPool += monster.SoulPoint;// * SoulRecycle;
        if (monster is HeroAttacker)
        {
            heroDie++;
            foreach (var item in monsters)
            {
                item.GetSoul((int)(monster.SoulPoint * SoulRecycle / monsters.Count));
            }
        }
        else
        {
            monsters.Remove(monster);
            if (monster.transform == Camera.main.GetComponent<FollowTarget>().target)
                Camera.main.GetComponent<FollowTarget>().target = monsters[0].transform;
            
        }
        hero.GetComponent<skeleton>().OnKillDie(monster.transform);
        foreach (var item in monsters)
        {
            item.GetComponent<ChracterBase>().OnKillDie(monster.transform);
        }

        
        if(SoulPool>1000 && monsters.Count < SpawnMonsterCount)
            CreateMonster(monsterPrefabs[0]);
        UpdateUIImmediate();
        tarState.SetAttakcer(Camera.main.GetComponent<FollowTarget>().target.GetComponent<Attacker>().Target);

    }
	// Update is called once per frame
	void Update () {
	
	}

    private string GetName() {
        string[] a = new string[] { "a", "e", "i", "o","or", "u" ,"er","y"};
        string[] b = new string[] { "gh","ch","sh","st","b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "r", "s", "t", "v", "w", "x", "y", "z" };
        StringBuilder sb = new StringBuilder();
        sb.Append(b[Random.Range(0, b.Length)]);
        sb.Append(a[Random.Range(0, a.Length)]);
        sb.Append(b[Random.Range(0, b.Length)]);
        int appendNext = Random.Range(0, 3);
        while (appendNext <= 1)
        {
            sb.Append(a[Random.Range(0, a.Length)]);
            if (appendNext > 0)
                sb.Append(b[Random.Range(0, b.Length)]);
            else
                break;
            if (sb.Length >= 10)
                break;
        }
        return sb.ToString();
    }

    private IEnumerator UpdaetStatusUI() {
        this.monsters.Sort((a, b) => { return b.SoulPoint.CompareTo(a.SoulPoint); });
        if (Camera.main.GetComponent<FollowTarget>().target == null)
        {
            Camera.main.GetComponent<FollowTarget>().target = monsters[0].transform;
        }
        while (true) {
            if (updateUITimeLock <= Time.time)
                UpdateUIImmediate();
            yield return new WaitForSeconds(10f);
            this.monsters.Sort((a, b) => { return b.SoulPoint.CompareTo(a.SoulPoint); });
        }
    }
    private void UpdateUIImmediate() {
        lock (locker)
        {
            updateUITimeLock = Time.time + 10f;
            for (int i = 0; i < this.monsters.Count; i++)
            {
                if (this.statusUI.Count > i)
                {
                    if (statusUI[i].attacker != null && statusUI[i].attacker.transform == Camera.main.GetComponent<FollowTarget>().target)
                        statusUI[i].ShowFocus(false);
                    statusUI[i].SetAttakcer(this.monsters[i]);
                    if (statusUI[i].attacker.transform == Camera.main.GetComponent<FollowTarget>().target)
                        statusUI[i].ShowFocus(true);
                }
                else
                    this.monsters[i].GetComponent<ChracterBase>().SetHudText(null);
            }
        }
    }

    private void StatusOnClick(GameObject obj) {
        //Debug.Log(obj.name);
        StateUI sui = obj.GetComponent<StateUI>();
        if (sui.attacker == null || (!(sui.attacker is HeroAttacker) && sui.attacker.IsDead))
            return;
        Camera.main.GetComponent<FollowTarget>().target = sui.attacker.transform;
        foreach (var item in statusUI)
        {
            item.ShowFocus(false);
        }
        sui.ShowFocus(true);
        tarState.SetAttakcer(sui.attacker.Target);
    }

    public Transform GetBoss() {
        return monsters[0].transform;
    }

    private void SoulUpgrade() {
        SoulLevel++;
        SoulPoolNext = 10000+SoulLevel*10000;
        foreach (var item in monsters)
        {
            item.GetSoul((int)(SoulPool/100 / monsters.Count));
        }
        SoulPool = 10000;
    }
    float effectTimeLock;
    enum SpaceEvent
    {
        Hot,    // hp-1% hp-100/s
        Disease,// atk-2 def-2 speed-2
        Cold,   // mana-1% mana-5/s
        Weak,   // atk-2 def-2
        Freeze, // def - 2 speed-2
        Wet,    // speed - 3

        Fanaticism, // atk+1
        Tight,      // def+1
        Attension,  // speed+1
        Relax,      // hp+1% hp+100
        Meditation, // mp+1% mp+100

        Unluck, // luck-1
        Lucky,   // luck+1
        Count,
    }
    IEnumerator SpaceEffect() {
        List<int> effected = new List<int>();
        while (true) {
            if (effectTimeLock < Time.time)
            {
                effectTimeLock = Time.time + 30;
                currentEvent.Clear();
                for (int i = 1; i < SoulLevel; i++)
                {
                    SpaceEvent se =(SpaceEvent) Random.Range(0, (int)SpaceEvent.Count);
                    currentEvent.Add(se);
                }                
            }
            int effectMons = Random.Range(0, monsters.Count+1);
            
            for (int i = 0; i < currentEvent.Count; i++)
            {
                if (Random.Range(0,100)<=5)
                {
                    GetEvent(hero, currentEvent[i]);                    
                }
                for (int j = 0; j < effectMons; j++)
                {
                    int mon = Random.Range(0, monsters.Count);
                    if (effected.Contains(mon)) continue;
                    GetEvent(monsters[j], currentEvent[i]);
                    effected.Add(mon);
                }
            }
            effected.Clear();
            //currentEvent
            yield return new WaitForSeconds(5f);
        }
    }

    private void GetEvent(Attacker atk, SpaceEvent se) {
        switch (se)
        {
            // hp-1% hp-100/s
            case SpaceEvent.Hot:
                atk.healPoint.AddValue(-100);
                atk.healPoint.AddMaxValue(-(int)(atk.healPoint.MaxValue * 0.01));
                break;
            // atk-2 def-2 speed-2
            case SpaceEvent.Disease:
                atk.attack.AddMaxValue(-2);
                atk.deffence.AddMaxValue(-2);
                atk.speed.AddMaxValue(-2);
                break;
            // mana-1% mana-50/s
            case SpaceEvent.Cold:
                atk.manaPoint.AddValue(-50);
                atk.manaPoint.AddMaxValue(-(int)(atk.healPoint.MaxValue * 0.01));
                break;
            // atk-2 def-2
            case SpaceEvent.Weak:
                atk.attack.AddMaxValue(-1);
                atk.deffence.AddMaxValue(-1);
                break;
            // def - 2 speed-2
            case SpaceEvent.Freeze:
                atk.deffence.AddMaxValue(-1);
                atk.speed.AddMaxValue(-1);
                break;
            // speed - 3
            case SpaceEvent.Wet:
                atk.speed.AddMaxValue(-2);
                break;
            // atk+1
            case SpaceEvent.Fanaticism:
                atk.attack.AddMaxValue(2);
                break;
            // def+1
            case SpaceEvent.Tight:
                atk.deffence.AddMaxValue(2);
                break;
            // speed+1
            case SpaceEvent.Attension:
                atk.speed.AddMaxValue(1);
                break;
            // hp+1% hp+100
            case SpaceEvent.Relax:
                atk.healPoint.AddValue(100);
                atk.healPoint.AddMaxValue((int)(atk.healPoint.MaxValue * 0.01));
                break;
            // mp+1% mp+50
            case SpaceEvent.Meditation:
                atk.manaPoint.AddValue(50);
                atk.manaPoint.AddMaxValue((int)(atk.healPoint.MaxValue * 0.01));
                break;
            // luck-1
            case SpaceEvent.Unluck:
                atk.luck.AddMaxValue(-1);
                break;
            // luck+1
            case SpaceEvent.Lucky:
                atk.luck.AddMaxValue(1);
                break;
            case SpaceEvent.Count:
            default:
                break;
        }
        atk.GetComponent<ChracterBase>().ShowHUDText("I Got " + se.ToString(), Color.yellow, 1f);
    }

    public void RecycleSoul(int soul) {
        this.SoulPool += soul;
    }

    public void HeroKillBoss(HeroAttacker h) {
        HeroInfo hf = new HeroInfo()
        {
            name = h.MonsterName,
            killed = h.Kill,
            level = h.Level,
            soul = h.SoulPoint,
            rebirth = h.rebirthTimes,
            atk = h.attack.MaxValue,
            def = h.deffence.MaxValue,
            speed = h.speed.MaxValue,
            hp = h.healPoint.MaxValue,
            mp = h.manaPoint.MaxValue,
            luck = h.luck.MaxValue,
            mind = h.mind.MaxValue
        };
        Debug.Log(hf.ToString());
        heros.Add(hf);
    }
}
