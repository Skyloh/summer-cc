using System.Collections.Generic;

[System.Serializable]
public struct SerializableKeyValuePair<K, V>
{
    public K key;
    public V value;

    public SerializableKeyValuePair(KeyValuePair<K, V> pair)
    {
        key = pair.Key;
        value = pair.Value;
    }
}
