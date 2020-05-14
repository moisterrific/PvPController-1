using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using PvPController.StorageTypes;

namespace PvPController
{
    public class Database
    {
        private MongoClient client;
        private IMongoDatabase Db;
        private IMongoCollection<BsonDocument> WeaponCollection;
        private IMongoCollection<BsonDocument> ProjectileCollection;
        private IMongoCollection<BsonDocument> WeaponBuffCollection;

        public Database(Config config)
        {
            var host = config.Database.Hostname;
            var port = config.Database.Port;
            var dbName = config.Database.DBName;

            client = new MongoClient($"mongodb://{host}:{port}");
            Db = client.GetDatabase(dbName);
            WeaponCollection = Db.GetCollection<BsonDocument>("weapons");
            ProjectileCollection = Db.GetCollection<BsonDocument>("projectiles");
            WeaponBuffCollection = Db.GetCollection<BsonDocument>("weaponbuffs");
        }

        public Dictionary<int, Weapon> GetWeapons()
        {
            var weaponList = new Dictionary<int, Weapon>();
            var cursor = WeaponCollection.Find(new BsonDocument()).ToCursor();
            foreach (var item in cursor.ToEnumerable())
            {
                var netID = item["NetID"].AsInt32;
                var weapon = new Weapon
                {
                    netID = netID,
                    damageRatio = Convert.ToSingle(item["CurrentDamage"]) / Convert.ToSingle(item["BaseDamage"]),
                    velocityRatio = Convert.ToSingle(item["CurrentVelocity"]) / Convert.ToSingle(item["BaseVelocity"]),
                    banned = Convert.ToBoolean(item["Banned"]),
                    currentVelocity = Convert.ToSingle(item["CurrentVelocity"]),
                    baseVelocity = Convert.ToSingle(item["BaseVelocity"]),
                    currentDamage = Convert.ToSingle(item["CurrentDamage"]),
                    baseDamage = Convert.ToSingle(item["BaseDamage"]),
                    minDamage = Convert.ToSingle(item["MinDamage"]),
                    maxDamage = Convert.ToSingle(item["MaxDamage"])
                };

                if (weaponList.ContainsKey(netID))
                {
                    Console.WriteLine($"NetID: {netID} already exists.");
                }
                weaponList.Add(netID, weapon);
            }

            return weaponList;
        }

        public List<Projectile> GetProjectiles()
        {
            var projectileList = new List<Projectile>();
            var cursor = ProjectileCollection.Find(new BsonDocument()).ToCursor();
            foreach (var item in cursor.ToEnumerable())
            {
                var projectile = new Projectile(item["NetID"].AsInt32,
                                        Convert.ToSingle(item["DamageRatio"]),
                                        Convert.ToSingle(item["VelocityRatio"]),
                                        Convert.ToBoolean(item["Banned"]));
                projectileList.Add(projectile);
            }

            return projectileList;
        }


        /// <summary>
        /// Gets the weapon buffs and adds them to the appropriate weapons in the weapons list
        /// </summary>
        /// <param name="weapons"></param>
        public void AddWeaponBuffs(Dictionary<int, Weapon> weapons)
        {
            var cursor = WeaponBuffCollection.Find(new BsonDocument()).ToCursor();
            foreach (var item in cursor.ToEnumerable())
            {
                var buff = new Buff(Convert.ToInt32(item["NetID"]), Convert.ToInt32(item["Milliseconds"]));
                var weaponNetID = item["WeaponNetID"].AsInt32;
                var hasWeapon = weapons.ContainsKey(weaponNetID);
                if (hasWeapon)
                {
                    weapons[weaponNetID].buffs.Add(buff);
                }
            }
        }
    }
}
