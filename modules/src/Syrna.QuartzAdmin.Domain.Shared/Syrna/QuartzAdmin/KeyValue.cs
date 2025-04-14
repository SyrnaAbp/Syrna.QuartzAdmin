namespace Syrna.QuartzAdmin
{
    public class KeyValue<TKey,TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public static KeyValue<TKey,TValue> Create(TKey key, TValue value)
        {
            return new KeyValue<TKey,TValue> { Key = key, Value = value };
        }
    }
}