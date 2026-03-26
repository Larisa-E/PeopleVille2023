namespace PeopleVilleEngine.Items;
using System;



public abstract class Clothes
{
    protected Clothes(string name, int value)
    {
    }
}

public class Shoes : Clothes
{
    public Shoes() : base("Shoes", 40) { }
}

public class Pants : Clothes
{
    public Pants() : base("Pants", 30) { }
}