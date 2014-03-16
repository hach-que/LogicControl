using System.Collections;


internal class Group
{
	public enum AdvanceMode
	{
		Token = 0,
		Character = 1
	}

	public enum EndingMode
	{
		Open = 0,
		Closed = 1
	}


	internal short TableIndex;
	internal string Name;
	internal Symbol Container;
	internal Symbol Start;

	internal Symbol End;
	internal AdvanceMode Advance;

	internal EndingMode Ending;

	internal IntegerList Nesting;
	internal Group()
	{
		Advance = AdvanceMode.Character;
		Ending = EndingMode.Closed;
		Nesting = new IntegerList();
		//GroupList
	}
}


internal class GroupList : ArrayList
{

	public GroupList() : base()
	{
	}

	internal GroupList(int Size) : base()
	{
		ReDimension(Size);
	}

	internal void ReDimension(int Size)
	{
		//Increase the size of the array to Size empty elements.
		int n = 0;

		base.Clear();
		for (n = 0; n <= Size - 1; n++) {
			base.Add(null);
		}
	}

	public new Group this[int Index] {
		get { return (Group)base[Index]; }

		set { base[Index] = value; }
	}

	public new int Add(Group Item)
	{
		return base.Add(Item);
	}
}


internal class IntegerList : ArrayList
{
	public new int this[int Index] {
		get { return (int)base[Index]; }

		set { base[Index] = value; }
	}

	public new int Add(int Value)
	{
		return base.Add(Value);
	}

	public new bool Contains(int Item)
	{
		return base.Contains(Item);
	}
}
