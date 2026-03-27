namespace PeopleVilleEngine.Locations
{
    public abstract class LocationBase : ILocation
    {
        protected readonly List<BaseVillager> _villagers = new();
        public abstract string Name { get; }
        public virtual List<BaseVillager> Villagers() => _villagers;
        public abstract void Interact(BaseVillager villager);
    }
}
