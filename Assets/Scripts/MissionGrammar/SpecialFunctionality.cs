public class SpecialFunctionality
{
    public string guid;
    public SpecialNodeType type;
    public string tempId = "";

    internal SpecialFunctionality DeepCopy()
    {
        return this.MemberwiseClone() as SpecialFunctionality;
    }
}
