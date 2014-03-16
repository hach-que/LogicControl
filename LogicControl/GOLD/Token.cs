using System.Collections;
 // ERROR: Not supported in C#: OptionDeclaration
using System.ComponentModel;

public class Token
{
	//================================================================================
	// Class Name:
	//      Token
	//
	// Purpose:
	//       While the Symbol represents a class of terminals and nonterminals, the
	//       Token represents an individual piece of information.
	//       Ideally, the token would inherit directly from the Symbol Class, but do to
	//       the fact that Visual Basic 5/6 does not support this aspect of Object Oriented
	//       Programming, a Symbol is created as a member and its methods are mimicked.
	//
	// Author(s):
	//      Devin Cook
	//
	// Dependacies:
	//      Symbol, Position
	//
	//================================================================================
	private short m_State;
	private object m_Data;
	private Symbol m_Parent;

	private Position m_Position = new Position();
	internal Token()
	{
		m_Parent = null;
		m_Data = null;
		m_State = 0;
	}

	public Token(Symbol Parent, object Data)
	{
		m_Parent = Parent;
		m_Data = Data;
		m_State = 0;
	}

	[Description("Returns the line/column position where the token was read.")]
	public Position Position()
	{
		return m_Position;
	}

	[Description("Returns/sets the object associated with the token.")]
	public dynamic Data {
		get { return m_Data; }
		set { m_Data = value; }
	}

	internal short State {
		get { return m_State; }
		set { m_State = value; }
	}

	[Description("Returns the parent symbol of the token.")]
	public Symbol Parent {
		get { return m_Parent; }
		internal set { m_Parent = value; }
	}

	[Description("Returns the symbol type associated with this token.")]
	public SymbolType Type()
	{
		return m_Parent.Type;
	}

	internal Group Group()
	{
		return m_Parent.Group;
	}
}

public class TokenList
{
		//Don't inherit - hide array modifying methods
	private ArrayList m_Array;

	internal TokenList()
	{
		m_Array = new ArrayList();
	}

	[Description("Returns the token with the specified index.")]
	public new Token this[int Index] {
		get { return (Token)this.m_Array[Index]; }

		internal set { m_Array[Index] = value; }
	}

	internal int Add(Token Item)
	{
		return m_Array.Add(Item);
	}

	[Description("Returns the total number of tokens in the list.")]
	public int Count()
	{
		return m_Array.Count;
	}

	internal void Clear()
	{
		m_Array.Clear();
	}
}

internal class TokenStack
{
	//================================================================================
	// Class Name:
	//      TokenStack    '
	// Instancing:
	//      Private; Internal  (VB Setting: 1 - Private)
	//
	// Purpose:
	//      This class is used by the GOLDParser class to store tokens during parsing.
	//      In particular, this class is used the the LALR(1) state machine.
	//
	// Author(s):
	//      Devin Cook
	//      GOLDParser@DevinCook.com
	//
	// Dependacies:
	//      Token Class
	//
	// Revision History
	//     12/11/2001
	//         Modified the stack to not deallocate the array until cleared
	//================================================================================


	private Stack m_Stack;
	public TokenStack()
	{
		m_Stack = new Stack();
	}

	internal int Count {
		get { return m_Stack.Count; }
	}

	public void Clear()
	{
		m_Stack.Clear();
	}

	public void Push(ref Token TheToken)
	{
		m_Stack.Push(TheToken);
	}

	public Token Pop()
	{
		return (Token)this.m_Stack.Pop();
	}

	public Token Top()
	{
		return (Token)this.m_Stack.Peek();
	}
}

internal class TokenQueueStack
{

	private ArrayList m_Items;
	public TokenQueueStack()
	{
		m_Items = new ArrayList();
	}

	internal int Count {
		get { return m_Items.Count; }
	}

	public void Clear()
	{
		m_Items.Clear();
	}

	public void Enqueue(ref Token TheToken)
	{
		m_Items.Add(TheToken);
		//End of list
	}

	public Token Dequeue()
	{
		Token Result = null;
		Result = (Token)this.m_Items[0];
		//Front of list
		m_Items.RemoveAt(0);

		return Result;
	}

	public Token Top()
	{
		if (m_Items.Count >= 1) {
			return (Token)this.m_Items[0];
		} else {
			return null;
		}
	}

	public void Push(Token TheToken)
	{
		m_Items.Insert(0, TheToken);
	}
	public Token Pop()
	{
		return Dequeue();
		//Same as dequeue
	}
}
