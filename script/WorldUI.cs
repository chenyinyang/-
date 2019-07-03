using UnityEngine;
using System.Collections;

public class WorldUI : MonoBehaviour {

    public UILabel curMons;
    public UILabel popMons;
    public UILabel dieMons;
    public UILabel dieHero;
    public UILabel souls;
    public UISlider sliderSouls;
    GameManager game;
    private void Awake()
    {
        curMons = this.transform.Find("counter/current").GetComponent<UILabel>();
        popMons = this.transform.Find("counter/pop").GetComponent<UILabel>();
        dieMons = this.transform.Find("counter/dead").GetComponent<UILabel>();
        dieHero = this.transform.Find("counter/heroDie").GetComponent<UILabel>();
        souls = this.transform.Find("soul/value").GetComponent<UILabel>();
        sliderSouls = this.transform.Find("soul").GetComponent<UISlider>();
    }
    // Use this for initialization
    void Start () {
        game = GameManager.gameMain;
	
	}
	
	// Update is called once per frame
	void Update () {
        curMons.text = game.curdCount.ToString();
        popMons.text = game.popCount.ToString();
        dieMons.text = game.deadCount.ToString();
        dieHero.text = game.heroDie.ToString();
        souls.text = ((int)game.SoulPool).ToString();
        sliderSouls.value = (float)(game.SoulPool / game.SoulPoolNext);
    }
}
