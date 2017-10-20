using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
    public class BossFight
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The name for this BossFight
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The instance that this BossFight belongs to
        /// </summary>
        [Display(Name="Instance")]
        public int InstanceId { get; set; }
        /// <summary>
        /// Obsolete - was used (a long time ago) for determining kill vs wipe when a boss disappears in a certain period of time
        /// </summary>
        [Obsolete]
        public long DpsCheck { get; set; }
        /// <summary>
        /// True if we need to check extra details when parsing, otherwise false.
        /// Some examples of this are when we want to check for more than one NPC dying to verify whether an encounter was successful or not
        /// e.g. Bulf in Mount Sharax dies 3 times and then vanishes when close to death #4
        /// Twins in frozen tempest - need to verify that they both died
        /// PUMPKIN in Tyrant's Forge - PUMPKIN and Lt Charles both have to die
        /// And so on
        /// </summary>
        [Display(Name = "Requires special processing")]
        public bool RequiresSpecialProcessing { get; set; }
        /// <summary>
        /// Obsolete - was previously used to distinguish between two bosses with the same name (of different tiers)
        /// such as Level 50 HK vs the newer HK
        /// </summary>
        [Obsolete]
        public string UniqueAbilityName { get; set; }
        /// <summary>
        /// Obsolete - similar to the last property but a boolean flag denoting this would be required
        /// </summary>
        [Obsolete]
        [Display(Name = "Priority if duplicate")]
        public bool PriorityIfDuplicate { get; set; }
        /// <summary>
        /// Obsolete - before difficulties were added, this property was used to track boss HP
        /// </summary>
        [Obsolete]
        public long Hitpoints { get; set; }
        /// <summary>
        /// Obsolete - before difficulties were added, this property was used to track the target to which the HP pool belonged
        /// </summary>
        [Obsolete]
        public string HitpointTarget { get; set; }

        /// <summary>
        /// Used to find the portrait image
        /// </summary>
        public string PortraitFilename
        {
            get
            {
                if (Name != null)
                {
                    return Name ?? Name.ToLower().Replace(" ", "").Replace(":", "");
                }
                return null;
            }
        }

        // Dapper objects
        public BossFightDifficulty Difficulty { get; set; }
        public Instance Instance { get; set; }

        // Lists for UI
        public List<Instance> Instances { get; set; }
        public List<EncounterDifficulty> DifficultySettings { get; set; }

        // Mappers
        public static Func<BossFight, Instance, BossFight> Map =
            (bf, ins) =>
            {
                bf.Instance = ins;
                return bf;
            };
    }
}
