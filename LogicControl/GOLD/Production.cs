using System.Collections;

 // ERROR: Not supported in C#: OptionDeclaration
using System.ComponentModel;

public class Production
{
	//================================================================================
	// Class Name:
	//      Production 
	//
	// Instancing:
	//      Public; Non-creatable  (VB Setting: 2- PublicNotCreatable)
	//
	// Purpose:
	//      The Rule class is used to represent the logical structures of the grammar.
	//      Rules consist of a head containing a nonterminal followed by a series of
	//      both nonterminals and terminals.
	//
	// Author(s):
	//      Devin Cook
	//      http://www.devincook.com/goldparser
	//
	// Dependacies:
	//      Symbol Class, SymbolList Class
	//
	//================================================================================

	private Symbol m_Head;
	private SymbolList m_Handle;

	private short m_TableIndex;
	internal Production(Symbol Head, short TableIndex)
	{
		m_Head = Head;
		m_Handle = new SymbolList();
		m_TableIndex = TableIndex;
	}

	internal Production()
	{
		//Nothing
	}

	[Description("Returns the head of the production.")]
	public Symbol Head()
	{
		return m_Head;
	}

	[Description("Returns the symbol list containing the handle (body) of the production.")]
	public SymbolList Handle()
	{
		return m_Handle;
	}

	[Description("Returns the index of the production in the Production Table.")]
	public short TableIndex()
	{
		return m_TableIndex;
	}

	public override string ToString()
	{
		return Text();
	}

	[Description("Returns the production in BNF.")]
	public string Text(bool AlwaysDelimitTerminals = false)
	{
		return m_Head.Text() + " ::= " + m_Handle.Text(" ", AlwaysDelimitTerminals);
	}

	internal bool ContainsOneNonTerminal()
	{
		bool Result = false;

		if (m_Handle.Count() == 1) {
			if (m_Handle[0].Type == SymbolType.Nonterminal) {
				Result = true;
			}
		}

		return Result;
	}
}

public class ProductionList
{
		//Cannot inherit, must hide methods that change the list
	private ArrayList m_Array;

	internal ProductionList()
	{
		m_Array = new ArrayList();
	}

	internal ProductionList(int Size)
	{
		m_Array = new ArrayList();
		ReDimension(Size);
	}

	internal void Clear()
	{
		m_Array.Clear();
	}

	internal void ReDimension(int Size)
	{
		//Increase the size of the array to Size empty elements.
		int n = 0;

		m_Array.Clear();
		for (n = 0; n <= Size - 1; n++) {
			m_Array.Add(null);
		}
	}

	[Description("Returns the production with the specified index.")]
	public new Production this[int Index] {
		get { return (Production)this.m_Array[Index]; }

		internal set { m_Array[Index] = value; }
	}

	[Description("Returns the total number of productions in the list.")]
	public int Count()
	{
		return m_Array.Count;
	}

	internal new int Add(Production Item)
	{
		return m_Array.Add(Item);
	}
}
