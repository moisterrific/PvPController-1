using System.Collections.Generic;

namespace PvPController.StorageTypes
{
    public class Weapon
    {
        public int netID;
        public float baseDamage;
        public float currentDamage;
        public float damageRatio;
        public float baseVelocity;
        public float currentVelocity;
        public float velocityRatio;
        public float minDamage = -1f;
        public float maxDamage = -1f;
        public bool banned;
        public List<Buff> buffs = new List<Buff>();

        public Weapon() { }

        public void setBuffs(List<Buff> buffs)
        {
            this.buffs = buffs;
        }   
    }
}
