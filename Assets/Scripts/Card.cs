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
        else if (name.Contains("JarJarBinks"))
        {
            SetCourage(16);
            SetRuse(32);
            SetAutorite(78);
            SetAptitudeAuCombat(84);
            SetTechniquesDeCombat(23);
            SetPouvoirJedi(3);
        }
        else if (name.Contains("AdmiralYularen"))
        {
            SetCourage(20);
            SetRuse(37);
            SetAutorite(82);
            SetAptitudeAuCombat(99);
            SetTechniquesDeCombat(70);
            SetPouvoirJedi(5);
        }
        else if (name.Contains("LuminaraUnduli"))
        {
            SetCourage(20);
            SetRuse(34);
            SetAutorite(80);
            SetAptitudeAuCombat(124);
            SetTechniquesDeCombat(24);
            SetPouvoirJedi(6);
        }
        else if (name.Contains("C3PO"))
        {
            SetCourage(12);
            SetRuse(22);
            SetAutorite(58);
            SetAptitudeAuCombat(62);
            SetTechniquesDeCombat(30);
            SetPouvoirJedi(3);
        }
        else if (name.Contains("ChancelerPalpatine"))
        {
            SetCourage(16);
            SetRuse(48);
            SetAutorite(92);
            SetAptitudeAuCombat(133);
            SetTechniquesDeCombat(28);
            SetPouvoirJedi(9);
        }
        else if (name.Contains("R2D2"))
        {
            SetCourage(16);
            SetRuse(27);
            SetAutorite(42);
            SetAptitudeAuCombat(76);
            SetTechniquesDeCombat(64);
            SetPouvoirJedi(3);
        }
    }
}
