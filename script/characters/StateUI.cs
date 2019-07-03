using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StateUI : MonoBehaviour
{

    public Attacker attacker;
    interface AttrUIControl
    {
        void UpdateValue();
        void SetAttr(CharacterAttri attr);
    }
    class SliderControl : AttrUIControl
    {
        Transform trans;
        CharacterAttri attr;
        public SliderControl(Transform trans, CharacterAttri attr)
        {
            this.trans = trans;
            this.attr = attr;

        }

        public void SetAttr(CharacterAttri attr)
        {
            this.attr = attr;
            UpdateValue();
        }
        public void UpdateValue()
        {
            int cur = attr == null ? 0 : attr.CurValue;
            int max = attr == null ? 0 : attr.MaxValue;
            int addValue = attr == null ? 0 : attr.BuffValue;
            UISlider bar = trans.GetComponent<UISlider>();
            bar.Set((float)cur / max);
            UILabel label = trans.Find("cur").GetComponent<UILabel>();
            label.text = cur.ToString() + (addValue > 0 ? ("(" + addValue.ToString() + ")") : "");
            label.color = addValue == 0 ? Color.white : (addValue < 0 ? Color.red : Color.green);
            label = trans.Find("max").GetComponent<UILabel>();
            label.text = max.ToString();
        }
    }

    class IconControl : AttrUIControl
    {
        Transform trans;
        CharacterAttri attr;
        public IconControl(Transform trans, CharacterAttri attr)
        {
            this.trans = trans;
            this.attr = attr;

        }
        public void SetAttr(CharacterAttri attr)
        {
            this.attr = attr;
            UpdateValue();
        }
        public void UpdateValue()
        {
            int cur = attr == null ? 0 : attr.CurValue;
            int max = attr == null ? 0 : attr.MaxValue;
            int addValue = attr == null ? 0 : attr.BuffValue;
            UILabel label = trans.Find("value").GetComponent<UILabel>();
            label.text = max.ToString();
            label.color = addValue == 0 ? Color.white : (addValue < 0 ? Color.red : Color.green);
        }

    }
    Dictionary<string, AttrUIControl> attrControls;
    UILabel lv;
    UILabel killed;
    UILabel soul;
    UITexture iconFocus;
    List<Transform> skillIcon;
    private void Awake()
    {
        attrControls = new Dictionary<string, AttrUIControl>();
        skillIcon = new List<Transform>();
        Transform skills = this.transform.Find("skills");
        for (int i = 0; i < skills.childCount; i++)
        {
            skillIcon.Add(skills.GetChild(i));
            
            UIEventListener.Get(skills.GetChild(i).gameObject).onHover = (go,state)=> {
                if (state)
                    UITooltip.Show(go.name);
                else
                    UITooltip.Hide();
            };
        }

        lv = this.transform.Find("lv").Find("value").GetComponent<UILabel>();
        killed = this.transform.Find("kill").Find("value").GetComponent<UILabel>();
        soul = this.transform.Find("soul").Find("value").GetComponent<UILabel>();
        iconFocus = this.transform.Find("focus").GetComponent<UITexture>();
        iconFocus.enabled = false;
        attrControls["hp"] = new SliderControl(this.transform.FindChild("hp"), null);
        attrControls["mp"] = new SliderControl(this.transform.FindChild("mp"), null);
        attrControls["exp"] = new SliderControl(this.transform.FindChild("exp"), null);
        attrControls["atk"] = new IconControl(this.transform.FindChild("atk"), null);
        attrControls["def"] = new IconControl(this.transform.FindChild("def"), null);
        attrControls["speed"] = new IconControl(this.transform.FindChild("speed"), null);
        attrControls["luck"] = new IconControl(this.transform.FindChild("luck"), null);
        attrControls["mind"] = new IconControl(this.transform.FindChild("mind"), null);
        //Transform hd = this.transform.FindChild("HUDText");
        //this.attacker.GetComponent<ChracterBase>().SetHudText(hd.GetComponent<HUDText>());
        //GameManager.gameMain.RegStateUI(this);

        

    }
    // Use this for initialization
    void Start()
    {
        GameManager.gameMain.RegStateUI(this);
        //this.attacker = this.GetComponentInParent<Attacker>();


        //UILabel label = this.transform.Find("name").GetComponent<UILabel>();
        //label.text = attacker.MonsterName;

        //lv.text = attacker.Level.ToString();        
        //killed.text = attacker.Kill.ToString();

        //Transform hd = this.transform.FindChild("HUDText");
        //UIFollowTarget ft = hd.GetComponent<UIFollowTarget>();
        //ft.target = this.attacker.transform;
        //ft.gameCamera = Camera.main;
        //ft.uiCamera = this.transform.GetComponentInParent<Camera>();

    }

    // Update is called once per frame
    void Update()
    {
        if (attacker == null)
            return;
        if (this.attacker != null && (!(attacker is HeroAttacker) && attacker.IsDead))
        {
            this.attacker = null;
            foreach (var item in attrControls)
            {
                item.Value.SetAttr(null);
            }
            Transform hd = this.transform.FindChild("HUDText");
            //UIFollowTarget ft = hd.GetComponent<UIFollowTarget>();
            //ft.target = null;
            //ft.enabled = false;
            return;
        }
        foreach (var item in attrControls)
        {
            item.Value.UpdateValue();
        }
        lv.text = attacker.Level.ToString();
        killed.text = attacker.Kill.ToString();
        soul.text = attacker.SoulPoint.ToString();
        UILabel labelName = this.transform.Find("name").GetComponent<UILabel>();
        labelName.text = this.attacker.MonsterName;
        for (int i = 0; i < skillIcon.Count; i++)
        {
            if (i < attacker.skills.Count)
            {
                skillIcon[i].gameObject.name = attacker.skills[i].Name;
                skillIcon[i].gameObject.SetActive(true);
                UITexture t = skillIcon[i].GetComponentInChildren<UITexture>();
                t.mainTexture = LoadResource<Texture2D>(attacker.skills[i].icon);
                t.color = attacker.skills[i].iconColor;
                UILabel l = skillIcon[i].GetComponentInChildren<UILabel>();
                l.text = attacker.skills[i].Level.ToString();
            }
            else if (i < attacker.skills.Count + attacker.passiveSkill.Count) {
                int ind = i - attacker.skills.Count;
                skillIcon[i].gameObject.name = attacker.passiveSkill[ind].Name;
                skillIcon[i].gameObject.SetActive(true);
                UITexture t = skillIcon[i].GetComponentInChildren<UITexture>();
                t.mainTexture = LoadResource<Texture2D>(attacker.passiveSkill[ind].icon);
                t.color = attacker.passiveSkill[ind].iconColor;
                UILabel l = skillIcon[i].GetComponentInChildren<UILabel>();
                l.text = attacker.passiveSkill[ind].Level.ToString();
            }
            else
            {
                skillIcon[i].gameObject.SetActive(false);
            }
        }
        // UITexture t = this.transform.Find("kill").GetComponent<UITexture>();
        //t.mainTexture = Resources.Load<Texture2D>("32px/Elements_Energy");
    }
    public void SetAttakcer(Attacker attacker)
    {
        this.attacker = attacker;
        attrControls["hp"].SetAttr(     this.attacker?this.attacker.healPoint:null);
        attrControls["mp"].SetAttr(     this.attacker?this.attacker.manaPoint : null);
        attrControls["exp"].SetAttr(    this.attacker?this.attacker.expPoint : null);
        attrControls["atk"].SetAttr(    this.attacker?this.attacker.attack : null);
        attrControls["def"].SetAttr(    this.attacker?this.attacker.deffence : null);
        attrControls["speed"].SetAttr(  this.attacker?this.attacker.speed : null);
        attrControls["luck"].SetAttr(   this.attacker?this.attacker.luck : null);
        attrControls["mind"].SetAttr(this.attacker ? this.attacker.mind : null);
        UILabel labelName = this.transform.Find("name").GetComponent<UILabel>();
        labelName.text = this.attacker?this.attacker.MonsterName:"";
        Transform hd = this.transform.FindChild("HUDText");
        //UIFollowTarget ft = hd.GetComponent<UIFollowTarget>();
        //ft.target = this.attacker.transform;    
        if(this.attacker)
            this.attacker.GetComponent<ChracterBase>().SetHudText(hd.GetComponent<HUDText>());
    }
    public void ShowFocus(bool focus) {
        iconFocus.enabled = focus;

    }
    private Dictionary<string, object> tempResources;
    public T LoadResource<T>(string path) where T:Object {
        if (tempResources == null)
            tempResources = new Dictionary<string, object>();
        if (tempResources.ContainsKey(path))
            return (T)tempResources[path];
        T obj =Resources.Load<T>(path);
        tempResources[path] = obj;
        return obj;


    }
}
