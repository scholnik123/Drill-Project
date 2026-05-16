namespace SCADASim.Core
{
    public readonly struct DifficultyTuning
    {
        public readonly string DisplayName;
        public readonly float CorridorMultiplier;
        public readonly float CreditMultiplier;
        public readonly float HoldTimeMultiplier;
        public readonly float FatigueMultiplier;
        public readonly float ReactionDelayMultiplier;
        public readonly float MistakeChanceMultiplier;
        public readonly float IncidentThresholdMultiplier;

        public DifficultyTuning(
            string displayName,
            float corridorMultiplier,
            float creditMultiplier,
            float holdTimeMultiplier,
            float fatigueMultiplier,
            float reactionDelayMultiplier,
            float mistakeChanceMultiplier,
            float incidentThresholdMultiplier)
        {
            DisplayName = displayName;
            CorridorMultiplier = corridorMultiplier;
            CreditMultiplier = creditMultiplier;
            HoldTimeMultiplier = holdTimeMultiplier;
            FatigueMultiplier = fatigueMultiplier;
            ReactionDelayMultiplier = reactionDelayMultiplier;
            MistakeChanceMultiplier = mistakeChanceMultiplier;
            IncidentThresholdMultiplier = incidentThresholdMultiplier;
        }
    }

    public static class DifficultyProfile
    {
        public static DifficultyTuning Get(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Realistic:
                    return new DifficultyTuning("МАСТЕР", 1.0f, 1.15f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f);

                case DifficultyLevel.Expert:
                    return new DifficultyTuning("ХАРДКОР", 0.72f, 1.55f, 1.35f, 1.38f, 1.32f, 1.75f, 0.82f);

                case DifficultyLevel.Easy:
                    return new DifficultyTuning("УЧЕНИК+", 1.28f, 0.92f, 0.82f, 0.78f, 0.82f, 0.68f, 1.14f);

                default:
                    return new DifficultyTuning("УЧЕНИК", 1.45f, 0.75f, 0.68f, 0.62f, 0.68f, 0.52f, 1.25f);
            }
        }
    }
}
