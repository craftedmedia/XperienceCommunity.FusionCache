namespace XperienceCommunity.FusionCache.KeyGenerators
{
    internal interface ICacheKeysGenerator<in TEventArgs>
    {
        /// <summary>
        /// Generates dummy cache keys.
        /// </summary>
        /// <param name="eventArgs">Event arguments used to generate dummy keys.</param>
        /// <returns>Dummy cache keys.</returns>
        IEnumerable<string> GetDummyKeys(TEventArgs eventArgs);
    }
}
