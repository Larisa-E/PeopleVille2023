public abstract class Item
{
	public string Name {  get; set; }
	public int Value { get; set; }

	protected Item(string name, int value) 
	{
		Name = name;
		Value = value;
	}
}

