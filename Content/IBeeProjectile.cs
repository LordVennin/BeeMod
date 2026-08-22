namespace VenninBeeMod.Content
{
    /// <summary>
    /// Marks a projectile as one of the mod's bees.
    /// </summary>
    /// <remarks>
    /// Anything that wants to act on "all your bees" - the Rotweave Shroud today, set bonuses
    /// and accessories later - reads this rather than carrying its own list of types. Declaring
    /// it on the class is what keeps it honest: a new bee opts in where it is written, instead
    /// of relying on somebody remembering to add it to a registry somewhere else.
    ///
    /// It means "this is a bee", not "this bee damages things". Healers and conduits carry it
    /// too, because the next thing that reads this set may well care about them.
    /// </remarks>
    public interface IBeeProjectile
    {
    }
}
