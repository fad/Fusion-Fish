using System;
using System.Collections.Generic;

public class EquallyDistributedWeightedPicker<T>
{
    private struct WeightedItem : IEquatable<WeightedItem>
    {
        public byte Weight;
        public T Item;

        public bool Equals(WeightedItem other)
        {
            return Weight == other.Weight && EqualityComparer<T>.Default.Equals(Item, other.Item);
        }

        public override bool Equals(object obj)
        {
            return obj is WeightedItem other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Weight, Item);
        }
    }

    private WeightedItem[] _items;

    private byte _totalWeight;
    private byte _equalWeightAmount;
    private byte _lessLikelyWeightAmount;

    private byte _lastItemPickedIndex;
    private bool _hasPickedAtLeastOnce = false;

    private EquallyDistributedWeightedPicker()
    {
    }

    public EquallyDistributedWeightedPicker<T> WithEqualWeight(byte value)
    {
        _equalWeightAmount = value;

        CheckForValidity();
        return this;
    }

    public EquallyDistributedWeightedPicker<T> WithLessLikelyWeight(byte value)
    {
        _lessLikelyWeightAmount = value;

        CheckForValidity();
        return this;
    }

    public EquallyDistributedWeightedPicker<T> WithItems(IList<T> items)
    {
        _items = new WeightedItem[items.Count];
        CheckForValidity();

        for (byte i = 0; i < items.Count; i++)
        {
            _items[i] = new WeightedItem
            {
                Item = items[i],
                Weight = _equalWeightAmount
            };
        }

        _totalWeight = (byte)(_equalWeightAmount * items.Count);

        return this;
    }

    public EquallyDistributedWeightedPicker<T> Build()
    {
        _totalWeight = (byte)(_equalWeightAmount * _items.Length);
        
        if(_items[0].Weight != _equalWeightAmount)
        {
            DistributeWeights();
        }
        
        return this;
    }

    public T Pick()
    {
        if (_items is null)
        {
            throw new InvalidOperationException("No items have been added to the picker.");
        }

        byte randomValue = (byte)UnityEngine.Random.Range(1, _totalWeight);
        byte currentWeight = 0;

        for (byte i = 0; i < _items.Length; i++)
        {
            currentWeight += _items[i].Weight;

            if (randomValue <= currentWeight)
            {
                _items[i].Weight = _lessLikelyWeightAmount;

                if (_hasPickedAtLeastOnce)
                {
                    _items[_lastItemPickedIndex].Weight = _equalWeightAmount;
                }

                _lastItemPickedIndex = i;
                _hasPickedAtLeastOnce = true;
                
                _totalWeight = (byte)(_totalWeight - _equalWeightAmount + _lessLikelyWeightAmount);

                return _items[i].Item;
            }
        }

        throw new InvalidOperationException($"No item was picked. Random value: {randomValue}, Total weight: {_totalWeight}, Current weight: {currentWeight}, Items Length: {_items.Length}");
    }

    public static EquallyDistributedWeightedPicker<T> Create() => new();

    private void CheckForValidity()
    {
        if (_items is { Length: > 0 })
        {
            int maxValuePossible = _items.Length * _equalWeightAmount;

            if (maxValuePossible > byte.MaxValue)
            {
                throw new ArgumentException("The total weight of all items exceeds the maximum value of a byte.");
            }
        }
    }
    
    private void DistributeWeights()
    {
        for (byte i = 0; i < _items.Length; i++)
        {
            _items[i].Weight = _equalWeightAmount;
        }
    }
}
