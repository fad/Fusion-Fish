using System;
using System.Collections.Generic;

/// <summary>
/// Utility class for picking items from a small list with equally weighted probabilities.
/// </summary>
/// <typeparam name="T">Type of item in the list.</typeparam>
public class EquallyDistributedWeightedPicker<T>
{
    /// <summary>
    /// Item in the list with a weight.
    /// </summary>
    private struct WeightedItem : IEquatable<WeightedItem>
    {
        /// <summary>
        /// Current weight of the item.
        /// </summary>
        public byte Weight;

        /// <summary>
        /// Item to return when picked.
        /// </summary>
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

    /// <summary>
    /// All items in the weighted picker.
    /// </summary>
    private WeightedItem[] _items;

    /// <summary>
    /// The total weight of all items in the list.
    /// </summary>
    private byte _totalWeight;

    /// <summary>
    /// The value for the equal weight.
    /// </summary>
    private byte _equalWeightAmount;

    /// <summary>
    /// The value for the less likely weight. This is used for the last picked item.
    /// </summary>
    private byte _lessLikelyWeightAmount;

    /// <summary>
    /// Index of the last picked item.
    /// </summary>
    private byte _lastItemPickedIndex;

    /// <summary>
    /// Flag to show whether all items still have the same weight.
    /// </summary>
    private bool _hasPickedAtLeastOnce = false;

    private EquallyDistributedWeightedPicker()
    {
    }

    /// <summary>
    /// Sets the equal weight value for all items in the list.
    /// </summary>
    /// <param name="value">The value to distribute over all items.</param>
    /// <returns>The current weighted picker object with the new equal weight value.</returns>
    public EquallyDistributedWeightedPicker<T> WithEqualWeight(byte value)
    {
        _equalWeightAmount = value;

        CheckForValidity();
        return this;
    }

    /// <summary>
    /// Sets the less likely weight value for all items in the list.
    /// </summary>
    /// <param name="value">The value to distribute over all items.</param>
    /// <returns>The current weighted picker object with the new less likely weight value.</returns>
    public EquallyDistributedWeightedPicker<T> WithLessLikelyWeight(byte value)
    {
        _lessLikelyWeightAmount = value;

        CheckForValidity();
        return this;
    }

    /// <summary>
    /// Initializes the weighted picker with a list of items.
    /// </summary>
    /// <param name="items">The list to use.</param>
    /// <returns>The current weighted picker object with the newly added items.</returns>
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

    /// <summary>
    /// Builds the current weighted picker object and initializes every needed value correctly.
    /// </summary>
    /// <returns>The current weighted picker object with freshly initialized values.</returns>
    public EquallyDistributedWeightedPicker<T> Build()
    {
        _totalWeight = (byte)(_equalWeightAmount * _items.Length);

        if (_items[0].Weight != _equalWeightAmount)
        {
            DistributeWeights();
        }

        return this;
    }

    /// <summary>
    /// Picks an item from the list with equally distributed weights.
    /// </summary>
    /// <returns>The item that was chosen.</returns>
    /// <exception cref="InvalidOperationException">No items have been added to the picker or no item was picked due to wrong initialization.</exception>
    public T Pick()
    {
        if (_items is null)
        {
            throw new InvalidOperationException("No items have been added to the picker.");
        }

        byte randomValue = (byte)UnityEngine.Random.Range(0, _totalWeight);
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
                else
                {
                    _totalWeight = (byte)(_totalWeight - _equalWeightAmount + _lessLikelyWeightAmount);
                }

                _lastItemPickedIndex = i;
                _hasPickedAtLeastOnce = true;

                return _items[i].Item;
            }
        }

        throw new InvalidOperationException($"No item was picked. Current weight: {currentWeight}, Random value: {randomValue}, total weight: {_totalWeight}");
    }

    /// <summary>
    /// Creates a new instance of the weighted picker.
    /// </summary>
    /// <returns>New instance of the weighted picker.</returns>
    public static EquallyDistributedWeightedPicker<T> Create() => new();

    /// <summary>
    /// Checks if values are valid for the weighted picker.
    /// </summary>
    /// <exception cref="ArgumentException">The byte max value has been exceeded by the total weight. Hence, the list is too long.</exception>
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

    /// <summary>
    /// Fills the weights of all items with the equal weight value.
    /// </summary>
    private void DistributeWeights()
    {
        for (byte i = 0; i < _items.Length; i++)
        {
            _items[i].Weight = _equalWeightAmount;
        }
    }
}
