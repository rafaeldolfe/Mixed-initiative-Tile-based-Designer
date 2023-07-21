namespace Tiles
{
    using System;

    [Serializable]
    public class IntFloatDictionary : UnitySerializedDictionary<int, float>
    {
        public IntFloatDictionary()
        {
        }

        public IntFloatDictionary(IntFloatDictionary toClone) : base(toClone)
        {
        }

        public IntFloatDictionary Clone()
        {
            return new IntFloatDictionary(this);
        }
    }
}