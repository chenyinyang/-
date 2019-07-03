using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterAttri{
    public enum buffType {
        abs,
        ratio
    }
    class buff {
        public float value;
        public  buffType type;
        public buff(float value, buffType type) {
            this.value = value;
            this.type = type;
        }
    }
    private List<buff> buffValues;
    private int absMax=-1;
    private int absMin=0;
    private int baseValue;
    public int BuffValue { get; private set; }
    private int _curValue;
    public int CurValue
    {
        get { return _curValue; }
    }
    public int MaxValue { get { return Mathf.Max(Mathf.Min(baseValue + BuffValue+ expValue, absMax), absMin); } }

    private int _exp;
    private int expValue;
    public int Exp
    {
        get { return _exp; }
        set
        {
            this._exp = value;
            if (this._exp > 100)
            {
                this.expValue += 1;
                this._exp = 0;
            }
        }


    }
    public CharacterAttri(int initialValue,int absMax=-1,int absMin=0) {
        this.baseValue = initialValue;
        this._curValue = initialValue;
        this.buffValues = new List<buff>();
        this.absMax = absMax;
        this.absMin = absMin;
    }

    

    public int AddValue(int value) {
        _curValue += value;
        if (_curValue < absMin)
            _curValue = absMin;
        if (_curValue > MaxValue)
            _curValue = MaxValue;
        return CurValue;
    }
    public void AddMaxValue(int value) {
        this.baseValue+=value;
        if (this.baseValue <= absMin)
            baseValue = absMin;
        if (absMax > 0 && baseValue > absMax)
            baseValue = absMax;
        _curValue += value;
        if (CurValue <= absMin) _curValue = absMin;
        
    }
    private int CalcBuffValue() {
        int value = 0;
        for (int i = 0; i < buffValues.Count; i++)
        {
            if (buffValues[i].type == buffType.abs)
                value += (int)buffValues[i].value;
            else
                value += (int)(baseValue * buffValues[i].value);
        }
        return value;
    }
    
    public void AddBuff(float value,buffType type)
    {
        buffValues.Add(new buff(value,type));
        BuffValue = CalcBuffValue();
    }
    public void RemoveBuff(float value,buffType type)
    {
        buff b = buffValues.Find((bf) => {
            return bf.value == value && bf.type == type; });
        buffValues.Remove(b);
        BuffValue = CalcBuffValue();
    }
    public void ResetTo(int value,bool resetExp=false) {
        this.buffValues.Clear();
        BuffValue = CalcBuffValue();
        baseValue = value;
        this._curValue = MaxValue;
        if (resetExp)
        {
            this.Exp = 0;
            this.expValue = 0;
        }
    }
}
