namespace StayFresh
{
    // The spoilage multiplier of the mod for one item: the factor of AnimalSpoilageSlowdown for an animal product, else
    // the factor of SpoilageSlowdown. A liquid spoils through the stack of its container, so it gets the multiplier of the container item.
    public sealed class SpoilageRule
    {
        private readonly AnimalProducts animalProducts;
        private readonly float spoilage;
        private readonly float animal;

        public SpoilageRule(AnimalProducts animalProducts, float spoilage, float animal)
        {
            this.animalProducts = animalProducts;
            this.spoilage = spoilage;
            this.animal = animal;
        }

        public float Multiplier(string item) => IsAnimalProduct(item) ? animal : spoilage;

        public bool IsAnimalProduct(string item) => animalProducts.Contains(item);
    }
}
