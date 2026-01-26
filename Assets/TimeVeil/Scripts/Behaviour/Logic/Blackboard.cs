using System.Collections.Generic;

/// <summary>
/// A shared memory structure used by behavior tree nodes to store and retrieve data.
/// </summary>
public class Blackboard
{
    /// <summary>
    /// Internal dictionary storing key-value pairs.
    /// </summary>
    private Dictionary<string, object> data = new Dictionary<string, object>();

    /// <summary>
    /// Stores a value of type <typeparamref name="T"/> under the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value to store.</typeparam>
    /// <param name="key">The key under which the value is stored.</param>
    /// <param name="value">The value to store.</param>
    public void Set<T>(string key, T value)
    {
        data[key] = value;
    }

    /// <summary>
    /// Retrieves a value of type <typeparamref name="T"/> associated with the specified key.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value if found; otherwise, the default value of type <typeparamref name="T"/>.</returns>
    public T Get<T>(string key)
    {
        if (data.TryGetValue(key, out object value))
        {
            return (T)value;
        }
        return default;
    }

    /// <summary>
    /// Checks whether the specified key exists in the blackboard.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
    public bool Contains(string key)
    {
        return data.ContainsKey(key);
    }

    /// <summary>
    /// Clears all data stored in the blackboard.
    /// </summary>
    public void Clear()
    {
        data.Clear();
    }
}