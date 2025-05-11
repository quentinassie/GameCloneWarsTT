using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Card : MonoBehaviour
{
    private int courage;
    private int ruse;
    private int autorite;
    private int aptitudeAuCombat;
    private int techniquesDeCombat;
    private int pouvoirJedi;
    //private string name;

    public int GetCourage() => courage;
    public int GetRuse() => ruse;
    public int GetAutorite() => autorite;
    public int GetAptitudeAuCombat() => aptitudeAuCombat;
    public int GetTechniquesDeCombat() => techniquesDeCombat;
    public int GetPouvoirJedi() => pouvoirJedi;
    //public string GetName() => name;

    public void SetCourage(int value) => courage = value;
    public void SetRuse(int value) => ruse = value;
    public void SetAutorite(int value) => autorite = value;
    public void SetAptitudeAuCombat(int value) => aptitudeAuCombat = value;
    public void SetTechniquesDeCombat(int value) => techniquesDeCombat = value;
    public void SetPouvoirJedi(int value) => pouvoirJedi = value;
    //public void SetName(string name) => this.name = name;

    public void SetParameters()
    {
        if (name.Contains("MaceWindu"))
        {
            SetCourage(21);
            SetRuse(38);
            SetAutorite(83);
            SetAptitudeAuCombat(133);
            SetTechniquesDeCombat(30);
            SetPouvoirJedi(8);
        }
        else if (name.Contains("Ziro"))
        {
            SetCourage(9);
            SetRuse(38);
            SetAutorite(69);
            SetAptitudeAuCombat(59);
            SetTechniquesDeCombat(26);
            SetPouvoirJedi(0);
        }
        else if (name.Contains("Rancor"))
        {
            SetCourage(20);
            SetRuse(14);
            SetAutorite(36);
            SetAptitudeAuCombat(140);
            SetTechniquesDeCombat(11);
            SetPouvoirJedi(0);
        }
        else if (name.Contains("MagnaGuard"))
        {
            SetCourage(24);
            SetRuse(28);
            SetAutorite(51);
            SetAptitudeAuCombat(117);
            SetTechniquesDeCombat(79);
            SetPouvoirJedi(0);
        }
        else if (name.Contains("NuteGunray"))
        {
            SetCourage(10);
            SetRuse(31);
            SetAutorite(53);
            SetAptitudeAuCombat(53);
            SetTechniquesDeCombat(66);
            SetPouvoirJedi(3);
        }
        else if (name.Contains("Yoda"))
        {
            SetCourage(21);
            SetRuse(43);
            SetAutorite(92);
            SetAptitudeAuCombat(135);
            SetTechniquesDeCombat(32);
            SetPouvoirJedi(10);
        }
    }
}
